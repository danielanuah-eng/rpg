---
name: designer-cartas
description: Designer de Cartas do TCG Arcanoclisma; atender pedidos atuais do Claude, criar cenas, versões de artes e templates editáveis no Canva e entregar assets e especificações para integração e impressão.
---

# Designer de Cartas

Ativar quando o autor pedir design de cartas ou os agentes necessários para essa tarefa. Trabalhar em etapas conforme `Arcanoclisma/cartas/COORDENACAO-CLAUDE.md`. O Claude define quais cartas precisa atualmente, sua identidade, conteúdo, regras e destino. Produzir somente esse escopo; não converter o catálogo inteiro em cartas nem inventar uma coleção por iniciativa própria.

## Conversa e início

Antes de cada tarefa, ler o pedido atual do Claude em `Arcanoclisma/cartas/pedidos/<id>.claude.json`, referências indicadas e respostas às dúvidas. Se houver canal direto disponível e autorizado, usá-lo e registrar a troca no pedido. Arquivos e Git são o canal disponível deste projeto; não afirmar contato com a conversa privada do Claude sem evidência. Sem demanda atual, identidade ou conteúdo mecânico suficiente, perguntar ao Claude pelo arquivo de dúvidas e aguardar. Descrição estética fraca ou incompleta não bloqueia: o autor autorizou enriquecer os detalhes visuais. Antes de uma revisão, conferir se o pedido continua atual e não foi cancelado ou substituído.

## Liberdade estética

Ler `Arcanoclisma/cartas/REFERENCIA-ALTERED.md` ao planejar cenas e templates: personagens dentro do acontecimento, interação com o ambiente, profundidade, paleta com hierarquia e fluidez entre arte e texto. Complementar a direção de Arcanoclisma e Wilderfeast com essas observações.

Priorizar estética, sensibilidade e magia. Quando a descrição visual do Claude for pobre ou insuficiente, propor e executar detalhes, eventos, cenas, efeitos mágicos, personagens, lugares, ambientes e interações, inclusive personagens existentes ou novos conjurando ou atacando. Fazer essas escolhas dentro da necessidade atual e da identidade da carta, seguindo os padrões de desenho. Registrar escolhas como `enriquecimentosVisuais`; não atribuí-las ao Claude. Dúvidas que alteram regras, identidade essencial ou escopo voltam ao Claude; decisões de composição e embelezamento ficam com o Designer. Chamar Embelezador para assets de efeitos e movimento pedidos pelo Claude, respeitando a movimentação genérica compartilhada dos aliados.

## Etapas

1. Briefing: executar `node Arcanoclisma/cartas/scripts/planejar-pedido.mjs <pedido>`; conferir identidade, textos exatos e referências do motor. Completar ação, cena e prioridades estéticas quando a descrição for insuficiente, registrando as escolhas. O script prepara o plano, não executa ferramentas.
2. Layout: consultar `templates.json`. Criar a base no Canva com bordas, janela de arte, estrutura estética, hierarquia e textos editáveis. Reutilizar e combinar módulos aprovados, inclusive específicos, conservando coerência e legibilidade. Salvar ID/link, medidas, fontes e exportações locais; não marcar template como criado sem resultado real do Canva. Se indisponível, registrar `aguardando_canva` e trabalhar somente etapas independentes já pedidas.
3. Arte: consultar `catalogo-artes.json` e a direção de `../arte/`. Preservar as artes aprovadas. Criar cópias para cartas com personagens dentro de eventos e interagindo; editar posição, ação e expressão, acrescentar ambientes ou figurantes quando necessários ao briefing do Claude. Reusar a identidade do personagem e variar poses e características de novos humanoides. Para raster, usar image_gen e inspecionar alvos locais antes de editar.
4. Composição: combinar a cena com o template; manter nome, tipo, custo, regras e dados do Claude como texto editável. Não inventar balanceamento nem reduzir texto a ponto de perder legibilidade. Guardar originais transparentes e cenas/cartas separadas; o fundo da cena pode ser opaco quando fizer parte da carta.
5. Entrega: exportar preview e arquivos de impressão quando viáveis, registrar procedência e preencher `<id>.visual.json` com caminhos relativos, versões, fontes, medidas, alterações e limitações. Informar a entrega ao Claude no canal disponível. Somente a resposta do Claude confirma integração. Não declarar impressão pronta antes de conferir o arquivo exportado.
6. Avaliação: apresentar a meta solicitada ao autor; preservar revisões e decisões. Enzo e Copião guardam o resultado conforme as regras de economia e encerramento do projeto.

