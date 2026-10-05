// Ícones vetoriais da interface: editar os desenhos abaixo e executar com Node.
// Não altera regras, gatilhos, arquivos de integração nem os 32 ícones de mecânicas.
import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {createRequire} from 'node:module';

const arcano = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..');
const sharp = createRequire(path.join(arcano, 'online/package.json'))('sharp');
const destino = path.join(arcano, 'online/public/kit/interface-v20/icones');
const urlBase = '/kit/interface-v20/icones';
const pedido = JSON.parse(await fs.readFile(path.join(arcano, 'online/pedidos-visuais/site.claude.json'), 'utf8'));
const agora = new Date().toISOString();
const dourado = '#e0bd6a', veludo = '#0e0b1d';
const caminhoEntrega = path.join(arcano, 'online/pedidos-visuais/site-online-v1.visual.json');
// Não sobrescrever uma entrega geral ou a contribuição de outro agente.
try {
  const anterior = JSON.parse(await fs.readFile(caminhoEntrega, 'utf8'));
  if (anterior.autor !== 'Codex' || anterior.versao !== 20 || anterior.total !== 23) {
    throw new Error('A entrega geral mudou; preserve-a e registre este lote em um arquivo separado.');
  }
} catch (erro) {
  if (erro.code !== 'ENOENT') throw erro;
}

