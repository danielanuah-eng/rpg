// Cronômetro sem tokens: espera (até N minutos) uma tarefa ou nota nova para uma IA e só então devolve o controle.
// Uso: node esperar.mjs <pasta-da-ponte> --eu codex [--minutos 10] [--intervalo 5] [--resposta arquivo.md]
// Saída (a IA só "acorda" quando o comando termina):
//   TAREFA  <arquivo>   (+ conteúdo)  -> há tarefa 'pendente' para <eu> ou 'qualquer'
//   NOTA    <arquivo>   (+ conteúdo)  -> nota nova para <eu> ou 'todos'
//   RESPOSTA <arquivo> (+ conteúdo)  -> resposta atômica explicitamente aguardada
//   SEM_TAREFA                         -> passou o tempo sem nada: a IA deve PARAR (o Henrique reativa)
//   PARAR                              -> existe o arquivo STOP na ponte
// Enquanto o script espera, nenhum modelo é chamado: custo zero de tokens.
import fs from 'node:fs';
import path from 'node:path';

const a = process.argv.slice(2);
const opt = (k, d) => { const i = a.indexOf('--' + k); return i >= 0 ? a[i + 1] : d; };
const dir = path.resolve(a[0] && !a[0].startsWith('--') ? a[0] : '.');
const eu = (opt('eu', '') || '').toLowerCase();
if (!eu) { console.error('Uso: node esperar.mjs <ponte> --eu <claude|codex|antigravity> [--minutos 10] [--intervalo 5]'); process.exit(1); }
const resposta = opt('resposta', '');
if (resposta && (path.basename(resposta) !== resposta || !resposta.endsWith('.md'))) {
  console.error('--resposta deve ser apenas o nome de um arquivo .md em respostas/.'); process.exit(1);
}
const limite = Date.now() + Number(opt('minutos', 10)) * 60000;
const passo = Math.max(1, Number(opt('intervalo', 5))) * 1000;

const ls = (d) => { try { return fs.readdirSync(path.join(dir, d)).sort(); } catch { return []; } };
const campo = (t, k) => (t.match(new RegExp(`\\*\\*${k}:\\*\\*\\s*(.+)`, 'i')) || [])[1]?.trim().toLowerCase() || '';
const aliases = (id) => id === 'antigravity' || id === 'gemini' ? ['antigravity', 'gemini'] : id === 'codex' || id === 'gpt' ? ['codex', 'gpt'] : [id];
const meu = (para) => aliases(eu).some((id) => para.includes(id)) || /qualquer|todos/.test(para);

const lidasArq = path.join(dir, 'notas', `.lidas-${eu}.txt`);
const lidas = new Set(fs.existsSync(lidasArq) ? fs.readFileSync(lidasArq, 'utf8').split('\n').filter(Boolean) : []);

function achar() {
  if (fs.existsSync(path.join(dir, 'STOP'))) return { tipo: 'PARAR' };
  if (resposta && fs.existsSync(path.join(dir, 'respostas', resposta))) {
    return { tipo: 'RESPOSTA', f: resposta, texto: fs.readFileSync(path.join(dir, 'respostas', resposta), 'utf8') };
  }
  for (const f of ls('tarefas').filter((f) => f.endsWith('.md'))) {
    const t = fs.readFileSync(path.join(dir, 'tarefas', f), 'utf8');
    if (/pendente/.test(campo(t, 'Status')) && meu(campo(t, 'Para'))) return { tipo: 'TAREFA', f, texto: t };
  }
  for (const f of ls('notas').filter((f) => f.endsWith('.md') && !lidas.has(f))) {
    const [, de, para] = f.replace(/\.md$/, '').split('_');
    if (aliases(eu).includes(de) || !para || !meu(para)) continue;
    return { tipo: 'NOTA', f, texto: fs.readFileSync(path.join(dir, 'notas', f), 'utf8') };
  }
  return null;
}

for (;;) {
  const r = achar();
  if (r) {
    if (r.tipo === 'PARAR') { console.log('PARAR'); break; }
    if (r.tipo === 'NOTA') { lidas.add(r.f); fs.writeFileSync(lidasArq, [...lidas].join('\n')); }
    console.log(`${r.tipo} ${r.f}\n=== CONTEÚDO ===\n${r.texto}\n=== FIM ===`);
    break;
  }
  if (Date.now() >= limite) { console.log('SEM_TAREFA'); break; }
  await new Promise((ok) => setTimeout(ok, passo));
}
