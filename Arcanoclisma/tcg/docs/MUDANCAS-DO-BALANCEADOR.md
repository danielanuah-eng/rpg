# Mudanças do agente-balanceador (rodada 4, Vida 20)

Cada linha é uma mudança medida com `node sim/avaliar.ts --variante <id> --partidas 30` (2.430 partidas; cerca de ±6 pp de ruído por confronto). Ponto de partida: **P** (pacote da rodada 3) com Vida 20: **11 de 20** critérios.

| Passo | Variante | Mudança | Critérios | O que melhorou | O que piorou |
|---|---|---|---|---|---|
| 0 | P | Pacote da rodada 3, Vida 20 | 11/20 | — | Aggro 67,7%; Controle 22,5% contra Aggro; partidas de 32 turnos de média |
| 1 | BAL1 | **Varreduras** (dano em todos os Aliados do oponente) no Controle, Stall e Eternyr | 12/20 | Aggro 67,7% → 60,6%; Controle contra Aggro 22,5% → 30% | Tempo 29% |
| 2 | BAL2 | **Remoção que cresce com o Grau** ("1 + suas cartas no Grau" de dano em um Aliado) | 11/20 | Stall ganha mais por dano | Midrange 63% |
| 3 | BAL3 | **Estrutura de cerco** no Stall (suspenda: 1 de dano no Conjurador inimigo) | 12/20 | Stall vence por dano em 67% (era 41%) | — |
| 4 | BAL4 | **Devolver** (repentina, custo 2) no Tempo | 13/20 | Tempo passa a pressionar o Controle | — |
| 5 | BAL4+etereo | **Etéreo** (só é bloqueado por Etéreo) no Tempo | 12/20 | Tempo 37% → 55%; Midrange 63% → 61% | (ruído) |
| 6 | BAL4+aggroCurva | Curva do Aggro mais baixa | 13/20 | Aggro gasta a mão (2,8 → abaixo de 2,5) | — |
| 7 | BAL4+anular | **Anular** (repentina, custo 3) no Eternyr e no Controle | 13/20 | Eternyr 36% → 37,5% | — |
| 8 | BAL4+cata10 | **Cataclisma crescente**: a partir do 10º turno próprio, o Cataclisma causa 1 de dano ao Conjurador do turno, +1 por turno | **15/20** | Duração de 31 para dentro da meta; Stall vence por dano | Stall 80% (cura), Eternyr 25% |
| 9 | BAL7 | Tudo acima + Controle do agente de testes + Estrutura de cura só cura Aliados | **15/20** | Aggro 61%, Midrange 59%, Stall 58% | Controle 65%; Ramp 30%; Eternyr 27% |
| 10 | BAL8 | Baralhos e estilos de **Eternyr, Ramp e Controle** ajustados pelo agente de testes em BAL7 (`sim/baralhos-bal7-agente.json`) | **15/20** | Todas as montagens entre 41% e 60% | Quem começa 43% |
| 11 | BAL9 | Quem começa **pode pôr Grau no turno 1, mas a carta entra suspensa** (em vez de não poder pôr) | 16/20 | — | Quem começa 59% (passou do ponto) |
| 12 | BAL10 / BAL10b | BAL9 com o segundo jogador recebendo 1 Arcano (BAL10) ou **1 carta extra (BAL10b)** | **17/20** (BAL10b) | Quem começa dentro de 45% a 55% | — |
| 13 | BAL11 | Controle "de verdade" ajustado pelo agente de testes (no máximo 14 Aliados, trocar bem, vencer partidas longas) | **19/20** | Controle troca 0,95 e vence 63% das longas | Ramp 65% |
| 14 | BAL12c = **BAL** | Aliados que descartam Grau descartam 1 carta a mais | **20/20** (confirmado com 100 partidas por confronto) | Ramp 52% | — |

Opções testadas para o Ramp e não usadas: geração de Arcano cara a 1,25 PP (18/20) e sem desconto (18/20).

Falta em BAL10b (resolvido nos passos 13 e 14): Ramp 60,4% (1,4 pp do teto), Controle vence 51,7% das partidas longas (meta 55%) e perde mais Aliados que o oponente (1,11; meta ≤ 1). O baralho de Controle achado antes tinha 22 Aliados, que é um baralho de criaturas, não de controle. Por isso o agente de testes recebeu um novo pedido (`sim/objetivos-bal10.json`): no máximo 14 Aliados, trocar bem e vencer partidas longas.

