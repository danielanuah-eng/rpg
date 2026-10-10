# Tchu tcha — Berserk possuído, pacote final

Nova forma criada no PixelLab: **armadura negra, viseira neon vermelha, escudo com olho demoníaco central, sombras e vapor negro**. UUID `215bb320-de53-44a1-bec7-f2e23f4e7265`; estado `Berserk Possuído v2 Sombras`. Os dois estados anteriores do mesmo grupo foram preservados.

**Importar somente `entregas/tchu-tcha-berserk-possuido/sprites/*.png`:** oito vistas de 128×128 e 40 tiras de idle/walk/attack/hit/death, nas oito direções. Hit com sete frames; demais com nove. Total **344 frames de animação**. Canvas por clipe 168/176/180 é margem transparente; o personagem não foi redimensionado. Nomes já correspondem ao carregador do jogo.

`berserk.visual.json` traz hash, pivô por clipe, PPU 128, FPS por ação e destinos. Usar a pasta **TchuTchaBerserk**, com altura atual 1,85 casas. Claude deve atualizar o UUID em Tools/pixellab-ids.json e ligar o pacote à forma de possessão existente; não criar regras novas. Preservar os GUIDs/referências ao substituir a versão anterior.

- Idle/walk fecham; attack/hit recuperam. Em **32/32 clipes não-death**, último e primeiro frame são idênticos em RGBA visível. Cada primeira pose registra exatamente na vista estática da sua direção.
- O hit permanece em pé; death cai e permanece caído. Revisão visual via DeepSeek das oito folhas e dos oito ataques; Codex confirmou e reparou efeitos sólidos, recuperação e capacete/direção.
- Os flashes que encobriam a armadura foram retirados; grandes rastros luminosos viraram sombras com operações exatas no PixelLab. Pequenos brilhos da armadura, visor e olho foram preservados. As sequências com exposição/retorno estão registradas; não são promessa de nove desenhos únicos por ação.
- `verificacao.json`: 32 reparos canônicos conferidos em RGBA visível contra as decisões; **476 PNGs** dos estados anteriores, vistas e death permaneceram intactos. Transparência medida, não inferida da cor da prévia.

`pixellab-final.zip` contém três estados e a animação alternativa de revisão. Selecionar apenas `Berserk_Possuido_v2_Sombras`, com os cinco nomes canônicos, se usar o ZIP. **Não importar `attack-refinado-sombras`**, grids, fontes, GIFs ou estados antigos como clipes do novo pacote. Prefira os 48 PNGs já empacotados em sprites/.

Prévia: `previas/south.gif`, `south-east.gif`, `north-west.gif`, com pivô fixo, repouso→caminhada→ataque→reação→repouso. São avaliação de assets; integração e reprodução na partida ficam com Claude. Nenhum arquivo de Assets/Tools/código foi alterado pelo Codex.
