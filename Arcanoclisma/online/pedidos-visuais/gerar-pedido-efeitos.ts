// Gera o pedido de efeitos e movimentos da mesa para o GPT/Codex (Embelezador): Arcanoclisma/efeitos/pedidos/mesa-efeitos-v1.claude.json.
// Pedidos do autor (03/10/2026): perspectiva da carta saindo da mão e entrando no campo; efeito de resolução para cada
// magia (indo até o alvo, acontecendo ou entrando em campo), com imagem própria de cada carta; e, sempre que uma carta
// bate em outra, a animação da carta e da perspectiva dela se deitando e se movendo para acertar a outra.
// O GPT cria todas as animações, movimentos e imagens; o Claude só liga os gatilhos (online/public/js/mesa-ganchos.js).
// Uso: node online/pedidos-visuais/gerar-pedido-efeitos.ts   (depois de node online/construir.ts)
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const raiz = fileURLToPath(new URL('../../', import.meta.url));
const D = JSON.parse(readFileSync(raiz + 'online/public/jogo/dados.json', 'utf8'));
type Ef = { tipo: string; valor?: number; alvo?: string; ambos?: boolean };
type Carta = { id: string; nome: string; tipo: string; valores: string[]; velocidade: string; texto: string; ataque?: number; vida?: number; categoriaItem?: string; palavrasChave?: string[]; efeitos?: Ef[]; aoEntrar?: Ef[]; aoDanoConjurador?: Ef[]; ativacao?: { efeitos: Ef[] } };

/** Forma do efeito na mesa, a partir da mecânica: vai até o alvo, acontece no lugar, ou entra em campo. */
function forma(efs: Ef[]): { forma: string; alvo: string } {
  const tipos = efs.map((e) => e.tipo);
  const alvo = efs.find((e) => e.alvo)?.alvo ?? '';
  if (tipos.some((t) => ['dano', 'destruir', 'devolver', 'curar'].includes(t)) && alvo) return { forma: 'projétil ou raio que vai de quem conjura até o alvo e o atinge', alvo: { escolhido: 'carta ou Conjurador escolhido', escolhidoAliado: 'Aliado escolhido', escolhidoProprio: 'Aliado seu escolhido', conjuradorInimigo: 'Conjurador rival', conjuradorProprio: 'seu Conjurador', pilha: 'carta na pilha' }[alvo] ?? alvo };
  if (tipos.includes('anular')) return { forma: 'corte, selo ou apagar sobre a carta inimiga na pilha (lateral direita)', alvo: 'carta na pilha' };
  if (tipos.some((t) => ['danoTodosAliados', 'destruirTodosAliados'].includes(t))) return { forma: 'onda que varre o campo inteiro (de um lado ou dos dois)', alvo: 'todos os Aliados afetados' };
  if (tipos.includes('reanimar')) return { forma: 'algo se ergue do cemitério e entra em campo', alvo: 'Aliado no cemitério' };
  return { forma: 'acontece no lugar, em volta de quem conjura (sem alvo)', alvo: 'nenhum' };
}

const cartas = Object.values(D.catalogo.cartas as Record<string, Carta>).filter((c) => c.tipo !== 'oculta');
const resolucao = cartas
  .map((c) => ({ c, efs: [...(c.efeitos ?? []), ...(c.aoEntrar ?? []), ...(c.ativacao?.efeitos ?? []), ...(c.aoDanoConjurador ?? [])] }))
  .filter(({ efs }) => efs.length)
  .map(({ c, efs }) => ({
    id: c.id, nome: c.nome, tipo: c.tipo, valores: c.valores, texto: c.texto, arteDaCarta: D.visual[c.id]?.arte ?? null,
    quando: c.tipo === 'temporaria' ? 'ao resolver da pilha' : c.ativacao && !c.aoEntrar ? 'ao resolver a ativação' : 'ao entrar em campo / gatilho',
    ...forma(efs),
    duracaoMs: '600–1200',
    imagem: 'efeito próprio desta carta (cor do Valor, elemento da arte), PNG/WebP com alfa ou sequência/sprite; sem texto',
  }));
const colisao = [
  ...Object.values(D.catalogo.conjuradores as Record<string, { id: string; nome: string; valores: string[] }>).map((c) => ({ id: c.id, nome: c.nome, tipo: 'conjurador', valores: c.valores, arteDaCarta: D.conjuradoresArte[c.id] })),
  ...cartas.filter((c) => c.tipo === 'aliado').map((c) => ({ id: c.id, nome: c.nome, tipo: 'aliado', valores: c.valores, palavrasChave: c.palavrasChave ?? [], ataque: c.ataque, arteDaCarta: D.visual[c.id]?.arte ?? null })),
];

