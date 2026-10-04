# Interações e apresentação — pedido do autor, 03/10/2026

## Responsabilidades

Codex/GPT cria ilustrações, assets, animações e movimentos. Claude conecta movimentos ao motor, aos pagamentos e aos alvos autorizados. Usar os assets já entregues; novas imagens somente quando necessárias. Não executar tarefas de outros agentes durante a produção visual.

## Mão e jogo de uma carta

- Poder reorganizar livremente as cartas da própria mão arrastando com o mouse; preservar a ordem local e reposicionar suavemente as outras cartas. O arraste não altera informação privada do rival.
- Segurar a carta com o mouse, vê-la acompanhar o cursor com perspectiva e sombra, e soltá-la sobre o campo.
- Ao soltar no campo, abrir a opção de pagar os custos segundo as regras atuais e os recursos autorizados do jogador. O servidor/motor confirma se pode jogar; a animação não gasta recursos sozinha.
- Confirmado o pagamento/jogo, animar a carta saindo da mão e assentando no campo. Se ilegal, sem recursos, pagamento cancelado ou resposta recusada, animar o retorno à mesma posição da mão.
- Arraste de mouse e opção equivalente acessível por clique/toque; não esconder confirmação ou custos sob a carta em movimento.
- Tela cheia selecionável, com saída visível e suporte a Escape; integrar pelo código do Claude.

## Resolução e efeitos

- Cada magia deve ter asset particular de resolução ligado ao seu ID: seguir ao alvo, acontecer no local ou acompanhar entrada em campo, conforme o efeito real da carta.
- Registrar gatilho, alvo, momento de resolução, cancelamento, paleta e arquivo. Não aplicar efeito de dano a cura nem confundir conjuração com resolução.
- Ataque dos aliados continua genérico compartilhado, fluido; não criar ataques elementais exclusivos por aliado. Os efeitos particulares aqui solicitados são para a resolução/entrada apropriada da carta.
- Respeitar redução de movimento e sincronizar efeitos com os eventos já confirmados pelo motor. Não revelar cartas ocultas por imagem, cor de raridade ou animação.

## Campo e nitidez

- Perspectiva um pouco mais afastada para mostrar o espaço de jogo sem reduzir demais as cartas. Aliados e permanentes dos dois jogadores devem ficar **dentro da área azul**, abaixo de seu conjurador, com os conjuradores ao centro encarando-se.
- Mão fora da mesa. Controles/pilha à direita, contadores sobre o campo. Ajustar o piso e seus pontos de ancoragem ao conteúdo real; não deixar cartas fora dos limites pintados.
- Correção visual local de nitidez em `online/public/css/cartas-nitidez-v11.css`: reduz a película clara do cenário provisório, recupera contraste/cor e reduz a inclinação da carta inteira. Carregada após `mesa-ajuste.css`. Ainda sem conferência visual no navegador; Claude confirma integração.
- Prioridade de produção atual: revisar as 151 artes do catálogo servido pelo jogo, carta por carta, antes da expansão de 90 cartas. Cada arte será uma cena inteiramente nova, não o recorte do RPG montado sobre um fundo. Não anunciar as 151 como prontas por existir a fila.

## Publicação e expansão

O autor autorizou push automático das entregas concluídas e integração com o Claude nesta conversa. Registrar o que foi entregue e o que segue pendente. **A Mestra dos Dragões** continua bloqueada para inclusão em jogo e alteração de cartas até liberação explícita específica.

