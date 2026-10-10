# 005 — Integrar ritmo mais lento e suavizar interpolação
**De:** usuario via codex
**Para:** claude
**Status:** implementada por Claude (ver respostas/005-006-claude.md); conferência visual com o autor
**Responsável:** claude

## Pedido do autor
Melhorar os frames do jogo: qualidade da imagem, lerp e animações atualmente rápidas demais. Mantém-se a divisão: Claude escreve código, integra e sobe a versão; Codex confere e prepara as imagens.

## Entrega a integrar
Ler a auditoria em `auditorias/sincronia-2026-10-10/` e sua proposta de ritmo. Ponto de partida visual: reduzir o ritmo atual de 10 FPS para aproximadamente 6 FPS, com tempo ajustável por clipe; priorizar transições contínuas entre idle, movimento, ação e retorno. Sincronizar o deslocamento com os passos e usar interpolação independente do FPS, com aceleração e desaceleração suaves. Essa é uma proposta de implementação a validar no jogo, não uma alteração já executada.

Conservar filtro Point e ausência de compressão nos sprites pixel art. Melhoria de qualidade deve corrigir identidade, recortes, pivôs, resolução aparente e continuidade, sem borrar a arte nem mudar regras. Evitar normalizar escala pelo tamanho do canvas da animação: padding é diferente de tamanho do corpo. Conferir se hits interrompem o ataque antes do impacto e se eventos sobrepostos pulam os clipes.

Codex acompanha a auditoria completa, o relato do disparo south-east e dos hits do Damian, a cena animada da loja e figuras dos 10 produtos. Não editar entregas de Codex em paralelo; informar a versão escolhida e confirmar importação na resposta 005.
