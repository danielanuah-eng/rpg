import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import crypto from 'node:crypto';
const dest=path.join(path.dirname(fileURLToPath(import.meta.url)),'base-cc0');
await fs.mkdir(dest,{recursive:true});
const root='https://raw.githubusercontent.com/makehumancommunity/makehuman/master/';
const files=['LICENSE.md','LICENSE.ASSETS.md','makehuman/data/3dobjs/base.obj',
 'makehuman/data/targets/macrodetails/caucasian-female-young.target',
 'makehuman/data/targets/macrodetails/african-female-young.target',
 'makehuman/data/targets/macrodetails/asian-female-young.target'];
const provenance=[];
for(const rel of files){
 const r=await fetch(root+rel);if(!r.ok)throw new Error(rel+': '+r.status);
 const b=Buffer.from(await r.arrayBuffer());const name=path.basename(rel);
 await fs.writeFile(path.join(dest,name),b);
 provenance.push({name,url:root+rel,bytes:b.length,sha256:crypto.createHash('sha256').update(b).digest('hex')});
 console.log(name,b.length);
}
await fs.writeFile(path.join(dest,'origem.json'),JSON.stringify({license:'CC0-1.0 (assets only)',author:'MakeHuman Team / MakeHuman Community',files:provenance},null,2));
