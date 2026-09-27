# 15 — Teste de escala, interações e criatividade

> Testes feitos: combate em todas as Molduras com 2, 3 e 4 jogadores e quatro dificuldades de encontro; testes de atributo em situações difíceis; Eventos sociais com Ameaça; e se as palavras resolvem problemas de forma criativa. Scripts em `calc/simular_mesa.py` (resultado em `calc/saida_mesa.md`). Propostas em P-111 a P-115 ([[09 Perguntas pra decidir]]). **Decidido e aplicado**: P-111 (régua nova), P-114 (Ameaça como pote), P-115 (cura só de Morbidyum) e uma Palavra Básica por magia.

## 1. Combate em todas as Molduras

Personagens genéricos: metade luta com arma, metade com magia no Grau máximo que o Arcano pagar; atributo principal +2/+3/+4/+5 e Esquiva 10/11/12/13 (o que um jogador razoável tem subindo de nível). Sem Hábitos, Atalhos, Dupla nem itens: **os jogadores reais são mais fortes que isso**. O mestre ativa adversários e interrompe de vez em quando.

### Com a régua atual (4 jogadores)

| Moldura | encontro padrão | difícil | chefe |
|---|---|---|---|
| 1 | 100% de vitória | 74% | 99% |
| 2 | 90% (10% todos caem) | 13% | 81% |
| 3 | 81% (19% todos caem) | 8% | 58% |
| **4** | **19% (81% todos caem)** | **0%** | **4%** |

**Achado principal: a régua dos adversários cresce mais rápido que os personagens.** A Dificuldade sobe 2 por Moldura (12 → 18) e o ataque sobe 2 (+1 → +7), mas o atributo do personagem sobe só 1 (+2 → +5) e a Esquiva também 1. Na Moldura 4, um adversário padrão acerta o personagem 80% das vezes e é acertado só 46%. Na Moldura 1 tudo funciona; da 2 em diante, desanda.

### Com a régua proposta

| Moldura | Dificuldade | Limiares | PV | Ataque | Dano |
|---|---|---|---|---|---|
| 1 | 12 | 8 / 14 | 5 | +1 | 1d10+2 |
| 2 | **13** | 11 / **21** | 6 | **+2** | 2d10+**2** |
| 3 | **14** | **16 / 30** | 7 | **+3** | 3d10+**3** |
| 4 | **15** | **22 / 42** | 8 | **+4** | 4d10+**4** |

Resultado (vitória e média de personagens caídos):

| Moldura | jogadores | padrão | difícil (+2) | muito difícil (+4) | chefe |
|---|---|---|---|---|---|
| 1 | 4 | 100% (0,4) | 95% (1,0) | 75% (2,1) | 98% (0,6) |
| 1 | 3 | 100% (0,1) | 97% (0,4) | 81% (1,2) | 96% (0,5) |
| 1 | 2 | 100% (0,0) | 98% (0,1) | 94% (0,3) | 98% (0,1) |
| 2 | 4 | 97% (0,9) | 77% (2,1) | 36% (3,3) | 96% (0,9) |
| 2 | 3 | 100% (0,2) | 92% (0,7) | 65% (1,6) | 94% (0,6) |
| 2 | 2 | 100% (0,0) | 96% (0,3) | 82% (0,7) | 96% (0,3) |
| 3 | 4 | 99% (0,5) | 93% (1,2) | 58% (2,7) | 96% (0,8) |
| 3 | 3 | 100% (0,2) | 93% (0,6) | 70% (1,6) | 95% (0,6) |
| 3 | 2 | 100% (0,0) | 97% (0,2) | 88% (0,5) | 95% (0,3) |
| 4 | 4 | 98% (0,9) | 68% (2,4) | 23% (3,6) | 88% (1,5) |
| 4 | 3 | 98% (0,3) | 84% (1,2) | 42% (2,3) | 80% (1,3) |
| 4 | 2 | 100% (0,1) | 92% (0,4) | 72% (0,9) | 83% (0,6) |

