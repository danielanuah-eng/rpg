// Edições do autor feitas no site (tela Editor): cartas e Conjuradores alterados por cima dos dados gerados
// (src/gerado/dados.json). Ficam no D1 (tabela edicoes, uma linha por salvamento) e valem no navegador
// (base.js), na conferência de baralhos do Worker e nas salas online (a sala guarda a versão do início da partida).
// Mesmo formato do editor do app de testes (tcg/data/edicoes.json: cartas e conjuradores com os campos mudados;
// null apaga o campo), mais `artes` para trocar a imagem. Convertido para public/jogo/edicoes.js pelo construir.ts.
import type { CartaDef, Catalogo, ConjuradorDef } from '../../tcg/src/tipos.ts';

export interface EdicoesSite {
  cartas: Record<string, Record<string, unknown>>;
  conjuradores: Record<string, Record<string, unknown>>;
  /** Arte escolhida (id do catálogo de artes aprovadas) por carta e por Conjurador. */
  artes: { cartas: Record<string, string>; conjuradores: Record<string, string> };
}

export const edicoesVazias = (): EdicoesSite => ({ cartas: {}, conjuradores: {}, artes: { cartas: {}, conjuradores: {} } });

/** Campos que o editor pode mudar (o resto é ignorado). */
export const CAMPOS_CARTA = ['nome', 'texto', 'tipo', 'subtipos', 'valores', 'custo', 'grau', 'custoValores', 'descartaGrau', 'velocidade', 'arcanoGerado', 'palavrasChave', 'ataque', 'vida', 'grauExtra', 'ativacao', 'pagarParaAnular', 'efeitos', 'aoEntrar', 'aoDanoConjurador', 'protegido', 'categoriaItem', 'defesa', 'vidaDesgaste'];
export const CAMPOS_CONJURADOR = ['nome', 'valores', 'comprometimento', 'vida', 'maoInicial', 'compraFinal', 'danoBasico', 'custoAtaque', 'habilidade', 'poder'];
/** Campos sem os quais a carta não funciona no motor (não podem ser apagados). */
const OBRIGATORIOS = new Set(['nome', 'tipo', 'subtipos', 'valores', 'custo', 'grau', 'velocidade', 'arcanoGerado', 'texto', 'vida', 'maoInicial', 'compraFinal', 'danoBasico', 'custoAtaque', 'habilidade', 'comprometimento']);

/** Limpa o que veio do navegador: só ids existentes, só campos conhecidos, tamanho limitado. */
export function sanitizarEdicoes(bruto: unknown, cat: Catalogo, artesValidas?: Set<string>): EdicoesSite {
  const b = (bruto ?? {}) as Partial<EdicoesSite>;
  const r = edicoesVazias();
  const copiar = (origem: unknown, campos: string[]) => {
    const o: Record<string, unknown> = {};
    for (const [k, v] of Object.entries((origem ?? {}) as Record<string, unknown>)) {
      if (!campos.includes(k) || v === undefined) continue;
      if (v === null && OBRIGATORIOS.has(k)) continue;
      o[k] = structuredClone(v);
    }
    return o;
  };
  for (const [id, p] of Object.entries(b.cartas ?? {})) if (cat.cartas[id]) { const o = copiar(p, CAMPOS_CARTA); if (Object.keys(o).length) r.cartas[id] = o; }
  for (const [id, p] of Object.entries(b.conjuradores ?? {})) if (cat.conjuradores[id]) { const o = copiar(p, CAMPOS_CONJURADOR); if (Object.keys(o).length) r.conjuradores[id] = o; }
  const okArte = (a: unknown) => typeof a === 'string' && /^[\w-]{1,80}$/.test(a) && (!artesValidas || artesValidas.has(a));
  for (const [id, a] of Object.entries(b.artes?.cartas ?? {})) if (cat.cartas[id] && okArte(a)) r.artes.cartas[id] = a as string;
  for (const [id, a] of Object.entries(b.artes?.conjuradores ?? {})) if (cat.conjuradores[id] && okArte(a)) r.artes.conjuradores[id] = a as string;
  return r;
}

function mesclar<T extends object>(base: T, parcial: Record<string, unknown>): T {
  const r = { ...base } as Record<string, unknown>;
  for (const [k, v] of Object.entries(parcial)) {
    if (v === null) delete r[k];
    else if (v !== undefined) r[k] = structuredClone(v);
  }
  return r as T;
}

/** Catálogo com as edições aplicadas (cópia; o original não muda). */
export function aplicarCatalogo(cat: Catalogo, ed: EdicoesSite | null | undefined): Catalogo {
  if (!ed) return cat;
  const cartas: Record<string, CartaDef> = { ...cat.cartas };
  for (const [id, p] of Object.entries(ed.cartas ?? {})) if (cartas[id]) cartas[id] = { ...mesclar(cartas[id], p), id };
  const conjuradores: Record<string, ConjuradorDef> = { ...cat.conjuradores };
  for (const [id, p] of Object.entries(ed.conjuradores ?? {})) if (conjuradores[id]) conjuradores[id] = { ...mesclar(conjuradores[id], p), id };
  return { cartas, conjuradores };
}

/** Quantas mudanças há (para mostrar ao autor). */
export const contarEdicoes = (ed: EdicoesSite) =>
  Object.keys(ed.cartas).length + Object.keys(ed.conjuradores).length + Object.keys(ed.artes.cartas).length + Object.keys(ed.artes.conjuradores).length;
