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
  const base = descEfeitoBase(ef) + (ef.escolha && ef.escolha !== 'controlador' && (ef.alcance !== 'todos') ? ESCOLHA_TXT[ef.escolha] : '');
  return textoRequisito(ef.requisito) + (temCusto(ef.custo) && ef.tipo !== 'pagar' ? textoComCusto(ef.custo, base) : base);
}
/** O custo tem algo a pagar? Custo vazio (ainda sendo montado no Editor) não aparece no texto nem é cobrado pelo motor. */
export const temCusto = (c) => !!c && ['arcano', 'vida', 'descartar', 'grauAoCemiterio', 'sacrificar'].some((k) => (c[k] ?? 0) > 0);

// ---------- custo condicional (05/10, decisão 40) ----------

/** Partes do custo: no infinitivo ("pagar 1 de Arcano") ou no imperativo ("pague 1 de Arcano"). */
function partesCusto(c, imp) {
  const v = imp ? { pagar: 'pague', descartar: 'descarte', mandar: 'mande', sacrificar: 'sacrifique' } : { pagar: 'pagar', descartar: 'descartar', mandar: 'mandar', sacrificar: 'sacrificar' };
  const pagos = [c.arcano ? `${c.arcano} de Arcano` : '', c.vida ? `${c.vida} de Vida` : ''].filter(Boolean);
  return [pagos.length ? `${v.pagar} ${pagos.join(' e ')}` : '', c.descartar ? `${v.descartar} ${cartas(c.descartar)}` : '',
    c.grauAoCemiterio ? `${v.mandar} ${cartas(c.grauAoCemiterio)} do Grau ao cemitério` : '', c.sacrificar ? `${v.sacrificar} ${c.sacrificar > 1 ? `${c.sacrificar} Aliados` : 'um Aliado'}` : ''].filter(Boolean);
}
const lista = (p) => (p.length > 1 ? p.slice(0, -1).join(', ') + ' e ' + p.at(-1) : p[0] ?? '');
/** Custo para a tela e o texto: "pagar 1 de Arcano e descartar 1 carta" (imp: "pague 1 de Arcano e descarte 1 carta"). */
export function textoCustoPagar(c, imp = false) { return lista(partesCusto(c ?? {}, imp)) || (imp ? 'não pague nada' : 'não pagar nada'); }
const SAI_TXT = { cemiterio: 'o cemitério', banimento: 'o banimento', mao: 'a mão do dono', fundo: 'o fundo do baralho do dono' };
/** O que acontece quando o efeito é impedido (texto curto). */
function consequencia(c, quemSai = 'ela') {
  switch (c.senao) {
    case 'carta': return 'esta carta é anulada';
    case 'alternativo': return c.alternativos?.length ? juntarEfeitos(c.alternativos) : 'nada acontece';
    case 'sai': return `${quemSai} vai para ${SAI_TXT[c.destino ?? 'cemiterio']}`;
    case 'pula': return 'o efeito não acontece neste turno';
    default: return '';
  }
}
/**
 * Linha com custo: "você pode pagar 1 de Arcano; se pagar, compre 2 cartas" ou, pago pelo oponente,
 * "cause 3 de dano ao Conjurador inimigo, a menos que o oponente pague 2 de Arcano".
 */