Testes rejeitados: Conjurador com dano básico 2 (deixa o Aggro mais forte), relógio de impasse 4 (pouco efeito), Cataclisma desde o turno 8 (mata o Controle) e desde o 12 (igual ao 10).

Resultado final e pedido de permissão: [DIAGNOSTICO-BALANCEADOR-R4.md](DIAGNOSTICO-BALANCEADOR-R4.md).

## Rodada 5: decisão do autor e outro meio para a duração

O autor aprovou a variante BAL **menos**: a Cataclisma crescente (não quer custo novo em mecânica existente), o Grau suspenso de quem começa e a cota de Aliados no Controle (deckbuilding livre). Regra nova do autor: o Aliado que acabou de entrar **não ataca nem usa efeito de suspender**, a menos que seja Proficiente (como o enjoo de invocação do Magic). As regras aprovadas viraram o padrão do motor e do livro; as cartas e baralhos estão na variante **PADRAO**.

Medido com `node sim/avaliar.ts --variante <id> --partidas 30` (60 no PADRAO) e `node sim/duracao.ts <id> 20`:

| Variante | Mudança | Critérios | Duração | Fim por impasse | Observação |
|---|---|---|---|---|---|
| R5 | BAL sem Cataclisma crescente nem Grau suspenso, com a regra de entrada | 15/20 | 29,2 | 18,6% | Tabuleiros entupidos (15 Aliados de cada lado, ninguém ataca); Controle 24% |
| R5a | + jogador automático ataca em massa quando tem mais atacantes que bloqueadores | 12/20 | 27,4 | 16,7% | Aggro 79%: como quem bloqueia não ataca no turno seguinte, atacar em massa é muito forte. Não usado |
| R5g | + jogador sobe o Grau até as cartas da mão | — | 30,2 | 19,1% | Sem efeito |
| R5e | + Aliados Etéreos caros (finalizadores) | 14/20 | 27,0 | 14,9% | Ajuda pouco |
| R5m | + **Aliados atacam Aliados** quando a troca compensa (antes só atacavam o Conjurador) | 14/20 | **24,9** | **4,1%** | Resolve a duração sem regra nova; Controle 22,5% |
| R5mc | R5m + **Controle de remoção** (26 remoções, 14 de recurso, 5 finalizadores, sem cota) | 16/22 | 24,2 | 3,5% | Controle vence devagar e em recurso |
| **PADRAO** | = R5mc (60 partidas por confronto) | **16/22** | **23,7** | 3,5% | Ver abaixo |

Critérios novos pedidos pelo autor: **Controle ganha em recurso** (no turno 16 tem mais cartas em mão + campo + Grau que o oponente em ≥ 55% das partidas) e **Controle não ganha rápido** (vitórias com média ≥ 20 turnos).

No PADRAO: o Controle tem mais recursos em 64,1% das partidas (+1,4 carta), vence em 32,1 turnos de média, vence 64,9% das partidas longas e perde 0,40 Aliado para cada Aliado do oponente. Faltam: Aggro 63,9% (teto 59%), Tempo 34,3% (piso 38%), quem começa 58,3% (meta 45–55%), Controle contra Aggro 21,7% (meta ≥ 35%), Midrange contra Controle 47,5% (≥ 50%) e Tempo contra Controle 35,8% (≥ 50%). Os baralhos de Aggro e Tempo foram ajustados para o jogador antigo, que não atacava Aliados; o próximo passo é a equipe de balanceamento reajustá-los.

## Rodada 6: varreduras, curva de poder por Grau, unidades-problema e bombas

Pedido do autor: [PEDIDO-RODADA6.md](PEDIDO-RODADA6.md). Variante de trabalho **BAL** em `sim/lista.ts` (= PADRAO + os passos abaixo; cada passo também existe como variante própria, `BAL6a` a `BAL6g`). Nada mudou no PADRAO nem em `REGRAS_PADRAO`. Lista das cartas: [CARTAS-BAL.md](CARTAS-BAL.md) (`node sim/cartas-padrao.ts BAL`).

### O que foi criado

**Curva de poder por Grau** (`bonusGrau` em `src/balanco.ts`, flag `r6CurvaGrau`): o orçamento de um Aliado passa a ser

