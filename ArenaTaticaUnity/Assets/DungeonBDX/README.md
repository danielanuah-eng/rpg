# DungeonBDX — aventuras de guildas

A entrada padrão agora é **AVENTURAS / GUILDAS & HERÓIS / REGRAS**. Execute `JOGAR-DUNGEONBDX.cmd` na raiz Unity depois de gerar o build. Escolha entre Portais em Ruínas, Praça dos Quatro Emblemas e Resgate nas Catacumbas; configure 2–4 guildas locais, cada uma com três heróis, e inicie ou retome o cenário.

O modo Arcadia usa nove tiles de 3×3 espaços, ocupação compartilhada, três pontos de movimento, um ataque com carta exaurida ou descanso da guilda. Guarda e Vingança são controladas pelo rival à direita; a reação automática é uma opção explícita em REGRAS. Há 31 habilidades naturais distintas. Os limites de crítico, mitigação, dano e cura continuam obrigatórios.

Vista **2D isométrica** (contrato em `coordenacao/dungeonbdx/contratos/ISOMETRICO-2D.md`): câmera ortográfica a 30°, giro de 360° que repousa em 45°+90°·k, arrasto preso ao chão; figuras em pé com os pés (pivô 0,153 da Arena) na casa e ordem de desenho pelos pés. Sobre as figuras fica só a vida; nomes ficam no HUD.

O HUD inferior compacto reúne heróis, cartas, habilidade natural, portas, descanso e encerramento do turno; GIRAR/ZOOM/REPOR ficam numa coluna à direita do tabuleiro. Selecione uma carta para ver distâncias/visão; selecione MOVER para ver custo pelo caminho. PC: roda aproxima/afasta, botão direito gira, botão central desloca. Touch: pinça, torção e deslocamento com dois dedos.

O Colosso usa altura própria (2,4 contra 1,5; a escala mede o quadro animado, não o estático) e ocupa a casa inteira. Figuras comuns dividem a casa em duas posições visuais. Golpes usam os clips correspondentes da Arena quando disponíveis; a Vingança mortal termina antes da morte visual. Pisos, salas, portas, mobiliário, portais e marcadores diferenciam os três cenários.

Cada ação aceita salva em `Application.persistentDataPath/DungeonBDX/Campaigns/Arcadia-v1`. Há replay determinístico, backup, escrita atômica, exclusão entre escritores e revisões monotônicas. Falhas de persistência suspendem novas ações. Saves da expedição anterior permanecem separados.

Referências, exceções e lacunas: `coordenacao/dungeonbdx/entregas/08-configuracao-arcadia/REFERENCIAS-E-DECISOES.md`. Esta base **não representa equivalência completa** a todas as regras, aventuras e expansões AQ. Campanha entre cenários, mercado, maldições, multiplayer sincronizado e acabamento final ainda estão pendentes. O cliente de contas/salas anterior continua acessível pelo modo legado (`-dungeonbdx-legacy`).

QA nativo Editor/Development: `-dungeonbdx-aq portais-em-ruinas -dungeonbdx-qa -dungeonbdx-capture C:\saida\arcadia.png`. O probe usa armazenamento temporário próprio e verifica ações reais, replay, animações, escala e câmera; grava JSON junto da captura. `-dungeonbdx-aq-menu` captura a seleção de aventuras. Não substitui QA físico Android.

## Expedição anterior — compatibilidade (`-dungeonbdx-legacy`)

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
