// Ajustes pedidos pelo autor no envio de 03/10: reposição da mão no Início, Conjurador levanta no Início,
// itens do Conjurador (arma no Canal/Extra, Armadura que evita dano ao bloquear) e "olhar o topo".
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { ataqueConjurador, criarJogo, executar, validarBaralho } from '../src/motor.ts';
import { itensBasicos } from '../src/itens.ts';
import type { CartaDef, Catalogo, ConjuradorDef, Estado, Jogador, Regras } from '../src/tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;
const cartas: CartaDef[] = [
  { ...base, id: 'soldado', nome: 'Soldado', tipo: 'aliado', ataque: 3, vida: 9, palavrasChave: ['Proficiente'] },
  { ...base, id: 'olhar', nome: 'Olhar', tipo: 'temporaria', velocidade: 'repentina', efeitos: [{ tipo: 'olharTopo', quantidade: 3, pegar: 1 }] },
  { ...base, id: 'nada', nome: 'Nada', tipo: 'temporaria', efeitos: [{ tipo: 'comprar', valor: 0 }] },
  ...itensBasicos,
];
const novas: Partial<Regras> = { primeiroSemGrauNoTurno1: false, reporMaoNoInicio: true, conjuradorLevantaNoInicio: true, conjuradorSoAtacaComArma: true };

function criar(itens: string[], regras: Partial<Regras> = novas, danoBasico = 0): Estado {
  const conj: ConjuradorDef = { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico, custoAtaque: 0, habilidade: '' };
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: conj } };
  const ids = ['soldado', 'olhar', 'nada'];
  const deck = { conjurador: 'c', cartas: Array.from({ length: 30 }, (_, i) => ids[i % ids.length]), itens };
  const e = criarJogo({ catalogo: cat, baralhos: [deck, deck], semente: 5, regras });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia!.jogador, primeiro: 0 });
  while (e.etapa !== 'principal') executar(e, { tipo: 'passar', jogador: e.prioridade });
  return e;
}
const passarAte = (e: Estado, ok: () => boolean) => { let n = 0; while (!ok() && n++ < 200) executar(e, e.pendencia?.tipo === 'bloqueio' ? { tipo: 'bloquear', jogador: e.pendencia.jogador, bloqueador: null } : { tipo: 'passar', jogador: e.pendencia ? e.pendencia.jogador : e.prioridade }); };
function emCampo(e: Estado, j: Jogador, defId: string): string {
  const z = e.jogadores[j];
  const id = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === defId)!;
  for (const zona of [z.baralho, z.mao]) { const k = zona.indexOf(id); if (k >= 0) zona.splice(k, 1); }
  z.campo.push(id);
  e.instancias[id].entrouNoTurno = 0;
  return id;
}

test('itens: equipados na preparação; o ataque do Conjurador é o das armas; sem arma ele não ataca', () => {
  const e = criar(['item-canal-lamina', 'item-extra-pocao', 'item-armadura-gibao']);
  assert.equal(e.jogadores[0].itens.length, 3);
  assert.equal(ataqueConjurador(e, 0), 2); // o Extra consumível não dá Ataque
  const sem = criar(['item-armadura-gibao']);
  assert.equal(ataqueConjurador(sem, 0), 0);
  assert.throws(() => executar(sem, { tipo: 'atacar', jogador: 0, atacante: sem.jogadores[0].conjuradorId, alvo: sem.jogadores[1].conjuradorId }), /arma/);
  // sem a regra nova, o dano básico continua valendo
  const antigo = criar([], { primeiroSemGrauNoTurno1: false }, 1);
  executar(antigo, { tipo: 'atacar', jogador: 0, atacante: antigo.jogadores[0].conjuradorId, alvo: antigo.jogadores[1].conjuradorId });
});

test('validarBaralho: um item por espaço, item fora do baralho', () => {
  const cat: Catalogo = { cartas: Object.fromEntries(cartas.map((c) => [c.id, c])), conjuradores: { c: { id: 'c', nome: 'C', valores: [], comprometimento: 'Brigão', vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 0, custoAtaque: 0, habilidade: '' } } };
  const cartasOk = Array.from({ length: 60 }, () => 'nada');
  assert.deepEqual(validarBaralho(cat, { conjurador: 'c', cartas: cartasOk, itens: ['item-canal-cajado', 'item-armadura-gibao'] }), []);
  assert.equal(validarBaralho(cat, { conjurador: 'c', cartas: cartasOk, itens: ['item-canal-cajado', 'item-canal-lamina'] }).length, 1);
  assert.equal(validarBaralho(cat, { conjurador: 'c', cartas: [...cartasOk.slice(1), 'item-canal-cajado'] }).length, 1);
});

test('armadura: quando o Conjurador bloqueia, evita até a Defesa e quebra ao completar o desgaste', () => {
  const e = criar(['item-canal-cajado', 'item-armadura-gibao']); // Gibão: defesa 1, desgaste 4
  const conj1 = e.jogadores[1].conjuradorId;
  const gibao = e.jogadores[1].itens.find((id) => e.instancias[id].defId === 'item-armadura-gibao')!;
  for (let k = 0; k < 4; k++) {
    const atacante = emCampo(e, 0, 'soldado');
    e.instancias[conj1].suspensa = false;
    e.habUsos[1].armadura = false; // como se fosse outro turno: a Armadura protege um ataque por turno
    executar(e, { tipo: 'atacar', jogador: 0, atacante, alvo: conj1 });
    passarAte(e, () => e.pendencia?.tipo === 'bloqueio');
    executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: conj1 });
    passarAte(e, () => e.combate === null);
  }
  // 4 ataques de 3: a armadura evitou 1 em cada (4 no total) e quebrou; o Conjurador sofreu 2 em cada
  assert.equal(e.instancias[conj1].dano, 8);
  // o Conjurador que bloqueia não devolve o dano da arma
  assert.equal(e.instancias[e.jogadores[0].conjuradorId].dano, 0);
  assert.ok(!e.jogadores[1].itens.includes(gibao));
  assert.ok(e.jogadores[1].cemiterio.includes(gibao));
});

