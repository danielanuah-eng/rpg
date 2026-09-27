# 01 — Regras centrais

> Rascunho v0 da Fase 1. Números conferidos em `calc/saida.md`. O que precisa da sua decisão está marcado **❓ P-xx** (ver [[09 Perguntas pra decidir]]).

## O jogo em um minuto

- Quando seu personagem tenta algo arriscado, você **descreve a ação com uma palavra de ação e um modo** ("roubar *furtivo*", "convencer *gentil*") e rola **2d12 + o atributo do modo** contra uma **Dificuldade**.
- **Cada dado par te dá 1 Arcano. Cada dado ímpar dá 1 Cataclisma ao mestre.** Par é controle e fechamento; ímpar é o incompleto, que muda (ver [[08 Mundo e entidades]]).
- **Dois dados iguais = Crítico.**
- Magia não usa atributo: você junta **palavras** do seu Valor, escolhe o **Grau** (o tamanho do feito) e rola um conjunto de dados que **sempre soma no máximo 24**: 2d12 → 3d8 → 4d6 → 6d4. Mais dados = mais confiável, mas mais Arcano *e* mais Cataclisma saindo da mesa.
- Adversários rolam **d20**. O mestre gasta **Cataclisma** para fazer o mundo reagir.
- Quando aperta, você escolhe: **Atalho** (aceitar Cataclisma e burlar agora, alimentando o seu Laço) ou **Passo de Valor** (fazer o difícil que aproxima do que importa, e crescer com isso).
- Não há turnos nem rodadas: o jogo anda em **Cenários** (uma situação completa: uma briga, uma conversa, uma fuga). Quem tem o **holofote** age; ele passa ao mestre quando alguém falha ou tira Ímpar. Efeitos duram por **marcadores** (até a próxima falha, até o fim do Cenário...), nunca por turno.

---

## 1. Atributos

Seis, usados só em testes e ataques com arma. **Não entram em magia** (exceto quando uma regra disser).

| Atributo | Para quê |
|---|---|
| **Agilidade** | esquivar ativamente, correr, acrobacia, armas leves e de disparo |
| **Força** | levantar, empurrar, agarrar, armas pesadas |
| **Carisma** | convencer, liderar, mentir, intimidar |
| **Inteligência** | lembrar, deduzir, consertar, estudar magia alheia |
| **Sabedoria** | intuição, sobreviver, resistir à mente, acalmar |
| **Percepção** | notar, mirar à distância, rastrear, ouvir |

Distribuição inicial: **+2, +1, +1, 0, 0, −1**. Máximo **+5**.

## 2. O teste: 2d12 + atributo

### Descrever a ação: palavra de ação + modo

Todo teste começa com o jogador dizendo **o que** faz e **como**, usando as **Palavras Básicas** ([[02 Magia]]): um **verbo de ação** e um **modo** (adjetivo). O **modo escolhe o atributo**.

- "Vou **roubar**, **furtivo**" → Agilidade.
- "Vou **convencer**, **gentil**" → Carisma.
- "Vou **empurrar** a porta, **bruto**" → Força.
- "Vou **procurar**, **metódico**" → Inteligência.

A palavra precisa fazer **sentido concreto** na ficção: o mestre pode recusar um modo que não combina com o que foi descrito ("roubar *imponente*" no meio da feira não é furtar, é assaltar — então é outro teste). Palavras de ação não custam nada. Se o jogador descreve sem as palavras, o mestre ajuda a achar as duas.

Uma magia de **Grau 1** pode ser o **meio** da ação ("atacar, preciso, com uma agulha de gelo"; "esconder, furtivo, numa sombra que eu puxo"): você rola o teste normal da ação e a magia é o seu instrumento (num ataque, dano **1d8 + Moldura**). Sozinha, sem virar teste, a magia de Grau 1 só acontece, sem rolar.

### Rolar

Role 2d12, some o atributo (e bônus), compare com a Dificuldade. Igual ou maior = sucesso.

