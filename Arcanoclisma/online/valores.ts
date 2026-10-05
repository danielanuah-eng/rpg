// Valores das cartas do site (pedido do autor, 03/10/2026, noite). Só a coleção do site: as simulações não mudam.
// - Não existe carta neutra: toda carta (menos itens) pertence a um Valor, a cor da bolinha do custo.
// - Normalmente 1 Valor; 1 a cada 10 cartas tem 2 (o par de um Conjurador); 1 a cada 50 tem 3.
// - A Mina Funda (recurso básico) vira 9 minas, uma por Valor, como terrenos básicos.
// - Os baralhos prontos seguem a regra das cores (decisão 37: 2 Valores = exatamente o par do Conjurador; 3+ = o par
//   está entre os da carta): a carta que deixa de servir é trocada por outra dos
//   Valores dele, do mesmo tipo e de custo próximo.
// Escolha do Valor: o tema que a carta já tinha (seus Valores antigos), o uso nos baralhos prontos e o equilíbrio entre os 9.
import type { CartaDef, Catalogo } from '../tcg/src/tipos.ts';
import { cartaServeConjurador, corrigirCores } from '../tcg/src/motor.ts';

type Lista = [string, number][];
interface Pronto { id: string; conjurador: string; nome: string; lista: Lista; itens: string[] }

export const RECURSO = 'base-recurso';
/** Nome de cada mina pelo lugar do Valor. */
const LUGAR: Record<string, string> = {
  conflituo: 'do Campo sem Trégua', sublimeyn: 'do Mar de Mercúrio', dominum: 'da Cidade-Muralha', persona: 'da Cidade dos Rostos',
  alvorum: 'do Farol sem Noite', morbidyum: 'do Cemitério que Respira', cognitium: 'do Arquivo sem Dono', eternyr: 'do Entretempo', primitivum: 'da Floresta Primeira',
};
const hash = (t: string) => { let h = 2166136261; for (const ch of t) h = Math.imul(h ^ ch.charCodeAt(0), 16777619); return h >>> 0; };

