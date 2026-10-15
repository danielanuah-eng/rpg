# Reserva DungeonBDX 02–03 — Codex

Autor autorizou explicitamente: “Codex assume as etapas 2–3”, após bloqueio semanal do Claude. Execução em `tmp/dungeonbdx-step1-git`, branch `codex/dungeonbdx-step2`; validação no cache principal. Nenhum outro executor altera estes arquivos. Preservar Arena, UI e trabalhos concorrentes.

Reserva antes de criação/edição dos seguintes C# sob `ArenaTaticaUnity/Assets/DungeonBDX/`:

- Combat/DungeonCombat.cs
- Combat/DungeonTurnBudget.cs
- Tests/Combat/DungeonCombatTests.cs
- Tests/Combat/DungeonBudgetTests.cs
- Skills/DungeonSkillCatalog.cs
- Skills/DungeonSkillSession.cs
- Skills/DungeonHeroProfiles.cs
- Tests/Skills/DungeonSkillTests.cs
- Tests/Skills/DungeonProfileTests.cs

Math/skills próprios, asmdefs sem dependência Runtime/UI; não converter nem sobrescrever atributos da Arena. Novos heróis são definições numéricas/protótipos; modelos, VFX/SFX e HUD final dependem de 04–06. Caps obrigatórios mesmo quando módulos da campanha forem desligados.
