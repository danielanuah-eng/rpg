// Jogador guloso para simulações. Não é forte: serve para expor quais mecânicas são usadas, ignoradas ou abusadas.
import { ataqueConjurador, custoEfetivo, custoProtecao, origemDaCarta, defCarta, defConjurador, GRAU_MAXIMO, grauEfetivo, recemChegado } from '../src/motor.ts';
import type { CartaDef, Comando, Estado, Jogador } from '../src/tipos.ts';

export interface Estilo {
  nome: string;
  /** 0 a 1: quanto do campo manda atacar. */
  agressividade: number;
  /** Segura Grau levantado para jogar cartas repentinas no turno do oponente. */
  reservaInstantes: boolean;
  /** Quantas cartas quer ter no Grau antes de guardar cartas na mão. */
  grauAlvo: number;
  /** Joga dano direto no Conjurador mesmo sem estar perto de matar. */
  queimaFace: boolean;
  /** Ataca com o Conjurador sempre que ele puder e o oponente não tiver Aliado levantado. */
  conjAtaca?: boolean;
  /** Remoção também pode mirar Estruturas. */
  alvejaEstruturas?: boolean;
  /** Jogador de deck lento: só se preocupa em sobreviver e em ganhar em Graus altos. */
  lento?: boolean;
  /** Antes de atacar, prevê como o oponente bloquearia (não ataca só por existir um bloqueador maior). */
  preveBloqueio?: boolean;
  /** Sobe o Grau até poder jogar as cartas da mão (não deixa carta morta por Grau alvo baixo). */
  sobeGrau?: boolean;
  /** Também ataca Aliados e Místicas inimigas quando a troca compensa (não só o Conjurador). */
  atacaAliados?: boolean;
  /** Rodada 6: dá às varreduras o valor de remoção (não as põe no Grau nem as usa de pitch primeiro). */
  valorizaVarredura?: boolean;
}

const outro = (j: Jogador): Jogador => (j === 0 ? 1 : 0);

function vidaMax(e: Estado, id: string): number {
  const i = e.instancias[id];
  return i.ehConjurador ? defConjurador(e, i.dono).vida : (defCarta(e, id).vida ?? Infinity);
}
const vidaRest = (e: Estado, id: string): number => vidaMax(e, id) - e.instancias[id].dano;
function atk(e: Estado, id: string): number {
  const i = e.instancias[id];
  return i.ehConjurador ? ataqueConjurador(e, i.dono) : (defCarta(e, id).ataque ?? 0);
}
const ehAliado = (e: Estado, id: string): boolean => !e.instancias[id].ehConjurador && defCarta(e, id).tipo === 'aliado';
const etereo = (e: Estado, id: string): boolean => !e.instancias[id].ehConjurador && !!defCarta(e, id).palavrasChave?.includes('Etéreo');
function valorPerm(e: Estado, id: string): number {
  const i = e.instancias[id];
  const bonus = !i.ehConjurador && defCarta(e, id).tipo === 'estrutura' ? 5 : 0;
  return atk(e, id) + Math.min(vidaRest(e, id), 20) * 0.7 + bonus;
}

/** Rodada 6: valor extra de um Aliado com gatilhos (ao entrar, ao causar dano ao Conjurador) e Protegido. 0 nas cartas antigas. */
function extraAliado(d: CartaDef): number {
  return 2 * (d.aoEntrar?.length ?? 0) + 2 * (d.aoDanoConjurador?.length ?? 0) + (d.protegido ?? 0);
}

/** Estilo do jogador que está decidindo agora (definido em decidir). */
let estAtual: Estilo | undefined;

function valorCarta(d: CartaDef, grauAtual: number): number {
  let v = 3;
  // rodada 6: varredura vale como remoção (não vai para o Grau nem para o pitch antes das outras cartas)
  if (estAtual?.valorizaVarredura && d.tipo === 'temporaria' && d.efeitos?.some((ef) => ef.tipo === 'danoTodosAliados' || ef.tipo === 'destruirTodosAliados')) {
    return 9 - (d.grau > grauAtual + 3 ? 2 : 0);
  }
  if (d.tipo === 'aliado') v = (d.ataque ?? 0) + (d.vida ?? 0) + 2 + (d.palavrasChave?.length ? 1 : 0) + extraAliado(d);
  else if (d.tipo === 'temporaria') {
    v = 3;
    for (const ef of d.efeitos ?? []) {
      if (ef.tipo === 'dano') v = 5 + ef.valor;
      else if (ef.tipo === 'comprar') v = 4 + 3 * ef.valor;
      else if (ef.tipo === 'olharTopo') v = 5 + 3 * ef.pegar;
      else if (ef.tipo === 'destruir') v = 9;
      else if (ef.tipo === 'destruirTodosAliados') v = 10;
      else if (ef.tipo === 'curar') v = 2;
    }
  } else if (d.tipo === 'mistica') v = 3 + (d.vida ?? 0) / 3;
  else if (d.tipo === 'estrutura') v = d.custo === 0 ? -2 : 2 + (d.ativacao ? 4 : 0) + (d.aoEntrar ?? []).reduce((s, ef) => s + (ef.tipo === 'comprar' ? 3 : 2), 0);
  if (d.grau > grauAtual + 3) v -= 2;
  return v;
}

