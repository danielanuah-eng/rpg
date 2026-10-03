# Avaliação de balanceamento: R6 passo 8: + baralhos do agente de testes (sim/baralhos-bal6-agente.json)

2430 partidas (30 por confronto ordenado), 2416 decididas. Turnos médios 25.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Eternyr 62.3% | ≤ 59.0% | 3.3 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.5 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.6% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 44.9% | ≥ 45.0% e ≤ 55.0% | 0.1 pp |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 60.0% (n=60) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 60.5% em 342 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 51.4% de 356 partidas (diferença média +0.6 cartas) | ≥ 55.0% | 3.6 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 27.6 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.66 (2241 × 3394 em 414 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 3.42 cartas | ≤ 2.5 | 37.0% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 5.9% em 34 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 40.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 53.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 73.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.96 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 75.3% de 247 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 71.2% em 52 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 21.5% em 209 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 77.1% em 462 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 47.7% | 54.6% | 47.4% | 47.3% | 51.8% | 49.3% | 46.5% | 43.1% | 62.3% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 26.7% | 40.0% | 83.3% | 38.3% | 48.3% | 50.0% | 50.0% | 45.0% |
| **Midrange** | 73.3% | — | 53.3% | 78.0% | 65.5% | 33.3% | 56.7% | 33.3% | 43.3% |
| **Controle** | 60.0% | 46.7% | — | 40.0% | 45.0% | 49.2% | 48.3% | 73.3% | 16.7% |
| **Burn** | 16.7% | 22.0% | 60.0% | — | 13.3% | 85.0% | 71.2% | 45.0% | 65.0% |
| **Stall** | 61.7% | 34.5% | 55.0% | 86.7% | — | 45.0% | 51.7% | 40.0% | 39.7% |
| **Ramp** | 51.7% | 66.7% | 50.8% | 15.0% | 55.0% | — | 36.7% | 85.0% | 33.3% |
| **Tempo** | 50.0% | 43.3% | 51.7% | 28.8% | 48.3% | 63.3% | — | 40.0% | 46.7% |
| **Mistura** | 50.0% | 66.7% | 26.7% | 55.0% | 60.0% | 15.0% | 60.0% | — | 11.7% |
| **Eternyr** | 55.0% | 56.7% | 83.3% | 35.0% | 60.3% | 66.7% | 53.3% | 88.3% | — |
