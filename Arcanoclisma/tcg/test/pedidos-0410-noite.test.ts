// Pacote do autor de 04/10 (noite): Protegido unificado (o antigo "Protegido por mágica"/"Proteção mágica" virou variação
// do Protegido), efeitos secundários (dano no Conjurador, descarte, sacrifício, Grau ao cemitério), reanimar estendido
// (pagamento, cemitério, filtros), gatilho "no Cataclisma do turno do oponente" e o efeito "tirar da mão" (descartar/banir).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, custoProtecao, executar, normalizarProtegido, protegidoDe, protegidosDe, arcanoNaHora } from '../src/motor.ts';
import { decidir } from '../sim/bot.ts';
import { aplicarCatalogo, edicoesVazias } from '../sim/edicoes.ts';
import { cartasColecao } from '../src/cartas-colecao.ts';
import { textoCarta, textoPoder, descEfeito } from '../app/textos.js';
import type { CartaDef, Catalogo, ConjuradorDef, Efeito, Estado, Habilidade, Jogador, PoderConjurador } from '../src/tipos.ts';

const base = { subtipos: [] as string[], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const aliado = (id: string, custo: number, extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'aliado', custo, ataque: 2, vida: 3, ...extra });
const temp = (id: string, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', velocidade: 'repentina', habilidades: [{ quando: 'jogar', efeitos }], ...extra });
const sec = <T extends Efeito>(ef: T): T => ({ ...ef, secundario: true });

const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  aliado('soldado', 2),
  { ...base, id: 'altar', nome: 'altar', tipo: 'mistica', vida: 3 },
  { ...base, id: 'torre', nome: 'torre', tipo: 'estrutura', vida: 5 },
  aliado('caro', 5, { ataque: 5, vida: 6 }),
  aliado('morto', 1, { subtipos: ['Morto-vivo'] }),
  aliado('guardiao', 1, { palavrasChave: ['Protessência'], protegido: 3 }),
  aliado('cofre', 0, { protegido: { arcano: 1, vida: 2, grauParaCemiterio: 1 } }),
  aliado('vigia', 1, { habilidades: [{ quando: 'cataclismaOponente', efeitos: [{ tipo: 'comprar', valor: 1 }] }] }),
  temp('raio', [{ tipo: 'campo', acao: 'dano', valor: 1, alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }]),
  temp('pacto', [{ tipo: 'comprar', valor: 1 }, sec({ tipo: 'perderVida', valor: 2 }), sec({ tipo: 'sacrificar', valor: 1 })]),
  temp('pacto2', [{ tipo: 'arcano', valor: 1 }, sec({ tipo: 'sacrificar', valor: 2, quem: 'oponente' })]),
  temp('raizes', [{ tipo: 'arcano', valor: 1 }, sec({ tipo: 'grauAoCemiterio', valor: 2 })]),
  temp('custo-mao', [{ tipo: 'comprar', valor: 2 }, sec({ tipo: 'descartar', quem: 'voce', valor: 1 })]),
  temp('fatal', [{ tipo: 'comprar', valor: 1 }, sec({ tipo: 'perderVida', valor: 29 })]),
  temp('acaso', [{ tipo: 'descartar', quem: 'oponente', valor: 2, modo: 'acaso' }]),
  temp('acaso-velho', [{ tipo: 'descartar', quem: 'oponente', valor: 1, aleatorio: true }]),
  temp('exilio', [{ tipo: 'descartar', quem: 'oponente', valor: 1, acao: 'banir', modo: 'dono' }]),
  temp('olhar', [{ tipo: 'descartar', quem: 'oponente', valor: 1, acao: 'banir', modo: 'olhando' }]),
  temp('todos', [{ tipo: 'descartar', quem: 'ambos', valor: 1 }]),
  temp('volta-custo', [{ tipo: 'reanimar', custoExtra: 0, pagamento: 'custo' }]),
  temp('volta-gratis', [{ tipo: 'reanimar', custoExtra: 0, pagamento: 'gratis', de: 'seu' }]),
  temp('volta-mais', [{ tipo: 'reanimar', custoExtra: 2, pagamento: 'custoMais', de: 'oponente', concessao: { palavras: ['Etéreo'] } }]),
  temp('volta-filtro', [{ tipo: 'reanimar', custoExtra: 0, pagamento: 'gratis', custoMax: 3, subtipo: 'morto-vivo' }]),
];