const grupos = [
  {
    id: 'icones-etapas', nome: 'Etapas do turno', tamanho: 48,
    icones: [
      ['etapa-inicio', 'Início', '<path d="M9 43h46M15 50h34M20 43a12 12 0 0 1 24 0M32 11v8M12 23l6 6M52 23l-6 6M7 36h7M50 36h7"/><path d="m27 30 5-4 5 4" fill="none"/>'],
      ['etapa-principal', 'Principal', '<path d="m32 6 6 17 16-7-8 16 12 7-18 4-8 15-8-15-18-4 12-7-8-16 16 7Z"/><path d="m32 22 10 10-10 10-10-10Z" fill="#6f9be0" fill-opacity=".28"/><circle cx="32" cy="32" r="3" fill="#e0bd6a" stroke="none"/>'],
      ['etapa-cataclisma', 'Cataclisma', '<path d="M27 7 7 32l20 25 6-16-11-8 12-12ZM38 7l19 25-19 25 9-20-13-8 10-12Z"/><path d="m35 10-7 17 11 8-8 19" stroke="#d27aa6"/><path d="M12 10 7 6M52 54l5 4M6 48l5-3M54 17l4-3"/>'],
      ['etapa-final', 'Final', '<path d="M36 8a19 19 0 1 0 13 29A20 20 0 0 1 36 8Z"/><rect x="36" y="40" width="16" height="17" rx="3"/><path d="M53 27a13 13 0 0 1 4 14m-7-4 7 4 2-8M43 15l2-4 2 4 4 2-4 2-2 4-2-4-4-2Z"/>'],
    ],
  },
  {
    id: 'icones-zonas', nome: 'Zonas e equipamentos', tamanho: 32,
    icones: [
      ['zona-baralho', 'Baralho', '<path d="M17 12h30a3 3 0 0 1 3 3v36M12 17h30a3 3 0 0 1 3 3v36"/><rect x="7" y="22" width="33" height="36" rx="3"/><path d="m24 30 8 10-8 10-8-10Z"/><path d="M22 7h29a4 4 0 0 1 4 4v35"/>'],
      ['zona-cemiterio', 'Cemitério', '<path d="M16 51V23a16 16 0 0 1 32 0v28ZM9 51h46v7H9Z"/><path d="m32 19 7 9-7 9-7-9ZM25 44h14M6 47l5 4M58 47l-5 4"/>'],
      ['zona-banimento', 'Banimento', '<ellipse cx="26" cy="32" rx="17" ry="25"/><ellipse cx="26" cy="32" rx="9" ry="17"/><path d="M29 32h27m-8-8 8 8-8 8"/><path d="M5 11l5 4M5 53l5-4"/>'],
      ['zona-arcano', 'Enviadas ao Arcano', '<rect x="24" y="5" width="16" height="20" rx="3"/><path d="M32 29v10m-6-6 6 6 6-6M10 31a22 22 0 1 0 44 0M9 45h46M17 53h30"/><path d="m20 36 3 5m18-5-3 5" stroke="#6f9be0"/>'],
      ['zona-mao-oponente', 'Mão do oponente', '<path d="m9 34-4-17 16-4 7 28ZM25 11h16v30H25ZM41 16l13 4-6 21-11-2Z"/><path d="M9 50q23-22 46 0-23 20-46 0Z"/><circle cx="32" cy="50" r="5" fill="#6f9be0" fill-opacity=".28"/>'],
      ['slot-canal', 'Canal', '<path d="m34 6 15 9-3 15-13 4-11-11ZM33 34 16 56l-7-6 20-24M14 43l8 7M34 6l-1 17 16-8M22 23h11l13 7"/><path d="m52 7 5-3M52 32l6 2"/>'],
      ['slot-armadura', 'Armadura', '<path d="m23 8 9 8 9-8 13 10-8 13 3 24-17 4-17-4 3-24L10 18Z"/><path d="M23 8q-2 22-5 23m23-23q2 22 5 23M32 16v32M20 37l12 7 12-7M24 52l8 2 8-2"/>'],
      ['slot-extra', 'Extra', '<path d="M22 7q10 9 20 0M26 10l3 13m9-13-3 13"/><circle cx="32" cy="40" r="17"/><path d="m32 27 4 9 10 4-10 4-4 9-4-9-10-4 10-4Z" fill="#d27aa6" fill-opacity=".18"/>'],
    ],
  },
  {
    id: 'selos-comunidade', nome: 'Status e categorias', tamanho: 64,
    icones: [
      ['status-novo', 'Novo', '<path d="m32 10 6 16 16 6-16 6-6 16-6-16-16-6 16-6Z"/><path d="M12 13l-5-5M51 13l5-5M12 51l-5 5M51 51l5 5"/>'],
      ['status-aprovado', 'Aprovado para mudar', '<path d="m32 7 7 5 9 1 3 9 6 10-6 10-3 9-9 1-7 5-7-5-9-1-3-9-6-10 6-10 3-9 9-1Z"/><path d="m20 32 8 8 16-17" stroke-width="4"/>'],
      ['status-em-revisao', 'Em revisão', '<rect x="9" y="7" width="31" height="46" rx="4"/><path d="M17 18h15M17 26h10M17 34h7"/><circle cx="39" cy="38" r="12"/><path d="m47 47 10 10" stroke-width="4"/>'],
      ['status-nao-vai-mudar', 'Não vai mudar', '<path d="M21 28V18a11 11 0 0 1 22 0v10"/><rect x="13" y="28" width="38" height="29" rx="5"/><circle cx="32" cy="40" r="4"/><path d="M32 44v6M21 53h22"/>'],
      ['status-feito', 'Feito', '<circle cx="32" cy="32" r="25"/><path d="m13 32 8 8 16-17M29 38l5 5 17-19" stroke-width="4"/>'],
      ['categoria-expansao', 'Expansão', '<path d="m12 48-5-29 16-3 6 30ZM38 16l18 4-6 30-18-4Z"/><rect x="23" y="9" width="19" height="42" rx="3"/><path d="m32 19 3 7 7 3-7 3-3 7-3-7-7-3 7-3Z"/>'],
      ['categoria-personagem', 'Personagem', '<circle cx="32" cy="20" r="12"/><path d="M9 56v-6q0-14 16-17l7 6 7-6q16 3 16 17v6ZM21 48v8M43 48v8"/>'],
      ['categoria-carta', 'Carta', '<rect x="13" y="6" width="38" height="52" rx="5"/><path d="m32 15 12 13-12 13-12-13ZM21 48h22M25 53h14"/>'],
      ['categoria-item', 'Item', '<path d="M23 8h18v7H23ZM27 15v12L15 44q-4 11 9 13h16q13-2 9-13L37 27V15M20 43h24"/><path d="m32 31 5 6-5 6-5-6Z" fill="#d27aa6" fill-opacity=".22"/>'],
      ['categoria-conjurador', 'Conjurador', '<path d="m17 31 13-24 12 25M10 34q22-9 44 0l-3 6H13ZM24 41q8 11 16 0M20 56l7-9m17 9-7-9"/><path d="m47 9 2 5 5 2-5 2-2 5-2-5-5-2 5-2Z" fill="#6f9be0" fill-opacity=".28"/>'],
      ['categoria-mecanica', 'Mecânica', '<path d="M27 7h10l2 8 6 3 8-3 5 9-6 6v6l6 6-5 9-8-3-6 3-2 8H27l-2-8-6-3-8 3-5-9 6-6v-6l-6-6 5-9 8 3 6-3Z"/><circle cx="32" cy="33" r="10"/>'],
    ],
  },
];

