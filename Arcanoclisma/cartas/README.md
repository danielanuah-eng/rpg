# Arcanoclisma — design visual do TCG

Agente: **Designer de Cartas**. O catálogo de personagens é uma biblioteca de entrada; as cartas podem usar essas artes e novas cenas pedidas pelo Claude. O foco é mostrar personagens em eventos, interações, ações e ambientes, mantendo a direção de arte do projeto.

## Trabalho atual

As 435 artes 087–521 foram aprovadas pelo autor, incluindo o segundo braço de Aurelhaz. O agente de cartas foi definido; nenhuma carta ou base Canva foi gerada nesta configuração. A consulta ao Canva falhou por erro de transporte em 03/10/2026; registrar nova confirmação de acesso quando um pedido real exigir a ferramenta.

O Designer só atende a necessidade atual do Claude. Não há pedido de carta confirmado na fila ainda. O código do TCG fica em `../tcg/`, sob responsabilidade do Claude.

## Arquivos

- `catalogo-artes.json`: IDs, caminhos, hashes e aprovação das artes disponíveis; gerar com `node Arcanoclisma/cartas/scripts/organizar-assets.mjs`, na raiz do repositório.
- `COORDENACAO-CLAUDE.md`: conversa por pedidos, dúvidas, entregas e confirmação de integração.
- `ESPECIFICACAO-IMPRESSAO.md`: corte, sangria, margem e exportação.
- `REFERENCIA-ALTERED.md`: estudo de personagens em cenas, ambientes, movimento, cor, fluxo do template e hierarquia de escrita, incorporado aos dois agentes.
- `templates.json`: modelos reais do Canva e módulos aprovados; começa vazio.
- `modelos/pedido.claude.exemplo.json`: exemplo de briefing, sem autorização de produção.
- `scripts/planejar-pedido.mjs`: prepara plano a partir de um pedido atual, sem gerar imagens nem chamar outras IAs.

## Organização das entregas

Cada tarefa usa `pedidos/<id>.claude.json`, `pedidos/<id>.duvidas.json`, `entregas/<id>.visual.json` e `entregas/<id>.integracao.claude.json`. As imagens derivadas ficam em `artes/<id>/vNN/`; templates e exportações locais em `templates/<templateId>/vNN/`; cartas em `cartas/<cartaId>/vNN/` com `preview.png` e, quando conferido, `impressao.pdf`. Essas pastas são criadas ao entregar arquivos reais. O manifesto liga cada carta à arte original, à cena, ao template e aos dados do Claude.

Texto, estatísticas, custos e nomes vêm do pedido do Claude. Alterações de regras voltam ao Claude antes da diagramação final. Conteúdos desta pasta documentam a coordenação; não significam conexão automática com uma conversa externa.
