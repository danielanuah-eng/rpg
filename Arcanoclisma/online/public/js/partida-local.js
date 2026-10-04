// Partida contra o bot: o motor e o jogador automático das simulações rodam no navegador.
// As jogadas do bot têm uma pausa curta para você ver cada uma (os passes dele são imediatos).
// A partida em andamento fica guardada no navegador (semente + comandos) e pode ser retomada.
import { criarJogo, executar, ErroRegra } from '/jogo/motor.js';
import { decidir } from '/jogo/bot.js';
import { DADOS, local } from './base.js';

const EU = 0, BOT = 1;

/**
 * cfg: { eu: BaralhoDef, bot: BaralhoDef, estiloBot, nomes, semente?, comandos?, modo?, aoTerminar? }
 */
export function criarPartidaLocal(cfg) {
  const semente = cfg.semente ?? Math.floor(Math.random() * 2 ** 31);
  const comandos = [];
  let e = cfg.estado ?? criarJogo({ catalogo: DADOS.catalogo, baralhos: [cfg.eu, cfg.bot], semente, regras: DADOS.regras });
  for (const c of cfg.comandos ?? []) { try { executar(e, c); comandos.push(c); } catch { break; } }
  const ouvintes = new Set();
  let timer = null;
  let terminou = false;

  const fonte = {
    modo: cfg.modo ?? 'bot', eu: EU, nomes: cfg.nomes ?? ['Você', 'Bot'], versao: 0,
    get e() { return e; },
    ouvir(f) { ouvintes.add(f); },
    async enviar(cmd) {
      const erro = aplicar({ ...cmd, jogador: EU });
      if (!erro) mudou();
      return erro;
    },
    motivo() { return e.log[e.log.length - 1]; },
    revanche: cfg.revanche,
    parar() { clearTimeout(timer); ouvintes.clear(); },
  };
  function aplicar(cmd) {
    try { executar(e, cmd); } catch (erro) { if (erro instanceof ErroRegra) return erro.message; throw erro; }
    comandos.push(cmd);
    return null;
  }
  function gravar() {
    if (cfg.chaveSalvar) local.gravar(cfg.chaveSalvar, e.vencedor === null ? { ...cfg.salvar, semente, comandos, versao: DADOS.versao } : null);
  }
  function mudou() {
    fonte.versao++;
    gravar();
    for (const f of ouvintes) f();
    if (e.vencedor !== null && !terminou) { terminou = true; cfg.aoTerminar?.(e); }
    agendarBot();
  }
  function ator() { return e.vencedor !== null ? null : e.pendencia ? e.pendencia.jogador : e.prioridade; }
  function agendarBot() {
    clearTimeout(timer);
    if (ator() !== BOT) return;
    const cmd = decidirBot();
    if (cmd === 'esperar') { timer = setTimeout(agendarBot, 600); return; }
    // ritmo do bot mais calmo (pedido do autor, 04/10): cada jogada espera a animação anterior terminar
    const pausa = cmd.tipo === 'passar' ? 450 : cmd.tipo === 'suspenderGrau' ? 750 : 1500;
    timer = setTimeout(() => {
      if (ator() !== BOT) return;
      let erro = aplicar(cmd);
      if (erro) {
        // jogada inválida do bot: passa (ou a decisão mais segura da pendência)
        const p = e.pendencia;
        const seguro = p?.tipo === 'bloqueio' ? { tipo: 'bloquear', jogador: BOT, bloqueador: null }
          : p?.tipo === 'pagarAnulacao' ? { tipo: 'pagarAnulacao', jogador: BOT, pagar: false }
          : p?.tipo === 'escolherDoTopo' ? { tipo: 'escolherDoTopo', jogador: BOT, cartas: p.cartas.slice(0, p.pegar) }
          : p?.tipo === 'descartar' ? { tipo: 'descartar', jogador: BOT, cartas: e.jogadores[BOT].mao.slice(-p.quantidade) }
          : p?.tipo === 'escolherInicio' ? { tipo: 'escolherInicio', jogador: BOT, primeiro: BOT }
          : { tipo: 'passar', jogador: BOT };
        erro = aplicar(seguro);
        if (erro) console.error('bot travou', erro, cmd);
      }
      mudou();
    }, pausa);
  }
  function decidirBot() {
    if (cfg.roteiroBot) { const r = cfg.roteiroBot(e); if (r) return r; }
    try { return decidir(e, BOT, cfg.estiloBot); } catch (err) { console.error(err); return { tipo: 'passar', jogador: BOT }; }
  }
  /** Para o tutorial: troca o estado (cenário montado) e recomeça a lista de comandos. */
  fonte.substituir = (novo) => { e = novo; comandos.length = 0; terminou = false; mudou(); };
  fonte.cutucar = () => agendarBot();
  fonte.comandos = comandos;
  // na abertura o D20 rola por ~4,2 s: o bot espera o dado parar antes de escolher quem começa
  setTimeout(agendarBot, e.pendencia?.tipo === 'escolherInicio' ? 6000 : 400);
  return fonte;
}

export const LOCAL = { EU, BOT };
