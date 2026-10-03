// Os 9 Conjuradores: um par de Valores cada, em anel (cada Valor aparece em dois Conjuradores),
// com uma habilidade própria que combina com os Valores ou com um tipo de carta/mecânica.
// Vida 20, mão inicial 5, compra 2 no Final, dano básico 1, ataque grátis (decisões do autor).
// Nomes e habilidades da rodada 7, escritos pelo autor. Habilidade que suspende o Conjurador custa o ataque dele no turno.
import type { ConjuradorDef, PoderConjurador } from './tipos.ts';

const padrao = { vida: 20, maoInicial: 5, compraFinal: 2, danoBasico: 1, custoAtaque: 0 } as const;

function conj(
  id: string, nome: string, valores: [string, string], comprometimento: ConjuradorDef['comprometimento'], habilidade: string, poder: PoderConjurador,
): ConjuradorDef {
  return { ...padrao, id, nome, valores, comprometimento, habilidade, poder };
}

export const conjuradoresPorValor: ConjuradorDef[] = [
  // Conflitüo (combate, rápido) + Primitivum (feras): a primeira pancada do turno pesa mais.
  conj('lyandra', 'Lyandra, a Domadora de Guerra', ['conflituo', 'primitivum'], 'Brigão',
    'O primeiro dano de combate que um Aliado seu causa em cada turno é 1 maior.',
    { primeiroDanoDeAliadoMais: 1 }),
  // Primitivum (aceleração, invocações) + Alvorum (Arcano): a matilha que chega traz Arcano.
  conj('carmal', 'Carmal, o Druida Crepuscular', ['primitivum', 'alvorum'], 'Invocador',
    'Quando o primeiro Aliado seu entra em campo em cada turno, ganhe 1 Arcano.',
    { gatilho: { quando: 'aliadoEntra', efeitos: [{ tipo: 'arcano', valor: 1 }] } }),
  // Alvorum (Arcano, cartas caras) + Sublimeyn (redução de custo): prepara a jogada para custar menos.
  conj('ricardo', 'Alquimista Ricardo', ['alvorum', 'sublimeyn'], 'Habilidoso',
    'Suspenda o Conjurador: a primeira carta que você jogar neste turno custa 1 a menos.',
    { ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'descontoProximaCarta', valor: 1 }] } }),
  // Sublimeyn (blink) + Persona (permanentes, burlar conjuração): entrar em campo outra vez.
  conj('roh', 'Roh, o Ilusionista das Máscaras', ['sublimeyn', 'persona'], 'Habilidoso',
    'Suspenda o Conjurador: devolva à sua mão um Aliado ou Mística seu.',
    { ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'devolver', alvo: 'escolhidoProprio' }] } }),
  // Persona (multidão) + Morbidyum (almas): o que cai no seu turno alimenta a mão.
  conj('agnelo', 'Agnelo, o Duelista Sombrio', ['persona', 'morbidyum'], 'Brigão',
    'Na primeira vez em cada um dos seus turnos que um Aliado seu vai do campo para o cemitério, compre 1 carta.',
    { gatilho: { quando: 'aliadoMorre', efeitos: [{ tipo: 'comprar', valor: 1 }], soNoSeuTurno: true } }),
  // Morbidyum (cemitério, reviver) + Cognitium (mente): chama de volta quem já caiu, de qualquer cemitério.
  conj('diastaros', 'Diástaros, o Necromante Vidente', ['morbidyum', 'cognitium'], 'Preparado',
    'Suspenda o Conjurador: ponha no seu campo um Aliado do seu cemitério ou do cemitério do oponente, pagando 2 de Arcano a mais além do custo dele.',
    { ativacao: { custo: 0, suspende: true, efeitos: [{ tipo: 'reanimar', custoExtra: 2 }] } }),
  // Cognitium (mente, olhar o futuro) + Eternyr (tempo): guarda uma carta no tempo para jogá-la mais barata.
  conj('ana', 'Ana, a Cronista do Tempo', ['cognitium', 'eternyr'], 'Preparado',
    'Suspenda o Conjurador e pague 2: bana uma carta da sua mão. No seu próximo turno, você pode jogá-la custando 4 a menos.',
    { ativacao: { custo: 2, suspende: true, efeitos: [{ tipo: 'banirDaMao', desconto: 4 }] } }),
  // Eternyr (anulação, tempo) + Dominum (prisão, controle): reaproveita no cemitério o que anula, banindo-o depois.
  conj('mitonho', 'Mitonho, o Carcereiro Temporal', ['eternyr', 'dominum'], 'Habilidoso',
    'Repentina: suspenda o Conjurador e pague 2. Neste turno, você pode jogar do seu cemitério uma carta cujo efeito anule outra, pagando o custo dela normalmente. Ao resolver, ela é banida.',
    { ativacao: { repentina: true, custo: 2, suspende: true, efeitos: [{ tipo: 'permitirAnularDoCemiterio' }] } }),
  // Dominum (defesa, custo extra ao oponente) + Conflitüo (combate): Aliados difíceis de mirar.
  conj('osmar', 'Osmar, o Capataz de Ferro', ['dominum', 'conflituo'], 'Parrudão da quebrada',
    'Seus Aliados têm Protegido 1.',
    { aliadosProtegidos: 1 }),
];

export const idsConjuradores = conjuradoresPorValor.map((c) => c.id);
