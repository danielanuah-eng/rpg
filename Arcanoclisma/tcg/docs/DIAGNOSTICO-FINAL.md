# Diagnóstico total (rodada 2)

Base de dados: [COMPARACAO-VARIANTES.md](COMPARACAO-VARIANTES.md) (41 configurações × 9 montagens × 9 × 40 partidas ≈ 133 mil partidas; `node sim/comparar.ts 40`). Cada variante usa as **mesmas sementes**, então as comparações são pareadas. O jogador é um bot guloso simples: **os números dizem como as regras e as cartas se comportam, não como humanos jogam.**

Referência (Base, sem nenhuma mudança): 84% terminam, 16% travam; turnos totais média 16,1 e mediana 12; só 24% entre 14 e 26 turnos e 34% com menos de 10; quem começa vence 57,5%; Burn vence 90,6% e Stall 11,0%; espalhamento das taxas de vitória 24,7 pontos percentuais (pp). Nesta rodada entrou uma nona montagem, Eternyr (late game, com poucas cartas de draw).

## 1. O que cada mudança fez, isoladamente

| Mudança | Efeito medido | Veredito |
|---|---|---|
| **A1** Burn: +2 de custo e de Grau | Burn cai de 90,6% para **59,2%**. Menos partidas acabam em menos de 10 turnos (33,5% → 23,1%). Aggro assume o topo (81,5%). | Resolve o Burn de forma simples, mas empurra o jogo para o Aggro. |
| **A2** Burn: oponente paga X (custo − 1) para anular | Burn despenca para **10,5%**; Midrange e Mistura passam de 80%. Partidas travadas dobram (16% → 32%), porque o Burn deixou de fechar jogos. O oponente paga em ~79% das ofertas (2,35 de 2,99 por partida). | Nerf excessivo com X pequeno. |
| **A3 / A4 / A5** X = custo, custo + 1, custo + 2 | Burn: 13,8% → 18,4% → **34,9%**. A5 tem o menor espalhamento entre as isoladas (20,0 pp), mas favorece mais quem começa (59,7%). | Quanto maior o X, mais viável o Burn. X = custo + 2 ainda é abaixo da meta. |
| **B** Mais recursos | Ativações de Arcano 1,2 por jogador/partida. Stall +3 pp, Ramp +3,5 pp, Eternyr −6,5 pp. Duração praticamente igual. | Efeito pequeno. Recursos já eram abundantes por causa do Grau. |
| **C** Eternyr: mais draw e mais barato | Eternyr 32,1% → 31,6%. Nenhuma mudança relevante. | Ignorável como está: as cartas de draw quase não mudam o desfecho em partidas curtas. |
| **D** Estruturas com efeito | Muito usadas (1,7 ativações de Arcano e 1,7 de outras por jogador/partida). Stall **11,0% → 24,3%**, Controle 21,0% → 24,2%. Espalhamento 24,7 → 22,9 pp. Travadas 16% → 19%. | A mudança mais útil entre as de carta. |
| **E** Cura melhor e escalável | Praticamente nenhuma mudança. D+E dá só +3,6 pp ao Stall sobre D. | Ignorável como está (ver item 3). |
| **F** Sem ações perde | **Nenhuma partida terminou por essa regra** em nenhuma das 41 configurações. | Não funciona como escrita (ver item 3). |
| **G** Quem começa não põe Grau no turno 1 | Quem começa vence **39,8%** (de 57,5%). Partidas com menos de 10 turnos: 33,5% → 26,9%. | Corrige demais: agora o segundo jogador é favorecido. |
| **G2 / G3 / G4** G + segundo sem Arcano inicial / sem carta extra / sem ambos | Quem começa vence 41,4% / 44,3% / **45,6%**. | G4 é o mais perto de 50%, mas ainda favorece o segundo. |

## 2. O ataque do Conjurador (simulações extras)

O bot passa a atacar com o Conjurador sempre que ele está livre e o oponente não tem Aliado levantado.

