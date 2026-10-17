# 07. GPT: produção visual via NanoBanana / Gemini

- **De:** codex
- **Para:** gpt | codex
- **Status:** reatribuída a Claude em 16/10 (GPT sem crédito, parado até segunda ordem do autor)
- **Projeto / pasta de trabalho:** C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\ArenaTaticaUnity
- **Entrega:** local, pela UltraBridge em C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx

## Comando direto do autor — 16/10/2026

> GPT: a produção de novas imagens e assets visuais do DungeonBDX será toda pelo **NanoBanana, no Gemini**, substituindo o PixelLab. Quando precisar criar imagens, organize os pedidos e encaminhe ao Gemini pela **UltraBridge, seguindo a skill empresa**. Se não conseguir encaminhar ou gerar por essa ponte, entregue **todos os prompts completos necessários**, organizados por asset, prontos para o autor usar no Gemini/NanoBanana. Não substitua silenciosamente a ferramenta e não fique parado esperando a integração.

## Aplicação ao projeto existente

Leia `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\HANDOFF-CLAUDE-16-10-2026.md`. Mantêm-se Unity, perspectiva 2D isométrica, regras AQ com caps, mapas/identidades já planejados, 31 heróis, escala própria do Colosso e reaproveitamento dos sprites/golpes existentes da Arena. Claude ajusta e integra o jogo; GPT prepara pedidos, acompanha entregas e confere assets; Gemini/NanoBanana gera as novas imagens. Antigravity não é necessário. Blender, se útil, prepara referências a partir dos modelos existentes; a geração visual nova continua no NanoBanana.

Escopo visual: tiles/texturas de pisos, paredes/portas/props, fichas/cartas/molduras, botões/menus e sprites planos de heróis/monstros. Para animações, especificar frames/estados/direções necessários; não tratar uma imagem estática como animação concluída.

## Execução

1. Identifique o próximo lote necessário a partir do estado e do contrato visual com Claude. Preserve as referências aprovadas e IDs existentes; não recrie tudo só pela troca de ferramenta.
2. Abra tarefas autossuficientes `Para: gemini` em `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\tarefas\`, usando o próximo número livre e o modelo da ponte. Registre uma nota para Claude sobre o lote e os arquivos sob responsabilidade de cada executor.
3. Cada pedido/prompt deve conter: ID/nome, função no jogo, referências com caminhos completos, descrição da identidade/cenário/efeito, perspectiva/ângulo, tamanho, escala/PPU, pivô, fundo/transparência, clip, direções e frames/FPS quando aplicável, continuidade entre frames, restrições e nomes exatos de saída. Texto/estatísticas mutáveis das cartas ficam na UI Unity.
4. Destino das imagens: `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\entregas\nanobanana\<lote>\`. Cada lote inclui `manifesto.json` e resposta na pasta `respostas` da ponte. Imagens entram no jogo somente após conferência e integração do Claude.
5. **Fallback obrigatório:** se a comunicação/geração estiver indisponível, criar `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\entregas\nanobanana\PROMPTS-COMPLETOS.md` e `PEDIDOS.json`, contendo TODOS os assets identificados como necessários, um prompt completo por asset/variante/clip, referências e saída esperada. Identificar a limitação real; não afirmar envio, recebimento ou geração sem evidência. Prompts não equivalem a imagens entregues.
6. Conferir dimensões, alfa real, pivô/escala, fidelidade do personagem, continuidade e compatibilidade com o importador. Registrar estados ausentes e pendências; não prometer oito direções prontas sem arquivos válidos.

## Saída e critério de pronto

Responder atomicamente em `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\respostas\07-gpt-nanobanana-gemini.resposta.md`: rota adotada, tarefas Gemini criadas, arquivos existentes, prompts de fallback e pendências. A diretriz deve substituir PixelLab nos pedidos ativos; manter históricos como históricos. Não alterar regras/código do combate, não apagar assets existentes e não fazer commit/push de entrega parcial.
