# Diagnóstico do agente-balanceador: rodada 6

Pedido do autor: [PEDIDO-RODADA6.md](PEDIDO-RODADA6.md). O diagnóstico da rodada 4 foi movido para [DIAGNOSTICO-BALANCEADOR-R4.md](DIAGNOSTICO-BALANCEADOR-R4.md).

**Situação: PRÓXIMO.** A variante **`BAL`** atinge **21 de 24 critérios** com 60 e com 80 partidas por confronto (4.860 e 6.480 partidas). Os 3 critérios que faltam ficam a menos de 2,1 pp (ou 2,1%) do alvo e mudam de um para outro conforme a amostra.

Relatórios: [AVALIACAO-BAL-60.md](AVALIACAO-BAL-60.md), [AVALIACAO-BAL-80.md](AVALIACAO-BAL-80.md); o PADRAO no mesmo formato está em [AVALIACAO-PADRAO-R6.md](AVALIACAO-PADRAO-R6.md). Passo a passo: seção "Rodada 6" de [MUDANCAS-DO-BALANCEADOR.md](MUDANCAS-DO-BALANCEADOR.md). Cartas e baralhos: [CARTAS-BAL.md](CARTAS-BAL.md).

**Nada disso virou padrão.** O PADRAO e as regras do motor (`REGRAS_PADRAO`) não mudaram. Tudo está na variante `BAL`, esperando a sua avaliação.

## 1. Critérios

| Critério | PADRAO (60 partidas) | BAL (60 partidas) | Meta |
|---|---|---|---|
| Critérios atingidos (os 22 seus + 2 novos) | 17/24 (15/22) | **21/24 (19/22)** | — |
| Maior vitória média | Aggro 63,1% | **Aggro 57,1%** | ≤ 59% |
| Menor vitória média | Tempo 35,3% | **Ramp 43,4%** | ≥ 38% |
| Quem começa vence | 38,6% | **45,6%** | 45% a 55% |
| Duração média | 24,4 | 26,5 (26,1 com 80 partidas) | 16 a 26 ❌ |
| Controle contra Aggro | 25,0% | **36,7%** | ≥ 35% |
| Controle nas partidas longas | 64,4% | 61,8% | ≥ 55% |
| Controle em recurso no turno 16 | 60,1% | 57,1% (53,0% com 80) | ≥ 55% (oscila) |
| Controle: vitórias em turnos / trocas | 32,7 / 0,40 | 34,2 / 0,53 | ≥ 20 / ≤ 1 |
| Aggro: mão no 4º turno | 2,55 | **2,26** | ≤ 2,5 |
| Midrange contra Controle / contra Aggro | 46,7% / 62,5% | **55,8%** / 48,3% | ≥ 50% / ≥ 50% ❌ |
| Tempo contra Controle | 45,0% | **70,8%** | ≥ 50% |
| Stall vence por dano | 82,3% | 72,9% (74,7% com 80) | ≥ 75% ❌ |
| *Novo:* Aggro em partidas de 20+ turnos ("ir largo perde no late game") | 19,3% | 15,1% | ≤ 40% |
| *Novo:* quem jogou Aliado de Grau ≥ 5 vence as partidas longas | 78,3% | 78,4% | ≥ 60% |

Vitória média na BAL: Aggro 57,1% · Midrange 54,5% · Controle 46,4% · Burn 43,6% · Stall 49,3% · Ramp 43,4% · Tempo 53,6% · Mistura 47,7% · Eternyr 54,5%. Todas as montagens ficaram entre 43% e 58% (no PADRAO, entre 35% e 63%).

## 2. Mudanças e impacto

