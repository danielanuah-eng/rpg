# 14 — Teste das mecânicas

> Cada mecânica testada em três perguntas: **quantas etapas** ela pede na mesa, se é **fácil de aplicar**, e se dá **vantagem demais quando repetida**. Números de `calc/simular_economia.py`, `calc/simular_combate.py` e contas diretas nos dados. **Decidido depois do teste**: P-104 (Grau 2 custa 1), P-105 (duas ajudas no máximo), P-106 (uma Dificuldade só), P-107 (Dupla numa rolagem), P-110 (caixinhas de Atalho) aplicados; P-108 virou o redesenho do Laço apertado (Dificuldade +2 fora do padrão, relação estremecida, consequência que cresce; o Passo solta). P-109: sem limite — todo Passo dá +1 Arcano.

## 1. Quanto cada ajuda vale

Chance de sucesso de um personagem com +2 no atributo:

| rolagem | Dif 12 | Dif 15 |
|---|---|---|
| sem nada | 78% | 58% |
| + Relação (+1) | 83% | 65% |
| + Vivência (+2, 1 Arcano) | 88% | 72% |
| + Atalho menor (+3 depois de ver os dados; 2 Cataclisma para o mestre) | 92% | 78% |
| + Ajudar (+1d6, 1 Arcano do aliado) | 92% | 80% |
| + Forçar 2 casas (+4, 2 Cansaço) | 94% | 83% |
| + Vivência e Ajudar juntos | 97% | 89% |

**O que isso mostra**: cada ajuda sozinha é justa (+5 a +20 pontos de chance). O problema é **empilhar**: nada impede Relação + Vivência + Ajudar + Atalho + Forçar na mesma rolagem (+1 +2 +1d6 +3 +4), e aí qualquer Dificuldade vira formalidade. Além de tirar a tensão, empilhar é o que mais cria etapas numa rolagem.

## 2. Economia de Arcano (P-101)

Um jogador ao longo de uma sessão (3 Eventos × 8 rolagens), gastando de um jeito razoável: Vivência quando ajuda, palavra extra em metade das magias, Hábito uma vez por Evento, ajudar de vez em quando. **Ainda sem contar** o Arcano das perguntas de fim de sessão e de nomear Atalhos (até +4 a mais por sessão).

| regra | Arcano ganho por rolagem | rolagens com Vivência | Hábitos por sessão | magias com palavra extra | Arcano perdido no máximo | Arcano no fim |
|---|---|---|---|---|---|---|
| **A · atual** (+1 por dado par; Vivência 1, Hábito 3, Grau 2 grátis) | 1,32 | 52% | 2,5 | 47% | 1,2 | 7,3 |
| B · custos maiores (Vivência 2, Hábito 4) | 1,31 | 35% | 1,7 | 47% | 0,2 | 5,1 |
| C · só o Par (dois pares) dá Arcano | 0,38 | 25% | 0,3 | 24% | 0 | 2,7 |
| D · +1 se tiver algum dado par | 0,88 | 46% | 1,4 | 45% | 0 | 3,8 |
| E · +1 por par, mas sucesso sem Par não dá | 0,94 | 40% | 1,9 | 37% | 0,5 | 5,1 |
| **H · Grau 2 custa 1** (resto igual ao atual) | 1,31 | 40% | 1,8 | 28% | 0,4 | 5,2 |
| J · Grau 2 custa 1, Vivência 2, Hábito 4 | 1,31 | 25% | 1,2 | 28% | 0,1 | 4,4 |

**Leitura**: com a regra atual, cada rolagem dá mais Arcano (1,3) do que o jogador consegue gastar, e ele termina a sessão com mais do dobro do que começou. Mexer no ganho (C, D, E) resolve, mas quebra a regra que é a cara do jogo ("cada dado par é 1 Arcano"). **A chave é o custo, não o ganho**: fazendo a **magia de Grau 2 custar 1 Arcano** (H), a magia passa a ser "paga pelos próprios dados" (um 2d12 devolve, em média, 1 Arcano), o jogador termina a sessão com ~5, quase nunca bate no máximo, e ainda usa Vivência em 40% das rolagens e o Hábito quase duas vezes por sessão. Arcano continua fácil e gostoso de ganhar; só não sobra.

Com H, a tabela de Graus ficaria: **Grau 1: 0 · Grau 2: 1 · Grau 3: 2 · Grau 4: 3 · Grau 5: 5**. Como os dados devolvem em média 1 / 1,5 / 2 / 3 Arcano, magias grandes passam a drenar de verdade (Grau 5: −2 por conjuração), que é o "gasto incrível" que você queria.

## 3. Auditoria, mecânica por mecânica

**Etapas** = o que a mesa faz na hora (decidir, olhar tabela, rolar, anotar). Até 4 é leve; 5–7 é médio; 8+ é pesado.

