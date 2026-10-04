import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import { cartasColecao } from '../src/cartas-colecao.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Invocador', vida: 12, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0, habilidade: '' };
const colecao = (id: string) => cartasColecao.find((c) => c.id === id) as CartaDef;
const extras: CartaDef[] = [
  { ...base, id: 'rep1', nome: 'Rep 1', tipo: 'temporaria', custo: 0, velocidade: 'repentina', efeitos: [{ tipo: 'comprar', valor: 1 }] },
  { ...base, id: 'rep2', nome: 'Rep 2', tipo: 'temporaria', custo: 0, velocidade: 'repentina', efeitos: [{ tipo: 'arcano', valor: 1 }] },
  { ...base, id: 'bicho', nome: 'Bicho', tipo: 'aliado', custo: 0, ataque: 2, vida: 3, palavrasChave: ['Proficiente'] },
  { ...base, id: 'guarda', nome: 'Guarda', tipo: 'aliado', custo: 0, ataque: 1, vida: 3 },
  { ...base, id: 'enchimento', nome: 'Enchimento', tipo: 'aliado', custo: 1, ataque: 1, vida: 1 },
];
const cartas = [...extras, colecao('item-extra-ovo'), colecao('ficha-wyvern-filhote'), colecao('item-armadura-gibao') ?? { ...base, id: 'item-armadura-gibao', nome: 'Gibão', tipo: 'item', categoriaItem: 'armadura', defesa: 1, vidaDesgaste: 4 } as CartaDef].filter(Boolean);
const catalogo: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };

function novo(itens: string[] = []): Estado {
  const ids = extras.map((c) => c.id);
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, (_, i) => ids[i % ids.length]), itens };
  const e = criarJogo({ catalogo, baralhos: [deck, deck], semente: 5 });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  return e;
}
const passarTodos = (e: Estado) => { executar(e, { tipo: 'passar', jogador: e.prioridade }); executar(e, { tipo: 'passar', jogador: e.prioridade }); };
function principal(e: Estado): void {
  while (e.etapa !== 'principal') {
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); continue; }
    passarTodos(e);
  }
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
  z.mao.splice(z.mao.indexOf(id), 1); z.campo.push(id); e.instancias[id].entrouNoTurno = 0;
  return id;
}

test('pilha como no Magic: quem joga recebe a prioridade de volta e empilha mais antes de passar', () => {
  const e = novo();
  principal(e);
  const a = daMao(e, 0, 'rep1'), b = daMao(e, 0, 'rep2');
  executar(e, { tipo: 'jogar', jogador: 0, carta: a });
  assert.equal(e.prioridade, 0, 'a prioridade volta para quem jogou');
  executar(e, { tipo: 'jogar', jogador: 0, carta: b });
  assert.equal(e.pilha.length, 2, 'empilhou uma segunda antes de passar');
  executar(e, { tipo: 'passar', jogador: 0 });
  assert.equal(e.prioridade, 1, 'agora o oponente pode responder');
  assert.equal(e.pilha.length, 2);
  executar(e, { tipo: 'passar', jogador: 1 });
  assert.equal(e.pilha.length, 1, 'só o topo resolveu');
  assert.equal(e.prioridade, 0, 'depois de resolver, a prioridade vai para o jogador ativo');
  assert.equal(e.pilha[0].tipo === 'carta' && e.pilha[0].instId, a);
});

test('pilha: se o oponente responde, o passe de quem jogou não resolve; só dois passes seguidos resolvem', () => {
  const e = novo();
  principal(e);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'rep1') });
  executar(e, { tipo: 'passar', jogador: 0 });
  executar(e, { tipo: 'jogar', jogador: 1, carta: daMao(e, 1, 'rep2') });
  assert.equal(e.prioridade, 1, 'quem respondeu também recebe a prioridade de volta');
  executar(e, { tipo: 'passar', jogador: 1 });
  assert.equal(e.pilha.length, 2, 'um passe depois de acrescentar não resolve');
  executar(e, { tipo: 'passar', jogador: 0 });
  assert.equal(e.pilha.length, 1);
});

test('item com Grau (Ovo de Wyvern, Grau 3): não pode ser usado sem o Grau; com o Grau, vale', () => {
  const e = novo(['item-extra-ovo']);
  principal(e);
  const ovo = e.jogadores[0].itens[0];
  assert.ok(ovo, 'o Ovo está equipado');
  e.jogadores[0].arcano = 5;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: ovo }), /Grau 3/);
  assert.equal(e.jogadores[0].itens.includes(ovo), true, 'o item não foi gasto');
  for (let i = 0; i < 3; i++) { const g = daMao(e, 0, 'enchimento'); e.jogadores[0].mao.splice(e.jogadores[0].mao.indexOf(g), 1); e.jogadores[0].grau.push(g); }
  executar(e, { tipo: 'ativar', jogador: 0, carta: ovo });
  passarTodos(e);
  assert.ok(e.jogadores[0].campo.some((id) => e.instancias[id].defId === 'ficha-wyvern-filhote'));
});

function ataqueAoConjurador(e: Estado) {
  const atacante = emCampo(e, 0, 'bicho');
  executar(e, { tipo: 'atacar', jogador: 0, atacante, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e);
  assert.equal(e.pendencia?.tipo, 'bloqueio');
  return atacante;
}

test('Conjurador bloqueia sozinho um ataque ao próprio Conjurador', () => {
  const e = novo();
  principal(e);
  ataqueAoConjurador(e);
  const c1 = e.jogadores[1].conjuradorId;
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: c1 });
  assert.equal(e.combate?.bloqueador, c1);
  assert.equal(e.instancias[c1].suspensa, true, 'bloquear suspende o Conjurador');
});

test('Conjurador bloqueia junto com um Aliado e escolhe não usar a Armadura', () => {
  const e = novo();
  principal(e);
  const guarda = emCampo(e, 1, 'guarda');
  ataqueAoConjurador(e);
  const c1 = e.jogadores[1].conjuradorId;
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueadores: [c1, guarda], usarArmadura: false });
  assert.deepEqual(e.combate?.bloqueadores, [c1, guarda]);
  assert.equal(e.combate?.usarArmadura, false);
});

test('Conjurador suspenso não bloqueia (como qualquer bloqueador); depois de levantar, bloqueia', () => {
  const e = novo();
  principal(e);
  const c1 = e.jogadores[1].conjuradorId;
  e.instancias[c1].suspensa = true;
  ataqueAoConjurador(e);
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: c1 }), /suspenso/);
  e.instancias[c1].suspensa = false;
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: c1 });
  assert.equal(e.combate?.bloqueador, c1);
});
