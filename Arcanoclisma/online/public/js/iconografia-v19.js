// Só apresentação: preserva os ícones/ID/textos produzidos pelo integrador do Claude.
(() => {
  const trechos = {
    'ao-entrar': /^\s*ao entrar\b/i,
    'requisito-grau': /^\s*\d+\+ no Grau\b/i,
    'requisito-arcano': /^\s*pague \d+/i,
    'pagar-arcano': /^\s*pague \d+/i,
    comprar: /^\s*compre \d+/i,
    protegido: /^\s*protegido\b(?:\s+\d+)?/i,
  };
  function decorar(raiz) {
    if (raiz.nodeType !== 1) return;
    const imagens = raiz.matches('img.icone-mecanica') ? [raiz] : raiz.querySelectorAll('img.icone-mecanica');
    for (const imagem of imagens) {
      const caminho = imagem.getAttribute('src');
      if (!/^\/kit\/icones\/[a-z0-9-]+\.svg$/.test(caminho ?? '')) continue;
      const simbolo = document.createElement('span');
      simbolo.className = 'icone-mecanica icone-mask-v19';
      simbolo.setAttribute('aria-hidden','true'); simbolo.title = imagem.title;
      simbolo.style.setProperty('--icone-v19',`url("${caminho}")`);
      imagem.before(simbolo); imagem.classList.remove('icone-mecanica'); simbolo.append(imagem);
      // Manter o desenho com a palavra/quantidade seguinte, sem alterar o texto.
      const texto=simbolo.nextSibling,id=caminho.split('/').pop().replace('.svg','');
      if (texto?.nodeType===3) {
        const trecho=(trechos[id]??/^\s*\S+/u).exec(texto.nodeValue)?.[0];
        if (trecho) {
          const grupo=document.createElement('span');grupo.className='mecanica-termo-v19';
          simbolo.before(grupo);grupo.append(simbolo,document.createTextNode(trecho));
          texto.nodeValue=texto.nodeValue.slice(trecho.length);
        }
      }
    }
  }
  decorar(document.body);
  new MutationObserver(mudancas => {
    for (const mudanca of mudancas) mudanca.addedNodes.forEach(decorar);
  }).observe(document.body,{childList:true,subtree:true});
})();
