import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const read=async file=>JSON.parse(await fs.readFile(path.join(root,file),'utf8'));
const write=async(file,data)=>fs.writeFile(path.join(root,file),JSON.stringify(data,null,2)+'\n');
const appendOnce=async(file,heading,text)=>{
  const source=await fs.readFile(path.join(root,file),'utf8');
  if(!source.includes(heading))await fs.appendFile(path.join(root,file),'\n'+heading+'\n\n'+text+'\n');
};
const spec=await read('acabamento-v5.json');
spec.versao=6;
spec.raridades.lendaria={
  paleta:['dourado','rosa','violeta','azul','verde'],
  efeito:'Película holográfica texturizada sobre toda a face; destaque na superfície da carta.',
  brilho:3,
  opacidadeBorda:0.4,
  opacidadeTextura:0.86,
  opacidadeReflexo:[0.18,0.38],
  periodoSegundos:6,
  texturaSVG:'vetores/foil-lendaria-v6.svg',
  texturaPNG:'templates/foil-lendaria-v6.png',
  camada:3,
  mesclagem:'soft-light',
  registro:'registros/foil-lendaria-v6.json'
};
spec.numerosProtegidos='Película lendária na camada 3; textos e estatísticas na camada 4, recursos e marcadores na camada 5. Rara/incomum mantêm a máscara v5.';
spec.atributosVersao=5;
spec.canva='Overlays de atributos v5 continuam em rascunho, aguardando aprovação; película v6 é um asset separado aplicado no HTML.';
await write('acabamento-v6.json',spec);
for(const file of ['layout.json','modelo-online.json']){
  const data=await read(file);data.acabamento=spec;await write(file,data);
}
const templates=await read('../cartas/templates.json');
templates.revisaoVisual=6;
templates.revisaoAtual.acabamento=spec;
templates.canva.resultado='Revisão v5 dos atributos em rascunho; acabamento lendário v6 sobre a superfície no HTML/CSS.';
templates.acabamentoLendario={svg:'../arena/vetores/foil-lendaria-v6.svg',png:'../arena/templates/foil-lendaria-v6.png',especificacao:'../arena/acabamento-v6.json'};
await write('../cartas/templates.json',templates);
const delivery=await read('../cartas/entregas/arena-2026-10-03.visual.json');
delivery.revisaoAtual.versao=6;
delivery.revisaoAtual.acabamento=spec;
await write('../cartas/entregas/arena-2026-10-03.visual.json',delivery);
const state=await read('retomada.json');
state.revisaoAtual=6;
state.acabamentoLendario='Película holográfica v6 aplicada à face; SVG e PNG semitransparentes salvos.';
await write('retomada.json',state);
await appendOnce('AGENTS.md','## Acabamento lendário v6',
  'A orientação mais recente do autor exige brilho como textura sobre a carta. Usar foil-lendaria-v6 na face pública, com textos e números acima; halo externo de 3px e borda discreta. Rara/incomum e atributos mantêm v5. Consultar acabamento-v6.json. Não aplicar a cartas ocultas.');
await appendOnce('README.md','## Película lendária v6',
  'Textura holográfica dourada/arco-íris sobre a face, com microtrama e reflexo animado. Asset vetorial: vetores/foil-lendaria-v6.svg; exportação semitransparente: templates/foil-lendaria-v6.png (1260 × 1760). A configuração está em acabamento-v6.json. Overlays Canva de atributos continuam v5.');
await appendOnce('entrega-claude.md','## Lendária v6 — textura sobre a face',
  'O renderer público adiciona .foil-texture dentro de .front somente para data-rarity="lendaria". CSS usa vetores/foil-lendaria-v6.svg em soft-light, camada 3, cobrindo toda a face. Nome/regras/estatísticas ficam na camada 4 e recursos na 5. Reflexo dourado/arco-íris varia em 6 segundos; prefers-reduced-motion conserva uma película estática. Halo externo reduzido a 3px, borda com opacidade 0,4. Versos ocultos não recebem a camada.\n\nAsset reutilizável: templates/foil-lendaria-v6.png, 1260 × 1760, alfa mínimo 0/máximo 139, quatro cantos com alfa 0. Origem procedural SVG exportada com Sharp, sem modificar as artes. Gerar novamente com node scripts/criar-foil-lendaria-v6.mjs. A animação não foi executada no navegador nesta revisão. Rascunhos Canva de atributos continuam v5.');
console.log('Película v6 documentada; overlays Canva v5 preservados.');
