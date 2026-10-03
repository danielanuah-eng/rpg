# Avaliação de balanceamento: Balanceador rodada 6 (varreduras, curva de Grau, unidades-problema e bombas) + baralhos de agente/objetivos-bal6/estado.json

3240 partidas (40 por confronto ordenado), 3232 decididas. Turnos médios 25.1. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Eternyr 59.5% | ≤ 59.0% | 0.5 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.1 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.2% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 43.8% (n=80) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 55.8% em 443 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 51.6% de 461 partidas (diferença média +0.7 cartas) | ≥ 55.0% | 3.4 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 27.6 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.68 (2946 × 4361 em 533 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 3.31 cartas | ≤ 2.5 | 32.3% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 15.6% em 32 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 56.3% (n=80) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 85.4% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 48.8% (n=80) | ≥ 50.0% | 1.3 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 70.0% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 72.5% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.91 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 76.0% de 312 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 56.3% (n=80) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 73.8% em 61 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 18.4% em 244 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 79.3% em 566 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 48.9% | 52.1% | 42.7% | 49.8% | 48.9% | 50.4% | 51.9% | 45.8% | 59.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 30.0% | 56.3% | 82.5% | 43.8% | 53.8% | 43.8% | 40.0% | 41.3% |
| **Midrange** | 70.0% | — | 48.8% | 73.8% | 62.0% | 33.8% | 53.8% | 31.3% | 43.8% |
| **Controle** | 43.8% | 51.2% | — | 42.5% | 52.5% | 40.0% | 27.5% | 62.5% | 21.3% |
| **Burn** | 17.5% | 26.3% | 57.5% | — | 18.8% | 77.5% | 70.0% | 53.8% | 77.2% |
| **Stall** | 56.3% | 38.0% | 47.5% | 81.3% | — | 50.0% | 55.0% | 20.0% | 43.6% |
| **Ramp** | 46.3% | 66.3% | 60.0% | 22.5% | 50.0% | — | 42.5% | 81.3% | 34.2% |
| **Tempo** | 56.3% | 46.3% | 72.5% | 30.0% | 45.0% | 57.5% | — | 57.5% | 50.0% |
| **Mistura** | 60.0% | 68.8% | 37.5% | 46.3% | 80.0% | 18.8% | 42.5% | — | 12.5% |
| **Eternyr** | 58.8% | 56.3% | 78.8% | 22.8% | 56.4% | 65.8% | 50.0% | 87.5% | — |
