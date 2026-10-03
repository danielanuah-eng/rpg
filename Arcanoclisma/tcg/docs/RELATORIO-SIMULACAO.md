# Relatório de simulação (gerado por sim/simular.ts)

6400 partidas: 8 montagens × 8 (ordenado, inclui espelhos) × 100 partidas. Jogador guloso simples (sim/bot.ts), Conjurador padrão (12 de Vida, mão 5, compra 2, dano básico 1), cartas básicas, vencedor do dado sempre começa. Limite de 150 turnos.

> **Leia com cuidado:** o bot é simples. Os números mostram tendências do desenho das regras e das cartas, não o desempenho de jogadores humanos.

## 1. Duração

| Medida | Valor |
|---|---|
| Partidas que terminaram | 5519 (86.2%) |
| Travadas (passaram de 150 turnos) | 881 |
| Turnos totais, média | 15.5 |
| Turnos totais, mediana | 11 |
| Turnos totais, p10 / p90 | 7 / 32 |
| Turnos por jogador, média | 7.7 |
| Meta do autor | cerca de 20 turnos |

Partidas entre 14 e 26 turnos totais: 23.2%. Menos de 10 turnos: 35.5%. Mais de 40: 5.1%.

### Duração por montagem (média de turnos totais quando a montagem joga)

| Montagem | Turnos médios | Vitórias | Derrotas | Travou |
|---|---|---|---|---|
| Aggro | 12.0 | 1107 | 372 | 21 |
| Midrange | 16.4 | 868 | 468 | 164 |
| Controle | 19.4 | 367 | 943 | 190 |
| Burn | 8.7 | 1361 | 139 | 0 |
| Stall | 18.9 | 106 | 845 | 549 |
| Ramp | 17.5 | 382 | 821 | 297 |
| Tempo | 14.3 | 663 | 766 | 71 |
| Mistura | 18.0 | 665 | 596 | 239 |

### Partidas travadas por confronto (% das partidas do par que passaram de 150 turnos)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura |
|---|---|---|---|---|---|---|---|---|
| **Aggro** | 0.0% | 0.0% | 0.0% | 0.0% | 9.5% | 0.0% | 0.0% | 1.0% |
| **Midrange** | 0.0% | 24.0% | 0.5% | 0.0% | 36.5% | 1.5% | 0.5% | 31.0% |
| **Controle** | 0.0% | 0.5% | 21.0% | 0.0% | 26.0% | 54.5% | 1.0% | 2.5% |
| **Burn** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Stall** | 9.5% | 36.5% | 26.0% | 0.0% | 94.0% | 64.5% | 33.5% | 57.5% |
| **Ramp** | 0.0% | 1.5% | 54.5% | 0.0% | 64.5% | 47.0% | 0.0% | 4.5% |
| **Tempo** | 0.0% | 0.5% | 1.0% | 0.0% | 33.5% | 0.0% | 0.0% | 0.5% |
| **Mistura** | 1.0% | 31.0% | 2.5% | 0.0% | 57.5% | 4.5% | 0.5% | 45.0% |

## 2. Quem começa

O jogador que começa vence **56.5%** das partidas terminadas (o segundo jogador já compra 1 carta a mais e começa com 1 Arcano). 50% seria equilíbrio.

## 3. Matriz de vitórias (linha contra coluna, % de vitória da linha, sem contar partidas travadas)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Média |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | 50.0% | 48.0% | 94.5% | 39.0% | 92.8% | 94.0% | 78.5% | 66.2% | **73.3%** |
| **Midrange** | 52.0% | 50.0% | 79.4% | 9.0% | 94.5% | 68.0% | 77.4% | 75.4% | **65.1%** |
| **Controle** | 5.5% | 20.6% | 50.0% | 0.0% | 79.1% | 22.0% | 27.8% | 22.6% | **25.4%** |
| **Burn** | 61.0% | 91.0% | 100.0% | 50.0% | 100.0% | 99.5% | 84.0% | 95.0% | **90.1%** |
| **Stall** | 7.2% | 5.5% | 20.9% | 0.0% | 50.0% | 19.7% | 22.6% | 5.9% | **11.7%** |
| **Ramp** | 6.0% | 32.0% | 78.0% | 0.5% | 80.3% | 50.0% | 19.0% | 45.5% | **37.3%** |
| **Tempo** | 21.5% | 22.6% | 72.2% | 16.0% | 77.4% | 81.0% | 50.0% | 17.6% | **44.1%** |
| **Mistura** | 33.8% | 24.6% | 77.4% | 5.0% | 94.1% | 54.5% | 82.4% | 50.0% | **53.1%** |

## 4. Uso das mecânicas (médias por jogador por partida)

| Mecânica | Média por jogador/partida | Observação |
|---|---|---|
| Cartas colocadas no Grau | 5.37 |  |
| Suspensões de Grau para gerar Arcano | 25.36 |  |
| Cartas enviadas ao Arcano (pitch) | 2.76 |  |
| Arcano que sobrou depois de pagar com pitch | 0.70 | a regra permite sobrar; até o Cataclisma |
| Arcano dissipado no Cataclisma (desperdiçado) | 0.90 |  |
| Cartas descartadas por limite de mão (7) | 0.49 |  |
| Ataques com Aliado | 3.02 |  |
| Ataques com o Conjurador | 0.00 | o Conjurador suspenso não pode pôr Grau |
| Ataques de Aliado no turno em que entrou (Proficiente) | 0.24 |  |
| Bloqueios com Aliado | 1.25 |  |
| Bloqueios com o Conjurador | 0.39 |  |
| Ataques sofridos sem bloqueio | 1.29 |  |
| Cartas repentinas jogadas no turno do oponente | 0.68 |  |
| Cartas "comprar" jogadas | 0.58 |  |
| Dano por combate recebido pelo Conjurador | 4.90 |  |
| Dano por efeito recebido pelo Conjurador | 2.95 |  |
| Dano guardado nos Aliados no início do turno (memória de dano) | 0.98 | média de pontos de dano acumulados em campo |
| Jogadores que esgotaram o baralho | 0.5% | a regra diz que não perde por isso |

