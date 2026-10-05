// Construtor de efeitos do Editor (pedido do autor, 04/10): tabelas de condições e efeitos que se acumulam.
// Cada escolha vira a mecânica real da carta (campos `habilidades` e `protegido`, lidos pelo motor tcg/src/motor.ts),
// e o texto da carta é gerado sozinho (tcg/app/textos.js). O autor só polir o texto, se quiser.
// Conjuradores (04/10): o poder abre no mesmo construtor (ativação, gatilho e habilidades viram linhas; passivas e
// permissões ficam em blocos próprios) e volta ao formato do poder sem perder nada (tcg/src/poder-conjurador.ts).
import { esc } from './base.js';
import { textoHabilidade, textoProtegido, protegidoDe, textoPoder } from '/jogo/textos.js';
import { poderParaConstrutor, construtorParaPoder } from '/jogo/poder-conjurador.js';
import {
  CONDICOES, CONDICOES_CONJURADOR, EFEITOS, PALAVRAS, PALAVRAS_CONCESSAO, DESTINO_SAIDA, PASSIVAS_CONJ, CAMPOS_PERMISSAO,
  definirFichas, opcoesDe, chaveDe, novoEfeito, efeitosDisponiveis, avisoEfeito, novaPermissao, ehSecundario,
} from './construtor-tabelas.js';

export { CONDICOES, CONDICOES_CONJURADOR, EFEITOS, PALAVRAS };

/**
 * Desenha o construtor dentro de `caixa`. `mec` é a mecânica atual (objeto do JSON; no Conjurador, `{ poder }`);
 * `aoMudar(mec)` grava. `form` é o formulário do Editor (palavras-chave e Protegido das cartas ficam nos campos dele).
 * Devolve `{ definir(mec) }` para recarregar quando o JSON muda por fora (requisitos por linha, JSON avançado).
 */
