# Avaliação de balanceamento: BAL11 + Aliados de descarte de Grau descartam 1 a mais

8100 partidas (100 por confronto ordenado), 8098 decididas. Turnos médios 20.8. Critérios em sim/criterios.json.

**Situação: TODOS OS CRITÉRIOS ATINGIDOS** — 20 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.8 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 50.2% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 53.0% (n=200) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 63.2% em 1236 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.95 (3924 × 4151 em 1398 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.33 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 14.5% em 124 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 47.0% (n=200) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 83.8% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 52.5% (n=200) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 84.0% (n=200) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 70.0% (n=200) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.19 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 98.9% de 877 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 62.5% (n=200) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 80.6% em 155 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 48.6% | 58.1% | 48.9% | 39.3% | 54.8% | 52.1% | 57.0% | 45.3% | 45.9% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 16.0% | 47.0% | 96.5% | 37.5% | 39.5% | 71.0% | 30.0% | 51.0% |
| **Midrange** | 84.0% | — | 52.5% | 67.0% | 48.0% | 44.0% | 57.0% | 62.5% | 49.5% |
| **Controle** | 53.0% | 47.5% | — | 50.0% | 72.5% | 39.5% | 30.0% | 54.0% | 45.0% |
| **Burn** | 3.5% | 33.0% | 50.0% | — | 24.5% | 48.5% | 54.5% | 33.0% | 67.5% |
| **Stall** | 62.5% | 52.0% | 27.5% | 75.5% | — | 51.5% | 35.5% | 62.0% | 72.0% |
| **Ramp** | 60.5% | 56.0% | 60.5% | 51.5% | 48.5% | — | 30.5% | 69.0% | 40.5% |
| **Tempo** | 29.0% | 43.0% | 70.0% | 45.5% | 64.5% | 69.5% | — | 61.8% | 72.5% |
| **Mistura** | 70.0% | 37.5% | 46.0% | 67.0% | 38.0% | 31.0% | 38.2% | — | 34.5% |
| **Eternyr** | 49.0% | 50.5% | 55.0% | 32.5% | 28.0% | 59.5% | 27.5% | 65.5% | — |
