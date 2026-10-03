# Avaliação de balanceamento: R6 passo 5: + Fúria (varredura simétrica barata) no Controle

2430 partidas (30 por confronto ordenado), 2425 decididas. Turnos médios 22.9. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 60.2%, Ramp 61.5% | ≤ 59.0% | 2.5 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Stall 32.3%, Tempo 33.9% | ≥ 38.0% | 5.7 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 22.9 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.9% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 23.3% (n=60) | ≥ 35.0% | 11.7 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 72.7% em 348 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 70.0% de 367 partidas (diferença média +2.0 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 31.3 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.35 (1407 × 4004 em 416 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.28 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 33.3% em 21 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 76.7% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 38.3% (n=60) | ≥ 50.0% | 11.7 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 50.8% (n=59) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.54 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 93.5% de 155 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 66.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 85.1% em 47 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 16.4% em 165 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 80.5% em 604 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 60.2% | 55.8% | 54.1% | 43.6% | 32.3% | 61.5% | 33.9% | 51.2% | 57.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 76.7% | 93.3% | 33.3% | 46.7% | 86.7% | 46.7% | 63.3% |
| **Midrange** | 65.0% | — | 38.3% | 78.0% | 91.7% | 20.0% | 76.7% | 33.3% | 43.3% |
| **Controle** | 23.3% | 61.7% | — | 13.3% | 90.0% | 70.0% | 49.2% | 70.0% | 55.2% |
| **Burn** | 6.7% | 22.0% | 86.7% | — | 18.3% | 43.3% | 61.7% | 45.0% | 65.0% |
| **Stall** | 66.7% | 8.3% | 10.0% | 81.7% | — | 1.7% | 68.3% | 6.7% | 15.0% |
| **Ramp** | 53.3% | 80.0% | 30.0% | 56.7% | 98.3% | — | 43.3% | 83.3% | 46.7% |
| **Tempo** | 13.3% | 23.3% | 50.8% | 38.3% | 31.7% | 56.7% | — | 16.7% | 40.0% |
| **Mistura** | 53.3% | 66.7% | 30.0% | 55.0% | 93.3% | 16.7% | 83.3% | — | 11.7% |
| **Eternyr** | 36.7% | 56.7% | 44.8% | 35.0% | 85.0% | 53.3% | 60.0% | 88.3% | — |