| Dificuldade | Nome | chance com +2 |
|---|---|---|
| 10 | Fácil | 85% |
| 12 | Normal | 75% |
| 15 | Difícil | 54% |
| 20 | Muito difícil | 19% |
| 25 | Lendário | 2% (precisa de ajuda) |

**Vantagem**: some +1d6. **Desvantagem**: subtraia 1d6. Não acumulam; uma anula a outra. O d6 extra **não conta** para par/ímpar.

### Leitura dos dados (paridade)

Olhe os dois d12:

| Os dois dados | Nome | Efeito |
|---|---|---|
| iguais (qualquer número) | **Crítico** | sucesso automático + efeito crítico; ganha Arcano igual ao número do dado (**máx. 6**) — ❓ P-03 |
| os dois pares | **Par** | +2 Arcano para você |
| um par, um ímpar | **Equilíbrio** | +1 Arcano para você, +1 Cataclisma para o mestre |
| os dois ímpares | **Ímpar** | +2 Cataclisma para o mestre; **o holofote passa ao mestre** |

Isso vale igual para sucesso e falha. Um sucesso Ímpar é "conseguiu, mas o mundo cobrou"; uma falha Par é "não deu, mas você aprendeu algo" (o Arcano).

Chance de cada leitura no 2d12: Crítico 8%, Par 21%, Equilíbrio 50%, Ímpar 21%.

**Efeitos de crítico** (escolha um): ignorar um efeito negativo de magia; o melhor resultado possível do teste; dobrar o dano.

**Rolagens de reação** (resistir a algo que o mestre fez com você) usam 2d12 + atributo mas **não geram** Arcano nem Cataclisma. Isso segura a economia.

❓ **P-01**: você escreveu "d20 pra testes básicos" e "2d12 pra tudo". Eu fiz: **jogador sempre 2d12, mestre sempre d20**. Confirma?

## 3. Arcano (dos jogadores)

Cada jogador tem sua reserva. **Começa com 3, máximo 12**, e passa de uma sessão para outra. Se você ganharia Arcano acima de 12, pode dar o excesso a um aliado que te veja.

Por rolagem de teste o jogador ganha em média **1 Arcano** (2 em magia de Grau 3, 3 em Grau 5). É bastante, de propósito: Arcano é para ser gasto o tempo todo.

| Custo | Gasto |
|---|---|
| 1 | **Vivência**: +2 numa rolagem em que uma Vivência sua se aplica (uma por rolagem) |
| 1 | **Ajudar**: dê Vantagem a um aliado (descreva como) |
| 1 | **Palavra extra** numa magia (cada palavra de Valor além da primeira; Palavras Básicas não custam) |
| 1–3 | **Ampliar** uma magia um passo (1 no Grau 1–2, 2 no Grau 3–4, 3 no Grau 5) |
| 2 | **Suavizar**: role de novo o d20 de uma Dificuldade de magia e fique com o melhor |
| 3 | **Hábito**: o talento especial do seu Comportamento |
| 3 | **Ação em Dupla**: você e um aliado rolam, os dois usam o maior resultado, e os efeitos se somam |
| 3 | **Detalhe**: acrescente algo verdadeiro à cena (uma corda, um conhecido, uma saída) — o mestre ajusta |
| 3 | **Forçar o Grau**: conjure 1 Grau acima do seu máximo (e marque Cansaço igual ao Grau) |
| igual ao custo | **Conter**: anule um Evento de Cataclisma de até 4 pontos que afete só a sua ficha. Aliados podem ajudar a pagar |

## 4. Cataclisma (do mestre)

O mestre **começa a sessão com 2 por jogador**, máximo **20**.

**Transbordo**: se o mestre ganharia Cataclisma acima de 20, o excesso não some — ele dispara *na hora* um Evento de Cataclisma do tamanho do excesso (ver [[07 Mestre - Cataclisma, eventos e terrenos]]). O mestre não consegue "guardar tudo pro chefe"; o mundo vaza.

