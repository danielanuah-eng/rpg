// Simula partidas entre as montagens de baralho e escreve docs/RELATORIO-SIMULACAO.md.
// Uso: node sim/simular.ts [partidasPorPar=30]
import { writeFileSync } from 'node:fs';
import { catalogoBase } from '../src/cartas-base.ts';
import { criarJogo, defCarta, ErroRegra, executar } from '../src/motor.ts';
import type { Catalogo, Comando, ConjuradorDef, Estado, Jogador } from '../src/tipos.ts';
import { decidir } from './bot.ts';
import { arquetipos, baralhoDe } from './baralhos.ts';

const conjurador: ConjuradorDef = {
  id: 'padrao', nome: 'Conjurador padrão', valores: [], comprometimento: 'Preparado', vida: 12, maoInicial: 5,
  compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '',
};
const catalogo: Catalogo = { cartas: catalogoBase.cartas, conjuradores: { padrao: conjurador } };
const LIMITE_TURNOS = 150;
const outro = (j: Jogador): Jogador => (j === 0 ? 1 : 0);

interface Jog {
  grauColocado: number; suspensoesGrau: number; cartasEnviadas: number; sobraDePitch: number; arcanoDissipado: number;
  descartes: number; ataquesAliado: number; ataquesConj: number; proficienteAoEntrar: number;
  bloqueiosAliado: number; bloqueiosConj: number; semBloqueio: number;
  jogadas: Record<string, number>; repentinasNoTurnoOponente: number;
  danoCombateRecebido: number; danoEfeitoRecebido: number; danoGuardado: number; amostrasDanoGuardado: number;
  baralhoVazioNoTurno: number; cartasCompradasPorEfeito: number;
}
const novoJog = (): Jog => ({ grauColocado: 0, suspensoesGrau: 0, cartasEnviadas: 0, sobraDePitch: 0, arcanoDissipado: 0, descartes: 0, ataquesAliado: 0, ataquesConj: 0, proficienteAoEntrar: 0,
  bloqueiosAliado: 0, bloqueiosConj: 0, semBloqueio: 0, jogadas: {}, repentinasNoTurnoOponente: 0, danoCombateRecebido: 0, danoEfeitoRecebido: 0, danoGuardado: 0, amostrasDanoGuardado: 0, baralhoVazioNoTurno: 0, cartasCompradasPorEfeito: 0 });

