# Avaliação de balanceamento: PACOTE (tudo) + baralhos de agente/estado.json

3240 partidas (40 por confronto ordenado), 3116 decididas. Turnos médios 32.3. Critérios em sim/criterios.json.

**Situação: LONGE** — 10 de 20 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ❌ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | Aggro 61.1%, Midrange 62.3%, Controle 59.2%, Mistura 64.2% | ≤ 59.0% | 5.2 pp |
| ❌ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | Tempo 31.8%, Eternyr 28.8% | ≥ 38.0% | 9.2 pp |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 32.3 turnos | 16 a 26 | 24.3% |
| ✅ | Partidas que não terminam: no máximo 5%. | 3.8% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.4% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 75.0% (n=80) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 55.0% em 469 partidas com 18+ turnos | ≥ 55.0% |  |
| ❌ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 1.10 (3333 × 3034 em 536 partidas) | ≤ 1 | 9.9% |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 2 (mediana) | ≤ 4 |  |
| ❌ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.70 cartas | ≤ 2.5 | 8.0% |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 16.9% em 65 partidas com 18+ turnos | ≤ 40.0% |  |
| ❌ | ...mas não é impossível: pelo menos 30% contra o Controle. | 25.0% (n=80) | ≥ 30.0% | 5.0 pp |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.3% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 71.1% (n=76) | ≥ 50.0% |  |
| ✅ | Midrange tem controle suficiente para segurar o Aggro. | 75.0% (n=80) | ≥ 50.0% |  |
| ❌ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 6.3% (n=80) | ≥ 50.0% | 43.8 pp |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 3.99 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 35.6% de 278 vitórias | ≥ 75.0% | 39.4 pp |
| ❌ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 20.0% (n=80) | ≥ 30.0% | 10.0 pp |
| ❌ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 22.9% em 70 partidas com 16+ turnos | ≥ 60.0% | 37.1 pp |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 61.1% | 62.3% | 59.2% | 44.2% | 46.5% | 51.8% | 31.8% | 64.2% | 28.8% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 25.0% | 25.0% | 93.8% | 80.0% | 57.5% | 83.8% | 48.8% | 75.0% |
| **Midrange** | 75.0% | — | 71.1% | 66.3% | 45.0% | 38.8% | 90.0% | 43.8% | 68.8% |
| **Controle** | 75.0% | 28.9% | — | 83.8% | 36.8% | 61.9% | 93.8% | 20.5% | 73.2% |
| **Burn** | 6.3% | 33.8% | 16.3% | — | 15.0% | 86.3% | 52.5% | 51.2% | 92.5% |
| **Stall** | 20.0% | 55.0% | 63.2% | 85.0% | — | 42.3% | 34.3% | 14.3% | 58.3% |
| **Ramp** | 42.5% | 61.3% | 38.1% | 13.8% | 57.7% | — | 61.3% | 57.9% | 81.7% |
| **Tempo** | 16.3% | 10.0% | 6.3% | 47.5% | 65.7% | 38.8% | — | 15.0% | 55.0% |
| **Mistura** | 51.2% | 56.3% | 79.5% | 48.8% | 85.7% | 42.1% | 85.0% | — | 65.0% |
| **Eternyr** | 25.0% | 31.2% | 26.8% | 7.5% | 41.7% | 18.3% | 45.0% | 35.0% | — |
