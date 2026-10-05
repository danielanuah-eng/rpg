// Regra das cores (pedido do autor, 04/10; decisão 37): carta de 2 Valores só com exatamente os 2 do Conjurador;
// de 3 ou mais, só se o par do Conjurador estiver entre os Valores da carta. 1 Valor e sem Valor: como antes.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { cartaServeConjurador, corrigirCores, validarBaralho, valoresServem } from '../src/motor.ts';
import type { CartaDef, Catalogo, ConjuradorDef } from '../src/tipos.ts';

const base = { subtipos: [], grau: 0, custo: 1, velocidade: 'findante', arcanoGerado: 0, cenario: '', texto: '' } as const;
const c = (id: string, valores: string[], extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'aliado', ataque: 1, vida: 1, valores, ...extra });
const conj: ConjuradorDef = { id: 'ab', nome: 'AB', valores: ['a', 'b'], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' };
const cartas = [c('sem', []), c('a', ['a']), c('c', ['c']), c('ab', ['b', 'a']), c('ac', ['a', 'c']), c('abc', ['a', 'b', 'c']), c('acd', ['a', 'c', 'd']),
  c('a-temp', ['a'], { tipo: 'temporaria', ataque: undefined, vida: undefined }), c('ac-temp', ['a', 'c'], { tipo: 'temporaria', ataque: undefined, vida: undefined })];
const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((x) => [x.id, x])), conjuradores: { ab: conj } };
const d = (id: string) => cat.cartas[id];

test('1 Valor: um dos 2 do Conjurador; sem Valor: todos', () => {
  assert.equal(cartaServeConjurador(d('sem'), conj), true);
  assert.equal(cartaServeConjurador(d('a'), conj), true);
  assert.equal(cartaServeConjurador(d('c'), conj), false);
});

test('2 Valores: só com exatamente os 2 do Conjurador (a ordem não importa)', () => {
  assert.equal(cartaServeConjurador(d('ab'), conj), true);
  assert.equal(cartaServeConjurador(d('ac'), conj), false);
});

test('3 ou mais Valores: o par do Conjurador tem de estar entre os da carta', () => {
  assert.equal(cartaServeConjurador(d('abc'), conj), true);
  assert.equal(cartaServeConjurador(d('acd'), conj), false);
  assert.equal(valoresServem(['a', 'b', 'c', 'd', 'e'], ['e', 'b']), true);
  assert.equal(valoresServem(['a', 'b', 'c', 'd', 'e'], ['e', 'f']), false);
});

test('validarBaralho recusa 2 Valores com só 1 em comum e explica; sem exceção', () => {
  const b = { conjurador: 'ab', cartas: [...Array(57).fill('a'), 'ac', 'acd', 'abc'] };
  const erros = validarBaralho(cat, b);
  assert.equal(erros.length, 2, erros.join(' | '));
  assert.match(erros[0], /exatamente esses 2/);
});

test('corrigirCores troca cada cópia que saiu da regra por carta do mesmo tipo que serve, até 3 cópias', () => {
  const r = corrigirCores(cat, 'ab', [['a', 3], ['ac', 3], ['ac-temp', 2], ['abc', 1]], { max: 3 });
  assert.equal(r.trocas.length, 5);
  assert.ok(r.lista.every(([id, n]) => cartaServeConjurador(d(id), conj) && n <= 3), JSON.stringify(r.lista));
  assert.equal(r.lista.reduce((s, [, n]) => s + n, 0), 9);
  for (const t of r.trocas.filter((x) => x.de === 'ac-temp')) assert.equal(d(t.para).tipo, 'temporaria');
});