function criar(poder?: PoderConjurador): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '', ...(poder ? { poder } : {}) };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, () => 'nada') };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 5, regras: { primeiroSemGrauNoTurno1: false } });
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
/** Joga a carta e passa até a pilha parar (ou abrir uma escolha). */
function jogarEPassar(e: Estado, j: Jogador, defId: string, alvo?: string): void {
  executar(e, { tipo: 'jogar', jogador: j, carta: poe(e, j, defId, 'mao'), alvo });
  for (let k = 0; k < 10 && e.pilha.length && !e.pendencia; k++) executar(e, { tipo: 'passar', jogador: e.prioridade });
}
const vidaConj = (e: Estado, j: Jogador) => 30 - e.instancias[e.jogadores[j].conjuradorId].dano;

// ---------- 1. Protegido unificado ----------
test('Protegido: o nome antigo (protecao) carrega e vira Protegido; número = só Arcano', () => {
  assert.deepEqual(normalizarProtegido({ protecao: { vida: 2, descartar: 1, grauMin: 1 } }), { protegido: { vida: 2, descartar: 1, grauMin: 1 } });
  assert.deepEqual(normalizarProtegido({ protecao: { arcano: 1 } }), { protegido: 1 });
  assert.deepEqual(normalizarProtegido({ protegido: 2, protecao: { arcano: 1, vida: 1 } }), { protegido: { arcano: 3, vida: 1 } });
  const so = { protegido: 2 };
  assert.equal(normalizarProtegido(so), so, 'sem nome antigo: a mesma carta');
  assert.deepEqual(protegidoDe({ protegido: 2 }), { arcano: 2 });
  assert.equal(protegidoDe({}), null);
  // edição antiga do autor com `protecao` é convertida na carga
  const ed = { ...edicoesVazias(), cartas: { 'col-guarda-costas': { protecao: { vida: 1 } } } };
  const cat = aplicarCatalogo({ cartas: Object.fromEntries(cartasColecao.map((c) => [c.id, c])), conjuradores: {} }, ed as never);
  assert.deepEqual(cat.cartas['col-guarda-costas'].protegido, { arcano: 2, vida: 1 });
  assert.equal(cat.cartas['col-guarda-costas'].protecao, undefined);
});

test('Protegido com variações: o oponente paga Arcano, Vida e Grau ao cemitério; o texto é um conceito só', () => {
  const e = criar();
  const cofre = poe(e, 1, 'cofre');
  const g = poe(e, 0, 'nada', 'grau');
  assert.equal(custoProtecao(e, 0, cofre), 1);
  e.jogadores[0].arcano = 1;
  jogarEPassar(e, 0, 'raio', cofre);
  assert.equal(vidaConj(e, 0), 28);
  assert.ok(e.jogadores[0].cemiterio.includes(g));
  assert.equal(e.jogadores[0].arcano, 0);
  assert.equal(textoCarta(cartas.find((c) => c.id === 'cofre')!), 'Protegido 1: para mirar esta carta, o oponente também precisa pagar 2 de Vida e mandar 1 carta do Grau ao cemitério.');
  assert.equal(textoCarta({ ...base, id: 'x', nome: 'x', tipo: 'aliado', protegido: 2 } as CartaDef), 'Protegido 2.');
  assert.equal(textoCarta({ ...base, id: 'x', nome: 'x', tipo: 'aliado', protecao: { grauMin: 2 } } as CartaDef), 'Protegido: para mirar esta carta, o oponente precisa ter 2+ no Grau.');
});

