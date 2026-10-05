# Relatório de simulação (gerado por sim/simular.ts)

1280 partidas: 8 montagens × 8 (ordenado, inclui espelhos) × 20 partidas. Jogador guloso simples (sim/bot.ts), Conjurador padrão (12 de Vida, mão 5, compra 2, dano básico 1), cartas básicas, vencedor do dado sempre começa. Limite de 150 turnos.

> **Leia com cuidado:** o bot é simples. Os números mostram tendências do desenho das regras e das cartas, não o desempenho de jogadores humanos.

## 1. Duração

| Medida | Valor |
|---|---|
| Partidas que terminaram | 1280 (100.0%) |
| Travadas (passaram de 150 turnos) | 0 |
| Turnos totais, média | 21.5 |
| Turnos totais, mediana | 13 |
| Turnos totais, p10 / p90 | 8 / 61 |
| Turnos por jogador, média | 10.8 |
| Meta do autor | cerca de 20 turnos |

Partidas entre 14 e 26 turnos totais: 23.7%. Menos de 10 turnos: 24.1%. Mais de 40: 14.7%.

### Duração por montagem (média de turnos totais quando a montagem joga)

| Montagem | Turnos médios | Vitórias | Derrotas | Travou |
|---|---|---|---|---|
| Aggro | 13.5 | 225 | 75 | 0 |
| Midrange | 21.1 | 180 | 120 | 0 |
| Controle | 22.1 | 106 | 194 | 0 |
| Burn | 9.7 | 279 | 21 | 0 |
| Stall | 32.5 | 62 | 238 | 0 |
| Ramp | 24.5 | 118 | 182 | 0 |
| Tempo | 17.3 | 138 | 162 | 0 |
| Mistura | 27.1 | 172 | 128 | 0 |

### Partidas travadas por confronto (% das partidas do par que passaram de 150 turnos)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura |
|---|---|---|---|---|---|---|---|---|
| **Aggro** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Midrange** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Controle** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Burn** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Stall** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Ramp** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Tempo** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| **Mistura** | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |

## 2. Quem começa

O jogador que começa vence **45.5%** das partidas terminadas (o segundo jogador já compra 1 carta a mais e começa com 1 Arcano). 50% seria equilíbrio.

## 3. Matriz de vitórias (linha contra coluna, % de vitória da linha, sem contar partidas travadas)

| | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Média |
|---|---|---|---|---|---|---|---|---|---|
| **Aggro** | 50.0% | 55.0% | 92.5% | 27.5% | 97.5% | 100.0% | 80.0% | 60.0% | **73.2%** |
| **Midrange** | 45.0% | 50.0% | 80.0% | 5.0% | 80.0% | 57.5% | 85.0% | 47.5% | **57.1%** |
| **Controle** | 7.5% | 20.0% | 50.0% | 0.0% | 77.5% | 45.0% | 32.5% | 32.5% | **30.7%** |
| **Burn** | 72.5% | 95.0% | 100.0% | 50.0% | 100.0% | 100.0% | 82.5% | 97.5% | **92.5%** |
| **Stall** | 2.5% | 20.0% | 22.5% | 0.0% | 50.0% | 22.5% | 30.0% | 7.5% | **15.0%** |
| **Ramp** | 0.0% | 42.5% | 55.0% | 0.0% | 77.5% | 50.0% | 22.5% | 47.5% | **35.0%** |
| **Tempo** | 20.0% | 15.0% | 67.5% | 17.5% | 70.0% | 77.5% | 50.0% | 27.5% | **42.1%** |
| **Mistura** | 40.0% | 52.5% | 67.5% | 2.5% | 92.5% | 52.5% | 72.5% | 50.0% | **54.3%** |

## 4. Uso das mecânicas (médias por jogador por partida)

| Mecânica | Média por jogador/partida | Observação |
|---|---|---|
| Cartas colocadas no Grau | 5.95 |  |
| Suspensões de Grau para gerar Arcano | 36.90 |  |
| Cartas enviadas ao Arcano (pitch) | 3.76 |  |
| Arcano que sobrou depois de pagar com pitch | 1.13 | a regra permite sobrar; até o Cataclisma |
| Arcano dissipado no Cataclisma (desperdiçado) | 1.26 |  |
| Cartas descartadas por limite de mão (7) | 1.05 |  |
| Ataques com Aliado | 2.72 |  |
| Ataques com o Conjurador | 0.00 | o Conjurador suspenso não pode pôr Grau |
| Ataques de Aliado no turno em que entrou (Proficiente) | 0.19 |  |
| Bloqueios com Aliado | 1.06 |  |
| Bloqueios com o Conjurador | 0.39 |  |
| Ataques sofridos sem bloqueio | 1.20 |  |
| Cartas repentinas jogadas no turno do oponente | 0.63 |  |
| Cartas "comprar" jogadas | 0.87 |  |
| Dano por combate recebido pelo Conjurador | 4.66 |  |
| Dano por efeito recebido pelo Conjurador | 2.44 |  |
| Dano guardado nos Aliados no início do turno (memória de dano) | 0.57 | média de pontos de dano acumulados em campo |
| Jogadores que esgotaram o baralho | 10.8% | a regra diz que não perde por isso |

