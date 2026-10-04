// Tabelas do construtor de efeitos do Editor (pedido do autor, 04/10): condições, efeitos, palavras-chave e passivas.
// Sem dependências e sem DOM: o construtor (construtor-efeitos.js) desenha a partir daqui e os testes do site
// (online/test/construtor.test.ts) conferem que toda mecânica dos Conjuradores e das cartas tem linha na tabela.

const TIPOS_CAMPO = [['aliado', 'Aliado'], ['mistica', 'Mística'], ['estrutura', 'Estrutura'], ['conjurador', 'Conjurador'], ['equipamento', 'Equipamento']];
/** Destruir, devolver e mandar ao baralho nunca miram o Conjurador (decisão 28): a lista de tipos não o oferece. */
const ACOES_SEM_CONJURADOR = ['destruir', 'devolver', 'paraBaralho'];
const tiposCampoDe = (acao) => (ACOES_SEM_CONJURADOR.includes(acao) ? TIPOS_CAMPO.filter(([k]) => k !== 'conjurador') : TIPOS_CAMPO);
const DESTINOS = [['mao', 'Mão'], ['grau', 'Grau'], ['cemiterio', 'Cemitério'], ['fundo', 'Fundo do baralho'], ['topo', 'Topo do baralho'], ['campo', 'Campo (jogar)'], ['banimento', 'Banimento']];
const LADO = [['inimigo', 'Do oponente'], ['proprio', 'Seu'], ['ambos', 'Dos dois']];
const ALCANCE = [['alvo', 'Um alvo'], ['todos', 'Todos (atinge tudo)']];
const DURACAO = [['turno', 'Neste turno'], ['permanente', 'Permanente']];
const TIPOS_JOGAVEIS = [['aliado', 'Aliado'], ['temporaria', 'Temporária'], ['mistica', 'Mística'], ['estrutura', 'Estrutura']];

/** Tabela de condições ("quando") das cartas. */
export const CONDICOES = [
  ['jogar', 'Ao jogar (Temporária)'],
  ['entrar', 'Quando entra em campo'],
  ['ativada', 'Virar / pagar (habilidade ativada)'],
  ['alvo', 'Quando vira alvo'],
  ['grau', 'Quando é colocada no Grau'],
  ['atacar', 'Quando ataca'],
  ['bloquear', 'Quando defende (bloqueia)'],
  ['cemiterio', 'Quando cai no cemitério'],
  ['banimento', 'Quando é banida'],
  ['danoConjurador', 'Quando causa dano de combate ao Conjurador inimigo'],
  ['cataclismaOponente', 'No Cataclisma do turno do oponente (fim do turno dele)'],
];
/** Condições do poder de um Conjurador (Editor de Conjuradores, 04/10). */
export const CONDICOES_CONJURADOR = [
  ['ativada', 'Suspender / pagar (habilidade ativada)'],
  ['aliadoEntra', 'Quando um Aliado seu entra em campo'],
  ['aliadoMorre', 'Quando um Aliado seu vai do campo para o cemitério'],
  ['aliadoDanoConjurador', 'Quando um Aliado seu causa dano de combate ao Conjurador inimigo'],
  ['atacar', 'Quando o Conjurador ataca'],
  ['bloquear', 'Quando o Conjurador defende (bloqueia)'],
  ['alvo', 'Quando o Conjurador vira alvo'],
  ['danoConjurador', 'Quando o Conjurador causa dano de combate ao Conjurador inimigo'],
  ['cataclismaOponente', 'No Cataclisma do turno do oponente (fim do turno dele)'],
];

