// Variantes de regras e de cartas para testar uma mudança por vez e em combinação.
import { readFileSync } from 'node:fs';
import { bonusGrau, grauPadrao, orcamento, PRECO, precoAtivacao, precoEfeito, type Precos } from '../src/balanco.ts';
import { cartasBase } from '../src/cartas-base.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Efeito, Regras } from '../src/tipos.ts';
import { conjuradoresPorValor } from '../src/conjuradores.ts';
import { itensBasicos } from '../src/itens.ts';
import { arquetipos as arquetiposBase, type Arquetipo } from './baralhos.ts';
import { arquetiposConjuradores } from './conjuradores-decks.ts';
import { comValores } from './valores-cartas.ts';
import { aplicarBaralhos, aplicarCatalogo, lerEdicoes } from './edicoes.ts';
import type { Estilo } from './bot.ts';

export interface Variante {
  id: string;
  nome: string;
  // ----- rodada 2 -----
  burn: 'base' | 'custoGrau' | 'pagarX';
  pagarXDelta: number;
  recursos: boolean;
  eternyr: boolean;
  estruturas: boolean;
  cura: boolean;
  semAcaoPerde: boolean;
  primeiroSemGrau: boolean;
  conjAtaca: boolean;
  conjDano: number;
  conjCusto: number;
  /** Vida do Conjurador (decisão do autor: 20). */
  conjVida: number;
  grauSemSuspender: boolean;
  segundoCarta: number;
  segundoArcano: number;
  // ----- rodada 3 -----
  /** Geradores de Arcano de custo alto (Estruturas e Aliados), mais baratos por Arcano; entram nos baralhos de late game. */
  arcanoAlto: boolean;
  /** Permanentes de recurso contam como cartas no Grau (aceleram o late game e poupam cartas da mão). */
  permAcelera: boolean;
  /** Eternyr: dano forte em Aliados (custo médio/alto), dano ao Conjurador difícil, poderes de Grau alto. */
  eternyrForte: boolean;
  /** 0 = dano ao Conjurador como antes; 1 = 2,5 PP por ponto; 2 = 3 PP por ponto (mais cartas pequenas para a constância). */
  burnNerf: 0 | 1 | 2;
  /** Cura a 0,4 PP por ponto, com efeitos extras maiores. */
  cura2: boolean;
  /** Estruturas com 1 de Vida a menos, remoção que mira Estruturas e o Stall com menos opções. */
  staxFragil: boolean;
  /** Relógio de impasse (turnos); 0 = desligado. */
  impasse: number;
  primeiroConjNo: boolean;
  /** Jogador de deck lento: só sobrevive e joga para ganhar em Graus altos. */
  lentos: boolean;
  // ----- agente-balanceador (rodada 4) -----
  /** Varreduras: dano em todos os Aliados do oponente (Controle, Stall, Eternyr). */
  balVarredura: boolean;
  /** Remoção que cresce com o Grau (Controle, Eternyr, Stall). */
  balRemocaoGrau: boolean;
  /** Estrutura de cerco: dá dano ao Conjurador todo turno (condição de vitória do Stall). */
  balStallPinga: boolean;
  /** Devolver Aliado à mão, repentina e barata (Tempo). */
  balDevolver: boolean;
  /** Aliados Etéreos (evasão) no Tempo. */
  balEtereo: boolean;
  /** Curva do Aggro mais baixa (gasta a mão mais rápido). */
  balAggroCurva: boolean;
  /** Controle usa o baralho e o estilo achados pelo agente de testes (sim/baralho-controle-agente.json). */
  balControleAgente: boolean;
  /** Carta Anular (repentina) no Eternyr e no Controle. */
  balAnular: boolean;
  /** Estrutura de cura só cura Aliados (não o Conjurador). */
  balCuraSoAliados: boolean;
  /** Arquivo em sim/ com baralhos e estilos achados pelo agente de testes (campo cands), aplicados por cima. */
  balBaralhos: string;
  /** Preço por Arcano das ativações de geração cara (0 = usa o da variante). */
  balPrecoArcanoAlto: number;
  /** Quantas cartas do Grau os Aliados de descarte de Grau descartam a mais (0 = como estão). */
  balDescarteExtra: number;
  // ----- rodada 5 -----
  /** Jogador automático prevê o bloqueio do oponente antes de atacar (estratégia, não regra). */
  botPreve: boolean;
  /** Jogador automático sobe o Grau até poder jogar as cartas da mão (estratégia). */
  botGrau: boolean;
  /** Aliados Etéreos caros (finalizadores com evasão) nos baralhos de jogo longo. */
  balEtereoTardio: boolean;
  /** Jogador automático ataca Aliados e Místicas inimigas quando a troca compensa (estratégia). */
  botAtacaAliados: boolean;
  /** Rodada 7: 9 Conjuradores (par de Valores + habilidade), cartas com Valores e um baralho para cada Conjurador. */
  r7Conjuradores: boolean;
  /** Aplica as edições do autor (data/edicoes.json, feitas no editor do app): cartas, Conjuradores, regras, régua e baralhos. */
  edicoesAutor: boolean;
  /** Controle de remoção: muita remoção e compra, poucos finalizadores (vence em recurso, devagar). */
  balControleRemocao: boolean;
  // ----- rodada 6 -----
  /** Mais varreduras no Controle, com variações de custo e Grau (Onda de fagulhas, Ira, Pôr do sol). */
  r6Varreduras: boolean;
  /** Curva de poder por Grau nos Aliados: bônus de orçamento k × G × (G − 1) / 2 (0 = desligada). Ver src/balanco.ts. */
  r6CurvaGrau: number;
  /** Unidades de Grau médio que viram problema e bombas de Grau alto (Protegido, gatilho de dano ao Conjurador), distribuídas nos baralhos. */
  r6Cartas: boolean;
  /** Varredura simétrica barata (Fúria: 3 de dano em cada Aliado, os seus também) no lugar da Onda de fagulhas no Controle. */
  r6Furia: boolean;
  /** Jogador automático trata varreduras como remoção (não as põe no Grau nem as usa de pitch primeiro). Estratégia, não regra. */
  botVarredura: boolean;
  /** Stall (fortaleza, quase sem Aliados) também usa as varreduras novas: Ira, Fúria e Pôr do sol. */
  r6StallVarre: boolean;
  /** Arquivo em sim/ com baralhos e estilos achados pelo agente de testes na rodada 6, aplicados por cima de tudo (como balBaralhos). */
  r6BaralhosAgente: string;
  /** Devolve ao Aggro do agente de testes o papel de Aggro (troca as cartas lentas por cartas baratas de pressão). */
  r6AggroIdentidade: boolean;
  /** O Aggro ignora o arquivo do agente de testes e fica com a lista e o estilo do PADRAO (identidade aprovada). */
  r6AggroPadrao: boolean;
  /** Controle com estilo de Controle: segura os Aliados para bloquear (agressividade 0,3), como no PADRAO. */
  r6ControleEstilo: boolean;
  /** A curva de Grau não soma com o bônus antigo de Grau acima do padrão (usa no máximo o Grau padrão do custo). */
  r6CurvaSemDobro: boolean;
  /** Eternyr sem o Mercador e o Dragão (fica só com as próprias cartas de Grau alto). */
  r6EternyrSemBombas: boolean;
  /** Controle troca os 2 Aliados tardios do Eternyr pelo Mercador de segredos (finalizador que compra). */
  r6ControleMercador: boolean;
  /** Controle troca as 3 cartas de Aggro do baralho do agente por compra (Eternyr: comprar 2 por custo 2). */
  r6ControleCompra: boolean;
  /** Controle ignora o arquivo do agente e fica com o Controle de remoção (passo 7), mais a cura que segura o Aggro. */
  r6ControleRemocao: boolean;
  /** Segundo arquivo de baralhos do agente de testes na rodada 6 (objetivos-bal6b), aplicado por cima de tudo. */
  r6BaralhosAgente2: string;
  /** Controle troca as 3 cartas de dano ao Conjurador por Recurso básico (true) ou por Eternyr: comprar 3 ('compra'). */
  r6ControleSemQueima: boolean | 'compra';
  /** Controle troca 2 Aliados de ataque pela Estrutura de recurso de custo 3 (compra ao entrar e fica em campo). */
  r6ControleEstruturas: boolean;
  /** Controle troca as 2 Feras de cerco por 2 "Eternyr: comprar 3" (vantagem de cartas). */
  r6ControleCompra3: boolean | number;
  /** Agressividade do Controle quando r6ControleEstilo (0 = 0,3). */
  r6ControleAgressividade: number;
  /** Preço por ponto da varredura simétrica (0 = o da régua, 2,5). */
  r6PrecoTodosAmbos: number;
}

