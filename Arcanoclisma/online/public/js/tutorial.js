// Tutorial: lições curtas montadas sobre o motor real, contra um bot com roteiro, e no fim uma partida completa com dicas.
// Cada lição monta um cenário (cartas na mão, no campo e no Grau), e cada passo tem um objetivo conferido no estado do jogo.
import { criarJogo, executar, recemChegado, grauEfetivo } from '/jogo/motor.js';
import { $, esc, DADOS, conj, carta, nomeCurto, cenario, local, avisar, prontoDe, listaParaCartas } from './base.js';
import { conjuradorHTML } from './carta.js';
import { abrirMesa } from './mesa.js';
import { criarPartidaLocal } from './partida-local.js';

const EU = 0, BOT = 1;
const N = (id) => carta(id).nome;

/** Monta um cenário de jogo a partir de listas de cartas por zona. */
function montarCenario(spec) {
  const deck = (s) => [...(s.mao ?? []), ...(s.campo ?? []), ...(s.grau ?? []), ...(s.topo ?? []), ...Array(24).fill(s.enchimento ?? 'base-recurso')];
  const e = criarJogo({ catalogo: DADOS.catalogo, regras: DADOS.regras, semente: 11,
    baralhos: [{ conjurador: spec.eu.conj, cartas: deck(spec.eu), itens: spec.eu.itens ?? [] }, { conjurador: spec.bot.conj, cartas: deck(spec.bot), itens: spec.bot.itens ?? [] }] });
  executar(e, { tipo: 'escolherInicio', jogador: e.pendencia.jogador, primeiro: 0 });
  for (const [j, s] of [[EU, spec.eu], [BOT, spec.bot]]) {
    const z = e.jogadores[j];
    const todas = [...z.baralho, ...z.mao];
    const tirar = (defId) => { const i = todas.findIndex((id) => e.instancias[id].defId === defId); if (i < 0) throw new Error('cenário: falta ' + defId); return todas.splice(i, 1)[0]; };
    z.mao = (s.mao ?? []).map(tirar);
    z.campo = (s.campo ?? []).map(tirar);
    for (const id of z.campo) e.instancias[id].entrouNoTurno = 1;
    z.grau = (s.grau ?? []).map(tirar);
    for (const id of z.grau) e.instancias[id].revelada = false;
    z.baralho = [...(s.topo ?? []).map(tirar), ...todas];
    z.arcano = s.arcano ?? 0;
    const c = e.instancias[z.conjuradorId];
    c.dano = conj(s.conj).vida - (s.vida ?? conj(s.conj).vida);
    for (const [defId, dano] of Object.entries(s.dano ?? {})) { const id = z.campo.find((x) => e.instancias[x].defId === defId); if (id) e.instancias[id].dano = dano; }
  }
  Object.assign(e, { turno: spec.turno ?? 3, ativo: spec.ativo ?? EU, primeiro: 0, etapa: 'principal', prioridade: spec.ativo ?? EU, passes: 0, pendencia: null, combate: null, grauNoTurno: false, agiuNoTurno: false, semAcaoTurnos: [0, 0], log: ['Tutorial: cenário montado.'] });
  return e;
}

// consultas usadas pelos passos
const campoDe = (e, j, defId) => e.jogadores[j].campo.find((id) => e.instancias[id].defId === defId);
const maoDe = (e, j, defId) => e.jogadores[j].mao.find((id) => e.instancias[id].defId === defId);
const noLog = (e, trecho) => e.log.some((l) => l.includes(trecho));
const danoConj = (e, j) => e.instancias[e.jogadores[j].conjuradorId].dano;

