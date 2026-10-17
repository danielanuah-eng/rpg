# DungeonBDX — retomada pelo Claude

**Parada solicitada pelo autor em 16/10/2026. Implementação não concluída, sem commit/push desta meta.** Não recomeçar o projeto nem repetir a leitura integral das referências. O build iniciado terminou com sucesso; apresentação visual ainda não validada. Aplicar a nova direção 2D isométrica abaixo.

## Atualização prioritária — NanoBanana / Gemini

Direção posterior do autor: **toda nova geração de imagens/assets visuais usa NanoBanana no Gemini, substituindo PixelLab**. GPT/Codex encaminha os pedidos ao Gemini pela UltraBridge, usando a skill empresa. Se não houver acesso/geração efetiva, entrega **todos os prompts completos necessários**, por asset/variante/clip, prontos para uso manual. Reaproveitar os assets Arena existentes. Não declarar imagens prontas, mensagem recebida ou animações completas sem evidência.

Comando encaminhado ao GPT: `tarefas/07-gpt-nanobanana-gemini.md`; aviso comum: `notas/20261016_codex_para_todos_nanobanana.md`. Claude continua responsável pela programação e integração 2D isométrica. Blender é auxiliar para referências, sem substituir NanoBanana na geração nova.

## Pedido e decisões vigentes

- Fazer o jogo seguir Arcadia Quest: regras, posicionamento das fichas, aventuras/mapas, seleção de partidas e HUD inferior. Melhorar chão/cenários, aumentar Colosso, importar animações reais da Arena e diferenciar habilidades dos heróis.
- Autor confirmou: **manter limites de balanceamento; seguir AQ no restante**. Críticos não recursivos e mitigação limitada continuam obrigatórios.
- PC/celular: girar, aproximar e deslocar tabuleiro; mostrar distância ao selecionar movimento/cartas.
- Codex assumiu etapas 2–3; Antigravity não é usado. Não esperar agentes externos.
- Contexto maior ainda pendente: multiplayer sincronizado, convites, campanha/cloud saves e assets finais. Salas/contas anteriores não equivalem a combate online.

## Nova direção visual do autor — prioridade na retomada

**DungeonBDX terá perspectiva 2D isométrica. Claude ajusta a apresentação na Unity.** A aparência tem profundidade de tabuleiro, mas heróis e monstros são sprites planos. Esta direção substitui o acabamento do cenário atual com personagens/props 3D como destino final; modelos existentes podem servir de fonte para renderização de sprites. O motor de regras já implementado permanece.

### Divisão de produção

| Responsável/ferramenta | Entrega |
|---|---|
| Claude / Unity | Projeção isométrica, grid/picking, profundidade/oclusão, animação direcional, câmera PC/touch, integração de UI/assets, combate e testes |
| GPT/Codex → UltraBridge → Gemini/NanoBanana | Tiles/texturas dos quadrados, pisos, paredes, portas, objetos, fichas/cartas, molduras, botões/menus; sprites planos de heróis/monstros e animações quando a ferramenta e as referências permitirem |
| Blender | Ferramenta auxiliar offline para preparar referências dos modelos existentes. A nova geração visual será feita pelo Gemini/NanoBanana, conforme direção posterior do autor |
| Autor | QA visual, jogabilidade e stress em PC/celular |

**Reaproveitar primeiro:** os sprites e golpes importados da Arena, identidades dos 31 heróis e conceitos já aprovados. Priorizar idle, walk, attack/cast, hit e death; manter clips especiais como pistol, hammer, summon e teleport quando existentes. Os três FBX nativos ainda não têm rigs: não declarar prontas suas animações em oito direções. Registrar estados/direções ausentes e usar fallback explícito até a entrega correspondente.

NanoBanana não fica limitado à interface: também produz os tabuleiros e personagens 2D solicitados. Pisos podem ser opacos; personagens, recortes, molduras e elementos sobrepostos usam transparência real. Preservar identidade dos heróis, variedade das habilidades e a escala maior do Colosso.

### Contrato visual antes de produzir lotes

