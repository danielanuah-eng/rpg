// Utilidades comuns do site: dados do jogo, chamadas à API, conta, avisos e janelas.
import { VALOR, TIPO, textoCarta, textoPoder, valoresTxt } from '/jogo/textos.js';
import { aplicarCatalogo, edicoesVazias } from '/jogo/edicoes.js';

export { VALOR, TIPO, textoCarta, textoPoder, valoresTxt };
export const $ = (s, r = document) => r.querySelector(s);
export const $$ = (s, r = document) => [...r.querySelectorAll(s)];
export const esc = (t) => String(t ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

export const DADOS = await (await fetch('/jogo/dados.json')).json();
/** Carta usada no lugar das escondidas (mão do oponente, baralhos) nas partidas online. */
DADOS.catalogo.cartas.oculta = { id: 'oculta', nome: 'Carta virada', tipo: 'oculta', subtipos: [], valores: [], custo: 0, grau: 0, velocidade: 'findante', arcanoGerado: 0, cenario: '', texto: '' };

export const CONJS = Object.values(DADOS.catalogo.conjuradores);
export const conj = (id) => DADOS.catalogo.conjuradores[id];
export const carta = (id) => DADOS.catalogo.cartas[id];
export const nomeCurto = (c) => c.nome.split(',')[0];
export const arteConj = (id) => `/arte/${DADOS.conjuradoresArte[id]}.webp`;
export const cenario = (k) => `/arte/cenarios/${DADOS.cenarios[k] ?? DADOS.cenarios.menu}.webp`;
export const cenarioValor = (v) => `/arte/cenarios/${DADOS.cenarios.valores[v]}.webp`;
export const COR_VALOR = { conflituo: '#c45a3f', sublimeyn: '#8fb4c8', dominum: '#8a5a98', persona: '#d27d8b', alvorum: '#c9a24b', morbidyum: '#7fa682', cognitium: '#6f7fd0', eternyr: '#5274a8', primitivum: '#8a9b4c' };

// ---------- API ----------
export async function api(caminho, { metodo, corpo } = {}) {
  const r = await fetch('/api' + caminho, {
    method: metodo ?? (corpo ? 'POST' : 'GET'),
    headers: corpo ? { 'content-type': 'application/json' } : {},
    body: corpo ? JSON.stringify(corpo) : undefined,
    credentials: 'same-origin',
  });
  let d = null;
  try { d = await r.json(); } catch { /* sem corpo */ }
  if (!r.ok) {
    const e = new Error(d?.erro ?? `Erro ${r.status}`);
    e.status = r.status;
    e.dados = d;
    throw e;
  }
  return d;
}

// ---------- conta ----------
export const sessao = { usuario: null, googleClientId: null, carregada: false };
const ouvintesConta = new Set();
export const aoMudarConta = (f) => ouvintesConta.add(f);
export function definirUsuario(u) {
  sessao.usuario = u;
  for (const f of ouvintesConta) f(u);
}
export async function carregarSessao() {
  try {
    const r = await api('/conta/eu');
    sessao.googleClientId = r.googleClientId;
    sessao.carregada = true;
    definirUsuario(r.usuario);
  } catch {
    sessao.carregada = true;
    definirUsuario(null);
  }
}

// ---------- avisos ----------
export function avisar(texto, tipo = 'info', ms = 3800) {
  const el = document.createElement('div');
  el.className = `aviso aviso-${tipo}`;
  el.textContent = texto;
  $('#avisos').append(el);
  setTimeout(() => el.classList.add('saindo'), ms);
  setTimeout(() => el.remove(), ms + 400);
}

// ---------- janelas ----------
/** Abre uma janela por cima da tela. Devolve { el, fechar }. */
export function janela(html, { classe = '', aoFechar, fixa = false } = {}) {
  const fundo = document.createElement('div');
  fundo.className = 'janela-fundo';
  fundo.innerHTML = `<div class="janela ${classe}" role="dialog" aria-modal="true">${fixa ? '' : '<button class="janela-x" aria-label="Fechar">×</button>'}${html}</div>`;
  $('#camada').append(fundo);
  const fechar = () => { fundo.remove(); document.removeEventListener('keydown', tecla); aoFechar?.(); };
  const tecla = (ev) => { if (ev.key === 'Escape' && !fixa) fechar(); };
  document.addEventListener('keydown', tecla);
  if (!fixa) {
    fundo.addEventListener('click', (ev) => { if (ev.target === fundo) fechar(); });
    $('.janela-x', fundo).onclick = fechar;
  }
  const el = $('.janela', fundo);
  setTimeout(() => el.querySelector('input,textarea,select,button:not(.janela-x)')?.focus(), 30);
  return { el, fechar };
}

export function confirmar(pergunta, { sim = 'Sim', nao = 'Cancelar' } = {}) {
  return new Promise((ok) => {
    const j = janela(`<p class="pergunta">${esc(pergunta)}</p><div class="linha-botoes"><button class="botao primario" data-r="1">${esc(sim)}</button><button class="botao" data-r="0">${esc(nao)}</button></div>`, { aoFechar: () => ok(false) });
    j.el.addEventListener('click', (ev) => {
      const b = ev.target.closest('[data-r]');
      if (!b) return;
      const v = b.dataset.r === '1';
      j.fechar();
      ok(v);
    });
  });
}

export const dataCurta = (t) => new Date(t).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });

