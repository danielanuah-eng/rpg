// Construtor de efeitos do Editor (lógica, sem navegador): as tabelas cobrem tudo o que os 9 Conjuradores têm e fazem,
// e o poder de cada um volta igual depois de passar pelo construtor (módulo gerado public/jogo/poder-conjurador.js).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
// @ts-ignore: módulos JS do site
import { CONDICOES, CONDICOES_CONJURADOR, EFEITOS, PASSIVAS_CONJ, chaveDe, novoEfeito, efeitosDisponiveis, avisoEfeito, ehSecundario } from '../public/js/construtor-tabelas.js';
// @ts-ignore
import { poderParaConstrutor, construtorParaPoder, PASSIVAS_CONJURADOR } from '../public/jogo/poder-conjurador.js';
// @ts-ignore
import { descEfeito, textoPoder } from '../public/jogo/textos.js';

const D = JSON.parse(readFileSync(new URL('../public/jogo/dados.json', import.meta.url), 'utf8'));
const conjs = Object.values(D.catalogo.conjuradores) as Array<{ id: string; poder: Record<string, unknown>; habilidade: string }>;
/** Campos que o formulário de um efeito mostra: os fixos da linha, os campos dela e o requisito/escolha (painel e campo). */
const camposDaLinha = (k: string) => new Set([...Object.keys(EFEITOS[k].fixo), ...EFEITOS[k].campos.map((c: { k: string }) => c.k), 'requisito', 'escolha']);

test('os 9 Conjuradores do site: ida e volta pelo construtor dá o mesmo JSON', () => {
  assert.equal(conjs.length, 9);
  for (const c of conjs) {
    const v = JSON.parse(JSON.stringify(poderParaConstrutor(c.poder)));
    assert.equal(JSON.stringify(construtorParaPoder(v)), JSON.stringify(c.poder), c.id);
    assert.equal(textoPoder(construtorParaPoder(v)), textoPoder(c.poder), c.id);
  }
});

test('tudo o que os 9 Conjuradores têm abre no construtor: condição, custo, efeitos (todos os campos) e passivas', () => {
  const condicoes = new Set(CONDICOES_CONJURADOR.map(([k]: [string]) => k));
  const passivas = new Set(PASSIVAS_CONJ.map(([k]: [string]) => k));
  assert.deepEqual([...passivas], [...PASSIVAS_CONJURADOR]);
  for (const c of conjs) {
    const v = poderParaConstrutor(c.poder);
    for (const k of Object.keys(c.poder)) if (!['ativacao', 'gatilho', 'habilidades', 'permiteJogarDe'].includes(k)) assert.ok(passivas.has(k), `${c.id}: passiva ${k}`);
    assert.equal(v._resto, undefined, `${c.id}: nada fica fora do construtor`);
    for (const h of v.habilidades) {
      assert.ok(condicoes.has(h.quando), `${c.id}: condição ${h.quando}`);
      for (const ef of h.efeitos) {
        const k = chaveDe(ef);
        assert.ok(k, `${c.id}: efeito ${ef.tipo} fora da tabela`);
        for (const campo of Object.keys(ef)) assert.ok(camposDaLinha(k).has(campo), `${c.id}: campo ${campo} de ${ef.tipo} sem lugar no formulário`);
        assert.equal(avisoEfeito(k, h.quando), '', `${c.id}: ${ef.tipo} funciona em ${h.quando}`);
      }
    }
  }
});

test('tabela de efeitos: cada linha cria um efeito que volta para a mesma linha e tem texto', () => {
  for (const k of Object.keys(EFEITOS)) {
    const ef = novoEfeito(k);
    assert.equal(chaveDe(ef), k, k);
    assert.notEqual(descEfeito(ef), ef.tipo, `${k}: sem texto`);
  }
  // efeitos com alvo escolhido ao ativar só são oferecidos onde funcionam; os do formato antigo só aparecem quando já estão na carta
  const chaves = (q: string, atual?: string) => efeitosDisponiveis(q, atual).map(([k]: [string]) => k);
  // reanimar (04/10, noite): o Aliado é escolhido ao resolver, então funciona em qualquer condição (gatilhos inclusive)
  assert.ok(chaves('ativada').includes('reanimar'));
  assert.ok(chaves('aliadoEntra').includes('reanimar'));
  assert.equal(avisoEfeito('reanimar', 'aliadoEntra'), '');
  assert.ok(!chaves('aliadoEntra').includes('banirDaMao'));
  assert.notEqual(avisoEfeito('banirDaMao', 'aliadoEntra'), '');
  assert.ok(!chaves('ativada').includes('devolverAlvo'));
  assert.ok(chaves('ativada', 'devolverAlvo').includes('devolverAlvo'));
});

