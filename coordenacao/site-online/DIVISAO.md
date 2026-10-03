# Divisão atual — site online Arcanoclisma

Pedido do autor em 03/10/2026: Codex assume sobretudo o gerenciamento dos agentes para diminuir os créditos gastos pelo Claude nessa atividade. Trabalhar por etapas, com pedidos curtos e entregas consolidadas. Coordenação por arquivos locais/Git; não existe conexão direta confirmada com a conversa em execução do Claude.

## Tarefas e responsáveis

| Tarefa | Responsável | Entrada/saída |
| --- | --- | --- |
| Prioridades, triagem, escolha de agentes e dependências | Codex | `estado.codex.json` |
| Criação do site, backend, regras, cartas/dados, rede e integração | Claude | `Arcanoclisma/online/` e `Arcanoclisma/tcg/` |
| Imagens, direção visual, composição, templates e assets de efeitos | Codex | `Arcanoclisma/arena/`, `cartas/`, `efeitos/` e entregas visuais |
| Pedido funcional/visual do site | Claude | `Arcanoclisma/online/pedidos-visuais/site.claude.json` |
| Entrega visual única com resumo, caminhos e limitações | Codex | `Arcanoclisma/online/entregas-visuais/site.codex.json` |
| Decisões técnicas, integração e dúvidas bloqueantes | Claude | `resposta.claude.json` |

Claude mantém seus arquivos do site, inclusive CSS/JS em andamento. Codex prepara componentes/estilos visuais no kit da arena e entrega os caminhos ou trechos de integração. Uma transferência de arquivo para edição direta precisa ser registrada antes, com proprietário único. Cada IA escreve seus próprios pedidos, respostas e estados.

## Etapas atuais

1. **Codex — gestão ativa:** registrar esta divisão e o material visual já disponível. Preservar os estados atuais de agentes desligados. Não editar `AGENTES-VISUAIS.json` enquanto Claude estiver ajustando esse mesmo arquivo.
2. **Claude — continuar o site:** implementar a necessidade atual e registrar um pedido visual consolidado quando houver trabalho visual específico. Usar o modelo curto abaixo; não precisa escolher ou gerenciar agentes visuais.
3. **Codex — atender o pedido:** selecionar somente as skills necessárias, reutilizar assets aprovados, produzir o que faltar e registrar uma entrega. Demandas não confirmadas não autorizam gerações extras.
4. **Claude — integrar:** usar os caminhos/contrato da entrega e responder apenas o resultado ou os bloqueios. A confirmação de integração é escrita por Claude, não presumida por Codex.

## Política de economia

- Uma triagem por pedido novo ou alteração relevante; trabalhar nas etapas independentes sem reuniões repetidas entre agentes.
- Designer, Carlos, Embelezador, Arquiteto e Vuco vuco são funções/skills disponíveis ao Codex. Não iniciar uma conversa, processo ou modelo separado para cada nome.
- Ler somente o pedido atual, as referências necessárias e o resumo da entrega. Reusar o kit aprovado; não refazer arte por haver nova etapa do site.
- Manter os agentes desativados pelo autor/Claude. Não reativar Enzo, Copião, Produtor de Cartas, balanceamento ou testes só para gerenciar esta etapa. Ativação depende de necessidade e autorização aplicável.
- Adiar arquivo/sincronização auxiliares durante produção visual. Nenhuma automação de acompanhamento é criada por esta divisão.
- Agrupar dúvidas técnicas bloqueantes em uma resposta. Claude só precisa validar mudanças de regras/contrato e registrar integração; composição estética segue a direção aprovada com Codex.
- Não criar valores de economia prometidos: consumo depende do trabalho real. Não executar novos testes por conta desta coordenação.
- Não fazer push de novas entregas sem perguntar ao autor. Nenhum segredo entra nos pedidos ou manifestos.

## Pedido curto para Claude preencher

Usar `Arcanoclisma/online/pedidos-visuais/site.exemplo.json` como modelo; o exemplo não inicia produção. Campos por asset de carta: raridade e arteAlternativa explícitas. Campos por movimento: evento real, alvo, duração e formato esperado, quando disponíveis. Informações ainda desconhecidas ficam declaradas como pendência.

## Material já disponível

- `Arcanoclisma/arena/entrega-claude.md`: arena, cartas/itens, API de suspensão/ataque/bloqueio e acabamento v9. Prévia em `Arcanoclisma/arena/index.html`.
- `Arcanoclisma/cartas/propostas/PARA-CLAUDE.md`: Dragão Transplanar e Lição do Jardim; artes aprovadas e regras propostas pelo autor. Claude define parâmetros e integra.
- `AGENTES-VISUAIS.json`: disponibilidade declarada das funções; não indica processos independentes em execução.

## Confirmação da coordenação

Não afirmar que Claude recebeu esta divisão apenas porque os arquivos existem. Ele pode confirmar em `resposta.claude.json` com `divisaoRecebida: true`, `arquivosEmEdicao`, `prioridadeAtual` e eventuais ajustes. Se sua conversa já estiver em andamento, reler a seção inicial de `CLAUDE.md` para aplicar a instrução nesta etapa.
