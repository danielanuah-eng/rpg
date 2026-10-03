// Agente de testes: ajusta montagens (cartas e estilo de jogo) e simula partidas até TODOS os objetivos de sim/objetivos.json
// serem atingidos e confirmados com uma amostra grande. Guarda o progresso em agente/ e retoma de onde parou.
// Uso: node sim/agente.ts [--objetivos sim/objetivos.json] [--variante P] [--limite 0] [--novo] [--semente 1]
//   --limite 0 (padrão) = só para quando alcançar. Use --limite N para parar depois de N tentativas (código de saída 2 se não alcançou).
import { appendFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { contextoDe, partida, type Contexto } from './nucleo.ts';
import { rodada3 } from './lista.ts';
import type { Estilo } from './bot.ts';

type Lista = Array<[string, number]>;
interface Candidato { lista: Lista; estilo: Estilo }
interface Meta { contra: string[]; vitoriaMin: number; vitoriaMax?: number }
interface Objetivo {
  id: string; descricao: string; arquetipo: string; metas: Meta[]; sobrevivenciaMin?: number;
  restricoes?: {
    contraOutrosMin?: number; contraOutrosMax?: number; adversarioMin?: number; travadasMax?: number;
    /** Aliados perdidos / Aliados do oponente perdidos, em partidas de 14+ turnos (todas as montagens). */
    trocasMax?: number;
    /** Vitória nas partidas de 18+ turnos (todas as montagens). */
    vitoriaLongaMin?: number;
  };
}
interface Config { variante: string; partidasPorConfronto: number; partidasConfirmacao: number; objetivos: Objetivo[] }

// ---------- argumentos ----------
const args = process.argv.slice(2);
const arg = (nome: string, padrao?: string) => { const i = args.indexOf(nome); return i >= 0 ? args[i + 1] : padrao; };
const caminhoObj = arg('--objetivos', fileURLToPath(new URL('./objetivos.json', import.meta.url)))!;
const limite = Number(arg('--limite', '0'));
const novo = args.includes('--novo');
const quieto = args.includes('--quieto');
let semente = Number(arg('--semente', '1')) * 1_000_003;

const cfg: Config = JSON.parse(readFileSync(caminhoObj, 'utf8'));
const nomeVariante = arg('--variante', cfg.variante)!;
const variante = rodada3.find((v) => v.id === nomeVariante);
if (!variante) throw new Error(`Variante desconhecida: ${nomeVariante}. Opções: ${rodada3.map((v) => v.id).join(', ')}`);
const ctx = contextoDe(variante);
const nomes = ctx.arqs.map((a) => a.nome);
for (const o of cfg.objetivos) {
  if (!nomes.includes(o.arquetipo)) throw new Error(`Montagem desconhecida: ${o.arquetipo}`);
  for (const m of o.metas) for (const c of m.contra) if (!nomes.includes(c)) throw new Error(`Montagem desconhecida em metas: ${c}`);
}

// objetivos diferentes do padrão guardam o progresso numa subpasta própria (agente/<nome-do-arquivo>/)
const padraoObj = fileURLToPath(new URL('./objetivos.json', import.meta.url));
const sub = caminhoObj === padraoObj ? '' : caminhoObj.split(/[\\/]/).pop()!.replace(/\.json$/, '') + '/';
const pasta = new URL('../agente/' + sub, import.meta.url);
mkdirSync(pasta, { recursive: true });
const arqEstado = new URL('estado.json', pasta);
const arqDiario = new URL('diario.md', pasta);
const hash = createHash('sha1').update(JSON.stringify({ o: cfg.objetivos, v: nomeVariante })).digest('hex').slice(0, 10);

const pool = Object.keys(ctx.catalogo.cartas);
const maxCopias = (id: string) => (id === 'base-recurso' ? 10 : 4);

// ---------- estado ----------
const inicial: Record<string, Candidato> = {};
for (const o of cfg.objetivos) {
  const a = ctx.arqs[nomes.indexOf(o.arquetipo)];
  const somada = new Map<string, number>();
  for (const [id, n] of a.lista) somada.set(id, (somada.get(id) ?? 0) + n);
  inicial[o.arquetipo] = { lista: [...somada.entries()].filter(([, n]) => n > 0), estilo: { ...a.estilo } };
}
interface Estado { hash: string; iteracao: number; melhorScore: number; cands: Record<string, Candidato>; inicio: string }
let estado: Estado = { hash, iteracao: 0, melhorScore: Infinity, cands: structuredClone(inicial), inicio: new Date().toISOString() };
if (!novo && existsSync(arqEstado)) {
  const salvo: Estado = JSON.parse(readFileSync(arqEstado, 'utf8'));
  if (salvo.hash === hash) { estado = salvo; if (typeof estado.melhorScore !== 'number') estado.melhorScore = Infinity; console.log(`Retomando da tentativa ${estado.iteracao} (melhor pontuação ${estado.melhorScore.toFixed(3)}).`); }
  else console.log('Os objetivos mudaram desde a última execução: começando do zero.');
}
const salvar = () => writeFileSync(arqEstado, JSON.stringify(estado, null, 1));
const diario = (linha: string) => appendFileSync(arqDiario, linha + '\n');
if (!existsSync(arqDiario)) writeFileSync(arqDiario, `# Diário do agente de testes\n\nVariante: ${nomeVariante}. Objetivos em sim/objetivos.json.\n\n`);

// ---------- simulação ----------
function contextoCom(cands: Record<string, Candidato>): Contexto {
  return { ...ctx, arqs: ctx.arqs.map((a) => (cands[a.nome] ? { ...a, lista: cands[a.nome].lista, estilo: cands[a.nome].estilo } : a)) };
}

interface Duelo { jogos: number; vitorias: number; travadas: number; derrotas: number; turnosDerrotas: number; perdiMeus: number; perdiDeles: number; longas: number; longasVencidas: number }
function duelo(c: Contexto, a: string, b: string, n: number): Duelo {
  const ia = nomes.indexOf(a), ib = nomes.indexOf(b);
  const d: Duelo = { jogos: n, vitorias: 0, travadas: 0, derrotas: 0, turnosDerrotas: 0, perdiMeus: 0, perdiDeles: 0, longas: 0, longasVencidas: 0 };
  for (let k = 0; k < n; k++) {
    const aEsq = k % 2 === 0;
    const r = aEsq ? partida(c, ia, ib, semente++) : partida(c, ib, ia, semente++);
    if (r.vencedor === null) { d.travadas++; continue; }
    const aGanhou = aEsq ? r.vencedor === 0 : r.vencedor === 1;
    if (aGanhou) d.vitorias++;
    else { d.derrotas++; d.turnosDerrotas += r.turnos; }
    const lado = aEsq ? 0 : 1;
    if (r.turnos >= 14) { d.perdiMeus += r.extras[lado].aliadosPerdidos; d.perdiDeles += r.extras[1 - lado].aliadosPerdidos; }
    if (r.turnos >= 18) { d.longas++; if (aGanhou) d.longasVencidas++; }
  }
  return d;
}

interface Medida { vsMeta: Record<string, Duelo>; vsOutros: number; travadasTotal: number; falhas: string[]; score: number }

function medir(cands: Record<string, Candidato>, o: Objetivo, n: number): Medida {
  const c = contextoCom(cands);
  const vsMeta: Record<string, Duelo> = {};
  const alvos = new Set(o.metas.flatMap((m) => m.contra));
  let score = 0;
  const falhas: string[] = [];
  let jogos = 0, travadas = 0;
  const todos: Duelo[] = [];
  for (const m of o.metas) {
    for (const op of m.contra) {
      const d = (vsMeta[op] ??= duelo(c, o.arquetipo, op, n));
      todos.push(d);
      jogos += d.jogos; travadas += d.travadas;
      const wr = d.vitorias / d.jogos;
      const max = m.vitoriaMax ?? (o.restricoes?.adversarioMin !== undefined ? 1 - o.restricoes.adversarioMin : 1);
      if (wr < m.vitoriaMin) { score += m.vitoriaMin - wr; falhas.push(`vitória contra ${op}: ${(100 * wr).toFixed(1)}% < ${(100 * m.vitoriaMin).toFixed(0)}%`); }
      if (wr > max) { score += wr - max; falhas.push(`vitória contra ${op}: ${(100 * wr).toFixed(1)}% > ${(100 * max).toFixed(0)}%`); }
    }
  }
  if (o.sobrevivenciaMin !== undefined) {
    for (const [op, d] of Object.entries(vsMeta)) {
      const surv = d.derrotas ? d.turnosDerrotas / d.derrotas : Infinity;
      if (surv < o.sobrevivenciaMin) { score += 0.2 * (1 - surv / o.sobrevivenciaMin); falhas.push(`sobrevivência contra ${op}: ${surv.toFixed(1)} < ${o.sobrevivenciaMin} turnos`); }
    }
  }
  const outros = nomes.filter((x) => x !== o.arquetipo && !alvos.has(x));
  let somaOutros = 0;
  const nOutros = Math.max(30, Math.round(n / 3));
  for (const op of outros) {
    const d = duelo(c, o.arquetipo, op, nOutros);
    todos.push(d);
    somaOutros += d.vitorias / d.jogos;
    jogos += d.jogos; travadas += d.travadas;
  }
  const vsOutros = outros.length ? somaOutros / outros.length : 0.5;
  const r = o.restricoes ?? {};
  if (r.contraOutrosMin !== undefined && vsOutros < r.contraOutrosMin) { score += r.contraOutrosMin - vsOutros; falhas.push(`vitória média contra as outras montagens: ${(100 * vsOutros).toFixed(1)}% < ${(100 * r.contraOutrosMin).toFixed(0)}%`); }
  if (r.contraOutrosMax !== undefined && vsOutros > r.contraOutrosMax) { score += vsOutros - r.contraOutrosMax; falhas.push(`vitória média contra as outras montagens: ${(100 * vsOutros).toFixed(1)}% > ${(100 * r.contraOutrosMax).toFixed(0)}%`); }
  const travadasTotal = jogos ? travadas / jogos : 0;
  const meus = todos.reduce((t, d) => t + d.perdiMeus, 0), deles = todos.reduce((t, d) => t + d.perdiDeles, 0);
  const longas = todos.reduce((t, d) => t + d.longas, 0), longasV = todos.reduce((t, d) => t + d.longasVencidas, 0);
  if (r.trocasMax !== undefined && deles > 0 && meus / deles > r.trocasMax) { score += 0.5 * (meus / deles - r.trocasMax); falhas.push(`trocas: ${(meus / deles).toFixed(2)} > ${r.trocasMax}`); }
  if (r.vitoriaLongaMin !== undefined && longas > 0 && longasV / longas < r.vitoriaLongaMin) { score += r.vitoriaLongaMin - longasV / longas; falhas.push(`vitória em partidas longas: ${(100 * longasV / longas).toFixed(1)}% < ${(100 * r.vitoriaLongaMin).toFixed(0)}%`); }
  if (r.travadasMax !== undefined && travadasTotal > r.travadasMax) { score += travadasTotal - r.travadasMax; falhas.push(`partidas travadas: ${(100 * travadasTotal).toFixed(1)}% > ${(100 * r.travadasMax).toFixed(0)}%`); }
  return { vsMeta, vsOutros, travadasTotal, falhas, score };
}

function medirTodos(cands: Record<string, Candidato>, n: number): { score: number; porObjetivo: Medida[] } {
  const porObjetivo = cfg.objetivos.map((o) => medir(cands, o, n));
  return { score: porObjetivo.reduce((s, m) => s + m.score, 0), porObjetivo };
}

// ---------- mutações ----------
const rnd = (n: number) => Math.floor(Math.random() * n);
const escolher = <T,>(v: T[]): T => v[rnd(v.length)];

function trocarUma(lista: Lista): Lista {
  const m = new Map(lista);
  const origens = [...m.entries()].filter(([, n]) => n > 0).map(([id]) => id);
  const de = escolher(origens);
  const destinos = pool.filter((id) => id !== de && (m.get(id) ?? 0) < maxCopias(id));
  const para = escolher(destinos);
  m.set(de, (m.get(de) ?? 0) - 1);
  m.set(para, (m.get(para) ?? 0) + 1);
  return [...m.entries()].filter(([, n]) => n > 0);
}

function mutar(c: Candidato, forca: number): Candidato {
  const r = Math.random();
  const nova: Candidato = { lista: c.lista.map(([id, n]) => [id, n] as [string, number]), estilo: { ...c.estilo } };
  if (r < 0.1) {
    const k = rnd(4);
    if (k === 0) nova.estilo.agressividade = escolher([0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.8]);
    else if (k === 1) nova.estilo.reservaInstantes = !nova.estilo.reservaInstantes;
    else if (k === 2) nova.estilo.grauAlvo = 5 + rnd(8);
    else nova.estilo.lento = !nova.estilo.lento;
  } else {
    const passos = r < 0.75 ? 1 : r < 0.92 ? 2 + rnd(forca) : 4;
    for (let i = 0; i < passos; i++) nova.lista = trocarUma(nova.lista);
  }
  return nova;
}

const descLista = (l: Lista) => l.filter(([, n]) => n > 0).sort((a, b) => b[1] - a[1] || a[0].localeCompare(b[0])).map(([id, n]) => `${n}× ${id}`).join(', ');
const pct = (x: number) => `${(100 * x).toFixed(1)}%`;

function resumoMedida(o: Objetivo, m: Medida): string {
  const partes = Object.entries(m.vsMeta).map(([op, d]) => `${op} ${pct(d.vitorias / d.jogos)} (sobrevive ${d.derrotas ? (d.turnosDerrotas / d.derrotas).toFixed(1) : '—'} turnos)`);
  return `${o.arquetipo}: ${partes.join(' · ')} · outras ${pct(m.vsOutros)} · travadas ${pct(m.travadasTotal)}`;
}

function escreverResultado(conf: { score: number; porObjetivo: Medida[] }): void {
  let md = `# Resultado do agente de testes\n\nObjetivos atingidos e confirmados (${cfg.partidasConfirmacao} partidas por confronto). Variante: ${nomeVariante}. Tentativas: ${estado.iteracao}.\n\n`;
  for (const [i, o] of cfg.objetivos.entries()) {
    const c = estado.cands[o.arquetipo];
    md += `## ${o.descricao}\n\n- Medidas: ${resumoMedida(o, conf.porObjetivo[i])}\n- Estilo do jogador: ${JSON.stringify(c.estilo)}\n- Baralho (${c.lista.reduce((s, [, n]) => s + n, 0)} cartas): ${descLista(c.lista)}\n\n`;
  }
  md += 'O baralho e o estilo estão em `agente/estado.json` (campo `cands`) e podem ser levados para `sim/baralhos.ts`.\n';
  writeFileSync(new URL('resultado.md', pasta), md);
  writeFileSync(new URL('resultado.json', pasta), JSON.stringify({ variante: nomeVariante, cands: estado.cands, medidas: conf.porObjetivo.map((m, i) => ({ objetivo: cfg.objetivos[i].id, ...m })) }, null, 1));
}

// ---------- laço principal ----------
const N = cfg.partidasPorConfronto;
const inicioDaExecucao = estado.iteracao;
let atual = estado.cands;
let scoreAtual = medirTodos(atual, N).score;
if (estado.melhorScore === Infinity) estado.melhorScore = scoreAtual;
let semMelhora = 0;
let temperatura = 0.04;
console.log(`Objetivos: ${cfg.objetivos.map((o) => o.descricao).join('; ')}`);
console.log(`Pontuação inicial: ${scoreAtual.toFixed(3)} (0 = tudo atingido).`);

for (;;) {
  if (scoreAtual <= 0.0001 || estado.melhorScore <= 0.0001) {
    // possível sucesso: confirma com amostra grande
    const conf = medirTodos(estado.cands, cfg.partidasConfirmacao);
    if (conf.score <= 0.0001) {
      escreverResultado(conf);
      diario(`\n**Concluído** na tentativa ${estado.iteracao}. ${cfg.objetivos.map((o, i) => resumoMedida(o, conf.porObjetivo[i])).join(' | ')}`);
      salvar();
      console.log('\nOBJETIVOS ATINGIDOS E CONFIRMADOS.');
      for (const [i, o] of cfg.objetivos.entries()) console.log(' -', resumoMedida(o, conf.porObjetivo[i]));
      console.log('Detalhes em agente/' + sub + 'resultado.md');
      process.exit(0);
    }
    console.log(`  confirmação falhou (${conf.porObjetivo.flatMap((m) => m.falhas).join('; ')}); continuando.`);
    diario(`- tentativa ${estado.iteracao}: confirmação falhou: ${conf.porObjetivo.flatMap((m) => m.falhas).join('; ')}`);
    estado.melhorScore = conf.score + 0.0005;
    atual = estado.cands;
    scoreAtual = conf.score;
  }
  if (limite > 0 && estado.iteracao - inicioDaExecucao >= limite) {
    console.log(`\nLimite de ${limite} tentativas nesta execução atingido sem alcançar os objetivos. Melhor pontuação: ${estado.melhorScore.toFixed(3)}.`);
    const m = medirTodos(estado.cands, cfg.partidasConfirmacao);
    for (const [i, o] of cfg.objetivos.entries()) console.log(' -', resumoMedida(o, m.porObjetivo[i]), '| falhas:', m.porObjetivo[i].falhas.join('; '));
    salvar();
    // passa o caso ao agente-balanceador (ele pode mudar cartas, efeitos e variáveis)
    let rel = `# Relatório para o agente-balanceador\n\nO agente de testes testou ${estado.iteracao} combinações de cartas existentes e estilos de jogo na variante ${nomeVariante} e **não alcançou** os objetivos.\n\n`;
    for (const [i, o] of cfg.objetivos.entries()) {
      rel += `## ${o.descricao}\n\n- Melhor medida (amostra de confirmação): ${resumoMedida(o, m.porObjetivo[i])}\n- Falhas: ${m.porObjetivo[i].falhas.join('; ') || 'nenhuma'}\n- Melhor baralho: ${descLista(estado.cands[o.arquetipo].lista)}\n- Estilo: ${JSON.stringify(estado.cands[o.arquetipo].estilo)}\n\n`;
    }
    rel += 'Próximo passo: acionar o agente-balanceador (`.claude/agents/agente-balanceador.md`).\n';
    writeFileSync(new URL('relatorio-para-balanceador.md', pasta), rel);
    console.log('Relatório para o agente-balanceador: agente/relatorio-para-balanceador.md');
    process.exit(2);
  }
  estado.iteracao++;
  // escolhe uma montagem com objetivo não atendido para mexer
  const pendentes = cfg.objetivos.length === 1 ? [cfg.objetivos[0].arquetipo] : cfg.objetivos.filter((o) => medir(atual, o, 20).score > 0).map((o) => o.arquetipo);
  const alvo = escolher(pendentes.length ? pendentes : cfg.objetivos.map((o) => o.arquetipo));
  const prop = structuredClone(atual);
  prop[alvo] = mutar(atual[alvo], 1 + Math.floor(semMelhora / 30));
  let m = medirTodos(prop, N);
  // candidato promissor: mede de novo com amostra maior para não confundir sorte com melhora
  if (m.score < estado.melhorScore - 0.0001 && m.score < 0.25) m = medirTodos(prop, N * 3);
  const aceita = m.score <= scoreAtual || Math.random() < Math.exp(-(m.score - scoreAtual) / temperatura);
  if (aceita) { atual = prop; scoreAtual = m.score; }
  if (m.score < estado.melhorScore - 0.0001) {
    estado.melhorScore = m.score;
    estado.cands = structuredClone(prop);
    semMelhora = 0;
    const linha = `- tentativa ${estado.iteracao}: pontuação ${m.score.toFixed(3)} · ${cfg.objetivos.map((o, i) => resumoMedida(o, m.porObjetivo[i])).join(' | ')}`;
    diario(linha);
    if (!quieto) console.log(linha);
    salvar();
  } else {
    semMelhora++;
    if (semMelhora % 40 === 0) { atual = structuredClone(estado.cands); scoreAtual = estado.melhorScore; temperatura = Math.min(0.08, temperatura * 1.3); }
    if (estado.iteracao % 25 === 0) { salvar(); if (!quieto) console.log(`  tentativa ${estado.iteracao}: melhor ${estado.melhorScore.toFixed(3)}, atual ${scoreAtual.toFixed(3)}`); }
  }
}
