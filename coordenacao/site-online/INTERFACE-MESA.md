# Entrega da revisão visual da mesa — 03/10/2026

Direção consolidada do autor, aplicada à estrutura atual de `Arcanoclisma/online/public/js/mesa.js`:

- Campo ampliado nos dois eixos; largura interna mínima 1120 px (1080 px em viewport intermediária), altura mínima 850 px e fileiras que crescem conforme as permanentes. Manter cartas do campo em 114 px, Conjuradores em 118 px e Grau em 64 px. O campo pode rolar quando o viewport não comporta esses tamanhos; não reduzir todas as cartas.
- Ambos os Conjuradores no eixo central, junto à divisão entre jogadores, acima dos respectivos campos de aliados/permanentes. O Conjurador rival fica voltado para o jogador dele. Contadores sobrepostos à carta ficam discretos e os números legíveis.
- Esfera lateral de Arcano com o número autoritativo da reserva. Fases, prioridade, decisões, combate e pilha permanecem no painel à direita. Em telas estreitas, o painel vai para fora do campo, abaixo da mesa.
- Mão própria em faixa externa, com espaço reservado. Mão rival também externa, com um verso visível por carta, separação para contagem e total textual.
- Grau em cartas de costas com tamanho próprio; suspensão usa `inst.suspensa` do estado. A animação visual interpola entre a posição/rotação anterior e a nova.
- Compras usam um verso genérico que viaja do baralho até a mão. A compra é detectada por novos elementos da mão e redução da quantidade no baralho; a distribuição/reposição de cartas existentes é animada. Nenhum comando ou regra é produzido por esse módulo.

## Arquivos

- `Arcanoclisma/online/public/css/mesa-codex-v10.css`: refinamento visual separado, carregado após `mesa.css` no `index.html`.
- `Arcanoclisma/online/public/js/mesa-movimentos-codex.js`: captura de geometria e animações com Web Animations, com limpeza e movimento reduzido.
- `mesa.js`: import, inicialização, captura antes de renderizar, animação após a renderização e limpeza ao fechar. O restante da programação continua com Claude.

Não há leitura de faces ocultas pela animação: cartas viajantes/rivais usam o verso comum. Não foi executada conferência no navegador nesta revisão. Não reiniciar servidor, publicar site ou fazer push por conta desta entrega. Integração/refinamentos posteriores são confirmados por Claude em `resposta.claude.json`.

## Artes e briefings

Toda carta final precisa de cenário estético; personagens não são obrigatórios em todos os tipos e eventos grandiosos não são obrigatórios. Direção em `Arcanoclisma/cartas/DIRETRIZ-CENARIOS.md`: variar ambiente, escala, clima e dia/noite; cenas do mesmo evento explicam uma relação própria com ele. Recortes do RPG são referências, não artes finais de carta.
