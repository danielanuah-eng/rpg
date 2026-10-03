# Avaliação de balanceamento: R6 passo 7: + varreduras novas no Stall

2430 partidas (30 por confronto ordenado), 2421 decididas. Turnos médios 24.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 60.6%, Ramp 61.7% | ≤ 59.0% | 2.7 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 32.5% | ≥ 38.0% | 5.5 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 24.3 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.4% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 46.3% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 26.7% (n=60) | ≥ 35.0% | 8.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 56.8% em 352 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 71.3% de 363 partidas (diferença média +2.2 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 30.4 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.44 (1753 × 3983 em 409 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.28 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 27.3% em 22 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 73.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.7% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.53 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 81.9% de 243 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 81.8% em 44 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 17.1% em 164 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 80.9% em 587 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 60.6% | 54.2% | 42.6% | 42.1% | 51.0% | 61.7% | 32.5% | 48.5% | 56.7% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 73.3% | 93.3% | 40.0% | 46.7% | 86.7% | 46.7% | 63.3% |
| **Midrange** | 65.0% | — | 51.7% | 78.0% | 65.5% | 20.0% | 76.7% | 33.3% | 43.3% |
| **Controle** | 26.7% | 48.3% | — | 20.0% | 45.8% | 56.7% | 48.3% | 58.3% | 36.7% |
| **Burn** | 6.7% | 22.0% | 80.0% | — | 13.3% | 43.3% | 61.7% | 45.0% | 65.0% |
| **Stall** | 60.0% | 34.5% | 54.2% | 86.7% | — | 13.3% | 80.0% | 40.0% | 39.7% |
| **Ramp** | 53.3% | 80.0% | 43.3% | 56.7% | 86.7% | — | 43.3% | 83.3% | 46.7% |
| **Tempo** | 13.3% | 23.3% | 51.7% | 38.3% | 20.0% | 56.7% | — | 16.7% | 40.0% |
| **Mistura** | 53.3% | 66.7% | 41.7% | 55.0% | 60.0% | 16.7% | 83.3% | — | 11.7% |
| **Eternyr** | 36.7% | 56.7% | 63.3% | 35.0% | 60.3% | 53.3% | 60.0% | 88.3% | — |
