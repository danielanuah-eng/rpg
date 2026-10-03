# Avaliação de balanceamento: R5 + jogador automático prevê bloqueios

2430 partidas (30 por confronto ordenado), 2414 decididas. Turnos médios 27.4. Critérios em sim/criterios.json.

**Situação: LONGE** — 12 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 79.4%, Midrange 59.7%, Tempo 62.3% | ≤ 59.0% | 20.4 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Controle 15.7% | ≥ 38.0% | 22.3 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 27.4 turnos | 16 a 26 | 5.5% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.7% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 54.3% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 6.7% (n=60) | ≥ 35.0% | 28.3 pp |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 23.8% em 315 partidas com 18+ turnos | ≥ 55.0% | 31.2 pp |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.83 (1788 × 2148 em 394 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 1.96 cartas | ≤ 2.5 |  |
| ❌ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. (amostra pequena) | 71.4% em 14 partidas com 18+ turnos | ≤ 40.0% | 31.4 pp |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 93.3% (n=60) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 80.3% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 95.0% (n=60) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 60.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 90.0% (n=60) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.61 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 53.7% de 203 vitórias | ≥ 75.0% | 21.3 pp |
| ❌ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 28.3% (n=60) | ≥ 30.0% | 1.7 pp |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 60.7% em 28 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 79.4% | 59.7% | 15.7% | 39.2% | 42.4% | 50.0% | 62.3% | 54.9% | 46.4% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 40.0% | 93.3% | 91.7% | 71.7% | 91.7% | 91.7% | 66.7% | 88.3% |
| **Midrange** | 60.0% | — | 95.0% | 56.7% | 78.0% | 51.7% | 51.7% | 38.3% | 46.7% |
| **Controle** | 6.7% | 5.0% | — | 36.7% | 16.7% | 23.3% | 10.0% | 13.3% | 14.0% |
| **Burn** | 8.3% | 43.3% | 63.3% | — | 21.7% | 43.3% | 45.0% | 36.7% | 51.7% |
| **Stall** | 28.3% | 22.0% | 83.3% | 78.3% | — | 13.3% | 28.3% | 33.3% | 52.5% |
| **Ramp** | 8.3% | 48.3% | 76.7% | 56.7% | 86.7% | — | 28.3% | 47.5% | 47.5% |
| **Tempo** | 8.3% | 48.3% | 90.0% | 55.0% | 71.7% | 71.7% | — | 71.7% | 81.7% |
| **Mistura** | 33.3% | 61.7% | 86.7% | 63.3% | 66.7% | 52.5% | 28.3% | — | 46.7% |
| **Eternyr** | 11.7% | 53.3% | 86.0% | 48.3% | 47.5% | 52.5% | 18.3% | 53.3% | — |
