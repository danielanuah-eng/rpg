// Editor de Conjuradores (pedido do autor, 04/10): o poder de cada Conjurador abre no construtor de efeitos das cartas,
// é editado campo a campo e volta ao formato do poder sem perder nada; o poder editado vale na partida (motor e bot).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { criarJogo, etereo, executar, origemDaCarta } from '../src/motor.ts';
import { conjuradoresPorValor } from '../src/conjuradores.ts';
import { construtorParaPoder, poderParaConstrutor } from '../src/poder-conjurador.ts';
import { decidir } from '../sim/bot.ts';
import { textoPoder } from '../app/textos.js';
import { aplicarCatalogo, sanitizarEdicoes } from '../../online/src/edicoes.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador, PoderConjurador } from '../src/tipos.ts';

const json = (v: unknown) => JSON.stringify(v);
/** O Editor guarda a visão do construtor como JSON (rascunho, textarea): a ida e volta passa por aí. */
const idaEVolta = (p: PoderConjurador) => construtorParaPoder(JSON.parse(JSON.stringify(poderParaConstrutor(p))));

test('ida e volta dos 9 poderes atuais: mesmo JSON (chaves e ordem), mesmo texto, original intacto', () => {
  assert.equal(conjuradoresPorValor.length, 9);
  for (const c of conjuradoresPorValor) {
    const antes = json(c.poder);
    const volta = idaEVolta(c.poder!);
    assert.equal(json(volta), antes, c.id);
    assert.equal(json(c.poder), antes, `${c.id}: o original não muda`);
    assert.equal(textoPoder(volta), textoPoder(c.poder), c.id);
  }
});

test('ida e volta com as edições do autor aplicadas (data/edicoes.json só lido)', () => {
  const ed = JSON.parse(readFileSync(new URL('../data/edicoes.json', import.meta.url), 'utf8'));
  for (const c of conjuradoresPorValor) {
    const p = { ...c, ...(ed.conjuradores?.[c.id] ?? {}) }.poder;
    assert.equal(json(idaEVolta(p)), json(p), c.id);
  }
});

test('ida e volta de um poder com todos os campos (ativação completa, gatilho, passivas, permissões, habilidades, campo desconhecido)', () => {
  const p = {
    gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'comprar', valor: 1, requisito: { grau: 2 } }], soNoSeuTurno: true },
    ativacao: { repentina: true, custo: 0, suspende: false, efeitos: [{ tipo: 'reanimar', custoExtra: 1, concessao: { palavras: ['Etéreo'], turnos: 2, destino: 'banimento' } }], descartar: 1, danoNoConjurador: 2, requisito: { grau: 3, arcano: 1 }, suspendeConjurador: true, consome: false },
    aliadosProtegidos: 2,
    primeiraCartaMaisBarata: 1,
    permiteJogarDe: [{ de: 'cemiterio', tipos: ['aliado'], custoMax: 3, banir: true, umaVezPorTurno: true }],
    habilidades: [
      { quando: 'ativada', custo: { suspender: true, arcano: 2, grau: 1 }, efeitos: [{ tipo: 'campo', acao: 'dano', valor: 2, alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }], repentina: true },
      { quando: 'atacar', umaVezPorTurno: true, efeitos: [{ tipo: 'arcano', valor: 1 }] },
    ],
    futuro: { qualquer: [1, 2] },
  } as unknown as PoderConjurador;
  assert.equal(json(idaEVolta(p)), json(p));
  assert.equal(json(idaEVolta({})), '{}');
});

test('o construtor vê o que cada Conjurador tem e faz', () => {
  const v = (id: string) => poderParaConstrutor(conjuradoresPorValor.find((c) => c.id === id)!.poder);
  assert.deepEqual(v('lyandra'), { habilidades: [], primeiroDanoDeAliadoMais: 1, _ordem: ['primeiroDanoDeAliadoMais'] });
  assert.equal(v('osmar').aliadosProtegidos, 1);
  assert.deepEqual(v('carmal').habilidades.map((h) => [h.quando, h.umaVezPorTurno]), [['aliadoEntra', true]]);
  assert.equal(v('agnelo').habilidades[0].soNoSeuTurno, true);
  const rica = v('ricardo').habilidades[0];
  assert.equal(rica.quando, 'ativada');
  assert.deepEqual(rica.custo, { suspender: true });
  const mit = v('mitonho').habilidades[0];
  assert.equal(mit.repentina, true);
  assert.deepEqual(mit.custo, { suspender: true, arcano: 2 });
  assert.deepEqual(v('ana').habilidades[0].efeitos, [{ tipo: 'banirDaMao', desconto: 4 }]);
  assert.deepEqual(v('diastaros').habilidades[0].efeitos[0], { tipo: 'reanimar', custoExtra: 2, concessao: { palavras: ['Etéreo'] } });
  assert.deepEqual(v('roh').habilidades[0].efeitos[0], { tipo: 'devolver', alvo: 'escolhidoProprio' });
});

