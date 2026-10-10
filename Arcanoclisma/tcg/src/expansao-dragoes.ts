// Expansão A Mestra dos Dragões (liberada pelo autor em 09/10/2026: "pode lançar a expansão de dragões"; o Claude propõe e
// põe as cartas em jogo e o autor ajusta pelo Editor do site). Decisão 41 do livro de regras.
// - 65 cartas jogáveis das artes 050–114 (Arcanoclisma/expansoes/a-mestra-dos-dragoes/arte/colecao-completa-90.visual.json):
//   12 monstros (Aliados), 8 magias dracônicas (Temporárias), 8 equipamentos (itens), 6 Estruturas, 6 Místicas e 25
//   personagens que viram Conjuradores (um par de Valores cada, no nível dos 9 de src/conjuradores.ts).
// - Dragolize (genérica nos dados): fichas de Ovo (Iluminado e Etéreo) criadas por algumas cartas; `entraSobre: 'Ovo'` (a carta
//   ou ficha só entra em cima de um Ovo seu, que sai do campo e passa a ela a sua `concedeAoSubstituto`); a arma Extra
//   Durandis, o Receptáculo Dracono, com 9 habilidades (um Dragão por Valor, pagando X de Arcano com X cartas daquele Valor no
//   Grau: `requisito.valor`); Dragões com habilidades ativadas pagas em Arcano.
// - As 25 cenas 025–049 não viram cartas; as 9 de dragão por Valor (039–047) e a dos ovos rúnicos (034) servem só de arte das
//   fichas (fichas não são cartas de baralho).
// Números pela régua de src/balanco.ts (teste em test/expansao-dragoes.test.ts); sem raridade (o autor escolhe no Editor).
// Fora das simulações de balanceamento: só o site (online/construir.ts) junta estas cartas ao catálogo.
import { textoCarta, textoPoder } from '../app/textos.js';
import type { CartaDef, CategoriaItem, ConjuradorDef, Efeito, Habilidade, PoderConjurador } from './tipos.ts';

export const EXPANSAO_DRAGOES = 'a-mestra-dos-dragoes';
/** Carta da expansão com o número da arte (manifesto da expansão). Fichas sem arte própria usam a cena indicada. */
export type CartaExpansao = CartaDef & { arteExpansao?: string };

const C = 'conflituo', S = 'sublimeyn', D = 'dominum', P = 'persona', A = 'alvorum', M = 'morbidyum', G = 'cognitium', E = 'eternyr', R = 'primitivum';
const molde = { subtipos: [] as string[], arcanoGerado: 1, cenario: '', texto: '', expansao: EXPANSAO_DRAGOES, velocidade: 'findante' as const };
const grau = (custo: number) => Math.max(0, custo - 1);
const rep = { velocidade: 'repentina' as const };

// ---------- fichas ----------
export const OVO_ILUMINADO = 'md-ficha-ovo-iluminado';
export const OVO_ETEREO = 'md-ficha-ovo-etereo';
const ovoIluminado: Efeito = { tipo: 'invocar', carta: OVO_ILUMINADO, nome: 'um Ovo Iluminado' };
const ovoEtereo: Efeito = { tipo: 'invocar', carta: OVO_ETEREO, nome: 'um Ovo Etéreo' };

function ficha(id: string, nome: string, tipo: 'aliado' | 'mistica', valores: string[], ataque: number | undefined, vida: number, extra: Partial<CartaDef>, arte: string): CartaExpansao {
  return { ...molde, arcanoGerado: 0, id, nome, tipo, valores, custo: 0, grau: 0, ficha: true, ...(ataque !== undefined ? { ataque } : {}), vida, ...extra, arteExpansao: arte };
}

