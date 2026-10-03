import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar, ErroRegra, validarBaralho } from '../src/motor.ts';
import { baralhoTeste, catalogoTeste } from '../src/cartas-teste.ts';
import type { Estado, Jogador, Regras } from '../src/tipos.ts';

const outro = (j: Jogador): Jogador => (j === 0 ? 1 : 0);

function novo(semente = 1, regras: Partial<Regras> = { primeiroSemGrauNoTurno1: false }): Estado {
  const e = criarJogo({
    catalogo: catalogoTeste,
    baralhos: [baralhoTeste('maga-da-cinza'), baralhoTeste('guardiao-do-vale')],
    semente,
    regras,
  });
  const quem = e.pendencia!.tipo === 'escolherInicio' ? e.pendencia!.jogador : 0;
  executar(e, { tipo: 'escolherInicio', jogador: quem, primeiro: 0 });
  return e;
}

/** Os dois passam: avança a etapa (ou resolve a pilha). */
function passarTodos(e: Estado): void {
  while (e.pendencia?.tipo === 'descartar') {
    const { jogador, quantidade } = e.pendencia;
    executar(e, { tipo: 'descartar', jogador, cartas: e.jogadores[jogador].mao.slice(0, quantidade) });
  }
  executar(e, { tipo: 'passar', jogador: e.prioridade });
  executar(e, { tipo: 'passar', jogador: e.prioridade });
}

function irParaPrincipal(e: Estado): void {
  while (e.etapa !== 'principal') passarTodos(e);
}

/** Põe na mão do jogador uma carta específica (do baralho) para o teste ser determinístico. */
function daMao(e: Estado, j: Jogador, defId: string): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId)!;
  const ib = z.baralho.indexOf(id);
  if (ib >= 0) {
    z.baralho.splice(ib, 1);
    z.mao.push(id);
  }
  return id;
}

function dar(e: Estado, j: Jogador, n: number): void {
  e.jogadores[j].arcano += n;
}

test('preparação: dados e mão inicial; o segundo jogador não recebe carta nem Arcano extra', () => {
  const e = novo();
  const segundo = e.primeiro === 0 ? 1 : 0;
  assert.equal(e.jogadores[e.primeiro].mao.length, 5);
  assert.equal(e.jogadores[segundo].mao.length, 5);
  assert.equal(e.jogadores[segundo].arcano, 0);
  assert.equal(e.etapa, 'inicio');
  assert.notEqual(e.dados[0], e.dados[1]);
});

test('determinismo: mesma semente, mesmo estado inicial', () => {
  const a = novo(7), b = novo(7);
  assert.deepEqual(a.jogadores[0].mao, b.jogadores[0].mao);
  assert.deepEqual(a.dados, b.dados);
});

test('só o vencedor do dado escolhe quem começa', () => {
  const e = criarJogo({
    catalogo: catalogoTeste,
    baralhos: [baralhoTeste('maga-da-cinza'), baralhoTeste('guardiao-do-vale')],
    semente: 3,
  });
  const quem = e.pendencia!.jogador;
  assert.throws(() => executar(e, { tipo: 'escolherInicio', jogador: outro(quem), primeiro: 0 }), ErroRegra);
});

test('Arcano: enviar carta só serve para pagar custo; a sobra fica até o Cataclisma e a carta vai ao fundo do baralho', () => {
  const e = novo();
  irParaPrincipal(e);
  const aprendiz = daMao(e, 0, 'aprendiz-da-torre'); // custo 1
  const dragao = daMao(e, 0, 'dragao-das-cinzas'); // gera 2
  executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz, enviarArcano: [dragao] });
  assert.equal(e.jogadores[0].arcano, 1); // gerou 2, gastou 1, sobrou 1
  assert.deepEqual(e.jogadores[0].zonaArcano, [dragao]);
  passarTodos(e); // resolve o Aliado
  passarTodos(e); // principal -> cataclisma
  assert.equal(e.etapa, 'cataclisma');
  passarTodos(e); // cataclisma -> final
  assert.equal(e.jogadores[0].arcano, 0);
  assert.equal(e.jogadores[0].zonaArcano.length, 0);
  assert.equal(e.jogadores[0].baralho.at(-1), dragao);
});

