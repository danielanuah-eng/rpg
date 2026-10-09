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
5. **Dano**: simultâneo. Cada bloqueador recebe o que lhe foi distribuído; **o que passa da Vida restante de cada bloqueador vai para o Conjurador defensor** (excedente). **Todos os bloqueadores causam dano ao atacante ao mesmo tempo**, mesmo os que morrem nesse combate. Sem bloqueio, o alvo recebe o dano; Aliado, Mística ou Estrutura atacados revidam, o Conjurador não. **O Conjurador atacante nunca recebe revide** (decisão 36): nem do alvo atacado nem dos bloqueadores. Quando o **Conjurador está entre os bloqueadores**, a Armadura dele evita dano (seção 9.3), inclusive do excedente.
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
- **Protegido** (rodada 6; unificado em 04/10, decisão 31): quem mira esta carta com uma carta ou ativação do oponente paga o que ele pede. "Protegido N" = N de Arcano a mais; as variações pedem também Vida, descarte, cartas do Grau ao cemitério ou Grau mínimo.  Dois Protegidos na mesma carta são cobrados os dois.
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

**Construção de baralho (decisão 37):** 60 cartas, no máximo 3 cópias de cada (Minas Fundas incluídas) e só cartas que servem ao Conjurador: 1 Valor = um dos 2 dele; 2 Valores = exatamente os 2 dele; 3 ou mais = os 2 dele estão entre os da carta.

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
23. **Itens v3 (04/10)**: o **custo** e o **Grau** da carta do item (podem ser 0) são o Arcano pago e o Grau mínimo para **atacar com a arma**, para a **Armadura proteger** ou para **usar** Extras, Focos e Transcritores. Armas básicas: Ataque 1 de graça (Grau 0); Ataque 2 com Grau 1 e custo 1; Ataque 3 com Grau 3 e custo 2. Todo item não básico tem efeito, e os efeitos dão usos diferentes, não necessariamente melhores: arma que compra ou gera Arcano ao atacar, que causa dano a mais em Aliados/Místicas/Estruturas, que desgasta a mais a Armadura inimiga; Armadura que gera Arcano ou compra ao proteger, ou que devolve dano ao atacante (espinhos). **Item com Valor segue a regra das cores das cartas** (decisão 37, revisto em 04/10: 1 Valor, quem tem esse Valor; 2, exatamente os do Conjurador; 3 ou mais, o par dele entre os do item); itens sem Valor (básicos e genéricos) servem a todos. Testes em `tcg/test/itens-v3.test.ts`.
24. **Etéreo (04/10)**: em combate, só Etéreos atingem uma carta Etérea. Aliados Etéreos atacam e bloqueiam; o Conjurador ataca um Etéreo se tiver **arma Etérea** e bloqueia um atacante Etéreo se tiver **Armadura Etérea**.
25. **Incomensurável (04/10)**: o dano que passa da Vida restante da carta atingida só vai para o Conjurador dela se o atacante (ou a carta fonte do dano) tiver **Incomensurável**: ao ser bloqueado, ao atacar uma unidade e em magias/efeitos de dano. Sem a palavra-chave, o excedente se perde. Para o Conjurador, vale a arma.
26. **Construtor de efeitos (04/10)**: cada carta pode ter `habilidades` (condição + efeitos que se acumulam, em ordem) e `protecao` (proteção mágica; hoje Protegido, decisão 31), montadas no Editor do site por tabelas. Condições: ao jogar, quando entra em campo, virar/pagar (custo: suspender, Arcano, Vida, descarte, Grau mínimo), quando vira alvo, quando é colocada no Grau, quando ataca, quando defende, quando cai no cemitério, quando é banida, quando causa dano ao Conjurador. Efeitos: dar dano, curar, destruir, voltar para a mão, voltar para o baralho, proibir de atacar, virar, levantar, aumentar Ataque/Vida (no turno ou permanente) — num alvo ou em todos de um lado, dos tipos escolhidos (Aliado, Mística, Estrutura, Conjurador, equipamento) —, comprar, gerar Arcano, olhar o topo (escolhidas e resto para destinos), descartar (você ou o oponente, escolhendo ou ao acaso), pôr no Grau (topo ou mão), olhar a mão do oponente (descartar, pegar, mandar ao fundo), jogar carta de uma zona sem pagar, perder Vida, anular, desconto. Efeitos depois de uma escolha esperam a escolha terminar. Gatilhos sem alvo escolhido miram sozinhos (o inimigo mais forte, ou o seu mais ferido). Proteção mágica: quem mira paga Arcano, Vida, descarte, cartas do Grau ao cemitério ou precisa de Grau mínimo. Testes em `tcg/test/construtor.test.ts`.
27. **Concessão e Reanimum (04/10)**: **Concessão** é uma estrutura genérica dos dados (`palavras`, `ataque`, `vida`, `turnos`, `destino`) que o Editor monta sozinho. Dá a uma carta em campo qualquer palavra-chave que o motor entende (Etéreo, Proficiente, Incomensurável, Protegido N, Reanimum...), ±Ataque/±Vida (permanente enquanto está em campo; Vida a 0 morre pelas regras normais) e/ou um tempo em campo. Pode ser: um efeito de campo com ação "conceder" (um alvo escolhido, ou todos), parte do efeito `invocar` (a ficha já entra com ela), de `reanimar` ou de `jogarDe`. **Turnos** (autor, 04/10): o turno em que a carta entra (ou recebe a concessão) conta como o **1º, seja de quem for**; depois contam só os turnos do **controlador**; a carta sai no **Final do X-ésimo turno contado** e vai para o **destino** (padrão cemitério; cemitério = destruída; banimento; baralho = embaralha no baralho do dono; mão). O contador fica em `turnosRestantes` na instância (a mesa mostra ⏳). Ao sair do campo por qualquer motivo, a concessão acaba. **Reanimum**: a carta com essa palavra-chave pode ser jogada do **seu** cemitério, pagando o custo normal, nas mesmas condições de velocidade e fase da mão; **não é repetível**: a Temporária jogada do cemitério vai para o **banimento** ao terminar de resolver (ou ao ser anulada), e o Aliado/Mística/Estrutura vai para o banimento quando sair do campo, por qualquer motivo. Ir para o cemitério por fim do tempo conta como morte. **Diástaros**: o Aliado que o `reanimar` dele traz entra com Etéreo (`concessao` no poder); os outros não. **Etéreo concedido** obedece às regras de bloqueio: só Etéreos (ou Conjurador com Armadura Etérea) bloqueiam um atacante Etéreo. Testes em `tcg/test/concessao.test.ts`.
28. **Destruir não mira o Conjurador (04/10)**: efeitos que tiram a carta do campo (`destruir`, `devolver` e as ações `destruir`/`devolver`/`paraBaralho` do efeito `campo`) e a concessão só de tempo em campo (turnos/destino, sem palavra nem ±Ataque/Vida) **nunca** têm o Conjurador como alvo válido, mesmo que o Editor marque o tipo Conjurador. Sem tipo declarado, o padrão é carta permanente que não seja Conjurador (Aliado, Mística, Estrutura). Vale na jogada, na escolha depois (`escolherCartas` com `aplicar`), na mesa (o Conjurador não acende) e no bot. Dano, cura e concessão de palavra/±Ataque/Vida continuam podendo mirar o Conjurador quando o efeito permite. Testes em `tcg/test/alvos-destruir.test.ts`.
29. **Editor de Conjuradores (04/10)**: o poder de cada Conjurador abre no construtor de efeitos do Editor do site. A ativação e o gatilho de cada um viram linhas do construtor e voltam aos campos deles sem mudar nada (conversão em `tcg/src/poder-conjurador.ts`; ida e volta dá o mesmo JSON). Além da ativação, do gatilho e das passivas, o poder pode ter **`habilidades`** no formato das cartas: **ativadas** (custo: suspender o Conjurador, Arcano, Vida, descarte, Grau mínimo; opção **Repentina**, que vale a qualquer momento com prioridade; as outras só na sua Principal com a pilha vazia), **gatilhos de Aliado** (um Aliado seu entra em campo / vai do campo para o cemitério / causa dano de combate ao Conjurador inimigo; opções "uma vez por turno" e "só no seu turno") e condições do **próprio Conjurador** (quando ataca, defende, vira alvo, causa dano de combate ao Conjurador inimigo). Cada habilidade conta o próprio "uma vez por turno". Nova passiva **`permiteJogarDe`**: pode jogar do seu cemitério ou banimento cartas dos tipos e custo máximo escolhidos, pagando o custo normal e na velocidade normal; opções "depois vai para o banimento" (como o Reanimum) e "uma carta por turno". Efeitos que entraram na tabela do construtor: reanimar (só em ativada), banir da mão com desconto, liberar Anular do cemitério, potência de magia, Ataque do Conjurador no turno, e os efeitos do formato antigo (devolver/dano/curar/destruir com alvo fixo, olhar o topo), que abrem sem virar efeito de campo. O texto da habilidade é gerado do poder quando o autor não o escreveu; se escreveu, só os números mudam sozinhos. Nenhum dos 9 mudou de comportamento. Testes em `tcg/test/editor-conjuradores.test.ts` e `online/test/construtor.test.ts`.
30. **Protessência (04/10)**: palavra-chave. A carta com Protessência **não pode ser escolhida como alvo por efeitos controlados pelo oponente do dono**: cartas, ativações, habilidades de Conjurador, gatilhos, escolha posterior (`escolherCartas` com `aplicar`), "o jogador atingido escolhe" e "ao acaso" (o sorteio do oponente não a inclui). Na pilha, o oponente **não pode anulá-la** (Anular, inclusive o liberado do cemitério); "pagar para anular" não mira e continua valendo (**confirmado pelo autor, 04/10 noite**). Continua sendo afetada por **efeitos sem alvo** (alcance todos, varreduras) e pelos **efeitos do próprio dono** (concessões, cura, até destruir). **Ataque de unidade não é efeito**: um Aliado com Protessência pode ser atacado por unidades (**confirmado pelo autor, 04/10 noite**). Vale também concedida (Concessão) e a Concessão ganhou o campo genérico `palavrasTurnos`: as palavras duram N turnos do dono (o turno em que são dadas conta como o 1º) e somem no Final do último. Com a mesma palavra dada fixa e por turnos, **só a por turnos expira**; a fixa continua (**confirmado pelo autor, 04/10 noite**). Se o alvo ganhar Protessência depois de escolhido e antes de resolver, o efeito **perde esse alvo** (registro "Alvo protegido"). Validação única em `protegidaDe` (motor: `alvosCampo`, `resolverAlvo`, `alvoServe`, anular); a mesa não acende a carta para o oponente e mostra ◈; o bot não a escolhe. Testes em `tcg/test/protessencia.test.ts`.
31. **Protegido unificado (04/10, noite)**: "Protegido por mágica"/"Proteção mágica" e "Protegido" eram o mesmo efeito; agora é um conceito só, **Protegido**, no campo `protegido`: número N (= N de Arcano a mais, como sempre) ou objeto com as variações `arcano`, `vida`, `descartar`, `grauParaCemiterio`, `grauMin`. O nome antigo `protecao` ainda carrega e é convertido na carga (`normalizarProtegido`, nas edições do site e de `data/edicoes.json`); com os dois, o Arcano se soma. **Vários Protegidos na mesma carta** (o dela, o dado pelo Conjurador, "Seus Aliados têm Protegido N", e cada um concedido, "Protegido 2", fixo ou por turnos) **valem ao mesmo tempo** (autor, 04/10 noite: "não soma, trigga os dois"): não viram um Protegido maior; cada um é cobrado por si e o oponente paga todos juntos, na mesma jogada (`protegidosDe` lista cada um; `custoProtecao` cobra o Arcano de todos). Antes valia o maior. Texto: "Protegido 2." ou "Protegido 1: para mirar esta carta, o oponente também precisa pagar 2 de Vida e descartar 1 carta." No Editor, o Protegido fica num bloco do construtor. Nenhuma carta mudou de comportamento.
32. **Efeitos secundários (04/10, noite)**: custos ou consequências que vêm depois de outro efeito (marca `secundario` no efeito; só muda o texto e a avaliação do bot): o Conjurador **toma X de dano** (`perderVida`); **descarta/bane X cartas** da própria mão (quem descarta escolhe, ou ao acaso); **sacrifica X Aliados** (`sacrificar`: **só Aliado**, Místicas e Estruturas não, autor 04/10 noite; o dono escolhe; com só o necessário, vão sem escolha; vai do campo para o cemitério sem ganhar nada; **não mira**, então ignora Protegido e Protessência; **conta como morte** para gatilhos, como a saída por tempo); **manda X cartas do Grau ao cemitério** (`grauAoCemiterio`, o dono escolhe; com menos, manda as que tiver). Sempre sobre quem jogou/ativou, ou "o oponente". Texto: "Compre 1 carta; depois, seu Conjurador toma 2 de dano e você sacrifica um Aliado." No construtor: grupo "Efeito secundário" e botão "+ efeito secundário" em cartas e Conjuradores. O bot desconta o custo da utilidade e não paga dano que mataria o próprio Conjurador.
33. **Reanimar estendido (04/10, noite)**: pôr em campo um Aliado de um cemitério. Pagamento por carta: **sem custo**, **pagando o custo dele** ou **custo + X**; cemitério: **seu, do oponente ou qualquer**; filtros opcionais de **custo máximo** e **subtipo**; Concessão. Quem controla o efeito **escolhe ao resolver** (`escolherCartas`, só os que dá para pagar) e paga na hora: reserva, depois cartas levantadas do Grau e, se quiser, **cartas da mão enviadas ao Arcano** (autor, 04/10 noite; o comando `escolherCartas` leva `enviarArcano`, pago pelo mesmo caminho do "pagar para anular", só as necessárias; a mesa abre a janela de pagamento). Sem poder pagar, **não traz nada**. Funciona em qualquer condição (ativação, ao jogar, ao entrar, gatilhos). Formato antigo (`custoExtra` sem `pagamento`) = custo + X; a ativação com o Aliado escolhido no comando continua pagando ao ativar. Cemitério que Respira e Velho Ossário, que o motor nunca cobrava ao jogar, ficaram com `pagamento: 'gratis'` (mesmo comportamento; o texto agora diz "sem pagar").
34. **Gatilho "No Cataclisma do turno do oponente" (04/10, noite)**: condição `cataclismaOponente` para cartas e Conjuradores; dispara ao entrar na etapa de Cataclisma do turno do adversário, para as cartas em campo, os itens e o Conjurador de quem não está jogando. Os efeitos vão para a pilha e resolvem antes do Final.
35. **Tirar da mão (04/10, noite)**: o efeito `descartar` virou genérico: ação **descartar** (cemitério) ou **banir** (banimento); de **você, do oponente ou dos dois** (você primeiro); X cartas; modo **ao acaso**, **o dono escolhe sem mostrar** ou **quem controla o efeito olha a mão e escolhe** (a visão online mostra essas cartas só para quem escolhe). `aleatorio: true` (formato antigo) = ao acaso. Texto: "O oponente descarta 1 carta ao acaso", "Olhe a mão do oponente e bana 1 carta dela". O "banir uma carta da sua mão com desconto" (`banirDaMao`) continua como efeito próprio. Testes de 31 a 35 em `tcg/test/pedidos-0410-noite.test.ts` e o sigilo em `online/test/online.test.ts`.
36. **Conjurador atacante sem revide (04/10, bug do autor)**: quando o Conjurador **ataca** com a arma um Aliado, Mística ou Estrutura (ataque não bloqueável), ou ataca o Conjurador inimigo e **é bloqueado** (por um ou vários), ele **não recebe o dano de volta**. Complementa a decisão 15 (o Conjurador que **bloqueia** não devolve o dano da arma): o Conjurador nunca participa do revide de combate, em nenhum dos lados. Antes, a seção 8 e a decisão 20 ("todos os bloqueadores causam dano ao atacante") valiam também para ele; nenhuma decisão anterior pedia isso de propósito. **Efeitos explícitos continuam valendo**: espinhos de Armadura (decisão 23) e gatilhos/habilidades que causam dano ao atacante são efeitos, não revide. O Aliado atacante continua recebendo o revide normal. O bot agora ataca com o Conjurador mesmo com Aliados inimigos de pé (de preferência matando uma unidade) e não conta "matar o Conjurador atacante" ao bloquear. Testes em `tcg/test/conjurador-sem-revide.test.ts`.
37. **Regra das cores na construção de baralho (04/10, pedido do autor)**: cor = Valor; o Conjurador tem 2. Carta de **1 Valor** entra se for um dos 2 do Conjurador (como antes). Carta de **2 Valores** só entra se forem **exatamente os 2** do Conjurador. Carta de **3 ou mais Valores** só entra se os 2 do Conjurador estiverem entre os dela (o par dele é uma das duplas da carta). Sem Valor: qualquer Conjurador. Itens seguem **a mesma regra** (`itemServe` usa `valoresServem`; o autor decidiu em 04/10 que item de 1 Valor serve a quem tem esse Valor), além de competência e exclusividade; o side deck do site e o sorteio do bot usam `itemServe`. Uma função só (`cartaServeConjurador` no motor) vale no motor (`validarBaralho`), no Worker (salvar, partida rápida, salas, comunidade), no construtor e no Editor do site (mostra quem pode usar a carta). Os 9 baralhos prontos do site foram refeitos (37 trocas pela regra depois da migração das simulações, de onde eles saem; mesmo tipo e custo próximo, até 3 cópias, listadas no `node construir.ts`). Baralhos de jogadores que ficaram fora da regra **não são apagados**: aparecem como inválidos, não entram em partida e o construtor oferece **Corrigir automaticamente** (a mesma troca dos prontos). Os 9 baralhos das simulações (`tcg/sim/conjuradores-decks.ts`) foram **migrados** a pedido do autor com `corrigirCores` (mesmo tipo e custo próximo, até 4 cópias como já usavam; 201 cópias trocadas) e são conferidos pela regra nova, sem exceção. O app de testes antigo (`tcg/app`) não muda. Testes em `tcg/test/cores-baralho.test.ts` e `online/test/online.test.ts`.
38. **Equipamento do bot aleatório ou escolhido (04/10, pedido do autor)**: na tela Jogar contra o bot, "Equipamento do bot" vem em **Aleatório** (padrão) ou **Escolher** (os mesmos seletores de Canal/Armadura/Extra, filtrados pelo Conjurador do bot). No aleatório, sai um item de cada espaço entre os que servem ao Conjurador do bot (`itemServe`), sorteado pela **semente da partida** misturada com o id do Conjurador (os 9 bots não tiram o mesmo item com a mesma semente), com **peso 3 para item com Valor do Conjurador e peso 1 para genérico/sem Valor** (`PESO_ITEM_DO_VALOR`, autor 04/10) (`sortearItensBot` em `tcg/sim/bot.ts`): retomar a partida dá o mesmo equipamento; a revanche sorteia de novo. Espaço sem item que sirva: o básico (Cajado, Gibão, Poção). A escolha fica em `pref-jogar`. O bot agora **usa os itens com uso**: Extra de cura só com a Vida em metade ou menos; Arcano só quando destrava uma carta da mão; comprar/olhar com até 3 cartas na mão; dano só para matar; foco só com uma Temporária de dano/cura pagável; o que suspende o Conjurador só quando ele não vai atacar. Simulações sem travas nem erros novos. Testes em `tcg/test/itens-bot.test.ts`.
39. **Cartas provisórias (04/10, madrugada, pedido do autor)**: para os 9 baralhos voltarem a ter a identidade que a regra das cores (decisão 37) tirou, 46 cartas novas (versão 2: + 5 Místicas com ativação ou entrada, no lugar das Místicas básicas, e + 2 de Primitivum) de **1 Valor** (`tcg/src/cartas-provisorias.ts`, prefixo `prov-`, campo `provisoria: true`) cumprem a função das que saíram (remoção, varredura, Ira, Remoção que cresce, devolver, anular, compra, cura, Espião, Mercador, Etéreos, Colosso, estruturas de cerco, torre, cura e Arcano, Aliados agressivos/defensivos). Só mecânicas que o motor já tem; números dentro da régua de `src/balanco.ts` (também com os preços das simulações); texto gerado da mecânica; nome = título de uma arte aprovada ainda sem uso (ligada em `online/dados/identidade.json`; nenhuma da expansão bloqueada). Elas não passam pela redistribuição de Valores do site (`online/valores.ts`): o Valor delas é o escolhido. No Editor do site aparecem com a etiqueta "provisória" e no filtro **Provisórias**; o autor revisa efeito e números. Arte final pedida ao GPT em `cartas/pedidos/cartas-provisorias-v1.claude.json`. Testes em `tcg/test/cartas-provisorias.test.ts`.
40. **Construtor de efeitos ampliado (05/10, pedido do autor)**: tudo genérico nos dados (`habilidades`), com controle no Editor, texto gerado e motor.
   - **Eventos de jogador** (condições novas para cartas em campo, itens e Conjurador): compra cartas (`compra`, uma vez por compra; a mão inicial não conta), Conjurador toma dano (`conjuradorDano`: dano de combate ou de efeito, e os custos escritos como "toma dano"), ganha Vida (`ganhaVida`: **cura e ganhar Vida são o mesmo evento**: curar dano de verdade ou +Vida no Conjurador), perde Vida (`perdeVida`: qualquer perda, inclusive dano, pagar Vida e "perde Vida"), joga Aliado / Mística / permanente (Aliado, Mística ou Estrutura), ativa uma habilidade (comando ativar, inclusive a de Arcano) e **carta vai para a pilha** (só cartas jogadas; gatilhos não contam). Cada um com **De quem** (`quem`: você, o oponente ou qualquer jogador; sem o campo = qualquer). Os eventos de jogar disparam quando a carta vai para a pilha, então resolvem antes dela.
   - **Vezes por turno** (`vezesPorTurno`) generaliza "uma vez por turno" (`umaVezPorTurno` continua valendo = 1; o Editor grava 1 assim). Conta quando dispara, pagando ou não.
   - **Sem laço infinito**: o gatilho guarda a cadeia de habilidades que o causou; uma habilidade não dispara de novo por algo que a própria cadeia fez (vale também depois de escolhas pendentes). Ex.: "quando você compra, compre 1" compra uma vez só.
   - **Custo condicional** (`custo` no efeito, `pagar` no gatilho, `manutencao`): Arcano (reserva, Grau levantado e cartas da mão enviadas ao Arcano), Vida (não pode zerar), descartar, cartas do Grau ao cemitério e sacrificar Aliados (o pagador escolhe as cartas em seguida). **Quem paga**: você paga **para acontecer**; o oponente paga **para impedir** ("a menos que o oponente pague X"). Pagar é sempre escolha (pendência `pagarCusto`); quem não tem como pagar é tratado como "não pagou". **Quando** (só em efeitos ao jogar/ao entrar): ao resolver (padrão) ou **ao jogar a carta** (decidido logo depois de jogar; guardado no item da pilha). **Se o efeito for impedido**: só esta linha não acontece (padrão), a carta inteira é anulada (as linhas seguintes não acontecem; decidido ao jogar, ela sai da pilha para o cemitério) ou outros efeitos acontecem no lugar. O gatilho com custo: sem o custo, nada acontece. `requisito.arcano` e `pagarParaAnular` continuam como antes (dados antigos carregam igual).
   - **Enquanto estiver em campo** (`quando: 'emCampo'`): **passivo** (sem pilha; vale assim que está em campo e some quando sai; recalculado a cada consulta, nada gravado nas cartas): bônus de Ataque/Vida e palavras-chave a um grupo (seus / do oponente / dos dois; Aliado, Mística, Estrutura, Conjurador; "menos esta carta") e custo das cartas mais caro ou mais barato (de você, do oponente ou dos dois; por tipo). **Recorrente**: dispara numa etapa (início, principal, Cataclisma, Final) do seu turno, do oponente ou de cada turno, enquanto estiver em campo ou só nos N primeiros turnos que contam; vai para a pilha e não resolve se a carta saiu. No Conjurador e nos itens vale sempre.
   - **Manutenção** (passivo ou recorrente): custo cobrado numa etapa/turno (padrão: início do seu turno); vai para a pilha por cima do recorrente da mesma carta. Sem pagar (ou com o oponente pagando): **a carta sai** (cemitério, que conta como morte; banimento; mão; fundo do baralho; o Conjurador nunca sai), **o efeito não acontece neste turno** (passivo e recorrente desligados até o fim do turno) ou **efeitos alternativos**.
   - Bot: paga quando pode e o efeito (ou manter a carta) vale mais que o custo; como oponente, paga para impedir quando o efeito dói mais que o custo. Mesa: painel com o custo, o que acontece pagando e não pagando, botões Pagar / Não pagar (Pagar e impedir / Deixar acontecer) e janela de envio ao Arcano; tudo no registro. Testes em `tcg/test/construtor-ampliado.test.ts`.

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
