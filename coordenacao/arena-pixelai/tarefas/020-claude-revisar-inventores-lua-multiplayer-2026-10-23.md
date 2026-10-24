# 020 — Claude: revisar entrega Inventores e Lua e corrigir bugs, com prioridade ao multiplayer

**De:** autor, por Carlos/Codex (pedido direto nesta conversa em 23/10/2026)
**Para:** claude
**Status:** 1ª rodada feita por Claude (23/10/2026): 5 bugs corrigidos, sem commit; multiplayer real ainda não validado. Ver `respostas/020-claude-revisao-inventores-lua-multiplayer.md`.
**Responsável:** claude
**Criada em UTC:** 2026-10-23T09:57:56.236947+00:00

## Autorização e objetivo

O autor pediu, depois da conclusão, um resumo de tudo que foi feito e uma solicitação a Claude para corrigir o código, avaliar e procurar bugs, principalmente no multiplayer e em qualquer comportamento que o atrapalhe. A entrega local foi concluída. Assuma esta revisão, registre a responsabilidade antes de editar e corrija bugs demonstráveis com regressões. Não é necessário pedir nova autorização para a revisão e as correções solicitadas. Não há evidência de recebimento/execução de Claude ainda: esta é a tarefa na ponte local.

Projeto principal: `C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/ArenaTaticaUnity`.
Cópia usada por Carlos: `C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/Artifacts/carlos-roster-2026-10-22/work/ArenaTaticaUnity`.
Registros: `C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/Artifacts/carlos-roster-2026-10-22`.
Ponte: `C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/coordenacao/arena-pixelai`.
Resposta esperada: `respostas/020-claude-revisao-inventores-lua-multiplayer.md` nesta ponte.

## O que foi entregue

- Quatro heróis: Nikola, Bifia, Desland e Nepturius. Invocações/atores: Blip, ModoBlipware, Celinos, esfera de Nepturius, peão/torre/cavalo/bispo/rainha (sem rei) e tubarão. Total: 14 rigs.
- 18 efeitos particulares, com ligações aos eventos, projéteis, cura, comando/invocação, clone, tabuleiro, radar, matriz, engrenagens e esfera. Total: 357 arquivos de sprites e 1.647 quadros. Manifesto inclui fontes, adaptações, dimensões, alfa medido e SHA256. Gerações restantes feitas no PixelLab; Nikola original foi preservado e conferido.
- Docas inundadas: piso de água com textura em movimento, custo dobrado para todos e mínimo de uma casa, passagens de madeira, armazém e evento de tubarões que rondam e mordem na água. Editor aceita água. Tabuleiro de Desland acompanha a altura de cada casa sem esconder os marcadores.
- HUD e fichas receberam descrições das novas regras e contadores. Engrenagens usam três marcadores de golpes, sem barra de PV. Radar é atualizado no deslocamento e ações.
- Importador Unity DataProvider próprio: slicing pelas coordenadas nativas, PPU 128, Point, sem compressão/mipmap, alfa e pivôs por arquivo, preservando GUIDs existentes. Sem editar metadados à mão.
- Compatibilidade de catálogo mínima no DungeonBDX: quatro IDs associados a perfis existentes e teste com contagem derivada. As regras novas da Arena não foram transferidas para o Dungeon.
- Protocolo da Arena/WireMessage passou de 13 para 14 com o novo estado/conteúdo. Clientes antigos não são compatíveis; conferir a negociação e os erros entre versões.

### Regras implementadas para revisar contra a intenção do autor

Nikola começa com os dois robôs autônomos. Funilaria gera duas engrenagens por turno; especial vale duas. Contador compartilhado; outros aliados recebem PM/PA temporários; inimigos destroem cada engrenagem com três golpes independentemente do dano. Comandos custam 3 PA, ressuscitam apenas o robô ausente; Blip vivo abre loja remota e Celinos vivo recebe marca global. Eletaser escala a cada duas engrenagens e reduz PA à metade com mínimo 1 (não cria PA a partir de zero). Proteção tecnológica bloqueia efeitos/controle externos; matriz tem vida própria, sem transbordar dano, expira no fim do próximo turno e permite saída por engrenagem. Robôs sobrevivem à morte do dono.

