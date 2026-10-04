// Partida online: um Durable Object por sala. Roda o motor real do TCG e é a única fonte da verdade:
// recebe os comandos dos dois jogadores por WebSocket, confere com as regras e manda a cada um só a visão dele.
// Guarda a semente e a lista de comandos (a partida é reconstruída a partir deles quando o objeto acorda).
// Prazo de decisão: quem precisa agir e não age a tempo tem a ação mais segura feita por ele (passar, não bloquear...);
// muitas vezes seguidas, ou desconectado por muito tempo, perde a partida.
import { DurableObject } from 'cloudflare:workers';
import { criarJogo, ErroRegra, executar } from '../../tcg/src/motor.ts';
import type { BaralhoDef, Catalogo, Comando, Estado, Jogador, Regras } from '../../tcg/src/tipos.ts';
import { agora, type Env } from './conta.ts';
import { visaoDe } from './visao.ts';
import DADOS from './gerado/dados.json';
import { aplicarCatalogo, type EdicoesSite } from './edicoes.ts';
import { edicoesAtuais } from './edicoes-db.ts';

const PRAZO_MS = 75_000;
const PRAZO_DESCONECTADO_MS = 8_000;
const MAX_TIMEOUTS = 6;
const DESCONECTADO_PERDE_MS = 180_000;

interface Lugar { id: string; nome: string; baralho: { def: BaralhoDef; nome: string } }
interface Meta {
  id: string; status: 'aguardando' | 'jogando' | 'fim'; lugares: Lugar[]; semente: number; inicio: number;
  vencedor: number | 'empate' | null; motivo: string; timeouts: [number, number]; desconectado: [number, number]; prazo: number; versao: string;
  /** Edições do autor em vigor no início da partida (a partida inteira usa esta versão, mesmo que o autor edite depois). */
  edicoes?: EdicoesSite; edVersao?: number;
}
type Registro = { cmd: Comando; t: number; auto?: boolean };

export class Partida extends DurableObject<Env> {
  private meta: Meta | null = null;
  private comandos: Registro[] = [];
  private e: Estado | null = null;
  private chat: { de: string; texto: string; t: number }[] = [];

  private async carregar(id: string): Promise<Meta> {
    if (this.meta) return this.meta;
    this.meta = (await this.ctx.storage.get<Meta>('meta')) ?? {
      id, status: 'aguardando', lugares: [], semente: 0, inicio: 0, vencedor: null, motivo: '', timeouts: [0, 0], desconectado: [0, 0], prazo: 0, versao: DADOS.versao,
    };
    this.comandos = (await this.ctx.storage.get<Registro[]>('comandos')) ?? [];
    this.chat = (await this.ctx.storage.get<typeof this.chat>('chat')) ?? [];
    if (this.meta.status !== 'aguardando') this.reconstruir();
    return this.meta;
  }
  private reconstruir(): void {
    const m = this.meta!;
    this.e = criarJogo({ catalogo: aplicarCatalogo(DADOS.catalogo as unknown as Catalogo, m.edicoes), baralhos: [m.lugares[0].baralho.def, m.lugares[1].baralho.def], semente: m.semente, regras: DADOS.regras as Partial<Regras> });
    for (const r of this.comandos) executar(this.e, r.cmd);
  }
  private async gravar(): Promise<void> {
    await this.ctx.storage.put({ meta: this.meta!, comandos: this.comandos, chat: this.chat.slice(-60) });
  }
  private async avisarSaguao(): Promise<void> {
    const m = this.meta!;
    const estado = m.status === 'fim' ? 'fim' : m.status === 'jogando' ? 'jogando' : 'aguardando';
    const jogadores = m.lugares.map((l) => ({ nome: l.nome, conjurador: l.baralho.def.conjurador }));
    try {
      await this.env.SAGUAO.get(this.env.SAGUAO.idFromName('saguao')).fetch('https://saguao/atualizar', { method: 'POST', body: JSON.stringify({ id: m.id, estado, jogadores }) });
    } catch (err) { console.error('saguão', err); }
  }

  private sockets(): { ws: WebSocket; lugar: number }[] {
    return this.ctx.getWebSockets().map((ws) => ({ ws, lugar: (ws.deserializeAttachment() as { lugar: number } | null)?.lugar ?? -1 }));
  }
  private conectado(lugar: number): boolean {
    return this.sockets().some((s) => s.lugar === lugar);
  }
  private enviar(ws: WebSocket, dados: unknown): void {
    try { ws.send(JSON.stringify(dados)); } catch { /* conexão caiu */ }
  }
  private pacote(lugar: number): unknown {
    const m = this.meta!;
    const base = {
      t: 'estado', eu: lugar, status: m.status, nomes: m.lugares.map((l) => l.nome), baralhos: m.lugares.map((l) => l.baralho.nome),
      conectados: [0, 1].map((k) => this.conectado(k)), vencedor: m.vencedor, motivo: m.motivo, prazo: m.prazo, versao: m.versao, edVersao: m.edVersao ?? 0, chat: this.chat.slice(-30),
    };
    if (!this.e) return base;
    const quem = lugar === 1 ? 1 : 0;
    return { ...base, estado: visaoDe(this.e, quem as Jogador), log: this.e.log.slice(-250), logTotal: this.e.log.length, ultimo: this.comandos.at(-1) ?? null };
  }
  private difundir(): void {
    for (const { ws, lugar } of this.sockets()) this.enviar(ws, this.pacote(lugar));
  }

