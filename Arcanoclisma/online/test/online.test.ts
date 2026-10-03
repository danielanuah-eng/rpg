// Testes do site online: a coleção nova, a identidade visual e a visão de cada jogador (sigilo das partidas online).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { criarJogo, executar, ErroRegra } from '../../tcg/src/motor.ts';
import { decidir } from '../../tcg/sim/bot.ts';
import { avaliar } from '../../tcg/src/balanco.ts';
import { cartasColecao } from '../../tcg/src/cartas-colecao.ts';
import type { Catalogo, Comando, Jogador } from '../../tcg/src/tipos.ts';
import { visaoDe } from '../src/visao.ts';

const D = JSON.parse(readFileSync(new URL('../public/jogo/dados.json', import.meta.url), 'utf8'));
const artes = new Set((JSON.parse(readFileSync(new URL('../../cartas/catalogo-artes.json', import.meta.url), 'utf8')).assets as { id: string }[]).map((a) => a.id));
const lista = (b: { lista: [string, number][] }) => b.lista.flatMap(([id, n]) => Array(n).fill(id));

test('coleção nova: ids únicos, artes aprovadas e números dentro da régua', () => {
  const ids = new Set<string>();
  for (const c of cartasColecao) {
    assert.ok(!ids.has(c.id), `id repetido ${c.id}`);
    ids.add(c.id);
    assert.ok(artes.has(c.arte), `${c.id}: arte ${c.arte} fora do catálogo aprovado`);
    if (c.tipo === 'item') { assert.ok(['canal', 'armadura', 'extra'].includes(c.categoriaItem!)); continue; }
    const a = avaliar(c);
    // Sino dos Juramentados passa da régua de propósito: o oponente pode pagar para anulá-lo (a régua não precifica isso)
    if (a.folga !== null && !Number.isNaN(a.folga) && c.id !== 'col-sino-dos-juramentados') assert.ok(a.folga >= -1.5, `${c.id} acima do orçamento (${a.folga})`);
  }
});

test('identidade: toda carta tem arte, menos os 7 itens originais (pedido ao GPT)', () => {
  const sem = Object.entries(D.visual as Record<string, { arte: string | null }>).filter(([, v]) => !v.arte).map(([id]) => id).sort();
  assert.deepEqual(sem, ['item-armadura-couraca', 'item-armadura-gibao', 'item-canal-cajado', 'item-canal-lamina', 'item-canal-lanca', 'item-extra-elixir', 'item-extra-pocao']);
  const usadas = Object.values(D.visual as Record<string, { arte: string | null }>).map((v) => v.arte).filter(Boolean);
  assert.equal(new Set(usadas).size, usadas.length, 'arte usada em duas cartas');
  for (const a of Object.values(D.conjuradoresArte as Record<string, string>)) assert.ok(!usadas.includes(a), `arte de Conjurador ${a} reaproveitada`);
});

test('cartas novas jogam no motor sem erro (partidas bot × bot com a coleção)', () => {
  const novas = cartasColecao.filter((c) => c.tipo !== 'item').map((c) => c.id);
  const cat = D.catalogo as Catalogo;
  for (let s = 1; s <= 6; s++) {
    const base = D.baralhos[s % D.baralhos.length];
    const cartas = [...novas.filter((id) => cat.cartas[id].valores.length === 0 || cat.cartas[id].valores.some((v: string) => cat.conjuradores[base.conjurador].valores.includes(v))), ...lista(base)].slice(0, 60);
    const e = criarJogo({ catalogo: cat, regras: D.regras, semente: s, baralhos: [{ conjurador: base.conjurador, cartas, itens: ['item-canal-badalo', 'item-armadura-capa', 'item-extra-sal'] }, { conjurador: D.baralhos[(s + 3) % 9].conjurador, cartas: lista(D.baralhos[(s + 3) % 9]), itens: D.baralhos[(s + 3) % 9].itens }] });
    for (let i = 0; i < 6000 && e.vencedor === null; i++) {
      const j = (e.pendencia ? e.pendencia.jogador : e.prioridade) as Jogador;
      const cmd = decidir(e, j, D.baralhos[j === 0 ? s % 9 : (s + 3) % 9].estilo);
      try { executar(e, cmd); } catch (err) { if (!(err instanceof ErroRegra)) throw err; executar(e, seguro(e, j)); }
    }
    assert.notEqual(e.vencedor, null, `partida ${s} não terminou`);
  }
});

function seguro(e: ReturnType<typeof criarJogo>, j: Jogador): Comando {
  const p = e.pendencia;
  if (p?.tipo === 'bloqueio') return { tipo: 'bloquear', jogador: j, bloqueador: null };
  if (p?.tipo === 'pagarAnulacao') return { tipo: 'pagarAnulacao', jogador: j, pagar: false };
  if (p?.tipo === 'escolherDoTopo') return { tipo: 'escolherDoTopo', jogador: j, cartas: p.cartas.slice(0, p.pegar) };
  if (p?.tipo === 'descartar') return { tipo: 'descartar', jogador: j, cartas: e.jogadores[j].mao.slice(-p.quantidade) };
  if (p?.tipo === 'escolherInicio') return { tipo: 'escolherInicio', jogador: j, primeiro: j };
  return { tipo: 'passar', jogador: j };
}

test('visão de um jogador: esconde mão do oponente, baralhos, Grau virado e a semente', () => {
  const cat = D.catalogo as Catalogo;
  const [a, b] = [D.baralhos[0], D.baralhos[8]];
  const e = criarJogo({ catalogo: cat, regras: D.regras, semente: 42, baralhos: [{ conjurador: a.conjurador, cartas: lista(a), itens: a.itens }, { conjurador: b.conjurador, cartas: lista(b), itens: b.itens }] });
  for (let i = 0; i < 400 && e.vencedor === null; i++) {
    const j = (e.pendencia ? e.pendencia.jogador : e.prioridade) as Jogador;
    try { executar(e, decidir(e, j, (j ? b : a).estilo)); } catch (err) { if (!(err instanceof ErroRegra)) throw err; executar(e, seguro(e, j)); }
  }
  for (const eu of [0, 1] as Jogador[]) {
    const v = visaoDe(e, eu) as any;
    const op = 1 - eu;
    assert.equal(v.rng, 0);
    assert.equal(v.catalogo, undefined);
    for (const id of v.jogadores[op].mao) assert.equal(v.instancias[id].defId, 'oculta');
    for (const k of [0, 1]) for (const id of v.jogadores[k].baralho) assert.equal(v.instancias[id].defId, 'oculta');
    for (const id of v.jogadores[eu].mao) assert.equal(v.instancias[id].defId, e.instancias[id].defId);
    for (const id of v.jogadores[op].grau) if (!e.instancias[id]?.revelada) assert.equal(v.instancias[id].defId, 'oculta');
    // os apelidos não podem ser ids reais (a ordem dos ids revelaria a carta)
    for (const id of v.jogadores[op].mao) assert.ok(id.startsWith('x'));
    assert.equal(v.jogadores[op].mao.length, e.jogadores[op].mao.length);
  }
});