Claude define e registra a projeção única (ângulo/proporção do losango), tamanho-base do tile, pixels por unidade, escala por personagem, pivô nos pés, FPS/duração por clip e margem de recorte. GPT/Codex prepara os pedidos segundo esse contrato e encaminha ao Gemini/NanoBanana pela UltraBridge. **Não criar um segundo grid ou motor de combate.**

- Manter coordenadas lógicas quadradas `AqCell(X,Y)`; a projeção em losangos é apresentação. Movimento continua ortogonal pelas regras; oito direções visuais não liberam movimento diagonal.
- Aproveitar `AqBoardView`, `DungeonActorAnimation`, `DungeonCameraInput`, `AqMatch` e `AqMatchMenu`. Claude pode substituir a geometria visual por tiles/sprites sem mudar saves, missões, ocupação, RNG ou caps por causa da arte.
- O código atual reconhece as direções `south`, `south-east`, `east`, `north-east`, `north`, `north-west`, `west`, `south-west`. Manter o padrão dos sub-sprites da Arena, como `idle_south_01` e `attack_north-east_03`; ID do personagem fica na pasta/manifesto. Evitar renomear toda a biblioteca para nomes em português.
- Ao girar o tabuleiro, recalcular a direção aparente dos sprites em relação à câmera e a ordem de desenho pela posição dos pés. Paredes/objetos precisam de vistas compatíveis; girar apenas uma imagem achatada do tabuleiro não resolve a rotação solicitada. Claude define a implementação preservando zoom, rotação e deslocamento no PC/touch.
- Duas figuras comuns continuam na mesma casa com posições legíveis; Colosso ocupa dois lugares e tem escala própria. Clique, preview de alcance, paredes/portas e elementos altos devem respeitar a profundidade visual.
- Cada entrega de arte deve ter manifesto com ID, origem, caminho, hash, projeção, dimensões, pivô/PPU, animação, direção, frames/FPS, loop e evento de impacto quando aplicável. Claude valida e integra; arte ainda pendente não deve aparecer como existente.
- Cartas mostram dados reais do herói/equipamento. Vida, defesa, custo, alcance, texto e números ficam em texto/UI da Unity sobre as molduras, para atualização e legibilidade; não embutir valores mutáveis nos PNGs.

### Comandos adaptados para passar ao Claude

**1 — Retomar e estabelecer a apresentação isométrica**

> Leia este handoff e continue o DungeonBDX existente na Unity, no worktree indicado. O autor definiu perspectiva 2D isométrica; você ajusta câmera, projeção, grid visual, profundidade e picking. Preserve AqBoard/AqSession/AqJournal, regras AQ com caps obrigatórios e saves. Reaproveite os sprites/atlases da Arena e DungeonActorAnimation para oito direções relativas à câmera, mantendo os pés fixos no tile. Preserve rotação/zoom/deslocamento PC e touch, duas figuras por casa e escala própria do Colosso. Registre um contrato visual para Codex/NanoBanana/Blender e liste só os assets realmente ausentes. Não reinicie o projeto e não suponha que todos os sprites novos já estão prontos.

**2 — Organizar a produção dos assets**

> Prepare pedidos de arte por ID e cenário, usando o contrato visual. GPT/Codex encaminhará ao Gemini pela UltraBridge/skill empresa os pedidos de NanoBanana para tiles/texturas, cenário, fichas/cartas/UI e sprites planos dos heróis/monstros. Se a ponte/geração não estiver disponível, entregará TODOS os prompts necessários, completos e organizados por asset, prontos para uso manual no Gemini. Blender poderá preparar referências dos modelos úteis; os novos sprites serão solicitados ao Gemini/NanoBanana, com vistas/animações padronizadas. Reaproveite primeiro as artes, golpes e identidades existentes da Arena; priorize corrigir o chão, as proporções e os estados ausentes. Cada pedido deve indicar referências, tamanho, transparência, pivô, direção, clip e função no jogo. Integre uma amostra de tile, personagem e carta antes de ampliar o lote.

**3 — Integrar UI, alcance e combate existentes**

