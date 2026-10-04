import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
const arcano=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const sharp=createRequire(path.join(arcano,'online/package.json'))('sharp');
const pedido=JSON.parse(await fs.readFile(path.join(arcano,'online/pedidos-visuais/iconografia.claude.json'),'utf8'));
const pasta=path.join(arcano,'online/public/kit/icones');
const manifesto=JSON.parse(await fs.readFile(path.join(pasta,'manifesto.json'),'utf8'));
assert.deepEqual(Object.keys(manifesto).sort(),pedido.icones.map(i=>i.id).sort());
const hashes=new Set(),medicoes=[];
for(const i of pedido.icones){
  assert.equal(manifesto[i.id].okGpt,true);assert.equal(manifesto[i.id].arquivo,`/kit/icones/${i.id}.svg`);
  const svg=await fs.readFile(path.join(pasta,i.id+'.svg'));
  assert.equal(Buffer.compare(svg,await fs.readFile(path.join(arcano,'arena/icones',i.id+'.svg'))),0);
  assert.match(svg.toString(),/viewBox="0 0 64 64"/);assert.match(svg.toString(),/stroke="currentColor"/);
  assert.doesNotMatch(svg.toString(),/<(?:text|script|foreignObject|image)\b/i);
  const hash=createHash('sha256').update(svg).digest('hex');assert.ok(!hashes.has(hash));hashes.add(hash);
  const tamanhos=[];
  for(const px of [64,20,16]){
    const {data,info}=await sharp(svg).resize(px,px).ensureAlpha().raw().toBuffer({resolveWithObject:true});
    let transparentes=0,visiveis=0,max=0;
    for(let k=3;k<data.length;k+=info.channels){if(!data[k])transparentes++;else visiveis++;max=Math.max(max,data[k]);}
    assert.equal(info.channels,4);assert.ok(transparentes>px*px*.3);assert.ok(visiveis>0);assert.ok(max>128);
    tamanhos.push({px,alfaMin:0,alfaMax:max,transparentes,visiveis,total:px*px});
  }
  medicoes.push({id:i.id,sha256:hash,alfaRasterizado:tamanhos});
}
const destino=path.join(arcano,'../coordenacao/site-online/evidencias-v19');await fs.mkdir(destino,{recursive:true});
await fs.writeFile(path.join(destino,'alfa-31-icones.json'),JSON.stringify({total:medicoes.length,verificado:'SVG nativo rasterizado a 64, 20 e 16 px; nenhum PNG é necessário para a entrega',medicoes},null,2)+'\n');
console.log(`${medicoes.length} ícones aprovados: contrato, origem/cópia, hashes distintos e alfa em três tamanhos.`);