Celinos revela invisíveis, atira em até dois próximos e escala dano/PM/revelação a cada três engrenagens; marca tem prioridade, depois proximidade e vida. Blip alterna coleta de ouro/engrenagem, ganha PM por engrenagem e transfere os recursos ao Nikola; com dez vira Blipware controlável, com regeneração, melee 6 e puxão de até dez casas.

Bifia usa flecha de alcance 8 que rouba 1 PM. Sonus zera PM do aliado, cura pela metade dos PM restantes dela e tem recarga de um turno completo. Viva Luna consome o turno, é uma vez por partida, centro global/raio 6 e modifica iniciativa/PM de aliados e inimigos.

Desland: Roque em linha, primeiro alvo representa o rei; Xeque-mate escala com outros aliados adjacentes ao alvo; clone de aliado por um turno após matar, com recarga. Domínio dura dois turnos do dono, cinco peças de PV 1 atacam automaticamente em sequência após ele. Durante o domínio, cinco habilidades movem aliados/inimigos/peças pelos movimentos de xadrez. A forma `dominio` agora expõe essas skills e é removida ao expirar.

Nepturius começa com esfera invulnerável e imune. Drena PA/PM de quem inicia na área, inclusive aliados; mãos agarram até quatro e atordoam, loucura dá movimento e bloqueia outras skills no turno, explosão causa dano em área. Sem skill usada, cubo aleatório causa dano e cura o dono se acertar personagem. Ao dono ser atacado, esfera o protege; após sua morte, torna-se neutra e caótica com PM 8 e ataques em área. Estado de esfera, matriz e tabuleiro é por dono e suporta heróis repetidos.

Atributos/custos não definidos pelo autor usam os padrões atuais e permanecem editáveis, conforme autorização expressa. Conferir os valores exatos em Content.InventoresLuna.cs e as descrições no parcial correspondente. Não inventar mudanças de balanceamento durante a revisão.

## Reparos adicionais pedidos pelo autor

1. **Nepturius hit north-west:** sete quadros corrigidos no PixelLab, de costas e sem clarão branco. Original preservado em `native/Nepturius-hit-nw-original.zip`; revisão em `nepturius-hit-nw-revisao.json`. Início, reação e fim conferidos no player final. Não regenerar.
2. **Primal Beast não parava em algumas casas azuis:** dois níveis do clique bloqueavam o corpo 2x2. O raycast devolvia sempre `f.cell` ao atingir o próprio collider; depois a seleção de uma casa do próprio corpo abria a ficha. Em modo mover e podendo agir, `GroundPickForMove` deixa o próprio lutador grande selecionar o chão; `InspectOnMoveTap` permite mover para outra casa ocupada pelo próprio corpo. Canto de origem ainda abre a ficha, outros personagens continuam selecionáveis e skills mantêm seleção de unidade. Regras de terreno/PM/colisão do corpo inteiro não foram relaxadas. No player: seis raycasts acertaram as casas esperadas e `Game.Submit("mover")` deslocou o Primal, com capturas antes/depois.
3. **Nomes:** Pele D. N/nomes antigos passam para **Pele D. Ark** e Reflectius/Homem dos Espelhos para **Mirhor**. IDs `pele-negra` e `espelhos` preservados. `Content.ApplyEdits` migra nomes conhecidos sem perder atributos; nomes personalizados são preservados. 19 regressões adicionais cobrem nomes/clique/raycast; expectativa antiga de Reflectius atualizada no teste Update2210.

## Evidências e limites reais

