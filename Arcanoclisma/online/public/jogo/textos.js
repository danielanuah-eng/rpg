// Textos gerados a partir da mecânica das cartas e dos Conjuradores (usados pelo app de testes e pelo editor).
export const VALOR = { conflituo: 'Conflitüo', sublimeyn: 'Sublimeyn', dominum: 'Dominum', persona: 'Persona', alvorum: 'Alvorum', morbidyum: 'Morbidyum', cognitium: 'Cognitium', eternyr: 'Eternyr', primitivum: 'Primitivum' };
export const TIPO = { aliado: 'Aliado', temporaria: 'Temporária', mistica: 'Mística', estrutura: 'Estrutura', item: 'Item' };
export const ALVO = { escolhido: 'um alvo', escolhidoAliado: 'um Aliado ou Mística', escolhidoProprio: 'um Aliado ou Mística seu', conjuradorInimigo: 'o Conjurador inimigo', conjuradorProprio: 'o seu Conjurador', pilha: 'uma carta na pilha' };

/** "a" + alvo, contraindo "a o" em "ao" (e "de o" em "do"). */
const a = (alvo) => (ALVO[alvo].startsWith('o ') ? 'ao ' + ALVO[alvo].slice(2) : 'a ' + ALVO[alvo]);
const de = (alvo) => (ALVO[alvo].startsWith('o ') ? 'do ' + ALVO[alvo].slice(2) : 'de ' + ALVO[alvo]);
/**
 * Requisito de uma linha (04/10): "(3+ no Grau · Pague 2)". As palavras "no Grau" e "Pague N" ligam a iconografia
 * de Grau e de pagar Arcano (online/public/js/icones.js) quando o GPT entregar os ícones.
 */