Com a régua nova, o **encontro padrão** fica estável em todas as Molduras (97–100%, o grupo perde metade dos PV), o **chefe** fica entre 80% e 98%, e o **difícil (+2)** fica entre 68% e 97%: tenso sem ser injusto. O **muito difícil (+4)** é de fato perigoso (23–94%), para usar de propósito.

### Menos jogadores

**Com 2 ou 3 jogadores, o mesmo orçamento fica mais fácil** (menos adversários na mesa e menos Cataclisma entrando para o mestre). Proposta: **+2 pontos para 2 jogadores, +1 para 3**. Duração: com menos gente, cada jogador age mais vezes (6–7 ações por luta em vez de 4–5), o que é bom: ninguém fica parado.

## 2. Testes de atributo: dá para fazer situações difíceis?

Chance de sucesso (sem ajuda / com 1 ajuda, a Vivência / com 2 ajudas, Vivência e Ajudar):

| atributo | Dif 15 | Dif 20 | Dif 25 |
|---|---|---|---|
| +0 | 43% / 58% / 80% | 17% / 25% / 47% | 8% / 10% / 20% |
| +2 | 58% / 72% / 89% | 25% / 36% / 61% | 10% / 14% / 29% |
| +3 | 65% / 78% / 92% | 31% / 43% / 68% | 11% / 17% / 34% |
| +4 | 72% / 83% / 95% | 36% / 50% / 74% | 14% / 21% / 41% |
| +5 | 78% / 88% / 97% | 43% / 58% / 80% | 17% / 25% / 47% |

- **Sim, dá.** Dificuldade 20 continua difícil para qualquer um sem ajuda, e 25 só vira possível com as duas ajudas e um atributo alto. O Crítico (dados iguais) garante sempre uns 8%: nada é impossível.
- **O teto de duas ajudas funciona**: com ele, ninguém transforma Dificuldade 25 em formalidade.
- Com duas ajudas, Dificuldade 15 vira quase certa (80–97%). Isso é bom: gastar Arcano e pedir ajuda devem resolver situações médias.

## 3. Eventos sociais e narrativos com Ameaça

Um Evento de conversa em que o grupo precisa de **3 sucessos antes de a Ameaça de 6 cair** (o guarda chamar reforços, o nobre perder a paciência). Modelado com a Ameaça **enchendo com os dados ímpares** das rolagens:

| jogadores | melhor atributo | Dif 12 | Dif 15 | Dif 18 |
|---|---|---|---|---|
| 4 | +2 | 89% / 97% / 99% | 63% / 87% / 96% | 30% / 60% / 85% |
| 4 | +4 | 96% / 99% / 100% | 83% / 95% / 99% | 52% / 80% / 94% |
| 2 | +2 | 91% / 98% / 100% | 69% / 89% / 97% | 35% / 65% / 87% |

(sem ajuda / 1 ajuda / 2 ajudas)

- **A tensão funciona**: numa conversa de Dificuldade 15, sem gastar nada o grupo perde um terço das vezes; gastando Arcano, quase sempre vence. Na Dificuldade 18, vira uma cena realmente difícil.
- **Achado**: a regra atual diz que a Ameaça "acontece quando o mestre pagar". Como o mestre começa com 2 Cataclisma por jogador (8 numa mesa de 4), ele pode pagar uma Ameaça de 6 **na hora**, e aí o anúncio não cria suspense. **Proposta (P-114)**: a Ameaça vira um **pote na mesa**: o mestre **coloca nela** o Cataclisma que quiser, quando quiser, à vista de todos (o que vai para o pote não volta), e ela dispara quando enche. Continua sendo escolha dele (é o gasto livre que você quis), mas os jogadores veem o perigo subir e o mestre precisa decidir entre alimentar a Ameaça ou gastar em outra coisa.

## 4. As palavras dão para ser usadas de forma criativa?

Sete problemas comuns de mesa, resolvidos só com as palavras de cada Valor e as Palavras Básicas, **na Moldura 1** (no máximo 2 palavras de Valor, Grau até 2). ✅ = solução direta · 🟡 = solução criativa, que depende da relação do personagem com a palavra (e ganha o +1 de Relação quando convence) · ❌ = o Valor não alcança sozinho.

