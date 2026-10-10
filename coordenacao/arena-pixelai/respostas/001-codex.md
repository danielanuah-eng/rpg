# 001 — Organização de sprites e imagens

**Autor:** codex
**Resultado:** entrega local concluída; responsabilidade do Claude por importação, código e uploads preservada.

Catálogo da biblioteca PixelLab, comparação com os 11 personagens mapeados e inventário de 141 PNGs entregues nesta ponte. Todas as imagens locais foram decodificadas; alfa e SHA256 foram medidos. Foram conferidos 48 strips existentes, todos com grade e número de quadros válidos. A fila registra 26 direções de clipes já prontas na biblioteca e ainda ausentes no snapshot local, além dos trabalhos remotos pendentes e da falta de metadados de importação.

Para reduzir o trabalho de preparação de Berserk, foram baixadas as rotações das duas variantes existentes do Tchu tcha: 16 PNGs de 128×128, 8 direções em cada pasta identificada pelo UUID. Todos têm conteúdo visível e pixels transparentes. Não foram copiados para Assets nem escolhidos como versão definitiva.

## Arquivos

- `catalogo-pixellab.json`
- `inventario-local.json`
- `fila-para-claude.json`
- `fontes-berserk.json`
- `entregas/tchu-tcha-berserk/` e seu `inventario.json`
- `notas/2026-10-10_codex_claude.md`
- `ArenaTaticaUnity/PARA-CLAUDE-PIXELAI.md` (aviso de descoberta no projeto)

## Validações e pendências

141/141 PNGs locais decodificados, 48/48 strips com grade e quantidade válidas, 16/16 PNGs de entrega decodificados e com alfa, ambas as variantes com as 8 direções esperadas. IDs do catálogo conferidos com `Tools/pixellab-ids.json`. O exame de código foi delegado ao DeepSeek por OpenRouter, somente leitura; suas dúvidas de dimensões, arquivos truncados e grade foram conferidas por Codex. A hipótese de fatiamento indevido da loja foi descartada para a imagem 384×216 e o importador atual.

Tarefa 002 resolvida: autor rejeitou o hit/north-west da Talyssa por mostrar morte. Codex confirmou, corrigiu e verificou o asset salvo no PixelLab. O strip corrigido está separado para importação por Claude; detalhes na resposta 002. A fila anterior foi atualizada com essa prioridade.

? Claude precisa escolher a variante Berserk, importar os assets, confirmar nomes/uso em Unity e publicar sua versão. Recebimento ainda não confirmado.

? As pastas e a biblioteca estão mudando em paralelo; esta entrega é uma consulta datada. Atualizar somente os personagens selecionados antes de importar.

Tags remotas não foram substituídas: a API permite escrever o conjunto completo, mas não expõe o conjunto atual para preservação. Não houve geração para a tarefa 001, edição de imagem, mudança de código ou upload.
