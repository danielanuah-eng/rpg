// Tipos do motor do Arcanoclisma TCG. Só sintaxe "apagável" (sem enum), para rodar direto no Node 24.

export type Jogador = 0 | 1;
export type Etapa = 'inicio' | 'principal' | 'cataclisma' | 'final';
/** 'item': equipamento do Conjurador (Canal, Armadura ou Extra), escolhido na preparação, fora do baralho. */
export type TipoCarta = 'aliado' | 'temporaria' | 'mistica' | 'estrutura' | 'item';
export type CategoriaItem = 'canal' | 'armadura' | 'extra';
export type Velocidade = 'findante' | 'repentina';
export type AlvoEfeito = 'escolhido' | 'escolhidoAliado' | 'conjuradorInimigo' | 'conjuradorProprio';

/**
 * Requisito de uma linha de efeito, habilidade ou ação (pedido do autor, 04/10), definido no Editor:
 * grau = mínimo de cartas no seu Grau para acontecer; arcano = Arcano pago na hora (reserva, depois Grau levantado).
 * Sem cumprir, aquela linha não acontece (as outras da carta continuam).
 */
export interface Requisito { grau?: number; arcano?: number }

export type Efeito = EfeitoBase & { requisito?: Requisito };
type EfeitoBase =
  /** escalaGrau: soma ao dano o número de cartas na Zona de Grau de quem joga (remoção que cresce com o jogo). */
  | { tipo: 'dano'; valor: number; alvo: AlvoEfeito; escalaGrau?: boolean }
  /** Devolve a permanente alvo para a mão do dono (tempo). */
  | { tipo: 'devolver'; alvo: 'escolhido' | 'escolhidoAliado' | 'escolhidoProprio' }
  /** Causa dano a cada Aliado do oponente. */
  | { tipo: 'danoTodosAliados'; valor: number; /** Rodada 6: também nos seus Aliados (varredura simétrica). */ ambos?: boolean }
  /** Destrói todos os Aliados do oponente (ambos: false) ou de todos os jogadores (ambos: true). Rodada 6. */
  | { tipo: 'destruirTodosAliados'; ambos: boolean }
  /** Anula uma carta na pilha (vai para o cemitério do dono). */
  | { tipo: 'anular'; alvo: 'pilha' }
  | { tipo: 'curar'; valor: number; alvo: AlvoEfeito }
  | { tipo: 'destruir'; alvo: 'escolhido' | 'escolhidoAliado' }
  | { tipo: 'comprar'; valor: number }
  | { tipo: 'arcano'; valor: number }
  | { tipo: 'arcanoOponente'; valor: number }
  /** Rodada 7. A primeira carta que você joga neste turno custa N a menos (só vale se você ainda não jogou carta no turno). */
  | { tipo: 'descontoProximaCarta'; valor: number }
  /** Rodada 7. Bane (zona de banimento) uma carta da sua mão; no seu próximo turno você pode jogá-la custando `desconto` a menos. Alvo: carta da mão. */
  | { tipo: 'banirDaMao'; desconto: number }
  /** Rodada 7. Põe no seu campo um Aliado do cemitério de qualquer jogador; você paga `custoExtra` a mais, além do custo dele. Alvo: Aliado em um cemitério. */
  | { tipo: 'reanimar'; custoExtra: number }
  /** Rodada 7. Neste turno você pode jogar do seu cemitério uma carta cujo efeito anule outra, pagando o custo dela; ao resolver ela é banida. */
  | { tipo: 'permitirAnularDoCemiterio' }
  /** Olhe as N cartas do topo do seu baralho, ponha `pegar` delas na mão e o resto no fundo do baralho. */
  | { tipo: 'olharTopo'; quantidade: number; pegar: number }
  /** Itens (03/10, noite). Foco Arcano: neste turno, o próximo efeito de dano ou cura de uma Temporária sua tem +N. */
  | { tipo: 'potenciaMagia'; valor: number }
  /** Itens (03/10, noite). Põe no seu campo uma ficha (carta do catálogo que não vai em baralho, ex.: Espírito Invocado). */
  | { tipo: 'invocar'; carta: string; /** Nome para o texto da carta. */ nome?: string }
  /** Itens (03/10, noite). Seu Conjurador ganha +N de Ataque neste turno. */
  | { tipo: 'ataqueConjuradorTurno'; valor: number };

