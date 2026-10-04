// Motor de regras do Arcanoclisma TCG (fase 1). Muta o estado e lança ErroRegra se o comando for ilegal.
import type {
  BaralhoDef, CartaDef, Catalogo, Comando, Continuacao, ConjuradorDef, Destino, Efeito, Estado, Etapa, FonteDano, HabUsos, Inst, ItemPilha, Jogador, Quando, Regras, Requisito, ZonasJogador,
} from './tipos.ts';

export const ARCANO_POR_GRAU = 1;
export const GRAU_MAXIMO = 12;
export const LIMITE_MAO = 7;

export class ErroRegra extends Error {}

const outro = (j: Jogador): Jogador => (j === 0 ? 1 : 0);

function falha(msg: string): never {
  throw new ErroRegra(msg);
}

function aleatorio(e: Estado): number {
  e.rng = (e.rng + 0x6d2b79f5) >>> 0;
  let t = e.rng;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}

function embaralhar<T>(e: Estado, v: T[]): void {
  for (let i = v.length - 1; i > 0; i--) {
    const j = Math.floor(aleatorio(e) * (i + 1));
    [v[i], v[j]] = [v[j], v[i]];
  }
}

function anotar(e: Estado, msg: string): void {
  e.log.push(msg);
}

const novoHab = (): HabUsos => ({ gatilho: false, carta: false, repentina: false, desconto: 0, anularDoCemiterio: false, danoAliado: false, armadura: false, potencia: 0, ataqueExtra: 0 });

function novaInst(e: Estado, defId: string, dono: Jogador, ehConjurador: boolean): Inst {
  const inst: Inst = { id: `i${e.proxId++}`, defId, ehConjurador, dono, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.instancias[inst.id] = inst;
  return inst;
}

// ---------- consultas ----------

/**
 * A carta pode ser jogada a qualquer momento com prioridade? Temporárias, Místicas e Estruturas: velocidade Repentina.
 * Aliado nunca é Repentina pelo tipo (decisão 21); só entra fora da fase principal com a palavra-chave Repentino no texto.
 */
export function ehRepentina(d: CartaDef): boolean {
  return d.tipo === 'aliado' ? !!d.palavrasChave?.includes('Repentino') : d.velocidade === 'repentina';
}

export function defCarta(e: Estado, instId: string): CartaDef {
  const inst = e.instancias[instId];
  if (!inst || inst.ehConjurador) falha(`Não é uma carta: ${instId}`);
  return e.catalogo.cartas[inst.defId];
}

export function defConjurador(e: Estado, j: Jogador): ConjuradorDef {
  return e.catalogo.conjuradores[e.instancias[e.jogadores[j].conjuradorId].defId];
}

function vidaDe(e: Estado, instId: string): number | undefined {
  const inst = e.instancias[instId];
  const base = inst.ehConjurador ? defConjurador(e, inst.dono).vida : defCarta(e, instId).vida;
  return base === undefined ? undefined : base + (inst.bonusVida ?? 0) + (inst.bonusVidaTurno ?? 0);
}

/** Vida atual (com bônus do construtor de efeitos), para a interface. */
export function vidaAtualDe(e: Estado, instId: string): number | undefined {
  return e.instancias[instId] ? vidaDe(e, instId) : undefined;
}
/** Ataque atual (com bônus), para a interface. */
export function ataqueAtualDe(e: Estado, instId: string): number {
  return e.instancias[instId] ? ataqueDe(e, instId) : 0;
}

/** Incomensurável (decisão 25): Aliado com a palavra-chave, ou Conjurador com arma Incomensurável. */
export function incomensuravel(e: Estado, instId: string): boolean {
  const i = e.instancias[instId];
  if (!i) return false;
  if (i.ehConjurador) { const a = armaDe(e, i.dono); return !!a && !!defCarta(e, a).palavrasChave?.includes('Incomensurável'); }
  return !!defCarta(e, instId).palavrasChave?.includes('Incomensurável');
}
const temIncomensuravel = (d: CartaDef | undefined) => !!d?.palavrasChave?.includes('Incomensurável');
/** Efeitos resolvendo agora vêm de uma fonte Incomensurável? (o dano que passar da Vida da carta vai para o Conjurador dela) */
let fonteIncomensuravel = false;

/** Etéreo (decisão 24): Aliado com a palavra-chave, ou Conjurador com arma Etérea. Só Etéreos atingem Etéreos em combate. */
export function etereo(e: Estado, instId: string): boolean {
  const i = e.instancias[instId];
  if (!i) return false;
  if (i.ehConjurador) { const a = armaDe(e, i.dono); return !!a && !!defCarta(e, a).palavrasChave?.includes('Etéreo'); }
  return !!defCarta(e, instId).palavrasChave?.includes('Etéreo');
}

function ataqueDe(e: Estado, instId: string, bloqueando = false): number {
  const inst = e.instancias[instId];
  // Conjurador que bloqueia não devolve o dano da arma (só dano básico), salvo efeito da arma (futuro)
  return inst.ehConjurador ? (bloqueando ? defConjurador(e, inst.dono).danoBasico : ataqueConjurador(e, inst.dono)) : (defCarta(e, instId).ataque ?? 0) + (inst.bonusAtaque ?? 0) + (inst.bonusAtaqueTurno ?? 0);
}

/** A arma do Conjurador (Canal do tipo Arma), se houver. */
export function armaDe(e: Estado, j: Jogador): string | undefined {
  return (e.jogadores[j].itens ?? []).find((id) => { const d = defCarta(e, id); return d.categoriaItem === 'canal' && (d.tipoCanal ?? 'arma') === 'arma' && (d.ataque ?? 0) > 0; });
}

/** Arcano que o Conjurador paga para atacar: o custo de ataque dele + o custo da arma (pedido do autor, 04/10). */
export function custoAtaqueConjurador(e: Estado, j: Jogador): number {
  const arma = armaDe(e, j);
  return defConjurador(e, j).custoAtaque + (arma ? defCarta(e, arma).custo : 0);
}

/** Grau mínimo para o Conjurador atacar: o Grau da arma (0 sem arma). */
export function grauAtaqueConjurador(e: Estado, j: Jogador): number {
  const arma = armaDe(e, j);
  return arma ? defCarta(e, arma).grau : 0;
}

/**
 * Item serve a este Conjurador? Itens sem Valor (básicos ou genéricos) servem a todos; um item com Valor só serve
 * se tiver os dois Valores do Conjurador (pedido do autor, 04/10). Competência e exclusividade continuam valendo.
 */
export function itemServe(conj: ConjuradorDef, d: CartaDef): boolean {
  if (d.tipo !== 'item') return false;
  if (d.valores.length && !conj.valores.every((v) => d.valores.includes(v))) return false;
  if (d.competencia && d.competencia !== conj.comprometimento) return false;
  if (d.exclusivoDe?.length && !d.exclusivoDe.includes(conj.id)) return false;
  return true;
}

/** Ataque do Conjurador ao atacar: dano básico + o Ataque do Canal (a arma). O Extra é um item consumível de suporte, sem Ataque. */
export function ataqueConjurador(e: Estado, j: Jogador): number {
  const z = e.jogadores[j];
  const armas = (z.itens ?? []).reduce((s, id) => { const d = defCarta(e, id); return s + (d.categoriaItem === 'canal' ? (d.ataque ?? 0) : 0); }, 0);
  const ci = e.instancias[z.conjuradorId];
  return defConjurador(e, j).danoBasico + armas + (e.habUsos?.[j]?.ataqueExtra ?? 0) + (ci?.bonusAtaque ?? 0) + (ci?.bonusAtaqueTurno ?? 0);
}

/** Armadura equipada no Conjurador (instância), se houver. */
export function armaduraDe(e: Estado, j: Jogador): string | undefined {
  return (e.jogadores[j].itens ?? []).find((id) => defCarta(e, id).categoriaItem === 'armadura');
}

/** Nome para o registro: Conjuradores levam o jogador, para não confundir em espelhos. */
function nomeReg(e: Estado, id: string): string {
  const i = e.instancias[id];
  if (!i) return id;
  return i.ehConjurador ? `${e.catalogo.conjuradores[i.defId].nome} (jogador ${i.dono})` : e.catalogo.cartas[i.defId].nome;
}

function naMesa(e: Estado, instId: string): boolean {
  const inst = e.instancias[instId];
  if (!inst) return false;
  const z = e.jogadores[inst.dono];
  return z.conjuradorId === instId || z.campo.includes(instId);
}

// ---------- movimentação ----------

function tirar(v: string[], id: string): boolean {
  const i = v.indexOf(id);
  if (i < 0) return false;
  v.splice(i, 1);
  return true;
}

function comprar(e: Estado, j: Jogador, n: number): void {
  const z = e.jogadores[j];
  for (let i = 0; i < n; i++) {
    const id = z.baralho.shift();
    if (id === undefined) return; // baralho vazio não faz perder
    z.mao.push(id);
  }
}

// ---------- construtor de efeitos (pedido do autor, 04/10) ----------

const habsDe = (d: CartaDef, q: Quando) => (d.habilidades ?? []).map((h, i) => ({ h, i })).filter((x) => x.h.quando === q);
/** Efeitos ao jogar: os da carta + habilidades "ao jogar". */
export function efeitosAoJogar(d: CartaDef): Efeito[] { return [...(d.efeitos ?? []), ...habsDe(d, 'jogar').flatMap((x) => x.h.efeitos)]; }
/** Efeitos ao entrar em campo: aoEntrar + habilidades "ao entrar". */
export function efeitosAoEntrar(d: CartaDef): Efeito[] { return [...(d.aoEntrar ?? []), ...habsDe(d, 'entrar').flatMap((x) => x.h.efeitos)]; }

const QUANDO_LOG: Record<Quando, string> = {
  jogar: 'ao ser jogada', entrar: 'ao entrar', ativada: 'ativada', alvo: 'virou alvo', grau: 'foi para o Grau', atacar: 'atacou',
  bloquear: 'defendeu', cemiterio: 'caiu no cemitério', banimento: 'foi banida', danoConjurador: 'acertou o Conjurador',
};
/** Dispara as habilidades da carta para esta condição: cada uma vai para a pilha como gatilho do dono. */
function disparar(e: Estado, instId: string, quando: Quando, alvo?: string): void {
  const inst = e.instancias[instId];
  if (!inst || inst.ehConjurador || !e.catalogo.cartas[inst.defId]) return;
  const d = defCarta(e, instId);
  for (const { h, i } of habsDe(d, quando)) {
    if (h.umaVezPorTurno) { inst.usosHab ??= {}; if (inst.usosHab[i] === e.turno) continue; inst.usosHab[i] = e.turno; }
    e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: inst.dono, efeitos: h.efeitos, alvo, fonte: d.nome, incomensuravel: temIncomensuravel(d) });
    anotar(e, `${d.nome} ${QUANDO_LOG[quando]}: o efeito vai para a pilha.`);
  }
}

/** Tira a carta de qualquer zona dos dois jogadores. */
function tirarDeTudo(e: Estado, id: string): void {
  for (const z of e.jogadores) for (const zona of [z.mao, z.baralho, z.cemiterio, z.banimento, z.grau, z.campo, z.zonaArcano, z.itens]) tirar(zona, id);
}
/** Carta que sai do campo perde dano e bônus. */
function limparInst(i: Inst): void {
  i.dano = 0; i.suspensa = false; i.bonusAtaque = 0; i.bonusVida = 0; i.bonusAtaqueTurno = 0; i.bonusVidaTurno = 0; i.naoAtacaAte = 0;
}
/** Põe uma permanente em campo sob o controle de `j` (sem pagar) e dispara "ao entrar". */
function porEmCampo(e: Estado, id: string, j: Jogador, alvo?: string): void {
  const inst = e.instancias[id];
  tirarDeTudo(e, id);
  limparInst(inst);
  inst.dono = j; inst.revelada = true; inst.entrouNoTurno = e.turno;
  e.jogadores[j].campo.push(id);
  const d = defCarta(e, id);
  anotar(e, `${d.nome} entra em campo (jogador ${j}).`);
  if (d.tipo === 'aliado') gatilhoConjurador(e, j, 'aliadoEntra');
  const efs = efeitosAoEntrar(d);
  if (efs.length) e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: j, efeitos: efs, alvo, fonte: d.nome, incomensuravel: temIncomensuravel(d) });
}
/** Manda a carta escolhida para o destino (do dono dela; "roubar" vai para a mão de quem escolheu). */
function moverPara(e: Estado, id: string, destino: Destino, escolhedor: Jogador): void {
  if (destino === 'nada') return;
  const inst = e.instancias[id];
  if (destino === 'campo') { porEmCampo(e, id, escolhedor); return; }
  tirarDeTudo(e, id);
  limparInst(inst);
  const z = e.jogadores[inst.dono];
  if (destino === 'mao') z.mao.push(id);
  else if (destino === 'roubar') { inst.dono = escolhedor; e.jogadores[escolhedor].mao.push(id); }
  else if (destino === 'grau') { inst.revelada = false; z.grau.push(id); disparar(e, id, 'grau'); }
  else if (destino === 'cemiterio') z.cemiterio.push(id);
  else if (destino === 'fundo') z.baralho.push(id);
  else if (destino === 'topo') z.baralho.unshift(id);
  else if (destino === 'banimento') z.banimento.push(id);
}
const DESTINO_TXT: Record<Destino, string> = { mao: 'para a mão', roubar: 'para a sua mão', grau: 'para o Grau', cemiterio: 'para o cemitério', fundo: 'para o fundo do baralho', topo: 'para o topo do baralho', campo: 'para o campo', banimento: 'para o banimento', nada: '' };

