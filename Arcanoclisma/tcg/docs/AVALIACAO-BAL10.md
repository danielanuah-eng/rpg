# Avaliação de balanceamento: BAL9 + segundo jogador começa com 1 Arcano

3240 partidas (40 por confronto ordenado), 3237 decididas. Turnos médios 19.8. Critérios em sim/criterios.json.

**Situação: LONGE** — 16 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Ramp 61.8% | ≤ 59.0% | 2.8 pp |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 19.8 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.1% | ≤ 5.0% |  |
| ❌ | Quem começa vence entre 45% e 55%. | 56.7% | ≥ 45.0% e ≤ 55.0% | 1.7 pp |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 55.0% (n=80) | ≥ 35.0% |  |
| ❌ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 52.1% em 430 partidas com 18+ turnos | ≥ 55.0% | 2.9 pp |
| ❌ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 1.08 (1924 × 1787 em 520 partidas) | ≤ 1 | 7.7% |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.22 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 15.6% em 45 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 45.0% (n=80) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 85.7% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 58.8% (n=80) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 72.5% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 65.0% (n=80) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.99 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 95.4% de 345 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 48.8% (n=80) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 73.6% em 53 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 50.9% | 57.9% | 44.7% | 40.0% | 53.9% | 61.8% | 52.7% | 39.8% | 48.3% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 27.5% | 45.0% | 97.5% | 51.2% | 35.0% | 70.0% | 30.0% | 51.2% |
| **Midrange** | 72.5% | — | 58.8% | 62.0% | 63.7% | 28.7% | 70.0% | 62.5% | 45.0% |
| **Controle** | 55.0% | 41.3% | — | 57.5% | 22.5% | 32.9% | 35.0% | 62.5% | 51.2% |
| **Burn** | 2.5% | 38.0% | 42.5% | — | 21.3% | 41.8% | 66.3% | 46.3% | 61.3% |
| **Stall** | 48.8% | 36.3% | 77.5% | 78.8% | — | 31.3% | 33.8% | 57.5% | 67.5% |
| **Ramp** | 65.0% | 71.3% | 67.1% | 58.2% | 68.8% | — | 38.8% | 78.8% | 46.3% |
| **Tempo** | 30.0% | 30.0% | 65.0% | 33.8% | 66.3% | 61.3% | — | 75.0% | 60.0% |
| **Mistura** | 70.0% | 37.5% | 37.5% | 53.8% | 42.5% | 21.3% | 25.0% | — | 31.3% |
| **Eternyr** | 48.8% | 55.0% | 48.8% | 38.8% | 32.5% | 53.8% | 40.0% | 68.8% | — |
