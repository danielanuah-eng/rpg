import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';

const library = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const source = path.resolve(process.argv[2] || '');
if (!process.argv[2]) throw new Error('Informe a pasta de arte do projeto.');
const project = path.basename(path.dirname(source));
const key = project.normalize('NFKD').replace(/[\u0300-\u036f]/g, '').replace(/[^a-zA-Z0-9_-]/g, '-').toLowerCase();
await fs.mkdir(path.join(library, 'objetos'), { recursive: true });
await fs.mkdir(path.join(library, 'indices'), { recursive: true });
const indexPath = path.join(library, 'indices', `${key}.json`);
let index;
try { index = JSON.parse(await fs.readFile(indexPath, 'utf8')); }
catch (e) { if (e.code !== 'ENOENT') throw e; index = { versao: 1, projeto: project, origem: source, assets: [], documentos: [] }; }
if (index.origem !== source) throw new Error('Nome de projeto repetido com outra origem; use um índice separado.');
let progress = { imagens: [] };
try { progress = JSON.parse(await fs.readFile(path.join(source, 'progresso.json'), 'utf8')); }
catch (e) { if (e.code !== 'ENOENT') throw e; }
let catalog = '';
try { catalog = await fs.readFile(path.join(source, 'descricoes_das_imagens.md'), 'utf8'); }
catch (e) { if (e.code !== 'ENOENT') throw e; }
const descriptions = new Map([...catalog.matchAll(/^### (\d+) · ([^\r\n]+)\r?\n([\s\S]*?)(?=^### |$(?![\s\S]))/gm)].map(m => [Number(m[1]), m[2] + '\n' + m[3]]));
async function list(dir) {
  let entries;
  try { entries = await fs.readdir(dir, { withFileTypes: true }); }
  catch (e) { if (e.code === 'ENOENT') return []; throw e; }
  const results = [];
  for (const entry of entries) {
    const file = path.join(dir, entry.name);
    if (entry.isDirectory()) results.push(...await list(file));
    else if (entry.isFile()) results.push(file);
  }
  return results;
}
async function archive(file, kind) {
  const data = await fs.readFile(file);
  const sha256 = crypto.createHash('sha256').update(data).digest('hex');
  const object = `objetos/${sha256}${path.extname(file).toLowerCase()}`;
  try { await fs.writeFile(path.join(library, object), data, { flag: 'wx' }); }
  catch (e) { if (e.code !== 'EEXIST') throw e; }
  const relative = path.relative(source, file).replaceAll('\\', '/');
  const items = kind === 'asset' ? index.assets : index.documentos;
  if (items.some(item => item.origem === relative && item.sha256 === sha256)) return;
  const item = { origem: relative, objeto: object, sha256, bytes: data.length, arquivadoEm: new Date().toISOString(), criador: 'não informado' };
  if (kind === 'asset') {
    const info = progress.imagens.find(i => i.arquivo === relative);
    if (info) {
      item.numero = info.numero; item.id = info.id; item.titulo = info.titulo;
      item.descricao = descriptions.get(info.numero) || null;
      item.verificacaoNaOrigem = { estado: info.estado, largura: info.largura, altura: info.altura, hasAlpha: info.hasAlpha, sha256: info.sha256 };
      try { item.registro = JSON.parse(await fs.readFile(path.join(source, 'registros', `${String(info.numero).padStart(3, '0')}.json`), 'utf8')); }
      catch (e) { if (e.code !== 'ENOENT') throw e; }
      if (item.registro?.ferramenta?.includes('image_gen')) item.criador = 'Codex / image_gen, conforme registro';
    }
    item.aprovacao = 'Consultar decisões do autor; geração não implica aprovação.';
  }
  items.push(item);
}
for (const dir of ['imagens', 'versoes-anteriores']) {
  for (const file of await list(path.join(source, dir))) {
    if (/\.(png|jpg|jpeg|webp|svg|glb|gltf|blend|fbx|obj)$/i.test(file)) await archive(file, 'asset');
  }
}
for (const dir of ['registros', 'coordenacao-movimentos']) {
  for (const file of await list(path.join(source, dir))) {
    if (/\.(json|md)$/i.test(file)) await archive(file, 'documento');
  }
}
for (const entry of await fs.readdir(source, { withFileTypes: true })) {
  if (entry.isFile() && /\.(md|json|mjs)$/i.test(entry.name)) await archive(path.join(source, entry.name), 'documento');
}
index.atualizadoEm = new Date().toISOString();
const temporary = indexPath + '.tmp';
await fs.writeFile(temporary, JSON.stringify(index, null, 2));
await fs.rename(temporary, indexPath);
console.log(JSON.stringify({ projeto: project, versoesDeAssets: index.assets.length, documentos: index.documentos.length, indice: indexPath }));
