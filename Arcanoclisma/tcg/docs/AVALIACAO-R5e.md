# Avaliação de balanceamento: R5 + Aliados Etéreos caros (finalizadores)

2430 partidas (30 por confronto ordenado), 2421 decididas. Turnos médios 27.0. Critérios em sim/criterios.json.

**Situação: LONGE** — 14 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 63.1% | ≤ 59.0% | 4.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Controle 28.1%, Burn 37.9% | ≥ 38.0% | 9.9 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 27.0 turnos | 16 a 26 | 4.0% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.4% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 56.0% | ≥ 45.0% e ≤ 55.0% | 1.0 pp |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 43.3% (n=60) | ≥ 35.0% |  |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 37.7% em 353 partidas com 18+ turnos | ≥ 55.0% | 17.3 pp |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.96 (2234 × 2316 em 402 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 1.95 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 10.3% em 29 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 56.7% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.4% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 73.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 76.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.06 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 66.4% de 250 vitórias | ≥ 75.0% | 8.6 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 90.5% em 42 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 52.3% | 63.1% | 28.1% | 37.9% | 52.2% | 55.5% | 47.6% | 57.1% | 56.1% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 23.3% | 56.7% | 91.7% | 36.7% | 48.3% | 75.0% | 26.7% | 60.0% |
| **Midrange** | 76.7% | — | 73.3% | 73.3% | 63.3% | 58.3% | 63.3% | 58.3% | 38.3% |
| **Controle** | 43.3% | 26.7% | — | 40.0% | 26.7% | 18.3% | 48.3% | 15.0% | 6.8% |
| **Burn** | 8.3% | 26.7% | 60.0% | — | 23.3% | 45.0% | 43.3% | 38.3% | 58.3% |
| **Stall** | 63.3% | 36.7% | 73.3% | 76.7% | — | 31.7% | 61.7% | 40.0% | 34.5% |
| **Ramp** | 51.7% | 41.7% | 81.7% | 55.0% | 68.3% | — | 38.3% | 57.6% | 50.0% |
| **Tempo** | 25.0% | 36.7% | 51.7% | 56.7% | 38.3% | 61.7% | — | 49.2% | 61.7% |
| **Mistura** | 73.3% | 41.7% | 85.0% | 61.7% | 60.0% | 42.4% | 50.8% | — | 41.7% |
| **Eternyr** | 40.0% | 61.7% | 93.2% | 41.7% | 65.5% | 50.0% | 38.3% | 58.3% | — |
