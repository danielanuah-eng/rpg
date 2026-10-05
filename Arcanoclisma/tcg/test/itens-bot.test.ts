// Equipamento do bot (pedido do autor, 04/10): sorteio pela semente entre os itens que servem ao Conjurador, e o bot usa o Extra.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar, itemServe } from '../src/motor.ts';
import { itensBasicos } from '../src/itens.ts';
import { decidir, sortearItensBot, ITEM_BASICO, PESO_ITEM_DO_VALOR, type Estilo } from '../sim/bot.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const item = (id: string, categoriaItem: CartaDef['categoriaItem'], extra: Partial<CartaDef>): CartaDef => ({ ...base, id, nome: id, tipo: 'item', categoriaItem, ...extra });
const conj = (id: string, valores: string[]): ConjuradorDef => ({ id, nome: id, valores, comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' });
const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  item('arma-ab', 'canal', { tipoCanal: 'arma', ataque: 2, valores: ['a', 'b', 'c'] }),
  item('arma-xy', 'canal', { tipoCanal: 'arma', ataque: 2, valores: ['x', 'y'] }),
  item('extra-ab', 'extra', { valores: ['a', 'b'] }),
  item('extra-brigao', 'extra', { competencia: 'Brigão' }),
  item('extra-outro', 'extra', { competencia: 'Invocador' }),
  ...itensBasicos,
];
const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj('c', ['a', 'b']), x: conj('x', ['x', 'z']) } };

test('sorteio do equipamento do bot: mesma semente dá o mesmo resultado e só sai item que serve', () => {
  for (let s = 0; s < 200; s++) {
    const a = sortearItensBot(cat, 'c', s);
    assert.deepEqual(a, sortearItensBot(cat, 'c', s));
    assert.equal(a.length, 3);
    assert.deepEqual(a.map((id) => cat.cartas[id].categoriaItem), ['canal', 'armadura', 'extra']);
    for (const id of a) assert.ok(itemServe(cat.conjuradores.c, cat.cartas[id]), `${id} não serve`);
  }
  const vistos = new Set(Array.from({ length: 200 }, (_, s) => sortearItensBot(cat, 'c', s)[0]));
  assert.ok(vistos.has('arma-ab') && vistos.size > 2, 'o sorteio varia entre as sementes');
  // o único Canal com Valor não serve ao Conjurador x (x+z): fica com um básico
  for (let s = 0; s < 50; s++) assert.ok(!sortearItensBot(cat, 'x', s).includes('arma-ab'));
  // Conjurador desconhecido: os básicos de cada espaço
  assert.deepEqual(sortearItensBot(cat, 'ninguem', 7), [ITEM_BASICO.canal, ITEM_BASICO.armadura, ITEM_BASICO.extra]);
});

test('sorteio do bot (decisão 38): item com Valor do Conjurador pesa 3, genérico pesa 1; o id do Conjurador entra na semente', () => {
  assert.equal(PESO_ITEM_DO_VALOR, 3);
  // Extra do Conjurador c: extra-ab (com Valor, peso 3) contra os genéricos que servem (peso 1 cada)
  const genericos = Object.values(cat.cartas).filter((d) => d.categoriaItem === 'extra' && !d.valores.length && itemServe(cat.conjuradores.c, d)).length;
  const N = 3000;
  let comValor = 0;
  for (let s = 0; s < N; s++) if (sortearItensBot(cat, 'c', s)[2] === 'extra-ab') comValor++;
  const esperado = 3 / (3 + genericos);
  assert.ok(Math.abs(comValor / N - esperado) < 0.05, `${comValor / N} longe de ${esperado}`);
  assert.ok(comValor / N > 1 / (1 + genericos), 'o item com Valor sai mais que no sorteio uniforme');
  // dois Conjuradores com o mesmo conjunto de itens e a mesma semente não tiram sempre o mesmo
  const c2: Catalogo = { ...cat, conjuradores: { ...cat.conjuradores, d: { ...cat.conjuradores.c, id: 'd', nome: 'd' } } };
  let iguais = 0;
  for (let s = 0; s < 200; s++) if (sortearItensBot(c2, 'c', s).join() === sortearItensBot(c2, 'd', s).join()) iguais++;
  assert.ok(iguais < 150, `iguais demais: ${iguais}`);
});

const est: Estilo = { nome: 't', agressividade: 0, reservaInstantes: false, grauAlvo: 0, queimaFace: false, conjAtaca: false };
function criar(itens: string[]): Estado {
  const deck = (it: string[]) => ({ conjurador: 'c', cartas: Array.from({ length: 30 }, () => 'nada'), itens: it });
  const e = criarJogo({ catalogo: cat, baralhos: [deck(itens), deck([])], semente: 3, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}

test('bot usa a Poção (Extra) só com a Vida baixa', () => {
  const e = criar(['item-extra-pocao']);
  e.jogadores[0].mao = [];
  assert.notEqual((decidir(e, 0, est) as { carta?: string }).carta, 'item-extra-pocao');
  const conjId = e.jogadores[0].conjuradorId;
  e.instancias[conjId].dano = 20;
  const pocao = e.jogadores[0].itens[0];
  const c = decidir(e, 0, est);
  assert.equal(c.tipo, 'ativar');
  assert.equal((c as { carta: string }).carta, pocao);
  executar(e, c);
  while (e.pilha.length) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.instancias[conjId].dano, 17);
  assert.ok(!e.jogadores[0].itens.includes(pocao), 'a Poção é consumida');
});
