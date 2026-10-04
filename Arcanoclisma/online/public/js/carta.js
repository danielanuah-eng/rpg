// Desenho das cartas: o renderizador do kit visual do Codex (Arcanoclisma/arena/arena.js, função card),
// com os caminhos apontando para /kit/ e o modelo montado a partir das cartas reais do motor.
// Só recebe o modelo autorizado: carta escondida vira verso, sem nome, arte ou atributos no DOM.
import { DADOS, VALOR, conj, arteConj, textoPoder, EDICOES } from './base.js';
import { aplicarIdentidadeValor } from './identidade-valor-v12.js';
/** Arte trocada pelo autor no Editor vale mais que a arte final do GPT. */
const EDICOES_ARTE = new Proxy({}, { get: (_, k) => EDICOES.dados?.artes?.[k] ?? {} });
/** Raridade definida pelo autor no Editor (sem raridade automática; pedido do autor, 03/10). */
const EDICOES_RARIDADE = new Proxy({}, { get: (_, k) => EDICOES.dados?.raridades?.[k] ?? {} });
/** Cores dos Valores em faixas: 1 Valor = cor cheia; 2 ou mais = divisões iguais (como a esfera do Conjurador). */
const faixas = (cores, angulo = 90) => cores.length === 1 ? cores[0] : `linear-gradient(${angulo}deg, ${cores.map((c, i) => `${c} ${(i * 100) / cores.length}% ${((i + 1) * 100) / cores.length}%`).join(', ')})`;

const K = '/kit/';
const el = (tag, className, text) => { const n = document.createElement(tag); if (className) n.className = className; if (text != null) n.textContent = String(text); return n; };
const img = (src, cls, alt = '') => { const n = el('img', cls); n.src = src; n.alt = alt; n.draggable = false; n.loading = 'lazy'; n.decoding = 'async'; return n; };
const names = { aliado: 'Aliado', temporaria: 'Temporária', mistica: 'Mística', estrutura: 'Estrutura', conjurador: 'Conjurador', item: 'Item', canal: 'Canal', armadura: 'Armadura', extra: 'Extra' };
const modelKey = (m) => m.categoriaItem || m.tipo;
const colors = { 'Conflitüo': '#9a4633', Sublimeyn: '#748e9d', Dominum: '#62416a', Persona: '#b76673', Alvorum: '#927638', Morbidyum: '#6b866d', Cognitium: '#505b94', Eternyr: '#304565', Primitivum: '#65743b' };
const valorSlug = (v) => String(v || '').normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
const valorName = (v) => Object.keys(colors).find((name) => valorSlug(name) === valorSlug(v)) || v;
const count = (v) => Math.max(0, Math.trunc(Number(v) || 0));
/** Cenário provisório atrás da arte recortada (até chegar a arte com fundo do GPT): o lugar do primeiro Valor da carta. */
/** Varia o cenário por carta (escolha fixa pelo id): qual Valor, enquadramento e hora do dia, com mais dia que noite. */
const hashDe = (s) => { let h = 2166136261; for (const ch of String(s)) h = Math.imul(h ^ ch.charCodeAt(0), 16777619); return h >>> 0; };
const HORAS = [ // [hora, luz (mistura "screen" sobre o cenário), peso]
  ['amanhecer', '#ffb98a70', 2], ['manhã', '#d8ecff78', 3], ['meio-dia', '#fff3d48a', 3], ['tarde', '#ffdf9c70', 3], ['entardecer', '#ff9d7a60', 2], ['noite', '#7586c840', 1],
];
const NEUTROS = ['saguao', 'baralhos', 'menu', 'tutorial'];
function cenaDe(id, valores, padrao = 'saguao') {
  const h = hashDe(id);
  const chave = valores?.length ? DADOS.cenarios.valores[valores[h % valores.length]] : DADOS.cenarios[padrao === 'saguao' ? NEUTROS[h % NEUTROS.length] : padrao] ?? DADOS.cenarios[padrao];
  let r = (h >>> 8) % HORAS.reduce((s, x) => s + x[2], 0), hora = HORAS[0];
  for (const x of HORAS) { if (r < x[2]) { hora = x; break; } r -= x[2]; }
  return { cena: `/arte/cenarios/mini/${chave}.webp`, cenaLuz: hora[1], cenaHora: hora[0], cenaPos: `${20 + ((h >>> 4) % 61)}% ${15 + ((h >>> 12) % 56)}%` };
}

