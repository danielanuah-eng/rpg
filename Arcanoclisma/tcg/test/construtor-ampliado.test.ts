// Construtor ampliado (pedido do autor em 05/10, decisão 40): eventos de jogador com "de quem" e vezes por turno,
// anti-laço, "enquanto estiver em campo" (aura e custo das cartas, passivos; recorrente com N turnos), manutenção
// (sai, pula, alternativo) e custo condicional por linha (pago por você ou pelo oponente para impedir; ao resolver ou
// ao jogar). Tudo genérico nos dados (campos de Habilidade e Efeito.custo), sem carta específica no motor.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { ataqueAtualDe, criarJogo, custoEfetivo, executar, temPalavra, vidaAtualDe } from '../src/motor.ts';
import { decidir } from '../sim/bot.ts';
import { textoCarta, textoHabilidade, descEfeito } from '../app/textos.js';
import type { CartaDef, Catalogo, Comando, ConjuradorDef, Efeito, Estado, Habilidade, Jogador } from '../src/tipos.ts';

const base = { subtipos: [] as string[], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const aliado = (id: string, custo: number, extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'aliado', custo, ataque: 2, vida: 3, ...extra });
const mistica = (id: string, habilidades: Habilidade[]): CartaDef => ({ ...base, id, nome: id, tipo: 'mistica', vida: 3, habilidades });
const temp = (id: string, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', velocidade: 'repentina', habilidades: [{ quando: 'jogar', efeitos }], ...extra });
const AURA: Habilidade = { quando: 'emCampo', modo: 'passivo', aura: { lado: 'proprio', ataque: 1, vida: 1, palavras: ['Etéreo'] }, efeitos: [] };

const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  aliado('soldado', 2),
  temp('saque', [{ tipo: 'comprar', valor: 1 }]),
  aliado('escriba', 1, { habilidades: [{ quando: 'compra', quem: 'voce', vezesPorTurno: 2, efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  aliado('espiao', 1, { habilidades: [{ quando: 'compra', quem: 'oponente', efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  aliado('egoista', 1, { habilidades: [{ quando: 'compra', quem: 'voce', efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  aliado('biblio', 1, { habilidades: [{ quando: 'compra', quem: 'voce', efeitos: [{ tipo: 'comprar', valor: 1 }] }] }),
  mistica('estandarte', [AURA]),
  aliado('capitao2', 1, { habilidades: [{ quando: 'emCampo', aura: { lado: 'proprio', ataque: 1, excetoEsta: true }, efeitos: [] }] }),
  temp('queda', [{ tipo: 'campo', acao: 'destruir', alcance: 'todos', lado: 'proprio', tipos: ['mistica'] }]),
  mistica('pedagio', [{ quando: 'emCampo', custoCartas: { de: 'oponente', tipos: ['aliado'], valor: 1 }, efeitos: [] }]),
  mistica('desconto', [{ quando: 'emCampo', custoCartas: { de: 'voce', valor: -3 }, efeitos: [] }]),
  { ...base, id: 'fonte', nome: 'fonte', tipo: 'estrutura', vida: 5, habilidades: [{ quando: 'emCampo', modo: 'recorrente', etapa: 'inicio', turnoDe: 'seu', turnos: 2, efeitos: [{ tipo: 'perderVida', valor: 1, quem: 'oponente' }] }] },
  { ...base, id: 'sempre', nome: 'sempre', tipo: 'estrutura', vida: 5, habilidades: [{ quando: 'emCampo', modo: 'recorrente', etapa: 'final', turnoDe: 'cada', efeitos: [{ tipo: 'perderVida', valor: 1, quem: 'oponente' }] }] },
  mistica('pesada', [{ quando: 'emCampo', manutencao: { arcano: 1 }, efeitos: [] }]),
  mistica('pesada-banida', [{ quando: 'emCampo', manutencao: { arcano: 1, destino: 'banimento' }, efeitos: [] }]),
  mistica('bandeira', [{ ...AURA, manutencao: { arcano: 1, senao: 'pula' } }]),
  mistica('pacto', [{ quando: 'emCampo', manutencao: { arcano: 1, senao: 'alternativo', alternativos: [{ tipo: 'perderVida', valor: 2 }] }, efeitos: [] }]),
  mistica('tributo', [{ quando: 'emCampo', manutencao: { arcano: 1, quem: 'oponente', turnoDe: 'oponente' }, efeitos: [] }]),
  temp('ameaca', [{ tipo: 'perderVida', valor: 3, quem: 'oponente', custo: { arcano: 1, quem: 'oponente' } }]),
  temp('oferta', [{ tipo: 'comprar', valor: 2, custo: { vida: 2, quando: 'jogar', senao: 'carta' } }, { tipo: 'arcano', valor: 1 }]),
  temp('escolha', [{ tipo: 'comprar', valor: 2, custo: { arcano: 1, senao: 'alternativo', alternativos: [{ tipo: 'perderVida', valor: 1 }] } }]),
  temp('vazio', [{ tipo: 'comprar', valor: 1, custo: { quem: 'oponente' } }]),
  aliado('mercador', 1, { habilidades: [{ quando: 'jogaAliado', quem: 'voce', pagar: { vida: 1 }, efeitos: [{ tipo: 'comprar', valor: 1 }] }] }),
  aliado('vampiro', 1, { habilidades: [{ quando: 'perdeVida', quem: 'oponente', efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  aliado('carrasco', 1, { habilidades: [{ quando: 'conjuradorDano', quem: 'oponente', efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  aliado('medico', 1, { habilidades: [{ quando: 'ganhaVida', quem: 'voce', efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  aliado('atento', 1, { habilidades: [{ quando: 'pilha', quem: 'oponente', efeitos: [{ tipo: 'arcano', valor: 1 }] }, { quando: 'jogaMistica', efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  temp('sangria', [{ tipo: 'perderVida', valor: 2, quem: 'oponente' }]),
  temp('corte', [{ tipo: 'perderVida', valor: 2, quem: 'oponente', secundario: true }]),
  temp('cura', [{ tipo: 'campo', acao: 'curar', valor: 2, alcance: 'todos', lado: 'proprio', tipos: ['conjurador'] }]),
];

function criar(): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 1, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = { conjurador: 'c', cartas: Array.from({ length: 40 }, () => 'nada') };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 7, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
let n = 0;
function poe(e: Estado, j: Jogador, defId: string, onde: 'campo' | 'mao' | 'grau' | 'cemiterio' = 'campo'): string {
  const id = `q${n++}`;
  e.instancias[id] = { id, defId, ehConjurador: false, dono: j, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.jogadores[j][onde].push(id);
  return id;
}
type Resp = (e: Estado) => Comando | undefined;
/** Responde pendências (padrão: não paga, descarta as últimas, escolhe as primeiras) e passa até `cond`. */
function ate(e: Estado, cond: (e: Estado) => boolean, resp?: Resp, max = 400): void {
  for (let k = 0; k < max && !cond(e) && e.vencedor === null; k++) {
    const p = e.pendencia;
    if (!p) { executar(e, { tipo: 'passar', jogador: e.prioridade }); continue; }
    const c = resp?.(e);
    if (c) { executar(e, c); continue; }
    if (p.tipo === 'descartar') executar(e, { tipo: 'descartar', jogador: p.jogador, cartas: e.jogadores[p.jogador].mao.slice(-p.quantidade) });
    else if (p.tipo === 'pagarCusto') executar(e, { tipo: 'pagarCusto', jogador: p.jogador, pagar: false });
    else if (p.tipo === 'escolherCartas') executar(e, { tipo: 'escolherCartas', jogador: p.jogador, cartas: p.cartas.slice(0, p.quantidade) });
    else throw new Error(`pendência inesperada: ${p.tipo}`);
  }
  assert.ok(cond(e), 'não chegou lá');
}
/** Joga a carta da mão e passa até a pilha esvaziar (respondendo pendências). */
function jogar(e: Estado, j: Jogador, defId: string, resp?: Resp): string {
  const id = poe(e, j, defId, 'mao');
  executar(e, { tipo: 'jogar', jogador: j, carta: id });
  ate(e, (x) => !x.pilha.length && !x.pendencia, resp);
  return id;
}
/** Avança até a fase principal do turno `t`. */
const ateTurno = (e: Estado, t: number, resp?: Resp) => ate(e, (x) => x.turno === t && x.etapa === 'principal' && !x.pilha.length && !x.pendencia, resp);
const vidaConj = (e: Estado, j: Jogador) => 30 - e.instancias[e.jogadores[j].conjuradorId].dano;
const pagar = (sim: boolean, enviar = false): Resp => (e) => {
  const p = e.pendencia;
  if (p?.tipo !== 'pagarCusto') return undefined;
  return { tipo: 'pagarCusto', jogador: p.jogador, pagar: sim, ...(sim && enviar ? { enviarArcano: [e.jogadores[p.jogador].mao[0]] } : {}) };
};

// ---------- eventos de jogador ----------

test('gatilho de compra: "de quem" (você/oponente) e no máximo N vezes por turno', () => {
  const e = criar();
  poe(e, 0, 'escriba');
  poe(e, 1, 'espiao');
  poe(e, 1, 'egoista');
  const a0 = e.jogadores[0].arcano, a1 = e.jogadores[1].arcano;
  for (let k = 0; k < 3; k++) jogar(e, 0, 'saque');
  assert.equal(e.jogadores[0].arcano - a0, 2, 'escriba: 3 compras, só 2 vezes por turno');
  assert.equal(e.jogadores[1].arcano - a1, 3, 'espiao: toda compra do oponente; egoista: só as próprias');
  // no turno seguinte o limite volta (a compra do Final do jogador 0 ainda é no turno 1)
  ateTurno(e, 3);
  const b0 = e.jogadores[0].arcano;
  jogar(e, 0, 'saque');
  assert.equal(e.jogadores[0].arcano - b0, 1);
});

test('anti-laço: "quando você compra, compre 1" não vira laço infinito (nem com duas cartas iguais)', () => {
  const e = criar();
  poe(e, 0, 'biblio');
  const h = e.jogadores[0].mao.length;
  jogar(e, 0, 'saque');
  assert.equal(e.jogadores[0].mao.length, h + 2, 'compra 1 do saque + 1 do gatilho, que não dispara de novo');
  poe(e, 0, 'biblio');
  const h2 = e.jogadores[0].mao.length;
  jogar(e, 0, 'saque');
  assert.equal(e.pilha.length, 0);
  assert.ok(e.jogadores[0].mao.length > h2 && e.jogadores[0].mao.length <= h2 + 6, 'termina com um número finito de compras');
});

test('perder Vida, tomar dano e ganhar Vida são eventos diferentes', () => {
  const e = criar();
  poe(e, 0, 'vampiro');
  poe(e, 0, 'carrasco');
  let a = e.jogadores[0].arcano;
  jogar(e, 0, 'sangria'); // "perde 2 de Vida": perde Vida, não toma dano
  assert.equal(vidaConj(e, 1), 28);
  assert.equal(e.jogadores[0].arcano - a, 1, 'só o vampiro');
  a = e.jogadores[0].arcano;
  jogar(e, 0, 'corte'); // "toma 2 de dano": toma dano e perde Vida
  assert.equal(e.jogadores[0].arcano - a, 2, 'vampiro e carrasco');
  // ganhar Vida: cura de fato no Conjurador
  poe(e, 0, 'medico');
  a = e.jogadores[0].arcano;
  jogar(e, 0, 'cura'); // sem dano: nada a curar, não ganha Vida
  assert.equal(e.jogadores[0].arcano - a, 0);
  e.instancias[e.jogadores[0].conjuradorId].dano = 5;
  jogar(e, 0, 'cura');
  assert.equal(e.jogadores[0].arcano - a, 1);
  assert.equal(vidaConj(e, 0), 27);
});

test('jogar carta: pilha (de quem) e por tipo', () => {
  const e = criar();
  poe(e, 1, 'atento');
  const a = e.jogadores[1].arcano;
  jogar(e, 0, 'saque'); // pilha do oponente: +1
  jogar(e, 0, 'pedagio'); // pilha do oponente e Mística de qualquer um: +2
  assert.equal(e.jogadores[1].arcano - a, 3);
});

test('gatilho com custo: quem controla decide pagar; sem pagar, os efeitos não acontecem', () => {
  const e = criar();
  poe(e, 0, 'mercador');
  e.jogadores[0].arcano = 10;
  let h = e.jogadores[0].mao.length;
  jogar(e, 0, 'soldado', pagar(true));
  assert.equal(vidaConj(e, 0), 29);
  assert.equal(e.jogadores[0].mao.length, h + 1);
  h = e.jogadores[0].mao.length;
  jogar(e, 0, 'soldado', pagar(false));
  assert.equal(vidaConj(e, 0), 29);
  assert.equal(e.jogadores[0].mao.length, h);
});

// ---------- enquanto estiver em campo: passivos ----------

test('aura: bônus e palavras para o grupo, recalculados; some quando a fonte sai do campo', () => {
  const e = criar();
  const s = poe(e, 0, 'soldado');
  const inimigo = poe(e, 1, 'soldado');
  poe(e, 0, 'estandarte');
  assert.equal(ataqueAtualDe(e, s), 3);
  assert.equal(vidaAtualDe(e, s), 4);
  assert.ok(temPalavra(e, s, 'Etéreo'));
  assert.equal(ataqueAtualDe(e, inimigo), 2, 'lado próprio: o inimigo não ganha');
  assert.ok(!temPalavra(e, inimigo, 'Etéreo'));
  e.instancias[s].dano = 3; // com a aura ele aguenta 3
  jogar(e, 0, 'queda'); // destrói as Místicas próprias: a aura some e o soldado cai
  assert.equal(e.jogadores[0].campo.includes(s), false, 'sem a aura, 3 de dano derruba o soldado');
  assert.equal(ataqueAtualDe(e, inimigo), 2);
  // "menos esta carta"
  const c1 = poe(e, 0, 'capitao2');
  const s2 = poe(e, 0, 'soldado');
  assert.equal(ataqueAtualDe(e, c1), 2);
  assert.equal(ataqueAtualDe(e, s2), 3);
});

test('custo das cartas: passivo para você ou para o oponente, por tipo, nunca abaixo de 0', () => {
  const e = criar();
  poe(e, 0, 'pedagio');
  const meu = poe(e, 0, 'soldado', 'mao');
  const dele = poe(e, 1, 'soldado', 'mao');
  const temp1 = poe(e, 1, 'saque', 'mao');
  assert.equal(custoEfetivo(e, 0, meu), 2);
  assert.equal(custoEfetivo(e, 1, dele), 3);
  assert.equal(custoEfetivo(e, 1, temp1), 0, 'só Aliados');
  const d = poe(e, 0, 'desconto');
  assert.equal(custoEfetivo(e, 0, meu), 0);
  e.jogadores[0].campo = e.jogadores[0].campo.filter((x) => x !== d);
  assert.equal(custoEfetivo(e, 0, meu), 2, 'a fonte saiu: volta ao normal');
});

// ---------- enquanto estiver em campo: recorrente ----------

test('recorrente: no início do seu turno, só nas N primeiras vezes', () => {
  const e = criar();
  poe(e, 0, 'fonte');
  ateTurno(e, 3);
  assert.equal(vidaConj(e, 1), 29);
  ateTurno(e, 5);
  assert.equal(vidaConj(e, 1), 28);
  ateTurno(e, 7);
  assert.equal(vidaConj(e, 1), 28, 'depois de 2 vezes, para');
});

test('recorrente "de cada turno": no Final dos dois turnos; para quando a carta sai', () => {
  const e = criar();
  const s = poe(e, 0, 'sempre');
  ateTurno(e, 2);
  assert.equal(vidaConj(e, 1), 29);
  ateTurno(e, 3);
  assert.equal(vidaConj(e, 1), 28);
  e.jogadores[0].campo = e.jogadores[0].campo.filter((x) => x !== s);
  ateTurno(e, 5);
  assert.equal(vidaConj(e, 1), 28);
});

// ---------- manutenção ----------

test('manutenção: sem pagar, a carta sai (cemitério por padrão ou o destino escolhido); pagando, fica', () => {
  const e = criar();
  const p = poe(e, 0, 'pesada');
  const b = poe(e, 0, 'pesada-banida');
  ateTurno(e, 3, (x) => (x.pendencia?.tipo === 'pagarCusto' && x.pendencia.fonte === p ? pagar(true, true)(x) : undefined));
  assert.ok(e.jogadores[0].campo.includes(p), 'pagou (enviando uma carta ao Arcano): fica');
  assert.ok(e.jogadores[0].banimento.includes(b), 'não pagou: banida');
  ateTurno(e, 5);
  assert.ok(e.jogadores[0].cemiterio.includes(p), 'não pagou: cemitério');
});

test('manutenção "pula": sem pagar, o passivo não vale neste turno e volta no seguinte', () => {
  const e = criar();
  const s = poe(e, 0, 'soldado');
  poe(e, 0, 'bandeira');
  assert.equal(ataqueAtualDe(e, s), 3);
  ateTurno(e, 3);
  assert.equal(ataqueAtualDe(e, s), 2, 'não pagou: desligada no turno 3');
  ate(e, (x) => x.turno === 4);
  assert.equal(ataqueAtualDe(e, s), 3, 'volta no turno seguinte');
});

test('manutenção "alternativo" e manutenção paga pelo oponente no turno dele', () => {
  const e = criar();
  poe(e, 0, 'pacto');
  ateTurno(e, 3);
  assert.equal(vidaConj(e, 0), 28, 'não pagou: o efeito alternativo acontece');
  const e2 = criar();
  const t = poe(e2, 0, 'tributo');
  ateTurno(e2, 2, (x) => (x.pendencia?.tipo === 'pagarCusto' ? (assert.equal(x.pendencia.jogador, 1), assert.equal(x.pendencia.impede, true), pagar(true, true)(x)) : undefined));
  assert.ok(e2.jogadores[0].cemiterio.includes(t), 'o oponente pagou: a carta sai');
});

// ---------- custo por linha ----------

test('custo pago pelo oponente para impedir: paga e impede; não paga ou não pode, acontece', () => {
  const e = criar();
  e.jogadores[1].arcano = 1;
  jogar(e, 0, 'ameaca', pagar(true));
  assert.equal(vidaConj(e, 1), 30, 'pagou 1 de Arcano e impediu');
  assert.equal(e.jogadores[1].arcano, 0);
  jogar(e, 0, 'ameaca', pagar(false));
  assert.equal(vidaConj(e, 1), 27);
  e.jogadores[1].mao = [];
  for (const g of e.jogadores[1].grau) e.instancias[g].suspensa = true;
  jogar(e, 0, 'ameaca', () => assert.fail('sem como pagar, não pergunta'));
  assert.equal(vidaConj(e, 1), 24);
});

test('custo decidido ao jogar: "a carta é anulada" tira a carta da pilha; pagando, resolve sem perguntar de novo', () => {
  const e = criar();
  let id = poe(e, 0, 'oferta', 'mao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: id });
  assert.equal(e.pendencia?.tipo, 'pagarCusto', 'pergunta ao jogar');
  const h = e.jogadores[0].mao.length;
  executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: false });
  assert.equal(e.pilha.length, 0, 'anulada: saiu da pilha');
  assert.ok(e.jogadores[0].cemiterio.includes(id));
  assert.equal(e.jogadores[0].mao.length, h);
  id = poe(e, 0, 'oferta', 'mao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: id });
  executar(e, { tipo: 'pagarCusto', jogador: 0, pagar: true });
  assert.equal(vidaConj(e, 0), 28);
  const a = e.jogadores[0].arcano;
  ate(e, (x) => !x.pilha.length, () => assert.fail('não pergunta de novo'));
  assert.equal(e.jogadores[0].mao.length, h + 1 - 1 + 2);
  assert.equal(e.jogadores[0].arcano - a, 1);
});

test('custo com efeito alternativo; custo vazio (ainda sendo montado) não pergunta nada', () => {
  const e = criar();
  const h = e.jogadores[0].mao.length;
  jogar(e, 0, 'escolha', pagar(false));
  assert.equal(vidaConj(e, 0), 29, 'não pagou: alternativo');
  assert.equal(e.jogadores[0].mao.length, h);
  e.jogadores[0].arcano = 1;
  jogar(e, 0, 'escolha', pagar(true));
  assert.equal(e.jogadores[0].mao.length, h + 2);
  jogar(e, 0, 'vazio', () => assert.fail('custo vazio não pergunta'));
  assert.equal(e.jogadores[0].mao.length, h + 3);
});

// ---------- bot ----------

const EST = { nome: 'teste', agressividade: 0.5, reservaInstantes: false, grauAlvo: 2, queimaFace: true };
test('bot responde à pendência pagarCusto: paga quando vale e pode, recusa quando não pode', () => {
  const e = criar();
  e.jogadores[1].arcano = 1;
  const id = poe(e, 0, 'ameaca', 'mao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: id });
  ate(e, (x) => !!x.pendencia);
  const c = decidir(e, 1, EST);
  assert.equal(c.tipo, 'pagarCusto');
  assert.equal((c as { pagar: boolean }).pagar, true, '3 de Vida valem 1 de Arcano');
  executar(e, c);
  assert.equal(vidaConj(e, 1), 30);
});

test('bot joga partidas inteiras com as mecânicas novas sem travar', () => {
  const ids = ['soldado', 'saque', 'escriba', 'biblio', 'estandarte', 'pedagio', 'fonte', 'pesada', 'bandeira', 'pacto', 'tributo', 'ameaca', 'oferta', 'escolha', 'mercador', 'vampiro', 'carrasco', 'atento', 'sangria', 'cura'];
  for (const semente of [1, 2, 3]) {
    const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0, habilidade: '' };
    const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
    const deck = { conjurador: 'c', cartas: Array.from({ length: 40 }, (_, k) => ids[k % ids.length]) };
    const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente, regras: { primeiroSemGrauNoTurno1: false } });
    let k = 0;
    for (; k < 6000 && e.vencedor === null && e.turno < 40; k++) {
      const j = (e.pendencia ? e.pendencia.jogador : e.prioridade) as Jogador;
      executar(e, decidir(e, j, EST));
    }
    assert.ok(k < 6000, `semente ${semente}: não trava`);
  }
});

// ---------- texto automático ----------

test('texto automático das opções novas', () => {
  const txt = (id: string) => textoCarta(cartas.find((c) => c.id === id)!);
  assert.match(textoHabilidade(cartas.find((c) => c.id === 'escriba')!.habilidades![0]), /^Quando você compra cartas \(até 2 vezes por turno\): gere 1 de Arcano|^Quando você compra cartas \(até 2 vezes por turno\)/);
  assert.match(txt('estandarte'), /Enquanto estiver em campo, seus Aliados ganham \+1\/\+1 e Etéreo/);
  assert.match(txt('capitao2'), /menos esta carta/);
  assert.match(txt('pedagio'), /as Aliados? do oponente custam 1 a mais|Aliados do oponente custam 1 a mais/);
  assert.match(txt('fonte'), /no início do seu turno/);
  assert.match(txt('fonte'), /2/);
  assert.match(txt('pesada'), /pague 1 de Arcano ou ela vai para o cemitério/);
  assert.match(txt('bandeira'), /não acontece neste turno/);
  assert.match(txt('tributo'), /o oponente pode pagar 1 de Arcano; se fizer isso, ela vai para o cemitério/);
  assert.match(txt('ameaca'), /a menos que o oponente pague 1 de Arcano/);
  assert.match(txt('oferta'), /você pode pagar 2 de Vida ao jogar esta carta; se fizer isso, compre 2 cartas; se não, esta carta é anulada/);
  assert.match(txt('escolha'), /se não, seu Conjurador perde 1 de Vida/);
  assert.match(txt('mercador'), /Quando você joga um Aliado: você pode pagar 1 de Vida; se fizer isso, compre 1 carta/);
  assert.doesNotMatch(descEfeito({ tipo: 'comprar', valor: 1, custo: { quem: 'oponente' } }), /oponente/, 'custo vazio não aparece');
  assert.equal(descEfeito({ tipo: 'comprar', valor: 1, custo: { arcano: 1, descartar: 1, quem: 'oponente' } }).includes('pague 1 de Arcano e descarte 1 carta'), true);
});