const LICOES = [
  {
    id: 'grau', titulo: 'Grau e Arcano', resumo: 'Pôr cartas no Grau, gerar Arcano e pagar custos (inclusive enviando cartas ao Arcano).',
    cenario: () => montarCenario({
      eu: { conj: 'lyandra', itens: ['item-canal-cajado', 'item-armadura-gibao'], mao: ['base-recurso', 'base-aliado-c2-agressivo', 'base-dano-c1', 'base-aliado-c4-agressivo', 'base-aliado-c1-equilibrado'], grau: ['base-aliado-c3-equilibrado'] },
      bot: { conj: 'carmal', itens: ['item-canal-cajado', 'item-armadura-gibao'], campo: ['base-aliado-c2-agressivo-proficiente'], grau: ['base-recurso', 'base-recurso'] },
    }),
    passos: [
      { titulo: 'Bem-vindo à mesa', texto: 'Embaixo fica o seu Conjurador (Lyandra): a vida ♥ e o ataque ⚔ (que vem da arma) aparecem sobre a carta dele. A esfera azul ao lado é o seu Arcano. Em cima, o Conjurador do oponente. Vence quem zerar a vida do Conjurador do outro. Sua mão fica fora da mesa, embaixo; fases, decisões e a pilha ficam aqui na direita.' },
      { titulo: 'A Zona de Grau', texto: `Uma vez por turno você põe uma carta da mão na Zona de Grau, onde ela fica virada (verso) no seu campo. O Grau decide quais cartas você pode jogar: uma carta de Grau 2 exige 2 cartas no seu Grau. Clique em <b>${N('base-recurso')}</b> na sua mão e escolha <b>Pôr no Grau</b>.`, objetivo: 'Ponha uma carta no Grau.', feito: (e) => e.jogadores[EU].grau.length >= 2, destaque: (e) => [maoDe(e, EU, 'base-recurso')], acoes: [{ tipo: 'colocarGrau', def: 'base-recurso' }] },
      { titulo: 'Arcano para pagar', texto: `Cada carta do Grau pode ser suspensa (deitada) para gerar 1 Arcano. O <b>${N('base-aliado-c2-agressivo')}</b> custa 2 e exige Grau 1. Clique nele e escolha <b>Jogar</b>: a janela de pagamento já sugere suspender as duas cartas do Grau.`, objetivo: `Jogue o ${N('base-aliado-c2-agressivo')}.`, feito: (e) => !!campoDe(e, EU, 'base-aliado-c2-agressivo'), destaque: (e) => [maoDe(e, EU, 'base-aliado-c2-agressivo')], acoes: [{ tipo: 'jogar', def: 'base-aliado-c2-agressivo' }] },
      { titulo: 'A pilha', texto: 'Viu a carta aqui na direita? Tudo que você joga vai para a pilha e o oponente pode responder antes de resolver. Quando os dois passam, a carta resolve. O Aliado entrou com "zZ": no turno em que entra ele não ataca.', botao: true },
      { titulo: 'Enviar cartas ao Arcano', texto: `Seu Grau já está todo suspenso. Para pagar mais, envie cartas da mão ao Arcano: cada uma gera o número do círculo de cima (a ${N('base-recurso')} gera 3). Elas vão para o fundo do baralho no seu Final. Jogue <b>${N('base-dano-c1')}</b> (2 de dano) no Aliado do oponente e pague enviando uma carta.`, objetivo: `Destrua o Aliado do oponente com ${N('base-dano-c1')}.`, feito: (e) => e.jogadores[BOT].campo.length === 0, destaque: (e) => [maoDe(e, EU, 'base-dano-c1')], alvos: (e) => e.jogadores[BOT].campo, acoes: [{ tipo: 'jogar', def: 'base-dano-c1' }] },
      { titulo: 'Grau limita o poder', texto: `O <b>${N('base-aliado-c4-agressivo')}</b> custa 4 e exige Grau 3: você só tem 2. As cartas fortes chegam quando o seu Grau cresce, um por turno. A sobra de Arcano fica na reserva até o seu Cataclisma, quando some.`, botao: true },
      { titulo: 'Lição concluída!', texto: 'Você aprendeu Grau, Arcano, pagamento e a pilha. Próxima: combate.', fim: true },
    ],
    bot: () => ({ tipo: 'passar', jogador: BOT }),
  },
  {
    id: 'combate', titulo: 'Ataque e bloqueio', resumo: 'Atacar com Aliados e com o Conjurador, Proficiente, bloquear e a Armadura.',
    cenario: () => montarCenario({
      eu: { conj: 'lyandra', itens: ['item-canal-cajado', 'item-armadura-gibao'], mao: ['base-aliado-c2-agressivo-proficiente', 'base-recurso', 'base-recurso'], campo: ['base-aliado-c2-agressivo', 'base-aliado-c6-defensivo'], grau: ['base-recurso', 'base-recurso', 'base-recurso'] },
      bot: { conj: 'carmal', itens: ['item-canal-cajado', 'item-armadura-gibao'], campo: ['base-aliado-c2-defensivo', 'base-aliado-c4-agressivo'], grau: ['base-recurso', 'base-recurso', 'base-recurso'] },
    }),
    passos: [
      { titulo: 'Atacar', texto: `Na sua fase Principal, com a pilha vazia, Aliados de pé podem atacar o Conjurador ou um Aliado inimigo. Atacar suspende (deita) a carta. Clique no <b>${N('base-aliado-c2-agressivo')}</b>, escolha <b>Atacar</b> e mire o Conjurador do oponente.`, objetivo: 'Cause dano ao Conjurador do oponente.', feito: (e) => danoConj(e, BOT) > 0 && !e.combate, destaque: (e) => [campoDe(e, EU, 'base-aliado-c2-agressivo')], alvos: (e) => [e.jogadores[BOT].conjuradorId], acoes: [{ tipo: 'atacar', def: 'base-aliado-c2-agressivo' }] },
      { titulo: 'Proficiente', texto: `Aliados que acabaram de entrar não atacam, a não ser que tenham <b>Proficiente</b>. A <b>${N('base-aliado-c2-agressivo-proficiente')}</b> tem. Jogue-a e ataque com ela no mesmo turno.`, objetivo: `Jogue e ataque com ${N('base-aliado-c2-agressivo-proficiente')}.`, feito: (e) => { const id = campoDe(e, EU, 'base-aliado-c2-agressivo-proficiente'); return !!id && e.instancias[id].suspensa && !e.combate; }, destaque: (e) => [maoDe(e, EU, 'base-aliado-c2-agressivo-proficiente') ?? campoDe(e, EU, 'base-aliado-c2-agressivo-proficiente')], acoes: [{ tipo: 'jogar', def: 'base-aliado-c2-agressivo-proficiente' }, { tipo: 'atacar', def: 'base-aliado-c2-agressivo-proficiente' }] },
      { titulo: 'O Conjurador ataca com a arma', texto: 'O seu Conjurador também ataca, com o Ataque do Canal (aqui, o Cajado de aprendiz: 1). Ataque o Conjurador do oponente com Lyandra. Ele vai bloquear: quem bloqueia troca dano com o atacante.', objetivo: 'Ataque com o seu Conjurador.', feito: (e) => e.instancias[e.jogadores[EU].conjuradorId].suspensa && !e.combate, destaque: (e) => [e.jogadores[EU].conjuradorId], alvos: (e) => [e.jogadores[BOT].conjuradorId], acoes: [{ tipo: 'atacar', conj: true }] },
      { titulo: 'Encerrar o turno', texto: 'Clique em <b>Encerrar fase Principal</b>. Depois vêm o Cataclisma (o Arcano que sobrou some) e o Final (suas cartas levantam).', objetivo: 'Passe o turno.', feito: (e) => e.ativo === BOT, passar: true },
      { titulo: 'Bloquear', texto: `O oponente vai atacar. Quando um ataque resolve, você escolhe um bloqueador de pé: clique no <b>${N('base-aliado-c6-defensivo')}</b> (5/14) quando ele brilhar. Quem bloqueia fica suspenso; um Aliado só levanta no seu Final, então não ataca no seu próximo turno. O Conjurador levanta no Início do seu turno, e a Armadura dele evita dano quando ele bloqueia.`, objetivo: 'Bloqueie o ataque.', feito: (e) => noLog(e, 'Jogador 0 bloqueia com') && !e.combate, destaque: (e) => [campoDe(e, EU, 'base-aliado-c6-defensivo')], alvos: (e) => [campoDe(e, EU, 'base-aliado-c6-defensivo')], acoes: [{ tipo: 'bloquear', def: 'base-aliado-c6-defensivo' }] },
      { titulo: 'Lição concluída!', texto: 'Dano fica guardado (memória de dano) até alguém curar. Etéreo: só é bloqueado por Aliados Etéreos. Próxima lição: como a pilha funciona.', fim: true },
    ],
    bot: (e) => {
      if (e.pendencia?.tipo === 'bloqueio') return e.combate && e.instancias[e.combate.atacante].ehConjurador ? { tipo: 'bloquear', jogador: BOT, bloqueador: campoDe(e, BOT, 'base-aliado-c2-defensivo') ?? null } : { tipo: 'bloquear', jogador: BOT, bloqueador: null };
      const javali = campoDe(e, BOT, 'base-aliado-c4-agressivo');
      if (e.ativo === BOT && e.etapa === 'principal' && !e.pilha.length && !e.combate && javali && !e.instancias[javali].suspensa) return { tipo: 'atacar', jogador: BOT, atacante: javali, alvo: e.jogadores[EU].conjuradorId };
      return { tipo: 'passar', jogador: BOT };
    },
  },
  {
    id: 'pilha-basico', titulo: 'Como a pilha funciona', resumo: 'Tudo entra na pilha: cartas, respostas e ataques. Prioridade, responder por cima e a ordem em que resolve.',
    cenario: () => montarCenario({
      eu: { conj: 'lyandra', itens: ['item-canal-cajado', 'item-armadura-gibao'], mao: ['base-dano-c1', 'base-dano-c1-repentina', 'base-recurso'], campo: ['base-aliado-c2-agressivo'], grau: ['base-recurso', 'base-recurso', 'base-recurso'] },
      bot: { conj: 'carmal', itens: ['item-canal-cajado', 'item-armadura-gibao'], mao: ['bal-devolver-c2-repentina', 'base-dano-c1-repentina'], campo: ['base-aliado-c2-agressivo'], grau: ['base-recurso', 'base-recurso'], arcano: 4, dano: { 'base-aliado-c2-agressivo': 2 } },
    }),
    passos: [
      { titulo: 'O que é a pilha', texto: 'A <b>pilha</b> é a fila do que está esperando para acontecer. Ela aparece aqui na direita. Toda carta jogada, toda habilidade ativada e todo <b>ataque</b> entram na pilha antes de fazer efeito. Enquanto algo está na pilha, os dois jogadores podem <b>responder</b> com cartas Repentinas.', botao: true },
      { titulo: 'Jogue uma carta', texto: `O Lobo-de-Cinza do oponente está ferido: só lhe resta 1 de vida. Jogue <b>${N('base-dano-c1')}</b> (2 de dano) nele. Ela não resolve na hora: vai para a pilha e espera.`, objetivo: `Jogue ${N('base-dano-c1')} no Lobo do oponente.`, feito: (e) => e.pilha.some((p) => p.controlador === EU), destaque: (e) => [maoDe(e, EU, 'base-dano-c1')], alvos: (e) => [campoDe(e, BOT, 'base-aliado-c2-agressivo')], acoes: [{ tipo: 'jogar', def: 'base-dano-c1' }] },
      { titulo: 'O oponente respondeu', texto: `Quem joga algo passa a <b>prioridade</b> (a vez de responder) para o outro. O oponente respondeu com <b>${N('bal-devolver-c2-repentina')}</b>: ela está <b>por cima</b> da sua carta e vai devolver o Lobo para a mão dele antes do seu dano chegar. Responda por cima com <b>${N('base-dano-c1-repentina')}</b> (1 de dano, Repentina) no Lobo.`, objetivo: `Responda com ${N('base-dano-c1-repentina')} no Lobo do oponente.`, feito: (e) => e.pilha.length >= 3, destaque: (e) => [maoDe(e, EU, 'base-dano-c1-repentina')], alvos: (e) => [campoDe(e, BOT, 'base-aliado-c2-agressivo')], acoes: [{ tipo: 'jogar', def: 'base-dano-c1-repentina' }], travaPassar: 'Responda primeiro: jogue a Faísca de Tempestade no Lobo, por cima da carta do oponente.' },
      { titulo: 'De cima para baixo', texto: `Agora há 3 cartas na pilha. Quando os dois jogadores passam seguidos, a carta <b>de cima</b> resolve, e assim por diante. Ordem: 1º a sua Faísca (mata o Lobo), 2º a ${N('bal-devolver-c2-repentina')} (o alvo sumiu: não faz nada), 3º a sua ${N('base-dano-c1')} (também sem alvo). Clique em <b>Passar</b> até a pilha esvaziar.`, objetivo: 'Passe e veja a pilha resolver.', feito: (e) => !e.pilha.length && !campoDe(e, BOT, 'base-aliado-c2-agressivo'), passar: true },
      { titulo: 'Ataques também usam a pilha', texto: `Ataque o Conjurador do oponente com o seu <b>Lobo-de-Cinza</b>. O ataque entra na pilha ("Ataque de Você") e o oponente ainda pode responder antes de escolher se bloqueia. Depois passe até o combate acabar.`, objetivo: 'Ataque o Conjurador do oponente e passe até o dano acontecer.', feito: (e) => danoConj(e, BOT) > 0 && !e.combate && !e.pilha.length, destaque: (e) => [campoDe(e, EU, 'base-aliado-c2-agressivo')], alvos: (e) => [e.jogadores[BOT].conjuradorId], acoes: [{ tipo: 'atacar', def: 'base-aliado-c2-agressivo' }] },
      { titulo: 'Resumo da pilha', texto: 'O oponente respondeu ao ataque com uma Faísca no seu Lobo, e ela resolveu antes do ataque. Lembre: <b>1)</b> tudo entra na pilha; <b>2)</b> quem joga passa a prioridade; <b>3)</b> quando os dois passam seguidos, a de cima resolve; <b>4)</b> carta sem alvo quando vai resolver não faz nada. Cartas Findantes só entram com a pilha vazia na sua fase Principal; Repentinas entram a qualquer momento em que você tem a prioridade.', botao: true },
      { titulo: 'Lição concluída!', texto: 'Próxima lição: anular cartas, pagar para anular e olhar o topo do baralho.', fim: true },
    ],
    bot: (e, passo) => {
      if (e.pendencia?.tipo === 'bloqueio') return { tipo: 'bloquear', jogador: BOT, bloqueador: null };
      const topo = e.pilha[e.pilha.length - 1];
      const lobo = campoDe(e, BOT, 'base-aliado-c2-agressivo');
      const porta = maoDe(e, BOT, 'bal-devolver-c2-repentina');
      // responde à Chuva de Faíscas devolvendo o próprio Lobo
      if (passo === 2 && porta && lobo && topo?.controlador === EU && e.pilha.length === 1) return { tipo: 'jogar', jogador: BOT, carta: porta, alvo: lobo, enviarArcano: [] };
      // responde ao ataque com uma Faísca no Lobo de quem ataca
      const faisca = maoDe(e, BOT, 'base-dano-c1-repentina');
      if (passo === 4 && faisca && topo?.tipo === 'ataque' && topo.controlador === EU) return { tipo: 'jogar', jogador: BOT, carta: faisca, alvo: topo.atacante, enviarArcano: [] };
      return { tipo: 'passar', jogador: BOT };
    },
  },
  {
    id: 'pilha', titulo: 'Respostas na pilha', resumo: 'Cartas Repentinas, anular, pagar para anular e olhar o topo do baralho.',
    cenario: () => montarCenario({
      ativo: BOT,
      eu: { conj: 'mitonho', itens: ['item-canal-cajado', 'item-armadura-gibao'], mao: ['bal-anular-c3-repentina', 'base-recurso', 'base-comprar-c1', 'base-aliado-c4-defensivo'], campo: ['base-aliado-c2-agressivo'], grau: ['base-aliado-c2-defensivo', 'base-aliado-c2-defensivo', 'base-aliado-c2-defensivo'], topo: ['base-recurso', 'base-recurso', 'base-recurso', 'r6-colosso-c7', 'base-dano-c3', 'base-curar-c2'] },
      bot: { conj: 'lyandra', itens: ['item-canal-cajado', 'item-armadura-gibao'], mao: ['base-dano-c2', 'col-sino-dos-juramentados', 'base-recurso'], grau: ['base-recurso', 'base-recurso'], arcano: 6 },
    }),
    passos: [
      { titulo: 'O turno do oponente', texto: 'Agora é a vez do oponente. Cartas <b>Findantes</b> só se jogam na sua fase Principal. Cartas <b>Repentinas</b> (como Anular) se jogam sempre que você tem a prioridade, até no turno do oponente.', botao: true },
      { titulo: 'Responda!', texto: `O oponente jogou uma carta no seu Aliado: ela aparece aqui na direita, na pilha, esperando a sua resposta. Jogue <b>${N('bal-anular-c3-repentina')}</b> (anula uma carta na pilha) e mire a carta dele. Pague com o seu Grau.`, objetivo: 'Anule a carta do oponente.', feito: (e) => noLog(e, 'foi anulada'), destaque: (e) => [maoDe(e, EU, 'bal-anular-c3-repentina')], alvos: (e) => e.pilha.filter((p) => p.controlador === BOT).flatMap((p) => [p.id, p.instId]), acoes: [{ tipo: 'jogar', def: 'bal-anular-c3-repentina' }], travaPassar: 'Responda primeiro: jogue o Anular na carta do oponente (senão o seu Aliado morre).' },
      { titulo: 'De cima para baixo', texto: 'O Anular entrou por cima e resolveu primeiro, tirando a carta do oponente da pilha. É assim sempre: a última carta posta resolve primeiro.', botao: true },
      { titulo: 'Pagar para anular', texto: `Agora ele jogou <b>${N('col-sino-dos-juramentados')}</b>: 5 de dano no seu Conjurador. Essa carta deixa você pagar 3 de Arcano para anulá-la quando ela for resolver. Passe a prioridade e, quando o jogo perguntar, pague enviando a ${N('base-recurso')} ao Arcano.`, objetivo: 'Pague 3 e anule o Sino.', feito: (e) => noLog(e, 'Jogador 0 paga 3'), falhou: (e) => noLog(e, 'Sino dos Juramentados resolve'), passar: true, destaque: (e) => (e.pendencia?.tipo === 'pagarAnulacao' ? [maoDe(e, EU, 'base-recurso')] : []), acoes: [{ tipo: 'pagarAnulacao', pagar: true }] },
      { titulo: 'Olhar o topo', texto: `De volta ao seu turno, você repôs a mão até 5 cartas. Jogue <b>${N('base-comprar-c1')}</b>: olhe as 3 cartas do topo, ponha 1 na mão e o resto vai para o fundo.`, objetivo: 'Escolha uma carta do topo.', feito: (e) => noLog(e, 'Jogador 0 põe 1 carta(s) na mão'), destaque: (e) => [maoDe(e, EU, 'base-comprar-c1')], acoes: [{ tipo: 'jogar', def: 'base-comprar-c1' }, { tipo: 'escolherDoTopo' }] },
      { titulo: 'Lição concluída!', texto: 'Você domina a pilha. Dica: em Opções da mesa dá para mandar o jogo sempre esperar seu clique quando o oponente jogar algo.', fim: true },
    ],
    bot: (e, passo) => {
      if (e.pendencia?.tipo === 'bloqueio') return { tipo: 'bloquear', jogador: BOT, bloqueador: null };
      if (e.ativo !== BOT || e.etapa !== 'principal') return { tipo: 'passar', jogador: BOT };
      if (passo === 0) return 'esperar';
      const brasa = maoDe(e, BOT, 'base-dano-c2');
      const lobo = campoDe(e, EU, 'base-aliado-c2-agressivo');
      if (passo === 1 && brasa && lobo && !e.pilha.length) return { tipo: 'jogar', jogador: BOT, carta: brasa, alvo: lobo, enviarArcano: [] };
      if (passo === 2) return e.pilha.length ? { tipo: 'passar', jogador: BOT } : 'esperar';
      const sino = maoDe(e, BOT, 'col-sino-dos-juramentados');
      if (passo === 3 && sino && !e.pilha.length) return { tipo: 'jogar', jogador: BOT, carta: sino, enviarArcano: [] };
      return { tipo: 'passar', jogador: BOT };
    },
  },
  {
    id: 'habilidades', titulo: 'Itens, estruturas e habilidades', resumo: 'O Extra consumível, Estruturas ativadas, a habilidade do Conjurador, Cataclisma e as regras que faltam.',
    cenario: () => montarCenario({
      eu: { conj: 'roh', vida: 13, itens: ['item-canal-cajado', 'item-armadura-gibao', 'item-extra-pocao'], mao: ['base-recurso', 'base-aliado-c2-agressivo', 'bal-etereo-c2'], campo: ['base-estrutura-c4-torre', 'base-aliado-c4-agressivo'], grau: ['base-recurso', 'base-recurso', 'base-recurso', 'base-recurso'], dano: { 'base-aliado-c4-agressivo': 4 } },
      bot: { conj: 'osmar', itens: ['item-canal-cajado', 'item-armadura-gibao'], campo: ['bal-etereo-c2'], grau: ['base-recurso', 'base-recurso'] },
    }),
    passos: [
      { titulo: 'Itens do Conjurador', texto: 'Antes da partida cada jogador escolhe do side deck um <b>Canal</b> (arma), uma <b>Armadura</b> e um <b>Extra</b> (consumível de uso único). Eles ficam ao lado do Conjurador. Use a <b>Poção de cura</b>: clique no botão Extra embaixo do seu Conjurador.', objetivo: 'Use a Poção de cura.', feito: (e) => !e.jogadores[EU].itens.some((i) => e.instancias[i].defId === 'item-extra-pocao'), destaque: (e) => [e.jogadores[EU].conjuradorId], acoes: [{ tipo: 'ativar', def: 'item-extra-pocao' }] },
      { titulo: 'Estruturas', texto: `Estruturas ficam em campo e não são atacadas, só atingidas por magias. A <b>${N('base-estrutura-c4-torre')}</b> diz: "Suspenda e pague 1: cause 2 de dano a um alvo". Use-a no Aliado Etéreo do oponente (Etéreo só é bloqueado por Etéreos).`, objetivo: 'Destrua o Aliado Etéreo do oponente com a Torre.', feito: (e) => e.jogadores[BOT].campo.length === 0, destaque: (e) => [campoDe(e, EU, 'base-estrutura-c4-torre')], alvos: (e) => e.jogadores[BOT].campo, acoes: [{ tipo: 'ativar', def: 'base-estrutura-c4-torre' }] },
      { titulo: 'Habilidade do Conjurador', texto: `Cada Conjurador tem uma habilidade. A de Roh: suspenda o Conjurador e devolva à mão um Aliado ou Mística seu. O <b>${N('base-aliado-c4-agressivo')}</b> está ferido (o dano fica guardado). Clique no Roh, <b>Usar habilidade</b>, e devolva o ${N('base-aliado-c4-agressivo')}: na mão ele perde o dano.`, objetivo: 'Devolva o Aliado ferido para a mão.', feito: (e) => !!maoDe(e, EU, 'base-aliado-c4-agressivo'), destaque: (e) => [e.jogadores[EU].conjuradorId], alvos: (e) => [campoDe(e, EU, 'base-aliado-c4-agressivo')], acoes: [{ tipo: 'ativar', conj: true }] },
      { titulo: 'Fim do turno', texto: 'Ao encerrar: no <b>Cataclisma</b> o Arcano que sobrou na reserva some; no <b>Final</b> tudo que é seu levanta, o Grau vira para baixo e as cartas enviadas ao Arcano vão para o fundo do baralho. No Início do seu turno você repõe a mão até 5. Encerre a fase Principal.', objetivo: 'Passe o turno.', feito: (e) => e.ativo === BOT, passar: true },
      { titulo: 'Regras para lembrar', texto: '<b>Limite de mão</b>: 7. <b>Impasse</b>: 5 turnos seguidos sem jogar, ativar ou atacar e você perde. <b>Protegido N</b>: mirar a carta custa N a mais. Algumas cartas pedem uma carta de certo Valor no Grau, ou descartar cartas do Grau. O <b>Banimento</b> guarda cartas fora de jogo (a Ana pode jogá-las depois). Baralho acabado não faz perder.', botao: true },
      { titulo: 'Tutorial completo!', texto: 'Agora jogue uma partida inteira contra o bot com dicas, ou vá direto para o modo Jogar.', fim: true },
    ],
    bot: () => ({ tipo: 'passar', jogador: BOT }),
  },
];

