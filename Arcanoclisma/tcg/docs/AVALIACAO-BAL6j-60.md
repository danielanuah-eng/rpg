# Avaliação de balanceamento: Balanceador rodada 6 (varreduras, curva de Grau, unidades-problema e bombas)

4860 partidas (60 por confronto ordenado), 4844 decididas. Turnos médios 25.4. Critérios em sim/criterios.json.

**Situação: LONGE** — 22 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.4 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.3% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.1% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 36.7% (n=120) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 56.2% em 653 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 46.3% de 683 partidas (diferença média +0.4 cartas) | ≥ 55.0% | 8.7 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 27.9 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.68 (4553 × 6684 em 802 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.25 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 2.3% em 44 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 63.3% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 83.3% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 53.3% (n=120) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 59.2% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 66.7% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.08 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 74.6% de 500 vitórias | ≥ 75.0% | 0.4 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 86.4% em 88 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 16.0% em 381 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 77.2% em 891 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 58.3% | 54.0% | 42.5% | 45.3% | 52.6% | 46.5% | 46.6% | 48.6% | 55.7% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 40.8% | 63.3% | 95.8% | 36.7% | 63.3% | 70.0% | 35.8% | 60.8% |
| **Midrange** | 59.2% | — | 53.3% | 79.2% | 65.8% | 45.0% | 54.2% | 35.0% | 40.0% |
| **Controle** | 36.7% | 46.7% | — | 40.8% | 50.0% | 44.2% | 33.3% | 66.7% | 21.8% |
| **Burn** | 4.2% | 20.8% | 59.2% | — | 14.2% | 86.7% | 67.5% | 37.5% | 72.5% |
| **Stall** | 63.3% | 34.2% | 50.0% | 85.8% | — | 49.6% | 56.7% | 30.0% | 50.9% |
| **Ramp** | 36.7% | 55.0% | 55.8% | 13.3% | 50.4% | — | 41.7% | 82.5% | 36.2% |
| **Tempo** | 30.0% | 45.8% | 66.7% | 32.5% | 43.3% | 58.3% | — | 45.0% | 50.8% |
| **Mistura** | 64.2% | 65.0% | 33.3% | 62.5% | 70.0% | 17.5% | 55.0% | — | 21.7% |
| **Eternyr** | 39.2% | 60.0% | 78.2% | 27.5% | 49.1% | 63.8% | 49.2% | 78.3% | — |