  async fetch(req: Request): Promise<Response> {
    const id = new URL(req.url).pathname.split('/')[1];
    const m = await this.carregar(id);
    const usuario = JSON.parse(req.headers.get('x-arc-usuario') ?? 'null') as { id: string; nome: string } | null;
    const baralho = JSON.parse(req.headers.get('x-arc-baralho') ?? 'null') as Lugar['baralho'] | null;
    if (!usuario || !baralho) return new Response('sem usuário', { status: 400 });
    const par = new WebSocketPair();
    const [cliente, servidor] = [par[0], par[1]];
    let lugar = m.lugares.findIndex((l) => l.id === usuario.id);
    if (lugar < 0 && m.status === 'aguardando' && m.lugares.length < 2) {
      m.lugares.push({ id: usuario.id, nome: usuario.nome, baralho });
      lugar = m.lugares.length - 1;
    } else if (lugar >= 0 && m.status === 'aguardando') m.lugares[lugar].baralho = baralho;
    this.ctx.acceptWebSocket(servidor);
    servidor.serializeAttachment({ lugar });
    if (lugar < 0) {
      this.enviar(servidor, { t: 'erro', fatal: true, msg: 'Esta sala já está cheia.' });
      servidor.close(4000, 'cheia');
      return new Response(null, { status: 101, webSocket: cliente });
    }
    if (m.status === 'jogando') m.desconectado[lugar] = 0;
    if (m.status === 'aguardando' && m.lugares.length === 2) {
      m.status = 'jogando';
      m.semente = crypto.getRandomValues(new Uint32Array(1))[0] >>> 1;
      m.inicio = agora();
      const ed = await edicoesAtuais(this.env);
      m.edicoes = ed.dados;
      m.edVersao = ed.id;
      this.comandos = [];
      this.reconstruir();
      this.marcarPrazo();
    }
    await this.gravar();
    await this.avisarSaguao();
    this.difundir();
    return new Response(null, { status: 101, webSocket: cliente });
  }

  /** Quem precisa agir agora. */
  private ator(): Jogador | null {
    const e = this.e;
    if (!e || e.vencedor !== null) return null;
    return e.pendencia ? e.pendencia.jogador : e.prioridade;
  }
  private marcarPrazo(): void {
    const m = this.meta!;
    const a = this.ator();
    if (a === null || m.status !== 'jogando') { m.prazo = 0; void this.ctx.storage.deleteAlarm(); return; }
    m.prazo = agora() + (this.conectado(a) ? PRAZO_MS : PRAZO_DESCONECTADO_MS);
    void this.ctx.storage.setAlarm(m.prazo);
  }

  /** Executa um comando de um jogador; devolve a mensagem de erro de regra, se houver. */
  private aplicar(cmd: Comando, auto = false): string | null {
    try {
      executar(this.e!, cmd);
    } catch (err) {
      if (err instanceof ErroRegra) return err.message;
      throw err;
    }
    this.comandos.push({ cmd, t: agora(), ...(auto ? { auto: true } : {}) });
    return null;
  }

  private async encerrar(vencedor: number | 'empate' | null, motivo: string): Promise<void> {
    const m = this.meta!;
    m.status = 'fim';
    m.vencedor = vencedor;
    m.motivo = motivo;
    m.prazo = 0;
    await this.ctx.storage.deleteAlarm();
    try {
      await this.env.DB.prepare('INSERT OR REPLACE INTO partidas (id, modo, j0, j1, conj0, conj1, vencedor, turnos, motivo, inicio, fim, versao) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)')
        .bind(m.id, 'online', m.lugares[0]?.id ?? null, m.lugares[1]?.id ?? null, m.lugares[0]?.baralho.def.conjurador ?? null, m.lugares[1]?.baralho.def.conjurador ?? null,
          typeof vencedor === 'number' ? vencedor : null, this.e?.turno ?? 0, motivo, m.inicio, agora(), m.versao).run();
    } catch (err) { console.error('registro da partida', err); }
    await this.avisarSaguao();
  }

  private async depoisDeAgir(): Promise<void> {
    if (this.e && this.e.vencedor !== null && this.meta!.status === 'jogando') await this.encerrar(this.e.vencedor, 'fim de jogo');
    else this.marcarPrazo();
    await this.gravar();
    this.difundir();
  }

