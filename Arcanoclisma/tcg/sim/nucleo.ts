// Núcleo das simulações: joga uma partida de um confronto sob uma variante e mede o uso das mecânicas.
import { criarJogo, defCarta, defConjurador, ErroRegra, executar } from '../src/motor.ts';
import type { Catalogo, Comando, Estado, Jogador } from '../src/tipos.ts';
import { decidir } from './bot.ts';
import { baralhoDe, type Arquetipo } from './baralhos.ts';
import { catalogoDe, arquetiposDe, estiloDe, regrasDe, type Variante } from './variantes.ts';

export const LIMITE_TURNOS = 150;

export interface Jog {
  grauColocado: number; suspensoesGrau: number; cartasEnviadas: number; sobraDePitch: number; arcanoDissipado: number;
  descartes: number; ataquesAliado: number; ataquesConj: number; proficienteAoEntrar: number;
  bloqueiosAliado: number; bloqueiosConj: number; semBloqueio: number;
  jogadas: Record<string, number>; repentinasNoTurnoOponente: number;
  danoCombateRecebido: number; danoEfeitoRecebido: number; danoGuardado: number; amostrasDanoGuardado: number;
  baralhoVazioNoTurno: number; cartasCompradasPorEfeito: number;
  ativacoesMana: number; ativacoesOutras: number; anulacoesOferecidas: number; anulacoesPagas: number; erros: number;
  danoDoConjurador: number;
  /** Cartas na mão ao fim do 4º e do 6º turno do jogador (antes de comprar); -1 = não chegou. */
  maoT4: number; maoT6: number;
  /** Recursos (cartas na mão + permanentes em campo + cartas no Grau) ao fim do turno global 16; -1 = não chegou. */
  recursosT16: number;
}
export const novoJog = (): Jog => ({
  grauColocado: 0, suspensoesGrau: 0, cartasEnviadas: 0, sobraDePitch: 0, arcanoDissipado: 0, descartes: 0, ataquesAliado: 0, ataquesConj: 0, proficienteAoEntrar: 0,
  bloqueiosAliado: 0, bloqueiosConj: 0, semBloqueio: 0, jogadas: {}, repentinasNoTurnoOponente: 0, danoCombateRecebido: 0, danoEfeitoRecebido: 0,
  danoGuardado: 0, amostrasDanoGuardado: 0, baralhoVazioNoTurno: 0, cartasCompradasPorEfeito: 0,
  ativacoesMana: 0, ativacoesOutras: 0, anulacoesOferecidas: 0, anulacoesPagas: 0, erros: 0, danoDoConjurador: 0, maoT4: -1, maoT6: -1, recursosT16: -1,
});

export interface Extra {
  aliadosPerdidos: number;
  turnosProprios: number;
  /** Turnos próprios em que causou dano a algo do oponente. */
  turnosProprioComDano: number;
  /** Turno global do primeiro dano causado (null = nunca). */
  primeiroDano: number | null;
  /** Vezes que a habilidade do Conjurador agiu (ativações e gatilhos). */
  habilidade: number;
}

export interface Resultado {
  a: number; b: number; turnos: number; vencedor: Jogador | null; primeiro: Jogador; m: [Jog, Jog]; fim: 'conjurador' | 'semAcao' | 'impasse' | 'travou';
  extras: [Extra, Extra];
}

