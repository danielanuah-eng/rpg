# Cenários 3D — entrega atual na revisão 03

A entrega atual está em **[v03/LEIA-ME-CLAUDE.md](v03/LEIA-ME-CLAUDE.md)**: sete composições com relevo e distribuição próprios, props em grupos menores nos quatro setores, espaço livre ao redor da loja central e moedas deitadas no chão. Dos **176 modelos por formato**, 161 são compartilhados com `v02/` e 15 entornos foram limpos/reexportados em `v03/`. As 40 texturas são compartilhadas. Conferência consolidada: `v03/validacao.json`.

O registro abaixo e os arquivos desta pasta são o **piloto anterior, preservado**. Para integrar a composição revisada, usar `v03/` com os modelos de `v02/`.

## Registro do piloto da tarefa 011

Produzido no **Blender 5.2.2** com texturas de **64×64 px geradas no PixelLab / Map**. Primeiro lote completo para avaliação do autor, conforme ordem da tarefa 011. **29 peças**: 26 módulos/props e três entornos para 12, 16 e 18 casas. O piloto recebeu aprovação com ajustes; os cinco cenários revisados estão em `v02/`. Código do jogo, Assets do Unity e upload continuam com o Claude.

## Arquivos

- `fontes/bosque-piloto.blend`: fonte editável com materiais e texturas incorporados. A cena inicial mostra a demonstração de 12×12; os modelos de origem, todos no pivô zero, estão na coleção `KIT_BOSQUE_ORIGINAIS_PIVO_ZERO`, oculta na prévia. Ativar a coleção para editar uma peça.
- `mapa/bosque/*.fbx`: formato principal para Unity; um arquivo por peça, escala em metros, forward −Z/up Y, transforms aplicados.
- `mapa/bosque/*.glb`: alternativa com texturas incorporadas e filtros nearest.
- `mapa/bosque/bosque_*.png`: oito texturas originais ao lado dos modelos, com os nomes dos materiais. Os 16 resultados originais do PixelLab e a seleção estão em `fontes/`.
- `manifesto.json`: dimensões, tris, pivôs, vento, materiais emissivos, sockets, substituições e hashes por peça.
- `validacao.json`: resultados da reimportação dos FBX e leitura dos GLB, com limitações explícitas.
- `previews/bosque-tabuleiro-12x12.png` e `bosque-detalhe.png`: renders de demonstração. A distribuição de relevo e obstáculos destes renders não substitui MapGen.

## Montagem

1. Manter cada casa em uma unidade. Os três `topo_*` ocupam **1×1×0,05 m**. O pivô é o centro da base, em altura zero.
2. Empilhar `coluna_a/b`, cada uma com **1×1×0,45 m**, conforme o nível do relevo. A tampa fica na altura `nível × 0,45`. As quatro casas da zona central usam `topo_zona`.
3. Colocar `arbusto`, `arvore` e `caixa` sobre a tampa; a base dos props está exatamente na altura zero. Os estados rachado/quebrado da caixa são opções visuais, sem nova regra.
4. `beira_reta/canto` encaixam na borda superior e têm raízes pendendo abaixo do pivô. Orientar/rotacionar conforme o lado externo. Seus bounds no manifesto descrevem essa exceção ao chão zero.
5. Escolher `entorno`, `entorno_16` ou `entorno_18` pelo tamanho real. Os três têm vão central aberto e livre para o tabuleiro correspondente; nenhum contém piso jogável. A divergência de tamanhos foi encontrada em `Core/Board.cs`: o código atual usa 12/16/18, enquanto o pedido mencionava máximo 12. As variantes evitam alterar as regras ou ampliar os props por escala.
6. Os props pequenos soltos são decorativos, exceto as substituições já especificadas para os modos Tesouro/Galeões/Domínio. A loja tem aproximadamente 2×2 m e pode ser usada separada do entorno; o vendedor continua sprite. Frente da loja: +Y Blender, convertida em −Z Unity.

## Materiais e movimento

Configurar os PNGs como texturas, com **Point**, **Repeat**, sem compressão que borre os pixels e com mipmaps desativados no piloto. São opacos; alfa 1 medido em todos os pixels. Usar o shader iluminado do jogo para preservar as sombras dinâmicas; os pisos não têm sombra projetada pintada.

As árvores e arbustos têm atributo de cor `vento`: canal **R=0** em troncos/raízes e **R=1** nas pontas das folhas, G/B=0. **FBX: primeira cor de vértice, linear. GLB: COLOR_1**, pois COLOR_0 branco preserva a aparência nos leitores glTF comuns. No fallback GLB, confirmar que o importador disponibiliza COLOR_1 ao shader; FBX é o formato principal. Há pesos também nas copas/arbustos incorporados aos entornos. A entrega contém os pesos, sem shader de vento ou animação de jogo pronta.

Materiais separados: `bosque_agua_emissivo`, `bosque_cristal_emissivo`, `bosque_runa_emissivo` e `bosque_lanterna_emissivo`, para animar parâmetros no jogo. `enfeite_tocha` e `enfeite_fogueira` têm socket **fx_fogo**; a loja tem **fx_fumaca**. Os mesmos sockets são preservados nos entornos e suas posições são verificadas na exportação. As chamas/fumaça entram como efeitos em sprite pelo Claude.

## Conferência e estado real

29 FBX reimportados no Blender; 29 GLB conferidos em estrutura, escala/eixos, UV, tris, pesos de vento, filtros e sockets. Comparação dos PNGs com os originais e hashes dos modelos registrada. Todos os props abaixo de 3 mil tris; entornos abaixo de 40 mil. O `.blend` foi reaberto e seus 29 modelos conferidos. O centro de cada entorno foi conferido por raycasts sobre todas as casas.

**Piloto aprovado com ajustes; avaliação visual da revisão 02: pendente. Integração e teste no Unity: pendentes do Claude.** Esta entrega não altera código nem publica o jogo.

Reprodução: executar `construir_bosque.py` no Blender em background, depois `validar_entrega.py`. Ambos usam somente esta pasta de entrega.