/** Cartas que o efeito "campo" pode atingir: lado (inimigo/próprio/ambos) e tipos (Aliado, Mística, Estrutura, Conjurador, equipamento). */
function alvosCampo(e: Estado, dono: Jogador, ef: Extract<Efeito, { tipo: 'campo' }>): string[] {
  const lados: Jogador[] = ef.lado === 'inimigo' ? [outro(dono)] : ef.lado === 'proprio' ? [dono] : [0, 1];
  const r: string[] = [];
  for (const j of lados) {
    const z = e.jogadores[j];
    if (ef.tipos.includes('conjurador')) r.push(z.conjuradorId);
    for (const id of z.campo) if ((ef.tipos as string[]).includes(defCarta(e, id).tipo)) r.push(id);
    if (ef.tipos.includes('equipamento')) r.push(...z.itens);
  }
  return r;
}
const ACOES_RUINS = ['dano', 'destruir', 'devolver', 'paraBaralho', 'naoAtaca', 'suspender'];
/** Alvo automático de gatilhos sem alvo escolhido: o inimigo mais forte (efeito ruim) ou o seu mais ferido (efeito bom). */
function alvoAutomatico(e: Estado, lista: string[], ruim: boolean): string | undefined {
  const nota = (id: string) => (ruim ? ataqueDe(e, id) * 10 + (vidaDe(e, id) ?? 0) : e.instancias[id].dano * 10 + ataqueDe(e, id));
  return [...lista].filter((id) => !e.jogadores.some((z) => z.itens.includes(id)) || ruim).sort((a, b) => nota(b) - nota(a))[0];
}
/** Aplica a ação do efeito "campo" numa carta. */
function aplicarAcaoCampo(e: Estado, id: string, ef: Extract<Efeito, { tipo: 'campo' }>, dono: Jogador, bonus: number): void {
  const inst = e.instancias[id];
  if (!inst) return;
  const v = (ef.valor ?? 0) + (ef.acao === 'dano' || ef.acao === 'curar' ? bonus : 0);
  const zDono = e.jogadores[inst.dono];
  if (zDono.itens.includes(id)) {
    // equipamento: destruir manda ao cemitério; dano desgasta a Armadura
    const d = defCarta(e, id);
    if (ef.acao === 'destruir' || (ef.acao === 'dano' && d.categoriaItem === 'armadura' && inst.dano + v >= (d.vidaDesgaste ?? 0))) {
      tirar(zDono.itens, id); inst.dano = 0; zDono.cemiterio.push(id); anotar(e, `${d.nome} foi destruído.`);
    } else if (ef.acao === 'dano' && d.categoriaItem === 'armadura') { inst.dano += v; anotar(e, `${d.nome} se desgasta ${v}.`); }
    return;
  }
  const turno = ef.duracao !== 'permanente';
  switch (ef.acao) {
    case 'dano': danoDeEfeito(e, id, v); break;
    case 'curar': inst.dano = Math.max(0, inst.dano - v); anotar(e, `${nomeReg(e, id)} recupera ${v}.`); break;
    case 'destruir': if (!inst.ehConjurador) destruir(e, id); break;
    case 'devolver': if (!inst.ehConjurador) devolver(e, id); break;
    case 'paraBaralho':
      if (!inst.ehConjurador && tirar(zDono.campo, id)) {
        limparInst(inst);
        if (ef.onde === 'topo') zDono.baralho.unshift(id); else zDono.baralho.push(id);
        if ((ef.onde ?? 'embaralhar') === 'embaralhar') embaralhar(e, zDono.baralho);
        anotar(e, `${defCarta(e, id).nome} volta para o baralho.`);
      }
      break;
    case 'naoAtaca': inst.naoAtacaAte = e.turno + (e.ativo === inst.dono ? 2 : 1); anotar(e, `${nomeReg(e, id)} não pode atacar até o fim do próximo turno do dono.`); break;
    case 'suspender': inst.suspensa = true; break;
    case 'levantar': inst.suspensa = false; break;
    case 'aumentarAtaque': if (turno) inst.bonusAtaqueTurno = (inst.bonusAtaqueTurno ?? 0) + v; else inst.bonusAtaque = (inst.bonusAtaque ?? 0) + v; anotar(e, `${nomeReg(e, id)} ganha +${v} de Ataque${turno ? ' neste turno' : ''}.`); break;
    case 'aumentarVida': if (turno) inst.bonusVidaTurno = (inst.bonusVidaTurno ?? 0) + v; else inst.bonusVida = (inst.bonusVida ?? 0) + v; anotar(e, `${nomeReg(e, id)} ganha +${v} de Vida${turno ? ' neste turno' : ''}.`); break;
  }
}

/** Abre a escolha de cartas; os efeitos seguintes esperam a escolha (efeitos acumulados). */
function pedirEscolha(e: Estado, j: Jogador, cartas: string[], quantidade: number, destino: Destino, titulo: string, resto: Destino | undefined, depois: Continuacao | undefined): void {
  e.pendencia = { tipo: 'escolherCartas', jogador: j, cartas, quantidade: Math.min(quantidade, cartas.length), destino, resto, titulo, ...(depois && depois.efeitos.length ? { depois } : {}) };
}

function destruir(e: Estado, instId: string): void {
  const inst = e.instancias[instId];
  const z = e.jogadores[inst.dono];
  if (tirar(z.campo, instId)) {
    inst.dano = 0;
    inst.suspensa = false;
    z.cemiterio.push(instId);
    anotar(e, `${defCarta(e, instId).nome} foi para o cemitério.`);
    if (defCarta(e, instId).tipo === 'aliado') gatilhoConjurador(e, inst.dono, 'aliadoMorre');
  }
}

/** Devolve uma permanente do campo para a mão do dono. */
function devolver(e: Estado, instId: string): void {
  const inst = e.instancias[instId];
  const z = e.jogadores[inst.dono];
  if (tirar(z.campo, instId)) {
    inst.dano = 0;
    inst.suspensa = false;
    z.mao.push(instId);
    anotar(e, `${defCarta(e, instId).nome} volta para a mão.`);
  }
}

/** Gatilho do Conjurador (uma vez por turno): vai para a pilha como um gatilho de "ao entrar". */
function gatilhoConjurador(e: Estado, j: Jogador, quando: 'aliadoEntra' | 'aliadoMorre' | 'aliadoDanoConjurador'): void {
  const g = defConjurador(e, j).poder?.gatilho;
  if (!g || g.quando !== quando || e.habUsos[j].gatilho) return;
  if (g.soNoSeuTurno && e.ativo !== j) return;
  e.habUsos[j].gatilho = true;
  e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: j, efeitos: g.efeitos, fonte: defConjurador(e, j).nome });
  anotar(e, `Jogador ${j}: gatilho da habilidade de ${defConjurador(e, j).nome}.`);
}

/** Custo que o jogador paga por esta carta agora (reduções do Conjurador). Não inclui Protegido do alvo. */
export function custoEfetivo(e: Estado, j: Jogador, id: string): number {
  const d = defCarta(e, id);
  const p = defConjurador(e, j).poder;
  let menos = 0;
  if (p?.primeiraCartaMaisBarata && !e.habUsos[j].carta) menos += p.primeiraCartaMaisBarata;
  if (p?.primeiraRepentinaMaisBarata && ehRepentina(d) && !e.habUsos[j].repentina) menos += p.primeiraRepentinaMaisBarata;
  if (e.habUsos[j].desconto && !e.habUsos[j].carta) menos += e.habUsos[j].desconto;
  if (e.jogadores[j].banimento.includes(id)) menos += e.instancias[id].descontoBanimento;
  return Math.max(0, d.custo - menos);
}

/** De onde o jogador pode jogar esta carta agora: mão, banimento (no turno liberado) ou cemitério (Anular liberado). */
export function origemDaCarta(e: Estado, j: Jogador, id: string): 'mao' | 'banimento' | 'cemiterio' | null {
  const z = e.jogadores[j];
  if (z.mao.includes(id)) return 'mao';
  if (z.banimento.includes(id) && e.instancias[id].banidaDisponivel === e.turno && e.ativo === j) return 'banimento';
  if (z.cemiterio.includes(id) && e.habUsos[j].anularDoCemiterio && defCarta(e, id).efeitos?.some((ef) => ef.tipo === 'anular')) return 'cemiterio';
  return null;
}

function acoesDeEstado(e: Estado): void {
  for (const j of [0, 1] as Jogador[]) {
    for (const id of [...e.jogadores[j].campo]) {
      const vida = vidaDe(e, id);
      if (vida !== undefined && e.instancias[id].dano >= vida) destruir(e, id);
    }
  }
  const morreu = ([0, 1] as Jogador[]).filter((j) => {
    const c = e.jogadores[j].conjuradorId;
    return e.instancias[c].dano >= (vidaDe(e, c) as number);
  });
  if (morreu.length === 2) e.vencedor = 'empate';
  else if (morreu.length === 1) e.vencedor = outro(morreu[0]);
  if (e.vencedor !== null) anotar(e, e.vencedor === 'empate' ? 'Empate.' : `Jogador ${e.vencedor} venceu.`);
}

// ---------- dano ----------

/** Único caminho de dano do jogo: Raio e pancada são o mesmo dano, com fontes diferentes. */
/** Dano de efeito: com fonte Incomensurável, o que passar da Vida restante da carta vai para o Conjurador do dono dela. */
function danoDeEfeito(e: Estado, alvoId: string, valor: number): void {
  const i = e.instancias[alvoId];
  if (fonteIncomensuravel && i && !i.ehConjurador && naMesa(e, alvoId)) {
    const resta = Math.max(0, (vidaDe(e, alvoId) ?? 0) - i.dano);
    const exc = Math.max(0, valor - resta);
    causarDano(e, alvoId, valor, 'efeito');
    if (exc > 0) { anotar(e, `Incomensurável: ${exc} de dano passa para o Conjurador.`); causarDano(e, e.jogadores[i.dono].conjuradorId, exc, 'efeito'); }
    return;
  }
  causarDano(e, alvoId, valor, 'efeito');
}

function causarDano(e: Estado, alvoId: string, valor: number, fonte: FonteDano): void {
  if (valor <= 0 || !naMesa(e, alvoId)) return;
  // Gancho para anulação: só a fonte 'efeito' poderá ser anulada (fase 2). Combate não.
  e.instancias[alvoId].dano += valor;
  const ia = e.instancias[alvoId];
  anotar(e, `${nomeReg(e, alvoId)} sofre ${valor} de dano (${fonte}).`);
  const autor = outro(e.instancias[alvoId].dono);
  const t = e.turnosDano[autor];
  if (t[t.length - 1] !== e.turno) t.push(e.turno);
}

// ---------- efeitos ----------

function alvoLegal(e: Estado, id: string | undefined): boolean {
  return id !== undefined && naMesa(e, id);
}

function alvoDeAliado(e: Estado, id: string): boolean {
  const i = e.instancias[id];
  return !!i && !i.ehConjurador && ['aliado', 'mistica'].includes(defCarta(e, id).tipo);
}

