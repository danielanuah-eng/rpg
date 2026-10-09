import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
import {createHash} from 'node:crypto';
const sharp=createRequire(import.meta.url)('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const root=path.dirname(fileURLToPath(import.meta.url));
const inputs=JSON.parse(await fs.readFile(process.argv[2],'utf8'));
const plan=JSON.parse(await fs.readFile(path.join(root,'fila.json'),'utf8'));
for(const input of inputs){
 const item=plan.itens.find(x=>x.id===input.id);if(!item)throw Error('ID inválido');
 if(input.prompt)item.prompt=input.prompt;
 if(input.referencias)item.referencias=input.referencias;
 for(const dir of ['imagens','prompts','registros'])await fs.mkdir(path.join(root,dir),{recursive:true});
 const versao=input.versao??'v1';item.versao=versao;
 const arquivo='imagens/'+item.id+'-'+item.slug+'-'+versao+'.png';
 await fs.copyFile(input.origem,path.join(root,arquivo));
 const bytes=await fs.readFile(path.join(root,arquivo));const meta=await sharp(bytes).metadata();
 const raw=await sharp(bytes).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let min=255,max=0,transparent=0,sat=0;
 for(let i=0;i<raw.data.length;i+=4){const a=raw.data[i+3];min=Math.min(min,a);max=Math.max(max,a);if(a===0)transparent++;const hi=Math.max(raw.data[i],raw.data[i+1],raw.data[i+2]);const lo=Math.min(raw.data[i],raw.data[i+1],raw.data[i+2]);sat+=hi?(hi-lo)/hi:0;}
 const prompt='prompts/'+item.id+'-'+versao+'.prompt.txt',registro='registros/'+item.id+'-'+versao+'.visual.json';
 await fs.writeFile(path.join(root,prompt),item.prompt+'\n');
 const record={...item,prompt,arquivo,estado:'gerada_aguardando_avaliacao',versao,ferramenta:'image_gen integrada',origem:input.origem,dimensoes:[meta.width,meta.height],sha256:createHash('sha256').update(bytes).digest('hex'),alfa:{min,max,pixelsTransparentes:transparent},saturacaoMediaHSV:Number((sat/(raw.data.length/4)*100).toFixed(1)),revisaoVisual:'inspecionada pelo Codex; avaliação do autor pendente',liberadaParaJogo:false,pushAutorizado:true,enriquecimentosVisuais:'composição, criatura, aparência e nome propostos; sem mecânica',animacao:{estado:'aguardando_definicao_de_carta_e_efeito',nota:'Vínculo por ID, skin e hash reservado; nenhuma animação de resolução pode ser especificada antes do efeito autorizado.'}};
 await fs.writeFile(path.join(root,registro),JSON.stringify(record,null,2)+'\n');
 Object.assign(item,{arquivo,prompt,registro,estado:record.estado,sha256:record.sha256,dimensoes:record.dimensoes,saturacaoMediaHSV:record.saturacaoMediaHSV});console.log(JSON.stringify({id:item.id,arquivo,dimensoes:record.dimensoes,saturacao:record.saturacaoMediaHSV}));
}
plan.concluidas=plan.itens.filter(x=>x.arquivo).length;plan.estado=plan.concluidas===plan.meta?'aguardando_avaliacao':'em_producao';
await fs.writeFile(path.join(root,'fila.json'),JSON.stringify(plan,null,2)+'\n');
const selected=plan.itens.filter(x=>x.arquivo);
await fs.writeFile(path.join(root,'colecao-ampliada.visual.json'),JSON.stringify({...plan,itens:selected.map(({prompt,...x})=>({...x,prompt:typeof prompt==='string'&&prompt.startsWith('prompts/')?prompt:null})),galeria:'galeria.html'},null,2)+'\n');
const old=JSON.parse(await fs.readFile(path.join(root,'../fila-lore-2026-10-09.json'),'utf8'));
const all=[...old.itens.filter(x=>x.arquivo).map(x=>({...x,arquivo:'../'+x.arquivo,categoriaVisual:'historia e dragoes'})),...selected];
const esc=s=>String(s??'').replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
const cats=[...new Set(all.map(x=>x.categoriaVisual))];
const cards=all.map(x=>'<figure data-category="'+esc(x.categoriaVisual)+'"><a href="'+esc(x.arquivo)+'"><img loading="lazy" src="'+esc(x.arquivo)+'" alt="'+esc(x.nomeVisual)+'"></a><figcaption>'+esc(x.id)+' · '+esc(x.nomeVisual)+'<small>'+esc(x.categoriaVisual)+(x.valores?' · '+x.valores.map(esc).join(' + '):'')+'</small><p>'+esc(x.cena)+'</p></figcaption></figure>').join('\n');
await fs.writeFile(path.join(root,'galeria.html'),'<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>A Mestra dos Dragões — coleção ampliada</title><style>body{margin:30px;background:#20262a;color:#eee;font:16px system-ui}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:24px}figure{margin:0;background:#2b3338;padding:12px;border-radius:10px}img{width:100%;border-radius:7px}figcaption{padding:12px 0}small,p{color:#bfc9ce}small{display:block;margin-top:8px}button{padding:10px;margin:4px;border:0;border-radius:6px;background:#43535b;color:white}figure[hidden]{display:none}</style><h1>A Mestra dos Dragões</h1><p>'+all.length+'/90 artes selecionadas. Novo lote: '+plan.concluidas+'/65. Cores menos saturadas; nomes e novas aparências em avaliação.</p><nav><button data-filter="all">Todas</button>'+cats.map(c=>'<button data-filter="'+esc(c)+'">'+esc(c)+'</button>').join('')+'</nav><main>'+cards+'</main><script>document.querySelectorAll("[data-filter]").forEach(b=>b.onclick=()=>document.querySelectorAll("figure").forEach(f=>f.hidden=b.dataset.filter!=="all"&&f.dataset.category!==b.dataset.filter));</script></html>');
