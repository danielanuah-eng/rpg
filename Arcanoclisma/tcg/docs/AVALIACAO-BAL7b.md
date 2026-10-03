# Avaliação de balanceamento: BAL7 mantendo a regra do primeiro Conjurador

2430 partidas (30 por confronto ordenado), 2430 decididas. Turnos médios 20.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 15 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Controle 66.3%, Stall 59.2%, Tempo 60.6% | ≤ 59.0% | 7.3 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Ramp 32.1%, Eternyr 27.3% | ≥ 38.0% | 10.7 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.3 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 42.5% | ≥ 45.0% e ≤ 55.0% | 2.5 pp |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 66.7% (n=60) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 73.5% em 370 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 1.11 (1465 × 1316 em 425 partidas) | ≤ 1 | 11.3% |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.23 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 7.0% em 43 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 33.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.3% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 31.7% (n=60) | ≥ 50.0% | 18.3 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 73.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 55.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.94 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 97.2% de 284 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 48.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 69.0% em 42 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 59.0% | 57.7% | 66.3% | 47.3% | 59.2% | 32.1% | 60.6% | 40.6% | 27.3% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 26.7% | 33.3% | 91.7% | 51.7% | 73.3% | 70.0% | 50.0% | 75.0% |
| **Midrange** | 73.3% | — | 31.7% | 61.7% | 41.7% | 46.7% | 56.7% | 65.0% | 85.0% |
| **Controle** | 66.7% | 68.3% | — | 71.7% | 41.7% | 75.0% | 45.0% | 78.3% | 83.3% |
| **Burn** | 8.3% | 38.3% | 28.3% | — | 36.7% | 80.0% | 53.3% | 43.3% | 90.0% |
| **Stall** | 48.3% | 58.3% | 58.3% | 63.3% | — | 76.7% | 31.7% | 63.3% | 73.3% |
| **Ramp** | 26.7% | 53.3% | 25.0% | 20.0% | 23.3% | — | 15.0% | 58.3% | 35.0% |
| **Tempo** | 30.0% | 43.3% | 55.0% | 46.7% | 68.3% | 85.0% | — | 66.7% | 90.0% |
| **Mistura** | 50.0% | 35.0% | 21.7% | 56.7% | 36.7% | 41.7% | 33.3% | — | 50.0% |
| **Eternyr** | 25.0% | 15.0% | 16.7% | 10.0% | 26.7% | 65.0% | 10.0% | 50.0% | — |
