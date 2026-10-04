// Família vetorial das mecânicas solicitadas: geometria explícita, sem números/letras visíveis.
// Só arquivos de arte e manifestos. Não modifica o catálogo, o motor ou os textos.
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createHash } from 'node:crypto';
const arcano = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const pedido = JSON.parse(await fs.readFile(path.join(arcano,'online/pedidos-visuais/iconografia.claude.json'),'utf8'));
const arte = {
  'requisito-grau': '<path d="M25 5 45 29 25 53 5 29Z"/><path d="m25 14 12 15-12 15-12-15Z" opacity=".55"/><path d="M51 35v18m-9-9h18"/>',
  'requisito-arcano': '<path d="m20 5 24 0 15 15v24L44 59H20L5 44V20Z"/><circle cx="32" cy="32" r="17"/><path d="M21 25q3-8 12-8M36 43l5-5"/><path d="m32 25 3 7-3 7-3-7Z" fill="currentColor" stroke="none"/>',
  'suspender': '<path d="M10 8h17v24H10Z" stroke-dasharray="4 5" opacity=".45"/><rect x="12" y="40" width="37" height="18" rx="3"/><path d="M33 9q25 1 23 23m-7-6 7 7 5-8M4 42h4M4 50h4"/>',
  'levantar': '<path d="M29 42h28v15H29Z" stroke-dasharray="4 5" opacity=".45"/><rect x="7" y="7" width="21" height="32" rx="3"/><path d="M46 46Q39 30 36 14m-5 6 5-8 7 6M4 47h10M4 55h10"/>',
  'pagar-arcano': '<circle cx="34" cy="16" r="10"/><path d="M29 13q2-4 6-4M46 16h13m-6-6 6 6-6 6M5 49l9-12 8 2 12-4q10-2 10 4l-10 6h19q6 0 4 5L42 59H18L5 53"/>',
  'ganhar-arcano': '<circle cx="34" cy="34" r="17"/><path d="M24 28q3-7 9-7M4 34h12m-6-6 6 6-6 6M34 5v5M55 13l-4 4M59 37h-5M30 54v6"/><path d="M53 48v11m-5-5h10"/>',
  'enviar-arcano': '<path d="M8 33h18v25H8Z"/><path d="m13 48 5-5 5 5M27 30l9-9m-8-2h10v10"/><circle cx="47" cy="15" r="10"/><path d="M41 12q2-4 6-4"/><g fill="currentColor" stroke="none"><circle cx="31" cy="39" r="3"/><circle cx="38" cy="32" r="2.5"/><circle cx="39" cy="44" r="2"/></g>',
  'grau': '<path d="M6 34V8h31M11 39V14h31"/><rect x="17" y="20" width="30" height="36" rx="3"/><path d="m43 30 16 17-16 13-15-13Z" fill="currentColor" fill-opacity=".15"/>',
  'comprar': '<path d="M6 27h25v8H6Zm0 13h25M6 47h18"/><path d="m36 7 18 5-6 23-18-5Z"/><path d="M22 10q-8 2-10 11m-4-5 4 5 5-4M27 54l8-12 6 3 7-3q5-2 7 3l-7 7h10M26 60h23l11-6"/>',
  'olhar-topo': '<path d="M7 17Q31 1 57 17 32 36 7 17Z"/><circle cx="32" cy="17" r="6"/><path d="m9 36 14-4 7 25-14 4Zm17-1h15v26H26Zm17-3 14 4-7 25-14-4Z"/>',
  'fundo-baralho': '<rect x="5" y="26" width="29" height="10" rx="2"/><path d="M5 42h29M5 49h29"/><rect x="7" y="6" width="25" height="13" rx="2"/><path d="M43 13q16 10 11 37m-6-6 6 8 6-6M13 58h28"/>',
  'descartar': '<path d="M5 8l14 5 14-1q5 0 5 5l-13 4-10-1-6 9-6-5"/><path d="m29 28 17 7-8 18-16-7Z"/><path d="M49 41v-8a6 6 0 0 1 12 0v23H46M49 37h6M30 22v3m8-2 3 3"/>',
  'dano': '<path d="m29 8 9 15 16-8-6 17 12 9-20 2-4 17-11-14-18 9 7-18-12-7 18-4Z" fill="currentColor" fill-opacity=".15"/><path d="M7 8l7 6M46 5l-3 9M4 48l9-4"/>',
  'curar': '<path d="M32 5C25 18 13 29 13 42a19 19 0 0 0 38 0C51 29 39 18 32 5Z"/><path d="M32 29v22M21 40h22M20 47q1 5 5 7"/>',
  'destruir': '<path d="M9 6h23l-7 19 11 2-15 12H9ZM41 17h14v41H30l7-17-10-3 15-12Z"/><path d="m10 48-5 6M36 8l3-4M58 9l4 1M52 5l-1-2"/>',
  'devolver': '<rect x="40" y="22" width="18" height="24" rx="3"/><path d="M49 16Q24 1 12 23m-2-9 2 9 9-2M5 53l8-14 7 4 9-2q5-1 5 4l-7 6h11q6 0 3 5l-8 5H15Z"/>',
  'banir': '<ellipse cx="41" cy="32" rx="17" ry="24" stroke-dasharray="5 5"/><path d="M7 18h20v28H7M35 17q16 15 0 31"/><path d="M21 21v6m0 12v5M32 27h3M31 37h4"/>',
  'cemiterio': '<path d="m5 35 15-5 8 27-15 4Z"/><path d="M28 58V23a13 13 0 0 1 26 0v35M23 58h35M35 26h11M35 34h11"/>',
  'anular': '<rect x="6" y="7" width="28" height="42" rx="3"/><circle cx="40" cy="39" r="19"/><path d="m27 26 26 26M12 15h15"/>',
  'invocar': '<ellipse cx="32" cy="52" rx="26" ry="9"/><rect x="21" y="13" width="22" height="31" rx="3"/><path d="M27 20h10M16 42l-3-6M48 42l3-6M7 16v9m-4-5h9M52 8v9m-4-5h9"/>',
  'ao-entrar': '<rect x="20" y="5" width="24" height="26" rx="3"/><path d="M32 36v11m-7-6 7 7 7-7M7 52q25 13 50 0M15 43l-7 4M49 43l7 4M27 13h10"/>',
  'atacar': '<path d="m7 7 14 7 35 35-7 7L14 21Zm50 0-14 7L8 49l7 7 35-35Z" fill="currentColor" fill-opacity=".14"/><path d="m7 42 15 15m20-15 15 15M4 27h7M54 28h6"/>',
  'bloquear': '<path d="M8 9q17-8 34 0v23q0 18-17 29C8 50 8 41 8 32Z"/><path d="M16 17q9-3 18 0v18q0 10-9 17M49 8l-4 13 13-3-9 12 11 6-12 4"/>',
  'repentina': '<path d="M35 5 13 35h16l-2 24 24-34H35Z" fill="currentColor" stroke-width="2"/><path d="M5 19h9M49 44h10"/>',
  'findante': '<path d="M12 6h40M12 58h40M17 8v7q0 11 15 17-15 6-15 17v7M47 8v7q0 11-15 17 15 6 15 17v7"/><path d="m23 15 9 12 9-12ZM23 51l9-12 9 12Z" fill="currentColor" stroke="none"/>',
  'proficiente': '<path d="M15 32h17v12l19 4q6 2 6 11H14l-4-13ZM32 31l14-22q12 0 14 7l-21 9m-4 6 23-10M7 18h13M4 26h13"/>',
  'etereo': '<path d="M10 52V28a22 22 0 0 1 44 0v25l-11-5-11 10-11-10Z" stroke-dasharray="6 5"/><g fill="currentColor" stroke="none"><circle cx="24" cy="28" r="3"/><circle cx="40" cy="28" r="3"/></g><path d="M23 38q9 6 18 0"/>',
  'protegido': '<path d="M8 11q24-13 48 0v22q0 15-24 28C8 48 8 41 8 33Z"/><rect x="22" y="18" width="20" height="28" rx="3"/><path d="M28 25h8"/>',
  'protessencia': '<path d="M22 5C8 9 5 21 5 32S9 55 22 59C36 55 39 43 39 32S36 9 22 5Z"/><rect x="13" y="18" width="18" height="28" rx="3"/><path d="M18 25h8"/><circle cx="49" cy="20" r="8"/><path d="M49 8v4m0 16v4M37 20h4m16 0h4M41 12l16 16M59 38q-13 4-10 17m-5-4 5 5 5-5"/>',
  'economia': '<rect x="17" y="5" width="18" height="27" rx="3"/><path d="m40 7 16 5-7 25-16-5ZM5 52l9-14 8 4 9-3q6-1 6 4l-9 6h23q7 0 6 5l-14 7H18Z"/>',
  'cataclisma': '<path d="M28 7a23 23 0 0 0 0 46l-7-16 13-6-13-8ZM39 8a23 23 0 0 1 0 46l6-15-12-7 12-9Z"/><path d="M28 14l-3 9M53 7l6-4M58 57l3 4M7 59l-3 3"/><circle cx="54" cy="45" r="2.3" fill="currentColor" stroke="none"/>',
  'uso-unico': '<path d="M24 5h16M26 7v14L12 47q-4 10 8 12h26q10-2 6-12L38 21V7M19 43h25"/><path d="M7 8 58 57"/><path d="m30 28 4 7-4 7-4-7Z" fill="currentColor" stroke="none"/>',
};
const destinos = [path.join(arcano,'arena/icones'),path.join(arcano,'online/public/kit/icones')];
for (const pasta of destinos) await fs.mkdir(pasta,{recursive:true});
const manifesto = {}, arquivos = [];
const escapar = s => String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
for (const item of pedido.icones) {
  if (!arte[item.id]) throw new Error('Desenho ausente: '+item.id);
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" fill="none" stroke="currentColor" stroke-width="3.8" stroke-linecap="round" stroke-linejoin="round"><title>${escapar(item.nome)}</title>${arte[item.id]}</svg>\n`;
  for (const pasta of destinos) await fs.writeFile(path.join(pasta,item.id+'.svg'),svg);
  manifesto[item.id] = {arquivo:'/kit/icones/'+item.id+'.svg',okGpt:true};
  arquivos.push({id:item.id,nome:item.nome,arquivo:manifesto[item.id].arquivo,sha256:createHash('sha256').update(svg).digest('hex'),viewBox:[0,0,64,64],cor:'currentColor',fundo:'transparente'});
}
const json = JSON.stringify(manifesto,null,2)+'\n';
for (const pasta of destinos) await fs.writeFile(path.join(pasta,'manifesto.json'),json);
const cartoes = pedido.icones.map(i=>`<article><div class="amostra">${[48,20,16].map(t=>`<span style="width:${t}px;height:${t}px">${arte[i.id] ? `<svg viewBox="0 0 64 64" fill="none" stroke="currentColor" stroke-width="3.8" stroke-linecap="round" stroke-linejoin="round">${arte[i.id]}</svg>` : ''}</span>`).join('')}</div><strong>${escapar(i.nome)}</strong><small>${escapar(i.id)}</small></article>`).join('');
const total = pedido.icones.length;
const html = `<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>Arcanoclisma · ${total} ícones</title><style>*{box-sizing:border-box}body{margin:0;padding:28px;background:#0e1727;color:#edf1ec;font-family:system-ui,sans-serif}header{display:flex;justify-content:space-between;align-items:center;gap:20px;margin-bottom:20px}h1{font-size:26px;margin:0}p{color:#b1c6d2;margin:6px 0 0}.grade{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:12px}article{background:#eee6d5;color:#203849;border:1px solid #bbaa86;border-radius:12px;padding:12px 16px;min-height:101px}.amostra{height:52px;display:flex;align-items:center;gap:20px}.amostra span{display:block;flex-shrink:0}.amostra svg{display:block;width:100%;height:100%}strong,small{display:block}strong{font-size:14px}small{font-size:11px;opacity:.7}.grade article:nth-child(8n+5),.grade article:nth-child(8n+6),.grade article:nth-child(8n+7),.grade article:nth-child(8n+8){background:#182b41;color:#f0e1b8}@media(max-width:800px){.grade{grid-template-columns:repeat(2,minmax(0,1fr))}body{padding:16px}}</style><header><div><h1>Iconografia de Arcanoclisma</h1><p>${total} mecânicas · vetores transparentes · tamanhos 48, 20 e 16 px</p></div><span>Família v19</span></header><section class="grade">${cartoes}</section></html>`;
for (const pasta of destinos) await fs.writeFile(path.join(pasta,'catalogo.html'),html);
const entrega = {tarefaId:pedido.tarefaId,versao:19,pedidoVersao:pedido.versao,estado:'entregue',total:arquivos.length,manifesto:'Arcanoclisma/online/public/kit/icones/manifesto.json',gerador:'Arcanoclisma/arena/scripts/gerar-iconografia-v19.mjs',arquivos,decisoes:['Uso único tem frasco riscado e uma marca pictográfica, sem algarismo; respeita o pedido de não embutir números.','Números dos requisitos e poderes permanecem no texto editável do jogo.','SVG externo em img não herda a cor do texto; o adaptador visual de máscara v19 aplica currentColor ao elemento decorativo.']};
await fs.mkdir(path.join(arcano,'online/entregas-visuais'),{recursive:true});
await fs.writeFile(path.join(arcano,'online/entregas-visuais/iconografia-v19.codex.json'),JSON.stringify(entrega,null,2)+'\n');
console.log(`${arquivos.length} ícones gerados; fonte e cópia pública sincronizadas.`);