test('Arcano: não se pode enviar cartas sem necessidade nem além do custo', () => {
  const e = novo();
  irParaPrincipal(e);
  const aprendiz = daMao(e, 0, 'aprendiz-da-torre'); // custo 1
  const x = daMao(e, 0, 'batedora-ligeira');
  const y = daMao(e, 0, 'moinho-antigo');
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz, enviarArcano: [x, y] }), /custo já está coberto/);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz, enviarArcano: [aprendiz] }), /inválida/);
  assert.equal(e.jogadores[0].mao.includes(x), true); // nada foi movido
  dar(e, 0, 1);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz, enviarArcano: [x] }), /custo já está coberto/);
});

test('Arcano gerado no turno do oponente sobrevive até o seu próprio Cataclisma', () => {
  const e = novo();
  irParaPrincipal(e);
  const bola = daMao(e, 1, 'bola-de-fogo'); // custo 2, repentina
  const dragao = daMao(e, 1, 'dragao-das-cinzas'); // gera 2
  e.jogadores[1].arcano = 1;
  executar(e, { tipo: 'passar', jogador: 0 });
  executar(e, { tipo: 'jogar', jogador: 1, carta: bola, alvo: e.jogadores[0].conjuradorId, enviarArcano: [dragao] });
  assert.equal(e.jogadores[1].arcano, 1); // 1 + 2 - 2
  while (e.ativo === 0) passarTodos(e); // fim do turno do jogador 0
  assert.equal(e.jogadores[1].arcano, 1);
});

test('jogar Aliado: paga custo, vai à pilha e resolve ao passarem', () => {
  const e = novo();
  irParaPrincipal(e);
  const aprendiz = daMao(e, 0, 'aprendiz-da-torre');
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz }), /Arcano insuficiente/);
  dar(e, 0, 1);
  executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz });
  assert.equal(e.pilha.length, 1);
  assert.equal(e.jogadores[0].arcano, 0);
  passarTodos(e);
  assert.equal(e.pilha.length, 0);
  assert.ok(e.jogadores[0].campo.includes(aprendiz));
});

test('Grau: exige cartas na Zona de Grau, 1 por turno sem suspender o Conjurador; suspender gera Arcano sem pilha', () => {
  const e = novo();
  irParaPrincipal(e);
  const guarda = daMao(e, 0, 'guarda-de-ferro');
  dar(e, 0, 2);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: guarda }), /Grau/);
  const g = e.jogadores[0].mao.find((i) => i !== guarda)!;
  executar(e, { tipo: 'colocarGrau', jogador: 0, carta: g });
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].suspensa, false);
  assert.throws(() => executar(e, { tipo: 'colocarGrau', jogador: 0, carta: guarda }), /já pôs Grau/);
  executar(e, { tipo: 'suspenderGrau', jogador: 0, carta: g });
  assert.equal(e.jogadores[0].arcano, 3);
  assert.equal(e.pilha.length, 0);
  executar(e, { tipo: 'jogar', jogador: 0, carta: guarda });
  passarTodos(e);
  assert.ok(e.jogadores[0].campo.includes(guarda));
});

test('pilha LIFO: carta repentina em resposta resolve antes; findante não entra em resposta', () => {
  const e = novo();
  irParaPrincipal(e);
  const aprendiz = daMao(e, 0, 'aprendiz-da-torre');
  const bola = daMao(e, 1, 'bola-de-fogo');
  const raio = daMao(e, 0, 'raio-do-conjurador');
  dar(e, 0, 5);
  dar(e, 1, 2);
  executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz });
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: raio }), /findante/);
  executar(e, { tipo: 'passar', jogador: 0 });
  executar(e, { tipo: 'jogar', jogador: 1, carta: bola, alvo: e.jogadores[0].conjuradorId });
  assert.equal(e.pilha.length, 2);
  passarTodos(e); // resolve a bola de fogo (topo)
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].dano, 3);
  assert.equal(e.jogadores[0].campo.includes(aprendiz), false);
  passarTodos(e); // resolve o aprendiz
  assert.ok(e.jogadores[0].campo.includes(aprendiz));
});

