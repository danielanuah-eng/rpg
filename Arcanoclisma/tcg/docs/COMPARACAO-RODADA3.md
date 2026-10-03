# Comparação de variantes (gerado por sim/comparar.ts)

32 configurações × 9 montagens × 9 (ordenado) × 30 partidas = 77760 partidas. Bot guloso simples; limite de 150 turnos; vencedor do dado começa.

Variantes: **base** = regras antigas (segundo jogador com carta e Arcano extra, sem relógio). **perm** = Estruturas com efeito + Aliados de recurso (referência para as que dependem de permanentes). **PACOTE (P)** = todas as peças da rodada 3: Arcano caro, permanentes que contam como Grau, Eternyr forte, burn com menos dano por carta, cura melhorada, stax frágil, relógio de impasse (5), segundo sem compensação, quem começa sem Grau no turno 1, Conjurador de quem começa não ataca no turno 1, Conjurador ataca grátis com Grau sem suspender. **Pacote sem: X** = o pacote com uma peça desligada. **lentos** = jogador de deck lento (só sobrevive e joga para Graus altos).

## 1. Visão geral

Meta do autor: cerca de 20 turnos. "Espalhamento" = desvio-padrão das taxas de vitória das montagens (0 = todas iguais; quanto menor, mais equilibrado).

