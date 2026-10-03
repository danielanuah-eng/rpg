// Avalia uma variante contra os parâmetros de balanceamento do autor (sim/criterios.json).
// Uso: node sim/avaliar.ts [--variante P] [--partidas 40] [--baralhos agente/estado.json] [--saida NOME]
// Código de saída: 0 = todos os critérios atingidos; 10 = "próximo" (ver criterios.json); 20 = longe.
import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { contextoDe, partida, type Contexto, type Resultado } from './nucleo.ts';
import { rodada3 } from './lista.ts';
import type { Estilo } from './bot.ts';

interface Criterio {
  id: string; tipo: string; descricao: string; opcional?: boolean;
  min?: number; max?: number; a?: string; b?: string; arquetipo?: string; contra?: string; turnosMin?: number;
  mediaMin?: number; mediaMax?: number; exceto?: string[]; grauMin?: number;
}
interface Avaliado { id: string; descricao: string; ok: boolean; valor: string; alvo: string; desvio: number; unidade: 'pp' | 'rel'; opcional: boolean; nota?: string }

const args = process.argv.slice(2);
const arg = (n: string, p?: string) => { const i = args.indexOf(n); return i >= 0 ? args[i + 1] : p; };
const idVariante = arg('--variante', 'PADRAO')!;
const porPar = Number(arg('--partidas', '40'));
const arqBaralhos = arg('--baralhos');
const cfg = JSON.parse(readFileSync(fileURLToPath(new URL('./criterios.json', import.meta.url)), 'utf8')) as { proximo: { fracaoMin: number; toleranciaPP: number }; criterios: Criterio[] };

