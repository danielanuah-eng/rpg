// Construtor de efeitos do Editor (pedido do autor, 04/10): tabelas de condições e efeitos que se acumulam.
// Cada escolha vira a mecânica real da carta (campo `habilidades` e `protecao`, lidos pelo motor tcg/src/motor.ts),
// e o texto da carta é gerado sozinho (tcg/app/textos.js). O autor só polir o texto, se quiser.
import { esc } from './base.js';
import { textoHabilidade, textoProtecao } from '/jogo/textos.js';

const TIPOS_CAMPO = [['aliado', 'Aliado'], ['mistica', 'Mística'], ['estrutura', 'Estrutura'], ['conjurador', 'Conjurador'], ['equipamento', 'Equipamento']];
const DESTINOS = [['mao', 'Mão'], ['grau', 'Grau'], ['cemiterio', 'Cemitério'], ['fundo', 'Fundo do baralho'], ['topo', 'Topo do baralho'], ['campo', 'Campo (jogar)'], ['banimento', 'Banimento']];
const LADO = [['inimigo', 'Do oponente'], ['proprio', 'Seu'], ['ambos', 'Dos dois']];
const ALCANCE = [['alvo', 'Um alvo'], ['todos', 'Todos (atinge tudo)']];
const DURACAO = [['turno', 'Neste turno'], ['permanente', 'Permanente']];

/** Tabela de condições ("quando"). */
export const CONDICOES = [
  ['jogar', 'Ao jogar (Temporária)'],
  ['entrar', 'Quando entra em campo'],
  ['ativada', 'Virar / pagar (habilidade ativada)'],
  ['alvo', 'Quando vira alvo'],
  ['grau', 'Quando é colocada no Grau'],
  ['atacar', 'Quando ataca'],
  ['bloquear', 'Quando defende (bloqueia)'],
  ['cemiterio', 'Quando cai no cemitério'],
  ['banimento', 'Quando é banida'],
  ['danoConjurador', 'Quando causa dano de combate ao Conjurador inimigo'],
];

/** Campo do formulário de um efeito: [chave, rótulo, tipo, opções/padrão]. */
const n = (k, rot, padrao = 1) => ({ k, rot, tipo: 'num', padrao });
const sel = (k, rot, opcoes, padrao = opcoes[0][0]) => ({ k, rot, tipo: 'sel', opcoes, padrao });
const multi = (k, rot, opcoes, padrao) => ({ k, rot, tipo: 'multi', opcoes, padrao });
const bool = (k, rot) => ({ k, rot, tipo: 'bool', padrao: false });
const ESCOLHA = [['controlador', 'Quem jogou/ativou escolhe'], ['afetado', 'O jogador atingido escolhe'], ['aleatorio', 'Ao acaso']];
const campoPadrao = (acao, extra = []) => ({ fixo: { tipo: 'campo', acao }, campos: [...extra, sel('alcance', 'Quem', ALCANCE), sel('escolha', 'Quem escolhe o alvo', ESCOLHA), sel('lado', 'Lado', LADO, acao === 'curar' || acao.startsWith('aumentar') || acao === 'levantar' ? 'proprio' : 'inimigo'), multi('tipos', 'Tipos', TIPOS_CAMPO, ['aliado'])] });

