// Habilidades dos Conjuradores (rodada 7): gatilhos, passivas e ativadas.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, custoProtecao, executar, validarBaralho } from '../src/motor.ts';
import { conjuradoresPorValor } from '../src/conjuradores.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador, PoderConjurador } from '../src/tipos.ts';
import { rodada3 } from '../sim/lista.ts';
import { baralhoDe } from '../sim/baralhos.ts';
import { contextoDe } from '../sim/nucleo.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const cartas: CartaDef[] = [
  { ...base, id: 'soldado', nome: 'Soldado', tipo: 'aliado', custo: 0, ataque: 2, vida: 3, palavrasChave: ['Proficiente'] },
  { ...base, id: 'barato', nome: 'Barato', tipo: 'temporaria', custo: 1, efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'rapido', nome: 'Rápido', tipo: 'temporaria', custo: 1, velocidade: 'repentina', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'anula', nome: 'Anula', tipo: 'temporaria', custo: 0, velocidade: 'repentina', efeitos: [{ tipo: 'anular', alvo: 'pilha' }] },
  { ...base, id: 'bomba', nome: 'Bomba', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'dano', valor: 9, alvo: 'escolhidoAliado' }] },
];
function criar(poder: PoderConjurador, tamanho = 30): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0, habilidade: '', poder };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const ids = cartas.map((c) => c.id);
  const deck = { conjurador: 'c', cartas: Array.from({ length: tamanho }, (_, i) => ids[i % ids.length]) };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 3, regras: { primeiroSemGrauNoTurno1: false } });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
const resolver = (e: Estado) => { while (e.pilha.length) { executar(e, { tipo: 'passar', jogador: e.prioridade }); executar(e, { tipo: 'passar', jogador: e.prioridade }); } };
function daMao(e: Estado, j: Jogador, defId: string): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId)!;
  const ib = z.baralho.indexOf(id);
  if (ib >= 0) { z.baralho.splice(ib, 1); z.mao.push(id); }
  return id;
}
function emCampo(e: Estado, j: Jogador, defId: string): string {
  const id = daMao(e, j, defId);
  e.jogadores[j].mao.splice(e.jogadores[j].mao.indexOf(id), 1);
  e.jogadores[j].campo.push(id);
  return id;
}

test('os 9 Conjuradores: 2 Valores diferentes cada, cada Valor em exatamente 2, habilidade própria e baralho legal de 60', () => {
  assert.equal(conjuradoresPorValor.length, 9);
  const uso: Record<string, number> = {};
  for (const c of conjuradoresPorValor) {
    assert.equal(new Set(c.valores).size, 2, c.id);
    for (const v of c.valores) uso[v] = (uso[v] ?? 0) + 1;
    assert.ok(c.poder && Object.keys(c.poder).length === 1 && c.habilidade.length > 10, c.id);
  }
  assert.equal(Object.keys(uso).length, 9);
  assert.ok(Object.values(uso).every((n) => n === 2), JSON.stringify(uso));
  const ctx = contextoDe(rodada3.find((x) => x.id === 'CONJ')!);
  for (const a of ctx.arqs) assert.deepEqual(validarBaralho(ctx.catalogo, baralhoDe(a, a.conjurador!)), [], a.nome);
});

test('gatilho aliadoEntra (uma vez por turno): o primeiro Aliado dá 1 Arcano, o segundo não', () => {
  const e = criar({ gatilho: { quando: 'aliadoEntra', efeitos: [{ tipo: 'arcano', valor: 1 }] } });
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'soldado') });
  resolver(e);
  assert.equal(e.jogadores[0].arcano, 1);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'soldado') });
  resolver(e);
  assert.equal(e.jogadores[0].arcano, 1);
});

test('gatilho aliadoMorre: compra 1 carta quando um Aliado seu vai para o cemitério', () => {
  const e = criar({ gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'comprar', valor: 1 }] } });
  const alvo = emCampo(e, 0, 'soldado');
  const mao = e.jogadores[0].mao.length;
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'bomba'), alvo });
  resolver(e);
  assert.ok(e.jogadores[0].cemiterio.includes(alvo));
  assert.equal(e.jogadores[0].mao.length, mao + 1 - 1 + 1); // bomba saiu da mão, +1 do gatilho (daMao já a pôs na mão)
});