| Passo | Mudança | Impacto |
|---|---|---|
| 1 | **Segundo jogador sem a carta extra** (como o senhor pediu na rodada 3) | Quem começa passa de 38,6% para 46% de vitória e entra na meta |
| 2 e 5 | **Varreduras no Controle**: Fúria, Ira, Pôr do sol (Onda de fagulhas fica como opção) | O Controle passa a ter resposta contra quem vai largo |
| 3 | **Curva de poder por Grau** nos Aliados | O Aggro cai de 64,6% para 59,8%; o Grau alto passa a decidir mais |
| 4 | **Unidades-problema e bombas**, Protegido, gatilho "ao causar dano de combate ao Conjurador" | Partidas longas decididas por unidades de Grau alto: 81% (antes 75%) |
| 6 | Jogador automático guarda as varreduras (antes ia com elas para o Grau ou o pitch) | Só estratégia: as varreduras passam a ser jogadas |
| 7 | Stall recebe Ira, Fúria e Pôr do sol (fortaleza quase sem Aliados) | Stall sobe de 31% para 51% |
| 8 e 12 | Baralhos do agente de testes (2 rodadas de ajuste) | Tempo de 31% para 54%; Midrange passa a pressionar o Controle |
| 9 | O Aggro volta à lista aprovada do PADRAO | O Aggro do agente estava cheio de cartas lentas que ele nunca joga (baixava a vitória estragando o baralho); com cartas baratas no lugar, chegaria a 65,8% |
| 10 | Eternyr sem o Mercador e o Dragão | Eternyr de 60% para 57% |
| 11 | O Controle volta a ser o Controle de remoção, com a Cura que compra e a Presa-trovão | Controle com papel de Controle (ganha em recurso e devagar, troca bem) |
| 13 | Controle em agressividade 0,3 (o agente tinha posto 0,1) | Duração de 27,5 para 25,7 |
| 14 e 15 | No Controle: 2 Estruturas de compra no lugar de 2 Aliados de ataque; 4 "Eternyr: comprar 3" no lugar do dano ao Conjurador e de uma Fera de cerco | Controle em recurso de 50% para 56–57% |

## 3. O que muda no jogo físico

**Regras:**
- O segundo jogador **não compra carta extra** (volta o que o senhor pediu na rodada 3). Quem começa continua sem pôr Grau no turno 1.

**Régua de poder (curva de Grau):** o orçamento de um Aliado é 2 × custo + 2 + 0,5 × G × (G − 1) / 2, com G = Grau, até 8.
- Bônus por Grau: G3 +1,5; G4 +3; G5 +5; G6 +7,5; G7 +10,5; G8 +14.
- Na mesa, isso só muda os números impressos. Exemplos: o Aliado de custo 4 passa de 4/6 para 4/7; o de custo 6, de 6/8 para 8/11; o de custo 8, de 8/10 para 12/16.

**Palavras-chave e efeitos novos (uma linha cada):**
- **Protegido N**: "O oponente paga N de Arcano a mais para mirar esta carta." Varreduras não miram e pegam a carta normalmente.
- **"Ao causar dano de combate ao Conjurador inimigo: …"**: gatilho que vai para a pilha, como o "ao entrar".
- **"Destrua todos os Aliados"** (do oponente, ou de todos) e **varredura simétrica** ("cause X de dano a cada Aliado, os seus também").

**Cartas novas:**

| Carta | Custo / Grau | Corpo | Texto |
|---|---|---|---|
| Onda de fagulhas | 2 / 1 | — | 1 de dano em cada Aliado do oponente |
| Fúria | 3 / 2 | — | 3 de dano em cada Aliado (os seus também) |
| Ira | 4 / **5** | — | Destrua todos os Aliados (os seus também) |
| Pôr do sol | 7 / 7 | — | Destrua todos os Aliados do oponente |
| Espião alado | 4 / 3 | 2/3 | Etéreo. Ao causar dano de combate ao Conjurador: compre 1 |
| Piromante | 4 / 3 | 3/4 | Ao entrar: 1 de dano em cada Aliado do oponente |
| Fera de cerco | 5 / 4 | 3/6 | Ao entrar: 2 de dano ao Conjurador inimigo e cure 2 do seu |
| Presa-trovão | 5 / 4 | 5/8 | Ao entrar: cure 5 do seu Conjurador |
| Anjo da aurora | 6 / 5 | 6/8 | Etéreo. Ao causar dano de combate ao Conjurador: cure 4 do seu |
| **Mercador de segredos** (no espírito do Tivit) | 7 / 6 | 4/7 | Etéreo. Protegido 2. Ao entrar: compre 1. Ao causar dano de combate ao Conjurador: compre 1 |
| Colosso de ferro | 7 / 6 | 9/11 | Protegido 2 |
| Dragão ancestral | 8 / 7 | 7/10 | Etéreo. Protegido 1. Ao entrar: 2 de dano em cada Aliado do oponente |

