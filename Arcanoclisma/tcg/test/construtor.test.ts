// Construtor de efeitos (pedido do autor, 04/10): condições (ao jogar, entrar, virar/pagar, virar alvo, ir para o Grau,
// atacar, defender, cair no cemitério, ser banida) + efeitos que se acumulam, proteção mágica e o Etéreo novo.
// Cada teste monta cartas como o Editor monta (campo `habilidades`) e confere que o motor faz o que o texto diz.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import { textoCarta } from '../app/textos.js';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Habilidade, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const aliado = (id: string, ataque: number, vida: number, habilidades: Habilidade[] = [], extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'aliado', ataque, vida, palavrasChave: ['Proficiente'], habilidades, ...extra });
const temp = (id: string, habilidades: Habilidade[], extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', velocidade: 'repentina', habilidades, ...extra });

const cartas: CartaDef[] = [
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  aliado('soldado', 2, 3),
  aliado('gigante', 6, 10),
  aliado('fantasma', 2, 2, [], { palavrasChave: ['Proficiente', 'Etéreo'] }),
  // ao entrar: olha 3, põe 1 no Grau e o resto no cemitério; DEPOIS compra 1 (efeitos acumulados esperando a escolha)
  aliado('vidente', 1, 2, [{ quando: 'entrar', efeitos: [{ tipo: 'olharTopoPara', quantidade: 3, pegar: 1, destino: 'grau', resto: 'cemiterio' }, { tipo: 'comprar', valor: 1 }] }]),
  // ao atacar: 1 de dano em cada Aliado inimigo e ganha 1 de Arcano
  aliado('furia', 3, 5, [{ quando: 'atacar', efeitos: [{ tipo: 'campo', acao: 'dano', valor: 1, alcance: 'todos', lado: 'inimigo', tipos: ['aliado'] }, { tipo: 'arcano', valor: 1 }] }]),
  // virar e pagar 1 (Grau 1): olha a mão do oponente e manda 1 carta ao cemitério
  aliado('espia', 1, 3, [{ quando: 'ativada', custo: { suspender: true, arcano: 1, grau: 1 }, efeitos: [{ tipo: 'olharMao', pegar: 1, destino: 'cemiterio' }] }]),
  // cai no cemitério: põe em campo um Aliado de custo até 2 do cemitério
  aliado('fenix', 1, 1, [{ quando: 'cemiterio', efeitos: [{ tipo: 'jogarDe', de: 'cemiterio', valor: 1, tipos: ['aliado'], custoMax: 2 }] }]),
  // vira alvo: ganha +3 de Vida para sempre (uma vez por turno)
  aliado('casca', 1, 2, [{ quando: 'alvo', umaVezPorTurno: true, efeitos: [{ tipo: 'campo', acao: 'aumentarVida', valor: 3, alcance: 'alvo', lado: 'proprio', tipos: ['aliado'], duracao: 'permanente' }] }]),
  // ao ir para o Grau: ganha 2 de Arcano
  aliado('semente', 1, 1, [{ quando: 'grau', efeitos: [{ tipo: 'arcano', valor: 2 }] }]),
  // ao defender: cura 2 do seu Conjurador e o atacante não ataca no próximo turno
  aliado('guarda', 1, 6, [{ quando: 'bloquear', efeitos: [{ tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }] }]),
  // banida: compra 2
  aliado('eco', 1, 1, [{ quando: 'banimento', efeitos: [{ tipo: 'comprar', valor: 2 }] }]),
  // proteção mágica: Vida 2, descarta 1, Grau 1 mínimo
  aliado('reliquia', 0, 4, [], { protecao: { vida: 2, descartar: 1, grauMin: 1 } }),
  aliado('reliquia2', 0, 4, [], { protecao: { arcano: 1, grauParaCemiterio: 1 } }),
  // Temporárias montadas no construtor ("ao jogar")
  temp('raio', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'dano', valor: 3, alcance: 'alvo', lado: 'inimigo', tipos: ['aliado', 'conjurador'] }] }]),
  temp('saque', [{ quando: 'jogar', efeitos: [{ tipo: 'descartar', quem: 'oponente', valor: 1 }, { tipo: 'comprar', valor: 1 }, { tipo: 'perderVida', valor: 1 }] }]),
  temp('raizes', [{ quando: 'jogar', efeitos: [{ tipo: 'colocarNoGrau', de: 'topo', valor: 1 }, { tipo: 'colocarNoGrau', de: 'mao', valor: 1 }] }]),
  temp('exilio', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'paraBaralho', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'], onde: 'fundo' }] }]),
  temp('corrente', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'naoAtaca', alcance: 'todos', lado: 'inimigo', tipos: ['aliado'] }, { tipo: 'campo', acao: 'suspender', alcance: 'todos', lado: 'inimigo', tipos: ['aliado'] }] }]),
  temp('furor', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'aumentarAtaque', valor: 2, alcance: 'todos', lado: 'proprio', tipos: ['aliado'] }] }]),
  temp('quebra', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'destruir', alcance: 'todos', lado: 'inimigo', tipos: ['equipamento'] }] }]),
  temp('chamado', [{ quando: 'jogar', efeitos: [{ tipo: 'jogarDe', de: 'mao', valor: 1, tipos: ['aliado'] }] }]),
  temp('meteoro', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'dano', valor: 5, alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }] }], { palavrasChave: ['Incomensurável'] }),
  temp('banir', [{ quando: 'jogar', efeitos: [{ tipo: 'campo', acao: 'dano', valor: 0, alcance: 'alvo', lado: 'proprio', tipos: ['aliado'] }] }]),
  { ...base, id: 'arma-eterea', nome: 'Lâmina Etérea', tipo: 'item', categoriaItem: 'canal', tipoCanal: 'arma', ataque: 2, palavrasChave: ['Etéreo'] },
  { ...base, id: 'escudo', nome: 'Escudo', tipo: 'item', categoriaItem: 'armadura', defesa: 1, vidaDesgaste: 4 },
];

