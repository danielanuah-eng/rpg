// Custo condicional na mesa (decisão 40, 05/10): o que o painel "pagarCusto" do site lê da pendência e os comandos que
// ele manda (Pagar com cartas da mão enviadas ao Arcano, Não pagar, e as escolhas de descarte/sacrifício que vêm depois).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import { textoCustoPagar, juntarEfeitos } from '../app/textos.js';
import type { CartaDef, Catalogo, ConjuradorDef, Efeito, Estado, Habilidade, Jogador, Pendencia } from '../src/tipos.ts';

const base = { subtipos: [] as string[], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const aliado = (id: string, custo: number, extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'aliado', custo, ataque: 2, vida: 3, ...extra });
const mistica = (id: string, habilidades: Habilidade[]): CartaDef => ({ ...base, id, nome: id, tipo: 'mistica', vida: 3, habilidades });
const temp = (id: string, efeitos: Efeito[]): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', velocidade: 'repentina', habilidades: [{ quando: 'jogar', efeitos }] });

const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'rica', nome: 'Rica', tipo: 'temporaria', arcanoGerado: 2, efeitos: [{ tipo: 'comprar', valor: 0 }] },
  aliado('soldado', 0),
  // gatilho com custo: o painel mostra os efeitos do bloco ("compre 2 cartas")
  aliado('mercador', 0, { habilidades: [{ quando: 'jogaAliado', quem: 'voce', pagar: { arcano: 2 }, efeitos: [{ tipo: 'comprar', valor: 2 }] }] }),
  // manutenção: a linha "pagar" sozinha, com a carta-fonte
  mistica('pesada', [{ quando: 'emCampo', manutencao: { arcano: 1 }, efeitos: [] }]),
  // custo por linha pago com descarte e sacrifício: as escolhas vêm depois do Pagar
  temp('pacto', [{ tipo: 'comprar', valor: 3, custo: { descartar: 1, sacrificar: 1 } }]),
  // o oponente paga para impedir
  temp('ameaca', [{ tipo: 'perderVida', valor: 3, quem: 'oponente', custo: { arcano: 1, quem: 'oponente' } }]),
  // decidido ao jogar: a carta na pilha é a fonte
  temp('oferta', [{ tipo: 'comprar', valor: 2, custo: { arcano: 1, quando: 'jogar', senao: 'carta' } }]),
];

function criar(): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 1, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = { conjurador: 'c', cartas: Array.from({ length: 40 }, () => 'nada') };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 3, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
let n = 0;
function poe(e: Estado, j: Jogador, defId: string, onde: 'campo' | 'mao' | 'grau' = 'campo'): string {
  const id = `m${n++}`;
  e.instancias[id] = { id, defId, ehConjurador: false, dono: j, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.jogadores[j][onde].push(id);
  return id;
}
/** Passa até aparecer uma pendência pagarCusto (ou a pilha esvaziar). */
function atePendencia(e: Estado): Extract<Pendencia, { tipo: 'pagarCusto' }> | null {
  for (let k = 0; k < 50; k++) {
    if (e.pendencia?.tipo === 'pagarCusto') return e.pendencia;
    if (e.pendencia || (!e.pilha.length && k > 0)) return null;
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
  return null;
}

test('gatilho com custo: a pendência traz a carta-fonte, o bloco de efeitos e o custo que o painel mostra', () => {
  const e = criar();
  const merc = poe(e, 0, 'mercador');
  e.jogadores[0].arcano = 5;
  executar(e, { tipo: 'jogar', jogador: 0, carta: poe(e, 0, 'soldado', 'mao') });
  const p = atePendencia(e);
  assert.ok(p, 'sem pendência de custo');
  assert.equal(p.jogador, 0);
  assert.equal(p.impede, false);
  assert.equal(p.fonte, merc);
  assert.equal(p.efeito.tipo, 'pagar');
  const bloco = (p.depois?.efeitos ?? []).slice(0, (p.efeito as { bloco?: number }).bloco);
  assert.equal(juntarEfeitos(bloco), 'compre 2 cartas');
  assert.equal(textoCustoPagar(p.custo), 'pagar 2 de Arcano');
});

test('Pagar com cartas da mão enviadas ao Arcano (botão Pagar da mesa)', () => {
  const e = criar();
  poe(e, 0, 'mercador');
  e.jogadores[0].arcano = 0;
  e.jogadores[0].grau = [];
  const rica = poe(e, 0, 'rica', 'mao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: poe(e, 0, 'soldado', 'mao') });
  const p = atePendencia(e);
  assert.ok(p);
  const mao = e.jogadores[0].mao.length;
  executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: true, enviarArcano: [rica] });
  assert.ok(e.jogadores[0].zonaArcano.includes(rica), 'a carta foi para a zona de Arcano');
  while (e.pilha.length && !e.pendencia) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.jogadores[0].mao.length, mao - 1 + 2, 'enviou 1 e comprou 2');
});

