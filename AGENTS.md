# Colaboração neste repositório

- Direção do autor em 10/10/2026: ao concluir uma **meta inteira de trabalho**, fazer commit e push automaticamente dos arquivos dessa entrega. A autorização inclui os **protótipos de site iniciados a partir desta instrução**, mesmo que ainda sejam protótipos. Não publicar etapas parciais como meta concluída; preservar alterações concorrentes e registrar o estado real de revisão e integração. Esta regra substitui pedidos anteriores de confirmação para commit/push dentro desse escopo; não libera conteúdo jogável da expansão bloqueada.

- Direção de 04/10/2026: toda magia/carta com efeito e cada skin nova têm animação correspondente à imagem e ao efeito atual. Ler `Arcanoclisma/efeitos/README.md` e manter o vínculo por ID/skin/hash. Economia (`compraFinal`) aparece ao lado da Vida nos Conjuradores. A escolha de câmera confirmada pelo autor ao Claude é manter o tabuleiro inteiro visível; refinar zonas/legendas sem reativar a câmera fixa com rolagem da v15. Push das entregas concluídas e de todos os pedidos visuais do Claude foi autorizado novamente; preservar alterações concorrentes.

- Direção posterior de 04/10/2026: **não usar nem reenviar o PNG do modelo frontal corrigido de Ciantia**. Já havia sido enviado em `dceec35` antes desse pedido; o manifesto de conjuradores marca retirada de uso. Reservar apenas o novo prompt para a expansão: Ciantia lateral, olhos fechados, agachada sobre os joelhos brincando com os dois dragões voltados para o jogador, livro atrás. Após a etapa atual de artes, priorizar D20 nas cores do Dragão Transplanar e entrada gradual da arena, antes da expansão. Ler `Arcanoclisma/efeitos/PRIORIDADES-04-10.md` e `coordenacao/site-online/RITMO-E-ATRIBUTOS-04-10.md`. Ataque/vida e seus números ficam centralizados; movimentos automáticos mais lentos e fluidos.

- Fila visual atual: 210 artes (151 iniciais mais 59 novas solicitadas pelo Claude para a variedade pedida pelo autor), em `Arcanoclisma/arte/cartas-site/fila.json`. Priorizar as pendências do lote inicial e depois o pedido `cartas/pedidos/lote-site-v2.claude.json`; consultar dados atuais e a fonte de cada item. A meta de avaliação continua sendo **toda a fila**, conforme o autor; instruções de avaliar a cada cinco artes no briefing de Claude não substituem essa escolha. Os 132 efeitos particulares de resolução têm pedido e estado de entrega separados.

- Direção mais recente do autor: Codex ajusta apenas arte, templates, movimentação e animações. Seguir `coordenacao/site-online/VALORES-E-RARIDADE-03-10.md`. Raridade fica sem aplicação automática no app, mas brilhos, texturas e animações são preservados para escolha do autor. Equipamentos genéricos podem ser cinza; demais cartas têm Valor. Push de todas as imagens foi explicitamente reafirmado; publicar entregas prontas com seu estado real de revisão.

- Autorização atual do autor, 03/10/2026: push automático das entregas concluídas deste trabalho e integração com o Claude. Prioridade: refazer as 151 artes do jogo atual, lendo cada imagem base e descrição, em cenas novas e integradas. A expansão A Mestra dos Dragões tem 90 cartas e aguarda essa revisão; inclusão em jogo e alteração de suas cartas seguem bloqueadas até liberação específica do autor. Detalhes de interação em `coordenacao/site-online/INTERACOES-E-NITIDEZ.md`.

- **A Mestra dos Dragões**: produção visual autorizada pelo autor em 03/10/2026; integração em jogo e criação/alteração das cartas pelo Claude bloqueadas até liberação explícita do autor. Ler `Arcanoclisma/expansoes/a-mestra-dos-dragoes/expansao.json`. Aprovar artes ou autorizar push não libera o conteúdo jogável.

- Direção atual do autor para artes de cartas (03/10/2026): **todas as cartas têm cenário e ambientação**, qualquer que seja o tipo. Seguir `Arcanoclisma/cartas/DIRETRIZ-CENARIOS.md` em todo prompt ao GPT. Recortes do RPG servem como referência de identidade, não como entrega final de carta. Preservar a biblioteca de referências e as cenas aprovadas.


- Etapa do site online (pedido do autor em 03/10/2026): **Codex gerencia os agentes e a divisão de tarefas**, consolidando demandas/entregas para poupar créditos do Claude. Seguir `coordenacao/site-online/DIVISAO.md` e `estado.codex.json`. Não iniciar processos de agentes por existir uma definição; usar as skills como funções conforme a tarefa. Claude cuida da programação e integração. Durante produção visual, manter só a produção e os registros mínimos; push de novas entregas exige autorização.


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
- **Exceção do autor (13/10/2026): na Arena Tática o GPT/Codex pode mexer em tudo**, inclusive no código de `ArenaTaticaUnity/` (turnos, câmera, sala online, torneio, loja, skills, UI). Ver `coordenacao/arena-pixelai/tarefas/018-codex-arena-pedidos-13-10.md`. Antes de editar um `.cs`, registrar o arquivo numa nota em `coordenacao/arena-pixelai/notas/` e rodar os testes depois; não mudar regras sem pedido do autor.
- Respeitar as instruções mais específicas de cada diretório e as orientações atuais do autor.
- Para trabalho simultâneo, usar pastas/worktrees e branches separadas; coordenar arquivos compartilhados.
- Push automático autorizado pelo autor em 03/10/2026 para este trabalho; substitui a instrução anterior de perguntar antes. A expansão continua exigindo liberação específica para entrar em jogo.
