# Avaliação de balanceamento: R6 passo 6: + jogador automático guarda as varreduras

2430 partidas (30 por confronto ordenado), 2427 decididas. Turnos médios 22.9. Critérios em sim/criterios.json.

**Situação: LONGE** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 59.4%, Ramp 63.1%, Eternyr 60.4% | ≤ 59.0% | 4.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Stall 31.3%, Tempo 32.9% | ≥ 38.0% | 6.7 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 22.9 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.1% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.7% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 26.7% (n=60) | ≥ 35.0% | 8.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 66.0% em 353 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 70.2% de 363 partidas (diferença média +2.1 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 30.5 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.39 (1501 × 3882 em 410 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.29 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 27.3% em 22 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 73.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.6% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.56 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 98.0% de 150 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 70.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 87.5% em 48 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 15.6% em 167 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 81.3% em 604 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 59.4% | 58.1% | 49.4% | 43.0% | 31.3% | 63.1% | 32.9% | 52.5% | 60.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 35.0% | 73.3% | 93.3% | 30.0% | 46.7% | 86.7% | 46.7% | 63.3% |
| **Midrange** | 65.0% | — | 51.7% | 78.0% | 96.7% | 20.0% | 76.7% | 33.3% | 43.3% |
| **Controle** | 26.7% | 48.3% | — | 20.0% | 100.0% | 56.7% | 48.3% | 58.3% | 36.7% |
| **Burn** | 6.7% | 22.0% | 80.0% | — | 20.0% | 43.3% | 61.7% | 45.0% | 65.0% |
| **Stall** | 70.0% | 3.3% | 0.0% | 80.0% | — | 1.7% | 76.7% | 8.3% | 10.2% |
| **Ramp** | 53.3% | 80.0% | 43.3% | 56.7% | 98.3% | — | 43.3% | 83.3% | 46.7% |
| **Tempo** | 13.3% | 23.3% | 51.7% | 38.3% | 23.3% | 56.7% | — | 16.7% | 40.0% |
| **Mistura** | 53.3% | 66.7% | 41.7% | 55.0% | 91.7% | 16.7% | 83.3% | — | 11.7% |
| **Eternyr** | 36.7% | 56.7% | 63.3% | 35.0% | 89.8% | 53.3% | 60.0% | 88.3% | — |
