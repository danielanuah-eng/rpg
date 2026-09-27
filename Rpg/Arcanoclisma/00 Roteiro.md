# Arcanoclisma — Roteiro de criação

> Título provisório. O plano segue as 5 fases que você definiu e termina como o **Peso do Céu**: Transcrito, cartas, escudo do mestre, fichas de papel enfeitadas, e um app (ficha + geradores) num **Netlify separado**, com imagens.

A regra que vale para todas as fases: **o texto das regras e os números moram em um lugar só** (estes arquivos agora; na fase 5, arquivos de dados). Transcrito, cartas, ficha e gerador são *gerados* a partir dali, então uma mudança numa regra chega em tudo de uma vez, igual ao Peso do Céu.

---

## Como a gente trabalha

1. Eu proponho (regra, número, texto) e marco o que precisa da sua decisão com **❓ P-xx**.
2. Você responde em [[09 Perguntas pra decidir]] (pode ser só "ok", "não", ou a sua versão).
3. Eu aplico, rodo a conta em `calc/dados.py` quando mexe em dado, e riscamos o item aqui.
4. Nada passa de fase sem um **teste de mesa** da fase (uma cena jogada de verdade, nem que seja com 2 pessoas).

Legenda: ✅ feito · 🟡 rascunho pronto, falta sua decisão · ⬜ não começado

---

## Fase 1 — Sistema e mecânicas (estamos aqui)

Objetivo: o jogo **funciona e é gostoso de jogar** antes de ter lore bonita. Nada de texto final ainda.

| # | Tarefa | Arquivo | Status |
|---|---|---|---|
| 1.1 | Rolagem base, paridade, Arcano/Cataclisma | [[01 Regras centrais]] | 🟡 |
| 1.2 | Holofote (combate sem turno), alcances, dano, PV, Cansaço, morte | [[01 Regras centrais]] | 🟡 |
| 1.3 | Magia: Graus, dados, custo, Dificuldades (d20), Crítico/Desequilíbrio | [[02 Magia]] | 🟡 |
| 1.4 | Magia em Dupla, Valores opostos, Canal | [[02 Magia]] | 🟡 |
| 1.5 | Os 9 Valores: 36 palavras, Skill, Drawback, Ressonâncias | [[03 Valores]] | 🟡 |
| 1.6 | Palavras Básicas (36, para todo mundo) | [[02 Magia]] | 🟡 |
| 1.7 | Os 10 Comportamentos com talentos e progressão | [[04 Comportamentos]] | 🟡 |
| 1.8 | Criação de personagem e níveis 1–10 | [[05 Personagem e níveis]] | 🟡 |
| 1.9 | Armas, armaduras, catalisadores, familiares, relíquias, Ordynus | [[06 Itens e dinheiro]] | 🟡 |
| 1.10 | Mestre: gastos de Cataclisma, Eventos, adversários, terrenos | [[07 Mestre - Cataclisma, eventos e terrenos]] | 🟡 |
| 1.11 | Você responde as perguntas 🔴 de [[09 Perguntas pra decidir]] | — | ✅ rodada 1 (27/09) |
| 1.11b | Aplicar rodada 1: Palavras Básicas só verbo/substantivo, talentos de outros Comportamentos, Caderno dos Valores, itens mágicos de mecânica/Dificuldade/Valor, Marés de Valor, adversários experientes, mais terrenos | vários | ✅ |
| 1.11c | Rodada 2 respondida | [[09 Perguntas pra decidir]] | ✅ |
| 1.11d | Aplicar rodada 2: Moldura, teste por verbo + modo, Cenários e marcadores, palavra como relação, Canais (foco/arma/livro), 2 talentos por Comportamento, perguntas de criação e sessão zero, ficha por Valor, Correntes, maestrias, adversários por palavras, bestiário, tesouros, dragões | vários | ✅ |
| 1.11e | Rodada 3 de perguntas (P-70 em diante) | [[09 Perguntas pra decidir]] | ⬜ |
| 1.12 | 4 personagens prontos (nível 1) para teste | — | ⬜ |
| 1.13 | **Teste de mesa 1**: uma cena social + um combate + uma magia em dupla | — | ⬜ |
| 1.14 | Ajustes do teste; **teste 2** no nível 5 (Moldura 3) | — | ⬜ |
| 1.15 | Simulador de combate (`calc/`) para conferir que nenhum Comportamento/Valor domina | — | ⬜ |

