# 22 — O que é gerado (Fase 3)

> A lista fechada de **tudo que o livro, as fichas e o app produzem**, com o formato de dados de cada coisa. Regra de ouro: **o texto e os números moram num lugar só**. Hoje são os arquivos `.md`; na Fase 5 viram arquivos de dados em `dados/` (um JSON por tipo), e o livro, as fichas, o app, o gerador e as cartas saem deles. **Cartas ficam para o fim.**

## Campos que todo objeto tem

| Campo | O que é |
|---|---|
| `id` | nome curto, sem acento e sem espaço (`kaelen-morvath`, `luva-de-forja`); é também o nome do arquivo de imagem |
| `nome` | como aparece no livro |
| `tipo` | um dos tipos abaixo |
| `moldura` | 1 a 4 (0 = serve para qualquer Moldura); é por ela que o gerador balanceia |
| `valor` | Valor(es) ligados, se tiver |
| `tags` | palavras para filtrar (lugar, povo, papel, Era, tom) |
| `texto` | a descrição (Markdown curto) |
| `prompt` | id do prompt de arte, se tiver imagem ([[20 Prompts de arte - personagens]]) |
| `fonte` | arquivo de onde veio (para conferir) |

## O catálogo

| # | Tipo | Quantos hoje | Fonte | Campos próprios | Vai para | Imagem |
|---|---|---|---|---|---|---|
| 1 | **Palavra de Valor** | 324 (9 × 36) | [[03 Valores]] | classe (verbo, substantivo, adjetivo), d66 | ficha do Valor, montador de magia, magia aleatória | não |
| 2 | **Palavra Básica** | 6 atributos + 48 verbos + 6 coisas + 12 formas | [[01 Regras centrais]], [[02 Magia]] | atributo da coluna, d6/d8 | ficha, montador de magia | não |
| 3 | **Valor** | 9 | [[03 Valores]] | Compromisso, Skill, Retaliação, Ressonâncias, oposto, falas, *Como é*, tabela de usos, cor, símbolo | ficha do Valor, livro | símbolo + arte |
| 4 | **Comportamento** | 12 | [[04 Comportamentos]] | números base, Talento e Hábito (Básico, Superior), falas, *Como é* | ficha, criador de personagem | ícone |
| 5 | **Personagem pronto** | 6 (kit + sessões) | [[13 Teste de mesa 1]], [[16 Sessão de teste - Castian]], [[18 Sessão de teste 2 - Alvenyr]] | ficha inteira | app (carregar e jogar), livro | sim |
| 6 | **PNJ / adversário** | 288 | [[11 Bestiário]], [[10 Adversários - como criar]] | Moldura, Papel, Maestria, Ser, Agir (com determinador), Parecer, Quer, Jeito, Básicas, Valor | gerador de encontro, gerador de PNJ, livro | todos no app do gerador; no livro, os relevantes |
| 7 | **Arma / armadura** | 15 + 3 | [[06 Itens e dinheiro]] | atributo, verbo, alcance, dano, Proficiência, traços, Limiares, Proteção, preço | ficha (loja), tesouro | sim |
| 8 | **Item mágico** (5 famílias) | ~30 | [[06 Itens e dinheiro]] | família, Moldura, efeito, cargas, **preço pela fórmula** (Base × fator) | ficha, tesouro, loja | sim |
| 9 | **Item narrativo, comum, bugiganga** | tabelas d66 | [[12 Tesouros e itens mágicos]] | descrição, d66 | tesouro | só os mágicos |
| 10 | **Consumível, material, montaria, familiar** | ~20 | [[06 Itens e dinheiro]] | efeito, preço | ficha, tesouro | sim |
| 11 | **Artefato** | 8 sementes | [[21 Kit de campanha]] | palavra inicial, como cresce, rumor | tesouro raro, Marcos | sim |
| 12 | **Lugar** | 22 | [[21 Kit de campanha]] | esboço, **sentimento**, dualidade, como trata a magia, ganchos, gente | gerador de lugar, livro | sim |
| 13 | **Terreno** | tabela | [[07 Mestre - Cataclisma, abalos e terrenos]] | Moldura, tipo, Valor, pote de Ameaça | gerador de terreno | sim |
| 14 | **Reino** (plano) | 9 | [[08 Mundo e entidades]] | Valor, quem vive, desafio, benefício | livro, gerador de lugar | sim |
| 15 | **Povo** | 22 + Innhomus | [[08 Mundo e entidades]] | esboço, pesa em (aparência, magia) | criador de personagem, gerador de PNJ | sim |
| 16 | **Entidade** (Primordial, semideus, dragão, Innhomus) | 9 + 9 + dragões | [[08 Mundo e entidades]], [[11 Bestiário]] | o que quer agora (d8), personalidade | gerador de gancho, livro | sim |
| 17 | **Dragão** (gerador) | 9 tipos | [[08 Mundo e entidades]] | Valor, Desejo, Domínio, pessoas preferidas, sopro | gerador de dragão | sim |
| 18 | **Gancho** | 20 + os de cada lugar | [[21 Kit de campanha]] | d20, lugar | gerador de gancho | não |
| 19 | **Arco** | 6 | [[21 Kit de campanha]] | a pergunta | gerador de campanha | não |
| 20 | **Abalo, Maré, Corrente** | tabelas | [[07 Mestre - Cataclisma, abalos e terrenos]] | tamanho, custo | escudo do mestre, gerador | não |
| 21 | **Efeito Colateral** | 20 | [[02 Magia]] | d20 | montador de magia (rola sozinho) | não |
| 22 | **Versões de lore** | d8 sociedade, d6 despertar, d4 Khairvhaan... | [[08 Mundo e entidades]] | dado, texto | gerador de lugar e de campanha | não |
| 23 | **Marcos** (loja) | 8 linhas | [[19 Fim de sessão - as perguntas de envolvimento]] | custo, limite | ficha (fim de aventura) | não |
| 24 | **Prompt de arte** | 10 personagens + 1 por objeto com imagem | [[20 Prompts de arte - personagens]] | estilo base, cena, elemento da página que toca | geração de imagem em lote | — |

