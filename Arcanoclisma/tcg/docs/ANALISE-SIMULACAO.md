# Análise das simulações (rodada 1)

Dados completos: [RELATORIO-SIMULACAO.md](RELATORIO-SIMULACAO.md) (6.400 partidas, 8 montagens, `npm run simular`). Bot guloso simples, cartas básicas, Conjurador padrão. **O bot é fraco e não conhece todas as mecânicas**: o que aparece como "ignorável" pode ser limite do bot, e isso está marcado.

## Duração

- Das partidas, 86% terminaram. 14% passaram de 150 turnos (travadas).
- Nas que terminaram: média de **15,5 turnos totais** (7,7 por jogador), mediana **11**. Só 23% ficaram entre 14 e 26 turnos. 36% acabaram em menos de 10.
- A distribuição é **bimodal**: Burn (8,7 turnos) e Aggro (12,0) acabam rápido demais; Controle, Stall e Ramp passam de 17 ou travam.
- A meta de "cerca de 20 turnos" ainda não é atingida. Se "20 turnos" for por jogador (e não total), o jogo está bem mais rápido do que a meta.

## Abusáveis

1. **Dano direto ao Conjurador.** Burn venceu 90% dos confrontos e quase nunca trava nem perde. Quem jogou *Dano ao Conjurador* venceu 92% a 96%. Causa: o preço é 1,5 PP por ponto, mas o Conjurador tem só 12 de Vida e dano de efeito não é bloqueável. Duas cartas de custo 3 e 5 já matam. Sugestões: subir o preço para 2,5 a 3 PP por ponto, ou dar mais Vida aos Conjuradores, ou criar proteção contra dano de efeito (a anulação já prevista).
2. **Aggro e Midrange** vencem 73% e 65% em média. Atacar é muito melhor do que defender, porque **ataque sem bloqueio quase sempre passa** (1,3 por jogador por partida) e quem bloqueia fica suspenso e não ataca (tema tático que você escolheu).
3. **Quem começa vence 56,5%.** O segundo jogador compra 1 carta a mais e recebe 1 Arcano; ainda assim o primeiro leva vantagem, sobretudo por causa da velocidade das montagens agressivas. Moderado, mas vale testar +1 Arcano a mais ou a carta extra com o Grau.
4. **Grau como recurso dominante.** Cada jogador faz em média **25 suspensões de Grau por partida**, contra só **2,8 cartas enviadas como pitch**. O Grau gera quase todo o Arcano.
5. **Aliados grandes com descarte de Grau** (custo 8) aparecem nas vitórias, mas isso se confunde com o fato de que partidas longas favorecem quem sobreviveu.

## Ignoráveis (nesta rodada)

| Mecânica | Dado | Leitura |
|---|---|---|
| **Ataque do Conjurador** | 0,00 por partida | Ele paga 1 Arcano e fica suspenso, sem poder pôr Grau. O bot nunca vale a pena. Humanos talvez usem no fim de jogo. Ignorável como está. |
| **Cataclisma** (Arcano dissipado) | 0,9 Arcano por partida | O Arcano quase sempre é gasto antes, então a fase quase não pesa. A regra só pesa quando a sobra do pitch é grande. |
| **Limite de mão de 7** | 0,49 cartas descartadas por partida | Quase nunca acontece, porque as partidas são curtas e o Grau consome cartas. |
| **Baralho esgotar** | 0,5% dos jogadores | Irrelevante neste ritmo. Só vira questão em partidas travadas. |
| **Cartas repentinas** | 0,03 a 0,08 por partida nas baratas | O bot só reage a ataques e ao Final do oponente. Pode ser limite do bot. |
| **Místicas** | vitória 33% a 35% quando jogadas | Sem efeito passivo no motor, são só Vida que não bloqueia. Hoje não têm função. |
| **Estruturas** (custo 1 e 3) | vitória 30% e 39% | O efeito de entrada (1 Arcano, comprar 1) paga pouco; o Arcano de entrada some se não for usado. |
| **Cura** | vitória 44% e 42% | Rara; cura vale pouco contra o dano que fica guardado. |
| **Comprar cartas** | 0,58 por jogador por partida | Só aparece quando a mão está vazia. |
| **Memória de dano** | ~1 ponto guardado no início do turno | Existe, mas pesa pouco porque o dano costuma matar na hora. |

## Travamento (Stall)

- Stall contra Stall trava em **94%** das partidas. Controle contra Ramp trava em 54%; Stall contra Ramp, 64%.
- Como **esgotar o baralho não faz perder**, partidas travadas não têm saída. Falta uma **forma de vencer pelo atrito**: dano passivo, Arcano que cresce, uma carta de condição de vitória, ou um limite de turnos que force o fim.
- Stall tem **11,7% de vitória** média: hoje, defender não funciona (sem Místicas ativas, bloqueio caro, dano de efeito direto).

## O que eu recomendo ajustar primeiro (para você decidir)

1. Encarecer ou limitar o dano de efeito ao Conjurador (item 1 acima).
2. Dar função às Místicas e Estruturas (passivos ou ativações por suspensão), porque hoje não têm.
3. Definir uma saída para partidas travadas.
4. Rever a vantagem de quem começa.
5. Só depois ajustar o ritmo de duração. Muitos desses itens já mexem na duração.

## Limites desta análise

- Um único Conjurador, sem habilidades. Sem equipamentos, Extras, tokens e Valores.
- O bot não faz combos, não segura mão para blefe e não planeja o Grau. Ele reage mal com cartas repentinas.
- Taxa de vitória por carta mistura a carta com a montagem em que ela está (Burn e Aggro dominam).