function resolverAlvo(e: Estado, ef: Efeito, dono: Jogador, escolhido?: string): string | undefined {
  if (!('alvo' in ef)) return undefined;
  if (ef.alvo === 'escolhido') return alvoLegal(e, escolhido) ? escolhido : undefined;
  if (ef.alvo === 'escolhidoAliado') return alvoLegal(e, escolhido) && alvoDeAliado(e, escolhido!) ? escolhido : undefined;
  if (ef.alvo === 'escolhidoProprio') return alvoLegal(e, escolhido) && alvoDeAliado(e, escolhido!) && e.instancias[escolhido!].dono === dono ? escolhido : undefined;
  return e.jogadores[ef.alvo === 'conjuradorInimigo' ? outro(dono) : dono].conjuradorId;
}

/**
 * Requisito de uma linha de efeito (04/10, definido no Editor): Grau mínimo e Arcano pago na hora
 * (reserva primeiro, depois cartas do Grau levantadas). Sem cumprir, a linha não acontece.
 */
function cumprirRequisito(e: Estado, r: Requisito | undefined, dono: Jogador): boolean {
  if (!r) return true;
  if (r.grau && grauEfetivo(e, dono) < r.grau) { anotar(e, `Requisito não cumprido: precisa de Grau ${r.grau} (tem ${grauEfetivo(e, dono)}); a linha não acontece.`); return false; }
  const custo = r.arcano ?? 0;
  if (custo <= 0) return true;
  const z = e.jogadores[dono];
  const levantadas = z.grau.filter((id) => !e.instancias[id].suspensa);
  if (z.arcano + levantadas.length * ARCANO_POR_GRAU < custo) { anotar(e, `Requisito não cumprido: precisa pagar ${custo} de Arcano; a linha não acontece.`); return false; }
  const daReserva = Math.min(z.arcano, custo);
  z.arcano -= daReserva;
  let falta = custo - daReserva;
  for (const id of levantadas) { if (falta <= 0) break; e.instancias[id].suspensa = true; falta -= ARCANO_POR_GRAU; }
  anotar(e, `Jogador ${dono} paga ${custo} de Arcano pelo requisito.`);
  return true;
}

function aplicarEfeitos(e: Estado, efeitos: Efeito[], dono: Jogador, escolhido?: string, bonus = 0, auto = false): void {
  for (let k = 0; k < efeitos.length; k++) {
    const ef = efeitos[k];
    if (ef.requisito && !cumprirRequisito(e, ef.requisito, dono)) continue;
    const depois: Continuacao = { efeitos: efeitos.slice(k + 1), dono, alvo: escolhido };
    if (ef.tipo === 'perderVida') {
      const c = e.jogadores[dono].conjuradorId;
      e.instancias[c].dano += ef.valor;
      anotar(e, `${defConjurador(e, dono).nome} perde ${ef.valor} de Vida.`);
      continue;
    }
    if (ef.tipo === 'descartar') {
      const j = ef.quem === 'voce' ? dono : outro(dono);
      const z = e.jogadores[j];
      const n = Math.min(ef.valor, z.mao.length);
      if (!n) { anotar(e, `Jogador ${j} não tem cartas para descartar.`); continue; }
      if (ef.aleatorio) {
        for (let i = 0; i < n; i++) { const id = z.mao.splice(Math.floor(aleatorio(e) * z.mao.length), 1)[0]; z.cemiterio.push(id); }
        anotar(e, `Jogador ${j} descarta ${n} carta(s) ao acaso.`);
        continue;
      }
      pedirEscolha(e, j, [...z.mao], n, 'cemiterio', `Descarte ${n} carta(s)`, undefined, depois);
      anotar(e, `Jogador ${j} escolhe ${n} carta(s) para descartar.`);
      return;
    }
    if (ef.tipo === 'colocarNoGrau') {
      const z = e.jogadores[dono];
      if (ef.de === 'topo') {
        for (let i = 0; i < ef.valor && z.baralho.length && z.grau.length < GRAU_MAXIMO; i++) { const id = z.baralho.shift()!; e.instancias[id].revelada = false; z.grau.push(id); disparar(e, id, 'grau'); }
        anotar(e, `Jogador ${dono} põe carta(s) do topo do baralho no Grau.`);
        continue;
      }
      if (!z.mao.length) continue;
      pedirEscolha(e, dono, [...z.mao], Math.min(ef.valor, GRAU_MAXIMO - z.grau.length), 'grau', `Ponha ${ef.valor} carta(s) da mão no Grau`, undefined, depois);
      return;
    }
    if (ef.tipo === 'olharTopoPara') {
      const z = e.jogadores[dono];
      const cartas = z.baralho.splice(0, ef.quantidade);
      if (!cartas.length) { anotar(e, 'Olhar o topo: o baralho está vazio.'); continue; }
      anotar(e, `Jogador ${dono} olha as ${cartas.length} carta(s) do topo do baralho.`);
      pedirEscolha(e, dono, cartas, ef.pegar, ef.destino, `Escolha ${ef.pegar} carta(s) ${DESTINO_TXT[ef.destino]}; o resto vai ${DESTINO_TXT[ef.resto]}`, ef.resto, depois);
      return;
    }
    if (ef.tipo === 'olharMao') {
      const adv = e.jogadores[outro(dono)];
      anotar(e, `Jogador ${dono} olha a mão do oponente (${adv.mao.length} carta(s)).`);
      if (!adv.mao.length || ef.pegar <= 0 || ef.destino === 'nada') {
        pedirEscolha(e, dono, [...adv.mao], 0, 'nada', 'Mão do oponente', undefined, depois);
        if (!adv.mao.length) { e.pendencia = null; continue; }
        return;
      }
      pedirEscolha(e, dono, [...adv.mao], ef.pegar, ef.destino, `Escolha ${ef.pegar} carta(s) da mão do oponente ${DESTINO_TXT[ef.destino]}`, undefined, depois);
      return;
    }
    if (ef.tipo === 'jogarDe') {
      const z = e.jogadores[dono];
      const zona = ef.de === 'cemiterio' ? z.cemiterio : ef.de === 'mao' ? z.mao : ef.de === 'baralho' ? z.baralho : z.banimento;
      const tipos = ef.tipos?.length ? ef.tipos : ['aliado'];
      const cands = zona.filter((id) => { const d = defCarta(e, id); return tipos.includes(d.tipo) && d.tipo !== 'temporaria' && d.tipo !== 'item' && (ef.custoMax === undefined || d.custo <= ef.custoMax); });
      if (!cands.length) { anotar(e, 'Nenhuma carta serve para entrar em campo.'); continue; }
      pedirEscolha(e, dono, cands, ef.valor, 'campo', `Ponha em campo ${ef.valor} carta(s)`, undefined, depois);
      return;
    }
    if (ef.tipo === 'campo') {
      const lista = alvosCampo(e, dono, ef);
      let alvos: string[];
      if (ef.alcance === 'todos') alvos = lista;
      else {
        const a = escolhido !== undefined && lista.includes(escolhido) ? escolhido : auto ? alvoAutomatico(e, lista, ACOES_RUINS.includes(ef.acao)) : undefined;
        alvos = a ? [a] : [];
      }
      if (!alvos.length) { anotar(e, 'Efeito sem alvo legal; nada acontece.'); continue; }
      for (const id of alvos) aplicarAcaoCampo(e, id, ef, dono, bonus);
      continue;
    }
    if (ef.tipo === 'comprar') { comprar(e, dono, ef.valor); anotar(e, `Jogador ${dono} compra ${ef.valor} carta(s).`); }
    else if (ef.tipo === 'olharTopo') {
      const z = e.jogadores[dono];
      const cartas = z.baralho.splice(0, ef.quantidade);
      if (!cartas.length) { anotar(e, 'Olhar o topo: o baralho está vazio.'); continue; }
      e.pendencia = { tipo: 'escolherDoTopo', jogador: dono, cartas, pegar: Math.min(ef.pegar, cartas.length) };
      anotar(e, `Jogador ${dono} olha as ${cartas.length} carta(s) do topo do baralho.`);
    }
    else if (ef.tipo === 'arcano') e.jogadores[dono].arcano += ef.valor;
    else if (ef.tipo === 'potenciaMagia') { e.habUsos[dono].potencia += ef.valor; anotar(e, `O próximo dano ou cura de uma Temporária sua neste turno tem +${e.habUsos[dono].potencia}.`); }
    else if (ef.tipo === 'ataqueConjuradorTurno') { e.habUsos[dono].ataqueExtra += ef.valor; anotar(e, `${defConjurador(e, dono).nome} ganha +${ef.valor} de Ataque neste turno.`); }
    else if (ef.tipo === 'invocar') {
      if (!e.catalogo.cartas[ef.carta]) { anotar(e, 'Invocar: ficha desconhecida.'); continue; }
      const inst = novaInst(e, ef.carta, dono, false);
      inst.revelada = true; inst.entrouNoTurno = e.turno;
      e.jogadores[dono].campo.push(inst.id);
      anotar(e, `${defCarta(e, inst.id).nome} é invocado no campo do jogador ${dono}.`);
      gatilhoConjurador(e, dono, 'aliadoEntra');
    }
    else if (ef.tipo === 'arcanoOponente') e.jogadores[outro(dono)].arcano += ef.valor;
    else if (ef.tipo === 'descontoProximaCarta') {
      if (e.habUsos[dono].carta) anotar(e, 'Você já jogou uma carta neste turno: sem desconto.');
      else { e.habUsos[dono].desconto += ef.valor; anotar(e, `A primeira carta do turno custa ${ef.valor} a menos.`); }
    } else if (ef.tipo === 'permitirAnularDoCemiterio') {
      e.habUsos[dono].anularDoCemiterio = true;
      anotar(e, 'Você pode jogar do cemitério uma carta que anula outra.');
    } else if (ef.tipo === 'banirDaMao') {
      const z = e.jogadores[dono];
      if (escolhido === undefined || !tirar(z.mao, escolhido)) { anotar(e, 'Banir: a carta saiu da mão; nada acontece.'); continue; }
      z.banimento.push(escolhido);
      const inst = e.instancias[escolhido];
      inst.descontoBanimento = ef.desconto;
      inst.banidaDisponivel = e.ativo === dono ? e.turno + 2 : e.turno + 1;
      anotar(e, `${defCarta(e, escolhido).nome} foi banida; só pode ser jogada no turno ${inst.banidaDisponivel}, custando ${ef.desconto} a menos.`);
    } else if (ef.tipo === 'reanimar') {
      const dc = e.jogadores[0].cemiterio.includes(escolhido ?? '') ? 0 : e.jogadores[1].cemiterio.includes(escolhido ?? '') ? 1 : -1;
      if (escolhido === undefined || dc < 0 || defCarta(e, escolhido).tipo !== 'aliado') { anotar(e, 'Reanimar: o Aliado saiu do cemitério; nada acontece.'); continue; }
      tirar(e.jogadores[dc as Jogador].cemiterio, escolhido);
      const inst = e.instancias[escolhido];
      inst.dono = dono; inst.dano = 0; inst.suspensa = false; inst.entrouNoTurno = e.turno;
      e.jogadores[dono].campo.push(escolhido);
      const dr = defCarta(e, escolhido);
      anotar(e, `${dr.nome} volta do cemitério para o campo do jogador ${dono}.`);
      gatilhoConjurador(e, dono, 'aliadoEntra');
      const aoEntrarR = efeitosAoEntrar(dr);
      if (aoEntrarR.length) e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: dono, efeitos: aoEntrarR, fonte: dr.nome });
    }
    else if (ef.tipo === 'anular') {
      const i = e.pilha.findIndex((p) => p.tipo === 'carta' && p.instId === escolhido);
      if (i < 0) { anotar(e, 'Anular: a carta já saiu da pilha.'); continue; }
      const item = e.pilha.splice(i, 1)[0];
      if (item.tipo === 'carta') {
        e.jogadores[item.controlador].cemiterio.push(item.instId);
        anotar(e, `${defCarta(e, item.instId).nome} foi anulada.`);
      }
    } else if (ef.tipo === 'danoTodosAliados') {
      const adv = e.jogadores[outro(dono)];
      const ids = ef.ambos ? [...e.jogadores[dono].campo, ...adv.campo] : [...adv.campo];
      for (const id of ids) if (defCarta(e, id).tipo === 'aliado') danoDeEfeito(e, id, ef.valor + bonus);
    } else if (ef.tipo === 'destruirTodosAliados') {
      const quem: Jogador[] = ef.ambos ? [0, 1] : [outro(dono)];
      for (const j of quem) for (const id of [...e.jogadores[j].campo]) if (defCarta(e, id).tipo === 'aliado') destruir(e, id);
    }
    else {
      let alvo = resolverAlvo(e, ef, dono, escolhido);
      if (alvo === undefined && auto && 'alvo' in ef && (ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado' || ef.alvo === 'escolhidoProprio')) {
        const proprio = ef.alvo === 'escolhidoProprio' || ef.tipo === 'curar';
        const lista = alvosCampo(e, dono, { tipo: 'campo', acao: 'dano', alcance: 'todos', lado: proprio ? 'proprio' : 'inimigo', tipos: ef.alvo === 'escolhido' ? ['aliado', 'mistica', 'conjurador'] : ['aliado', 'mistica'] });
        alvo = alvoAutomatico(e, lista, !proprio);
      }
      if (alvo === undefined) {
        anotar(e, 'Efeito sem alvo legal; nada acontece.');
        continue;
      }
      const inst = e.instancias[alvo];
      if (ef.tipo === 'dano') danoDeEfeito(e, alvo, ef.valor + bonus + (ef.escalaGrau ? e.jogadores[dono].grau.length : 0));
      else if (ef.tipo === 'devolver' && !inst.ehConjurador) devolver(e, alvo);
      else if (ef.tipo === 'curar') inst.dano = Math.max(0, inst.dano - ef.valor - bonus);
      else if (ef.tipo === 'destruir' && !inst.ehConjurador) destruir(e, alvo);
    }
  }
}