/** Campo do formulário de um efeito: [chave, rótulo, tipo, opções/padrão]. */
const n = (k, rot, padrao = 1) => ({ k, rot, tipo: 'num', padrao });
const sel = (k, rot, opcoes, padrao = typeof opcoes === 'function' ? undefined : opcoes[0][0]) => ({ k, rot, tipo: 'sel', opcoes, padrao });
/** Campo de texto livre (ex.: subtipo). */
const txt = (k, rot) => ({ k, rot, tipo: 'txt', padrao: '' });
/** `ler(ef)`: valor mostrado quando o efeito está no formato antigo (ex.: `aleatorio: true` = modo "ao acaso"). */
const comLeitura = (c, ler) => ({ ...c, ler });
/** Concessão (04/10): palavras-chave, ±Ataque/±Vida, tempo em campo e destino ao sair. Vira o objeto `concessao` do efeito. */
const conc = (k, rot) => ({ k, rot, tipo: 'conc' });
export const PALAVRAS_CONCESSAO = ['Etéreo', 'Proficiente', 'Incomensurável', 'Protessência', 'Reanimum', 'Protegido 1', 'Protegido 2', 'Protegido 3'];
export const DESTINO_SAIDA = [['', 'Cemitério (padrão)'], ['banimento', 'Banimento'], ['baralho', 'Baralho (embaralha)'], ['mao', 'Mão do dono']];
/** Fichas do catálogo (cartas com `ficha`), enviadas pelo Editor: [id, nome]. */
let FICHAS = [];
export const definirFichas = (f) => { FICHAS = f ?? []; };
export const opcoesDe = (c) => (typeof c.opcoes === 'function' ? c.opcoes() : c.opcoes);
const multi = (k, rot, opcoes, padrao) => ({ k, rot, tipo: 'multi', opcoes, padrao });
const bool = (k, rot) => ({ k, rot, tipo: 'bool', padrao: false });
const ESCOLHA = [['controlador', 'Quem jogou/ativou escolhe'], ['afetado', 'O jogador atingido escolhe'], ['aleatorio', 'Ao acaso']];
const campoPadrao = (acao, extra = []) => ({ fixo: { tipo: 'campo', acao }, campos: [...extra, sel('alcance', 'Quem', ALCANCE), sel('escolha', 'Quem escolhe o alvo', ESCOLHA), sel('lado', 'Lado', LADO, acao === 'curar' || acao === 'conceder' || acao.startsWith('aumentar') || acao === 'levantar' ? 'proprio' : 'inimigo'), multi('tipos', 'Tipos', tiposCampoDe(acao), ['aliado'])] });
/** "Tirar da mão" (04/10, noite): ação, modo (o formato antigo `aleatorio: true` abre como "ao acaso") e quem sofre o secundário. */
const ACAO_MAO = sel('acao', 'Ação', [['descartar', 'Descartar (cemitério)'], ['banir', 'Banir']]);
const MODO_MAO = [['dono', 'O dono escolhe (sem mostrar)'], ['acaso', 'Ao acaso'], ['olhando', 'Quem jogou olha a mão e escolhe']];
const modoMao = (ef) => ef.modo ?? (ef.aleatorio ? 'acaso' : 'dono');
const QUEM_SEC = sel('quem', 'Quem', [['voce', 'Quem jogou/ativou'], ['oponente', 'O oponente']]);
const ALVO_DANO = [['escolhidoAliado', 'Um Aliado ou Mística'], ['escolhido', 'Qualquer alvo (inclui Conjurador)'], ['conjuradorInimigo', 'O Conjurador inimigo'], ['conjuradorProprio', 'O seu Conjurador']];

/**
 * Tabela de efeitos (rótulo → mecânica do motor). `soEm`: condições em que o efeito funciona (o alvo é escolhido ao
 * ativar/jogar). `legado`: formato antigo (o efeito "campo" faz o mesmo); só aparece na lista quando já está na carta.
 */
