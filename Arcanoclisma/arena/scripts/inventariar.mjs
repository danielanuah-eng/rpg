import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(import.meta.url);
const sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const chosen=new Set(['arena-jogador-1-ampla-larga-v2','arena-jogador-2-ampla-larga-v2','verso-arcano-v3','ornamento-arcano']);
const assets=[];
for(const folder of ['raster','vetores','templates']){
 for(const name of (await fs.readdir(path.join(root,folder))).sort()){
  if(!/\.(png|svg)$/.test(name))continue;
  const file=folder+'/'+name,b=await fs.readFile(path.join(root,file)),id=name.replace(/\.(png|svg)$/,'');
  const a={id,arquivo:file,bytes:b.length,sha256:crypto.createHash('sha256').update(b).digest('hex'),avaliacaoAutor:folder==='raster'?'pendente':'template solicitado pelo autor'};
  if(name.endsWith('.svg')){a.formato='SVG';a.viewBox=b.toString().match(/viewBox="([^"]+)"/)?.[1];a.escalavel=true;}
  else{
   a.formato='PNG';const m=await sharp(b).metadata();a.dimensoes=[m.width,m.height];a.hasAlpha=m.hasAlpha;
   const {data,info}=await sharp(b).ensureAlpha().raw().toBuffer({resolveWithObject:true});let zero=0,opaque=0;
   for(let i=3;i<data.length;i+=4){if(data[i]===0)zero++;if(data[i]===255)opaque++;}
   a.alfa={pixelsTransparentes:zero,pixelsOpacos:opaque,cantos:[[0,0],[info.width-1,0],[0,info.height-1],[info.width-1,info.height-1]].map(([x,y])=>data[(y*info.width+x)*4+3])};
   if(folder==='raster'){a.vigente=chosen.has(id);a.registro='registros/'+id+'.json';}
  }
  assets.push(a);
 }
}
await fs.writeFile(path.join(root,'assets.json'),JSON.stringify({versao:1,assets,nota:'Vetor/HTML editável; PNGs originais com alfa medido. Versões de verso v1/v2 preservadas e fora da composição vigente.'},null,2)+'\n');
let previousState={};try{previousState=JSON.parse(await fs.readFile(path.join(root,'retomada.json'),'utf8'));}catch(error){if(error.code!=='ENOENT')throw error;}
await fs.writeFile(path.join(root,'retomada.json'),JSON.stringify({...previousState,projeto:'arena-arcanoclisma',estado:'aguardando_avaliacao',rasterVigentes:[...chosen],geracaoConcluida:true,templateCanva:'base salva; revisão v5 e templates de itens aguardam aprovação após prévia',integracaoClaude:'pendente',arquivamentoEnzo:'adiado conforme economia durante produção',push:'não autorizado nesta entrega'},null,2)+'\n');
console.log(JSON.stringify({assets:assets.length,rasterVigentes:assets.filter(a=>a.vigente).length}));