/** Menor valor = melhor carta para descartar, pôr no Grau ou enviar como pitch. */
function ordemDeSacrificio(e: Estado, j: Jogador, ids: string[]): string[] {
  const g = e.jogadores[j].grau.length;
  const nota = (id: string) => valorCarta(defCarta(e, id), g) - 2 * defCarta(e, id).arcanoGerado;
  return [...ids].sort((a, b) => nota(a) - nota(b));
}

function alvosInimigos(e: Estado, j: Jogador, est?: Estilo): string[] {
  const adv = e.jogadores[outro(j)];
  return adv.campo.filter((id) => {
    const t = defCarta(e, id).tipo;
    return t === 'aliado' || t === 'mistica' || (!!est?.alvejaEstruturas && t === 'estrutura');
  });
}

interface Intencao {
  carta: string;
  alvo?: string;
  utilidade: number;
}

/** Deck lento: só joga carta defensiva, utilitária ou de Grau alto. */
function permitidoAoLento(d: CartaDef): boolean {
  if (d.grau >= 6) return true;
  if (d.tipo === 'mistica' || d.tipo === 'estrutura') return true;
  if (d.tipo === 'aliado') return (d.ataque ?? 0) <= 2 || (d.vida ?? 0) >= 1.3 * (d.ataque ?? 0);
  return !!d.efeitos?.some((ef) => ef.tipo === 'comprar' || ef.tipo === 'curar' || ef.tipo === 'destruir' || (ef.tipo === 'dano' && ef.alvo !== 'conjuradorInimigo'));
}

function intencao(e: Estado, j: Jogador, id: string, est: Estilo): Intencao | null {
  const r = intencaoBase(e, j, id, est);
  if (!r || !est.lento) return r;
  const d = defCarta(e, id);
  const letal = r.utilidade >= 30;
  return permitidoAoLento(d) || letal ? r : null;
}

function intencaoBase(e: Estado, j: Jogador, id: string, est: Estilo): Intencao | null {
  const d = defCarta(e, id);
  const z = e.jogadores[j];
  const adv = e.jogadores[outro(j)];
  const face = adv.conjuradorId;
  if (d.tipo === 'aliado') {
    let u = ((d.ataque ?? 0) + (d.vida ?? 0)) / 2 + 2 + (d.palavrasChave?.length ? 1 : 0) + extraAliado(d);
    // ao entrar com dano em todos os Aliados do oponente: soma o que morre
    for (const ef of d.aoEntrar ?? []) if (ef.tipo === 'danoTodosAliados') u += adv.campo.filter((a) => ehAliado(e, a) && vidaRest(e, a) <= ef.valor).reduce((t, a) => t + valorPerm(e, a), 0) / 2;
    return { carta: id, utilidade: u };
  }
  if (d.tipo === 'mistica') return { carta: id, utilidade: 3 + (d.vida ?? 0) / 3 };
  if (d.tipo === 'estrutura') {
    if (d.custo === 0) return null;
    return { carta: id, utilidade: 2 + (d.ativacao ? 4 : 0) + (d.aoEntrar ?? []).reduce((s, ef) => s + (ef.tipo === 'comprar' ? 3 : 2), 0) + (d.vida ?? 0) / 4 };
  }
  const ef0 = d.efeitos?.[0];
  if (!ef0) return null;
  // dano que cresce com o Grau: avalia pelo valor de agora
  const ef = ef0.tipo === 'dano' && ef0.escalaGrau ? { ...ef0, valor: ef0.valor + z.grau.length } : ef0;
  if (ef.tipo === 'anular') {
    const topo = e.pilha[e.pilha.length - 1];
    if (!topo || topo.tipo !== 'carta' || topo.controlador === j) return null;
    const dv = defCarta(e, topo.instId);
    const v = valorCarta(dv, z.grau.length);
    const queima = !!dv.efeitos?.some((x) => x.tipo === 'dano' && x.alvo === 'conjuradorInimigo');
    return v >= 7 || queima ? { carta: id, alvo: topo.instId, utilidade: 5 + v / 2 } : null;
  }
  if (ef.tipo === 'devolver') {
    const melhor = alvosInimigos(e, j, est).filter((a) => defCarta(e, a).tipo !== 'estrutura' || ef.alvo === 'escolhido').sort((a, b) => valorPerm(e, b) - valorPerm(e, a))[0];
    return melhor && valorPerm(e, melhor) >= 4 ? { carta: id, alvo: melhor, utilidade: 3 + valorPerm(e, melhor) / 3 } : null;
  }
  if (ef.tipo === 'danoTodosAliados') {
    const advs = e.jogadores[outro(j)].campo.filter((a) => ehAliado(e, a));
    const mortos = advs.filter((a) => vidaRest(e, a) <= ef.valor);
    let valor = mortos.reduce((t, a) => t + valorPerm(e, a), 0);
    // varredura simétrica: desconta os seus Aliados que morrem junto
    if (ef.ambos) valor -= z.campo.filter((a) => ehAliado(e, a) && vidaRest(e, a) <= ef.valor).reduce((t, a) => t + valorPerm(e, a), 0);
    return (mortos.length >= 2 && valor > 0) || valor >= 8 ? { carta: id, utilidade: 4 + valor / 2 } : null;
  }
  if (ef.tipo === 'destruirTodosAliados') {
    const deles = adv.campo.filter((a) => ehAliado(e, a));
    const valor = deles.reduce((t, a) => t + valorPerm(e, a), 0);
    const perda = ef.ambos ? z.campo.filter((a) => ehAliado(e, a)).reduce((t, a) => t + valorPerm(e, a), 0) : 0;
    const liquido = valor - perda;
    return (deles.length >= 3 && liquido >= 8) || liquido >= 14 ? { carta: id, utilidade: 4 + liquido / 2 } : null;
  }
  if (ef.tipo === 'comprar') return { carta: id, utilidade: z.mao.length <= 4 ? 3 * ef.valor : 1 };
  if (ef.tipo === 'olharTopo') return { carta: id, utilidade: z.mao.length <= 4 ? 3.5 * ef.pegar : 1 };
  if (ef.tipo === 'dano' && ef.alvo === 'conjuradorInimigo') {
    const letal = vidaRest(e, face) <= ef.valor;
    return { carta: id, utilidade: letal ? 30 : est.queimaFace ? ef.valor + 1 : vidaRest(e, face) <= 8 ? 3 : 0 };
  }
  if (ef.tipo === 'dano') {
    const soAliados = ef.alvo === 'escolhidoAliado';
    if (!soAliados && vidaRest(e, face) <= ef.valor) return { carta: id, alvo: face, utilidade: 30 };
    const matavel = alvosInimigos(e, j, est).filter((a) => vidaRest(e, a) <= ef.valor).sort((a, b) => valorPerm(e, b) - valorPerm(e, a))[0];
    if (matavel) return { carta: id, alvo: matavel, utilidade: 4 + valorPerm(e, matavel) / 2 };
    return est.queimaFace && !soAliados ? { carta: id, alvo: face, utilidade: ef.valor } : null;
  }
  if (ef.tipo === 'curar') {
    const meus = [...(ef.alvo === 'escolhidoAliado' ? [] : [z.conjuradorId]), ...z.campo.filter((a) => ef.alvo !== 'escolhidoAliado' || ehAliado(e, a))].filter((a) => e.instancias[a].dano >= 2).sort((a, b) => e.instancias[b].dano - e.instancias[a].dano);
    return meus[0] ? { carta: id, alvo: meus[0], utilidade: 2 + Math.min(e.instancias[meus[0]].dano, ef.valor) / 2 } : null;
  }
  if (ef.tipo === 'destruir') {
    const melhor = alvosInimigos(e, j, est).sort((a, b) => valorPerm(e, b) - valorPerm(e, a))[0];
    return melhor && valorPerm(e, melhor) >= 6 ? { carta: id, alvo: melhor, utilidade: 6 + valorPerm(e, melhor) / 2 } : null;
  }
  return null;
}

