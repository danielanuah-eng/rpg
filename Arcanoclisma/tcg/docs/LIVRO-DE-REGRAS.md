# Arcanoclisma TCG — Livro de Regras (rascunho 0.11)

Escrito a partir de `TCG ARCANOCLISMA.txt`. Tudo que está marcado **[PROPOSTA]** é uma leitura minha de um trecho ambíguo e **precisa do seu OK** (ver seção 12). O motor em `src/` implementa exatamente o que está aqui.

Tudo em português. Números do jogo ficam entre 0 e 99; efeitos grandes, no máximo 20.

---

## 1. Visão geral

Dois conjuradores brigam usando feitiços. Cada jogador tem um **Conjurador** (carta fora do baralho), um **baralho de 60 cartas** e um **side deck de 15** (6 itens de Conjurador; equipamentos entram na fase 2). Vence quem zerar a vida do Conjurador do outro. **Acabar o baralho não faz perder**: o jogador só fica sem opções de compra. Quem passa **5 turnos seguidos sem agir** perde (relógio de impasse, seção 5).

Meta de ritmo: partidas de cerca de 20 turnos, com early, mid e late game bem definidos.

## 2. Zonas

| Zona | Quem vê | Observação |
|---|---|---|
| Baralho | ninguém | topo = primeira carta |
| Mão | dono | limite geral de 7 cartas (ver 5, Início) |
| Campo | todos | **sem limite de cartas**; guarda Aliados, Místicas e Estruturas (permanentes) |
| Cemitério | todos | |
| Zona de Grau | ver 2.1 | cartas viradas para cima ao entrar |
| Zona de Contador Arcano | todos | uma por jogador |
| Banimento | todos | fora de jogo; ver 9.2 |
| Pilha | todos | resolve de cima para baixo (LIFO) |

### 2.1 Zona de Grau
- 1 vez por turno, o jogador põe uma carta da mão na Zona de Grau (quem começa não pode no turno 1). **Não suspende o Conjurador** (ele ainda pode atacar no mesmo turno). Entra **virada para cima** e **não abre pilha**. No fim do turno vira para baixo.
- Cada carta no Grau pode ser **suspensa para gerar 1 Arcano**, em qualquer turno (como velocidade repentina que **não usa a pilha** e não pode ser respondida).
- Grau vai de 0 a 12 por enquanto.
- Para jogar uma carta de **grau X**, o jogador precisa ter **pelo menos X cartas** na Zona de Grau. Cartas poderosas ainda exigem **descartar N cartas do Grau** para o cemitério.
- Algumas cartas exigem Grau de um **Valor** específico (como o Poder do Riftbound): precisa haver ao menos uma carta daquele Valor no Grau.

### 2.2 Zona de Contador Arcano (enviar carta ao Arcano)
- Funciona como o **pitch do Flesh and Blood**: só se envia carta da mão ao Arcano **para pagar um custo** (jogar uma carta, atacar com o Conjurador, ativar algo que custe Arcano). Não se envia "porque sim".
- Cada carta enviada precisa ser **necessária**: não se pode enviar outra se o custo já está coberto. Mas a última carta pode gerar **mais do que falta**; a **sobra fica na reserva** até o Cataclisma (ex.: custo 1, envia uma carta que gera 4: sobram 3).
- Também vale no turno do oponente, ao responder com carta repentina.
- Não abre pilha. Sem limite de cartas por turno, desde que cada uma seja necessária.
- **No fim do turno do dono**, as cartas desta zona vão para o **fundo do baralho**.
- No motor: o comando `jogar` (e `atacar`, para o Conjurador) aceita `enviarArcano: [cartas]`. Não existe comando solto de enviar Arcano.

## 3. Arcano

- O Arcano fica numa **reserva** (como o mana pool do Magic).
- A reserva do jogador **do turno** some ao fim da fase de **Cataclisma**.
- **[PROPOSTA]** Arcano que o jogador gerou **durante o turno do oponente** permanece até o Cataclisma do próprio turno dele ("o arcano que sobrou fica na volta"). Isso resolve a ideia de compensação entre turnos.
- Cartas podem dar Arcano ao oponente como compensação (efeito `arcanoOponente`).
- Nenhum jogador recebe carta ou Arcano extra na preparação (decisão do autor na rodada 3, confirmada na rodada 6).

