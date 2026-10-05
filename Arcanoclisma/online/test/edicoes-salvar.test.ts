// Salvar no Editor sem apagar edições (bug de 05/10: v117 veio de um rascunho velho e apagou 37 cartas da v116).
// Mescla por diferença, conflitos por carta, 409 com base desatualizada e "Restaurar cartas perdidas".
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import type { Catalogo } from '../../tcg/src/tipos.ts';
import { aplicarDiferenca, cartasPerdidas, conflitosEdicoes, contarDiferenca, diferencaEdicoes, edicoesVazias, tirarDaDiferenca, type EdicoesSite } from '../src/edicoes.ts';
import { salvarDiferenca, esquecerEdicoes } from '../src/edicoes-db.ts';
import type { Env } from '../src/conta.ts';

const D = JSON.parse(readFileSync(new URL('../public/jogo/dados.json', import.meta.url), 'utf8'));
const cat = D.catalogo as Catalogo;
const [A, B, C] = Object.keys(cat.cartas);
const J = Object.keys(cat.conjuradores)[0];
const ed = (p: Partial<EdicoesSite>): EdicoesSite => ({ ...edicoesVazias(), ...p });

/** D1 de mentira: só os comandos que salvarDiferenca usa (tabela edicoes em memória). */
function bancoFalso(inicial: EdicoesSite[] = []) {
  const linhas = inicial.map((d, i) => ({ id: i + 1, dados: JSON.stringify(d), nota: '', usuario: 'autor', criado: i }));
  const DB = {
    prepare(sql: string) {
      let args: unknown[] = [];
      const st = {
        bind(...a: unknown[]) { args = a; return st; },
        async first() {
          if (/ORDER BY id DESC LIMIT 1/.test(sql)) return linhas.at(-1) ?? null;
          if (/WHERE id = \?/.test(sql)) return linhas.find((l) => l.id === args[0]) ?? null;
          throw new Error('sql inesperado: ' + sql);
        },
        async run() {
          if (!/^INSERT INTO edicoes/.test(sql)) throw new Error('sql inesperado: ' + sql);
          const max = linhas.at(-1)?.id ?? 0;
          if (/WHERE COALESCE/.test(sql) && max !== args[4]) return { meta: { changes: 0, last_row_id: 0 } };
          linhas.push({ id: max + 1, dados: args[0] as string, nota: args[1] as string, usuario: args[2] as string, criado: args[3] as number });
          return { meta: { changes: 1, last_row_id: max + 1 } };
        },
      };
      return st;
    },
  };
  return { env: { DB } as unknown as Env, linhas };
}

test('diferença: só o que mudou; null tira a carta; semRemocoes não apaga nada', () => {
  const base = ed({ cartas: { [A]: { custo: 1 }, [B]: { custo: 2 } }, artes: { cartas: { [A]: 'x' }, conjuradores: {} } });
  const novo = ed({ cartas: { [A]: { custo: 1 }, [C]: { custo: 3 } }, artes: { cartas: { [A]: 'y' }, conjuradores: {} } });
  const d = diferencaEdicoes(base, novo);
  assert.deepEqual(d.cartas, { [B]: null, [C]: { custo: 3 } });
  assert.deepEqual(d.artes.cartas, { [A]: 'y' });
  assert.equal(contarDiferenca(d), 3);
  assert.deepEqual(aplicarDiferenca(base, d), { ...novo, raridades: { cartas: {}, conjuradores: {} } });
  assert.deepEqual(diferencaEdicoes(base, novo, { semRemocoes: true }).cartas, { [C]: { custo: 3 } });
  // ordem das chaves não conta como mudança
  assert.equal(contarDiferenca(diferencaEdicoes(ed({ cartas: { [A]: { a: 1, b: 2 } } }), ed({ cartas: { [A]: { b: 2, a: 1 } } }))), 0);
});

test('mescla: o rascunho velho de outra aba não apaga as cartas salvas depois (o caso v116/v117)', () => {
  const v115 = ed({ cartas: { [A]: { custo: 1 } } });
  // aba 1 salvou 37 cartas (aqui: B e C) → v116
  const v116 = ed({ cartas: { [A]: { custo: 1 }, [B]: { custo: 2 }, [C]: { custo: 3 } } });
  // aba 2, apoiada na v115, mudou só o Conjurador
  const rascunho2 = ed({ cartas: { [A]: { custo: 1 } }, conjuradores: { [J]: { vida: 30 } } });
  const dif = diferencaEdicoes(v115, rascunho2);
  assert.deepEqual(conflitosEdicoes(v115, v116, dif), []);
  const v117 = aplicarDiferenca(v116, dif);
  assert.deepEqual(Object.keys(v117.cartas).sort(), [A, B, C].sort());
  assert.deepEqual(v117.conjuradores, { [J]: { vida: 30 } });
});

