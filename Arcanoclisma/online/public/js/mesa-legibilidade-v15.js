/* Complemento visual independente do motor: legenda para cartas públicas.
   Não lê fonte.e, catálogo, mãos nem dados privados. Não muda ações da mesa. */
(() => {
  const tela = document.getElementById('tela');
  if (!tela) return;
  let agendado = false;
  let ultimoPalco = null;

  function atualizar() {
    agendado = false;
    const mesa = tela.querySelector('.mesa');
    if (!mesa) { ultimoPalco = null; return; }
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
    const palco = mesa.querySelector('.mesa-palco');
    if (palco && palco !== ultimoPalco && mesa.querySelector('.lado-eu')) {
      ultimoPalco = palco;
      // Primeira abertura: os dois Conjuradores juntos ficam na vista.
      // Após isso a rolagem pertence ao jogador e é preservada nos redraws.
      palco.scrollLeft = Math.max(0, (palco.scrollWidth - palco.clientWidth) / 2);
      const linha = mesa.querySelector('.linha-meio');
      const centro = linha ? linha.getBoundingClientRect().top - palco.getBoundingClientRect().top + palco.scrollTop : palco.scrollHeight / 2;
      palco.scrollTop = Math.max(0, centro - palco.clientHeight / 2);
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
