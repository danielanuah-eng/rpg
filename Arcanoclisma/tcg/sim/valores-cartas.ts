// Valores das cartas na rodada 7. Cada carta fica ligada aos Valores cujas respostas do autor
// (data/respostas-autor.mjs) combinam com o que ela faz; cartas sem Valor servem a qualquer Conjurador.
// Uma carta é legal num baralho se tiver ao menos um dos Valores do Conjurador (ou nenhum Valor).
import type { CartaDef, Catalogo } from '../src/tipos.ts';

export const C = 'conflituo', S = 'sublimeyn', D = 'dominum', P = 'persona', A = 'alvorum', M = 'morbidyum', G = 'cognitium', E = 'eternyr', R = 'primitivum';

/** A primeira regra que casar com o id define os Valores. Cartas do Eternyr (já marcadas) e as não listadas ficam como estão. */
const REGRAS: Array<[RegExp, string[]]> = [
  [/^base-recurso$/, []],
  [/^base-aliado-c[1-5]-equilibrado$/, []],
  [/^base-aliado-c[678]-equilibrado$/, [S, D, A, M, G, E, R]],
  [/agressivo/, [C, R, P, A, M]], // inclui os Proficientes
  [/^base-aliado-c\d-defensivo$/, [D, E, R, P, M]],
  [/descarta-grau|grau5/, [A, M, E, R, D]],
  [/^base-aliado-recurso/, [S, R, A, G, E]],
  [/^bal-etereo/, [M, S, P, G]],
  [/^base-dano-c[12]$/, []],
  [/^base-dano-c[34]$/, [C, A, M, E, R]],
  [/^base-dano-c\d-repentina$/, [C, S, D, G, E]],
  [/^bal-remocao-grau/, [E, D, A, C]],
  [/^base-dano-conjurador/, [C, R, A, E]],
  [/^base-comprar-c1$/, []],
  [/^base-comprar-c3$/, [A, G, E, S, P]],
  [/^base-comprar-c2-repentina$/, [G, E, S, A]],
  [/^base-curar/, [M, R, A]],
  [/^base-destruir/, [D, A, E, S]],
  [/^base-mistica/, [S, D, E, M]],
  [/^base-estrutura-c1$/, [S, P, A, R, M]],
  [/^base-estrutura-c3$/, [S, P, G, E]],
  [/^base-estrutura-c3-enfermaria$/, [M, R]],
  [/^base-estrutura-c4-torre$/, [C, D]],
  [/^base-estrutura-c(4-fonte|6-nucleo|8-reator)$/, [A, G, E, R]],
  [/^bal-estrutura-cerco/, [D, C]],
  [/^(bal-varredura|r6-varredura|r6-furia)/, [A, C, E]],
  [/^r6-(ira|por-do-sol)/, [A, D]],
  [/^bal-devolver/, [S, E, P]],
  [/^bal-anular/, [D, G, E]],
  [/^r6-espiao/, [P, G]],
  [/^r6-piromante/, [C, R, A]],
  [/^r6-fera-cerco/, [R, C]],
  [/^r6-presa-trovao/, [R, C, D]],
  [/^r6-anjo/, [A, S]],
  [/^r6-mercador/, [G, P, S]],
  [/^r6-colosso/, [D, C]],
  [/^r6-dragao/, [R, A]],
];

/** Devolve uma cópia do catálogo com os Valores marcados nas cartas. */
export function comValores(cat: Catalogo): Catalogo {
  const cartas: Record<string, CartaDef> = {};
  for (const [id, c] of Object.entries(cat.cartas)) {
    const regra = REGRAS.find(([re]) => re.test(id));
    cartas[id] = regra && c.valores.length === 0 ? { ...c, valores: [...regra[1]] } : c;
  }
  return { ...cat, cartas };
}

/** Ids que nenhuma regra cobre e que não têm Valor (para conferir que não sobrou carta sem decisão). */
export function semDecisao(cat: Catalogo): string[] {
  return Object.keys(cat.cartas).filter((id) => cat.cartas[id].tipo !== 'item' && !REGRAS.some(([re]) => re.test(id)) && cat.cartas[id].valores.length === 0);
}
