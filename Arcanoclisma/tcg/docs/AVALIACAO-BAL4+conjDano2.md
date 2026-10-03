# Avaliação de balanceamento: BAL4 + Conjurador com dano básico 2

2430 partidas (30 por confronto ordenado), 2366 decididas. Turnos médios 28.7. Critérios em sim/criterios.json.

**Situação: LONGE** — 12 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 65.0%, Midrange 63.8% | ≤ 59.0% | 6.0 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Eternyr 30.3% | ≥ 38.0% | 7.7 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 28.7 turnos | 16 a 26 | 10.4% |
| ✅ | Partidas que não terminam: no máximo 5%. | 2.6% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 44.5% | ≥ 45.0% e ≤ 55.0% | 0.5 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 20.0% (n=60) | ≥ 35.0% | 15.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 70.3% em 276 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.41 (1178 × 2855 em 348 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 3 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.89 cartas | ≤ 2.5 | 15.5% |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 14.3% em 14 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 80.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 87.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 51.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 70.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 75.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.07 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 73.6% de 265 vitórias | ≥ 75.0% | 1.4 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 66.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 81.6% em 49 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 65.0% | 63.8% | 41.7% | 54.3% | 56.8% | 45.0% | 42.4% | 50.8% | 30.3% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 30.0% | 80.0% | 90.0% | 33.3% | 65.0% | 76.3% | 61.7% | 83.3% |
| **Midrange** | 70.0% | — | 51.7% | 67.2% | 80.7% | 39.7% | 83.3% | 48.3% | 69.5% |
| **Controle** | 20.0% | 48.3% | — | 13.3% | 77.6% | 50.0% | 25.0% | 58.3% | 41.4% |
| **Burn** | 10.0% | 32.8% | 86.7% | — | 11.7% | 86.7% | 60.0% | 46.7% | 100.0% |
| **Stall** | 66.7% | 19.3% | 22.4% | 88.3% | — | 59.3% | 69.5% | 45.6% | 83.1% |
| **Ramp** | 35.0% | 60.3% | 50.0% | 13.3% | 40.7% | — | 26.7% | 64.8% | 68.8% |
| **Tempo** | 23.7% | 16.7% | 75.0% | 40.0% | 30.5% | 73.3% | — | 21.7% | 58.3% |
| **Mistura** | 38.3% | 51.7% | 41.7% | 53.3% | 54.4% | 35.2% | 78.3% | — | 53.3% |
| **Eternyr** | 16.7% | 30.5% | 58.6% | 0.0% | 16.9% | 31.3% | 41.7% | 46.7% | — |