## 4. Preparação

1. Cada jogador rola 1d20 (empate rola de novo).
2. Cada jogador equipa e revela os **itens** do Conjurador (seção 9.3).
3. Quem tirou o maior número escolhe quem começa.
4. Cada Conjurador define a **mão inicial**.

## 5. Estrutura do turno

Cada jogador tem o seu turno. Etapas:

1. **Início** — o **Conjurador levanta** (mesmo que tenha bloqueado no turno do oponente) Não se compra no Início. Depois, gatilhos de início de turno.
2. **Principal** — joga cartas, ataca, gera Arcano, põe carta no Grau.
3. **Cataclisma** — ao fim desta fase, **todo o Arcano do jogador do turno some**.
4. **Final** — **levanta tudo que está suspenso** (Aliados, Conjurador, Grau), vira o Grau para baixo, e as cartas da Zona de Contador Arcano vão para o fundo do baralho; depois o jogador **compra a Economia do Conjurador** (2 cartas nos 9 Conjuradores). O limite de mão (7) continua sendo conferido no Início do turno. (Decisão 19, 04/10: só se compra no Final.)

**Relógio de impasse**: no Final, se o jogador do turno passou o turno inteiro **sem jogar carta, ativar efeito ou atacar**, ele marca 1 turno parado; qualquer ação zera a contagem. Com **5 turnos parados seguidos**, ele perde.

**Cada etapa gera um gatilho**: ao entrar nela, os jogadores recebem prioridade (ativo primeiro) e podem responder. Cartas em campo sem velocidade repentina precisam dizer quando ativam ("no começo da sua fase principal", etc.).

## 6. Prioridade e pilha (modelo do Magic)

- Quem joga uma carta (paga o custo, põe na pilha) recebe a prioridade de volta.
- Quando um jogador passa, o outro recebe a prioridade. Se **os dois passam seguidos**: com pilha não vazia, o item de cima resolve e o jogador ativo recebe prioridade; com pilha vazia, a etapa termina.
- **Velocidade**: **Repentina** pode ser jogada sempre que o jogador tem prioridade. **Findante** só pelo jogador ativo, na Principal, com a pilha vazia e sem combate em andamento.
- Aliados, Místicas e Estruturas são Findantes, a menos que digam "velocidade repentina".
- **Não usam a pilha**: gerar Arcano, pôr carta no Grau, suspender carta do Grau.
- Efeito sem limite escrito (por carta ou por turno) pode ser usado quantas vezes o jogador pagar.
- Alvos são checados ao jogar e de novo ao resolver. Se o alvo sumiu, aquele efeito não acontece.

## 7. Tipos de carta

Toda carta é a **relação dela com o mundo**: pode ser um personagem, o efeito de uma magia, um cenário que está acontecendo ou uma figura mágica. Todas são **feitiços**. Podem ter vários subtipos (Lendária, elfo, dragão, Aura, Vilarejo, nome de Valor...).

1. **Aliado** — pode ser personagem, criatura, monstro ou divindade, de qualquer Valor. Tem Ataque, Vida e Arcano gerado. Pode ser suspenso para atacar, bloquear ou ativar efeitos.
2. **Temporária** — efeito único, vai direto ao cemitério. **Repentina** (como instantânea) ou **Findante** (como feitiço).
3. **Mística** — fica em campo: aura, ser divino, barreira. Pode ter efeito passivo ou ativo e, se tiver Vida, pode ser destruída em ataque.
4. **Estrutura** — só é atingida por magias. Gera recurso, protege ou atrapalha. Aliados e Conjurador podem se suspender para entrar nela (fase 2).
5. **Item de Conjurador** — fora do baralho: **Canal**, **Armadura** e **Extra** (nome provisório). Ver 9.3.
6. **Extra** — carta de mais de um Valor; pode ser de qualquer tipo anterior. Fase 2.

