# Avaliação de balanceamento: Balanceador 5: + relógio de impasse 3

2430 partidas (30 por confronto ordenado), 2374 decididas. Turnos médios 26.2. Critérios em sim/criterios.json.

**Situação: LONGE** — 13 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 65.5%, Stall 59.4% | ≤ 59.0% | 6.5 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Eternyr 30.7% | ≥ 38.0% | 7.3 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 26.2 turnos | 16 a 26 | 0.7% |
| ✅ | Partidas que não terminam: no máximo 5%. | 2.3% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 40.9% | ≥ 45.0% e ≤ 55.0% | 4.1 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 74.4% em 227 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.35 (866 × 2491 em 264 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.86 cartas | ≤ 2.5 | 14.5% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 14.3% em 21 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.5% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 50.0% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 70.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.17 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 58.8% de 279 vitórias | ≥ 75.0% | 16.2 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 70.6% em 51 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 56.7% | 65.5% | 43.9% | 48.1% | 59.4% | 52.2% | 39.6% | 53.8% | 30.7% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 68.3% | 95.0% | 40.0% | 55.0% | 68.3% | 36.7% | 71.7% |
| **Midrange** | 81.7% | — | 50.0% | 75.0% | 76.9% | 37.5% | 85.0% | 43.3% | 74.6% |
| **Controle** | 31.7% | 50.0% | — | 23.3% | 67.2% | 36.7% | 30.0% | 58.3% | 54.2% |
| **Burn** | 5.0% | 25.0% | 76.7% | — | 6.7% | 80.0% | 53.3% | 41.7% | 96.7% |
| **Stall** | 60.0% | 23.1% | 32.8% | 93.3% | — | 50.0% | 79.7% | 46.6% | 90.0% |
| **Ramp** | 45.0% | 62.5% | 63.3% | 20.0% | 50.0% | — | 40.0% | 71.4% | 65.3% |
| **Tempo** | 31.7% | 15.0% | 70.0% | 46.7% | 20.3% | 60.0% | — | 18.3% | 55.0% |
| **Mistura** | 63.3% | 56.7% | 41.7% | 58.3% | 53.4% | 28.6% | 81.7% | — | 46.7% |
| **Eternyr** | 28.3% | 25.4% | 45.8% | 3.3% | 10.0% | 34.7% | 45.0% | 53.3% | — |
