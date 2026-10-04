// App de testes do Arcanoclisma: jogar contra o jogador automático com os 9 Conjuradores, registrar tudo,
// comentar durante as partidas e enviar os dados ao Claude (botão verde "Enviar").
// Sem dependências: usa o motor real (dist/motor.js) e o jogador automático das simulações (dist/bot.js).
import { criarJogo, executar, ErroRegra, defCarta, defConjurador, custoEfetivo, custoProtecao, custoReanimar, origemDaCarta, grauEfetivo, recemChegado, ataqueConjurador } from './dist/motor.js';
import { decidir } from './dist/bot.js';
import { TIPO, frase, textoCarta, valoresTxt } from './textos.js';

const DADOS = await (await fetch('./dist/dados.json', { cache: 'no-store' })).json();
const VERSAO_APP = 1;
// ?teste: guarda em outra chave (para o Claude testar sem mexer nas suas partidas)
const CHAVE = new URLSearchParams(location.search).has('teste') ? 'arcanoclisma-testes-claude' : 'arcanoclisma-testes-v1';
const EU = 0, BOT = 1;
const ETAPA = { preparacao: 'Preparação', inicio: 'Início', principal: 'Principal', cataclisma: 'Cataclisma', final: 'Final' };
const $ = (s) => document.querySelector(s);
const esc = (t) => String(t).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

// ---------- armazenamento ----------
function armVazio() {
  return { versao: VERSAO_APP, partidas: [], comentariosGerais: [], envios: [], config: { autoAtaques: true, segurarPrioridade: false, pararFinalOponente: true } };
}
let arm = (() => { try { return { ...armVazio(), ...JSON.parse(localStorage.getItem(CHAVE) ?? 'null') }; } catch { return armVazio(); } })();
let avisoSalvar = '';
function salvar() {
  try { localStorage.setItem(CHAVE, JSON.stringify(arm)); avisoSalvar = ''; } catch (erro) { avisoSalvar = 'Não consegui salvar no navegador (' + erro.message + '). Use o botão Enviar para não perder os dados.'; }
}

// ---------- estado da tela ----------
let tela = 'inicio'; // 'inicio' | 'partida' | 'conjuradores'
let partida = null;   // registro salvo da partida atual
let e = null;         // estado do motor
let aba = 'mesa';
let mensagem = null;  // { tipo: 'aviso' | 'ok', texto }
let servidorOnline = null;

// ---------- partida ----------
// DP: dados (cartas, regras, baralhos) da partida aberta. Partidas antigas continuam com a versão com que foram jogadas
// (app/versoes/<versao>.json), mesmo depois de edições no editor.
let DP = DADOS;
const conjDef = (id) => DP.catalogo.conjuradores[id] ?? DADOS.catalogo.conjuradores[id];
const baralhoDe = (conjId, itens) => {
  const b = DP.baralhos.find((x) => x.conjurador === conjId);
  return { conjurador: conjId, cartas: b.lista.flatMap(([id, n]) => Array.from({ length: n }, () => id)), itens: itens ?? b.itens ?? [] };
};
/** Side deck: itens que o Conjurador pode levar (um Canal, uma Armadura e um Extra por partida, sem trocar durante o jogo). */
const ESPACO_ROT = { canal: 'Canal (arma)', armadura: 'Armadura', extra: 'Extra (consumível)' };
function itensDoSide(conjId) {
  const c = conjDef(conjId);
  return Object.values(DADOS.catalogo.cartas).filter((x) => x.tipo === 'item' && (!x.valores?.length || x.valores.some((v) => c.valores.includes(v))));
}
const estiloDe = (conjId) => DP.baralhos.find((x) => x.conjurador === conjId).estilo;
function novoMotor(p) {
  return criarJogo({ catalogo: DP.catalogo, baralhos: [baralhoDe(p.eu, p.itensEu), baralhoDe(p.oponente)], semente: p.semente, regras: DP.regras });
}
/** Partidas de antes do editor não guardavam a versão: foram jogadas com esta (o jogo sem edições). */
const VERSAO_ANTES_DO_EDITOR = '5539baf083';
async function dadosDaPartida(p) {
  const versao = p.dadosVersao ?? VERSAO_ANTES_DO_EDITOR;
  if (versao === DADOS.versao) return DADOS;
  const r = await fetch(`./versoes/${versao}.json`, { cache: 'no-store' });
  if (!r.ok) throw new Error(`não achei as cartas da versão ${versao} (app/versoes/)`);
  return r.json();
}
function reconstruir(p) {
  const m = novoMotor(p);
  for (const c of p.comandos) executar(m, c.cmd);
  return m;
}
function novaPartida(eu, oponente, itensEu) {
  const ids = DADOS.baralhos.map((b) => b.conjurador);
  if (oponente === 'aleatorio') oponente = ids[Math.floor(Math.random() * ids.length)];
  partida = {
    id: 'p' + Date.now().toString(36), inicio: new Date().toISOString(), fim: null, eu, oponente,
    semente: Math.floor(Math.random() * 2 ** 31), variante: DADOS.variante, dadosGeradosEm: DADOS.geradoEm, dadosVersao: DADOS.versao,
    comandos: [], erros: [], comentarios: [], resultado: null, enviada: false, itensEu,
  };
  arm.partidas.push(partida);
  DP = DADOS;
  e = novoMotor(partida);
  tela = 'partida'; aba = 'mesa'; mensagem = null;
  avancar();
}
async function abrirPartida(id) {
  partida = arm.partidas.find((p) => p.id === id);
  if (!partida) return;
  try { DP = await dadosDaPartida(partida); e = reconstruir(partida); } catch (erro) { mensagem = { tipo: 'aviso', texto: 'Não consegui reconstruir esta partida: ' + erro.message }; render(); return; }
  tela = 'partida'; aba = partida.resultado ? 'registro' : 'mesa'; mensagem = null;
  avancar();
}

/** Executa um comando e registra (quem: 'eu' | 'bot' | 'auto'). Devolve true se deu certo. */
function tentar(cmd, quem = 'eu') {
  const antes = { turno: e.turno, etapa: e.etapa, log: e.log.length };
  try {
    executar(e, cmd);
  } catch (erro) {
    if (!(erro instanceof ErroRegra)) throw erro;
    partida.erros.push({ quem, cmd, msg: erro.message, turno: antes.turno, etapa: antes.etapa, ts: new Date().toISOString() });
    if (quem === 'eu') mensagem = { tipo: 'aviso', texto: erro.message };
    salvar();
    return false;
  }
  partida.comandos.push({ n: partida.comandos.length + 1, quem, cmd, turno: antes.turno, etapa: antes.etapa, ts: new Date().toISOString(), resultado: e.log.slice(antes.log) });
  if (quem === 'eu') mensagem = null;
  return true;
}

