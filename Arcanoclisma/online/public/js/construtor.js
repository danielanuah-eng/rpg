// Baralhos: os 9 prontos, os seus e os da comunidade; construtor com a regra das cores (Valores do Conjurador).
import { $, $$, esc, api, DADOS, CONJS, conj, carta, nomeCurto, cenario, cenarioValor, COR_VALOR, VALOR, TIPO, sessao, avisar, janela, confirmar, itensDoConjurador, cartaServe, prontoDe, dataCurta } from './base.js';
import { cartaHTML, conjuradorHTML, desenharCarta, modeloCarta } from './carta.js';

const TAM = DADOS.tamanhoBaralho;
const total = (lista) => lista.reduce((s, [, n]) => s + n, 0);
const chips = (vs) => `<div class="valores-chips">${vs.map((v) => `<span class="chip-valor" style="background:${COR_VALOR[v]}">${esc(VALOR[v])}</span>`).join('')}</div>`;

function cartaoBaralho(b, { autor, acoes = '' } = {}) {
  const c = conj(b.conjurador);
  return `<div class="cartao-baralho" style="--cena:url('${cenarioValor(c.valores[0])}')">
    ${conjuradorHTML(b.conjurador, { width: 96 })}
    <div class="cb-info"><b>${esc(b.nome)}</b><span class="peq">${esc(nomeCurto(c))}${autor ? ` · de ${esc(autor)}` : ''}${b.atualizado ? ` · ${dataCurta(b.atualizado)}` : ''}</span>${chips(c.valores)}
      <span class="peq">${total(b.lista)} cartas · itens: ${(b.itens ?? []).map((i) => esc(carta(i)?.nome ?? i)).join(', ') || 'nenhum'}</span>
      <div class="linha-botoes">${acoes}</div></div></div>`;
}

