// Site do Arcanoclisma: rotas, conta, início, escolha de partida, salas online e opiniões.
import { $, $$, esc, api, DADOS, CONJS, conj, carta, nomeCurto, cenario, cenarioValor, COR_VALOR, VALOR, sessao, carregarSessao, aoMudarConta, definirUsuario, avisar, janela, local, itensDoConjurador, prontoDe, listaParaCartas, dataCurta, errosBaralho } from './base.js';
import { conjuradorHTML } from './carta.js';
import { abrirMesa } from './mesa.js';
import { criarPartidaLocal } from './partida-local.js';
import { sortearItensBot } from '/jogo/bot.js';
import { conectarPartida } from './partida-online.js';

const tela = $('#tela');
let limpar = null; // função para fechar a tela atual (mesa, conexões)
/** Confirmação antes de sair da tela atual (Editor com mudanças não salvas): async () => true para sair. */
let antesDeSair = null, perguntandoSaida = false;

// ---------- rotas ----------
const rotas = {
  inicio: telaInicio,
  jogar: telaJogar,
  tutorial: async (p) => (await import('./tutorial.js')).telaTutorial(tela, p, ctx),
  baralhos: async (p) => (await import('./construtor.js')).telaBaralhos(tela, p, ctx),
  construtor: async (p) => (await import('./construtor.js')).telaConstrutor(tela, p, ctx),
  comentarios: async (p) => (await import('./comunidade.js')).telaComentarios(tela, p, ctx),
  ideias: async (p) => (await import('./comunidade.js')).telaIdeias(tela, p, ctx),
  sala: telaSala,
  bot: telaPartidaBot,
  conta: telaConta,
  editor: async (p) => (await import('./editor.js')).telaEditor(tela, p, ctx),
  lore: async (p) => (await import('./lore.js')).telaLore(tela, p, ctx),
  'palavras-chave': async () => {
    const { palavrasChaveHTML } = await import('./palavras-chave.js');
    fundo(cenario('tutorial'));
    tela.innerHTML = `<div class="titulo-secao"><h1>Palavras-chave</h1></div><p class="peq">O que significa cada termo escrito nas cartas.</p><div class="painel pc-lista">${palavrasChaveHTML(esc)}</div>`;
  },
};
async function navegar() {
  const [nome, ...partes] = (location.hash.replace(/^#\/?/, '') || 'inicio').split('/');
  limpar?.();
  limpar = null;
  antesDeSair = null;
  $('#menu').classList.remove('aberto');
  for (const a of $$('#menu a')) a.classList.toggle('ativo', a.dataset.r === nome || (nome === 'construtor' && a.dataset.r === 'baralhos'));
  tela.classList.remove('cheia');
  document.body.style.removeProperty('--x');
  const f = rotas[nome] ?? telaInicio;
  tela.innerHTML = '<div class="girando"></div>';
  try { await f(partes.map(decodeURIComponent)); } catch (err) { console.error(err); tela.innerHTML = `<div class="painel vazio">Algo deu errado: ${esc(err.message)}</div>`; }
  tela.focus({ preventScroll: true });
}
const fundo = (url) => $('#fundo').style.setProperty('--cena', `url('${url}')`);
export const ctx = { navegar: (r) => { location.hash = '#/' + r; }, fundo, exigirConta, opinar, definirLimpeza: (f) => { limpar = f; }, definirSaida: (f) => { antesDeSair = f; }, abrirPartidaBot };

// ---------- conta ----------
function desenharConta() {
  const u = sessao.usuario;
  $('#conta-topo').innerHTML = u
    ? `${u.admin ? '<a class="botao peq" href="#/editor" title="Editar cartas e Conjuradores">✎ Editor</a> ' : ''}<a class="botao peq fantasma" href="#/conta" title="${esc(u.email)}">👤 ${esc(u.nome)}${u.admin ? ' · autor' : ''}</a>`
    : '<button class="botao peq primario" data-entrar>Entrar</button>';
}
aoMudarConta(desenharConta);
document.addEventListener('click', (ev) => { if (ev.target.closest('[data-entrar]')) entrar(); });
$('#menu-celular').onclick = () => $('#menu').classList.toggle('aberto');

/** Garante que há alguém logado; abre a janela de entrada se não houver. */
export function exigirConta(motivo = 'Entre na sua conta para continuar.') {
  if (sessao.usuario) return Promise.resolve(sessao.usuario);
  return entrar(motivo);
}
function entrar(motivo) {
  return new Promise((ok) => {
    let aba = 'entrar';
    const j = janela(`<h2>Entrar no Arcanoclisma</h2>${motivo ? `<p class="peq">${esc(motivo)}</p>` : ''}<div class="login"></div>`, { aoFechar: () => ok(sessao.usuario) });
    const area = $('.login', j.el);
    const desenhar = () => {
      area.innerHTML = `${sessao.googleClientId ? '<div id="google-botao" class="google-botao"></div><div class="ou"><span>ou com e-mail</span></div>' : ''}
        <div class="abas"><button data-aba="entrar" class="${aba === 'entrar' ? 'ativo' : ''}">Entrar</button><button data-aba="criar" class="${aba === 'criar' ? 'ativo' : ''}">Criar conta</button><button data-aba="autor" class="${aba === 'autor' ? 'ativo' : ''}">Sou o autor</button></div>
        <form class="form-login">
          ${aba === 'criar' ? '<label class="campo">Nome que aparece para os outros<input name="nome" type="text" maxlength="40" autocomplete="nickname" required></label>' : ''}
          <label class="campo">E-mail<input name="email" type="email" autocomplete="email" required></label>
          <label class="campo">Senha${aba !== 'entrar' ? ' (mínimo 8 caracteres)' : ''}<input name="senha" type="password" autocomplete="${aba === 'entrar' ? 'current-password' : 'new-password'}" minlength="${aba === 'entrar' ? 1 : 8}" required></label>
          ${aba === 'autor' ? '<label class="campo">Chave do autor<input name="chave" type="password" autocomplete="off" required></label><p class="peq">Confirma a conta do autor e libera o painel de aprovação. A chave fica no Cloudflare (segredo AUTOR_CHAVE).</p>' : ''}
          <div class="linha-botoes"><button class="botao primario">${{ entrar: 'Entrar', criar: 'Criar conta', autor: 'Confirmar' }[aba]}</button></div>
          <p class="erro-form peq" role="alert"></p>
        </form>`;
      if (sessao.googleClientId) montarGoogle($('#google-botao', area), async (credential) => {
        try { const r = await api('/conta/google', { corpo: { credential } }); definirUsuario(r.usuario); avisar(`Bem-vindo, ${r.usuario.nome}!`, 'ok'); j.fechar(); } catch (err) { $('.erro-form', area).textContent = err.message; }
      });
    };
    area.addEventListener('click', (ev) => { const b = ev.target.closest('[data-aba]'); if (b) { aba = b.dataset.aba; desenhar(); } });
    area.addEventListener('submit', async (ev) => {
      ev.preventDefault();
      const f = Object.fromEntries(new FormData(ev.target));
      try {
        const r = await api({ entrar: '/conta/entrar', criar: '/conta/registrar', autor: '/conta/autor' }[aba], { corpo: f });
        definirUsuario(r.usuario);
        avisar(`Bem-vindo, ${r.usuario.nome}!`, 'ok');
        j.fechar();
      } catch (err) { $('.erro-form', area).textContent = err.message; }
    });
    desenhar();
  });
}
let googleCarregado = null;
function montarGoogle(el, aoEntrar) {
  googleCarregado ??= new Promise((ok, falha) => { const s = document.createElement('script'); s.src = 'https://accounts.google.com/gsi/client'; s.onload = ok; s.onerror = falha; document.head.append(s); });
  googleCarregado.then(() => {
    google.accounts.id.initialize({ client_id: sessao.googleClientId, callback: (r) => aoEntrar(r.credential) });
    google.accounts.id.renderButton(el, { theme: 'filled_black', size: 'large', text: 'continue_with', shape: 'pill', locale: 'pt-BR' });
  }).catch(() => { el.textContent = 'Não consegui carregar o botão do Google.'; });
}

async function telaConta() {
  const u = await exigirConta();
  if (!u) return ctx.navegar('inicio');
  fundo(cenario('comunidade'));
  tela.innerHTML = `<div class="painel" style="max-width:560px;margin:auto"><h2>Sua conta</h2><p class="peq">${esc(u.email)}${u.admin ? ' · <b>autor</b> (pode aprovar opiniões e ideias)' : ''}</p>
    <form class="form-nome"><label class="campo">Nome que aparece para os outros<input name="nome" type="text" maxlength="40" value="${esc(u.nome)}"></label><button class="botao primario">Salvar nome</button></form>
    <div class="linha-botoes"><button class="botao perigo" data-sair-conta>Sair da conta</button></div></div>`;
  $('.form-nome', tela).onsubmit = async (ev) => {
    ev.preventDefault();
    try { const r = await api('/conta/nome', { corpo: { nome: ev.target.nome.value } }); definirUsuario(r.usuario); avisar('Nome salvo.', 'ok'); } catch (err) { avisar(err.message, 'erro'); }
  };
  $('[data-sair-conta]', tela).onclick = async () => { await api('/conta/sair', { corpo: {} }); definirUsuario(null); ctx.navegar('inicio'); };
}

// ---------- início ----------
async function telaInicio() {
  fundo(cenario('menu'));
  const destaque = ['lyandra', 'ana', 'osmar'];
  const salvo = local.ler('partida-bot', null);
  tela.innerHTML = `
    <section class="heroi" style="--cena:url('${cenario('menu')}')">
      <div class="conjs-heroi">${destaque.map((c) => conjuradorHTML(c, { width: 190 })).join('')}</div>
      <h1>Arcanoclisma</h1>
      <p class="lema">Dois Conjuradores, um campo de batalha e a magia que sobrou do fim do mundo. Monte seu baralho, suba o seu Grau e quebre o Conjurador rival.</p>
      <div class="linha-botoes">
        <a class="botao primario grande" href="#/tutorial">Aprender a jogar</a>
        <a class="botao grande rosa" href="#/jogar">Jogar agora</a>
        ${salvo?.versao === DADOS.versao ? '<button class="botao grande azul" data-retomar>Retomar partida</button>' : ''}
      </div>
    </section>
    <div class="titulo-secao"><h2>Ideias da comunidade</h2><a class="botao peq" href="#/ideias">Ver todas e mandar a sua</a></div>
    <div class="painel" id="ideias-inicio"><div class="girando"></div></div>
    <div class="titulo-secao"><h2>Modos de jogo</h2></div>
    <div class="grade g3">
      <a class="cartao-modo" href="#/tutorial" style="--cena:url('${cenario('tutorial')}')"><div class="icone">📜</div><h3>Tutorial</h3><p>Uma partida guiada contra o bot que ensina todas as mecânicas.</p></a>
      <a class="cartao-modo" href="#/jogar" style="--cena:url('${cenarioValor('conflituo')}')"><div class="icone">🤖</div><h3>Contra o bot</h3><p>Escolha seu baralho e enfrente um Conjurador ou o baralho de outro jogador.</p></a>
      <a class="cartao-modo" href="#/jogar/online" style="--cena:url('${cenario('saguao')}')"><div class="icone">⚔️</div><h3>Contra jogadores</h3><p>Partida rápida, salas abertas ou sala privada com código.</p></a>
      <a class="cartao-modo" href="#/baralhos" style="--cena:url('${cenario('baralhos')}')"><div class="icone">🃏</div><h3>Baralhos</h3><p>Use os ${DADOS.baralhos.length} baralhos prontos ou monte o seu com os Valores do Conjurador.</p></a>
      <a class="cartao-modo" href="#/comentarios" style="--cena:url('${cenarioValor('persona')}')"><div class="icone">💬</div><h3>Comentários</h3><p>Opiniões, notas e dúvidas sobre o jogo. O autor lê e aprova o que vai mudar.</p></a>
      <a class="cartao-modo" href="#/ideias" style="--cena:url('${cenario('ideias')}')"><div class="icone">✨</div><h3>Ideias</h3><p>Proponha expansões, personagens, cartas, itens e Conjuradores.</p></a>
    </div>
    <div class="titulo-secao"><h2>Os 9 Conjuradores</h2></div>
    <div class="faixa-conjs">${CONJS.map((c) => `<div class="conj-vitrine" style="--cena:url('${cenarioValor(c.valores[0])}')">${conjuradorHTML(c.id, { width: 170 })}<div><b>${esc(nomeCurto(c))}</b><div class="valores-chips">${c.valores.map((v) => `<span class="chip-valor" style="background:${COR_VALOR[v]}">${esc(VALOR[v])}</span>`).join('')}</div><p class="peq">${esc(c.habilidade)}</p></div></div>`).join('')}</div>`;
  $('[data-retomar]', tela)?.addEventListener('click', () => abrirPartidaBot(salvo));
  try {
    const r = await api('/ideias');
    const top = r.ideias.slice(0, 5);
    $('#ideias-inicio').innerHTML = top.length ? top.map((i) => `<div class="ideia-curta"><span class="etiqueta">${esc(i.categoria)}</span> <b>${esc(i.titulo)}</b> <span class="peq">· ${esc(i.autor)} · ▲ ${i.votos}</span></div>`).join('') : '<p class="vazio">Nenhuma ideia ainda. Seja o primeiro a propor uma expansão, um personagem ou uma carta!</p>';
  } catch { $('#ideias-inicio').innerHTML = '<p class="vazio">As ideias aparecem quando o servidor estiver no ar.</p>'; }
}

// ---------- escolher partida ----------
async function meusBaralhos() {
  if (!sessao.usuario) return [];
  try { return (await api('/baralhos')).baralhos; } catch { return []; }
}
async function baralhosPublicos() {
  try { return (await api('/baralhos/publicos')).baralhos; } catch { return []; }
}
function opcoesBaralho(meus, publicos, selecionado) {
  return `<optgroup label="Prontos">${DADOS.baralhos.map((b) => `<option value="pronto:${b.conjurador}" ${selecionado === 'pronto:' + b.conjurador ? 'selected' : ''}>${esc(b.nome)}</option>`).join('')}</optgroup>
    ${meus.length ? `<optgroup label="Meus baralhos">${meus.map((b) => { const inv = (b.erros ?? errosBaralho(b)).length; return `<option value="meu:${b.id}" ${selecionado === 'meu:' + b.id && !inv ? 'selected' : ''} ${inv ? 'disabled' : ''}>${esc(b.nome)} · ${esc(nomeCurto(conj(b.conjurador)))}${inv ? ' (inválido: corrija no construtor)' : ''}</option>`; }).join('')}</optgroup>` : ''}
    ${publicos.length ? `<optgroup label="Da comunidade">${publicos.filter((b) => !(b.erros ?? errosBaralho(b)).length).map((b) => `<option value="publico:${b.id}">${esc(b.nome)} · ${esc(nomeCurto(conj(b.conjurador)))} (de ${esc(b.autor)})</option>`).join('')}</optgroup>` : ''}`;
}
/** Resolve a escolha do <select> em { def, nome, conjurador, estilo }. */
function resolverLocal(ref, meus, publicos, itens) {
  const [tipo, id] = ref.split(':');
  let b;
  if (tipo === 'pronto') { const p = prontoDe(id); b = { conjurador: id, lista: p.lista, itens: p.itens, nome: p.nome }; }
  else b = (tipo === 'meu' ? meus : publicos).find((x) => x.id === id);
  const estilo = prontoDe(b.conjurador).estilo;
  return { def: { conjurador: b.conjurador, cartas: listaParaCartas(b.lista), itens: itens ?? b.itens }, nome: b.nome, conjurador: b.conjurador, estilo, itensPadrao: b.itens };
}
function seletorItens(conjId, escolhidos) {
  const lista = itensDoConjurador(conjId);
  return ['canal', 'armadura', 'extra'].map((esp) => `<label class="campo">${{ canal: 'Canal (arma, foco arcano ou transcritor)', armadura: 'Armadura', extra: 'Extra (da competência ou genérico)' }[esp]}
    <select data-item="${esp}"><option value="">nenhum</option>${lista.filter((x) => x.categoriaItem === esp).map((x) => `<option value="${x.id}" ${escolhidos.includes(x.id) ? 'selected' : ''}>${esc(x.nome)}: ${esc(x.texto.replace(/^[^.]+\.\s*/, ''))}</option>`).join('')}</select></label>`).join('');
}

async function telaJogar(partes) {
  fundo(cenario('saguao'));
  const [meus, publicos] = await Promise.all([meusBaralhos(), baralhosPublicos()]);
  const pref = local.ler('pref-jogar', { meu: 'pronto:lyandra', op: 'pronto:osmar', itens: null });
  const abaOnline = partes[0] === 'online';
  tela.innerHTML = `<h1>Jogar</h1>
    <div class="abas abas-jogar"><button data-modo="bot" class="${abaOnline ? '' : 'ativo'}">🤖 Contra o bot</button><button data-modo="online" class="${abaOnline ? 'ativo' : ''}">⚔️ Contra jogadores</button></div>
    <div class="grade g2">
      <div class="painel">
        <h2>Seu baralho</h2>
        <label class="campo">Baralho<select id="sel-meu">${opcoesBaralho(meus, [], pref.meu)}</select></label>
        <div id="itens-meu" class="grade g3 itens-grade"></div>
        <div id="conj-meu"></div>
        ${!sessao.usuario ? '<p class="peq">Entre na sua conta para usar baralhos que você montou.</p>' : '<a class="botao peq" href="#/construtor/novo">Montar um baralho novo</a>'}
      </div>
      <div class="painel" id="lado-direito"></div>
    </div>`;
  const selMeu = $('#sel-meu');
  let itens = pref.itens;
  const atualizarMeu = () => {
    const r = resolverLocal(selMeu.value, meus, publicos);
    if (!itens || !itens.every((i) => itensDoConjurador(r.conjurador).some((x) => x.id === i))) itens = r.itensPadrao;
    $('#itens-meu').innerHTML = seletorItens(r.conjurador, itens);
    $('#conj-meu').innerHTML = `<div class="conj-escolhido">${conjuradorHTML(r.conjurador, { width: 150 })}<p class="peq">${esc(conj(r.conjurador).habilidade)}</p></div>`;
  };
  selMeu.onchange = () => { itens = null; atualizarMeu(); };
  $('#itens-meu').onchange = () => { itens = $$('#itens-meu select').map((s) => s.value).filter(Boolean); };
  atualizarMeu();
  const lerItens = () => $$('#itens-meu select').map((s) => s.value).filter(Boolean);
  let modoItensBot = pref.itensBot?.modo === 'escolher' ? 'escolher' : 'aleatorio';
  let itensBot = pref.itensBot?.itens ?? null;
  const direito = $('#lado-direito');
  const desenharDireito = (modo) => {
    for (const b of $$('[data-modo]', tela)) b.classList.toggle('ativo', b.dataset.modo === modo);
    if (modo === 'bot') {
      direito.innerHTML = `<h2>Oponente (bot)</h2>
        <label class="campo">Baralho do bot<select id="sel-op"><option value="aleatorio">Aleatório (um dos prontos)</option>${opcoesBaralho([], publicos, pref.op)}</select></label>
        <h3>Equipamento do bot</h3>
        <div class="linha-botoes" id="modo-itens-bot">
          <label class="opcao-linha"><input type="radio" name="itens-bot" value="aleatorio" ${modoItensBot === 'aleatorio' ? 'checked' : ''}> Aleatório</label>
          <label class="opcao-linha"><input type="radio" name="itens-bot" value="escolher" ${modoItensBot === 'escolher' ? 'checked' : ''}> Escolher</label>
        </div>
        <div id="itens-bot" class="grade g3 itens-grade"></div>
        <p class="peq">O bot usa o mesmo jogador automático das simulações de balanceamento. Baralhos da comunidade são os que outros jogadores tornaram públicos.</p>
        <div class="linha-botoes"><button class="botao primario grande" data-comecar-bot>Começar partida</button></div>`;
      const lerItensBot = () => $$('#itens-bot select').map((s) => s.value).filter(Boolean);
      const atualizarBot = () => {
        const op = $('#sel-op').value;
        const el = $('#itens-bot');
        if (modoItensBot === 'aleatorio') { el.innerHTML = '<p class="peq">Um Canal, uma Armadura e um Extra sorteados entre os que servem ao Conjurador do bot (veja em "Ver equipamento" na partida).</p>'; return; }
        if (op === 'aleatorio') { el.innerHTML = '<p class="peq">Escolha o baralho do bot para escolher o equipamento dele.</p>'; return; }
        const r = resolverLocal(op, meus, publicos);
        if (!itensBot || !itensBot.every((i) => itensDoConjurador(r.conjurador).some((x) => x.id === i))) itensBot = r.itensPadrao;
        el.innerHTML = seletorItens(r.conjurador, itensBot);
      };
      $('#sel-op').onchange = () => { itensBot = null; atualizarBot(); };
      $('#itens-bot').onchange = () => { itensBot = lerItensBot(); };
      $('#modo-itens-bot').onchange = (ev) => { modoItensBot = ev.target.value; atualizarBot(); };
      atualizarBot();
      $('[data-comecar-bot]', direito).onclick = () => {
        let op = $('#sel-op').value;
        const escolher = modoItensBot === 'escolher' && op !== 'aleatorio';
        if (op === 'aleatorio') op = 'pronto:' + DADOS.baralhos[Math.floor(Math.random() * DADOS.baralhos.length)].conjurador;
        local.gravar('pref-jogar', { meu: selMeu.value, op: $('#sel-op').value, itens: lerItens(), itensBot: { modo: modoItensBot, itens: escolher ? lerItensBot() : itensBot } });
        const eu = resolverLocal(selMeu.value, meus, publicos, lerItens());
        const bot = resolverLocal(op, meus, publicos, escolher ? lerItensBot() : undefined);
        abrirPartidaBot({ eu: eu.def, bot: bot.def, estiloBot: bot.estilo, nomeBot: `Bot · ${nomeCurto(conj(bot.conjurador))}`, itensBotAleatorio: !escolher });
      };
    } else desenharOnline(direito, () => ({ baralho: selMeu.value, itens: lerItens() }));
  };
  tela.addEventListener('click', (ev) => { const b = ev.target.closest('[data-modo]'); if (b) desenharDireito(b.dataset.modo); });
  desenharDireito(abaOnline ? 'online' : 'bot');
}

async function desenharOnline(el, escolha) {
  if (!sessao.usuario) {
    el.innerHTML = `<h2>Contra jogadores</h2><p>Para jogar online, entre com sua conta de e-mail.</p><button class="botao primario" data-entrar>Entrar</button>`;
    aoMudarConta(() => { if (sessao.usuario && el.isConnected) desenharOnline(el, escolha); });
    return;
  }
  el.innerHTML = `<h2>Contra jogadores</h2>
    <div class="linha-botoes"><button class="botao primario grande" data-rapida>⚡ Partida rápida</button><button class="botao grande" data-criar>Criar sala</button></div>
    <form class="form-codigo"><label class="campo">Entrar com código de sala privada<input name="codigo" type="text" maxlength="5" placeholder="EX: K7M2Q" style="text-transform:uppercase"></label><button class="botao">Entrar</button></form>
    <div class="titulo-secao"><h3>Salas abertas</h3><button class="botao peq" data-atualizar>Atualizar</button></div><div id="lista-salas"><div class="girando"></div></div>`;
  const listar = async () => {
    try {
      const r = await api('/salas');
      $('#lista-salas', el).innerHTML = (r.salas.length ? r.salas.map((s) => `<div class="sala-linha"><div><b>${esc(s.nome)}</b><div class="peq">${s.jogadores.map((j) => `${esc(j.nome)} (${esc(nomeCurto(conj(j.conjurador)))})`).join(' × ') || esc(s.donoNome)} · ${s.estado === 'jogando' ? 'em jogo' : 'esperando oponente'}</div></div>${s.estado === 'aguardando' ? `<button class="botao peq primario" data-entrar-sala="${s.id}">Entrar</button>` : ''}</div>`).join('') : '<p class="vazio">Nenhuma sala aberta. Crie uma ou use a partida rápida.</p>')
        + `<p class="peq">${r.jogando} partida(s) em andamento · ${r.esperandoRapida} jogador(es) na fila rápida.</p>`;
    } catch (err) { $('#lista-salas', el).innerHTML = `<p class="vazio">${esc(err.message)}</p>`; }
  };
  listar();
  const intervalo = setInterval(() => { if (!el.isConnected) clearInterval(intervalo); else listar(); }, 6000);
  const ir = (id) => { local.gravar('escolha-online', escolha()); ctx.navegar('sala/' + id); };
  el.addEventListener('click', async (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    try {
      if (b.matches('[data-rapida]')) { const r = await api('/salas/rapida', { corpo: {} }); ir(r.id); }
      if (b.matches('[data-criar]')) {
        const j = janela(`<h3>Criar sala</h3><form><label class="campo">Nome da sala<input name="nome" type="text" maxlength="40" value="Sala de ${esc(sessao.usuario.nome)}"></label><label class="opcao-linha"><input type="checkbox" name="privada"> Privada (só entra quem tiver o código)</label><button class="botao primario">Criar</button></form>`);
        $('form', j.el).onsubmit = async (e2) => { e2.preventDefault(); const r = await api('/salas', { corpo: { nome: e2.target.nome.value, privada: e2.target.privada.checked } }); j.fechar(); local.gravar('codigo-sala-' + r.id, r.codigo); ir(r.id); };
      }
      if (b.matches('[data-atualizar]')) listar();
      if (b.dataset.entrarSala) ir(b.dataset.entrarSala);
    } catch (err) { avisar(err.message, 'erro'); }
  });
  $('.form-codigo', el).onsubmit = async (ev) => {
    ev.preventDefault();
    try { const r = await api('/salas/codigo/' + ev.target.codigo.value.trim().toUpperCase()); ir(r.id); } catch (err) { avisar(err.message, 'erro'); }
  };
}

// ---------- sala online ----------
async function telaSala([id]) {
  const u = await exigirConta('Entre na sua conta para jogar online.');
  if (!u) return ctx.navegar('jogar/online');
  const escolha = local.ler('escolha-online', { baralho: 'pronto:lyandra', itens: null });
  const codigo = local.ler('codigo-sala-' + id, null);
  tela.innerHTML = `<div class="painel espera-sala" style="max-width:640px;margin:40px auto;text-align:center"><h2>Sala</h2><p class="status-sala">Conectando…</p>${codigo ? `<p>Código da sala: <b class="codigo-grande">${esc(codigo)}</b><br><span class="peq">Mande para quem você quer enfrentar.</span></p>` : ''}<div class="girando"></div><button class="botao" data-sair-sala>Sair</button></div>`;
  let mesa = null;
  const fonte = conectarPartida(id, escolha, {
    aoEstado(m) {
      if (m.status === 'aguardando' || !m.estado) {
        const st = $('.status-sala', tela);
        if (st) st.textContent = m.nomes.length < 2 ? 'Esperando o oponente entrar…' : 'Preparando…';
        return;
      }
      if (!mesa) {
        tela.classList.add('cheia');
        tela.innerHTML = '<div id="mesa-raiz"></div>';
        mesa = abrirMesa($('#mesa-raiz'), fonte, { aoSair: () => ctx.navegar('jogar/online'), opinar: (c) => opinar(c) });
      }
    },
    aoErro(msg, fatal) { avisar(msg, 'erro', 6000); if (fatal && !mesa) ctx.navegar('jogar/online'); },
  });
  tela.addEventListener('click', (ev) => { if (ev.target.closest('[data-sair-sala]')) ctx.navegar('jogar/online'); });
  limpar = () => { mesa?.fechar(); fonte.fechar(); };
}

// ---------- partida contra o bot ----------
let cfgBotAtual = null;
/** Equipamento aleatório do bot: sorteado pela semente da partida (retomar dá o mesmo); a revanche sorteia de novo. */
function sortearEquipBot(cfg) {
  if (!cfg.itensBotAleatorio || cfg.comandos) return cfg;
  const semente = cfg.semente ?? Math.floor(Math.random() * 2 ** 31);
  return { ...cfg, semente, bot: { ...cfg.bot, itens: sortearItensBot(DADOS.catalogo, cfg.bot.conjurador, semente) } };
}
function abrirPartidaBot(cfg) {
  cfg = sortearEquipBot(cfg);
  // baralho inválido (ex.: regra das cores, decisão 37) não entra em partida; o bot também só joga com baralho válido
  for (const [quem, d] of cfg.versao ? [] : [['Seu baralho', cfg.eu], ['O baralho do bot', cfg.bot]]) { // partida salva (retomar): já começou
    const erros = errosBaralho({ conjurador: d.conjurador, lista: [...d.cartas.reduce((m, id) => m.set(id, (m.get(id) ?? 0) + 1), new Map())], itens: d.itens });
    if (erros.length) { avisar(`${quem} não vale: ${erros.join(' ')} Corrija no construtor.`, 'erro', 8000); return; }
  }
  cfgBotAtual = cfg;
  if (location.hash === '#/bot') iniciarBot(cfg); else ctx.navegar('bot');
}
async function telaPartidaBot() {
  if (!cfgBotAtual) { const s = local.ler('partida-bot', null); if (s?.versao === DADOS.versao) cfgBotAtual = s; else return ctx.navegar('jogar'); }
  iniciarBot(cfgBotAtual);
}
function iniciarBot(cfg) {
  if (location.hash !== '#/bot') return;
  cfg = sortearEquipBot(cfg);
  cfgBotAtual = cfg;
  limpar?.();
  tela.classList.add('cheia');
  tela.innerHTML = '<div id="mesa-raiz"></div>';
  const fonte = criarPartidaLocal({
    eu: cfg.eu, bot: cfg.bot, estiloBot: cfg.estiloBot, nomes: ['Você', cfg.nomeBot ?? 'Bot'], semente: cfg.semente, comandos: cfg.comandos,
    chaveSalvar: 'partida-bot', salvar: { eu: cfg.eu, bot: cfg.bot, estiloBot: cfg.estiloBot, nomeBot: cfg.nomeBot, itensBotAleatorio: cfg.itensBotAleatorio },
    revanche: () => { cfgBotAtual = { ...cfg, semente: undefined, comandos: undefined }; iniciarBot(cfgBotAtual); },
    aoTerminar: (e) => { cfgBotAtual = null; if (sessao.usuario) api('/partidas/bot', { corpo: { conjurador: cfg.eu.conjurador, oponente: cfg.bot.conjurador, venci: e.vencedor === 'empate' ? null : e.vencedor === 0, turnos: e.turno, modo: 'bot' } }).catch(() => {}); },
  });
  const mesa = abrirMesa($('#mesa-raiz'), fonte, { aoSair: () => ctx.navegar('jogar'), opinar: (c) => opinar(c) });
  limpar = () => { fonte.parar(); mesa.fechar(); };
}

// ---------- opinar ----------
const TIPOS_OP = [['opiniao', 'Opinião'], ['duvida', 'Dúvida'], ['bug', 'Problema / bug'], ['balanceamento', 'Balanceamento'], ['carta', 'Sobre uma carta'], ['elogio', 'Elogio']];
async function opinar(contexto) {
  const u = await exigirConta('Entre na sua conta para mandar sua opinião.');
  if (!u) return;
  let nota = 0;
  const j = janela(`<h2>Sua opinião</h2><p class="peq">O autor lê tudo e marca o que vai mudar ou ser revisado. Fica visível na aba Comentários.</p>
    <form class="form-opiniao">
      <div class="abas tipos-op">${TIPOS_OP.map(([k, r], i) => `<button type="button" data-tipo="${k}" class="${i === 0 ? 'ativo' : ''}">${r}</button>`).join('')}</div>
      <label class="campo">Nota para o jogo agora (opcional)<span class="estrelas">${[1, 2, 3, 4, 5].map((n) => `<button type="button" data-nota="${n}" aria-label="${n} estrela(s)">★</button>`).join('')}</span></label>
      <label class="campo">Comentário<textarea name="texto" maxlength="4000" required placeholder="O que você achou? O que ficou confuso? Qual carta está forte ou fraca demais?"></textarea></label>
      ${contexto ? `<label class="opcao-linha"><input type="checkbox" name="anexar" checked> Anexar o momento da partida (turno ${contexto.turno}, ${esc(contexto.etapa)}, últimas jogadas)</label>` : ''}
      <button class="botao primario">Enviar</button>
    </form>`);
  let tipo = 'opiniao';
  j.el.addEventListener('click', (ev) => {
    const t = ev.target.closest('[data-tipo]');
    if (t) { tipo = t.dataset.tipo; for (const b of $$('[data-tipo]', j.el)) b.classList.toggle('ativo', b === t); }
    const n = ev.target.closest('[data-nota]');
    if (n) { nota = Number(n.dataset.nota); for (const b of $$('[data-nota]', j.el)) b.classList.toggle('on', Number(b.dataset.nota) <= nota); }
  });
  $('form', j.el).onsubmit = async (ev) => {
    ev.preventDefault();
    try {
      await api('/opinioes', { corpo: { tipo, nota: nota || undefined, texto: ev.target.texto.value, contexto: contexto && ev.target.anexar?.checked ? contexto : undefined } });
      avisar('Obrigado! Sua opinião foi enviada.', 'ok');
      j.fechar();
    } catch (err) { avisar(err.message, 'erro'); }
  };
}

// botão flutuante de opinião fora da mesa
document.body.insertAdjacentHTML('beforeend', '<button class="botao rosa opinar-flutuante" data-opinar-global title="Mandar opinião, nota ou dúvida">💬 Opinar</button>');
document.addEventListener('click', (ev) => { if (ev.target.closest('[data-opinar-global]')) opinar(null); });

window.addEventListener('hashchange', async (ev) => {
  // saída do Editor com mudanças não salvas (pedido do autor, 04/10): pergunta antes; cancelou → o endereço volta
  const rota = (h) => (h.replace(/^#\/?/, '') || 'inicio').split('/')[0];
  if (antesDeSair && rota(location.hash) !== rota(new URL(ev.oldURL).hash)) {
    const destino = location.hash;
    history.replaceState(null, '', ev.oldURL);
    // uma pergunta de saída por vez (bug de 05/10: duas janelas de saída abriam dois "Salvar" ao mesmo tempo)
    if (perguntandoSaida) return;
    perguntandoSaida = true;
    let pode = false;
    try { pode = await antesDeSair(); } finally { perguntandoSaida = false; }
    if (!pode) return;
    history.replaceState(null, '', destino);
  }
  navegar();
});
await carregarSessao();
navegar();
export { dataCurta, carta, CONJS };