export const VARIANTE_BASE: Variante = {
  id: 'base', nome: 'Base', burn: 'base', pagarXDelta: -1, recursos: false, eternyr: false, estruturas: false, cura: false,
  semAcaoPerde: false, primeiroSemGrau: false, conjAtaca: false, conjDano: 1, conjCusto: 1, conjVida: 20, grauSemSuspender: false,
  segundoCarta: 1, segundoArcano: 1,
  arcanoAlto: false, permAcelera: false, eternyrForte: false, burnNerf: 0, cura2: false, staxFragil: false, impasse: 0, primeiroConjNo: false, lentos: false,
  balVarredura: false, balRemocaoGrau: false, balStallPinga: false, balDevolver: false, balEtereo: false, balAggroCurva: false, balControleAgente: false, balAnular: false, balCuraSoAliados: false, balBaralhos: '', balPrecoArcanoAlto: 0, balDescarteExtra: 0,
  botPreve: false, botGrau: false, balEtereoTardio: false, botAtacaAliados: false, r7Conjuradores: false, edicoesAutor: false, balControleRemocao: false,
  r6Varreduras: false, r6CurvaGrau: 0, r6Cartas: false, r6Furia: false, botVarredura: false, r6StallVarre: false, r6BaralhosAgente: '', r6AggroIdentidade: false, r6AggroPadrao: false, r6ControleEstilo: false, r6CurvaSemDobro: false, r6EternyrSemBombas: false, r6ControleMercador: false, r6ControleCompra: false, r6ControleRemocao: false, r6BaralhosAgente2: '', r6ControleAgressividade: 0, r6ControleSemQueima: false, r6ControleEstruturas: false, r6ControleCompra3: false, r6PrecoTodosAmbos: 0,
};

export function variante(id: string, nome: string, mudancas: Partial<Variante>): Variante {
  return { ...VARIANTE_BASE, id, nome, ...mudancas };
}

const comEstruturas = (v: Variante) => v.estruturas || v.permAcelera || v.arcanoAlto;
const comRecursos = (v: Variante) => v.recursos || v.permAcelera || v.arcanoAlto;

export function precosDe(v: Variante): Precos {
  return {
    ...PRECO,
    curar: v.cura2 ? 0.4 : v.cura ? 0.6 : PRECO.curar,
    comprarEternyr: v.eternyr ? 2.5 : PRECO.comprarEternyr,
    danoConjurador: v.burnNerf === 2 ? 3 : v.burnNerf === 1 ? 2.5 : PRECO.danoConjurador,
    ativacaoArcanoAlto: v.balPrecoArcanoAlto || (v.arcanoAlto ? 1 : PRECO.ativacaoArcanoAlto),
    danoAliadoEternyr: v.eternyrForte ? 1.5 : PRECO.danoAliadoEternyr,
    danoConjEternyr: v.eternyrForte ? 3 : PRECO.danoConjEternyr,
    grauAltoEternyr: v.eternyrForte ? 2 : PRECO.grauAltoEternyr,
    danoTodosAmbos: v.r6PrecoTodosAmbos || PRECO.danoTodosAmbos,
  };
}

export function regrasDe(v: Variante): Partial<Regras> {
  return { ...regrasGeradas(v), ...(v.edicoesAutor ? lerEdicoes().regras : {}) };
}

/** Regras da variante sem as edições do autor. */
export function regrasGeradas(v: Variante): Partial<Regras> {
  return {
    semAcaoPerde: v.semAcaoPerde, primeiroSemGrauNoTurno1: v.primeiroSemGrau, grauSemSuspender: v.grauSemSuspender,
    segundoCartaExtra: v.segundoCarta, segundoArcanoInicial: v.segundoArcano, primeiroConjSemAtacarTurno1: v.primeiroConjNo, impasseTurnos: v.impasse,
  };
}

export function conjuradorDe(v: Variante): ConjuradorDef {
  return {
    id: 'padrao', nome: 'Conjurador padrão', valores: [], comprometimento: 'Preparado', vida: v.conjVida, maoInicial: 5,
    compraFinal: 2, danoBasico: v.conjDano, custoAtaque: v.conjCusto, habilidade: '',
  };
}

const LENTOS = new Set(['Controle', 'Stall', 'Ramp', 'Eternyr']);

export function estiloDe(a: Arquetipo, v: Variante): Estilo {
  const est: Estilo = { ...a.estilo, conjAtaca: v.conjAtaca, alvejaEstruturas: v.staxFragil, preveBloqueio: v.botPreve, sobeGrau: v.botGrau, atacaAliados: v.botAtacaAliados, valorizaVarredura: v.botVarredura };
  if (v.lentos && LENTOS.has(a.nome)) return { ...est, lento: true, agressividade: 0.1, grauAlvo: 12, reservaInstantes: true, conjAtaca: false };
  return est;
}

// ---------- fábricas de cartas que dependem dos preços ----------
const molde = { subtipos: [], valores: [], arcanoGerado: 1, cenario: '', texto: '' } as const;
const cap = (n: number) => Math.min(20, n);

function curarHibrida(custo: number, p: Precos, parte: number): CartaDef {
  const o = orcamento(custo, 1, p);
  const cura = cap(Math.floor((parte * o) / p.curar));
  const resto = o - cura * p.curar;
  const efeitos: Efeito[] = [{ tipo: 'curar', valor: cura, alvo: 'escolhido' }];
  if (resto >= p.comprar) efeitos.push({ tipo: 'comprar', valor: Math.floor(resto / p.comprar) });
  else if (resto >= p.arcano) efeitos.push({ tipo: 'arcano', valor: 1 });
  const extra = efeitos.slice(1).map((ef) => (ef.tipo === 'comprar' ? `compre ${ef.valor}` : 'ganhe 1 Arcano')).join(' e ');
  return {
    ...molde, id: `base-curar-c${custo}`, nome: `Cura (custo ${custo})`, tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'findante',
    efeitos, texto: `Cure ${cura} de dano de um alvo${extra ? ' e ' + extra : ''}.`,
  };
}

