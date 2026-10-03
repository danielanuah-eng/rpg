// Editor de cartas e mecânicas do Arcanoclisma: edita cartas, Conjuradores, regras, curva de Grau, régua de pontos e
// baralhos. O que você salva vai para data/edicoes.json (pelo servidor local), é conferido (baralhos legais, partidas de
// teste) e passa a valer nas novas partidas do app de testes e nas simulações.
import { avaliar, bonusGrau, grauPadrao } from './dist/balanco.js';
import { VALOR, TIPO, frase, textoCarta, textoPoder, valoresTxt } from './textos.js';

const $ = (s) => document.querySelector(s);
const esc = (t) => String(t ?? '').replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
const clone = (x) => (x === undefined ? undefined : structuredClone(x));
const igual = (a, b) => JSON.stringify(a) === JSON.stringify(b);

// ---------- estado ----------
let EST = null;   // estado do servidor: edições salvas + jogo gerado (original)
let ed = null;    // cópia de trabalho das edições
let salvo = '';   // edições salvas, para saber se há mudança pendente
let aba = 'cartas';
let msg = null;   // { tipo: 'ok' | 'aviso', html }
const sel = { carta: null, conj: null, baralho: null };
let atual = null; // carta ou Conjurador em edição (cópia completa)
const filtro = { busca: '', tipo: '', valor: '', uso: '', editadas: false };
let ultimaSim = null;
let timerSim = null;

const chave = (x) => JSON.stringify({ ...x, historico: undefined });
const pendente = () => ed && chave(ed) !== salvo;

async function carregar() {
  const r = await fetch('/editor/estado', { cache: 'no-store' });
  if (!r.ok) throw new Error('o servidor respondeu ' + r.status);
  EST = await r.json();
  ed = clone(EST.edicoes);
  salvo = chave(ed);
  ultimaSim = EST.simulacao;
  atual = null;
}

// ---------- cartas, Conjuradores e baralhos com as edições ----------
function mesclar(base, parcial) {
  const r = { ...base };
  for (const [k, v] of Object.entries(parcial)) { if (v === null) delete r[k]; else if (v !== undefined) r[k] = clone(v); }
  return r;
}
function diferenca(base, c) {
  const d = {};
  for (const k of new Set([...Object.keys(base), ...Object.keys(c)])) {
    if (k === 'id') continue;
    if (!igual(base[k], c[k])) d[k] = c[k] === undefined ? null : clone(c[k]);
  }
  return d;
}
function cartaDe(id, E = ed) {
  const base = EST.gerado.cartas[id];
  const p = E.cartas[id];
  if (!p) return base ? clone(base) : null;
  const { nova, ...campos } = p;
  if (!base && !nova) return null;
  return { ...mesclar(base ?? {}, campos), id };
}
function gravarCarta(c) {
  const base = EST.gerado.cartas[c.id];
  if (!base) { ed.cartas[c.id] = { ...clone(c), nova: true }; return; }
  const d = diferenca(base, c);
  if (Object.keys(d).length) ed.cartas[c.id] = d; else delete ed.cartas[c.id];
}
const idsCartas = (E = ed) => [...new Set([...Object.keys(EST.gerado.cartas), ...Object.keys(E.cartas).filter((id) => E.cartas[id].nova)])];
const editada = (id) => !!ed.cartas[id];

const idsConj = () => Object.keys(EST.gerado.conjuradores).filter((id) => id !== 'padrao');
function conjDe(id, E = ed) { const base = EST.gerado.conjuradores[id]; const p = E.conjuradores[id]; return p ? { ...mesclar(base, p), id } : clone(base); }
function gravarConj(c) {
  const d = diferenca(EST.gerado.conjuradores[c.id], c);
  if (Object.keys(d).length) ed.conjuradores[c.id] = d; else delete ed.conjuradores[c.id];
}

const listaDe = (conj, E = ed) => E.baralhos[conj]?.lista ?? EST.baralhosOriginais[conj].lista;
const itensDe = (conj, E = ed) => E.baralhos[conj]?.itens ?? EST.baralhosOriginais[conj].itens ?? [];
function gravarItens(conj, itens) {
  const b = { ...(ed.baralhos[conj] ?? {}) };
  if (igual(itens, EST.baralhosOriginais[conj].itens ?? [])) delete b.itens; else b.itens = itens;
  if (Object.keys(b).length) ed.baralhos[conj] = b; else delete ed.baralhos[conj];
}
const estiloDe = (conj, E = ed) => ({ ...EST.baralhosOriginais[conj].estilo, ...(E.baralhos[conj]?.estilo ?? {}) });
function gravarLista(conj, lista) {
  const b = { ...(ed.baralhos[conj] ?? {}) };
  if (igual(lista, EST.baralhosOriginais[conj].lista)) delete b.lista; else b.lista = lista;
  if (Object.keys(b).length) ed.baralhos[conj] = b; else delete ed.baralhos[conj];
}
function gravarEstilo(conj, campo, valor) {
  const b = { ...(ed.baralhos[conj] ?? {}) };
  const est = { ...(b.estilo ?? {}) };
  if (igual(valor, EST.baralhosOriginais[conj].estilo[campo])) delete est[campo]; else est[campo] = valor;
  if (Object.keys(est).length) b.estilo = est; else delete b.estilo;
  if (Object.keys(b).length) ed.baralhos[conj] = b; else delete ed.baralhos[conj];
}
/** id → [[Conjurador, cópias]] */
function usos() {
  const m = {};
  for (const c of idsConj()) {
    for (const [id, n] of listaDe(c)) if (n > 0) (m[id] ??= []).push([c, n]);
    for (const id of itensDe(c)) (m[id] ??= []).push([c, 1]);
  }
  return m;
}
const legal = (carta, conj) => !carta.valores?.length || carta.valores.some((v) => conj.valores.includes(v));

// ---------- régua de pontos ----------
const precos = () => ({ ...EST.precosOriginais, ...ed.precos });
const curva = () => ed.curvaGrau ?? EST.curvaOriginal;
function avaliacao(c) {
  if (c.tipo === 'item') return null;
  let a;
  try { a = avaliar(c, precos()); } catch { return null; }
  if (!Number.isFinite(a.pontos)) return null;
  let bonus = 0;
  if (c.tipo === 'aliado' && !c.ativacao) bonus = Math.floor(bonusGrau(EST.curvaSemDobro ? Math.min(c.grau, grauPadrao(c.custo)) : c.grau, curva()));
  return { pontos: a.pontos, orcamento: a.orcamento + bonus, bonus, folga: a.orcamento + bonus - a.pontos };
}
const fmt1 = (x) => (Math.round(x * 10) / 10).toString().replace('.', ',');

