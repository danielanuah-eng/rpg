# Divisão de responsabilidades

## Site online — gestão atribuída ao Codex em 03/10/2026

O autor pediu reduzir o gasto do Claude com gerenciamento. Codex assume triagem de demandas, planejamento, escolha das funções/agentes necessários, dependências, fila, pedidos de revisão e consolidação das entregas. Claude recebe uma demanda/entrega por etapa e mantém programação, lógica, rede, dados e integração. Protocolo atual em `coordenacao/site-online/DIVISAO.md`; estado escrito por Codex em `estado.codex.json`, confirmação escrita por Claude em `resposta.claude.json`. A atribuição foi autorizada pelo autor; a leitura pelo Claude ainda precisa de confirmação. Essa divisão não inicia novos processos nem altera agentes desativados.


## TCG — atualização de 03/10/2026

Designer de Cartas atende exclusivamente necessidades atuais do Claude e cria templates Canva, cenas e cartas para impressão. Pode enriquecer descrições visuais insuficientes com detalhes, eventos, efeitos mágicos, personagens, lugares e interações, priorizando estética, sensibilidade e magia sem alterar regras ou identidade essencial. Embelezador cria os assets visuais de efeitos e animações de que Claude precisa, junto do Designer ou separadamente. Claude implementa comportamento e integra as entregas. Aliados têm somente movimento genérico fluido e compartilhado de ataque, sem efeito específico individual. Os protocolos e manifestos ficam em `Arcanoclisma/cartas/` e `Arcanoclisma/efeitos/`.

O autor autorizou nesta conversa o push desta entrega de artes aprovadas, agentes e documentação. Pushes futuros continuam exigindo autorização. Essas definições não iniciam uma produção autônoma de cartas sem pedido atual do Claude.

Definida pelo autor em 02/10/2026 para este repositório.

## Codex — criação e direção visual

- Criar e revisar imagens, ilustrações, designs e assets visuais.
- Ler e analisar referências, modelos de imagem e resultados de geração.
- Definir estilo, composição, poses, expressões, cores, materiais e identidade visual.
- Criar modelos 3D quando o projeto precisar, usando as ferramentas disponíveis; registrar formatos, dimensões, materiais e limitações de cada entrega.
- Preparar arquivos visuais para uso no jogo e fornecer ao Claude informações de integração: caminhos, formatos, tamanho, escala, transparência, pivôs e animações quando aplicáveis.
- Manter os prompts, registros, galeria e progresso da produção visual.

## Claude — demais partes do projeto

- Cuidar da programação, lógica do jogo, sistemas, arquitetura, configuração, ferramentas, integrações e demais tarefas fora da produção visual.
- Integrar os assets entregues pelo Codex ao jogo e implementar o comportamento de interfaces e animações.
- Solicitar ajustes visuais por meio de especificações com objetivo, formato, dimensões e destino; preservar os arquivos visuais aprovados.

## Trabalho conjunto

- Canva teve o acesso confirmado em 02/10/2026. O autor autorizou seu uso nas futuras cartas para criar modelos editáveis e cuidar de alinhamento, tipografia, hierarquia e organização dos textos junto das imagens.
- Codex cria as ilustrações e coordena a composição das cartas. Pode dividir etapas com o Canva usando a ferramenta mais eficiente para cada tarefa, conforme recursos disponíveis, qualidade, tempo e consumo. Claude continua responsável pelas demais partes do projeto.
- Para as cartas, preservar a arte original e manter textos e elementos do modelo editáveis sempre que possível. Definir dimensões, margens e áreas de imagem e texto conforme o destino da carta; registrar o link do modelo e os arquivos de entrega no projeto.

- Cada agente trabalha em arquivos de seu escopo. Para trabalho simultâneo, usar pastas/worktrees e branches separadas.
- Não editar os mesmos arquivos ao mesmo tempo. Coordenar alterações de arquivos compartilhados antes de executá-las.
- Registrar entregas e pendências em arquivos do projeto. Esta divisão não cria um canal automático de mensagens entre agentes.
- Alterações na divisão de responsabilidades dependem das instruções do autor.
- Sempre perguntar ao autor antes de qualquer push. Salvar o trabalho localmente até obter autorização explícita.

## Direção visual de Arcanoclisma

- Seguir `Arcanoclisma/AGENTS.md`, a direção de arte e as descrições do catálogo.
- Aplicar `Arcanoclisma/arte/variacao-personagens.md`: pessoas e humanoides com poses, ações, silhuetas, rostos, cabelos, olhos, tons de pele e expressões variados.
- Wilderfeast é referência de linguagem corporal, expressão, movimento e profundidade. O foco pedido pelo autor é a variedade de poses.
- Imagens novas com fundo transparente e alfa conferido. Preservar versões aprovadas e registrar adaptações.