test('gatilho aliadoDanoConjurador: o Conjurador inimigo sofre 1 de dano a mais', () => {
  const e = criar({ gatilho: { quando: 'aliadoDanoConjurador', efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] } });
  const s = emCampo(e, 0, 'soldado');
  e.instancias[s].entrouNoTurno = 0;
  const conjInimigo = e.jogadores[1].conjuradorId;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: s, alvo: conjInimigo });
  resolver(e);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: null });
  executar(e, { tipo: 'passar', jogador: e.prioridade });
  executar(e, { tipo: 'passar', jogador: e.prioridade }); // dano de combate; o gatilho entra na pilha
  resolver(e);
  assert.equal(e.instancias[conjInimigo].dano, 3); // 2 do ataque + 1 da habilidade
});

test('passiva primeiraCartaMaisBarata: só a primeira carta do turno custa 1 a menos', () => {
  const e = criar({ primeiraCartaMaisBarata: 1 });
  e.jogadores[0].arcano = 0;
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'barato') }); // custo 1 → 0
  resolver(e);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'barato') }), /Arcano|custo/i);
});

test('passiva primeiraRepentinaMaisBarata: vale só para Repentinas', () => {
  const e = criar({ primeiraRepentinaMaisBarata: 1 });
  e.jogadores[0].arcano = 0;
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'barato') }), /Arcano|custo/i);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'rapido') });
  resolver(e);
});

test('passiva aliadosProtegidos: mirar um Aliado do dono custa 1 a mais; Mística e Conjurador não', () => {
  const e = criar({ aliadosProtegidos: 1 });
  const s = emCampo(e, 1, 'soldado');
  assert.equal(custoProtecao(e, 0, s), 1);
  assert.equal(custoProtecao(e, 1, s), 0); // o próprio dono não paga
  assert.equal(custoProtecao(e, 0, e.jogadores[1].conjuradorId), 0);
});

test('ativada do Conjurador: suspende-o, causa o efeito e ele não ataca naquele turno', () => {
  const e = criar({ ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'escolhidoAliado' }] } });
  const alvo = emCampo(e, 1, 'soldado');
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo });
  resolver(e);
  assert.equal(e.instancias[alvo].dano, 1);
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].suspensa, true);
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo }), /suspensa/);
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 0, atacante: e.jogadores[0].conjuradorId, alvo: e.jogadores[1].conjuradorId }), /suspenso/);
});

function ate(e: Estado, turno: number): void {
  let passos = 0;
  while ((e.turno < turno || e.etapa !== 'principal') && passos++ < 200) {
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); continue; }
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
}

test('passiva primeiroDanoDeAliadoMais: só o primeiro dano de combate de Aliado em cada turno ganha o bônus', () => {
  const e = criar({ primeiroDanoDeAliadoMais: 1 });
  const s1 = emCampo(e, 0, 'soldado'); const s2 = emCampo(e, 0, 'soldado');
  e.instancias[s1].entrouNoTurno = 0; e.instancias[s2].entrouNoTurno = 0;
  const alvo = e.jogadores[1].conjuradorId;
  for (const at of [s1, s2]) {
    executar(e, { tipo: 'atacar', jogador: 0, atacante: at, alvo });
    resolver(e);
    executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: null });
    executar(e, { tipo: 'passar', jogador: e.prioridade });
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
  assert.equal(e.instancias[alvo].dano, 3 + 2);
});

test('ativada com desconto: a primeira carta do turno custa 1 a menos, mas só se nada foi jogado antes', () => {
  const e = criar({ ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'descontoProximaCarta', valor: 1 }] } });
  e.jogadores[0].arcano = 0;
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId });
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'barato') }); // custo 1 → 0
  resolver(e);
  assert.equal(e.jogadores[0].arcano, 0);
});