function exigeAlvo(efeitos: Efeito[] | undefined): boolean {
  return (efeitos ?? []).some((ef) => (ef.tipo === 'campo' && ef.alcance === 'alvo') || ef.tipo === 'banirDaMao' || ef.tipo === 'reanimar' || ('alvo' in ef && (ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado' || ef.alvo === 'escolhidoProprio' || ef.alvo === 'pilha')));
}

/** O alvo escolhido serve para todos os efeitos que o exigem? */
function alvoServe(e: Estado, efeitos: Efeito[], alvo: string | undefined, dono: Jogador): boolean {
  return efeitos.every((ef) => {
    if (ef.tipo === 'banirDaMao') return alvo !== undefined && e.jogadores[dono].mao.includes(alvo);
    if (ef.tipo === 'campo') return ef.alcance === 'todos' || (alvo !== undefined && alvosCampo(e, dono, ef).includes(alvo));
    if (ef.tipo === 'reanimar') return alvo !== undefined && [0, 1].some((j) => e.jogadores[j as Jogador].cemiterio.includes(alvo)) && defCarta(e, alvo).tipo === 'aliado';
    if (!('alvo' in ef)) return true;
    if (ef.alvo === 'escolhidoProprio') return alvoLegal(e, alvo) && alvoDeAliado(e, alvo!) && e.instancias[alvo!].dono === dono;
    if (ef.alvo === 'pilha') return alvo !== undefined && e.pilha.some((p) => p.tipo === 'carta' && p.instId === alvo);
    if (ef.alvo === 'escolhidoAliado') return alvoLegal(e, alvo) && alvoDeAliado(e, alvo!);
    if (ef.alvo === 'escolhido') return alvoLegal(e, alvo);
    return true;
  });
}

/** Grau para requisitos: cartas na Zona de Grau mais o grauExtra das permanentes em campo. */
export function grauEfetivo(e: Estado, j: Jogador): number {
  const z = e.jogadores[j];
  return z.grau.length + z.campo.reduce((s, id) => s + (defCarta(e, id).grauExtra ?? 0), 0);
}

// ---------- criação ----------

export function validarBaralho(cat: Catalogo, b: BaralhoDef, tamanho = 60): string[] {
  const erros: string[] = [];
  const conj = cat.conjuradores[b.conjurador];
  if (!conj) return [`Conjurador desconhecido: ${b.conjurador}`];
  if (b.cartas.length !== tamanho) erros.push(`O baralho tem ${b.cartas.length} cartas; precisa de ${tamanho}.`);
  for (const id of new Set(b.cartas)) {
    const c = cat.cartas[id];
    if (!c) erros.push(`Carta desconhecida: ${id}`);
    else if (c.valores.length > 0 && !c.valores.some((v) => conj.valores.includes(v)))
      erros.push(`${c.nome} não combina com os Valores de ${conj.nome}.`);
  }
  const espacos = new Set<string>();
  for (const id of b.itens ?? []) {
    const c = cat.cartas[id];
    if (!c) { erros.push(`Item desconhecido: ${id}`); continue; }
    if (c.tipo !== 'item' || !c.categoriaItem) { erros.push(`${c.nome} não é um item.`); continue; }
    if (c.competencia && c.competencia !== conj.comprometimento) erros.push(`${c.nome} é só para Conjuradores ${c.competencia}.`);
    if (c.exclusivoDe?.length && !c.exclusivoDe.includes(conj.id)) erros.push(`${c.nome} é exclusivo de outro Conjurador.`);
    if (espacos.has(c.categoriaItem)) erros.push(`Dois itens no mesmo espaço (${c.categoriaItem}).`);
    espacos.add(c.categoriaItem);
    if (c.valores.length > 0 && !conj.valores.every((v) => c.valores.includes(v))) erros.push(`${c.nome} não combina com os dois Valores de ${conj.nome}.`);
  }
  if (b.cartas.some((id) => cat.cartas[id]?.tipo === 'item')) erros.push('Itens não entram no baralho: equipe-os no Conjurador.');
  return erros;
}

/**
 * Regras aprovadas pelo autor (rodada 4, sem a Cataclisma crescente): pôr Grau não suspende o Conjurador (1 por turno),
 * relógio de impasse de 5 turnos, o segundo jogador não recebe carta nem Arcano extra e quem começa não põe Grau no turno 1.
 */
export const REGRAS_PADRAO: Regras = {
  semAcaoPerde: false, primeiroSemGrauNoTurno1: true, grauSemSuspender: true, primeiroConjSemAtacarTurno1: false, impasseTurnos: 5,
  segundoCartaExtra: 0, segundoArcanoInicial: 0, limiteMao: LIMITE_MAO,
  reporMaoNoInicio: false, conjuradorLevantaNoInicio: false, conjuradorSoAtacaComArma: false,
};

export function criarJogo(opcoes: { catalogo: Catalogo; baralhos: [BaralhoDef, BaralhoDef]; semente: number; regras?: Partial<Regras> }): Estado {
  const e: Estado = {
    catalogo: opcoes.catalogo,
    jogadores: [null, null] as unknown as Estado['jogadores'],
    instancias: {},
    pilha: [],
    turno: 0,
    ativo: 0,
    etapa: 'preparacao',
    prioridade: 0,
    passes: 0,
    pendencia: null,
    combate: null,
    vencedor: null,
    dados: [0, 0],
    regras: { ...REGRAS_PADRAO, ...opcoes.regras },
    primeiro: 0,
    grauNoTurno: false,
    semAcaoTurnos: [0, 0],
    agiuNoTurno: false,
    turnosDano: [[], []],
    habUsos: [novoHab(), novoHab()],
    rng: opcoes.semente >>> 0,
    proxId: 1,
    log: [],
  };
  for (const j of [0, 1] as Jogador[]) {
    const b = opcoes.baralhos[j];
    if (!opcoes.catalogo.conjuradores[b.conjurador]) falha(`Conjurador desconhecido: ${b.conjurador}`);
    const conj = novaInst(e, b.conjurador, j, true);
    const baralho = b.cartas.map((id) => {
      if (!opcoes.catalogo.cartas[id]) falha(`Carta desconhecida: ${id}`);
      return novaInst(e, id, j, false).id;
    });
    const itens = (b.itens ?? []).map((id) => {
      if (opcoes.catalogo.cartas[id]?.tipo !== 'item') falha(`Item desconhecido: ${id}`);
      const inst = novaInst(e, id, j, false);
      inst.revelada = true;
      return inst.id;
    });
    embaralhar(e, baralho);
    const z: ZonasJogador = {
      conjuradorId: conj.id, baralho, mao: [], campo: [], cemiterio: [], grau: [], zonaArcano: [], banimento: [], itens, arcano: 0,
    };
    e.jogadores[j] = z;
  }
  do {
    e.dados = [1 + Math.floor(aleatorio(e) * 20), 1 + Math.floor(aleatorio(e) * 20)];
  } while (e.dados[0] === e.dados[1]);
  const vencedorDado: Jogador = e.dados[0] > e.dados[1] ? 0 : 1;
  anotar(e, `Dados: jogador 0 tirou ${e.dados[0]}, jogador 1 tirou ${e.dados[1]}.`);
  e.pendencia = { tipo: 'escolherInicio', jogador: vencedorDado };
  return e;
}

function iniciarPartida(e: Estado, primeiro: Jogador): void {
  e.ativo = primeiro;
  e.primeiro = primeiro;
  for (const j of [0, 1] as Jogador[]) comprar(e, j, defConjurador(e, j).maoInicial);
  const segundo = outro(primeiro);
  comprar(e, segundo, e.regras.segundoCartaExtra);
  e.jogadores[segundo].arcano += e.regras.segundoArcanoInicial;
  e.turno = 1;
  anotar(e, `Jogador ${primeiro} começa. O jogador ${segundo} compra ${e.regras.segundoCartaExtra} carta(s) a mais e recebe ${e.regras.segundoArcanoInicial} Arcano.`);
  for (const j of [0, 1] as Jogador[]) {
    const its = e.jogadores[j].itens;
    if (its.length) anotar(e, `Jogador ${j} equipa ${its.map((id) => defCarta(e, id).nome).join(', ')} (ataque do Conjurador: ${ataqueConjurador(e, j)}).`);
  }
  entrarEtapa(e, 'inicio');
}

// ---------- etapas ----------

function entrarEtapa(e: Estado, etapa: Etapa): void {
  e.etapa = etapa;
  e.passes = 0;
  e.prioridade = e.ativo;
  anotar(e, `Turno ${e.turno} (jogador ${e.ativo}): ${etapa}.`);
  const z = e.jogadores[e.ativo];
  if (etapa === 'principal' && e.regras.semAcaoPerde && !temAcao(e, e.ativo)) {
    e.vencedor = outro(e.ativo);
    anotar(e, `Jogador ${e.ativo} não tem mais ações e perde. Jogador ${e.vencedor} venceu.`);
    return;
  }
  if (etapa === 'final' && e.regras.impasseTurnos > 0) {
    e.semAcaoTurnos[e.ativo] = e.agiuNoTurno ? 0 : e.semAcaoTurnos[e.ativo] + 1;
    e.agiuNoTurno = false;
    if (e.semAcaoTurnos[e.ativo] >= e.regras.impasseTurnos) {
      e.vencedor = outro(e.ativo);
      anotar(e, `Impasse: o jogador ${e.ativo} ficou ${e.regras.impasseTurnos} turnos sem agir e perde. Jogador ${e.vencedor} venceu.`);
      return;
    }
  }
  if (etapa === 'inicio') {
    e.grauNoTurno = false;
    e.habUsos = [novoHab(), novoHab()];
    if (e.regras.conjuradorLevantaNoInicio && e.instancias[z.conjuradorId].suspensa) {
      e.instancias[z.conjuradorId].suspensa = false;
      anotar(e, `Jogador ${e.ativo} levanta o Conjurador.`);
    }
    if (e.regras.reporMaoNoInicio) {
      const falta = defConjurador(e, e.ativo).maoInicial - z.mao.length;
      if (falta > 0) {
        const antes = z.mao.length;
        comprar(e, e.ativo, falta);
        anotar(e, `Jogador ${e.ativo} repõe a mão: compra ${z.mao.length - antes} carta(s) (fica com ${z.mao.length}).`);
      }
    }
    if (z.mao.length > e.regras.limiteMao) e.pendencia = { tipo: 'descartar', jogador: e.ativo, quantidade: z.mao.length - e.regras.limiteMao };
  } else if (etapa === 'final') {
    if (!e.regras.reporMaoNoInicio) comprar(e, e.ativo, defConjurador(e, e.ativo).compraFinal);
    for (const id of [z.conjuradorId, ...z.campo, ...z.grau]) e.instancias[id].suspensa = false;
    // bônus "neste turno" do construtor acabam no Final
    for (const i of Object.values(e.instancias)) { i.bonusAtaqueTurno = 0; i.bonusVidaTurno = 0; }
    for (const id of z.grau) e.instancias[id].revelada = false;
    z.baralho.push(...z.zonaArcano);
    z.zonaArcano = [];
  }
}

/** Existe alguma ação possível para o jogador (usada pela regra semAcaoPerde)? */
function temAcao(e: Estado, j: Jogador): boolean {
  const z = e.jogadores[j];
  if (z.mao.length > 0 || z.baralho.length > 0) return true;
  const conj = e.instancias[z.conjuradorId];
  const fontes = z.arcano + z.grau.filter((id) => !e.instancias[id].suspensa).length;
  if (!conj.suspensa && fontes >= custoAtaqueConjurador(e, j) && grauEfetivo(e, j) >= grauAtaqueConjurador(e, j)) return true;
  for (const id of z.campo) {
    const d = defCarta(e, id);
    const inst = e.instancias[id];
    if (d.tipo === 'aliado' && !inst.suspensa) return true;
    if (d.ativacao && (!d.ativacao.suspende || !inst.suspensa)) return true;
  }
  return false;
}

function avancarEtapa(e: Estado): void {
  if (e.etapa === 'inicio') entrarEtapa(e, 'principal');
  else if (e.etapa === 'principal') entrarEtapa(e, 'cataclisma');
  else if (e.etapa === 'cataclisma') {
    e.jogadores[e.ativo].arcano = 0;
    anotar(e, `Cataclisma: jogador ${e.ativo} perde o Arcano que sobrou na reserva.`);
    entrarEtapa(e, 'final');
  } else if (e.etapa === 'final') {
    e.ativo = outro(e.ativo);
    e.turno += 1;
    entrarEtapa(e, 'inicio');
  }
}

// ---------- pilha ----------

function resolverTopo(e: Estado): void {
  const item = e.pilha.pop();
  if (!item) return;
  if (item.tipo === 'carta') {
    const d = defCarta(e, item.instId);
    const z = e.jogadores[item.controlador];
    if (d.tipo === 'temporaria' && d.pagarParaAnular !== undefined && podePagar(e, outro(item.controlador), d.pagarParaAnular)) {
      e.pendencia = { tipo: 'pagarAnulacao', jogador: outro(item.controlador), custo: d.pagarParaAnular, item };
      anotar(e, `${d.nome}: o jogador ${outro(item.controlador)} pode pagar ${d.pagarParaAnular} de Arcano para anulá-la.`);
      return;
    }
    if (d.tipo === 'temporaria') {
      anotar(e, `${d.nome} resolve.`);
      // Foco Arcano: o bônus vale para a próxima Temporária de dano ou cura do dono e se gasta
      const efs = efeitosAoJogar(d);
      const usaPotencia = efs.some((ef) => ['dano', 'curar', 'danoTodosAliados'].includes(ef.tipo) || (ef.tipo === 'campo' && (ef.acao === 'dano' || ef.acao === 'curar')));
      const bonus = usaPotencia ? e.habUsos[item.controlador].potencia : 0;
      if (bonus) { e.habUsos[item.controlador].potencia = 0; anotar(e, `Foco Arcano: +${bonus}.`); }
      // a carta já sai da pilha para o cemitério; efeitos que pedem escolha continuam depois
      const pendAntes = e.pendencia;
      fonteIncomensuravel = temIncomensuravel(d);
      try { aplicarEfeitos(e, efs, item.controlador, item.alvo, bonus); } finally { fonteIncomensuravel = false; }
      void pendAntes;
      if (e.instancias[item.instId].banirAoResolver) {
        e.instancias[item.instId].banirAoResolver = false;
        e.instancias[item.instId].banidaDisponivel = 0;
        z.banimento.push(item.instId);
        anotar(e, `${d.nome} é banida ao resolver.`);
      } else z.cemiterio.push(item.instId);
    } else {
      z.campo.push(item.instId);
      e.instancias[item.instId].entrouNoTurno = e.turno;
      anotar(e, `${d.nome} entra em campo.`);
      if (d.tipo === 'aliado') gatilhoConjurador(e, item.controlador, 'aliadoEntra');
      const aoEntrar = efeitosAoEntrar(d);
      if (aoEntrar.length) {
        e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: item.controlador, efeitos: aoEntrar, alvo: item.alvo, fonte: d.nome, incomensuravel: temIncomensuravel(d) });
        anotar(e, `${d.nome}: gatilho ao entrar vai para a pilha.`);
      }
    }
  } else if (item.tipo === 'gatilho') {
    if (item.fonte) anotar(e, `Efeito de ${item.fonte} resolve.`);
    fonteIncomensuravel = !!item.incomensuravel;
    try { aplicarEfeitos(e, item.efeitos, item.controlador, item.alvo, 0, item.alvo === undefined); } finally { fonteIncomensuravel = false; }
  } else if (e.combate && naMesa(e, item.atacante) && naMesa(e, item.alvo)) {
    if (e.instancias[item.alvo].ehConjurador) {
      e.combate.etapa = 'aguardandoBloqueio';
      e.pendencia = { tipo: 'bloqueio', jogador: outro(item.controlador) };
      anotar(e, `Ataque de ${nomeReg(e, item.atacante)} em ${nomeReg(e, item.alvo)}: o jogador ${outro(item.controlador)} escolhe o bloqueio.`);
    } else {
      // Decisão 20: ataque a Aliado, Mística ou Estrutura não pode ser bloqueado; a janela antes do dano continua.
      e.combate.etapa = 'bloqueado';
      e.passes = 0;
      e.prioridade = e.ativo;
      anotar(e, `Ataque de ${nomeReg(e, item.atacante)} em ${nomeReg(e, item.alvo)}: ataque a ${defCarta(e, item.alvo).tipo === 'aliado' ? 'Aliado' : defCarta(e, item.alvo).tipo === 'mistica' ? 'Mística' : 'Estrutura'} não pode ser bloqueado.`);
    }
  } else {
    e.combate = null;
    anotar(e, 'O ataque falhou: atacante ou alvo saiu do campo.');
  }
  acoesDeEstado(e);
}

/** Arcano que o jogador consegue juntar agora: reserva + Grau levantado + pitch de toda a mão. */
function podePagar(e: Estado, j: Jogador, custo: number): boolean {
  const z = e.jogadores[j];
  const grau = z.grau.filter((id) => !e.instancias[id].suspensa).length * ARCANO_POR_GRAU;
  const pitch = z.mao.reduce((s, id) => s + defCarta(e, id).arcanoGerado, 0);
  return z.arcano + grau + pitch >= custo;
}

/** Paga um custo extra usando a reserva, depois o Grau levantado, depois as cartas enviadas (necessárias). Valida antes de mexer. */
function pagarExtra(e: Estado, j: Jogador, custo: number, cartas: string[]): void {
  const z = e.jogadores[j];
  const levantadas = z.grau.filter((id) => !e.instancias[id].suspensa);
  let falta = custo - z.arcano;
  const usarGrau = Math.max(0, Math.min(levantadas.length, Math.ceil(falta / ARCANO_POR_GRAU)));
  falta -= usarGrau * ARCANO_POR_GRAU;
  if (new Set(cartas).size !== cartas.length) falha('Carta repetida no envio ao Arcano.');
  let gerado = 0;
  for (const id of cartas) {
    if (!z.mao.includes(id)) falha('Carta inválida para enviar ao Arcano.');
    if (falta <= 0) falha('Não envie mais cartas ao Arcano: o custo já está coberto.');
    const g = defCarta(e, id).arcanoGerado;
    falta -= g;
    gerado += g;
  }
  if (falta > 0) falha('Arcano insuficiente para pagar.');
  const total = z.arcano + usarGrau * ARCANO_POR_GRAU + gerado;
  for (const id of levantadas.slice(0, usarGrau)) e.instancias[id].suspensa = true;
  for (const id of cartas) {
    tirar(z.mao, id);
    z.zonaArcano.push(id);
  }
  z.arcano = total - custo;
}

function finalizarAnulacao(e: Estado): void {
  e.passes = 0;
  e.prioridade = e.ativo;
  acoesDeEstado(e);
}

/** Vida que ainda falta para derrubar a carta (o Conjurador conta a vida inteira restante). */
function vidaRestante(e: Estado, id: string): number {
  return Math.max(0, (vidaDe(e, id) ?? 0) - e.instancias[id].dano);
}

/** Dano do atacante com a passiva "primeiro dano de combate de um Aliado em cada turno é maior" (marca o uso). */
/** Arma com "dano a mais em unidades": soma quando o golpe do Conjurador acerta Aliados, Místicas ou Estruturas. */
function bonusArmaUnidades(e: Estado, atacante: string, acertaUnidade: boolean): number {
  const at = e.instancias[atacante];
  if (!at?.ehConjurador || !acertaUnidade) return 0;
  const arma = armaDe(e, at.dono);
  const n = arma ? defCarta(e, arma).aoAtacar?.danoUnidades ?? 0 : 0;
  if (n) anotar(e, `${defCarta(e, arma!).nome}: +${n} de dano contra unidades.`);
  return n;
}

function comPrimeiroDanoDeAliado(e: Estado, id: string, dano: number): number {
  const it = e.instancias[id];
  if (!it || it.ehConjurador || defCarta(e, id).tipo !== 'aliado' || dano <= 0) return dano;
  const mais = defConjurador(e, it.dono).poder?.primeiroDanoDeAliadoMais ?? 0;
  if (!mais || e.habUsos[it.dono].danoAliado) return dano;
  e.habUsos[it.dono].danoAliado = true;
  anotar(e, `${defConjurador(e, it.dono).nome}: o primeiro dano de Aliado do turno causa ${mais} a mais.`);
  return dano + mais;
}

/** Bloqueadores que continuam na mesa (os que saíram antes do dano não recebem nem causam dano). */
function bloqueadoresVivos(e: Estado): string[] {
  return e.combate ? e.combate.bloqueadores.filter((id) => naMesa(e, id)) : [];
}

/** Distribuição padrão: derruba o máximo de bloqueadores (os de menos vida primeiro; Conjurador por último). */
export function distribuicaoPadrao(e: Estado, bloqs: string[], dano: number): Record<string, number> {
  const ordem = [...bloqs].sort((a, b) => Number(e.instancias[a].ehConjurador) - Number(e.instancias[b].ehConjurador) || vidaRestante(e, a) - vidaRestante(e, b));
  const d: Record<string, number> = {};
  let resta = dano;
  for (const id of ordem) {
    const v = Math.min(resta, vidaRestante(e, id));
    d[id] = v;
    resta -= v;
  }
  // O que sobra passa como excedente: vai para o primeiro (já derrubado), que repassa ao Conjurador.
  if (resta > 0 && ordem.length) d[ordem[0]] += resta;
  return d;
}

/** Armadura: quando o Conjurador bloqueia e o dono escolhe usá-la, evita até "defesa" de dano e se desgasta (decisão 16). */
function aplicarArmadura(e: Estado, conjId: string, dano: number): number {
  const c = e.combate;
  const dono = e.instancias[conjId].dono;
  if (!c || dano <= 0 || c.usarArmadura === false || e.habUsos[dono].armadura) return dano;
  const arm = armaduraDe(e, dono);
  if (!arm) return dano;
  const da = defCarta(e, arm);
  // Armadura com Grau mínimo ou custo em Arcano (pedido do autor, 04/10): sem cumprir, não protege neste ataque
  if ((da.grau || da.custo) && !cumprirRequisito(e, { grau: da.grau, arcano: da.custo }, dono)) return dano;
  e.habUsos[dono].armadura = true;
  const ia = e.instancias[arm];
  const evita = Math.min(da.defesa ?? 0, dano, Math.max(0, (da.vidaDesgaste ?? 0) - ia.dano));
  if (evita <= 0) return dano;
  ia.dano += evita;
  anotar(e, `${da.nome} (jogador ${dono}) evita ${evita} de dano (desgaste ${ia.dano}/${da.vidaDesgaste}).`);
  // arma do atacante que desgasta a Armadura a mais
  const atk = e.instancias[c.atacante];
  const armaAtk = atk?.ehConjurador ? armaDe(e, atk.dono) : undefined;
  const extra = armaAtk ? defCarta(e, armaAtk).aoAtacar?.desgasteArmadura ?? 0 : 0;
  if (extra > 0) { ia.dano += extra; anotar(e, `${defCarta(e, armaAtk!).nome} desgasta ${extra} a mais (${ia.dano}/${da.vidaDesgaste}).`); }
  // efeitos da Armadura ao proteger
  if (da.aoProteger?.length) { anotar(e, `${da.nome}: efeito ao proteger.`); aplicarEfeitos(e, da.aoProteger, dono); }
  if (da.espinhos && naMesa(e, c.atacante)) causarDano(e, c.atacante, da.espinhos, 'efeito');
  if (ia.dano >= (da.vidaDesgaste ?? 0)) {
    const z = e.jogadores[dono];
    tirar(z.itens, arm);
    ia.dano = 0;
    z.cemiterio.push(arm);
    anotar(e, `${da.nome} (jogador ${dono}) quebrou e foi para o cemitério.`);
  }
  return dano - evita;
}

/** Valida a distribuição escolhida por quem ataca: só bloqueadores vivos, inteiros não negativos, soma = dano. */
function validarDistribuicao(bloqs: string[], dano: number, d: Record<string, number>): void {
  let soma = 0;
  for (const [id, v] of Object.entries(d)) {
    if (!bloqs.includes(id)) falha('Distribuição com uma carta que não está bloqueando.');
    if (!Number.isInteger(v) || v < 0) falha('Distribuição inválida.');
    soma += v;
  }
  if (soma !== dano) falha(`Distribua exatamente ${dano} de dano entre os bloqueadores.`);
}

/** Precisa que quem ataca escolha a distribuição (2 ou mais bloqueadores vivos e dano a distribuir)? */
function precisaDistribuir(e: Estado): boolean {
  const c = e.combate;
  if (!c || c.distribuicao || !naMesa(e, c.atacante)) return false;
  return bloqueadoresVivos(e).length > 1 && ataqueDe(e, c.atacante) > 0;
}

function danoDeCombate(e: Estado): void {
  const c = e.combate;
  if (!c) return;
  const atacanteOk = naMesa(e, c.atacante);
  const atk = e.instancias[c.atacante];
  const conjAlvo = e.jogadores[outro(atk.dono)].conjuradorId;
  const recebido: Record<string, number> = {};
  let danoAoAtacante = 0;
  let resumo: string;
  if (c.bloqueadores.length === 0) {
    // Sem bloqueio: o alvo recebe o dano; Aliado, Mística ou Estrutura atacados revidam (o Conjurador não).
    const alvoOk = naMesa(e, c.alvo);
    const revida = !e.instancias[c.alvo].ehConjurador;
    const dano = atacanteOk && alvoOk ? comPrimeiroDanoDeAliado(e, c.atacante, ataqueDe(e, c.atacante)) + bonusArmaUnidades(e, c.atacante, !e.instancias[c.alvo].ehConjurador) : 0;
    if (alvoOk && revida && incomensuravel(e, c.atacante)) {
      // Incomensurável (decisão 25): o que passar da Vida da unidade atacada vai para o Conjurador dela
      const cabe = Math.min(dano, vidaRestante(e, c.alvo));
      recebido[c.alvo] = cabe;
      if (dano > cabe) { recebido[conjAlvo] = (recebido[conjAlvo] ?? 0) + dano - cabe; anotar(e, `Incomensurável: ${dano - cabe} de dano passa para o Conjurador.`); }
    } else if (alvoOk) recebido[c.alvo] = dano;
    if (alvoOk && revida) danoAoAtacante = comPrimeiroDanoDeAliado(e, c.alvo, ataqueDe(e, c.alvo));
    resumo = `${nomeReg(e, c.atacante)} causa ${dano} a ${nomeReg(e, c.alvo)}${revida && alvoOk ? `, que causa ${danoAoAtacante} de volta` : ''}`;
  } else {
    // Decisão 20: vários bloqueadores. Quem ataca distribui o dano; o que passa da vida de cada bloqueador vai para o Conjurador.
    // Todo bloqueador na mesa causa dano ao atacante ao mesmo tempo, mesmo que morra no combate.
    const bloqs = bloqueadoresVivos(e);
    const dano = atacanteOk && bloqs.length ? comPrimeiroDanoDeAliado(e, c.atacante, ataqueDe(e, c.atacante)) + bonusArmaUnidades(e, c.atacante, bloqs.some((b) => !e.instancias[b].ehConjurador)) : 0;
    const dist = c.distribuicao ?? distribuicaoPadrao(e, bloqs, dano);
    // a passiva pode ter somado dano depois da escolha: a diferença vai para o primeiro da distribuição
    const somaDist = Object.values(dist).reduce((a, b) => a + b, 0);
    if (somaDist !== dano && bloqs.length) dist[bloqs[0]] = (dist[bloqs[0]] ?? 0) + (dano - somaDist);
    let excedente = 0;
    const partes: string[] = [];
    for (const id of bloqs) {
      const v = Math.max(0, dist[id] ?? 0);
      if (e.instancias[id].ehConjurador) { recebido[id] = (recebido[id] ?? 0) + v; partes.push(`${v} em ${nomeReg(e, id)}`); continue; }
      const cabe = Math.min(v, vidaRestante(e, id));
      recebido[id] = (recebido[id] ?? 0) + cabe;
      excedente += v - cabe;
      partes.push(`${v} em ${nomeReg(e, id)}`);
    }
    // decisão 25: o excedente só passa para o Conjurador se o atacante for Incomensurável
    if (!incomensuravel(e, c.atacante)) excedente = 0;
    if (excedente > 0 && naMesa(e, conjAlvo)) recebido[conjAlvo] = (recebido[conjAlvo] ?? 0) + excedente;
    let revide = 0;
    let passivaUsada = false;
    for (const id of bloqs) {
      let v = ataqueDe(e, id, true);
      if (!passivaUsada && v > 0 && !e.instancias[id].ehConjurador) {
        const antes = v;
        v = comPrimeiroDanoDeAliado(e, id, v);
        passivaUsada = v !== antes;
      }
      revide += v;
    }
    danoAoAtacante = revide;
    resumo = `${nomeReg(e, c.atacante)} distribui ${dano} (${partes.join(', ')})${excedente > 0 ? `; Incomensurável: ${excedente} de excedente vai para ${nomeReg(e, conjAlvo)}` : ''}; os bloqueadores causam ${revide} de volta`;
  }
  // Armadura: só quando o Conjurador está entre os bloqueadores (protege o dano recebido, inclusive o excedente).
  if (recebido[conjAlvo] && c.bloqueadores.includes(conjAlvo)) recebido[conjAlvo] = aplicarArmadura(e, conjAlvo, recebido[conjAlvo]);
  e.combate = null;
  for (const [id, v] of Object.entries(recebido)) causarDano(e, id, v, 'combate');
  if (atacanteOk) causarDano(e, c.atacante, danoAoAtacante, 'combate');
  anotar(e, `Combate: ${resumo}.`);
  // Gatilho "ao causar dano de combate ao Conjurador inimigo" (rodada 6): vai para a pilha como um gatilho de "ao entrar".
  if ((recebido[conjAlvo] ?? 0) > 0 && !atk.ehConjurador) {
    gatilhoConjurador(e, atk.dono, 'aliadoDanoConjurador');
    const efs = [...(defCarta(e, c.atacante).aoDanoConjurador ?? []), ...habsDe(defCarta(e, c.atacante), 'danoConjurador').flatMap((x) => x.h.efeitos)];
    if (efs.length) {
      e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: atk.dono, efeitos: efs, fonte: defCarta(e, c.atacante).nome });
      anotar(e, `${defCarta(e, c.atacante).nome}: gatilho de dano de combate ao Conjurador.`);
    }
  }
  acoesDeEstado(e);
}


