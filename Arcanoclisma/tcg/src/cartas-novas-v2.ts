// Cartas novas, lote 2 (pedido do autor, 03/10/2026, noite): mortos-vivos, Temporárias, Repentinas, Místicas, Estruturas
// e lugares (casas, portos, feiras). Toda carta tem Valor (quase sempre 1), espalhadas pelos 9, com reforço nos Valores com
// menos cartas (Persona, Cognitium, Morbidyum). Só usam efeitos que o motor já executa. Arte provisória: uma arte aprovada
// livre do catálogo; o GPT faz a arte final com cenário e o efeito da magia de cada uma, ligado à arte
// (pedidos: cartas/pedidos/lote-site-v2.claude.json e efeitos/pedidos/mesa-efeitos-v1.claude.json).
// Números conferidos com a régua de pontos (src/balanco.ts; teste em online/test/online.test.ts). Abertos à revisão do autor.
import type { CartaDef, Efeito } from './tipos.ts';
import type { CartaColecao } from './cartas-colecao.ts';

const base = { subtipos: [] as string[], grau: 0, velocidade: 'findante' as const, arcanoGerado: 1, cenario: '', texto: '' };
const g = (custo: number) => Math.max(0, custo - 1);
const aliado = (id: string, arte: string, nome: string, v: string, custo: number, ataque: number, vida: number, extra: Partial<CartaDef> = {}): CartaColecao =>
  ({ ...base, id, arte, nome, tipo: 'aliado', valores: [v], custo, grau: g(custo), ataque, vida, ...extra });
