// Editor do autor no site: muda cartas e Conjuradores (nome, texto, arte, números, Valores, velocidade, palavras-chave
// e a mecânica em JSON). As mudanças vão para um rascunho que já vale nesta página (dá para testar contra o bot);
// "Salvar no site" grava uma versão nova no D1 e ela passa a valer para todos (partidas online já começadas
// continuam com a versão do início). Mesmo formato do editor do app de testes (tcg/data/edicoes.json).
import { $, $$, esc, api, DADOS, BASE, EDICOES, VALOR, COR_VALOR, TIPO, textoCarta, textoPoder, sessao, avisar, janela, local, usarEdicoes, carregarEdicoes, dataCurta } from './base.js';
import { desenharCarta, modeloCarta, modeloConjurador } from './carta.js';
import { edicoesVazias } from '/jogo/edicoes.js';
import { descEfeito, protegidoDe } from '/jogo/textos.js';
import { montarConstrutor } from './construtor-efeitos.js';
import { cartaServeConjurador, itemServe } from '/jogo/motor.js';

/** Quais Conjuradores podem usar a carta ou o item com estes Valores (regra das cores, decisão 37; a mesma para itens). */
function quemUsa(c) {
  const cs = Object.values(DADOS.catalogo.conjuradores);
  const ok = cs.filter((j) => (c.tipo === 'item' ? itemServe(j, c) : cartaServeConjurador(c, j))).map((j) => j.nome.split(',')[0]);
  const regra = c.valores.length === 1 ? ' (1 Valor: quem tem esse Valor)' : c.valores.length === 2 ? ' (2 Valores: só quem tem exatamente esse par)' : c.valores.length >= 3 ? ' (3+ Valores: quem tem um par entre eles)' : ' (sem Valor: todos)';
  const extra = c.tipo === 'item' && (c.competencia || c.exclusivoDe?.length) ? ' Itens também respeitam competência e exclusividade.' : '';
  return (ok.length ? `Podem usar${regra}: ${ok.join(', ')}.` : `Nenhum Conjurador pode usar esta carta com estes Valores${regra}.`) + extra;
}

const VALORES = Object.keys(VALOR);
const CHAVE_FILTRO = 'editor:filtro';
/** Nome do conjunto de Valores (cabeçalho do agrupamento): "Alvorum + Primitivum", ou "Sem Valor". */
const nomeConjunto = (vs) => (vs.length ? [...vs].sort((a, b) => VALORES.indexOf(a) - VALORES.indexOf(b)).map((v) => VALOR[v] ?? v).join(' + ') : 'Sem Valor');
/**
 * Filtro por Valor da lista (pedido do autor, 04/10): marcados = Valores escolhidos (e 'sem' = Sem Valor).
 * 'algum': tem algum dos marcados; 'exato': o conjunto de Valores é exatamente o marcado (achar as cartas de um par).
 */
function passaValor(vs, marcados, modo) {
  if (!marcados.length) return true;
  const sem = marcados.includes('sem'), quais = marcados.filter((v) => v !== 'sem');
  if (sem && !vs.length) return true;
  if (!quais.length) return false;
  return modo === 'exato' ? vs.length === quais.length && quais.every((v) => vs.includes(v)) : vs.some((v) => quais.includes(v));
}
const COMPROMETIMENTOS = ['Invocador', 'Brigão', 'Parrudão da quebrada', 'Habilidoso', 'Preparado'];
const NUM_CARTA = [['custo', 'Custo'], ['grau', 'Grau'], ['arcanoGerado', 'Gera Arcano'], ['ataque', 'Ataque'], ['vida', 'Vida'], ['defesa', 'Defesa (armadura)'], ['vidaDesgaste', 'Desgaste (armadura)'], ['descartaGrau', 'Descarta do Grau'], ['pagarParaAnular', 'Pagar para anular'], ['grauExtra', 'Grau extra'], ['espinhos', 'Espinhos (armadura)']];
const NUM_CONJ = [['vida', 'Vida'], ['maoInicial', 'Mão inicial'], ['compraFinal', 'Compra no Final'], ['danoBasico', 'Dano básico'], ['custoAtaque', 'Custo de ataque']];
/** Campos de mecânica editados como JSON. Protegido (04/10, noite) fica no construtor; `protecao` (nome antigo) só sai: é convertido em `protegido`. */
const MEC_CARTA = ['efeitos', 'aoEntrar', 'aoDanoConjurador', 'ativacao', 'custoValores', 'aoAtacar', 'aoProteger', 'habilidades', 'protegido'];
const igual = (a, b) => JSON.stringify(a ?? null) === JSON.stringify(b ?? null);
/** Números citados no texto (efeitos, gatilhos, ativação, Protegido, pagar para anular, descarte do Grau; poder do Conjurador). */
const NUM_TEXTO_CARTA = [...MEC_CARTA, 'pagarParaAnular', 'descartaGrau', 'grauExtra'];
/** Caminho → número de cada valor numérico dentro dos campos indicados. */
function numerosDe(obj, campos) {
  const r = new Map();
  const andar = (v, cam) => {
    if (typeof v === 'number') r.set(cam, v);
    else if (Array.isArray(v)) v.forEach((x, i) => andar(x, `${cam}[${i}]`));
    else if (v && typeof v === 'object') for (const [k, x] of Object.entries(v)) andar(x, `${cam}.${k}`);
  };
  for (const k of campos) andar(obj?.[k], k);
  return r;
}
/**
 * Atualização automática do texto escrito pelo autor (pedido de 04/10): cada número da mecânica que mudou
 * é trocado no texto quando aparece uma vez só; senão, avisa para conferir. Devolve { texto, avisos }.
 */