/** Modelo público de uma carta do catálogo (CartaDef + identidade visual). */
export function modeloCarta(defId) {
  const d = DADOS.catalogo.cartas[defId];
  if (!d || d.tipo === 'oculta') return null;
  const v = DADOS.visual[defId] ?? {};
  return {
    id: defId, nome: d.nome, tipo: d.tipo, valores: (d.valores ?? []).map((x) => VALOR[x] ?? x), custo: d.custo, grau: d.grau, arcanoGerado: d.arcanoGerado,
    ataque: d.tipo === 'aliado' ? d.ataque ?? 0 : d.ataque, vida: d.vida, texto: d.texto, subtipos: d.subtipos,
    categoriaItem: d.categoriaItem, defesa: d.defesa, vidaDesgaste: d.vidaDesgaste, velocidade: d.velocidade,
    // arte final do GPT (cena inteira, aprovada por ele) quando existe; senão, a arte do RPG recortada sobre um cenário
    ...(DADOS.artesFinais?.[defId] && !(EDICOES_ARTE.cartas?.[defId])
      ? { arte: `/arte/finais/${defId}.webp`, arteModo: 'cena' }
      : { arte: v.arte ? `/arte/${v.arte}.webp` : `${K}raster/ornamento-arcano.webp`, arteModo: 'recorte', ...cenaDe(defId, d.valores, d.tipo === 'item' ? 'baralhos' : 'saguao') }),
    cosmetico: { raridade: EDICOES_RARIDADE.cartas?.[defId] ?? null },
  };
}
export function modeloConjurador(conjId) {
  const c = conj(conjId);
  return {
    id: conjId, nome: c.nome, tipo: 'conjurador', valores: c.valores.map((x) => VALOR[x] ?? x), comprometimento: c.comprometimento,
    texto: c.habilidade || textoPoder(c.poder), vida: c.vida, ...(DADOS.artesFinais?.['conjurador-' + conjId] && !(EDICOES_ARTE.conjuradores?.[conjId])
      ? { arte: `/arte/finais/conjurador-${conjId}.webp`, arteModo: 'cena' }
      : { arte: arteConj(conjId), arteModo: 'recorte', ...cenaDe(conjId, c.valores) }), cosmetico: { raridade: EDICOES_RARIDADE.conjuradores?.[conjId] ?? null },
  };
}

