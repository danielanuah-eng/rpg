# 010 — Mapa vivo: abas Objects, Map e UI do PixelLab
**De:** usuario (via claude)
**Para:** codex
**Status:** aberta
**Responsável:** codex

O autor achou o mapa estático demais. Hoje o tabuleiro é feito só de cubos coloridos e piso texturizado, e as árvores, arbustos e caixas também são cubos.
O pedido é produzir **toda a arte do mapa e da interface no PixelLab**, usando cada aba para o tipo de arte a que ela serve.
Esta tarefa inclui a 009 (`OBJETOS-CENARIO.md`) e amplia o pedido.

Padrão geral:
- Vista isométrica, igual à dos lutadores.
- 128 px por casa.
- Pixel art com contorno escuro de 1 px.
- Luz vinda do noroeste.
- Fundo transparente com alfa medido.
- Uma variante por cenário: Pedreira, Bosque, Deserto, Neve e Vulcão.

---

## Aba Objects: tudo o que fica em cima do mapa
Serve para objetos soltos que ficam sobre uma casa ou fora do tabuleiro, como props, estruturas e interativos, com estados e animação.
Use `create_map_object`, `create_1_direction_object` ou `create_object_pro_flash` para criar, `create_object_state` para os estados e `animate_object` para os loops.

1. **Tudo de `ArenaTaticaUnity/Documentacao/OBJETOS-CENARIO.md`**, na ordem indicada lá. Começar pela Loja do Mascate, depois a Caixa, o Arbusto, a Árvore e a Pedra.
   - Arbusto, Árvore e Caixa vão substituir os cubos do tabuleiro. Por isso precisam caber em 1 casa, com a âncora no centro da base.
2. **Vida no mapa:** objetos animados em loop, para tirar o ar estático.
   - Árvore e arbusto balançando ao vento: 4 a 6 quadros.
   - Tocha ou braseiro com chama.
   - Bandeira tremulando.
   - Água ou lava borbulhando, no Vulcão.
   - Cristais pulsando, na Neve.
   - Poeira rolando, no Deserto.
   - Vagalumes, no Bosque.
3. **Pequenos seres de fundo, só decorativos e fora do tabuleiro:** pássaro pousado que levanta voo, coelho, corvo na placa, morcego no Vulcão.

> **Substituída pela tarefa 011 (mapa em Blender).** Nesta tarefa, a aba Map fica só com as texturas em pixel art que o kit do Blender usa.

## Aba Map: o chão e o terreno (PRIORIDADE 1, atualizado pelo autor)
O autor acha os mapas atuais muito feios. **Os mapas não devem ser totalmente gerados.** A disposição continua vindo do MapGen do jogo: relevo, árvores, caixas e zona.
Do PixelLab vem **só a estética**: um kit modular por cenário, que o jogo monta casa a casa. Por isso, nada de imagem de mapa pronta para o tabuleiro.
O `create_map` só serve para o entorno decorativo, fora do tabuleiro.
Faça este lote antes de Objects e UI.

Serve para o terreno em si: tiles, tilesets, transições entre terrenos e o entorno do mapa.
Use `create_isometric_tile`, `create_tiles_pro`, `create_topdown_tileset` e `create_map`.

1. **Piso do tabuleiro por cenário**, em tile isométrico de 1 casa:
   - Versões: base, variação 1, variação 2 (com pedrinhas e rachaduras) e a casa de zona central, mais destacada.
   - Os 4 terrenos que você já entregou em `eventos-cenarios/terrenos` servem de referência de paleta.
2. **Bordas e laterais do bloco:** a face lateral de terra ou pedra que aparece nos desníveis de altura. Hoje são cubos lisos.
3. **Entorno, fora do tabuleiro:** uma ilha ou plataforma que segura o tabuleiro, com as bordas se desfazendo em grama, areia, neve ou rocha. Pode ser um mapa grande via `create_map`, para ficar atrás de tudo.
4. **Transições:** grama com terra, areia com pedra, neve com rocha e lava com basalto.

## Aba UI: a interface
Serve para elementos da interface: painéis, botões, molduras, barras e ícones. Use `create_ui_asset`.

1. **Painéis:** a lateral esquerda (lutadores, ações e loja), o rodapé de log, o topo com rodada e placar, e a janela da loja. Precisam de moldura com bordas em 9-slice, ou seja, cantos separados das bordas.
2. **Botões:** os estados normal, hover, pressionado e desativado. Uma versão em destaque dourado para "Mover" e "Iniciar".
3. **Barras e contadores:** a barra de PV, os marcadores de PA e PM e o contador de moedas e Galeões.
4. **Ícones das skills de cada lutador** (por skillId). Os ícones da loja já existem; manter o estilo deles.
5. **Logo "ARENA TÁTICA"** em pixel art.

---

## Formato de entrega (para Claude integrar)
- **Pastas:** `entregas/mapa-vivo/objects/<objeto>/`, `entregas/mapa-vivo/map/<cenario>/` e `entregas/mapa-vivo/ui/<elemento>/`.
- **Animação:** tira horizontal de quadros quadrados (`<nome>_<estado>.png`). Objeto não quadrado vai em quadros separados (`frame-NN.png`).
- **Manifesto:** um `manifesto.json` por aba, com os campos `aba`, `nome`, `cenario`, `estado`, `arquivo`, `quadros`, `fps`, `tamanho`, `ancora`, `alfa`, `colisao`, `camada` (`ysort`, `chao` ou `topo`) e `substitui`.
  - `substitui` diz qual elemento atual a peça troca. Exemplos: `terreno:Arvore`, `piso:a`, `ui:botao`.
  - Para a UI, incluir também `slice9`, com os 4 cantos em px.
- **Ordem:** entregar por lotes, na ordem objects, map, ui. Cada lote fechado pode ser entregue sem esperar o resto.

## Regras
- A arte não muda regra. Dentro do tabuleiro, os objetos só representam os terrenos que já existem: Arbusto, Árvore e Caixa.
- O que está marcado como "(proposta)" ganha arte, mas não ganha regra.
- Claude troca os cubos e painéis atuais pelos sprites, integra e faz o upload.
- O Codex confere loop, âncora e transparência antes de entregar.
