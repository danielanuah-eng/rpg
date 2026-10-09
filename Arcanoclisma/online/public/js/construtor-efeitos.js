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
  QUANDO_EVENTO, QUEM_EVENTO, ETAPAS, TURNOS, LADO_AURA, TIPOS_AURA, DE_QUEM_CUSTO, TIPOS_CUSTO, CAMPOS_CUSTO, QUEM_PAGA, QUANDO_PAGA,
  SENAO_EFEITO, SENAO_MANUT, DESTINO_MANUT,
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
  // ---------- caminhos (05/10): listas de efeitos aninhadas ----------
  // Lista: "3" = efeitos da habilidade 3; "p3" = efeitos alternativos do custo do gatilho; "m3" = alternativos da
  // manutenção; "e<efeito>" = alternativos do custo de um efeito. Efeito: "<lista>/<índice>" (ex.: "0/1", "e0/1/0").
  const habDe = (cam) => +String(cam).match(/\d+/)[0];
  function listaDe(lp, criar = false) {
    const pre = lp[0];
    if (pre === 'p' || pre === 'm' || pre === 'e') {
      const dono = pre === 'e' ? efeitoDe(lp.slice(1)) : hs[+lp.slice(1)];
      const k = pre === 'p' ? 'pagar' : pre === 'm' ? 'manutencao' : 'custo';
      if (!dono || (!dono[k] && !criar)) return [];
      dono[k] ??= {};
      if (!dono[k].alternativos && !criar) return [];
      return (dono[k].alternativos ??= []);
    }
    return hs[+lp]?.efeitos ?? [];
  }
  function efeitoDe(cam) { const k = cam.lastIndexOf('/'); return listaDe(cam.slice(0, k))[+cam.slice(k + 1)]; }
  /** Objeto de custo pela chave: "p3" (gatilho), "m3" (manutenção), "e0/1" (efeito). */
  const custoDe = (ch) => (ch[0] === 'p' ? hs[+ch.slice(1)].pagar : ch[0] === 'm' ? hs[+ch.slice(1)].manutencao : efeitoDe(ch.slice(1))?.custo);
  /** Campos de um custo condicional (o que se paga, quem paga, quando e o que acontece sem ele). */
  const custoCondHTML = (c, ch, { quando = false, senao = SENAO_EFEITO, alt = true } = {}) => `
      <span class="cst-multi">${CAMPOS_CUSTO.map(([k, r]) => `<label>${r} <input type="number" min="0" step="1" data-cc="${ch}" data-k="${k}" value="${c[k] ?? ''}"></label>`).join('')}</span>
      <label>Quem paga <select data-cs="${ch}" data-k="quem">${opc(QUEM_PAGA, c.quem ?? 'voce')}</select></label>
      ${quando ? `<label>Quando paga <select data-cs="${ch}" data-k="quando">${opc(QUANDO_PAGA, c.quando ?? 'resolver')}</select></label>` : ''}
      <label>${(c.quem ?? 'voce') === 'oponente' ? 'Se o oponente pagar' : 'Se não pagar'} <select data-cs="${ch}" data-k="senao">${opc(senao, c.senao ?? senao[0][0])}</select></label>
      ${(c.senao ?? senao[0][0]) === 'sai' ? `<label>A carta vai para <select data-cs="${ch}" data-k="destino">${opc(DESTINO_MANUT, c.destino ?? 'cemiterio')}</select></label>` : ''}
      ${alt && c.senao === 'alternativo' ? `<div class="cst-alt"><b class="peq">Efeitos alternativos:</b>${listaDe(ch).map((ef, i) => efHTML(ef, `${ch}/${i}`)).join('')}<button class="botao peq" data-add-ef="${ch}">+ efeito alternativo</button></div>` : ''}
      ${CAMPOS_CUSTO.some(([k]) => c[k]) ? '' : '<span class="peq falta">Custo vazio: preencha o que se paga.</span>'}`;
  const efHTML = (ef, cam) => {
    const quando = hs[habDe(cam)].quando;
    const chave = chaveDe(ef);
    if (!chave) return `<div class="cst-efeito"><span class="peq">Efeito fora da tabela (${esc(ef.tipo)}): edite no JSON.</span><button class="botao perigo peq" data-rm-ef="${cam}">×</button></div>`;
    const t = EFEITOS[chave];
    const aviso = avisoEfeito(chave, quando);
    // efeitos secundários (custos/consequências, 04/10 noite) num grupo à parte da lista
    const disp = efeitosDisponiveis(quando, chave).map(([k, v]) => [k, v.rot]);
    const opcoes = opc(disp.filter(([k]) => !ehSecundario(k)), chave) + `<optgroup label="Efeito secundário">${opc(disp.filter(([k]) => ehSecundario(k)), chave)}</optgroup>`;
    // custo do efeito ("pague para funcionar", 05/10); "ao jogar" só nas condições ao jogar / ao entrar
    const custo = ef.custo
      ? `<div class="cst-custo cst-custo-efeito"><b>Custo para esta linha funcionar:</b>${custoCondHTML(ef.custo, `e${cam}`, { quando: /^\d+\/\d+$/.test(cam) &&(quando === 'jogar' || quando === 'entrar') })}
          <button class="botao perigo peq" data-rm-custo="${cam}" title="Tirar o custo">× custo</button></div>`
      : `<button class="botao peq" data-add-custo="${cam}" title="Pague para funcionar: você paga para a linha acontecer, ou o oponente paga para impedir">+ custo</button>`;
    return `<div class="cst-efeito${ehSecundario(chave) ? ' cst-secundario' : ''}"><select data-tipo-ef="${cam}">${opcoes}</select>
      ${t.campos.map((c) => campoHTML(c, c.ler ? c.ler(ef) : ef[c.k], cam)).join('')}
      ${aviso ? `<span class="peq falta">${esc(aviso)}</span>` : ''}
      <button class="botao perigo peq" data-rm-ef="${cam}" title="Tirar este efeito">×</button>
      ${custo}</div>`;
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
  /** Gatilhos: "De quem" (eventos de jogador), "Vezes por turno" (1 = uma vez por turno) e "Só no seu turno" (Conjurador). */
  const flagsHTML = (h, idx) => {
    if (h.quando === 'ativada' || h.quando === 'emCampo' || (!conjur && (h.quando === 'jogar' || h.quando === 'entrar'))) return '';
    const vezes = h.vezesPorTurno ?? (h.umaVezPorTurno ? 1 : '');
    return `${QUANDO_EVENTO.includes(h.quando) ? `<label>De quem <select data-hsel="${idx}" data-k="quem">${opc(QUEM_EVENTO, h.quem ?? 'qualquer')}</select></label>` : ''}
      <label title="1 = uma vez por turno">Vezes por turno <input type="number" min="1" step="1" data-vezes="${idx}" value="${vezes}" placeholder="sem limite"></label>
      ${conjur ? `<label class="opcao-linha"><input type="checkbox" data-flag="${idx}" data-k="soNoSeuTurno" ${h.soNoSeuTurno ? 'checked' : ''}> Só no seu turno</label>` : ''}`;
  };
  /** Custo do gatilho (05/10): quando dispara, quem paga decide; sem o custo, os efeitos não acontecem. */
  const pagarHTML = (h, idx) => {
    if (h.quando === 'ativada' || (h.quando === 'emCampo' && (h.modo ?? 'passivo') === 'passivo')) return '';
    if (!h.pagar) return `<button class="botao peq" data-add-pagar="${idx}" title="Quando dispara, alguém pode pagar: você para acontecer, ou o oponente para impedir">+ custo para acontecer</button>`;
    return `<div class="cst-custo"><b>Custo para acontecer:</b>${custoCondHTML(h.pagar, `p${idx}`)}
      <button class="botao perigo peq" data-rm-pagar="${idx}">× custo</button></div>`;
  };
  /** "Enquanto estiver em campo" (05/10): passivo (grupo e custo das cartas) ou recorrente (etapa, turno, por quantos turnos). */
  const emCampoHTML = (h, idx) => {
    if (h.quando !== 'emCampo') return '';
    const modo = h.modo ?? 'passivo';
    const sel = (k, lista, v) => `<select data-hsel="${idx}" data-k="${k}">${opc(lista, v)}</select>`;
    let corpo;
    if (modo === 'passivo') {
      const a = h.aura ?? {}, cc = h.custoCartas ?? {};
      const ck = (dk, k, v, rot, marcado) => `<label class="opcao-linha"><input type="checkbox" data-${dk}="${idx}" data-k="${k}" data-v="${v}" ${marcado ? 'checked' : ''}> ${esc(rot)}</label>`;
      corpo = `<div class="cst-bloco"><b>Bônus a um grupo</b> <span class="peq">(vale só enquanto a carta está em campo; nada fica gravado nas cartas)</span>
          <label>Quem recebe ${`<select data-aura="${idx}" data-k="lado">${opc(LADO_AURA, a.lado ?? 'proprio')}</select>`}</label>
          <span class="cst-multi">Tipos: ${TIPOS_AURA.map(([k, r]) => ck('aura', 'tipos', k, r, (a.tipos?.length ? a.tipos : ['aliado']).includes(k))).join('')}</span>
          <label>Ataque ± <input type="number" step="1" data-aura="${idx}" data-k="ataque" value="${a.ataque ?? ''}"></label>
          <label>Vida ± <input type="number" step="1" data-aura="${idx}" data-k="vida" value="${a.vida ?? ''}"></label>
          <span class="cst-multi">Palavras-chave: ${PALAVRAS_CONCESSAO.map((p) => ck('aura', 'palavras', p, p, (a.palavras ?? []).includes(p))).join('')}</span>
          ${ck('aura', 'excetoEsta', '1', 'Menos esta carta', a.excetoEsta)}</div>
        <div class="cst-bloco"><b>Custo das cartas</b> <span class="peq">(+ = mais caras, − = mais baratas; vazio = não muda)</span>
          <label>De quem <select data-ccart="${idx}" data-k="de">${opc(DE_QUEM_CUSTO, cc.de ?? 'oponente')}</select></label>
          <span class="cst-multi">Tipos (nenhum = todas): ${TIPOS_CUSTO.map(([k, r]) => ck('ccart', 'tipos', k, r, (cc.tipos ?? []).includes(k))).join('')}</span>
          <label>Custam ± <input type="number" step="1" data-ccart="${idx}" data-k="valor" value="${cc.valor ?? ''}"></label></div>`;
    } else {
      corpo = `<label>Em que etapa ${sel('etapa', ETAPAS, h.etapa ?? 'inicio')}</label> <label>De qual turno ${sel('turnoDe', TURNOS, h.turnoDe ?? 'seu')}</label>
        <label>Até quando: por N turnos <input type="number" min="1" step="1" data-hnum="${idx}" data-k="turnos" value="${h.turnos ?? ''}" placeholder="enquanto em campo"></label>`;
    }
    const m = h.manutencao;
    const manut = m
      ? `<div class="cst-custo"><b>Manutenção:</b> <label>Cobrada em ${`<select data-cs="m${idx}" data-k="etapa">${opc(ETAPAS, m.etapa ?? 'inicio')}</select>`}</label>
          <label>${`<select data-cs="m${idx}" data-k="turnoDe">${opc(TURNOS, m.turnoDe ?? 'seu')}</select>`}</label>
          ${custoCondHTML(m, `m${idx}`, { senao: SENAO_MANUT })}
          <button class="botao perigo peq" data-rm-manut="${idx}">× manutenção</button></div>`
      : `<button class="botao peq" data-add-manut="${idx}" title="Custo cobrado numa etapa enquanto a carta está em campo; sem pagar, ela sai (ou o efeito para neste turno)">+ manutenção</button>`;
    return `<div class="cst-emcampo"><label>Modo ${sel('modo', [['passivo', 'Passivo (vale assim que está em campo, sem pilha)'], ['recorrente', 'Recorrente (dispara numa etapa e vai para a pilha)']], modo)}</label>
      ${corpo}${manut}</div>`;
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
        ${emCampoHTML(h, idx)}
        ${pagarHTML(h, idx)}
        ${h.quando === 'emCampo' && (h.modo ?? 'passivo') === 'passivo' ? '' : `
        ${(h.efeitos ?? []).map((ef, i) => efHTML(ef, `${idx}/${i}`)).join('')}
        ${(h.efeitos ?? []).length ? '' : `<p class="peq falta">${conjur ? 'Habilidade' : 'Condição'} sem efeitos: não faz nada. Acrescente um efeito ou use "${conjur ? 'Tirar habilidade' : 'Tirar condição'}".</p>`}
        <button class="botao peq" data-add-ef="${idx}">+ efeito</button>
        <button class="botao peq" data-add-sec="${idx}" title="Custo ou consequência depois dos efeitos (dano no seu Conjurador, descarte, sacrificar um Aliado, Grau ao cemitério)">+ efeito secundário</button>`}
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
  /** Objeto editado por um campo: efeito pelo caminho ("0/1", "e0/1/0"...) ou permissão "perm.i". */
  const alvoDe = (cam) => (cam.startsWith('perm.') ? mec.permiteJogarDe[+cam.slice(5)] : efeitoDe(cam));
  /** Número do campo: vazio = sem o campo; `negativo` aceita valores menores que zero (bônus e custo das cartas). */
  const numDe = (t, negativo = false) => (t.value === '' ? undefined : negativo ? Math.trunc(+t.value || 0) : Math.max(0, Math.trunc(+t.value || 0)));
  const definir = (o, k, v) => { if (v === undefined || v === 0 || v === '' || v === false) delete o[k]; else o[k] = v; };
  const alternar = (o, k, v, ligado) => { const st = new Set(o[k] ?? []); if (ligado) st.add(v); else st.delete(v); if (st.size) o[k] = [...st]; else delete o[k]; };
  /** Campos que só valem numa condição (05/10): sai da condição, sai o campo. */
  function limparCondicao(h) {
    if (h.quando !== 'ativada') { delete h.custo; delete h.repentina; } else h.custo ??= { suspender: true };
    if (h.quando === 'ativada') { delete h.umaVezPorTurno; delete h.vezesPorTurno; delete h.soNoSeuTurno; delete h.pagar; }
    if (!QUANDO_EVENTO.includes(h.quando)) delete h.quem; else h.quem ??= 'voce';
    if (h.quando !== 'emCampo') { for (const k of ['modo', 'aura', 'custoCartas', 'etapa', 'turnoDe', 'turnos', 'manutencao']) delete h[k]; }
    else {
      delete h.umaVezPorTurno; delete h.vezesPorTurno; delete h.soNoSeuTurno;
      h.modo ??= 'passivo';
      if (h.modo === 'passivo') { h.efeitos = []; delete h.pagar; if (!h.aura && !h.custoCartas) h.aura = { lado: 'proprio', ataque: 1 }; }
    }
  }
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
      limparCondicao(h);
      return gravar(true);
    }
    // vezes por turno (05/10): 1 grava como "uma vez por turno" (formato de sempre); mais de 1, vezesPorTurno
    if (t.dataset.vezes !== undefined) {
      const h = hs[+t.dataset.vezes], v = numDe(t);
      delete h.umaVezPorTurno; delete h.vezesPorTurno;
      if (v === 1) h.umaVezPorTurno = true; else if (v && v > 1) h.vezesPorTurno = v;
      return gravar(false);
    }
    if (t.dataset.hsel !== undefined) {
      const h = hs[+t.dataset.hsel]; h[t.dataset.k] = t.value;
      if (t.dataset.k === 'modo') {
        if (t.value === 'passivo') { delete h.etapa; delete h.turnoDe; delete h.turnos; } else { delete h.aura; delete h.custoCartas; h.etapa ??= 'inicio'; h.turnoDe ??= 'seu'; if (!h.efeitos?.length) h.efeitos = [novoEfeito('comprar')]; }
        limparCondicao(h);
      }
      return gravar(true);
    }
    if (t.dataset.hnum !== undefined) { definir(hs[+t.dataset.hnum], t.dataset.k, numDe(t)); return gravar(false); }
    if (t.dataset.aura !== undefined) {
      const h = hs[+t.dataset.aura], a = (h.aura ??= { lado: 'proprio' }), k = t.dataset.k;
      if (k === 'lado') a.lado = t.value;
      else if (k === 'tipos') { a.tipos ??= ['aliado']; alternar(a, 'tipos', t.dataset.v, t.checked); if (!a.tipos) { a.tipos = [t.dataset.v]; t.checked = true; } } // ao menos um tipo: sem tipos o motor volta para Aliado
      else if (k === 'palavras') alternar(a, 'palavras', t.dataset.v, t.checked);
      else if (k === 'excetoEsta') definir(a, k, t.checked);
      else definir(a, k, numDe(t, true));
      return gravar(false);
    }
    if (t.dataset.ccart !== undefined) {
      const h = hs[+t.dataset.ccart], c = (h.custoCartas ??= { de: 'oponente', valor: 0 }), k = t.dataset.k;
      if (k === 'de') c.de = t.value;
      else if (k === 'tipos') alternar(c, 'tipos', t.dataset.v, t.checked);
      else c.valor = numDe(t, true) ?? 0;
      if (!c.valor && !c.tipos && c.de === 'oponente') delete h.custoCartas; // só some se nada foi escolhido
      return gravar(false);
    }
    // custo condicional (gatilho "p", manutenção "m", efeito "e"): o que se paga, quem paga, quando e o que acontece sem ele
    if (t.dataset.cc !== undefined) { const c = custoDe(t.dataset.cc); if (c) definir(c, t.dataset.k, numDe(t)); return gravar(false); }
    if (t.dataset.cs !== undefined) {
      const c = custoDe(t.dataset.cs), k = t.dataset.k;
      if (!c) return;
      c[k] = t.value;
      if (k === 'senao') { if (t.value !== 'alternativo') delete c.alternativos; else if (!c.alternativos?.length) c.alternativos = [novoEfeito('secDano')]; if (t.value !== 'sai') delete c.destino; }
      if (k === 'quando' && t.value === 'resolver') delete c.quando;
      return gravar(true);
    }
    if (t.dataset.flag !== undefined) { const h = hs[+t.dataset.flag]; if (t.checked) h[t.dataset.k] = true; else delete h[t.dataset.k]; return gravar(false); }
    if (t.dataset.tipoEf) {
      const cam = t.dataset.tipoEf, k = cam.lastIndexOf('/'), lista = listaDe(cam.slice(0, k)), i = +cam.slice(k + 1);
      const { requisito: req, custo } = lista[i];
      lista[i] = { ...novoEfeito(t.value), ...(req ? { requisito: req } : {}), ...(custo ? { custo } : {}) };
      return gravar(true);
    }
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
    if (b.dataset.addEf !== undefined) { listaDe(b.dataset.addEf, true).push(novoEfeito(/^\d+$/.test(b.dataset.addEf) ? 'comprar' : 'secDano')); return gravar(true); }
    if (b.dataset.addSec !== undefined) { hs[+b.dataset.addSec].efeitos.push(novoEfeito('secDano')); return gravar(true); }
    if (b.dataset.rmEf !== undefined) { const cam = b.dataset.rmEf, k = cam.lastIndexOf('/'); listaDe(cam.slice(0, k)).splice(+cam.slice(k + 1), 1); return gravar(true); }
    // custos (05/10): da linha ("pague para funcionar"), do gatilho e a manutenção
    if (b.dataset.addCusto !== undefined) { efeitoDe(b.dataset.addCusto).custo = { arcano: 1 }; return gravar(true); }
    if (b.dataset.rmCusto !== undefined) { delete efeitoDe(b.dataset.rmCusto).custo; return gravar(true); }
    if (b.dataset.addPagar !== undefined) { hs[+b.dataset.addPagar].pagar = { arcano: 1 }; return gravar(true); }
    if (b.dataset.rmPagar !== undefined) { delete hs[+b.dataset.rmPagar].pagar; return gravar(true); }
    if (b.dataset.addManut !== undefined) { hs[+b.dataset.addManut].manutencao = { arcano: 1 }; return gravar(true); }
    if (b.dataset.rmManut !== undefined) { delete hs[+b.dataset.rmManut].manutencao; return gravar(true); }
    if (b.dataset.addPerm !== undefined) { (mec.permiteJogarDe ??= []).push(novaPermissao()); return gravar(true); }
    if (b.dataset.rmPerm !== undefined) { mec.permiteJogarDe.splice(+b.dataset.rmPerm, 1); if (!mec.permiteJogarDe.length) delete mec.permiteJogarDe; return gravar(true); }
  });
  desenhar();
  return { definir(m) { carregar(m); desenhar(); } };
}