// ---------- descrição das mudanças (vai para o histórico) ----------
function valorTxt(v) {
  if (v === undefined || v === null) return '—';
  if (Array.isArray(v) && v.every((x) => x && typeof x === 'object' && 'tipo' in x)) return v.length ? `"${frase(v)}"` : 'nenhum';
  if (v && typeof v === 'object' && Array.isArray(v.efeitos)) return `"${textoCarta({ ativacao: v })}"`;
  if (typeof v === 'object') return JSON.stringify(v);
  return String(v);
}
function difCampos(a, b) {
  return [...new Set([...Object.keys(a), ...Object.keys(b)])].filter((k) => k !== 'id' && !igual(a[k], b[k])).map((k) => `${k}: ${valorTxt(a[k])} → ${valorTxt(b[k])}`);
}
function resumoMudancas() {
  const S = normal(EST.edicoes);
  const out = [];
  for (const id of new Set([...Object.keys(S.cartas), ...Object.keys(ed.cartas)])) {
    if (igual(S.cartas[id], ed.cartas[id])) continue;
    const a = cartaDe(id, S), b = cartaDe(id, ed);
    if (!a && b) out.push(`Carta nova: ${b.nome} (${TIPO[b.tipo]}, custo ${b.custo}, Grau ${b.grau})`);
    else if (a && !b) out.push(`Carta excluída: ${a.nome}`);
    else if (a && b) out.push(`Carta ${a.nome}: ${difCampos(a, b).join('; ')}`);
  }
  for (const id of new Set([...Object.keys(S.conjuradores), ...Object.keys(ed.conjuradores)])) {
    if (igual(S.conjuradores[id], ed.conjuradores[id])) continue;
    const a = conjDe(id, S), b = conjDe(id, ed);
    out.push(`Conjurador ${a.nome}: ${difCampos(a, b).map((x) => (x.startsWith('poder:') ? `mecânica da habilidade: "${textoPoder(a.poder) || '—'}" → "${textoPoder(b.poder) || '—'}"` : x)).join('; ')}`);
  }
  const rA = { ...EST.regrasOriginais, ...S.regras }, rB = { ...EST.regrasOriginais, ...ed.regras };
  for (const k of Object.keys(rB)) if (!igual(rA[k], rB[k])) out.push(`Regra ${REGRAS[k]?.[0] ?? k}: ${valorTxt(rA[k])} → ${valorTxt(rB[k])}`);
  const cA = S.curvaGrau ?? EST.curvaOriginal;
  if (cA !== curva()) out.push(`Curva de Grau (k): ${cA} → ${curva()}`);
  const pA = { ...EST.precosOriginais, ...S.precos }, pB = precos();
  for (const k of Object.keys(pB)) if (pA[k] !== pB[k]) out.push(`Régua ${PRECOS[k] ?? k}: ${pA[k]} → ${pB[k]}`);
  for (const c of idsConj()) {
    const la = new Map(listaDe(c, S)), lb = new Map(listaDe(c, ed));
    const d = [...new Set([...la.keys(), ...lb.keys()])].map((id) => [id, (lb.get(id) ?? 0) - (la.get(id) ?? 0)]).filter(([, n]) => n !== 0)
      .map(([id, n]) => `${n > 0 ? '+' : '−'}${Math.abs(n)} ${(cartaDe(id) ?? cartaDe(id, S))?.nome ?? id}`);
    if (d.length) out.push(`Baralho de ${conjDe(c).nome}: ${d.join(', ')}`);
    const ia = itensDe(c, S), ib = itensDe(c, ed);
    if (!igual(ia, ib)) out.push(`Itens de ${conjDe(c).nome}: ${ia.map((id) => cartaDe(id, S)?.nome ?? id).join(', ') || 'nenhum'} → ${ib.map((id) => cartaDe(id)?.nome ?? id).join(', ') || 'nenhum'}`);
    const ea = estiloDe(c, S), eb = estiloDe(c, ed);
    const de = Object.keys(eb).filter((k) => !igual(ea[k], eb[k])).map((k) => `${ESTILO[k]?.[0] ?? k} ${valorTxt(ea[k])} → ${valorTxt(eb[k])}`);
    if (de.length) out.push(`Estilo do jogador automático com ${conjDe(c).nome}: ${de.join('; ')}`);
  }
  return out;
}
const normal = (x) => ({ cartas: {}, conjuradores: {}, regras: {}, precos: {}, baralhos: {}, historico: [], ...(x ?? {}) });

// ---------- textos de apoio ----------
const REGRAS = {
  limiteMao: ['Limite de mão', 'int', 'Máximo de cartas na mão no fim do turno; o excesso é descartado.'],
  impasseTurnos: ['Relógio de impasse (turnos)', 'int', 'Perde quem passa este número de turnos seguidos sem jogar carta, ativar ou atacar. 0 desliga.'],
  primeiroSemGrauNoTurno1: ['Quem começa não põe Grau no turno 1', 'bool', ''],
  grauSemSuspender: ['Pôr carta no Grau não suspende o Conjurador', 'bool', 'Uma vez por turno; o Conjurador ainda pode atacar.'],
  primeiroConjSemAtacarTurno1: ['O Conjurador de quem começa não ataca no turno 1', 'bool', ''],
  segundoCartaExtra: ['Cartas a mais para o segundo jogador', 'int', 'Compradas na preparação.'],
  segundoArcanoInicial: ['Arcano inicial do segundo jogador', 'int', ''],
  semAcaoPerde: ['Sem baralho, mão e ações: perde', 'bool', 'No início da Principal.'],
  reporMaoNoInicio: ['Repor a mão no Início do turno', 'bool', 'Compra até ficar com a mão inicial do Conjurador; o Final deixa de comprar.'],
  conjuradorLevantaNoInicio: ['Conjurador levanta no Início do turno', 'bool', 'Mesmo que tenha bloqueado no turno do oponente. Aliados continuam levantando só no Final.'],
  conjuradorSoAtacaComArma: ['Conjurador só ataca com arma', 'bool', 'Precisa de um Canal com Ataque; o ataque dele é o do Canal (+ dano básico).'],
};
const PRECOS = {
  ataque: 'Cada ponto de Ataque', vida: 'Cada ponto de Vida', danoAlvo: 'Dano a um alvo (por ponto)', danoConjurador: 'Dano ao Conjurador (por ponto)',
  curar: 'Cura (por ponto)', destruir: 'Destruir um alvo', comprar: 'Comprar 1 carta', comprarEternyr: 'Comprar 1 carta (Eternyr)', arcano: 'Ganhar 1 Arcano',
  arcanoOponente: 'Dar 1 Arcano ao oponente (abate)', repentina: 'Velocidade Repentina', proficiente: 'Proficiente', etereo: 'Etéreo',
  descartaGrau: 'Cada carta descartada do Grau (libera)', custoValor: 'Cada Valor exigido no Grau (libera)', pitchExtra: 'Cada Arcano a mais como pitch (abate do orçamento)',
  ativacaoArcano: 'Multiplicador: ativação que gera Arcano', ativacaoOutro: 'Multiplicador: outras ativações', ativacaoCusto: 'Cada Arcano de custo da ativação (abate)',
  ativacaoArcanoAlto: 'Multiplicador: ativação de Arcano em carta de custo 5+', grauExtra: 'Cada ponto de "conta como carta no Grau"',
  danoAliadoEternyr: 'Eternyr: dano a Aliado (por ponto)', danoConjEternyr: 'Eternyr: dano ao Conjurador (por ponto)', grauAltoEternyr: 'Eternyr: cada Grau acima do padrão',
  devolver: 'Devolver à mão', anular: 'Anular uma carta', danoTodos: 'Dano em cada Aliado do oponente (por ponto)', grauMedioEscala: 'Grau médio para dano que cresce com o Grau',
  danoTodosAmbos: 'Dano em cada Aliado, os seus também (por ponto)', destruirTodos: 'Destruir todos os Aliados do oponente', destruirTodosAmbos: 'Destruir todos os Aliados (os seus também)',
  protegido: 'Protegido (por ponto)', aoDanoConjurador: 'Multiplicador: gatilho ao causar dano ao Conjurador',
};
const ESTILO = {
  agressividade: ['Agressividade (0 a 1)', 'num', 'Quanto do campo o jogador automático manda atacar.'],
  grauAlvo: ['Grau desejado', 'int', 'Quantas cartas ele quer no Grau antes de guardar cartas na mão.'],
  reservaInstantes: ['Guarda Arcano para Repentinas', 'bool', 'Segura Grau levantado para jogar no turno do oponente.'],
  queimaFace: ['Dano direto no Conjurador cedo', 'bool', 'Joga dano no Conjurador mesmo longe de matar.'],
};
const ALVOS_TODOS = ['escolhido', 'escolhidoAliado', 'conjuradorInimigo', 'conjuradorProprio'];
const ALVO_TXT = { escolhido: 'um alvo qualquer', escolhidoAliado: 'Aliado ou Mística', escolhidoProprio: 'Aliado ou Mística seu', conjuradorInimigo: 'Conjurador inimigo', conjuradorProprio: 'seu Conjurador', pilha: 'carta na pilha' };
/** Tipos de efeito: rótulo, campos (num | bool | lista de opções) e valores iniciais. */
const EFEITOS = {
  dano: ['Dano', { valor: 'num', alvo: ALVOS_TODOS, escalaGrau: 'bool' }, { valor: 2, alvo: 'escolhido' }],
  curar: ['Curar', { valor: 'num', alvo: ALVOS_TODOS }, { valor: 2, alvo: 'conjuradorProprio' }],
  destruir: ['Destruir', { alvo: ['escolhido', 'escolhidoAliado'] }, { alvo: 'escolhidoAliado' }],
  devolver: ['Devolver à mão', { alvo: ['escolhido', 'escolhidoAliado', 'escolhidoProprio'] }, { alvo: 'escolhidoAliado' }],
  danoTodosAliados: ['Dano em cada Aliado', { valor: 'num', ambos: 'bool' }, { valor: 1 }],
  destruirTodosAliados: ['Destruir todos os Aliados', { ambos: 'bool' }, { ambos: false }],
  anular: ['Anular carta na pilha', { alvo: ['pilha'] }, { alvo: 'pilha' }],
  comprar: ['Comprar cartas', { valor: 'num' }, { valor: 1 }],
  arcano: ['Ganhar Arcano', { valor: 'num' }, { valor: 1 }],
  arcanoOponente: ['Oponente ganha Arcano', { valor: 'num' }, { valor: 1 }],
  descontoProximaCarta: ['Desconto na 1ª carta do turno', { valor: 'num' }, { valor: 1 }],
  banirDaMao: ['Banir da mão (jogar depois com desconto)', { desconto: 'num' }, { desconto: 4 }],
  reanimar: ['Reanimar Aliado de um cemitério', { custoExtra: 'num' }, { custoExtra: 2 }],
  permitirAnularDoCemiterio: ['Jogar do cemitério carta que anula', {}, {}],
  olharTopo: ['Olhar o topo (escolher para a mão)', { quantidade: 'num', pegar: 'num' }, { quantidade: 3, pegar: 1 }],
};
const ROTULO_CAMPO = { valor: 'valor', alvo: 'alvo', escalaGrau: '+ cartas no Grau', ambos: 'os seus também', desconto: 'desconto', custoExtra: 'custo a mais', quantidade: 'cartas olhadas', pegar: 'vão para a mão' };
const ESPACOS = { canal: 'Canal (arma)', armadura: 'Armadura', extra: 'Extra (consumível)' };
const COMPROMETIMENTO = ['Invocador', 'Brigão', 'Parrudão da quebrada', 'Habilidoso', 'Preparado'];
const GATILHOS = { aliadoEntra: 'quando um Aliado seu entra', aliadoMorre: 'quando um Aliado seu vai para o cemitério', aliadoDanoConjurador: 'quando um Aliado seu causa dano ao Conjurador inimigo' };

