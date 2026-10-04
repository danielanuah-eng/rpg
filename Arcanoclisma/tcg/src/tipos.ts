// Tipos do motor do Arcanoclisma TCG. Só sintaxe "apagável" (sem enum), para rodar direto no Node 24.

export type Jogador = 0 | 1;
export type Etapa = 'inicio' | 'principal' | 'cataclisma' | 'final';
/** 'item': equipamento do Conjurador (Canal, Armadura ou Extra), escolhido na preparação, fora do baralho. */
export type TipoCarta = 'aliado' | 'temporaria' | 'mistica' | 'estrutura' | 'item';
export type CategoriaItem = 'canal' | 'armadura' | 'extra';
export type Velocidade = 'findante' | 'repentina';
export type AlvoEfeito = 'escolhido' | 'escolhidoAliado' | 'conjuradorInimigo' | 'conjuradorProprio';

// ---------- construtor de efeitos (pedido do autor, 04/10) ----------
/** Quando uma habilidade acontece. */
export type Quando = 'jogar' | 'entrar' | 'ativada' | 'alvo' | 'grau' | 'atacar' | 'bloquear' | 'cemiterio' | 'banimento' | 'danoConjurador'
  /** Só de Conjurador (Editor, 04/10): um Aliado seu entra em campo / vai do campo para o cemitério / causa dano de combate ao Conjurador inimigo. */
  | 'aliadoEntra' | 'aliadoMorre' | 'aliadoDanoConjurador'
  /** Pedido do autor (04/10, noite): na etapa de Cataclisma do turno do oponente (fim do turno dele). Cartas em campo, itens e Conjurador. */
  | 'cataclismaOponente';
/** Custo de uma habilidade ativada ("virar/pagar"). */
export interface CustoHabilidade { suspender?: boolean; arcano?: number; vida?: number; descartar?: number; grau?: number }
/** Habilidade montada no Editor: condição + efeitos (que se acumulam, em ordem). */
export interface Habilidade {
  quando: Quando; custo?: CustoHabilidade; efeitos: Efeito[];
  /** Gatilho só na primeira vez em cada turno. */ umaVezPorTurno?: boolean;
  /** Gatilho só no turno do dono (Editor de Conjuradores, 04/10). */ soNoSeuTurno?: boolean;
  /** Ativada de Conjurador com velocidade Repentina: a qualquer momento, com prioridade (as demais: só na sua Principal, pilha vazia). */ repentina?: boolean;
  /** Requisito da habilidade ativada: Grau mínimo e Arcano a mais (soma ao custo). */ requisito?: Requisito;
}
/**
 * Protegido (unificado em 04/10, noite; antes havia "Protegido N" e "Proteção mágica"): quem mira esta carta com carta ou
 * ativação do oponente paga isto; sem poder pagar, não pode mirar. `protegido: N` (número) é o mesmo que `{ arcano: N }`.
 */
export interface Protegido { arcano?: number; vida?: number; descartar?: number; grauParaCemiterio?: number; grauMin?: number }
/** @deprecated Nome antigo ("Proteção mágica"); use Protegido. */
export type Protecao = Protegido;
/** Para onde vão as cartas escolhidas. */
export type Destino = 'mao' | 'roubar' | 'grau' | 'cemiterio' | 'fundo' | 'topo' | 'campo' | 'banimento' | 'nada';
export type TipoAlvoCampo = 'aliado' | 'mistica' | 'estrutura' | 'conjurador' | 'equipamento';
/**
 * Concessão (pedido do autor, 04/10): palavras-chave e/ou condições dadas a uma carta em campo, definidas nos dados.
 * palavras: qualquer palavra-chave que o motor entende ("Etéreo", "Protegido 2", "Reanimum"...); ataque/vida: modificador
 * permanente enquanto está em campo (pode ser negativo); turnos: fica X turnos do seu controlador em campo (conta o da
 * entrada); destino: para onde vai ao sair por turnos (padrão: cemitério).
 */
export interface Concessao { palavras?: string[]; /** As palavras duram N turnos do dono da carta (o turno em que são dadas conta como o 1º); vazio = permanentes. */ palavrasTurnos?: number; ataque?: number; vida?: number; turnos?: number; destino?: 'cemiterio' | 'banimento' | 'baralho' | 'mao' }
export type AcaoCampo = 'conceder' | 'dano' | 'curar' | 'destruir' | 'devolver' | 'paraBaralho' | 'naoAtaca' | 'suspender' | 'levantar' | 'aumentarAtaque' | 'aumentarVida';

/**
 * Requisito de uma linha de efeito, habilidade ou ação (pedido do autor, 04/10), definido no Editor:
 * grau = mínimo de cartas no seu Grau para acontecer; arcano = Arcano pago na hora (reserva, depois Grau levantado).
 * Sem cumprir, aquela linha não acontece (as outras da carta continuam).
 */
