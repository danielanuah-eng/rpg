// Comentários (opiniões, notas e dúvidas, com aprovação do autor) e Ideias (expansões, personagens, cartas, itens, Conjuradores).
import { $, $$, esc, api, conj, nomeCurto, cenario, sessao, avisar, janela, dataCurta } from './base.js';

const TIPOS = { opiniao: 'Opinião', duvida: 'Dúvida', bug: 'Problema', balanceamento: 'Balanceamento', carta: 'Carta', elogio: 'Elogio', comentario: 'Comentário' };
const ESTADOS_OP = { novo: 'Novo', aprovado: 'Aprovado para mudar', revisar: 'Em revisão', recusado: 'Não vai mudar', feito: 'Feito' };
const CATEGORIAS = { expansao: 'Expansão', personagem: 'Personagem', carta: 'Carta', item: 'Item', conjurador: 'Conjurador', mecanica: 'Mecânica', outro: 'Outro' };
const ESTADOS_ID = { nova: 'Nova', aprovada: 'Aprovada', estudando: 'Estudando', recusada: 'Recusada', feita: 'Feita' };
const estrelas = (n) => (n ? `<span class="nota" title="${n} de 5">${'★'.repeat(n)}${'☆'.repeat(5 - n)}</span>` : '');

export async function telaComentarios(tela, partes, ctx) {
  ctx.fundo(cenario('comunidade'));
  const filtro = { estado: '', tipo: '' };
  tela.innerHTML = `<div class="titulo-secao"><h1>Comentários</h1><button class="botao rosa" data-novo>💬 Escrever</button></div>
    <p class="peq">Opiniões, notas, dúvidas e problemas mandados pelos jogadores (também de dentro das partidas). O autor marca o que passa para mudança ou revisão.</p>
    <div class="filtros"><select id="f-estado"><option value="">Todos os estados</option>${Object.entries(ESTADOS_OP).map(([k, r]) => `<option value="${k}">${r}</option>`).join('')}</select><select id="f-tipo"><option value="">Todos os tipos</option>${Object.entries(TIPOS).filter(([k]) => k !== 'comentario').map(([k, r]) => `<option value="${k}">${r}</option>`).join('')}</select></div>
    <div id="contagem" class="peq"></div><div id="posts"><div class="girando"></div></div>`;
  const carregar = async () => {
    const q = new URLSearchParams(Object.entries(filtro).filter(([, v]) => v));
    try {
      const r = await api('/opinioes?' + q);
      $('#contagem').innerHTML = r.contagem.map((c) => `<span class="etiqueta estado-${c.estado}">${esc(ESTADOS_OP[c.estado] ?? c.estado)}: ${c.n}</span>`).join(' ');
      $('#posts').innerHTML = r.opinioes.map(postOpiniao).join('') || '<div class="painel vazio">Nenhum comentário ainda.</div>';
    } catch (err) { $('#posts').innerHTML = `<div class="painel vazio">${esc(err.message)}</div>`; }
  };
  tela.onchange = (ev) => { if (ev.target.id === 'f-estado') filtro.estado = ev.target.value; if (ev.target.id === 'f-tipo') filtro.tipo = ev.target.value; carregar(); };
  tela.onclick = async (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    if (b.matches('[data-novo]')) return ctx.opinar(null);
    if (b.dataset.responder) {
      const u = await ctx.exigirConta('Entre para responder.');
      if (!u) return;
      const j = janela('<h3>Responder</h3><form><textarea name="t" maxlength="2000" required></textarea><button class="botao primario">Enviar</button></form>');
      $('form', j.el).onsubmit = async (e2) => { e2.preventDefault(); try { await api(`/opinioes/${b.dataset.responder}/responder`, { corpo: { texto: e2.target.t.value } }); j.fechar(); carregar(); } catch (err) { avisar(err.message, 'erro'); } };
    }
    if (b.dataset.decidir) decidir(`/opinioes/${b.dataset.decidir}/estado`, ESTADOS_OP, carregar);
    if (b.dataset.contexto) {
      const post = JSON.parse(decodeURIComponent(b.dataset.contexto));
      janela(`<h3>Momento da partida</h3><p class="peq">${esc(post.modo ?? '')} · turno ${post.turno} · ${esc(post.etapa)} · ${esc(conjNome(post.eu))} × ${esc(conjNome(post.oponente))} · vida ${post.vida?.join(' × ') ?? '?'} · dados ${esc(post.versao ?? '')}</p><div class="registro-ctx">${(post.registro ?? []).map((l) => `<div>${esc(l)}</div>`).join('')}</div>`, { classe: 'larga' });
    }
  };
  carregar();
}
const conjNome = (id) => (id && conj(id) ? nomeCurto(conj(id)) : id ?? '?');

function postOpiniao(o) {
  const admin = sessao.usuario?.admin;
  return `<article class="post">
    <div class="cab"><span class="etiqueta">${esc(TIPOS[o.tipo] ?? o.tipo)}</span><span class="etiqueta estado-${o.estado}">${esc(ESTADOS_OP[o.estado] ?? o.estado)}</span>${estrelas(o.nota)}<b>${esc(o.autor)}</b><span>${dataCurta(o.criado)}</span>
      ${o.contexto ? `<button class="botao peq fantasma" data-contexto="${encodeURIComponent(JSON.stringify(o.contexto))}">🎴 ${esc(conjNome(o.contexto.eu))} × ${esc(conjNome(o.contexto.oponente))}, turno ${o.contexto.turno}</button>` : ''}</div>
    <div class="corpo">${esc(o.texto)}</div>
    ${o.resposta ? `<div class="resposta-autor"><b>Autor:</b> ${esc(o.resposta)}</div>` : ''}
    ${o.respostas?.length ? `<div class="respostas">${o.respostas.map((r) => `<div><b>${esc(r.autor)}</b> <span class="peq">${dataCurta(r.criado)}</span><br>${esc(r.texto)}</div>`).join('')}</div>` : ''}
    <div class="linha-botoes"><button class="botao peq" data-responder="${o.id}">Responder</button>${admin ? `<button class="botao peq primario" data-decidir="${o.id}">Decidir (autor)</button>` : ''}</div>
  </article>`;
}

