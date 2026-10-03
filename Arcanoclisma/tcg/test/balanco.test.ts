import { test } from 'node:test';
import assert from 'node:assert/strict';
import { avaliar } from '../src/balanco.ts';
import { cartasBase, catalogoBase } from '../src/cartas-base.ts';
import { catalogoTeste } from '../src/cartas-teste.ts';
import { criarJogo, executar, ErroRegra } from '../src/motor.ts';
import type { Catalogo, Estado, Jogador } from '../src/tipos.ts';

test('cartas básicas: ids únicos e dentro do orçamento (no máximo 2 PP abaixo, nunca acima)', () => {
  const ids = new Set(cartasBase.map((c) => c.id));
  assert.equal(ids.size, cartasBase.length);
  for (const c of cartasBase) {
    const a = avaliar(c);
    assert.ok(a.folga >= -1e-9, `${c.id} passa do orçamento: ${a.pontos} > ${a.orcamento}`);
    assert.ok(a.folga <= 2, `${c.id} está fraca demais: folga ${a.folga}`);
  }
});

test('cartas básicas: números do jogo ficam entre 0 e 20', () => {
  for (const c of cartasBase) {
    for (const n of [c.custo, c.grau, c.ataque ?? 0, c.vida ?? 0]) assert.ok(n >= 0 && n <= 20, `${c.id}: ${n}`);
  }
});

// ---------- fuzz: jogadores aleatórios não podem quebrar o motor ----------

function prng(semente: number): () => number {
  let s = semente >>> 0;
  return () => {
    s = (s + 0x6d2b79f5) >>> 0;
    let t = s;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

const catalogo: Catalogo = { cartas: catalogoBase.cartas, conjuradores: catalogoTeste.conjuradores };

function escolher<T>(r: () => number, v: T[]): T | undefined {
  return v.length ? v[Math.floor(r() * v.length)] : undefined;
}

function alvosPossiveis(e: Estado): string[] {
  return [0, 1].flatMap((j) => [e.jogadores[j].conjuradorId, ...e.jogadores[j].campo]);
}

function passoAleatorio(e: Estado, r: () => number): void {
  if (e.pendencia) {
    const p = e.pendencia;
    if (p.tipo === 'escolherInicio') executar(e, { tipo: 'escolherInicio', jogador: p.jogador, primeiro: r() < 0.5 ? 0 : 1 });
    else if (p.tipo === 'descartar') executar(e, { tipo: 'descartar', jogador: p.jogador, cartas: e.jogadores[p.jogador].mao.slice(0, p.quantidade) });
    else {
      const z = e.jogadores[p.jogador];
      const opcoes = [z.conjuradorId, ...z.campo].filter((id) => !e.instancias[id].suspensa);
      executar(e, { tipo: 'bloquear', jogador: p.jogador, bloqueador: r() < 0.5 ? null : (escolher(r, opcoes) ?? null) });
    }
    return;
  }
  const j: Jogador = e.prioridade;
  const z = e.jogadores[j];
  const sorte = r();
  if (sorte < 0.35 && z.mao.length) {
    const carta = escolher(r, z.mao)!;
    const d = catalogo.cartas[e.instancias[carta].defId];
    // paga com Grau suspenso e, se faltar, envia cartas da mão como pitch
    for (const g of z.grau) if (!e.instancias[g].suspensa && z.arcano < d.custo) executar(e, { tipo: 'suspenderGrau', jogador: j, carta: g });
    const pitch: string[] = [];
    let reserva = z.arcano;
    for (const outra of z.mao) {
      if (reserva >= d.custo) break;
      if (outra === carta) continue;
      pitch.push(outra);
      reserva += catalogo.cartas[e.instancias[outra].defId].arcanoGerado;
    }
    const descartarGrau = z.grau.slice(0, d.descartaGrau ?? 0);
    executar(e, { tipo: 'jogar', jogador: j, carta, alvo: escolher(r, alvosPossiveis(e)), descartarGrau, enviarArcano: pitch });
  } else if (sorte < 0.45 && z.mao.length) {
    executar(e, { tipo: 'colocarGrau', jogador: j, carta: escolher(r, z.mao)! });
  } else if (sorte < 0.6) {
    const atacante = escolher(r, [z.conjuradorId, ...z.campo]);
    const alvos = e.jogadores[j === 0 ? 1 : 0];
    const alvo = escolher(r, [alvos.conjuradorId, ...alvos.campo]);
    if (atacante && alvo) {
      const custo = atacante === z.conjuradorId ? e.catalogo.conjuradores[e.instancias[atacante].defId].custoAtaque : 0;
      const pitch: string[] = [];
      let reserva = z.arcano;
      for (const outra of z.mao) {
        if (reserva >= custo) break;
        pitch.push(outra);
        reserva += catalogo.cartas[e.instancias[outra].defId].arcanoGerado;
      }
      executar(e, { tipo: 'atacar', jogador: j, atacante, alvo, enviarArcano: atacante === z.conjuradorId ? pitch : [] });
    }
  } else executar(e, { tipo: 'passar', jogador: j });
}

test('fuzz: 40 partidas aleatórias não quebram o motor', () => {
  let terminadas = 0;
  const turnos: number[] = [];
  for (let semente = 1; semente <= 40; semente++) {
    const r = prng(semente * 977);
    const ids = Object.keys(catalogo.cartas);
    const deck = (c: string) => ({ conjurador: c, cartas: Array.from({ length: 60 }, (_, i) => ids[(i * 7 + semente) % ids.length]) });
    const e = criarJogo({ catalogo, baralhos: [deck('maga-da-cinza'), deck('guardiao-do-vale')], semente });
    for (let passo = 0; passo < 6000 && e.vencedor === null; passo++) {
      try {
        passoAleatorio(e, r);
      } catch (erro) {
        if (!(erro instanceof ErroRegra)) throw erro;
        if (e.pendencia?.tipo !== 'descartar' && r() < 0.5 && !e.pendencia) {
          try { executar(e, { tipo: 'passar', jogador: e.prioridade }); } catch (e2) { if (!(e2 instanceof ErroRegra)) throw e2; }
        }
      }
    }
    if (e.vencedor !== null) {
      terminadas++;
      turnos.push(e.turno);
    }
  }
  console.log(`  fuzz: ${terminadas}/40 terminaram; turnos (min/mediana/max): ${turnos.length ? [Math.min(...turnos), turnos.sort((a, b) => a - b)[Math.floor(turnos.length / 2)], Math.max(...turnos)].join('/') : '-'}`);
  assert.ok(terminadas > 0, 'nenhuma partida terminou');
});

// ---------- variantes de cartas respeitam o orçamento ----------
import { avaliar as avaliarCarta } from '../src/balanco.ts';
import { catalogoDe, precosDe, variante } from '../sim/variantes.ts';

test('variantes de cartas (recursos, Eternyr, Estruturas, Cura, Burn) respeitam o orçamento de PP', () => {
  const v = variante('todas', 'todas', { burn: 'pagarX', recursos: true, eternyr: true, estruturas: true, cura: true });
  const cat = catalogoDe(v);
  const p = precosDe(v);
  for (const c of Object.values(cat.cartas)) {
    const a = avaliarCarta(c, p);
    assert.ok(a.folga >= -1.01, `${c.id} passa do orçamento: ${a.pontos} > ${a.orcamento}`);
  }
  const burn = catalogoDe(variante('b', 'b', { burn: 'custoGrau' }));
  assert.ok(burn.cartas['base-dano-c1'].custo === 3 && burn.cartas['base-dano-c1'].grau === 2);
});
