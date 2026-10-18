# Passagem — DungeonBDX: equipamentos e balanceamento pelo host (17/10/2026, 23:27)

Passagem anterior (Arena no celular): `agente/passagens/PASSAGEM-2026-10-17_23-27.md`.
Transcript desta conversa: `C:/Users/Anuah/.claude/projects/C--Users-Anuah-OneDrive--rea-de-Trabalho-Rpg/f217df0e-076d-4caf-8549-37b9be638276.jsonl`.
Contexto completo do DungeonBDX: `coordenacao/dungeonbdx/HANDOFF-CLAUDE-17-10-2026.md` (ler a última seção).

## Objetivos do autor (palavras dele)
1. "o host pode balancear status, etc dos personagens": quem cria a partida ajusta os números (vida, defesa, dados, movimento, moedas, overkill) dos heróis e inimigos antes de começar.
2. "ajuste o dragão e o rei goblin (os equipamentos podem servir como forma de balancear e upar os personagens (priorize os valores base do arcadia quest))".

## Feito e no GitHub (push autorizado)
- `a9ac43a5`: um ícone PixelLab por missão na faixa (`AqScenarios.QuestIcon`).
- `6c9838f7`: inimigos no padrão Arcadia Quest. `AqProfiles` (em `Arcadia/AqBoard.cs`) guarda as linhas de nível 1 da planilha `KrosArcano/Arcadia_Quest_-_Card_List_1.2.xlsx`. As habilidades estão no `AqSession`: undying, pack, pierce, mend, cleave, brutal, quake, prickly, might, breath, echo. EditMode 417/417, build e capturas ok.
- O problema: o Rei Goblin (`dread`, Dread King: vida 3, só críticos acertam) e o dragão (`dragon`, Havok jovem: vida 10, def 2, `breath+might`) ficam quase imunes. Os heróis só têm armas de 2–3 dados (`AqContent.Signature` e as reservas no `AqSession`). Não há loja nem forma de gastar moedas.

## Próximo passo (concreto)
1. **Equipamentos no estilo upgrades do Arcadia Quest** (as moedas compram cartas):
   - Catálogo `AqGear` com custo em moedas: armas de 3–5 dados (corpo a corpo e à distância), +defesa, rerrolagem e uma propriedade "afiada" (face 5 também conta como crítico), que é a resposta ao `might`.
   - Compra no descanso ou na área da guilda, gastando `AqGuild.Coins`.
   - O comprador não pode passar de 4 cartas (`Gear.Count`, igual à transferência).
   - Manter as linhas base do Arcadia nos inimigos. O equilíbrio vem dos itens, não de baixar o dragão e o rei.
   - Ver na planilha as abas `Upgrades` e `Item Type by Set` e copiar os valores, com nomes originais.
2. **Ajuste do dragão e do Rei Goblin:** só se, mesmo com os itens, continuarem impossíveis. Primeira opção: o crítico contra `might` vale 2 acertos (hoje já vale crit+min(3,crit)). Se precisar mais, ajustar a vida. Registrar a decisão.
3. **Balanceamento pelo host:**
   - Na configuração da partida (`AqMatchMenu.cs`), uma tela "Ajustes do host" para editar, por perfil ou herói, vida, defesa, dados, movimento, overkill e moedas.
   - Os ajustes vão no save e no replay: precisam fazer parte da sessão determinística (`AqSession`, seed + comandos). Ver como o lobby online da Arena passa as configurações, se houver multijogador.
   - Os valores padrão continuam sendo os do Arcadia.
4. Testes novos (compra de item, item "afiado" contra `might`, override do host aplicado aos `Units`), EditMode, build e capturas. Commit local e **perguntar antes do push**.

## Antes de rodar o Unity
- Às 23h a sessão da Arena (`local_03ac0235…`, "Arena Sprint 1") estava gerando o build Windows + Android 1.2.3 (cerca de 40 min, numa cópia em `%LOCALAPPDATA%\ArenaTaticaAndroid`). O script dela encerra o VBCSCompiler.
- **Não rode testes nem build do Unity até ela avisar** ou até não haver `Unity.exe` dessa cópia: `powershell "Get-CimInstance Win32_Process -Filter \"name='Unity.exe'\" | select CommandLine"`. Enquanto isso, dá para escrever o código.