function comprarEternyr(custo: number, repentina: boolean, p: Precos): CartaDef {
  const n = Math.max(1, Math.floor((orcamento(custo, 1, p) - (repentina ? p.repentina : 0)) / p.comprarEternyr));
  return {
    ...molde, id: `eternyr-comprar-c${custo}${repentina ? '-repentina' : ''}`, nome: `Eternyr: comprar (custo ${custo}${repentina ? ', repentina' : ''})`,
    valores: ['eternyr'], tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: repentina ? 'repentina' : 'findante',
    efeitos: [{ tipo: 'comprar', valor: n }], texto: `Compre ${n} carta${n > 1 ? 's' : ''}.`,
  };
}

function danoConjuradorCard(custo: number, p: Precos): CartaDef {
  const dano = Math.max(1, Math.floor(orcamento(custo, 1, p) / p.danoConjurador));
  return {
    ...molde, id: `base-dano-conjurador-c${custo}`, nome: `Dano ao Conjurador (custo ${custo})`, tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'findante',
    efeitos: [{ tipo: 'dano', valor: dano, alvo: 'conjuradorInimigo' }], texto: `Causa ${dano} de dano ao Conjurador inimigo.`,
  };
}

function eternyrDanoAliado(custo: number, p: Precos): CartaDef {
  const dano = cap(Math.floor(orcamento(custo, 1, p) / p.danoAliadoEternyr));
  return {
    ...molde, id: `eternyr-dano-aliado-c${custo}`, nome: `Eternyr: dano em Aliado (custo ${custo})`, valores: ['eternyr'], tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'findante',
    efeitos: [{ tipo: 'dano', valor: dano, alvo: 'escolhidoAliado' }], texto: `Causa ${dano} de dano a um Aliado ou Mística.`,
  };
}

function eternyrDanoJogador(custo: number, p: Precos): CartaDef {
  const dano = Math.max(1, Math.floor(orcamento(custo, 1, p) / p.danoConjEternyr));
  return {
    ...molde, id: `eternyr-dano-jogador-c${custo}`, nome: `Eternyr: dano no Conjurador (custo ${custo})`, valores: ['eternyr'], tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'findante',
    efeitos: [{ tipo: 'dano', valor: dano, alvo: 'conjuradorInimigo' }], texto: `Causa ${dano} de dano ao Conjurador inimigo.`,
  };
}

/** Poder tardio do Eternyr: Grau bem acima do padrão paga mais PP (preço de Grau alto do Eternyr). */
function eternyrAliadoTardio(custo: number, grau: number, p: Precos): CartaDef {
  const dif = grau - grauPadrao(custo);
  const corpo = orcamento(custo, 1, p) + dif * p.grauAltoEternyr;
  const vida = Math.min(20, Math.ceil(corpo * 0.6));
  return {
    ...molde, id: `eternyr-aliado-c${custo}-g${grau}`, nome: `Eternyr: Aliado tardio (custo ${custo}, Grau ${grau})`, valores: ['eternyr'], tipo: 'aliado', custo, grau, velocidade: 'findante',
    ataque: Math.min(20, Math.max(0, Math.floor(corpo - vida))), vida,
  };
}

function eternyrDestruirTardio(custo: number, grau: number, p: Precos): CartaDef {
  const dif = grau - grauPadrao(custo);
  const folga = orcamento(custo, 1, p) + dif * p.grauAltoEternyr - p.destruir;
  const n = Math.max(1, Math.floor(folga / p.comprarEternyr));
  return {
    ...molde, id: `eternyr-destruir-c${custo}-g${grau}`, nome: `Eternyr: destruir e comprar (custo ${custo}, Grau ${grau})`, valores: ['eternyr'], tipo: 'temporaria', custo, grau, velocidade: 'findante',
    efeitos: [{ tipo: 'destruir', alvo: 'escolhido' }, { tipo: 'comprar', valor: n }], texto: `Destrua um alvo e compre ${n} carta${n > 1 ? 's' : ''}.`,
  };
}

function estruturaAtiva(id: string, nome: string, custo: number, ativ: { custo: number; efeitos: Efeito[] }, p: Precos, grauExtra = 0, fragil = false): CartaDef {
  const carta: CartaDef = {
    ...molde, id, nome, tipo: 'estrutura', custo, grau: grauPadrao(custo), velocidade: 'findante', vida: 1,
    ativacao: { custo: ativ.custo, suspende: true, efeitos: ativ.efeitos }, ...(grauExtra ? { grauExtra } : {}),
  };
  const preco = precoAtivacao(carta, p) + p.grauExtra * grauExtra;
  carta.vida = Math.max(1, Math.floor(orcamento(custo, 1, p) - preco) - (fragil ? 1 : 0));
  const textoEf = ativ.efeitos.map((ef) => (ef.tipo === 'arcano' ? `ganhe ${ef.valor} Arcano` : ef.tipo === 'comprar' ? `compre ${ef.valor}` : ef.tipo === 'curar' ? `cure ${ef.valor} de um alvo` : ef.tipo === 'dano' ? `cause ${ef.valor} de dano ${ef.alvo === 'conjuradorInimigo' ? 'ao Conjurador inimigo' : 'a um alvo'}` : '')).join(' e ');
  carta.texto = `${grauExtra ? `Conta como ${grauExtra} carta${grauExtra > 1 ? 's' : ''} no Grau. ` : ''}Suspenda${ativ.custo ? ` e pague ${ativ.custo}` : ''}: ${textoEf}.`;
  return carta;
}

function aliadoRecurso(custo: number, arcano: number, p: Precos, grauExtra = 0): CartaDef {
  const carta: CartaDef = {
    ...molde, id: `base-aliado-recurso-c${custo}`, nome: `Aliado de recurso (custo ${custo})`, tipo: 'aliado', custo, grau: grauPadrao(custo), velocidade: 'findante',
    ataque: 0, vida: 1, ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: arcano }] }, ...(grauExtra ? { grauExtra } : {}),
  };
  const corpo = Math.max(1, orcamento(custo, 1, p) - precoAtivacao(carta, p) - p.grauExtra * grauExtra);
  const vida = Math.max(1, Math.ceil(corpo * 0.6));
  carta.vida = Math.min(20, vida);
  carta.ataque = Math.min(20, Math.max(0, corpo - vida));
  carta.texto = `${grauExtra ? `Conta como ${grauExtra} carta${grauExtra > 1 ? 's' : ''} no Grau. ` : ''}Suspenda: ganhe ${arcano} Arcano.`;
  return carta;
}

function varredura(custo: number, p: Precos): CartaDef {
  const x = Math.max(1, Math.floor(orcamento(custo, 1, p) / p.danoTodos));
  return {
    ...molde, id: `bal-varredura-c${custo}`, nome: `Varredura (custo ${custo})`, tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'findante',
    efeitos: [{ tipo: 'danoTodosAliados', valor: x }], texto: `Causa ${x} de dano a cada Aliado do oponente.`,
  };
}