O mestre gasta Cataclisma para: pegar o holofote, ativar mais adversários, ativar habilidades de adversários e de lugares, trazer reforços, endurecer uma magia (+2 na Dificuldade por ponto, até +4), cancelar uma magia Vacilante, e disparar Eventos (que podem mexer até na ficha dos jogadores). Tabela completa no arquivo 07.

## 5. Atalho ou Valor

Toda vez que a coisa aperta, o personagem tem dois caminhos. **Os dois são válidos.** O jogo só mostra o que cada um custa com o tempo.

### Atalho: aceitar Cataclisma para burlar

O personagem foge do desconforto do jeito que sempre funcionou: dá um jeitinho, mente, some, explode, empurra o problema para frente. Na mesa, o jogador **aceita Cataclisma** para conseguir uma vantagem agora:

| Atalho | O mestre ganha | Você ganha |
|---|---|---|
| **Menor** | 2 Cataclisma | +3 numa rolagem (depois de ver os dados), **ou** ignorar uma condição ou Desvantagem |
| **Maior** | 4 Cataclisma | uma falha vira sucesso (o mestre escolhe o custo depois), **ou** sair de uma situação sem ter que lidar com ela |

- Funciona sempre, mesmo com 0 Arcano. É o que o personagem fez a vida inteira, e deu certo muitas vezes.
- Cada Atalho **marca 1 casa de Laço** na ficha (5 casas).
- O segundo Atalho no mesmo Cenário custa **+1 Cataclisma**, o terceiro +2, e assim por diante: quanto mais se foge, mais caro fica.
- O Cataclisma de um Atalho é **do personagem**: o mestre gasta ele, sempre que puder, em coisas ligadas ao que foi evitado. O problema empurrado para frente volta, e maior.

### O Laço

Quando as 5 casas de Laço enchem, o personagem entra no **Laço**: a situação desconfortável de sempre começa a se repetir. É uma **Corrente pessoal** ([[07 Mestre - Cataclisma, eventos e terrenos]]) que o mestre monta com as respostas de criação dele: a fraqueza, a dor, o jeito como ele lida com desconforto, o que ele evita.

Enquanto estiver no Laço:
- o Laço aparece **pelo menos uma vez por sessão**, com outra cara (outra pessoa, outro lugar, a mesma sensação);
- Atalhos custam o **dobro** de Cataclisma e dão **metade** (+1 em vez de +3; a falha vira sucesso parcial);
- no Pouso, você só escolhe **um** descanso em vez de dois.

O Laço não acaba sozinho, nem com descanso. Ele acaba quando o personagem **encara a situação do Laço com um Passo de Valor** (abaixo).

### Passo de Valor: fazer o difícil que aproxima do que importa

Agir envolve desconforto. Fazer o que é difícil coloca o personagem perto das coisas que importam para ele: o **Compromisso** do seu Valor, o que ele disse que é valioso manter, as pessoas da party.

O jogador declara um **Passo de Valor** quando o personagem escolhe o caminho difícil **porque** ele leva para perto do que importa, e **aceita o desconforto** em vez de burlar. Escolha um:
- rolar com **Desvantagem** (o jeito difícil, sem o atalho);
- marcar **1 ou 2 Cansaço**;
- aceitar uma consequência que o Atalho evitaria (ser visto, perder algo, falar a verdade, ficar).

E ganhe, **dê certo ou não**:
- **+1 Arcano** agora;
- **1 casa de Passo** (3 casas);
- se for uma magia: a relação com a palavra conta como forte (sem a Dificuldade maior de relação fraca).

O resultado da rolagem não importa para o Passo: ele conta pela **direção**, não pelo sucesso.

**3 Passos** → apague **2 casas de Laço** e zere os Passos. Uma vez por sessão, no máximo, o mestre ou a mesa podem dizer "isso foi um Passo" mesmo sem o jogador declarar.

