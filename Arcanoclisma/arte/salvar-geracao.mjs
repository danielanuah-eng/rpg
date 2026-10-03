// Registro e checkpoint essenciais após cada geração. Não arquiva no Enzo.
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';
const root = path.dirname(fileURLToPath(import.meta.url));
const readJSON = async file => JSON.parse((await fs.readFile(file, 'utf8')).replace(/^\uFEFF/, ''));
const input = await readJSON(process.argv[2]);
if (input.erro) {
  const resume = await readJSON(path.join(root, 'retomada.json'));
  resume.estado = input.cota ? 'bloqueado_por_cota_de_imagens' : 'erro_de_geracao';
  resume.erroAtual = input.erro;
  resume.proximaImagem = input.numero;
  if (input.resetsAt) {
    resume.resetsAt = input.resetsAt;
    resume.liberacaoPrevistaFortaleza = new Date(input.resetsAt * 1000).toLocaleString('pt-BR', { timeZone: 'America/Fortaleza' });
  }
  await fs.writeFile(path.join(root, 'retomada.json'), JSON.stringify(resume, null, 2));
  await fs.writeFile(path.join(root, 'registros', `${input.numero}-ultima-tentativa.json`), JSON.stringify(input, null, 2));
  console.log(JSON.stringify({ estado: resume.estado, proxima: input.numero, resetsAt: resume.resetsAt }));
} else {
  if (!/^[a-z0-9-]+$/.test(input.id)) throw new Error('Identificador inválido.');
  const target = path.join(root, 'imagens', input.id + '.png');
  await fs.copyFile(input.origem, target, fs.constants.COPYFILE_EXCL);
  const record = { ...input, number: input.numero, arquivo: `imagens/${input.id}.png`, ferramenta: 'image_gen integrada', estiloVersao: 2, geradaEm: new Date().toISOString() };
  await fs.writeFile(path.join(root, 'registros', `${String(input.numero).padStart(3, '0')}.json`), JSON.stringify(record, null, 2));
  const report = execFileSync(process.execPath, [path.join(root, 'conferir.mjs')], { encoding: 'utf8' });
  const progress = await readJSON(path.join(root, 'progresso.json'));
  const pending = progress.imagens.filter(i => i.estado !== 'gerada e alfa conferido').map(i => i.numero);
  const resume = await readJSON(path.join(root, 'retomada.json'));
  Object.assign(resume, { estado: pending.length ? 'em_producao' : 'aguardando_avaliacao', salvasComAlfaValido: progress.concluidas, pendentes: pending.length, proximaImagem: pending[0] ?? null, ordemDeRetomada: pending, atualizadaEm: new Date().toISOString(), arquivamentoEnzo: 'adiado a pedido do autor' });
  delete resume.erroAtual;
  await fs.writeFile(path.join(root, 'retomada.json'), JSON.stringify(resume, null, 2));
  console.log(report.trim());
  if (progress.imagens.find(i => i.numero === input.numero)?.estado !== 'gerada e alfa conferido') throw new Error('Imagem salva precisa de revisão de transparência.');
}
