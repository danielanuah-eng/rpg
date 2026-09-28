# Arcanoclisma — Roteiro de criação

> **Arcanoclisma — um passo diante do desconforto e do compromisso.** O plano segue as 5 fases que você definiu e termina como o **Peso do Céu**: livro, cartas, escudo do mestre, fichas de papel enfeitadas, e um app (ficha + geradores) num **Netlify separado**, com imagens.

A regra que vale para todas as fases: **o texto das regras e os números moram em um lugar só** (estes arquivos agora; na fase 5, arquivos de dados). Livro, cartas, ficha e gerador são *gerados* a partir dali, então uma mudança numa regra chega em tudo de uma vez, igual ao Peso do Céu.

---

## Como a gente trabalha

1. Eu proponho (regra, número, texto) e marco o que precisa da sua decisão com **❓ P-xx**.
2. Você responde em [[09 Perguntas pra decidir]] (pode ser só "ok", "não", ou a sua versão).
3. Eu aplico, rodo a conta em `calc/dados.py` quando mexe em dado, e riscamos o item aqui.
4. Nada passa de fase sem um **teste de mesa** da fase (uma cena jogada de verdade, nem que seja com 2 pessoas).

Legenda: ✅ feito · 🟡 rascunho pronto, falta sua decisão · ⬜ não começado

---

## Fase 1 — Sistema e mecânicas (rascunho completo; volta quando precisar)

Objetivo: o jogo **funciona e é gostoso de jogar** antes de ter lore bonita. Nada de texto final ainda.

| # | Tarefa | Arquivo | Status |
|---|---|---|---|
| 1.1 | Rolagem base, paridade, Arcano/Cataclisma | [[01 Regras centrais]] | 🟡 |
| 1.2 | Holofote (combate sem turno), alcances, dano, PV, Cansaço, morte | [[01 Regras centrais]] | 🟡 |
| 1.3 | Magia: Graus, dados, custo, Efeitos Colaterais (d20), Crítico/Desequilíbrio | [[02 Magia]] | 🟡 |
| 1.4 | Magia em Dupla, Valores opostos, Canal | [[02 Magia]] | 🟡 |
| 1.5 | Os 9 Valores: 36 palavras, Skill, Retaliação, Ressonâncias | [[03 Valores]] | 🟡 |
| 1.6 | Palavras Básicas (36, para todo mundo) | [[02 Magia]] | 🟡 |
| 1.7 | Os 10 Comportamentos com talentos e progressão | [[04 Comportamentos]] | 🟡 |
| 1.8 | Criação de personagem e níveis 1–10 | [[05 Personagem e níveis]] | 🟡 |
| 1.9 | Armas, armaduras, catalisadores, familiares, relíquias, Ordynus | [[06 Itens e dinheiro]] | 🟡 |
| 1.10 | Mestre: gastos de Cataclisma, Abalos, adversários, terrenos | [[07 Mestre - Cataclisma, abalos e terrenos]] | 🟡 |
| 1.11 | Você responde as perguntas 🔴 de [[09 Perguntas pra decidir]] | — | ✅ rodada 1 (27/09) |
| 1.11b | Aplicar rodada 1: Palavras Básicas só verbo/substantivo, talentos de outros Comportamentos, Caderno dos Valores, itens mágicos de mecânica/Dificuldade/Valor, Marés de Valor, adversários experientes, mais terrenos | vários | ✅ |
| 1.11c | Rodada 2 respondida | [[09 Perguntas pra decidir]] | ✅ |
| 1.11d | Aplicar rodada 2: Moldura, teste por verbo + modo, Eventos e marcadores, palavra como relação, Canais (foco/arma/Transcrito), 2 talentos por Comportamento, perguntas de criação e sessão zero, ficha por Valor, Correntes, maestrias, adversários por palavras, bestiário, tesouros, dragões | vários | ✅ |
| 1.11e | Rodada 3 de perguntas (P-70 em diante) | [[09 Perguntas pra decidir]] | ⬜ |
| 1.12 | 4 personagens prontos (nível 1) para teste | [[13 Teste de mesa 1]] | ✅ |
| 1.13 | **Teste de mesa 1**: uma cena social + um combate + uma magia em dupla | [[13 Teste de mesa 1]], [[16 Sessão de teste - Castian]] | ✅ jogado (Castian, Moldura 1) |
| 1.14 | Ajustes do teste; **teste 2** no nível 5 (Moldura 3) | [[18 Sessão de teste 2 - Alvenyr]] | ✅ jogado (Alvenyr, solo, Chefe) |
| 1.15 | Simulador de combate (`calc/`) para conferir que nenhum Comportamento/Valor domina | `calc/simular_combate.py` | 🟡 primeira versão |