/** Roda o jogador automático e os passes automáticos até ser a vez de você decidir. */
function avancar() {
  let passos = 0;
  while (e.vencedor === null && passos++ < 4000) {
    const ator = e.pendencia ? e.pendencia.jogador : e.prioridade;
    if (ator === EU) {
      if (deveAutoPassar()) { tentar({ tipo: 'passar', jogador: EU }, 'auto'); continue; }
      break;
    }
    const cmd = decidir(e, BOT, estiloDe(partida.oponente));
    if (!tentar(cmd, 'bot')) tentar(e.pendencia ? { tipo: 'passar', jogador: BOT } : { tipo: 'passar', jogador: BOT }, 'bot');
  }
  if (e.vencedor !== null && !partida.resultado) {
    partida.resultado = { vencedor: e.vencedor === EU ? 'você' : e.vencedor === BOT ? 'bot' : 'empate', turnos: e.turno, ultimoEvento: e.log[e.log.length - 1], motivo: 'fim de jogo' };
    partida.fim = new Date().toISOString();
    partida.logCompleto = e.log;
  }
  salvar();
  render();
}

function potencialArcano(j) {
  const z = e.jogadores[j];
  return z.arcano + z.grau.filter((g) => !e.instancias[g].suspensa).length + z.mao.reduce((t, id) => t + defCarta(e, id).arcanoGerado, 0);
}
/** Você tem alguma resposta possível agora (carta Repentina, habilidade Repentina ou ativação)? */
function possoResponder() {
  const z = e.jogadores[EU];
  const pot = potencialArcano(EU);
  for (const id of [...z.mao, ...z.banimento, ...z.cemiterio]) {
    if (!origemDaCarta(e, EU, id)) continue;
    const d = defCarta(e, id);
    if (d.velocidade === 'repentina' && d.grau <= grauEfetivo(e, EU) && custoEfetivo(e, EU, id) <= pot) return true;
  }
  const at = conjDef(partida.eu).poder?.ativacao;
  if (at?.repentina && !e.instancias[z.conjuradorId].suspensa) return true;
  for (const id of z.campo) {
    const a = defCarta(e, id).ativacao;
    if (a && !a.efeitos.every((ef) => ef.tipo === 'arcano') && !(a.suspende && (e.instancias[id].suspensa || recemChegado(e, id)))) return true;
  }
  return false;
}
function deveAutoPassar() {
  if (e.pendencia) return false;
  const cfg = arm.config;
  const topo = e.pilha[e.pilha.length - 1];
  if (topo) {
    if (topo.controlador === EU) return !cfg.segurarPrioridade;
    if (topo.tipo === 'ataque' && cfg.autoAtaques) return true;
    return !possoResponder();
  }
  if (e.combate) return !possoResponder();
  if (e.ativo === BOT) return !(e.etapa === 'final' && cfg.pararFinalOponente && possoResponder());
  return e.etapa !== 'principal';
}

function desfazer() {
  let i = partida.comandos.length - 1;
  while (i >= 0 && partida.comandos[i].quem !== 'eu') i--;
  if (i < 0) { mensagem = { tipo: 'aviso', texto: 'Nada seu para desfazer.' }; render(); return; }
  partida.desfeitos = (partida.desfeitos ?? []).concat([{ ts: new Date().toISOString(), comandos: partida.comandos.slice(i) }]);
  partida.comandos = partida.comandos.slice(0, i);
  partida.resultado = null; partida.fim = null;
  e = reconstruir(partida);
  mensagem = { tipo: 'ok', texto: 'Última ação sua desfeita (o que veio depois dela também).' };
  salvar();
  render();
}
function encerrar() {
  if (!confirm('Encerrar esta partida agora? Ela fica registrada como abandonada.')) return;
  partida.resultado = { vencedor: 'ninguém', turnos: e.turno, motivo: 'abandonada', ultimoEvento: e.log[e.log.length - 1] };
  partida.fim = new Date().toISOString();
  partida.logCompleto = e.log;
  salvar();
  render();
}

// ---------- nomes ----------
function nomeInst(id) {
  const i = e.instancias[id];
  if (!i) return id;
  return i.ehConjurador ? conjDef(i.defId).nome : defCarta(e, id).nome;
}

// ---------- ações ----------
function minhaJanela() { return e.ativo === EU && e.etapa === 'principal' && e.pilha.length === 0 && e.combate === null && !e.pendencia; }
function tenhoPrioridade() { return !e.pendencia && e.prioridade === EU && e.vencedor === null; }

function tiposDeAlvo(efs) {
  const s = new Set();
  for (const ef of efs ?? []) {
    if (ef.tipo === 'banirDaMao') s.add('mao');
    else if (ef.tipo === 'reanimar') s.add('cemiterio');
    else if ('alvo' in ef && ['escolhido', 'escolhidoAliado', 'escolhidoProprio', 'pilha'].includes(ef.alvo)) s.add(ef.alvo);
  }
  return s;
}
function candidatosAlvo(tipos, excluir) {
  const lista = [];
  const add = (id, rotulo) => { if (id !== excluir && !lista.some((x) => x.id === id)) lista.push({ id, rotulo }); };
  const dono = (id) => (e.instancias[id].dono === EU ? 'seu' : 'do oponente');
  const campoTodos = [EU, BOT].flatMap((j) => e.jogadores[j].campo);
  if (tipos.has('pilha')) for (const p of [...e.pilha].reverse()) if (p.tipo === 'carta') add(p.instId, `Na pilha: ${nomeInst(p.instId)} (${dono(p.instId)})`);
  if (tipos.has('mao')) for (const id of e.jogadores[EU].mao) add(id, `Da sua mão: ${nomeInst(id)}`);
  if (tipos.has('cemiterio')) for (const j of [EU, BOT]) for (const id of e.jogadores[j].cemiterio) if (defCarta(e, id).tipo === 'aliado') add(id, `Cemitério ${j === EU ? 'seu' : 'do oponente'}: ${nomeInst(id)} (custo ${defCarta(e, id).custo})`);
  if (tipos.has('escolhido')) { for (const j of [BOT, EU]) add(e.jogadores[j].conjuradorId, `Conjurador ${j === EU ? 'seu' : 'do oponente'}: ${nomeInst(e.jogadores[j].conjuradorId)}`); for (const id of campoTodos) add(id, `${nomeInst(id)} (${dono(id)})`); }
  if (tipos.has('escolhidoAliado')) for (const id of campoTodos) if (['aliado', 'mistica'].includes(defCarta(e, id).tipo)) add(id, `${nomeInst(id)} (${dono(id)})`);
  if (tipos.has('escolhidoProprio')) for (const id of e.jogadores[EU].campo) if (['aliado', 'mistica'].includes(defCarta(e, id).tipo)) add(id, nomeInst(id));
  return lista;
}