export function avaliar(ctx: Contexto, partidasPorPar: number, criterios: Criterio[], proximo: { fracaoMin: number; toleranciaPP: number }) {
  const nomes = ctx.arqs.map((a) => a.nome);
  const R: Resultado[] = [];
  let semente = 777;
  for (let a = 0; a < nomes.length; a++) for (let b = 0; b < nomes.length; b++) for (let k = 0; k < partidasPorPar; k++) R.push(partida(ctx, a, b, semente++));
  const idx = (n: string) => nomes.indexOf(n);
  const decididas = R.filter((r) => r.vencedor !== null);
  const lado = (r: Resultado, i: number) => (r.a === i ? 0 : 1);
  const venceu = (r: Resultado, i: number) => r.vencedor === lado(r, i);
  const envolve = (r: Resultado, i: number, j?: number) => r.a !== r.b && (r.a === i || r.b === i) && (j === undefined || r.a === j || r.b === j);
  const taxa = (i: number, j?: number, filtro: (r: Resultado) => boolean = () => true) => {
    const rs = decididas.filter((r) => envolve(r, i, j) && filtro(r));
    return { n: rs.length, v: rs.length ? rs.filter((r) => venceu(r, i)).length / rs.length : NaN };
  };
  const geral = nomes.map((_, i) => {
    const vs = nomes.map((_, j) => (j === i ? NaN : taxa(i, j).v)).filter((x) => !Number.isNaN(x));
    return vs.reduce((s, x) => s + x, 0) / vs.length;
  });
  const media = (v: number[]) => (v.length ? v.reduce((s, x) => s + x, 0) / v.length : NaN);
  const pct = (x: number) => (Number.isNaN(x) ? '—' : `${(100 * x).toFixed(1)}%`);
  const out: Avaliado[] = [];
  const push = (c: Criterio, ok: boolean, valor: string, alvo: string, desvio: number, unidade: 'pp' | 'rel', nota?: string) =>
    out.push({ id: c.id, descricao: c.descricao, ok, valor, alvo, desvio: ok ? 0 : desvio, unidade, opcional: !!c.opcional, nota });
  const faixa = (c: Criterio, v: number, comoPct = true) => {
    const okMin = c.min === undefined || v >= c.min;
    const okMax = c.max === undefined || v <= c.max;
    const desvio = !okMin ? c.min! - v : !okMax ? v - c.max! : 0;
    const alvo = [c.min !== undefined ? `≥ ${comoPct ? pct(c.min) : c.min}` : '', c.max !== undefined ? `≤ ${comoPct ? pct(c.max) : c.max}` : ''].filter(Boolean).join(' e ');
    return { ok: okMin && okMax && !Number.isNaN(v), desvio: comoPct ? desvio * 100 : (c.min ?? c.max ?? 1) ? (desvio / (c.min ?? c.max ?? 1)) * 100 : desvio, alvo };
  };
  for (const c of criterios) {
    const ausente = [c.a, c.b, c.arquetipo, c.contra].filter(Boolean).find((n) => idx(n!) < 0);
    if (c.tipo === 'existe') { const ok = idx(c.arquetipo!) >= 0; push(c, ok, ok ? 'existe' : 'não existe', 'existir', 100, 'rel'); continue; }
    if (ausente) { push(c, false, `montagem ${ausente} não existe`, '—', 100, 'rel', 'ausente'); continue; }
    switch (c.tipo) {
      case 'vitoriaGeralMax':
      case 'vitoriaGeralMin': {
        const alvos = nomes.filter((n) => !(c.exceto ?? []).includes(n));
        const fora = alvos.filter((n) => (c.tipo === 'vitoriaGeralMax' ? geral[idx(n)] > c.max! : geral[idx(n)] < c.min!));
        const pior = c.tipo === 'vitoriaGeralMax' ? Math.max(...alvos.map((n) => geral[idx(n)])) : Math.min(...alvos.map((n) => geral[idx(n)]));
        const f = faixa(c, pior);
        push(c, fora.length === 0, `${fora.length ? fora.map((n) => `${n} ${pct(geral[idx(n)])}`).join(', ') : 'todas dentro'}`, f.alvo, f.desvio, 'pp');
        break;
      }
      case 'duracao': {
        const m = media(decididas.map((r) => r.turnos));
        const ok = m >= c.mediaMin! && m <= c.mediaMax!;
        push(c, ok, `${m.toFixed(1)} turnos`, `${c.mediaMin} a ${c.mediaMax}`, ok ? 0 : (m < c.mediaMin! ? (c.mediaMin! - m) / c.mediaMin! : (m - c.mediaMax!) / c.mediaMax!) * 100, 'rel');
        break;
      }
      case 'travadasMax': { const v = (R.length - decididas.length) / R.length; const f = faixa(c, v); push(c, f.ok, pct(v), f.alvo, f.desvio, 'pp'); break; }
      case 'primeiroJogador': { const v = decididas.filter((r) => r.vencedor === r.primeiro).length / decididas.length; const f = faixa(c, v); push(c, f.ok, pct(v), f.alvo, f.desvio, 'pp'); break; }
      case 'confronto': { const t = taxa(idx(c.a!), idx(c.b!)); const f = faixa(c, t.v); push(c, f.ok, `${pct(t.v)} (n=${t.n})`, f.alvo, f.desvio, 'pp'); break; }
      case 'vitoriaLonga': {
        const t = taxa(idx(c.arquetipo!), c.contra ? idx(c.contra) : undefined, (r) => r.turnos >= c.turnosMin!);
        const f = faixa(c, t.v);
        push(c, f.ok && t.n >= 20, `${pct(t.v)} em ${t.n} partidas com ${c.turnosMin}+ turnos`, f.alvo, t.n < 20 ? Math.max(f.desvio, 10) : f.desvio, 'pp', t.n < 20 ? 'amostra pequena' : undefined);
        break;
      }
      case 'trocas': {
        const i = idx(c.arquetipo!);
        const rs = decididas.filter((r) => envolve(r, i) && r.turnos >= c.turnosMin!);
        const meus = rs.reduce((s, r) => s + r.extras[lado(r, i)].aliadosPerdidos, 0);
        const deles = rs.reduce((s, r) => s + r.extras[1 - lado(r, i)].aliadosPerdidos, 0);
        const v = deles ? meus / deles : NaN;
        const f = faixa(c, v, false);
        push(c, f.ok, `${Number.isNaN(v) ? '—' : v.toFixed(2)} (${meus} × ${deles} em ${rs.length} partidas)`, f.alvo, f.desvio, 'rel');
        break;
      }
      case 'primeiroDano': {
        const i = idx(c.arquetipo!);
        const v = R.filter((r) => envolve(r, i)).map((r) => r.extras[lado(r, i)].primeiroDano ?? 99).sort((x, y) => x - y);
        const med = v[Math.floor(v.length / 2)];
        const f = faixa(c, med, false);
        push(c, f.ok, `turno ${med} (mediana)`, f.alvo, f.desvio, 'rel');
        break;
      }
      case 'maoT4': {
        const i = idx(c.arquetipo!);
        const v = media(R.filter((r) => envolve(r, i)).map((r) => r.m[lado(r, i)].maoT4).filter((x) => x >= 0));
        const f = faixa(c, v, false);
        push(c, f.ok, `${v.toFixed(2)} cartas`, f.alvo, f.desvio, 'rel');
        break;
      }
      case 'constanciaDano': {
        const i = idx(c.arquetipo!);
        const rs = R.filter((r) => envolve(r, i));
        const com = rs.reduce((s, r) => s + r.extras[lado(r, i)].turnosProprioComDano, 0);
        const total = rs.reduce((s, r) => s + Math.max(0, r.extras[lado(r, i)].turnosProprios - 1), 0);
        const v = total ? Math.min(1, com / total) : NaN;
        const f = faixa(c, v);
        push(c, f.ok, `${pct(v)} dos turnos próprios (a partir do 2º)`, f.alvo, f.desvio, 'pp');
        break;
      }
      case 'repentinas': {
        const i = idx(c.arquetipo!);
        const v = media(R.filter((r) => envolve(r, i)).map((r) => r.m[lado(r, i)].repentinasNoTurnoOponente));
        const f = faixa(c, v, false);
        push(c, f.ok, `${v.toFixed(2)} por partida`, f.alvo, f.desvio, 'rel');
        break;
      }
      case 'vitoriaSemImpasse': {
        const i = idx(c.arquetipo!);
        const ganhou = decididas.filter((r) => envolve(r, i) && venceu(r, i));
        const v = ganhou.length ? ganhou.filter((r) => r.fim !== 'impasse').length / ganhou.length : NaN;
        const f = faixa(c, v);
        push(c, f.ok, `${pct(v)} de ${ganhou.length} vitórias`, f.alvo, f.desvio, 'pp');
        break;
      }
      case 'vantagemRecurso': {
        // fração das partidas (que chegam ao turno 16) em que a montagem tem mais cartas (mão + campo + Grau) que o oponente
        const i = idx(c.arquetipo!);
        const rs = R.filter((r) => envolve(r, i) && r.m[0].recursosT16 >= 0);
        const v = rs.length ? rs.filter((r) => r.m[lado(r, i)].recursosT16 > r.m[1 - lado(r, i)].recursosT16).length / rs.length : NaN;
        const dif = media(rs.map((r) => r.m[lado(r, i)].recursosT16 - r.m[1 - lado(r, i)].recursosT16));
        const f = faixa(c, v);
        push(c, f.ok, `${pct(v)} de ${rs.length} partidas (diferença média ${dif >= 0 ? '+' : ''}${dif.toFixed(1)} cartas)`, f.alvo, f.desvio, 'pp');
        break;
      }
      case 'bombaDecide': {
        // partidas longas em que só um lado jogou Aliado de Grau ≥ grauMin: esse lado vence?
        const grande = (r: Resultado, l: number) => Object.entries(r.m[l].jogadas).some(([id, n]) => n > 0 && ctx.catalogo.cartas[id]?.tipo === 'aliado' && ctx.catalogo.cartas[id].grau >= (c.grauMin ?? 5));
        const rs = decididas.filter((r) => r.a !== r.b && r.turnos >= c.turnosMin! && grande(r, 0) !== grande(r, 1));
        const v = rs.length ? rs.filter((r) => grande(r, r.vencedor as number)).length / rs.length : NaN;
        const f = faixa(c, v);
        push(c, f.ok && rs.length >= 20, `${pct(v)} em ${rs.length} partidas com ${c.turnosMin}+ turnos`, f.alvo, rs.length < 20 ? Math.max(f.desvio, 10) : f.desvio, 'pp', rs.length < 20 ? 'amostra pequena' : undefined);
        break;
      }
      case 'duracaoVitorias': {
        const i = idx(c.arquetipo!);
        const v = media(decididas.filter((r) => envolve(r, i) && venceu(r, i)).map((r) => r.turnos));
        const f = faixa(c, v, false);
        push(c, f.ok, `${v.toFixed(1)} turnos`, f.alvo, f.desvio, 'rel');
        break;
      }
      default: push(c, false, `tipo desconhecido: ${c.tipo}`, '—', 100, 'rel');
    }
  }
  const obrig = out.filter((x) => !x.opcional);
  const okN = obrig.filter((x) => x.ok).length;
  const todos = okN === obrig.length;
  const prox = okN / obrig.length >= proximo.fracaoMin && obrig.every((x) => x.ok || x.desvio <= proximo.toleranciaPP);
  const matriz = nomes.map((_, i) => nomes.map((_, j) => (i === j ? NaN : taxa(i, j).v)));
  return { nomes, geral, matriz, criterios: out, ok: okN, total: obrig.length, todos, proximo: prox, partidas: R.length, decididas: decididas.length,
    turnosMedia: media(decididas.map((r) => r.turnos)) };
}

