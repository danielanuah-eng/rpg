import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
import crypto from 'node:crypto';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const require = createRequire(import.meta.url);
const sharp = require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const [id, origin] = process.argv.slice(2);
if (!/^[a-z0-9-]+$/.test(id || '') || !origin) throw new Error('Uso: node salvar-raster.mjs <id> <origem.png>');
const destination = path.join(root, 'raster', id + '.png');
try { await fs.access(destination); throw new Error('Asset já existe; preservar e versionar antes de revisar.'); }
catch (error) { if (error.code !== 'ENOENT') throw error; }
const buffer = await fs.readFile(origin);
const metadata = await sharp(buffer).metadata();
const { data, info } = await sharp(buffer).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
let transparent = 0, opaque = 0;
for (let n = 3; n < data.length; n += 4) { if (data[n] === 0) transparent++; if (data[n] === 255) opaque++; }
const at = (x,y) => data[(y * info.width + x) * 4 + 3];
const corners = [at(0,0), at(info.width-1,0), at(0,info.height-1), at(info.width-1,info.height-1)];
if (!metadata.hasAlpha || transparent === 0 || opaque === 0 || corners.some(x=>x>1)) throw new Error('Conferir transparência antes de entregar.');
await fs.copyFile(origin, destination);
const prompt = await fs.readFile(path.join(root, 'registros', id + '-prompt.txt'), 'utf8');
const record = { id, arquivo: `raster/${id}.png`, origem: origin, ferramenta: 'image_gen integrada', prompt, criadoEm: new Date().toISOString(), largura: metadata.width, altura: metadata.height, hasAlpha: metadata.hasAlpha, alfaCantos: corners, pixelsTransparentes: transparent, pixelsOpacos: opaque, sha256: crypto.createHash('sha256').update(buffer).digest('hex'), avaliacaoAutor: 'pendente' };
await fs.writeFile(path.join(root, 'registros', id + '.json'), JSON.stringify(record,null,2)+'\n');
console.log(JSON.stringify({ id, arquivo: destination, largura:record.largura, altura:record.altura, alfaCantos:corners, pixelsTransparentes:transparent }));
