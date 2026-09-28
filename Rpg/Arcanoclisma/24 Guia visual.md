# 24 — Guia visual (Fase 4)

> O rascunho da identidade visual: a página, o personagem, os símbolos dos Valores, as cores e as fontes. Prévia das esferas em `arte/esferas.html` (também publicada como página). Personagens das sessões em [[20 Prompts de arte - personagens]].

## Três camadas

| Camada | Referência | Como é |
|---|---|---|
| **A página** | a estética das páginas do **Daggerheart** | ornamentos finos em linha dourada, molduras discretas nos cantos, fundo com textura leve (papel ou noite), ilustrações de canto que vazam da coluna, títulos com serifa clássica; nunca copiar molduras ou elementos gráficos de lá, só a qualidade: elegância, respiro, ornamento com parcimônia |
| **O personagem** | o traço do **Wilderfeast** | tinta grossa e expressiva, contorno preto marcado, cores chapadas e vibrantes com sombra em bloco, proporções estilizadas, poses exageradas; a magia desenhada como formas gráficas que **saem do quadro e mexem na página** |
| **O símbolo** | próprio do Arcanoclisma | as **Esferas dos Valores** (abaixo) |

## A linguagem de par e ímpar

A forma conta a regra do jogo:
- **Par é fechado**: círculos, esferas, contornos completos, simetria. É o **Arcano**.
- **Ímpar é aberto**: frestas, formas quebradas, o elo que falta, o ponto que escapa, a espiral. É o **Cataclisma**.
- Toda esfera é par (fechada) e tem, dentro, figuras ímpares (algo que abre, quebra ou escapa). Nenhuma composição é perfeitamente simétrica.
- Nos ornamentos da página: cantos com molduras fechadas nas seções de regra (o que se controla) e molduras abertas, rachadas, nas seções do mestre e dos Disruptos (o que muda).

## As Esferas dos Valores

Cada Valor é uma **esfera** com uma cor só dela. Dentro, **traços e figuras geométricas** se juntam numa imagem **abstrata** que ainda lembra o Valor. Regras:

- Mesmo tamanho, mesmo brilho no alto à esquerda, mesma borda escura, mesma espessura de traço: as nove têm que parecer uma família.
- Dentro, só **traço** e **figura geométrica** (linha, círculo, triângulo, quadrado, arco, espiral). Nada de desenho figurativo detalhado.
- A figura usa a cor de **linha** do Valor; o fundo é um degradê da cor clara para a escura.
- Funciona pequena (ícone de 24 px na ficha) e grande (abertura de capítulo).

| Valor | | Esfera (clara · base · escura) | Linha | A imagem dentro |
|---|---|---|---|---|
| **Conflitüo** | a Lâmina | `#E07A55` · `#A8432E` · `#3A120B` | `#F6D2B8` | Duas lâminas em X; uma se parte no meio, e a fresta solta faíscas. O choque que faz cada coisa saber onde termina. |
| **Sublimeyn** | o Eixo | `#DDE7EE` · `#8FA3B0` · `#25323B` | `#F4F8FB` | Quadrado, gota e espiral num mesmo anel pontilhado: sólido, líquido e gás girando no ciclo da matéria. |
| **Dominum** | a Coleira | `#B4508C` · `#6E2150` · `#22081A` | `#F0C6DE` | Um anel de elos com um fecho embaixo; linhas puxam tudo para o centro. A vontade que dá dono. |
| **Persona** | o Coração | `#F7B49B` · `#E0735A` · `#4E1D14` | `#FFF1EA` | Um rosto partido: metade sorri, metade não. Embaixo, um coração feito de dois círculos e um triângulo. |
| **Alvorum** | o Olho | `#F4D98A` · `#D4A43A` · `#120E06` | `#FFF3CF` | A esfera é dia em cima e noite embaixo; no meio, um olho aberto, com raios. O que é visto não fica escondido. |
| **Morbidyum** | a Porta | `#D6EDD9` · `#8FBF98` · `#16281C` | `#F2FBF3` | Uma porta em arco: de um lado uma flor, do outro um osso. Tudo passa por ela, nos dois sentidos. |
| **Cognitium** | o Véu | `#7E84E0` · `#3C409E` · `#0E0F2E` | `#DADDFF` | Uma espiral quadrada que vira labirinto a partir de um ponto. O pensamento que dá voltas até achar saída. |
| **Eternyr** | o Sempre | `#4A5FA8` · `#1E2B5C` · `#070B1E` | `#EEF2FF` | Um anel perfeito e um ponto escapando pela tangente, entre estrelas miúdas. O tempo que continua. |
| **Primitivum** | o Uivo | `#9DBB63` · `#5E7D3A` · `#1C2710` | `#E6A43F` | Uma pegada dentro de uma folha, em âmbar. O corpo que sabe antes da cabeça. |

Vetores em `arte/simbolos/` (um SVG por Valor). Para uma versão pintada (livro, abertura de capítulo), use o SVG como base e o prompt abaixo.

