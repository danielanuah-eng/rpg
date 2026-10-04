# Vista próxima e interface das zonas — 04/10/2026

Pedido do autor: aproximar o tabuleiro no PC e no celular para reconhecer as cartas sem clicar/ampliar; melhorar baralho, Grau e demais zonas.

## Entrega visual v15

- `Arcanoclisma/online/public/css/mesa-proxima-v15.css`, carregado por último no `public/index.html`.
- `Arcanoclisma/online/public/js/mesa-legibilidade-v15.js`, carregado com `defer`.
- Cartas em campo com largura real de 148 px no PC, 120 px no celular em pé e 132 px na tela curta/deitada; Conjuradores centrais de 152/116/132 px. Celular até 360 px usa 112/106 px.
- A redução automática do tabuleiro inteiro por `ajustarMesa()` é substituída apenas por CSS. `zoom`, posição e dimensões inline de `.mesa-mundo` deixam de reduzir as cartas. O palco permite rolagem quando o campo não cabe. Não voltar a encaixar toda a partida mediante redução das cartas.
- O piso é recortado para sua área azul, conservando ambos os jogadores e os Conjuradores no centro. Mãos e decisões continuam fora da superfície de jogo.
- Campo local/adversário tem rótulo discreto no canto. Cartas públicas em campo têm legenda legível abaixo da arte, extraída exclusivamente do nome já renderizado; nenhuma mão ou informação oculta é consultada.
- Grau tem bandeja com título, contagem fornecida pelo motor e versos separados. A faixa rola lateralmente em vez de ocultar todas as cartas em um leque. Suspensão continua no wrapper inteiro.
- Baralho, Cemitério, Banidas e Arcano têm etiquetas, contadores e bordas distintas. O verso do baralho e a imagem pública de topo das demais pilhas continuam sendo os do código existente.
- No celular em pé: recursos à esquerda/direita do Conjurador, painel de decisões menor abaixo e campo com cartas grandes; quando cheio, cresce verticalmente. Rolagem inicial centralizada nos Conjuradores; atualizações preservam a posição escolhida pelo jogador.

Não há alteração em `mesa.js`, `mesa-ajuste.css`, regras, dados, tutorial, velocidades ou resolução de ações. Essas áreas tinham alterações simultâneas do Claude.

## Integração

As duas referências já estão inseridas no HTML do site. Manter a ordem da camada v15 depois das outras folhas. Não copiar o CSS como uma nova câmera com `scale()`/`zoom` menor que 1.

## Escolha posterior confirmada pelo Claude

Em `resposta.claude.json`, atualizado no commit `e2f20fa`, o autor escolheu manter o tabuleiro inteiro. **A câmera fixa v15 permanece sem carregamento.** Os tamanhos em pixels e a rolagem descritos acima são a alternativa arquivada, não a configuração ativa.

O refinamento ativo é `mesa-zonas-v16.css`: conserva a câmera do Claude; títulos e contadores compensam a escala, pilhas têm cores discretas diferentes e no celular em pé o painel inferior ocupa menos altura. `mesa-legibilidade-v15.js` carrega essa folha e `cartas-cabecalho-v16.css` explicitamente. Nome e esfera de Valor têm uma faixa acima da imagem; a área de arte começa em 16% da carta, mantendo o rosto livre. O enquadramento não altera os arquivos originais nem as metades de Valor.

Implementação por leitura de DOM/CSS existentes; testes e validação visual em navegador não foram executados nesta entrega. Confirmar na sessão real PC/retrato/paisagem ao integrar, inclusive campo cheio, Grau suspenso, arraste, zoom de inspeção e D20 de abertura.
