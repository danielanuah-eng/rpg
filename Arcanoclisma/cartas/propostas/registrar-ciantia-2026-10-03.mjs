// Registro local autorizado pelo autor. Não altera o motor, gera artes ou publica no Git.
import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const here=path.dirname(fileURLToPath(import.meta.url));
const cartas=path.resolve(here,'..');
const arena=path.resolve(cartas,'../arena');
const base=path.join(cartas,'modelos-visuais/2026-10-03-v1');
const proposal='Arcanoclisma/cartas/propostas/ciantia-dragoes-2026-10-03.autor.json';
const read=async file=>JSON.parse(await fs.readFile(file,'utf8'));
const write=async(file,value)=>fs.writeFile(file,JSON.stringify(value,null,2)+'\n');

const data=await read(path.join(base,'modelos.json'));
const history=path.join(base,'modelos-antes-propostas-autor.json');
try{await fs.writeFile(history,JSON.stringify(data,null,2)+'\n',{flag:'wx'});}catch(e){if(e.code!=='EEXIST')throw e;}
data.versao=2;data.estado='artes_aprovadas_propostas_aguardando_claude';data.propostaAutor=proposal;
for(const model of data.cartas){
  model.avaliacaoArte='aprovada_pelo_autor';model.estadoCarta='proposta_aguardando_claude';
  model.propostaAutor=proposal;model.identidadeCanonica=false;model.camposProvisorios=true;
  model.observacao='Arte aprovada. Habilidades propostas pelo autor; números e redação final a definir pelo Claude. Raridade só estética.';
  if(model.id==='licao-do-jardim'){
    model.subtipos=['Feitiço'];model.velocidade='findante';
    model.texto='Seus Dragões podem se suspender para gerar 1 Arcano em velocidade Repentina. Até o fim do turno, seus Dragões contribuem para o número de Grau.';
  }else if(['dragao-da-travessia','dragao-transplanar'].includes(model.id)){
    model.idEstudoAnterior='dragao-da-travessia';model.id='dragao-transplanar';model.nome='Dragão Transplanar';
    model.vinculoProposto={equipamento:'Extra',conjurador:'Ciantia',requerDesenvolvimento:true};
    model.texto='Extra exclusivo de Ciantia desenvolvida. Jogue um Dragão da sua mão sem pagar seus custos, respeitando o Grau necessário. [Proteção a definir pelo Claude.]';
  }
}
await write(path.join(base,'modelos.json'),data);
await fs.writeFile(path.join(base,'modelos-cenas.js'),'window.ARENA_CENAS='+JSON.stringify(data.cartas)+';\n');

const manifest=await read(path.join(base,'manifesto.json'));
manifest.versao=2;manifest.estado=data.estado;manifest.propostaAutor=proposal;
for(const art of manifest.artes)if(art.vigente)art.aprovacao='Arte aprovada pelo autor';
manifest.composicao='Prévia de propostas de cartas com artes aprovadas; integração mecânica aguardando Claude.';
manifest.dadosMecanicos='Habilidades propostas pelo autor no arquivo vinculado; números pendentes aparecem como —, não como zero.';
manifest.suspensaoConjurador='Atacar/Bloquear usam animateMotion no wrapper; retornam suspensos em perspectiva. Suspender/Preparar usam setSuspended.';
await write(path.join(base,'manifesto.json'),manifest);

const elements=await read(path.join(base,'elementos-derivaveis.json'));
for(const element of elements.elementos)if(element.modeloDerivado==='dragao-da-travessia'){
  element.modeloDerivado='dragao-transplanar';element.estado='proposta_do_autor_aguardando_claude';element.propostaAutor=proposal;
}
await write(path.join(base,'elementos-derivaveis.json'),elements);

const delivery=await read(path.join(cartas,'entregas/modelos-cenarios-2026-10-03.visual.json'));
delivery.versao=2;delivery.propostaAutor=proposal;delivery.artesAprovadas=['licao-do-jardim','dragao-transplanar'];
delivery.arteAprovada='dragao-transplanar';delivery.integracaoConfirmada=false;
await write(path.join(cartas,'entregas/modelos-cenarios-2026-10-03.visual.json'),delivery);

const resume=await read(path.join(arena,'retomada.json'));
resume.propostasCiantia={estado:data.estado,arquivo:proposal,artesAprovadas:true,integracaoConfirmada:false};
resume.conjuradorMovimento={arquivo:'modelos-visuais-ui.js',acoes:['attack','block'],wrapper:'.board-position.self.conjurador',final:'suspensa',integracaoMotorConfirmada:false};
await write(path.join(arena,'retomada.json'),resume);
console.log('Duas artes aprovadas registradas como propostas para Claude; modelos e metadados locais atualizados.');