export interface Requisito { grau?: number; arcano?: number }

/**
 * Quem escolhe o alvo de um efeito de alvo único (pedido do autor, 04/10): quem jogou/ativou (padrão), o jogador atingido
 * ou ao acaso. Nada escolhe o alvo sozinho; "atinge tudo" é o alcance "todos".
 */
export type QuemEscolhe = 'controlador' | 'afetado' | 'aleatorio';
/**
 * Efeito secundário (pedido do autor, 04/10, noite): custo ou consequência que acompanha outro efeito, como linha extra
 * ("...; depois, seu Conjurador toma 2 de dano"). Só muda o texto e a avaliação do bot; o motor resolve em ordem.
 */
export type Efeito = EfeitoBase & { requisito?: Requisito; escolha?: QuemEscolhe; secundario?: boolean };
/** Quem sofre o "tirar da mão": quem jogou/ativou, o oponente ou os dois. */
export type QuemAfeta = 'voce' | 'oponente' | 'ambos';
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
  /**
   * Estendido em 04/10 (noite). pagamento: 'gratis', 'custo' (o custo do Aliado) ou 'custoMais' (custo + custoExtra); sem
   * `pagamento` = custo + custoExtra (formato antigo). de: 'seu' | 'oponente' | 'qualquer' (padrão). Filtros: custoMax, subtipo.
   * Quem controla o efeito escolhe ao resolver (escolherCartas) e paga na hora (reserva, depois Grau levantado); sem poder
   * pagar, não traz nada. Ativação com o alvo já escolhido no comando (formato antigo): paga ao ativar.
   */
  | { tipo: 'reanimar'; custoExtra: number; pagamento?: 'gratis' | 'custo' | 'custoMais'; de?: 'seu' | 'oponente' | 'qualquer'; custoMax?: number; subtipo?: string; concessao?: Concessao; /** Interno: escolhido ao resolver, paga agora. */ cobrarNaHora?: boolean; /** Interno: alvo escolhido e pago ao ativar (formato antigo). */ alvoPago?: boolean; /** Interno: cartas da mão enviadas ao Arcano para pagar (escolherCartas.enviarArcano). */ pagarCom?: string[] }
  /** Rodada 7. Neste turno você pode jogar do seu cemitério uma carta cujo efeito anule outra, pagando o custo dela; ao resolver ela é banida. */
  | { tipo: 'permitirAnularDoCemiterio' }
  /** Olhe as N cartas do topo do seu baralho, ponha `pegar` delas na mão e o resto no fundo do baralho. */
  | { tipo: 'olharTopo'; quantidade: number; pegar: number }
  /** Itens (03/10, noite). Foco Arcano: neste turno, o próximo efeito de dano ou cura de uma Temporária sua tem +N. */
  | { tipo: 'potenciaMagia'; valor: number }
  /** Itens (03/10, noite). Põe no seu campo uma ficha (carta do catálogo que não vai em baralho, ex.: Espírito Invocado). */
  | { tipo: 'invocar'; carta: string; /** Nome para o texto da carta. */ nome?: string; /** A ficha já entra com isto. */ concessao?: Concessao }
  /** Itens (03/10, noite). Seu Conjurador ganha +N de Ataque neste turno. */
  | { tipo: 'ataqueConjuradorTurno'; valor: number }
  /**
   * Construtor (04/10); "tirar da mão" (04/10, noite). Tira N cartas da mão de um jogador (você, o oponente ou os dois):
   * acao 'descartar' (cemitério, padrão) ou 'banir' (banimento). modo: 'acaso'; 'dono' (o dono escolhe, sem mostrar; padrão);
   * 'olhando' (quem controla o efeito olha a mão e escolhe). `aleatorio: true` (formato antigo) = modo 'acaso'.
   */
  | { tipo: 'descartar'; quem: QuemAfeta; valor: number; aleatorio?: boolean; acao?: 'descartar' | 'banir'; modo?: 'acaso' | 'dono' | 'olhando' }
  /** Construtor. Põe N cartas no seu Grau, do topo do baralho ou da mão (você escolhe). */
  | { tipo: 'colocarNoGrau'; de: 'topo' | 'mao'; valor: number }
  /** Construtor. Olha as N cartas do topo: escolhe `pegar` para `destino`; o resto vai para `resto`. */
  | { tipo: 'olharTopoPara'; quantidade: number; pegar: number; destino: Destino; resto: Destino }
  /** Construtor. Olha a mão do oponente e escolhe `pegar` cartas para `destino` (cemitério, fundo do baralho, roubar). */
  | { tipo: 'olharMao'; pegar: number; destino: Destino }
  /** Construtor. Põe em campo (sem pagar) até N cartas da zona, dos tipos e custo máximo indicados. */
  | { tipo: 'jogarDe'; de: 'cemiterio' | 'mao' | 'baralho' | 'banimento'; valor: number; tipos?: TipoCarta[]; custoMax?: number; concessao?: Concessao }
  /** Construtor. Afeta o campo: um alvo escolhido ou todos de um lado, dos tipos indicados. */
  | { tipo: 'campo'; acao: AcaoCampo; valor?: number; alcance: 'alvo' | 'todos'; lado: 'inimigo' | 'proprio' | 'ambos'; tipos: TipoAlvoCampo[]; duracao?: 'turno' | 'permanente'; onde?: 'topo' | 'fundo' | 'embaralhar'; /** Ação 'conceder'. */ concessao?: Concessao }
  /** Construtor. Seu Conjurador perde N de Vida (custo de outro efeito, por exemplo). */
  | { tipo: 'perderVida'; valor: number; /** Efeito secundário: 'oponente' = o Conjurador do oponente. */ quem?: 'voce' | 'oponente' }
  /**
   * Efeito secundário (04/10, noite). O jogador sacrifica N Aliados dele (só Aliado; autor, 04/10 noite), se tiver: o dono
   * escolhe e elas vão do campo para o cemitério sem ganhar nada. Não mira (ignora Protegido e Protessência); conta como morte.
   */
  | { tipo: 'sacrificar'; valor: number; quem?: 'voce' | 'oponente' }
  /** Efeito secundário (04/10, noite). O jogador manda N cartas do Grau dele ao cemitério (o dono escolhe; com menos, manda as que tiver). */
  | { tipo: 'grauAoCemiterio'; valor: number; quem?: 'voce' | 'oponente' };

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
  /** Palavras-chave. 'Proficiente': ataca e usa ativações que suspendem no turno em que entra. 'Etéreo': só Etéreos (e armas Etéreas) o atingem e o bloqueiam em combate (decisão 24); 'Incomensurável': o dano que passa da Vida vai para o Conjurador (decisão 25). Antes: só é bloqueado por Aliados Etéreos. */
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
  /**
   * Protegido (rodada 6; unificado em 04/10): número N = o oponente paga N de Arcano a mais para mirar esta carta com uma
   * carta ou ativação; objeto = Arcano e/ou Vida, descarte, Grau ao cemitério e Grau mínimo (as variações da antiga "Proteção mágica").
   */
  protegido?: number | Protegido;
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
  /** Construtor de efeitos (04/10): habilidades com condição e efeitos. */
  habilidades?: Habilidade[];
  /** @deprecated Nome antigo de Protegido ("Proteção mágica"): ainda carrega e é convertido em `protegido` (normalizarProtegido). */
  protecao?: Protegido;
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
  /** Passiva (Editor, 04/10): você pode jogar cartas destas zonas, pagando o custo normal. */
  permiteJogarDe?: PermissaoJogar[];
  /**
   * Editor de Conjuradores (04/10): habilidades no mesmo formato das cartas (construtor de efeitos), além da ativação e do
   * gatilho acima. Condições: ativada (com `repentina`), aliadoEntra/aliadoMorre/aliadoDanoConjurador (com `umaVezPorTurno` e
   * `soNoSeuTurno`) e as do próprio Conjurador: atacar, bloquear, alvo, danoConjurador. Ativar: comando `ativar` com `habilidade`.
   */
  habilidades?: Habilidade[];
}