// ---------- 2. Efeitos secundários ----------
test('secundários: texto "...; depois, seu Conjurador toma 2 de dano e você sacrifica um Aliado"', () => {
  assert.equal(textoCarta(cartas.find((c) => c.id === 'pacto')!), 'Ao jogar: compre 1 carta; depois, seu Conjurador toma 2 de dano e você sacrifica um Aliado.');
  assert.match(textoCarta(cartas.find((c) => c.id === 'raizes')!), /; depois, você manda 2 cartas do seu Grau ao cemitério\.$/);
  assert.match(textoCarta(cartas.find((c) => c.id === 'pacto2')!), /; depois, o oponente sacrifica 2 Aliados dele\.$/);
  assert.match(textoCarta(cartas.find((c) => c.id === 'custo-mao')!), /; depois, você descarta 1 carta\.$/);
  // no Conjurador também
  assert.equal(textoPoder({ habilidades: [{ quando: 'ativada', custo: { suspender: true }, efeitos: [{ tipo: 'comprar', valor: 2 }, sec({ tipo: 'perderVida', valor: 1 })] }] }), 'Suspenda o Conjurador: compre 2 cartas; depois, seu Conjurador toma 1 de dano.');
});

test('sacrificar: o dono escolhe; vai para o cemitério sem mirar (ignora Protessência e Protegido) e conta como morte', () => {
  const e = criar({ habilidades: [{ quando: 'aliadoMorre', efeitos: [{ tipo: 'arcano', valor: 3 }] }] });
  const guard = poe(e, 0, 'guardiao');
  const sold = poe(e, 0, 'soldado');
  const mao = e.jogadores[0].mao.length;
  jogarEPassar(e, 0, 'pacto');
  assert.equal(vidaConj(e, 0), 28, 'toma 2 de dano');
  assert.equal(e.jogadores[0].mao.length, mao + 1, 'comprou antes');
  const p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 0 && p.cartas.includes(guard) && p.cartas.includes(sold));
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [guard] });
  assert.ok(e.jogadores[0].cemiterio.includes(guard));
  assert.ok(e.jogadores[0].campo.includes(sold));
  for (let k = 0; k < 6 && e.pilha.length; k++) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.jogadores[0].arcano, 3, 'gatilho "um Aliado seu vai para o cemitério" disparou');
});

test('sacrificar: sem escolha quando só há o necessário; sem unidade, nada; "o oponente" escolhe as dele', () => {
  const e = criar();
  const s = poe(e, 0, 'soldado');
  jogarEPassar(e, 0, 'pacto');
  assert.equal(e.pendencia, null);
  assert.ok(e.jogadores[0].cemiterio.includes(s));
  jogarEPassar(e, 0, 'pacto'); // sem unidades: só o dano
  assert.equal(vidaConj(e, 0), 26);
  const f = criar();
  const a = poe(f, 1, 'guardiao'), b = poe(f, 1, 'soldado'), c = poe(f, 1, 'caro');
  jogarEPassar(f, 0, 'pacto2');
  const p = f.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 1 && p.quantidade === 2);
  executar(f, { tipo: 'escolherCartas', jogador: 1, cartas: [a, b] });
  assert.deepEqual(f.jogadores[1].campo, [c]);
  assert.equal(custoProtecao(f, 0, a), 0);
});

test('Grau ao cemitério: o dono escolhe; com menos cartas, manda as que tiver', () => {
  const e = criar();
  const g = [poe(e, 0, 'nada', 'grau'), poe(e, 0, 'nada', 'grau'), poe(e, 0, 'nada', 'grau')];
  jogarEPassar(e, 0, 'raizes');
  const p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 0 && p.quantidade === 2);
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [g[0], g[2]] });
  assert.deepEqual(e.jogadores[0].grau, [g[1]]);
  jogarEPassar(e, 0, 'raizes');
  assert.equal(e.pendencia, null);
  assert.equal(e.jogadores[0].grau.length, 0);
});

