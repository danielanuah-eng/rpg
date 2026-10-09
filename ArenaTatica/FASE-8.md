# Fase 8 — integrada em 08/10/2026

Abra `ABRIR-GODOT.cmd` e pressione F5. HP/PA aparecem na HUD; o turno indica P1, P2 ou Monstros. Ative **Atacar** para atacar a peça adversária ou um monstro. Use Host/Join para dois jogadores; sem conexão, o controle local alterna entre P1 e P2.

## Integração

- `scenes/UI.tscn` / `scripts/UIManager.gd`: CanvasLayer da cena principal, atualização por sinais locais, barras HP/PA, painel de resultado. Modo compacto abaixo de 1000 pixels de largura.
- `scripts/GameLoopManager.gd`: filho da cena principal, autoridade peer 1. RPCs confiáveis `rpc_remove` e `rpc_result`, com execução local no Host e validação da origem.
- `PlayerStats.gd`: sinais `hp_changed`, `pa_changed` e `died`; dano continua calculado no Host. A Fase 6 mantém a sincronização de atributos.
- `CombatManager.gd`: estende o registro existente para alvos monstros; rejeita ações após o fim.
- `move_authority.gd`: remove ocupação/rotas da entidade, liberando a célula para o AStar existente sem alterar obstáculos do terreno.
- `PlayerMovement.gd`: limpa referências das peças eliminadas, aceita snapshots com sobreviventes e restaura as conexões das peças recriadas.
- `TurnManager.gd`: pula jogadores eliminados e bloqueia ações após o resultado final.
- `AIManager.gd`: reaproveita a entrega validada da Fase 7; limpa referências a monstros eliminados, sincroniza suas posições lógicas e interrompe coroutines ao terminar/reiniciar a partida.

A Fase 7 estava entregue em `entregas/fase7`, mas não ligada à cena principal. Foi integrada com os dois monstros previstos nessa entrega. Cada monstro recebeu `PlayerStats` com 30 HP; IDs de combate 1001/1002. Jogadores mantêm IDs 1/2.

Os nós visuais são removidos com `queue_free()` em todos os peers. O pequeno estado de atributos é preservado no CombatManager para snapshots, HUD pós-morte e entrada tardia. Este projeto não usa MultiplayerSpawner para essas peças.

No PvEvP, morrer apresenta Derrota para aquele jogador. O sobrevivente vence após derrotar o outro jogador e todos os monstros. Se ambos morrerem na mesma resolução, ninguém vence. No modo local compartilhado, o painel final identifica o vencedor. Sair/desconectar restaura o elenco e a HUD.

## Validação em Godot 4.6.1

| Bateria | Verificações | Falhas |
| --- | ---: | ---: |
| Grade/base | 815 | 0 |
| Movimento e entrada | 47 | 0 |
| Combate | 37 | 0 |
| Fase 8 local | 24 | 0 |
| Fase 8 Host | 13 | 0 |
| Fase 8 cliente | 18 | 0 |
| Total | 954 | 0 |

Inclui mortes simultâneas, morte durante a IA, célula liberada, reinício com peças recriadas, bloqueio após o fim, origem autoritativa, derrota individual, rodada PvEvP sincronizada e entrada tardia após a vitória.

Regressões de movimento isolam a ocupação dos monstros; o teste de combate aguarda o término automático da IA da Fase 7.

Testes da Fase 8:

```powershell
& ./.tools/godot/Godot_v4.6.1-stable_win64_console.exe --headless --path . --log-file phase8-local.log --script res://tests/test_phase8.gd
& ./tests/run_phase8_network.ps1
```

Capturas verificadas: `.artifacts/phase8-1280.png`, `.artifacts/phase8-800.png`, `.artifacts/phase8-victory.png`. A captura gráfica no sandbox avisou que não podia criar o cache de shaders em `user://`; os PNGs foram gerados. Nenhum erro de script ou de liberação de nó permaneceu nos testes aprovados.

Originais dos arquivos existentes modificados na integração: `.tools/fase8-original/`. A entrega original da Fase 7 permanece em `entregas/fase7`.