| Variante | Ataques do Conjurador por jogador/partida | % do dano ao Conjurador causado por ele | Duração (mediana / média) | Travadas |
|---|---|---|---|---|
| Base | 0,00 | 0% | 12 / 16,1 | 16,0% |
| H1 só o bot muda | 0,39 | 9,4% | 12 / 16,3 | 14,9% |
| H2 dano básico 2 | 0,39 | 10,0% | 12 / 16,1 | 15,1% |
| H3 ataque grátis | 0,79 | 18,3% | 12 / 15,0 | 11,6% |
| H4 Grau sem suspender o Conjurador | 0,93 | 21,2% | 11 / 15,1 | 14,0% |
| H5 dano 2 + grátis + Grau sem suspender | **2,26** | **52,8%** | 8 / 10,5 | 5,6% |

- Só mudar o bot quase não muda nada: o ataque do Conjurador custa 1 Arcano e o impede de pôr Grau no turno, e isso raramente compensa.
- **H3 e H4** aumentam o uso moderadamente e reduzem as partidas travadas.
- **H5 faz o Conjurador ser o personagem principal** (53% do dano), mas encurta demais o jogo: 63% das partidas acabam em menos de 10 turnos.
- H3 (ataque grátis) com a penalidade de Grau dá vantagem grande a quem começa (63% a 65% nas candidatas R3 e R7).

## 3. Mecânicas ignoráveis e abusáveis (visão consolidada)

**Abusáveis**
- **Burn**: resolvido por A1 ou A3–A5; ver acima. Sem mudança, vence 90%.
- **Aggro e Midrange**: vencem entre 58% e 84% em todas as configurações. Podem ser limitação do bot, que defende mal, mas é um sinal para olhar.
- **Vantagem de quem começa**: de 57,5% para 38% a 45% com G; é preciso um ponto intermediário.
- **Grau como fonte principal de Arcano**: continua com 14 a 37 suspensões por jogador/partida contra 2 a 4 cartas de pitch.

**Ignoráveis**
- **Cura (E)** e **draw do Eternyr (C)**: sem efeito mensurável. O bot só cura com 3 ou mais de dano acumulado e só joga draw com a mão pequena.
- **"Sem ações perde" (F)**: a regra nunca disparou. Mesmo nas partidas travadas, o jogador sempre tem uma ação legal (pôr Grau, atacar, ativar). Para funcionar precisa de outra definição, por exemplo um relógio de impasse (N turnos seguidos sem jogar carta, ativar ou atacar).
- **Cataclisma** (cerca de 1 Arcano dissipado por partida) e **baralho esgotado**: continuam quase irrelevantes.
- **Memória de dano**: cerca de 1 ponto guardado; chega a 1,5 quando as partidas ficam longas (A2).
- **Recursos extras (B)**: efeito pequeno.

**Passaram a ter função**
- **Estruturas** (D): as ativações acontecem em média 3,3 vezes por jogador/partida e mudam o resultado do Stall e do Controle.
- **Conjurador** (H3 a H5): ganha peso quando o ataque é grátis e não impede o Grau.
- **Pagar para anular** (A2 a A5): 3 ofertas por partida; o oponente aceita na maior parte.

## 4. Combinações

