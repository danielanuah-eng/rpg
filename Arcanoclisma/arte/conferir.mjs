import fs from 'node:fs/promises';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';

const root = path.dirname(fileURLToPath(import.meta.url));
const require = createRequire(import.meta.url);
const sharp = require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const text = await fs.readFile(path.join(root, 'descricoes_das_imagens.md'), 'utf8');
let previous = new Map();
try {
  const saved = JSON.parse(await fs.readFile(path.join(root, 'progresso.json'), 'utf8'));
  previous = new Map(saved.imagens.map(entry => [entry.numero, entry]));
} catch (error) {
  if (error.code !== 'ENOENT' && !(error instanceof SyntaxError)) throw error;
}
const entries = [];
for (const match of text.matchAll(/^### (\d+) · ([^\r\n]+)\r?\n([\s\S]*?)(?=^### |$(?![\s\S]))/gm)) {
  const number = Number(match[1]);
  if (number < 87) continue;
  const id = match[3].match(/`([^`]+)`/)?.[1];
  if (!id) throw new Error(`Identificador ausente: ${number}`);
  const entry = { numero: number, id, titulo: match[2], estado: 'pendente' };
  const file = path.join(root, 'imagens', `${id}.png`);
  try {
    const stat = await fs.stat(file);
    const cached = previous.get(number);
    if (cached?.sha256 && cached.mtimeMs === stat.mtimeMs && cached.bytes === stat.size) {
      entries.push(cached);
      continue;
    }
    const buf = await fs.readFile(file);
    const meta = await sharp(buf).metadata();
    const { data, info } = await sharp(buf).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
    const alpha = (x, y) => data[(y * info.width + x) * 4 + 3];
    const corners = [alpha(0, 0), alpha(info.width - 1, 0), alpha(0, info.height - 1), alpha(info.width - 1, info.height - 1)];
    let transparentPixels = 0;
    let opaquePixels = 0;
    for (let i = 3; i < data.length; i += 4) {
      if (data[i] === 0) transparentPixels++;
      if (data[i] === 255) opaquePixels++;
    }
    // Alfa 1/255 num pixel de canto é ruído residual praticamente invisível.
    const valid = Boolean(meta.hasAlpha && corners.every(value => value <= 1) && transparentPixels > 0 && opaquePixels > 0);
    Object.assign(entry, { estado: valid ? 'gerada e alfa conferido' : 'revisar transparência', arquivo: `imagens/${id}.png`, largura: meta.width, altura: meta.height, hasAlpha: meta.hasAlpha, alfaCantos: corners, pixelsTransparentes: transparentPixels, pixelsOpacos: opaquePixels, bytes: buf.length, mtimeMs: stat.mtimeMs, sha256: createHash('sha256').update(buf).digest('hex') });
  } catch (error) {
    if (error.code !== 'ENOENT') throw error;
  }
  entries.push(entry);
}
const completed = entries.filter(entry => entry.estado === 'gerada e alfa conferido').length;
const styleReady = [];
for (const entry of entries) {
  try {
    const record = JSON.parse(await fs.readFile(path.join(root, 'registros', `${String(entry.numero).padStart(3, '0')}.json`), 'utf8'));
    if (record.estiloVersao === 2 && entry.estado === 'gerada e alfa conferido') styleReady.push(entry.numero);
  } catch (error) {
    if (error.code !== 'ENOENT') throw error;
  }
}
const result = { inicio: 87, fim: 521, total: entries.length, concluidas: completed, pendentes: entries.length - completed, fundo: 'Transparente; verificação do canal alfa e dos quatro cantos', push: 'Somente após autorização explícita do usuário', imagens: entries };
result.estiloAtual = { versao: 2, geradasComAlfaConferido: styleReady.length, pendentes: entries.length - styleReady.length, numeros: styleReady, nota: 'Versão do prompt; revisão visual é registrada separadamente.' };
const progressTemp = path.join(root, `progresso-${process.pid}.tmp`);
await fs.writeFile(progressTemp, `${JSON.stringify(result, null, 2)}\n`);
await fs.rename(progressTemp, path.join(root, 'progresso.json'));
await import('./galeria.mjs');
console.log(JSON.stringify({ total: entries.length, concluidas: completed, pendentes: result.pendentes, revisar: entries.filter(entry => entry.estado === 'revisar transparência').map(entry => entry.numero) }));
