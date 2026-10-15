# Validação da preparação

`DungeonBuild.Prepare`: CLI retornou success:true; `DUNGEONBDX_PREPARE_OK heroes=28`.

Corrigidos dois problemas de preparação: TMP ausente no projeto-base (37 arquivos originais de TMP Essential Resources restaurados no módulo, com metas/licenças); Unity não permite criar cena aditiva quando batch começa numa cena sem nome. Em batch a preparação cria a cena própria Single; no Editor interativo permanece aditiva. EditorBuildSettings e a cena Arena não foram alterados.

O importador canônico e sua API interna foram tentados, mas não materializaram os arquivos neste batch. Recursos vieram do pacote oficial já instalado, sem alteração dos bytes/GUIDs. Prepare agora exige esses recursos versionados, evitando importações imprevisíveis durante build. O relatório inicial 9/10 foi substituído pela bateria integrada após as correções; aguardar o resultado real em respostas/01.
