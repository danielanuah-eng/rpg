import fs from 'node:fs/promises';
import {constants} from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
import crypto from 'node:crypto';
const require=createRequire(import.meta.url),sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const here=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(here,'../../..');
const ler=async f=>JSON.parse(await fs.readFile(f,'utf8'));
const salvar=async(f,d)=>{await fs.mkdir(path.dirname(f),{recursive:true});await fs.writeFile(f,JSON.stringify(d,null,2)+'\n','utf8');};
const rel=f=>path.relative(repo,f).split(path.sep).join('/');
const entrada=await ler(process.argv[2]);
const fila=await ler(path.join(here,'fila.json'));
const item=fila.itens.find(c=>c.id===entrada.id);
if(!item)throw new Error('ID fora do catálogo: '+entrada.id);
const dados=await fs.readFile(entrada.origem),hash=crypto.createHash('sha256').update(dados).digest('hex');
let destino=path.join(here,entrada.id+'.png');
try{await fs.copyFile(entrada.origem,destino,constants.COPYFILE_EXCL);}catch(e){if(e.code!=='EEXIST')throw e;const velho=crypto.createHash('sha256').update(await fs.readFile(destino)).digest('hex');if(velho!==hash){destino=path.join(here,entrada.id+'-'+hash.slice(0,8)+'.png');await fs.copyFile(entrada.origem,destino,constants.COPYFILE_EXCL);}}
const meta=await sharp(destino).metadata(),{data,info}=await sharp(destino).ensureAlpha().raw().toBuffer({resolveWithObject:true});
let min=255,max=0,transparentes=0;for(let i=3;i<data.length;i+=4){const a=data[i];min=Math.min(min,a);max=Math.max(max,a);if(a===0)transparentes++;}
const promptFile=path.join(here,'prompts',entrada.id+'.txt');await fs.mkdir(path.dirname(promptFile),{recursive:true});await fs.writeFile(promptFile,entrada.prompt+'\n','utf8');
const registro={...entrada,prompt:rel(promptFile),ferramenta:'image_gen.imagegen integrada',modo:'built-in',arquivo:rel(destino),dimensoes:[meta.width,meta.height],dimensoesSolicitadas:[1260,1760],dimensoesEntregues:'Saída nativa real; não redimensionada',sha256:hash,alfa:{hasAlpha:meta.hasAlpha,min,max,pixelsTransparentes:transparentes,cantos:[0,info.width-1,(info.height-1)*info.width,info.height*info.width-1].map(i=>data[i*4+3])},raridade:item.raridade,arteAlternativa:item.arteAlternativa,fundo:'Cena completa opaca',estado:'gerada_aguardando_avaliacao_final',integracaoClaudeConfirmada:false};
await salvar(path.join(here,'registros',entrada.id+'.visual.json'),registro);
Object.assign(item,{estado:registro.estado,baseInspecionada:entrada.baseInspecionada,descricaoLida:entrada.descricaoLida,arteNova:registro.arquivo,registro:rel(path.join(here,'registros',entrada.id+'.visual.json'))});
await salvar(path.join(here,'fila.json'),fila);
const prontas=fila.itens.filter(c=>c.arteNova),pendentes=fila.itens.filter(c=>!c.arteNova);
const estado=pendentes.length?'em_producao':'aguardando_avaliacao';
await salvar(path.join(here,'progresso.json'),{estado,meta:fila.meta,concluidas:prontas.length,pendentes:pendentes.map(c=>c.id),expansaoAdiada:'A Mestra dos Dragões, 90 cartas, bloqueada para jogo',aguardandoAvaliacaoFinal:pendentes.length===0,pushAutorizado:true,publicar:'Entregas concluídas; aprovação visual e liberação da expansão são independentes'});
await salvar(path.join(here,'producao.lock.json'),{ativa:true,atualizadaEm:new Date().toISOString(),dono:'Codex nesta conversa',regra:'Automação aguarda lock ativa atualizada há menos de 45 minutos'});
const entrega={tarefaId:fila.tarefaId,pedidoVersao:fila.pedidoVersao,estado,meta:fila.meta,concluidas:prontas.length,pendentes:pendentes.map(c=>c.id),cartas:prontas.map(c=>({id:c.id,nome:c.nome,arquivo:c.arteNova,arteModo:'cena',registro:c.registro,estado:c.estado})),integracaoClaudeConfirmada:false,pushAutorizado:true,instrucoesClaude:['Substituir a arte provisória pelo PNG correspondente ao mesmo ID; arteModo=cena; sem fundo separado nem recorte colado.','Novas artes não alteram custos, Grau, habilidades ou outros dados de jogo.','Usar saída nativa de alta resolução; não rebaixar a miniatura de 640px como arquivo único da carta.','Manter verso idêntico para cartas ocultas; jamais colocar identidade/raridade privada no DOM.','A Mestra dos Dragões permanece bloqueada até liberação explícita do autor.'],galeria:'Arcanoclisma/arte/cartas-site/galeria.html'};
await salvar(path.join(repo,'Arcanoclisma/cartas/entregas/lote-site-v1.visual.json'),entrega);
const galeria={meta:fila.meta,concluidas:prontas.length,cartas:prontas.map(c=>({id:c.id,nome:c.nome,raridade:c.raridade,imagem:path.relative(here,path.join(repo,c.arteNova)).split(path.sep).join('/'),base:c.referenciaJogoAtual?path.relative(here,path.join(repo,c.referenciaJogoAtual)).split(path.sep).join('/'):null,estado:c.estado}))};
await fs.writeFile(path.join(here,'galeria-dados.js'),'window.ARTES_SITE='+JSON.stringify(galeria)+';\n','utf8');
console.log(JSON.stringify({id:entrada.id,arquivo:registro.arquivo,dimensoes:registro.dimensoes,alfa:registro.alfa,concluidas:prontas.length,meta:fila.meta}));