test('memória de dano: dano persiste entre turnos e mata ao acumular', () => {
  const e = novo();
  irParaPrincipal(e);
  const guarda = daMao(e, 0, 'guarda-de-ferro');
  e.jogadores[0].grau.push(daMao(e, 0, 'aprendiz-da-torre'));
  dar(e, 0, 2);
  executar(e, { tipo: 'jogar', jogador: 0, carta: guarda });
  passarTodos(e);
  e.instancias[guarda].dano = 2; // simula dano de turno anterior
  while (e.ativo === 0) passarTodos(e);
  assert.equal(e.instancias[guarda].dano, 2); // não curou no fim do turno
  const bola = daMao(e, 1, 'bola-de-fogo');
  dar(e, 1, 2);
  irParaPrincipal(e);
  executar(e, { tipo: 'jogar', jogador: 1, carta: bola, alvo: guarda });
  passarTodos(e);
  assert.ok(e.jogadores[0].cemiterio.includes(guarda));
});

test('combate: ataque abre pilha, defensor bloqueia, dano simultâneo', () => {
  const e = novo();
  irParaPrincipal(e);
  const aprendiz = daMao(e, 0, 'batedora-ligeira');
  dar(e, 0, 1);
  executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz });
  passarTodos(e);
  executar(e, { tipo: 'atacar', jogador: 0, atacante: aprendiz, alvo: e.jogadores[1].conjuradorId });
  assert.equal(e.instancias[aprendiz].suspensa, true);
  assert.equal(e.pilha.length, 1);
  passarTodos(e); // resolve o ataque
  assert.equal(e.pendencia?.tipo, 'bloqueio');
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: e.jogadores[1].conjuradorId });
  assert.equal(e.pendencia, null);
  passarTodos(e); // dano
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 1);
  assert.ok(e.jogadores[0].cemiterio.includes(aprendiz)); // a Batedora (vida 1) morre com o dano básico 1 do Conjurador
  assert.equal(e.combate, null);
});

test('ataque sem bloqueio no Conjurador: só o atacante bate', () => {
  const e = novo();
  irParaPrincipal(e);
  const guarda = daMao(e, 0, 'guarda-de-ferro');
  e.jogadores[0].grau.push(daMao(e, 0, 'aprendiz-da-torre'));
  dar(e, 0, 2);
  executar(e, { tipo: 'jogar', jogador: 0, carta: guarda });
  passarTodos(e);
  e.instancias[guarda].entrouNoTurno = 0; // já estava em campo de um turno anterior
  executar(e, { tipo: 'atacar', jogador: 0, atacante: guarda, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: null });
  passarTodos(e);
  assert.equal(e.instancias[e.jogadores[1].conjuradorId].dano, 2);
  assert.equal(e.instancias[guarda].dano, 0);
});

test('vitória: dano letal ao Conjurador encerra a partida', () => {
  const e = novo();
  irParaPrincipal(e);
  const raio = daMao(e, 0, 'raio-do-conjurador');
  dar(e, 0, 3);
  e.instancias[e.jogadores[1].conjuradorId].dano = 9;
  executar(e, { tipo: 'jogar', jogador: 0, carta: raio });
  passarTodos(e);
  assert.equal(e.vencedor, 0);
  assert.throws(() => executar(e, { tipo: 'passar', jogador: 0 }), /acabou/);
});

test('baralho vazio não faz perder', () => {
  const e = novo();
  e.jogadores[0].baralho = [];
  while (e.ativo === 0) passarTodos(e);
  assert.equal(e.vencedor, null);
});