Nada exige marcador novo, zona nova ou conta longa. Os gatilhos são de uma linha, e o Protegido é um número fixo na carta.

## 4. Baralhos de teste

São as montagens que os agentes usam para medir; não são listas obrigatórias.
- **Aggro:** a lista aprovada do PADRAO.
- **Controle:** do agente de testes, com os ajustes dos passos 13 a 15: remoção, varreduras, Anular, Cura que compra, Presa-trovão, muita compra, poucos finalizadores.
- **Midrange:** do agente: curva de Aliados, Piromante ×4, Anjo ×2, Mercador.
- **Tempo:** do agente: Etéreos, remoção repentina, Devolver.
- **Ramp:** do agente (1º ajuste).
- **Stall, Burn, Eternyr e Mistura:** do PADRAO, com as trocas da rodada 6.

Arquivos: `sim/baralhos-bal6-agente.json` e `sim/baralhos-bal6b-agente.json`. Listas completas em [CARTAS-BAL.md](CARTAS-BAL.md).

## 5. Riscos e o que depende de teste com pessoas

- **Os 3 critérios que faltam** ficam no limite e trocam de lugar com a amostra: duração (26,1 a 26,5; meta ≤ 26), Midrange × Aggro (48%), Stall por dano (73–75%). O Controle em recurso também oscila (53–57%). Para fechar de vez, provavelmente basta mais um ajuste fino de baralhos.
- **Partidas mais longas** que no PADRAO (26,5 contra 24,4): o Controle e o Stall ainda perdem algumas partidas por impasse (6% das partidas terminam assim).
- **Valores:** os baralhos de teste misturam cartas do Eternyr com cartas neutras (o Controle usa várias "Eternyr: comprar"). No jogo físico, os Valores do Conjurador limitam isso. É preciso definir se o Controle será um Conjurador Eternyr ou se a carta de compra neutra deve ser tão boa quanto a do Eternyr.
- **Curva de Grau:** os Aliados tardios do Eternyr chegam a 14/20 (a curva soma com o bônus antigo do Eternyr por Grau acima do padrão). Tirar essa soma foi testado e piorou o Eternyr contra o Controle do agente; fica para o senhor decidir.
- **Bombas:** o jogador automático não sabe guardar a remoção para a bomba nem jogar em volta do Protegido. Com pessoas, o Mercador e o Dragão podem ser mais fortes (difíceis de remover) ou mais fracos (varreduras na hora certa) do que aqui.
- **Ira (Grau 5 e custo 4)** e **Fúria (simétrica)** dependem de o jogador ter poucos Aliados. Vale testar se o Controle físico gosta de jogar assim.
- **O Tempo do agente** tem 2 cartas que nunca joga (o Dragão e o Aliado tardio do Eternyr, acima do Grau alvo dele). Não afeta as regras, só o baralho de teste.

---

PEDIR PERMISSÃO AO AUTOR: aplicar as mudanças da variante BAL como padrão?

Resumo: a BAL atende o pedido da rodada 6 e fica PRÓXIMO (21/24 com 60 e com 80 partidas por confronto; o PADRAO tem 17/24). O que entra: varreduras novas, curva de poder por Grau, unidades de Grau médio que viram problema e bombas no espírito do Tivit, Protegido N, o gatilho "ao causar dano de combate ao Conjurador" e o segundo jogador sem carta extra. Faltam, no limite, a duração (26,5 contra a meta de 26), o Midrange contra o Aggro (48%) e o Stall vencendo por dano (73–75%).
