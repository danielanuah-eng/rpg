import { spawnSync } from 'node:child_process';
import { mkdirSync, readFileSync, existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
const root = fileURLToPath(new URL('../', import.meta.url));
const targets = {
  pack: ['Windows Desktop', 'build/phase10.pck'],
  windows: ['Windows Desktop', 'build/windows/ArenaTatica.exe'],
  web: ['Web', 'build/web/index.html'],
  android: ['Android', 'build/android/ArenaTatica.apk'],
};
const target = process.argv[2] || 'pack';
if (!(target in targets)) throw new Error('Alvo: pack, windows, web ou android.');
const [preset, output] = targets[target];
mkdirSync(root + output.slice(0, output.lastIndexOf('/')), { recursive: true });
mkdirSync(root + '.artifacts/phase10', { recursive: true });
const log = root + `.artifacts/phase10/export-${target}.log`;
const result = spawnSync(root + '.tools/godot/Godot_v4.6.1-stable_win64_console.exe', [
  '--headless', '--path', root, '--log-file', log,
  target === 'pack' ? '--export-pack' : '--export-release', preset, root + output,
], { windowsHide: true, stdio: 'inherit', timeout: 180000 });
const contents = existsSync(log) ? readFileSync(log, 'utf8') : '';
const ok = result.status === 0 && !/SCRIPT ERROR|^ERROR:/m.test(contents) && existsSync(root + output);
if (!ok) console.error(`Exportação não validada. Consulte ${log}. Verifique templates 4.6.1 e SDK/assinatura Android.`);
else console.log(`Exportação concluída: ${root + output}`);
process.exitCode = ok ? 0 : 1;
