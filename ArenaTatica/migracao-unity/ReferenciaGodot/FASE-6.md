# Fase 6 — HP, PA e LoS

## Novas dependências

```text
ArenaInterativa
├── Player1
│   └── Stats (Node; scripts/PlayerStats.gd)
├── Player2
│   └── Stats (Node; scripts/PlayerStats.gd)
└── CombatManager (Node; scripts/CombatManager.gd)
```

`CombatManager` é filho da cena, não um Autoload. Usa os Autoloads existentes `TurnManager` e `NetworkManager`. A interface temporária acrescenta somente o botão alternável **Atacar** e o indicador de HP/PA ao `UI/TurnPanel` existente.

Valores iniciais: HP máximo 100; PA máximo 2; dano 15; custo 1 PA. P1 recebe PA no primeiro turno; P2 começa com 0 e recebe seus PA quando seu turno começa. Valores configuráveis no Inspector de cada `Stats` e do `CombatManager`.

LoS matemática na grelha, sem RayCast3D e sem teste contra o corpo do atacante. Células opacas precisam de `blocks_los = true`; o campo `walkable` é independente. Exemplo após gerar o tabuleiro:

```gdscript
$Arena/Board.model.cells[Vector2i(5, 5)]["blocks_los"] = true
```

O percurso verifica todas as casas atravessadas, incluindo as duas vizinhas nos cruzamentos exatos de cantos. A casa de origem é ignorada; casas fora da grelha ou opacas bloqueiam o disparo. Não foi adicionado alcance máximo de ataque nesta fase.

## Integração pontual

- `PlayerMovement._on_move_intent`: três linhas encaminham o clique ao `CombatManager` quando o modo de ataque está ligado. O movimento existente permanece no mesmo módulo.
- `TurnManager`: registro fraco dos Stats, sinal de reinício da partida, sinal de início do turno e restauração autoritativa dos PA. Os pontos de chamada são somente `reset()` e `_advance_turn()`; publicar PM ou receber RPC de estado não restaura PA. O movimento de peça derrotada é recusado por `can_move()`.
- `PlayerStats`: guarda HP/PA, verifica a autoridade em toda mutação do servidor e aceita snapshots no cliente somente dentro de RPC originado pelo peer 1.
- `CombatManager`: valida remetente, dono, turno, movimento em andamento, versões, alvo vivo, custo e LoS. O cliente envia apenas IDs e versões; dano e custo são definidos pelo Host.

Resposta compacta por ataque: `{"atk":1,"tgt":2,"dmg":15,"hp":85,"pa":1,"r":4}`. O campo `r` identifica a revisão dos atributos e impede reutilizar pedidos antigos. Snapshots de atributos são enviados no início do turno e na entrada de clientes; não restauram atributos por conta própria.

## Validação no Godot 4.6.1

| Teste | Verificações | Falhas |
|---|---:|---:|
| Combate local / clique / LoS / HP / PA | 37 | 0 |
| Combate WebSocket Host | 15 | 0 |
| Combate WebSocket cliente | 22 | 0 |
| Regressão de turnos | 25 | 0 |
| Regressão de movimento | 47 | 0 |
| **Total** | **146** | **0** |

Logs finais: `phase6-validation.log`, `test_combat-phase6.log`, `combat-host.log`, `combat-client.log`, `test_turns-phase6.log` e `test_movement-phase6.log`. Nenhum erro de script ou de dependência de Autoload nesses logs.

O teste em rede colocou um obstáculo somente no modelo do Host: o cliente enxergava LoS livre, mas o servidor rejeitou o ataque sem cobrar PA. Também foram validados dono incorreto, ataque fora de turno, pedido repetido, ausência de PA, restauração somente do ativo e snapshot tardio com HP e PA já alterados.

Somente instâncias nativas foram executadas. Fase 7 não iniciada.
