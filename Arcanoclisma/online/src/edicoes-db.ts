// Leitura das edições do autor no D1 (tabela edicoes). Separado de edicoes.ts, que também roda no navegador.
import type { Env } from './conta.ts';
import { edicoesVazias, type EdicoesSite } from './edicoes.ts';

export interface VersaoEdicoes { id: number; dados: EdicoesSite; criado: number; nota: string }

const vazia = (): VersaoEdicoes => ({ id: 0, dados: edicoesVazias(), criado: 0, nota: '' });
let cache: { t: number; v: VersaoEdicoes } | null = null;
const CACHE_MS = 10_000;

function ler(r: { id: number; dados: string; criado: number; nota: string | null } | null): VersaoEdicoes | null {
  if (!r) return null;
  const d = JSON.parse(r.dados);
  return { id: r.id, dados: { ...edicoesVazias(), ...d, raridades: { cartas: {}, conjuradores: {}, ...(d.raridades ?? {}) } }, criado: r.criado, nota: r.nota ?? '' };
}

/** A versão em vigor (a mais nova). Sem tabela ou sem edições: nenhuma edição. */
export async function edicoesAtuais(env: Env): Promise<VersaoEdicoes> {
  if (cache && Date.now() - cache.t < CACHE_MS) return cache.v;
  let v = vazia();
  try {
    v = ler(await env.DB.prepare('SELECT id, dados, criado, nota FROM edicoes ORDER BY id DESC LIMIT 1').first()) ?? vazia();
  } catch (err) { console.error('edições', err); }
  cache = { t: Date.now(), v };
  return v;
}

/** Uma versão antiga (a partida online em andamento usa a do seu início). 0 = nenhuma edição. */
export async function edicoesVersao(env: Env, id: number): Promise<VersaoEdicoes | null> {
  if (!id) return vazia();
  return ler(await env.DB.prepare('SELECT id, dados, criado, nota FROM edicoes WHERE id = ?').bind(id).first());
}

/** Depois de salvar, a próxima leitura busca de novo. */
export const esquecerEdicoes = () => { cache = null; };
