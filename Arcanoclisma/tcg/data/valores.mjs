// Fonte dos dados dos Valores. `node data/gerar-tabela.mjs` gera valores.json e docs/TABELA-DE-VALORES.md.
// RASCUNHO do Claude: só o que a estética (guia de estilo da arte) sugere; o resto fica sem nota (a definir pelo autor).

const P = (n, id, texto, tipo = 'escala') => ({ n, id, texto, tipo });

export const perguntas = [
  P(1, 'comprar', 'Dá draw por efeitos?'),
  P(2, 'suporteRecurso', 'Tem criaturas de suporte para recursos?'),
  P(3, 'dano', 'Tem boas formas de dar dano ou muito dano?'),
  P(4, 'burlarConjuracao', 'Tem formas de burlar conjuração?'),
  P(5, 'aliadosAtaque', 'Tem aliados bons para atacar?'),
  P(6, 'monRecurso', 'Os monumentos (Estruturas) são bons para gerar recurso?'),
  P(7, 'monDefesa', 'Os monumentos são bons para defender?'),
  P(8, 'monAtrapalhar', 'Os monumentos são bons para atrapalhar o outro jogador?'),
  P(9, 'permanentesCusto', 'Suas permanentes têm efeitos bons em troca de custo em outras coisas?'),
  P(10, 'cartasCaras', 'Suas melhores cartas são de alto custo, em maioria?'),
  P(11, 'cartasMedias', 'Suas melhores cartas são de custo médio, em maioria?'),
  P(12, 'reducaoCusto', 'Tem formas de redução de gastos?'),
  P(13, 'estetica', 'Quais seriam os aliados, feitiços e locais esteticamente presentes nas cartas?', 'texto'),
  P(14, 'early', 'Suas cartas são boas no começo do jogo, em maioria?'),
  P(15, 'anulacao', 'Tem cartas de anulação (dano, magias, místicas, feitiços em geral)?'),
  P(16, 'sorte', 'Tem efeitos que envolvam sorte?'),
  P(17, 'blink', 'Tem efeitos de blink, teleporte, sair do campo, trocar de lugar?'),
  P(18, 'suporteInvocacoes', 'Tem efeitos que facilitam ou dão suporte a invocações e aliados?'),
  P(19, 'equipamentos', 'Tem efeitos que alteram, melhoram ou pioram os equipamentos?'),
  P(20, 'combos', 'Facilita ou está envolvida com combos em geral?'),
  P(21, 'proativa', 'É proativa?'),
  P(22, 'reativa', 'É reativa?'),
  P(23, 'cemiterio', 'Interage com cemitério?'),
  P(24, 'repentinos', 'Tem muitos efeitos repentinos?'),
  P(25, 'geraArcano', 'Gera muito Arcano?'),
  P(26, 'descarte', 'Envolve descartar cartas?'),
  P(27, 'cura', 'Tem formas de curar?'),
  P(28, 'mexerGrau', 'Tem formas de mexer nas cartas do Grau?'),
  P(29, 'ritmo', 'Normalmente gosta de jogos mais: lentos, rápidos ou demorados?', 'texto'),
  P(30, 'olharTopo', 'Consegue olhar o topo do baralho?'),
  P(31, 'mexerTopo', 'Mexe com o topo do baralho?'),
  P(32, 'atrapalhaAliados', 'Consegue inviabilizar, atrapalhar, stunar ou prender Aliados?'),
  P(33, 'removeMisticas', 'Tem remoção de Místicas?'),
  P(34, 'removeEstruturas', 'Tem remoção de Estruturas?'),
  P(35, 'olhaMao', 'Olha a mão do oponente?'),
  P(36, 'inalvejaveis', 'Tem criaturas que não podem ser alvo ou afetadas por coisas específicas?'),
  P(37, 'aliadosConjuradores', 'Tem muitos aliados conjuradores?'),
  P(38, 'aliadosTankBate', 'Tem muitos aliados que tankam ou batem muito?'),
  P(39, 'limparCampo', 'Tem formas de limpar o campo?'),
  P(40, 'conjurador', 'Tem cartas para atrapalhar ou ajudar o Conjurador? Como?', 'texto'),
  P(41, 'tribais', 'Tem cartas tribais? Quais?', 'texto'),
  P(42, 'recuperar', 'Tem cartas para recuperar cartas do Grau ou do cemitério?'),
  P(43, 'custoExtraOponente', 'Pode fazer o oponente pagar custos extras?'),
];

const V = (id, nome, cor, temperatura, forma, ideia, notas, descritivas) => ({ id, nome, cor, temperatura, forma, ideia, notas, descritivas });

export const universais = {
  dragoes: 'Dragões existem em todos os Valores: há dragão de cada Valor. Subtipo universal, não é exclusivo de Alvorum.',
  divindades: 'Divindades também existem em todos os Valores.',
  aliados:
    'Aliados não são só pessoas: podem ser criaturas, monstros e divindades, de qualquer Valor. O tipo Aliado não depende do Valor nem de ser humano.',
  cartas:
    'Uma carta é a relação dela com o mundo: pode ser um personagem, o efeito de uma magia, um cenário que está acontecendo ou uma figura mágica.',
};

