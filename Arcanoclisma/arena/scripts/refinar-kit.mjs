import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(import.meta.url);
const sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const save=(f,s)=>fs.writeFile(path.join(root,f),s+'\n');
const defs='<defs><linearGradient id="paper" x2="0" y2="1"><stop stop-color="#f4eddd" stop-opacity=".85"/><stop offset="1" stop-color="#e8dfcd" stop-opacity=".93"/></linearGradient></defs>';
const themes={aliado:'#728d92',temporaria:'#a581a7',mistica:'#8d9b86',estrutura:'#a49377',conjurador:'#b7a36c',canal:'#78aab9',armadura:'#a59b89',extra:'#a98fa6'};
for(const [id,color] of Object.entries(themes)){
 const item=['canal','armadura','extra'].includes(id);
 const edges=`<rect x="7" y="7" width="616" height="866" rx="22" fill="none" stroke="#152434" stroke-width="8"/><rect x="12" y="12" width="606" height="856" rx="18" fill="none" stroke="#b7a17b" stroke-width="2"/><path d="M20 80V38q0-18 18-18h46M610 80V38q0-18-18-18h-46M20 802v38q0 18 18 18h46M610 802v38q0 18-18 18h-46" fill="none" stroke="${color}" stroke-width="4"/><path d="M22 99q12 10 0 30M608 99q-12 10 0 30" fill="none" stroke="#c6b085" stroke-width="2"/>`;
 const itemDetail=id==='canal'?'<path d="M25 575q20 12 0 24M605 575q-20 12 0 24" fill="none" stroke="#9bc5cf" stroke-width="3"/>':id==='armadura'?'<path d="M21 122 34 135 21 148M609 122 596 135 609 148" fill="none" stroke="#c5c0ae" stroke-width="4"/>':id==='extra'?'<path d="m23 150 8 10-8 10M607 150l-8 10 8 10" fill="none" stroke="#cdb2ce" stroke-width="3"/>':'';
 const inner=`<path d="M12 12h606v77H12Z" fill="#152939" fill-opacity=".75"/><rect x="24" y="650" width="582" height="203" rx="14" fill="url(#paper)"/><path d="M42 677h34M588 677h-34" stroke="${color}" stroke-width="2"/><path d="M285 868h-35M345 868h35" stroke="#b7a17b" stroke-width="2"/><circle cx="315" cy="868" r="12" fill="#132a39" stroke="#b7a17b" stroke-width="2"/>`;
 const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="63mm" height="88mm" viewBox="0 0 630 880">${defs}${edges}${inner}${itemDetail}</svg>`;
 await save(`templates/${id}.svg`,svg);
 if(item)await sharp(Buffer.from(svg)).resize(1260,1760).png().toFile(path.join(root,`templates/overlay-${id}.png`));
}
const glyph={defesa:'<path d="M8 8h48v28q0 17-24 25Q8 53 8 36Z" fill="none" stroke="currentColor" stroke-width="5"/><path d="M32 17v30M20 32h24" stroke="currentColor" stroke-width="4"/>',desgaste:'<path d="M8 8h48v28q0 17-24 25Q8 53 8 36Z" fill="none" stroke="currentColor" stroke-width="5"/><path d="m36 12-12 16 13 4-12 16" stroke="currentColor" fill="none" stroke-width="5"/>',item:'<path d="M9 22h46v34H9ZM20 22V10h24v12M8 34h48M28 30h8v11h-8Z" fill="none" stroke="currentColor" stroke-width="4"/>',canal:'<path d="M32 5 47 27 32 59 17 27Z" fill="none" stroke="currentColor" stroke-width="4"/><path d="M32 14v38M17 27h30" stroke="currentColor" stroke-width="3"/>',armadura:'<path d="M16 8 6 25l11 9v24h30V34l11-9L48 8l-9 12H25Z" fill="none" stroke="currentColor" stroke-width="4"/>',extra:'<path d="M32 5 57 32 32 59 7 32Z" fill="none" stroke="currentColor" stroke-width="4"/><circle cx="32" cy="32" r="9" fill="currentColor"/>'};
for(const [id,body] of Object.entries(glyph))await save(`vetores/icone-${id}.svg`,`<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64"><g color="#f4e8d1">${body}</g></svg>`);
const data=JSON.parse((await fs.readFile(path.join(root,'modelos-cartas.json'),'utf8')).replace(/^\uFEFF/,''));
data.cartas=data.cartas.filter(x=>x.tipo!=='item');
data.cartas[0].nome='Dragão do Limiar';data.cartas[0].arte=data.cartas[2].arte;data.cartas[0].subtipos=['Dragão','Guardião'];data.cartas[0].valores=['Persona','Eternyr'];
for(const [i,categoriaItem] of ['canal','armadura','extra'].entries())data.cartas.push({id:`modelo-item-${categoriaItem}`,tipo:'item',categoriaItem,nome:{canal:'Canal arcano',armadura:'Armadura do Limiar',extra:'Item Extra'}[categoriaItem],subtipos:[categoriaItem==='armadura'?'Proteção':'Equipamento'],valores:['Cognitium'],custo:2,grau:1,arcanoGerado:2,texto:'[Texto de regras do item]',ambientacao:'',arte:'',defesa:categoriaItem==='armadura'?'—':null,vidaDesgaste:categoriaItem==='armadura'?'—':null,cosmetico:{raridade:'comum'},demonstracaoVisual:true});
data.aviso='Modelos visuais. Item/categoriaItem/defesa/vidaDesgaste são proposta do autor para o Claude integrar ao motor. Extra é nome provisório.';
await save('modelos-cartas.json',JSON.stringify(data,null,2));await save('modelos-cartas.js','window.ARENA_MODELOS='+JSON.stringify(data.cartas)+';');
console.log('8 molduras, 3 modelos de itens e símbolos próprios criados.');