## Os geradores (o que o app faz com isso)

| Gerador | Entra | Sai | Balanceia por |
|---|---|---|---|
| **Encontro** | Moldura, nº de jogadores, tamanho (escaramuça, combate, batalha), dificuldade | adversários do bestiário que somam o orçamento, com melhorias | régua e orçamento de [[10 Adversários - como criar]] |
| **PNJ** | Moldura, papel, lugar ou povo | ficha curta (Ser, Quer, Básicas, Valor, Jeito) | Dificuldade = 11 + Moldura |
| **Lugar** | nada, ou um sentimento | lugar do kit ou um novo: dualidade + sentimento + como trata a magia (d8) + 3 ganchos | — |
| **Tesouro** | Moldura | Ordynus, item mágico e bugigangas | preço pela fórmula (Base × fator) |
| **Magia aleatória** | Valor, Grau | palavras sorteadas (d66) + verbo básico + forma, com custo e dados | tabela de Graus |
| **Gancho e arco** | lugar, Laço de um jogador | gancho que puxa o Laço | — |
| **Dragão** | Valor | Desejo, Domínio, pessoas preferidas, ficha pela idade | Moldura da idade |
| **Nome** | povo ou cultura | nome e epíteto ([[23 Nomes e Laços]]) | — |
| **Campanha em 15 minutos** | nada | 2–3 lugares, uma entidade e o que ela quer, um arco, um gancho | — |

`calc/conferir.py` (Fase 5) roda antes de publicar: todo item e adversário dentro da faixa da sua Moldura, todo preço pela fórmula, toda palavra de Valor existindo na tabela, nenhum nome repetido.

## Exemplo de dado (adversário)

```json
{
  "id": "kaelen-morvath",
  "nome": "Kaelen Morvath, o Invicto",
  "tipo": "adversario",
  "moldura": 3,
  "papel": "Chefe",
  "valor": ["Conflitüo", "Dominum"],
  "maestria": ["Resiliente"],
  "ser": ["Ex-prodígio", "Duelista"],
  "agir": [
    {"verbo": "Lâmina", "ataque": true},
    {"verbo": "Coleira", "custo": "2C", "alcance": "Longe", "alvo": "1", "resistencia": "Firmar", "efeito": "puxado até o Toque + Preso", "marcador": "Emoldura"}
  ],
  "parecer": ["Duro", "Astuto"],
  "quer": "um duelo digno, com plateia; que a escola diga o nome dele de novo",
  "jeito": "luta limpo enquanto o oponente luta limpo; com metade dos PV, oferece o lugar de 213º",
  "basicas": {"fortes": ["Bater", "Ameaçar", "Chamar"], "fraco": "Intuir"},
  "palavras": ["Lâmina", "Implacável", "Corrente", "Coleira"],
  "tags": ["Confluência", "GrandGaleonis", "Chefe", "Arena Velha"],
  "prompt": "kaelen-invicto",
  "fonte": "18 Sessão de teste 2 - Alvenyr.md"
}
```

## Decisões pendentes da Fase 3

- **P-123** ✅ **Gerador de nomes**: tabelas por cultura e por povo, mais epítetos, em [[23 Nomes e Laços]].
- **P-124** ✅ **Personagem aleatório**: botão "role tudo"; tabelas de Laços (d12 relação, atitude, sentimento) em [[23 Nomes e Laços]].
- **P-125** ✅ **Imagens**: **todos** os do bestiário ganham imagem **no app do gerador**; no **livro**, só os personagens relevantes, e aqui e ali um ao lado da ficha de PNJ.
- **P-126** ✅ ids em português sem acento.
