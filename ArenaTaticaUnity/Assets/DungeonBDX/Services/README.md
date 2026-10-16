# DungeonBDX — persistência offline v1

`CampaignJournal` é a única entrada de comandos da sessão local. `Cast`, `Move` e `EndTurn` registram apenas comandos aceitos. Alterar diretamente `journal.Session` invalida a exportação. Seed, cenário, configuração do mapa, flags da campanha, perfis e comandos recompõem turnos, RNG, cooldowns, invocações e limites pelo motor atual.

O formato inclui versões do combate/skills/perfis e exige mapa v1 com as dimensões esperadas pela fábrica (padrão 11 × 11). A fábrica deve gerar exatamente o mapa dessas configurações; o armazenamento não gera mapas. Arquivos de outra versão são rejeitados, sem migração implícita. Limites: 32 heróis iniciais, 4.096 comandos, 1 MiB de journal JSON UTF-8. Ao atingir o limite, a interface deve encerrar/pausar a campanha em vez de descartar seu histórico.

`LocalCampaignStore(Application.persistentDataPath + "/DungeonBDX", "local")` mantém um slot por perfil offline. `Load` retorna `null` apenas se não existem save nem backup; corrupção de ambos gera erro e preserva os arquivos. O chamador mantém `StoredCampaign.Revision` e fornece essa revisão e um ID de operação novo em cada `Save`. Repetir a mesma operação e conteúdo devolve a revisão já gravada; conteúdo diferente ou revisão antiga gera `SaveConflictException`.

A escrita usa lock exclusivo, arquivo temporário com flush e substituição atômica, conservando a versão anterior como backup; o primeiro save também recebe uma cópia de backup. Um contador durável separado impede reutilizar revisões após recuperar um backup; interrupções podem deixar lacunas na numeração. Uma interrupção antes da substituição conserva o save confirmado; um primário danificado recupera o último backup válido. Ao recuperar o backup, a próxima escrita conserva esse backup. Nenhum arquivo corrompido é apagado ou sobrescrito silenciosamente quando não há histórico válido.

SHA-256 e replay detectam corrupção/divergência local. Não constituem autenticação, autorização multiplayer, anti-cheat, criptografia ou cloud save. Não há senhas, tokens ou PlayerPrefs neste módulo.