/** Os 9 Dragões de Durandis: Valor, X (Arcano e cartas daquele Valor no Grau), arte, nome e corpo. */
const DRAGOES: Array<{ v: string; x: number; arte: string; nome: string; ataque: number; vida: number; extra: Partial<CartaDef> }> = [
  { v: C, x: 3, arte: '039', nome: 'Dragão de Crista Ferrugem', ataque: 4, vida: 4, extra: { palavrasChave: ['Proficiente'], ativacao: { custo: 2, suspende: false, efeitos: [{ tipo: 'dano', valor: 1, alvo: 'escolhidoAliado' }] } } },
  { v: S, x: 2, arte: '040', nome: 'Dragão de Fita Mercurial', ataque: 3, vida: 3, extra: { palavrasChave: ['Etéreo'], ativacao: { custo: 3, suspende: true, efeitos: [{ tipo: 'devolver', alvo: 'escolhidoAliado' }] } } },
  { v: D, x: 3, arte: '041', nome: 'Dragão de Coroa Rubra', ataque: 4, vida: 7, extra: { protegido: 1, ativacao: { custo: 2, suspende: false, efeitos: [{ tipo: 'campo', acao: 'naoAtaca', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }] } } },
  { v: P, x: 2, arte: '042', nome: 'Dragão de Asas em Leque', ataque: 3, vida: 4, extra: { ativacao: { custo: 2, suspende: true, efeitos: [ovoEtereo] } } },
  { v: A, x: 3, arte: '043', nome: 'Dragão de Carvão e Ouro', ataque: 4, vida: 6, extra: { ativacao: { custo: 2, suspende: false, efeitos: [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }] } } },
  { v: M, x: 3, arte: '044', nome: 'Dragão de Casulo Pálido', ataque: 5, vida: 5, extra: { aoDanoConjurador: [{ tipo: 'curar', valor: 3, alvo: 'conjuradorProprio' }] } },
  { v: G, x: 3, arte: '045', nome: 'Dragão de Chifres Prismáticos', ataque: 3, vida: 5, extra: { palavrasChave: ['Etéreo'], ativacao: { custo: 2, suspende: true, efeitos: [{ tipo: 'comprar', valor: 1 }] } } },
  { v: E, x: 5, arte: '046', nome: 'Dragão Antigo de Eternyr', ataque: 7, vida: 9, extra: { ativacao: { custo: 4, suspende: true, efeitos: [{ tipo: 'anular', alvo: 'pilha' }] } } },
  { v: R, x: 4, arte: '047', nome: 'Dragão de Raízes e Musgo', ataque: 7, vida: 8, extra: { aoEntrar: [ovoIluminado], ativacao: { custo: 2, suspende: false, efeitos: [{ tipo: 'curar', valor: 2, alvo: 'escolhidoAliado' }] } } },
];
export const idDragao = (v: string) => `md-ficha-dragao-${v}`;

const fichas: CartaExpansao[] = [
  ficha(OVO_ILUMINADO, 'Ovo Iluminado', 'mistica', [], undefined, 2, { subtipos: ['Ficha', 'Ovo'], concedeAoSubstituto: { ataque: 1, vida: 1 } }, '034'),
  ficha(OVO_ETEREO, 'Ovo Etéreo', 'mistica', [], undefined, 1, { subtipos: ['Ficha', 'Ovo'], concedeAoSubstituto: { palavras: ['Etéreo'] } }, '034'),
  ...DRAGOES.map((d) => ficha(idDragao(d.v), d.nome, 'aliado', [d.v], d.ataque, d.vida, { subtipos: ['Ficha', 'Dragão'], entraSobre: 'Ovo', ...d.extra }, d.arte)),
];