test('ativada devolver próprio: só mira Aliado ou Mística seu', () => {
  const e = criar({ ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'devolver', alvo: 'escolhidoProprio' }] } });
  const inimigo = emCampo(e, 1, 'soldado');
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo: inimigo }), /alvo/);
  const meu = emCampo(e, 0, 'soldado');
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo: meu });
  resolver(e);
  assert.ok(e.jogadores[0].mao.includes(meu));
});

test('banir da mão (zona de banimento): jogável só no próximo turno do dono, com 4 a menos', () => {
  const e = criar({ ativacao: { custo: 2, suspende: true, efeitos: [{ tipo: 'banirDaMao', desconto: 4 }] } });
  e.jogadores[0].arcano = 2;
  const carta = daMao(e, 0, 'bomba'); // custo 0, Grau 0: o desconto só aparece no custo mínimo
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo: carta });
  assert.ok(e.jogadores[0].banimento.includes(carta));
  assert.ok(!e.jogadores[0].mao.includes(carta));
  assert.equal(e.instancias[carta].banidaDisponivel, e.turno + 2);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta, alvo: emCampo(e, 1, 'soldado') }), /mão/);
  ate(e, e.turno + 2);
  assert.equal(e.ativo, 0);
  executar(e, { tipo: 'jogar', jogador: 0, carta, alvo: emCampo(e, 1, 'soldado') });
  resolver(e);
  assert.ok(!e.jogadores[0].banimento.includes(carta));
});

test('banido que não é jogado no turno liberado fica perdido no banimento', () => {
  const e = criar({ ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'banirDaMao', desconto: 4 }] } });
  const carta = daMao(e, 0, 'bomba');
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo: carta });
  ate(e, e.turno + 4);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta, alvo: emCampo(e, 1, 'soldado') }), /mão/);
  assert.ok(e.jogadores[0].banimento.includes(carta));
});

test('reanimar: põe no seu campo um Aliado de qualquer cemitério pagando 2 a mais além do custo dele', () => {
  const e = criar({ ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'reanimar', custoExtra: 2 }] } });
  const caido = daMao(e, 1, 'soldado');
  e.jogadores[1].mao.splice(e.jogadores[1].mao.indexOf(caido), 1);
  e.jogadores[1].cemiterio.push(caido);
  e.jogadores[0].arcano = 1;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo: caido }), /Arcano/);
  e.jogadores[0].arcano = 2;
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId, alvo: caido });
  resolver(e);
  assert.ok(e.jogadores[0].campo.includes(caido));
  assert.ok(!e.jogadores[1].cemiterio.includes(caido));
  assert.equal(e.jogadores[0].arcano, 0);
});

test('Repentina: libera jogar do cemitério uma carta que anula; ao resolver, ela é banida', () => {
  const e = criar({ ativacao: { repentina: true, custo: 0, suspende: true, efeitos: [{ tipo: 'permitirAnularDoCemiterio' }] } });
  const anula = daMao(e, 0, 'anula');
  e.jogadores[0].mao.splice(e.jogadores[0].mao.indexOf(anula), 1);
  e.jogadores[0].cemiterio.push(anula);
  const alvoCarta = daMao(e, 0, 'barato');
  e.jogadores[0].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 0, carta: alvoCarta });
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: anula, alvo: alvoCarta }), /mão/); // ainda não liberado
  executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId });
  executar(e, { tipo: 'jogar', jogador: 0, carta: anula, alvo: alvoCarta });
  resolver(e);
  assert.ok(e.jogadores[0].banimento.includes(anula));
  assert.ok(!e.jogadores[0].cemiterio.includes(anula));
  assert.ok(e.jogadores[0].cemiterio.includes(alvoCarta)); // a carta alvo foi anulada
  assert.equal(e.habUsos[0].anularDoCemiterio, false);
});

test('habilidade de Conjurador sem Repentina: só na Principal com pilha vazia', () => {
  const e = criar({ ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'descontoProximaCarta', valor: 1 }] } });
  e.etapa = 'cataclisma';
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: e.jogadores[0].conjuradorId }), /Principal|principal/);
});