const temp = (id: string, arte: string, nome: string, v: string, custo: number, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaColecao =>
  ({ ...base, id, arte, nome, tipo: 'temporaria', valores: [v], custo, grau: g(custo), efeitos, ...extra });
const repentina = (id: string, arte: string, nome: string, v: string, custo: number, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaColecao =>
  temp(id, arte, nome, v, custo, efeitos, { velocidade: 'repentina', ...extra });
const mistica = (id: string, arte: string, nome: string, v: string, custo: number, vida: number, extra: Partial<CartaDef>): CartaColecao =>
  ({ ...base, id, arte, nome, tipo: 'mistica', valores: [v], custo, grau: g(custo), vida, ...extra });
const estrutura = (id: string, arte: string, nome: string, v: string, custo: number, vida: number, extra: Partial<CartaDef>): CartaColecao =>
  ({ ...base, id, arte, nome, tipo: 'estrutura', valores: [v], custo, grau: g(custo), vida, ...extra });
const morto = ['Morto-vivo'];

export const cartasNovasV2: CartaColecao[] = [
  // ---- Mortos-vivos (Morbidyum, com um Cognitium e um Persona) ----
  aliado('nv-luz-de-velorio', 'luz-de-velorio', 'Luz de Velório', 'morbidyum', 1, 1, 1, { subtipos: morto, palavrasChave: ['Etéreo'] }),
  aliado('nv-carcaca-andante', 'carcaca-andante', 'Carcaça Andante', 'morbidyum', 2, 2, 3, { subtipos: morto }),
  aliado('nv-coveiro', 'coveiro-de-vau-grande', 'Coveiro de Vau Grande', 'morbidyum', 2, 1, 2, { subtipos: ['Humano'], aoEntrar: [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }] }),
  aliado('nv-embalsamador', 'embalsamador', 'Embalsamador', 'morbidyum', 3, 2, 3, { subtipos: ['Humano'], ativacao: { custo: 2, suspende: true, efeitos: [{ tipo: 'reanimar', custoExtra: 1 }] } }),
  aliado('nv-ossario', 'velho-ossario-dragao-palido', 'Velho Ossário, o Dragão Pálido', 'morbidyum', 7, 7, 7, { subtipos: [...morto, 'Dragão'], aoEntrar: [{ tipo: 'reanimar', custoExtra: 0 }] }),
  aliado('nv-esquecido', 'esquecido', 'O Esquecido', 'cognitium', 2, 2, 1, { subtipos: morto, palavrasChave: ['Etéreo'] }),
  aliado('nv-moca-que-nao-sangra', 'moca-que-nao-sangra', 'Moça que Não Sangra', 'persona', 3, 3, 3, { subtipos: morto, protegido: 1 }),

  // ---- Temporárias (Findantes), uma por Valor ----
  temp('nv-rosto-sem-gente', 'rosto-sem-gente', 'Rosto sem Gente', 'persona', 2, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }]),
  temp('nv-leitura-de-sombras', 'leitor-de-sombras', 'Leitura de Sombras', 'cognitium', 2, [{ tipo: 'olharTopo', quantidade: 4, pegar: 2 }]),
  temp('nv-sentenca-do-selo', 'juiz-do-selo', 'Sentença do Selo', 'dominum', 4, [{ tipo: 'destruir', alvo: 'escolhidoAliado' }]),
  temp('nv-luz-da-faroleira', 'faroleira-de-vau-grande', 'Luz da Faroleira', 'alvorum', 3, [{ tipo: 'curar', valor: 4, alvo: 'conjuradorProprio' }, { tipo: 'comprar', valor: 1 }]),
  temp('nv-chamado-da-matilha', 'matilha-da-voragem', 'Chamado da Matilha', 'primitivum', 3, [{ tipo: 'danoTodosAliados', valor: 2 }]),
  temp('nv-quebra-escudos', 'quebra-escudos', 'Golpe do Quebra-Escudos', 'conflituo', 3, [{ tipo: 'dano', valor: 4, alvo: 'escolhidoAliado' }]),
  temp('nv-nuvem-triste', 'nuvem-triste', 'Nuvem Triste', 'sublimeyn', 3, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }, { tipo: 'comprar', valor: 1 }]),
  temp('nv-cadeira-de-espera', 'cadeira-de-espera', 'Cadeira de Espera', 'eternyr', 1, [{ tipo: 'banirDaMao', desconto: 2 }]),
  temp('nv-ladrao-de-tumulos', 'ladrao-de-tumulos', 'Saque de Túmulos', 'morbidyum', 2, [{ tipo: 'comprar', valor: 2 }]),

  // ---- Repentinas ----
  repentina('nv-mascara-do-outro', 'mascara-do-outro', 'Máscara do Outro', 'persona', 2, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }]),
  repentina('nv-refutacao', 'professor-de-logica', 'Refutação', 'cognitium', 3, [{ tipo: 'anular', alvo: 'pilha' }]),
  repentina('nv-lamento-da-carpideira', 'carpideira', 'Lamento da Carpideira', 'morbidyum', 2, [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }]),
  repentina('nv-hora-roubada', 'relojoeiro-fugitivo', 'Hora Roubada', 'eternyr', 1, [{ tipo: 'comprar', valor: 1 }]),
  repentina('nv-selo-subito', 'sentinela-de-selo', 'Selo Súbito', 'alvorum', 1, [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }]),
  repentina('nv-tiro-de-aluguel', 'pistoleira-de-aluguel', 'Tiro de Aluguel', 'conflituo', 2, [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }]),
  repentina('nv-espelho-da-serpente', 'serpente-espelho', 'Espelho da Serpente', 'sublimeyn', 2, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }]),

  // ---- Místicas ----
  mistica('nv-dama-da-noite', 'a-dama-da-noite-de-fhim', 'A Dama da Noite de Fhim', 'morbidyum', 3, 3, { subtipos: ['Aura'], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] } }),
  mistica('nv-canto-sem-cantor', 'canto-sem-cantor', 'Canto sem Cantor', 'persona', 3, 3, { subtipos: ['Aura'], ativacao: { custo: 2, suspende: true, efeitos: [{ tipo: 'comprar', valor: 1 }] } }),
  mistica('nv-o-sentido', 'sentido', 'O Sentido', 'cognitium', 2, 2, { subtipos: ['Presença'], ativacao: { custo: 1, suspende: true, efeitos: [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }] } }),
  mistica('nv-tique', 'tique', 'Tique, o Segundo a Mais', 'eternyr', 4, 4, { subtipos: ['Semideus'], aoEntrar: [{ tipo: 'comprar', valor: 1 }], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] } }),
  mistica('nv-hael', 'hael-o-insone', 'Hael, o Insone', 'alvorum', 5, 5, { subtipos: ['Semideus'], ativacao: { custo: 1, suspende: true, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }] } }),
  mistica('nv-estandarte-sem-exercito', 'estandarte-sem-exercito', 'Estandarte sem Exército', 'dominum', 3, 4, { subtipos: ['Relíquia'], protegido: 1, aoEntrar: [{ tipo: 'arcano', valor: 1 }] }),

  // ---- Estruturas e lugares (casas, portos, feiras) ----
  estrutura('nv-feira-das-mascaras', 'feira-das-mascaras', 'Feira das Máscaras', 'persona', 2, 3, { subtipos: ['Lugar'], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] } }),
  estrutura('nv-o-postulario', 'o-postulario', 'O Postulário', 'cognitium', 3, 4, { subtipos: ['Lugar'], ativacao: { custo: 1, suspende: true, efeitos: [{ tipo: 'comprar', valor: 1 }] } }),
  estrutura('nv-brejo-alto', 'brejo-alto', 'Brejo Alto', 'morbidyum', 3, 4, { subtipos: ['Lugar'], grauExtra: 1 }),
  estrutura('nv-patio-de-duelos', 'patio-de-duelos', 'Pátio de Duelos', 'conflituo', 3, 4, { subtipos: ['Lugar'], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] } }),
  estrutura('nv-porto-de-vidro', 'o-porto-de-vidro', 'O Porto de Vidro', 'sublimeyn', 2, 3, { subtipos: ['Lugar'], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'arcano', valor: 1 }] } }),
  estrutura('nv-mercado-de-feras', 'mercado-de-feras', 'Mercado de Feras', 'primitivum', 3, 4, { subtipos: ['Lugar'], ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'descontoProximaCarta', valor: 1 }] } }),
  estrutura('nv-casas-de-sarn', 'sarn-das-escadas', 'Casas de Sarn das Escadas', 'alvorum', 2, 3, { subtipos: ['Casa'], grauExtra: 1 }),
  estrutura('nv-estalagem-de-vau', 'estalajadeiro-de-vau-grande', 'Estalagem de Vau Grande', 'dominum', 3, 4, { subtipos: ['Casa'], ativacao: { custo: 1, suspende: true, efeitos: [{ tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }] } }),
  estrutura('nv-vila-do-mesmo-dia', 'vila-do-mesmo-dia', 'Vila do Mesmo Dia', 'eternyr', 3, 4, { subtipos: ['Casa'], ativacao: { custo: 2, suspende: true, efeitos: [{ tipo: 'olharTopo', quantidade: 3, pegar: 1 }] } }),
  estrutura('nv-floresta-que-anda', 'floresta-que-anda', 'Floresta que Anda', 'primitivum', 4, 6, { subtipos: ['Lugar'], grauExtra: 1, aoEntrar: [{ tipo: 'arcano', valor: 1 }] }),
];
