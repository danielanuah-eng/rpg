// Edições do autor feitas no site (tela Editor): cartas e Conjuradores alterados por cima dos dados gerados
// (src/gerado/dados.json). Ficam no D1 (tabela edicoes, uma linha por salvamento) e valem no navegador
// (base.js), na conferência de baralhos do Worker e nas salas online (a sala guarda a versão do início da partida).
// Mesmo formato do editor do app de testes (tcg/data/edicoes.json: cartas e conjuradores com os campos mudados;
// null apaga o campo), mais `artes` para trocar a imagem. Convertido para public/jogo/edicoes.js pelo construir.ts.
import type { CartaDef, Catalogo, ConjuradorDef } from '../../tcg/src/tipos.ts';
import { normalizarProtegido } from '../../tcg/src/motor.ts';

export interface EdicoesSite {
  cartas: Record<string, Record<string, unknown>>;
  conjuradores: Record<string, Record<string, unknown>>;
  /** Arte escolhida (id do catálogo de artes aprovadas) por carta e por Conjurador. */
  artes: { cartas: Record<string, string>; conjuradores: Record<string, string> };
  /** Raridade definida pelo autor (sem raridade automática): comum, incomum, rara ou lendaria. */
  raridades: { cartas: Record<string, string>; conjuradores: Record<string, string> };
}
export const RARIDADES = ['comum', 'incomum', 'rara', 'lendaria'];

export const edicoesVazias = (): EdicoesSite => ({ cartas: {}, conjuradores: {}, artes: { cartas: {}, conjuradores: {} }, raridades: { cartas: {}, conjuradores: {} } });

/** Campos que o editor pode mudar (o resto é ignorado). */
export const CAMPOS_CARTA = ['nome', 'texto', 'tipo', 'subtipos', 'valores', 'custo', 'grau', 'custoValores', 'descartaGrau', 'velocidade', 'arcanoGerado', 'palavrasChave', 'ataque', 'vida', 'grauExtra', 'ativacao', 'pagarParaAnular', 'efeitos', 'aoEntrar', 'aoDanoConjurador', 'protegido', 'categoriaItem', 'defesa', 'vidaDesgaste', 'aoAtacar', 'aoProteger', 'espinhos', 'tipoCanal', 'habilidades', 'protecao', 'entraSobre', 'concedeAoSubstituto'];
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
  for (const [id, x] of Object.entries(b.raridades?.cartas ?? {})) if (cat.cartas[id] && RARIDADES.includes(x as string)) r.raridades.cartas[id] = x as string;
  for (const [id, x] of Object.entries(b.raridades?.conjuradores ?? {})) if (cat.conjuradores[id] && RARIDADES.includes(x as string)) r.raridades.conjuradores[id] = x as string;
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
  // Protegido unificado (04/10, noite): edição antiga com `protecao` ("Proteção mágica") vira `protegido`
  for (const [id, p] of Object.entries(ed.cartas ?? {})) if (cartas[id]) cartas[id] = normalizarProtegido({ ...mesclar(cartas[id], p), id });
  const conjuradores: Record<string, ConjuradorDef> = { ...cat.conjuradores };
  for (const [id, p] of Object.entries(ed.conjuradores ?? {})) if (conjuradores[id]) conjuradores[id] = { ...mesclar(conjuradores[id], p), id };
  return { cartas, conjuradores };
}

/** Quantas mudanças há (para mostrar ao autor). */
export const contarEdicoes = (ed: EdicoesSite) =>
  new Set([...Object.keys(ed.cartas), ...Object.keys(ed.artes?.cartas ?? {}), ...Object.keys(ed.raridades?.cartas ?? {})]).size
  + new Set([...Object.keys(ed.conjuradores), ...Object.keys(ed.artes?.conjuradores ?? {}), ...Object.keys(ed.raridades?.conjuradores ?? {})]).size;

// ---------- salvar sem apagar o trabalho de outra aba (bug de 05/10: v117 apagou 37 cartas da v116) ----------
// O Editor não manda mais o rascunho inteiro: manda a DIFERENÇA para a versão em que o rascunho se apoia (`base`),
// por carta/Conjurador e por grupo (campos, arte, raridade). null = voltar ao original. O servidor só grava se a
// base ainda for a versão em vigor (senão 409 com a atual); o navegador então reaplica a diferença sobre a atual
// e, se a mesma carta mudou nos dois lados, pergunta ao autor qual manter.

/** Mesma forma de EdicoesSite, com null para "tirar esta carta/Conjurador das edições" (volta ao original). */
export interface DiferencaEdicoes {
  cartas: Record<string, Record<string, unknown> | null>;
  conjuradores: Record<string, Record<string, unknown> | null>;
  artes: { cartas: Record<string, string | null>; conjuradores: Record<string, string | null> };
  raridades: { cartas: Record<string, string | null>; conjuradores: Record<string, string | null> };
}
type Grupo = readonly ['cartas' | 'conjuradores'] | readonly ['artes' | 'raridades', 'cartas' | 'conjuradores'];
const GRUPOS: Grupo[] = [['cartas'], ['conjuradores'], ['artes', 'cartas'], ['artes', 'conjuradores'], ['raridades', 'cartas'], ['raridades', 'conjuradores']];
const PROIBIDOS = new Set(['__proto__', 'constructor', 'prototype']);
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const mapaDe = (ed: any, g: Grupo, criar = false): Record<string, unknown> => {
  if (!ed) return {};
  if (g.length === 1) return criar ? (ed[g[0]] ??= {}) : (ed[g[0]] ?? {});
  if (criar) return ((ed[g[0]] ??= {})[g[1]] ??= {});
  return ed[g[0]]?.[g[1]] ?? {};
};
const tipoDo = (g: Grupo) => (g[g.length - 1] === 'cartas' ? 'carta' : 'conjurador') as 'carta' | 'conjurador';
const pegar = (m: Record<string, unknown>, id: string) => (Object.hasOwn(m, id) ? m[id] : undefined);

