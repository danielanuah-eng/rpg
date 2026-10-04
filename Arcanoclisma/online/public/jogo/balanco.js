// Gerado por online/construir.ts a partir de tcg/src/balanco.ts. Não edite: edite o .ts e reconstrua.
// Orçamento de Pontos de Poder (PP): régua para comparar cartas. Ver docs/ESTATISTICAS-DAS-CARTAS.md.
                                                   

/** Preço de cada coisa, em PP. */
export const PRECO = {
  ataque: 1,
  vida: 1,
  danoAlvo: 2,
  danoConjurador: 1.5,
  curar: 1,
  destruir: 7,
  comprar: 3,
  /** Preço de comprar carta para cartas do Valor Eternyr (melhor custo-benefício quando menor). */
  comprarEternyr: 3,
  arcano: 2,
  /** Dar Arcano ao oponente é uma desvantagem: abate do preço. */
  arcanoOponente: -2,
  repentina: 2,
  proficiente: 2,
  /** Etéreo: só é bloqueado por Aliados Etéreos (evasão). */
  etereo: 2,
  /** Cada carta descartada do Grau é um custo real: libera PP extra. */
  descartaGrau: 3,
  custoValor: 1,
  /** Cada carta de Arcano gerado acima de 1 ao ser enviada como pitch abate do orçamento. */
  pitchExtra: 2,
  /** Habilidade ativada: multiplica o preço do efeito (ela se repete a cada turno). */
  ativacaoArcano: 1.5,
  ativacaoOutro: 2,
  /** Cada Arcano de custo da ativação abate do preço. */
  ativacaoCusto: 1.5,
  /** Ativação que só gera Arcano em carta de custo 5 ou mais (a geração cara pode ser mais barata por ponto). */
  ativacaoArcanoAlto: 1.5,
  /** Cada ponto de grauExtra (a permanente conta como carta no Grau). */
  grauExtra: 2,
  /** Eternyr: dano a Aliado/Místicas (mais barato), dano ao Conjurador (mais caro) e bônus por Grau acima do padrão. */
  danoAliadoEternyr: 2,
  danoConjEternyr: 1.5,
  grauAltoEternyr: 1,
  /** Devolver uma permanente à mão (tempo). */
  devolver: 4,
  /** Anular uma carta na pilha. */
  anular: 6,
  /** Dano em cada Aliado do oponente, por ponto. */
  danoTodos: 4,
  /** Grau médio assumido para precificar dano que cresce com o Grau. */
  grauMedioEscala: 3,
  // ----- rodada 6 (variante BAL; nenhuma carta do padrão usa) -----
  /** Dano em cada Aliado de todos os jogadores, por ponto (varredura simétrica: também acerta os seus). */
  danoTodosAmbos: 2.5,
  /** Destruir todos os Aliados do oponente (efeito único, não depende de dano). */
  destruirTodos: 16,
  /** Destruir todos os Aliados de todos os jogadores (simétrico: também custa os seus). */
  destruirTodosAmbos: 11,
  /** Protegido N: por ponto de N. */
  protegido: 1.5,
  /** Gatilho "ao causar dano de combate ao Conjurador": multiplica o preço do efeito (repete a cada acerto, mas precisa passar). */
  aoDanoConjurador: 1.5,
  /** Olhar o topo: cada carta vista além das que vão para a mão (escolha). As que vão para a mão custam como comprar. */
  olharTopoExtra: 0.5,
}         ;

                                                           

/** Orçamento: 2 PP por ponto de custo, mais 2, menos o que a carta rende a mais como pitch. Nunca negativo. */
export function orcamento(custo        , arcanoGerado = 1, p         = PRECO)         {
  return Math.max(0, 2 * custo + 2 - p.pitchExtra * (arcanoGerado - 1));
}

/** Grau "normal" para um custo: castável no turno `custo - 1` do dono (1 carta no Grau por turno, mais 1 de pitch). */
export const grauPadrao = (custo        )         => Math.max(0, custo - 1);

