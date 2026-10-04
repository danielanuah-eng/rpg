// Gerado por online/construir.ts a partir de tcg/src/motor.ts. Não edite: edite o .ts e reconstrua.
// Motor de regras do Arcanoclisma TCG (fase 1). Muta o estado e lança ErroRegra se o comando for ilegal.
             
                                                                                                                                                                                                                                                                                   
                    

export const ARCANO_POR_GRAU = 1;
export const GRAU_MAXIMO = 12;
export const LIMITE_MAO = 7;

export class ErroRegra extends Error {}

const outro = (j         )          => (j === 0 ? 1 : 0);

function falha(msg        )        {
  throw new ErroRegra(msg);
}

function aleatorio(e        )         {
  e.rng = (e.rng + 0x6d2b79f5) >>> 0;
  let t = e.rng;
  t = Math.imul(t ^ (t >>> 15), t | 1);
  t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
  return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
}

function embaralhar   (e        , v     )       {
  for (let i = v.length - 1; i > 0; i--) {
    const j = Math.floor(aleatorio(e) * (i + 1));
    [v[i], v[j]] = [v[j], v[i]];
  }
}

function anotar(e        , msg        )       {
  e.log.push(msg);
}

const novoHab = ()          => ({ gatilho: false, carta: false, repentina: false, desconto: 0, anularDoCemiterio: false, danoAliado: false, armadura: false, potencia: 0, ataqueExtra: 0 });

function novaInst(e        , defId        , dono         , ehConjurador         )       {
  const inst       = { id: `i${e.proxId++}`, defId, ehConjurador, dono, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.instancias[inst.id] = inst;
  return inst;
}

// ---------- consultas ----------

/**
 * A carta pode ser jogada a qualquer momento com prioridade? Temporárias, Místicas e Estruturas: velocidade Repentina.
 * Aliado nunca é Repentina pelo tipo (decisão 21); só entra fora da fase principal com a palavra-chave Repentino no texto.
 */
export function ehRepentina(d          )          {
  return d.tipo === 'aliado' ? !!d.palavrasChave?.includes('Repentino') : d.velocidade === 'repentina';
}

export function defCarta(e        , instId        )           {
  const inst = e.instancias[instId];
  if (!inst || inst.ehConjurador) falha(`Não é uma carta: ${instId}`);
  return e.catalogo.cartas[inst.defId];
}

export function defConjurador(e        , j         )                {
  return e.catalogo.conjuradores[e.instancias[e.jogadores[j].conjuradorId].defId];
}

function vidaDe(e        , instId        )                     {
  const inst = e.instancias[instId];
  const base = inst.ehConjurador ? defConjurador(e, inst.dono).vida : defCarta(e, instId).vida;
  return base === undefined ? undefined : base + (inst.bonusVida ?? 0) + (inst.bonusVidaTurno ?? 0);
}

/** Vida atual (com bônus do construtor de efeitos), para a interface. */
export function vidaAtualDe(e        , instId        )                     {
  return e.instancias[instId] ? vidaDe(e, instId) : undefined;
}
/** Ataque atual (com bônus), para a interface. */
export function ataqueAtualDe(e        , instId        )         {
  return e.instancias[instId] ? ataqueDe(e, instId) : 0;
}

/** A carta tem a palavra-chave (da própria carta ou concedida)? Conjuradores só têm as concedidas. */
export function temPalavra(e        , instId        , palavra        )          {
  const i = e.instancias[instId];
  if (!i) return false;
  if (palavrasConcedidas(i).includes(palavra)) return true;
  return !i.ehConjurador && !!defCarta(e, instId).palavrasChave?.includes(palavra);
}
/** Palavras-chave concedidas: permanentes e as que duram alguns turnos. */
export function palavrasConcedidas(i      )           {
  return [...(i.palavrasExtra ?? []), ...(i.palavrasTemp ?? []).flatMap((t) => t.palavras)];
}
/** Protessência (decisão 30): a carta não pode ser alvo de efeitos de quem não é o dono dela (nem anulada por ele na pilha). */
export const PROTESSENCIA = 'Protessência';
/** A carta está protegida dos alvos deste controlador (tem Protessência, própria ou concedida, e o efeito é do oponente)? */
export function protegidaDe(e        , instId                    , controlador         )          {
  const i = instId === undefined ? undefined : e.instancias[instId];
  return !!i && i.dono !== controlador && temPalavra(e, instId , PROTESSENCIA);
}
/**
 * Protegido da carta (unificado em 04/10, noite): número N = { arcano: N }; o nome antigo `protecao` ("Proteção mágica")
 * ainda é lido e somado (o Arcano dos dois se soma, como antes). null = sem Protegido.
 */
export function protegidoDe(d                                                      )                   {
  if (!d) return null;
  const p = d.protegido;
  const r            = typeof p === 'number' ? (p ? { arcano: p } : {}) : { ...(p ?? {}) };
  for (const [k, v] of Object.entries(d.protecao ?? {})                                                ) {
    if (!v) continue;
    r[k] = k === 'grauMin' ? Math.max(r[k] ?? 0, v) : (r[k] ?? 0) + v;
  }
  for (const k of Object.keys(r)                          ) if (!r[k]) delete r[k];
  return Object.keys(r).length ? r : null;
}
/** Converte o nome antigo: `protecao` vira parte de `protegido` (número quando só tem Arcano). Devolve a mesma carta se não há o que converter. */
export function normalizarProtegido                                                    (d   )    {
  if (!d || d.protecao === undefined) return d;
  const pr = protegidoDe(d);
  const { protecao: _velho, ...resto } = d;
  const r = { ...resto }     ;
  if (!pr) delete r.protegido;
  else r.protegido = Object.keys(pr).length === 1 && pr.arcano ? pr.arcano : pr;
  return r;
}
/** Protegidos concedidos ("Protegido 2"), cada um por si (fixos e por turnos). */
function protegidosConcedidos(i      )           {
  return palavrasConcedidas(i).map((p) => { const m = /^Protegido\s*(\d+)$/i.exec(p.trim()); return m ? Number(m[1]) : 0; }).filter((n) => n > 0);
}

/** Incomensurável (decisão 25): Aliado com a palavra-chave, ou Conjurador com arma Incomensurável. */
export function incomensuravel(e        , instId        )          {
  const i = e.instancias[instId];
  if (!i) return false;
  if (palavrasConcedidas(i).includes('Incomensurável')) return true;
  if (i.ehConjurador) { const a = armaDe(e, i.dono); return !!a && !!defCarta(e, a).palavrasChave?.includes('Incomensurável'); }
  return !!defCarta(e, instId).palavrasChave?.includes('Incomensurável');
}
const temIncomensuravel = (d                      ) => !!d?.palavrasChave?.includes('Incomensurável');
/** Efeitos resolvendo agora vêm de uma fonte Incomensurável? (o dano que passar da Vida da carta vai para o Conjurador dela) */
let fonteIncomensuravel = false;

/**
 * Etéreo (decisão 24): só Etéreos atingem Etéreos em combate. Aliado: a palavra-chave. Conjurador: ao ATACAR vale a arma
 * Etérea; ao BLOQUEAR vale a Armadura Etérea (pedido do autor, 04/10).
 */
export function etereo(e        , instId        , papel                        = 'ataque')          {
  const i = e.instancias[instId];
  if (!i) return false;
  if (palavrasConcedidas(i).includes('Etéreo')) return true;
  if (i.ehConjurador) { const a = papel === 'bloqueio' ? armaduraDe(e, i.dono) : armaDe(e, i.dono); return !!a && !!defCarta(e, a).palavrasChave?.includes('Etéreo'); }
  return !!defCarta(e, instId).palavrasChave?.includes('Etéreo');
}

function ataqueDe(e        , instId        , bloqueando = false)         {
  const inst = e.instancias[instId];
  // Conjurador que bloqueia não devolve o dano da arma (só dano básico), salvo efeito da arma (futuro)
  return inst.ehConjurador ? (bloqueando ? defConjurador(e, inst.dono).danoBasico : ataqueConjurador(e, inst.dono)) : (defCarta(e, instId).ataque ?? 0) + (inst.bonusAtaque ?? 0) + (inst.bonusAtaqueTurno ?? 0);
}

/** A arma do Conjurador (Canal do tipo Arma), se houver. */
export function armaDe(e        , j         )                     {
  return (e.jogadores[j].itens ?? []).find((id) => { const d = defCarta(e, id); return d.categoriaItem === 'canal' && (d.tipoCanal ?? 'arma') === 'arma' && (d.ataque ?? 0) > 0; });
}

/** Arcano que o Conjurador paga para atacar: o custo de ataque dele + o custo da arma (pedido do autor, 04/10). */
export function custoAtaqueConjurador(e        , j         )         {
  const arma = armaDe(e, j);
  return defConjurador(e, j).custoAtaque + (arma ? defCarta(e, arma).custo : 0);
}

/** Grau mínimo para o Conjurador atacar: o Grau da arma (0 sem arma). */
export function grauAtaqueConjurador(e        , j         )         {
  const arma = armaDe(e, j);
  return arma ? defCarta(e, arma).grau : 0;
}

/**
 * Item serve a este Conjurador? Itens sem Valor (básicos ou genéricos) servem a todos; um item com Valor só serve
 * se tiver os dois Valores do Conjurador (pedido do autor, 04/10). Competência e exclusividade continuam valendo.
 */
export function itemServe(conj               , d          )          {
  if (d.tipo !== 'item') return false;
  if (d.valores.length && !conj.valores.every((v) => d.valores.includes(v))) return false;
  if (d.competencia && d.competencia !== conj.comprometimento) return false;
  if (d.exclusivoDe?.length && !d.exclusivoDe.includes(conj.id)) return false;
  return true;
}

/** Ataque do Conjurador ao atacar: dano básico + o Ataque do Canal (a arma). O Extra é um item consumível de suporte, sem Ataque. */
export function ataqueConjurador(e        , j         )         {
  const z = e.jogadores[j];
  const armas = (z.itens ?? []).reduce((s, id) => { const d = defCarta(e, id); return s + (d.categoriaItem === 'canal' ? (d.ataque ?? 0) : 0); }, 0);
  const ci = e.instancias[z.conjuradorId];
  return defConjurador(e, j).danoBasico + armas + (e.habUsos?.[j]?.ataqueExtra ?? 0) + (ci?.bonusAtaque ?? 0) + (ci?.bonusAtaqueTurno ?? 0);
}

/** Armadura equipada no Conjurador (instância), se houver. */
export function armaduraDe(e        , j         )                     {
  return (e.jogadores[j].itens ?? []).find((id) => defCarta(e, id).categoriaItem === 'armadura');
}

/** Nome para o registro: Conjuradores levam o jogador, para não confundir em espelhos. */
function nomeReg(e        , id        )         {
  const i = e.instancias[id];
  if (!i) return id;
  return i.ehConjurador ? `${e.catalogo.conjuradores[i.defId].nome} (jogador ${i.dono})` : e.catalogo.cartas[i.defId].nome;
}

function naMesa(e        , instId        )          {
  const inst = e.instancias[instId];
  if (!inst) return false;
  const z = e.jogadores[inst.dono];
  return z.conjuradorId === instId || z.campo.includes(instId);
}

// ---------- movimentação ----------

function tirar(v          , id        )          {
  const i = v.indexOf(id);
  if (i < 0) return false;
  v.splice(i, 1);
  return true;
}

function comprar(e        , j         , n        )       {
  const z = e.jogadores[j];
  for (let i = 0; i < n; i++) {
    const id = z.baralho.shift();
    if (id === undefined) return; // baralho vazio não faz perder
    z.mao.push(id);
  }
}

// ---------- construtor de efeitos (pedido do autor, 04/10) ----------

const habsDe = (d          , q        ) => (d.habilidades ?? []).map((h, i) => ({ h, i })).filter((x) => x.h.quando === q);
/** Efeitos ao jogar: os da carta + habilidades "ao jogar". */
export function efeitosAoJogar(d          )           { return [...(d.efeitos ?? []), ...habsDe(d, 'jogar').flatMap((x) => x.h.efeitos)]; }
/** Efeitos ao entrar em campo: aoEntrar + habilidades "ao entrar". */
export function efeitosAoEntrar(d          )           { return [...(d.aoEntrar ?? []), ...habsDe(d, 'entrar').flatMap((x) => x.h.efeitos)]; }

const QUANDO_LOG                         = {
  jogar: 'ao ser jogada', entrar: 'ao entrar', ativada: 'ativada', alvo: 'virou alvo', grau: 'foi para o Grau', atacar: 'atacou',
  bloquear: 'defendeu', cemiterio: 'caiu no cemitério', banimento: 'foi banida', danoConjurador: 'acertou o Conjurador',
  aliadoEntra: 'viu um Aliado entrar', aliadoMorre: 'viu um Aliado cair', aliadoDanoConjurador: 'viu um Aliado acertar o Conjurador',
  cataclismaOponente: 'no Cataclisma do turno do oponente',
};
/** Habilidades do construtor de efeitos desta instância: as da carta, ou as do poder do Conjurador (Editor, 04/10). */
export function habilidadesDe(e        , instId        )               {
  const inst = e.instancias[instId];
  if (!inst) return [];
  if (inst.ehConjurador) return e.catalogo.conjuradores[inst.defId]?.poder?.habilidades ?? [];
  return e.catalogo.cartas[inst.defId]?.habilidades ?? [];
}
/**
 * Habilidade "virar/pagar" do construtor no formato de ativação (o mesmo das cartas e do poder do Conjurador):
 * custo.arcano + requisito.arcano é o Arcano; custo.grau ou requisito.grau, o Grau mínimo; custo.vida, dano no Conjurador.
 */
export function ativacaoDeHabilidade(h            )           {
  const grau = Math.max(h.custo?.grau ?? 0, h.requisito?.grau ?? 0);
  const req = { ...(grau ? { grau } : {}), ...(h.requisito?.arcano ? { arcano: h.requisito.arcano } : {}) };
  return {
    ...(h.repentina ? { repentina: true } : {}), custo: h.custo?.arcano ?? 0, suspende: !!h.custo?.suspender, efeitos: h.efeitos,
    descartar: h.custo?.descartar, danoNoConjurador: h.custo?.vida, ...(Object.keys(req).length ? { requisito: req } : {}), consome: false,
  };
}
/** Dispara as habilidades da carta (ou do Conjurador) para esta condição: cada uma vai para a pilha como gatilho do dono. */
function disparar(e        , instId        , quando        , alvo         )       {
  const inst = e.instancias[instId];
  if (!inst) return;
  const conj = inst.ehConjurador ? e.catalogo.conjuradores[inst.defId] : undefined;
  if (!conj && !e.catalogo.cartas[inst.defId]) return;
  const habs = habilidadesDe(e, instId);
  if (!habs.length) return;
  const nome = conj ? conj.nome : defCarta(e, instId).nome;
  for (const [i, h] of habs.entries()) {
    if (h.quando !== quando) continue;
    if (h.soNoSeuTurno && e.ativo !== inst.dono) continue;
    if (h.umaVezPorTurno) { inst.usosHab ??= {}; if (inst.usosHab[i] === e.turno) continue; inst.usosHab[i] = e.turno; }
    e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: inst.dono, efeitos: h.efeitos, alvo, fonte: nome, incomensuravel: conj ? false : temIncomensuravel(defCarta(e, instId)) });
    anotar(e, `${nome} ${QUANDO_LOG[quando]}: o efeito vai para a pilha.`);
  }
}

