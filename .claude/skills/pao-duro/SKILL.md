---
name: pao-duro
description: Pão Duro, o agente de economia de créditos. Use quando a conversa ficar grande (contexto acima de 40%, cerca de 400 mil tokens) ou quando o autor disser "pão duro". Copia o estado da conversa para um arquivo de passagem e continua os mesmos objetivos numa conversa nova.
---

# Pão Duro

Pedido do autor (03/10/2026): "sempre que a conversa ficar grande demais e consumir crédito, crie uma cópia dela e inicie outra conversa com os mesmos objetivos".

Cada mensagem numa conversa grande reenvia todo o histórico, então ela custa mais crédito do que uma conversa nova com um bom resumo.

## Quando agir

- Ao terminar cada tarefa grande, ou a cada 20 a 30 chamadas de ferramenta, chame `get_usage` (servidor ccd_session_mgmt). Se `context.percentUsed` for **40 ou mais**, faça a passagem.
- Também quando o autor disser "pão duro", "troca de conversa" ou algo parecido.
- Não interrompa um passo pela metade: termine a edição ou o teste em andamento e só então faça a passagem.

## Passagem

1. Escreva `Arcanoclisma/tcg/agente/PASSAGEM.md`, sobrescrevendo a anterior depois de copiá-la para `agente/passagens/PASSAGEM-<AAAA-MM-DD_HH-MM>.md`. O arquivo deve trazer:
   - **Objetivos** do autor, com as palavras dele quando forem importantes.
   - **Feito**: arquivos e decisões.
   - **Em andamento e próximo passo**, de forma concreta.
   - **Esperando o autor**: perguntas abertas.
   - **Regras que valem**: CLAUDE.md, a memória e as decisões desta conversa que ainda não estão na memória.
   - **Como verificar**: testes e comandos.
   - O caminho do transcript desta conversa, para consulta.
2. Atualize a memória com o que for duradouro.
3. Inicie a conversa nova:
   - **Há trabalho para continuar sem o autor**: crie uma tarefa agendada de uma vez (`create_scheduled_task` com `fireAt` daqui a 1 a 2 minutos e taskId `pao-duro-<data-hora>`). O prompt é: "Leia `Arcanoclisma/tcg/agente/PASSAGEM.md` e continue o trabalho a partir do próximo passo, seguindo as regras do projeto. Ao terminar, registre o resultado no mesmo arquivo." Essa tarefa roda numa conversa nova.
   - **A próxima ação depende do autor**: não abra uma conversa vazia, porque isso também gasta crédito. Diga ao autor para abrir uma conversa nova e escrever: "continue pela passagem".
4. Nesta conversa, avise em uma linha que a passagem foi feita e onde está, e pare de trabalhar aqui. A equipe de balanceamento segue a mesma regra: o estado dela já vive em `agente/EQUIPE-STATUS.md`.

## Na conversa nova

Comece lendo `agente/PASSAGEM.md` e a memória. Não peça ao autor para repetir o que já está lá.
