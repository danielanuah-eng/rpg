// Servidor local do app de testes (sem dependências). Serve a pasta app/ e recebe o botão "Enviar":
// cada envio vira um arquivo JSON em Arcanoclisma/tcg/envios/, onde o Claude lê.
// Uso: node app/servidor.ts   →   http://localhost:8765
import { createServer } from 'node:http';
import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { extname, join, normalize, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

import { construir } from './construir.ts';
import { estadoEditor, iniciarSimulacao, lerSimulacao, salvarEdicoes } from './editor-api.ts';

construir();

function lerCorpo(req: import('node:http').IncomingMessage, pronto: (texto: string) => void, res: import('node:http').ServerResponse): void {
  let corpo = '';
  let grande = false;
  req.setEncoding('utf8');
  req.on('data', (p) => { corpo += p; if (corpo.length > 100_000_000) { grande = true; req.destroy(); } });
  req.on('end', () => { if (!grande) pronto(corpo); });
  req.on('error', () => { res.writeHead(400); res.end(); });
}
function json(res: import('node:http').ServerResponse, status: number, dados: unknown): void {
  res.writeHead(status, { 'content-type': 'application/json; charset=utf-8', 'cache-control': 'no-store' });
  res.end(JSON.stringify(dados));
}

const PORTA = Number(process.env.PORTA ?? 8765);
const pastaApp = fileURLToPath(new URL('./', import.meta.url));
const pastaEnvios = fileURLToPath(new URL('../envios/', import.meta.url));
const tipos: Record<string, string> = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.json': 'application/json; charset=utf-8', '.css': 'text/css; charset=utf-8' };

const servidor = createServer((req, res) => {
  const url = new URL(req.url ?? '/', `http://localhost:${PORTA}`);
  if (req.method === 'GET' && url.pathname === '/ping') { res.writeHead(200, { 'content-type': 'application/json' }); res.end('{"ok":true}'); return; }
  // editor de cartas e mecânicas
  if (req.method === 'GET' && url.pathname === '/editor/estado') { try { json(res, 200, estadoEditor()); } catch (erro) { json(res, 500, { erro: String(erro) }); } return; }
  if (req.method === 'GET' && url.pathname === '/editor/simulacao') { json(res, 200, lerSimulacao()); return; }
  if (req.method === 'POST' && url.pathname === '/editor/salvar') {
    lerCorpo(req, (texto) => {
      try { const r = salvarEdicoes(JSON.parse(texto)); if (r.ok) console.log(`Edições salvas (versão ${r.versao}).`); json(res, 200, r); }
      catch (erro) { json(res, 400, { ok: false, erros: [String(erro)], avisos: [] }); }
    }, res);
    return;
  }
  if (req.method === 'POST' && url.pathname === '/editor/simular') {
    lerCorpo(req, (texto) => { try { json(res, 200, iniciarSimulacao(JSON.parse(texto || '{}').porPar)); } catch (erro) { json(res, 400, { ok: false, erro: String(erro) }); } }, res);
    return;
  }
  if (req.method === 'POST' && url.pathname === '/enviar') {
    let corpo = '';
    let grande = false;
    req.setEncoding('utf8');
    req.on('data', (p) => { corpo += p; if (corpo.length > 100_000_000) { grande = true; req.destroy(); } });
    req.on('end', () => {
      if (grande) return;
      try {
        const dados = JSON.parse(corpo);
        mkdirSync(pastaEnvios, { recursive: true });
        const agora = new Date();
        const p2 = (n: number) => String(n).padStart(2, '0');
        const nome = `envio-${agora.getFullYear()}-${p2(agora.getMonth() + 1)}-${p2(agora.getDate())}_${p2(agora.getHours())}-${p2(agora.getMinutes())}-${p2(agora.getSeconds())}.json`;
        writeFileSync(join(pastaEnvios, nome), JSON.stringify(dados, null, 1));
        console.log(`Envio recebido: envios/${nome}`);
        res.writeHead(200, { 'content-type': 'application/json' });
        res.end(JSON.stringify({ ok: true, arquivo: `envios/${nome}` }));
      } catch (erro) {
        res.writeHead(400, { 'content-type': 'application/json' });
        res.end(JSON.stringify({ ok: false, erro: String(erro) }));
      }
    });
    return;
  }
  if (req.method !== 'GET') { res.writeHead(405); res.end(); return; }
  const caminho = normalize(join(pastaApp, url.pathname === '/' ? 'index.html' : decodeURIComponent(url.pathname)));
  if (!caminho.startsWith(pastaApp.endsWith(sep) ? pastaApp : pastaApp + sep) || !existsSync(caminho) || statSync(caminho).isDirectory()) { res.writeHead(404); res.end('Não encontrado'); return; }
  res.writeHead(200, { 'content-type': tipos[extname(caminho)] ?? 'application/octet-stream', 'cache-control': 'no-store' });
  res.end(readFileSync(caminho));
});

servidor.listen(PORTA, '127.0.0.1', () => {
  console.log(`App de testes do Arcanoclisma: http://localhost:${PORTA}`);
  console.log('Deixe esta janela aberta enquanto testa. Os envios vão para Arcanoclisma/tcg/envios/.');
});
