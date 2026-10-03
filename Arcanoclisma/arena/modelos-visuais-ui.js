/* Composição visual: exemplos e suspensão local. O motor do Claude define as ações reais. */
(() => {
  const visual=window.ArcanoclismaCardVisual;
  const button=document.createElement('button');button.id='suspend-conjurador';button.type='button';
  const attack=document.createElement('button');attack.id='attack-conjurador';attack.type='button';attack.textContent='Atacar com conjurador';
  const block=document.createElement('button');block.id='block-conjurador';block.type='button';block.textContent='Bloquear com conjurador';
  document.querySelector('#ready-cards').before(attack,block,button);
  const ownConjurador=()=>document.querySelector('.board-position.self.conjurador');
  const sync=()=>{
    const target=ownConjurador(),suspended=target?.classList.contains('is-suspended')||false;
    const moving=target?.matches('.attack-generic,.block-generic')||false;
    button.disabled=!target||moving;attack.disabled=block.disabled=!target||moving||suspended;
    button.textContent=suspended?'Preparar conjurador':'Suspender conjurador';button.setAttribute('aria-pressed',String(suspended));
  };
  const play=action=>{const target=ownConjurador();if(!target||attack.disabled)return;visual.animateMotion(target,{action}).finally(sync);sync();};
  attack.addEventListener('click',()=>play('attack'));block.addEventListener('click',()=>play('block'));
  button.addEventListener('click',()=>{const target=ownConjurador();if(target)visual.setSuspended(target,!target.classList.contains('is-suspended'));sync();});
  new MutationObserver(sync).observe(document.querySelector('#board-cards'),{childList:true,subtree:true,attributes:true,attributeFilter:['class']});sync();

  const section=document.createElement('section');section.id='modelos-cenarios';section.className='scene-examples';
  const title=document.createElement('h2');title.textContent='Propostas de cartas · Ciantia';
  const note=document.createElement('p');note.className='quiet';note.textContent='Artes aprovadas e habilidades propostas pelo autor. Claude define os números e integra as regras. Os traços nos campos não representam custo zero.';
  const grid=document.createElement('div');grid.className='scene-example-grid';section.append(title,note,grid);
  for(const model of window.ARENA_CENAS||[]){
    const entry=document.createElement('article');entry.className='scene-example';
    const image=document.createElement('div');image.className='scene-example-card';image.append(visual.create(model,{width:300}));
    const details=document.createElement('div');details.className='scene-example-details';
    const heading=document.createElement('h3');heading.textContent=model.nome;
    const profile=document.createElement('p');profile.className='scene-example-profile';profile.textContent=(model.cosmetico.raridade==='lendaria'?'Lendária':'Rara')+(model.cosmetico.arteAlternativa?' · Arte alternativa':' · Cena narrativa');
    const context=document.createElement('p');context.textContent=model.contexto;
    const status=document.createElement('p');status.className='quiet';status.textContent='Proposta de carta · aguardando definição e integração pelo Claude.';
    const elements=document.createElement('p');elements.textContent=model.id==='licao-do-jardim'?'Elementos derivados: dragãozinho dourado, jardim suspenso, caderno e canais de bronze.':'Elementos derivados: dragão de energia, ruptura no céu, ponte interrompida e viajantes.';
    const link=document.createElement('a');link.href=model.arte;link.textContent='Abrir ilustração original';
    details.append(heading,profile,context,status,elements,link);entry.append(image,details);grid.append(entry);
  }
  document.querySelector('footer').before(section);
})();
