# 01 — Regras centrais

> Rascunho v0 da Fase 1. Números conferidos em `calc/saida.md`. O que precisa da sua decisão está marcado **❓ P-xx** (ver [[09 Perguntas pra decidir]]).

## O jogo em um minuto

- Quando seu personagem tenta algo arriscado, você **descreve a ação com uma palavra de ação e um modo** ("roubar *furtivo*", "convencer *gentil*") e rola **2d12 + o atributo do modo** contra uma **Dificuldade**.
- **Cada dado par te dá 1 Arcano. Cada dado ímpar dá 1 Cataclisma ao mestre.** Par é controle e fechamento; ímpar é o incompleto, que muda (ver [[08 Mundo e entidades]]).
- **Dois dados iguais = Crítico.**
- Magia não usa atributo: você junta **palavras** do seu Valor, escolhe o **Grau** (o tamanho do feito) e rola um conjunto de dados que **sempre soma no máximo 24**: 2d12 → 3d8 → 4d6 → 6d4. Mais dados = mais confiável, mas mais Arcano *e* mais Cataclisma saindo da mesa.
- Adversários rolam **d20**. O mestre gasta **Cataclisma** para fazer o mundo reagir.
- Quando aperta, você escolhe: **Atalho** (entregar Cataclisma ao mestre e burlar agora, marcando Laço) ou **Passo de Valor** (fazer o difícil que aproxima do que importa, e apagar Laço).
- Não há turnos nem rodadas: o jogo anda em **Eventos** (uma situação completa: uma briga, uma conversa, uma fuga). Quem tem o **holofote** age; ele passa ao mestre quando alguém falha ou tira Ímpar. Efeitos duram por **marcadores** (até a próxima falha, até o fim do Evento...), nunca por turno.

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
| igual ao custo | **Conter**: anule um Abalo de até 4 pontos que afete só a sua ficha. Aliados podem ajudar a pagar |

## 4. Cataclisma (do mestre)

O mestre **começa a sessão com 2 por jogador**, máximo **20**.

**Transbordo**: se o mestre ganharia Cataclisma acima de 20, o excesso não some — ele dispara *na hora* um Abalo do tamanho do excesso (ver [[07 Mestre - Cataclisma, abalos e terrenos]]). O mestre não consegue "guardar tudo pro chefe"; o mundo vaza.

O mestre gasta Cataclisma para: pegar o holofote, ativar mais adversários, ativar habilidades de adversários e de lugares, trazer reforços, endurecer uma magia (+2 na Dificuldade por ponto, até +4), cancelar uma magia Vacilante, e disparar Abalos (que podem mexer até na ficha dos jogadores). Tabela completa no arquivo 07.

## 5. Atalho ou Valor

Quando a coisa aperta, o personagem tem dois caminhos. **Os dois são válidos.** Só existe **uma trilha** na ficha: o **Laço** (5 casas).

| | O que é | O que custa | O que dá |
|---|---|---|---|
| **Atalho** | fugir do desconforto do jeito que sempre funcionou (dar um jeitinho, mentir, sumir, explodir, empurrar para frente) | o mestre ganha **2 Cataclisma** (menor) ou **4** (maior); **marque 1 Laço** | menor: +3 numa rolagem já feita, ou ignorar uma condição · maior: a falha vira sucesso (o mestre diz o custo), ou sair da situação sem lidar com ela |
| **Passo de Valor** | fazer o difícil **porque** aproxima do que importa (o Compromisso, o que o personagem valoriza, as pessoas da party) | aceitar o desconforto: rolar com **Desvantagem**, **ou** marcar **1–2 Cansaço**, **ou** aceitar a consequência que o Atalho evitaria | **+1 Arcano** e **apague 1 Laço**, **dê certo ou não** (conta a direção, não o resultado) |

Só o mestre gasta Cataclisma. No Atalho o jogador não gasta nada: ele **entrega** Cataclisma ao mestre, e o mestre usa esse Cataclisma, de preferência, no que foi evitado.

### O Laço

Com as **5 casas** marcadas, o personagem está **no Laço**: a situação desconfortável de sempre passa a se repetir, com outras caras. Enquanto isso, **Atalhos rendem metade** (+1 em vez de +3; a falha vira sucesso parcial).

**O Laço é montado junto**, não só pelo mestre:
- o **jogador** diz como o Laço é por dentro: que sensação volta, do que o Atalho está protegendo o personagem, que situação ele mais evita;
- o **mestre** traz a situação para a mesa, com outra cara a cada vez;
- os **outros jogadores** podem sugerir caras para o Laço ("e se for o meu personagem pedindo ajuda?") e interpretar os PNJs dele;
- o **jogador** diz quando sente que o personagem encarou de verdade.

Um **Passo de Valor dentro da situação do Laço** quebra o Laço: apague todas as casas e ganhe uma **Marca de Valor** (+1 Arcano máximo, uma Vivência nova, ou reescrever uma resposta de criação: o personagem mudou).

### Olhar para os Laços (recompensas)

O jogo recompensa o jogador por **acompanhar** o próprio personagem, não só por agir:

- **Na hora**: quando o jogador **nomeia o Atalho** que o personagem está pegando e **do que ele protege** ("ele faz piada porque não aguenta ver o irmão ferido"), ganhe **+1 Arcano**. Uma vez por Evento.
- **No fim da sessão**, cada jogador responde as três perguntas. Cada "sim", com uma frase, vale **+1 Arcano** ou **apagar 1 Laço** (o jogador escolhe):
  1. **Você percebeu um Atalho do seu personagem**, e qual é a relação dele com esse Atalho? (Por que ele pega tanto esse?)
  2. **Uma relação se formou ou mudou** na party, ou com alguém do mundo?
  3. **Você notou o que outro personagem sentiu ou fez** (um Atalho, um Passo) e **participou** disso: falou, ajudou, esteve junto?

### A ideia por trás

O desconforto não some quando é evitado: **muda de lugar**. No Atalho, o mundo carrega por você (Cataclisma) e cobra depois. No Passo de Valor, você carrega agora e muda com isso. Atalho às vezes é esperto; **só** Atalho vira laço. Por isso quem usa magia se chama **Comprometido**.

## 6. Eventos e holofote

Não há iniciativa, turno nem rodada. A unidade do jogo é o **Evento**: uma situação completa, com começo e fim na ficção (um combate, um interrogatório, uma negociação, uma perseguição, um ritual). O mestre anuncia quando um Evento começa e o que o encerra ("acaba quando vocês saírem da ponte, ou quando a ponte cair").

Dentro do Evento, o ritmo é o holofote:

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
| **O Evento** | a situação terminar |
| **Até o Pouso** | a próxima noite de descanso |

**Ameaças**: para coisas que se aproximam (o teto caindo, o ritual terminando, reforços chegando), o mestre **anuncia a ameaça e o preço dela em Cataclisma**: "o teto cai quando eu pagar 6". Ela acontece quando ele pagar. Como o Cataclisma vem dos dados ímpares da mesa, todo mundo vê a ameaça chegando conforme a reserva do mestre enche. Os jogadores podem resolver a ameaça pela ficção, ou adiá-la uma vez gastando Arcano igual à metade do preço (o preço sobe 2).

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

- **Apagar**: você cai inconsciente e volta depois do Evento com 1 PV. Role 1d12; se tirar **até o seu nível**, ganha uma **Marca do Cataclisma** (risque 1 casa de Arcano máximo, e diga que marca ficou no seu corpo).
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
