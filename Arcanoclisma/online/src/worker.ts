// Worker do Arcanoclisma online: a API (/api/...) e o site estático (public/, servido pelo Cloudflare).
import { validarBaralho } from '../../tcg/src/motor.ts';
import type { BaralhoDef, Catalogo } from '../../tcg/src/tipos.ts';
import { agora, erro, json, novoId, rotasConta, usuarioDaSessao, type Env, type Usuario } from './conta.ts';
import DADOS from './gerado/dados.json';
import { aplicarCatalogo, contarEdicoes } from './edicoes.ts';
import { edicoesAtuais, edicoesVersao, salvarDiferenca } from './edicoes-db.ts';

export { Partida } from './partida.ts';
export { Saguao } from './saguao.ts';

type Lista = [string, number][];
const catalogo = DADOS.catalogo as unknown as Catalogo;
const texto = (v: unknown, max: number) => String(v ?? '').replace(/\r/g, '').trim().slice(0, max);

/** Confere um baralho do construtor: 60 cartas, Valores do Conjurador, cópias e itens (um por espaço). */
export function conferirBaralho(conjurador: string, lista: Lista, itens: string[], cat: Catalogo = catalogo): string[] {
  const erros: string[] = [];
  for (const [id, n] of lista) {
    if (!Number.isInteger(n) || n < 1) erros.push(`Quantidade inválida de ${id}.`);
    else if (n > DADOS.maxCopias && !DADOS.semLimite.includes(id)) erros.push(`${catalogo.cartas[id]?.nome ?? id}: no máximo ${DADOS.maxCopias} cópias.`);
  }
  const b: BaralhoDef = { conjurador, cartas: lista.flatMap(([id, n]) => Array(Math.max(0, Math.min(60, n | 0))).fill(id)), itens };
  return [...erros, ...validarBaralho(cat, b, DADOS.tamanhoBaralho)];
}

/** Baralho salvo como sai da API, com `erros` quando deixou de valer (ex.: regra das cores, decisão 37). Não apaga nada:
 *  o site marca o baralho como inválido e o Worker recusa usá-lo em partida até ser corrigido. */
function comErros(x: Record<string, unknown>, cat: Catalogo) {
  const lista = JSON.parse(x.lista as string) as Lista, itens = JSON.parse(x.itens as string) as string[];
  const erros = conferirBaralho(String(x.conjurador), lista, itens, cat);
  return { ...x, lista, itens, erros: erros.length ? erros : undefined };
}

/** Resolve a referência de baralho escolhida no site: "pronto:<conjurador>", "meu:<id>" ou "publico:<id>". */
export async function resolverBaralho(env: Env, u: Usuario, ref: string, itensEscolhidos?: string[]): Promise<{ def: BaralhoDef; nome: string; estilo?: unknown } | string> {
  const [tipo, id] = ref.split(':');
  let conj: string, lista: Lista, itens: string[], nome: string, estilo: unknown;
  if (tipo === 'pronto') {
    const p = DADOS.baralhos.find((x) => x.conjurador === id);
    if (!p) return 'Baralho pronto desconhecido.';
    conj = p.conjurador; lista = p.lista as Lista; itens = p.itens; nome = p.nome; estilo = p.estilo;
  } else if (tipo === 'meu' || tipo === 'publico') {
    const r = await env.DB.prepare('SELECT * FROM baralhos WHERE id = ?').bind(id).first<{ usuario: string; conjurador: string; lista: string; itens: string; publico: number; nome: string }>();
    if (!r || (r.usuario !== u.id && !r.publico)) return 'Baralho não encontrado.';
    conj = r.conjurador; lista = JSON.parse(r.lista); itens = JSON.parse(r.itens); nome = r.nome;
  } else return 'Baralho inválido.';
  if (itensEscolhidos) itens = itensEscolhidos;
  const erros = conferirBaralho(conj, lista, itens, aplicarCatalogo(catalogo, (await edicoesAtuais(env)).dados));
  if (erros.length) return `${nome}: o baralho não vale (corrija no construtor). ${erros.join(' ')}`;
  return { def: { conjurador: conj, cartas: lista.flatMap(([c, n]) => Array(n).fill(c)), itens }, nome, estilo };
}

async function corpo<T>(req: Request): Promise<T> {
  try { return await req.json<T>(); } catch { return {} as T; }
}

