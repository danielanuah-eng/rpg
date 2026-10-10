# Direção do autor — heróis, mapas e edição de jogo

Autor: pedido direto nesta conversa, 10/10/2026. Projeto atual: Arena Tática / ArenaTaticaUnity. Responsável visual: Codex. Programação e integração: Claude.

## Prioridade e sequência

1. Priorizar Baltazar e concluir os detalhes e as pendências da Arena Tática.
2. Durante períodos sem novos comandos, continuar a produção de mapas variados, vistas rotacionadas por jogador e novos props, preservando material existente. Manter uma fila finita por execução e um registro de IDs para não gerar duplicatas. Não ocupar slots do PixelLab já usados por outra produção.
3. Na etapa de fechamento da Arena, disponibilizar uma aba Edição de jogo com catálogo de tudo que foi entregue e de futuras entregas. Incluir personagens, mapas, props e efeitos. Valores editáveis: movimento, vida, alcance (range) e dano. Codex fornece catálogo e especificações; Claude implementa a aba e confirma cada integração.
4. Somente depois da finalização da Arena Tática, produzir versões comparativas em Blender de todos os assets feitos no PixelLab. Preservar os sprites e permitir comparação e escolha, sem substituir automaticamente a arte.
5. Somente depois da finalização da Arena Tática, preparar mapas e modelos para o PvPvE inspirado em Arcadia Quest, usando os documentos locais em KrosArcano. Não iniciar essa etapa enquanto Arena estiver pendente. Nenhum PDF/arquivo do PvPvE foi interpretado nesta entrega.

## Baltazar (pedido prioritário)

Draconato paladino heroico, armadura de kevlar prateada e martelo de guerra. Boa vida, dano moderado. Anjo e demônio orbitam ao redor dele numa aura de aproximadamente duas casas; pode gastar um ponto por turno para mantê-la ligada. O contato com aliado cura um pouco e com inimigo causa um dano pequeno. O tipo de ponto, os valores e a métrica das duas casas precisam ser expressos na ficha e implementados por Claude. O movimento visual não pode multiplicar a frequência de cura/dano.

## Balanceamento autorizado agora

O autor esclareceu nesta conversa: **o rebalanceamento de crítico e armadura é para o futuro PvEvP/PvPvE**, baseado em Arcadia Quest. A implementação e a leitura dos documentos para definir os números finais esperam a conclusão da Arena Tática. Registrar agora a intenção: reduzir explosões de dano e defesa que anula builds de dano baixo, mantendo escolhas de equipamento e tanque úteis. A auditoria da Arena serve apenas para distinguir os modos; não mudar suas fórmulas, nem reintroduzir crítico no seu núcleo, nem transferir a alteração para Arcanoclisma.

## Estado real

A entrega visual anterior declara integração final pendente de Claude. Não considerar Arena finalizada apenas porque o kit de cenários ou sprites está pronto. Catálogo pronto para importação não significa disponível no jogo. Registrar aceitação, importação e testes separados.