function criar(itens: [string[], string[]] = [[], []]): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = (it: string[]) => ({ conjurador: 'c', cartas: Array.from({ length: 30 }, () => 'nada'), itens: it });
  const e = criarJogo({ catalogo: cat, baralhos: [deck(itens[0]), deck(itens[1])], semente: 11, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
let n = 0;
function poe(e: Estado, j: Jogador, defId: string, onde: 'campo' | 'mao' | 'grau' | 'cemiterio' | 'baralho' = 'campo'): string {
  const id = `k${n++}`;
  e.instancias[id] = { id, defId, ehConjurador: false, dono: j, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  if (onde === 'baralho') e.jogadores[j].baralho.unshift(id); else e.jogadores[j][onde].push(id);
  return id;
}
/** Passa até a pilha esvaziar (respondendo pendências simples: escolhe as primeiras cartas). */
function resolver(e: Estado): void {
  let k = 0;
  while ((e.pilha.length || e.pendencia || (e.combate && e.combate.etapa !== 'aguardandoBloqueio')) && k++ < 100) {
    const p = e.pendencia;
    if (p?.tipo === 'escolherCartas') executar(e, { tipo: 'escolherCartas', jogador: p.jogador, cartas: p.cartas.slice(0, p.quantidade) });
    else if (p?.tipo === 'bloqueio') executar(e, { tipo: 'bloquear', jogador: p.jogador, bloqueador: null });
    else if (p) break;
    else executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
}
const jogar = (e: Estado, j: Jogador, id: string, alvo?: string) => { executar(e, { tipo: 'jogar', jogador: j, carta: id, alvo }); };

test('ao entrar: olhar 3 → 1 no Grau, resto no cemitério, e depois compra 1 (efeitos acumulados)', () => {
  const e = criar();
  const v = poe(e, 0, 'vidente', 'mao');
  const z = e.jogadores[0];
  jogar(e, 0, v);
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 }); // entra
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 }); // gatilho resolve → escolha
  assert.equal(e.pendencia?.tipo, 'escolherCartas');
  const mao = z.mao.length;
  const p = e.pendencia as Extract<NonNullable<Estado['pendencia']>, { tipo: 'escolherCartas' }>;
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [p.cartas[0]] });
  assert.equal(z.grau.length, 1);
  assert.equal(z.cemiterio.length, 2);
  assert.equal(z.mao.length, mao + 1); // a compra veio depois da escolha
});