function medir(e: Estado, cmd: Comando, m: [Jog, Jog]): void {
  const antes = {
    etapa: e.etapa, ativo: e.ativo, pool: [e.jogadores[0].arcano, e.jogadores[1].arcano],
    dano: [e.instancias[e.jogadores[0].conjuradorId].dano, e.instancias[e.jogadores[1].conjuradorId].dano], combate: e.combate?.etapa,
    mao: [e.jogadores[0].mao.length, e.jogadores[1].mao.length],
  };
  const inst = cmd.tipo === 'atacar' ? e.instancias[cmd.atacante] : null;
  const entrouAgora = inst ? inst.entrouNoTurno === e.turno : false;
  executar(e, cmd);
  if (antes.etapa === 'cataclisma' && e.etapa === 'final') m[antes.ativo].arcanoDissipado += antes.pool[antes.ativo];
  for (const j of [0, 1] as Jogador[]) {
    const d = e.instancias[e.jogadores[j].conjuradorId].dano - antes.dano[j];
    if (d > 0) {
      if (antes.combate === 'bloqueado' && e.combate === null) m[j].danoCombateRecebido += d;
      else m[j].danoEfeitoRecebido += d;
    }
  }
  if (antes.etapa === 'final' && e.etapa === 'inicio') {
    const a = e.ativo;
    m[a].danoGuardado += e.jogadores[a].campo.reduce((s, id) => s + e.instancias[id].dano, 0);
    m[a].amostrasDanoGuardado += 1;
  }
  if (e.jogadores[0].baralho.length === 0 && m[0].baralhoVazioNoTurno === 0) m[0].baralhoVazioNoTurno = e.turno;
  if (e.jogadores[1].baralho.length === 0 && m[1].baralhoVazioNoTurno === 0) m[1].baralhoVazioNoTurno = e.turno;
  const j = 'jogador' in cmd ? cmd.jogador : 0;
  if (cmd.tipo === 'colocarGrau') m[j].grauColocado++;
  else if (cmd.tipo === 'suspenderGrau') m[j].suspensoesGrau++;
  else if (cmd.tipo === 'descartar' && e.etapa === 'inicio') m[j].descartes += cmd.cartas.length;
  else if (cmd.tipo === 'jogar') {
    const d = defCarta(e, cmd.carta);
    const defId = e.instancias[cmd.carta].defId;
    m[j].jogadas[defId] = (m[j].jogadas[defId] ?? 0) + 1;
    m[j].cartasEnviadas += cmd.enviarArcano?.length ?? 0;
    if (cmd.enviarArcano?.length) m[j].sobraDePitch += e.jogadores[j].arcano;
    if (d.velocidade === 'repentina' && antes.ativo !== j) m[j].repentinasNoTurnoOponente++;
    if (d.efeitos?.some((x) => x.tipo === 'comprar')) m[j].cartasCompradasPorEfeito += 1;
  } else if (cmd.tipo === 'atacar') {
    if (e.instancias[cmd.atacante].ehConjurador) m[j].ataquesConj++;
    else {
      m[j].ataquesAliado++;
      if (entrouAgora) m[j].proficienteAoEntrar++;
    }
  } else if (cmd.tipo === 'bloquear') {
    const bl = cmd.bloqueadores?.[0] ?? cmd.bloqueador ?? null;
    if (bl === null) m[j].semBloqueio++;
    else if (e.instancias[bl].ehConjurador) m[j].bloqueiosConj++;
    else m[j].bloqueiosAliado++;
  }
}

interface Resultado {
  a: number; b: number; semente: number; turnos: number; vencedor: Jogador | null; primeiro: Jogador; m: [Jog, Jog];
  passos: number;
}

function partida(a: number, b: number, semente: number): Resultado {
  const e = criarJogo({ catalogo, baralhos: [baralhoDe(arquetipos[a], 'padrao'), baralhoDe(arquetipos[b], 'padrao')], semente });
  const m: [Jog, Jog] = [novoJog(), novoJog()];
  const estilos = [arquetipos[a].estilo, arquetipos[b].estilo];
  let passos = 0;
  let primeiro: Jogador = 0;
  while (e.vencedor === null && e.turno <= LIMITE_TURNOS && passos < 40000) {
    passos++;
    const ator: Jogador = e.pendencia ? e.pendencia.jogador : e.prioridade;
    const cmd = decidir(e, ator, estilos[ator]);
    if (cmd.tipo === 'escolherInicio') primeiro = cmd.primeiro;
    try {
      medir(e, cmd, m);
    } catch (erro) {
      if (!(erro instanceof ErroRegra)) throw erro;
      // o bot errou: passa para a partida não travar
      try { medir(e, { tipo: 'passar', jogador: e.pendencia ? e.pendencia.jogador : e.prioridade }, m); } catch (e2) { if (!(e2 instanceof ErroRegra)) throw e2; }
      m[ator].jogadas['__erro_do_bot'] = (m[ator].jogadas['__erro_do_bot'] ?? 0) + 1;
    }
  }
  const venc = e.vencedor === 0 || e.vencedor === 1 ? e.vencedor : null;
  return { a, b, semente, turnos: e.turno, vencedor: venc, primeiro, m, passos };
}

// ---------- execução ----------
const porPar = Number(process.argv[2] ?? 30);
const resultados: Resultado[] = [];
let semente = 1;
for (let a = 0; a < arquetipos.length; a++) for (let b = 0; b < arquetipos.length; b++) for (let k = 0; k < porPar; k++) resultados.push(partida(a, b, semente++));