export function desenharCarta(model, { hidden = false, opponent = false, readable = false, width = 150, rarity } = {}) {
  const n = el('article', 'tcg-card' + (hidden ? ' is-hidden' : '') + (opponent ? ' facing-opponent' : '') + (readable ? ' revealed-readable' : ''));
  n.style.setProperty('--card-w', width + 'px');
  if (hidden || !model) {
    n.classList.add('is-hidden');
    n.setAttribute('aria-label', 'Carta oculta');
    const back = el('div', 'back'); back.append(img(`${K}raster/verso-arcano-v3.webp`, 'back-art'), img(`${K}vetores/verso-titulo.svg`, 'back-title')); n.append(back); return n;
  }
  n.dataset.rarity = rarity || model.cosmetico?.raridade || 'nenhuma';
  n.dataset.type = model.tipo;
  const alternative = model.cosmetico?.arteAlternativa === true;
  n.dataset.artVariant = alternative ? 'alternativa' : 'padrao';
  n.dataset.finish = alternative ? 'alternativa' : n.dataset.rarity;
  if (alternative) n.classList.add('scene-art', 'is-alt-art');
  if (model.arteModo === 'cena') n.classList.add('scene-art');
  if (model.artePosicao) n.style.setProperty('--art-position', model.artePosicao);
  n.setAttribute('aria-label', `${model.nome}, ${names[model.tipo]}`);
  const front = el('div', 'front');
  if (model.cena) {
    front.classList.add('com-cena');
    front.style.setProperty('--fundo-cena', `url('${model.cena}')`);
    if (model.cenaLuz) front.style.setProperty('--cena-luz', model.cenaLuz);
    if (model.cenaPos) front.style.setProperty('--cena-pos', model.cenaPos);
    if (model.cenaHora) front.dataset.hora = model.cenaHora;
  }
  const frameType = modelKey(model);
  front.append(img(model.arte, 'portrait'));
  if (['lendaria', 'alternativa'].includes(n.dataset.finish)) { front.append(img(model.arte, 'portrait art-magic-glow-v9')); if (alternative) front.append(el('div', 'art-depth-v9')); }
  const moldura = `${K}templates/${frameType}-${model.tipo === 'conjurador' ? 'v8' : 'v4'}.svg`;
  const coresValor = (model.valores || []).map((v) => colors[valorName(v)]).filter(Boolean);
  const tinta = el('div', 'frame-tinta' + (coresValor.length ? '' : ' sem-valor'));
  tinta.style.setProperty('--moldura', `url('${moldura}')`);
  tinta.style.background = coresValor.length ? faixas(coresValor, 180) : '#8d9399';
  n.dataset.valores = coresValor.length;
  front.append(img(moldura, 'frame'), tinta, el('div', 'name', model.nome), img(`${K}vetores/icone-${frameType}.svg`, 'type-glyph', names[frameType]));
  if (model.tipo === 'conjurador') {
    const pair = [...new Set((model.valores || []).map(valorName))].slice(0, 2);
    const sphere = el('div', 'conjurador-values'); sphere.setAttribute('role', 'img'); sphere.setAttribute('aria-label', `Valores do conjurador: ${pair.join(' e ')}`); sphere.title = sphere.getAttribute('aria-label');
    for (let i = 0; i < 2; i++) {
      const value = pair[i], half = el('span', 'conjurador-value-half'); half.style.setProperty('--valor-color', colors[value] || '#31485b');
      if (value && colors[value]) half.append(img(`${K}vetores/valor-${valorSlug(value)}-simbolo-v8.svg`, '', ''));
      else half.append(el('span', 'value-placeholder', '?'));
      half.title = value || ''; sphere.append(half);
    }
    sphere.append(img(`${K}vetores/esfera-conjurador-v8.svg`, 'conjurador-sphere-rim', '')); front.append(sphere);
  }
  if (model.tipo !== 'conjurador') {
    const generated = count(model.arcanoGerado), cost = count(model.custo), degree = count(model.grau);
    if (model.tipo !== 'item') {
      const g = el('div', 'badge generated', generated); g.title = `Gera ${generated} Arcano ao ser enviada ao Arcano`; front.append(g);
      g.style.backgroundColor = colors[model.valores?.[0]] || '#325e72';
      const c = el('div', 'badge cost esfera-custo', cost); c.title = `Custo: ${cost} Arcano · ${(model.valores || []).join(' + ') || 'sem Valor'}`;
      c.style.background = coresValor.length ? faixas(coresValor) : '#6f757c';
      c.append(img(`${K}vetores/esfera-conjurador-v8.svg`, 'esfera-custo-aro', ''));
      const palette = (model.valores || []).map((v) => colors[v] || '#849eae');
      if (false && palette.length) { const rim = el('i', 'valor-rim'); rim.style.background = `conic-gradient(${palette.map((v, i) => `${v} ${i * 360 / palette.length}deg ${(i + 1) * 360 / palette.length}deg`).join(',')})`; c.append(rim); }
      const values = el('div', 'value-symbols'); for (const value of model.valores || []) { const slug = valorSlug(value); if (Object.keys(colors).map(valorSlug).includes(slug)) { const glyph = img(`${K}vetores/valor-${slug}.svg`, '', value); glyph.title = value; values.append(glyph); } } if (values.childElementCount) c.append(values);
      front.append(c);
      if (degree > 0) { const d = el('div', 'degree', degree); d.title = `Grau necessário: ${degree}`; front.append(d); }
    }
  }
  const subtypes = [...(model.subtipos || [])].filter((s) => !(model.valores || []).includes(s));
  const typeLabel = model.tipo === 'item' ? `Item — ${names[model.categoriaItem] ?? 'Equipamento'}` : names[model.tipo] || model.tipo;
  const speed = model.velocidade === 'repentina' ? 'Repentina' : model.tipo === 'temporaria' ? 'Findante' : '';
  const classification = '';
  const typeText = model.tipo === 'conjurador' ? [typeLabel, model.comprometimento].join(' · ') : [typeLabel, speed, classification].filter(Boolean).join(' · ');
  const detail = el('div', model.tipo === 'conjurador' ? 'subtype-line conjurador-detail' : 'subtype-line', [...new Set(subtypes)].join(' · '));
  front.append(el('div', 'type-line', typeText), detail, el('div', 'rules', model.texto), el('div', 'flavour', ''));
  if (n.dataset.rarity !== 'nenhuma') front.append(img(`${K}vetores/raridade-${n.dataset.rarity}.svg`, 'rarity', `Raridade: ${n.dataset.rarity}`));
  const fields = model.categoriaItem === 'armadura' ? [['defesa', 'defesa'], ['vidaDesgaste', 'desgaste']] : [['ataque', 'ataque'], ['vida', 'vida']];
  if (fields.some(([key]) => model[key] != null)) { const stats = el('div', 'stats'); for (const [key, symbol] of fields) if (model[key] != null) { const stat = el('span', 'stat stat-' + key); stat.title = { ataque: 'Ataque', vida: 'Vida', defesa: 'Defesa', vidaDesgaste: 'Vida de desgaste' }[key]; stat.append(img(`${K}vetores/icone-${symbol}-v5.svg`, '', stat.title), el('span', '', model[key])); stats.append(stat); } front.append(stats); }
  if (['incomum', 'rara', 'lendaria', 'alternativa'].includes(n.dataset.finish)) { const foil = el('div', 'foil-texture'); foil.setAttribute('aria-hidden', 'true'); front.append(foil); const reflection = el('div', 'foil-reflection'); reflection.setAttribute('aria-hidden', 'true'); front.append(reflection); }
  n.append(front);
  return aplicarIdentidadeValor(n, model);
}

/** HTML de uma carta do catálogo (para listas em innerHTML). */
export function cartaHTML(defId, opcoes = {}) {
  const m = defId === 'conj' ? null : modeloCarta(defId);
  return desenharCarta(m, opcoes).outerHTML;
}
export const conjuradorHTML = (conjId, opcoes = {}) => desenharCarta(modeloConjurador(conjId), opcoes).outerHTML;
