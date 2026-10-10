import http from 'node:http';
import { createReadStream, existsSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { resolve, relative, extname, isAbsolute } from 'node:path';
const root=fileURLToPath(new URL('../Builds/Web/',import.meta.url));
const port=Number(process.env.ARENA_WEB_PORT||8080);
const mime={'.html':'text/html; charset=utf-8','.js':'application/javascript','.wasm':'application/wasm','.data':'application/octet-stream','.json':'application/json','.css':'text/css','.png':'image/png','.svg':'image/svg+xml'};
http.createServer((req,res)=>{
  try {
    const name=decodeURIComponent(new URL(req.url,'http://localhost').pathname);
    const target=resolve(root,'.'+(name==='/'?'/index.html':name));
    const path=relative(root,target);
    if(path.startsWith('..')||isAbsolute(path)) {res.writeHead(403);res.end();return;}
    if(!existsSync(target)||!statSync(target).isFile()) {res.writeHead(404);res.end('Build Web não encontrada.');return;}
    res.writeHead(200,{'Content-Type':mime[extname(target)]||'application/octet-stream','Content-Length':statSync(target).size,'Cross-Origin-Opener-Policy':'same-origin','Cross-Origin-Embedder-Policy':'require-corp','Cache-Control':'no-cache'});
    createReadStream(target).pipe(res);
  } catch {res.writeHead(400);res.end();}
}).listen(port,'127.0.0.1',()=>console.log(`Arena Tática Web: http://127.0.0.1:${port}`));
