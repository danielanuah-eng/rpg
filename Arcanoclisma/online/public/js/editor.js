// Editor do autor no site: muda cartas e Conjuradores (nome, texto, arte, números, Valores, velocidade, palavras-chave
// e a mecânica em JSON). As mudanças vão para um rascunho que já vale nesta página (dá para testar contra o bot);
// "Salvar no site" grava uma versão nova no D1 e ela passa a valer para todos (partidas online já começadas
// continuam com a versão do início). Mesmo formato do editor do app de testes (tcg/data/edicoes.json).
import { $, $$, esc, api, DADOS, BASE, EDICOES, VALOR, TIPO, textoCarta, sessao, avisar, janela, local, usarEdicoes, carregarEdicoes, dataCurta } from './base.js';
import { desenharCarta, modeloCarta, modeloConjurador } from './carta.js';
import { edicoesVazias } from '/jogo/edicoes.js';

const VALORES = Object.keys(VALOR);
const COMPROMETIMENTOS = ['Invocador', 'Brigão', 'Parrudão da quebrada', 'Habilidoso', 'Preparado'];
const NUM_CARTA = [['custo', 'Custo'], ['grau', 'Grau'], ['arcanoGerado', 'Gera Arcano'], ['ataque', 'Ataque'], ['vida', 'Vida'], ['defesa', 'Defesa (armadura)'], ['vidaDesgaste', 'Desgaste (armadura)'], ['protegido', 'Protegido'], ['descartaGrau', 'Descarta do Grau'], ['pagarParaAnular', 'Pagar para anular'], ['grauExtra', 'Grau extra']];
const NUM_CONJ = [['vida', 'Vida'], ['maoInicial', 'Mão inicial'], ['compraFinal', 'Compra no Final'], ['danoBasico', 'Dano básico'], ['custoAtaque', 'Custo de ataque']];
/** Campos de mecânica editados como JSON. */
const MEC_CARTA = ['efeitos', 'aoEntrar', 'aoDanoConjurador', 'ativacao', 'custoValores'];
const igual = (a, b) => JSON.stringify(a ?? null) === JSON.stringify(b ?? null);
const CHAVE_RASCUNHO = 'editor:rascunho';

let ARTES = null;
const artes = async () => (ARTES ??= await (await fetch('/jogo/artes.json')).json());

