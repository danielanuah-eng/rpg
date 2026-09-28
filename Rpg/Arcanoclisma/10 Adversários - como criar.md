# 10 — Adversários: como criar

> Rascunho v0. Adversários **não têm habilidades escritas**: têm **palavras**, como os jogadores. Palavras que dizem quem ele é, como age e como parece, cada uma com um **determinador** curto que diz como ela funciona na mesa. O mestre improvisa dentro do determinador. Adversários não viram cartas; moram no gerador (com imagem) e no [[11 Bestiário]].

## A ficha

```
NOME — Moldura · Papel · Valor(es) · Maestria
Dificuldade · Limiares · PV · Cansaço · Ataque · Dano      ← da régua
SER:     1–2 substantivos        quem ele é
AGIR:    2–4 verbos              o que ele faz (com determinador)
PARECER: 1–3 adjetivos           como ele é (cada um é uma passiva)
QUER:    uma frase               o que ele quer, e quando foge
JEITO:   (Chefes) personalidade e modo de agir, em 2–3 frases
LAÇO:    (opcional) uma palavra  o que o prende ou o conecta (narrativo)
BÁSICAS: tudo = Ataque · 2–3 verbos fortes (+2) · 1 fraco (−2)   ← a tabela de Interações dele
VALOR:   palavras de Valor, se ele conjura (um humano comum não tem)
```

Exemplo:

```
LOBO-DE-CINZA — Moldura 1 · Ágil · Morbidyum
Dif 14 · Limiares 8/14 · PV 4 · Cansaço 2 · Ataque +2 · Dano 1d10+2
SER:     Lobo, Luto
AGIR:    Morder (ataque) · Uivar (2C · Longe · Grupo · Firmar · Amedrontado · até a próxima Falha)
         Sumir (1C · si · Instante: vira cinza e reaparece até Médio)
PARECER: Pálido (Vantagem para se esconder à noite) · Faminto (ataca primeiro quem está ferido)
QUER:    comer o que morreu; foge com 1 PV
BÁSICAS: tudo +2 · fortes: Rastrear, Esconder, Bater (+4) · fraco: Firmar (0)
VALOR:   Morbidyum: Assombrar, Cinzas, Pálido
```

## Os números: régua por Moldura

| Moldura | Dificuldade | Limiares | PV | Cansaço | Ataque (d20 +) | Dano |
|---|---|---|---|---|---|---|
| 1 | 12 | 8 / 14 | 5 | 2 | +1 | 1d10+2 |
| 2 | 13 | 11 / 21 | 6 | 3 | +2 | 2d10+2 |
| 3 | 14 | 16 / 30 | 7 | 3 | +3 | 3d10+3 |
| 4 | 15 | 22 / 42 | 8 | 4 | +4 | 4d10+4 |

A **Dificuldade** é o número que os jogadores tiram para acertar o adversário (arma ou magia) e para resistir ao que ele faz. Um golpe médio de jogador na Moldura certa marca 2 PV; um golpe médio do adversário marca 1–2 PV num jogador de armadura Média. A régua sobe devagar (Dificuldade +1 e ataque +1 por Moldura) porque os personagens também sobem devagar (atributo e Esquiva +1 por Moldura); conferido em [[15 Teste de escala e criatividade]].

**Cada adversário tem a sua tabela de Interações**, do jeito curto: **um número para tudo** (o Ataque) e, pelas palavras dele (*Ser*, *Parecer*), **2 ou 3 verbos fortes (+2)** e **1 fraco (−2)**. Ele rola **d20 + o número** do verbo. Quando um jogador age **contra um verbo forte** dele (se esconder de quem Observa bem), a Dificuldade sobe **+2**; contra um fraco, cai **−2**. Se ele usa magia, tem também **palavras de Valor**; um humano comum não tem. PNJ importante pode ter a tabela inteira, com atributos, como um jogador. Todos os 280 do [[11 Bestiário]] já têm a linha **Básicas**.

