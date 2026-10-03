import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const sharp=createRequire(import.meta.url)('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const read=async file=>JSON.parse(await fs.readFile(path.join(root,file),'utf8'));
const write=async(file,data)=>fs.writeFile(path.join(root,file),JSON.stringify(data,null,2)+'\n');
const values=['Conflitüo','Sublimeyn','Dominum','Persona','Alvorum','Morbidyum','Cognitium','Eternyr','Primitivum'];
const slug=v=>v.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase();
const types=['Invocador','Brigão','Parrudão da quebrada','Habilidoso','Preparado'];
for(const value of values){
  const source=await fs.readFile(path.join(root,`vetores/valor-${slug(value)}.svg`),'utf8');
  await fs.writeFile(path.join(root,`vetores/valor-${slug(value)}-simbolo-v8.svg`),source.replace(/<circle\b[^>]*\/>/,''));
}
const rim=`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 76 76"><defs><linearGradient id="orbmetal" x2=".2" y2="1"><stop stop-color="#f8e4b3"/><stop offset=".4" stop-color="#c4a66d"/><stop offset=".7" stop-color="#76604b"/><stop offset="1" stop-color="#dec493"/></linearGradient><radialGradient id="orbshine" cx=".3" cy=".18" r=".7"><stop stop-color="#fff2c5" stop-opacity=".2"/><stop offset="1" stop-color="#071828" stop-opacity=".12"/></radialGradient></defs><circle cx="38" cy="38" r="36.5" fill="url(#orbshine)" stroke="#142332" stroke-width="2.5"/><circle cx="38" cy="38" r="34.6" fill="none" stroke="url(#orbmetal)" stroke-width="1.4"/><path d="M38 4.2v67.6" stroke="#d7bd89" stroke-width="1" stroke-opacity=".8"/><path d="M13 17q9-9 21-9M59 63q-8 5-18 5" fill="none" stroke="#fff2cc" stroke-width="1" stroke-opacity=".6"/></svg>`;
await fs.writeFile(path.join(root,'vetores/esfera-conjurador-v8.svg'),rim+'\n');
let frame=await fs.readFile(path.join(root,'templates/conjurador-v4.svg'),'utf8');
frame=frame.replace('</svg>','<path d="M76 683h478" stroke="#baab8a" stroke-width="1" stroke-opacity=".7"/></svg>');
await fs.writeFile(path.join(root,'templates/conjurador-v8.svg'),frame);
const stats=await Promise.all(['ataque','vida'].map(async key=>{
  const svg=await fs.readFile(path.join(root,`vetores/estatistica-${key}-v5.svg`),'utf8');
  const viewBox=svg.match(/viewBox="([^"]+)"/)?.[1]||'0 0 80 84';
  const body=svg.replace(/<svg\b[^>]*>/,'').replace('</svg>','').replace(/\bid="([^"]+)"/g,(_,id)=>`id="conj-${key}-${id}"`).replace(/url\(#([^)]+)\)/g,(_,id)=>`url(#conj-${key}-${id})`);
  return `<svg x="${key==='ataque'?420:515}" y="559" width="80" height="84" viewBox="${viewBox}">${body}</svg>`;
}));
const overlay=frame.replace('width="63mm" height="88mm"','width="1260" height="1760"').replace('</svg>',`<svg x="287" y="88" width="56" height="56" viewBox="0 0 76 76">${rim.replace(/<svg\b[^>]*>/,'').replace('</svg>','')}</svg>${stats.join('')}</svg>`);
await fs.writeFile(path.join(root,'templates/overlay-conjurador-v8.svg'),overlay);
const png=path.join(root,'templates/overlay-conjurador-v8.png');await sharp(Buffer.from(overlay)).png().toFile(png);
const {data,info}=await sharp(png).ensureAlpha().raw().toBuffer({resolveWithObject:true});let zero=0;for(let n=3;n<data.length;n+=4)if(data[n]===0)zero++;
const alpha={pixelsTransparentes:zero,cantos:[[0,0],[info.width-1,0],[0,info.height-1],[info.width-1,info.height-1]].map(([x,y])=>data[(y*info.width+x)*4+3])};
const options=values.map(v=>`<option value="${v}">${v}</option>`).join('');
const editor=`<section id="conjurador-editor" class="conjurador-editor" hidden><div class="selectors"><label>Valor · metade esquerda<select id="conjurador-value-1"><option value="">[Valor 1]</option>${options}</select></label><label>Valor · metade direita<select id="conjurador-value-2"><option value="">[Valor 2]</option>${options}</select></label></div><label>Tipo · Comprometimento<select id="conjurador-type"><option value="">[Comprometimento]</option>${types.map(t=>`<option>${t}</option>`).join('')}</select></label><label>Descrição do tipo<input id="conjurador-description" type="text" placeholder="Descrição curta, sem substituir a habilidade"></label><p class="quiet">A esfera mostra os dois Valores. Valores de exemplo editáveis; não definem a identidade canônica de Ciantia.</p></section>`;
let html=await fs.readFile(path.join(root,'index.html'),'utf8');
if(!html.includes('id="conjurador-editor"'))html=html.replace('<div class="symbol-key">',editor+'<div class="symbol-key">');
await fs.writeFile(path.join(root,'index.html'),html);
const cssMarker='/* Conjurador v8: esfera de dois Valores e Comprometimento. */',cssEnd='/* Fim do conjurador v8. */';
const css=[cssMarker,
'.tcg-card .conjurador-values{position:absolute;left:45.56%;top:10%;width:8.89%;aspect-ratio:1;border-radius:50%;overflow:hidden;display:flex;z-index:5;background:#193044;box-shadow:0 1px 2px #09142366}',
'.tcg-card .conjurador-value-half{width:50%;height:100%;display:flex;align-items:center;justify-content:center;background:linear-gradient(145deg,#0000,#07182988),var(--valor-color);position:relative}',
'.tcg-card .conjurador-value-half img{width:100%;height:66%;object-fit:contain;filter:drop-shadow(0 1px 1px #07182899)}',
'.tcg-card .conjurador-sphere-rim{position:absolute;inset:0;width:100%;height:100%;pointer-events:none}',
'.tcg-card .value-placeholder{color:#dfceb2;font:700 calc(var(--card-w)*.029)/1 Georgia,serif}',
'.tcg-card[data-type="conjurador"] .type-line{white-space:normal;max-height:3.4%;font-size:calc(var(--card-w)*.035);line-height:1.18}',
'.tcg-card .conjurador-detail{top:79.2%;max-height:4.7%;font-size:calc(var(--card-w)*.032);line-height:1.3}',cssEnd].join('\n')+'\n';
const cssFile=path.join(root,'cartas.css');let cssSource=await fs.readFile(cssFile,'utf8');
if(cssSource.includes(cssMarker)){const start=cssSource.indexOf(cssMarker),end=cssSource.indexOf(cssEnd,start);if(end<0)throw new Error('Bloco de CSS do conjurador incompleto.');cssSource=cssSource.slice(0,start)+cssSource.slice(end+cssEnd.length);}
await fs.writeFile(cssFile,cssSource.trimEnd()+'\n'+css);
const arenaCssFile=path.join(root,'arena.css');let arenaCss=await fs.readFile(arenaCssFile,'utf8');
if(!arenaCss.includes('/* Editor do conjurador v8 */'))arenaCss+='\n/* Editor do conjurador v8 */\n.conjurador-editor{display:grid;gap:10px}.conjurador-editor label{display:grid;gap:5px}.conjurador-editor input{width:100%;padding:8px;background:#172e3a;color:#efe8d6;border:1px solid #63716f;border-radius:6px}.conjurador-editor[hidden],.resource-editor[hidden],.speed-editor[hidden],.symbol-key[hidden]{display:none}\n';
await fs.writeFile(arenaCssFile,arenaCss);
const models=await read('modelos-cartas.json'),conj=models.cartas.find(m=>m.tipo==='conjurador');
conj.valores=['Persona','Sublimeyn'];conj.valoresReferenciaLayout=true;conj.comprometimento=null;conj.descricaoComprometimento='[Descrição do Comprometimento]';conj.identidadeCanonica=false;conj.pendenciasIdentidade=['Par de Valores e Comprometimento reais de Ciantia a informar pelo Claude/autor; os Valores desta prévia são exemplos.'];
await write('modelos-cartas.json',models);await fs.writeFile(path.join(root,'modelos-cartas.js'),'window.ARENA_MODELOS='+JSON.stringify(models.cartas)+';\n');
const spec={versao:8,template:'templates/conjurador-v8.svg',overlay:['templates/overlay-conjurador-v8.svg','templates/overlay-conjurador-v8.png'],esfera:'vetores/esfera-conjurador-v8.svg',simbolos:'vetores/valor-*-simbolo-v8.svg',valores:'ConjuradorDef.valores: dois símbolos completos, cada um centralizado em sua metade; cores separadas; sem custo/Arcano gerado.',tipo:{campoMotor:'ConjuradorDef.comprometimento',opcoes:types,descricao:'descricaoComprometimento: texto visual editável; não inventar benefícios mecânicos.'},referencias:['Arcanoclisma/tcg/src/tipos.ts:115','Arcanoclisma/tcg/src/conjuradores.ts:1','Arcanoclisma/tcg/docs/LIVRO-DE-REGRAS.md:168'],camadas:{esfera:5,tipoEDescricao:4,reflexoLendario:3},dimensoes:[info.width,info.height],alfa:alpha,canva:'Asset local pronto para composição futura; não altera os rascunhos v5 nem confirma salvamento no Canva.'};
spec.posicaoEsfera={viewBox:[630,880],centro:[315,116],diametro:56,descricao:'Centralizada logo abaixo do nome, fora da caixa de texto; pequena e discreta.'};
await write('conjurador-v8.json',spec);await write('registros/conjurador-v8.json',{...spec,procedencia:'Vetores próprios; símbolos derivados dos SVGs de Valor existentes; composição SVG exportada com Sharp.'});
const online=await read('modelo-online.json');online.modeloPublico.comprometimento='ConjuradorDef.comprometimento';online.modeloPublico.descricaoComprometimento='Texto visual opcional do Comprometimento, fornecido pelo Claude/autor';online.conjurador=spec;for(const f of ['comprometimento','descricaoComprometimento'])if(!online.oculta.camposProibidos.includes(f))online.oculta.camposProibidos.push(f);await write('modelo-online.json',online);
const layout=await read('layout.json');layout.conjurador=spec;await write('layout.json',layout);
const templates=await read('../cartas/templates.json');templates.revisaoVisual=8;templates.conjurador=spec;await write('../cartas/templates.json',templates);
const delivery=await read('../cartas/entregas/arena-2026-10-03.visual.json');delivery.revisaoAtual.versao=8;delivery.revisaoAtual.conjurador=spec;await write('../cartas/entregas/arena-2026-10-03.visual.json',delivery);
const state=await read('retomada.json');state.revisaoAtual=8;state.conjurador='Esfera dividida em dois Valores, linha de Comprometimento e descrição editável.';state.acabamentoLendarioAprovado=true;await write('retomada.json',state);
const appendOnce=async(file,heading,text)=>{const source=await fs.readFile(path.join(root,file),'utf8');if(!source.includes(heading))await fs.appendFile(path.join(root,file),'\n'+heading+'\n\n'+text+'\n');};
await appendOnce('AGENTS.md','## Conjurador v8','Exibir esfera pequena e discreta, centralizada logo abaixo do nome e fora da caixa de texto, dividida com os dois símbolos de Valor completos. Tipo do conjurador = ConjuradorDef.comprometimento, com espaço próprio para descrição. Não classificar o conjurador como Extra por ter dois Valores. A lendária v7 foi aprovada pelo autor.');
await appendOnce('entrega-claude.md','## Conjurador v8 — Valores e tipo','A esfera fica centralizada logo abaixo do nome, fora da caixa de texto: centro 315,116 e diâmetro 56 no viewBox 630×880. É pequena e discreta, dividida verticalmente. Cada metade contém cor e símbolo de um Valor, sem número. Usar ConjuradorDef.valores; aceita IDs em minúsculas ou nomes legíveis. ConjuradorDef.comprometimento alimenta a linha de tipo; opções verificadas: Invocador, Brigão, Parrudão da quebrada, Habilidoso, Preparado. descricaoComprometimento é uma descrição visual opcional, separada da habilidade/texto de regras. O Comprometimento não informado permanece como campo a preencher. A prévia de Ciantia usa Persona/Sublimeyn como exemplos de layout, sem afirmar identidade canônica. Para dados reais, mapear habilidade → texto e danoBasico → ataque.\n\nArquivos: conjurador-v8.json, templates/conjurador-v8.svg e overlay-conjurador-v8.svg/png (1260×1760, centro transparente); vetores/esfera-conjurador-v8.svg e valor-*-simbolo-v8.svg. Os campos textuais continuam editáveis. Editor local permite escolher o par, Comprometimento e descrição. O brilho v7 aprovado permanece. Canva não foi alterado nesta revisão.');
console.log(JSON.stringify({conjurador:'v8',tipos:types,overlay:[info.width,info.height],alfa:alpha}));