export async function telaBaralhos(tela, partes, ctx) {
  ctx.fundo(cenario('baralhos'));
  tela.innerHTML = `<div class="titulo-secao"><h1>Baralhos</h1><a class="botao primario" href="#/construtor/novo">+ Montar baralho</a></div>
    <p class="peq">Regra das cores: o baralho tem ${TAM} cartas, só com cartas dos 2 Valores do Conjurador (a cor da bolinha do custo), no máximo ${DADOS.maxCopias} cópias de cada carta. Antes da partida você escolhe um Canal, uma Armadura e um Extra.</p>
    <div class="abas"><button data-aba="meus" class="ativo">Meus</button><button data-aba="prontos">Prontos</button><button data-aba="comunidade">Comunidade</button><button data-aba="colecao">Coleção</button></div>
    <div id="lista-baralhos"><div class="girando"></div></div>`;
  const area = $('#lista-baralhos');
  let meus = [], publicos = [];
  const carregar = async () => {
    [meus, publicos] = await Promise.all([sessao.usuario ? api('/baralhos').then((r) => r.baralhos).catch(() => []) : [], api('/baralhos/publicos').then((r) => r.baralhos).catch(() => [])]);
  };
  const desenhar = async (aba) => {
    for (const b of $$('.abas [data-aba]', tela)) b.classList.toggle('ativo', b.dataset.aba === aba);
    if (aba === 'meus') {
      if (!sessao.usuario) { area.innerHTML = '<div class="painel vazio">Entre na sua conta para montar e guardar baralhos. <button class="botao primario" data-entrar>Entrar</button></div>'; return; }
      area.innerHTML = meus.length ? `<div class="grade g2">${meus.map((b) => cartaoBaralho(b, { acoes: `<a class="botao peq primario" href="#/construtor/${b.id}">Editar</a><button class="botao peq" data-jogar="meu:${b.id}">Jogar contra o bot</button><button class="botao peq perigo" data-apagar="${b.id}">Apagar</button>${b.publico ? '<span class="etiqueta">público</span>' : ''}` })).join('')}</div>` : '<div class="painel vazio">Você ainda não montou nenhum baralho. Comece de um pronto ou do zero.</div>';
    } else if (aba === 'prontos') {
      area.innerHTML = `<div class="grade g2">${DADOS.baralhos.map((b) => cartaoBaralho(b, { acoes: `<button class="botao peq" data-ver-pronto="${b.conjurador}">Ver cartas</button><a class="botao peq" href="#/construtor/copiar-${b.conjurador}">Copiar e editar</a><button class="botao peq primario" data-jogar="pronto:${b.conjurador}">Jogar contra o bot</button>` })).join('')}</div>`;
    } else if (aba === 'comunidade') {
      area.innerHTML = publicos.length ? `<div class="grade g2">${publicos.map((b) => cartaoBaralho(b, { autor: b.autor, acoes: `<button class="botao peq" data-ver-pub="${b.id}">Ver cartas</button><button class="botao peq primario" data-enfrentar="${b.id}">Enfrentar com o bot</button>` })).join('')}</div>` : '<div class="painel vazio">Ninguém publicou um baralho ainda. Marque o seu como público no construtor!</div>';
    } else colecao(area);
  };
  tela.addEventListener('click', async (ev) => {
    const b = ev.target.closest('button,a');
    if (!b) return;
    if (b.dataset.aba) return desenhar(b.dataset.aba);
    if (b.dataset.verPronto) { const p = prontoDe(b.dataset.verPronto); return verLista(p.nome, p.lista); }
    if (b.dataset.verPub) { const p = publicos.find((x) => x.id === b.dataset.verPub); return verLista(p.nome, p.lista); }
    if (b.dataset.jogar) {
      const [tipo, id] = b.dataset.jogar.split(':');
      const fonteB = tipo === 'pronto' ? { conjurador: id, lista: prontoDe(id).lista, itens: prontoDe(id).itens } : meus.find((x) => x.id === id);
      const op = DADOS.baralhos[Math.floor(Math.random() * DADOS.baralhos.length)];
      return ctx.abrirPartidaBot({ eu: { conjurador: fonteB.conjurador, cartas: fonteB.lista.flatMap(([c, n]) => Array(n).fill(c)), itens: fonteB.itens }, bot: { conjurador: op.conjurador, cartas: op.lista.flatMap(([c, n]) => Array(n).fill(c)), itens: op.itens }, estiloBot: op.estilo, nomeBot: `Bot · ${nomeCurto(conj(op.conjurador))}` });
    }
    if (b.dataset.enfrentar) {
      const p = publicos.find((x) => x.id === b.dataset.enfrentar);
      const meu = DADOS.baralhos[0];
      return ctx.abrirPartidaBot({ eu: { conjurador: meu.conjurador, cartas: meu.lista.flatMap(([c, n]) => Array(n).fill(c)), itens: meu.itens }, bot: { conjurador: p.conjurador, cartas: p.lista.flatMap(([c, n]) => Array(n).fill(c)), itens: p.itens }, estiloBot: prontoDe(p.conjurador).estilo, nomeBot: `${p.nome} (de ${p.autor})` });
    }
    if (b.dataset.apagar) {
      if (!(await confirmar('Apagar este baralho?', { sim: 'Apagar' }))) return;
      await api('/baralhos/' + b.dataset.apagar, { metodo: 'DELETE' });
      await carregar();
      desenhar('meus');
    }
  });
  await carregar();
  desenhar(partes[0] ?? (sessao.usuario && meus.length ? 'meus' : 'prontos'));
}

function verLista(nome, lista) {
  janela(`<h2>${esc(nome)}</h2><div class="vitrine">${lista.map(([id, n]) => `<div class="item-vitrine" data-zoom="${id}">${cartaHTML(id, { width: 128 })}<span class="contador-copias">${n}</span></div>`).join('')}</div>`, { classe: 'larga' });
}

