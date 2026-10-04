// Decisão 30 (pedido do autor, 04/10): Protessência — a carta não pode ser alvo de efeitos do oponente, nem anulada por ele.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar, alvosCampo, protegidaDe, temPalavra, ErroRegra } from '../src/motor.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Efeito, Estado, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '' };
const temp = (id: string, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', custo: 0, efeitos, ...extra });

const CARTAS: CartaDef[] = [
  { ...base, id: 'ali', nome: 'Ali', tipo: 'aliado', custo: 1, ataque: 2, vida: 3 },
  { ...base, id: 'prot', nome: 'Prot', tipo: 'aliado', custo: 1, ataque: 1, vida: 3, palavrasChave: ['Protessência'] },
  temp('dano', [{ tipo: 'dano', valor: 1, alvo: 'escolhido' }]),
  temp('destroi', [{ tipo: 'destruir', alvo: 'escolhido' }]),
  temp('devolve', [{ tipo: 'devolver', alvo: 'escolhido' }]),
  temp('campoDestroi', [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'ambos', tipos: ['aliado'] }]),
  temp('depois', [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'], escolha: 'afetado' }]),
  temp('acaso', [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'], escolha: 'aleatorio' }]),
  temp('massa', [{ tipo: 'campo', acao: 'dano', valor: 1, alcance: 'todos', lado: 'inimigo', tipos: ['aliado'] }]),
  temp('anula', [{ tipo: 'anular', alvo: 'pilha' }], { velocidade: 'repentina' }),
  temp('saber', [{ tipo: 'comprar', valor: 1 }], { palavrasChave: ['Protessência'] }),
  temp('veu', [{ tipo: 'campo', acao: 'conceder', alcance: 'alvo', lado: 'proprio', tipos: ['aliado'], concessao: { palavras: ['Protessência'], palavrasTurnos: 1 } }], { velocidade: 'repentina' }),
];

function novo(): Estado {
  const cat: Catalogo = { cartas: Object.fromEntries(CARTAS.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const ids = Object.keys(cat.cartas);
  const deck = { conjurador: 'c', cartas: Array.from({ length: 48 }, (_, i) => ids[i % ids.length]) };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 11 });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') {
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); continue; }
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
  return e;
}
function pegar(e: Estado, j: Jogador, defId: string, onde: 'mao' | 'campo'): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId && !(onde === 'campo' && z.mao.includes(i)))!;
  for (const zona of [z.baralho, z.mao]) { const k = zona.indexOf(id); if (k >= 0) zona.splice(k, 1); }
  if (onde === 'campo') { z.campo.push(id); e.instancias[id].revelada = true; } else z.mao.push(id);
  return id;
}
function resolverPilha(e: Estado): void {
  while (e.pilha.length && !e.pendencia) executar(e, { tipo: 'passar', jogador: e.prioridade });
}
const lados = (e: Estado) => { const j = e.ativo; return { j, o: (j === 0 ? 1 : 0) as Jogador }; };

for (const carta of ['dano', 'destroi', 'devolve', 'campoDestroi']) {
  test(`${carta}: o oponente não mira a Protessência; o dono mira`, () => {
    const e = novo();
    const { j, o } = lados(e);
    const prot = pegar(e, o, 'prot', 'campo');
    assert.ok(protegidaDe(e, prot, j) && !protegidaDe(e, prot, o));
    assert.throws(() => executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, carta, 'mao'), alvo: prot }), ErroRegra);
    // o dono pode mirar a própria carta
    const meu = pegar(e, j, 'prot', 'campo');
    executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, carta, 'mao'), alvo: meu });
    resolverPilha(e);
    assert.ok(carta === 'dano' ? e.instancias[meu].dano === 1 : !e.jogadores[j].campo.includes(meu));
  });
}

test('escolha posterior ("o jogador atingido escolhe"): a Protessência não entra na lista', () => {
  const e = novo();
  const { j, o } = lados(e);
  const prot = pegar(e, o, 'prot', 'campo');
  const ali = pegar(e, o, 'ali', 'campo');
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'depois', 'mao') });
  resolverPilha(e);
  const p = e.pendencia as Extract<NonNullable<Estado['pendencia']>, { tipo: 'escolherCartas' }>;
  assert.equal(p.tipo, 'escolherCartas');
  assert.deepEqual(p.cartas, [ali]);
  assert.throws(() => executar(e, { tipo: 'escolherCartas', jogador: p.jogador, cartas: [prot] }));
});