export const valores = [
  V('conflituo', 'Conflitüo', 'vermelho-ferrugem', 'quente', 'construtos sólidos de luz (Arsenal Vivo): lâminas, escudos, estandartes',
    'Tudo que envolve algum tipo de conflito: guerras, combates, emboscadas, explosões, brigas de bar.',
    { dano: 4, aliadosAtaque: 4, equipamentos: 4, early: 3, proativa: 4, reativa: 1, comprar: 1, cura: 1, anulacao: 1, cemiterio: 1, aliadosTankBate: 3 },
    { ritmo: 'rápidos', tribais: 'a definir (soldados, guerreiros, brigões?)', estetica: 'guerras, combates, emboscadas, explosões, brigas de bar' }),
  V('sublimeyn', 'Sublimeyn', 'prata-mercúrio', 'fria', 'fumaça e vapor que escorrem, pingam e cristalizam',
    'Tudo que envolve sólidos, líquidos, gases: a matéria em si.',
    { blink: 4, burlarConjuracao: 3, reativa: 3, repentinos: 4, anulacao: 3, combos: 3, dano: 1, aliadosAtaque: 1 },
    { ritmo: 'rápidos e reativos (tempo)', tribais: 'a definir', estetica: 'sólidos, líquidos e gases; laboratórios, vapor, metal líquido' }),
  V('dominum', 'Dominum', 'roxo-sangue', 'fria', 'construtos: correntes, coleiras e elos presos ao corpo',
    'Correntes, coleiras, jaulas, restrições, prisões, confinamentos, bloqueios.',
    { atrapalhaAliados: 4, custoExtraOponente: 4, monAtrapalhar: 4, monDefesa: 3, removeMisticas: 3, reativa: 3, cartasCaras: 3, dano: 2 },
    { ritmo: 'lentos', tribais: 'a definir', estetica: 'prisões, jaulas, correntes, coleiras, confinamentos' }),
  V('persona', 'Persona', 'coral', 'quente', 'aura suave e uniforme que transborda com emoção forte',
    'Pessoas, emoções, vínculos, mudanças de status, aumento de resistência, clonagem e multiplicação.',
    { aliadosConjuradores: 4, suporteInvocacoes: 4, aliadosTankBate: 3, conjurador: 3, dano: 1 },
    { ritmo: 'variados (depende da emoção)', tribais: 'a definir', estetica: 'pessoas, multidões, vínculos, clones e multiplicação' }),
  V('alvorum', 'Alvorum', 'dourado e preto', 'quente', 'aura dividida, com falhas entre a parte de luz e a de sombra',
    'Dualidade de luz e sombra; olhar a mão, descartar, efeitos com custos diferentes.',
    { olhaMao: 3, descarte: 3, permanentesCusto: 3 },
    { ritmo: 'a definir', tribais: 'a definir', estetica: 'luz e sombra lado a lado, escolhas com custo' }),
  V('morbidyum', 'Morbidyum', 'verde-pálido', 'fria', 'fumaça fria e fios que partem, como almas',
    'Morte, almas, fantasmas, curas, premonições incertas, descartar, destruir, sacrificar sem dar alvo, reviver.',
    { cemiterio: 4, recuperar: 4, descarte: 4, cura: 3, removeMisticas: 3, olharTopo: 2 },
    { ritmo: 'lentos', tribais: 'fantasmas (a definir mais)', estetica: 'morte, almas, fantasmas, cemitérios, névoa' }),
  V('cognitium', 'Cognitium', 'índigo', 'fria', 'véu de fumaça com falhas, como se faltassem pedaços',
    'Mente, memória, conhecimento, cartas na mão, ideias, previsão, previsibilidade, manipular a mente.',
    { comprar: 4, olharTopo: 4, mexerTopo: 4, olhaMao: 4, atrapalhaAliados: 3, sorte: 0 },
    { ritmo: 'lentos', tribais: 'a definir', estetica: 'mentes, memórias, bibliotecas, véus, ideias' }),
  V('eternyr', 'Eternyr', 'azul-noite com riscos brancos', 'fria', 'aura com falhas e rastros parados no ar', 'Tempo e permanência.',
    { cartasCaras: 4, inalvejaveis: 3, reativa: 3, anulacao: 3, early: 1, cura: 2 },
    { ritmo: 'demorados', tribais: 'a definir', estetica: 'relógios, rastros, noite' }),
  V('primitivum', 'Primitivum', 'verde-musgo e âmbar', 'quente/terroso', 'construtos orgânicos: raízes, galhos e esporos soltos',
    'Natureza e animais, proteção, sobrevivência na natureza.',
    { geraArcano: 4, suporteRecurso: 4, monRecurso: 4, aliadosTankBate: 3, monDefesa: 3, early: 3, cura: 3 },
    { ritmo: 'médios, crescendo', tribais: 'animais (a definir mais)', estetica: 'animais, florestas, vilarejos, raízes, sobrevivência' }),
];

export const aviso =
  '**Tabela dos Valores 0.3.** Notas em negrito são respostas do autor; ~N são conversões minhas de respostas em palavras (perguntas 42 e 43), ainda sem número confirmado. Ritmo (29) é do autor; tribais (41) foram propostas pelo Claude seguindo a lore, por decisão do autor. Estética (13) segue como rascunho aceito por enquanto.';
