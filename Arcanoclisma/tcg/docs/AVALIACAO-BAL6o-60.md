# Avaliação de balanceamento: teste: passo 14 com o Controle trocando dano ao Conjurador e 1 Fera por Eternyr: comprar 3

4860 partidas (60 por confronto ordenado), 4837 decididas. Turnos médios 26.5. Critérios em sim/criterios.json.

**Situação: PRÓXIMO (pedir permissão ao autor)** — 21 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 26.5 turnos | 16 a 26 | 2.1% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.5% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.6% | ≥ 45.0% e ≤ 55.0% |  |
| ✅ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 36.7% (n=120) | ≥ 35.0% |  |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 61.8% em 685 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 57.1% de 718 partidas (diferença média +1.0 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 34.2 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.53 (4272 × 8108 em 792 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.26 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 4.4% em 45 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 63.3% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 81.9% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 55.8% (n=120) | ≥ 50.0% |  |
| ❌ | Midrange tem controle suficiente para segurar o Aggro. | 48.3% (n=120) | ≥ 50.0% | 1.7 pp |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 70.8% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.04 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 72.9% de 469 vitórias | ≥ 75.0% | 2.1 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 86.4% em 88 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 15.1% em 392 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 78.4% em 940 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 57.1% | 54.5% | 46.4% | 43.6% | 49.3% | 43.4% | 53.6% | 47.7% | 54.5% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 51.7% | 63.3% | 95.8% | 36.7% | 63.3% | 49.2% | 35.8% | 60.8% |
| **Midrange** | 48.3% | — | 55.8% | 65.0% | 75.8% | 41.7% | 61.7% | 49.2% | 38.3% |
| **Controle** | 36.7% | 44.2% | — | 50.8% | 54.6% | 62.5% | 29.2% | 61.7% | 31.3% |
| **Burn** | 4.2% | 35.0% | 49.2% | — | 14.2% | 86.7% | 49.6% | 37.5% | 72.5% |
| **Stall** | 63.3% | 24.2% | 45.4% | 85.8% | — | 49.6% | 45.0% | 30.0% | 50.9% |
| **Ramp** | 36.7% | 58.3% | 37.5% | 13.3% | 50.4% | — | 32.5% | 82.5% | 36.2% |
| **Tempo** | 50.8% | 38.3% | 70.8% | 50.4% | 55.0% | 67.5% | — | 43.3% | 52.5% |
| **Mistura** | 64.2% | 50.8% | 38.3% | 62.5% | 70.0% | 17.5% | 56.7% | — | 21.7% |
| **Eternyr** | 39.2% | 61.7% | 68.8% | 27.5% | 49.1% | 63.8% | 47.5% | 78.3% | — |
