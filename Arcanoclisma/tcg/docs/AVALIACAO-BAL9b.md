# Avaliação de balanceamento: BAL8 + segundo jogador sem Grau suspenso, com regra do primeiro Conjurador

3240 partidas (40 por confronto ordenado), 3237 decididas. Turnos médios 20.1. Critérios em sim/criterios.json.

**Situação: LONGE** — 16 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Ramp 59.4% | ≤ 59.0% | 0.4 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.1 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.1% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 42.9% | ≥ 45.0% e ≤ 55.0% | 2.1 pp |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 43.8% (n=80) | ≥ 35.0% |  |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 51.4% em 434 partidas com 18+ turnos | ≥ 55.0% | 3.6 pp |
| ❌ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 1.12 (1844 × 1641 em 539 partidas) | ≤ 1 | 12.4% |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.18 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 20.5% em 39 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 56.3% (n=80) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 80.2% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 61.3% (n=80) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 62.5% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 51.2% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.11 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 95.4% de 351 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 53.8% (n=80) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 78.2% em 55 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 56.6% | 55.0% | 43.8% | 42.3% | 54.8% | 59.4% | 52.4% | 42.0% | 43.6% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 37.5% | 56.3% | 95.0% | 46.3% | 45.0% | 63.7% | 41.3% | 67.5% |
| **Midrange** | 62.5% | — | 61.3% | 63.7% | 52.5% | 32.5% | 62.5% | 60.0% | 45.0% |
| **Controle** | 43.8% | 38.8% | — | 55.1% | 21.3% | 42.5% | 48.8% | 57.5% | 42.5% |
| **Burn** | 5.0% | 36.3% | 44.9% | — | 25.0% | 43.8% | 66.3% | 51.2% | 66.3% |
| **Stall** | 53.8% | 47.5% | 78.8% | 75.0% | — | 28.7% | 33.8% | 57.5% | 63.7% |
| **Ramp** | 55.0% | 67.5% | 57.5% | 56.3% | 71.3% | — | 32.9% | 76.3% | 58.8% |
| **Tempo** | 36.3% | 37.5% | 51.2% | 33.8% | 66.3% | 67.1% | — | 65.0% | 62.5% |
| **Mistura** | 58.8% | 40.0% | 42.5% | 48.8% | 42.5% | 23.8% | 35.0% | — | 45.0% |
| **Eternyr** | 32.5% | 55.0% | 57.5% | 33.8% | 36.3% | 41.3% | 37.5% | 55.0% | — |