- `editmode-13.xml`: **684/684 aprovados**, na cópia isolada, com o código final.
- `build-windows-11.log` e `build-report.json`: Windows compilado com sucesso, **zero erros, 505 avisos**. Não foi gerado Android nesta entrega.
- `import-final.json`: 357 arquivos, 1.647 quadros, 14 rigs e oito verificações das rotas de efeitos.
- `qa-render-release/resultado.json`: passed=true, primalPickCases=6, primalMovementVerified=true, displayNamesVerified=true, 14 rigs e sete frames hit NW. Capturas são do renderizador URP real, via render request; foram inspecionadas. Ver `player.log`, `primal-antes.png`, `primal-depois.png`, docas, tabuleiro e três momentos de Nepturius.
- `validacao-visual.json`: revisão técnica do Codex, NÃO aprovação estética do autor; contém hashes das fontes e do executável.
- `integracao/plano.json`, `integracao/integracao-realizada.json`: **26 fontes conciliadas**, idênticas às testadas; **773 arquivos próprios** integrados, incluindo assets/metadados. Conferência posterior de todos os hashes passou. Backups de arquivos existentes estão em `integracao/principal-antes`.
- `integracao/pacote-windows.json`: 253 arquivos do pacote Windows copiados e verificados. Executável: `C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/ArenaTaticaUnity/Builds/InventoresLuna-2026-10-23/Windows/ArenaTatica.exe`.
- Galeria: `galeria-inventores-luna.html`; resumo: `ENTREGA.md`; estado: `progresso.json`.
- **Não foi realizada partida real host + cliente / reconexão / clientes simultâneos. Testes locais e smoke de renderização não comprovam multiplayer correto. Essa é a prioridade desta revisão.**

## Revisão prioritária de multiplayer

1. Serialização, cópia/filtragem e validação de todo `MatchState.Luna`: famílias/engrenagens/golpes, marcas, caixas, esfera por dono, turnos de xadrez, formas, flags/cooldowns e donos mortos. Snapshot completo, delta se houver, entrada tardia e reconexão devem manter o mesmo estado. Revisar filtros de dados novos na névoa, radar e invisibilidade.
2. Autoridade do host: somente a autoridade deve sortear engrenagens, cubo da esfera, tubarões e decisões autônomas. Validar alvo, proprietário, PA, cooldown, alcance, consumo de engrenagem e acesso à loja/robô em comandos recebidos; clientes não podem criar recursos ou controlar invocações alheias. Verificar duplicatas, revisão antiga e comandos fora de turno.
3. Determinismo e ordem: RNG do Match, desempates de robôs, fim/início do turno, morte do dono, invocações sequenciais, alterações de iniciativa/timeline, desaparecimento de clones/peças/matriz e transferência dos recursos de Blip. Tentar heróis repetidos, seis jogadores, aliados e inimigos em áreas ao mesmo tempo.
4. Estados especiais: teleporte/portal/imunidade, água e PM mínimo, Eletaser/roubo de PM, Sonus, marca de Celinos, Blipware controlável, alvo invisível, matriz voando para engrenagem, esfera órfã, Roque e controle de peças inimigas.
5. UI/apresentação não pode avançar regras no cliente: comando do Primal usa a casa correta também no cliente remoto/toque/rotação de câmera; HUD/efeitos/animações não bloqueiam comandos permanentemente nem repetem dano/passiva. Conferir que flags de smoke/QA só executam com seus argumentos próprios e não afetam partidas normais.
6. Compatibilidade 14 nas pontas, catálogo/edições do host, nomes migrados e mensagens claras para versões incompatíveis. Conferir que transição de sala para partida, desconexão/reconexão e término não deixam fila de eventos, referências ou donos inválidos.

## Procedimento e critérios de conclusão

Trabalhar em cópia/worktree própria e reservar arquivos antes de editar; a produção de Carlos foi liberada. Não concorrer sobre os mesmos arquivos. Preservar alterações/commits concorrentes, IDs e contratos existentes. Decisões de design e lógica sensível seguem as instruções do autor/Codex; não substituir uma correção por uma mudança de regra sem explicar.

