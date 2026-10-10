# Arena Tática — pacote para migração ao Unity

Preparado em 09/10/2026. Unity instalado neste PC: **6000.6.4f1**. Este pacote reúne os assets e a referência funcional do Godot; a programação do jogo ainda precisa ser portada para C# pelo responsável pela integração.

## Conteúdo

- `Assets/ArenaTatica/Models`: Ciantia, dragão dourado e dragão transplanar em GLB, com malhas, materiais e hierarquia. Exportados das cenas existentes e reabertos pelo Godot. Nenhuma cena original foi regenerada.
- `Assets/ArenaTatica/Textures`: piso e sprites PNG originais. Os personagens atuais usam modelos 3D; os sprites ficam como referência e alternativa visual.
- `Assets/ArenaTatica/Audio`: ataque, sopro, morte e turno em WAV.
- `ReferenciaGodot`: projeto, cenas, scripts, testes e registros de produção. Ficam fora de `Assets`, pois GDScript e cenas Godot não executam no Unity.
- `Referencias`: direção estética do autor.
- `Validacao`: logs da bateria atual e resumo. `manifesto.json`: inventário, tamanhos e SHA-256 dos arquivos.

Caches, executáveis do Godot, clones externos, builds antigas e entregas anteriores ficam fora do pacote. Os modelos conservam o estado **aguardando avaliação do autor** registrado na produção; exportar não constitui aprovação estética. Não foram importados modelos do repositório externo de assets.

## Entrada no Unity

1. Criar um projeto 3D no Unity 6. Usar URP como direção inicial para desktop, Web e Android; conferir materiais e performance em cada alvo.
2. No Package Manager, adicionar pelo nome `com.unity.cloud.gltfast`. O importador oficial registra `.glb`/`.gltf`; consultar [a documentação de importação no Editor](https://github.com/Unity-Technologies/com.unity.cloud.gltfast/blob/main/Packages/com.unity.cloud.gltfast/Documentation~/ImportEditor.md).
3. Copiar a pasta `Assets/ArenaTatica` para `Assets` do projeto. Manter os documentos e a referência Godot fora de `Assets`.
4. Conferir os três modelos no Editor: orientação, base no chão, escala relativa, materiais e cores. O pacote ainda não foi importado ou validado dentro do Unity.
5. Montar prefabs com a raiz lógica separada do modelo visual; adicionar colisores à raiz do personagem conforme o jogo. Os modelos exportados não possuem colisores, esqueleto ou clips de animação. Movimento/ataque atuais são procedurais e precisarão ser reproduzidos.
6. Reconstruir o tabuleiro procedural 24 × 24 e câmera ortográfica que mantém o tabuleiro completo visível. Testar janela horizontal, vertical e quadrada antes de portar o combate.
7. Portar regras, IA e UI; testar localmente. Depois portar o transporte multiplayer com autoridade do Host e executar os cenários de sincronização e reconexão.

## Contrato visual e funcional

Preservar Ciantia, os dois dragões, o piso e a direção de miniaturas existentes. Dourado pequeno reservado às futuras skills; os inimigos atuais usam o transplanar. Não acrescentar automaticamente uma skill do dragão dourado.

A célula lógica é `(coluna, linha)`; linha corresponde a Z, Y é altura. A base do tabuleiro é Y=0. Tratar convenções de orientação do importador separadamente dos dados lógicos; conferir uma célula conhecida e a frente do personagem no Unity.

Manter a câmera com todo o tabuleiro visível. O Godot enquadra os limites do mapa com margem, altura extra e proporção da janela; não copiar apenas o valor `size` da cena. Em Unity, `orthographicSize` representa meia altura e exige recalcular o enquadramento.

Preservar IDs de unidades separados dos IDs de conexão, bloqueio de ação durante animações/recuperação, HP/PA, morte sem ressurreição na reconexão, snapshots tardios e efeitos transitórios sem repetição. O protocolo Godot não é compatível automaticamente com bibliotecas de rede Unity.

## Mapa de portabilidade

| Origem Godot | Responsabilidade no Unity |
|---|---|
| `grid_model`, `grid_navigation` | dados de células, obstáculos e caminhos em C# independente de GameObjects |
| `board`, `arena`, `grid_picker` | geração do piso, câmera, raycast e conversão célula/mundo |
| `grid_mover`, `move_authority`, `PlayerMovement` | apresentação do movimento e validação autoritativa |
| `PlayerStats`, `TurnManager`, `CombatManager` | atributos, turnos e ataques |
| `AIManager`, `GameLoopManager` | decisões dos monstros e vitória/derrota |
| `NetworkManager` | sessão, comandos, snapshots, reconexão e recuperação |
| `VisualSetup`, `ChibiModels` | integração dos GLBs, dimensões visuais e animações procedurais |
| `VisualFXManager`, `AudioManager` | partículas, dano flutuante, sons e limpeza/pool |
| `UIManager`, `DebugInfoOverlay`, `UI.tscn` | HUD, controles e diagnóstico |

Consultar o código e o relatório de investigação para os valores e validações exatos; a cena pode sobrescrever padrões dos scripts. GDScript não é convertido automaticamente.

## Critérios para fechar a migração

- Tabuleiro, câmera e modelos equivalentes, sem trocar artes nem recortar o mapa.
- Clique/toque, caminho, ocupação, PA, ordem de turnos, IA, dano e condições de término equivalentes.
- Host/Client: entrada tardia, snapshots durante ações, desconexão e reconexão, rejeição de ações inválidas e debounce.
- Efeitos e sons correspondentes; sem duplicação de efeitos, sons ilimitados ou objetos transitórios acumulados.
- Testes Unity executados e partida real no Editor; builds de plataforma verificadas separadamente. Os testes Godot não comprovam o port C#.

## Reproduzir a preparação

Na raiz da ArenaTatica, com Godot 4.6.1 disponível em `.tools/godot`:

```powershell
node tools/test_phase9.mjs --phase10
& '.tools/godot/Godot_v4.6.1-stable_win64_console.exe' --headless --path . --script res://tests/test_models_3d.gd
& '.tools/godot/Godot_v4.6.1-stable_win64_console.exe' --headless --path . --script res://tools/export_unity_assets.gd
node tools/prepare_unity.mjs
```

O exportador usa [GLTFDocument](https://docs.godotengine.org/en/stable/classes/class_gltfdocument.html). O preparador interrompe se os logs contiverem falhas ou se os hashes divergirem. É um retrato da versão atual; novas mudanças no Godot exigem repetir a preparação.