async function rotasBaralhos(req: Request, env: Env, u: Usuario | null, caminho: string): Promise<Response | null> {
  if (caminho === '/api/baralhos/publicos' && req.method === 'GET') {
    const r = await env.DB.prepare('SELECT b.id, b.nome, b.conjurador, b.lista, b.itens, b.atualizado, u.nome AS autor FROM baralhos b JOIN usuarios u ON u.id = b.usuario WHERE b.publico = 1 ORDER BY b.atualizado DESC LIMIT 200').all();
    const cat = aplicarCatalogo(catalogo, (await edicoesAtuais(env)).dados);
    return json({ baralhos: r.results.map((x: Record<string, unknown>) => comErros(x, cat)) });
  }
  if (!caminho.startsWith('/api/baralhos')) return null;
  if (!u) return erro('Entre na sua conta.', 401);
  if (caminho === '/api/baralhos' && req.method === 'GET') {
    const r = await env.DB.prepare('SELECT id, nome, conjurador, lista, itens, publico, atualizado FROM baralhos WHERE usuario = ? ORDER BY atualizado DESC').bind(u.id).all();
    const cat = aplicarCatalogo(catalogo, (await edicoesAtuais(env)).dados);
    return json({ baralhos: r.results.map((x: Record<string, unknown>) => ({ ...comErros(x, cat), publico: !!x.publico })) });
  }
  if (caminho === '/api/baralhos' && req.method === 'POST') {
    const b = await corpo<{ id?: string; nome: string; conjurador: string; lista: Lista; itens: string[]; publico?: boolean }>(req);
    const lista = (Array.isArray(b.lista) ? b.lista : []).filter((x) => Array.isArray(x)).map(([id, n]) => [String(id), Number(n)] as [string, number]);
    const itens = (Array.isArray(b.itens) ? b.itens : []).map(String);
    const erros = conferirBaralho(String(b.conjurador), lista, itens, aplicarCatalogo(catalogo, (await edicoesAtuais(env)).dados));
    if (erros.length) return json({ erro: 'O baralho ainda não está pronto.', erros }, 400);
    const nome = texto(b.nome, 50) || 'Meu baralho';
    if (b.id) {
      const r = await env.DB.prepare('UPDATE baralhos SET nome = ?, conjurador = ?, lista = ?, itens = ?, publico = ?, atualizado = ? WHERE id = ? AND usuario = ?')
        .bind(nome, b.conjurador, JSON.stringify(lista), JSON.stringify(itens), b.publico ? 1 : 0, agora(), b.id, u.id).run();
      if (!r.meta.changes) return erro('Baralho não encontrado.', 404);
      return json({ id: b.id });
    }
    const n = await env.DB.prepare('SELECT COUNT(*) AS n FROM baralhos WHERE usuario = ?').bind(u.id).first<{ n: number }>();
    if ((n?.n ?? 0) >= 60) return erro('Limite de 60 baralhos por conta.');
    const id = novoId('b');
    await env.DB.prepare('INSERT INTO baralhos (id, usuario, nome, conjurador, lista, itens, publico, criado, atualizado) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)')
      .bind(id, u.id, nome, b.conjurador, JSON.stringify(lista), JSON.stringify(itens), b.publico ? 1 : 0, agora(), agora()).run();
    return json({ id });
  }
  const m = caminho.match(/^\/api\/baralhos\/([\w-]+)$/);
  if (m && req.method === 'DELETE') {
    await env.DB.prepare('DELETE FROM baralhos WHERE id = ? AND usuario = ?').bind(m[1], u.id).run();
    return json({ ok: true });
  }
  return null;
}

const TIPOS_OPINIAO = ['opiniao', 'duvida', 'bug', 'balanceamento', 'carta', 'elogio', 'comentario'];
const ESTADOS_OPINIAO = ['novo', 'aprovado', 'revisar', 'recusado', 'feito'];
const CATEGORIAS_IDEIA = ['expansao', 'personagem', 'carta', 'item', 'conjurador', 'mecanica', 'outro'];
const ESTADOS_IDEIA = ['nova', 'aprovada', 'estudando', 'recusada', 'feita'];

