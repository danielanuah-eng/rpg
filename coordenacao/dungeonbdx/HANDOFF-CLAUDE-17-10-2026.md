# DungeonBDX — retomada (Claude → Claude, 16/10 à noite)

**Leia só este arquivo.** O anterior (`HANDOFF-CLAUDE-16-10-2026.md`) já foi absorvido; consulte-o apenas para caminhos/cuidados operacionais.
**GPT/Codex parado até segunda ordem: Claude faz tudo.** NanoBanana (Gemini) gera as imagens novas. Priorizar etapas curtas, handoff e /clear.

## Feito nesta sessão (sem commit)

- **Isométrico 2D aplicado** (editado no WT `tmp/dungeonbdx-step1-git` e sincronizado com `tmp/dungeonbdx-aq-sync.py`):
  - `Runtime/AqBoardView.cs`: câmera ortográfica com inclinação de 30° (losango 2:1), giro 360° que repousa em 45°+90k (`RotateCamera`, `SettleCamera`, `EaseCamera`), enquadramento por cantos do tabuleiro, arrasto preso ao chão, sprites em pé com `IsoStretch`, `SortingGroup` na âncora (ordem pelos pés), duas figuras na casa atrás-esquerda/frente-direita, ganchos `Resources/DungeonBDX/Iso/Tiles/<superficie>.png` e `Iso/Walls/wall.png` (fallback procedural).
  - `Runtime/DungeonActorAnimation.cs`: `Upright` (gira só no eixo Y). Legado continua com billboard completo.
  - `Runtime/AqMatch.cs`: botões girar ±90; probe verifica a troca da direção do sprite após girar 90° (`facingBefore/After`).
- **Testes EditMode: 361/361** (`ArenaTaticaUnity/Artifacts/dungeonbdx-aq-iso.xml`).
- **Bug do save no player encontrado:** o build "scripts-only" não registrou `DungeonBDX.Arcadia.dll` em `ScriptingAssemblies.json`, então o JSON salvo vinha sem `Record` → "Partida e cópia de segurança inválidas". Correção = build completo.
- **Build completo `DungeonBuild.Windows` foi iniciado** às ~21h. Confirme: `ArenaTaticaUnity/Builds/DungeonBDX/DungeonBDX_Data/ScriptingAssemblies.json` deve conter `DungeonBDX.Arcadia.dll` e o .exe deve ter data posterior a 16/10 21h. Se não, rode de novo:
  `unity.exe build --target StandaloneWindows64 --execute-method DungeonBuild.Windows "<Rpg>/ArenaTaticaUnity"` (em segundo plano, demorado).
- **Contrato visual:** `contratos/ISOMETRICO-2D.md` (projeção, formatos, nomes, FPS, manifesto, o que realmente falta). Tarefa 07 reatribuída a Claude.

## Próximos passos, em ordem

1. Conferir o build (acima) e rodar `tmp/dungeonbdx-aq-captures.ps1` (PowerShell, na raiz Rpg). Exigir `passed:true` nos 3 JSON.
2. Abrir as capturas em `entregas/08-configuracao-arcadia/` e corrigir: enquadramento, piso, tamanho das figuras, sobreposição, HUD, legibilidade das duas figuras na casa, Colosso, portas, Vingança mortal (`deathAfterAttack`), interferência do shell legado.
3. Pedidos ao NanoBanana (antes do GPT; agora Claude): **amostra** = piso `wood` (512×512, de cima), parede (512×192, frontal) e `lyra-faisca` (estáticos 8 dir + `idle` 4 diagonais). Enviar ao Gemini pela UltraBridge (skill `empresa`); se não houver ponte com Gemini funcionando, gerar `entregas/nanobanana/PROMPTS-COMPLETOS.md` + `PEDIDOS.json` para o autor colar. Integrar a amostra antes do resto.
4. Bateria EditMode final, README/REFERENCIAS-E-DECISOES (WT), `entregas/08.../README.md` + `validacao.json`, atualizar `PIPELINE.md` e `estado.codex.json`.
5. Commit só dos arquivos próprios (`tmp/dungeonbdx-aq-owned.txt` + `.meta` novos do Unity principal + contrato/notas), nunca `git add .`. Push automático só com a meta validada.

## Fatos úteis (não reler)

- Sprites da Arena: quadros 176×176, PPU 128, pivô (0,5; 0,153); clipes animados existem nas **4 diagonais** (exatamente as direções da vista iso em repouso); estáticos em 8 direções.
- Faltam de verdade: sprites de `lyra-faisca`, `orion-prisma`, `vaelis-vinculo`; `hit`/`death` de `baltazar`; texturas de piso (7), parede, fichas, UI, props.
- Python do projeto: `C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe` (o `python` do PATH não existe).
- Unity CLI: `C:/Users/Anuah/AppData/Local/Unity/bin/unity.exe`; os vários `unity.exe` pequenos no Gerenciador são processos do CLI, não Editor.
