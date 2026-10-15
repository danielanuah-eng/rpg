# Ponte: DungeonBDX

Projeto: C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\ArenaTaticaUnity
Pipeline atual do autor (15/10/2026): Codex implementa 01–03 (02/03 reatribuídas explicitamente); Antigravity 04; Codex 05; Codex + Antigravity 06. Ordem estrita; ver `PIPELINE.md`. Codex consolida gates, revisão e entrega. O pedido direto do autor prevalece sobre a tabela genérica do PROTOCOLO.md.

**Regras completas: `PROTOCOLO.md` (nesta pasta).** Para ativar uma IA: `PROMPT-COLAR.md`.
Estado: `node estado.mjs`.

## Execução

Gate 03 aprovado: **218/218 testes Unity integrados**, incluindo regressão Arena/UI, core e skills. `contratos/skills.json` foi exportado das DLLs compiladas; `contratos/assets-pedido.json` define a entrega 04. Ativar Antigravity nesta ponte para a tarefa 04; nenhum executor externo está confirmado. Etapas 05/06 aguardam assets. A prévia Windows continua sendo a etapa 01; o domínio 02/03 ainda será ligado à cena/HUD em 05/06. Saves e online não estão implementados.

Direção posterior do autor: **“Codex assume as etapas 2–3”**. Claude não deve iniciar outro executor nessa cópia; o launcher antigo agora verifica essa atribuição e interrompe a abertura. Histórico de autenticação/cota foi preservado nas notas, sem representar bloqueio atual do Codex.

Implementação isolada em `tmp/dungeonbdx-step1-git`, branches `codex/dungeonbdx-step2` e `codex/dungeonbdx-step3`, com validação no cache principal; sem duplicar Library/builds. Respostas atuais: `02-codex-balanceamento.resposta.md` e `03-codex-skills.resposta.md`. Para aguardar outra entrega: `node esperar.mjs <ponte> --eu codex --resposta <arquivo.resposta.md>`.
Ler a tarefa de menor número destinada à IA, somente após a dependência estar revisada no estado. Cada executor usa cópia/worktree e os arquivos exclusivos da tarefa. Não editar os scripts de outro executor. Respostas atômicas em `respostas/`; Codex confere diff e testes antes de liberar a próxima etapa.

Etapa 01 tem módulo/cena próprios em `ArenaTaticaUnity/Assets/DungeonBDX`; catálogo por ID, sem alterar combate da Arena. Branch `codex/dungeonbdx-step1`. O cache Unity existente é usado na validação por limite de espaço em disco. Fontes TMP oficiais ficam dentro do módulo. Imagem de referência citada não anexada; paleta provisória registrada.

Etapas 04–06 ainda não executadas. Não declarar contas, multiplayer, saves, cenários finais ou música como prontos. Não adquirir assets pagos sem autorização específica de gasto. Acesso a KrosArcano é local e autorizado; referências foram lidas localmente, sem envio ao DeepSeek.

Push automático da entrega completa autorizado pelo AGENTS.md do autor. Executores externos entregam localmente; Codex faz revisão/commit/push apenas dos arquivos da entrega, preservando alterações concorrentes.
