# Cenários Blender — revisão 02

Entrega visual revisada conforme os pedidos do autor: **loja no meio, props também pelo interior, árvores diferentes por mapa e moedas deitadas no chão**. A versão anterior permanece preservada na pasta acima. Esta pasta é a entrega atual.

Há cinco kits: **Pedreira, Bosque, Deserto, Neve e Vulcão**. O Bosque tem três composições editáveis: **Clareira e colina**, **Trilhas cruzadas** e **Terraços e lagoa**. São exemplos de composição; a geração e a integração no jogo continuam com o Claude.

## Arquivos

- `fontes/<tema>.blend`: fonte com texturas incorporadas, kit de originais no pivô zero e prévia de 12×12.
- `fontes/bosque-versao-02.blend` e `bosque-versao-03.blend`: outras duas composições do bosque.
- `mapa/<tema>/*.fbx`: formato principal, Y-up/forward −Z, unidade em metros.
- `mapa/<tema>/*.glb`: alternativa com texturas incorporadas e filtro nearest.
- `mapa/<tema>/*.png`: oito texturas de 64×64 por tema, geradas no PixelLab / Map. Originais e seleção preservados em `fontes/`.
- `manifestos/<tema>.json`: medidas, tris, materiais, sockets, pesos de vento e hashes dos modelos.
- `manifestos/<tema>-distribuicao.json`: posições de exemplo e orientação para distribuição. O Bosque tem mais dois arquivos de distribuição, correspondentes às versões.
- `manifestos/<tema>-validacao.json`: conferência dos arquivos exportados e das fontes reais.
- `manifesto.json` e `validacao.json`: índice e resultados consolidados dos cinco kits.
- `previews/`: renders de cada tema, versões do bosque, detalhes e comparação das árvores. `bosque-tres-versoes.png` mostra as três composições lado a lado.

## Composição solicitada pelo autor

**A loja é um módulo separado e fica no centro, independentemente do tema e do tamanho.** Nas fontes de exemplo seu pivô está em `(0, 0, 0.5)` Blender, convertido para `(0, 0.5, 0)` Unity. Essa altura corresponde às quatro tampas centrais de nível 1. No jogo, calcular a altura sobre o piso central real. A loja ocupa cerca de 2×2 m; Claude precisa conciliar sua ocupação com a zona central, a circulação e a interação do vendedor.

Árvores, lagoa, pedras, fogueira e cristais ficam em casas do **interior**. Os entornos contêm apenas detalhes baixos e poucas cercas, com vão central livre; não há loja, lago ou árvores grandes presos à borda. A prévia reserva uma área plana de 3×3 casas para a lagoa, evita ocupações repetidas e mantém pelo menos 0,5 m entre os bounds dos props internos e o contorno do tabuleiro. As bases são conferidas contra a superfície real das tampas.

As árvores são modelos distintos: pinheiro cônico na pedreira, copa frondosa no bosque, cacto e palmeira no deserto, abeto com **28 ramos abertos e pendentes cobertos de neve** no mapa nevado e árvore carbonizada no vulcão. A palmeira é um módulo adicional do deserto. O abeto nevado foi remodelado após a comparação visual, para distinguir sua silhueta da pedreira.

`moeda_chao` tem relevo e pivô na face inferior; seu disco fica **deitado**. Instanciar sobre o piso onde a lógica existente indicar moedas. As moedas das prévias demonstram aparência e distribuição, sem criar regra de recompensa, preço ou coleta.

## Escala e materiais

Uma casa mede 1 m. `topo_a/b/zona` mede 1×1×0,05 m; cada `coluna_a/b` sobe 0,45 m. Props usam base Z=0 na fonte. As peças de beira e os entornos possuem a exceção documentada nos bounds, pois suas laterais descem abaixo do nível zero.

Cada kit tem entornos de **12, 16 e 18 casas**, para acompanhar os tamanhos atuais do jogo. Colocar o entorno correspondente sem escalar os props. A frente da loja é +Y Blender, equivalente a −Z Unity.

PNG: Point/Closest, Repeat, sem mipmaps no piloto. Os 40 PNGs selecionados têm alfa opaco medido e são comparados com os originais do PixelLab. Não há sombra projetada pintada no piso.

Vento: atributo `vento`, canal R. Base e tronco R=0, pontas R=1 nas peças marcadas com vento. **FBX: primeira cor de vértice. GLB: COLOR_1; COLOR_0 é branco neutro.** O cacto não usa deformação de vento. O shader fica com o Claude.

Materiais emissivos e empties `fx_fogo`/`fx_fumaca` são pontos de integração. Esta entrega contém geometria, materiais e pesos; as animações de vento, água, fogo e fumaça no Unity ainda precisam de integração.

## Verificação e estado

Os relatórios registram reimportação real dos FBX, leitura dos GLB, escala/eixos, pivôs, UV, tris, hashes, filtros, sockets, pesos de vento e vão central de cada entorno. As sete fontes de composição são reabertas para conferir loja central, margem dos props, moedas deitadas, apoio das bases no piso e separação entre os footprints da loja e da lagoa. A lagoa da terceira versão foi deslocada para melhorar a leitura pela câmera.

**Revisão visual 02: entregue para avaliação. Integração e testes no Unity: pendentes do Claude.** Nenhum código ou Asset do jogo foi alterado nesta entrega.

Reprodução: Blender em background com `fontes/construir_cenarios.py -- --theme <tema>`, seguido de `fontes/validar_cenarios.py -- --theme <tema>`. `--preview-only` refaz somente as composições a partir do `.blend` existente; `--layout 2` restringe à terceira composição. Executar também `contato_texturas.py`, `comparar_arvores.py`, `comparar_versoes.py` e `consolidar_entrega.py`. Usar `--python-exit-code 1` para falhas interromperem o processo.