export function textoRequisito(r) {
  if (!r || (!r.grau && !r.arcano)) return '';
  return '(' + [r.grau ? `${r.grau}+ no Grau` : '', r.arcano ? `Pague ${r.arcano}` : ''].filter(Boolean).join(' · ') + ') ';
}
const ESCOLHA_TXT = { afetado: ' (o dono da carta atingida escolhe qual)', aleatorio: ' (ao acaso)' };
export function descEfeito(ef) {
  return textoRequisito(ef.requisito) + descEfeitoBase(ef) + (ef.escolha && ef.escolha !== 'controlador' && (ef.alcance !== 'todos') ? ESCOLHA_TXT[ef.escolha] : '');
}
function descEfeitoBase(ef) {
  switch (ef.tipo) {
    case 'dano': return `cause ${ef.valor}${ef.escalaGrau ? ' + o número de cartas no seu Grau' : ''} de dano ${a(ef.alvo)}`;
    case 'curar': return `cure ${ef.valor} de dano ${de(ef.alvo)}`;
    case 'destruir': return `destrua ${ALVO[ef.alvo]}`;
    case 'devolver': return `devolva ${ALVO[ef.alvo]} à mão do dono`;
    case 'danoTodosAliados': return `cause ${ef.valor} de dano a cada Aliado ${ef.ambos ? '(inclusive os seus)' : 'do oponente'}`;
    case 'destruirTodosAliados': return `destrua todos os Aliados${ef.ambos ? '' : ' do oponente'}`;
    case 'anular': return 'anule uma carta na pilha';
    case 'comprar': return `compre ${ef.valor} carta${ef.valor > 1 ? 's' : ''}`;
    case 'arcano': return `ganhe ${ef.valor} de Arcano`;
    case 'arcanoOponente': return `o oponente ganha ${ef.valor} de Arcano`;
    case 'descontoProximaCarta': return `a primeira carta que você jogar neste turno custa ${ef.valor} a menos`;
    case 'banirDaMao': return `bana uma carta da sua mão; no seu próximo turno ela pode ser jogada custando ${ef.desconto} a menos`;
    case 'reanimar': return `ponha no seu campo um Aliado de qualquer cemitério, pagando o custo dele + ${ef.custoExtra}${ef.concessao ? `; ele ${textoConcessao(ef.concessao, 'ele')}` : ''}`;
    case 'olharTopo': return `olhe as ${ef.quantidade} cartas do topo do seu baralho; ponha ${ef.pegar} na mão e as outras no fundo do baralho`;
    case 'permitirAnularDoCemiterio': return 'neste turno você pode jogar do cemitério uma carta que anula outra (ela é banida ao resolver)';
    case 'potenciaMagia': return `o próximo dano ou cura de uma Temporária sua neste turno tem +${ef.valor}`;
    case 'invocar': return `invoque ${ef.nome ?? 'uma criatura'}${ef.concessao ? ` (${textoConcessao(ef.concessao, 'ela')})` : ''}`;
    case 'ataqueConjuradorTurno': return `seu Conjurador ganha +${ef.valor} de Ataque neste turno`;
    // construtor de efeitos (04/10)
    case 'perderVida': return `seu Conjurador perde ${ef.valor} de Vida`;
    case 'descartar': return `${ef.quem === 'voce' ? 'você descarta' : 'o oponente descarta'} ${ef.valor} carta${ef.valor > 1 ? 's' : ''}${ef.aleatorio ? ' ao acaso' : ''}`;
    case 'colocarNoGrau': return `ponha ${ef.valor} carta${ef.valor > 1 ? 's' : ''} ${ef.de === 'topo' ? 'do topo do seu baralho' : 'da sua mão'} no seu Grau`;
    case 'olharTopoPara': return `olhe as ${ef.quantidade} cartas do topo do seu baralho; ponha ${ef.pegar} ${DESTINO[ef.destino]} e as outras ${DESTINO[ef.resto]}`;
    case 'olharMao': return ef.pegar > 0 && ef.destino !== 'nada' ? `olhe a mão do oponente e mande ${ef.pegar} carta${ef.pegar > 1 ? 's' : ''} dela ${PARA[ef.destino]}` : 'olhe a mão do oponente';
    case 'jogarDe': return `ponha em campo, sem pagar, ${ef.valor > 1 ? `até ${ef.valor} cartas` : 'uma carta'} (${(ef.tipos?.length ? ef.tipos : ['aliado']).map((t) => TIPO_PL[t] ?? t).join(' ou ')}${ef.custoMax !== undefined ? `, custo até ${ef.custoMax}` : ''}) ${ORIGEM[ef.de]}${ef.concessao ? `; elas ${textoConcessao(ef.concessao, 'elas')}` : ''}`;
    case 'campo': return textoCampo(ef);
    default: return ef.tipo;
  }
}
/** Maiúscula na primeira letra (depois do requisito entre parênteses, se houver). */
const maiuscula = (t) => t.replace(/^(\([^)]*\)\s*)?(.)/, (m, p, c) => (p ?? '') + c.toUpperCase());
const DESTINO = { mao: 'na mão', roubar: 'na sua mão', grau: 'no Grau', cemiterio: 'no cemitério', fundo: 'no fundo do baralho', topo: 'no topo do baralho', campo: 'em campo', banimento: 'no banimento', nada: 'onde estão' };
const PARA = { mao: 'para a mão do dono', roubar: 'para a sua mão', grau: 'para o Grau', cemiterio: 'para o cemitério', fundo: 'para o fundo do baralho', topo: 'para o topo do baralho', campo: 'para o campo', banimento: 'para o banimento', nada: '' };
const ORIGEM = { cemiterio: 'do seu cemitério', mao: 'da sua mão', baralho: 'do seu baralho', banimento: 'do seu banimento' };
const TIPO_PL = { aliado: 'Aliado', mistica: 'Mística', estrutura: 'Estrutura', conjurador: 'Conjurador', equipamento: 'equipamento', temporaria: 'Temporária' };
const SAIDA = { cemiterio: 'o cemitério', banimento: 'o banimento', baralho: 'o baralho do dono, que embaralha', mao: 'a mão do dono' };
/**
 * Texto de uma concessão (04/10): "ganha Etéreo e +1/-1 por 2 turnos; depois vai para o banimento".
 * Os turnos são do controlador e o da entrada conta.
 */
