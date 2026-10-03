# Avaliação de balanceamento: BAL9 + segundo jogador compra 1 carta a mais

3240 partidas (40 por confronto ordenado), 3235 decididas. Turnos médios 20.0. Critérios em sim/criterios.json.

**Situação: LONGE** — 17 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Ramp 60.4% | ≤ 59.0% | 1.4 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.0 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.2% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 52.7% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 61.3% (n=80) | ≥ 35.0% |  |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 51.7% em 429 partidas com 18+ turnos | ≥ 55.0% | 3.3 pp |
| ❌ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 1.11 (2032 × 1823 em 533 partidas) | ≤ 1 | 11.5% |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.37 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 8.3% em 48 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 38.8% (n=80) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 84.2% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 58.8% (n=80) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 71.3% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 68.8% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.07 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 98.0% de 354 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 58.8% (n=80) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 85.5% em 55 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 47.8% | 54.5% | 44.8% | 39.0% | 55.3% | 60.4% | 56.6% | 42.7% | 48.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 28.7% | 38.8% | 95.0% | 41.3% | 33.8% | 70.0% | 23.8% | 51.2% |
| **Midrange** | 71.3% | — | 58.8% | 62.0% | 53.8% | 23.8% | 60.0% | 66.3% | 40.0% |
| **Controle** | 61.3% | 41.3% | — | 61.3% | 26.3% | 38.8% | 31.3% | 55.0% | 43.8% |
| **Burn** | 5.0% | 38.0% | 38.8% | — | 23.8% | 36.7% | 63.3% | 46.8% | 59.5% |
| **Stall** | 58.8% | 46.3% | 73.8% | 76.3% | — | 33.8% | 28.7% | 57.5% | 67.5% |
| **Ramp** | 66.3% | 76.3% | 61.3% | 63.3% | 66.3% | — | 26.3% | 75.0% | 48.8% |
| **Tempo** | 30.0% | 40.0% | 68.8% | 36.7% | 71.3% | 73.8% | — | 67.5% | 65.0% |
| **Mistura** | 76.3% | 33.8% | 45.0% | 53.2% | 42.5% | 25.0% | 32.5% | — | 33.8% |
| **Eternyr** | 48.8% | 60.0% | 56.3% | 40.5% | 32.5% | 51.2% | 35.0% | 66.3% | — |