| Valor | Atravessar um abismo | Convencer um guarda | Estabilizar um aliado | Descobrir quem mente | Parar uma briga sem ferir | Sair de uma cela | Proteger o grupo de flechas |
|---|---|---|---|---|---|---|---|
| **Conflitüo** | 🟡 *Lança*: cravar e saltar | 🟡 *Estandarte*: mostrar a patente | 🟡 *Cicatriz*: fechar como se fecha uma ferida de guerra | 🟡 *Provocar*: quem mente se trai | ✅ *Desarmar* | ✅ *Romper* + objeto | ✅ *Escudo* + cúpula |
| **Sublimeyn** | ✅ *Cristalizar* + ar: ponte de vidro | 🟡 *Vapor* + distrair: tirar o guarda do caminho | 🟡 *Fogo*: cauterizar | 🟡 *Sal*: o suor do mentiroso | ✅ *Lama* + chão | ✅ *Derreter* + metal | ✅ *Vidro* + cúpula |
| **Dominum** | 🟡 *Mandar* + fera: um bicho carrega | ✅ *Obrigar* | 🟡 *Carne* + obrigar: mandar o corpo aguentar | ✅ *Obrigar* + falar | ✅ *Parar* | ✅ *Arrombar* | 🟡 *Mandar*: os arqueiros baixam o arco |
| **Persona** | ✅ *Fortalecer* + músculo: o salto | ✅ *Encantar* | ✅ *Remodelar* + pele | ✅ *Sentir*: medo, culpa | ✅ *Acalmar* | 🟡 *Encantar*: o carcereiro abre | 🟡 *Amedrontar*: os arqueiros hesitam |
| **Alvorum** | 🟡 *Luz* + linha: uma ponte de luz (Grau 3) | 🟡 *Sonho*: o guarda vê uma ordem que não recebeu | 🟡 *Aurora*: o calor que mantém acordado | ✅ *Revelar* | ✅ *Eclipsar*: todo mundo no escuro | 🟡 *Ocultar*: sumir quando a porta abrir | ✅ *Espelho* + cúpula |
| **Morbidyum** | 🟡 *Etéreo*: flutuar como fantasma | 🟡 *Presságio*: mostrar o que vai acontecer com ele | ✅ *Curar* | 🟡 *Sussurrar*: os mortos contam | ✅ *Esvaziar*: a vontade de brigar | ✅ *Apodrecer* + objeto | ✅ *Osso* + barreira |
| **Cognitium** | 🟡 com a Palavra Básica: *Imaginar* + escalar — a ponte imaginada vira apoio para os pés de quem acredita | ✅ *Convencer* | 🟡 *Esquecer*: a dor, para ele conseguir andar | ✅ *Ler* + mente | ✅ *Confundir* | ✅ *Sugerir*: o guarda abre | 🟡 *Ilusão*: o grupo está em outro lugar |
| **Eternyr** | ✅ *Saltar* ou *Teleportar* | 🟡 *Futuro*: mostrar a consequência | 🟡 *Voltar* + instante: desfazer o golpe | ✅ *Passado*: ver o que aconteceu | ✅ *Parar* | ✅ *Atravessar* | ✅ *Lento* + área |
| **Primitivum** | ✅ *Asa* ou Forma Animal | 🟡 *Farejar*: sentir o que ele teme | 🟡 *Semente*: a erva certa | 🟡 *Farejar*: o cheiro do medo | ✅ *Enraizar* | ✅ *Cavar* | ✅ *Árvore* + barreira |