// ---------- o poder editado vale na partida ----------
const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const cartas: CartaDef[] = [
  { ...base, id: 'enchimento', nome: 'Enchimento', tipo: 'aliado', custo: 1, ataque: 1, vida: 1 },
  { ...base, id: 'barato', nome: 'Barato', tipo: 'temporaria', custo: 1, efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'mata', nome: 'Mata', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'ambos', tipos: ['aliado'] }] },
];
function jogo(conj: ConjuradorDef): Estado {
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { [conj.id]: conj } };
  const deck = { conjurador: conj.id, cartas: Array.from({ length: 30 }, (_, i) => ['enchimento', 'barato', 'mata'][i % 3]) };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 3, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
const passarTodos = (e: Estado) => { executar(e, { tipo: 'passar', jogador: e.prioridade }); executar(e, { tipo: 'passar', jogador: e.prioridade }); };
const resolver = (e: Estado) => { while (e.pilha.length) passarTodos(e); };
function pegar(e: Estado, j: Jogador, defId: string, para: 'mao' | 'campo' | 'cemiterio'): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId && !z.campo.includes(i))!;
  for (const zona of [z.baralho, z.mao]) { const k = zona.indexOf(id); if (k >= 0) zona.splice(k, 1); }
  z[para].push(id);
  return id;
}
const conjBase = (poder: PoderConjurador): ConjuradorDef => ({ id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0, habilidade: '', poder });

test('Diástaros editado no construtor (um efeito a mais) vale na partida pelas edições do site', () => {
  const dia = conjuradoresPorValor.find((c) => c.id === 'diastaros')!;
  const v = poderParaConstrutor(dia.poder);
  v.habilidades[0].efeitos.push({ tipo: 'comprar', valor: 1 });
  const poder = construtorParaPoder(JSON.parse(JSON.stringify(v)));
  assert.equal(poder.ativacao?.efeitos.length, 2, 'continua sendo a ativação dele');
  assert.match(textoPoder(poder), /compre 1 carta/);
  const catBase: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { diastaros: dia } };
  const ed = sanitizarEdicoes({ conjuradores: { diastaros: { poder } } }, catBase);
  const cat = aplicarCatalogo(catBase, ed);
  assert.deepEqual(cat.conjuradores.diastaros.poder, poder);
  assert.equal(dia.poder!.ativacao!.efeitos.length, 1, 'o Diástaros original não muda');
  const e = jogo(cat.conjuradores.diastaros);
  const z = e.jogadores[0];
  const morto = pegar(e, 0, 'enchimento', 'cemiterio');
  z.arcano = 5;
  const mao = z.mao.length;
  executar(e, { tipo: 'ativar', jogador: 0, carta: z.conjuradorId, alvo: morto });
  resolver(e);
  assert.ok(z.campo.includes(morto));
  assert.equal(etereo(e, morto), true, 'a concessão Etéreo continua');
  assert.equal(z.mao.length, mao + 1, 'e o efeito novo (compre 1) acontece');
  assert.equal(z.arcano, 5 - 1 - 2, 'paga o custo do Aliado + 2');
});

test('habilidade ativada nova no poder: comando ativar com o índice; Repentina vale no turno do oponente, a outra não', () => {
  const e = jogo(conjBase({ habilidades: [
    { quando: 'ativada', repentina: true, custo: { arcano: 1 }, efeitos: [{ tipo: 'comprar', valor: 1 }] },
    { quando: 'ativada', custo: { suspender: true }, efeitos: [{ tipo: 'arcano', valor: 2 }] },
  ] }));
  // passa para o turno do jogador 1
  while (e.ativo === 0) { if (e.pendencia?.tipo === 'descartar') executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); else executar(e, { tipo: 'passar', jogador: e.prioridade }); }
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  const z = e.jogadores[0];
  z.arcano = 3;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: z.conjuradorId, habilidade: 1 }), /fase principal/);
  // jogador 1 joga algo; o jogador 0 responde com a Repentina
  const b = pegar(e, 1, 'barato', 'mao');
  e.jogadores[1].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 1, carta: b });
  executar(e, { tipo: 'passar', jogador: 1 });
  const mao = z.mao.length;
  executar(e, { tipo: 'ativar', jogador: 0, carta: z.conjuradorId, habilidade: 0 });
  resolver(e);
  assert.equal(z.mao.length, mao + 1);
  assert.equal(z.arcano, 2);
});

