// Identidade visual de cartas. Não atribui Valores, custos, raridade ou regras.
// O catálogo e o editor do autor fornecem esses dados; este módulo apenas os desenha.
export const PALETA_VALORES = {
  conflituo: { nome: 'Conflitüo', cor: '#9a4633', escuro: '#3c211e', claro: '#cf9279' },
  sublimeyn: { nome: 'Sublimeyn', cor: '#748e9d', escuro: '#233848', claro: '#bacdd5' },
  dominum: { nome: 'Dominum', cor: '#62416a', escuro: '#291c35', claro: '#b598bf' },
  persona: { nome: 'Persona', cor: '#b76673', escuro: '#472a35', claro: '#e1adb6' },
  alvorum: { nome: 'Alvorum', cor: '#927638', escuro: '#3e3424', claro: '#d0bb80' },
  morbidyum: { nome: 'Morbidyum', cor: '#6b866d', escuro: '#243b31', claro: '#adc5a6' },
  cognitium: { nome: 'Cognitium', cor: '#505b94', escuro: '#222944', claro: '#a5add2' },
  eternyr: { nome: 'Eternyr', cor: '#304565', escuro: '#192738', claro: '#8299b8' },
  primitivum: { nome: 'Primitivum', cor: '#65743b', escuro: '#2d3522', claro: '#b7bf86' },
};
const KIT = '/kit/';
export const slugValor = (valor) => String(valor ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
export const valoresVisuais = (valores = []) => [...new Set(valores.map(slugValor))].filter((v) => PALETA_VALORES[v]);
const setores = (valores) => valores.length === 1 ? PALETA_VALORES[valores[0]].cor
  : `conic-gradient(from 0deg, ${valores.map((v, i) => `${PALETA_VALORES[v].cor} ${i * 100 / valores.length}% ${(i + 1) * 100 / valores.length}%`).join(', ')})`;

function carregarEstilo() {
  if (document.getElementById('identidade-valor-v12')) return;
  const estilo = document.createElement('link');
  estilo.id = 'identidade-valor-v12'; estilo.rel = 'stylesheet';
  estilo.href = '/css/templates-valor-v12.css'; document.head.append(estilo);
}

/** Esfera única: setores por Valor, símbolos discretos e número independente por cima. */
export function criarEsferaValor({ valores, numero, classe = '', rotulo = 'Valores' }) {
  const ids = valoresVisuais(valores);
  const esfera = document.createElement('div');
  esfera.className = `esfera-valor-v12 ${classe}`.trim();
  esfera.style.background = ids.length ? setores(ids) : '#777d83';
  const titulo = `${rotulo}: ${ids.map((v) => PALETA_VALORES[v].nome).join(' + ')}${numero == null ? '' : ` · ${numero}`}`;
  esfera.title = titulo; esfera.setAttribute('aria-label', titulo);
  if (numero == null) esfera.setAttribute('role', 'img');
  const simbolos = document.createElement('span'); simbolos.className = 'esfera-simbolos-v12'; simbolos.setAttribute('aria-hidden', 'true');
  ids.forEach((v, i) => {
    const simbolo = document.createElement('img');
    simbolo.src = `${KIT}vetores/valor-${v}-simbolo-v8.svg`; simbolo.alt = ''; simbolo.draggable = false;
    const angulo = 2 * Math.PI * (i + .5) / ids.length;
    const raio = ids.length === 1 ? 0 : .28;
    simbolo.style.left = `${(0.5 + raio * Math.sin(angulo)) * 100}%`;
    simbolo.style.top = `${(0.5 - raio * Math.cos(angulo)) * 100}%`;
    simbolo.style.width = ids.length === 1 ? '70%' : `${Math.min(44, 115 / ids.length)}%`;
    simbolos.append(simbolo);
  });
  esfera.append(simbolos);
  if (numero != null) {
    const numeroEl = document.createElement('span'); numeroEl.className = 'esfera-numero-v12';
    numeroEl.textContent = String(numero); esfera.append(numeroEl);
  }
  const aro = document.createElement('img'); aro.className = 'esfera-aro-v12';
  aro.src = `${KIT}templates/valores-v12/aro-esfera-v12.svg`; aro.alt = ''; aro.draggable = false;
  aro.setAttribute('aria-hidden', 'true'); esfera.append(aro);
  return esfera;
}

/** Recebe a carta visível já renderizada; nunca consulta nem revela cartas ocultas. */
export function aplicarIdentidadeValor(carta, modelo) {
  if (!modelo || carta.classList.contains('is-hidden')) return carta;
  carregarEstilo();
  const valores = valoresVisuais(modelo.valores);
  const tipo = modelo.categoriaItem || modelo.tipo;
  const tipos = ['aliado', 'temporaria', 'mistica', 'estrutura', 'conjurador', 'canal', 'armadura', 'extra'];
  const generico = modelo.tipo === 'item' && valores.length === 0;
  carta.dataset.valorEstado = valores.length ? 'definido' : generico ? 'item-generico' : 'ausente';
  carta.dataset.valores = String(valores.length);
  carta.querySelector('.frame-tinta')?.remove();
  const moldura = carta.querySelector('.frame');
  if (moldura && tipos.includes(tipo) && (valores.length || generico)) {
    moldura.src = `${KIT}templates/valores-v12/${tipo}-${generico ? 'generico' : valores[0]}.svg`;
    if (valores.length > 1) {
      const tinta = document.createElement('div'); tinta.className = 'moldura-multivalor-v12';
      tinta.style.background = `linear-gradient(90deg, ${valores.map((v, i) => `${PALETA_VALORES[v].cor} ${i * 100 / valores.length}% ${(i + 1) * 100 / valores.length}%`).join(', ')})`;
      carta.querySelector('.front').append(tinta);
    }
  }
  const antiga = carta.querySelector(modelo.tipo === 'conjurador' ? '.conjurador-values' : '.cost');
  if (valores.length) {
    const numero = modelo.tipo === 'conjurador' || modelo.custo == null ? null : Math.max(0, Math.trunc(Number(modelo.custo) || 0));
    const central = modelo.tipo === 'conjurador' || (modelo.tipo === 'item' && numero == null);
    const esfera = criarEsferaValor({ valores: modelo.valores, numero,
      classe: central ? 'valores-centrais-v12' : 'badge cost esfera-custo',
      rotulo: numero == null ? 'Valores' : 'Custo de Arcano e Valores' });
    if (antiga) antiga.replaceWith(esfera); else carta.querySelector('.front').append(esfera);
  }
  const geracao = carta.querySelector('.generated');
  if (geracao && valores.length) geracao.style.backgroundImage = `url('${KIT}vetores/acabamento-geracao-v5.svg'), ${valores.length > 1 ? setores(valores) : `linear-gradient(${setores(valores)}, ${setores(valores)})`}`;
  return carta;
}
