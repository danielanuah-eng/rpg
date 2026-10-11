# 011 — Cinco cenários Blender, revisão 03

Entrega atual: **`entregas/mapa-blender/v03/`**, em 10/10/2026. Guia: `LEIA-ME-CLAUDE.md`; índice: `manifesto.json`; conferência real: `validacao.json`. As versões anteriores permanecem preservadas.

O autor pediu nova revisão porque os mapas pareciam iguais e os props ficavam juntos. As sete composições agora têm relevo e ocupação próprios: três Bosques com clareiras/encostas diferentes, Deserto com dunas e oásis lateral, Montanha rochosa com passo entre maciços, Neve com vale diagonal e três abetos afastados, Vulcão com fenda e três poças de lava separadas. A Montanha mantém o ID `pedreira` por compatibilidade.

Os props principais ocupam os quatro setores, separados da loja central por raio mínimo de 2 m entre pivôs. A área central e oito casas adjacentes são reservadas; lago principal com apoio plano de 3×3 casas. Cada composição tem nove moedas deitadas, 0,5 m de margem dos props internos e bases conferidas contra o piso real. Os JSONs `manifestos/*-distribuicao.json` entregam alturas, posições Blender/Unity, escala e rotação de cada colocação.

A primeira revisão visual do DeepSeek confirmou designs distintos, mas apontou cerca solta, detalhes externos e concentração de abetos. Codex removeu a geometria decorativa incorporada de todos os entornos, abriu a Neve e completou nove moedas por cena. A reconferência confirmou bordas limpas e três abetos separados; o possível encosto do abeto inferior em uma parede foi corrigido deslocando-o para uma clareira plana de 3×3 casas. A folga das árvores em relação a terraços mais altos passou a ser medida na validação da Neve. Registros em `fontes/revisao-visual-deepseek.log` e `revisao-visual-final-deepseek.log`; a última revisão antecede esse deslocamento final.

## Arquivos e conferência

Sete fontes Blender com texturas incorporadas, sete renders e duas pranchas comparativas: `previews/bosque-tres-versoes.png` e `quatro-cenarios.png`. Os scripts reproduzem os layouts e conferem todas as fontes.

O conjunto tem **176 modelos por formato**: **161 FBX/GLB reutilizados de `v02/` e 15 entornos limpos/reexportados em `v03/mapa/`**, nos tamanhos 12/16/18 por tema. As 40 texturas continuam compartilhadas e comparadas com os originais. Usar os caminhos indicados nos manifestos da revisão 03; não copiar os entornos antigos com a cerca incorporada.

**176/176 FBX reimportados e 176/176 GLB conferidos**. Escala/eixos, pivôs, UV, hashes, tris, vento, filtros, sockets e vão dos entornos aprovados. As sete fontes são reabertas para conferir apoio, margem, loja, moedas e distribuição por setor. Os 21 pares de composições têm diferenças reais de altura e ocupação; não há layout duplicado. Detalhes e números definitivos em `validacao.json`.

## Integração a cargo do Claude

- Adaptar MapGen aos conceitos e exemplos da revisão 03: variar relevo e posição dos marcos por bioma, com grupos menores e áreas abertas. Evitar o mesmo anel de props ao redor da loja em todos os mapas.
- Manter a loja central em qualquer tema/tamanho e calcular altura pelo piso real. Nas fontes o pivô é `(0, 0.5, 0)` Unity, com orientações diferentes conforme o tema. Conciliar ocupação aproximada de 2×2 m, circulação e interação do vendedor.
- Instanciar moedas pelo sistema existente; os exemplos não criam regras de recompensa/coleta. Preservar apoio, margem e espaço entre os grupos.
- Usar entornos da revisão 03 sem cercas ou detalhes nas pontas. Árvores continuam específicas de cada bioma, conforme a revisão 02.
- Integrar shader, água e efeitos: FBX usa a primeira cor de vértice para vento; GLB usa COLOR_1 e COLOR_0 neutro. Os entornos limpos não usam vento. Sockets de fogo/fumaça e materiais emissivos dos demais modelos permanecem disponíveis.

Estado: **revisão 03 entregue para avaliação visual do autor; integração e testes no Unity ainda pendentes do Claude**. Nenhum código ou Asset do jogo foi alterado por esta entrega. Push dos arquivos não confirma deploy ou integração no site.