| Variante | Terminou | Travou | Sem ação | Turnos média | Mediana | p90 | 14–26 | <10 | Começa vence | Espalhamento | Maior win | Menor win |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Base (regras antigas, sem mudanças) | 84.1% | 15.9% | 0 | 16.6 | 12 | 35 | 24.0% | 32.7% | 55.8% | 24.2 pp | Burn 92.3% | Stall 7.8% |
| REF Estruturas com efeito + Aliados de recurso | 78.3% | 21.7% | 0 | 16.3 | 12 | 31 | 28.1% | 27.9% | 57.7% | 20.4 pp | Burn 83.5% | Eternyr 21.2% |
| Perm + geração de Arcano cara | 78.8% | 21.2% | 0 | 16.3 | 12 | 31 | 27.8% | 28.8% | 57.7% | 21.1 pp | Burn 84.0% | Stall 22.0% |
| Perm + permanentes contam como Grau | 78.3% | 21.7% | 0 | 16.1 | 12 | 31 | 28.0% | 28.7% | 57.9% | 20.8 pp | Burn 84.2% | Eternyr 20.6% |
| Perm + Arcano caro + contam como Grau | 78.7% | 21.3% | 0 | 16.0 | 12 | 30 | 29.0% | 28.9% | 56.9% | 21.4 pp | Burn 85.0% | Eternyr 24.2% |
| Eternyr forte (dano em Aliado, poder tardio) | 86.5% | 13.5% | 0 | 16.9 | 12 | 37 | 24.4% | 32.6% | 56.5% | 24.5 pp | Burn 92.3% | Stall 6.9% |
| Burn: dano ao Conjurador 2,5 PP/ponto | 83.5% | 16.5% | 0 | 18.0 | 13 | 37 | 30.6% | 20.7% | 55.4% | 21.8 pp | Aggro 77.4% | Stall 8.7% |
| Burn: dano ao Conjurador 3 PP/ponto | 83.3% | 16.7% | 0 | 18.1 | 13 | 37 | 30.6% | 19.3% | 56.2% | 21.7 pp | Aggro 76.8% | Stall 8.5% |
| Cura melhorada de novo | 81.1% | 18.9% | 0 | 16.0 | 12 | 33 | 25.3% | 33.7% | 56.3% | 24.2 pp | Burn 91.9% | Stall 9.3% |
| Segundo sem carta nem Arcano extra | 83.6% | 16.4% | 0 | 16.7 | 12 | 35 | 23.8% | 32.5% | 65.0% | 21.9 pp | Burn 89.2% | Stall 11.2% |
| REF Conjurador ataca grátis, Grau sem suspender | 89.0% | 11.0% | 0 | 13.7 | 10 | 26 | 22.4% | 46.1% | 55.2% | 27.0 pp | Burn 91.6% | Stall 10.7% |
| Conj + primeiro Conjurador não ataca no turno 1 | 88.4% | 11.6% | 0 | 14.2 | 10 | 28 | 23.3% | 43.1% | 54.4% | 26.7 pp | Burn 92.1% | Stall 10.3% |
| REF Estruturas com efeito (stax padrão) | 82.0% | 18.0% | 0 | 16.6 | 12 | 34 | 25.3% | 33.3% | 56.1% | 22.5 pp | Burn 90.6% | Stall 19.4% |
| Stax frágil | 79.3% | 20.7% | 0 | 16.6 | 12 | 34 | 25.3% | 31.3% | 56.7% | 22.6 pp | Burn 91.0% | Stall 21.8% |
| Relógio de impasse: 3 turnos | 100.0% | 0.0% | 0 | 18.8 | 11 | 56 | 16.5% | 43.3% | 51.3% | 22.2 pp | Burn 92.3% | Controle 20.4% |
| Relógio de impasse: 5 turnos | 100.0% | 0.0% | 0 | 23.7 | 13 | 63 | 19.8% | 29.1% | 53.4% | 21.7 pp | Burn 92.3% | Stall 15.6% |
| Relógio de impasse: 8 turnos | 100.0% | 0.0% | 0 | 25.2 | 14 | 70 | 20.2% | 27.5% | 54.1% | 21.7 pp | Burn 92.3% | Stall 15.0% |
| PACOTE (tudo) | 98.8% | 1.2% | 0 | 23.5 | 14 | 61 | 26.3% | 19.4% | 45.4% | 19.3 pp | Aggro 80.2% | Eternyr 23.0% |
| Pacote sem: geração de Arcano cara | 99.1% | 0.9% | 0 | 24.0 | 14 | 61 | 26.0% | 18.9% | 46.7% | 19.3 pp | Aggro 81.7% | Eternyr 22.1% |
| Pacote sem: permanentes contam como Grau | 98.9% | 1.1% | 0 | 23.4 | 14 | 60 | 26.8% | 18.9% | 44.2% | 18.3 pp | Aggro 78.8% | Eternyr 25.9% |
| Pacote sem: Eternyr forte | 99.1% | 0.9% | 0 | 24.9 | 14 | 63 | 25.8% | 18.3% | 45.3% | 19.3 pp | Aggro 80.2% | Eternyr 23.4% |
| Pacote sem: burn com menos dano por carta | 99.0% | 1.0% | 0 | 21.8 | 12 | 60 | 21.5% | 27.7% | 46.3% | 22.5 pp | Burn 88.8% | Eternyr 20.6% |
| Pacote sem: cura melhorada | 98.8% | 1.2% | 0 | 22.9 | 14 | 60 | 26.7% | 19.1% | 46.4% | 19.2 pp | Aggro 80.2% | Eternyr 24.5% |
| Pacote sem: segundo sem carta nem Arcano extra | 98.8% | 1.2% | 0 | 23.1 | 14 | 61 | 25.0% | 23.3% | 38.9% | 18.7 pp | Aggro 76.5% | Eternyr 22.2% |
| Pacote sem: primeiro Conjurador não ataca no turno 1 | 98.8% | 1.2% | 0 | 22.8 | 14 | 60 | 26.5% | 18.1% | 47.6% | 20.2 pp | Aggro 79.6% | Eternyr 22.3% |
| Pacote sem: stax frágil | 98.2% | 1.8% | 0 | 21.2 | 14 | 52 | 25.8% | 22.0% | 44.5% | 19.9 pp | Aggro 80.4% | Eternyr 26.4% |
| Pacote sem: relógio de impasse (5) | 81.9% | 18.1% | 0 | 16.1 | 13 | 29 | 33.5% | 19.7% | 45.6% | 21.3 pp | Aggro 80.7% | Eternyr 21.6% |
| Pacote sem: quem começa sem Grau no turno 1 | 99.0% | 1.0% | 0 | 23.5 | 14 | 61 | 23.1% | 24.4% | 60.8% | 18.0 pp | Aggro 79.4% | Eternyr 27.0% |
| Pacote sem: Conjurador ataca grátis e Grau sem suspender | 98.5% | 1.5% | 0 | 26.4 | 17 | 62 | 30.3% | 12.9% | 46.2% | 15.9 pp | Aggro 77.9% | Eternyr 25.5% |
| Base com jogador lento (sobreviver / Grau alto) | 66.9% | 33.1% | 0 | 12.5 | 10 | 20 | 22.4% | 41.1% | 55.6% | 36.5 pp | Burn 92.3% | Controle 0.0% |
| Perm com jogador lento | 62.6% | 37.4% | 0 | 12.8 | 11 | 20 | 25.8% | 34.9% | 56.9% | 35.6 pp | Burn 86.4% | Controle 0.0% |
| Pacote com jogador lento | 98.3% | 1.7% | 0 | 28.1 | 14 | 65 | 19.0% | 19.2% | 47.0% | 24.8 pp | Aggro 83.8% | Eternyr 15.2% |

## 2. Taxa de vitória média por montagem (contra as outras, sem espelho)

