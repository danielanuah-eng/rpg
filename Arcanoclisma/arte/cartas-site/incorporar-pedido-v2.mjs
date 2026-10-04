// Acrescenta o pedido atual de Claude sem reiniciar a produção nem apagar entregas anteriores.
import { readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
const pasta = new URL('./', import.meta.url), repo = new URL('../../../', pasta);
const pedidoRel = 'Arcanoclisma/cartas/pedidos/lote-site-v2.claude.json';
const ler = (url) => JSON.parse(readFileSync(url, 'utf8'));
const salvar = (url, obj) => writeFileSync(url, JSON.stringify(obj, null, 2) + '\n', 'utf8');
const pedido = ler(new URL(pedidoRel, repo));
if (pedido.necessidadeAtual !== true) throw new Error('Pedido não está ativo.');
const fila = ler(new URL('fila.json', pasta));
const dados = ler(new URL('Arcanoclisma/online/public/jogo/dados.json', repo));
const antigos = new Set(fila.itens.map((i) => i.id));
let adicionados = 0;
for (const c of pedido.cartas) {
  if (antigos.has(c.id)) continue;
  if (!/^[a-z0-9-]+$/.test(c.id) || !dados.catalogo.cartas[c.id]) throw new Error(`Carta ausente do catálogo atual: ${c.id}`);
  const refs = [...(c.personagens ?? []).map((p) => p.arquivo), c.objetoReferencia?.arquivo].filter(Boolean);
  const arteAtual = dados.visual[c.id]?.arte;
  fila.itens.push({
    id: c.id, nome: c.nome, tipo: c.tipo, raridade: c.raridade ?? null, arteAlternativa: c.arteAlternativa === true,
    destino: `Arcanoclisma/arte/cartas-site/${c.id}.png`, fontePedido: pedidoRel,
    pedidoVersao: pedido.versao, prioridade: 2,
    referenciaJogoAtual: arteAtual ? `Arcanoclisma/online/public/arte/${arteAtual}.webp` : null,
    referenciasIdentidade: [...new Set(refs)], estado: 'pendente', baseInspecionada: false, descricaoLida: false,
    identidade: c.identidade, cenaOriginal: c.cena, ambienteOriginal: c.ambiente, variacaoOriginal: c.variacao,
    personagens: c.personagens ?? [], objetoReferencia: c.objetoReferencia ?? null,
  });
  antigos.add(c.id); adicionados++;
}
fila.metaOriginal ??= fila.meta;
fila.meta = fila.itens.length;
fila.pedidosAdicionais = [...new Set([...(fila.pedidosAdicionais ?? []), pedidoRel])];
fila.metaAvaliacaoAutor = 'Ao concluir toda a fila atual; não perguntar a cada cinco artes nem uma a uma.';
fila.valoresAtualizadosDoLote1 = pedido.valoresAtualizadosDoLote1;
salvar(new URL('fila.json', pasta), fila);
const prontas = fila.itens.filter((c) => c.arteNova), pendentes = fila.itens.filter((c) => !c.arteNova).map((c) => c.id);
const progresso = ler(new URL('progresso.json', pasta));
Object.assign(progresso, { meta: fila.meta, concluidas: prontas.length, pendentes, metaOriginal: fila.metaOriginal, pedidosAdicionais: fila.pedidosAdicionais });
salvar(new URL('progresso.json', pasta), progresso);
const entregaURL = new URL('Arcanoclisma/cartas/entregas/lote-site-v1.visual.json', repo), entrega = ler(entregaURL);
Object.assign(entrega, { meta: fila.meta, concluidas: prontas.length, pendentes, metaOriginal: fila.metaOriginal, pedidosAdicionais: fila.pedidosAdicionais });
salvar(entregaURL, entrega);
const galeriaURL = new URL('galeria-dados.js', pasta), prefixo = 'window.ARTES_SITE=';
const texto = readFileSync(galeriaURL, 'utf8');
if (!texto.startsWith(prefixo)) throw new Error('Formato da galeria mudou; conservar arquivo e atualizar pelo registrador.');
const galeria = JSON.parse(texto.slice(prefixo.length).trim().replace(/;$/, ''));
Object.assign(galeria, { meta: fila.meta, concluidas: prontas.length });
writeFileSync(galeriaURL, prefixo + JSON.stringify(galeria) + ';\n', 'utf8');
salvar(new URL('producao.lock.json', pasta), { ativa: true, atualizadaEm: new Date().toISOString(), dono: 'Codex nesta conversa', regra: 'Automação aguarda lock ativa atualizada há menos de 45 minutos' });
console.log(JSON.stringify({ adicionados, meta: fila.meta, prontas: prontas.length, pendentes: pendentes.length, pasta: fileURLToPath(pasta) }));
