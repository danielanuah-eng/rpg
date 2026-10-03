// Gera o pedido de arte de TODAS as cartas do site para o GPT (Codex): Arcanoclisma/cartas/pedidos/lote-site-v1.claude.json.
// Pedido do autor (03/10/2026): toda carta (Conjurador, feitiço, equipamento, Aliado, Estrutura, Mística, item) tem
// CENÁRIO DE FUNDO e AMBIENTAÇÃO. A imagem recortada do RPG (catálogo aprovado) é só referência de identidade do
// personagem ou objeto; a arte da carta é uma cena nova, inteira, com fundo.
// Uso: node online/pedidos-visuais/gerar-lote-cartas.ts   (depois de node online/construir.ts)
import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const aqui = (p: string) => fileURLToPath(new URL(p, import.meta.url));
const raiz = aqui('../../');
type Ef = { tipo: string; valor?: number; alvo?: string; ambos?: boolean; quantidade?: number; pegar?: number };
type Carta = {
  id: string; nome: string; tipo: string; subtipos: string[]; valores: string[]; custo: number; grau: number; arcanoGerado: number; velocidade: string;
  ataque?: number; vida?: number; defesa?: number; vidaDesgaste?: number; categoriaItem?: string; texto: string; palavrasChave?: string[];
  efeitos?: Ef[]; aoEntrar?: Ef[]; aoDanoConjurador?: Ef[]; ativacao?: { efeitos: Ef[]; suspende: boolean; custo: number }; protegido?: number; descartaGrau?: number; custoValores?: string[];
};
const D = JSON.parse(readFileSync(raiz + 'online/public/jogo/dados.json', 'utf8'));
const CAT = JSON.parse(readFileSync(raiz + 'cartas/catalogo-artes.json', 'utf8')).assets as { numero: number; id: string; titulo: string; arquivo: string; registro: string }[];
const ARTE = new Map(CAT.map((a) => [a.id, a]));
const secoes = (() => {
  const md = readFileSync(raiz + 'arte/descricoes_das_imagens.md', 'utf8').split('\n');
  const m = new Map<number, string>(); let s = '';
  for (const l of md) { if (l.startsWith('## ')) s = l.slice(3).replace(/\s*\(\d+\)\s*$/, ''); const x = l.match(/^### (\d+) · /); if (x) m.set(Number(x[1]), s); }
  return m;
})();
const promptOriginal = (arteId: string): string => {
  const a = ARTE.get(arteId);
  if (!a) return '';
  try { return JSON.parse(readFileSync(raiz + '../' + a.registro, 'utf8')).prompt ?? ''; } catch { return ''; }
};

// ---------- Valores: cor, forma da magia (arte/27 Estilo de arte) e lugar ----------
const VALOR: Record<string, { nome: string; cor: string; forma: string; lugar: string; temperatura: 'quente' | 'fria' }> = {
  conflituo: { nome: 'Conflitüo', cor: 'vermelho-ferrugem', forma: 'construtos sólidos de luz: lâminas, escudos, estandartes', lugar: 'campos de batalha sem trégua, arenas, trincheiras e forjas de guerra', temperatura: 'quente' },
  sublimeyn: { nome: 'Sublimeyn', cor: 'prata-mercúrio', forma: 'fumaça e vapor que escorrem, pingam e cristalizam', lugar: 'céus de duas pálpebras, mares de mercúrio, nuvens e torres flutuantes', temperatura: 'fria' },
  dominum: { nome: 'Dominum', cor: 'roxo-sangue', forma: 'construtos de correntes, coleiras e elos', lugar: 'cidades-muralha, prisões, tronos e salões de juramento', temperatura: 'fria' },
  persona: { nome: 'Persona', cor: 'coral', forma: 'aura suave e uniforme que transborda quando a emoção é forte', lugar: 'a cidade dos rostos, feiras de máscaras, ruas de espelhos e tavernas cheias', temperatura: 'quente' },
  alvorum: { nome: 'Alvorum', cor: 'dourado e preto', forma: 'aura dividida, com falhas entre a parte de luz e a de sombra', lugar: 'faróis sem noite, auroras, campos dourados ao amanhecer e ao entardecer', temperatura: 'quente' },
  morbidyum: { nome: 'Morbidyum', cor: 'verde-pálido', forma: 'fumaça fria e fios que partem, como almas', lugar: 'cemitérios que respiram, brejos, criptas e aldeias de curandeiras', temperatura: 'fria' },
  cognitium: { nome: 'Cognitium', cor: 'índigo', forma: 'véu de fumaça com falhas, como se faltassem pedaços', lugar: 'o arquivo sem dono, bibliotecas que esquecem, observatórios', temperatura: 'fria' },
  eternyr: { nome: 'Eternyr', cor: 'azul-noite com riscos brancos', forma: 'aura com falhas e rastros parados no ar', lugar: 'o entretempo, vilas do mesmo dia, estações paradas e relógios gigantes', temperatura: 'fria' },
  primitivum: { nome: 'Primitivum', cor: 'verde-musgo e âmbar', forma: 'construtos orgânicos: raízes, galhos e esporos soltos', lugar: 'a floresta primeira, florestas que andam, mercados de feras e montanhas', temperatura: 'quente' },
};
const NEUTRO = 'uma encruzilhada arcana devastada pelo Cataclisma, com ruínas, céu dividido entre luz quente e sombra fria e restos de magia no chão';

// ---------- estilo por seção do catálogo de artes ----------
function estiloDe(arteId: string | null, c: Carta | null): 'P' | 'M' | 'D' | 'I' | 'C' {
  if (!arteId) return c?.tipo === 'item' ? 'I' : 'P';
  const s = secoes.get(ARTE.get(arteId)!.numero) ?? '';
  if (/Dragões/.test(s)) return 'D';
  if (/Terrenos|Lugares|Reinos/.test(s)) return 'C';
  if (/Armas|Armaduras|Relíquias|Consumíveis|Materiais|Itens mágicos|Maravilhas|Artefatos/.test(s)) return 'I';
  if (/Feras|Disruptos|Seres de energia|Seres dos planos|Semideuses|Montarias/.test(s)) return 'M';
  return 'P';
}

// ---------- variação por carta (versão 2, pedido do autor em 03/10) ----------
// Prompts menos rígidos: cada carta recebe, de forma fixa pelo id (mesma escolha a cada geração), uma pose, uma
// perspectiva, um enquadramento, a direção do olhar, uma variação de acabamento e uma HORA DO DIA, com mais dia que
// noite ("todas as cartas parecem estar à noite"). Quando várias cartas retratam o mesmo evento (ex.: todas as de dano),
// cada uma se relaciona com ele de um jeito diferente.
const hashDe = (t: string): number => { let h = 2166136261; for (const ch of t) h = Math.imul(h ^ ch.charCodeAt(0), 16777619); return h >>> 0; };
const escolher = <T>(id: string, sal: string, lista: readonly T[]): T => lista[hashDe(sal + ':' + id) % lista.length];
const HORAS: [string, number][] = [
  ['amanhecer: luz rosada e dourada vinda de baixo, névoa clara, céu limpo', 2],
  ['manhã clara: sol alto e fresco, sombras curtas, céu azul com poucas nuvens', 3],
  ['meio-dia: luz forte e branca, cores vivas e saturadas, sombras duras', 3],
  ['tarde ensolarada: luz dourada e quente entrando de lado, poeira brilhando no ar', 3],
  ['dia nublado e luminoso: luz difusa e clara, sem sombras fortes, cores suaves', 2],
  ['depois da chuva: chão molhado refletindo o céu claro, arco-íris discreto ou gotas brilhando', 1],
  ['tempestade de dia: céu carregado, relâmpagos e vento, mas a cena continua legível e clara', 1],
  ['entardecer: céu laranja e violeta, contraluz, silhuetas recortadas', 2],
  ['noite clara: lua cheia e céu estrelado, luz azulada suave (a magia ilumina a cena)', 1],
];
function horaDe(id: string): string {
  let r = hashDe('hora:' + id) % HORAS.reduce((t, [, p]) => t + p, 0);
  for (const [h, p] of HORAS) { if (r < p) return h; r -= p; }
  return HORAS[0][0];
}
const POSES = ['em movimento, no meio de um passo ou salto', 'parado e firme, peso apoiado numa perna', 'agachado ou ajoelhado, pronto para agir', 'girando o corpo, com roupa e cabelo em movimento', 'inclinado para a frente, investindo', 'recuando um passo, surpreso ou em defesa', 'de costas parcialmente, virando o rosto', 'sentado em cima de algo do cenário, à vontade', 'braços abertos, conduzindo a magia', 'em pleno esforço físico, músculos e postura tensos'];
const PERSPECTIVAS = ['câmera baixa, de baixo para cima (o assunto parece grande)', 'câmera alta, de cima para baixo (mostra o chão e o entorno)', 'altura dos olhos, frontal', 'três quartos, levemente de lado', 'de perfil', 'por cima do ombro de outra figura em primeiro plano', 'plano aberto e distante, a figura pequena no cenário grande', 'plano próximo e dramático, cenário visível só atrás e dos lados', 'ângulo inclinado (holandês), sensação de ação'];
const ENQUADRAMENTOS = ['assunto no terço esquerdo, espaço do cenário à direita', 'assunto no terço direito, espaço do cenário à esquerda', 'assunto centralizado e baixo, céu ou teto amplo em cima', 'assunto alto na imagem, chão e caminho em primeiro plano', 'assunto em diagonal, do canto inferior ao superior', 'primeiro plano com elementos do cenário emoldurando o assunto ao fundo'];
const OLHARES = ['rosto virado para a esquerda', 'rosto virado para a direita', 'olhando direto para quem vê a carta', 'olhando para cima', 'olhando para baixo, para a própria magia ou mão', 'rosto parcialmente encoberto (capuz, sombra, cabelo ou máscara)', 'olhando por cima do ombro, para trás'];
const ACABAMENTOS = ['pintura digital com pinceladas visíveis', 'ilustração de livro de fábulas, contornos suaves', 'guache com cores chapadas e luz recortada', 'aquarela com tinta escura nos contornos', 'pintura a óleo com textura de tela', 'gravura iluminada à mão, linhas finas e cores vivas', 'ilustração com contorno forte e sombras gráficas', 'arte conceitual pintada, atmosférica'];
/** Como a cena se relaciona com o evento da mecânica (para não repetir a mesma imagem em todas as cartas de dano, cura...). */
const RELACOES = ['mostrar quem CAUSA o efeito, no instante em que ele sai', 'mostrar quem SOFRE o efeito, no instante do impacto', 'mostrar o instante ANTES: a magia se formando, a tensão', 'mostrar o instante DEPOIS: o rastro, as marcas e as consequências no lugar', 'mostrar o efeito visto de longe por testemunhas ou curiosos no cenário', 'mostrar o efeito através de um detalhe próximo (mãos, objeto, chão) com a cena maior atrás', 'mostrar o efeito como parte de uma batalha maior, com outras figuras ao fundo'];
const variacaoDe = (id: string, figura: boolean) => ({
  horaDoDia: horaDe(id),
  ...(figura ? { pose: escolher(id, 'pose', POSES), olhar: escolher(id, 'olhar', OLHARES) } : {}),
  perspectiva: escolher(id, 'persp', PERSPECTIVAS),
  enquadramento: escolher(id, 'enq', ENQUADRAMENTOS),
  acabamento: escolher(id, 'acab', ACABAMENTOS) + ' (dentro do traço do Arcanoclisma, arte/27)',
  relacaoComOEvento: escolher(id, 'rel', RELACOES),
});

// ---------- o momento retratado, a partir da mecânica ----------
const alvoTxt = (a?: string) => ({ escolhido: 'um inimigo (criatura ou conjurador rival)', escolhidoAliado: 'uma criatura inimiga', escolhidoProprio: 'um aliado', conjuradorInimigo: 'o conjurador rival, ao longe', conjuradorProprio: 'quem conjura' } as Record<string, string>)[a ?? ''] ?? 'o alvo';
/** Formas diferentes de descrever o mesmo evento; cada carta pega uma (fixa pelo id). */
const VARIANTES_EVENTO: Record<string, ((ef: Ef) => string)[]> = {
  dano: [
    (ef) => `a magia atinge ${alvoTxt(ef.alvo)} com um impacto ${ef.valor && ef.valor >= 5 ? 'devastador' : 'certeiro'}`,
    (ef) => `um projétil de magia corta o ar em direção a ${alvoTxt(ef.alvo)}${ef.valor && ef.valor >= 5 ? ', grande o bastante para rachar o chão' : ''}`,
    (ef) => `${alvoTxt(ef.alvo)} é arremessado para trás pela explosão, com estilhaços e poeira`,
    (ef) => `a magia estoura na guarda de ${alvoTxt(ef.alvo)}, que quase cede`,
    () => 'o chão onde o inimigo estava ainda queima ou brilha, marcado pelo golpe que acabou de cair',
    () => 'quem conjura mira com calma e solta a magia, que ainda está a caminho do alvo',
  ],
  curar: [
    (ef) => `uma luz de cura fecha feridas de ${alvoTxt(ef.alvo)}`,
    () => 'ataduras de luz se enrolam sozinhas num ferido que volta a ficar de pé',
    () => 'uma fonte, flor ou chuva mágica devolve as forças a quem estava caído',
  ],
  comprar: [
    () => 'visões, páginas ou cartas de luz se abrem diante de quem conjura, revelando o que vem',
    () => 'um mensageiro, pássaro ou vento traz pergaminhos e segredos até as mãos de quem conjura',
    () => 'livros se abrem sozinhos numa estante e as letras saltam no ar',
  ],
  destruir: [
    () => 'uma criatura inimiga é desfeita por completo pela magia',
    () => 'uma criatura inimiga vira pó, cinza ou cristal, partindo-se em pedaços',
    () => 'uma criatura inimiga é engolida por uma fenda ou sombra que se fecha',
  ],
  anular: [
    () => 'um feitiço inimigo é fechado, cortado ou apagado no ar antes de terminar',
    () => 'a magia inimiga bate numa parede invisível e se desfaz em faíscas',
    () => 'as runas de um feitiço rival se embaralham e caem no chão, sem efeito',
  ],
};
let idAtual = '';
function momentoDoEfeito(ef: Ef): string {
  const vs = VARIANTES_EVENTO[ef.tipo];
  if (vs) return escolher(idAtual, 'ev-' + ef.tipo, vs)(ef);
  switch (ef.tipo) {
    case 'dano': return `a magia atinge ${alvoTxt(ef.alvo)} com um impacto ${ef.valor && ef.valor >= 5 ? 'devastador' : 'certeiro'}`;
    case 'danoTodosAliados': return `uma onda de magia varre o campo inimigo e atinge várias criaturas ao mesmo tempo${ef.ambos ? ', sem poupar as próprias' : ''}`;
    case 'destruirTodosAliados': return `um fim de mundo em miniatura: todas as criaturas ${ef.ambos ? 'dos dois lados' : 'do lado inimigo'} se desfazem`;
    case 'destruir': return 'uma criatura inimiga é desfeita por completo pela magia';
    case 'curar': return `uma luz de cura fecha feridas de ${alvoTxt(ef.alvo)}`;
    case 'comprar': return 'visões, páginas ou cartas de luz se abrem diante de quem conjura, revelando o que vem';
    case 'olharTopo': return 'quem conjura espia o futuro: algumas imagens brilham e uma é puxada para a mão, as outras afundam';
    case 'arcano': return 'Arcano bruto jorra e se acumula em esferas de luz na cor do Valor';
    case 'arcanoOponente': return 'parte da magia escapa e escorre para o lado do rival, como um preço pago';
    case 'devolver': return 'uma criatura é puxada de volta através de um portal, sumindo do campo';
    case 'anular': return 'um feitiço inimigo é fechado, cortado ou apagado no ar antes de terminar';
    case 'reanimar': return 'um aliado caído se levanta do cemitério, com fios de alma voltando ao corpo';
    case 'banirDaMao': return 'um objeto ou carta é selado num instante parado do tempo, guardado para depois';
    case 'permitirAnularDoCemiterio': return 'feitiços já usados se reacendem nas sepulturas, prontos para responder de novo';
    case 'descontoProximaCarta': return 'um atalho mágico se abre, encurtando o caminho do próximo feitiço';
    default: return 'a magia acontece';
  }
}
function cenaDe(c: Carta, assunto: string, estilo: string): string {
  const efs = c.efeitos ?? [];
  if (c.tipo === 'temporaria') return `${assunto} como fonte ou forma do feitiço: ${efs.map(momentoDoEfeito).join('; ')}.`;
  if (c.tipo === 'aliado') {
    const partes: string[] = [];
    const atk = c.ataque ?? 0, vida = c.vida ?? 0;
    if (c.ativacao?.efeitos.every((e) => e.tipo === 'arcano')) partes.push(`${assunto} canaliza Arcano para os aliados, concentrado e sereno, com energia brotando das mãos ou do corpo`);
    else if (c.palavrasChave?.includes('Etéreo')) partes.push(`${assunto} em forma etérea, meio translúcido, flutuando acima do campo de batalha, fora do alcance de quem anda no chão`);
    else if (c.palavrasChave?.includes('Proficiente')) partes.push(`${assunto} chegando já em disparada, atacando no mesmo instante em que entra na batalha`);
    else if (atk >= vida) partes.push(`${assunto} avançando ao ataque com fúria, em pleno golpe`);
    else partes.push(`${assunto} em guarda firme, protegendo os aliados e segurando a linha`);
    if (c.aoEntrar?.length) partes.push(`ao chegar, ${c.aoEntrar.map(momentoDoEfeito).join(' e ')}`);
    if (c.aoDanoConjurador?.length) partes.push('rumo ao conjurador rival');
    if (c.protegido) partes.push('cercado por uma proteção que desvia magias inimigas');
    if (c.descartaGrau) partes.push('consumindo runas de Grau que se quebram ao seu redor para poder existir');
    if (c.custoValores?.length) partes.push(`vindo diretamente do plano de ${VALOR[c.custoValores[0]].nome}`);
    if (c.velocidade === 'repentina') partes.push('surgindo de repente, de surpresa, no meio da ação');
    return partes.join('; ') + '.';
  }
  if (c.tipo === 'estrutura') return `${assunto} como lugar vivo e em funcionamento${c.ativacao ? `: ${c.ativacao.efeitos.map(momentoDoEfeito).join(' e ')}` : ''}. Estrutura não ataca: transmite permanência e poder.`;
  if (c.tipo === 'mistica') return `${assunto} como presença mística pairando sobre o campo (aura, barreira ou ser divino)${c.ativacao ? `; ${c.ativacao.efeitos.map(momentoDoEfeito).join(' e ')}` : ''}${c.aoEntrar ? `; ao surgir, ${c.aoEntrar.map(momentoDoEfeito).join(' e ')}` : ''}.`;
  if (c.tipo === 'item') {
    if (c.categoriaItem === 'canal') return `${assunto} em uso como arma ou foco de magia (Canal, Ataque ${c.ataque}), empunhado por um conjurador visto de modo parcial (mão, braço ou silhueta), no instante de um ataque, num cenário de batalha.`;
    if (c.categoriaItem === 'armadura') return `${assunto} protegendo um conjurador que bloqueia um golpe (Armadura: Defesa ${c.defesa}, desgaste ${c.vidaDesgaste}), com marcas de uso e a magia de proteção visível, num cenário de batalha.`;
    return `${assunto} sendo usado uma única vez (Extra consumível): ${(c.ativacao?.efeitos ?? []).map(momentoDoEfeito).join(' e ')}, num lugar coerente com o item.`;
  }
  return assunto + '.';
}

const FUNDO_OBRIGATORIO = 'CENÁRIO DE FUNDO OBRIGATÓRIO: a carta é uma cena completa, com ambiente, profundidade e ambientação (chão, céu ou teto, construções ou natureza, clima, luz do momento). Nunca fundo branco, liso, cinza ou recortado; nunca o objeto ou personagem isolado. Iluminação difusa e discreta no fundo, brilho forte só focal na magia relevante (direção de cenários de 03/10). Retrato vertical 63:88, sem texto, números, molduras ou logotipo na arte; deixar o terço inferior menos carregado para a caixa de regras.';

const BLOCO_LENDARIO = 'Iluminação focal coerente, pontos de luz expressivos e reflexos dourados/iridescentes; realçar o brilho das magias presentes na cena, com núcleo luminoso e transições suaves para a cor do Valor. Textura gráfica fina e materialidade visível, preservando o traço, as cores e a leitura da figura. Brilho concentrado, sem apagar rosto, detalhes ou silhueta. Composição preparada para o acabamento holográfico dourado e arco-íris da carta.';

function ambiente(valores: string[]): string {
  if (!valores.length) return `Ambiente: ${NEUTRO}.`;
  const v = VALOR[valores[0]];
  const outros = valores.slice(1).map((x) => VALOR[x].nome);
  return `Ambiente do Valor ${v.nome}: ${v.lugar}. Magia na cor ${v.cor}, em ${v.forma}. Dupla temperatura: a magia é ${v.temperatura}, então o cenário puxa o ${v.temperatura === 'quente' ? 'frio' : 'quente'}.${outros.length ? ` A carta também serve aos Valores ${outros.join(', ')}: detalhes discretos dessas cores podem aparecer.` : ''}`;
}

// ---------- Conjuradores e itens sem arte de referência (descrições do Claude) ----------
const CONJURADORES: Record<string, { aparencia: string; cena: string }> = {
  lyandra: { aparencia: 'domadora de feras de guerra, corpo forte e marcado por cicatrizes, pele morena, cabelo trançado com dentes e penas, armadura de couro remendada com ombreira de metal, chicote-corrente de luz vermelho-ferrugem', cena: 'à frente de uma matilha de feras de guerra que avançam junto com ela num campo de batalha de Conflitüo e Primitivum, ela aponta o ataque e a primeira fera morde com força redobrada' },
  carmal: { aparencia: 'druida idoso e alto, pele escura, barba com musgo e flores pequenas, manto de folhas secas e couro, cajado de raiz viva com âmbar', cena: 'numa floresta primeira, criaturas brotam do chão a cada passo dele e o primeiro animal que chega deixa um rastro de Arcano dourado e âmbar' },
  ricardo: { aparencia: 'alquimista de meia-idade, magro, óculos de lentes múltiplas, avental de couro cheio de frascos, luvas de oficina, bigode farto', cena: 'num laboratório de torre aberta, ele destila uma gota de Arcano dourado e prateado que faz a próxima magia nascer mais barata, com frascos de mercúrio flutuando' },
  roh: { aparencia: 'ilusionista jovem e esguio, várias máscaras presas no cinto e no ombro, uma máscara meio coral meio prata no rosto, casaco de palco com bolsos fundos', cena: 'numa feira de máscaras, ele troca de rosto e puxa um aliado de volta pela sombra de uma máscara, cercado de reflexos e vapor prateado' },
  agnelo: { aparencia: 'duelista sombrio, alto, pele pálida, cabelo preso, sobretudo longo e luvas finas, florete fino com fios verde-pálidos, expressão calma e triste', cena: 'num cemitério de duelos, ele se curva sobre um aliado que tomba e recebe do fio de alma que sobe um lampejo de conhecimento (uma carta de luz coral e verde)' },
  diastaros: { aparencia: 'necromante vidente, idoso e corcunda, olhos cobertos por uma venda índigo bordada com olhos, mãos ossudas com anéis, manto de penas de corvo', cena: 'num cemitério que respira, ele ergue um morto de qualquer lado da batalha, puxando-o com fios índigo e verde-pálidos de uma sepultura rachada' },
  ana: { aparencia: 'cronista do tempo, mulher jovem de pele marrom, cabelos com fios brancos que flutuam parados, óculos redondos, caderno enorme preso por correntes, relógio de bolso no pulso', cena: 'no entretempo, ela escreve num livro e uma carta fica presa num instante parado (riscos brancos no ar), para ser usada depois, com engrenagens e páginas suspensas' },
  mitonho: { aparencia: 'carcereiro temporal, homem robusto e baixo, barba grisalha, molho de chaves-relógio no cinto, casaco de guarda com correntes roxas, lampião azul-noite', cena: 'numa prisão de relógios parados, ele destranca uma cela de onde sai um feitiço já usado, preso por correntes roxas, pronto para anular de novo' },
  osmar: { aparencia: 'capataz de ferro, homem enorme e largo, pele clara queimada de sol, capacete de operário, braço mecânico de bronze, colete de couro e correntes', cena: 'diante de uma muralha de cidade em obra, ele ergue escudos de correntes roxas e lâminas vermelho-ferrugem que protegem os seus aliados de qualquer magia mirada' },
};
const ITENS_SEM_ARTE: Record<string, string> = {
  'item-canal-cajado': 'cajado de aprendiz simples, de madeira clara com ponteira de Bronze Arco e runas que mal acendem, gasto de tanto treino',
  'item-canal-lamina': 'lâmina rúnica de Bronze Arco, espada curta com runas acesas ao longo do fio',
  'item-canal-lanca': 'lança do eclipse, enorme, maior que o dono, com lâmina em meia-lua escura e borda de luz dourada',
  'item-armadura-gibao': 'gibão de couro acolchoado e remendado, com placas pequenas de Bronze Arco nos ombros',
  'item-armadura-couraca': 'couraça de placas de Bronze Arco, pesada, com rebites e runas de proteção',
  'item-extra-pocao': 'poção de cura num frasco grosso de vidro com rolha de cera, líquido vermelho-coral que brilha',
  'item-extra-elixir': 'elixir de foco num frasco fino e alto, líquido azul com pontos de luz que sobem',
};

function entradaCarta(id: string, c: Carta) {
  idAtual = id;
  const vis = D.visual[id] ?? {};
  const arteRef: string | null = vis.arte ?? null;
  const estilo = estiloDe(arteRef, c);
  const tituloRef = arteRef ? ARTE.get(arteRef)!.titulo : c.nome;
  const assunto = arteRef ? tituloRef : ITENS_SEM_ARTE[id] ?? c.nome;
  const raridade = vis.raridade ?? 'comum';
  const cena = cenaDe(c, assunto, estilo);
  return {
    id, nome: c.nome, tipo: c.tipo, categoriaItem: c.categoriaItem ?? null, subtipos: c.subtipos, valores: c.valores,
    numeros: { custo: c.tipo === 'item' ? null : c.custo, grau: c.tipo === 'item' ? null : c.grau, arcanoGerado: c.tipo === 'item' ? null : c.arcanoGerado, ataque: c.ataque ?? null, vida: c.vida ?? null, defesa: c.defesa ?? null, vidaDesgaste: c.vidaDesgaste ?? null },
    velocidade: c.velocidade,
    textos: { regras: c.texto, ambientacao: '' },
    raridade, arteAlternativa: false, acabamentoVisual: 'arcanoclisma-v9',
    identidade: arteRef ? `${tituloRef}: personagem/objeto do catálogo aprovado (${arteRef}). Preservar a identidade; a arte da carta é uma cena NOVA com fundo.` : `Item sem arte de referência: ${assunto}.`,
    personagens: arteRef && ['P', 'M', 'D'].includes(estilo) ? [{ nome: tituloRef, jaExiste: true, referencias: [ARTE.get(arteRef)!.arquivo], aparencia: promptOriginal(arteRef), papelNaCena: 'protagonista da cena' }] : [],
    objetoReferencia: arteRef && estilo === 'I' ? { arquivo: ARTE.get(arteRef)!.arquivo, descricao: promptOriginal(arteRef) } : arteRef && estilo === 'C' ? { arquivo: ARTE.get(arteRef)!.arquivo, descricao: promptOriginal(arteRef), nota: 'o lugar vira a cena inteira da carta, em retrato vertical' } : null,
    estiloBase: estilo,
    cena,
    ambiente: ambiente(c.valores),
    arteObrigatorio: [FUNDO_OBRIGATORIO, 'Mostrar a ligação com a mecânica descrita na cena', ...(raridade === 'lendaria' ? ['Iluminação, textura e brilho conforme a diretriz v9: ' + BLOCO_LENDARIO] : [])],
    arteProibido: ['fundo branco, liso ou recortado', 'reaproveitar ou recortar a arte aprovada como se fosse cena nova', 'reaproveitar a arte de um Conjurador', 'texto, números ou símbolos de regra dentro da arte'],
    composicao: estilo === 'C' ? 'vista aberta em retrato vertical, com um ponto focal claro' : 'retrato vertical com o cenário visível; seguir a variação (não repetir a pose central e frontal de sempre)',
    variacao: variacaoDe(id, estilo !== 'C' && estilo !== 'I'),
    template: c.tipo === 'item' ? c.categoriaItem : c.tipo,
    uso: 'site online (cartas na mesa, na mão e no construtor) e impressão 63 × 88 mm',
    destino: `Arcanoclisma/arte/cartas-site/${id}.png`,
  };
}

function entradaConjurador(cid: string) {
  const c = D.catalogo.conjuradores[cid];
  const k = CONJURADORES[cid];
  const ref = D.conjuradoresArte[cid];
  return {
    id: `conjurador-${cid}`, nome: c.nome, tipo: 'conjurador', subtipos: [], valores: c.valores,
    numeros: { vida: c.vida, mao: c.maoInicial }, comprometimento: c.comprometimento,
    textos: { regras: c.habilidade, ambientacao: '' }, raridade: 'lendaria', arteAlternativa: false, acabamentoVisual: 'arcanoclisma-v9',
    identidade: `${c.nome} (${c.valores.map((v: string) => VALOR[v].nome).join(' + ')}, ${c.comprometimento}). Personagem NOVO e próprio: a arte provisória do site (${ref}) é só um marcador e não deve ser copiada.`,
    personagens: [{ nome: c.nome, jaExiste: false, referencias: [], aparencia: k.aparencia, papelNaCena: 'protagonista, pose heroica de corpo inteiro' }],
    estiloBase: 'P', cena: `${k.cena}. A cena mostra a habilidade: ${c.habilidade}`,
    ambiente: ambiente(c.valores) + ` Os dois Valores dividem a cena: ${VALOR[c.valores[0]].cor} de um lado, ${VALOR[c.valores[1]].cor} do outro.`,
    arteObrigatorio: [FUNDO_OBRIGATORIO, 'Iluminação, textura e brilho conforme a diretriz v9: ' + BLOCO_LENDARIO, 'Esta arte é exclusiva do Conjurador: nunca reaproveitar em outra carta'],
    arteProibido: ['fundo branco ou recortado', 'traços animais no personagem', 'texto dentro da arte'],
    composicao: 'retrato vertical, personagem de corpo inteiro, cenário dos Valores atrás; seguir a variação',
    variacao: variacaoDe('conjurador-' + cid, true),
    template: 'conjurador', uso: 'carta de Conjurador no site (mesa, escolha de baralho, início) e impressão',
    destino: `Arcanoclisma/arte/cartas-site/conjurador-${cid}.png`,
  };
}

const cartas = Object.entries(D.catalogo.cartas as Record<string, Carta>).filter(([id]) => id !== 'oculta');
const lote = {
  tarefaId: 'lote-site-v1', autor: 'Claude', estado: 'solicitado', necessidadeAtual: true, versao: 2,
  mudancasVersao2: 'Pedido do autor (03/10): prompts menos rígidos (pose, perspectiva, enquadramento, posição do rosto e acabamento variam por carta), eventos repetidos descritos de formas diferentes e HORA DO DIA em cada carta, com mais dia que noite ("todas as cartas parecem estar à noite"). As cenas dos Conjuradores perderam a hora fixa.', atualizadoEm: '2026-10-03',
  objetivoAtual: 'Arte final de TODAS as cartas jogáveis do site online (1ª versão jogável): os 9 Conjuradores e as ' + cartas.length + ' cartas do catálogo do site (Aliados, Temporárias/feitiços, Místicas, Estruturas e itens de Canal, Armadura e Extra). Pedido do autor: toda carta com cenário de fundo e ambientação; nada de usar só a imagem recortada do RPG.',
  pedidoDoAutor: 'TODAS AS CARTAS devem ter cenário de fundo. Não criar as artes da carta só com a imagem do RPG: colocar fundo e ambientação em TODAS, Conjurador, feitiço, equipamento etc.',
  prioridade: ['os 9 Conjuradores', 'as cartas dos 9 baralhos prontos (aparecem em toda partida)', 'os itens (Canal, Armadura, Extra)', 'o resto da coleção'],
  baralhosProntos: D.baralhos.map((b: { conjurador: string; lista: [string, number][] }) => ({ conjurador: b.conjurador, cartas: b.lista.map(([id]) => id) })),
  metaDeAvaliacao: 'avaliar a cada 5 artes com o autor (Carlos); as 9 de Conjurador são aprovadas uma a uma',
  referenciasClaude: ['Arcanoclisma/arte/27 Estilo de arte - o traço.md', 'Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md', 'Arcanoclisma/arena/diretrizes-arte-por-carta.json', 'Arcanoclisma/online/dados/identidade.json (carta → arte de referência)', 'Arcanoclisma/tcg/src/cartas-colecao.ts (cartas novas)', 'Arcanoclisma/online/public/jogo/dados.json (dados exatos do site)'],
  montagemDoPrompt: {
    regra: 'prompt = bloco de estilo do tipo (P, M, D, I ou C, em arte/27 Estilo de arte) SEM a frase final de fundo liso/branco + FUNDO_OBRIGATORIO + cena + ambiente + raridade (e o bloco v9 se lendária). O assunto em português vem da identidade, da cena e da aparência do personagem.',
    variacao: 'Cada carta traz "variacao" (hora do dia, pose, perspectiva, enquadramento, olhar, acabamento e relação com o evento), escolhida de forma fixa pelo id. Usar no prompt; ela vale mais que o hábito de figura central, frontal e noturna. A hora do dia define a luz da cena inteira (maioria de dia).',
    trocarNoBlocoDeEstilo: { de: ['Plain solid white background, no scenery, no text, no logo.', 'Plain solid white background, no hand, no character, no text, no logo.', 'Plain light gray background, no scenery, no text, no logo, no signature.', 'Fundo branco liso, sem cenário, sem texto, sem logotipo.'], para: 'Full scene with a detailed background environment and atmosphere (ground, sky or ceiling, architecture or nature, weather, time-of-day light), vertical 63:88 card composition, diffuse subtle background lighting with focal glow only on the relevant magic, no text, no logo.' },
    estiloI: 'Itens deixam de ser objeto isolado: aparecem em uso, num lugar, com mão/braço/silhueta de quem usa quando fizer sentido.',
    estiloC: 'Lugares viram cena vertical; podem ter figuras pequenas ao fundo para escala.',
    FUNDO_OBRIGATORIO,
    blocoLendarioV9: BLOCO_LENDARIO,
    formatoEntrega: 'PNG opaco 1260 × 1760 (63:88), sem alfa; arquivo em destino de cada carta; registrar prompt completo, referência usada e decisão do autor na entrega (entregas/lote-site-v1.visual.json).',
  },
  conjuradores: Object.keys(D.catalogo.conjuradores).map(entradaConjurador),
  cartas: cartas.map(([id, c]) => entradaCarta(id, c)),
  respostas: [],
  instrucoesAcabamento: 'Raridade só estética. Lendárias (Grau 7+) e os Conjuradores levam o bloco v9. Nenhuma carta é arte alternativa neste lote.',
};
writeFileSync(raiz + 'cartas/pedidos/lote-site-v1.claude.json', JSON.stringify(lote, null, 1) + '\n');
console.log(`lote-site-v1: ${lote.conjuradores.length} Conjuradores + ${lote.cartas.length} cartas.`);
