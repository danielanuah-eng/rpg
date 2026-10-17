# DungeonBDX — retomada (Claude → Claude, 16/10 noite, 2ª parte)

**Leia só este arquivo.** GPT/Codex parado: Claude faz tudo. Imagens novas: NanoBanana (Gemini), geradas **manualmente pelo autor** (o Gemini confirmou que não grava arquivos).

## Feito e commitado no `main` local (SEM push — perguntar ao autor antes)

- `2688762c` modo Arcadia em 2D isométrico (câmera 30°, giro 45°+90k, pés no pivô, Colosso em escala, contrato `contratos/ISOMETRICO-2D.md`).
- `0872c45d` mapa mais limpo (fichas só por cor, distâncias discretas, passagens abertas sem moldura), figuras a 74% (`AqBoardView.FigureScale`), HUD do PC em três colunas, coluna de câmera à direita.
- Evidência: 362/362 EditMode, 3 probes `passed:true`, capturas em `entregas/08-configuracao-arcadia/`.
- Prompts NanoBanana em estilo **RPG pixel art 16 bits**: `entregas/nanobanana/PROMPTS-COMPLETOS.md` (amostra 01, lote 02, UI/ícones); nota `notas/20261016_claude_para_autor_nanobanana-manual.md`.

## Próximos passos, em ordem

1. **Tabuleiro mais bonito no Blender** (pedido do autor "no final"): modelar/renderizar peças do tabuleiro (moldura, paredes baixas 0,26, porta fechada, piso por superfície, props) em estilo que combine com pixel art (render ortográfico 30° ou texturas pixeladas, sem cara de peça de tabuleiro impressa). Ver fontes anteriores em `coordenacao/arena-pixelai/entregas/mapa-blender/v02/`. Integrar pelos ganchos `Resources/DungeonBDX/Iso/Tiles/<superficie>.png`, `Iso/Walls/wall.png` (já no código) e criar os de porta/props.
2. Quando o autor salvar PNGs em `entregas/nanobanana/amostra-01/`: recortar o verde, reduzir (piso 512, parede 512×192, quadro 176×176, PPU 128, pivô 0,5/0,153), medir alfa, gravar `manifesto.json`, integrar.
3. Opcional, só se o autor quiser: subdividir o grid (2×2 por casa) e dobrar movimento/alcance — hoje a proporção foi resolvida só na escala visual.
4. Pendências: captura dedicada da Vingança mortal; QA de toque e desempenho Android.

## Fatos operacionais

- Editar no WT `tmp/dungeonbdx-step1-git` e sincronizar: `.../python.exe tmp/dungeonbdx-aq-sync.py` (o WT está atrás do `main` só no histórico; conteúdo igual).
- Python: `C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe`. Em heredoc bash, `"\\n"` vira quebra real: use `chr(92)+'n'` ou o Edit.
- Testes: `unity test . --mode EditMode --output Artifacts/x.xml` (em `ArenaTaticaUnity`). Build: `unity build . --target StandaloneWindows64 --execute-method DungeonBuild.Windows`. Capturas: `powershell -File tmp/dungeonbdx-aq-captures.ps1` na raiz `Rpg`.
- Commit com caminhos explícitos; nunca `git add .`, nem cena `DungeonBDX.unity` nem `ProjectSettings.asset`.