/**
 * Permissão contínua do Conjurador (Editor, 04/10): pode jogar do seu cemitério ou banimento cartas dos tipos indicados
 * (padrão: qualquer uma) e custo até `custoMax`, pagando o custo normal, na velocidade normal da carta. `banir`: a carta
 * jogada assim vai para o banimento ao resolver ou ao sair do campo (como o Reanimum). `umaVezPorTurno`: só uma carta por turno.
 */
export interface PermissaoJogar { de: 'cemiterio' | 'banimento'; tipos?: TipoCarta[]; custoMax?: number; banir?: boolean; umaVezPorTurno?: boolean }

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
  /** Construtor (04/10): bônus permanentes e até o fim do turno. */
  bonusAtaque?: number;
  bonusVida?: number;
  bonusAtaqueTurno?: number;
  bonusVidaTurno?: number;
  /** Não pode atacar enquanto e.turno <= este número. */
  naoAtacaAte?: number;
  /** Habilidades "uma vez por turno" já usadas: índice → turno. */
  usosHab?: Record<number, number>;
  /** Concessão: palavras-chave extras enquanto está em campo. */
  palavrasExtra?: string[];
  /** Concessão com palavras por N turnos (palavrasTurnos): somem no Final do último turno. */
  palavrasTemp?: Array<{ palavras: string[]; restantes: number; conta?: number }>;
  /** Concessão: turnos do controlador que ainda faltam (conta o atual); some no Final do último. Visível para a tela. */
  turnosRestantes?: number;
  /** Concessão: para onde vai quando o tempo acaba. */
  destinoSaida?: 'cemiterio' | 'banimento' | 'baralho' | 'mao';
  /** Concessão com tempo dada no turno do oponente: esse turno (global) conta como o 1º. */
  turnoEntradaConta?: number;
  /** Jogada pelo Reanimum: ao sair do campo vai para o banimento. */
  banirAoSair?: boolean;
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
  /** Permissões do Conjurador (`permiteJogarDe` uma vez por turno) já usadas neste turno: índices. */
  permissoes?: number[];
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
  | { tipo: 'gatilho'; id: string; controlador: Jogador; efeitos: Efeito[]; alvo?: string; /** Nome de quem gerou o gatilho (registro). */ fonte?: string; /** A fonte tem Incomensurável: o dano que passar da Vida vai para o Conjurador (decisão 25). */ incomensuravel?: boolean }
  | { tipo: 'ataque'; id: string; controlador: Jogador; atacante: string; alvo: string };

