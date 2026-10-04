// Constrói assets geométricos locais; não usa serviços, credenciais ou modelos externos.
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);
const sharp = require('C:/Users/Anuah/ferramentas/agente-deepseek/node_modules/sharp');
const dir = path.dirname(fileURLToPath(import.meta.url));
const out = path.join(dir, 'assets/abertura-partida/v01');
await fs.mkdir(path.join(out, 'faces'), { recursive: true });
await fs.mkdir(path.join(out, 'vistas'), { recursive: true });
const add=(a,b)=>a.map((v,i)=>v+b[i]), sub=(a,b)=>a.map((v,i)=>v-b[i]);
const mul=(a,k)=>a.map(v=>v*k), dot=(a,b)=>a.reduce((s,v,i)=>s+v*b[i],0);
const cross=(a,b)=>[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]];
const unit=a=>mul(a,1/Math.hypot(...a));
const qm=(a,b)=>[
 a[3]*b[0]+a[0]*b[3]+a[1]*b[2]-a[2]*b[1],
 a[3]*b[1]-a[0]*b[2]+a[1]*b[3]+a[2]*b[0],
 a[3]*b[2]+a[0]*b[1]-a[1]*b[0]+a[2]*b[3],
 a[3]*b[3]-a[0]*b[0]-a[1]*b[1]-a[2]*b[2]];
const rotate=(q,v)=>add(v,add(mul(cross(q.slice(0,3),v),2*q[3]),mul(cross(q.slice(0,3),cross(q.slice(0,3),v)),2)));
const axis=(v,a)=>[...mul(unit(v),Math.sin(a/2)),Math.cos(a/2)];
const fromTo=(a,b)=>{
 const d=dot(a,b);
 if(d < -.999999) return axis(cross(a,Math.abs(a[0])<.8?[1,0,0]:[0,1,0]),Math.PI);
 const c=cross(a,b),q=[...c,1+d];return mul(q,1/Math.hypot(...q));
};
const lerp=(a,b,t)=>a.map((v,i)=>v+(b[i]-v)*t);
const mixq=(a,b,t)=>{
 if(dot(a,b)<0)b=mul(b,-1);
 const q=lerp(a,b,t);return mul(q,1/Math.hypot(...q));
};
const phi=(1+Math.sqrt(5))/2;
const vertices=[[-1,phi,0],[1,phi,0],[-1,-phi,0],[1,-phi,0],
 [0,-1,phi],[0,1,phi],[0,-1,-phi],[0,1,-phi],[phi,0,-1],[phi,0,1],[-phi,0,-1],[-phi,0,1]].map(unit);
const triangles=[[0,11,5],[0,5,1],[0,1,7],[0,7,10],[0,10,11],
 [1,5,9],[5,11,4],[11,10,2],[10,7,6],[7,1,8],
 [3,9,4],[3,4,2],[3,2,6],[3,6,8],[3,8,9],
 [4,9,5],[2,4,11],[6,2,10],[8,6,7],[9,8,1]];
