import fs from 'node:fs/promises';import path from 'node:path';import {fileURLToPath} from 'node:url';import {createRequire} from 'node:module';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..'),require=createRequire(import.meta.url),sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
for(const id of ['generico','canal','armadura','extra']){
 const source=id==='generico'?'aliado':id;
 let s=await fs.readFile(path.join(root,`templates/${source}.svg`),'utf8');
 const stats=id==='armadura'?'<path d="M431 569h58v31q0 16-29 30-29-14-29-30Z" fill="#535c63" stroke="#d0ccc1" stroke-width="1.5"/><path d="M555 569 584 584v32l-29 15-29-15v-32Z" fill="#816647" stroke="#dac7a4" stroke-width="1.5"/>':id==='generico'?'<path d="M460 569 489 584v32l-29 15-29-15v-32Z" fill="#385a52" stroke="#c4d2c4" stroke-width="1.5"/><path d="M526 569h58v31q0 16-29 30-29-14-29-30Z" fill="#713f4c" stroke="#dabbbc" stroke-width="1.5"/>':'';
 const badges='<circle cx="52" cy="51" r="34" fill="#638697" stroke="#142837" stroke-width="2"/><circle cx="52" cy="132" r="31" fill="#182735" stroke="#6479a1" stroke-width="4"/><path d="M52 190 83 221 52 252 21 221Z" fill="#182735" stroke="#b3c5cd" stroke-width="2"/><circle cx="315" cy="868" r="6" fill="#a9b3ba"/>';
 s=s.replace('width="63mm" height="88mm"','width="1260" height="1760"').replace('</svg>',badges+stats+'</svg>');
 await fs.writeFile(path.join(root,`templates/overlay-${id}-v3.svg`),s);
 await sharp(Buffer.from(s)).png().toFile(path.join(root,`templates/overlay-${id}-v3.png`));
}
console.log('Molduras decoradas discretamente: genérica, Canal, Armadura e Extra.');