> orçamento = 2 × custo + 2 + k × G × (G − 1) / 2, com G = Grau da carta (limitado a 8) e k = 0,5.

Bônus: Grau 3 → +1,5 PP; 4 → +3; 5 → +5; 6 → +7,5; 7 → +10,5; 8 ou mais → +14. Grau baixo quase não muda, Grau alto ganha muito (o Grau é o "custo de tempo" da carta). Vale para os Aliados (as unidades que põem clock); o bônus entra em Ataque e Vida na proporção do perfil. Exemplos: Aliado de custo 4 4/6 → 4/7; custo 6 6/8 → 8/11; custo 8 8/10 → 12/16; custo 8 que descarta Grau 10/14 → 14/20.

**Mecânicas novas (uma linha cada; motor + teste em `test/rodada6.test.ts`):**
- **Ao causar dano de combate ao Conjurador inimigo: …** — gatilho que vai para a pilha como o "ao entrar" (preço: 1,5 × o efeito).
- **Protegido N** — o oponente paga N de Arcano a mais para mirar esta carta com uma carta ou ativação (preço: 1,5 PP por ponto). Varreduras não miram, então pegam a carta normalmente.
- **Destrua todos os Aliados** (do oponente, 16 PP; de todos, 11 PP) e **varredura simétrica** (dano em cada Aliado, os seus também: 2,5 PP por ponto, contra 4 da varredura de um lado só).

**Varreduras (Controle, e o Stall no passo 7):** Onda de fagulhas (custo 2, 1 de dano em cada Aliado do oponente), **Fúria** (custo 3, Grau 2: 3 de dano em cada Aliado, os seus também; tipo Anger of the Gods), **Ira** (custo 4, **Grau 5**: destrua todos os Aliados; tipo Wrath, barata mas de Grau alto), **Pôr do sol** (custo 7, Grau 7: destrua todos os Aliados do oponente; tipo Sunfall). Continuam as de custo 3 (2 de dano) e 5 (3 de dano).

**Unidades de Grau médio que viram problema:** Espião alado (custo 4, Grau 3, 2/3, Etéreo, ao causar dano de combate ao Conjurador: compre 1), Piromante (custo 4, Grau 3, 3/4, ao entrar: 1 de dano em cada Aliado do oponente), Fera de cerco (custo 5, Grau 4, 3/6, ao entrar: 2 de dano ao Conjurador inimigo e cure 2 do seu; tipo Siege Rhino), Presa-trovão (custo 5, Grau 4, 5/8, ao entrar: cure 5 do seu Conjurador; tipo Thragtusk).

**Bombas de Grau alto:** Anjo da aurora (custo 6, Grau 5, 6/8, Etéreo, ao causar dano de combate ao Conjurador: cure 4 do seu; tipo Baneslayer), **Mercador de segredos** (custo 7, Grau 6, 4/7, Etéreo, Protegido 2, ao entrar: compre 1, ao causar dano de combate ao Conjurador: compre 1; no espírito do Tivit), Colosso de ferro (custo 7, Grau 6, 9/11, Protegido 2; tipo Wurmcoil), Dragão ancestral (custo 8, Grau 7, 7/10, Etéreo, Protegido 1, ao entrar: 2 de dano em cada Aliado do oponente).

**Distribuição (trocas uma a uma, sem cotas):** Controle: Mercador ×2, Piromante, Fúria ×3, Ira, Pôr do sol (saem dano de custo 1 a 3, uma Varredura de custo 3, dois Aliados de custo 8 e um defensivo de custo 4). Midrange: Espião ×2, Piromante ×2, Fera ×2, Presa, Anjo, Mercador. Ramp: Dragão ×2, Colosso ×2, Anjo. Tempo: Espião ×2 (no lugar do 4-drop agressivo). Stall: Anjo ×2, Presa ×2, Ira ×2, Fúria ×2, Pôr do sol. Eternyr: Mercador, Dragão. Aggro, Burn e Mistura não recebem nada (ir largo e barato perde força no late game).

**Jogador automático:** avalia os gatilhos novos e o Protegido (custo extra ao mirar), decide varreduras simétricas e "destrua todos" pelo saldo (o que mata do oponente menos o que perde) e, na BAL, guarda as varreduras como remoção (antes elas eram as primeiras cartas a ir para o Grau ou para o pitch; flag `botVarredura`, só estratégia).

