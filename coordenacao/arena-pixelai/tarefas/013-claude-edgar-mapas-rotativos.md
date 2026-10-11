# Edgar corrigido e novos mapas — Codex para Claude

Estado: resposta 013 recebida em 11/10. Os três primeiros mapas, relicário, barricada e câmera local em quatro vistas estão confirmados pelo Claude. Edgar corrigido, os mapas posteriores e o catálogo visual seguem pendentes. Resposta 012 confirma Baltazar e edição de valores/mapas. Arena ainda não encerrada.

## Edgar — substituir as vistas anteriores

Fonte: `../entregas/edgar-norte-2026-10-10/`.
O personagem PixelLab `590cc7bc-3673-477c-aa23-f5655dd2d97a` já tem a correção salva nos mesmos IDs. Coroa com cabelo grisalho rente em north, north-east e north-west; artefato de braço na cabeça removido dos quadros afetados. `fontes/edgar-antes.zip` preserva a versão anterior; `fontes/edgar-depois.zip` contém a versão nativa corrigida.

Importar os oito PNGs em `sprites/`, obedecendo dimensões e contagem de `verificacao.json`: três bases, idle NE/NW, walk NE/NW e attack NE. São 42 quadros corrigidos. Alfa, ordem, dimensões e todos os 64 outros quadros originais ficaram idênticos. Nove quadros novos de transform-ghost SE chegaram durante a produção e foram mantidos sem correção adicional.

Não reutilizar a antiga folha de Edgar para sobrescrever esses quadros. Clips/direções novos devem partir das bases corrigidas e passar por revisão da cabeça. A direção variável do golpe NE já existia na geração original e não foi alterada por esta correção localizada.

## Mapas e props

Fonte: `../entregas/mapas-rotativos-2026-10-10/LEIA-ME-CLAUDE.md`.
Três mapas, nove layouts e doze prévias; duas peças em FBX/GLB. Claude confirmou os nove layouts jogáveis, câmera local Q/E/Girar e props no editor. Falta tratar a oclusão das árvores. Deserto e Vulcão são entregas posteriores; não estender a confirmação dos três primeiros mapas automaticamente a elas.

## Editor e Baltazar

Cumprir tarefa 012: incluir todos os assets no catálogo, com estados reais de integração, e abrir edição de movimento, vida, alcance e dano. O catálogo preparado está em `../entregas/editor-jogo-2026-10-10/catalogo.json` e o gerador deve ser executado quando chegar nova entrega. Baltazar tem 32 clipes aprovados: oito bases e três ações em oito direções, 160 quadros, mais dez quadros do anjo/demônio. Ficha e instruções em `../entregas/baltazar-2026-10-10/LEIA-ME-CLAUDE.md`. Consultar seu manifesto atualizado e importar apenas clipes com `importAllowed:true`. A fonte original do golpe SE com mancha branca foi reprovada; a substituição `attack-v2` foi corrigida e aprovada. Importar a tira final SE em células 188x188, obedecendo padding e referência de impacto 5.

Confirmação do autor: o rebalanceamento de crítico e armadura pertence exclusivamente ao futuro PvEvP, depois de finalizar Arena Tática. Não alterar essas mecânicas na Arena por este pedido. A produção comparativa em Blender de todos os sprites também começa depois do encerramento confirmado da Arena.

Responder em `../respostas/013-claude-edgar-mapas-rotativos.md`, indicando importações realizadas, pendências e validação em partida.

## Complemento da retomada — novos quadros e quarto mapa

- Edgar: chegou `transform-ghost/north-east`, grupo `ff56fd4f-3054-42af-a54a-7c68df8f6286`, com o topo calvo nos cinco quadros humanos. A correção já está salva no PixelLab. Importar `../entregas/edgar-norte-2026-10-10/suplemento-transformacao-norte/edgar-transform-ghost-north-east.png`, nove células 180×180; conferir o manifesto, padding e SHA. Somente 1.091 pixels da coroa mudaram; 164 outros PNGs e todo o alfa ficaram iguais. Preservados os quatro quadros espectrais e o efeito original, cuja revisão completa continua com Claude.
- Novo mapa: `../entregas/mapas-rotativos-lote-02-2026-10-10/LEIA-ME-CLAUDE.md`. Deserto Galerias Deslocadas, fonte Blender editável, três layouts e quatro vistas do mesmo tabuleiro. Proposta visual com integração e jogabilidade pendentes.
- Todos os doze layouts entregues agora mantêm uma zona central de exatamente 2×2. Recarregar os JSONs 16/18 dos três estudos anteriores: sua janela central antes aumentava para 4×4. Fontes/prévias 12×12 permanecem iguais; dimensões de fonte, prévias e layouts ficam explicitadas nos manifestos.
- O gerador do catálogo agora descobre manifestos `mapas-rotativos*/manifesto.json` e inclui o complemento de Edgar. Baltazar/companheiros aparecem como importados segundo a resposta 012, com confirmação do SHA exato ainda pendente. Favor confirmar em especial a folha corrigida do golpe SE, seguindo `selecao.json`.
- Atualização de 11/10: props decorativos e câmera local já confirmados. Aguardam conclusão: catálogo visual/previews, importação de Edgar corrigido, mapas posteriores e oclusão das árvores. Preservar a edição de valores e os ajustes do autor.

## Lote 03 — Vulcão, Passarela em S

Nova entrega: `../entregas/mapas-rotativos-lote-03-2026-10-11/LEIA-ME-CLAUDE.md`. ID `vulcao-passarela-em-s`. Fonte Blender editável 12×12, quatro prévias locais do mesmo tabuleiro e três layouts alternativos 12/16/18, com quatro casas centrais preservadas. A passarela sobe a níveis 1/2; lava e motivos quentes são somente visuais do kit existente. Nenhuma regra nova nem código de jogo foi alterado.

Adicionar ao catálogo e ao editor, validar no MapGen antes de liberar em partida e confirmar o SHA dos arquivos importados. A confirmação anterior cobre somente Bosque/Pedreira/Neve e os dois props. Importações de Deserto/Vulcão e Edgar continuam pendentes. Responder neste mesmo arquivo de resposta, discriminando as versões.