/** Arma: efeitos ao atacar (pedido do autor, 04/10). */
export interface AoAtacar {
  /** Efeitos ao declarar o ataque (ex.: ganhe 1 de Arcano, compre 1 carta). */
  efeitos?: Efeito[];
  /** Dano a mais quando o golpe acerta Aliados, Místicas ou Estruturas (atacados ou bloqueando). */
  danoUnidades?: number;
  /** Desgaste a mais na Armadura inimiga quando ela protege deste ataque. */
  desgasteArmadura?: number;
}

/** Habilidade ativada de uma permanente: paga custo em Arcano e (opcionalmente) suspende. */
export interface Ativacao {
  /** Habilidade de Conjurador de velocidade Repentina: pode ser usada com prioridade, a qualquer momento (as demais: só na sua Principal, pilha vazia). */
  repentina?: boolean;
  custo: number;
  suspende: boolean;
  efeitos: Efeito[];
  /** Itens: usar suspende o Conjurador (que precisa estar de pé). */
  suspendeConjurador?: boolean;
  /** Custo extra: descartar N cartas da mão (escolhidas no comando, em descartarCusto). */
  descartar?: number;
  /** Custo extra: o seu Conjurador toma N de dano. */
  danoNoConjurador?: number;
  /** Itens: o item vai para o cemitério depois de usado (padrão: só o Extra). */
  consome?: boolean;
  /** Requisito da habilidade (04/10): Grau mínimo para ativar; o Arcano soma ao custo. */
  requisito?: Requisito;
}

export interface Regras {
  /** Sem baralho, sem mão e sem ações possíveis no início da Principal: o jogador perde. */
  semAcaoPerde: boolean;
  /** Quem começa não pode pôr carta no Grau no turno 1. */
  primeiroSemGrauNoTurno1: boolean;
  /** Pôr carta no Grau não suspende o Conjurador (1 por turno), então ele ainda pode atacar. */
  grauSemSuspender: boolean;
  /** O Conjurador de quem começa não pode atacar no turno 1. */
  primeiroConjSemAtacarTurno1: boolean;
  /** Relógio de impasse: perde quem passa este número de turnos seguidos sem jogar carta, ativar ou atacar (0 = desligado). */
  impasseTurnos: number;
  /** Cartas a mais que o segundo jogador compra na preparação. */
  segundoCartaExtra: number;
  /** Arcano com que o segundo jogador começa. */
  segundoArcanoInicial: number;
  /** Máximo de cartas na mão no fim do turno (o excesso é descartado). */
  limiteMao: number;
  /** No Início do próprio turno, o jogador compra até ficar com a mão inicial do Conjurador (em vez de comprar no Final). */
  reporMaoNoInicio: boolean;
  /** No Início do próprio turno, o Conjurador levanta (ex.: depois de bloquear no turno do oponente). */
  conjuradorLevantaNoInicio: boolean;
  /** O Conjurador só ataca com arma (item no Canal ou Extra com Ataque). */
  conjuradorSoAtacaComArma: boolean;
}

