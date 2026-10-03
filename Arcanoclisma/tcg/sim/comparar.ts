// Roda todas as variantes (uma por vez e em combinação) e escreve docs/COMPARACAO-VARIANTES.md.
// Uso: node sim/comparar.ts [partidasPorPar=40]
import { writeFileSync } from 'node:fs';
import { contextoDe, LIMITE_TURNOS, partida, type Jog, type Resultado } from './nucleo.ts';
import { variante, type Variante } from './variantes.ts';
import { rodada3 } from './lista.ts';

const BURN1 = { burn: 'custoGrau' as const };
const BURN2 = { burn: 'pagarX' as const };
const H1 = { conjAtaca: true };
const H5 = { conjAtaca: true, conjDano: 2, conjCusto: 0, grauSemSuspender: true };
const CARTAS = { recursos: true, eternyr: true, estruturas: true, cura: true };
const REGRAS = { semAcaoPerde: true, primeiroSemGrau: true };

export const rodada2: Variante[] = [
  variante('base', 'Base (sem mudanças)', {}),
  // isoladas
  variante('A1', 'A1 Burn: +2 de custo e de Grau', BURN1),
  variante('A2', 'A2 Burn: oponente paga X para anular', BURN2),
  variante('B', 'B Mais cartas de recurso', { recursos: true }),
  variante('C', 'C Eternyr: mais draw, mais barato', { eternyr: true }),
  variante('D', 'D Estruturas com efeito', { estruturas: true }),
  variante('E', 'E Cura melhor e escalável', { cura: true }),
  variante('F', 'F Sem ações perde', { semAcaoPerde: true }),
  variante('G', 'G Quem começa sem Grau no turno 1', { primeiroSemGrau: true }),
  variante('H1', 'H1 Conjurador ataca (só o bot muda)', H1),
  variante('H2', 'H2 Conjurador ataca + dano básico 2', { ...H1, conjDano: 2 }),
  variante('H3', 'H3 Conjurador ataca + ataque grátis', { ...H1, conjCusto: 0 }),
  variante('H4', 'H4 Conjurador ataca + Grau sem suspender', { ...H1, grauSemSuspender: true }),
  variante('H5', 'H5 Conjurador: dano 2, grátis, Grau sem suspender', H5),
  // ajustes finos (rodada 2)
  variante('A3', 'A3 Burn anulável: X = custo', { ...BURN2, pagarXDelta: 0 }),
  variante('A4', 'A4 Burn anulável: X = custo + 1', { ...BURN2, pagarXDelta: 1 }),
  variante('A5', 'A5 Burn anulável: X = custo + 2', { ...BURN2, pagarXDelta: 2 }),
  variante('G2', 'G2 G + segundo sem Arcano inicial', { primeiroSemGrau: true, segundoArcano: 0 }),
  variante('G3', 'G3 G + segundo sem carta extra', { primeiroSemGrau: true, segundoCarta: 0 }),
  variante('G4', 'G4 G + segundo sem Arcano nem carta extra', { primeiroSemGrau: true, segundoArcano: 0, segundoCarta: 0 }),
  // combinações
  variante('A1+G', 'A1 + G', { ...BURN1, ...REGRAS, semAcaoPerde: false }),
  variante('A2+G', 'A2 + G', { ...BURN2, primeiroSemGrau: true }),
  variante('B+D', 'B + D', { recursos: true, estruturas: true }),
  variante('D+E', 'D + E', { estruturas: true, cura: true }),
  variante('A2+D+E', 'A2 + D + E', { ...BURN2, estruturas: true, cura: true }),
  variante('A2+D+E+G', 'A2 + D + E + G', { ...BURN2, estruturas: true, cura: true, primeiroSemGrau: true }),
  variante('CartasSemBurn', 'B+C+D+E+F+G (burn sem mudança)', { ...CARTAS, ...REGRAS }),
  variante('K1', 'K1 Tudo com A1', { ...BURN1, ...CARTAS }),
  variante('K2', 'K2 Tudo com A2', { ...BURN2, ...CARTAS }),
  variante('K3', 'K3 K1 + F + G', { ...BURN1, ...CARTAS, ...REGRAS }),
  variante('K4', 'K4 K2 + F + G', { ...BURN2, ...CARTAS, ...REGRAS }),
  variante('K4+H1', 'K4 + H1 (bot ataca com Conjurador)', { ...BURN2, ...CARTAS, ...REGRAS, ...H1 }),
  variante('K3+H5', 'K3 + H5', { ...BURN1, ...CARTAS, ...REGRAS, ...H5 }),
  variante('K4+H5', 'K4 + H5', { ...BURN2, ...CARTAS, ...REGRAS, ...H5 }),
  // candidatas (rodada 2)
  variante('R1', 'R1 cartas B+C+D+E + A4 + G2', { ...CARTAS, ...BURN2, pagarXDelta: 1, primeiroSemGrau: true, segundoArcano: 0, semAcaoPerde: true }),
  variante('R2', 'R2 R1 com A3', { ...CARTAS, ...BURN2, pagarXDelta: 0, primeiroSemGrau: true, segundoArcano: 0, semAcaoPerde: true }),
  variante('R3', 'R3 R1 + Conjurador grátis (H3)', { ...CARTAS, ...BURN2, pagarXDelta: 1, primeiroSemGrau: true, segundoArcano: 0, semAcaoPerde: true, conjAtaca: true, conjCusto: 0 }),
  variante('R4', 'R4 R1 + Conjurador grátis e Grau sem suspender', { ...CARTAS, ...BURN2, pagarXDelta: 1, primeiroSemGrau: true, segundoArcano: 0, semAcaoPerde: true, conjAtaca: true, conjCusto: 0, grauSemSuspender: true }),
  variante('R5', 'R5 R1 + H5 completo', { ...CARTAS, ...BURN2, pagarXDelta: 1, primeiroSemGrau: true, segundoArcano: 0, semAcaoPerde: true, ...H5 }),
  variante('R6', 'R6 R1 com A5', { ...CARTAS, ...BURN2, pagarXDelta: 2, primeiroSemGrau: true, segundoArcano: 0, semAcaoPerde: true }),
  variante('R7', 'R7 R3 com G4', { ...CARTAS, ...BURN2, pagarXDelta: 1, primeiroSemGrau: true, segundoArcano: 0, segundoCarta: 0, semAcaoPerde: true, conjAtaca: true, conjCusto: 0 }),
];

