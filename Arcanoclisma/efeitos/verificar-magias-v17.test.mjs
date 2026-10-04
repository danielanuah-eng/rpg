// Contrato e ciclo de vida do animador, sem navegador, rede ou comandos do motor.
import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import crypto from 'node:crypto';
const raiz=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
const publico=path.join(raiz,'Arcanoclisma/online/public');
const manifesto=JSON.parse(await fs.readFile(path.join(publico,'kit/efeitos/manifesto.json'),'utf8'));
let reduzido=false,leituras=0,registro=[];
class No {
  constructor(tag='div'){this.tag=tag;this.style={setProperty(k,v){this[k]=v;}};this.children=[];this.attrs={};this.dataset={};this.isConnected=true;this.classList={contains:()=>false,remove(){},add(){}};}
  append(...filhos){for(const n of filhos){this.children.push(n);n.parent=this;n.isConnected=true;}}
  remove(){if(this.parent)this.parent.children=this.parent.children.filter(x=>x!==this);this.isConnected=false;}
  setAttribute(k,v){this.attrs[k]=String(v);}
  removeAttribute(k){delete this.attrs[k];}
  querySelectorAll(){return [];}
  querySelector(){return null;}
  cloneNode(){const n=new No(this.tag);n.src=this.src;return n;}
  animate(quadros,opcoes){registro.push({tag:this.tag,style:{...this.style},quadros,opcoes});return {finished:Promise.resolve(),cancel(){}};}
  getBoundingClientRect(){return this.rect || {left:100,top:100,width:100,height:140};}
}
globalThis.Image=class extends No{constructor(){super('img');} async decode(){leituras++;await fs.access(path.join(publico,this.src));}};
globalThis.document={createElement:t=>new No(t),createElementNS:(_,t)=>new No(t),body:new No(),head:new No(),getElementById:()=>null};
globalThis.MutationObserver=class{observe(){}disconnect(){}};
globalThis.matchMedia=()=>({matches:reduzido});
globalThis.fetch=async()=>({ok:true,json:async()=>manifesto});
globalThis.innerWidth=1200;globalThis.innerHeight=800;
await import('../online/public/kit/efeitos/magias-v17.js');
const {criarEfeitosMesa}=await import('../online/public/js/mesa-efeitos-codex.js');
const origem={left:100,top:100,width:100,height:140},destino={left:600,top:360,width:100,height:140};
const tocar=v=>ArcanoclismaMagias.conjurar({camada:new No(),visual:v,origem,destinos:[destino]});

test('todas as 132 cartas possuem camadas reais e PNG com alfa medido',async()=>{
  const pedido=JSON.parse(await fs.readFile(path.join(raiz,'Arcanoclisma/efeitos/pedidos/mesa-efeitos-v1.claude.json'),'utf8'));
  assert.deepEqual(Object.keys(manifesto).sort(),pedido.resolucao.map(p=>p.id).sort());
  const entrega=JSON.parse(await fs.readFile(path.join(raiz,'Arcanoclisma/efeitos/entregas/magias-v17.visual.json'),'utf8'));
  assert.equal(entrega.totalCartas,132);assert.equal(entrega.arquivos.length,396);
  for(const a of entrega.arquivos){
    const png=await fs.readFile(path.join(publico,a.png)),svg=await fs.readFile(path.join(publico,a.svg));
    assert.equal(crypto.createHash('sha256').update(png).digest('hex'),a.sha256Png,a.png);
    assert.equal(crypto.createHash('sha256').update(svg).digest('hex'),a.sha256Svg,a.svg);
    assert.ok(a.alfa.transparentes>0 && a.alfa.parciais>0,a.png);
    assert.equal(a.alfa.transparentes+a.alfa.parciais+a.alfa.opacos,512*512);
  }
});
test('cada efeito conclui e remove os elementos transitórios',async()=>{
  for(const [id,v] of Object.entries(manifesto)){
    const camada=new No();const e=ArcanoclismaMagias.conjurar({camada,visual:v,origem,destinos:[destino]});
    assert.equal((await e.finished).realizado,true,id);assert.equal(camada.children.length,0,id);
  }
});
test('o feixe reto se estende da origem ao alvo, em vez de voar como ícone',async()=>{
  registro=[];await tocar(manifesto['base-dano-conjurador-c4']).finished;
  const feixe=registro.find(r=>r.tag==='div'&&r.quadros[0].clipPath);
  assert.ok(feixe);assert.equal(parseFloat(feixe.style.width),Math.hypot(500,260));
  assert.match(feixe.style.transform,/rotate\(/);assert.equal(feixe.quadros[1].clipPath,'inset(-50% 0 -50% 0)');
});
test('redução de movimento, geometria inválida e cancelamento não deixam efeitos',async()=>{
  reduzido=true;const n=leituras;assert.equal((await tocar(manifesto['base-dano-c2']).finished).realizado,false);assert.equal(leituras,n);reduzido=false;
  const camada=new No();const invalido=ArcanoclismaMagias.conjurar({camada,visual:manifesto['base-dano-c2'],origem,destinos:[null]});assert.equal((await invalido.finished).realizado,false);assert.equal(camada.children.length,0);
  const e=tocar(manifesto['base-dano-c2']);e.cancelar();assert.equal((await e.finished).realizado,false);
});
test('o adaptador só executa resolução confirmada, no gatilho correto e com alvo',async()=>{
  const root=new No(),alvo=new No(),caster=new No();alvo.rect=destino;caster.rect=origem;
  root.querySelector=s=>s.includes('conj-bloco')?caster:null;
  const api=criarEfeitosMesa(root),base={item:{tipo:'carta'},defId:'base-dano-c2',origemEl:caster,alvoEl:alvo,meu:true};
  assert.equal((await api.resolver(base)).realizado,false);
  assert.equal((await api.resolver({...base,resultado:'anulado'})).realizado,false);
  assert.equal((await api.resolver({...base,resultado:'resolvido',evento:'habilidade'})).realizado,false);
  assert.equal((await api.resolver({...base,resultado:'resolvido',alvoEl:null})).realizado,false);
  assert.equal((await api.resolver({...base,resultado:'resolvido'})).realizado,true);
  assert.equal((await api.resolver({...base,resultado:'resolvido',skinId:'arte-nao-revisada'})).realizado,false);
  api.limpar();assert.equal(document.body.children.length,0);
});
