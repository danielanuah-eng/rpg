# 02. Core matemático do DungeonBDX

- **De:** autor / UltraBridge, consolidado por codex
- **Para:** claude
- **Status:** pendente
- **Projeto / pasta de trabalho:** C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg
- **Entrega:** arquivos locais, sem commit/push do executor
- **Dependência:** resposta 01-codex-ui-herois.resposta.md com validação funcional da prévia

Ler PIPELINE.md da ponte e investigar referências locais autorizadas em `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\KrosArcano`. Não enviar PDFs/planilhas comerciais a serviços externos. Esta etapa pertence ao DungeonBDX; manter regras da Arena existentes.

Definir e implementar core matemático próprio com limites explícitos e documentados: armadura com soft/hard caps, mitigação limitada, dano mínimo/máximo, crítico e rerrolagens limitadas sem crescimento exponencial, ordem de modificadores e empilhamento. Demonstre o pior caso de builds e limites por turno/ação. Decisões de lógica sensível ficam com Claude/Codex; DeepSeek apenas investigação mecânica, via OpenRouter.

Propriedade exclusiva: `ArenaTaticaUnity/Assets/DungeonBDX/Combat/` (incluindo asmdef); `ArenaTaticaUnity/Assets/DungeonBDX/Tests/Combat/`; `coordenacao/dungeonbdx/contratos/balanceamento.json`; `coordenacao/dungeonbdx/respostas/02-claude-balanceamento.resposta.md`.

Registrar cada .cs antes de editar em coordenacao/arena-pixelai/notas. Trabalhar em cópia/worktree própria; ninguém edita Runtime/DungeonShell.cs nesta etapa. Contrato JSON deve conter fórmulas, unidades, caps, seed/RNG, exemplos, versão e invariantes para UI e futuras skills. Não alterar arquivos compartilhados sem nota.

Pronto: testes de limites/pior caso, determinismo e monotonicidade; probabilidades calculadas; relatório com comandos e resultados reais. Codex revisa antes de liberar 03. Resposta atômica .tmp → .md. Não iniciar 03 sem registro de gate aprovado.

Dependências de código: core Combat numérico/determinístico, sem depender de DungeonBDX.Runtime/UI. Runtime poderá referenciar Combat/Skills nas integrações posteriores; evitar ciclos entre asmdefs. Os valores importados na prévia são explicitamente de origem, sem conversão AQ já aplicada.
