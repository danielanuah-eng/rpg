# 011 — Kit Blender integrado no Unity (Claude, 11/10/2026)

**Integrado e testado.** A partir de agora o jogo usa os mapas e os modelos do Codex (decisão do autor, 11/10).

## O que entrou
- **Modelos:** os 176 GLB da revisão 02 e os entornos limpos da revisão 03. Ficam em `Assets/ArenaTatica/Resources/Models/Mapa/<cenário>/`. O relicário e a barricada (mapas rotativos) ficam em `comum/`. Os FBX não são usados.
- **Tabuleiro** (`ArenaPresentation.BuildKit`):
  - cada casa tem uma tampa `topo_a`/`topo_b` (a zona usa `topo_zona`) e colunas empilhadas pelo relevo;
  - `beira_reta` nas bordas e o entorno do tamanho da partida (12, 16 ou 18);
  - árvore, arbusto e caixa usam o modelo do cenário;
  - as moedas usam `moeda_chao`.
  - Sem o kit, o desenho antigo em cubos continua funcionando.
- **Loja:** decisão do autor de 11/10. A `loja_mascate` ocupa as 4 casas do meio, como terreno novo `Loja`: ninguém entra e ela tapa a visão. A zona central (Domínio) passa a ser as **8 casas encostadas na loja**.
- **Mapas:** `Tools/mapas-codex.mjs` converte as 7 composições da revisão 03 (12×12) e os 9 layouts rotativos (12/16/18) em `Resources/Mapas/codex.json`.
  - Os 16 passam em `MapGen.Problem`, com casas de início e zona alcançáveis (teste `CodexMapsAreAllPlayable`).
  - Na partida, "sorteado" escolhe um mapa do Codex do tamanho certo. O gerador antigo virou opção ("Gerado na hora").
  - Atenção: nas composições a matriz de altura é `[coluna][linha]`; nos layouts rotativos é `[linha][coluna]`. O conversor trata os dois casos e conferiu a altura de cada peça contra a matriz.
- **Peças → regra:**
  - `arvore*` vira Árvore; `arbusto` vira Arbusto; `caixa`, `caixa_rachada` e `pedra` viram Caixa (bloqueiam).
  - `enfeite_*`, `relicario_solar` e `barricada_partida` são só visuais.
  - As `moeda_chao` dos exemplos foram ignoradas: as moedas continuam vindo da regra.
- **Editor de mapas:** ferramentas "Trocar modelo da peça" (pinheiro, palmeira, pedra…) e "Enfeite", com 14 enfeites. A loja é fixa.

## Ainda não integrado
- Shader de vento (a cor de vértice está no GLB, como COLOR_1).
- Animação da água e da lava (`*_emissivo`).
- Fogo e fumaça nos sockets `fx_fogo`/`fx_fumaca`.
- Modelos de modo: `bau_fechado`, `pilar_galeao`, `totem_dominio`.
- Estados `caixa_quebrada`.
