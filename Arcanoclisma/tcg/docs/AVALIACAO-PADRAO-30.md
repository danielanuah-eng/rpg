# Avaliação de balanceamento: Padrão aprovado (rodada 4 sem Cataclisma crescente nem Grau suspenso; Aliados atacam Aliados; Controle de remoção)

2430 partidas (30 por confronto ordenado), 2424 decididas. Turnos médios 24.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 17 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 62.7% | ≤ 59.0% | 3.7 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 36.3% | ≥ 38.0% | 1.8 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 24.3 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 38.4% | ≥ 45.0% e ≤ 55.0% | 6.6 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 30.0% (n=60) | ≥ 35.0% | 5.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 64.3% em 353 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 65.0% de 366 partidas (diferença média +1.5 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 31.5 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.41 (1552 × 3805 em 420 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.54 cartas | ≤ 2.5 | 1.7% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 14.3% em 21 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 70.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 84.5% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 43.3% (n=60) | ≥ 50.0% | 6.7 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 60.0% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 40.0% (n=60) | ≥ 50.0% | 10.0 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.56 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 82.7% de 202 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 78.7% em 47 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 20.2% em 178 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 75.1% em 587 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 62.7% | 53.7% | 49.0% | 51.9% | 42.3% | 48.8% | 36.3% | 47.3% | 58.2% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 40.0% | 70.0% | 86.7% | 38.3% | 60.0% | 85.0% | 53.3% | 68.3% |
| **Midrange** | 60.0% | — | 43.3% | 51.7% | 69.5% | 35.0% | 71.7% | 63.3% | 35.0% |
| **Controle** | 30.0% | 56.7% | — | 25.0% | 80.0% | 56.7% | 60.0% | 58.3% | 25.0% |
| **Burn** | 13.3% | 48.3% | 75.0% | — | 31.7% | 60.0% | 71.7% | 40.0% | 75.0% |
| **Stall** | 61.7% | 30.5% | 20.0% | 68.3% | — | 11.7% | 60.0% | 43.3% | 43.1% |
| **Ramp** | 40.0% | 65.0% | 43.3% | 40.0% | 88.3% | — | 36.7% | 55.0% | 21.7% |
| **Tempo** | 15.0% | 28.3% | 40.0% | 28.3% | 40.0% | 63.3% | — | 23.3% | 51.7% |
| **Mistura** | 46.7% | 36.7% | 41.7% | 60.0% | 56.7% | 45.0% | 76.7% | — | 15.0% |
| **Eternyr** | 31.7% | 65.0% | 75.0% | 25.0% | 56.9% | 78.3% | 48.3% | 85.0% | — |