test('ao atacar: dano em todos os Aliados inimigos e Arcano; ao defender: cura', () => {
  const e = criar();
  const f = poe(e, 0, 'furia');
  const s1 = poe(e, 1, 'soldado'), s2 = poe(e, 1, 'soldado');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: f, alvo: e.jogadores[1].conjuradorId });
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 }); // gatilho do ataque resolve primeiro
  assert.equal(e.instancias[s1].dano, 1); assert.equal(e.instancias[s2].dano, 1);
  assert.equal(e.jogadores[0].arcano, 1);
  // defender
  const g = criar();
  const atk = poe(g, 0, 'gigante');
  const guarda = poe(g, 1, 'guarda');
  g.instancias[g.jogadores[1].conjuradorId].dano = 5;
  executar(g, { tipo: 'atacar', jogador: 0, atacante: atk, alvo: g.jogadores[1].conjuradorId });
  executar(g, { tipo: 'passar', jogador: 0 }); executar(g, { tipo: 'passar', jogador: 1 });
  executar(g, { tipo: 'bloquear', jogador: 1, bloqueador: guarda });
  executar(g, { tipo: 'passar', jogador: 0 }); executar(g, { tipo: 'passar', jogador: 1 }); // gatilho de defesa
  assert.equal(g.instancias[g.jogadores[1].conjuradorId].dano, 3);
});

test('virar e pagar (com Grau mínimo): olha a mão do oponente e descarta 1 dela', () => {
  const e = criar();
  const esp = poe(e, 0, 'espia');
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: esp, habilidade: 0 }), /Grau 1/);
  poe(e, 0, 'nada', 'grau');
  e.jogadores[0].arcano = 1;
  const mao1 = e.jogadores[1].mao.length;
  executar(e, { tipo: 'ativar', jogador: 0, carta: esp, habilidade: 0 });
  assert.equal(e.instancias[esp].suspensa, true);
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 });
  const p = e.pendencia as Extract<NonNullable<Estado['pendencia']>, { tipo: 'escolherCartas' }>;
  assert.equal(p.tipo, 'escolherCartas'); assert.equal(p.jogador, 0);
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [p.cartas[0]] });
  assert.equal(e.jogadores[1].mao.length, mao1 - 1);
  assert.equal(e.jogadores[1].cemiterio.length, 1);
});

test('cai no cemitério: a Fênix morre e põe em campo um Aliado barato do cemitério', () => {
  const e = criar();
  const fx = poe(e, 0, 'fenix');
  const sold = poe(e, 0, 'soldado', 'cemiterio');
  const r = poe(e, 1, 'raio', 'mao');
  executar(e, { tipo: 'passar', jogador: 0 }); // prioridade ao jogador 1
  jogar(e, 1, r, fx);
  resolver(e);
  assert.ok(e.jogadores[0].cemiterio.includes(fx));
  assert.ok(e.jogadores[0].campo.includes(sold) || e.jogadores[0].campo.includes(fx));
});

test('vira alvo: ganha +3 de Vida para sempre antes do dano resolver (o gatilho resolve primeiro)', () => {
  const e = criar();
  const c = poe(e, 0, 'casca');
  const r = poe(e, 1, 'raio', 'mao');
  executar(e, { tipo: 'passar', jogador: 0 });
  jogar(e, 1, r, c);
  resolver(e);
  assert.ok(e.jogadores[0].campo.includes(c)); // 2 + 3 de Vida aguentou os 3 de dano
  assert.equal(e.instancias[c].dano, 3);
});

