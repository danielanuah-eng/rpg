// Textos gerados a partir da mecânica das cartas e dos Conjuradores (usados pelo app de testes e pelo editor).
export const VALOR = { conflituo: 'Conflitüo', sublimeyn: 'Sublimeyn', dominum: 'Dominum', persona: 'Persona', alvorum: 'Alvorum', morbidyum: 'Morbidyum', cognitium: 'Cognitium', eternyr: 'Eternyr', primitivum: 'Primitivum' };
export const TIPO = { aliado: 'Aliado', temporaria: 'Temporária', mistica: 'Mística', estrutura: 'Estrutura', item: 'Item' };
export const ALVO = { escolhido: 'um alvo', escolhidoAliado: 'um Aliado ou Mística', escolhidoProprio: 'um Aliado ou Mística seu', conjuradorInimigo: 'o Conjurador inimigo', conjuradorProprio: 'o seu Conjurador', pilha: 'uma carta na pilha' };

/** "a" + alvo, contraindo "a o" em "ao" (e "de o" em "do"). */
const a = (alvo) => (ALVO[alvo].startsWith('o ') ? 'ao ' + ALVO[alvo].slice(2) : 'a ' + ALVO[alvo]);
const de = (alvo) => (ALVO[alvo].startsWith('o ') ? 'do ' + ALVO[alvo].slice(2) : 'de ' + ALVO[alvo]);
export function descEfeito(ef) {
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
    case 'reanimar': return `ponha no seu campo um Aliado de qualquer cemitério, pagando o custo dele + ${ef.custoExtra}`;
    case 'olharTopo': return `olhe as ${ef.quantidade} cartas do topo do seu baralho; ponha ${ef.pegar} na mão e as outras no fundo do baralho`;
    case 'permitirAnularDoCemiterio': return 'neste turno você pode jogar do cemitério uma carta que anula outra (ela é banida ao resolver)';
    case 'potenciaMagia': return `o próximo dano ou cura de uma Temporária sua neste turno tem +${ef.valor}`;
    case 'invocar': return `invoque ${ef.nome ?? 'uma criatura'}`;
    case 'ataqueConjuradorTurno': return `seu Conjurador ganha +${ef.valor} de Ataque neste turno`;
    default: return ef.tipo;
  }
}
export const frase = (efs) => { const t = efs.map(descEfeito).join(' e '); return t.charAt(0).toUpperCase() + t.slice(1) + '.'; };
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
    const custos = [a.suspendeConjurador ? 'suspenda o Conjurador' : a.suspende ? 'suspenda' : '', a.custo ? `pague ${a.custo}` : '', a.descartar ? `descarte ${a.descartar} carta${a.descartar > 1 ? 's' : ''}` : '', a.danoNoConjurador ? `seu Conjurador toma ${a.danoNoConjurador} de dano` : ''].filter(Boolean);
    const lista = custos.length > 1 ? custos.slice(0, -1).join(', ') + ' e ' + custos.at(-1) : custos[0] ?? (d.categoriaItem === 'extra' ? 'use' : 'ative');
    p.push(`${lista.charAt(0).toUpperCase() + lista.slice(1)}: ${fraseMin(a.efeitos ?? [])}`);
  }
  if (d.pagarParaAnular !== undefined) p.push(`O oponente pode pagar ${d.pagarParaAnular} para anulá-la.`);
  return p.join(' ');
}

const QUANDO = { aliadoEntra: 'Quando o primeiro Aliado seu entra em campo em cada turno', aliadoMorre: 'Na primeira vez em cada turno que um Aliado seu vai do campo para o cemitério', aliadoDanoConjurador: 'Na primeira vez em cada turno que um Aliado seu causa dano de combate ao Conjurador inimigo' };
/** Texto sugerido para a habilidade de um Conjurador, a partir do `poder`. */
export function textoPoder(p) {
  if (!p) return '';
  const t = [];
  if (p.ativacao) {
    const a = p.ativacao;
    const custo = [a.suspende ? 'suspenda o Conjurador' : '', a.custo ? `pague ${a.custo}` : ''].filter(Boolean).join(' e ') || 'ative';
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
  return t.join(' ');
}

export const valoresTxt = (vs) => (vs?.length ? vs.map((v) => VALOR[v] ?? v).join(' + ') : 'sem Valor');
