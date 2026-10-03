// Prepara o app de testes: converte o motor, o jogador automático e a régua de pontos para JavaScript (sem dependências,
// usando o apagador de tipos do Node 24) e gera app/dist/dados.json com as cartas, os 9 Conjuradores e os baralhos,
// já com as edições do autor (data/edicoes.json). Cada versão dos dados também fica em app/versoes/<versao>.json,
// para as partidas antigas continuarem com as cartas com que foram jogadas.
// Uso: node app/construir.ts   (o servidor chama isto ao iniciar e depois de cada salvamento do editor)
import { stripTypeScriptTypes } from 'node:module';
import { createHash } from 'node:crypto';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { REGRAS_PADRAO } from '../src/motor.ts';
import { rodada3 } from '../sim/lista.ts';
import { contextoDe } from '../sim/nucleo.ts';
import { estiloDe, regrasDe } from '../sim/variantes.ts';
import { lerEdicoes } from '../sim/edicoes.ts';

const raiz = new URL('../', import.meta.url);
const dist = new URL('./dist/', import.meta.url);
const versoes = new URL('./versoes/', import.meta.url);

export const varianteDoApp = () => rodada3.find((x) => x.id === 'CONJ')!;

export function construir(): { versao: string } {
  mkdirSync(dist, { recursive: true });
  mkdirSync(versoes, { recursive: true });
  // 1. motor, jogador automático e régua em JS (os imports apontam para os .js gerados ao lado)
  for (const [de, para] of [['src/motor.ts', 'motor.js'], ['src/tipos.ts', 'tipos.js'], ['src/balanco.ts', 'balanco.js'], ['sim/bot.ts', 'bot.js']]) {
    let js = stripTypeScriptTypes(readFileSync(new URL(de, raiz), 'utf8'));
    js = js.replace(/from\s+'(?:\.\.\/src\/|\.\.\/sim\/|\.\/)([\w-]+)\.ts'/g, "from './$1.js'");
    writeFileSync(new URL(para, dist), `// Gerado por app/construir.ts a partir de ${de}. Não edite: edite o .ts e reconstrua.\n${js}`);
  }
  // 2. dados do jogo: variante CONJ (o padrão aprovado + os 9 Conjuradores + as edições do autor)
  const v = varianteDoApp();
  const ctx = contextoDe(v);
  const conteudo = {
    variante: v.id,
    regras: { ...REGRAS_PADRAO, ...regrasDe(v) },
    catalogo: ctx.catalogo,
    baralhos: ctx.arqs.map((a) => ({ conjurador: a.conjurador, nome: a.nome, estilo: estiloDe(a, v), lista: a.lista, itens: a.itens ?? [] })),
  };
  const versao = createHash('sha1').update(JSON.stringify(conteudo)).digest('hex').slice(0, 10);
  const ed = lerEdicoes();
  const dados = { geradoEm: new Date().toISOString(), versao, ...conteudo, edicoes: ed };
  writeFileSync(new URL('dados.json', dist), JSON.stringify(dados));
  const arqVersao = new URL(`${versao}.json`, versoes);
  if (!existsSync(arqVersao)) writeFileSync(arqVersao, JSON.stringify(dados));
  console.log(`app/dist: motor.js, bot.js, balanco.js, dados.json (versão ${versao}: ${Object.keys(ctx.catalogo.cartas).length} cartas, ${dados.baralhos.length} baralhos).`);
  return { versao };
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) construir();
