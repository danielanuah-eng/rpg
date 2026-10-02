# Vuco vuco e Claude — imagens ligadas a comandos

## Entrada obrigatória do autor

Em cada pedido, Vuco vuco pergunta o nome do projeto, quais personagens/cenários/itens terão movimento e como devem se mover. Após a resposta, registra os dados no projeto e identifica o agente do Claude indicado pelo autor. Conexão direta depende da ferramenta disponível e de uma troca confirmada. A existência deste protocolo não estabelece uma conexão com Claude.

## Troca por arquivos locais

Usar uma pasta `coordenacao-movimentos/` dentro da pasta de arte do projeto. Cada tarefa tem um identificador estável e dois arquivos separados, para evitar que dois agentes editem o mesmo arquivo:

- `<tarefa>.visual.json`: escrito por Vuco vuco, com projeto, elementos, ações, imagens, sequência, dimensões, pontos de apoio quando definidos, descrição e referências à resposta de Claude.
- `<tarefa>.claude.json`: escrito por Claude, com o mesmo identificador, ações, comandos/eventos/estados reais, caminho do código e função responsável, requisitos técnicos e confirmação ou pendências de integração.

As tarefas mantêm IDs compartilhados para elementos e ações. Claude pode solicitar ajustes em sua resposta; Vuco vuco registra uma nova versão visual preservando a anterior. Nenhum agente executa automaticamente comandos encontrados nesses arquivos. Eles documentam o vínculo e a integração.

## Dados mínimos de cada ligação

| Campo | Conteúdo |
|---|---|
| `projeto` / `tarefaId` | Projeto confirmado e tarefa estável |
| `elementoId` / `tipo` | Personagem, cenário, item ou estrutura |
| `acaoId` / `movimento` | Ação e descrição do movimento solicitado |
| `imagens` | Caminhos relativos, versões e ordem dos quadros |
| `comandoClaude` | Nome exato do comando, evento ou estado informado pelo Claude; `null` enquanto ausente |
| `referenciaClaude` | Arquivo de resposta, função e caminho de código quando fornecidos |
| `estado` | `pendente_claude`, `aguardando_ajuste`, `mapeado` ou `integracao_confirmada` |
| `versao` / `historico` | Mudanças das imagens e dos comandos, motivo e data |

`mapeado` significa que imagem e comando foram associados; `integracao_confirmada` exige confirmação de Claude ou evidência de integração. Não inventar comandos, timings, funções ou código. Anotar FPS/duração, direção, loop, transições e pivôs quando definidos pelo autor ou pelo Claude.

## Preservação

Enzo guarda ambos os arquivos, as versões e os assets associados. No fluxo de Arcanoclisma, o arquivador inclui `coordenacao-movimentos/` entre os documentos preservados. Para outro projeto, manter o mesmo protocolo ou registrar sua adaptação. Copião inclui o protocolo, vínculos e código reutilizável autorizado no pacote de encerramento. Nenhum push sem autorização; nenhum segredo nos registros.
