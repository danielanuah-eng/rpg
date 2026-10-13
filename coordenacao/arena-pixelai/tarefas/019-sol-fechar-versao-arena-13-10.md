# 019 — Sol: fechar a nova versão da Arena Tática

Origem: ordem direta do autor para Astra revisar a versão do Claude e repassar a execução a Sol, com autorização para intervenções necessárias. Prioridade atual substitui a produção de personagens Blender, que fica pausada. Status inicial: pronta para execução.

Leia primeiro `../auditorias/2026-10-13_astra-prontidao-release.md`, o pedido 018 e as instruções atuais do repositório. A exceção de 13/10 permite ao GPT/Codex editar código da Arena, além de visuais.

## Responsabilidade e sequência

Sol executa correções, validação, builds, documentação e preparação/publicação desta versão. Astra concluiu a auditoria e não editará esses arquivos em paralelo. Você não está sozinho no monorepo: preservar as alterações dos outros, registrar numa nota a lista exata de `.cs` antes de tocá-los e usar checkout isolado se precisar de execução concorrente. Nunca reset/clean/add em massa, nem pull cego sobre o trabalho local. Não retomar personagens Blender.

1. **Reproduzir e corrigir a exposição da posição dos invisíveis ao adversário** em snapshots/eventos/reconexão. Astra confirmou o caminho `Match.ViewFor` → `ArenaGame.Send`. Projetar visão de rede sem quebrar Host, timeline, ocupação, Preview, aliados, contato/revelação nem identidade dos clones. Testes de regressão devem demonstrar o segredo ausente do payload inimigo e o jogo ainda funcional. Evitar mudar regras: a correção é de implementação do comportamento já pedido.
2. **Testar o ciclo de sala e torneio novo**, incluindo falhas, desconexão, reinício sem recriar sala, 2/3/6 participantes, bans, bye, escolha de time e campeão. Corrigir defeitos reproduzidos. Não alterar a pool mínima apenas pela conta 5−2=3: quatro é máximo, não mínimo. Não inventar novas regras. Relay deve ter evidência própria; WebSocket local não o comprova.
3. **Conferir interface em uso real**: Configurar/Liberar/Iniciar, seleção e ficha, textos/skills, câmera e tabuleiro completo, controles mouse/toque. Priorizar defeitos de uso/leitura. Os refinamentos visuais do briefing 018 são secundários a uma versão funcional; ícones novos e personagens não são bloqueios. Espectador de torneio continua limitação explícita enquanto só houver resumo.
4. **Fechar distribuição**: investigar a tentativa Android interrompida, gerar APK do código final e testar instalação/abertura se houver dispositivo. Unificar ZIP com o método corrigido do script Windows; conferir duplicatas, estrutura e extração. Não deixar `package.ps1` regenerar ZIP incompatível. Registrar versão, commit, hashes, testes e plataformas. Recompilar Windows para a revisão final; atualizar Web se esse for um canal desta publicação, sem misturar a build de 09/10 com as novas.
5. **Atualizar documentação e publicar com evidência**: LEIA-ME, VALIDACAO e registro de testes devem refletir o produto atual. Recuperar o destino/canais existentes de publicação do Claude; não criar página duplicada. Seguir a autorização vigente de commit/push da meta concluída, somente arquivos próprios. Antes de upload, conferir o pacote final e canal correto. Não afirmar publicado sem verificar o artefato remoto/versão acessível. Se faltar autenticação ou informação exclusiva do autor, entregar tudo pronto e registrar exatamente o impedimento.

## Evidência existente que pode ser reaproveitada

Astra executou agora 84/84 EditMode e passou smoke Windows (local, Host, cliente) e contrato de rede (10 checks). São evidências de base, não substituem reteste depois das suas mudanças. O ZIP Windows existente tem proveniência de checkout dirty; APK e Web estão antigos. Veja o relatório para caminhos e datas.

O Unity de testes modificou um define de analytics no ProjectSettings e existem arquivos de conta Unity não rastreados: revisar antes de versionar. Não publicar informações de conta ou logs com tokens. Evitar builds Windows/Android paralelos: os scripts compartilham a cópia ASCII. Nenhum build/teste da auditoria Astra permanece em execução ao entregar esta tarefa.

## Critério de encerramento

Relatar por plataforma: revisão exata, build concluída, testes atuais, conferência visual, SHA-256, destino e confirmação de publicação (ou bloqueio específico). Sala/torneio e invisibilidade precisam de evidências próprias. Registrar pendências deliberadas sem chamar a meta incompleta de concluída.

Escrever o resultado em `../respostas/019-sol.md`, separando implementado, testado, publicado e pendente. Não depender de mensagem de volta para Astra; o relatório fica na ponte e o autor acompanha este chat. Pedidos futuros de comunicação entre chats continuam sujeitos à autorização direta do autor.
