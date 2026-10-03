# Avaliação de balanceamento: R6 passo 10: Controle do agente com o estilo de Controle (agressividade 0,3)

2430 partidas (30 por confronto ordenado), 2413 decididas. Turnos médios 25.8. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Eternyr 61.1% | ≤ 59.0% | 2.1 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.8 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.7% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.3% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 38.3% (n=60) | ≥ 35.0% |  |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 54.5% em 341 partidas com 18+ turnos | ≥ 55.0% | 0.5 pp |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 52.8% de 354 partidas (diferença média +0.8 cartas) | ≥ 55.0% | 2.2 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 32.0 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.65 (2437 × 3742 em 398 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.33 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 11.5% em 26 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.1% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 55.0% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 58.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.00 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 74.6% de 248 vitórias | ≥ 75.0% | 0.4 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 81.8% em 44 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 17.1% em 199 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 78.4% em 449 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 58.5% | 53.8% | 40.0% | 46.9% | 52.0% | 49.0% | 45.1% | 43.8% | 61.1% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 61.7% | 93.3% | 40.0% | 60.0% | 68.3% | 46.7% | 63.3% |
| **Midrange** | 65.0% | — | 55.0% | 78.0% | 65.5% | 33.3% | 56.7% | 33.3% | 43.3% |
| **Controle** | 38.3% | 45.0% | — | 33.3% | 41.7% | 39.7% | 41.7% | 71.7% | 8.5% |
| **Burn** | 6.7% | 22.0% | 66.7% | — | 13.3% | 85.0% | 71.2% | 45.0% | 65.0% |
| **Stall** | 60.0% | 34.5% | 58.3% | 86.7% | — | 45.0% | 51.7% | 40.0% | 39.7% |
| **Ramp** | 40.0% | 66.7% | 60.3% | 15.0% | 55.0% | — | 36.7% | 85.0% | 33.3% |
| **Tempo** | 31.7% | 43.3% | 58.3% | 28.8% | 48.3% | 63.3% | — | 40.0% | 46.7% |
| **Mistura** | 53.3% | 66.7% | 28.3% | 55.0% | 60.0% | 15.0% | 60.0% | — | 11.7% |
| **Eternyr** | 36.7% | 56.7% | 91.5% | 35.0% | 60.3% | 66.7% | 53.3% | 88.3% | — |