await fs.mkdir(destino, {recursive: true});
const manifesto = {}, arquivos = [];
for (const grupo of grupos) {
  for (const [id, nome, desenho] of grupo.icones) {
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${grupo.tamanho}" height="${grupo.tamanho}" viewBox="0 0 64 64"><g fill="${veludo}" stroke="${dourado}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">${desenho}</g></svg>\n`;
    await fs.writeFile(path.join(destino, `${id}.svg`), svg);
    // Medição do alfa do SVG rasterizado, sem fundo; não é teste do jogo.
    const {data, info} = await sharp(Buffer.from(svg)).ensureAlpha().raw().toBuffer({resolveWithObject: true});
    let transparentes = 0, parciais = 0, opacos = 0;
    for (let i = 3; i < data.length; i += info.channels) {
      if (data[i] === 0) transparentes++;
      else if (data[i] === 255) opacos++;
      else parciais++;
    }
    const arquivo = `${urlBase}/${id}.svg`;
    manifesto[id] = {arquivo, nome, grupo: grupo.id, largura: grupo.tamanho, altura: grupo.tamanho, okGpt: true};
    arquivos.push({id, nome, grupo: grupo.id, arquivo, largura: info.width, altura: info.height,
      viewBox: [0, 0, 64, 64], sha256: createHash('sha256').update(svg).digest('hex'),
      alfa: {medidoCom: 'sharp: SVG rasterizado no tamanho nominal, sem fundo', transparentes, parciais, opacos,
        percentualTransparente: Number((100 * transparentes / (info.width * info.height)).toFixed(2))}});
  }
}
await fs.writeFile(path.join(destino, 'manifesto.json'), JSON.stringify(manifesto, null, 2) + '\n');

