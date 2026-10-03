# Avaliação de balanceamento: Balanceador rodada 6 (varreduras, curva de Grau, unidades-problema e bombas)

6480 partidas (80 por confronto ordenado), 6447 decididas. Turnos médios 26.1. Critérios em sim/criterios.json.

**Situação: PRÓXIMO (pedir permissão ao autor)** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 26.1 turnos | 16 a 26 | 0.5% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.5% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 46.4% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 35.6% (n=160) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 59.0% em 913 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 53.0% de 953 partidas (diferença média +0.8 cartas) | ≥ 55.0% | 2.0 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 33.5 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.54 (5531 × 10158 em 1055 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.18 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 10.9% em 64 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 64.4% (n=160) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.7% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 60.0% (n=160) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 50.6% (n=160) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 67.5% (n=160) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.06 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 74.7% de 605 vitórias | ≥ 75.0% | 0.3 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 66.9% (n=160) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 87.0% em 123 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 16.3% em 497 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 79.5% em 1200 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 58.6% | 58.3% | 44.3% | 43.8% | 47.7% | 44.5% | 52.2% | 45.6% | 54.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 49.4% | 64.4% | 98.1% | 33.1% | 56.9% | 61.9% | 44.4% | 61.0% |
| **Midrange** | 50.6% | — | 60.0% | 61.9% | 76.9% | 46.3% | 62.5% | 56.9% | 51.2% |
| **Controle** | 35.6% | 40.0% | — | 50.6% | 56.3% | 53.5% | 32.5% | 55.6% | 30.3% |
| **Burn** | 1.9% | 38.1% | 49.4% | — | 16.3% | 81.9% | 52.8% | 34.8% | 75.6% |
| **Stall** | 66.9% | 23.1% | 43.7% | 83.8% | — | 49.4% | 43.7% | 29.4% | 42.1% |
| **Ramp** | 43.1% | 53.8% | 46.5% | 18.1% | 50.6% | — | 29.4% | 85.6% | 29.0% |
| **Tempo** | 38.1% | 37.5% | 67.5% | 47.2% | 56.3% | 70.6% | — | 48.1% | 52.5% |
| **Mistura** | 55.6% | 43.1% | 44.4% | 65.2% | 70.6% | 14.4% | 51.9% | — | 19.4% |
| **Eternyr** | 39.0% | 48.8% | 69.7% | 24.4% | 57.9% | 71.0% | 47.5% | 80.6% | — |
