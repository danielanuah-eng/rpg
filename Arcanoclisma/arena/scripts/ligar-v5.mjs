import fs from 'node:fs/promises';import path from 'node:path';import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
let js=await fs.readFile(path.join(root,'arena.js'),'utf8');js=js.replace('g.style.background=colors','g.style.backgroundColor=colors').replace('vetores/icone-${symbol}.svg','vetores/icone-${symbol}-v5.svg');await fs.writeFile(path.join(root,'arena.js'),js);
let html=await fs.readFile(path.join(root,'index.html'),'utf8');for(const k of ['ataque','vida','defesa','desgaste','grau','custo-arcano','arcano-gerado'])html=html.replaceAll('vetores/icone-'+k+'.svg','vetores/icone-'+k+'-v5.svg');await fs.writeFile(path.join(root,'index.html'),html);
let g=await fs.readFile(path.join(root,'scripts/gerar-vetores.mjs'),'utf8');if(!g.includes("import('./refinar-atributos-v5.mjs')"))g+="\nawait import('./refinar-atributos-v5.mjs');\nawait import('./ligar-v5.mjs');\n";await fs.writeFile(path.join(root,'scripts/gerar-vetores.mjs'),g);
console.log('Ícones e ornamentos v5 ligados à prévia.');
