# Status da equipe de balanceamento

Arquivo lido pela tarefa agendada "equipe-balanceamento-retomar". A tarefa **só age se `sessao: ativa` e o trabalho parou no meio** (por exemplo, limite de uso). Ela nunca começa trabalho novo.

sessao: finalizada
etapa: balanceador
rodada: 6
variante: BAL
objetivos: sim/objetivos-bal6b.json
atualizado: 2026-10-03 (autor aprovou a rodada 6 e a BAL virou PADRAO; próximo projeto: Conjuradores por par de Valores)

## Sessão (só o autor muda)
- `ativa`: o autor mandou a equipe trabalhar ("preciso da minha equipe de balanceamento"). Se o trabalho for interrompido, a tarefa agendada retoma.
- `pausada`: o autor mandou pausar. Ninguém continua até ele mandar retomar.
- `finalizada`: o autor encerrou o trabalho. Ninguém continua; um projeto novo só começa quando ele pedir.
- `aguardando-autor`: a equipe entregou um diagnóstico que precisa da avaliação do autor. Ninguém continua até ele responder.

## Etapas (dentro de uma sessão ativa)
- `testes`: agente-de-testes ajusta baralhos/estilos com `node sim/agente.ts --objetivos <objetivos>` (retoma de `agente/<objetivos>/estado.json`).
- `balanceador`: agente-balanceador muda cartas/efeitos/estratégias na variante BAL e mede com `node sim/avaliar.ts --variante BAL`.

## Histórico
- 2026-10-03: o autor mandou a equipe trabalhar na rodada 6. Balanceador implementou a rodada 6 (BAL 17/22). Agente de testes acionado com sim/objetivos-bal6.json.
- 2026-10-03 04:47: agente de testes ATINGIU e confirmou sim/objetivos-bal6.json (3173 tentativas, 240 partidas por confronto). avaliar BAL + baralhos do agente (40 partidas): 20/24 critérios (antes 19/24); falham Eternyr 59,5% (teto 59%), Controle ganha em recurso 51,6% (55%), Aggro mão 3,31 (2,5), Midrange × Controle 48,8% (50%). Snapshot: sim/baralhos-bal6-agente.json; relatório docs/AVALIACAO-BAL-testes.md.
- 2026-10-03 04:57: balanceador incorporou os baralhos do agente (passo 8, 20/24) e corrigiu a identidade. O Aggro volta à lista aprovada do PADRAO (o do agente usava cartas lentas; trocadas por cartas baratas, ele daria 65,8%). O Eternyr fica sem Mercador e Dragão (22/24 com 30 partidas; com 60, o Controle do agente só tem 46% em recurso). O Controle volta a ser o Controle de remoção, com Cura que compra e 2 Presa-trovão (69% em recurso, 48% contra o Aggro, mas 67% de média). BAL = BAL6k, 20/24. Novo pedido ao agente de testes: sim/objetivos-bal6b.json (Controle, Midrange e Tempo; o Aggro fica fixo). Etapa: testes.
- 2026-10-03 06:04: agente de testes ATINGIU e confirmou sim/objetivos-bal6b.json (1653 tentativas, 240 partidas por confronto). avaliar BAL + baralhos do agente (40 partidas): 21/24; falham Aggro 59,2% (teto 59%), duração 27,6 turnos (16 a 26), Controle ganha em recurso 51,8% (55%). Snapshot: sim/baralhos-bal6b-agente.json; relatório docs/AVALIACAO-BAL-testes2.md. Etapa passa ao balanceador.
- 2026-10-03 06:11: balanceador incorporou o 2º ajuste do agente (passo 12) e fechou com passos 13 a 15: Controle em agressividade 0,3 (a duração cai de 27,5 para 25,7), 2 Estruturas de compra e 4 'Eternyr: comprar 3' no lugar de Aliados de ataque, dano ao Conjurador e 1 Fera (recurso 50% para 57%). BAL = BAL6o: PRÓXIMO, 21/24 com 60 e com 80 partidas. Faltam, no limite: duração 26,1 a 26,5, Midrange × Aggro 48%, Stall por dano 73–75%. Diagnóstico: docs/DIAGNOSTICO-BALANCEADOR.md. Sessão: aguardando-autor.