export interface CartaDef {
  id: string;
  nome: string;
  tipo: TipoCarta;
  subtipos: string[];
  valores: string[];
  custo: number;
  grau: number;
  /** Valores que precisam ter ao menos uma carta na Zona de Grau. */
  custoValores?: string[];
  /** Quantas cartas do Grau vão para o cemitério ao jogar. */
  descartaGrau?: number;
  velocidade: Velocidade;
  /** Arcano que gera ao ser enviada à Zona de Contador Arcano. */
  arcanoGerado: number;
  /** Palavras-chave. 'Proficiente': ataca e usa ativações que suspendem no turno em que entra. 'Etéreo': só é bloqueado por Aliados Etéreos. */
  palavrasChave?: string[];
  ataque?: number;
  vida?: number;
  /** Enquanto estiver em campo, conta como cartas extras na Zona de Grau para os requisitos de Grau. */
  grauExtra?: number;
  /** Habilidade ativada (Estruturas, Aliados de suporte). */
  ativacao?: Ativacao;
  /** Burn: depois de jogada, o oponente pode pagar este Arcano para ela não resolver. */
  pagarParaAnular?: number;
  /** Efeitos de uma Temporária. */
  efeitos?: Efeito[];
  /** Gatilho ao entrar em campo (permanentes). */
  aoEntrar?: Efeito[];
  /** Rodada 6: gatilho "ao causar dano de combate ao Conjurador inimigo" (Aliados). */
  aoDanoConjurador?: Efeito[];
  /** Rodada 6, palavra-chave Protegido N: o oponente paga N de Arcano a mais para mirar esta carta com uma carta ou ativação. */
  protegido?: number;
  /** Item: espaço que ocupa no Conjurador. O Ataque de Canal e Extra é o valor da arma. */
  categoriaItem?: CategoriaItem;
  /** Canal: arma (dá Ataque ao Conjurador), foco arcano (melhora magias) ou transcritor (mexe no baralho e na mão). */
  tipoCanal?: 'arma' | 'foco' | 'transcritor';
  /** Item exclusivo de uma competência (Comprometimento do Conjurador), ex.: invocações para Invocador. */
  competencia?: ConjuradorDef['comprometimento'];
  /** Item exclusivo destes Conjuradores (ids). */
  exclusivoDe?: string[];
  /** Ficha: carta criada por efeito (invocar); não entra em baralho. */
  ficha?: boolean;
  /**
   * Arma (Canal): o que acontece quando o Conjurador ataca com ela (pedido do autor, 04/10). O Arcano pago e o Grau
   * mínimo para atacar são o custo e o Grau da própria carta do item (podem ser 0).
   */
  aoAtacar?: AoAtacar;
  /** Armadura: efeitos quando ela evita dano (ex.: ganhe 1 de Arcano, compre 1 carta). */
  aoProteger?: Efeito[];
  /** Armadura: quando ela evita dano, causa este dano ao atacante. */
  espinhos?: number;
  /** Armadura: dano de combate que ela evita quando o Conjurador bloqueia. */
  defesa?: number;
  /** Armadura: quanto dano ela ainda pode evitar antes de quebrar (vida de desgaste). */
  vidaDesgaste?: number;
  cenario: string;
  texto: string;
}

/** Habilidade própria do Conjurador (rodada 7): uma ativada, um gatilho ou uma passiva. */
export interface PoderConjurador {
  /** Ativada: suspende o Conjurador (então ele não ataca nesse turno), paga o custo em Arcano e aplica os efeitos. */
  ativacao?: Ativacao;
  /** Gatilho de uma linha, no máximo uma vez por turno (qualquer turno). */
  gatilho?: { quando: 'aliadoEntra' | 'aliadoMorre' | 'aliadoDanoConjurador'; efeitos: Efeito[]; /** Só conta no turno do dono do Conjurador. */ soNoSeuTurno?: boolean };
  /** Passiva: o primeiro dano de combate que um Aliado seu causa em cada turno é N maior. */
  primeiroDanoDeAliadoMais?: number;
  /** Passiva: a primeira carta que você joga em cada turno custa N a menos. */
  primeiraCartaMaisBarata?: number;
  /** Passiva: a primeira carta Repentina que você joga em cada turno custa N a menos. */
  primeiraRepentinaMaisBarata?: number;
  /** Passiva: seus Aliados têm Protegido N. */
  aliadosProtegidos?: number;
}

