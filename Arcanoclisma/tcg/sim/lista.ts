// Lista de configurações da rodada 3: uma mudança por vez e em combinação (pacote P e "pacote menos uma peça").
import { variante, type Variante } from './variantes.ts';

const PERM = { estruturas: true, recursos: true };
const CONJ = { conjAtaca: true, conjCusto: 0, grauSemSuspender: true };

/** Pacote com todas as decisões e propostas da rodada 3. */
export const PACOTE: Partial<Variante> = {
  ...PERM, arcanoAlto: true, permAcelera: true, eternyrForte: true, burnNerf: 1, cura2: true, staxFragil: true, impasse: 5,
  segundoCarta: 0, segundoArcano: 0, primeiroSemGrau: true, primeiroConjNo: true, ...CONJ,
};

/** Cartas e efeitos aprovados do agente-balanceador (rodada 4). */
export const BAL_CARTAS: Partial<Variante> = {
  balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, balEtereo: true, balAggroCurva: true, balAnular: true,
  balControleAgente: true, balCuraSoAliados: true, balDescarteExtra: 1,
};
/** Regras aprovadas da rodada 4 (sem a Cataclisma crescente e sem o Grau suspenso do primeiro jogador). */
export const BAL_REGRAS: Partial<Variante> = { primeiroConjNo: false, primeiroSemGrau: true, segundoCarta: 1 };

/** Padrão da rodada 5 (guardado como PADRAO-R5; a rodada 6 foi aprovada pelo autor e virou o PADRAO). */
const PADRAO_CFG: Partial<Variante> = { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botAtacaAliados: true, balControleRemocao: true };

/** Rodada 6: passos do agente-balanceador, na ordem em que foram medidos (docs/MUDANCAS-DO-BALANCEADOR.md). */
const R6_PASSOS: Array<Partial<Variante>> = [
  { segundoCarta: 0 },
  { r6Varreduras: true },
  { r6CurvaGrau: 0.5 },
  { r6Cartas: true },
  { r6Furia: true },
  { botVarredura: true },
  { r6StallVarre: true },
  { r6BaralhosAgente: 'baralhos-bal6-agente.json' },
  { r6AggroPadrao: true },
  { r6EternyrSemBombas: true },
  { r6ControleRemocao: true },
  { r6BaralhosAgente2: 'baralhos-bal6b-agente.json' },
  { r6ControleEstilo: true, r6ControleAgressividade: 0.3 },
  { r6ControleEstruturas: true },
  { r6ControleSemQueima: 'compra', r6ControleCompra3: 1 },
];
/** PADRAO mais os n primeiros passos da rodada 6. */
const r6 = (n: number): Partial<Variante> => Object.assign({}, PADRAO_CFG, ...R6_PASSOS.slice(0, n));

const PECAS: Array<[string, string, Partial<Variante>]> = [
  ['arcanoAlto', 'geração de Arcano cara', { arcanoAlto: false }],
  ['permAcelera', 'permanentes contam como Grau', { permAcelera: false }],
  ['eternyrForte', 'Eternyr forte', { eternyrForte: false }],
  ['burnNerf', 'burn com menos dano por carta', { burnNerf: 0 }],
  ['cura2', 'cura melhorada', { cura2: false }],
  ['segundo00', 'segundo sem carta nem Arcano extra', { segundoCarta: 1, segundoArcano: 1 }],
  ['primeiroConjNo', 'primeiro Conjurador não ataca no turno 1', { primeiroConjNo: false }],
  ['staxFragil', 'stax frágil', { staxFragil: false }],
  ['impasse', 'relógio de impasse (5)', { impasse: 0 }],
  ['primeiroSemGrau', 'quem começa sem Grau no turno 1', { primeiroSemGrau: false }],
  ['conj', 'Conjurador ataca grátis e Grau sem suspender', { conjAtaca: false, conjCusto: 1, grauSemSuspender: false }],
];

