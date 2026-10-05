// Bug do autor (04/10): o Conjurador que ATACA (com arma) não recebe o dano de volta (sem revide), nem do Aliado/Mística/Estrutura
// atacado nem dos bloqueadores. Efeitos explícitos (espinhos) continuam valendo. O Aliado atacante continua recebendo o revide. Decisão 36.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import { itensBasicos } from '../src/itens.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const item = (id: string, categoriaItem: CartaDef['categoriaItem'], extra: Partial<CartaDef>): CartaDef => ({ ...base, id, nome: id, tipo: 'item', categoriaItem, ...extra });
const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'soldado', nome: 'Soldado', tipo: 'aliado', ataque: 3, vida: 5, palavrasChave: ['Proficiente'] },
  { ...base, id: 'torre', nome: 'Torre', tipo: 'estrutura', ataque: 2, vida: 5 },
  item('machado', 'canal', { tipoCanal: 'arma', ataque: 4 }),
  item('espinhosa', 'armadura', { defesa: 1, vidaDesgaste: 9, espinhos: 2 }),
  ...itensBasicos,
];
function criar(itens: [string[], string[]]): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = (it: string[]) => ({ conjurador: 'c', cartas: Array.from({ length: 30 }, () => 'nada'), itens: it });
  const e = criarJogo({ catalogo: cat, baralhos: [deck(itens[0]), deck(itens[1])], semente: 3, regras: { primeiroSemGrauNoTurno1: false, conjuradorSoAtacaComArma: true } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
let n = 0;
function poe(e: Estado, j: Jogador, defId: string): string {
  const id = `r${n++}`;
  e.instancias[id] = { id, defId, ehConjurador: false, dono: j, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.jogadores[j].campo.push(id);
  return id;
}
const passar = (e: Estado) => executar(e, { tipo: 'passar', jogador: e.pendencia ? e.pendencia.jogador : e.prioridade });
const resolver = (e: Estado) => { for (let i = 0; i < 8 && e.combate; i++) passar(e); };

test('Conjurador ataca um Aliado e uma Estrutura sem perder Vida', () => {
  const e = criar([['machado'], []]);
  const c0 = e.jogadores[0].conjuradorId;
  const s = poe(e, 1, 'soldado');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: c0, alvo: s });
  resolver(e);
  assert.equal(e.instancias[c0].dano, 0);
  assert.equal(e.instancias[s].dano, 4); // Soldado 3/5 leva 4 e não devolve os 3
});

test('Conjurador atacante bloqueado por vários Aliados não perde Vida', () => {
  const e = criar([['machado'], []]);
  const c0 = e.jogadores[0].conjuradorId, c1 = e.jogadores[1].conjuradorId;
  const a = poe(e, 1, 'soldado'), b = poe(e, 1, 'soldado');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: c0, alvo: c1 });
  passar(e); passar(e);
  assert.equal(e.pendencia?.tipo, 'bloqueio');
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueadores: [a, b] });
  passar(e); passar(e);
  assert.equal(e.pendencia?.tipo, 'distribuirDano');
  executar(e, { tipo: 'distribuirDano', jogador: 0, distribuicao: { [a]: 2, [b]: 2 } });
  assert.equal(e.instancias[c0].dano, 0);
  assert.equal(e.instancias[a].dano, 2);
  assert.equal(e.instancias[b].dano, 2);
  assert.match(e.log.join('\n'), /não recebe dano de volta/);
});

test('espinhos da Armadura ainda causam dano ao Conjurador atacante', () => {
  const e = criar([['machado'], ['espinhosa']]);
  const c0 = e.jogadores[0].conjuradorId, c1 = e.jogadores[1].conjuradorId;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: c0, alvo: c1 });
  passar(e); passar(e);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: c1 });
  resolver(e);
  assert.equal(e.instancias[c1].dano, 3); // 4 - Defesa 1
  assert.equal(e.instancias[c0].dano, 2); // espinhos (efeito), não revide
});

test('Aliado atacante continua recebendo o revide normal', () => {
  const e = criar([[], []]);
  const s0 = poe(e, 0, 'soldado');
  const t = poe(e, 1, 'torre');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: s0, alvo: t });
  resolver(e);
  assert.equal(e.instancias[t].dano, 3);
  assert.equal(e.instancias[s0].dano, 2);
});
