// Visão de um jogador sobre a partida: o estado do motor sem o que esse jogador não pode ver.
// Esconde a mão do oponente, a ordem dos dois baralhos, o Grau virado do oponente e as cartas olhadas pelo outro jogador.
// As cartas escondidas recebem apelidos novos a cada visão (o id real revelaria qual carta é, pela ordem da lista do baralho)
// e a definição "oculta". A semente do embaralhador some, para ninguém prever as próximas compras.
import type { Estado, Inst, Jogador } from '../../tcg/src/tipos.ts';

export const OCULTA = 'oculta';

export function visaoDe(e: Estado, j: Jogador): Record<string, unknown> {
  const visiveis = new Set<string>();
  const ver = (ids: Iterable<string>) => { for (const id of ids) visiveis.add(id); };
  for (const k of [0, 1] as Jogador[]) {
    const z = e.jogadores[k];
    ver([z.conjuradorId, ...z.campo, ...z.cemiterio, ...z.zonaArcano, ...z.banimento, ...z.itens]);
    if (k === j) ver([...z.mao, ...z.grau]);
    else ver(z.grau.filter((id) => e.instancias[id].revelada));
  }
  for (const p of e.pilha) {
    if (p.tipo === 'carta') ver([p.instId, ...(p.alvo ? [p.alvo] : [])]);
    else if (p.tipo === 'ataque') ver([p.atacante, p.alvo]);
    else if (p.alvo) visiveis.add(p.alvo);
  }
  if (e.combate) ver([e.combate.atacante, e.combate.alvo, ...(e.combate.bloqueadores ?? (e.combate.bloqueador ? [e.combate.bloqueador] : []))]);
  const pend = e.pendencia;
  if (pend?.tipo === 'escolherDoTopo' && pend.jogador === j) ver(pend.cartas);
  // construtor (04/10): quem escolhe vê as cartas da escolha (inclusive a mão do oponente, em "olhar a mão")
  if (pend?.tipo === 'escolherCartas' && pend.jogador === j) ver(pend.cartas);
  if (pend?.tipo === 'pagarAnulacao' && pend.item.tipo === 'carta') visiveis.add(pend.item.instId);
  // custo condicional (05/10): a carta que cobra (em campo ou na pilha) é pública
  if (pend?.tipo === 'pagarCusto' && pend.fonte) visiveis.add(pend.fonte);

  let n = 0;
  const apelidos = new Map<string, string>();
  const instancias: Record<string, Inst> = {};
  const tr = (id: string): string => {
    if (visiveis.has(id)) { instancias[id] = e.instancias[id]; return id; }
    let a = apelidos.get(id);
    if (!a) {
      a = `x${++n}`;
      apelidos.set(id, a);
      const i = e.instancias[id];
      instancias[a] = { ...i, id: a, defId: OCULTA, dano: 0, entrouNoTurno: 0, descontoBanimento: 0, banidaDisponivel: 0, banirAoResolver: false };
    }
    return a;
  };
  const embaralhar = (ids: string[]) => ids.map(tr).sort();
  const jogadores = e.jogadores.map((z, k) => ({
    ...z,
    conjuradorId: tr(z.conjuradorId),
    baralho: embaralhar(z.baralho),
    mao: k === j ? z.mao.map(tr) : embaralhar(z.mao),
    campo: z.campo.map(tr), cemiterio: z.cemiterio.map(tr), grau: z.grau.map(tr), zonaArcano: z.zonaArcano.map(tr),
    banimento: z.banimento.map(tr), itens: z.itens.map(tr),
  }));
  // cartas públicas fora das zonas (na pilha, na pendência de anular): o cliente também precisa delas
  for (const id of visiveis) if (e.instancias[id] && !instancias[id]) instancias[id] = e.instancias[id];
  let pendencia: unknown = pend;
  if (pend?.tipo === 'escolherDoTopo' && pend.jogador !== j) pendencia = { ...pend, cartas: pend.cartas.map(tr) };
  if (pend?.tipo === 'escolherCartas' && pend.jogador !== j) pendencia = { ...pend, cartas: pend.cartas.map(tr), depois: undefined };
  const { catalogo: _c, rng: _r, ...resto } = e;
  return { ...resto, rng: 0, jogadores, instancias, pendencia };
}