/** Permanentes que só geram Arcano (sem custo): fontes extras de mana. */
function manaPerm(e: Estado, j: Jogador): string[] {
  return e.jogadores[j].campo.filter((id) => {
    const at = defCarta(e, id).ativacao;
    return !!at && at.suspende && at.custo === 0 && at.efeitos.every((ef) => ef.tipo === 'arcano') && !e.instancias[id].suspensa && !recemChegado(e, id);
  });
}
const valorMana = (e: Estado, id: string): number => (defCarta(e, id).ativacao?.efeitos ?? []).reduce((s, ef) => s + (ef.tipo === 'arcano' ? ef.valor : 0), 0);

function grauUsavel(e: Estado, j: Jogador): string[] {
  return e.jogadores[j].grau.filter((id) => !e.instancias[id].suspensa);
}

/** Cartas que o jogador pode jogar agora: mão, banimento liberado e cemitério (quando a habilidade libera um Anular). */
function candidatas(e: Estado, j: Jogador): string[] {
  const z = e.jogadores[j];
  return [...z.mao, ...z.banimento, ...z.cemiterio].filter((id) => origemDaCarta(e, j, id) !== null);
}

/** Planeja jogar uma carta: devolve o próximo comando (suspender Grau ou jogar). */
function planejarJogada(e: Estado, j: Jogador, est: Estilo, apenasRepentinas: boolean, filtro?: (i: Intencao) => boolean, reserva = 0, alvoCombate?: string): Comando | null {
  const z = e.jogadores[j];
  const usavel = grauUsavel(e, j);
  const mana = manaPerm(e, j);
  const disponivel = z.arcano + mana.reduce((t, id) => t + valorMana(e, id), 0) + Math.max(0, usavel.length - reserva);
  let melhor: { i: Intencao; custo: number; pitch: string[]; nota: number } | null = null;
  for (const id of candidatas(e, j)) {
    const d = defCarta(e, id);
    const rep = d.velocidade === 'repentina' && d.tipo !== 'aliado'; // Aliado nunca é Repentina pelo tipo (decisão 21)
    if (apenasRepentinas && !rep) continue;
    if (!apenasRepentinas && rep && est.reservaInstantes) continue;
    if (d.grau > grauEfetivo(e, j) || (d.descartaGrau ?? 0) > z.grau.length) continue;
    // custo de Valor: precisa de uma carta daquele Valor no Grau (senão o motor recusa a jogada)
    if ((d.custoValores ?? []).some((v) => !z.grau.some((g) => defCarta(e, g).valores.includes(v)))) continue;
    // janela de combate: só serve dano que tira o inimigo do combate antes do dano (ele não chega a bater)
    const efc = d.efeitos?.[0];
    const it = alvoCombate
      ? (efc?.tipo === 'dano' && efc.alvo !== 'conjuradorInimigo' && vidaRest(e, alvoCombate) <= efc.valor ? { carta: id, alvo: alvoCombate, utilidade: 4 + valorPerm(e, alvoCombate) / 2 } : null)
      : intencao(e, j, id, est);
    if (!it || it.utilidade <= 0 || (filtro && !filtro(it))) continue;
    const custo = custoEfetivo(e, j, id) + custoProtecao(e, j, it.alvo);
    let pitch: string[] = [];
    const falta = custo - disponivel;
    if (falta > 0) {
      const maxPitch = z.mao.length > 4 ? 2 : 1;
      let somado = 0;
      for (const o of ordemDeSacrificio(e, j, z.mao.filter((x) => x !== id))) {
        if (somado >= falta || pitch.length >= maxPitch) break;
        pitch.push(o);
        somado += defCarta(e, o).arcanoGerado;
      }
      if (somado < falta) continue;
      const perda = pitch.reduce((s, o) => s + valorCarta(defCarta(e, o), z.grau.length), 0) / 2;
      if (it.utilidade <= perda) continue;
    }
    const nota = it.utilidade - pitch.length * 2 + d.custo * 0.01;
    if (!melhor || nota > melhor.nota) melhor = { i: it, custo, pitch, nota };
  }
  if (!melhor) return null;
  // paga com permanentes de Arcano e com Grau antes de gastar cartas
  if (z.arcano < melhor.custo && mana[0]) return { tipo: 'ativar', jogador: j, carta: mana[0] };
  if (z.arcano < melhor.custo && usavel.length > reserva) return { tipo: 'suspenderGrau', jogador: j, carta: usavel[0] };
  // recalcula o pitch mínimo com a reserva atual
  const pitch: string[] = [];
  let reservaArcano = z.arcano;
  for (const o of melhor.pitch) {
    if (reservaArcano >= melhor.custo) break;
    pitch.push(o);
    reservaArcano += defCarta(e, o).arcanoGerado;
  }
  if (reservaArcano < melhor.custo) return null;
  const d = defCarta(e, melhor.i.carta);
  return {
    tipo: 'jogar', jogador: j, carta: melhor.i.carta, alvo: melhor.i.alvo,
    descartarGrau: ordemDeSacrificio(e, j, z.grau).slice(0, d.descartaGrau ?? 0), enviarArcano: pitch,
  };
}