const progresso = () => local.ler('tutorial', {});
/** Passo só de leitura (sem objetivo a cumprir): avança pelo botão Próximo. */
const temProximo = (p) => !p.feito && !p.fim;

export async function telaTutorial(tela, partes, ctx) {
  ctx.fundo(cenario('tutorial'));
  const id = partes[0];
  if (id === 'partida') return partidaComDicas(tela, ctx);
  const licao = LICOES.find((l) => l.id === id);
  if (!licao) {
    const p = progresso();
    tela.innerHTML = `<h1>Tutorial</h1><p class="lema-peq">Cinco lições rápidas contra o bot e uma partida completa com dicas. Cada lição usa as regras de verdade.</p>
      <div class="grade g2 licoes">${LICOES.map((l, i) => `<a class="cartao-modo licao ${p[l.id] ? 'feita' : ''}" href="#/tutorial/${l.id}" style="--cena:url('${cenario(['tutorial', 'mesa', 'saguao', 'baralhos', 'comunidade'][i % 5])}')"><div class="icone">${p[l.id] ? '✅' : `${i + 1}`}</div><h3>${esc(l.titulo)}</h3><p>${esc(l.resumo)}</p></a>`).join('')}
      <a class="cartao-modo licao" href="#/tutorial/partida" style="--cena:url('${cenario('menu')}')"><div class="icone">🎓</div><h3>Partida completa com dicas</h3><p>Lyandra contra Osmar, com dicas do que fazer a cada momento.</p></a></div>`;
    return;
  }
  tela.classList.add('cheia');
  tela.innerHTML = '<div id="mesa-raiz"></div>';
  let passo = 0;
  let mesa = null;
  const e0 = licao.cenario();
  const fonte = criarPartidaLocal({ estado: e0, modo: 'tutorial', nomes: ['Você', 'Bot do tutorial'], estiloBot: prontoDe(e0.instancias[e0.jogadores[BOT].conjuradorId].defId).estilo, roteiroBot: (e) => licao.bot(e, passo) });
  const atual = () => licao.passos[passo];
  const indice = LICOES.indexOf(licao);
  fonte.tutorial = {
    balao() {
      const p = atual();
      return `<div class="balao-tutorial"><div class="passo">Lição ${indice + 1} · passo ${passo + 1} de ${licao.passos.length}</div><h3>${esc(p.titulo)}</h3><div>${p.texto}</div>
        ${p.objetivo ? `<div class="objetivo">🎯 ${esc(p.objetivo)}</div>` : ''}
        <div class="linha-botoes">${temProximo(p) ? '<button class="botao primario peq" data-tut-prox>Próximo ▸</button>' : ''}
        ${p.fim ? `${LICOES[indice + 1] ? `<a class="botao primario peq" href="#/tutorial/${LICOES[indice + 1].id}">Próxima lição ▸</a>` : '<a class="botao primario peq" href="#/tutorial/partida">Partida com dicas ▸</a>'}<a class="botao peq" href="#/tutorial">Lições</a>` : ''}
        <button class="botao peq fantasma" data-tut-reiniciar>Recomeçar lição</button></div></div>`;
    },
    /** Lição rígida (pedido do autor, 04/10): só brilha e só vale o que o passo pede. */
    restrito: true,
    destaques() { return atual().destaque?.(fonte.e)?.filter(Boolean) ?? []; },
    /** Alvos aceitos no passo (null = qualquer). */
    alvos() { const a = atual().alvos?.(fonte.e); return a ? a.filter(Boolean) : null; },
    permitir(cmd) { return permitirTutorial(fonte.e, atual(), cmd, travado()); },
    semAutoPasse() { return travado() || (temProximo(atual()) && fonte.e.ativo === EU); },
  };
  /** O passo pede uma resposta e a carta do oponente ainda está no topo da pilha. */
  function travado() { const p = atual(); const topo = fonte.e.pilha[fonte.e.pilha.length - 1]; return !!p.travaPassar && !p.feito?.(fonte.e) && !!topo && topo.controlador !== EU; }
  const avancar = () => {
    const p = atual();
    if (p.falhou?.(fonte.e) && !p.feito?.(fonte.e)) { avisar('Não deu certo desta vez. Recomeçando a lição…', 'erro', 4000); setTimeout(() => ctx.navegar('tutorial/' + licao.id + '/' + Date.now()), 1200); return; }
    if (p.feito?.(fonte.e)) { passo = Math.min(passo + 1, licao.passos.length - 1); if (atual().fim) { const pr = progresso(); pr[licao.id] = true; local.gravar('tutorial', pr); } mesa?.render(); fonte.cutucar(); }
  };
  fonte.ouvir(() => setTimeout(avancar, 0));
  mesa = abrirMesa($('#mesa-raiz'), fonte, { aoSair: () => ctx.navegar('tutorial'), opinar: ctx.opinar, cena: cenario('tutorial') });
  const clique = (ev) => {
    if (ev.target.closest('[data-tut-prox]')) { passo = Math.min(passo + 1, licao.passos.length - 1); mesa.render(); fonte.cutucar(); }
    if (ev.target.closest('[data-tut-reiniciar]')) { location.hash = '#/tutorial/' + licao.id + '/' + Date.now(); }
  };
  tela.addEventListener('click', clique);
  ctx.definirLimpeza(() => { tela.removeEventListener('click', clique); fonte.parar(); mesa.fechar(); });
}

