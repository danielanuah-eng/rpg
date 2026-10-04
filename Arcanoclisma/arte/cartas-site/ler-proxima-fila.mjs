// Leitura compacta: lê a fonte própria de cada item e a regra atual do catálogo.
import { readFileSync } from 'node:fs';
const pasta = new URL('./', import.meta.url), repo = new URL('../../../', pasta);
const ler = (url) => JSON.parse(readFileSync(url, 'utf8'));
const fila = ler(new URL('fila.json', pasta));
const dados = ler(new URL('Arcanoclisma/online/public/jogo/dados.json', repo));
const fontes = new Map();
const permitidas = new Set(['Arcanoclisma/cartas/pedidos/lote-site-v1.claude.json', 'Arcanoclisma/cartas/pedidos/lote-site-v2.claude.json']);
const itens = fila.itens.filter((c) => !c.arteNova).slice(0, 2).map((item) => {
  if (!permitidas.has(item.fontePedido)) throw new Error('Fonte de pedido não prevista: ' + item.fontePedido);
  if (!fontes.has(item.fontePedido)) fontes.set(item.fontePedido, ler(new URL(item.fontePedido, repo)));
  const pedido = fontes.get(item.fontePedido);
  const c = [...(pedido.cartas ?? []), ...(pedido.conjuradores ?? [])].find((x) => x.id === item.id || 'conjurador-' + x.id === item.id);
  if (!c) throw new Error('Descrição não encontrada: ' + item.id);
  const atual = dados.catalogo.cartas[item.id];
  return { ...item, textos: { ...c.textos, ...(atual?.texto ? { regras: atual.texto } : {}) }, velocidade: atual?.velocidade ?? c.velocidade,
    valoresAtuais: atual?.valores ?? c.valores,
    numeros: { ...c.numeros, ...(atual ? Object.fromEntries(['custo', 'grau', 'arcanoGerado', 'ataque', 'vida', 'defesa', 'vidaDesgaste'].map((k) => [k, atual[k] ?? null])) : {}) } };
});
console.log(JSON.stringify(itens));
