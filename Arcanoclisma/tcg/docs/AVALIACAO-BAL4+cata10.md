# Avaliação de balanceamento: BAL4 + Cataclisma crescente desde o turno 10

2430 partidas (30 por confronto ordenado), 2430 decididas. Turnos médios 21.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 15 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 61.7%, Midrange 71.5%, Stall 80.0% | ≤ 59.0% | 21.0 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Ramp 34.8%, Eternyr 24.8% | ≥ 38.0% | 13.2 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 21.3 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 42.0% | ≥ 45.0% e ≤ 55.0% | 3.0 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 26.7% (n=60) | ≥ 35.0% | 8.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 57.6% em 330 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.46 (726 × 1564 em 386 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.83 cartas | ≤ 2.5 | 13.4% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 33.3% em 24 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 73.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 83.5% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 68.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 80.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 66.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.38 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 99.2% de 384 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 70.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 82.4% em 51 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 61.7% | 71.5% | 40.8% | 49.4% | 80.0% | 34.8% | 43.7% | 43.3% | 24.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 20.0% | 73.3% | 95.0% | 30.0% | 58.3% | 65.0% | 66.7% | 85.0% |
| **Midrange** | 80.0% | — | 68.3% | 73.3% | 41.7% | 61.7% | 86.7% | 73.3% | 86.7% |
| **Controle** | 26.7% | 31.7% | — | 23.3% | 25.0% | 68.3% | 33.3% | 61.7% | 56.7% |
| **Burn** | 5.0% | 26.7% | 76.7% | — | 13.3% | 80.0% | 51.7% | 43.3% | 98.3% |
| **Stall** | 70.0% | 58.3% | 75.0% | 86.7% | — | 86.7% | 80.0% | 85.0% | 98.3% |
| **Ramp** | 41.7% | 38.3% | 31.7% | 20.0% | 13.3% | — | 26.7% | 58.3% | 48.3% |
| **Tempo** | 35.0% | 13.3% | 66.7% | 48.3% | 20.0% | 73.3% | — | 31.7% | 61.7% |
| **Mistura** | 33.3% | 26.7% | 38.3% | 56.7% | 15.0% | 41.7% | 68.3% | — | 66.7% |
| **Eternyr** | 15.0% | 13.3% | 43.3% | 1.7% | 1.7% | 51.7% | 38.3% | 33.3% | — |
