// Respostas do AUTOR às perguntas de escala (0 Péssimo ... 4 Muito bem), uma nota por Valor.
// Ordem dos Valores: Conflitüo, Sublimeyn, Dominum, Persona, Alvorum, Morbidyum, Cognitium, Eternyr, Primitivum.
// Têm prioridade sobre os palpites do Claude em valores.mjs. Perguntas ainda não respondidas ficam de fora.
export const ordem = ['conflituo', 'sublimeyn', 'dominum', 'persona', 'alvorum', 'morbidyum', 'cognitium', 'eternyr', 'primitivum'];

export const respostas = {
  // Eternyr (8º valor) subiu de 3 para 4 a pedido do autor (mais cartas de draw, melhor custo-benefício).
  comprar: [0, 2, 0, 2, 3, 1, 4, 4, 0],
  suporteRecurso: [0, 4, 0, 1, 1, 2, 0, 0, 3],
  dano: [4, 0, 2, 2, 3, 3, 1, 4, 3],
  burlarConjuracao: [0, 3, 2, 4, 0, 4, 0, 3, 1],
  aliadosAtaque: [4, 1, 0, 3, 3, 3, 0, 0, 4],
  monRecurso: [0, 3, 0, 0, 2, 4, 1, 4, 0],
  monDefesa: [0, 0, 3, 0, 1, 0, 0, 3, 0],
  monAtrapalhar: [0, 3, 4, 0, 2, 0, 0, 4, 0],
  permanentesCusto: [0, 3, 0, 4, 4, 3, 2, 2, 4],
  cartasCaras: [0, 3, 4, 2, 4, 3, 4, 4, 3],
  cartasMedias: [0, 4, 3, 4, 3, 4, 2, 2, 3],
  reducaoCusto: [3, 4, 0, 4, 0, 3, 0, 3, 4],
  early: [4, 2, 3, 2, 0, 2, 0, 0, 3],
  anulacao: [0, 0, 4, 0, 0, 0, 2, 3, 0],
  sorte: [0, 3, 0, 2, 0, 0, 0, 0, 0],
  blink: [0, 4, 0, 0, 0, 0, 0, 3, 0],
  suporteInvocacoes: [0, 2, 0, 0, 4, 0, 2, 0, 4],
  equipamentos: [4, 2, 3, 0, 3, 0, 0, 0, 0],
  combos: [0, 3, 0, 2, 2, 3, 2, 4, 0],
  proativa: [4, 2, 1, 3, 2, 3, 2, 0, 4],
  reativa: [0, 2, 4, 0, 2, 0, 3, 3, 0],
  cemiterio: [0, 0, 0, 0, 0, 4, 0, 0, 0],
  repentinos: [4, 4, 4, 2, 1, 2, 3, 3, 1],
  geraArcano: [0, 2, 3, 1, 4, 1, 4, 4, 2],
  descarte: [0, 0, 0, 0, 1, 3, 2, 0, 0],
  // ATENÇÃO: o autor enviou '1/0/0/02/4/0/0/3' (8 valores). Li como 1/0/0/0/2/4/0/0/3; confirmar.
  cura: [1, 0, 0, 0, 2, 4, 0, 0, 3],
  mexerGrau: [0, 0, 2, 0, 0, 0, 4, 4, 4],
  olharTopo: [0, 0, 0, 0, 3, 0, 4, 3, 0],
  mexerTopo: [0, 0, 2, 2, 3, 0, 4, 0, 0],
  atrapalhaAliados: [2, 2, 4, 0, 3, 0, 4, 4, 0],
  removeMisticas: [0, 2, 4, 0, 4, 0, 0, 4, 0],
  removeEstruturas: [4, 0, 0, 0, 0, 0, 0, 2, 0],
  olhaMao: [0, 0, 0, 0, 0, 0, 4, 0, 0],
  inalvejaveis: [0, 2, 0, 1, 0, 0, 0, 0, 4],
  aliadosConjuradores: [0, 0, 2, 0, 3, 2, 3, 4, 0],
  // O autor enviou '4/2/0/3/0/3//0/2/4' (barra dupla antes do Cognitium); li o vazio como erro de digitação.
  aliadosTankBate: [4, 2, 0, 3, 0, 3, 0, 2, 4],
  limparCampo: [2, 0, 0, 0, 4, 0, 0, 2, 0],
};

// Respostas descritivas do autor (pergunta 40: ajudar ou atrapalhar o Conjurador).
export const descritivas = {
  // 29 (ritmo): resposta do autor.
  ritmo: {
    conflituo: 'rápido',
    sublimeyn: 'meio-termo',
    dominum: 'demorado',
    persona: 'meio-termo para rápido',
    alvorum: 'meio-termo para demorado',
    morbidyum: 'lento',
    cognitium: 'lento',
    eternyr: 'lento',
    primitivum: 'rápido para meio-termo; conforme a estratégia, pode acelerar o late game',
  },
  // 41 (tribais): o autor deixou a decisão com o Claude, de acordo com a lore. Proposta, ajustável.
  // Dragões e divindades existem em todos os Valores (subtipos universais).
  tribais: {
    conflituo: 'Combatente, Brigão, Mercenário, Soldado',
    sublimeyn: 'Alquimista, Elemental, Metamorfo, Autômato',
    dominum: 'Carcereiro, Prisioneiro, Acorrentado, Fera presa',
    persona: 'Clone, Multidão, Companheiro, Rival',
    alvorum: 'Gêmeo, Crepuscular, Luzente, Sombra',
    morbidyum: 'Fantasma, Alma, Ceifador, Revenante',
    cognitium: 'Sábio, Vidente, Ilusionista, Memória',
    eternyr: 'Relojoeiro, Eterno, Cronista, Guardião do tempo',
    primitivum: 'Fera, Caçador, Sobrevivente, Druida',
  },
  conjurador: {
    eternyr: 'ajuda o Conjurador',
    cognitium: 'atrapalha o Conjurador',
    dominum: 'atrapalha um pouco o Conjurador',
  },
};

// Respostas do autor só em palavras (sem número). O Claude converteu em nota PROVISÓRIA (aparece como ~N) até o autor confirmar.
// 42: 'só Morbidyum e cartas poderosas de Eternyr têm' | 43: 'só Conflitüo, Cognitium e Dominum'.
export const provisorias = {
  recuperar: [0, 0, 0, 0, 0, 4, 0, 2, 0],
  custoExtraOponente: [3, 0, 3, 0, 0, 0, 3, 0, 0],
};
