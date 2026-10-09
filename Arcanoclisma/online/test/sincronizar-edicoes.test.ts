import { test } from 'node:test';
import assert from 'node:assert/strict';
import { sincronizarEdicoesDaSala } from '../public/js/sincronizar-edicoes.js';

test('a sala em espera não apaga as artes nem a raridade com versão zero', async () => {
  const edicoes = { id: 7, rascunho: false, dados: { raridades: { cartas: { c: 'rara' } } } };
  let chamadas = 0;
  assert.equal(await sincronizarEdicoesDaSala({ t: 'estado', status: 'aguardando', edVersao: 0 }, edicoes, async () => { chamadas++; return true; }), true);
  assert.equal(chamadas, 0);
  assert.equal(edicoes.dados.raridades.cartas.c, 'rara');
});

test('dois clientes com versões ou rascunhos distintos usam a edição fixada pela sala', async () => {
  for (const edicoes of [{ id: 4, rascunho: false, dados: {} }, { id: 7, rascunho: true, dados: {} }]) {
    const carregar = async (id: number) => { edicoes.id = id; edicoes.rascunho = false; edicoes.dados = { raridades: { cartas: { c: 'rara' } } }; return true; };
    assert.equal(await sincronizarEdicoesDaSala({ t: 'estado', status: 'jogando', edVersao: 7 }, edicoes, carregar), true);
    assert.equal(edicoes.id, 7);
    assert.equal(edicoes.rascunho, false);
    assert.deepEqual(edicoes.dados, { raridades: { cartas: { c: 'rara' } } });
  }
});

test('zero continua válido numa partida iniciada sem edições', async () => {
  const edicoes = { id: 7, rascunho: false };
  assert.equal(await sincronizarEdicoesDaSala({ t: 'estado', status: 'jogando', edVersao: 0 }, edicoes, async id => { edicoes.id = id; return true; }), true);
  assert.equal(edicoes.id, 0);
});

test('falha de carregamento ou versão incompatível não libera desenho desatualizado', async () => {
  const edicoes = { id: 4, rascunho: false };
  const mensagem = { t: 'estado', status: 'jogando', edVersao: 7 };
  assert.equal(await sincronizarEdicoesDaSala(mensagem, edicoes, async () => false), false);
  assert.equal(await sincronizarEdicoesDaSala(mensagem, edicoes, async () => true), false);
});
