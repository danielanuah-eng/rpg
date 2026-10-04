# Artes novas das cartas atuais

Fila inicial de 151 ilustrações com cena completa, ampliada para 210 com 59 artes novas solicitadas por Claude conforme a variedade pedida pelo autor. `progresso.json` informa quantas estão prontas e quais faltam. As artes novas aguardam avaliação final; autorização de push não equivale à aprovação visual de todas elas. `incorporar-pedido-v2.mjs` registra o lote adicional preservando imagens e estados anteriores.

- PNGs na raiz: originais da geração, sem redimensionar ou comprimir.
- `prompts/` e `registros/`: instruções completas, imagem inspecionada, descrição/regra consultada, dimensões, alfa medido e decisões visuais.
- `variantes/`: versões anteriores preservadas nas correções.
- `galeria.html`: galeria local. `galeria-dados.js` fornece os dados sem fetch obrigatório.
- `fila.json`: estado por carta, usado na integração. Claude libera no jogo as artes aprovadas pelo autor conforme o fluxo atual.
- `registrar-entrega.mjs`: registra entregas e atualiza galeria/progresso. Sharp mede alfa sem modificar pixels. `preparar-revisao.mjs` é preparação inicial; não executar para reiniciar a fila atual.

O lock local evita produção simultânea e não é publicado. A expansão **A Mestra dos Dragões** tem autorização de integração separada; estes arquivos não liberam a expansão.
