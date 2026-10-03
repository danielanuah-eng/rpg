# Avaliação de balanceamento: BAL4 + Anular (Eternyr e Controle)

2430 partidas (30 por confronto ordenado), 2353 decididas. Turnos médios 31.4. Critérios em sim/criterios.json.

**Situação: LONGE** — 13 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 62.9% | ≤ 59.0% | 3.9 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Eternyr 37.5% | ≥ 38.0% | 0.5 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 31.4 turnos | 16 a 26 | 20.8% |
| ✅ | Partidas que não terminam: no máximo 5%. | 3.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 46.9% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 33.3% (n=60) | ≥ 35.0% | 1.7 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 72.0% em 321 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.34 (1213 × 3572 em 384 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.82 cartas | ≤ 2.5 | 12.9% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 13.0% em 23 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 66.7% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.9% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 48.3% (n=60) | ≥ 50.0% | 1.7 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 53.3% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.60 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 69.0% de 245 vitórias | ≥ 75.0% | 6.0 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 61.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 72.5% em 51 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 55.8% | 62.9% | 50.7% | 46.5% | 52.8% | 52.6% | 38.8% | 52.5% | 37.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 66.7% | 95.0% | 38.3% | 55.0% | 68.3% | 40.0% | 65.0% |
| **Midrange** | 81.7% | — | 48.3% | 75.0% | 80.4% | 28.6% | 86.7% | 45.0% | 57.6% |
| **Controle** | 33.3% | 51.7% | — | 30.0% | 83.0% | 53.3% | 46.7% | 56.7% | 50.8% |
| **Burn** | 5.0% | 25.0% | 70.0% | — | 6.7% | 80.0% | 53.3% | 40.0% | 91.7% |
| **Stall** | 61.7% | 19.6% | 17.0% | 93.3% | — | 49.1% | 79.7% | 48.3% | 53.4% |
| **Ramp** | 45.0% | 71.4% | 46.7% | 20.0% | 50.9% | — | 41.7% | 73.2% | 71.7% |
| **Tempo** | 31.7% | 13.3% | 53.3% | 46.7% | 20.3% | 58.3% | — | 18.3% | 68.3% |
| **Mistura** | 60.0% | 55.0% | 43.3% | 60.0% | 51.7% | 26.8% | 81.7% | — | 41.7% |
| **Eternyr** | 35.0% | 42.4% | 49.2% | 8.3% | 46.6% | 28.3% | 31.7% | 58.3% | — |