### Vencedores × perdedores (média por partida)

| Mecânica | Quem venceu | Quem perdeu |
|---|---|---|
| Cartas no Grau | 5.85 | 4.89 |
| Pitch (cartas enviadas) | 2.87 | 2.65 |
| Ataques com Aliado | 4.99 | 1.06 |
| Bloqueios com Aliado | 0.44 | 2.05 |
| Arcano dissipado | 0.67 | 1.14 |
| Dano de efeito recebido | 0.26 | 5.64 |
| Dano de combate recebido | 1.51 | 8.29 |

## 5. Cartas (todas as montagens)

Taxa de jogo = quantas vezes a carta foi jogada por partida em que estava no baralho. Vitória com/sem = taxa de vitória do jogador quando jogou a carta ao menos uma vez, ou quando não jogou.

| Carta | Jogada por partida | Vitória se jogou | Vitória se não jogou | Diferença |
|---|---|---|---|---|
| base-recurso | 0.00 | - | 48.3% | - |
| base-comprar-c2-repentina | 0.03 | 35.1% | 40.2% | -5.0 pp |
| base-dano-c1-repentina | 0.08 | 40.5% | 47.2% | -6.7 pp |
| base-curar-c4 | 0.12 | 41.7% | 30.1% | 11.7 pp |
| base-aliado-c6-agressivo | 0.13 | 56.7% | 61.9% | -5.2 pp |
| base-dano-c1 | 0.16 | 69.0% | 69.8% | -0.8 pp |
| base-curar-c2 | 0.19 | 44.4% | 38.7% | 5.7 pp |
| base-dano-c2-repentina | 0.24 | 54.3% | 51.9% | 2.4 pp |
| base-comprar-c1 | 0.25 | 52.4% | 65.6% | -13.2 pp |
| base-mistica-c4 | 0.26 | 34.9% | 30.3% | 4.6 pp |
| base-aliado-c4-equilibrado-grau5 | 0.28 | 63.9% | 33.4% | 30.5 pp |
| base-aliado-c1-equilibrado | 0.32 | 58.2% | 64.8% | -6.6 pp |
| base-dano-c3 | 0.34 | 62.0% | 70.0% | -7.9 pp |
| base-dano-c2 | 0.35 | 52.3% | 62.9% | -10.6 pp |
| base-dano-conjurador-c3 | 0.37 | 91.8% | 50.7% | 41.1 pp |
| base-dano-conjurador-c5 | 0.38 | 95.7% | 55.5% | 40.1 pp |
| base-aliado-c7-equilibrado | 0.39 | 67.3% | 31.2% | 36.1 pp |
| base-dano-c4 | 0.39 | 67.1% | 58.2% | 8.9 pp |
| base-mistica-c2 | 0.40 | 33.1% | 42.2% | -9.0 pp |
| base-aliado-c8-equilibrado | 0.40 | 59.0% | 33.2% | 25.8 pp |
| base-dano-c4-repentina | 0.41 | 48.9% | 33.4% | 15.5 pp |
| base-aliado-c2-agressivo-proficiente | 0.43 | 58.9% | 65.2% | -6.4 pp |
| base-estrutura-c1 | 0.43 | 30.4% | 50.2% | -19.8 pp |
| base-aliado-c8-equilibrado-descarta-grau | 0.43 | 71.5% | 25.4% | 46.0 pp |
| base-dano-c3-repentina | 0.47 | 43.0% | 38.4% | 4.6 pp |
| base-estrutura-c3 | 0.52 | 38.7% | 37.2% | 1.5 pp |
| base-aliado-c2-equilibrado | 0.56 | 62.8% | 60.3% | 2.5 pp |
| base-comprar-c3 | 0.56 | 49.8% | 45.8% | 4.1 pp |
| base-aliado-c4-agressivo | 0.56 | 54.0% | 55.8% | -1.8 pp |
| base-aliado-c5-equilibrado | 0.56 | 60.3% | 46.7% | 13.7 pp |
| base-aliado-c4-agressivo-proficiente | 0.56 | 56.7% | 53.7% | 3.0 pp |
| base-destruir-c3 | 0.58 | 45.4% | 33.1% | 12.3 pp |
| base-aliado-c6-equilibrado | 0.58 | 65.8% | 28.3% | 37.5 pp |
| base-aliado-c6-equilibrado-descarta-grau | 0.62 | 59.4% | 29.8% | 29.7 pp |
| base-aliado-c2-defensivo | 0.65 | 43.4% | 38.9% | 4.6 pp |
| base-aliado-c2-agressivo | 0.66 | 61.6% | 64.3% | -2.7 pp |
| base-destruir-c4-repentina | 0.70 | 40.8% | 39.5% | 1.3 pp |
| base-aliado-c4-equilibrado | 0.71 | 64.5% | 48.7% | 15.8 pp |
| base-aliado-c6-defensivo | 0.73 | 45.6% | 21.6% | 24.0 pp |
| base-aliado-c4-defensivo | 0.74 | 46.0% | 33.9% | 12.1 pp |
| base-aliado-c3-equilibrado | 1.80 | 56.0% | 55.8% | 0.2 pp |

Jogadas ilegais do bot (ignoradas): 0 em 11038 jogadores-partida.
