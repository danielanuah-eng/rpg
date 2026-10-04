// Contrato: efeitos/pedidos/mesa-efeitos-v1.claude.json.
// Anima somente apresentação. Não paga custos, envia comandos, aplica dano ou decide alvos.
import { RITMO_VISUAL, duracaoVisual } from './ritmo-visual-v13.js';
const centro = (r) => ({ x: r.left + r.width / 2, y: r.top + r.height / 2 });
const rectDe = (el) => el?.isConnected ? el.getBoundingClientRect() : null;
const valido = (r) => r && r.width > 0 && r.height > 0;
const anguloDe = (el, meu) => {
  const css = el ? parseFloat(getComputedStyle(el).getPropertyValue('--mesa-turn')) : NaN;
  return Number.isFinite(css) ? css : meu ? 0 : 180;
};
const transformacao = (x, y, giro, inclinacao = 8, escala = 1) =>
  `translate(calc(-50% + ${x}px), calc(-50% + ${y}px)) rotateX(${inclinacao}deg) rotateZ(${giro}deg) scale(${escala})`;

export function criarEfeitosMesa(raiz) {
  const reduzido = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
  const camada = document.createElement('div'); camada.className = 'mesa-efeitos-codex';
  camada.setAttribute('aria-hidden', 'true'); document.body.append(camada);
  if (!document.getElementById('mesa-efeitos-codex-css')) {
    const css = document.createElement('link'); css.id = 'mesa-efeitos-codex-css'; css.rel = 'stylesheet';
    css.href = '/css/mesa-efeitos-codex.css'; document.head.append(css);
  }
  const ativos = new Set(), snapshots = new Map(), ocultos = new Map();
  let fechado = false, manifestoPromise;

  // Cópias exclusivamente do que já estava visível. Conserva a carta mesmo se o combate a remover.
  function copiarCarta(el, meu) {
    const carta = el?.matches('.tcg-card') ? el : el?.querySelector('.tcg-card');
    if (!carta || carta.classList.contains('is-hidden')) return null;
    const rect = rectDe(carta); if (!valido(rect)) return null;
    const giro = anguloDe(el, meu), deitada = Math.abs(Math.round(giro / 90)) % 2 === 1;
    const copia = carta.cloneNode(true);
    copia.classList.remove('facing-opponent', 'card-motion', 'attack-generic', 'block-generic');
    copia.removeAttribute('id'); copia.querySelectorAll('[id]').forEach((n) => n.removeAttribute('id'));
    return { copia, rect, giro, largura: deitada ? rect.height : rect.width, altura: deitada ? rect.width : rect.height };
  }
  function guardarSnapshots() {
    if (fechado) return;
    for (const el of raiz.querySelectorAll('.cm[data-id]')) {
      const snap = copiarCarta(el, !el.closest('.lado-op'));
      if (snap) { snap.capturadoEm = performance.now(); snapshots.set(el.dataset.id, snap); }
    }
    while (snapshots.size > 200) snapshots.delete(snapshots.keys().next().value);
  }
  guardarSnapshots();
  const observador = new MutationObserver(guardarSnapshots);
  observador.observe(raiz, { childList: true, subtree: true });

  function snapshotPerto(rect) {
    if (!valido(rect)) return null;
    const p = centro(rect); let resultado = null, distancia = Infinity;
    for (const snap of snapshots.values()) {
      const s = centro(snap.rect), d = Math.hypot(p.x - s.x, p.y - s.y);
      if (d < distancia || (d === distancia && (snap.capturadoEm ?? 0) > (resultado?.capturadoEm ?? 0))) { resultado = snap; distancia = d; }
    }
    return distancia < 8 ? resultado : null;
  }
  function planoDe(snap, posicao) {
    const plano = document.createElement('div'); plano.className = 'efeito-plano-carta';
    plano.style.left = `${posicao.x}px`; plano.style.top = `${posicao.y}px`;
    plano.style.width = `${snap.largura}px`; plano.style.height = `${snap.altura}px`;
    plano.style.setProperty('--efeito-largura', `${snap.largura}px`);
    const copia = snap.copia.cloneNode(true);
    copia.style.setProperty('--card-w', `${snap.largura}px`);
    plano.append(copia); camada.append(plano); return plano;
  }
  function animar(node, quadros, duracao, elOculto) {
    if (elOculto?.isConnected) {
      const registro = ocultos.get(elOculto) ?? { original: elOculto.style.visibility, total: 0 };
      registro.total++; ocultos.set(elOculto, registro); elOculto.style.visibility = 'hidden';
    }
    let encerrado = false, movimento;
    const terminar = () => {
      if (encerrado) return; encerrado = true;
      const registro = ocultos.get(elOculto);
      if (registro && --registro.total === 0) {
        if (elOculto.isConnected) elOculto.style.visibility = registro.original;
        ocultos.delete(elOculto);
      }
      node.remove(); ativos.delete(cancelar);
    };
    const cancelar = () => { movimento?.cancel(); terminar(); };
    try {
      movimento = node.animate(quadros, { duration: duracao, easing: 'cubic-bezier(.25,.1,.25,1)', fill: 'both' });
      ativos.add(cancelar);
      return movimento.finished.then(() => { terminar(); return { realizado: true }; }, () => { terminar(); return { realizado: false, motivo: 'cancelado' }; });
    } catch {
      terminar(); return Promise.resolve({ realizado: false, motivo: 'animacao indisponivel' });
    }
  }
  const ignorar = (motivo) => Promise.resolve({ realizado: false, motivo });

  function entrarEmCampo({ el, meu }) {
    if (fechado || reduzido()) return ignorar('movimento reduzido ou mesa fechada');
    const snap = copiarCarta(el, meu); if (!snap) return ignorar('carta visivel indisponivel');
    const destino = centro(snap.rect), mesa = raiz.getBoundingClientRect();
    const mao = rectDe(raiz.querySelector(meu ? '.mao-eu' : '.mao-op'));
    const origem = valido(mao) ? centro(mao) : { x: mesa.left + mesa.width / 2, y: meu ? Math.min(innerHeight - 20, mesa.bottom) : Math.max(20, mesa.top) };
    const dx = origem.x - destino.x, dy = origem.y - destino.y;
    const plano = planoDe(snap, destino);
    return animar(plano, [
      { transform: transformacao(dx, dy, snap.giro, 32, 1.16), opacity: 0, offset: 0 },
      { transform: transformacao(dx * .83, dy * .83, snap.giro + (meu ? -7 : 7), 26, 1.14), opacity: 1, offset: .15 },
      { transform: transformacao(0, -8, snap.giro, 10, 1.04), opacity: 1, offset: .79 },
      { transform: transformacao(0, 0, snap.giro, 5, 1), opacity: 1, offset: 1 },
    ], RITMO_VISUAL.entrarEmCampo, el);
  }

  function colidir({ atacanteEl, alvoEl, atacanteRect, alvoRect, bloqueio, meu }) {
    if (fechado || reduzido()) return ignorar('movimento reduzido ou mesa fechada');
    const snap = copiarCarta(atacanteEl, meu) ?? snapshotPerto(atacanteRect);
    const alvo = rectDe(alvoEl) ?? alvoRect;
    if (!snap || !valido(alvo)) return ignorar('geometria ou imagem visivel indisponivel');
    const origem = centro(atacanteRect ?? snap.rect), destino = centro(alvo);
    const dx = destino.x - origem.x, dy = destino.y - origem.y;
    const giroFinal = (meu ? 0 : 180) + 90;
    const plano = planoDe(snap, origem);
    return animar(plano, [
      { transform: transformacao(0, 0, snap.giro, 5), opacity: 1, offset: 0 },
      { transform: transformacao(-dx * .055, -dy * .055, giroFinal, 30, 1.10), opacity: 1, offset: .23 },
      { transform: transformacao(dx * .76, dy * .76, giroFinal, 18, 1.03), opacity: 1, offset: .49 },
      { transform: transformacao(dx * .70, dy * .70, giroFinal + (bloqueio ? -4 : 4), 12), opacity: 1, offset: .57 },
      { transform: transformacao(0, -3, giroFinal, 8), opacity: 1, offset: .91 },
      { transform: transformacao(0, 0, giroFinal, 5), opacity: atacanteEl?.isConnected ? 1 : 0, offset: 1 },
    ], RITMO_VISUAL.ataqueBloqueio, atacanteEl);
  }

  // Cada magia exige arquivo específico aprovado. Não substituir pendências por efeitos genéricos.
  async function resolver({ item, defId, origemEl, alvoEl, alvoRect, meu, resultado = item?.resultado }) {
    if (fechado || reduzido() || item?.anulado || item?.resultado === 'anulado') return ignorar('sem resolucao visual');
    if (!/^[a-z0-9-]+$/.test(defId ?? '')) return ignorar('carta sem ID visual');
    manifestoPromise ??= fetch('/kit/efeitos/manifesto.json').then((r) => r.ok ? r.json() : {}).catch(() => ({}));
    const manifesto = await manifestoPromise;
    const visual = Object.hasOwn(manifesto, defId) ? manifesto[defId] : null;
    if (fechado || visual?.okGpt !== true || !visual.arquivos?.length) return ignorar('asset particular aguardando entrega');
    if (resultado !== 'resolvido') return ignorar('resultado real da pilha nao informado');
    const arquivo = visual.arquivos[0];
    const prefixo = `/kit/efeitos/${defId}/`;
    const caminho = typeof arquivo === 'string' && arquivo.startsWith(prefixo) ? arquivo : prefixo + arquivo;
    if (typeof arquivo !== 'string' || caminho.includes('..') || !caminho.startsWith(prefixo) || !/^\/kit\/efeitos\/[a-z0-9_./-]+\.(png|webp|svg)$/i.test(caminho)) return ignorar('caminho visual invalido');
    const caster = raiz.querySelector(`${meu ? '.lado-eu' : '.lado-op'} .conj-bloco .cm`);
    const origemRect = rectDe(origemEl) ?? rectDe(caster);
    const destinoRect = rectDe(alvoEl) ?? alvoRect ?? raiz.getBoundingClientRect();
    if (!valido(origemRect) || !valido(destinoRect)) return ignorar('geometria indisponivel');
    const origem = centro(origemRect), destino = centro(destinoRect);
    const efeito = document.createElement('img'); efeito.className = 'efeito-imagem-carta';
    efeito.alt = ''; efeito.src = caminho; efeito.draggable = false;
    const tamanho = Math.min(220, Math.max(48, Number(visual.tamanhoPx) || 96));
    efeito.style.setProperty('--efeito-tamanho', `${tamanho}px`);
    const area = visual.modo === 'area';
    efeito.style.left = `${area ? destino.x : origem.x}px`; efeito.style.top = `${area ? destino.y : origem.y}px`;
    try { await efeito.decode(); } catch { return ignorar('asset inacessivel'); }
    if (fechado) return ignorar('mesa fechada');
    camada.append(efeito);
    const dx = destino.x - origem.x, dy = destino.y - origem.y;
    const giro = area ? 0 : Math.atan2(dy, dx) * 180 / Math.PI;
    const trans = (x, y, escala) => `translate(calc(-50% + ${x}px), calc(-50% + ${y}px)) rotate(${giro}deg) scale(${escala})`;
    return animar(efeito, [
      { transform: trans(0, 0, .65), opacity: 0, offset: 0 },
      { transform: trans(0, 0, 1), opacity: 1, offset: .13 },
      { transform: trans(area ? 0 : dx, area ? 0 : dy, 1.16), opacity: 1, offset: .78 },
      { transform: trans(area ? 0 : dx, area ? 0 : dy, 1.30), opacity: 0, offset: 1 },
    ], duracaoVisual(visual.duracaoMs));
  }

  function limpar() {
    fechado = true; observador.disconnect();
    for (const cancelar of [...ativos]) cancelar();
    snapshots.clear(); ocultos.clear(); camada.remove();
  }
  return { entrarEmCampo, resolver, colidir, limpar };
}
