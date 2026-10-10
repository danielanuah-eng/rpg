# Fase 4 — WebSocket

Registro histórico. A cena e os scripts atuais foram consolidados na [Fase 5](FASE-5.md), com duas peças e propriedade por jogador.

## Configuração

- Autoload `TurnManager`: `res://scripts/TurnManager.gd`.
- Autoload `NetworkManager`: `res://scripts/NetworkManager.gd`, após TurnManager.
- Cena principal: `res://scenes/arena_interativa.tscn`.
- Raiz `ArenaInterativa`: `PlayerMovement.gd`, que estende a composição local `arena_interativa.gd`.
- Preservar os caminhos e nomes dos nós nos dois processos. Autoridade dos nós: peer 1.
- UI temporária criada em execução: `UI/TurnPanel/{TurnState,PassTurn,ServerURL,Host,Join,Leave,Connection}`.
- `MultiplayerSpawner` e `MultiplayerSynchronizer` dispensados: a cena e o bloco são estáticos e os RPCs replicam percurso, posição e turnos. Não há propriedade replicada por dois mecanismos.
- Protótipo com um bloco compartilhado: P1 é o Host; P2 é o primeiro cliente; IA_NEUTRA é controlada temporariamente pelo Host. As peças individuais e decisões automáticas da IA são etapas futuras.
- As duas instalações usam o mesmo tabuleiro estático e regras. Edição de mapa durante a sessão não integra o protocolo desta fase.

## Conexão

1. Na instalação nativa, usar Host (TCP 9080).
2. No segundo processo, usar Join com `ws://127.0.0.1:9080`; em outro dispositivo, substituir pelo IP do Host.
3. No navegador, somente Join; o servidor WebSocket precisa executar nativamente.
4. Para uma página HTTPS, usar `wss://` com certificado válido e terminação TLS no servidor/proxy encaminhando ao Host. O Host local fornecido usa `ws://`.
5. No export Android, habilitar a permissão `INTERNET`.

Referências: [WebSocketMultiplayerPeer](https://docs.godotengine.org/en/stable/classes/class_websocketmultiplayerpeer.html) e [High-level multiplayer](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html).

## Protocolo

- O cliente envia somente destino `{x,z}` e revisão observada.
- O Host identifica o remetente pelo MultiplayerAPI, verifica posse do turno, revisão e coordenadas; MoveAuthority recalcula o AStar e TurnManager valida PM.
- O Host envia o percurso aprovado como `{p:[{x,z},...]}`. Apenas o Host confirma casas e desconta PM. Clientes animam o mesmo percurso.
- Estado compacto `{t,pm}` e revisão sincronizados por RPC confiável. Estado enviado por cliente é recusado, mesmo que tenha formato válido.
- Ao finalizar, o snapshot `{x,z}` reconcilia a posição visual e lógica do cliente.
- Entrada durante movimento recebe o snapshot ao fim do percurso, antes de permitir comandos.
- Desconexão encerra a sessão e reinicia o protótipo local; reconectar cria outra sessão.

## Validação

Godot 4.6.1: Fase 1 (815 verificações), Fase 2 (47), Fase 3 (25), Fase 4 (19 no Host e 17 no cliente). Total: 923 verificações, zero falhas.

`tests/test_network.gd` roda em dois processos nativos com argumentos `-- --host` e `-- --client`, usando TCP 19084. Exercita movimento dos dois jogadores, limites AStar/PM, remetente inválido, revisão antiga, falsificação de estado, passagem de turno, desconexão e entrada tardia. Usar `--headless --log-file ./arquivo.log --script res://tests/test_network.gd`.

O teste de rede requer acesso ao loopback; o sandbox sem rede pode produzir timeout. HTML5/Android não foram executados nesta validação nativa.
