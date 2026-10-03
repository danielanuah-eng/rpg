// Duração por confronto e como as partidas terminam: node sim/duracao.ts [variante] [partidas por par]
import { contextoDe, partida } from './nucleo.ts';
import { rodada3 } from './lista.ts';

const id = process.argv[2] ?? 'R5';
const n = Number(process.argv[3] ?? 20);
const v = rodada3.find((x) => x.id === id);
if (!v) throw new Error(`Variante desconhecida: ${id}`);
const ctx = contextoDe(v);
const nomes = ctx.arqs.map((a) => a.nome);
const fins: Record<string, number> = {};
const porMontagem = nomes.map(() => ({ turnos: 0, jogos: 0 }));
const linhas: string[] = [];
const perdeImpasse = nomes.map(() => 0);
let semente = 777;
let total = 0, jogos = 0;
for (let a = 0; a < nomes.length; a++) {
  const cel: string[] = [];
  for (let b = 0; b < nomes.length; b++) {
    let t = 0;
    for (let k = 0; k < n; k++) {
      const r = partida(ctx, a, b, semente++);
      t += r.turnos;
      fins[r.fim] = (fins[r.fim] ?? 0) + 1;
      if (r.fim === 'impasse' && r.vencedor !== null) perdeImpasse[r.vencedor === 0 ? b : a]++;
      porMontagem[a].turnos += r.turnos; porMontagem[a].jogos++;
      porMontagem[b].turnos += r.turnos; porMontagem[b].jogos++;
    }
    total += t; jogos += n;
    cel.push((t / n).toFixed(1));
  }
  linhas.push(`| **${nomes[a]}** | ${cel.join(' | ')} |`);
}
console.log(`Variante ${id}: ${jogos} partidas, ${(total / jogos).toFixed(1)} turnos de média.`);
console.log('Fim:', Object.entries(fins).map(([k, q]) => `${k} ${(100 * q / jogos).toFixed(1)}%`).join(', '));
console.log('Perdem por impasse:', nomes.map((x, i) => `${x} ${perdeImpasse[i]}`).join(', '));
console.log('Média por montagem:', nomes.map((x, i) => `${x} ${(porMontagem[i].turnos / porMontagem[i].jogos).toFixed(1)}`).join(', '));
console.log(`| | ${nomes.join(' | ')} |\n|${'---|'.repeat(nomes.length + 1)}\n${linhas.join('\n')}`);
