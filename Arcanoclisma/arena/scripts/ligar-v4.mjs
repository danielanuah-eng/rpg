import fs from 'node:fs/promises';import path from 'node:path';import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
let html=await fs.readFile(path.join(root,'index.html'),'utf8');
if(!html.includes('id="speed-select"'))html=html.replace('<div class="resource-editor">','<label class="speed-editor">Velocidade de jogo<select id="speed-select"><option value="findante">Findante</option><option value="repentina">Repentina</option></select></label><div class="resource-editor">');
if(!html.includes('id="ready-cards"'))html=html.replace('<button id="generic-block">Suspender e bloquear</button>','<button id="generic-block">Suspender e bloquear</button><button id="ready-cards">Levantar cartas</button>');
await fs.writeFile(path.join(root,'index.html'),html);
let js=await fs.readFile(path.join(root,'arena.js'),'utf8');
js=js.replace("currentType);for(const [id,key]","currentType);document.querySelector('#speed-select').value=m.velocidade||'findante';for(const [id,key]");
await fs.writeFile(path.join(root,'arena.js'),js);
const f=path.join(root,'modelos-cartas.json'),data=JSON.parse(await fs.readFile(f,'utf8'));for(const m of data.cartas){if(m.tipo!=='item'&&m.tipo!=='conjurador')m.velocidade=m.tipo==='temporaria'?'repentina':'findante';m.arteReferenciaLayout=true;m.arteFinalDeCarta=false;}
await fs.writeFile(f,JSON.stringify(data,null,2)+'\n');await fs.writeFile(path.join(root,'modelos-cartas.js'),'window.ARENA_MODELOS='+JSON.stringify(data.cartas)+';\n');
const generator=path.join(root,'scripts/gerar-vetores.mjs');let g=await fs.readFile(generator,'utf8');if(!g.includes("import('./refinar-cartas-v4.mjs')"))g+="\nawait import('./refinar-cartas-v4.mjs');\n";await fs.writeFile(generator,g);
console.log('v4 ligada à prévia; velocidade explícita e retorno da carta à vertical disponíveis.');