function medir(e: Estado, cmd: Comando, m: [Jog, Jog]): void {
  const antes = {
    etapa: e.etapa, ativo: e.ativo, pool: [e.jogadores[0].arcano, e.jogadores[1].arcano],
    dano: [e.instancias[e.jogadores[0].conjuradorId].dano, e.instancias[e.jogadores[1].conjuradorId].dano], combate: e.combate?.etapa,
    mao: [e.jogadores[0].mao.length, e.jogadores[1].mao.length],
  };
  const inst = cmd.tipo === 'atacar' ? e.instancias[cmd.atacante] : null;
  const entrouAgora = inst ? inst.entrouNoTurno === e.turno : false;
  const ehConjAtk = e.combate ? e.instancias[e.combate.atacante].ehConjurador : (inst?.ehConjurador ?? false);
  const pendAntes = e.pendencia?.tipo;
  executar(e, cmd);
  if (antes.etapa === 'cataclisma' && e.etapa === 'final') {
    m[antes.ativo].arcanoDissipado += antes.pool[antes.ativo];
    const proprio = Math.ceil(e.turno / 2);
    if (proprio === 4) m[antes.ativo].maoT4 = antes.mao[antes.ativo];
    if (proprio === 6) m[antes.ativo].maoT6 = antes.mao[antes.ativo];
    if (e.turno === 16) for (const j of [0, 1] as Jogador[]) m[j].recursosT16 = e.jogadores[j].mao.length + e.jogadores[j].campo.length + e.jogadores[j].grau.length;
  }
  for (const j of [0, 1] as Jogador[]) {
    const d = e.instancias[e.jogadores[j].conjuradorId].dano - antes.dano[j];
    if (d > 0) {
      if (antes.combate === 'bloqueado' && e.combate === null) {
        m[j].danoCombateRecebido += d;
        if (ehConjAtk) m[j === 0 ? 1 : 0].danoDoConjurador += d;
      } else m[j].danoEfeitoRecebido += d;
    }
  }
  if (antes.etapa === 'final' && e.etapa === 'inicio') {
    const a = e.ativo;
    m[a].danoGuardado += e.jogadores[a].campo.reduce((s, id) => s + e.instancias[id].dano, 0);
    m[a].amostrasDanoGuardado += 1;
  }
  for (const j of [0, 1] as Jogador[]) if (e.jogadores[j].baralho.length === 0 && m[j].baralhoVazioNoTurno === 0) m[j].baralhoVazioNoTurno = e.turno;
  if (e.pendencia?.tipo === 'pagarAnulacao' && pendAntes !== 'pagarAnulacao') m[e.pendencia.jogador].anulacoesOferecidas++;
  const j = 'jogador' in cmd ? cmd.jogador : 0;
  if (cmd.tipo === 'colocarGrau') m[j].grauColocado++;
  else if (cmd.tipo === 'suspenderGrau') m[j].suspensoesGrau++;
  else if (cmd.tipo === 'descartar' && e.etapa === 'inicio') m[j].descartes += cmd.cartas.length;
  else if (cmd.tipo === 'ativar') {
    const at = e.instancias[cmd.carta].ehConjurador ? defConjurador(e, j).poder?.ativacao : defCarta(e, cmd.carta).ativacao;
    if (at?.efeitos.every((ef) => ef.tipo === 'arcano')) m[j].ativacoesMana++;
    else m[j].ativacoesOutras++;
  } else if (cmd.tipo === 'pagarAnulacao') {
    if (cmd.pagar) m[j].anulacoesPagas++;
  } else if (cmd.tipo === 'jogar') {
    const d = defCarta(e, cmd.carta);
    const defId = e.instancias[cmd.carta].defId;
    m[j].jogadas[defId] = (m[j].jogadas[defId] ?? 0) + 1;
    m[j].cartasEnviadas += cmd.enviarArcano?.length ?? 0;
    if (cmd.enviarArcano?.length) m[j].sobraDePitch += e.jogadores[j].arcano;
    if (d.velocidade === 'repentina' && antes.ativo !== j) m[j].repentinasNoTurnoOponente++;
    if (d.efeitos?.some((x) => x.tipo === 'comprar')) m[j].cartasCompradasPorEfeito += 1;
  } else if (cmd.tipo === 'atacar') {
    if (e.instancias[cmd.atacante].ehConjurador) m[j].ataquesConj++;
    else {
      m[j].ataquesAliado++;
      if (entrouAgora) m[j].proficienteAoEntrar++;
    }
  } else if (cmd.tipo === 'bloquear') {
    if (cmd.bloqueador === null) m[j].semBloqueio++;
    else if (e.instancias[cmd.bloqueador].ehConjurador) m[j].bloqueiosConj++;
    else m[j].bloqueiosAliado++;
  }
}

export interface Contexto {
  v: Variante;
  catalogo: Catalogo;
  arqs: Arquetipo[];
}

export function contextoDe(v: Variante): Contexto {
  const catalogo = catalogoDe(v);
  return { v, catalogo, arqs: arquetiposDe(v, catalogo) };
}

export function partida(ctx: Contexto, a: number, b: number, semente: number): Resultado {
  const { v, catalogo, arqs } = ctx;
  const e = criarJogo({ catalogo, baralhos: [baralhoDe(arqs[a], arqs[a].conjurador ?? 'padrao'), baralhoDe(arqs[b], arqs[b].conjurador ?? 'padrao')], semente, regras: regrasDe(v) });
  const m: [Jog, Jog] = [novoJog(), novoJog()];
  const estilos = [estiloDe(arqs[a], v), estiloDe(arqs[b], v)];
  let passos = 0;
  let primeiro: Jogador = 0;
  while (e.vencedor === null && e.turno <= LIMITE_TURNOS && passos < 40000) {
    passos++;
    const ator: Jogador = e.pendencia ? e.pendencia.jogador : e.prioridade;
    const cmd = decidir(e, ator, estilos[ator]);
    if (cmd.tipo === 'escolherInicio') primeiro = cmd.primeiro;
    try {
      medir(e, cmd, m);
    } catch (erro) {
      if (!(erro instanceof ErroRegra)) throw erro;
      m[ator].erros++;
      try { medir(e, { tipo: 'passar', jogador: e.pendencia ? e.pendencia.jogador : e.prioridade }, m); } catch (e2) { if (!(e2 instanceof ErroRegra)) throw e2; }
    }
  }
  const venc = e.vencedor === 0 || e.vencedor === 1 ? e.vencedor : null;
  const semAcao = e.log.some((l) => l.includes('não tem mais ações'));
  const impasse = e.log.some((l) => l.includes('Impasse'));
  const ativoEm = (t: number): Jogador => (t % 2 === 1 ? primeiro : primeiro === 0 ? 1 : 0);
  const extras = ([0, 1] as Jogador[]).map((j): Extra => {
    const z = e.jogadores[j];
    let proprios = 0;
    for (let t = 1; t <= e.turno; t++) if (ativoEm(t) === j) proprios++;
    return {
      aliadosPerdidos: z.cemiterio.filter((id) => e.catalogo.cartas[e.instancias[id].defId]?.tipo === 'aliado').length,
      turnosProprios: proprios,
      turnosProprioComDano: e.turnosDano[j].filter((t) => ativoEm(t) === j).length,
      primeiroDano: e.turnosDano[j][0] ?? null,
      habilidade: e.log.filter((l) => l.startsWith(`Jogador ${j}: gatilho da habilidade`) || l.startsWith(`Jogador ${j} ativa ${defConjurador(e, j).nome}`)).length,
    };
  }) as [Extra, Extra];
  return { a, b, turnos: e.turno, vencedor: venc, primeiro, m, extras, fim: venc === null ? 'travou' : impasse ? 'impasse' : semAcao ? 'semAcao' : 'conjurador' };
}