/** Partida completa contra o bot com dicas do que dá para fazer a cada momento. */
function partidaComDicas(tela, ctx) {
  tela.classList.add('cheia');
  tela.innerHTML = '<div id="mesa-raiz"></div>';
  const eu = prontoDe('lyandra'), op = prontoDe('osmar');
  const fonte = criarPartidaLocal({ eu: { conjurador: 'lyandra', cartas: listaParaCartas(eu.lista), itens: eu.itens }, bot: { conjurador: 'osmar', cartas: listaParaCartas(op.lista), itens: op.itens }, estiloBot: op.estilo, nomes: ['Você', 'Bot · Osmar'], modo: 'tutorial-partida' });
  let fechado = false;
  fonte.tutorial = {
    balao() {
      if (fechado) return '<button class="botao peq balao-reabrir" data-dicas>💡 Dicas</button>';
      return `<div class="balao-tutorial"><div class="passo">Partida com dicas</div><h3>O que fazer agora</h3><div>${dica(fonte.e)}</div><div class="linha-botoes"><button class="botao peq fantasma" data-dicas>Esconder dicas</button></div></div>`;
    },
  };
  const mesa = abrirMesa($('#mesa-raiz'), fonte, { aoSair: () => ctx.navegar('tutorial'), opinar: ctx.opinar });
  const clique = (ev) => { if (ev.target.closest('[data-dicas]')) { fechado = !fechado; mesa.render(); } };
  tela.addEventListener('click', clique);
  ctx.definirLimpeza(() => { tela.removeEventListener('click', clique); fonte.parar(); mesa.fechar(); });
}