| Mecânica | Etapas | Aplicação | Vantagem repetível? | Veredito |
|---|---|---|---|---|
| Teste: verbo + modo → atributo → rolar → ler par/ímpar | 4 | leve; a leitura de par/ímpar vira hábito em 2–3 rolagens | — | ✅ |
| Magia de Grau 1 sozinha | 1 | leve | não (só narrativa) | ✅ |
| Grau 1 no meio de uma ação (dano 1d8 + Moldura) | 4 | leve, igual a um teste | todos têm uma "arma" que não sai da mão, mais fraca que arma de verdade | ✅ |
| Magia de Grau 2 | 7 (intenção, palavras, Relação, Grau, pagar, rolar contra **a maior** entre Grau e alvo, dano) | média; a comparação "a maior entre duas Dificuldades" é a etapa que mais trava | não | 🟡 simplificar (P-106) |
| Grau 3+ (Forçar o Grau, Moldura 2+) | 9–10 (mais Dificuldades no d20, Crítico/Desequilíbrio) | pesada, mas rara e é o momento grande | não | ✅ aceitável |
| Relação (+1) | 1 frase | leve; pode alongar a conversa se virar obrigação | é o normal | ✅ |
| Ampliar (custo muda com o Grau) | 2 | leve | limitada pela Moldura | ✅ |
| Forçar (Cansaço, +2 por casa, depois de rolar) | 2 | leve | limitada pelo Cansaço e pelo Grau | ✅ |
| Canal (foco, arma, Transcrito...) | 0–1 | passivo | não | ✅ |
| Vivência (1 Arcano, +2) | 2 | leve | **sim**: com a economia atual, em metade das rolagens | 🟡 resolvido pela economia (P-104) e pelo teto (P-105) |
| Ajudar (1 Arcano, +1d6) | 2 | leve | forte e barata; não acumula com outra Vantagem | 🟡 teto (P-105) |
| **Magia em Dupla** | 10+ (3 Arcano, palavras dos dois, Grau +1, **duas rolagens**, somar, somar Dificuldades, duas Skills, dois Drawbacks, Crítico/Desequilíbrio de cada metade, regras de opostos) | **pesada**: é a mecânica mais longa do jogo | não (3 Arcano) | 🔴 simplificar (P-107) |
| Ação em Dupla (sem magia) | 4 | leve | não | ✅ |
| **Atalho** menor | 2 | leve | **sim, e forte**: +3 depois de ver os dados, sem custo para o jogador; depois que o Laço aperta, nada mais freia dentro da sessão | 🔴 freio (P-108) |
| Passo de Valor | 3 | leve | dá para "farmar" Arcano declarando Passo em tudo (paga com Desvantagem, que custa ~20 pontos de chance) | 🟡 limite (P-109) |
| Laço apertado ("dois Atalhos sem Passo na sessão") | memória | média: alguém precisa lembrar quantos Atalhos houve | — | 🟡 ficha (P-110) |
| Olhar para os Laços (+1 por Evento; 3 perguntas no fim) | 1 / 3 | leve | soma até ~4 Arcano por sessão, o que piora a sobra atual | ✅ com a economia H |
| Fusão | 3 (circular; o mestre força com 1 Cataclisma; Desvantagem de perspectiva) | média, mas quem conduz é o mestre | não | ✅ |
| Crítico / Desequilíbrio / Magia Ímpar / Dificuldades (d20) | 1–3 | leve na Moldura 1 (só o Crítico aparece); média da Moldura 2 em diante | não | ✅ |
| Dano 1/2/3 e Proteção | 2 | leve | não | ✅ |
| Lacaios 1/2/3 (P-102, aplicado) | 1 | leve | — | ✅ (o combate simulado continua igual: 96% de vitória, 22 ações) |
| Cansaço e Exausto | 1 | leve | — | ✅ |
| Mestre: Abalos, Marés, Correntes, Ameaças | 1–3 cada | médio, mas é ferramenta do mestre, usada quando ele quer | — | ✅ |
| Adversário por palavras (determinador de 6 partes) | leitura | média para ler na hora; o gerador resolve | — | ✅ |

## 4. O que ajustar (propostas)

1. **P-104 · Economia**: magia de **Grau 2 custa 1 Arcano** (Grau 1 grátis; custos 0/1/2/3/5). Mantém "cada dado par = 1 Arcano", faz a magia se pagar com os próprios dados e para a sobra.
2. **P-105 · Teto de ajudas**: **no máximo duas ajudas por rolagem** (Vivência, Ajudar, Atalho, Forçar, talentos). A Relação não conta, porque é o normal. Tira o empilhamento e corta etapas.
3. **P-106 · Uma Dificuldade só**: magia **contra um alvo** usa **só a Dificuldade do alvo**; a Dificuldade do Grau vale só para efeitos **sem alvo**. As duas já são quase iguais em toda Moldura (12/14/16/19 contra 12/14/16/18), então o "a maior entre as duas" só atrasa a mesa (e foi o que escondeu a Skill de Persona).
4. **P-107 · Dupla em uma rolagem**: quem conduz rola **os dados do Grau do efeito** (o maior Grau dos dois + 1) + Sintonia + Relação; o parceiro soma **+2**; contra a Dificuldade desse Grau. As duas Skills valem. Opostos: efeito +2 Graus, Dificuldade +2, os dois marcam Cansaço. Mesma chance de hoje (89–90% em qualquer Moldura; opostos 57–70%), com metade das etapas.
5. **P-108 · Freio do Atalho**: com o Laço **já apertado**, cada novo Atalho que toca nele faz a consequência **crescer na hora**, não só no fim da sessão. O Atalho continua válido e forte; só passa a cobrar de verdade quando vira o único caminho.
6. **P-109 · Passo de Valor**: ~~o Arcano do Passo vem uma vez por Evento~~ — decidido **sem limite**: o custo do Passo (desconforto, e o +2 com o Laço apertado) já segura.
7. **P-110 · Ficha**: duas caixinhas "Atalhos nesta sessão □□" do lado dos Laços, para ninguém precisar lembrar.

## 5. Aplicado junto com este teste

- **P-97**: quando uma palavra se funde fica no olho do mestre e da mesa, sem número.
- **P-98**: fundida por **uma sessão inteira**, vira Laço.
- **P-100**: o −2 de Persona vale depois de escolher qual Dificuldade vale.
- **P-101**: Drawback de Conflitüo agora é **"fora de combate ou violência, magia custa +2 Arcano"** (pesa desde a Moldura 1).
- **P-102**: Lacaios com Limiares pela metade e dano **1/2/3 como no Daggerheart** (cada golpe derruba 1, 2 ou 3).