function colecao(area) {
  const todas = Object.values(DADOS.catalogo.cartas).filter((c) => c.tipo !== 'oculta');
  area.innerHTML = `<div class="filtros"><input type="search" placeholder="Buscar carta" id="busca-col"><select id="tipo-col"><option value="">Todos os tipos</option>${Object.entries(TIPO).map(([k, r]) => `<option value="${k}">${r}</option>`).join('')}</select><select id="valor-col"><option value="">Todos os Valores</option><option value="-">Sem Valor</option>${Object.entries(VALOR).map(([k, r]) => `<option value="${k}">${r}</option>`).join('')}</select></div><p class="peq" id="n-col"></p><div class="vitrine" id="vit-col"></div>`;
  const f = () => {
    const q = $('#busca-col').value.toLowerCase(), t = $('#tipo-col').value, v = $('#valor-col').value;
    const lista = todas.filter((c) => (!q || (c.nome + ' ' + c.texto).toLowerCase().includes(q)) && (!t || c.tipo === t) && (!v || (v === '-' ? !c.valores.length : c.valores.includes(v)))).sort((a, b) => a.custo - b.custo || a.nome.localeCompare(b.nome));
    $('#n-col').textContent = `${lista.length} cartas`;
    $('#vit-col').innerHTML = lista.map((c) => `<div class="item-vitrine" data-zoom="${c.id}">${cartaHTML(c.id, { width: 150 })}</div>`).join('');
  };
  area.oninput = f;
  f();
}
document.addEventListener('click', (ev) => {
  const z = ev.target.closest('[data-zoom]');
  if (!z || ev.target.closest('button')) return;
  const c = carta(z.dataset.zoom);
  janela(`${desenharCarta(modeloCarta(z.dataset.zoom), { width: 320 }).outerHTML}<div class="zoom-info painel"><h3>${esc(c.nome)}</h3><p class="peq">${esc(TIPO[c.tipo] ?? c.tipo)} · custo ${c.custo} · Grau ${c.grau} · gera ${c.arcanoGerado}${c.velocidade === 'repentina' ? ' · Repentina' : ''}</p><p>${esc(c.texto || 'Sem texto de regras.')}</p><p class="peq">Valores: ${esc(c.valores.map((v) => VALOR[v]).join(' + ') || 'sem Valor (qualquer Conjurador)')}</p>${DADOS.visual[c.id]?.nomeMecanico ? `<p class="peq">Nome no motor: ${esc(DADOS.visual[c.id].nomeMecanico)}</p>` : ''}</div>`, { classe: 'carta-zoom' });
});

export async function telaConstrutor(tela, [id], ctx) {
  const u = await ctx.exigirConta('Entre na sua conta para montar e guardar baralhos.');
  if (!u) return ctx.navegar('baralhos');
  let b = { id: null, nome: 'Novo baralho', conjurador: null, lista: [], itens: [], publico: false };
  if (id?.startsWith('copiar-')) {
    const p = prontoDe(id.slice(7));
    b = { id: null, nome: `${nomeCurto(conj(p.conjurador))} (meu)`, conjurador: p.conjurador, lista: p.lista.map((x) => [...x]), itens: [...p.itens], publico: false };
  } else if (id && id !== 'novo') {
    const meus = (await api('/baralhos')).baralhos;
    const m = meus.find((x) => x.id === id);
    if (!m) { avisar('Baralho não encontrado.', 'erro'); return ctx.navegar('baralhos'); }
    b = { ...m, lista: m.lista.map((x) => [...x]) };
  }
  if (!b.conjurador) return escolherConjurador(tela, ctx, (c) => { b.conjurador = c; b.nome = `Baralho de ${nomeCurto(conj(c))}`; b.itens = prontoDe(c).itens; montar(tela, b, ctx); });
  montar(tela, b, ctx);
}

function escolherConjurador(tela, ctx, ok) {
  ctx.fundo(cenario('baralhos'));
  tela.innerHTML = `<h1>Escolha o Conjurador</h1><p class="peq">O Conjurador define os 2 Valores (cores) que o baralho pode usar, a habilidade e a vida.</p>
    <div class="escolha-conj">${CONJS.map((c) => `<button data-c="${c.id}">${conjuradorHTML(c.id, { width: 150 })}<span class="nome-conj">${esc(nomeCurto(c))}</span>${chips(c.valores)}<span class="peq">${esc(c.habilidade)}</span></button>`).join('')}</div>`;
  tela.onclick = (ev) => { const x = ev.target.closest('[data-c]'); if (x) { tela.onclick = null; ok(x.dataset.c); } };
}

