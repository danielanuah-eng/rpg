# Avaliação de balanceamento: BAL4 + relógio de impasse 4

2430 partidas (30 por confronto ordenado), 2362 decididas. Turnos médios 29.6. Critérios em sim/criterios.json.

**Situação: LONGE** — 12 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 64.5% | ≤ 59.0% | 5.5 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Eternyr 34.3% | ≥ 38.0% | 3.7 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 29.6 turnos | 16 a 26 | 14.0% |
| ✅ | Partidas que não terminam: no máximo 5%. | 2.8% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 43.7% | ≥ 45.0% e ≤ 55.0% | 1.3 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 31.7% (n=60) | ≥ 35.0% | 3.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 73.4% em 293 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.36 (1173 × 3256 em 343 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.86 cartas | ≤ 2.5 | 14.3% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 20.8% em 24 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 46.7% (n=60) | ≥ 50.0% | 3.3 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 66.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.42 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 64.9% de 262 vitórias | ≥ 75.0% | 10.1 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 70.6% em 51 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 57.1% | 64.5% | 48.4% | 47.3% | 56.0% | 52.1% | 38.4% | 51.9% | 34.3% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 68.3% | 95.0% | 40.0% | 55.0% | 68.3% | 40.0% | 71.7% |
| **Midrange** | 81.7% | — | 46.7% | 75.0% | 82.4% | 32.1% | 85.0% | 43.3% | 69.5% |
| **Controle** | 31.7% | 53.3% | — | 28.3% | 84.5% | 43.3% | 33.3% | 70.0% | 43.1% |
| **Burn** | 5.0% | 25.0% | 71.7% | — | 6.7% | 80.0% | 53.3% | 40.0% | 96.7% |
| **Stall** | 60.0% | 17.6% | 15.5% | 93.3% | — | 49.1% | 81.4% | 46.6% | 84.7% |
| **Ramp** | 45.0% | 67.9% | 56.7% | 20.0% | 50.9% | — | 40.0% | 73.2% | 63.0% |
| **Tempo** | 31.7% | 15.0% | 66.7% | 46.7% | 18.6% | 60.0% | — | 16.7% | 51.7% |
| **Mistura** | 60.0% | 56.7% | 30.0% | 60.0% | 53.4% | 26.8% | 83.3% | — | 45.0% |
| **Eternyr** | 28.3% | 30.5% | 56.9% | 3.3% | 15.3% | 37.0% | 48.3% | 55.0% | — |
