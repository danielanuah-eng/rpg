# Auditoria visual somente leitura — Berserk possuído, 8 direções

Projeto isolado contém apenas folhas de pixel art e métricas de assets do jogo. NÃO editar arquivos, não abrir outras pastas, não executar ou ler código do jogo. Provedor OpenRouter. Inspecionar uma vez cada uma destas 8 imagens com a ferramenta de visão:

- north.png
- north-east.png
- east.png
- south-east.png
- south.png
- south-west.png
- west.png
- north-west.png

Cada imagem tem BASE na linha 0; idle, walk, attack, hit, death nas linhas 1..5. Cada frame é desenhado no tamanho original, centralizado em célula de 192 px. Numeração começa em 1. Primeira pose/visibilidade já foi medida em Python: igual à base em todas as 40 animações, alfa/cores visíveis idênticos descontando padding. Contagem: hit 7; demais 9, em cada uma das oito direções. Dimensões das células 168/176/180 são MARGENS TRANSPARENTES; não são prova de escala variável do corpo.

Conferir só falhas concretas: hit cai/morre, idle cai, arma ou escudo trocam de lado/identidade, direção errada, mudança visível do volume do corpo, saltos de posição de todo o corpo fora de passos intencionais, placeholder/mancha opaca sobre personagem, loop idle/walk com última pose incompatível. Attack deve atacar e retornar; death deve cair e permanecer caído. Vapor negro e respingos sombrios pequenos são intencionais; não remover por gosto. Esperado: armadura negra, viseira neon vermelha, espada à direita anatômica, escudo à esquerda com olho vermelho demoníaco (pode ficar oculto de costas). Não confundir lado anatômico com lado da tela.

Ler só o resumo inicial de metricas.json se precisar. Não usar bbox de arma/fumaça para certificar volume. Não inferir FPS/tempo de Unity por essas imagens, não inventar sincronismo em runtime. Se não conseguir abrir alguma imagem, reportar ? e a imagem que faltou; não anunciar tudo verificado.

Resposta breve em português: cobertura exata, lista de defeitos por direção/ação/frame com confiança e indicação da folha. Se houver loop quebrado, dizer últimos frames e pose que espera recuperar. Se não houver defeito inequívoco, dizer explicitamente. Concluir limites/? pendentes. Sem prescrever regras do jogo nem alterar os sprites. Ajudar Codex a selecionar apenas correções necessárias. Abrir todas as oito folhas uma vez, sem varrer arquivos-fontes de centenas de frames.
