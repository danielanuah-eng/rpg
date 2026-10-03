# Avaliação de balanceamento: R6 passo 6 com curva k = 0,3

2430 partidas (30 por confronto ordenado), 2425 decididas. Turnos médios 23.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 64.2%, Ramp 59.5% | ≤ 59.0% | 5.2 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Stall 34.8%, Tempo 32.2% | ≥ 38.0% | 5.8 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 23.5 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 47.7% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 20.0% (n=60) | ≥ 35.0% | 15.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 58.5% em 342 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 70.6% de 360 partidas (diferença média +2.0 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 32.4 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.39 (1502 × 3823 em 407 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.27 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 38.9% em 18 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 80.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 63.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 55.9% (n=59) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.63 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 92.8% de 167 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 66.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 80.0% em 50 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 23.3% em 172 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 77.6% em 617 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 64.2% | 57.3% | 43.2% | 46.8% | 34.8% | 59.5% | 32.2% | 55.6% | 56.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 80.0% | 93.3% | 33.3% | 53.3% | 95.0% | 50.0% | 73.3% |
| **Midrange** | 65.0% | — | 63.3% | 75.0% | 93.3% | 23.3% | 70.0% | 26.7% | 41.7% |
| **Controle** | 20.0% | 36.7% | — | 10.0% | 91.7% | 58.3% | 44.1% | 48.3% | 36.7% |
| **Burn** | 6.7% | 25.0% | 90.0% | — | 20.0% | 52.5% | 65.0% | 48.3% | 66.7% |
| **Stall** | 66.7% | 6.7% | 8.3% | 80.0% | — | 3.3% | 80.0% | 8.3% | 25.4% |
| **Ramp** | 46.7% | 76.7% | 41.7% | 47.5% | 96.7% | — | 46.7% | 80.0% | 40.0% |
| **Tempo** | 5.0% | 30.0% | 55.9% | 35.0% | 20.0% | 53.3% | — | 15.0% | 43.3% |
| **Mistura** | 50.0% | 73.3% | 51.7% | 51.7% | 91.7% | 20.0% | 85.0% | — | 21.7% |
| **Eternyr** | 26.7% | 58.3% | 63.3% | 33.3% | 74.6% | 60.0% | 56.7% | 78.3% | — |