// ---------- cartas ----------
function aliado(arte: string, slug: string, nome: string, valor: string, custo: number, ataque: number, vida: number, extra: Partial<CartaDef> = {}): CartaExpansao {
  return { ...molde, id: `md-${slug}`, nome, tipo: 'aliado', subtipos: ['Monstro'], valores: [valor], custo, grau: grau(custo), ataque, vida, ...extra, arteExpansao: arte };
}
function temporaria(arte: string, slug: string, nome: string, valores: string[], custo: number, efeitos: Efeito[], extra: Partial<CartaDef> = {}): CartaExpansao {
  return { ...molde, id: `md-${slug}`, nome, tipo: 'temporaria', subtipos: ['Magia Dracônica'], valores, custo, grau: grau(custo), efeitos, ...extra, arteExpansao: arte };
}
function permanente(tipo: 'estrutura' | 'mistica', arte: string, slug: string, nome: string, valor: string, custo: number, vida: number, extra: Partial<CartaDef> = {}): CartaExpansao {
  return { ...molde, id: `md-${slug}`, nome, tipo, valores: [valor], custo, grau: grau(custo), vida, ...extra, arteExpansao: arte };
}
const ativ = (custo: number, efeitos: Efeito[], suspende = true) => ({ ativacao: { custo, suspende, efeitos } });
const hab = (h: Habilidade) => ({ habilidades: [h] });
function item(arte: string, slug: string, nome: string, categoriaItem: CategoriaItem, valores: string[], campos: Partial<CartaDef>): CartaExpansao {
  return {
    ...molde, arcanoGerado: 0, id: `md-item-${slug}`, nome, tipo: 'item', categoriaItem, subtipos: [categoriaItem === 'armadura' ? 'Proteção' : 'Equipamento'], valores,
    custo: 0, grau: 0, ...(categoriaItem === 'canal' ? { tipoCanal: 'arma' as const, subtipos: ['Arma'] } : {}), ...campos, arteExpansao: arte,
  };
}
/** Uso de item (itens v2): suspende o Conjurador e paga o custo. */
const usar = (custo: number, efeitos: Efeito[], extra: Partial<NonNullable<CartaDef['ativacao']>> = {}) => ({ custo, ativacao: { custo, suspende: false, suspendeConjurador: true, efeitos, ...extra } });
const competencia = (c: NonNullable<CartaDef['competencia']>) => ({ competencia: c });

