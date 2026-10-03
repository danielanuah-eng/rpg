// Gera docs/CARTAS-BASE.md. Uso: node data/gerar-cartas-base.ts
import { writeFileSync } from 'node:fs';
import { avaliar } from '../src/balanco.ts';
import { cartasBase } from '../src/cartas-base.ts';

const tipos: Record<string, string> = { aliado: 'Aliado', temporaria: 'Temporária', mistica: 'Mística', estrutura: 'Estrutura' };
let md = `# Cartas básicas (gerado de src/cartas-base.ts)\n\nReferência neutra: sem Valor, sem arte, nomes provisórios. Todas seguem o orçamento de [ESTATISTICAS-DAS-CARTAS.md](ESTATISTICAS-DAS-CARTAS.md).\n\n| Id | Tipo | Custo | Grau | Vel. | Ataque/Vida | Efeito | Pitch | PP | Orçamento |\n|---|---|---|---|---|---|---|---|---|---|\n`;
for (const c of cartasBase) {
  const a = avaliar(c);
  const corpo = c.ataque !== undefined ? `${c.ataque}/${c.vida}` : c.vida !== undefined ? `-/${c.vida}` : '';
  const extras = [c.palavrasChave?.join(', '), c.descartaGrau ? `descarte ${c.descartaGrau} do Grau` : '', c.texto].filter(Boolean).join('. ');
  md += `| ${c.id} | ${tipos[c.tipo]} | ${c.custo} | ${c.grau} | ${c.velocidade === 'repentina' ? 'repentina' : 'findante'} | ${corpo} | ${extras} | ${c.arcanoGerado} | ${a.pontos} | ${a.orcamento} |\n`;
}
writeFileSync(new URL('../docs/CARTAS-BASE.md', import.meta.url), md);
console.log(`ok: ${cartasBase.length} cartas`);