/** Custo real do comando que está sendo preparado (com Protegido do alvo e o Aliado reanimado). */
function custoReal(tipo, id, alvo) {
  const z = e.jogadores[EU];
  if (tipo === 'jogar') return custoEfetivo(e, EU, id) + custoProtecao(e, EU, alvo);
  if (tipo === 'ativar') {
    const at = id === z.conjuradorId ? conjDef(partida.eu).poder.ativacao : defCarta(e, id).ativacao;
    const rean = at.efeitos.find((ef) => ef.tipo === 'reanimar');
    return at.custo + custoProtecao(e, EU, alvo) + (rean && alvo ? custoReanimar(rean, defCarta(e, alvo)) : 0);
  }
  return conjDef(partida.eu).custoAtaque;
}
/** Das cartas marcadas, só as necessárias (na ordem marcada); a última pode gerar mais do que falta. */
function pitchNecessario(marcadas, custo) {
  let reserva = e.jogadores[EU].arcano;
  const usar = [];
  for (const p of marcadas) { if (reserva >= custo) break; usar.push(p); reserva += defCarta(e, p).arcanoGerado; }
  return { usar, reserva };
}

/** Abre a janela para preparar uma jogada, ativação ou ataque (alvo, descarte do Grau, envio de cartas ao Arcano). */
function preparar(tipo, id) {
  const z = e.jogadores[EU];
  let titulo, efs = [], custo = 0, descarta = 0, alvos = [], obs = '';
  if (tipo === 'jogar') {
    const d = defCarta(e, id);
    titulo = `Jogar ${d.nome}`;
    efs = [...(d.efeitos ?? []), ...(d.aoEntrar ?? [])];
    custo = custoEfetivo(e, EU, id);
    descarta = d.descartaGrau ?? 0;
    const origem = origemDaCarta(e, EU, id);
    if (origem === 'banimento') obs = 'Jogando do banimento, com desconto';
    if (origem === 'cemiterio') obs = 'Jogando do cemitério: ao resolver, ela é banida';
  } else if (tipo === 'ativar') {
    const ehConj = id === z.conjuradorId;
    const at = ehConj ? conjDef(partida.eu).poder.ativacao : defCarta(e, id).ativacao;
    titulo = `Ativar ${nomeInst(id)}`;
    efs = at.efeitos; custo = at.custo;
    if (efs.some((ef) => ef.tipo === 'reanimar')) obs = 'Custo final: o que o reanimar pede pelo Aliado escolhido (nada, o custo dele ou o custo + X)';
  } else {
    titulo = `Atacar com ${nomeInst(id)}`;
    const adv = e.jogadores[BOT];
    alvos = [{ id: adv.conjuradorId, rotulo: `Conjurador do oponente: ${nomeInst(adv.conjuradorId)}` },
      ...adv.campo.filter((a) => ['aliado', 'mistica'].includes(defCarta(e, a).tipo) && defCarta(e, a).vida !== undefined).map((a) => ({ id: a, rotulo: nomeInst(a) }))];
  }
  if (tipo !== 'atacar') alvos = tiposDeAlvo(efs).size ? candidatosAlvo(tiposDeAlvo(efs), id) : [];
  const precisaAlvo = tipo === 'atacar' || tiposDeAlvo(efs).size > 0;
  const grauLivre = z.grau.filter((g) => !e.instancias[g].suspensa);
  const pitch = z.mao.filter((x) => x !== id);
  const html = `<div class="modal-fundo"><div class="modal">
    <h3>${esc(titulo)}</h3>
    ${tipo !== 'atacar' ? `<p>Custo: <b>${custo}</b> de Arcano${obs ? ` · ${esc(obs)}` : ''}. Você tem <b>${z.arcano}</b> na reserva.</p>` : ''}
    ${precisaAlvo ? `<p><b>Alvo</b></p>${alvos.length ? alvos.map((a, i) => `<label class="opcao"><input type="radio" name="alvo" value="${a.id}" ${i === 0 ? 'checked' : ''}> ${esc(a.rotulo)}</label>`).join('') : '<p class="peq">Nenhum alvo disponível.</p>'}` : ''}
    ${descarta ? `<p><b>Descarte ${descarta} carta(s) do seu Grau</b></p>${z.grau.map((g) => `<label class="opcao"><input type="checkbox" name="descarte" value="${g}"> ${esc(nomeInst(g))}${e.instancias[g].suspensa ? ' (suspensa)' : ''}</label>`).join('')}` : ''}
    ${tipo !== 'atacar' ? `<p><b>Gerar Arcano</b> <span class="peq">(o que faltar)</span></p>
      <div class="linha">${grauLivre.length ? `<button data-m="suspGrau">Suspender 1 carta do Grau (+1)</button>` : '<span class="peq">Sem Grau levantado.</span>'}
      ${z.campo.filter((c) => { const a = defCarta(e, c).ativacao; return a && a.efeitos.every((ef) => ef.tipo === 'arcano') && !e.instancias[c].suspensa && !recemChegado(e, c); }).map((c) => `<button data-m="mana" data-id="${c}">${esc(nomeInst(c))}</button>`).join('')}</div>
      <p class="peq">Enviar cartas da mão ao Arcano (pitch): só o necessário.</p>
      ${pitch.map((p) => `<label class="opcao"><input type="checkbox" name="pitch" value="${p}"> ${esc(nomeInst(p))} <span class="peq">(gera ${defCarta(e, p).arcanoGerado})</span></label>`).join('') || '<p class="peq">Mão vazia.</p>'}
      <p id="falta-arcano" class="peq"></p>` : ''}
    <div class="linha" style="margin-top:12px"><button class="primario" data-m="ok">Confirmar</button><button data-m="cancelar">Cancelar</button></div>
    <div id="modal-erro"></div>
  </div></div>`;
  $('#modal').innerHTML = html;
  const marcadas = [];
  const atualizarFalta = () => {
    const el = $('#falta-arcano');
    if (!el) return;
    const alvo = document.querySelector('input[name="alvo"]:checked')?.value;
    const c = custoReal(tipo, id, alvo);
    const { usar, reserva } = pitchNecessario(marcadas, c);
    const sobra = marcadas.filter((p) => !usar.includes(p));
    el.innerHTML = reserva >= c
      ? `<b style="color:var(--verde)">Custo ${c} coberto</b> (reserva + envio = ${reserva}${reserva > c ? `; sobram ${reserva - c} na reserva` : ''}).${sobra.length ? ` Não é preciso enviar: ${sobra.map((p) => esc(nomeInst(p))).join(', ')} (fica na mão).` : ''}`
      : `<b style="color:var(--vermelho)">Faltam ${c - reserva}</b> de Arcano (custo ${c}).`;
  };
  $('#modal').onchange = (ev) => {
    const x = ev.target;
    if (x.name === 'pitch') { if (x.checked) marcadas.push(x.value); else marcadas.splice(marcadas.indexOf(x.value), 1); }
    atualizarFalta();
  };
  atualizarFalta();
  $('#modal').onclick = (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    const m = b.dataset.m;
    if (m === 'cancelar') { fecharModal(); return; }
    if (m === 'suspGrau') { const g = e.jogadores[EU].grau.find((x) => !e.instancias[x].suspensa); if (g && tentar({ tipo: 'suspenderGrau', jogador: EU, carta: g })) { salvar(); preparar(tipo, id); render(); } return; }
    if (m === 'mana') { if (tentar({ tipo: 'ativar', jogador: EU, carta: b.dataset.id })) { salvar(); preparar(tipo, id); render(); } return; }
    if (m === 'ok') {
      const alvo = document.querySelector('input[name="alvo"]:checked')?.value;
      const enviarArcano = tipo === 'atacar' ? [] : pitchNecessario(marcadas, custoReal(tipo, id, alvo)).usar;
      const descartarGrau = [...document.querySelectorAll('input[name="descarte"]:checked')].map((x) => x.value);
      let cmd;
      if (tipo === 'jogar') cmd = { tipo: 'jogar', jogador: EU, carta: id, alvo, descartarGrau, enviarArcano };
      else if (tipo === 'ativar') cmd = { tipo: 'ativar', jogador: EU, carta: id, alvo, enviarArcano };
      else cmd = { tipo: 'atacar', jogador: EU, atacante: id, alvo, enviarArcano: [] };
      if (tentar(cmd)) { fecharModal(); avancar(); }
      else { $('#modal-erro').innerHTML = `<div class="aviso">${esc(mensagem?.texto ?? 'Jogada inválida.')}</div>`; }
    }
  };
}
function fecharModal() { $('#modal').innerHTML = ''; $('#modal').onclick = null; $('#modal').onchange = null; }