Tokens: têm o tipo definido pela carta que os criou (fase 2).

## 8. Combate

Todo ataque acontece na Principal, com a pilha vazia.

1. **Declarar**: o atacante é **suspenso**. O Conjurador só ataca **com Canal** (arma); o ataque dele é o Ataque do Canal. Ele paga seu **custo de ataque** em Arcano (no padrão é **0**). Cria-se um item "ataque" na pilha, e o oponente pode responder.
2. **Bloqueio** (decisão 20): só se bloqueia **ataque ao Conjurador**. Quando ele resolve, o defensor escolhe **um ou vários** bloqueadores (Aliados e o próprio Conjurador **não suspensos**, que ficam suspensos) ou nenhum. Ataque a **Aliado, Mística ou Estrutura não pode ser bloqueado**: a pilha, as respostas e o cálculo do dano continuam iguais, só não há declaração de bloqueio.
3. **Resposta**: há mais uma rodada de prioridade antes do dano.
4. **Distribuição**: com 2 ou mais bloqueadores, **quem ataca distribui** o Ataque entre eles.
5. **Dano**: simultâneo. Cada bloqueador recebe o que lhe foi distribuído; **o que passa da Vida restante de cada bloqueador vai para o Conjurador defensor** (excedente). **Todos os bloqueadores causam dano ao atacante ao mesmo tempo**, mesmo os que morrem nesse combate. Sem bloqueio, o alvo recebe o dano; Aliado, Mística ou Estrutura atacados revidam, o Conjurador não. Quando o **Conjurador está entre os bloqueadores**, a Armadura dele evita dano (seção 9.3), inclusive do excedente.
6. Atacantes válidos: Aliados e Conjurador. Alvos válidos: Conjurador inimigo e Aliados, Místicas e Estruturas inimigas com Vida.

### 8.0 Entrada em campo e Proficiente
Um Aliado, no turno em que entra, **não ataca e não usa efeitos que exijam suspendê-lo** (como "Suspenda: ganhe 1 Arcano"). É como o enjoo de invocação do Magic.
- **Proficiente** tira essa restrição: o Aliado pode atacar e usar efeitos de suspender assim que entra (como a Ímpeto/haste do Magic). Aparece sobretudo em cartas de Aggro e Tempo.
- Se algum efeito permitir que o Aliado ataque no turno em que entrou, ele também pode usar efeitos de suspender, porque atacar já exige suspender.
- O Conjurador, as Estruturas e as Místicas não têm essa restrição. Entrar em campo não impede bloquear.

### 8.1 Memória de dano
Tudo que tem Vida **guarda o dano recebido mesmo depois do fim do turno**. Morre quando dano ≥ Vida. Cura reduz o dano guardado.

### 8.2 Dano tem uma fonte
Todo dano é o mesmo dano; muda a **fonte**: **efeito** (Raio, Bola de Fogo) ou **combate** (pancada). Efeito pode ser **anulado** por cartas de anulação (como **Anular**, seção 8.4); combate não. O motor tem um único caminho de dano com a fonte registrada.

### 8.3 Levantar e bloquear (tema tático)
O Final levanta tudo, e cada jogador levanta as próprias cartas no **próprio** Final. Quem **ataca** levanta no Final do mesmo turno e pode bloquear no turno do oponente; um **Aliado que bloqueia** fica suspenso e **não ataca** no próprio turno seguinte (só levanta no Final dele). Isso vale também para o **Conjurador** (decisão 22): se bloquear no turno do oponente, só levanta no próprio Final e não ataca nem usa itens no seu turno seguinte.

