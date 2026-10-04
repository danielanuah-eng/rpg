// Copia apenas os componentes novos para o site e documenta a entrega.
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
const pasta=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(pasta,'../..');
const assets=path.join(pasta,'assets/abertura-partida/v01');
const destino=path.join(repo,'Arcanoclisma/online/public/kit/abertura-v14');
await fs.mkdir(destino,{recursive:true});
for(const arquivo of ['d20-modelo.js','abertura.js','abertura.css'])await fs.copyFile(path.join(assets,arquivo),path.join(destino,arquivo));
const rel=p=>path.relative(repo,p).split(path.sep).join('/');
async function listar(pasta){
 const itens=await fs.readdir(pasta,{withFileTypes:true}),arquivos=[];
 for(const item of itens.sort((a,b)=>a.name.localeCompare(b.name))){
  const p=path.join(pasta,item.name);
  if(item.isDirectory())arquivos.push(...await listar(p));else if(item.isFile())arquivos.push(p);
 }
 return arquivos;
}
const paths=[...await listar(assets),...await listar(destino),
 path.join(pasta,'construir-abertura.mjs'),fileURLToPath(import.meta.url),
 path.join(pasta,'previas/abertura-partida/index.html')];
const arquivos=[];
for(const p of paths){const bytes=await fs.readFile(p);arquivos.push({arquivo:rel(p),bytes:bytes.length,sha256:crypto.createHash('sha256').update(bytes).digest('hex')});}
const modelo=JSON.parse(await fs.readFile(path.join(assets,'d20-modelo.json'),'utf8'));
const entrega={
 tarefaId:'abertura-partida',versao:1,autor:'Codex — função Embelezador',criadaEm:new Date().toISOString(),
 origemPedido:'Autor nesta conversa; Arcanoclisma/efeitos/PRIORIDADES-04-10.md',
 estado:'produzida_aguardando_avaliacao_visual_e_integracao',integracaoClaudeConfirmada:false,
 previa:'Arcanoclisma/efeitos/previas/abertura-partida/index.html',
 referenciaVisual:'Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/dragao-da-travessia-arte-v1.png',
 cores:modelo.paleta,geometria:modelo.geometria,faces:modelo.faces.map(f=>({numero:f.numero,oposta:21-f.numero,indices:f.vertices,orientacaoFrontal:f.orientacao})),
 medicoesFaces:modelo.medicoes,arquivos,
 animacoes:[
  {id:'d20-rolagem-confirmada',tipo:'3D procedural executável',duracaoMs:4200,
   fases:['preparação','giro e deslocamento','contatos na superfície','desaceleração','acomodação','leitura'],
   contatosNormalizados:[.48,.64,.78,.90],resultado:'número 1–20 fornecido pelo motor, nunca sorteado por este componente',
   clipeGLB:'rolagem-exemplo-20',regraDoClipe:'exemplo fixo em 20, não determina jogador inicial'},
  {id:'arena-entrada-gradual',tipo:'animação de wrappers pela Web Animations API',
   duracaoGrupoMs:1600,intervaloGruposMs:180,duracaoTotalMaximaMs:2320,
   grupos:['piso','zonas','conjuradores','baralhosMao','controles'],
   repetirPorRedesenho:false,ocultas:'mesmo verso; sem acesso a identidade privada'}
 ],
 acessibilidade:{movimentoReduzido:'face/arena final direta',semWebGL:'face ilustrada final',audioAutomatico:false},
 vinculosClaude:[
  {acao:'d20-rolagem-confirmada',comandoReal:null,estado:'pendente_claude',integracao:'numero confirmado do motor -> D20Transplanar.rolar(numero)'},
  {acao:'arena-entrada-gradual',comandoReal:null,estado:'pendente_claude',integracao:'abertura real da tela/partida -> entrarArena(root,grupos,{partidaId})'}
 ],
 validacao:{construcaoLocal:'GLTF/GLB e PNGs construídos pelo script local',alfaFacesMedido:true,
  interfaceTestada:false,jogoIntegrado:false},
 procedimentos:['construir-abertura.mjs','empacotar-abertura.mjs'],
 instrucoesClaude:'coordenacao/site-online/ENTREGA-ABERTURA-D20-04-10.md',
 restricoes:['Não alterar regras, custos, primeiro jogador ou timers.','Expansão A Mestra dos Dragões segue bloqueada para jogo.'],
 usoIAExterna:false,pushAutorizado:true
};
await fs.mkdir(path.join(pasta,'entregas'),{recursive:true});
await fs.writeFile(path.join(pasta,'entregas/abertura-partida.visual.json'),JSON.stringify(entrega,null,2)+'\n');
console.log(JSON.stringify({estado:entrega.estado,faces:modelo.faces.length,arquivos:arquivos.length,componentes:rel(destino),previa:entrega.previa}));

