# Avaliação de balanceamento: R6 passo 1: PADRAO sem a carta extra do segundo jogador

2430 partidas (30 por confronto ordenado), 2422 decididas. Turnos médios 24.4. Critérios em sim/criterios.json.

**Situação: LONGE** — 18 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 64.6% | ≤ 59.0% | 5.6 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 33.1% | ≥ 38.0% | 4.9 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 24.4 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.3% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 46.0% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 20.0% (n=60) | ≥ 35.0% | 15.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 66.9% em 359 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 62.3% de 374 partidas (diferença média +1.4 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 33.0 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.39 (1509 × 3823 em 417 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.22 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 42.9% em 21 partidas com 18+ turnos | ≤ 40.0% | 2.9 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 80.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.6% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 40.0% (n=60) | ≥ 50.0% | 10.0 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 70.0% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 31.7% (n=60) | ≥ 50.0% | 18.3 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.55 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 83.7% de 202 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 53.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 76.2% em 42 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 20.5% em 166 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 74.6% em 602 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 64.6% | 56.0% | 51.3% | 50.0% | 42.1% | 50.4% | 33.1% | 44.0% | 58.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 30.0% | 80.0% | 93.3% | 46.7% | 51.7% | 83.3% | 55.0% | 76.7% |
| **Midrange** | 70.0% | — | 40.0% | 63.3% | 63.3% | 45.0% | 71.7% | 58.3% | 36.7% |
| **Controle** | 20.0% | 60.0% | — | 23.3% | 85.0% | 60.0% | 68.3% | 63.3% | 30.0% |
| **Burn** | 6.7% | 36.7% | 76.7% | — | 35.0% | 51.7% | 81.7% | 45.0% | 66.7% |
| **Stall** | 53.3% | 36.7% | 15.0% | 65.0% | — | 10.0% | 70.0% | 43.3% | 43.3% |
| **Ramp** | 48.3% | 55.0% | 40.0% | 48.3% | 90.0% | — | 31.7% | 71.7% | 18.3% |
| **Tempo** | 16.7% | 28.3% | 31.7% | 18.3% | 30.0% | 68.3% | — | 25.0% | 46.7% |
| **Mistura** | 45.0% | 41.7% | 36.7% | 55.0% | 56.7% | 28.3% | 75.0% | — | 13.3% |
| **Eternyr** | 23.3% | 63.3% | 70.0% | 33.3% | 56.7% | 81.7% | 53.3% | 86.7% | — |