**A régua é o ponto de partida, não uma lei.** A Dificuldade de um adversário **varia** pelo que ele é: Valores, história, corpo, palavras. Um bicho frágil e distraído pode ter Dificuldade **abaixo** da régua; um guarda veterano de armadura, acima. Ajuste de −3 a +3 é normal; use o *Parecer* e o *Ser* como guia (*Lento*, *Cego*, *Enorme*: mais fácil de acertar; *Veloz*, *Blindado*, *Etéreo*: mais difícil).

**Contra magia**, o adversário se defende de dois jeitos, e os dois crescem com a Moldura dele:
- **Desviar**: magia do tipo **Projétil** rola contra a **Dificuldade** dele.
- **Resistir**: num **Teste mágico** (sono, encanto, medo, rede que prende) só ele rola: **d20 + Ataque** contra a **Dificuldade da magia** (12 + Sintonia + palavras de quem conjura). 20 natural sempre escapa; 1 natural é Crítico de quem conjurou; o d20 dele gera o Arcano (par) ou o Cataclisma (ímpar) daquela magia. Palavras dele que combinam dão Vantagem; as que o expõem, Desvantagem. **Cada ser rola o seu**; um **bando ou grupo** rola uma vez e, se falhar, perde 1 / 2 / 3 membros (falhou por 1–4 / 5–9 / 10+).
- Maestrias (*Resiliente*, *Anulador*) e o Valor do adversário mexem nas duas.

### Papéis

| Papel | Ajuste sobre a régua | Verbos | Pontos |
|---|---|---|---|
| **Lacaio** | Dif −2, dano = metade. Vem em grupo do tamanho da mesa. O grupo tem **Limiares pela metade** (M1 4/7 · M2 5/10 · M3 8/15 · M4 11/21) e o dano funciona **como no Daggerheart, 1/2/3**: um golpe abaixo do Maior derruba **1** Lacaio, no Maior derruba **2**, no Grave derruba **3** | 1–2 | 1 por grupo |
| **Social** | não luta | 2 (de conversa) | 1 |
| **Suporte** | PV −1 | 2–3, um de cuidar ou proteger | 1 |
| **Padrão** | a régua | 2–3 | 2 |
| **Ágil** | Dif +2, PV −1, Ataque +1, move até Médio | 2–3 | 2 |
| **Atirador** | PV −1, ataque até Longe, dano ×1,1 | 2–3 | 2 |
| **Conjurador** | Dif +1, PV −1, ataca com magia | 3–4 | 3 |
| **Líder** | Dif +1, PV +1; ao agir, ativa um aliado de graça | 3 | 3 |
| **Bruto** | Dif −1, Limiares ×1,3, PV +3, dano ×1,4 | 2–3 | 4 |
| **Chefe** | Dif +2, Limiares ×1,5, **PV +5** (10 a 13, como os solos do Daggerheart); age 2 vezes quando ativado; um verbo pode ser **reação**; tem **personalidade e modo de agir** descritos, que guiam o que ele faz | 4–5 | 5 |

**Vida**: como no Daggerheart, os PV dos adversários ficam em números pequenos: Lacaio cai com um golpe, Padrão 5–8, Bruto 8–11, Chefe 10–13. Nenhuma melhoria passa de **12 PV** (exceto o Chefe base da Moldura 4).

### Tamanho do combate

Nem toda luta precisa ser grande. Escolha o tamanho pelo que a cena pede. Tempo estimado numa mesa que conversa antes de agir (uns 2–3 minutos por ação de jogador, planos em grupo incluídos):

| Combate | Orçamento | Termina quando | Tempo | Risco |
|---|---|---|---|---|
| **Escaramuça** | **2 × jogadores** | metade dos adversários cai (moral) | **15–25 min** | baixo: custa recursos (Arcano, Cansaço, Proteção), raramente derruba alguém |
| **Combate** | **3 × jogadores + 2** | metade dos adversários cai (moral) | 40–60 min | médio: o grupo perde metade dos PV, às vezes alguém cai |
| **Batalha** | **3 × jogadores + 2**, ou com Chefe | até o fim (sem moral) | 50–90 min | alto: para os momentos grandes da história |