function remocaoGrau(custo: number, p: Precos): CartaDef {
  const base = Math.max(0, Math.floor(orcamento(custo, 1, p) / p.danoAlvo) - p.grauMedioEscala);
  return {
    ...molde, id: `bal-remocao-grau-c${custo}`, nome: `Remoção que cresce (custo ${custo})`, tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'findante',
    efeitos: [{ tipo: 'dano', valor: base, alvo: 'escolhidoAliado', escalaGrau: true }], texto: `Causa ${base} + o número de cartas no seu Grau de dano a um Aliado ou Mística.`,
  };
}

function aliadoEtereo(custo: number, p: Precos): CartaDef {
  const corpo = Math.max(2, orcamento(custo, 1, p) - p.etereo);
  const vida = Math.max(1, Math.ceil(corpo / 2));
  return {
    ...molde, id: `bal-etereo-c${custo}`, nome: `Aliado Etéreo (custo ${custo})`, tipo: 'aliado', custo, grau: grauPadrao(custo), velocidade: 'findante',
    palavrasChave: ['Etéreo'], ataque: corpo - vida, vida, texto: 'Etéreo (só é bloqueado por Aliados Etéreos).',
  };
}

function devolverCarta(custo: number, p: Precos): CartaDef {
  void p;
  return {
    ...molde, id: `bal-devolver-c${custo}-repentina`, nome: `Devolver (custo ${custo}, repentina)`, tipo: 'temporaria', custo, grau: grauPadrao(custo), velocidade: 'repentina',
    efeitos: [{ tipo: 'devolver', alvo: 'escolhidoAliado' }], texto: 'Devolva um Aliado ou Mística à mão do dono.',
  };
}

// ---------- rodada 6: varreduras, unidades-problema e bombas ----------
const temporariaR6 = (id: string, nome: string, custo: number, grau: number, efeitos: Efeito[], texto: string): CartaDef => ({
  ...molde, id, nome, tipo: 'temporaria', custo, grau, velocidade: 'findante', efeitos, texto,
});

interface UnidadeR6 {
  id: string; nome: string; custo: number; grau: number; fracVida: number;
  etereo?: boolean; protegido?: number; aoEntrar?: Efeito[]; aoDano?: Efeito[]; texto: string;
}

/** Aliado da rodada 6: orçamento da régua + curva de Grau + Grau acima do padrão; o que sobra das habilidades vira Ataque e Vida. */
function unidadeR6(u: UnidadeR6, p: Precos, k: number): CartaDef {
  const orc = orcamento(u.custo, 1, p) + bonusGrau(u.grau, k) + (u.grau - grauPadrao(u.custo));
  const soma = (efs: Efeito[] = []) => efs.reduce((s, ef) => s + precoEfeito(ef, p), 0);
  const gasto = (u.etereo ? p.etereo : 0) + p.protegido * (u.protegido ?? 0) + soma(u.aoEntrar) + p.aoDanoConjurador * soma(u.aoDano);
  const corpo = Math.max(2, Math.floor(orc - gasto));
  const vida = Math.min(20, Math.max(1, Math.ceil(corpo * u.fracVida)));
  return {
    ...molde, id: u.id, nome: u.nome, tipo: 'aliado', custo: u.custo, grau: u.grau, velocidade: 'findante',
    ataque: Math.min(20, corpo - vida), vida, texto: u.texto,
    ...(u.etereo ? { palavrasChave: ['Etéreo'] } : {}),
    ...(u.protegido ? { protegido: u.protegido } : {}),
    ...(u.aoEntrar ? { aoEntrar: u.aoEntrar } : {}),
    ...(u.aoDano ? { aoDanoConjurador: u.aoDano } : {}),
  };
}

const UNIDADES_R6: UnidadeR6[] = [
  // Grau médio: ameaças que pedem resposta
  { id: 'r6-espiao-c4', nome: 'Espião alado', custo: 4, grau: 3, fracVida: 0.5, etereo: true, aoDano: [{ tipo: 'comprar', valor: 1 }],
    texto: 'Etéreo. Ao causar dano de combate ao Conjurador inimigo: compre 1 carta.' },
  { id: 'r6-piromante-c4', nome: 'Piromante', custo: 4, grau: 3, fracVida: 0.55, aoEntrar: [{ tipo: 'danoTodosAliados', valor: 1 }],
    texto: 'Ao entrar: cause 1 de dano a cada Aliado do oponente.' },
  { id: 'r6-fera-cerco-c5', nome: 'Fera de cerco', custo: 5, grau: 4, fracVida: 0.6,
    aoEntrar: [{ tipo: 'dano', valor: 2, alvo: 'conjuradorInimigo' }, { tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }],
    texto: 'Ao entrar: cause 2 de dano ao Conjurador inimigo e cure 2 do seu Conjurador.' },
  { id: 'r6-presa-trovao-c5', nome: 'Presa-trovão', custo: 5, grau: 4, fracVida: 0.55, aoEntrar: [{ tipo: 'curar', valor: 5, alvo: 'conjuradorProprio' }],
    texto: 'Ao entrar: cure 5 do seu Conjurador.' },
  // Grau alto: bombas de late game
  { id: 'r6-anjo-c6', nome: 'Anjo da aurora', custo: 6, grau: 5, fracVida: 0.55, etereo: true, aoDano: [{ tipo: 'curar', valor: 4, alvo: 'conjuradorProprio' }],
    texto: 'Etéreo. Ao causar dano de combate ao Conjurador inimigo: cure 4 do seu Conjurador.' },
  { id: 'r6-mercador-c7', nome: 'Mercador de segredos', custo: 7, grau: 6, fracVida: 0.6, etereo: true, protegido: 2,
    aoEntrar: [{ tipo: 'comprar', valor: 1 }], aoDano: [{ tipo: 'comprar', valor: 1 }],
    texto: 'Etéreo. Protegido 2. Ao entrar: compre 1 carta. Ao causar dano de combate ao Conjurador inimigo: compre 1 carta.' },
  { id: 'r6-colosso-c7', nome: 'Colosso de ferro', custo: 7, grau: 6, fracVida: 0.55, protegido: 2,
    texto: 'Protegido 2.' },
  { id: 'r6-dragao-c8', nome: 'Dragão ancestral', custo: 8, grau: 7, fracVida: 0.55, etereo: true, protegido: 1, aoEntrar: [{ tipo: 'danoTodosAliados', valor: 2 }],
    texto: 'Etéreo. Protegido 1. Ao entrar: cause 2 de dano a cada Aliado do oponente.' },
];

/** Catálogo da variante, com as edições do autor quando `edicoesAutor`. */
export function catalogoDe(v: Variante): Catalogo {
  const cat = catalogoGerado(v);
  return v.edicoesAutor ? aplicarCatalogo(cat, lerEdicoes()) : cat;
}

