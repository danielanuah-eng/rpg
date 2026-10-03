import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const svg = (body,w=630,h=880,physical=false)=>`<svg xmlns="http://www.w3.org/2000/svg" width="${physical?'63mm':w}" height="${physical?'88mm':h}" viewBox="0 0 ${w} ${h}">${body}</svg>`;
const save = async (file, text) => fs.writeFile(path.join(root,file),text+'\n');
const glyphs = {
  'arcano-gerado':'<circle cx="32" cy="32" r="23" fill="currentColor"/><circle cx="32" cy="32" r="30" fill="none" stroke="currentColor" stroke-width="3"/>',
  'custo-arcano':'<circle cx="32" cy="32" r="26" fill="none" stroke="currentColor" stroke-width="6"/><path d="M32 2v12M32 50v12" stroke="currentColor" stroke-width="4"/>',
  'grau':'<path d="M32 4 60 32 32 60 4 32Z" fill="none" stroke="currentColor" stroke-width="4"/><path d="M18 44h10V33h10V21h10" fill="none" stroke="currentColor" stroke-width="4"/>',
  'ataque':'<path d="m14 50 9-20L49 7l8 8-23 26-20 9Zm9-20 11 11M8 45l11 11M12 52l-7 7" fill="none" stroke="currentColor" stroke-width="5" stroke-linejoin="round"/>',
  'vida':'<path d="M32 56C-12 28 10-6 32 14 54-6 76 28 32 56Z" fill="currentColor"/>',
  'aliado':'<circle cx="32" cy="18" r="12" fill="currentColor"/><path d="M9 59V46c0-23 46-23 46 0v13Z" fill="currentColor"/>',
  'temporaria':'<path d="M38 2 9 35h19l-2 27 29-35H36Z" fill="currentColor"/>',
  'mistica':'<path d="M32 3 41 22 61 32 41 41 32 61 23 41 3 32 23 22Z" fill="currentColor"/>',
  'estrutura':'<path d="M6 57h52M12 57V23h40v34M8 23 32 6l24 17M24 57V38h16v19" fill="none" stroke="currentColor" stroke-width="5"/>',
  'conjurador':'<path d="m7 21 11 9L32 7l14 23 11-9-7 25H14ZM14 54h36" fill="none" stroke="currentColor" stroke-width="5" stroke-linejoin="round"/>'
};
for(const [id,body] of Object.entries(glyphs)) await save(`vetores/icone-${id}.svg`,svg(`<g color="#f4e8d1">${body}</g>`,64,64));
const types={aliado:'#728d92',temporaria:'#a581a7',mistica:'#8d9b86',estrutura:'#a49377',conjurador:'#b7a36c'};
for(const [type,color] of Object.entries(types)) {
  // Janela de arte transparente; nome, números, bolinhas, custo e Grau são camadas HTML.
  const body=`<defs><linearGradient id="paper" x2="0" y2="1"><stop stop-color="#f4eddd" stop-opacity=".84"/><stop offset="1" stop-color="#e8dfcd" stop-opacity=".9"/></linearGradient></defs><rect x="7" y="7" width="616" height="866" rx="17" fill="none" stroke="#162333" stroke-width="6"/><path d="M7 7h616v82H7Z" fill="#152939" fill-opacity=".72"/><path d="M26 706q0-16 16-16h546q16 0 16 16v132q0 16-16 16H42q-16 0-16-16Z" fill="url(#paper)"/><g transform="translate(534 42) scale(.52)" color="#fff6e4">${glyphs[type]}</g></svg>`;
  await save(`templates/${type}.svg`,svg(body.replace('</svg>',''),630,880,true));
}
const rarity={comum:{cor:'#a9b3ba',forma:'<circle cx="32" cy="32" r="17"/>'},incomum:{cor:'#66bdb6',forma:'<path d="M32 9 55 32 32 55 9 32Z"/>'},rara:{cor:'#a4a2e8',forma:'<path d="M32 5 54 18v28L32 59 10 46V18Z"/>'},lendaria:{cor:'#dcba68',forma:'<path d="m32 4 7 18 21 1-16 13 6 22-18-12-18 12 6-22L4 23l21-1Z"/>'}};
for(const [id,r] of Object.entries(rarity)) await save(`vetores/raridade-${id}.svg`,svg(`<g fill="${r.cor}" stroke="#121e2d" stroke-width="3">${r.forma}</g>`,64,64));
const zones=['campo','grau','arcano','cemiterio','baralho','mao','pilha','conjurador'];
for(const id of zones) await save(`vetores/zona-${id}.svg`,svg(`<rect x="5" y="5" width="550" height="170" rx="20" fill="#142738" fill-opacity=".15" stroke="#b8b090" stroke-opacity=".44" stroke-width="2" stroke-dasharray="${id==='campo'?'0':'8 8'}"/><path d="M21 20h32M21 20v25M539 160h-32M539 160v-25" stroke="#d8c8a0" stroke-opacity=".7" stroke-width="2" fill="none"/>`,560,180));
await save('vetores/divisoria.svg',svg('<path d="M4 20h596M620 20h596" stroke="#c0ae85" stroke-width="2" opacity=".65"/><path d="m610 7 13 13-13 13-13-13Z" fill="#d8bd80"/><circle cx="610" cy="20" r="5" fill="#25374b"/>',1220,40));
await save('vetores/contador-vida.svg',svg('<circle cx="80" cy="80" r="68" fill="#101f30" stroke="#bea776" stroke-width="3"/><circle cx="80" cy="80" r="59" fill="none" stroke="#6e9da8" stroke-width="5"/><path d="M36 30 24 51M124 30l12 21M36 130l-12-21M124 130l12-21" stroke="#dfc795" stroke-width="2"/><path d="M80 125C55 108 66 89 80 99c14-10 25 9 0 26Z" fill="#a8cad0"/>',160,160));
await save('vetores/contador-arcano.svg',svg('<rect x="3" y="3" width="114" height="52" rx="26" fill="#102435" stroke="#789ba8" stroke-width="2"/><circle cx="28" cy="29" r="9" fill="#94bcc9"/><circle cx="28" cy="29" r="14" fill="none" stroke="#94bcc9" stroke-width="1"/>',120,58));
await save('vetores/pilha-baralho.svg',svg('<rect x="27" y="14" width="171" height="235" rx="13" fill="#101722" stroke="#c5b17f" stroke-width="2"/><path d="M25 242h173M23 235h174M20 226h175M17 217h177" stroke="#c7bdaa" stroke-width="2"/><rect x="12" y="3" width="171" height="235" rx="13" fill="#182d49" stroke="#d3b573" stroke-width="3"/>',210,265));
await save('vetores/sombra-carta.svg',svg('<defs><radialGradient id="s"><stop stop-color="#020b16" stop-opacity=".7"/><stop offset="1" stop-color="#020b16" stop-opacity="0"/></radialGradient></defs><ellipse cx="180" cy="70" rx="176" ry="66" fill="url(#s)"/>',360,140));
await save('vetores/base-mao.svg',svg('<path d="M8 118Q400 5 792 118" fill="none" stroke="#bdad87" stroke-width="2" opacity=".4"/><path d="M20 122Q400 25 780 122" fill="none" stroke="#536b85" stroke-width="18" opacity=".13"/>',800,150));
await save('vetores/verso-titulo.svg',svg('<defs><linearGradient id="gold" x2="0" y2="1"><stop stop-color="#dec491"/><stop offset=".45" stop-color="#d9b665"/><stop offset="1" stop-color="#9b7435"/></linearGradient></defs><text x="315" y="789" text-anchor="middle" font-family="Georgia,serif" font-size="35" font-weight="bold" letter-spacing="3" fill="url(#gold)" stroke="#0a1020" stroke-width="1.2" paint-order="stroke">ARCANOCLISMA</text>',630,880,true));
const tokens={versao:1,base:{ink:'#0e1928',navy:'#14283d',paper:'#f3ebd9',gold:'#cdb078',player:'#81bfce',opponent:'#da968f'},tipos:types,valores:{'Conflitüo':'#b95f45',Sublimeyn:'#a7bcc5',Dominum:'#745074',Persona:'#d68486',Alvorum:'#d1b05c',Morbidyum:'#a4bea5',Cognitium:'#6b74ad',Eternyr:'#304565',Primitivum:'#8b9761'},raridades:Object.fromEntries(Object.entries(rarity).map(([id,r])=>[id,{cor:r.cor,apenasCosmetica:true,nomeProvisorio:true,icone:`vetores/raridade-${id}.svg`}])),impressao:{corteMm:[63,88],sangriaPropostaMm:3,fonteSVG:'templates/*.svg com dimensões físicas de 63mm × 88mm'}};
await save('tokens.json',JSON.stringify(tokens,null,2));
const gallery=JSON.parse(await fs.readFile(path.resolve(root,'../cartas/catalogo-artes.json'),'utf8'));
const find=(text)=>gallery.assets.find(x=>x.titulo.toLowerCase().includes(text)) || gallery.assets[0];
const refs=[find('ciantia'),find('vermelho'),find('aurelhaz'),find('eternyr')];
const demo=Object.keys(types).map((type,i)=>({id:`modelo-${type}`,nome:['Guardião do Limiar','Ruptura do Horizonte','Memória do Éter','Santuário das Marés','Ciantia'][i],tipo:type,subtipos:[],valores:['Persona'],custo:[2,3,2,4,0][i],grau:[1,2,0,3,0][i],arcanoGerado:[3,4,2,3,0][i],ataque:['aliado','conjurador'].includes(type)?'—':null,vida:['aliado','conjurador'].includes(type)?'—':null,texto:'Texto da carta.',ambientacao:'Cada encontro deixa uma marca no mundo.',arte:`../${refs[i%refs.length].arquivo.replace(/^Arcanoclisma\//,'')}`,cosmetico:{raridade:['comum','rara','incomum','lendaria','rara'][i]},demonstracaoVisual:true}));
// As cartas acima são preenchimentos de layout, não definições jogáveis ou balanceamento.
await save('modelos-cartas.json',JSON.stringify({versao:1,aviso:'Exemplos visuais; o Claude substitui nome, textos e números por CartaDef/ConjuradorDef reais. Raridade fica em cosmetico e não altera efeitos.',cartas:demo},null,2));
await save('modelos-cartas.js','window.ARENA_MODELOS='+JSON.stringify(demo)+';');
console.log(JSON.stringify({templates:Object.keys(types).length,icones:Object.keys(glyphs).length,raridades:Object.keys(rarity).length,zonas:zones.length,modelos:demo.length}));
await import('./refinar-kit.mjs');



await import('./compor-overlays.mjs');

await import('./refinar-cartas-v4.mjs');

await import('./ligar-v4.mjs');

await import('./refinar-atributos-v5.mjs');
await import('./ligar-v5.mjs');
await import('./criar-foil-lendaria-v6.mjs');
await import('./refinar-reflexo-v7.mjs');
await import('./preparar-conjurador-v8.mjs');
await import('./preparar-acabamento-v9.mjs');
await import('./propagar-acabamento-v9.mjs');
