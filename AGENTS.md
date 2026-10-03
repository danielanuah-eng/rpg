# Colaboração neste repositório

- Arcanoclisma é um TCG. Para cartas, usar `.agents/skills/designer-cartas/SKILL.md` e `Arcanoclisma/cartas/COORDENACAO-CLAUDE.md`; produzir apenas a necessidade atual do Claude, com liberdade de enriquecer cenas e detalhes estéticos. Para assets de animação/efeitos, usar `.agents/skills/embelezador/SKILL.md`. Aliados usam movimento genérico compartilhado de ataque, sem efeitos exclusivos. Claude implementa o jogo e confirma integração.

- Durante a retomada de imagens, focar apenas na produção e seus registros essenciais. Pausar atividades auxiliares e a automação de Enzo para reduzir consumo; adiar arquivamento e sincronizações até pedido do autor ou preservação final do encerramento aprovado.

- Ao comando “quero os agentes necessários” ou equivalente, seguir `.agents/skills/agentes-necessarios/SKILL.md` e trabalhar em etapas. Selecionar os agentes conforme o pedido.
- Arquiteto segue `.agents/skills/arquiteto/SKILL.md`: edita e combina imagens com procedimentos legíveis e código local reutilizável. Enzo arquiva os resultados e Copião preserva o código no encerramento.

- Copião: no encerramento de cada projeto, seguir `.agents/skills/copiao/SKILL.md` para guardar skills, procedimentos, código e ferramentas utilizadas em um pacote reutilizável para um futuro assistente offline de jogos simples.
- Vuco vuco: seguir `.agents/skills/vuco-vuco/SKILL.md` ao solicitar imagens para jogos; criar variantes de movimento quando relevantes à função do asset. Enzo arquiva essas variantes e Copião preserva o procedimento no encerramento.

- Carlos é o agente de produção visual: seguir `.agents/skills/carlos/SKILL.md`. Sua meta é concluir a fila do projeto e pedir aprovação, revisão ou encerramento. Se o autor pedir revisão sem explicar, perguntar o motivo antes de refazer.
- Enzo é o agente de memória visual: seguir `.agents/skills/enzo/SKILL.md`. Após criar ou receber assets e descrições, arquivar os materiais autorizados na biblioteca local `C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo`, preservando versões, prompts, procedência e definições de agentes para projetos futuros.

- Seguir a divisão definida pelo autor em `COLABORACAO-IAS.md`.
- Codex é responsável por imagens, designs, assets visuais, leitura e análise de referências e modelos de imagem e criação de modelos 3D quando necessários.
- Claude é responsável pelas demais partes do projeto, incluindo código e integração dos assets.
- Respeitar as instruções mais específicas de cada diretório e as orientações atuais do autor.
- Para trabalho simultâneo, usar pastas/worktrees e branches separadas; coordenar arquivos compartilhados.
- Sempre perguntar ao autor antes de qualquer push para o GitHub.
