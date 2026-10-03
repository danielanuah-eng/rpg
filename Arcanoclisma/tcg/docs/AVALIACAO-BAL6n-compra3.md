# Avaliação de balanceamento: teste: passo 14 com o Controle trocando 2 Feras de cerco por 2 Eternyr: comprar 3

4860 partidas (60 por confronto ordenado), 4838 decididas. Turnos médios 26.5. Critérios em sim/criterios.json.

**Situação: LONGE** — 20 de 24 critérios obrigatórios atingidos.

## Critérios

| | Critério | Medido | Alvo | Distância |
|---|---|---|---|---|
| ✅ | Nenhuma montagem passa de 59% de vitória média (confrontos podem ser desiguais). | todas dentro | ≤ 59.0% |  |
| ✅ | Toda montagem fica jogável (pelo menos 38% de vitória média). Proposta do Claude. | todas dentro | ≥ 38.0% |  |
| ❌ | Partidas por volta de 20 turnos totais (média entre 16 e 26). | 26.5 turnos | 16 a 26 | 1.8% |
| ✅ | Partidas que não terminam: no máximo 5%. | 0.5% | ≤ 5.0% |  |
| ✅ | Quem começa vence entre 45% e 55%. | 45.9% | ≥ 45.0% e ≤ 55.0% |  |
| ❌ | Controle tem recursos para controlar o Aggro (o Aggro pode explodir às vezes). | 32.5% (n=120) | ≥ 35.0% | 2.5 pp |
| ✅ | Remoções e recursos do Controle pesam mais quanto mais o jogo passa: vence pelo menos 55% das partidas longas. | 61.4% em 673 partidas com 18+ turnos | ≥ 55.0% |  |
| ✅ | Controle ganha em recurso: no turno 16, tem mais cartas (mão + campo + Grau) que o oponente em pelo menos 55% das partidas (pedido do autor, rodada 5). | 59.2% de 710 partidas (diferença média +1.1 cartas) | ≥ 55.0% |  |
| ✅ | Controle não ganha rápido: as vitórias dele duram em média 20 turnos ou mais (pedido do autor, rodada 5). | 35.0 turnos | ≥ 20 |  |
| ✅ | Controle perde menos Aliados que o oponente a longo prazo (razão perdidos/oponente ≤ 1). | 0.49 (3817 × 7835 em 793 partidas) | ≤ 1 |  |
| ✅ | Aggro começa a bater cedo: primeiro dano até o turno global 4 (mediana). | turno 1 (mediana) | ≤ 4 |  |
| ✅ | Aggro gasta a mão rápido: até 2,5 cartas na mão ao fim do seu 4º turno (média). | 2.25 cartas | ≤ 2.5 |  |
| ✅ | Controlado, o Aggro cansa: vence no máximo 40% das partidas longas contra o Controle. | 7.7% em 39 partidas com 18+ turnos | ≤ 40.0% |  |
| ✅ | ...mas não é impossível: pelo menos 30% contra o Controle. | 67.5% (n=120) | ≥ 30.0% |  |
| ✅ | Burn causa dano (em Aliados ou no Conjurador) em pelo menos 75% dos próprios turnos, mesmo que pouco. | 82.2% dos turnos próprios (a partir do 2º) | ≥ 75.0% |  |
| ✅ | Midrange tem early game para pressionar o Controle. | 57.5% (n=120) | ≥ 50.0% |  |
| ❌ | Midrange tem controle suficiente para segurar o Aggro. | 48.3% (n=120) | ≥ 50.0% | 1.7 pp |
| ✅ | Tempo (aggro-controle: ameaça barata + interação barata) pressiona o Controle. | 70.8% (n=120) | ≥ 50.0% |  |
| ✅ | Tempo interage no turno do oponente: pelo menos 1 carta repentina por partida. | 2.03 por partida | ≥ 1 |  |
| ❌ | Stall tem forma de ganhar dando dano: pelo menos 75% das vitórias são por dano, não por impasse. | 74.5% de 459 vitórias | ≥ 75.0% | 0.5 pp |
| ✅ | Stall costuma ser pior contra Aggro, mas joga (pelo menos 30%). | 63.3% (n=120) | ≥ 30.0% |  |
| ✅ | Se o Stall controla o Aggro (partida longa), ele ganha: pelo menos 60%. | 86.4% em 88 partidas com 16+ turnos | ≥ 60.0% |  |
| ✅ | Ir largo perde no late game: o Aggro vence no máximo 40% das partidas de 20+ turnos, contra todas as montagens (pedido do autor, rodada 6; acrescentado pelo balanceador). | 15.6% em 385 partidas com 20+ turnos | ≤ 40.0% |  |
| ✅ | No late game, unidades de Grau alto decidem: nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau 5 ou mais, esse lado vence pelo menos 60% (pedido do autor, rodada 6; acrescentado pelo balanceador). | 78.0% em 932 partidas com 18+ turnos | ≥ 60.0% |  |
| ➖ | Pode existir um deck de Combo (lento, pouca interação, muito forte se montar). | não existe | existir | 100.0% |
| ➖ | O counter do Combo é o Controle. (ausente) | montagem Combo não existe | — | 100.0% |
| ➖ | Se monta (partida longa), o Combo fica muito forte. (ausente) | montagem Combo não existe | — | 100.0% |

## Vitória média por montagem

| Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|
| 57.6% | 54.7% | 45.5% | 45.1% | 48.3% | 42.9% | 53.6% | 48.3% | 54.1% |

## Matriz (linha contra coluna)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | — | 51.7% | 67.5% | 95.8% | 36.7% | 63.3% | 49.2% | 35.8% | 60.8% |
| **Midrange** | 48.3% | — | 57.5% | 65.0% | 75.8% | 41.7% | 61.7% | 49.2% | 38.3% |
| **Controle** | 32.5% | 42.5% | — | 39.2% | 62.7% | 66.7% | 29.2% | 57.1% | 33.9% |
| **Burn** | 4.2% | 35.0% | 60.8% | — | 14.2% | 86.7% | 49.6% | 37.5% | 72.5% |
| **Stall** | 63.3% | 24.2% | 37.3% | 85.8% | — | 49.6% | 45.0% | 30.0% | 50.9% |
| **Ramp** | 36.7% | 58.3% | 33.3% | 13.3% | 50.4% | — | 32.5% | 82.5% | 36.2% |
| **Tempo** | 50.8% | 38.3% | 70.8% | 50.4% | 55.0% | 67.5% | — | 43.3% | 52.5% |
| **Mistura** | 64.2% | 50.8% | 42.9% | 62.5% | 70.0% | 17.5% | 56.7% | — | 21.7% |
| **Eternyr** | 39.2% | 61.7% | 66.1% | 27.5% | 49.1% | 63.8% | 47.5% | 78.3% | — |