function atualizarNumerosNoTexto(texto, antes, depois, campos) {
  const a = numerosDe(antes, campos), d = numerosDe(depois, campos);
  const avisos = [];
  let t = texto;
  for (const [cam, novo] of d) {
    const velho = a.get(cam);
    if (velho === undefined || velho === novo) continue;
    const re = new RegExp(String.raw`(?<![\d.,])${velho}(?![\d]|[.,]\d)`, 'g');
    const n = (t.match(re) ?? []).length;
    if (n === 1) t = t.replace(re, String(novo));
    else avisos.push(n === 0 ? `${cam}: ${velho} → ${novo} (o número não aparece no texto)` : `${cam}: ${velho} → ${novo} (o ${velho} aparece ${n} vezes; confira)`);
  }
  return { texto: t, avisos };
}
const CHAVE_RASCUNHO = 'editor:rascunho';
/** Linhas que aceitam requisito (Grau mínimo e Arcano): cada efeito, cada gatilho e cada habilidade ativada. */
function linhasComRequisito(mec, conjur) {
  const l = [];
  const efs = (lista, cam, rot) => (lista ?? []).forEach((ef, i) => l.push({ cam: `${cam}.${i}`, rot: `${rot} ${i + 1}: ${descEfeito({ ...ef, requisito: undefined })}`, req: ef.requisito }));
  if (conjur) {
    if (mec.poder?.ativacao) { l.push({ cam: 'poder.ativacao', rot: 'Habilidade ativada (inteira)', req: mec.poder.ativacao.requisito }); efs(mec.poder.ativacao.efeitos, 'poder.ativacao.efeitos', 'Habilidade, efeito'); }
    efs(mec.poder?.gatilho?.efeitos, 'poder.gatilho.efeitos', 'Gatilho, efeito');
    // habilidades montadas no construtor (Editor de Conjuradores, 04/10)
    (mec.poder?.habilidades ?? []).forEach((h, k) => {
      if (h.quando === 'ativada') l.push({ cam: `poder.habilidades.${k}`, rot: `Habilidade ${k + 1} (inteira)`, req: h.requisito });
      efs(h.efeitos, `poder.habilidades.${k}.efeitos`, `Habilidade ${k + 1}, efeito`);
    });
  } else {
    efs(mec.efeitos, 'efeitos', 'Efeito');
    efs(mec.aoEntrar, 'aoEntrar', 'Ao entrar');
    efs(mec.aoDanoConjurador, 'aoDanoConjurador', 'Ao causar dano ao Conjurador');
    if (mec.ativacao) { l.push({ cam: 'ativacao', rot: 'Ativação (inteira)', req: mec.ativacao.requisito }); efs(mec.ativacao.efeitos, 'ativacao.efeitos', 'Ativação, efeito'); }
    (mec.habilidades ?? []).forEach((h, k) => efs(h.efeitos, `habilidades.${k}.efeitos`, `Habilidade ${k + 1}, efeito`));
    efs(mec.aoAtacar?.efeitos, 'aoAtacar.efeitos', 'Arma, ao atacar');
    efs(mec.aoProteger, 'aoProteger', 'Armadura, ao proteger');
  }
  return l;
}
const pegarCam = (obj, cam) => cam.split('.').reduce((o, k) => o?.[k], obj);

let ARTES = null;
const artes = async () => (ARTES ??= await (await fetch('/jogo/artes.json')).json());