function planejarAtivacao(e: Estado, j: Jogador, est: Estilo): Comando | null {
  const z = e.jogadores[j];
  const adv = e.jogadores[outro(j)];
  const doConj = defConjurador(e, j).poder?.ativacao;
  const fontes: Array<[string, NonNullable<CartaDef['ativacao']>]> = [];
  for (const id of z.campo) { const a = defCarta(e, id).ativacao; if (a) fontes.push([id, a]); }
  if (doConj) fontes.push([z.conjuradorId, doConj]);
  for (const [id, at] of fontes) {
    if ((at.suspende && (e.instancias[id].suspensa || recemChegado(e, id))) || at.efeitos.every((ef) => ef.tipo === 'arcano')) continue;
    if (at.requisito?.grau && grauEfetivo(e, j) < at.requisito.grau) continue; // requisito do Editor (04/10)
    const ef = at.efeitos[0];
    let alvo: string | undefined;
    let necessario = at.custo + (at.requisito?.arcano ?? 0);
    if (ef.tipo === 'comprar' || ef.tipo === 'olharTopo') { if (z.mao.length > 5) continue; }
    else if (ef.tipo === 'devolver') {
      // piscar um Aliado seu: reaproveita "ao entrar" ou salva um Aliado valioso machucado
      alvo = z.campo.filter((a) => ehAliado(e, a) && (!!defCarta(e, a).aoEntrar?.length || (valorPerm(e, a) >= 5 && e.instancias[a].dano >= vidaMax(e, a) - 2 && e.instancias[a].dano > 0)))
        .sort((a, b) => valorPerm(e, b) - valorPerm(e, a))[0];
      if (!alvo) continue;
    } else if (ef.tipo === 'dano' && ef.alvo === 'escolhidoAliado') {
      // cutucar: termina um Aliado ferido ou machuca o mais frágil (o dano fica guardado)
      const alvos = alvosInimigos(e, j, est);
      alvo = alvos.filter((a) => vidaRest(e, a) <= ef.valor).sort((a, b) => valorPerm(e, b) - valorPerm(e, a))[0]
        ?? alvos.filter((a) => e.instancias[a].dano > 0).sort((a, b) => vidaRest(e, a) - vidaRest(e, b))[0]
        ?? alvos.sort((a, b) => vidaRest(e, a) - vidaRest(e, b))[0];
      if (!alvo) continue;
    }
    else if (ef.tipo === 'descontoProximaCarta' || ef.tipo === 'permitirAnularDoCemiterio') continue; // tratados em planejarDesconto / planejarLiberarAnular
    else if (ef.tipo === 'banirDaMao') {
      // guarda a carta mais cara da mão para jogá-la mais barata no próximo turno
      alvo = z.mao.filter((a) => defCarta(e, a).custo >= 5 && defCarta(e, a).grau <= grauEfetivo(e, j) + 2).sort((a, b) => defCarta(e, b).custo - defCarta(e, a).custo)[0];
      if (!alvo) continue;
    } else if (ef.tipo === 'reanimar') {
      const todos = [...e.jogadores[0].cemiterio, ...e.jogadores[1].cemiterio].filter((a) => defCarta(e, a).tipo === 'aliado' && defCarta(e, a).custo + ef.custoExtra <= z.arcano + grauUsavel(e, j).length + manaPerm(e, j).length);
      alvo = todos.sort((a, b) => valorCarta(defCarta(e, b), 0) - valorCarta(defCarta(e, a), 0))[0];
      if (!alvo || valorCarta(defCarta(e, alvo), 0) < 8) continue;
      necessario += defCarta(e, alvo).custo + ef.custoExtra;
    }
    else if (ef.tipo === 'curar') {
      alvo = [...(ef.alvo === 'escolhidoAliado' ? [] : [z.conjuradorId]), ...z.campo.filter((a) => ef.alvo !== 'escolhidoAliado' || ehAliado(e, a))].filter((a) => e.instancias[a].dano >= 2).sort((a, b) => e.instancias[b].dano - e.instancias[a].dano)[0];
      if (!alvo) continue;
    } else if (ef.tipo === 'dano') {
      const valor = ef.valor;
      alvo = alvosInimigos(e, j, est).filter((a) => vidaRest(e, a) <= valor).sort((a, b) => valorPerm(e, b) - valorPerm(e, a))[0];
      if (!alvo && (est.queimaFace || vidaRest(e, adv.conjuradorId) <= valor + 2)) alvo = adv.conjuradorId;
      if (!alvo) continue;
    } else continue;
    if (z.arcano < necessario) {
      const mana = manaPerm(e, j)[0];
      if (mana) return { tipo: 'ativar', jogador: j, carta: mana };
      const g = grauUsavel(e, j)[0];
      if (!g) continue;
      return { tipo: 'suspenderGrau', jogador: j, carta: g };
    }
    return { tipo: 'ativar', jogador: j, carta: id, alvo, enviarArcano: [] };
  }
  return null;
}