test('ao ir para o Grau: gera 2 de Arcano', () => {
  const e = criar();
  const s = poe(e, 0, 'semente', 'mao');
  executar(e, { tipo: 'colocarGrau', jogador: 0, carta: s });
  resolver(e);
  assert.equal(e.jogadores[0].arcano, 2);
});

test('banida: compra 2', () => {
  const e = criar();
  const eco = poe(e, 0, 'eco', 'mao');
  // um efeito de banir a carta da mão (Rodada 7) leva o eco para o banimento
  e.catalogo.cartas['banidor'] = { ...base, id: 'banidor', nome: 'Banidor', tipo: 'temporaria', efeitos: [{ tipo: 'banirDaMao', desconto: 0 }] };
  const b = poe(e, 0, 'banidor', 'mao');
  const mao = e.jogadores[0].mao.length;
  jogar(e, 0, b, eco);
  resolver(e);
  assert.ok(e.jogadores[0].banimento.includes(eco));
  assert.equal(e.jogadores[0].mao.length, mao - 2 + 2);
});

test('acumulados: o oponente descarta 1 (escolhe), você compra 1 e perde 1 de Vida', () => {
  const e = criar();
  const s = poe(e, 0, 'saque', 'mao');
  const m0 = e.jogadores[0].mao.length, m1 = e.jogadores[1].mao.length;
  jogar(e, 0, s);
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 });
  assert.equal((e.pendencia as { jogador: number }).jogador, 1); // quem descarta escolhe
  resolver(e);
  assert.equal(e.jogadores[1].mao.length, m1 - 1);
  assert.equal(e.jogadores[0].mao.length, m0 - 1 + 1);
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].dano, 1);
});

test('pôr no Grau: do topo do baralho e da mão', () => {
  const e = criar();
  const r = poe(e, 0, 'raizes', 'mao');
  jogar(e, 0, r);
  resolver(e);
  assert.equal(e.jogadores[0].grau.length, 2);
});

test('campo: mandar para o fundo do baralho, proibir de atacar + suspender, aumentar Ataque no turno, destruir equipamento', () => {
  const e = criar([[], ['escudo']]);
  const alvo = poe(e, 1, 'soldado');
  jogar(e, 0, poe(e, 0, 'exilio', 'mao'), alvo);
  resolver(e);
  assert.equal(e.jogadores[1].baralho.at(-1), alvo);
  const s = poe(e, 1, 'soldado');
  jogar(e, 0, poe(e, 0, 'corrente', 'mao'));
  resolver(e);
  assert.equal(e.instancias[s].suspensa, true);
  assert.ok((e.instancias[s].naoAtacaAte ?? 0) >= e.turno + 1);
  const meu = poe(e, 0, 'soldado');
  jogar(e, 0, poe(e, 0, 'furor', 'mao'));
  resolver(e);
  assert.equal(e.instancias[meu].bonusAtaqueTurno, 2);
  jogar(e, 0, poe(e, 0, 'quebra', 'mao'));
  resolver(e);
  assert.equal(e.jogadores[1].itens.length, 0);
  // o bônus "neste turno" acaba no Final
  while (e.ativo === 0) executar(e, e.pendencia?.tipo === 'descartar' ? { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) } : { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.instancias[meu].bonusAtaqueTurno, 0);
  // e o Soldado preso não ataca no turno dele
  while (e.etapa !== 'principal') executar(e, e.pendencia?.tipo === 'descartar' ? { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) } : { tipo: 'passar', jogador: e.prioridade });
  e.instancias[s].suspensa = false; // mesmo de pé, continua proibido de atacar
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 1, atacante: s, alvo: e.jogadores[0].conjuradorId }), /não pode atacar/);
});

