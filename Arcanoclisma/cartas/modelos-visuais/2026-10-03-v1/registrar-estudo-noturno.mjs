// Preservação mínima da geração: cópia sem alterar pixels, medidas/alfa, prompt e descrição.
import fs from 'node:fs/promises';
import {constants} from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {createRequire} from 'node:module';

const base=path.dirname(fileURLToPath(import.meta.url));
const source='C:/Users/Anuah/.codex/generated_images/01a0f911-59e9-7d32-b27a-67ead45442f5/exec-b008b874-65fa-4300-82b5-ac298cfa89c3.png';
const filename='licao-do-jardim-estudo-noturno-v1.png';
const target=path.join(base,filename);
try{await fs.copyFile(source,target,constants.COPYFILE_EXCL);}catch(e){if(e.code!=='EEXIST')throw e;}
const sharp=createRequire(import.meta.url)('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const buf=await fs.readFile(target),meta=await sharp(buf).metadata();
const {data,info}=await sharp(buf).ensureAlpha().raw().toBuffer({resolveWithObject:true});
let min=255,max=0,transparent=0;
for(let k=3;k<data.length;k+=4){min=Math.min(min,data[k]);max=Math.max(max,data[k]);if(data[k]===0)transparent++;}
const relative='Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/'+filename;
const study={
  versao:1,data:'2026-10-03',cartaId:'licao-do-jardim',estado:'estudo_visual_aguardando_avaliacao',
  arquivo:relative,ferramenta:'image_gen.imagegen integrada',origemGerada:source,
  referencia:'Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/licao-do-jardim-arte-v2.png',
  prompt:'licao-do-jardim-estudo-noturno.prompt.txt',
  cosmetico:{raridade:'rara',arteAlternativa:false},
  dimensoes:[meta.width,meta.height],sha256:crypto.createHash('sha256').update(buf).digest('hex'),
  alfa:{hasAlpha:meta.hasAlpha,min,max,pixelsTransparentes:transparent,cantos:[[0,0],[info.width-1,0],[0,info.height-1],[info.width-1,info.height-1]].map(([x,y])=>data[(y*info.width+x)*4+3])},
  fundo:'Cenário completo opaco intencional',
  descricao:'Ciantia cuida de uma planta junto ao pequeno dragão dourado, num jardim de vegetação e pedra à noite. Lua visível, folhas úmidas, lanternas discretas e água com reflexos; árvores substituem a cidade distante. Cena tranquila, com luz focal no dragão e no broto.',
  ambiente:'jardim botânico com água e vegetação, escala íntima',momentoDoDia:'noite com luar',
  clima:'folhas molhadas, reflexos e gotas/ripples visíveis; chuva recente como interpretação visual',
  escala:'personagem e dragão pequeno legíveis; plantas, degraus e água dão apoio e profundidade',
  eventoId:null,continuidadeCom:['licao-do-jardim'],
  relacaoComEvento:'Estudo de outro horário e ambientação para a mesma carta; cronologia canônica não definida',
  alteracoes:['dia para noite com luar','vegetação/água no fundo','remoção da cidade e das pontes distantes','ajuste de pose/mão e iluminação'],
  originalAprovadoPreservado:true,substituiVersaoVigente:false,integracaoClaudeConfirmada:false,
};
await fs.writeFile(path.join(base,'licao-do-jardim-estudo-noturno.visual.json'),JSON.stringify(study,null,2)+'\n');
const manifestPath=path.join(base,'manifesto.json');
const manifest=JSON.parse(await fs.readFile(manifestPath,'utf8'));
manifest.estudosVisuais=[...(manifest.estudosVisuais||[]).filter(s=>s.arquivo!==relative),{arquivo:relative,registro:'licao-do-jardim-estudo-noturno.visual.json',estado:study.estado}];
await fs.writeFile(manifestPath,JSON.stringify(manifest,null,2)+'\n');
const auditPath=path.resolve(base,'../../entregas/revisao-cenarios-2026-10-03.visual.json');
const audit=JSON.parse(await fs.readFile(auditPath,'utf8'));
audit.estudoNoturno={arquivo:relative,registro:'Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/licao-do-jardim-estudo-noturno.visual.json',estado:study.estado};
await fs.writeFile(auditPath,JSON.stringify(audit,null,2)+'\n');
console.log(JSON.stringify({arquivo:filename,dimensoes:study.dimensoes,alfa:study.alfa,estado:study.estado}));
