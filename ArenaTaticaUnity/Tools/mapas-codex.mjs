// Converte os mapas do Codex (tarefa 011 revisão 03 e mapas rotativos de 10/10) em mapas do jogo:
// Resources/Mapas/codex.json no formato de ArenaMaps (MapDef: relevo, terreno, zona e enfeites).
// Uso: node Tools/mapas-codex.mjs   (rodar de novo quando o Codex entregar mapas novos)
import fs from 'fs'; import path from 'path'; import {fileURLToPath} from 'url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..'), ent=path.resolve(root,'../coordenacao/arena-pixelai/entregas');
const THEMES={pedreira:0,'montanha rochosa':0,bosque:1,deserto:2,neve:3,'vulcão':4,vulcao:4};
// Peças que fazem papel de terreno (igual a MapGen.SkinTerrain); 0 Livre, 1 Arbusto, 2 Arvore, 3 Caixa.
const skin=p=>p.startsWith('arvore')?2:p==='arbusto'?1:(p==='caixa'||p==='caixa_rachada'||p==='pedra')?3:0;
const r2=v=>Math.round(v*100)/100, maps=[], report=[];
// level(coluna, linha): nível da casa; a linha do Codex cresce com o y do Blender, que é o −z do jogo.
function build(name,theme,n,level,props){
  const tiles=[]; for(let z=0;z<n;z++) for(let x=0;x<n;x++) tiles.push({height:level(x,n-1-z),terrain:0,zone:false});
  const decor=[];
  for(const p of props){
    if(p.piece==='moeda_chao') continue; // moedas vêm da regra (MapGen.SpawnCoins)
    const cx=Math.round(p.x), cz=Math.round(p.z), t=skin(p.piece);
    if(cx<0||cz<0||cx>=n||cz>=n) { report.push(`${name}: ${p.piece} fora do tabuleiro`); continue; }
    if(t) tiles[cz*n+cx].terrain=t;
    decor.push({piece:p.piece,x:r2(p.x),z:r2(p.z),rot:r2(p.rot),scale:r2(p.scale??1)});
  }
  maps.push({name,width:n,theme,tiles,decor});
}
// Revisão 03: composições 12×12 (posição Blender centrada; y Blender cresce com a linha da matriz).
const man=path.join(ent,'mapa-blender/v03/manifestos');
for(const f of fs.readdirSync(man).filter(f=>f.endsWith('-distribuicao.json')).sort()){
  const j=JSON.parse(fs.readFileSync(path.join(man,f),'utf8')), n=j.alturaExemplo12x12.length, h=(n-1)/2;
  const theme=THEMES[j.cenario.toLowerCase()]; if(theme===undefined) throw new Error('cenário '+j.cenario);
  const props=j.colocacoes.map(c=>({piece:c.peca,x:c.posicaoBlender[0]+h,z:h-c.posicaoBlender[1],rot:-c.rotacaoZBlender,scale:c.escala}));
  // Conferência: a base de cada peça deve estar no nível da matriz (0,05 + 0,45·nível).
  for(const c of j.colocacoes){ const [col,row]=c.casaExemplo, lvl=Math.round((c.posicaoBlender[2]-.05)/.45); if(lvl!==j.alturaExemplo12x12[col][row]) report.push(`${f}: ${c.peca} em ${col},${row} nível ${lvl} ≠ matriz ${j.alturaExemplo12x12[col][row]}`); }
  // Nas composições a matriz é [coluna][linha]; nos layouts rotativos, [linha][coluna] (construir.py do Codex).
  build(`${j.cenario.replace('Montanha rochosa','Montanha')} · ${j.nomeComposicao}`,theme,n,(c,r)=>j.alturaExemplo12x12[c][r],props);
}
// Mapas rotativos: layouts de 12, 16 e 18 casas (casa = [coluna, linha]).
const NAMES={'bosque-quatro-patios':'Bosque dos Quatro Pátios','neve-bastioes-diagonais':'Neve dos Bastiões Diagonais','pedreira-anel-partido':'Pedreira do Anel Partido'};
const lay=path.join(ent,'mapas-rotativos-2026-10-10/layouts');
for(const f of fs.readdirSync(lay).filter(f=>f.endsWith('.json')).sort()){
  const j=JSON.parse(fs.readFileSync(path.join(lay,f),'utf8')), n=j.dimensao, id=f.replace(/-\d+\.json$/,'');
  const props=j.props.map(p=>({piece:p.peca,x:p.casa[0],z:n-1-p.casa[1],rot:-(p.rotacaoGraus||0)}));
  build(`${NAMES[id]??id} ${n}×${n}`,THEMES[id.split('-')[0]],n,(c,r)=>j.alturas[r][c],props);
}
const out=path.join(root,'Assets/ArenaTatica/Resources/Mapas/codex.json');
fs.writeFileSync(out,JSON.stringify({maps}));
console.log(`${maps.length} mapas → ${path.relative(root,out)}`); for(const m of maps) console.log(' ',m.width,m.theme,m.name,'enfeites',m.decor.length);
if(report.length) console.log('Avisos:\n '+report.join('\n '));