export const EFEITOS = {
  dano: { rot: 'Dar dano', ...campoPadrao('dano', [n('valor', 'Quanto', 2)]) },
  curar: { rot: 'Curar', ...campoPadrao('curar', [n('valor', 'Quanto', 2)]) },
  destruir: { rot: 'Destruir', ...campoPadrao('destruir') },
  devolver: { rot: 'Voltar para a mão do dono', ...campoPadrao('devolver') },
  paraBaralho: { rot: 'Voltar para o baralho', ...campoPadrao('paraBaralho', [sel('onde', 'Onde', [['embaralhar', 'Embaralhar'], ['fundo', 'Fundo'], ['topo', 'Topo']])]) },
  naoAtaca: { rot: 'Proibir de atacar (até o próximo turno do dono)', ...campoPadrao('naoAtaca') },
  suspender: { rot: 'Virar (suspender)', ...campoPadrao('suspender') },
  levantar: { rot: 'Levantar', ...campoPadrao('levantar') },
  aumentarAtaque: { rot: 'Aumentar Ataque', ...campoPadrao('aumentarAtaque', [n('valor', 'Quanto', 1), sel('duracao', 'Duração', DURACAO)]) },
  aumentarVida: { rot: 'Aumentar Vida', ...campoPadrao('aumentarVida', [n('valor', 'Quanto', 2), sel('duracao', 'Duração', DURACAO)]) },
  conceder: { rot: 'Conceder palavras-chave / condições', ...campoPadrao('conceder', [conc('concessao', 'Concede')]) },
  invocar: { rot: 'Invocar uma ficha', fixo: { tipo: 'invocar' }, campos: [sel('carta', 'Ficha', () => FICHAS), conc('concessao', 'A ficha entra com')] },
  comprar: { rot: 'Comprar cartas', fixo: { tipo: 'comprar' }, campos: [n('valor', 'Quantas', 1)] },
  arcano: { rot: 'Gerar Arcano', fixo: { tipo: 'arcano' }, campos: [n('valor', 'Quanto', 1)] },
  arcanoOponente: { rot: 'O oponente ganha Arcano', fixo: { tipo: 'arcanoOponente' }, campos: [n('valor', 'Quanto', 1)] },
  olharTopoPara: { rot: 'Olhar cartas do topo do baralho', fixo: { tipo: 'olharTopoPara' }, campos: [n('quantidade', 'Olhar', 3), n('pegar', 'Escolher', 1), sel('destino', 'As escolhidas vão para', DESTINOS), sel('resto', 'O resto vai para', [['fundo', 'Fundo do baralho'], ['cemiterio', 'Cemitério'], ['topo', 'Topo (mesma ordem)'], ['grau', 'Grau'], ['banimento', 'Banimento']])] },
  // "tirar da mão" (04/10, noite): descartar ou banir; de quem; ao acaso, o dono escolhe sem mostrar, ou quem jogou olha e escolhe
  descartar: { rot: 'Tirar cartas da mão (descartar ou banir)', fixo: { tipo: 'descartar' }, campos: [ACAO_MAO, sel('quem', 'De quem', [['oponente', 'O oponente'], ['voce', 'Quem jogou/ativou'], ['ambos', 'Os dois']]), n('valor', 'Quantas', 1), comLeitura(sel('modo', 'Como', MODO_MAO), modoMao)] },
  colocarNoGrau: { rot: 'Colocar carta extra no Grau', fixo: { tipo: 'colocarNoGrau' }, campos: [sel('de', 'De onde', [['topo', 'Topo do baralho'], ['mao', 'Uma da mão (você escolhe)']]), n('valor', 'Quantas', 1)] },
  olharMao: { rot: 'Olhar a mão do oponente', fixo: { tipo: 'olharMao' }, campos: [n('pegar', 'Escolher', 1), sel('destino', 'Fazer o quê com as escolhidas', [['cemiterio', 'Descartar (cemitério)'], ['roubar', 'Pegar para você'], ['fundo', 'Mandar para o fundo do baralho'], ['nada', 'Só olhar']])] },
  jogarDe: { rot: 'Jogar carta de uma zona (sem pagar)', fixo: { tipo: 'jogarDe' }, campos: [sel('de', 'De onde', [['cemiterio', 'Cemitério'], ['mao', 'Mão'], ['baralho', 'Baralho'], ['banimento', 'Banimento']]), n('valor', 'Quantas', 1), multi('tipos', 'Tipos', [['aliado', 'Aliado'], ['mistica', 'Mística'], ['estrutura', 'Estrutura']], ['aliado']), n('custoMax', 'Custo máximo (vazio = qualquer)', ''), conc('concessao', 'As cartas entram com')] },
  perderVida: { rot: 'Seu Conjurador perde Vida', fixo: { tipo: 'perderVida' }, campos: [n('valor', 'Quanto', 1)] },
  // Efeitos secundários (04/10, noite): custos/consequências depois de outro efeito, sobre quem jogou/ativou (ou o oponente)
  secDano: { rot: 'O Conjurador toma dano', grupo: 'secundario', fixo: { tipo: 'perderVida', secundario: true }, campos: [n('valor', 'Quanto', 1), QUEM_SEC] },
  secDescartar: { rot: 'Tirar cartas da própria mão (descartar ou banir)', grupo: 'secundario', fixo: { tipo: 'descartar', secundario: true }, campos: [ACAO_MAO, n('valor', 'Quantas', 1), comLeitura(sel('modo', 'Como', MODO_MAO.filter(([k]) => k !== 'olhando')), modoMao), sel('quem', 'Quem', [['voce', 'Quem jogou/ativou'], ['oponente', 'O oponente']])] },
  secSacrificar: { rot: 'Sacrifique um Aliado (o dono escolhe; não mira)', grupo: 'secundario', fixo: { tipo: 'sacrificar', secundario: true }, campos: [n('valor', 'Quantas', 1), QUEM_SEC] },
  secGrau: { rot: 'Mandar cartas do Grau ao cemitério (o dono escolhe)', grupo: 'secundario', fixo: { tipo: 'grauAoCemiterio', secundario: true }, campos: [n('valor', 'Quantas', 1), QUEM_SEC] },
  anular: { rot: 'Anular uma carta na pilha', fixo: { tipo: 'anular', alvo: 'pilha' }, campos: [] },
  descontoProximaCarta: { rot: 'Desconto na próxima carta do turno', fixo: { tipo: 'descontoProximaCarta' }, campos: [n('valor', 'Quanto', 1)] },
  // Editor de Conjuradores (04/10): efeitos dos 9 Conjuradores e dos itens, que antes só existiam no JSON
  // reanimar (04/10, noite): quem controla escolhe no cemitério ao resolver e paga na hora; funciona em qualquer condição
  reanimar: { rot: 'Pôr em campo um Aliado do cemitério (reanimar)', fixo: { tipo: 'reanimar' }, campos: [
    comLeitura(sel('pagamento', 'Pagamento', [['custo', 'Pagando o custo dele'], ['gratis', 'Sem custo'], ['custoMais', 'Pagando o custo + X']]), (ef) => ef.pagamento ?? (ef.custoExtra ? 'custoMais' : 'custo')),
    n('custoExtra', 'X (em "custo + X")', 0), sel('de', 'De qual cemitério', [['qualquer', 'De qualquer um'], ['seu', 'Do seu'], ['oponente', 'Do oponente']]),
    n('custoMax', 'Custo máximo (vazio = qualquer)', ''), txt('subtipo', 'Subtipo (vazio = qualquer)'), conc('concessao', 'O Aliado entra com')] },
  banirDaMao: { rot: 'Banir uma carta da mão para jogá-la no seu próximo turno com desconto', fixo: { tipo: 'banirDaMao' }, soEm: ['ativada', 'jogar'], campos: [n('desconto', 'Desconto', 4)] },
  permitirAnularDoCemiterio: { rot: 'Neste turno, pode jogar do cemitério uma carta que anula (banida ao resolver)', fixo: { tipo: 'permitirAnularDoCemiterio' }, campos: [] },
  potenciaMagia: { rot: 'Próximo dano/cura de Temporária sua neste turno fica maior', fixo: { tipo: 'potenciaMagia' }, campos: [n('valor', 'Quanto', 1)] },
  ataqueConjuradorTurno: { rot: 'Seu Conjurador ganha Ataque neste turno', fixo: { tipo: 'ataqueConjuradorTurno' }, campos: [n('valor', 'Quanto', 1)] },
  // formato antigo (cartas e Conjuradores de antes do construtor): abre no formulário sem mudar o JSON
  devolverAlvo: { rot: 'Voltar para a mão do dono (formato antigo)', legado: true, fixo: { tipo: 'devolver' }, campos: [sel('alvo', 'Qual', [['escolhidoProprio', 'Um Aliado ou Mística seu'], ['escolhidoAliado', 'Um Aliado ou Mística'], ['escolhido', 'Uma carta em campo']])] },
  danoAlvo: { rot: 'Dar dano (formato antigo)', legado: true, fixo: { tipo: 'dano' }, campos: [n('valor', 'Quanto', 2), sel('alvo', 'Em quem', ALVO_DANO), bool('escalaGrau', '+ cartas no seu Grau')] },
  curarAlvo: { rot: 'Curar (formato antigo)', legado: true, fixo: { tipo: 'curar' }, campos: [n('valor', 'Quanto', 2), sel('alvo', 'Quem', ALVO_DANO)] },
  destruirAlvo: { rot: 'Destruir (formato antigo)', legado: true, fixo: { tipo: 'destruir' }, campos: [sel('alvo', 'Qual', [['escolhidoAliado', 'Um Aliado ou Mística'], ['escolhido', 'Uma carta em campo']])] },
  danoTodosAliados: { rot: 'Dano em cada Aliado (formato antigo)', legado: true, fixo: { tipo: 'danoTodosAliados' }, campos: [n('valor', 'Quanto', 1), bool('ambos', 'Também nos seus')] },
  destruirTodosAliados: { rot: 'Destruir todos os Aliados (formato antigo)', legado: true, fixo: { tipo: 'destruirTodosAliados' }, campos: [bool('ambos', 'Também os seus')] },
  olharTopo: { rot: 'Olhar o topo, 1 para a mão (formato antigo)', legado: true, fixo: { tipo: 'olharTopo' }, campos: [n('quantidade', 'Olhar', 3), n('pegar', 'Para a mão', 1)] },
};
/** Palavras-chave existentes (ligadas aos campos palavrasChave/protegido do formulário). */
export const PALAVRAS = [['Proficiente', 'Proficiente'], ['Etéreo', 'Etéreo'], ['Incomensurável', 'Incomensurável'], ['Protessência', 'Protessência (não pode ser alvo de efeitos do oponente)'], ['Repentino', 'Repentino (Aliado)'], ['Reanimum', 'Reanimum (jogável do cemitério; depois é banida)']];