| Variante | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| Base (regras antigas, sem mudanças) | 73.1% | 62.5% | 26.7% | 92.3% | 7.8% | 31.0% | 45.8% | 49.8% | 38.8% |
| REF Estruturas com efeito + Aliados de recurso | 71.1% | 63.8% | 27.6% | 83.5% | 26.5% | 37.8% | 43.6% | 47.4% | 21.2% |
| Perm + geração de Arcano cara | 70.5% | 66.9% | 27.6% | 84.0% | 22.0% | 37.2% | 40.0% | 51.9% | 24.0% |
| Perm + permanentes contam como Grau | 70.5% | 67.9% | 27.8% | 84.2% | 28.4% | 37.0% | 41.5% | 46.6% | 20.6% |
| Perm + Arcano caro + contam como Grau | 70.4% | 68.8% | 24.9% | 85.0% | 24.5% | 37.0% | 41.1% | 51.0% | 24.2% |
| Eternyr forte (dano em Aliado, poder tardio) | 74.3% | 61.6% | 26.1% | 92.3% | 6.9% | 31.5% | 49.2% | 50.5% | 37.9% |
| Burn: dano ao Conjurador 2,5 PP/ponto | 77.4% | 70.7% | 29.0% | 74.4% | 8.7% | 33.4% | 44.8% | 51.0% | 39.8% |
| Burn: dano ao Conjurador 3 PP/ponto | 76.8% | 72.3% | 29.5% | 72.5% | 8.5% | 33.4% | 42.7% | 53.4% | 40.2% |
| Cura melhorada de novo | 72.6% | 64.0% | 25.3% | 91.9% | 9.3% | 30.1% | 44.4% | 47.4% | 35.9% |
| Segundo sem carta nem Arcano extra | 69.8% | 59.4% | 28.1% | 89.2% | 11.2% | 36.9% | 48.2% | 49.8% | 36.9% |
| REF Conjurador ataca grátis, Grau sem suspender | 79.0% | 64.0% | 20.4% | 91.6% | 10.7% | 23.3% | 54.5% | 62.4% | 27.5% |
| Conj + primeiro Conjurador não ataca no turno 1 | 78.4% | 63.2% | 20.1% | 92.1% | 10.3% | 23.7% | 53.4% | 61.2% | 29.3% |
| REF Estruturas com efeito (stax padrão) | 71.7% | 65.4% | 26.9% | 90.6% | 19.4% | 36.4% | 41.7% | 45.5% | 27.0% |
| Stax frágil | 70.1% | 65.6% | 22.5% | 91.0% | 21.8% | 33.1% | 42.8% | 43.9% | 29.2% |
| Relógio de impasse: 3 turnos | 74.2% | 60.8% | 20.4% | 92.3% | 24.4% | 35.8% | 49.0% | 55.8% | 37.3% |
| Relógio de impasse: 5 turnos | 72.9% | 58.5% | 29.6% | 92.3% | 15.6% | 37.3% | 48.1% | 54.0% | 41.7% |
| Relógio de impasse: 8 turnos | 72.9% | 58.5% | 31.7% | 92.3% | 15.0% | 36.5% | 47.3% | 54.0% | 41.9% |
| PACOTE (tudo) | 80.2% | 61.7% | 27.7% | 73.6% | 37.6% | 33.8% | 50.6% | 60.6% | 23.0% |
| Pacote sem: geração de Arcano cara | 81.7% | 62.8% | 28.5% | 71.8% | 39.4% | 33.1% | 50.6% | 59.2% | 22.1% |
| Pacote sem: permanentes contam como Grau | 78.8% | 62.4% | 28.6% | 71.8% | 36.3% | 34.9% | 49.7% | 60.9% | 25.9% |
| Pacote sem: Eternyr forte | 80.2% | 61.7% | 28.8% | 73.6% | 36.1% | 34.3% | 49.8% | 61.7% | 23.4% |
| Pacote sem: burn com menos dano por carta | 76.9% | 56.3% | 27.9% | 88.8% | 32.1% | 29.6% | 52.7% | 63.7% | 20.6% |
| Pacote sem: cura melhorada | 80.2% | 63.5% | 25.9% | 73.4% | 37.4% | 34.7% | 51.5% | 57.9% | 24.5% |
| Pacote sem: segundo sem carta nem Arcano extra | 76.5% | 62.1% | 28.3% | 73.5% | 38.5% | 35.2% | 49.9% | 62.6% | 22.2% |
| Pacote sem: primeiro Conjurador não ataca no turno 1 | 79.6% | 62.9% | 26.4% | 75.6% | 36.1% | 32.1% | 52.8% | 60.9% | 22.3% |
| Pacote sem: stax frágil | 80.4% | 63.8% | 26.6% | 75.7% | 28.3% | 38.7% | 49.1% | 59.7% | 26.4% |
| Pacote sem: relógio de impasse (5) | 80.7% | 68.9% | 22.5% | 73.6% | 36.2% | 28.3% | 48.2% | 56.2% | 21.6% |
| Pacote sem: quem começa sem Grau no turno 1 | 79.4% | 62.1% | 27.9% | 73.1% | 38.1% | 36.5% | 49.0% | 56.0% | 27.0% |
| Pacote sem: Conjurador ataca grátis e Grau sem suspender | 77.9% | 62.4% | 33.2% | 64.6% | 38.4% | 40.0% | 52.1% | 54.4% | 25.5% |
| Base com jogador lento (sobreviver / Grau alto) | 76.8% | 71.1% | 0.0% | 92.3% | 0.0% | 0.4% | 54.3% | 58.8% | 0.0% |
| Perm com jogador lento | 76.0% | 73.7% | 0.0% | 86.4% | 0.0% | 0.4% | 53.2% | 58.0% | 0.0% |
| Pacote com jogador lento | 83.8% | 64.2% | 19.9% | 77.2% | 37.1% | 22.4% | 59.8% | 68.1% | 15.2% |

