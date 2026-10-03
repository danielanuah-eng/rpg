// Mecânicas da rodada 6 (variante BAL): gatilho ao causar dano de combate ao Conjurador, Protegido N, destruir todos os Aliados, curva de Grau.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import { bonusGrau, avaliar, PRECO } from '../src/balanco.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador } from '../src/tipos.ts';
import { rodada3 } from '../sim/lista.ts';
import { arquetiposDe, catalogoDe } from '../sim/variantes.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0, habilidade: '' };
const cartas: CartaDef[] = [
  { ...base, id: 'espiao', nome: 'Espião', tipo: 'aliado', ataque: 2, vida: 3, aoDanoConjurador: [{ tipo: 'comprar', valor: 1 }] },
  { ...base, id: 'guarda', nome: 'Guarda', tipo: 'aliado', ataque: 1, vida: 5 },
  { ...base, id: 'blindado', nome: 'Blindado', tipo: 'aliado', ataque: 3, vida: 3, protegido: 2 },
  { ...base, id: 'raio', nome: 'Raio', tipo: 'temporaria', custo: 1, efeitos: [{ tipo: 'dano', valor: 3, alvo: 'escolhidoAliado' }] },
  { ...base, id: 'ira', nome: 'Ira', tipo: 'temporaria', efeitos: [{ tipo: 'destruirTodosAliados', ambos: true }] },
  { ...base, id: 'sol', nome: 'Sol', tipo: 'temporaria', efeitos: [{ tipo: 'destruirTodosAliados', ambos: false }] },
];
const catalogo: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };

function novo(): Estado {
  const ids = cartas.map((c) => c.id);
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, (_, i) => ids[i % ids.length]) };
  const e = criarJogo({ catalogo, baralhos: [deck, deck], semente: 3 });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') { executar(e, { tipo: 'passar', jogador: e.prioridade }); executar(e, { tipo: 'passar', jogador: e.prioridade }); }
  return e;
}
const passarTodos = (e: Estado) => { executar(e, { tipo: 'passar', jogador: e.prioridade }); executar(e, { tipo: 'passar', jogador: e.prioridade }); };
function daMao(e: Estado, j: Jogador, defId: string): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId)!;
  const ib = z.baralho.indexOf(id);
  if (ib >= 0) { z.baralho.splice(ib, 1); z.mao.push(id); }
  return id;
}
function emCampo(e: Estado, j: Jogador, defId: string): string {
  const id = daMao(e, j, defId);
  const z = e.jogadores[j];
  z.mao.splice(z.mao.indexOf(id), 1);
  z.campo.push(id);
  return id;
}

test('gatilho ao causar dano de combate ao Conjurador: compra 1 carta quando o ataque passa', () => {
  const e = novo();
  const a = emCampo(e, 0, 'espiao');
  const mao = e.jogadores[0].mao.length;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: a, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e); // resolve o ataque: pede bloqueio
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: null });
  passarTodos(e); // dano de combate: o gatilho vai para a pilha
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 2);
  assert.equal(e.pilha.length, 1);
  passarTodos(e); // resolve o gatilho
  assert.equal(e.jogadores[0].mao.length, mao + 1);
});

test('gatilho de dano ao Conjurador não dispara quando o ataque é bloqueado', () => {
  const e = novo();
  const a = emCampo(e, 0, 'espiao');
  const g = emCampo(e, 1, 'guarda');
  const mao = e.jogadores[0].mao.length;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: a, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: g });
  passarTodos(e);
  assert.equal(e.pilha.length, 0);
  assert.equal(e.jogadores[0].mao.length, mao);
});

test('Protegido N: mirar a carta do oponente custa N a mais; a sua própria não', () => {
  const e = novo();
  const b = emCampo(e, 1, 'blindado');
  const raio = daMao(e, 0, 'raio');
  e.jogadores[0].arcano = 2; // Raio custa 1 + Protegido 2 = 3
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: raio, alvo: b }), /insuficiente/);
  e.jogadores[0].arcano = 3;
  executar(e, { tipo: 'jogar', jogador: 0, carta: raio, alvo: b });
  assert.equal(e.jogadores[0].arcano, 0);
  passarTodos(e);
  assert.ok(e.jogadores[1].cemiterio.includes(b));
  const meu = emCampo(e, 0, 'blindado');
  const raio2 = daMao(e, 0, 'raio');
  e.jogadores[0].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 0, carta: raio2, alvo: meu });
  assert.equal(e.jogadores[0].arcano, 0);
});

test('destruir todos os Aliados: "ambos" leva os seus; sem "ambos", só os do oponente', () => {
  const e = novo();
  const m1 = emCampo(e, 0, 'guarda');
  const o1 = emCampo(e, 1, 'guarda');
  const o2 = emCampo(e, 1, 'blindado');
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'sol') });
  passarTodos(e);
  assert.ok(e.jogadores[1].cemiterio.includes(o1) && e.jogadores[1].cemiterio.includes(o2));
  assert.ok(e.jogadores[0].campo.includes(m1));
  emCampo(e, 1, 'espiao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'ira') });
  passarTodos(e);
  assert.equal(e.jogadores[0].campo.length, 0);
  assert.equal(e.jogadores[1].campo.length, 0);
});

test('curva de Grau: bônus cresce com o quadrado do Grau e para no Grau 8', () => {
  assert.deepEqual([0, 1, 2, 3, 4, 5, 6, 7, 8, 10].map((g) => bonusGrau(g, 0.5)), [0, 0, 0.5, 1.5, 3, 5, 7.5, 10.5, 14, 14]);
  assert.equal(bonusGrau(7, 0), 0);
});

test('variante BAL: baralhos com 60 cartas, cartas novas dentro do orçamento com a curva', () => {
  const v = rodada3.find((x) => x.id === 'BAL')!;
  const cat = catalogoDe(v);
  const arqs = arquetiposDe(v, cat);
  for (const a of arqs) assert.equal(a.lista.reduce((s, [, n]) => s + n, 0), 60, a.nome);
  for (const c of Object.values(cat.cartas).filter((x) => x.id.startsWith('r6-'))) {
    const av = avaliar(c, { ...PRECO, curar: 0.4 });
    const orc = av.orcamento + (c.tipo === 'aliado' ? bonusGrau(c.grau, v.r6CurvaGrau) : 0);
    assert.ok(av.pontos <= orc + 1e-9, `${c.id}: ${av.pontos} > ${orc}`);
    assert.ok(orc - av.pontos <= 2.5, `${c.id} fraca demais: ${av.pontos} de ${orc}`);
    for (const n of [c.custo, c.grau, c.ataque ?? 0, c.vida ?? 0]) assert.ok(n >= 0 && n <= 20, `${c.id}: ${n}`);
  }
  // o PADRAO-R5 (antes da rodada 6) não tem cartas da rodada 6
  const antigo = catalogoDe(rodada3.find((x) => x.id === 'PADRAO-R5')!);
  assert.equal(Object.keys(antigo.cartas).some((id) => id.startsWith('r6-')), false);
});