/** Passivas do poder de um Conjurador (campos numéricos). */
export const PASSIVAS_CONJ = [
  ['primeiroDanoDeAliadoMais', 'O 1º dano de combate de um Aliado seu em cada turno é maior em'],
  ['primeiraCartaMaisBarata', 'A 1ª carta que você joga em cada turno custa a menos'],
  ['primeiraRepentinaMaisBarata', 'A 1ª carta Repentina que você joga em cada turno custa a menos'],
  ['aliadosProtegidos', 'Seus Aliados têm Protegido'],
];
/** Campos de uma permissão contínua (`permiteJogarDe`): pode jogar cartas do cemitério/banimento. */
export const CAMPOS_PERMISSAO = [sel('de', 'De onde', [['cemiterio', 'Seu cemitério'], ['banimento', 'Seu banimento']]), multi('tipos', 'Tipos (nenhum = qualquer)', TIPOS_JOGAVEIS, []), n('custoMax', 'Custo máximo (vazio = qualquer)', ''), bool('banir', 'Depois vai para o banimento'), bool('umaVezPorTurno', 'Só uma carta por turno')];
export const novaPermissao = () => ({ de: 'cemiterio', banir: true, umaVezPorTurno: true });

/**
 * Qual linha da tabela representa este efeito da mecânica (para reabrir no formulário); null = fora da tabela.
 * Vale a linha mais específica; a marca `secundario` desempata (sem ela, a linha comum ganha; só a secundária existe → ela).
 */