export function textoComCusto(c, acao) {
  const custo = textoCustoPagar(c);
  const ao = c.quando === 'jogar' ? ' ao jogar esta carta' : '';
  const sen = consequencia(c);
  if (c.quem === 'oponente') return `${acao}, a menos que o oponente ${textoCustoPagar(c, true)}${ao}${sen ? ` (se ele fizer isso, ${sen})` : ''}`;
  return `você pode ${custo}${ao}; se fizer isso, ${acao}${sen ? `; se não, ${sen}` : ''}`;
}
/** Manutenção: "pague 1 de Arcano ou ela vai para o cemitério" / "o oponente pode pagar 2 de Arcano; se pagar, ela vai para o cemitério". */
export function textoManutencao(m) {
  const sen = consequencia({ senao: m.senao ?? 'sai', destino: m.destino, alternativos: m.alternativos }) || 'nada acontece';
  if (m.quem === 'oponente') return `o oponente pode ${textoCustoPagar(m)}; se fizer isso, ${sen}`;
  return `${textoCustoPagar(m, true)} ou ${sen}`;
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
    case 'reanimar': return textoReanimar(ef);
    case 'olharTopo': return `olhe as ${ef.quantidade} cartas do topo do seu baralho; ponha ${ef.pegar} na mão e as outras no fundo do baralho`;
    case 'permitirAnularDoCemiterio': return 'neste turno você pode jogar do cemitério uma carta que anula outra (ela é banida ao resolver)';
    case 'potenciaMagia': return `o próximo dano ou cura de uma Temporária sua neste turno tem +${ef.valor}`;
    case 'invocar': return `invoque ${ef.nome ?? 'uma criatura'}${ef.concessao ? ` (${textoConcessao(ef.concessao, 'ela')})` : ''}`;
    case 'ataqueConjuradorTurno': return `seu Conjurador ganha +${ef.valor} de Ataque neste turno`;
    // construtor de efeitos (04/10)
    case 'perderVida': return `${ef.quem === 'oponente' ? 'o Conjurador do oponente' : 'seu Conjurador'} ${ef.secundario ? `toma ${ef.valor} de dano` : `perde ${ef.valor} de Vida`}`;
    case 'descartar': return textoTirarDaMao(ef);
    // efeitos secundários (04/10, noite)
    case 'sacrificar': { const n = ef.valor ?? 1; return `${ef.quem === 'oponente' ? 'o oponente sacrifica' : 'você sacrifica'} ${n > 1 ? `${n} Aliados` : 'um Aliado'}${ef.quem === 'oponente' ? ' dele' : ''}`; }
    case 'grauAoCemiterio': return `${ef.quem === 'oponente' ? 'o oponente manda' : 'você manda'} ${ef.valor} carta${ef.valor > 1 ? 's' : ''} do ${ef.quem === 'oponente' ? 'Grau dele' : 'seu Grau'} ao cemitério`;
    case 'colocarNoGrau': return `ponha ${ef.valor} carta${ef.valor > 1 ? 's' : ''} ${ef.de === 'topo' ? 'do topo do seu baralho' : 'da sua mão'} no seu Grau`;
    case 'olharTopoPara': return `olhe as ${ef.quantidade} cartas do topo do seu baralho; ponha ${ef.pegar} ${DESTINO[ef.destino]} e as outras ${DESTINO[ef.resto]}`;
    case 'olharMao': return ef.pegar > 0 && ef.destino !== 'nada' ? `olhe a mão do oponente e mande ${ef.pegar} carta${ef.pegar > 1 ? 's' : ''} dela ${PARA[ef.destino]}` : 'olhe a mão do oponente';
    case 'jogarDe': return `ponha em campo, sem pagar, ${ef.valor > 1 ? `até ${ef.valor} cartas` : 'uma carta'} (${(ef.tipos?.length ? ef.tipos : ['aliado']).map((t) => TIPO_PL[t] ?? t).join(' ou ')}${ef.custoMax !== undefined ? `, custo até ${ef.custoMax}` : ''}) ${ORIGEM[ef.de]}${ef.concessao ? `; elas ${textoConcessao(ef.concessao, 'elas')}` : ''}`;
    case 'campo': return textoCampo(ef);
    // internos (05/10): linha que só cobra o custo; continuação de custos ao jogar
    case 'pagar': return ef.custo ? (ef.custo.quem === 'oponente' ? `o oponente pode ${textoCustoPagar(ef.custo)} para impedir` : `você pode ${textoCustoPagar(ef.custo)}`) : '';
    case 'proximoCusto': return '';
    default: return ef.tipo;
  }
}
const cartas = (n) => `${n} carta${n > 1 ? 's' : ''}`;
/**
 * "Tirar da mão" (04/10, noite): descartar ou banir, de quem (você, o oponente, os dois) e como (ao acaso, o dono escolhe,
 * quem controla o efeito olha e escolhe). Ex.: "o oponente descarta 1 carta ao acaso", "olhe a mão do oponente e bana 1 carta dela".
 */
