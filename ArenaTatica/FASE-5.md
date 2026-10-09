# Fase 5 — duas peças e RPCs consolidados

Implementado em Godot 4.6.1. A cena atual usa `PlayerMovement.gd` diretamente, sem herdar o controlador de uma peça da Fase 3. `grid_mover.gd` continua sendo o executor visual da Fase 2; `move_authority.gd` recalcula o AStar e valida a ocupação e o dono de cada peça.

## Scripts finais

- [PlayerMovement.gd](scripts/PlayerMovement.gd): composição completa de input, seleção, duas peças, UI e RPCs de pedido, percurso, snapshot e rejeição.
- [TurnManager.gd](scripts/TurnManager.gd): estado compacto `{t,pm}`, revisão, limite de três casas e RPCs de passagem e sincronização do turno.
- [move_authority.gd](scripts/move_authority.gd): registro do dono por unidade e rejeição de ator incompatível, além das validações de percurso já existentes.
- [NetworkManager.gd](scripts/NetworkManager.gd): transporte WebSocket e atribuição de P1 ao Host e P2 ao cliente, preservados da Fase 4.

## Configuração na interface do Godot

1. Abra `project.godot` e a cena `scenes/arena_interativa.tscn`.
2. Em **Projeto → Configurações do Projeto → Globais → Autoload**, mantenha, nesta ordem:
   - `res://scripts/TurnManager.gd`, nome `TurnManager`, ativado.
   - `res://scripts/NetworkManager.gd`, nome `NetworkManager`, ativado.
3. Selecione `ArenaInterativa`: no Inspector, **Script** deve ser `res://scripts/PlayerMovement.gd`.
4. Na raiz, `Player1` e `Player2` são `Node3D`, ambos com **Script** `res://scripts/grid_mover.gd`. Cada um tem um filho `Visual` (`MeshInstance3D`). P1 é laranja e P2 é azul.
5. No Inspector de `ArenaInterativa`, **Spawn Cell** é `(1,1)` e **Spawn Cell 2** é `(10,10)`. As duas casas precisam estar livres; o script procura alternativas se necessário.
6. Não adicione `MultiplayerSpawner` ou `MultiplayerSynchronizer` nesta configuração. As duas peças já estão salvas na mesma cena em ambos os processos. Os RPCs replicam os percursos e reconciliam posições e turnos. Não configure replicação de `position`/`global_position` em paralelo ao Tween.
7. A autoridade de rede de toda a cena permanece **1 (Host)**, inclusive de `Player2`. A propriedade de jogabilidade é validada no servidor; não transfira a autoridade do Node `Player2` ao cliente.

Hierarquia relevante:

```text
ArenaInterativa (PlayerMovement.gd)
├── Arena
│   ├── Board
│   └── Camera3D
├── Player1 (grid_mover.gd)
│   └── Visual
├── Player2 (grid_mover.gd)
│   └── Visual
├── Destination
├── GridPicker
├── MoveAuthority
└── UI
    ├── InfoPanel
    ├── StatusPanel
    └── TurnPanel (criado em execução)
        ├── TurnState
        ├── PassTurn
        ├── ServerURL
        ├── Host
        ├── Join
        ├── Leave
        └── Connection
```

`MultiplayerSpawner` replica criação e remoção de Nodes; `MultiplayerSynchronizer` replica propriedades configuradas. A solução desta fase usa cena estática e RPCs, portanto não exige nenhum desses Nodes. Referências: [Spawner](https://docs.godotengine.org/en/stable/classes/class_multiplayerspawner.html) e [Synchronizer](https://docs.godotengine.org/en/stable/classes/class_multiplayersynchronizer.html).

## Protocolo e propriedade

- `player_1` pertence a P1; `player_2` pertence a P2.
- O pedido contém ID da peça, destino `{x,z}` e revisão observada. O ID não é prova de posse: o Host obtém o remetente via `multiplayer.get_remote_sender_id()` e verifica o dono cadastrado.
- O Host rejeita peça alheia, turno incorreto, revisão antiga, destino inválido/ocupado, outro movimento em andamento e custo AStar acima dos PM.
- Somente o Host aprova e confirma os passos. O percurso aprovado inclui ID da peça, casas e duração por casa. Ambos os processos animam a peça indicada; a outra permanece em sua posição.
- Estado de turno `{t,pm}` e revisão são publicados por RPC confiável. Alterações enviadas por clientes são recusadas.
- O snapshot final inclui ambas as posições, o turno e a revisão. Um cliente que entra durante movimento recebe esse snapshot ao final do percurso.
- `IA_NEUTRA` continua no ciclo, sem peça neutra nesta fase. O Host usa **Passar Turno** para encerrá-la. Peças humanas não se movem no turno neutro.
- As animações reproduzem o mesmo percurso nos dois processos; o início no cliente está sujeito à latência da rede. Não há sincronização de relógios para simultaneidade de quadro.
- Desconectar encerra a sessão e reinicia o modo local. Reconectar inicia uma nova sessão; não há retomada de uma partida anterior.

## QA executado

| Verificação | Total | Falhas |
|---|---:|---:|
| Tabuleiro / Fase 1 | 815 | 0 |
| Movimento / Fase 2 | 47 | 0 |
| Turnos / Fase 3 | 25 | 0 |
| Propriedade e ocupação / Fase 5 | 15 | 0 |
| WebSocket Host / Fase 5 | 27 | 0 |
| WebSocket cliente / Fase 5 | 23 | 0 |
| **Total** | **952** | **0** |

Consola: nenhum `SCRIPT ERROR` ou `ERROR` nos logs da validação final. Rede validada com dois processos Godot nativos em loopback TCP 19084. Os testes cobrem clique em peça adversária, RPC forjado para peça adversária (inclusive no próprio turno), falsificação de estado, PM, ocupação, AStar, passagem de turno, animação de ambas as peças, desconexão e snapshot tardio das duas posições. Os testes locais também exercitam interrupção e recuperação de movimentos.

Logs locais: `phase5-validation.log`, `run_tests-phase5.log`, `test_movement-phase5.log`, `test_turns-phase5.log`, `ownership-phase5.log`, `network-host-phase5.log` e `network-client-phase5.log`.

Prévia gráfica do próprio motor: [.artifacts/phase5.png](.artifacts/phase5.png). A captura final terminou com `FASE 5 PREVIA: OK`, sem erros. A primeira tentativa dentro do sandbox acusou `Can't create shader cache folder`; repetir com acesso ao cache do usuário resolveu esse erro de ambiente, sem alteração dos scripts.

## Conferência manual

1. Rode duas instâncias nativas. Na primeira, clique **Host**; na segunda, **Join** com `ws://127.0.0.1:9080`.
2. No Host, mova a peça laranja até três casas; confira o mesmo percurso nos dois ecrãs.
3. No cliente, clique na peça laranja: deve aparecer a mensagem de peça pertencente ao outro jogador.
4. No turno P2, mova a peça azul; confira PM e posições nos dois ecrãs. No Host, clicar na peça azul também deve ser rejeitado.
5. Use **Passar Turno** e confira o ciclo `P1 → P2 → IA_NEUTRA → P1`. Somente o Host encerra a fase neutra.

HTML5 e Android não foram executados nesta validação. As restrições WebSocket de exportação e a permissão Android INTERNET continuam descritas na Fase 4.
