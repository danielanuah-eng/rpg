# DungeonBDX — reserva de código

Autor: pedido UltraBridge de 15/10/2026. Codex executa Step 1 em `tmp/dungeonbdx-step1-git`, branch `codex/dungeonbdx-step1`.

Arquivos C# novos reservados, antes da edição:
- `ArenaTaticaUnity/Assets/DungeonBDX/Runtime/DungeonHeroCatalog.cs`
- `ArenaTaticaUnity/Assets/DungeonBDX/Runtime/DungeonSetup.cs`
- `ArenaTaticaUnity/Assets/DungeonBDX/Runtime/DungeonMotion.cs`
- `ArenaTaticaUnity/Assets/DungeonBDX/Runtime/DungeonShell.cs`
- `ArenaTaticaUnity/Assets/DungeonBDX/Editor/DungeonBuild.cs`
- `ArenaTaticaUnity/Assets/DungeonBDX/Tests/Editor/DungeonSetupTests.cs`

Integração aditiva; cena própria, sem editar motor, personagens, menu ou cena da Arena. Validação posterior pela CLI oficial Unity; usa cache existente do projeto para não duplicar Library (disco com cerca de 5 GB livres). Ponte: `coordenacao/dungeonbdx`.
