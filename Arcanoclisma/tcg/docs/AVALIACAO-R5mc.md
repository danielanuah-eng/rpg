# Avaliação de balanceamento: R5m + Controle de remoção

2430 partidas (30 por confronto ordenado), 2419 decididas. Turnos médios 23.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 16 de 22 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 62.1% | ≤ 59.0% | 3.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 33.4% | ≥ 38.0% | 4.6 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 23.5 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.5% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 57.0% | ≥ 45.0% e ≤ 55.0% | 2.0 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 26.7% (n=60) | ≥ 35.0% | 8.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 68.2% em 337 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 66.3% de 365 partidas (diferença média +1.7 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 30.2 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.40 (1475 × 3659 em 413 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 1.94 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 31.8% em 22 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 73.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 83.2% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 46.7% (n=60) | ≥ 50.0% | 3.3 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 70.0% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 30.0% (n=60) | ≥ 50.0% | 20.0 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.45 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 80.2% de 202 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 56.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 82.9% em 41 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 62.1% | 54.4% | 51.7% | 47.3% | 42.2% | 51.5% | 33.4% | 51.8% | 55.7% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 30.0% | 73.3% | 91.7% | 43.3% | 55.0% | 88.3% | 40.0% | 75.0% |
| **Midrange** | 70.0% | — | 46.7% | 51.7% | 71.7% | 43.3% | 70.0% | 56.7% | 25.0% |
| **Controle** | 26.7% | 53.3% | — | 20.3% | 76.3% | 61.7% | 70.0% | 63.3% | 41.7% |
| **Burn** | 8.3% | 48.3% | 79.7% | — | 26.7% | 45.0% | 69.5% | 36.7% | 64.4% |
| **Stall** | 56.7% | 28.3% | 23.7% | 73.3% | — | 16.7% | 58.3% | 32.2% | 48.3% |
| **Ramp** | 45.0% | 56.7% | 38.3% | 55.0% | 83.3% | — | 48.3% | 65.0% | 20.3% |
| **Tempo** | 11.7% | 30.0% | 30.0% | 30.5% | 41.7% | 51.7% | — | 18.3% | 53.3% |
| **Mistura** | 60.0% | 43.3% | 36.7% | 63.3% | 67.8% | 35.0% | 81.7% | — | 26.7% |
| **Eternyr** | 25.0% | 75.0% | 58.3% | 35.6% | 51.7% | 79.7% | 46.7% | 73.3% | — |