/** Tabela de efeitos (rótulo → mecânica do motor). */
export const EFEITOS = {
  dano: { rot: 'Dar dano', ...campoPadrao('dano', [n('valor', 'Quanto', 2)]) },
  curar: { rot: 'Curar', ...campoPadrao('curar', [n('valor', 'Quanto', 2)]) },
  destruir: { rot: 'Destruir', ...campoPadrao('destruir') },
  devolver: { rot: 'Voltar para a mão do dono', ...campoPadrao('devolver') },
  paraBaralho: { rot: 'Voltar para o baralho', ...campoPadrao('paraBaralho', [sel('onde', 'Onde', [['embaralhar', 'Embaralhar'], ['fundo', 'Fundo'], ['topo', 'Topo']])]) },
  naoAtaca: { rot: 'Proibir de atacar (até o próximo turno do dono)', ...campoPadrao('naoAtaca') },
  suspender: { rot: 'Virar (suspender)', ...campoPadrao('suspender') },
  levantar: { rot: 'Levantar', ...campoPadrao('levantar') },
  aumentarAtaque: { rot: 'Aumentar Ataque', ...campoPadrao('aumentarAtaque', [n('valor', 'Quanto', 1), sel('duracao', 'Duração', DURACAO)]) },
  aumentarVida: { rot: 'Aumentar Vida', ...campoPadrao('aumentarVida', [n('valor', 'Quanto', 2), sel('duracao', 'Duração', DURACAO)]) },
  comprar: { rot: 'Comprar cartas', fixo: { tipo: 'comprar' }, campos: [n('valor', 'Quantas', 1)] },
  arcano: { rot: 'Gerar Arcano', fixo: { tipo: 'arcano' }, campos: [n('valor', 'Quanto', 1)] },
  arcanoOponente: { rot: 'O oponente ganha Arcano', fixo: { tipo: 'arcanoOponente' }, campos: [n('valor', 'Quanto', 1)] },
  olharTopoPara: { rot: 'Olhar cartas do topo do baralho', fixo: { tipo: 'olharTopoPara' }, campos: [n('quantidade', 'Olhar', 3), n('pegar', 'Escolher', 1), sel('destino', 'As escolhidas vão para', DESTINOS), sel('resto', 'O resto vai para', [['fundo', 'Fundo do baralho'], ['cemiterio', 'Cemitério'], ['topo', 'Topo (mesma ordem)'], ['grau', 'Grau'], ['banimento', 'Banimento']])] },
  descartar: { rot: 'Jogador descarta cartas', fixo: { tipo: 'descartar' }, campos: [sel('quem', 'Quem descarta', [['oponente', 'O oponente'], ['voce', 'Quem jogou']]), n('valor', 'Quantas', 1), bool('aleatorio', 'Ao acaso')] },
  colocarNoGrau: { rot: 'Colocar carta extra no Grau', fixo: { tipo: 'colocarNoGrau' }, campos: [sel('de', 'De onde', [['topo', 'Topo do baralho'], ['mao', 'Uma da mão (você escolhe)']]), n('valor', 'Quantas', 1)] },
  olharMao: { rot: 'Olhar a mão do oponente', fixo: { tipo: 'olharMao' }, campos: [n('pegar', 'Escolher', 1), sel('destino', 'Fazer o quê com as escolhidas', [['cemiterio', 'Descartar (cemitério)'], ['roubar', 'Pegar para você'], ['fundo', 'Mandar para o fundo do baralho'], ['nada', 'Só olhar']])] },
  jogarDe: { rot: 'Jogar carta de uma zona (sem pagar)', fixo: { tipo: 'jogarDe' }, campos: [sel('de', 'De onde', [['cemiterio', 'Cemitério'], ['mao', 'Mão'], ['baralho', 'Baralho'], ['banimento', 'Banimento']]), n('valor', 'Quantas', 1), multi('tipos', 'Tipos', [['aliado', 'Aliado'], ['mistica', 'Mística'], ['estrutura', 'Estrutura']], ['aliado']), n('custoMax', 'Custo máximo (vazio = qualquer)', '')] },
  perderVida: { rot: 'Seu Conjurador perde Vida', fixo: { tipo: 'perderVida' }, campos: [n('valor', 'Quanto', 1)] },
  anular: { rot: 'Anular uma carta na pilha', fixo: { tipo: 'anular', alvo: 'pilha' }, campos: [] },
  descontoProximaCarta: { rot: 'Desconto na próxima carta do turno', fixo: { tipo: 'descontoProximaCarta' }, campos: [n('valor', 'Quanto', 1)] },
};
/** Palavras-chave existentes (ligadas aos campos palavrasChave/protegido do formulário). */
export const PALAVRAS = [['Proficiente', 'Proficiente'], ['Etéreo', 'Etéreo'], ['Incomensurável', 'Incomensurável'], ['Repentino', 'Repentino (Aliado)']];

/** Qual linha da tabela representa este efeito da mecânica (para reabrir no formulário). */
function chaveDe(ef) {
  if (ef.tipo === 'campo') return ef.acao;
  if (EFEITOS[ef.tipo]) return ef.tipo;
  return null; // efeito antigo fora da tabela (fica no JSON)
}
function novoEfeito(chave) {
  const t = EFEITOS[chave];
  const ef = { ...t.fixo };
  for (const c of t.campos) if (c.padrao !== '' && c.padrao !== false) ef[c.k] = Array.isArray(c.padrao) ? [...c.padrao] : c.padrao;
  return ef;
}

/**
 * Desenha o construtor dentro de `caixa`. `mec` é a mecânica atual (objeto do JSON); `aoMudar(mec)` grava.
 * `form` é o formulário do Editor (palavras-chave e Protegido ficam nos campos dele).
 */
