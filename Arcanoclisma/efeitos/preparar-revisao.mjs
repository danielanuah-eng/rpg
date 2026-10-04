// Leitura local das artes aprovadas para orientar efeitos; não altera imagens.
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
const require = createRequire(new URL('../online/package.json', import.meta.url));
const sharp = require('sharp');

const repo = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const pedido = JSON.parse(await fs.readFile(path.join(repo, 'Arcanoclisma/efeitos/pedidos/mesa-efeitos-v1.claude.json'), 'utf8'));
const artes = JSON.parse(await fs.readFile(path.join(repo, 'Arcanoclisma/cartas/entregas/lote-site-v1.visual.json'), 'utf8')).cartas;
const mapa = new Map(artes.map(a => [a.id, a]));
const dir = path.join(repo, 'Arcanoclisma/efeitos/previas/revisao-artes');
await fs.mkdir(dir, { recursive: true });
const escape = s => String(s).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
const referencias = [];
for (let inicio = 0; inicio < pedido.resolucao.length; inicio += 24) {
  const itens = pedido.resolucao.slice(inicio, inicio + 24), layers = [];
  for (let n = 0; n < itens.length; n++) {
    const p = itens[n], arte = mapa.get(p.id);
    if (!arte) throw new Error('Arte aprovada ausente: ' + p.id);
    const registro = JSON.parse(await fs.readFile(path.join(repo, `Arcanoclisma/arte/cartas-site/registros/${p.id}.visual.json`), 'utf8'));
    const imagem = path.join(repo, arte.arquivo);
    const thumb = await sharp(imagem).resize(170, 230, { fit: 'cover' }).png().toBuffer();
    const x = n % 6 * 190, y = Math.floor(n / 6) * 278;
    layers.push({ input: thumb, left: x + 10, top: y });
    const title = Buffer.from(`<svg width="190" height="48"><rect width="190" height="48" fill="#142038"/><text x="8" y="15" font-size="11" fill="white" font-family="Arial">${escape(String(inicio + n + 1) + ' · ' + p.nome.slice(0, 26))}</text><text x="8" y="32" font-size="9" fill="#c4d2ee" font-family="Arial">${escape(p.id.slice(0, 34))}</text></svg>`);
    layers.push({ input: title, left: x, top: y + 230 });
    referencias.push({ ...p, arte: arte.arquivo, decisoesDaArte: registro.decisoes || [], referenciasDaArte: registro.referenciasGeracao || [] });
  }
  const arquivo = path.join(dir, `artes-${String(inicio + 1).padStart(3, '0')}-${Math.min(inicio + 24, pedido.resolucao.length)}.png`);
  await sharp({ create: { width: 1140, height: Math.ceil(itens.length / 6) * 278, channels: 3, background: '#0c1425' } }).composite(layers).png().toFile(arquivo);
  console.log(path.relative(repo, arquivo));
}
await fs.writeFile(path.join(dir, 'referencias.json'), JSON.stringify(referencias, null, 2) + '\n');
console.log(`${referencias.length} artes e descrições locais preparadas.`);