/** Alquimista Ricardo: ativa o desconto antes de jogar quando há algo para jogar. */
function planejarDesconto(e: Estado, j: Jogador, est: Estilo): Comando | null {
  const z = e.jogadores[j];
  const at = defConjurador(e, j).poder?.ativacao;
  if (!at || !at.efeitos.some((ef) => ef.tipo === 'descontoProximaCarta')) return null;
  if (e.instancias[z.conjuradorId].suspensa || e.habUsos[j].carta || e.habUsos[j].desconto) return null;
  if (at.requisito?.grau && grauEfetivo(e, j) < at.requisito.grau) return null;
  if (!planejarJogada(e, j, est, false, undefined, 0)) return null;
  const custoTotal = at.custo + (at.requisito?.arcano ?? 0);
  if (custoTotal > 0) {
    // habilidade paga (editor do autor): só vale a pena se o desconto for maior que o custo, e é preciso ter o Arcano
    const desconto = at.efeitos.reduce((s, ef) => s + (ef.tipo === 'descontoProximaCarta' ? ef.valor : 0), 0);
    if (desconto <= custoTotal) return null;
    const disponivel = z.arcano + grauUsavel(e, j).length + manaPerm(e, j).reduce((t, id) => t + valorMana(e, id), 0);
    if (disponivel < custoTotal + 1) return null;
    if (z.arcano < custoTotal) {
      const mana = manaPerm(e, j)[0];
      if (mana) return { tipo: 'ativar', jogador: j, carta: mana };
      return { tipo: 'suspenderGrau', jogador: j, carta: grauUsavel(e, j)[0] };
    }
  }
  return { tipo: 'ativar', jogador: j, carta: z.conjuradorId };
}

/** Mitonho: em resposta a uma carta valiosa do oponente, libera o Anular do cemitério. */
function planejarLiberarAnular(e: Estado, j: Jogador, est: Estilo): Comando | null {
  const z = e.jogadores[j];
  const at = defConjurador(e, j).poder?.ativacao;
  if (!at?.repentina || !at.efeitos.some((ef) => ef.tipo === 'permitirAnularDoCemiterio')) return null;
  if (e.instancias[z.conjuradorId].suspensa || e.habUsos[j].anularDoCemiterio) return null;
  const topo = e.pilha[e.pilha.length - 1];
  if (!topo || topo.tipo !== 'carta' || topo.controlador === j) return null;
  const anula = z.cemiterio.find((id) => defCarta(e, id).efeitos?.some((ef) => ef.tipo === 'anular') && defCarta(e, id).grau <= grauEfetivo(e, j));
  if (!anula || !intencao(e, j, anula, est)) return null;
  const disponivel = z.arcano + grauUsavel(e, j).length + manaPerm(e, j).reduce((t, id) => t + valorMana(e, id), 0);
  if (disponivel < at.custo + defCarta(e, anula).custo) return null;
  if (z.arcano < at.custo) {
    const mana = manaPerm(e, j)[0];
    if (mana) return { tipo: 'ativar', jogador: j, carta: mana };
    return { tipo: 'suspenderGrau', jogador: j, carta: grauUsavel(e, j)[0] };
  }
  return { tipo: 'ativar', jogador: j, carta: z.conjuradorId, enviarArcano: [] };
}