**+1 ponto com 3 jogadores, +2 com 2 jogadores** (mesa menor deixa o mesmo encontro mais fácil). −2 se houver 2 Chefes, +1 se usar adversários de Moldura abaixo.

**Moral**: quando **metade dos pontos de adversários** caiu, o resto **foge, se rende ou negocia** (o campo *Quer* de cada um diz como). A moral é a **linha de base** para todo adversário. **Chefes, feras encurraladas e Deidades** partem dela, mas o que fazem quando a luta vira sai da interpretação. **Chefes não têm regra de comportamento**: o que um Chefe faz quando a luta vira (lutar até o fim, fugir, barganhar, trair os próprios aliados, mudar de lado) sai da **descrição da personalidade e do modo de agir** dele, e o mestre interpreta. Um Chefe é um ser contextual e subjetivo, não uma tabela. A moral é o que deixa a maior parte das lutas curta, e abre espaço para conversa no meio do combate.

### Dificuldade do encontro

**Fácil −2 · Difícil +2 · Muito difícil +4 pontos.** Cada ponto pode virar **mais adversários** (como sempre) ou **uma melhoria para todos os adversários do encontro**:

| Melhoria (1 ponto) | Moldura 1–2 | Moldura 3–4 |
|---|---|---|
| **Mais certeiros** | +1 no ataque | +1 no ataque |
| **Mais fortes** | +2 de dano | +3 de dano |
| **Mais difíceis de acertar** | +1 de Dificuldade | +1 de Dificuldade |
| **Mais duros** | Limiares +2 / +4 | Limiares +3 / +6 |
| **Mais vida** | +1 PV (máx. 12) | +1 PV (máx. 12) |

A mesma melhoria não se repete no mesmo encontro. Nos testes, uma melhoria para todos pesa mais ou menos o mesmo que um ponto de adversário a mais, e deixa a luta mais tensa **sem deixá-la mais longa** (conferido em [[15 Teste de escala e criatividade]]).

## As palavras

### SER (substantivos)

Um ou dois nomes: **Lobo**, **Carrasco**, **Estátua**, **Mãe**, **Tempestade**. Não têm número. Servem para o mestre e os jogadores saberem o que esperar e para **justificar** o que ele faz: um *Carrasco* sabe prender; uma *Estátua* não sente dor nem medo. Se um jogador usa o Ser contra ele ("é uma Estátua: vou quebrar, *bruto*"), o mestre pode dar Vantagem.

### AGIR (verbos com determinador)

Cada verbo é algo que ele faz. Um verbo **sem determinador** é o ataque comum dele (o dano da régua) ou uma ação narrativa. Um verbo **com determinador** é especial e segue o molde:

```
Verbo (Custo · Alcance · Alvo · Resistência · Efeito · Marcador)
```

| Parte | Opções |
|---|---|
| **Custo** | livre (usa ao ser ativado) · **NC** (N de Cataclisma) · **reação** (Chefes; custa 1C a mais) |
| **Alcance** | Toque · Perto · Médio · Longe · Horizonte |
| **Alvo** | 1 · Grupo (até 3) · Área (todos no alcance) · si |
| **Resistência** | *ataque* (d20 + Ataque contra Esquiva) ou um **verbo** (o jogador rola 2d12 + atributo + verbo contra a Dificuldade do adversário: Desviar, Aguentar, Firmar, Fugir...). Quando o bestiário pede só um atributo ("Agilidade"), o jogador soma o verbo que a descrição pedir (Agilidade + Desviar) |
| **Efeito** | Dano ×½ / ×1 / ×1,5 / ×2 · uma **Condição** · Movimento · Criar/Invocar · Proteger · Anular · Selar palavra · Mexer em Arcano/Cansaço |
| **Marcador** | Instante · até a próxima Falha · até o mestre emoldurar · até o mestre pagar · o Evento |

**Quanto custa um verbo** (o gerador usa isto para balancear):