## 3. Turnos médios por montagem

| Variante | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| Base (regras antigas, sem mudanças) | 12.3 | 17.3 | 21.4 | 8.8 | 20.5 | 17.9 | 14.9 | 18.9 | 20.6 |
| REF Estruturas com efeito + Aliados de recurso | 13.1 | 17.6 | 20.9 | 10.2 | 20.4 | 17.0 | 15.8 | 18.5 | 17.1 |
| Perm + geração de Arcano cara | 12.9 | 17.7 | 20.8 | 10.2 | 21.2 | 17.2 | 15.9 | 18.3 | 16.3 |
| Perm + permanentes contam como Grau | 12.8 | 16.9 | 20.4 | 10.2 | 21.1 | 16.8 | 15.7 | 18.3 | 17.0 |
| Perm + Arcano caro + contam como Grau | 12.4 | 17.1 | 20.1 | 10.1 | 21.3 | 17.1 | 15.3 | 17.7 | 16.4 |
| Eternyr forte (dano em Aliado, poder tardio) | 11.9 | 17.4 | 21.6 | 8.8 | 21.3 | 18.3 | 14.4 | 18.9 | 21.3 |
| Burn: dano ao Conjurador 2,5 PP/ponto | 13.2 | 17.9 | 22.0 | 11.8 | 21.5 | 19.1 | 16.2 | 21.3 | 21.3 |
| Burn: dano ao Conjurador 3 PP/ponto | 13.4 | 18.0 | 22.3 | 12.1 | 21.5 | 19.1 | 16.6 | 21.7 | 21.2 |
| Cura melhorada de novo | 12.4 | 17.1 | 19.8 | 8.9 | 19.3 | 17.3 | 14.7 | 19.1 | 19.3 |
| Segundo sem carta nem Arcano extra | 12.4 | 17.0 | 20.9 | 8.9 | 21.3 | 18.8 | 15.2 | 18.7 | 21.3 |
| REF Conjurador ataca grátis, Grau sem suspender | 9.9 | 14.5 | 15.3 | 7.6 | 17.9 | 15.3 | 11.9 | 15.0 | 17.6 |
| Conj + primeiro Conjurador não ataca no turno 1 | 10.3 | 15.2 | 16.1 | 7.9 | 18.2 | 15.7 | 12.3 | 15.8 | 18.1 |
| REF Estruturas com efeito (stax padrão) | 12.5 | 17.5 | 21.5 | 9.1 | 20.8 | 18.4 | 15.1 | 19.2 | 19.2 |
| Stax frágil | 13.2 | 17.6 | 19.9 | 9.4 | 21.0 | 18.5 | 15.7 | 20.2 | 18.9 |
| Relógio de impasse: 3 turnos | 12.1 | 20.2 | 14.1 | 8.6 | 29.8 | 20.1 | 14.4 | 23.7 | 22.1 |
| Relógio de impasse: 5 turnos | 12.8 | 22.2 | 24.5 | 8.8 | 36.0 | 29.4 | 16.9 | 25.8 | 31.6 |
| Relógio de impasse: 8 turnos | 12.9 | 22.9 | 27.3 | 8.8 | 38.5 | 31.5 | 17.4 | 26.7 | 34.3 |
| PACOTE (tudo) | 14.4 | 25.1 | 21.6 | 12.4 | 42.8 | 25.3 | 17.2 | 27.8 | 21.4 |
| Pacote sem: geração de Arcano cara | 15.0 | 24.9 | 21.9 | 12.5 | 44.8 | 26.8 | 17.3 | 27.7 | 23.0 |
| Pacote sem: permanentes contam como Grau | 14.6 | 24.6 | 22.2 | 12.4 | 42.7 | 25.9 | 17.0 | 27.3 | 22.0 |
| Pacote sem: Eternyr forte | 14.9 | 25.9 | 22.8 | 12.4 | 44.2 | 26.6 | 17.5 | 28.6 | 28.3 |
| Pacote sem: burn com menos dano por carta | 13.7 | 24.3 | 21.4 | 9.9 | 40.9 | 24.2 | 16.2 | 22.7 | 20.4 |
| Pacote sem: cura melhorada | 14.5 | 24.2 | 21.2 | 12.3 | 41.1 | 25.5 | 15.8 | 27.7 | 21.3 |
| Pacote sem: segundo sem carta nem Arcano extra | 14.2 | 24.4 | 22.4 | 11.6 | 42.2 | 26.2 | 17.0 | 27.0 | 20.6 |
| Pacote sem: primeiro Conjurador não ataca no turno 1 | 14.2 | 23.9 | 21.1 | 12.0 | 42.7 | 25.1 | 16.6 | 26.7 | 21.0 |
| Pacote sem: stax frágil | 13.0 | 23.6 | 20.4 | 11.7 | 34.9 | 23.9 | 15.7 | 25.1 | 20.7 |
| Pacote sem: relógio de impasse (5) | 12.4 | 16.4 | 17.7 | 12.4 | 23.5 | 16.7 | 14.6 | 18.7 | 16.9 |
| Pacote sem: quem começa sem Grau no turno 1 | 14.1 | 24.1 | 22.0 | 11.6 | 43.5 | 27.4 | 16.9 | 26.7 | 22.2 |
| Pacote sem: Conjurador ataca grátis e Grau sem suspender | 15.9 | 27.1 | 27.8 | 14.4 | 45.0 | 28.5 | 19.3 | 31.7 | 24.9 |
| Base com jogador lento (sobreviver / Grau alto) | 12.0 | 14.7 | 11.6 | 8.8 | 12.0 | 11.1 | 13.4 | 16.1 | 12.5 |
| Perm com jogador lento | 12.2 | 15.1 | 11.4 | 10.2 | 12.9 | 11.8 | 13.8 | 15.6 | 12.0 |
| Pacote com jogador lento | 15.2 | 25.9 | 32.3 | 13.2 | 49.0 | 34.9 | 20.3 | 30.1 | 28.6 |

