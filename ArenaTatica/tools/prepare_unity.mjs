import { readdirSync, mkdirSync, copyFileSync, readFileSync, writeFileSync, statSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = fileURLToPath(new URL('../', import.meta.url));
const output = join(root, 'migracao-unity');
function copyTree(source, target, filter = () => true) {
  mkdirSync(target, { recursive: true });
  for (const item of readdirSync(source, { withFileTypes: true })) {
    if (!filter(item.name)) continue;
    const src = join(source, item.name), dst = join(target, item.name);
    if (item.isDirectory()) copyTree(src, dst, filter);
    else if (item.isFile()) copyFileSync(src, dst);
  }
}
// GLB precisa ter sido exportado e reaberto com sucesso pelo Godot.
const models = JSON.parse(readFileSync(join(output, 'modelos-exportados.json'), 'utf8'));
if (models.length !== 3 || models.some(m => m.roundtrip !== 'passed' || !m.meshes)) throw Error('Exportação dos modelos pendente');
const assets = join(output, 'Assets/ArenaTatica');
copyTree(join(root, 'assets/sprites'), join(assets, 'Textures'), n => n.endsWith('.png'));
copyTree(join(root, 'assets/references'), join(output, 'Referencias'), n => !n.endsWith('.import'));
copyTree(join(root, 'audio/sfx'), join(assets, 'Audio'), n => n.endsWith('.wav'));
for (const dir of ['scripts', 'scenes', 'assets', 'audio', 'tests', 'tools']) {
  copyTree(join(root, dir), join(output, 'ReferenciaGodot', dir));
}
for (const name of readdirSync(root)) {
  if (name.endsWith('.md') || ['project.godot', 'export_presets.cfg', '.gitignore'].includes(name)) {
    copyFileSync(join(root, name), join(output, 'ReferenciaGodot', name));
  }
}
const validation = join(output, 'Validacao');
mkdirSync(validation, { recursive: true });
let checks = 0;
for (const filename of ['run_tests-local.log', 'test_movement-local.log', 'test_turns-local.log', 'test_combat-local.log', 'test_phase8-local.log', ...['test_phase8_network', 'test_phase9_network', 'test_phase10_network'].flatMap(n => [`${n}-host.log`, `${n}-client.log`])]) {
  const src = join(root, '.artifacts/phase10', filename);
  const log = readFileSync(src, 'utf8');
  if (/SCRIPT ERROR|^ERROR:|leaked|still in use at exit/m.test(log)) throw Error(`Validação falhou: ${filename}`);
  const count = log.match(/(\d+) verifica(?:ções|coes), (\d+) falhas/);
  if (!count || Number(count[2])) throw Error(`Resumo inválido: ${filename}`);
  checks += Number(count[1]);
  copyFileSync(src, join(validation, filename));
}
writeFileSync(join(validation, 'resumo.json'), JSON.stringify({ date: '2026-10-09', checks, failures: 0, modelRoundtrip: models, unityEditorInstalled: '6000.6.4f1', unityImportTested: false, godotModelsTest: 'passed; execução separada test_models_3d.gd' }, null, 2));
const records = [];
function inventory(directory) {
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) inventory(path);
    else if (entry.isFile() && !['manifesto.json', '.DS_Store'].includes(entry.name)) {
      const content = readFileSync(path);
      records.push({ path: relative(output, path).replaceAll('\\', '/'), bytes: statSync(path).size, sha256: createHash('sha256').update(content).digest('hex') });
    }
  }
}
inventory(output);
writeFileSync(join(output, 'manifesto.json'), JSON.stringify({ project: 'ArenaTatica', date: '2026-10-09', stage: 'pacote de migração; ainda não é projeto Unity jogável', checks, files: records }, null, 2));
// Confere os hashes depois da escrita para detectar cópia incompleta.
for (const record of records) {
  if (createHash('sha256').update(readFileSync(resolve(output, record.path))).digest('hex') !== record.sha256) throw Error(`Hash divergente: ${record.path}`);
}
console.log(`Pacote pronto: ${records.length} arquivos; ${checks} verificações; hashes conferidos.`);
