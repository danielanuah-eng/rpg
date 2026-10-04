// Decisão 28 (bug do autor, 04/10): destruir/devolver/mandar ao baralho nunca miram o Conjurador.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar, alvosCampo, efeitoSemConjurador, ErroRegra } from '../src/motor.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Efeito, Estado, Jogador } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 12, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '' };
const temp = (id: string, efeitos: Efeito[]): CartaDef => ({ ...base, id, nome: id, tipo: 'temporaria', custo: 0, efeitos });

const CARTAS: CartaDef[] = [
  { ...base, id: 'ali', nome: 'Ali', tipo: 'aliado', custo: 1, ataque: 1, vida: 3 },
  { ...base, id: 'mis', nome: 'Mis', tipo: 'mistica', custo: 1, ataque: 0, vida: 3 },
  temp('destroi', [{ tipo: 'destruir', alvo: 'escolhido' }]),
  temp('devolve', [{ tipo: 'devolver', alvo: 'escolhido' }]),
  temp('campoConj', [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'inimigo', tipos: ['conjurador', 'aliado'] }]),
  temp('campoSemTipo', [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'inimigo', tipos: [] }]),
  temp('campoDepois', [{ tipo: 'campo', acao: 'destruir', alcance: 'alvo', lado: 'inimigo', tipos: ['conjurador', 'aliado'], escolha: 'afetado' }]),
  temp('campoDano', [{ tipo: 'campo', acao: 'dano', valor: 1, alcance: 'alvo', lado: 'inimigo', tipos: ['conjurador', 'aliado'] }]),
];

function novo(): Estado {
  const cat: Catalogo = { cartas: Object.fromEntries(CARTAS.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const ids = Object.keys(cat.cartas);
  const deck = { conjurador: 'c', cartas: Array.from({ length: 32 }, (_, i) => ids[i % ids.length]) };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 7 });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') {
    if (e.pendencia?.tipo === 'descartar') { executar(e, { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, e.pendencia.quantidade) }); continue; }
    executar(e, { tipo: 'passar', jogador: e.prioridade });
    executar(e, { tipo: 'passar', jogador: e.prioridade });
  }
  return e;
}
/** Tira uma carta do baralho/mão do jogador e a põe na mão (ou no campo). */
function pegar(e: Estado, j: Jogador, defId: string, onde: 'mao' | 'campo'): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId && !(onde === 'campo' && z.mao.includes(i)))!;
  for (const zona of [z.baralho, z.mao]) { const k = zona.indexOf(id); if (k >= 0) zona.splice(k, 1); }
  if (onde === 'campo') { z.campo.push(id); e.instancias[id].revelada = true; } else z.mao.push(id);
  return id;
}
function passarTodos(e: Estado): void {
  executar(e, { tipo: 'passar', jogador: e.prioridade });
  executar(e, { tipo: 'passar', jogador: e.prioridade });
}

for (const carta of ['destroi', 'devolve', 'campoConj', 'campoSemTipo']) {
  test(`${carta}: recusa o Conjurador e aceita Aliado/Mística`, () => {
    const e = novo();
    const j = e.ativo; const o = (j === 0 ? 1 : 0) as Jogador;
    const ali = pegar(e, o, 'ali', 'campo');
    const mis = pegar(e, o, 'mis', 'campo');
    const c1 = pegar(e, j, carta, 'mao');
    assert.throws(() => executar(e, { tipo: 'jogar', jogador: j, carta: c1, alvo: e.jogadores[o].conjuradorId }), ErroRegra);
    executar(e, { tipo: 'jogar', jogador: j, carta: c1, alvo: ali });
    passarTodos(e);
    assert.ok(!e.jogadores[o].campo.includes(ali), 'o Aliado sai do campo');
    if (carta !== 'campoConj') { // campoConj só declara Conjurador e Aliado
      const c2 = pegar(e, j, carta, 'mao');
      executar(e, { tipo: 'jogar', jogador: j, carta: c2, alvo: mis });
      passarTodos(e);
      assert.ok(!e.jogadores[o].campo.includes(mis), 'a Mística sai do campo');
    }
    assert.equal(e.jogadores[o].conjuradorId in e.instancias, true);
  });
}

test('campo destruir com escolha depois: o Conjurador não entra na lista', () => {
  const e = novo();
  const j = e.ativo; const o = (j === 0 ? 1 : 0) as Jogador;
  const ali = pegar(e, o, 'ali', 'campo');
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'campoDepois', 'mao') });
  passarTodos(e);
  assert.equal(e.pendencia?.tipo, 'escolherCartas');
  const p = e.pendencia as Extract<NonNullable<Estado['pendencia']>, { tipo: 'escolherCartas' }>;
  assert.deepEqual(p.cartas, [ali]);
  assert.throws(() => executar(e, { tipo: 'escolherCartas', jogador: p.jogador, cartas: [e.jogadores[o].conjuradorId] }));
});

test('dano do efeito campo continua podendo mirar o Conjurador', () => {
  const e = novo();
  const j = e.ativo; const o = (j === 0 ? 1 : 0) as Jogador;
  const cid = e.jogadores[o].conjuradorId;
  executar(e, { tipo: 'jogar', jogador: j, carta: pegar(e, j, 'campoDano', 'mao'), alvo: cid });
  passarTodos(e);
  assert.equal(e.instancias[cid].dano, 1);
});

test('efeitoSemConjurador: destruir/devolver/paraBaralho e concessão só de tempo', () => {
  const campo = (acao: string, extra = {}) => ({ tipo: 'campo', acao, alcance: 'alvo', lado: 'ambos', tipos: ['conjurador'], ...extra }) as Efeito;
  assert.ok(efeitoSemConjurador({ tipo: 'destruir', alvo: 'escolhido' }));
  assert.ok(efeitoSemConjurador(campo('paraBaralho')));
  assert.ok(efeitoSemConjurador(campo('conceder', { concessao: { turnos: 2 } })));
  assert.ok(!efeitoSemConjurador(campo('conceder', { concessao: { turnos: 2, palavras: ['Etéreo'] } })));
  assert.ok(!efeitoSemConjurador(campo('curar')));
  assert.ok(!efeitoSemConjurador({ tipo: 'dano', valor: 1, alvo: 'escolhido' }));
  const e = novo();
  assert.deepEqual(alvosCampo(e, 0, campo('devolver') as Extract<Efeito, { tipo: 'campo' }>), []);
  assert.equal(alvosCampo(e, 0, campo('aumentarVida') as Extract<Efeito, { tipo: 'campo' }>).length, 2);
});
