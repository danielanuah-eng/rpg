import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const sharp=createRequire(import.meta.url)('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const save=(f,s)=>fs.writeFile(path.join(root,f),s+'\n');
const themes={aliado:'#7f9ead',temporaria:'#bca0c9',mistica:'#a7b695',estrutura:'#c1ad84',conjurador:'#d5bd84',canal:'#9bc9d4',armadura:'#c3beb1',extra:'#c7a4c1'};
const metal='<defs><linearGradient id="metal" x2="0" y2="1"><stop stop-color="#f1deae"/><stop offset=".42" stop-color="#b79b68"/><stop offset=".72" stop-color="#7b6950"/><stop offset="1" stop-color="#d7c199"/></linearGradient><linearGradient id="title" x2="0" y2="1"><stop stop-color="#40596b"/><stop offset="1" stop-color="#162c3b"/></linearGradient></defs>';
for(const [id,color]of Object.entries(themes)){
 let s=await fs.readFile(path.join(root,`templates/${id}.svg`),'utf8');
 s=s.replace('</defs>','</defs>'+metal).replace('stroke="#b7a17b" stroke-width="2"','stroke="#d0b787" stroke-width="3"');
 const header=`<path d="M107 29h461l25 27-25 26H107L90 56Z" fill="url(#title)" fill-opacity=".91" stroke="url(#metal)" stroke-width="2.5"/><path d="M116 34h445l19 22-19 21H116" fill="none" stroke="${color}" stroke-opacity=".7" stroke-width="1.6"/><path d="m281 27 17-5 17 5 17-5 17 5M284 84l15 5 16-5 16 5 15-5" fill="none" stroke="url(#metal)" stroke-width="2"/><path d="M114 48v16M575 46l8 10-8 10" fill="none" stroke="${color}" stroke-width="2"/>`;
 s=s.replace('<path d="M12 12h606v77H12Z" fill="#152939" fill-opacity=".75"/>',header);
 s=s.replace('</svg>',`<path d="M21 310v105m0 6v15M609 310v105m0 6v15" stroke="${color}" stroke-opacity=".8" stroke-width="2.4"/><path d="m18 298 4-7 4 7-4 7ZM604 298l4-7 4 7-4 7Z" fill="${color}"/><path d="M35 648q17-9 34 0M561 648q17-9 34 0" fill="none" stroke="url(#metal)" stroke-width="2.4"/></svg>`);
 await save(`templates/${id}-v4.svg`,s);
}
const values={conflituo:['#b85a43','M24 14 15 38l18-10-9 27 27-33-19 9 7-17'],sublimeyn:['#94bcc9','M17 35q15-30 30 0M12 45q20-19 40 0'],dominum:['#9675ac','M13 23 25 32l7-19 7 19 12-9-6 26H19Z'],persona:['#d89aac','M17 24q8-12 15 0 7-12 15 0 10 16-15 29-25-13-15-29Z'],alvorum:['#d6ba75','M32 10v8M32 46v8M10 32h8M46 32h8M17 17l6 6M41 41l6 6M17 47l6-6M41 23l6-6'],morbidyum:['#9cbea5','M19 21q13-15 26 0v19L32 54 19 40ZM25 30h2M37 30h2M27 43h10'],cognitium:['#97a9dc','M10 32q22-28 44 0-22 28-44 0ZM32 24v16'],eternyr:['#a5b9d2','M16 15h32L17 49h30ZM18 15l29 34M47 15 18 49'],primitivum:['#acbc80','M32 10q24 28 0 45-24-17 0-45ZM32 23v26']};
for(const [id,[color,d]]of Object.entries(values))await save(`vetores/valor-${id}.svg`,`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><circle cx="32" cy="32" r="30" fill="#182d3b" stroke="${color}" stroke-width="3"/><path d="${d}" fill="none" stroke="${color}" stroke-width="3.2" stroke-linecap="round" stroke-linejoin="round"/></svg>`);
const cost=`<circle cx="52" cy="132" r="38" fill="#162b3b" stroke="#d0b484" stroke-width="2"/><circle cx="52" cy="132" r="31" fill="#152634" stroke="#7c9bc4" stroke-width="3"/><path d="M52 91v10M52 163v10M11 132h10M83 132h10" stroke="#e2c997" stroke-width="2.6"/><path d="m24 104-5-5M80 160l5 5M80 104l5-5M24 160l-5 5" stroke="#a4bdd3" stroke-width="2"/>`;
const degree=`<path d="M52 178 94 221 52 264 10 221Z" fill="#162c3b" stroke="#d2bb87" stroke-width="2.5"/><path d="M52 187 85 221 52 255 19 221Z" fill="#233c4f" stroke="#9fc6d6" stroke-width="2"/><path d="m44 179 8-7 8 7M44 263l8 7 8-7M11 214l-6 7 6 7M93 214l6 7-6 7" fill="none" stroke="#d9c08a" stroke-width="2"/>`;
await save('vetores/acabamento-custo-v4.svg',`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><g transform="translate(-2 -82)">${cost}</g></svg>`);
await save('vetores/acabamento-grau-v4.svg',`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 104 104"><g transform="translate(0 -169)">${degree}</g></svg>`);
const badge=(kind)=>{const shield=['vida','defesa'].includes(kind),color={ataque:'#3a6660',vida:'#794e62',defesa:'#526573',desgaste:'#846745'}[kind],d=shield?'M14 14h52v35q0 18-26 29-26-11-26-29Z':'M40 9 68 25v36L40 78 12 61V25Z';return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 88"><defs><linearGradient id="b-${kind}" x2="0" y2="1"><stop stop-color="${color}"/><stop offset="1" stop-color="#172e3b"/></linearGradient></defs><path d="${d}" fill="url(#b-${kind})" stroke="#d6be8c" stroke-width="2.8"/><path d="M23 22h34M23 68l17 8 17-8" fill="none" stroke="#f3e0b0" stroke-opacity=".7" stroke-width="1.6"/><path d="M12 31 5 35l7 8M68 31l7 4-7 8" fill="none" stroke="#bca676" stroke-width="2"/><circle cx="40" cy="10" r="2.4" fill="#f3d8a0"/></svg>`;};
for(const id of ['ataque','vida','defesa','desgaste'])await save(`vetores/estatistica-${id}-v4.svg`,badge(id));
const insertBadge=(kind,x)=>`<svg x="${x}" y="562" width="76" height="80" viewBox="0 0 80 88">${badge(kind).replace(/^<svg[^>]*>/,'').replace('</svg>','')}</svg>`;
for(const id of ['generico','canal','armadura','extra']){
 let s=await fs.readFile(path.join(root,`templates/${id==='generico'?'aliado':id}-v4.svg`),'utf8');
 const stats=id==='generico'?insertBadge('ataque',422)+insertBadge('vida',517):id==='armadura'?insertBadge('defesa',422)+insertBadge('desgaste',517):'';
 const generated='<circle cx="52" cy="51" r="34" fill="#547a90" stroke="#d5c393" stroke-width="2.5"/><circle cx="52" cy="51" r="29" fill="none" stroke="#aacbd9" stroke-opacity=".75" stroke-width="1.5"/>';
 const value=`<svg x="44" y="159" width="16" height="16" viewBox="0 0 64 64">${(await fs.readFile(path.join(root,'vetores/valor-persona.svg'),'utf8')).replace(/^<svg[^>]*>/,'').replace('</svg>','')}</svg>`;
 s=s.replace('width="63mm" height="88mm"','width="1260" height="1760"').replace('</svg>',generated+cost+degree+value+stats+'<circle cx="315" cy="868" r="6" fill="#b9c6cb"/></svg>');
 await save(`templates/overlay-${id}-v4.svg`,s);await sharp(Buffer.from(s)).png().toFile(path.join(root,`templates/overlay-${id}-v4.png`));
}
console.log('v4: oito molduras, quatro overlays, nove símbolos de Valor e estatísticas adornadas. v3 preservada.');
