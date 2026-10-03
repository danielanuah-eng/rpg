# Avaliação de balanceamento: teste: passo 14 com o Controle trocando dano ao Conjurador por Eternyr: comprar 3

4860 partidas (60 por confronto ordenado), 4841 decididas. Turnos médios 26.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 19 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 26.3 turnos | 16 a 26 | 1.3% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.4% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.9% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 35.8% (n=120) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 65.5% em 698 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 52.9% de 724 partidas (diferença média +0.8 cartas) | ≥ 55.0% | 2.1 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 33.7 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.55 (4354 × 7847 em 804 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.25 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 10.9% em 46 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 64.2% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.1% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 49.2% (n=120) | ≥ 50.0% | 0.8 pp |
| ❌ | Midrange tem controle suficiente para segurar o Aggro. | 48.3% (n=120) | ≥ 50.0% | 1.7 pp |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 58.3% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.04 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 74.2% de 453 vitórias | ≥ 75.0% | 0.8 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 86.4% em 88 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 15.4% em 389 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 78.6% em 947 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 57.2% | 53.6% | 50.2% | 43.2% | 47.6% | 43.6% | 52.0% | 47.8% | 54.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 51.7% | 64.2% | 95.8% | 36.7% | 63.3% | 49.2% | 35.8% | 60.8% |
| **Midrange** | 48.3% | — | 49.2% | 65.0% | 75.8% | 41.7% | 61.7% | 49.2% | 38.3% |
| **Controle** | 35.8% | 50.8% | — | 54.2% | 68.1% | 61.3% | 41.7% | 60.8% | 29.1% |
| **Burn** | 4.2% | 35.0% | 45.8% | — | 14.2% | 86.7% | 49.6% | 37.5% | 72.5% |
| **Stall** | 63.3% | 24.2% | 31.9% | 85.8% | — | 49.6% | 45.0% | 30.0% | 50.9% |
| **Ramp** | 36.7% | 58.3% | 38.7% | 13.3% | 50.4% | — | 32.5% | 82.5% | 36.2% |
| **Tempo** | 50.8% | 38.3% | 58.3% | 50.4% | 55.0% | 67.5% | — | 43.3% | 52.5% |
| **Mistura** | 64.2% | 50.8% | 39.2% | 62.5% | 70.0% | 17.5% | 56.7% | — | 21.7% |
| **Eternyr** | 39.2% | 61.7% | 70.9% | 27.5% | 49.1% | 63.8% | 47.5% | 78.3% | — |