function decidir(rota, estados, depois) {
  const j = janela(`<h3>Decisão do autor</h3><form><label class="campo">Estado<select name="estado">${Object.entries(estados).map(([k, r]) => `<option value="${k}">${r}</option>`).join('')}</select></label><label class="campo">Resposta (aparece para todos, opcional)<textarea name="resposta" maxlength="2000"></textarea></label><button class="botao primario">Salvar</button></form>`);
  $('form', j.el).onsubmit = async (ev) => {
    ev.preventDefault();
    try { await api(rota, { corpo: { estado: ev.target.estado.value, resposta: ev.target.resposta.value } }); j.fechar(); avisar('Decisão salva.', 'ok'); depois(); } catch (err) { avisar(err.message, 'erro'); }
  };
}

export async function telaIdeias(tela, partes, ctx) {
  ctx.fundo(cenario('ideias'));
  let categoria = partes[0] ?? '';
  tela.innerHTML = `<div class="titulo-secao"><h1>Ideias</h1><button class="botao primario" data-nova>✨ Propor ideia</button></div>
    <p class="peq">Proponha expansões, personagens, cartas, itens, Conjuradores e mecânicas. Vote nas que você quer ver no jogo; o autor marca as aprovadas.</p>
    <div class="abas" id="cats"><button data-cat="" class="${!categoria ? 'ativo' : ''}">Todas</button>${Object.entries(CATEGORIAS).map(([k, r]) => `<button data-cat="${k}" class="${categoria === k ? 'ativo' : ''}">${r}</button>`).join('')}</div>
    <div id="ideias"><div class="girando"></div></div>`;
  const carregar = async () => {
    try {
      const r = await api('/ideias' + (categoria ? '?categoria=' + categoria : ''));
      const votei = new Set(r.votei);
      $('#ideias').innerHTML = r.ideias.map((i) => `<article class="post ideia"><div class="ideia-linha"><button class="voto ${votei.has(i.id) ? 'on' : ''}" data-votar="${i.id}" aria-label="Votar">▲<span>${i.votos}</span></button><div>
        <div class="cab"><span class="etiqueta">${esc(CATEGORIAS[i.categoria] ?? i.categoria)}</span><span class="etiqueta estado-${i.estado}">${esc(ESTADOS_ID[i.estado] ?? i.estado)}</span><b>${esc(i.autor)}</b><span>${dataCurta(i.criado)}</span></div>
        <h3>${esc(i.titulo)}</h3><div class="corpo">${esc(i.texto)}</div>${i.resposta ? `<div class="resposta-autor"><b>Autor:</b> ${esc(i.resposta)}</div>` : ''}
        ${sessao.usuario?.admin ? `<div class="linha-botoes"><button class="botao peq primario" data-decidir-ideia="${i.id}">Decidir (autor)</button></div>` : ''}</div></div></article>`).join('') || '<div class="painel vazio">Nenhuma ideia nesta categoria ainda.</div>';
    } catch (err) { $('#ideias').innerHTML = `<div class="painel vazio">${esc(err.message)}</div>`; }
  };
  tela.onclick = async (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    if (b.dataset.cat !== undefined) { categoria = b.dataset.cat; for (const x of $$('#cats button')) x.classList.toggle('ativo', x === b); history.replaceState(null, '', '#/ideias' + (categoria ? '/' + categoria : '')); return carregar(); }
    if (b.dataset.votar) { const u = await ctx.exigirConta('Entre para votar.'); if (!u) return; await api(`/ideias/${b.dataset.votar}/votar`, { corpo: {} }); return carregar(); }
    if (b.dataset.decidirIdeia) return decidir(`/ideias/${b.dataset.decidirIdeia}/estado`, ESTADOS_ID, carregar);
    if (b.matches('[data-nova]')) {
      const u = await ctx.exigirConta('Entre para propor uma ideia.');
      if (!u) return;
      const j = janela(`<h2>Nova ideia</h2><form><label class="campo">Categoria<select name="categoria">${Object.entries(CATEGORIAS).map(([k, r]) => `<option value="${k}" ${k === categoria ? 'selected' : ''}>${r}</option>`).join('')}</select></label>
        <label class="campo">Título<input name="titulo" type="text" maxlength="120" required></label>
        <label class="campo">Descreva (o que é, como funciona, como seria a arte)<textarea name="texto" maxlength="6000" required></textarea></label><button class="botao primario">Enviar ideia</button></form>`);
      $('form', j.el).onsubmit = async (e2) => { e2.preventDefault(); try { await api('/ideias', { corpo: Object.fromEntries(new FormData(e2.target)) }); j.fechar(); avisar('Ideia enviada!', 'ok'); carregar(); } catch (err) { avisar(err.message, 'erro'); } };
    }
  };
  carregar();
}
