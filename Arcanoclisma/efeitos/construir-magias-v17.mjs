// Assets vetoriais e PNG nativos, transparentes, a partir de direção por carta.
// Reexecutável offline; não chama modelos nem consulta chaves.
import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
import { DIRECAO } from './direcao-magias-v17.mjs';
const require = createRequire(new URL('../online/package.json', import.meta.url));
const sharp = require('sharp');
const raiz = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const ler = async p => JSON.parse(await fs.readFile(path.join(raiz, p), 'utf8'));
const referencias = await ler('Arcanoclisma/efeitos/previas/revisao-artes/referencias.json');
const catalogo = (await ler('Arcanoclisma/online/public/jogo/dados.json')).catalogo;
const saida = path.join(raiz, 'Arcanoclisma/online/public/kit/efeitos');
const hash = b => crypto.createHash('sha256').update(b).digest('hex');
const xml = s => String(s).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
const P = d => `<path d="${d}"/>`;
const C = (x,y,r) => `<circle cx="${x}" cy="${y}" r="${r}"/>`;
const L = (x1,y1,x2,y2) => `<path d="M${x1} ${y1}L${x2} ${y2}" fill="none"/>`;
const rect = (x,y,w,h,r=3) => `<rect x="${x}" y="${y}" width="${w}" height="${h}" rx="${r}"/>`;
const estrela = (x,y,r=30) => P(`M${x} ${y-r}Q${x+5} ${y-5} ${x+r} ${y}Q${x+5} ${y+5} ${x} ${y+r}Q${x-5} ${y+5} ${x-r} ${y}Q${x-5} ${y-5} ${x} ${y-r}Z`);
const folhas = Array.from({length:5},(_,i)=>`<g transform="translate(${85+i*21} ${147-i*14}) rotate(${i*30-65})">${P('M0 0Q-27-26 0-55Q27-26 0 0Z')}${L(0,-4,0,-44)}</g>`).join('');
const paginas = Array.from({length:4},(_,i)=>`<g transform="translate(${58+i*25} ${70+(i%2)*24}) rotate(${i*12-18} 24 32)">${rect(0,0,45,65)}${L(9,17,35,17)}${L(9,28,31,28)}${L(9,40,33,40)}</g>`).join('');
const dentes = Array.from({length:7},(_,i)=>P(`M${63+i*19} 111l9 24 7-28Z`)).join('');
const motivo = {
  dragao: P('M43 147Q49 97 95 99L104 61 124 89 164 83 187 109 216 117 193 141 161 132 139 151Q101 174 51 164L98 143 112 127 84 127Z')+P('M138 94 160 62 158 92M80 101 60 73 65 116')+C(166,110,4),
  relampago: P('M26 143 73 103 96 126 135 63 131 113 174 95 156 143 230 112 182 165 151 154 119 187 127 143 94 151 86 132Z'),
  brasa: P('M54 139 79 95 132 80 179 100 202 133 168 157 115 174 71 157Z')+P('M69 138 116 107 167 106 145 132 105 151Z')+L(79,95,116,107)+L(179,100,168,157),
  vento: P('M25 150Q81 75 188 117Q214 127 227 106Q229 153 179 137Q92 110 25 150Z')+P('M40 183Q80 119 184 162Q124 138 40 183Z')+P('M66 91Q116 49 184 84Q137 69 66 91Z'),
  mandibula: P('M40 92Q110 60 211 107L169 122 141 109 99 121 65 139Z')+P('M48 159 75 137 110 159 169 161 207 141Q167 210 76 185Z')+dentes,
  ossos: P('M64 78Q80 57 91 82L179 162Q202 155 204 174Q219 194 196 202Q185 211 176 190L79 99Q53 100 64 78Z')+P('M96 72Q131 51 161 82L171 124 155 141 152 161 111 162 106 140 89 121Z')+C(111,112,9)+C(147,112,9),
  olho: P('M31 128Q125 39 225 128Q125 213 31 128Z')+C(129,127,37)+P('M130 95Q110 127 130 159Q145 127 130 95Z'),
  cristal: P('M71 166 94 53 169 77 207 142 121 199Z')+P('M94 53 129 119 169 77M129 119 207 142M129 119 121 199M129 119 71 166'),
  cartas: paginas,
  carta: rect(80,56,91,142,9)+estrela(126,126,31),
  livro: P('M38 77Q86 64 128 91Q170 64 215 76L210 177Q162 162 128 187Q87 164 45 177Z')+L(128,91,128,181)+P('M61 98 106 111M60 119 106 132M152 110 195 96M152 133 192 119'),
  paginas,
  folhas,
  laco: P('M33 160Q30 86 124 115T220 76Q212 146 129 144T51 195Q78 143 146 152T224 154Q158 190 103 163T33 160Z'),
  balanca: L(128,54,128,199)+L(75,196,181,196)+L(51,90,205,90)+L(71,90,45,147)+L(71,90,97,147)+P('M39 145Q70 194 103 145Z')+L(187,90,161,147)+L(187,90,212,147)+P('M155 145Q184 194 219 145Z')+estrela(128,63,18),
  trigo: L(126,61,126,211)+Array.from({length:5},(_,i)=>P(`M126 ${91+i*21}Q90 ${88+i*21} 92 ${64+i*21}Q120 ${66+i*21} 126 ${91+i*21}Q157 ${85+i*21} 164 ${61+i*21}Q133 ${61+i*21} 126 ${91+i*21}Z`)).join(''),
  som: [30,52,77,102].map(r=>`<path d="M${128-r*.7} ${128-r}Q${128+r} 128 ${128-r*.7} ${128+r}" fill="none"/>`).join(''),
  laser: P('M10 125 218 114 247 128 218 142 10 132Z')+L(15,128,235,128),
  lenco: P('M50 75Q108 103 198 70L183 126 210 177Q128 159 63 198L83 142Z')+P('M60 87Q106 116 183 87M75 174Q131 147 194 166'),
  agua: P('M42 166Q54 81 126 88Q192 95 148 148Q143 104 113 118Q96 131 137 161Q168 193 221 139Q181 224 109 188Q58 158 81 127Q57 135 42 166Z'),
  raiz: P('M116 218 112 158 78 136 47 117 83 124 103 140 97 95 70 75 95 82 108 49 108 104 123 132 160 102 184 56 177 103 211 101 163 119 135 160 144 219Z'),
  bolhas: C(86,91,22)+C(157,73,17)+C(165,148,34)+C(79,171,12)+estrela(112,136,15),
  portal: `<ellipse cx="128" cy="128" rx="67" ry="88" fill="none"/><ellipse cx="128" cy="128" rx="53" ry="73" fill="none"/>`+P('M88 47 71 25 101 38M171 215 183 238 153 222'),
  mapa: P('M34 87 84 65 139 87 208 67 224 181 165 196 112 178 46 199Z')+L(84,65,112,178)+L(139,87,165,196)+P('M57 151Q75 103 100 127T169 111L192 138')+estrela(189,140,12),
  ampulheta: P('M74 49H181V69H74ZM73 185H182V205H73Z')+P('M83 69Q86 108 127 128Q169 150 171 183H86Q87 149 127 128Q167 108 170 70Z'),
  engrenagem: `<g fill="none">${C(128,128,59)}${C(128,128,32)}${Array.from({length:12},(_,i)=>`<g transform="rotate(${i*30} 128 128)">${rect(120,43,16,29)}</g>`).join('')}</g>`+L(128,128,153,91)+L(128,128,99,118),
  garras: [0,1,2].map(i=>P(`M${54+i*49} 43Q${20+i*49} 132 ${76+i*49} 211Q${56+i*49} 118 ${93+i*49} 45Z`)).join(''),
  vespas: Array.from({length:6},(_,i)=>`<g transform="translate(${42+(i%3)*69} ${86+Math.floor(i/3)*67}) rotate(${i*13-30})">${P('M-5 0Q-30-34-34-8L-3 6Q22-31 31-7L7 9Z')}${P('M-15 8Q0-6 25 8L37 12 24 16Q0 28-15 8Z')}${L(-1,1,-1,21)}${L(10,2,10,20)}</g>`).join(''),
  areia: P('M70 212Q226 190 90 165T151 110Q209 95 84 78Q184 105 128 123T177 170Q228 195 70 212Z')+P('M26 203Q110 211 141 193M100 58Q165 26 215 56'),
  porta: P('M67 209V49H192V209H170V72H90V209Z')+P('M90 74 157 54V192L90 210Z')+C(143,137,5),
  lamina: P('M39 190 170 54 216 34 199 82 71 207Z')+L(171,54,69,184)+P('M32 170 86 218 96 209 44 158Z'),
  vapor: P('M63 208Q118 164 70 134T99 42Q74 111 123 121T113 191Z')+P('M142 198Q202 140 155 113T184 39Q170 100 205 123T187 195Z'),
  singularidade: `<ellipse cx="128" cy="128" rx="91" ry="48" transform="rotate(-24 128 128)" fill="none"/>`+C(128,128,49)+`<circle cx="128" cy="128" r="41" fill="#080c22" stroke="none"/>`+P('M37 166Q174 191 209 83M51 102Q57 56 152 66'),
  pena: P('M58 211 185 50Q192 30 207 43Q234 106 164 171L131 179 110 161Z')+L(57,211,185,72)+P('M141 130 172 128M162 103 192 100'),
  gota: P('M128 37Q96 83 74 120Q35 186 108 213Q178 234 188 165Q192 116 128 37Z')+P('M85 143Q74 180 113 190'),
  espelho: `<ellipse cx="128" cy="113" rx="64" ry="79" fill="none"/><ellipse cx="128" cy="113" rx="51" ry="65" fill="none"/>`+P('M117 191H139L147 224H110Z')+P('M104 69 147 130M113 104 154 163'),
  foice: P('M76 230 129 57 142 61 90 235Z')+P('M114 60Q190 8 228 95Q179 56 127 108Z'),
  estrela: estrela(128,128,79)+estrela(59,68,21)+estrela(202,170,17),
  coroa: P('M48 79 88 113 128 50 163 112 211 77 192 181H66Z')+P('M69 157H191M74 173H184')+C(128,130,11),
  moeda: C(128,128,82)+C(128,128,67)+estrela(128,128,45),
  ancora: C(128,55,21)+L(128,76,128,200)+L(85,111,171,111)+P('M55 157Q61 218 128 213Q196 216 207 157L186 177 197 181Q161 201 137 184L128 198 119 184Q90 203 62 181L75 175Z'),
  chave: C(80,99,36)+C(80,99,17)+P('M100 117 183 205 203 187 185 169 173 183 157 165 169 153 153 137 142 150 119 124Z'),
  chuva: P('M45 109Q24 70 69 69Q75 25 115 62Q146 33 167 67Q217 55 218 107Z')+[65,109,155,193].map((x,i)=>P(`M${x} ${131+i%2*18}q-20 35 0 38q20-2 0-38Z`)).join(''),
  flecha: P('M28 133 188 117 171 97 238 123 179 163 192 139 26 149Z')+P('M61 131 34 104 77 128M64 141 45 179 88 140'),
  sal: Array.from({length:11},(_,i)=>`<g transform="translate(${57+(i*41)%147} ${63+(i*29)%135}) rotate(${i*21})">${rect(-9,-9,18,18,1)}</g>`).join(''),
  dado: P('M72 68 166 51 213 105 197 190 97 209 43 151Z')+P('M72 68 127 124 213 105M127 124 97 209')+C(125,85,8)+C(82,129,7)+C(78,164,7)+C(157,145,9)+C(181,161,9),
  relogio: C(128,130,79)+C(128,130,64)+rect(112,30,32,22)+L(128,130,128,83)+L(128,130,164,151)+C(128,130,7),
  ovo: P('M124 35Q178 60 194 129Q224 207 144 220Q48 230 59 149Q62 80 124 35Z')+P('M124 37 113 92 142 112 106 143 137 177 127 218'),
  punho: P('M61 191 49 141 73 99 83 78 109 78 119 65 144 69 155 76 177 83 191 125 206 157 171 206 106 217Z')+P('M73 99 151 124 182 106M88 92 97 121M119 78 127 126M154 82 162 115M56 152 106 142 130 170 112 189'),
  pente: P('M42 79Q125 48 215 84L210 108 198 101 191 187 180 185 184 98 171 94 166 185 154 185 157 92 141 90 138 185 126 185 125 89 111 89 109 185 97 185 98 91 84 94 82 185 70 185 73 98 53 107Z'),
  nevoa: P('M34 168Q46 104 93 126Q56 92 109 65Q98 123 155 110Q227 93 220 151Q173 123 160 159Q177 192 109 197Q161 168 116 157Q66 181 34 168Z'),
  mascara: P('M54 63Q129 101 201 62L192 154 151 202 112 215 70 167Z')+P('M72 111Q98 97 116 121L86 131ZM144 123Q168 96 186 111L179 133Z')+P('M107 180Q133 160 160 175'),
  nuvem: P('M44 145Q14 112 53 90Q58 46 100 66Q129 26 163 64Q201 50 211 91Q246 128 210 154Z')+P('M57 176Q85 145 117 172T206 179'),
  selo: C(128,128,79)+C(128,128,60)+P('M81 79 176 177M81 177 176 79M128 66 188 161H70Z'),
  serpente: P('M42 190Q127 216 149 160T99 94Q73 69 118 51Q181 43 211 87L184 96 168 83Q124 67 128 95Q228 178 127 214Q75 228 42 190Z')+C(186,79,4),
  estandarte: L(72,35,72,224)+P('M77 45Q119 69 188 54L215 129 175 156Q137 129 77 140Z')+estrela(141,100,28),
  corrente: Array.from({length:6},(_,i)=>`<g transform="translate(${56+i*27} ${77+i*18}) rotate(-35)">${rect(-14,-23,28,46,12)}</g>`).join(''),
};
function svg(d, id, fase) {
  const seed = Number.parseInt(hash(id).slice(0,6),16);
  const pontos = Array.from({length:fase==='impacto'?15:9},(_,i)=>{
    const a=(i*2.399+seed%10),r=75+(i*17+seed)%43,x=128+Math.cos(a)*r,y=128+Math.sin(a)*r;
    return `<circle cx="${x.toFixed(1)}" cy="${y.toFixed(1)}" r="${1.4+i%3}" fill="${i%2?d.cor:d.nucleo}" opacity="${.35+i%4*.14}"/>`;
  }).join('');
  const forma = motivo[d.motivo];
  if (!forma) throw new Error('Motivo ausente: '+d.motivo);
  const linhas = fase==='impacto' ? [0,1,2,3,4,5].map(i=>`<path d="M128 34V14" transform="rotate(${i*60+seed%25} 128 128)" stroke="${d.cor}" stroke-width="3" stroke-linecap="round"/>`).join('') : '';
  const corpo = `<g fill="url(#corpo)" stroke="${d.nucleo}" stroke-width="2.4" stroke-linejoin="round" stroke-linecap="round">${forma}</g>`;
  return `<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 256 256"><title>${xml(id+' · '+fase+' · '+d.motivo)}</title><defs><linearGradient id="corpo" x1="0" y1="0" x2="1" y2="1"><stop stop-color="${d.nucleo}" stop-opacity=".9"/><stop offset=".44" stop-color="${d.cor}" stop-opacity=".78"/><stop offset="1" stop-color="${d.cor}" stop-opacity=".3"/></linearGradient><filter id="brilho" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="4"/></filter></defs><g opacity=".26" filter="url(#brilho)">${corpo}</g>${linhas}<g transform="translate(128 128) scale(${fase==='conjurar'?.72:1}) translate(-128 -128)">${corpo}</g>${pontos}</svg>`;
}
function destino(p, d) {
  const t=p.texto.toLowerCase();
  if(d.movimento==='onda') return /os seus|inclusive os seus|todos os aliados \(/.test(t)?'ambos-campos':'campo-rival';
  if(d.movimento==='selo') return 'alvo-pilha';
  if(d.movimento==='ressurgir') return 'campo-proprio';
  if(d.movimento==='local') return 'origem';
  if(d.movimento==='cura'&&/seu conjurador/.test(t)) return 'conjurador-proprio';
  if(/conjurador inimigo/.test(t)) return 'conjurador-rival';
  return 'alvo';
}
const manifesto={}, arquivos=[];
for(const p of referencias){
  const d=DIRECAO[p.id]; if(!d)throw new Error('Falta direção individual: '+p.id);
  const dir=path.join(saida,p.id);await fs.mkdir(dir,{recursive:true});
  const entregues={};
  for(const fase of ['conjurar','trajeto','impacto']){
    const vetorial=Buffer.from(svg(d,p.id,fase));
    const nome=fase+'-v17'; const arq=path.join(dir,nome);
    await fs.writeFile(arq+'.svg',vetorial);
    const png=await sharp(vetorial).png().toBuffer();await fs.writeFile(arq+'.png',png);
    const {data,info}=await sharp(png).ensureAlpha().raw().toBuffer({resolveWithObject:true});
    let zero=0,parcial=0,opaco=0;for(let i=3;i<data.length;i+=4){if(data[i]===0)zero++;else if(data[i]===255)opaco++;else parcial++;}
    const base=`/kit/efeitos/${p.id}/${nome}`;
    entregues[fase]=base+'.svg';
    arquivos.push({id:p.id,fase,svg:base+'.svg',png:base+'.png',sha256Svg:hash(vetorial),sha256Png:hash(png),largura:info.width,altura:info.height,alfa:{transparentes:zero,parciais:parcial,opacos:opaco,total:info.width*info.height}});
  }
  const origem=await fs.readFile(path.join(raiz,p.arte));
  const t=p.texto.toLowerCase();
  const gatilhos=p.tipo==='temporaria'?['resolucao']:[];
  if(/ao entrar/.test(t))gatilhos.push('entrada','gatilho');
  if(/ao causar/.test(t))gatilhos.push('gatilho');
  if(/suspenda|consumível|uso único/.test(t))gatilhos.push('habilidade');
  const atual=catalogo?.cartas?.[p.id];
  manifesto[p.id]={versao:17,nome:p.nome,okGpt:true,arquivos:Object.values(entregues),camadas:entregues,movimento:d.movimento,motivo:d.motivo,paleta:[d.cor,d.nucleo],destino:destino(p,d),duracaoMs:d.movimento==='onda'?1550:1300,tamanhoPx:d.movimento==='feixe'?66:108,gatilhosAceitos:gatilhos.length?gatilhos:['habilidade'],arte:p.arte,arteSha256:hash(origem),textoBase:atual?.texto||p.texto,assinaturaEfeito:hash(JSON.stringify(atual?.efeitos||p.texto)),revisaoVisual:'132 artes inspecionadas em seis pranchas; direção individual por ID',testadoNoJogo:false};
}
await fs.writeFile(path.join(saida,'manifesto.json'),JSON.stringify(manifesto,null,2)+'\n');
await fs.writeFile(path.join(saida,'manifesto-v17.js'),'globalThis.ARCANOCLISMA_MAGIAS_V17 = '+JSON.stringify(manifesto)+';\n');
await fs.mkdir(path.join(raiz,'Arcanoclisma/efeitos/entregas'),{recursive:true});
await fs.writeFile(path.join(raiz,'Arcanoclisma/efeitos/entregas/magias-v17.visual.json'),JSON.stringify({tarefaId:'mesa-efeitos-v1',versao:17,estado:'assets_e_animador_entregues_integracao_de_eventos_pendente',totalCartas:Object.keys(manifesto).length,totalAssets:arquivos.length*2,arquivos,referencias:'Arcanoclisma/efeitos/previas/revisao-artes/referencias.json',direcao:'Arcanoclisma/efeitos/direcao-magias-v17.mjs',motorAlterado:false,integracaoClaudeConfirmada:false},null,2)+'\n');
console.log(`${Object.keys(manifesto).length} cartas; ${arquivos.length} SVG e ${arquivos.length} PNG com alfa medido.`);