**Critérios acrescentados** (`sim/criterios.json`, pedido do autor; nenhum existente mudou): *Ir largo perde no late game* (Aggro vence ≤ 40% das partidas de 20+ turnos, contra todos) e *Unidades de Grau alto decidem* (nas partidas de 18+ turnos em que só um lado jogou Aliado de Grau ≥ 5, esse lado vence ≥ 60%). O PADRAO já cumpre os dois (19,3% e 78,3%), então a conta do PADRAO passa de 15/22 para 17/24; a BAL mostra o efeito pedido com folga maior (13,8% e 81,1%).

### Medições

`node sim/avaliar.ts --variante <id> --partidas 30` (2.430 partidas, ±6 pp por confronto). Vitória média na ordem Aggro / Midrange / Controle / Burn / Stall / Ramp / Tempo / Mistura / Eternyr. "Largo" = vitória do Aggro em 20+ turnos; "Bomba" = vitória de quem jogou Aliado de Grau ≥ 5 (18+ turnos, só um lado).

| Passo | Variante | Mudança | Critérios (de 24) | Quem começa | Turnos | Vitória média | Controle × Aggro | Largo / Bomba | Melhorou | Piorou |
|---|---|---|---|---|---|---|---|---|---|---|
| 0 | PADRAO | ponto de partida (30 partidas) | 17 (15/22) | 38,4% | 24,3 | 62,7 / 53,7 / 49,0 / 51,9 / 42,3 / 48,8 / 36,3 / 47,3 / 58,2 | 30,0% | 20,2% / 75,1% | — | — |
| 1 | BAL6a | **segundo jogador sem carta extra** (`segundoCarta: 0`) | 18 | **46,0%** | 24,4 | 64,6 / 56,0 / 51,3 / 50,0 / 42,1 / 50,4 / 33,1 / 44,0 / 58,5 | 20,0% | 20,5% / 74,6% | quem começa dentro de 45–55%; Aggro gasta a mão | Controle × Aggro (ruído grande, n=60) |
| 2 | BAL6b | + Onda de fagulhas, Ira e Pôr do sol no Controle | 18 | 46,5% | 24,4 | 64,6 / 56,9 / 53,1 / 51,0 / 41,9 / 48,1 / 34,4 / 43,5 / 56,5 | 20,0% | 19,6% / 75,6% | Controle 53%; Tempo × Controle 42% | quase não são jogadas (ver passo 6) |
| 3 | BAL6c | + **curva de Grau** (k = 0,5) nos Aliados | 18 | 45,8% | 23,6 | 59,8 / 54,8 / 58,1 / 50,2 / 37,7 / 53,4 / 38,3 / 42,7 / 54,9 | 25,0% | 19,4% / 76,0% | Aggro 64,6 → 59,8%; Controle 58% | Stall 37,7%; Midrange × Controle 35% |
| 4 | BAL6d | + Protegido, gatilho de dano ao Conjurador, unidades-problema e bombas nos baralhos | 19 | 45,4% | 22,9 | 60,6 / 57,7 / 52,8 / 43,4 / 31,9 / 62,1 / 33,5 / 51,0 / 57,0 | 20,0% | **15,0% / 81,5%** | Midrange × Controle ok; Grau alto decide mais | Stall 31,9% (sem resposta às bombas Etéreas); Ramp 62% |
| 5 | BAL6e | + Fúria (simétrica, 3 de dano) no lugar da Onda de fagulhas | 20 | 45,9% | 22,9 | 60,2 / 55,8 / 54,1 / 43,6 / 32,3 / 61,5 / 33,9 / 51,2 / 57,5 | 23,3% | 16,4% / 80,5% | Aggro cansa contra o Controle | Midrange × Controle 38% (ruído) |
| 6 | BAL6f | + jogador automático guarda as varreduras | 21 | 45,7% | 22,9 | 59,4 / 58,1 / 49,4 / 43,0 / 31,3 / 63,1 / 32,9 / 52,5 / 60,4 | 26,7% | 15,6% / 81,3% | varreduras jogadas bem mais; Midrange × Controle ok | Ramp 63%, Eternyr 60% |
| 7 | **BAL** (= BAL6g) | + Ira ×2, Fúria ×2 e Pôr do sol no Stall (fortaleza quase sem Aliados) | **21** | 46,3% | 24,3 | 60,6 / 54,2 / 42,6 / 42,1 / **51,0** / 61,7 / 32,5 / 48,5 / 56,7 | 26,7% | 17,1% / 80,9% | Stall 31 → 51%; duração volta a 24 | Controle 42,6% (perde o Stall como presa) |

