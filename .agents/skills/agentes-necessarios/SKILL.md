---
name: agentes-necessarios
description: Coordenar o trabalho por etapas quando o autor disser quero os agentes necessários ou pedido equivalente, selecionando Carlos, Enzo, Copião, Vuco vuco e Arquiteto conforme a tarefa.
---

# Agentes necessários

O comando “quero os agentes necessários” e variações como “chame os agentes”, “use a equipe” ou “ative os agentes necessários” acionam esta coordenação. Ler `C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo/equipe.json` e as definições pertinentes em `definicoes/`. Selecionar somente os agentes necessários ao pedido; nomes de agentes representam funções e skills, não processos independentes já executando.

Trabalhar em etapas com arquivos de entrega e estado de execução:

1. **Briefing:** identificar o objetivo, projeto e escopo. Para imagens de jogo, Vuco vuco sempre confirma projeto, elementos com movimento e como se movem. Se o comando vier sem tarefa, perguntar qual trabalho o autor quer realizar.
2. **Planejamento:** registrar em JSON as etapas, agentes, entradas, saídas e dependências. Enzo consulta referências disponíveis. Identificar ferramentas locais e remotas; não anunciar uma capacidade offline ainda inexistente.
3. **Produção:** Carlos cria as artes; Vuco vuco produz variantes de movimento quando solicitadas; Arquiteto edita ou combina imagens quando isso fizer parte do pedido. Seguir a direção de arte e preservar versões aprovadas.
4. **Ligação com o jogo:** quando necessária, coordenar comandos/estados com Claude pelo protocolo disponível e preservar os vínculos. Não inventar uma conexão com Claude nem integração confirmada.
5. **Avaliação:** ao atingir a meta definida pelo autor, apresentar entregas e perguntar aprovar, refazer ou parar. Se pedir refazer sem explicar, perguntar o motivo e aguardar. Só retornar às etapas afetadas.
6. **Preservação:** Enzo arquiva entregas e decisões durante o trabalho. Na aprovação final ou encerramento, Copião cria o pacote versionado de skills, procedimentos, código e ferramentas realmente usados.

Executar etapas dependentes em ordem. Não permitir dois agentes editando o mesmo arquivo. Registrar pendências e impedir avanço dependente quando faltar uma resposta obrigatória. As automações de Carlos e Enzo devem respeitar a etapa atual e a espera por aprovação. Nunca fazer push sem autorização.

Para portar o fluxo a outra IA, entregar `equipe.json`, os SKILL.md pertinentes, scripts e manifestos. Esses arquivos ensinam o procedimento por instruções e exemplos executáveis; não são treinamento de pesos de um modelo. Código remoto exige uma alternativa local para funcionar offline.

Planejador local disponível: `node "C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo/scripts/planejar_agentes.mjs" pedido.json`. O pedido aceita `projeto`, `objetivo`, `criarImagens`, `imagensParaJogo`, `movimentos`, `briefingConfirmadoPeloAutor`, `editarImagens`, `combinarImagens`, `integrarComClaude` e `encerrarProjeto`. Ele produz um plano JSON e identifica pendências; não executa os agentes ou ferramentas. Ainda não foi executado/testado.
