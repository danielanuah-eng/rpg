// Lista tarefas abertas e notas recentes de uma ponte. Uso: node estado.mjs [pasta-da-ponte] [--para codex]
import fs from 'node:fs';
import path from 'node:path';

const a = process.argv.slice(2);
const i = a.indexOf('--para');
const para = i >= 0 ? a.splice(i, 2)[1] : null;
const dir = path.resolve(a[0] || '.');
const ls = (d) => { try { return fs.readdirSync(path.join(dir, d)).sort(); } catch { return []; } };
const campo = (t, k) => (t.match(new RegExp(`\\*\\*${k}:\\*\\*\\s*(.+)`, 'i')) || [])[1]?.trim() || '?';

const aliases = (id) => id === 'antigravity' || id === 'gemini' ? ['antigravity', 'gemini'] : id === 'codex' || id === 'gpt' ? ['codex', 'gpt'] : [id];
const matchPara = (pr, alvo) => alvo ? aliases(alvo.toLowerCase()).some((id) => pr.toLowerCase().includes(id)) || /qualquer|todos/i.test(pr) : true;

console.log(`Ponte: ${dir}`);
for (const f of ls('tarefas').filter((f) => f.endsWith('.md'))) {
  const t = fs.readFileSync(path.join(dir, 'tarefas', f), 'utf8');
  const st = campo(t, 'Status');
  const pr = campo(t, 'Para');
  if (/conclu/i.test(st)) continue;
  if (!matchPara(pr, para)) continue;
  console.log(`[${st}] ${f}  de ${campo(t, 'De')} para ${pr}`);
}
const resp = ls('respostas').filter((f) => f.endsWith('.md'));
console.log(`Respostas: ${resp.length ? resp.join(', ') : 'nenhuma'}`);
const notas = ls('notas').filter((f) => f.endsWith('.md')).slice(-8);
console.log(`Notas recentes:${notas.length ? '\n  ' + notas.join('\n  ') : ' nenhuma'}`);
