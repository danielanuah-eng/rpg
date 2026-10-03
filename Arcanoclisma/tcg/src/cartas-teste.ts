// Cartas e Conjuradores PLACEHOLDER só para testar o motor. Não são a coleção.
import type { BaralhoDef, CartaDef, Catalogo, ConjuradorDef } from './tipos.ts';

const base = { subtipos: [], valores: [], grau: 0, custo: 0, velocidade: 'findante', arcanoGerado: 1, cenario: '', texto: '' } as const;

const cartas: CartaDef[] = [
  { ...base, id: 'aprendiz-da-torre', nome: 'Aprendiz da Torre', tipo: 'aliado', custo: 1, ataque: 1, vida: 2,
    cenario: 'Um aprendiz corre pelas escadas da torre com um livro maior que ele.', texto: '' },
  { ...base, id: 'batedora-ligeira', nome: 'Batedora Ligeira', tipo: 'aliado', custo: 1, ataque: 1, vida: 1, palavrasChave: ['Proficiente'],
    cenario: 'Ela já estava correndo antes de a trombeta soar.', texto: 'Proficiente.' },
  { ...base, id: 'guarda-de-ferro', nome: 'Guarda de Ferro', tipo: 'aliado', custo: 2, grau: 1, ataque: 2, vida: 3,
    cenario: 'O portão da cidade range, e o guarda não sai do lugar.', texto: '' },
  { ...base, id: 'dragao-das-cinzas', nome: 'Dragão das Cinzas', tipo: 'aliado', subtipos: ['dragão'], custo: 4, grau: 2, descartaGrau: 1,
    ataque: 5, vida: 5, arcanoGerado: 2, cenario: 'Um vulcão acorda e a montanha inteira respira fogo.', texto: 'Ao jogar, descarte 1 carta do seu Grau.' },
  { ...base, id: 'bola-de-fogo', nome: 'Bola de Fogo', tipo: 'temporaria', custo: 2, velocidade: 'repentina',
    efeitos: [{ tipo: 'dano', valor: 3, alvo: 'escolhido' }],
    cenario: 'Aventureiros correm enquanto o céu fica laranja.', texto: 'Causa 3 de dano a um alvo.' },
  { ...base, id: 'raio-do-conjurador', nome: 'Raio do Conjurador', tipo: 'temporaria', custo: 3,
    efeitos: [{ tipo: 'dano', valor: 4, alvo: 'conjuradorInimigo' }],
    cenario: 'Um relâmpago cai direto no cajado.', texto: 'Causa 4 de dano ao Conjurador inimigo. Findante.' },
  { ...base, id: 'visao-do-oraculo', nome: 'Visão do Oráculo', tipo: 'temporaria', custo: 1, velocidade: 'repentina',
    efeitos: [{ tipo: 'comprar', valor: 2 }], cenario: 'Um véu de luz mostra o que ainda vai acontecer.', texto: 'Compre 2 cartas.' },
  { ...base, id: 'curandeira-do-vale', nome: 'Curandeira do Vale', tipo: 'aliado', custo: 2, ataque: 0, vida: 2,
    aoEntrar: [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }], cenario: 'A curandeira chega com o orvalho da manhã.',
    texto: 'Ao entrar, cure 3 de dano de um alvo.' },
  { ...base, id: 'barreira-de-luz', nome: 'Barreira de Luz', tipo: 'mistica', custo: 2, vida: 4, cenario: 'Um véu de luz fecha o desfiladeiro.', texto: '' },
  { ...base, id: 'moinho-antigo', nome: 'Moinho Antigo', tipo: 'estrutura', custo: 1, vida: 3, aoEntrar: [{ tipo: 'arcano', valor: 1 }],
    cenario: 'O moinho ainda gira, mesmo sem vento.', texto: 'Ao entrar, ganhe 1 Arcano.' },
];

const conjuradores: ConjuradorDef[] = [
  { id: 'maga-da-cinza', nome: 'Maga da Cinza', valores: [], comprometimento: 'Brigão', vida: 10, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '' },
  { id: 'guardiao-do-vale', nome: 'Guardião do Vale', valores: [], comprometimento: 'Parrudão da quebrada', vida: 12, maoInicial: 5,
    compraFinal: 2, danoBasico: 1, custoAtaque: 1, habilidade: '' },
];

export const catalogoTeste: Catalogo = {
  cartas: Object.fromEntries(cartas.map((c) => [c.id, c])),
  conjuradores: Object.fromEntries(conjuradores.map((c) => [c.id, c])),
};

/** Baralho de teste com `n` cartas (ciclando entre as cartas do catálogo de teste). */
export function baralhoTeste(conjurador: string, n = 30): BaralhoDef {
  const ids = Object.keys(catalogoTeste.cartas);
  return { conjurador, cartas: Array.from({ length: n }, (_, i) => ids[i % ids.length]) };
}