const cartas: CartaExpansao[] = [
  // ---------- monstros (Aliados) ----------
  aliado('050', 'boca-da-cisterna', 'Boca da Cisterna', M, 5, 5, 8, hab({ quando: 'cemiterio', efeitos: [ovoEtereo] })),
  aliado('051', 'rastejante-de-vidro-fosco', 'Rastejante de Vidro Fosco', S, 2, 1, 1, { palavrasChave: ['Etéreo'], aoEntrar: [ovoEtereo] }),
  aliado('052', 'vespa-de-sino', 'Vespa de Sino', C, 3, 3, 2, { palavrasChave: ['Proficiente', 'Etéreo'] }),
  aliado('053', 'caramujo-de-basalto', 'Caramujo de Basalto', E, 4, 2, 7, { aoEntrar: [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }] }),
  aliado('054', 'mandibula-de-juncos', 'Mandíbula de Juncos', R, 3, 5, 4, { subtipos: ['Monstro', 'Dracônico'], entraSobre: 'Ovo', palavrasChave: ['Proficiente'] }),
  aliado('055', 'gigante-dos-degraus', 'Gigante dos Degraus', D, 6, 7, 11, { protegido: 1, ...hab({ quando: 'cemiterio', efeitos: [ovoIluminado] }) }),
  aliado('056', 'peixe-de-telhado', 'Peixe de Telhado', A, 4, 3, 4, { palavrasChave: ['Etéreo'], aoDanoConjurador: [{ tipo: 'arcano', valor: 1 }] }),
  aliado('057', 'coletor-de-ovos', 'Coletor de Ovos', M, 3, 3, 3, { aoEntrar: [ovoIluminado] }),
  aliado('058', 'mariposa-do-breu', 'Mariposa do Breu', A, 2, 1, 1, { palavrasChave: ['Etéreo'], aoEntrar: [{ tipo: 'arcano', valor: 1 }] }),
  aliado('059', 'manta-de-pedreira', 'Manta de Pedreira', C, 5, 6, 7, { subtipos: ['Monstro', 'Dracônico'], entraSobre: 'Ovo', aoEntrar: [{ tipo: 'dano', valor: 2, alvo: 'escolhido' }] }),
  aliado('060', 'ninho-andarilho', 'Ninho Andarilho', R, 4, 1, 5, { aoEntrar: [ovoIluminado], ...hab({ quando: 'emCampo', modo: 'recorrente', etapa: 'inicio', turnoDe: 'seu', efeitos: [ovoEtereo] }) }),
  aliado('061', 'devorador-de-ecos', 'Devorador de Ecos', M, 7, 9, 10, { aoEntrar: [{ tipo: 'descartar', quem: 'oponente', valor: 1, modo: 'dono' }] }),

  // ---------- magias dracônicas (Temporárias) ----------
  temporaria('062', 'asas-sobre-a-inundacao', 'Asas sobre a Inundação', [S], 3, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }, ovoEtereo], rep),
  temporaria('063', 'muralha-de-escamas', 'Muralha de Escamas', [C], 2, [{ tipo: 'campo', acao: 'aumentarVida', valor: 2, alcance: 'todos', lado: 'proprio', tipos: ['aliado'], duracao: 'turno' }, ovoIluminado], rep),
  temporaria('064', 'a-escama-que-floresce', 'A Escama que Floresce', [R], 1, [{ tipo: 'curar', valor: 2, alvo: 'escolhido' }, ovoIluminado]),
  temporaria('065', 'memoria-da-primeira-asa', 'Memória da Primeira Asa', [G], 3, [{ tipo: 'olharTopo', quantidade: 3, pegar: 1 }, ovoEtereo]),
  // Ciantia articula todos os Valores: a carta de 9 Valores serve a todo Conjurador (decisão 37)
  temporaria('066', 'nove-correntes-de-um-ceu', 'Nove Correntes de um Céu', [C, S, D, P, A, M, G, E, R], 6, [{ tipo: 'danoTodosAliados', valor: 2 }, ovoIluminado, ovoEtereo]),
  temporaria('067', 'suspensao-do-desabamento', 'Suspensão do Desabamento', [E], 3, [{ tipo: 'campo', acao: 'naoAtaca', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }, { tipo: 'comprar', valor: 1 }], rep),
  temporaria('068', 'corrente-desfeita', 'Corrente Desfeita', [D], 4, [{ tipo: 'anular', alvo: 'pilha', roubar: true }], rep),
  temporaria('069', 'o-vinculo-que-atravessa', 'O Vínculo que Atravessa', [P], 2, [
    { tipo: 'campo', acao: 'aumentarAtaque', valor: 2, alcance: 'alvo', lado: 'proprio', tipos: ['aliado'], duracao: 'turno' },
    { tipo: 'campo', acao: 'conceder', alcance: 'alvo', lado: 'proprio', tipos: ['aliado'], concessao: { palavras: ['Etéreo'], palavrasTurnos: 1 } },
    ovoEtereo]),

  // ---------- equipamentos (um por Comprometimento, mais genéricos) ----------
  item('070', 'lanca-de-escama', 'Lança de Escama', 'canal', [], { ...competencia('Brigão'), ataque: 2, custo: 1, grau: 2, aoAtacar: { danoUnidades: 1 } }),
  item('071', 'manto-de-escamas-mudas', 'Manto de Escamas Mudas', 'armadura', [], { defesa: 1, vidaDesgaste: 5, palavrasChave: ['Etéreo'] }),
  item('072', 'lentes-do-ovo', 'Lentes do Ovo', 'canal', [G], { tipoCanal: 'transcritor', subtipos: ['Transcritor'], grau: 1, ...usar(1, [{ tipo: 'olharTopo', quantidade: 3, pegar: 1 }]) }),
  item('073', 'incubadora-portatil', 'Incubadora Portátil', 'canal', [], { ...competencia('Invocador'), tipoCanal: 'foco', subtipos: ['Foco Arcano'], grau: 1, ...usar(2, [ovoIluminado]) }),
  item('074', 'diapasao-da-primeira-asa', 'Diapasão da Primeira Asa', 'canal', [], { ...competencia('Preparado'), tipoCanal: 'transcritor', subtipos: ['Transcritor'], grau: 2, ...usar(0, [{ tipo: 'arcano', valor: 1 }]) }),
  item('075', 'escudo-da-muda', 'Escudo da Muda', 'armadura', [], { ...competencia('Parrudão da quebrada'), defesa: 1, vidaDesgaste: 6, grau: 2, aoProteger: [ovoEtereo] }),
  item('076', 'luvas-de-contencao', 'Luvas de Contenção', 'extra', [], { grau: 1, ...usar(1, [{ tipo: 'campo', acao: 'suspender', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }], { consome: true }) }),
  item('077', 'compasso-de-rotas-draconicas', 'Compasso de Rotas Dracônicas', 'canal', [], { ...competencia('Habilidoso'), tipoCanal: 'transcritor', subtipos: ['Transcritor'], grau: 2, ...usar(1, [{ tipo: 'olharTopoPara', quantidade: 3, pegar: 1, destino: 'grau', resto: 'fundo' }]) }),
  // Durandis, o Receptáculo Dracono (arma Extra pedida pelo autor; sem arte ainda): um Dragão por Valor
  {
    ...molde, arcanoGerado: 0, id: 'md-item-durandis', nome: 'Durandis, o Receptáculo Dracono', tipo: 'item', categoriaItem: 'extra', subtipos: ['Equipamento', 'Receptáculo'], valores: [], custo: 0, grau: 0,
    habilidades: DRAGOES.map((d) => ({
      quando: 'ativada' as const, custo: { suspender: true, arcano: d.x }, requisito: { grau: d.x, valor: d.v },
      efeitos: [{ tipo: 'invocar' as const, carta: idDragao(d.v), nome: `o ${d.nome} (${d.ataque}/${d.vida})`, sobre: 'Ovo' }],
    })),
  },

  // ---------- Estruturas ----------
  permanente('estrutura', '078', 'viveiro-entre-os-morros', 'Viveiro entre os Morros', R, 3, 5, ativ(1, [ovoIluminado])),
  permanente('estrutura', '079', 'tribunal-das-asas', 'Tribunal das Asas', P, 4, 5, ativ(1, [{ tipo: 'campo', acao: 'naoAtaca', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }])),
  permanente('estrutura', '080', 'torre-de-vigilia-antidraconica', 'Torre de Vigília Antidracônica', C, 3, 4, ativ(0, [{ tipo: 'dano', valor: 1, alvo: 'escolhidoAliado' }])),
  permanente('estrutura', '081', 'estufa-das-cascas', 'Estufa das Cascas', A, 4, 3, { grauExtra: 1, ...ativ(1, [ovoEtereo]) }),
  permanente('estrutura', '082', 'arquivo-das-catastrofes', 'Arquivo das Catástrofes', G, 3, 4, ativ(1, [{ tipo: 'comprar', valor: 1 }])),
  // arte 049 (043-v2, o dragão dormindo na forja): segunda carta de Alvorum pedida pelo autor
  permanente('estrutura', '049', 'repouso-na-forja', 'Repouso na Forja', A, 4, 5, ativ(4, [{ tipo: 'invocar', carta: idDragao(A), nome: 'o Dragão de Carvão e Ouro (4/6)', sobre: 'Ovo' }])),
  permanente('estrutura', '083', 'santuario-da-muda', 'Santuário da Muda', S, 2, 2, ativ(2, [{ tipo: 'devolver', alvo: 'escolhidoProprio' }])),

  // ---------- Místicas ----------
  permanente('mistica', '084', 'pacto-das-cascas-vazias', 'Pacto das Cascas Vazias', P, 3, 3, hab({ quando: 'emCampo', modo: 'passivo', aura: { lado: 'proprio', tipos: ['aliado'], ataque: 1 }, efeitos: [] })),
  permanente('mistica', '085', 'silencio-sobre-o-viveiro', 'Silêncio sobre o Viveiro', G, 2, 2, hab({ quando: 'emCampo', modo: 'passivo', custoCartas: { de: 'oponente', tipos: ['mistica', 'estrutura'], valor: 1 }, efeitos: [] })),
  permanente('mistica', '086', 'sombra-que-guarda-o-ovo', 'Sombra que Guarda o Ovo', A, 3, 3, { aoEntrar: [ovoIluminado], ...ativ(1, [{ tipo: 'curar', valor: 2, alvo: 'escolhido' }]) }),
  permanente('mistica', '087', 'estacao-que-nao-termina', 'Estação que Não Termina', E, 4, 4, { aoEntrar: [ovoEtereo], ...ativ(3, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }]) }),
  permanente('mistica', '088', 'vigilia-das-escamas-mortas', 'Vigília das Escamas Mortas', M, 2, 2, ativ(1, [{ tipo: 'curar', valor: 3, alvo: 'escolhidoAliado' }])),
  permanente('mistica', '089', 'fronteira-de-escamas', 'Fronteira de Escamas', D, 3, 4, hab({ quando: 'emCampo', modo: 'passivo', aura: { lado: 'inimigo', tipos: ['aliado'], ataque: -1 }, efeitos: [] })),
];