function ordenado(v: unknown): unknown {
  if (Array.isArray(v)) return v.map(ordenado);
  if (v && typeof v === 'object') return Object.fromEntries(Object.keys(v).sort().map((k) => [k, ordenado((v as Record<string, unknown>)[k])]));
  return v;
}
/** Igualdade de JSON sem depender da ordem das chaves. */
export const igualJson = (a: unknown, b: unknown) => JSON.stringify(ordenado(a)) === JSON.stringify(ordenado(b));

export const diferencaVazia = (): DiferencaEdicoes => edicoesVazias() as DiferencaEdicoes;

/** O que mudou de `base` para `novo`. `semRemocoes`: não gera null (o que falta em `novo` fica como está). */
export function diferencaEdicoes(base: unknown, novo: unknown, { semRemocoes = false } = {}): DiferencaEdicoes {
  const d = diferencaVazia();
  for (const g of GRUPOS) {
    const a = mapaDe(base, g), b = mapaDe(novo, g), saida = mapaDe(d, g, true);
    for (const id of new Set([...Object.keys(a), ...Object.keys(b)])) {
      if (PROIBIDOS.has(id)) continue;
      const va = pegar(a, id), vb = pegar(b, id);
      if (igualJson(va, vb)) continue;
      if (vb === undefined || vb === null) { if (!semRemocoes && va !== undefined) saida[id] = null; }
      else saida[id] = structuredClone(vb);
    }
  }
  return d;
}

/** Edições `ed` com a diferença aplicada (cópia). */
export function aplicarDiferenca(ed: unknown, dif: unknown): EdicoesSite {
  const r = { ...edicoesVazias(), ...structuredClone((ed ?? {}) as EdicoesSite) };
  r.artes = { cartas: {}, conjuradores: {}, ...(r.artes ?? {}) };
  r.raridades = { cartas: {}, conjuradores: {}, ...(r.raridades ?? {}) };
  for (const g of GRUPOS) {
    const m = mapaDe(r, g, true);
    for (const [id, v] of Object.entries(mapaDe(dif, g))) {
      if (PROIBIDOS.has(id) || v === undefined) continue;
      if (v === null) delete m[id]; else m[id] = structuredClone(v);
    }
  }
  return r;
}

/** Quantas cartas/Conjuradores a diferença mexe. */
export function contarDiferenca(dif: DiferencaEdicoes): number {
  const s = new Set<string>();
  for (const g of GRUPOS) for (const id of Object.keys(mapaDe(dif, g))) s.add(tipoDo(g) + ':' + id);
  return s.size;
}

export interface ConflitoEdicao { tipo: 'carta' | 'conjurador'; id: string; partes: string[] }
/**
 * A mesma carta mudou nos dois lados: de `base` para `atual` (outra aba/salvamento) e na diferença do rascunho,
 * com resultados diferentes. Sem conflito, a diferença pode ser reaplicada sobre `atual` com segurança.
 */
export function conflitosEdicoes(base: unknown, atual: unknown, dif: unknown): ConflitoEdicao[] {
  const por = new Map<string, ConflitoEdicao>();
  for (const g of GRUPOS) {
    const mb = mapaDe(base, g), ma = mapaDe(atual, g);
    for (const [id, v] of Object.entries(mapaDe(dif, g))) {
      const noSite = pegar(ma, id);
      if (igualJson(pegar(mb, id), noSite)) continue; // ninguém mexeu nesta parte desde a base
      if (igualJson(v ?? undefined, noSite)) continue; // os dois lados chegaram ao mesmo resultado
      const k = tipoDo(g) + ':' + id;
      if (!por.has(k)) por.set(k, { tipo: tipoDo(g), id, partes: [] });
      por.get(k)!.partes.push(g.length === 1 ? 'campos' : g[0] === 'artes' ? 'arte' : 'raridade');
    }
  }
  return [...por.values()];
}

/** Tira uma carta/Conjurador da diferença (o autor escolheu manter a versão do site). */
export function tirarDaDiferenca(dif: DiferencaEdicoes, tipo: 'carta' | 'conjurador', id: string) {
  for (const g of GRUPOS) if (tipoDo(g) === tipo) delete mapaDe(dif, g, true)[id];
}

/**
 * "Restaurar cartas perdidas": o que existia na versão `antiga` e não existe em `atual` (por carta/Conjurador e por
 * grupo: campos, arte, raridade). Nada é apagado; o resultado vai para o rascunho, sem salvar sozinho.
 */
export function cartasPerdidas(antiga: unknown, atual: unknown): DiferencaEdicoes {
  const d = diferencaVazia();
  for (const g of GRUPOS) {
    const ma = mapaDe(atual, g), saida = mapaDe(d, g, true);
    for (const [id, v] of Object.entries(mapaDe(antiga, g))) if (!PROIBIDOS.has(id) && v != null && pegar(ma, id) === undefined) saida[id] = structuredClone(v);
  }
  return d;
}
