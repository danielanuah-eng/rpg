# 010 — Conferir cobertura visual dos eventos da partida

**De:** codex, conforme pedido do autor de animar os eventos
**Para:** claude
**Status:** pendente de integração/verificação
**Responsável:** claude

Usar `entregas/eventos-cenarios/vinculos-eventos.json` para ligar as animações de cura, área, invocação e moedas aos eventos atuais. O código de apresentação já implementado pelo Claude cobre tiros, compra, moedas e mudanças de forma. Esta tarefa completa a cobertura visual do pedido, preservando as regras.

Conferência estática atual: `turno` toca som; `loja`, `rodada`, `dominio`, `eliminado` e `vitoria` ainda aparecem só no registro da apresentação. Propor feedback visível correspondente: foco suave no ativo; pulse nos produtos renovados; transição curta do contador de rodada; pulso nas casas centrais; dissolução/indicador no retrato da equipe eliminada; encerramento com luz e ícone da equipe vencedora. Não precisam de geração nova: usar ícones existentes, partículas, materiais e transições UI.

Ligar cura/invocação aos anéis novos, impacto-area às casas atingidas por área, e raio animado ao segmento ou área efetivamente resolvida. Testar ataques ranged mesmo quando um item/altura altera alcance e cada alvo secundário de área/linha. Selecionar estilo por skillId, nunca inferindo o tipo de tiro pelo tamanho da imagem.

Confirmar o tempo real do hit completo. Há uma espera fixa de 0,45 s na armadilha: o hit atual de 7 frames/8 FPS dura 0,875 s. Não avançar a fila/idle interrompendo esse clipe. Conferir casos simultâneos de hit, death e golpe em andamento de acordo com a decisão de apresentação do Claude.

Manter testes do núcleo e informar quais eventos foram conferidos na partida. Os 24 testes EditMode relatados antes da entrega não certificam essa cobertura visual nem a versão final dos assets.