Testes da intensidade da curva (sobre o passo 6): **k = 0** 19/24 (Aggro 68,5%, Controle perde as partidas longas), **k = 0,3** 20/24 (Aggro 64,2%), **k = 0,5** 21/24, **k = 0,7** 20/24 (Ramp 63%, Stall 30%). Fica k = 0,5.

**Confirmação com 60 partidas por confronto** (o relatório desta medida foi substituído pelo da etapa seguinte; números abaixo; PADRAO no mesmo tamanho e com os critérios novos em `docs/AVALIACAO-PADRAO-R6.md`):

| | Critérios | Quem começa | Turnos | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| PADRAO | 17/24 (15/22) | 38,6% | 24,4 | 63,1% | 56,3% | 47,8% | 47,3% | 44,3% | 53,3% | 35,3% | 47,7% | 54,8% |
| BAL | 19/24 (17/22) | 46,2% | 24,5 | 59,1% | 55,2% | 38,8% | 45,3% | 50,4% | 59,6% | 30,7% | 54,8% | 56,1% |

`node sim/duracao.ts BAL 20`: 24,6 turnos de média (PADRAO 24,8); 95,6% das partidas terminam por dano ao Conjurador, 4,1% por impasse, 0,3% travam.

Ainda falham na BAL (60 partidas): Aggro 59,1% e Ramp 59,6% (teto 59%), Tempo 30,7% (piso 38%), Controle 23,3% contra o Aggro (meta 35%), Controle vence 53,1% das partidas longas (meta 55%) e Tempo 45% contra o Controle (meta 50%). São problemas de montagem e de estilo de jogo (nos registros de partida, o Controle passa os primeiros turnos só pondo Grau e segurando Arcano para repentinas; o Tempo já estava em 35% no PADRAO), não de cartas que faltam. Pedido ao agente de testes: `sim/objetivos-bal6.json`.

### Rodada 6, segunda etapa: baralhos do agente de testes e identidade das montagens

O agente de testes atingiu `sim/objetivos-bal6.json` (3.173 tentativas, confirmado com 240 partidas por confronto); snapshot em `sim/baralhos-bal6-agente.json` (Controle, Tempo, Aggro e Ramp). Medido com 30 partidas por confronto. Ordem da vitória média: Aggro / Midrange / Controle / Burn / Stall / Ramp / Tempo / Mistura / Eternyr.

| Passo | Variante | Mudança | Critérios (de 24) | Turnos | Vitória média | Controle × Aggro | Controle em recurso | Mão do Aggro no T4 | Melhorou | Piorou |
|---|---|---|---|---|---|---|---|---|---|---|
| 8 | BAL6h | + baralhos e estilos do agente (`r6BaralhosAgente`) | 20 | 25,5 | 47,7 / 54,6 / 47,4 / 47,3 / 51,8 / 49,3 / 46,5 / 43,1 / 62,3 | 60,0% | 51,4% | 3,42 | Tempo 46%, Controle × Aggro 60% | Aggro gasta a mão devagar; Eternyr 62%; Controle em recurso 51% |
| 9 | BAL6i | **Aggro volta à lista e ao estilo aprovados do PADRAO** (`r6AggroPadrao`) | **22** (PRÓXIMO) | 25,2 | 58,3 / 53,6 / 44,9 / 46,0 / 51,6 / 47,8 / 44,2 / 43,5 / 60,0 | 40,0% | 52,0% | **2,32** | Aggro gasta a mão; Aggro abaixo do teto | Eternyr 60% |
| 10 | BAL6j | Eternyr sem o Mercador e o Dragão (`r6EternyrSemBombas`) | **22** (PRÓXIMO) | 25,4 | 57,9 / 53,8 / 45,1 / 46,7 / 52,3 / 47,5 / 45,7 / 44,0 / **57,0** | 40,0% | 51,7% | 2,31 | Eternyr abaixo do teto | — |
| 10 (60 partidas) | BAL6j | confirmação | 22, **LONGE** | 25,4 | 58,3 / 54,0 / 42,5 / 45,3 / 52,6 / 46,5 / 46,6 / 48,6 / 55,7 | 36,7% | **46,3%** | 2,25 | — | Controle em recurso 8,7 pp abaixo; Stall vence por dano 74,6% (meta 75%) |
| 11 | **BAL** (= BAL6k) | **Controle volta a ser o Controle de remoção** (passo 7) + 2 Cura de custo 3 e 2 Presa-trovão no lugar de dano de custo 1, Varredura de custo 3 e compra repentina (`r6ControleRemocao`) | 20 | 26,3 | 56,9 / 50,9 / **67,4** / 46,7 / 48,8 / 41,8 / 44,0 / 42,5 / 51,0 | **48,3%** | **69,1%** | 2,33 | Controle com cara de Controle: recurso 69%, 83% das longas, trocas 0,48, vitórias em 32 turnos | Controle forte demais (67%); Midrange × Controle 30%; Tempo × Controle 38%; duração 26,3 |

