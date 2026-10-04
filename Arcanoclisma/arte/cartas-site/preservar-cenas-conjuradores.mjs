import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const here=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(here,'../../..');
const fila=JSON.parse(await fs.readFile(path.join(here,'fila.json'),'utf8'));
const destino=path.join(here,'variantes/conjuradores-cenas-para-cartas-futuras');
await fs.mkdir(destino,{recursive:true});
const indice=path.join(destino,'catalogo.json');
try{await fs.access(indice);console.log('Catálogo anterior já preservado.');process.exit(0);}catch{}
const cenas=[];
for(const item of fila.itens.filter(i=>i.tipo==='conjurador')){
 const registro=JSON.parse(await fs.readFile(path.join(repo,item.registro),'utf8'));
 const prompt=await fs.readFile(path.join(repo,registro.prompt),'utf8');
 const nomePrompt=item.id+'.prompt.txt',nomeRegistro=item.id+'.visual.json';
 await fs.writeFile(path.join(destino,nomePrompt),prompt,'utf8');
 await fs.writeFile(path.join(destino,nomeRegistro),JSON.stringify({...registro,prompt:'Arcanoclisma/arte/cartas-site/variantes/conjuradores-cenas-para-cartas-futuras/'+nomePrompt,papelAtual:'Opção de cena para carta futura; não usar como substituição do retrato de conjurador'},null,2)+'\n','utf8');
 try{await fs.copyFile(path.join(here,'registros',item.id+'.entrada.json'),path.join(destino,item.id+'.entrada.json'));}catch(e){if(e.code!=='ENOENT')throw e;}
 cenas.push({id:item.id,nome:item.nome,arquivo:item.arteNova,registro:'Arcanoclisma/arte/cartas-site/variantes/conjuradores-cenas-para-cartas-futuras/'+nomeRegistro,prompt:'Arcanoclisma/arte/cartas-site/variantes/conjuradores-cenas-para-cartas-futuras/'+nomePrompt,uso:'Cartas futuras com novas poses e eventos',aprovacaoFinal:false,regras:'Não cria carta nem habilidade; Claude define uma carta futura conforme pedido do autor.'});
}
await fs.writeFile(indice,JSON.stringify({pedidoAutor:'Guardar as versões anteriores dos conjuradores com cenas e poses diferentes para cartas futuras.',imagensOriginaisPreservadas:true,cenas},null,2)+'\n','utf8');
await fs.writeFile(path.join(destino,'galeria.html'),'<!doctype html><meta charset="utf-8"><title>Cenas reservadas para cartas futuras</title><style>body{background:#17202b;color:#eee;font:16px system-ui;margin:30px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(230px,1fr));gap:24px}img{width:100%;border-radius:12px}h2{font-size:17px}</style><h1>Cenas reservadas para cartas futuras</h1><p>Versões anteriores preservadas por pedido do autor. Não substituem os retratos de conjurador.</p><main>'+cenas.map(c=>'<article><h2>'+c.nome+'</h2><img src="../../'+path.basename(c.arquivo)+'"></article>').join('')+'</main>','utf8');
console.log(JSON.stringify({cenas:cenas.length,indice}));