**Pronto quando:** uma pessoa que nunca viu o jogo cria um personagem em até 20 minutos com a ficha, e um combate de 4 jogadores contra um encontro padrão termina em 45–60 minutos sem o mestre abrir o livro mais de 3 vezes.

Perguntas de teste para a mesa (anotar depois de cada sessão):
- Alguém ficou parado esperando muito tempo? (quando o mestre emoldura)
- Arcano sobrou ou faltou? Cataclisma encheu? (economia)
- Alguém travou pensando em que palavras usar? (magia)
- Alguma magia resolveu o Evento sozinha? (Grau/limites)
- Os marcadores (até a próxima Falha, até o mestre pagar) ficaram claros sem turno?
- Os jogadores acharam fácil escolher as duas palavras do teste?
- Quem só usou arma se sentiu menor que quem usou magia?

---

## Fase 2 — Lore e descrições (rascunho completo)

Objetivo: o mundo explica as regras (por que par é Arcano, por que Eternyr é difícil, por que Dominum pode reagir).

**Princípio (decidido)**: a lore é um **esboço de universo**: resumos, rumores e versões, nada definitivo. O mestre escolhe o tipo de campanha e pode reinterpretar tudo (até a personalidade dos deuses). O que não muda é o jogo ser **profundo, empático e sensível** em qualquer uso.

| # | Tarefa | Status |
|---|---|---|
| 2.1 | Cosmogonia: Arcanoclisma, a Cisão, os Primordiais | 🟡 rascunho em [[08 Mundo e entidades]] |
| 2.2 | Um Primordial e ao menos um semideus por Valor | 🟡 |
| 2.2b | Reinos (9 de Valor + Arcano + Cataclisma), povos, culturas, estruturas — só lore de criação, sem história fechada | 🟡 rascunho em [[08 Mundo e entidades]] |
| 2.3 | As Eras jogáveis (antes da magia, clássica, tecnomaga, escola de magia) com o ajuste de regra de cada uma | 🟡 |
| 2.4 | Sociedade: como gente comum vê os magos, dinheiro (Ordynus), lugares | 🟡 em [[08 Mundo e entidades]] e [[06 Itens e dinheiro]] |
| 2.5 | Texto de ambientação de cada Valor e Comportamento (1 parágrafo + 3 frases de exemplo de jogo) | 🟡 falas e *Como é* em [[03 Valores]] e [[04 Comportamentos]] |
| 2.6 | Nomes definitivos (Cataclisma de Khairvhaan, criaturas, ordens) | 🟡 Khairvhaan e Conselho Vestryel em [[08 Mundo e entidades]] |
| 2.7 | Kit de peças para campanha (lugares, entidades, artefatos) em vez de uma história pronta: cada mesa monta a sua | 🟡 [[21 Kit de campanha]] |

As perguntas de escritor estão no fim de [[08 Mundo e entidades]]. Responda como quiser, até em áudio transcrito; eu transformo em texto.

---

## Fase 3 — O que vai ser gerado (fechada)

Catálogo fechado, formato de dados e geradores em [[22 O que é gerado]].

Objetivo: fechar a **lista de coisas** que o livro e o app vão produzir, com o formato de dados de cada uma. **Cartas ficam para o fim** (fase 5, último passo): primeiro definimos tudo.

| Tipo | Onde está o rascunho | Vai para o gerador? | Imagem? |
|---|---|---|---|
| Talentos de Comportamento | [[04 Comportamentos]] | ficha | não (ícone) |
| Ressonâncias de Valor | [[03 Valores]] | ficha | não (símbolo do Valor) |
| Palavras (9 × 36 + Básicas) | [[03 Valores]], [[02 Magia]] | ficha + sorteio de magia | não |
| Armas, armaduras, montarias | [[06 Itens e dinheiro]] | ficha (loja) + tesouro | sim |
| Itens mecânicos (5 famílias) | [[06 Itens e dinheiro]] | ficha + tesouro | sim |
| Itens narrativos, Maravilhas, comuns, bugigangas | [[12 Tesouros e itens mágicos]] | tesouro | sim (os mágicos) |
| Familiares, materiais, consumíveis | [[06 Itens e dinheiro]] | ficha + tesouro | sim |
| Terrenos, cidades, Reinos | [[07 Mestre - Cataclisma, abalos e terrenos]], [[08 Mundo e entidades]] | gerador de lugares | sim |
| Adversários (palavras) | [[10 Adversários - como criar]], [[11 Bestiário]] | gerador de encontros, balanceado | sim (só no gerador) |
| Dragões e wyverns | [[08 Mundo e entidades]], [[11 Bestiário]] | gerador | sim |
| Abalos, Marés, Correntes | [[07 Mestre - Cataclisma, abalos e terrenos]] | gerador | não |
| PNJs sociais, nomes, ganchos | — | gerador | opcional |

