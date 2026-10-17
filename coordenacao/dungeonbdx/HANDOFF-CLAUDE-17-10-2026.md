# DungeonBDX — retomada (Claude → Claude, 17/10, 5ª parte)

**Leia só este arquivo.** GPT/Codex parado: Claude faz tudo. **Tudo que o Gemini faria agora vai pelo PixelLab** (decisão do autor, 17/10); prioridade: UI do jogo (comum à Arena Tática e ao DungeonBDX) e personagens. `Rpg/Tripo/` = artes que o autor já fez no Gemini. `Rpg/KrosArcano/` = material do Arcadia Quest, **referência de mecânica E visual**, mas arte/nomes sempre originais. Sem push sem perguntar.

## Feito e commitado (local, sem push)
- `47bdb66e` tabuleiro Blender + Tripo + HUD PixelLab. `9147d216` docs.
- Commit desta parte (ver `git log`): elenco e inimigos.
  - `AqContent.Playable` = os 14 heróis do RPG (principe-kellen, ezequiela, damian, arqueiro-jack, cheiro-pneu, ciantia, tchu-tcha, colosso **"Primal Beast"**, baltazar, asamyh, perry, diantus, talyssa, edgar). O dicionário `Heroes` continua com 31 definições (fixtures de teste e reuso de habilidades em inimigos); só `Playable` aparece na escolha (`AqHeroImport.PlayableIds`). `DefaultParties` só com jogáveis.
  - `AqBoard`: lacaios por capítulo (I: Saqueador de pedra, Troll do esgoto / Cultista, Xamã; II: Carcereiro, Lobo de cinzas / Vespa, Cultista; III: Esqueleto lanceiro, Viúva / Xamã, Cultista). Jack saiu dos inimigos. Números dos lacaios inalterados.
  - Chefes (`Boss()`: 5 vida, 4 se à distância, DEF 1, 3 dados, overkill 6, recompensa 2): Cap. I Rei Goblin Ferrugem (4,0); Cap. III Necromante da Cripta (3,5, distância) e Conde Vesperal (5,3). Cap. II mantém o Guardião da praça.
  - `AqSpawn.SpriteFolder` + `AqBoard.EnemyArt` (id → pasta) para inimigos sem entrada em `Content.Fighters`; `AqHeroImport.Apply` não sobrescreve. Chefes desenhados com altura 1,8 (`AqBoardView`: lacaio = 1,35 só se recompensa < 2).
  - Sprites PixelLab (8 direções) em `ArenaTatica/Resources/Sprites/`: TrollEsgoto, CultistaEncapuzado, EsqueletoLanceiro, ReiGoblinFerrugem, NecromanteCripta, CondeVesperal (+ caminhada só nas 4 diagonais; o animador espelha/aproxima as outras). Exportador `entregas/pixellab-viloes/exportar.py` (corrigido para quadros `frame_000`).
  - Novos testes `AqMapTests`: elenco de 14 e Jack fora dos inimigos; lacaios variam por capítulo e chefes têm arte carregável.
- Verificação: EditMode **372/372**; build Windows ok (o build aplica `EnsureSprites` e grava os `.meta`); capturas em `entregas/08-configuracao-arcadia/` com probes `passed:true` (conferidas visualmente).

## Pendente
1. **Perguntar ao autor** como gastar as ~113 gerações PixelLab que restam até 10/11 (caminhada 4 diagonais ≈ 12/personagem; conjunto completo ≈ 45): animar chefes x lacaios x UI (aba, barra de vida, ícones de ação que faltam).
2. Mais vilões/lacaios (autor quer muito mais; 4 níveis do Arcadia Quest: lacaio menor/maior, vilão menor/maior + dragões).
3. Modo DungeonBDX clássico (`DungeonExpedition.cs:98`, `DungeonHeroProfiles`) ainda usa ids antigos como inimigos/arquétipos — não tocado.

## Fatos operacionais
- Editar no WT `tmp/dungeonbdx-step1-git` e sincronizar `.../python.exe -I tmp/dungeonbdx-aq-sync.py` (copia .cs/.asmdef/.md). PNGs direto no `main`.
- Python: `C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe -I`. Blender: `C:/Program Files/Blender Foundation/Blender 5.2/blender.exe`.
- Testes: `unity test . --mode EditMode --output Artifacts/x.xml` (em `ArenaTaticaUnity`). Build: `unity build . --target StandaloneWindows64 --execute-method DungeonBuild.Windows` (~10+ min). Capturas: `powershell -NoProfile -ExecutionPolicy Bypass -File tmp/dungeonbdx-aq-captures.ps1`.
- PNG novo em `Resources/Sprites` só vira Sprite depois do build (ou `ArenaBuild.EnsureSprites`).
- A outra sessão (UI da Arena) testa na cópia `%LOCALAPPDATA%/ArenaTaticaAndroid`; conferir processos `Unity.exe` antes de abrir o projeto principal.
- Nunca `git add .`, nem `DungeonBDX.unity`, `ProjectSettings.asset` ou os `.meta` de `Sprites/Menu` (alterações alheias).
