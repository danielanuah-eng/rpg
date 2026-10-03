// Edições do autor no site: limpeza do que vem do navegador e aplicação por cima do catálogo gerado.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import type { Catalogo } from '../../tcg/src/tipos.ts';
import { aplicarCatalogo, sanitizarEdicoes } from '../src/edicoes.ts';

const D = JSON.parse(readFileSync(new URL('../public/jogo/dados.json', import.meta.url), 'utf8'));
const cat = D.catalogo as Catalogo;

test('sanitizarEdicoes: só ids existentes, só campos conhecidos, obrigatórios não se apagam', () => {
  const ed = sanitizarEdicoes({
    cartas: {
      'base-aliado-c1-equilibrado': { ataque: 4, nome: 'Novo', hack: 1, custo: null, protegido: null },
      'nao-existe': { ataque: 9 },
    },
    conjuradores: { lyandra: { vida: 25, segredo: 'x' } },
    artes: { cartas: { 'base-aliado-c1-equilibrado': 'sal-do-viajante', 'nao-existe': 'x' }, conjuradores: { lyandra: '../fora' } },
  }, cat);
  assert.deepEqual(ed.cartas, { 'base-aliado-c1-equilibrado': { ataque: 4, nome: 'Novo', protegido: null } });
  assert.deepEqual(ed.conjuradores, { lyandra: { vida: 25 } });
  assert.deepEqual(ed.artes, { cartas: { 'base-aliado-c1-equilibrado': 'sal-do-viajante' }, conjuradores: {} });
});

test('aplicarCatalogo: muda a cópia, null apaga o campo, o original fica igual', () => {
  const id = Object.keys(cat.cartas).find((k) => cat.cartas[k].ativacao)!;
  const ed = sanitizarEdicoes({ cartas: { [id]: { ativacao: null, custo: 9 } }, conjuradores: { lyandra: { vida: 31 } } }, cat);
  const novo = aplicarCatalogo(cat, ed);
  assert.equal(novo.cartas[id].custo, 9);
  assert.equal(novo.cartas[id].ativacao, undefined);
  assert.equal(novo.conjuradores.lyandra.vida, 31);
  assert.ok(cat.cartas[id].ativacao);
  assert.notEqual(cat.conjuradores.lyandra.vida, 31);
});
