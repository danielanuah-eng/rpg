import { spawn } from 'node:child_process';
import { mkdirSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
const root = fileURLToPath(new URL('../', import.meta.url));
const exe = root + '.tools/godot/Godot_v4.6.1-stable_win64_console.exe';
const phase = process.argv.includes('--phase10') ? 'phase10' : 'phase9';
mkdirSync(root + `.artifacts/${phase}`, { recursive: true });
async function run(script, role = '') {
  return new Promise(resolve => {
    const args = ['--headless', '--path', root, '--log-file', root + `.artifacts/${phase}/engine-${script}-${role || 'local'}.log`, '--script', `res://tests/${script}.gd`];
    if (role) args.push('--', `--${role}`);
    const child = spawn(exe, args, { windowsHide: true });
    let output = '';
    child.stdout.on('data', b => output += b);
    child.stderr.on('data', b => output += b);
    const timeout = setTimeout(() => child.kill(), 45000);
    child.on('exit', code => {
      clearTimeout(timeout);
      writeFileSync(root + `.artifacts/${phase}/${script}-${role || 'local'}.log`, output);
      const failed = code !== 0 || /SCRIPT ERROR|^ERROR:|leaked|still in use at exit/m.test(output);
      console.log(`${script} ${role}: ${failed ? 'FAIL' : 'PASS'} (exit ${code})`);
      console.log(output.split(/\r?\n/).filter(s => /verifica|falha|FAIL|ERROR|PASS|OK/.test(s)).join('\n'));
      resolve(!failed);
    });
  });
}
let passed = true;
for (const script of (process.argv.includes('--network-only') ? [] : ['run_tests', 'test_movement', 'test_turns', 'test_combat', 'test_phase8'])) {
  passed = await run(script) && passed;
}
for (const script of ['test_phase8_network', 'test_phase9_network', ...(phase === 'phase10' ? ['test_phase10_network'] : [])]) {
  const results = await Promise.all([run(script, 'host'), run(script, 'client')]);
  passed = results.every(Boolean) && passed;
}
process.exitCode = passed ? 0 : 1;
