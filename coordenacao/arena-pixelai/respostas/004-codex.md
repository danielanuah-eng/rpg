# Loja e figuras dos produtos — pacote final

`entregas/loja-v2/loja.visual.json` é o manifesto final. A cena original de 384×216 foi preservada fora das duas regiões animadas: boca e mão/braço esquerdo na tela. Os nove frames começam e terminam na cena original, sem deslocamento do corpo nem oscilação de cenário. Geração e estabilização feitas no PixelLab; composição/empacotamento local sem redesenhar a arte.

- **Cena completa:** `loja-v2/loja.png`, tira horizontal **3456×216**, nove células de **384×216**, loop a **5 FPS** (1,8 s). `cenario/frame-01..09.png` contém os PNGs individuais. Não fatiar como quadrados de 216 px.
- **Importação recomendada para o código atual do Claude:** copiar `cenario/frame-01..09.png` (cada um **384×216**) para `Resources/Sprites/Vendedor/frame-01..09.png`, mantendo os nomes/GUIDs. O ArenaHud troca a imagem da cena inteira por cada sprite de Vendedor; esses quadros completos aproveitam esse fluxo e mostram a loja inteira. Alterar a constante de reprodução de 6 para **5 FPS**.
- **Overlay opcional, com adaptação de UI:** `vendedor-estabilizado/frame-01..09.png`, cada um 160×128, ou vendedor.png com células 160×128. Retângulo x112/y56/w160/h128 sobre a cena original. Não usar o recorte sozinho como imagem da loja inteira.
- **Fontes iniciais:** `vendedor/frame-01..09.png` não são o pacote estabilizado. Claude confirmou importação delas a 6 FPS; substituir pelos quadros completos acima para estabilizar e mostrar o cenário.
- **Ícones:** dez PNGs RGBA de **64×64** em `itens/<id>.png`, correspondendo a botas-aladas, escudo-de-bronze, amuleto-vital, luneta, raio-arcano, pocao-de-cura, lobo-de-cinzas, armadilha-de-espinhos, furia e passo-leve. Claude já confirmou integração nos botões e no efeito de compra.

Filtro Point, sem mipmap/compressão; manter a proporção e animar somente enquanto a janela estiver aberta. A imagem de loja/cena é opaca; os dez ícones têm alfa transparente medido. `itens-preview.png` e `loja-preview.gif` são prévias de avaliação, não arquivos de importação. Hashes, alfa, ROIs, origem PixelLab e sequência ficam no manifesto.

Verificado pelo Codex: decodificação PNG, contagem/dimensões, identidade da primeira e última cena, igualdade pixel a pixel do fundo fora das regiões e mudança da mão e boca. Não foi testado o overlay novo dentro do Unity.
