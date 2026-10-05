// Cartas provisórias (pedido do autor, 04/10, madrugada): devolvem a identidade aos 9 baralhos depois da regra das cores.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readdirSync, readFileSync } from 'node:fs';
import { avaliar } from '../src/balanco.ts';
import { cartasProvisorias } from '../src/cartas-provisorias.ts';
import { cartasColecao } from '../src/cartas-colecao.ts';
import { validarBaralho } from '../src/motor.ts';
import { textoCarta } from '../app/textos.js';
import { rodada3 } from '../sim/lista.ts';
import { arquetiposDe, catalogoDe, precosDe } from '../sim/variantes.ts';

const raiz = new URL('../../', import.meta.url);
const ler = (p: string) => JSON.parse(readFileSync(new URL(p, raiz), 'utf8'));
const CONJ = rodada3.find((x) => x.id === 'CONJ')!;

test('provisórias: marcadas, 1 Valor cada, ids únicos e texto gerado da mecânica', () => {
  assert.ok(cartasProvisorias.length >= 20 && cartasProvisorias.length <= 50, `${cartasProvisorias.length} cartas`);
  const ids = new Set<string>();
  for (const c of cartasProvisorias) {
    assert.ok(!ids.has(c.id), `id repetido ${c.id}`);
    ids.add(c.id);
    assert.ok(c.id.startsWith('prov-'), c.id);
    assert.equal(c.provisoria, true, c.id);
    assert.equal(c.valores.length, 1, `${c.id} deve ter 1 Valor`);
    assert.equal(c.texto, textoCarta(c), c.id);
  }
});

test('provisórias: dentro da régua (src/balanco.ts), também com os preços das simulações', () => {
  const p = precosDe(CONJ);
  for (const c of cartasProvisorias) {
    const a = avaliar(c);
    assert.ok(a.folga >= -1e-9, `${c.id} passa do orçamento: ${a.pontos} > ${a.orcamento}`);
    assert.ok(a.folga <= 2.5, `${c.id} fraca demais: folga ${a.folga}`);
    assert.ok(avaliar(c, p).folga >= -1e-9, `${c.id} passa do orçamento com os preços da variante CONJ`);
    for (const n of [c.custo, c.grau, c.ataque ?? 0, c.vida ?? 0]) assert.ok(n >= 0 && n <= 20, `${c.id}: ${n}`);
  }
});

test('os 9 baralhos dos Conjuradores valem pela regra das cores e usam as provisórias', () => {
  const cat = catalogoDe(CONJ);
  const arqs = arquetiposDe(CONJ, cat);
  assert.equal(arqs.length, 9);
  const usadas = new Set<string>();
  for (const a of arqs) {
    const cartas = a.lista.flatMap(([id, n]) => Array(n).fill(id) as string[]);
    assert.deepEqual(validarBaralho(cat, { conjurador: a.conjurador!, cartas }), [], a.nome);
    for (const [id, n] of a.lista) if (id !== 'base-recurso') assert.ok(n <= 4, `${a.nome}: ${n} × ${id}`);
    const prov = a.lista.filter(([id]) => id.startsWith('prov-'));
    assert.ok(prov.length > 0, `${a.nome} sem provisória`);
    for (const [id] of prov) usadas.add(id);
  }
  for (const c of cartasProvisorias) assert.ok(usadas.has(c.id), `${c.id} não está em nenhum baralho`);
});

test('provisórias: arte aprovada, só delas, nome = título, nada da expansão A Mestra dos Dragões', () => {
  const artes = new Map((ler('cartas/catalogo-artes.json').assets as { id: string; titulo: string }[]).map((a) => [a.id, a]));
  const ident = ler('online/dados/identidade.json') as { cartas: Record<string, string>; conjuradores: Record<string, string> };
  const pasta = new URL('expansoes/a-mestra-dos-dragoes/', raiz);
  const daExpansao = new Set(readdirSync(new URL('arte/imagens/', pasta)).map((f) => f.replace(/\.\w+$/, '')));
  const textoExpansao = readdirSync(pasta).filter((f) => f.endsWith('.json')).map((f) => readFileSync(new URL(f, pasta), 'utf8')).join('\n');
  const outras = [...Object.entries(ident.cartas).filter(([id]) => !id.startsWith('prov-')).map(([, a]) => a), ...Object.values(ident.conjuradores), ...cartasColecao.map((c) => c.arte)];
  const vistas = new Set<string>();
  for (const c of cartasProvisorias) {
    const arte = ident.cartas[c.id];
    assert.ok(arte, `${c.id} sem arte em online/dados/identidade.json`);
    assert.ok(artes.has(arte), `${c.id}: ${arte} fora do catálogo aprovado`);
    assert.equal(artes.get(arte)!.titulo, c.nome, `${c.id}: o nome vem do título da arte`);
    assert.ok(!outras.includes(arte) && !vistas.has(arte), `${c.id}: arte ${arte} já usada`);
    vistas.add(arte);
    assert.ok(!daExpansao.has(arte) && !textoExpansao.includes(`"${arte}"`), `${c.id}: arte ${arte} da expansão bloqueada`);
    assert.ok(!/ciantia|philgard|drag|aurelhaz|wyvern/i.test(arte + ' ' + c.nome), `${c.id}: ${arte} lembra a expansão bloqueada`);
  }
});
