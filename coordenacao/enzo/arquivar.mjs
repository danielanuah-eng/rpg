// Memória visual de Enzo. Só lê a pasta de arte autorizada e escreve na biblioteca.
// Uso: node arquivar.mjs <pasta-de-arte> [pasta-da-biblioteca]
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';

if (!process.argv[2]) throw new Error('Informe a pasta de arte autorizada.');
const source = path.resolve(process.argv[2]);
const library = path.resolve(process.argv[3] || path.resolve(source, '../../..', 'Biblioteca-Enzo'));
const repo = path.resolve(source, '../..');
const project = path.basename(path.dirname(source));
const key = project.normalize('NFKD').replace(/[\u0300-\u036f]/g, '').replace(/[^a-zA-Z0-9_-]/g, '-').toLowerCase();
const indexPath = path.join(library, 'indices', key + '.json');
const lockPath = path.join(library, 'indices', '.' + key + '.arquivamento.lock.json');
const assetExt = /\.(png|jpg|jpeg|webp|svg|glb|gltf|blend|fbx|obj|mp4|webm|gif|wav|ogg)$/i;
const docExt = /\.(json|md|mjs|js|css|html|txt)$/i;
const secretName = /(^|\/)(\.env(?:\..*)?|credentials[^/]*|secrets?[^/]*|api[-_]?keys?[^/]*)$/i;
const secretValue = /(?:sk-(?:or-v1-)?[A-Za-z0-9_-]{20,}|AIza[A-Za-z0-9_-]{25,}|BSA[A-Za-z0-9_-]{25,})/;
const relative = file => path.relative(source, file).replaceAll('\\', '/');
const hash = data => crypto.createHash('sha256').update(data).digest('hex');
const skipped = new Set();

