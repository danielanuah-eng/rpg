# Arcanoclisma online

Primeira versão jogável do TCG na internet, no Cloudflare. Usa o motor real (`../tcg/src/motor.ts`), o jogador automático das simulações (`../tcg/sim/bot.ts`), as cartas e edições do autor (variante CONJ + `tcg/data/edicoes.json`), a coleção nova (`tcg/src/cartas-colecao.ts`), as artes aprovadas e o kit visual do Codex (`../arena`).

## O que tem

- **Contas** com e-mail e senha; botão "Entrar com Google" quando `GOOGLE_CLIENT_ID` estiver configurado.
- **Jogar contra o bot**: qualquer baralho (prontos, seus ou da comunidade) contra um Conjurador ou o baralho público de outro jogador. Roda no navegador; a partida fica guardada e pode ser retomada.
- **Jogar contra pessoas**: partida rápida (fila), salas abertas e sala privada com código. Cada sala é um Durable Object que roda o motor e manda a cada jogador só o que ele pode ver (`src/visao.ts`). Prazo de 75 s por decisão (depois o servidor faz a ação mais segura); desconectado por 3 min perde.
- **Tutorial**: 4 lições sobre o motor real (Grau e Arcano; ataque e bloqueio; pilha, Anular, pagar para anular e olhar o topo; itens, Estruturas, habilidade do Conjurador e fim de turno) e uma partida completa com dicas.
- **Baralhos**: 9 prontos, construtor com a regra das cores (só cartas sem Valor ou dos 2 Valores do Conjurador), 60 cartas, até 3 cópias de cada carta, Mina Funda inclusive (os baralhos prontos são ajustados no build; os das simulações não mudam), side deck de itens (Canal, Armadura, Extra); baralhos públicos.
- **Comentários**: opiniões, notas (1 a 5), dúvidas e problemas, de dentro da partida (com o momento anexado) ou fora dela; respostas; o autor marca Aprovado para mudar / Em revisão / Não vai mudar / Feito.
- **Ideias**: expansões, personagens, cartas, itens, Conjuradores e mecânicas, com votos e decisão do autor.
- **Editor do autor** (`#/editor`, botão ✎ Editor no topo, só para a conta do autor): muda nome, texto, arte (qualquer uma das 435 aprovadas), números, Valores, velocidade, palavras-chave e a mecânica em JSON de cartas e Conjuradores. O rascunho já vale na página (teste contra o bot); "Salvar no site" grava uma versão nova no D1 (tabela `edicoes`) que passa a valer para todos; partidas online guardam a versão do início. Baixar/carregar as edições em JSON (mesmo formato do editor do app de testes) e voltar a versões antigas.
- **Mesa** (direção do autor): Conjuradores no centro e Aliados atrás; equipamento guardado (clique no Conjurador); tabuleiro limpo; vida e ataque sobre as imagens; Arcano numa esfera na lateral; Grau como cartas viradas que se deitam para gerar Arcano; mão fora da mesa; fases, decisões e a **pilha** (carta + efeito + alvo) na coluna da direita.

## Rodar no computador

```bash
npm install
npm run construir
npx wrangler d1 execute arcanoclisma --local --file=schema.sql
npx wrangler dev
```

Abre em http://localhost:8787. No Claude Code a configuração `arcanoclisma-online` do `.claude/launch.json` faz o mesmo.
`.dev.vars` (fora do Git) guarda a `AUTOR_CHAVE` local; `ADMIN_EMAILS` ali só vale no teste local.

Testes: `npm test` (coleção, identidade, sigilo da visão) e, com o servidor local no ar, `node test/online-local.mjs` (duas contas de teste jogam uma partida online inteira com bots).

## Publicar no Cloudflare (precisa da conta do autor)

1. `npx wrangler login` (abre o navegador para autorizar).
2. `npx wrangler d1 create arcanoclisma` e copiar o `database_id` para `wrangler.jsonc`.
3. `npm run db:remoto` (cria as tabelas; rodar de novo depois de atualizar o site, para criar as tabelas novas, como `edicoes`).
4. `npx wrangler secret put AUTOR_CHAVE` (uma frase longa só sua; libera o painel do autor em "Entrar → Sou o autor").
5. Opcional, Google: criar um "ID do cliente OAuth" (aplicativo da Web) no Google Cloud, com a origem `https://arcanoclisma.<sua-conta>.workers.dev`, e pôr em `GOOGLE_CLIENT_ID` no `wrangler.jsonc`.
6. `npm run publicar`.

O painel do autor só abre para conta verificada: quem entrou pelo Google com o e-mail de `ADMIN_EMAILS` ou quem usou a chave do autor. Como o site não manda e-mail de confirmação, uma conta de senha não prova a posse do e-mail; por isso essa trava.

## Arte

- Provisório: cada carta usa uma arte aprovada do catálogo (`dados/identidade.json`) recortada sobre o cenário do seu Valor.
- Pedido ao GPT/Codex de todas as cartas com cenário de fundo: `../cartas/pedidos/lote-site-v1.claude.json` (gerado por `pedidos-visuais/gerar-lote-cartas.ts`).
- Pedido da arte do site (logo, telas, cenários dos Valores, peças de interface, animações): `pedidos-visuais/site.claude.json`.

## Arquivos

- `construir.ts`: gera `public/jogo` (motor em JS e `dados.json`), as artes em WebP e copia o kit visual.
- `src/worker.ts` (API), `src/conta.ts` (contas), `src/partida.ts` (sala online), `src/saguao.ts` (lista de salas e fila), `src/visao.ts` (sigilo).
- `public/js`: `app.js` (rotas e telas), `mesa.js` (mesa de jogo), `partida-local.js` (contra o bot), `partida-online.js` (WebSocket), `tutorial.js`, `construtor.js`, `comunidade.js`, `carta.js` (renderizador do Codex).
