// Oito montagens de baralho com as cartas básicas, para as simulações. Sem limite de cópias (o livro de regras ainda não define).
import { catalogoBase } from '../src/cartas-base.ts';
import type { BaralhoDef } from '../src/tipos.ts';
import type { Estilo } from './bot.ts';

type Lista = Array<[string, number]>;

export interface Arquetipo {
  nome: string;
  /** Conjurador do baralho (padrão: 'padrao'). */
  conjurador?: string;
  estilo: Estilo;
  lista: Lista;
  /** Itens equipados no Conjurador (fora das 60 cartas). */
  itens?: string[];
}

const A = (c: number, p = 'equilibrado') => `base-aliado-c${c}-${p}`;

function completar(lista: Lista, preenchimento = 'base-recurso'): Lista {
  const total = lista.reduce((s, [, n]) => s + n, 0);
  if (total > 60) throw new Error(`Lista com ${total} cartas`);
  for (const [id] of lista) if (!catalogoBase.cartas[id]) throw new Error(`Carta inexistente: ${id}`);
  return total === 60 ? lista : [...lista, [preenchimento, 60 - total]];
}

export const arquetipos: Arquetipo[] = [
  {
    nome: 'Aggro',
    estilo: { nome: 'Aggro', agressividade: 0.95, reservaInstantes: false, grauAlvo: 4, queimaFace: false },
    lista: completar([[A(1), 4], [A(2, 'agressivo'), 6], [A(2, 'agressivo-proficiente'), 4], [A(2), 4], [A(3), 5], [A(4, 'agressivo'), 4], [A(4, 'agressivo-proficiente'), 3], [A(5), 3], [A(6, 'agressivo'), 2],
      ['base-dano-c1', 4], ['base-dano-c2', 4], ['base-dano-c3', 3], ['base-dano-c4', 2], ['base-dano-conjurador-c3', 3], ['base-comprar-c1', 3], ['base-estrutura-c1', 2], ['base-recurso', 4]]),
  },
  {
    nome: 'Midrange',
    estilo: { nome: 'Midrange', agressividade: 0.6, reservaInstantes: false, grauAlvo: 6, queimaFace: false },
    lista: completar([[A(2), 4], [A(3), 4], [A(4), 4], [A(5), 4], [A(6), 3], [A(8), 2], [A(2, 'defensivo'), 3], [A(4, 'defensivo'), 3],
      ['base-dano-c2', 3], ['base-dano-c3', 3], ['base-dano-c4', 2], ['base-destruir-c3', 2], ['base-comprar-c3', 3], ['base-comprar-c1', 2], ['base-estrutura-c3', 2], ['base-mistica-c2', 2], ['base-curar-c2', 2], ['base-recurso', 4]]),
  },
  {
    nome: 'Controle',
    estilo: { nome: 'Controle', agressividade: 0.3, reservaInstantes: true, grauAlvo: 8, queimaFace: false },
    lista: completar([['base-destruir-c3', 4], ['base-destruir-c4-repentina', 4], ['base-dano-c2-repentina', 4], ['base-dano-c3-repentina', 4], ['base-dano-c4-repentina', 3], ['base-comprar-c3', 4], ['base-comprar-c2-repentina', 4],
      ['base-curar-c2', 3], ['base-curar-c4', 3], [A(4, 'defensivo'), 3], [A(6, 'defensivo'), 4], [A(6), 3], [A(8), 3], [A(8, 'equilibrado-descarta-grau'), 2], ['base-mistica-c4', 3], ['base-mistica-c2', 2], ['base-estrutura-c3', 3], ['base-recurso', 3]]),
  },
  {
    nome: 'Burn',
    estilo: { nome: 'Burn', agressividade: 0.5, reservaInstantes: false, grauAlvo: 5, queimaFace: true },
    lista: completar([['base-dano-conjurador-c3', 6], ['base-dano-conjurador-c5', 4], ['base-dano-c1', 4], ['base-dano-c2', 4], ['base-dano-c3', 4], ['base-dano-c4', 3], ['base-dano-c2-repentina', 4], ['base-comprar-c1', 4], ['base-comprar-c3', 4],
      [A(1), 4], [A(2, 'agressivo'), 4], [A(2, 'agressivo-proficiente'), 4], [A(3), 4], ['base-recurso', 4]]),
  },
  {
    nome: 'Stall',
    estilo: { nome: 'Stall', agressividade: 0.2, reservaInstantes: false, grauAlvo: 7, queimaFace: false },
    lista: completar([['base-mistica-c2', 4], ['base-mistica-c4', 4], ['base-estrutura-c1', 4], ['base-estrutura-c3', 4], ['base-curar-c2', 4], ['base-curar-c4', 4], [A(2, 'defensivo'), 4], [A(4, 'defensivo'), 4], [A(6, 'defensivo'), 4], [A(8), 4],
      ['base-comprar-c3', 4], ['base-dano-c2', 3], ['base-destruir-c3', 3], [A(3), 3], ['base-recurso', 3]]),
  },
  {
    nome: 'Ramp',
    estilo: { nome: 'Ramp', agressividade: 0.5, reservaInstantes: false, grauAlvo: 9, queimaFace: false },
    lista: completar([['base-recurso', 8], ['base-estrutura-c1', 4], ['base-estrutura-c3', 4], [A(5), 3], [A(6), 4], [A(6, 'equilibrado-descarta-grau'), 4], [A(7), 4], [A(8), 4], [A(8, 'equilibrado-descarta-grau'), 4], [A(4, 'equilibrado-grau5'), 3],
      ['base-comprar-c3', 4], ['base-destruir-c3', 3], ['base-dano-c4', 3], [A(2, 'defensivo'), 4]]),
  },
  {
    nome: 'Tempo',
    estilo: { nome: 'Tempo', agressividade: 0.8, reservaInstantes: true, grauAlvo: 5, queimaFace: false },
    lista: completar([[A(1), 4], [A(2, 'agressivo'), 4], [A(2, 'agressivo-proficiente'), 4], [A(3), 4], [A(4, 'agressivo'), 3], [A(4, 'agressivo-proficiente'), 4], [A(5), 3],
      ['base-dano-c1-repentina', 4], ['base-dano-c2-repentina', 4], ['base-dano-c3-repentina', 4], ['base-comprar-c2-repentina', 4], ['base-comprar-c1', 3], ['base-destruir-c4-repentina', 3], ['base-dano-conjurador-c3', 2], ['base-recurso', 4]]),
  },
  {
    nome: 'Mistura',
    estilo: { nome: 'Mistura', agressividade: 0.5, reservaInstantes: false, grauAlvo: 6, queimaFace: false },
    // uma de cada carta do catálogo, repetidas até 60
    lista: completar(Object.keys(catalogoBase.cartas).map((id): [string, number] => [id, 1]).slice(0, 41), 'base-aliado-c3-equilibrado'),
  },
];

export function baralhoDe(a: Arquetipo, conjurador: string): BaralhoDef {
  const cartas = a.lista.flatMap(([id, n]) => Array.from({ length: n }, () => id));
  if (cartas.length !== 60) throw new Error(`${a.nome} tem ${cartas.length} cartas`);
  return { conjurador, cartas, itens: a.itens ?? [] };
}