test('descarte secundário: quem jogou descarta e escolhe', () => {
  const e = criar();
  jogarEPassar(e, 0, 'custo-mao');
  const p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 0 && p.destino === 'cemiterio' && p.quantidade === 1);
});

test('bot: não paga efeito secundário que mataria o Conjurador; escolhe as unidades menos valiosas para sacrificar', () => {
  const e = criar();
  const z = e.jogadores[0];
  z.mao.splice(0).forEach((id) => z.cemiterio.push(id));
  const fatal = poe(e, 0, 'fatal', 'mao');
  const est = { nome: 'teste', agressividade: 0, reservaInstantes: false, grauAlvo: 0, queimaFace: false };
  for (let i = 0; i < 5; i++) { const cmd = decidir(e, 0, est); assert.ok(!(cmd.tipo === 'jogar' && cmd.carta === fatal)); if (cmd.tipo === 'passar') break; executar(e, cmd); }
  const f = criar();
  poe(f, 1, 'caro'); const s1 = poe(f, 1, 'soldado'), s2 = poe(f, 1, 'morto');
  jogarEPassar(f, 0, 'pacto2');
  const cmd = decidir(f, 1, est);
  assert.ok(cmd.tipo === 'escolherCartas');
  assert.deepEqual(new Set(cmd.cartas), new Set([s1, s2]));
});

// ---------- 3. Reanimar estendido ----------
test('reanimar pagando o custo: escolhe ao resolver, paga na hora (reserva e Grau); sem poder pagar, não traz', () => {
  const e = criar();
  const barato = poe(e, 1, 'soldado', 'cemiterio'), caro = poe(e, 0, 'caro', 'cemiterio');
  e.jogadores[0].mao.splice(0).forEach((id) => e.jogadores[0].baralho.push(id)); // sem cartas na mão para enviar ao Arcano
  poe(e, 0, 'nada', 'grau');
  e.jogadores[0].arcano = 1;
  assert.equal(arcanoNaHora(e, 0), 2);
  jogarEPassar(e, 0, 'volta-custo');
  const p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 0);
  assert.deepEqual(p.cartas, [barato], 'o de custo 5 não dá para pagar');
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [barato] });
  assert.ok(e.jogadores[0].campo.includes(barato));
  assert.equal(arcanoNaHora(e, 0), 0, 'pagou 2');
  assert.ok(e.jogadores[0].cemiterio.includes(caro));
  jogarEPassar(e, 0, 'volta-custo');
  assert.equal(e.pendencia, null, 'sem dinheiro: nada acontece');
  assert.ok(e.jogadores[0].cemiterio.includes(caro));
  assert.equal(descEfeito({ tipo: 'reanimar', custoExtra: 0, pagamento: 'custo' }), 'ponha no seu campo um Aliado de qualquer cemitério, pagando o custo dele');
});

test('reanimar: sem custo do seu cemitério; custo + X do cemitério do oponente com concessão; filtros de custo e subtipo', () => {
  const e = criar();
  const meu = poe(e, 0, 'caro', 'cemiterio'), dele = poe(e, 1, 'soldado', 'cemiterio');
  jogarEPassar(e, 0, 'volta-gratis');
  let p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas');
  assert.deepEqual(p.cartas, [meu]);
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [meu] });
  assert.ok(e.jogadores[0].campo.includes(meu));
  e.jogadores[0].arcano = 4;
  jogarEPassar(e, 0, 'volta-mais');
  p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas');
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [dele] });
  assert.ok(e.jogadores[0].campo.includes(dele));
  assert.equal(e.instancias[dele].dono, 0);
  assert.equal(e.jogadores[0].arcano, 0, 'custo 2 + 2');
  assert.deepEqual(e.instancias[dele].palavrasExtra, ['Etéreo']);
  const f = criar();
  poe(f, 0, 'soldado', 'cemiterio'); poe(f, 1, 'caro', 'cemiterio');
  const m = poe(f, 1, 'morto', 'cemiterio');
  jogarEPassar(f, 0, 'volta-filtro');
  p = f.pendencia;
  assert.ok(p?.tipo === 'escolherCartas');
  assert.deepEqual(p.cartas, [m]);
  assert.equal(descEfeito(cartas.find((c) => c.id === 'volta-filtro')!.habilidades![0].efeitos[0]), 'ponha no seu campo um Aliado morto-vivo de custo até 3 de qualquer cemitério, sem pagar');
  assert.match(descEfeito(cartas.find((c) => c.id === 'volta-mais')!.habilidades![0].efeitos[0]), /do cemitério do oponente, pagando o custo dele \+ 2; ele ganha Etéreo/);
});

