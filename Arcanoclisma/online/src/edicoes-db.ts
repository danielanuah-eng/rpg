// Leitura das edições do autor no D1 (tabela edicoes). Separado de edicoes.ts, que também roda no navegador.
import type { Env } from './conta.ts';
import type { Catalogo } from '../../tcg/src/tipos.ts';
import { aplicarDiferenca, conflitosEdicoes, edicoesVazias, sanitizarEdicoes, type ConflitoEdicao, type EdicoesSite } from './edicoes.ts';

export interface VersaoEdicoes { id: number; dados: EdicoesSite; criado: number; nota: string }

const vazia = (): VersaoEdicoes => ({ id: 0, dados: edicoesVazias(), criado: 0, nota: '' });
let cache: { t: number; v: VersaoEdicoes } | null = null;
const CACHE_MS = 10_000;

function ler(r: { id: number; dados: string; criado: number; nota: string | null } | null): VersaoEdicoes | null {
  if (!r) return null;
  const d = JSON.parse(r.dados);
  return { id: r.id, dados: { ...edicoesVazias(), ...d, raridades: { cartas: {}, conjuradores: {}, ...(d.raridades ?? {}) } }, criado: r.criado, nota: r.nota ?? '' };
}

/** A versão em vigor (a mais nova). Sem tabela ou sem edições: nenhuma edição. `fresca`: sem o cache (ao salvar). */
export async function edicoesAtuais(env: Env, fresca = false): Promise<VersaoEdicoes> {
  if (!fresca && cache && Date.now() - cache.t < CACHE_MS) return cache.v;
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

export type ResultadoSalvar =
  | { ok: true; id: number; base: number; dados: EdicoesSite }
  | { ok: false; status: number; corpo: { erro: string; atual?: VersaoEdicoes; conflitos?: ConflitoEdicao[] | null } };

/**
 * Salva uma versão nova com concorrência otimista (bug de 05/10: salvamentos no mesmo segundo; um rascunho velho
 * apagou 37 cartas). O Editor manda só a diferença (`diff`) contra a versão em que se apoia (`base`). Se a versão
 * em vigor for outra, nada é gravado: 409 com a atual e as cartas mudadas nos dois lados. O INSERT é condicional
 * (um comando só), então dois envios juntos com a mesma base não passam os dois.
 */
export async function salvarDiferenca(env: Env, cat: Catalogo, b: { base?: unknown; diff?: unknown; nota?: string }, usuario: string): Promise<ResultadoSalvar> {
  if (!Number.isInteger(b.base) || !b.diff || typeof b.diff !== 'object' || Array.isArray(b.diff))
    return { ok: false, status: 409, corpo: { erro: 'Este Editor está desatualizado: recarregue a página (Ctrl+F5) antes de salvar. Nada foi gravado.' } };
  if (JSON.stringify(b.diff).length > 400_000) return { ok: false, status: 400, corpo: { erro: 'Edições grandes demais.' } };
  const base = b.base as number;
  const conflito = async (atual: VersaoEdicoes): Promise<ResultadoSalvar> => {
    const antes = await edicoesVersao(env, base);
    return { ok: false, status: 409, corpo: { erro: `O site já tem a versão #${atual.id}, mais nova que a do seu rascunho (#${base}). Nada foi gravado.`, atual, conflitos: antes ? conflitosEdicoes(antes.dados, atual.dados, b.diff) : null } };
  };
  const atual = await edicoesAtuais(env, true);
  if (atual.id !== base) return conflito(atual);
  const dados = sanitizarEdicoes(aplicarDiferenca(atual.dados, b.diff), cat);
  const nota = String(b.nota ?? '').replace(/\r/g, '').trim().slice(0, 500);
  const r = await env.DB.prepare('INSERT INTO edicoes (dados, nota, usuario, criado) SELECT ?, ?, ?, ? WHERE COALESCE((SELECT MAX(id) FROM edicoes), 0) = ?')
    .bind(JSON.stringify(dados), nota, usuario, Date.now(), atual.id).run();
  esquecerEdicoes();
  if (!r.meta.changes) return conflito(await edicoesAtuais(env, true));
  return { ok: true, id: Number(r.meta.last_row_id), base: atual.id, dados };
}