  async webSocketMessage(ws: WebSocket, texto: string | ArrayBuffer): Promise<void> {
    const m = await this.carregar(this.ctx.id.name ?? '');
    const lugar = (ws.deserializeAttachment() as { lugar: number }).lugar;
    let msg: { t: string; cmd?: Comando; texto?: string };
    try { msg = JSON.parse(typeof texto === 'string' ? texto : new TextDecoder().decode(texto)); } catch { return; }
    if (msg.t === 'ping') { this.enviar(ws, { t: 'pong' }); return; }
    if (msg.t === 'estado') { this.enviar(ws, this.pacote(lugar)); return; }
    if (msg.t === 'chat' && msg.texto) {
      this.chat.push({ de: m.lugares[lugar]?.nome ?? '?', texto: String(msg.texto).slice(0, 300), t: agora() });
      await this.gravar();
      this.difundir();
      return;
    }
    if (msg.t === 'desistir' && m.status === 'jogando') {
      await this.encerrar(1 - lugar, 'desistência');
      await this.gravar();
      this.difundir();
      return;
    }
    if (msg.t === 'cmd' && msg.cmd) {
      if (m.status !== 'jogando' || !this.e) { this.enviar(ws, { t: 'erro', msg: 'A partida não está em andamento.' }); return; }
      const cmd = { ...msg.cmd, jogador: lugar as Jogador } as Comando;
      const falha = this.aplicar(cmd);
      if (falha) { this.enviar(ws, { t: 'erro', msg: falha, cmd }); return; }
      m.timeouts[lugar] = 0;
      await this.depoisDeAgir();
    }
  }

  async webSocketClose(ws: WebSocket): Promise<void> {
    const m = await this.carregar(this.ctx.id.name ?? '');
    const lugar = (ws.deserializeAttachment() as { lugar: number } | null)?.lugar ?? -1;
    ws.close();
    if (lugar < 0) return;
    const aindaConectado = this.sockets().some((s) => s.lugar === lugar && s.ws !== ws);
    if (aindaConectado) return;
    if (m.status === 'aguardando') {
      m.lugares.splice(lugar, 1);
      for (const s of this.sockets()) if (s.lugar > lugar) s.ws.serializeAttachment({ lugar: s.lugar - 1 });
      if (!m.lugares.length) m.status = 'fim';
      await this.gravar();
      await this.avisarSaguao();
    } else if (m.status === 'jogando') {
      m.desconectado[lugar] = agora();
      if (this.ator() === lugar) this.marcarPrazo();
      await this.gravar();
    }
    this.difundir();
  }
  async webSocketError(ws: WebSocket): Promise<void> {
    await this.webSocketClose(ws);
  }

  /** Prazo esgotado: faz a ação mais segura por quem precisava agir. */
  async alarm(): Promise<void> {
    const m = await this.carregar(this.ctx.id.name ?? '');
    if (m.status !== 'jogando' || !this.e) return;
    if (m.prazo > agora() + 500) { await this.ctx.storage.setAlarm(m.prazo); return; }
    const a = this.ator();
    if (a === null) return;
    for (const k of [0, 1] as Jogador[]) {
      if (m.desconectado[k] && !this.conectado(k) && agora() - m.desconectado[k] > DESCONECTADO_PERDE_MS) {
        await this.encerrar(1 - k, 'abandono (desconectado)');
        await this.gravar();
        this.difundir();
        return;
      }
    }
    m.timeouts[a]++;
    if (m.timeouts[a] > MAX_TIMEOUTS && this.conectado(a)) {
      await this.encerrar(1 - a, 'abandono (tempo esgotado)');
      await this.gravar();
      this.difundir();
      return;
    }
    const e = this.e;
    const z = e.jogadores[a];
    const p = e.pendencia;
    let cmd: Comando;
    if (p?.tipo === 'escolherInicio') cmd = { tipo: 'escolherInicio', jogador: a, primeiro: a };
    else if (p?.tipo === 'bloqueio') cmd = { tipo: 'bloquear', jogador: a, bloqueador: null };
    else if (p?.tipo === 'distribuirDano') cmd = { tipo: 'distribuirDano', jogador: a };
    else if (p?.tipo === 'descartar') cmd = { tipo: 'descartar', jogador: a, cartas: z.mao.slice(-p.quantidade) };
    else if (p?.tipo === 'pagarAnulacao') cmd = { tipo: 'pagarAnulacao', jogador: a, pagar: false };
    else if (p?.tipo === 'escolherDoTopo') cmd = { tipo: 'escolherDoTopo', jogador: a, cartas: p.cartas.slice(0, p.pegar) };
    else if (p?.tipo === 'escolherCartas') cmd = { tipo: 'escolherCartas', jogador: a, cartas: p.cartas.slice(0, p.quantidade) };
    else cmd = { tipo: 'passar', jogador: a };
    const falha = this.aplicar(cmd, true);
    if (falha) console.error('ação automática falhou', falha, cmd);
    await this.depoisDeAgir();
  }
}

