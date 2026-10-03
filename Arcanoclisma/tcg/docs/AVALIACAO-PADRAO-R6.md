# Avaliação de balanceamento: Padrão aprovado (rodada 4 sem Cataclisma crescente nem Grau suspenso; Aliados atacam Aliados; Controle de remoção)

4860 partidas (60 por confronto ordenado), 4842 decididas. Turnos médios 24.4. Critérios em sim/criterios.json.

**Situação: LONGE** — 17 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 63.1% | ≤ 59.0% | 4.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 35.3% | ≥ 38.0% | 2.7 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 24.4 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.4% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 38.6% | ≥ 45.0% e ≤ 55.0% | 6.4 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 25.0% (n=120) | ≥ 35.0% | 10.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 64.4% em 702 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 60.1% de 729 partidas (diferença média +1.2 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 32.7 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.40 (3084 × 7658 em 828 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.55 cartas | ≤ 2.5 | 2.0% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 23.7% em 38 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 75.0% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 84.2% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 46.7% (n=120) | ≥ 50.0% | 3.3 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 62.5% (n=120) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 45.0% (n=120) | ≥ 50.0% | 5.0 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.46 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 82.3% de 423 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 53.3% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 79.0% em 81 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 19.3% em 348 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 78.3% em 1191 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 63.1% | 56.3% | 47.8% | 47.3% | 44.3% | 53.3% | 35.3% | 47.7% | 54.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 37.5% | 75.0% | 95.0% | 46.7% | 52.5% | 85.0% | 40.8% | 72.5% |
| **Midrange** | 62.5% | — | 46.7% | 63.0% | 68.3% | 35.0% | 75.0% | 67.5% | 32.5% |
| **Controle** | 25.0% | 53.3% | — | 17.5% | 75.8% | 60.8% | 55.0% | 57.5% | 37.5% |
| **Burn** | 5.0% | 37.0% | 82.5% | — | 21.7% | 54.2% | 66.4% | 40.0% | 71.4% |
| **Stall** | 53.3% | 31.7% | 24.2% | 78.3% | — | 10.8% | 64.2% | 44.2% | 47.8% |
| **Ramp** | 47.5% | 65.0% | 39.2% | 45.8% | 89.2% | — | 35.0% | 65.8% | 38.7% |
| **Tempo** | 15.0% | 25.0% | 45.0% | 33.6% | 35.8% | 65.0% | — | 21.7% | 41.7% |
| **Mistura** | 59.2% | 32.5% | 42.5% | 60.0% | 55.8% | 34.2% | 78.3% | — | 19.2% |
| **Eternyr** | 27.5% | 67.5% | 62.5% | 28.6% | 52.2% | 61.3% | 58.3% | 80.8% | — |