// ---------- execução ----------
const v = rodada3.find((x) => x.id === idVariante);
if (!v) throw new Error(`Variante desconhecida: ${idVariante}. Opções: ${rodada3.map((x) => x.id).join(', ')}`);
let ctx = contextoDe(v);
if (arqBaralhos) {
  const cands = JSON.parse(readFileSync(arqBaralhos, 'utf8')).cands as Record<string, { lista: Array<[string, number]>; estilo: Estilo }>;
  ctx = { ...ctx, arqs: ctx.arqs.map((a) => (cands[a.nome] ? { ...a, lista: cands[a.nome].lista, estilo: cands[a.nome].estilo } : a)) };
}
const r = avaliar(ctx, porPar, cfg.criterios, cfg.proximo);
const pct = (x: number) => (Number.isNaN(x) ? '—' : `${(100 * x).toFixed(1)}%`);
const situacao = r.todos ? 'TODOS OS CRITÉRIOS ATINGIDOS' : r.proximo ? 'PRÓXIMO (pedir permissão ao autor)' : 'LONGE';
let md = `# Avaliação de balanceamento: ${v.nome}${arqBaralhos ? ` + baralhos de ${arqBaralhos}` : ''}\n\n`;
md += `${r.partidas} partidas (${porPar} por confronto ordenado), ${r.decididas} decididas. Turnos médios ${r.turnosMedia.toFixed(1)}. Critérios em sim/criterios.json.\n\n`;
md += `**Situação: ${situacao}** — ${r.ok} de ${r.total} critérios obrigatórios atingidos.\n\n## Critérios\n\n| | Critério | Medido | Alvo | Distância |\n|---|---|---|---|---|\n`;
for (const c of r.criterios) md += `| ${c.ok ? '✅' : c.opcional ? '➖' : '❌'} | ${c.descricao}${c.nota ? ` (${c.nota})` : ''} | ${c.valor} | ${c.alvo} | ${c.ok ? '' : c.desvio.toFixed(1) + (c.unidade === 'pp' ? ' pp' : '%')} |\n`;
md += `\n## Vitória média por montagem\n\n| ${r.nomes.join(' | ')} |\n|${r.nomes.map(() => '---').join('|')}|\n| ${r.geral.map(pct).join(' | ')} |\n`;
md += `\n## Matriz (linha contra coluna)\n\n| | ${r.nomes.join(' | ')} |\n|---|${r.nomes.map(() => '---').join('|')}|\n`;
for (const [i, n] of r.nomes.entries()) md += `| **${n}** | ${r.matriz[i].map(pct).join(' | ')} |\n`;
const saida = arg('--saida', `AVALIACAO-${idVariante}`)!;
writeFileSync(new URL(`../docs/${saida}.md`, import.meta.url), md);
writeFileSync(new URL(`../data/${saida.toLowerCase()}.json`, import.meta.url), JSON.stringify(r, null, 1));
console.log(`${situacao}: ${r.ok}/${r.total} critérios. Relatório: docs/${saida}.md`);
for (const c of r.criterios) if (!c.ok && !c.opcional) console.log(`  ❌ ${c.id}: ${c.valor} (alvo ${c.alvo})`);
process.exit(r.todos ? 0 : r.proximo ? 10 : 20);
