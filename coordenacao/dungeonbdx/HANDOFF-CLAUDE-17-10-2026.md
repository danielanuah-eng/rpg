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

## Commit seguinte: placar com ícones, golpe/dano dos vilões, modo clássico (17/10)
- `AqUi.Chip(parent, ícone, texto, …)`: ícone PixelLab quadrado + texto (sem arte, só texto). Cabeçalho: emblema (guilda), dado (turno), moeda, save. Faixa de missões: troféu por guilda + pergaminho (`quest`).
- PixelLab com ~4,9 gerações até 10/11: ataque e dano dos 6 vilões feitos por `entregas/pixellab-viloes/golpe-dano.py` (inclinação/deslocamento do quadro parado; `attack_*` 6 quadros, `hit_*` 4 com clarão vermelho), só diagonais leste (espelhadas).
- Modo clássico: `DungeonHeroCatalog.ImportPlayable()` (14 do RPG + 3 originais) no `DungeonShell`; inimigos `raizor`, `nixira`, `domador-de-correntes`/`vaelis-vinculo` (nenhum jogável).
- `DungeonWorldView.Pick`: quando o corpo de quem está na frente cobre a base de quem está atrás, ganha o mais próximo em termos relativos (base pelo raio, corpo pela meia-altura).
- EditMode 407/407. Build não refeito nesta parte (os PNGs novos viram Sprite no próximo build).
- Lacaios: cada capítulo agora tem 4 tipos corpo a corpo e 4 à distância (um por posição), com NPCs da Arena que já têm sprite (raizor, guardião de musgo, brumak, gryvro, devon, ossyro, alvyr, viúva, rei bobett, teantha) e nomes de capítulo. Números de combate iguais. Testes `EveryChapterFieldsFourMeleeAndFourRangedMinionKinds` e `EveryChapterMonsterHasSpriteArt`. EditMode 408/409 — a falha é `UiSkinTests` (`Iso/UI/panel-dark.png` e `card-frame.png` apagados na cópia de trabalho por outra mão, não commitado).
- Próximo: níveis Arcadia (vilão menor por capítulo, dragões) mexem no balanceamento — combinar com o autor.

## Níveis Arcadia e o CCleaner (17/10, noite)
- Código (não commitado até os testes passarem): `AqSpawn` ganhou `Huge` e `Tier` (minion/villain/boss/dragon). Vilão menor por capítulo: brumak (6,0) no cap. I; ossyro (5,8) no II, com o Guardião como boss+Huge; gryvro (6,5) no III, mais o `dragao-espectral` (4,4) com altura 2.6. Teste `ChaptersClimbTheArcadiaLevelsUpToADragon`.
- **O Localizador de arquivos duplicados do CCleaner 7 apagou arquivos** do projeto, do `Library/PackageCache` e da instalação do Unity, porque Unity e git têm muitos arquivos idênticos. Foram 4.438 arquivos rastreados (pasta `Arcanoclisma` inteira). O autor decidiu restaurar só o DungeonBDX.
- Ao rodar o localizador, escolher só pastas pessoais. Em `Rpg`, só `ArenaTaticaUnity/Builds`, `Artifacts` e `Logs`. Nunca `Rpg/tmp` (tem o WT).
- Pacotes: o PackageCache foi recriado. 13 pacotes pediam login no Hub, então reaproveitei os tarballs do backup `%LOCALAPPDATA%/Unity/cache/upm.corrompido`. Os embutidos do Editor que faltarem são copiados por `$TEMP/cmp_pkgs.py fix` (compara com `BuiltInPackages`).
- A instalação do Editor 6000.6.4f1 perdeu 24 arquivos (HDRP, physics, não usados). Se der problema, reparar pelo Hub.
- A Lixeira tem 21 arquivos de `Rpg/Tripo` e 5 de `Rpg`, para o autor decidir se restaura.

## Inimigos no padrão Arcadia Quest (17/10, autor: "use como padrão skills e valores do arcadia quest")
- `AqProfiles` (AqBoard.cs): linhas de nível 1 da planilha `KrosArcano/Arcadia_Quest_-_Card_List_1.2.xlsx` (vida, overkill, defesa, movimento, dados, moedas, habilidade). Todo inimigo nasce de um perfil (`Spawn(..., perfil)`); nomes e arte continuam do DungeonBDX. Dragão: Havok jovem (10 dados limitados a `MaxDice` 6).
- Níveis: `minion` (lacaio menor), `major` (lacaio maior: 4º tipo de cada fileira), `villain`, `boss`, `dragon`.
- Habilidades no motor (`AqSession`): undying, pack, pierce, mend, cleave, brutal, quake, prickly, might, breath, echo (`AqProfiles.Has`, combináveis com `+`). Sem efeitos de status (atordoar, empurrar, invocar ficaram de fora).
- Novos: Papa-moedas (Owlbunny) no cap. I; 4 gatos de rua (Wild Pet) e o Guardião como Underlord no II; Conde Vesperal = Lord Fang (à distância), Necromante = Malkhor no III.
- EditMode 417/417, build ok, capturas ok. **Balanceamento ainda não jogado pelo autor.**
- PIL do Python do Codex quebrado (CCleaner): `from PIL import Image` falha.
