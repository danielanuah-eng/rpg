# Kit visual da arena

Pedido direto do autor, 2026-10-03. Designer de Cartas e Embelezador produzem apenas assets e composição visual; Claude integra motor e rede.

- Pedido posterior de 03/10/2026: conjuradores também têm animação de ataque e bloqueio. Usar animateMotion sobre o wrapper da carta inteira, com suspensão final em 90° no plano do tabuleiro, conservando a perspectiva. Não girar apenas a ilustração. Controles de estudo visual não alteram as permissões do motor. As duas artes de Ciantia viraram propostas: ler ../cartas/propostas/ciantia-dragoes-2026-10-03.autor.md.

- Arte full art central, textos leves no rodapé e recursos na lateral. Detalhes finos decorativos em margens e cantos são desejados; não ocupar a ilustração com molduras grossas.
- Arcano gerado: esfera única com cor simples e número. Custo: anel distinto abaixo. Grau: losango. Ataque: hexágono; vida: escudo, acima das regras.
- Verso escuro aveludado com amálgama rosa e azul, dourado discreto, sem faixa atravessando o centro. Usar `raster/verso-arcano-v3.png`; preservar revisões anteriores.
- Raster recortado com alfa real; medir alfa, guardar prompts e versões. SVGs/HTML/CSS são elementos editáveis de interface.
- Raridade apenas cosmética. Verso idêntico em todas as cartas ocultas. Informações privadas só chegam ao cliente quando autorizadas pelo motor/servidor.
- Revisão v3 do Canva e modelos de itens precisam de aprovação após a prévia, conforme a ferramenta de edição. Base anterior está salva. Não fazer push sem autorização explícita desta entrega.
- Ampliar a arena horizontal e verticalmente. Conjuradores no centro, encarando um ao outro, entre os campos de aliados. Separar camadas e manter palco lógico editável.
- Itens: Canal, Armadura e Extra (nome provisório). Armadura tem defesa e vida de desgaste. Online: botão de itens; mesa física: itens sob conjurador. Claude integra as regras e permissões.
- Valor é classificação extra, pode haver vários; mostrar cor no anel de custo sem obscurecer o número. Preservar subtipos como Dragão. Raridade no centro da borda inferior, sem interferir nos textos.
- Ataque e bloqueio suspendem a carta, com sombra, escala e pouso fluido. Brilho de raridade um pouco maior, restrito a frentes autorizadas.
- Não chamar outras IAs durante produção de imagens, não usar NVIDIA nem credenciais nos arquivos. Adiar arquivamento e atividades auxiliares.
- Refinamento v4: nome, números, Valor e Grau com acabamento moderado mais visível, sem invadir a arte ou encobrir números. Lendária tem iridescência suave azul/rosa/dourado na frente autorizada.
- Temporária mostra Findante ou Repentina; outros tipos mostram Repentina quando aplicável. Manter Findante conforme o livro. Não escrever Permanente. Extra multivalor é classificação documentada, distinta do slot provisório de item.
- Suspender significa deitar a carta inteira em 90 graus. Ataque e bloqueio retornam deitados; o motor decide quando preparar e voltar à vertical.
- Personagem existente em outra carta exige nova arte, pose e cenário contextual. Nunca reutilizar a arte do conjurador em outra carta. Consultar diretrizes-arte-por-carta.json.
- Revisão v5 prevalece no acabamento: lendária arco-íris com dourado mais brilhante/visível; rara prateada/roxa; incomum herda a iridescência azul/rosa/dourado da lendária v4 com refinamento. Ícones e atributos mais adornados, centro dos números livre. Consultar acabamento-v5.json. Rascunhos atuais do Canva são v5 e aguardam aprovação.

## Acabamento lendário v6

A orientação mais recente do autor exige brilho como textura sobre a carta. Usar foil-lendaria-v6 na face pública, com textos e números acima; halo externo de 3px e borda discreta. Rara/incomum e atributos mantêm v5. Consultar acabamento-v6.json. Não aplicar a cartas ocultas.

## Reflexo lendário v7

A orientação atual do autor pede efeito mais visível e exuberante: arco-íris dourado atravessando a face de um canto ao outro. Textura v6 reutilizada com opacidade 0,96; faixa v7 em screen, sob textos e números, ciclo de 5,2s. Consultar acabamento-v7.json.

## Conjurador v8

Exibir esfera pequena e discreta, centralizada logo abaixo do nome e fora da caixa de texto, dividida com os dois símbolos de Valor completos. Tipo do conjurador = ConjuradorDef.comprometimento, com espaço próprio para descrição. Não classificar o conjurador como Extra por ter dois Valores. A lendária v7 foi aprovada pelo autor.

## Acabamento v9 e arte alternativa

Mais arco-íris/luz dourada para lendárias. Padrão de brilho obrigatório nas ilustrações lendárias ou alternativas. Rara herda lendária v7 em prata/roxo; incomum herda rara v5. Alternativa cria profundidade dentro da carta. Raridade só estética. Prompts definem raridade e arteAlternativa. Ver acabamento-v9.json e ../cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md.

## Iluminação de cenários e derivações — 03/10/2026

Direção mais recente do autor: fundos de carta têm iluminação difusa e discreta, com hierarquia semelhante à referência Altered e a paleta/traço do personagem regular. Evitar HDR, céu estourado, anime genérico, bloom por toda a paisagem e dourado extremamente resplandecente. Cenários dourados usam ocre/âmbar/bronze envelhecido; brilho forte fica focal nas magias relevantes. Não ampliar a aura da personagem para iluminar toda a cena. Variar pose, olhar, expressão, roupa e interação, preservando identidade. Após cada carta, identificar criaturas, itens, lugares e eventos realmente visíveis em elementos-derivaveis.json, como ideias para outras cartas; o Claude define a necessidade e mecânica, sem produzir novas cartas por iniciativa própria. A arte do dragão da travessia foi aprovada e deve ser preservada.