function planejarAtaqueConj(e: Estado, j: Jogador, est: Estilo): Comando | null {
  if (!est.conjAtaca) return null;
  if (e.regras.primeiroConjSemAtacarTurno1 && j === e.primeiro && e.turno === 1) return null;
  const z = e.jogadores[j];
  const adv = e.jogadores[outro(j)];
  if (e.instancias[z.conjuradorId].suspensa) return null;
  if (ataqueConjurador(e, j) <= 0) return null;
  if (adv.campo.some((id) => ehAliado(e, id) && !e.instancias[id].suspensa)) return null;
  const custo = defConjurador(e, j).custoAtaque;
  if (z.arcano < custo) {
    const mana = manaPerm(e, j)[0];
    if (mana) return { tipo: 'ativar', jogador: j, carta: mana };
    const g = grauUsavel(e, j)[0];
    if (!g) return null;
    return { tipo: 'suspenderGrau', jogador: j, carta: g };
  }
  return { tipo: 'atacar', jogador: j, atacante: z.conjuradorId, alvo: adv.conjuradorId, enviarArcano: [] };
}

function decidirAnulacao(e: Estado, j: Jogador, custo: number, instId: string, alvoEscolhido?: string): Comando {
  const z = e.jogadores[j];
  // na visão online a carta pode não ter chegado (fora da pilha): sem saber o que é, não paga
  if (!e.instancias[instId] || e.instancias[instId].ehConjurador || e.instancias[instId].defId === 'oculta') return { tipo: 'pagarAnulacao', jogador: j, pagar: false };
  const d = defCarta(e, instId);
  const ef = d.efeitos?.[0];
  let pagar = false;
  if (ef && ef.tipo === 'dano') {
    const alvo = ef.alvo === 'escolhido' ? alvoEscolhido : z.conjuradorId;
    if (alvo === z.conjuradorId) pagar = vidaRest(e, alvo) - ef.valor <= 6 || custo <= 1;
    else if (alvo && z.campo.includes(alvo)) pagar = valorPerm(e, alvo) >= 5 && vidaRest(e, alvo) <= ef.valor;
  }
  const pitch: string[] = [];
  if (pagar) {
    let falta = custo - z.arcano - grauUsavel(e, j).length;
    for (const o of ordemDeSacrificio(e, j, z.mao)) {
      if (falta <= 0) break;
      pitch.push(o);
      falta -= defCarta(e, o).arcanoGerado;
    }
    if (falta > 0) pagar = false;
  }
  return { tipo: 'pagarAnulacao', jogador: j, pagar, enviarArcano: pagar ? pitch : [] };
}

function podeAtacar(e: Estado, id: string): boolean {
  const i = e.instancias[id];
  return !i.suspensa && ehAliado(e, id) && !recemChegado(e, id);
}

function planejarAtaque(e: Estado, j: Jogador, est: Estilo): Comando | null {
  const z = e.jogadores[j];
  const adv = e.jogadores[outro(j)];
  const disponiveis = z.campo.filter((id) => ehAliado(e, id) && !e.instancias[id].suspensa);
  if (disponiveis.length === 0) return null;
  const emCasa = Math.round((1 - est.agressividade) * disponiveis.length);
  // quem fica em casa: os de maior vida restante (melhores defensores)
  const ficam = new Set([...disponiveis].sort((a, b) => vidaRest(e, b) - vidaRest(e, a)).slice(0, emCasa));
  const bloqueadores = adv.campo.filter((id) => ehAliado(e, id) && !e.instancias[id].suspensa);
  if (est.preveBloqueio) {
    // cada bloqueador só bloqueia um ataque e fica suspenso até o fim do próximo turno do dono:
    // com mais atacantes do que bloqueadores, o excesso passa
    const atacantes = disponiveis.filter((a) => !ficam.has(a) && podeAtacar(e, a) && atk(e, a) > 0 && !(est.lento && defCarta(e, a).grau < 6));
    if (atacantes.length > bloqueadores.length + 1 || (atacantes.length > bloqueadores.length && bloqueadores.length <= 2)) {
      const a = [...atacantes].sort((x, y) => valorPerm(e, x) - valorPerm(e, y))[0];
      return { tipo: 'atacar', jogador: j, atacante: a, alvo: adv.conjuradorId };
    }
  }
  if (est.atacaAliados && est.agressividade < 0.9) {
    const c = ataqueAoAliado(e, j, est, disponiveis);
    if (c) return c;
  }
  for (const a of [...disponiveis].sort((x, y) => atk(e, y) - atk(e, x))) {
    if (ficam.has(a) || !podeAtacar(e, a) || atk(e, a) <= 0) continue;
    if (est.lento && defCarta(e, a).grau < 6) continue;
    if (est.preveBloqueio) {
      const b = escolherBloqueador(e, outro(j), a, adv.conjuradorId);
      const ok = b === null || b === adv.conjuradorId || atk(e, b) < vidaRest(e, a) || (atk(e, a) >= vidaRest(e, b) && valorPerm(e, a) <= valorPerm(e, b));
      if (ok || (est.agressividade >= 0.9 && b !== null && atk(e, a) >= vidaRest(e, b))) return { tipo: 'atacar', jogador: j, atacante: a, alvo: adv.conjuradorId };
      continue;
    }
    const podemBloquear = etereo(e, a) ? bloqueadores.filter((b) => etereo(e, b)) : bloqueadores;
    if (podemBloquear.length === 0) return { tipo: 'atacar', jogador: j, atacante: a, alvo: adv.conjuradorId };
    if (bloqueadores.length === 0) return { tipo: 'atacar', jogador: j, atacante: a, alvo: adv.conjuradorId };
    const pior = [...bloqueadores].sort((x, y) => atk(e, y) - atk(e, x))[0];
    const sobrevive = atk(e, pior) < vidaRest(e, a);
    const mata = atk(e, a) >= vidaRest(e, pior);
    if (sobrevive || (mata && valorPerm(e, a) <= valorPerm(e, pior)) || (est.agressividade >= 0.9 && mata)) {
      return { tipo: 'atacar', jogador: j, atacante: a, alvo: adv.conjuradorId };
    }
  }
  return est.atacaAliados ? ataqueAoAliado(e, j, est, disponiveis) : null;
}

