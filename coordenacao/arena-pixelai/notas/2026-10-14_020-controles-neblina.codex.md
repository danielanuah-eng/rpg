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

Reserva para a etapa posterior ao deploy 1.2.0: ArenaTaticaUnity/Assets/ArenaTatica/Scripts/ArenaEfeitos.cs; ArenaPresentation.cs (já reservado); Assets/ArenaTatica/Tests/Editor/PresentationTests.cs (já reservado). Responsabilidade Codex: resolver chamadas de fases das skills atuais e variantes copiadas/sorteadas. DeepSeek somente leitura. Não alterar PNGs, catálogo da produção visual ou arquivos reservados pela frente Sol.

Etapa de efeitos após deploy: 11 fases existentes ligadas a forma, esconder, armadilha, invocação/cura de totem, zona e compra de Item/Buff. 154/154 EditMode em 14/10 às 19:09 UTC. DeepSeek confirmou caminhos e flags no manifesto formatado. Dúvida sobre fumaça resolvida em Core/Match.cs EventsFor: esconder inclui o ator no conjunto oculto antes de descartar seus eventos ao inimigo; armadilha-posta também é exclusiva dos aliados. Não criar estado contínuo artificial para aura-gatinho: skill é cura pontual. Regra e PNGs preservados. Verificar 11 fases no player e builds antes de publicar 1.2.1/Android4.
