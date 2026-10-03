# Avaliação de balanceamento: Balanceador 1: + varreduras

2430 partidas (30 por confronto ordenado), 2376 decididas. Turnos médios 31.8. Critérios em sim/criterios.json.

**Situação: LONGE** — 12 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 60.6%, Midrange 60.3% | ≤ 59.0% | 1.6 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 29.4%, Eternyr 36.4% | ≥ 38.0% | 8.6 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 31.8 turnos | 16 a 26 | 22.1% |
| ✅ | Partidas que não terminam: no máximo 5%. | 2.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.2% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 30.0% (n=60) | ≥ 35.0% | 5.0 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 70.4% em 307 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.43 (1342 × 3107 em 372 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.76 cartas | ≤ 2.5 | 10.3% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 10.0% em 20 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 70.0% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.6% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 45.8% (n=59) | ≥ 50.0% | 4.2 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 41.7% (n=60) | ≥ 50.0% | 8.3 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.42 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 57.8% de 275 vitórias | ≥ 75.0% | 17.2 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 60.0% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 69.2% em 52 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 60.6% | 60.3% | 46.7% | 48.3% | 57.7% | 55.3% | 29.4% | 55.2% | 36.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 70.0% | 95.0% | 40.0% | 55.0% | 85.0% | 40.0% | 81.7% |
| **Midrange** | 81.7% | — | 45.8% | 75.0% | 46.7% | 28.6% | 96.7% | 45.0% | 63.3% |
| **Controle** | 30.0% | 54.2% | — | 18.3% | 76.3% | 55.0% | 58.3% | 50.0% | 31.7% |
| **Burn** | 5.0% | 25.0% | 81.7% | — | 10.0% | 80.0% | 48.3% | 40.0% | 96.7% |
| **Stall** | 60.0% | 53.3% | 23.7% | 90.0% | — | 36.8% | 83.3% | 43.3% | 71.2% |
| **Ramp** | 45.0% | 71.4% | 45.0% | 20.0% | 63.2% | — | 48.3% | 73.2% | 76.1% |
| **Tempo** | 15.0% | 3.3% | 41.7% | 51.7% | 16.7% | 51.7% | — | 13.3% | 41.7% |
| **Mistura** | 60.0% | 55.0% | 50.0% | 60.0% | 56.7% | 26.8% | 86.7% | — | 46.7% |
| **Eternyr** | 18.3% | 36.7% | 68.3% | 3.3% | 28.8% | 23.9% | 58.3% | 53.3% | — |