function todosPassaram(e: Estado): void {
  e.passes = 0;
  e.prioridade = e.ativo;
  if (e.pilha.length > 0) resolverTopo(e);
  else if (e.combate?.etapa === 'bloqueado') {
    // Vários bloqueadores: antes do dano, quem ataca escolhe como distribuir (decisão 20).
    if (precisaDistribuir(e)) {
      e.pendencia = { tipo: 'distribuirDano', jogador: e.instancias[e.combate.atacante].dono };
      anotar(e, `O jogador ${e.pendencia.jogador} distribui o dano de ${nomeReg(e, e.combate.atacante)} entre os bloqueadores.`);
    } else danoDeCombate(e);
  } else avancarEtapa(e);
}

// ---------- comandos ----------

function exigirVivo(e: Estado): void {
  if (e.vencedor !== null) falha('A partida já acabou.');
}

function exigirLivre(e: Estado, j: Jogador): void {
  exigirVivo(e);
  if (e.pendencia) falha('Há uma decisão pendente.');
  if (e.etapa === 'preparacao') falha('A partida ainda não começou.');
  void j;
}

function exigirPrioridade(e: Estado, j: Jogador): void {
  exigirLivre(e, j);
  if (e.prioridade !== j) falha('Você não tem a prioridade.');
}

