# Diagnóstico da rodada 3

Dados: [COMPARACAO-RODADA3.md](COMPARACAO-RODADA3.md) (31 configurações × 9 montagens × 9 × 30 partidas ≈ 75 mil partidas, `node sim/comparar.ts 30 rodada3`). Jogador automático simples: vale para ele, não para humanos.

Referência (**Base**, regras antigas, sem mudanças): 84,1% terminam; turnos totais média 16,6 e mediana 12; 24,0% entre 14 e 26 turnos e 32,7% com menos de 10; quem começa vence 55,8%; Burn 92,3% e Stall 7,8%; espalhamento das taxas de vitória 24,2 pp.

## 1. Cada pedido, medido

| Pedido | O que mudei | Resultado | Veredito |
|---|---|---|---|
| **Geração de Arcano cara, sobretudo nos Valores de late game** | Estruturas de custo 6 e 8 (3 e 4 Arcano por turno) e Aliado de custo 7 (3 Arcano), mais baratos por Arcano (1 PP em vez de 1,5), nos baralhos Ramp, Eternyr, Controle e Stall. | Ativações de Arcano sobem de 2,6 para 3,3 por jogador/partida. Mas Ramp 37,8% → 37,2%, Eternyr 21,2% → 24,0%, Controle 27,6% → 27,6%. A duração não muda (mediana 12). | **Sem efeito.** Usadas, mas o jogo acaba antes de compensar. |
| **Recurso das permanentes acelera o late game ou poupa cartas da mão** | Aliados e Estruturas de recurso passam a contar como 1 ou 2 cartas na Zona de Grau (`grauExtra`). | Stall +1,9 pp, Ramp −0,8 pp, Eternyr −0,6 pp. | **Sem efeito.** Cada jogador põe só **5,5 cartas no Grau por partida** (6,2 no pacote); poucas cartas de Grau 8 ou mais chegam a ser jogáveis. |
| **Cartas caras do Eternyr mais fortes, com Grau maior** | Dano a Aliado mais barato (1,5 PP/ponto, custos 4 a 6), dano ao Conjurador mais caro (3 PP/ponto), Aliados e destruição de Grau 8 a 10 com bônus de 2 PP por Grau acima do padrão. | Eternyr 38,8% → 37,9%. No pacote, 23,0% com a peça e 23,4% sem. | **Sem efeito.** Mesmo motivo: as partidas acabam antes do Grau 8. |
| **Nerf do dano do burn (constância, não dano por carta)** | Dano ao Conjurador a 2,5 e 3 PP por ponto (custo 5: 4 de dano em vez de 8), mais cartas pequenas no Burn. | Burn 92,3% → **74,4%** e **72,5%**. Partidas com menos de 10 turnos: 32,7% → 20,7% e 19,3%. Mas Aggro 73,1% → 77,4% e Midrange 62,5% → 70,7%. | **Funciona**, e o Burn ainda é o mais forte. O Aggro passa a ser o problema. |
| **Cura melhorada de novo** | Cura a 0,4 PP por ponto com efeitos extras maiores. | Controle 26,7% → 25,3%; Stall 7,8% → 9,3%. | **Sem efeito.** Quase não é jogada. |
| **Segundo jogador sem Arcano e sem carta extra** | Regra: agora é o padrão do motor. | Sozinha, quem começa passa de 55,8% para **65,0%**. | Precisa ser combinada com a próxima. |
| **Quem começa não põe Grau no turno 1** (pedido anterior) | Combinada com a anterior. | Pacote: quem começa vence **45,4%** (60,8% sem esta regra e 38,9% sem a anterior). | As duas juntas deixam perto de 50%. |
| **Primeiro Conjurador não ataca (turno 1)** | Regra, lida como "no primeiro turno". | Efeito pequeno: quem começa 54,4% com ela contra 55,2% sem; no pacote, 45,4% contra 47,6%. | Pequeno. |
| **Stax menos consistente** | Estruturas com 1 de Vida a menos, remoção que mira Estruturas, Stall com menos compra e mais Estruturas de cura. | Isolada: Stall 19,4% → 21,8%. No pacote: Stall **37,6%** com a peça e 28,3% sem. | **Falhou.** Piorou o stax. |
| **Relógio de impasse** | Perde quem passa N turnos seguidos sem jogar carta, ativar ou atacar. | Travadas vão a **0%** com 3, 5 ou 8 turnos. Mediana 11 / 13 / 14; média 18,8 / 23,7 / 25,2; p90 56 / 63 / 70. | **Funciona** contra o travamento, com cauda longa de partidas. |

## 2. O pacote completo

Todas as peças juntas: terminou 98,8%; média 23,5 e mediana 14; 26,3% entre 14 e 26 turnos; 19,4% abaixo de 10; quem começa vence 45,4%; espalhamento 19,3 pp.

Vitória por montagem: Aggro 80,2%, Burn 73,6%, Midrange 61,7%, Mistura 60,6%, Tempo 50,6%, Stall 37,6%, Ramp 33,8%, **Controle 27,7%**, **Eternyr 23,0%**.

