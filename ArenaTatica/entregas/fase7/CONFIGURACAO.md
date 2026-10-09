```text
ArenaInterativa (PlayerMovement.gd existente)
├── Arena/Board                 # Fase 2 existente
├── MoveAuthority               # Fase 2 existente
├── CombatManager               # Fase 6 existente
├── Player1/Stats               # Fase 6 existente
├── Player2/Stats               # Fase 6 existente
├── Monsters (Node3D)
│   ├── Monster1 (Node3D, res://scripts/grid_mover.gd)
│   │   └── Visual (MeshInstance3D)
│   └── Monster2 (Node3D, res://scripts/grid_mover.gd)
│       └── Visual (MeshInstance3D)
└── AIManager (Node, res://scripts/AIManager.gd)
```

```ini
# Copiar AIManager.gd desta entrega para res://scripts/AIManager.gd.
# Aplicar TurnManager-bloco.gd.txt ao autoload TurnManager existente.
# Mesmos nomes, IDs, metadados e caminhos de nós em todos os peers.
# Autoridade multiplayer de AIManager e Monsters: peer 1 (Host).
# Visual.scale inicial: Vector3(1, 1, 1); ajustar tamanho no mesh.
# Inspector > Metadata de cada Monster, tipos explícitos:

[Monster1.Metadata]
id = 1                         # int, único
spawn_cell = Vector2i(1, 2)     # Vector2i, casa livre e caminhável
pm = 3                         # int, passos por fase
range = 1                      # int, alcance Manhattan
damage = 10                    # int, dano antes do limite por HP

[Monster2.Metadata]
id = 2
spawn_cell = Vector2i(4, 1)
pm = 3
range = 1
damage = 10

# Não adicionar os monstros a PlayerMovement.units: essa tabela é de P1/P2.
# AIManager registra monster_<id> na ocupação da autoridade da Fase 2.
# RPC: reliable, canal 0. Um pacote por monstro, incluindo caminho e dano.
# ACK: somente conclusão visual; decisões e HP permanecem no Host.
# Falha visual: suspende IA_NEUTRA e registra push_error na consola.
# Validação Godot 4.6.1: sintaxe, teste local e RPC Host/cliente aprovados.
# Teste RPC inclui cliente com movimento mais lento que o Host.
```
