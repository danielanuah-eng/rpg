# Avaliação de balanceamento: R5 + Aliados atacam Aliados (mesa)

2430 partidas (30 por confronto ordenado), 2421 decididas. Turnos médios 24.6. Critérios em sim/criterios.json.

**Situação: LONGE** — 14 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 61.5%, Midrange 61.0%, Mistura 59.5%, Eternyr 59.4% | ≤ 59.0% | 2.5 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Controle 22.5%, Tempo 33.2% | ≥ 38.0% | 15.5 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 24.6 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.4% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 56.4% | ≥ 45.0% e ≤ 55.0% | 1.4 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 29.3% em 369 partidas com 18+ turnos | ≥ 55.0% | 25.7 pp |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.79 (2833 × 3581 em 417 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 1.94 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 36.7% em 30 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 100.0% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 70.0% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 28.3% (n=60) | ≥ 50.0% | 21.7 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.84 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 78.8% de 245 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 56.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 82.9% em 41 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 61.5% | 61.0% | 22.5% | 43.2% | 51.1% | 58.6% | 33.2% | 59.5% | 59.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 30.0% | 68.3% | 91.7% | 43.3% | 55.0% | 88.3% | 40.0% | 75.0% |
| **Midrange** | 70.0% | — | 100.0% | 51.7% | 71.7% | 43.3% | 70.0% | 56.7% | 25.0% |
| **Controle** | 31.7% | 0.0% | — | 53.3% | 5.0% | 5.0% | 71.7% | 1.7% | 11.7% |
| **Burn** | 8.3% | 48.3% | 46.7% | — | 26.7% | 45.0% | 69.5% | 36.7% | 64.4% |
| **Stall** | 56.7% | 28.3% | 95.0% | 73.3% | — | 16.7% | 58.3% | 32.2% | 48.3% |
| **Ramp** | 45.0% | 56.7% | 95.0% | 55.0% | 83.3% | — | 48.3% | 65.0% | 20.3% |
| **Tempo** | 11.7% | 30.0% | 28.3% | 30.5% | 41.7% | 51.7% | — | 18.3% | 53.3% |
| **Mistura** | 60.0% | 43.3% | 98.3% | 63.3% | 67.8% | 35.0% | 81.7% | — | 26.7% |
| **Eternyr** | 25.0% | 75.0% | 88.3% | 35.6% | 51.7% | 79.7% | 46.7% | 73.3% | — |
