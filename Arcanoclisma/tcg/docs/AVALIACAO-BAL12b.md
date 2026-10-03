# Avaliação de balanceamento: BAL11 + geração de Arcano cara sem desconto (1,5 PP)

4860 partidas (60 por confronto ordenado), 4856 decididas. Turnos médios 20.6. Critérios em sim/criterios.json.

**Situação: PRÓXIMO (pedir permissão ao autor)** — 18 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Ramp 61.9% | ≤ 59.0% | 2.9 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Burn 37.2% | ≥ 38.0% | 0.8 pp |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.6 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.1% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 52.8% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 49.2% (n=120) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 61.1% em 761 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.89 (2340 × 2636 em 845 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.37 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 21.3% em 75 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 50.8% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 84.1% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 59.2% (n=120) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 75.8% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 52.5% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.17 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 99.4% de 502 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 52.5% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 76.8% em 82 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 49.9% | 55.9% | 48.5% | 37.2% | 52.3% | 61.9% | 55.7% | 42.8% | 45.7% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 24.2% | 50.8% | 95.0% | 47.5% | 37.5% | 63.3% | 28.3% | 52.5% |
| **Midrange** | 75.8% | — | 59.2% | 68.3% | 45.8% | 31.7% | 53.3% | 65.8% | 47.5% |
| **Controle** | 49.2% | 40.8% | — | 50.8% | 63.3% | 35.0% | 47.5% | 59.2% | 42.5% |
| **Burn** | 5.0% | 31.7% | 49.2% | — | 22.5% | 35.6% | 51.3% | 39.2% | 63.3% |
| **Stall** | 52.5% | 54.2% | 36.7% | 77.5% | — | 32.5% | 38.3% | 59.2% | 67.5% |
| **Ramp** | 62.5% | 68.3% | 65.0% | 64.4% | 67.5% | — | 39.2% | 70.8% | 57.5% |
| **Tempo** | 36.7% | 46.7% | 52.5% | 48.7% | 61.7% | 60.8% | — | 68.3% | 70.0% |
| **Mistura** | 71.7% | 34.2% | 40.8% | 60.8% | 40.8% | 29.2% | 31.7% | — | 33.3% |
| **Eternyr** | 47.5% | 52.5% | 57.5% | 36.7% | 32.5% | 42.5% | 30.0% | 66.7% | — |