/** Diagnóstico: só as opiniões e ideias que o autor marcou "Yes" (claude = 1). É o que o Claude lê. */
async function diagnostico(env: Env, formato: string | null): Promise<Response> {
  const ops = (await env.DB.prepare('SELECT o.id, o.tipo, o.nota, o.texto, o.contexto, o.estado, o.resposta, o.criado, u.nome AS autor FROM opinioes o JOIN usuarios u ON u.id = o.usuario WHERE o.pai IS NULL AND o.claude = 1 ORDER BY o.criado').all()).results as Record<string, unknown>[];
  const ids = ops.map((x) => x.id as string);
  const resps = ids.length ? (await env.DB.prepare(`SELECT o.pai, o.texto, u.nome AS autor FROM opinioes o JOIN usuarios u ON u.id = o.usuario WHERE o.pai IN (${ids.map(() => '?').join(',')}) ORDER BY o.criado`).bind(...ids).all()).results as Record<string, unknown>[] : [];
  const ideias = (await env.DB.prepare('SELECT i.id, i.categoria, i.titulo, i.texto, i.estado, i.resposta, i.votos, i.criado, u.nome AS autor FROM ideias i JOIN usuarios u ON u.id = i.usuario WHERE i.claude = 1 ORDER BY i.votos DESC, i.criado').all()).results as Record<string, unknown>[];
  const opinioes = ops.map((o) => ({ ...o, contexto: o.contexto ? JSON.parse(o.contexto as string) : null, respostas: resps.filter((r) => r.pai === o.id) }));
  if (formato !== 'md') return json({ opinioes, ideias });
  const data = (t: unknown) => new Date(Number(t)).toISOString().slice(0, 10);
  const md = ['# Diagnóstico — marcados "Yes" pelo autor', '', `## Opiniões (${opinioes.length})`, ''];
  for (const o of opinioes) {
    md.push(`### ${o.id} · ${o.tipo}${o.nota ? ` · nota ${o.nota}` : ''} · ${o.estado} · ${o.autor} · ${data(o.criado)}`, '', String(o.texto), '');
    const c = o.contexto as Record<string, unknown> | null;
    if (c) md.push(`Partida: ${c.modo ?? ''} turno ${c.turno} (${c.etapa}) ${c.eu} × ${c.oponente}; últimos acontecimentos: ${((c.registro as string[]) ?? []).slice(-12).join(' | ')}`, '');
    if (o.resposta) md.push(`Resposta do autor: ${o.resposta}`, '');
    for (const r of o.respostas) md.push(`- ${r.autor}: ${r.texto}`);
    if (o.respostas.length) md.push('');
  }
  md.push(`## Ideias (${ideias.length})`, '');
  for (const i of ideias) md.push(`### ${i.id} · ${i.categoria} · ${i.titulo} · ${i.votos} votos · ${i.estado} · ${i.autor}`, '', String(i.texto), ...(i.resposta ? ['', `Resposta do autor: ${i.resposta}`] : []), '');
  return new Response(md.join('\n'), { headers: { 'content-type': 'text/markdown; charset=utf-8', 'cache-control': 'no-store' } });
}

