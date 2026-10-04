// Pedidos do autor de 04/10: vários bloqueadores com dano distribuído e excedente no Conjurador,
// ataque a Aliado/Mística/Estrutura sem bloqueio, Aliado nunca Repentina pelo tipo, Conjurador levanta só no Final.
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { criarJogo, executar } from '../src/motor.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador, Regras } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const cartas: CartaDef[] = [
  { ...base, id: 'gigante', nome: 'Gigante', tipo: 'aliado', ataque: 10, vida: 20, palavrasChave: ['Proficiente'] },
  { ...base, id: 'soldado', nome: 'Soldado', tipo: 'aliado', ataque: 3, vida: 4, palavrasChave: ['Proficiente'] },
  { ...base, id: 'escudeiro', nome: 'Escudeiro', tipo: 'aliado', ataque: 2, vida: 2, palavrasChave: ['Proficiente'] },
  { ...base, id: 'torre', nome: 'Torre', tipo: 'estrutura', vida: 5 },
  { ...base, id: 'raposa', nome: 'Raposa', tipo: 'aliado', velocidade: 'repentina', ataque: 1, vida: 1 },
  { ...base, id: 'gato', nome: 'Gato', tipo: 'aliado', ataque: 1, vida: 1, palavrasChave: ['Repentino'] },
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
];
const regras: Partial<Regras> = { primeiroSemGrauNoTurno1: false };

function criar(r: Partial<Regras> = regras): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 30, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, () => 'nada') };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 3, regras: r });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
let n = 0;
function poe(e: Estado, j: Jogador, defId: string, onde: 'campo' | 'mao' | 'grau' = 'campo'): string {
  const id = `t${n++}`;
  e.instancias[id] = { id, defId, ehConjurador: false, dono: j, suspensa: false, dano: 0, revelada: false, entrouNoTurno: 0, banidaDisponivel: 0, descontoBanimento: 0, banirAoResolver: false };
  e.jogadores[j][onde].push(id);
  return id;
}
const passar = (e: Estado) => executar(e, { tipo: 'passar', jogador: e.prioridade });
/** Ataca e passa até a decisão de bloqueio (ou até o ataque resolver sem bloqueio). */
function atacar(e: Estado, atacante: string, alvo: string): void {
  executar(e, { tipo: 'atacar', jogador: 0, atacante, alvo });
  passar(e); passar(e);
}

test('vários bloqueadores: quem ataca distribui; o excedente vai para o Conjurador; todos revidam, mesmo morrendo', () => {
  const e = criar();
  const g = poe(e, 0, 'gigante');
  const s = poe(e, 1, 'soldado');
  const esc = poe(e, 1, 'escudeiro');
  const conj1 = e.jogadores[1].conjuradorId;
  atacar(e, g, conj1);
  assert.equal(e.pendencia?.tipo, 'bloqueio');
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueadores: [s, esc] });
  assert.deepEqual(e.combate?.bloqueadores, [s, esc]);
  passar(e); passar(e); // janela antes do dano
  assert.equal(e.pendencia?.tipo, 'distribuirDano');
  assert.equal(e.pendencia?.jogador, 0);
  assert.throws(() => executar(e, { tipo: 'distribuirDano', jogador: 0, distribuicao: { [s]: 3 } }), /exatamente 10/);
  executar(e, { tipo: 'distribuirDano', jogador: 0, distribuicao: { [s]: 7, [esc]: 3 } });
  // Soldado (vida 4) leva 7: 3 de excedente; Escudeiro (vida 2) leva 3: 1 de excedente → 4 no Conjurador
  assert.equal(e.instancias[conj1].dano, 4);
  assert.ok(e.jogadores[1].cemiterio.includes(s) && e.jogadores[1].cemiterio.includes(esc));
  assert.equal(e.instancias[g].dano, 5); // 3 + 2, os dois morreram e ainda assim causaram dano
});

test('um bloqueador também passa o excedente; distribuição padrão derruba o máximo', () => {
  const e = criar();
  const g = poe(e, 0, 'gigante');
  const s = poe(e, 1, 'soldado');
  const conj1 = e.jogadores[1].conjuradorId;
  atacar(e, g, conj1);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: s });
  passar(e); passar(e);
  assert.equal(e.pendencia, null); // um só bloqueador: não há o que distribuir
  assert.equal(e.instancias[conj1].dano, 6);
  // padrão com dois: sem escolha, o motor derruba os dois e manda o resto ao Conjurador
  const f = criar();
  const g2 = poe(f, 0, 'gigante');
  const a = poe(f, 1, 'soldado');
  const b = poe(f, 1, 'escudeiro');
  atacar(f, g2, f.jogadores[1].conjuradorId);
  executar(f, { tipo: 'bloquear', jogador: 1, bloqueadores: [a, b] });
  passar(f); passar(f);
  executar(f, { tipo: 'distribuirDano', jogador: 0 });
  assert.equal(f.instancias[f.jogadores[1].conjuradorId].dano, 4);
});