// ---------- caminhos (efeitos.0.valor) ----------
function pegar(obj, caminho) { return caminho.split('.').reduce((o, k) => (o == null ? undefined : o[k]), obj); }
function por(obj, caminho, valor) {
  const ks = caminho.split('.');
  let o = obj;
  for (let i = 0; i < ks.length - 1; i++) { o[ks[i]] ??= /^\d+$/.test(ks[i + 1]) ? [] : {}; o = o[ks[i]]; }
  const k = ks.at(-1);
  if (valor === undefined) { if (Array.isArray(o)) o.splice(Number(k), 1); else delete o[k]; } else o[k] = valor;
}

// ---------- componentes de formulário ----------
/** Um campo. `orig`: valor original (marca o campo quando mudou). */
function campo(rotulo, caminho, tipo, valor, { orig, opcoes, dica, largo, alvo = 'atual', reestrutura = false } = {}) {
  const mudou = orig !== undefined && !igual(orig, valor) ? 'mudou' : '';
  const dados = `data-alvo="${alvo}" data-campo="${caminho}" data-tipo="${tipo}"${reestrutura ? ' data-reestrutura="1"' : ''}`;
  let el;
  if (tipo === 'bool') return `<label class="campo ${mudou}" style="flex-direction:row;align-items:center;gap:6px;color:var(--texto)"${dica ? ` title="${esc(dica)}"` : ''}><input type="checkbox" ${dados} ${valor ? 'checked' : ''}> ${esc(rotulo)}${mudou ? ` <span class="orig peq">(era ${orig ? 'sim' : 'não'})</span>` : ''}</label>`;
  if (opcoes) el = `<select ${dados}>${Object.entries(opcoes).map(([k, r]) => `<option value="${esc(k)}" ${k === String(valor ?? '') ? 'selected' : ''}>${esc(r)}</option>`).join('')}</select>`;
  else if (tipo === 'textoLongo') el = `<textarea ${dados} rows="3">${esc(valor ?? '')}</textarea>`;
  else if (tipo === 'int' || tipo === 'num') el = `<input type="number" ${tipo === 'num' ? 'step="any"' : 'step="1"'} ${dados} value="${valor ?? ''}">`;
  else el = `<input type="text" ${dados} value="${esc(Array.isArray(valor) ? valor.join(', ') : valor ?? '')}">`;
  const origTxt = mudou ? `<span class="orig">era: ${esc(Array.isArray(orig) ? orig.join(', ') || '—' : opcoes ? opcoes[orig] ?? orig : orig ?? '—')}</span>` : '';
  return `<label class="campo ${mudou}"${largo ? ' style="grid-column:1/-1"' : ''}${dica ? ` title="${esc(dica)}"` : ''}>${esc(rotulo)}${el}${origTxt}</label>`;
}
function checksValores(caminho, valores, orig) {
  const mudou = orig !== undefined && !igual([...(orig ?? [])].sort(), [...(valores ?? [])].sort());
  return `<div class="campo ${mudou ? 'mudou' : ''}" style="grid-column:1/-1">Valores${mudou ? ` <span class="orig">(era: ${esc(valoresTxt(orig))})</span>` : ''}<div class="checks">${Object.entries(VALOR).map(([k, r]) => `<label><input type="checkbox" data-alvo="atual" data-campo="${caminho}" data-tipo="valor" data-valor="${k}" ${valores?.includes(k) ? 'checked' : ''}> ${r}</label>`).join('')}</div></div>`;
}
function grupoEfeitos(titulo, caminho, efs, { opcional = true, nota = '' } = {}) {
  const linhas = (efs ?? []).map((ef, i) => {
    const def = EFEITOS[ef.tipo] ?? [ef.tipo, {}, {}];
    const opTipo = Object.fromEntries(Object.entries(EFEITOS).map(([k, d]) => [k, d[0]]));
    const campos = Object.entries(def[1]).map(([k, t]) => {
      const p = `${caminho}.${i}.${k}`;
      if (Array.isArray(t)) return campo(ROTULO_CAMPO[k] ?? k, p, 'texto', ef[k], { opcoes: Object.fromEntries(t.map((a) => [a, ALVO_TXT[a] ?? a])) });
      return campo(ROTULO_CAMPO[k] ?? k, p, t === 'num' ? 'int' : 'bool', ef[k]);
    }).join('');
    return `<div class="efeito">${campo('efeito', `${caminho}.${i}.tipo`, 'efeitoTipo', ef.tipo, { opcoes: opTipo, reestrutura: true })}${campos}<button data-acao="remEf" data-caminho="${caminho}" data-i="${i}" title="Remover efeito">✕</button></div>`;
  }).join('');
  return `<div class="grupo"><div class="titulo">${esc(titulo)} <button data-acao="addEf" data-caminho="${caminho}" data-opcional="${opcional ? 1 : 0}">+ efeito</button></div>
    ${nota ? `<div class="peq" style="margin-bottom:6px">${nota}</div>` : ''}${linhas || '<span class="peq">nenhum</span>'}</div>`;
}

