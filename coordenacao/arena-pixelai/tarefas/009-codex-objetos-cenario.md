# 009 — Objetos de cenário (aba "objects")
**De:** usuario (via claude)
**Para:** codex
**Status:** aberta
**Responsável:** codex

Produzir no PixelLab, aba "objects", os objetos listados em `ArenaTaticaUnity/Documentacao/OBJETOS-CENARIO.md`.
Essa lista já traz, para cada objeto, a descrição, o prompt em inglês, os estados e os quadros de animação.

**Ordem:**
1. Loja do Mascate.
2. Caixa nos 3 estados.
3. Arbusto e Árvore nos 5 cenários.
4. Pedra Grande.
5. Baú, Pilar de Galeão e Totem.
6. Props de chão e enfeites de borda.

**Formato:**
- Vista isométrica, igual à dos lutadores.
- 128 px por casa. A loja tem 256×256 px e a árvore 128×256.
- Fundo transparente com alfa medido.
- Animação como tira horizontal de quadros quadrados (`<objeto>_<estado>.png`). Objeto não quadrado vai em quadros separados (`frame-NN.png`).
- Âncora no pé, no centro da base.
- Uma variante por cenário (Pedreira, Bosque, Deserto, Neve e Vulcão), com a mesma silhueta.

**Entrega:**
- Pasta: `entregas/objetos-cenario/<objeto>/`.
- Incluir um `manifesto.json` com estes campos: `objeto`, `cenario`, `estado`, `arquivo`, `quadros`, `fps`, `tamanho`, `ancora`, `alfa`, `colisao` e `camada` (`ysort`, `chao` ou `topo`).

**Regras:**
- Dentro do tabuleiro, os objetos só representam os terrenos que já existem: Arbusto, Árvore e Caixa.
- Os itens marcados como "(proposta)" (caixa destrutível, barril explosivo, poste tombado) podem ganhar arte, mas não viram regra sem decisão do autor.
- Claude integra no Unity e faz o upload.
