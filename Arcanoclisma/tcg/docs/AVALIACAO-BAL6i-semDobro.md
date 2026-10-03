# Avaliação de balanceamento: R6 passo 10: curva de Grau sem somar com o bônus antigo de Grau acima do padrão

2430 partidas (30 por confronto ordenado), 2416 decididas. Turnos médios 25.4. Critérios em sim/criterios.json.

**Situação: PRÓXIMO (pedir permissão ao autor)** — 22 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Eternyr 62.4% | ≤ 59.0% | 3.4 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.4 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.6% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.9% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 36.7% (n=60) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 55.0% em 329 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 50.4% de 343 partidas (diferença média +0.7 cartas) | ≥ 55.0% | 4.6 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 29.2 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.69 (2355 × 3399 em 399 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.32 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 12.5% em 24 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 63.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.2% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 61.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 61.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.01 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 75.4% de 252 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 81.8% em 44 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 15.0% em 193 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 79.3% em 449 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 58.3% | 55.4% | 41.2% | 46.0% | 52.8% | 46.0% | 47.1% | 40.6% | 62.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 63.3% | 93.3% | 40.0% | 56.7% | 68.3% | 48.3% | 61.7% |
| **Midrange** | 65.0% | — | 61.7% | 78.0% | 65.5% | 36.7% | 56.7% | 36.7% | 43.3% |
| **Controle** | 36.7% | 38.3% | — | 41.7% | 43.3% | 54.2% | 38.3% | 66.7% | 10.2% |
| **Burn** | 6.7% | 22.0% | 58.3% | — | 13.3% | 86.7% | 71.2% | 46.7% | 63.3% |
| **Stall** | 60.0% | 34.5% | 56.7% | 86.7% | — | 48.3% | 51.7% | 43.3% | 41.4% |
| **Ramp** | 43.3% | 63.3% | 45.8% | 13.3% | 51.7% | — | 38.3% | 88.3% | 24.1% |
| **Tempo** | 31.7% | 43.3% | 61.7% | 28.8% | 48.3% | 61.7% | — | 55.0% | 46.7% |
| **Mistura** | 51.7% | 63.3% | 33.3% | 53.3% | 56.7% | 11.7% | 45.0% | — | 10.0% |
| **Eternyr** | 38.3% | 56.7% | 89.8% | 36.7% | 58.6% | 75.9% | 53.3% | 90.0% | — |