// ---------- aba Cartas ----------
function linhasCartas() {
  const u = usos();
  const b = filtro.busca.trim().toLowerCase();
  const ids = idsCartas().map((id) => cartaDe(id)).filter((c) => c
    && (!b || c.nome.toLowerCase().includes(b) || c.id.includes(b))
    && (!filtro.tipo || c.tipo === filtro.tipo)
    && (!filtro.valor || (filtro.valor === 'nenhum' ? !c.valores?.length : c.valores?.includes(filtro.valor)))
    && (!filtro.uso || (filtro.uso === 'algum' ? u[c.id] : filtro.uso === 'nenhum' ? !u[c.id] : u[c.id]?.some(([cj]) => cj === filtro.uso)))
    && (!filtro.editadas || editada(c.id)))
    .sort((x, y) => x.custo - y.custo || x.nome.localeCompare(y.nome));
  return ids.map((c) => {
    const av = avaliacao(c);
    const copias = (u[c.id] ?? []).reduce((s, [, n]) => s + n, 0);
    return `<tr data-acao="selCarta" data-id="${esc(c.id)}" class="${sel.carta === c.id ? 'sel' : ''}">
      <td>${editada(c.id) ? `<span class="editada" title="editada">✎</span> ` : ''}${esc(c.nome)}${ed.cartas[c.id]?.nova ? ' <span class="peq">(nova)</span>' : ''}</td>
      <td class="peq">${TIPO[c.tipo] ?? c.tipo}</td><td>${c.custo}</td><td>${c.grau}</td>
      <td>${c.vida !== undefined && (c.tipo === 'aliado' || c.vida > 0) ? `${c.ataque ?? 0}/${c.vida}` : ''}</td>
      <td class="peq">${av ? fmt1(av.folga) : '?'}</td><td class="peq">${copias || ''}</td></tr>`;
  }).join('');
}
function painelCartas() {
  const opUso = `<option value="">todas</option><option value="algum" ${filtro.uso === 'algum' ? 'selected' : ''}>em algum baralho</option><option value="nenhum" ${filtro.uso === 'nenhum' ? 'selected' : ''}>em nenhum baralho</option>${idsConj().map((c) => `<option value="${c}" ${filtro.uso === c ? 'selected' : ''}>no baralho de ${esc(conjDe(c).nome)}</option>`).join('')}`;
  const lista = `<div class="painel">
    <div class="linha"><input type="text" id="f-busca" placeholder="Buscar carta…" value="${esc(filtro.busca)}" style="flex:1">
      <button data-acao="novaCarta">+ Nova carta</button><button data-acao="novoItem">+ Novo item</button></div>
    <div class="linha" style="margin-top:6px">
      <select id="f-tipo"><option value="">todos os tipos</option>${Object.entries(TIPO).map(([k, r]) => `<option value="${k}" ${filtro.tipo === k ? 'selected' : ''}>${r}</option>`).join('')}</select>
      <select id="f-valor"><option value="">todos os Valores</option><option value="nenhum" ${filtro.valor === 'nenhum' ? 'selected' : ''}>sem Valor</option>${Object.entries(VALOR).map(([k, r]) => `<option value="${k}" ${filtro.valor === k ? 'selected' : ''}>${r}</option>`).join('')}</select>
      <select id="f-uso">${opUso}</select>
      <label class="peq"><input type="checkbox" id="f-editadas" ${filtro.editadas ? 'checked' : ''}> só editadas</label></div>
    <div class="lista" style="margin-top:6px"><table><thead><tr><th>Carta</th><th>Tipo</th><th title="Custo">C</th><th title="Grau">G</th><th title="Ataque/Vida">A/V</th><th title="Folga de pontos: positivo = abaixo do orçamento (fraca); negativo = acima (forte)">PP</th><th title="Cópias nos baralhos">Cóp.</th></tr></thead>
      <tbody id="lista-corpo">${linhasCartas()}</tbody></table></div></div>`;
  return `<div class="duas">${lista}<div>${sel.carta ? formCarta() : '<div class="painel peq">Escolha uma carta na lista para editar, ou crie uma nova.</div>'}</div></div>`;
}
function ppHTML(c) {
  const av = avaliacao(c);
  if (!av) return c.tipo === 'item' ? '<span class="peq">Itens ficam fora do baralho (equipados no Conjurador) e não entram na conta de pontos.</span>' : '<span class="peq">Sem conta de pontos para algum efeito desta carta (a régua não tem preço para ele).</span>';
  const cls = Math.abs(av.folga) <= 1 ? 'bom' : 'ruim';
  const leitura = av.folga > 1 ? 'abaixo do orçamento (mais fraca que o padrão)' : av.folga < -1 ? 'acima do orçamento (mais forte que o padrão)' : 'no orçamento';
  return `Pontos usados <b>${fmt1(av.pontos)}</b> de um orçamento de <b>${fmt1(av.orcamento)}</b>${av.bonus ? ` (inclui +${fmt1(av.bonus)} da curva de Grau)` : ''} → folga <b class="${cls}">${fmt1(av.folga)}</b>: ${leitura}.`;
}
function formCarta() {
  const c = atual;
  const o = EST.gerado.cartas[c.id] ?? {};
  const nova = !EST.gerado.cartas[c.id];
  const g = (k) => (nova ? undefined : o[k]);
  const ehItem = c.tipo === 'item';
  const ehPerm = c.tipo !== 'temporaria' && !ehItem;
  const podeAtivar = ehPerm || (ehItem && c.categoriaItem === 'extra');
  const u = usos()[c.id] ?? [];
  const difs = nova ? [] : difCampos(o, c);
  return `<div class="painel">
    <div class="linha"><b style="font-size:16px">${esc(c.nome)}</b> <span class="peq">${esc(c.id)}</span>
      ${editada(c.id) && !nova ? '<button data-acao="restaurarCarta">Restaurar original</button>' : ''}
      <button data-acao="duplicarCarta">Duplicar como nova</button>
      ${nova ? '<button class="perigo" data-acao="excluirCarta">Excluir carta nova</button>' : ''}</div>
    <div class="peq">Nos baralhos: ${u.length ? u.map(([cj, n]) => `${esc(conjDe(cj).nome)} ${n}×`).join(', ') : 'nenhum'}</div>
    <div class="pp painel" id="pp" style="margin:8px 0">${ppHTML(c)}</div>
    <div class="campos">
      ${campo('Nome', 'nome', 'texto', c.nome, { orig: g('nome'), largo: true })}
      ${campo('Tipo', 'tipo', 'texto', c.tipo, { orig: g('tipo'), opcoes: TIPO, reestrutura: true })}      ${campo('Custo', 'custo', 'int', c.custo, { orig: g('custo') })}
      ${campo('Grau', 'grau', 'int', c.grau, { orig: g('grau') })}
      ${campo('Velocidade', 'velocidade', 'texto', c.velocidade, { orig: g('velocidade'), opcoes: { findante: 'Findante', repentina: 'Repentina' } })}
      ${campo('Arcano gerado (pitch)', 'arcanoGerado', 'int', c.arcanoGerado, { orig: g('arcanoGerado') })}
      ${ehPerm ? campo('Ataque', 'ataque', 'int', c.ataque, { orig: g('ataque') }) : ''}
      ${ehPerm ? campo('Vida', 'vida', 'int', c.vida, { orig: g('vida') }) : ''}
      ${campo('Descarta do Grau', 'descartaGrau', 'int', c.descartaGrau, { orig: g('descartaGrau'), dica: 'Cartas do Grau que vão para o cemitério ao jogar.' })}
      ${ehPerm ? campo('Protegido', 'protegido', 'int', c.protegido, { orig: g('protegido'), dica: 'O oponente paga este Arcano a mais para mirar a carta.' }) : ''}
      ${ehPerm ? campo('Conta como cartas no Grau', 'grauExtra', 'int', c.grauExtra, { orig: g('grauExtra') }) : ''}
      ${campo('Oponente paga para anular', 'pagarParaAnular', 'int', c.pagarParaAnular, { orig: g('pagarParaAnular'), dica: 'Burn: depois de jogada, o oponente pode pagar este Arcano para ela não resolver. Vazio = não tem.' })}
      ${campo('Palavras-chave', 'palavrasChave', 'lista', c.palavrasChave, { orig: g('palavrasChave'), dica: 'Separadas por vírgula. O motor entende: Proficiente, Etéreo.' })}
      ${campo('Subtipos', 'subtipos', 'listaObrig', c.subtipos, { orig: g('subtipos') })}
      ${campo('Valores exigidos no Grau', 'custoValores', 'lista', c.custoValores, { orig: g('custoValores'), dica: 'Ids de Valor separados por vírgula (ex.: eternyr).' })}
      ${checksValores('valores', c.valores, g('valores'))}
    </div>
    ${ehItem ? `<div class="grupo"><div class="titulo">Item do Conjurador</div><div class="campos">
      ${campo('Espaço', 'categoriaItem', 'texto', c.categoriaItem, { orig: g('categoriaItem'), opcoes: ESPACOS })}
      ${c.categoriaItem === 'canal' ? campo('Ataque (valor da arma)', 'ataque', 'int', c.ataque, { orig: g('ataque') }) : ''}
      ${c.categoriaItem === 'armadura' ? campo('Defesa', 'defesa', 'int', c.defesa, { orig: g('defesa'), dica: 'Dano de combate que evita quando o Conjurador bloqueia.' }) : ''}
      ${c.categoriaItem === 'armadura' ? campo('Vida de desgaste', 'vidaDesgaste', 'int', c.vidaDesgaste, { orig: g('vidaDesgaste'), dica: 'Quanto dano evita no total antes de quebrar.' }) : ''}
      </div><p class="peq">Itens ficam fora do baralho, num side deck. Aqui você escolhe o item padrão de cada Conjurador na aba Baralhos; no app, o jogador escolhe um de cada espaço antes da partida. O Extra é consumível: use a habilidade ativada abaixo (marque "Habilidade ativada").</p></div>` : ''}
    ${c.tipo === 'temporaria' || c.efeitos?.length ? grupoEfeitos('Efeitos ao resolver', 'efeitos', c.efeitos) : ''}
    ${ehPerm ? grupoEfeitos('Ao entrar em campo', 'aoEntrar', c.aoEntrar) : ''}
    ${c.tipo === 'aliado' ? grupoEfeitos('Ao causar dano de combate ao Conjurador inimigo', 'aoDanoConjurador', c.aoDanoConjurador) : ''}
    ${podeAtivar ? `<div class="grupo"><div class="titulo"><label><input type="checkbox" data-alvo="atual" data-campo="ativacao" data-tipo="ativacao" ${c.ativacao ? 'checked' : ''}> Habilidade ativada</label></div>
      ${c.ativacao ? `<div class="campos">${campo('Custo em Arcano', 'ativacao.custo', 'int', c.ativacao.custo, { orig: g('ativacao')?.custo })}${campo('Suspende a carta', 'ativacao.suspende', 'bool', c.ativacao.suspende, { orig: g('ativacao')?.suspende })}</div>
        ${grupoEfeitos('Efeitos da habilidade', 'ativacao.efeitos', c.ativacao.efeitos, { opcional: false })}` : ''}</div>` : ''}
    <div class="grupo"><div class="titulo">Texto da carta <button data-acao="usarTextoGerado">Usar o texto gerado</button></div>
      <div class="texto-gerado" id="texto-gerado"><span class="peq">Gerado pela mecânica (é o que o app de testes mostra):</span> ${esc(textoCarta(c))}</div>
      <div class="campos" style="margin-top:6px">${campo('Texto (para a carta impressa)', 'texto', 'textoLongo', c.texto, { orig: g('texto'), largo: true })}${campo('Cenário', 'cenario', 'texto', c.cenario, { orig: g('cenario'), largo: true })}</div></div>
    <div class="grupo" id="diferencas"><div class="titulo">Diferenças do original</div>${difs.length ? `<ul style="margin:0;padding-left:18px">${difs.map((d) => `<li class="peq">${esc(d)}</li>`).join('')}</ul>` : `<span class="peq">${nova ? 'carta nova' : 'nenhuma'}</span>`}</div>
  </div>`;
}

