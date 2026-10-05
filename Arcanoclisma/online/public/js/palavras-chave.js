// Palavras-chave e termos das cartas (aba "Palavras-chave" do site e ajuda da mesa). Texto ligado ao motor real
// (tcg/src/motor.ts) e ao Livro de Regras (tcg/docs/LIVRO-DE-REGRAS.md, seções 5 a 9).
import { ICONES, icone } from './icones.js';
import { protegidoDe } from '/jogo/textos.js';
export const PALAVRAS_CHAVE = [
  { grupo: 'Velocidade', itens: [
    ['Findante', 'Só pode ser jogada na sua fase principal, com a pilha vazia e fora de combate.'],
    ['Repentina', 'Pode ser jogada a qualquer momento em que você tiver prioridade, inclusive em resposta a outra carta ou durante um ataque.'],
  ] },
  { grupo: 'Aliados', itens: [
    ['Repentino', 'Aliado que pode ser jogado a qualquer momento em que você tiver prioridade (em resposta, até no turno do oponente). Sem Repentino, Aliado só entra na sua fase principal.'],
    ['Proficiente', 'Ataca e usa efeitos de suspender no mesmo turno em que entra em campo (os outros Aliados esperam um turno).'],
    ['Etéreo', 'Em combate, só Etéreos atingem esta carta: Aliados Etéreos atacam e bloqueiam; um Conjurador com arma Etérea pode atacá-la e, com Armadura Etérea, pode bloqueá-la.'],
    ['Incomensurável', 'O dano que passar da Vida restante da carta atingida vai para o Conjurador dela (ao atacar, ao ser bloqueado ou com uma magia Incomensurável). Sem Incomensurável, o excedente se perde.'],
    ['Reanimum', 'Reanimum (pode ser jogada do seu cemitério; depois é banida): a carta pode ser jogada do cemitério do dono, pagando o custo normal, nas mesmas condições de velocidade e fase de uma carta da mão. Não é repetível: Temporária vai para o banimento ao resolver; Aliado, Mística ou Estrutura vai para o banimento quando sair do campo.'],
    ['Concessão', 'Efeito que dá palavras-chave, +/- Ataque e Vida e/ou um tempo em campo a uma carta (ex.: as unidades invocadas ganham Etéreo por 2 turnos). O turno em que a carta entra conta como o 1º (seja de quem for); depois contam só os turnos do controlador; ao acabar a carta vai para o destino indicado (padrão: cemitério).'],
    ['Protessência', 'Esta carta não pode ser escolhida como alvo de efeitos do oponente (cartas, ativações, habilidades, gatilhos, escolhas posteriores e sorteios), nem anulada por ele na pilha. Efeitos sem alvo (que atingem todos) e os seus próprios efeitos continuam a afetá-la. Ataques não são efeitos: um Aliado com Protessência pode ser atacado.'],
    ['Protegido', 'Quem mirar esta carta com uma carta ou ativação do oponente paga o que o Protegido pede: "Protegido N" = N de Arcano a mais; algumas cartas pedem também Vida, descarte, cartas do Grau ao cemitério ou um Grau mínimo. Com dois Protegidos (o da carta e outro dado pelo Conjurador ou por uma Concessão), os dois valem: o oponente paga cada um, juntos. Sem poder pagar, não mira. O dono não paga; efeitos sem alvo e sacrifícios não pagam.'],
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
    ['Reanimar', 'Um Aliado volta do cemitério para o campo: quem controla o efeito o escolhe ao resolver e paga o que a carta pedir (nada, o custo dele ou o custo + X), com a reserva, o Grau e cartas da mão enviadas ao Arcano. Sem poder pagar, não traz nada.'],
    ['Tirar da mão', 'Descartar (vai para o cemitério) ou banir cartas da mão: ao acaso, o dono escolhe sem mostrar, ou quem jogou olha a mão e escolhe (só ele vê).'],
    ['Efeito secundário', 'Custo ou consequência que vem depois dos outros efeitos ("...; depois, seu Conjurador toma 2 de dano"): dano no Conjurador, descarte, sacrificar um Aliado ou cartas do Grau ao cemitério.'],
    ['Sacrificar', 'O dono escolhe um Aliado dele (só Aliado; Místicas e Estruturas não) e o manda do campo para o cemitério, sem ganhar nada. Não mira: Protegido e Protessência não impedem. Conta como morte para gatilhos.'],
    ['No Cataclisma do turno do oponente', 'Gatilho que acontece na etapa de Cataclisma (fim) de cada turno do oponente.'],
    ['Olhar o topo', 'Olhe as cartas do topo do baralho, ponha algumas na mão e o resto no fundo.'],
    ['Suspender (ativação)', 'Deite a carta para usar a habilidade dela; ela levanta no seu Final.'],
  ] },
  { grupo: 'Conjurador e itens', itens: [
    ['Bloquear', 'Quando um inimigo ataca o seu Conjurador, Aliados de pé e o próprio Conjurador podem entrar na frente, um ou vários. Quem ataca distribui o dano entre eles; se o atacante for Incomensurável, o que passar da Vida de cada um vai para o Conjurador; todos os bloqueadores causam dano ao atacante, mesmo os que morrem. Ataques a Aliados, Místicas e Estruturas não podem ser bloqueados. Quem bloqueia fica deitado até o seu Final.'],
    ['Canal', 'Item do Conjurador: Arma, Foco Arcano ou Transcritor. Só a Arma dá Ataque ao Conjurador.'],
    ['Foco Arcano', 'Canal que fortalece magias: suspenda o Conjurador e pague o custo; o próximo dano ou cura de uma Temporária sua neste turno fica maior.'],
    ['Transcritor', 'Canal que mexe no baralho e na mão: suspenda o Conjurador e pague o custo para olhar o topo ou comprar cartas.'],
    ['Custo de item', 'Além de suspender o Conjurador, cada item pede algo: pagar Arcano, descartar cartas da mão ou o Conjurador tomar dano.'],
    ['Extra de competência', 'Extra que só serve aos Conjuradores de um Comprometimento (Invocador, Brigão, Parrudão da quebrada, Habilidoso, Preparado).'],
    ['Ficha', 'Criatura criada por um efeito (ex.: Wyvern Filhote). Não vem do baralho.'],
    ['Valor', 'Toda carta pertence a um Valor: a cor da bolinha do custo. O baralho só leva cartas dos 2 Valores do seu Conjurador: carta de 2 Valores só se forem exatamente os 2 dele; de 3 ou mais, se os 2 dele estiverem entre os da carta.'],
    ['Armadura', 'Quando o Conjurador bloqueia, você escolhe se a usa: ela evita até a Defesa de dano de um ataque por turno e se desgasta até quebrar.'],
    ['Extra', 'Item de apoio de uso único (ou temático da competência do Conjurador).'],
  ] },
];

/** Lembrete curto (em itálico na carta) do que cada palavra-chave faz; pedido do autor, 04/10. */
export const LEMBRETES = {
  Repentino: 'este Aliado pode ser jogado em resposta, até no turno do oponente.',
  Proficiente: 'ataca e usa ativações no turno em que entra.',
  Etéreo: 'em combate, só Etéreos o atingem (arma Etérea ataca, Armadura Etérea bloqueia).',
  Reanimum: 'pode ser jogada do seu cemitério; depois é banida.',
  Incomensurável: 'o dano que passar da Vida da carta atingida vai para o Conjurador dela.',
  Protessência: 'não pode ser alvo de efeitos do oponente, nem anulada por eles.',
  Protegido: (n) => `mirar esta carta custa ${n} de Arcano a mais ao oponente.`,
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
  // Protegido só de Arcano ("Protegido N") ganha lembrete; com as variações, o próprio texto já explica
  const pr = protegidoDe(d);
  if (pr && Object.keys(pr).length === 1 && pr.arcano && !jaExplica('Protegido')) l.push(`Protegido ${pr.arcano}: ${LEMBRETES.Protegido(pr.arcano)}`);
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
