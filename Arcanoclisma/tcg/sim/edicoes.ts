// Edições do autor: cartas, Conjuradores, regras, curva de Grau e baralhos alterados pelo editor do app
// (app/editor.html). Ficam em data/edicoes.json e são aplicadas por cima do jogo gerado nas variantes com
// `edicoesAutor` (PADRAO e CONJ). Assim o app de testes, as simulações e a equipe de balanceamento usam a versão editada.
// A régua de pontos editada (`precos`) só muda a conta de pontos que o editor mostra; não muda carta nenhuma.
// Nos testes automáticos (node --test) as edições não entram, para o motor ser testado sobre o jogo gerado.
import { existsSync, readFileSync, statSync } from 'node:fs';
import type { CartaDef, Catalogo, ConjuradorDef, Regras } from '../src/tipos.ts';
import type { Precos } from '../src/balanco.ts';
import type { Estilo } from './bot.ts';
import type { Arquetipo } from './baralhos.ts';

/** Um campo com valor `null` apaga o campo da carta gerada (ex.: tirar a ativação). */
export type Parcial<T> = { [K in keyof T]?: T[K] | null };

export interface EntradaHistorico {
  quando: string;
  /** Nota do autor ao salvar (o porquê da mudança). */
  nota: string;
  /** Lista legível do que mudou neste salvamento. */
  mudancas: string[];
}

export interface Edicoes {
  /** Por id: campos alterados de uma carta existente, ou a carta inteira quando é nova (`nova: true`). */
  cartas: Record<string, Parcial<CartaDef> & { nova?: boolean }>;
  /** Por id: campos alterados de um Conjurador (o `poder` substitui o inteiro). */
  conjuradores: Record<string, Parcial<ConjuradorDef>>;
  regras: Partial<Regras>;
  /** Régua de pontos (PP) usada pelo editor para avaliar as cartas (não altera as cartas). */
  precos: Partial<Precos>;
  /** Curva de poder por Grau dos Aliados (k da fórmula k × G × (G − 1) / 2). */
  curvaGrau?: number;
  /** Por Conjurador: lista do baralho (substitui a inteira) e ajustes do estilo do jogador automático. */
  baralhos: Record<string, { lista?: Array<[string, number]>; estilo?: Partial<Estilo>; /** Itens equipados (substitui a lista inteira). */ itens?: string[] }>;
  historico: EntradaHistorico[];
}

export const ARQUIVO_EDICOES = new URL('../data/edicoes.json', import.meta.url);

export const edicoesVazias = (): Edicoes => ({ cartas: {}, conjuradores: {}, regras: {}, precos: {}, baralhos: {}, historico: [] });

export function normalizar(bruto: Partial<Edicoes> | null | undefined): Edicoes {
  return { ...edicoesVazias(), ...(bruto ?? {}) };
}

/** Lê data/edicoes.json (vazio se não existe ou se está rodando nos testes automáticos). */
let cache: { mtime: number; ed: Edicoes } | null = null;
export function lerEdicoes(): Edicoes {
  if (process.env.NODE_TEST_CONTEXT || process.env.ARCANO_SEM_EDICOES) return edicoesVazias();
  if (!existsSync(ARQUIVO_EDICOES)) return edicoesVazias();
  const mtime = statSync(ARQUIVO_EDICOES).mtimeMs;
  if (cache?.mtime !== mtime) cache = { mtime, ed: normalizar(JSON.parse(readFileSync(ARQUIVO_EDICOES, 'utf8'))) };
  return cache.ed;
}

function mesclar<T extends object>(base: T, parcial: Parcial<T>): T {
  const r = { ...base } as Record<string, unknown>;
  for (const [k, v] of Object.entries(parcial)) {
    if (v === null) delete r[k];
    else if (v !== undefined) r[k] = structuredClone(v);
  }
  return r as T;
}

/** Aplica as cartas e os Conjuradores editados a um catálogo (devolve uma cópia). */
export function aplicarCatalogo(cat: Catalogo, ed: Edicoes): Catalogo {
  const cartas: Record<string, CartaDef> = { ...cat.cartas };
  for (const [id, parcial] of Object.entries(ed.cartas)) {
    const { nova, ...campos } = parcial;
    const base = cartas[id];
    if (!base && !nova) continue; // a carta gerada deixou de existir: a edição fica guardada, mas sem efeito
    cartas[id] = { ...mesclar(base ?? ({} as CartaDef), campos as Parcial<CartaDef>), id };
  }
  const conjuradores: Record<string, ConjuradorDef> = { ...cat.conjuradores };
  for (const [id, parcial] of Object.entries(ed.conjuradores)) {
    if (conjuradores[id]) conjuradores[id] = { ...mesclar(conjuradores[id], parcial), id };
  }
  return { cartas, conjuradores };
}

/** Aplica as listas e os estilos editados aos baralhos dos Conjuradores. */
export function aplicarBaralhos(arqs: Arquetipo[], ed: Edicoes): Arquetipo[] {
  return arqs.map((a) => {
    const b = a.conjurador ? ed.baralhos[a.conjurador] : undefined;
    if (!b) return a;
    return { ...a, lista: b.lista ? b.lista.map(([id, n]) => [id, n] as [string, number]) : a.lista, estilo: { ...a.estilo, ...(b.estilo ?? {}) }, itens: b.itens ? [...b.itens] : a.itens };
  });
}