test('efeitos das habilidades das cartas do site também abrem na tabela (o formato antigo não vira efeito de campo)', () => {
  let n = 0;
  for (const c of Object.values(D.catalogo.cartas) as Array<{ id: string; habilidades?: Array<{ efeitos: Array<{ tipo: string }> }> }>) {
    for (const h of c.habilidades ?? []) for (const ef of h.efeitos) { n++; assert.ok(chaveDe(ef), `${c.id}: ${ef.tipo}`); }
  }
  assert.equal(chaveDe({ tipo: 'devolver', alvo: 'escolhidoProprio' }), 'devolverAlvo');
  assert.equal(chaveDe({ tipo: 'campo', acao: 'devolver', alcance: 'alvo', lado: 'proprio', tipos: ['aliado'] }), 'devolver');
  assert.equal(chaveDe({ tipo: 'dano', valor: 2, alvo: 'escolhido' }), 'danoAlvo');
  void n;
});

test('04/10 (noite): efeitos secundários num grupo próprio; formato antigo abre na linha certa; gatilho do Cataclisma do oponente', () => {
  // secundários: dano no Conjurador, tirar da própria mão, sacrificar, Grau ao cemitério
  for (const k of ['secDano', 'secDescartar', 'secSacrificar', 'secGrau']) {
    assert.ok(ehSecundario(k), k);
    assert.equal(novoEfeito(k).secundario, true, k);
    assert.ok(efeitosDisponiveis('jogar').some(([x]: [string]) => x === k), `${k} em cartas`);
    assert.ok(efeitosDisponiveis('ativada').some(([x]: [string]) => x === k), `${k} em Conjuradores`);
  }
  assert.equal(chaveDe({ tipo: 'perderVida', valor: 1 }), 'perderVida');
  assert.equal(chaveDe({ tipo: 'perderVida', valor: 1, secundario: true }), 'secDano');
  assert.equal(chaveDe({ tipo: 'descartar', quem: 'oponente', valor: 1, aleatorio: true }), 'descartar');
  assert.equal(chaveDe({ tipo: 'descartar', quem: 'voce', valor: 1, secundario: true }), 'secDescartar');
  assert.equal(chaveDe({ tipo: 'sacrificar', valor: 1 }), 'secSacrificar');
  // "tirar da mão": o formato antigo `aleatorio: true` abre como "ao acaso"
  const modo = EFEITOS.descartar.campos.find((c: { k: string }) => c.k === 'modo');
  assert.equal(modo.ler({ tipo: 'descartar', quem: 'oponente', valor: 1, aleatorio: true }), 'acaso');
  // reanimar antigo (custo + 2, sem `pagamento`) abre como "custo + X"
  const pg = EFEITOS.reanimar.campos.find((c: { k: string }) => c.k === 'pagamento');
  assert.equal(pg.ler({ tipo: 'reanimar', custoExtra: 2 }), 'custoMais');
  assert.equal(pg.ler({ tipo: 'reanimar', custoExtra: 0 }), 'custo');
  assert.ok(CONDICOES.some(([k]: [string]) => k === 'cataclismaOponente'));
  assert.ok(CONDICOES_CONJURADOR.some(([k]: [string]) => k === 'cataclismaOponente'));
});

test('construtor ampliado (05/10, decisão 40): eventos de jogador e "enquanto estiver em campo" nas duas tabelas de condições', async () => {
  // @ts-ignore: módulo JS do site
  const t = await import('../public/js/construtor-tabelas.js');
  for (const lista of [CONDICOES, CONDICOES_CONJURADOR]) {
    const ks = lista.map(([k]: [string]) => k);
    for (const q of [...t.QUANDO_EVENTO, 'emCampo']) assert.ok(ks.includes(q), q);
  }
  assert.deepEqual(t.QUEM_EVENTO.map(([k]: [string]) => k), ['voce', 'oponente', 'qualquer']);
  assert.deepEqual(t.SENAO_EFEITO.map(([k]: [string]) => k), ['linha', 'carta', 'alternativo']);
  assert.deepEqual(t.SENAO_MANUT.map(([k]: [string]) => k), ['sai', 'pula', 'alternativo']);
});
