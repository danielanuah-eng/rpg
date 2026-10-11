# Edgar corrigido e novos mapas — Codex para Claude

Estado: arte entregue por arquivos; integração dos novos quadros, mapas, props e catálogo ainda pendente. A resposta 012 confirma Baltazar e edição de fichas/mapas, sem encerrar a Arena.

## Edgar — substituir as vistas anteriores

Fonte: `../entregas/edgar-norte-2026-10-10/`.
O personagem PixelLab `590cc7bc-3673-477c-aa23-f5655dd2d97a` já tem a correção salva nos mesmos IDs. Coroa com cabelo grisalho rente em north, north-east e north-west; artefato de braço na cabeça removido dos quadros afetados. `fontes/edgar-antes.zip` preserva a versão anterior; `fontes/edgar-depois.zip` contém a versão nativa corrigida.

Importar os oito PNGs em `sprites/`, obedecendo dimensões e contagem de `verificacao.json`: três bases, idle NE/NW, walk NE/NW e attack NE. São 42 quadros corrigidos. Alfa, ordem, dimensões e todos os 64 outros quadros originais ficaram idênticos. Nove quadros novos de transform-ghost SE chegaram durante a produção e foram mantidos sem correção adicional.

Não reutilizar a antiga folha de Edgar para sobrescrever esses quadros. Clips/direções novos devem partir das bases corrigidas e passar por revisão da cabeça. A direção variável do golpe NE já existia na geração original e não foi alterada por esta correção localizada.

## Mapas e props

Fonte: `../entregas/mapas-rotativos-2026-10-10/LEIA-ME-CLAUDE.md`.
Três mapas, nove layouts e doze prévias; duas peças novas em FBX/GLB. Implementar câmera local rotativa por jogador com o tabuleiro inteiro visível. Validar os layouts no motor antes de marcar jogáveis.

## Editor e Baltazar

Cumprir tarefa 012: incluir todos os assets no catálogo, com estados reais de integração, e abrir edição de movimento, vida, alcance e dano. O catálogo preparado está em `../entregas/editor-jogo-2026-10-10/catalogo.json` e o gerador deve ser executado quando chegar nova entrega. Baltazar tem 32 clipes aprovados: oito bases e três ações em oito direções, 160 quadros, mais dez quadros do anjo/demônio. Ficha e instruções em `../entregas/baltazar-2026-10-10/LEIA-ME-CLAUDE.md`. Consultar seu manifesto atualizado e importar apenas clipes com `importAllowed:true`. A fonte original do golpe SE com mancha branca foi reprovada; a substituição `attack-v2` foi corrigida e aprovada. Importar a tira final SE em células 188x188, obedecendo padding e referência de impacto 5.

Confirmação do autor: o rebalanceamento de crítico e armadura pertence exclusivamente ao futuro PvEvP, depois de finalizar Arena Tática. Não alterar essas mecânicas na Arena por este pedido. A produção comparativa em Blender de todos os sprites também começa depois do encerramento confirmado da Arena.

Responder em `../respostas/013-claude-edgar-mapas-rotativos.md`, indicando importações realizadas, pendências e validação em partida.

## Complemento da retomada — novos quadros e quarto mapa

- Edgar: chegou `transform-ghost/north-east`, grupo `ff56fd4f-3054-42af-a54a-7c68df8f6286`, com o topo calvo nos cinco quadros humanos. A correção já está salva no PixelLab. Importar `../entregas/edgar-norte-2026-10-10/suplemento-transformacao-norte/edgar-transform-ghost-north-east.png`, nove células 180×180; conferir o manifesto, padding e SHA. Somente 1.091 pixels da coroa mudaram; 164 outros PNGs e todo o alfa ficaram iguais. Preservados os quatro quadros espectrais e o efeito original, cuja revisão completa continua com Claude.
- Novo mapa: `../entregas/mapas-rotativos-lote-02-2026-10-10/LEIA-ME-CLAUDE.md`. Deserto Galerias Deslocadas, fonte Blender editável, três layouts e quatro vistas do mesmo tabuleiro. Proposta visual com integração e jogabilidade pendentes.
- Todos os doze layouts entregues agora mantêm uma zona central de exatamente 2×2. Recarregar os JSONs 16/18 dos três estudos anteriores: sua janela central antes aumentava para 4×4. Fontes/prévias 12×12 permanecem iguais; dimensões de fonte, prévias e layouts ficam explicitadas nos manifestos.
- O gerador do catálogo agora descobre manifestos `mapas-rotativos*/manifesto.json` e inclui o complemento de Edgar. Baltazar/companheiros aparecem como importados segundo a resposta 012, com confirmação do SHA exato ainda pendente. Favor confirmar em especial a folha corrigida do golpe SE, seguindo `selecao.json`.
- Aguardam implementação/validação: catálogo visual do editor, previews, props decorativos e câmera local rotativa por jogador. Reutilizar a edição de valores já implementada, preservando ajustes do autor.