// ---------- aba Conjuradores ----------
function painelConj() {
  const lista = `<div class="painel"><table class="lista">${idsConj().map((id) => { const c = conjDe(id); return `<tr data-acao="selConj" data-id="${id}" class="${sel.conj === id ? 'sel' : ''}"><td>${ed.conjuradores[id] ? '<span class="editada">✎</span> ' : ''}<b>${esc(c.nome)}</b><div class="peq">${esc(valoresTxt(c.valores))} · Vida ${c.vida}</div></td></tr>`; }).join('')}</table>
    <div class="grupo"><div class="titulo">Aplicar a todos os Conjuradores</div>
      <div class="campos">${['vida', 'maoInicial', 'compraFinal', 'danoBasico', 'custoAtaque'].map((k) => `<label class="campo">${ROT_CONJ[k]}<input type="number" step="1" id="todos-${k}" placeholder="—"></label>`).join('')}</div>
      <button data-acao="aplicarTodos" style="margin-top:6px">Aplicar</button> <span class="peq">Campos vazios ficam como estão.</span></div></div>`;
  return `<div class="duas">${lista}<div>${sel.conj ? formConj() : '<div class="painel peq">Escolha um Conjurador.</div>'}</div></div>`;
}
const ROT_CONJ = { vida: 'Vida', maoInicial: 'Mão inicial', compraFinal: 'Compra no Final', danoBasico: 'Dano básico', custoAtaque: 'Custo de ataque' };
function formConj() {
  const c = atual;
  const o = EST.gerado.conjuradores[c.id];
  const p = c.poder ?? {};
  const op = o.poder ?? {};
  return `<div class="painel">
    <div class="linha"><b style="font-size:16px">${esc(c.nome)}</b>${ed.conjuradores[c.id] ? ' <button data-acao="restaurarConj">Restaurar original</button>' : ''}</div>
    <div class="campos" style="margin-top:8px">
      ${campo('Nome', 'nome', 'texto', c.nome, { orig: o.nome, largo: true })}
      ${checksValores('valores', c.valores, o.valores)}
      ${campo('Comprometimento', 'comprometimento', 'texto', c.comprometimento, { orig: o.comprometimento, opcoes: Object.fromEntries(COMPROMETIMENTO.map((x) => [x, x])) })}
      ${Object.entries(ROT_CONJ).map(([k, r]) => campo(r, k, 'int', c[k], { orig: o[k] })).join('')}
    </div>
    <p class="peq">Mudar os Valores muda as cartas que o baralho pode ter: confira a aba Baralhos.</p>
    <div class="grupo"><div class="titulo"><label><input type="checkbox" data-alvo="atual" data-campo="poder.ativacao" data-tipo="ativacaoConj" ${p.ativacao ? 'checked' : ''}> Habilidade ativada</label></div>
      ${p.ativacao ? `<div class="campos">${campo('Custo em Arcano', 'poder.ativacao.custo', 'int', p.ativacao.custo, { orig: op.ativacao?.custo })}${campo('Suspende o Conjurador', 'poder.ativacao.suspende', 'bool', p.ativacao.suspende, { orig: op.ativacao?.suspende })}${campo('Repentina (a qualquer momento)', 'poder.ativacao.repentina', 'bool', !!p.ativacao.repentina, { orig: op.ativacao ? !!op.ativacao.repentina : undefined, dica: 'Sem isso, só na sua fase Principal com a pilha vazia.' })}</div>
        ${grupoEfeitos('Efeitos', 'poder.ativacao.efeitos', p.ativacao.efeitos, { opcional: false })}` : ''}</div>
    <div class="grupo"><div class="titulo"><label><input type="checkbox" data-alvo="atual" data-campo="poder.gatilho" data-tipo="gatilhoConj" ${p.gatilho ? 'checked' : ''}> Gatilho (no máximo uma vez por turno)</label></div>
      ${p.gatilho ? `<div class="campos">${campo('Quando', 'poder.gatilho.quando', 'texto', p.gatilho.quando, { orig: op.gatilho?.quando, opcoes: GATILHOS })}${campo('Só no seu turno', 'poder.gatilho.soNoSeuTurno', 'bool', !!p.gatilho.soNoSeuTurno, { orig: op.gatilho ? !!op.gatilho.soNoSeuTurno : undefined })}</div>
        ${grupoEfeitos('Efeitos', 'poder.gatilho.efeitos', p.gatilho.efeitos, { opcional: false })}` : ''}</div>
    <div class="grupo"><div class="titulo">Passivas</div><div class="campos">
      ${campo('1º dano de combate de Aliado +', 'poder.primeiroDanoDeAliadoMais', 'int', p.primeiroDanoDeAliadoMais, { orig: op.primeiroDanoDeAliadoMais })}
      ${campo('1ª carta do turno custa − ', 'poder.primeiraCartaMaisBarata', 'int', p.primeiraCartaMaisBarata, { orig: op.primeiraCartaMaisBarata })}
      ${campo('1ª Repentina do turno custa −', 'poder.primeiraRepentinaMaisBarata', 'int', p.primeiraRepentinaMaisBarata, { orig: op.primeiraRepentinaMaisBarata })}
      ${campo('Aliados têm Protegido', 'poder.aliadosProtegidos', 'int', p.aliadosProtegidos, { orig: op.aliadosProtegidos })}</div></div>
    <div class="grupo"><div class="titulo">Texto da habilidade <button data-acao="usarTextoPoder">Usar o texto gerado</button></div>
      <div class="texto-gerado" id="texto-gerado"><span class="peq">Gerado pela mecânica:</span> ${esc(textoPoder(c.poder) || '—')}</div>
      <div class="campos" style="margin-top:6px">${campo('Texto (o que aparece no app e na carta)', 'habilidade', 'textoLongo', c.habilidade, { orig: o.habilidade, largo: true })}</div></div>
  </div>`;
}

// ---------- aba Regras ----------
function painelRegras() {
  const r = { ...EST.regrasOriginais, ...ed.regras };
  const linhas = Object.entries(REGRAS).map(([k, [rot, tipo, dica]]) => `<tr><td>${campo(rot, k, tipo, r[k], { orig: EST.regrasOriginais[k], alvo: 'regra' })}</td><td class="peq">${esc(dica)}</td></tr>`).join('');
  const k = curva();
  const tabela = [2, 3, 4, 5, 6, 7, 8].map((gr) => `<td>G${gr}: +${fmt1(bonusGrau(gr, k))}</td>`).join('');
  return `<div class="painel"><b>Regras da partida</b> <span class="peq">valem para todas as partidas novas e simulações</span>
      <table style="margin-top:6px">${linhas}</table></div>
    <div class="painel"><b>Curva de poder por Grau</b>
      <p class="peq">Bônus de orçamento dos Aliados: k × G × (G − 1) ÷ 2, com G = Grau limitado a 8. Ao salvar, recalcula o Ataque e a Vida de todos os Aliados gerados (as cartas que você editou à mão mantêm os seus números).</p>
      <div class="campos" style="max-width:300px">${campo('k', 'curvaGrau', 'num', k, { orig: EST.curvaOriginal, alvo: 'curva' })}</div>
      <table style="margin-top:6px"><tr>${tabela}</tr></table></div>`;
}

// ---------- aba Régua ----------
function painelRegua() {
  const p = precos();
  return `<div class="painel"><b>Régua de pontos (PP)</b>
    <p class="peq">É a balança do editor: define a conta de pontos (coluna PP e o quadro de cada carta). Mudar a régua <b>não muda nenhuma carta</b>; serve para você medir as cartas com outros preços. Orçamento de uma carta = 2 × custo + 2.</p>
    <table>${Object.keys(EST.precosOriginais).map((k) => `<tr><td style="width:60%">${esc(PRECOS[k] ?? k)}</td><td>${campo('', k, 'num', p[k], { orig: EST.precosOriginais[k], alvo: 'preco' })}</td></tr>`).join('')}</table></div>`;
}