test('ao acaso: o sorteio do oponente não inclui a Protessência', () => {
  for (let k = 0; k < 6; k++) {
    const e = novo();
    const { j, o } = lados(e);
    const prot = pegar(e, o, 'prot', 'campo');
    const ali = pegar(e, o, 'ali', 'campo');
    executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'acaso', 'mao') });
    resolverPilha(e);
    assert.ok(e.jogadores[o].campo.includes(prot));
    assert.ok(!e.jogadores[o].campo.includes(ali));
  }
  const e = novo();
  const { j, o } = lados(e);
  const prot = pegar(e, o, 'prot', 'campo');
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'acaso', 'mao') });
  resolverPilha(e);
  assert.ok(e.jogadores[o].campo.includes(prot), 'só ela em campo: o efeito fica sem alvo');
});

test('anular na pilha: o oponente não escolhe a carta com Protessência', () => {
  const e = novo();
  const { j, o } = lados(e);
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'saber', 'mao') });
  const naPilha = e.pilha[e.pilha.length - 1];
  assert.equal(naPilha.tipo, 'carta');
  if (naPilha.tipo !== 'carta') return;
  if (e.prioridade === j) executar(e, { tipo: 'passar', jogador: j });
  assert.equal(e.prioridade, o);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: o, carta: pegar(e, o, 'anula', 'mao'), alvo: naPilha.instId }), ErroRegra);
});

test('efeito em massa (todos) atinge a Protessência', () => {
  const e = novo();
  const { j, o } = lados(e);
  const prot = pegar(e, o, 'prot', 'campo');
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'massa', 'mao') });
  resolverPilha(e);
  assert.equal(e.instancias[prot].dano, 1);
  const lista = alvosCampo(e, j, { tipo: 'campo', acao: 'dano', alcance: 'todos', lado: 'inimigo', tipos: ['aliado'] });
  assert.ok(lista.includes(prot));
});

test('ganhou Protessência com o efeito na pilha: o efeito perde o alvo', () => {
  const e = novo();
  const { j, o } = lados(e);
  const ali = pegar(e, o, 'ali', 'campo');
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'dano', 'mao'), alvo: ali });
  if (e.prioridade === j) executar(e, { tipo: 'passar', jogador: j });
  assert.equal(e.prioridade, o);
  executar(e, { tipo: 'jogar', jogador: o, carta: pegar(e, o, 'veu', 'mao'), alvo: ali });
  resolverPilha(e);
  assert.ok(temPalavra(e, ali, 'Protessência'));
  assert.equal(e.instancias[ali].dano, 0);
  assert.ok(e.log.some((l) => /Alvo protegido/.test(l)));
});

test('concedida por turnos: vale e expira', () => {
  const e = novo();
  const { j, o } = lados(e);
  const meu = pegar(e, j, 'ali', 'campo');
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'veu', 'mao'), alvo: meu });
  resolverPilha(e);
  assert.ok(protegidaDe(e, meu, o));
  const t = e.turno;
  let guarda = 0;
  while (e.turno === t && guarda++ < 50) {
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); continue; }
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
  assert.ok(e.turno > t);
  assert.ok(!temPalavra(e, meu, 'Protessência'), 'a Protessência some no Final do turno');
  assert.ok(!protegidaDe(e, meu, o));
});

test('ataque de unidade não é efeito: Aliado com Protessência pode ser atacado', () => {
  const e = novo();
  const { j, o } = lados(e);
  const prot = pegar(e, o, 'prot', 'campo');
  const ali = pegar(e, j, 'ali', 'campo');
  e.instancias[ali].entrouNoTurno = -5;
  executar(e, { tipo: 'atacar', jogador: j, atacante: ali, alvo: prot, enviarArcano: [] });
  assert.ok(e.combate || e.pilha.length || e.instancias[prot].dano > 0);
});
