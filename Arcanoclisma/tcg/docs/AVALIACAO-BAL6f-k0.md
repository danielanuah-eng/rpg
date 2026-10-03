# Avaliação de balanceamento: R6 passo 6 sem a curva de Grau (k = 0)

2430 partidas (30 por confronto ordenado), 2426 decididas. Turnos médios 24.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 19 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 68.5% | ≤ 59.0% | 9.5 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 32.5% | ≥ 38.0% | 5.5 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 24.5 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 47.9% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 18.3% (n=60) | ≥ 35.0% | 16.7 pp |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 53.8% em 351 partidas com 18+ turnos | ≥ 55.0% | 1.2 pp |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 69.0% de 364 partidas (diferença média +2.0 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 36.2 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.44 (1858 × 4183 em 412 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.27 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 42.1% em 19 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 81.7% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.0% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 66.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 56.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 56.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.81 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 83.0% de 182 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 48.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 61.7% em 47 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 27.3% em 165 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 76.6% em 620 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 68.5% | 52.1% | 40.6% | 46.0% | 38.1% | 59.0% | 32.5% | 55.0% | 58.2% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 43.3% | 81.7% | 93.3% | 51.7% | 56.7% | 93.3% | 55.0% | 73.3% |
| **Midrange** | 56.7% | — | 66.7% | 66.7% | 88.1% | 13.3% | 71.7% | 18.3% | 35.0% |
| **Controle** | 18.3% | 33.3% | — | 15.0% | 73.3% | 65.0% | 43.3% | 51.7% | 25.0% |
| **Burn** | 6.7% | 33.3% | 85.0% | — | 13.3% | 53.3% | 68.3% | 45.0% | 63.3% |
| **Stall** | 48.3% | 11.9% | 26.7% | 86.7% | — | 5.0% | 80.0% | 5.0% | 41.4% |
| **Ramp** | 43.3% | 86.7% | 35.0% | 46.7% | 95.0% | — | 43.3% | 83.3% | 38.3% |
| **Tempo** | 6.7% | 28.3% | 56.7% | 31.7% | 20.0% | 56.7% | — | 13.3% | 46.7% |
| **Mistura** | 45.0% | 81.7% | 48.3% | 55.0% | 95.0% | 16.7% | 86.7% | — | 11.7% |
| **Eternyr** | 26.7% | 65.0% | 75.0% | 36.7% | 58.6% | 61.7% | 53.3% | 88.3% | — |