test('ataque a Aliado, Mística ou Estrutura não pode ser bloqueado, mas a pilha e a janela antes do dano continuam', () => {
  const e = criar();
  const g = poe(e, 0, 'gigante');
  const alvo = poe(e, 1, 'soldado');
  const s = poe(e, 1, 'escudeiro');
  atacar(e, g, alvo);
  assert.equal(e.pendencia, null);
  assert.equal(e.combate?.etapa, 'bloqueado');
  assert.ok(e.log.some((l) => l.includes('não pode ser bloqueado')));
  passar(e); passar(e);
  assert.ok(e.jogadores[1].cemiterio.includes(alvo));
  assert.equal(e.instancias[g].dano, 3); // o Aliado atacado revida
  assert.ok(e.jogadores[1].campo.includes(s));
  // Estrutura com Vida pode ser atacada
  const f = criar();
  const g2 = poe(f, 0, 'gigante');
  const t = poe(f, 1, 'torre');
  atacar(f, g2, t);
  assert.equal(f.pendencia, null);
  passar(f); passar(f);
  assert.ok(f.jogadores[1].cemiterio.includes(t));
});

test('não se declara bloqueio de ataque a um Aliado', () => {
  const e = criar();
  const g = poe(e, 0, 'gigante');
  const alvo = poe(e, 1, 'soldado');
  executar(e, { tipo: 'atacar', jogador: 0, atacante: g, alvo });
  passar(e); passar(e);
  assert.throws(() => executar(e, { tipo: 'bloquear', jogador: 1, bloqueadores: [alvo] }));
});

test('Aliado nunca é Repentina pelo tipo; só entra fora da fase principal com a palavra-chave Repentino', () => {
  const e = criar();
  const r = poe(e, 1, 'raposa', 'mao');
  const g = poe(e, 1, 'gato', 'mao');
  passar(e); // o jogador 1 recebe prioridade na fase principal do jogador 0
  assert.equal(e.prioridade, 1);
  assert.throws(() => executar(e, { tipo: 'jogar', jogador: 1, carta: r }), /fase principal/);
  executar(e, { tipo: 'jogar', jogador: 1, carta: g });
  assert.equal(e.pilha.at(-1)?.tipo, 'carta');
});

test('o Conjurador que bloqueou só levanta no Final do dono (regra conjuradorLevantaNoInicio desligada)', () => {
  const e = criar({ ...regras, conjuradorLevantaNoInicio: false });
  const g = poe(e, 0, 'gigante');
  const conj1 = e.jogadores[1].conjuradorId;
  atacar(e, g, conj1);
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: conj1 });
  passar(e); passar(e);
  while (e.ativo !== 1 || e.etapa !== 'principal') executar(e, e.pendencia ? { tipo: 'descartar', jogador: e.pendencia.jogador, cartas: e.jogadores[e.pendencia.jogador].mao.slice(0, (e.pendencia as { quantidade: number }).quantidade) } : { tipo: 'passar', jogador: e.prioridade });
  assert.equal(e.instancias[conj1].suspensa, true); // no Início e na Principal do jogador 1 continua deitado
  while (e.ativo === 1) passar(e);
  assert.equal(e.instancias[conj1].suspensa, false);
});

test('requisito por linha (Editor): Grau mínimo e Arcano pago na hora; sem cumprir, só aquela linha não acontece', async () => {
  const { textoCarta } = await import('../app/textos.js');
  const e = criar();
  e.catalogo.cartas['req'] = { ...base, id: 'req', nome: 'Req', tipo: 'temporaria', efeitos: [{ tipo: 'arcano', valor: 1 }, { tipo: 'comprar', valor: 2, requisito: { grau: 1 } }, { tipo: 'comprar', valor: 1, requisito: { arcano: 5 } }] };
  assert.match(textoCarta(e.catalogo.cartas['req']), /\(1\+ no Grau\) compre 2 cartas e \(Pague 5\) compre 1 carta/);
  const c = poe(e, 0, 'req', 'mao');
  const mao = e.jogadores[0].mao.length;
  executar(e, { tipo: 'jogar', jogador: 0, carta: c });
  passar(e); passar(e);
  // sem Grau e sem 5 de Arcano: só a primeira linha acontece
  assert.equal(e.jogadores[0].mao.length, mao - 1);
  assert.equal(e.jogadores[0].arcano, 1);
  assert.equal(e.log.filter((l) => l.includes('Requisito não cumprido')).length, 2);
  // com Grau 1 e Arcano 5 de reserva: tudo acontece e o Arcano é pago
  const f = criar();
  f.catalogo.cartas['req'] = e.catalogo.cartas['req'];
  poe(f, 0, 'nada', 'grau');
  f.jogadores[0].arcano = 5;
  const c2 = poe(f, 0, 'req', 'mao');
  const mao2 = f.jogadores[0].mao.length;
  executar(f, { tipo: 'jogar', jogador: 0, carta: c2 });
  passar(f); passar(f);
  assert.equal(f.jogadores[0].mao.length, mao2 - 1 + 3);
  assert.equal(f.jogadores[0].arcano, 5 + 1 - 5);
});
