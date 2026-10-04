// Aba Lore (pedido do autor, 04/10): o mundo do Arcanoclisma. Dados gerados no build (online/lore.ts → jogo/lore.json):
// os 9 Valores, os Conjuradores (do catálogo do jogo), o atlas do mundo (com as artes aprovadas) e as novidades do jogo (git).
import { $, esc, DADOS, VALOR, cenario, janela } from './base.js';
import { desenharCarta, modeloConjurador } from './carta.js';

const ABAS = [['mundo', 'O mundo e os Valores'], ['conjuradores', 'Conjuradores'], ['atlas', 'Atlas'], ['novidades', 'Novidades']];
let cache = null;

export async function telaLore(tela, partes, ctx) {
  ctx.fundo(cenario('saguao'));
  let aba = ABAS.some(([k]) => k === partes[0]) ? partes[0] : 'mundo';
  tela.innerHTML = '<div class="girando"></div>';
  cache ??= await fetch('/jogo/lore.json?v=' + DADOS.versao).then((r) => r.json()).catch(() => ({ valores: [], categorias: [], novidades: [] }));
  const L = cache;
  const filtro = { cat: '', busca: '' };

  const corpo = () => {
    if (aba === 'mundo') return `<p class="lema-peq">No Arcanoclisma, a magia vem de <b>nove Valores</b>. Cada Conjurador domina dois deles, e o baralho dele só usa cartas dessas cores.</p>
      <div class="lore-valores">${L.valores.map((v) => `<article class="lore-valor" style="--cor:${esc(corVal(v.nome))}"><h3>${esc(v.nome)}</h3><p class="peq"><b>Cor:</b> ${esc(v.cor)} · <b>Magia:</b> ${esc(v.forma)}</p><p>${esc(v.ideia)}</p></article>`).join('')}</div>`;
    if (aba === 'conjuradores') return `<div class="lore-conjs">${Object.values(DADOS.catalogo.conjuradores).map((c) => `<article class="lore-conj" data-conj="${esc(c.id)}">
        <div class="lore-conj-carta">${desenharCarta(modeloConjurador(c.id), { width: 170 }).outerHTML}</div>
        <div><h3>${esc(c.nome)}</h3><p class="peq">${c.valores.map((v) => esc(VALOR[v] ?? v)).join(' · ')} · ${esc(c.comprometimento ?? '')}</p><p>${esc(c.habilidade ?? '')}</p></div></article>`).join('')}</div>`;
    if (aba === 'novidades') return `<p class="lema-peq">As últimas mudanças do jogo, direto do histórico do projeto.</p>
      <ol class="lore-novidades">${L.novidades.map((n) => `<li><span class="peq">${esc(n.data)}</span> ${esc(n.texto)}</li>`).join('') || '<li>Nada ainda.</li>'}</ol>`;
    // atlas
    const busca = filtro.busca.toLowerCase();
    const cats = L.categorias.filter((c) => !filtro.cat || c.titulo === filtro.cat);
    const blocos = cats.map((c) => {
      const itens = c.itens.filter((i) => !busca || (i.nome + ' ' + i.texto).toLowerCase().includes(busca));
      return itens.length ? `<h2 class="lore-cat">${esc(c.titulo)} <span class="peq">(${itens.length})</span></h2><div class="lore-atlas">${itens.map((i) => `<button class="lore-verbete" data-verbete="${esc(i.id)}"><img loading="lazy" src="/arte/${esc(i.id)}.webp" alt=""><b>${esc(i.nome)}</b><span>${esc(i.texto)}</span></button>`).join('')}</div>` : '';
    }).join('');
    return blocos || '<div class="painel vazio">Nada encontrado.</div>';
  };
  const filtrosAtlas = () => `<div class="filtros lore-filtros"><select id="lore-cat"><option value="">Todas as categorias</option>${L.categorias.map((c) => `<option ${c.titulo === filtro.cat ? 'selected' : ''}>${esc(c.titulo)}</option>`).join('')}</select><input id="lore-busca" type="search" placeholder="Buscar no atlas (ex.: dragão, Permal, Morbidyum)" value="${esc(filtro.busca)}"></div>`;
  const desenhar = () => {
    tela.innerHTML = `<div class="titulo-secao"><h1>Lore</h1></div>
      <div class="abas lore-abas">${ABAS.map(([k, r]) => `<button data-aba="${k}" class="${aba === k ? 'ativo' : ''}">${r}</button>`).join('')}</div>
      ${aba === 'atlas' ? filtrosAtlas() : ''}<div id="lore-corpo">${corpo()}</div>`;
  };
  desenhar();
  tela.onclick = (ev) => {
    const a = ev.target.closest('[data-aba]');
    if (a) { aba = a.dataset.aba; history.replaceState(null, '', '#/lore/' + aba); return desenhar(); }
    const v = ev.target.closest('[data-verbete]');
    if (v) {
      const it = L.categorias.flatMap((c) => c.itens).find((i) => i.id === v.dataset.verbete);
      if (it) janela(`<div class="lore-zoom"><img src="/arte/${esc(it.id)}.webp" alt=""><div><h2>${esc(it.nome)}</h2><p>${esc(it.texto)}</p></div></div>`, { classe: 'larga' });
    }
  };
  tela.oninput = (ev) => { if (ev.target.id === 'lore-busca') { filtro.busca = ev.target.value; $('#lore-corpo').innerHTML = corpo(); } };
  tela.onchange = (ev) => { if (ev.target.id === 'lore-cat') { filtro.cat = ev.target.value; $('#lore-corpo').innerHTML = corpo(); } };
}

const CORES = { 'Conflitüo': '#b5452e', 'Sublimeyn': '#b9c3cc', 'Dominum': '#6e1f45', 'Persona': '#ef7f6b', 'Alvorum': '#d6aa3a', 'Morbidyum': '#9cc39a', 'Cognitium': '#4b3f9e', 'Eternyr': '#1f2d5c', 'Primitivum': '#5d7d34' };
const corVal = (n) => CORES[n] ?? '#7b6f9c';
