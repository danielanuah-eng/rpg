// Um baralho de 60 cartas e um estilo de jogo para cada um dos 9 Conjuradores (rodada 7).
// Cada baralho segue a regra das cores (decisão 37; migrado em 04/10 com corrigirCores: mesmo tipo e custo próximo). São ponto de partida: o agente de testes ajusta.
import type { Estilo } from './bot.ts';
import type { Arquetipo } from './baralhos.ts';

type Lista = Array<[string, number]>;
const A = (c: number, p = 'equilibrado') => `base-aliado-c${c}-${p}`;
const est = (nome: string, agressividade: number, grauAlvo: number, reservaInstantes: boolean, queimaFace = false): Estilo => ({ nome, agressividade, reservaInstantes, grauAlvo, queimaFace });

interface Def { conjurador: string; estilo: Estilo; lista: Lista }

export const decksConjuradores: Def[] = [
  { // Conflitüo + Primitivum: aggro de combate com feras
    conjurador: 'lyandra', estilo: est('Lyandra', 0.85, 5, false, true),
    lista: [[A(1), 4], [A(2, 'agressivo'), 4], [A(2, 'agressivo-proficiente'), 4], [A(3), 3], [A(4, 'agressivo'), 3], [A(4, 'agressivo-proficiente'), 3], [A(6, 'agressivo'), 2], ['r6-piromante-c4', 3], ['r6-presa-trovao-c5', 3], ['r6-fera-cerco-c5', 2], ['base-dano-c1', 3], ['base-dano-c2', 3], ['base-dano-c3', 3], ['base-dano-c4', 2], ['base-dano-conjurador-c3', 2], ['base-dano-conjurador-c1', 2], ['base-comprar-c1', 3], ['base-recurso', 5], ['base-dano-conjurador-c2', 4], ['base-dano-conjurador-c4', 1], [A(4), 1]],
  },
  { // Primitivum + Alvorum: ramp de invocações com Arcano na entrada
    conjurador: 'carmal', estilo: est('Carmal', 0.5, 8, false),
    lista: [[A(1), 3], [A(2), 3], [A(3), 3], [A(4), 3], [A(5), 2], ['base-aliado-recurso-c1', 2], ['base-aliado-recurso-c3', 2], ['base-aliado-recurso-c5', 2], ['base-aliado-recurso-c7', 1], [A(6), 2], [A(8), 2], [A(8, 'equilibrado-descarta-grau'), 2], [A(6, 'equilibrado-descarta-grau'), 3], ['r6-dragao-c8', 2], ['r6-piromante-c4', 2], ['base-estrutura-c1', 2], ['base-estrutura-c4-fonte', 2], ['base-estrutura-c6-nucleo', 1], ['base-estrutura-c8-reator', 1], ['base-curar-c4', 2], ['base-dano-c3', 2], ['base-recurso', 3], [A(2, 'agressivo-proficiente'), 2], [A(2, 'agressivo'), 1], [A(4, 'agressivo'), 1], [A(4, 'agressivo-proficiente'), 1], [A(6, 'agressivo'), 1], ['base-dano-conjurador-c3', 4], ['base-curar-c3', 3]],
  },
  { // Alvorum + Sublimeyn: ramp elegante, primeira jogada mais barata
    conjurador: 'ricardo', estilo: est('Alquimista Ricardo', 0.4, 8, true),
    lista: [[A(2), 4], [A(3), 4], [A(4), 4], [A(5), 2], ['base-aliado-recurso-c3', 4], ['base-aliado-recurso-c5', 2], ['r6-anjo-c6', 3], [A(7), 2], [A(8), 4], ['base-estrutura-c1', 4], ['base-comprar-c3', 4], ['base-comprar-c2-repentina', 3], ['base-destruir-c3', 4], ['base-destruir-c4-repentina', 4], ['base-recurso', 4], [A(1), 1], [A(6), 2], ['base-aliado-recurso-c7', 1], ['base-dano-c2', 4]],
  },
  { // Sublimeyn + Persona: tempo de Etéreos e blink
    conjurador: 'roh', estilo: est('Roh', 0.8, 5, true),
    lista: [['bal-etereo-c1', 4], ['bal-etereo-c2', 4], ['bal-etereo-c3', 4], ['r6-mercador-c7', 2], ['bal-devolver-c2-repentina', 4], ['base-comprar-c3', 4], ['base-estrutura-c1', 2], ['base-estrutura-c3', 2], ['base-comprar-c1', 4], ['base-recurso', 4], [A(4), 4], [A(2), 4], [A(3), 3], [A(5), 1], [A(1), 3], ['base-dano-c2', 4], ['base-dano-c1', 4]],
  },
  { // Persona + Morbidyum: multidão e almas, troca de Aliados que alimenta a mão
    conjurador: 'agnelo', estilo: est('Agnelo', 0.65, 6, false),
    lista: [[A(1), 3], [A(2), 4], [A(2, 'agressivo'), 3], [A(2, 'agressivo-proficiente'), 3], [A(3), 4], [A(4, 'agressivo'), 3], [A(2, 'defensivo'), 3], [A(4, 'defensivo'), 3], ['bal-etereo-c2', 3], ['bal-etereo-c3', 2], [A(6, 'defensivo'), 2], ['base-comprar-c1', 4], ['base-recurso', 4], [A(4), 2], [A(4, 'agressivo-proficiente'), 1], [A(6, 'agressivo'), 2], ['base-dano-c2', 4], ['base-dano-c1', 4], ['base-estrutura-c1', 2]],
  },
  { // Morbidyum + Cognitium: controle paciente, cura e mente
    conjurador: 'diastaros', estilo: est('Diástaros', 0.3, 8, true),
    lista: [['bal-etereo-c2', 4], ['bal-etereo-c3', 4], [A(6), 4], [A(8), 4], ['base-recurso', 4], [A(2), 4], [A(4), 4], [A(3), 4], [A(5), 4], [A(7), 4], ['base-dano-c2', 4], ['base-dano-c1', 4], ['base-comprar-c1', 4], [A(1), 4], ['bal-etereo-c1', 4]],
  },
  { // Cognitium + Eternyr: controle reativo com repentinas baratas
    conjurador: 'ana', estilo: est('Ana', 0.25, 8, true),
    lista: [['eternyr-comprar-c1', 2], ['eternyr-comprar-c2', 3], ['eternyr-comprar-c3', 2], ['eternyr-comprar-c2-repentina', 3], ['eternyr-comprar-c4', 1], ['bal-anular-c3-repentina', 4], ['base-dano-c2-repentina', 3], ['base-dano-c3-repentina', 4], ['eternyr-dano-aliado-c5', 2], ['eternyr-destruir-c6-g8', 2], ['eternyr-aliado-c6-g8', 2], ['base-estrutura-c3', 2], ['base-estrutura-c6-nucleo', 1], ['base-comprar-c3', 2], ['base-recurso', 5], ['base-dano-c4-repentina', 2], ['eternyr-dano-aliado-c4', 1], ['eternyr-comprar-c3-repentina', 4], ['eternyr-dano-jogador-c5', 1], ['base-comprar-c2-repentina', 1], ['base-dano-c2', 1], [A(4), 4], [A(7), 1], ['base-aliado-recurso-c7', 1], ['base-aliado-recurso-c5', 1], [A(6), 2], [A(2), 2]],
  },
  { // Eternyr + Dominum: stall e cutuco, desgaste de Aliados e Místicas
    conjurador: 'mitonho', estilo: est('Mitonho', 0.2, 8, true),
    lista: [[A(2, 'defensivo'), 4], [A(4, 'defensivo'), 4], [A(6, 'defensivo'), 3], [A(8, 'equilibrado-descarta-grau'), 2], ['eternyr-aliado-c6-g8', 2], ['base-mistica-c2', 3], ['base-mistica-c4', 3], ['base-destruir-c3', 3], ['base-destruir-c4-repentina', 2], ['bal-remocao-grau-c3', 3], ['bal-anular-c3-repentina', 3], ['eternyr-dano-aliado-c4', 2], ['eternyr-dano-aliado-c5', 2], ['eternyr-comprar-c2', 2], ['eternyr-comprar-c3', 2], ['base-recurso', 4], [A(7), 2], [A(5), 2], ['eternyr-comprar-c3-repentina', 3], ['base-dano-c3-repentina', 1], [A(3), 1], ['base-dano-c4-repentina', 2], [A(4), 1], ['eternyr-comprar-c4', 1], ['eternyr-destruir-c6-g8', 1]],
  },
  { // Dominum + Conflitüo: parrudos protegidos que pressionam no meio do jogo
    conjurador: 'osmar', estilo: est('Osmar', 0.6, 6, false),
    lista: [[A(1), 4], [A(3), 4], ['r6-colosso-c7', 4], ['r6-presa-trovao-c5', 4], ['base-dano-c2-repentina', 2], ['base-estrutura-c4-torre', 4], ['bal-estrutura-cerco-c3', 4], ['base-comprar-c1', 2], ['base-recurso', 5], [A(2), 4], [A(4), 4], [A(5), 4], ['base-dano-c4-repentina', 4], ['base-dano-c3-repentina', 4], ['bal-remocao-grau-c3', 2], ['base-dano-c2', 2]],
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
