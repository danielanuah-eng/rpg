// Gera data/valores.json e docs/TABELA-DE-VALORES.md. Uso: node data/gerar-tabela.mjs
// Notas do autor (respostas-autor.mjs) valem; palpites do Claude (valores.mjs) aparecem como ~N até o autor responder.
import { writeFileSync } from 'node:fs';
import { aviso, perguntas, universais, valores } from './valores.mjs';
import { descritivas, ordem, provisorias, respostas } from './respostas-autor.mjs';

if (ordem.join() !== valores.map((v) => v.id).join()) throw new Error('A ordem dos Valores em respostas-autor.mjs difere de valores.mjs');
const idsEscala = new Set(perguntas.filter((q) => q.tipo === 'escala').map((q) => q.id));
for (const id of Object.keys(respostas)) if (!idsEscala.has(id)) throw new Error(`Pergunta desconhecida em respostas-autor: ${id}`);
for (const [id, v] of Object.entries(respostas)) if (v.length !== valores.length || v.some((n) => !Number.isInteger(n) || n < 0 || n > 4)) throw new Error(`Respostas inválidas: ${id}`);

for (const [qid, porValor] of Object.entries(descritivas)) for (const v of valores) v.descritivas[qid] = porValor[v.id] ?? (qid === 'conjurador' ? 'nenhuma (autor)' : '—');
// nota final = autor, senão palpite; guarda a origem
for (const v of valores) {
  v.notasAutor = {};
  v.palpites = {};
  for (const q of perguntas) {
    if (q.tipo !== 'escala') continue;
    const i = ordem.indexOf(v.id);
    if (respostas[q.id]) v.notasAutor[q.id] = respostas[q.id][i];
    else if (provisorias[q.id]) v.palpites[q.id] = provisorias[q.id][i];
    else if (v.notas[q.id] !== undefined) v.palpites[q.id] = v.notas[q.id];
  }
  v.notas = { ...v.palpites, ...v.notasAutor };
}
writeFileSync(new URL('./valores.json', import.meta.url), JSON.stringify({ aviso, universais, perguntas, valores }, null, 1));

const esc = perguntas.filter((q) => q.tipo === 'escala');
const celula = (v, id) => (v.notasAutor[id] !== undefined ? `**${v.notasAutor[id]}**` : v.palpites[id] !== undefined ? `~${v.palpites[id]}` : '—');
let md = `# Tabela de Valores (0.3)\n\n${aviso}\n\nEscala: 0 Péssimo · 1 Mais ou menos · 2 Normal · 3 Bom · 4 Muito bem.\nLegenda: **negrito** = resposta do autor · ~N = palpite do Claude, ainda a confirmar · — = não definido.\n\n## Em todos os Valores\n\n- ${universais.dragoes}\n- ${universais.divindades}\n- ${universais.aliados}\n- ${universais.cartas}\n\n## Os 9 Valores\n\n| Valor | Cor | Temperatura | Forma da magia | Ideia |\n|---|---|---|---|---|\n`;
for (const v of valores) md += `| **${v.nome}** | ${v.cor} | ${v.temperatura} | ${v.forma} | ${v.ideia} |\n`;
md += `\n## Perguntas de escala\n\n| # | Pergunta | ${valores.map((v) => v.nome).join(' | ')} |\n|---|---|${valores.map(() => '---').join('|')}|\n`;
for (const q of esc) md += `| ${q.n} | ${q.texto} | ${valores.map((v) => celula(v, q.id)).join(' | ')} |\n`;
md += '\n## Perguntas descritivas\n\n';
for (const q of perguntas.filter((q) => q.tipo !== 'escala')) {
  md += `**${q.n}. ${q.texto}**\n\n`;
  for (const v of valores) md += `- ${v.nome}: ${v.descritivas[q.id] ?? '—'}\n`;
  md += '\n';
}
const respondidas = Object.keys(respostas).length;
md += `## Cobertura\n\nRespondidas pelo autor: ${respondidas}/${esc.length} perguntas de escala (todas as colunas).\n\n`;
for (const v of valores) md += `- ${v.nome}: ${Object.keys(v.palpites).length} palpites do Claude ainda sem confirmação\n`;
md += '\nNota: o rascunho original repete os números 32 e 33; aqui as perguntas foram renumeradas de 1 a 43 na ordem em que aparecem.\n';
writeFileSync(new URL('../docs/TABELA-DE-VALORES.md', import.meta.url), md);
console.log('ok');
