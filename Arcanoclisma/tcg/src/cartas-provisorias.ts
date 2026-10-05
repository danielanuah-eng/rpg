// Cartas PROVISÓRIAS (pedido do autor, 04/10, madrugada): a regra das cores (decisão 37) tirou dos 9 baralhos as
// cartas genéricas de 2+ Valores que não serviam mais ao Conjurador, e a troca automática (corrigirCores) pôs no lugar
// cartas genéricas do mesmo tipo e custo, sem a identidade do baralho. Estas cartas devolvem a função das que saíram
// (remoção, varredura, compra, anulação, cura, Aliados de evasão etc.), com 1 Valor cada (servem aos 2 Conjuradores que
// têm esse Valor), efeitos simples que o motor já tem e números dentro da régua de src/balanco.ts (teste em
// test/cartas-provisorias.test.ts). Ficam marcadas `provisoria: true` para o autor revisar no Editor do site.
// O nome é o título da arte aprovada ligada em online/dados/identidade.json (nunca da expansão A Mestra dos Dragões).
// Entram no catálogo das simulações (sim/variantes.ts) ANTES da curva de Grau: os Aliados ganham o bônus de Grau como os
// Aliados genéricos. Os números abaixo são os de antes da curva.
import { textoCarta } from '../app/textos.js';
import type { CartaDef, Efeito } from './tipos.ts';

const C = 'conflituo', S = 'sublimeyn', D = 'dominum', P = 'persona', A = 'alvorum', M = 'morbidyum', G = 'cognitium', E = 'eternyr', R = 'primitivum';
const molde = { subtipos: [] as string[], arcanoGerado: 1, cenario: '', provisoria: true as const };
const grau = (custo: number) => Math.max(0, custo - 1);

