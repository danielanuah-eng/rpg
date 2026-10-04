// Mesa de jogo: desenha a partida e transforma cliques em comandos do motor.
// Serve para os três modos (contra o bot, online e tutorial) por meio de uma "fonte":
//   fonte.e            estado atual (do motor local ou a visão mandada pelo servidor), com e.catalogo
//   fonte.eu           0 ou 1
//   fonte.nomes        [nome do jogador 0, nome do jogador 1]
//   fonte.enviar(cmd)  Promise<string|null> (mensagem de erro de regra ou null)
//   fonte.ouvir(fn)    chama fn a cada mudança
//   fonte.versao       número que cresce a cada mudança
//   opcionais: fonte.chat(texto), fonte.desistir(), fonte.tutorial (dicas, destaques e travas), fonte.prazo(), fonte.conectados()
import { defCarta, custoEfetivo, custoProtecao, origemDaCarta, grauEfetivo, recemChegado, ataqueConjurador } from '/jogo/motor.js';
import { $, $$, esc, DADOS, VALOR, TIPO, textoCarta, janela, local, avisar, conj as conjDefDe } from './base.js';
import { desenharCarta, modeloCarta, modeloConjurador } from './carta.js';
import { criarMovimentosMesa } from './mesa-movimentos-codex.js';
import { RITMO_VISUAL } from './ritmo-visual-v13.js';
import { criarGanchosMesa } from './mesa-ganchos.js';
import { palavrasChaveHTML } from './palavras-chave.js';

const ETAPAS = [['inicio', 'Início'], ['principal', 'Principal'], ['cataclisma', 'Cataclisma'], ['final', 'Final']];
const CFG_PADRAO = { autoPassar: true, esperarPilha: false, pararFinalOponente: true, mostrarDicas: true };

