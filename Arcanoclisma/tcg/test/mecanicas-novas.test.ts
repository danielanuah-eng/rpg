import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import { baralhoTeste } from '../src/cartas-teste.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador, Regras } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 12, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '' };
const cartas: CartaDef[] = [
  { ...base, id: 'moinho', nome: 'Moinho', tipo: 'estrutura', custo: 1, vida: 2, ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] } },
  { ...base, id: 'biblioteca', nome: 'Biblioteca', tipo: 'estrutura', custo: 1, vida: 2, ativacao: { custo: 1, suspende: true, efeitos: [{ tipo: 'comprar', valor: 1 }] } },
  { ...base, id: 'enfermaria', nome: 'Enfermaria', tipo: 'estrutura', custo: 1, vida: 2, ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }] } },
  { ...base, id: 'fogo', nome: 'Fogo', tipo: 'temporaria', custo: 1, pagarParaAnular: 1, efeitos: [{ tipo: 'dano', valor: 4, alvo: 'conjuradorInimigo' }] },
  { ...base, id: 'fonte', nome: 'Fonte', tipo: 'estrutura', custo: 1, vida: 2, grauExtra: 2 },
  { ...base, id: 'grandao', nome: 'Grandão', tipo: 'aliado', custo: 0, grau: 3, ataque: 3, vida: 3 },
  { ...base, id: 'queima-aliado', nome: 'Queima', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'dano', valor: 2, alvo: 'escolhidoAliado' }] },
  { ...base, id: 'devolve', nome: 'Devolve', tipo: 'temporaria', custo: 0, velocidade: 'repentina', efeitos: [{ tipo: 'devolver', alvo: 'escolhidoAliado' }] },
  { ...base, id: 'varre', nome: 'Varre', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'danoTodosAliados', valor: 1 }] },
  { ...base, id: 'cresce', nome: 'Cresce', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'dano', valor: 0, alvo: 'escolhidoAliado', escalaGrau: true }] },
  { ...base, id: 'anula', nome: 'Anula', tipo: 'temporaria', custo: 0, velocidade: 'repentina', efeitos: [{ tipo: 'anular', alvo: 'pilha' }] },
  { ...base, id: 'fantasma', nome: 'Fantasma', tipo: 'aliado', custo: 0, ataque: 1, vida: 1, palavrasChave: ['Etéreo', 'Proficiente'] },
  { ...base, id: 'enchimento', nome: 'Enchimento', tipo: 'aliado', custo: 1, ataque: 1, vida: 1 },
  { ...base, id: 'servo', nome: 'Servo', tipo: 'aliado', custo: 1, ataque: 0, vida: 1, ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] } },
  { ...base, id: 'servo-proficiente', nome: 'Servo ágil', tipo: 'aliado', custo: 1, ataque: 0, vida: 1, palavrasChave: ['Proficiente'], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] } },
];
const catalogo: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
void baralhoTeste;

function novo(regras: Partial<Regras> = {}, tamanho = 30): Estado {
  const ids = cartas.map((c) => c.id);
  const deck = { conjurador: 'c', cartas: Array.from({ length: tamanho }, (_, i) => ids[i % ids.length]) };
  const e = criarJogo({ catalogo, baralhos: [deck, deck], semente: 5, regras });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  return e;
}
function passarTodos(e: Estado): void {
  executar(e, { tipo: 'passar', jogador: e.prioridade });
  executar(e, { tipo: 'passar', jogador: e.prioridade });
}
function principal(e: Estado): void {
  while (e.etapa !== 'principal') passarTodos(e);
}
function daMao(e: Estado, j: Jogador, defId: string): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId)!;
  const ib = z.baralho.indexOf(id);
  if (ib >= 0) { z.baralho.splice(ib, 1); z.mao.push(id); }
  return id;
}
function emCampo(e: Estado, j: Jogador, defId: string): string {
  const id = daMao(e, j, defId);
  const z = e.jogadores[j];
  z.mao.splice(z.mao.indexOf(id), 1);
  z.campo.push(id);
  return id;
}