/** Tira a carta de qualquer zona dos dois jogadores. */
function tirarDeTudo(e        , id        )       {
  for (const z of e.jogadores) for (const zona of [z.mao, z.baralho, z.cemiterio, z.banimento, z.grau, z.campo, z.zonaArcano, z.itens]) tirar(zona, id);
}
/** Carta que sai do campo perde dano e bônus. */
function limparInst(i      )       {
  i.dano = 0; i.suspensa = false; i.bonusAtaque = 0; i.bonusVida = 0; i.bonusAtaqueTurno = 0; i.bonusVidaTurno = 0; i.naoAtacaAte = 0;
  i.palavrasExtra = undefined; i.palavrasTemp = undefined; i.turnosRestantes = undefined; i.destinoSaida = undefined; i.turnoEntradaConta = undefined; i.banirAoSair = undefined;
}
/** Carta jogada pelo Reanimum (permanente): ao sair do campo, por qualquer motivo, vai para o banimento (decisão 27). */
function saiDoCampoPara(e        , id        , padrao                                 )                                                {
  void e;
  return e.instancias[id].banirAoSair ? 'banimento' : padrao;
}
/** Carta que sai da pilha sem ficar em campo (anulada ou resolvida): cemitério, ou banimento se foi jogada pelo Reanimum. */
function fimDaPilha(e        , instId        , dono         )       {
  const inst = e.instancias[instId];
  if (inst.banirAoResolver) { inst.banirAoResolver = false; inst.banidaDisponivel = 0; e.jogadores[dono].banimento.push(instId); anotar(e, `${defCarta(e, instId).nome} é banida.`); }
  else e.jogadores[dono].cemiterio.push(instId);
}
/**
 * Concessão (pedido do autor, 04/10): dá palavras-chave, ±Ataque/±Vida e/ou tempo em campo a uma carta (ou Conjurador).
 * Palavras se somam; ataque/vida somam ao bônus permanente; turnos e destino novos substituem os antigos.

 * O turno em que a carta entra conta como o 1º (de quem for); depois só os turnos do dono. Sai no Final do X-ésimo turno contado.
 */
function conceder(e        , id        , c           )       {
  const inst = e.instancias[id];
  if (!inst) return;
  const nome = nomeReg(e, id);
  const partes           = [];
  if (c.palavras?.length && c.palavrasTurnos && c.palavrasTurnos > 0) {
    (inst.palavrasTemp ??= []).push({ palavras: [...c.palavras], restantes: c.palavrasTurnos, conta: e.ativo !== inst.dono ? e.turno : undefined });
    partes.push(`${c.palavras.join(', ')} por ${c.palavrasTurnos} turno(s)`);
  } else if (c.palavras?.length) { inst.palavrasExtra = [...new Set([...(inst.palavrasExtra ?? []), ...c.palavras])]; partes.push(c.palavras.join(', ')); }
  if (c.ataque) { inst.bonusAtaque = (inst.bonusAtaque ?? 0) + c.ataque; partes.push(`${c.ataque > 0 ? '+' : ''}${c.ataque} de Ataque`); }
  if (c.vida) { inst.bonusVida = (inst.bonusVida ?? 0) + c.vida; partes.push(`${c.vida > 0 ? '+' : ''}${c.vida} de Vida`); }
  if (c.turnos && c.turnos > 0 && !inst.ehConjurador) {
    inst.turnosRestantes = c.turnos; inst.destinoSaida = c.destino ?? 'cemiterio';
    inst.turnoEntradaConta = e.ativo !== inst.dono ? e.turno : undefined; // entrou no turno do oponente: esse turno conta como o 1º
    partes.push(`fica ${c.turnos} turno(s) em campo`);
  }
  if (partes.length) anotar(e, `${nome} recebe: ${partes.join('; ')}.`);
}
/**
 * Final de um turno: conta um turno das cartas com tempo em campo. O turno em que a carta entra conta como o 1º, seja de quem for;
 * depois só contam os turnos do dono. A que zerar sai para o destino (padrão: cemitério; jogada pelo Reanimum: banimento).
 */
