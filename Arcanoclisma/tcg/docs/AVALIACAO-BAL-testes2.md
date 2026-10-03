# Avaliação de balanceamento: Balanceador rodada 6 (varreduras, curva de Grau, unidades-problema e bombas) + baralhos de agente/objetivos-bal6b/estado.json

3240 partidas (40 por confronto ordenado), 3233 decididas. Turnos médios 27.6. Critérios em sim/criterios.json.

**Situação: LONGE** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 59.2% | ≤ 59.0% | 0.2 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 27.6 turnos | 16 a 26 | 6.0% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.4% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 40.0% (n=80) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 59.0% em 476 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 51.8% de 485 partidas (diferença média +0.7 cartas) | ≥ 55.0% | 3.2 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 46.6 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.59 (3874 × 6593 em 535 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.28 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 8.6% em 35 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 60.0% (n=80) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.9% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 62.5% (n=80) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 50.0% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 53.8% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.20 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 75.6% de 283 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 65.0% (n=80) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 83.9% em 62 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 21.8% em 262 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 74.2% em 636 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 59.2% | 56.1% | 43.9% | 48.1% | 44.2% | 44.1% | 51.8% | 45.1% | 57.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 50.0% | 60.0% | 96.3% | 35.0% | 66.3% | 57.5% | 41.3% | 67.5% |
| **Midrange** | 50.0% | — | 62.5% | 62.5% | 83.8% | 40.0% | 61.3% | 55.0% | 34.2% |
| **Controle** | 40.0% | 37.5% | — | 13.8% | 65.0% | 62.5% | 46.3% | 60.0% | 26.3% |
| **Burn** | 3.8% | 37.5% | 86.3% | — | 18.8% | 77.5% | 41.3% | 53.8% | 66.3% |
| **Stall** | 65.0% | 16.3% | 35.0% | 81.3% | — | 50.0% | 41.3% | 20.0% | 45.0% |
| **Ramp** | 33.8% | 60.0% | 37.5% | 22.5% | 50.0% | — | 41.3% | 81.3% | 26.9% |
| **Tempo** | 42.5% | 38.8% | 53.8% | 58.8% | 58.8% | 58.8% | — | 45.6% | 57.5% |
| **Mistura** | 58.8% | 45.0% | 40.0% | 46.3% | 80.0% | 18.8% | 54.4% | — | 17.5% |
| **Eternyr** | 32.5% | 65.8% | 73.8% | 33.8% | 55.0% | 73.1% | 42.5% | 82.5% | — |
