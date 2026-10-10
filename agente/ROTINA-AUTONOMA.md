# Rotina autônoma (pedido do autor, 10/10/2026)

1. Ao abrir a conversa: rodar em segundo plano `bash agente/vigia-gpt.sh`.
   Ele confere a cada 30 min (git fetch + pastas de entrega) sem gastar crédito e só termina quando chega algo do GPT/Codex.
2. Quando o vigia terminar: integrar a entrega (agente integrador-visual) e religar o vigia.
3. Sem nada aprovado ou pedido: seguir programação — organizar código, integrar assets pendentes,
   limpar imagens, construir mecânicas genéricas no motor. Testes + build sempre; sem partidas entre bots.
4. Contexto ≥ 40%: skill pao-duro (PASSAGEM.md) e conversa nova, religando o vigia.
5. Push: perguntar ao autor (exceto entregas de imagem já autorizadas).

## Feito
- 10/10: testes do online atualizados para 36 Conjuradores (9 + 27 da expansão dos dragões). 28/28 passam.
