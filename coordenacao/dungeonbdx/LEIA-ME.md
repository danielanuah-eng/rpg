# Ponte: DungeonBDX

Projeto: C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\ArenaTaticaUnity
Pipeline do autor (15/10/2026): Codex implementa 01; Claude 02 e 03; Antigravity 04; Codex 05; Codex + Antigravity 06. Ordem estrita; ver `PIPELINE.md`. Codex consolida gates, revisão e entrega. O pedido direto do autor prevalece sobre a tabela genérica do PROTOCOLO.md.

**Regras completas: `PROTOCOLO.md` (nesta pasta).** Para ativar uma IA: `PROMPT-COLAR.md`.
Estado: `node estado.mjs`.

## Execução
Ler a tarefa de menor número destinada à IA, somente após a dependência estar revisada no estado. Cada executor usa cópia/worktree e os arquivos exclusivos da tarefa. Não editar os scripts de outro executor. Respostas atômicas em `respostas/`; Codex confere diff e testes antes de liberar a próxima etapa.

Etapa 01 tem módulo/cena próprios em `ArenaTaticaUnity/Assets/DungeonBDX`; catálogo por ID, sem alterar combate da Arena. Branch `codex/dungeonbdx-step1`. O cache Unity existente é usado na validação por limite de espaço em disco. Fontes TMP oficiais ficam dentro do módulo. Imagem de referência citada não anexada; paleta provisória registrada.

Etapas 02–06 ainda não executadas. Não declarar contas, multiplayer, saves, cenários finais ou música como prontos. Não adquirir assets pagos sem autorização específica de gasto. Acesso a KrosArcano é local e autorizado.

Push automático da entrega completa autorizado pelo AGENTS.md do autor. Executores externos entregam localmente; Codex faz revisão/commit/push apenas dos arquivos da entrega, preservando alterações concorrentes.