test('reanimar: carta antiga (Cemitério que Respira) continua sem cobrar o Aliado; texto diz "sem pagar"', () => {
  const c = cartasColecao.find((x) => x.id === 'col-cemiterio-que-respira')!;
  assert.match(textoCarta(c), /de qualquer cemitério, sem pagar\.$/);
});

test('reanimar num gatilho (sem alvo na jogada) e escolha do bot: o Aliado mais valioso que dá para pagar', () => {
  const e = criar({ habilidades: [{ quando: 'aliadoMorre', efeitos: [{ tipo: 'reanimar', custoExtra: 0, pagamento: 'gratis' }] }] });
  const s = poe(e, 0, 'soldado');
  const caro = poe(e, 1, 'caro', 'cemiterio');
  jogarEPassar(e, 0, 'pacto'); // sacrifica o único: dispara o gatilho
  for (let k = 0; k < 6 && e.pilha.length && !e.pendencia; k++) executar(e, { tipo: 'passar', jogador: e.prioridade });
  const p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.cartas.includes(s) && p.cartas.includes(caro));
  const cmd = decidir(e, 0, { nome: 'teste', agressividade: 0, reservaInstantes: false, grauAlvo: 0, queimaFace: false });
  assert.deepEqual(cmd, { tipo: 'escolherCartas', jogador: 0, cartas: [caro] });
});

// ---------- 4. Gatilho no Cataclisma do turno do oponente ----------
test('gatilho "no Cataclisma do turno do oponente": dispara no fim do turno do adversário, não no seu', () => {
  const e = criar({ habilidades: [{ quando: 'cataclismaOponente', efeitos: [{ tipo: 'arcano', valor: 2 }] }] });
  poe(e, 1, 'vigia');
  const mao1 = e.jogadores[1].mao.length;
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 }); // principal → cataclisma
  assert.equal(e.etapa, 'cataclisma');
  assert.equal(e.pilha.length, 2, 'o vigia e o Conjurador do jogador 1; o Conjurador do jogador 0 está no próprio turno');
  while (e.pilha.length) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.jogadores[1].mao.length, mao1 + 1);
  // turno do jogador 1: agora o Conjurador do jogador 0 dispara, o vigia não
  while (!(e.ativo === 1 && e.etapa === 'cataclisma')) executar(e, e.pendencia?.tipo === 'descartar' ? { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) } : { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.pilha.length, 1);
  while (e.pilha.length) executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.jogadores[0].arcano, 2);
  const h: Habilidade = { quando: 'cataclismaOponente', efeitos: [{ tipo: 'comprar', valor: 1 }] };
  assert.equal(textoCarta({ ...base, id: 'x', nome: 'x', tipo: 'mistica', habilidades: [h] } as CartaDef), 'No Cataclisma do turno do oponente: compre 1 carta.');
});