### 8.4 Palavras-chave e efeitos de uma linha
- **Proficiente**: ataca e usa efeitos de suspender no turno em que entra (seção 8.0).
- **Etéreo**: só é bloqueado por Aliados Etéreos.
- **Devolver**: devolve um Aliado ou Mística à mão do dono.
- **Anular**: anula uma carta na pilha (ela vai para o cemitério sem efeito).
- **Varredura**: causa X de dano a cada Aliado do oponente.
- **Remoção que cresce**: causa "X + o número de cartas no seu Grau" de dano a um Aliado ou Mística.
- **Estrutura de cerco**: "Suspenda: cause 1 de dano ao Conjurador inimigo".
- **Protegido N** (rodada 6): quem mira esta carta com uma carta ou ativação paga N de Arcano a mais.
- **"Ao causar dano de combate ao Conjurador inimigo"** (rodada 6): gatilho de uma linha em Aliados (ex.: compre 1 carta).
- **"Destrua todos os Aliados"** (rodada 6): varredura que mata (Ira: de todos; Pôr do sol: só do oponente). **Fúria**: 3 de dano em cada Aliado, inclusive os seus.
- **Curva de Grau** (rodada 6): quanto maior o Grau de um Aliado, mais Ataque e Vida ele tem para o mesmo custo (bônus de orçamento k × G × (G − 1) / 2, com k = 0,5 e G até 8). Bombas de Grau alto (Mercador de segredos, Colosso de ferro, Dragão ancestral) e unidades de Grau médio que punem quem vai largo (Piromante, Presa-trovão, Fera de cerco).

A lista de cartas do padrão, com custo, Grau e texto, está em [CARTAS-PADRAO.md](CARTAS-PADRAO.md).

## 9. Conjurador

Define: Valores permitidos no baralho, habilidade exclusiva, **Vida** (padrão: **20**, decisão do autor), **mão inicial**, **Economia** (quantas cartas compra no Final do próprio turno; 2 nos 9 Conjuradores; aparece na carta ao lado da Vida), **dano básico** (agora **0**: o ataque vem das armas) e **custo de ataque** (padrão: 0). Suspende-se para ativar habilidades, atacar e bloquear.

### 9.1 Os 9 Conjuradores iniciais (rodada 7)
Cada Conjurador combina **2 Valores** (o baralho só leva cartas desses Valores ou sem Valor) e tem uma **habilidade própria** ligada aos Valores ou a um tipo de carta. Cada Valor aparece em exatamente 2 Conjuradores. Vida 20, mão inicial 5 (repõe até 5 no Início), dano básico 0, ataque grátis. Itens padrão de teste: Cajado de aprendiz, Gibão de couro e Poção de cura.

- **Suspender o Conjurador** para usar a habilidade custa o ataque dele naquele turno (pôr Grau não o suspende).
- Habilidades ativadas de Conjurador são usadas na sua fase Principal, com a pilha vazia, **a menos que digam "Repentina"**: essas podem ser usadas sempre que você tiver prioridade.
- "Uma vez por turno" e "primeiro ... em cada turno" valem para qualquer turno, salvo quando a habilidade diz "seus turnos".

| Conjurador | Valores | Habilidade |
|---|---|---|
| Lyandra, a Domadora de Guerra | Conflitüo + Primitivum | O primeiro dano de combate que um Aliado seu causa em cada turno é 1 maior. |
| Carmal, o Druida Crepuscular | Primitivum + Alvorum | Quando o primeiro Aliado seu entra em campo em cada turno, ganhe 1 Arcano. |
| Alquimista Ricardo | Alvorum + Sublimeyn | Suspenda o Conjurador: a primeira carta que você jogar neste turno custa 1 a menos. |
| Roh, o Ilusionista das Máscaras | Sublimeyn + Persona | Suspenda o Conjurador: devolva à sua mão um Aliado ou Mística seu. |
| Agnelo, o Duelista Sombrio | Persona + Morbidyum | Na primeira vez em cada um dos seus turnos que um Aliado seu vai do campo para o cemitério, compre 1 carta. |
| Diástaros, o Necromante Vidente | Morbidyum + Cognitium | Suspenda o Conjurador: ponha no seu campo um Aliado do seu cemitério ou do cemitério do oponente, pagando 2 de Arcano a mais além do custo dele. |
| Ana, a Cronista do Tempo | Cognitium + Eternyr | Suspenda o Conjurador e pague 2: bana uma carta da sua mão. No seu próximo turno, você pode jogá-la custando 4 a menos. |
| Mitonho, o Carcereiro Temporal | Eternyr + Dominum | Repentina: suspenda o Conjurador e pague 2. Neste turno, você pode jogar do seu cemitério uma carta cujo efeito anule outra, pagando o custo dela normalmente. Ao resolver, ela é banida. |
| Osmar, o Capataz de Ferro | Dominum + Conflitüo | Seus Aliados têm Protegido 1. |

