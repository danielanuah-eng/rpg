# DungeonBDX — prévia de seleção

Módulo independente sobre ArenaTaticaUnity. Abra `Scenes/DungeonBDX.unity` e entre em Play; também disponível pelo build `Builds/DungeonBDX/DungeonBDX.exe`. A cena Arena e seu combate permanecem independentes.

Abas: Heróis, Campanha, Sessão. Clique/toque na carta para consultar; Recrutar/Remover monta o grupo local de até três. Setas percorrem todos os heróis e todas as skills, inclusive as seis do Damian. Ações inferiores consultam a ficha de origem; ainda não executam combate. Módulos da campanha conservam a seleção enquanto a prévia está aberta. LERP usa tempo não escalado; controles reagem a mouse, foco e toque. Safe area e layout respondem à orientação.

Paleta provisória: carvão, bronze e vinho (a referência citada ainda não foi anexada). Domínio 02–03 concluído: núcleo determinístico com caps, 18 skills executáveis, adaptador explícito de 28 heróis e três perfis novos. Integração na cena/HUD, assets finais, contas/salas/convites, saves e mapas seguem 04–06 em `coordenacao/dungeonbdx/PIPELINE.md`. A aba Sessão informa que os serviços ainda não estão disponíveis.

`Combat/` e `Skills/` são assemblies sem UnityEngine; `DungeonSkillSession` executa os comandos com orçamentos compartilhados por dono/ação e resultados imutáveis para animação/SFX. Contratos em `coordenacao/dungeonbdx/contratos/`; guia da API e limitações em `coordenacao/dungeonbdx/entregas/03-skills/README.md`. Exportar contratos das DLLs compiladas com `pwsh -NoProfile -File Tools/export-dungeonbdx-contracts.ps1`. Nenhum modelo/SFX/BGM final é incluído por esses bindings.

CLI oficial já instalada; execute do projeto Unity:

```powershell
.\Tools\dungeonbdx.ps1 -Action Prepare
.\Tools\dungeonbdx.ps1 -Action Test
.\Tools\dungeonbdx.ps1 -Action Windows
```

Fontes e recursos TMP oficiais estão incluídos no módulo, com GUIDs e licença da fonte preservados. O build aponta explicitamente para a cena DungeonBDX; não troca a cena padrão da Arena. Builds/Artifacts são saídas locais ignoradas pelo Git. Prévia Windows é Development Build; captura automática de QA aceita `-dungeonbdx-capture C:\caminho\previa.png`, com `-dungeonbdx-tab Campaign` opcional, e só existe em Editor/Development.

QA usa renderização direta URP para textura (janelas ocultas não forneciam framebuffer útil). As capturas são da UI Unity real. Teste de retrato foi limitado pelo desktop a 900x1080; dispositivos móveis físicos permanecem pendentes. Após um build completo, alterações exclusivamente de código podem usar `unity build . --target StandaloneWindows64 --execute-method DungeonBuild.WindowsScriptsOnly`. Mudanças de recursos/cena exigem o build completo.