**O que isso mostra**:
- **Nenhum Valor fica sem resposta** para mais de um tipo de problema. Todo Valor tem 3 a 5 soluções diretas e resolve o resto de forma criativa. O único buraco (Cognitium atravessar um abismo) se resolve com **uma Palavra Básica**: é para isso que ela existe na magia, dar ao Valor ações que ele sozinho não tem. Além dela, há a party, a Dupla e as 4 palavras extras.
- **As soluções criativas são as mais interessantes** (*Cicatriz* para estabilizar, *Sal* para achar o mentiroso, *Esvaziar* para parar uma briga). Elas dependem justamente da **relação do personagem com a palavra**, que é o que a regra da Relação (+1) recompensa. A mecânica e a filosofia apontam para o mesmo lugar.
- **Cada Valor resolve os mesmos problemas de um jeito diferente** (parar uma briga desarmando, afundando na lama, esvaziando a vontade, confundindo, parando o tempo): a party inteira pode enfrentar a mesma cena sem ninguém repetir o outro.
- **Cura (decidido, P-115)**: **só Morbidyum devolve PV**. As soluções da coluna "estabilizar" dos outros Valores (*Cicatriz*, *Pele*, *Voltar*, *Semente*) **aliviam**: tiram uma condição ou mantêm o aliado de pé por um momento, sem recuperar PV.

## 5. Duração dos combates e melhorias (P-113)

Tempo estimado com o ritmo da sua mesa: uns **2–3 minutos por ação de jogador** (2 min conversando, 6 planejando uma ação do grupo, 8 fazendo planos, variando) e 1,5 minuto cada vez que o mestre emoldura. 4 jogadores:

| Moldura | escaramuça (2 × jog., moral) | combate (3 × jog. + 2, moral) | combate difícil (+2 melhorias em todos) | batalha (sem moral) | chefe |
|---|---|---|---|---|---|
| 1 | 17 min · 100% · 0 caídos | 38 min · 100% · 0,3 | 38 min · 98% · 0,6 | 51 min · 99% · 0,4 | 51 min · 98% · 0,6 |
| 2 | 19 min · 100% · 0 | 43 min · 98% · 0,7 | 43 min · 96% · 1,0 | 58 min · 98% · 0,9 | — |
| 3 | 20 min · 100% · 0 | 48 min · 100% · 0,3 | 48 min · 98% · 0,7 | 65 min · 99% · 0,5 | — |
| 4 | 23 min · 100% · 0 | 59 min · 99% · 0,6 | 59 min · 94% · 1,1 | 81 min · 97% · 0,9 | 82 min · 91% · 1,3 |

- **A moral (o resto foge quando metade cai) corta um quarto do tempo** e combina com o campo *Quer* dos adversários. Testei também "o resto foge quando o líder cai": deixa tudo curto, mas vira sempre "mate o líder" e todo combate acaba igual. Ficou só a metade.
- **Escaramuça** fica nos **15–25 minutos** que você quer para as lutas do dia a dia: quase nunca derruba alguém, mas gasta Arcano, Cansaço e Proteção, que é o que pesa depois.
- **Melhorias**: aplicar uma melhoria a **um** adversário quase não muda nada; aplicada a **todos**, 1 ponto de melhoria pesa como 1 ponto de adversário, deixa a luta mais tensa e **não a deixa mais longa**. Foi assim que entrou nas regras.
- **Vida parecida com o Daggerheart**: o Chefe agora tem PV +5 (10 a 13) em vez do dobro (que chegava a 16), e nenhuma melhoria passa de 12 PV. O chefe continua difícil (91–98%).

Aplicado em [[10 Adversários - como criar]]: tamanhos de combate (escaramuça, combate, batalha), moral, dificuldade fácil −2 / difícil +2 / muito difícil +4, e a tabela de melhorias.

## 6. Resumo das propostas

- **P-111** ✅ aplicada: régua nova de adversários (Dificuldade 12/13/14/15, ataque +1/+2/+3/+4, Limiares e dano levemente menores nas Molduras 2–4).
- **P-112** ✅ aplicado: orçamento: +2 pontos com 2 jogadores, +1 com 3.
- **P-113** ✅ aplicado: dificuldade de encontro: **difícil = +2 pontos**, **muito difícil = +4**, **fácil = −2**.
- **P-114** ✅ aplicada: Ameaça como **pote** visível que o mestre enche quando quiser e que dispara quando cheio.
- **P-115** ✅ decidido: cura é só de Morbidyum.