test('gatilhos novos do poder: sem "uma vez por turno" dispara a cada Aliado; "só no seu turno" não dispara no do oponente', () => {
  const e = jogo(conjBase({ habilidades: [
    { quando: 'aliadoMorre', efeitos: [{ tipo: 'arcano', valor: 1 }] },
    { quando: 'aliadoMorre', umaVezPorTurno: true, soNoSeuTurno: true, efeitos: [{ tipo: 'arcano', valor: 10 }] },
  ] }));
  const z = e.jogadores[0];
  const a = pegar(e, 0, 'enchimento', 'campo');
  const b = pegar(e, 0, 'enchimento', 'campo');
  z.arcano = 0;
  for (const alvo of [a, b]) { const m = pegar(e, 0, 'mata', 'mao'); executar(e, { tipo: 'jogar', jogador: 0, carta: m, alvo }); resolver(e); }
  assert.ok(z.cemiterio.includes(a) && z.cemiterio.includes(b));
  assert.equal(z.arcano, 1 + 1 + 10, 'cada morte dá 1; a de 10 só uma vez no turno');
  // no turno do oponente, a "só no seu turno" não dispara
  while (e.ativo === 0) { if (e.pendencia?.tipo === 'descartar') executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); else executar(e, { tipo: 'passar', jogador: e.prioridade }); }
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  const c = pegar(e, 0, 'enchimento', 'campo');
  z.arcano = 0;
  const m = pegar(e, 1, 'mata', 'mao');
  executar(e, { tipo: 'jogar', jogador: 1, carta: m, alvo: c });
  resolver(e);
  assert.ok(z.cemiterio.includes(c));
  assert.equal(z.arcano, 1);
});

test('condição do próprio Conjurador no poder: "quando o Conjurador ataca" vai para a pilha', () => {
  const e = jogo(conjBase({ habilidades: [{ quando: 'atacar', efeitos: [{ tipo: 'arcano', valor: 2 }] }] }));
  const z = e.jogadores[0];
  z.arcano = 0;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: z.conjuradorId, alvo: e.jogadores[1].conjuradorId });
  resolver(e);
  assert.equal(z.arcano, 2);
});

test('permissão contínua do poder: joga do cemitério pagando o custo, vai para o banimento, uma por turno', () => {
  const e = jogo(conjBase({ permiteJogarDe: [{ de: 'cemiterio', tipos: ['temporaria'], banir: true, umaVezPorTurno: true }] }));
  const z = e.jogadores[0];
  const t1 = pegar(e, 0, 'barato', 'cemiterio');
  const t2 = pegar(e, 0, 'barato', 'cemiterio');
  const al = pegar(e, 0, 'enchimento', 'cemiterio');
  assert.equal(origemDaCarta(e, 0, t1), 'cemiterio');
  assert.equal(origemDaCarta(e, 0, al), null, 'só os tipos permitidos');
  z.arcano = 2;
  executar(e, { tipo: 'jogar', jogador: 0, carta: t1 });
  resolver(e);
  assert.equal(z.arcano, 1, 'paga o custo normal');
  assert.ok(z.banimento.includes(t1), 'depois vai para o banimento');
  assert.equal(origemDaCarta(e, 0, t2), null, 'uma por turno');
  assert.equal(origemDaCarta(e, 1, t2), null, 'o oponente não joga do seu cemitério');
});

test('bot usa a habilidade ativada nova do poder (comando com o índice)', () => {
  const e = jogo(conjBase({ habilidades: [{ quando: 'ativada', custo: { suspender: true }, efeitos: [{ tipo: 'comprar', valor: 1 }] }] }));
  const z = e.jogadores[0];
  z.mao.splice(0).forEach((id) => z.cemiterio.push(id));
  const est = { nome: 'teste', agressividade: 0, reservaInstantes: false, grauAlvo: 0, queimaFace: false };
  let achou = false;
  for (let i = 0; i < 20 && !achou && e.ativo === 0; i++) {
    const cmd = decidir(e, 0, est);
    if (cmd.tipo === 'ativar' && cmd.carta === z.conjuradorId) { assert.equal(cmd.habilidade, 0); achou = true; }
    executar(e, cmd);
  }
  assert.ok(achou);
  assert.ok(e.instancias[z.conjuradorId].suspensa);
});