// ---------- 7. Tirar da mão (descartar ou banir) ----------
test('tirar da mão ao acaso: sem escolha; formato antigo (aleatorio) continua igual; banir vai para o banimento', () => {
  const e = criar();
  const m1 = e.jogadores[1].mao.length;
  jogarEPassar(e, 0, 'acaso');
  assert.equal(e.pendencia, null);
  assert.equal(e.jogadores[1].mao.length, m1 - 2);
  assert.equal(e.jogadores[1].cemiterio.length, 2);
  jogarEPassar(e, 0, 'acaso-velho');
  assert.equal(e.jogadores[1].cemiterio.length, 3);
  assert.equal(descEfeito({ tipo: 'descartar', quem: 'oponente', valor: 1, modo: 'acaso' }), 'o oponente descarta 1 carta ao acaso');
  assert.equal(descEfeito({ tipo: 'descartar', quem: 'oponente', valor: 1, acao: 'banir', modo: 'olhando' }), 'olhe a mão do oponente e bana 1 carta dela');
  assert.equal(descEfeito({ tipo: 'descartar', quem: 'ambos', valor: 2, acao: 'banir', modo: 'acaso' }), 'cada jogador bane 2 cartas da própria mão ao acaso');
  assert.equal(descEfeito({ tipo: 'descartar', quem: 'oponente', valor: 1 }), 'o oponente descarta 1 carta');
});

test('tirar da mão: o dono escolhe (sem mostrar) / quem jogou olha e escolhe / os dois', () => {
  const e = criar();
  jogarEPassar(e, 0, 'exilio');
  let p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 1 && p.destino === 'banimento');
  assert.deepEqual(p.cartas, e.jogadores[1].mao);
  const x = p.cartas[0];
  executar(e, { tipo: 'escolherCartas', jogador: 1, cartas: [x] });
  assert.ok(e.jogadores[1].banimento.includes(x));
  jogarEPassar(e, 0, 'olhar');
  p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 0, 'quem jogou escolhe');
  assert.deepEqual(p.cartas, e.jogadores[1].mao);
  assert.throws(() => executar(e, { tipo: 'escolherCartas', jogador: 1, cartas: [p.cartas[0]] }), /Outro jogador/);
  const y = p.cartas[1];
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [y] });
  assert.ok(e.jogadores[1].banimento.includes(y));
  jogarEPassar(e, 0, 'todos');
  p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 0, 'primeiro você');
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [p.cartas[0]] });
  p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.jogador === 1, 'depois o oponente');
});

// ---------- Respostas do autor (04/10, noite) ----------
test('autor: só Aliado se sacrifica (Mística e Estrutura não entram na escolha nem vão sozinhas)', () => {
  const e = criar();
  const alt = poe(e, 0, 'altar'), tor = poe(e, 0, 'torre');
  jogarEPassar(e, 0, 'pacto');
  assert.equal(e.pendencia, null, 'sem Aliado: nada a sacrificar');
  assert.deepEqual(e.jogadores[0].campo, [alt, tor]);
  const s = poe(e, 0, 'soldado');
  jogarEPassar(e, 0, 'pacto');
  assert.equal(e.pendencia, null, 'só um Aliado: vai sem escolha');
  assert.ok(e.jogadores[0].cemiterio.includes(s));
  assert.deepEqual(e.jogadores[0].campo, [alt, tor]);
});

test('autor: dois Protegidos na mesma carta valem juntos (cada um cobrado por si, sem virar o maior)', () => {
  const e = criar({ aliadosProtegidos: 1 });
  const g = poe(e, 1, 'guardiao'); // Protegido 3 da carta + 1 do Conjurador do dono
  e.instancias[g].palavrasExtra = ['Protegido 2'];
  assert.deepEqual(protegidosDe(e, g), [{ fonte: 'carta', arcano: 3 }, { fonte: 'conjurador', arcano: 1 }, { fonte: 'concedido', arcano: 2 }]);
  assert.equal(custoProtecao(e, 0, g), 6);
  const s = poe(e, 1, 'soldado');
  e.instancias[s].palavrasExtra = ['Protegido 1'];
  e.instancias[s].palavrasTemp = [{ palavras: ['Protegido 2'], restantes: 1 }];
  assert.equal(custoProtecao(e, 0, s), 1 + 1 + 2, 'Conjurador + concedido fixo + concedido por turnos');
  // o cofre (Protegido 1 + 2 de Vida + 1 do Grau) com o Protegido 1 do Conjurador: paga os dois juntos, na mesma jogada
  const cofre = poe(e, 1, 'cofre');
  poe(e, 0, 'nada', 'grau');
  e.jogadores[0].arcano = 1;
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: poe(e, 0, 'raio', 'mao'), alvo: cofre, enviarArcano: [] }), /Arcano/);
  e.jogadores[0].arcano = 2;
  jogarEPassar(e, 0, 'raio', cofre);
  assert.equal(e.jogadores[0].arcano, 0, 'pagou 1 do cofre + 1 do Conjurador');
  assert.equal(vidaConj(e, 0), 28, 'e a Vida do cofre');
});

