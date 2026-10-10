// Expansão A Mestra dos Dragões (liberada pelo autor em 09/10; decisão 41): as 65 cartas jogáveis, as fichas (Ovos e os 9
// Dragões de Durandis), a régua de pontos, o texto gerado e a Dragolize no motor e no bot.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { criarJogo, executar, temPalavra, ataqueAtualDe, vidaAtualDe, ErroRegra } from '../src/motor.ts';
import { decidir } from '../sim/bot.ts';
import { avaliar, bonusGrau } from '../src/balanco.ts';
import { textoCarta } from '../app/textos.js';
import { poderParaConstrutor, construtorParaPoder } from '../src/poder-conjurador.ts';
import { cartasExpansaoDragoes, conjuradoresExpansaoDragoes, artesDaExpansao, EXPANSAO_DRAGOES, OVO_ILUMINADO, OVO_ETEREO, idDragao } from '../src/expansao-dragoes.ts';
import type { CartaDef, Catalogo, Estado, Jogador } from '../src/tipos.ts';

const VALORES = ['conflituo', 'sublimeyn', 'dominum', 'persona', 'alvorum', 'morbidyum', 'cognitium', 'eternyr', 'primitivum'];
const jogaveis = cartasExpansaoDragoes.filter((c) => !c.ficha);
const fichas = cartasExpansaoDragoes.filter((c) => c.ficha);
const por = (t: string) => jogaveis.filter((c) => c.tipo === t);

test('65 cartas jogáveis + Durandis: 12 Aliados, 8 Temporárias, 8 itens, 6 Estruturas, 6 Místicas e 25 Conjuradores', () => {
  assert.equal(por('aliado').length, 12);
  assert.equal(por('temporaria').length, 8);
  assert.equal(por('item').length, 9); // 8 equipamentos das artes 070–077 + Durandis
  assert.equal(por('estrutura').length, 7); // + Repouso na Forja (arte 049)
  assert.equal(por('mistica').length, 6);
  assert.equal(conjuradoresExpansaoDragoes.length, 27); // + Ciantia e Valiant
  // todos marcados com a expansão, sem raridade, ids únicos
  const ids = new Set<string>();
  for (const c of [...cartasExpansaoDragoes, ...conjuradoresExpansaoDragoes]) {
    assert.equal(c.expansao, EXPANSAO_DRAGOES, c.id);
    assert.ok(!('raridade' in c), c.id);
    assert.ok(!ids.has(c.id), c.id);
    ids.add(c.id);
  }
  // Conjuradores: pares de Valores diferentes, nenhum repetido, no padrão dos 9 (Vida 20, mão 5, compra 2)
  const pares = new Set(conjuradoresExpansaoDragoes.map((c) => [...c.valores].sort().join('+')));
  assert.equal(pares.size, 27);
  for (const c of conjuradoresExpansaoDragoes) {
    assert.equal(c.valores.length, 2);
    assert.deepEqual([c.vida, c.maoInicial, c.compraFinal, c.danoBasico, c.custoAtaque], [20, 5, 2, 1, 0], c.id);
    assert.ok(c.habilidade.length > 10, c.id);
  }
});

test('Valores: nenhuma carta neutra (só itens), equilibradas entre os 9 Valores; fichas: 2 Ovos e 1 Dragão por Valor', () => {
  const conta: Record<string, number> = Object.fromEntries(VALORES.map((v) => [v, 0]));
  for (const c of jogaveis) {
    if (c.tipo === 'item') continue;
    assert.ok(c.valores.length >= 1, `${c.id} neutra`);
    if (c.valores.length === 1) conta[c.valores[0]]++;
  }
  const n = Object.values(conta);
  assert.ok(Math.max(...n) - Math.min(...n) <= 2, JSON.stringify(conta)); // 10/10: + Repouso na Forja (2ª arte de Alvorum)
  assert.deepEqual(fichas.filter((c) => c.subtipos.includes('Ovo')).map((c) => c.id).sort(), [OVO_ETEREO, OVO_ILUMINADO].sort());
  for (const v of VALORES) {
    const d = fichas.find((c) => c.id === idDragao(v));
    assert.ok(d && d.valores[0] === v && d.entraSobre === 'Ovo' && (d.ativacao || d.aoDanoConjurador), v);
  }
  // cada Valor tem ao menos uma carta (ou Conjurador) que cria Ovos, para usar Durandis
  const criaOvo = (x: unknown) => JSON.stringify(x).includes(OVO_ILUMINADO) || JSON.stringify(x).includes(OVO_ETEREO);
  for (const v of VALORES) assert.ok(jogaveis.some((c) => c.valores.includes(v) && c.valores.length === 1 && criaOvo(c)), `${v} sem Ovo`);
});