// ---------- aba Baralhos ----------
function painelBaralhos() {
  sel.baralho ??= idsConj()[0];
  const cj = sel.baralho;
  const conj = conjDe(cj);
  const lista = listaDe(cj);
  const total = lista.reduce((s, [, n]) => s + n, 0);
  const est = estiloDe(cj);
  const estO = EST.baralhosOriginais[cj].estilo;
  const linhas = lista.map(([id, n]) => {
    const c = cartaDe(id);
    if (!c) return `<tr class="ilegal"><td colspan="5">${esc(id)} (carta não existe mais)</td><td><input class="num" type="number" min="0" data-alvo="copias" data-id="${esc(id)}" value="${n}"></td></tr>`;
    return `<tr class="${legal(c, conj) ? '' : 'ilegal'}"><td><a href="#" data-acao="irCarta" data-id="${esc(c.id)}">${esc(c.nome)}</a>${editada(id) ? ' <span class="editada">✎</span>' : ''}</td><td class="peq">${TIPO[c.tipo]}</td><td>${c.custo}/${c.grau}</td><td>${c.vida !== undefined ? `${c.ataque ?? 0}/${c.vida}` : ''}</td><td class="peq">${esc(valoresTxt(c.valores))}${legal(c, conj) ? '' : ' (não combina)'}</td>
      <td><input class="num" type="number" min="0" step="1" data-alvo="copias" data-id="${esc(id)}" value="${n}"></td></tr>`;
  }).join('');
  const naLista = new Set(lista.map(([id]) => id));
  const disponiveis = idsCartas().map((id) => cartaDe(id)).filter((c) => c && c.tipo !== 'item' && legal(c, conj) && !naLista.has(c.id)).sort((x, y) => x.nome.localeCompare(y.nome));
  const itens = itensDe(cj);
  const todosItens = idsCartas().map((id) => cartaDe(id)).filter((c) => c && c.tipo === 'item' && legal(c, conj));
  const slot = (esp) => {
    const atualId = itens.find((id) => cartaDe(id)?.categoriaItem === esp) ?? '';
    return `<label class="campo">${ESPACOS[esp]}<select data-alvo="itemSlot" data-campo="${esp}"><option value="">— nenhum —</option>${todosItens.filter((c) => c.categoriaItem === esp).map((c) => `<option value="${esc(c.id)}" ${c.id === atualId ? 'selected' : ''}>${esc(c.nome)} (${esc(textoCarta(c))})</option>`).join('')}</select></label>`;
  };
  return `<div class="abas">${idsConj().map((id) => `<button class="${id === cj ? 'ativa' : ''}" data-acao="selBaralho" data-id="${id}">${ed.baralhos[id] ? '✎ ' : ''}${esc(conjDe(id).nome.split(',')[0])}</button>`).join('')}</div>
    <div class="painel"><div class="linha"><b>${esc(conj.nome)}</b> <span class="peq">${esc(valoresTxt(conj.valores))}</span>
      <span id="total-baralho" class="${total === 60 ? 'salvo' : 'pendente'}">${total} de 60 cartas</span>
      ${ed.baralhos[cj]?.lista ? '<button data-acao="restaurarBaralho">Restaurar baralho original</button>' : ''}</div>
      <table style="margin-top:6px"><tr><th>Carta</th><th>Tipo</th><th>Custo/Grau</th><th>A/V</th><th>Valores</th><th>Cópias</th></tr>${linhas}</table>
      <div class="linha" style="margin-top:8px">Adicionar <select id="add-carta">${disponiveis.map((c) => `<option value="${esc(c.id)}">${esc(c.nome)} (custo ${c.custo}, Grau ${c.grau})</option>`).join('')}</select>
        <input class="num" type="number" id="add-n" value="1" min="1"> <button data-acao="addCarta">Adicionar</button></div>
      <p class="peq">Só aparecem as cartas que combinam com os Valores do Conjurador (ou sem Valor). O baralho precisa ter 60 cartas para salvar.</p></div>
    <div class="painel"><b>Itens equipados no Conjurador</b> <span class="peq">(fora das 60 cartas; um por espaço)</span>${ed.baralhos[cj]?.itens && !igual(itens, EST.baralhosOriginais[cj].itens ?? []) ? ' <span class="editada">✎</span>' : ''}
      <div class="campos" style="margin-top:6px">${['canal', 'armadura', 'extra'].map(slot).join('')}</div>
      <p class="peq">Ataque do Conjurador com estes itens: ${conj.danoBasico + itens.reduce((s, id) => { const c = cartaDe(id); return s + (c && c.categoriaItem !== 'armadura' ? c.ataque ?? 0 : 0); }, 0)} (dano básico ${conj.danoBasico} + armas).</p></div>
    <div class="painel"><b>Estilo do jogador automático com este baralho</b>
      <div class="campos" style="margin-top:6px">${Object.entries(ESTILO).map(([k, [rot, tipo, dica]]) => campo(rot, k, tipo, est[k], { orig: estO[k], alvo: 'estilo', dica })).join('')}</div></div>`;
}

// ---------- aba Testar ----------
function painelTestar() {
  const s = ultimaSim ?? {};
  const st = s.estado;
  const at = s.atual, an = s.anterior;
  let tabela = '';
  if (at) {
    const ant = an ? Object.fromEntries(an.nomes.map((n, i) => [n, an.geral[i]])) : {};
    const ordem = at.nomes.map((n, i) => [n, at.geral[i]]).sort((a, b) => b[1] - a[1]);
    const pct = (x) => (x === undefined || x === null || Number.isNaN(x) ? '—' : `${(100 * x).toFixed(1).replace('.', ',')}%`);
    tabela = `<table style="margin-top:8px"><tr><th>Conjurador</th><th>Vitória média</th><th>Simulação anterior</th><th>Diferença</th></tr>${ordem.map(([n, g]) => {
      const d = ant[n] !== undefined ? g - ant[n] : null;
      return `<tr><td>${esc(n)}</td><td><b>${pct(g)}</b></td><td>${pct(ant[n])}</td><td class="${d > 0.005 ? 'delta-pos' : d < -0.005 ? 'delta-neg' : ''}">${d === null ? '—' : `${d > 0 ? '+' : ''}${(100 * d).toFixed(1).replace('.', ',')}`}</td></tr>`;
    }).join('')}</table>
    <p class="peq">Duração média ${fmt1(at.duracao)} turnos${an ? ` (antes ${fmt1(an.duracao)})` : ''} · quem começa vence ${pct(at.primeiro)}${an ? ` (antes ${pct(an.primeiro)})` : ''}. Relatório completo em docs/CONJURADORES-EDITOR.md.</p>
    <details><summary class="peq">Matriz (linha contra coluna)</summary><table><tr><th></th>${at.nomes.map((n) => `<th class="peq">${esc(n)}</th>`).join('')}</tr>${at.nomes.map((n, i) => `<tr><th class="peq">${esc(n)}</th>${at.matriz[i].map((x) => `<td class="peq">${x === null ? '—' : pct(x)}</td>`).join('')}</tr>`).join('')}</table></details>`;
  }
  return `<div class="painel"><b>Simular os 9 Conjuradores todos contra todos</b>
    <p class="peq">Usa o que está <b>salvo</b>. O jogador automático joga as partidas; cada confronto é jogado nas duas ordens.</p>
    ${pendente() ? '<div class="aviso">Você tem alterações não salvas: salve antes de simular, senão elas ficam de fora.</div>' : ''}
    <div class="linha">Partidas por confronto <select id="sim-n">${[10, 20, 40, 80].map((n) => `<option ${n === 40 ? 'selected' : ''}>${n}</option>`).join('')}</select>
      <button class="primario" data-acao="simular" ${st?.rodando ? 'disabled' : ''}>${st?.rodando ? 'Simulando…' : 'Simular'}</button>
      ${st ? `<span class="peq">${st.rodando ? `começou ${new Date(st.inicio).toLocaleTimeString('pt-BR')}` : `terminou ${new Date(st.fim).toLocaleTimeString('pt-BR')}${st.codigo ? ` com erro (código ${st.codigo})` : ''}`}</span>` : ''}</div>
    ${st && st.codigo ? `<pre class="registro">${esc(st.saida)}</pre>` : ''}
    ${tabela || '<p class="peq">Nenhuma simulação feita pelo editor ainda.</p>'}</div>`;
}

