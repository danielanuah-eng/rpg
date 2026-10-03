// Edições do autor (editor do app): aplicação por cima do jogo gerado.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aplicarBaralhos, aplicarCatalogo, edicoesVazias, lerEdicoes } from '../sim/edicoes.ts';
import { rodada3 } from '../sim/lista.ts';
import { contextoDe } from '../sim/nucleo.ts';

const ctx = contextoDe(rodada3.find((x) => x.id === 'CONJ')!);

test('nos testes automáticos as edições do arquivo não entram', () => {
  assert.deepEqual(lerEdicoes(), edicoesVazias());
});

test('carta editada: campos mudam, null apaga, o resto fica', () => {
  const id = 'r6-colosso-c7';
  const base = ctx.catalogo.cartas[id];
  assert.ok(base.protegido);
  const ed = { ...edicoesVazias(), cartas: { [id]: { custo: base.custo - 1, protegido: null } } };
  const c = aplicarCatalogo(ctx.catalogo, ed).cartas[id];
  assert.equal(c.custo, base.custo - 1);
  assert.equal(c.protegido, undefined);
  assert.equal(c.nome, base.nome);
  assert.equal(ctx.catalogo.cartas[id].custo, base.custo, 'o catálogo original não muda');
});

test('carta nova entra no catálogo; edição de carta que não existe é ignorada', () => {
  const nova = { nova: true, nome: 'Teste', tipo: 'aliado' as const, subtipos: [], valores: [], custo: 1, grau: 0, velocidade: 'findante' as const, arcanoGerado: 1, ataque: 1, vida: 1, cenario: '', texto: '' };
  const cat = aplicarCatalogo(ctx.catalogo, { ...edicoesVazias(), cartas: { 'autor-teste': nova, 'sumiu': { custo: 3 } } });
  assert.equal(cat.cartas['autor-teste'].nome, 'Teste');
  assert.equal(cat.cartas['autor-teste'].id, 'autor-teste');
  assert.equal(cat.cartas['sumiu'], undefined);
});

test('Conjurador editado e baralho editado', () => {
  const cat = aplicarCatalogo(ctx.catalogo, { ...edicoesVazias(), conjuradores: { lyandra: { vida: 25, poder: { aliadosProtegidos: 2 } } } });
  assert.equal(cat.conjuradores.lyandra.vida, 25);
  assert.deepEqual(cat.conjuradores.lyandra.poder, { aliadosProtegidos: 2 });
  const lista: Array<[string, number]> = [['base-recurso', 60]];
  const arqs = aplicarBaralhos(ctx.arqs, { ...edicoesVazias(), baralhos: { carmal: { lista, estilo: { agressividade: 0.9 } } } });
  const carmal = arqs.find((a) => a.conjurador === 'carmal')!;
  assert.deepEqual(carmal.lista, lista);
  assert.equal(carmal.estilo.agressividade, 0.9);
  assert.equal(carmal.estilo.grauAlvo, ctx.arqs.find((a) => a.conjurador === 'carmal')!.estilo.grauAlvo);
});