| Pacote | Mediana / média | 14–26 turnos | < 10 | Travadas | Começa vence | Espalhamento | Burn / Stall |
|---|---|---|---|---|---|---|---|
| Base | 12 / 16,1 | 24,1% | 33,5% | 16,0% | 57,5% | 24,7 pp | 90,6% / 11,0% |
| K1 B+C+D+E + A1 | 14 / 17,5 | 35,9% | 18,1% | 18,0% | 57,3% | 17,3 pp | 48,1% / 41,1% |
| K3 K1 + F + G | 14 / 17,3 | 36,5% | 14,3% | 17,2% | 38,6% | 19,0 pp | 51,4% / 36,8% |
| K4 B+C+D+E + A2 + F + G | 17 / 21,9 | 35,1% | 10,4% | 30,3% | 37,8% | 17,6 pp | 14,4% / 49,2% |
| R1 B+C+D+E + A4 + G2 + F | 18 / 23,5 | 35,1% | 7,7% | 29,4% | 37,9% | 17,0 pp | 18,8% / 48,1% |
| **R6** R1 com A5 | 18 / 23,7 | 35,0% | 7,8% | 28,5% | 37,3% | **15,8 pp** | 25,7% / 43,8% |
| **R4** R1 + Conjurador grátis e Grau sem suspender | 16 / 21,5 | 30,7% | 16,5% | 27,3% | 38,3% | 16,5 pp | 26,8% / 56,6% |
| R5 R1 + H5 completo | 11 / 15,9 | 16,7% | 32,9% | 21,1% | 40,6% | 20,6 pp | 39,4% / 65,9% |
| R3 R1 + Conjurador grátis (sem Grau sem suspender) | 16 / 20,8 | 36,1% | 11,2% | 24,5% | **63,1%** | 21,1 pp | 34,7% / 54,1% |

Leitura:
- **Todas as combinações com A2 a A5** aproximam a duração da meta (média de 21 a 24, mediana 16 a 18), mas **aumentam as partidas travadas** de 16% para quase 30%. O Burn deixa de fechar jogos.
- **A1 é o contrário**: deixa o jogo um pouco mais rápido (mediana 14) e com menos travamento, mas o Aggro passa a dominar (80%).
- **Nenhum pacote atinge a meta de 20 turnos de forma estável.** O melhor é a faixa de 14 a 26 turnos em cerca de 36% das partidas; o resto fica antes ou depois.
- **Espalhamento**: o melhor é R6 (15,8 pp). A meta ideal seria abaixo de 10 pp.
- **O Burn não se estabiliza em 45% a 55%** com anulação paga. Chega a 35% com X = custo + 2.
- **Cartas e estruturas (D)** ajudam a diversidade sem mudar a velocidade. As demais mudanças de carta (B, C, E) quase não mudam nada.

## 5. Pacote que eu recomendo agora

1. **Estruturas com efeito (D).** Mantém.
2. **Burn**: anulável pagando **X = custo + 2** (A5), ou subir mais (custo + 3) e testar. Alternativa mais simples: A1 (custo e Grau +2), aceitando o Aggro forte.
3. **Quem começa**: G4 (sem Grau no turno 1 e o segundo sem compensação). Fica em 45,6%; falta um ponto intermediário (por exemplo, o primeiro pode pôr Grau mas o segundo perde só a carta).
4. **Conjurador**: ataque grátis e Grau que não suspende o Conjurador (R4). Dá 38% do dano ao Conjurador e mantém quem começa em 38%. As travadas caem pouco (de 29,4% para 27,3%). H5 completo é rápido demais.
5. **"Sem ações perde" (F)**: trocar pela regra de relógio de impasse ou remover. Como está, não faz nada.
6. **Cura (E), draw do Eternyr (C) e recursos (B)**: não vale mexer mais sem rever o jogador automático. O problema não é o preço: é que ninguém precisa dessas cartas nessas partidas curtas.

## 6. O que ainda precisa de decisão ou de teste

- Aceitar um intervalo de 14 a 26 turnos para "cerca de 20", já que nenhuma configuração põe a maioria das partidas aí.
- Definir a regra de impasse (relógio) para substituir F.
- Testar X = custo + 3 e G com compensação parcial.
- Dar função às Místicas (continuam só Vida).
- Rodar com um bot mais forte (ou com você jogando) antes de fechar números.

## 7. Limites desta análise

- Bot guloso e simples. Defende mal, segura pouco mana para repentinas, não faz combos.
- Cartas básicas e um Conjurador padrão (12 de Vida, mão 5, compra 2).
- Sem Valores reais, equipamentos, Extras, tokens e Místicas funcionais.
- O Eternyr aqui é uma montagem de cartas neutras com draw; não é o Valor completo.
- A taxa de vitória por montagem mistura a montagem com o comportamento do bot.