export async function telaEditor(tela, partes, ctx) {
  const u = await ctx.exigirConta('Entre com a conta do autor para editar as cartas.');
  if (!u) return ctx.navegar('inicio');
  if (!u.admin) { tela.innerHTML = '<div class="painel vazio">Só o autor edita as cartas. Entre com o e-mail do autor (Google ou "Sou o autor").</div>'; return; }
  // rascunho: o que foi salvo neste navegador, ou a versão em vigor
  let rascunho = local.ler(CHAVE_RASCUNHO, null) ?? structuredClone(EDICOES.dados);
  rascunho = { ...edicoesVazias(), ...rascunho, artes: { cartas: {}, conjuradores: {}, ...(rascunho.artes ?? {}) }, raridades: { cartas: {}, conjuradores: {}, ...(rascunho.raridades ?? {}) } };
  let sel = partes[0] ? decodeURIComponent(partes[0]) : null; // "c:<id>" (carta) ou "j:<id>" (Conjurador)
  // filtros da lista, lembrados no navegador (pedido do autor, 04/10): tipo, Valores, modo, agrupar e Conjurador
  const filtroSalvo = local.ler(CHAVE_FILTRO, {});
  let filtro = { busca: '', tipo: 'conjurador', valores: [], modo: 'algum', agrupar: false, conj: '', ...filtroSalvo };
  const guardarFiltro = () => { const { busca, ...resto } = filtro; void busca; local.gravar(CHAVE_FILTRO, resto); };
  /**
   * Texto que o editor gerou sozinho da mecânica (não conta como texto do autor); null = texto do autor.
   * Variável, não `form.dataset.textoAuto`: o atributo `data-texto-auto` no formulário fazia todo clique nele
   * cair no botão "Gerar o texto" (bug de 04/10: a arte não trocava e clicar no texto o reescrevia).
   */
  let textoAuto = null;

  const aplicarRascunho = () => {
    usarEdicoes({ id: EDICOES.id, dados: rascunho, rascunho: difereDoSalvo() });
    local.gravar(CHAVE_RASCUNHO, rascunho);
  };
  const mudancas = () => new Set([...Object.keys(rascunho.cartas).map((x) => 'c:' + x), ...Object.keys(rascunho.artes.cartas).map((x) => 'c:' + x), ...Object.keys(rascunho.raridades.cartas).map((x) => 'c:' + x), ...Object.keys(rascunho.conjuradores).map((x) => 'j:' + x), ...Object.keys(rascunho.artes.conjuradores).map((x) => 'j:' + x), ...Object.keys(rascunho.raridades.conjuradores).map((x) => 'j:' + x)]);
  const difereDoSalvo = () => !igual(rascunho, EDICOES.salvo ?? EDICOES.dados);
  EDICOES.salvo ??= structuredClone(EDICOES.dados);
  /** Cartas/Conjuradores do rascunho diferentes da versão salva no site (contador do botão Salvar). */
  const pendentes = () => {
    const s = EDICOES.salvo ?? EDICOES.dados, r = new Set();
    for (const [t, g] of [['c', 'cartas'], ['j', 'conjuradores']]) {
      for (const [a, b] of [[rascunho[g], s[g]], [rascunho.artes[g], s.artes?.[g]], [rascunho.raridades[g], s.raridades?.[g]]]) {
        for (const id of new Set([...Object.keys(a ?? {}), ...Object.keys(b ?? {})])) if (!igual(a?.[id], b?.[id])) r.add(t + ':' + id);
      }
    }
    return r.size;
  };
  // Mudança não salva impossível de ignorar (pedido do autor, 04/10): aviso ao fechar/recarregar e ao sair do Editor
  globalThis.editorPendente = pendentes;
  if (!globalThis.editorAvisoFechar) {
    globalThis.editorAvisoFechar = true;
    window.addEventListener('beforeunload', (ev) => { if (globalThis.editorPendente?.()) { ev.preventDefault(); ev.returnValue = ''; } });
  }
  ctx.definirSaida?.(() => new Promise((ok) => {
    // um erro ao ler o formulário nunca pode prender o autor no Editor
    clearTimeout(espera); try { if (sel) lerForm(); } catch (err) { console.error('editor: lerForm', err); }
    const n = pendentes();
    if (!n) return ok(true);
    const j = janela(`<h3>Edição não salva</h3><p>Você tem <b>${n}</b> mudança(s) não salva(s). Os outros jogadores só veem depois de salvar.</p>
      <div class="linha-botoes"><button class="botao primario" data-s="salvar">💾 Salvar</button><button class="botao" data-s="testar">Testar sem salvar</button><button class="botao" data-s="cancelar">Cancelar</button></div>`, { aoFechar: () => { if (!escolha) ok(false); } });
    let escolha = null;
    j.el.addEventListener('click', (ev) => {
      const b = ev.target.closest('[data-s]');
      if (!b) return;
      escolha = b.dataset.s;
      j.fechar();
      if (escolha === 'testar') return ok(true);
      if (escolha === 'cancelar') return ok(false);
      abrirSalvar(() => ok(true), () => ok(false));
    });
  }));
  aplicarRascunho();

  tela.classList.add('cheia');
  tela.innerHTML = `<div class="editor-barra-salvar painel"><span class="editor-barra-estado" data-barra-estado></span><button class="botao primario" data-salvar>💾 Salvar edição</button></div>
    <div class="editor">
    <aside class="editor-lista painel">
      <div class="titulo-secao"><h2>Editor</h2></div>
      <input type="search" placeholder="Buscar" data-busca>
      <div class="abas editor-tipos">${[['conjurador', 'Conjuradores'], ['aliado', 'Aliados'], ['temporaria', 'Temporárias'], ['mistica', 'Místicas'], ['estrutura', 'Estruturas'], ['item', 'Itens'], ['mudadas', 'Mudadas'], ['provisorias', 'Provisórias']].map(([k, r]) => `<button data-tipo="${k}">${r}</button>`).join('')}</div>
      <div class="abas editor-tipos" title="Filtrar por Valor (vários ao mesmo tempo)">${[...VALORES.map((v) => [v, `<span class="chip-valor" style="background:${COR_VALOR[v]}">${esc(VALOR[v])}</span>`]), ['sem', 'Sem Valor']].map(([k, r]) => `<button data-valor="${k}">${r}</button>`).join('')}</div>
      <div class="filtros">
        <label class="opcao-linha"><input type="radio" name="editor-modo-valor" value="algum" data-modo-valor> Tem algum dos marcados</label>
        <label class="opcao-linha"><input type="radio" name="editor-modo-valor" value="exato" data-modo-valor> Exatamente estes</label>
        <label class="opcao-linha"><input type="checkbox" data-agrupar> Agrupar por Valor</label>
        <select data-conj-filtro><option value="">Qualquer Conjurador</option>${Object.values(DADOS.catalogo.conjuradores).map((j) => `<option value="${esc(j.id)}">Cartas que ${esc(j.nome.split(',')[0])} pode usar</option>`).join('')}</select>
      </div>
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
    const barra = $('[data-barra-estado]', tela);
    if (barra) {
      barra.innerHTML = difereDoSalvo()
        ? `<b class="falta">Edição não salva</b> (${n} carta(s)/Conjurador(es) no rascunho). Só vale para todos depois de salvar.`
        : `<b class="ok-salvo">Tudo salvo</b> · versão em vigor no site: ${EDICOES.id || 'nenhuma edição'}.`;
      tela.querySelector('.editor-barra-salvar').classList.toggle('pendente', difereDoSalvo());
      const np = pendentes();
      tela.querySelector('.editor-barra-salvar [data-salvar]').textContent = np ? `💾 Salvar edição (${np} não salva${np > 1 ? 's' : ''})` : '💾 Salvar edição';
    }
    estado.innerHTML = `<p class="peq">Versão em vigor no site: <b>${EDICOES.id || 'nenhuma edição'}</b>. Rascunho: <b>${n}</b> carta(s)/Conjurador(es) mudados${difereDoSalvo() ? ' · <span class="falta">não salvo</span>' : ' · salvo'}.<br>O rascunho já vale nesta página (teste contra o bot); os outros jogadores só veem depois de salvar.</p>`;
  }
  function desenharLista() {
    for (const b of $$('[data-tipo]', tela)) b.classList.toggle('ativo', b.dataset.tipo === filtro.tipo);
    for (const b of $$('[data-valor]', tela)) b.classList.toggle('ativo', filtro.valores.includes(b.dataset.valor));
    for (const r of $$('[data-modo-valor]', tela)) r.checked = r.value === filtro.modo;
    $('[data-agrupar]', tela).checked = !!filtro.agrupar;
    $('[data-conj-filtro]', tela).value = filtro.conj;
    const conjF = filtro.conj ? DADOS.catalogo.conjuradores[filtro.conj] : null;
    const mud = mudancas();
    const busca = filtro.busca.toLowerCase();
    const itens = [
      ...Object.values(DADOS.catalogo.conjuradores).map((c) => ({ k: 'j:' + c.id, nome: c.nome, tipo: 'conjurador', valores: c.valores, serve: !conjF || c.id === conjF.id, sub: c.valores.map((v) => VALOR[v]).join(' + ') })),
      ...Object.values(DADOS.catalogo.cartas).filter((c) => c.tipo !== 'oculta').map((c) => ({ k: 'c:' + c.id, nome: c.nome, tipo: c.tipo, prov: !!c.provisoria, valores: c.valores ?? [], serve: !conjF || (c.tipo === 'item' ? itemServe(conjF, c) : cartaServeConjurador(c, conjF)), sub: `${TIPO[c.categoriaItem ?? c.tipo] ?? c.tipo} · custo ${c.custo} · Grau ${c.grau}` })),
    ].filter((x) => (filtro.tipo === 'mudadas' ? mud.has(x.k) : filtro.tipo === 'provisorias' ? x.prov : x.tipo === filtro.tipo) && (!busca || x.nome.toLowerCase().includes(busca) || x.k.includes(busca))
      && x.serve && passaValor(x.valores, filtro.valores, filtro.modo));
    const botao = (x) => `<button class="editor-item ${x.k === sel ? 'ativo' : ''}" data-k="${esc(x.k)}">${mud.has(x.k) ? '<span class="ponto-mudou" title="Mudada no rascunho">●</span> ' : ''}<b>${esc(x.nome)}</b>${x.prov ? ' <span class="etiqueta" title="Carta provisória: criada para os baralhos após a regra das cores; revise efeito e números">provisória</span>' : ''}<span class="peq">${esc(x.sub)}</span></button>`;
    if (!itens.length) { lista.innerHTML = '<p class="peq">Nada encontrado.</p>'; return; }
    if (!filtro.agrupar) { lista.innerHTML = itens.map(botao).join(''); return; }
    // agrupado por Valor: a carta de 2+ Valores fica no grupo do conjunto dela ("Alvorum + Primitivum")
    const grupos = new Map();
    for (const x of itens) { const g = nomeConjunto(x.valores); if (!grupos.has(g)) grupos.set(g, { n: x.valores.length, itens: [] }); grupos.get(g).itens.push(x); }
    lista.innerHTML = [...grupos].sort(([a, ga], [b, gb]) => (ga.n || 9) - (gb.n || 9) || a.localeCompare(b))
      .map(([g, { itens: xs }]) => `<p class="peq"><b>${esc(g)}</b> · ${xs.length}</p>${xs.map(botao).join('')}`).join('');
  }

  // ---------- formulário ----------
  const numCampo = (k, rot, v) => `<label class="campo campo-num">${rot}<input type="number" step="1" data-num="${k}" value="${v ?? ''}" placeholder="—"></label>`;
  function desenharForm() {
    textoAuto = null;
    if (!sel) { form.innerHTML = '<p class="vazio">Escolha uma carta ou um Conjurador na lista.</p>'; preview.innerHTML = ''; return; }
    const [t, id] = [sel.slice(0, 1), sel.slice(2)];
    const conjur = t === 'j';
    const c = conjur ? DADOS.catalogo.conjuradores[id] : DADOS.catalogo.cartas[id];
    if (!c) { form.innerHTML = '<p class="vazio">Não encontrada.</p>'; return; }
    const arteAtual = conjur ? DADOS.conjuradoresArte[id] : DADOS.visual[id]?.arte;
    const mec = conjur ? { poder: c.poder ?? null } : Object.fromEntries(MEC_CARTA.filter((k) => c[k] !== undefined).map((k) => [k, c[k]]));
    // nome antigo "Proteção mágica" (protecao): abre como parte do Protegido
    if (!conjur && c.protecao) { const pr = protegidoDe(c); if (pr) mec.protegido = Object.keys(pr).length === 1 && pr.arcano ? pr.arcano : pr; }
    form.innerHTML = `<div class="titulo-secao"><h2>${esc(c.nome)}</h2><span class="etiqueta">${conjur ? 'Conjurador' : esc(TIPO[c.categoriaItem ?? c.tipo] ?? c.tipo)} · ${esc(id)}</span>${!conjur && c.provisoria ? ' <span class="etiqueta">provisória</span>' : ''}</div>
      <label class="campo">Nome<input type="text" data-f="nome" value="${esc(c.nome)}"></label>
      <label class="campo">${conjur ? 'Habilidade (texto)' : 'Texto de regras'}<textarea data-f="${conjur ? 'habilidade' : 'texto'}">${esc(conjur ? c.habilidade : c.texto)}</textarea></label>
      <div class="linha-botoes"><button class="botao peq" data-texto-auto>Gerar o texto a partir da mecânica</button></div>
      <div class="campo"><span>Arte</span><div class="editor-arte"><img src="/arte/${esc(arteAtual)}.webp" alt=""><div><b>${esc(arteAtual ?? 'sem arte')}</b><div class="linha-botoes"><button class="botao peq" data-trocar-arte>Trocar arte</button></div></div></div></div>
      <label class="campo">Raridade (só aparência; você decide)<select data-raridade>${[['', 'Sem raridade'], ['comum', 'Comum'], ['incomum', 'Incomum'], ['rara', 'Rara'], ['lendaria', 'Lendária']].map(([v, r]) => `<option value="${v}" ${(rascunho.raridades[conjur ? 'conjuradores' : 'cartas'][id] ?? '') === v ? 'selected' : ''}>${r}</option>`).join('')}</select></label>
      <div class="editor-nums">${(conjur ? NUM_CONJ : NUM_CARTA).map(([k, r]) => numCampo(k, r, c[k])).join('')}</div>
      <div class="campo"><span>Valores</span><div class="editor-valores">${VALORES.map((v) => `<label class="opcao-linha"><input type="checkbox" data-valor="${v}" ${c.valores.includes(v) ? 'checked' : ''}> ${esc(VALOR[v])}</label>`).join('')}</div>${conjur ? '' : `<p class="peq" data-quem-usa>${esc(quemUsa(c))}</p>`}</div>
      ${conjur
        ? `<label class="campo">Comprometimento<select data-f="comprometimento">${COMPROMETIMENTOS.map((x) => `<option ${x === c.comprometimento ? 'selected' : ''}>${esc(x)}</option>`).join('')}</select></label>`
        : c.tipo === 'aliado'
        ? `<label class="campo">Velocidade<select data-f="velocidade" disabled><option value="findante" selected>Findante (para entrar a qualquer momento, palavra-chave Repentino)</option></select></label>
           <label class="campo">Palavras-chave (separadas por vírgula)<input type="text" data-f="palavrasChave" value="${esc((c.palavrasChave ?? []).join(', '))}" placeholder="Proficiente, Etéreo, Repentino"></label>
           <label class="campo">Subtipos (separados por vírgula)<input type="text" data-f="subtipos" value="${esc((c.subtipos ?? []).join(', '))}"></label>`
        : `<label class="campo">Velocidade<select data-f="velocidade"><option value="findante" ${c.velocidade === 'findante' ? 'selected' : ''}>Findante (na sua fase principal)</option><option value="repentina" ${c.velocidade === 'repentina' ? 'selected' : ''}>Repentina (a qualquer momento)</option></select></label>
           <label class="campo">Palavras-chave (separadas por vírgula)<input type="text" data-f="palavrasChave" value="${esc((c.palavrasChave ?? []).join(', '))}" placeholder="Proficiente, Etéreo"></label>
           <label class="campo">Subtipos (separados por vírgula)<input type="text" data-f="subtipos" value="${esc((c.subtipos ?? []).join(', '))}"></label>`}
      <div class="campo"><span>Construtor de efeitos <span class="peq">${conjur ? '(habilidades ativadas e gatilhos com efeitos que se acumulam, passivas e permissões; vira o poder real e o texto da habilidade)' : '(condição + efeitos que se acumulam; vira a mecânica real e o texto da carta)'}</span></span><div class="editor-construtor" data-construtor></div></div>
      <label class="campo">Mecânica em JSON (${conjur ? 'poder; alternativa avançada ao construtor' : MEC_CARTA.join(', ')})<textarea class="editor-json" data-json spellcheck="false">${esc(JSON.stringify(mec, null, 2))}</textarea></label>
      <p class="peq editor-json-erro" data-json-erro></p>
      <div class="campo"><span>Requisitos por linha (Grau mínimo e Arcano a pagar; sem cumprir, a linha não acontece)</span><div class="editor-req" data-req-lista></div></div>
      <p class="peq editor-auto" data-auto-aviso></p>
      <div class="linha-botoes"><button class="botao perigo peq" data-desfazer>Desfazer as mudanças desta ${conjur ? 'Conjurador' : 'carta'}</button></div>`;
    // Texto automático (bug de 04/10: ao reabrir a carta, o texto gerado virava "texto do autor" e congelava;
    // apagar efeitos no JSON não mudava mais a carta). Sem texto no rascunho, ou texto igual ao gerado da
    // mecânica atual, o texto continua automático e acompanha a mecânica.
    {
      const campoTxt = conjur ? 'habilidade' : 'texto';
      const atual = c[campoTxt];
      let gerado = null;
      try { gerado = conjur ? textoPoder(c.poder) : textoCarta({ ...c, texto: '' }); } catch { /* mecânica que o texto não descreve */ }
      if (!(campoTxt in ((conjur ? rascunho.conjuradores : rascunho.cartas)[id] ?? {})) || atual === gerado) textoAuto = atual;
    }
    construtor = montarConstrutor($('[data-construtor]', form), mec, {
      conjur, tipoCarta: c.tipo, form, fichas: Object.values(DADOS.catalogo.cartas).filter((x) => x.ficha).map((x) => [x.id, x.nome]),
      aoMudar: (novo) => { $('[data-json]', form).value = JSON.stringify(novo, null, 2); desenharRequisitos(); clearTimeout(espera); lerForm(); },
    });
    desenharRequisitos();
    desenharPreview();
  }
  /** Construtor aberto (recarregado quando o JSON muda por fora: requisitos por linha ou JSON avançado). */
  let construtor = null;
  const recarregarConstrutor = () => { try { construtor?.definir(JSON.parse($('[data-json]', form).value || '{}')); } catch { /* JSON inválido: o aviso já aparece */ } };
  function desenharRequisitos() {
    const caixa = $('[data-req-lista]', form);
    if (!caixa) return;
    let mec;
    try { mec = JSON.parse($('[data-json]', form).value || '{}'); } catch { return; }
    const linhas = linhasComRequisito(mec, sel.startsWith('j:'));
    caixa.innerHTML = linhas.length ? linhas.map((x) => `<div class="editor-req-linha"><span class="peq">${esc(x.rot)}</span>
      <label>Grau <input type="number" min="0" step="1" data-req="${x.cam}" data-req-k="grau" value="${x.req?.grau ?? ''}" placeholder="—"></label>
      <label>Arcano <input type="number" min="0" step="1" data-req="${x.cam}" data-req-k="arcano" value="${x.req?.arcano ?? ''}" placeholder="—"></label></div>`).join('') : '<p class="peq">Esta carta não tem efeitos nem habilidades.</p>';
  }
  /** Mudou um requisito: grava no JSON da mecânica (que o motor lê) e aplica na hora. */
  form.addEventListener('input', (ev) => {
    const inp = ev.target.closest('[data-req]');
    if (!inp) return;
    ev.stopImmediatePropagation();
    const caixa = $('[data-json]', form);
    let mec;
    try { mec = JSON.parse(caixa.value || '{}'); } catch { return; }
    const alvo = pegarCam(mec, inp.dataset.req);
    if (!alvo) return;
    const req = { ...(alvo.requisito ?? {}) };
    const v = inp.value === '' ? 0 : Math.max(0, Math.trunc(Number(inp.value)));
    if (v) req[inp.dataset.reqK] = v; else delete req[inp.dataset.reqK];
    if (Object.keys(req).length) alvo.requisito = req; else delete alvo.requisito;
    caixa.value = JSON.stringify(mec, null, 2);
    clearTimeout(espera); espera = setTimeout(() => { lerForm(); recarregarConstrutor(); }, 350);
  }, true);
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
    const antes = structuredClone(conjur ? DADOS.catalogo.conjuradores[id] : DADOS.catalogo.cartas[id]);
    const novo = {};
    for (const el of $$('[data-f]', form)) {
      const k = el.dataset.f;
      novo[k] = ['palavrasChave', 'subtipos'].includes(k) ? el.value.split(',').map((x) => x.trim()).filter(Boolean) : el.value;
    }
    const avisoAuto = [];
    // Aliado nunca é Repentina pelo tipo ou subtipo (decisão 21); para entrar fora da fase principal, a palavra-chave Repentino
    if (!conjur && base.tipo === 'aliado') {
      novo.velocidade = base.velocidade === 'repentina' ? 'findante' : base.velocidade;
      if (novo.palavrasChave.some((x) => /^repentin[ao]$/i.test(x) && x !== 'Repentino')) {
        novo.palavrasChave = novo.palavrasChave.map((x) => (/^repentin[ao]$/i.test(x) ? 'Repentino' : x));
        $('[data-f="palavrasChave"]', form).value = novo.palavrasChave.join(', ');
        avisoAuto.push('Em Aliado a palavra-chave é "Repentino".');
      }
      for (const k of ['subtipos']) {
        const sem = novo[k].filter((x) => !/^repentin/i.test(x));
        if (sem.length !== novo[k].length) avisoAuto.push('Aliado não pode ser Repentina no subtipo: tirei. Para entrar fora da fase principal, use a palavra-chave Repentino.');
        if (sem.length !== novo[k].length) $(`[data-f="${k}"]`, form).value = sem.join(', ');
        novo[k] = sem;
      }
    }
    // texto que o editor escreveu sozinho (gerado da mecânica) não conta como texto do autor
    const campoTxt = conjur ? 'habilidade' : 'texto';
    if (textoAuto !== null && novo[campoTxt] === textoAuto) novo[campoTxt] = base[campoTxt];
    if (!conjur && !novo.palavrasChave.length) novo.palavrasChave = undefined;
    for (const el of $$('[data-num]', form)) novo[el.dataset.num] = el.value === '' ? undefined : Math.trunc(Number(el.value));
    novo.valores = $$('[data-valor]', form).filter((x) => x.checked).map((x) => x.dataset.valor);
    if (!conjur && $('[data-quem-usa]', form)) $('[data-quem-usa]', form).textContent = quemUsa({ ...base, ...novo, tipo: base.tipo });
    const erroJson = $('[data-json-erro]', form);
    try {
      const mec = JSON.parse($('[data-json]', form).value || '{}');
      if (typeof mec !== 'object' || Array.isArray(mec) || mec === null) throw new Error('use um objeto { }');
      for (const k of conjur ? ['poder'] : MEC_CARTA) novo[k] = mec[k] ?? undefined;
      if (!conjur) novo.protecao = undefined; // nome antigo: convertido em `protegido` ao salvar
      erroJson.textContent = '';
    } catch (err) {
      // JSON inválido (no meio da digitação): mantém a mecânica atual do rascunho, sem voltar à da base
      erroJson.textContent = 'JSON inválido (a mecânica não foi aplicada): ' + err.message;
      for (const k of conjur ? ['poder'] : MEC_CARTA) novo[k] = antes[k] ?? undefined;
      if (!conjur) novo.protecao = undefined;
    }
    const parcial = {};
    for (const [k, v] of Object.entries(novo)) {
      if (!(k in novo) || igual(v, base[k])) continue;
      if (v === undefined) { if (base[k] !== undefined) parcial[k] = null; } else parcial[k] = v;
    }
    const rar = $('[data-raridade]', form)?.value;
    const grupoRar = rascunho.raridades[conjur ? 'conjuradores' : 'cartas'];
    if (rar) grupoRar[id] = rar; else delete grupoRar[id];
    // texto: se só mudou a mecânica, o texto continua automático (gerado na hora); se o autor escreveu, vale o dele
    const grupo = conjur ? rascunho.conjuradores : rascunho.cartas;
    if (Object.keys(parcial).length) grupo[id] = parcial; else delete grupo[id];
    aplicarRascunho();
    // Atualização automática (pedido do autor, 04/10): mudou um número da mecânica → o texto acompanha.
    const depois = conjur ? DADOS.catalogo.conjuradores[id] : DADOS.catalogo.cartas[id];
    const caixa = $(`[data-f="${campoTxt}"]`, form);
    if (grupo[id] && campoTxt in grupo[id]) {
      // texto escrito pelo autor: troca os números que mudaram
      const r = atualizarNumerosNoTexto(grupo[id][campoTxt], antes, depois, conjur ? ['poder'] : NUM_TEXTO_CARTA);
      if (r.texto !== grupo[id][campoTxt]) { grupo[id][campoTxt] = r.texto; caixa.value = r.texto; aplicarRascunho(); avisoAuto.push('Números do texto atualizados sozinhos.'); }
      if (r.avisos.length) avisoAuto.push('Confira o texto: ' + r.avisos.join('; ') + '.');
      textoAuto = null;
    } else if (caixa && caixa.value !== depois[campoTxt] && document.activeElement !== caixa) {
      // texto automático: mostra o texto novo gerado da mecânica (carta) ou do poder (Conjurador)
      caixa.value = depois[campoTxt]; textoAuto = depois[campoTxt];
      avisoAuto.push('Texto gerado de novo a partir da mecânica.');
    }
    const av = $('[data-auto-aviso]', form);
    if (av) av.textContent = avisoAuto.join(' ');
    desenharPreview();
    desenharLista();
    desenharEstado();
  }
  let espera = null;
  form.addEventListener('input', () => { clearTimeout(espera); espera = setTimeout(lerForm, 350); });
  form.addEventListener('change', (ev) => { clearTimeout(espera); lerForm(); if (ev.target.matches('[data-json]')) { desenharRequisitos(); recarregarConstrutor(); } });
  form.addEventListener('click', async (ev) => {
    const [t, id] = sel ? [sel.slice(0, 1), sel.slice(2)] : [];
    if (ev.target.closest('[data-desfazer]')) {
      delete (t === 'j' ? rascunho.conjuradores : rascunho.cartas)[id];
      delete (t === 'j' ? rascunho.artes.conjuradores : rascunho.artes.cartas)[id];
      delete rascunho.raridades[t === 'j' ? 'conjuradores' : 'cartas'][id];
      aplicarRascunho(); desenharTudo(); return;
    }
    if (ev.target.closest('button[data-texto-auto]')) {
      // o texto volta a ser automático (acompanha a mecânica daqui em diante)
      const caixaTxt = $(t === 'j' ? '[data-f="habilidade"]' : '[data-f="texto"]', form);
      caixaTxt.value = textoAuto = t === 'j' ? textoPoder(DADOS.catalogo.conjuradores[id].poder) : textoCarta({ ...DADOS.catalogo.cartas[id], texto: '' });
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
  /** Janela de salvar no site. `aoSalvar`/`aoCancelar`: usados ao sair do Editor com mudanças pendentes. */
  function abrirSalvar(aoSalvar, aoCancelar) {
    // grava no rascunho a última mudança ainda esperando o intervalo de digitação
    if (sel) { clearTimeout(espera); lerForm(); }
    let salvou = false;
    const j = janela(`<h3>Salvar no site</h3><p class="peq">A versão nova vale para todos a partir de agora (partidas online já começadas continuam com a versão do início).</p>
      <label class="campo">Nota (o porquê da mudança)<textarea data-nota maxlength="500"></textarea></label><div class="linha-botoes"><button class="botao primario" data-ok>Salvar</button></div>`, { aoFechar: () => { if (!salvou) aoCancelar?.(); } });
    $('[data-ok]', j.el).onclick = async () => {
      try {
        const r = await api('/edicoes', { corpo: { dados: rascunho, nota: $('[data-nota]', j.el).value } });
        salvou = true;
        j.fechar();
        await carregarEdicoes();
        EDICOES.salvo = structuredClone(EDICOES.dados);
        rascunho = structuredClone(EDICOES.dados);
        aplicarRascunho();
        avisar(`Versão #${r.id} salva (${r.mudancas} mudança(s)).`, 'ok', 5000);
        if (r.avisos?.length) janela(`<h3>Atenção: baralhos prontos afetados</h3><p class="peq">As edições foram salvas, mas estes baralhos prontos deixaram de valer e não abrem partida até serem ajustados:</p><ul>${r.avisos.map((a) => `<li>${esc(a)}</li>`).join('')}</ul>`);
        if (aoSalvar) return aoSalvar();
        desenharTudo(); desenharHistorico();
      } catch (err) { avisar(err.message, 'erro', 6000); }
    };
  }
  // salvar: botão fixo no topo e o da coluna da direita (pedido do autor, 04/10: a edição só vale depois de salvar)
  tela.addEventListener('click', async (ev) => {
    if (!ev.target.closest('[data-salvar], [data-baixar], [data-descartar], [data-versao]')) return;
    if (ev.target.closest('[data-salvar]')) { abrirSalvar(); return; }
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
        rascunho = { ...edicoesVazias(), ...r.dados, raridades: { cartas: {}, conjuradores: {}, ...(r.dados.raridades ?? {}) } };
        aplicarRascunho(); desenharTudo(); avisar(`Versão #${r.id} aberta como rascunho. Salve para ela voltar a valer.`, 'info', 5000);
      } catch (err) { avisar(err.message, 'erro'); }
    }
  });
  $('[data-carregar]', tela).onchange = async (ev) => {
    const f = ev.target.files?.[0];
    if (!f) return;
    try {
      const d = JSON.parse(await f.text());
      rascunho = { ...edicoesVazias(), cartas: d.cartas ?? {}, conjuradores: d.conjuradores ?? {}, artes: { cartas: {}, conjuradores: {}, ...(d.artes ?? {}) }, raridades: { cartas: {}, conjuradores: {}, ...(d.raridades ?? {}) } };
      aplicarRascunho(); desenharTudo(); avisar('Edições carregadas no rascunho.', 'ok');
    } catch (err) { avisar('Arquivo inválido: ' + err.message, 'erro'); }
    ev.target.value = '';
  };

  // ---------- lista ----------
  $('[data-busca]', tela).oninput = (ev) => { filtro.busca = ev.target.value; desenharLista(); };
  for (const r of $$('[data-modo-valor]', tela)) r.onchange = () => { filtro.modo = r.value; guardarFiltro(); desenharLista(); };
  $('[data-agrupar]', tela).onchange = (ev) => { filtro.agrupar = ev.target.checked; guardarFiltro(); desenharLista(); };
  $('[data-conj-filtro]', tela).onchange = (ev) => { filtro.conj = ev.target.value; guardarFiltro(); desenharLista(); };
  $('.editor-lista', tela).addEventListener('click', (ev) => {
    const tp = ev.target.closest('[data-tipo]');
    if (tp) { filtro.tipo = tp.dataset.tipo; guardarFiltro(); desenharLista(); return; }
    const vb = ev.target.closest('[data-valor]');
    if (vb) { const v = vb.dataset.valor; filtro.valores = filtro.valores.includes(v) ? filtro.valores.filter((x) => x !== v) : [...filtro.valores, v]; guardarFiltro(); desenharLista(); return; }
    const it = ev.target.closest('[data-k]');
    if (it) { sel = it.dataset.k; history.replaceState(null, '', '#/editor/' + encodeURIComponent(sel)); desenharLista(); desenharForm(); }
  });
  function desenharTudo() { desenharLista(); desenharForm(); desenharEstado(); }
  if (sel) filtro.tipo = sel.startsWith('j:') ? 'conjurador' : DADOS.catalogo.cartas[sel.slice(2)]?.tipo ?? 'aliado';
  desenharTudo();
  desenharHistorico();
  ctx.definirLimpeza(() => tela.classList.remove('cheia'));
}
