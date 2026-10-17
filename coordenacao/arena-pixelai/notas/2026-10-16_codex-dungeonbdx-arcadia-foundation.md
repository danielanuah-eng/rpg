# Configuração Arcadia Quest — reserva de arquivos

Pedido do autor: seguir Arcadia Quest nas regras, aventuras, mapas, tamanho/posição das miniaturas, habilidades individuais, golpes Arena e seleção de partidas. Exceção confirmada: preservar limites contra combos/críticos excessivos.

Codex assume o trabalho após os três auxiliares atingirem limite. Preservar alterações concorrentes, saves e modo de expedição anterior. Trabalho isolado em `tmp/dungeonbdx-step1-git`, branch `codex/dungeonbdx-arcadia-foundation`.

Arquivos .cs reservados antes da edição:
- Assets/DungeonBDX/Arcadia/AqBoard.cs
- Assets/DungeonBDX/Arcadia/AqContent.cs
- Assets/DungeonBDX/Arcadia/AqSession.cs
- Assets/DungeonBDX/Arcadia/AqJournal.cs
- Assets/DungeonBDX/Runtime/AqBoardView.cs
- Assets/DungeonBDX/Runtime/AqMatch.cs
- Assets/DungeonBDX/Runtime/AqMatchMenu.cs
- Assets/DungeonBDX/Runtime/AqHeroImport.cs
- Assets/DungeonBDX/Runtime/AqLocalSave.cs
- Assets/DungeonBDX/Runtime/DungeonShell.cs
- Assets/DungeonBDX/Runtime/DungeonCameraInput.cs
- Assets/DungeonBDX/Runtime/DungeonActorAnimation.cs
- Assets/DungeonBDX/Runtime/DungeonWorldView.cs
- Assets/DungeonBDX/Tests/Arcadia/AqRulesTests.cs
- Assets/DungeonBDX/Tests/Editor/AqPresentationTests.cs
- Assets/DungeonBDX/Tests/Editor/AqPersistenceTests.cs — reserva do auxiliar aq_save; persistência, CAS e recuperação.
- Assets/DungeonBDX/Editor/AqAssets.cs

DeepSeek pode escrever APENAS AqBoardView.cs na cópia/worktree, após receber contrato. Codex revisa todo diff e executa testes/build/partida real. Referências KrosArcano são lidas localmente e nunca enviadas ao OpenRouter. Demais .cs são de responsabilidade exclusiva do Codex.

Reserva adicional antes da criação: Assets/DungeonBDX/Tests/Arcadia/AqMapTests.cs (agente aq_maps).

Reserva adicional antes da criação: Assets/DungeonBDX/Tests/Arcadia/AqCombatTests.cs (agente aq_combat_tests).
