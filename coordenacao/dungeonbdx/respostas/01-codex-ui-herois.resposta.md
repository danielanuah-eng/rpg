# 01 — UI e heróis

**Status:** CONCLUIDO
**Escopo entregue:** prévia funcional da etapa 01, sem alegar conclusão do DungeonBDX completo. Paleta provisória; imagem de referência não anexada.

## Entrega

- `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\ArenaTaticaUnity\Assets\DungeonBDX\`: runtime, cena própria, scripts de build, testes e recursos/licenças TMP.
- `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\ArenaTaticaUnity\Tools\dungeonbdx.ps1` e `JOGAR-DUNGEONBDX.cmd`.
- Executável local: `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\ArenaTaticaUnity\Builds\DungeonBDX\DungeonBDX.exe` (saída ignorada no Git).
- Capturas: `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\entregas\01-ui\`.

28 heróis jogáveis importados por ID com retratos, atributos e todas as skills de origem; invocações excluídas do seletor. Cartas próprias, recrutamento local até três heróis, módulos da campanha, abas Heróis/Campanha/Sessão e HUD inferior centralizado. Paginação inclui as seis skills do Damian. UI mouse/toque com hover/foco/press/seleção por LERP não escalado, transição do painel e safe area. Ações consultam a ficha; não executam combate.

## Validação

- Unity CLI oficial 1.0.0-beta.12; Editor 6000.6.4f1; Pipeline 0.8.0-exp.1 já instalado no projeto.
- Preparação da cena própria: sucesso, `DUNGEONBDX_PREPARE_OK heroes=28`.
- Bateria integrada EditMode após câmera dedicada: **165/165**, zero falhas.
- Reverificação do módulo após QA/captura e ajustes visuais: **11/11**, zero falhas.
- Build Windows completo aprovado; atualização final somente de scripts aprovada (dados/cena mantidos do build completo). Pacote de aproximadamente 1,60 GB por herdar Resources da Arena.
- Três telas legíveis renderizadas diretamente pelo Canvas/URP. PC 1600x900, retrato 900x1080 (limitado pelo desktop), Campanha 1600x900. Comparação direta e QA DeepSeek; correções e falsos positivos registrados na nota de QA.
- Sem exceções/erros nos três logs finais do player. A captura de framebuffer de janela oculta era vazia; foi substituída por RenderRequest para textura, só no código Editor/Development.
- Cena Arena, motor, pacotes e EditorBuildSettings preservados. Hash de ProjectSettings igual ao snapshot antes do build (incluindo alterações preexistentes do autor).

## Limites e próximo gate

Serviços de contas/salas/convites, salvamento contínuo, core de combate/caps, novas classes/skills, mapas procedurais e música final ainda não implementados: seguem 02–06. Sessão informa explicitamente indisponibilidade dos serviços nesta prévia. Não há login/presença exigidos para o setup offline. Android/iOS físicos e stress-test pendentes do autor; não alegar validação nativa.

**Gate funcional 01 liberado para Claude executar 02.** Etapas 03–06 mantêm suas dependências. Claude/Antigravity recebem tarefas por arquivos; ativação externa não confirmada. Código isolado na branch `codex/dungeonbdx-step1`; publicação e integração registradas na nota Git final.
