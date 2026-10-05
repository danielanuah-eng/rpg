import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {createRequire} from 'node:module';
const pasta=path.dirname(fileURLToPath(import.meta.url)),repo=path.resolve(pasta,'../../..');
const sharp=createRequire(path.join(repo,'Arcanoclisma/online/package.json'))('sharp');
const config=JSON.parse(await fs.readFile(path.join(pasta,'lote.json'),'utf8'));
const dados=JSON.parse(await fs.readFile(path.join(repo,'Arcanoclisma/online/public/jogo/dados.json'),'utf8'));
const fonte=await fs.readFile(path.join(repo,config.escopoIdentificadoEm));
const cartas=[];
for(const entrada of config.cartas){
  const imagem=await fs.readFile(path.join(repo,entrada.arquivo));
  const meta=await sharp(imagem).metadata();
  const {data,info}=await sharp(imagem).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let min=255,max=0,transparentes=0;
  for(let k=3;k<data.length;k+=info.channels){min=Math.min(min,data[k]);max=Math.max(max,data[k]);if(data[k]===0)transparentes++;}
  if(min!==255)throw new Error('Cena não é totalmente opaca: '+entrada.id);
  const registro={
    ...entrada,origem:entrada.origem,ferramenta:'image_gen.imagegen integrada',modo:'built-in',
    estado:'entregue_aguardando_avaliacao_do_autor',baseInspecionada:true,
    dimensoes:[meta.width,meta.height],dimensoesEntregues:'PNG nativo preservado sem redimensionamento',
    proporcaoSolicitada:'63:88',proporcaoReal:meta.width/meta.height,
    sha256:createHash('sha256').update(imagem).digest('hex'),
    alfa:{hasAlpha:meta.hasAlpha??false,min,max,pixelsTransparentes:transparentes,total:info.width*info.height},
    fundo:'Cena completa opaca',raridade:dados.visual[entrada.id]?.raridade??null,arteAlternativa:false,
    definicaoAtualClaude:dados.catalogo.cartas[entrada.id]??null,
    referenciaCodigo:config.escopoIdentificadoEm,fonteCodigoSha256:createHash('sha256').update(fonte).digest('hex'),
    alteracoesMecanicas:false,integracaoClaudeConfirmada:false,
    movimento:'Aliado: reutilizar ataque/bloqueio genéricos existentes; nenhum projétil exclusivo foi inventado.'
  };
  await fs.writeFile(path.join(pasta,entrada.id+'.visual.json'),JSON.stringify(registro,null,2)+'\n');
  cartas.push(registro);
}
const entrega={
  tarefaId:config.tarefaId,versao:1,data:config.data,autor:'Codex',estado:'entregue',
  escopo:'Pequeno lote de 3 cenas para cartas provisórias novas do Claude, fora das 210 artes já aprovadas.',
  escopoIdentificadoEm:config.escopoIdentificadoEm,respostaFormalClaudeRecebida:false,meta:3,concluidas:cartas.length,
  cartas:cartas.map(c=>({id:c.id,nome:c.nome,arquivo:c.arquivo,arteModo:'cena',prompt:c.prompt,
    registro:'Arcanoclisma/arte/cartas-rapidas/'+c.id+'.visual.json',sha256:c.sha256,
    dimensoes:c.dimensoes,alfa:c.alfa,raridade:c.raridade,arteAlternativa:false,
    estado:c.estado,integracaoClaudeConfirmada:false})),
  galeria:'Arcanoclisma/arte/cartas-rapidas/galeria.html',pushAutorizadoPeloAutor:true,
  instrucoesClaude:[
    'Integrar cada PNG somente à carta de mesmo ID; cenário já faz parte da imagem, sem montagem de recorte com fundo.',
    'Preservar o PNG nativo como fonte de alta resolução; converter para WebP no build quando necessário.',
    'Não alterar regras, custos, Grau, Valores, raridades ou o catálogo por causa desta entrega.',
    'As 210 artes já aprovadas e a expansão A Mestra dos Dragões não foram modificadas.',
    'Confirmar integração em cartas/entregas/artes-rapidas-jogo.integracao.claude.json; não há confirmação automática.'
  ]
};
await fs.writeFile(path.join(repo,'Arcanoclisma/cartas/entregas/artes-rapidas-jogo.visual.json'),JSON.stringify(entrega,null,2)+'\n');
const esc=s=>String(s).replace(/[&<>"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;'}[c]));
const html='<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Artes rápidas — 3 cartas</title><style>body{background:#111b2a;color:#e9e2d5;font-family:system-ui;margin:0;padding:24px}h1{margin-top:0}p{color:#c4cbcf}.grade{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:20px;max-width:1260px;margin:auto}article{background:#1c2c3b;border:1px solid #6a7280;border-radius:12px;padding:12px}img{width:100%;height:auto;border-radius:8px;display:block}h2{font-size:20px;margin:12px 0 6px}small{overflow-wrap:anywhere}a{color:#d6b97f}@media(max-width:720px){.grade{grid-template-columns:1fr}}</style><h1>Artes rápidas do Arcanoclisma</h1><p>3 cenas novas · PNGs nativos · aguardando avaliação visual e integração do Claude</p><section class="grade">'+cartas.map(c=>'<article><a href="'+esc(path.basename(c.arquivo))+'"><img src="'+esc(path.basename(c.arquivo))+'" alt="'+esc(c.nome)+'"></a><h2>'+esc(c.nome)+'</h2><small>'+esc(c.id)+' · '+c.dimensoes.join(' × ')+' px</small><p>'+esc(c.descricaoVisivel)+'</p></article>').join('')+'</section></html>';
await fs.writeFile(path.join(pasta,'galeria.html'),html);
console.log(JSON.stringify({concluidas:cartas.length,cartas:cartas.map(c=>({id:c.id,dimensoes:c.dimensoes,alfa:c.alfa,definicaoAtualEncontrada:!!c.definicaoAtualClaude}))}));