Detalhes de regra das habilidades:
- **Ricardo**: se você já jogou uma carta no turno, a habilidade não dá desconto.
- **Diástaros**: o Aliado entra no seu campo como se tivesse acabado de entrar (não ataca nem usa efeitos de suspender no turno, salvo Proficiente) e conta como "entrar em campo" para outros efeitos. Não exige o Grau do Aliado; exige pagar o custo dele + 2. Se vier do cemitério do oponente, passa a ser seu (inclusive vai para o seu cemitério depois).
- **Ana**: a carta banida só pode ser jogada no seu próximo turno, respeitando Grau, velocidade, alvos e as demais regras e efeitos em jogo; o desconto não deixa o custo abaixo de 0. Se não for jogada nesse turno, fica no banimento.
- **Mitonho**: a carta jogada do cemitério respeita o Grau e paga o custo normal; ao resolver, vai para o banimento.

### 9.2 Zona de banimento
Zona nova, de cada jogador, ao lado do cemitério: cartas **fora de jogo**. Só voltam se um efeito disser (como a habilidade da Ana). No jogo online ela fica discreta, numa aba perto do cemitério, visível ao clicar.

As habilidades usam efeitos de uma linha. Código em `src/conjuradores.ts`; simulações em [CONJURADORES.md](CONJURADORES.md). Os Valores de cada carta (`sim/valores-cartas.ts`) são uma primeira proposta baseada nas respostas da tabela e ainda precisam da revisão do autor.

### 9.3 Itens do Conjurador (pedido do autor em 03/10; respostas confirmadas)
Ficam **fora do baralho**, num **side deck**. Antes da partida, cada jogador escolhe do seu side deck **um Canal, uma Armadura e um Extra** e os equipa e revela na preparação. **Itens não são trocados durante a partida.**
- **Canal**: a arma (cajado, espada, lança, item mágico...). Tem **Ataque**.
- **Armadura**: tem **Defesa** e **vida de desgaste**. Quando o Conjurador **bloqueia**, ela evita até a Defesa de cada dano de combate; o dano evitado gasta o desgaste e, quando ele se esgota, a armadura quebra e vai para o cemitério. Sem bloquear, a armadura não protege. O dono escolhe se a usa em cada bloqueio, e ela protege no máximo **um ataque por turno** (decisão 16).
- **Extra**: item **consumível de suporte**, sem Ataque. Tem uma ativação de uso único: ao usar, o item é consumido e vai para o cemitério (ex.: Poção de cura, Elixir de foco).

O Conjurador ataca e defende **suspendendo**. O ataque dele é o dano básico (0) + o Ataque do Canal; sem Canal ele não ataca, mas pode bloquear. **O Conjurador que bloqueia não devolve o dano da arma**, só se a arma tiver efeito que diga isso (efeitos de item virão depois, junto com os modificadores por raridade).
Itens de teste: Cajado de aprendiz (Ataque 1), Lâmina rúnica (2), Lança do eclipse (3); Gibão de couro (Defesa 1, desgaste 4), Couraça de placas (Defesa 2, desgaste 6); Poção de cura (cure 3 do seu Conjurador), Elixir de foco (ganhe 2 de Arcano). Itens não são alvo de efeitos.

**Comprometimento** (libera itens, fase 2): Invocador, Brigão, Parrudão da quebrada, Habilidoso, Preparado.

## 10. Valores