const porPar = Number(process.argv[2] ?? 40);
const rodada = process.argv[3] ?? 'rodada3';
const variantes: Variante[] = rodada === 'rodada2' ? rodada2 : rodada3;
const saida = rodada === 'rodada2' ? 'COMPARACAO-VARIANTES' : 'COMPARACAO-RODADA3';
const media = (v: number[]) => (v.length ? v.reduce((s, x) => s + x, 0) / v.length : 0);
const desvio = (v: number[]) => { const m = media(v); return Math.sqrt(media(v.map((x) => (x - m) ** 2))); };
const pct = (x: number) => `${(100 * x).toFixed(1)}%`;
const f1 = (x: number) => x.toFixed(1);
const f2 = (x: number) => x.toFixed(2);
const percentil = (v: number[], p: number) => [...v].sort((x, y) => x - y)[Math.min(v.length - 1, Math.floor(p * v.length))] ?? 0;

export interface Resumo {
  v: Variante;
  n: number;
  terminadas: number;
  travadas: number;
  semAcao: number;
  turnosMedia: number;
  turnosMediana: number;
  turnosP90: number;
  faixa14a26: number;
  menos10: number;
  primeiroVence: number;
  nomes: string[];
  taxaPorArq: number[];
  durPorArq: number[];
  travaPorArq: number[];
  espalhamento: number;
  uso: Record<string, number>;
  danoConjShare: number;
}

