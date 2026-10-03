import fs from 'node:fs/promises';
import path from 'node:path';
import crypto from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { execFileSync } from 'node:child_process';

const root = path.dirname(fileURLToPath(import.meta.url));
const repo = path.resolve(root, '../..');
const library = path.resolve(repo, '../Biblioteca-Enzo');
const read = async file => JSON.parse((await fs.readFile(file, 'utf8')).replace(/^\uFEFF/, ''));
const write = (file, value) => fs.writeFile(file, JSON.stringify(value, null, 2) + '\n');
const progress = await read(path.join(root, 'progresso.json'));
const stateFile = path.join(root, 'retomada.json');
const state = await read(stateFile);
if (state.estado === 'aprovado' && state.pacoteFinal && state.arquivamentoEnzo === 'preservação final concluída') {
  console.log(JSON.stringify({ estado: 'já encerrado', pacote: state.pacoteFinal }));
  process.exit(0);
}
if (progress.total !== 435 || progress.pendentes !== 0) throw new Error('Catálogo incompleto; encerramento bloqueado.');
for (const asset of progress.imagens) await fs.access(path.join(root, asset.arquivo));
const approval = { estado: 'aprovada', textoAutor: 'Pronto, pode confirmar todas elas', registradaEm: new Date().toISOString(), origem: 'Solicitação direta do autor nesta conversa', escopo: '435 imagens atuais do catálogo 087–521, incluindo Aurelhaz com segundo braço; versões anteriores não são aprovadas por esta decisão.' };
const versions = path.join(library, 'pacotes/arcanoclisma');
await fs.mkdir(versions, { recursive: true });
let version = '2026-10-03-v1';
for (let n = 1; ; n++) {
  version = `2026-10-03-v${n}`;
  try { await fs.mkdir(path.join(versions, version)); break; }
  catch (error) {
    if (error.code !== 'EEXIST') throw error;
    try { await fs.access(path.join(versions, version, 'manifesto.json')); }
    catch (missing) { if (missing.code === 'ENOENT') break; throw missing; }
  }
}
const pack = path.join(versions, version);
for (const folder of ['instrucoes', 'skills', 'codigo']) await fs.mkdir(path.join(pack, folder), { recursive: true });
await write(path.join(root, 'registros/aprovacao-final-2026-10-03.json'), approval);
for (const asset of progress.imagens) {
  const file = path.join(root, 'registros', `${String(asset.numero).padStart(3, '0')}.json`);
  let record;
  try { record = await read(file); }
  catch (error) {
    if (error.code !== 'ENOENT') throw error;
    record = { numero: asset.numero, id: asset.id, titulo: asset.titulo, arquivo: asset.arquivo, ferramenta: 'não informado', nota: 'Registro de aprovação criado no encerramento; prompt e procedência originais indisponíveis neste registro. Não inventar esses dados.' };
  }
  record.aprovacaoAutor = approval;
  if (record.pedidoAutor?.avaliacao) record.pedidoAutor.avaliacao = 'aprovada pelo autor no encerramento';
  await write(file, record);
  asset.aprovacaoAutor = { estado: 'aprovada', registradaEm: approval.registradaEm };
}
await write(path.join(root, 'progresso.json'), progress);
Object.assign(state, { estado: 'aprovado', avaliacaoAutor: approval.textoAutor, aprovacaoFinal: approval, avaliacaoPendente: [], revisoesSolicitadas: [], aprovadasPeloAutor: progress.imagens.map(x => x.numero), atualizadoEm: approval.registradaEm, atualizadaEm: approval.registradaEm, automacaoCarlos: 'PAUSED — confirmada via ferramenta', pacoteFinal: pack, arquivamentoEnzo: 'preservação final em andamento', nota: 'Todas as 435 imagens aprovadas, incluindo a correção de Aurelhaz. Ciclo de produção encerrado. Nenhum push autorizado ou realizado.' });
await write(stateFile, state);
const archiveScript = path.join(library, 'scripts/arquivar.mjs');
console.log(execFileSync(process.execPath, [archiveScript, root], { encoding: 'utf8', maxBuffer: 1024 * 1024 }).trim());
const indexFile = path.join(library, 'indices/arcanoclisma.json');
const index = await read(indexFile);
const approvedHashes = new Map(progress.imagens.map(x => [x.arquivo, x.sha256]));
for (const asset of index.assets) {
  if (approvedHashes.get(asset.origem) === asset.sha256) {
    asset.aprovacao = approval;
    const record = await read(path.join(root, 'registros', `${String(asset.numero).padStart(3, '0')}.json`));
    asset.registro = record;
  }
}
await write(indexFile, index);
const copies = [
  [path.join(repo, 'AGENTS.md'), 'instrucoes/AGENTS-repositorio.md'],
  [path.join(repo, 'Arcanoclisma/AGENTS.md'), 'instrucoes/AGENTS-Arcanoclisma.md'],
  [path.join(root, 'agente-artes.md'), 'instrucoes/agente-artes.md'],
  [path.join(root, '27 Estilo de arte - o traço.md'), 'instrucoes/estilo.md'],
  [path.join(root, 'variacao-personagens.md'), 'instrucoes/variacao-personagens.md'],
  [path.join(root, 'registros/aprovacao-final-2026-10-03.json'), 'instrucoes/aprovacao-final.json'],
  [path.join(repo, '.agents/skills/carlos/SKILL.md'), 'skills/carlos.md'],
  [path.join(repo, '.agents/skills/enzo/SKILL.md'), 'skills/enzo.md'],
  [path.join(repo, '.agents/skills/copiao/SKILL.md'), 'skills/copiao.md'],
  ['C:/Users/Anuah/.codex/skills/.system/imagegen/SKILL.md', 'skills/imagegen.md'],
  [path.join(root, 'salvar-geracao.mjs'), 'codigo/salvar-geracao.mjs'],
  [path.join(root, 'conferir.mjs'), 'codigo/conferir.mjs'],
  [path.join(root, 'galeria.mjs'), 'codigo/galeria.mjs'],
  [path.join(root, 'encerrar-projeto.mjs'), 'codigo/encerrar-projeto.mjs'],
  [archiveScript, 'codigo/arquivar.mjs']
];
const files = [];
for (const [origin, target] of copies) {
  const buffer = await fs.readFile(origin);
  await fs.writeFile(path.join(pack, target), buffer);
  files.push({ arquivo: target, origem: origin, sha256: crypto.createHash('sha256').update(buffer).digest('hex') });
}
for (const agent of ['carlos', 'enzo']) {
  await fs.mkdir(path.join(library, 'definicoes', agent), { recursive: true });
  await fs.copyFile(path.join(repo, '.agents/skills', agent, 'SKILL.md'), path.join(library, 'definicoes', agent, 'SKILL.md'));
}
const entry = `# Arcanoclisma — pacote final aprovado\n\nO autor aprovou as 435 imagens atuais do catálogo 087–521 em sua mensagem “Pronto, pode confirmar todas elas”. A revisão de Aurelhaz (510) acrescentou o segundo braço preservando o desenho. Versões antigas foram preservadas, mas esta aprovação corresponde aos arquivos atuais.\n\n## Entradas e assets\n\nO índice de Enzo está em ../../../indices/arcanoclisma.json, relativo a este pacote. Cada entrada aponta para um objeto SHA-256 em ../../../objetos/. As entradas cujo campo aprovacao tem estado aprovada identificam os arquivos finais; versões anteriores preservam sua procedência. Descrições e prompts estão nos registros arquivados e nas entradas do índice. Não inferir materiais de Claude inacessíveis.\n\n## Procedimentos utilizados\n\n1. Ler catálogo, direção de arte e preferências; produzir ou editar somente o escopo autorizado.\n2. Usar image_gen integrada. Para editar, inspecionar o alvo com view_image e fornecer o arquivo como referência. Solicitar alfa transparente, limitar a alteração e preservar identidade, pose e estilo.\n3. Copiar a saída para imagens/<id>.png, preservando o original da ferramenta e versões anteriores. Registrar prompt, fonte, adaptação e decisão em registros/<numero>.json.\n4. Executar conferir.mjs no projeto original com Node.js. Usa Sharp, mede alfa, cantos e pixels, grava progresso e gera galeria. Medição não equivale a aprovação estética.\n5. No encerramento aprovado, executar encerrar-projeto.mjs. Preserva decisão, registros, biblioteca e pacote. A pausa de Carlos foi feita pela ferramenta automation_update, não pelo script.\n\nComandos efetivamente usados no encerramento:\n\n- Node.js: ${process.execPath}\n- Arquivamento: node "${archiveScript}" "${root}"\n- Encerramento: node "${path.join(root, 'encerrar-projeto.mjs')}"\n\nOs scripts copiados são snapshots. Conferir/galeria/salvar dependem da estrutura original com catálogo, registros e imagens; Sharp tem caminho absoluto local, a adaptar em outra máquina. Arquivar depende de sua localização em Biblioteca-Enzo/scripts. Encerrar depende da estrutura Rpg/Arcanoclisma/arte ao lado de Biblioteca-Enzo. Não executar os snapshots isoladamente sem adaptar os caminhos.\n\n## Ferramentas e reprodução local\n\nNode.js ${process.version}, sistema de arquivos e Sharp fazem salvamento, hash, medição e indexação localmente. image_gen e automation_update são ferramentas remotas desta sessão, não bibliotecas offline. O pacote não contém pesos, capacidades internas do Codex ou credenciais. Uma IA offline pode ler procedimentos, prompts e código, mas precisa de uma alternativa de geração de imagens para reproduzir o fluxo sem rede. Alternativa local não foi implementada neste trabalho.\n\nCanva, Unity e modelos de movimento não foram usados na produção destes assets. Claude cuida de código e integração; imagens estáticas não incluem lógica de movimento. Nenhum push foi realizado. As automações auxiliares permanecem pausadas.\n`;
await fs.writeFile(path.join(pack, 'ENTRADA.md'), entry);
await write(path.join(pack, 'manifesto.json'), { projeto: 'Arcanoclisma', versao: version, criadoEm: approval.registradaEm, aprovacao: approval, totalAprovadas: progress.total, indiceBiblioteca: '../../../indices/arcanoclisma.json', arquivos: files, ferramentasUtilizadas: ['image_gen integrada: geração e edição remotas', 'view_image: inspeção visual', 'Node.js e Sharp: arquivos, alfa, hash e galeria locais', 'arquivar.mjs: biblioteca local', 'automation_update: pausa de Carlos'], procedimentosLocais: ['salvamento', 'alfa', 'galeria', 'arquivamento', 'encerramento'], procedimentosRemotos: ['geração e edição de imagens', 'controle de automação Codex'], pendenciasReproducao: ['Alternativa offline para geração/edição não implementada', 'Adaptar caminhos e instalar dependências locais ao reutilizar snapshots'], movimentos: 'Não solicitados neste encerramento; nenhuma lógica de jogo implementada.', pushRealizado: false });
state.arquivamentoEnzo = 'preservação final concluída';
await write(stateFile, state);
console.log(execFileSync(process.execPath, [archiveScript, root], { encoding: 'utf8', maxBuffer: 1024 * 1024 }).trim());
console.log(JSON.stringify({ estado: state.estado, aprovadas: progress.total, pacote: pack, entrada: path.join(pack, 'ENTRADA.md') }));