/** Catálogo gerado (com a curva de Grau editada, mas sem as cartas e Conjuradores editados): o "original" do editor. */
export function catalogoGerado(v: Variante): Catalogo {
  const ed = v.edicoesAutor ? lerEdicoes() : null;
  if (ed?.curvaGrau !== undefined) v = { ...v, r6CurvaGrau: ed.curvaGrau };
  const p = precosDe(v);
  const mapa: Record<string, CartaDef> = Object.fromEntries(cartasBase.map((c) => [c.id, structuredClone(c)]));
  // Burn (rodada 2)
  for (const c of Object.values(mapa)) {
    if (c.tipo === 'temporaria' && c.efeitos?.some((ef) => ef.tipo === 'dano')) {
      if (v.burn === 'custoGrau') { c.custo += 2; c.grau += 2; }
      else if (v.burn === 'pagarX') c.pagarParaAnular = Math.max(1, c.custo + v.pagarXDelta);
    }
  }
  // Burn: menos dano por carta (rodada 3)
  if (v.burnNerf > 0) for (const c of [1, 2, 3, 4, 5]) mapa[`base-dano-conjurador-c${c}`] = danoConjuradorCard(c, p);
  // Cura
  if (v.cura2) for (const c of [1, 2, 3, 4, 6, 8]) mapa[`base-curar-c${c}`] = curarHibrida(c, p, 0.6);
  else if (v.cura) for (const c of [1, 2, 3, 4, 6, 8]) mapa[`base-curar-c${c}`] = curarHibrida(c, p, 0.65);
  const ge = v.permAcelera ? 1 : 0;
  // Estruturas com efeito
  if (comEstruturas(v)) {
    mapa['base-estrutura-c1'] = estruturaAtiva('base-estrutura-c1', 'Estrutura de Arcano (custo 1)', 1, { custo: 0, efeitos: [{ tipo: 'arcano', valor: 1 }] }, p, 0, v.staxFragil);
    mapa['base-estrutura-c3'] = estruturaAtiva('base-estrutura-c3', 'Estrutura de compra (custo 3)', 3, { custo: 1, efeitos: [{ tipo: 'comprar', valor: 1 }] }, p, 0, v.staxFragil);
    mapa['base-estrutura-c3-enfermaria'] = estruturaAtiva('base-estrutura-c3-enfermaria', 'Estrutura de cura (custo 3)', 3, { custo: 0, efeitos: [{ tipo: 'curar', valor: 3, alvo: v.balCuraSoAliados ? 'escolhidoAliado' : 'escolhido' }] }, p, 0, v.staxFragil);
    mapa['base-estrutura-c4-torre'] = estruturaAtiva('base-estrutura-c4-torre', 'Estrutura de dano (custo 4)', 4, { custo: 1, efeitos: [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }] }, p, 0, v.staxFragil);
    mapa['base-estrutura-c4-fonte'] = estruturaAtiva('base-estrutura-c4-fonte', 'Estrutura de Arcano (custo 4)', 4, { custo: 0, efeitos: [{ tipo: 'arcano', valor: 2 }] }, p, ge, v.staxFragil);
  }
  if (comRecursos(v)) {
    mapa['base-aliado-recurso-c1'] = aliadoRecurso(1, 1, p);
    mapa['base-aliado-recurso-c3'] = aliadoRecurso(3, 1, p, ge);
    mapa['base-aliado-recurso-c5'] = aliadoRecurso(5, 2, p, ge);
  }
  // Geração de Arcano cara (late game)
  if (v.arcanoAlto) {
    const g2 = v.permAcelera ? 2 : 0;
    mapa['base-estrutura-c6-nucleo'] = estruturaAtiva('base-estrutura-c6-nucleo', 'Estrutura de Arcano (custo 6)', 6, { custo: 0, efeitos: [{ tipo: 'arcano', valor: 3 }] }, p, ge, v.staxFragil);
    mapa['base-estrutura-c8-reator'] = estruturaAtiva('base-estrutura-c8-reator', 'Estrutura de Arcano (custo 8)', 8, { custo: 0, efeitos: [{ tipo: 'arcano', valor: 4 }] }, p, g2, v.staxFragil);
    mapa['base-aliado-recurso-c7'] = aliadoRecurso(7, 3, p, ge);
  }
  // Eternyr: sempre existem; os preços mudam com as variantes
  for (const [c, r] of [[1, false], [2, false], [3, false], [4, false], [2, true], [3, true]] as Array<[number, boolean]>) {
    const carta = comprarEternyr(c, r, p);
    mapa[carta.id] = carta;
  }
  for (const c of [4, 5, 6]) mapa[`eternyr-dano-aliado-c${c}`] = eternyrDanoAliado(c, p);
  mapa['eternyr-dano-jogador-c5'] = eternyrDanoJogador(5, p);
  mapa['eternyr-aliado-c6-g8'] = eternyrAliadoTardio(6, 8, p);
  mapa['eternyr-aliado-c8-g10'] = eternyrAliadoTardio(8, 10, p);
  mapa['eternyr-destruir-c6-g8'] = eternyrDestruirTardio(6, 8, p);
  if (v.balVarredura) { mapa['bal-varredura-c3'] = varredura(3, p); mapa['bal-varredura-c5'] = varredura(5, p); }
  if (v.balRemocaoGrau) mapa['bal-remocao-grau-c3'] = remocaoGrau(3, p);
  if (v.balStallPinga) mapa['bal-estrutura-cerco-c3'] = estruturaAtiva('bal-estrutura-cerco-c3', 'Estrutura de cerco (custo 3)', 3, { custo: 0, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] }, p, 0, v.staxFragil);
  if (v.balDevolver) mapa['bal-devolver-c2-repentina'] = devolverCarta(2, p);
  if (v.balAnular) mapa['bal-anular-c3-repentina'] = {
    ...molde, id: 'bal-anular-c3-repentina', nome: 'Anular (custo 3, repentina)', tipo: 'temporaria', custo: 3, grau: grauPadrao(3), velocidade: 'repentina',
    efeitos: [{ tipo: 'anular', alvo: 'pilha' }], texto: 'Anule uma carta na pilha.',
  };
  if (v.balEtereo) for (const c of [1, 2, 3]) mapa[`bal-etereo-c${c}`] = aliadoEtereo(c, p);
  if (v.balEtereoTardio) for (const c of [5, 7]) mapa[`bal-etereo-c${c}`] = aliadoEtereo(c, p);
  if (v.balDescarteExtra) for (const c of Object.values(mapa)) if (c.descartaGrau) c.descartaGrau += v.balDescarteExtra;
  if (v.staxFragil) for (const c of Object.values(mapa)) if (c.tipo === 'estrutura' && c.vida !== undefined && c.vida > 1) c.vida = Math.max(1, c.vida - (c.ativacao ? 0 : 1));
  // ----- rodada 6 -----
  if (v.r6Varreduras) {
    const x = Math.max(1, Math.floor(orcamento(2, 1, p) / p.danoTodos));
    mapa['r6-varredura-c2'] = temporariaR6('r6-varredura-c2', 'Onda de fagulhas', 2, grauPadrao(2), [{ tipo: 'danoTodosAliados', valor: x }], `Causa ${x} de dano a cada Aliado do oponente.`);
    mapa['r6-ira-c4'] = temporariaR6('r6-ira-c4', 'Ira', 4, 5, [{ tipo: 'destruirTodosAliados', ambos: true }], 'Destrua todos os Aliados (os seus também).');
    mapa['r6-por-do-sol-c7'] = temporariaR6('r6-por-do-sol-c7', 'Pôr do sol', 7, 7, [{ tipo: 'destruirTodosAliados', ambos: false }], 'Destrua todos os Aliados do oponente.');
  }
  if (v.r6Furia) {
    const x = Math.floor(orcamento(3, 1, p) / p.danoTodosAmbos);
    mapa['r6-furia-c3'] = temporariaR6('r6-furia-c3', 'Fúria', 3, grauPadrao(3), [{ tipo: 'danoTodosAliados', valor: x, ambos: true }], `Causa ${x} de dano a cada Aliado (os seus também).`);
  }
  // Curva de Grau: os Aliados (sem habilidade de Arcano) ganham bonusGrau em Ataque e Vida, na proporção do perfil.
  if (v.r6CurvaGrau > 0) {
    for (const c of Object.values(mapa)) {
      if (c.tipo !== 'aliado' || c.ativacao) continue;
      // r6CurvaSemDobro: Grau acima do padrão já paga pelo preço antigo (+1 PP por Grau; +2 no Eternyr), então a curva usa no máximo o Grau padrão do custo
      const g = v.r6CurvaSemDobro ? Math.min(c.grau, grauPadrao(c.custo)) : c.grau;
      const b = Math.floor(bonusGrau(g, v.r6CurvaGrau));
      if (b <= 0) continue;
      const frac = c.id.includes('agressivo') ? 0.4 : c.id.includes('defensivo') ? 0.7 : c.palavrasChave?.includes('Etéreo') ? 0.5 : 0.55;
      const dv = Math.ceil(b * frac);
      c.vida = Math.min(20, (c.vida ?? 0) + dv);
      c.ataque = Math.min(20, (c.ataque ?? 0) + b - dv);
    }
  }
  if (v.r6Cartas) for (const u of UNIDADES_R6) mapa[u.id] = unidadeR6(u, p, v.r6CurvaGrau);
  const cat: Catalogo = { cartas: mapa, conjuradores: { padrao: conjuradorDe(v) } };
  if (!v.r7Conjuradores) return cat;
  const comV = comValores(cat);
  for (const c of conjuradoresPorValor) comV.conjuradores[c.id] = c;
  for (const it of itensBasicos) comV.cartas[it.id] = structuredClone(it);
  return comV;
}

