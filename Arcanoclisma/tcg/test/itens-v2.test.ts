// Itens do Conjurador, versão 2 (pedido do autor, 03/10/2026, noite): Canal arma, foco arcano ou transcritor;
// todo uso suspende o Conjurador e tem um custo (Arcano, descartar, tomar dano); Extras por competência; fichas.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { ataqueConjurador, criarJogo, executar, validarBaralho } from '../src/motor.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Regras } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const item = (id: string, categoriaItem: CartaDef['categoriaItem'], extra: Partial<CartaDef>): CartaDef => ({ ...base, id, nome: id, tipo: 'item', categoriaItem, ...extra });
const cartas: CartaDef[] = [
  { ...base, id: 'faisca', nome: 'Faísca', tipo: 'temporaria', velocidade: 'repentina', efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] },
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  { ...base, id: 'ficha', nome: 'Espírito', tipo: 'aliado', ataque: 2, vida: 2, ficha: true },
  item('foco', 'canal', { tipoCanal: 'foco', ativacao: { custo: 1, suspende: false, suspendeConjurador: true, efeitos: [{ tipo: 'potenciaMagia', valor: 2 }] } }),
  item('transcritor', 'canal', { tipoCanal: 'transcritor', ativacao: { custo: 0, suspende: false, suspendeConjurador: true, descartar: 1, danoNoConjurador: 1, efeitos: [{ tipo: 'comprar', valor: 2 }] } }),
  item('pergaminho', 'extra', { competencia: 'Invocador', ativacao: { custo: 0, suspende: false, suspendeConjurador: true, efeitos: [{ tipo: 'invocar', carta: 'ficha' }] } }),
  item('luvas', 'extra', { competencia: 'Brigão', ativacao: { custo: 0, suspende: false, danoNoConjurador: 2, efeitos: [{ tipo: 'ataqueConjuradorTurno', valor: 2 }] } }),
  item('arma', 'canal', { tipoCanal: 'arma', ataque: 1 }),
];
const regras: Partial<Regras> = { primeiroSemGrauNoTurno1: false, reporMaoNoInicio: true, conjuradorLevantaNoInicio: true, conjuradorSoAtacaComArma: true };
function criar(itens: string[], comprometimento: ConjuradorDef['comprometimento'] = 'Invocador'): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento, vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, (_, i) => (i % 2 ? 'faisca' : 'nada')), itens };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 7, regras });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
const passarAte = (e: Estado, ok: () => boolean) => { let n = 0; while (!ok() && n++ < 200) executar(e, { tipo: 'passar', jogador: e.pendencia ? e.pendencia.jogador : e.prioridade }); };
const inst = (e: Estado, defId: string) => e.jogadores[0].itens.find((id) => e.instancias[id].defId === defId)!;

test('Foco Arcano: paga, suspende o Conjurador e a próxima Temporária de dano tem +2 (só uma vez)', () => {
  const e = criar(['foco']);
  const z = e.jogadores[0];
  z.arcano = 1;
  executar(e, { tipo: 'ativar', jogador: 0, carta: inst(e, 'foco') });
  assert.equal(e.instancias[z.conjuradorId].suspensa, true);
  assert.equal(z.arcano, 0);
  assert.ok(z.itens.includes(inst(e, 'foco')), 'o Canal não se consome');
  const conjOp = e.jogadores[1].conjuradorId;
  const f1 = z.mao.find((id) => e.instancias[id].defId === 'faisca')!;
  executar(e, { tipo: 'jogar', jogador: 0, carta: f1 });
  passarAte(e, () => e.pilha.length === 0);
  assert.equal(e.instancias[conjOp].dano, 3);
  const f2 = z.mao.find((id) => e.instancias[id].defId === 'faisca');
  if (f2) { executar(e, { tipo: 'jogar', jogador: 0, carta: f2 }); passarAte(e, () => e.pilha.length === 0); assert.equal(e.instancias[conjOp].dano, 4); }
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: inst(e, 'foco') }), /suspens/);
});

test('Transcritor: exige descartar 1 e o Conjurador toma 1 de dano; compra 2', () => {
  const e = criar(['transcritor']);
  const z = e.jogadores[0];
  const t = inst(e, 'transcritor');
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: t }), /Descarte 1/);
  const mao = z.mao.length;
  const desc = z.mao[0];
  executar(e, { tipo: 'ativar', jogador: 0, carta: t, descartarCusto: [desc] });
  passarAte(e, () => e.pilha.length === 0);
  assert.ok(z.cemiterio.includes(desc));
  assert.equal(e.instancias[z.conjuradorId].dano, 1);
  assert.equal(z.mao.length, mao - 1 + 2);
});

test('Extra por competência: só para o Comprometimento certo; invocar põe a ficha em campo; luvas dão Ataque no turno', () => {
  const cat = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' } as ConjuradorDef } };
  const lista = Array.from({ length: 60 }, () => 'nada');
  assert.equal(validarBaralho(cat, { conjurador: 'c', cartas: lista, itens: ['pergaminho'] }).length, 1);
  assert.deepEqual(validarBaralho(cat, { conjurador: 'c', cartas: lista, itens: ['luvas'] }), []);
  const e = criar(['pergaminho']);
  executar(e, { tipo: 'ativar', jogador: 0, carta: inst(e, 'pergaminho') });
  passarAte(e, () => e.pilha.length === 0);
  const z = e.jogadores[0];
  assert.equal(z.campo.filter((id) => e.instancias[id].defId === 'ficha').length, 1);
  assert.ok(!z.itens.length, 'o Extra se consome');
  const b = criar(['arma', 'luvas'], 'Brigão');
  assert.equal(ataqueConjurador(b, 0), 1);
  executar(b, { tipo: 'ativar', jogador: 0, carta: inst(b, 'luvas') });
  assert.equal(ataqueConjurador(b, 0), 3);
  assert.equal(b.instancias[b.jogadores[0].conjuradorId].suspensa, false, 'as luvas não suspendem: o Conjurador ainda ataca');
});