function exigirNaMao(e: Estado, j: Jogador, id: string): void {
  if (!e.jogadores[j].mao.includes(id)) falha('A carta não está na sua mão.');
}

/**
 * Enviar carta ao Arcano só existe para pagar um custo (como o pitch do Flesh and Blood):
 * cada carta enviada precisa ser necessária, mas a última pode gerar mais do que falta; a sobra fica na reserva.
 * Devolve a reserva final (já antes de descontar o custo).
 */
function validarEnvio(e: Estado, j: Jogador, custo: number, cartas: string[], excluir?: string): number {
  const z = e.jogadores[j];
  if (new Set(cartas).size !== cartas.length) falha('Carta repetida no envio ao Arcano.');
  let reserva = z.arcano;
  for (const id of cartas) {
    if (!z.mao.includes(id) || id === excluir) falha('Carta inválida para enviar ao Arcano.');
    if (reserva >= custo) falha('Não envie mais cartas ao Arcano: o custo já está coberto.');
    reserva += defCarta(e, id).arcanoGerado;
  }
  if (reserva < custo) falha(`Arcano insuficiente (${reserva}/${custo}).`);
  return reserva;
}

function pagarCusto(e: Estado, j: Jogador, custo: number, cartas: string[], reserva: number): void {
  const z = e.jogadores[j];
  for (const id of cartas) {
    tirar(z.mao, id);
    z.zonaArcano.push(id);
  }
  z.arcano = reserva - custo;
  if (cartas.length) anotar(e, `Jogador ${j} envia ao Arcano: ${cartas.map((id) => `${defCarta(e, id).nome} (+${defCarta(e, id).arcanoGerado})`).join(', ')}.`);
}

/** Protegido N (rodada 6): mirar uma carta do oponente com Protegido custa N de Arcano a mais. */
export function custoProtecao(e: Estado, j: Jogador, alvo: string | undefined): number {
  if (alvo === undefined) return 0;
  const inst = e.instancias[alvo];
  if (!inst || inst.ehConjurador || inst.dono === j || !e.jogadores[inst.dono].campo.includes(alvo)) return 0;
  const d = defCarta(e, alvo);
  const doConj = d.tipo === 'aliado' ? (defConjurador(e, inst.dono).poder?.aliadosProtegidos ?? 0) : 0;
  return Math.max(d.protegido ?? 0, doConj) + (d.protecao?.arcano ?? 0);
}

/** Proteção mágica do alvo (04/10): confere e escolhe o que pagar além do Arcano (vida, descarte, Grau ao cemitério, Grau mínimo). */
function protecaoAPagar(e: Estado, j: Jogador, alvo: string | undefined, c: { carta: string; enviarArcano?: string[]; protecaoDescartar?: string[]; protecaoGrau?: string[]; descartarCusto?: string[] }): { vida: number; descartar: string[]; grau: string[] } {
  const nada = { vida: 0, descartar: [] as string[], grau: [] as string[] };
  if (alvo === undefined) return nada;
  const inst = e.instancias[alvo];
  if (!inst || inst.ehConjurador || inst.dono === j || !e.jogadores[inst.dono].campo.includes(alvo)) return nada;
  const pr = defCarta(e, alvo).protecao;
  if (!pr) return nada;
  const z = e.jogadores[j];
  if (pr.grauMin && grauEfetivo(e, j) < pr.grauMin) falha(`Proteção mágica: para mirar esta carta precisa de Grau ${pr.grauMin}.`);
  const vida = pr.vida ?? 0;
  if (vida && (vidaDe(e, z.conjuradorId) ?? 0) - e.instancias[z.conjuradorId].dano <= vida) falha('Proteção mágica: o seu Conjurador não aguenta pagar essa Vida.');
  const ocupadas = new Set([c.carta, ...(c.enviarArcano ?? []), ...(c.descartarCusto ?? [])]);
  const nD = pr.descartar ?? 0;
  const descartar = c.protecaoDescartar ?? (nD ? z.mao.filter((id) => !ocupadas.has(id)).slice(-nD) : []);
  if (descartar.length !== nD || descartar.some((id) => !z.mao.includes(id) || ocupadas.has(id))) falha(`Proteção mágica: descarte ${nD} carta(s) da mão.`);
  const nG = pr.grauParaCemiterio ?? 0;
  const grau = c.protecaoGrau ?? z.grau.slice(0, nG);
  if (grau.length !== nG || grau.some((id) => !z.grau.includes(id))) falha(`Proteção mágica: mande ${nG} carta(s) do seu Grau ao cemitério.`);
  return { vida, descartar, grau };
}
function pagarProtecao(e: Estado, j: Jogador, pg: { vida: number; descartar: string[]; grau: string[] }): void {
  const z = e.jogadores[j];
  if (pg.vida) { e.instancias[z.conjuradorId].dano += pg.vida; anotar(e, `Proteção mágica: jogador ${j} paga ${pg.vida} de Vida.`); }
  for (const id of pg.descartar) { tirar(z.mao, id); z.cemiterio.push(id); }
  for (const id of pg.grau) { tirar(z.grau, id); e.instancias[id].suspensa = false; z.cemiterio.push(id); }
  if (pg.descartar.length || pg.grau.length) anotar(e, `Proteção mágica: jogador ${j} paga com ${pg.descartar.length} carta(s) da mão e ${pg.grau.length} do Grau.`);
}