// ---------- baralhos ----------
type Lista = Array<[string, number]>;

function trocar(lista: Lista, de: string, para: string, n: number): void {
  const e = lista.find(([id]) => id === de);
  if (!e) return;
  const q = Math.min(n, e[1]);
  e[1] -= q;
  const d = lista.find(([id]) => id === para);
  if (d) d[1] += q;
  else lista.push([para, q]);
}

function tirarDosMaiores(lista: Lista, n: number): void {
  for (let i = 0; i < n; i++) {
    const maior = [...lista].filter(([id]) => id !== 'base-recurso').sort((a, b) => b[1] - a[1])[0];
    if (maior) maior[1] -= 1;
  }
}

function adicionar(lista: Lista, id: string, n: number): void {
  const e = lista.find(([x]) => x === id);
  if (e) e[1] += n;
  else lista.push([id, n]);
}

const A = (c: number, p = 'equilibrado') => `base-aliado-c${c}-${p}`;

function listaEternyr(v: Variante): Lista {
  const draws: Lista = v.eternyr
    ? [['eternyr-comprar-c1', 3], ['eternyr-comprar-c2', 3], ['eternyr-comprar-c3', 3], ['eternyr-comprar-c4', 2], ['eternyr-comprar-c2-repentina', 3]]
    : [['eternyr-comprar-c1', 2], ['eternyr-comprar-c3', 2], ['eternyr-comprar-c2-repentina', 2]];
  const lista: Lista = [
    ...draws, ['base-destruir-c3', 3], ['base-destruir-c4-repentina', 3], ['base-dano-c2-repentina', 3], ['base-dano-c3-repentina', 3],
    [A(4, 'defensivo'), 3], [A(6, 'defensivo'), 4], [A(6), 3], [A(8), 4], [A(8, 'equilibrado-descarta-grau'), 3], [A(5), 3], [A(3), 4], ['base-estrutura-c3', 3],
  ];
  const total = lista.reduce((s, [, n]) => s + n, 0);
  adicionar(lista, 'base-recurso', 60 - total);
  if (v.eternyrForte) {
    trocar(lista, 'base-dano-c2-repentina', 'eternyr-dano-aliado-c4', 3);
    trocar(lista, 'base-dano-c3-repentina', 'eternyr-dano-aliado-c5', 2);
    trocar(lista, 'base-dano-c3-repentina', 'eternyr-dano-aliado-c6', 1);
    trocar(lista, 'base-destruir-c3', 'eternyr-destruir-c6-g8', 3);
    trocar(lista, A(8), 'eternyr-aliado-c8-g10', 3);
    trocar(lista, A(6), 'eternyr-aliado-c6-g8', 3);
    trocar(lista, 'base-aliado-c8-equilibrado-descarta-grau', 'eternyr-dano-jogador-c5', 0);
    trocar(lista, 'base-recurso', 'eternyr-dano-jogador-c5', 2);
  }
  return lista;
}