test('Pagar com cartas demais enviadas ao Arcano é recusado (a mesa avisa antes)', () => {
  const e = criar();
  poe(e, 0, 'mercador');
  e.jogadores[0].arcano = 2;
  const extra = e.jogadores[0].mao[0];
  executar(e, { tipo: 'jogar', jogador: 0, carta: poe(e, 0, 'soldado', 'mao') });
  assert.ok(atePendencia(e));
  assert.throws(() => executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: true, enviarArcano: [extra] }));
  assert.equal(e.pendencia?.tipo, 'pagarCusto', 'a pendência continua');
});

test('manutenção: a pendência tem a carta-fonte e a linha "pagar" sem bloco (painel: continua em campo / vai para o cemitério)', () => {
  const e = criar();
  const pes = poe(e, 0, 'pesada');
  e.jogadores[0].arcano = 0;
  // vai até o início do próximo turno do jogador 0
  let p: Extract<Pendencia, { tipo: 'pagarCusto' }> | null = null;
  for (let k = 0; k < 200 && !p; k++) {
    if (e.pendencia?.tipo === 'pagarCusto') { p = e.pendencia; break; }
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(-e.pendencia.quantidade) }); continue; }
    if (e.ativo === 0 && e.turno > 1) e.jogadores[0].arcano = Math.max(e.jogadores[0].arcano, 1);
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
  assert.ok(p, 'a manutenção não foi cobrada');
  assert.equal(p.fonte, pes);
  assert.equal(p.efeito.tipo, 'pagar');
  assert.ok(!(p.efeito as { bloco?: number }).bloco && !p.depois?.efeitos?.length);
  executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: false });
  while (e.pilha.length && !e.pendencia) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.ok(e.jogadores[0].cemiterio.includes(pes), 'sem pagar, vai para o cemitério');
});

test('custo com descarte e sacrifício: depois do Pagar, o pagador escolhe as cartas', () => {
  const e = criar();
  const sold = poe(e, 0, 'soldado');
  const mao = e.jogadores[0].mao.length;
  const pac = poe(e, 0, 'pacto', 'mao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: pac });
  const p = atePendencia(e);
  assert.ok(p);
  assert.equal(p.fonte, pac, 'custo por linha: o painel mostra a carta que está resolvendo');
  assert.equal(textoCustoPagar(p.custo), 'descartar 1 carta e sacrificar um Aliado');
  executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: true });
  // as escolhas (descarte e sacrifício) aparecem como pendências comuns, ou vão sozinhas quando só há uma opção
  for (let k = 0; k < 10 && e.pendencia; k++) {
    const q = e.pendencia;
    if (q.tipo === 'escolherCartas') executar(e, { tipo: 'escolherCartas', jogador: q.jogador, cartas: q.cartas.slice(0, q.quantidade) });
    else if (q.tipo === 'descartar') executar(e, { tipo: 'descartar', jogador: q.jogador, cartas: e.jogadores[q.jogador].mao.slice(-q.quantidade) });
    else break;
  }
  while (e.pilha.length && !e.pendencia) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.ok(e.jogadores[0].cemiterio.includes(sold), 'sacrificou o Aliado');
  assert.equal(e.jogadores[0].mao.length, mao - 1 + 3, 'descartou 1 e comprou 3');
});

test('o oponente paga para impedir: a pendência é dele, com impede (botões Pagar e impedir / Deixar acontecer)', () => {
  const e = criar();
  e.jogadores[1].arcano = 3;
  executar(e, { tipo: 'jogar', jogador: 0, carta: poe(e, 0, 'ameaca', 'mao') });
  const p = atePendencia(e);
  assert.ok(p);
  assert.equal(p.jogador, 1);
  assert.equal(p.impede, true);
  assert.equal(p.dono, 0);
  assert.equal(juntarEfeitos([{ ...p.efeito, custo: undefined }]).includes('3'), true);
  executar(e, { tipo: 'pagarCusto', jogador: 1, pagar: true });
  while (e.pilha.length && !e.pendencia) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 0, 'pagou e impediu');
});

test('decidido ao jogar: a fonte é a carta na pilha (visível) e a decisão fica no item', () => {
  const e = criar();
  e.jogadores[0].arcano = 3;
  const id = poe(e, 0, 'oferta', 'mao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: id });
  const p = e.pendencia;
  assert.equal(p?.tipo, 'pagarCusto');
  if (p?.tipo !== 'pagarCusto') return;
  assert.equal(p.fonte, id);
  assert.ok(p.naPilha);
  executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: false });
  assert.ok(!e.pilha.some((x) => x.tipo === 'carta' && x.instId === id), 'sem pagar, a carta é anulada e sai da pilha');
});
