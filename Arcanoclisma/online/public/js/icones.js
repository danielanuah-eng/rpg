// Iconografia das mecânicas (pedido do autor, 04/10). O GPT/Codex desenha os ícones (pedido em
// online/pedidos-visuais/iconografia.claude.json); o Claude só liga: cada ícone entra ao lado da palavra no texto da carta,
// nas Palavras-chave e no tutorial. Ícone só aparece se estiver no manifesto do GPT com okGpt: true; sem ele, fica só o texto.
// `gatilho`: trecho do texto das cartas onde o ícone entra (antes da palavra). A lista é a mesma do pedido.
export const ICONES = [
  { id: 'suspender', nome: 'Suspender', mostra: 'uma carta se deitando (girando 90°) com uma seta curva apontando para baixo e riscos de movimento atrás', gatilho: /\b(Suspenda|suspenda|[Ss]uspender|suspende|Suspende|suspensa|suspenso)\b/ },
  { id: 'levantar', nome: 'Levantar', mostra: 'uma carta deitada se erguendo com uma seta curva para cima e riscos de movimento', gatilho: /\b(levanta|Levanta|levante|Levante)\b/ },
  // requisitos por linha (04/10): "(3+ no Grau · Pague 2)" antes de um efeito ou habilidade
  { id: 'requisito-grau', nome: 'Requisito de Grau', mostra: 'o losango azul do Grau com um sinal de + e o número ao lado (mínimo de cartas no Grau para a linha acontecer)', gatilho: /(\d+\+ no Grau)/ },
  { id: 'requisito-arcano', nome: 'Custo de Arcano da linha', mostra: 'uma esfera de Arcano pequena com moldura de selo (custo pago na hora para a linha acontecer), com o número ao lado', gatilho: /(Pague \d+)(?=\))/ },
  { id: 'pagar-arcano', nome: 'Pagar Arcano', mostra: 'uma esfera de Arcano azul saindo (seta para fora) de uma mão aberta, com o número ao lado', gatilho: /\b([Pp]ague \d+)/ },
  { id: 'ganhar-arcano', nome: 'Ganhar Arcano', mostra: 'uma esfera de Arcano azul com brilho e uma seta entrando (para dentro), sinal de +', gatilho: /\b([Gg]anhe \d+ Arcano|[Gg]era \d+ Arcano)/ },
  { id: 'enviar-arcano', nome: 'Enviar ao Arcano', mostra: 'uma carta virando faíscas azuis que sobem para dentro de uma esfera de Arcano', gatilho: /\b(enviar? (?:uma |as |a )?cartas? ao Arcano|enviada ao Arcano)/ },
  { id: 'grau', nome: 'Grau', mostra: 'uma pilha de cartas viradas (verso) com um losango azul na frente', gatilho: /\b(Zona de Grau|no Grau|do Grau)\b/ },
  { id: 'comprar', nome: 'Comprar', mostra: 'uma carta saindo do topo do baralho em direção a uma mão, seta curva', gatilho: /\b([Cc]ompre \d+|[Cc]omprar)/ },
  { id: 'olhar-topo', nome: 'Olhar o topo', mostra: 'um olho aberto sobre o topo de um baralho, com 3 cartas em leque', gatilho: /\b([Oo]lhe (?:as )?\d+|Olhar o topo)/ },
  { id: 'fundo-baralho', nome: 'Fundo do baralho', mostra: 'uma carta deslizando para baixo do baralho, seta para baixo', gatilho: /\b(no fundo do baralho|para o fundo do baralho)/ },
  { id: 'descartar', nome: 'Descartar', mostra: 'uma carta caindo da mão em direção a uma lápide pequena', gatilho: /\b([Dd]escarte|descartar)\b/ },
  { id: 'dano', nome: 'Causar dano', mostra: 'uma faísca/estilhaço vermelho de impacto em forma de estrela pontiaguda', gatilho: /\b([Cc]ause \d+)/ },
  { id: 'curar', nome: 'Curar', mostra: 'uma gota verde-clara com uma pequena cruz de luz e brilho suave', gatilho: /\b([Cc]ure \d+)/ },
  { id: 'destruir', nome: 'Destruir', mostra: 'uma carta partida ao meio na diagonal, com fragmentos voando', gatilho: /\b([Dd]estrua|[Dd]estruir)\b/ },
  { id: 'devolver', nome: 'Devolver à mão', mostra: 'uma carta voltando em arco (seta curva para trás) até uma mão', gatilho: /\b([Dd]evolva|[Dd]evolver)\b/ },
  { id: 'banir', nome: 'Banir', mostra: 'uma carta sumindo num portal circular escuro, com contorno pontilhado', gatilho: /\b([Bb]ana|[Bb]anir|banida|[Bb]animento)\b/ },
  { id: 'cemiterio', nome: 'Cemitério', mostra: 'uma lápide pequena com uma carta encostada', gatilho: /\b(cemitério)\b/ },
  { id: 'anular', nome: 'Anular', mostra: 'uma carta com um círculo cortado (símbolo de proibido) dourado por cima', gatilho: /\b([Aa]nul[ea]|anula|[Aa]nular)\b/ },
  { id: 'invocar', nome: 'Invocar ficha', mostra: 'uma carta pequena brotando de um círculo mágico no chão', gatilho: /\b([Ii]nvoque|[Ii]nvocar|Ficha)\b/ },
  { id: 'ao-entrar', nome: 'Ao entrar', mostra: 'uma carta descendo até a mesa com uma onda de impacto embaixo', gatilho: /\b(Ao entrar|ao entrar)\b/ },
  { id: 'atacar', nome: 'Atacar', mostra: 'duas lâminas cruzadas com riscos de movimento para a frente', gatilho: /\b(atacar|ataca|Ataque|Bloquear e atacar)\b/ },
  { id: 'bloquear', nome: 'Bloquear', mostra: 'um escudo erguido recebendo um impacto', gatilho: /\b([Bb]loquear|bloqueia|bloqueio)\b/ },
  { id: 'repentina', nome: 'Repentina', mostra: 'um raio dourado rápido (velocidade de resposta)', gatilho: /\b(Repentina)\b/ },
  { id: 'findante', nome: 'Findante', mostra: 'uma ampulheta com a areia descendo (só na sua fase principal)', gatilho: /\b(Findante)\b/ },
  { id: 'proficiente', nome: 'Proficiente', mostra: 'uma bota alada / seta dupla para a frente (age no turno em que entra)', gatilho: /\b(Proficiente)\b/ },
  { id: 'etereo', nome: 'Etéreo', mostra: 'uma silhueta translúcida com contorno tracejado e brilho fantasma', gatilho: /\b(Etéreos?)\b/ },
  { id: 'protessencia', nome: 'Protessência', mostra: 'uma carta envolta por um véu/aura fechada que desvia uma mira (retícula) riscada', gatilho: /(Protessência)/ },
  { id: 'protegido', nome: 'Protegido', mostra: 'uma carta com uma redoma/escudo arredondado em volta e um número', gatilho: /\b(Protegido \d+)/ },
  { id: 'economia', nome: 'Economia', mostra: 'duas cartas entrando na mão no fim do turno (o mesmo símbolo da estatística da carta do Conjurador)', gatilho: /\b(Economia)\b/ },
  { id: 'cataclisma', nome: 'Cataclisma', mostra: 'uma esfera de Arcano rachando e se desfazendo (o Arcano que sobra some)', gatilho: /\b(Cataclisma)\b/ },
  { id: 'uso-unico', nome: 'Uso único', mostra: 'um frasco com o número 1 e um risco (consome ao usar)', gatilho: /\b(uso único|[Cc]onsumível)\b/ },
];