**Prompt de esfera pintada** (trocar o que está entre colchetes):
> Esfera mágica perfeita, flutuando, superfície lisa com brilho suave no alto à esquerda, degradê de [cor clara] para [cor escura], dentro dela uma composição abstrata de traços finos e figuras geométricas em [cor da linha] formando [a imagem dentro], estilo emblema gráfico com textura leve de pincel, fundo transparente, sem texto.

## Cores de apoio

| Uso | Cor |
|---|---|
| Fundo escuro de página (noite) | `#15141B` |
| Placa / quadro sobre o fundo escuro | `#1E1C26` |
| Texto claro | `#ECE5D5` |
| Texto secundário | `#A69E8E` |
| Ornamento, fio dourado | `#B8955A` |
| **Arcano** (par) | dourado claro `#E8D39A` |
| **Cataclisma** (ímpar) | ferrugem profunda `#8E3B2A` |

A versão clara da página (para impressão) troca o fundo por papel quente e mantém o fio dourado; a versão preto e branco usa só linha e hachura.

## Fontes (sugestão)

| Papel | Fonte | Por quê |
|---|---|---|
| Títulos | **Marcellus SC** | versalete clássico, lembra inscrição em pedra e moeda (Ordyn) |
| Texto | **Spectral** | serifa legível em parágrafos longos, com itálico bonito para as falas |
| Números, tabelas, códigos | **IBM Plex Mono** | dados, preços, Dificuldades, ids |

## Ícones

Rascunho vetorial em `arte/icones/` (prévia em `arte/icones.html`): 12 Comportamentos, Arcano e Cataclisma. Todos em traço de mesma espessura e `currentColor`, para trocar a cor na ficha colorida e na preto e branco.

- **Comportamentos**: ícone de linha em fio dourado, dentro de um losango (não esfera, para não confundir com Valor).
- **Atributos**: não existem mais (as Palavras Básicas são soltas). Os glifos de atributo foram apagados.
- **Arcano e Cataclisma** na ficha: Arcano é um círculo fechado dourado; Cataclisma é um círculo com uma fresta, em ferrugem.

## Fichas

Rascunho das duas em `arte/fichas.html`. **O centro da Ficha 1 é a tabela de Palavras Básicas**: é ali que o jogador põe os números dele, palavra por palavra: as 52 em ordem alfabética, cada uma com a sua casa de número e uma marca de avanço, sem colunas de atributo. Esse é o diferencial do jogo, e a ficha tem que mostrar isso primeiro.


- **Ficha 1 (Personagem)**, frente: identidade, Esquiva, Limiares, PV, Proteção, Cansaço, Arcano, **Palavras Básicas** (o maior bloco), Laços, Comportamento, armas. Verso: retrato, Vivências, itens, perguntas de criação, relações da party, Marcos, notas.
- **Ficha 2 (Valor)**: uma para cada um dos **9 Valores**, **limpa**: só o que se usa na mesa. Esfera e cor do Valor, Compromisso, Skill, Retaliação e Ressonâncias, as 36 palavras, as casas de Grau máximo e de palavras, **a fórmula** (a mesma da Ficha 1), a tabela de Graus e os **Descritores** em quadrados (Dano, Restringe, Duração, Alcance, Tamanho). As explicações ficam no **livro** (`arte/livro-magia.html`, capítulo de Magia) e na **ficha do app**. Cores de texto mais escuras nos Valores claros (Sublimeyn, Alvorum, Morbidyum).

## Dragões

Traço do **Wilderfeast** (tinta grossa, contorno preto, cor chapada com sombra em bloco) com o que vem das referências de dragão do dono e da lore ([[08 Mundo e entidades]]):

- **Imponência**: visto um pouco de baixo, cabeça erguida, peito orgulhoso, envergadura enorme; majestoso até ameaçando.
- **Corpo**: nobre e alongado, pescoço e cauda longos; escamas angulosas como cristal, desenhadas em blocos de cor.
- **Paleta**: duas ou três cores do Valor e **um acento que brilha**.
- **Chamas bruxuleantes**: saem dos espinhos, da borda das asas e da ponta da cauda e **envolvem o corpo como um manto que se mexe**, com fiapos translúcidos e veios acesos nas escamas; os olhos queimam com o mesmo fogo.
- **Arcano e Cataclisma num corpo só**: parte do corpo inteira, fechada e simétrica; parte rachada, com luz vazando e forma mudando.
- **Peso emocional**: olhos fundos e expressivos (amor, medo, saudade), olhando quem vê como se os mortais fossem estações.
- **Idade**: o filhote e o jovem são feras de impulso; o adulto é senhor consciente; o ancião é memória do mundo, sábio, de cicatrizes antigas.

As imagens de referência não entram no repositório (são de outros artistas): servem só de direção.

## Página do livro

Rascunho do capítulo de Magia em `arte/livro-magia.html`: páginas de papel com fio dourado e cantos fechados (seção de regra), título em Marcellus SC, texto em duas colunas, capitular na abertura, caixas de regra com borda dourada (ferrugem quando fala de custo e Cataclisma), exemplos montados como pedaços da ficha. Sete páginas: abertura e fórmula, palavras, Graus, quem rola e contra o quê, Descritores, os dados e os Efeitos Colaterais, Canal e Dupla.