## Regras que valem
- Respostas curtas. Sem partidas entre bots. Nunca dar push sem perguntar.
- Commitar só os caminhos explícitos. Nunca `DungeonBDX.unity`, `ProjectSettings.asset` nem os `.meta` de `Sprites/Menu`. Preservar o CRLF (não use `sed -i`, ele troca o fim de linha; use Python binário).
- Arte e nomes sempre originais; o Arcadia serve só de referência de mecânica e números.
- O WT `tmp/dungeonbdx-step1-git` está desatualizado e o sync (`tmp/dungeonbdx-aq-sync.py`) copia o WT por cima do main. **Editar direto no main e, se rodar o sync, copiar antes os arquivos editados para o WT.** O `AqBoardView.cs` só existe no main.
- O Python do Codex está com o PIL quebrado (CCleaner): `from PIL import Image` falha.

## Como verificar
- Testes: em `ArenaTaticaUnity`, `unity test . --mode EditMode --output Artifacts/x.xml` (417 hoje).
- Build: `unity build . --target StandaloneWindows64 --execute-method DungeonBuild.Windows`. Se o Burst falhar por falta de um `.pdb`, renomear `Library/BurstCache`.
- Capturas: `powershell -NoProfile -ExecutionPolicy Bypass -File tmp/dungeonbdx-aq-captures.ps1` → `coordenacao/dungeonbdx/entregas/08-configuracao-arcadia/`.

## Esperando o autor
- Jogar e dizer se os lacaios e vilões do Arcadia ficaram fortes ou fracos demais.

## Tarefa agendada pao-duro-2026-10-17-2330 (23:40)
- Não alterou nada. A sessão `a42568c9` (handoff aberto pelo autor) já estava editando `AqContent.cs`/`AqSession.cs` com este mesmo passo (`AqShop`, `AqTuning`). Regra: um agente por arquivo. O build da Arena 1.2.3 ainda rodava (Unity.exe da cópia ativo), então não houve testes.
- Valores úteis da planilha (aba `Upgrades`, set base): nível 1 = armas de 3 dados (Trusty Blade 3 + def 1 + rerrolagem; Bleeder 3 + crítico dá 1 acerto extra; Crescent Bow 3 à distância), Orc Pelt def +2 (1 armadura por herói), Sun Ring rerrola 1. Nível 2 = armas de 4 dados (Smashface, Decimator), Knight's Plate def 2 + 1 de vida. Nível 3 = Night Fury 4 + def 2 + 2 rerrolagens, Thorn 4 à distância, Lord's Plate def 4 + 1 de vida, Sun Ring II rerrola 3. A coluna `Price` (0–9) não segue o nível e não serve como custo em moedas.
- Python com openpyxl: `C:/Users/Anuah/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe` (o `python` do PATH é o atalho da Store).

## Loja e ajustes do host — feito (18/10, sessão a42568c9)
- Commit local `85d1c1ef` (push feito com aprovação do autor). EditMode 422/422, build Windows ok, 4 capturas + `loja-descanso.png` e `ajustes-do-host.png` em `entregas/08-configuracao-arcadia/`.
- Loja (`AqShop` em `AqContent.cs`): 10 cartas originais, 3 a 7 moedas, compradas no descanso (comando `buy`), limite de 4 cartas, `discard` (a arma própria fica). "Afiada": o 5 também é crítico, então passa pelo `might` do dragão/Rei Goblin.
- Ajustes do host (`AqTuning`, aba nova no menu): 11 valores somados aos do Arcadia, gravados como comandos `tune` antes da 1ª jogada (save e replay sem mudar de formato; saves antigos com a mesma fingerprint).
- Pendente: o autor jogar e dizer se dragão e Rei Goblin ficaram viáveis com os itens; ajustar preços e quantas moedas os inimigos dão se preciso.

