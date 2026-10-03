# Avaliação de balanceamento: BAL6 com Cataclisma desde o turno 8

2430 partidas (30 por confronto ordenado), 2430 decididas. Turnos médios 18.7. Critérios em sim/criterios.json.

**Situação: LONGE** — 14 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 65.2%, Midrange 73.8%, Stall 73.8% | ≤ 59.0% | 14.8 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Controle 36.5%, Ramp 26.0%, Eternyr 22.5% | ≥ 38.0% | 15.5 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 18.7 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 41.6% | ≥ 45.0% e ≤ 55.0% | 3.4 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 11.7% (n=60) | ≥ 35.0% | 23.3 pp |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 53.0% em 317 partidas com 18+ turnos | ≥ 55.0% | 2.0 pp |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.35 (489 × 1380 em 383 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.22 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 46.2% em 13 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 88.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.9% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 78.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 76.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 63.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.76 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 97.5% de 354 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 80.9% em 47 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 65.2% | 73.8% | 36.5% | 52.7% | 73.8% | 26.0% | 55.4% | 44.2% | 22.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 23.3% | 88.3% | 90.0% | 36.7% | 78.3% | 68.3% | 58.3% | 78.3% |
| **Midrange** | 76.7% | — | 78.3% | 70.0% | 45.0% | 85.0% | 65.0% | 71.7% | 98.3% |
| **Controle** | 11.7% | 21.7% | — | 26.7% | 25.0% | 68.3% | 36.7% | 35.0% | 66.7% |
| **Burn** | 10.0% | 30.0% | 73.3% | — | 23.3% | 88.3% | 58.3% | 45.0% | 93.3% |
| **Stall** | 63.3% | 55.0% | 75.0% | 76.7% | — | 83.3% | 71.7% | 75.0% | 90.0% |
| **Ramp** | 21.7% | 15.0% | 31.7% | 11.7% | 16.7% | — | 16.7% | 51.7% | 43.3% |
| **Tempo** | 31.7% | 35.0% | 63.3% | 41.7% | 28.3% | 83.3% | — | 70.0% | 90.0% |
| **Mistura** | 41.7% | 28.3% | 65.0% | 55.0% | 25.0% | 48.3% | 30.0% | — | 60.0% |
| **Eternyr** | 21.7% | 1.7% | 33.3% | 6.7% | 10.0% | 56.7% | 10.0% | 40.0% | — |
