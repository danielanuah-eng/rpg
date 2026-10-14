# 019 — reserva de execução Sol

Recebido o repasse Astra para fechar a versão. Produção de personagens Blender pausada. Astra reserva apenas seus documentos de auditoria e repasse.

Sol reserva antes de editar:

- `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Match.cs`
- `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/CoreTypes.cs`
- `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/ArenaGame.cs`
- `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/ArenaPresentation.cs`
- `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/ArenaSmoke.cs`
- `ArenaTaticaUnity/Assets/ArenaTatica/Tests/Editor/CoreMatchTests.cs`
- `ArenaTaticaUnity/Assets/ArenaTatica/Tests/Editor/RoomReleaseTests.cs` (novo, se necessário)
- `ArenaTaticaUnity/Assets/Editor/ArenaBuild.cs`

Também sob execução: scripts `Tools/package.ps1`, build Windows/Android, verificações de rede/sala, documentação e identificação desta entrega. Novos `.cs` adicionais serão registrados antes de edição. Alterações prévias de mapas/metadados, analytics e arquivos de conta Unity são preservadas e não incluídas automaticamente. DeepSeek somente leitura, limitado ao código ArenaTaticaUnity, sem logs de conta; decisões de protocolo e revisão pelo Codex. Builds/testes sequenciais. Resultado em `respostas/019-sol.md`.

Reserva adicional antes de editar: ArenaTaticaUnity/Assets/ArenaTatica/Scripts/ArenaTransport.cs. O teste de times completos reproduziu snapshot de seis participantes acima dos 48 KiB do Relay; ajustar capacidade do transporte, sem mudar regras.
Capacidade final: 80 KiB (81.920 bytes), abaixo do máximo confiável de aproximadamente 88 KiB documentado no Unity Transport instalado, janela 64. Maior snapshot medido: 57.347 bytes (6 participantes, 4 lutadores/time).

Reserva adicional antes de editar: ArenaTaticaUnity/Assets/ArenaTatica/Scripts/ArenaMenu.cs. Pedido atual do autor: conferir aviso de artes Damian/Tchu Tcha. Remover aviso genérico obsoleto da tarefa 015 e corrigir retorno de forma com clipe reverso existente, sem criar ou alterar imagens.

Reteste no player: o smoke antigo assume Ciantia única e turnos completos, mas lê seleção, fichas e turnos persistidos. Correção reservada em ArenaGame.cs/ArenaSmoke.cs: fixture somente no player batch com --arena-smoke, em memória, sem gravar preferências; jogo normal preservado. Recompilar e repetir validação antes do upload.

Captura de interface do batch confirmada preta. Tools/release-network.mjs executará o cenário de sala com dois participantes em renderização normal e flag explícita --arena-qa-render; ArenaGame/ArenaSmoke usam a mesma fixture somente para essa flag combinada com --arena-smoke. As demais verificações seguem batch. Nenhum controle de aplicativo externo.