export interface ConjuradorDef {
  id: string;
  nome: string;
  valores: string[];
  comprometimento: 'Invocador' | 'Brigão' | 'Parrudão da quebrada' | 'Habilidoso' | 'Preparado';
  vida: number;
  maoInicial: number;
  compraFinal: number;
  danoBasico: number;
  custoAtaque: number;
  habilidade: string;
  /** Versão mecânica da habilidade (o texto fica em `habilidade`). */
  poder?: PoderConjurador;
}

export interface Catalogo {
  cartas: Record<string, CartaDef>;
  conjuradores: Record<string, ConjuradorDef>;
}

export interface BaralhoDef {
  conjurador: string;
  cartas: string[];
  /** Itens equipados na preparação (no máximo um por espaço: Canal, Armadura, Extra). */
  itens?: string[];
}

export interface Inst {
  id: string;
  defId: string;
  ehConjurador: boolean;
  dono: Jogador;
  suspensa: boolean;
  dano: number;
  revelada: boolean;
  /** Turno em que entrou em campo (0 = nunca entrou). */
  entrouNoTurno: number;
  /** Na zona de banimento: único turno (global) em que o dono pode jogá-la (0 = nunca). */
  banidaDisponivel: number;
  /** Desconto ao jogá-la a partir do banimento. */
  descontoBanimento: number;
  /** Quando resolver, vai para o banimento em vez do cemitério. */
  banirAoResolver: boolean;
}

/** Todo dano é dano; só muda a fonte. Efeitos podem ser anulados; combate não. */
export type FonteDano = 'efeito' | 'combate';

/** Estado por turno das habilidades de Conjurador, por jogador. */
export interface HabUsos {
  gatilho: boolean; carta: boolean; repentina: boolean;
  /** Desconto pendente para a primeira carta do turno. */
  desconto: number;
  /** Pode jogar do cemitério uma carta que anula (Mitonho). */
  anularDoCemiterio: boolean;
  /** Já aplicou o bônus de primeiro dano de Aliado neste turno. */
  danoAliado: boolean;
  /** A Armadura já protegeu um ataque neste turno (nenhuma protege mais de um ataque por turno). */
  armadura: boolean;
  /** Foco Arcano: bônus para o próximo efeito de dano ou cura de uma Temporária neste turno. */
  potencia: number;
  /** Ataque extra do Conjurador neste turno. */
  ataqueExtra: number;
}

export interface ZonasJogador {
  conjuradorId: string;
  baralho: string[];
  mao: string[];
  campo: string[];
  cemiterio: string[];
  grau: string[];
  zonaArcano: string[];
  /** Zona de banimento: cartas fora de jogo; algumas podem ser jogadas no turno indicado em Inst.banidaDisponivel. */
  banimento: string[];
  /** Itens equipados no Conjurador (instâncias). Na Armadura, Inst.dano guarda o desgaste. */
  itens: string[];
  arcano: number;
}

export type ItemPilha =
  | { tipo: 'carta'; id: string; controlador: Jogador; instId: string; alvo?: string }
  | { tipo: 'gatilho'; id: string; controlador: Jogador; efeitos: Efeito[]; alvo?: string; /** Nome de quem gerou o gatilho (registro). */ fonte?: string }
  | { tipo: 'ataque'; id: string; controlador: Jogador; atacante: string; alvo: string };

export type Pendencia =
  | { tipo: 'escolherInicio'; jogador: Jogador }
  | { tipo: 'descartar'; jogador: Jogador; quantidade: number }
  | { tipo: 'bloqueio'; jogador: Jogador }
  /** Vários bloqueadores: quem ataca distribui o dano entre eles (decisão 20). */
  | { tipo: 'distribuirDano'; jogador: Jogador }
  | { tipo: 'pagarAnulacao'; jogador: Jogador; custo: number; item: ItemPilha }
  | { tipo: 'escolherDoTopo'; jogador: Jogador; cartas: string[]; pegar: number };