test('jogar carta de outra zona: põe um Aliado da mão em campo sem pagar', () => {
  const e = criar();
  const g = poe(e, 0, 'gigante', 'mao');
  jogar(e, 0, poe(e, 0, 'chamado', 'mao'));
  executar(e, { tipo: 'passar', jogador: 0 }); executar(e, { tipo: 'passar', jogador: 1 });
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [g] });
  assert.ok(e.jogadores[0].campo.includes(g));
});

test('proteção mágica: Grau mínimo, Vida e descarte; ou Arcano e Grau ao cemitério', () => {
  const e = criar();
  const rel = poe(e, 1, 'reliquia');
  const r = poe(e, 0, 'raio', 'mao');
  assert.throws(() => jogar(e, 0, r, rel), /Grau 1/);
  poe(e, 0, 'nada', 'grau');
  const mao = e.jogadores[0].mao.length;
  jogar(e, 0, r, rel);
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].dano, 2);
  assert.equal(e.jogadores[0].mao.length, mao - 2); // o Raio e o descarte
  const f = criar();
  const rel2 = poe(f, 1, 'reliquia2');
  const g = poe(f, 0, 'nada', 'grau');
  f.jogadores[0].arcano = 1;
  jogar(f, 0, poe(f, 0, 'raio', 'mao'), rel2);
  assert.ok(f.jogadores[0].cemiterio.includes(g));
  assert.equal(f.jogadores[0].arcano, 0);
});

test('Etéreo: só Etéreos e armas Etéreas atingem um Etéreo (atacar e bloquear)', () => {
  const e = criar();
  const fan = poe(e, 1, 'fantasma');
  const sold = poe(e, 0, 'soldado');
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 0, atacante: sold, alvo: fan }), /Etéreo/);
  const f = criar([['arma-eterea'], []]);
  const fan2 = poe(f, 1, 'fantasma');
  executar(f, { tipo: 'atacar', jogador: 0, atacante: f.jogadores[0].conjuradorId, alvo: fan2 });
  resolver(f);
  assert.ok(f.jogadores[1].cemiterio.includes(fan2)); // a Lâmina Etérea (2) derrubou o Fantasma (2)
  // o Conjurador com arma Etérea bloqueia um atacante Etéreo
  const g = criar([[], ['arma-eterea']]);
  const fan3 = poe(g, 0, 'fantasma');
  executar(g, { tipo: 'atacar', jogador: 0, atacante: fan3, alvo: g.jogadores[1].conjuradorId });
  executar(g, { tipo: 'passar', jogador: 0 }); executar(g, { tipo: 'passar', jogador: 1 });
  executar(g, { tipo: 'bloquear', jogador: 1, bloqueador: g.jogadores[1].conjuradorId });
});

test('texto gerado: cada condição e efeito vira uma frase', () => {
  for (const c of cartas) assert.ok(typeof textoCarta(c) === 'string');
  assert.match(textoCarta(cartas.find((c) => c.id === 'vidente')!), /Quando entra em campo: olhe as 3 cartas.*no Grau.*no cemitério e compre 1 carta/);
  assert.match(textoCarta(cartas.find((c) => c.id === 'reliquia')!), /Proteção mágica/);
});

test('Incomensurável numa magia: o dano que passa da Vida do Aliado vai para o Conjurador dele', () => {
  const e = criar();
  const s = poe(e, 1, 'soldado'); // Vida 3
  jogar(e, 0, poe(e, 0, 'meteoro', 'mao'), s);
  resolver(e);
  assert.ok(e.jogadores[1].cemiterio.includes(s));
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 2);
  const f = criar();
  const s2 = poe(f, 1, 'soldado');
  jogar(f, 0, poe(f, 0, 'raio', 'mao'), s2); // Raio (3) sem Incomensurável: nada passa
  resolver(f);
  assert.equal(f.instancias[f.jogadores[1].conjuradorId].dano, 0);
});
