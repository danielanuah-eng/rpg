# Avaliação de balanceamento: BAL4 + curva do Aggro mais baixa

2430 partidas (30 por confronto ordenado), 2358 decididas. Turnos médios 30.7. Critérios em sim/criterios.json.

**Situação: LONGE** — 13 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Midrange 63.3% | ≤ 59.0% | 4.3 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 37.2%, Eternyr 36.5% | ≥ 38.0% | 1.5 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 30.7 turnos | 16 a 26 | 18.1% |
| ✅ | Partidas que não terminam: no máximo 5%. | 3.0% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 46.1% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 26.7% (n=60) | ≥ 35.0% | 8.3 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 73.6% em 318 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.37 (1332 × 3594 em 373 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.28 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 0.0% em 16 partidas com 18+ turnos | ≤ 40.0% | 10.0 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 73.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.7% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ❌ | Midrange tem early game para pressionar o Controle. | 45.0% (n=60) | ≥ 50.0% | 5.0 pp |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 81.7% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 55.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 4.58 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 69.5% de 259 vitórias | ≥ 75.0% | 5.5 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 68.3% (n=60) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 87.2% em 47 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 57.0% | 63.3% | 50.6% | 47.3% | 55.4% | 51.0% | 37.2% | 51.7% | 36.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 18.3% | 73.3% | 93.3% | 31.7% | 70.0% | 64.4% | 36.7% | 68.3% |
| **Midrange** | 81.7% | — | 45.0% | 75.0% | 80.4% | 28.6% | 86.7% | 45.0% | 64.4% |
| **Controle** | 26.7% | 55.0% | — | 30.0% | 93.1% | 45.0% | 45.0% | 70.0% | 39.7% |
| **Burn** | 6.7% | 25.0% | 70.0% | — | 6.7% | 80.0% | 53.3% | 40.0% | 96.7% |
| **Stall** | 68.3% | 19.6% | 6.9% | 93.3% | — | 49.1% | 79.7% | 48.3% | 78.0% |
| **Ramp** | 30.0% | 71.4% | 55.0% | 20.0% | 50.9% | — | 41.7% | 73.2% | 65.9% |
| **Tempo** | 35.6% | 13.3% | 55.0% | 46.7% | 20.3% | 58.3% | — | 18.3% | 50.0% |
| **Mistura** | 63.3% | 55.0% | 30.0% | 60.0% | 51.7% | 26.8% | 81.7% | — | 45.0% |
| **Eternyr** | 31.7% | 35.6% | 60.3% | 3.3% | 22.0% | 34.1% | 50.0% | 55.0% | — |