function temporaria(id: string, nome: string, valor: string, custo: number, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaDef {
  return { ...molde, id, nome, valores: [valor], tipo: 'temporaria', custo, grau: grau(custo), velocidade: 'findante', efeitos, texto: '', ...extra };
}
const repentina = { velocidade: 'repentina' as const };

function aliado(id: string, nome: string, valor: string, custo: number, ataque: number, vida: number, extra: Partial<CartaDef> = {}): CartaDef {
  return { ...molde, id, nome, valores: [valor], tipo: 'aliado', custo, grau: grau(custo), velocidade: 'findante', ataque, vida, texto: '', ...extra };
}

function estrutura(id: string, nome: string, valor: string, custo: number, vida: number, ativ: { custo: number; efeitos: Efeito[] }, extra: Partial<CartaDef> = {}): CartaDef {
  return { ...molde, id, nome, valores: [valor], tipo: 'estrutura', custo, grau: grau(custo), velocidade: 'findante', vida, ativacao: { custo: ativ.custo, suspende: true, efeitos: ativ.efeitos }, texto: '', ...extra };
}

/** Mística: Vida e, quando tem, uma ativação que suspende (repete a cada turno). */
function mistica(id: string, nome: string, valor: string, custo: number, vida: number, ativ: { custo: number; efeitos: Efeito[] } | null, extra: Partial<CartaDef> = {}): CartaDef {
  return { ...molde, id, nome, valores: [valor], tipo: 'mistica', custo, grau: grau(custo), velocidade: 'findante', vida, ...(ativ ? { ativacao: { custo: ativ.custo, suspende: true, efeitos: ativ.efeitos } } : {}), texto: '', ...extra };
}

const lista: CartaDef[] = [
  // ---------- Conflitüo (Lyandra, Osmar) ----------
  temporaria('prov-conflituo-dano-c2-repentina', 'Assassina Paga', C, 2, [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }], repentina),
  estrutura('prov-conflituo-torre-c4', 'Cidade-Muralha', C, 4, 2, { custo: 1, efeitos: [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }] }),
  aliado('prov-conflituo-piromante-c4', 'Arruaceiro de Taverna', C, 4, 2, 4, { aoEntrar: [{ tipo: 'danoTodosAliados', valor: 1 }] }),
  aliado('prov-conflituo-fera-c5', 'Cavalo de Guerra', C, 5, 2, 4, { aoEntrar: [{ tipo: 'dano', valor: 2, alvo: 'conjuradorInimigo' }, { tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }] }),
  aliado('prov-conflituo-aliado-c2-agressivo', 'Mercenário de Serath', C, 2, 3, 3),
  // ---------- Dominum (Mitonho, Osmar) ----------
  estrutura('prov-dominum-cerco-c3', 'Arena de Kairvahnn', D, 3, 3, { custo: 0, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] }),
  aliado('prov-dominum-colosso-c7', 'Autômato do Postulário', D, 7, 5, 8, { protegido: 2 }),
  temporaria('prov-dominum-ira-c4', 'Juíza da Cidade', D, 4, [{ tipo: 'destruirTodosAliados', ambos: true }], { grau: 5 }),
  temporaria('prov-dominum-destruir-c3', 'Carrasco da Cidade-Muralha', D, 3, [{ tipo: 'destruir', alvo: 'escolhido' }]),
  aliado('prov-dominum-aliado-c4-defensivo', 'Guarda da Cidade-Muralha', D, 4, 3, 7),
  // ---------- Eternyr (Ana, Mitonho) ----------
  temporaria('prov-eternyr-varredura-c3', 'Estação Parada', E, 3, [{ tipo: 'danoTodosAliados', valor: 2 }]),
  temporaria('prov-eternyr-remocao-grau-c3', 'Guardião do Ciclo', E, 3, [{ tipo: 'dano', valor: 1, alvo: 'escolhidoAliado', escalaGrau: true }]),
  temporaria('prov-eternyr-destruir-c4-repentina', 'O Eco que Come o Som', E, 4, [{ tipo: 'destruir', alvo: 'escolhido' }], repentina),
  temporaria('prov-eternyr-devolver-c2-repentina', 'Mensageira do Portal', E, 2, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }], repentina),
  // ---------- Cognitium (Diástaros, Ana) ----------
  aliado('prov-cognitium-espiao-c4', 'Espião da Coroa', G, 4, 2, 1, { palavrasChave: ['Etéreo'], aoDanoConjurador: [{ tipo: 'comprar', valor: 1 }] }),
  aliado('prov-cognitium-mercador-c7', 'Taverneira que Vende Segredos', G, 7, 1, 2, { palavrasChave: ['Etéreo'], protegido: 2, aoEntrar: [{ tipo: 'comprar', valor: 1 }], aoDanoConjurador: [{ tipo: 'comprar', valor: 1 }] }),
  temporaria('prov-cognitium-comprar-c2-repentina', 'Contadora de Mentes', G, 2, [{ tipo: 'comprar', valor: 1 }], repentina),
  temporaria('prov-cognitium-anular-c3-repentina', 'Carcereira de Palavras', G, 3, [{ tipo: 'anular', alvo: 'pilha' }], repentina),
  aliado('prov-cognitium-aliado-c4-defensivo', 'Inspetor de Escola', G, 4, 3, 7),
  // ---------- Persona (Roh, Agnelo) ----------
  aliado('prov-persona-espiao-c4', 'Encantadora de Salão', P, 4, 2, 1, { palavrasChave: ['Etéreo'], aoDanoConjurador: [{ tipo: 'comprar', valor: 1 }] }),
  aliado('prov-persona-aliado-c2-agressivo-proficiente', 'Trapaceiro de Feira', P, 2, 2, 2, { palavrasChave: ['Proficiente'] }),
  // ---------- Sublimeyn (Ricardo, Roh) ----------
  aliado('prov-sublimeyn-etereo-c2', 'Garça-de-Neblina', S, 2, 2, 2, { palavrasChave: ['Etéreo'] }),
  aliado('prov-sublimeyn-etereo-c3', 'Grifo-de-Vidro', S, 3, 3, 3, { palavrasChave: ['Etéreo'] }),
  temporaria('prov-sublimeyn-devolver-c2-repentina', 'Menina de Pele de Mercúrio', S, 2, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }], repentina),
  temporaria('prov-sublimeyn-dano-c2-repentina', 'Contrabandista de Mercúrio', S, 2, [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }], repentina),
  temporaria('prov-sublimeyn-dano-c1-repentina', 'Polvo do Porto de Vidro', S, 1, [{ tipo: 'dano', valor: 1, alvo: 'escolhido' }], repentina),
  temporaria('prov-sublimeyn-comprar-c2-repentina', 'Mercador de Sal', S, 2, [{ tipo: 'comprar', valor: 1 }], repentina),
  // ---------- Morbidyum (Agnelo, Diástaros) ----------
  temporaria('prov-morbidyum-curar-c2', 'Curandeira de Beira de Estrada', M, 2, [{ tipo: 'curar', valor: 6, alvo: 'escolhido' }]),
  temporaria('prov-morbidyum-curar-c4', 'Morwen Terceira-Vida', M, 4, [{ tipo: 'curar', valor: 7, alvo: 'escolhido' }, { tipo: 'comprar', valor: 1 }]),
  estrutura('prov-morbidyum-enfermaria-c3', 'Sacerdote da Porta', M, 3, 2, { custo: 0, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'escolhidoAliado' }] }),
  temporaria('prov-morbidyum-dano-c3', 'Abutre da Ferida', M, 3, [{ tipo: 'dano', valor: 4, alvo: 'escolhido' }]),
  aliado('prov-morbidyum-aliado-c6-descarta-grau', 'A Coisa Que Sobrou', M, 6, 9, 11, { descartaGrau: 2 }),
  temporaria('prov-morbidyum-comprar-c3', 'Velho de Ossos Claros', M, 3, [{ tipo: 'comprar', valor: 2 }]),
  // ---------- Alvorum (Carmal, Ricardo) ----------
  temporaria('prov-alvorum-varredura-c3', 'Olhos-de-Farol', A, 3, [{ tipo: 'danoTodosAliados', valor: 2 }]),
  aliado('prov-alvorum-anjo-c6', 'Cervo Branco', A, 6, 2, 4, { palavrasChave: ['Etéreo'], aoDanoConjurador: [{ tipo: 'curar', valor: 4, alvo: 'conjuradorProprio' }] }),
  aliado('prov-alvorum-bomba-c8', 'Juiz-Feiticeiro', A, 8, 2, 4, { palavrasChave: ['Etéreo'], protegido: 1, aoEntrar: [{ tipo: 'danoTodosAliados', valor: 2 }] }),
  estrutura('prov-alvorum-fonte-c4', 'AlvoMarfim', A, 4, 1, { custo: 0, efeitos: [{ tipo: 'arcano', valor: 2 }] }, { grauExtra: 1 }),
  temporaria('prov-alvorum-comprar-c3', 'Coruja-Sábia', A, 3, [{ tipo: 'comprar', valor: 2 }]),
  // ---------- Primitivum (Lyandra, Carmal) ----------
  aliado('prov-primitivum-aliado-c2-defensivo', 'Mula Teimosa', R, 2, 1, 5),
  // v2 (autor, 04/10): mais Primitivum, no lugar da cura genérica da Lyandra e do Aliado defensivo c4 genérico do Carmal
  temporaria('prov-primitivum-curar-c2', 'Invocadora de Espíritos', R, 2, [{ tipo: 'curar', valor: 4, alvo: 'escolhido' }, { tipo: 'arcano', valor: 1 }]),
  aliado('prov-primitivum-aliado-c4-defensivo', 'Urso-Raiz', R, 4, 3, 7),
  // ---------- Místicas (v2, autor 04/10): onde os baralhos tinham a Mística básica ----------
  mistica('prov-morbidyum-mistica-c2', 'Porteira da Antessala', M, 2, 2, { custo: 0, efeitos: [{ tipo: 'curar', valor: 2, alvo: 'escolhido' }] }),
  mistica('prov-morbidyum-mistica-c4', 'Fantasma em Fila', M, 4, 7, { custo: 1, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'escolhido' }] }),
  mistica('prov-cognitium-mistica-c2', 'Escriba do Porto', G, 2, 3, { custo: 2, efeitos: [{ tipo: 'comprar', valor: 1 }] }),
  mistica('prov-eternyr-mistica-c2', 'Velho Relojoeiro da Vila do Mesmo Dia', E, 2, 3, { custo: 1, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'escolhidoAliado' }] }),
  mistica('prov-dominum-mistica-c2', 'Nostrum', D, 2, 2, null, { protegido: 1, aoEntrar: [{ tipo: 'arcano', valor: 1 }] }),
];

/** As cartas provisórias, com o texto gerado da mecânica (app/textos.js). */
export const cartasProvisorias: CartaDef[] = lista.map((c) => ({ ...c, texto: textoCarta(c) }));
