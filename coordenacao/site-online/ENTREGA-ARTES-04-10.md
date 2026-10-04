# Entrega visual para Claude — 04/10/2026

## Atualização posterior: 176/210

O lote e a galeria agora têm 176 artes concluídas, com 34 pendentes. Consultar o mapa corrente em `lote-site-v1.visual.json`, não o número histórico da entrega inicial de conjuradores abaixo. Entraram Moça que Não Sangra, Rosto sem Gente, Leitura de Sombras, Sentença do Selo, Luz da Faroleira, Chamado da Matilha, Golpe do Quebra-Escudos e Nuvem Triste. As 151 do pedido inicial estão entregues; as adicionais pertencem à mesma meta de 210. Cada ID tem cena e registro próprios, com dimensões e alfa medidos. Os Valores atuais determinam a cor da magia; aparência física e roupas dos personagens aprovados continuam preservadas. Os gestos e cenários não adicionam efeitos ao jogo.

Rosto sem Gente recebeu revisão de cabeça para acompanhar o portal; o estudo em `variantes/` fica como histórico. Protegido 1 da Moça é custo adicional para escolhê-la como alvo, sem reflexão. Leitura de Sombras escolhe duas entre quatro; Sentença destrói um único Aliado ou Mística. Faroleira cura o próprio Conjurador e compra uma; Matilha atinge só os Aliados adversários. Quebra-Escudos mantém rosto, martelo e impacto alinhados e não atinge Conjurador. Nuvem Triste conserva o corpo coral, mas o efeito atual de Sublimeyn é prata-mercúrio: devolução de uma criatura intacta e compra de uma carta.

Luz de Velório permanece vela de energia sem anatomia humana. Carcaça não recebe proteção por estar em postura firme. O Esquecido conserva três formas visuais como identidade de uma única carta, sem criar três Aliados. Ossário tem quatro patas e duas asas, com ambos os braços dianteiros presentes; não é wyvern. Ossário e Embalsamador trazem um único aliado de cemitério, ainda pagando o custo indicado no texto. As transparências dos seres etéreos são visuais dentro de PNGs com fundo opaco medido.

**Corrigir mapeamento das Luvas de Força:** o briefing usava `cordao-de-rhu`, que é um colar. O novo `item-extra-luvas.png` mostra de fato luvas, inspiradas só nos materiais do cordão. A habilidade de entender lobos não foi transferida. Use o novo caminho do mapa para as Luvas, sem alterar o Cordão de Rhu. Estes oito equipamentos são genéricos sem Valores conforme os dados atuais; suas cores físicas não atribuem afinidades. Custos de ativação, dano próprio e uso único continuam nos textos do Claude. Ovo mostra uma única wyvern com duas pernas e duas asas, sem braços adicionais.

Última Flecha: rosto, arco e flecha reta miram o mesmo alvo. Maré de Sobras atinge aliados dos dois lados, não protege os próprios. Mapa do Reino usa a regra atual de escolher duas entre cinco, sem repetir a descrição antiga que escolhia uma. Coroa Riscada ainda exige descarte de uma carta do Grau. Espelho é Canal / Foco Arcano genérico, sem Valores ou ataque; os cinco rostos coloridos pertencem à identidade do objeto e não são afinidades de jogo.

Cemitério que Respira mostra um único aliado retornando, ainda sujeito ao pagamento do custo; O Que Voltou aparece em pose nova, voltado para o conjurador. Cadeado Ciumento usa Dominum atual: recuperar uma carta que anula outra neste turno, banida ao resolver, e comprar uma carta. Não interpretar a aparência do cadeado como anulação permanente. Ninho de Estrelas usa Sublimeyn, Âncora do Mesmo Dia usa Cognitium e as raridades do catálogo RPG não substituem o briefing do TCG.

A Ceifadora passou por revisão mínima de rosto e pupilas para mirar o alvo da foice. Usar `col-ceifadora-da-peste.png` do mapa, não o estudo anterior em `variantes/`. O Ilusionista mostra uma criatura intacta atravessando um portal, enquanto a Ceifadora mostra um único alvo se dissipando. Protegido 2 do Guarda-costas continua sendo pagamento adicional de Arcano para escolhê-lo como alvo, sem imunidade ou reflexão. Curandeira: Valor atual Primitivum, cura pontual de 2 de dano em um alvo.

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

