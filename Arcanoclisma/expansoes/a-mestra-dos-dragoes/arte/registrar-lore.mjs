import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
import { createHash } from 'node:crypto';
const sharp = createRequire(import.meta.url)('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const root = path.dirname(fileURLToPath(import.meta.url));
const input = JSON.parse(await fs.readFile(process.argv[2], 'utf8'));
const queuePath = path.join(root, 'fila-lore-2026-10-09.json');
const queue = JSON.parse(await fs.readFile(queuePath, 'utf8'));
const item = queue.itens.find(x => x.id === input.id);
if (!item) throw Error('ID desconhecido');
if (input.cena) item.cena = input.cena;
if (input.momentoDoDia) item.momentoDoDia = input.momentoDoDia;
const name = `${item.id}-${item.slug}-${input.versao}.png`;
const dest = path.join(root, 'imagens', name);
try { await fs.access(dest); } catch { await fs.copyFile(input.origem, dest); }
const bytes = await fs.readFile(dest);
const meta = await sharp(bytes).metadata();
const raw = await sharp(bytes).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
let min = 255, max = 0, transparent = 0;
for (let i = 3; i < raw.data.length; i += raw.info.channels) {
  const a = raw.data[i]; min = Math.min(min,a); max = Math.max(max,a); if(a === 0) transparent++;
}
await fs.writeFile(path.join(root, 'prompts', `${item.id}-${input.versao}.prompt.txt`), input.prompt + '\n');
const record = { ...item, estado:input.estado??'gerada_aguardando_avaliacao', arquivo:`imagens/${name}`, prompt:`prompts/${item.id}-${input.versao}.prompt.txt`, versao:input.versao, origem:input.origem, ferramenta:'image_gen integrada', dimensoes:[meta.width,meta.height], sha256:createHash('sha256').update(bytes).digest('hex'), alfa:{hasAlpha:meta.hasAlpha??false,min,max,pixelsTransparentes:transparent}, fundo:'cenário completo opaco', saturacao:'reduzida conforme pedido do autor', futuroDeValiant:'obscuro, sem desfecho revelado', aparenciasNovas:'propostas visuais aguardando aprovação', revisaoVisual:input.aprovacaoDoAutor??'inspecionada pelo Codex; aprovação do autor pendente', liberadaParaJogo:false, ambiente:input.ambiente??item.cena, clima:input.clima??'conforme imagem', escala:'personagens e objetos integrados ao ambiente', eventoId:input.eventoId??null, continuidadeCom:input.continuidadeCom??[], relacaoComEvento:input.relacaoComEvento??null, liberdadesVisuais:input.liberdadesVisuais??['composição e detalhes estéticos; sem novas regras'], pushAutorizado:input.pushAutorizado??false };
await fs.writeFile(path.join(root,'registros',`${item.id}-${input.versao}.visual.json`),JSON.stringify(record,null,2)+'\n');
item.estado = record.estado; item.arquivo = record.arquivo; item.versao = input.versao;
await fs.writeFile(queuePath,JSON.stringify(queue,null,2)+'\n');
const escape = s => String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
const cards = queue.itens.filter(x => x.arquivo).map(x => `<figure><a href="${escape(x.arquivo)}"><img src="${escape(x.arquivo)}" alt="${escape(x.nomeVisual)}"></a><figcaption>${escape(x.id)} · ${escape(x.nomeVisual)}<small>${escape(x.cena)}</small></figcaption></figure>`).join('\n');
await fs.writeFile(path.join(root,'galeria-lore-2026-10-09.html'),`<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>Ciantia — artes da história</title><style>body{margin:32px;background:#22262a;color:#eee;font:16px system-ui}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(250px,1fr));gap:24px}figure{margin:0}img{width:100%;border-radius:8px}small{display:block;color:#bdc0c7;margin-top:8px}p{color:#bdc0c7}</style><h1>A Mestra dos Dragões — cenas da história</h1><p>${queue.itens.filter(x=>x.arquivo).length}/${queue.itens.length} cenas produzidas. Saturação reduzida. Aguardando avaliação visual; conteúdo jogável bloqueado. Aparências de Gramael, Valiant e novos conjuradores são propostas.</p><main>${cards}</main></html>`);
console.log(JSON.stringify({id:item.id,arquivo:dest,dimensoes:record.dimensoes,alfa:record.alfa}));