function acao(nome, id) {
  if (nome === 'jogar' || nome === 'ativar' || nome === 'atacar') { preparar(nome, id); return; }
  let cmd = null;
  if (nome === 'grau') cmd = { tipo: 'colocarGrau', jogador: EU, carta: id };
  if (nome === 'susp') cmd = { tipo: 'suspenderGrau', jogador: EU, carta: id };
  if (nome === 'mana') cmd = { tipo: 'ativar', jogador: EU, carta: id };
  if (nome === 'passar') cmd = { tipo: 'passar', jogador: EU };
  if (nome === 'bloquear') cmd = { tipo: 'bloquear', jogador: EU, bloqueador: id === 'nenhum' ? null : id };
  if (nome === 'comecar') cmd = { tipo: 'escolherInicio', jogador: EU, primeiro: id === 'eu' ? EU : BOT };
  if (nome === 'naoPagar') cmd = { tipo: 'pagarAnulacao', jogador: EU, pagar: false };
  if (nome === 'pagar') cmd = { tipo: 'pagarAnulacao', jogador: EU, pagar: true, enviarArcano: [...document.querySelectorAll('input[name="pitchAnul"]:checked')].map((x) => x.value) };
  if (nome === 'escolherTopo') cmd = { tipo: 'escolherDoTopo', jogador: EU, cartas: [...document.querySelectorAll('input[name="topo"]:checked')].map((x) => x.value) };
  if (nome === 'descartar') cmd = { tipo: 'descartar', jogador: EU, cartas: [...document.querySelectorAll('input[name="desc"]:checked')].map((x) => x.value) };
  if (cmd && tentar(cmd)) avancar(); else render();
}

