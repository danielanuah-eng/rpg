// Aprova somente as 210 versões correntes; estudos e variantes ficam preservados.
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const pasta = path.dirname(fileURLToPath(import.meta.url));
const ler = async nome => JSON.parse(await fs.readFile(path.join(pasta, nome), 'utf8'));
const salvar = (nome, dados) => fs.writeFile(path.join(pasta, nome), JSON.stringify(dados, null, 2) + '\n');
const progresso = await ler('progresso.json');
if (progresso.meta !== 210 || progresso.concluidas !== 210 || progresso.pendentes.length) {
  throw new Error('Esta aprovação é exclusiva do lote completo de 210 artes.');
}
const aprovacao = {
  textoAutor: 'aprovado, voce fez o dado e a animação que pedi?',
  registradaEm: new Date().toISOString(),
  origem: 'Autor nesta conversa',
  escopo: '210 versões correntes do lote do site; não libera a expansão para jogo.'
};
const fila = await ler('fila.json');
for (const carta of fila.itens) {
  if (!carta.arteNova || !carta.registro) throw new Error('Entrega ausente: ' + carta.id);
  carta.estado = 'aprovada_pelo_autor';
  const registroPath = path.resolve(pasta, '../../..', carta.registro);
  const registro = JSON.parse(await fs.readFile(registroPath, 'utf8'));
  registro.estadoAntesDaAprovacao ??= registro.estado;
  registro.estado = carta.estado;
  registro.aprovacaoAutor = aprovacao;
  await fs.writeFile(registroPath, JSON.stringify(registro, null, 2) + '\n');
}
fila.estado = 'aprovado';
fila.aprovacaoAutor = aprovacao;
await salvar('fila.json', fila);
Object.assign(progresso, { estado: 'aprovado', aguardandoAvaliacaoFinal: false,
  aprovacaoAutor: aprovacao, automacaoCarlos: 'PAUSED — confirmada via ferramenta',
  proximaEtapa: 'D20 Transplanar e entrada gradual da arena',
  producaoPermitidaEnquantoAguarda: false });
await salvar('progresso.json', progresso);
const mapaPath = path.resolve(pasta, '../../cartas/entregas/lote-site-v1.visual.json');
const mapa = JSON.parse(await fs.readFile(mapaPath, 'utf8'));
mapa.estado = 'aprovado'; mapa.aprovacaoAutor = aprovacao;
for (const carta of mapa.cartas) carta.estado = 'aprovada_pelo_autor';
await fs.writeFile(mapaPath, JSON.stringify(mapa, null, 2) + '\n');
const texto = await fs.readFile(path.join(pasta, 'galeria-dados.js'), 'utf8');
const galeria = JSON.parse(texto.slice(texto.indexOf('{')).replace(/;\s*$/, ''));
for (const carta of galeria.cartas) carta.estado = 'aprovada_pelo_autor';
await fs.writeFile(path.join(pasta, 'galeria-dados.js'), 'window.ARTES_SITE=' + JSON.stringify(galeria) + ';\n');
console.log(JSON.stringify({ estado: 'aprovado', artes: fila.itens.length, automacao: 'PAUSED' }));
