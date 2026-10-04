/* Complemento visual independente do motor: legenda para cartas públicas.
   Não lê fonte.e, catálogo, mãos nem dados privados. Não muda ações da mesa. */
(() => {
  // A escolha confirmada pelo autor ao Claude é manter o tabuleiro inteiro.
  // Não carregar a câmera fixa/rolagem v15; só rótulos, zonas e cabeçalho.
  for (const href of ['/css/mesa-zonas-v16.css', '/css/cartas-cabecalho-v16.css']) {
    if (document.querySelector(`link[href="${href}"]`)) continue;
    const css = document.createElement('link');
    css.rel = 'stylesheet'; css.href = href; document.head.append(css);
  }
  const tela = document.getElementById('tela');
  if (!tela) return;
  let agendado = false;
  let mundoAtual = null;
  const escalaObserver = new MutationObserver(agendar);

  function atualizar() {
    agendado = false;
    for (const frente of tela.querySelectorAll('.tcg-card:not(.is-hidden) .front')) {
      if (frente.querySelector('.cabecalho-arte-v16')) continue;
      const faixa = document.createElement('div');
      faixa.className = 'cabecalho-arte-v16'; faixa.setAttribute('aria-hidden', 'true');
      frente.append(faixa);
    }
    const mesa = tela.querySelector('.mesa');
    if (!mesa) { mundoAtual = null; escalaObserver.disconnect(); return; }
    const mundo = mesa.querySelector('.mesa-mundo');
    if (mundo && mundo !== mundoAtual) {
      escalaObserver.disconnect(); mundoAtual = mundo;
      escalaObserver.observe(mundo, { attributes: true, attributeFilter: ['style'] });
    }
    const escala = Math.max(.25, Math.min(1.5, Number.parseFloat(mundo?.style.zoom) || 1));
    mesa.style.setProperty('--mesa-escala-visual', escala.toFixed(4));
    // Economia impressa no template também acompanha a Vida sobre a carta em campo.
    for (const conj of mesa.querySelectorAll('.conj-bloco .cm')) {
      const valor = conj.querySelector('.stat-compraFinal > span')?.textContent;
      const marcas = conj.querySelector('.marca-status');
      if (valor == null || !marcas) continue;
      let economia = marcas.querySelector('.st-economia');
      if (!economia) {
        economia = document.createElement('b'); economia.className = 'st-economia';
        marcas.append(economia);
      }
      economia.title = `Economia: compra ${valor} cartas no fim do turno`;
      const texto = `▱ ${valor}`;
      if (economia.textContent !== texto) economia.textContent = texto;
    }
    for (const carta of mesa.querySelectorAll('.campo-eu > .cm, .campo-op > .cm')) {
      const frente = carta.querySelector('.tcg-card:not(.is-hidden) .name');
      const nome = frente?.textContent?.trim();
      if (!nome) { carta.querySelector('.mesa-card-nome')?.remove(); continue; }
      let legenda = carta.querySelector('.mesa-card-nome');
      if (!legenda) {
        legenda = document.createElement('span');
        legenda.className = 'mesa-card-nome';
        legenda.setAttribute('aria-hidden', 'true');
        carta.append(legenda);
      }
      if (legenda.textContent !== nome) legenda.textContent = nome;
    }
  }
  function agendar() {
    if (agendado) return;
    agendado = true;
    requestAnimationFrame(atualizar);
  }
  const observador = new MutationObserver(agendar);
  observador.observe(tela, { childList: true, subtree: true });
  agendar();
})();
