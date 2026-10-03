import fs from 'node:fs/promises';import path from 'node:path';import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..'),repo=path.resolve(root,'../..');
const read=async f=>JSON.parse(await fs.readFile(path.join(root,f),'utf8')),write=async(f,d)=>fs.writeFile(path.join(root,f),JSON.stringify(d,null,2)+'\n');
const heading='## Diretriz obrigatória de acabamento v9';
const body='Direção atual do autor (03/10/2026): raridade é somente estética, sem alterar regras/efeitos/números. Claude informa raridade e arteAlternativa em todo pedido/prompt de imagem de carta. Toda lendária ou alternativa inclui iluminação, textura e brilho na ilustração conforme Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md; salvar o prompt e os parâmetros cosméticos. Alternativa pode sugerir volume 3D com luz, escorço e planos, mas fica inteiramente dentro da carta. Manter identidade, direção de arte e cenas novas para personagens existentes. O acabamento HTML/CSS complementa a arte; não substitui a iluminação pedida na geração. Não refazer automaticamente artes aprovadas.';
for(const file of ['Arcanoclisma/cartas/COORDENACAO-CLAUDE.md','.agents/skills/designer-cartas/SKILL.md','.agents/skills/produtor-de-cartas/SKILL.md','.agents/skills/carlos/SKILL.md','.agents/skills/embelezador/SKILL.md']){
 const target=path.join(repo,file),source=await fs.readFile(target,'utf8');
 if(!source.includes(heading))await fs.appendFile(target,'\n'+heading+'\n\n'+body+'\n');
}
for(const file of ['../cartas/modelos/pedido.claude.exemplo.json','../cartas/modelos/lote.claude.exemplo.json']){
 const data=await read(file),rows=data.cartas||[data.carta];
 for(const row of rows){
  const dst=row.cosmetico||row;
  if(!['comum','incomum','rara','lendaria'].includes(dst.raridade))dst.raridade='comum';
  dst.arteAlternativa??=false;
  row.acabamentoVisual='arcanoclisma-v9';row.diretrizAcabamento='Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md';
 }
 data.instrucoesAcabamento='Definir raridade e arteAlternativa de cada carta; aplicar o padrão luminoso v9 a toda lendária/alternativa e incluir os campos no prompt completo. Raridade somente estética.';
 await write(file,data);
}
const spec=await read('acabamento-v9.json');
await write('../cartas/entregas/acabamento-2026-10-03.visual.json',{tarefaId:'acabamento-v9',autorPedido:'Usuário',destinatario:'Claude',estado:'disponivel_localmente',integracaoConfirmada:false,versao:9,apenasEstetica:true,diretriz:'Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md',arquivos:['Arcanoclisma/arena/acabamento-v9.css','Arcanoclisma/arena/acabamento-v9.json','Arcanoclisma/arena/index.html','Arcanoclisma/arena/arena.js','Arcanoclisma/arena/vetores/foil-arcoiris-dourado-v9.svg','Arcanoclisma/arena/vetores/foil-prata-roxo-v9.svg'],acabamento:spec,prompts:{raridadeObrigatoria:true,arteAlternativaObrigatoria:true,brilhoObrigatorioQuando:'lendaria ou arteAlternativa=true'},push:'Não autorizado nesta revisão; perguntar ao autor antes de publicar.',canal:'Arquivos locais do projeto; não confirma contato com conversa privada nem sincronização remota.'});
console.log('Diretrizes, modelos de pedidos e entrega local para Claude atualizados.');