function janelaFindante(e: Estado, j: Jogador): boolean {
  return j === e.ativo && e.etapa === 'principal' && e.pilha.length === 0 && e.combate === null;
}

function jogarCarta(e: Estado, c: Extract<Comando, { tipo: 'jogar' }>): void {
  exigirPrioridade(e, c.jogador);
  const origem = origemDaCarta(e, c.jogador, c.carta);
  if (!origem) falha('A carta não está na sua mão (nem pode ser jogada de outra zona agora).');
  const d = defCarta(e, c.carta);
  const z = e.jogadores[c.jogador];
  // Aliado só entra fora da fase principal com a palavra-chave Repentino (decisão 21).
  if (!ehRepentina(d) && !janelaFindante(e, c.jogador))
    falha(d.tipo === 'aliado' ? 'Aliado sem Repentino: só na sua fase principal, com a pilha vazia.' : 'Carta findante: só na sua fase principal, com a pilha vazia.');
  if (grauEfetivo(e, c.jogador) < d.grau) falha(`Precisa de Grau ${d.grau} (você tem ${grauEfetivo(e, c.jogador)}).`);
  for (const v of d.custoValores ?? []) {
    if (!z.grau.some((id) => defCarta(e, id).valores.includes(v))) falha(`Precisa de uma carta de ${v} no Grau.`);
  }
  const custo = custoEfetivo(e, c.jogador, c.carta) + custoProtecao(e, c.jogador, c.alvo);
  const reserva = validarEnvio(e, c.jogador, custo, c.enviarArcano ?? [], c.carta);
  const descarte = c.descartarGrau ?? [];
  if (descarte.length !== (d.descartaGrau ?? 0)) falha(`Descarte exatamente ${d.descartaGrau ?? 0} carta(s) do Grau.`);
  if (new Set(descarte).size !== descarte.length || !descarte.every((id) => z.grau.includes(id)))
    falha('Descarte inválido do Grau.');
  const efsAlvo = [...efeitosAoJogar(d), ...(d.tipo === 'temporaria' ? [] : efeitosAoEntrar(d))];
  const precisaAlvo = exigeAlvo(efsAlvo);
  if (precisaAlvo && !alvoServe(e, efsAlvo, c.alvo, c.jogador)) falha('Esta carta precisa de um alvo legal.');
  const prot = protecaoAPagar(e, c.jogador, precisaAlvo ? c.alvo : undefined, c);
  pagarCusto(e, c.jogador, custo, c.enviarArcano ?? [], reserva);
  pagarProtecao(e, c.jogador, prot);
  e.habUsos[c.jogador].carta = true;
  e.habUsos[c.jogador].desconto = 0;
  if (ehRepentina(d)) e.habUsos[c.jogador].repentina = true;
  for (const id of descarte) {
    tirar(z.grau, id);
    z.cemiterio.push(id);
  }
  if (origem === 'mao') tirar(z.mao, c.carta);
  else if (origem === 'banimento') { tirar(z.banimento, c.carta); e.instancias[c.carta].banidaDisponivel = 0; e.instancias[c.carta].descontoBanimento = 0; }
  else { tirar(z.cemiterio, c.carta); e.instancias[c.carta].banirAoResolver = true; e.habUsos[c.jogador].anularDoCemiterio = false; }
  e.agiuNoTurno = true;
  e.pilha.push({ tipo: 'carta', id: `p${e.proxId++}`, controlador: c.jogador, instId: c.carta, alvo: c.alvo });
  e.passes = 0;
  e.prioridade = c.jogador;
  anotar(e, `Jogador ${c.jogador} joga ${d.nome}.`);
  if (precisaAlvo && c.alvo) disparar(e, c.alvo, 'alvo');
}

function ativar(e: Estado, c: Extract<Comando, { tipo: 'ativar' }>): void {
  exigirLivre(e, c.jogador);
  const z = e.jogadores[c.jogador];
  const ehConj = c.carta === z.conjuradorId;
  const ehItem = !ehConj && z.itens.includes(c.carta);
  if (!ehConj && !ehItem && !z.campo.includes(c.carta)) falha('A carta não está no seu campo.');
  if (ehItem && !defCarta(e, c.carta).ativacao && c.habilidade === undefined) falha('Este item não tem uso.');
  if (ehItem && grauEfetivo(e, c.jogador) < defCarta(e, c.carta).grau) falha(`Para usar ${defCarta(e, c.carta).nome}, precisa de Grau ${defCarta(e, c.carta).grau}.`);
  let at = ehConj ? defConjurador(e, c.jogador).poder?.ativacao : defCarta(e, c.carta).ativacao;
  if (c.habilidade !== undefined) {
    // habilidade "virar/pagar" montada no construtor (04/10)
    const h = ehConj ? undefined : defCarta(e, c.carta).habilidades?.[c.habilidade];
    if (!h || h.quando !== 'ativada') falha('Esta carta não tem essa habilidade.');
    at = { custo: h.custo?.arcano ?? 0, suspende: !!h.custo?.suspender, efeitos: h.efeitos, descartar: h.custo?.descartar, danoNoConjurador: h.custo?.vida, ...(h.custo?.grau ? { requisito: { grau: h.custo.grau } } : {}), consome: false };
  }
  if (!at) falha(ehConj ? 'Este Conjurador não tem habilidade ativada.' : 'Esta carta não tem ativação.');
  const inst = e.instancias[c.carta];
  if (at.suspende && inst.suspensa) falha('A carta está suspensa.');
  if (at.suspende && recemChegado(e, c.carta)) falha('Este Aliado acabou de entrar e não é Proficiente.');
  if (ehConj && !at.repentina && !janelaFindante(e, c.jogador)) falha('Habilidade do Conjurador: só na sua fase principal, com a pilha vazia (ou se ela for Repentina).');
  if (at.requisito?.grau && grauEfetivo(e, c.jogador) < at.requisito.grau) falha(`Precisa de Grau ${at.requisito.grau} para usar.`);
  const imediatos = ['arcano', 'descontoProximaCarta', 'permitirAnularDoCemiterio', 'banirDaMao', 'potenciaMagia', 'ataqueConjuradorTurno'];
  const soArcano = at.efeitos.every((ef) => imediatos.includes(ef.tipo));
  if (!soArcano || ehConj) exigirPrioridade(e, c.jogador);
  if (exigeAlvo(at.efeitos) && !alvoServe(e, at.efeitos, c.alvo, c.jogador)) falha('Esta ativação precisa de um alvo legal.');
  const rean = at.efeitos.find((ef) => ef.tipo === 'reanimar');
  const extraRean = rean && rean.tipo === 'reanimar' && c.alvo ? rean.custoExtra + defCarta(e, c.alvo).custo : 0;
  const custo = at.custo + (at.requisito?.arcano ?? 0) + extraRean + custoProtecao(e, c.jogador, c.alvo);
  const conjInst = e.instancias[z.conjuradorId];
  if (at.suspendeConjurador && conjInst.suspensa) falha('O Conjurador está suspenso.');
  const descarte = c.descartarCusto ?? [];
  if ((at.descartar ?? 0) !== descarte.length) falha(`Descarte ${at.descartar ?? 0} carta(s) da mão para usar.`);
  if (new Set(descarte).size !== descarte.length || descarte.some((id) => !z.mao.includes(id) || (c.enviarArcano ?? []).includes(id))) falha('Descarte cartas da sua mão (sem repetir as enviadas ao Arcano).');
  if (at.danoNoConjurador && (vidaDe(e, z.conjuradorId) ?? 0) - conjInst.dano <= at.danoNoConjurador) falha('O Conjurador não aguenta esse dano.');
  const reserva = validarEnvio(e, c.jogador, custo, c.enviarArcano ?? []);
  const protA = protecaoAPagar(e, c.jogador, exigeAlvo(at.efeitos) ? c.alvo : undefined, c);
  pagarCusto(e, c.jogador, custo, c.enviarArcano ?? [], reserva);
  pagarProtecao(e, c.jogador, protA);
  for (const id of descarte) { tirar(z.mao, id); z.cemiterio.push(id); }
  if (descarte.length) anotar(e, `Jogador ${c.jogador} descarta ${descarte.length} carta(s) para usar ${defCarta(e, c.carta).nome}.`);
  if (at.danoNoConjurador) { conjInst.dano += at.danoNoConjurador; anotar(e, `${defConjurador(e, c.jogador).nome} toma ${at.danoNoConjurador} de dano para usar ${defCarta(e, c.carta).nome}.`); }
  if (at.suspende) inst.suspensa = true;
  if (at.suspendeConjurador) conjInst.suspensa = true;
  if (ehItem && (at.consome ?? defCarta(e, c.carta).categoriaItem === 'extra')) { tirar(z.itens, c.carta); z.cemiterio.push(c.carta); anotar(e, `${defCarta(e, c.carta).nome} é consumido.`); }
  e.agiuNoTurno = true;
  if (soArcano) {
    if (ehConj) anotar(e, `Jogador ${c.jogador} ativa ${defConjurador(e, c.jogador).nome}.`);
    aplicarEfeitos(e, at.efeitos, c.jogador, c.alvo);
    return;
  }
  e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: c.jogador, efeitos: at.efeitos, alvo: c.alvo, fonte: ehConj ? defConjurador(e, c.jogador).nome : defCarta(e, c.carta).nome, incomensuravel: !ehConj && temIncomensuravel(defCarta(e, c.carta)) });
  e.passes = 0;
  e.prioridade = c.jogador;
  anotar(e, `Jogador ${c.jogador} ativa ${ehConj ? defConjurador(e, c.jogador).nome : defCarta(e, c.carta).nome}.`);
  if (exigeAlvo(at.efeitos) && c.alvo) disparar(e, c.alvo, 'alvo');
}

/** Aliado que entrou neste turno e não é Proficiente: não ataca nem usa ativação que suspende. */
export function recemChegado(e: Estado, id: string): boolean {
  const inst = e.instancias[id];
  if (inst.ehConjurador || inst.entrouNoTurno !== e.turno) return false;
  const d = defCarta(e, id);
  return d.tipo === 'aliado' && !d.palavrasChave?.includes('Proficiente');
}