/** Ataca um Aliado, Mística ou Estrutura inimiga quando mata o alvo e sai bem da troca (esses ataques não podem ser bloqueados). */
function ataqueAoAliado(e: Estado, j: Jogador, est: Estilo, disponiveis: string[]): Comando | null {
  const adv = e.jogadores[outro(j)];
  const alvos = adv.campo
    .filter((id) => ['aliado', 'mistica', 'estrutura'].includes(defCarta(e, id).tipo) && (defCarta(e, id).vida ?? 0) > 0)
    .sort((x, y) => valorPerm(e, y) - valorPerm(e, x));
  const atacantes = disponiveis.filter((a) => podeAtacar(e, a) && atk(e, a) > 0 && !(est.lento && defCarta(e, a).grau < 6));
  for (const x of alvos) {
    // o atacante mais barato que mata o alvo e sobrevive (ou troca para cima)
    const bons = atacantes
      .filter((a) => atk(e, a) >= vidaRest(e, x) && (atk(e, x) < vidaRest(e, a) || valorPerm(e, a) < valorPerm(e, x)))
      .sort((p, q) => valorPerm(e, p) - valorPerm(e, q));
    for (const a of bons) {
      const b = escolherBloqueador(e, outro(j), a, x);
      const ok = b === null || atk(e, b) < vidaRest(e, a) || (atk(e, a) >= vidaRest(e, b) && valorPerm(e, a) <= valorPerm(e, b));
      if (ok) return { tipo: 'atacar', jogador: j, atacante: a, alvo: x };
    }
  }
  return null;
}

/** Bloqueador que o jogador j escolheria contra este ataque (null = não bloqueia). */
function escolherBloqueador(e: Estado, j: Jogador, atacante: string, alvo: string): string | null {
  const z = e.jogadores[j];
  const forca = atk(e, atacante);
  const alvoEhMeuConj = alvo === z.conjuradorId;
  if (!alvoEhMeuConj) return null; // decisão 20: só se bloqueia ataque ao Conjurador
  const letal = alvoEhMeuConj && vidaRest(e, alvo) <= forca;
  const opcoes = [...z.campo.filter((id) => ehAliado(e, id)), z.conjuradorId].filter((id) => !e.instancias[id].suspensa && (!etereo(e, atacante) || etereo(e, id)));
  const ehConj = (id: string) => id === z.conjuradorId;
  const pontuar = (b: string): number => {
    const sobrevive = forca < vidaRest(e, b);
    const mata = atk(e, b) >= vidaRest(e, atacante);
    if (ehConj(b)) return letal ? 1 : forca <= 1 ? 2 : -10;
    if (mata && sobrevive) return 10;
    if (mata && valorPerm(e, b) <= valorPerm(e, atacante)) return 6;
    if (letal) return 3;
    if (alvoEhMeuConj && sobrevive && vidaRest(e, alvo) <= forca * 3) return 4;
    return -1;
  };
  const melhor = opcoes.map((b) => ({ b, p: pontuar(b) })).sort((x, y) => y.p - x.p)[0];
  return melhor && melhor.p > 0 ? melhor.b : null;
}

function decidirBloqueio(e: Estado, j: Jogador): Comando {
  const c = e.combate!;
  const primeiro = escolherBloqueador(e, j, c.atacante, c.alvo);
  if (primeiro === null || e.instancias[primeiro].ehConjurador) return { tipo: 'bloquear', jogador: j, bloqueador: primeiro };
  // Bloqueio em grupo (decisão 20): junta Aliados quando, juntos, derrubam o atacante, ou quando o ataque seria letal
  // (o dano que passa da vida dos bloqueadores vai para o Conjurador).
  const z = e.jogadores[j];
  const forca = atk(e, c.atacante);
  const letal = vidaRest(e, z.conjuradorId) <= forca;
  const extras = z.campo
    .filter((id) => id !== primeiro && ehAliado(e, id) && !e.instancias[id].suspensa && (!etereo(e, c.atacante) || etereo(e, id)))
    .sort((x, y) => valorPerm(e, x) - valorPerm(e, y));
  const grupo = [primeiro];
  const soma = (f: (id: string) => number) => grupo.reduce((t, id) => t + f(id), 0);
  for (const b of extras) {
    const mata = soma((id) => atk(e, id)) >= vidaRest(e, c.atacante);
    const absorve = soma((id) => vidaRest(e, id)) >= forca;
    if (mata && (absorve || !letal)) break;
    if (!mata && !letal && valorPerm(e, b) > valorPerm(e, c.atacante)) continue;
    grupo.push(b);
  }
  // só fica com o grupo se ele derruba o atacante ou segura um ataque letal; senão, o bloqueio simples
  const vale = soma((id) => atk(e, id)) >= vidaRest(e, c.atacante) || (letal && soma((id) => vidaRest(e, id)) > 0);
  return vale && grupo.length > 1 ? { tipo: 'bloquear', jogador: j, bloqueadores: grupo } : { tipo: 'bloquear', jogador: j, bloqueador: primeiro };
}