function resumir(v: Variante): Resumo {
  const ctx = contextoDe(v);
  const nomes = ctx.arqs.map((a) => a.nome);
  const resultados: Resultado[] = [];
  let semente = 1000;
  for (let a = 0; a < nomes.length; a++) for (let b = 0; b < nomes.length; b++) for (let k = 0; k < porPar; k++) resultados.push(partida(ctx, a, b, semente++));
  const term = resultados.filter((r) => r.vencedor !== null);
  const turnos = term.map((r) => r.turnos);
  const taxaPorArq = nomes.map((_, i) => {
    const rs = term.filter((r) => (r.a === i || r.b === i) && r.a !== r.b);
    const g = rs.filter((r) => (r.a === i && r.vencedor === 0) || (r.b === i && r.vencedor === 1)).length;
    return rs.length ? g / rs.length : 0;
  });
  const durPorArq = nomes.map((_, i) => media(term.filter((r) => r.a === i || r.b === i).map((r) => r.turnos)));
  const travaPorArq = nomes.map((_, i) => { const rs = resultados.filter((r) => r.a === i || r.b === i); return rs.filter((r) => r.vencedor === null).length / rs.length; });
  const jogs: Jog[] = term.flatMap((r) => r.m);
  const med = (f: (j: Jog) => number) => media(jogs.map(f));
  const danoTotalConj = jogs.reduce((s, j) => s + j.danoCombateRecebido + j.danoEfeitoRecebido, 0);
  const danoPorConj = jogs.reduce((s, j) => s + j.danoDoConjurador, 0);
  return {
    v, n: resultados.length, terminadas: term.length, travadas: resultados.length - term.length, semAcao: term.filter((r) => r.fim === 'semAcao').length,
    turnosMedia: media(turnos), turnosMediana: percentil(turnos, 0.5), turnosP90: percentil(turnos, 0.9),
    faixa14a26: term.filter((r) => r.turnos >= 14 && r.turnos <= 26).length / (term.length || 1),
    menos10: term.filter((r) => r.turnos < 10).length / (term.length || 1),
    primeiroVence: term.filter((r) => r.vencedor === r.primeiro).length / (term.length || 1),
    nomes, taxaPorArq, durPorArq, travaPorArq, espalhamento: desvio(taxaPorArq),
    danoConjShare: danoTotalConj ? danoPorConj / danoTotalConj : 0,
    uso: {
      grau: med((j) => j.grauColocado), suspGrau: med((j) => j.suspensoesGrau), pitch: med((j) => j.cartasEnviadas), dissipado: med((j) => j.arcanoDissipado),
      descartes: med((j) => j.descartes), atqAliado: med((j) => j.ataquesAliado), atqConj: med((j) => j.ataquesConj), blqConj: med((j) => j.bloqueiosConj),
      ativMana: med((j) => j.ativacoesMana), ativOutras: med((j) => j.ativacoesOutras), anulOfer: med((j) => j.anulacoesOferecidas), anulPagas: med((j) => j.anulacoesPagas),
      danoEfeito: med((j) => j.danoEfeitoRecebido), danoCombate: med((j) => j.danoCombateRecebido), danoGuardado: media(jogs.filter((j) => j.amostrasDanoGuardado > 0).map((j) => j.danoGuardado / j.amostrasDanoGuardado)),
      erros: med((j) => j.erros),
    },
  };
}

const resumos: Resumo[] = [];
for (const v of variantes) {
  const t0 = Date.now();
  resumos.push(resumir(v));
  console.log(`${v.id}: ${((Date.now() - t0) / 1000).toFixed(1)}s`);
}

