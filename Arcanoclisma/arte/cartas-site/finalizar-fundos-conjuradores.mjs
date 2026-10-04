import fs from 'node:fs/promises';
import {constants} from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';
const require=createRequire(import.meta.url),sharp=require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const here=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(here,'../../..'),modelos=path.join(repo,'Arcanoclisma/cartas/modelos-visuais/2026-10-04-conjuradores');
const ler=async f=>JSON.parse(await fs.readFile(f,'utf8'));
const salvar=async(f,d)=>{await fs.mkdir(path.dirname(f),{recursive:true});await fs.writeFile(f,JSON.stringify(d,null,2)+'\n','utf8');};
const rel=f=>path.relative(repo,f).split(path.sep).join('/');
const variantes=[];
for(const [entrada,nomePrompt,selecionada] of [['ciantia.entrada.json','ciantia-lago.prompt.txt',false],['ciantia-dimensao.entrada.json','ciantia-dimensao.prompt.txt',true]]){
 const e=await ler(path.join(modelos,entrada)),f=path.join(repo,e.destino),dados=await fs.readFile(e.origem);
 const sha256=crypto.createHash('sha256').update(dados).digest('hex');
 try{await fs.copyFile(e.origem,f,constants.COPYFILE_EXCL);}catch(err){if(err.code!=='EEXIST')throw err;const velho=crypto.createHash('sha256').update(await fs.readFile(f)).digest('hex');if(velho!==sha256)throw new Error('Arquivo existente diferente: '+f);}
 const meta=await sharp(f).metadata(),{data,info}=await sharp(f).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let min=255,max=0,transparentes=0;for(let i=3;i<data.length;i+=4){const a=data[i];min=Math.min(min,a);max=Math.max(max,a);if(a===0)transparentes++;}
 await fs.writeFile(path.join(modelos,nomePrompt),e.prompt+'\n','utf8');
 variantes.push({...e,prompt:rel(path.join(modelos,nomePrompt)),arquivo:e.destino,sha256,dimensoes:[meta.width,meta.height],alfa:{hasAlpha:meta.hasAlpha,min,max,pixelsTransparentes:transparentes,cantos:[0,info.width-1,(info.height-1)*info.width,info.height*info.width-1].map(i=>data[i*4+3])},selecionada,ferramenta:'image_gen.imagegen integrada',dimensoesEntregues:'Saída nativa, sem redimensionamento',estado:selecionada?'gerada_aguardando_avaliacao_final':'variante_anterior_nao_selecionada'});
}
await salvar(path.join(modelos,'manifesto.json'),{versao:1,personagemFonte:'Arcanoclisma/arte/imagens/ciantia-dragoes.png',dimensaoFonte:'Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/dragao-da-travessia-arte-v1.png',liberadaNoJogo:false,variantes});
const fila=await ler(path.join(here,'fila.json')),conjuradores=fila.itens.filter(i=>i.tipo==='conjurador');
for(const i of conjuradores)i.diretrizAutorAtual='Conjurador: preservar personagem, pose e traço da arte atual do site; mudar só o fundo. Cenas anteriores com outra pose ficam reservadas para cartas futuras.';
await salvar(path.join(here,'fila.json'),fila);
const prontas=fila.itens.filter(i=>i.arteNova);
const entrega={versao:1,data:'2026-10-04',pedidoAutor:'Enviar os conjuradores com fundos corrigidos e todas as outras artes já concluídas no mesmo push.',estado:'entrega_visual_concluida_fila_em_producao',pushAutorizado:true,aprovacaoVisualFinal:false,integracaoClaudeConfirmada:false,totalFila:fila.meta,concluidas:prontas.length,pendentes:fila.meta-prontas.length,loteCompleto:'Arcanoclisma/cartas/entregas/lote-site-v1.visual.json',galeriaLote:'Arcanoclisma/arte/cartas-site/galeria.html',conjuradores:conjuradores.map(i=>({id:i.id,idConjurador:i.id.replace('conjurador-',''),nome:i.nome,arquivo:i.arteNova,registro:i.registro,arteModo:'cena',estado:i.estado,origemPersonagem:i.referenciaJogoAtual})),ciantia:{arquivo:variantes.find(v=>v.selecionada).arquivo,manifesto:rel(path.join(modelos,'manifesto.json')),dimensao:'Mesma dimensão da primeira carta do Dragão Transplanar, com dragões verde, coral e âmbar vistos de baixo.',liberadaNoJogo:false},cenasParaCartasFuturas:'Arcanoclisma/arte/cartas-site/variantes/conjuradores-cenas-para-cartas-futuras/catalogo.json',instrucoesClaude:['Os arquivos deste mapa substituem as versões de conjurador anteriores; use o caminho exato, incluindo o hash no nome.','As nove cenas anteriores continuam preservadas para cartas futuras, não para o retrato de conjurador.','As 94 artes concluídas estão no lote vinculado; a fila de 210 ainda tem 116 pendências.','Não marcar aprovação final do autor automaticamente. A aprovação de push permite publicar a entrega.','ArteModo=cena. Preservar nitidez dos PNGs nativos e evitar filtro fosco.','Raridade somente estética, atualmente sem definição no app; preservar os acabamentos disponíveis.','Ciantia é modelo visual suplementar; A Mestra dos Dragões continua bloqueada para ativação ou mudanças de cartas até liberação explícita do autor.']};
await salvar(path.join(repo,'Arcanoclisma/cartas/entregas/conjuradores-fundos-2026-10-04.visual.json'),entrega);
const imagens=[...entrega.conjuradores,...[{nome:'Ciantia — dimensão transplanar',arquivo:entrega.ciantia.arquivo}]];
await fs.writeFile(path.join(modelos,'galeria.html'),'<!doctype html><meta charset="utf-8"><title>Conjuradores com cenários</title><style>body{background:#18212d;color:#eee;font:16px system-ui;margin:30px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:24px}img{width:100%;border-radius:12px}h2{font-size:17px}a{color:#b5dcff}</style><h1>Conjuradores com cenários</h1><p>Composição original preservada; somente fundos refeitos. Ciantia: modelo da expansão ainda bloqueada.</p><p><a href="../../../arte/cartas-site/galeria.html">Todas as 94 artes concluídas</a> · <a href="../../../arte/cartas-site/variantes/conjuradores-cenas-para-cartas-futuras/galeria.html">Nove cenas reservadas para cartas futuras</a></p><main>'+imagens.map(i=>'<article><h2>'+i.nome+'</h2><img src="'+path.relative(modelos,path.join(repo,i.arquivo)).split(path.sep).join('/')+'"></article>').join('')+'</main>','utf8');
console.log(JSON.stringify({conjuradores:conjuradores.length,ciantia:variantes.find(v=>v.selecionada).arquivo,alfaCiantia:variantes.find(v=>v.selecionada).alfa,concluidas:prontas.length,meta:fila.meta,entrega:'Arcanoclisma/cartas/entregas/conjuradores-fundos-2026-10-04.visual.json'}));

