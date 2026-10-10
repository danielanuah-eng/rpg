import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const theme=process.argv[process.argv.indexOf('--theme')+1];
if(!['pedreira','deserto','neve','vulcao'].includes(theme)) throw new Error('Tema inválido');
const job=theme==='vulcao'?JSON.parse(await fs.readFile(path.join(root,'fontes','vulcao-job.json'),'utf8')):JSON.parse(await fs.readFile(path.join(root,'fontes','pixellab-jobs.json'),'utf8'))[theme];
const orig=path.join(root,'fontes','pixellab-originais',theme);
const output=path.join(root,'mapa',theme);
await fs.mkdir(orig,{recursive:true}); await fs.mkdir(output,{recursive:true});
for(let start=0;start<job.urls.length;start+=4){
 await Promise.all(job.urls.slice(start,start+4).map(async({index,url})=>{
  if(!url.startsWith('https://backblaze.pixellab.ai/')) throw new Error('Host inválido');
  const r=await fetch(url,{signal:AbortSignal.timeout(60000)});
  if(!r.ok) throw new Error(`${theme}:${index} HTTP ${r.status}`);
  const bytes=Buffer.from(await r.arrayBuffer());
  if(bytes.subarray(1,4).toString()!=='PNG') throw new Error('Não PNG');
  await fs.writeFile(path.join(orig,`tile_${index}.png`),bytes);
 }));
}
const selected={};const keys=['grama_a','grama_b','zona','terra','casca','folhagem','madeira','pedra'];
for(let i=0;i<keys.length;i++){
 const index=[0,1,5,6,8,10,12,14][i]; const material=`${theme}_${keys[i]}`;
 await fs.copyFile(path.join(orig,`tile_${index}.png`),path.join(output,`${material}.png`));
 selected[material]=`fontes/pixellab-originais/${theme}/tile_${index}.png`;
}
await fs.writeFile(path.join(root,'fontes',`${theme}-texturas.json`),JSON.stringify({origem:'PixelLab / Map',id:job.id,description:job.description,selecao:selected},null,2));
console.log(`TEXTURAS_OK ${theme} originais=${job.urls.length} selecionadas=8`);
