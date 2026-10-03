# Avaliação de balanceamento: R6 passo 3: + curva de poder por Grau nos Aliados

2430 partidas (30 por confronto ordenado), 2424 decididas. Turnos médios 23.6. Critérios em sim/criterios.json.

**Situação: LONGE** — 18 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 59.8% | ≤ 59.0% | 0.8 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Stall 37.7% | ≥ 38.0% | 0.3 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 23.6 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.8% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 25.0% (n=60) | ≥ 35.0% | 10.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 78.4% em 343 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 68.1% de 360 partidas (diferença média +1.7 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 31.7 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.34 (1291 × 3796 em 403 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.25 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 21.1% em 19 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 75.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.6% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 35.0% (n=60) | ≥ 50.0% | 15.0 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 76.7% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 41.7% (n=60) | ≥ 50.0% | 8.3 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.37 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 86.7% de 181 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 55.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 84.6% em 39 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 19.4% em 165 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 76.0% em 563 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 59.8% | 54.8% | 58.1% | 50.2% | 37.7% | 53.4% | 38.3% | 42.7% | 54.9% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 23.3% | 75.0% | 93.3% | 45.0% | 55.0% | 80.0% | 46.7% | 60.0% |
| **Midrange** | 76.7% | — | 35.0% | 63.3% | 65.0% | 35.0% | 60.0% | 56.7% | 46.7% |
| **Controle** | 25.0% | 65.0% | — | 11.7% | 94.8% | 80.0% | 58.3% | 78.3% | 51.7% |
| **Burn** | 6.7% | 36.7% | 88.3% | — | 41.7% | 40.0% | 73.3% | 45.0% | 70.0% |
| **Stall** | 55.0% | 35.0% | 5.2% | 58.3% | — | 16.7% | 60.0% | 38.3% | 33.3% |
| **Ramp** | 45.0% | 65.0% | 20.0% | 60.0% | 83.3% | — | 36.7% | 83.3% | 33.9% |
| **Tempo** | 20.0% | 40.0% | 41.7% | 26.7% | 40.0% | 63.3% | — | 25.0% | 50.0% |
| **Mistura** | 53.3% | 43.3% | 21.7% | 55.0% | 61.7% | 16.7% | 75.0% | — | 15.0% |
| **Eternyr** | 40.0% | 53.3% | 48.3% | 30.0% | 66.7% | 66.1% | 50.0% | 85.0% | — |
