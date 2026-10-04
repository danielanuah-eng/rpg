import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar, etereo, origemDaCarta, ataqueAtualDe, vidaAtualDe, temPalavra } from '../src/motor.ts';
import { conjuradoresPorValor as conjuradoresLista } from '../src/conjuradores.ts';
import type { CartaDef, Catalogo, Concessao, ConjuradorDef, Efeito, Estado, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 12, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '' };

function catalogoCom(extra: CartaDef[]): Catalogo {
  const cartas: CartaDef[] = [
    { ...base, id: 'ficha', nome: 'Ficha', tipo: 'aliado', custo: 0, ataque: 2, vida: 2, palavrasChave: ['Proficiente'], ficha: true } as CartaDef,
    { ...base, id: 'enchimento', nome: 'Enchimento', tipo: 'aliado', custo: 1, ataque: 1, vida: 1 },
    { ...base, id: 'rean', nome: 'Revenante', tipo: 'aliado', custo: 2, ataque: 1, vida: 1, palavrasChave: ['Reanimum'] },
    { ...base, id: 'mata', nome: 'Mata', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'proprio', tipos: ['aliado'] }] },
    { ...base, id: 'rtemp', nome: 'Rtemp', tipo: 'temporaria', custo: 0, palavrasChave: ['Reanimum'], efeitos: [{ tipo: 'comprar', valor: 1 }] },
    ...extra,
  ];
  return { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
}
const invoca = (id: string, concessao?: Concessao): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'invocar', carta: 'ficha', concessao }] });

function novo(extra: CartaDef[]): Estado {
  const cat = catalogoCom(extra);
  const ids = Object.keys(cat.cartas);
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, (_, i) => ids[i % ids.length]) };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 5 });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  return e;
}
function passarTodos(e: Estado): void {
  executar(e, { tipo: 'passar', jogador: e.prioridade });
  executar(e, { tipo: 'passar', jogador: e.prioridade });
}
/** Avança passando até a etapa principal, descartando o que sobrar da mão. */
function principal(e: Estado): void {
  while (e.etapa !== 'principal') {
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); continue; }
    passarTodos(e);
  }
}
function proximoTurno(e: Estado): void {
  const t = e.turno;
  do { passarTodos(e); principal(e); } while (e.turno === t);
}
function daMao(e: Estado, j: Jogador, defId: string): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId)!;
  const ib = z.baralho.indexOf(id);
  if (ib >= 0) { z.baralho.splice(ib, 1); z.mao.push(id); }
  return id;
}
function jogaInvocar(e: Estado, j: Jogador, carta: string): string {
  const antes = new Set(e.jogadores[j].campo);
  executar(e, { tipo: 'jogar', jogador: j, carta: daMao(e, j, carta) });
  passarTodos(e);
  const novoId = e.jogadores[j].campo.find((id) => !antes.has(id));
  return novoId ?? '';
}

test('invocar com concessão: a ficha entra Etérea e o motor trata como Etérea', () => {
  const e = novo([invoca('chama', { palavras: ['Etéreo'] })]);
  principal(e);
  const f = jogaInvocar(e, 0, 'chama');
  assert.equal(etereo(e, f), true);
  assert.equal(temPalavra(e, f, 'Etéreo'), true);
  // sem concessão, a mesma ficha não é Etérea
  const e2 = novo([invoca('chama')]);
  principal(e2);
  assert.equal(etereo(e2, jogaInvocar(e2, 0, 'chama')), false);
});

test('Etéreo concedido: só Etéreos bloqueiam a ficha e ela não é atingida por não Etéreos', () => {
  const e = novo([invoca('chama', { palavras: ['Etéreo'] })]);
  principal(e);
  const f = jogaInvocar(e, 0, 'chama');
  e.instancias[f].entrouNoTurno = 0;
  const bloqueador = e.jogadores[1].campo[0] ?? (() => { const id = daMao(e, 1, 'enchimento'); e.jogadores[1].mao.splice(e.jogadores[1].mao.indexOf(id), 1); e.jogadores[1].campo.push(id); return id; })();
  e.jogadores[0].arcano = 5;
  executar(e, { tipo: 'atacar', jogador: 0, atacante: f, alvo: e.jogadores[1].conjuradorId });
  passarTodos(e);
  assert.equal(e.pendencia?.tipo, 'bloqueio');
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueador }), /Etére/);
});