export function aplicarValoresDoSite(catalogo: Catalogo, baralhos: Pronto[], visual: Record<string, { arte: string | null; raridade: string | null; nomeMecanico?: string }>) {
  const VALORES = [...new Set(Object.values(catalogo.conjuradores).flatMap((c) => c.valores))].sort();
  const conjV = (b: Pronto) => catalogo.conjuradores[b.conjurador].valores;
  const cartas = Object.values(catalogo.cartas).filter((c) => c.tipo !== 'item' && !c.ficha && c.id !== RECURSO);
  // uso[carta][valor]: cópias nos baralhos prontos de Conjuradores com esse Valor (onde a carta serve hoje)
  const uso: Record<string, Record<string, number>> = {};
  for (const b of baralhos) for (const [id, n] of b.lista) {
    const c = catalogo.cartas[id];
    if (!c || c.tipo === 'item') continue;
    for (const v of conjV(b)) if (!c.valores.length || c.valores.includes(v)) (uso[id] ??= {})[v] = (uso[id]?.[v] ?? 0) + n;
  }
  const novos: Record<string, string[]> = {};
  const conta: Record<string, number> = Object.fromEntries(VALORES.map((v) => [v, 0]));
  const N = cartas.length;
  // 1) cartas de 3 Valores (1 a cada 50): as que mais têm Valores, pelos 3 Valores mais usados
  const n3 = Math.max(1, Math.round(N / 50));
  for (const c of [...cartas].filter((c) => c.valores.length >= 3).sort((a, b) => b.valores.length - a.valores.length || hash(a.id) - hash(b.id)).slice(0, n3)) {
    novos[c.id] = [...c.valores].sort((a, b) => (uso[c.id]?.[b] ?? 0) - (uso[c.id]?.[a] ?? 0) || a.localeCompare(b)).slice(0, 3);
    for (const v of novos[c.id]) conta[v] += 1 / 3;
  }
  // 2) cartas de 2 Valores (1 a cada 10): o par de um Conjurador em cujo baralho a carta mais aparece
  const n2 = Math.round(N / 10);
  const porPar: Record<string, number> = {};
  const cand: { id: string; par: string[]; n: number }[] = [];
  for (const b of baralhos) {
    const par = conjV(b);
    for (const [id, n] of b.lista) { const c = catalogo.cartas[id]; if (c && c.tipo !== 'item' && id !== RECURSO && par.every((v) => c.valores.includes(v))) cand.push({ id, par, n }); }
  }
  cand.sort((a, b) => b.n - a.n || hash(a.id) - hash(b.id));
  for (const x of cand) {
    if (Object.keys(novos).length >= n2 + n3) break;
    const k = x.par.join('+');
    if (novos[x.id] || (porPar[k] ?? 0) >= 2) continue;
    novos[x.id] = [...x.par]; porPar[k] = (porPar[k] ?? 0) + 1;
    for (const v of x.par) conta[v] += 0.5;
  }
  // 3) o resto: 1 Valor, entre os que a carta já tinha (sem Valor: qualquer um), pelo uso e pelo equilíbrio
  const alvo = N / VALORES.length;
  const resto = cartas.filter((c) => !novos[c.id]).sort((a, b) => (a.valores.length || 9) - (b.valores.length || 9) || hash(a.id) - hash(b.id));
  for (const c of resto) {
    const opcoes = c.valores.length ? c.valores : VALORES;
    let melhor = opcoes[0], nota = -Infinity;
    for (const v of opcoes) {
      const s = (uso[c.id]?.[v] ?? 0) * 2 - Math.max(0, conta[v] - alvo) * 3 - conta[v] * 0.1 + (v === c.valores[0] ? 0.5 : 0);
      if (s > nota) { nota = s; melhor = v; }
    }
    novos[c.id] = [melhor]; conta[melhor] += 1;
  }
  const mudancas: string[] = [];
  for (const c of cartas) {
    if (c.valores.join() !== novos[c.id].join()) mudancas.push(`${c.nome}: ${c.valores.join('+') || 'sem Valor'} → ${novos[c.id].join('+')}`);
    catalogo.cartas[c.id] = { ...c, valores: novos[c.id] };
  }
  // 4) as 9 minas: a original fica com um Valor e ganha 8 irmãs (mesma mecânica, nome do lugar de cada Valor)
  const base = catalogo.cartas[RECURSO];
  const minas: Record<string, string> = {};
  for (const v of VALORES) {
    const id = v === VALORES[0] ? RECURSO : `${RECURSO}--${v}`;
    catalogo.cartas[id] = { ...base, id, valores: [v], nome: `Mina Funda ${LUGAR[v] ?? v}` } as CartaDef;
    visual[id] = { ...visual[RECURSO], nomeMecanico: base.nome };
    minas[v] = id;
  }
  // 5) baralhos prontos: só cartas dos Valores do Conjurador
  const trocas: string[] = [], trocasCores: string[] = [];
  for (const b of baralhos) {
    const V = conjV(b);
    const qtd = new Map<string, number>();
    const add = (id: string, n: number) => qtd.set(id, (qtd.get(id) ?? 0) + n);
    const serve = (id: string) => cartaServeConjurador(catalogo.cartas[id], catalogo.conjuradores[b.conjurador]);
    const minasN = b.lista.filter(([id]) => id === RECURSO).reduce((s, [, n]) => s + n, 0);
    for (const [id, n] of b.lista) if (id !== RECURSO && (catalogo.cartas[id].tipo === 'item' || serve(id))) add(id, n);
    // minas: metade (para cima) do Valor com mais cartas no baralho, o resto do outro
    const peso = (v: string) => [...qtd].reduce((s, [id, n]) => s + (catalogo.cartas[id].valores.includes(v) ? n : 0), 0);
    const [vMaior, vMenor] = [...V].sort((a, c) => peso(c) - peso(a));
    if (minasN) { add(minas[vMaior], Math.ceil(minasN / 2)); if (minasN > 1) add(minas[vMenor], Math.floor(minasN / 2)); }
    // regra das cores (decisão 37): o que não serve vira carta do mesmo tipo e custo próximo (a mesma função do botão
    // "Corrigir automaticamente" do construtor), sem Minas Fundas e até 3 cópias
    const ruins: Lista = b.lista.filter(([id]) => id !== RECURSO && catalogo.cartas[id].tipo !== 'item' && !serve(id));
    const r = corrigirCores(catalogo, b.conjurador, [...qtd, ...ruins], { max: 3, semente: b.id, aceitar: (x) => !x.id.startsWith(RECURSO) });
    if (r.semSubstituto.length) throw new Error(`Baralho pronto ${b.id}: sem carta dos Valores ${V.join('+')} para trocar ${r.semSubstituto.join(', ')}`);
    for (const t of r.trocas) {
      const de = catalogo.cartas[t.de];
      const nova = de.valores.some((v) => V.includes(v)); // só saiu pela decisão 37 (2+ Valores sem o par do Conjurador)
      (nova ? trocasCores : trocas).push(`${b.id}: ${de.nome} (${de.valores.join('+')}) → ${catalogo.cartas[t.para].nome}`);
    }
    qtd.clear();
    for (const [id, n] of r.lista) add(id, n);
    b.lista = [...qtd].filter(([, n]) => n > 0);
  }
  const porValor = Object.fromEntries(VALORES.map((v) => [v, cartas.filter((c) => catalogo.cartas[c.id].valores.includes(v)).length]));
  const porQtd: Record<number, number> = {};
  for (const c of cartas) porQtd[novos[c.id].length] = (porQtd[novos[c.id].length] ?? 0) + 1;
  return { minas, mudancas, trocas, trocasCores, porValor, porQtd };
}
