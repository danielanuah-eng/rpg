# Entrega visual para Claude — 04/10/2026

## Atualização posterior: 128/210

O lote e a galeria agora têm 128 artes concluídas, com 82 pendentes. Consultar o mapa corrente em `lote-site-v1.visual.json`, não o número histórico da entrega inicial de conjuradores abaixo. Entraram Sonhador Perdido, Juramentado, O Seu Rosto, O Ministrum, O Que Voltou, Bibliotecária do Arquivo, Repetido e Espírito-Animal. Cada ID tem cena e registro próprios, com dimensões e alfa medidos. Os Valores atuais determinam a cor da magia; aparência física e roupas dos personagens aprovados continuam preservadas. As regras vazias desses Aliados continuam vazias: eventos narrativos da arte não adicionam efeitos ao jogo.

A **Luneta do Fim** foi revista após o autor apontar cabeça/mira incoerentes e raio dobrado: arquivo corrente `Arcanoclisma/arte/cartas-site/base-dano-conjurador-c4-a745c23c.png`. A versão sem hash e o estudo intermediário são históricos, não devem prevalecer sobre o mapa. Os exemplos novos de ambientes e a coerência de mira estão em `DIRETRIZ-CENARIOS.md`. Nenhuma nova imagem de Ciantia faz parte desta publicação. A fila continua em produção; avaliação final e integração do Claude seguem pendentes.

O autor pediu o envio conjunto das artes concluídas e dos conjuradores com cenários corrigidos. Autorizou os pushes das imagens. Esta entrega não altera habilidades, custos ou regras.

## Arquivos de entrada

- Mapa dos nove conjuradores e do modelo de Ciantia: `Arcanoclisma/cartas/entregas/conjuradores-fundos-2026-10-04.visual.json`.
- Lote completo: `Arcanoclisma/cartas/entregas/lote-site-v1.visual.json`.
- Prévia dos conjuradores: `Arcanoclisma/cartas/modelos-visuais/2026-10-04-conjuradores/galeria.html`.
- Galeria geral: `Arcanoclisma/arte/cartas-site/galeria.html`.

## Diretriz atual do autor

Os retratos de conjurador do site estavam bons em traço, aparência, pose e detalhes. O pedido é preservar essa composição e acrescentar apenas um ambiente sinérgico. Use os novos caminhos exatos do mapa, inclusive o hash no nome. Eles correspondem a Lyandra, Carmal, Ricardo, Roh, Agnelo, Diástaros, Ana, Mitonho e Osmar. A figura fica dominante, com o corpo frontal ou em três-quartos frontal.

As nove cenas anteriores, que mudavam o personagem e a pose, foram guardadas por pedido do autor para cartas futuras. Imagens, prompts e registros estão preservados no catálogo `Arcanoclisma/arte/cartas-site/variantes/conjuradores-cenas-para-cartas-futuras/catalogo.json`. Não usá-las como retrato principal de conjurador.

Ciantia: **orientação posterior do autor, 04/10 — não usar nem reenviar o modelo frontal corrigido**. Essa imagem havia sido publicada no commit `dceec35` antes da nova instrução. Está retirada do uso; não integra o jogo. O prompt para uma futura cena lateral, de olhos fechados, agachada brincando com os dois dragões voltados para o jogador e livro atrás está reservado em `Arcanoclisma/expansoes/a-mestra-dos-dragoes/arte/prompts/ciantia-brincando-dimensao-transplanar.prompt.txt`. Não gerar essa revisão durante a fila prioritária.

## Estado da entrega inicial (histórico)

Há **94 de 210** artes da fila concluídas, incluindo os nove conjuradores agora revisados; **116** permanecem pendentes. O modelo de Ciantia é suplementar. Os quatro itens recém-concluídos são Couraça de placas, Poção de cura, Elixir de foco e Harpa de Vidro de Liss.

As cenas completas têm fundo opaco medido. Os PNGs são saídas nativas, sem redimensionamento. Preserve essa nitidez na carta; use `arteModo=cena` e evite tratamento fosco.

A aprovação de publicação não representa a avaliação visual final de toda a fila. Não registrar aprovação final automaticamente. Raridade é somente estética, sem definição automática no aplicativo; os acabamentos continuam disponíveis.

**A Mestra dos Dragões permanece bloqueada para ativação no jogo ou mudanças de cartas até liberação específica do autor.** A produção e publicação das imagens não liberam a expansão. Claude faz a integração; o recebimento e a integração desta revisão ainda não foram confirmados.