test('autor: o custo do reanimar também pode ser pago enviando cartas da mão ao Arcano (mesmo caminho do motor)', () => {
  const e = criar();
  const z = e.jogadores[0];
  z.mao.splice(0).forEach((id) => z.baralho.push(id));
  const caro = poe(e, 1, 'caro', 'cemiterio');
  const m = [poe(e, 0, 'nada', 'mao'), poe(e, 0, 'nada', 'mao'), poe(e, 0, 'nada', 'mao'), poe(e, 0, 'nada', 'mao')];
  poe(e, 0, 'nada', 'grau');
  z.arcano = 1;
  jogarEPassar(e, 0, 'volta-custo');
  const p = e.pendencia;
  assert.ok(p?.tipo === 'escolherCartas' && p.cartas.includes(caro), 'custo 5: 1 de reserva + 1 do Grau + até 4 da mão');
  assert.throws(() => executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [caro], enviarArcano: m.slice(0, 2) }), /insuficiente/);
  assert.throws(() => executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [caro], enviarArcano: m }), /já está coberto/);
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [caro], enviarArcano: m.slice(0, 3) });
  assert.ok(z.campo.includes(caro));
  assert.equal(z.arcano, 0);
  assert.deepEqual(z.zonaArcano, m.slice(0, 3));
  assert.deepEqual(z.mao, [m[3]]);
  // sem enviar cartas e sem dinheiro: não traz nada (sem erro, como a ação automática do servidor)
  const f = criar();
  const c2 = poe(f, 1, 'caro', 'cemiterio');
  jogarEPassar(f, 0, 'volta-custo');
  assert.equal(f.pendencia?.tipo, 'escolherCartas');
  executar(f, { tipo: 'escolherCartas', jogador: 0, cartas: [c2] });
  assert.ok(f.jogadores[1].cemiterio.includes(c2));
  // envio fora de uma escolha com custo é recusado
  const g = criar();
  jogarEPassar(g, 0, 'exilio');
  const pg = g.pendencia!;
  assert.ok(pg.tipo === 'escolherCartas');
  assert.throws(() => executar(g, { tipo: 'escolherCartas', jogador: 1, cartas: [pg.cartas[0]], enviarArcano: [g.jogadores[1].mao[1]] }), /não tem custo/);
});

test('autor: o bot paga o reanimar com cartas da mão quando só assim dá', () => {
  const e = criar();
  const z = e.jogadores[0];
  z.mao.splice(0).forEach((id) => z.baralho.push(id));
  const caro = poe(e, 1, 'caro', 'cemiterio');
  for (let i = 0; i < 5; i++) poe(e, 0, 'nada', 'mao');
  jogarEPassar(e, 0, 'volta-custo');
  const cmd = decidir(e, 0, { nome: 'teste', agressividade: 0, reservaInstantes: false, grauAlvo: 0, queimaFace: false });
  assert.ok(cmd.tipo === 'escolherCartas' && cmd.enviarArcano?.length === 5);
  executar(e, cmd);
  assert.ok(z.campo.includes(caro));
});
