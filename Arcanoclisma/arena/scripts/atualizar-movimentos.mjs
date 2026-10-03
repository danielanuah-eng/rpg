import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
let js=await fs.readFile(path.join(root,'arena.js'),'utf8');
js=js.replace('window.ArcanoclismaCardVisual={create:card};',`const activeMotions=new WeakMap();
  function animateMotion(target,{action='attack',opponent=false}={}) {
    if(!target) return Promise.resolve({played:false});
    if(!['attack','block'].includes(action)) throw new Error('Movimento visual desconhecido');
    activeMotions.get(target)?.();
    const cls=action==='block'?'block-generic':'attack-generic';
    const reduced=matchMedia('(prefers-reduced-motion: reduce)').matches;
    target.classList.add('card-motion');
    target.style.setProperty('--motion-advance',opponent?'72px':'-72px');
    return new Promise(resolve=>{
      let done=false,timer;
      const end=e=>{if(e.target===target)finish(false);};
      const finish=cancelled=>{if(done)return;done=true;clearTimeout(timer);target.removeEventListener('animationend',end);target.classList.remove(cls);target.style.removeProperty('--motion-advance');activeMotions.delete(target);resolve({played:!reduced,cancelled});};
      activeMotions.set(target,()=>finish(true));
      void target.offsetWidth;
      target.classList.add(cls);target.addEventListener('animationend',end);
      timer=setTimeout(()=>finish(false),reduced?0:1000);
    });
  }
  window.ArcanoclismaCardVisual={create:card,animateMotion};`);
js=js.replace(/  document\.querySelector\('#generic-attack'\)\.addEventListener[^\n]+/,`  document.querySelector('#generic-attack').addEventListener('click',()=>animateMotion(document.querySelector('.board-position.self:not(.conjurador)'),{action:'attack'}));
  document.querySelector('#generic-block').addEventListener('click',()=>animateMotion(document.querySelector('.board-position.self:not(.conjurador)'),{action:'block'}));`);
js=js.replace('wrap.clientWidth/1600','wrap.clientWidth/1920');
// As coordenadas abaixo continuam em um palco lógico; abrir 160 px em cada lateral.
for(const s of ["left:x+'px'","left:x+'px'","left:x+'px'","left:x+'px'","left:x+'px'","left:x+'px'"]){js=js.replace(s,"left:(x+160)+'px'");}
js=js.replace("style.left='1010px'","style.left='1170px'");
// Duas perspectivas frontais vigentes; os estudos antigos permanecem em raster/.
js=js.replace(/  const views=\{superior:[^\n]+/, '  const views={};');
js=js.replace("file:'arena-jogador-1-ampla'","file:'arena-jogador-1-ampla-larga'").replace("file:'arena-jogador-2-ampla'","file:'arena-jogador-2-ampla-larga'");
await fs.writeFile(path.join(root,'arena.js'),js);
let html=await fs.readFile(path.join(root,'index.html'),'utf8');
html=html.replace(/<button data-view="(?:superior|inclinada|oponente)"[^>]*>[^<]*<\/button>/g,'');
html=html.replace(/<button id="generic-attack">[^<]*<\/button>/,'<button id="generic-attack">Suspender e atacar</button><button id="generic-block">Suspender e bloquear</button>');
await fs.writeFile(path.join(root,'index.html'),html);
console.log('Movimentos de ataque/bloqueio e palco lateral ampliado gravados.');
