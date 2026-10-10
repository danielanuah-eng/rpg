# Referência de implementação — 09/10/2026

Investigação somente leitura pelo DeepSeek via OpenRouter, consolidada pelo Codex. A validação atual abaixo foi executada pelo Codex; números dos documentos antigos não representam a suíte atual.

| Regra atual | Fonte na cópia ReferenciaGodot |
|---|---|
| Cena principal usa PlayerMovement; 3 autoloads | `project.godot:14`, `scenes/arena_interativa.tscn:58` |
| Mapa 24 × 24; dimensão padrão do script 12 × 12 | `scenes/arena.tscn:18`, `scripts/board.gd:10` |
| Célula 1 unidade, altura do piso 0,25; topo Y=0 | `scripts/board.gd:12`, `scripts/board.gd:83` |
| Movimento: 3 PM por turno, custo por casa, sem diagonais | `scripts/TurnManager.gd:9`, `scripts/TurnManager.gd:96`, `scripts/grid_navigation.gd:19` |
| Animação de passo 0,28 s, SINE/IN_OUT | `scripts/grid_mover.gd:10`, `scripts/grid_mover.gd:85` |
| Jogadores: HP máximo 100, PA máximo 2 | `scripts/PlayerStats.gd:10` |
| Ataque: 15 de dano, 1 PA; sem alcance máximo | `scripts/CombatManager.gd:8`, `scripts/CombatManager.gd:160` |
| Linha de visão lógica por supercover; cantos fechados bloqueiam | `scripts/CombatManager.gd:174` |
| Turnos P1 → P2 → IA_NEUTRA; pula eliminados | `scripts/TurnManager.gd:8`, `scripts/TurnManager.gd:191` |
| Spawns dos jogadores (1,1) e (22,22) | `scripts/PlayerMovement.gd:12`, `scenes/arena_interativa.tscn:59` |
| Monstros: 30 HP; metadados controlam PM, alcance e dano | `scenes/arena_interativa.tscn:171`, `scripts/AIManager.gd:125` |
| IDs jogadores 1/2; monstros 1001/1002 | `FASE-8.md:16`, `scripts/GameLoopManager.gd:34` |
| Alturas visuais jogadores 1,6; monstros 1,0 e 2,2 | `scenes/arena_interativa.tscn:67`, `scenes/arena_interativa.tscn:171` |
| Frente local dos modelos +Z | `scripts/ChibiModels.gd:2` |
| Câmera ortográfica, margem 1,38 e altura extra 1,2 | `scripts/arena.gd:30`, `scenes/arena_interativa.tscn:62` |
| Servidor peer 1, WebSocket TCP 9080; WSS em release | `scripts/NetworkManager.gd:9`, `scripts/NetworkManager.gd:149` |
| Debounce 120 ms por ação/peer | `scripts/NetworkManager.gd:22` |
| RTT: sonda 1 s, timeout 3 s, janela 60 resultados | `scripts/NetworkManager.gd:31` |
| Máximo 12 vozes; volume -12 dB | `scripts/AudioManager.gd:48` |
| Máximo 64 contêineres de VFX | `scripts/VisualFXManager.gd:6` |

## Pendências da investigação conferidas

- Logs antigos de combate e turno apresentam falhas históricas. A bateria atual executada em 09/10 passou: 2.765 verificações, zero falhas, incluindo combate local e pares Host/Client. Os logs atuais estão em `Validacao`; os logs antigos não foram incluídos.
- `scripts/arena_interativa.gd` é uma versão anterior. A cena principal usa `PlayerMovement.gd`; preservar como referência, sem portar o script antigo como entrada ativa. Não foi realizada validação isolada desse script legado.
- `ReferenciaGodot/LEIA-ME.md` e documentos FASE-* contêm descrições e totais históricos; o mapa atual é 24 × 24 e o resumo atual fica em `Validacao/resumo.json`.
- `movement_cost` existe nos dados mas não há aplicação de custo variável por terreno. `step_rule` é um gancho sem atribuição em produção. Não acrescentar essas regras durante a migração.
- `blocks_los` é exercitado nos testes, sem configuração de obstáculos de visão no mapa atual. Preservar o suporte lógico sem inventar obstáculos.
- Alfa/dimensões declarados nos registros de produção não foram medidos novamente nesta preparação; os PNGs originais foram copiados e seus hashes constam no manifesto. Aprovação estética continua pendente.
- GLBs foram exportados e reabertos pelo Godot: Ciantia com 60 malhas; cada dragão com 54. O teste `test_models_3d.gd` passou. Importação e aparência no Unity ainda precisam de validação.
- Não há executável Unity, port C#, teste Unity, cross-play ou builds finais de plataforma nesta entrega. Assinatura Android, infraestrutura WSS e teste em aparelho/navegador ficam na etapa de implementação.
- O runner Godot requer o executável local `.tools/godot`, excluído do pacote. A referência poderá ser aberta por um Godot 4.6.1 instalado separadamente; para o runner, provisionar seu caminho esperado.

Codex preparou exports e documentação. Programação, rede e integração do port continuam com o responsável definido em `COLABORACAO-IAS.md`.