// ---------- análise ----------
const media = (v: number[]) => (v.length ? v.reduce((s, x) => s + x, 0) / v.length : 0);
const pct = (x: number) => `${(100 * x).toFixed(1)}%`;
const f1 = (x: number) => x.toFixed(1);
const f2 = (x: number) => x.toFixed(2);
const percentil = (v: number[], p: number) => [...v].sort((x, y) => x - y)[Math.min(v.length - 1, Math.floor(p * v.length))] ?? 0;

const terminadas = resultados.filter((r) => r.vencedor !== null);
const travadas = resultados.length - terminadas.length;
const turnos = terminadas.map((r) => r.turnos);
let md = `# Relatório de simulação (gerado por sim/simular.ts)\n\n`;
md += `${resultados.length} partidas: ${arquetipos.length} montagens × ${arquetipos.length} (ordenado, inclui espelhos) × ${porPar} partidas. Jogador guloso simples (sim/bot.ts), Conjurador padrão (12 de Vida, mão 5, compra 2, dano básico 1), cartas básicas, vencedor do dado sempre começa. Limite de ${LIMITE_TURNOS} turnos.\n\n`;
md += `> **Leia com cuidado:** o bot é simples. Os números mostram tendências do desenho das regras e das cartas, não o desempenho de jogadores humanos.\n\n`;
md += `## 1. Duração\n\n| Medida | Valor |\n|---|---|\n| Partidas que terminaram | ${terminadas.length} (${pct(terminadas.length / resultados.length)}) |\n| Travadas (passaram de ${LIMITE_TURNOS} turnos) | ${travadas} |\n`;
md += `| Turnos totais, média | ${f1(media(turnos))} |\n| Turnos totais, mediana | ${percentil(turnos, 0.5)} |\n| Turnos totais, p10 / p90 | ${percentil(turnos, 0.1)} / ${percentil(turnos, 0.9)} |\n| Turnos por jogador, média | ${f1(media(turnos) / 2)} |\n| Meta do autor | cerca de 20 turnos |\n\n`;
const dentro = terminadas.filter((r) => r.turnos >= 14 && r.turnos <= 26).length;
md += `Partidas entre 14 e 26 turnos totais: ${pct(dentro / terminadas.length)}. Menos de 10 turnos: ${pct(terminadas.filter((r) => r.turnos < 10).length / terminadas.length)}. Mais de 40: ${pct(terminadas.filter((r) => r.turnos > 40).length / terminadas.length)}.\n\n`;

md += `### Duração por montagem (média de turnos totais quando a montagem joga)\n\n| Montagem | Turnos médios | Vitórias | Derrotas | Travou |\n|---|---|---|---|---|\n`;
const nomes = arquetipos.map((x) => x.nome);
const stat = nomes.map((_, i) => {
  const rs = resultados.filter((r) => r.a === i || r.b === i);
  const vit = rs.filter((r) => (r.a === i && r.vencedor === 0) || (r.b === i && r.vencedor === 1)).length;
  const der = rs.filter((r) => r.vencedor !== null && !((r.a === i && r.vencedor === 0) || (r.b === i && r.vencedor === 1))).length;
  return { i, turnos: media(rs.filter((r) => r.vencedor !== null).map((r) => r.turnos)), vit, der, trav: rs.filter((r) => r.vencedor === null).length };
});
for (const s of stat) md += `| ${nomes[s.i]} | ${f1(s.turnos)} | ${s.vit} | ${s.der} | ${s.trav} |\n`;