const escapar = texto => texto.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('"', '&quot;');
const catalogo = `<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Arcanoclisma — ícones da interface v20</title><style>
*{box-sizing:border-box}body{margin:0;padding:28px;background:#0e0b1d;color:#eee8da;font:16px/1.5 "Segoe UI",sans-serif}main{max-width:1250px;margin:auto}h1{color:#e0bd6a;font-size:26px}h2{font-size:20px;margin-top:30px}p{color:#b5afc6}section{display:grid;grid-template-columns:repeat(auto-fill,minmax(215px,1fr));gap:12px}article{padding:17px;background:#181329;border:1px solid #e0bd6a33;border-radius:12px}strong{display:block;font-size:15px}small{display:block;color:#aaa1be;font-size:12px}.amostras{height:80px;display:flex;align-items:center;gap:24px}.claro{background:#e8dcc4;padding:6px;border-radius:7px;line-height:0}img{vertical-align:middle}a{color:#e0bd6a}
</style><main><h1>Ícones da interface · v20</h1><p>23 SVGs para etapas, zonas e comunidade. Amostras no tamanho nominal e a 24 px, em fundo escuro e claro. Os rótulos são HTML editável.</p>${grupos.map(g => `<h2>${g.nome} · ${g.tamanho} px</h2><section>${g.icones.map(([id,nome]) => `<article><div class="amostras"><img src="${id}.svg" width="${g.tamanho}" height="${g.tamanho}" alt=""><img src="${id}.svg" width="24" height="24" alt=""><span class="claro"><img src="${id}.svg" width="24" height="24" alt=""></span></div><strong>${escapar(nome)}</strong><small>${id}</small></article>`).join('')}</section>`).join('')}<p><a href="manifesto.json">Manifesto de integração</a></p></main></html>\n`;
await fs.writeFile(path.join(destino, 'catalogo.html'), catalogo);

// Prancha de apresentação derivada dos SVGs; só esta prévia tem fundo opaco.
const prancha = ['<rect width="1250" height="1080" fill="#0e0b1d"/>',
  '<text x="25" y="40" fill="#e0bd6a" font-size="25">Arcanoclisma · ícones da interface v20</text>',
  '<text x="25" y="69" fill="#bab1cb" font-size="14">Desenho ampliado · tamanho nominal · 24 px em fundo claro</text>'];
let y = 115;
for (const grupo of grupos) {
  prancha.push(`<text x="25" y="${y}" fill="#e0bd6a" font-size="19">${grupo.nome} · ${grupo.tamanho} px</text>`);
  y += 18;
  for (let i = 0; i < grupo.icones.length; i++) {
    const [id, nome, desenho] = grupo.icones[i];
    const x = 25 + (i % 4) * 305, topo = y + Math.floor(i / 4) * 130;
    prancha.push(`<rect x="${x}" y="${topo}" width="291" height="119" rx="12" fill="#181329" stroke="#e0bd6a" stroke-opacity=".18"/>`);
    prancha.push(`<rect x="${x+231}" y="${topo+27}" width="38" height="38" rx="6" fill="#e8dcc4"/>`);
    for (const [dx, px] of [[14,64],[125,grupo.tamanho],[238,24]]) {
      prancha.push(`<svg x="${x+dx}" y="${topo+46-px/2}" width="${px}" height="${px}" viewBox="0 0 64 64"><g fill="${veludo}" stroke="${dourado}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">${desenho}</g></svg>`);
    }
    prancha.push(`<text x="${x+14}" y="${topo+97}" fill="#eee8da" font-size="14">${escapar(nome)}</text>`);
  }
  y += Math.ceil(grupo.icones.length / 4) * 130 + 35;
}
await sharp(Buffer.from(`<svg xmlns="http://www.w3.org/2000/svg" width="1250" height="1080" viewBox="0 0 1250 1080" font-family="Segoe UI, sans-serif">${prancha.join('')}</svg>`)).png().toFile(path.join(destino, 'previa.png'));