function textoTirarDaMao(ef) {
  const banir = ef.acao === 'banir';
  const modo = ef.modo ?? (ef.aleatorio ? 'acaso' : 'dono');
  const n = cartas(ef.valor);
  const sujeito = { voce: 'você', oponente: 'o oponente', ambos: 'cada jogador' }[ef.quem] ?? 'o oponente';
  const verbo = banir ? 'bane' : 'descarta';
  if (modo === 'olhando' && ef.quem !== 'voce') {
    const olhe = `olhe a mão do oponente e ${banir ? 'bana' : 'descarte'} ${n} dela`;
    return ef.quem === 'ambos' ? `${banir ? 'bana' : 'descarte'} ${n} da sua mão; ${olhe}` : olhe;
  }
  return `${sujeito} ${verbo} ${n}${ef.quem === 'oponente' ? '' : ef.quem === 'ambos' ? ' da própria mão' : ''}${modo === 'acaso' ? ' ao acaso' : ''}`;
}
/** Reanimar (04/10, noite): de qual cemitério, filtros e pagamento (sem pagar, o custo dele, ou o custo + X). */
function textoReanimar(ef) {
  const origem = { seu: 'do seu cemitério', oponente: 'do cemitério do oponente' }[ef.de] ?? 'de qualquer cemitério';
  const filtro = `${ef.subtipo ? ` ${ef.subtipo}` : ''}${ef.custoMax !== undefined ? ` de custo até ${ef.custoMax}` : ''}`;
  const pg = ef.pagamento === 'gratis' ? 'sem pagar' : ef.pagamento === 'custo' || !ef.custoExtra ? 'pagando o custo dele' : `pagando o custo dele + ${ef.custoExtra}`;
  return `ponha no seu campo um Aliado${filtro} ${origem}, ${pg}${ef.concessao ? `; ele ${textoConcessao(ef.concessao, 'ele')}` : ''}`;
}
/**
 * Efeitos em sequência: "a e b". Efeitos secundários no fim (custos/consequências, 04/10 noite) entram como
 * "...; depois, seu Conjurador toma 2 de dano e você sacrifica um Aliado".
 */
export function juntarEfeitos(efs) {
  // linha interna "pagar" (custo do gatilho ou manutenção, 05/10): o custo envolve as linhas do bloco dela
  const ip = efs.findIndex((ef) => ef?.tipo === 'pagar');
  if (ip >= 0) {
    const c = efs[ip].custo ?? {};
    const bloco = efs.slice(ip + 1, ip + 1 + (efs[ip].bloco ?? efs.length));
    const depois = efs.slice(ip + 1 + bloco.length);
    const meio = !bloco.length && efs[ip].fonteId ? textoManutencao(c) : textoComCusto(c, bloco.length ? juntarEfeitos(bloco) : 'nada acontece');
    return [efs.slice(0, ip).length ? juntarEfeitos(efs.slice(0, ip)) : '', meio, depois.length ? juntarEfeitos(depois) : ''].filter(Boolean).join('; ');
  }
  let k = efs.length;
  while (k > 0 && efs[k - 1]?.secundario) k--;
  // linhas com custo (05/10) ficam separadas por ponto e vírgula, para o custo não se misturar com a linha seguinte
  const txt = (l) => l.map(descEfeito).join(l.some((ef) => ef.custo) ? '; ' : ' e ');
  if (k === 0 || k === efs.length) return txt(efs);
  return `${txt(efs.slice(0, k))}; depois, ${txt(efs.slice(k))}`;
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
export const frase = (efs) => maiuscula(juntarEfeitos(efs)) + '.';
const reqAtivacao = (a) => (a.requisito?.grau ? `com ${a.requisito.grau}+ no Grau, ` : '');
const custoAtivacao = (a) => (a.custo || a.requisito?.arcano ? `pague ${(a.custo ?? 0) + (a.requisito?.arcano ?? 0)}` : '');
/** Frase no meio do texto (depois de dois-pontos): começa com minúscula sem rebaixar nomes próprios como Arcano. */
const fraseMin = (efs) => juntarEfeitos(efs) + '.';

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
  const prot = textoProtegido(protegidoDe(d));
  if (prot) p.push(prot);
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
  return p.join(' ');
}