**Pronto quando:** uma pessoa que nunca viu o jogo cria um personagem em até 20 minutos com a ficha, e um combate de 4 jogadores contra um encontro padrão termina em 45–60 minutos sem o mestre abrir o Transcrito mais de 3 vezes.

Perguntas de teste para a mesa (anotar depois de cada sessão):
- Alguém ficou parado esperando muito tempo? (holofote)
- Arcano sobrou ou faltou? Cataclisma encheu? (economia)
- Alguém travou pensando em que palavras usar? (magia)
- Alguma magia resolveu o Cenário sozinha? (Grau/limites)
- Os marcadores (até a próxima Falha, até o mestre pagar) ficaram claros sem turno?
- Os jogadores acharam fácil escolher verbo + modo?
- Quem só usou arma se sentiu menor que quem usou magia?

---

## Fase 2 — Lore e descrições

Objetivo: o mundo explica as regras (por que par é Arcano, por que Eternyr é difícil, por que Dominum pode reagir).

| # | Tarefa | Status |
|---|---|---|
| 2.1 | Cosmogonia: Arcanoclisma, a Cisão, os Primordiais | 🟡 rascunho em [[08 Mundo e entidades]] |
| 2.2 | Um Primordial e ao menos um semideus por Valor | 🟡 |
| 2.2b | Reinos (9 de Valor + Arcano + Cataclisma), povos, culturas, estruturas — só lore de criação, sem história fechada | 🟡 rascunho em [[08 Mundo e entidades]] |
| 2.3 | As Eras jogáveis (antes da magia, clássica, tecnomaga, escola de magia) com o ajuste de regra de cada uma | 🟡 |
| 2.4 | Sociedade: como gente comum vê os magos, dinheiro (Ordynus), lugares | ⬜ |
| 2.5 | Texto de ambientação de cada Valor e Comportamento (1 parágrafo + 3 frases de exemplo de jogo) | ⬜ |
| 2.6 | Nomes definitivos (Cataclisma do Kairvahnn, criaturas, ordens) | ⬜ |
| 2.7 | Kit de peças para campanha (lugares, entidades, artefatos) em vez de uma história pronta: cada mesa monta a sua | ⬜ |

As perguntas de escritor estão no fim de [[08 Mundo e entidades]]. Responda como quiser, até em áudio transcrito; eu transformo em texto.

---

## Fase 3 — O que vai ser gerado

Objetivo: fechar a **lista de coisas** que o Transcrito e o app vão produzir, com o formato de dados de cada uma. **Cartas ficam para o fim** (fase 5, último passo): primeiro definimos tudo.

| Tipo | Onde está o rascunho | Vai para o gerador? | Imagem? |
|---|---|---|---|
| Talentos de Comportamento | [[04 Comportamentos]] | ficha | não (ícone) |
| Ressonâncias de Valor | [[03 Valores]] | ficha | não (símbolo do Valor) |
| Palavras (9 × 36 + Básicas) | [[03 Valores]], [[02 Magia]] | ficha + sorteio de magia | não |
| Armas, armaduras, montarias | [[06 Itens e dinheiro]] | ficha (loja) + tesouro | sim |
| Itens mecânicos (5 famílias) | [[06 Itens e dinheiro]] | ficha + tesouro | sim |
| Itens narrativos, Maravilhas, comuns, bugigangas | [[12 Tesouros e itens mágicos]] | tesouro | sim (os mágicos) |
| Familiares, materiais, consumíveis | [[06 Itens e dinheiro]] | ficha + tesouro | sim |
| Terrenos, cidades, Reinos | [[07 Mestre - Cataclisma, eventos e terrenos]], [[08 Mundo e entidades]] | gerador de lugares | sim |
| Adversários (palavras) | [[10 Adversários - como criar]], [[11 Bestiário]] | gerador de encontros, balanceado | sim (só no gerador) |
| Dragões e wyverns | [[08 Mundo e entidades]], [[11 Bestiário]] | gerador | sim |
| Eventos, Marés, Correntes | [[07 Mestre - Cataclisma, eventos e terrenos]] | gerador | não |
| PNJs sociais, nomes, ganchos | — | gerador | opcional |

