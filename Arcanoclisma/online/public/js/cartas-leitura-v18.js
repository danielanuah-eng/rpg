/* Layout visual medido no DOM. Não lê catálogo privado, não altera texto nem regras. */
(() => {
  const pendentes = new Set();
  const assinaturas = new WeakMap();
  const observadas = new WeakSet();
  let quadro = 0;
  const redimensionar = new ResizeObserver(entradas => {
    for (const entrada of entradas) agendar(entrada.target);
  });
  const percent = (valor, altura) => `${(valor / altura * 100).toFixed(3)}%`;
  const estilo = (el, chave, valor) => { if (el.style.getPropertyValue(chave) !== valor) el.style.setProperty(chave, valor); };

  function agendar(carta) {
    if (!carta?.matches?.('.tcg-card:not(.is-hidden)')) return;
    pendentes.add(carta);
    if (!quadro) quadro = requestAnimationFrame(atualizar);
  }
  function localizar(no, remover = false) {
    if (no.nodeType !== 1) { if (!remover) agendar(no.parentElement?.closest('.tcg-card')); return; }
    const cartas = no.matches('.tcg-card') ? [no] : no.querySelectorAll('.tcg-card');
    for (const carta of cartas) {
      if (remover) { redimensionar.unobserve(carta); observadas.delete(carta); pendentes.delete(carta); }
      else agendar(carta);
    }
    if (!remover) agendar(no.closest('.tcg-card'));
  }
  function atualizar() {
    quadro = 0;
    for (const carta of pendentes) {
      if (!carta.isConnected || carta.classList.contains('is-hidden')) continue;
      const regras = carta.querySelector('.rules'), tipo = carta.querySelector('.type-line'), subtipo = carta.querySelector('.subtype-line');
      const frente = carta.querySelector('.front');
      if (!regras || !tipo || !frente) continue;
      carta.classList.add('template-v18');
      if (!observadas.has(carta)) { observadas.add(carta); redimensionar.observe(carta); }
      const largura = carta.clientWidth, altura = carta.clientHeight;
      if (largura < 1 || altura < 1) continue;
      const nome = carta.querySelector('.name');
      const chave = `${largura}|${altura}|${nome?.textContent}|${tipo.textContent}|${subtipo?.textContent}|${regras.innerHTML}`;
      if (assinaturas.get(carta) === chave) continue;
      if (!frente.querySelector('.painel-regras-v18')) {
        const painel = document.createElement('div'); painel.className = 'painel-regras-v18';
        painel.setAttribute('aria-hidden','true'); frente.append(painel);
      }
      // Medir altura natural antes de reservar a superfície; reduzir só após ampliar o painel.
      estilo(regras,'height','auto'); estilo(regras,'bottom','auto');
      let fonte = largura * .043;
      const margem = altura * .04, intervalo = largura * .013;
      const tipoAltura = tipo.scrollHeight;
      const subtipoAltura = subtipo?.textContent.trim() ? subtipo.scrollHeight : 0;
      const cabecalho = altura * .013 + tipoAltura + intervalo + (subtipoAltura ? subtipoAltura + intervalo : 0);
      const limite = altura * .38;
      let bloco;
      for (let tentativa=0; tentativa<9; tentativa++) {
        estilo(regras,'font-size',`${fonte.toFixed(3)}px`);
        bloco = regras.scrollHeight + cabecalho + margem;
        if (bloco <= limite || fonte <= largura * .034) break;
        fonte *= .97;
      }
      const tamanho = Math.max(altura * .21,bloco);
      const topo = Math.max(altura * .4,altura * .972 - tamanho);
      const tipoTopo = topo + altura * .013;
      const subtipoTopo = tipoTopo + tipoAltura + intervalo;
      const regrasTopo = subtipoTopo + (subtipoAltura ? subtipoAltura + intervalo : 0);
      estilo(carta,'--leitura-topo',percent(topo,altura));
      estilo(carta,'--tipo-topo',percent(tipoTopo,altura));
      estilo(carta,'--subtipo-topo',percent(subtipoTopo,altura));
      estilo(carta,'--regras-topo',percent(regrasTopo,altura));
      // Nome completo ocupa a mesma faixa; a redução acompanha o espaço disponível.
      if (nome) {
        nome.classList.remove('nome-duplo-v18');
        let nomeFonte = largura * .055;
        for (let i=0;i<15;i++) {
          estilo(nome,'font-size',`${nomeFonte.toFixed(3)}px`);
          if (nome.scrollWidth <= nome.clientWidth + 1 || nomeFonte <= largura * .037) break;
          nomeFonte *= .965;
        }
        if (nome.scrollWidth > nome.clientWidth + 1) {
          nome.classList.add('nome-duplo-v18'); estilo(nome,'font-size',`${(largura*.035).toFixed(3)}px`);
        }
      }
      assinaturas.set(carta,chave);
    }
    pendentes.clear();
  }
  new MutationObserver(mudancas => {
    for (const m of mudancas) {
      if (m.type==='characterData') localizar(m.target);
      else { m.removedNodes.forEach(no=>localizar(no,true)); m.addedNodes.forEach(no=>localizar(no)); agendar(m.target.closest?.('.tcg-card')); }
    }
  }).observe(document.body,{subtree:true,childList:true,characterData:true});
  localizar(document.body);
  function invalidarFontes() {
    for (const carta of document.querySelectorAll('.tcg-card')) { assinaturas.delete(carta); agendar(carta); }
  }
  document.fonts?.ready.then(invalidarFontes);
  document.fonts?.addEventListener('loadingdone',invalidarFontes);
  for (const folha of document.querySelectorAll('link[rel="stylesheet"]')) folha.addEventListener('load',invalidarFontes,{once:true});
})();
