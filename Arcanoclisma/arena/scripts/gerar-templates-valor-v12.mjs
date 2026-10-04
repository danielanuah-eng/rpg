// Gera SVGs editáveis a partir da geometria já aprovada, sem rasterizar nem modificar ilustrações.
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { PALETA_VALORES } from '../../online/public/js/identidade-valor-v12.js';
const entrada = new URL('../templates/', import.meta.url);
const saida = new URL('../../online/public/kit/templates/valores-v12/', import.meta.url);
mkdirSync(saida, { recursive: true });
const tipos = ['aliado', 'temporaria', 'mistica', 'estrutura', 'conjurador', 'canal', 'armadura', 'extra'];
for (const tipo of tipos) {
  const base = readFileSync(new URL(`${tipo}-${tipo === 'conjurador' ? 'v8' : 'v4'}.svg`, entrada), 'utf8');
  for (const [id, paleta] of Object.entries(PALETA_VALORES)) {
    let svg = base.replace(/<linearGradient id="title"[^>]*>[\s\S]*?<\/linearGradient>/,
      `<linearGradient id="title" x2="0" y2="1"><stop stop-color="${paleta.cor}"/><stop offset="1" stop-color="${paleta.escuro}"/></linearGradient>`);
    svg = svg.replace(/#(?:728d92|7f9ead|b7a36c|d5bd84)/gi, paleta.claro).replace(/#152434/gi, paleta.escuro);
    writeFileSync(new URL(`${tipo}-${id}.svg`, saida), svg, 'utf8');
  }
  if (['canal', 'armadura', 'extra'].includes(tipo)) {
    const cinza = base.replace(/#[0-9a-f]{6}/gi, (hex) => {
      const r = parseInt(hex.slice(1, 3), 16), g = parseInt(hex.slice(3, 5), 16), b = parseInt(hex.slice(5, 7), 16);
      const canal = Math.round(.2126 * r + .7152 * g + .0722 * b).toString(16).padStart(2, '0');
      return `#${canal.repeat(3)}`;
    });
    writeFileSync(new URL(`${tipo}-generico.svg`, saida), cinza, 'utf8');
  }
}
writeFileSync(new URL('contorno-valor-v12.svg', saida), `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 630 880"><rect x="12" y="12" width="606" height="856" rx="18" fill="none" stroke="white" stroke-width="3"/><path d="M107 29h461l25 27-25 26H107L90 56Z" fill="white"/><path d="M20 80V38q0-18 18-18h46M610 80V38q0-18-18-18h-46M20 802v38q0 18 18 18h46M610 802v38q0 18-18 18h-46" fill="none" stroke="white" stroke-width="4"/></svg>`, 'utf8');
writeFileSync(new URL('aro-esfera-v12.svg', saida), `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 76 76"><defs><linearGradient id="metal" x2=".2" y2="1"><stop stop-color="#f8e4b3"/><stop offset=".4" stop-color="#c4a66d"/><stop offset=".7" stop-color="#76604b"/><stop offset="1" stop-color="#dec493"/></linearGradient></defs><circle cx="38" cy="38" r="36.5" fill="none" stroke="#142332" stroke-width="2.5"/><circle cx="38" cy="38" r="34.6" fill="none" stroke="url(#metal)" stroke-width="1.4"/><path d="M13 17q9-9 21-9M59 63q-8 5-18 5" fill="none" stroke="#fff2cc" stroke-width="1" stroke-opacity=".6"/></svg>`, 'utf8');
console.log(JSON.stringify({ molduras: 75, complementos: 2, pasta: fileURLToPath(saida), proporcao: '63 × 88 mm', formato: 'SVG editável' }));