export function abrirMesa(raiz, fonte, { aoSair, opinar, cena } = {}) {
  const EU = fonte.eu, OP = 1 - fonte.eu;
  let e = fonte.e;
  const cfg = { ...CFG_PADRAO, ...local.ler('cfg-mesa', {}) };
  const ui = { menu: null, modo: null, selecionadas: new Set(), aba: 'partida', pilhaVista: new Set(), logVisto: e.logTotal ?? e.log.length, enviando: false, painel: innerWidth > 1100 };
  let fechada = false;
  let timerAuto = null;

  // Layout (pedido do autor): o tabuleiro fica limpo, sem informação no meio; vida e ataque por cima das imagens;
  // Arcano numa esfera na lateral; Grau como cartas viradas no campo; a mão fora da mesa; fases, decisões e a pilha na direita.
  raiz.innerHTML = `<div class="mesa" style="--cena:url('${cena ?? '/arte/cenarios/' + DADOS.cenarios.mesa + '.webp'}')">
    <div class="mesa-esq">
      <div class="mao-op-faixa"></div>
      <div class="mesa-palco">
        <div class="mesa-mundo">
          <img class="arena-piso" src="/kit/raster/arena-jogador-1-ampla-larga-v2.webp" alt="">
          <div class="mesa-conteudo"></div>
        </div>
      </div>
      <div class="mao-faixa"></div>
    </div>
    <aside class="mesa-direita" aria-label="Partida: fases, pilha e decisões"></aside>
    <div class="mesa-menu-acao"></div>
    <div class="mesa-zoom" aria-hidden="true"></div>
  </div>`;
  const conteudo = $('.mesa-conteudo', raiz);
  const palco = $('.mesa-palco', raiz);
  const piso = $('.arena-piso', raiz);
  const mundo = $('.mesa-mundo', raiz);
  /** Área azul (o chão) do piso arena-jogador-1-ampla-larga-v2, em fração da imagem: o tabuleiro inteiro fica dentro dela,
   *  com a linha do meio da mesa sobre a linha do meio desenhada (44% da altura da imagem). */
  const AZUL = { x0: 0.12, x1: 0.89, y0: 0.085, y1: 0.79 };
  /** Menor redução da câmera; abaixo disso o palco rola. */
  const ZOOM_MIN = 0.5;
  let centrou = false;
  /** Câmera afastada (pedido do autor, 03/10): o piso envolve o tabuleiro, que fica dentro da área azul, e o conjunto
   *  é reduzido para caber no palco. No celular, sem redução: rola. */
  function ajustarMesa() {
    if (!palco.isConnected) return;
    mundo.style.zoom = '';
    if (!matchMedia('(min-width: 761px)').matches) { mundo.style.cssText = ''; conteudo.style.left = conteudo.style.top = ''; return; }
    const w = conteudo.offsetWidth, h = conteudo.offsetHeight;
    const pw = w / (AZUL.x1 - AZUL.x0), ph = h / (AZUL.y1 - AZUL.y0);
    Object.assign(mundo.style, { width: pw + 'px', height: ph + 'px' });
    Object.assign(conteudo.style, { left: AZUL.x0 * pw + 'px', top: AZUL.y0 * ph + 'px' });
    const k = Math.max(ZOOM_MIN, Math.min(1, palco.clientWidth / pw, palco.clientHeight / ph));
    mundo.style.zoom = k;
    // na primeira vez, mostra o meio da mesa (os dois Conjuradores)
    if (!centrou && palco.scrollHeight > palco.clientHeight) { centrou = true; palco.scrollTop = (palco.scrollHeight - palco.clientHeight) / 2; }
  }
  new ResizeObserver(() => ajustarMesa()).observe(palco);
  // tela cheia: some o topo do site e o botão troca de nome
  const aoTelaCheia = () => { if (!document.fullscreenElement) document.body.classList.remove('tela-cheia'); if (!fechada && e) lateral.innerHTML = lateralHTML(); };
  document.addEventListener('fullscreenchange', aoTelaCheia);
  const lateral = $('.mesa-direita', raiz);
  const faixaMao = $('.mao-faixa', raiz);
  const faixaMaoOp = $('.mao-op-faixa', raiz);
  const menuAcao = $('.mesa-menu-acao', raiz);
  const zoom = $('.mesa-zoom', raiz);
  const acontecimentos = [];
  const movimentosMesa = criarMovimentosMesa(raiz);
  const ganchos = criarGanchosMesa({ raiz, conteudo, eu: EU });

  // ---------- abertura da partida (entrega do GPT, kit/abertura-v14) ----------
  // Entrada gradual da arena e o D20 Transplanar mostrando os dados que o MOTOR já rolou (e.dados); o dado não decide nada.
  const partidaId = fonte.partidaId ?? (fonte.partidaId = 'p' + Date.now().toString(36) + Math.random().toString(36).slice(2, 6));
  let aberturaFeita = false, dadosAbertura = [];
  function abertura() {
    if (aberturaFeita) return;
    aberturaFeita = true;
    const A = globalThis.ArcanoclismaAbertura;
    if (!A) return;
    const inicio = e.pendencia?.tipo === 'escolherInicio' || (e.turno <= 1 && (e.logTotal ?? e.log.length) <= 12);
    if (!inicio) return;
    try {
      A.entrarArena(raiz, { piso, zonas: [conteudo], baralhosMao: [faixaMao, faixaMaoOp], controles: [lateral] },
        { partidaId, duracao: RITMO_VISUAL.entradaArena, intervalo: RITMO_VISUAL.intervaloEntradaArena }).catch(() => {});
    } catch (err) { console.error('abertura', err); }
    if (!(e.dados?.[0] > 0)) return;
    const caixa = document.createElement('div');
    caixa.className = 'abertura-dados';
    caixa.innerHTML = `<div class="abertura-titulo">Quem começa?</div><div class="abertura-par">
      <div class="abertura-lado"><div class="abertura-dado"></div><b>${esc(nome(EU))}</b></div>
      <div class="abertura-lado"><div class="abertura-dado"></div><b>${esc(nome(OP))}</b></div></div><div class="abertura-resultado"></div>`;
    palco.append(caixa);
    const hosts = [...caixa.querySelectorAll('.abertura-dado')];
    try { dadosAbertura = hosts.map((h) => new A.D20Transplanar(h)); } catch (err) { console.error('d20', err); caixa.remove(); return; }
    const fim = () => { for (const d of dadosAbertura) try { d.destruir(); } catch { /* nada */ } dadosAbertura = []; caixa.remove(); };
    Promise.all([dadosAbertura[0].rolar(e.dados[EU], { duracao: 4200 }), dadosAbertura[1].rolar(e.dados[OP], { duracao: 4200 })]).then(() => {
      if (fechada) return fim();
      const ganhei = e.dados[EU] > e.dados[OP];
      $('.abertura-resultado', caixa).textContent = `${e.dados[EU]} × ${e.dados[OP]} — ${ganhei ? 'você escolhe quem começa' : 'o oponente escolhe quem começa'}`;
      caixa.classList.add('resolvido');
      setTimeout(() => caixa.animate([{ opacity: 1 }, { opacity: 0 }], { duration: 700, easing: 'ease' }).finished.then(fim, fim), 1600);
    }, fim);
    caixa.addEventListener('click', () => { for (const d of dadosAbertura) try { d.cancelar({ concluir: true }); } catch { /* nada */ } });
  }

  // ---------- consultas ----------
  const z = (j) => e.jogadores[j];
  const inst = (id) => e.instancias[id];
  const def = (id) => defCarta(e, id);
  const ehConj = (id) => inst(id)?.ehConjurador;
  const conjDef = (j) => conjDefDe(inst(z(j).conjuradorId).defId);
  const nomeInst = (id) => { const i = inst(id); if (!i) return '?'; return i.ehConjurador ? conjDefDe(i.defId).nome.split(',')[0] : def(id).nome; };
  const nome = (j) => (j === EU ? 'Você' : fonte.nomes[j] ?? 'Oponente');
  const tenhoPrioridade = () => !e.pendencia && e.prioridade === EU && e.vencedor === null;
  const minhaJanela = () => e.ativo === EU && e.etapa === 'principal' && e.pilha.length === 0 && e.combate === null && !e.pendencia && e.vencedor === null;
  const vidaDe = (id) => (ehConj(id) ? conjDefDe(inst(id).defId).vida : def(id).vida);
  const podeGrau = () => minhaJanela() && !e.grauNoTurno && !(e.regras.primeiroSemGrauNoTurno1 && e.primeiro === EU && e.turno === 1) && z(EU).grau.length < 12;
  const fontesArcano = () => z(EU).campo.filter((c) => { const a = def(c).ativacao; return a && a.efeitos.every((ef) => ef.tipo === 'arcano') && !(a.suspende && (inst(c).suspensa || recemChegado(e, c))) && a.custo === 0; });
  const ganhoFonte = (c) => def(c).ativacao.efeitos.reduce((s, ef) => s + ef.valor, 0);
  function potencial(excluir) {
    const m = z(EU);
    return m.arcano + m.grau.filter((g) => !inst(g).suspensa).length + fontesArcano().reduce((s, c) => s + ganhoFonte(c), 0)
      + m.mao.filter((x) => x !== excluir).reduce((t, id) => t + def(id).arcanoGerado, 0);
  }
  function ativacaoDe(id) {
    if (id === z(EU).conjuradorId) return conjDef(EU).poder?.ativacao ?? null;
    return def(id).ativacao ?? null;
  }
  const soArcano = (at) => at.efeitos.every((ef) => ef.tipo === 'arcano');
  function podeJogar(id) {
    if (!tenhoPrioridade() || !origemDaCarta(e, EU, id)) return false;
    const d = def(id);
    if (d.velocidade !== 'repentina' && !minhaJanela()) return false;
    if (d.grau > grauEfetivo(e, EU)) return false;
    if ((d.custoValores ?? []).some((v) => !z(EU).grau.some((g) => def(g).valores.includes(v)))) return false;
    return custoEfetivo(e, EU, id) <= potencial(id);
  }
  function podeAtivar(id) {
    const at = ativacaoDe(id);
    if (!at || e.vencedor !== null || e.pendencia) return false;
    const conjurador = id === z(EU).conjuradorId;
    const item = z(EU).itens.includes(id);
    if (item) {
      // itens v2: suspendem o Conjurador e podem pedir descarte ou dano no Conjurador
      const cj = inst(z(EU).conjuradorId);
      if (at.suspendeConjurador && cj.suspensa) return false;
      if ((at.descartar ?? 0) > z(EU).mao.length) return false;
      if (at.danoNoConjurador && conjDef(EU).vida - cj.dano <= at.danoNoConjurador) return false;
    }
    if (at.suspende && (inst(id).suspensa || recemChegado(e, id))) return false;
    if (conjurador && !at.repentina && !minhaJanela()) return false;
    if ((!soArcano(at) || conjurador) && !tenhoPrioridade()) return false;
    return at.custo <= potencial();
  }
  function podeAtacar(id) {
    if (!minhaJanela() || inst(id).suspensa) return false;
    if (ehConj(id)) return ataqueConjurador(e, EU) > 0 || !e.regras.conjuradorSoAtacaComArma;
    return def(id).tipo === 'aliado' && !recemChegado(e, id) && (def(id).ataque ?? 0) > 0;
  }
  /** Você tem alguma resposta agora (carta Repentina, habilidade Repentina ou ativação)? */
  function possoResponder() {
    const m = z(EU);
    for (const id of [...m.mao, ...m.banimento, ...m.cemiterio]) if (def(id).velocidade === 'repentina' && podeJogar(id)) return true;
    const at = conjDef(EU).poder?.ativacao;
    if (at?.repentina && podeAtivar(m.conjuradorId)) return true;
    for (const id of [...m.campo, ...m.itens]) { const a = ativacaoDe(id); if (a && !soArcano(a) && podeAtivar(id)) return true; }
    return false;
  }

  // ---------- alvos ----------
  function tiposDeAlvo(efs) {
    const s = new Set();
    for (const ef of efs ?? []) {
      if (ef.tipo === 'banirDaMao') s.add('mao');
      else if (ef.tipo === 'reanimar') s.add('cemiterio');
      else if ('alvo' in ef && ['escolhido', 'escolhidoAliado', 'escolhidoProprio', 'pilha'].includes(ef.alvo)) s.add(ef.alvo);
    }
    return s;
  }
  function candidatos(tipos, excluir) {
    const lista = new Set();
    const campoTodos = [EU, OP].flatMap((j) => z(j).campo);
    const deAliado = (id) => ['aliado', 'mistica'].includes(def(id).tipo);
    if (tipos.has('pilha')) for (const p of e.pilha) if (p.tipo === 'carta') lista.add(p.instId);
    if (tipos.has('mao')) for (const id of z(EU).mao) lista.add(id);
    if (tipos.has('cemiterio')) for (const j of [EU, OP]) for (const id of z(j).cemiterio) if (def(id).tipo === 'aliado') lista.add(id);
    if (tipos.has('escolhido')) { for (const j of [OP, EU]) lista.add(z(j).conjuradorId); for (const id of campoTodos) lista.add(id); }
    if (tipos.has('escolhidoAliado')) for (const id of campoTodos) if (deAliado(id)) lista.add(id);
    if (tipos.has('escolhidoProprio')) for (const id of z(EU).campo) if (deAliado(id)) lista.add(id);
    lista.delete(excluir);
    return [...lista];
  }
  const alvosDeAtaque = () => [z(OP).conjuradorId, ...z(OP).campo.filter((a) => ['aliado', 'mistica'].includes(def(a).tipo) && def(a).vida !== undefined)];

  // ---------- envio ----------
  async function enviar(cmd, { silencioso = false } = {}) {
    const trava = fonte.tutorial?.permitir?.(cmd);
    if (trava) { avisar(trava, 'info', 5000); return trava; }
    ui.enviando = true;
    const erro = await fonte.enviar({ ...cmd, jogador: EU });
    ui.enviando = false;
    if (erro && !silencioso) avisar(erro, 'erro', 5000);
    agendarAutoPasse();
    return erro;
  }

  /** Prepara uma jogada: alvo, descarte do Grau e pagamento; depois envia. */
  async function preparar(tipo, id) {
    fecharMenu();
    let efs = [], custoBase = 0, descarta = 0, titulo = '';
    if (tipo === 'jogar') {
      const d = def(id);
      efs = [...(d.efeitos ?? []), ...(d.aoEntrar ?? [])];
      custoBase = custoEfetivo(e, EU, id);
      descarta = d.descartaGrau ?? 0;
      titulo = `Jogar ${d.nome}`;
    } else if (tipo === 'ativar') {
      const at = ativacaoDe(id);
      efs = at.efeitos; custoBase = at.custo;
      titulo = `${z(EU).itens.includes(id) ? 'Usar' : 'Ativar'} ${nomeInst(id)}${at.danoNoConjurador ? ` (seu Conjurador toma ${at.danoNoConjurador} de dano)` : ''}`;
    } else titulo = `Atacar com ${nomeInst(id)}`;
    let alvo;
    const tipos = tipo === 'atacar' ? null : tiposDeAlvo(efs);
    const lista = tipo === 'atacar' ? alvosDeAtaque() : tipos.size ? candidatos(tipos, id) : [];
    if (tipo === 'atacar' || tipos.size) {
      if (!lista.length) { avisar('Não há alvo legal para isso agora.', 'erro'); return; }
      alvo = await escolherAlvo(titulo, lista, tipo === 'atacar' ? 'Escolha quem atacar' : 'Escolha o alvo');
      if (alvo === null) return;
    }
    let descartarGrau = [];
    if (descarta) {
      descartarGrau = await escolherCartas(`${titulo}: descarte ${descarta} carta(s) do seu Grau`, z(EU).grau, descarta);
      if (!descartarGrau) return;
    }
    if (tipo === 'atacar') { await enviar({ tipo: 'atacar', atacante: id, alvo, enviarArcano: [] }); return; }
    // custo de descarte (itens v2): escolhe as cartas da mão antes de pagar
    let descartarCusto = [];
    const nDesc = tipo === 'ativar' ? ativacaoDe(id).descartar ?? 0 : 0;
    if (nDesc) {
      descartarCusto = await escolherCartas(`${titulo}: descarte ${nDesc} carta(s) da mão`, z(EU).mao.filter((x) => x !== alvo), nDesc);
      if (!descartarCusto) return;
    }
    let custo = custoBase + custoProtecao(e, EU, alvo);
    if (tipo === 'ativar') { const r = efs.find((ef) => ef.tipo === 'reanimar'); if (r && alvo) custo += r.custoExtra + def(alvo).custo; }
    const pg = await pagar(titulo, custo, tipo === 'jogar' ? id : undefined, alvo, { excluir: descartarCusto });
    if (!pg) return;
    for (const g of pg.grau) if (await enviar({ tipo: 'suspenderGrau', carta: g })) return;
    for (const f of pg.fontes) if (await enviar({ tipo: 'ativar', carta: f, enviarArcano: [] })) return;
    if (tipo === 'jogar') await enviar({ tipo: 'jogar', carta: id, alvo, descartarGrau, enviarArcano: pg.pitch });
    else await enviar({ tipo: 'ativar', carta: id, alvo, enviarArcano: pg.pitch, ...(descartarCusto.length ? { descartarCusto } : {}) });
  }

  function escolherAlvo(titulo, lista, instrucao) {
    return new Promise((ok) => {
      ui.modo = { tipo: 'alvo', ids: new Set(lista), titulo, instrucao, ok };
      render();
    });
  }
  function terminarModo(valor) {
    const m = ui.modo;
    ui.modo = null;
    m?.ok(valor);
    render();
  }

  /** Janela para escolher exatamente n cartas (por id de instância). */
  function escolherCartas(titulo, ids, n, { minimo = n, rotuloOk = 'Confirmar' } = {}) {
    return new Promise((ok) => {
      const sel = new Set();
      const j = janela(`<h3>${esc(titulo)}</h3><div class="vitrine escolha-cartas">${ids.map((id) => `<div class="item-vitrine" data-id="${id}">${desenharCarta(modeloCarta(inst(id).defId), { width: 120 }).outerHTML}</div>`).join('')}</div>
        <div class="linha-botoes"><button class="botao primario" data-ok disabled>${esc(rotuloOk)}</button></div>`, { classe: 'larga', aoFechar: () => ok(null) });
      const botao = $('[data-ok]', j.el);
      const atual = () => { botao.disabled = sel.size < minimo || sel.size > n; botao.textContent = `${rotuloOk} (${sel.size}/${n})`; };
      j.el.addEventListener('click', (ev) => {
        const it = ev.target.closest('.item-vitrine');
        if (it) {
          const id = it.dataset.id;
          if (sel.has(id)) sel.delete(id); else if (sel.size < n) sel.add(id);
          it.classList.toggle('marcada', sel.has(id));
          atual();
        }
        if (ev.target.closest('[data-ok]') && !botao.disabled) { const r = [...sel]; ok(r); j.el.closest('.janela-fundo').remove(); }
      });
      atual();
    });
  }

  /** Pagamento: reserva, Grau, fontes de Arcano e envio de cartas da mão (pitch). Devolve o que usar ou null. */
  function pagar(titulo, custo, jogada, alvo, { soPitch = false, excluir = [] } = {}) {
    const m = z(EU);
    const reserva = m.arcano;
    const grauLivre = soPitch ? [] : m.grau.filter((g) => !inst(g).suspensa);
    const fontes = soPitch ? [] : fontesArcano().filter((f) => f !== jogada && f !== alvo);
    const grauAuto = soPitch ? m.grau.filter((g) => !inst(g).suspensa).length : 0;
    const mao = m.mao.filter((x) => x !== jogada && x !== alvo && !excluir.includes(x));
    if (reserva + grauAuto >= custo) return Promise.resolve({ grau: [], fontes: [], pitch: [] });
    // sugestão automática: Grau, fontes e depois as cartas que geram mais Arcano
    const sel = { grau: new Set(), fontes: new Set(), pitch: [] };
    let falta = custo - reserva - grauAuto;
    for (const g of grauLivre) if (falta > 0) { sel.grau.add(g); falta -= 1; }
    for (const f of fontes) if (falta > 0) { sel.fontes.add(f); falta -= ganhoFonte(f); }
    const ordem = [...mao].sort((a, b) => def(b).arcanoGerado - def(a).arcanoGerado || (def(b).grau > grauEfetivo(e, EU)) - (def(a).grau > grauEfetivo(e, EU)) || def(b).custo - def(a).custo);
    for (const p of ordem) if (falta > 0) { sel.pitch.push(p); falta -= def(p).arcanoGerado; }
    return new Promise((ok) => {
      const j = janela(`<h3>${esc(titulo)}</h3><div class="pagamento"></div>`, { classe: 'larga', aoFechar: () => ok(null) });
      const area = $('.pagamento', j.el);
      const conta = () => {
        const base = reserva + grauAuto + sel.grau.size + [...sel.fontes].reduce((s, f) => s + ganhoFonte(f), 0);
        let r = base; const usar = [];
        for (const p of sel.pitch) { if (r >= custo) break; usar.push(p); r += def(p).arcanoGerado; }
        return { base, total: r, usar };
      };
      const desenhar = () => {
        const c = conta();
        const coberto = c.total >= custo;
        area.innerHTML = `<p class="pag-resumo">Custo <b class="num-ouro">${custo}</b> · Reserva <b>${reserva}</b>${grauAuto ? ` · Grau levantado <b>${grauAuto}</b> (usado sozinho)` : ''} ·
          ${coberto ? `<span class="ok">Coberto (${c.total})${c.total > custo ? `, sobram ${c.total - custo} na reserva` : ''}</span>` : `<span class="falta">Faltam ${custo - c.total}</span>`}</p>
          ${grauLivre.length ? `<h4>Suspender cartas do Grau <span class="peq">(+1 cada; levantam no seu Final)</span></h4><div class="pag-linha">${grauLivre.map((g) => `<button class="pag-item ${sel.grau.has(g) ? 'on' : ''}" data-g="${g}">${esc(def(g).nome)} <b>+1</b></button>`).join('')}</div>` : ''}
          ${fontes.length ? `<h4>Fontes de Arcano no campo</h4><div class="pag-linha">${fontes.map((f) => `<button class="pag-item ${sel.fontes.has(f) ? 'on' : ''}" data-f="${f}">${esc(def(f).nome)} <b>+${ganhoFonte(f)}</b></button>`).join('')}</div>` : ''}
          <h4>Enviar cartas da mão ao Arcano <span class="peq">(vão para o fundo do baralho no seu Final; só as necessárias)</span></h4>
          <div class="vitrine pag-mao">${mao.map((p) => `<div class="item-vitrine ${sel.pitch.includes(p) ? (c.usar.includes(p) ? 'marcada' : 'sobrando') : ''}" data-p="${p}">${desenharCarta(modeloCarta(inst(p).defId), { width: 104 }).outerHTML}<span class="gera">+${def(p).arcanoGerado}</span></div>`).join('') || '<p class="peq">Mão vazia.</p>'}</div>
          <div class="linha-botoes"><button class="botao primario" data-ok ${coberto ? '' : 'disabled'}>Pagar e ${soPitch ? 'anular' : 'confirmar'}</button><button class="botao" data-cancelar>Cancelar</button></div>`;
      };
      area.addEventListener('click', (ev) => {
        const b = ev.target.closest('[data-g],[data-f],[data-p],[data-ok],[data-cancelar]');
        if (!b) return;
        if (b.dataset.g) { sel.grau.has(b.dataset.g) ? sel.grau.delete(b.dataset.g) : sel.grau.add(b.dataset.g); }
        else if (b.dataset.f) { sel.fontes.has(b.dataset.f) ? sel.fontes.delete(b.dataset.f) : sel.fontes.add(b.dataset.f); }
        else if (b.dataset.p) { const i = sel.pitch.indexOf(b.dataset.p); if (i >= 0) sel.pitch.splice(i, 1); else sel.pitch.push(b.dataset.p); }
        else if (b.hasAttribute('data-cancelar')) { j.fechar(); return; }
        else if (b.hasAttribute('data-ok')) { const c = conta(); if (c.total < custo) return; j.el.closest('.janela-fundo').remove(); ok({ grau: [...sel.grau], fontes: [...sel.fontes], pitch: c.usar }); return; }
        desenhar();
      });
      desenhar();
    });
  }

  // ---------- menu de ações de uma carta ----------
  function acoesDe(id) {
    const a = [];
    const i = inst(id);
    if (!i || i.defId === 'oculta') return a;
    const m = z(EU);
    if (m.mao.includes(id) || m.banimento.includes(id) || m.cemiterio.includes(id)) {
      if (podeJogar(id)) a.push(['jogar', m.mao.includes(id) ? 'Jogar' : 'Jogar desta zona', 'primario']);
      if (m.mao.includes(id) && podeGrau()) a.push(['grau', 'Pôr no Grau', 'azul']);
    } else if (m.grau.includes(id)) {
      if (!i.suspensa && !e.pendencia && e.vencedor === null) a.push(['suspGrau', 'Suspender: +1 Arcano', 'azul']);
    } else if (id === m.conjuradorId || m.campo.includes(id) || m.itens.includes(id)) {
      if (podeAtacar(id)) a.push(['atacar', `Atacar (${ehConj(id) ? ataqueConjurador(e, EU) : def(id).ataque})`, 'rosa']);
      const at = ativacaoDe(id);
      if (at && podeAtivar(id)) a.push(['ativar', m.itens.includes(id) ? 'Usar' : id === m.conjuradorId ? 'Usar habilidade' : soArcano(at) ? `Gerar Arcano (+${ganhoFonte(id)})` : 'Ativar', 'primario']);
      // itens com uso (Canal foco/transcritor e Extra) aparecem no menu do Conjurador
      if (id === m.conjuradorId) for (const it of m.itens) if (ativacaoDe(it) && podeAtivar(it)) a.push([`usarItem:${it}`, `Usar ${def(it).nome}${(ativacaoDe(it).consome ?? def(it).categoriaItem === 'extra') ? ' (uso único)' : ''}`, 'primario']);
    }
    if (ehConj(id) && z(i.dono).itens.length) a.push(['equip', 'Ver equipamento', '']);
    a.push(['ver', 'Ver carta', '']);
    return a;
  }
  /** Ações que fazem algo no jogo (sem contar "ver"). */
  const temAcao = (id) => acoesDe(id).some(([k]) => k !== 'ver' && k !== 'equip');
  const itemDe = (j, cat) => z(j).itens.find((x) => def(x).categoriaItem === cat);
  const extraDe = (j) => itemDe(j, 'extra');
  /** Equipamento do Conjurador (Canal, Armadura e Extra): fica fora da mesa e aparece aqui. */
  function verEquipamento(j) {
    const rot = { canal: 'Canal', armadura: 'Armadura', extra: 'Extra' };
    const blocos = ['canal', 'armadura', 'extra'].map((cat) => {
      const id = itemDe(j, cat);
      if (!id) return `<div class="equip-vitrine vazio"><b>${rot[cat]}</b><p class="peq">Vazio.</p></div>`;
      const d = def(id);
      const st = cat === 'armadura' ? `Defesa ${d.defesa} · desgaste ${inst(id).dano}/${d.vidaDesgaste}${e.habUsos?.[j]?.armadura ? ' · já protegeu neste turno' : ''}` : cat === 'canal' && (d.tipoCanal ?? 'arma') === 'arma' ? `Arma · Ataque ${d.ataque}` : d.texto;
      const usar = j === EU && ativacaoDe(id) && podeAtivar(id) ? `<button class="botao primario peq" data-usar="${id}">Usar</button>` : '';
      return `<div class="equip-vitrine">${desenharCarta(modeloCarta(inst(id).defId), { width: 150 }).outerHTML}<b>${rot[cat]}: ${esc(d.nome)}</b><p class="peq">${esc(st)}</p>${usar}</div>`;
    }).join('');
    const jan = janela(`<h3>${j === EU ? "Seu equipamento" : `Equipamento de ${esc(nome(j))}`}</h3><div class="equip-linha">${blocos}</div>`, { classe: 'larga' });
    jan.el.addEventListener('click', (ev) => {
      const b = ev.target.closest('[data-usar]');
      if (!b) return;
      jan.fechar();
      preparar('ativar', b.dataset.usar);
    });
  }
  /** Bloqueio com o próprio Conjurador: pergunta se usa a Armadura (nenhuma protege mais de um ataque por turno). */
  function perguntarArmadura() {
    const arm = itemDe(EU, 'armadura');
    if (!arm || e.habUsos?.[EU]?.armadura) return Promise.resolve(undefined);
    const d = def(arm);
    return new Promise((ok) => {
      const jan = janela(`<h3>Usar a Armadura?</h3><p>${esc(d.nome)} evita até <b>${d.defesa}</b> de dano deste ataque e se desgasta (desgaste ${inst(arm).dano}/${d.vidaDesgaste}). Ela protege só <b>um ataque por turno</b>.</p>
        <div class="linha-botoes"><button class="botao primario" data-arm="1">Usar a Armadura</button><button class="botao" data-arm="0">Não usar</button></div>`, { aoFechar: () => ok(null) });
      jan.el.addEventListener('click', (ev) => {
        const b = ev.target.closest('[data-arm]');
        if (!b) return;
        jan.el.closest('.janela-fundo').remove();
        ok(b.dataset.arm === '1');
      });
    });
  }
  function abrirMenu(id, alvoEl) {
    const acoes = acoesDe(id);
    ui.menu = id;
    const r = alvoEl.getBoundingClientRect();
    const rp = raiz.getBoundingClientRect();
    menuAcao.innerHTML = `<div class="menu-titulo">${esc(nomeInst(id))}</div>${acoes.map(([k, rot, cls]) => `<button class="botao peq ${cls}" data-a="${k}">${esc(rot)}</button>`).join('')}`;
    menuAcao.style.left = Math.max(8, Math.min(rp.width - 200, r.left - rp.left + r.width / 2 - 95)) + 'px';
    menuAcao.classList.add('aberto');
    const emCima = r.top - rp.top - menuAcao.offsetHeight - 14;
    menuAcao.style.top = (emCima > 8 ? emCima : Math.min(rp.height - menuAcao.offsetHeight - 8, r.bottom - rp.top + 8)) + 'px';
    menuAcao.onclick = async (ev) => {
      const b = ev.target.closest('[data-a]');
      if (!b) return;
      const k = b.dataset.a;
      fecharMenu();
      if (k === 'ver') return verCarta(id);
      if (k === 'equip') return verEquipamento(inst(id).dono);
      if (k.startsWith('usarItem:')) return preparar('ativar', k.slice(9));
      if (k === 'grau') return enviar({ tipo: 'colocarGrau', carta: id });
      if (k === 'suspGrau') return enviar({ tipo: 'suspenderGrau', carta: id });
      if (k === 'ativar' && soArcano(ativacaoDe(id)) && id !== z(EU).conjuradorId && !z(EU).itens.includes(id)) return enviar({ tipo: 'ativar', carta: id, enviarArcano: [] });
      return preparar(k, id);
    };
  }
  function fecharMenu() { ui.menu = null; menuAcao.classList.remove('aberto'); menuAcao.innerHTML = ''; }
  function verCarta(id) {
    const i = inst(id);
    const m = i.ehConjurador ? modeloConjurador(i.defId) : modeloCarta(i.defId);
    if (!m) return;
    const d = i.ehConjurador ? null : def(id);
    const extra = [];
    if (i.dano) extra.push(`Dano guardado: ${i.dano}`);
    if (i.suspensa) extra.push('Suspensa');
    if (!i.ehConjurador && recemChegado(e, id)) extra.push('Acabou de entrar (não ataca neste turno)');
    if (d?.palavrasChave?.length) extra.push(d.palavrasChave.join(', '));
    const nomeMec = DADOS.visual[i.defId]?.nomeMecanico;
    janela(`${desenharCarta(m, { width: 300 }).outerHTML}<div class="zoom-info painel"><h3>${esc(m.nome)}</h3>
      <p class="peq">${esc(i.ehConjurador ? 'Conjurador · ' + m.comprometimento : `${TIPO[d.tipo] ?? d.tipo} · custo ${d.custo} · Grau ${d.grau} · gera ${d.arcanoGerado}`)}${nomeMec ? ` · <i>${esc(nomeMec)}</i>` : ''}</p>
      <p>${esc(m.texto || 'Sem texto de regras.')}</p>
      <p class="peq">Valores: ${esc((m.valores ?? []).join(' + ') || 'sem Valor')}</p>${extra.length ? `<p class="peq">${esc(extra.join(' · '))}</p>` : ''}</div>`, { classe: 'carta-zoom' });
  }

  // ---------- desenho ----------
  /** Carta na mesa. status: mostra Ataque e Vida atuais por cima da imagem. verso: desenha virada (Grau). */
  function cartaMesa(id, { largura = 96, oponente = false, classes = '', status = false, verso = false } = {}) {
    const i = inst(id);
    const oculto = !i || i.defId === 'oculta';
    const m = oculto ? null : i.ehConjurador ? modeloConjurador(i.defId) : modeloCarta(i.defId);
    const el = desenharCarta(m, { width: largura, hidden: oculto || verso });
    const vida = oculto ? undefined : vidaDe(id);
    const marcas = [];
    if (status && !oculto && !verso) {
      const atk = i.ehConjurador ? ataqueConjurador(e, i.dono) : def(id).ataque;
      const resto = vida !== undefined ? vida - i.dano : undefined;
      marcas.push(`<span class="marca-status">${atk !== undefined ? `<b class="st-atk" title="Ataque">⚔ ${atk}</b>` : ''}${resto !== undefined ? `<b class="st-vida ${i.dano ? 'ferida' : ''} ${resto <= Math.ceil(vida / 3) ? 'baixa' : ''}" title="Vida restante">♥ ${resto}</b>` : ''}</span>`);
    } else if (!oculto && !verso && i.dano && vida !== undefined) marcas.push(`<span class="marca-vida ${vida - i.dano <= Math.ceil(vida / 3) ? 'baixa' : ''}">♥ ${vida - i.dano}</span>`);
    if (verso && !oculto && i.revelada) marcas.push('<span class="marca-sono" title="Posta no Grau neste turno (à vista)">👁</span>');
    if (!oculto && !verso && !i.ehConjurador && recemChegado(e, id)) marcas.push('<span class="marca-sono" title="Acabou de entrar">zZ</span>');
    if (!oculto && !verso && i.ehConjurador === false && def(id).palavrasChave?.includes('Etéreo')) marcas.push('<span class="marca-kw" title="Etéreo: só Aliados Etéreos o bloqueiam">✦</span>');
    const cls = ['cm', i?.suspensa ? 'suspensa' : '', oponente ? 'do-oponente' : '', classes];
    const acionavel = !oculto && !ui.modo && i.dono === EU && temAcao(id);
    if (acionavel) cls.push('acionavel');
    if (ui.modo?.tipo === 'alvo' && ui.modo.ids.has(id)) cls.push('alvo-ok');
    if (ui.modo?.tipo === 'bloqueio' && ui.modo.ids.has(id)) cls.push('alvo-ok');
    if (e.combate && (e.combate.atacante === id)) cls.push('atacando');
    if (e.combate && (e.combate.alvo === id)) cls.push('alvejado');
    if (e.combate?.bloqueador === id) cls.push('bloqueando');
    if (fonte.tutorial?.destaques?.()?.includes(id)) cls.push('destaque-tutorial');
    return `<div class="${cls.filter(Boolean).join(' ')}" data-id="${id}" style="--w:${largura}px">${el.outerHTML}${marcas.join('')}</div>`;
  }

  // ---------- um lado da mesa ----------
  function ladoHTML(j) {
    const m = z(j);
    const c = conjDef(j);
    const meu = j === EU;
    const grau = m.grau.map((g) => cartaMesa(g, { largura: 50, oponente: !meu, verso: true })).join('');
    const pilhas = `<div class="pilhas-zona">
      <button class="zona-btn" data-zona="baralho" data-j="${j}" title="Baralho">🂠 ${m.baralho.length}</button>
      <button class="zona-btn" data-zona="cemiterio" data-j="${j}" title="Cemitério">☠ ${m.cemiterio.length}</button>
      <button class="zona-btn" data-zona="banimento" data-j="${j}" title="Banimento">⊘ ${m.banimento.length}</button>
      ${m.zonaArcano.length ? `<button class="zona-btn" data-zona="zonaArcano" data-j="${j}" title="Enviadas ao Arcano (vão para o fundo do baralho no Final)">✧ ${m.zonaArcano.length}</button>` : ''}
    </div>`;
    // Layout v10 (Codex/autor): os dois Conjuradores no eixo central, junto da linha do meio, encarando-se; a esfera de Arcano,
    // o Grau (virado) e os montes na mesma fileira; os Aliados e permanentes atrás. O equipamento fica guardado (clique no Conjurador).
    // A ordem das fileiras (Conjurador perto do meio) vem do CSS (.lado-op/.lado-eu .campo/.fileira-fundo { order }).
    const fileira = `<div class="fileira-fundo">
      <div class="esfera-arcano ${m.arcano ? 'cheia' : ''}" title="Arcano na reserva"><b>${m.arcano}</b><small>Arcano</small></div>
      <div class="conj-bloco"><div class="conj-nome">${esc(nome(j))}${fonte.conectados && !meu && !fonte.conectados()[j] ? ' <span class="etiqueta">desconectado</span>' : ''}</div>${cartaMesa(m.conjuradorId, { largura: 118, status: true })}</div>
      <div class="zona-grau"><span class="rot" title="Grau: ${grauEfetivo(e, j)} (cartas no Grau${grauEfetivo(e, j) !== m.grau.length ? ' + bônus' : ''})">◆ Grau ${grauEfetivo(e, j)}</span><div class="grau-cartas">${grau}</div></div>
      ${pilhas}
    </div>`;
    const campo = `<div class="campo campo-${meu ? 'eu' : 'op'}">${m.campo.map((id) => cartaMesa(id, { largura: 114, oponente: !meu, status: true })).join('')}</div>`;
    return `<div class="lado lado-${meu ? 'eu' : 'op'}">${campo}${fileira}</div>`;
  }

  // ---------- painel da direita ----------
  function faixaFase() {
    const meu = e.ativo === EU;
    return `<div class="fase">
      <div class="fase-topo"><span class="fase-turno">Turno ${e.turno || '–'}</span><span class="fase-vez ${meu ? 'minha' : ''}">${e.etapa === 'preparacao' ? 'Preparação' : meu ? 'Seu turno' : `Turno de ${esc(nome(e.ativo))}`}</span>${fonte.prazo?.() ? `<span class="fase-prazo" data-prazo="${fonte.prazo()}"></span>` : ''}</div>
      <div class="fase-etapas">${ETAPAS.map(([k, r]) => `<span class="${e.etapa === k ? 'atual' : ''}">${r}</span>`).join('')}</div>
    </div>`;
  }
  /** A carta que gerou um efeito na pilha (pelo nome guardado no gatilho). */
  function fonteDoGatilho(p) {
    const zc = z(p.controlador);
    if (p.fonte && conjDef(p.controlador).nome === p.fonte) return zc.conjuradorId;
    const zonas = [zc.campo, zc.itens, zc.cemiterio, z(1 - p.controlador).campo];
    for (const zona of zonas) { const id = zona.find((x) => inst(x) && !inst(x).ehConjurador && inst(x).defId !== 'oculta' && def(x).nome === p.fonte); if (id) return id; }
    return null;
  }
  function itemPilhaHTML(p, topo) {
    const dono = p.controlador === EU ? 'Você' : nome(p.controlador);
    const novo = !ui.pilhaVista.has(p.id) ? 'chegando' : '';
    const alvoTxt = (a) => (a && inst(a) ? `<div class="pilha-alvo">🎯 Alvo: <b>${esc(nomeInst(a))}</b></div>` : '');
    const largura = topo ? 150 : 86;
    if (p.tipo === 'carta') {
      const d = def(p.instId);
      return `<div class="pilha-item ${topo ? 'topo' : ''} ${novo} ${p.controlador === EU ? 'meu' : 'dele'}">
        ${cartaMesa(p.instId, { largura })}
        <div class="pilha-info"><div class="pilha-rot">${esc(dono)} joga</div><b>${esc(d.nome)}</b><div class="pilha-efeito">${esc(d.texto || 'Entra em campo.')}</div>${alvoTxt(p.alvo)}</div></div>`;
    }
    if (p.tipo === 'gatilho') {
      const origem = fonteDoGatilho(p);
      return `<div class="pilha-item efeito ${topo ? 'topo' : ''} ${novo} ${p.controlador === EU ? 'meu' : 'dele'}">
        ${origem ? cartaMesa(origem, { largura }) : '<div class="pilha-sem-carta">✦</div>'}
        <div class="pilha-info"><div class="pilha-rot">Efeito de ${esc(dono)}</div><b>${esc(p.fonte ?? 'Efeito')}</b><div class="pilha-efeito">${esc(textoCarta({ tipo: 'temporaria', efeitos: p.efeitos }))}</div>${alvoTxt(p.alvo)}</div></div>`;
    }
    return `<div class="pilha-item ataque ${topo ? 'topo' : ''} ${novo} ${p.controlador === EU ? 'meu' : 'dele'}">
      ${cartaMesa(p.atacante, { largura, status: true })}
      <div class="pilha-info"><div class="pilha-rot">Ataque de ${esc(dono)}</div><b>⚔ ${esc(nomeInst(p.atacante))}</b><div class="pilha-efeito">ataca <b>${esc(nomeInst(p.alvo))}</b>. Quando resolver, o defensor escolhe se bloqueia.</div></div></div>`;
  }

  function painelPartida() {
    const partes = [faixaFase()];
    if (fonte.tutorial?.balao) partes.push(fonte.tutorial.balao());
    const p = e.pendencia;
    if (ui.modo?.tipo === 'alvo') partes.push(`<div class="pendencia modo-alvo"><b>${esc(ui.modo.titulo)}</b><span>${esc(ui.modo.instrucao)}: clique numa carta brilhando na mesa.</span><div class="linha-botoes"><button class="botao peq" data-lista-alvos>Ver lista de alvos</button><button class="botao peq" data-cancelar-alvo>Cancelar</button></div></div>`);
    if (p && p.jogador === EU) partes.push(pendenciaHTML(p));
    else if (p) partes.push(`<div class="pendencia espera">${esc(nome(OP))} está decidindo${{ bloqueio: ' o bloqueio', descartar: ' o descarte', pagarAnulacao: ' se paga para anular', escolherDoTopo: ' as cartas do topo', escolherInicio: ' quem começa' }[p.tipo] ?? ''}…</div>`);
    if (e.combate && !e.pilha.some((x) => x.tipo === 'ataque')) {
      const c = e.combate;
      partes.push(`<div class="combate-faixa">⚔ <b>${esc(nomeInst(c.atacante))}</b> ataca <b>${esc(nomeInst(c.alvo))}</b>${c.bloqueador ? ` · bloqueado por <b>${esc(nomeInst(c.bloqueador))}</b>` : c.etapa === 'bloqueado' ? ' · sem bloqueio' : ''}</div>`);
    }
    if (e.pilha.length) {
      const itens = [...e.pilha].reverse();
      const topo = itens[0];
      partes.push(`<div class="pilha"><div class="pilha-titulo">Pilha <span class="peq">· a de cima resolve primeiro</span></div>
        ${itens.map((x, k) => itemPilhaHTML(x, k === 0)).join('')}
        ${tenhoPrioridade() ? `<div class="pilha-pergunta">${topo.controlador === EU ? 'Passe para o oponente poder responder.' : possoResponder() ? '<b>Responder?</b> Suas cartas Repentinas e habilidades disponíveis estão brilhando.' : 'Você não tem resposta disponível.'}</div>` : `<div class="pilha-pergunta espera">Aguardando ${esc(nome(OP))} passar ou responder…</div>`}
      </div>`);
    }
    partes.push(botaoPrincipal());
    partes.push(`<div class="acontecimentos"><div class="rot-peq">Últimos acontecimentos</div>${acontecimentos.slice(-6).reverse().map((l, k) => `<div class="${k === 0 ? 'novo' : ''}">${esc(l)}</div>`).join('') || '<div class="peq">—</div>'}</div>`);
    return partes.join('');
  }

  function pendenciaHTML(p) {
    if (p.tipo === 'escolherInicio') return `<div class="pendencia"><b>Você venceu no dado (${e.dados[EU]} × ${e.dados[OP]}).</b> Quem começa?<div class="linha-botoes"><button class="botao primario" data-pend="comecar-eu">Eu começo</button><button class="botao" data-pend="comecar-op">O oponente começa</button></div><p class="peq">Quem começa não põe carta no Grau no turno 1.</p></div>`;
    if (p.tipo === 'bloqueio') {
      const c = e.combate;
      const arm = z(EU).itens.find((i) => def(i).categoriaItem === 'armadura');
      return `<div class="pendencia urgente"><b>Bloqueio!</b> ${esc(nomeInst(c.atacante))} ataca ${esc(nomeInst(c.alvo))}.<div class="pilha-item">${cartaMesa(c.atacante, { largura: 86, status: true })}<div class="pilha-info">Clique num bloqueador brilhando na mesa, ou:<div class="linha-botoes"><button class="botao primario" data-pend="naoBloquear">Não bloquear</button></div></div></div>
        <p class="peq">Quem bloqueia fica suspenso. Um Aliado que bloqueia só levanta no seu Final; o Conjurador levanta no Início do seu turno${arm ? ` e, bloqueando, ${esc(def(arm).nome)} evita até ${def(arm).defesa} de dano` : ''}.${!ehConj(c.atacante) && def(c.atacante).palavrasChave?.includes('Etéreo') ? ' O atacante é Etéreo: só Aliados Etéreos bloqueiam.' : ''}</p></div>`;
    }
    if (p.tipo === 'descartar') return `<div class="pendencia urgente"><b>Limite de mão:</b> descarte ${p.quantidade} carta(s). Clique nelas na sua mão.<div class="linha-botoes"><button class="botao primario" data-pend="descartar" ${ui.selecionadas.size === p.quantidade ? '' : 'disabled'}>Descartar (${ui.selecionadas.size}/${p.quantidade})</button></div></div>`;
    if (p.tipo === 'pagarAnulacao') return `<div class="pendencia urgente"><div class="pilha-item">${cartaMesa(p.item.instId, { largura: 86 })}<div class="pilha-info"><b>${esc(def(p.item.instId).nome)}</b> pode ser anulada se você pagar <b>${p.custo}</b> de Arcano.<div class="pilha-efeito">${esc(def(p.item.instId).texto)}</div></div></div><div class="linha-botoes"><button class="botao primario" data-pend="pagarAnul">Pagar ${p.custo} e anular</button><button class="botao" data-pend="naoPagar">Deixar resolver</button></div></div>`;
    if (p.tipo === 'escolherDoTopo') return `<div class="pendencia urgente"><b>Olhar o topo:</b> escolha ${p.pegar} carta(s) para a mão; as outras vão para o fundo do baralho.<div class="vitrine topo-cartas">${p.cartas.map((id) => `<div class="item-vitrine ${ui.selecionadas.has(id) ? 'marcada' : ''}" data-topo="${id}">${desenharCarta(modeloCarta(inst(id).defId), { width: 92 }).outerHTML}</div>`).join('')}</div><div class="linha-botoes"><button class="botao primario" data-pend="topo" ${ui.selecionadas.size === p.pegar ? '' : 'disabled'}>Confirmar</button></div></div>`;
    return '';
  }

  // ---------- mãos (fora da mesa) ----------
  function maoHTML() {
    const m = z(EU);
    const descartando = e.pendencia?.tipo === 'descartar' && e.pendencia.jogador === EU;
    return `<div class="mao">${m.mao.map((id) => {
      const brilho = !ui.modo && (podeJogar(id) ? 'jogavel' : '');
      const sel = descartando && ui.selecionadas.has(id) ? 'marcada' : '';
      const alvo = ui.modo?.tipo === 'alvo' && ui.modo.ids.has(id) ? 'alvo-mao' : '';
      return `<div class="mao-slot ${brilho} ${sel} ${alvo}">${cartaMesa(id, { largura: 104 })}</div>`;
    }).join('') || '<span class="peq mao-vazia">Sua mão está vazia.</span>'}</div>`;
  }
  function maoOpHTML() {
    const n = z(OP).mao.length;
    return `<span class="rot-peq">Mão de ${esc(nome(OP))}: ${n}</span><div class="mao-op">${Array.from({ length: n }, () => '<div class="verso-mini"></div>').join('')}</div>`;
  }

  function botaoPrincipal() {
    if (e.vencedor !== null || ui.modo) return '';
    if (!tenhoPrioridade()) return e.pendencia ? '' : `<div class="barra-acao"><span class="espera-txt">Vez de ${esc(nome(OP))}…</span></div>`;
    let rot = 'Passar';
    if (e.pilha.length) rot = 'Passar: deixar resolver';
    else if (e.combate) rot = 'Passar: resolver o combate';
    else if (minhaJanela()) rot = 'Encerrar fase Principal';
    else if (e.ativo === EU) rot = `Passar (${ETAPAS.find(([k]) => k === e.etapa)?.[1] ?? ''})`;
    return `<div class="barra-acao"><button class="botao primario grande" data-passar>${esc(rot)} ▸</button></div>`;
  }

  function lateralHTML() {
    const abas = [['partida', 'Partida'], ['registro', 'Registro'], ...(fonte.chat ? [['chat', 'Conversa']] : []), ['ajuda', 'Ajuda']];
    let corpo = '';
    if (ui.aba === 'partida') corpo = painelPartida();
    else if (ui.aba === 'registro') corpo = registroHTML();
    else if (ui.aba === 'chat') corpo = `<div class="chat-lista">${(fonte.mensagens?.() ?? []).map((m) => `<div><b>${esc(m.de)}:</b> ${esc(m.texto)}</div>`).join('') || '<p class="peq">Sem mensagens.</p>'}</div><form class="chat-form"><input type="text" maxlength="300" placeholder="Mensagem" aria-label="Mensagem"><button class="botao peq">Enviar</button></form>`;
    else corpo = ajudaHTML();
    return `<div class="lateral-topo"><div class="abas">${abas.map(([k, r]) => `<button data-aba="${k}" class="${ui.aba === k ? 'ativo' : ''}">${r}${k === 'partida' && ui.aba !== 'partida' && (e.pilha.length || e.pendencia?.jogador === EU) ? ' •' : ''}</button>`).join('')}</div></div>
      <div class="lateral-corpo">${corpo}</div>
      <div class="lateral-pe">
        ${opinar ? '<button class="botao rosa peq" data-opinar>💬 Opinar / dúvida</button>' : ''}
        <button class="botao peq" data-tela-cheia title="Tela cheia (Esc sai)">${document.body.classList.contains('tela-cheia') ? '🗗 Sair da tela cheia' : '⛶ Tela cheia'}</button>
        <button class="botao peq" data-config>⚙ Opções</button>
        ${fonte.desistir && e.vencedor === null ? '<button class="botao peq perigo" data-desistir>Desistir</button>' : ''}
        <button class="botao peq fantasma" data-sair>Sair</button>
      </div>`;
  }
  function traduzir(l) {
    return l.replace(/\b([oO]) [Jj]ogador (\d)\b/g, (_, o, k) => (Number(k) === EU ? (o === 'O' ? 'Você' : 'você') : nome(Number(k))))
      .replace(/\b[Jj]ogador (\d)\b/g, (_, k) => (Number(k) === EU ? 'Você' : nome(Number(k))));
  }
  function registroHTML() {
    const blocos = [];
    let atual = { titulo: 'Preparação', linhas: [] };
    for (const l of e.log) {
      const m = l.match(/^Turno (\d+) \((?:j|J)ogador (\d)\): (\w+)\.$/);
      if (m) {
        if (m[3] === 'inicio' || !blocos.length) { if (atual.linhas.length || blocos.length === 0) blocos.push(atual); atual = { titulo: `Turno ${m[1]} · ${Number(m[2]) === EU ? 'você' : nome(Number(m[2]))}`, linhas: [] }; }
        else atual.linhas.push(`<span class="reg-etapa">${esc(ETAPAS.find(([k]) => k === m[3])?.[1] ?? m[3])}</span>`);
        continue;
      }
      atual.linhas.push(esc(traduzir(l)));
    }
    blocos.push(atual);
    return `<div class="registro">${blocos.filter((b) => b.linhas.length).reverse().slice(0, 40).map((b) => `<details open><summary>${esc(b.titulo)}</summary>${b.linhas.map((x) => `<div>${x}</div>`).join('')}</details>`).join('')}</div>`;
  }
  function ajudaHTML() {
    return `<div class="ajuda-mesa">
      <p><b>Objetivo:</b> zerar a vida do Conjurador do oponente (♥ sobre a carta dele).</p>
      <p><b>Grau:</b> 1 vez por turno, ponha uma carta da mão no Grau (fica virada no seu campo). Uma carta de Grau X exige X cartas no seu Grau. Cada carta do Grau pode ser suspensa (deitada) para gerar 1 Arcano.</p>
      <p><b>Arcano:</b> a esfera na lateral mostra a reserva. Pague custos com a reserva, o Grau e enviando cartas da mão ao Arcano (o número no círculo de cima da carta). A sobra some no seu Cataclisma.</p>
      <p><b>Findante</b> só na sua fase Principal com a pilha vazia. <b>Repentina</b> sempre que você tiver a prioridade, até no turno do oponente.</p>
      <p><b>Pilha:</b> cartas, efeitos e ataques aparecem aqui na direita; cada jogador pode responder. Quando os dois passam, a de cima resolve.</p>
      <p><b>Combate:</b> atacar suspende. O defensor pode bloquear com um Aliado ou o Conjurador que esteja de pé. Aliados que acabaram de entrar (zZ) não atacam, salvo Proficiente.</p>
      <p><b>Etéreo:</b> só Aliados Etéreos o bloqueiam. <b>Protegido N:</b> mirar custa N a mais.</p>
      <p><b>Impasse:</b> 5 turnos seguidos sem agir e você perde.</p>
      <p class="peq">Passe o mouse numa carta para ver; clique ou arraste para agir.</p>
      <h3>Palavras-chave</h3>${palavrasChaveHTML(esc)}</div>`;
  }

  function fimHTML() {
    if (e.vencedor === null) return '';
    const venci = e.vencedor === EU;
    const motivo = fonte.motivo?.() ?? '';
    return `<div class="fim-partida"><div class="fim-caixa ${venci ? 'vitoria' : e.vencedor === 'empate' ? '' : 'derrota'}">
      <h2>${e.vencedor === 'empate' ? 'Empate' : venci ? 'Vitória!' : 'Derrota'}</h2>
      <p>${esc(traduzir(motivo || `${e.turno} turnos.`))}</p>
      <div class="linha-botoes">${fonte.revanche ? '<button class="botao primario" data-revanche>Jogar de novo</button>' : ''}${opinar ? '<button class="botao rosa" data-opinar>Contar o que achou</button>' : ''}<button class="botao" data-sair>Voltar</button><button class="botao fantasma" data-fechar-fim>Ver a mesa</button></div>
    </div></div>`;
  }

  function render() {
    if (fechada) return;
    const posicoesAnteriores = movimentosMesa.capturar();
    const anteriorGanchos = ganchos.antes(e);
    e = fonte.e;
    const total = e.logTotal ?? e.log.length;
    const nNovas = total - ui.logVisto;
    const novas = nNovas > 0 ? e.log.slice(-Math.min(nNovas, e.log.length)) : [];
    ui.logVisto = total;
    for (const l of novas) if (!/^Turno \d+ \(/.test(l)) acontecimentos.push(traduzir(l));
    if (acontecimentos.length > 30) acontecimentos.splice(0, acontecimentos.length - 30);
    if (e.pilha.some((p) => !ui.pilhaVista.has(p.id)) || (e.pendencia?.jogador === EU) || ui.modo) ui.aba = 'partida';
    conteudo.innerHTML = `${ladoHTML(OP)}<div class="linha-meio"></div>${ladoHTML(EU)}${ui.fimFechado ? '' : fimHTML()}`;
    ajustarMesa();
    faixaMao.innerHTML = maoHTML();
    faixaMaoOp.innerHTML = maoOpHTML();
    const rolagem = $('.lateral-corpo', lateral)?.scrollTop ?? 0;
    lateral.innerHTML = lateralHTML();
    if (!e.pilha.length) $('.lateral-corpo', lateral).scrollTop = rolagem;
    for (const p of e.pilha) ui.pilhaVista.add(p.id);
    if (ui.modo?.tipo === 'bloqueio' && !(e.pendencia?.tipo === 'bloqueio' && e.pendencia.jogador === EU)) ui.modo = null;
    if (e.pendencia?.tipo === 'bloqueio' && e.pendencia.jogador === EU && !ui.modo) {
      ui.modo = { tipo: 'bloqueio', ids: new Set([...z(EU).campo.filter((id) => def(id).tipo === 'aliado'), z(EU).conjuradorId].filter((id) => !inst(id).suspensa)) };
      render();
      return;
    }
    if (!e.pendencia || !['descartar', 'escolherDoTopo'].includes(e.pendencia.tipo)) ui.selecionadas.clear();
    atualizarPrazo();
    agendarAutoPasse();
    movimentosMesa.animar(posicoesAnteriores);
    ganchos.depois(e, anteriorGanchos);
    abertura();
  }

  let timerPrazo = null;
  function atualizarPrazo() {
    clearInterval(timerPrazo);
    const el = $('.fase-prazo', lateral);
    if (!el) return;
    const tic = () => { const s = Math.max(0, Math.round((Number(el.dataset.prazo) - Date.now()) / 1000)); el.textContent = `⏳ ${s}s`; el.classList.toggle('acabando', s <= 15); };
    tic();
    timerPrazo = setInterval(tic, 1000);
  }

  // ---------- passe automático ----------
  function deveAutoPassar() {
    if (!tenhoPrioridade() || ui.modo || ui.enviando) return false;
    if (fonte.tutorial?.semAutoPasse?.()) return false;
    const topo = e.pilha[e.pilha.length - 1];
    if (topo) {
      if (topo.controlador === EU) return true;
      if (cfg.esperarPilha) return false;
      return cfg.autoPassar && !possoResponder();
    }
    if (e.combate) return cfg.autoPassar && !possoResponder();
    if (e.ativo !== EU) return !(e.etapa === 'final' && cfg.pararFinalOponente && possoResponder());
    return e.etapa !== 'principal';
  }
  function agendarAutoPasse() {
    clearTimeout(timerAuto);
    if (!deveAutoPassar()) return;
    const v = fonte.versao;
    const topo = e.pilha[e.pilha.length - 1];
    // pausas um pouco mais longas (pedido do autor, 04/10): dá tempo de ver cada jogada e animação
    const espera = topo && topo.controlador !== EU ? 2300 : e.combate ? 1400 : 480;
    timerAuto = setTimeout(() => { if (!fechada && fonte.versao === v && deveAutoPassar()) enviar({ tipo: 'passar' }, { silencioso: true }); }, espera);
  }

  // ---------- eventos ----------
  // ---------- arrastar cartas da mão (pedido do autor, 03/10) ----------
  // Segurar a carta da mão e soltar no campo: abre o pagamento (e o alvo, se houver); sem poder pagar ou cancelando,
  // ela volta para a mão. Soltar na zona de Grau põe a carta no Grau. Clique sem arrastar continua abrindo o menu.
  let arrasto = null;
  let ignorarClique = false;
  const reduzido = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
  function voar(el, para, { duracao = RITMO_VISUAL.voltarParaMao, depois } = {}) {
    const de = el.getBoundingClientRect();
    if (!para || reduzido()) { el.remove(); depois?.(); return; }
    const dx = para.left + para.width / 2 - (de.left + de.width / 2), dy = para.top + para.height / 2 - (de.top + de.height / 2);
    const k = para.width / de.width;
    const a = el.animate([{ transform: el.style.transform }, { transform: `${el.style.transform} translate(${dx}px, ${dy}px) rotate(0deg) scale(${k})` }], { duration: duracao, easing: 'cubic-bezier(.2,.75,.25,1)', fill: 'forwards' });
    a.finished.then(() => { el.remove(); depois?.(); }, () => { el.remove(); depois?.(); });
  }
  const sobre = (x, y, sel) => document.elementsFromPoint(x, y).some((n) => n.closest?.(sel));
  function marcarAlvos(id, ligar) {
    const pode = ligar && podeJogar(id);
    palco.classList.toggle('alvo-jogar', !!pode);
    palco.classList.toggle('alvo-negado', !!ligar && !pode);
    $('.lado-eu .zona-grau', raiz)?.classList.toggle('alvo-grau', !!ligar && podeGrau());
  }
  faixaMao.addEventListener('pointerdown', (ev) => {
    const carta = ev.target.closest('.mao-slot .cm[data-id]');
    if (!carta || ev.button !== 0 || ui.modo || e.vencedor !== null) return;
    arrasto = { id: carta.dataset.id, carta, x0: ev.clientX, y0: ev.clientY, ativo: false, ultimoX: ev.clientX, inclinacao: 0 };
  });
  window.addEventListener('pointermove', moverArrasto);
  function moverArrasto(ev) {
    const a = arrasto;
    if (!a) return;
    if (!a.ativo) {
      if (Math.hypot(ev.clientX - a.x0, ev.clientY - a.y0) < 8) return;
      a.ativo = true;
      zoom.classList.remove('visivel');
      const r = a.carta.getBoundingClientRect();
      a.dx = a.x0 - r.left; a.dy = a.y0 - r.top;
      a.fantasma = a.carta.cloneNode(true);
      a.fantasma.classList.add('carta-arrastada');
      Object.assign(a.fantasma.style, { left: r.left + 'px', top: r.top + 'px', width: r.width + 'px' });
      document.body.append(a.fantasma);
      a.carta.closest('.mao-slot')?.classList.add('arrastando');
      marcarAlvos(a.id, true);
    }
    // a carta "maleável": inclina para o lado do movimento e levanta um pouco
    a.inclinacao = Math.max(-16, Math.min(16, a.inclinacao * 0.7 + (ev.clientX - a.ultimoX) * 0.9));
    a.ultimoX = ev.clientX;
    const subiu = Math.min(1, Math.max(0, (a.y0 - ev.clientY) / 160));
    a.fantasma.style.transform = `translate(${ev.clientX - a.dx - parseFloat(a.fantasma.style.left)}px, ${ev.clientY - a.dy - parseFloat(a.fantasma.style.top)}px) perspective(700px) rotateX(${8 * subiu}deg) rotate(${a.inclinacao}deg) scale(${1.08 + 0.1 * subiu})`;
    a.fantasma.classList.toggle('sobre-campo', sobre(ev.clientX, ev.clientY, '.mesa-palco') && !sobre(ev.clientX, ev.clientY, '.mao-faixa'));
  }
  async function soltar(ev) {
    const a = arrasto;
    arrasto = null;
    if (!a?.ativo) return;
    ignorarClique = true;
    setTimeout(() => { ignorarClique = false; }, 0);
    marcarAlvos(a.id, false);
    const f = a.fantasma;
    f.style.pointerEvents = 'none';
    const voltar = () => {
      const slot = $(`.mao-slot .cm[data-id="${a.id}"]`, faixaMao);
      voar(f, slot?.getBoundingClientRect(), { depois: () => slot?.closest('.mao-slot')?.classList.remove('arrastando') });
    };
    const x = ev.clientX, y = ev.clientY;
    if (ev.type === 'pointerup' && sobre(x, y, '.lado-eu .zona-grau') && podeGrau()) {
      await enviar({ tipo: 'colocarGrau', carta: a.id });
      const g = $(`.zona-grau .cm[data-id="${a.id}"]`, raiz);
      return g ? voar(f, g.getBoundingClientRect()) : voltar();
    }
    if (ev.type !== 'pointerup' || !sobre(x, y, '.mesa-palco') || sobre(x, y, '.mao-faixa')) return voltar();
    if (!podeJogar(a.id)) {
      const d = def(a.id);
      avisar(!tenhoPrioridade() ? 'Agora não é a sua vez de jogar.' : d.velocidade !== 'repentina' && !minhaJanela() ? 'Esta carta só entra na sua fase principal (sem nada na pilha).' : d.grau > grauEfetivo(e, EU) ? `Precisa de Grau ${d.grau}.` : 'Arcano insuficiente para os custos.', 'erro', 3500);
      return voltar();
    }
    f.classList.add('aguardando');
    await preparar('jogar', a.id);
    if (z(EU).mao.includes(a.id)) return voltar(); // cancelou ou não pagou: volta para a mão
    // jogou: a carta vai do ponto onde foi solta até o lugar dela (campo, ou a pilha se for Temporária)
    const destino = $(`.mesa-conteudo .cm[data-id="${a.id}"]`, raiz) ?? $(`.mesa-direita .cm[data-id="${a.id}"]`, raiz);
    if (destino) {
      destino.style.visibility = 'hidden';
      voar(f, destino.getBoundingClientRect(), { duracao: RITMO_VISUAL.arrasteParaCampo, depois: () => { destino.style.visibility = ''; } });
    } else f.animate([{ opacity: 1 }, { opacity: 0 }], { duration: RITMO_VISUAL.desaparecer }).finished.then(() => f.remove());
  }
  window.addEventListener('pointerup', soltar);
  window.addEventListener('pointercancel', soltar);
  faixaMao.addEventListener('dragstart', (ev) => ev.preventDefault());

  raiz.addEventListener('click', async (ev) => {
    if (ignorarClique) { ev.stopPropagation(); return; }
    const alvo = ev.target;
    if (ev.target.closest('.mesa-menu-acao')) return;
    if (ui.menu && !alvo.closest('.cm')) fecharMenu();
    const b = alvo.closest('button, .cm, [data-topo]');
    if (!b) return;
    if (b.matches('[data-passar]')) { fecharMenu(); return enviar({ tipo: 'passar' }); }
    if (b.matches('[data-cancelar-alvo]')) return terminarModo(null);
    if (b.matches('[data-lista-alvos]')) {
      const ids = [...ui.modo.ids];
      const r = await escolherCartas(ui.modo.titulo, ids, 1);
      if (r?.length) terminarModo(r[0]);
      return;
    }
    if (b.matches('[data-aba]')) { ui.aba = b.dataset.aba; lateral.innerHTML = lateralHTML(); return; }
    if (b.matches('[data-opinar]')) return opinar?.(contexto());
    if (b.matches('[data-config]')) return configurar();
    if (b.matches('[data-tela-cheia]')) {
      // a mesa ocupa a janela inteira mesmo onde o navegador não deixa a tela cheia de verdade (ex.: dentro de outro app)
      const ligar = !document.body.classList.contains('tela-cheia');
      document.body.classList.toggle('tela-cheia', ligar);
      lateral.innerHTML = lateralHTML();
      try {
        if (ligar && !document.fullscreenElement) await document.documentElement.requestFullscreen({ navigationUI: 'hide' });
        if (!ligar && document.fullscreenElement) await document.exitFullscreen();
      } catch { /* sem tela cheia do navegador: fica só a mesa maximizada */ }
      return;
    }
    if (b.matches('[data-desistir]')) { if (confirm('Desistir desta partida?')) fonte.desistir(); return; }
    if (b.matches('[data-sair]')) return aoSair?.();
    if (b.matches('[data-revanche]')) return fonte.revanche();
    if (b.matches('[data-fechar-fim]')) { ui.fimFechado = true; render(); return; }
    if (b.matches('[data-zona]')) return verZona(b.dataset.zona, Number(b.dataset.j));
    if (b.matches('.item-conj')) { const id = b.dataset.id; if (inst(id)?.dono === EU && podeAtivar(id)) return preparar('ativar', id); return verCarta(id); }
    if (b.matches('[data-topo]')) {
      const id = b.dataset.topo; const p = e.pendencia;
      if (ui.selecionadas.has(id)) ui.selecionadas.delete(id); else if (ui.selecionadas.size < p.pegar) ui.selecionadas.add(id);
      render(); return;
    }
    if (b.matches('[data-pend]')) {
      const k = b.dataset.pend;
      if (k === 'comecar-eu') return enviar({ tipo: 'escolherInicio', primeiro: EU });
      if (k === 'comecar-op') return enviar({ tipo: 'escolherInicio', primeiro: OP });
      if (k === 'naoBloquear') { ui.modo = null; return enviar({ tipo: 'bloquear', bloqueador: null }); }
      if (k === 'descartar') return enviar({ tipo: 'descartar', cartas: [...ui.selecionadas] });
      if (k === 'topo') return enviar({ tipo: 'escolherDoTopo', cartas: [...ui.selecionadas] });
      if (k === 'naoPagar') return enviar({ tipo: 'pagarAnulacao', pagar: false });
      if (k === 'pagarAnul') {
        const p = e.pendencia;
        const pg = await pagar(`Pagar ${p.custo} para anular`, p.custo, undefined, undefined, { soPitch: true });
        if (!pg) return;
        return enviar({ tipo: 'pagarAnulacao', pagar: true, enviarArcano: pg.pitch });
      }
    }
    if (b.matches('.cm')) {
      const id = b.dataset.id;
      if (ui.modo?.tipo === 'alvo') { if (ui.modo.ids.has(id)) terminarModo(id); return; }
      if (ui.modo?.tipo === 'bloqueio') {
        if (!ui.modo.ids.has(id)) return verCarta(id);
        let usarArmadura;
        if (id === z(EU).conjuradorId) { usarArmadura = await perguntarArmadura(); if (usarArmadura === null) return; }
        ui.modo = null;
        return enviar({ tipo: 'bloquear', bloqueador: id, ...(usarArmadura === undefined ? {} : { usarArmadura }) });
      }
      if (e.pendencia?.tipo === 'descartar' && e.pendencia.jogador === EU && z(EU).mao.includes(id)) {
        if (ui.selecionadas.has(id)) ui.selecionadas.delete(id); else if (ui.selecionadas.size < e.pendencia.quantidade) ui.selecionadas.add(id);
        render(); return;
      }
      if (inst(id)?.defId === 'oculta') return;
      if ((inst(id)?.dono === EU || ehConj(id)) && acoesDe(id).length > 1) return abrirMenu(id, b);
      return verCarta(id);
    }
  });
  raiz.addEventListener('submit', (ev) => {
    if (!ev.target.matches('.chat-form')) return;
    ev.preventDefault();
    const i = $('input', ev.target);
    if (i.value.trim()) fonte.chat(i.value.trim());
    i.value = '';
  });
  // pré-visualização ao passar o mouse
  raiz.addEventListener('mouseover', (ev) => {
    const c = ev.target.closest('.cm');
    if (!c || matchMedia('(hover: none)').matches) return;
    const i = inst(c.dataset.id);
    if (!i || i.defId === 'oculta' || (i.dono !== EU && z(i.dono).grau.includes(c.dataset.id) && !i.revelada)) { zoom.classList.remove('visivel'); return; }
    const m = i.ehConjurador ? modeloConjurador(i.defId) : modeloCarta(i.defId);
    zoom.innerHTML = desenharCarta(m, { width: 250 }).outerHTML + (i.dano ? `<div class="zoom-dano">Dano: ${i.dano}</div>` : '');
    const r = c.getBoundingClientRect();
    zoom.style.left = (r.left > innerWidth / 2 ? Math.max(8, r.left - 266) : Math.min(innerWidth - 262, r.right + 12)) + 'px';
    zoom.style.top = Math.max(70, Math.min(innerHeight - 370, r.top - 60)) + 'px';
    zoom.classList.add('visivel');
  });
  raiz.addEventListener('mouseout', (ev) => { if (ev.target.closest('.cm') && !ev.relatedTarget?.closest?.('.cm')) zoom.classList.remove('visivel'); });

  function verZona(zona, j) {
    const ids = z(j)[zona];
    if (zona === 'baralho') { avisar(`${j === EU ? 'Seu baralho' : 'Baralho do oponente'}: ${ids.length} cartas.`); return; }
    const rot = { cemiterio: 'Cemitério', banimento: 'Banimento', zonaArcano: 'Zona de Contador Arcano' }[zona];
    const jogaveis = j === EU ? ids.filter((id) => podeJogar(id)) : [];
    const jj = janela(`<h3>${rot} ${j === EU ? '(seu)' : `de ${esc(nome(j))}`} · ${ids.length}</h3><div class="vitrine">${ids.map((id) => `<div class="item-vitrine" data-ver="${id}">${desenharCarta(modeloCarta(inst(id).defId), { width: 120 }).outerHTML}${jogaveis.includes(id) ? `<button class="botao peq primario jogar-zona" data-jogar="${id}">Jogar</button>` : ''}</div>`).join('') || '<p class="vazio">Vazio.</p>'}</div>`, { classe: 'larga' });
    jj.el.addEventListener('click', (ev) => {
      const jb = ev.target.closest('[data-jogar]');
      if (jb) { jj.fechar(); preparar('jogar', jb.dataset.jogar); return; }
      const v = ev.target.closest('[data-ver]');
      if (v) verCarta(v.dataset.ver);
    });
  }

  function configurar() {
    const j = janela(`<h3>Opções da mesa</h3>
      <label class="opcao-linha"><input type="checkbox" name="autoPassar" ${cfg.autoPassar ? 'checked' : ''}> Passar sozinho quando eu não tiver resposta</label>
      <label class="opcao-linha"><input type="checkbox" name="esperarPilha" ${cfg.esperarPilha ? 'checked' : ''}> Sempre esperar meu clique quando o oponente puser algo na pilha</label>
      <label class="opcao-linha"><input type="checkbox" name="pararFinalOponente" ${cfg.pararFinalOponente ? 'checked' : ''}> Parar no Final do oponente quando eu puder responder</label>
      <div class="linha-botoes"><button class="botao primario" data-ok>Salvar</button></div>`);
    j.el.addEventListener('click', (ev) => {
      if (!ev.target.closest('[data-ok]')) return;
      for (const c of $$('input[type=checkbox]', j.el)) cfg[c.name] = c.checked;
      local.gravar('cfg-mesa', cfg);
      j.fechar();
      render();
    });
  }

  function contexto() {
    return {
      modo: fonte.modo, turno: e.turno, etapa: e.etapa, eu: conjDef(EU).id, oponente: conjDef(OP).id,
      vida: [conjDef(EU).vida - inst(z(EU).conjuradorId).dano, conjDef(OP).vida - inst(z(OP).conjuradorId).dano],
      vencedor: e.vencedor === null ? null : e.vencedor === EU ? 'eu' : 'oponente', registro: e.log.slice(-40).map(traduzir), versao: DADOS.versao,
    };
  }

  fonte.ouvir(() => render());
  render();
  return {
    render,
    fechar() { fechada = true; for (const d of dadosAbertura) try { d.destruir(); } catch { /* nada */ } globalThis.ArcanoclismaAbertura?.limparEntradaArena(raiz); clearTimeout(timerAuto); clearInterval(timerPrazo); movimentosMesa.limpar(); ganchos.limpar(); document.removeEventListener('fullscreenchange', aoTelaCheia); if (document.fullscreenElement) document.exitFullscreen().catch(() => {}); document.body.classList.remove('tela-cheia'); window.removeEventListener('pointermove', moverArrasto); window.removeEventListener('pointerup', soltar); window.removeEventListener('pointercancel', soltar); arrasto?.fantasma?.remove(); raiz.innerHTML = ''; },
    contexto,
  };
}