export function precoEfeito(ef        , p         = PRECO, valores           = [])         {
  switch (ef.tipo) {
    case 'dano': {
      const et = valores.includes('eternyr');
      const v = ef.valor + (ef.escalaGrau ? p.grauMedioEscala : 0);
      if (ef.alvo === 'escolhidoAliado') return (et ? p.danoAliadoEternyr : p.danoAlvo) * v;
      if (ef.alvo === 'escolhido') return p.danoAlvo * v;
      return (et ? p.danoConjEternyr : p.danoConjurador) * v;
    }
    case 'curar': return p.curar * ef.valor;
    case 'destruir': return p.destruir;
    case 'comprar': return (valores.includes('eternyr') ? p.comprarEternyr : p.comprar) * ef.valor;
    case 'olharTopo': return (valores.includes('eternyr') ? p.comprarEternyr : p.comprar) * ef.pegar + p.olharTopoExtra * Math.max(0, ef.quantidade - ef.pegar);
    case 'arcano': return p.arcano * ef.valor;
    case 'arcanoOponente': return p.arcanoOponente * ef.valor;
    case 'devolver': return p.devolver;
    case 'anular': return p.anular;
    case 'danoTodosAliados': return (ef.ambos ? p.danoTodosAmbos : p.danoTodos) * ef.valor;
    case 'destruirTodosAliados': return ef.ambos ? p.destruirTodosAmbos : p.destruirTodos;
  }
}

/**
 * Curva de poder por Grau (rodada 6, variante BAL): bônus de orçamento que cresce com o quadrado do Grau,
 * bônus = k × G × (G − 1) / 2, com G = Grau da carta limitado a 8. Com k = 0,5:
 * Grau 3 → +1,5 PP; 4 → +3; 5 → +5; 6 → +7,5; 7 → +10,5; 8 ou mais → +14.
 * Grau baixo quase não muda; Grau alto ganha muito mais (o Grau é o "custo de tempo" da carta).
 */
export function bonusGrau(grau        , k        )         {
  const g = Math.min(grau, 8);
  return k > 0 && g > 1 ? (k * g * (g - 1)) / 2 : 0;
}

/** PP de uma habilidade ativada. */
export function precoAtivacao(c          , p         = PRECO)         {
  const at = c.ativacao;
  if (!at) return 0;
  const soma = at.efeitos.reduce((s, ef) => s + precoEfeito(ef, p, c.valores), 0);
  const soArcano = at.efeitos.every((ef) => ef.tipo === 'arcano');
  const fator = soArcano ? (c.custo >= 5 ? p.ativacaoArcanoAlto : p.ativacaoArcano) : p.ativacaoOutro;
  return Math.max(0, fator * soma - p.ativacaoCusto * at.custo);
}

/** PP líquidos da carta: o que ela entrega menos o que ela cobra em Grau e descarte. */
export function pontos(c          , p         = PRECO)         {
  let t = (c.ataque ?? 0) + (c.vida ?? 0);
  for (const ef of [...(c.efeitos ?? []), ...(c.aoEntrar ?? [])]) t += precoEfeito(ef, p, c.valores);
  t += precoAtivacao(c, p);
  for (const ef of c.aoDanoConjurador ?? []) t += p.aoDanoConjurador * precoEfeito(ef, p, c.valores);
  t += p.protegido * (typeof c.protegido === 'number' ? c.protegido : c.protegido?.arcano ?? 0); // Protegido unificado (04/10): conta o Arcano, como antes
  if (c.velocidade === 'repentina') t += p.repentina;
  if (c.palavrasChave?.includes('Proficiente')) t += p.proficiente;
  if (c.palavrasChave?.includes('Etéreo')) t += p.etereo;
  t -= p.descartaGrau * (c.descartaGrau ?? 0);
  t -= p.custoValor * (c.custoValores?.length ?? 0);
  t += p.grauExtra * (c.grauExtra ?? 0);
  const difGrau = c.grau - grauPadrao(c.custo);
  t -= difGrau > 0 && c.valores.includes('eternyr') ? difGrau * p.grauAltoEternyr : difGrau;
  return t;
}

                            
                 
                    
                                                                                    
                
 

export function avaliar(c          , p         = PRECO)            {
  const o = orcamento(c.custo, c.arcanoGerado, p);
  const t = pontos(c, p);
  return { pontos: t, orcamento: o, folga: o - t };
}
