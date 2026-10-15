# QA visual da prévia

Capturas reais do Canvas Unity URP em textura, sem depender do framebuffer da janela oculta. Captura padrão de janela oculta gerou imagens vazias; não serviu como prova. O caminho de QA usa câmera própria e SubmitRenderRequest, somente Editor/Development, com limpeza da textura temporária. Não é um mockup HTML nem imagem gerada.

DeepSeek conferiu 3 imagens: UI legível e sem sobreposição confirmada. Correções de revisão: breadcrumb acompanha a aba; ATIVO/INATIVO usam caracteres ASCII disponíveis na fonte; hover tem cor distinta da seleção; ações inferiores centralizam inclusive páginas com poucas skills.

Achados não confirmados: 1/4 páginas no PC versus 1/7 no retrato é correto (8 versus 4 heróis por página, total 28); as quatro cartas do retrato têm as mesmas âncoras, com elevação/escala intencional na selecionada. Margens e conteúdo do Baltazar estão íntegros na conferência direta. O rosa de um módulo INATIVO era hover/foco, sem alterar seu estado; contraste entre hover e seleção refinado.

Limites: janela Windows de retrato foi limitada pelo desktop a 900x1080; não alegar captura 900x1600 nem execução Android física. Paleta ainda provisória por falta da imagem de referência. Stress-test e interação em dispositivo físico pertencem ao QA do autor.
