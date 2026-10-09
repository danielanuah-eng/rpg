// Partida online: conversa com o Durable Object da sala por WebSocket. O servidor manda só a visão deste jogador.
import { DADOS, EDICOES, carregarEdicoes } from './base.js';
import { sincronizarEdicoesDaSala } from './sincronizar-edicoes.js';

export function conectarPartida(id, { baralho, itens }, { aoEstado, aoErro, aoFechar }) {
  const ouvintes = new Set();
  let ultimo = null;
  let ws = null;
  let fechadoDeProposito = false;
  let tentativas = 0;
  let pendente = null;
  const fonte = {
    modo: 'online', eu: 0, nomes: ['?', '?'], versao: 0, e: null,
    ouvir(f) { ouvintes.add(f); },
    enviar(cmd) {
      return new Promise((ok) => {
        if (!ws || ws.readyState !== 1) { ok('Sem conexão com a sala.'); return; }
        pendente = ok;
        ws.send(JSON.stringify({ t: 'cmd', cmd }));
        setTimeout(() => { if (pendente === ok) { pendente = null; ok(null); } }, 8000);
      });
    },
    chat(texto) { ws?.send(JSON.stringify({ t: 'chat', texto })); },
    mensagens() { return ultimo?.chat ?? []; },
    desistir() { ws?.send(JSON.stringify({ t: 'desistir' })); },
    prazo() { return ultimo?.prazo || 0; },
    conectados() { return ultimo?.conectados ?? [true, true]; },
    motivo() { return ultimo?.motivo ?? ''; },
    pacote() { return ultimo; },
    fechar() { fechadoDeProposito = true; ws?.close(); ouvintes.clear(); },
  };
  function abrir() {
    const proto = location.protocol === 'https:' ? 'wss' : 'ws';
    const q = new URLSearchParams({ baralho, ...(itens ? { itens: itens.join(',') } : {}) });
    ws = new WebSocket(`${proto}://${location.host}/api/partida/${id}/ws?${q}`);
    // Abrir o socket não garante que as edições carregaram; manter o limite de reconexões.
    // Mensagens em fila: se a sala usa outra versão das edições do autor, ela é carregada antes de desenhar.
    let fila = Promise.resolve();
    ws.onmessage = (ev) => { fila = fila.then(() => tratar(JSON.parse(ev.data))); };
    const tratar = async (m) => {
      if (!await sincronizarEdicoesDaSala(m, EDICOES, carregarEdicoes)) {
        aoErro?.('Não foi possível carregar as artes e edições desta sala. Reconectando.', false);
        // Reconectar pede uma visão atual; não desenhar usando cosméticos de outra versão.
        ws?.close();
        return;
      }
      if (m.t === 'erro') {
        if (pendente) { const p = pendente; pendente = null; p(m.msg); }
        else aoErro?.(m.msg, m.fatal);
        if (m.fatal) { fechadoDeProposito = true; }
        return;
      }
      if (m.t !== 'estado') return;
      tentativas = 0;
      ultimo = m;
      fonte.eu = m.eu;
      fonte.nomes = m.nomes;
      if (m.estado) {
        m.estado.catalogo = DADOS.catalogo;
        m.estado.log = m.log;
        m.estado.logTotal = m.logTotal;
        // desistência e abandono encerram a sala sem mexer no motor: o vencedor vem da sala
        if (m.status === 'fim' && m.estado.vencedor === null && m.vencedor !== null) m.estado.vencedor = m.vencedor;
        fonte.e = m.estado;
      }
      if (pendente) { const p = pendente; pendente = null; p(null); }
      fonte.versao++;
      aoEstado?.(m, fonte);
      for (const f of ouvintes) f();
    };
    ws.onclose = () => {
      if (fechadoDeProposito) { aoFechar?.(); return; }
      if (tentativas++ < 8) setTimeout(abrir, Math.min(8000, 600 * tentativas));
      else aoErro?.('A conexão com a sala caiu.', true);
    };
  }
  abrir();
  const ping = setInterval(() => { if (ws?.readyState === 1) ws.send('{"t":"ping"}'); }, 25000);
  const fecharOriginal = fonte.fechar;
  fonte.fechar = () => { clearInterval(ping); fecharOriginal(); };
  return fonte;
}
