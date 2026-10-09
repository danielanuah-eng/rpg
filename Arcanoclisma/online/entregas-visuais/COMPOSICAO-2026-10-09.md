# Composição da arena e leitura da pilha — 09/10/2026

Pedido direto do autor: fundo/campo poluído, esquerda mal aproveitada, muito vazio, cartas pequenas e carta da pilha pequena.

Entrega: `public/css/mesa-composicao-v21.css`, carregada pelo HTML. Fundo neutro azul escuro, piso decorativo com opacidade de 12%, pilhas à esquerda, Arcano ao lado dos Conjuradores e Grau à direita. Campo com cartas de 132 px antes da câmera (antes 114 px), menor largura mínima e zonas secundárias compactas. A carta do topo da pilha tem 204 px no PC e texto abaixo; 144 px no celular em pé e 168 px deitado.

A câmera continua encaixando o tabuleiro inteiro. `public/js/mesa.js` observa também o tamanho do conteúdo para reencaixar depois de carregar estilos/rótulos; o observador é desligado ao fechar a mesa. Nenhuma regra foi alterada.

Conferência com Edge headless e o renderizador real da mesa, usando estado de teste local:

| Tela / cartas por lado | Campo antes | Campo depois | Ganho | Carta na pilha |
| --- | ---: | ---: | ---: | ---: |
| PC 1366×768 / 3 | 90,3 px | 105,5 px | 17% | 204 px |
| PC 1920×1080 / 3 | 136,9 px | 160,0 px | 17% | 204 px |
| Celular 390×844 / 3 | 49,9 px | 77,2 px | 55% | 144 px |
| Celular 844×390 / 3 | 51,6 px | 60,6 px | 17% | 168 px |
| PC 1366×768 / 8 | 90,3 px | 105,5 px | 17% | 204 px |

Nos cinco casos: tabuleiro dentro do palco em ambos os eixos, pilhas à esquerda, Grau à direita, página sem rolagem horizontal e zero erros JavaScript. 28 testes online passaram e o build completo terminou num checkout isolado. Capturas em `composicao-2026-10-09/pc.png` e `celular.png`. O painel inferior do celular conserva rolagem interna para chegar à pilha.

Estado: corrigido e conferido localmente; publicação no serviço hospedado ainda não confirmada. Claude pode integrar/publicar esta entrega conforme a coordenação do projeto. A expansão continua bloqueada para jogo.