> Integre molduras, fichas e botões produzidos no NanoBanana ao menu e ao HUD inferior já existentes. Selecionar o herói mostra seus dados/habilidade; selecionar sua carta de ataque mostra alcance/linha de visão e distância no grid; Atacar, Mover e Encerrar turno emitem os comandos existentes de AqMatch/AqSession. Preserve exaustão, descanso, Guarda, Vingança, Extermínio e habilidades individuais. Use os clips reais dos golpes e feedback de ferimentos, bloqueio, cura e morte. Salve a ação aceita antes do feedback visual. Não crie cálculo de combate paralelo na UI.

**4 — Validar e concluir a entrega**

> Verifique primeiro o resultado do build que já estava rodando. Depois de integrar a apresentação isométrica, rode os testes e probes/capturas descritos neste handoff. Confira telas PC e vertical, oclusão, seleção de duas figuras na mesma casa, Colosso, portas, rotação com mudança de direção dos sprites, idle/caminhada/golpe/vingança/morte e replay do save. Compare os cenários com os diagramas estudados. Registre limitações reais e só faça commit/push quando a meta estiver validada. Multiplayer, campanha completa e arte final continuam pendentes até implementação própria.

## Onde estão os arquivos

Raiz: `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg`

Worktree de autoria: `tmp/dungeonbdx-step1-git`, branch `codex/dungeonbdx-arcadia-foundation`, base `1a861c94`. Projeto executado/testado: `ArenaTaticaUnity` na **raiz principal**.

**Editar primeiro no worktree e sincronizar somente arquivos próprios para o projeto principal.** Há muitas alterações concorrentes fora deste escopo; nunca usar `git add .`, reset/clean ou restaurar toda a árvore.

- Lista exata sincronizada: `tmp/dungeonbdx-aq-owned.txt` (22 arquivos incluindo reserva).
- Sincronizador: `tmp/dungeonbdx-aq-sync.py`; copia `.cs/.asmdef/.md` alterados/novos do módulo WT→main. Não copia automaticamente documentos de coordenação, imagens nem `.meta`.
- Reserva anterior às edições C#: `coordenacao/arena-pixelai/notas/2026-10-16_codex-dungeonbdx-arcadia-foundation.md`, em ambas as árvores.
- **Somente WT, ainda não sincronizados:** atualização de `ArenaTaticaUnity/Assets/DungeonBDX/README.md` e `coordenacao/dungeonbdx/entregas/08-configuracao-arcadia/REFERENCIAS-E-DECISOES.md`.
- Novos `.meta` gerados pelo Unity principal precisam ser preservados/copiados ao consolidar a entrega; não gerar GUIDs diferentes no WT.

## Implementado nesta meta

Todos os caminhos seguintes são relativos a `ArenaTaticaUnity/Assets/DungeonBDX/`.

| Arquivos | Estado/ responsabilidade |
|---|---|
| `Arcadia/AqBoard.cs` | Grade 9×9, nove tiles, paredes/portas, caminhos, LOS com exceção da quina; três topologias distintas e conectadas |
| `Arcadia/AqContent.cs` | 31 habilidades naturais distintas, atributos/armas/clip de golpe próprios; cinco equipamentos iniciais por guilda |
| `Arcadia/AqSession.cs` | Regras puras determinísticas: 2–4 guildas×3 heróis, ocupação 2, Colosso enorme, movimento 3+ataque 1 ou descanso, exaustão, Guarda/Vingança/Extermínio, exploração/missões, ressuscitar/redistribuir, entrada de monstros por símbolos, vitória por missões |
| `Arcadia/AqJournal.cs` | Journal/replay independente do modo antigo; saves antigos preservados |
| `Runtime/AqLocalSave.cs` | Slots por cenário, digest+replay, escrita atômica/exclusiva, backup, CAS, revisão monotônica e rejeição de cenário trocado |
| `Runtime/AqHeroImport.cs` | Sprites Arena, alturas próprias (Colosso 2,4; comuns geralmente 1,5), grupos iniciais e retratos |
| `Runtime/AqBoardView.cs` | Pisos/texturas procedurais, salas/mobiliário, portas animadas, tokens, duas posições por casa, câmera, preview de distâncias, sprites e feedback |
| `Runtime/AqMatch.cs` | Liga motor→save→animação→HUD; comandos só prosseguem após salvar; controles de reação; probe nativo com ações reais |
| `Runtime/AqMatchMenu.cs` | Entrada padrão: AVENTURAS / GUILDAS & HERÓIS / REGRAS, seleção/retomada e 2–4 guildas |
| `Runtime/DungeonActorAnimation.cs` | `PlayClip` para golpes específicos da Arena, mantendo fallback |
| `Runtime/DungeonCameraInput.cs` | Entrada genérica reutilizável por ambos os modos; mouse/touch |
| `Runtime/DungeonShell.cs` | Abre novo menu por padrão; flags QA/menu e `-dungeonbdx-legacy` para expedição/contas anteriores |
| `Runtime/DungeonWorldView.cs` | Alturas corretas também no legado |
| `Tests/Arcadia/*`, `Tests/Editor/AqPersistenceTests.cs` | Regras, mapas, combate/habilidades/caps e persistência; asmdefs atualizados |