export function chaveDe(ef) {
  if (!ef || typeof ef !== 'object') return null;
  let melhor = null, nota = -1;
  for (const [k, t] of Object.entries(EFEITOS)) {
    let n = 0, ok = true;
    for (const [fk, fv] of Object.entries(t.fixo)) {
      if (ef[fk] === fv) n++;
      else if (fk === 'secundario' && !ef.secundario) n -= 0.5;
      else { ok = false; break; }
    }
    if (ok && n > nota) { melhor = k; nota = n; }
  }
  return melhor;
}
/** Linhas do grupo "Efeito secundário" (custos e consequências que vêm depois de outro efeito). */
export const ehSecundario = (chave) => EFEITOS[chave]?.grupo === 'secundario';
/** Efeito novo com os valores padrão da linha da tabela. */
export function novoEfeito(chave) {
  const t = EFEITOS[chave];
  const ef = { ...t.fixo };
  for (const c of t.campos) if (c.padrao !== undefined && c.padrao !== '' && c.padrao !== false) ef[c.k] = Array.isArray(c.padrao) ? [...c.padrao] : c.padrao;
  if (chave === 'invocar' && FICHAS.length) { ef.carta = FICHAS[0][0]; ef.nome = FICHAS[0][1]; }
  return ef;
}
/** Linhas da tabela oferecidas para um efeito desta condição (a atual sempre aparece). */
export function efeitosDisponiveis(quando, atual) {
  return Object.entries(EFEITOS).filter(([k, t]) => k === atual || (!t.legado && (!t.soEm || t.soEm.includes(quando))));
}
/** O efeito não funciona nesta condição (alvo escolhido ao ativar/jogar)? Devolve o aviso, ou ''. */
export function avisoEfeito(chave, quando) {
  const t = EFEITOS[chave];
  if (!t?.soEm || t.soEm.includes(quando)) return '';
  return `Só funciona em ${t.soEm.map((q) => (q === 'ativada' ? 'habilidade ativada' : 'Temporária (ao jogar)')).join(' ou ')}.`;
}