// ---------- renderização ----------
function cartaHTML(id, acoes = []) {
  const inst = e.instancias[id];
  const d = defCarta(e, id);
  const corpo = d.vida !== undefined && (d.tipo === 'aliado' || d.vida > 0) ? ` · ${d.ataque ?? 0}/${d.vida}${inst.dano ? ` <b style="color:var(--vermelho)">(dano ${inst.dano})</b>` : ''}` : '';
  const flags = [inst.suspensa ? 'suspensa' : '', recemChegado(e, id) ? 'acabou de entrar' : ''].filter(Boolean).join(', ');
  return `<div class="carta ${inst.suspensa ? 'suspensa' : ''}" title="${esc(valoresTxt(d.valores))}">
    <div class="nome">${esc(d.nome)}</div>
    <div class="meta">${TIPO[d.tipo]} · custo ${d.custo} · Grau ${d.grau}${d.velocidade === 'repentina' ? ' · Repentina' : ''}${corpo}</div>
    <div class="texto">${esc(textoCarta(d))}</div>
    ${flags ? `<div class="peq">${flags}</div>` : ''}
    ${acoes.length ? `<div class="acoes">${acoes.map(([n, r]) => `<button data-acao="${n}" data-id="${id}">${r}</button>`).join('')}</div>` : ''}
  </div>`;
}
function ladoHTML(j) {
  const z = e.jogadores[j];
  const meu = j === EU;
  const c = conjDef(e.instancias[z.conjuradorId].defId);
  const conjInst = e.instancias[z.conjuradorId];
  const pode = meu && tenhoPrioridade();
  const janela = meu && minhaJanela();
  const acoesConj = [];
  if (janela && !conjInst.suspensa && (ataqueConjurador(e, j) > 0 || !e.regras.conjuradorSoAtacaComArma)) acoesConj.push(['atacar', `Atacar (${ataqueConjurador(e, j)})`]);
  if (meu && c.poder?.ativacao && !conjInst.suspensa && pode && (c.poder.ativacao.repentina || janela)) acoesConj.push(['ativar', 'Usar habilidade']);
  const grau = z.grau.map((g) => (meu || e.instancias[g].revelada) ? cartaHTML(g, meu && !e.instancias[g].suspensa && !e.pendencia ? [['susp', 'Suspender (+1)']] : []) : `<div class="carta ${e.instancias[g].suspensa ? 'suspensa' : ''}"><div class="nome">Carta virada</div><div class="peq">${e.instancias[g].suspensa ? 'suspensa' : 'levantada'}</div></div>`).join('');
  const campo = z.campo.map((id) => {
    const d = defCarta(e, id);
    const ac = [];
    if (janela && d.tipo === 'aliado' && !e.instancias[id].suspensa && !recemChegado(e, id) && (d.ataque ?? 0) > 0) ac.push(['atacar', 'Atacar']);
    if (meu && d.ativacao && pode && !(d.ativacao.suspende && (e.instancias[id].suspensa || recemChegado(e, id)))) ac.push([d.ativacao.efeitos.every((ef) => ef.tipo === 'arcano') ? 'mana' : 'ativar', 'Ativar']);
    return cartaHTML(id, ac);
  }).join('');
  const zonaLista = (nome, ids, comAcao) => `<details class="zona"><summary class="rotulo">${nome} (${ids.length})</summary><div class="cartas">${ids.map((id) => cartaHTML(id, comAcao && pode && origemDaCarta(e, EU, id) ? [['jogar', 'Jogar']] : [])).join('') || '<span class="peq">vazio</span>'}</div></details>`;
  const mao = meu
    ? `<div class="zona"><div class="rotulo">Sua mão (${z.mao.length})</div><div class="cartas">${z.mao.map((id) => {
        const ac = [];
        if (pode && (defCarta(e, id).velocidade === 'repentina' || janela)) ac.push(['jogar', 'Jogar']);
        if (janela && !e.grauNoTurno && !(e.regras.primeiroSemGrauNoTurno1 && e.primeiro === EU && e.turno === 1)) ac.push(['grau', 'Pôr no Grau']);
        return cartaHTML(id, ac);
      }).join('') || '<span class="peq">vazia</span>'}</div></div>`
    : `<div class="zona"><span class="rotulo">Mão do oponente:</span> ${z.mao.length} carta(s)</div>`;
  return `<div class="lado ${meu ? 'meu' : 'deles'}">
    <div class="linha"><span class="conj">${esc(c.nome)}</span> <span class="peq">${esc(valoresTxt(c.valores))}</span>
      <span class="vida">♥ ${c.vida - conjInst.dano}/${c.vida}</span>
      <span>Arcano na reserva: <b>${z.arcano}</b></span><span>Grau: <b>${grauEfetivo(e, j)}</b></span><span>Baralho: ${z.baralho.length}</span>
      ${conjInst.suspensa ? '<span class="peq">(Conjurador suspenso)</span>' : ''}
      ${acoesConj.map(([n, r]) => `<button data-acao="${n}" data-id="${z.conjuradorId}">${r}</button>`).join('')}</div>
    <div class="peq">Habilidade: ${esc(c.habilidade)}</div>
    <div class="peq">Ataque do Conjurador: <b>${ataqueConjurador(e, j)}</b>${e.regras.conjuradorSoAtacaComArma && ataqueConjurador(e, j) <= 0 ? ' (sem Canal: não ataca)' : ''} · Itens: ${(z.itens ?? []).map((id) => itemTxt(id) + (meu && tenhoPrioridade() && defCarta(e, id).categoriaItem === 'extra' ? ` <button data-acao="ativar" data-id="${id}">Usar</button>` : '')).join(' · ') || 'nenhum'}</div>
    <div class="zona"><div class="rotulo">Campo (${z.campo.length})</div><div class="cartas">${campo || '<span class="peq">vazio</span>'}</div></div>
    <details class="zona" ${meu ? 'open' : ''}><summary class="rotulo">Grau (${z.grau.length})</summary><div class="cartas">${grau || '<span class="peq">vazio</span>'}</div></details>
    ${zonaLista('Cemitério', z.cemiterio, meu)}
    ${zonaLista('Banimento', z.banimento, meu)}
    ${mao}
  </div>`;
}
const ESPACO = { canal: 'Canal', armadura: 'Armadura', extra: 'Extra' };
function montarItensEu() {
  const alvo = $('#itens-eu');
  if (!alvo) return;
  const conj = $('#sel-eu').value;
  const padrao = DADOS.baralhos.find((b) => b.conjurador === conj)?.itens ?? [];
  const lista = itensDoSide(conj);
  alvo.innerHTML = '<span class="peq">Seu side deck (escolha um de cada, não troca durante a partida):</span> ' + ['canal', 'armadura', 'extra'].map((esp) => `<label class="campo" style="flex-direction:row;gap:4px;align-items:center">${ESPACO_ROT[esp]} <select data-slot="${esp}"><option value="">nenhum</option>${lista.filter((x) => x.categoriaItem === esp).map((x) => `<option value="${x.id}" ${padrao.includes(x.id) ? 'selected' : ''}>${esc(x.nome)} (${esc(textoCarta(x).replace(/^[^.]+.s*/, ''))})</option>`).join('')}</select></label>`).join('');
}
function itemTxt(id) {
  const d = defCarta(e, id);
  const st = d.categoriaItem === 'armadura' ? `Defesa ${d.defesa ?? 0}, desgaste ${e.instancias[id].dano}/${d.vidaDesgaste ?? 0}` : d.categoriaItem === 'extra' ? textoCarta(d).replace(/^Extra.s*/, '') : `Ataque ${d.ataque ?? 0}`;
  return `<b>${esc(d.nome)}</b> (${ESPACO[d.categoriaItem] ?? d.categoriaItem}, ${st})`;
}
function pendenciaHTML() {
  const p = e.pendencia;
  if (!p || p.jogador !== EU) return '';
  if (p.tipo === 'escolherInicio') return `<div class="pendencia"><b>Você ganhou no dado (${e.dados[EU]} × ${e.dados[BOT]}).</b> Quem começa?
    <div class="linha"><button class="primario" data-acao="comecar" data-id="eu">Eu começo</button><button data-acao="comecar" data-id="bot">O oponente começa</button></div></div>`;
  if (p.tipo === 'bloqueio') {
    const c = e.combate;
    const z = e.jogadores[EU];
    const opcoes = [...z.campo.filter((id) => defCarta(e, id).tipo === 'aliado' && !e.instancias[id].suspensa), z.conjuradorId].filter((id) => !e.instancias[id].suspensa);
    return `<div class="pendencia"><b>Bloqueio:</b> ${esc(nomeInst(c.atacante))} ataca ${esc(nomeInst(c.alvo))}. Quem bloqueia? <span class="peq">(quem bloqueia fica suspenso${e.regras.conjuradorLevantaNoInicio ? '; um Aliado só levanta no seu Final, o Conjurador levanta no Início do seu turno' : ' e não ataca no seu próximo turno'}${(() => { const a = (e.jogadores[EU].itens ?? []).find((i) => defCarta(e, i).categoriaItem === 'armadura'); return a ? `; se o Conjurador bloquear, ${esc(defCarta(e, a).nome)} evita até ${defCarta(e, a).defesa} de dano` : ''; })()})</span>
      <div class="linha">${opcoes.map((id) => `<button data-acao="bloquear" data-id="${id}">${esc(nomeInst(id))}</button>`).join('')}<button class="primario" data-acao="bloquear" data-id="nenhum">Não bloquear</button></div></div>`;
  }
  if (p.tipo === 'descartar') return `<div class="pendencia"><b>Limite de mão:</b> descarte ${p.quantidade} carta(s).
    ${e.jogadores[EU].mao.map((id) => `<label class="opcao"><input type="checkbox" name="desc" value="${id}"> ${esc(nomeInst(id))}</label>`).join('')}
    <button class="primario" data-acao="descartar">Descartar</button></div>`;
  if (p.tipo === 'escolherDoTopo') return `<div class="pendencia"><b>Olhar o topo:</b> escolha ${p.pegar} carta(s) para pôr na mão; as outras vão para o fundo do baralho.
    ${p.cartas.map((id, i) => { const d = defCarta(e, id); return `<label class="opcao"><input type="${p.pegar === 1 ? 'radio' : 'checkbox'}" name="topo" value="${id}" ${i === 0 && p.pegar === 1 ? 'checked' : ''}> <b>${esc(d.nome)}</b> <span class="peq">${TIPO[d.tipo]} · custo ${d.custo} · Grau ${d.grau}${d.vida !== undefined ? ` · ${d.ataque ?? 0}/${d.vida}` : ''} · ${esc(textoCarta(d))}</span></label>`; }).join('')}
    <button class="primario" data-acao="escolherTopo">Pôr na mão</button></div>`;
  if (p.tipo === 'pagarAnulacao') return `<div class="pendencia"><b>${esc(nomeInst(p.item.instId))}</b>: você pode pagar ${p.custo} de Arcano para anulá-la.
    ${e.jogadores[EU].mao.map((id) => `<label class="opcao"><input type="checkbox" name="pitchAnul" value="${id}"> enviar ${esc(nomeInst(id))} ao Arcano</label>`).join('')}
    <div class="linha"><button class="primario" data-acao="pagar">Pagar e anular</button><button data-acao="naoPagar">Não pagar</button></div></div>`;
  return '';
}
function pilhaHTML() {
  if (!e.pilha.length && !e.combate) return '';
  const itens = [...e.pilha].reverse().map((p, i) => {
    const quem = p.controlador === EU ? 'você' : 'oponente';
    if (p.tipo === 'carta') return `<li>${i === 0 ? '<b>topo</b> · ' : ''}${esc(nomeInst(p.instId))} (${quem})${p.alvo ? ` → ${esc(nomeInst(p.alvo))}` : ''}</li>`;
    if (p.tipo === 'ataque') return `<li>${i === 0 ? '<b>topo</b> · ' : ''}Ataque de ${esc(nomeInst(p.atacante))} em ${esc(nomeInst(p.alvo))} (${quem})</li>`;
    return `<li>${i === 0 ? '<b>topo</b> · ' : ''}Efeito: ${esc(frase(p.efeitos))} (${quem})${p.alvo ? ` → ${esc(nomeInst(p.alvo))}` : ''}</li>`;
  }).join('');
  const comb = e.combate ? `<div>Combate: ${esc(nomeInst(e.combate.atacante))} → ${esc(nomeInst(e.combate.alvo))}${e.combate.bloqueador ? `, bloqueado por ${esc(nomeInst(e.combate.bloqueador))}` : ''}</div>` : '';
  return `<div class="pilha"><b>Pilha</b>${itens ? `<ol style="margin:4px 0 0 18px;padding:0">${itens}</ol>` : ' vazia'}${comb}</div>`;
}
function cabecalho() {
  if (tela !== 'partida' || !e) { $('#cabecalho').textContent = ''; return; }
  const quem = (j) => (j === EU ? 'você' : 'oponente');
  $('#cabecalho').innerHTML = e.etapa === 'preparacao' ? 'Preparação' : `Turno ${e.turno} · ${ETAPA[e.etapa]} · vez de <b>${quem(e.ativo)}</b> · prioridade: ${e.pendencia ? quem(e.pendencia.jogador) + ' (decisão)' : quem(e.prioridade)}`;
}