test('ativar: Estrutura que só gera Arcano não usa a pilha e suspende', () => {
  const e = novo();
  principal(e);
  const m = emCampo(e, 0, 'moinho');
  executar(e, { tipo: 'ativar', jogador: 0, carta: m });
  assert.equal(e.jogadores[0].arcano, 1);
  assert.equal(e.pilha.length, 0);
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: m }), /suspensa/);
});

test('ativar: efeito com pilha (comprar) paga custo e resolve ao passarem', () => {
  const e = novo();
  principal(e);
  const b = emCampo(e, 0, 'biblioteca');
  const mao = e.jogadores[0].mao.length;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: b }), /Arcano insuficiente/);
  e.jogadores[0].arcano = 1;
  executar(e, { tipo: 'ativar', jogador: 0, carta: b });
  assert.equal(e.pilha.length, 1);
  passarTodos(e);
  assert.equal(e.jogadores[0].mao.length, mao + 1);
});

test('ativar: cura exige alvo e reduz o dano guardado', () => {
  const e = novo();
  principal(e);
  const en = emCampo(e, 0, 'enfermaria');
  const c = e.jogadores[0].conjuradorId;
  e.instancias[c].dano = 5;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: en }), /alvo/);
  executar(e, { tipo: 'ativar', jogador: 0, carta: en, alvo: c });
  passarTodos(e);
  assert.equal(e.instancias[c].dano, 2);
});

test('burn com pagarParaAnular: o oponente paga e a carta é anulada', () => {
  const e = novo();
  principal(e);
  const fogo = daMao(e, 0, 'fogo');
  e.jogadores[0].arcano = 1;
  e.jogadores[1].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 0, carta: fogo, alvo: undefined });
  passarTodos(e);
  assert.equal(e.pendencia?.tipo, 'pagarAnulacao');
  executar(e, { tipo: 'pagarAnulacao', jogador: 1, pagar: true });
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 0);
  assert.equal(e.jogadores[1].arcano, 0);
  assert.ok(e.jogadores[0].cemiterio.includes(fogo));
  assert.equal(e.pendencia, null);
});

test('burn com pagarParaAnular: se o oponente recusa, resolve; sem recursos, resolve direto', () => {
  const e = novo();
  principal(e);
  const fogo = daMao(e, 0, 'fogo');
  e.jogadores[0].arcano = 1;
  e.jogadores[1].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 0, carta: fogo });
  passarTodos(e);
  executar(e, { tipo: 'pagarAnulacao', jogador: 1, pagar: false });
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 4);

  const f = novo();
  principal(f);
  const fogo2 = daMao(f, 0, 'fogo');
  f.jogadores[0].arcano = 1;
  f.jogadores[1].arcano = 0;
  f.jogadores[1].mao = []; // sem Arcano, Grau nem cartas: não pode pagar
  f.jogadores[1].grau = [];
  executar(f, { tipo: 'jogar', jogador: 0, carta: fogo2 });
  passarTodos(f);
  assert.equal(f.pendencia, null);
  assert.equal(f.instancias[f.jogadores[1].conjuradorId].dano, 4);
});

test('burn: o oponente pode pagar com Grau levantado e carta enviada', () => {
  const e = novo();
  principal(e);
  const fogo = daMao(e, 0, 'fogo');
  e.jogadores[0].arcano = 1;
  e.jogadores[1].arcano = 0;
  const g = daMao(e, 1, 'enchimento');
  e.jogadores[1].mao.splice(e.jogadores[1].mao.indexOf(g), 1);
  e.jogadores[1].grau.push(g);
  executar(e, { tipo: 'jogar', jogador: 0, carta: fogo });
  passarTodos(e);
  executar(e, { tipo: 'pagarAnulacao', jogador: 1, pagar: true });
  assert.equal(e.instancias[g].suspensa, true);
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 0);
});