export interface Combate {
  atacante: string;
  alvo: string;
  /** Primeiro bloqueador (compatibilidade com a interface); null sem bloqueio. */
  bloqueador: string | null;
  /** Todos os bloqueadores (decisão 20: vários podem bloquear o mesmo ataque ao Conjurador). */
  bloqueadores: string[];
  /** Dano do atacante por bloqueador, escolhido por quem ataca; o excedente de cada um vai para o Conjurador. */
  distribuicao?: Record<string, number>;
  /** Conjurador bloqueando: o dono escolheu usar a Armadura (padrão: usa). */
  usarArmadura?: boolean;
  etapa: 'declarado' | 'aguardandoBloqueio' | 'bloqueado';
}

export interface Estado {
  catalogo: Catalogo;
  jogadores: [ZonasJogador, ZonasJogador];
  instancias: Record<string, Inst>;
  pilha: ItemPilha[];
  turno: number;
  ativo: Jogador;
  etapa: Etapa | 'preparacao';
  prioridade: Jogador;
  passes: number;
  pendencia: Pendencia | null;
  combate: Combate | null;
  vencedor: Jogador | 'empate' | null;
  dados: [number, number];
  regras: Regras;
  /** Quem começou a partida. */
  primeiro: Jogador;
  /** Já pôs carta no Grau neste turno (regra grauSemSuspender). */
  grauNoTurno: boolean;
  /** Turnos seguidos de cada jogador sem jogar carta, ativar nem atacar (relógio de impasse). */
  semAcaoTurnos: [number, number];
  agiuNoTurno: boolean;
  /** Turnos (globais) em que cada jogador causou dano a algo do oponente; só estatística. */
  turnosDano: [number[], number[]];
  /** Habilidade do Conjurador já usada neste turno (gatilho, 1ª carta, 1ª repentina), por jogador. */
  habUsos: [HabUsos, HabUsos];
  rng: number;
  proxId: number;
  log: string[];
}

export type Comando =
  | { tipo: 'escolherInicio'; jogador: Jogador; primeiro: Jogador }
  | { tipo: 'colocarGrau'; jogador: Jogador; carta: string }
  | { tipo: 'suspenderGrau'; jogador: Jogador; carta: string }
  | { tipo: 'jogar'; jogador: Jogador; carta: string; alvo?: string; descartarGrau?: string[]; enviarArcano?: string[] }
  | { tipo: 'atacar'; jogador: Jogador; atacante: string; alvo: string; enviarArcano?: string[] }
  | { tipo: 'bloquear'; jogador: Jogador; /** Um bloqueador (null: não bloqueia). */ bloqueador?: string | null; /** Vários bloqueadores (decisão 20); vale no lugar de bloqueador. */ bloqueadores?: string[]; /** Conjurador bloqueando: usar a Armadura? Sem o campo, usa. */ usarArmadura?: boolean }
  | { tipo: 'distribuirDano'; jogador: Jogador; /** Dano por bloqueador (soma = Ataque do atacante). Sem o campo: distribuição padrão, que derruba o máximo de bloqueadores. */ distribuicao?: Record<string, number> }
  | { tipo: 'descartar'; jogador: Jogador; cartas: string[] }
  | { tipo: 'ativar'; jogador: Jogador; carta: string; alvo?: string; enviarArcano?: string[]; /** Cartas da mão descartadas como custo (Ativacao.descartar). */ descartarCusto?: string[] }
  | { tipo: 'pagarAnulacao'; jogador: Jogador; pagar: boolean; enviarArcano?: string[] }
  | { tipo: 'escolherDoTopo'; jogador: Jogador; cartas: string[] }
  | { tipo: 'passar'; jogador: Jogador };