function telaPartida() {
  const fim = partida.resultado;
  const barra = `<div class="linha" style="margin-bottom:10px">
    <button data-acao="voltar">← Início</button>
    ${!fim ? `<button class="primario" data-acao="passar" ${tenhoPrioridade() ? '' : 'disabled'}>Passar ${e.pilha.length ? '(deixar resolver)' : minhaJanela() ? '(encerrar a fase)' : ''}</button>
    <button data-acao="desfazer">Desfazer minha última ação</button>
    <button class="perigo" data-acao="encerrar">Encerrar partida</button>` : `<button class="primario" data-acao="revanche">Revanche</button>`}
    <span class="peq">Você: <b>${esc(conjDef(partida.eu).nome)}</b> × Oponente (jogador automático): <b>${esc(conjDef(partida.oponente).nome)}</b></span>
  </div>`;
  const abas = `<div class="abas">${[['mesa', 'Mesa'], ['comentarios', `Comentários (${partida.comentarios.length})`], ['registro', 'Registro'], ['cartas', 'Cartas'], ['opcoes', 'Opções']].map(([k, r]) => `<button class="${aba === k ? 'ativa' : ''}" data-aba="${k}">${r}</button>`).join('')}</div>`;
  let corpo = '';
  if (aba === 'mesa') {
    const eventos = eventosHTML();
    corpo = `${fim ? `<div class="ok"><b>Fim de jogo:</b> vencedor: ${esc(fim.vencedor)} · ${fim.turnos} turnos · ${esc(fim.ultimoEvento ?? '')}</div>` : ''}
      ${pendenciaHTML()}
      <div class="grade"><div>${ladoHTML(BOT)}${pilhaHTML()}${ladoHTML(EU)}</div>
      <div><div class="painel"><b>Acontecimentos</b> <span class="peq">(etapa mais recente em cima; dentro de cada etapa, na ordem em que aconteceu)</span><div class="eventos">${eventos}</div></div>
      ${comentarioRapido()}</div></div>`;
  } else if (aba === 'comentarios') {
    corpo = `${comentarioRapido()}<div class="painel"><b>Comentários desta partida</b>${listaComentarios(partida.comentarios)}</div>`;
  } else if (aba === 'registro') {
    const linhas = partida.comandos.map((c) => `#${c.n} T${c.turno} ${ETAPA[c.etapa] ?? c.etapa} · ${c.quem}: ${resumoCmd(c.cmd)}${c.resultado.length ? '\n    ' + c.resultado.map(traduzir).join('\n    ') : ''}`).join('\n');
    const erros = partida.erros.map((x) => `T${x.turno} ${x.quem}: ${resumoCmd(x.cmd)} → ${x.msg}`).join('\n');
    corpo = `<div class="painel"><b>Ações e resultados (${partida.comandos.length})</b><div class="registro">${esc(linhas)}</div></div>
      <div class="painel"><b>Tentativas recusadas pelas regras (${partida.erros.length})</b><div class="registro">${esc(erros) || 'nenhuma'}</div></div>`;
  } else if (aba === 'cartas') {
    corpo = listaBaralho(partida.eu, 'Seu baralho') + listaBaralho(partida.oponente, 'Baralho do oponente');
  } else {
    const c = arm.config;
    corpo = `<div class="painel"><b>Passes automáticos</b> <span class="peq">(para não precisar clicar em "Passar" a cada etapa)</span>
      <label class="opcao"><input type="checkbox" data-cfg="autoAtaques" ${c.autoAtaques ? 'checked' : ''}> Não parar quando o oponente declara ataque (o bloqueio continua sendo perguntado)</label>
      <label class="opcao"><input type="checkbox" data-cfg="pararFinalOponente" ${c.pararFinalOponente ? 'checked' : ''}> Parar no Final do turno do oponente quando eu tiver uma Repentina para jogar</label>
      <label class="opcao"><input type="checkbox" data-cfg="segurarPrioridade" ${c.segurarPrioridade ? 'checked' : ''}> Segurar a prioridade depois de jogar (para empilhar mais de uma carta)</label>
      <p class="peq">O app sempre para quando você precisa decidir algo, na sua fase Principal e quando o oponente joga uma carta à qual você pode responder.</p></div>`;
  }
  const versaoAntiga = DP !== DADOS ? `<div class="peq" style="margin-bottom:8px">Esta partida usa as cartas da versão ${esc(partida.dadosVersao ?? VERSAO_ANTES_DO_EDITOR)}, de antes das últimas edições. Novas partidas usam a versão atual (${esc(DADOS.versao)}).</div>` : '';
  return barra + versaoAntiga + (mensagem ? `<div class="${mensagem.tipo}">${esc(mensagem.texto)}</div>` : '') + (avisoSalvar ? `<div class="aviso">${esc(avisoSalvar)}</div>` : '') + abas + corpo;
}
/** Troca "jogador 0/1" por você/oponente nas linhas do motor. */
function traduzir(l) {
  return l.replace(/\bO jogador 0\b/g, 'Você').replace(/\bo jogador 0\b/g, 'você').replace(/\bdo jogador 0\b/g, 'de você')
    .replace(/\bO jogador 1\b/g, 'O oponente').replace(/\bo jogador 1\b/g, 'o oponente').replace(/\bdo jogador 1\b/g, 'do oponente')
    .replace(/\bJogador 0\b/g, 'Você').replace(/\bjogador 0\b/g, 'você').replace(/\bJogador 1\b/g, 'Oponente').replace(/\bjogador 1\b/g, 'oponente');
}
function eventosHTML() {
  const grupos = [];
  let atual = { titulo: 'Preparação', linhas: [] };
  for (const l of e.log) {
    const m = l.match(/^Turno (\d+) \(jogador (\d)\): (\w+)\.$/);
    if (m) { grupos.push(atual); atual = { titulo: `Turno ${m[1]} · vez ${m[2] === String(EU) ? 'sua' : 'do oponente'} · ${ETAPA[m[3]] ?? m[3]}`, linhas: [] }; continue; }
    atual.linhas.push(l);
  }
  grupos.push(atual);
  const mostrar = grupos.filter((g) => g.linhas.length || g === atual).slice(-14).reverse();
  return mostrar.map((g) => `<div class="ev-grupo"><div class="ev-titulo">${esc(g.titulo)}</div>${g.linhas.map((l) => `<div>${esc(traduzir(l))}</div>`).join('') || '<div class="peq">nada aconteceu ainda</div>'}</div>`).join('');
}
function resumoCmd(c) {
  const n = (id) => (id && e?.instancias[id] ? nomeInst(id) : id);
  switch (c.tipo) {
    case 'jogar': return `joga ${n(c.carta)}${c.alvo ? ' → ' + n(c.alvo) : ''}${c.enviarArcano?.length ? ` (envia ${c.enviarArcano.map(n).join(', ')})` : ''}${c.descartarGrau?.length ? ` (descarta do Grau ${c.descartarGrau.map(n).join(', ')})` : ''}`;
    case 'ativar': return `ativa ${n(c.carta)}${c.alvo ? ' → ' + n(c.alvo) : ''}`;
    case 'atacar': return `ataca com ${n(c.atacante)} → ${n(c.alvo)}`;
    case 'bloquear': return c.bloqueador ? `bloqueia com ${n(c.bloqueador)}` : 'não bloqueia';
    case 'colocarGrau': return `põe ${n(c.carta)} no Grau`;
    case 'suspenderGrau': return `suspende ${n(c.carta)} do Grau (+1 Arcano)`;
    case 'descartar': return `descarta ${c.cartas.map(n).join(', ')}`;
    case 'escolherInicio': return `escolhe quem começa: jogador ${c.primeiro}`;
    case 'pagarAnulacao': return c.pagar ? 'paga para anular' : 'não paga';
    case 'escolherDoTopo': return `fica com ${c.cartas.map(n).join(', ')} (olhar o topo)`;
    default: return 'passa';
  }
}
function comentarioRapido() {
  return `<div class="painel"><b>Comentar agora</b> <span class="peq">(guarda o turno, a etapa e as vidas)</span>
    <textarea id="novo-comentario" placeholder="O que você notou nesta jogada?"></textarea>
    <div class="linha"><button class="primario" data-acao="comentar">Adicionar comentário</button></div></div>`;
}
function listaComentarios(lista) {
  if (!lista.length) return '<p class="peq">Nenhum comentário ainda.</p>';
  return `<table><tr><th>Quando</th><th>Situação</th><th>Comentário</th></tr>${[...lista].reverse().map((c) => `<tr><td class="peq">${esc(new Date(c.ts).toLocaleString('pt-BR'))}</td><td class="peq">${c.turno !== undefined ? `T${c.turno} ${ETAPA[c.etapa] ?? ''} · vidas ${c.vidaVoce}×${c.vidaOponente}` : 'geral'}</td><td>${esc(c.texto)}</td></tr>`).join('')}</table>`;
}
function listaBaralho(conjId, titulo) {
  const c = conjDef(conjId);
  const fonte = tela === 'partida' ? DP : DADOS;
  const b = fonte.baralhos.find((x) => x.conjurador === conjId);
  const linhas = b.lista.map(([id, n]) => { const d = fonte.catalogo.cartas[id]; return `<tr><td>${n}×</td><td><b>${esc(d.nome)}</b><div class="peq">${TIPO[d.tipo]} · custo ${d.custo} · Grau ${d.grau}${d.velocidade === 'repentina' ? ' · Repentina' : ''}${d.vida !== undefined ? ` · ${d.ataque ?? 0}/${d.vida}` : ''} · ${esc(valoresTxt(d.valores))}</div></td><td>${esc(textoCarta(d))}</td></tr>`; }).join('');
  return `<div class="painel"><b>${esc(titulo)}: ${esc(c.nome)}</b> <span class="peq">${esc(valoresTxt(c.valores))}</span><p>${esc(c.habilidade)}</p><table>${linhas}</table></div>`;
}