/** Condições do construtor de efeitos (o mesmo texto aparece no Editor). */
export const QUANDO_TXT = {
  jogar: 'Ao jogar', entrar: 'Quando entra em campo', ativada: '', alvo: 'Quando vira alvo', grau: 'Quando é colocada no Grau',
  atacar: 'Quando ataca', bloquear: 'Quando defende', cemiterio: 'Quando cai no cemitério', banimento: 'Quando é banida', danoConjurador: 'Quando causa dano de combate ao Conjurador inimigo', cataclismaOponente: 'No Cataclisma do turno do oponente',
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
  if (h.quando === 'emCampo') return textoEmCampo(h, conj);
  const quando = EVENTO_TXT[h.quando] ? `Quando ${EVENTO_TXT[h.quando][h.quem ?? 'qualquer']}` : conj && QUANDO_CONJ_TXT[h.quando] ? QUANDO_CONJ_TXT[h.quando] : QUANDO_TXT[h.quando] ?? h.quando;
  const vezes = h.vezesPorTurno ?? (h.umaVezPorTurno ? 1 : 0);
  const extras = [vezes === 1 ? 'uma vez por turno' : vezes > 1 ? `até ${vezes} vezes por turno` : '', h.soNoSeuTurno ? 'só no seu turno' : ''].filter(Boolean);
  const corpo = temCusto(h.pagar) ? textoComCusto(h.pagar, juntarEfeitos(h.efeitos ?? [])) + '.' : efs;
  return `${quando}${extras.length ? ` (${extras.join(', ')})` : ''}: ${corpo}`;
}
/** Eventos de jogador (05/10): o texto depende de quem ('voce', 'oponente', 'qualquer'). */
export const EVENTO_TXT = {
  compra: { voce: 'você compra cartas', oponente: 'o oponente compra cartas', qualquer: 'um jogador compra cartas' },
  conjuradorDano: { voce: 'seu Conjurador toma dano', oponente: 'o Conjurador do oponente toma dano', qualquer: 'um Conjurador toma dano' },
  ganhaVida: { voce: 'seu Conjurador ganha Vida (cura)', oponente: 'o Conjurador do oponente ganha Vida (cura)', qualquer: 'um Conjurador ganha Vida (cura)' },
  perdeVida: { voce: 'seu Conjurador perde Vida', oponente: 'o Conjurador do oponente perde Vida', qualquer: 'um Conjurador perde Vida' },
  jogaAliado: { voce: 'você joga um Aliado', oponente: 'o oponente joga um Aliado', qualquer: 'um jogador joga um Aliado' },
  jogaMistica: { voce: 'você joga uma Mística', oponente: 'o oponente joga uma Mística', qualquer: 'um jogador joga uma Mística' },
  jogaPermanente: { voce: 'você joga uma permanente (Aliado, Mística ou Estrutura)', oponente: 'o oponente joga uma permanente (Aliado, Mística ou Estrutura)', qualquer: 'um jogador joga uma permanente (Aliado, Mística ou Estrutura)' },
  ativaHabilidade: { voce: 'você ativa uma habilidade', oponente: 'o oponente ativa uma habilidade', qualquer: 'um jogador ativa uma habilidade' },
  pilha: { voce: 'uma carta sua vai para a pilha', oponente: 'uma carta do oponente vai para a pilha', qualquer: 'uma carta vai para a pilha' },
};
const ETAPA_TXT = { inicio: 'no início', principal: 'na fase principal', cataclisma: 'no Cataclisma', final: 'no Final' };
const TURNO_TXT = { seu: 'do seu turno', oponente: 'do turno do oponente', cada: 'de cada turno' };
const LADO_AURA = { proprio: 'seus', inimigo: 'do oponente', ambos: 'de todos' };
const TIPO_AURA = { aliado: 'Aliados', mistica: 'Místicas', estrutura: 'Estruturas', conjurador: 'Conjuradores' };
/**
 * "Enquanto estiver em campo" (05/10): passivo ("Enquanto estiver em campo, seus Aliados ganham +1/+1 e Etéreo.") ou
 * recorrente ("Enquanto estiver em campo, no início do seu turno: compre 1 carta."), mais a manutenção
 * ("Enquanto estiver em campo, no início do seu turno, pague 1 de Arcano ou ela vai para o cemitério.").
 * No Conjurador, sem o "enquanto estiver em campo".
 */