/** As cartas e fichas da expansão, com o texto gerado da mecânica (app/textos.js). */
export const cartasExpansaoDragoes: CartaExpansao[] = [...fichas, ...cartas].map((c) => ({ ...c, texto: textoCarta(c) }));

// ---------- Conjuradores (25 personagens, 090–114, mais Ciantia 001 e Valiant 030) ----------
const padrao = { vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0 } as const;
type ConjExpansao = ConjuradorDef & { arteExpansao: string };
function conj(arte: string, id: string, nome: string, valores: [string, string], comprometimento: ConjuradorDef['comprometimento'], poder: PoderConjurador): ConjExpansao {
  return { ...padrao, id: `md-${id}`, nome, valores, comprometimento, habilidade: textoPoder(poder), poder, expansao: EXPANSAO_DRAGOES, arteExpansao: arte };
}
const suspPague = (arcano: number, efeitos: Efeito[], extra: Partial<NonNullable<PoderConjurador['ativacao']>> = {}) => ({ ativacao: { custo: arcano, suspende: true, efeitos, ...extra } });

export const conjuradoresExpansaoDragoes: ConjExpansao[] = [
  conj('090', 'iria', 'Iria, Forjadora de Marés', [C, S], 'Brigão', { habilidades: [{ quando: 'atacar', umaVezPorTurno: true, efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  conj('091', 'serena', 'Serena, Voz das Barricadas', [C, P], 'Parrudão da quebrada', { habilidades: [{ quando: 'emCampo', modo: 'passivo', aura: { lado: 'proprio', tipos: ['aliado'], vida: 1 }, efeitos: [] }] }),
  conj('092', 'darian', 'Darian, Vigia do Eclipse', [C, A], 'Parrudão da quebrada', suspPague(1, [{ tipo: 'dano', valor: 1, alvo: 'escolhidoAliado' }])),
  conj('093', 'maira', 'Maira, Guardiã dos Ausentes', [C, M], 'Brigão', { gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'dano', valor: 1, alvo: 'conjuradorInimigo' }] } }),
  conj('094', 'orven', 'Orven, Estrategista das Asas', [C, G], 'Preparado', suspPague(2, [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }])),
  conj('095', 'hestia', 'Hestia, Duelista do Instante', [C, E], 'Brigão', { primeiraRepentinaMaisBarata: 1 }),
  conj('096', 'teren', 'Teren, Carcereiro de Mercúrio', [S, D], 'Habilidoso', suspPague(2, [{ tipo: 'campo', acao: 'naoAtaca', alcance: 'alvo', lado: 'inimigo', tipos: ['aliado'] }])),
  conj('097', 'nalia', 'Nália, Alquimista do Luto', [S, M], 'Preparado', { gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'arcano', valor: 1 }] } }),
  conj('098', 'caldris', 'Caldris, Pesquisador das Cascas', [S, G], 'Invocador', suspPague(2, [ovoEtereo])),
  conj('099', 'elda', 'Elda, Restauradora de Instantes', [S, E], 'Habilidoso', suspPague(3, [{ tipo: 'devolver', alvo: 'escolhidoAliado' }], { repentina: true })),
  conj('100', 'vesna', 'Vesna, Tecelã do Mangue', [S, R], 'Invocador', { gatilho: { quando: 'aliadoEntra', efeitos: [{ tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }] } }),
  conj('101', 'amara', 'Amara, Porta-Voz do Limite', [D, P], 'Parrudão da quebrada', { habilidades: [{ quando: 'emCampo', modo: 'passivo', custoCartas: { de: 'oponente', tipos: ['temporaria'], valor: 1 }, efeitos: [] }] }),
  conj('102', 'rudek', 'Rudek, Guardião do Pórtico', [D, A], 'Parrudão da quebrada', { habilidades: [{ quando: 'bloquear', umaVezPorTurno: true, efeitos: [{ tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }] }] }),
  conj('103', 'saira', 'Sáira, Custódia das Memórias', [D, M], 'Preparado', suspPague(1, [{ tipo: 'reanimar', custoExtra: 0, pagamento: 'custo', de: 'seu', custoMax: 3 }])),
  conj('104', 'brann', 'Brann, Guarda da Reserva', [D, R], 'Parrudão da quebrada', suspPague(1, [{ tipo: 'campo', acao: 'aumentarVida', valor: 2, alcance: 'alvo', lado: 'proprio', tipos: ['aliado'], duracao: 'permanente' }])),
  conj('105', 'liora', 'Liora, Mediadora do Crepúsculo', [P, A], 'Habilidoso', suspPague(1, [{ tipo: 'curar', valor: 3, alvo: 'escolhido' }])),
  conj('106', 'iven', 'Iven, Professor de Vínculos', [P, G], 'Preparado', suspPague(2, [{ tipo: 'comprar', valor: 1 }])),
  conj('107', 'naima', 'Naíma, Criadora dos Filhotes', [P, R], 'Invocador', { gatilho: { quando: 'aliadoEntra', efeitos: [ovoEtereo], soNoSeuTurno: true } }),
  conj('108', 'odran', 'Odran, Vigia dos Vestígios', [A, M], 'Preparado', { gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'curar', valor: 2, alvo: 'conjuradorProprio' }] } }),
  conj('109', 'talia', 'Tália, Leitura do Eclipse', [A, G], 'Habilidoso', suspPague(0, [{ tipo: 'olharTopoPara', quantidade: 3, pegar: 1, destino: 'topo', resto: 'fundo' }])),
  conj('110', 'erian', 'Erian, Guardião do Ocaso', [A, E], 'Habilidoso', { habilidades: [{ quando: 'cataclismaOponente', umaVezPorTurno: true, efeitos: [{ tipo: 'arcano', valor: 1 }] }] }),
  conj('111', 'tessa', 'Tessa, Cronista das Almas', [M, E], 'Preparado', { gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }] } }),
  conj('112', 'mauro', 'Mauro, Cuidador do Ciclo', [M, R], 'Invocador', { gatilho: { quando: 'aliadoMorre', efeitos: [ovoIluminado] } }),
  conj('113', 'adira', 'Adira, Escutadora de Ninhos', [G, R], 'Invocador', { gatilho: { quando: 'aliadoEntra', efeitos: [{ tipo: 'olharTopo', quantidade: 2, pegar: 1 }], soNoSeuTurno: true } }),
  // os 2 pares que faltavam: Ciantia (Persona + Eternyr) e Valiant (Dominum + Cognitium), da história do autor
  conj('001', 'ciantia', 'Ciantia, a Mestra dos Dragões', [P, E], 'Invocador', suspPague(2, [ovoIluminado])),
  conj('030', 'valiant', 'Valiant, a Palavra que Responde', [D, G], 'Preparado', suspPague(5, [{ tipo: 'anular', alvo: 'pilha', roubar: true }], { repentina: true })),
  conj('114', 'selmo', 'Selmo, Jardineiro das Eras', [E, R], 'Habilidoso', suspPague(2, [{ tipo: 'colocarNoGrau', de: 'topo', valor: 1 }])),
];

/** Arte de cada carta/ficha/Conjurador da expansão: id → número no manifesto da expansão (Conjurador: "conjurador-<id>"). */
export function artesDaExpansao(): Record<string, string> {
  const r: Record<string, string> = {};
  for (const c of cartasExpansaoDragoes) if (c.arteExpansao) r[c.id] = c.arteExpansao;
  for (const c of conjuradoresExpansaoDragoes) r['conjurador-' + c.id] = c.arteExpansao;
  return r;
}
