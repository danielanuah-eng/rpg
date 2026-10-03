# Avaliação de balanceamento: Rodada 5: BAL sem Cataclisma crescente nem Grau suspenso

2430 partidas (30 por confronto ordenado), 2413 decididas. Turnos médios 29.2. Critérios em sim/criterios.json.

**Situação: LONGE** — 15 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Tempo 62.1% | ≤ 59.0% | 3.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Controle 24.4% | ≥ 38.0% | 13.6 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 29.2 turnos | 16 a 26 | 12.3% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.7% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 54.8% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 36.7% (n=60) | ≥ 35.0% |  |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 32.4% em 361 partidas com 18+ turnos | ≥ 55.0% | 22.6 pp |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.90 (2055 × 2295 em 417 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 1.96 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 38.9% em 36 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 63.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 79.9% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 78.3% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 76.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 91.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.87 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 59.2% de 238 vitórias | ≥ 75.0% | 15.8 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 58.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 89.7% em 39 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 54.8% | 57.2% | 24.4% | 38.1% | 49.9% | 56.2% | 62.1% | 56.3% | 50.9% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 23.3% | 63.3% | 91.7% | 41.7% | 51.7% | 75.0% | 28.3% | 63.3% |
| **Midrange** | 76.7% | — | 78.3% | 53.3% | 71.2% | 38.3% | 53.3% | 45.0% | 41.7% |
| **Controle** | 36.7% | 21.7% | — | 41.7% | 16.7% | 23.3% | 8.3% | 23.3% | 23.7% |
| **Burn** | 8.3% | 46.7% | 58.3% | — | 21.7% | 40.0% | 43.3% | 33.3% | 53.3% |
| **Stall** | 58.3% | 28.8% | 83.3% | 78.3% | — | 28.3% | 30.0% | 35.0% | 56.9% |
| **Ramp** | 48.3% | 61.7% | 76.7% | 60.0% | 71.7% | — | 33.3% | 55.9% | 42.4% |
| **Tempo** | 25.0% | 46.7% | 91.7% | 56.7% | 70.0% | 66.7% | — | 65.0% | 75.0% |
| **Mistura** | 71.7% | 55.0% | 76.7% | 66.7% | 65.0% | 44.1% | 35.0% | — | 36.7% |
| **Eternyr** | 36.7% | 58.3% | 76.3% | 46.7% | 43.1% | 57.6% | 25.0% | 63.3% | — |
