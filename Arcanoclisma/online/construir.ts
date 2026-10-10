// Prepara o site online do Arcanoclisma (pasta public/ e os dados que o Worker importa).
// 1. Motor, jogador automático e régua do TCG convertidos para JavaScript de navegador (sem dependências).
// 2. dados.json: a variante CONJ do app de testes (padrão aprovado + 9 Conjuradores + edições do autor em tcg/data/edicoes.json)
//    + a coleção nova (tcg/src/cartas-colecao.ts) + a identidade visual (dados/identidade.json: arte, nome e raridade).
// 3. Artes aprovadas usadas pelas cartas, Conjuradores e cenários, reduzidas para WebP (public/arte/).
// 4. Kit visual do Codex (Arcanoclisma/arena): molduras, ícones, verso e CSS das cartas, copiados sem alteração (public/kit/).
// Uso: node construir.ts   (ou npm run construir)
import { stripTypeScriptTypes } from 'node:module';
import { createHash } from 'node:crypto';
import { copyFileSync, existsSync, mkdirSync, readdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import sharp from 'sharp';

import { REGRAS_PADRAO, validarBaralho } from '../tcg/src/motor.ts';
import { rodada3 } from '../tcg/sim/lista.ts';
import { contextoDe } from '../tcg/sim/nucleo.ts';
import { estiloDe, regrasDe } from '../tcg/sim/variantes.ts';
import { cartasColecao } from '../tcg/src/cartas-colecao.ts';
import { artesDaExpansao, cartasExpansaoDragoes, conjuradoresExpansaoDragoes } from '../tcg/src/expansao-dragoes.ts';
import { cartaServeConjurador, itemServe } from '../tcg/src/motor.ts';
import type { CartaDef, Catalogo } from '../tcg/src/tipos.ts';
// @ts-ignore: módulo JS do app de testes (textos gerados a partir da mecânica)
import { textoCarta } from '../tcg/app/textos.js';
import { aplicarValoresDoSite } from './valores.ts';
import { gerarLore } from './lore.ts';

const aqui = (p: string) => fileURLToPath(new URL(p, import.meta.url));
const raizArcano = aqui('../');
const publico = aqui('./public/');
const garantir = (p: string) => mkdirSync(p, { recursive: true });

export const MAX_COPIAS = 3;
/** Cartas sem limite de cópias no construtor de baralhos (o recurso básico, como terrenos básicos). */
export const SEM_LIMITE: string[] = []; // pedido do autor (03/10): a Mina Funda também tem o limite de cópias

type Identidade = {
  raridadePorGrau: Record<string, number>;
  cartas: Record<string, string>;
  conjuradores: Record<string, string>;
  cenarios: Record<string, string> & { valores: Record<string, string> };
};
type Arte = { numero: number; id: string; titulo: string; arquivo: string; largura: number; altura: number };

function motorJS(): void {
  const dest = publico + 'jogo/';
  garantir(dest);
  for (const [de, para] of [['tcg/src/motor.ts', 'motor.js'], ['tcg/src/tipos.ts', 'tipos.js'], ['tcg/src/balanco.ts', 'balanco.js'], ['tcg/sim/bot.ts', 'bot.js'], ['online/src/edicoes.ts', 'edicoes.js'], ['tcg/src/poder-conjurador.ts', 'poder-conjurador.js']]) {
    let js = stripTypeScriptTypes(readFileSync(raizArcano + de, 'utf8'));
    js = js.replace(/from\s+'(?:\.\.\/src\/|\.\.\/sim\/|\.\/|\.\.\/\.\.\/tcg\/src\/)([\w-]+)\.ts'/g, "from './$1.js'");
    writeFileSync(dest + para, `// Gerado por online/construir.ts a partir de ${de}. Não edite: edite o .ts e reconstrua.\n${js}`);
  }
  copyFileSync(raizArcano + 'tcg/app/textos.js', dest + 'textos.js');
}

function raridade(c: { grau: number; tipo: string }, id: Identidade): string {
  const r = id.raridadePorGrau;
  if (c.grau >= r.lendaria) return 'lendaria';
  if (c.grau >= r.rara) return 'rara';
  if (c.grau >= r.incomum) return 'incomum';
  return 'comum';
}

function dados(artes: Map<string, Arte>, ident: Identidade) {
  const v = rodada3.find((x) => x.id === 'CONJ')!;
  const ctx = contextoDe(v);
  const catalogo: Catalogo = { cartas: { ...ctx.catalogo.cartas }, conjuradores: {} };
  for (const [id, c] of Object.entries(ctx.catalogo.conjuradores)) if (id !== 'padrao') catalogo.conjuradores[id] = c;
  const visual: Record<string, { arte: string | null; raridade: string | null; nomeMecanico?: string }> = {};
  const usadas = new Map<string, string>();
  const usar = (arte: string, quem: string) => {
    if (!artes.has(arte)) throw new Error(`Arte desconhecida em ${quem}: ${arte}`);
    if (usadas.has(arte)) throw new Error(`Arte ${arte} usada duas vezes (${usadas.get(arte)} e ${quem})`);
    usadas.set(arte, quem);
  };
  for (const [cid, arte] of Object.entries(ident.conjuradores)) usar(arte, `conjurador ${cid}`);
  for (const c of cartasColecao) {
    if (catalogo.cartas[c.id]) throw new Error(`Carta da coleção repete id: ${c.id}`);
    const { arte, ...def } = c;
    catalogo.cartas[c.id] = def as CartaDef;
    usar(arte, c.id);
    visual[c.id] = { arte, raridade: null };
  }
  for (const [cid, c] of Object.entries(catalogo.cartas)) {
    if (visual[cid]) continue;
    const arte = ident.cartas[cid] ?? null;
    if (arte) {
      usar(arte, cid);
      visual[cid] = { arte, raridade: null, nomeMecanico: c.nome };
      catalogo.cartas[cid] = { ...c, nome: artes.get(arte)!.titulo };
    } else visual[cid] = { arte: null, raridade: null };
  }
  // itens: texto sempre gerado da mecânica (custo, Grau e efeitos mudam com o Editor; pedido do autor, 04/10)
  for (const [cid, c] of Object.entries(catalogo.cartas)) if (!c.texto || c.tipo === 'item') catalogo.cartas[cid] = { ...c, texto: textoCarta(c) };
  const baralhos = ctx.arqs.map((a) => ({
    id: `pronto-${a.conjurador}`, conjurador: a.conjurador, nome: `${catalogo.conjuradores[a.conjurador].nome.split(',')[0]} (pronto)`,
    estilo: estiloDe(a, v), lista: a.lista, itens: a.itens ?? [],
  }));
  // Valores (pedido do autor, 03/10, noite): sem carta neutra, 1 Valor quase sempre, 9 minas; baralhos prontos só com os Valores do Conjurador
  const valoresSite = aplicarValoresDoSite(catalogo, baralhos, visual);
  console.log(`Valores: ${JSON.stringify(valoresSite.porQtd)} (nº de Valores → cartas); por Valor ${JSON.stringify(valoresSite.porValor)}; ${valoresSite.trocas.length} troca(s) nos baralhos prontos por Valor de fora; ${valoresSite.trocasCores.length} pela regra das cores (decisão 37).`);
  if (valoresSite.trocasCores.length) console.log('  ' + valoresSite.trocasCores.join('\n  '));
  // Limite de cópias (pedido do autor, 03/10: até 3): o excesso dos baralhos prontos passa para outras cartas do mesmo
  // baralho que ainda têm menos de 3, de preferência do mesmo tipo e custo próximo. As simulações (tcg/sim) não mudam.
  const ajustesCopias: string[] = [];
  for (const b of baralhos) {
    const qtd = new Map(b.lista);
    for (const [id, n] of b.lista) {
      if (SEM_LIMITE.includes(id)) continue;
      for (let sobra = n - MAX_COPIAS; sobra > 0; sobra--) {
        const c = catalogo.cartas[id];
        const destino = [...qtd.keys()]
          .filter((x) => x !== id && !SEM_LIMITE.includes(x) && qtd.get(x)! < MAX_COPIAS)
          .map((x) => ({ x, d: catalogo.cartas[x] }))
          .sort((p, q) => Number(p.d.tipo !== c.tipo) - Number(q.d.tipo !== c.tipo) || Math.abs(p.d.custo - c.custo) - Math.abs(q.d.custo - c.custo) || Math.abs(p.d.grau - c.grau) - Math.abs(q.d.grau - c.grau))[0]?.x;
        if (!destino) throw new Error(`Baralho pronto ${b.id}: sem carta para receber o excesso de ${id}`);
        qtd.set(id, qtd.get(id)! - 1);
        qtd.set(destino, qtd.get(destino)! + 1);
        ajustesCopias.push(`${b.id}: 1 ${catalogo.cartas[id].nome} → ${catalogo.cartas[destino].nome}`);
      }
    }
    b.lista = [...qtd].filter(([, n]) => n > 0);
  }
  if (ajustesCopias.length) console.log(`Limite de ${MAX_COPIAS} cópias: ${ajustesCopias.length} troca(s) nos baralhos prontos.\n  ${ajustesCopias.join('\n  ')}`);
  // Expansão A Mestra dos Dragões (liberada em 09/10, decisão 41): entra depois dos Valores do site (os Valores vêm da carta)
  // e dos baralhos prontos antigos (que não mudam). Cada Conjurador novo ganha um baralho pronto montado a partir das cartas
  // da expansão e dos prontos dos Conjuradores que dividem um Valor com ele, com Durandis no Extra.
  for (const c of cartasExpansaoDragoes) {
    if (catalogo.cartas[c.id]) throw new Error(`Carta da expansão repete id: ${c.id}`);
    const { arteExpansao, ...def } = c;
    void arteExpansao;
    catalogo.cartas[c.id] = def as CartaDef;
    visual[c.id] = { arte: null, raridade: null };
  }
  for (const c of conjuradoresExpansaoDragoes) {
    if (catalogo.conjuradores[c.id]) throw new Error(`Conjurador da expansão repete id: ${c.id}`);
    const { arteExpansao, ...def } = c;
    void arteExpansao;
    catalogo.conjuradores[c.id] = def;
  }
  baralhos.push(...prontosDaExpansao(catalogo, baralhos, valoresSite.minas));
  for (const b of baralhos) {
    const erros = validarBaralho(catalogo, { conjurador: b.conjurador, cartas: b.lista.flatMap(([id, n]) => Array(n).fill(id)), itens: b.itens });
    if (erros.length) throw new Error(`Baralho pronto ${b.id} inválido: ${erros.join(' ')}`);
  }
  const conteudo = {
    variante: v.id, regras: { ...REGRAS_PADRAO, ...regrasDe(v) }, catalogo, baralhos, visual,
    artesFinais: { ...artesFinaisAprovadas(catalogo), ...artesDaExpansaoNoSite(catalogo) },
    /** Mina Funda de cada Valor (o construtor completa o baralho com as do Conjurador). */
    minas: valoresSite.minas,
    /** O GPT já entregou o módulo de efeitos da mesa (public/js/mesa-efeitos-codex.js)? */
    efeitosCodex: existsSync(publico + 'js/mesa-efeitos-codex.js'),
    conjuradoresArte: ident.conjuradores, cenarios: ident.cenarios, maxCopias: MAX_COPIAS, semLimite: SEM_LIMITE, tamanhoBaralho: 60,
  };
  const versao = createHash('sha1').update(JSON.stringify(conteudo)).digest('hex').slice(0, 10);
  return { versao, geradoEm: new Date().toISOString(), ...conteudo };
}

async function reduzir(origem: string, destino: string, largura: number): Promise<void> {
  if (existsSync(destino) && statSync(destino).mtimeMs >= statSync(origem).mtimeMs) return;
  garantir(dirname(destino));
  await sharp(origem).resize({ width: largura, withoutEnlargement: true }).webp({ quality: 82, alphaQuality: 90 }).toFile(destino);
}

/** Artes finais das cartas feitas pelo GPT. Fonte: o mapa corrente do Codex (cartas/entregas/lote-site-v1.visual.json,
 *  campo "arquivo", que pode ter hash no nome, p.ex. os Conjuradores com cenário de 04/10). Só entram as aprovadas
 *  (estado "aprovada..." ou okGpt). Chave: id da carta, ou "conjurador-<id>"; valor: nome do arquivo sem extensão
 *  (muda junto com a arte, então o navegador não fica com a imagem antiga no cache). */
const PASTA_FINAIS = raizArcano + 'arte/cartas-site/';
const MAPA_FINAIS = raizArcano + 'cartas/entregas/lote-site-v1.visual.json';
/** Outras entregas de artes finais no mesmo formato (cartas provisórias, 05/10). */
const MAPAS_EXTRAS = ['cartas-provisorias-v1', 'artes-rapidas-jogo', 'artes-rapidas-valores'].map((n) => raizArcano + 'cartas/entregas/' + n + '.visual.json');
const origemFinal = new Map<string, string>();
type Pronto = { id: string; conjurador: string; nome: string; estilo: unknown; lista: [string, number][]; itens: string[] };
/** Baralhos prontos dos Conjuradores da expansão: 2 de cada carta da expansão que serve, depois as cartas dos prontos que dividem um Valor (até 3 cópias). */
function prontosDaExpansao(cat: Catalogo, antigos: Pronto[], minas: Record<string, string>): Pronto[] {
  const exp = cartasExpansaoDragoes.filter((c) => c.tipo !== 'item' && !c.ficha);
  return conjuradoresExpansaoDragoes.map((cj) => {
    const conj = cat.conjuradores[cj.id];
    const qtd = new Map<string, number>();
    const total = () => [...qtd.values()].reduce((s, n) => s + n, 0);
    const add = (id: string, n: number) => { const t = Math.min(n, MAX_COPIAS - (qtd.get(id) ?? 0), 60 - total()); if (t > 0) qtd.set(id, (qtd.get(id) ?? 0) + t); };
    for (const c of exp) if (cartaServeConjurador(c, conj)) add(c.id, 2);
    for (const v of conj.valores) if (minas[v]) add(minas[v], 3);
    const fontes = antigos.filter((b) => cat.conjuradores[b.conjurador].valores.some((v) => conj.valores.includes(v)));
    // rodízio entre os prontos que dividem um Valor, uma cópia por vez, para o baralho misturar os dois Valores
    for (let rodada = 0; rodada < MAX_COPIAS && total() < 60; rodada++)
      for (let i = 0; total() < 60 && fontes.some((b) => i < b.lista.length); i++)
        for (const b of fontes) { const x = b.lista[i]; if (x && x[1] > rodada && cartaServeConjurador(cat.cartas[x[0]], conj)) add(x[0], 1); }
    if (total() < 60) throw new Error(`Pronto da expansão ${cj.id}: só ${total()} cartas`);
    const base = fontes[0];
    const item = (esp: string) => base.itens.find((id) => cat.cartas[id]?.categoriaItem === esp && itemServe(conj, cat.cartas[id])) ?? { canal: 'item-canal-cajado', armadura: 'item-armadura-gibao' }[esp]!;
    return { id: `pronto-${cj.id}`, conjurador: cj.id, nome: `${conj.nome.split(',')[0]} (pronto)`, estilo: base.estilo, lista: [...qtd], itens: [item('canal'), item('armadura'), 'md-item-durandis'] };
  });
}

/** Artes da expansão A Mestra dos Dragões (manifesto da coleção de 90 artes): todas as escolhidas pelo autor para a expansão. */
const PASTA_EXPANSAO = raizArcano + 'expansoes/a-mestra-dos-dragoes/arte/';
function artesDaExpansaoNoSite(cat: Catalogo): Record<string, string> {
  const man = JSON.parse(readFileSync(PASTA_EXPANSAO + 'colecao-completa-90.visual.json', 'utf8')) as { itens: { id: string; arquivo: string; estado?: string }[] };
  const porNumero = new Map(man.itens.filter((i) => !/rejeit|descart|retirad/i.test(i.estado ?? '')).map((i) => [i.id, i.arquivo]));
  // Ciantia (arte 001 da fila da expansão, fora do manifesto das 90)
  if (!porNumero.has('001')) porNumero.set('001', 'imagens/001-ciantia-a-mestra-dos-dragoes-v1.png');
  const r: Record<string, string> = {};
  for (const [id, numero] of Object.entries(artesDaExpansao())) {
    const arq = porNumero.get(numero);
    const existe = cat.cartas[id] || cat.conjuradores[id.replace(/^conjurador-/, '')];
    if (!arq || !existe || !existsSync(PASTA_EXPANSAO + arq)) continue;
    const nome = arq.replace(/^.*[/]/, '').replace(/.png$/i, '');
    r[id] = nome;
    origemFinal.set(nome, PASTA_EXPANSAO + arq);
  }
  return r;
}
function artesFinaisAprovadas(cat: Catalogo): Record<string, string> {
  const itens = [MAPA_FINAIS, ...MAPAS_EXTRAS].filter((m) => existsSync(m)).flatMap((m) => (JSON.parse(readFileSync(m, 'utf8')).cartas ?? []) as { id: string; arquivo?: string; estado?: string; okGpt?: boolean }[]);
  const r: Record<string, string> = {};
  for (const i of itens) {
    const aprovada = i.okGpt === true || /^aprovad/.test(i.estado ?? '');
    const existe = cat.cartas[i.id] || cat.conjuradores[i.id.replace(/^conjurador-/, '')];
    const arquivo = i.arquivo ? raizArcano + '../' + i.arquivo : PASTA_FINAIS + i.id + '.png';
    if (!aprovada || !existe || !existsSync(arquivo)) continue;
    const nome = arquivo.replace(/^.*[\/]/, '').replace(/.png$/i, '');
    r[i.id] = nome;
    origemFinal.set(nome, arquivo);
  }
  return r;
}

async function artesDoSite(artes: Map<string, Arte>, d: ReturnType<typeof dados>): Promise<number> {
  const tarefas: [string, string, number][] = [];
  const add = (id: string, largura: number, pasta = 'arte/') => {
    const a = artes.get(id);
    if (!a) throw new Error(`Arte desconhecida: ${id}`);
    tarefas.push([raizArcano + '../' + a.arquivo, `${publico}${pasta}${id}.webp`, largura]);
  };
  // Todas as artes aprovadas (o editor do autor pode trocar a arte de qualquer carta), com a lista em jogo/artes.json.
  const deConj = new Set(Object.values(d.conjuradoresArte));
  for (const a of artes.keys()) if (!deConj.has(a)) add(a, 480);
  for (const a of Object.values(d.conjuradoresArte)) add(a, 640);
  // 1024 px de largura: nítida na carta ampliada sem levar os PNG nativos (~3 MB cada) para o site
  for (const nome of Object.values(d.artesFinais)) tarefas.push([origemFinal.get(nome)!, `${publico}arte/finais/${nome}.webp`, 1024]);
  const cen = d.cenarios;
  for (const id of new Set([...Object.entries(cen).filter(([k]) => k !== 'valores').map(([, x]) => x as string), ...Object.values(cen.valores)])) { add(id, 1600, 'arte/cenarios/'); add(id, 420, 'arte/cenarios/mini/'); }
  let feitas = 0;
  const fila = [...tarefas];
  await Promise.all(Array.from({ length: 6 }, async () => {
    for (let t = fila.shift(); t; t = fila.shift()) { await reduzir(...t); feitas++; }
  }));
  return feitas;
}

async function kit(): Promise<void> {
  const arena = raizArcano + 'arena/';
  const dest = publico + 'kit/';
  for (const p of ['vetores', 'templates', 'raster']) garantir(dest + p);
  for (const f of ['cartas.css', 'acabamento-v9.css']) copyFileSync(arena + f, dest + f);
  for (const f of readdirSync(arena + 'vetores')) if (f.endsWith('.svg')) copyFileSync(arena + 'vetores/' + f, dest + 'vetores/' + f);
  for (const f of readdirSync(arena + 'templates')) if (f.endsWith('.svg') && /-(v4|v8)\.svg$/.test(f)) copyFileSync(arena + 'templates/' + f, dest + 'templates/' + f);
  const rasters: [string, number][] = [['verso-arcano-v3', 480], ['arena-jogador-1-ampla-larga-v2', 1920], ['arena-jogador-2-ampla-larga-v2', 1920], ['ornamento-arcano', 800]];
  for (const [r, w] of rasters) await reduzir(arena + `raster/${r}.png`, dest + `raster/${r}.webp`, w);
}

export async function construirSite(): Promise<{ versao: string }> {
  const cat = JSON.parse(readFileSync(raizArcano + 'cartas/catalogo-artes.json', 'utf8')) as { assets: Arte[] };
  const artes = new Map(cat.assets.map((a) => [a.id, a]));
  const ident = JSON.parse(readFileSync(aqui('./dados/identidade.json'), 'utf8')) as Identidade;
  motorJS();
  const d = dados(artes, ident);
  garantir(publico + 'jogo/');
  writeFileSync(publico + 'jogo/dados.json', JSON.stringify(d));
  garantir(aqui('./src/gerado/'));
  writeFileSync(aqui('./src/gerado/dados.json'), JSON.stringify(d));
  // aba Lore: Valores, atlas do mundo (só entradas com arte aprovada) e novidades do git
  const lore = gerarLore(raizArcano, (id) => artes.has(id));
  writeFileSync(publico + 'jogo/lore.json', JSON.stringify(lore));
  console.log(`online/public: lore com ${lore.valores.length} Valores, ${lore.categorias.reduce((t, c) => t + c.itens.length, 0)} verbetes em ${lore.categorias.length} categorias e ${lore.novidades.length} novidades.`);
  writeFileSync(publico + 'jogo/artes.json', JSON.stringify([...artes.values()].map((a) => ({ id: a.id, titulo: a.titulo, numero: a.numero }))));
  const n = await artesDoSite(artes, d);
  await kit();
  const nCartas = Object.keys(d.catalogo.cartas).length;
  console.log(`online/public: ${Object.keys(d.artesFinais).length} arte(s) final(is) aprovada(s) pelo GPT.`);
  console.log(`online/public: versão ${d.versao}, ${nCartas} cartas (${cartasColecao.length} da coleção nova), ${Object.keys(d.catalogo.conjuradores).length} Conjuradores, ${d.baralhos.length} baralhos prontos, ${n} artes.`);
  return { versao: d.versao };
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) await construirSite();