Formato de dados (proposta): um JSON por tipo em `dados/`, com o mesmo campo `moldura` em tudo, para o gerador escolher e balancear sozinho.

## Fase 4 — Arte

| # | Tarefa |
|---|---|
| 4.1 | Guia visual: paleta de cada Valor, estilo (proposta: pintura com contorno, fundo neutro, cor do Valor como luz de recorte) |
| 4.2 | 9 símbolos abstratos dos Valores (vetor, para ficha e cartas) |
| 4.3 | Capa do Transcrito e das fichas |
| 4.4 | Lista de prompts, um por item/terreno/adversário (igual ao `prompts das Peças novas.md`), com id = nome do arquivo |
| 4.5 | Geração em lote (Canva), recorte de fundo, conferência uma a uma |
| 4.6 | Molduras e ornamentos da ficha de papel |

---

## Fase 5 — Fichas, geradores, Transcrito, site

Entregas, na mesma família do Peso do Céu:

1. **App** (um HTML só, funciona offline, instalável no celular):
   - **Ficha digital**: tudo que o personagem liberou aparece nela; contadores de Arcano/Cansaço/PV/Proteção clicáveis; montador de magia (escolhe palavras → mostra Grau, custo, dados, Dificuldade, e rola os dados contando pares/ímpares); loja em Ordynus; subir de nível guiado; guardar/importar/exportar.
   - **Escudo do mestre digital**: contador de Cataclisma com Transbordo, tabela de Eventos, holofote.
   - **Geradores**: encontro (balanceado pela Moldura e nº de jogadores), adversário com imagem, terreno, tesouro/item, relíquia com palavra, PNJ, nome, gancho, Evento de Cataclisma, "magia aleatória" (d66 nas tabelas).
2. **Ficha de papel** enfeitada para imprimir, em **duas folhas** ([[05 Personagem e níveis]]):
   - **Ficha 1 — Personagem** (igual para todos): nome, skin, Comportamento, atributos, Palavras Básicas, Arcano, Laço e Passos, Vivências, itens, Ordynus, perguntas de criação e relações da party.
   - **Ficha 2 — Valor** (9 versões, uma por Valor, com arte própria): símbolo, Compromisso, Skill, Drawback, Ressonâncias, as 36 palavras, 4 espaços extras, Canal, tabela de Graus e magias favoritas.
   - Opcional: folha de Familiar/montaria.
3. **Transcrito** (no lugar de "livro"): o texto completo do jogo, com capa, feito para sair em **vários formatos** a partir da mesma fonte:
   - PDF completo para baixar e imprimir;
   - páginas no site, com busca;
   - leitura dentro do app (cada regra abre a partir da ficha);
   - Transcritos menores separados: do jogador, do mestre, de um Valor, de uma Era, do bestiário.
   E o **escudo do mestre** (PDF).
4. **Site** em um **Netlify novo** (separado do Peso do Céu): página de apresentação com capa, documentos para baixar, botão para instalar o app, galeria de imagens.
5. `calc/conferir.py`: confere antes de publicar que todo item/adversário está dentro da faixa da sua Moldura (o "balanceador" que o gerador usa).
