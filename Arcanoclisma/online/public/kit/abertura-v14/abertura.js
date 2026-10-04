/* Arcanoclisma: apresentação visual. Nenhuma regra, sorteio ou estado de jogo. */
(function (global) {
  'use strict';
  const modelo = global.ARCANOCLISMA_D20_MODELO;
  const dot = (a,b) => a.reduce((s,v,i) => s+v*b[i],0);
  const escala = (a,k) => a.map(v=>v*k);
  const norma = a => escala(a,1/Math.hypot(...a));
  const eixo = (v,a) => [...escala(norma(v),Math.sin(a/2)),Math.cos(a/2)];
  const produto = (a,b) => [
    a[3]*b[0]+a[0]*b[3]+a[1]*b[2]-a[2]*b[1],
    a[3]*b[1]-a[0]*b[2]+a[1]*b[3]+a[2]*b[0],
    a[3]*b[2]+a[0]*b[1]-a[1]*b[0]+a[2]*b[3],
    a[3]*b[3]-a[0]*b[0]-a[1]*b[1]-a[2]*b[2]
  ];
  const slerp = (a,b,t) => {
    let d=dot(a,b);
    if(d<0){b=escala(b,-1);d=-d;}
    if(d>.9995)return norma(a.map((v,i)=>v+(b[i]-v)*t));
    const angle=Math.acos(Math.min(1,d)),sin=Math.sin(angle);
    return a.map((v,i)=>(v*Math.sin((1-t)*angle)+b[i]*Math.sin(t*angle))/sin);
  };
  const suave = t => t*t*(3-2*t);
  const reduzido = () => Boolean(global.matchMedia?.('(prefers-reduced-motion: reduce)').matches);
  const validar = n => {
    if(!Number.isInteger(n)||n<1||n>20)throw new RangeError('O resultado confirmado deve ser um inteiro de 1 a 20.');
    return n;
  };
  const alturaApoio = q => -Math.min(...modelo.vertices.map(v =>
    v[1]+2*(q[3]*(q[2]*v[0]-q[0]*v[2])+q[2]*(q[1]*v[2]-q[2]*v[1])-q[0]*(q[0]*v[1]-q[1]*v[0]))
  ));
  function desenharFace(ctx,n,x,y,tamanho) {
    const [a,b]=modelo.cores[(n-1)%modelo.cores.length];
    const s=tamanho/512;
    ctx.save();ctx.translate(x,y);ctx.scale(s,s);
    const grad=ctx.createLinearGradient(40,472,472,94);
    grad.addColorStop(0,a);grad.addColorStop(.65,b);grad.addColorStop(1,'#252353');
    ctx.beginPath();ctx.moveTo(40,472);ctx.lineTo(472,472);ctx.lineTo(256,94);ctx.closePath();
    ctx.fillStyle=grad;ctx.fill();ctx.strokeStyle='#6574b7';ctx.lineWidth=3;ctx.stroke();
    ctx.beginPath();ctx.moveTo(66,451);ctx.lineTo(446,451);ctx.lineTo(256,125);ctx.closePath();
    ctx.strokeStyle='rgba(183,155,98,.48)';ctx.lineWidth=2;ctx.stroke();
    ctx.strokeStyle='rgba(149,186,255,.35)';ctx.beginPath();
    ctx.moveTo(92,431);ctx.quadraticCurveTo(156,407,158,323);ctx.quadraticCurveTo(164,246,210,201);
    ctx.moveTo(406,432);ctx.quadraticCurveTo(340,396,346,328);ctx.quadraticCurveTo(352,266,310,213);ctx.stroke();
    const gold=ctx.createLinearGradient(240,390,240,260);
    gold.addColorStop(0,'#c79744');gold.addColorStop(.45,'#fff5c4');gold.addColorStop(1,'#f3d276');
    ctx.font='bold '+(n<10?150:134)+'px Georgia, serif';ctx.textAlign='center';ctx.textBaseline='middle';
    ctx.lineWidth=5;ctx.strokeStyle='#171531';ctx.strokeText(String(n),256,349);
    ctx.fillStyle=gold;ctx.fillText(String(n),256,349);ctx.restore();
  }
  function criarAtlas(limite=4096) {
    const tile=Math.min(512,Math.floor(limite/5));
    const canvas=document.createElement('canvas');
    canvas.width=tile*5;canvas.height=tile*4;
    const ctx=canvas.getContext('2d');
    ctx.fillStyle='#0c102c';ctx.fillRect(0,0,canvas.width,canvas.height);
    for(let n=1;n<=20;n++)desenharFace(ctx,n,((n-1)%5)*tile,Math.floor((n-1)/5)*tile,tile);
    return canvas;
  }
  const vertexSource = [
    'attribute vec3 aPosition; attribute vec3 aNormal; attribute vec2 aUV;',
    'uniform mat4 uProjection; uniform vec4 uQuaternion; uniform vec3 uTranslation;',
    'varying vec2 vUV; varying vec3 vNormal;',
    'vec3 rotateQ(vec3 p,vec4 q){return p+2.0*cross(q.xyz,cross(q.xyz,p)+q.w*p);}',
    'void main(){vUV=aUV;vNormal=rotateQ(aNormal,uQuaternion);',
    'vec3 p=rotateQ(aPosition,uQuaternion)+uTranslation-vec3(0.0,1.05,4.8);',
    'gl_Position=uProjection*vec4(p,1.0);}'
  ].join('\n');
  const fragmentSource = [
    'precision mediump float; uniform sampler2D uAtlas; uniform float uTime;',
    'varying vec2 vUV; varying vec3 vNormal;',
    'void main(){vec3 ink=texture2D(uAtlas,vUV).rgb;vec3 n=normalize(vNormal);',
    'vec3 l=normalize(vec3(-0.45,0.65,1.0));float diffuse=max(dot(n,l),0.0);',
    'float gold=step(0.6,ink.r)*step(ink.b*1.13,ink.g);',
    'float spec=pow(max(dot(reflect(-l,n),vec3(0.0,0.0,1.0)),0.0),28.0);',
    'vec3 c=ink*(0.62+0.48*diffuse)+vec3(0.26,0.33,0.55)*spec*0.25;',
    'c+=gold*(vec3(0.10,0.07,0.015)+vec3(0.60,0.42,0.12)*spec);',
    'gl_FragColor=vec4(c,1.0);}'
  ].join('\n');
  function programa(gl) {
    const shaders=[];
    const shader=(type,source)=>{
      const s=gl.createShader(type);gl.shaderSource(s,source);gl.compileShader(s);
      if(!gl.getShaderParameter(s,gl.COMPILE_STATUS)){const message=gl.getShaderInfoLog(s);gl.deleteShader(s);throw new Error(message);}
      shaders.push(s);return s;
    };
    const p=gl.createProgram();
    try{
      gl.attachShader(p,shader(gl.VERTEX_SHADER,vertexSource));
      gl.attachShader(p,shader(gl.FRAGMENT_SHADER,fragmentSource));gl.linkProgram(p);
      if(!gl.getProgramParameter(p,gl.LINK_STATUS))throw new Error(gl.getProgramInfoLog(p));
      return p;
    }catch(error){gl.deleteProgram(p);throw error;}
    finally{for(const s of shaders)gl.deleteShader(s);}
  }
  function projection(aspect) {
    const f=1/Math.tan(38*Math.PI/360),near=.1,far=30,nf=1/(near-far);
    return new Float32Array([f/aspect,0,0,0,0,f,0,0,0,0,(far+near)*nf,-1,0,0,2*far*near*nf,0]);
  }
  class D20Transplanar {
    constructor(host,{numeroInicial=20,reducaoMovimento}={}) {
      if(!modelo)throw new Error('Carregar d20-modelo.js antes de abertura.js.');
      this.host=host;this.numero=validar(numeroInicial);this.reducaoMovimento=reducaoMovimento;
      this.host.classList.add('d20-transplanar');
      this.canvas=document.createElement('canvas');this.canvas.className='d20-transplanar__canvas';
      this.canvas.setAttribute('role','img');this.host.append(this.canvas);
      this.sombra=document.createElement('span');this.sombra.className='d20-transplanar__sombra';
      this.sombra.setAttribute('aria-hidden','true');this.host.append(this.sombra);
      this.quaternion=modelo.faces.find(f=>f.numero===this.numero).orientacao.slice();
      this.pose={quaternion:this.quaternion,x:0,y:alturaApoio(this.quaternion),time:0,altura:0};
      this.buffers=[];
      try {this._iniciarGL();} catch (_) {this._iniciarAlternativa();}
      this._perdaContexto=event=>{
        event.preventDefault();this.cancelar({concluir:true});this._iniciarAlternativa();this._pintar(this.pose);
      };
      this.canvas.addEventListener('webglcontextlost',this._perdaContexto);
      this._resize=()=>this.redimensionar();
      if(global.ResizeObserver){this.observer=new ResizeObserver(this._resize);this.observer.observe(host);}
      else global.addEventListener('resize',this._resize);
      this.redimensionar();this.mostrarFace(this.numero);
    }
    _iniciarGL() {
      const gl=this.canvas.getContext('webgl',{alpha:true,antialias:true,premultipliedAlpha:false});
      if(!gl)throw new Error('Usar face vetorial de apoio.');
      this.gl=gl;this.program=programa(gl);gl.useProgram(this.program);
      for(const [name,data,size] of [
        ['aPosition',modelo.malha.positions,3],['aNormal',modelo.malha.normals,3],['aUV',modelo.malha.uvs,2]
      ]){
        const buffer=gl.createBuffer();this.buffers.push(buffer);gl.bindBuffer(gl.ARRAY_BUFFER,buffer);
        gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(data),gl.STATIC_DRAW);
        const id=gl.getAttribLocation(this.program,name);gl.enableVertexAttribArray(id);gl.vertexAttribPointer(id,size,gl.FLOAT,false,0,0);
      }
      this.uniforms={};
      for(const name of ['uProjection','uQuaternion','uTranslation','uAtlas','uTime'])this.uniforms[name]=gl.getUniformLocation(this.program,name);
      this.texture=gl.createTexture();gl.bindTexture(gl.TEXTURE_2D,this.texture);
      // Atlas gerado no próprio navegador: sem fetch, CORS, imagem remota ou dependência.
      gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA,gl.RGBA,gl.UNSIGNED_BYTE,criarAtlas(gl.getParameter(gl.MAX_TEXTURE_SIZE)));
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MIN_FILTER,gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAG_FILTER,gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_WRAP_S,gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_WRAP_T,gl.CLAMP_TO_EDGE);
      gl.enable(gl.DEPTH_TEST);gl.enable(gl.CULL_FACE);gl.clearColor(0,0,0,0);
      gl.uniform1i(this.uniforms.uAtlas,0);
    }
    _iniciarAlternativa() {
      this.gl=null;
      if(this.canvas.getContext('2d')===null){
        const replacement=document.createElement('canvas');replacement.className=this.canvas.className;
        replacement.setAttribute('role','img');this.canvas.replaceWith(replacement);this.canvas=replacement;
      }
      this.ctx=this.canvas.getContext('2d');
    }
    redimensionar() {
      if(this.destruido)return;
      const rect=this.host.getBoundingClientRect(),dpr=Math.min(global.devicePixelRatio||1,2.5);
      this.canvas.width=Math.max(1,Math.round(rect.width*dpr));
      this.canvas.height=Math.max(1,Math.round(rect.height*dpr));
      this._pintar(this.pose);
    }
    _pintar(pose) {
      if(this.destruido)return;this.pose=pose;this.quaternion=pose.quaternion;
      this.sombra.style.setProperty('--altura-d20',String(Math.min(1,pose.altura||0)));
      if(this.gl){
        const gl=this.gl;gl.viewport(0,0,this.canvas.width,this.canvas.height);
        gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);gl.useProgram(this.program);
        gl.uniformMatrix4fv(this.uniforms.uProjection,false,projection(this.canvas.width/this.canvas.height));
        gl.uniform4fv(this.uniforms.uQuaternion,new Float32Array(pose.quaternion));
        gl.uniform3f(this.uniforms.uTranslation,pose.x,pose.y,0);
        gl.uniform1f(this.uniforms.uTime,pose.time||0);
        gl.drawArrays(gl.TRIANGLES,0,modelo.malha.positions.length/3);
      }else if(this.ctx){
        const ctx=this.ctx,w=this.canvas.width,h=this.canvas.height,size=Math.min(w,h)*.86;
        ctx.clearRect(0,0,w,h);desenharFace(ctx,this.numero,(w-size)/2,(h-size)/2,size);
      }
    }
    mostrarFace(numero) {
      validar(numero);this.cancelar({concluir:false});this.numero=numero;
      const q=modelo.faces.find(f=>f.numero===numero).orientacao.slice();
      this._pintar({quaternion:q,x:0,y:alturaApoio(q),time:0,altura:0});
      this.canvas.setAttribute('aria-label','Dado de vinte faces: '+numero);
    }
    mostrarVista(nome) {
      const vistas={frente:[0,0,0],tras:[0,Math.PI,0],esquerda:[0,-Math.PI/2,0],direita:[0,Math.PI/2,0],superior:[Math.PI/2,0,0],inferior:[-Math.PI/2,0,0]};
      if(!vistas[nome])throw new RangeError('Vista desconhecida.');
      this.cancelar({concluir:false});
      const [x,y,z]=vistas[nome],q=produto(eixo([0,0,1],z),produto(eixo([0,1,0],y),eixo([1,0,0],x)));
      this._pintar({quaternion:q,x:0,y:alturaApoio(q),time:0,altura:0});
      this.canvas.setAttribute('aria-label','Dado de vinte faces, vista '+nome);
    }
    rolar(numero,{duracao=4200}={}) {
      validar(numero);
      if(this.destruido)throw new Error('Instância encerrada.');
      if(!Number.isFinite(duracao)||duracao<=0)throw new RangeError('Duração inválida.');
      this.cancelar({concluir:false});
      const inicial=this.quaternion.slice(),final=modelo.faces.find(f=>f.numero===numero).orientacao.slice();
      this.numero=numero;
      if((this.reducaoMovimento??reduzido())||!this.gl){
        this.mostrarFace(numero);return Promise.resolve({numero,apresentacaoConcluida:true});
      }
      this.canvas.setAttribute('aria-label','Dado de vinte faces em movimento');
      const start=performance.now(),tempoLimite=Math.max(1200,duracao);
      const giroTermino=produto(eixo([.25,1,.3],Math.PI*9),produto(eixo([1,.2,.5],Math.PI*6.21),inicial));
      return new Promise(resolve=>{
        this.pendente={resolve,numero};
        const quadro=now=>{
          const t=Math.min(1,(now-start)/tempoLimite);
          // Preparação, movimento principal, três contatos e acomodação final.
          const giro=suave(Math.min(1,t/.72));
          let q=produto(eixo([.25,1,.3],giro*Math.PI*9),produto(eixo([1,.2,.5],giro*Math.PI*6.21),inicial));
          if(t>.72)q=slerp(giroTermino,final,suave((t-.72)/.28));
          let altura=0;
          for(const [inicio,fim,h] of [[.08,.48,.65],[.48,.64,.28],[.64,.78,.12],[.78,.90,.04]]){
            if(t>inicio&&t<fim){const u=(t-inicio)/(fim-inicio);altura=4*h*u*(1-u);}
          }
          const chao=alturaApoio(q);
          this._pintar({quaternion:q,x:(-.5+1.0*t)*Math.sin(Math.PI*t),y:chao+altura,time:t*4.2,altura});
          if(t<1)this.raf=requestAnimationFrame(quadro);
          else{
            this.raf=null;this.pendente=null;
            this._pintar({quaternion:final,x:0,y:alturaApoio(final),time:4.2,altura:0});
            this.canvas.setAttribute('aria-label','Resultado confirmado: '+numero);
            resolve({numero,apresentacaoConcluida:true});
          }
        };
        this.raf=requestAnimationFrame(quadro);
      });
    }
    cancelar({concluir=true}={}) {
      if(this.raf){cancelAnimationFrame(this.raf);this.raf=null;}
      if(!this.pendente)return;
      const {resolve,numero}=this.pendente;this.pendente=null;
      if(concluir){
        const q=modelo.faces.find(f=>f.numero===numero).orientacao.slice();
        this._pintar({quaternion:q,x:0,y:alturaApoio(q),time:0,altura:0});
        this.canvas.setAttribute('aria-label','Resultado confirmado: '+numero);
      }
      resolve({numero,apresentacaoConcluida:concluir,cancelada:!concluir});
    }
    destruir() {
      this.cancelar({concluir:false});this.destruido=true;this.observer?.disconnect();
      global.removeEventListener('resize',this._resize);
      this.canvas.removeEventListener('webglcontextlost',this._perdaContexto);
      if(this.gl){
        for(const buffer of this.buffers)this.gl.deleteBuffer(buffer);
        if(this.texture)this.gl.deleteTexture(this.texture);
        if(this.program)this.gl.deleteProgram(this.program);
      }
      this.canvas.remove();this.sombra.remove();this.host.classList.remove('d20-transplanar');
    }
  }
  const entradas=new WeakMap(),partidasApresentadas=new Map();
  /**
   * Os elementos/grupos são entregues pelo integrador. Não lê cartas, dados ou permissões.
   * partidaId identifica a abertura; redesenhar a mesma partida não repete a entrada.
   */
  function entrarArena(root,grupos,{partidaId,duracao=1600,intervalo=180,reducaoMovimento}={}) {
    if(!root||partidaId===undefined||partidaId===null)throw new TypeError('Informar arena e partidaId.');
    if(!Number.isFinite(duracao)||duracao<=0||!Number.isFinite(intervalo)||intervalo<0)throw new RangeError('Tempos de entrada inválidos.');
    const atual=entradas.get(root);
    if(atual?.partidaId===partidaId)return atual.promise;
    if(partidasApresentadas.has(partidaId)){
      const entrada=partidasApresentadas.get(partidaId);entradas.set(root,entrada);
      return entrada.promise;
    }
    atual?.cancelar();
    const ordem=['piso','zonas','conjuradores','baralhosMao','controles'];
    const elementos=new Set(),anims=[];
    const direto=reducaoMovimento??reduzido();
    for(let index=0;index<ordem.length;index++){
      const grupo=grupos[ordem[index]];
      const lista=grupo?.nodeType===1?[grupo]:Array.from(grupo||[]);
      for(const element of lista){
        if(!root.contains(element)||elementos.has(element))continue;
        elementos.add(element);
        if(direto||!element.animate)continue;
        const style=getComputedStyle(element);
        const frames=[
          {opacity:0,translate:index===0?'0 12px':'0 28px',filter:'blur(3px)'},
          {opacity:style.opacity,translate:style.translate==='none'?'0 0':style.translate,filter:style.filter}
        ];
        anims.push(element.animate(frames,{
          duration:duracao,delay:index*intervalo,easing:'cubic-bezier(.22,.65,.28,1)',fill:'both'
        }));
      }
    }
    const entrada={partidaId,cancelada:false,cancelar(){this.cancelada=true;for(const a of anims)a.cancel();}};
    entrada.promise=Promise.all(anims.map(a=>a.finished.catch(()=>undefined))).then(()=>{
      for(const a of anims)a.cancel(); // Restaura os estilos originais, sem tirar a perspectiva.
      return {partidaId,apresentacaoConcluida:!entrada.cancelada,cancelada:entrada.cancelada};
    });
    entradas.set(root,entrada);partidasApresentadas.set(partidaId,entrada);
    return entrada.promise;
  }
  function limparEntradaArena(root) {
    const entrada=entradas.get(root);
    entrada?.cancelar();if(entrada)partidasApresentadas.delete(entrada.partidaId);
    entradas.delete(root);
  }
  global.ArcanoclismaAbertura=Object.freeze({D20Transplanar,entrarArena,limparEntradaArena});
})(globalThis);

