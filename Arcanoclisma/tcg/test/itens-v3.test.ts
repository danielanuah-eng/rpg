// Itens v3 (pedido do autor, 04/10): armas com custo e Grau para atacar e efeitos ao atacar; armaduras com custo/Grau
// para proteger e efeitos ao proteger; itens com Valor só servem ao Conjurador que tem os dois Valores deles.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar, itemServe, validarBaralho } from '../src/motor.ts';
import { itensBasicos } from '../src/itens.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const item = (id: string, categoriaItem: CartaDef['categoriaItem'], extra: Partial<CartaDef>): CartaDef => ({ ...base, id, nome: id, tipo: 'item', categoriaItem, ...extra });
const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'soldado', nome: 'Soldado', tipo: 'aliado', ataque: 1, vida: 5 },
  item('harpa', 'canal', { tipoCanal: 'arma', ataque: 1, custo: 1, grau: 1, aoAtacar: { efeitos: [{ tipo: 'comprar', valor: 1 }] } }),
  item('badalo', 'canal', { tipoCanal: 'arma', ataque: 2, aoAtacar: { danoUnidades: 2 } }),
  item('coleira', 'canal', { tipoCanal: 'arma', ataque: 3, aoAtacar: { desgasteArmadura: 2 } }),
  item('capa', 'armadura', { defesa: 1, vidaDesgaste: 6, aoProteger: [{ tipo: 'arcano', valor: 1 }] }),
  item('vyal', 'armadura', { defesa: 3, vidaDesgaste: 9, custo: 1, grau: 1, espinhos: 2 }),
  item('do-par', 'extra', { valores: ['a', 'b', 'c'] }),
  item('so-de-um', 'extra', { valores: ['a'] }),
  ...itensBasicos,
];
function criar(itens: [string[], string[]]): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: ['a', 'b'], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = (it: string[]) => ({ conjurador: 'c', cartas: Array.from({ length: 30 }, () => 'nada'), itens: it });
  const e = criarJogo({ catalogo: cat, baralhos: [deck(itens[0]), deck(itens[1])], semente: 3, regras: { primeiroSemGrauNoTurno1: false, conjuradorSoAtacaComArma: true } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
const passar = (e: Estado) => executar(e, { tipo: 'passar', jogador: e.pendencia ? e.pendencia.jogador : e.prioridade });
const poeNoGrau = (e: Estado, j: Jogador) => { const z = e.jogadores[j]; z.grau.push(z.mao.shift()!); };
let n = 0;
function poe(e: Estado, j: Jogador, defId: string): string {
  const id = `t${n++}`;
  e.instancias[id] = { id, defId, ehConjurador: false, dono: j, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.jogadores[j].campo.push(id);
  return id;
}

test('armas básicas: Ataque 1 de graça; Ataque 2 com Grau 1 e custo 1; Ataque 3 com Grau 3 e custo 2', () => {
  const d = (id: string) => itensBasicos.find((x) => x.id === id)!;
  assert.deepEqual([d('item-canal-cajado').ataque, d('item-canal-cajado').grau, d('item-canal-cajado').custo], [1, 0, 0]);
  assert.deepEqual([d('item-canal-lamina').ataque, d('item-canal-lamina').grau, d('item-canal-lamina').custo], [2, 1, 1]);
  assert.deepEqual([d('item-canal-lanca').ataque, d('item-canal-lanca').grau, d('item-canal-lanca').custo], [3, 3, 2]);
});

test('arma com Grau e custo: sem Grau não ataca; com Grau paga o custo e aplica o efeito ao atacar', () => {
  const e = criar([['harpa'], []]);
  const z = e.jogadores[0];
  const conj = z.conjuradorId, alvo = e.jogadores[1].conjuradorId;
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 0, atacante: conj, alvo }), /Grau 1/);
  poeNoGrau(e, 0); poeNoGrau(e, 0);
  executar(e, { tipo: 'suspenderGrau', jogador: 0, carta: z.grau[0] }); // paga o custo 1 com o Grau
  const mao = z.mao.length;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: conj, alvo });
  assert.equal(z.mao.length, mao + 1); // comprou ao atacar
  assert.equal(z.arcano, 0); // o Arcano do Grau pagou o ataque
});

test('arma com dano a mais em unidades: +2 quando acerta um Aliado', () => {
  const e = criar([['badalo'], []]);
  const s = poe(e, 1, 'soldado');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: e.jogadores[0].conjuradorId, alvo: s });
  passar(e); passar(e); passar(e); passar(e);
  assert.equal(e.instancias[s].dano, 4); // Ataque 2 + 2 contra unidades
});

test('armadura com custo/Grau, espinhos e efeito ao proteger; arma que desgasta a mais', () => {
  // Vyal: precisa de Grau 1 e paga 1 para proteger; espinhos 2
  const e = criar([['coleira'], ['vyal']]);
  const c0 = e.jogadores[0].conjuradorId, c1 = e.jogadores[1].conjuradorId;
  poeNoGrau(e, 1);
  executar(e, { tipo: 'atacar', jogador: 0, atacante: c0, alvo: c1 });
  passar(e); passar(e);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: c1 });
  passar(e); passar(e);
  assert.equal(e.instancias[c1].dano, 0); // Defesa 3 segurou os 3
  assert.equal(e.instancias[c0].dano, 2); // espinhos
  const vyal = e.jogadores[1].itens[0];
  assert.equal(e.instancias[vyal].dano, 3 + 2); // desgaste normal + 2 da Coleira
  // Capa: ao proteger, ganha 1 de Arcano; sem Grau, a Vyal não protege
  const f = criar([['coleira'], ['capa']]);
  executar(f, { tipo: 'atacar', jogador: 0, atacante: f.jogadores[0].conjuradorId, alvo: f.jogadores[1].conjuradorId });
  passar(f); passar(f);
  executar(f, { tipo: 'bloquear', jogador: 1, bloqueador: f.jogadores[1].conjuradorId });
  passar(f); passar(f);
  assert.equal(f.jogadores[1].arcano, 1);
  const g = criar([['coleira'], ['vyal']]);
  executar(g, { tipo: 'atacar', jogador: 0, atacante: g.jogadores[0].conjuradorId, alvo: g.jogadores[1].conjuradorId });
  passar(g); passar(g);
  executar(g, { tipo: 'bloquear', jogador: 1, bloqueador: g.jogadores[1].conjuradorId });
  passar(g); passar(g);
  assert.equal(g.instancias[g.jogadores[1].conjuradorId].dano, 3); // sem Grau 1, a Vyal não protegeu
});

test('item com Valor só serve ao Conjurador que tem os dois Valores dele; sem Valor serve a todos', () => {
  const conj = { id: 'c', valores: ['a', 'b'], comprometimento: 'Brigão' } as ConjuradorDef;
  const d = (id: string) => cartas.find((x) => x.id === id)!;
  assert.equal(itemServe(conj, d('do-par')), true);
  assert.equal(itemServe(conj, d('so-de-um')), false);
  assert.equal(itemServe(conj, d('item-canal-cajado')), true);
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: { ...conj, nome: 'C', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' } } };
  const erros = validarBaralho(cat, { conjurador: 'c', cartas: Array.from({ length: 60 }, () => 'nada'), itens: ['so-de-um'] });
  assert.equal(erros.length, 1);
});
