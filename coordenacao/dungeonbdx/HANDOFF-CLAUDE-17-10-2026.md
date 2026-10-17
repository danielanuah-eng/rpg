# DungeonBDX — retomada (Claude → Claude, 16/10 noite, 3ª parte)

**Leia só este arquivo.** GPT/Codex parado: Claude faz tudo. Imagens do Gemini/NanoBanana são feitas **pelo autor** e salvas em `Rpg/Tripo/` (o Gemini só reseta à meia-noite). O que não estiver lá → **PixelLab** (prioridade do autor: **UI do jogo**, compartilhada entre Arena Tática e DungeonBDX). PixelLab: ~133 gerações até 10/11; painel de UI custa 10–25.

## Feito nesta parte (commit local, SEM push — perguntar antes)

- **Tabuleiro no Blender** (`entregas/09-tabuleiro-blender/`): `fontes/tabuleiro_texturas.py` (render ortográfico emendável) + `fontes/pos_pixel.py` (paleta reduzida, 8×). Pisos do Blender ficaram como reserva; **a moldura (`Iso/Frame/frame.png`) é a do Blender** e se repete ao longo da borda (`FrameLook`).
- **Lote Tripo integrado** (`entregas/nanobanana/lote-tripo/processar.py` → `saida/` + `manifesto.json`): 7 pisos, parede, porta (faixa central), estante 4 direções (prop `books`, sprite por quadrante da câmera), 8 fichas (sprites no chão, `sortingOrder -20`), portal (espiral), botões normal/selecionado/desativado (9-slice, `AqUi.Skin/Slice`, `DungeonMotion.UseSprites`), painel do rodapé (`AqUi.Panelled`). Moldura de carta **não** usada (formato vertical não casa com os cartões do rodapé). Lyra da Tripo é só referência (1 quadro).
- **Pedidos do autor**: círculos enormes removidos — heróis têm anel fino da guilda; **monstros sem nada embaixo**; início = 4 marcas de canto; portal = anel fino ou arte. Botão **VIDA / VIDA OFF** no topo (ao lado de SOM), salvo em `PlayerPrefs` (`AqBoardView.ShowLife`).
- Import: `DungeonAssetImport.OnPreprocessTexture` → `Iso/` com filtro Point, sem compressão; `Frame/` em Repeat.
- Evidência: 362/362 EditMode, 3 probes `passed:true`, capturas em `entregas/08-configuracao-arcadia/`.

## Em andamento

- PixelLab UI asset `f712c2aa-607a-4fec-9b4e-02eb6129c8b8` ("kit-hud-arena-dungeon": botão, botão de ícone, aba, barra de vida, moldura de retrato, painel; 25 gerações). Recortar as peças para `DungeonBDX/Art/Resources/DungeonBDX/Iso/UI/` e para a Arena (`ArenaHud`).

## Próximos passos

1. Integrar o kit de HUD nas duas engines (Arena: abas Ações/Partida, barra lateral, botões).
2. Ícones (mover, porta, habilidade, descansar, encerrar, girar, zoom, som, menu, vida): pedir ao autor no Gemini (`Tripo/`) ou PixelLab barato (objeto 1 direção).
3. Heróis faltantes (`lyra-faisca`, `orion-prisma`, `vaelis-vinculo`) e `baltazar` hit/death: ~60–80 gerações por herói no PixelLab — só com OK do autor.
4. Pendências antigas: subdividir grid (opcional), captura da Vingança mortal, QA Android.

## Fatos operacionais

- Editar no WT `tmp/dungeonbdx-step1-git` e sincronizar: `.../python.exe tmp/dungeonbdx-aq-sync.py` (copia só .cs/.asmdef/.md). PNGs vão direto no `main`.
- Python: `C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe` (use `-I` ao ler pastas de imagens). Blender: `C:/Program Files/Blender Foundation/Blender 5.2/blender.exe -b --factory-startup --python ...`.
- Testes: `unity test . --mode EditMode --output Artifacts/x.xml` (em `ArenaTaticaUnity`). Build: `unity build . --target StandaloneWindows64 --execute-method DungeonBuild.Windows`. Capturas: `powershell -NoProfile -ExecutionPolicy Bypass -File tmp/dungeonbdx-aq-captures.ps1` na raiz `Rpg`.
- Commit com caminhos explícitos; nunca `git add .`, nem cena `DungeonBDX.unity` nem `ProjectSettings.asset`.