function atacar(e: Estado, c: Extract<Comando, { tipo: 'atacar' }>): void {
  exigirPrioridade(e, c.jogador);
  if (!janelaFindante(e, c.jogador)) falha('Só se ataca na sua fase principal, com a pilha vazia.');
  const z = e.jogadores[c.jogador];
  const adv = e.jogadores[outro(c.jogador)];
  if (!(z.conjuradorId === c.atacante || z.campo.includes(c.atacante))) falha('Atacante inválido.');
  const atacante = e.instancias[c.atacante];
  if (atacante.suspensa) falha('O atacante está suspenso.');
  if (atacante.ehConjurador && e.regras.primeiroConjSemAtacarTurno1 && c.jogador === e.primeiro && e.turno === 1) falha('O Conjurador de quem começa não ataca no primeiro turno.');
  if (!atacante.ehConjurador) {
    const da = defCarta(e, c.atacante);
    if (da.tipo !== 'aliado') falha('Só Aliados e o Conjurador atacam.');
    if (recemChegado(e, c.atacante)) falha('Este Aliado acabou de entrar e não é Proficiente.');
  }
  const alvoOk =
    c.alvo === adv.conjuradorId ||
    (adv.campo.includes(c.alvo) && ['aliado', 'mistica', 'estrutura'].includes(defCarta(e, c.alvo).tipo) && (vidaDe(e, c.alvo) ?? 0) > 0);
  if (!alvoOk) falha('Alvo de ataque inválido.');
  if (!e.instancias[c.alvo].ehConjurador && etereo(e, c.alvo) && !etereo(e, c.atacante)) falha('Só Etéreos (ou um Conjurador com arma Etérea) atingem um Etéreo.');
  if (atacante.naoAtacaAte && e.turno <= atacante.naoAtacaAte) falha('Esta carta não pode atacar agora.');
  if (atacante.ehConjurador && e.regras.conjuradorSoAtacaComArma && ataqueConjurador(e, c.jogador) <= 0) falha('O Conjurador só ataca com uma arma (item no Canal).');
  if (atacante.ehConjurador && grauEfetivo(e, c.jogador) < grauAtaqueConjurador(e, c.jogador)) falha(`Para atacar com esta arma, precisa de Grau ${grauAtaqueConjurador(e, c.jogador)}.`);
  if (atacante.ehConjurador) {
    const custo = custoAtaqueConjurador(e, c.jogador);
    const reserva = validarEnvio(e, c.jogador, custo, c.enviarArcano ?? []);
    pagarCusto(e, c.jogador, custo, c.enviarArcano ?? [], reserva);
  } else if (c.enviarArcano?.length) falha('Este ataque não custa Arcano.');
  atacante.suspensa = true;
  e.agiuNoTurno = true;
  e.combate = { atacante: c.atacante, alvo: c.alvo, bloqueador: null, bloqueadores: [], etapa: 'declarado' };
  e.pilha.push({ tipo: 'ataque', id: `p${e.proxId++}`, controlador: c.jogador, atacante: c.atacante, alvo: c.alvo });
  e.passes = 0;
  e.prioridade = c.jogador;
  anotar(e, `Jogador ${c.jogador} ataca com ${nomeReg(e, c.atacante)} (ataque ${ataqueDe(e, c.atacante)}) → ${nomeReg(e, c.alvo)}.`);
  if (!atacante.ehConjurador) disparar(e, c.atacante, 'atacar', c.alvo);
  // Arma: efeitos ao atacar (ganhar Arcano, comprar...), na hora em que o ataque é declarado
  const arma = atacante.ehConjurador ? armaDe(e, c.jogador) : undefined;
  const efs = arma ? defCarta(e, arma).aoAtacar?.efeitos : undefined;
  if (arma && efs?.length) { anotar(e, `${defCarta(e, arma).nome}: efeito ao atacar.`); aplicarEfeitos(e, efs, c.jogador); }
}

export function executar(e: Estado, c: Comando): void {
  const antes = e.jogadores.map((z) => ({ cem: new Set(z.cemiterio), ban: new Set(z.banimento) }));
  executarComando(e, c);
  // construtor (04/10): cartas que caíram no cemitério ou no banimento disparam as habilidades delas
  if (e.vencedor !== null) return;
  let disparou = false;
  e.jogadores.forEach((z, j) => {
    for (const id of z.cemiterio) if (!antes[j].cem.has(id) && habsDe(e.catalogo.cartas[e.instancias[id]?.defId] ?? ({} as CartaDef), 'cemiterio').length) { disparar(e, id, 'cemiterio'); disparou = true; }
    for (const id of z.banimento) if (!antes[j].ban.has(id) && habsDe(e.catalogo.cartas[e.instancias[id]?.defId] ?? ({} as CartaDef), 'banimento').length) { disparar(e, id, 'banimento'); disparou = true; }
  });
  if (disparou && !e.pendencia) { e.passes = 0; e.prioridade = e.ativo; }
}

function executarComando(e: Estado, c: Comando): void {
  exigirVivo(e);
  if (e.pendencia) {
    const p = e.pendencia;
    if (c.tipo === 'escolherInicio' && p.tipo === 'escolherInicio') {
      if (c.jogador !== p.jogador) falha('Quem ganhou no dado escolhe.');
      e.pendencia = null;
      iniciarPartida(e, c.primeiro);
      return;
    }
    if (c.tipo === 'descartar' && p.tipo === 'descartar') {
      if (c.jogador !== p.jogador) falha('Outro jogador precisa descartar.');
      const z = e.jogadores[c.jogador];
      if (c.cartas.length !== p.quantidade || new Set(c.cartas).size !== c.cartas.length || !c.cartas.every((id) => z.mao.includes(id)))
        falha(`Descarte exatamente ${p.quantidade} carta(s) da mão.`);
      for (const id of c.cartas) {
        tirar(z.mao, id);
        z.cemiterio.push(id);
      }
      e.pendencia = null;
      return;
    }
    if (c.tipo === 'pagarAnulacao' && p.tipo === 'pagarAnulacao') {
      if (c.jogador !== p.jogador) falha('Outro jogador decide a anulação.');
      const item = p.item;
      if (item.tipo !== 'carta') falha('Item inválido.');
      const z = e.jogadores[item.controlador];
      e.pendencia = null;
      if (c.pagar) {
        pagarExtra(e, c.jogador, p.custo, c.enviarArcano ?? []);
        z.cemiterio.push(item.instId);
        anotar(e, `Jogador ${c.jogador} paga ${p.custo} e anula ${defCarta(e, item.instId).nome}.`);
      } else {
        anotar(e, `${defCarta(e, item.instId).nome} resolve.`);
        aplicarEfeitos(e, efeitosAoJogar(defCarta(e, item.instId)), item.controlador, item.alvo);
        z.cemiterio.push(item.instId);
      }
      finalizarAnulacao(e);
      return;
    }
    if (c.tipo === 'escolherCartas' && p.tipo === 'escolherCartas') {
      if (c.jogador !== p.jogador) falha('Outro jogador escolhe.');
      if (c.cartas.length !== p.quantidade || new Set(c.cartas).size !== c.cartas.length || !c.cartas.every((id) => p.cartas.includes(id)))
        falha(`Escolha exatamente ${p.quantidade} carta(s).`);
      e.pendencia = null;
      for (const id of c.cartas) moverPara(e, id, p.destino, c.jogador);
      const resto = p.cartas.filter((id) => !c.cartas.includes(id));
      if (p.resto) for (const id of resto) moverPara(e, id, p.resto, c.jogador);
      if (c.cartas.length) anotar(e, `Jogador ${c.jogador}: ${c.cartas.length} carta(s) ${DESTINO_TXT[p.destino]}${p.resto && resto.length ? `, ${resto.length} ${DESTINO_TXT[p.resto]}` : ''}.`);
      if (p.depois) aplicarEfeitos(e, p.depois.efeitos, p.depois.dono, p.depois.alvo);
      if (!e.pendencia) finalizarAnulacao(e);
      return;
    }
    if (c.tipo === 'escolherDoTopo' && p.tipo === 'escolherDoTopo') {
      if (c.jogador !== p.jogador) falha('Outro jogador escolhe.');
      if (c.cartas.length !== p.pegar || new Set(c.cartas).size !== c.cartas.length || !c.cartas.every((id) => p.cartas.includes(id)))
        falha(`Escolha exatamente ${p.pegar} carta(s) entre as que você olhou.`);
      const z = e.jogadores[c.jogador];
      z.mao.push(...c.cartas);
      const resto = p.cartas.filter((id) => !c.cartas.includes(id));
      z.baralho.push(...resto);
      e.pendencia = null;
      anotar(e, `Jogador ${c.jogador} põe ${c.cartas.length} carta(s) na mão e ${resto.length} no fundo do baralho.`);
      finalizarAnulacao(e);
      return;
    }
    if (c.tipo === 'bloquear' && p.tipo === 'bloqueio') {
      if (c.jogador !== p.jogador || !e.combate) falha('Bloqueio inválido.');
      const z = e.jogadores[c.jogador];
      const lista = c.bloqueadores ?? (c.bloqueador ? [c.bloqueador] : []);
      if (new Set(lista).size !== lista.length) falha('Bloqueador repetido.');
      if (lista.length && !e.instancias[e.combate.alvo].ehConjurador) falha('Só se bloqueia ataque ao Conjurador.');
      const atacanteEtereo = etereo(e, e.combate.atacante);
      for (const b of lista) {
        const ok = z.conjuradorId === b || (z.campo.includes(b) && defCarta(e, b).tipo === 'aliado');
        if (!ok) falha('Bloqueador inválido.');
        if (e.instancias[b].suspensa) falha('O bloqueador está suspenso.');
        if (atacanteEtereo && !etereo(e, b)) falha('Só Etéreos (ou um Conjurador com arma Etérea) bloqueiam um atacante Etéreo.');
      }
      for (const b of lista) e.instancias[b].suspensa = true;
      for (const b of lista) disparar(e, b, 'bloquear', e.combate.atacante);
      if (lista.length) {
        const semArmadura = c.usarArmadura === false && lista.includes(z.conjuradorId) && armaduraDe(e, c.jogador);
        anotar(e, `Jogador ${c.jogador} bloqueia com ${lista.map((b) => nomeReg(e, b)).join(' e ')}${semArmadura ? ' sem usar a Armadura' : ''}.`);
      } else anotar(e, `Jogador ${c.jogador} não bloqueia.`);
      e.combate.bloqueadores = lista;
      e.combate.bloqueador = lista[0] ?? null;
      e.combate.usarArmadura = c.usarArmadura !== false;
      e.combate.etapa = 'bloqueado';
      e.pendencia = null;
      e.passes = 0;
      e.prioridade = e.ativo;
      return;
    }
    if (c.tipo === 'distribuirDano' && p.tipo === 'distribuirDano') {
      if (c.jogador !== p.jogador || !e.combate) falha('Distribuição inválida.');
      const bloqs = bloqueadoresVivos(e);
      const dano = ataqueDe(e, e.combate.atacante);
      const d = c.distribuicao ?? distribuicaoPadrao(e, bloqs, dano);
      validarDistribuicao(bloqs, dano, d);
      e.combate.distribuicao = { ...d };
      e.pendencia = null;
      danoDeCombate(e);
      return;
    }
    falha('Há uma decisão pendente.');
  }
  switch (c.tipo) {
    case 'colocarGrau': {
      exigirLivre(e, c.jogador);
      if (!janelaFindante(e, c.jogador)) falha('O Grau só se coloca na sua fase principal, com a pilha vazia.');
      exigirNaMao(e, c.jogador, c.carta);
      const z = e.jogadores[c.jogador];
      const conj = e.instancias[z.conjuradorId];
      if (e.regras.primeiroSemGrauNoTurno1 && c.jogador === e.primeiro && e.turno === 1) falha('Quem começa não pode pôr Grau no primeiro turno.');
      if (e.regras.grauSemSuspender) {
        if (e.grauNoTurno) falha('Você já pôs Grau neste turno.');
      } else if (conj.suspensa) falha('O Conjurador está suspenso.');
      if (z.grau.length >= GRAU_MAXIMO) falha('Zona de Grau cheia.');
      if (e.regras.grauSemSuspender) e.grauNoTurno = true;
      else conj.suspensa = true;
      tirar(z.mao, c.carta);
      z.grau.push(c.carta);
      e.instancias[c.carta].revelada = true;
      anotar(e, `Jogador ${c.jogador} põe ${defCarta(e, c.carta).nome} no Grau.`);
      disparar(e, c.carta, 'grau');
      if (e.pilha.length) { e.passes = 0; e.prioridade = c.jogador; }
      return;
    }
    case 'suspenderGrau': {
      exigirLivre(e, c.jogador);
      const z = e.jogadores[c.jogador];
      if (!z.grau.includes(c.carta)) falha('A carta não está no seu Grau.');
      const inst = e.instancias[c.carta];
      if (inst.suspensa) falha('A carta já está suspensa.');
      inst.suspensa = true;
      z.arcano += ARCANO_POR_GRAU;
      anotar(e, `Jogador ${c.jogador} suspende ${defCarta(e, c.carta).nome} do Grau: +${ARCANO_POR_GRAU} Arcano (reserva ${z.arcano}).`);
      return;
    }
    case 'jogar':
      jogarCarta(e, c);
      return;
    case 'ativar':
      ativar(e, c);
      return;
    case 'atacar':
      atacar(e, c);
      return;
    case 'passar': {
      exigirPrioridade(e, c.jogador);
      e.passes += 1;
      if (e.passes >= 2) todosPassaram(e);
      else e.prioridade = outro(c.jogador);
      return;
    }
    default:
      falha('Comando inválido agora.');
  }
}
