# 03 — Habilidades e perfis DungeonBDX

**Status:** CONCLUIDO
**Executor:** Codex, conforme autorização explícita do autor para 02–03.

Entrega de domínio: `ArenaTaticaUnity/Assets/DungeonBDX/Skills/`, `Tests/Skills/`, `Tools/export-dungeonbdx-contracts.ps1`; contratos `skills.json` v1.0.0 e `assets-pedido.json`. Códigos criados no worktree, branch codex/dungeonbdx-step3, e validados no cache principal.

18 habilidades originais: básicos, disparos/mira/ruptura, magia de dano/controle/escudo/cura/teleporte e invocação/comando/sopro/pacto/recolhimento. Custos, alcance, área, recarga, duração, usos, limites e IDs de animação/SFX/ícones são imutáveis. Expectativas e piores casos exportados pelo core compilado; nenhuma porcentagem se multiplica nem crítico gera dado extra.

Adaptador explícito dos 28 IDs jogáveis da Arena, com snapshots dos atributos/IDs de origem, conversão versionada e loadouts por arquétipo; não é uma transcrição 1:1 dos efeitos/formas/passivas antigos. Três perfis novos: Lyra Faísca (pistoleira), Orion Prisma (arcano versátil), Vaelis do Vínculo (domador). Vida 40–60 nas adaptações, três PA, armadura 0–50 de partida, movimento 2–5. Fichas da Arena não são alteradas.

Execução real de domínio: turno do ator, alvos/equipe/alcance/LOS/ocupação, passos/teleporte, cooldown e usos; comandos inválidos não gastam PA nem RNG. Vida/escudo com caps; uma invocação atacável por dono, sem turno próprio. Pet/área compartilham PA, RNG, cooldown e orçamento do dono. Sem reset/refund ao recolher/matar pet. Mira é consumida por um ataque e não cria rerrolls em cadeia. Módulos de companions/control/teleport/friendly fire são independentes; caps continuam obrigatórios.

Verificação: primeiro run integrado **217/217**. Revisão mecânica DeepSeek leu sete arquivos, não encontrou falha concreta e deixou a âncora de área como dúvida. Codex reproduziu em DLL real uma formação válida rejeitada por ordenar o centro pelos IDs; corrigiu para `targetIds[0]` ser o centro escolhido, mantendo ordem ordinal só no orçamento, e acrescentou regressão. Run final Unity CLI/6000.6.4f1, EditMode completo: **218/218 Passed, zero falhas**, dos quais 25 skills/perfis, 28 core e 165 Arena/UI. XML em `Artifacts/dungeonbdx-skills-all-editmode.xml`, cópia/evidência em `entregas/03-skills/`.

Pendências `?` do revisor: compilação e correspondência ArenaSources resolvidas pelos testes reais; convenção de cooldown mantida/documentada; âncora de área corrigida e testada. Codex revisou o diff final. Exportador executado em PowerShell 7 contra DLLs puras, com expectativas fracionárias exatas, pesos verificados e orçamento pelo TurnBudget real.

**Gate 03 aprovado: liberar 04 para Antigravity.** Pedido 04 contém 3 heróis/1 companheiro novos, 28 adaptações, 18 bindings e 3 cenários planejados com trilha própria. Nenhum modelo/arte/áudio final é declarado entregue. Não há compra de assets.

Limites: engine/domínio e contratos compilados/testados; cena/preview/HUD de combate não integrados ainda (05–06). Movimento comum do mapa, persistência completa, contas, salas e convites pertencem a 05. `RandomState` sozinho não é save completo. Bateria Windows Editor, sem build nativo Android/iOS, QA físico ou stress. Referência visual ainda ausente. Balanceamento empírico/PvP permanece para QA; invariantes matemáticos estão confirmados.