test('conflito: a mesma carta mudou nos dois lados; manter a do site tira a carta da diferença', () => {
  const base = ed({ cartas: { [A]: { custo: 1 } } });
  const atual = ed({ cartas: { [A]: { custo: 5 } } });
  const dif = diferencaEdicoes(base, ed({ cartas: { [A]: { custo: 2 } }, raridades: { cartas: { [B]: 'rara' }, conjuradores: {} } }));
  const c = conflitosEdicoes(base, atual, dif);
  assert.deepEqual(c, [{ tipo: 'carta', id: A, partes: ['campos'] }]);
  // mesmo resultado nos dois lados não é conflito
  assert.deepEqual(conflitosEdicoes(base, atual, diferencaEdicoes(base, atual)), []);
  tirarDaDiferenca(dif, 'carta', A);
  const r = aplicarDiferenca(atual, dif);
  assert.deepEqual(r.cartas[A], { custo: 5 });
  assert.equal(r.raridades.cartas[B], 'rara');
});

test('restaurar cartas perdidas: só o que existia na antiga e falta na atual; nada é apagado', () => {
  const antiga = ed({ cartas: { [A]: { custo: 1 }, [B]: { custo: 2 } }, artes: { cartas: { [C]: 'arte-c' }, conjuradores: {} } });
  const atual = ed({ cartas: { [A]: { custo: 9 } }, conjuradores: { [J]: { vida: 30 } } });
  const p = cartasPerdidas(antiga, atual);
  assert.deepEqual(p.cartas, { [B]: { custo: 2 } });
  assert.deepEqual(p.artes.cartas, { [C]: 'arte-c' });
  const r = aplicarDiferenca(atual, p);
  assert.deepEqual(r.cartas[A], { custo: 9 });
  assert.deepEqual(r.conjuradores, { [J]: { vida: 30 } });
});

test('Carregar JSON sem base (restauracao-edicoes.json) só acrescenta: nada da versão em vigor some', () => {
  const arq = JSON.parse(readFileSync(new URL('../restauracao-edicoes.json', import.meta.url), 'utf8'));
  const atual = ed({ cartas: { [A]: { custo: 7 } }, conjuradores: { [J]: { vida: 33 } } });
  const dif = diferencaEdicoes(atual, { ...edicoesVazias(), ...arq }, { semRemocoes: true });
  const r = aplicarDiferenca(atual, dif);
  assert.deepEqual(r.conjuradores[J], { vida: 33 });
  for (const id of Object.keys(arq.cartas)) assert.deepEqual(r.cartas[id], arq.cartas[id]);
  assert.ok(contarDiferenca(dif) >= Object.keys(arq.cartas).length - 1);
});

test('servidor: base certa grava a diferença sobre a versão em vigor', async () => {
  esquecerEdicoes();
  const { env, linhas } = bancoFalso([ed({ cartas: { [A]: { custo: 1 } } })]);
  const r = await salvarDiferenca(env, cat, { base: 1, diff: { cartas: { [B]: { custo: 2 } } }, nota: 'teste' }, 'autor');
  assert.ok(r.ok);
  assert.equal(r.id, 2);
  assert.deepEqual(JSON.parse(linhas[1].dados).cartas, { [A]: { custo: 1 }, [B]: { custo: 2 } });
});

test('servidor: base desatualizada → 409 com a versão atual e os conflitos; nada é gravado', async () => {
  esquecerEdicoes();
  const { env, linhas } = bancoFalso([ed({ cartas: { [A]: { custo: 1 } } }), ed({ cartas: { [A]: { custo: 5 } } })]);
  const r = await salvarDiferenca(env, cat, { base: 1, diff: { cartas: { [A]: { custo: 2 }, [B]: { custo: 3 } } } }, 'autor');
  assert.equal(r.ok, false);
  if (r.ok) return;
  assert.equal(r.status, 409);
  assert.equal(r.corpo.atual?.id, 2);
  assert.deepEqual(r.corpo.conflitos, [{ tipo: 'carta', id: A, partes: ['campos'] }]);
  assert.equal(linhas.length, 2);
});

test('servidor: dois envios juntos com a mesma base → só o primeiro grava', async () => {
  esquecerEdicoes();
  const { env, linhas } = bancoFalso([ed({})]);
  const [r1, r2] = await Promise.all([
    salvarDiferenca(env, cat, { base: 1, diff: { cartas: { [A]: { custo: 2 } } } }, 'autor'),
    salvarDiferenca(env, cat, { base: 1, diff: { cartas: { [A]: { custo: 2 } } } }, 'autor'),
  ]);
  assert.deepEqual([r1.ok, r2.ok].sort(), [false, true]);
  assert.equal(linhas.length, 2);
});

test('servidor: Editor antigo (rascunho inteiro em `dados`, sem base) é recusado', async () => {
  esquecerEdicoes();
  const { env, linhas } = bancoFalso([ed({ cartas: { [A]: { custo: 1 } } })]);
  const r = await salvarDiferenca(env, cat, { dados: ed({}) } as never, 'autor');
  assert.equal(r.ok, false);
  if (!r.ok) assert.equal(r.status, 409);
  assert.equal(linhas.length, 1);
});