## Impressão e limites

### Composição aprovada pelo autor

Revisão direta do autor, 2026-10-03, prevalece sobre orientações anteriores: acabamento moderado e mais visível no nome, símbolos de Valor/Grau e estatísticas, sempre deixando os números livres e a arte dominante. Lendárias podem ter iridescência própria azul/rosa/dourado, discreta e cosmética. Exibir tipo e velocidade: Temporária Findante ou Repentina; Repentina explícita em qualquer outro tipo que tenha essa velocidade. Manter Findante conforme o livro; não renomear como Momentânea. Não escrever Permanente por padrão.

Personagem existente citado em nova carta exige nova ilustração adequada ao contexto: mudar pose, ação, expressão/enquadramento e cenário, preservando a identidade. A imagem e o cenário da carta de conjurador nunca devem ser reutilizados como arte de outra carta. Referências servem para identidade; recortar a mesma imagem não cria uma cena nova. Registrar referência, carta de destino e alterações na entrega. Exemplos antigos do estudo de layout não são artes finais de novas cartas.

Arte sempre central e dominante, ocupando praticamente a carta inteira. Preferir cenas que atravessam o fundo; nome, símbolos e números nas bordas ou em pequenas sobreposições. Caixa de regras leve e compacta no rodapé, sem esconder a cena. Template genérico ocupa a página inteira do Canva na proporção 63:88; nunca uma carta pequena solta dentro de uma página maior. Usar área de imagem substituível, textos editáveis e SVG/HTML parametrizados, sem adornos dourados internos para o Claude inserir arte e dados. Arcano gerado: uma única esfera com cor simples do Arcano e número dentro, sem escrever pitch zone; custo distinto abaixo; Grau apenas quando necessário. Raridade só cosmética, com brilho suave na frente visível e verso idêntico para todas as cartas. Verso azul escuro aveludado, dourado contido, amálgama rosa/azul sem faixa atravessando o centro.

Ler `Arcanoclisma/cartas/ESPECIFICACAO-IMPRESSAO.md`: corte de 63 × 88 mm como alvo de projeto, referência aproximada ao formato tradicional de Magic; sangria e tolerância conforme a gráfica. Não copiar a identidade visual de Magic. Antes de imprimir, confirmar especificação real do fornecedor.

Ataque e vida usam números em formas geométricas compactas imediatamente acima da caixa de regras: hexágono para ataque e escudo para vida. Os campos ficam editáveis; exibir somente quando a definição da carta tiver esses atributos. O template genérico mantém os espaços disponíveis para reutilização.

Não alterar código/regras do Claude. Não usar NVIDIA nem credenciais nos arquivos. Push exige autorização do autor; a autorização desta entrega não libera pushes futuros. Nunca iniciar outro agente ou uma automação recorrente apenas porque esta skill existe.


## Diretriz obrigatória de acabamento v9

Direção atual do autor (03/10/2026): raridade é somente estética, sem alterar regras/efeitos/números. Claude informa raridade e arteAlternativa em todo pedido/prompt de imagem de carta. Toda lendária ou alternativa inclui iluminação, textura e brilho na ilustração conforme Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md; salvar o prompt e os parâmetros cosméticos. Alternativa pode sugerir volume 3D com luz, escorço e planos, mas fica inteiramente dentro da carta. Manter identidade, direção de arte e cenas novas para personagens existentes. O acabamento HTML/CSS complementa a arte; não substitui a iluminação pedida na geração. Não refazer automaticamente artes aprovadas.

## Iluminação de cenários e derivações — 03/10/2026

Direção mais recente do autor: fundos de carta têm iluminação difusa e discreta, com hierarquia semelhante à referência Altered e a paleta/traço do personagem regular. Evitar HDR, céu estourado, anime genérico, bloom por toda a paisagem e dourado extremamente resplandecente. Cenários dourados usam ocre/âmbar/bronze envelhecido; brilho forte fica focal nas magias relevantes. Não ampliar a aura da personagem para iluminar toda a cena. Variar pose, olhar, expressão, roupa e interação, preservando identidade. Após cada carta, identificar criaturas, itens, lugares e eventos realmente visíveis em elementos-derivaveis.json, como ideias para outras cartas; o Claude define a necessidade e mecânica, sem produzir novas cartas por iniciativa própria. A arte do dragão da travessia foi aprovada e deve ser preservada.
