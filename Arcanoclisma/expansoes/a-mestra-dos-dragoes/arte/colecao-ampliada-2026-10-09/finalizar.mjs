import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
const root=path.dirname(fileURLToPath(import.meta.url));
const plan=JSON.parse(await fs.readFile(path.join(root,'fila.json'),'utf8'));
if(plan.concluidas!==65||plan.itens.some(x=>!x.arquivo))throw Error('Lote incompleto');
const pairs=plan.itens.filter(x=>x.valores).map(x=>[...x.valores].sort().join('+'));
if(new Set([...pairs,...plan.paresOcupados]).size!==36||pairs.length!==25)throw Error('Cobertura de pares inválida');
for(const item of plan.itens){
 const bytes=await fs.readFile(path.join(root,item.arquivo));
 if(createHash('sha256').update(bytes).digest('hex')!==item.sha256)throw Error('Hash inválido: '+item.id);
 const record=JSON.parse(await fs.readFile(path.join(root,item.registro),'utf8'));
 if(record.alfa.min!==255||record.dimensoes[0]<1000)throw Error('Arquivo visual inválido: '+item.id);
}
const parent=path.dirname(root),old=JSON.parse(await fs.readFile(path.join(parent,'lore-ciantia-2026-10-09.visual.json'),'utf8'));
const added=plan.itens.map(x=>({id:x.id,nomeVisual:x.nomeVisual,categoriaVisual:x.categoriaVisual,arquivo:path.basename(root)+'/'+x.arquivo,prompt:path.basename(root)+'/'+x.prompt,registro:path.basename(root)+'/'+x.registro,sha256:x.sha256,dimensoes:x.dimensoes,valores:x.valores,postura:x.postura,estado:x.estado}));
const total=[...old.itens,...added];if(total.length!==90)throw Error('Total deve ser 90');
await fs.writeFile(path.join(parent,'colecao-completa-90.visual.json'),JSON.stringify({id:'mestra-dos-dragoes-90-artes-2026-10-09',estado:'aguardando_avaliacao',total:90,origem:'25 anteriores + 65 novas',liberadaParaJogo:false,pushAutorizado:true,galeria:path.basename(root)+'/galeria.html',escolhasDoAutor:old.escolhasDoAutor,animacoes:'pendentes de efeito e integração autorizados; registros individuais mantêm ID e hash',itens:total},null,2)+'\n');
const queuePath=path.join(parent,'../fila.json'),queue=JSON.parse(await fs.readFile(queuePath,'utf8'));
for(const item of plan.itens){const stripped={...item};stripped.prompt='arte/'+path.basename(root)+'/'+item.prompt;stripped.registro='arte/'+path.basename(root)+'/'+item.registro;stripped.arquivo='arte/'+path.basename(root)+'/'+item.arquivo;const index=queue.itens.findIndex(x=>x.id===item.id);if(index<0)queue.itens.push(stripped);else queue.itens[index]=stripped;}
queue.ultimaAmpliacao='colecao-ampliada-2026-10-09';queue.artesSelecionadasNestePedido=90;
await fs.writeFile(queuePath,JSON.stringify(queue,null,2)+'\n');
console.log(JSON.stringify({novas:65,total:90,paresRestantes:25,coberturaTotal:36,categorias:plan.itens.reduce((a,x)=>(a[x.categoriaVisual]=(a[x.categoriaVisual]??0)+1,a),{}),arquivosVerificados:65}));