test('régua de pontos (balanco.ts): dentro do orçamento; Aliados contam a curva de Grau e o Ovo que substituem', () => {
  for (const c of jogaveis) {
    if (c.tipo === 'item') continue;
    const a = avaliar(c);
    if (Number.isNaN(a.folga)) continue; // efeitos sem preço na régua (aura, custo das cartas, naoAtaca...)
    const curva = c.tipo === 'aliado' ? Math.floor(bonusGrau(c.grau, 0.5)) : 0;
    const ovo = c.entraSobre ? 3 : 0;
    assert.ok(a.folga >= -(curva + ovo + 1.5), `${c.id} acima do orçamento (${a.folga}, curva ${curva})`);
  }
});

test('texto gerado da mecânica e arte ligada pelo número do manifesto da expansão', () => {
  for (const c of cartasExpansaoDragoes) assert.equal(c.texto, textoCarta({ ...c, texto: '' }), c.id);
  const man = JSON.parse(readFileSync(new URL('../../expansoes/a-mestra-dos-dragoes/arte/colecao-completa-90.visual.json', import.meta.url), 'utf8')) as { itens: { id: string }[] };
  const nums = new Set(man.itens.map((i) => i.id));
  const artes = artesDaExpansao();
  for (const c of [...jogaveis, ...conjuradoresExpansaoDragoes]) {
    if (c.id === 'md-item-durandis') continue; // sem arte ainda
    const a = artes[('comprometimento' in c ? 'conjurador-' : '') + c.id];
    // 10/10: Ciantia (001, fora do manifesto), Valiant (030) e Repouso na Forja (049) usam artes da história
    assert.ok(a && (a === '001' || nums.has(a)) && (+a >= 50 || ['001', '030', '049'].includes(a)), `${c.id}: arte ${a}`);
  }
  // as 65 artes 050–114 viram exatamente uma carta ou Conjurador cada
  const usadas = [...jogaveis, ...conjuradoresExpansaoDragoes].map((c) => artes[('comprometimento' in c ? 'conjurador-' : '') + c.id]).filter(Boolean);
  assert.equal(new Set(usadas).size, 68);
});

test('poder dos 25 Conjuradores: ida e volta pelo construtor do Editor dá o mesmo JSON', () => {
  for (const c of conjuradoresExpansaoDragoes) {
    const v = JSON.parse(JSON.stringify(poderParaConstrutor(c.poder)));
    assert.equal(JSON.stringify(construtorParaPoder(v)), JSON.stringify(c.poder), c.id);
  }
});

