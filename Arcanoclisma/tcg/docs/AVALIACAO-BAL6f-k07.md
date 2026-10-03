# Avaliação de balanceamento: R6 passo 6 com curva k = 0,7

2430 partidas (30 por confronto ordenado), 2429 decididas. Turnos médios 22.4. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 61.3%, Ramp 63.3% | ≤ 59.0% | 4.3 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Stall 30.2%, Tempo 34.4% | ≥ 38.0% | 7.8 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 22.4 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.4% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 67.6% em 346 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 71.8% de 358 partidas (diferença média +2.1 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 29.8 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.37 (1379 × 3764 em 405 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.11 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 25.0% em 24 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.3% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 48.3% (n=60) | ≥ 50.0% | 1.7 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 73.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 56.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.41 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 95.2% de 145 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 84.4% em 45 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 15.6% em 160 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 81.5% em 596 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 61.3% | 58.5% | 50.8% | 42.1% | 30.2% | 63.3% | 34.4% | 50.8% | 58.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 26.7% | 68.3% | 96.7% | 36.7% | 53.3% | 86.7% | 55.0% | 66.7% |
| **Midrange** | 73.3% | — | 48.3% | 80.0% | 96.7% | 16.7% | 76.7% | 38.3% | 38.3% |
| **Controle** | 31.7% | 51.7% | — | 20.0% | 100.0% | 56.7% | 43.3% | 56.7% | 46.7% |
| **Burn** | 3.3% | 20.0% | 80.0% | — | 18.3% | 50.0% | 58.3% | 48.3% | 58.3% |
| **Stall** | 63.3% | 3.3% | 0.0% | 81.7% | — | 5.0% | 65.0% | 8.3% | 15.3% |
| **Ramp** | 46.7% | 83.3% | 43.3% | 50.0% | 95.0% | — | 53.3% | 81.7% | 53.3% |
| **Tempo** | 13.3% | 23.3% | 56.7% | 41.7% | 35.0% | 46.7% | — | 23.3% | 35.0% |
| **Mistura** | 45.0% | 61.7% | 43.3% | 51.7% | 91.7% | 18.3% | 76.7% | — | 18.3% |
| **Eternyr** | 33.3% | 61.7% | 53.3% | 41.7% | 84.7% | 46.7% | 65.0% | 81.7% | — |
