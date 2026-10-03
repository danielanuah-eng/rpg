// Parte do servidor que atende o editor de cartas e mecânicas (app/editor.html):
// lê o estado, valida e salva data/edicoes.json (com cópia da versão anterior) e roda a simulação dos Conjuradores.
import { spawn } from 'node:child_process';
import { copyFileSync, existsSync, mkdirSync, readFileSync, unlinkSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { ErroRegra, REGRAS_PADRAO, validarBaralho } from '../src/motor.ts';
import type { CartaDef, Efeito } from '../src/tipos.ts';
import { baralhoDe } from '../sim/baralhos.ts';
import { arquetiposConjuradores } from '../sim/conjuradores-decks.ts';
import { ARQUIVO_EDICOES, lerEdicoes, normalizar, type Edicoes } from '../sim/edicoes.ts';
import { contextoDe, partida } from '../sim/nucleo.ts';
import { catalogoGerado, precosDe, regrasGeradas } from '../sim/variantes.ts';
import { construir, varianteDoApp } from './construir.ts';

const raiz = fileURLToPath(new URL('../', import.meta.url));
const pastaHistorico = new URL('../data/edicoes-historico/', import.meta.url);
const arqSim = new URL('../data/conjuradores-editor.json', import.meta.url);
const arqSimAnterior = new URL('../data/conjuradores-editor-anterior.json', import.meta.url);

/** Tudo o que o editor precisa: as edições salvas e o jogo "original" para comparar e restaurar. */
export function estadoEditor() {
  const v = varianteDoApp();
  const original = { ...v, edicoesAutor: false };
  return {
    edicoes: lerEdicoes(),
    /** Cartas e Conjuradores gerados (com a curva de Grau editada, sem as cartas editadas): base das edições de carta. */
    gerado: catalogoGerado(v),
    regrasOriginais: { ...REGRAS_PADRAO, ...regrasGeradas(v) },
    precosOriginais: precosDe(original),
    curvaSemDobro: v.r6CurvaSemDobro,
    curvaOriginal: v.r6CurvaGrau,
    baralhosOriginais: Object.fromEntries(arquetiposConjuradores().map((a) => [a.conjurador, { lista: a.lista, estilo: a.estilo, itens: a.itens ?? [] }])),
    simulacao: lerSimulacao(),
  };
}

const TIPOS_CARTA = ['aliado', 'temporaria', 'mistica', 'estrutura', 'item'];
const TIPOS_EFEITO = ['dano', 'devolver', 'danoTodosAliados', 'destruirTodosAliados', 'anular', 'curar', 'destruir', 'comprar', 'arcano', 'arcanoOponente', 'descontoProximaCarta', 'banirDaMao', 'reanimar', 'permitirAnularDoCemiterio', 'olharTopo'];

function conferirEfeitos(onde: string, efs: Efeito[] | undefined, erros: string[]): void {
  if (efs === undefined) return;
  if (!Array.isArray(efs)) { erros.push(`${onde}: lista de efeitos inválida.`); return; }
  for (const ef of efs) if (!TIPOS_EFEITO.includes(ef?.tipo)) erros.push(`${onde}: efeito desconhecido "${ef?.tipo}".`);
}

function conferirCarta(c: CartaDef, erros: string[]): void {
  const n = c.nome || c.id;
  if (!c.nome) erros.push(`${c.id}: falta o nome.`);
  if (!TIPOS_CARTA.includes(c.tipo)) erros.push(`${n}: tipo inválido.`);
  for (const k of ['custo', 'grau', 'arcanoGerado'] as const) if (!Number.isInteger(c[k]) || c[k] < 0) erros.push(`${n}: ${k} precisa ser um número inteiro (0 ou mais).`);
  if (c.tipo === 'aliado' && (!Number.isInteger(c.vida) || (c.vida ?? 0) < 1)) erros.push(`${n}: Aliado precisa de Vida 1 ou mais.`);
  if (c.tipo === 'item' && !['canal', 'armadura', 'extra'].includes(c.categoriaItem ?? '')) erros.push(`${n}: item sem espaço (Canal, Armadura ou Extra).`);
  if (c.tipo === 'temporaria' && !(c.efeitos?.length)) erros.push(`${n}: Temporária sem efeito.`);
  conferirEfeitos(`${n} (efeitos)`, c.efeitos, erros);
  conferirEfeitos(`${n} (ao entrar)`, c.aoEntrar, erros);
  conferirEfeitos(`${n} (ao causar dano ao Conjurador)`, c.aoDanoConjurador, erros);
  conferirEfeitos(`${n} (habilidade ativada)`, c.ativacao?.efeitos, erros);
}

/** Confere o jogo com as edições atuais do arquivo: cartas, baralhos (60 cartas, Valores) e uma partida entre cada par vizinho. */
function validarJogo(): { erros: string[]; avisos: string[] } {
  const erros: string[] = [];
  const avisos: string[] = [];
  const ctx = contextoDe(varianteDoApp());
  for (const c of Object.values(ctx.catalogo.cartas)) conferirCarta(c, erros);
  for (const c of Object.values(ctx.catalogo.conjuradores)) {
    if (c.id === 'padrao') continue;
    for (const k of ['vida', 'maoInicial', 'compraFinal', 'danoBasico', 'custoAtaque'] as const) if (!Number.isInteger(c[k]) || c[k] < 0) erros.push(`${c.nome}: ${k} precisa ser um número inteiro.`);
    conferirEfeitos(`${c.nome} (habilidade ativada)`, c.poder?.ativacao?.efeitos, erros);
    conferirEfeitos(`${c.nome} (gatilho)`, c.poder?.gatilho?.efeitos, erros);
  }
  for (const a of ctx.arqs) for (const er of validarBaralho(ctx.catalogo, baralhoDe(a, a.conjurador!))) erros.push(`Baralho de ${a.nome}: ${er}`);
  if (erros.length) return { erros, avisos };
  // partidas de fumaça: o motor e o jogador automático aguentam as cartas novas?
  for (let i = 0; i < ctx.arqs.length; i++) {
    try {
      const r = partida(ctx, i, (i + 1) % ctx.arqs.length, 777 + i);
      if (r.fim === 'travou') avisos.push(`A partida de teste ${ctx.arqs[i].nome} × ${ctx.arqs[(i + 1) % ctx.arqs.length].nome} travou (o jogador automático não achou o que fazer).`);
    } catch (erro) {
      if (erro instanceof ErroRegra) continue;
      erros.push(`A partida de teste ${ctx.arqs[i].nome} × ${ctx.arqs[(i + 1) % ctx.arqs.length].nome} quebrou o motor: ${(erro as Error).message}`);
    }
  }
  return { erros, avisos };
}

/** Salva as edições se o jogo continuar válido; se não, devolve o arquivo anterior e os erros. */
export function salvarEdicoes(corpo: { edicoes: Edicoes; nota?: string; mudancas?: string[] }) {
  const antes = existsSync(ARQUIVO_EDICOES) ? readFileSync(ARQUIVO_EDICOES, 'utf8') : null;
  const anterior = antes ? normalizar(JSON.parse(antes)) : null;
  const ed = normalizar(corpo.edicoes);
  ed.historico = [...(anterior?.historico ?? []), { quando: new Date().toISOString(), nota: String(corpo.nota ?? '').trim(), mudancas: (corpo.mudancas ?? []).map(String) }];
  mkdirSync(new URL('../data/', import.meta.url), { recursive: true });
  writeFileSync(ARQUIVO_EDICOES, JSON.stringify(ed, null, 1));
  let r: { erros: string[]; avisos: string[] };
  try { r = validarJogo(); } catch (erro) { r = { erros: [`Erro ao montar o jogo: ${(erro as Error).message}`], avisos: [] }; }
  if (r.erros.length) {
    if (antes === null) unlinkSync(ARQUIVO_EDICOES);
    else writeFileSync(ARQUIVO_EDICOES, antes);
    return { ok: false, erros: r.erros, avisos: r.avisos };
  }
  if (antes !== null) {
    mkdirSync(pastaHistorico, { recursive: true });
    writeFileSync(new URL(`edicoes-${new Date().toISOString().replace(/[:.]/g, '-')}.json`, pastaHistorico), antes);
  }
  const { versao } = construir();
  return { ok: true, erros: [], avisos: r.avisos, versao };
}

// ---------- simulação ----------
let sim: { rodando: boolean; inicio: string; fim: string | null; porPar: number; saida: string; codigo: number | null } | null = null;

function lerJSON(u: URL) { try { return existsSync(u) ? JSON.parse(readFileSync(u, 'utf8')) : null; } catch { return null; } }
export function lerSimulacao() {
  return { estado: sim, atual: lerJSON(arqSim), anterior: lerJSON(arqSimAnterior) };
}

export function iniciarSimulacao(porPar: number) {
  if (sim?.rodando) return { ok: false, erro: 'Já tem uma simulação rodando.' };
  const n = Math.max(2, Math.min(200, Math.floor(Number(porPar) || 20)));
  if (existsSync(arqSim)) copyFileSync(arqSim, arqSimAnterior);
  sim = { rodando: true, inicio: new Date().toISOString(), fim: null, porPar: n, saida: '', codigo: null };
  const atual = sim;
  const filho = spawn(process.execPath, ['sim/conjuradores.ts', String(n), '--saida', 'editor'], { cwd: raiz });
  const juntar = (b: Buffer) => { atual.saida += b.toString('utf8'); };
  filho.stdout.on('data', juntar);
  filho.stderr.on('data', juntar);
  filho.on('close', (codigo) => { atual.rodando = false; atual.fim = new Date().toISOString(); atual.codigo = codigo; });
  return { ok: true };
}