export function textoConcessao(c, quem = 'ele') {
  const partes = [];
  if (c.palavras?.length) partes.push(c.palavras.join(', ') + (c.palavrasTurnos ? ` por ${c.palavrasTurnos} turno${c.palavrasTurnos > 1 ? 's' : ''}` : ''));
  const sinal = (v) => (v > 0 ? '+' : '') + v;
  if (c.ataque && c.vida) partes.push(`${sinal(c.ataque)}/${sinal(c.vida)}`);
  else if (c.ataque) partes.push(`${sinal(c.ataque)} de Ataque`);
  else if (c.vida) partes.push(`${sinal(c.vida)} de Vida`);
  let t = partes.length ? `ganha ${partes.join(' e ')}` : 'fica';
  if (c.turnos) t += `${partes.length ? ' e fica' : ''} ${c.turnos} turno${c.turnos > 1 ? 's' : ''} em campo (o turno da entrada conta como o 1º); depois vai para ${SAIDA[c.destino ?? 'cemiterio']}`;
  return quem === 'elas' ? t.replace(/^ganha /, 'ganham ').replace(/ fica /, ' ficam ').replace(/^fica /, 'ficam ').replace(/vai para/, 'vão para') : t;
}
/** Texto do efeito "campo": ação + quem (um alvo ou todos, de que lado, de que tipos). */
function textoCampo(ef) {
  const tipos = ef.tipos.map((t) => TIPO_PL[t] ?? t);
  const lista = tipos.length > 1 ? tipos.slice(0, -1).join(', ') + ' ou ' + tipos.at(-1) : tipos[0] ?? 'carta';
  const lado = ef.lado === 'inimigo' ? (ef.alcance === 'todos' ? ' inimigos' : ' inimigo') : ef.lado === 'proprio' ? (ef.alcance === 'todos' ? ' seus' : ' seu') : '';
  const quem = ef.alcance === 'todos' ? `cada ${lista}${lado ? lado.replace('inimigos', 'inimigo').replace('seus', 'seu') : ''}` : `um ${lista}${lado}`;
  const tempo = ef.duracao === 'permanente' ? '' : ' neste turno';
  switch (ef.acao) {
    case 'dano': return `cause ${ef.valor ?? 0} de dano a ${quem}`;
    case 'curar': return `cure ${ef.valor ?? 0} de dano de ${quem}`;
    case 'destruir': return `destrua ${quem}`;
    case 'devolver': return `devolva ${quem} à mão do dono`;
    case 'paraBaralho': return `mande ${quem} ${ef.onde === 'topo' ? 'para o topo do baralho do dono' : ef.onde === 'fundo' ? 'para o fundo do baralho do dono' : 'de volta ao baralho do dono, que embaralha'}`;
    case 'naoAtaca': return `${quem} não pode atacar até o fim do próximo turno do dono`;
    case 'suspender': return `suspenda ${quem}`;
    case 'levantar': return `levante ${quem}`;
    case 'aumentarAtaque': return `${quem} ganha +${ef.valor ?? 0} de Ataque${tempo}`;
    case 'aumentarVida': return `${quem} ganha +${ef.valor ?? 0} de Vida${tempo}`;
    case 'conceder': return `${quem} ${textoConcessao(ef.concessao ?? {}, 'ele')}`;
    default: return ef.acao;
  }
}
export const frase = (efs) => maiuscula(efs.map(descEfeito).join(' e ')) + '.';
const reqAtivacao = (a) => (a.requisito?.grau ? `com ${a.requisito.grau}+ no Grau, ` : '');
const custoAtivacao = (a) => (a.custo || a.requisito?.arcano ? `pague ${(a.custo ?? 0) + (a.requisito?.arcano ?? 0)}` : '');
/** Frase no meio do texto (depois de dois-pontos): começa com minúscula sem rebaixar nomes próprios como Arcano. */
const fraseMin = (efs) => efs.map(descEfeito).join(' e ') + '.';

