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

## Commit seguinte: UI e props PixelLab (autor: "UI primeiro, sobra para animações"; "props batendo no muro, mais props")
- Props: `AqBoardView.PropAnchor(board, cell, metade)` encosta o prop no lado aberto da casa; com parede, `deslocamento + metade ≤ .38` (a parede entra .105). Arte PixelLab em `Iso/Props/<id>.png` (outdoor/indoor: tree, pine, deadtree, bush, barrels, table, chest, sarcophagus, statue, fountain, crates, weapons, candles, pillar, bones, mushrooms) vira billboard com altura por `PropHeights` e largura máx. .72; sem arte, fica o primitivo 3D antigo. `AqBoard.Deco()` espalha cenário extra (sem efeito de regra). Fonte e recorte: `entregas/pixellab-props/` (`recortar.py`).
- UI: `entregas/pixellab-ui/kit2/` (kit 72fc636d) → `Iso/UI/tab|bar-frame|bar-fill|slot|medallion|plaque.png`. `AqUi.Tab` (abas do menu), `AqUi.LifeBar` (cartões de herói do HUD), `AqUi.Icon` (64 ícones 40 px em `Iso/UI/Icons/<nome>.png`, lista em `entregas/pixellab-ui/icones/icones.json` e `prancha.png`; usados em ações, VIDA/SOM/MENU e câmera).
- Testes `Tests/Editor/AqPropArtTests.cs`. EditMode 374/374, build ok, capturas ok.
- PixelLab: **66 gerações** até 10/11 (gasto: props 15, ícones 15, kit UI 25). **Se acabar: Canva** para imagens, textos e fontes (decisão do autor 17/10; conector Canva precisa de autorização em claude.ai).

## Commit seguinte: mapa e paredes PixelLab (autor: "tire as paredes do Blender, faça paredes e mapa no PixelLab")
- Um `create_tiles_pro` (3c8c9727, 64 px, ampliado 8x): pisos `Iso/Tiles/*` (wood, marble, terracotta, mosaic, crypt, garden, street), `Iso/Walls/wall|wall-crypt|hedge`, `Iso/Doors/door`, `Iso/Frame/frame`. Extras não usados: piso vertical, terra, cripta com ossos, grade (`entregas/pixellab-mapa/`). `Iso/manifesto.json` atualizado.
- Paredes finas: `AqBoardView.WallThickness=.06`, `WallHeight=.42` (antes .15–.21 de espessura). `WallArtFor`: cripta → wall-crypt, jardim → sebe, resto → pedra. Portas mais finas. `PropAnchor` usa a espessura nova (prop pode ir até .45 do centro).
- EditMode 374/374, build ok, capturas ok. PixelLab: ver `get_balance` (≈41 gerações).

## Commit seguinte: paredes-estante (autor: "tire as estantes de livros e faça paredes com skin com estante de livros")
- Sem prop de estante: `Room(...,"books")` marca `AqSpace.Wall="books"` em toda a sala; `WallArtFor` usa `Iso/Walls/wall-books` (PixelLab pixflux b6cd2199, 128x56 ampliado 8x, tiling 1x1) em qualquer parede vizinha. Removidos `BuildShelfSprite` e o primitivo de livros (PNGs `Props/bookshelf_*` ficam, sem uso). Fonte em `entregas/pixellab-mapa/estante/`.
- Teste `BookRoomsWearShelfWallsInsteadOfAShelfProp`. EditMode 375/375, build ok, capturas ok (estante um pouco escura sob a luz da cena).

## Commit seguinte: caminhada e idle dos vilões (autor: "espelhe a que já existe… economiza no pixel")
- `walking-6-frames` (template, ~3 ger./direção em 180 px) para Troll, Rei Goblin, Cultista, Esqueleto e Necromante. Regra nova: gerar só 1 diagonal de cada par; `DungeonActorAnimation.Frames` já espelha `(8-d)%8` (SE↔SW, NE↔NW). Troll SE = espelho do SW (martelo sumia); Cultista sem SE (adaga sumia) → usa SW espelhado.
- Idle sem geração: `entregas/pixellab-viloes/idle-respiracao.py` (parte de cima do quadro parado desce 0-1-2-1 px) → `idle_south-east|north-east` dos 6 vilões.
- Teste `VillainsWalkAndBreatheOnEveryDiagonalOrItsMirror`. EditMode: todos do DungeonBDX passam; 1 falha da Arena (`MatchModifierTests.RandomizedSixPlayerSnapshotFitsRelayBudget`, trabalho em andamento da outra sessão). Build ok, probes ok.
- PixelLab: ~21 gerações até 10/11.

## Pendente
1. Animações de ataque/dano dos vilões (gerar 1 diagonal por par e espelhar).
2. Ícones ainda não usados (moedas, emblema, relíquia, pista, cativo, ritual, portal, tesouro, missão, dado, caveira, coroa…) podem entrar nos marcadores do placar/missões.
3. Mais vilões/lacaios (autor quer muito mais; 4 níveis do Arcadia Quest: lacaio menor/maior, vilão menor/maior + dragões).
3. Modo DungeonBDX clássico (`DungeonExpedition.cs:98`, `DungeonHeroProfiles`) ainda usa ids antigos como inimigos/arquétipos — não tocado.

## Fatos operacionais
- Editar no WT `tmp/dungeonbdx-step1-git` e sincronizar `.../python.exe -I tmp/dungeonbdx-aq-sync.py` (copia .cs/.asmdef/.md). PNGs direto no `main`.
- Python: `C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe -I`. Blender: `C:/Program Files/Blender Foundation/Blender 5.2/blender.exe`.
- Testes: `unity test . --mode EditMode --output Artifacts/x.xml` (em `ArenaTaticaUnity`). Build: `unity build . --target StandaloneWindows64 --execute-method DungeonBuild.Windows` (~10+ min). Capturas: `powershell -NoProfile -ExecutionPolicy Bypass -File tmp/dungeonbdx-aq-captures.ps1`.
- PNG novo em `Resources/Sprites` só vira Sprite depois do build (ou `ArenaBuild.EnsureSprites`).
- A outra sessão (UI da Arena) testa na cópia `%LOCALAPPDATA%/ArenaTaticaAndroid`; conferir processos `Unity.exe` antes de abrir o projeto principal.
- Nunca `git add .`, nem `DungeonBDX.unity`, `ProjectSettings.asset` ou os `.meta` de `Sprites/Menu` (alterações alheias).
