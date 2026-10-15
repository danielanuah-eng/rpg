# 02 — Core matemático DungeonBDX

**Status:** CONCLUIDO
**Executor:** Codex, por autorização explícita do autor para assumir 02–03.

Entrega: `ArenaTaticaUnity/Assets/DungeonBDX/Combat/`, `Tests/Combat/` e `coordenacao/dungeonbdx/contratos/balanceamento.json` (v1.0.0), neste worktree. Cena/UI/Arena permanecem sem integração de combate nesta etapa.

Crítico soma um ponto, sem dados adicionais. Até seis dados e uma rerrolagem somente de falha (12 faces aceitas no máximo). Modificadores somados uma vez; porcentagens não se multiplicam. Armadura: soft cap 50, hard cap 150, eficiência de 1/4 após soft cap; mitigação total até 60%. Potência/bonus/extremos limitados com soma int64, combate inteiro determinístico.

Orçamento obrigatório por dono: três PA/ações; dano bruto 30/ação, 60/turno; cura 12/ação, 18/turno; escudo 8/ação, 12/turno. Área, projéteis, pets e reações devem compartilhar os mesmos handles. Sem reset/refund/PA grátis; handles fechados não gastam. Estado de vida/escudo e execução real de pet/área serão aplicados pelo consumidor 03. Helpers matemáticos puros não autorizam comandos de cliente; servidor e identidade são etapas futuras.

Probabilidades verificadas exaustivamente: sem rerroll, acerto 2/3 e crítico 1/6; com uma rerrolagem de falha, 8/9 e 2/9. Potência 6 sem armadura: dano esperado 29/6 ou 58/9. Pior combinação: 48 bruto solicitado, limitado a 30/ação e 60/turno. O contrato contém fórmulas, exemplos, ordem, RNG/estado, unidades e invariantes para 03/06.

Validação real: Unity CLI oficial, Editor 6000.6.4f1, `unity test <ArenaTaticaUnity> --mode EditMode --filter DungeonBDX.Tests.Combat --output <Artifacts/dungeonbdx-combat-editmode.xml> --timeout 600 --format json`: **28/28 Passed, zero falhas**. A primeira tentativa no sandbox caiu em IPC Mono antes dos testes; a segunda fora dele passou. Nenhuma licença/pacote foi alterado para isso.

Revisão mecânica DeepSeek/OpenRouter sem escrita: nenhum problema funcional de overflow, limites ou reutilização. Seus três apontamentos: testes presentes não significam executados (status pendente estava correto antes do run); capacidade de vida/escudo é parâmetro para estado 03; testes de orçamento 02 são unitários, interação real de pet/área será testada em 03. Pendências de compilação/vetores/RNG do revisor foram resolvidas pelo run Unity 28/28. Referências PDF lidas localmente via pypdf, sem envio dos arquivos ao DeepSeek.

**Gate 02 aprovado por Codex: liberar 03.** Isto comprova invariantes matemáticos; balanceamento empírico/PvP, gameplay de cena, QA físico e stress continuam pendentes. Nova arte/animação não produzida em 02.
