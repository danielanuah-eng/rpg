# Avaliação de balanceamento: BAL11 + Aliados de descarte de Grau descartam 1 a mais

4860 partidas (60 por confronto ordenado), 4859 decididas. Turnos médios 20.7. Critérios em sim/criterios.json.

**Situação: TODOS OS CRITÉRIOS ATINGIDOS** — 20 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ✅ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 20.7 turnos | 16 a 26 |  |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.0% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 52.5% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 54.2% (n=120) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 60.9% em 744 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.94 (2397 × 2554 em 846 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.40 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 14.5% em 76 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 45.8% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 83.6% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 60.0% (n=120) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 77.5% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 63.3% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.10 por partida | ≥ 1 |  |
| ✅ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 99.4% de 523 vitórias | ≥ 75.0% |  |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 52.5% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 75.0% em 84 partidas com 16+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 49.9% | 57.2% | 47.3% | 39.1% | 54.5% | 56.0% | 57.8% | 42.7% | 45.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 22.5% | 45.8% | 95.0% | 47.5% | 37.5% | 63.3% | 35.8% | 51.7% |
| **Midrange** | 77.5% | — | 60.0% | 67.5% | 41.7% | 41.7% | 53.3% | 66.7% | 49.2% |
| **Controle** | 54.2% | 40.0% | — | 44.2% | 65.0% | 41.7% | 36.7% | 55.0% | 41.7% |
| **Burn** | 5.0% | 32.5% | 55.8% | — | 21.7% | 42.5% | 49.6% | 36.7% | 69.2% |
| **Stall** | 52.5% | 58.3% | 35.0% | 78.3% | — | 43.3% | 37.5% | 58.3% | 72.5% |
| **Ramp** | 62.5% | 58.3% | 58.3% | 57.5% | 56.7% | — | 35.0% | 68.3% | 51.7% |
| **Tempo** | 36.7% | 46.7% | 63.3% | 50.4% | 62.5% | 65.0% | — | 70.0% | 67.5% |
| **Mistura** | 64.2% | 33.3% | 45.0% | 63.3% | 41.7% | 31.7% | 30.0% | — | 32.5% |
| **Eternyr** | 48.3% | 50.8% | 58.3% | 30.8% | 27.5% | 48.3% | 32.5% | 67.5% | — |