export function textoCarta(d) {
  const p = [];
  if (d.tipo === 'item') {
    const esp = { canal: 'Canal', armadura: 'Armadura', extra: 'Extra' }[d.categoriaItem] ?? 'Item';
    const canal = { arma: 'Arma', foco: 'Foco Arcano', transcritor: 'Transcritor' }[d.tipoCanal ?? (d.ataque ? 'arma' : '')] ?? 'Arma';
    p.push(d.categoriaItem === 'armadura' ? `${esp}. Defesa ${d.defesa ?? 0}, desgaste ${d.vidaDesgaste ?? 0}.`
      : d.categoriaItem === 'canal' ? (canal === 'Arma' ? `${esp}. Arma: Ataque ${d.ataque ?? 0}.` : `${esp}. ${canal}.`)
      : `${esp}${d.competencia ? ` de ${d.competencia}` : ''}. Uso único.`);
    // custo e Grau do item (pedido do autor, 04/10): para atacar (arma), para proteger (armadura) ou para usar
    const req = [d.grau ? `Grau ${d.grau}` : '', d.custo && !d.ativacao ? `pague ${d.custo}` : ''].filter(Boolean).join(' e ');
    if (canal === 'Arma' && d.categoriaItem === 'canal') p.push(req ? `Para atacar: ${req}.` : 'Atacar é de graça.');
    else if (d.categoriaItem === 'armadura') { if (req) p.push(`Para proteger: ${req}.`); }
    else if (d.grau) p.push(`Para usar: Grau ${d.grau}.`);
    const aa = d.aoAtacar;
    if (aa?.efeitos?.length) p.push('Ao atacar com ela: ' + fraseMin(aa.efeitos));
    if (aa?.danoUnidades) p.push(`+${aa.danoUnidades} de dano quando acerta Aliados, Místicas ou Estruturas.`);
    if (aa?.desgasteArmadura) p.push(`Desgasta ${aa.desgasteArmadura} a mais a Armadura inimiga que protege deste ataque.`);
    if (d.aoProteger?.length) p.push('Ao proteger: ' + fraseMin(d.aoProteger));
    if (d.espinhos) p.push(`Ao proteger, causa ${d.espinhos} de dano ao atacante.`);
  }
  if (d.palavrasChave?.length) p.push(d.palavrasChave.join(', ') + '.');
  if (d.protegido) p.push(`Protegido ${d.protegido}.`);
  if (d.grauExtra) p.push(`Conta como ${d.grauExtra} carta${d.grauExtra > 1 ? 's' : ''} no Grau.`);
  if (d.descartaGrau) p.push(`Para jogar, descarte ${d.descartaGrau} carta${d.descartaGrau > 1 ? 's' : ''} do seu Grau.`);
  if (d.efeitos?.length) p.push(frase(d.efeitos));
  if (d.aoEntrar?.length) p.push('Ao entrar: ' + fraseMin(d.aoEntrar));
  if (d.aoDanoConjurador?.length) p.push('Ao causar dano de combate ao Conjurador inimigo: ' + fraseMin(d.aoDanoConjurador));
  if (d.ativacao) {
    const a = d.ativacao;
    const custos = [a.suspendeConjurador ? 'suspenda o Conjurador' : a.suspende ? 'suspenda' : '', custoAtivacao(a), a.descartar ? `descarte ${a.descartar} carta${a.descartar > 1 ? 's' : ''}` : '', a.danoNoConjurador ? `seu Conjurador toma ${a.danoNoConjurador} de dano` : ''].filter(Boolean);
    const lista = custos.length > 1 ? custos.slice(0, -1).join(', ') + ' e ' + custos.at(-1) : custos[0] ?? (d.categoriaItem === 'extra' ? 'use' : 'ative');
    const l2 = reqAtivacao(a) + lista;
    p.push(`${l2.charAt(0).toUpperCase() + l2.slice(1)}: ${fraseMin(a.efeitos ?? [])}`);
  }
  if (d.pagarParaAnular !== undefined) p.push(`O oponente pode pagar ${d.pagarParaAnular} para anulá-la.`);
  for (const h of d.habilidades ?? []) p.push(textoHabilidade(h));
  if (d.protecao) p.push(textoProtecao(d.protecao));
  return p.join(' ');
}

