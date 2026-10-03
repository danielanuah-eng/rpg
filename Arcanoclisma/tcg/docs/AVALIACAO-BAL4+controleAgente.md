# Avaliação de balanceamento: BAL4 + Controle do agente de testes

2430 partidas (30 por confronto ordenado), 2336 decididas. Turnos médios 31.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 13 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 65.0%, Stall 61.2% | ≤ 59.0% | 6.0 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 34.6%, Eternyr 32.8% | ≥ 38.0% | 5.2 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 31.5 turnos | 16 a 26 | 21.1% |
| ✅ | Partidas que não terminam: no máximo 5%. | 3.9% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 45.0% | ≥ 45.0% e ≤ 55.0% | 0.0 pp |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 66.7% (n=60) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 59.9% em 359 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.96 (2637 × 2733 em 408 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.82 cartas | ≤ 2.5 | 12.6% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 17.0% em 47 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 33.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.4% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 58.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 38.3% (n=60) | ≥ 50.0% | 11.7 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.91 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 64.9% de 282 vitórias | ≥ 75.0% | 10.1 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 72.5% em 51 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 52.3% | 65.0% | 57.3% | 41.7% | 61.2% | 49.8% | 34.6% | 55.3% | 32.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 33.3% | 95.0% | 38.3% | 55.0% | 68.3% | 40.0% | 70.0% |
| **Midrange** | 81.7% | — | 58.3% | 75.0% | 80.4% | 28.6% | 86.7% | 45.0% | 64.4% |
| **Controle** | 66.7% | 41.7% | — | 73.3% | 40.4% | 69.6% | 61.7% | 37.5% | 67.3% |
| **Burn** | 5.0% | 25.0% | 26.7% | — | 6.7% | 80.0% | 53.3% | 40.0% | 96.7% |
| **Stall** | 61.7% | 19.6% | 59.6% | 93.3% | — | 49.1% | 79.7% | 48.3% | 78.0% |
| **Ramp** | 45.0% | 71.4% | 30.4% | 20.0% | 50.9% | — | 41.7% | 73.2% | 65.9% |
| **Tempo** | 31.7% | 13.3% | 38.3% | 46.7% | 20.3% | 58.3% | — | 18.3% | 50.0% |
| **Mistura** | 60.0% | 55.0% | 62.5% | 60.0% | 51.7% | 26.8% | 81.7% | — | 45.0% |
| **Eternyr** | 30.0% | 35.6% | 32.7% | 3.3% | 22.0% | 34.1% | 50.0% | 55.0% | — |