export function arquetiposDe(v: Variante, catalogo: Catalogo): Arquetipo[] {
  if (v.r7Conjuradores) return v.edicoesAutor ? aplicarBaralhos(arquetiposConjuradores(), lerEdicoes()) : arquetiposConjuradores();
  const lista = arquetiposBase.map((a) => ({ ...a, lista: a.lista.map(([id, n]) => [id, n] as [string, number]) }));
  lista.push({ nome: 'Eternyr', estilo: { nome: 'Eternyr', agressividade: 0.3, reservaInstantes: true, grauAlvo: 8, queimaFace: false }, lista: listaEternyr(v) });
  for (const a of lista) {
    const L = a.lista;
    if (v.cura || v.cura2) {
      for (const [de, para] of [['base-curar-c2', 'base-curar-c3'], ['base-curar-c4', 'base-curar-c6']] as const) {
        const e = L.find(([id]) => id === de);
        if (e && e[1] >= 2) trocar(L, de, para, Math.floor(e[1] / 2));
      }
    }
    if (v.burnNerf > 0 && a.nome === 'Burn') {
      trocar(L, 'base-dano-conjurador-c3', 'base-dano-conjurador-c1', 2);
      trocar(L, 'base-dano-conjurador-c3', 'base-dano-conjurador-c2', 2);
      trocar(L, 'base-dano-conjurador-c5', 'base-dano-conjurador-c4', 2);
    }
    if (comEstruturas(v)) {
      if (a.nome === 'Stall') { trocar(L, 'base-mistica-c2', 'base-estrutura-c3-enfermaria', 4); trocar(L, 'base-mistica-c4', 'base-estrutura-c4-torre', 2); trocar(L, 'base-mistica-c4', 'base-estrutura-c4-fonte', 2); }
      if (a.nome === 'Controle') { trocar(L, 'base-mistica-c4', 'base-estrutura-c4-torre', 2); trocar(L, 'base-mistica-c4', 'base-estrutura-c3-enfermaria', 1); trocar(L, 'base-mistica-c2', 'base-estrutura-c3-enfermaria', 2); }
      if (a.nome === 'Midrange') trocar(L, 'base-mistica-c2', 'base-estrutura-c3-enfermaria', 2);
      if (a.nome === 'Ramp') trocar(L, 'base-recurso', 'base-estrutura-c4-fonte', 2);
    }
    if (comRecursos(v)) {
      tirarDosMaiores(L, 6);
      adicionar(L, 'base-recurso', 2);
      adicionar(L, 'base-aliado-recurso-c1', 2);
      adicionar(L, 'base-aliado-recurso-c3', 1);
      adicionar(L, 'base-aliado-recurso-c5', 1);
    }
    if (v.arcanoAlto) {
      const novos: Record<string, Array<[string, number]>> = {
        Ramp: [['base-estrutura-c6-nucleo', 2], ['base-estrutura-c8-reator', 2], ['base-aliado-recurso-c7', 2]],
        Eternyr: [['base-estrutura-c6-nucleo', 2], ['base-estrutura-c8-reator', 2]],
        Controle: [['base-estrutura-c6-nucleo', 2]],
        Stall: [['base-estrutura-c6-nucleo', 2], ['base-estrutura-c8-reator', 1]],
      };
      for (const [id, n] of novos[a.nome] ?? []) {
        tirarDosMaiores(L, n);
        adicionar(L, id, n);
      }
    }
    const poe = (id: string, n: number) => { tirarDosMaiores(L, n); adicionar(L, id, n); };
    if (v.balVarredura) {
      if (a.nome === 'Controle') { poe('bal-varredura-c3', 2); poe('bal-varredura-c5', 2); }
      if (a.nome === 'Stall') { poe('bal-varredura-c3', 2); poe('bal-varredura-c5', 1); }
      if (a.nome === 'Eternyr') poe('bal-varredura-c5', 1);
    }
    if (v.balRemocaoGrau) {
      if (a.nome === 'Controle' || a.nome === 'Eternyr') poe('bal-remocao-grau-c3', 3);
      if (a.nome === 'Stall') poe('bal-remocao-grau-c3', 2);
    }
    if (v.balStallPinga && a.nome === 'Stall') poe('bal-estrutura-cerco-c3', 4);
    if (v.balDevolver && a.nome === 'Tempo') poe('bal-devolver-c2-repentina', 4);
    if (v.balAnular && a.nome === 'Eternyr') poe('bal-anular-c3-repentina', 4);
    if (v.balAnular && a.nome === 'Controle') poe('bal-anular-c3-repentina', 3);
    if (v.balEtereo && a.nome === 'Tempo') { trocar(L, A(1), 'bal-etereo-c1', 4); trocar(L, A(2, 'agressivo'), 'bal-etereo-c2', 4); trocar(L, A(3), 'bal-etereo-c3', 4); }
    if (v.balAggroCurva && a.nome === 'Aggro') { trocar(L, A(5), A(2, 'agressivo'), 3); trocar(L, A(6, 'agressivo'), A(1), 2); trocar(L, A(4, 'agressivo'), A(2), 2); }
    if (v.balControleAgente && a.nome === 'Controle') {
      const snap = JSON.parse(readFileSync(new URL('./baralho-controle-agente.json', import.meta.url), 'utf8')) as { lista: Lista; estilo: Estilo };
      L.splice(0, L.length, ...snap.lista.filter(([id]) => catalogo.cartas[id]).map(([id, n]) => [id, n] as [string, number]));
      const falta = 60 - L.reduce((t, [, n]) => t + n, 0);
      if (falta > 0) adicionar(L, 'base-recurso', falta);
      a.estilo = { ...snap.estilo };
    }
    if (v.staxFragil && a.nome === 'Stall') {
      trocar(L, 'base-comprar-c3', comEstruturas(v) ? 'base-estrutura-c3-enfermaria' : 'base-mistica-c2', 4);
    }
    if (v.balBaralhos) {
      const cands = (JSON.parse(readFileSync(new URL('./' + v.balBaralhos, import.meta.url), 'utf8')) as { cands: Record<string, { lista: Lista; estilo: Estilo }> }).cands;
      const c = cands[a.nome];
      if (c) { L.splice(0, L.length, ...c.lista.map(([id, n]) => [id, n] as [string, number])); a.estilo = { ...c.estilo }; }
    }
    if (v.balEtereoTardio) {
      const n: Record<string, Array<[string, number]>> = {
        Midrange: [['bal-etereo-c5', 2], ['bal-etereo-c7', 1]], Mistura: [['bal-etereo-c5', 2], ['bal-etereo-c7', 1]],
        Controle: [['bal-etereo-c5', 1], ['bal-etereo-c7', 2]], Stall: [['bal-etereo-c7', 2]], Eternyr: [['bal-etereo-c7', 2]], Ramp: [['bal-etereo-c7', 2]],
      };
      for (const [id, q] of n[a.nome] ?? []) poe(id, q);
    }
    if (v.balControleRemocao && a.nome === 'Controle') {
      L.splice(0, L.length,
        // remoção (26): destruir, dano em Aliado, remoção que cresce, varreduras, anular
        ['base-destruir-c3', 4], ['base-destruir-c4-repentina', 2], ['base-dano-c1', 3], ['base-dano-c2', 3], ['base-dano-c3', 3], ['base-dano-c2-repentina', 2],
        ['bal-remocao-grau-c3', 4], ['bal-varredura-c3', 2], ['bal-varredura-c5', 2], ['bal-anular-c3-repentina', 2],
        // recurso (14): compra, Estrutura de compra, cura que compra, Arcano tardio
        ['base-comprar-c3', 4], ['base-comprar-c2-repentina', 3], ['base-estrutura-c3', 3], ['base-curar-c3', 3], ['base-estrutura-c6-nucleo', 1],
        // segurar o começo (7) e finalizar (5)
        [A(2, 'defensivo'), 3], [A(4, 'defensivo'), 3], ['base-estrutura-c3-enfermaria', 1], [A(6), 2], [A(8), 3],
        ['base-recurso', 7]);
      a.estilo = { nome: 'Controle', agressividade: 0.3, reservaInstantes: true, grauAlvo: 8, queimaFace: false };
    }
    // ----- rodada 6: trocas uma a uma (sem cotas), cada estratégia recebe o que combina com ela -----
    if (v.r6Varreduras && a.nome === 'Controle') {
      trocar(L, 'base-dano-c1', 'r6-varredura-c2', 2);
      trocar(L, 'base-dano-c3', 'r6-ira-c4', 1);
      trocar(L, 'base-dano-c2', 'r6-por-do-sol-c7', 1);
    }
    if (v.r6Furia && a.nome === 'Controle') {
      trocar(L, 'r6-varredura-c2', 'r6-furia-c3', 2);
      trocar(L, 'bal-varredura-c3', 'r6-furia-c3', 1);
    }
    if (v.r6StallVarre && a.nome === 'Stall') {
      trocar(L, 'base-dano-c2', 'r6-ira-c4', 2);
      trocar(L, 'bal-varredura-c3', 'r6-furia-c3', 2);
      trocar(L, 'base-curar-c2', 'r6-por-do-sol-c7', 1);
    }
    if (v.r6Cartas) {
      const trocas: Record<string, Array<[string, string, number]>> = {
        // Controle: o finalizador vira o Mercador (vantagem de cartas e difícil de remover); Piromante segura o começo largo
        Controle: [[A(8), 'r6-mercador-c7', 2], [A(4, 'defensivo'), 'r6-piromante-c4', 1]],
        // Midrange: ameaças de Grau médio e o topo da curva
        Midrange: [[A(4), 'r6-espiao-c4', 2], [A(4, 'defensivo'), 'r6-piromante-c4', 2], [A(5), 'r6-fera-cerco-c5', 2], [A(5), 'r6-presa-trovao-c5', 1], [A(6), 'r6-anjo-c6', 1], [A(8), 'r6-mercador-c7', 1]],
        // Ramp: as bombas mais caras
        Ramp: [[A(8), 'r6-dragao-c8', 2], [A(7), 'r6-colosso-c7', 1], [A(8, 'equilibrado-descarta-grau'), 'r6-colosso-c7', 1], [A(6, 'equilibrado-descarta-grau'), 'r6-anjo-c6', 1]],
        // Tempo: ameaça evasiva que compra
        Tempo: [[A(4, 'agressivo'), 'r6-espiao-c4', 2]],
        // Stall: vida e finalizador que cura
        Stall: [[A(8), 'r6-anjo-c6', 2], [A(6, 'defensivo'), 'r6-presa-trovao-c5', 2]],
        ...(v.r6EternyrSemBombas ? {} : { Eternyr: [[A(7), 'r6-mercador-c7', 1], ['eternyr-aliado-c8-g10', 'r6-dragao-c8', 1]] as Array<[string, string, number]> }),
      };
      for (const [de, para, n] of trocas[a.nome] ?? []) trocar(L, de, para, n);
    }
    // baralhos e estilos que o agente de testes achou na BAL (rodada 6), por cima de tudo
    if (v.r6BaralhosAgente) {
      const cands = (JSON.parse(readFileSync(new URL('./' + v.r6BaralhosAgente, import.meta.url), 'utf8')) as { cands: Record<string, { lista: Lista; estilo: Estilo }> }).cands;
      const ignora = (v.r6AggroPadrao && a.nome === 'Aggro') || (v.r6ControleRemocao && a.nome === 'Controle');
      const c = ignora ? undefined : cands[a.nome];
      if (c) { L.splice(0, L.length, ...c.lista.map(([id, n]) => [id, n] as [string, number])); a.estilo = { ...c.estilo }; }
    }
    // identidade (rodada 6): o agente de testes enfraqueceu o Aggro com cartas que um Aggro não usa (Grau 5 a 10, Eternyr,
    // varreduras, cura, Místicas, defensivos); voltam a ser cartas baratas de pressão
    if (v.r6AggroIdentidade && a.nome === 'Aggro') {
      const trocas: Array<[string, string, number]> = [
        ['eternyr-comprar-c3', 'base-comprar-c1', 2], ['eternyr-dano-aliado-c5', 'base-dano-c1', 2], ['bal-varredura-c3', A(2, 'agressivo'), 2],
        ['base-mistica-c2', A(2, 'agressivo-proficiente'), 2], [A(2, 'defensivo'), A(1), 2], ['eternyr-destruir-c6-g8', 'base-dano-c3', 1],
        ['eternyr-aliado-c8-g10', A(3), 1], ['base-aliado-recurso-c7', A(2, 'agressivo'), 1], [A(6, 'equilibrado-descarta-grau'), A(2, 'agressivo-proficiente'), 1],
        [A(8), A(3), 1], ['base-estrutura-c4-fonte', 'base-estrutura-c1', 1], ['base-curar-c6', 'base-dano-c1', 1], ['base-comprar-c3', 'base-comprar-c1', 1], ['r6-furia-c3', A(1), 1],
      ];
      for (const [de, para, n] of trocas) trocar(L, de, para, n);
    }
    if (v.r6ControleRemocao && a.nome === 'Controle') {
      // o que o agente achou para segurar o Aggro (cura que compra, Presa-trovão) entra no lugar do dano fraco e da compra pura
      trocar(L, 'base-dano-c1', 'base-curar-c3', 1);
      trocar(L, 'bal-varredura-c3', 'base-curar-c3', 1);
      trocar(L, 'base-comprar-c2-repentina', 'r6-presa-trovao-c5', 2);
    }
    // teste: Controle com o Mercador no lugar dos Aliados tardios do Eternyr
    if (v.r6ControleMercador && a.nome === 'Controle') trocar(L, 'eternyr-aliado-c6-g8', 'r6-mercador-c7', 2);
    // teste: Controle sem as cartas de Aggro (Aliado de custo 1, 4-drop proficiente, dano ao Conjurador), com compra
    if (v.r6ControleCompra && a.nome === 'Controle') {
      trocar(L, A(1), 'eternyr-comprar-c2', 1);
      trocar(L, A(4, 'agressivo-proficiente'), 'eternyr-comprar-c2', 1);
      trocar(L, 'base-dano-conjurador-c3', 'eternyr-comprar-c2', 1);
    }
    // segundo ajuste do agente de testes (rodada 6, objetivos-bal6b): Controle, Midrange e Tempo, por cima de tudo acima
    if (v.r6BaralhosAgente2) {
      const cands = (JSON.parse(readFileSync(new URL('./' + v.r6BaralhosAgente2, import.meta.url), 'utf8')) as { cands: Record<string, { lista: Lista; estilo: Estilo }> }).cands;
      const c = cands[a.nome];
      if (c) { L.splice(0, L.length, ...c.lista.map(([id, n]) => [id, n] as [string, number])); a.estilo = { ...c.estilo }; }
    }
    // Controle sem dano ao Conjurador (é de Burn): no lugar, Recurso básico (3 de Arcano por carta = menos cartas gastas para pagar)
    if (v.r6ControleSemQueima && a.nome === 'Controle') {
      for (const id of ['base-dano-conjurador-c2', 'base-dano-conjurador-c3', 'base-dano-conjurador-c5']) trocar(L, id, v.r6ControleSemQueima === 'compra' ? 'eternyr-comprar-c4' : 'base-recurso', 1);
    }
    // Controle troca 2 Aliados de ataque sem texto (custo 5 e custo 6 agressivo) pela Estrutura que compra ao entrar (fica em campo)
    if (v.r6ControleEstruturas && a.nome === 'Controle') {
      trocar(L, A(6, 'agressivo'), 'base-estrutura-c3', 1);
      trocar(L, A(5), 'base-estrutura-c3', 1);
    }
    // Controle troca a Fera de cerco (ameaça de Midrange) por compra pesada (Eternyr: compre 3 por custo 4)
    if (v.r6ControleCompra3 && a.nome === 'Controle') trocar(L, 'r6-fera-cerco-c5', 'eternyr-comprar-c4', v.r6ControleCompra3 === true ? 2 : v.r6ControleCompra3);
    // estilo do Controle (agressividade); aplicado por último, depois dos baralhos do agente
    if (v.r6ControleEstilo && a.nome === 'Controle') a.estilo = { ...a.estilo, agressividade: v.r6ControleAgressividade || 0.3 };
    const sobra = L.filter(([, n]) => n <= 0);
    for (const s of sobra) L.splice(L.indexOf(s), 1);
    const total = L.reduce((s, [, n]) => s + n, 0);
    if (total !== 60) throw new Error(`${a.nome} (${v.id}) tem ${total} cartas`);
    for (const [id] of L) if (!catalogo.cartas[id]) throw new Error(`${a.nome} (${v.id}): carta inexistente ${id}`);
  }
  return lista;
}