/** Condições do construtor de efeitos (o mesmo texto aparece no Editor). */
export const QUANDO_TXT = {
  jogar: 'Ao jogar', entrar: 'Quando entra em campo', ativada: '', alvo: 'Quando vira alvo', grau: 'Quando é colocada no Grau',
  atacar: 'Quando ataca', bloquear: 'Quando defende', cemiterio: 'Quando cai no cemitério', banimento: 'Quando é banida', danoConjurador: 'Quando causa dano de combate ao Conjurador inimigo',
  // só de Conjurador (Editor de Conjuradores, 04/10)
  aliadoEntra: 'Quando um Aliado seu entra em campo', aliadoMorre: 'Quando um Aliado seu vai do campo para o cemitério', aliadoDanoConjurador: 'Quando um Aliado seu causa dano de combate ao Conjurador inimigo',
};
/** Texto de uma habilidade do construtor. `conj`: habilidade do poder de um Conjurador ("suspenda o Conjurador", "Repentina:"). */
export function textoHabilidade(h, conj = false) {
  const efs = fraseMin(h.efeitos ?? []);
  if (h.quando === 'ativada') {
    const c = h.custo ?? {};
    const arcano = (c.arcano ?? 0) + (h.requisito?.arcano ?? 0);
    const grau = Math.max(c.grau ?? 0, h.requisito?.grau ?? 0);
    const partes = [c.suspender ? (conj ? 'suspenda o Conjurador' : 'suspenda') : '', arcano ? `pague ${arcano}` : '', c.vida ? `pague ${c.vida} de Vida` : '', c.descartar ? `descarte ${c.descartar} carta${c.descartar > 1 ? 's' : ''}` : ''].filter(Boolean);
    const lista = partes.length > 1 ? partes.slice(0, -1).join(', ') + ' e ' + partes.at(-1) : partes[0] ?? 'ative';
    const pre = grau ? `Com ${grau}+ no Grau, ${lista}` : lista.charAt(0).toUpperCase() + lista.slice(1);
    return `${h.repentina ? 'Repentina: ' + pre.charAt(0).toLowerCase() + pre.slice(1) : pre}: ${efs}`;
  }
  const quando = conj && QUANDO_CONJ_TXT[h.quando] ? QUANDO_CONJ_TXT[h.quando] : QUANDO_TXT[h.quando] ?? h.quando;
  const extras = [h.umaVezPorTurno ? 'uma vez por turno' : '', h.soNoSeuTurno ? 'só no seu turno' : ''].filter(Boolean);
  return `${quando}${extras.length ? ` (${extras.join(', ')})` : ''}: ${efs}`;
}
/** Condições do próprio Conjurador no texto do poder. */
const QUANDO_CONJ_TXT = { atacar: 'Quando o Conjurador ataca', bloquear: 'Quando o Conjurador defende', alvo: 'Quando o Conjurador vira alvo', danoConjurador: 'Quando o Conjurador causa dano de combate ao Conjurador inimigo' };
const ZONA_TXT = { cemiterio: 'do seu cemitério', banimento: 'do seu banimento' };
/** Permissão contínua do Conjurador: "Você pode jogar Aliados de custo até 3 do seu cemitério; ...". */
export function textoPermissao(pm) {
  const tipos = (pm.tipos ?? []).map((t) => TIPO_PL[t] ?? t);
  const lista = tipos.length > 1 ? tipos.slice(0, -1).join(', ') + ' ou ' + tipos.at(-1) : tipos[0];
  const uma = !!pm.umaVezPorTurno;
  return `Você pode jogar ${uma ? 'uma carta' : 'cartas'}${lista ? ` (${lista})` : ''}${pm.custoMax !== undefined ? ` de custo até ${pm.custoMax}` : ''} ${ZONA_TXT[pm.de] ?? ZONA_TXT.cemiterio}${uma ? ' por turno' : ''}, pagando o custo${pm.banir ? (uma ? '; depois ela vai para o banimento' : '; depois elas vão para o banimento') : ''}.`;
}
export function textoProtecao(pr) {
  const partes = [pr.arcano ? `pagar ${pr.arcano} de Arcano` : '', pr.vida ? `pagar ${pr.vida} de Vida` : '', pr.descartar ? `descartar ${pr.descartar} carta${pr.descartar > 1 ? 's' : ''}` : '', pr.grauParaCemiterio ? `mandar ${pr.grauParaCemiterio} carta${pr.grauParaCemiterio > 1 ? 's' : ''} do Grau ao cemitério` : ''].filter(Boolean);
  const lista = partes.length > 1 ? partes.slice(0, -1).join(', ') + ' e ' + partes.at(-1) : partes[0] ?? '';
  return `Proteção mágica: para mirar esta carta, o oponente precisa${pr.grauMin ? ` ter ${pr.grauMin}+ no Grau${lista ? ' e' : ''}` : ''}${lista ? ` ${lista}` : ''}.`;
}