test('±Ataque e ±Vida: modificador permanente; Vida a 0 morre pelas regras normais', () => {
  const e = novo([invoca('forte', { ataque: 2, vida: 1 }), invoca('fraca', { ataque: -1, vida: -2 })]);
  principal(e);
  const f = jogaInvocar(e, 0, 'forte');
  assert.equal(ataqueAtualDe(e, f), 4);
  assert.equal(vidaAtualDe(e, f), 3);
  proximoTurno(e); proximoTurno(e);
  assert.equal(ataqueAtualDe(e, f), 4, 'continua depois de turnos');
  const antes = e.jogadores[0].cemiterio.length;
  assert.equal(jogaInvocar(e, 0, 'fraca'), '', 'Vida 2 - 2 = 0: não ficou em campo');
  assert.equal(e.jogadores[0].cemiterio.length, antes + 2, 'a carta de efeito e a ficha foram para o cemitério');
});

for (const destino of ['cemiterio', 'banimento', 'baralho', 'mao'] as const) {
  test(`turnos: conta o da entrada, sai no Final do 2º turno do controlador e vai para ${destino}`, () => {
    const e = novo([invoca('tempo', { turnos: 2, destino })]);
    principal(e);
    const t0 = e.turno;
    const f = jogaInvocar(e, 0, 'tempo');
    assert.equal(e.instancias[f].turnosRestantes, 2);
    proximoTurno(e); // turno do oponente: não conta
    assert.equal(e.turno, t0 + 1);
    assert.equal(e.instancias[f].turnosRestantes, 1, 'o turno em que entrou contou');
    assert.ok(e.jogadores[0].campo.includes(f));
    proximoTurno(e); // começa o 2º turno do dono
    assert.ok(e.jogadores[0].campo.includes(f));
    proximoTurno(e); // o 2º turno do dono acaba: sai no Final dele
    assert.equal(e.jogadores[0].campo.includes(f), false);
    const z = e.jogadores[0];
    const onde = { cemiterio: z.cemiterio, banimento: z.banimento, baralho: z.baralho, mao: z.mao }[destino];
    assert.ok(onde.includes(f), `foi para ${destino}`);
    assert.equal(e.instancias[f].turnosRestantes, undefined);
  });
}

test('turnos: sem valor a concessão é permanente (a carta não sai)', () => {
  const e = novo([invoca('perm', { palavras: ['Etéreo'] })]);
  principal(e);
  const f = jogaInvocar(e, 0, 'perm');
  for (let i = 0; i < 4; i++) proximoTurno(e);
  assert.ok(e.jogadores[0].campo.includes(f));
  assert.equal(e.instancias[f].turnosRestantes, undefined);
});

test('efeito campo "conceder" com alvo escolhido: dá palavra e ataque; Protegido N concedido cobra Arcano', () => {
  const ef: Efeito = { tipo: 'campo', acao: 'conceder', alcance: 'alvo', lado: 'proprio', tipos: ['aliado'], concessao: { palavras: ['Etéreo', 'Protegido 2'], ataque: 1 } };
  const dar: CartaDef = { ...base, id: 'dar', nome: 'Dar', tipo: 'temporaria', custo: 0, efeitos: [ef] };
  const e = novo([dar]);
  principal(e);
  const a = daMao(e, 0, 'enchimento');
  e.jogadores[0].mao.splice(e.jogadores[0].mao.indexOf(a), 1);
  e.jogadores[0].campo.push(a);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'dar'), alvo: a });
  passarTodos(e);
  assert.equal(etereo(e, a), true);
  assert.equal(ataqueAtualDe(e, a), 2);
  assert.equal(temPalavra(e, a, 'Etéreo'), true);
});