// ---------- motor ----------
const catalogo: Catalogo = {
  cartas: {
    ...Object.fromEntries(cartasExpansaoDragoes.map(({ arteExpansao, ...c }) => { void arteExpansao; return [c.id, c as CartaDef]; })),
    nada: { id: 'nada', nome: 'Nada', tipo: 'temporaria', subtipos: [], valores: ['conflituo'], custo: 0, grau: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  },
  conjuradores: Object.fromEntries(conjuradoresExpansaoDragoes.map(({ arteExpansao, ...c }) => { void arteExpansao; return [c.id, c]; })),
};
const DECK = ['md-mandibula-de-juncos', 'md-coletor-de-ovos', 'md-a-escama-que-floresce', 'md-corrente-desfeita', 'md-asas-sobre-a-inundacao', 'md-vespa-de-sino'];

function criar(conj = 'md-naima', itens: string[] = ['md-item-durandis']): Estado {
  const cartas = [...DECK, ...DECK, ...Array.from({ length: 40 }, () => 'nada')];
  const e = criarJogo({ catalogo, baralhos: [{ conjurador: conj, cartas, itens }, { conjurador: 'md-elda', cartas, itens: [] }], semente: 3, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  for (const j of [0, 1] as Jogador[]) for (let i = 0; i < 3; i++) trazer(e, j, 'nada', 'grau');
  return e;
}
/** Traz para a mão (ou o Grau) a primeira cópia da carta no baralho do jogador. */
function trazer(e: Estado, j: Jogador, defId: string, zona: 'mao' | 'grau' = 'mao'): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((x) => e.instancias[x].defId === defId && !z[zona].includes(x))!;
  z.baralho = z.baralho.filter((x) => x !== id);
  z.mao = z.mao.filter((x) => x !== id);
  z[zona].push(id);
  return id;
}
function resolver(e: Estado): void {
  for (let i = 0; i < 50 && (e.pilha.length || e.pendencia) && !e.pendencia; i++) executar(e, { tipo: 'passar', jogador: e.prioridade });
}
const noCampo = (e: Estado, j: Jogador, defId: string) => e.jogadores[j].campo.filter((id) => e.instancias[id].defId === defId);

test('Ovo: criado por carta; carta com entraSobre só se joga com um Ovo e entra no lugar dele com a concessão', () => {
  const e = criar();
  e.jogadores[0].arcano = 20;
  const mand = trazer(e, 0, 'md-mandibula-de-juncos');
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: mand }), ErroRegra);
  executar(e, { tipo: 'jogar', jogador: 0, carta: trazer(e, 0, 'md-a-escama-que-floresce'), alvo: e.jogadores[0].conjuradorId });
  resolver(e);
  const [ovo] = noCampo(e, 0, OVO_ILUMINADO);
  assert.ok(ovo, 'o Ovo Iluminado entrou');
  const pos = e.jogadores[0].campo.indexOf(ovo);
  executar(e, { tipo: 'jogar', jogador: 0, carta: mand });
  resolver(e);
  assert.equal(noCampo(e, 0, OVO_ILUMINADO).length, 0, 'o Ovo saiu');
  assert.ok(!e.jogadores[0].cemiterio.includes(ovo) && !e.jogadores[0].banimento.includes(ovo), 'a ficha deixa de existir');
  assert.equal(e.jogadores[0].campo[pos], mand, 'entrou no lugar do Ovo');
  assert.equal(ataqueAtualDe(e, mand), 6); // 5 + 1 do Ovo Iluminado
  assert.equal(vidaAtualDe(e, mand), 5);
});

test('Durandis: Dragão do Valor com X cartas daquele Valor no Grau, X de Arcano e um Ovo; suspende o Conjurador e não é consumido', () => {
  const e = criar('md-naima'); // Persona + Primitivum
  const z = e.jogadores[0];
  const dur = z.itens.find((id) => e.instancias[id].defId === 'md-item-durandis')!;
  const habs = catalogo.cartas['md-item-durandis'].habilidades!;
  const hP = habs.findIndex((h) => h.requisito?.valor === 'persona');
  z.arcano = 10;
  // sem Ovo: não ativa
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: dur, habilidade: hP }), ErroRegra);
  // Ovo Etéreo pelo poder de Naíma ao entrar um Aliado (Coletor de Ovos ainda traz um Ovo Iluminado)
  executar(e, { tipo: 'jogar', jogador: 0, carta: trazer(e, 0, 'md-vespa-de-sino') });
  resolver(e);
  assert.equal(noCampo(e, 0, OVO_ETEREO).length, 1);
  // Grau: 3 cartas, mas nenhuma de Persona → não ativa o Dragão de Persona (X = 2)
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: dur, habilidade: hP }), /Grau 2 de persona/);
  // com 2 cartas de Persona no Grau
  catalogo.cartas['nada-p'] = { ...catalogo.cartas.nada, id: 'nada-p', valores: ['persona'] };
  for (const id of z.grau) e.instancias[id].defId = 'nada-p';
  executar(e, { tipo: 'ativar', jogador: 0, carta: dur, habilidade: hP });
  resolver(e);
  const [dragao] = noCampo(e, 0, idDragao('persona'));
  assert.ok(dragao, 'o Dragão de Persona entrou');
  assert.equal(noCampo(e, 0, OVO_ETEREO).length, 0, 'em cima do Ovo');
  assert.ok(temPalavra(e, dragao, 'Etéreo'), 'ganha Etéreo do Ovo Etéreo');
  assert.equal(z.arcano, 5); // 10 - 3 (Vespa) - 2 (Dragão de Persona)
  assert.ok(e.instancias[z.conjuradorId].suspensa, 'o Conjurador fica suspenso');
  assert.ok(z.itens.includes(dur) && !e.instancias[dur].suspensa, 'Durandis continua equipado e de pé');
  // Conjurador suspenso: não ativa de novo neste turno
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: dur, habilidade: hP }), ErroRegra);
});

