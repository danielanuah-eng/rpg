import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const read=async file=>JSON.parse(await fs.readFile(path.join(root,file),'utf8'));
const write=async(file,data)=>fs.writeFile(path.join(root,file),JSON.stringify(data,null,2)+'\n');
const marker='/* Lendária v7: reflexo diagonal mais exuberante sobre a face. */';
const css=`${marker}
.tcg-card .foil-texture{opacity:.96}
.tcg-card .foil-texture:before{animation:none!important;opacity:0!important}
.tcg-card:not(.is-hidden)[data-rarity="lendaria"] .foil-reflection{
  position:absolute;inset:0;border-radius:inherit;overflow:hidden;
  z-index:3;pointer-events:none;mix-blend-mode:screen;
}
.tcg-card .foil-reflection:before,.tcg-card .foil-reflection:after{
  content:'';position:absolute;inset:-50%;
  transform:translate3d(-72%,-72%,0);opacity:0;
  animation:legendary-diagonal-v7 5.2s linear infinite;
}
.tcg-card .foil-reflection:before{
  --foil-peak:.66;
  background:linear-gradient(135deg,transparent 30%,#ffd56f44 38%,#ffd675bb 43%,#ff9ecfb0 48%,#b998ffc7 51%,#79cfffbd 54%,#9be6b0b8 58%,#fff0b5da 61%,#ffc75f99 66%,transparent 74%);
}
.tcg-card .foil-reflection:after{
  --foil-peak:.28;
  background:linear-gradient(135deg,transparent 42%,#ffe6a433 47%,#fff5d0aa 49.2%,#fffdf2 50%,#ffe19faa 50.8%,#ffd77a33 53%,transparent 58%);
}
@keyframes legendary-diagonal-v7{
  0%,10%{transform:translate3d(-72%,-72%,0);opacity:0}
  18%{transform:translate3d(-52%,-52%,0);opacity:calc(var(--foil-peak)*.25)}
  40%{transform:translate3d(-20%,-20%,0);opacity:var(--foil-peak)}
  60%{transform:translate3d(20%,20%,0);opacity:var(--foil-peak)}
  82%{transform:translate3d(52%,52%,0);opacity:calc(var(--foil-peak)*.25)}
  90%,100%{transform:translate3d(72%,72%,0);opacity:0}
}
@media(prefers-reduced-motion:reduce){
  .tcg-card .foil-reflection:before,.tcg-card .foil-reflection:after{
    animation:none!important;transform:none;
  }
  .tcg-card .foil-reflection:before{opacity:.23}
  .tcg-card .foil-reflection:after{opacity:.08}
}
/* Fim do reflexo lendário v7. */
`;
const cssFile=path.join(root,'cartas.css');
let cssSource=await fs.readFile(cssFile,'utf8');
if(cssSource.includes(marker)){
  const start=cssSource.indexOf(marker),endMarker='/* Fim do reflexo lendário v7. */',end=cssSource.indexOf(endMarker,start);
  const next=cssSource.indexOf('\n/*',start+marker.length);
  const stop=end>=0?end+endMarker.length:next>=0?next:cssSource.length;
  cssSource=cssSource.slice(0,start)+cssSource.slice(stop);
}
await fs.writeFile(cssFile,cssSource.trimEnd()+'\n'+css);

const jsFile=path.join(root,'arena.js');
let jsSource=await fs.readFile(jsFile,'utf8');
if(!jsSource.includes("el('div','foil-reflection')")){
  const before="foil.setAttribute('aria-hidden','true');front.append(foil);";
  const after="foil.setAttribute('aria-hidden','true');front.append(foil);const reflection=el('div','foil-reflection');reflection.setAttribute('aria-hidden','true');front.append(reflection);";
  if(!jsSource.includes(before))throw new Error('Ponto de montagem da película lendária não encontrado.');
  jsSource=jsSource.replace(before,after);
  await fs.writeFile(jsFile,jsSource);
}

const spec=await read('acabamento-v6.json');
spec.versao=7;
Object.assign(spec.raridades.lendaria,{
  efeito:'Textura holográfica com faixa larga dourada/arco-íris e reflexo luminoso, atravessando a face do canto superior esquerdo ao inferior direito.',
  opacidadeTextura:0.96,
  opacidadeReflexo:[0,0.66],
  opacidadeReflexoDourado:[0,0.28],
  periodoSegundos:5.2,
  mesclagemReflexo:'screen',
  direcaoReflexo:'superior esquerdo → inferior direito',
  classeReflexo:'foil-reflection',
  animacao:'legendary-diagonal-v7'
});
await write('acabamento-v7.json',spec);
for(const file of ['layout.json','modelo-online.json']){
  const data=await read(file);data.acabamento=spec;await write(file,data);
}
const templates=await read('../cartas/templates.json');
templates.revisaoVisual=7;
templates.revisaoAtual.acabamento=spec;
templates.canva.resultado='Revisão v5 dos atributos em rascunho; textura e reflexo diagonal lendário v7 no HTML/CSS.';
templates.acabamentoLendario.especificacao='../arena/acabamento-v7.json';
await write('../cartas/templates.json',templates);
const delivery=await read('../cartas/entregas/arena-2026-10-03.visual.json');
delivery.revisaoAtual.versao=7;delivery.revisaoAtual.acabamento=spec;
await write('../cartas/entregas/arena-2026-10-03.visual.json',delivery);
const state=await read('retomada.json');
state.revisaoAtual=7;
state.acabamentoLendario='Textura holográfica com reflexo dourado/arco-íris mais exuberante atravessando a face em diagonal.';
await write('retomada.json',state);

const appendOnce=async(file,heading,body)=>{
  const source=await fs.readFile(path.join(root,file),'utf8');
  if(!source.includes(heading))await fs.appendFile(path.join(root,file),'\n'+heading+'\n\n'+body+'\n');
};
await appendOnce('AGENTS.md','## Reflexo lendário v7',
  'A orientação atual do autor pede efeito mais visível e exuberante: arco-íris dourado atravessando a face de um canto ao outro. Textura v6 reutilizada com opacidade 0,96; faixa v7 em screen, sob textos e números, ciclo de 5,2s. Consultar acabamento-v7.json.');
await appendOnce('entrega-claude.md','## Reflexo diagonal lendário v7',
  'Montar .foil-reflection como irmã de .foil-texture dentro da frente pública lendária. Ambas ficam na camada 3; textos/estatísticas na 4 e recursos na 5. A textura SVG v6 permanece, com opacidade 0,96. O reflexo screen tem uma faixa larga arco-íris/dourada (pico 0,66) e uma linha de luz dourada (pico 0,28), atravessando a face do canto superior esquerdo ao inferior direito a cada 5,2s. A camada independente preserva o brilho do screen sem atenuá-lo pelo soft-light da textura. Halo/borda seguem discretos. Movimento reduzido deixa reflexo estático. CSS: legendary-diagonal-v7; configuração: acabamento-v7.json. Refinamento da animação salvo, sem execução no navegador nesta revisão.');
console.log('Reflexo diagonal lendário v7 aplicado e documentado.');
