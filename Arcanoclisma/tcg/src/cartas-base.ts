// Cartas BÁSICAS de referência (neutras, sem Valor, sem arte, nomes provisórios).
// Cada carta é gerada pela régua de src/balanco.ts, então serve de ponto de comparação para cartas novas.
import { grauPadrao, orcamento, PRECO } from './balanco.ts';
import type { CartaDef, Catalogo, Efeito } from './tipos.ts';

type Perfil = 'equilibrado' | 'agressivo' | 'defensivo';
const FRACAO_VIDA: Record<Perfil, number> = { equilibrado: 0.55, agressivo: 0.4, defensivo: 0.7 };

const molde = { subtipos: [], valores: [], arcanoGerado: 1, cenario: '', texto: '' } as const;

interface Extras {
  grau?: number;
  descartaGrau?: number;
  repentina?: boolean;
  proficiente?: boolean;
}

function folgaDe(custo: number, e: Extras): number {
  // PP disponíveis para o corpo/efeito da carta, depois de pagar as palavras-chave e somar o que o Grau e o descarte liberam.
  const grau = e.grau ?? grauPadrao(custo);
  return (
    orcamento(custo) -
    (e.repentina ? PRECO.repentina : 0) -
    (e.proficiente ? PRECO.proficiente : 0) +
    PRECO.descartaGrau * (e.descartaGrau ?? 0) +
    (grau - grauPadrao(custo))
  );
}

function comum(id: string, nome: string, custo: number, e: Extras) {
  return {
    ...molde,
    id,
    nome,
    custo,
    grau: e.grau ?? grauPadrao(custo),
    velocidade: e.repentina ? ('repentina' as const) : ('findante' as const),
    ...(e.descartaGrau ? { descartaGrau: e.descartaGrau } : {}),
    ...(e.proficiente ? { palavrasChave: ['Proficiente'] } : {}),
  };
}

export function aliado(custo: number, perfil: Perfil = 'equilibrado', e: Extras = {}): CartaDef {
  const total = folgaDe(custo, e);
  const vida = Math.max(1, Math.ceil(total * FRACAO_VIDA[perfil]));
  const sufixo = [perfil, e.proficiente ? 'proficiente' : '', e.descartaGrau ? 'descarta-grau' : '', e.grau !== undefined ? `grau${e.grau}` : '']
    .filter(Boolean)
    .join('-');
  return {
    ...comum(`base-aliado-c${custo}-${sufixo}`, `Aliado básico (custo ${custo}, ${sufixo.replaceAll('-', ' ')})`, custo, e),
    tipo: 'aliado',
    ataque: Math.max(0, total - vida),
    vida,
  };
}

export function danoAlvo(custo: number, e: Extras = {}): CartaDef {
  const dano = Math.floor(folgaDe(custo, e) / PRECO.danoAlvo);
  return {
    ...comum(`base-dano-c${custo}${e.repentina ? '-repentina' : ''}`, `Dano básico (custo ${custo}${e.repentina ? ', repentina' : ''})`, custo, e),
    tipo: 'temporaria',
    efeitos: [{ tipo: 'dano', valor: dano, alvo: 'escolhido' }],
    texto: `Causa ${dano} de dano a um alvo.`,
  };
}

export function danoConjurador(custo: number, e: Extras = {}): CartaDef {
  const dano = Math.floor(folgaDe(custo, e) / PRECO.danoConjurador);
  return {
    ...comum(`base-dano-conjurador-c${custo}`, `Dano ao Conjurador (custo ${custo})`, custo, e),
    tipo: 'temporaria',
    efeitos: [{ tipo: 'dano', valor: dano, alvo: 'conjuradorInimigo' }],
    texto: `Causa ${dano} de dano ao Conjurador inimigo.`,
  };
}

export function comprarCartas(custo: number, e: Extras = {}): CartaDef {
  const n = Math.max(1, Math.floor(folgaDe(custo, e) / PRECO.comprar));
  return {
    ...comum(`base-comprar-c${custo}${e.repentina ? '-repentina' : ''}`, `Comprar cartas (custo ${custo}${e.repentina ? ', repentina' : ''})`, custo, e),
    tipo: 'temporaria',
    efeitos: [{ tipo: 'comprar', valor: n }],
    texto: `Compre ${n} carta${n > 1 ? 's' : ''}.`,
  };
}