Os 9 Valores (Conflitüo, Sublimeyn, Dominum, Persona, Alvorum, Morbidyum, Cognitium, Eternyr, Primitivum) respondem ao questionário do rascunho original (nota Péssimo a Muito bem). Respondida quase toda pelo autor em [TABELA-DE-VALORES.md](TABELA-DE-VALORES.md), gerado de `data/valores.mjs`. Preenchi só o que a cor e a forma da magia de cada Valor sugerem; o resto está em branco para você definir. **Dragões e divindades existem em todos os Valores** (subtipos universais, não pertencem a um Valor só).

## 11. O que está implementado agora (fase 1)

Motor puro em TypeScript, sem interface, comandado por ações: preparação, d20, zonas, Arcano, Grau, pilha com prioridade, fases, combate com bloqueio, memória de dano, cemitério, vitória. Cartas de teste (Aliado, Temporária repentina e findante, Mística, Estrutura) em `src/cartas-teste.ts`.

Fora desta fase: equipamentos, Extras, tokens, texto de efeitos em linguagem natural (parser, ver 13), interface HTML, NPCs, decks, rede.

## 12. Decisões

**Decididas pelo autor:**
1. Tudo levanta no Final; quem bloqueia não ataca, quem ataca não bloqueia (tema tático).
2. Sem descarte forçado no Início, além do limite de mão (item 6).
3. Arcano gerado no turno do oponente dura até o seu próprio Cataclisma.
4. O Conjurador pode bloquear ataques. Dano de efeito e dano de combate são o mesmo dano com fontes diferentes (anulável × não anulável).
5. Aliado não ataca ao entrar; palavra-chave **Proficiente** permite. (Ampliada na rodada 5: também não usa efeitos de suspender ao entrar.)

