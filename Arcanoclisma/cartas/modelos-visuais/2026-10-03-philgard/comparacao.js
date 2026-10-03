/* Prévia de comparação: mantém as quatro ilustrações intactas. */
(() => {
 const header=document.querySelector('header .intro');
 header.textContent='V2 aprovada nas duas cartas: cenários mais lisos e menos rugosos. As V1 permanecem disponíveis para comparação.';
 const style=document.createElement('style');
 style.textContent='main{max-width:1200px}.grid{grid-template-columns:1fr;gap:50px}.comparison{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:24px}.variant{margin:0;min-width:0}.variant figcaption{text-align:center;margin:0 0 16px;color:#e7d1a0;font-weight:600}.tcg-card{--card-w:420px}.entry>h2{margin-top:12px}.variant-links{display:block;text-align:center;margin:10px 0 22px}@media(max-width:780px){.comparison{grid-template-columns:1fr}.tcg-card{--card-w:360px}}';
 document.head.append(style);
 for (const entry of document.querySelectorAll('.entry')) {
  const original=entry.querySelector('.tcg-card');
  const smooth=original.cloneNode(true);
  const originalSrc=original.querySelector('.portrait').getAttribute('src');
  const smoothSrc=originalSrc.replace('-v1.png','-v2-liso.png');
  smooth.querySelector('.portrait').setAttribute('src',smoothSrc);
  smooth.setAttribute('aria-label',original.getAttribute('aria-label')+' — V2, cenário mais liso');
  const pair=document.createElement('div');pair.className='comparison';
  original.before(pair);
  for (const [card,label,src] of [[smooth,'V2 · aprovada',smoothSrc],[original,'V1 · versão anterior',originalSrc]]) {
   const figure=document.createElement('figure');figure.className='variant';
   const caption=document.createElement('figcaption');caption.textContent=label;
   const link=document.createElement('a');link.className='variant-links';link.href=src;link.textContent='Abrir arte sem moldura';
   figure.append(caption,card,link);pair.append(figure);
  }
  const oldLinks=entry.querySelector('.links');if(oldLinks)oldLinks.remove();
 }
 const note=document.querySelector('footer .note');
 note.textContent='V2 aprovada nas duas artes · proporção 63 × 88 mm. Raridade e dados de jogo serão definidos pelo Claude. — indica um campo ainda sem valor definido.';
})();