const faces=triangles.map((ids,index)=>{
 let p=ids.map(i=>vertices[i]);
 if(dot(cross(sub(p[1],p[0]),sub(p[2],p[0])),add(add(p[0],p[1]),p[2]))<0){ids=[ids[1],ids[0],ids[2]];p=ids.map(i=>vertices[i]);}
 const center=mul(add(add(p[0],p[1]),p[2]),1/3),normal=unit(center);
 const up=unit(sub(p[2],center));
 const a=fromTo(normal,[0,0,1]),v=rotate(a,up);
 const orientacao=qm(axis([0,0,1],Math.atan2(v[0],v[1])),a);
 return {index,vertices:ids,center,normal,orientacao};
});
// Distribuição estável: as faces opostas somam 21. Não se afirma uma marca comercial.
let numero=1;
for(const face of faces){
 if(face.numero)continue;
 const opposite=faces.reduce((best,f)=>dot(face.normal,f.normal)<dot(face.normal,best.normal)?f:best,faces[0]);
 face.numero=numero;opposite.numero=21-numero;
 face.oposta=opposite.index;opposite.oposta=face.index;numero++;
}
const cores=[['#171c52','#3259b8'],['#24205c','#6654bb'],['#141f50','#354794'],['#232359','#526ccb']];
const paleta={corpo:'#182252',azul:'#3869d7',violeta:'#7865d0',energia:'#8fbaff',numeros:'#fff0ad',ouroSombra:'#c79744'};
function faceSVG(n){
 const [a,b]=cores[(n-1)%cores.length],font=n<10?150:134;
 return '<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">'+
 '<defs><linearGradient id="f" x1="0" y1="1" x2="1" y2="0"><stop stop-color="'+a+'"/><stop offset=".65" stop-color="'+b+'"/><stop offset="1" stop-color="#252353"/></linearGradient>'+
 '<linearGradient id="o" x1="0" y1="1" x2=".5" y2="0"><stop stop-color="#c79744"/><stop offset=".45" stop-color="#fff5c4"/><stop offset="1" stop-color="#f3d276"/></linearGradient></defs>'+
 '<path d="M40 472L472 472L256 94Z" fill="url(#f)" stroke="#6574b7" stroke-width="3"/>'+
 '<path d="M66 451L446 451L256 125Z" fill="none" stroke="#b79b62" stroke-opacity=".48" stroke-width="2"/>'+
 '<path d="M92 431Q156 407 158 323Q164 246 210 201M406 432Q340 396 346 328Q352 266 310 213" fill="none" stroke="#95baff" stroke-opacity=".35" stroke-width="2"/>'+
 '<path d="M103 425Q128 390 141 368M398 422Q376 390 370 361" fill="none" stroke="#bf9ef5" stroke-opacity=".55" stroke-width="3"/>'+
 '<text x="256" y="349" text-anchor="middle" dominant-baseline="middle" font-family="Georgia,serif" font-weight="bold" font-size="'+font+'" fill="url(#o)" stroke="#171531" stroke-width="5" paint-order="stroke">'+n+'</text></svg>';
}
const atlasWidth=2560,atlasHeight=2048,tile=512,composites=[],medicoes=[];
for(let n=1;n<=20;n++){
 const svg=faceSVG(n),filename='face-'+String(n).padStart(2,'0');
 await fs.writeFile(path.join(out,'faces',filename+'.svg'),svg);
 const png=await sharp(Buffer.from(svg)).png().toBuffer();
 await fs.writeFile(path.join(out,'faces',filename+'.png'),png);
 const {data,info}=await sharp(png).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let transparentes=0,min=255,max=0;
 for(let i=3;i<data.length;i+=4){const a=data[i];min=Math.min(min,a);max=Math.max(max,a);if(a<255)transparentes++;}
 medicoes.push({numero:n,arquivo:'faces/'+filename+'.png',dimensoes:[info.width,info.height],alfa:{min,max,pixelsComAlfaMenor255:transparentes}});
 composites.push({input:png,left:((n-1)%5)*tile,top:Math.floor((n-1)/5)*tile});
}
await sharp({create:{width:atlasWidth,height:atlasHeight,channels:4,background:'#0c102c'}}).composite(composites).png().toFile(path.join(out,'atlas-faces.png'));
const positions=[],normals=[],uvs=[];
for(const f of faces){
 const col=(f.numero-1)%5,row=Math.floor((f.numero-1)/5);
 const localUV=[[40,472],[472,472],[256,94]];
 f.inicio=positions.length/3;f.quantidade=3;
 f.vertices.forEach((id,i)=>{
  const p=add(add(f.center,mul(sub(vertices[id],f.center),.955)),mul(f.normal,.0018));
  positions.push(...p);normals.push(...f.normal);
  uvs.push((col*tile+localUV[i][0])/atlasWidth,(row*tile+localUV[i][1])/atlasHeight);
 });
}
const coreStart=positions.length/3;
for(const f of faces)for(const id of f.vertices){positions.push(...vertices[id]);normals.push(...f.normal);uvs.push(.002,.002);}
const model={versao:1,geometria:'icosaedro com 20 faces triangulares e rebaixo estreito entre placas',
 vertices,faces,paleta,cores,atlas:{largura:atlasWidth,altura:atlasHeight,tamanhoTile:tile},
 malha:{positions,normals,uvs,coreStart,coreCount:60},medicoes,
 mapaNumeros:'opostos somam 21; mapa próprio, estável',resultadoConfiguravel:true};