test('dois Ovos: o jogador escolhe em qual o Dragão entra', () => {
  const e = criar('md-naima');
  const z = e.jogadores[0];
  z.arcano = 10;
  executar(e, { tipo: 'jogar', jogador: 0, carta: trazer(e, 0, 'md-coletor-de-ovos') }); // Ovo Iluminado (ao entrar) + Ovo Etéreo (Naíma)
  resolver(e);
  assert.equal(noCampo(e, 0, OVO_ILUMINADO).length + noCampo(e, 0, OVO_ETEREO).length, 2);
  catalogo.cartas['nada-p'] = { ...catalogo.cartas.nada, id: 'nada-p', valores: ['persona'] };
  for (let i = 0; i < 2; i++) e.instancias[trazer(e, 0, 'nada', 'grau')].defId = 'nada-p';
  const dur = z.itens[0];
  executar(e, { tipo: 'ativar', jogador: 0, carta: dur, habilidade: catalogo.cartas['md-item-durandis'].habilidades!.findIndex((h) => h.requisito?.valor === 'persona') });
  while (e.pilha.length && !e.pendencia) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.pendencia?.tipo, 'escolherCartas');
  const ilum = noCampo(e, 0, OVO_ILUMINADO)[0];
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [ilum] });
  const [dragao] = noCampo(e, 0, idDragao('persona'));
  assert.equal(ataqueAtualDe(e, dragao), 4); // 3 + 1 do Ovo Iluminado
  assert.equal(noCampo(e, 0, OVO_ETEREO).length, 1);
});

test('Corrente Desfeita: anula e põe a carta anulada na mão de quem anulou', () => {
  const e = criar('md-teren');
  e.jogadores[0].arcano = 10; e.jogadores[1].arcano = 10;
  const corrente = trazer(e, 1, 'md-corrente-desfeita');
  const vespa = trazer(e, 0, 'md-vespa-de-sino');
  executar(e, { tipo: 'jogar', jogador: 0, carta: vespa });
  executar(e, { tipo: 'passar', jogador: 0 });
  executar(e, { tipo: 'jogar', jogador: 1, carta: corrente, alvo: vespa });
  resolver(e);
  assert.ok(e.jogadores[1].mao.includes(vespa), 'a Vespa foi para a mão do jogador 1');
  assert.equal(e.instancias[vespa].dono, 1);
});

test('bot: usa Durandis quando tem Ovo, Arcano e Grau do Valor; não joga carta de Ovo sem Ovo', () => {
  const e = criar('md-naima');
  const z = e.jogadores[0];
  const est = { nome: 't', agressividade: 0.5, reservaInstantes: false, grauAlvo: 0, queimaFace: false };
  z.mao = [];
  trazer(e, 0, 'md-mandibula-de-juncos');
  z.arcano = 10;
  const c1 = decidir(e, 0, est);
  assert.ok(!(c1.tipo === 'jogar' && e.instancias[c1.carta].defId === 'md-mandibula-de-juncos'), 'sem Ovo, não joga a Mandíbula');
  catalogo.cartas['nada-p'] = { ...catalogo.cartas.nada, id: 'nada-p', valores: ['persona'] };
  for (let i = 0; i < 2; i++) e.instancias[trazer(e, 0, 'nada', 'grau')].defId = 'nada-p';
  executar(e, { tipo: 'jogar', jogador: 0, carta: trazer(e, 0, 'md-vespa-de-sino') });
  resolver(e);
  z.mao = z.mao.filter((id) => e.instancias[id].defId !== 'md-mandibula-de-juncos');
  let usou = false;
  for (let i = 0; i < 20 && !usou; i++) {
    const c = decidir(e, 0, est);
    if (c.tipo === 'passar') break;
    if (c.tipo === 'ativar' && c.carta === z.itens[0]) usou = true;
    executar(e, c);
    resolver(e);
  }
  assert.ok(usou, 'o bot ativou Durandis');
  // o maior Dragão possível: o de Conflitüo (X = 3, as 3 cartas de teste do Grau são de Conflitüo) vale mais que o de Persona
  assert.equal(noCampo(e, 0, idDragao('conflituo')).length, 1);
});

