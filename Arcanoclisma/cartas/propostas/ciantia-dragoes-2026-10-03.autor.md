# Para Claude: cartas de Ciantia com artes aprovadas

Pedido direto do autor em 03/10/2026. As duas artes foram aprovadas e devem ser consideradas propostas de cartas para o jogo. Este arquivo é a instrução de trabalho para Claude pelo canal Git; leitura e integração ainda não confirmadas. Implementação, balanceamento e redação final são responsabilidade do Claude.

## Dragão Transplanar

- Usar a arte `Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/dragao-da-travessia-arte-v1.png`, preservando a imagem aprovada. O nome anterior era apenas o nome do estudo visual.
- Aliado lendário, subtipo **Dragão**, exclusivo de **Ciantia desenvolvida**. O autor também o definiu como **equipamento no espaço Extra**. Representar esse vínculo na preparação/desenvolvimento de Ciantia conforme as regras; confirmar como conciliar a função de Aliado em campo com a de equipamento Extra. Não transformar silenciosamente em Aliado comum do baralho nem em um item sem a função de Aliado solicitada.
- Presença de late game: Claude define custos, Grau, estatísticas, forma de acesso, poderes/efeitos e limites compatíveis com o jogo.
- Habilidade central obrigatória: **jogar um Dragão da sua mão sem pagar seus custos, desde que tenha o Grau necessário**. Conservar a exigência de Grau. Claude define gatilho, velocidade, frequência, eventual custo de ativação e quais custos a expressão abrange na redação final.
- Dar uma **keyword de proteção**, escolhida pelo Claude entre as regras existentes; se precisar de uma nova, documentá-la antes de integrar. Não atribuir uma keyword fictícia na prévia.

## Lição do Jardim

- Usar a arte aprovada `Arcanoclisma/cartas/modelos-visuais/2026-10-03-v1/licao-do-jardim-arte-v2.png`. V1 permanece como histórico.
- **Temporária · Findante**, com identidade de feitiço. A carta é Findante; **Repentina** é a velocidade da habilidade concedida aos Dragões.
- Seus Dragões podem **se suspender para gerar 1 Arcano** em velocidade **Repentina**, como a ação das cartas de Grau mencionada pelo autor.
- **Até o fim do turno, seus Dragões contribuem para o número de Grau.** A proposta é uma contribuição numérica, mantendo os Dragões em campo; Claude define a implementação e esclarece a interação com requisitos de Valor e outras contagens de Grau.
- Claude define custo, Grau exigido, Arcano gerado pela própria carta quando enviada ao recurso, Valores e demais parâmetros. A esfera de recurso da carta não é automaticamente 1 por causa da habilidade dos Dragões.
- Confirmar a duração da habilidade de gerar Arcano: o autor explicitou “até o fim do turno” para a contribuição ao Grau, mas não explicitou a duração da concessão da primeira habilidade. Não adicionar duração permanente por suposição.

## Arte e entrega

- Manter as duas ilustrações aprovadas; textos e números são editáveis, separados dos PNGs. Prévia local atualizada em `Arcanoclisma/arena/index.html`, seção **Propostas de cartas · Ciantia**.
- Custos, Grau, ataque, vida e recurso indefinidos continuam como **—**, sem significar zero. O texto da prévia é um resumo da proposta, não redação final homologada.
- Raridade é **somente estética**. A força de late game do Dragão vem do pedido de design do autor, não de uma regra associada à raridade. Dragão: lendária; Jardim: rara mantida do estudo visual, a confirmar no briefing final.
- Usar acabamento v9, campos de raridade e `arteAlternativa` nos pedidos de imagem. Não reutilizar a arte de conjurador de Ciantia. Preservar cenários de luz difusa, ouro/bronze contido e magia focal.
- Dados estruturados deste pedido: `ciantia-dragoes-2026-10-03.autor.json`. Para registrar a integração, preencher `Arcanoclisma/cartas/entregas/ciantia-dragoes-2026-10-03.integracao.claude.json` com versão recebida, decisões, IDs/caminhos implementados, pendências e estado.

## Animação do conjurador entregue junto

Na arena de estudo, **Atacar com conjurador** e **Bloquear com conjurador** usam `ArcanoclismaCardVisual.animateMotion` sobre `.board-position.self.conjurador`, com `action: 'attack'` ou `'block'`. A carta inteira avança/recua e termina suspensa em 90° no plano do tabuleiro, antes da projeção de perspectiva. **Preparar conjurador** chama `setSuspended(wrapper, false)`. Ações repetidas durante movimento e novas ações com carta suspensa são bloqueadas na prévia; o motor do jogo continua responsável pela autorização real de ataque/bloqueio e pelos eventos de preparar.

## Publicação

O autor autorizou a publicação deste pacote em 03/10/2026. Publicar na branch `main` do repositório `danielanuah-eng/rpg`, junto das artes e do kit visual da arena. A confirmação de envio é o commit existente no remoto; recebimento e integração pelo Claude exigem o arquivo de resposta indicado acima. Esta autorização não libera pushes futuros.