md += `\n### Partidas travadas por confronto (% das partidas do par que passaram de ${LIMITE_TURNOS} turnos)\n\n| | ${nomes.join(' | ')} |\n|---|${nomes.map(() => '---').join('|')}|\n`;
for (let a = 0; a < nomes.length; a++) {
  md += `| **${nomes[a]}** | ${nomes.map((_, b) => { const rs = resultados.filter((r) => (r.a === a && r.b === b) || (r.a === b && r.b === a)); return pct(rs.filter((r) => r.vencedor === null).length / rs.length); }).join(' | ')} |\n`;
}
md += `\n## 2. Quem começa\n\n`;
const venceuPrimeiro = terminadas.filter((r) => r.vencedor === r.primeiro).length;
md += `O jogador que começa vence **${pct(venceuPrimeiro / terminadas.length)}** das partidas terminadas (o segundo jogador já compra 1 carta a mais e começa com 1 Arcano). 50% seria equilíbrio.\n\n`;

md += `## 3. Matriz de vitórias (linha contra coluna, % de vitória da linha, sem contar partidas travadas)\n\n| | ${nomes.join(' | ')} | Média |\n|---|${nomes.map(() => '---').join('|')}|---|\n`;
for (let a = 0; a < nomes.length; a++) {
  const cel: string[] = [];
  const todas: number[] = [];
  for (let b = 0; b < nomes.length; b++) {
    const rs = terminadas.filter((r) => (r.a === a && r.b === b) || (r.a === b && r.b === a));
    const ganhou = rs.filter((r) => (r.a === a && r.vencedor === 0) || (r.b === a && r.vencedor === 1)).length;
    // espelho: 50% por construção; se a === b conta como 0,5
    const taxa = a === b ? 0.5 : rs.length ? ganhou / rs.length : 0;
    cel.push(pct(taxa));
    if (a !== b) todas.push(taxa);
  }
  md += `| **${nomes[a]}** | ${cel.join(' | ')} | **${pct(media(todas))}** |\n`;
}

md += `\n## 4. Uso das mecânicas (médias por jogador por partida)\n\n`;
const jogs = terminadas.flatMap((r) => r.m);
const mec = (nome: string, v: number[], nota = '') => `| ${nome} | ${f2(media(v))} | ${nota} |\n`;
md += `| Mecânica | Média por jogador/partida | Observação |\n|---|---|---|\n`;
md += mec('Cartas colocadas no Grau', jogs.map((x) => x.grauColocado));
md += mec('Suspensões de Grau para gerar Arcano', jogs.map((x) => x.suspensoesGrau));
md += mec('Cartas enviadas ao Arcano (pitch)', jogs.map((x) => x.cartasEnviadas));
md += mec('Arcano que sobrou depois de pagar com pitch', jogs.map((x) => x.sobraDePitch), 'a regra permite sobrar; até o Cataclisma');
md += mec('Arcano dissipado no Cataclisma (desperdiçado)', jogs.map((x) => x.arcanoDissipado));
md += mec('Cartas descartadas por limite de mão (7)', jogs.map((x) => x.descartes), '');
md += mec('Ataques com Aliado', jogs.map((x) => x.ataquesAliado));
md += mec('Ataques com o Conjurador', jogs.map((x) => x.ataquesConj), 'o Conjurador suspenso não pode pôr Grau');
md += mec('Ataques de Aliado no turno em que entrou (Proficiente)', jogs.map((x) => x.proficienteAoEntrar));
md += mec('Bloqueios com Aliado', jogs.map((x) => x.bloqueiosAliado));
md += mec('Bloqueios com o Conjurador', jogs.map((x) => x.bloqueiosConj));
md += mec('Ataques sofridos sem bloqueio', jogs.map((x) => x.semBloqueio));
md += mec('Cartas repentinas jogadas no turno do oponente', jogs.map((x) => x.repentinasNoTurnoOponente));
md += mec('Cartas "comprar" jogadas', jogs.map((x) => x.cartasCompradasPorEfeito));
md += mec('Dano por combate recebido pelo Conjurador', jogs.map((x) => x.danoCombateRecebido));
md += mec('Dano por efeito recebido pelo Conjurador', jogs.map((x) => x.danoEfeitoRecebido));
const guardado = jogs.filter((x) => x.amostrasDanoGuardado > 0).map((x) => x.danoGuardado / x.amostrasDanoGuardado);
md += mec('Dano guardado nos Aliados no início do turno (memória de dano)', guardado, 'média de pontos de dano acumulados em campo');
const vazios = jogs.filter((x) => x.baralhoVazioNoTurno > 0).length;
md += `| Jogadores que esgotaram o baralho | ${pct(vazios / jogs.length)} | a regra diz que não perde por isso |\n`;
const venceu = terminadas.map((r) => (r.vencedor === 0 ? r.m[0] : r.m[1]));
const perdeu = terminadas.map((r) => (r.vencedor === 0 ? r.m[1] : r.m[0]));
const comp = (nome: string, f: (x: Jog) => number) => `| ${nome} | ${f2(media(venceu.map(f)))} | ${f2(media(perdeu.map(f)))} |\n`;
md += `\n### Vencedores × perdedores (média por partida)\n\n| Mecânica | Quem venceu | Quem perdeu |\n|---|---|---|\n`;
md += comp('Cartas no Grau', (x) => x.grauColocado) + comp('Pitch (cartas enviadas)', (x) => x.cartasEnviadas) + comp('Ataques com Aliado', (x) => x.ataquesAliado) + comp('Bloqueios com Aliado', (x) => x.bloqueiosAliado) + comp('Arcano dissipado', (x) => x.arcanoDissipado) + comp('Dano de efeito recebido', (x) => x.danoEfeitoRecebido) + comp('Dano de combate recebido', (x) => x.danoCombateRecebido);

