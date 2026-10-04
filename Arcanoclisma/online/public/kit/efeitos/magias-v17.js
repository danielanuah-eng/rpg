/* Animação vetorial reutilizável por ID/skin. Também abre na prévia file://.
   Só apresentação; dano, cura, custos, compras e alvos pertencem ao motor. */
(() => {
  const cache = new Map();
  const validar = s => typeof s === 'string' && /^\/kit\/efeitos\/[a-z0-9-]+\/(conjurar|trajeto|impacto)-v17\.svg$/.test(s);
  const imagem = async (caminho, resolverURL) => {
    if (!validar(caminho)) throw new Error('Asset visual inválido');
    const url = resolverURL(caminho);
    if (!cache.has(url)) {
      const i = new Image(); i.decoding = 'async'; i.src = url;
      cache.set(url, i.decode().then(() => i).catch(e => { cache.delete(url); throw e; }));
    }
    return cache.get(url);
  };
  const centro = r => ({ x: r.left + r.width / 2, y: r.top + r.height / 2 });
  const rectValido = r => r && [r.left,r.top,r.width,r.height].every(Number.isFinite) && r.width > 0 && r.height > 0;
  const css = (el, valores) => Object.assign(el.style, valores);
  const transform = (x,y,s=1,r=0) => `translate(calc(-50% + ${x}px), calc(-50% + ${y}px)) rotate(${r}deg) scale(${s})`;
  const ease = 'cubic-bezier(.22,.7,.24,1)';

  function conjurar({ camada, visual, origem, destinos, resolverURL = s => s, duracaoMs, geometria }) {
    const animacoes = new Set(), nodes = new Set();
    let cancelado = false;
    const cancelar = () => {
      cancelado = true;
      for (const a of animacoes) a.cancel();
      for (const n of nodes) n.remove();
      animacoes.clear(); nodes.clear();
    };
    const finished = (async () => {
      if (!visual?.okGpt || matchMedia('(prefers-reduced-motion: reduce)').matches) return { realizado: false, motivo: 'movimento reduzido ou efeito sem entrega' };
      let fontes;
      try { fontes = await Promise.all(['conjurar','trajeto','impacto'].map(f => imagem(visual.camadas?.[f],resolverURL))); }
      catch { return { realizado: false, motivo: 'asset indisponivel' }; }
      if (cancelado || !camada?.isConnected) return { realizado: false, motivo: 'cancelado' };
      const g = geometria?.();
      if (g) { origem = g.origem; destinos = g.destinos; }
      if (!rectValido(origem) || !Array.isArray(destinos) || !destinos.length || destinos.some(r=>!rectValido(r))) return { realizado: false, motivo: 'geometria confirmada indisponivel' };
      const cor = /^#[a-f0-9]{6}$/i.test(visual.paleta?.[0]) ? visual.paleta[0] : '#9ab2cf';
      const nucleo = /^#[a-f0-9]{6}$/i.test(visual.paleta?.[1]) ? visual.paleta[1] : '#edf4ff';
      const duracao = Math.max(850,Math.min(2200,Number(duracaoMs ?? visual.duracaoMs)||1300));
      const tarefas = [];
      const origemP = centro(origem);
      const tamanho = Math.max(62,Math.min(150,visual.tamanhoPx || 108));
      function novoImg(fonte, ponto, w=tamanho, h=w) {
        const el = fonte.cloneNode(); el.alt = ''; el.draggable = false;
        css(el,{position:'absolute',left:ponto.x+'px',top:ponto.y+'px',width:w+'px',height:h+'px',pointerEvents:'none',objectFit:'contain',transform:transform(0,0)});
        camada.append(el); nodes.add(el); return el;
      }
      function animar(el, quadros, fracao=1, atraso=0) {
        const a = el.animate(quadros,{duration:duracao*fracao,delay:duracao*atraso,easing:ease,fill:'both'});
        animacoes.add(a);
        tarefas.push(a.finished.catch(()=>{}).finally(()=>{a.cancel();animacoes.delete(a);el.remove();nodes.delete(el);}));
      }
      function pulso(p,atraso=.64,escala=1) {
        const impacto = novoImg(fontes[2],p,tamanho*1.55*escala);
        animar(impacto,[{transform:transform(0,0,.24),opacity:0},{transform:transform(0,0,.85),opacity:.95,offset:.3},{transform:transform(0,-3,1.12,8),opacity:0}],.36,atraso);
      }
      const carga = novoImg(fontes[0],origemP,tamanho*.95);
      animar(carga,[{transform:transform(0,0,.24,-12),opacity:0},{transform:transform(0,0,.88),opacity:.9,offset:.6},{transform:transform(0,0,1.12,8),opacity:0}],.32);

      for (const retangulo of destinos.slice(0,24)) {
        const alvo = centro(retangulo),dx=alvo.x-origemP.x,dy=alvo.y-origemP.y,dist=Math.hypot(dx,dy),giro=Math.atan2(dy,dx)*180/Math.PI;
        const modo = visual.movimento;
        if (modo==='feixe' && dist>4) {
          // O feixe se estende até o alvo: não é um desenho de raio voando.
          // Laser/som seguem uma linha reta; a ramificação elétrica é apenas detalhe.
          const alto=Math.max(22,tamanho*.65), meio=alto/2;
          const feixe=document.createElement('div');
          css(feixe,{position:'absolute',left:origemP.x+'px',top:origemP.y+'px',width:dist+'px',height:alto+'px',transformOrigin:'0 50%',transform:`translateY(-50%) rotate(${giro}deg)`,pointerEvents:'none'});
          const ns='http://www.w3.org/2000/svg',svg=document.createElementNS(ns,'svg');
          svg.setAttribute('viewBox',`0 0 ${dist} ${alto}`);svg.setAttribute('width','100%');svg.setAttribute('height','100%');svg.style.overflow='visible';
          let d=`M0 ${meio}L${dist} ${meio}`;
          if(visual.motivo==='relampago') d=`M0 ${meio}`+Array.from({length:11},(_,i)=>`L${dist*(i+1)/12} ${meio+Math.sin(i*5.3)*(alto*.23)}`).join('')+`L${dist} ${meio}`;
          for(const [largura,alpha,c] of [[alto*.48,.10,cor],[alto*.20,.38,cor],[alto*.07,.95,nucleo]]){
            const p=document.createElementNS(ns,'path');p.setAttribute('d',d);p.setAttribute('fill','none');p.setAttribute('stroke',c);p.setAttribute('stroke-width',largura);p.setAttribute('opacity',alpha);p.setAttribute('stroke-linecap','round');svg.append(p);
          }
          feixe.append(svg);camada.append(feixe);nodes.add(feixe);
          animar(feixe,[{clipPath:'inset(-50% 100% -50% 0)',opacity:0},{clipPath:'inset(-50% 0 -50% 0)',opacity:1,offset:.34},{clipPath:'inset(-50% 0 -50% 0)',opacity:.88,offset:.68},{clipPath:'inset(-50% 0 -50% 0)',opacity:0}],.69,.18);
          const ponta=novoImg(fontes[1],origemP,tamanho*.7);
          animar(ponta,[{transform:transform(0,0,.5,giro),opacity:0},{transform:transform(dx,dy,.8,giro),opacity:1,offset:.6},{transform:transform(dx,dy,1,giro),opacity:0}],.46,.18);
          pulso(alvo,.64);
        } else if (modo==='onda') {
          // Limitada ao(s) campo(s) indicado(s), sem encobrir a interface lateral.
          const area=document.createElement('div');
          css(area,{position:'absolute',left:retangulo.left+'px',top:retangulo.top+'px',width:retangulo.width+'px',height:retangulo.height+'px',overflow:'hidden',pointerEvents:'none'});
          camada.append(area);nodes.add(area);
          const onda=fontes[1].cloneNode();
          css(onda,{position:'absolute',width:Math.max(160,retangulo.width*.7)+'px',height:retangulo.height+'px',left:'-65%',top:'0',objectFit:'contain'});area.append(onda);
          animar(area,[{opacity:0},{opacity:1,offset:.18},{opacity:1,offset:.78},{opacity:0}],.83,.13);
          animar(onda,[{transform:'translateX(0) rotate(-8deg)'},{transform:'translateX(215%) rotate(8deg)'}],.79,.13);
          pulso(alvo,.61,Math.min(1.6,retangulo.width/150));
        } else if (modo==='local') {
          const local=novoImg(fontes[1],alvo,tamanho*1.4);
          animar(local,[{transform:transform(0,18,.25,-14),opacity:0},{transform:transform(0,-12,.95,6),opacity:.95,offset:.42},{transform:transform(0,-48,.78,14),opacity:0}],.78,.15);
          pulso(alvo,.66,.85);
        } else if (modo==='portal' || modo==='selo' || modo==='alvo') {
          const p=novoImg(fontes[1],alvo,Math.max(tamanho*1.7,retangulo.width*1.35));
          animar(p,[{transform:transform(0,0,.16,-18),opacity:0},{transform:transform(0,0,1,0),opacity:.95,offset:.35},{transform:transform(0,0,modo==='portal'?.06:.8,modo==='selo'?0:12),opacity:0}],.83,.12);
          pulso(alvo,.69,.8);
        } else {
          const p=novoImg(fontes[1],origemP,tamanho);
          const cura=modo==='cura',ressurgir=modo==='ressurgir';
          // Cura tem chegada suave; ressurgimento sobe do cemitério ao campo.
          animar(p,[{transform:transform(0,ressurgir?35:0,.4,giro),opacity:0},{transform:transform(dx*.13,dy*.13,.8,giro),opacity:.9,offset:.17},{transform:transform(dx,dy,.98,cura?0:giro),opacity:1,offset:.8},{transform:transform(dx,dy-(cura?12:0),.5,cura?0:giro),opacity:0}],.75,.14);
          pulso(alvo,.64,cura?1.1:1);
        }
      }
      await Promise.all(tarefas);
      return { realizado: !cancelado, motivo: cancelado?'cancelado':undefined };
    })().catch(err=>{cancelar();return {realizado:false,motivo:String(err?.message||err)};});
    return { finished, cancelar };
  }
  globalThis.ArcanoclismaMagias = Object.freeze({ conjurar, versao: 17 });
})();
