// Coleção da primeira versão online (03/10/2026): cartas e itens que faltavam para cada mecânica do motor ter ao menos
// uma carta jogável. Os nomes vêm das artes aprovadas (cartas/catalogo-artes.json); o campo `arte` liga a carta à imagem.
// Nenhum baralho pronto usa estas cartas, então as simulações de balanceamento não mudam. Os números foram conferidos com
// a régua de pontos (src/balanco.ts, teste em test/colecao.test.ts) e ficam abertos à revisão do autor.
import type { CartaDef, CategoriaItem, Efeito } from './tipos.ts';
import { cartasNovasV2 } from './cartas-novas-v2.ts';

export type CartaColecao = CartaDef & { arte: string };

const base = { subtipos: [] as string[], grau: 0, velocidade: 'findante' as const, arcanoGerado: 1, cenario: '', texto: '' };

function aliado(id: string, arte: string, nome: string, valores: string[], custo: number, ataque: number, vida: number, extra: Partial<CartaDef> = {}): CartaColecao {
  return { ...base, id, arte, nome, tipo: 'aliado', valores, custo, grau: Math.max(0, custo - 1), ataque, vida, ...extra };
}
function temporaria(id: string, arte: string, nome: string, valores: string[], custo: number, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaColecao {
  return { ...base, id, arte, nome, tipo: 'temporaria', valores, custo, grau: Math.max(0, custo - 1), efeitos, ...extra };
}
function item(id: string, arte: string, nome: string, categoriaItem: CategoriaItem, valores: string[], campos: Partial<CartaDef>): CartaColecao {
  return {
    ...base, id, arte, nome, tipo: 'item', categoriaItem, subtipos: [categoriaItem === 'armadura' ? 'Proteção' : 'Equipamento'], valores,
    custo: 0, grau: 0, arcanoGerado: 0, ...(categoriaItem === 'canal' ? { tipoCanal: 'arma' as const, subtipos: ['Arma'] } : {}), ...campos,
  };
}
const consumivel = (efeitos: Efeito[], custo = 0) => ({ custo, ativacao: { custo, suspende: false, efeitos } });
/** Itens v2 (pedido do autor, 03/10, noite): todo uso suspende o Conjurador e tem um custo (Arcano, descartar, tomar dano). */
const usar = (custo: number, efeitos: Efeito[], extra: Partial<NonNullable<CartaDef['ativacao']>> = {}) => ({ custo, ativacao: { custo, suspende: false, suspendeConjurador: true, efeitos, ...extra } });
const foco = { tipoCanal: 'foco' as const, subtipos: ['Foco Arcano'] };
const transcritor = { tipoCanal: 'transcritor' as const, subtipos: ['Transcritor'] };
const competencia = (c: NonNullable<CartaDef['competencia']>) => ({ competencia: c, subtipos: [c] });

/** Campeões de Valor: exigem uma carta do próprio Valor no Grau (mecânica custoValores). Um por Valor. */
const campeoes: [string, string, string][] = [
  ['conflituo', 'ferreiro-de-guerra', 'Ferreiro de Guerra'],
  ['sublimeyn', 'sonhador-perdido', 'Sonhador Perdido'],
  ['dominum', 'juramentado', 'Juramentado'],
  ['persona', 'o-seu-rosto', 'O Seu Rosto'],
  ['alvorum', 'o-ministrum', 'O Ministrum'],
  ['morbidyum', 'o-que-voltou', 'O que Voltou'],
  ['cognitium', 'bibliotecaria-do-arquivo', 'Bibliotecária do Arquivo'],
  ['eternyr', 'repetido', 'Repetido'],
  ['primitivum', 'espirito-animal', 'Espírito Animal'],
];

export const cartasColecao: CartaColecao[] = [
  ...campeoes.map(([v, arte, nome]) => aliado(`col-campeao-${v}`, arte, nome, [v], 3, 4, 5, { custoValores: [v], subtipos: ['Ser dos Planos'] })),

  // Aliados com a palavra-chave Repentino (decisão 21: Aliado só entra fora da fase principal com Repentino no texto)
  aliado('col-gato-de-navio', 'gato-de-navio', 'Gato de Navio', ['persona', 'primitivum'], 1, 1, 1, { subtipos: ['Fera'], palavrasChave: ['Repentino'] }),
  aliado('col-menina-raposa', 'menina-raposa', 'Menina Raposa', ['persona', 'sublimeyn', 'primitivum'], 2, 2, 2, { palavrasChave: ['Repentino'] }),
  // Gatilhos ao entrar e ao causar dano de combate ao Conjurador
  aliado('col-ilusionista-do-labirinto', 'ilusionista-do-labirinto', 'Ilusionista do Labirinto', ['sublimeyn', 'persona'], 4, 2, 3, { aoEntrar: [{ tipo: 'devolver', alvo: 'escolhidoAliado' }] }),
  aliado('col-ceifadora-da-peste', 'ceifadora-da-peste', 'Ceifadora da Peste', ['morbidyum', 'dominum'], 6, 3, 4, { aoEntrar: [{ tipo: 'destruir', alvo: 'escolhidoAliado' }] }),
  aliado('col-contrabandista-de-porto', 'contrabandista-de-porto', 'Contrabandista de Porto', ['persona', 'cognitium'], 3, 1, 2, { palavrasChave: ['Etéreo'], aoDanoConjurador: [{ tipo: 'arcano', valor: 1 }] }),
  aliado('col-vento-mensageiro', 'vento-mensageiro', 'Vento Mensageiro', ['sublimeyn', 'conflituo'], 3, 2, 1, { palavrasChave: ['Etéreo', 'Proficiente'], subtipos: ['Ser de energia'] }),
  aliado('col-guarda-costas', 'guarda-costas-contratado', 'Guarda-costas Contratado', ['dominum', 'conflituo'], 3, 2, 3, { protegido: 2 }),
  aliado('col-curandeira-de-ervas', 'curandeira-de-ervas', 'Curandeira de Ervas', ['morbidyum', 'alvorum', 'primitivum'], 2, 0, 2, { ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'curar', valor: 2, alvo: 'escolhido' }] } }),

  // Místicas com ativação e com gatilho ao entrar
  { ...base, id: 'col-ninho-de-estrelas', arte: 'ninho-de-estrelas', nome: 'Ninho de Estrelas', tipo: 'mistica', valores: ['alvorum', 'morbidyum', 'sublimeyn'], custo: 3, grau: 2, vida: 3, subtipos: ['Aura'],
    ativacao: { custo: 1, suspende: true, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }] } },
  { ...base, id: 'col-coroa-sem-rei', arte: 'coroa-sem-rei', nome: 'Coroa sem Rei', tipo: 'mistica', valores: ['dominum', 'persona'], custo: 4, grau: 3, vida: 6, subtipos: ['Relíquia'],
    aoEntrar: [{ tipo: 'comprar', valor: 1 }] },
  // Estrutura que barateia a próxima carta (descontoProximaCarta só funciona fora de uma carta jogada)
  { ...base, id: 'col-tapete-de-duas-casas', arte: 'tapete-de-duas-casas', nome: 'Tapete de Duas Casas', tipo: 'estrutura', valores: ['alvorum', 'sublimeyn'], custo: 2, grau: 1, vida: 2,
    ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'descontoProximaCarta', valor: 1 }] } },

  // Temporárias das mecânicas que ainda não tinham carta
  temporaria('col-primeira-moeda', 'a-primeira-moeda', 'A Primeira Moeda', ['persona', 'cognitium', 'alvorum'], 2, [{ tipo: 'comprar', valor: 2 }, { tipo: 'arcanoOponente', valor: 1 }], { velocidade: 'repentina' }),
  temporaria('col-sino-dos-juramentados', 'sino-dos-juramentados', 'Sino dos Juramentados', ['conflituo', 'primitivum', 'dominum'], 2, [{ tipo: 'dano', valor: 5, alvo: 'conjuradorInimigo' }], { pagarParaAnular: 3 }),
  temporaria('col-ancora-do-mesmo-dia', 'ancora-do-mesmo-dia', 'Âncora do Mesmo Dia', ['eternyr', 'cognitium'], 1, [{ tipo: 'banirDaMao', desconto: 3 }]),
  temporaria('col-cemiterio-que-respira', 'cemiterio-que-respira', 'Cemitério que Respira', ['morbidyum', 'cognitium'], 6, [{ tipo: 'reanimar', custoExtra: 0, pagamento: 'gratis' }]), // 04/10: o motor nunca cobrou o custo do Aliado ao jogar; 'gratis' mantém igual
  temporaria('col-cadeado-ciumento', 'cadeado-ciumento', 'Cadeado Ciumento', ['eternyr', 'dominum'], 1, [{ tipo: 'permitirAnularDoCemiterio' }, { tipo: 'comprar', valor: 1 }], { velocidade: 'repentina' }),
  temporaria('col-frasco-de-mare', 'frasco-de-mare', 'Frasco de Maré', ['alvorum', 'primitivum', 'eternyr'], 2, [{ tipo: 'arcano', valor: 3 }]),
  temporaria('col-chapeu-de-chuva', 'chapeu-de-chuva-particular', 'Chapéu de Chuva Particular', ['morbidyum', 'alvorum', 'dominum'], 2, [{ tipo: 'curar', valor: 4, alvo: 'conjuradorProprio' }], { velocidade: 'repentina' }),
  temporaria('col-barco-dobravel', 'barco-dobravel', 'Barco Dobrável', ['sublimeyn', 'persona', 'eternyr'], 2, [{ tipo: 'devolver', alvo: 'escolhidoProprio' }], { velocidade: 'repentina' }),
  temporaria('col-mapa-do-reino', 'mapa-do-reino', 'Mapa do Reino', ['cognitium', 'sublimeyn', 'eternyr'], 3, [{ tipo: 'olharTopo', quantidade: 5, pegar: 2 }]),
  temporaria('col-arqueira-da-ultima-flecha', 'arqueira-da-ultima-flecha', 'Última Flecha', ['conflituo', 'primitivum', 'persona'], 3, [{ tipo: 'dano', valor: 3, alvo: 'escolhidoAliado' }], { velocidade: 'repentina' }),
  temporaria('col-mare-de-sobras', 'mare-de-sobras', 'Maré de Sobras', ['morbidyum', 'eternyr'], 4, [{ tipo: 'danoTodosAliados', valor: 2, ambos: true }, { tipo: 'comprar', valor: 1 }]),
  temporaria('col-descartar-grau', 'coroa-riscada', 'Coroa Riscada', ['dominum', 'alvorum'], 3, [{ tipo: 'destruir', alvo: 'escolhido' }, { tipo: 'comprar', valor: 1 }], { descartaGrau: 1 }),

  // Itens não básicos (pedido do autor, 04/10): todos com efeito e com custo e Grau próprios (podem ser 0). Não são melhores
  // que os básicos: servem a situações diferentes. Item com Valor só serve ao Conjurador que tem os dois Valores dele.
  // Arma: custo e Grau da carta = Arcano e Grau mínimo para atacar. Armadura: para proteger. Extra/Foco/Transcritor: para usar.
  item('item-canal-harpa', 'harpa-de-vidro-de-liss', 'Harpa de Vidro de Liss', 'canal', ['sublimeyn', 'persona', 'cognitium'], { ataque: 1, custo: 1, grau: 1, aoAtacar: { efeitos: [{ tipo: 'comprar', valor: 1 }] } }),
  item('item-canal-badalo', 'o-badalo-de-vau-grande', 'O Badalo de Vau Grande', 'canal', ['conflituo', 'dominum', 'primitivum'], { ataque: 2, custo: 1, grau: 2, aoAtacar: { danoUnidades: 2 } }),
  item('item-canal-violino', 'violino-do-ontem', 'Violino do Ontem', 'canal', ['eternyr', 'cognitium', 'morbidyum'], { ataque: 1, aoAtacar: { efeitos: [{ tipo: 'arcano', valor: 1 }] } }),
  item('item-canal-coleira', 'coleira-de-kairvahnn', 'Coleira de Kairvahnn', 'canal', ['primitivum', 'conflituo', 'alvorum'], { ataque: 3, custo: 2, grau: 3, aoAtacar: { desgasteArmadura: 2 } }),
  item('item-armadura-capa', 'capa-de-vento', 'Capa de Vento', 'armadura', [], { defesa: 1, vidaDesgaste: 6, aoProteger: [{ tipo: 'arcano', valor: 1 }] }),
  item('item-armadura-escama', 'escama-do-dragao-mercurio', 'Escama do Dragão Mercúrio', 'armadura', ['alvorum', 'primitivum', 'sublimeyn'], { defesa: 2, vidaDesgaste: 5, grau: 2, espinhos: 1 }),
  item('item-armadura-manto', 'manto-de-cor-de-parede', 'Manto de Cor de Parede', 'armadura', ['persona', 'sublimeyn', 'morbidyum'], { defesa: 1, vidaDesgaste: 7, grau: 1, aoProteger: [{ tipo: 'comprar', valor: 1 }] }),
  item('item-armadura-vyal', 'escama-do-vyal', 'Escama do Vyal', 'armadura', ['dominum', 'eternyr', 'conflituo'], { defesa: 3, vidaDesgaste: 4, custo: 1, grau: 3, espinhos: 2 }),
  item('item-extra-sal', 'sal-do-viajante', 'Sal do Viajante', 'extra', [], consumivel([{ tipo: 'olharTopo', quantidade: 3, pegar: 1 }])),
  item('item-extra-dado', 'dado-honesto', 'Dado Honesto', 'extra', [], { grau: 1, ...consumivel([{ tipo: 'dano', valor: 2, alvo: 'escolhido' }], 1) }),
  item('item-extra-lanterna', 'lanterna-do-veu', 'Lanterna do Véu', 'extra', ['sublimeyn', 'persona', 'eternyr'], { grau: 2, ...consumivel([{ tipo: 'devolver', alvo: 'escolhidoAliado' }], 1) }),
  item('item-extra-relogio', 'relogio-de-bolso-atrasado', 'Relógio de Bolso Atrasado', 'extra', ['eternyr', 'cognitium', 'alvorum'], { grau: 2, ...consumivel([{ tipo: 'comprar', valor: 1 }, { tipo: 'arcano', valor: 1 }]) }),

  // Itens v2 (03/10, noite). Canal Foco Arcano: melhora a próxima magia (início, meio e fim de jogo). Sem Ataque: o Conjurador não ataca com ele.
  item('item-foco-espelho', 'espelho-da-cidade-dos-rostos', 'Espelho da Cidade dos Rostos', 'canal', [], { ...foco, ...usar(1, [{ tipo: 'potenciaMagia', valor: 1 }]) }),
  item('item-foco-pedra', 'pedra-parada', 'Pedra Parada', 'canal', [], { ...foco, grau: 1, ...usar(0, [{ tipo: 'potenciaMagia', valor: 2 }], { descartar: 1 }) }),
  item('item-foco-coroa', 'a-coroa-de-vyal', 'A Coroa de Vyal', 'canal', [], { ...foco, grau: 4, ...usar(2, [{ tipo: 'potenciaMagia', valor: 4 }], { danoNoConjurador: 2 }) }),
  // Canal Transcritor: mexe no baralho e na mão.
  item('item-transcritor-pena', 'pena-de-idrael', 'Pena de Idrael', 'canal', [], { ...transcritor, ...usar(1, [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }]) }),
  item('item-transcritor-pagina', 'a-pagina-arrancada', 'A Página Arrancada', 'canal', [], { ...transcritor, grau: 1, ...usar(0, [{ tipo: 'comprar', valor: 1 }], { danoNoConjurador: 1 }) }),
  item('item-transcritor-chave', 'chave-da-voragem', 'Chave da Voragem', 'canal', [], { ...transcritor, grau: 3, ...usar(2, [{ tipo: 'comprar', valor: 3 }], { descartar: 1 }) }),
  // Extras por competência (Comprometimento do Conjurador), de uso único.
  item('item-extra-ovo', 'ovo-de-wyvern', 'Ovo de Wyvern', 'extra', [], { ...competencia('Invocador'), grau: 3, ...usar(2, [{ tipo: 'invocar', carta: 'ficha-wyvern-filhote', nome: 'um Wyvern Filhote 2/2' }], { consome: true }) }),
  item('item-extra-luvas', 'cordao-de-rhu', 'Luvas de Força', 'extra', [], { ...competencia('Brigão'), ativacao: { custo: 0, suspende: false, danoNoConjurador: 2, consome: true, efeitos: [{ tipo: 'ataqueConjuradorTurno', valor: 2 }] } }),
  item('item-extra-trono', 'trono-dobravel', 'Trono Dobrável', 'extra', [], { ...competencia('Parrudão da quebrada'), grau: 2, ...usar(1, [{ tipo: 'curar', valor: 4, alvo: 'conjuradorProprio' }], { consome: true }) }),
  item('item-extra-pente', 'pente-das-mil-caras', 'Pente das Mil Caras', 'extra', [], { ...competencia('Habilidoso'), grau: 1, ...usar(0, [{ tipo: 'olharTopo', quantidade: 3, pegar: 2 }], { descartar: 1, consome: true }) }),
  item('item-extra-balde', 'balde-que-sobe', 'Balde que Sobe', 'extra', [], { ...competencia('Preparado'), ...usar(0, [{ tipo: 'arcano', valor: 2 }, { tipo: 'comprar', valor: 1 }], { danoNoConjurador: 1, consome: true }) }),
  // Ficha (criada pelo Ovo de Wyvern; não entra em baralho)
  aliado('ficha-wyvern-filhote', 'dragonete', 'Wyvern Filhote', [], 0, 2, 2, { ficha: true, subtipos: ['Ficha', 'Dragão'] }),
  // Lote 2 (03/10, noite): mortos-vivos, Temporárias, Repentinas, Místicas, Estruturas e lugares
  ...cartasNovasV2,
];
