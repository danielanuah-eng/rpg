// Um baralho de 60 cartas e um estilo de jogo para cada um dos 9 Conjuradores (rodada 7).
// Cada baralho segue a regra das cores (decisão 37). Na migração de 04/10 as cartas que deixaram de servir foram trocadas
// pelas cartas provisórias de mesma função (src/cartas-provisorias.ts, prefixo `prov-`, comentários "prov ←" abaixo); onde
// não há provisória, ficou a carta genérica do mesmo tipo e custo. São ponto de partida: o agente de testes ajusta.
import type { Estilo } from './bot.ts';
import type { Arquetipo } from './baralhos.ts';

type Lista = Array<[string, number]>;
const A = (c: number, p = 'equilibrado') => `base-aliado-c${c}-${p}`;
const P = (id: string) => `prov-${id}`;
const est = (nome: string, agressividade: number, grauAlvo: number, reservaInstantes: boolean, queimaFace = false): Estilo => ({ nome, agressividade, reservaInstantes, grauAlvo, queimaFace });

interface Def { conjurador: string; estilo: Estilo; lista: Lista }

export const decksConjuradores: Def[] = [
  { // Conflitüo + Primitivum: aggro de combate com feras
    conjurador: 'lyandra', estilo: est('Lyandra', 0.85, 5, false, true),
    lista: [[A(1), 4], [A(2, 'agressivo'), 4], [A(2, 'agressivo-proficiente'), 4], [A(3), 3], [A(4, 'agressivo'), 3], [A(4, 'agressivo-proficiente'), 3], [A(6, 'agressivo'), 2],
      ['r6-piromante-c4', 3], ['r6-presa-trovao-c5', 3], ['r6-fera-cerco-c5', 2],
      ['base-dano-c1', 3], ['base-dano-c2', 3], ['base-dano-c3', 3], ['base-dano-c4', 2], ['base-dano-conjurador-c3', 2], ['base-dano-conjurador-c1', 2],
      ['base-comprar-c1', 3], ['base-recurso', 5],
      [P('conflituo-dano-c2-repentina'), 2], [P('conflituo-torre-c4'), 2], // prov ← dano repentino c2, torre (C+D)
      [P('primitivum-curar-c2'), 2]], // prov ← cura c2 (M+R+A)
  },
  { // Primitivum + Alvorum: ramp de invocações com Arcano na entrada
    conjurador: 'carmal', estilo: est('Carmal', 0.5, 8, false),
    lista: [[A(1), 3], [A(2), 3], [A(3), 3], [A(4), 3], [A(5), 2],
      ['base-aliado-recurso-c1', 2], ['base-aliado-recurso-c3', 2], ['base-aliado-recurso-c5', 2], ['base-aliado-recurso-c7', 1],
      [A(6), 2], [A(8), 2], [A(8, 'equilibrado-descarta-grau'), 2], [A(6, 'equilibrado-descarta-grau'), 2], ['r6-dragao-c8', 2], ['r6-piromante-c4', 2],
      ['base-estrutura-c1', 2], ['base-estrutura-c4-fonte', 2], ['base-estrutura-c6-nucleo', 1], ['base-estrutura-c8-reator', 1],
      ['base-curar-c4', 2], ['base-dano-c3', 2], ['base-recurso', 3],
      [P('primitivum-aliado-c2-defensivo'), 3], [P('alvorum-anjo-c6'), 2], [P('alvorum-comprar-c3'), 3], // prov ← Aliado defensivo c2, Anjo da aurora, comprar c3
      [P('alvorum-varredura-c3'), 4], // prov ← varredura c3 (2) e destruir c3 (2)
      [P('primitivum-aliado-c4-defensivo'), 2]], // prov ← Aliado defensivo c4
  },
  { // Alvorum + Sublimeyn: ramp elegante, primeira jogada mais barata
    conjurador: 'ricardo', estilo: est('Alquimista Ricardo', 0.4, 8, true),
    lista: [[A(2), 2], [A(3), 3], [A(4), 4], [A(5), 2],
      ['base-aliado-recurso-c3', 2], ['base-aliado-recurso-c5', 2], ['r6-anjo-c6', 3], [A(7), 2], [A(8), 2],
      ['base-estrutura-c1', 2],
      ['base-comprar-c3', 4], ['base-comprar-c2-repentina', 3], ['base-destruir-c3', 3], ['base-destruir-c4-repentina', 2], ['base-recurso', 4],
      [P('sublimeyn-etereo-c2'), 3], [P('sublimeyn-etereo-c3'), 3], [P('sublimeyn-devolver-c2-repentina'), 3], // prov ← Aliados Etéreos c2/c3, devolver
      [P('alvorum-bomba-c8'), 3], // prov ← Dragão ancestral (2) e estrutura de Arcano c8 (1)
      [P('alvorum-fonte-c4'), 4], // prov ← estruturas de Arcano c4 (2) e c6 (2)
      [P('alvorum-varredura-c3'), 4]], // prov ← varreduras c3 (2) e c5 (1), Ira (1)
  },
  { // Sublimeyn + Persona: tempo de Etéreos e blink
    conjurador: 'roh', estilo: est('Roh', 0.8, 5, true),
    lista: [['bal-etereo-c1', 4], ['bal-etereo-c2', 4], ['bal-etereo-c3', 4], ['r6-mercador-c7', 2],
      ['bal-devolver-c2-repentina', 4], ['base-comprar-c3', 2], ['base-estrutura-c1', 2], ['base-estrutura-c3', 2], ['base-comprar-c1', 2], ['base-recurso', 7],
      [P('persona-espiao-c4'), 3], [P('persona-aliado-c2-agressivo-proficiente'), 4], // prov ← Espião alado, Aliados agressivos c2
      [P('sublimeyn-comprar-c2-repentina'), 3], [P('sublimeyn-dano-c1-repentina'), 4], [P('sublimeyn-dano-c2-repentina'), 4], // prov ← compra e danos repentinos
      [P('sublimeyn-devolver-c2-repentina'), 2], // prov ← destruir c4 repentina
      [A(2), 3], [A(4), 2], [A(1), 2]], // genéricas ← Aliados agressivo/defensivo c2 e c4, Aliado de recurso c1
  },
  { // Persona + Morbidyum: multidão e almas, troca de Aliados que alimenta a mão
    conjurador: 'agnelo', estilo: est('Agnelo', 0.65, 6, false),
    lista: [[A(1), 3], [A(2), 2], [A(2, 'agressivo'), 3], [A(2, 'agressivo-proficiente'), 3], [A(3), 3], [A(4, 'agressivo'), 3], [A(2, 'defensivo'), 3], [A(4, 'defensivo'), 3], ['bal-etereo-c2', 3], ['bal-etereo-c3', 2],
      [A(6, 'defensivo'), 2], ['base-comprar-c1', 2], ['base-recurso', 8],
      [P('persona-espiao-c4'), 2], [P('morbidyum-aliado-c6-descarta-grau'), 2], [P('morbidyum-dano-c3'), 4], // prov ← Espião alado, Aliado c6 descarta Grau, dano c3/c4
      [P('morbidyum-curar-c2'), 2], [P('morbidyum-curar-c4'), 2], [P('morbidyum-enfermaria-c3'), 2], [P('morbidyum-comprar-c3'), 2], // prov ← curas, enfermaria, comprar c3
      ['base-dano-c2', 1], // genérica ← dano c4 (1)
      [P('morbidyum-mistica-c2'), 2], [P('morbidyum-mistica-c4'), 1]], // prov ← Místicas c2/c4
  },
  { // Morbidyum + Cognitium: controle paciente, cura e mente
    conjurador: 'diastaros', estilo: est('Diástaros', 0.3, 8, true),
    lista: [['bal-etereo-c2', 3], ['bal-etereo-c3', 3], [A(6), 4], [A(8), 4], ['base-recurso', 4],
      [P('cognitium-aliado-c4-defensivo'), 4], [P('cognitium-espiao-c4'), 3], [P('cognitium-mercador-c7'), 2], // prov ← Aliados defensivos c4 e Mística c4, Espião, Mercador
      [P('morbidyum-curar-c2'), 3], [P('morbidyum-curar-c4'), 4], [P('morbidyum-enfermaria-c3'), 3], // prov ← curas c2/c4/c6 e enfermaria
      [P('morbidyum-comprar-c3'), 4], [P('cognitium-comprar-c2-repentina'), 4], [P('cognitium-anular-c3-repentina'), 4], // prov ← compras, anular, estrutura de compra
      [P('morbidyum-dano-c3'), 2], // prov ← dano c3
      [A(2), 3], ['base-dano-c2', 2], // genéricas ← Aliados defensivos c2/c6 e c8, estrutura de Arcano, dano repentino c2
      [P('cognitium-mistica-c2'), 2], [P('morbidyum-mistica-c4'), 2]], // prov ← Místicas c2/c4
  },
  { // Cognitium + Eternyr: controle reativo com repentinas baratas
    conjurador: 'ana', estilo: est('Ana', 0.25, 8, true),
    lista: [['eternyr-comprar-c1', 2], ['eternyr-comprar-c2', 3], ['eternyr-comprar-c3', 2], ['eternyr-comprar-c2-repentina', 3], ['eternyr-comprar-c4', 1],
      ['bal-anular-c3-repentina', 4], ['base-dano-c2-repentina', 3], ['base-dano-c3-repentina', 3],
      ['eternyr-dano-aliado-c5', 2], ['eternyr-destruir-c6-g8', 2], ['eternyr-aliado-c6-g8', 2],
      ['base-estrutura-c3', 2], ['base-estrutura-c6-nucleo', 1], ['base-comprar-c3', 2], ['base-recurso', 6],
      [P('eternyr-destruir-c4-repentina'), 3], [P('eternyr-remocao-grau-c3'), 3], [P('eternyr-varredura-c3'), 3], [P('eternyr-devolver-c2-repentina'), 2], // prov ← Destruir, Remoção que cresce, varreduras, devolver
      [P('eternyr-mistica-c2'), 2], // prov ← Místicas c2
      [P('cognitium-espiao-c4'), 2], [P('cognitium-mercador-c7'), 2], [P('cognitium-aliado-c4-defensivo'), 3], // prov ← Espião, Mercador, Aliado defensivo c4
      [A(6), 2]], // genérica ← Aliado defensivo c6
  },
  { // Eternyr + Dominum: stall e cutuco, desgaste de Aliados e Místicas
    conjurador: 'mitonho', estilo: est('Mitonho', 0.2, 8, true),
    lista: [[A(2, 'defensivo'), 4], [A(4, 'defensivo'), 4], [A(6, 'defensivo'), 3], [A(8, 'equilibrado-descarta-grau'), 2], ['eternyr-aliado-c6-g8', 2],
      ['base-mistica-c2', 3], ['base-mistica-c4', 3], ['base-destruir-c3', 3], ['base-destruir-c4-repentina', 2],
      ['bal-remocao-grau-c3', 3], ['bal-anular-c3-repentina', 3], ['eternyr-dano-aliado-c4', 2], ['eternyr-dano-aliado-c5', 2],
      ['eternyr-comprar-c2', 2], ['eternyr-comprar-c3', 2], ['base-recurso', 6],
      [P('dominum-colosso-c7'), 2], [P('dominum-aliado-c4-defensivo'), 2], // prov ← Colosso de ferro, Presa-trovão
      [P('dominum-cerco-c3'), 4], [P('dominum-destruir-c3'), 1], // prov ← estruturas de cerco (3) e torre (2)
      [P('dominum-ira-c4'), 3], [P('eternyr-varredura-c3'), 2]], // prov ← Ira, Pôr do sol, varredura c3
  },
  { // Dominum + Conflitüo: parrudos protegidos que pressionam no meio do jogo
    conjurador: 'osmar', estilo: est('Osmar', 0.6, 6, false),
    lista: [[A(1), 4], [A(3), 4],
      ['r6-colosso-c7', 3], ['r6-presa-trovao-c5', 3],
      ['base-dano-c2-repentina', 2], ['base-estrutura-c4-torre', 2], ['bal-estrutura-cerco-c3', 2], ['base-comprar-c1', 2], ['base-recurso', 8],
      [P('conflituo-aliado-c2-agressivo'), 4], [P('dominum-aliado-c4-defensivo'), 4], [P('dominum-colosso-c7'), 2], // prov ← Aliados agressivos c2, defensivos c4 e c6
      [P('conflituo-fera-c5'), 2], [P('conflituo-piromante-c4'), 2], // prov ← Fera de cerco, Piromante
      [P('conflituo-dano-c2-repentina'), 2], [P('dominum-destruir-c3'), 2], [P('dominum-mistica-c2'), 2], // prov ← dano c3, destruir c3, Místicas c2
      [A(2), 4], [A(4), 3], [A(5), 3]], // genéricas ← Aliados agressivos/defensivos sem provisória
  },
];

export const arquetiposConjuradores = (): Arquetipo[] =>
  decksConjuradores.map((d) => {
    const somada = new Map<string, number>();
    for (const [id, n] of d.lista) somada.set(id, (somada.get(id) ?? 0) + n);
    const total = [...somada.values()].reduce((s, n) => s + n, 0);
    if (total > 60) throw new Error(`${d.conjurador} tem ${total} cartas`);
    if (total < 60) somada.set('base-recurso', (somada.get('base-recurso') ?? 0) + (60 - total));
    return { nome: d.estilo.nome, conjurador: d.conjurador, estilo: { ...d.estilo }, lista: [...somada.entries()] };
  });