const manifesto = await fetch('/kit/icones/manifesto.json').then((r) => (r.ok ? r.json() : {})).catch(() => ({}));
const prontos = new Map(ICONES.filter((i) => manifesto[i.id]?.okGpt === true && manifesto[i.id].arquivo).map((i) => [i.id, manifesto[i.id].arquivo]));
const esc = (t) => String(t ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

/** HTML do ícone (vazio se o GPT ainda não entregou). */
export function icone(id, titulo) {
  const arq = prontos.get(id);
  if (!arq) return '';
  const nome = titulo ?? ICONES.find((i) => i.id === id)?.nome ?? id;
  return `<img class="icone-mecanica" src="${esc(arq)}" alt="" title="${esc(nome)}" aria-hidden="true">`;
}
/** Texto de carta com os ícones antes das palavras das mecânicas (texto escapado; só ícones aprovados). */
export function iconizar(texto) {
  const plano = String(texto ?? '');
  if (!prontos.size) return esc(plano);
  // uma passada só, sobre o texto original: junta as posições de todos os gatilhos e não sobrepõe
  const achados = [];
  for (const i of ICONES) {
    if (!prontos.has(i.id)) continue;
    for (const m of plano.matchAll(new RegExp(i.gatilho.source, 'g'))) achados.push({ ini: m.index, fim: m.index + m[0].length, id: i.id });
  }
  achados.sort((x, y) => x.ini - y.ini);
  let html = '', pos = 0;
  for (const a of achados) {
    if (a.ini < pos) continue;
    html += esc(plano.slice(pos, a.ini)) + icone(a.id) + esc(plano.slice(a.ini, a.fim));
    pos = a.fim;
  }
  return html + esc(plano.slice(pos));
}
export const temIcones = () => prontos.size > 0;