await fs.writeFile(path.join(out,'d20-modelo.json'),JSON.stringify(model,null,2)+'\n');
await fs.writeFile(path.join(out,'d20-modelo.js'),'globalThis.ARCANOCLISMA_D20_MODELO='+JSON.stringify(model)+';\n');
const buffers=[],bufferViews=[],accessors=[];
let total=0;
function accessor(data,type,target,min,max){
 const floats=new Float32Array(data),bytes=Buffer.from(floats.buffer);
 const view=bufferViews.length;
 bufferViews.push({buffer:0,byteOffset:total,byteLength:bytes.length,...(target?{target}:{})});buffers.push(bytes);total+=bytes.length;
 const dimensions={SCALAR:1,VEC2:2,VEC3:3,VEC4:4}[type];
 const id=accessors.length;
 if(min){min=Array.from({length:dimensions},(_,k)=>Math.min(...Array.from(floats).filter((_,i)=>i%dimensions===k)));max=Array.from({length:dimensions},(_,k)=>Math.max(...Array.from(floats).filter((_,i)=>i%dimensions===k)));}
 accessors.push({bufferView:view,componentType:5126,count:data.length/dimensions,type,...(min?{min,max}:{})});return id;
}
const pAcc=accessor(positions,'VEC3',34962,[-1,-1,-1],[1,1,1]);
const nAcc=accessor(normals,'VEC3',34962),uAcc=accessor(uvs,'VEC2',34962);
const meshes=[],nodes=[];
for(const f of faces){
 const partials=[['POSITION',pAcc,3],['NORMAL',nAcc,3],['TEXCOORD_0',uAcc,2]].map(([name,base,size])=>{
  const acc={...accessors[base],byteOffset:f.inicio*size*4,count:3};delete acc.min;delete acc.max;
  if(name==='POSITION'){const points=positions.slice(f.inicio*3,(f.inicio+3)*3);acc.min=[0,1,2].map(k=>Math.min(points[k],points[3+k],points[6+k]));acc.max=[0,1,2].map(k=>Math.max(points[k],points[3+k],points[6+k]));}
  const id=accessors.push(acc)-1;return [name,id];
 });
 meshes.push({name:'Face '+f.numero,primitives:[{attributes:Object.fromEntries(partials),material:0}]});
 nodes.push({name:'face-'+String(f.numero).padStart(2,'0'),mesh:meshes.length-1,extras:{numero:f.numero,oposta:21-f.numero}});
}
const coreAttributes={};
for(const [name,base,size]of[['POSITION',pAcc,3],['NORMAL',nAcc,3],['TEXCOORD_0',uAcc,2]]){
 const acc={...accessors[base],byteOffset:coreStart*size*4,count:60};coreAttributes[name]=accessors.push(acc)-1;
}
meshes.push({name:'Juntas estreitas',primitives:[{attributes:coreAttributes,material:0}]});
nodes.push({name:'juntas',mesh:20});
const rootNode=nodes.length;
nodes.push({name:'D20 Transplanar',children:Array.from({length:21},(_,i)=>i),translation:[0,1,0],extras:{autor:'Codex; briefing do autor',resultadoExemplo:20}});
const qFinal=faces.find(f=>f.numero===20).orientacao;
const times=[],rotations=[],translations=[];
let previous;
for(let i=0;i<=84;i++){
 const t=i/84,a=(t<.72?t/.72:1)*Math.PI*9;
 let q=qm(axis([.25,1,.3],a),axis([1,.2,.5],a*.69));
 if(t>.72){const s=(t-.72)/.28;q=mixq(q,qFinal,s*s*(3-2*s));}
 if(previous&&dot(previous,q)<0)q=mul(q,-1);previous=q;
 let hop=0;
 for(const [inicio,fim,height] of [[.08,.48,.65],[.48,.64,.28],[.64,.78,.12],[.78,.90,.04]]){
  if(t>inicio&&t<fim){const u=(t-inicio)/(fim-inicio);hop=4*height*u*(1-u);}
 }
 const minY=Math.min(...vertices.map(v=>rotate(q,v)[1]));
 times.push(t*4.2);rotations.push(...q);translations.push(-.55+1.1*t,-minY+hop,0);
}
const timeAcc=accessor(times,'SCALAR',undefined,[0],[4.2]);
const rotAcc=accessor(rotations,'VEC4'),transAcc=accessor(translations,'VEC3');
const animation={name:'rolagem-exemplo-20',samplers:[{input:timeAcc,output:rotAcc,interpolation:'LINEAR'},{input:timeAcc,output:transAcc,interpolation:'LINEAR'}],channels:[{sampler:0,target:{node:rootNode,path:'rotation'}},{sampler:1,target:{node:rootNode,path:'translation'}}]};
const gltf={asset:{version:'2.0',generator:'Arcanoclisma — construção geométrica local'},scene:0,scenes:[{nodes:[rootNode]}],nodes,meshes,
 materials:[{name:'Azul violeta e ouro claro',pbrMetallicRoughness:{baseColorTexture:{index:0},metallicFactor:.26,roughnessFactor:.34},doubleSided:false}],
 textures:[{sampler:0,source:0}],samplers:[{magFilter:9729,minFilter:9987,wrapS:33071,wrapT:33071}],images:[{uri:'atlas-faces.png'}],
 buffers:[{uri:'d20-malha.bin',byteLength:total}],bufferViews,accessors,animations:[animation],extras:{mapaFaces:faces.map(f=>({numero:f.numero,indices:f.vertices,oposta:21-f.numero,orientacaoFrontal:f.orientacao})),paleta,resultadoDoMotor:true}};