**Encarar o Laço**: quando você dá um Passo de Valor **dentro da situação do Laço**, o Laço se quebra. Apague todas as casas de Laço e ganhe uma **Marca de Valor**: escolha uma — +1 Arcano máximo, uma Vivência nova sobre o que aprendeu, ou reescrever uma resposta de criação (o personagem mudou). A situação daquele Laço não volta do mesmo jeito.

### A ideia por trás

O desconforto não some quando é evitado: **muda de lugar**. No Atalho, o mundo carrega o desconforto por você (Cataclisma) e cobra depois. No Passo de Valor, você carrega agora e muda com isso. Usar Atalho às vezes é esperto; usar **só** Atalho vira um laço de situações e sensações desconfortáveis. Por isso quem usa magia se chama **Comprometido**: o poder vem do compromisso com um Valor, não da fuga.

## 6. Cenários e holofote

Não há iniciativa, turno nem rodada. A unidade do jogo é o **Cenário**: uma situação completa, com começo e fim na ficção (um combate, um interrogatório, uma negociação, uma perseguição, um ritual). O mestre anuncia quando um Cenário começa e o que o encerra ("acaba quando vocês saírem da ponte, ou quando a ponte cair").

Dentro do Cenário, o ritmo é o holofote:

- Os jogadores decidem entre si quem age. Quem age faz **uma ação** (atacar, conjurar, ajudar, uma manobra) e pode se mover até **Perto** junto com ela.
- **O holofote passa ao mestre** quando um jogador **falha** ou tira **Ímpar** (dois ímpares). Na vez dele, o mestre ativa **1 adversário de graça** e **mais 1 por Cataclisma** gasto; depois devolve aos jogadores.
- O mestre pode **tomar o holofote** a qualquer momento gastando 1 Cataclisma (ex.: logo depois de uma magia gigante).
- Regra de mesa: ninguém age duas vezes seguidas se outro jogador ainda não agiu desde a sua última ação, a não ser que todos concordem.

### Marcadores (no lugar de turnos)

Nada dura "X turnos". Efeitos duram até um **marcador** acontecer, e os marcadores estão amarrados aos erros e à economia, que é onde o jogo se equilibra:

| Marcador | Termina quando... |
|---|---|
| **Instante** | acontece e acaba |
| **Até a próxima Falha** | qualquer jogador falhar uma rolagem |
| **Até o holofote virar** | o holofote passar ao mestre |
| **Até o mestre pagar** | o mestre gastar Cataclisma igual ao Grau para encerrar (ele pode, a qualquer momento) |
| **O Cenário** | a situação terminar |
| **Até o Pouso** | a próxima noite de descanso |

**Contadores**: para coisas que se aproximam (o teto caindo, o ritual terminando, reforços chegando), o mestre põe um contador (ex.: 4). Ele desce 1 **a cada Falha dos jogadores** e **a cada vez que o mestre gasta 3+ Cataclisma de uma vez**. Quem erra acelera o perigo; quem acerta ganha tempo.

## 7. Alcance

| Faixa | Na mesa | Na ficção |
|---|---|---|
| **Toque** | encostado | ao alcance da mão ou da arma |
| **Perto** | até 1 passo curto | do outro lado de uma sala pequena |
| **Médio** | até 3 passos | um salão, um pátio |
| **Longe** | até 6 passos | do outro lado da praça, tiro de arco |
| **Horizonte** | o que se vê | outro morro, o céu |

Mover até Perto vem junto com a ação. Ir mais longe **é** a ação (ou um teste de Agilidade para ir mais rápido).

## 8. Esquiva, dano e Vida

**Esquiva** vem do Comportamento. Adversário acerta se o **d20 + ataque ≥ Esquiva**.

