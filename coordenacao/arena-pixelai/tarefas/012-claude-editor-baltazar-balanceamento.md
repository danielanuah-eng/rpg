# 012 — Baltazar, balanceamento e aba Edição de jogo

De: Codex, por pedido direto do autor em 10/10/2026.
Para: Claude, responsável pela programação de ArenaTaticaUnity.
Estado: recebida e parcialmente implementada, conforme respostas/012-claude-editor-baltazar.md. Baltazar, edição de valores/mapas e props confirmados; catálogo visual ainda pendente. Câmera local confirmada na resposta 013. Arena ainda não encerrada.
Arquivos de integração pertencem ao Claude. Codex trabalha somente em coordenacao/arena-pixelai/.

Leia a direção em ../notas/2026-10-10_foco-heroicos-mapas-editor.codex.md. Esta demanda é da Arena Tática, sem mudar Arcanoclisma.

## Baltazar primeiro

Adicionar Baltazar, draconato paladino com kevlar prateado e martelo de guerra; tanque/suporte de boa vida e dano moderado. Codex entrega oito vistas e animações, com anjo e demônio em sprites separados. Ficha numérica e especificação de aura em ../entregas/baltazar-2026-10-10/ficha-proposta.json; manifestos indicam os arquivos prontos e os estados de revisão.

A aura custa um ponto por turno em que for mantida, tem alcance de duas casas, cura pouco aliados e fere pouco inimigos. Definir e mostrar qual recurso é usado. Vincular efeitos à resolução lógica, nunca a colisão de cada frame dos acompanhantes. O anjo/demônio deve desaparecer ou encerrar a órbita quando a habilidade desligar ou Baltazar cair.

## Crítico e armadura — prioridade de balanceamento

**Escopo confirmado pelo autor: futuro PvEvP/PvPvE, depois da Arena Tática.** Não alterar crítico, Cataclisma ou armadura da Arena por esta tarefa. Veja ../entregas/baltazar-2026-10-10/balanceamento-proposta.md. Quando o projeto futuro começar, ler os documentos em KrosArcano e reduzir o domínio de crítico e armadura para recuperar diversidade de builds, com configuração central e testes de ataques fracos, médios e fortes; registrar valores anteriores/novos e resultados.

## Edição de jogo — etapa de fechamento da Arena

Criar aba acessível no próprio jogo com catálogo de personagens, mapas, props e efeitos. Expor movimento, vida, alcance e dano de cada personagem; permitir salvar, restaurar padrão e carregar configurações. Mostrar unidade de alcance/movimento em casas. Validar limites e exibir alterações de maneira compreensível. Em multiplayer, configurações da partida precisam ser compartilhadas entre todos, sob controle da autoridade da sala; câmera e preferências de visualização são locais.

Todos os assets antigos entregues e os novos entram no catálogo, com preview, ID, versão e estados separados: entregue, importado, disponível e pendente. Catálogo preparado pelo Codex: ../entregas/editor-jogo-2026-10-10/catalogo.json. Uma entrega nova deve aparecer no catálogo da aba após importação, sem editar uma lista fixa a cada item. Assets ainda sem implementação podem ser inspecionados, mas não declarados jogáveis. Não carregar ZIPs/fontes/backups de revisão como opções de arte.

Permitir selecionar mapas e props, editar distribuição visual compatível com o grid, escolher orientação da câmera por jogador e comparar versões visuais. Rotação da câmera não altera coordenadas, alcance, obstáculos ou turno. O mesmo cenário deve admitir quatro vistas e rotação; considerar orientação dos sprites existentes e oclusão de árvores.

## Critérios de conclusão e etapas posteriores

Confirmar em respostas/012-claude-editor-baltazar.md o recebimento, os arquivos importados, o estado de Baltazar, a aba e os testes executados. Registrar crítico/armadura como demanda futura, sem aplicar novas fórmulas à Arena. Informar pendências reais para fechar a Arena. Somente com Arena concluída, Codex inicia a conversão comparativa de TODOS os assets PixelLab para Blender e a produção PvPvE baseada em KrosArcano/Arcadia Quest. As duas etapas posteriores continuam na fila, sem início antecipado.