| Efeito | 1 alvo | Grupo | Área |
|---|---|---|---|
| Dano ×1 | livre | 1C | 2C |
| Dano ×1,5 | 1C | 2C | 3C |
| Dano ×2 | 2C | 3C | 4C |
| Condição leve (Caído, Distraído, Amedrontado) | livre | 1C | 2C |
| Condição forte (Preso, Cego, Atordoado, Selado) | 1C | 2C | 3C |
| Invocar Lacaios / um Padrão | — | 3C / 4C | — |
| Anular uma magia | Cataclisma igual ao Grau | | |
| Marcar Cansaço (1 / 2) | livre / 1C | 1C / 2C | 2C / 3C |

Dano **e** condição no mesmo verbo: some os custos. Marcador "o Evento" ou "até o mestre pagar": +1C.

**Condições**: *Caído* (o próximo ataque contra ele tem Vantagem; levantar é a ação), *Distraído* (Desvantagem na próxima rolagem), *Amedrontado* (não se aproxima da fonte do medo), *Preso* (não se move; soltar-se é Força ou Agilidade contra a Dificuldade), *Cego* (Desvantagem para tudo que precisa ver), *Atordoado* (perde a próxima vez que agiria), *Selado* (uma palavra sua não funciona), *Vulnerável* (ataques contra ele têm Vantagem), *Envenenado* (marca 1 Cansaço sempre que o mestre emoldura).

### PARECER (adjetivos-passivas)

Cada adjetivo é uma passiva pronta. Tabela d66 para o gerador sortear:

| d66 | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---|---|---|---|---|---|
| **1** | **Blindado**: Limiares +25% | **Veloz**: move até Médio, Dif +1 | **Enorme**: Limiares ×1,3, ataca Grupo no Toque, não se esconde | **Minúsculo**: Dif +2, PV −1 | **Voador** | **Aquático**: no fundo da água, Vantagem em tudo |
| **2** | **Escavador**: some no chão e reaparece até Médio | **Invisível**: até atacar; ataques contra ele com Desvantagem | **Venenoso**: o ataque comum deixa Envenenado | **Espinhoso**: quem o ataca no Toque sofre 1d6 | **Regenerante**: limpa 1 PV quando o mestre emoldura | **Etéreo**: armas sem magia causam metade |
| **3** | **Faminto**: ataca primeiro o mais ferido | **Furioso**: com metade dos PV, +1 dado de dano | **Covarde**: foge com 2 PV ou menos | **Astuto**: Dif +2 contra enganar e convencer | **Antigo**: um Valor a mais | **Cego**: imune a ilusões e a Alvorum de luz, Desvantagem à distância |
| **4** | **Silencioso**: Vantagem para emboscar | **Numeroso**: é um bando; conta como Lacaio de 2 pontos que não cai com um golpe | **Brilhante**: não se esconde; quem o ataca no Toque fica Distraído | **Gélido**: quem o toca marca 1 Cansaço | **Ardente**: seu dano também queima objetos e armadura (−1 Proteção) | **Pesado**: não é empurrado; Agilidade −, Dif −1 |
| **5** | **Leal**: se um aliado cai, ele age na hora | **Mentiroso**: o Curioso só descobre metade | **Faminto de Magia**: toda magia Perto dele gera +1 Cataclisma | **Marcado**: um Valor alheio que ele roubou (uma palavra de outro Valor) | **Sagrado**: Dif +2 contra Morbidyum e Dominum | **Imundo**: cura perto dele custa +1 Arcano |
| **6** | **Barulhento**: todo mundo sabe onde ele está | **Paciente**: só age quando o mestre emoldura por uma Falha | **Frágil**: Limiares −25%, Dif +1 | **Duro**: marca 1 PV a menos por golpe (mínimo 1) | **Belo**: Desvantagem para atacá-lo na primeira vez (Carisma para resistir) | **Estranho**: uma regra só dele, que o mestre inventa |

### QUER

Uma frase: o que ele quer e quando desiste. "Quer levar a criança; foge se ferido." Adversários com vontade conversam, fogem, se rendem, traem. Nem todo Evento com adversário é combate.

## Valores e maestria

Todo adversário pode ter **Valores**. Regra geral da roda de opostos ([[03 Valores]]): **resiste ao próprio Valor** (magias dele: Dificuldade +2 e metade do dano) e é **fraco contra o oposto** (Dificuldade −2 e +1 dado de dano).

