// Aprendizado dos bots pelos diagnósticos (pedido do autor, 09/10/2026).
// Lê as opiniões e ideias marcadas "Yes" no site, pede ao DeepSeek (OpenRouter) ajustes de estilo dos bots e grava
// sim/aprendizado-bot.json. O bot só aplica campos conhecidos do Estilo, com valores validados aqui (nada de código).
// Comportamentos que exigem mudar a lógica do bot ficam em `pedidosDeCodigo`, para o Claude.
// uso: node sim/aprender-diagnosticos.mjs [--site https://...] [--seco]   (--seco: só mostra o que gravaria)
import { writeFileSync, readFileSync, existsSync } from 'node:fs';

const args = process.argv.slice(2);
const SITE = args.includes('--site') ? args[args.indexOf('--site') + 1] : 'https://arcanoclisma.arcanoclisma-online.workers.dev';
const SECO = args.includes('--seco');
const SAIDA = new URL('./aprendizado-bot.json', import.meta.url);
const CHAVE = process.env.OPENROUTER_API_KEY;
if (!CHAVE) { console.error('Falta OPENROUTER_API_KEY no ambiente.'); process.exit(2); }

/** Campos do Estilo (sim/bot.ts) que o aprendizado pode mudar, com o tipo e o intervalo aceitos. */
const CAMPOS = {
  agressividade: ['numero', 0, 1], grauAlvo: ['numero', 1, 12], reservaInstantes: ['bool'], queimaFace: ['bool'], conjAtaca: ['bool'],
  alvejaEstruturas: ['bool'], lento: ['bool'], preveBloqueio: ['bool'], sobeGrau: ['bool'], atacaAliados: ['bool'], valorizaVarredura: ['bool'],
};
function validar(estilo) {
  const r = {};
  for (const [k, v] of Object.entries(estilo ?? {})) {
    const c = CAMPOS[k];
    if (!c) continue;
    if (c[0] === 'bool' && typeof v === 'boolean') r[k] = v;
    if (c[0] === 'numero' && typeof v === 'number' && Number.isFinite(v)) r[k] = Math.min(c[2], Math.max(c[1], v));
  }
  return r;
}

const diag = await (await fetch(`${SITE}/api/diagnostico?formato=md`)).text();
const anterior = existsSync(SAIDA) ? readFileSync(SAIDA, 'utf8') : '{}';
const pedido = `Você ajusta os bots do TCG Arcanoclisma a partir das opiniões dos jogadores.
Campos de estilo disponíveis (e só eles): ${JSON.stringify(CAMPOS)} (agressividade 0-1 = quanto do campo ataca; grauAlvo = cartas no Grau antes de guardar na mão;
preveBloqueio = prevê bloqueios antes de atacar; sobeGrau = sobe Grau até poder jogar a mão; atacaAliados = ataca Aliados quando a troca compensa).
Responda SÓ um JSON: {"geral": {campos para todos os bots}, "porConjurador": {"<id do conjurador em minúsculas>": {campos}},
"pedidosDeCodigo": ["comportamento que não cabe nos campos, em uma frase, com a opinião de origem"], "observacoes": ["resumo curto do que foi aprendido"]}.
Mude só o que as opiniões justificam; sem opinião sobre bots, devolva objetos vazios. Não proponha regras de jogo nem balanceamento de cartas.
Aprendizado anterior (mantenha o que continua valendo): ${anterior}

DIAGNÓSTICO:
${diag}`;

const resp = await fetch('https://openrouter.ai/api/v1/chat/completions', {
  method: 'POST',
  headers: { authorization: `Bearer ${CHAVE}`, 'content-type': 'application/json' },
  body: JSON.stringify({ model: '~deepseek/deepseek-flash-latest', messages: [{ role: 'user', content: pedido }], response_format: { type: 'json_object' } }),
});
if (!resp.ok) { console.error(`OpenRouter: ${resp.status} ${await resp.text()}`); process.exit(1); }
const texto = (await resp.json()).choices?.[0]?.message?.content ?? '';
let bruto;
try { bruto = JSON.parse(texto.replace(/^```(json)?|```$/gm, '')); } catch { console.error('Resposta não é JSON:\n' + texto); process.exit(1); }

const porConjurador = {};
for (const [id, est] of Object.entries(bruto.porConjurador ?? {})) { const v = validar(est); if (Object.keys(v).length) porConjurador[id.toLowerCase()] = v; }
const final = {
  atualizado: new Date().toISOString().slice(0, 10),
  geral: validar(bruto.geral),
  porConjurador,
  pedidosDeCodigo: (bruto.pedidosDeCodigo ?? []).filter((x) => typeof x === 'string').slice(0, 20),
  observacoes: (bruto.observacoes ?? []).filter((x) => typeof x === 'string').slice(0, 20),
};
console.log(JSON.stringify(final, null, 2));
if (!SECO) { writeFileSync(SAIDA, JSON.stringify(final, null, 2) + '\n'); console.error(`Gravado em ${SAIDA.pathname}`); }
