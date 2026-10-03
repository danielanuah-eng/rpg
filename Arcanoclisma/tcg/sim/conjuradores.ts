// Mede os 9 Conjuradores (variante CONJ) todos contra todos.
// Uso: node sim/conjuradores.ts [partidas por confronto ordenado, padrão 40] [--saida NOME] [--baralhos arquivo.json]
// Escreve docs/CONJURADORES-<NOME>.md (padrão: docs/CONJURADORES.md) e data/conjuradores-<nome>.json.
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { validarBaralho } from '../src/motor.ts';
import { baralhoDe } from './baralhos.ts';
import { rodada3 } from './lista.ts';
import { contextoDe, partida, type Contexto } from './nucleo.ts';
import { semDecisao } from './valores-cartas.ts';

const args = process.argv.slice(2);
const arg = (n: string, p?: string) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : p; };
const porPar = Number(args.find((a) => /^\d+$/.test(a)) ?? 40);
const saida = arg('--saida');
const arqBaralhos = arg('--baralhos');
const idVariante = arg('--variante', 'CONJ')!;

export function carregar(): Contexto {
  const v = rodada3.find((x) => x.id === idVariante);
  if (!v) throw new Error(`Variante desconhecida: ${idVariante}`);
  const ctx = contextoDe(v);
  if (arqBaralhos) {
    const cands = (JSON.parse(readFileSync(arqBaralhos.startsWith('.') || arqBaralhos.includes(':') ? arqBaralhos : fileURLToPath(new URL('./' + arqBaralhos, import.meta.url)), 'utf8')) as { cands: Record<string, { lista: Array<[string, number]>; estilo: Contexto['arqs'][number]['estilo'] }> }).cands;
    ctx.arqs = ctx.arqs.map((a) => (cands[a.nome] ? { ...a, lista: cands[a.nome].lista, estilo: cands[a.nome].estilo } : a));
  }
  return ctx;
}

const ctx = carregar();
const nomes = ctx.arqs.map((a) => a.nome);
const n = nomes.length;

// 1. legalidade
const problemas: string[] = [];
for (const a of ctx.arqs) {
  const erros = validarBaralho(ctx.catalogo, baralhoDe(a, a.conjurador!));
  for (const er of erros) problemas.push(`${a.nome}: ${er}`);
}
const esquecidas = semDecisao(ctx.catalogo);
if (problemas.length) { console.log('BARALHOS ILEGAIS:\n' + problemas.join('\n')); process.exit(1); }
if (esquecidas.length) console.log('Cartas sem decisão de Valor:', esquecidas.join(', '));

// 2. partidas
const vit = Array.from({ length: n }, () => Array(n).fill(0));
const jog = Array.from({ length: n }, () => Array(n).fill(0));
const turnosPar = Array.from({ length: n }, () => Array(n).fill(0));
const hab = nomes.map(() => ({ usos: 0, partidas: 0 }));
const fins: Record<string, number> = {};
let semente = 4242, decididas = 0, vitPrimeiro = 0, turnosTotal = 0, total = 0;
const porVencedorLongo = nomes.map(() => ({ longas: 0, vencidas: 0 }));
for (let a = 0; a < n; a++) {
  for (let b = 0; b < n; b++) {
    if (a === b) continue;
    for (let k = 0; k < porPar; k++) {
      const r = partida(ctx, a, b, semente++);
      total++;
      fins[r.fim] = (fins[r.fim] ?? 0) + 1;
      hab[a].usos += r.extras[0].habilidade; hab[a].partidas++;
      hab[b].usos += r.extras[1].habilidade; hab[b].partidas++;
      turnosTotal += r.turnos;
      if (r.vencedor === null) continue;
      decididas++;
      if (r.vencedor === r.primeiro) vitPrimeiro++;
      jog[a][b]++; jog[b][a]++;
      turnosPar[a][b] += r.turnos; turnosPar[b][a] += r.turnos;
      if (r.vencedor === 0) vit[a][b]++; else vit[b][a]++;
      if (r.turnos >= 18) for (const [i, lado] of [[a, 0], [b, 1]] as const) { porVencedorLongo[i].longas++; if (r.vencedor === lado) porVencedorLongo[i].vencidas++; }
    }
  }
}
const taxa = (i: number, j: number) => (jog[i][j] ? vit[i][j] / jog[i][j] : NaN);
const geral = nomes.map((_, i) => { const t = nomes.map((_, j) => (j === i ? NaN : taxa(i, j))).filter((x) => !Number.isNaN(x)); return t.reduce((s, x) => s + x, 0) / t.length; });
const pct = (x: number) => (Number.isNaN(x) ? '—' : `${(100 * x).toFixed(1)}%`);
const duracaoMedia = turnosTotal / total;