const nomes = resumos[0].nomes;
let md = `# Comparação de variantes (gerado por sim/comparar.ts)\n\n${variantes.length} configurações × ${nomes.length} montagens × ${nomes.length} (ordenado) × ${porPar} partidas = ${resumos.reduce((s, r) => s + r.n, 0)} partidas. Bot guloso simples; limite de ${LIMITE_TURNOS} turnos; vencedor do dado começa.\n\n`;
md += rodada === 'rodada3' ? `Variantes: **base** = regras antigas (segundo jogador com carta e Arcano extra, sem relógio). **perm** = Estruturas com efeito + Aliados de recurso (referência para as que dependem de permanentes). **PACOTE (P)** = todas as peças da rodada 3: Arcano caro, permanentes que contam como Grau, Eternyr forte, burn com menos dano por carta, cura melhorada, stax frágil, relógio de impasse (5), segundo sem compensação, quem começa sem Grau no turno 1, Conjurador de quem começa não ataca no turno 1, Conjurador ataca grátis com Grau sem suspender. **Pacote sem: X** = o pacote com uma peça desligada. **lentos** = jogador de deck lento (só sobrevive e joga para Graus altos).

` : `Legenda das variantes: **A1** burn com +2 de custo e Grau · **A2** burn anulável pagando X (custo − 1) · **B** mais cartas de recurso · **C** Eternyr com mais draw e mais barato · **D** Estruturas com efeito · **E** cura melhor e escalável · **F** sem ações perde · **G** quem começa não põe Grau no turno 1 · **H1–H5** Conjurador ataca mais.\n\n`;
md += `## 1. Visão geral\n\nMeta do autor: cerca de 20 turnos. "Espalhamento" = desvio-padrão das taxas de vitória das montagens (0 = todas iguais; quanto menor, mais equilibrado).\n\n`;
md += `| Variante | Terminou | Travou | Sem ação | Turnos média | Mediana | p90 | 14–26 | <10 | Começa vence | Espalhamento | Maior win | Menor win |\n|---|---|---|---|---|---|---|---|---|---|---|---|---|\n`;
for (const r of resumos) {
  const imax = r.taxaPorArq.indexOf(Math.max(...r.taxaPorArq));
  const imin = r.taxaPorArq.indexOf(Math.min(...r.taxaPorArq));
  md += `| ${r.v.nome} | ${pct(r.terminadas / r.n)} | ${pct(r.travadas / r.n)} | ${r.semAcao} | ${f1(r.turnosMedia)} | ${r.turnosMediana} | ${r.turnosP90} | ${pct(r.faixa14a26)} | ${pct(r.menos10)} | ${pct(r.primeiroVence)} | ${f1(r.espalhamento * 100)} pp | ${r.nomes[imax]} ${pct(r.taxaPorArq[imax])} | ${r.nomes[imin]} ${pct(r.taxaPorArq[imin])} |\n`;
}
md += `\n## 2. Taxa de vitória média por montagem (contra as outras, sem espelho)\n\n| Variante | ${nomes.join(' | ')} |\n|---|${nomes.map(() => '---').join('|')}|\n`;
for (const r of resumos) md += `| ${r.v.nome} | ${r.taxaPorArq.map(pct).join(' | ')} |\n`;
md += `\n## 3. Turnos médios por montagem\n\n| Variante | ${nomes.join(' | ')} |\n|---|${nomes.map(() => '---').join('|')}|\n`;
for (const r of resumos) md += `| ${r.v.nome} | ${r.durPorArq.map(f1).join(' | ')} |\n`;
md += `\n## 4. Partidas travadas por montagem\n\n| Variante | ${nomes.join(' | ')} |\n|---|${nomes.map(() => '---').join('|')}|\n`;
for (const r of resumos) md += `| ${r.v.nome} | ${r.travaPorArq.map(pct).join(' | ')} |\n`;
md += `\n## 5. Uso das mecânicas (média por jogador por partida)\n\n| Variante | Grau posto | Susp. Grau | Pitch | Arcano dissipado | Descartes | Atq Aliado | Atq Conj | Blq Conj | Ativ. mana | Ativ. outras | Anul. oferecidas | Anul. pagas | Dano efeito | Dano combate | Dano guardado | Dano do Conj (% do dano ao Conj) |\n|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|\n`;
for (const r of resumos) {
  const u = r.uso;
  md += `| ${r.v.nome} | ${f2(u.grau)} | ${f1(u.suspGrau)} | ${f2(u.pitch)} | ${f2(u.dissipado)} | ${f2(u.descartes)} | ${f2(u.atqAliado)} | ${f2(u.atqConj)} | ${f2(u.blqConj)} | ${f2(u.ativMana)} | ${f2(u.ativOutras)} | ${f2(u.anulOfer)} | ${f2(u.anulPagas)} | ${f2(u.danoEfeito)} | ${f2(u.danoCombate)} | ${f2(u.danoGuardado)} | ${pct(r.danoConjShare)} |\n`;
}
md += `\nJogadas ilegais do bot (ignoradas), média por jogador por partida: ${f2(media(resumos.map((r) => r.uso.erros)))} (máx. ${f2(Math.max(...resumos.map((r) => r.uso.erros)))}).\n`;
writeFileSync(new URL('../docs/' + saida + '.md', import.meta.url), md);
writeFileSync(new URL('../data/' + saida.toLowerCase() + '.json', import.meta.url), JSON.stringify(resumos.map((r) => ({ ...r, v: r.v })), null, 1));
console.log('ok');
