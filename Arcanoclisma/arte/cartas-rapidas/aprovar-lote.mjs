import fs from 'node:fs/promises';
import path from 'node:path';
import {createHash} from 'node:crypto';
import {fileURLToPath} from 'node:url';

const folder=path.dirname(fileURLToPath(import.meta.url));
const repo=path.resolve(folder,'../../..');
const library=process.argv[2];
if(!library)throw new Error('Informe a biblioteca local de Enzo.');
const read=async f=>JSON.parse(await fs.readFile(f,'utf8'));
const save=async(f,j)=>fs.writeFile(f,JSON.stringify(j,null,2)+'\n');
const hash=b=>createHash('sha256').update(b).digest('hex');
const relative=f=>path.relative(repo,f).replaceAll('\\','/');
const queue=await read(path.join(folder,'fila-valores.json'));
const initial=await read(path.join(folder,'lote.json'));
const initialDeliveryPath=path.join(repo,'Arcanoclisma/cartas/entregas/artes-rapidas-jogo.visual.json');
const initialDelivery=await read(initialDeliveryPath);
const formalPath=path.join(repo,'Arcanoclisma/cartas/entregas/cartas-provisorias-v1.visual.json');
const formal=await read(formalPath);
const cards=[...initial.cartas,...queue.cartas];
if(cards.length!==formal.meta||cards.some(c=>!c.arquivo))throw new Error('Lote incompleto.');
for(const c of cards){
 c.registro??=initialDelivery.cartas.find(x=>x.id===c.id)?.registro;
 if(!c.registro)throw new Error('Registro ausente: '+c.id);
 const bytes=await fs.readFile(path.join(repo,c.arquivo));
 const record=await read(path.join(repo,c.registro));
 if(hash(bytes)!==record.sha256)throw new Error('Hash divergente: '+c.id);
 c.sha256=record.sha256;
}
const now=new Date().toISOString();
const decision={estado:'aprovado_pelo_autor',registradaEm:now,mensagemDoAutor:'aprovo',origem:'Conversa atual, após a apresentação da galeria completa.',escopo:'As 46 artes das cartas provisórias deste lote.',ids:cards.map(c=>c.id),versoes:cards.map(c=>({id:c.id,arquivo:c.arquivo,sha256:c.sha256})),expansaoLiberada:false,integracaoClaudeConfirmada:false};
const decisionPath=path.join(folder,'aprovacao-autor.json');
for(const c of cards){
 c.estado='aprovado_pelo_autor';c.aprovacao=decision.registradaEm;
 const rp=path.join(repo,c.registro),record=await read(rp);
 record.estado=c.estado;record.aprovacao={estado:c.estado,registradaEm:now,mensagemDoAutor:decision.mensagemDoAutor,decisao:relative(decisionPath)};
 await save(rp,record);
}
queue.estado='aprovado';queue.avaliacao=decision;queue.atualizadaEm=now;queue.pendentes=[];
initial.estado='aprovado';initial.avaliacao=decision;initial.atualizadaEm=now;
await save(path.join(folder,'fila-valores.json'),queue);await save(path.join(folder,'lote.json'),initial);await save(decisionPath,decision);
for(const file of [path.join(folder,'progresso-valores.json'),initialDeliveryPath,path.join(repo,'Arcanoclisma/cartas/entregas/artes-rapidas-valores.visual.json'),formalPath]){
 const j=await read(file);j.estado='aprovado';j.avaliacao=decision;j.atualizadaEm=now;
 for(const c of j.cartas??[]){const approved=cards.find(x=>x.id===c.id);if(approved){c.estado='aprovado_pelo_autor';c.aprovacao=now;c.registro=approved.registro;c.sha256=approved.sha256;}}
 if(file===formalPath){j.aprovacaoDoAutor='aprovado';j.publicacaoSiteAutorizada=true;j.escopoDaAprovacao='Uso destas 46 artes aprovadas nas cartas provisórias já solicitadas. A expansão A Mestra dos Dragões continua bloqueada; integração e publicação do jogo seguem com o Claude.';}
 await save(file,j);
}
await save(path.join(folder,'producao-valores.lock.json'),{ativa:false,atualizadaEm:now,motivo:'Lote aprovado pelo autor; ciclo de produção encerrado.'});
const gp=path.join(folder,'galeria.html');
await fs.writeFile(gp,(await fs.readFile(gp,'utf8')).replace('avaliação visual e integração em andamento','artes aprovadas pelo autor · integração com Claude pendente'));
const objectDir=path.join(library,'objetos');await fs.mkdir(objectDir,{recursive:true});
const indexPath=path.join(library,'indices/arcanoclisma-cartas-rapidas.json');
await fs.mkdir(path.dirname(indexPath),{recursive:true});
let index;
try{index=await read(indexPath);}catch(e){if(e.code!=='ENOENT')throw e;index={versao:1,projeto:'Arcanoclisma — cartas provisórias',origem:folder,assets:[],documentos:[]};}
if(index.origem!==folder)throw new Error('Origem do índice divergente.');
async function archive(file,kind,metadata={}){
 const bytes=await fs.readFile(file),sha256=hash(bytes),object='objetos/'+sha256+path.extname(file).toLowerCase();
 try{await fs.writeFile(path.join(library,object),bytes,{flag:'wx'});}catch(e){if(e.code!=='EEXIST')throw e;}
 const items=kind==='asset'?index.assets:index.documentos,origin=relative(file);
 let entry=items.find(x=>x.origem===origin&&x.sha256===sha256);
 if(!entry){entry={origem:origin,objeto:object,sha256,bytes:bytes.length,arquivadoEm:now};items.push(entry);}
 Object.assign(entry,metadata);return object;
}
for(const c of cards){
 const recordObject=await archive(path.join(repo,c.registro),'documento',{id:c.id});
 const promptObject=await archive(path.join(repo,c.promptArquivo??c.prompt),'documento',{id:c.id});
 await archive(path.join(repo,c.arquivo),'asset',{id:c.id,titulo:c.nome,valor:c.valor,criador:'Codex / image_gen, conforme registro',aprovacao:'aprovado_pelo_autor',aprovacaoRegistradaEm:now,registro:recordObject,prompt:promptObject,referencia:c.referencia,integracaoClaudeConfirmada:false});
}
for(const f of ['aprovacao-autor.json','fila-valores.json','lote.json','progresso-valores.json','cenas-valores.json','galeria.html'])await archive(path.join(folder,f),'documento');
for(const f of [initialDeliveryPath,formalPath,path.join(repo,'Arcanoclisma/cartas/entregas/artes-rapidas-valores.visual.json')])await archive(f,'documento');
index.atualizadoEm=now;await save(indexPath,index);
const version=now.replace(/[:.]/g,'-')+'-46-aprovadas';
const packageDir=path.join(library,'pacotes/arcanoclisma-cartas-rapidas',version);
await fs.mkdir(packageDir,{recursive:true});
const files=[];
for(const rel of [
'Arcanoclisma/arte/cartas-rapidas/valores.mjs',
'Arcanoclisma/arte/cartas-rapidas/registrar-lote.mjs',
'Arcanoclisma/arte/cartas-rapidas/aprovar-lote.mjs',
'Arcanoclisma/arte/cartas-rapidas/aprovacao-autor.json',
'Arcanoclisma/cartas/DIRETRIZ-CENARIOS.md',
'AGENTS.md',
'.agents/skills/carlos/SKILL.md',
'.agents/skills/designer-cartas/SKILL.md',
'.agents/skills/enzo/SKILL.md',
'.agents/skills/copiao/SKILL.md']){
 const target=path.join(packageDir,rel);await fs.mkdir(path.dirname(target),{recursive:true});
 const bytes=await fs.readFile(path.join(repo,rel));await fs.writeFile(target,bytes,{flag:'wx'});
 files.push({arquivo:rel,origem:rel,sha256:hash(bytes)});
}
const manifest={projeto:'Arcanoclisma — lote de 46 artes de cartas provisórias',versao:version,data:now,estado:'encerrado_aprovado',escopo:'Apenas este lote visual; o desenvolvimento do TCG continua.',aprovacao:decision,arquivos:files,indiceEnzo:'../../../indices/arcanoclisma-cartas-rapidas.json',ferramentas:[{nome:'image_gen.imagegen integrada',uso:'geração de cenas novas com referência de identidade',internet:true},{nome:'Node.js',uso:'registros, galeria, aprovação e preservação',internet:false},{nome:'Sharp',uso:'dimensões e alfa na entrega',internet:false},{nome:'Git',uso:'commits e push autorizados',internet:'somente push'}],procedimentos:{locais:['salvar PNG nativo','medir dimensões e alfa com Sharp','registrar por ID e hash','gerar galeria','arquivar por hash e registrar aprovação'],remotos:['gerar e editar imagens com image_gen integrada'],naoImplementados:['geração de imagens offline','assistente offline de jogos']},dependencias:['Node.js','Sharp instalado no projeto online para valores.mjs','acesso à image_gen integrada para geração'],pendencias:['Claude confirmar a integração das artes no jogo','efeitos particulares de magia ficam em entrega separada'],segredosIncluidos:false};
await save(path.join(packageDir,'manifesto.json'),manifest);
await fs.writeFile(path.join(packageDir,'ENTRADA.md'),'# Lote visual aprovado — 46 cartas provisórias\n\nEste pacote preserva somente a produção visual deste lote de Arcanoclisma. O TCG continua em desenvolvimento; a expansão A Mestra dos Dragões permanece bloqueada para inclusão em jogo.\n\n## Assets e aprovação\n\nConsultar ../../../indices/arcanoclisma-cartas-rapidas.json. Cada asset liga a imagem nativa, seu SHA-256, prompt, referência, registro e aprovação do autor. Os objetos ficam em ../../../objetos/. Não inventar integração confirmada: o Claude ainda precisa registrá-la.\n\n## Fluxo usado\n\n1. Ler o briefing do Claude e a referência de cada ID; conservar identidade, variar pose, ambiente e horário.\n2. Gerar uma cena vertical 63:88 completa e opaca com image_gen integrada, sem textos ou molduras. A geração usa serviço remoto; não funciona offline.\n3. No repositório original, executar node Arcanoclisma/arte/cartas-rapidas/valores.mjs registrar <ID> <PNG-nativo>. O script usa Sharp do projeto online para medir alfa e dimensões, preserva o PNG e registra o hash. Não regenerar IDs entregues.\n4. A galeria e os manifestos são atualizados pelo script. A aprovação foi registrada com aprovar-lote.mjs após a mensagem explícita do autor.\n5. Publicar somente arquivos visuais próprios com git add/commit --only e git push autorizados, preservando mudanças concorrentes do Claude.\n\n## Reutilização local\n\nNode.js, hashes, JSON, galeria e registro podem funcionar offline quando suas dependências já estiverem instaladas. Para executar os scripts, restaurar os caminhos e arquivos do projeto original; o pacote não inclui o jogo inteiro nem node_modules. Geração de imagens offline e decisões de uma IA local não foram implementadas. Não há pesos do Codex, chaves, configurações pessoais ou testes inventados neste pacote.\n');
console.log(JSON.stringify({estado:'aprovado',artes:cards.length,indice:indexPath,pacote:packageDir,integracaoClaudeConfirmada:false}));

