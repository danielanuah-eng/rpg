// Contas: e-mail e senha (PBKDF2 do WebCrypto) ou Google (token de identidade conferido no Google), sessões por cookie.
// O site não manda e-mail de confirmação, então uma conta de senha não prova que a pessoa é dona do e-mail.
// Por isso o painel do autor só abre para conta "verificada": entrou pelo Google, ou usou a chave do autor (segredo AUTOR_CHAVE).
// Entrar pelo Google numa conta de senha não verificada apaga a senha e as sessões antigas (expulsa quem tomou o e-mail).
export interface Env {
  DB: D1Database;
  ASSETS: Fetcher;
  PARTIDA: DurableObjectNamespace;
  SAGUAO: DurableObjectNamespace;
  ADMIN_EMAILS?: string;
  GOOGLE_CLIENT_ID?: string;
  AUTOR_CHAVE?: string;
}
export interface Usuario { id: string; email: string; nome: string; admin: boolean }

const COOKIE = 'arc_sessao';
const DIAS_SESSAO = 30;
const ITERACOES = 60_000;

export const agora = () => Date.now();
export const novoId = (prefixo = '') => prefixo + crypto.randomUUID().replace(/-/g, '').slice(0, 16);
const hex = (b: ArrayBuffer | Uint8Array) => [...new Uint8Array(b)].map((x) => x.toString(16).padStart(2, '0')).join('');

export function json(dados: unknown, status = 200, extra: Record<string, string> = {}): Response {
  return new Response(JSON.stringify(dados), { status, headers: { 'content-type': 'application/json; charset=utf-8', 'cache-control': 'no-store', ...extra } });
}
export const erro = (msg: string, status = 400) => json({ erro: msg }, status);

async function hashSenha(senha: string, sal: string): Promise<string> {
  const chave = await crypto.subtle.importKey('raw', new TextEncoder().encode(senha), 'PBKDF2', false, ['deriveBits']);
  const bits = await crypto.subtle.deriveBits({ name: 'PBKDF2', hash: 'SHA-256', salt: new TextEncoder().encode(sal), iterations: ITERACOES }, chave, 256);
  return hex(bits);
}
function iguais(a: string, b: string): boolean {
  if (a.length !== b.length) return false;
  let d = 0;
  for (let i = 0; i < a.length; i++) d |= a.charCodeAt(i) ^ b.charCodeAt(i);
  return d === 0;
}

const naLista = (env: Env, email: string) => (env.ADMIN_EMAILS ?? '').split(',').map((s) => s.trim().toLowerCase()).filter(Boolean).includes(email.toLowerCase());
type Linha = { id: string; email: string; nome: string; verificado: number };
const paraUsuario = (env: Env, r: Linha): Usuario => ({ id: r.id, email: r.email, nome: r.nome, admin: naLista(env, r.email) && r.verificado === 1 });

function lerCookie(req: Request, nome: string): string | null {
  const c = req.headers.get('cookie') ?? '';
  for (const parte of c.split(';')) {
    const [k, ...v] = parte.trim().split('=');
    if (k === nome) return decodeURIComponent(v.join('='));
  }
  return null;
}

export async function usuarioDaSessao(req: Request, env: Env): Promise<Usuario | null> {
  const token = lerCookie(req, COOKIE);
  if (!token) return null;
  const r = await env.DB.prepare('SELECT u.id, u.email, u.nome, u.verificado FROM sessoes s JOIN usuarios u ON u.id = s.usuario WHERE s.token = ? AND s.expira > ?')
    .bind(token, agora()).first<Linha>();
  return r ? paraUsuario(env, r) : null;
}

async function abrirSessao(env: Env, usuario: string, req: Request): Promise<string> {
  const token = hex(crypto.getRandomValues(new Uint8Array(32)));
  await env.DB.prepare('INSERT INTO sessoes (token, usuario, expira) VALUES (?, ?, ?)').bind(token, usuario, agora() + DIAS_SESSAO * 864e5).run();
  const seguro = new URL(req.url).protocol === 'https:' ? '; Secure' : '';
  return `${COOKIE}=${token}; Path=/; HttpOnly; SameSite=Lax; Max-Age=${DIAS_SESSAO * 86400}${seguro}`;
}

