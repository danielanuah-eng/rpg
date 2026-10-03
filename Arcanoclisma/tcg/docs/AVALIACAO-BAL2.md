# Avaliação de balanceamento: Balanceador 2: + remoção que cresce com o Grau

2430 partidas (30 por confronto ordenado), 2350 decididas. Turnos médios 31.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 11 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 61.5%, Midrange 63.1% | ≤ 59.0% | 4.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 30.3%, Eternyr 37.8% | ≥ 38.0% | 7.7 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 31.3 turnos | 16 a 26 | 20.3% |
| ✅ | Partidas que não terminam: no máximo 5%. | 3.3% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.7% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 72.9% em 329 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.39 (1411 × 3624 em 390 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.80 cartas | ≤ 2.5 | 11.8% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 20.8% em 24 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 45.0% (n=60) | ≥ 50.0% | 5.0 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 38.3% (n=60) | ≥ 50.0% | 11.7 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.29 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 59.5% de 237 vitórias | ≥ 75.0% | 15.5 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 40.0% (n=60) | ≥ 30.0% |  |
| ❌ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 46.2% em 52 partidas com 16+ turnos | ≥ 60.0% | 13.8 pp |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 61.5% | 63.1% | 51.6% | 46.9% | 52.2% | 53.5% | 30.3% | 53.2% | 37.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 68.3% | 95.0% | 60.0% | 55.0% | 85.0% | 40.0% | 70.0% |
| **Midrange** | 81.7% | — | 45.0% | 75.0% | 68.5% | 28.6% | 96.7% | 45.0% | 64.4% |
| **Controle** | 31.7% | 55.0% | — | 30.0% | 79.7% | 45.0% | 61.7% | 70.0% | 39.7% |
| **Burn** | 5.0% | 25.0% | 70.0% | — | 10.0% | 80.0% | 48.3% | 40.0% | 96.7% |
| **Stall** | 40.0% | 31.5% | 20.3% | 90.0% | — | 51.1% | 79.7% | 37.7% | 67.2% |
| **Ramp** | 45.0% | 71.4% | 55.0% | 20.0% | 48.9% | — | 48.3% | 73.2% | 65.9% |
| **Tempo** | 15.0% | 3.3% | 38.3% | 51.7% | 20.3% | 51.7% | — | 13.3% | 48.3% |
| **Mistura** | 60.0% | 55.0% | 30.0% | 60.0% | 62.3% | 26.8% | 86.7% | — | 45.0% |
| **Eternyr** | 30.0% | 35.6% | 60.3% | 3.3% | 32.8% | 34.1% | 51.7% | 55.0% | — |