test('Reanimum: a carta é jogada do cemitério pagando o custo; ao sair do campo vai para o banimento (não repetível)', () => {
  const e = novo([]);
  principal(e);
  const r = daMao(e, 0, 'rean');
  const z = e.jogadores[0];
  z.mao.splice(z.mao.indexOf(r), 1);
  z.cemiterio.push(r);
  assert.equal(origemDaCarta(e, 0, r), 'cemiterio');
  z.arcano = 1;
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: r }), /Arcano insuficiente/);
  z.arcano = 2;
  executar(e, { tipo: 'jogar', jogador: 0, carta: r });
  assert.equal(z.arcano, 0, 'pagou o custo normal');
  passarTodos(e);
  assert.ok(z.campo.includes(r));
  assert.equal(z.banimento.includes(r), false, 'em campo, não no banimento');
  assert.equal(e.instancias[r].banirAoSair, true);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'mata'), alvo: r });
  passarTodos(e);
  assert.equal(z.campo.includes(r), false);
  assert.ok(z.banimento.includes(r), 'saiu do campo: banimento');
  assert.equal(z.cemiterio.includes(r), false);
  assert.equal(origemDaCarta(e, 0, r), null, 'não dá para jogar de novo');
});

test('Reanimum em Temporária: ao terminar de resolver vai para o banimento', () => {
  const e = novo([]);
  principal(e);
  const r = daMao(e, 0, 'rtemp');
  const z = e.jogadores[0];
  z.mao.splice(z.mao.indexOf(r), 1);
  z.cemiterio.push(r);
  executar(e, { tipo: 'jogar', jogador: 0, carta: r });
  passarTodos(e);
  assert.ok(z.banimento.includes(r));
  assert.equal(z.cemiterio.includes(r), false);
});

test('turnos: concessão dada no turno do oponente conta esse turno como o 1º', () => {
  const e = novo([{ ...base, id: 'rep-tempo', nome: 'RepTempo', tipo: 'temporaria', custo: 0, velocidade: 'repentina', efeitos: [{ tipo: 'invocar', carta: 'ficha', concessao: { turnos: 2, destino: 'banimento' } }] }]);
  principal(e);
  proximoTurno(e); // agora é o turno do jogador 1
  assert.equal(e.ativo, 1);
  executar(e, { tipo: 'passar', jogador: 1 });
  const antes = new Set(e.jogadores[0].campo);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'rep-tempo') });
  passarTodos(e);
  const f = e.jogadores[0].campo.find((id) => !antes.has(id))!;
  assert.ok(f);
  proximoTurno(e); // acabou o turno do oponente: contou como o 1º
  assert.equal(e.instancias[f].turnosRestantes, 1);
  assert.ok(e.jogadores[0].campo.includes(f));
  proximoTurno(e); // o turno do dono: 2º, sai
  assert.ok(e.jogadores[0].banimento.includes(f));
});

test('Diástaros: o Aliado que o reanimar dele traz entra com Etéreo; os outros dele não', () => {
  const e = criarJogo({ catalogo: { cartas: catalogoCom([]).cartas, conjuradores: { diastaros: conjuradoresDef('diastaros') } }, baralhos: [{ conjurador: 'diastaros', cartas: Array.from({ length: 30 }, (_, i) => ['enchimento', 'rean', 'ficha'][i % 3]) }, { conjurador: 'diastaros', cartas: Array.from({ length: 30 }, () => 'enchimento') }], semente: 3 });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  principal(e);
  const z = e.jogadores[0];
  const a = daMao(e, 0, 'enchimento'); z.mao.splice(z.mao.indexOf(a), 1); z.cemiterio.push(a);
  const b = daMao(e, 0, 'enchimento'); z.mao.splice(z.mao.indexOf(b), 1); z.campo.push(b);
  z.arcano = 5;
  executar(e, { tipo: 'ativar', jogador: 0, carta: z.conjuradorId, alvo: a });
  passarTodos(e);
  assert.ok(z.campo.includes(a));
  assert.equal(etereo(e, a), true);
  assert.equal(etereo(e, b), false);
});

