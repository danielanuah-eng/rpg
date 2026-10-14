# Lote 020 — controles, neblina e modificadores (14/10/2026)
Autorização: pedido atual do autor; atributos aleatórios numéricos limitados. Codex implementa ArenaTatica conforme exceção de 13/10. Antes de editar C#, ficam reservados:
- Core/CoreTypes.cs, Core/Board.cs, Core/Content.cs, Core/Match.cs
- ArenaGame.cs, ArenaMenu.cs, ArenaProtocol.cs, ArenaNetwork.cs, ArenaSmoke.cs
- ArenaPresentation.cs, ArenaHud.cs, ArenaActionConfirmation.cs, ArenaTouchGesture.cs
- Tests/Editor/CoreMatchTests.cs, RoomReleaseTests.cs, PresentationTests.cs, MatchModifierTests.cs, MatchInteractionTests.cs, ActionConfirmationTests.cs
Arquivos sob ArenaTaticaUnity/Assets/ArenaTatica/Scripts, salvo Tests sob Assets/ArenaTatica/Tests/Editor.
DeepSeek: apenas ArenaPresentation.cs, ArenaHud.cs, ArenaActionConfirmation.cs, ArenaTouchGesture.cs e seus testes ActionConfirmationTests.cs em worktree isolada; Codex revisará diff e rodará os testes. Outros arquivos permanecem sob Codex. Preservar mudanças concorrentes e reservas de artes PixelLab. Novos efeitos somente após publicar este lote de código.

Reserva complementar antes de editar: ArenaTaticaUnity/Assets/Editor/ArenaBuild.cs (versão 1.2.0/código Android 3); Core/Bot.cs somente se testes detectarem incompatibilidade.

DeepSeek entregou 8 arquivos em worktree; Codex revisou o diff antes de integrar. Pendências de testes, UI ao vivo, resets de confirmação e identidade numérica serão resolvidas pelo Codex. Nenhum arquivo fora da reserva foi integrado.

140/140 testes aprovados na revisão bea26431; player, contrato LAN, salas/torneios 2/3/6 e Relay Windows real aprovados. DeepSeek conferiu as quatro capturas e não encontrou corte bloqueante. Dúvidas sobre 24 lutadores e rótulos na imagem rolada resolvidas pelo runner (contagem >=24 e rolagem ao fim) e pelas capturas de início da fila. Android físico ausente. APK e publicação ainda em andamento; novas chamadas de animação somente depois do deploy.

Publicação 1.2.0 verificada: Windows 2127612 e Android 2127611 COMPLETED. Os downloads remotos correspondem integralmente (248 arquivos Windows; SHA-256 APK 9cd66dfc6e803ecbdd32218f53aaaf12970951f3f3d88e9dc8948a8294a0e11a). Página pública confirma ambas as versões. Itens 1–3 concluídos; fase de efeitos pode iniciar após este deploy.
