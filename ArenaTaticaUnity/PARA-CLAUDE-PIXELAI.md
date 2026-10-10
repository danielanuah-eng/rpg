# Assets finais do Codex — Arena Tática, 10/10

Entrega pronta: `../coordenacao/arena-pixelai/entregas/LEIA-ME-CLAUDE.md` e `plano-de-importacao.json`.

Li seus recebimentos 003, 005-006 e 004-007: integração inicial, ARENA_PREPARE_OK e 24 testes confirmados pelo seu registro. Este pacote é posterior.

- Damian pistol SE agora é **1512×168, 9 frames**. Substituir a tira provisória de 172; PixelLab canônico/PNG final iguais, outras 30 direções do Damian preservadas.
- Aplicar pivôs registrados de **284 clipes** em `auditorias/sincronia-2026-10-10/ancoras-para-claude.json`: **127** origens diferem do snapshot. Tarefa 009; um pivô por clipe, sem normalizar cada passo/partícula.
- **Loja completa animada:** copiar `entregas/loja-v2/cenario/frame-01..09.png` (384×216) para `Resources/Sprites/Vendedor/`, mantendo nomes/GUIDs; 5 FPS. O ArenaHud atual substitui a imagem da cena inteira, não compõe overlay. Por isso os quadros completos, em vez dos recortes 160×128, mostram o cenário todo. Fonte inicial vendedor/ não é final. Dez ícones já integrados.
- Novos efeitos em eventos-cenarios/efeitos; vínculos de nove skills/26 eventos. Neve usa neve.png e Vulcão vulcao.png; cristal roxo não é Neve. Tarefa 010 fecha feedbacks ainda só-log e duração completa do hit.
- **008 pronto:** UUID **215bb320-de53-44a1-bec7-f2e23f4e7265**, pasta `entregas/tchu-tcha-berserk-possuido/sprites/`: **48 PNGs, oito vistas + 40 clipes/344 frames**, cinco ações em oito direções reais. Atualizar seu ID da pasta TchuTchaBerserk e usar os pivôs/FPS de berserk.visual.json. Não importar estados antigos ou attack-refinado-sombras do ZIP. Todos os 32 clipes não-death retornam à referência; death permanece caído.

Claude continua responsável pelo código, integração, teste no Unity e upload. Codex preservou Assets/Tools/código. Manifestos/hashes/alfa/recuperação foram verificados; nova forma preservou 476 PNGs dos estados anteriores/vistas/death. Integração final e avaliação do autor ainda não confirmadas.
