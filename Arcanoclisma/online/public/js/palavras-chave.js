// Palavras-chave e termos das cartas (aba "Palavras-chave" do site e ajuda da mesa). Texto ligado ao motor real
// (tcg/src/motor.ts) e ao Livro de Regras (tcg/docs/LIVRO-DE-REGRAS.md, seções 5 a 9).
import { ICONES, icone } from './icones.js';
export const PALAVRAS_CHAVE = [
  { grupo: 'Velocidade', itens: [
    ['Findante', 'Só pode ser jogada na sua fase principal, com a pilha vazia e fora de combate.'],
    ['Repentina', 'Pode ser jogada a qualquer momento em que você tiver prioridade, inclusive em resposta a outra carta ou durante um ataque.'],
  ] },
  { grupo: 'Aliados', itens: [
    ['Proficiente', 'Ataca e usa efeitos de suspender no mesmo turno em que entra em campo (os outros Aliados esperam um turno).'],
    ['Etéreo', 'Só pode ser bloqueado por Aliados Etéreos.'],
    ['Protegido N', 'Quem mirar esta carta com uma carta ou ativação paga N de Arcano a mais (a sua própria não paga).'],
    ['Ao entrar', 'Efeito que acontece quando a carta entra em campo.'],
    ['Ao causar dano de combate ao Conjurador inimigo', 'Gatilho que acontece quando este Aliado acerta o Conjurador rival em combate.'],
  ] },
  { grupo: 'Custos', itens: [
    ['Custo', 'Arcano que você paga para jogar a carta (a bolinha colorida; a cor é o Valor da carta).'],
    ['Grau', 'Número mínimo de cartas na sua Zona de Grau para poder jogar a carta.'],
    ['Gera Arcano', 'Quanto Arcano a carta dá quando você a envia da mão ao Arcano para pagar algo.'],
    ['Custo de Valor', 'Precisa ter no seu Grau pelo menos uma carta do Valor indicado.'],
    ['Descarta do Grau N', 'Ao jogar, N cartas da sua Zona de Grau vão para o cemitério.'],
    ['Pagar para anular N', 'Depois de jogada, o oponente pode pagar N de Arcano para que ela não resolva.'],
    ['Grau extra N', 'Enquanto estiver em campo, conta como N cartas a mais no seu Grau.'],
  ] },
  { grupo: 'Efeitos', itens: [
    ['Anular', 'Uma carta na pilha é anulada: vai para o cemitério sem efeito.'],
    ['Devolver', 'Um Aliado ou Mística volta para a mão do dono.'],
    ['Destruir', 'A carta vai para o cemitério.'],
    ['Varredura', 'Causa dano a cada Aliado do oponente (ou de todos, se disser "inclusive os seus").'],
    ['Banir', 'A carta vai para a zona de banimento; algumas podem ser jogadas de lá depois.'],
    ['Reanimar', 'Um Aliado volta do cemitério para o campo.'],
    ['Olhar o topo', 'Olhe as cartas do topo do baralho, ponha algumas na mão e o resto no fundo.'],
    ['Suspender (ativação)', 'Deite a carta para usar a habilidade dela; ela levanta no seu Final.'],
  ] },
  { grupo: 'Conjurador e itens', itens: [
    ['Bloquear', 'Quando um inimigo ataca o seu Conjurador, Aliados de pé e o próprio Conjurador podem entrar na frente, um ou vários. Quem ataca distribui o dano entre eles e o que passar da Vida de cada um vai para o Conjurador; todos os bloqueadores causam dano ao atacante, mesmo os que morrem. Ataques a Aliados, Místicas e Estruturas não podem ser bloqueados. Quem bloqueia fica deitado até o seu Final.'],
    ['Canal', 'Item do Conjurador: Arma, Foco Arcano ou Transcritor. Só a Arma dá Ataque ao Conjurador.'],
    ['Foco Arcano', 'Canal que fortalece magias: suspenda o Conjurador e pague o custo; o próximo dano ou cura de uma Temporária sua neste turno fica maior.'],
    ['Transcritor', 'Canal que mexe no baralho e na mão: suspenda o Conjurador e pague o custo para olhar o topo ou comprar cartas.'],
    ['Custo de item', 'Além de suspender o Conjurador, cada item pede algo: pagar Arcano, descartar cartas da mão ou o Conjurador tomar dano.'],
    ['Extra de competência', 'Extra que só serve aos Conjuradores de um Comprometimento (Invocador, Brigão, Parrudão da quebrada, Habilidoso, Preparado).'],
    ['Ficha', 'Criatura criada por um efeito (ex.: Wyvern Filhote). Não vem do baralho.'],
    ['Valor', 'Toda carta pertence a um Valor: a cor da bolinha do custo. O baralho só leva cartas dos 2 Valores do seu Conjurador.'],
    ['Armadura', 'Quando o Conjurador bloqueia, você escolhe se a usa: ela evita até a Defesa de dano de um ataque por turno e se desgasta até quebrar.'],
    ['Extra', 'Item de apoio de uso único (ou temático da competência do Conjurador).'],
  ] },
];

/** Lembrete curto (em itálico na carta) do que cada palavra-chave faz; pedido do autor, 04/10. */
export const LEMBRETES = {
  Proficiente: 'ataca e usa ativações no turno em que entra.',
  Etéreo: 'só Aliados Etéreos o bloqueiam.',
  Protegido: (n) => `mirar esta carta custa ${n} a mais ao oponente.`,
  Repentina: 'pode ser jogada em resposta, até no turno do oponente.',
  'Pagar para anular': (n) => `o oponente pode pagar ${n} de Arcano para que ela não resolva.`,
};

/** Lembretes da carta (palavras-chave dela, Protegido, Repentina e "pagar para anular"). */
export function lembretesDe(d) {
  if (!d) return [];
  const l = [];
  // o texto já explica a palavra entre parênteses ("Etéreo (só é bloqueado...)"): não repete
  const jaExplica = (k) => (d.texto ?? '').includes(k) && /^\s*(\d+\s*)?\(/.test((d.texto ?? '').split(k)[1] ?? '');
  for (const k of d.palavrasChave ?? []) if (typeof LEMBRETES[k] === 'string' && !jaExplica(k)) l.push(`${k}: ${LEMBRETES[k]}`);
  if (d.protegido && !jaExplica('Protegido')) l.push(`Protegido ${d.protegido}: ${LEMBRETES.Protegido(d.protegido)}`);
  if (d.velocidade === 'repentina' && d.tipo !== 'aliado') l.push(`Repentina: ${LEMBRETES.Repentina}`);
  if (d.pagarParaAnular) l.push(`Pagar para anular ${d.pagarParaAnular}: ${LEMBRETES['Pagar para anular'](d.pagarParaAnular)}`);
  return l;
}

/** HTML da lista (com âncoras), para a aba do site e a ajuda da mesa. */
export function palavrasChaveHTML(esc) {
  // ícone da mecânica antes do termo (quando o GPT já entregou), pelo mesmo gatilho do texto das cartas
  const comIcone = (t) => { const i = ICONES.find((x) => x.gatilho.test(t)); return (i ? icone(i.id) : '') + esc(t); };
  return PALAVRAS_CHAVE.map((g) => `<section class="pc-grupo"><h3>${esc(g.grupo)}</h3><dl>${g.itens.map(([t, d]) => `<dt>${comIcone(t)}</dt><dd>${esc(d)}</dd>`).join('')}</dl></section>`).join('');
}