test('bloqueio Etéreo: não Etéreo (inclusive o Conjurador sem Armadura Etérea) não bloqueia atacante Etéreo; Etéreo concedido bloqueia', () => {
  const e = novo([invoca('chama', { palavras: ['Etéreo'] })]);
  principal(e);
  const f = jogaInvocar(e, 0, 'chama');
  e.instancias[f].entrouNoTurno = 0;
  const c1 = e.jogadores[1].conjuradorId;
  const guarda = daMao(e, 1, 'enchimento'); e.jogadores[1].mao.splice(e.jogadores[1].mao.indexOf(guarda), 1); e.jogadores[1].campo.push(guarda);
  executar(e, { tipo: 'atacar', jogador: 0, atacante: f, alvo: c1 });
  passarTodos(e);
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: guarda }), /Etére/);
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: c1 }), /Etére/);
  e.instancias[guarda].palavrasExtra = ['Etéreo'];
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: guarda });
  assert.equal(e.combate?.bloqueador, guarda);
});

test('Reanimum: sem a palavra-chave a carta do cemitério não é jogável; do cemitério do oponente também não', () => {
  const e = novo([]);
  principal(e);
  const a = daMao(e, 0, 'enchimento');
  e.jogadores[0].mao.splice(e.jogadores[0].mao.indexOf(a), 1);
  e.jogadores[0].cemiterio.push(a);
  assert.equal(origemDaCarta(e, 0, a), null);
  const r = daMao(e, 0, 'rean');
  e.jogadores[0].mao.splice(e.jogadores[0].mao.indexOf(r), 1);
  e.jogadores[0].cemiterio.push(r);
  assert.equal(origemDaCarta(e, 1, r), null);
});

test('Reanimum respeita a fase: Aliado do cemitério só na fase principal com a pilha vazia', () => {
  const e = novo([]);
  principal(e);
  const r = daMao(e, 0, 'rean');
  const z = e.jogadores[0];
  z.mao.splice(z.mao.indexOf(r), 1);
  z.cemiterio.push(r);
  z.arcano = 5;
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'enchimento') });
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 0, carta: r }), /só na sua fase principal/);
});

test('jogarDe e reanimar com concessão: as cartas postas em campo já entram com Etéreo e tempo', () => {
  const traz: CartaDef = { ...base, id: 'traz', nome: 'Traz', tipo: 'temporaria', custo: 0, efeitos: [{ tipo: 'jogarDe', de: 'cemiterio', valor: 1, concessao: { palavras: ['Etéreo'], turnos: 1, destino: 'banimento' } }] };
  const altar: CartaDef = { ...base, id: 'altar', nome: 'Altar', tipo: 'estrutura', custo: 0, vida: 3, ativacao: { custo: 0, suspende: false, efeitos: [{ tipo: 'reanimar', custoExtra: 0, concessao: { palavras: ['Etéreo'], ataque: 1 } }] } };
  const e = novo([traz, altar]);
  principal(e);
  const z = e.jogadores[0];
  const a = daMao(e, 0, 'enchimento'); z.mao.splice(z.mao.indexOf(a), 1); z.cemiterio.push(a);
  executar(e, { tipo: 'jogar', jogador: 0, carta: daMao(e, 0, 'traz') });
  passarTodos(e);
  assert.equal(e.pendencia?.tipo, 'escolherCartas');
  executar(e, { tipo: 'escolherCartas', jogador: 0, cartas: [a] });
  assert.ok(z.campo.includes(a));
  assert.equal(etereo(e, a), true);
  assert.equal(e.instancias[a].turnosRestantes, 1);
  proximoTurno(e);
  assert.ok(z.banimento.includes(a), 'saiu no Final do 1º turno para o banimento');
  proximoTurno(e); // volta a ser o seu turno
  // reanimar (Diástaros): outra unidade, de volta do cemitério, com Etéreo e +1 de Ataque
  const b = daMao(e, 0, 'enchimento'); z.mao.splice(z.mao.indexOf(b), 1); z.cemiterio.push(b);
  const alt = daMao(e, 0, 'altar'); z.mao.splice(z.mao.indexOf(alt), 1); z.campo.push(alt); e.instancias[alt].entrouNoTurno = 0;
  z.arcano = 1;
  executar(e, { tipo: 'ativar', jogador: 0, carta: alt, alvo: b });
  passarTodos(e);
  assert.ok(z.campo.includes(b));
  assert.equal(etereo(e, b), true);
  assert.equal(ataqueAtualDe(e, b), 2);
});

function conjuradoresDef(id: string): ConjuradorDef { return (conjuradoresLista as ConjuradorDef[]).find((c) => c.id === id)!; }
