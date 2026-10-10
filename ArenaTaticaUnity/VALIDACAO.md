# Validação da migração — 09/10/2026

Projeto: Unity 6000.6.4f1, URP, cena `Assets/ArenaTatica/Scenes/Arena.unity`.

## Resultados confirmados

- Importação dos três modelos GLB e criação da cena pelo Editor concluídas.
- 26 testes EditMode aprovados, zero falhas (`Artifacts/editmode.xml`).
- Build Windows executada: partida local e partida com Host/cliente aprovadas pelo teste automático.
- Contrato WebSocket com cliente JavaScript: sete cenários aprovados (autoridade, revisão antiga, movimento, morte, reconexão, ausência de repetição de efeitos e rejeição de estado forjado).
- Captura gráfica real do Windows conferida em `Documentacao/arena-unity.png`. Capturas de processos em batch podem ser pretas e não são usadas como prova visual.
- HUD vertical conferida em `Documentacao/arena-unity-vertical.png`; câmera de fundo limpa a tela inteira e elimina rastros dos números. A build com essa correção passou novamente nos testes local, Host e cliente.
- Build Web compilada e carregada no navegador integrado do Codex. Confirmados tabuleiro, miniaturas, HUD, botão Encerrar turno, sequência P1 → P2 → dragões → rodada 2 e dano automático de 25 HP ao P1.
- Cliente Web conectado ao Host Windows e snapshot inicial confirmado pela HUD como jogador 2.
- Módulo Android, SDK, NDK e JDK instalados pela ferramenta oficial do Unity.

## Etapas finais em execução

- Concluir a build Web com a limpeza da tela completa para a HUD vertical.
- Gerar APK Android. Teste físico em celular ainda não realizado.

O Android recusou o caminho original com acentos. `Tools/build-android.ps1` exporta a partir de uma cópia temporária ASCII e traz o APK para Builds/Android; o projeto original permanece no lugar.

## Limites da verificação

A arte preserva os modelos do projeto original; sua aprovação estética continua pendente. Não houve publicação na internet, teste entre redes externas ou avaliação de desempenho em aparelhos Android. O multiplayer é uma sessão de duas pessoas com autoridade do Host; não inclui lobby público, contas ou infraestrutura de matchmaking.

Os logs e caches de `Artifacts`, `Library` e `Logs` são locais e não devem acompanhar distribuições. O projeto editável é formado por Assets (com `.meta`), Packages e ProjectSettings. As builds ficam em Builds.