6. Limite de mão geral de **7**: cada jogador descarta o excesso no **Início do próprio turno**.
7. Enviar carta ao Arcano só para pagar custo (pitch), com sobra permitida (seção 2.2). Vale também para o custo de ataque do Conjurador.
8. Vida do Conjurador **20**; o Conjurador ataca sem custo.
9. Pôr carta no Grau não suspende o Conjurador (1 por turno).
10. Relógio de impasse de 5 turnos.
11. Nenhum jogador recebe carta ou Arcano extra na preparação. **Quem começa não põe carta no Grau no turno 1** (só por efeito de carta, e efeitos de pôr Grau não devem aparecer tão cedo); o Conjurador dele pode atacar no turno 1.
12. Cartas e efeitos do agente-balanceador aprovados: Etéreo, Devolver, Anular, Varredura, Remoção que cresce, Estrutura de cerco; a Estrutura de cura só cura Aliados; Aliados que descartam Grau descartam 1 a mais.
13. **Rodada 6 aprovada pelo autor**: varreduras novas, curva de Grau, unidades de Grau médio e bombas, Protegido N, gatilho de dano de combate ao Conjurador, destruir todos os Aliados e o segundo jogador sem carta extra (variante BAL → PADRAO).
14. **Recusadas**: Cataclisma crescente (dano no Cataclisma a partir do 10º turno) e Grau suspenso para quem começa. Também não se limita a quantidade de cartas de um tipo por baralho (o deckbuilding é livre).
15. **Envio de 03/10**: repor a mão até a mão inicial do Conjurador no Início (no lugar da compra no Final; limite geral 7); o Conjurador levanta no Início; o Conjurador ataca só com Canal e não devolve o dano da arma ao bloquear; itens vêm de um side deck (Canal, Armadura e Extra consumível), seção 9.3, dano básico 0; "Comprar cartas (custo 1)" virou "Olhar o topo" (Repentina: olhe 3, ponha 1 na mão, o resto no fundo). Feitos como edições do autor (`data/edicoes.json`), visíveis no editor.
16. **Armadura por escolha (03/10, site)**: quando o Conjurador bloqueia, o dono **escolhe** se usa a Armadura naquele bloqueio; **nenhuma Armadura protege mais de um ataque por turno** (o limite volta no Início de cada turno). Sem escolha explícita (bot), ela é usada.
17. **Valores das cartas (03/10, noite, site)**: não existe carta neutra; toda carta pertence a um Valor, que é a cor da bolinha do custo. Normalmente 1 Valor; 1 a cada 10 cartas tem 2 (o par de um Conjurador); 1 a cada 50 tem 3 (e assim, multiplicando por 5, até 9). Só itens podem ser genéricos (cinza), e há itens exclusivos. As cartas de um Conjurador são só dos Valores dele, salvo regra escrita. A Mina Funda virou 9 minas, uma por Valor. Sem raridade por enquanto (o autor define no Editor). Aplicado na coleção do site (`online/valores.ts`); as simulações não mudaram.
18. **Itens v2 (03/10, noite)**: o Canal pode ser **Arma** (dá Ataque ao Conjurador), **Foco Arcano** (o próximo dano ou cura de uma Temporária sua no turno fica maior) ou **Transcritor** (mexe no baralho e na mão: olhar o topo, comprar). Todo uso de item suspende o Conjurador (que precisa estar de pé) e tem um custo: pagar Arcano, descartar cartas da mão ou o Conjurador tomar dano; há itens melhores no início, no meio e no fim de jogo. **Extras de competência** só servem ao Comprometimento certo (ex.: Ovo de Wyvern para Invocador invoca uma ficha Wyvern Filhote 2/2; Luvas de Força para Brigão dão +2 de Ataque no turno pagando 2 de dano, sem suspender, para ele ainda poder atacar). O Canal não se consome; o Extra é de uso único. Ver `tcg/test/itens-v2.test.ts`.
19. **Compra só no Final (04/10)**: o autor pediu que os Conjuradores comprem apenas no Final do próprio turno, não antes da fase Principal. Volta a valer a **Economia** do Conjurador (campo `compraFinal`, hoje 2 para os 9): no Final, ele compra esse número de cartas. A regra da decisão 15 ("repor a mão no Início") foi desligada (`reporMaoNoInicio: false` em `tcg/data/edicoes.json`). A Economia aparece na carta do Conjurador ao lado da Vida (template do GPT).
20. **Vários bloqueadores (04/10)**: mais de uma unidade (e o Conjurador) pode bloquear o mesmo ataque e ajudar; quem ataca distribui o dano entre os bloqueadores e o excedente de cada um vai para a vida do jogador defensor; todos os bloqueadores causam dano, mesmo morrendo. Só se bloqueia ataque ao Conjurador; ataques a Aliados, Místicas e Estruturas (que agora podem ser atacadas, se tiverem Vida) não podem ser bloqueados. Seção 8. Testes em `tcg/test/envio-0410.test.ts`.
21. **Aliado nunca é Repentina pelo tipo (04/10)**: unidade não tem Repentina no tipo nem no subtipo; sem nada no texto, só entra na fase principal com a pilha vazia. A palavra-chave **Repentino** no texto de efeito do Aliado permite jogá-lo a qualquer momento com prioridade (em resposta, até no turno do oponente). Gato de Navio e Menina Raposa têm Repentino; o Editor do site trava a velocidade dos Aliados e troca "Repentina" por "Repentino" nas palavras-chave.
22. **Conjurador levanta só no Final (04/10)**: a regra "levanta também no Início" (decisão 15) foi desligada (`conjuradorLevantaNoInicio: false` em `tcg/data/edicoes.json`).

**Ainda abertas:**
- Cada carta do Grau gera 1 Arcano na fase 1. Recomendo manter.
- Cartas de teste são placeholders, não a coleção.
- A tabela de Valores (seção 10) é rascunho meu e precisa da sua correção.

## 13. Próximos passos sugeridos

1. Seu OK nos itens acima.
2. Tabela de Valores com as 41 perguntas e cartas-base estatísticas.
3. Linguagem de efeitos e **parser de regras** ("Games Rules Parser" à la Wizards): texto de carta em português ⇄ efeitos do motor.
4. Interface HTML jogável (campo, mão, pilha, prioridade) e depois deck builder e NPCs.
5. Pedidos ao Codex: modelo de carta no Canva (dimensões, margens, áreas de arte e texto) e arte dos Valores. Estilo Altered/Legends of Runeterra.
6. Salvar a skill de criação ao fim do processo.