export function montarConstrutor(caixa, mecInicial, { conjur, tipoCarta, form, aoMudar }) {
  if (conjur) { caixa.innerHTML = '<p class="peq">Conjuradores usam o JSON do poder (abaixo).</p>'; return; }
  let mec = structuredClone(mecInicial ?? {});
  let hs = mec.habilidades ?? [];
  const opc = (lista, v) => lista.map(([k, r]) => `<option value="${k}" ${String(v) === String(k) ? 'selected' : ''}>${esc(r)}</option>`).join('');
  const campoHTML = (c, valor, cam) => {
    if (c.tipo === 'num') return `<label>${esc(c.rot)} <input type="number" min="0" step="1" data-cam="${cam}" data-k="${c.k}" data-t="num" value="${valor ?? ''}"></label>`;
    if (c.tipo === 'sel') return `<label>${esc(c.rot)} <select data-cam="${cam}" data-k="${c.k}" data-t="sel">${opc(c.opcoes, valor ?? c.padrao)}</select></label>`;
    if (c.tipo === 'bool') return `<label class="opcao-linha"><input type="checkbox" data-cam="${cam}" data-k="${c.k}" data-t="bool" ${valor ? 'checked' : ''}> ${esc(c.rot)}</label>`;
    return `<span class="cst-multi">${esc(c.rot)}: ${c.opcoes.map(([k, r]) => `<label class="opcao-linha"><input type="checkbox" data-cam="${cam}" data-k="${c.k}" data-t="multi" data-v="${k}" ${(valor ?? []).includes(k) ? 'checked' : ''}> ${esc(r)}</label>`).join('')}</span>`;
  };
  const efHTML = (ef, h, i) => {
    const chave = chaveDe(ef);
    if (!chave) return `<div class="cst-efeito"><span class="peq">Efeito fora da tabela (${esc(ef.tipo)}): edite no JSON.</span><button class="botao perigo peq" data-rm-ef="${h}.${i}">×</button></div>`;
    const t = EFEITOS[chave];
    return `<div class="cst-efeito"><select data-tipo-ef="${h}.${i}">${opc(Object.entries(EFEITOS).map(([k, v]) => [k, v.rot]), chave)}</select>
      ${t.campos.map((c) => campoHTML(c, ef[c.k], `${h}.${i}`)).join('')}
      <button class="botao perigo peq" data-rm-ef="${h}.${i}" title="Tirar este efeito">×</button></div>`;
  };
  const custoHTML = (h, idx) => h.quando !== 'ativada' ? '' : `<div class="cst-custo"><b>Custo:</b>
      <label class="opcao-linha"><input type="checkbox" data-custo="${idx}" data-k="suspender" ${h.custo?.suspender ? 'checked' : ''}> Virar (suspender)</label>
      <label>Arcano <input type="number" min="0" data-custo="${idx}" data-k="arcano" value="${h.custo?.arcano ?? ''}"></label>
      <label>Vida <input type="number" min="0" data-custo="${idx}" data-k="vida" value="${h.custo?.vida ?? ''}"></label>
      <label>Descartar <input type="number" min="0" data-custo="${idx}" data-k="descartar" value="${h.custo?.descartar ?? ''}"></label>
      <label>Grau mínimo <input type="number" min="0" data-custo="${idx}" data-k="grau" value="${h.custo?.grau ?? ''}"></label></div>`;
  const pr = () => mec.protecao ?? {};
  const palavrasForm = () => ($('[data-f="palavrasChave"]')?.value ?? '').split(',').map((x) => x.trim()).filter(Boolean);
  function $(sel) { return form.querySelector(sel); }
  function desenhar() {
    const pk = palavrasForm();
    caixa.innerHTML = `
      <div class="cst-bloco"><b>Palavras-chave</b> ${PALAVRAS.filter(([k]) => k !== 'Repentino' || tipoCarta === 'aliado').map(([k, r]) => `<label class="opcao-linha"><input type="checkbox" data-palavra="${k}" ${pk.includes(k) ? 'checked' : ''}> ${esc(r)}</label>`).join('')}</div>
      ${hs.map((h, idx) => `<div class="cst-hab">
        <div class="cst-cab"><select data-quando="${idx}">${opc(CONDICOES, h.quando)}</select>
          ${h.quando !== 'ativada' && h.quando !== 'jogar' && h.quando !== 'entrar' ? `<label class="opcao-linha"><input type="checkbox" data-uma="${idx}" ${h.umaVezPorTurno ? 'checked' : ''}> Uma vez por turno</label>` : ''}
          <button class="botao perigo peq" data-rm-hab="${idx}">Tirar condição</button></div>
        ${custoHTML(h, idx)}
        ${(h.efeitos ?? []).map((ef, i) => efHTML(ef, idx, i)).join('')}
        <button class="botao peq" data-add-ef="${idx}">+ efeito</button>
        <p class="peq cst-previa">${esc(textoHabilidade(h))}</p></div>`).join('')}
      <button class="botao peq" data-add-hab>+ condição (habilidade)</button>
      <div class="cst-bloco"><b>Proteção mágica</b> <span class="peq">(quem mirar esta carta paga)</span>
        ${[['arcano', 'Arcano'], ['vida', 'Vida'], ['descartar', 'Descartar cartas'], ['grauParaCemiterio', 'Cartas do Grau ao cemitério'], ['grauMin', 'Grau mínimo']].map(([k, r]) => `<label>${r} <input type="number" min="0" data-prot="${k}" value="${pr()[k] ?? ''}"></label>`).join('')}
        ${mec.protecao ? `<p class="peq cst-previa">${esc(textoProtecao(mec.protecao))}</p>` : ''}</div>`;
  }
  function gravar(redesenhar) {
    if (hs.length) mec.habilidades = hs; else delete mec.habilidades;
    aoMudar(structuredClone(mec));
    if (redesenhar) desenhar();
    else for (const [idx, h] of hs.entries()) { const p = caixa.querySelectorAll('.cst-previa')[idx]; if (p) p.textContent = textoHabilidade(h); }
  }
  const ef = (cam) => { const [h, i] = cam.split('.').map(Number); return hs[h].efeitos[i]; };
  caixa.addEventListener('change', (ev) => {
    const t = ev.target;
    if (t.dataset.palavra) {
      const pk = palavrasForm().filter((x) => x !== t.dataset.palavra);
      if (t.checked) pk.push(t.dataset.palavra);
      const inp = $('[data-f="palavrasChave"]');
      if (inp) { inp.value = pk.join(', '); inp.dispatchEvent(new Event('change', { bubbles: true })); }
      return;
    }
    if (t.dataset.quando !== undefined) { const h = hs[+t.dataset.quando]; h.quando = t.value; if (h.quando !== 'ativada') delete h.custo; else h.custo ??= { suspender: true }; return gravar(true); }
    if (t.dataset.uma !== undefined) { if (t.checked) hs[+t.dataset.uma].umaVezPorTurno = true; else delete hs[+t.dataset.uma].umaVezPorTurno; return gravar(false); }
    if (t.dataset.tipoEf) { const [h, i] = t.dataset.tipoEf.split('.').map(Number); const req = hs[h].efeitos[i].requisito; hs[h].efeitos[i] = { ...novoEfeito(t.value), ...(req ? { requisito: req } : {}) }; return gravar(true); }
    if (t.dataset.custo !== undefined) {
      const h = hs[+t.dataset.custo]; h.custo ??= {};
      if (t.type === 'checkbox') h.custo[t.dataset.k] = t.checked; else { const v = Math.max(0, Math.trunc(+t.value || 0)); if (v) h.custo[t.dataset.k] = v; else delete h.custo[t.dataset.k]; }
      return gravar(false);
    }
    if (t.dataset.prot) {
      mec.protecao ??= {};
      const v = Math.max(0, Math.trunc(+t.value || 0));
      if (v) mec.protecao[t.dataset.prot] = v; else delete mec.protecao[t.dataset.prot];
      if (!Object.keys(mec.protecao).length) delete mec.protecao;
      return gravar(true);
    }
    if (t.dataset.cam) {
      const e = ef(t.dataset.cam), k = t.dataset.k;
      if (t.dataset.t === 'num') { if (t.value === '') delete e[k]; else e[k] = Math.max(0, Math.trunc(+t.value)); }
      else if (t.dataset.t === 'sel') e[k] = t.value;
      else if (t.dataset.t === 'bool') { if (t.checked) e[k] = true; else delete e[k]; }
      else if (t.dataset.t === 'multi') { const s = new Set(e[k] ?? []); if (t.checked) s.add(t.dataset.v); else s.delete(t.dataset.v); e[k] = [...s]; }
      return gravar(false);
    }
  });
  // não deixa o "input" das tabelas disparar a leitura do formulário antes da hora (o change acima grava)
  caixa.addEventListener('input', (ev) => ev.stopPropagation());
  caixa.addEventListener('click', (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    ev.preventDefault();
    if (b.dataset.addHab !== undefined) { hs.push({ quando: tipoCarta === 'temporaria' ? 'jogar' : 'entrar', efeitos: [novoEfeito('dano')] }); return gravar(true); }
    if (b.dataset.rmHab !== undefined) { hs.splice(+b.dataset.rmHab, 1); return gravar(true); }
    if (b.dataset.addEf !== undefined) { hs[+b.dataset.addEf].efeitos.push(novoEfeito('comprar')); return gravar(true); }
    if (b.dataset.rmEf !== undefined) { const [h, i] = b.dataset.rmEf.split('.').map(Number); hs[h].efeitos.splice(i, 1); return gravar(true); }
  });
  desenhar();
}