/** Guarda preferências e partidas locais com tolerância a navegador sem armazenamento. */
export const local = {
  ler(chave, padrao) { try { const v = localStorage.getItem('arc:' + chave); return v === null ? padrao : JSON.parse(v); } catch { return padrao; } },
  gravar(chave, valor) { try { localStorage.setItem('arc:' + chave, JSON.stringify(valor)); } catch { /* sem armazenamento */ } },
  apagar(chave) { try { localStorage.removeItem('arc:' + chave); } catch { /* sem armazenamento */ } },
};

// ---------- baralhos ----------
/** Itens que um Conjurador pode levar no side deck (sem Valor, ou de um dos Valores dele). */
export function itensDoConjurador(conjId) {
  const c = conj(conjId);
  return Object.values(DADOS.catalogo.cartas).filter((x) => x.tipo === 'item' && (!x.valores?.length || x.valores.some((v) => c.valores.includes(v)))
    && (!x.competencia || x.competencia === c.comprometimento) && (!x.exclusivoDe?.length || x.exclusivoDe.includes(conjId)));
}
/** A carta pode entrar no baralho deste Conjurador (algum Valor dele; toda carta tem Valor, menos itens). */
export const cartaServe = (c, conjId) => c.tipo !== 'item' && c.tipo !== 'oculta' && (!c.valores.length || c.valores.some((v) => conj(conjId).valores.includes(v)));
export const listaParaCartas = (lista) => lista.flatMap(([id, n]) => Array.from({ length: n }, () => id));
export const prontos = () => DADOS.baralhos;
export const prontoDe = (conjId) => DADOS.baralhos.find((b) => b.conjurador === conjId);

// ---------- edições do autor (tela Editor) ----------
/** Os dados como saíram do construir.ts, antes das edições do autor. */
export const BASE = { catalogo: structuredClone(DADOS.catalogo), visual: structuredClone(DADOS.visual), conjuradoresArte: { ...DADOS.conjuradoresArte } };
delete BASE.catalogo.cartas.oculta;
/** Versão das edições em uso nesta página. */
export const EDICOES = { id: 0, dados: edicoesVazias() };
const CAMPOS_TEXTO_AUTOMATICO = ['efeitos', 'aoEntrar', 'aoDanoConjurador', 'ativacao', 'palavrasChave', 'protegido', 'descartaGrau', 'custoValores', 'pagarParaAnular', 'grauExtra', 'defesa', 'vidaDesgaste', 'ataque', 'vida'];
/** Aplica uma versão das edições por cima dos dados gerados, mudando DADOS no lugar (as telas leem DADOS na hora). */
export function usarEdicoes(v) {
  const dados = { ...edicoesVazias(), ...(v?.dados ?? {}) };
  dados.raridades = { cartas: {}, conjuradores: {}, ...(dados.raridades ?? {}) };
  const cat = aplicarCatalogo(BASE.catalogo, dados);
  for (const [id, p] of Object.entries(dados.cartas)) {
    // mudou a mecânica e o autor não escreveu o texto: texto gerado a partir da mecânica
    if (cat.cartas[id] && !('texto' in p) && CAMPOS_TEXTO_AUTOMATICO.some((k) => k in p)) cat.cartas[id] = { ...cat.cartas[id], texto: textoCarta(cat.cartas[id]) };
  }
  Object.assign(DADOS.catalogo.cartas, cat.cartas);
  Object.assign(DADOS.catalogo.conjuradores, cat.conjuradores);
  CONJS.splice(0, CONJS.length, ...Object.values(DADOS.catalogo.conjuradores));
  for (const [id, b] of Object.entries(BASE.visual)) DADOS.visual[id] = dados.artes.cartas[id] ? { ...b, arte: dados.artes.cartas[id] } : b;
  for (const [id, a] of Object.entries(BASE.conjuradoresArte)) DADOS.conjuradoresArte[id] = dados.artes.conjuradores[id] ?? a;
  EDICOES.id = v?.id ?? 0;
  EDICOES.dados = dados;
  /** Rascunho do editor aplicado nesta página (ainda não salvo no site). */
  EDICOES.rascunho = !!v?.rascunho;
}
/** Carrega uma versão das edições do servidor (sem id: a em vigor). */
export async function carregarEdicoes(id) {
  try { usarEdicoes(await api('/edicoes' + (id === undefined ? '' : '?id=' + id))); return true; } catch { return false; }
}
await carregarEdicoes();
