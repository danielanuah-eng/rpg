# Avaliação de balanceamento: R6 passo 14: Controle troca 2 Aliados de ataque pela Estrutura que compra ao entrar

2430 partidas (30 por confronto ordenado), 2421 decididas. Turnos médios 26.0. Critérios em sim/criterios.json.

**Situação: PRÓXIMO (pedir permissão ao autor)** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 59.2% | ≤ 59.0% | 0.2 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 26.0 turnos | 16 a 26 | 0.1% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.4% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.3% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 40.0% (n=60) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 68.5% em 356 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 50.8% de 366 partidas (diferença média +0.5 cartas) | ≥ 55.0% | 4.2 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 33.6 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.54 (2138 × 3980 em 403 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.29 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 4.0% em 25 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.1% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 60.0% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 55.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 65.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.00 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 75.8% de 223 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 81.8% em 44 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 18.6% em 188 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 74.8% em 437 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 59.2% | 56.7% | 52.7% | 44.8% | 46.7% | 42.6% | 53.1% | 40.8% | 53.3% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 45.0% | 60.0% | 93.3% | 40.0% | 60.0% | 68.3% | 46.7% | 60.0% |
| **Midrange** | 55.0% | — | 60.0% | 58.3% | 71.7% | 31.7% | 66.7% | 56.7% | 53.3% |
| **Controle** | 40.0% | 40.0% | — | 51.7% | 68.3% | 83.3% | 35.0% | 65.0% | 38.3% |
| **Burn** | 6.7% | 41.7% | 48.3% | — | 13.3% | 85.0% | 48.3% | 45.0% | 70.0% |
| **Stall** | 60.0% | 28.3% | 31.7% | 86.7% | — | 45.0% | 36.7% | 40.0% | 45.6% |
| **Ramp** | 40.0% | 68.3% | 16.7% | 15.0% | 55.0% | — | 30.0% | 85.0% | 31.0% |
| **Tempo** | 31.7% | 33.3% | 65.0% | 51.7% | 63.3% | 70.0% | — | 50.0% | 60.0% |
| **Mistura** | 53.3% | 43.3% | 35.0% | 55.0% | 60.0% | 15.0% | 50.0% | — | 15.0% |
| **Eternyr** | 40.0% | 46.7% | 61.7% | 30.0% | 54.4% | 69.0% | 40.0% | 85.0% | — |