## Vida, ícones, campanha e medidor — feito (18/10, sessão a42568c9)
- Commit `712dc2b6` (push feito com aprovação do autor). EditMode 428/428, build e capturas ok.
- Vida: selo na tela (coração, número e barra) na frente dos pés, por cima das paredes e por baixo do HUD (`AqBoardView.PlaceBadge`).
- Ícones: cartas, loja e ficha do herói usam os ícones do PixelLab já existentes (`AqGear.Icon`). A ficha mostra a defesa total.
- Campanha: o botão "PRÓXIMO CAPÍTULO" na vitória, ou o menu (se as mesmas guildas venceram o capítulo anterior), leva as moedas, as cartas compradas e os ajustes (comandos `purse`/`grant`).
- Medidor oculto: com 50% de 4096 comandos, compacta numa troca de turno (`AqJournal.Compact`, retrato `AqSnapshot`). Save v2 só quando compactado; v1 continua igual.

## Artes da loja e ficha do herói — feito (18/10, sessão a42568c9)
- Commit `d894822f` (push feito com aprovação do autor). EditMode 428/428, build Windows e capturas ok (`ficha-heroi.png`).
- Android PARADO por ordem do autor (18/10): não gerar APK; só build Windows.
- 8 artes PixelLab em `Iso/UI/Cards/<id>.png` (README e prompts em `coordenacao/dungeonbdx/entregas/10-cartas-loja/`). Faltam a `iron-blade` e a `heavy-crossbow` (o rascunho virou fuzil e foi rejeitado). O PixelLab ficou sem gerações até o bônus diário (+5/dia; o ciclo renova em 10/11).
- Ficha do herói: segundo clique no herói selecionado, ou o botão HABILIDADE.

## Fase de Melhorias do Arcadia e artes que faltavam — feito (18/10, sessão f65f6a45)
- Pedido do autor: loja entre capítulos, baralho por nível, escolha de 6, até 3 compras e só 1 moeda guardada; desenhar as 2 artes no editor do PixelLab.
- Regra (livro do Arcadia, p. 27): `CarryCommands` termina com o comando `upgrade`; o capítulo II usa o baralho de nível 1 e o III o de nível 2 (`AqShopItem.Level`, 5 cópias de cada = 25 cartas). Cada guilda recebe 6, escolhe 2 e passa 4 para a guilda seguinte, escolhe 2 e passa 2, fica com as 2 últimas (comando `draft`). Depois compra até 3 (`buy` com índice nas escolhas) e encerra (`ready`), guardando no máximo 1 moeda. Nada de jogar até todas encerrarem.
- Saves antigos continuam abrindo: a compra no descanso ficou só no motor (sem botão) e capítulos sem `upgrade` mantêm a fingerprint. O sorteio usa um gerador separado, sem mexer nos dados. O retrato de compactação guarda `UpgradeLevel`.
- Tela: painel FASE DE MELHORIAS em hot seat (mão da guilda da vez, heróis, descarte, ENCERRAR COMPRAS). Captura: `-dungeonbdx-aq-shop` abre o capítulo II com 8 moedas por guilda.
- Artes: `iron-blade` e `heavy-crossbow` desenhadas à mão no `pixelart_workbench` (grátis); as 10 cartas têm arte. README em `entregas/10-cartas-loja/`.
- EditMode 436/436 (testes novos em `Tests/Arcadia/AqUpgradeTests.cs`).
- Canva conectado pelo autor em 18/10 (conector do claude.ai).

## Arco de teixo no Blender — feito (18/10)
- Commit `bf0185b5` (push feito com aprovação do autor). EditMode 436/436, build Windows ok.
- `yew-bow` refeito no Blender 5.2 (64×64, rampas da paleta das cartas, contorno `#141217`). Script e comparação em `entregas/10-cartas-loja/exemplo-blender/`; versão antiga em `yew-bow-pixflux-antiga.png`. Blender fora do PATH: `C:/Program Files/Blender Foundation/Blender 5.2/blender.exe -b -P blender_bow.py -- <saída.png>` (este PC não tem Python; usar o do Blender).
- O sorteio fixo de `-dungeonbdx-aq-shop` não traz o arco; ainda sem captura dele no jogo.
- Pendente: o autor dizer se lacaios/vilões estão fortes ou fracos e se aprova o arco; refazer no Blender as cartas que ele achar fracas.