Cenários: `portais-em-ruinas`, `praca-dos-quatro-emblemas`, `resgate-nas-catacumbas`. São **adaptações DungeonBDX**, não cópias integrais das aventuras oficiais. Praça: centro aberto/alas; catacumbas: câmaras/rotas externas; Portal teve 17 casas isoladas corrigidas.

Correções importantes já feitas: ataque após porta gratuita encerra a etapa de movimento; mortes/recompensas não duplicam; espinhos podem matar o monstro durante Guarda; entrada compara dois símbolos e exige casa completamente vazia; uma guilda não recolhe todas as relíquias empilhadas. Saves de backup recebem revisão nova, invalidando escritores antigos.

## Validação real e ponto exato de parada

**361/361 testes EditMode aprovados, zero falhas**, em `ArenaTaticaUnity/Artifacts/dungeonbdx-aq-fifth.xml` (16/10, 23:08:29–23:08:42 UTC). Incluem os testes de porta gratuita e entrada em casa ocupada. Baseline anterior: 286 testes.

Depois dessa bateria houve ajustes de apresentação/QA (marcadores legíveis, separação de tokens, escala real no probe, morte após golpe de Vingança, captura do menu e perfil QA temporário). **Esses ajustes ainda precisam de verificação final**; não tratar os 361 testes como comprovação visual do estado final.

O **build Windows scripts-only concluiu com sucesso**, usando `DungeonBuild.WindowsScriptsOnly`: retorno CLI `success:true` e `DUNGEONBDX_WINDOWS_OK bytes=1601983356` (resultado conferido ao atualizar este handoff). Isso confirma compilação/build, não o funcionamento visual:

- Log de controle: `tmp/dungeonbdx-aq-build.log`.
- Log Unity: `ArenaTaticaUnity/Logs/build-StandaloneWindows64-1792192162448.log`.
- Sessão original do terminal Codex: `84081`; o log confirma encerramento bem-sucedido do Unity.
- Executável previsto: `ArenaTaticaUnity/Builds/DungeonBDX/DungeonBDX.exe`.

As capturas **ainda não foram executadas**. Script pronto: `tmp/dungeonbdx-aq-captures.ps1`. Produz três partidas (PC e vertical) e menu em `coordenacao/dungeonbdx/entregas/08-configuracao-arcadia/`. Probe valida ações reais, avanço idle, frames de ataque, escala renderizada do Colosso, ocupação compartilhada, rotação/zoom e replay do save. Não aceitar JSON com `passed:false`, arquivo antigo ou apenas captura sem verificar aparência.

## Próximos passos, em ordem