export function montarConstrutor(caixa, mecInicial, { conjur, tipoCarta, form, aoMudar, fichas = [] }) {
  definirFichas(fichas);
  let mec, hs;
  /** Conjurador: `mec` é o poder no formato do construtor (habilidades + passivas + permissões). */
  const carregar = (m) => {
    mec = conjur ? poderParaConstrutor(m?.poder ?? null) : structuredClone(m ?? {});
    // Protegido unificado (04/10, noite): o nome antigo `protecao` ("Proteção mágica") abre dentro do Protegido
    if (!conjur && mec.protecao) { const pr = protegidoDe(mec); delete mec.protecao; gravarProtegido(pr ?? {}); }
    mec.habilidades ??= [];
    hs = mec.habilidades;
  };
  carregar(mecInicial);
  const condicoes = conjur ? CONDICOES_CONJURADOR : CONDICOES;
  const opc = (lista, v) => lista.map(([k, r]) => `<option value="${k}" ${String(v) === String(k) ? 'selected' : ''}>${esc(r)}</option>`).join('');
  const campoHTML = (c, valor, cam) => {
    if (c.tipo === 'num') return `<label>${esc(c.rot)} <input type="number" min="0" step="1" data-cam="${cam}" data-k="${c.k}" data-t="num" value="${valor ?? ''}"></label>`;
    if (c.tipo === 'txt') return `<label>${esc(c.rot)} <input type="text" data-cam="${cam}" data-k="${c.k}" data-t="txt" value="${esc(valor ?? '')}"></label>`;
    if (c.tipo === 'sel') return `<label>${esc(c.rot)} <select data-cam="${cam}" data-k="${c.k}" data-t="sel">${opc(opcoesDe(c), valor ?? c.padrao)}</select></label>`;
    if (c.tipo === 'conc') {
      const v = valor ?? {};
      const a = (ck) => `data-cam="${cam}" data-k="${c.k}" data-ck="${ck}" data-t="conc"`;
      return `<span class="cst-multi cst-conc">${esc(c.rot)}: ${PALAVRAS_CONCESSAO.map((p) => `<label class="opcao-linha"><input type="checkbox" ${a('palavras')} data-v="${p}" ${(v.palavras ?? []).includes(p) ? 'checked' : ''}> ${esc(p)}</label>`).join('')}
        <label>Palavras duram X turnos (vazio = permanentes) <input type="number" min="1" step="1" ${a('palavrasTurnos')} value="${v.palavrasTurnos ?? ''}"></label>
        <label>Ataque ± <input type="number" step="1" ${a('ataque')} value="${v.ataque ?? ''}"></label>
        <label>Vida ± <input type="number" step="1" ${a('vida')} value="${v.vida ?? ''}"></label>
        <label>Fica X turnos em campo (vazio = permanente) <input type="number" min="1" step="1" ${a('turnos')} value="${v.turnos ?? ''}"></label>
        <label>Ao sair vai para <select ${a('destino')}>${opc(DESTINO_SAIDA, v.destino ?? '')}</select></label></span>`;
    }
    if (c.tipo === 'bool') return `<label class="opcao-linha"><input type="checkbox" data-cam="${cam}" data-k="${c.k}" data-t="bool" ${valor ? 'checked' : ''}> ${esc(c.rot)}</label>`;
    return `<span class="cst-multi">${esc(c.rot)}: ${c.opcoes.map(([k, r]) => `<label class="opcao-linha"><input type="checkbox" data-cam="${cam}" data-k="${c.k}" data-t="multi" data-v="${k}" ${(valor ?? []).includes(k) ? 'checked' : ''}> ${esc(r)}</label>`).join('')}</span>`;
  };
  const efHTML = (ef, h, i) => {
    const chave = chaveDe(ef);
    if (!chave) return `<div class="cst-efeito"><span class="peq">Efeito fora da tabela (${esc(ef.tipo)}): edite no JSON.</span><button class="botao perigo peq" data-rm-ef="${h}.${i}">×</button></div>`;
    const t = EFEITOS[chave];
    const aviso = avisoEfeito(chave, hs[h].quando);
    // efeitos secundários (custos/consequências, 04/10 noite) num grupo à parte da lista
    const disp = efeitosDisponiveis(hs[h].quando, chave).map(([k, v]) => [k, v.rot]);
    const opcoes = opc(disp.filter(([k]) => !ehSecundario(k)), chave) + `<optgroup label="Efeito secundário">${opc(disp.filter(([k]) => ehSecundario(k)), chave)}</optgroup>`;
    return `<div class="cst-efeito${ehSecundario(chave) ? ' cst-secundario' : ''}"><select data-tipo-ef="${h}.${i}">${opcoes}</select>
      ${t.campos.map((c) => campoHTML(c, c.ler ? c.ler(ef) : ef[c.k], `${h}.${i}`)).join('')}
      ${aviso ? `<span class="peq falta">${esc(aviso)}</span>` : ''}
      <button class="botao perigo peq" data-rm-ef="${h}.${i}" title="Tirar este efeito">×</button></div>`;
  };
  const custoHTML = (h, idx) => h.quando !== 'ativada' ? '' : `<div class="cst-custo"><b>Custo:</b>
      <label class="opcao-linha"><input type="checkbox" data-custo="${idx}" data-k="suspender" ${h.custo?.suspender ? 'checked' : ''}> ${conjur ? 'Suspender o Conjurador' : 'Virar (suspender)'}</label>
      <label>Arcano <input type="number" min="0" data-custo="${idx}" data-k="arcano" value="${h.custo?.arcano ?? ''}"></label>
      <label>Vida <input type="number" min="0" data-custo="${idx}" data-k="vida" value="${h.custo?.vida ?? ''}"></label>
      <label>Descartar <input type="number" min="0" data-custo="${idx}" data-k="descartar" value="${h.custo?.descartar ?? ''}"></label>
      <label>Grau mínimo <input type="number" min="0" data-custo="${idx}" data-k="grau" value="${h.custo?.grau ?? ''}"></label>
      ${conjur ? `<label class="opcao-linha"><input type="checkbox" data-flag="${idx}" data-k="repentina" ${h.repentina ? 'checked' : ''}> Repentina (a qualquer momento, com prioridade)</label>` : ''}</div>`;
  /** Texto de uma linha: no Conjurador, o mesmo que o texto do poder gera para ela. */
  const previa = (h) => (conjur ? textoPoder(construtorParaPoder({ habilidades: [h] })) : textoHabilidade(h));
  const flagsHTML = (h, idx) => {
    if (h.quando === 'ativada' || (!conjur && (h.quando === 'jogar' || h.quando === 'entrar'))) return '';
    return `<label class="opcao-linha"><input type="checkbox" data-uma="${idx}" ${h.umaVezPorTurno ? 'checked' : ''}> Uma vez por turno</label>
      ${conjur ? `<label class="opcao-linha"><input type="checkbox" data-flag="${idx}" data-k="soNoSeuTurno" ${h.soNoSeuTurno ? 'checked' : ''}> Só no seu turno</label>` : ''}`;
  };
  /** Protegido da carta (04/10, noite: um conceito só): número quando só tem Arcano; objeto com as variações. */
  const pr = () => protegidoDe(mec) ?? {};
  function gravarProtegido(p) {
    for (const k of Object.keys(p)) if (!p[k]) delete p[k];
    if (!Object.keys(p).length) delete mec.protegido;
    else mec.protegido = Object.keys(p).length === 1 && p.arcano ? p.arcano : p;
  }
  const palavrasForm = () => ($('[data-f="palavrasChave"]')?.value ?? '').split(',').map((x) => x.trim()).filter(Boolean);
  function $(s) { return form.querySelector(s); }
  const condicoesDe = (q) => (condicoes.some(([k]) => k === q) ? condicoes : [...condicoes, [q, q]]);
  const habsHTML = () => `${hs.map((h, idx) => `<div class="cst-hab">
        <div class="cst-cab"><select data-quando="${idx}">${opc(condicoesDe(h.quando), h.quando)}</select>
          ${flagsHTML(h, idx)}
          <button class="botao perigo peq" data-rm-hab="${idx}">${conjur ? 'Tirar habilidade' : 'Tirar condição'}</button></div>
        ${custoHTML(h, idx)}
        ${(h.efeitos ?? []).map((ef, i) => efHTML(ef, idx, i)).join('')}
        ${(h.efeitos ?? []).length ? '' : `<p class="peq falta">${conjur ? 'Habilidade' : 'Condição'} sem efeitos: não faz nada. Acrescente um efeito ou use "${conjur ? 'Tirar habilidade' : 'Tirar condição'}".</p>`}
        <button class="botao peq" data-add-ef="${idx}">+ efeito</button>
        <button class="botao peq" data-add-sec="${idx}" title="Custo ou consequência depois dos efeitos (dano no seu Conjurador, descarte, sacrificar um Aliado, Grau ao cemitério)">+ efeito secundário</button>
        <p class="peq cst-previa">${esc(previa(h))}</p></div>`).join('')}`;
  const permHTML = () => (mec.permiteJogarDe ?? []).map((pm, i) => `<div class="cst-efeito">${CAMPOS_PERMISSAO.map((c) => campoHTML(c, pm[c.k], `perm.${i}`)).join('')}
        <button class="botao perigo peq" data-rm-perm="${i}" title="Tirar esta permissão">×</button></div>`).join('');
  function desenhar() {
    if (conjur) {
      caixa.innerHTML = `
        ${habsHTML()}
        <div class="linha-botoes"><button class="botao peq" data-add-hab="ativada">+ habilidade ativada</button><button class="botao peq" data-add-hab="aliadoEntra">+ gatilho</button></div>
        <div class="cst-bloco"><b>Passivas</b> <span class="peq">(valem sempre; vazio = não tem)</span>
          ${PASSIVAS_CONJ.map(([k, r]) => `<label>${esc(r)} <input type="number" min="0" data-passiva="${k}" value="${mec[k] ?? ''}"></label>`).join('')}</div>
        <div class="cst-bloco"><b>Permissões</b> <span class="peq">(pode jogar cartas destas zonas, pagando o custo)</span>
          ${permHTML()}
          <button class="botao peq" data-add-perm>+ permissão</button></div>
        <p class="peq cst-previa-total"><b>Texto gerado:</b> ${esc(textoPoder(construtorParaPoder(mec)) || '—')}</p>`;
      return;
    }
    const pk = palavrasForm();
    caixa.innerHTML = `
      <div class="cst-bloco"><b>Palavras-chave</b> ${PALAVRAS.filter(([k]) => k !== 'Repentino' || tipoCarta === 'aliado').map(([k, r]) => `<label class="opcao-linha"><input type="checkbox" data-palavra="${k}" ${pk.includes(k) ? 'checked' : ''}> ${esc(r)}</label>`).join('')}</div>
      ${habsHTML()}
      <button class="botao peq" data-add-hab>+ condição (habilidade)</button>
      <div class="cst-bloco"><b>Protegido</b> <span class="peq">(o oponente que mirar esta carta paga; só Arcano = "Protegido N")</span>
        ${[['arcano', 'Arcano (N)'], ['vida', 'Vida'], ['descartar', 'Descartar cartas'], ['grauParaCemiterio', 'Cartas do Grau ao cemitério'], ['grauMin', 'Grau mínimo']].map(([k, r]) => `<label>${r} <input type="number" min="0" data-prot="${k}" value="${pr()[k] ?? ''}"></label>`).join('')}
        ${mec.protegido ? `<p class="peq cst-previa">${esc(textoProtegido(protegidoDe(mec)))}</p>` : ''}</div>`;
  }
  /** O que vai para o Editor: a mecânica da carta, ou `{ poder }` do Conjurador no formato do motor. */
  const saida = () => {
    if (conjur) return { poder: construtorParaPoder(mec) };
    const m = structuredClone(mec);
    if (!hs.length) delete m.habilidades;
    return m;
  };
  function gravar(redesenhar) {
    aoMudar(saida());
    if (redesenhar) desenhar();
    else {
      for (const [idx, h] of hs.entries()) { const p = caixa.querySelectorAll('.cst-previa')[idx]; if (p) p.textContent = previa(h); }
      const tot = caixa.querySelector('.cst-previa-total');
      if (tot) tot.innerHTML = `<b>Texto gerado:</b> ${esc(textoPoder(construtorParaPoder(mec)) || '—')}`;
    }
  }
  /** Objeto editado por um campo: efeito "h.i" ou permissão "perm.i". */
  const alvoDe = (cam) => { const [a, b] = cam.split('.'); return a === 'perm' ? mec.permiteJogarDe[+b] : hs[+a].efeitos[+b]; };
  caixa.addEventListener('change', (ev) => {
    const t = ev.target;
    if (t.dataset.palavra) {
      const pk = palavrasForm().filter((x) => x !== t.dataset.palavra);
      if (t.checked) pk.push(t.dataset.palavra);
      const inp = $('[data-f="palavrasChave"]');
      if (inp) { inp.value = pk.join(', '); inp.dispatchEvent(new Event('change', { bubbles: true })); }
      return;
    }
    if (t.dataset.quando !== undefined) {
      const h = hs[+t.dataset.quando]; h.quando = t.value;
      if (h.quando !== 'ativada') { delete h.custo; delete h.repentina; } else h.custo ??= { suspender: true };
      if (h.quando === 'ativada') { delete h.umaVezPorTurno; delete h.soNoSeuTurno; }
      return gravar(true);
    }
    if (t.dataset.uma !== undefined) { if (t.checked) hs[+t.dataset.uma].umaVezPorTurno = true; else delete hs[+t.dataset.uma].umaVezPorTurno; return gravar(false); }
    if (t.dataset.flag !== undefined) { const h = hs[+t.dataset.flag]; if (t.checked) h[t.dataset.k] = true; else delete h[t.dataset.k]; return gravar(false); }
    if (t.dataset.tipoEf) { const [h, i] = t.dataset.tipoEf.split('.').map(Number); const req = hs[h].efeitos[i].requisito; hs[h].efeitos[i] = { ...novoEfeito(t.value), ...(req ? { requisito: req } : {}) }; return gravar(true); }
    if (t.dataset.custo !== undefined) {
      const h = hs[+t.dataset.custo]; h.custo ??= {};
      if (t.type === 'checkbox') h.custo[t.dataset.k] = t.checked; else { const v = Math.max(0, Math.trunc(+t.value || 0)); if (v) h.custo[t.dataset.k] = v; else delete h.custo[t.dataset.k]; }
      return gravar(false);
    }
    if (t.dataset.passiva) {
      const v = Math.max(0, Math.trunc(+t.value || 0));
      if (v) mec[t.dataset.passiva] = v; else delete mec[t.dataset.passiva];
      return gravar(false);
    }
    if (t.dataset.prot) {
      const p = { ...pr() };
      p[t.dataset.prot] = Math.max(0, Math.trunc(+t.value || 0));
      gravarProtegido(p);
      return gravar(true);
    }
    if (t.dataset.cam) {
      const e = alvoDe(t.dataset.cam), k = t.dataset.k;
      if (t.dataset.t === 'conc') {
        const cc = (e[k] ??= {}), ck = t.dataset.ck;
        if (ck === 'palavras') { const st = new Set(cc.palavras ?? []); if (t.checked) st.add(t.dataset.v); else st.delete(t.dataset.v); if (st.size) cc.palavras = [...st]; else delete cc.palavras; }
        else if (ck === 'destino') { if (t.value) cc.destino = t.value; else delete cc.destino; }
        else { const v = Math.trunc(+t.value || 0); if (v && ((ck !== 'turnos' && ck !== 'palavrasTurnos') || v > 0)) cc[ck] = v; else delete cc[ck]; }
        if (!Object.keys(cc).length) delete e[k];
        return gravar(false);
      }
      if (e.tipo === 'invocar' && k === 'carta') { e.nome = (opcoesDe(EFEITOS.invocar.campos[0]).find(([id]) => id === t.value) ?? [])[1] ?? e.nome; }
      if (t.dataset.t === 'num') { if (t.value === '') delete e[k]; else e[k] = Math.max(0, Math.trunc(+t.value)); }
      else if (t.dataset.t === 'sel') { e[k] = t.value; if (k === 'modo') delete e.aleatorio; }
      else if (t.dataset.t === 'txt') { if (t.value.trim()) e[k] = t.value.trim(); else delete e[k]; }
      else if (t.dataset.t === 'bool') { if (t.checked) e[k] = true; else delete e[k]; }
      else if (t.dataset.t === 'multi') { const s = new Set(e[k] ?? []); if (t.checked) s.add(t.dataset.v); else s.delete(t.dataset.v); e[k] = [...s]; if (t.dataset.cam.startsWith('perm.') && !e[k].length) delete e[k]; }
      return gravar(false);
    }
  });
  // não deixa o "input" das tabelas disparar a leitura do formulário antes da hora (o change acima grava)
  caixa.addEventListener('input', (ev) => ev.stopPropagation());
  caixa.addEventListener('click', (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    ev.preventDefault();
    if (b.dataset.addHab !== undefined) {
      if (conjur) hs.push(b.dataset.addHab === 'ativada' ? { quando: 'ativada', custo: { suspender: true }, efeitos: [novoEfeito('comprar')] } : { quando: b.dataset.addHab, umaVezPorTurno: true, efeitos: [novoEfeito('arcano')] });
      else hs.push({ quando: tipoCarta === 'temporaria' ? 'jogar' : 'entrar', efeitos: [novoEfeito('dano')] });
      return gravar(true);
    }
    if (b.dataset.rmHab !== undefined) { hs.splice(+b.dataset.rmHab, 1); return gravar(true); }
    if (b.dataset.addEf !== undefined) { hs[+b.dataset.addEf].efeitos.push(novoEfeito('comprar')); return gravar(true); }
    if (b.dataset.addSec !== undefined) { hs[+b.dataset.addSec].efeitos.push(novoEfeito('secDano')); return gravar(true); }
    if (b.dataset.rmEf !== undefined) { const [h, i] = b.dataset.rmEf.split('.').map(Number); hs[h].efeitos.splice(i, 1); return gravar(true); }
    if (b.dataset.addPerm !== undefined) { (mec.permiteJogarDe ??= []).push(novaPermissao()); return gravar(true); }
    if (b.dataset.rmPerm !== undefined) { mec.permiteJogarDe.splice(+b.dataset.rmPerm, 1); if (!mec.permiteJogarDe.length) delete mec.permiteJogarDe; return gravar(true); }
  });
  desenhar();
  return { definir(m) { carregar(m); desenhar(); } };
}
