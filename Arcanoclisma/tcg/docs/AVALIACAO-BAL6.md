# Avaliação de balanceamento: BAL4 + Etéreo + curva do Aggro + Anular + Cataclisma 10

2430 partidas (30 por confronto ordenado), 2430 decididas. Turnos médios 20.7. Critérios em sim/criterios.json.

**Situação: LONGE** — 15 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 62.5%, Midrange 67.3%, Stall 77.1% | ≤ 59.0% | 18.1 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Ramp 29.8%, Eternyr 27.1% | ≥ 38.0% | 10.9 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.7 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 42.6% | ≥ 45.0% e ≤ 55.0% | 2.4 pp |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 16.7% (n=60) | ≥ 35.0% | 18.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 59.7% em 330 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.37 (627 × 1704 em 383 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.22 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 33.3% em 15 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 83.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.3% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 61.7% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 76.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 61.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.01 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 97.3% de 370 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 66.7% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 85.1% em 47 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 62.5% | 67.3% | 42.1% | 47.7% | 77.1% | 29.8% | 55.6% | 40.8% | 27.1% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 23.3% | 83.3% | 91.7% | 33.3% | 73.3% | 70.0% | 50.0% | 75.0% |
| **Midrange** | 76.7% | — | 61.7% | 73.3% | 41.7% | 61.7% | 63.3% | 73.3% | 86.7% |
| **Controle** | 16.7% | 38.3% | — | 33.3% | 26.7% | 68.3% | 38.3% | 46.7% | 68.3% |
| **Burn** | 8.3% | 26.7% | 66.7% | — | 13.3% | 80.0% | 53.3% | 43.3% | 90.0% |
| **Stall** | 66.7% | 58.3% | 73.3% | 86.7% | — | 86.7% | 71.7% | 85.0% | 88.3% |
| **Ramp** | 26.7% | 38.3% | 31.7% | 20.0% | 13.3% | — | 15.0% | 58.3% | 35.0% |
| **Tempo** | 30.0% | 36.7% | 61.7% | 46.7% | 28.3% | 85.0% | — | 66.7% | 90.0% |
| **Mistura** | 50.0% | 26.7% | 53.3% | 56.7% | 15.0% | 41.7% | 33.3% | — | 50.0% |
| **Eternyr** | 25.0% | 13.3% | 31.7% | 10.0% | 11.7% | 65.0% | 10.0% | 50.0% | — |
