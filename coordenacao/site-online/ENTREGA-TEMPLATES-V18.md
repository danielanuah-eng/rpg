# Templates v18 — custo, Grau e leitura

Entrega concluída em 2026-10-04, a pedido do autor. Disponível no aplicativo online após carregar os arquivos desta entrega e recarregar a página.

## Acabamento

- Custo de Arcano: aro metálico com facetas pequenas, reflexo e sombra; mantém a cor dos Valores e o número legível.
- Grau: losango azul com borda biselada e detalhes metálicos.
- Arcano gerado: esfera com reflexo de vidro, sombra inferior e aro duplo, dando profundidade.
- Texto: uma superfície de leitura que cresce conforme o efeito e os lembretes. Tipo e subtipo têm linhas próprias; estatísticas acompanham o topo do painel. Nomes muito longos usam duas linhas.
- Carta aberta: maior no computador; cabe no celular em pé. No celular deitado, o texto completo também aparece ao lado da carta, para compensar a altura disponível.

## Integração para Claude

`Arcanoclisma/online/public/js/mesa-legibilidade-v15.js`, já carregado pelo aplicativo, passa a carregar automaticamente:

- `/css/cartas-leitura-v18.css`
- `/js/cartas-leitura-v18.js`

Os SVGs ficam em `online/public/kit/vetores/`: `aro-custo-v18.svg`, `aro-geracao-v18.svg`, `selo-grau-v18.svg`.

O adaptador mede o conteúdo renderizado e atualiza o layout quando texto ou tamanho mudam. Inclui as cartas abertas em diálogo. Não exige duplicar o texto da carta em outro template. Não altera dados ou comandos do motor.

Preservar os novos arquivos públicos durante o build/deploy. Não é necessário alterar o gerador de cartas para aplicar o acabamento. O autor já autorizou o push desta entrega.

## Conferência realizada

- Aplicativo local real via Wrangler, `http://127.0.0.1:8787`.
- Coleção atual: **201 cartas**, todas com o template aplicado; nenhum nome truncado e nenhuma regra passando do painel. Medição repetida após recarregar os dados disponíveis.
- Arquivista Sem Rosto, incluindo lembretes de Etéreo e Protegido: leitura completa em **1366×768**, **390×844** e **844×390**. Diálogo cabe nas três telas sem precisar rolar para ler as regras.
- Sintaxe dos dois scripts conferida com Node.
- Suíte existente `Arcanoclisma/efeitos/verificar-magias-v17.test.mjs`: **5 testes passaram**, cobrindo contrato/arquivos, ciclo de vida, feixe, redução de movimento e adaptador dos 132 efeitos.

Evidências: `evidencias-v18/carta-pc-1366x768.jpg`, `carta-celular-390x844.jpg`, `carta-celular-844x390.jpg`, `medicao-colecao-pc.json`.

## Achado de integração das animações para Claude

No tutorial **5 — Respostas na pilha**, reproduzido pela interface: bot joga Brasa de Forja no Lobo-de-Cinza; jogador responde com Olho-que-Fecha, mira Brasa de Forja e paga com três cartas do Grau.

O motor registra corretamente `Olho-que-Fecha resolve` e `Brasa de Forja foi anulada`. As três camadas específicas da anulação são criadas e removidas ao concluir. A Brasa anulada não produz o efeito de dano. Sem erros de carregamento nessa execução.

**Ajuste necessário no posicionamento da pilha:** a geometria pública capturada para a carta alvo fica abaixo da janela quando o painel da lição e as duas cartas estão presentes. Em 1366×768, o trajeto da anulação foi desenhado em `x=1110.49, y=833.46, largura=37.01, altura=37.01`; o impacto também ficou fora da tela. A camada de efeitos limpou todos os elementos depois da execução.

`mesa-ganchos.js` captura os retângulos da pilha lateral antes de redesenhar e envia `alvoRect` ao animador. Claude deve manter o alvo da pilha visível nesse fluxo e entregar sua geometria visível confirmada, inclusive quando a carta é removida por anulação. O Codex mantém os assets e o animador. A evidência `evidencias-v18/anulacao-pilha.jpg` mostra o estado da lição após a resolução; a medição acima foi feita enquanto as camadas existiam.

Os efeitos de ambos os campos e de ressurgimento possuem cobertura de contrato/ciclo de vida na suíte; ainda não foram conferidos visualmente em uma partida nesta revisão. Não considerar esta entrega uma aprovação visual individual dos 132 efeitos.

## Revisão do autor — remover faixa azul abaixo do nome

Retirada a faixa ampla de cabeçalho em `online/public/css/cartas-cabecalho-v16.css`. A cena agora ocupa 100% da frente, inclusive abaixo do nome, eliminando também o espaço que expunha o fundo azulado da carta. O acabamento compacto do nome e os indicadores continuam nas posições atuais.

Conferido na carta aberta Arquivista Sem Rosto no aplicativo local: a arte começa no topo da frente e preenche sua altura. Prévia em `evidencias-v18/carta-sem-faixa-azul.jpg`. Claude deve publicar o CSS atualizado junto desta revisão.

## Revisão do autor — painel bege e nome centralizado

O painel de regras em `online/public/css/cartas-leitura-v18.css` usa transparência suave (78%–82% de opacidade no fundo), mantendo os caracteres opacos. A camada de papel das oito molduras utilizadas foi reduzida para 10%–14%, evitando que o fundo antigo esconda a transparência do painel adaptável. As origens em `arena/templates/` e as cópias em `online/public/kit/templates/` estão sincronizadas para preservar a alteração no próximo build.

O nome foi centralizado na horizontal e na vertical sobre a faixa de título existente, usando o centro geométrico do desenho SVG. A redução automática de fonte e os nomes em duas linhas continuam aplicáveis.

Prévia conferida na carta aberta Arquivista Sem Rosto: regras completas, caracteres opacos e cenário visível suavemente através do bege. Evidência em `evidencias-v18/carta-painel-transparente-nome-central.jpg`. Incluir CSS e molduras na publicação.