export function textoEmCampo(h, conj = false) {
  const pre = conj ? '' : 'Enquanto estiver em campo';
  const junta = (t) => (pre ? `${pre}, ${t}` : t.charAt(0).toUpperCase() + t.slice(1));
  const frases = [];
  if ((h.modo ?? 'passivo') === 'passivo') {
    const a = h.aura;
    if (a && (a.ataque || a.vida || a.palavras?.length)) {
      const tipos = (a.tipos?.length ? a.tipos : ['aliado']).map((t) => TIPO_AURA[t] ?? t);
      const grupo = a.lado === 'proprio' ? `seus ${lista(tipos)}` : a.lado === 'inimigo' ? `os ${lista(tipos)} do oponente` : `todos os ${lista(tipos)}`;
      const sinal = (v) => (v > 0 ? '+' : '') + v;
      const ganhos = [a.ataque && a.vida ? `${sinal(a.ataque)}/${sinal(a.vida)}` : a.ataque ? `${sinal(a.ataque)} de Ataque` : a.vida ? `${sinal(a.vida)} de Vida` : '', ...(a.palavras ?? [])].filter(Boolean);
      frases.push(`${grupo}${a.excetoEsta ? ' (menos esta carta)' : ''} ganham ${lista(ganhos)}`);
    }
    const c = h.custoCartas;
    if (c?.valor) {
      const tipos = (c.tipos ?? []).map((t) => TIPO_PL[t] ?? t);
      const pl = tipos.map((t) => t + 's');
      const quais = !tipos.length ? 'as cartas' : c.tipos.every((t) => t === 'aliado') ? 'os Aliados' : c.tipos.includes('aliado') ? `as cartas (${lista(pl)})` : `as ${lista(pl)}`;
      const de = c.de === 'voce' ? ' que você joga' : c.de === 'oponente' ? ' do oponente' : ' de todos';
      frases.push(`${quais}${de} custam ${Math.abs(c.valor)} a ${c.valor > 0 ? 'mais' : 'menos'}`);
    }
  } else if (h.efeitos?.length) {
    const quando = `${ETAPA_TXT[h.etapa ?? 'inicio']} ${TURNO_TXT[h.turnoDe ?? 'seu']}${h.turnos ? `, por ${h.turnos} turno${h.turnos > 1 ? 's' : ''}` : ''}`;
    frases.push(`${quando}: ${temCusto(h.pagar) ? textoComCusto(h.pagar, juntarEfeitos(h.efeitos)) : juntarEfeitos(h.efeitos)}`);
  }
  const t = [];
  if (frases.length) t.push(junta(frases.join('; ')) + '.');
  const m = h.manutencao;
  if (temCusto(m)) {
    const mt = `${ETAPA_TXT[m.etapa ?? 'inicio']} ${TURNO_TXT[m.turnoDe ?? 'seu']}, ${textoManutencao(m)}`;
    // depois de um passivo/recorrente a manutenção não repete "Enquanto estiver em campo"
    t.push((t.length ? mt.charAt(0).toUpperCase() + mt.slice(1) : junta(mt)) + '.');
  }
  return t.join(' ') || junta('nada acontece') + '.';
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
/**
 * Protegido da carta (unificado em 04/10, noite): número N = { arcano: N }; o nome antigo `protecao` ("Proteção mágica")
 * ainda é lido e somado. Mesma conta de protegidoDe em tcg/src/motor.ts. null = sem Protegido.
 */
export function protegidoDe(d) {
  const p = d?.protegido;
  const r = typeof p === 'number' ? (p ? { arcano: p } : {}) : { ...(p ?? {}) };
  for (const [k, v] of Object.entries(d?.protecao ?? {})) { if (!v) continue; r[k] = k === 'grauMin' ? Math.max(r[k] ?? 0, v) : (r[k] ?? 0) + v; }
  for (const k of Object.keys(r)) if (!r[k]) delete r[k];
  return Object.keys(r).length ? r : null;
}
/** Texto do Protegido: "Protegido 2." ou, com as variações, "Protegido 1: para mirar esta carta, o oponente também precisa pagar 2 de Vida." */
export function textoProtegido(pr) {
  if (!pr) return '';
  const partes = [pr.grauMin ? `ter ${pr.grauMin}+ no Grau` : '', pr.vida ? `pagar ${pr.vida} de Vida` : '', pr.descartar ? `descartar ${cartas(pr.descartar)}` : '', pr.grauParaCemiterio ? `mandar ${cartas(pr.grauParaCemiterio)} do Grau ao cemitério` : ''].filter(Boolean);
  if (!partes.length) return `Protegido ${pr.arcano}.`;
  const lista = partes.length > 1 ? partes.slice(0, -1).join(', ') + ' e ' + partes.at(-1) : partes[0];
  return `Protegido${pr.arcano ? ` ${pr.arcano}` : ''}: para mirar esta carta, o oponente ${pr.arcano ? 'também ' : ''}precisa ${lista}.`;
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