1. Usar o build aprovado como ponto de partida e aplicar a direção 2D isométrica. Se o executável não carregar novos recursos/assemblies, fazer build completo `DungeonBuild.Windows`.
2. Executar `tmp/dungeonbdx-aq-captures.ps1`; abrir as imagens e corrigir legibilidade, piso, dimensões dos personagens, sobreposições, enquadramento e HUD. Os novos visuais ainda não foram vistos em execução.
3. Conferir especialmente animação de Vingança mortal: `AqBoardView.Sync(..., dyingReaction)` mantém o monstro, `deathAfterAttack` posterga a morte; verificar que o golpe é realmente exibido. Conferir se objetos/luzes do shell legado interferem na cena nova.
4. Rodar a bateria EditMode final após as últimas correções. Registrar resultado e capturas reais; QA vertical Windows não é QA em aparelho Android.
5. Consolidar README/REFERENCIAS-E-DECISOES, criar relatório `08.../README.md` e `validacao.json`; atualizar `coordenacao/dungeonbdx/PIPELINE.md` e `estado.codex.json` (ainda descrevem o modo anterior).
6. Conferir diff somente dos arquivos próprios, copiar `.meta` pertinentes, preservar alterações concorrentes. Commit/push automático **somente quando esta entrega estiver completa e validada**, conforme autorização vigente. Ainda não houve commit desta meta.

Lacunas funcionais assumidas: campanha encadeada/mercado/maldições da morte, catálogo completo de monstros/itens/eventos e expansões, combate online, convites/cloud saves, rigs dos três FBX nativos e acabamento high-end. Não declarar “100% igual AQ” nem pipeline inteiro concluído.

## Comandos e cuidados operacionais

```powershell
# Na raiz Rpg; Python disponível:
& 'C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' tmp/dungeonbdx-aq-sync.py
# Depois de confirmar que o build anterior encerrou:
& 'C:/Users/Anuah/AppData/Local/Unity/bin/unity.exe' test "$PWD/ArenaTaticaUnity" --mode EditMode --output "$PWD/ArenaTaticaUnity/Artifacts/dungeonbdx-aq-final.xml" --timeout 600 --format json
# Capturas nativas:
& ./tmp/dungeonbdx-aq-captures.ps1
```

- CLI oficial instalado; Editor `6000.6.4f1`. IPC/test/build/Git remoto precisam das permissões usuais do ambiente. Não contornar bloqueios de aprovação.
- `Library/ilpp.pid`: remover somente se o PID registrado não existir. Não matar processos Unity de outros projetos.
- Cache Burst apresentou corrupção; foi **movido, preservado**, de `ArenaTaticaUnity/Library/BurstCache` para `tmp/dungeonbdx-burst-cache-corrupt-20261016`. Bateria seguinte passou. Não repetir limpeza sem causa nova.
- Unity pode remover `;SENTIS_ANALYTICS_ENABLED` de ProjectSettings. Snapshot anterior: `tmp/dungeonbdx-aq-settings-before.asset`; restaurar apenas alteração automática conhecida, nunca sobrepor mudanças concorrentes.
- **Não imprimir/gravar cabeçalhos integrais de logs Unity ou argumentos de processos:** podem conter tokens temporários do Hub. Filtrar erros/resumos. Não versionar logs crus.
- AGENTS: investigação grande via DeepSeek **OpenRouter**, nunca NVIDIA; escrita DS só WT. Auditoria já feita: `tmp/dungeonbdx-aq-code-audit-network.log`; geração visual: `tmp/dungeonbdx-aq-view.log`. Não repetir essas leituras caras. Não enviar KrosArcano/documentos pessoais ao DS.
- Subagentes de mapas/persistência concluíram; agente de testes deixou `AqCombatTests.cs` completo antes do limite de uso (testes passaram). Não depender de agentes ativos.

## Referências: o que já foi entendido

Estudo consolidado no documento `REFERENCIAS-E-DECISOES.md` do WT. Manual Inferno p.13–24/26; diagramas Base/Inferno de Um Portal Turbulento; mapa O Dia de Ação de Tacos p.2; folhas de campanha/inventário; FAQ oficial sobre LOS. Cache local em `tmp/dungeonbdx-reference-local/`, texto `tmp/dungeonbdx-aq-manual.txt`.

Limitações descobertas: PDF local Pets salta da página impressa 13 para 32, sem aventuras intermediárias; Riders Campaign local tem só duas folhas, não livro completo. Não afirmar leitura das páginas ausentes. Originais de KrosArcano não foram importados como assets nem enviados ao DeepSeek. Expansão TCG A Mestra dos Dragões e PNG frontal retirado de Ciantia continuam fora do escopo.