## 4. Partidas travadas por montagem

| Variante | Aggro | Midrange | Controle | Burn | Stall | Ramp | Tempo | Mistura | Eternyr |
|---|---|---|---|---|---|---|---|---|---|
| Base (regras antigas, sem mudanças) | 1.0% | 11.0% | 11.8% | 0.0% | 36.9% | 25.3% | 4.7% | 14.9% | 26.9% |
| REF Estruturas com efeito + Aliados de recurso | 3.1% | 19.0% | 19.8% | 0.0% | 54.5% | 28.2% | 5.1% | 19.8% | 37.8% |
| Perm + geração de Arcano cara | 2.9% | 20.0% | 18.6% | 0.0% | 51.4% | 29.2% | 5.3% | 19.4% | 34.9% |
| Perm + permanentes contam como Grau | 3.1% | 19.4% | 18.8% | 0.0% | 53.5% | 29.6% | 5.3% | 19.6% | 37.1% |
| Perm + Arcano caro + contam como Grau | 3.9% | 22.2% | 17.1% | 0.0% | 51.8% | 27.3% | 5.3% | 21.4% | 33.7% |
| Eternyr forte (dano em Aliado, poder tardio) | 1.0% | 9.8% | 10.0% | 0.0% | 34.3% | 23.9% | 4.5% | 12.9% | 15.7% |
| Burn: dano ao Conjurador 2,5 PP/ponto | 1.4% | 11.4% | 11.8% | 0.0% | 39.2% | 25.3% | 5.7% | 16.5% | 26.3% |
| Burn: dano ao Conjurador 3 PP/ponto | 2.0% | 11.4% | 11.8% | 0.0% | 40.2% | 25.3% | 5.9% | 16.9% | 26.5% |
| Cura melhorada de novo | 1.8% | 11.2% | 17.8% | 0.0% | 44.9% | 28.2% | 6.7% | 17.8% | 31.4% |
| Segundo sem carta nem Arcano extra | 2.0% | 11.8% | 13.7% | 0.0% | 36.9% | 25.3% | 3.7% | 17.6% | 27.3% |
| REF Conjurador ataca grátis, Grau sem suspender | 0.8% | 8.0% | 5.5% | 0.4% | 30.6% | 18.4% | 2.7% | 8.6% | 15.3% |
| Conj + primeiro Conjurador não ataca no turno 1 | 0.8% | 8.0% | 6.5% | 0.2% | 31.6% | 19.8% | 3.1% | 8.0% | 17.8% |
| REF Estruturas com efeito (stax padrão) | 2.5% | 13.3% | 13.7% | 0.0% | 49.0% | 24.3% | 4.7% | 17.5% | 29.4% |
| Stax frágil | 2.9% | 16.9% | 18.8% | 0.0% | 57.3% | 27.3% | 4.7% | 18.8% | 32.7% |
| Relógio de impasse: 3 turnos | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| Relógio de impasse: 5 turnos | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| Relógio de impasse: 8 turnos | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% | 0.0% |
| PACOTE (tudo) | 0.0% | 0.0% | 0.8% | 0.6% | 1.4% | 3.9% | 0.0% | 1.0% | 2.2% |
| Pacote sem: geração de Arcano cara | 0.0% | 0.2% | 0.4% | 0.6% | 1.6% | 2.4% | 0.4% | 0.4% | 1.8% |
| Pacote sem: permanentes contam como Grau | 0.0% | 0.2% | 1.0% | 0.6% | 3.1% | 1.4% | 1.0% | 0.6% | 1.0% |
| Pacote sem: Eternyr forte | 0.0% | 0.0% | 0.8% | 0.6% | 1.2% | 2.5% | 0.0% | 1.0% | 0.4% |
| Pacote sem: burn com menos dano por carta | 0.0% | 0.0% | 0.6% | 0.0% | 1.4% | 3.7% | 0.0% | 0.2% | 2.2% |
| Pacote sem: cura melhorada | 0.0% | 0.0% | 0.2% | 0.6% | 2.0% | 3.5% | 0.4% | 0.6% | 2.4% |
| Pacote sem: segundo sem carta nem Arcano extra | 0.0% | 0.4% | 0.6% | 0.2% | 1.4% | 3.3% | 0.2% | 0.4% | 2.5% |
| Pacote sem: primeiro Conjurador não ataca no turno 1 | 0.0% | 0.0% | 0.6% | 0.2% | 1.8% | 3.9% | 0.2% | 0.4% | 2.0% |
| Pacote sem: stax frágil | 0.0% | 0.4% | 0.4% | 0.6% | 4.3% | 4.1% | 0.6% | 2.0% | 1.4% |
| Pacote sem: relógio de impasse (5) | 3.7% | 19.6% | 11.8% | 0.6% | 53.9% | 24.1% | 5.5% | 21.0% | 15.7% |
| Pacote sem: quem começa sem Grau no turno 1 | 0.0% | 0.0% | 0.6% | 0.2% | 1.0% | 3.5% | 0.4% | 0.6% | 2.0% |
| Pacote sem: Conjurador ataca grátis e Grau sem suspender | 0.0% | 0.2% | 0.8% | 0.0% | 1.8% | 4.9% | 0.0% | 1.2% | 3.5% |
| Base com jogador lento (sobreviver / Grau alto) | 3.7% | 21.0% | 52.0% | 0.0% | 57.8% | 52.4% | 15.1% | 27.1% | 58.8% |
| Perm com jogador lento | 7.5% | 29.4% | 55.7% | 0.6% | 71.2% | 56.1% | 18.6% | 33.1% | 56.9% |
| Pacote com jogador lento | 0.0% | 0.0% | 0.4% | 0.6% | 0.6% | 5.3% | 0.0% | 0.6% | 5.3% |

