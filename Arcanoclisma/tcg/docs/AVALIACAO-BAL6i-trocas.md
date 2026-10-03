# Avaliação de balanceamento: teste: passo 8 com o Aggro do agente trocando as cartas lentas por cartas baratas

2430 partidas (30 por confronto ordenado), 2415 decididas. Turnos médios 25.1. Critérios em sim/criterios.json.

**Situação: LONGE** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 65.8% | ≤ 59.0% | 6.8 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 25.1 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.6% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.2% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 38.3% (n=60) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 59.5% em 328 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 51.0% de 339 partidas (diferença média +0.7 cartas) | ≥ 55.0% | 4.0 pp |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 28.0 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.65 (2180 × 3336 em 400 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.41 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 0.0% em 20 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.6% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 53.3% (n=60) | ≥ 50.0% |  |
| ❌ | Midrange tem controle suficiente para segurar o Aggro. | 43.3% (n=60) | ≥ 50.0% | 6.7 pp |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.96 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 75.3% de 247 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 82.2% em 45 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 25.0% em 164 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 75.1% em 406 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 65.8% | 50.9% | 44.7% | 46.0% | 51.8% | 45.6% | 43.8% | 42.7% | 58.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 56.7% | 61.7% | 93.3% | 38.3% | 78.0% | 71.7% | 53.3% | 73.3% |
| **Midrange** | 43.3% | — | 53.3% | 78.0% | 65.5% | 33.3% | 56.7% | 33.3% | 43.3% |
| **Controle** | 38.3% | 46.7% | — | 40.0% | 45.0% | 49.2% | 48.3% | 73.3% | 16.7% |
| **Burn** | 6.7% | 22.0% | 60.0% | — | 13.3% | 85.0% | 71.2% | 45.0% | 65.0% |
| **Stall** | 61.7% | 34.5% | 55.0% | 86.7% | — | 45.0% | 51.7% | 40.0% | 39.7% |
| **Ramp** | 22.0% | 66.7% | 50.8% | 15.0% | 55.0% | — | 36.7% | 85.0% | 33.3% |
| **Tempo** | 28.3% | 43.3% | 51.7% | 28.8% | 48.3% | 63.3% | — | 40.0% | 46.7% |
| **Mistura** | 46.7% | 66.7% | 26.7% | 55.0% | 60.0% | 15.0% | 60.0% | — | 11.7% |
| **Eternyr** | 26.7% | 56.7% | 83.3% | 35.0% | 60.3% | 66.7% | 53.3% | 88.3% | — |
