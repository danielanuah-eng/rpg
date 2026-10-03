import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const cards = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repo = path.resolve(cards, '../..');
const art = path.resolve(cards, '../arte');
const read = async file => JSON.parse((await fs.readFile(file, 'utf8')).replace(/^\uFEFF/, ''));
const progress = await read(path.join(art, 'progresso.json'));
const assets = [];
for (const entry of progress.imagens) {
  const record = await read(path.join(art, 'registros', `${String(entry.numero).padStart(3, '0')}.json`));
  if (record.aprovacaoAutor?.estado !== 'aprovada' || entry.estado !== 'gerada e alfa conferido') continue;
  const file = path.join(art, entry.arquivo);
  await fs.access(file);
  assets.push({ numero: entry.numero, id: entry.id, titulo: entry.titulo, arquivo: path.relative(repo, file).replaceAll('\\', '/'), registro: `Arcanoclisma/arte/registros/${String(entry.numero).padStart(3, '0')}.json`, sha256: entry.sha256, largura: entry.largura, altura: entry.altura, hasAlpha: entry.hasAlpha, aprovacao: record.aprovacaoAutor, finalidade: 'referência aprovada; derivar somente para pedidos atuais do Claude' });
}
await fs.writeFile(path.join(cards, 'catalogo-artes.json'), JSON.stringify({ versao: 1, projeto: 'Arcanoclisma TCG', agente: 'Designer de Cartas', atualizadoEm: new Date().toISOString(), total: assets.length, assets }, null, 2) + '\n');
console.log(JSON.stringify({ artesAprovadas: assets.length, catalogo: path.join(cards, 'catalogo-artes.json') }));