async function rotasComunidade(req: Request, env: Env, u: Usuario | null, caminho: string): Promise<Response | null> {
  if (caminho === '/api/diagnostico' && req.method === 'GET') return diagnostico(env, new URL(req.url).searchParams.get('formato'));
  if (caminho === '/api/opinioes' && req.method === 'GET') {
    const url = new URL(req.url);
    const estado = url.searchParams.get('estado');
    const tipo = url.searchParams.get('tipo');
    const filtros: string[] = ['o.pai IS NULL'];
    const args: unknown[] = [];
    if (estado && ESTADOS_OPINIAO.includes(estado)) { filtros.push('o.estado = ?'); args.push(estado); }
    const claude = url.searchParams.get('claude');
    if (claude === 'sim') filtros.push('o.claude = 1'); else if (claude === 'nao') filtros.push('o.claude = 0'); else if (claude === 'pendente') filtros.push('o.claude IS NULL');
    if (tipo && TIPOS_OPINIAO.includes(tipo)) { filtros.push('o.tipo = ?'); args.push(tipo); }
    const r = await env.DB.prepare(`SELECT o.*, u.nome AS autor FROM opinioes o JOIN usuarios u ON u.id = o.usuario WHERE ${filtros.join(' AND ')} ORDER BY o.criado DESC LIMIT 300`).bind(...args).all();
    const ids = r.results.map((x: Record<string, unknown>) => x.id as string);
    const respostas = ids.length
      ? (await env.DB.prepare(`SELECT o.id, o.pai, o.texto, o.criado, u.nome AS autor FROM opinioes o JOIN usuarios u ON u.id = o.usuario WHERE o.pai IN (${ids.map(() => '?').join(',')}) ORDER BY o.criado`).bind(...ids).all()).results
      : [];
    const stats = await env.DB.prepare('SELECT estado, COUNT(*) AS n FROM opinioes WHERE pai IS NULL GROUP BY estado').all();
    return json({ opinioes: r.results.map((x: Record<string, unknown>) => ({ ...x, contexto: x.contexto ? JSON.parse(x.contexto as string) : null, respostas: respostas.filter((y: Record<string, unknown>) => y.pai === x.id) })), contagem: stats.results });
  }
  if (caminho === '/api/ideias' && req.method === 'GET') {
    const cat = new URL(req.url).searchParams.get('categoria');
    const r = cat && CATEGORIAS_IDEIA.includes(cat)
      ? await env.DB.prepare('SELECT i.*, u.nome AS autor FROM ideias i JOIN usuarios u ON u.id = i.usuario WHERE categoria = ? ORDER BY votos DESC, criado DESC LIMIT 300').bind(cat).all()
      : await env.DB.prepare('SELECT i.*, u.nome AS autor FROM ideias i JOIN usuarios u ON u.id = i.usuario ORDER BY votos DESC, criado DESC LIMIT 300').all();
    const meus = u ? (await env.DB.prepare('SELECT ideia FROM votos WHERE usuario = ?').bind(u.id).all()).results.map((x: Record<string, unknown>) => x.ideia) : [];
    return json({ ideias: r.results, votei: meus });
  }
  if (req.method !== 'POST') return null;
  if (!caminho.startsWith('/api/opinioes') && !caminho.startsWith('/api/ideias')) return null;
  if (!u) return erro('Entre na sua conta.', 401);
  if (caminho === '/api/opinioes') {
    const b = await corpo<{ tipo: string; nota?: number; texto: string; contexto?: unknown }>(req);
    const t = texto(b.texto, 4000);
    if (t.length < 2) return erro('Escreva o comentário.');
    const tipo = TIPOS_OPINIAO.includes(b.tipo) ? b.tipo : 'opiniao';
    const nota = Number.isInteger(b.nota) && b.nota! >= 1 && b.nota! <= 5 ? b.nota : null;
    const ctx = b.contexto ? JSON.stringify(b.contexto).slice(0, 20000) : null;
    const id = novoId('o');
    await env.DB.prepare('INSERT INTO opinioes (id, usuario, tipo, nota, texto, contexto, criado) VALUES (?, ?, ?, ?, ?, ?, ?)').bind(id, u.id, tipo, nota, t, ctx, agora()).run();
    return json({ id });
  }
  let m = caminho.match(/^\/api\/opinioes\/([\w-]+)\/responder$/);
  if (m) {
    const b = await corpo<{ texto: string }>(req);
    const t = texto(b.texto, 2000);
    if (t.length < 1) return erro('Escreva a resposta.');
    const pai = await env.DB.prepare('SELECT id FROM opinioes WHERE id = ? AND pai IS NULL').bind(m[1]).first();
    if (!pai) return erro('Comentário não encontrado.', 404);
    await env.DB.prepare("INSERT INTO opinioes (id, usuario, tipo, texto, pai, criado) VALUES (?, ?, 'comentario', ?, ?, ?)").bind(novoId('o'), u.id, t, m[1], agora()).run();
    return json({ ok: true });
  }
  m = caminho.match(/^\/api\/opinioes\/([\w-]+)\/estado$/);
  if (m) {
    if (!u.admin) return erro('Só o autor decide.', 403);
    const b = await corpo<{ estado: string; resposta?: string }>(req);
    if (!ESTADOS_OPINIAO.includes(b.estado)) return erro('Estado inválido.');
    await env.DB.prepare('UPDATE opinioes SET estado = ?, resposta = ? WHERE id = ?').bind(b.estado, texto(b.resposta, 2000) || null, m[1]).run();
    return json({ ok: true });
  }
  m = caminho.match(/^\/api\/(opinioes|ideias)\/([\w-]+)\/claude$/);
  if (m) {
    if (!u.admin) return erro('Só o autor decide.', 403);
    const b = await corpo<{ ler: boolean | null }>(req);
    const v = b.ler === true ? 1 : b.ler === false ? 0 : null;
    await env.DB.prepare(`UPDATE ${m[1]} SET claude = ? WHERE id = ?`).bind(v, m[2]).run();
    return json({ ok: true, claude: v });
  }
  if (caminho === '/api/ideias') {
    const b = await corpo<{ categoria: string; titulo: string; texto: string }>(req);
    const titulo = texto(b.titulo, 120);
    const t = texto(b.texto, 6000);
    if (titulo.length < 3 || t.length < 3) return erro('Dê um título e descreva a ideia.');
    const id = novoId('i');
    await env.DB.prepare('INSERT INTO ideias (id, usuario, categoria, titulo, texto, criado) VALUES (?, ?, ?, ?, ?, ?)')
      .bind(id, u.id, CATEGORIAS_IDEIA.includes(b.categoria) ? b.categoria : 'outro', titulo, t, agora()).run();
    return json({ id });
  }
  m = caminho.match(/^\/api\/ideias\/([\w-]+)\/votar$/);
  if (m) {
    const r = await env.DB.prepare('INSERT OR IGNORE INTO votos (ideia, usuario) VALUES (?, ?)').bind(m[1], u.id).run();
    if (r.meta.changes) await env.DB.prepare('UPDATE ideias SET votos = votos + 1 WHERE id = ?').bind(m[1]).run();
    else {
      await env.DB.prepare('DELETE FROM votos WHERE ideia = ? AND usuario = ?').bind(m[1], u.id).run();
      await env.DB.prepare('UPDATE ideias SET votos = MAX(0, votos - 1) WHERE id = ?').bind(m[1]).run();
    }
    return json({ ok: true });
  }
  m = caminho.match(/^\/api\/ideias\/([\w-]+)\/estado$/);
  if (m) {
    if (!u.admin) return erro('Só o autor decide.', 403);
    const b = await corpo<{ estado: string; resposta?: string }>(req);
    if (!ESTADOS_IDEIA.includes(b.estado)) return erro('Estado inválido.');
    await env.DB.prepare('UPDATE ideias SET estado = ?, resposta = ? WHERE id = ?').bind(b.estado, texto(b.resposta, 2000) || null, m[1]).run();
    return json({ ok: true });
  }
  return null;
}

