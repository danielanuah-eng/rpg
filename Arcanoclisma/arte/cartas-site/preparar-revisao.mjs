import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
import crypto from 'node:crypto';
const require=createRequire(import.meta.url);
const sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const here=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(here,'../../..');
const read=async f=>JSON.parse(await fs.readFile(f,'utf8'));
const write=async(f,d)=>{await fs.mkdir(path.dirname(f),{recursive:true});await fs.writeFile(f,JSON.stringify(d,null,2)+'\n','utf8');};
const mestra=path.join(repo,'Arcanoclisma/expansoes/a-mestra-dos-dragoes');
const expansao=await read(path.join(mestra,'expansao.json'));
expansao.nomeProvisorio=false;expansao.quantidadeNova={alvo:90,provisoria:false,aguardandoResposta:false};
expansao.metaCartas=90;expansao.estado='aguardando_revisao_visual_do_jogo_atual';expansao.prioridade='depois de concluir revisão das 151 artes atuais';expansao.planejamentoVisualCompleto=false;
await write(path.join(mestra,'expansao.json'),expansao);
const filaExpansao=await read(path.join(mestra,'fila.json'));filaExpansao.metaProvisoria=null;filaExpansao.meta=90;filaExpansao.planejamentoCompleto=false;filaExpansao.estado=expansao.estado;filaExpansao.itens[0].estado='gerada_aguardando_avaliacao';
await write(path.join(mestra,'fila.json'),filaExpansao);
const origem='C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-5c99d17c-e442-412f-820d-b8550b409637.png';
const destino=path.join(mestra,'arte/imagens/001-ciantia-a-mestra-dos-dragoes-v1.png');
await fs.mkdir(path.dirname(destino),{recursive:true});await fs.copyFile(origem,destino);
const meta=await sharp(destino).metadata();
const {data,info}=await sharp(destino).ensureAlpha().raw().toBuffer({resolveWithObject:true});
let min=255,max=0,transparentes=0;for(let i=3;i<data.length;i+=4){min=Math.min(min,data[i]);max=Math.max(max,data[i]);if(data[i]===0)transparentes++;}
await write(path.join(mestra,'arte/registros/001.visual.json'),{id:'001',estado:'gerada_aguardando_avaliacao',arquivo:'Arcanoclisma/expansoes/a-mestra-dos-dragoes/arte/imagens/001-ciantia-a-mestra-dos-dragoes-v1.png',origem,ferramenta:'image_gen.imagegen integrada',dimensoes:[meta.width,meta.height],sha256:crypto.createHash('sha256').update(await fs.readFile(destino)).digest('hex'),alfa:{hasAlpha:meta.hasAlpha,min,max,pixelsTransparentes:transparentes},fundo:'Cenário completo opaco',liberadaParaJogo:false,claudePodeAlterarCarta:false,pushAutorizado:false});
const briefing=await read(path.join(repo,'Arcanoclisma/cartas/pedidos/lote-site-v1.claude.json'));
const dados=await read(path.join(repo,'Arcanoclisma/online/public/jogo/dados.json'));
const todas=[...briefing.conjuradores,...briefing.cartas];
const usoBaralhos=new Set((briefing.baralhosProntos||[]).flatMap(b=>b.cartas));
const idsJogo=new Set([...Object.keys(dados.catalogo.cartas),...Object.keys(dados.catalogo.conjuradores).map(id=>'conjurador-'+id)]);
const fila=todas.filter(c=>idsJogo.has(c.id)).map(c=>({
 id:c.id,nome:c.nome,tipo:c.tipo,raridade:c.raridade,arteAlternativa:c.arteAlternativa,
 destino:c.destino,fontePedido:'Arcanoclisma/cartas/pedidos/lote-site-v1.claude.json',pedidoVersao:briefing.versao,
 prioridade:c.tipo==='conjurador'?0:usoBaralhos.has(c.id)?1:c.tipo==='item'?2:3,
 referenciaJogoAtual:c.tipo==='conjurador'?'Arcanoclisma/online/public/arte/'+dados.conjuradoresArte[c.id.replace('conjurador-','')]+'.webp':null,
 referenciasIdentidade:c.personagens.flatMap(p=>p.referencias||[]),
 estado:'pendente',baseInspecionada:false,descricaoLida:false,
 identidade:c.identidade,cenaOriginal:c.cena,ambienteOriginal:c.ambiente,variacaoOriginal:c.variacao,
 personagens:c.personagens,objetoReferencia:c.objetoReferencia||null
})).sort((a,b)=>a.prioridade-b.prioridade);
for(const item of fila.filter(c=>c.tipo!=='conjurador')) {
 const definicao=dados.catalogo.cartas[item.id];
 const visual=dados.visual?.[item.id]||definicao?.visual;
 const arte=dados.cartasVisual?.[item.id]?.arte||definicao?.arte||visual?.arte;
 if(arte)item.referenciaJogoAtual='Arcanoclisma/online/public/arte/'+arte+'.webp';
}
const arquivoFila=path.join(here,'fila.json');
try {const anterior=await read(arquivoFila);for(const item of fila){const velho=anterior.itens?.find(x=>x.id===item.id);if(velho)Object.assign(item,velho);}}catch(e){if(e.code!=='ENOENT')throw e;}
await write(arquivoFila,{versao:1,tarefaId:briefing.tarefaId,pedidoVersao:briefing.versao,meta:fila.length,origem:'Catálogo realmente servido pelo jogo atual e briefing do Claude',estado:'em_producao',prioridades:['conjuradores','cartas de baralhos prontos','itens','restante'],naoAlterarRegras:true,itens:fila});
await write(path.join(here,'progresso.json'),{estado:'em_producao',meta:fila.length,concluidas:fila.filter(c=>c.estado!=='pendente').length,pendentes:fila.filter(c=>c.estado==='pendente').map(c=>c.id),expansaoAdiada:'A Mestra dos Dragões, 90 cartas, bloqueada para jogo',aguardandoAvaliacaoFinal:false,pushAutorizado:false});
await write(path.join(here,'producao.lock.json'),{ativa:true,iniciadaEm:new Date().toISOString(),atualizadaEm:new Date().toISOString(),dono:'Codex nesta conversa',regra:'Automação deve aguardar enquanto a lock ativa foi atualizada há menos de 45 minutos'});
console.log(JSON.stringify({meta:fila.length,conjuradores:fila.filter(c=>c.tipo==='conjurador').length,cartas:fila.filter(c=>c.tipo!=='conjurador').length,expansao:{meta:90,imagemSalva:'001-ciantia-a-mestra-dos-dragoes-v1.png',dimensoes:[meta.width,meta.height]}}));