### Vencedores × perdedores (média por partida)

| Mecânica | Quem venceu | Quem perdeu |
|---|---|---|
| Cartas no Grau | 6.30 | 5.61 |
| Pitch (cartas enviadas) | 3.88 | 3.65 |
| Ataques com Aliado | 4.14 | 1.30 |
| Bloqueios com Aliado | 0.55 | 1.57 |
| Arcano dissipado | 1.07 | 1.45 |
| Dano de efeito recebido | 0.19 | 4.69 |
| Dano de combate recebido | 1.75 | 7.57 |

## 5. Cartas (todas as montagens)

Taxa de jogo = quantas vezes a carta foi jogada por partida em que estava no baralho. Vitória com/sem = taxa de vitória do jogador quando jogou a carta ao menos uma vez, ou quando não jogou.

| Carta | Jogada por partida | Vitória se jogou | Vitória se não jogou | Diferença |
|---|---|---|---|---|
| base-recurso | 0.00 | - | 48.3% | - |
| base-dano-c1-repentina | 0.04 | 57.7% | 48.0% | 9.6 pp |
| base-comprar-c2-repentina | 0.06 | 56.7% | 42.4% | 14.2 pp |
| base-dano-c1 | 0.16 | 75.0% | 69.6% | 5.4 pp |
| base-dano-c2-repentina | 0.20 | 56.0% | 53.9% | 2.1 pp |
| base-curar-c4 | 0.22 | 38.3% | 34.9% | 3.4 pp |
| base-aliado-c6-agressivo | 0.22 | 62.4% | 61.9% | 0.5 pp |
| base-curar-c2 | 0.28 | 45.5% | 39.3% | 6.2 pp |
| base-comprar-c1 | 0.30 | 52.8% | 65.3% | -12.5 pp |
| base-dano-c3 | 0.33 | 63.5% | 68.1% | -4.6 pp |
| base-dano-conjurador-c5 | 0.35 | 95.1% | 58.9% | 36.2 pp |
| base-dano-conjurador-c3 | 0.35 | 93.7% | 51.7% | 42.0 pp |
| base-dano-c2 | 0.36 | 52.9% | 59.2% | -6.3 pp |
| base-dano-c3-repentina | 0.38 | 48.9% | 41.0% | 7.9 pp |
| base-dano-c4-repentina | 0.40 | 60.8% | 35.0% | 25.7 pp |
| base-dano-c4 | 0.42 | 66.1% | 58.0% | 8.1 pp |
| base-aliado-c1-equilibrado | 0.43 | 61.7% | 64.6% | -2.9 pp |
| base-aliado-c2-agressivo-proficiente | 0.45 | 60.8% | 65.1% | -4.3 pp |
| base-aliado-c4-equilibrado-grau5 | 0.51 | 63.8% | 33.8% | 30.1 pp |
| base-aliado-c4-agressivo | 0.61 | 55.6% | 55.8% | -0.2 pp |
| base-aliado-c8-equilibrado-descarta-grau | 0.62 | 67.0% | 31.6% | 35.4 pp |
| base-mistica-c4 | 0.63 | 36.0% | 35.2% | 0.8 pp |
| base-aliado-c4-agressivo-proficiente | 0.64 | 56.8% | 54.7% | 2.1 pp |
| base-aliado-c2-equilibrado | 0.65 | 62.2% | 58.5% | 3.7 pp |
| base-aliado-c2-agressivo | 0.69 | 63.8% | 63.4% | 0.3 pp |
| base-mistica-c2 | 0.70 | 38.5% | 42.1% | -3.6 pp |
| base-aliado-c7-equilibrado | 0.72 | 65.6% | 32.6% | 33.0 pp |
| base-aliado-c5-equilibrado | 0.74 | 57.6% | 47.3% | 10.4 pp |
| base-aliado-c8-equilibrado | 0.76 | 50.6% | 36.2% | 14.4 pp |
| base-estrutura-c1 | 0.76 | 37.9% | 50.1% | -12.3 pp |
| base-aliado-c6-equilibrado | 0.78 | 61.5% | 31.2% | 30.3 pp |
| base-destruir-c4-repentina | 0.78 | 51.9% | 36.0% | 15.9 pp |
| base-estrutura-c3 | 0.81 | 41.4% | 38.6% | 2.7 pp |
| base-destruir-c3 | 0.81 | 48.5% | 32.1% | 16.4 pp |
| base-comprar-c3 | 0.88 | 50.1% | 45.7% | 4.4 pp |
| base-aliado-c4-equilibrado | 0.91 | 59.2% | 49.5% | 9.7 pp |
| base-aliado-c6-equilibrado-descarta-grau | 0.94 | 61.4% | 29.3% | 32.2 pp |
| base-aliado-c2-defensivo | 0.97 | 41.9% | 41.2% | 0.7 pp |
| base-aliado-c4-defensivo | 1.13 | 46.2% | 33.0% | 13.1 pp |
| base-aliado-c6-defensivo | 1.17 | 44.7% | 25.4% | 19.3 pp |
| base-aliado-c3-equilibrado | 2.80 | 53.8% | 57.6% | -3.8 pp |

Jogadas ilegais do bot (ignoradas): 2 em 2560 jogadores-partida.