/** Editor do autor no site: ler as edições (todos) e salvar uma versão nova (só o autor). */
async function rotasEdicoes(req: Request, env: Env, u: Usuario | null, caminho: string): Promise<Response | null> {
  if (!caminho.startsWith('/api/edicoes')) return null;
  if (caminho === '/api/edicoes' && req.method === 'GET') {
    const q = new URL(req.url).searchParams;
    const e = q.has('id') ? await edicoesVersao(env, Number(q.get('id')) | 0) : await edicoesAtuais(env);
    if (!e) return erro('Versão de edições não encontrada.', 404);
    return json(e);
  }
  if (!u?.admin) return erro('Só o autor edita as cartas.', 403);
  if (caminho === '/api/edicoes' && req.method === 'POST') {
    // Concorrência otimista e só a diferença (bug de 05/10): ver salvarDiferenca em edicoes-db.ts
    const r = await salvarDiferenca(env, catalogo, await corpo<{ base?: unknown; diff?: unknown; nota?: string }>(req), u.id);
    if (!r.ok) return json(r.corpo, r.status);
    // Aviso (não bloqueia): baralhos prontos que deixam de valer com as edições (ex.: Valores trocados).
    const cat = aplicarCatalogo(catalogo, r.dados);
    const avisos = DADOS.baralhos.flatMap((p) => conferirBaralho(p.conjurador, p.lista as Lista, p.itens, cat).map((x) => `${p.nome}: ${x}`));
    return json({ id: r.id, base: r.base, dados: r.dados, mudancas: contarEdicoes(r.dados), avisos });
  }
  if (caminho === '/api/edicoes/historico' && req.method === 'GET') {
    const r = await env.DB.prepare('SELECT e.id, e.nota, e.criado, e.dados, u.nome FROM edicoes e LEFT JOIN usuarios u ON u.id = e.usuario ORDER BY e.id DESC LIMIT 60').all<{ id: number; nota: string; criado: number; dados: string; nome: string }>();
    return json({ versoes: r.results.map((x) => ({ id: x.id, nota: x.nota, criado: x.criado, autor: x.nome, mudancas: contarEdicoes(JSON.parse(x.dados)) })) });
  }
  return erro('Rota desconhecida.', 404);
}

