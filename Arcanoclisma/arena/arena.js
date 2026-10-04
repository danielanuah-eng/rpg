/* Estudo visual local. Este arquivo não implementa regras, rede, gacha ou permissões de jogo. */
(() => {
  const models = window.ARENA_MODELOS;
  const el = (tag, className, text) => { const n=document.createElement(tag); if(className)n.className=className; if(text!=null)n.textContent=String(text); return n; };
  const img = (src, cls, alt='') => { const n=el('img',cls);n.src=src;n.alt=alt;n.draggable=false;return n; };
  const names={aliado:'Aliado',temporaria:'Temporária',mistica:'Mística',estrutura:'Estrutura',conjurador:'Conjurador',item:'Item',canal:'Canal',armadura:'Armadura',extra:'Extra'};
  const modelKey=m=>m.categoriaItem||m.tipo;
  const colors={'Conflitüo':'#9a4633',Sublimeyn:'#748e9d',Dominum:'#62416a',Persona:'#b76673',Alvorum:'#927638',Morbidyum:'#6b866d',Cognitium:'#505b94',Eternyr:'#304565',Primitivum:'#65743b'};
  const valorSlug=v=>String(v||'').normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();
  const valorName=v=>Object.keys(colors).find(name=>valorSlug(name)===valorSlug(v))||v;
  const count = v => Math.max(0,Math.trunc(Number(v)||0));
  function card(model,{hidden=false,opponent=false,readable=false,width=150,rarity}={}) {
    const n=el('article','tcg-card'+(hidden?' is-hidden':'')+(opponent?' facing-opponent':'')+(readable?' revealed-readable':''));
    n.style.setProperty('--card-w',width+'px');
    if(hidden) {
      // Carta oculta: sem frente, identidade, raridade ou atributos privados no DOM.
      n.setAttribute('aria-label','Carta oculta');
      const back=el('div','back');back.append(img('raster/verso-arcano-v3.png','back-art'),img('vetores/verso-titulo.svg','back-title'));n.append(back);return n;
    }
    n.dataset.rarity=rarity||model.cosmetico?.raridade||'comum';
    n.dataset.type=model.tipo;
    const alternative=model.cosmetico?.arteAlternativa===true;
    n.dataset.artVariant=alternative?'alternativa':'padrao';
    n.dataset.finish=alternative?'alternativa':n.dataset.rarity;
    if(alternative)n.classList.add('scene-art','is-alt-art');
    if(model.arteModo==='cena')n.classList.add('scene-art');
    if(model.artePosicao)n.style.setProperty('--art-position',model.artePosicao);
    n.setAttribute('aria-label',`${model.nome}, ${names[model.tipo]}`);
    const front=el('div','front');
    const frameType=modelKey(model);
    if(model.arte){front.append(img(model.arte,'portrait'));if(['lendaria','alternativa'].includes(n.dataset.finish)){front.append(img(model.arte,'portrait art-magic-glow-v9'));if(alternative)front.append(el('div','art-depth-v9'));}}else front.append(el('div','art-placeholder','[Ilustração do item]'));
    front.append(img(`templates/${frameType}-${model.tipo==='conjurador'?'v8':'v4'}.svg`,'frame'),el('div','name',model.nome),img(`vetores/icone-${frameType}.svg`,'type-glyph',names[frameType]));
    if(model.tipo==='conjurador') {
      const pair=[...new Set((model.valores||[]).map(valorName))].slice(0,2);
      const sphere=el('div','conjurador-values');sphere.setAttribute('role','img');sphere.setAttribute('aria-label',`Valores do conjurador: ${pair.join(' e ')||'não informados'}`);sphere.title=sphere.getAttribute('aria-label');
      for(let i=0;i<2;i++){
        const value=pair[i],half=el('span','conjurador-value-half');half.style.setProperty('--valor-color',colors[value]||'#31485b');
        if(value&&colors[value])half.append(img(`vetores/valor-${valorSlug(value)}-simbolo-v8.svg`,'',''));
        else half.append(el('span','value-placeholder','?'));
        half.title=value||`Valor ${i+1} não informado`;sphere.append(half);
      }
      sphere.append(img('vetores/esfera-conjurador-v8.svg','conjurador-sphere-rim',''));front.append(sphere);
    }
    if(model.tipo!=='conjurador') {
      const generated=count(model.arcanoGerado),cost=count(model.custo),degree=count(model.grau);
      const provisional=model.camposProvisorios===true;
      const g=el('div','badge generated',provisional?'—':generated);g.title=provisional?'Recurso arcano a definir':`Gera ${generated} Arcano`;g.setAttribute('aria-label',g.title);front.append(g);
      g.style.backgroundColor=colors[model.valores?.[0]]||'#325e72';
      const c=el('div','badge cost',provisional?'—':cost);c.title=provisional?'Custo arcano a definir':`Custo: ${cost} Arcano${model.valores?.length?' · Valores: '+model.valores.join(', '):''}`;c.setAttribute('aria-label',c.title);
      const palette=(model.valores||[]).map(v=>colors[v]||'#849eae');
      if(palette.length){const rim=el('i','valor-rim');rim.style.background=`conic-gradient(${palette.map((v,i)=>`${v} ${i*360/palette.length}deg ${(i+1)*360/palette.length}deg`).join(',')})`;c.append(rim);}
      const values=el('div','value-symbols');for(const value of model.valores||[]){const slug=value.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();if(['conflituo','sublimeyn','dominum','persona','alvorum','morbidyum','cognitium','eternyr','primitivum'].includes(slug)){const glyph=img(`vetores/valor-${slug}.svg`,'',value);glyph.title=value;values.append(glyph);}}if(values.childElementCount)c.append(values);
      front.append(c);
      if(degree>0||(provisional&&model.mostrarGrauModelo)){const d=el('div','degree',provisional?'—':degree);d.title=provisional?'Grau a definir':`Grau necessário: ${degree}`;d.setAttribute('aria-label',d.title);front.append(d);}
    }
    const subtypes=[...(model.categoriaItem?[names[model.categoriaItem]]:[]),...(model.subtipos||[])].filter(s=>!(model.valores||[]).includes(s));
    const typeLabel=model.tipo==='item'?'Item de Conjurador':names[model.tipo]||model.tipo;
    const speed=model.velocidade==='repentina'?'Repentina':model.tipo==='temporaria'?(model.velocidade==='findante'?'Findante':'Velocidade não informada'):'';
    const classification=model.tipo!=='conjurador'&&(model.valores||[]).length>1?'Extra':'';
    const typeText=model.tipo==='conjurador'?[typeLabel,model.comprometimento||'[Comprometimento]'].join(' · '):[typeLabel,speed,classification].filter(Boolean).join(' · ');
    const detailText=[...new Set(subtypes)].join(' · ');
    const detail=model.tipo==='conjurador'?el('div','subtype-line conjurador-detail',[model.descricaoComprometimento,detailText].filter(Boolean).join(' · ')):el('div','subtype-line',detailText);
    front.append(el('div','type-line',typeText),detail,el('div','rules',model.texto),el('div','flavour',model.ambientacao));
    const rarityIcon=img(`vetores/raridade-${n.dataset.rarity}.svg`,'rarity',`Raridade cosmética: ${n.dataset.rarity}`);front.append(rarityIcon);
    const fields=model.categoriaItem==='armadura'?[['defesa','defesa'],['vidaDesgaste','desgaste']]:[['ataque','ataque'],['vida','vida']];
    if(fields.some(([key])=>model[key]!=null)){const stats=el('div','stats');for(const [key,symbol] of fields)if(model[key]!=null){const stat=el('span','stat stat-'+key);stat.title={ataque:'Ataque',vida:'Vida',defesa:'Defesa',vidaDesgaste:'Vida de desgaste'}[key];stat.append(img(`vetores/icone-${symbol}-v5.svg`,'',stat.title),el('span','',model[key]));stats.append(stat);}front.append(stats);}
    if(['incomum','rara','lendaria','alternativa'].includes(n.dataset.finish)){const foil=el('div','foil-texture');foil.setAttribute('aria-hidden','true');front.append(foil);const reflection=el('div','foil-reflection');reflection.setAttribute('aria-hidden','true');front.append(reflection);}
    n.append(front);return n;
  }
  // Interface reutilizável pelo Claude: recebe só o modelo autorizado para o observador.
  const activeMotions=new WeakMap();
  function setSuspended(target,suspended=true) {
    if(!target)return;activeMotions.get(target)?.();target.classList.add('card-motion');target.classList.toggle('is-suspended',suspended);target.dataset.state=suspended?'suspensa':'pronta';target.setAttribute('aria-description',suspended?'Carta suspensa, deitada em 90 graus':'Carta pronta, na vertical');
  }
  function animateMotion(target,{action='attack',opponent=false}={}) {
    if(!target) return Promise.resolve({played:false});
    if(!['attack','block'].includes(action)) throw new Error('Movimento visual desconhecido');
    if(activeMotions.has(target))return Promise.resolve({played:false,reason:'em_movimento'});
    if(target.classList.contains('is-suspended'))return Promise.resolve({played:false,reason:'suspensa'});
    const cls=action==='block'?'block-generic':'attack-generic';
    const reduced=matchMedia('(prefers-reduced-motion: reduce)').matches;
    target.classList.add('card-motion');
    target.style.setProperty('--motion-advance',opponent?'72px':'-72px');
    return new Promise(resolve=>{
      let done=false,timer;
      const end=e=>{if(e.target===target)finish(false);};
      const finish=cancelled=>{if(done)return;done=true;clearTimeout(timer);target.removeEventListener('animationend',end);target.classList.add('motion-finalizing');if(!cancelled){target.classList.add('is-suspended');target.dataset.state='suspensa';target.setAttribute('aria-description','Carta suspensa, deitada em 90 graus');}target.classList.remove(cls);target.style.removeProperty('--motion-advance');void target.offsetWidth;target.classList.remove('motion-finalizing');activeMotions.delete(target);resolve({played:!reduced,cancelled,suspended:target.classList.contains('is-suspended')});};
      activeMotions.set(target,()=>finish(true));
      void target.offsetWidth;
      target.classList.add(cls);target.addEventListener('animationend',end);
      const css=getComputedStyle(target),paraMs=v=>v.trim().endsWith('ms')?parseFloat(v):parseFloat(v)*1000;
      const duracao=paraMs(css.animationDuration.split(',')[0])||1320,atraso=paraMs(css.animationDelay.split(',')[0])||0;
      timer=setTimeout(()=>finish(false),reduced?0:Math.max(0,duracao+atraso)+160);
    });
  }
  window.ArcanoclismaCardVisual={create:card,animateMotion,setSuspended};
  const stage=document.querySelector('#stage'),wrap=document.querySelector('.stage-wrap');
  const scale=()=>{stage.style.transform=`scale(${wrap.clientWidth/1920})`;};new ResizeObserver(scale).observe(wrap);scale();
  let view='jogador-1',reveal=false,currentType='aliado',currentRarity='comum',cardState='frente';
  const views={};
  views['jogador-1']={file:'arena-jogador-1-ampla-larga-v2',fieldTop:210,fieldBottom:900,description:'Jogador 1 de frente: sua área em primeiro plano (azul) e rival ao fundo.'};
  views['jogador-2']={file:'arena-jogador-2-ampla-larga-v2',fieldTop:210,fieldBottom:900,description:'Jogador 2 de frente: sua área em primeiro plano (coral) e rival ao fundo.'};
  const zones=document.querySelector('#zones'),board=document.querySelector('#board-cards'),decks=document.querySelector('#decks'),hands=document.querySelector('#hands'),hud=document.querySelector('#hud');
  function zone(kind,x,y,w,h,label,opponent=false){const z=el('div',`world-zone ${kind}${opponent?' opponent':''}`);Object.assign(z.style,{left:(x+160)+'px',top:y+'px',width:w+'px',height:h+'px'});z.append(el('span','zone-title',label));zones.append(z);}
  function positioned(model,x,y,opponent=false){const p=el('div',`board-position card-motion ${opponent?'opponent':'self'}${model.tipo==='conjurador'?' conjurador':''}`);Object.assign(p.style,{left:(x+160)+'px',top:y+'px'});p.append(card(model,{opponent,width:opponent?112:126}));board.append(p);}
  function deck(x,y,opponent){const p=el('div','deck-position'+(opponent?' opponent':''));Object.assign(p.style,{left:(x+160)+'px',top:y+'px'});p.append(img('vetores/pilha-baralho.svg','deck-base'),card(null,{hidden:true,width:99}),el('span','deck-count','32'));decks.append(p);}
  function hand(opponent){const h=el('div','hand-wrap '+(opponent?'opponent':'self'));h.append(img('vetores/base-mao.svg','hand-guide'));for(let i=0;i<5;i++){const m=models[i%models.length],p=el('div','card-motion');p.style.setProperty('--fan-x',(i-2)*(opponent?60:92)+'px');p.style.setProperty('--fan-y',Math.abs(i-2)*(opponent?3:11)+'px');p.style.setProperty('--fan-r',(i-2)*(opponent?4:5)+'deg');p.append(card(m,{hidden:opponent&&!reveal,opponent:opponent&&!reveal,readable:opponent&&reveal,width:opponent?78:139}));if(!opponent)p.addEventListener('click',()=>{document.querySelector('#type-select').value=modelKey(m);currentType=modelKey(m);loadEditor();});h.append(p);}hands.append(h);}
  function life(x,y,name,opponent){const n=el('div','life-hud'+(opponent?' opponent':''));Object.assign(n.style,{left:(x+160)+'px',top:y+'px'});n.append(img('vetores/contador-vida.svg',''),el('strong','',20),el('span','player-name',name),el('small','','Conjurador'));hud.append(n);}
  function arcano(x,y,value){const n=el('div','arcano-hud');Object.assign(n.style,{left:(x+160)+'px',top:y+'px'});n.append(img('vetores/contador-arcano.svg',''),el('strong','',value));hud.append(n);}
  function showItems(opponent=false){const dialog=document.querySelector('#items-dialog'),list=document.querySelector('#item-list');list.replaceChildren();document.querySelector('#items-title').textContent=opponent?'Itens do conjurador rival':'Itens do seu conjurador';
    for(const m of models.filter(x=>x.tipo==='item')){const box=el('section','item-option');box.append(el('h3','',names[m.categoriaItem]),card(m,{width:170}),el('p','',m.categoriaItem==='armadura'?'Defesa e vida de desgaste editáveis.':'Modelo de item para preencher com os dados do Claude.'));list.append(box);}dialog.showModal();
  }
  function itemButton(x,y,opponent){const b=el('button','items-button','Itens disponíveis');Object.assign(b.style,{left:(x+160)+'px',top:y+'px'});b.addEventListener('click',()=>showItems(opponent));hud.append(b);}
  function renderBoard(){const v=views[view],center=v.centerY||645;stage.dataset.view=view;document.querySelector('.phase').style.top=(center-4)+'px';document.querySelector('.phase').style.left='1170px';document.querySelector('.phase').style.width='320px';document.querySelector('.centre-seal').hidden=true;document.querySelector('#arena-floor').src=`raster/${v.file}.png`;document.querySelector('#view-description').textContent=v.description;for(const e of [zones,board,decks,hands,hud])e.replaceChildren();
    zone('campo',405,192,800,184,'',true);zone('campo',400,890,800,200,'');zone('grau',345,366,250,34,'GRAU',true);zone('grau',1000,847,250,34,'GRAU');zone('arcano',1060,365,220,40,'ARCANO',true);zone('arcano',305,847,220,40,'ARCANO');zone('cemiterio',1285,893,128,176,'CEMITÉRIO');zone('cemiterio',198,205,128,166,'CEMITÉRIO',true);
    positioned(models[0],520,210,true);positioned(models[2],710,210,true);positioned(models[3],900,210,true);positioned(models[0],495,900);positioned(models[2],710,900);positioned(models[3],925,900);
    const conj=models.find(m=>m.tipo==='conjurador');positioned(conj,737,center+45);positioned(conj,744,center-170,true);itemButton(882,center+96,false);itemButton(882,center-116,true);
    deck(1260,1180,false);deck(226,57,true);hand(true);hand(false);life(92,1190,view==='jogador-2'?'VOCÊ · J2':'VOCÊ · J1',false);life(1380,90,view==='jogador-2'?'ADVERSÁRIO · J1':'ADVERSÁRIO · J2',true);arcano(145,1092,3);arcano(1354,348,2);zone('pilha',1360,625,160,112,'PILHA');
  }
  function loadEditor(){const m=models.find(x=>modelKey(x)===currentType),isConj=m.tipo==='conjurador';document.querySelector('#alt-art-toggle').checked=m.cosmetico?.arteAlternativa===true;document.querySelector('#speed-select').value=m.velocidade||'findante';for(const [id,key] of [['generated-input','arcanoGerado'],['cost-input','custo'],['degree-input','grau']])document.querySelector('#'+id).value=m[key]||0;document.querySelector('#conjurador-editor').hidden=!isConj;document.querySelector('.speed-editor').hidden=isConj;document.querySelector('.resource-editor').hidden=isConj;document.querySelector('.symbol-key').hidden=isConj;for(const [id,value] of [['conjurador-value-1',m.valores?.[0]],['conjurador-value-2',m.valores?.[1]]])document.querySelector('#'+id).value=valorName(value)||'';document.querySelector('#conjurador-type').value=m.comprometimento||'';document.querySelector('#conjurador-description').value=m.descricaoComprometimento||'';renderInspector();}
  function renderInspector(){const source=models.find(x=>modelKey(x)===currentType),m={...source,cosmetico:{...source.cosmetico,arteAlternativa:document.querySelector('#alt-art-toggle').checked},velocidade:document.querySelector('#speed-select').value,arcanoGerado:count(document.querySelector('#generated-input').value),custo:count(document.querySelector('#cost-input').value),grau:count(document.querySelector('#degree-input').value)};if(m.tipo==='conjurador'){m.valores=['#conjurador-value-1','#conjurador-value-2'].map(id=>document.querySelector(id).value).filter(Boolean);m.comprometimento=document.querySelector('#conjurador-type').value;m.descricaoComprometimento=document.querySelector('#conjurador-description').value;}const hidden=cardState==='verso',opponent=cardState==='frente-rival',readable=cardState==='revelada';document.querySelector('#inspector-card').replaceChildren(card(m,{width:242,rarity:currentRarity,hidden,opponent,readable}));}
  document.querySelectorAll('[data-view]').forEach(b=>b.addEventListener('click',()=>{view=b.dataset.view;document.querySelectorAll('[data-view]').forEach(x=>x.classList.toggle('active',x===b));renderBoard();}));
  document.querySelector('#type-select').addEventListener('change',e=>{currentType=e.target.value;loadEditor();});document.querySelector('#rarity-select').addEventListener('change',e=>{currentRarity=e.target.value;renderInspector();});document.querySelectorAll('.resource-editor input').forEach(e=>e.addEventListener('input',renderInspector));
  document.querySelectorAll('#conjurador-editor select,#conjurador-editor input').forEach(e=>e.addEventListener('input',renderInspector));
  document.querySelector('#alt-art-toggle').addEventListener('change',renderInspector);
  document.querySelector('#reveal-hand').addEventListener('click',e=>{reveal=!reveal;e.target.textContent=reveal?'Voltar à mão oculta':'Mostrar mão revelada';renderBoard();});
  document.querySelector('#generic-attack').addEventListener('click',()=>animateMotion(document.querySelector('.board-position.self:not(.conjurador):not(.is-suspended)'),{action:'attack'}));
  document.querySelector('#generic-block').addEventListener('click',()=>animateMotion(document.querySelector('.board-position.self:not(.conjurador):not(.is-suspended)'),{action:'block'}));
  document.querySelector('#ready-cards').addEventListener('click',()=>document.querySelectorAll('.board-position').forEach(p=>setSuspended(p,false)));
  document.querySelector('#speed-select').addEventListener('change',renderInspector);
  document.querySelectorAll('[data-card-state]').forEach(b=>b.addEventListener('click',()=>{cardState=b.dataset.cardState;document.querySelectorAll('[data-card-state]').forEach(x=>x.classList.toggle('active',x===b));renderInspector();}));
  document.querySelector('#items-close').addEventListener('click',()=>document.querySelector('#items-dialog').close());
  renderBoard();loadEditor();
})();





