# DungeonBDX — expedição local

Abra Scenes/DungeonBDX.unity em Play ou execute JOGAR-DUNGEONBDX.cmd na raiz Unity após o build. Na aba Sessão, escolha Cripta de Bronze, Biblioteca do Crepúsculo ou Ninho do Eclipse. Sem recrutados, a equipe é Lyra, Orion e Vaelis; a seleção oferece também os 28 heróis adaptados da Arena.

Escolha uma habilidade no HUD inferior e toque/clique no alvo. MOVER custa 1 PA e respeita caminho, ocupação e lentidão. ENCERRAR TURNO avança equipe e adversários. A ficha mostra valores e loadout DungeonBDX; os dados de origem permanecem preservados no catálogo. Críticos, armadura, dano, cura e escudo usam o domínio das etapas 02–03.

GIRAR ‹/›, ZOOM +/− e RESET funcionam no PC e no celular. PC: roda do mouse aproxima, botão direito arrastado gira e botão central arrastado desloca. Touch: pinça aproxima, torção com dois dedos gira e arrasto com dois dedos desloca. RESET retorna ao tabuleiro inteiro. Gestos e passagem pelo HUD cancelam o toque de ação; um toque só é confirmado ao soltar.

Ao selecionar MOVER, casas alcançáveis mostram distância pelo caminho livre (verde). Skills mostram alcance com visão (azul), alvos elegíveis (âmbar) e distância; comandos de ataque do companheiro partem do dragão. Teleporte segue suas regras próprias. O preview não consome AP/RNG; o motor ainda valida recargas e usos. Falhas de ação permanecem visíveis junto ao alcance.

Heróis importados agora reproduzem as sub-sprites reais da Arena: idle, caminhada, ataque/conjuração, reação e morte, conforme os clips disponíveis. Direção e pivô dos pés acompanham a câmera. Os FBX nativos continuam sem rig; recebem movimento procedural leve de apresentação.

Cada comando aceito salva o progresso local. RETOMAR recompõe turnos, RNG, recargas, buffs e companheiro pelo journal. Há backup, revisões e exclusão entre escritores. Falhas interrompem novas ações até recuperação. Limite desta versão: 4.096 comandos por expedição. Pasta: Application.persistentDataPath/DungeonBDX/Campaigns/local. Senhas não entram nesse save.

Fogo amigo e encontros com dragões são módulos disponíveis; objetivos especiais e armadilhas aparecem como preparação. Três mapas por seed, salas/corredores conectados, visão integral, câmera com safe area, modelos originais e LERP por casas. Os 18 SFX e três ambientes musicais são síntese original provisória. Fontes Blender/Python: Tools/GenerateDungeonAssets; procedência/hashes: Art/manifest.json. Arte high-end, rigs, LODs, texturas finais e animações particulares permanecem pendentes.

CONTAS & SALAS abre o cliente real Unity Authentication/MPS: login/cadastro por senha, salas privadas e entrada por código. Só conecta após ação explícita. O provedor Username & Password precisa estar configurado no Unity Cloud; funcionamento cloud não foi testado nesta entrega. A sala gerencia membros; **combate multiplayer, convites direcionados e saves cloud ainda não estão implementados**. APIs online exigem autorização real. O SDK gerencia seus tokens de sessão; DESCONECTAR pede sua limpeza.

CLI oficial, a partir do projeto Unity:

```powershell
.\Tools\dungeonbdx.ps1 -Action Prepare
.\Tools\dungeonbdx.ps1 -Action Test
.\Tools\dungeonbdx.ps1 -Action Windows
```

Builds/Artifacts são saídas locais ignoradas pelo Git. QA somente Editor/Development: -dungeonbdx-play cripta-de-bronze -dungeonbdx-qa -dungeonbdx-smoke -dungeonbdx-capture C:\saida\partida.png. O perfil QA é separado. O smoke exercita ações, escudo, invocação, save, replay e projeção do tabuleiro; grava JSON ao lado da imagem. Acrescentar `-dungeonbdx-presentation rotate` (ou `range`/`attack`) verifica câmera, preview, feedback de rejeição e avanço real idle→attack→idle, preservando o estado do domínio; gera também `.presentation.json`.

Tema provisório carvão/bronze/vinho; referências Arcadia Quest fornecidas pelo autor analisadas em `coordenacao/dungeonbdx/referencias/arcadia-quest/`, com cobertura e diferenças do protótipo explícitas. Android nativo, desempenho mobile e QA físico pendentes. Licenças TMP preservadas. Evidências anteriores: `coordenacao/dungeonbdx/entregas/04-06-local/`; câmera/alcance/animação: `entregas/07-camera-alcance-animacao/` na mesma coordenação.
