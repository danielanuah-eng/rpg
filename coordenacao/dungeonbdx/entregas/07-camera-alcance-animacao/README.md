# Câmera, alcance, sprites e referências

Meta: pedidos do autor para girar/aproximar o tabuleiro em PC/celular, exibir distâncias nas ações e corrigir sprites animados. Inclui o estudo dos materiais Arcadia Quest fornecidos para orientar as próximas metas.

- Botões de girar, aproximar, afastar e reset; wheel/right-drag/middle-drag no PC e pinch/twist/pan com dois dedos no touch. Reset enquadra o tabuleiro inteiro. Gestos e passagem pelo HUD cancelam o toque de ação; wheel durante clique segurado também cancela a ação ao soltar.
- Alcance numerado: movimento por caminho e ocupação; skills por distância/visão, com alvos destacados e origem do dragão nos seus ataques. Preview não gasta AP/RNG. Mensagens de ação rejeitada continuam visíveis.
- Animações de sub-sprites reais da Arena: idle, caminhada, ataque/conjuração, reação e morte, com fallback quando falta clip. Billboard/direção/pivô acompanham a câmera. Reinvocar companheiro reinicia a animação; visual removido deixa de aparecer após a morte.
- FBX nativos recebem feedback procedural leve, sem rig esquelético. Regras de combate, catálogo Arena, autenticação e saves mantêm seus contratos.

## Validação

`editmode-integrated.xml`: 286/286 testes, zero falhas/skips. Inclui 132 casos DungeonBDX e a regressão Arena. O teste novo de input usa eventos reais de MouseState para verificar zoom durante clique segurado e ausência de ação na soltura.

Três execuções do player Development verificam seis comandos salvos, replay, sete unidades com companheiro, rotação/zoom/reset, seleção após órbita, preview de movimento/skill, origem do pet, feedback de alcance rejeitado e sequência real de quadros idle→attack→idle sem mudar o estado do domínio. Cada imagem tem `.json` do combate e `.presentation.json` da apresentação. `validacao.json` registra resultados e hashes das fontes.

| Evidência | Layout |
|---|---|
| [PC / câmera](pc-rotacao.png) | 1600×900, câmera girada e zoom |
| [Retrato / movimento](celular-alcance.png) | 900×1080 efetivos no Windows; pedido de janela 900×1600 limitado pelo desktop |
| [PC / animação e skill](pc-animacao.png) | 1600×900, quadro de ataque e alcance do dragão |

O retrato é renderização Windows, não teste em Android/iOS físico. Desempenho mobile e build Android permanecem pendentes.

## Referências e próximas metas

[Estudo Arcadia Quest](../../referencias/arcadia-quest/README.md): 72 arquivos, 46 conteúdos únicos, 12 abas da planilha, manuais/campanhas/cartas, 71 PNGs de pacotes e metadados dos STL. Inventário distingue extração, classificação visual e páginas efetivamente examinadas; não declara auditoria editorial integral de todas as traduções.

Campanha ramificada, inventário/equipamentos, passivas individuais, objetivos variados e reações de monstros são prioridades fundamentadas para outras metas. A base atual de três AP, alcance por skill e caps é uma adaptação DungeonBDX explícita. Não foram importados automaticamente regras, artes comerciais ou conteúdo da expansão TCG bloqueada. Multiplayer sincronizado, convites, cloud saves, rigs/arte/trilhas finais continuam pendentes.