export function decidir(e: Estado, j: Jogador, est: Estilo): Comando {
  estAtual = est;
  const z = e.jogadores[j];
  const pend = e.pendencia;
  if (pend) {
    if (pend.tipo === 'escolherInicio') return { tipo: 'escolherInicio', jogador: j, primeiro: j };
    if (pend.tipo === 'descartar') return { tipo: 'descartar', jogador: j, cartas: ordemDeSacrificio(e, j, z.mao).slice(0, pend.quantidade) };
    if (pend.tipo === 'pagarAnulacao' && pend.item.tipo === 'carta') return decidirAnulacao(e, j, pend.custo, pend.item.instId, pend.item.alvo);
    if (pend.tipo === 'escolherDoTopo') {
      // fica com a carta mais valiosa que dá para jogar logo (Grau até 1 acima do atual)
      const g = grauEfetivo(e, j);
      const nota = (id: string) => { const d = defCarta(e, id); return valorCarta(d, z.grau.length) - (d.grau > g + 1 ? 6 : 0) - (d.custo === 0 ? 4 : 0); };
      return { tipo: 'escolherDoTopo', jogador: j, cartas: [...pend.cartas].sort((a, b) => nota(b) - nota(a)).slice(0, pend.pegar) };
    }
    if (pend.tipo === 'distribuirDano') return { tipo: 'distribuirDano', jogador: j }; // padrão: derruba o máximo de bloqueadores
    return decidirBloqueio(e, j);
  }
  const passar: Comando = { tipo: 'passar', jogador: j };
  const principalLivre = e.ativo === j && e.etapa === 'principal' && e.pilha.length === 0 && e.combate === null;
  if (principalLivre) {
    const conj = e.instancias[z.conjuradorId];
    const adv = e.jogadores[outro(j)];
    const conjLivreParaAtacar = !!est.conjAtaca && !e.regras.grauSemSuspender && z.grau.length >= Math.min(est.grauAlvo, 3) && !adv.campo.some((id) => ehAliado(e, id) && !e.instancias[id].suspensa);
    const grauProibido = (e.regras.primeiroSemGrauNoTurno1 && j === e.primeiro && e.turno === 1) || (e.regras.grauSemSuspender && e.grauNoTurno);
    // jogador melhor: sobe o Grau até poder jogar as cartas que tem na mão
    const grauDaMao = est.sobeGrau ? Math.max(0, ...z.mao.map((id) => defCarta(e, id).grau)) : 0;
    const grauAlvo = Math.min(Math.max(est.grauAlvo, grauDaMao), GRAU_MAXIMO);
    if ((!conj.suspensa || e.regras.grauSemSuspender) && !grauProibido && !conjLivreParaAtacar && z.mao.length >= 2 && z.grau.length < GRAU_MAXIMO && (est.sobeGrau ? grauEfetivo(e, j) : z.grau.length) < grauAlvo) {
      return { tipo: 'colocarGrau', jogador: j, carta: ordemDeSacrificio(e, j, z.mao)[0] };
    }
    let reserva = 0;
    if (est.reservaInstantes) {
      const custos = z.mao.map((id) => defCarta(e, id)).filter((d) => d.velocidade === 'repentina' && d.tipo !== 'aliado' && d.grau <= z.grau.length).map((d) => d.custo);
      if (custos.length && Math.min(...custos) <= z.grau.length) reserva = Math.min(...custos);
    }
    return planejarDesconto(e, j, est) ?? planejarJogada(e, j, est, false, undefined, reserva) ?? planejarAtivacao(e, j, est) ?? planejarAtaque(e, j, est) ?? planejarAtaqueConj(e, j, est) ?? passar;
  }
  if (e.ativo !== j) {
    const topo = e.pilha[e.pilha.length - 1];
    if (topo && topo.tipo === 'carta' && topo.controlador !== j) {
      const alvo = topo.instId;
      const c = planejarJogada(e, j, est, true, (i) => i.alvo === alvo, 0) ?? planejarLiberarAnular(e, j, est);
      if (c) return c;
    } else if (topo && topo.tipo === 'ataque' && topo.controlador !== j) {
      const atacante = topo.atacante;
      const c = planejarJogada(e, j, est, true, (i) => i.alvo === atacante, 0);
      if (c) return c;
    } else if (!topo && e.etapa === 'final') {
      const c = planejarJogada(e, j, est, true, (i) => i.utilidade >= 4, 0);
      if (c) return c;
    }
  }
  // janela depois do bloqueio e antes do dano (pedido do autor, 04/10): os dois jogadores podem jogar Repentinas.
  // Quem ataca mira o bloqueador; quem defende mira o atacante.
  if (e.combate?.etapa === 'bloqueado' && e.pilha.length === 0 && e.combate.bloqueador) {
    const inimigo = e.ativo === j ? e.combate.bloqueador : e.combate.atacante;
    const c = planejarJogada(e, j, est, true, (i) => i.alvo === inimigo, 0, inimigo);
    if (c) return c;
  }
  return passar;
}