async function rotasPartidas(req: Request, env: Env, u: Usuario | null, caminho: string): Promise<Response | null> {
  const saguao = () => env.SAGUAO.get(env.SAGUAO.idFromName('saguao'));
  if (caminho === '/api/salas' && req.method === 'GET') return saguao().fetch('https://saguao/listar');
  if (caminho === '/api/estatisticas' && req.method === 'GET') {
    const r = await env.DB.prepare("SELECT conj0, conj1, vencedor, modo FROM partidas WHERE vencedor IS NOT NULL AND fim > ?").bind(agora() - 30 * 864e5).all();
    const por: Record<string, { jogos: number; vitorias: number }> = {};
    for (const x of r.results as { conj0: string; conj1: string; vencedor: number }[]) {
      for (const [k, c] of [[0, x.conj0], [1, x.conj1]] as [number, string][]) {
        por[c] ??= { jogos: 0, vitorias: 0 };
        por[c].jogos++;
        if (x.vencedor === k) por[c].vitorias++;
      }
    }
    return json({ porConjurador: por, total: r.results.length });
  }
  if (!u) return caminho.startsWith('/api/salas') || caminho.startsWith('/api/partida') ? erro('Entre na sua conta.', 401) : null;
  if (caminho === '/api/salas' && req.method === 'POST') {
    const b = await corpo<{ nome?: string; privada?: boolean }>(req);
    return saguao().fetch('https://saguao/criar', { method: 'POST', body: JSON.stringify({ nome: texto(b.nome, 40) || `Sala de ${u.nome}`, privada: !!b.privada, dono: u.id, donoNome: u.nome }) });
  }
  if (caminho === '/api/salas/rapida' && req.method === 'POST') return saguao().fetch('https://saguao/rapida', { method: 'POST', body: JSON.stringify({ usuario: u.id, nome: u.nome }) });
  let m = caminho.match(/^\/api\/salas\/codigo\/(\w+)$/);
  if (m) return saguao().fetch('https://saguao/codigo/' + m[1].toUpperCase());
  m = caminho.match(/^\/api\/partida\/([\w-]+)\/ws$/);
  if (m) {
    if (req.headers.get('upgrade')?.toLowerCase() !== 'websocket') return erro('Use WebSocket.', 426);
    const url = new URL(req.url);
    const itens = url.searchParams.get('itens');
    const baralho = await resolverBaralho(env, u, url.searchParams.get('baralho') ?? '', itens ? itens.split(',').filter(Boolean) : undefined);
    if (typeof baralho === 'string') return erro(baralho);
    const cab = new Headers(req.headers);
    cab.set('x-arc-usuario', JSON.stringify({ id: u.id, nome: u.nome }));
    cab.set('x-arc-baralho', JSON.stringify(baralho));
    const stub = env.PARTIDA.get(env.PARTIDA.idFromName(m[1]));
    return stub.fetch(new Request(`https://partida/${m[1]}/ws`, { headers: cab }));
  }
  if (caminho === '/api/partidas/bot' && req.method === 'POST') {
    // Resultado de partida contra o bot (só estatística; o jogo contra o bot roda no navegador).
    const b = await corpo<{ conjurador: string; oponente: string; venci: boolean | null; turnos: number; modo: string }>(req);
    if (!catalogo.conjuradores[b.conjurador] || !catalogo.conjuradores[b.oponente]) return erro('Conjurador inválido.');
    await env.DB.prepare('INSERT INTO partidas (id, modo, j0, j1, conj0, conj1, vencedor, turnos, motivo, inicio, fim, versao) VALUES (?, ?, ?, NULL, ?, ?, ?, ?, ?, ?, ?, ?)')
      .bind(novoId('p'), texto(b.modo, 20) || 'bot', u.id, b.conjurador, b.oponente, b.venci === null ? null : b.venci ? 0 : 1, Number(b.turnos) | 0, 'fim', agora(), agora(), DADOS.versao).run();
    return json({ ok: true });
  }
  return null;
}

export default {
  async fetch(req: Request, env: Env): Promise<Response> {
    const url = new URL(req.url);
    const caminho = url.pathname;
    if (!caminho.startsWith('/api/')) return env.ASSETS.fetch(req);
    try {
      const conta = await rotasConta(req, env, caminho);
      if (conta) return conta;
      const u = await usuarioDaSessao(req, env);
      return (await rotasEdicoes(req, env, u, caminho))
        ?? (await rotasBaralhos(req, env, u, caminho))
        ?? (await rotasComunidade(req, env, u, caminho))
        ?? (await rotasPartidas(req, env, u, caminho))
        ?? erro('Rota desconhecida.', 404);
    } catch (e) {
      console.error(e);
      return erro('Erro no servidor: ' + (e instanceof Error ? e.message : String(e)), 500);
    }
  },
};
