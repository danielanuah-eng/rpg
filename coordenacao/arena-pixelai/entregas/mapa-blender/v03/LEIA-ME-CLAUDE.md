# Cenários Blender — revisão 03

Revisão solicitada pelo autor em 10/10/2026: **dividir os props, evitar que tudo fique junto e dar um design diferente aos mapas**. A entrega atual é esta pasta. A revisão 02 permanece preservada e fornece os modelos/texturas compartilhados.

Há **sete cenas editáveis**: três composições do Bosque, Deserto, Montanha rochosa, Neve e Vulcão. Cada composição tem uma matriz de altura e posições próprias; não são apenas trocas de material sobre o mesmo relevo. A loja permanece central e as moedas ficam deitadas no chão.

| Tema | Composição e distribuição |
| --- | --- |
| Bosque 1 | Clareira entre bosquetes: árvores em setores opostos, clareira ampla e acampamento lateral. |
| Bosque 2 | Trilha entre clareiras: lago e relevo em lados diferentes, duas áreas abertas. |
| Bosque 3 | Encosta e clareira: lago elevado e encosta lateral, lado oposto mais aberto. |
| Deserto | Dunas assimétricas baixas e oásis lateral; duas palmeiras, cactos em setores separados e vazios de areia. |
| Montanha rochosa | Passo largo entre dois maciços; quatro afloramentos espaçados e lago elevado. Mantém o ID `pedreira`. |
| Neve | Vale diagonal entre bancos de neve; abetos afastados, lago congelado e cristais em pontos distintos. |
| Vulcão | Fenda diagonal baixa e três poças de lava em setores separados; basalto e árvores carbonizadas, sem o acampamento repetido do bosque. |

Os props principais estão nos **quatro setores** de cada cena. O centro tem um raio de 2 m sem pivôs de props grandes, além da própria loja. As oito casas adjacentes à zona central são reservadas para passagem. Cada prop interno tem pelo menos 0,5 m de margem do limite do tabuleiro e base conferida contra a superfície real das tampas. A área de 3×3 casas sob o lago principal é plana. Objetos menores, moedas e placa também são conferidos quanto ao apoio. Cada composição tem nove moedas. Após a revisão visual, os entornos de todos os temas foram limpos: sem cerca solta e sem cogumelos, capim ou pedras incorporados nas bordas externas. A Neve usa três abetos afastados, com o centro mais aberto.

## Arquivos e reprodução

- `fontes/<tema>.blend`: cenas com texturas incorporadas, originais no pivô zero em coleção oculta e composição visual 12×12 em coleção separada.
- `fontes/bosque-versao-02.blend` e `bosque-versao-03.blend`: outras composições do bosque.
- `manifestos/*-distribuicao.json`: matriz de alturas, coordenadas Blender/Unity, rotação, escala e distribuição por setor. Cada fonte possui seu JSON correspondente.
- `mapa/<tema>/entorno*.fbx` e `.glb`: os três tamanhos de entorno de cada tema, limpos e reexportados nesta revisão.
- `manifestos/<tema>.json`: índice dos modelos; os caminhos de FBX/GLB apontam para **`../v02/mapa/`**, exceto os entornos revisados que ficam nesta pasta.
- `manifestos/*-validacao.json` e `validacao.json`: conferência técnica real, incluindo comparação dos 21 pares de layouts.
- `previews/quatro-cenarios.png` e `bosque-tres-versoes.png`: comparação das cenas desta revisão.
- `fontes/revisao-visual-deepseek.log` e `revisao-visual-final-deepseek.log`: revisão visual via OpenRouter e reconferência após os ajustes, limitadas aos renders do projeto.

O conjunto mantém **176 FBX e 176 GLB**: 161 modelos por formato são reutilizados da revisão 02, e os 15 entornos foram limpos e reexportados. As 40 texturas são compartilhadas. Os sete arquivos `.blend` desta revisão incorporam as imagens e preservam os originais, com os entornos já limpos. Para reconstruir, manter `v02/` e `v03/` lado a lado.

Blender 5.2.2: executar `--background --threads 6 --python-exit-code 1 --python fontes/recompor_cenarios.py -- --theme <tema>`, depois `fontes/validar_cenarios.py` com o mesmo argumento. Gerar comparações com `fontes/comparar_versoes.py` e com `-- --temas`; finalizar com `fontes/consolidar_entrega.py`. Os scripts resolvem caminhos pelo próprio arquivo.

## Integração e estado

**Revisão visual 03 entregue para avaliação do autor. Integração e testes no Unity pendentes do Claude.** Estas fontes e JSONs são exemplos visuais de composição; a adaptação ao MapGen, ocupação e rotas jogáveis fica com o Claude. Não afirmar publicação no site com base apenas no push dos assets.

A loja usa pivô `(0, 0, 0.5)` no Blender, equivalente a `(0, 0.5, 0)` no Unity sobre a zona central de nível 1; calcular a altura pelo piso real no jogo. Sua orientação varia entre temas. Manter a reserva de aproximadamente 2×2 m e acesso ao vendedor. Não aplicar uma disposição fixa de árvores/cristais/fogueira em volta da loja a todos os biomas.

As moedas são exemplos visuais. Instanciar o modelo `moeda_chao` onde a lógica existente determinar. Vento, fogo/fumaça e água continuam com os sockets, materiais e pesos da revisão 02: FBX usa a primeira cor de vértice; GLB usa COLOR_1 com COLOR_0 neutro. Escala, eixos, pivôs, filtros e limites são os descritos no [guia da revisão 02](../v02/LEIA-ME-CLAUDE.md).
