import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import crypto from 'node:crypto';

const cards = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repo = path.resolve(cards, '../..');
const input = process.argv[2];
if (!input) throw new Error('Informe pedidos/<id>.claude.json; o plano não cria uma demanda.');
const file = path.resolve(input);
const relative = path.relative(path.join(cards, 'pedidos'), file);
if (relative.startsWith('..') || path.isAbsolute(relative) || !relative.endsWith('.claude.json')) throw new Error('Usar um pedido da pasta pedidos do projeto.');
const raw = await fs.readFile(file);
const request = JSON.parse(raw.toString('utf8').replace(/^\uFEFF/, ''));
const missing = [];
const text = value => typeof value === 'string' && value.trim().length > 0;
if (request.autor !== 'Claude' || request.estado !== 'solicitado' || request.necessidadeAtual !== true) missing.push('pedido atual solicitado pelo Claude');
if (!text(request.tarefaId) || !/^[a-zA-Z0-9_-]+$/.test(request.tarefaId || '')) missing.push('tarefaId válido');
if (!Number.isInteger(request.versao) || request.versao < 1) missing.push('versão do pedido');
if (!text(request.atualizadoEm) || !Number.isFinite(Date.parse(request.atualizadoEm))) missing.push('data de atualização');
if (!text(request.objetivoAtual)) missing.push('objetivo atual do Claude');
if (!Array.isArray(request.referenciasClaude) || request.referenciasClaude.length === 0) missing.push('referências de carta/código do Claude');
for (const field of ['id', 'nome', 'identidade', 'tipo', 'uso']) if (!text(request.carta?.[field])) missing.push(`carta.${field}`);
if (!request.carta?.textos || typeof request.carta.textos !== 'object' || Array.isArray(request.carta.textos) || Object.keys(request.carta.textos).length === 0) missing.push('textos exatos ou ausência justificada pelo Claude');
if (request.carta?.personagens != null && !Array.isArray(request.carta.personagens)) missing.push('personagens deve ser lista quando fornecida');
const rarity=request.carta?.cosmetico?.raridade||request.carta?.raridade;
const alternative=request.carta?.cosmetico?.arteAlternativa??request.carta?.arteAlternativa;
if (!['comum','incomum','rara','lendaria'].includes(rarity)) missing.push('raridade cosmética explícita: comum, incomum, rara ou lendaria');
if (typeof alternative !== 'boolean') missing.push('arteAlternativa explícita: true ou false');
if (missing.length) { console.log(JSON.stringify({ estado: 'aguardando_claude', pendencias: missing })); process.exitCode = 1; }
else {
  const out = path.join(cards, 'planos', `${request.tarefaId}.visual.json`);
  await fs.mkdir(path.dirname(out), { recursive: true });
  const plan = { tarefaId: request.tarefaId, cartaId: request.carta.id, pedidoVersao: request.versao, pedido: path.relative(repo, file).replaceAll('\\', '/'), pedidoSha256: crypto.createHash('sha256').update(raw).digest('hex'), estado: 'planejado', agente: 'Designer de Cartas', etapas: ['conferir demanda atual com Claude', 'consultar artes e templates aprovados', 'criar ou combinar template Canva necessário', 'criar cena/derivação pedida', 'diagramar textos exatos', 'entregar arquivos e manifesto ao Claude', 'aguardar confirmação de integração e avaliação do autor'], executaFerramentas: false, nota: 'Reler pedido e comparar versão/hash antes de produzir. Plano não é execução nem confirmação de contato externo.' };
  plan.acabamento={versao:9,raridade:rarity,arteAlternativa:alternative,apenasEstetica:true,padraoBrilhoObrigatorio:rarity==='lendaria'||alternative,diretriz:'Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md',etapa:'Incluir a diretriz de iluminação, brilho e textura no prompt completo antes de gerar.'};
  await fs.writeFile(out, JSON.stringify(plan, null, 2) + '\n');
  console.log(JSON.stringify({ estado: plan.estado, plano: out }));
}