export const rodada3: Variante[] = [
  variante('base', 'Base (regras antigas, sem mudanças)', {}),
  // referências e isoladas (partem de "perm" quando a mudança precisa de permanentes de recurso)
  variante('perm', 'REF Estruturas com efeito + Aliados de recurso', PERM),
  variante('perm+arcAlto', 'Perm + geração de Arcano cara', { ...PERM, arcanoAlto: true }),
  variante('perm+acelera', 'Perm + permanentes contam como Grau', { ...PERM, permAcelera: true }),
  variante('perm+ambos', 'Perm + Arcano caro + contam como Grau', { ...PERM, arcanoAlto: true, permAcelera: true }),
  variante('eternyrForte', 'Eternyr forte (dano em Aliado, poder tardio)', { eternyrForte: true }),
  variante('burn1', 'Burn: dano ao Conjurador 2,5 PP/ponto', { burnNerf: 1 }),
  variante('burn2', 'Burn: dano ao Conjurador 3 PP/ponto', { burnNerf: 2 }),
  variante('cura2', 'Cura melhorada de novo', { cura2: true }),
  variante('segundo00', 'Segundo sem carta nem Arcano extra', { segundoCarta: 0, segundoArcano: 0 }),
  variante('conj', 'REF Conjurador ataca grátis, Grau sem suspender', CONJ),
  variante('conj+primeiroNo', 'Conj + primeiro Conjurador não ataca no turno 1', { ...CONJ, primeiroConjNo: true }),
  variante('stax-ref', 'REF Estruturas com efeito (stax padrão)', { estruturas: true }),
  variante('stax', 'Stax frágil', { estruturas: true, staxFragil: true }),
  variante('impasse3', 'Relógio de impasse: 3 turnos', { impasse: 3 }),
  variante('impasse5', 'Relógio de impasse: 5 turnos', { impasse: 5 }),
  variante('impasse8', 'Relógio de impasse: 8 turnos', { impasse: 8 }),
  // pacote e pacote menos uma peça
  variante('P', 'PACOTE (tudo)', PACOTE),
  ...PECAS.map(([id, nome, desliga]) => variante(`P-${id}`, `Pacote sem: ${nome}`, { ...PACOTE, ...desliga })),
  // variante de trabalho do agente-balanceador: começa igual ao pacote; ele muda o que precisar aqui e em variantes.ts
  variante('BAL1', 'Balanceador 1: + varreduras', { ...PACOTE, balVarredura: true }),
  variante('BAL2', 'Balanceador 2: + remoção que cresce com o Grau', { ...PACOTE, balVarredura: true, balRemocaoGrau: true }),
  variante('BAL3', 'Balanceador 3: + Estrutura de cerco no Stall', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true }),
  variante('BAL4', 'Balanceador 4: + devolver no Tempo', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true }),
  variante('BAL5', 'Balanceador 5: + relógio de impasse 3', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, impasse: 3 }),
  variante('BAL4+conjDano2', 'BAL4 + Conjurador com dano básico 2', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, conjDano: 2 }),
  variante('BAL4+etereo', 'BAL4 + Aliados Etéreos no Tempo', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, balEtereo: true }),
  variante('BAL4+aggroCurva', 'BAL4 + curva do Aggro mais baixa', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, balAggroCurva: true }),
  variante('BAL4+controleAgente', 'BAL4 + Controle do agente de testes', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, balControleAgente: true }),
  variante('BAL4+impasse4', 'BAL4 + relógio de impasse 4', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, impasse: 4 }),
  variante('BAL4+anular', 'BAL4 + Anular (Eternyr e Controle)', { ...PACOTE, balVarredura: true, balRemocaoGrau: true, balStallPinga: true, balDevolver: true, balAnular: true }),
  // BAL4+cata10 até BAL12c usavam a Cataclisma crescente e o Grau suspenso do primeiro jogador, que o autor recusou
  // (resultados guardados em docs/MUDANCAS-DO-BALANCEADOR.md). A rodada 5 parte daqui.
  // PADRAO: regras, cartas e baralhos aprovados pelo autor (= R5mc: R5 + estratégia de atacar Aliados + Controle de remoção). Base para os agentes de testes e balanceador.
  // Rodada 7: um Conjurador por par de Valores, com cartas marcadas por Valor e um baralho para cada um.
  variante('CONJ', 'Rodada 7: 9 Conjuradores (par de Valores + habilidade própria)', { ...r6(R6_PASSOS.length), r7Conjuradores: true, edicoesAutor: true }),
  variante('PADRAO-R5', 'Padrão da rodada 5 (antes da rodada 6)', PADRAO_CFG),
  variante('PADRAO', 'Padrão aprovado pelo autor (rodada 6: varreduras, curva de Grau, unidades-problema, bombas) + edições do autor', { ...r6(R6_PASSOS.length), edicoesAutor: true }),
  // ----- rodada 6 (agente-balanceador): um passo por vez a partir do PADRAO; BAL = estado atual de trabalho -----
  variante('BAL6a', 'R6 passo 1: PADRAO sem a carta extra do segundo jogador', r6(1)),
  variante('BAL6b', 'R6 passo 2: + varreduras no Controle', r6(2)),
  variante('BAL6c', 'R6 passo 3: + curva de poder por Grau nos Aliados', r6(3)),
  variante('BAL6d', 'R6 passo 4: + unidades-problema e bombas', r6(4)),
  variante('BAL6e', 'R6 passo 5: + Fúria (varredura simétrica barata) no Controle', r6(5)),
  variante('BAL6f', 'R6 passo 6: + jogador automático guarda as varreduras', r6(6)),
  variante('BAL6g', 'R6 passo 7: + varreduras novas no Stall', r6(7)),
  variante('BAL6h', 'R6 passo 8: + baralhos do agente de testes (sim/baralhos-bal6-agente.json)', r6(8)),
  variante('BAL6i', 'R6 passo 9: Aggro com a lista e o estilo aprovados do PADRAO (o do agente usava cartas lentas)', r6(9)),
  variante('BAL6j', 'R6 passo 10: Eternyr sem o Mercador e o Dragão', r6(10)),
  variante('BAL6j-compra', 'teste: passo 10 com o Controle trocando 3 cartas de Aggro (Aliado de custo 1, 4-drop proficiente, dano ao Conjurador) por compra', { ...r6(10), r6ControleCompra: true }),
  variante('BAL6k', 'R6 passo 11: Controle de remoção (passo 7) no lugar do Controle do agente, com a cura que compra e 2 Presa-trovão', r6(11)),
  variante('BAL6l', 'R6 passo 12: + baralhos do 2º ajuste do agente de testes (sim/baralhos-bal6b-agente.json: Controle, Midrange, Tempo)', r6(12)),
  variante('BAL6m', 'R6 passo 13: Controle do agente em agressividade 0,3 (0,1 deixava as partidas longas demais)', r6(13)),
  variante('BAL6n', 'R6 passo 14: Controle troca 2 Aliados de ataque pela Estrutura que compra ao entrar', r6(14)),
  variante('BAL6n-compra3', 'teste: passo 14 com o Controle trocando 2 Feras de cerco por 2 Eternyr: comprar 3', { ...r6(14), r6ControleCompra3: true }),
  variante('BAL6n-queimaCompra', 'teste: passo 14 com o Controle trocando dano ao Conjurador por Eternyr: comprar 3', { ...r6(14), r6ControleSemQueima: 'compra' }),
  variante('BAL6o', 'R6 passo 15: Controle troca 3 cartas de dano ao Conjurador e 1 Fera de cerco por 4 Eternyr: comprar 3', r6(15)),
  variante('BAL6m-semQueima', 'teste: passo 13 com o Controle trocando dano ao Conjurador por Recurso básico', { ...r6(13), r6ControleSemQueima: true }),
  variante('BAL6l-c05', 'teste: passo 12 com o Controle em agressividade 0,5', { ...r6(12), r6ControleEstilo: true, r6ControleAgressividade: 0.5 }),
  variante('BAL6j-mercador', 'teste: passo 10 com o Mercador no lugar dos Aliados tardios do Eternyr no Controle', { ...r6(10), r6ControleMercador: true }),
  variante('BAL6i-semDobro', 'teste: passo 9 com a curva de Grau sem somar com o bônus antigo de Grau acima do padrão', { ...r6(9), r6CurvaSemDobro: true }),
  variante('BAL6i-controle03', 'teste: passo 9 com o Controle do agente em agressividade 0,3', { ...r6(9), r6ControleEstilo: true }),
  variante('BAL6i-trocas', 'teste: passo 8 com o Aggro do agente trocando as cartas lentas por cartas baratas', { ...r6(8), r6AggroIdentidade: true }),
  variante('BAL6i-furia4', 'teste: BAL6i-trocas com a Fúria a 2 PP por ponto (4 de dano)', { ...r6(8), r6AggroIdentidade: true, r6PrecoTodosAmbos: 2 }),
  // testes da intensidade da curva de Grau (k) sobre o passo 6
  variante('BAL6f-k0', 'R6 passo 6 sem a curva de Grau (k = 0)', { ...r6(6), r6CurvaGrau: 0 }),
  variante('BAL6f-k03', 'R6 passo 6 com curva k = 0,3', { ...r6(6), r6CurvaGrau: 0.3 }),
  variante('BAL6f-k07', 'R6 passo 6 com curva k = 0,7', { ...r6(6), r6CurvaGrau: 0.7 }),
  variante('BAL', 'Balanceador rodada 6 (varreduras, curva de Grau, unidades-problema e bombas)', r6(R6_PASSOS.length)),
  variante('R5', 'Rodada 5: BAL sem Cataclisma crescente nem Grau suspenso', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json' }),
  variante('R5a', 'R5 + jogador automático prevê bloqueios', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botPreve: true }),
  variante('R5g', 'R5 + jogador automático sobe o Grau até as cartas da mão', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botGrau: true }),
  variante('R5e', 'R5 + Aliados Etéreos caros (finalizadores)', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', balEtereoTardio: true }),
  variante('R5m', 'R5 + Aliados atacam Aliados (mesa)', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botAtacaAliados: true }),
  variante('R5me', 'R5 + Aliados atacam Aliados + Etéreos caros', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botAtacaAliados: true, balEtereoTardio: true }),
  variante('R5mc', 'R5m + Controle de remoção', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botAtacaAliados: true, balControleRemocao: true }),
  variante('R5ge', 'R5 + sobe o Grau + Etéreos caros', { ...PACOTE, ...BAL_CARTAS, ...BAL_REGRAS, balBaralhos: 'baralhos-bal11.json', botGrau: true, balEtereoTardio: true }),
  // jogador de deck lento (outra variância)
  variante('base+lentos', 'Base com jogador lento (sobreviver / Grau alto)', { lentos: true }),
  variante('perm+lentos', 'Perm com jogador lento', { ...PERM, lentos: true }),
  variante('P+lentos', 'Pacote com jogador lento', { ...PACOTE, lentos: true }),
];
