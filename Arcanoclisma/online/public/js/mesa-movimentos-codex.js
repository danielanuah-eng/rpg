// Animação somente visual: lê elementos já autorizados e não envia comandos do jogo.
// Cartas em trânsito sempre usam o verso genérico, inclusive as do oponente.
import { RITMO_VISUAL } from './ritmo-visual-v13.js';
export function criarMovimentosMesa(raiz) {
  const ativos = new Set();
  const reduzido = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
  const rect = el => el?.getBoundingClientRect();
  const centro = r => ({ x: r.left + r.width / 2, y: r.top + r.height / 2 });
  const estadoEl = el => ({ el, rect: rect(el), transform: getComputedStyle(el).transform });
  const qtdBaralho = el => Number(el?.textContent.match(/\d+/)?.[0] ?? 0);

  function limpar() {
    for (const encerrar of [...ativos]) encerrar();
  }
  function capturar() {
    limpar();
    const minhas = new Map();
    raiz.querySelectorAll('.mao-slot').forEach(slot => {
      const carta = slot.querySelector('.cm[data-id]');
      if (carta) minhas.set(carta.dataset.id, estadoEl(slot));
    });
    const campo = new Map();
    raiz.querySelectorAll('.mesa-conteudo .cm[data-id]').forEach(el => campo.set(el.dataset.id, estadoEl(el)));
    const versos = [...raiz.querySelectorAll('.mao-op .verso-mini')].map(estadoEl);
    const baralhoEu = raiz.querySelector('.lado-eu [data-zona="baralho"]');
    const baralhoOp = raiz.querySelector('.lado-op [data-zona="baralho"]');
    return {
      minhas, campo, versos,
      baralhoEu: rect(baralhoEu), baralhoOp: rect(baralhoOp),
      qtdEu: qtdBaralho(baralhoEu), qtdOp: qtdBaralho(baralhoOp),
      inicializado: Boolean(baralhoEu && baralhoOp),
    };
  }

  function acompanhar(animation, finalizar = () => {}) {
    let terminou = false;
    const encerrar = () => {
      if (terminou) return;
      terminou = true;
      animation.cancel();
      finalizar();
      ativos.delete(encerrar);
    };
    ativos.add(encerrar);
    animation.finished.then(encerrar, encerrar);
  }
  function reposicionar(anterior, atual, duracao = RITMO_VISUAL.reposicionarMao) {
    const a = centro(anterior.rect), b = centro(atual.rect);
    const dx = a.x - b.x, dy = a.y - b.y;
    if (Math.abs(dx) < .5 && Math.abs(dy) < .5 && anterior.transform === atual.transform) return;
    const antiga = anterior.transform === 'none' ? '' : anterior.transform;
    acompanhar(atual.el.animate([
      { transform: `translate(${dx}px, ${dy}px) ${antiga}`.trim() },
      { transform: atual.transform },
    ], { duration: duracao, easing: 'cubic-bezier(.2,.75,.25,1)' }));
  }
  function comprar(origem, destino, atraso = 0) {
    if (!origem || !destino.el.isConnected || destino.rect.width === 0) return;
    const a = centro(origem), b = centro(destino.rect);
    const viajante = document.createElement('div');
    viajante.className = 'mesa-carta-viajante';
    viajante.setAttribute('aria-hidden', 'true');
    Object.assign(viajante.style, {
      left: `${destino.rect.left}px`, top: `${destino.rect.top}px`,
      width: `${destino.rect.width}px`, height: `${destino.rect.height}px`,
    });
    document.body.append(viajante);
    const visibilidade = destino.el.style.visibility;
    destino.el.style.visibility = 'hidden';
    const animation = viajante.animate([
      { transform: `translate(${a.x - b.x}px, ${a.y - b.y}px) rotate(-9deg) scale(${origem.width / destino.rect.width})`, opacity: 1 },
      { transform: 'translate(0,0) rotate(0deg) scale(1)', opacity: 1 },
    ], { duration: RITMO_VISUAL.comprarParaMao, delay: atraso, fill: 'both', easing: 'cubic-bezier(.2,.7,.2,1)' });
    acompanhar(animation, () => {
      viajante.remove();
      destino.el.style.visibility = visibilidade;
    });
  }

  function animar(anterior) {
    if (reduzido() || typeof Element.prototype.animate !== 'function') return;
    // Não chamar capturar() aqui: sua limpeza interromperia animações que acabamos de iniciar.
    const novasMinhas = new Map();
    raiz.querySelectorAll('.mao-slot').forEach(slot => {
      const carta = slot.querySelector('.cm[data-id]');
      if (carta) novasMinhas.set(carta.dataset.id, estadoEl(slot));
    });
    const novosVersos = [...raiz.querySelectorAll('.mao-op .verso-mini')].map(estadoEl);
    raiz.querySelectorAll('.mesa-conteudo .cm[data-id]').forEach(el => {
      const antiga = anterior.campo.get(el.dataset.id);
      if (antiga) reposicionar(antiga, estadoEl(el), RITMO_VISUAL.suspender);
    });
    const deckEu = raiz.querySelector('.lado-eu [data-zona="baralho"]');
    const deckOp = raiz.querySelector('.lado-op [data-zona="baralho"]');
    let comprasEu = anterior.inicializado ? Math.max(0, anterior.qtdEu - qtdBaralho(deckEu)) : 0;
    let comprasOp = anterior.inicializado ? Math.max(0, anterior.qtdOp - qtdBaralho(deckOp)) : 0;
    let atrasoEu = 0, atrasoOp = 0;
    for (const [id, atual] of novasMinhas) {
      const antiga = anterior.minhas.get(id);
      if (antiga) reposicionar(antiga, atual);
      else if (comprasEu > 0) {
        comprar(anterior.baralhoEu ?? rect(deckEu), atual, atrasoEu);
        comprasEu--; atrasoEu += 170;
      } else {
        const origemCampo = anterior.campo.get(id);
        if (origemCampo) reposicionar(origemCampo, atual);
      }
    }
    const inicioComprasOp = novosVersos.length - Math.min(comprasOp, novosVersos.length);
    novosVersos.forEach((atual, k) => {
      const antiga = anterior.versos[k];
      if (comprasOp > 0 && k >= inicioComprasOp) {
        comprar(anterior.baralhoOp ?? rect(deckOp), atual, atrasoOp);
        comprasOp--; atrasoOp += 170;
      } else if (antiga) reposicionar(antiga, atual);
    });
  }
  return { capturar, animar, limpar };
}