const ajuste = {
  id: 'v19-protegido-sem-numero', estado: 'entregue',
  arquivo: 'Arcanoclisma/online/public/js/iconografia-v19.js',
  expressao: '/^\\s*protegido\\b(?:\\s+\\d+)?/i',
  descricao: 'Agrupa ícone com Protegido, com ou sem número; conserva o número no mesmo grupo quando existe.',
  integracaoClaudeConfirmada: false,
};
const entrega = {
  tarefaId: 'site-online-v1', autor: 'Codex', versao: 20, pedidoVersao: pedido.versao,
  atualizadoEm: agora, estado: 'entrega_parcial',
  escopo: 'Somente os três conjuntos de ícones da interface solicitados pelo Claude.',
  pedidosAtendidos: grupos.map(g => ({id: g.id, estado: 'entregue', total: g.icones.length})),
  total: arquivos.length, manifesto: 'Arcanoclisma/online/public/kit/interface-v20/icones/manifesto.json',
  catalogo: 'Arcanoclisma/online/public/kit/interface-v20/icones/catalogo.html',
  previa: 'Arcanoclisma/online/public/kit/interface-v20/icones/previa.png',
  gerador: 'Arcanoclisma/arena/scripts/gerar-icones-interface-v20.mjs',
  formato: 'SVG com fundo transparente, contorno dourado #e0bd6a e interior veludo #0e0b1d; sem texto ou números embutidos.',
  conferenciaVisual: {
    estado: 'conferida_pelo_codex',
    evidencia: 'Arcanoclisma/online/public/kit/interface-v20/icones/previa.png',
    escopo: 'Prancha dos 23 desenhos: ampliação, tamanho nominal e redução a 24 px em fundos escuro e claro.',
    noJogo: 'Aguardando integração e conferência do Claude; o jogo não foi executado nesta entrega.',
  },
  arquivos,
  integracao: {
    estado: 'aguardando_claude', confirmada: false,
    orientacoes: [
      'Ler o manifesto separado da interface; não mesclar estes IDs aos gatilhos de palavras-chave de icones.js.',
      'Usar <img src="arquivo" width="largura" height="altura" alt=""> ao lado de um rótulo HTML; se o ícone estiver sozinho, dar nome acessível ao controle.',
      'Os SVGs da interface têm cores próprias para manter a linha dourada e o preenchimento escuro. Não aplicar a máscara currentColor da iconografia de mecânicas.',
      'Seleção, hover e desabilitado pertencem ao botão HTML; os SVGs não contêm lógica ou rótulos.',
      'O kit anterior de 32 mecânicas permanece em /kit/icones/ e tem integração confirmada pelo Claude. O adaptador recebeu o ajuste de Protegido sem número.',
    ],
    respostaEsperada: 'Arcanoclisma/online/pedidos-visuais/site-online-v1.integracao.claude.json',
  },
  ajusteIconografia: ajuste,
  observacoes: [
    'Esta entrega parcial não conclui os outros cenários, botões e peças do pedido geral do site.',
    'Não executados testes, build ou simulações do jogo nesta etapa, conforme orientação atual do autor.',
    'Push das entregas visuais autorizado pelo autor. Preservar arquivos concorrentes de Claude.',
  ],
};
await fs.writeFile(caminhoEntrega, JSON.stringify(entrega, null, 2) + '\n');
const pedidoMecanicas = JSON.parse(await fs.readFile(path.join(arcano, 'online/pedidos-visuais/iconografia.claude.json'), 'utf8'));
await fs.writeFile(path.join(arcano, 'online/entregas-visuais/iconografia-v19-ajuste-protegido.codex.json'), JSON.stringify({
  tarefaId: pedidoMecanicas.tarefaId, autor: 'Codex', versao: 20, pedidoVersao: pedidoMecanicas.versao,
  atualizadoEm: agora, ...ajuste,
  kitMecanicas: {total: 32, manifesto: 'Arcanoclisma/online/public/kit/icones/manifesto.json',
    estado: 'entregue_anteriormente', integracaoAnteriorConfirmada: true,
    retornoClaude: 'Arcanoclisma/online/entregas-visuais/iconografia-v19.integracao.claude.json'},
  testes: 'Não executados; alteração visual solicitada e inspecionada no código. Nenhuma alteração em icones.js ou nas regras.',
}, null, 2) + '\n');
console.log(JSON.stringify({total: arquivos.length, grupos: entrega.pedidosAtendidos,
  pasta: destino, entrega: 'online/pedidos-visuais/site-online-v1.visual.json',
  alfaTransparenteMinimo: Math.min(...arquivos.map(a => a.alfa.percentualTransparente))}));