## Arena: DungeonBDX abandonado, labirintos e mapas aleatórios — feito, sem commit (18/10)
- Autor desistiu do DungeonBDX; arte, props, personagens e funções passam para a Arena. EditMode 449/449, build Windows ok, capturas em `ArenaTaticaUnity/Artifacts/mapa-*.png`.
- Mapas (MapGen reescrito): sem espelho; blocos temáticos prontos (girados/espelhados) + manchas; inícios opostos e `MapGen.Fair` (zona, vela, baú). Padrão do menu = aleatório; mapas do Codex só pela opção "Um mapa pronto do Codex" (`ArenaMaps.Codex`).
- Piso: kit 3D do Blender como base (labirintos: Cripta→pedreira, Biblioteca→deserto, Jardim→bosque); pisos do DungeonBDX só em manchas (`Decor` "piso_<tile>", `MapGen.FloorPatches`).
- Labirintos (temas 5–7, escolhidos no menu; o sorteio -1 só pega abertos): um tamanho acima (16/18/20), paredes, portas (encostar, 1 PM, comando `abrir`), névoa por linha de visão (`Match.Sees/Concealed`, ViewFor esconde), vela única (`acender`: tira a névoa, +2 moedas), boss único (evento `chefe`: Dragão Espectral Ancião, Necromante da Cripta, Rei Goblin Ferrugem; anda, abre portas, bate).
- Objetos (`Tile.obj`/`TileObj`): baú (+3 moedas), espetos (1 de dano), braseiro (+1 PA; Domínio), portais fixos em pares pela ordem.
- Mapa com paredes: Salto/Teleporte com metade do alcance e +1 PA (`Match.Cost(f,sk,s)`).
- Menu: "Posições iniciais" liga/desliga a escolha das casas (`arena-posicionar`).
- Multiplayer (protocolo 7): fichas editadas fora de lobby/draft/estado; vão em partes (`edits`, "i/n") só quando mudam por jogador; limpeza da fila (Overflow→Resend) reenvia fichas + snapshot completo; pedido em menos de 1 s fica pendente em vez de se perder.
- Primal Beast (ex-Colosso) 3,1; Edgar 1,65; navio 2,1.
- Build: faltava `ArenaTatica_Data/Resources/unity default resources` (o exe caía na abertura); copiado do build do DungeonBDX.
- 18/10 tarde: piso das manchas sem frestas (escala 1). Testes 449/449 e build refeitos; capturas `Artifacts/aberto-tema1-1810-vista*.png` ok. Falta: retorno do autor (tamanhos Primal Beast/Edgar/navio, visual das portas) e aprovação do commit/push.
- 18/10 (pedido "mais organizado"): MapGen sem manchas aleatórias. Pisos com forma em `Decor.rot` (Losango/Moldura/Cruz/Quadrado; Estrada+dir = trecho reto), blocos com piso desenhado, 0–2 desenhos com centro em mosaico (`Motif`), estrada de tijolos 0–2 vezes da beira até a zona (`Roads`, com tochas no `Decorate`), vilas (`praca`, `feira`, `poco`, `armazem`: fonte, barris, cerca, tocha, placa, fogueira), barris soltos e enfeites em canteiros. Testes 450/450, build ok, capturas `Artifacts/vila*-tema*.png`.
- Commit `a9d870a0` com push (autor aprovou). Tamanhos Primal Beast/Edgar/navio e portas aprovados.
- 18/10: Primal Beast ocupa 2×2 (`FighterDef.size`, não editável: `ApplyEdits` mantém o da ficha original). `Board`: `Footprint`, `Occupies`, `Gap` (distância entre quadrados), `Fits`, `Step`; `At` acha qualquer casa dele. Alcance, linha, visão, área (Pisão sai das 4 casas), trava, zona e movimento usam a pegada; não usa portal; não troca de casa no posicionamento; `Settle` leva para o canto livre mais perto onde cabe (início, reviver). Sprite no meio do 2×2 e `Plant` põe os pés de todo sprite no meio da casa (vértice mais baixo da malha justa). Captura do modo `--arena-shot` usa o Primal Beast. Testes 454/454 (`BigFighterTests`), build ok, capturas `Artifacts/beast4-vista*.png` (pés tocam a marca 2×2). Sem commit, aguardando aprovação do autor.
