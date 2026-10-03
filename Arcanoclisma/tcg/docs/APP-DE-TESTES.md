# App de testes

App simples para jogar contra o jogador automático com os 9 Conjuradores, testar as mecânicas e mandar tudo ao Claude.

## Como abrir

Dê dois cliques em `Arcanoclisma/tcg/app/INICIAR.bat`. Abre uma janela preta (o servidor; deixe aberta enquanto testa) e o navegador em `http://localhost:8765`.

Sem o .bat: no terminal, na pasta `Arcanoclisma/tcg`, rode `node app/servidor.ts` e abra `http://localhost:8765`.

## O que tem

- **Tela inicial**: botão verde **ENVIAR**, nova partida (seu Conjurador contra um escolhido ou aleatório), lista de partidas (continuar ou ver), comentários gerais e a lista de Conjuradores e baralhos.
- **Partida**, em abas:
  - **Mesa**: os dois lados (Conjurador, vida, Arcano, Grau, campo, cemitério, banimento, mão), a pilha, os acontecimentos e um campo para comentar na hora.
  - **Comentários**: tudo o que você comentou nesta partida, com turno, etapa e vidas.
  - **Registro**: cada ação (sua, do oponente e os passes automáticos) com o resultado, e as tentativas recusadas pelas regras.
  - **Cartas**: os dois baralhos com os textos.
  - **Opções**: quando o app passa a prioridade sozinho.
- **Desfazer minha última ação**: volta até antes da sua última ação (o que veio depois também é desfeito; o desfeito fica guardado no registro).
- Para jogar uma carta, ativar ou atacar, clique no botão da carta: abre uma janela para escolher o alvo, descartar do Grau, suspender Grau e enviar cartas da mão ao Arcano.

## Editor de cartas e mecânicas

Na tela inicial, botão **Editor de cartas e mecânicas** (ou `http://localhost:8765/editor.html`). Abas:

- **Cartas**: lista com busca e filtros (tipo, Valor, baralho, só editadas). Ao escolher uma carta, edite custo, Grau, Ataque/Vida, Valores, palavras-chave, Protegido, efeitos (ao resolver, ao entrar, ao causar dano ao Conjurador, habilidade ativada) e o texto. Mostra em que baralhos ela está, a conta de pontos (orçamento × pontos usados, com a curva de Grau) e as diferenças do original. Dá para restaurar o original, duplicar como carta nova ou criar uma do zero.
- **Conjuradores**: nome, Valores, Vida, mão inicial, compra, dano básico, custo de ataque, habilidade (ativada, Repentina ou não, gatilho e passivas) e o texto, com sugestão gerada pela mecânica. Também aplica Vida, mão etc. a todos de uma vez.
- **Regras e curva de Grau**: limite de mão, relógio de impasse, Grau no turno 1, segundo jogador, e o k da curva de poder por Grau (recalcula os Aliados gerados).
- **Baralhos**: as cópias de cada carta, adicionar cartas que combinam com os Valores (o total precisa dar 60) e o estilo do jogador automático.
- **Testar (simulação)**: roda os 9 Conjuradores todos contra todos com o que está salvo e compara com a simulação anterior.
- **Régua de pontos**: os preços da conta de pontos. Só muda a medição; não muda carta nenhuma.
- **Histórico**: cada salvamento, com a sua nota e a lista do que mudou.

**Salvar** confere o jogo (cartas válidas, baralhos com 60 cartas e Valores certos, uma partida de teste por baralho). Se algo quebrar, não salva e mostra o motivo. O que é salvo vai para `data/edicoes.json` (o arquivo anterior fica em `data/edicoes-historico/`) e passa a valer nas novas partidas, nas simulações e para a equipe de balanceamento (variantes `PADRAO` e `CONJ`). Partidas já começadas continuam com as cartas com que foram jogadas.

## Onde ficam os dados

- Tudo fica salvo no navegador (localStorage) e continua lá depois do envio.
- **ENVIAR** grava um arquivo em `Arcanoclisma/tcg/envios/envio-AAAA-MM-DD_HH-MM-SS.json` com todas as partidas, ações, resultados e comentários. O Claude lê direto dessa pasta.
- Se o servidor estiver desligado, o botão baixa o arquivo .json para você mandar ao Claude.

## Para o Claude

- `app/construir.ts` converte `src/motor.ts` e `sim/bot.ts` para JS (`app/dist/`, gerado, fora do git) e gera `app/dist/dados.json` da variante `CONJ`. O servidor reconstrói ao iniciar, então mudanças no motor, nas cartas ou nos baralhos entram sozinhas.
- Cada partida guarda a semente e a lista de comandos; o estado é reconstruído executando os comandos de novo (`reconstruir` em `app/app.js`).
- Cada versão dos dados (hash do conteúdo) também fica em `app/versoes/<versao>.json`; a partida guarda `dadosVersao` e reabre com ela. Partidas sem versão são de antes do editor (`5539baf083`).
- Edições do autor: `sim/edicoes.ts` (aplicação), `app/editor-api.ts` (estado, validação, salvar, simular), `app/editor.js`. Entram nas variantes com `edicoesAutor` (PADRAO, CONJ); nos testes automáticos (`node --test`) não entram. O envio (ENVIAR) leva `edicoes` com o histórico e as notas.