function dica(e) {
  const z = e.jogadores[EU];
  const p = e.pendencia;
  if (e.vencedor !== null) return e.vencedor === EU ? 'Você venceu! Agora experimente montar o seu próprio baralho.' : 'Não foi dessa vez. Tente de novo: suba o Grau todo turno e guarde Aliados para bloquear.';
  if (p?.jogador === EU) return ({ escolherInicio: 'Escolha quem começa. Quem começa não põe carta no Grau no turno 1, mas joga primeiro.', bloqueio: 'Um ataque vai acertar. Bloqueie com um Aliado de pé que aguente o dano, ou deixe passar se o dano for pequeno.', descartar: 'Sua mão passou de 7: descarte o excesso.', pagarAnulacao: 'Você pode pagar para anular essa carta. Vale a pena se o efeito for forte.', escolherDoTopo: 'Escolha a carta mais útil agora; as outras vão para o fundo.' })[p.tipo] ?? 'Decida.';
  if (p) return 'O oponente está decidindo…';
  if (e.pilha.length) { const t = e.pilha[e.pilha.length - 1]; return t.controlador === EU ? 'Sua jogada está na pilha. O oponente pode responder.' : 'O oponente jogou algo. Se tiver uma carta Repentina que ajude (ela brilha), jogue; senão, passe e deixe resolver.'; }
  if (e.ativo !== EU) return 'Turno do oponente. Observe: você pode responder com cartas Repentinas quando ele jogar algo.';
  if (e.etapa !== 'principal') return 'Etapas automáticas: o jogo passa sozinho.';
  const dicas = [];
  if (!e.grauNoTurno && !(e.primeiro === EU && e.turno === 1)) dicas.push('Ponha uma carta no Grau (de preferência uma que você não vai jogar logo).');
  const jogaveis = z.mao.filter((id) => { const d = DADOS.catalogo.cartas[e.instancias[id].defId]; return d.grau <= grauEfetivo(e, EU); });
  if (jogaveis.length) dicas.push(`Você tem ${jogaveis.length} carta(s) com Grau suficiente. As que brilham dá para pagar agora.`);
  if (z.campo.some((id) => DADOS.catalogo.cartas[e.instancias[id].defId].tipo === 'aliado' && !e.instancias[id].suspensa && !recemChegado(e, id))) dicas.push('Seus Aliados de pé podem atacar. Deixe algum de pé se precisar bloquear no turno do oponente.');
  if (!e.instancias[z.conjuradorId].suspensa) dicas.push('Lyandra pode atacar com a arma (Canal).');
  dicas.push('Quando terminar, clique em Encerrar fase Principal.');
  return dicas.map((d) => `• ${esc(d)}`).join('<br>');
}