test('armadura: protege só um ataque por turno, e o dono pode escolher não usar', () => {
  const e = criar(['item-canal-cajado', 'item-armadura-gibao']); // Gibão: defesa 1
  const conj1 = e.jogadores[1].conjuradorId;
  const gibao = e.jogadores[1].itens.find((id) => e.instancias[id].defId === 'item-armadura-gibao')!;
  const bloquear = (usarArmadura?: boolean) => {
    const atacante = emCampo(e, 0, 'soldado');
    e.instancias[conj1].suspensa = false;
    executar(e, { tipo: 'atacar', jogador: 0, atacante, alvo: conj1 });
    passarAte(e, () => e.pendencia?.tipo === 'bloqueio');
    executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: conj1, usarArmadura });
    passarAte(e, () => e.combate === null);
  };
  bloquear(false); // escolheu não usar: sofre os 3 e a armadura não se desgasta
  assert.equal(e.instancias[conj1].dano, 3);
  assert.equal(e.instancias[gibao].dano, 0);
  bloquear(); // usa: evita 1
  assert.equal(e.instancias[conj1].dano, 5);
  bloquear(); // mesmo turno: já protegeu um ataque
  assert.equal(e.instancias[conj1].dano, 8);
  assert.equal(e.instancias[gibao].dano, 1);
});

test('sem bloquear, a armadura não evita dano', () => {
  const e = criar(['item-canal-cajado', 'item-armadura-couraca']);
  const conj1 = e.jogadores[1].conjuradorId;
  const atacante = emCampo(e, 0, 'soldado');
  executar(e, { tipo: 'atacar', jogador: 0, atacante, alvo: conj1 });
  passarAte(e, () => e.pendencia?.tipo === 'bloqueio');
  executar(e, { tipo: 'bloquear', jogador: 1, bloqueador: null });
  passarAte(e, () => e.combate === null);
  assert.equal(e.instancias[conj1].dano, 3);
});

test('Início: repõe a mão até a mão inicial do Conjurador e levanta o Conjurador que bloqueou', () => {
  const e = criar(['item-canal-cajado']);
  const z1 = e.jogadores[1];
  // o jogador 1 gasta cartas e bloqueia com o Conjurador no turno do jogador 0
  z1.cemiterio.push(...z1.mao.splice(0, 3));
  e.instancias[z1.conjuradorId].suspensa = true;
  passarAte(e, () => e.ativo === 1 && e.etapa === 'principal');
  assert.equal(z1.mao.length, 5);
  assert.equal(e.instancias[z1.conjuradorId].suspensa, false);
  // e o Final não compra mais
  passarAte(e, () => e.ativo === 0 && e.etapa === 'inicio');
  assert.equal(z1.mao.length, 5);
});

test('olhar o topo: escolhe 1 entre 3, o resto vai para o fundo do baralho', () => {
  const e = criar(['item-canal-cajado']);
  const z = e.jogadores[0];
  const carta = [...z.baralho, ...z.mao].find((i) => e.instancias[i].defId === 'olhar')!;
  for (const zona of [z.baralho, z.mao]) { const k = zona.indexOf(carta); if (k >= 0) zona.splice(k, 1); }
  z.mao.push(carta);
  const topo = z.baralho.slice(0, 3);
  const tamanho = z.baralho.length;
  executar(e, { tipo: 'jogar', jogador: 0, carta });
  passarAte(e, () => e.pendencia?.tipo === 'escolherDoTopo');
  assert.deepEqual(e.pendencia!.tipo === 'escolherDoTopo' && e.pendencia.cartas, topo);
  assert.throws(() => executar(e, { tipo: 'escolherDoTopo', jogador: 0, cartas: [topo[0], topo[1]] }));
  executar(e, { tipo: 'escolherDoTopo', jogador: 0, cartas: [topo[1]] });
  assert.ok(z.mao.includes(topo[1]));
  assert.deepEqual(z.baralho.slice(-2), [topo[0], topo[2]]);
  assert.equal(z.baralho.length, tamanho - 1);
  assert.equal(e.pendencia, null);
});

test('Extra é consumível: usa uma vez, aplica o efeito e vai para o cemitério; item sem uso (arma) não se ativa', () => {
  const e = criar(['item-canal-cajado', 'item-armadura-gibao', 'item-extra-elixir']);
  const z = e.jogadores[0];
  const extra = z.itens.find((id) => e.instancias[id].defId === 'item-extra-elixir')!;
  const canal = z.itens.find((id) => e.instancias[id].defId === 'item-canal-cajado')!;
  assert.throws(() => executar(e, { tipo: 'ativar', jogador: 0, carta: canal }), /não tem uso/);
  executar(e, { tipo: 'ativar', jogador: 0, carta: extra });
  passarAte(e, () => e.pilha.length === 0);
  assert.equal(z.arcano, 2);
  assert.ok(!z.itens.includes(extra));
  assert.ok(z.cemiterio.includes(extra));
});
