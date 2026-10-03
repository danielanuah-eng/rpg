# Avaliação de balanceamento: BAL4 + Aliados Etéreos no Tempo

2430 partidas (30 por confronto ordenado), 2359 decididas. Turnos médios 30.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 12 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 60.6% | ≤ 59.0% | 1.6 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Eternyr 30.9% | ≥ 38.0% | 7.1 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 30.3 turnos | 16 a 26 | 16.4% |
| ✅ | Partidas que não terminam: no máximo 5%. | 2.9% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 44.7% | ≥ 45.0% e ≤ 55.0% | 0.3 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 70.8% em 329 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.37 (1325 × 3544 em 381 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.82 cartas | ≤ 2.5 | 12.9% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 20.8% em 24 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.9% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 45.0% (n=60) | ≥ 50.0% | 5.0 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 63.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.42 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 67.9% de 252 vitórias | ≥ 75.0% | 7.1 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 72.5% em 51 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 56.9% | 60.6% | 50.1% | 47.5% | 53.9% | 49.6% | 55.3% | 45.2% | 30.9% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 68.3% | 95.0% | 38.3% | 55.0% | 70.0% | 40.0% | 70.0% |
| **Midrange** | 81.7% | — | 45.0% | 75.0% | 80.4% | 28.6% | 65.0% | 45.0% | 64.4% |
| **Controle** | 31.7% | 55.0% | — | 30.0% | 93.1% | 45.0% | 36.7% | 70.0% | 39.7% |
| **Burn** | 5.0% | 25.0% | 70.0% | — | 6.7% | 80.0% | 56.7% | 40.0% | 96.7% |
| **Stall** | 61.7% | 19.6% | 6.9% | 93.3% | — | 49.1% | 74.6% | 48.3% | 78.0% |
| **Ramp** | 45.0% | 71.4% | 55.0% | 20.0% | 50.9% | — | 15.0% | 73.2% | 65.9% |
| **Tempo** | 30.0% | 35.0% | 63.3% | 43.3% | 25.4% | 85.0% | — | 66.7% | 93.3% |
| **Mistura** | 60.0% | 55.0% | 30.0% | 60.0% | 51.7% | 26.8% | 33.3% | — | 45.0% |
| **Eternyr** | 30.0% | 35.6% | 60.3% | 3.3% | 22.0% | 34.1% | 6.7% | 55.0% | — |