test('regra: quem começa não pode pôr Grau no turno 1, mas pode depois', () => {
  const e = novo({ primeiroSemGrauNoTurno1: true });
  principal(e);
  assert.throws(() => executar(e, { tipo: 'colocarGrau', jogador: 0, carta: e.jogadores[0].mao[0] }), /primeiro turno/);
  while (e.ativo === 0) passarTodos(e);
  while (e.ativo === 1) passarTodos(e);
  principal(e);
  executar(e, { tipo: 'colocarGrau', jogador: 0, carta: e.jogadores[0].mao[0] });
  assert.equal(e.jogadores[0].grau.length, 1);
});

test('regra grauSemSuspender: pôr Grau não suspende o Conjurador (1 por turno)', () => {
  const e = novo({ grauSemSuspender: true, primeiroSemGrauNoTurno1: false });
  principal(e);
  executar(e, { tipo: 'colocarGrau', jogador: 0, carta: e.jogadores[0].mao[0] });
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].suspensa, false);
  assert.throws(() => executar(e, { tipo: 'colocarGrau', jogador: 0, carta: e.jogadores[0].mao[0] }), /neste turno/);
});

test('regra semAcaoPerde: sem baralho, mão e ações, perde no início da Principal', () => {
  const e = novo({ semAcaoPerde: true });
  const z = e.jogadores[1];
  z.baralho = [];
  z.mao = [];
  z.arcano = 0;
  while (e.ativo === 0 && e.vencedor === null) passarTodos(e);
  if (e.vencedor === null) passarTodos(e); // Início do jogador 1 -> Principal
  assert.equal(e.vencedor, 0);
});

test('regra semAcaoPerde: ainda com mão ou Conjurador capaz de atacar, não perde', () => {
  const e = novo({ semAcaoPerde: true });
  const z = e.jogadores[1];
  z.baralho = [];
  z.mao = [];
  z.arcano = 1; // paga o ataque do Conjurador
  while (e.ativo === 0 && e.vencedor === null) passarTodos(e);
  if (e.vencedor === null) passarTodos(e); // Início do jogador 1 -> Principal
  assert.equal(e.vencedor, null);
});

test('grauExtra: permanente conta como Grau para requisito', () => {
  const e = novo();
  principal(e);
  const g = daMao(e, 0, 'grandao');
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: g }), /Precisa de Grau 3/);
  e.jogadores[0].grau.push(daMao(e, 0, 'enchimento'));
  e.jogadores[0].mao.splice(e.jogadores[0].mao.indexOf(e.jogadores[0].grau[0]), 1);
  emCampo(e, 0, 'fonte'); // +2 -> 1 real + 2 extra = 3
  executar(e, { tipo: 'jogar', jogador: 0, carta: g });
  assert.equal(e.pilha.length, 1);
});

test('dano a aliado (escolhidoAliado): não pode mirar o Conjurador', () => {
  const e = novo();
  principal(e);
  const q = daMao(e, 0, 'queima-aliado');
  const alvo = emCampo(e, 1, 'enchimento');
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: q, alvo: e.jogadores[1].conjuradorId }), /alvo legal/);
  executar(e, { tipo: 'jogar', jogador: 0, carta: q, alvo });
  passarTodos(e);
  assert.ok(e.jogadores[1].cemiterio.includes(alvo)); // enchimento tem 1 de vida
});

test('relógio de impasse: perde quem passa N turnos seguidos sem agir', () => {
  const e = novo({ impasseTurnos: 2 });
  while (e.vencedor === null && e.turno < 20) passarTodos(e);
  assert.notEqual(e.vencedor, null);
  assert.ok(e.log.some((l) => l.includes('Impasse')));
  assert.ok(e.turno <= 6);
});

test('relógio de impasse: agir (jogar carta) zera a contagem', () => {
  const e = novo({ impasseTurnos: 2 });
  principal(e);
  const en = daMao(e, 0, 'enchimento');
  e.jogadores[0].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 0, carta: en });
  passarTodos(e);
  while (e.ativo === 0) passarTodos(e);
  assert.equal(e.semAcaoTurnos[0], 0);
});

