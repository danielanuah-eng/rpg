// Saguão: um único Durable Object que guarda as salas abertas, os códigos de sala privada e a fila da partida rápida.
// Só o Worker e as Partidas falam com ele (não é exposto direto na internet).
import { DurableObject } from 'cloudflare:workers';
import type { Env } from './conta.ts';

export interface Sala {
  id: string; codigo: string; nome: string; privada: boolean; rapida: boolean;
  dono: string; donoNome: string; estado: 'aguardando' | 'pareando' | 'jogando' | 'fim';
  jogadores: { nome: string; conjurador: string }[]; criada: number; atualizada: number;
}

const LETRAS = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
const codigoNovo = () => Array.from(crypto.getRandomValues(new Uint8Array(5)), (b) => LETRAS[b % LETRAS.length]).join('');
const resp = (d: unknown, status = 200) => new Response(JSON.stringify(d), { status, headers: { 'content-type': 'application/json; charset=utf-8', 'cache-control': 'no-store' } });

export class Saguao extends DurableObject<Env> {
  private salas: Map<string, Sala> | null = null;

  private async carregar(): Promise<Map<string, Sala>> {
    if (!this.salas) this.salas = new Map(Object.entries((await this.ctx.storage.get<Record<string, Sala>>('salas')) ?? {}));
    const t = Date.now();
    for (const [id, s] of this.salas) {
      const idade = t - s.atualizada;
      if ((s.estado === 'aguardando' && idade > 30 * 60e3) || (s.estado === 'pareando' && idade > 5 * 60e3) || (s.estado === 'jogando' && idade > 3 * 3600e3) || (s.estado === 'fim' && idade > 60e3)) this.salas.delete(id);
    }
    return this.salas;
  }
  private async gravar(): Promise<void> {
    await this.ctx.storage.put('salas', Object.fromEntries(this.salas!));
  }
  private async criar(dados: Partial<Sala>): Promise<Sala> {
    const salas = await this.carregar();
    const codigos = new Set([...salas.values()].map((s) => s.codigo));
    let codigo = codigoNovo();
    while (codigos.has(codigo)) codigo = codigoNovo();
    const s: Sala = {
      id: 'm' + crypto.randomUUID().replace(/-/g, '').slice(0, 14), codigo, nome: dados.nome ?? 'Sala', privada: !!dados.privada, rapida: !!dados.rapida,
      dono: dados.dono ?? '', donoNome: dados.donoNome ?? '', estado: 'aguardando', jogadores: [], criada: Date.now(), atualizada: Date.now(),
    };
    salas.set(s.id, s);
    await this.gravar();
    return s;
  }

  async fetch(req: Request): Promise<Response> {
    const url = new URL(req.url);
    const salas = await this.carregar();
    if (url.pathname === '/listar') {
      const lista = [...salas.values()].filter((s) => !s.privada && !s.rapida && s.estado !== 'fim').sort((a, b) => b.criada - a.criada)
        .map(({ id, nome, donoNome, estado, jogadores, criada }) => ({ id, nome, donoNome, estado, jogadores, criada }));
      const esperandoRapida = [...salas.values()].filter((s) => s.rapida && s.estado === 'aguardando').length;
      const jogando = [...salas.values()].filter((s) => s.estado === 'jogando').length;
      return resp({ salas: lista, esperandoRapida, jogando });
    }
    if (url.pathname === '/criar') {
      const b = await req.json<Partial<Sala>>();
      const s = await this.criar(b);
      return resp({ id: s.id, codigo: s.codigo });
    }
    if (url.pathname === '/rapida') {
      const b = await req.json<{ usuario: string; nome: string }>();
      const minha = [...salas.values()].find((s) => s.rapida && s.estado === 'aguardando' && s.dono === b.usuario);
      if (minha) { minha.atualizada = Date.now(); await this.gravar(); return resp({ id: minha.id, esperando: true }); }
      const outra = [...salas.values()].filter((s) => s.rapida && s.estado === 'aguardando' && s.dono !== b.usuario).sort((a, c) => a.criada - c.criada)[0];
      if (outra) {
        outra.estado = 'pareando';
        outra.atualizada = Date.now();
        await this.gravar();
        return resp({ id: outra.id, esperando: false });
      }
      const s = await this.criar({ nome: 'Partida rápida', rapida: true, dono: b.usuario, donoNome: b.nome });
      return resp({ id: s.id, esperando: true });
    }
    const m = url.pathname.match(/^\/codigo\/(\w+)$/);
    if (m) {
      const s = [...salas.values()].find((x) => x.codigo === m[1] && x.estado !== 'fim');
      return s ? resp({ id: s.id, nome: s.nome, estado: s.estado }) : resp({ erro: 'Nenhuma sala com este código.' }, 404);
    }
    if (url.pathname === '/atualizar') {
      const b = await req.json<{ id: string; estado: Sala['estado']; jogadores: Sala['jogadores'] }>();
      const s = salas.get(b.id);
      if (s) {
        // Na partida rápida, a sala só volta a "aguardando" se o segundo jogador ainda não chegou.
        if (!(s.estado === 'pareando' && b.estado === 'aguardando' && b.jogadores.length < 2 && Date.now() - s.atualizada < 20e3)) s.estado = b.estado;
        s.jogadores = b.jogadores;
        s.atualizada = Date.now();
        await this.gravar();
      }
      return resp({ ok: true });
    }
    return resp({ erro: 'rota' }, 404);
  }
}