const pedido = {
  tarefaId: 'mesa-efeitos-v1', autor: 'Claude', estado: 'solicitado', necessidadeAtual: true, versao: 1, atualizadoEm: '2026-10-03',
  objetivoAtual: 'Efeitos e movimentos da mesa do site online: entrada em campo, resolução de cada magia e colisão de cada carta.',
  pedidoDoAutor: [
    'Efeitos de quando a carta é jogada: perspectiva de quando ela sai da mão e entra no campo (a carta pode ser arrastada até o campo).',
    'Efeito de resolução para cada magia: o efeito indo na direção do alvo, ou acontecendo, ou entrando em campo, com uma imagem de efeito particular para cada carta.',
    'Sempre que uma carta bater em outra: a imagem da animação da carta e da perspectiva dela se deitando e se movendo para acertar essa carta (vale para Aliados e Conjuradores; esta ordem do autor, de 03/10, substitui a regra antiga de movimento só genérico para Aliados).',
    'O GPT cria todas as animações, movimentos e imagens; o Claude só conecta. Imagem só entra no site com o OK do GPT.',
    'Toda carta nova vem com o efeito da magia dela, sempre adaptado e relacionado à arte da carta (mesmos elementos, cores do Valor, personagem ou objeto da arte).',
  ],
  jaEntregueEUsado: 'Ataque e bloqueio genéricos (kit/cartas.css: .card-motion + .attack-generic / .block-generic, 880 ms, terminando deitada) já estão ligados em online/public/js/mesa-ganchos.js.',
  contratoDeIntegracao: {
    arquivo: 'Arcanoclisma/online/public/js/mesa-efeitos-codex.js (ES module)',
    exporta: 'criarEfeitosMesa(raiz) → objeto com os métodos abaixo; cada um pode devolver uma Promise; nenhum envia comando ao jogo.',
    metodos: {
      entrarEmCampo: '{ el, id, defId, dono, meu } — el é a carta já desenhada no campo (wrapper .cm). Animar a carta saindo da mão (sua: faixa de baixo; rival: faixa de cima) e pousando no campo, com perspectiva.',
      resolver: '{ item, defId, origemEl, alvoEl, alvoRect, meu } — item é o item da pilha que acabou de sair (tipo carta/gatilho/ataque; item.alvo é o id do alvo). Mostrar o efeito de resolução da carta defId.',
      colidir: '{ atacanteEl, alvoEl, atacanteRect, alvoRect, atacanteDefId, alvoDefId, bloqueio, meu } — o dano de combate acabou de acontecer. Mostrar a carta se deitando e indo acertar a outra (perspectiva e imagem próprias de cada carta).',
      limpar: '() — a mesa fechou; cancelar o que estiver tocando.',
    },
    assets: 'Arcanoclisma/online/public/kit/efeitos/<id da carta>/... com manifesto kit/efeitos/manifesto.json: { "<id>": { "arquivos": [...], "okGpt": true, "duracaoMs": n } }. Asset sem okGpt true não é usado.',
    geometria: 'A mesa usa zoom no .mesa-mundo; use getBoundingClientRect() (já com zoom) e desenhe numa camada position:fixed acima da mesa e abaixo das janelas (z-index entre 40 e 59).',
    carregamento: 'A mesa só importa o módulo quando ele existe no build (DADOS.efeitosCodex, calculado por online/construir.ts); depois da entrega, o Claude reconstrói e publica.',
    movimentoReduzido: 'Respeitar prefers-reduced-motion (sem animação).',
    sigilo: 'Cartas ocultas do rival usam só o verso; não ler o DOM de faces escondidas.',
  },
  entradaEmCampo: { cartas: 'todas as permanentes (Aliado, Mística, Estrutura)', formato: 'movimento por transformação da carta, com perspectiva; efeito de pouso opcional por Valor' },
  resolucao,
  colisao,
  formatoEntrega: 'Embelezador: assets em Arcanoclisma/efeitos/assets/mesa-efeitos-v1/vNN/ e cópia para online/public/kit/efeitos/; entrega em Arcanoclisma/efeitos/entregas/mesa-efeitos-v1.visual.json (arquivos, dimensões, alfa, timings, okGpt). O Claude responde em entregas/mesa-efeitos-v1.integracao.claude.json.',
  prioridade: ['colisão dos 9 Conjuradores e dos Aliados dos baralhos prontos', 'resolução das Temporárias dos baralhos prontos', 'entrada em campo', 'o resto da coleção'],
};
mkdirSync(raiz + 'efeitos/pedidos', { recursive: true });
writeFileSync(raiz + 'efeitos/pedidos/mesa-efeitos-v1.claude.json', JSON.stringify(pedido, null, 1) + '\n');
console.log(`mesa-efeitos-v1: ${resolucao.length} cartas com resolução, ${colisao.length} com colisão.`);