function telaInicio() {
  const total = arm.partidas.length;
  const naoEnviadas = arm.partidas.filter((p) => !p.enviada).length;
  const opcoes = DADOS.baralhos.map((b) => `<option value="${b.conjurador}">${esc(conjDef(b.conjurador).nome)} (${esc(valoresTxt(conjDef(b.conjurador).valores))})</option>`).join('');
  const partidas = [...arm.partidas].reverse().map((p) => `<tr>
      <td class="peq">${esc(new Date(p.inicio).toLocaleString('pt-BR'))}</td>
      <td>${esc(conjDef(p.eu)?.nome ?? p.eu)} × ${esc(conjDef(p.oponente)?.nome ?? p.oponente)}</td>
      <td>${p.resultado ? `${esc(p.resultado.vencedor)} (${p.resultado.turnos} turnos${p.resultado.motivo === 'abandonada' ? ', abandonada' : ''})` : '<i>em andamento</i>'}</td>
      <td>${p.comandos.length} ações · ${p.comentarios.length} coment.</td>
      <td>${p.enviada ? 'enviada' : '<b>não enviada</b>'}</td>
      <td><button data-acao="abrir" data-id="${p.id}">${p.resultado ? 'Ver' : 'Continuar'}</button></td></tr>`).join('');
  return `${mensagem ? `<div class="${mensagem.tipo}">${esc(mensagem.texto)}</div>` : ''}${avisoSalvar ? `<div class="aviso">${esc(avisoSalvar)}</div>` : ''}
    <div class="painel centro">
      <button id="enviar" data-acao="enviar">ENVIAR</button>
      <p class="peq">Envia ao Claude todas as partidas, ações, resultados e comentários (${total} partida(s), ${naoEnviadas} ainda não enviada(s)).<br>
      Servidor local: ${servidorOnline === null ? 'verificando…' : servidorOnline ? '<b style="color:var(--verde)">conectado</b> (o envio vai para Arcanoclisma/tcg/envios/)' : '<b style="color:var(--vermelho)">desligado</b> (o envio baixa um arquivo .json para você mandar ao Claude)'}</p>
    </div>
    <div class="painel"><b>Nova partida</b>
      <div class="linha" style="margin-top:6px">Você joga com <select id="sel-eu" data-itens="1">${opcoes}</select> contra <select id="sel-op"><option value="aleatorio">um Conjurador aleatório</option>${opcoes}</select>
      <button class="primario" data-acao="nova">Começar</button></div>
      <div class="linha" id="itens-eu" style="margin-top:6px"></div>
      <p class="peq">O oponente é o jogador automático das simulações. Cada partida usa os baralhos atuais de cada Conjurador (aba Cartas, ou "Ver Conjuradores e baralhos").</p>
    </div>
    <div class="painel linha"><a class="botao-link" href="./editor.html">Editor de cartas e mecânicas</a>
      <span class="peq">Edite cartas, Conjuradores, regras, régua de pontos e baralhos para balancear. As novas partidas e as simulações usam o que você salvar lá.${DADOS.edicoes?.historico?.length ? ` Última edição: ${esc(new Date(DADOS.edicoes.historico.at(-1).quando).toLocaleString('pt-BR'))}.` : ''}</span></div>
    <div class="painel"><b>Partidas</b>${partidas ? `<table><tr><th>Início</th><th>Confronto</th><th>Resultado</th><th>Registro</th><th>Envio</th><th></th></tr>${partidas}</table>` : '<p class="peq">Nenhuma partida ainda.</p>'}</div>
    <div class="painel"><b>Comentários gerais</b> <span class="peq">(fora das partidas)</span>
      <textarea id="comentario-geral" placeholder="Ideias, dúvidas, regras para rever…"></textarea>
      <div class="linha"><button class="primario" data-acao="comentarGeral">Adicionar</button><button data-acao="verConj">Ver Conjuradores e baralhos</button></div>
      ${listaComentarios(arm.comentariosGerais)}</div>
    <p class="peq">Dados do jogo gerados em ${esc(new Date(DADOS.geradoEm).toLocaleString('pt-BR'))} (variante ${esc(DADOS.variante)}, versão das cartas ${esc(DADOS.versao)}). Tudo fica salvo neste navegador até você enviar (e continua salvo depois).</p>`;
}
function telaConjuradores() {
  return `<div class="linha" style="margin-bottom:10px"><button data-acao="voltar">← Início</button></div>${DADOS.baralhos.map((b) => listaBaralho(b.conjurador, 'Conjurador')).join('')}`;
}