await fs.mkdir(path.join(library, 'objetos'), {recursive: true});
await fs.mkdir(path.join(library, 'indices'), {recursive: true});
let lock;
try {
  lock = await fs.open(lockPath, 'wx');
} catch (error) {
  if (error.code !== 'EEXIST') throw error;
  console.log(JSON.stringify({estado: 'arquivamento_em_execucao_ou_lock_pendente', projeto: project}));
  process.exit(0);
}
try {
  await lock.writeFile(JSON.stringify({pid: process.pid, criadoEm: new Date().toISOString(), origem: source}));
  let index;
  try { index = JSON.parse(await fs.readFile(indexPath, 'utf8')); }
  catch (error) {
    if (error.code !== 'ENOENT') throw error;
    index = {versao: 1, projeto: project, origem: source, assets: [], documentos: []};
  }
  if (path.resolve(index.origem) !== source) throw new Error('Índice pertence a outra origem. Preserve-o e use outra biblioteca.');
  const before = {assets: index.assets.length, documentos: index.documentos.length};
  const json = async file => {
    try {
      const text = await fs.readFile(file, 'utf8');
      if (secretValue.test(text)) { skipped.add(relative(file)); return null; }
      return JSON.parse(text);
    } catch (error) {
      if (error.code === 'ENOENT') return null;
      throw error;
    }
  };
  const progress = await json(path.join(source, 'progresso.json')) || {};
  let catalog = '';
  try { catalog = await fs.readFile(path.join(source, 'descricoes_das_imagens.md'), 'utf8'); }
  catch (error) { if (error.code !== 'ENOENT') throw error; }
  const descriptions = new Map([...catalog.matchAll(/^### (\d+) · ([^\r\n]+)\r?\n([\s\S]*?)(?=^### |$(?![\s\S]))/gm)].map(m => [Number(m[1]), m[2] + '\n' + m[3]]));
  async function list(dir) {
    const results = [];
    for (const entry of await fs.readdir(dir, {withFileTypes: true})) {
      if (['.git', 'node_modules', '.cache'].includes(entry.name)) continue;
      const file = path.join(dir, entry.name);
      // Não seguir links para pastas/documentos fora do escopo autorizado.
      if (entry.isDirectory()) results.push(...await list(file));
      else if (entry.isFile()) results.push(file);
    }
    return results;
  }
  const files = await list(source);
  const records = new Map();
  for (const file of files) {
    if (!/\/registros\/[^/]+\.json$/i.test(relative(file))) continue;
    const record = await json(file);
    if (typeof record?.arquivo !== 'string' || !assetExt.test(record.arquivo)) continue;
    const target = path.resolve(repo, record.arquivo);
    const rel = relative(target);
    if (rel.startsWith('../') || path.isAbsolute(rel)) continue;
    records.set(target.toLowerCase(), {arquivo: relative(file), dados: record});
  }
  const existing = {
    asset: new Set(index.assets.map(i => i.origem + '\0' + i.sha256)),
    documento: new Set(index.documentos.map(i => i.origem + '\0' + i.sha256)),
  };
  async function archive(file, kind) {
    const rel = relative(file);
    if (secretName.test(rel)) { skipped.add(rel); return; }
    const data = await fs.readFile(file);
    if (kind === 'documento' && secretValue.test(data.toString('utf8'))) { skipped.add(rel); return; }
    const sha256 = hash(data);
    if (existing[kind].has(rel + '\0' + sha256)) return;
    const object = 'objetos/' + sha256 + path.extname(file).toLowerCase();
    try { await fs.writeFile(path.join(library, object), data, {flag: 'wx'}); }
    catch (error) { if (error.code !== 'EEXIST') throw error; }
    const item = {origem: rel, objeto: object, sha256, bytes: data.length, arquivadoEm: new Date().toISOString(), criador: 'não informado'};
    if (kind === 'asset') {
      const info = (progress.imagens || []).find(i => i.arquivo === rel);
      if (info) {
        item.numero = info.numero; item.id = info.id; item.titulo = info.titulo;
        item.descricao = descriptions.get(info.numero) || null;
        item.verificacaoNaOrigem = {estado: info.estado, largura: info.largura, altura: info.altura, hasAlpha: info.hasAlpha, sha256: info.sha256};
        const file = path.join(source, 'registros', String(info.numero).padStart(3, '0') + '.json');
        const data = await json(file);
        if (data) { item.registro = data; item.arquivoRegistro = relative(file); }
      }
      const record = records.get(file.toLowerCase());
      if (record && (!record.dados.sha256 || record.dados.sha256 === sha256)) {
        item.registro = record.dados;
        item.arquivoRegistro = record.arquivo;
        item.id = record.dados.id || item.id;
        item.aprovacao = record.dados.aprovacaoAutor || record.dados.estado || null;
      }
      if (item.registro?.ferramenta?.includes('image_gen')) item.criador = 'Codex / image_gen, conforme registro';
      item.aprovacao ??= 'Consultar decisões do autor; geração não implica aprovação.';
    }
    (kind === 'asset' ? index.assets : index.documentos).push(item);
    existing[kind].add(rel + '\0' + sha256);
  }
  for (const file of files) {
    if (assetExt.test(file)) await archive(file, 'asset');
    else if (docExt.test(file)) await archive(file, 'documento');
  }
  const definitions = [];
  for (const agent of ['carlos', 'enzo']) {
    const origin = path.join(repo, '.agents/skills', agent, 'SKILL.md');
    const target = path.join(library, 'definicoes', agent, 'SKILL.md');
    const data = await fs.readFile(origin);
    if (secretValue.test(data.toString('utf8'))) throw new Error('Definição contém dado confidencial; cópia interrompida.');
    let previous = null;
    try { previous = await fs.readFile(target); }
    catch (error) { if (error.code !== 'ENOENT') throw error; }
    if (!previous || hash(previous) !== hash(data)) {
      await fs.mkdir(path.dirname(target), {recursive: true});
      await fs.writeFile(target, data);
      definitions.push(agent);
    }
  }
  index.atualizadoEm = new Date().toISOString();
  const temporary = indexPath + '.' + process.pid + '.tmp';
  await fs.writeFile(temporary, JSON.stringify(index, null, 2) + '\n');
  await fs.rename(temporary, indexPath);
  console.log(JSON.stringify({projeto: project, novosAssets: index.assets.length - before.assets, novosDocumentos: index.documentos.length - before.documentos,
    versoesDeAssets: index.assets.length, documentos: index.documentos.length, definicoesAtualizadas: definitions,
    arquivosIgnoradosPorConfidencialidade: [...skipped], indice: indexPath}));
} finally {
  await lock.close();
  await fs.unlink(lockPath);
}