function montar(tela, b, ctx) {
  const c = conj(b.conjurador);
  ctx.fundo(cenarioValor(c.valores[0]));
  const qtd = new Map(b.lista);
  const pool = Object.values(DADOS.catalogo.cartas).filter((x) => x.tipo !== 'item' && x.tipo !== 'oculta');
  const filtros = { q: '', tipo: '', so: true, custo: '' };
  tela.innerHTML = `<div class="construtor">
    <div class="ct-esq">
      <div class="ct-cab">${'<button class="botao peq fantasma" data-voltar>← Baralhos</button>'}<input type="text" id="nome-bar" maxlength="50" value="${esc(b.nome)}" aria-label="Nome do baralho"></div>
      <div class="ct-conj">${'<div>' + conjuradorHTML(b.conjurador, { width: 120 }) + '</div>'}<div><b>${esc(nomeCurto(c))}</b>${chips(c.valores)}<p class="peq">${esc(c.habilidade)}</p><button class="botao peq" data-trocar>Trocar Conjurador</button></div></div>
      <div class="filtros"><input type="search" id="q" placeholder="Buscar"><select id="ft"><option value="">Tipos</option>${Object.entries(TIPO).filter(([k]) => k !== 'item').map(([k, r]) => `<option value="${k}">${r}</option>`).join('')}</select><select id="fc"><option value="">Custo</option>${[0, 1, 2, 3, 4, 5, 6, 7, 8].map((n) => `<option>${n}</option>`).join('')}</select><label class="opcao-linha"><input type="checkbox" id="fso" checked> Só as que posso usar</label></div>
      <div class="vitrine ct-pool"></div>
    </div>
    <div class="ct-dir painel">
      <h3>Baralho <span id="ct-total"></span></h3>
      <div class="ct-curva"></div>
      <div class="ct-lista"></div>
      <h3>Side deck (itens)</h3><div class="ct-itens"></div>
      <label class="opcao-linha"><input type="checkbox" id="publico" ${b.publico ? 'checked' : ''}> Público (outros podem ver e enfrentar com o bot)</label>
      <div class="ct-erros peq"></div>
      <div class="linha-botoes"><button class="botao primario" data-salvar>Salvar</button><button class="botao" data-completar>Completar com Minas Fundas</button><button class="botao fantasma" data-limpar>Limpar</button></div>
    </div></div>`;
  const pode = (x) => cartaServe(x, b.conjurador);
  const limite = (idc) => (DADOS.semLimite.includes(idc) ? Infinity : DADOS.maxCopias);
  const desenharPool = () => {
    const lista = pool.filter((x) => (!filtros.so || pode(x)) && (!filtros.q || (x.nome + ' ' + x.texto).toLowerCase().includes(filtros.q)) && (!filtros.tipo || x.tipo === filtros.tipo) && (filtros.custo === '' || x.custo === Number(filtros.custo)))
      .sort((a, z) => a.custo - z.custo || a.tipo.localeCompare(z.tipo) || a.nome.localeCompare(z.nome));
    $('.ct-pool', tela).innerHTML = lista.map((x) => `<div class="item-vitrine ${pode(x) ? '' : 'fora'}" data-add="${x.id}" title="${pode(x) ? 'Clique para adicionar · botão direito para ver' : 'Não combina com os Valores do Conjurador'}">${cartaHTML(x.id, { width: 132 })}${qtd.get(x.id) ? `<span class="contador-copias">${qtd.get(x.id)}</span>` : ''}</div>`).join('');
  };
  const desenharLista = () => {
    const n = [...qtd.values()].reduce((s, k) => s + k, 0);
    $('#ct-total', tela).innerHTML = `<span class="${n === TAM ? 'ok' : 'falta'}">${n}/${TAM}</span>`;
    const curva = Array(9).fill(0);
    for (const [idc, k] of qtd) curva[Math.min(8, carta(idc).custo)] += k;
    const max = Math.max(1, ...curva);
    $('.ct-curva', tela).innerHTML = curva.map((k, i) => `<div class="barra-curva" title="${k} carta(s) de custo ${i}${i === 8 ? '+' : ''}"><i style="height:${(k / max) * 100}%"></i><span>${i}${i === 8 ? '+' : ''}</span></div>`).join('');
    const itens = [...qtd].sort(([a], [z]) => carta(a).custo - carta(z).custo || carta(a).nome.localeCompare(carta(z).nome));
    $('.ct-lista', tela).innerHTML = itens.map(([idc, k]) => `<div class="ct-linha ${pode(carta(idc)) ? '' : 'invalida'}"><span class="ct-custo">${carta(idc).custo}</span><span class="ct-nome" data-zoom="${idc}">${esc(carta(idc).nome)}</span><button class="botao peq fantasma" data-menos="${idc}">−</button><b>${k}</b><button class="botao peq fantasma" data-mais="${idc}">+</button></div>`).join('') || '<p class="peq">Clique nas cartas à esquerda para adicionar.</p>';
    $('.ct-itens', tela).innerHTML = ['canal', 'armadura', 'extra'].map((esp) => `<label class="campo">${{ canal: 'Canal (arma, foco arcano ou transcritor)', armadura: 'Armadura', extra: 'Extra' }[esp]}<select data-slot="${esp}"><option value="">nenhum</option>${itensDoConjurador(b.conjurador).filter((x) => x.categoriaItem === esp).map((x) => `<option value="${x.id}" ${b.itens.includes(x.id) ? 'selected' : ''}>${esc(x.nome)}: ${esc(x.texto.replace(/^[^.]+\.\s*/, ''))}</option>`).join('')}</select></label>`).join('');
    const erros = [];
    if (n !== TAM) erros.push(n < TAM ? `Faltam ${TAM - n} cartas.` : `Sobram ${n - TAM} cartas.`);
    for (const [idc, k] of qtd) { if (!pode(carta(idc))) erros.push(`${carta(idc).nome} não combina com os Valores.`); if (k > limite(idc)) erros.push(`${carta(idc).nome}: no máximo ${DADOS.maxCopias}.`); }
    $('.ct-erros', tela).innerHTML = erros.map(esc).join('<br>');
    desenharPool();
  };
  const mudar = (idc, d) => {
    const k = (qtd.get(idc) ?? 0) + d;
    if (d > 0 && (!pode(carta(idc)) || k > limite(idc))) { avisar(pode(carta(idc)) ? `No máximo ${DADOS.maxCopias} cópias.` : 'Esta carta não combina com os Valores do Conjurador.', 'erro'); return; }
    if (k <= 0) qtd.delete(idc); else qtd.set(idc, k);
    desenharLista();
  };
  tela.oninput = (ev) => {
    if (ev.target.id === 'q') filtros.q = ev.target.value.toLowerCase();
    if (ev.target.id === 'ft') filtros.tipo = ev.target.value;
    if (ev.target.id === 'fc') filtros.custo = ev.target.value;
    if (ev.target.id === 'fso') filtros.so = ev.target.checked;
    if (ev.target.dataset.slot) b.itens = $$('[data-slot]', tela).map((s) => s.value).filter(Boolean);
    if (['q', 'ft', 'fc', 'fso'].includes(ev.target.id)) desenharPool();
  };
  tela.oncontextmenu = (ev) => { const x = ev.target.closest('[data-add]'); if (x) { ev.preventDefault(); x.dataset.zoom = x.dataset.add; x.click(); delete x.dataset.zoom; } };
  tela.onclick = async (ev) => {
    const add = ev.target.closest('[data-add]');
    if (add && !add.dataset.zoom) return mudar(add.dataset.add, 1);
    const bt = ev.target.closest('button');
    if (!bt) return;
    if (bt.dataset.mais) return mudar(bt.dataset.mais, 1);
    if (bt.dataset.menos) return mudar(bt.dataset.menos, -1);
    if (bt.matches('[data-voltar]')) return ctx.navegar('baralhos');
    if (bt.matches('[data-trocar]')) return escolherConjurador(tela, ctx, (novo) => { b.conjurador = novo; b.itens = prontoDe(novo).itens; b.lista = [...qtd]; montar(tela, b, ctx); });
    if (bt.matches('[data-limpar]')) { if (await confirmar('Tirar todas as cartas?')) { qtd.clear(); desenharLista(); } return; }
    if (bt.matches('[data-completar]')) {
      // as Minas Fundas dos Valores do Conjurador, até o limite de cópias de cada uma
      let falta = TAM - [...qtd.values()].reduce((s, k) => s + k, 0);
      for (const v of conj(b.conjurador).valores) {
        const id = DADOS.minas?.[v];
        if (!id || falta <= 0) continue;
        const pode = Math.min(falta, DADOS.maxCopias - (qtd.get(id) ?? 0));
        if (pode > 0) { qtd.set(id, (qtd.get(id) ?? 0) + pode); falta -= pode; }
      }
      if (falta > 0) avisar(`Faltam ${falta} carta(s): as Minas Fundas já estão no limite de ${DADOS.maxCopias} cópias.`, 'info', 5000);
      desenharLista(); return;
    }
    if (bt.matches('[data-salvar]')) {
      try {
        const r = await api('/baralhos', { corpo: { id: b.id, nome: $('#nome-bar', tela).value, conjurador: b.conjurador, lista: [...qtd], itens: b.itens, publico: $('#publico', tela).checked } });
        b.id = r.id;
        avisar('Baralho salvo!', 'ok');
        history.replaceState(null, '', '#/construtor/' + r.id);
      } catch (err) { avisar(err.dados?.erros?.join(' ') || err.message, 'erro', 7000); }
    }
  };
  desenharLista();
}