function render() {
  cabecalho();
  const app = $('#app');
  const aberto = [...app.querySelectorAll('details')].map((d) => d.open);
  const rascunho = $('#novo-comentario')?.value ?? '';
  app.innerHTML = tela === 'partida' ? telaPartida() : tela === 'conjuradores' ? telaConjuradores() : telaInicio();
  if (tela === 'inicio') montarItensEu();
  if (tela === 'partida') {
    const ds = [...app.querySelectorAll('details')];
    if (ds.length === aberto.length) ds.forEach((d, i) => { d.open = aberto[i]; });
    if ($('#novo-comentario')) $('#novo-comentario').value = rascunho;
  }
}

// ---------- envio ----------
async function enviar() {
  const pacote = {
    app: 'arcanoclisma-testes', versaoApp: VERSAO_APP, enviadoEm: new Date().toISOString(), dadosGeradosEm: DADOS.geradoEm, variante: DADOS.variante,
    conjuradores: Object.values(DADOS.catalogo.conjuradores).filter((c) => c.id !== 'padrao').map((c) => ({ id: c.id, nome: c.nome, valores: c.valores, habilidade: c.habilidade })),
    partidas: arm.partidas, comentariosGerais: arm.comentariosGerais,
    dadosVersao: DADOS.versao, edicoes: DADOS.edicoes ?? null,
  };
  try {
    const r = await fetch('/enviar', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(pacote) });
    const j = await r.json();
    if (!j.ok) throw new Error(j.erro ?? 'falha no envio');
    for (const p of arm.partidas) p.enviada = true;
    arm.envios.push({ ts: pacote.enviadoEm, arquivo: j.arquivo });
    mensagem = { tipo: 'ok', texto: `Enviado! O Claude encontra os dados em Arcanoclisma/tcg/${j.arquivo}.` };
  } catch (erro) {
    const blob = new Blob([JSON.stringify(pacote, null, 1)], { type: 'application/json' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `arcanoclisma-envio-${pacote.enviadoEm.replace(/[:.]/g, '-')}.json`;
    a.click();
    mensagem = { tipo: 'aviso', texto: `O servidor local não respondeu (${erro.message}). Baixei o arquivo ${a.download}: mande-o para o Claude, ou abra o app pelo INICIAR.bat e envie de novo.` };
  }
  salvar();
  render();
}

// ---------- eventos ----------
document.addEventListener('click', (ev) => {
  const b = ev.target.closest('[data-acao], [data-aba]');
  if (!b || b.closest('#modal')) return;
  if (b.dataset.aba) { aba = b.dataset.aba; mensagem = null; render(); return; }
  const a = b.dataset.acao, id = b.dataset.id;
  if (a === 'enviar') { enviar(); return; }
  if (a === 'nova') { novaPartida($('#sel-eu').value, $('#sel-op').value, [...document.querySelectorAll('select[data-slot]')].map((s) => s.value).filter(Boolean)); return; }
  if (a === 'abrir') { abrirPartida(id); return; }
  if (a === 'voltar') { tela = 'inicio'; mensagem = null; DP = DADOS; render(); return; }
  if (a === 'verConj') { tela = 'conjuradores'; render(); return; }
  if (a === 'revanche') { novaPartida(partida.eu, partida.oponente); return; }
  if (a === 'desfazer') { desfazer(); return; }
  if (a === 'encerrar') { encerrar(); return; }
  if (a === 'comentar') {
    const t = $('#novo-comentario').value.trim();
    if (!t) return;
    partida.comentarios.push({ ts: new Date().toISOString(), turno: e.turno, etapa: e.etapa, vidaVoce: vidaDe(EU), vidaOponente: vidaDe(BOT), acoesAteAqui: partida.comandos.length, texto: t });
    $('#novo-comentario').value = '';
    salvar(); mensagem = { tipo: 'ok', texto: 'Comentário guardado.' }; render(); return;
  }
  if (a === 'comentarGeral') {
    const t = $('#comentario-geral').value.trim();
    if (!t) return;
    arm.comentariosGerais.push({ ts: new Date().toISOString(), texto: t });
    salvar(); render(); return;
  }
  acao(a, id);
});
document.addEventListener('change', (ev) => {
  if (ev.target.id === 'sel-eu') montarItensEu();
  const k = ev.target.dataset?.cfg;
  if (!k) return;
  arm.config[k] = ev.target.checked;
  salvar();
});
function vidaDe(j) { const id = e.jogadores[j].conjuradorId; return conjDef(e.instancias[id].defId).vida - e.instancias[id].dano; }

fetch('/ping').then((r) => { servidorOnline = r.ok; }).catch(() => { servidorOnline = false; }).finally(() => { if (tela === 'inicio') render(); });
render();