export function curar(custo: number, e: Extras = {}): CartaDef {
  const v = Math.floor(folgaDe(custo, e) / PRECO.curar);
  return {
    ...comum(`base-curar-c${custo}`, `Cura básica (custo ${custo})`, custo, e),
    tipo: 'temporaria',
    efeitos: [{ tipo: 'curar', valor: v, alvo: 'escolhido' }],
    texto: `Cure ${v} de dano de um alvo.`,
  };
}

export function destruir(custo: number, e: Extras = {}): CartaDef {
  const ef: Efeito = { tipo: 'destruir', alvo: 'escolhido' };
  return {
    ...comum(`base-destruir-c${custo}${e.repentina ? '-repentina' : ''}`, `Destruir (custo ${custo}${e.repentina ? ', repentina' : ''})`, custo, e),
    tipo: 'temporaria',
    efeitos: [ef],
    texto: 'Destrua um Aliado, Mística ou Estrutura.',
  };
}

export function mistica(custo: number, e: Extras = {}): CartaDef {
  return {
    ...comum(`base-mistica-c${custo}`, `Mística básica (custo ${custo})`, custo, e),
    tipo: 'mistica',
    vida: Math.max(1, folgaDe(custo, e)),
  };
}

export function estruturaDeRecurso(custo: number, e: Extras = {}): CartaDef {
  const aoEntrar: Efeito[] = custo >= 3 ? [{ tipo: 'comprar', valor: 1 }, { tipo: 'arcano', valor: 1 }] : [{ tipo: 'arcano', valor: 1 }];
  const gasto = aoEntrar.reduce((s, ef) => s + (ef.tipo === 'comprar' ? PRECO.comprar : PRECO.arcano), 0);
  return {
    ...comum(`base-estrutura-c${custo}`, `Estrutura básica (custo ${custo})`, custo, e),
    tipo: 'estrutura',
    vida: Math.max(0, folgaDe(custo, e) - gasto),
    aoEntrar,
    texto: 'Ao entrar: ' + aoEntrar.map((ef) => (ef.tipo === 'comprar' ? 'compre 1 carta' : 'ganhe 1 Arcano')).join(' e ') + '.',
  };
}

/** Carta de pura geração de Arcano: não faz nada, mas rende 3 de pitch. */
export function recurso(): CartaDef {
  return { ...molde, id: 'base-recurso', nome: 'Recurso básico', tipo: 'estrutura', custo: 0, grau: 0, velocidade: 'findante', arcanoGerado: 3, vida: 0 };
}

export const cartasBase: CartaDef[] = [
  recurso(),
  // Aliados equilibrados em toda a curva
  ...[1, 2, 3, 4, 5, 6, 7, 8].map((c) => aliado(c)),
  // Perfis agressivo e defensivo no early, mid e late
  ...[2, 4, 6].flatMap((c) => [aliado(c, 'agressivo'), aliado(c, 'defensivo')]),
  // Proficientes (atacam ao entrar)
  aliado(2, 'agressivo', { proficiente: true }),
  aliado(4, 'agressivo', { proficiente: true }),
  // Cartas "poderosas": descartam Grau ou pedem Grau acima do normal
  aliado(6, 'equilibrado', { descartaGrau: 1 }),
  aliado(8, 'equilibrado', { descartaGrau: 2 }),
  aliado(4, 'equilibrado', { grau: 5 }),
  // Temporárias
  ...[1, 2, 3, 4].flatMap((c) => [danoAlvo(c), danoAlvo(c, { repentina: true })]),
  danoConjurador(3),
  danoConjurador(5),
  comprarCartas(1),
  comprarCartas(3),
  comprarCartas(2, { repentina: true }),
  curar(2),
  curar(4),
  destruir(3),
  destruir(4, { repentina: true }),
  // Místicas e Estruturas
  mistica(2),
  mistica(4),
  estruturaDeRecurso(1),
  estruturaDeRecurso(3),
];

export const catalogoBase: Catalogo = {
  cartas: Object.fromEntries(cartasBase.map((c) => [c.id, c])),
  conjuradores: {},
};