/** Trava da lição: devolve a mensagem quando o comando não faz parte do passo atual (null = pode). */
function permitirTutorial(e, p, cmd, travado) {
  const faca = p.objetivo ? `No tutorial, agora: ${p.objetivo}${p.destaque?.(e)?.some(Boolean) ? ' Use o que está brilhando.' : ''}` : 'Leia o balão e clique em Próximo para continuar.';
  if (p.fim) return 'Lição concluída! Use os botões do balão para seguir.';
  // pagar custos (suspender o Grau) e as decisões obrigatórias do jogo sempre valem
  if (['suspenderGrau', 'escolherInicio', 'descartar'].includes(cmd.tipo)) return null;
  const defDe = (id) => (id ? e.instancias[id]?.defId : null);
  // fontes de Arcano (ativação que só gera Arcano) servem para pagar
  if (cmd.tipo === 'ativar') { const d = DADOS.catalogo.cartas[defDe(cmd.carta)]; if (d?.ativacao?.efeitos?.every((ef) => ef.tipo === 'arcano')) return null; }
  if (cmd.tipo === 'passar') {
    if (travado) return p.travaPassar;
    if (p.passar || e.pendencia) return null;
    // deixar resolver o que já está na pilha, o combate, e o turno do oponente
    if (e.pilha.length || e.combate || e.ativo !== EU || e.etapa !== 'principal') return null;
    return faca;
  }
  if (!p.acoes) return faca;
  const alvosOk = p.alvos?.(e)?.filter(Boolean) ?? null;
  const ok = p.acoes.some((a) => {
    if (a.tipo !== cmd.tipo) return false;
    const id = cmd.carta ?? cmd.atacante ?? cmd.bloqueador ?? null;
    if (a.conj && id !== e.jogadores[EU].conjuradorId) return false;
    if (a.def && defDe(id) !== a.def) return false;
    if ('pagar' in a && cmd.pagar !== a.pagar) return false;
    if (alvosOk && cmd.alvo && !alvosOk.includes(cmd.alvo)) return false;
    if (alvosOk && cmd.tipo === 'atacar' && !alvosOk.includes(cmd.alvo)) return false;
    return true;
  });
  return ok ? null : faca;
}

export { conjuradorHTML, nomeCurto };