export async function telaEditor(tela, partes, ctx) {
  const u = await ctx.exigirConta('Entre com a conta do autor para editar as cartas.');
  if (!u) return ctx.navegar('inicio');
  if (!u.admin) { tela.innerHTML = '<div class="painel vazio">Só o autor edita as cartas. Entre com o e-mail do autor (Google ou "Sou o autor").</div>'; return; }
  // rascunho: o que foi salvo neste navegador, ou a versão em vigor
  let rascunho = local.ler(CHAVE_RASCUNHO, null) ?? structuredClone(EDICOES.dados);
  rascunho = { ...edicoesVazias(), ...rascunho, artes: { cartas: {}, conjuradores: {}, ...(rascunho.artes ?? {}) } };
  let sel = partes[0] ? decodeURIComponent(partes[0]) : null; // "c:<id>" (carta) ou "j:<id>" (Conjurador)
  let filtro = { busca: '', tipo: 'conjurador' };

  const aplicarRascunho = () => {
    usarEdicoes({ id: EDICOES.id, dados: rascunho, rascunho: difereDoSalvo() });
    local.gravar(CHAVE_RASCUNHO, rascunho);
  };
  const mudancas = () => new Set([...Object.keys(rascunho.cartas).map((x) => 'c:' + x), ...Object.keys(rascunho.artes.cartas).map((x) => 'c:' + x), ...Object.keys(rascunho.conjuradores).map((x) => 'j:' + x), ...Object.keys(rascunho.artes.conjuradores).map((x) => 'j:' + x)]);
  const difereDoSalvo = () => !igual(rascunho, EDICOES.salvo ?? EDICOES.dados);
  EDICOES.salvo ??= structuredClone(EDICOES.dados);
  aplicarRascunho();

  tela.classList.add('cheia');
  tela.innerHTML = `<div class="editor">
    <aside class="editor-lista painel">
      <div class="titulo-secao"><h2>Editor</h2></div>
      <input type="search" placeholder="Buscar" data-busca>
      <div class="abas editor-tipos">${[['conjurador', 'Conjuradores'], ['aliado', 'Aliados'], ['temporaria', 'Temporárias'], ['mistica', 'Místicas'], ['estrutura', 'Estruturas'], ['item', 'Itens'], ['mudadas', 'Mudadas']].map(([k, r]) => `<button data-tipo="${k}">${r}</button>`).join('')}</div>
      <div class="editor-itens"></div>
    </aside>
    <section class="editor-form painel"></section>
    <aside class="editor-lado painel">
      <div class="editor-preview"></div>
      <div class="editor-estado"></div>
      <div class="linha-botoes">
        <button class="botao primario" data-salvar>Salvar no site</button>
        <button class="botao" data-baixar>Baixar edições (JSON)</button>
        <label class="botao">Carregar JSON<input type="file" accept="application/json,.json" data-carregar hidden></label>
        <button class="botao perigo" data-descartar>Descartar rascunho</button>
      </div>
      <h3>Versões salvas</h3><div class="editor-historico peq">…</div>
    </aside></div>`;
  const lista = $('.editor-itens', tela), form = $('.editor-form', tela), preview = $('.editor-preview', tela), estado = $('.editor-estado', tela);

  function desenharEstado() {
    const n = mudancas().size;
    estado.innerHTML = `<p class="peq">Versão em vigor no site: <b>${EDICOES.id || 'nenhuma edição'}</b>. Rascunho: <b>${n}</b> carta(s)/Conjurador(es) mudados${difereDoSalvo() ? ' · <span class="falta">não salvo</span>' : ' · salvo'}.<br>O rascunho já vale nesta página (teste contra o bot); os outros jogadores só veem depois de salvar.</p>`;
  }
  function desenharLista() {
    for (const b of $$('[data-tipo]', tela)) b.classList.toggle('ativo', b.dataset.tipo === filtro.tipo);
    const mud = mudancas();
    const busca = filtro.busca.toLowerCase();
    const itens = [
      ...Object.values(DADOS.catalogo.conjuradores).map((c) => ({ k: 'j:' + c.id, nome: c.nome, tipo: 'conjurador', sub: c.valores.map((v) => VALOR[v]).join(' + ') })),
      ...Object.values(DADOS.catalogo.cartas).filter((c) => c.tipo !== 'oculta').map((c) => ({ k: 'c:' + c.id, nome: c.nome, tipo: c.tipo, sub: `${TIPO[c.categoriaItem ?? c.tipo] ?? c.tipo} · custo ${c.custo} · Grau ${c.grau}` })),
    ].filter((x) => (filtro.tipo === 'mudadas' ? mud.has(x.k) : x.tipo === filtro.tipo) && (!busca || x.nome.toLowerCase().includes(busca) || x.k.includes(busca)));
    lista.innerHTML = itens.map((x) => `<button class="editor-item ${x.k === sel ? 'ativo' : ''}" data-k="${esc(x.k)}">${mud.has(x.k) ? '<span class="ponto-mudou" title="Mudada no rascunho">●</span> ' : ''}<b>${esc(x.nome)}</b><span class="peq">${esc(x.sub)}</span></button>`).join('') || '<p class="peq">Nada encontrado.</p>';
  }

  // ---------- formulário ----------
  const numCampo = (k, rot, v) => `<label class="campo campo-num">${rot}<input type="number" step="1" data-num="${k}" value="${v ?? ''}" placeholder="—"></label>`;
  function desenharForm() {
    if (!sel) { form.innerHTML = '<p class="vazio">Escolha uma carta ou um Conjurador na lista.</p>'; preview.innerHTML = ''; return; }
    const [t, id] = [sel.slice(0, 1), sel.slice(2)];
    const conjur = t === 'j';
    const c = conjur ? DADOS.catalogo.conjuradores[id] : DADOS.catalogo.cartas[id];
    if (!c) { form.innerHTML = '<p class="vazio">Não encontrada.</p>'; return; }
    const arteAtual = conjur ? DADOS.conjuradoresArte[id] : DADOS.visual[id]?.arte;
    const mec = conjur ? { poder: c.poder ?? null } : Object.fromEntries(MEC_CARTA.filter((k) => c[k] !== undefined).map((k) => [k, c[k]]));
    form.innerHTML = `<div class="titulo-secao"><h2>${esc(c.nome)}</h2><span class="etiqueta">${conjur ? 'Conjurador' : esc(TIPO[c.categoriaItem ?? c.tipo] ?? c.tipo)} · ${esc(id)}</span></div>
      <label class="campo">Nome<input type="text" data-f="nome" value="${esc(c.nome)}"></label>
      <label class="campo">${conjur ? 'Habilidade (texto)' : 'Texto de regras'}<textarea data-f="${conjur ? 'habilidade' : 'texto'}">${esc(conjur ? c.habilidade : c.texto)}</textarea></label>
      ${conjur ? '' : '<div class="linha-botoes"><button class="botao peq" data-texto-auto>Gerar o texto a partir da mecânica</button></div>'}
      <div class="campo"><span>Arte</span><div class="editor-arte"><img src="/arte/${esc(arteAtual)}.webp" alt=""><div><b>${esc(arteAtual ?? 'sem arte')}</b><div class="linha-botoes"><button class="botao peq" data-trocar-arte>Trocar arte</button></div></div></div></div>
      <div class="editor-nums">${(conjur ? NUM_CONJ : NUM_CARTA).map(([k, r]) => numCampo(k, r, c[k])).join('')}</div>
      <div class="campo"><span>Valores</span><div class="editor-valores">${VALORES.map((v) => `<label class="opcao-linha"><input type="checkbox" data-valor="${v}" ${c.valores.includes(v) ? 'checked' : ''}> ${esc(VALOR[v])}</label>`).join('')}</div></div>
      ${conjur
        ? `<label class="campo">Comprometimento<select data-f="comprometimento">${COMPROMETIMENTOS.map((x) => `<option ${x === c.comprometimento ? 'selected' : ''}>${esc(x)}</option>`).join('')}</select></label>`
        : `<label class="campo">Velocidade<select data-f="velocidade"><option value="findante" ${c.velocidade === 'findante' ? 'selected' : ''}>Findante (na sua fase principal)</option><option value="repentina" ${c.velocidade === 'repentina' ? 'selected' : ''}>Repentina (a qualquer momento)</option></select></label>
           <label class="campo">Palavras-chave (separadas por vírgula)<input type="text" data-f="palavrasChave" value="${esc((c.palavrasChave ?? []).join(', '))}" placeholder="Proficiente, Etéreo"></label>
           <label class="campo">Subtipos (separados por vírgula)<input type="text" data-f="subtipos" value="${esc((c.subtipos ?? []).join(', '))}"></label>`}
      <label class="campo">Mecânica em JSON (${conjur ? 'poder' : MEC_CARTA.join(', ')})<textarea class="editor-json" data-json spellcheck="false">${esc(JSON.stringify(mec, null, 2))}</textarea></label>
      <p class="peq editor-json-erro" data-json-erro></p>
      <div class="linha-botoes"><button class="botao perigo peq" data-desfazer>Desfazer as mudanças desta ${conjur ? 'Conjurador' : 'carta'}</button></div>`;
    desenharPreview();
  }
  function desenharPreview() {
    const [t, id] = [sel.slice(0, 1), sel.slice(2)];
    const m = t === 'j' ? modeloConjurador(id) : modeloCarta(id);
    preview.innerHTML = m ? desenharCarta(m, { width: 260 }).outerHTML : '';
  }

  /** Lê o formulário e grava no rascunho só o que difere dos dados gerados (null apaga um campo). */
  function lerForm() {
    const [t, id] = [sel.slice(0, 1), sel.slice(2)];
    const conjur = t === 'j';
    const base = conjur ? BASE.catalogo.conjuradores[id] : BASE.catalogo.cartas[id];
    const novo = {};
    for (const el of $$('[data-f]', form)) {
      const k = el.dataset.f;
      novo[k] = ['palavrasChave', 'subtipos'].includes(k) ? el.value.split(',').map((x) => x.trim()).filter(Boolean) : el.value;
    }
    if (!conjur && !novo.palavrasChave.length) novo.palavrasChave = undefined;
    for (const el of $$('[data-num]', form)) novo[el.dataset.num] = el.value === '' ? undefined : Math.trunc(Number(el.value));
    novo.valores = $$('[data-valor]', form).filter((x) => x.checked).map((x) => x.dataset.valor);
    const erroJson = $('[data-json-erro]', form);
    try {
      const mec = JSON.parse($('[data-json]', form).value || '{}');
      if (typeof mec !== 'object' || Array.isArray(mec) || mec === null) throw new Error('use um objeto { }');
      for (const k of conjur ? ['poder'] : MEC_CARTA) novo[k] = mec[k] ?? undefined;
      erroJson.textContent = '';
    } catch (err) { erroJson.textContent = 'JSON inválido (a mecânica não foi aplicada): ' + err.message; for (const k of conjur ? ['poder'] : MEC_CARTA) delete novo[k]; }
    const parcial = {};
    for (const [k, v] of Object.entries(novo)) {
      if (!(k in novo) || igual(v, base[k])) continue;
      if (v === undefined) { if (base[k] !== undefined) parcial[k] = null; } else parcial[k] = v;
    }
    // texto: se só mudou a mecânica, o texto continua automático (gerado na hora); se o autor escreveu, vale o dele
    const grupo = conjur ? rascunho.conjuradores : rascunho.cartas;
    if (Object.keys(parcial).length) grupo[id] = parcial; else delete grupo[id];
    aplicarRascunho();
    desenharPreview();
    desenharLista();
    desenharEstado();
  }
  let espera = null;
  form.addEventListener('input', () => { clearTimeout(espera); espera = setTimeout(lerForm, 350); });
  form.addEventListener('change', () => { clearTimeout(espera); lerForm(); });
  form.addEventListener('click', async (ev) => {
    const [t, id] = sel ? [sel.slice(0, 1), sel.slice(2)] : [];
    if (ev.target.closest('[data-desfazer]')) {
      delete (t === 'j' ? rascunho.conjuradores : rascunho.cartas)[id];
      delete (t === 'j' ? rascunho.artes.conjuradores : rascunho.artes.cartas)[id];
      aplicarRascunho(); desenharTudo(); return;
    }
    if (ev.target.closest('[data-texto-auto]')) {
      const c = DADOS.catalogo.cartas[id];
      $('[data-f="texto"]', form).value = textoCarta({ ...c, texto: '' });
      lerForm(); return;
    }
    if (ev.target.closest('[data-trocar-arte]')) {
      const a = await escolherArte();
      if (!a) return;
      const grupo = t === 'j' ? rascunho.artes.conjuradores : rascunho.artes.cartas;
      const original = t === 'j' ? BASE.conjuradoresArte[id] : BASE.visual[id]?.arte;
      if (a === original) delete grupo[id]; else grupo[id] = a;
      aplicarRascunho(); desenharTudo();
    }
  });

  function escolherArte() {
    return new Promise(async (ok) => {
      const todas = await artes();
      const usadas = new Map(Object.entries(DADOS.visual).filter(([, v]) => v.arte).map(([k, v]) => [v.arte, DADOS.catalogo.cartas[k]?.nome ?? k]));
      for (const [k, a] of Object.entries(DADOS.conjuradoresArte)) usadas.set(a, DADOS.catalogo.conjuradores[k]?.nome ?? k);
      const j = janela(`<h3>Escolher arte</h3><input type="search" placeholder="Buscar pelo título" data-busca-arte>
        <div class="editor-galeria"></div>`, { classe: 'larga', aoFechar: () => ok(null) });
      const gal = $('.editor-galeria', j.el);
      const desenhar = (q) => {
        const b = q.toLowerCase();
        gal.innerHTML = todas.filter((a) => !b || a.titulo.toLowerCase().includes(b) || a.id.includes(b)).map((a) => `<button class="editor-arte-op" data-arte="${esc(a.id)}" title="${esc(a.titulo)}"><img loading="lazy" src="/arte/${esc(a.id)}.webp" alt=""><span>${esc(a.titulo)}</span>${usadas.has(a.id) ? `<small>em: ${esc(usadas.get(a.id))}</small>` : ''}</button>`).join('');
      };
      desenhar('');
      $('[data-busca-arte]', j.el).oninput = (ev) => desenhar(ev.target.value);
      gal.onclick = (ev) => { const b = ev.target.closest('[data-arte]'); if (!b) return; j.el.closest('.janela-fundo').remove(); ok(b.dataset.arte); };
    });
  }

  // ---------- salvar, baixar, histórico ----------
  async function desenharHistorico() {
    const h = $('.editor-historico', tela);
    try {
      const { versoes } = await api('/edicoes/historico');
      h.innerHTML = versoes.length ? versoes.map((v) => `<div class="editor-versao"><b>#${v.id}</b> · ${dataCurta(v.criado)} · ${v.mudancas} mudança(s)${v.id === EDICOES.id ? ' · <span class="etiqueta">em vigor</span>' : ''}<br>${esc(v.nota || 'sem nota')}<div class="linha-botoes"><button class="botao peq" data-versao="${v.id}">Abrir como rascunho</button></div></div>`).join('') : 'Nenhuma versão salva ainda.';
    } catch (err) { h.textContent = 'Não foi possível ler as versões: ' + err.message; }
  }
  $('.editor-lado', tela).addEventListener('click', async (ev) => {
    if (ev.target.closest('[data-salvar]')) {
      const j = janela(`<h3>Salvar no site</h3><p class="peq">A versão nova vale para todos a partir de agora (partidas online já começadas continuam com a versão do início).</p>
        <label class="campo">Nota (o porquê da mudança)<textarea data-nota maxlength="500"></textarea></label><div class="linha-botoes"><button class="botao primario" data-ok>Salvar</button></div>`);
      $('[data-ok]', j.el).onclick = async () => {
        try {
          const r = await api('/edicoes', { corpo: { dados: rascunho, nota: $('[data-nota]', j.el).value } });
          j.fechar();
          await carregarEdicoes();
          EDICOES.salvo = structuredClone(EDICOES.dados);
          rascunho = structuredClone(EDICOES.dados);
          aplicarRascunho();
          avisar(`Versão #${r.id} salva (${r.mudancas} mudança(s)).`, 'ok', 5000);
          if (r.avisos?.length) janela(`<h3>Atenção: baralhos prontos afetados</h3><p class="peq">As edições foram salvas, mas estes baralhos prontos deixaram de valer e não abrem partida até serem ajustados:</p><ul>${r.avisos.map((a) => `<li>${esc(a)}</li>`).join('')}</ul>`);
          desenharTudo(); desenharHistorico();
        } catch (err) { avisar(err.message, 'erro', 6000); }
      };
      return;
    }
    if (ev.target.closest('[data-baixar]')) {
      const a = document.createElement('a');
      a.href = URL.createObjectURL(new Blob([JSON.stringify(rascunho, null, 2)], { type: 'application/json' }));
      a.download = `arcanoclisma-edicoes-${new Date().toISOString().slice(0, 10)}.json`;
      a.click();
      setTimeout(() => URL.revokeObjectURL(a.href), 2000);
      return;
    }
    if (ev.target.closest('[data-descartar]')) {
      rascunho = structuredClone(EDICOES.salvo);
      aplicarRascunho(); desenharTudo(); avisar('Rascunho descartado: voltou à versão salva.', 'info');
      return;
    }
    const v = ev.target.closest('[data-versao]');
    if (v) {
      try {
        const r = await api('/edicoes?id=' + v.dataset.versao);
        rascunho = { ...edicoesVazias(), ...r.dados };
        aplicarRascunho(); desenharTudo(); avisar(`Versão #${r.id} aberta como rascunho. Salve para ela voltar a valer.`, 'info', 5000);
      } catch (err) { avisar(err.message, 'erro'); }
    }
  });
  $('[data-carregar]', tela).onchange = async (ev) => {
    const f = ev.target.files?.[0];
    if (!f) return;
    try {
      const d = JSON.parse(await f.text());
      rascunho = { ...edicoesVazias(), cartas: d.cartas ?? {}, conjuradores: d.conjuradores ?? {}, artes: { cartas: {}, conjuradores: {}, ...(d.artes ?? {}) } };
      aplicarRascunho(); desenharTudo(); avisar('Edições carregadas no rascunho.', 'ok');
    } catch (err) { avisar('Arquivo inválido: ' + err.message, 'erro'); }
    ev.target.value = '';
  };

  // ---------- lista ----------
  $('[data-busca]', tela).oninput = (ev) => { filtro.busca = ev.target.value; desenharLista(); };
  $('.editor-lista', tela).addEventListener('click', (ev) => {
    const tp = ev.target.closest('[data-tipo]');
    if (tp) { filtro.tipo = tp.dataset.tipo; desenharLista(); return; }
    const it = ev.target.closest('[data-k]');
    if (it) { sel = it.dataset.k; history.replaceState(null, '', '#/editor/' + encodeURIComponent(sel)); desenharLista(); desenharForm(); }
  });
  function desenharTudo() { desenharLista(); desenharForm(); desenharEstado(); }
  if (sel) filtro.tipo = sel.startsWith('j:') ? 'conjurador' : DADOS.catalogo.cartas[sel.slice(2)]?.tipo ?? 'aliado';
  desenharTudo();
  desenharHistorico();
  ctx.definirLimpeza(() => tela.classList.remove('cheia'));
}