export type Pendencia =
  | { tipo: 'escolherInicio'; jogador: Jogador }
  | { tipo: 'descartar'; jogador: Jogador; quantidade: number }
  | { tipo: 'bloqueio'; jogador: Jogador }
  /** Vários bloqueadores: quem ataca distribui o dano entre eles (decisão 20). */
  | { tipo: 'distribuirDano'; jogador: Jogador }
  | { tipo: 'pagarAnulacao'; jogador: Jogador; custo: number; item: ItemPilha }
  | { tipo: 'escolherDoTopo'; jogador: Jogador; cartas: string[]; pegar: number }
  /** Construtor (04/10): escolher cartas de uma lista e mandá-las a um destino; o resto (se saiu da zona) vai para `resto`. */
  | { tipo: 'escolherCartas'; jogador: Jogador; cartas: string[]; quantidade: number; destino: Destino; resto?: Destino; titulo: string; depois?: Continuacao; /** Escolha de alvo: aplica este efeito em cada carta escolhida. */ aplicar?: { efeito: Efeito; dono: Jogador }; /** Concessão dada às cartas que entram em campo por esta escolha. */ concessao?: Concessao };

/** Efeitos que esperam uma escolha terminar para continuar (efeitos acumulados). */
export interface Continuacao { efeitos: Efeito[]; dono: Jogador; alvo?: string }

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
  | { tipo: 'jogar'; jogador: Jogador; carta: string; alvo?: string; descartarGrau?: string[]; enviarArcano?: string[]; /** Protegido do alvo (nome antigo dos campos mantido): cartas da mão e do Grau que pagam. */ protecaoDescartar?: string[]; protecaoGrau?: string[] }
  | { tipo: 'atacar'; jogador: Jogador; atacante: string; alvo: string; enviarArcano?: string[] }
  | { tipo: 'bloquear'; jogador: Jogador; /** Um bloqueador (null: não bloqueia). */ bloqueador?: string | null; /** Vários bloqueadores (decisão 20); vale no lugar de bloqueador. */ bloqueadores?: string[]; /** Conjurador bloqueando: usar a Armadura? Sem o campo, usa. */ usarArmadura?: boolean }
  | { tipo: 'distribuirDano'; jogador: Jogador; /** Dano por bloqueador (soma = Ataque do atacante). Sem o campo: distribuição padrão, que derruba o máximo de bloqueadores. */ distribuicao?: Record<string, number> }
  | { tipo: 'descartar'; jogador: Jogador; cartas: string[] }
  | { tipo: 'ativar'; jogador: Jogador; carta: string; alvo?: string; enviarArcano?: string[]; /** Cartas da mão descartadas como custo (Ativacao.descartar). */ descartarCusto?: string[]; /** Habilidade ativada do construtor (índice em habilidades). */ habilidade?: number; /** Proteção mágica do alvo: cartas da mão e do Grau pagas. */ protecaoDescartar?: string[]; protecaoGrau?: string[] }
  | { tipo: 'pagarAnulacao'; jogador: Jogador; pagar: boolean; enviarArcano?: string[] }
  | { tipo: 'escolherDoTopo'; jogador: Jogador; cartas: string[] }
  | { tipo: 'escolherCartas'; jogador: Jogador; cartas: string[]; /** Reanimar com custo (04/10, noite): cartas da mão enviadas ao Arcano para pagar o Aliado escolhido. */ enviarArcano?: string[] }
  | { tipo: 'passar'; jogador: Jogador };