**Identidade (pedido do coordenador):**
- **Aggro do agente**: tinha 18 cartas que um Aggro não usa (Aliados de Grau 7 a 10 do Eternyr, Destruir de Grau 8, Varreduras, Fúria, Cura de custo 6, Místicas, defensivos, Arcano de custo 7). Com Grau alvo 4 elas nunca são jogadas: viram cartas mortas que só baixam a vitória (mão no 4º turno 3,4; meta 2,5). Trocar só essas cartas por cartas baratas (`BAL6i-trocas`) deixa o Aggro em **65,8%**: o agente tinha baixado o Aggro estragando o baralho, não equilibrando. Por isso o Aggro volta à lista aprovada do PADRAO (58,3%, mão 2,32) e **fica fora dos próximos objetivos**.
- **Controle do agente** (agressividade 0,6, 15 Aliados, 8 Curas, Aliados tardios do Eternyr 14/20): ganhava do Aggro, mas era um Midrange de cura. Em recurso, perde para Midrange, Stall, Ramp, Tempo, Mistura e Eternyr (20% a 49%). Testes que não resolveram: agressividade 0,3 (`BAL6i-controle03`, Controle 40%), Mercador no lugar dos Aliados tardios (`BAL6j-mercador`, Controle 31,9%), compra no lugar das 3 cartas de Aggro (`BAL6j-compra`, recurso 48%). O que funcionou foi juntar as duas coisas: o Controle de remoção com as peças do agente que seguram o Aggro (Cura que compra e Presa-trovão). Com uma Presa só, o Controle cai para 28% contra o Aggro. Com duas, ganha demais dos baralhos que o agente afinou contra o Controle antigo.
- **Tempo e Ramp do agente**: continuam no papel (Tempo com 4 Devolver, 8 remoções repentinas e Etéreos; Ramp com geração de Arcano e Aliados de custo 8). O Tempo tem 3 cartas caras (Colosso, Aliado de custo 7, Aliado de custo 6), fora do papel mas pouco relevantes. Ficam como estão.

Outros testes: Fúria a 2 PP por ponto, com 4 de dano (`BAL6i-furia4`), não muda nada (Aggro 65,4%). Curva de Grau sem somar com o bônus antigo de Grau acima do padrão (`BAL6i-semDobro`): o Eternyr sobe para 62,4%, porque o Controle do agente dependia dos Aliados tardios do Eternyr.

Próximo pedido ao agente de testes: `sim/objetivos-bal6b.json`. O Controle de remoção deve ficar abaixo do teto e manter a identidade (contra o Aggro ≥ 38%; contra Midrange e Tempo ≤ 47%; média 42–56%; trocas ≤ 0,7; vitória longa ≥ 60%). Midrange deve ter ≥ 53% contra Controle e Aggro. Tempo, ≥ 53% contra o Controle. O Aggro fica fixo.

### Rodada 6, terceira etapa: segundo ajuste do agente e fechamento

O agente de testes atingiu `sim/objetivos-bal6b.json` (1.653 tentativas, confirmado com 240 partidas por confronto). Snapshot em `sim/baralhos-bal6b-agente.json` (Controle, Midrange e Tempo). Medido com 30 partidas por confronto, salvo indicação.