function expirarConcessoes(e        , ativo         )       {
  // palavras por N turnos: contam os turnos do dono e o turno em que foram dadas
  for (const [id, i] of Object.entries(e.instancias)) {
    if (!i.palavrasTemp?.length) continue;
    for (const t of i.palavrasTemp) {
      if (i.dono === ativo || t.conta === e.turno) { t.conta = undefined; t.restantes -= 1; }
      if (t.restantes <= 0) anotar(e, `${nomeReg(e, id)} perde ${t.palavras.join(', ')}.`);
    }
    i.palavrasTemp = i.palavrasTemp.filter((t) => t.restantes > 0);
    if (!i.palavrasTemp.length) i.palavrasTemp = undefined;
  }
  for (const j of [0, 1]             ) {
    for (const id of [...e.jogadores[j].campo]) {
      const i = e.instancias[id];
      if (i.turnosRestantes === undefined) continue;
      if (j === ativo) { if (i.turnoEntradaConta === e.turno) i.turnoEntradaConta = undefined; i.turnosRestantes -= 1; }
      else if (i.turnoEntradaConta === e.turno) { i.turnoEntradaConta = undefined; i.turnosRestantes -= 1; }
      else continue;
      if (i.turnosRestantes > 0) continue;
      const dest = i.banirAoSair ? 'banimento' : i.destinoSaida ?? 'cemiterio';
      anotar(e, `${defCarta(e, id).nome} cumpriu o tempo em campo.`);
      if (dest === 'cemiterio') { destruir(e, id); continue; }
      tirar(e.jogadores[j].campo, id);
      moverPara(e, id, dest === 'baralho' ? 'fundo' : dest, j);
      if (dest === 'baralho') embaralhar(e, e.jogadores[i.dono].baralho);
    }
  }
}
/** Põe uma permanente em campo sob o controle de `j` (sem pagar) e dispara "ao entrar". */
function porEmCampo(e        , id        , j         , alvo         )       {
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
function moverPara(e        , id        , destino         , escolhedor         )       {
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
const DESTINO_TXT                          = { mao: 'para a mão', roubar: 'para a sua mão', grau: 'para o Grau', cemiterio: 'para o cemitério', fundo: 'para o fundo do baralho', topo: 'para o topo do baralho', campo: 'para o campo', banimento: 'para o banimento', nada: '' };

/** Tipos padrão quando o efeito não declara tipos de alvo: carta permanente que não seja Conjurador (decisão 28). */
export const TIPOS_PERMANENTE                  = ['aliado', 'mistica', 'estrutura'];
const ACOES_SEM_CONJURADOR              = ['destruir', 'devolver', 'paraBaralho'];
/**
 * O efeito nunca mira o Conjurador (decisão 28): destruir, devolver à mão, mandar ao baralho e conceder só tempo em campo
 * (turnos/destino, sem palavra nem ±Ataque/Vida). Dano, cura e concessões continuam podendo mirá-lo quando o efeito permitir.
 */
export function efeitoSemConjurador(ef        )          {
  if (ef.tipo === 'destruir' || ef.tipo === 'devolver') return true;
  if (ef.tipo !== 'campo') return false;
  if (ACOES_SEM_CONJURADOR.includes(ef.acao)) return true;
  const c = ef.concessao;
  return ef.acao === 'conceder' && !!c && !!c.turnos && !c.palavras?.length && !c.ataque && !c.vida;
}
/** Cartas que o efeito "campo" pode atingir: lado (inimigo/próprio/ambos) e tipos (Aliado, Mística, Estrutura, Conjurador, equipamento). */
export function alvosCampo(e        , dono         , ef                                    )           {
  const lados            = ef.lado === 'inimigo' ? [outro(dono)] : ef.lado === 'proprio' ? [dono] : [0, 1];
  const tipos           = ef.tipos?.length ? ef.tipos : TIPOS_PERMANENTE;
  const semConj = efeitoSemConjurador(ef);
  const r           = [];
  for (const j of lados) {
    const z = e.jogadores[j];
    if (tipos.includes('conjurador') && !semConj) r.push(z.conjuradorId);
    for (const id of z.campo) if (tipos.includes(defCarta(e, id).tipo)) r.push(id);
    if (tipos.includes('equipamento')) r.push(...z.itens);
  }
  // Protessência: efeito com alvo do oponente não a escolhe; efeito em massa (todos) a atinge
  return ef.alcance === 'todos' ? r : r.filter((id) => !protegidaDe(e, id, dono));
}
/**
 * Alvo sem escolha prévia (gatilhos, ou "o jogador atingido escolhe"/"ao acaso"): nunca é automático (pedido do autor, 04/10).
 * Ao acaso: sorteia. Senão: abre a escolha para quem jogou/ativou ou para o jogador atingido, e o efeito acontece no escolhido.
 */
function escolherAlvoDepois(e        , ef        , dono         , lista          , depois             , ladoAtingido         )          {
  if (!lista.length) { anotar(e, 'Efeito sem alvo legal; nada acontece.'); return false; }
  const quem = ef.escolha ?? 'controlador';
  const efeito = { ...ef, escolha: undefined }          ;
  if (quem === 'aleatorio') {
    const id = lista[Math.floor(aleatorio(e) * lista.length)];
    anotar(e, `Alvo sorteado: ${nomeReg(e, id)}.`);
    aplicarEfeitos(e, [efeito], dono, id);
    return false;
  }
  const j = quem === 'afetado' ? ladoAtingido : dono;
  e.pendencia = { tipo: 'escolherCartas', jogador: j, cartas: lista, quantidade: 1, destino: 'nada', titulo: quem === 'afetado' ? 'Escolha qual carta sua é atingida' : 'Escolha o alvo', aplicar: { efeito, dono }, ...(depois.efeitos.length ? { depois } : {}) };
  anotar(e, `Jogador ${j} escolhe o alvo.`);
  return true;
}
/** O alvo deste efeito é escolhido na hora de jogar (só quando quem jogou escolhe). */
const alvoNaJogada = (ef        ) => (ef.escolha ?? 'controlador') === 'controlador';
/** Aplica a ação do efeito "campo" numa carta. */
function aplicarAcaoCampo(e        , id        , ef                                    , dono         , bonus        )       {
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
    case 'conceder': if (ef.concessao) conceder(e, id, ef.concessao); break;
    case 'dano': danoDeEfeito(e, id, v); break;
    case 'curar': inst.dano = Math.max(0, inst.dano - v); anotar(e, `${nomeReg(e, id)} recupera ${v}.`); break;
    case 'destruir': if (!inst.ehConjurador) destruir(e, id); break;
    case 'devolver': if (!inst.ehConjurador) devolver(e, id); break;
    case 'paraBaralho':
      if (!inst.ehConjurador && tirar(zDono.campo, id)) {
        const dest = saiDoCampoPara(e, id, 'baralho');
        limparInst(inst);
        if (dest === 'banimento') { zDono.banimento.push(id); anotar(e, `${defCarta(e, id).nome} é banida.`); break; }
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
function pedirEscolha(e        , j         , cartas          , quantidade        , destino         , titulo        , resto                     , depois                         )       {
  e.pendencia = { tipo: 'escolherCartas', jogador: j, cartas, quantidade: Math.min(quantidade, cartas.length), destino, resto, titulo, ...(depois && depois.efeitos.length ? { depois } : {}) };
}

function destruir(e        , instId        )       {
  const inst = e.instancias[instId];
  const z = e.jogadores[inst.dono];
  if (tirar(z.campo, instId)) {
    const dest = saiDoCampoPara(e, instId, 'cemiterio');
    inst.dano = 0;
    inst.suspensa = false;
    limparInst(inst);
    (dest === 'banimento' ? z.banimento : z.cemiterio).push(instId);
    anotar(e, `${defCarta(e, instId).nome} foi para ${dest === 'banimento' ? 'o banimento' : 'o cemitério'}.`);
    if (defCarta(e, instId).tipo === 'aliado') gatilhoConjurador(e, inst.dono, 'aliadoMorre');
  }
}

/** Devolve uma permanente do campo para a mão do dono. */
function devolver(e        , instId        )       {
  const inst = e.instancias[instId];
  const z = e.jogadores[inst.dono];
  if (tirar(z.campo, instId)) {
    inst.dano = 0;
    inst.suspensa = false;
    const dest = saiDoCampoPara(e, instId, 'mao');
    limparInst(inst);
    (dest === 'banimento' ? z.banimento : z.mao).push(instId);
    anotar(e, `${defCarta(e, instId).nome} ${dest === 'banimento' ? 'é banida' : 'volta para a mão'}.`);
  }
}

/** Gatilho do Conjurador (uma vez por turno): vai para a pilha como um gatilho de "ao entrar". */
function gatilhoConjurador(e        , j         , quando                                                        )       {
  const g = defConjurador(e, j).poder?.gatilho;
  if (g && g.quando === quando && !e.habUsos[j].gatilho && !(g.soNoSeuTurno && e.ativo !== j)) {
    e.habUsos[j].gatilho = true;
    e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: j, efeitos: g.efeitos, fonte: defConjurador(e, j).nome });
    anotar(e, `Jogador ${j}: gatilho da habilidade de ${defConjurador(e, j).nome}.`);
  }
  // habilidades do construtor no poder do Conjurador (Editor, 04/10): cada uma com o seu "uma vez por turno"
  disparar(e, e.jogadores[j].conjuradorId, quando);
}

/** Custo que o jogador paga por esta carta agora (reduções do Conjurador). Não inclui Protegido do alvo. */
export function custoEfetivo(e        , j         , id        )         {
  const d = defCarta(e, id);
  const p = defConjurador(e, j).poder;
  let menos = 0;
  if (p?.primeiraCartaMaisBarata && !e.habUsos[j].carta) menos += p.primeiraCartaMaisBarata;
  if (p?.primeiraRepentinaMaisBarata && ehRepentina(d) && !e.habUsos[j].repentina) menos += p.primeiraRepentinaMaisBarata;
  if (e.habUsos[j].desconto && !e.habUsos[j].carta) menos += e.habUsos[j].desconto;
  if (e.jogadores[j].banimento.includes(id) && e.instancias[id].banidaDisponivel === e.turno && e.ativo === j) menos += e.instancias[id].descontoBanimento;
  return Math.max(0, d.custo - menos);
}

/** Permissão contínua do Conjurador (`permiteJogarDe`, Editor 04/10) que deixa jogar esta carta agora: índice, ou -1. */
export function permissaoDeJogar(e        , j         , id        )         {
  const lista                               = defConjurador(e, j).poder?.permiteJogarDe;
  const inst = e.instancias[id];
  if (!lista?.length || !inst || inst.dono !== j || inst.ehConjurador) return -1;
  const z = e.jogadores[j];
  const d = defCarta(e, id);
  if (d.tipo === 'item' || d.ficha) return -1;
  return lista.findIndex((p, i) => (p.de === 'banimento' ? z.banimento : z.cemiterio).includes(id)
    && (!p.tipos?.length || p.tipos.includes(d.tipo)) && (p.custoMax === undefined || d.custo <= p.custoMax)
    && !(p.umaVezPorTurno && e.habUsos[j].permissoes?.includes(i)));
}
/** Por que a carta pode ser jogada agora: da mão, banida liberada, Reanimum, Anular liberado ou permissão do Conjurador (índice). */
function fonteDaCarta(e        , j         , id        )                                                             {
  const z = e.jogadores[j];
  if (z.mao.includes(id)) return 'mao';
  if (z.banimento.includes(id) && e.instancias[id].banidaDisponivel === e.turno && e.ativo === j) return 'liberada';
  if (z.cemiterio.includes(id) && e.instancias[id].dono === j && defCarta(e, id).palavrasChave?.includes('Reanimum')) return 'reanimum';
  if (z.cemiterio.includes(id) && e.habUsos[j].anularDoCemiterio && defCarta(e, id).efeitos?.some((ef) => ef.tipo === 'anular')) return 'anular';
  const p = permissaoDeJogar(e, j, id);
  return p >= 0 ? p : null;
}
/** De onde o jogador pode jogar esta carta agora: mão, banimento (no turno liberado), cemitério (Anular liberado, Reanimum) ou a zona que o Conjurador permite. */
export function origemDaCarta(e        , j         , id        )                                           {
  const f = fonteDaCarta(e, j, id);
  if (f === null) return null;
  if (f === 'mao') return 'mao';
  return e.jogadores[j].banimento.includes(id) ? 'banimento' : 'cemiterio';
}

function acoesDeEstado(e        )       {
  for (const j of [0, 1]             ) {
    for (const id of [...e.jogadores[j].campo]) {
      const vida = vidaDe(e, id);
      if (vida !== undefined && e.instancias[id].dano >= vida) destruir(e, id);
    }
  }
  const morreu = ([0, 1]             ).filter((j) => {
    const c = e.jogadores[j].conjuradorId;
    return e.instancias[c].dano >= (vidaDe(e, c)          );
  });
  if (morreu.length === 2) e.vencedor = 'empate';
  else if (morreu.length === 1) e.vencedor = outro(morreu[0]);
  if (e.vencedor !== null) anotar(e, e.vencedor === 'empate' ? 'Empate.' : `Jogador ${e.vencedor} venceu.`);
}

// ---------- dano ----------

/** Único caminho de dano do jogo: Raio e pancada são o mesmo dano, com fontes diferentes. */
/** Dano de efeito: com fonte Incomensurável, o que passar da Vida restante da carta vai para o Conjurador do dono dela. */
function danoDeEfeito(e        , alvoId        , valor        )       {
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

function causarDano(e        , alvoId        , valor        , fonte           )       {
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

function alvoLegal(e        , id                    )          {
  return id !== undefined && naMesa(e, id);
}

function alvoDeAliado(e        , id        )          {
  const i = e.instancias[id];
  return !!i && !i.ehConjurador && ['aliado', 'mistica'].includes(defCarta(e, id).tipo);
}

function resolverAlvo(e        , ef        , dono         , escolhido         )                     {
  if (!('alvo' in ef)) return undefined;
  if ((ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado') && protegidaDe(e, escolhido, dono)) return undefined;
  if (ef.alvo === 'escolhido') return alvoLegal(e, escolhido) && !(efeitoSemConjurador(ef) && e.instancias[escolhido ].ehConjurador) ? escolhido : undefined;
  if (ef.alvo === 'escolhidoAliado') return alvoLegal(e, escolhido) && alvoDeAliado(e, escolhido ) ? escolhido : undefined;
  if (ef.alvo === 'escolhidoProprio') return alvoLegal(e, escolhido) && alvoDeAliado(e, escolhido ) && e.instancias[escolhido ].dono === dono ? escolhido : undefined;
  return e.jogadores[ef.alvo === 'conjuradorInimigo' ? outro(dono) : dono].conjuradorId;
}

/**
 * Requisito de uma linha de efeito (04/10, definido no Editor): Grau mínimo e Arcano pago na hora
 * (reserva primeiro, depois cartas do Grau levantadas). Sem cumprir, a linha não acontece.
 */
function cumprirRequisito(e        , r                       , dono         )          {
  if (!r) return true;
  if (r.grau && grauEfetivo(e, dono) < r.grau) { anotar(e, `Requisito não cumprido: precisa de Grau ${r.grau} (tem ${grauEfetivo(e, dono)}); a linha não acontece.`); return false; }
  const custo = r.arcano ?? 0;
  if (custo <= 0) return true;
  if (!pagarNaHora(e, dono, custo)) { anotar(e, `Requisito não cumprido: precisa pagar ${custo} de Arcano; a linha não acontece.`); return false; }
  anotar(e, `Jogador ${dono} paga ${custo} de Arcano pelo requisito.`);
  return true;
}
/** Arcano que dá para pagar ao resolver um efeito: a reserva mais as cartas levantadas do Grau. */
export function arcanoNaHora(e        , j         )         {
  const z = e.jogadores[j];
  return z.arcano + z.grau.filter((id) => !e.instancias[id].suspensa).length * ARCANO_POR_GRAU;
}
/** Paga ao resolver (requisito, reanimar): reserva primeiro, depois suspende cartas levantadas do Grau. false = não deu. */
function pagarNaHora(e        , j         , custo        )          {
  if (custo <= 0) return true;
  if (arcanoNaHora(e, j) < custo) return false;
  const z = e.jogadores[j];
  const daReserva = Math.min(z.arcano, custo);
  z.arcano -= daReserva;
  let falta = custo - daReserva;
  for (const id of z.grau) { if (falta <= 0) break; if (e.instancias[id].suspensa) continue; e.instancias[id].suspensa = true; falta -= ARCANO_POR_GRAU; }
  return true;
}

/** Reanimar (04/10, noite): quanto custa trazer este Aliado (grátis, o custo dele, ou o custo + X; sem `pagamento` = custo + custoExtra). */
export function custoReanimar(ef                                       , d          )         {
  if (ef.pagamento === 'gratis') return 0;
  if (ef.pagamento === 'custo') return d.custo;
  return d.custo + (ef.custoExtra ?? 0);
}
const normal = (s        ) => s.normalize('NFD').replace(/[̀-ͯ]/g, '').trim().toLowerCase();
/** O Aliado serve para este reanimar (está no cemitério certo, é Aliado e passa nos filtros de custo e subtipo)? */
export function reanimarServe(e        , controlador         , ef                                       , id        )          {
  const dc = e.jogadores[0].cemiterio.includes(id) ? 0 : e.jogadores[1].cemiterio.includes(id) ? 1 : -1;
  if (dc < 0) return false;
  const de = ef.de ?? 'qualquer';
  if ((de === 'seu' && dc !== controlador) || (de === 'oponente' && dc === controlador)) return false;
  const d = defCarta(e, id);
  if (d.tipo !== 'aliado') return false;
  if (ef.custoMax !== undefined && d.custo > ef.custoMax) return false;
  if (ef.subtipo && !(d.subtipos ?? []).some((s) => normal(s) === normal(ef.subtipo ))) return false;
  return true;
}
/** Aliados que o reanimar pode trazer agora (e que dá para pagar ao resolver, contando as cartas da mão enviadas ao Arcano). */
export function candidatosReanimar(e        , controlador         , ef                                       , pagando = true)           {
  const disp = arcanoNaHora(e, controlador) + e.jogadores[controlador].mao.reduce((s, id) => s + defCarta(e, id).arcanoGerado, 0);
  return [...e.jogadores[0].cemiterio, ...e.jogadores[1].cemiterio].filter((id) => reanimarServe(e, controlador, ef, id) && (!pagando || custoReanimar(ef, defCarta(e, id)) <= disp));
}
/** Põe o Aliado do cemitério no campo de `dono`: concessão, gatilho do Conjurador e "ao entrar". */
function trazerDoCemiterio(e        , id        , dono         , concessao                       )       {
  for (const z of e.jogadores) tirar(z.cemiterio, id);
  const inst = e.instancias[id];
  limparInst(inst);
  inst.dono = dono; inst.entrouNoTurno = e.turno;
  e.jogadores[dono].campo.push(id);
  if (concessao) conceder(e, id, concessao);
  const dr = defCarta(e, id);
  anotar(e, `${dr.nome} volta do cemitério para o campo do jogador ${dono}.`);
  gatilhoConjurador(e, dono, 'aliadoEntra');
  const aoEntrarR = efeitosAoEntrar(dr);
  if (aoEntrarR.length) e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: dono, efeitos: aoEntrarR, fonte: dr.nome });
}
/** O que pode ser sacrificado (autor, 04/10 noite): só Aliados em campo; Místicas e Estruturas não. */
const unidadesDe = (e        , j         ) => e.jogadores[j].campo.filter((id) => defCarta(e, id).tipo === 'aliado');
/** Sacrifício (efeito secundário): o "destruir" do próprio dono, sem mirar; conta como morte (como a saída por tempo). */
const EFEITO_SACRIFICIO         = { tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'proprio', tipos: ['aliado'] };

function aplicarEfeitos(e        , efeitos          , dono         , escolhido         , bonus = 0, auto = false)       {
  for (let k = 0; k < efeitos.length; k++) {
    const ef = efeitos[k];
    if (ef.requisito && !cumprirRequisito(e, ef.requisito, dono)) continue;
    const depois              = { efeitos: efeitos.slice(k + 1), dono, alvo: escolhido };
    if (ef.tipo === 'perderVida') {
      const j = ef.quem === 'oponente' ? outro(dono) : dono;
      const c = e.jogadores[j].conjuradorId;
      e.instancias[c].dano += ef.valor;
      anotar(e, `${defConjurador(e, j).nome} ${ef.secundario ? `toma ${ef.valor} de dano` : `perde ${ef.valor} de Vida`}.`);
      continue;
    }
    if (ef.tipo === 'descartar') {
      // "tirar da mão" (04/10, noite): os dois jogadores = o efeito para você e depois para o oponente
      if (ef.quem === 'ambos') { aplicarEfeitos(e, [{ ...ef, quem: 'voce', requisito: undefined }, { ...ef, quem: 'oponente', requisito: undefined }, ...efeitos.slice(k + 1)], dono, escolhido, bonus, auto); return; }
      const j = ef.quem === 'voce' ? dono : outro(dono);
      const z = e.jogadores[j];
      const banir = ef.acao === 'banir';
      const destino          = banir ? 'banimento' : 'cemiterio';
      const n = Math.min(ef.valor, z.mao.length);
      if (!n) { anotar(e, `Jogador ${j} não tem cartas na mão para ${banir ? 'banir' : 'descartar'}.`); continue; }
      const modo = ef.modo ?? (ef.aleatorio ? 'acaso' : 'dono');
      if (modo === 'acaso') {
        for (let i = 0; i < n; i++) moverPara(e, z.mao[Math.floor(aleatorio(e) * z.mao.length)], destino, j);
        anotar(e, `Jogador ${j} ${banir ? 'bane' : 'descarta'} ${n} carta(s) da mão ao acaso.`);
        continue;
      }
      if (modo === 'olhando' && j !== dono) {
        // quem controla o efeito vê a mão do oponente (só ele: a visão online mostra as cartas da escolha a quem escolhe)
        pedirEscolha(e, dono, [...z.mao], n, destino, `Mão do oponente: escolha ${n} carta(s) para ${banir ? 'banir' : 'descartar'}`, undefined, depois);
        anotar(e, `Jogador ${dono} olha a mão do jogador ${j} e escolhe ${n} carta(s) para ${banir ? 'banir' : 'descartar'}.`);
        return;
      }
      pedirEscolha(e, j, [...z.mao], n, destino, `${banir ? 'Bana' : 'Descarte'} ${n} carta(s) da sua mão`, undefined, depois);
      anotar(e, `Jogador ${j} escolhe ${n} carta(s) para ${banir ? 'banir' : 'descartar'}.`);
      return;
    }
    if (ef.tipo === 'sacrificar') {
      const j = ef.quem === 'oponente' ? outro(dono) : dono;
      const unidades = unidadesDe(e, j);
      const n = Math.min(ef.valor ?? 1, unidades.length);
      if (!n) { anotar(e, `Jogador ${j} não tem Aliado para sacrificar.`); continue; }
      if (n >= unidades.length) {
        for (const id of unidades) { anotar(e, `Jogador ${j} sacrifica ${nomeReg(e, id)}.`); destruir(e, id); }
        continue;
      }
      e.pendencia = { tipo: 'escolherCartas', jogador: j, cartas: unidades, quantidade: n, destino: 'nada', titulo: `Sacrifique ${n} Aliado(s) seu(s)`, aplicar: { efeito: EFEITO_SACRIFICIO, dono: j }, ...(depois.efeitos.length ? { depois } : {}) };
      anotar(e, `Jogador ${j} escolhe ${n} Aliado(s) para sacrificar.`);
      return;
    }
    if (ef.tipo === 'grauAoCemiterio') {
      const j = ef.quem === 'oponente' ? outro(dono) : dono;
      const z = e.jogadores[j];
      const n = Math.min(ef.valor, z.grau.length);
      if (!n) { anotar(e, `Jogador ${j} não tem cartas no Grau.`); continue; }
      if (n >= z.grau.length) {
        for (const id of [...z.grau]) moverPara(e, id, 'cemiterio', j);
        anotar(e, `Jogador ${j} manda ${n} carta(s) do Grau ao cemitério.`);
        continue;
      }
      pedirEscolha(e, j, [...z.grau], n, 'cemiterio', `Mande ${n} carta(s) do seu Grau ao cemitério`, undefined, depois);
      anotar(e, `Jogador ${j} escolhe ${n} carta(s) do Grau para o cemitério.`);
      return;
    }
    if (ef.tipo === 'reanimar' && !ef.alvoPago && !ef.cobrarNaHora) {
      // 04/10 (noite): quem controla o efeito escolhe no cemitério ao resolver e paga na hora
      const cands = candidatosReanimar(e, dono, ef);
      if (!cands.length) { anotar(e, 'Reanimar: nenhum Aliado serve (ou não dá para pagar); nada acontece.'); continue; }
      const pago = ef.pagamento === 'gratis' ? '' : ef.pagamento === 'custo' || !ef.custoExtra ? ' (paga o custo dele)' : ` (paga o custo dele + ${ef.custoExtra})`;
      e.pendencia = { tipo: 'escolherCartas', jogador: dono, cartas: cands, quantidade: 1, destino: 'nada', titulo: `Escolha o Aliado que volta do cemitério${pago}`, aplicar: { efeito: { ...ef, cobrarNaHora: true, requisito: undefined }, dono }, ...(depois.efeitos.length ? { depois } : {}) };
      anotar(e, `Jogador ${dono} escolhe um Aliado do cemitério.`);
      return;
    }
    if (ef.tipo === 'colocarNoGrau') {
      const z = e.jogadores[dono];
      if (ef.de === 'topo') {
        for (let i = 0; i < ef.valor && z.baralho.length && z.grau.length < GRAU_MAXIMO; i++) { const id = z.baralho.shift() ; e.instancias[id].revelada = false; z.grau.push(id); disparar(e, id, 'grau'); }
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
      if (ef.concessao && e.pendencia?.tipo === 'escolherCartas') e.pendencia.concessao = ef.concessao;
      return;
    }
    if (ef.tipo === 'campo') {
      const lista = alvosCampo(e, dono, ef);
      let alvos          ;
      if (ef.alcance === 'todos') alvos = lista;
      else if (escolhido !== undefined && alvoNaJogada(ef) && protegidaDe(e, escolhido, dono)) { alvoProtegido(e, escolhido); continue; }
      else if (escolhido !== undefined && alvoNaJogada(ef) && lista.includes(escolhido)) alvos = [escolhido];
      else if (escolhido === undefined || !alvoNaJogada(ef)) {
        // sem alvo escolhido na jogada: quem escolhe é definido no efeito (nunca automático)
        if (escolherAlvoDepois(e, ef, dono, lista, depois, ef.lado === 'proprio' ? dono : outro(dono))) return;
        continue;
      } else alvos = [];
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
      if (ef.concessao) conceder(e, inst.id, ef.concessao);
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
      // escolhido ao resolver (cobrarNaHora) ou na ativação antiga (alvoPago, já pago)
      if (escolhido === undefined || !reanimarServe(e, dono, ef, escolhido)) { anotar(e, 'Reanimar: o Aliado saiu do cemitério; nada acontece.'); continue; }
      if (ef.cobrarNaHora) {
        const custo = custoReanimar(ef, defCarta(e, escolhido));
        if (ef.pagarCom?.length) pagarExtra(e, dono, custo, ef.pagarCom); // já validado na escolha
        else if (!pagarNaHora(e, dono, custo)) { anotar(e, `Reanimar: não dá para pagar ${custo}; nada acontece.`); continue; }
        if (custo) anotar(e, `Jogador ${dono} paga ${custo} de Arcano para reanimar.`);
      }
      trazerDoCemiterio(e, escolhido, dono, ef.concessao);
    }
    else if (ef.tipo === 'anular') {
      const i = e.pilha.findIndex((p) => p.tipo === 'carta' && p.instId === escolhido);
      if (i < 0) { anotar(e, 'Anular: a carta já saiu da pilha.'); continue; }
      if (protegidaDe(e, escolhido, dono)) { alvoProtegido(e, escolhido ); continue; }
      const item = e.pilha.splice(i, 1)[0];
      if (item.tipo === 'carta') {
        fimDaPilha(e, item.instId, item.controlador);
        anotar(e, `${defCarta(e, item.instId).nome} foi anulada.`);
      }
    } else if (ef.tipo === 'danoTodosAliados') {
      const adv = e.jogadores[outro(dono)];
      const ids = ef.ambos ? [...e.jogadores[dono].campo, ...adv.campo] : [...adv.campo];
      for (const id of ids) if (defCarta(e, id).tipo === 'aliado') danoDeEfeito(e, id, ef.valor + bonus);
    } else if (ef.tipo === 'destruirTodosAliados') {
      const quem            = ef.ambos ? [0, 1] : [outro(dono)];
      for (const j of quem) for (const id of [...e.jogadores[j].campo]) if (defCarta(e, id).tipo === 'aliado') destruir(e, id);
    }
    else {
      if (escolhido !== undefined && alvoNaJogada(ef) && 'alvo' in ef && (ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado') && protegidaDe(e, escolhido, dono)) { alvoProtegido(e, escolhido); continue; }
      let alvo = alvoNaJogada(ef) ? resolverAlvo(e, ef, dono, escolhido) : undefined;
      if (alvo === undefined && (escolhido === undefined || !alvoNaJogada(ef)) && 'alvo' in ef && (ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado' || ef.alvo === 'escolhidoProprio')) {
        // gatilho sem alvo (ou alvo escolhido depois): abre a escolha, nunca escolhe sozinho
        const proprio = ef.alvo === 'escolhidoProprio';
        const lista = alvosCampo(e, dono, { tipo: 'campo', acao: 'dano', alcance: 'alvo', lado: proprio ? 'proprio' : 'ambos', tipos: ef.alvo === 'escolhido' ? (efeitoSemConjurador(ef) ? TIPOS_PERMANENTE : ['aliado', 'mistica', 'conjurador']) : ['aliado', 'mistica'] });
        if (escolherAlvoDepois(e, ef, dono, lista, depois, proprio ? dono : outro(dono))) return;
        continue;
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

/** O alvo ganhou Protessência depois de escolhido (decisão 30): o efeito perde esse alvo. */
function alvoProtegido(e        , id        )       {
  anotar(e, `Alvo protegido: ${nomeReg(e, id)} tem Protessência; o efeito perde esse alvo.`);
}

function exigeAlvo(efeitos                      )          {
  // reanimar (04/10, noite) não pede alvo na jogada: o Aliado é escolhido no cemitério ao resolver
  return (efeitos ?? []).filter(alvoNaJogada).some((ef) => (ef.tipo === 'campo' && ef.alcance === 'alvo') || ef.tipo === 'banirDaMao' || ('alvo' in ef && (ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado' || ef.alvo === 'escolhidoProprio' || ef.alvo === 'pilha')));
}

/** O alvo escolhido serve para todos os efeitos que o exigem? */
function alvoServe(e        , efeitos          , alvo                    , dono         )          {
  return efeitos.filter(alvoNaJogada).every((ef) => {
    if (ef.tipo === 'banirDaMao') return alvo !== undefined && e.jogadores[dono].mao.includes(alvo);
    if (ef.tipo === 'campo') return ef.alcance === 'todos' || (alvo !== undefined && alvosCampo(e, dono, ef).includes(alvo));
    if (ef.tipo === 'reanimar') return true;
    if (!('alvo' in ef)) return true;
    if ((ef.alvo === 'escolhido' || ef.alvo === 'escolhidoAliado' || ef.alvo === 'pilha') && protegidaDe(e, alvo, dono)) return false;
    if (ef.alvo === 'escolhidoProprio') return alvoLegal(e, alvo) && alvoDeAliado(e, alvo ) && e.instancias[alvo ].dono === dono;
    if (ef.alvo === 'pilha') return alvo !== undefined && e.pilha.some((p) => p.tipo === 'carta' && p.instId === alvo);
    if (ef.alvo === 'escolhidoAliado') return alvoLegal(e, alvo) && alvoDeAliado(e, alvo );
    if (ef.alvo === 'escolhido') return alvoLegal(e, alvo) && !(efeitoSemConjurador(ef) && e.instancias[alvo ].ehConjurador);
    return true;
  });
}

/** Grau para requisitos: cartas na Zona de Grau mais o grauExtra das permanentes em campo. */
export function grauEfetivo(e        , j         )         {
  const z = e.jogadores[j];
  return z.grau.length + z.campo.reduce((s, id) => s + (defCarta(e, id).grauExtra ?? 0), 0);
}

// ---------- criação ----------

export function validarBaralho(cat          , b            , tamanho = 60)           {
  const erros           = [];
  const conj = cat.conjuradores[b.conjurador];
  if (!conj) return [`Conjurador desconhecido: ${b.conjurador}`];
  if (b.cartas.length !== tamanho) erros.push(`O baralho tem ${b.cartas.length} cartas; precisa de ${tamanho}.`);
  for (const id of new Set(b.cartas)) {
    const c = cat.cartas[id];
    if (!c) erros.push(`Carta desconhecida: ${id}`);
    else if (c.valores.length > 0 && !c.valores.some((v) => conj.valores.includes(v)))
      erros.push(`${c.nome} não combina com os Valores de ${conj.nome}.`);
  }
  const espacos = new Set        ();
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
export const REGRAS_PADRAO         = {
  semAcaoPerde: false, primeiroSemGrauNoTurno1: true, grauSemSuspender: true, primeiroConjSemAtacarTurno1: false, impasseTurnos: 5,
  segundoCartaExtra: 0, segundoArcanoInicial: 0, limiteMao: LIMITE_MAO,
  reporMaoNoInicio: false, conjuradorLevantaNoInicio: false, conjuradorSoAtacaComArma: false,
};

export function criarJogo(opcoes                                                                                                       )         {
  const e         = {
    catalogo: opcoes.catalogo,
    jogadores: [null, null]                                  ,
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
  for (const j of [0, 1]             ) {
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
    const z               = {
      conjuradorId: conj.id, baralho, mao: [], campo: [], cemiterio: [], grau: [], zonaArcano: [], banimento: [], itens, arcano: 0,
    };
    e.jogadores[j] = z;
  }
  do {
    e.dados = [1 + Math.floor(aleatorio(e) * 20), 1 + Math.floor(aleatorio(e) * 20)];
  } while (e.dados[0] === e.dados[1]);
  const vencedorDado          = e.dados[0] > e.dados[1] ? 0 : 1;
  anotar(e, `Dados: jogador 0 tirou ${e.dados[0]}, jogador 1 tirou ${e.dados[1]}.`);
  e.pendencia = { tipo: 'escolherInicio', jogador: vencedorDado };
  return e;
}

function iniciarPartida(e        , primeiro         )       {
  e.ativo = primeiro;
  e.primeiro = primeiro;
  for (const j of [0, 1]             ) comprar(e, j, defConjurador(e, j).maoInicial);
  const segundo = outro(primeiro);
  comprar(e, segundo, e.regras.segundoCartaExtra);
  e.jogadores[segundo].arcano += e.regras.segundoArcanoInicial;
  e.turno = 1;
  anotar(e, `Jogador ${primeiro} começa. O jogador ${segundo} compra ${e.regras.segundoCartaExtra} carta(s) a mais e recebe ${e.regras.segundoArcanoInicial} Arcano.`);
  for (const j of [0, 1]             ) {
    const its = e.jogadores[j].itens;
    if (its.length) anotar(e, `Jogador ${j} equipa ${its.map((id) => defCarta(e, id).nome).join(', ')} (ataque do Conjurador: ${ataqueConjurador(e, j)}).`);
  }
  entrarEtapa(e, 'inicio');
}

// ---------- etapas ----------

function entrarEtapa(e        , etapa       )       {
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
  } else if (etapa === 'cataclisma') {
    // gatilho "no Cataclisma do turno do oponente" (04/10, noite): cartas em campo, itens e Conjurador de quem não está jogando
    const zo = e.jogadores[outro(e.ativo)];
    for (const id of [zo.conjuradorId, ...zo.campo, ...zo.itens]) disparar(e, id, 'cataclismaOponente');
  } else if (etapa === 'final') {
    if (!e.regras.reporMaoNoInicio) comprar(e, e.ativo, defConjurador(e, e.ativo).compraFinal);
    for (const id of [z.conjuradorId, ...z.campo, ...z.grau]) e.instancias[id].suspensa = false;
    // bônus "neste turno" do construtor acabam no Final
    for (const i of Object.values(e.instancias)) { i.bonusAtaqueTurno = 0; i.bonusVidaTurno = 0; }
    expirarConcessoes(e, e.ativo);
    for (const id of z.grau) e.instancias[id].revelada = false;
    z.baralho.push(...z.zonaArcano);
    z.zonaArcano = [];
  }
}

/** Existe alguma ação possível para o jogador (usada pela regra semAcaoPerde)? */
function temAcao(e        , j         )          {
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

function avancarEtapa(e        )       {
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

function resolverTopo(e        )       {
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
      if (e.instancias[item.instId].banirAoResolver) anotar(e, `${d.nome} é banida ao resolver.`);
      fimDaPilha(e, item.instId, item.controlador);
    } else {
      z.campo.push(item.instId);
      e.instancias[item.instId].entrouNoTurno = e.turno;
      // jogada pelo Reanimum: quando sair do campo, vai para o banimento
      if (e.instancias[item.instId].banirAoResolver) { e.instancias[item.instId].banirAoResolver = false; e.instancias[item.instId].banirAoSair = true; }
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
function podePagar(e        , j         , custo        )          {
  const z = e.jogadores[j];
  const grau = z.grau.filter((id) => !e.instancias[id].suspensa).length * ARCANO_POR_GRAU;
  const pitch = z.mao.reduce((s, id) => s + defCarta(e, id).arcanoGerado, 0);
  return z.arcano + grau + pitch >= custo;
}

/** Confere um pagamento extra (reserva, Grau levantado, cartas enviadas só as necessárias) sem mexer no estado. */
function validarExtra(e        , j         , custo        , cartas          )       {
  const z = e.jogadores[j];
  let falta = custo - z.arcano - z.grau.filter((id) => !e.instancias[id].suspensa).length * ARCANO_POR_GRAU;
  if (new Set(cartas).size !== cartas.length) falha('Carta repetida no envio ao Arcano.');
  for (const id of cartas) {
    if (!z.mao.includes(id)) falha('Carta inválida para enviar ao Arcano.');
    if (falta <= 0) falha('Não envie mais cartas ao Arcano: o custo já está coberto.');
    falta -= defCarta(e, id).arcanoGerado;
  }
  if (falta > 0) falha('Arcano insuficiente para pagar.');
}
/** Paga um custo extra usando a reserva, depois o Grau levantado, depois as cartas enviadas (necessárias). Valida antes de mexer. */
function pagarExtra(e        , j         , custo        , cartas          )       {
  validarExtra(e, j, custo, cartas);
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

function finalizarAnulacao(e        )       {
  e.passes = 0;
  e.prioridade = e.ativo;
  acoesDeEstado(e);
}

/** Vida que ainda falta para derrubar a carta (o Conjurador conta a vida inteira restante). */
function vidaRestante(e        , id        )         {
  return Math.max(0, (vidaDe(e, id) ?? 0) - e.instancias[id].dano);
}

/** Dano do atacante com a passiva "primeiro dano de combate de um Aliado em cada turno é maior" (marca o uso). */
/** Arma com "dano a mais em unidades": soma quando o golpe do Conjurador acerta Aliados, Místicas ou Estruturas. */
function bonusArmaUnidades(e        , atacante        , acertaUnidade         )         {
  const at = e.instancias[atacante];
  if (!at?.ehConjurador || !acertaUnidade) return 0;
  const arma = armaDe(e, at.dono);
  const n = arma ? defCarta(e, arma).aoAtacar?.danoUnidades ?? 0 : 0;
  if (n) anotar(e, `${defCarta(e, arma ).nome}: +${n} de dano contra unidades.`);
  return n;
}

function comPrimeiroDanoDeAliado(e        , id        , dano        )         {
  const it = e.instancias[id];
  if (!it || it.ehConjurador || defCarta(e, id).tipo !== 'aliado' || dano <= 0) return dano;
  const mais = defConjurador(e, it.dono).poder?.primeiroDanoDeAliadoMais ?? 0;
  if (!mais || e.habUsos[it.dono].danoAliado) return dano;
  e.habUsos[it.dono].danoAliado = true;
  anotar(e, `${defConjurador(e, it.dono).nome}: o primeiro dano de Aliado do turno causa ${mais} a mais.`);
  return dano + mais;
}

/** Bloqueadores que continuam na mesa (os que saíram antes do dano não recebem nem causam dano). */
function bloqueadoresVivos(e        )           {
  return e.combate ? e.combate.bloqueadores.filter((id) => naMesa(e, id)) : [];
}

/** Distribuição padrão: derruba o máximo de bloqueadores (os de menos vida primeiro; Conjurador por último). */
export function distribuicaoPadrao(e        , bloqs          , dano        )                         {
  const ordem = [...bloqs].sort((a, b) => Number(e.instancias[a].ehConjurador) - Number(e.instancias[b].ehConjurador) || vidaRestante(e, a) - vidaRestante(e, b));
  const d                         = {};
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
function aplicarArmadura(e        , conjId        , dano        )         {
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
  if (extra > 0) { ia.dano += extra; anotar(e, `${defCarta(e, armaAtk ).nome} desgasta ${extra} a mais (${ia.dano}/${da.vidaDesgaste}).`); }
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
function validarDistribuicao(bloqs          , dano        , d                        )       {
  let soma = 0;
  for (const [id, v] of Object.entries(d)) {
    if (!bloqs.includes(id)) falha('Distribuição com uma carta que não está bloqueando.');
    if (!Number.isInteger(v) || v < 0) falha('Distribuição inválida.');
    soma += v;
  }
  if (soma !== dano) falha(`Distribua exatamente ${dano} de dano entre os bloqueadores.`);
}

/** Precisa que quem ataca escolha a distribuição (2 ou mais bloqueadores vivos e dano a distribuir)? */
function precisaDistribuir(e        )          {
  const c = e.combate;
  if (!c || c.distribuicao || !naMesa(e, c.atacante)) return false;
  return bloqueadoresVivos(e).length > 1 && ataqueDe(e, c.atacante) > 0;
}

function danoDeCombate(e        )       {
  const c = e.combate;
  if (!c) return;
  const atacanteOk = naMesa(e, c.atacante);
  const atk = e.instancias[c.atacante];
  const conjAlvo = e.jogadores[outro(atk.dono)].conjuradorId;
  const recebido                         = {};
  let danoAoAtacante = 0;
  let resumo        ;
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
    const partes           = [];
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
  // o Conjurador que acerta o Conjurador inimigo: habilidade "quando causa dano de combate ao Conjurador" do poder (Editor, 04/10)
  if ((recebido[conjAlvo] ?? 0) > 0 && atk.ehConjurador) disparar(e, c.atacante, 'danoConjurador');
  acoesDeEstado(e);
}


function todosPassaram(e        )       {
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

function exigirVivo(e        )       {
  if (e.vencedor !== null) falha('A partida já acabou.');
}

function exigirLivre(e        , j         )       {
  exigirVivo(e);
  if (e.pendencia) falha('Há uma decisão pendente.');
  if (e.etapa === 'preparacao') falha('A partida ainda não começou.');
  void j;
}

function exigirPrioridade(e        , j         )       {
  exigirLivre(e, j);
  if (e.prioridade !== j) falha('Você não tem a prioridade.');
}

function exigirNaMao(e        , j         , id        )       {
  if (!e.jogadores[j].mao.includes(id)) falha('A carta não está na sua mão.');
}

/**
 * Enviar carta ao Arcano só existe para pagar um custo (como o pitch do Flesh and Blood):
 * cada carta enviada precisa ser necessária, mas a última pode gerar mais do que falta; a sobra fica na reserva.
 * Devolve a reserva final (já antes de descontar o custo).
 */
function validarEnvio(e        , j         , custo        , cartas          , excluir         )         {
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

function pagarCusto(e        , j         , custo        , cartas          , reserva        )       {
  const z = e.jogadores[j];
  for (const id of cartas) {
    tirar(z.mao, id);
    z.zonaArcano.push(id);
  }
  z.arcano = reserva - custo;
  if (cartas.length) anotar(e, `Jogador ${j} envia ao Arcano: ${cartas.map((id) => `${defCarta(e, id).nome} (+${defCarta(e, id).arcanoGerado})`).join(', ')}.`);
}

/**
 * Protegido (rodada 6; unificado em 04/10): mirar uma carta do oponente com Protegido custa o Arcano dele a mais. Vários
 * Protegidos na mesma carta (o dela, o dado pelo Conjurador, cada um concedido) valem ao mesmo tempo (autor, 04/10 noite:
 * "não soma, trigga os dois"): não viram um Protegido maior; o oponente paga cada um, juntos na mesma janela.
 */
export function custoProtecao(e        , j         , alvo                    )         {
  if (alvo === undefined) return 0;
  const inst = e.instancias[alvo];
  if (!inst || inst.ehConjurador || inst.dono === j || !e.jogadores[inst.dono].campo.includes(alvo)) return 0;
  return protegidosDe(e, alvo).reduce((s, p) => s + p.arcano, 0);
}
/** Cada Protegido da carta em campo, separado: o dela, o do Conjurador do dono e cada um concedido (fixo ou por turnos). */
export function protegidosDe(e        , instId        )                                                                         {
  const inst = e.instancias[instId];
  if (!inst || inst.ehConjurador) return [];
  const d = defCarta(e, instId);
  const r                                                                         = [];
  const prop = protegidoDe(d);
  if (prop) r.push({ fonte: 'carta', arcano: prop.arcano ?? 0 });
  const doConj = d.tipo === 'aliado' ? (defConjurador(e, inst.dono).poder?.aliadosProtegidos ?? 0) : 0;
  if (doConj) r.push({ fonte: 'conjurador', arcano: doConj });
  for (const n of protegidosConcedidos(inst)) r.push({ fonte: 'concedido', arcano: n });
  return r;
}

/** Protegido do alvo (04/10): confere e escolhe o que pagar além do Arcano (vida, descarte, Grau ao cemitério, Grau mínimo). */
function protegidoAPagar(e        , j         , alvo                    , c                                                                                                                              )                                                        {
  const nada = { vida: 0, descartar: []            , grau: []             };
  if (alvo === undefined) return nada;
  const inst = e.instancias[alvo];
  if (!inst || inst.ehConjurador || inst.dono === j || !e.jogadores[inst.dono].campo.includes(alvo)) return nada;
  const pr = protegidoDe(defCarta(e, alvo));
  if (!pr) return nada;
  const z = e.jogadores[j];
  if (pr.grauMin && grauEfetivo(e, j) < pr.grauMin) falha(`Protegido: para mirar esta carta precisa de Grau ${pr.grauMin}.`);
  const vida = pr.vida ?? 0;
  if (vida && (vidaDe(e, z.conjuradorId) ?? 0) - e.instancias[z.conjuradorId].dano <= vida) falha('Protegido: o seu Conjurador não aguenta pagar essa Vida.');
  const ocupadas = new Set([c.carta, ...(c.enviarArcano ?? []), ...(c.descartarCusto ?? [])]);
  const nD = pr.descartar ?? 0;
  const descartar = c.protecaoDescartar ?? (nD ? z.mao.filter((id) => !ocupadas.has(id)).slice(-nD) : []);
  if (descartar.length !== nD || descartar.some((id) => !z.mao.includes(id) || ocupadas.has(id))) falha(`Protegido: descarte ${nD} carta(s) da mão.`);
  const nG = pr.grauParaCemiterio ?? 0;
  const grau = c.protecaoGrau ?? z.grau.slice(0, nG);
  if (grau.length !== nG || grau.some((id) => !z.grau.includes(id))) falha(`Protegido: mande ${nG} carta(s) do seu Grau ao cemitério.`);
  return { vida, descartar, grau };
}
function pagarProtegido(e        , j         , pg                                                       )       {
  const z = e.jogadores[j];
  if (pg.vida) { e.instancias[z.conjuradorId].dano += pg.vida; anotar(e, `Protegido: jogador ${j} paga ${pg.vida} de Vida.`); }
  for (const id of pg.descartar) { tirar(z.mao, id); z.cemiterio.push(id); }
  for (const id of pg.grau) { tirar(z.grau, id); e.instancias[id].suspensa = false; z.cemiterio.push(id); }
  if (pg.descartar.length || pg.grau.length) anotar(e, `Protegido: jogador ${j} paga com ${pg.descartar.length} carta(s) da mão e ${pg.grau.length} do Grau.`);
}

function janelaFindante(e        , j         )          {
  return j === e.ativo && e.etapa === 'principal' && e.pilha.length === 0 && e.combate === null;
}

function jogarCarta(e        , c                                     )       {
  exigirPrioridade(e, c.jogador);
  const origem = origemDaCarta(e, c.jogador, c.carta);
  if (!origem) falha('A carta não está na sua mão (nem pode ser jogada de outra zona agora).');
  const fonte = fonteDaCarta(e, c.jogador, c.carta);
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
  const prot = protegidoAPagar(e, c.jogador, precisaAlvo ? c.alvo : undefined, c);
  pagarCusto(e, c.jogador, custo, c.enviarArcano ?? [], reserva);
  pagarProtegido(e, c.jogador, prot);
  e.habUsos[c.jogador].carta = true;
  e.habUsos[c.jogador].desconto = 0;
  if (ehRepentina(d)) e.habUsos[c.jogador].repentina = true;
  for (const id of descarte) {
    tirar(z.grau, id);
    z.cemiterio.push(id);
  }
  if (typeof fonte === 'number') {
    // permissão contínua do Conjurador (Editor, 04/10)
    const p = defConjurador(e, c.jogador).poder .permiteJogarDe [fonte];
    if (origem === 'banimento') { tirar(z.banimento, c.carta); e.instancias[c.carta].banidaDisponivel = 0; e.instancias[c.carta].descontoBanimento = 0; }
    else tirar(z.cemiterio, c.carta);
    if (p.banir) e.instancias[c.carta].banirAoResolver = true;
    if (p.umaVezPorTurno) (e.habUsos[c.jogador].permissoes ??= []).push(fonte);
  } else if (origem === 'mao') tirar(z.mao, c.carta);
  else if (origem === 'banimento') { tirar(z.banimento, c.carta); e.instancias[c.carta].banidaDisponivel = 0; e.instancias[c.carta].descontoBanimento = 0; }
  else {
    tirar(z.cemiterio, c.carta);
    // Reanimum (decisão 27): não é repetível, a carta jogada do cemitério acaba no banimento. A permissão de Mitonho também bane.
    e.instancias[c.carta].banirAoResolver = true;
    if (!d.palavrasChave?.includes('Reanimum')) e.habUsos[c.jogador].anularDoCemiterio = false;
  }
  e.agiuNoTurno = true;
  e.pilha.push({ tipo: 'carta', id: `p${e.proxId++}`, controlador: c.jogador, instId: c.carta, alvo: c.alvo });
  e.passes = 0;
  e.prioridade = c.jogador;
  anotar(e, `Jogador ${c.jogador} joga ${d.nome}.`);
  if (precisaAlvo && c.alvo) disparar(e, c.alvo, 'alvo');
}

function ativar(e        , c                                      )       {
  exigirLivre(e, c.jogador);
  const z = e.jogadores[c.jogador];
  const ehConj = c.carta === z.conjuradorId;
  const ehItem = !ehConj && z.itens.includes(c.carta);
  if (!ehConj && !ehItem && !z.campo.includes(c.carta)) falha('A carta não está no seu campo.');
  if (ehItem && !defCarta(e, c.carta).ativacao && c.habilidade === undefined) falha('Este item não tem uso.');
  if (ehItem && grauEfetivo(e, c.jogador) < defCarta(e, c.carta).grau) falha(`Para usar ${defCarta(e, c.carta).nome}, precisa de Grau ${defCarta(e, c.carta).grau}.`);
  let at = ehConj ? defConjurador(e, c.jogador).poder?.ativacao : defCarta(e, c.carta).ativacao;
  if (c.habilidade !== undefined) {
    // habilidade "virar/pagar" montada no construtor (04/10); no Conjurador, a do poder (Editor de Conjuradores)
    const h = habilidadesDe(e, c.carta)[c.habilidade];
    if (!h || h.quando !== 'ativada') falha(ehConj ? 'Este Conjurador não tem essa habilidade.' : 'Esta carta não tem essa habilidade.');
    at = ativacaoDeHabilidade(h);
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
  // reanimar com o Aliado já escolhido no comando (formato antigo): paga o custo dele ao ativar; sem isso, escolhe e paga ao resolver
  const rean = at.efeitos.find((ef) => ef.tipo === 'reanimar');
  const reanPago = rean?.tipo === 'reanimar' && c.alvo !== undefined && reanimarServe(e, c.jogador, rean, c.alvo) ? rean : undefined;
  const extraRean = reanPago ? custoReanimar(reanPago, defCarta(e, c.alvo )) : 0;
  const efeitosAt = reanPago ? at.efeitos.map((ef) => (ef === reanPago ? { ...ef, alvoPago: true } : ef)) : at.efeitos;
  const custo = at.custo + (at.requisito?.arcano ?? 0) + extraRean + custoProtecao(e, c.jogador, c.alvo);
  const conjInst = e.instancias[z.conjuradorId];
  if (at.suspendeConjurador && conjInst.suspensa) falha('O Conjurador está suspenso.');
  const descarte = c.descartarCusto ?? [];
  if ((at.descartar ?? 0) !== descarte.length) falha(`Descarte ${at.descartar ?? 0} carta(s) da mão para usar.`);
  if (new Set(descarte).size !== descarte.length || descarte.some((id) => !z.mao.includes(id) || (c.enviarArcano ?? []).includes(id))) falha('Descarte cartas da sua mão (sem repetir as enviadas ao Arcano).');
  if (at.danoNoConjurador && (vidaDe(e, z.conjuradorId) ?? 0) - conjInst.dano <= at.danoNoConjurador) falha('O Conjurador não aguenta esse dano.');
  const reserva = validarEnvio(e, c.jogador, custo, c.enviarArcano ?? []);
  const protA = protegidoAPagar(e, c.jogador, exigeAlvo(at.efeitos) ? c.alvo : undefined, c);
  pagarCusto(e, c.jogador, custo, c.enviarArcano ?? [], reserva);
  pagarProtegido(e, c.jogador, protA);
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
  e.pilha.push({ tipo: 'gatilho', id: `p${e.proxId++}`, controlador: c.jogador, efeitos: efeitosAt, alvo: c.alvo, fonte: ehConj ? defConjurador(e, c.jogador).nome : defCarta(e, c.carta).nome, incomensuravel: !ehConj && temIncomensuravel(defCarta(e, c.carta)) });
  e.passes = 0;
  e.prioridade = c.jogador;
  anotar(e, `Jogador ${c.jogador} ativa ${ehConj ? defConjurador(e, c.jogador).nome : defCarta(e, c.carta).nome}.`);
  if (exigeAlvo(at.efeitos) && c.alvo) disparar(e, c.alvo, 'alvo');
}

/** Aliado que entrou neste turno e não é Proficiente: não ataca nem usa ativação que suspende. */
export function recemChegado(e        , id        )          {
  const inst = e.instancias[id];
  if (inst.ehConjurador || inst.entrouNoTurno !== e.turno) return false;
  const d = defCarta(e, id);
  return d.tipo === 'aliado' && !temPalavra(e, id, 'Proficiente');
}

function atacar(e        , c                                      )       {
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
  disparar(e, c.atacante, 'atacar'); // Aliado, ou o Conjurador com habilidade "quando ataca" no poder (Editor, 04/10)
  // Arma: efeitos ao atacar (ganhar Arcano, comprar...), na hora em que o ataque é declarado
  const arma = atacante.ehConjurador ? armaDe(e, c.jogador) : undefined;
  const efs = arma ? defCarta(e, arma).aoAtacar?.efeitos : undefined;
  if (arma && efs?.length) { anotar(e, `${defCarta(e, arma).nome}: efeito ao atacar.`); aplicarEfeitos(e, efs, c.jogador); }
}

export function executar(e        , c         )       {
  const antes = e.jogadores.map((z) => ({ cem: new Set(z.cemiterio), ban: new Set(z.banimento) }));
  executarComando(e, c);
  // construtor (04/10): cartas que caíram no cemitério ou no banimento disparam as habilidades delas
  if (e.vencedor !== null) return;
  let disparou = false;
  e.jogadores.forEach((z, j) => {
    for (const id of z.cemiterio) if (!antes[j].cem.has(id) && habsDe(e.catalogo.cartas[e.instancias[id]?.defId] ?? ({}            ), 'cemiterio').length) { disparar(e, id, 'cemiterio'); disparou = true; }
    for (const id of z.banimento) if (!antes[j].ban.has(id) && habsDe(e.catalogo.cartas[e.instancias[id]?.defId] ?? ({}            ), 'banimento').length) { disparar(e, id, 'banimento'); disparou = true; }
  });
  if (disparou && !e.pendencia) { e.passes = 0; e.prioridade = e.ativo; }
}

function executarComando(e        , c         )       {
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
        fimDaPilha(e, item.instId, item.controlador);
        anotar(e, `Jogador ${c.jogador} paga ${p.custo} e anula ${defCarta(e, item.instId).nome}.`);
      } else {
        anotar(e, `${defCarta(e, item.instId).nome} resolve.`);
        aplicarEfeitos(e, efeitosAoJogar(defCarta(e, item.instId)), item.controlador, item.alvo);
        fimDaPilha(e, item.instId, item.controlador);
      }
      finalizarAnulacao(e);
      return;
    }
    if (c.tipo === 'escolherCartas' && p.tipo === 'escolherCartas') {
      if (c.jogador !== p.jogador) falha('Outro jogador escolhe.');
      if (c.cartas.length !== p.quantidade || new Set(c.cartas).size !== c.cartas.length || !c.cartas.every((id) => p.cartas.includes(id)))
        falha(`Escolha exatamente ${p.quantidade} carta(s).`);
      // reanimar com custo (autor, 04/10 noite): o custo do Aliado também pode ser pago enviando cartas da mão ao Arcano
      let efA = p.aplicar?.efeito;
      if (c.enviarArcano?.length) {
        if (efA?.tipo !== 'reanimar' || !efA.cobrarNaHora || c.cartas.length !== 1) falha('Esta escolha não tem custo para pagar.');
        validarExtra(e, c.jogador, custoReanimar(efA, defCarta(e, c.cartas[0])), c.enviarArcano);
        efA = { ...efA, pagarCom: [...c.enviarArcano] };
      }
      e.pendencia = null;
      if (p.aplicar) for (const id of c.cartas) aplicarEfeitos(e, [efA ], p.aplicar.dono, id);
      else for (const id of c.cartas) { moverPara(e, id, p.destino, c.jogador); if (p.concessao && p.destino === 'campo') conceder(e, id, p.concessao); }
      const resto = p.cartas.filter((id) => !c.cartas.includes(id));
      if (p.resto) for (const id of resto) moverPara(e, id, p.resto, c.jogador);
      if (c.cartas.length) anotar(e, `Jogador ${c.jogador}: ${c.cartas.length} carta(s) ${DESTINO_TXT[p.destino]}${p.resto && resto.length ? `, ${resto.length} ${DESTINO_TXT[p.resto]}` : ''}.`);
      if (p.depois && !e.pendencia) aplicarEfeitos(e, p.depois.efeitos, p.depois.dono, p.depois.alvo);
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
        if (atacanteEtereo && !etereo(e, b, 'bloqueio')) falha('Só Etéreos (ou um Conjurador com Armadura Etérea) bloqueiam um atacante Etéreo.');
      }
      for (const b of lista) e.instancias[b].suspensa = true;
      for (const b of lista) disparar(e, b, 'bloquear');
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