test('limite de mão 7: descarta o excesso no Início do turno de cada jogador, não no Final', () => {
  const e = novo();
  const z = e.jogadores[0];
  z.mao.push(...z.baralho.splice(0, 2)); // 7 na mão; o Final compra mais 2 e não descarta
  irParaPrincipal(e);
  while (e.etapa !== 'final') passarTodos(e);
  assert.equal(e.pendencia, null);
  assert.equal(z.mao.length, 9);
  passarTodos(e); // vira o turno do jogador 1
  assert.equal(e.ativo, 1);
  while (e.ativo === 1) passarTodos(e); // jogador 1 tem 6 cartas (limite 7): sem descarte
  assert.equal(e.ativo, 0);
  assert.equal(e.etapa, 'inicio');
  assert.deepEqual(e.pendencia, { tipo: 'descartar', jogador: 0, quantidade: 2 });
  assert.throws(() => executar(e, { tipo: 'passar', jogador: 0 }), /pendente/);
  executar(e, { tipo: 'descartar', jogador: 0, cartas: z.mao.slice(0, 2) });
  assert.equal(z.mao.length, 7);
  assert.equal(z.cemiterio.length, 2);
});

test('Aliado não ataca ao entrar, a menos que seja Proficiente', () => {
  const e = novo();
  irParaPrincipal(e);
  const aprendiz = daMao(e, 0, 'aprendiz-da-torre');
  const batedora = daMao(e, 0, 'batedora-ligeira');
  dar(e, 0, 2);
  executar(e, { tipo: 'jogar', jogador: 0, carta: aprendiz });
  passarTodos(e);
  executar(e, { tipo: 'jogar', jogador: 0, carta: batedora });
  passarTodos(e);
  const alvo = e.jogadores[1].conjuradorId;
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 0, atacante: aprendiz, alvo }), /Proficiente/);
  executar(e, { tipo: 'atacar', jogador: 0, atacante: batedora, alvo });
  assert.equal(e.combate?.atacante, batedora);
});

test('tudo levanta no Final: quem bloqueou fica suspenso no turno do oponente e levanta só no próprio Final', () => {
  const e = novo();
  irParaPrincipal(e);
  const guarda = daMao(e, 1, 'guarda-de-ferro');
  e.instancias[guarda].suspensa = false;
  e.jogadores[1].mao.splice(e.jogadores[1].mao.indexOf(guarda), 1);
  e.jogadores[1].campo.push(guarda);
  const bat = daMao(e, 0, 'batedora-ligeira');
  dar(e, 0, 1);
  executar(e, { tipo: 'jogar', jogador: 0, carta: bat });
  passarTodos(e);
  executar(e, { tipo: 'atacar', jogador: 0, atacante: bat, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: guarda });
  passarTodos(e);
  assert.equal(e.instancias[guarda].suspensa, true);
  while (e.ativo === 0) passarTodos(e);
  irParaPrincipal(e);
  assert.equal(e.instancias[guarda].suspensa, true); // quem bloqueou não ataca no turno seguinte
  assert.throws(() => executar(e, { tipo: 'atacar', jogador: 1, atacante: guarda, alvo: e.jogadores[0].conjuradorId }), /suspenso/);
  while (e.ativo === 1) passarTodos(e);
  assert.equal(e.instancias[guarda].suspensa, false);
});

test('validarBaralho acusa tamanho errado e carta desconhecida', () => {
  const erros = validarBaralho(catalogoTeste, { conjurador: 'maga-da-cinza', cartas: ['x'] });
  assert.equal(erros.length, 2);
});

test('padrão: quem começa não põe Grau no turno 1', () => {
  const e = novo(1, {});
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.turno, 1);
  assert.throws(() => executar(e, { tipo: 'colocarGrau', jogador: e.ativo, carta: e.jogadores[e.ativo].mao[0] }), /primeiro turno/);
});