Reproduzir os bugs, corrigir com diffs delimitados e testes de regressão. Rodar a suíte e o build afetado. Fazer teste real host+cliente, incluindo reconexão, habilidades novas, invocações e eventos; quando não puder fazê-lo, registrar o impedimento e o que segue NÃO validado. Não declarar multiplayer pronto apenas pelo número de testes.

Responder com: severidade, reprodução, arquivo:linha, causa, correção, arquivos alterados, testes/resultados, evidências de multiplayer e pendências `?`. Registrar responsável/status na tarefa e resposta própria, sem sobrescrever respostas anteriores. Não iniciar outras frentes nem gerar assets.

## Publicação e aprovações

Commit/push **não realizado**. Na última conferência, `Core/MapEvents.cs` e `Core/Match.Effects.cs` não pertencem ao HEAD e há alterações anteriores de outros agentes. Índice Git foi preservado, sem commits adiantados do upstream. Conciliar a base antes de publicar somente a entrega autorizada; não incluir trabalho alheio ou publicar uma etapa parcial.

A nova meta visual está `aguardando_avaliacao` do autor. Não repetir a pergunta nem editar/regenerar artes entregues sem orientação. As **210 cartas já aprovadas** permanecem aprovadas e intactas; **A Mestra dos Dragões** não foi liberada para cartas/jogo. Não usar NVIDIA, pedir/imprimir/gravar chaves ou transmitir segredos. A ponte não comprova que Claude recebeu ou executou esta tarefa; registrar o recebimento quando assumir.

## Fontes alteradas e integradas

- `Assets/ArenaTatica/Scripts/Core/Board.cs`
- `Assets/ArenaTatica/Scripts/Core/Content.cs`
- `Assets/ArenaTatica/Scripts/Core/CoreTypes.cs`
- `Assets/ArenaTatica/Scripts/Core/MapEvents.cs`
- `Assets/ArenaTatica/Scripts/Core/MapGen.cs`
- `Assets/ArenaTatica/Scripts/Core/Match.cs`
- `Assets/ArenaTatica/Scripts/Core/Match.Effects.cs`
- `Assets/Editor/ArenaBuild.cs`
- `Assets/ArenaTatica/Scripts/ArenaGame.cs`
- `Assets/ArenaTatica/Scripts/ArenaHud.cs`
- `Assets/ArenaTatica/Scripts/ArenaMapEditor.cs`
- `Assets/ArenaTatica/Scripts/ArenaPresentation.cs`
- `Assets/ArenaTatica/Scripts/ArenaSmoke.cs`
- `Assets/ArenaTatica/Scripts/Core/Fog.cs`
- `Assets/ArenaTatica/Tests/Editor/Update2210Tests.cs`
- `Assets/DungeonBDX/Skills/DungeonHeroProfiles.cs`
- `Assets/DungeonBDX/Tests/Editor/DungeonExpeditionTests.cs`
- `Assets/ArenaTatica/Tests/Editor/ArenaCorrecoesOutubroTests.cs`
- `Assets/ArenaTatica/Scripts/Core/Content.InventoresLuna.Descriptions.cs`
- `Assets/Editor/ArenaInventoresLunaBuild.cs`
- `Assets/ArenaTatica/Scripts/Core/Content.InventoresLuna.cs`
- `Assets/ArenaTatica/Scripts/Core/Match.InventoresLuna.cs`
- `Assets/ArenaTatica/Scripts/ArenaPresentation.InventoresLuna.cs`
- `Assets/ArenaTatica/Scripts/ArenaSmoke.InventoresLuna.cs`
- `Assets/ArenaTatica/Tests/Editor/InventoresLunaTests.cs`
- `Assets/Editor/ArenaInventoresLunaImport.cs`