const binary=Buffer.concat(buffers);
await fs.writeFile(path.join(out,'d20-malha.bin'),binary);
await fs.writeFile(path.join(out,'d20-transplanar.gltf'),JSON.stringify(gltf,null,2)+'\n');
// GLB autocontido: geometria e atlas no mesmo arquivo.
const image=await fs.readFile(path.join(out,'atlas-faces.png'));
const paddedImage=Buffer.concat([image,Buffer.alloc((4-image.length%4)%4)]);
const glbJson=structuredClone(gltf);delete glbJson.buffers[0].uri;
glbJson.images=[{bufferView:glbJson.bufferViews.length,mimeType:'image/png'}];
glbJson.bufferViews.push({buffer:0,byteOffset:binary.length,byteLength:image.length});
glbJson.buffers[0].byteLength=binary.length+paddedImage.length;
const json=Buffer.from(JSON.stringify(glbJson)),paddedJSON=Buffer.concat([json,Buffer.alloc((4-json.length%4)%4,32)]);
const bin=Buffer.concat([binary,paddedImage]);
const header=Buffer.alloc(12),jsonHeader=Buffer.alloc(8),binHeader=Buffer.alloc(8);
header.writeUInt32LE(0x46546c67);header.writeUInt32LE(2,4);header.writeUInt32LE(28+paddedJSON.length+bin.length,8);
jsonHeader.writeUInt32LE(paddedJSON.length);jsonHeader.writeUInt32LE(0x4e4f534a,4);
binHeader.writeUInt32LE(bin.length);binHeader.writeUInt32LE(0x004e4942,4);
await fs.writeFile(path.join(out,'d20-transplanar.glb'),Buffer.concat([header,jsonHeader,paddedJSON,binHeader,bin]));
const views={frente:[0,0,0],tras:[0,Math.PI,0],esquerda:[0,-Math.PI/2,0],direita:[0,Math.PI/2,0],superior:[Math.PI/2,0,0],inferior:[-Math.PI/2,0,0]};
for(const [name,[x,y,z]] of Object.entries(views)){
 const q=qm(axis([0,0,1],z),qm(axis([0,1,0],y),axis([1,0,0],x)));
 const visible=faces.map(f=>({...f,n:rotate(q,f.normal),p:f.vertices.map(id=>rotate(q,vertices[id]))})).filter(f=>f.n[2]>0).sort((a,b)=>a.n[2]-b.n[2]);
 let svg='<svg xmlns="http://www.w3.org/2000/svg" width="800" height="800" viewBox="0 0 800 800"><defs><linearGradient id="metal"><stop stop-color="#4562a3"/><stop offset="1" stop-color="#24215d"/></linearGradient></defs>';
 for(const f of visible){
  const pts=f.p.map(v=>[400+v[0]*320,400-v[1]*320]),cx=pts.reduce((s,p)=>s+p[0],0)/3,cy=pts.reduce((s,p)=>s+p[1],0)/3;
  svg+='<polygon points="'+pts.map(p=>p.join(',')).join(' ')+'" fill="url(#metal)" stroke="#151733" stroke-width="7"/><text x="'+cx+'" y="'+cy+'" fill="#fff0ad" font-family="Georgia,serif" font-size="'+(30+f.n[2]*35)+'" font-weight="bold" text-anchor="middle" dominant-baseline="middle">'+f.numero+'</text>';
 }
 svg+='</svg>';await fs.writeFile(path.join(out,'vistas',name+'.svg'),svg);
}
const sheet='<svg xmlns="http://www.w3.org/2000/svg" width="1280" height="1100" viewBox="0 0 2560 2200"><rect width="2560" height="2200" fill="#10152c"/><text x="1280" y="80" fill="#f7e4ac" font-size="60" text-anchor="middle" font-family="Georgia,serif">D20 Transplanar · vinte faces</text>'+Array.from({length:20},(_,i)=>'<image x="'+(i%5*512)+'" y="'+(Math.floor(i/5)*512+120)+'" width="512" height="512" href="data:image/svg+xml;base64,'+Buffer.from(faceSVG(i+1)).toString('base64')+'"/>').join('')+'</svg>';
await fs.writeFile(path.join(out,'todas-as-faces.svg'),sheet);
console.log(JSON.stringify({modelo:'d20-transplanar.glb',faces:20,animacao:'rolagem-exemplo-20',duracaoMs:4200,atlas:[atlasWidth,atlasHeight],ferramentas:['JavaScript local','Sharp local'],semServicosExternos:true}));