md += `\n## 5. Cartas (todas as montagens)\n\nTaxa de jogo = quantas vezes a carta foi jogada por partida em que estava no baralho. Vitória com/sem = taxa de vitória do jogador quando jogou a carta ao menos uma vez, ou quando não jogou.\n\n| Carta | Jogada por partida | Vitória se jogou | Vitória se não jogou | Diferença |\n|---|---|---|---|---|\n`;
const porCarta: Record<string, { jogos: number; jogadas: number; vJog: number; nJog: number; vNao: number; nNao: number }> = {};
for (const r of terminadas) {
  for (const lado of [0, 1] as Jogador[]) {
    const arq = arquetipos[lado === 0 ? r.a : r.b];
    const venceuLado = r.vencedor === lado;
    for (const [id] of arq.lista) {
      const c = (porCarta[id] ??= { jogos: 0, jogadas: 0, vJog: 0, nJog: 0, vNao: 0, nNao: 0 });
      const n = r.m[lado].jogadas[id] ?? 0;
      c.jogos++;
      c.jogadas += n;
      if (n > 0) { c.nJog++; if (venceuLado) c.vJog++; } else { c.nNao++; if (venceuLado) c.vNao++; }
    }
  }
}
const linhas = Object.entries(porCarta).map(([id, c]) => ({ id, taxa: c.jogadas / c.jogos, vj: c.nJog ? c.vJog / c.nJog : NaN, vn: c.nNao ? c.vNao / c.nNao : NaN }));
linhas.sort((x, y) => x.taxa - y.taxa);
for (const l of linhas) md += `| ${l.id} | ${f2(l.taxa)} | ${Number.isNaN(l.vj) ? '-' : pct(l.vj)} | ${Number.isNaN(l.vn) ? '-' : pct(l.vn)} | ${Number.isNaN(l.vj) || Number.isNaN(l.vn) ? '-' : ((l.vj - l.vn) * 100).toFixed(1) + ' pp'} |\n`;
const erros = jogs.reduce((s, x) => s + (x.jogadas['__erro_do_bot'] ?? 0), 0);
md += `\nJogadas ilegais do bot (ignoradas): ${erros} em ${jogs.length} jogadores-partida.\n`;

writeFileSync(new URL('../docs/RELATORIO-SIMULACAO.md', import.meta.url), md);
console.log(md.split('## 4.')[0]);
console.log(`terminadas=${terminadas.length} travadas=${travadas} erros_do_bot=${erros}`);
