import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {createRequire} from 'node:module';
const pasta=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(pasta,'../../..');
const sharp=createRequire(path.join(repo,'Arcanoclisma/online/package.json'))('sharp');
const ler=async f=>JSON.parse(await fs.readFile(f,'utf8'));
const salvar=async(f,obj)=>fs.writeFile(f,JSON.stringify(obj,null,2)+'\n');
const existe=async f=>{try{await fs.access(f);return true;}catch{return false;}};
const filaPath=path.join(pasta,'fila-valores.json');
const dadosPath=path.join(repo,'Arcanoclisma/online/public/jogo/dados.json');
const comum='Use case: stylized-concept. NEW full illustration for Arcanoclisma TCG, one vertical portrait in 63:88 aspect ratio, native high resolution, fully opaque scene to every edge. No text, no writing, no lettering, no logos, no borders, no card UI. Reference image is approved IDENTITY and graphic STYLE only: preserve unique face/species, body type, age, hair, distinctive attire/materials/gear; NEVER paste or repeat its cutout or pose. Create a new unified coherent scene. Simple efficient composition, one clear main subject, few props, readable silhouettes, subject large, face below top12%, bottom17% less busy for editable rules. Modern independent comics and contemporary animation, organic confident varied linework, flat color shapes, simple BLOCKED cel shadows, sparse halftone. Surroundings smooth and broad, minimal grit or painterly detail. Diffuse contained lighting, strong magic only locally, complementary warm/cool color areas, matte restrained gold. Not a photograph, not 3D, not a painting, not glossy generic anime, no HDR, no broad bloom, no empty or transparent backdrop. Humans have human anatomy, expressive faces and individual character; keep reference identity. Directed magic follows gaze and gesture axis, not a bent beam.';
const cores={persona:'coral smooth aura',cognitium:'indigo fragmented smoke veil',primitivum:'moss green and muted amber organic constructs',dominum:'dark blood-purple hard-light chains',sublimeyn:'cool silver-mercury mist',morbidyum:'pale green cold smoke',conflituo:'rust-red hard-light shapes',eternyr:'night-blue broken aura with stopped short white traces',alvorum:'muted gold merging with dark violet-black, no dazzling gold background'};
async function atualizar(fila){
 const prontas=fila.cartas.filter(c=>c.arquivo),pendentes=fila.cartas.filter(c=>!c.arquivo);
 fila.estado=pendentes.length?'em_producao':'aguardando_avaliacao';fila.concluidas=prontas.length;fila.pendentes=pendentes.map(c=>c.id);
 await salvar(filaPath,fila);
 await salvar(path.join(pasta,'progresso-valores.json'),{estado:fila.estado,meta:fila.meta,concluidas:prontas.length,pendentes:fila.pendentes,ordem:'rodadas_por_valor_menos_representado',integracaoClaudeConfirmada:false});
 await salvar(path.join(pasta,'producao-valores.lock.json'),{ativa:pendentes.length>0,atualizadaEm:new Date().toISOString(),dono:'Codex nesta conversa',escopo:prontas.length+'/'+fila.meta});
 const entrega={tarefaId:fila.tarefaId,versao:1,data:'2026-10-04',estado:fila.estado,meta:fila.meta,concluidas:prontas.length,pendentes:fila.pendentes,criterio:fila.criterio,porValor:fila.porValor,cartas:prontas.map(c=>({id:c.id,nome:c.nome,valor:c.valor,arquivo:c.arquivo,prompt:c.promptArquivo,registro:c.registro,arteModo:'cena',sha256:c.sha256,dimensoes:c.dimensoes,estado:'entregue_aguardando_avaliacao_do_autor'})),galeria:'Arcanoclisma/arte/cartas-rapidas/galeria.html',integracaoClaudeConfirmada:false,pushAutorizadoPeloAutor:true,instrucoesClaude:['Consumir PNG pelo ID exato. Cena e personagem já são uma ilustração única; não montar um recorte em fundo separado.','Não criar cartas nem mudar regras, custos, Grau ou Valores por causa desta entrega.','Este lote completa artes das cartas provisórias já definidas em tcg/src/cartas-provisorias.ts; a expansão A Mestra dos Dragões não foi alterada.','Preservar PNG nativo como fonte, converter no build quando necessário, e registrar a integração por arquivo.']};
 await salvar(path.join(repo,'Arcanoclisma/cartas/entregas/artes-rapidas-valores.visual.json'),entrega);
 const antigas=await ler(path.join(pasta,'lote.json'));
 const todasDoPedido=[...antigas.cartas,...fila.cartas];
 await salvar(path.join(repo,'Arcanoclisma/cartas/entregas/cartas-provisorias-v1.visual.json'),{tarefaId:'cartas-provisorias-v1',versao:2,estado:fila.estado,meta:todasDoPedido.length,concluidas:antigas.cartas.filter(c=>c.arquivo).length+prontas.length,escopo:'Todas as cartas do briefing atual, com as três cenas iniciais preservadas.',ordem:todasDoPedido.map(c=>c.id),ficamDeFora:[],referenciasAlteradas:[],cartas:todasDoPedido.map(c=>({id:c.id,nome:c.nome,referencia:c.referencia??c.arteReferencia,arquivo:c.arquivo??null,registro:c.registro??null,estado:c.arquivo?'entregue_aguardando_avaliacao_do_autor':'pendente'})),aprovacaoDoAutor:'aguardando_avaliacao_visual',publicacaoSiteAutorizada:false,efeitos:'Produzir após concluir as cenas; não estão incluídos nestes PNGs.',raridade:'Somente estética; seguir o briefing.',integracaoClaudeConfirmada:false});
 const galeria=[...antigas.cartas.map(c=>({...c,valor:c.valor})),...prontas];
 const esc=s=>String(s).replace(/[&<>"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));
 const html='<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Artes rápidas por Valor</title><style>body{margin:0;padding:24px;background:#111b2a;color:#e9e2d5;font-family:system-ui}p,small{color:#bac8d3}.grade{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:18px}article{background:#1c2c3b;border:1px solid #6a7280;border-radius:12px;padding:12px}img{width:100%;height:auto;display:block;border-radius:8px}h2{font-size:20px;margin:10px 0 6px}small{overflow-wrap:anywhere}a{color:#ddbc80}@media(max-width:750px){.grade{grid-template-columns:1fr}}</style><h1>Artes por Valor</h1><p>'+prontas.length+'/'+fila.meta+' novas cenas · mais as 3 do lote inicial · avaliação visual e integração em andamento</p><section class="grade">'+galeria.map(c=>'<article><a href="'+esc(path.basename(c.arquivo))+'"><img src="'+esc(path.basename(c.arquivo))+'" alt="'+esc(c.nome)+'" loading="lazy"></a><h2>'+esc(c.nome)+'</h2><small>'+esc(c.valor)+' · '+esc(c.id)+'</small></article>').join('')+'</section></html>';
 await fs.writeFile(path.join(pasta,'galeria.html'),html);
}
if(process.argv[2]==='preparar'){
 if(await existe(filaPath))throw new Error('Fila já preparada; retomar sem sobrescrever.');
 const dados=await ler(dadosPath),cenas=await ler(path.join(pasta,'cenas-valores.json'));
 const todas=Object.values(dados.catalogo.cartas),contagens={};
 for(const c of todas)for(const v of c.valores)contagens[v]=(contagens[v]??0)+1;
 const valores=Object.keys(contagens).sort((a,b)=>contagens[a]-contagens[b]||a.localeCompare(b));
 const grupos={};for(const v of valores)grupos[v]=[];
 for(const c of todas){
  if(!c.provisoria||await existe(path.join(pasta,c.id+'.cena-v1.png')))continue;
  if(!cenas[c.id])throw new Error('Cena não planejada: '+c.id);
  const valor=c.valores[0],referencia='Arcanoclisma/online/public/arte/'+dados.visual[c.id].arte+'.webp';
  if(!await existe(path.join(repo,referencia)))throw new Error('Referência ausente: '+c.id);
  const prompt=comum+' VALUE color and magic: '+cores[valor]+'. Scene: '+cenas[c.id]+' Game identity/effect (context only, never write it): '+c.nome+'; '+c.texto+'. Rarity is unspecified, no rarity foil filter.';
  const promptArquivo='Arcanoclisma/arte/cartas-rapidas/'+c.id+'.prompt.txt';
  await fs.writeFile(path.join(repo,promptArquivo),prompt+'\n');
  grupos[valor].push({id:c.id,nome:c.nome,tipo:c.tipo,valor,referencia,prompt,promptArquivo,cena:cenas[c.id],definicaoAtualClaude:c,raridade:dados.visual[c.id].raridade??null,arteAlternativa:false,estado:'pendente'});
 }
 const cartas=[];while(valores.some(v=>grupos[v].length))for(const v of valores)if(grupos[v].length)cartas.push(grupos[v].shift());
 const fila={tarefaId:'artes-rapidas-valores-2026-10-04',versao:1,data:'2026-10-04',meta:cartas.length,criterio:'Somente cartas provisórias existentes sem cena própria. Rodadas entre os Valores, priorizando os menos representados no catálogo; não inventa mecânicas.',porValor:valores.map(v=>({valor:v,cartasNoCatalogo:contagens[v],artesPendentesDesteLote:cartas.filter(c=>c.valor===v).length})),cartas};
 await atualizar(fila);console.log(JSON.stringify(fila));
}else if(process.argv[2]==='registrar'){
 const [id,origem]=process.argv.slice(3),fila=await ler(filaPath),c=fila.cartas.find(c=>c.id===id);
 if(!c||c.arquivo)throw new Error('ID ausente ou já entregue: '+id);
 const atual=(await ler(dadosPath)).catalogo.cartas[id];if(!atual)throw new Error('Carta removida do catálogo atual: '+id);
 const destino=path.join(pasta,id+'.cena-v1.png');
 await fs.copyFile(origem,destino,fs.constants.COPYFILE_EXCL);
 const bytes=await fs.readFile(destino),meta=await sharp(bytes).metadata();
 const {data,info}=await sharp(bytes).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let min=255,max=0,transparentes=0;for(let k=3;k<data.length;k+=info.channels){min=Math.min(min,data[k]);max=Math.max(max,data[k]);if(!data[k])transparentes++;}
 if(min!==255)throw new Error('Cena final não é opaca: '+id);
 Object.assign(c,{arquivo:'Arcanoclisma/arte/cartas-rapidas/'+id+'.cena-v1.png',registro:'Arcanoclisma/arte/cartas-rapidas/'+id+'.visual.json',sha256:createHash('sha256').update(bytes).digest('hex'),dimensoes:[meta.width,meta.height],estado:'entregue_aguardando_avaliacao_do_autor'});
 const registro={...c,prompt:c.promptArquivo,origem,ferramenta:'image_gen.imagegen integrada',modo:'built-in',baseInspecionada:true,fundo:'Cena completa opaca',alfa:{hasAlpha:meta.hasAlpha??false,min,max,pixelsTransparentes:transparentes,total:info.width*info.height},dimensoesEntregues:'PNG nativo preservado',proporcaoSolicitada:'63:88',definicaoAtualClaude:atual,integracaoClaudeConfirmada:false,alteracoesMecanicas:false};
 delete registro.promptArquivo;await salvar(path.join(repo,c.registro),registro);await atualizar(fila);
 console.log(JSON.stringify({id,concluidas:fila.concluidas,meta:fila.meta,dimensoes:c.dimensoes,alfa:registro.alfa}));
}else if(process.argv[2]==='cota'){
 const fila=await ler(filaPath);fila.estado='aguardando_cota';fila.limitacao=process.argv[3]??'Horário de liberação não fornecido pela ferramenta.';
 await salvar(filaPath,fila);await salvar(path.join(pasta,'progresso-valores.json'),{estado:fila.estado,meta:fila.meta,concluidas:fila.concluidas,pendentes:fila.pendentes,limitacao:fila.limitacao});
 await salvar(path.join(pasta,'producao-valores.lock.json'),{ativa:false,atualizadaEm:new Date().toISOString(),motivo:'Cota de imagens; não repetir nesta execução.'});
 console.log(JSON.stringify({estado:fila.estado,concluidas:fila.concluidas,meta:fila.meta}));
}else if(process.argv[2]==='sincronizar'){
 const fila=await ler(filaPath);await atualizar(fila);console.log(JSON.stringify({estado:fila.estado,concluidas:fila.concluidas,meta:fila.meta}));
}else{const fila=await ler(filaPath);console.log(JSON.stringify({estado:fila.estado,concluidas:fila.concluidas,meta:fila.meta,proxima:fila.cartas.find(c=>!c.arquivo)}));}