## 5. Uso das mecânicas (média por jogador por partida)

| Variante | Grau posto | Susp. Grau | Pitch | Arcano dissipado | Descartes | Atq Aliado | Atq Conj | Blq Conj | Ativ. mana | Ativ. outras | Anul. oferecidas | Anul. pagas | Dano efeito | Dano combate | Dano guardado | Dano do Conj (% do dano ao Conj) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Base (regras antigas, sem mudanças) | 5.66 | 28.4 | 2.97 | 1.05 | 0.56 | 3.15 | 0.00 | 0.39 | 0.00 | 0.00 | 0.00 | 0.00 | 2.79 | 5.06 | 1.05 | 0.0% |
| REF Estruturas com efeito + Aliados de recurso | 5.56 | 26.1 | 2.81 | 0.74 | 0.65 | 3.38 | 0.00 | 0.47 | 2.59 | 1.74 | 0.00 | 0.00 | 3.03 | 5.57 | 0.85 | 0.0% |
| Perm + geração de Arcano cara | 5.56 | 25.3 | 2.79 | 0.82 | 0.62 | 3.32 | 0.00 | 0.45 | 3.34 | 1.88 | 0.00 | 0.00 | 2.98 | 5.63 | 0.83 | 0.0% |
| Perm + permanentes contam como Grau | 5.53 | 25.8 | 2.83 | 0.73 | 0.62 | 3.31 | 0.00 | 0.49 | 2.42 | 1.78 | 0.00 | 0.00 | 3.02 | 5.51 | 0.83 | 0.0% |
| Perm + Arcano caro + contam como Grau | 5.56 | 24.8 | 2.77 | 0.80 | 0.61 | 3.20 | 0.00 | 0.47 | 2.96 | 1.80 | 0.00 | 0.00 | 3.00 | 5.60 | 0.77 | 0.0% |
| Eternyr forte (dano em Aliado, poder tardio) | 5.77 | 29.2 | 3.05 | 1.05 | 0.71 | 3.19 | 0.00 | 0.40 | 0.00 | 0.00 | 0.00 | 0.00 | 2.77 | 5.18 | 1.13 | 0.0% |
| Burn: dano ao Conjurador 2,5 PP/ponto | 5.91 | 31.5 | 3.20 | 1.10 | 0.59 | 3.53 | 0.00 | 0.44 | 0.00 | 0.00 | 0.00 | 0.00 | 2.27 | 5.59 | 1.12 | 0.0% |
| Burn: dano ao Conjurador 3 PP/ponto | 5.93 | 31.9 | 3.22 | 1.12 | 0.59 | 3.60 | 0.00 | 0.45 | 0.00 | 0.00 | 0.00 | 0.00 | 2.19 | 5.70 | 1.12 | 0.0% |
| Cura melhorada de novo | 5.54 | 26.9 | 2.87 | 1.01 | 0.57 | 3.12 | 0.00 | 0.41 | 0.00 | 0.00 | 0.00 | 0.00 | 2.86 | 5.04 | 1.02 | 0.0% |
| Segundo sem carta nem Arcano extra | 5.73 | 28.4 | 3.08 | 0.89 | 0.42 | 3.14 | 0.00 | 0.38 | 0.00 | 0.00 | 0.00 | 0.00 | 2.76 | 5.06 | 1.05 | 0.0% |
| REF Conjurador ataca grátis, Grau sem suspender | 5.36 | 21.6 | 2.40 | 0.90 | 0.34 | 2.15 | 1.77 | 1.93 | 0.00 | 0.00 | 0.00 | 0.00 | 2.51 | 6.59 | 0.76 | 37.7% |
| Conj + primeiro Conjurador não ataca no turno 1 | 5.45 | 22.9 | 2.50 | 0.92 | 0.35 | 2.31 | 1.51 | 1.68 | 0.00 | 0.00 | 0.00 | 0.00 | 2.60 | 6.34 | 0.82 | 32.5% |
| REF Estruturas com efeito (stax padrão) | 5.65 | 28.0 | 2.87 | 0.65 | 0.72 | 3.57 | 0.00 | 0.47 | 1.79 | 1.78 | 0.00 | 0.00 | 2.86 | 5.67 | 1.02 | 0.0% |
| Stax frágil | 5.62 | 27.9 | 2.78 | 0.67 | 0.52 | 3.41 | 0.00 | 0.44 | 0.68 | 0.85 | 0.00 | 0.00 | 2.95 | 5.38 | 1.07 | 0.0% |
| Relógio de impasse: 3 turnos | 5.44 | 34.1 | 3.30 | 1.45 | 0.62 | 2.63 | 0.00 | 0.30 | 0.00 | 0.00 | 0.00 | 0.00 | 2.08 | 3.79 | 1.15 | 0.0% |
| Relógio de impasse: 5 turnos | 6.47 | 44.6 | 4.12 | 1.73 | 1.06 | 3.18 | 0.00 | 0.37 | 0.00 | 0.00 | 0.00 | 0.00 | 2.33 | 4.64 | 1.44 | 0.0% |
| Relógio de impasse: 8 turnos | 6.57 | 46.0 | 4.21 | 1.75 | 1.14 | 3.26 | 0.00 | 0.38 | 0.00 | 0.00 | 0.00 | 0.00 | 2.35 | 4.75 | 1.48 | 0.0% |
| PACOTE (tudo) | 6.15 | 35.9 | 3.48 | 2.23 | 1.28 | 2.97 | 1.81 | 2.07 | 6.90 | 2.46 | 0.00 | 0.00 | 1.79 | 7.72 | 1.03 | 36.5% |
| Pacote sem: geração de Arcano cara | 6.28 | 39.3 | 3.75 | 1.69 | 1.24 | 3.08 | 1.79 | 2.05 | 5.99 | 2.64 | 0.00 | 0.00 | 1.79 | 7.84 | 1.08 | 35.6% |
| Pacote sem: permanentes contam como Grau | 6.17 | 34.9 | 3.32 | 2.28 | 1.32 | 3.08 | 1.80 | 2.07 | 7.56 | 2.52 | 0.00 | 0.00 | 1.79 | 7.76 | 1.08 | 36.2% |
| Pacote sem: Eternyr forte | 6.36 | 37.7 | 3.68 | 1.93 | 1.04 | 3.01 | 1.81 | 2.06 | 7.63 | 2.70 | 0.00 | 0.00 | 1.76 | 7.69 | 1.10 | 36.9% |
| Pacote sem: burn com menos dano por carta | 5.90 | 32.4 | 3.25 | 2.13 | 1.25 | 2.65 | 1.76 | 1.98 | 6.39 | 2.35 | 0.00 | 0.00 | 2.21 | 7.24 | 0.92 | 35.7% |
| Pacote sem: cura melhorada | 6.14 | 34.4 | 3.27 | 2.05 | 1.07 | 2.82 | 1.83 | 2.08 | 7.56 | 2.50 | 0.00 | 0.00 | 1.80 | 7.61 | 1.00 | 37.2% |
| Pacote sem: segundo sem carta nem Arcano extra | 6.08 | 35.4 | 3.40 | 2.45 | 1.35 | 2.93 | 1.74 | 2.01 | 6.88 | 2.44 | 0.00 | 0.00 | 1.75 | 7.54 | 1.02 | 36.0% |
| Pacote sem: primeiro Conjurador não ataca no turno 1 | 6.10 | 34.6 | 3.38 | 2.20 | 1.26 | 2.78 | 2.05 | 2.31 | 6.65 | 2.35 | 0.00 | 0.00 | 1.83 | 7.92 | 0.99 | 40.6% |
| Pacote sem: stax frágil | 6.07 | 33.2 | 3.52 | 1.30 | 1.24 | 2.87 | 1.80 | 2.05 | 6.53 | 3.04 | 0.00 | 0.00 | 1.86 | 7.80 | 0.92 | 35.9% |
| Pacote sem: relógio de impasse (5) | 5.64 | 23.6 | 2.74 | 0.98 | 0.69 | 2.64 | 1.85 | 2.09 | 2.14 | 0.92 | 0.00 | 0.00 | 2.18 | 7.63 | 0.76 | 36.1% |
| Pacote sem: quem começa sem Grau no turno 1 | 6.37 | 37.6 | 3.37 | 2.27 | 1.07 | 3.05 | 1.66 | 1.91 | 7.31 | 2.63 | 0.00 | 0.00 | 1.80 | 7.67 | 1.11 | 33.6% |
| Pacote sem: Conjurador ataca grátis e Grau sem suspender | 6.30 | 41.3 | 3.97 | 2.53 | 1.65 | 3.76 | 0.00 | 0.52 | 7.66 | 2.61 | 0.00 | 0.00 | 1.89 | 5.83 | 1.19 | 0.0% |
| Base com jogador lento (sobreviver / Grau alto) | 4.79 | 17.3 | 2.15 | 0.69 | 0.29 | 2.35 | 0.00 | 0.37 | 0.00 | 0.00 | 0.00 | 0.00 | 3.27 | 4.21 | 0.60 | 0.0% |
| Perm com jogador lento | 4.83 | 17.4 | 2.15 | 0.58 | 0.33 | 2.42 | 0.00 | 0.41 | 1.29 | 0.55 | 0.00 | 0.00 | 3.57 | 4.42 | 0.49 | 0.0% |
| Pacote com jogador lento | 7.50 | 45.1 | 3.34 | 3.24 | 1.67 | 1.77 | 1.25 | 1.40 | 8.33 | 2.06 | 0.00 | 0.00 | 1.70 | 5.20 | 0.49 | 34.4% |

Jogadas ilegais do bot (ignoradas), média por jogador por partida: 0.05 (máx. 0.32).