Formato de dados (proposta): um JSON por tipo em `dados/`, com o mesmo campo `moldura` em tudo, para o gerador escolher e balancear sozinho.

## Fase 4 — Arte (estamos aqui)

Guia visual e Esferas dos Valores em [[24 Guia visual]].

Referências visuais (P-52):
- **Página, diagramação, ornamentos**: a estética das artes do **Daggerheart**.
- **Desenho de personagens**: o traço e a estética do **Wilderfeast**.
- **Personagens das fichas e do livro**: estilos, épocas e poses diferentes (alguns futuristas, outros não), sempre **fazendo magia de algum jeito** (pelo foco, pela arma, pelo Transcrito, pelo corpo...). A ideia é passar o ponto do sistema sem prender a temática a uma Era só.

| # | Tarefa |
|---|---|
| 4.1 | Guia visual: paleta de cada Valor, a página no estilo Daggerheart, o personagem no estilo Wilderfeast |
| 4.2 | 9 símbolos abstratos dos Valores (vetor, para as fichas) — decidir depois que o resto estiver fechado |
| 4.3 | Capa do livro e das fichas |
| 4.4 | Personagens de exemplo: um por Valor e um por Canal, misturando Eras e poses |
| 4.5 | Lista de prompts, um por item/terreno/adversário/dragão (igual ao `prompts das Peças novas.md`), com id = nome do arquivo |
| 4.6 | Geração em lote (Canva), recorte de fundo, conferência uma a uma |
| 4.7 | Molduras e ornamentos das duas fichas (a do Valor muda de arte para cada um dos 9) — ✅ rascunho das 9 fichas de Valor em `arte/fichas.html` (seletor por esfera); falta a versão preto e branco e a arte de moldura de cada Valor |
| 4.8 | **Personagens das sessões ao longo do livro**, interagindo com as páginas e soltando magia (traço Wilderfeast): prompts em [[20 Prompts de arte - personagens]], uma linha nova a cada personagem criado |

## Fase 5 — Fichas, geradores, livro, site

Entregas, na mesma família do Peso do Céu:

1. **App** (um HTML só, funciona offline, instalável no celular):
   - **Ficha digital**: tudo que o personagem liberou aparece nela; contadores de Arcano/Cansaço/PV/Proteção clicáveis; montador de magia (escolhe palavras → mostra Grau, custo, dados, Dificuldade, e rola os dados contando pares/ímpares); loja em Ordynus; subir de nível guiado; guardar/importar/exportar.
   - **Escudo do mestre digital**: contador de Cataclisma com Transbordo, tabela de Abalos, o emoldurar.
   - **Geradores**: encontro (balanceado pela Moldura e nº de jogadores), adversário com imagem, terreno, tesouro/item, relíquia com palavra, PNJ, nome, gancho, Abalo, "magia aleatória" (d66 nas tabelas).
2. **Ficha de papel** enfeitada para imprimir, em **A4**, colorida e com versão econômica em preto e branco, em **duas folhas** ([[05 Personagem e níveis]]):
   - **Ficha 1 — Personagem** (igual para todos): nome, skin, Comportamento, Palavras Básicas, Arcano, Laços, Vivências, itens, Ordynus, perguntas de criação e relações da party.
   - **Ficha 2 — Valor** (9 versões, uma por Valor, com arte própria): símbolo, Compromisso, Skill, Retaliação, Ressonâncias, as 36 palavras, 4 espaços extras, Canal, tabela de Graus e magias favoritas.
3. **Livro — *Arcanoclisma: um passo diante do desconforto e do compromisso*** — o texto completo do jogo, com capa, feito para sair em **vários formatos** a partir da mesma fonte:
   - PDF completo para baixar e imprimir;
   - páginas no site, com busca;
   - leitura dentro do app (cada regra abre a partir da ficha);
   - livretos menores separados: do jogador, do mestre, de um Valor, de uma Era, do bestiário, dos Reinos, e **uma página de regras** (o que fazer na mesa, as mecânicas num resumo).
   E o **escudo do mestre** (PDF).
4. **Site** (só depois de tudo pronto) em um **Netlify novo** (separado do Peso do Céu): página de apresentação com capa, documentos para baixar, botão para instalar o app, galeria de imagens.
5. `calc/conferir.py`: confere antes de publicar que todo item/adversário está dentro da faixa da sua Moldura (o "balanceador" que o gerador usa).
