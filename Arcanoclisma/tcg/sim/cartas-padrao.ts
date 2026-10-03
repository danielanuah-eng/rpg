// Gera docs/CARTAS-<VARIANTE>.md: cartas e baralhos de uma variante (padrão: PADRAO, o que o autor aprovou).
// Uso: node sim/cartas-padrao.ts [variante]   (ex.: node sim/cartas-padrao.ts BAL → docs/CARTAS-BAL.md)
import { writeFileSync } from 'node:fs';
import { bonusGrau, pontos, orcamento } from '../src/balanco.ts';
import { rodada3 } from './lista.ts';
import { arquetiposDe, catalogoDe, precosDe } from './variantes.ts';

const id = process.argv[2] ?? 'PADRAO';
const v = rodada3.find((x) => x.id === id);
if (!v) throw new Error(`Variante desconhecida: ${id}`);
const catalogo = catalogoDe(v);
const p = precosDe(v);
const arqs = arquetiposDe(v, catalogo);
const usadas = new Set(arqs.flatMap((a) => a.lista.map(([id]) => id)));
const curva = v.r6CurvaGrau > 0;

const texto = (c: (typeof catalogo.cartas)[string]): string => {
  const partes: string[] = [];
  if (c.descartaGrau) partes.push(`Para jogar, descarte ${c.descartaGrau} carta${c.descartaGrau > 1 ? 's' : ''} do seu Grau.`);
  if (c.texto) partes.push(c.texto);
  else if (c.palavrasChave?.length) partes.push(`${c.palavrasChave.join(', ')}.`);
  return partes.join(' ');
};
/** Orçamento da carta; na variante com curva de Grau, os Aliados somam o bônus de Grau. */
const orc = (c: (typeof catalogo.cartas)[string]): number =>
  orcamento(c.custo, c.arcanoGerado, p) + (curva && c.tipo === 'aliado' && !c.ativacao ? bonusGrau(c.grau, v.r6CurvaGrau) : 0);

const arquivo = id === 'PADRAO' ? 'CARTAS-PADRAO.md' : `CARTAS-${id}.md`;
let md = id === 'PADRAO'
  ? `# Cartas e baralhos do padrão\n\nGerado por \`node sim/cartas-padrao.ts\` a partir da variante **PADRAO** (sim/lista.ts). Conjurador: Vida ${v.conjVida}, ataque grátis.\n\n`
  : `# Cartas e baralhos da variante ${id}\n\nGerado por \`node sim/cartas-padrao.ts ${id}\` (sim/lista.ts): ${v.nome}. Conjurador: Vida ${v.conjVida}, ataque grátis; o segundo jogador compra ${v.segundoCarta} carta(s) a mais.\n\n`;
md += `PP = pontos de poder da carta; Orçamento = o que o custo permite${curva ? ` (Aliados: mais o bônus da curva de Grau, k × G × (G − 1) / 2 com k = ${v.r6CurvaGrau} e G até 8)` : ''}. Cartas sem uso nos baralhos de teste ficam marcadas com *.\n\n`;
md += '| Carta | Tipo | Custo | Grau | Velocidade | Ataque/Vida | Texto | PP / Orçamento |\n|---|---|---|---|---|---|---|---|\n';
const ordem = Object.values(catalogo.cartas).sort((a, b) => a.tipo.localeCompare(b.tipo) || a.custo - b.custo || a.id.localeCompare(b.id));
for (const c of ordem) {
  const corpo = c.ataque !== undefined || c.vida !== undefined ? `${c.ataque ?? '–'}/${c.vida ?? '–'}` : '';
  md += `| ${usadas.has(c.id) ? '' : '*'}${c.nome} | ${c.tipo} | ${c.custo} | ${c.grau} | ${c.velocidade} | ${corpo} | ${texto(c).replace(/\|/g, '/')} | ${pontos(c, p).toFixed(1)} / ${orc(c).toFixed(1)} |\n`;
}
md += '\n## Baralhos de teste (60 cartas)\n\nNão são listas obrigatórias: são as montagens que os agentes usam para medir o balanceamento.\n';
for (const a of arqs) {
  md += `\n### ${a.nome}\n\n`;
  const somada = new Map<string, number>();
  for (const [id, n] of a.lista) somada.set(id, (somada.get(id) ?? 0) + n);
  md += [...somada].filter(([, n]) => n > 0).sort((x, y) => y[1] - x[1]).map(([id, n]) => `${n}× ${catalogo.cartas[id].nome}`).join(' · ') + '\n';
}
writeFileSync(new URL(`../docs/${arquivo}`, import.meta.url), md);
console.log(`docs/${arquivo}: ${ordem.length} cartas, ${arqs.length} baralhos.`);