| Passo | Variante | Mudança | Critérios (de 24) | Turnos | Vitória média | Controle × Aggro | Controle em recurso | Melhorou | Piorou |
|---|---|---|---|---|---|---|---|---|---|
| 12 | BAL6l | + baralhos do 2º ajuste do agente (`r6BaralhosAgente2`) | 18 | 27,5 | 60,0 / 55,2 / 44,4 / 48,5 / 46,5 / 43,7 / 54,2 / 42,1 / 55,4 | 33,3% | 50,7% | Tempo 54%, Tempo × Controle 73% | duração 27,5 (o Controle em agressividade 0,1 quase não ataca: partidas de Controle com 38 turnos de média, 45 derrotas por impasse em 1.620) |
| 13 | BAL6m | Controle em agressividade 0,3, como o Controle aprovado (era 0,1) | 21 | **25,7** | 59,8 / 54,2 / 54,4 / 46,0 / 44,5 / 43,5 / 52,7 / 40,8 / 54,2 | 35,0% | 49,9% | duração dentro da meta | — |
| 14 | BAL6n | Controle troca 2 Aliados de ataque sem texto (custo 5 e custo 6 agressivo) por 2 Estruturas que compram ao entrar | 21 | 26,0 | 59,2 / 56,7 / 52,7 / 44,8 / 46,7 / 42,6 / 53,1 / 40,8 / 53,3 | 40,0% | 50,8% | Midrange × Controle; Controle × Aggro | recurso não sobe; com 60 partidas: 21/24 LONGE (recurso 49,9%, 5,1 pp) |
| 15 | **BAL** (= BAL6o) | Controle troca as 3 cartas de dano ao Conjurador (papel de Burn) e 1 Fera de cerco por 4 "Eternyr: comprar 3" | 20 (30 partidas); **21 com 60 e com 80 (PRÓXIMO)** | 26,4 | 59,4 / 55,2 / 44,3 / 45,4 / 48,2 / 44,8 / 54,2 / 42,7 / 55,8 | 38,3% | **56,1%** | Controle ganha em recurso (57,1% com 60 partidas) | — |

Testes que não ficaram: agressividade 0,5 no Controle (`BAL6l-c05`, igual a 0,3); dano ao Conjurador trocado por Recurso básico (`BAL6m-semQueima`, recurso 50,7%); 2 Feras trocadas por compra (`BAL6n-compra3`, recurso 59%, mas Controle × Aggro 32,5%); só o dano ao Conjurador trocado por compra (`BAL6n-queimaCompra`, recurso 52,9%).

**Identidade das montagens do 2º ajuste:**
- **Midrange** (21 Aliados ao longo da curva, remoção de dano e destruir, compra, Piromante ×4, Anjo ×2, Mercador): continua Midrange. O Piromante ×4 é o que o pedido do autor queria: uma unidade de Grau médio que pune quem vai largo.
- **Controle** (agressividade 0,1, varreduras, remoção, Anular, Cura que compra, Presa-trovão ×3, poucos finalizadores): continua Controle. Mudei só a agressividade, que deixava as partidas longas demais, e tirei as cartas de dano ao Conjurador, que são de Burn, para pôr compra.
- **Tempo** (Etéreos, Destruir repentina ×4, dano repentino, Devolver): continua Tempo. Tem 2 cartas que nunca joga: o Dragão (Grau 7) e o Aliado tardio do Eternyr (Grau 8), acima do Grau alvo 6. Servem só de pitch. Não mexi para não mudar as medidas; fica como observação.

**Confirmação** (`docs/AVALIACAO-BAL-60.md` e `docs/AVALIACAO-BAL-80.md`):
- **60 partidas por confronto: PRÓXIMO, 21/24.** Faltam duração 26,5 turnos (2,1% acima de 26), Midrange × Aggro 48,3% (1,7 pp) e Stall vencendo por dano em 72,9% (2,1 pp).
- **80 partidas por confronto: PRÓXIMO, 21/24.** Faltam duração 26,1, Controle em recurso 53,0% (2 pp) e Stall por dano 74,7% (0,3 pp).
- Os três critérios que faltam oscilam no limite conforme a amostra.

Diagnóstico e pedido de permissão: [DIAGNOSTICO-BALANCEADOR.md](DIAGNOSTICO-BALANCEADOR.md).