// ---------- aba Histórico ----------
function painelHistorico() {
  const h = [...(EST.edicoes.historico ?? [])].reverse();
  return `<div class="painel"><b>Salvamentos</b> <span class="peq">(o arquivo anterior a cada salvamento fica em data/edicoes-historico/)</span>
    ${h.length ? h.map((x) => `<div class="grupo"><div class="titulo">${esc(new Date(x.quando).toLocaleString('pt-BR'))}</div>${x.nota ? `<p>${esc(x.nota)}</p>` : ''}<ul style="margin:0;padding-left:18px">${x.mudancas.map((m) => `<li class="peq">${esc(m)}</li>`).join('') || '<li class="peq">sem mudanças</li>'}</ul></div>`).join('') : '<p class="peq">Nada salvo ainda.</p>'}</div>`;
}

// ---------- tela ----------
function barraHTML() {
  const n = pendente() ? resumoMudancas().length : 0;
  return `<span id="estado-salvo" class="${n ? 'pendente' : 'salvo'}">${n ? `${n} alteração(ões) não salva(s)` : 'Tudo salvo'}</span>`;
}
function render() {
  const abas = [['cartas', 'Cartas'], ['conjuradores', 'Conjuradores'], ['regras', 'Regras e curva de Grau'], ['baralhos', 'Baralhos'], ['testar', 'Testar (simulação)'], ['regua', 'Régua de pontos'], ['historico', 'Histórico']];
  const corpo = { cartas: painelCartas, conjuradores: painelConj, regras: painelRegras, baralhos: painelBaralhos, testar: painelTestar, regua: painelRegua, historico: painelHistorico }[aba]();
  const rolagem = $('.lista')?.scrollTop;
  $('#app').innerHTML = `<div class="barra"><div class="linha"><span id="barra-estado">${barraHTML()}</span>
      <input type="text" id="nota" placeholder="Nota deste salvamento (por que mudou?) — vai para o histórico e para o Claude" value="${esc($('#nota')?.value ?? '')}">
      <button class="primario" data-acao="salvar">Salvar</button><button data-acao="descartar">Descartar alterações</button></div>
      ${msg ? `<div class="${msg.tipo}" style="margin:8px 0 0">${msg.html}</div>` : ''}</div>
    <div class="abas">${abas.map(([k, r]) => `<button class="${aba === k ? 'ativa' : ''}" data-aba="${k}">${r}</button>`).join('')}</div>${corpo}`;
  if (rolagem && $('.lista')) $('.lista').scrollTop = rolagem;
}
/** Depois de mudar um campo simples: atualiza só o que depende dele (sem redesenhar o formulário). */
function atualizarParcial() {
  $('#barra-estado').innerHTML = barraHTML();
  if (aba === 'cartas' && atual) {
    $('#pp').innerHTML = ppHTML(atual);
    $('#texto-gerado').innerHTML = `<span class="peq">Gerado pela mecânica (é o que o app de testes mostra):</span> ${esc(textoCarta(atual))}`;
    $('#lista-corpo').innerHTML = linhasCartas();
    const o = EST.gerado.cartas[atual.id];
    const difs = o ? difCampos(o, atual) : [];
    $('#diferencas').innerHTML = `<div class="titulo">Diferenças do original</div>${difs.length ? `<ul style="margin:0;padding-left:18px">${difs.map((d) => `<li class="peq">${esc(d)}</li>`).join('')}</ul>` : `<span class="peq">${o ? 'nenhuma' : 'carta nova'}</span>`}`;
  }
  if (aba === 'conjuradores' && atual) $('#texto-gerado').innerHTML = `<span class="peq">Gerado pela mecânica:</span> ${esc(textoPoder(atual.poder) || '—')}`;
  if (aba === 'baralhos') {
    const total = listaDe(sel.baralho).reduce((s, [, n]) => s + n, 0);
    $('#total-baralho').className = total === 60 ? 'salvo' : 'pendente';
    $('#total-baralho').textContent = `${total} de 60 cartas`;
  }
}
function gravarAtual() { if (aba === 'cartas') gravarCarta(atual); else gravarConj(atual); }

// ---------- eventos ----------
function lerValor(el) {
  const t = el.dataset.tipo;
  if (t === 'bool') return el.checked;
  if (t === 'int' || t === 'num') { if (el.value.trim() === '') return undefined; const n = Number(el.value); return Number.isFinite(n) ? (t === 'int' ? Math.round(n) : n) : undefined; }
  if (t === 'lista' || t === 'listaObrig') { const l = el.value.split(',').map((x) => x.trim()).filter(Boolean); return l.length || t === 'listaObrig' ? l : undefined; }
  return el.value;
}
function aoMudar(el) {
  const alvo = el.dataset.alvo;
  const t = el.dataset.tipo;
  const caminho = el.dataset.campo;
  if (alvo === 'atual') {
    if (t === 'valor') {
      const vs = new Set(pegar(atual, caminho) ?? []);
      if (el.checked) vs.add(el.dataset.valor); else vs.delete(el.dataset.valor);
      por(atual, caminho, Object.keys(VALOR).filter((v) => vs.has(v)));
    } else if (t === 'ativacao' || t === 'ativacaoConj') {
      por(atual, caminho, el.checked ? (clone(pegar(aba === 'cartas' ? EST.gerado.cartas[atual.id] ?? {} : EST.gerado.conjuradores[atual.id], caminho)) ?? { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] }) : undefined);
      gravarAtual(); render(); return;
    } else if (t === 'gatilhoConj') {
      por(atual, caminho, el.checked ? (clone(EST.gerado.conjuradores[atual.id].poder?.gatilho) ?? { quando: 'aliadoEntra', efeitos: [{ tipo: 'arcano', valor: 1 }] }) : undefined);
      gravarAtual(); render(); return;
    } else if (t === 'efeitoTipo') {
      const i = caminho.replace(/\.tipo$/, '');
      por(atual, i, { tipo: el.value, ...clone(EFEITOS[el.value][2]) });
      gravarAtual(); render(); return;
    } else {
      const v = lerValor(el);
      if (caminho.startsWith('poder.') && atual.poder === undefined) atual.poder = {};
      // marcas opcionais desligadas saem do objeto (como nas cartas geradas); "ambos" é obrigatório em destruirTodosAliados
      const k = caminho.split('.').at(-1);
      const pai = pegar(atual, caminho.split('.').slice(0, -1).join('.'));
      const opcional = ['repentina', 'soNoSeuTurno', 'escalaGrau'].includes(k) || (k === 'ambos' && pai?.tipo === 'danoTodosAliados');
      por(atual, caminho, v === false && opcional ? undefined : v);
    }
    gravarAtual();
    if (el.dataset.reestrutura) render(); else atualizarParcial();
    return;
  }
  if (alvo === 'regra') {
    const v = lerValor(el);
    if (v === undefined || igual(v, EST.regrasOriginais[caminho])) delete ed.regras[caminho]; else ed.regras[caminho] = v;
  } else if (alvo === 'preco') {
    const v = lerValor(el);
    if (v === undefined || v === EST.precosOriginais[caminho]) delete ed.precos[caminho]; else ed.precos[caminho] = v;
  } else if (alvo === 'curva') {
    const v = lerValor(el);
    if (v === undefined || v === EST.curvaOriginal) delete ed.curvaGrau; else ed.curvaGrau = v;
  } else if (alvo === 'estilo') {
    const v = lerValor(el);
    gravarEstilo(sel.baralho, caminho, v ?? EST.baralhosOriginais[sel.baralho].estilo[caminho]);
  } else if (alvo === 'itemSlot') {
    const outros = itensDe(sel.baralho).filter((id) => cartaDe(id)?.categoriaItem !== caminho);
    gravarItens(sel.baralho, el.value ? [...outros, el.value] : outros);
    render(); return;
  } else if (alvo === 'copias') {
    const n = Math.max(0, Math.round(Number(el.value) || 0));
    gravarLista(sel.baralho, listaDe(sel.baralho).map(([id, q]) => [id, id === el.dataset.id ? n : q]));
  }
  atualizarParcial();
}
document.addEventListener('input', (ev) => {
  const el = ev.target;
  if (el.id === 'f-busca') { filtro.busca = el.value; $('#lista-corpo').innerHTML = linhasCartas(); return; }
  if (el.dataset?.alvo && el.tagName !== 'SELECT' && el.type !== 'checkbox') aoMudar(el);
});
document.addEventListener('change', (ev) => {
  const el = ev.target;
  if (el.id === 'f-tipo') { filtro.tipo = el.value; render(); return; }
  if (el.id === 'f-valor') { filtro.valor = el.value; render(); return; }
  if (el.id === 'f-uso') { filtro.uso = el.value; render(); return; }
  if (el.id === 'f-editadas') { filtro.editadas = el.checked; render(); return; }
  // números e textos já foram tratados no "input"; selects e caixas de marcar entram aqui
  if (el.dataset?.alvo && (el.tagName === 'SELECT' || el.type === 'checkbox')) aoMudar(el);
  if (el.dataset?.alvo === 'copias' && Number(el.value) === 0) { gravarLista(sel.baralho, listaDe(sel.baralho).filter(([, n]) => n > 0)); render(); }
});

