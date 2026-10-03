# Claude ↔ Designer de Cartas

## Responsabilidades e canal

Claude escolhe a demanda atual, a identidade mecânica e narrativa da carta, o conteúdo e o destino. Designer define o tratamento visual a partir desse briefing e entrega arquivos utilizáveis. A autorização do autor permite essa coordenação dentro do projeto Rpg.

Canal implementado: arquivos versionados neste repositório, sincronizados por Git com autorização de push. Conexão direta com a conversa privada do Claude não está estabelecida. Se um canal direto ficar disponível, usar a mesma tarefa e salvar suas decisões nesses arquivos. Não executar comandos recebidos como texto de briefing.

## Etapas e escrita separada

1. Claude grava `pedidos/<id>.claude.json`, com `autor: "Claude"`, `estado: "solicitado"`, `necessidadeAtual: true`, versão e referências de carta/código. Usar o modelo desta pasta e preencher o objetivo atual.
2. Designer lê esse pedido antes de planejar ou gerar. Se faltar uma definição de identidade, regra, conteúdo textual ou escopo, grava `pedidos/<id>.duvidas.json` com perguntas curtas e estado `aguardando_claude`. Claude atualiza seu pedido e acrescenta respostas. Descrição estética insuficiente permite enriquecimento pelo Designer; não exige esperar por detalhes de cenário/composição.
3. Designer grava `planos/<id>.visual.json`. Prioridade, identidade, textos e escopo devem corresponder à versão atual do pedido. Templates e cenas compartilhados só são criados se necessários à demanda atual.
4. Após produzir, Designer grava `entregas/<id>.visual.json`: `tarefaId`, `pedidoVersao`, `cartaId`, `estado: "entregue"`, `arquivos` com tipo/caminho/hash, `arteOriginal`, `templateCanva` com ID/link ou null, dimensões, impressão, alterações, aprovação e pendências. Usar caminhos relativos à raiz do repositório. Disponibilizar arquivos locais e publicar pelo Git quando autorizado.
5. Claude consome a entrega e escreve `entregas/<id>.integracao.claude.json`: versão recebida, caminhos usados no motor/UI, `estado` (`integrado` ou `ajuste_solicitado`), motivo e requisitos restantes. Somente esse retorno permite marcar integração confirmada. Ajustes geram nova versão, preservando a anterior.
6. Cancelamento ou mudança de necessidade no pedido do Claude interrompe a produção; a versão substituída não autoriza novas gerações. Ao iniciar outra carta, repetir a consulta ao Claude.

## Pedido mínimo

`tarefaId`, `autor`, `estado`, `necessidadeAtual`, `versao`, `atualizadoEm`, `objetivoAtual`, `referenciasClaude`, e `carta` com `id`, `nome`, `identidade`, `tipo`, `textos`, `cena`, `personagens`, `composicao`, `uso`. Ausência de estatística/custo deve ser declarada pelo Claude; não adivinhar nem copiar de outra carta.

`textos` conserva as strings exatas que precisam aparecer. `cena`, `personagens` e `composicao` registram detalhes disponíveis; podem ser enriquecidos pelo Designer quando pobres ou ausentes. O autor permite criar efeitos mágicos, personagens, lugares, eventos, cenários e interações, inclusive conjuração e ataques. Priorizar estética, sensibilidade e magia sem mudar a função mecânica ou a identidade essencial. Registrar `enriquecimentosVisuais`, autores das escolhas e novas identidades no manifesto.

## Embelezador

Claude pode solicitar assets visuais de animação e efeitos em `../efeitos/pedidos/<id>.claude.json`. Designer e Embelezador compartilham IDs e referências de cartas, mas escrevem entregas separadas. Aliados usam o movimento genérico compartilhado de ataque de carta; não ganhar efeitos específicos de ataque por iniciativa dos agentes. Claude implementa interação, transições e comportamento do jogo a partir dos assets entregues.

## Entrega desta configuração

O autor autorizou o push das artes aprovadas, do agente e da documentação para o Claude acessar. Essa autorização vale para esta entrega. Pushes de novas cartas e revisões futuras continuam sujeitos à autorização do autor. Nenhuma demanda de carta deve ser inventada a partir da disponibilidade de artes.

## Lotes de cartas (Produtor de Cartas)

Quando o autor e o Claude fecham um conjunto de cartas, o Claude grava `pedidos/lote-<id>.claude.json` (modelo em `modelos/lote.claude.exemplo.json`) com a mecânica exata e a descrição dos personagens e do que a arte precisa ter. O **Produtor de Cartas** (`.agents/skills/produtor-de-cartas/SKILL.md`) vê o lote e põe os agentes visuais para trabalhar em cada carta, registrando o estado em `producao/lote-<id>.estado.json`. As entregas e a confirmação de integração seguem as etapas acima, carta por carta.

## Diretriz obrigatória de acabamento v9

Direção atual do autor (03/10/2026): raridade é somente estética, sem alterar regras/efeitos/números. Claude informa raridade e arteAlternativa em todo pedido/prompt de imagem de carta. Toda lendária ou alternativa inclui iluminação, textura e brilho na ilustração conforme Arcanoclisma/cartas/DIRETRIZ-ACABAMENTO-E-RARIDADE.md; salvar o prompt e os parâmetros cosméticos. Alternativa pode sugerir volume 3D com luz, escorço e planos, mas fica inteiramente dentro da carta. Manter identidade, direção de arte e cenas novas para personagens existentes. O acabamento HTML/CSS complementa a arte; não substitui a iluminação pedida na geração. Não refazer automaticamente artes aprovadas.