const QUANDO = { aliadoEntra: 'Quando o primeiro Aliado seu entra em campo em cada turno', aliadoMorre: 'Na primeira vez em cada turno que um Aliado seu vai do campo para o cemitério', aliadoDanoConjurador: 'Na primeira vez em cada turno que um Aliado seu causa dano de combate ao Conjurador inimigo' };
/** Texto sugerido para a habilidade de um Conjurador, a partir do `poder`. */
export function textoPoder(p) {
  if (!p) return '';
  const t = [];
  if (p.ativacao) {
    const a = p.ativacao;
    const custo = reqAtivacao(a) + ([a.suspende ? 'suspenda o Conjurador' : '', custoAtivacao(a)].filter(Boolean).join(' e ') || 'ative');
    const c = custo.charAt(0).toUpperCase() + custo.slice(1);
    t.push(`${a.repentina ? 'Repentina: ' + custo : c}: ${fraseMin(a.efeitos ?? [])}`);
  }
  if (p.gatilho) {
    let q = QUANDO[p.gatilho.quando] ?? p.gatilho.quando;
    if (p.gatilho.soNoSeuTurno) q = q.replace('em cada turno', 'em cada um dos seus turnos').replace('em cada turno', 'em cada um dos seus turnos');
    t.push(`${q}, ${fraseMin(p.gatilho.efeitos ?? [])}`);
  }
  if (p.primeiroDanoDeAliadoMais) t.push(`O primeiro dano de combate que um Aliado seu causa em cada turno é ${p.primeiroDanoDeAliadoMais} maior.`);
  if (p.primeiraCartaMaisBarata) t.push(`A primeira carta que você joga em cada turno custa ${p.primeiraCartaMaisBarata} a menos.`);
  if (p.primeiraRepentinaMaisBarata) t.push(`A primeira carta Repentina que você joga em cada turno custa ${p.primeiraRepentinaMaisBarata} a menos.`);
  if (p.aliadosProtegidos) t.push(`Seus Aliados têm Protegido ${p.aliadosProtegidos}.`);
  // Editor de Conjuradores (04/10): permissões contínuas e habilidades no formato do construtor
  for (const pm of p.permiteJogarDe ?? []) t.push(textoPermissao(pm));
  for (const h of p.habilidades ?? []) t.push(textoHabilidade(h, true));
  return t.join(' ');
}

export const valoresTxt = (vs) => (vs?.length ? vs.map((v) => VALOR[v] ?? v).join(' + ') : 'sem Valor');