test('regra: o Conjurador de quem começa não ataca no turno 1', () => {
  const e = novo({ primeiroConjSemAtacarTurno1: true });
  principal(e);
  e.jogadores[0].arcano = 1;
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 0, atacante: e.jogadores[0].conjuradorId, alvo: e.jogadores[1].conjuradorId }), /não ataca no primeiro turno/);
});

test('devolver: Aliado volta para a mão do dono sem dano', () => {
  const e = novo();
  principal(e);
  const alvo = emCampo(e, 1, 'enchimento');
  e.instancias[alvo].dano = 0;
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'devolve'), alvo });
  passarTodos(e);
  assert.ok(e.jogadores[1].mao.includes(alvo));
  assert.equal(e.jogadores[1].campo.includes(alvo), false);
});

test('varredura: dano em cada Aliado do oponente', () => {
  const e = novo();
  principal(e);
  const a1 = emCampo(e, 1, 'enchimento');
  const a2 = emCampo(e, 1, 'grandao');
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'varre') });
  passarTodos(e);
  assert.ok(e.jogadores[1].cemiterio.includes(a1));
  assert.equal(e.instancias[a2].dano, 1);
});

test('dano que cresce com o Grau soma as cartas do Grau de quem joga', () => {
  const e = novo();
  principal(e);
  const a2 = emCampo(e, 1, 'grandao');
  for (let i = 0; i < 2; i++) { const g = e.jogadores[0].mao[0]; e.jogadores[0].mao.splice(0, 1); e.jogadores[0].grau.push(g); }
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'cresce'), alvo: a2 });
  passarTodos(e);
  assert.equal(e.instancias[a2].dano, 2);
});

test('anular: carta na pilha vai para o cemitério sem resolver', () => {
  const e = novo();
  principal(e);
  const fogo = daMao(e, 0, 'fogo');
  e.jogadores[0].arcano = 1;
  executar(e, { tipo: 'jogar', jogador: 0, carta: fogo });
  executar(e, { tipo: 'passar', jogador: 0 });
  executar(e, { tipo: 'jogar', jogador: 1, carta: daMao(e, 1, 'anula'), alvo: fogo });
  passarTodos(e); // resolve Anula
  assert.equal(e.pilha.length, 0);
  assert.ok(e.jogadores[0].cemiterio.includes(fogo));
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 0);
});

test('Etéreo: só é bloqueado por Aliado Etéreo', () => {
  const e = novo();
  principal(e);
  const f = emCampo(e, 0, 'fantasma');
  const b = emCampo(e, 1, 'enchimento');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: f, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e);
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: b }), /Etéreo/);
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: e.jogadores[1].conjuradorId }), /Etéreo/);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: null });
});

test('Aliado que acabou de entrar não usa ativação que suspende, a menos que seja Proficiente', () => {
  const e = novo();
  principal(e);
  const s = emCampo(e, 0, 'servo');
  e.instancias[s].entrouNoTurno = e.turno;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: s }), /Proficiente/);
  const p = emCampo(e, 0, 'servo-proficiente');
  e.instancias[p].entrouNoTurno = e.turno;
  executar(e, { tipo: 'ativar', jogador: 0, carta: p });
  assert.equal(e.jogadores[0].arcano, 1);
  const m = emCampo(e, 0, 'moinho'); // Estrutura: não tem a restrição
  e.instancias[m].entrouNoTurno = e.turno;
  executar(e, { tipo: 'ativar', jogador: 0, carta: m });
  assert.equal(e.jogadores[0].arcano, 2);
  e.instancias[s].entrouNoTurno = e.turno - 1; // no turno seguinte já pode
  executar(e, { tipo: 'ativar', jogador: 0, carta: s });
  assert.equal(e.jogadores[0].arcano, 3);
});