// 3. relatório
const tag = saida ? `-${saida}` : '';
let md = `# Conjuradores por par de Valores: simulação\n\nVariante ${idVariante}; ${total} partidas (${porPar} por confronto ordenado); duração média ${duracaoMedia.toFixed(1)} turnos; quem começa vence ${pct(vitPrimeiro / decididas)}; fim: ${Object.entries(fins).map(([k, q]) => `${k} ${(100 * q / total).toFixed(1)}%`).join(', ')}.\n\n`;
md += '## Vitória média e uso da habilidade\n\n| Conjurador | Valores | Vitória média | Vence partidas longas (18+) | Habilidade por partida |\n|---|---|---|---|---|\n';
const defs = ctx.arqs.map((a) => ctx.catalogo.conjuradores[a.conjurador!]);
const ordem = nomes.map((_, i) => i).sort((x, y) => geral[y] - geral[x]);
for (const i of ordem) {
  const l = porVencedorLongo[i];
  md += `| ${nomes[i]} | ${defs[i].valores.join(' + ')} | ${pct(geral[i])} | ${pct(l.longas ? l.vencidas / l.longas : NaN)} (${l.longas}) | ${(hab[i].usos / hab[i].partidas).toFixed(2)} |\n`;
}
md += '\n## Matriz (linha contra coluna)\n\n| | ' + nomes.join(' | ') + ' |\n|' + '---|'.repeat(n + 1) + '\n';
for (let i = 0; i < n; i++) md += `| **${nomes[i]}** | ${nomes.map((_, j) => (i === j ? '—' : pct(taxa(i, j)))).join(' | ')} |\n`;
md += '\n## Duração média por confronto (turnos)\n\n| | ' + nomes.join(' | ') + ' |\n|' + '---|'.repeat(n + 1) + '\n';
for (let i = 0; i < n; i++) md += `| **${nomes[i]}** | ${nomes.map((_, j) => (i === j || !jog[i][j] ? '—' : (turnosPar[i][j] / jog[i][j]).toFixed(1))).join(' | ')} |\n`;
md += '\n## Habilidades\n\n' + defs.map((d) => `- **${d.nome}** (${d.valores.join(' + ')}): ${d.habilidade}`).join('\n') + '\n';
writeFileSync(new URL(`../docs/CONJURADORES${tag.toUpperCase()}.md`, import.meta.url), md);
mkdirSync(new URL('../data/', import.meta.url), { recursive: true });
writeFileSync(new URL(`../data/conjuradores${tag}.json`, import.meta.url), JSON.stringify({ variante: idVariante, nomes, geral, matriz: nomes.map((_, i) => nomes.map((_, j) => (i === j ? null : taxa(i, j)))), duracao: duracaoMedia, primeiro: vitPrimeiro / decididas }, null, 1));
console.log(`Variante ${idVariante}: ${total} partidas, ${duracaoMedia.toFixed(1)} turnos, primeiro vence ${pct(vitPrimeiro / decididas)}`);
for (const i of ordem) console.log(`${nomes[i].padEnd(26)} ${pct(geral[i]).padStart(6)}  habilidade/partida ${(hab[i].usos / hab[i].partidas).toFixed(2)}`);
