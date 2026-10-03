import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const here=path.dirname(fileURLToPath(import.meta.url));
const repo=path.resolve(here,'../../../..');
const ler=async arquivo=>JSON.parse(await fs.readFile(arquivo,'utf8'));
const salvar=async(arquivo,dados)=>fs.writeFile(arquivo,JSON.stringify(dados,null,2)+'\n','utf8');
const decisao={data:'2026-10-03',autor:'Autor nesta conversa',texto:'o v2 ficou melhor',varianteSelecionada:'v2-liso',escopo:'ambas as cartas de Philgard'};
const arquivoModelos=path.join(here,'modelos.json');
const modelos=await ler(arquivoModelos);
modelos.versao=2;
modelos.estado='artes_aprovadas_dados_pendentes_claude';
modelos.aprovacao=decisao;
for(const carta of modelos.cartas) {
 const escolhida=carta.variantes.find(v=>v.variante==='v2-liso');
 if(!escolhida)throw new Error('V2 ausente: '+carta.id);
 carta.versaoSelecionada='v2-liso';carta.artePrincipal=escolhida.arquivo;carta.aprovacaoArte='aprovada_pelo_autor';
 for(const variante of carta.variantes) {variante.estado=variante.variante==='v2-liso'?'aprovada_pelo_autor':'anterior_preservada';variante.principal=variante.variante==='v2-liso';}
}
modelos.pendencias=modelos.pendencias.filter(p=>!p.startsWith('Autor escolher'));
await salvar(arquivoModelos,modelos);
const arquivoManifesto=path.join(here,'manifesto.json');
const manifesto=await ler(arquivoManifesto);
manifesto.versao=2;manifesto.estado=modelos.estado;manifesto.aprovacao=decisao;
for(const imagem of manifesto.imagens) {imagem.estado=imagem.variante==='v2-liso'?'aprovada_pelo_autor':'anterior_preservada';imagem.principal=imagem.variante==='v2-liso';}
await salvar(arquivoManifesto,manifesto);
const arquivoEntrega=path.join(repo,'Arcanoclisma/cartas/entregas/philgard-2026-10-03.visual.json');
const entrega=await ler(arquivoEntrega);
entrega.versao=2;entrega.estado='artes_aprovadas_aguardando_claude';entrega.aprovacao=decisao;
entrega.cartas=modelos.cartas.map(c=>({id:c.id,nome:c.nome||c.nomeProvisorio,versaoSelecionada:c.versaoSelecionada,artePrincipal:c.artePrincipal,aprovacaoArte:c.aprovacaoArte,variantes:c.variantes}));
for(const arquivo of entrega.arquivos) {arquivo.principal=arquivo.variante==='v2-liso';arquivo.estado=arquivo.principal?'aprovada_pelo_autor':'anterior_preservada';}
entrega.paraClaude=entrega.paraClaude.map(p=>p.startsWith('Aguardar escolha do autor')?'Usar V2-liso como arte principal nas duas cartas; aprovadas pelo autor. Tipo, custo, Grau, Arcano gerado, Valores, habilidades e raridade continuam pendentes.':p);
entrega.paraClaude.push('Preferência estética confirmada: cenários mais lisos e menos rugosos, com menos textura expressiva e contraste de microdetalhes; conservar profundidade e detalhes dos personagens.');
await salvar(arquivoEntrega,entrega);
console.log(JSON.stringify({estado:modelos.estado,aprovadas:modelos.cartas.map(c=>({id:c.id,variante:c.versaoSelecionada})),push:false}));

