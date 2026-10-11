# 011 — Cinco cenários Blender, revisão 02

Entrega atual: **`entregas/mapa-blender/v02/`**, em 10/10/2026. Instruções: `LEIA-ME-CLAUDE.md`; índice: `manifesto.json`; conferência: `validacao.json`. O piloto anterior está preservado na pasta acima.

O autor aprovou o aspecto do piloto com ajustes explícitos: **loja no meio independentemente do mapa; lago, pedras e árvores também pelo interior, sem cair nas pontas; árvores mais frondosas ou pinheiros, com modelos próprios por mapa; diferentes versões; moedas no chão**. A revisão segue essa direção.

## O que foi entregue

Cinco kits modulares: **Montanha rochosa, Bosque, Deserto, Neve e Vulcão**. Cada um contém 35 peças, com uma palmeira adicional no Deserto: **176 FBX e 176 GLB**. Há oito texturas PixelLab / Map de 64×64 por tema, fontes Blender com imagens incorporadas e entornos de 12/16/18 casas. O pedido posterior do autor por Montanha rochosa foi atendido reforçando relevo e afloramentos no kit de ID `pedreira`; a pasta mantém esse ID por compatibilidade. Comparação dos quatro temas adicionais em `previews/quatro-cenarios.png`.

O Bosque tem três fontes e composições: `fontes/bosque.blend` (Clareira e colina), `bosque-versao-02.blend` (Trilhas cruzadas) e `bosque-versao-03.blend` (Terraços e lagoa), com renders e arquivos de distribuição correspondentes. Os demais temas possuem sua própria fonte e render. Comparação das árvores: `previews/arvores-cinco-temas.png`.

A loja foi retirada dos entornos e permanece como módulo separado, no centro das sete composições. Árvores, lagoa e pedras grandes ficam dentro do tabuleiro; só detalhes baixos e poucas cercas permanecem no entorno. A lagoa usa reserva plana de 3×3 casas, e os props têm base apoiada na superfície real, sem ocupar a mesma casa de outros pontos principais. As moedas usam o módulo horizontal `moeda_chao`.

A revisão visual do DeepSeek confirmou loja central, distribuição interna, moedas e composições diferentes, e apontou semelhança entre os dois pinheiros e oclusão da lagoa na terceira composição. Após essa revisão, Codex remodelou a Neve como abeto de 28 ramos abertos e mantos de neve, e deslocou a lagoa da versão 03. A separação entre os footprints da loja e da lagoa é conferida numericamente nas sete fontes.

## Integração a cargo do Claude

- Usar os modelos da revisão 02 e manter a **loja central em qualquer tema e tamanho**. Calcular sua altura pelo piso central real; nas prévias o pivô é `(0, 0.5, 0)` Unity. A ocupação de aproximadamente 2×2 m precisa ser conciliada com zona central, acesso e interação do vendedor.
- Distribuir os props também no interior, em casas livres, com suporte sob a base e margem das bordas. Não reproduzir um anel de grandes props nas pontas. Os JSONs `manifestos/*-distribuicao.json` são exemplos visuais, sem substituir MapGen.
- Instanciar `moeda_chao` sobre o piso nos locais indicados pela lógica existente. As moedas das prévias não estabelecem regra de coleta ou recompensa.
- Integrar vento/material/efeitos: FBX usa a primeira cor de vértice; GLB usa **COLOR_1**, com COLOR_0 branco. Materiais emissivos e sockets de fogo/fumaça estão entregues; shader e animação no jogo continuam pendentes.

## Resultado da conferência

**176/176 FBX reimportados e 176/176 GLB conferidos.** Escala/eixos, pivôs, UV, tris, hashes, pesos de vento, filtros e sockets aprovados. Vão central dos três tamanhos de entorno por tema conferido por raycasts. Os 40 PNGs têm alfa 1 medido e bytes comparados com os originais do PixelLab.

As **sete fontes de composição** foram reabertas; loja central, margem mínima de 0,5 m dos props internos, moedas deitadas e apoio de suas bases no piso foram conferidos. Máximos: **952 tris por módulo/prop e 2.888 por entorno**.

Estado: **revisão 02 entregue para avaliação visual do autor**. Integração e testes no Unity ainda não confirmados. Nenhum código ou Asset do Unity foi alterado nesta entrega. As outras partes da tarefa 010 permanecem independentes deste cenário Blender.