function slug(t) { return t.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '').slice(0, 40) || 'carta'; }
function novoId(nome) { let id = 'autor-' + slug(nome), i = 2; const base = id; while (EST.gerado.cartas[id] || ed.cartas[id]) id = `${base}-${i++}`; return id; }

async function salvar() {
  if (!pendente()) { msg = { tipo: 'ok', html: 'Nada para salvar.' }; render(); return; }
  const mudancas = resumoMudancas();
  msg = { tipo: 'ok', html: 'Salvando e conferindo (baralhos e partidas de teste)…' }; render();
  try {
    const r = await (await fetch('/editor/salvar', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ edicoes: ed, nota: $('#nota').value, mudancas }) })).json();
    if (!r.ok) { msg = { tipo: 'aviso', html: `<b>Não salvei: o jogo ficaria com problema.</b><ul style="margin:4px 0 0;padding-left:18px">${r.erros.map((x) => `<li>${esc(x)}</li>`).join('')}</ul>` }; render(); return; }
    const selCarta = sel.carta, selConj = sel.conj;
    await carregar();
    if (aba === 'cartas' && selCarta) atual = cartaDe(selCarta);
    if (aba === 'conjuradores' && selConj) atual = conjDe(selConj);
    $('#nota').value = '';
    msg = { tipo: 'ok', html: `<b>Salvo.</b> ${mudancas.length} mudança(s). As novas partidas do app de testes e as simulações já usam esta versão (${esc(r.versao)}).${r.avisos.length ? `<ul style="margin:4px 0 0;padding-left:18px">${r.avisos.map((x) => `<li>${esc(x)}</li>`).join('')}</ul>` : ''}` };
  } catch (erro) {
    msg = { tipo: 'aviso', html: `Não consegui falar com o servidor (${esc(erro.message)}). Abra o app pelo INICIAR.bat.` };
  }
  render();
}

async function acompanharSim() {
  clearTimeout(timerSim);
  try { ultimaSim = await (await fetch('/editor/simulacao', { cache: 'no-store' })).json(); } catch { return; }
  if (aba === 'testar') render();
  if (ultimaSim.estado?.rodando) timerSim = setTimeout(acompanharSim, 1000);
}

document.addEventListener('click', async (ev) => {
  const b = ev.target.closest('[data-acao], [data-aba]');
  if (!b) return;
  if (b.tagName === 'A') ev.preventDefault();
  if (b.dataset.aba) {
    aba = b.dataset.aba; msg = null;
    atual = aba === 'cartas' && sel.carta ? cartaDe(sel.carta) : aba === 'conjuradores' && sel.conj ? conjDe(sel.conj) : null;
    render(); return;
  }
  const a = b.dataset.acao, id = b.dataset.id;
  switch (a) {
    case 'salvar': salvar(); return;
    case 'descartar':
      if (!pendente() || !confirm('Descartar todas as alterações não salvas?')) return;
      ed = clone(EST.edicoes); atual = aba === 'cartas' && sel.carta ? cartaDe(sel.carta) : aba === 'conjuradores' && sel.conj ? conjDe(sel.conj) : null;
      if (aba === 'cartas' && !atual) sel.carta = null;
      msg = { tipo: 'ok', html: 'Alterações descartadas.' }; break;
    case 'selCarta': sel.carta = id; atual = cartaDe(id); msg = null; break;
    case 'irCarta': aba = 'cartas'; sel.carta = id; atual = cartaDe(id); filtro.busca = ''; break;
    case 'selConj': sel.conj = id; atual = conjDe(id); msg = null; break;
    case 'selBaralho': sel.baralho = id; break;
    case 'novoItem': {
      const nome = prompt('Nome do item novo:', 'Novo item');
      if (!nome) return;
      const c = { id: novoId(nome), nome, tipo: 'item', categoriaItem: 'canal', subtipos: ['Equipamento'], valores: [], custo: 0, grau: 0, velocidade: 'findante', arcanoGerado: 0, ataque: 1, cenario: '', texto: '' };
      gravarCarta(c); sel.carta = c.id; atual = cartaDe(c.id); filtro.editadas = false; break;
    }
    case 'novaCarta': {
      const nome = prompt('Nome da carta nova:', 'Nova carta');
      if (!nome) return;
      const c = { id: novoId(nome), nome, tipo: 'aliado', subtipos: [], valores: [], custo: 2, grau: 1, velocidade: 'findante', arcanoGerado: 1, ataque: 2, vida: 2, cenario: '', texto: '' };
      gravarCarta(c); sel.carta = c.id; atual = cartaDe(c.id); filtro.editadas = false; break;
    }
    case 'duplicarCarta': {
      const nome = prompt('Nome da carta nova (cópia):', atual.nome + ' (cópia)');
      if (!nome) return;
      const c = { ...clone(atual), id: novoId(nome), nome };
      gravarCarta(c); sel.carta = c.id; atual = cartaDe(c.id); break;
    }
    case 'excluirCarta': {
      if (!confirm(`Excluir a carta nova "${atual.nome}"? Ela sai também dos baralhos.`)) return;
      const cid = atual.id;
      delete ed.cartas[cid];
      for (const cj of idsConj()) if (listaDe(cj).some(([x]) => x === cid)) gravarLista(cj, listaDe(cj).filter(([x]) => x !== cid));
      sel.carta = null; atual = null; break;
    }
    case 'restaurarCarta': delete ed.cartas[atual.id]; atual = cartaDe(atual.id); break;
    case 'restaurarConj': delete ed.conjuradores[atual.id]; atual = conjDe(atual.id); break;
    case 'restaurarBaralho': gravarLista(sel.baralho, clone(EST.baralhosOriginais[sel.baralho].lista)); break;
    case 'usarTextoGerado': atual.texto = textoCarta(atual); gravarCarta(atual); break;
    case 'usarTextoPoder': atual.habilidade = textoPoder(atual.poder); gravarConj(atual); break;
    case 'addEf': {
      const caminho = b.dataset.caminho;
      const efs = pegar(atual, caminho) ?? [];
      por(atual, caminho, [...efs, { tipo: 'dano', ...clone(EFEITOS.dano[2]) }]);
      gravarAtual(); break;
    }
    case 'remEf': {
      const caminho = b.dataset.caminho;
      const efs = [...(pegar(atual, caminho) ?? [])];
      efs.splice(Number(b.dataset.i), 1);
      const opcional = !caminho.includes('.');
      por(atual, caminho, efs.length || !opcional ? efs : undefined);
      gravarAtual(); break;
    }
    case 'aplicarTodos': {
      const vals = Object.fromEntries(Object.keys(ROT_CONJ).map((k) => [k, $(`#todos-${k}`).value.trim()]).filter(([, v]) => v !== '').map(([k, v]) => [k, Math.round(Number(v))]));
      if (!Object.keys(vals).length) return;
      for (const cid of idsConj()) gravarConj({ ...conjDe(cid), ...vals });
      if (atual && sel.conj) atual = conjDe(sel.conj);
      msg = { tipo: 'ok', html: `Aplicado a todos os Conjuradores: ${esc(Object.entries(vals).map(([k, v]) => `${ROT_CONJ[k]} ${v}`).join(', '))}. Falta salvar.` };
      break;
    }
    case 'addCarta': {
      const cid = $('#add-carta').value, n = Math.max(1, Math.round(Number($('#add-n').value) || 1));
      if (!cid) return;
      gravarLista(sel.baralho, [...listaDe(sel.baralho), [cid, n]]);
      break;
    }
    case 'simular': {
      if (pendente() && !confirm('Há alterações não salvas, e a simulação usa só o que está salvo. Simular mesmo assim?')) return;
      const r = await (await fetch('/editor/simular', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ porPar: Number($('#sim-n').value) }) })).json();
      if (!r.ok) { msg = { tipo: 'aviso', html: esc(r.erro) }; break; }
      acompanharSim(); return;
    }
    default: return;
  }
  // limpa listas de baralho com zero cópias
  for (const cj of idsConj()) { const l = listaDe(cj); if (l.some(([, n]) => n === 0) && a !== 'restaurarBaralho') gravarLista(cj, l.filter(([, n]) => n > 0)); }
  render();
});
window.addEventListener('beforeunload', (ev) => { if (pendente()) { ev.preventDefault(); ev.returnValue = ''; } });

try {
  await carregar();
  render();
  if (ultimaSim?.estado?.rodando) acompanharSim();
} catch (erro) {
  $('#app').innerHTML = `<div class="aviso">O editor precisa do servidor local (${esc(erro.message)}). Abra pelo <b>Arcanoclisma/tcg/app/INICIAR.bat</b> e entre em http://localhost:8765/editor.html.</div>`;
}