const emailValido = (e: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(e) && e.length <= 200;
const limparNome = (n: string) => n.replace(/[<>]/g, '').trim().slice(0, 40);
const linhaPorEmail = (env: Env, email: string) =>
  env.DB.prepare('SELECT id, email, nome, verificado, senha, sal FROM usuarios WHERE email = ?').bind(email).first<Linha & { senha: string | null; sal: string | null }>();

export async function rotasConta(req: Request, env: Env, caminho: string): Promise<Response | null> {
  if (caminho === '/api/conta/eu' && req.method === 'GET') {
    const u = await usuarioDaSessao(req, env);
    return json({ usuario: u, googleClientId: env.GOOGLE_CLIENT_ID || null });
  }
  if (req.method !== 'POST') return null;
  if (caminho === '/api/conta/sair') {
    const token = lerCookie(req, COOKIE);
    if (token) await env.DB.prepare('DELETE FROM sessoes WHERE token = ?').bind(token).run();
    return json({ ok: true }, 200, { 'set-cookie': `${COOKIE}=; Path=/; HttpOnly; SameSite=Lax; Max-Age=0` });
  }
  if (caminho === '/api/conta/nome') {
    const u = await usuarioDaSessao(req, env);
    if (!u) return erro('Entre na sua conta.', 401);
    const { nome } = await req.json<{ nome: string }>();
    const n = limparNome(String(nome ?? ''));
    if (n.length < 2) return erro('Escolha um nome com pelo menos 2 letras.');
    await env.DB.prepare('UPDATE usuarios SET nome = ? WHERE id = ?').bind(n, u.id).run();
    return json({ usuario: { ...u, nome: n } });
  }
  if (caminho === '/api/conta/registrar') {
    const b = await req.json<{ email: string; senha: string; nome: string }>();
    const email = String(b.email ?? '').trim().toLowerCase();
    const senha = String(b.senha ?? '');
    const nome = limparNome(String(b.nome ?? '')) || email.split('@')[0];
    if (!emailValido(email)) return erro('E-mail inválido.');
    if (senha.length < 8) return erro('A senha precisa ter pelo menos 8 caracteres.');
    const existe = await linhaPorEmail(env, email);
    if (existe) return erro(existe.senha ? 'Já existe uma conta com este e-mail. Use "Entrar".' : 'Este e-mail entra pelo Google. Use "Entrar com Google".');
    const sal = hex(crypto.getRandomValues(new Uint8Array(16)));
    const id = novoId('u');
    await env.DB.prepare('INSERT INTO usuarios (id, email, nome, senha, sal, criado) VALUES (?, ?, ?, ?, ?, ?)').bind(id, email, nome, await hashSenha(senha, sal), sal, agora()).run();
    const cookie = await abrirSessao(env, id, req);
    return json({ usuario: paraUsuario(env, { id, email, nome, verificado: 0 }) }, 200, { 'set-cookie': cookie });
  }
  if (caminho === '/api/conta/entrar') {
    const b = await req.json<{ email: string; senha: string }>();
    const r = await linhaPorEmail(env, String(b.email ?? '').trim().toLowerCase());
    if (!r?.senha || !r.sal || !iguais(await hashSenha(String(b.senha ?? ''), r.sal), r.senha)) return erro('E-mail ou senha incorretos.', 401);
    const cookie = await abrirSessao(env, r.id, req);
    return json({ usuario: paraUsuario(env, r) }, 200, { 'set-cookie': cookie });
  }
  if (caminho === '/api/conta/google') {
    if (!env.GOOGLE_CLIENT_ID) return erro('Entrar com Google ainda não foi configurado.');
    const { credential } = await req.json<{ credential: string }>();
    const r = await fetch('https://oauth2.googleapis.com/tokeninfo?id_token=' + encodeURIComponent(String(credential ?? '')));
    if (!r.ok) return erro('Não consegui confirmar a conta Google.', 401);
    const t = await r.json<{ aud: string; email: string; email_verified: string | boolean; sub: string; name?: string }>();
    if (t.aud !== env.GOOGLE_CLIENT_ID || !(t.email_verified === true || t.email_verified === 'true')) return erro('Conta Google não confirmada.', 401);
    const email = t.email.toLowerCase();
    let u = await linhaPorEmail(env, email);
    if (!u) {
      const nome = limparNome(t.name ?? '') || email.split('@')[0];
      const id = novoId('u');
      await env.DB.prepare('INSERT INTO usuarios (id, email, nome, google, verificado, criado) VALUES (?, ?, ?, ?, 1, ?)').bind(id, email, nome, t.sub, agora()).run();
      u = { id, email, nome, verificado: 1, senha: null, sal: null };
    } else {
      if (u.verificado !== 1) {
        await env.DB.prepare('UPDATE usuarios SET senha = NULL, sal = NULL WHERE id = ?').bind(u.id).run();
        await env.DB.prepare('DELETE FROM sessoes WHERE usuario = ?').bind(u.id).run();
      }
      await env.DB.prepare('UPDATE usuarios SET google = ?, verificado = 1 WHERE id = ?').bind(t.sub, u.id).run();
      u = { ...u, verificado: 1 };
    }
    const cookie = await abrirSessao(env, u.id, req);
    return json({ usuario: paraUsuario(env, u) }, 200, { 'set-cookie': cookie });
  }
  if (caminho === '/api/conta/autor') {
    // Chave do autor: confirma a conta de um e-mail da lista ADMIN_EMAILS e define a senha dela (expulsa sessões antigas).
    const b = await req.json<{ email: string; senha: string; chave: string }>();
    const email = String(b.email ?? '').trim().toLowerCase();
    if (!env.AUTOR_CHAVE || !iguais(String(b.chave ?? ''), env.AUTOR_CHAVE) || !naLista(env, email)) return erro('Chave do autor incorreta.', 401);
    if (String(b.senha ?? '').length < 8) return erro('A senha precisa ter pelo menos 8 caracteres.');
    const sal = hex(crypto.getRandomValues(new Uint8Array(16)));
    const h = await hashSenha(String(b.senha), sal);
    let u = await linhaPorEmail(env, email);
    if (!u) {
      const id = novoId('u');
      await env.DB.prepare('INSERT INTO usuarios (id, email, nome, senha, sal, verificado, criado) VALUES (?, ?, ?, ?, ?, 1, ?)').bind(id, email, 'Autor', h, sal, agora()).run();
      u = { id, email, nome: 'Autor', verificado: 1, senha: h, sal };
    } else {
      await env.DB.prepare('UPDATE usuarios SET senha = ?, sal = ?, verificado = 1 WHERE id = ?').bind(h, sal, u.id).run();
      await env.DB.prepare('DELETE FROM sessoes WHERE usuario = ?').bind(u.id).run();
      u = { ...u, verificado: 1 };
    }
    const cookie = await abrirSessao(env, u.id, req);
    return json({ usuario: paraUsuario(env, u) }, 200, { 'set-cookie': cookie });
  }
  return null;
}