**Limiares** (como no Daggerheart): cada personagem tem **Maior** e **Grave**.
- Dano **abaixo do Maior** → marque **1 PV**
- **Maior ou mais** → marque **2 PV**
- **Grave ou mais** → marque **3 PV**

Limiares = base do Comportamento + nível + armadura.

**Proteção**: casas da armadura. Marque 1 casa para reduzir em 1 os PV marcados de um golpe. Restaura em Pausa.

**Magia causa dano físico** normal (passa pelos Limiares). Efeitos de magia que não são dano (medo, paralisia, cegueira) são resistidos com **2d12 + atributo** contra a Dificuldade do efeito.

**Vida (PV)**: 5–7 casas conforme o Comportamento, máximo 12.

## 9. Cansaço

**6 casas** no início, máximo 12. Cansaço é o preço do corpo e da mente; Arcano é o preço do mundo.

**Como ganha**: em Passos de Valor, gastando para Forçar magia, algumas Dificuldades de magia, reações de Dominum, talentos de Comportamento, alguns ataques de adversários.

**Com o que gasta (marcando casas)**:
- **Forçar** uma magia: marque até (Grau) casas, **+2 no total por casa**.
- **Ampliar** uma magia: pague a ampliação em Cansaço em vez de Arcano.
- Talentos que pedem Cansaço.

**Exausto** (todas as casas marcadas): você fica **Vulnerável** (ataques contra você têm Vantagem), não pode marcar Cansaço de propósito, e cada Cansaço que você teria de marcar vira **1 PV**.

## 10. Descansos

**Pausa** (uma hora): escolha **dois**: recuperar 1d4 + Moldura PV; limpar 1d4 + Moldura Cansaço; restaurar toda a Proteção; **Meditar** (+1 Arcano). O mestre ganha **1d4 Cataclisma**.

**Pouso** (uma noite): escolha **dois**, cada um completo: todos os PV; todo o Cansaço; toda a Proteção; trocar uma Palavra Estrangeira; trabalhar num projeto. O mestre ganha **1d4 + nº de jogadores** de Cataclisma.

## 11. Quando os PV acabam

Ao marcar o último PV, escolha:

- **Apagar**: você cai inconsciente e volta depois do Cenário com 1 PV. Role 1d12; se tirar **até o seu nível**, ganha uma **Marca do Cataclisma** (risque 1 casa de Arcano máximo, e diga que marca ficou no seu corpo).
- **Última Palavra**: conjure uma magia 1 Grau acima do seu máximo, sem pagar Arcano e sem rolar Dificuldades. Depois, seu personagem morre.
- **Jogar os dados**: role 2d12. **Crítico ou Par**: levanta com PV e Cansaço limpos. **Equilíbrio**: levanta com 1 PV e uma Marca do Cataclisma. **Ímpar**: morre.

(Morbidyum tem regra própria, ver [[03 Valores]].)

## 12. Vivências

Frases curtas do passado do personagem ("Cresci num porto", "Fui soldado do Rei-Semideus", "Sei mentir para guardas"). Começa com **2**, cada uma vale **+2** quando você gasta **1 Arcano** numa rolagem em que ela faz sentido. Ganha mais nos níveis 2, 5 e 8.

## 13. Moldura

Quase tudo que cresce com o nível passa pela **Moldura**:

| Nível | Moldura | Grau máximo | Palavras de Valor por magia | Sintonia | Proficiência |
|---|---|---|---|---|---|
| 1 | 1 | 2 | 2 | +2 | 1 |
| 2–4 | 2 | 3 | 3 | +3 | 2 |
| 5–7 | 3 | 4 | 4 | +4 | 3 |
| 8–10 | 4 | 5 | 5 | +5 | 4 |

Regra fácil de lembrar: **Grau máximo, Palavras e Sintonia = Moldura + 1**. (Se a fraqueza do seu Canal acontecer, a Sintonia cai para a Moldura.)

**Sintonia** é o bônus que se soma aos dados de magia (e ao dano de magia). Atributo não entra em magia.