test('Dracolize: o texto usa a palavra-chave em vez de "entra em cima de um Ovo"', () => {
  for (const c of cartasExpansaoDragoes) assert.ok(!/em cima de um Ovo/.test(c.texto), c.id);
  const mand = cartasExpansaoDragoes.find((c) => c.id === 'md-mandibula-de-juncos')!;
  assert.match(mand.texto, /Dracolize/);
  assert.match(cartasExpansaoDragoes.find((c) => c.id === 'md-item-durandis')!.texto, /com Dracolize/);
});

test('Turno 1 (decisão 42): quem começa põe Grau, mas não compra no Final; o segundo compra normalmente', () => {
  const cartas = [...DECK, ...DECK, ...Array.from({ length: 40 }, () => 'nada')];
  const e = criarJogo({ catalogo, baralhos: [{ conjurador: 'md-naima', cartas, itens: [] }, { conjurador: 'md-elda', cartas, itens: [] }], semente: 3, regras: { primeiroSemGrauNoTurno1: false, primeiroSemCompraNoTurno1: true } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  executar(e, { tipo: 'colocarGrau', jogador: 0, carta: e.jogadores[0].mao[0] });
  assert.equal(e.jogadores[0].grau.length, 1);
  const mao0 = e.jogadores[0].mao.length;
  while (e.ativo === 0) { if (e.pendencia) break; executar(e, { tipo: 'passar', jogador: e.prioridade }); }
  assert.equal(e.jogadores[0].mao.length, mao0);
  const mao1 = e.jogadores[1].mao.length;
  while (e.ativo === 1) { if (e.pendencia) break; executar(e, { tipo: 'passar', jogador: e.prioridade }); }
  assert.equal(e.jogadores[1].mao.length, mao1 + 2);
});

test('Valiant (10/10): suspende, paga 5 e anula uma carta na pilha, que vai para a mão dele; Ciantia invoca um Ovo Iluminado', () => {
  const cartas = [...DECK, ...DECK, ...Array.from({ length: 40 }, () => 'nada')];
  const e = criarJogo({ catalogo, baralhos: [{ conjurador: 'md-naima', cartas, itens: [] }, { conjurador: 'md-valiant', cartas, itens: [] }], semente: 3, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  for (const j of [0, 1] as Jogador[]) for (let i = 0; i < 3; i++) trazer(e, j, 'nada', 'grau');
  e.jogadores[0].arcano = 10; e.jogadores[1].arcano = 10;
  const vespa = trazer(e, 0, 'md-vespa-de-sino');
  executar(e, { tipo: 'jogar', jogador: 0, carta: vespa });
  executar(e, { tipo: 'passar', jogador: 0 });
  executar(e, { tipo: 'ativar', jogador: 1, carta: e.jogadores[1].conjuradorId, alvo: vespa });
  resolver(e);
  assert.ok(e.jogadores[1].mao.includes(vespa), 'a Vespa foi para a mão de Valiant');
  assert.equal(e.jogadores[1].arcano, 5);

  const e2 = criar('md-ciantia');
  e2.jogadores[0].arcano = 5;
  executar(e2, { tipo: 'ativar', jogador: 0, carta: e2.jogadores[0].conjuradorId });
  resolver(e2);
  assert.equal(noCampo(e2, 0, OVO_ILUMINADO).length, 1);
});
