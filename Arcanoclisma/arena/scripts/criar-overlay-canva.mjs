import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const require=createRequire(import.meta.url);
const sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
// Overlay com janela totalmente transparente; colar a cena e definir como fundo no Canva.
const s=`<svg xmlns="http://www.w3.org/2000/svg" width="1260" height="1760" viewBox="0 0 1260 1760"><rect x="4" y="4" width="1252" height="1752" rx="24" fill="none" stroke="#162333" stroke-width="8"/><path d="M8 8h1244v172H8Z" fill="#142837" fill-opacity=".72"/><rect x="40" y="1380" width="1180" height="330" rx="12" fill="#f0ebde" fill-opacity=".83"/><circle cx="104" cy="102" r="68" fill="#638697" stroke="#182735" stroke-width="4"/><circle cx="104" cy="264" r="62" fill="#182735" stroke="#b3c5cd" stroke-width="4"/><path d="M104 380 166 442 104 504 42 442Z" fill="#182735" stroke="#b3c5cd" stroke-width="4"/></svg>`;
const final=s.replace('</svg>','<path d="M920 1238 978 1269 978 1331 920 1362 862 1331 862 1269Z" fill="#385a52" stroke="#c4d2c4" stroke-width="3"/><path d="M1052 1238h116v73q0 31-58 51-58-20-58-51Z" fill="#713f4c" stroke="#dabbbc" stroke-width="3"/></svg>');
await fs.writeFile(path.join(root,'templates','overlay-canva.svg'),final);
await sharp(Buffer.from(final)).png().toFile(path.join(root,'templates','overlay-canva.png'));
console.log('Overlay transparente criado em 1260 × 1760.');
