// Teste de ponta a ponta do modo online contra o servidor local (wrangler dev em http://localhost:8787):
// cria duas contas de teste, pareia as duas pela partida rápida e deixa dois bots jogarem uma partida inteira
// pelas salas (WebSocket), usando só a visão que o servidor manda a cada jogador.
// Uso: node test/online-local.mjs   (com o servidor local rodando)
import { randomBytes } from 'node:crypto';
import { decidir } from '../public/jogo/bot.js';

const BASE = process.env.BASE ?? 'http://localhost:8787';
const DADOS = await (await fetch(BASE + '/jogo/dados.json')).json();
DADOS.catalogo.cartas.oculta = { id: 'oculta', nome: 'Carta virada', tipo: 'oculta', subtipos: [], valores: [], custo: 99, grau: 99, velocidade: 'findante', arcanoGerado: 0, texto: '' };

async function conta(nome) {
  const email = `${nome.split(" ")[0].toLowerCase()}-${randomBytes(3).toString("hex")}@teste.local`;
  const r = await fetch(BASE + '/api/conta/registrar', { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify({ email, senha: randomBytes(9).toString('base64url'), nome }) });
  if (!r.ok) throw new Error('registro: ' + (await r.text()));
  return r.headers.get('set-cookie').split(';')[0];
}
const api = async (cookie, caminho, corpo) => {
  const r = await fetch(BASE + '/api' + caminho, { method: corpo ? 'POST' : 'GET', headers: { cookie, 'content-type': 'application/json' }, body: corpo ? JSON.stringify(corpo) : undefined });
  return r.json();
};

function jogador(nome, cookie, salaId, baralho) {
  return new Promise((ok, falha) => {
    const ws = new WebSocket(`${BASE.replace('http', 'ws')}/api/partida/${salaId}/ws?baralho=${baralho}`, { headers: { cookie } });
    let ultimo = null, enviados = 0, erros = 0;
    const estilo = DADOS.baralhos.find((b) => `pronto:${b.conjurador}` === baralho).estilo;
    ws.onmessage = (ev) => {
      const m = JSON.parse(ev.data);
      if (m.t === 'erro') {
        erros++;
        if (m.fatal) falha(new Error(m.msg));
        // jogada recusada pelo servidor: passa (ou decide a pendência do jeito mais simples)
        const e = ultimo?.estado; if (!e) return;
        const p = e.pendencia;
        const eu = ultimo.eu;
        const cmd = p?.jogador === eu ? ({ bloqueio: { tipo: 'bloquear', bloqueador: null }, pagarAnulacao: { tipo: 'pagarAnulacao', pagar: false }, escolherDoTopo: { tipo: 'escolherDoTopo', cartas: p.cartas?.slice(0, p.pegar) }, descartar: { tipo: 'descartar', cartas: e.jogadores[eu].mao.slice(-p.quantidade) }, escolherInicio: { tipo: 'escolherInicio', primeiro: eu } })[p.tipo] : { tipo: 'passar' };
        ws.send(JSON.stringify({ t: 'cmd', cmd }));
        return;
      }
      if (m.t !== 'estado') return;
      ultimo = m;
      if (m.status === 'fim') { ws.close(); ok({ nome, eu: m.eu, vencedor: m.vencedor, motivo: m.motivo, turnos: m.estado?.turno, enviados, erros, oculto: contarOcultas(m), edicoes: m.edVersao }); return; }
      const e = m.estado;
      if (!e) return;
      e.catalogo = DADOS.catalogo;
      const ator = e.pendencia ? e.pendencia.jogador : e.prioridade;
      if (ator !== m.eu || e.vencedor !== null) return;
      let cmd;
      try { cmd = decidir(e, m.eu, estilo); } catch { cmd = { tipo: 'passar', jogador: m.eu }; }
      enviados++;
      ws.send(JSON.stringify({ t: 'cmd', cmd }));
    };
    ws.onerror = (e) => falha(new Error('ws ' + nome + ': ' + (e.message ?? 'erro')));
  });
}
/** Confere o sigilo: a mão do oponente e os baralhos chegam como "oculta". */
function contarOcultas(m) {
  const e = m.estado; const op = 1 - m.eu;
  const vaza = e.jogadores[op].mao.some((id) => e.instancias[id].defId !== 'oculta') || e.jogadores[m.eu].baralho.some((id) => e.instancias[id].defId !== 'oculta');
  return vaza ? 'VAZOU' : 'ok';
}

if (process.env.SALA) {
  // modo convidado: um bot entra numa sala já aberta (por exemplo, criada no navegador) e joga até o fim
  const cb = await conta('Bot Convidado');
  console.log(await jogador('Convidado', cb, process.env.SALA, process.env.BARALHO ?? 'pronto:osmar'));
  process.exit(0);
}
const [ca, cb] = await Promise.all([conta('Ana Teste'), conta('Beto Teste')]);
const ra = await api(ca, '/salas/rapida', {});
const pa = jogador('A', ca, ra.id, 'pronto:lyandra');
await new Promise((r) => setTimeout(r, 400));
const rb = await api(cb, '/salas/rapida', {});
console.log('salas:', ra, rb, ra.id === rb.id ? '(pareados)' : '(NÃO pareados)');
const pb = jogador('B', cb, rb.id, 'pronto:osmar');
const t0 = Date.now();
const res = await Promise.race([Promise.all([pa, pb]), new Promise((_, f) => setTimeout(() => f(new Error('tempo esgotado')), 240000))]);
console.log(res, `${((Date.now() - t0) / 1000).toFixed(1)}s`);
const lista = await api(ca, '/salas');
console.log('saguão depois:', JSON.stringify(lista));