Um Conjurador ou Chefe com Valor **conjura com palavras do Valor** como os jogadores: os verbos dele podem ser palavras da tabela daquele Valor.

Adversários experientes têm **Maestria**, que muda como a magia dos jogadores interage com eles:

| Maestria | Efeito |
|---|---|
| **Anulador** | reação: gasta Cataclisma igual ao Grau para anular uma magia contra ele ou Perto dele |
| **Resiliente** | +2 na Dificuldade e na resistência contra toda magia, e condições causadas por magia duram só **até o mestre emoldurar** |
| **Inquebrável** | não pode ser controlado, movido, derrubado, selado nem enganado por magia; Dominum, Persona e Cognitium não o controlam |
| **Quimerarcano** | mistura 2–3 Valores: conjura como Magia em Dupla sozinho (Grau +1), troca o Valor ativo gastando 2C (mudando resistência e fraqueza), e **não tem fraqueza fixa** |
| **Deidade** | ignora magias de Grau 1 e 2, age duas vezes quando ativado, tem uma **Presença** (o lugar à volta vira Terreno do seu Valor) e não tem fraqueza |

| Moldura | Valores | Maestrias |
|---|---|---|
| 1 | 0–1 | nenhuma |
| 2 | 1 | 0–1 (Anulador ou Resiliente) |
| 3 | 1–2 | 1 |
| 4 | 2–3 | 1–2 (Chefes: 2; Deidade só para semideuses e acima) |

Marcas menores, que o gerador também sorteia: *Imune a [Valor]*, *Absorve [Valor]* (a magia cura), *Espelho* (magia que falha contra ele volta para quem conjurou), *Controlável* (Dominum e Persona sem Retaliação contra ele), *Indomável* (só Dominum controla), *Com Familiar*.

## Tipos (para o gerador)

| Tipo | Papéis comuns | Valores e maestrias típicas | Palavras de Ser comuns |
|---|---|---|---|
| **Gente comum** | Lacaio, Padrão, Social | nenhum ou 1 fraco | Guarda, Bandido, Nobre, Mercador |
| **Comprometidos hostis** (feiticeiros, magos) | Conjurador, Líder, Chefe | 1–2, Resiliente, Espelho | Mago, Aprendiz, Sacerdote, Duelista |
| **Monstros e feras** (de Kairvahnn) | Bruto, Ágil, Lacaio | Dominum ou Primitivum, Controlável | Fera, Matilha, Predador |
| **Disruptos** (do Cataclisma) | qualquer | Valor sorteado; **Instável** (dano Grave nele dispara uma Faísca) | Sobra, Rasgo, Eco |
| **Protetores do Arcano** (ordem e espécie) | Padrão, Líder, Chefe | Anulador, Inquebrável | Guardião, Selo, Juiz |
| **Seres de energia** | Ágil, Conjurador | Etéreo, Absorve [seu Valor] | Chama, Luz, Vento, Eco |
| **Seres dos planos** (disformes, feéricos, entidades, projetos, deuses sem nome) | qualquer | Valor do plano | Juramentado, Moldado, Manso, Sentido, Vigia, Esquecido, Repetido |
| **Semideuses** | Chefe | Valor do pai, Deidade | o nome dele |
| **Deuses antigos** | não se derrota em combate | Deidade + Inquebrável | Evento de Ameaça: sobreviver, fugir, convencer |

## Criar em 6 passos

1. **Moldura e Papel** → copie os números da régua e aplique o Papel.
2. **Ser**: 1–2 substantivos.
3. **Agir**: verbos (quantos o Papel pede). O primeiro é o ataque comum. Dê determinador aos especiais e some o custo pela tabela.
4. **Parecer**: 1–3 adjetivos da tabela (ou invente um com peso parecido).
5. **Valores e Maestria** pela Moldura.
6. **Quer**: uma frase.

Conferência rápida: nenhum verbo livre pode causar mais que dano ×1 num alvo ou condição leve; um adversário não deve ter mais de um verbo de Área na Moldura 1.
