# Avaliação de balanceamento: R6 passo 11: Controle troca 3 cartas de Aggro (Aliado de custo 1, 4-drop proficiente, dano ao Conjurador) por compra

2430 partidas (30 por confronto ordenado), 2415 decididas. Turnos médios 25.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.5 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.6% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.5% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 53.1% em 322 partidas com 18+ turnos | ≥ 55.0% | 1.9 pp |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 48.4% de 339 partidas (diferença média +0.5 cartas) | ≥ 55.0% | 6.6 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 28.2 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.62 (2094 × 3389 em 384 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.33 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 10.0% em 20 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.4% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 56.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 70.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.96 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 74.6% de 248 vitórias | ≥ 75.0% | 0.4 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 81.8% em 44 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 17.1% em 193 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 77.3% em 440 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 59.0% | 54.2% | 39.0% | 46.0% | 52.1% | 48.9% | 48.0% | 45.6% | 57.2% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 68.3% | 93.3% | 40.0% | 60.0% | 68.3% | 46.7% | 60.0% |
| **Midrange** | 65.0% | — | 56.7% | 78.0% | 65.5% | 33.3% | 56.7% | 33.3% | 45.0% |
| **Controle** | 31.7% | 43.3% | — | 45.0% | 46.7% | 38.3% | 30.0% | 60.0% | 17.2% |
| **Burn** | 6.7% | 22.0% | 55.0% | — | 13.3% | 85.0% | 71.2% | 45.0% | 70.0% |
| **Stall** | 60.0% | 34.5% | 53.3% | 86.7% | — | 45.0% | 51.7% | 40.0% | 45.6% |
| **Ramp** | 40.0% | 66.7% | 61.7% | 15.0% | 55.0% | — | 36.7% | 85.0% | 31.0% |
| **Tempo** | 31.7% | 43.3% | 70.0% | 28.8% | 48.3% | 63.3% | — | 40.0% | 58.3% |
| **Mistura** | 53.3% | 66.7% | 40.0% | 55.0% | 60.0% | 15.0% | 60.0% | — | 15.0% |
| **Eternyr** | 40.0% | 55.0% | 82.8% | 30.0% | 54.4% | 69.0% | 41.7% | 85.0% | — |