Peça por peça (pacote sem a peça):
- **Sem o burn nerf**: Burn volta a 88,8% e as partidas ficam curtas (média 21,8). É a peça que mais mexe no equilíbrio.
- **Sem o relógio**: travadas voltam a 18,1%.
- **Sem o Conjurador grátis (que não suspende no Grau)**: melhor espalhamento (15,9 pp), mediana 17, média 26,4, 30,3% entre 14 e 26, e Burn 64,6%. O Conjurador grátis causa 36,5% do dano ao Conjurador, mas encurta as partidas.
- **Sem o stax frágil**: Stall cai de 37,6% para 28,3%.
- **Sem Arcano caro, sem permanentes que contam como Grau, sem Eternyr forte, sem cura melhorada**: quase nenhuma diferença.

## 3. Jogador de deck lento (outra variância)

O jogador de deck lento (Controle, Stall, Ramp, Eternyr) só se preocupa em sobreviver, segura Arcano para repentinas, joga defesa, cura e compra, e só ataca com cartas de Grau 6 ou mais.

| | Controle | Stall | Ramp | Eternyr | Partidas travadas |
|---|---|---|---|---|---|
| Base | 26,7% | 7,8% | 31,0% | 38,8% | 15,9% |
| Base com jogador lento | **0,0%** | 0,0% | 0,4% | 0,0% | 33,1% |
| Pacote | 27,7% | 37,6% | 33,8% | 23,0% | 1,2% |
| Pacote com jogador lento | 19,9% | 37,1% | 22,4% | 15,2% | 1,7% |

- Sem relógio, o jogador lento **nunca vence**: não tem condição de vitória e vira partida travada.
- Com o pacote, ele chega a mais Grau (7,5 cartas contra 6,2) e a partida dura 28 turnos de média, mas **piora** Controle, Ramp e Eternyr. A promessa de "ganhar em Graus altos" não se cumpre: ele ataca menos (1,8 ataques de Aliado por partida contra 3,0) e as cartas de Grau alto são poucas.

## 4. Diagnóstico geral

1. **O jogo ainda é curto demais para o late game existir.** Cada jogador põe ~5,5 cartas no Grau antes da partida acabar (mediana de 12 turnos totais, 6 por jogador). Por isso Arcano caro, permanentes que contam como Grau e as cartas de Grau 8 a 10 do Eternyr não mudam nada. Elas precisam de partidas de 16 turnos totais ou mais para aparecer.
2. **O Aggro domina** em todas as configurações (73% a 83%). O burn nerf tirou o Burn do topo e passou o Aggro para ele. O **Controle** está em 5% a 13% de vitória contra Aggro e 0% a 4% contra Burn. É o pedido para o agente de testes (ver [AGENTE-DE-TESTES.md](AGENTE-DE-TESTES.md)).
3. **O relógio de impasse resolve o travamento**, mas deixa uma cauda longa (p90 de 56 a 70 turnos). Um valor menor (3) deixa a cauda menor.
4. **Quem começa**: o conjunto "segundo sem compensação + primeiro sem Grau no turno 1" dá 45,4%, bem perto de 50%.
5. **Cura, stax frágil, Eternyr forte e permanentes aceleradoras** não funcionaram como implementadas. O stax frágil piorou o stax.

## 5. Próximos passos que eu sugiro

1. **Alongar o jogo para o late game existir**: testar Vida do Conjurador de 16 ou 18 (hoje 12). É a mudança mais direta para dar espaço ao Grau alto, ao Eternyr e aos geradores de Arcano.
2. **Dar ao Controle ferramentas contra o Aggro** (o agente de testes está procurando a combinação com as cartas existentes; se não achar, falta carta defensiva barata de custo 1 a 3 para os decks lentos).
3. **Trocar a ideia do stax frágil**: em vez de menos Vida, uma carta de remoção específica de Estruturas, ou um custo para manter Estruturas.
4. **Manter o relógio de impasse** (3 a 5) e o conjunto de regras do segundo jogador.
5. **Revisar o jogador lento**: dar a ele cartas de Grau alto que vençam sozinhas, senão ele só sobrevive.

## 6. Resultado do agente de testes (objetivo: Controle sobrevive a Aggro e Burn)

Atingido e confirmado com 400 partidas por confronto, na variante P (pacote da rodada 3), depois de 3.032 tentativas (ver `agente/diario.md` e `agente/resultado.md`).

| Medida | Antes (pacote) | Depois | Meta |
|---|---|---|---|
| Controle contra Aggro | ~5% | **40,0%** | 40% |
| Controle contra Burn | ~4% | **44,8%** | 40% |
| Turnos que sobrevive quando perde (Aggro / Burn) | 10 a 11 | 14,4 / 12,6 | 12 |
| Vitória média contra as outras montagens | ~28% | 59,9% | 30% a 62% |
| Partidas travadas | — | 1,3% | até 35% |

O que o agente achou: um Controle com muita **cura** (a cura melhorada da rodada 3 passou a ser usada), Estruturas de efeito, remoção repentina, **sem reservar Arcano para repentinas**, com agressividade 0,6 e Grau desejado 11. Ou seja, o Controle que sobrevive ao Aggro e ao Burn é mais proativo que o Controle clássico.

Ressalvas: a meta contra Aggro bateu exatamente em 40,0%, então a margem é mínima; a confirmação falhou algumas vezes por 0,2 pp antes de passar. Vale para o jogador automático. O Controle ficou forte contra as outras montagens (59,9%), perto do teto de 62% que defini.
