# Fase 2 — seleção e movimento no tabuleiro 12×12

A cena, os nós e os scripts completos já estão criados e conectados neste projeto.
A cena principal agora é `scenes/arena_interativa.tscn`.

## Abrir e testar

1. Dê dois cliques em **ABRIR-GODOT.cmd**.
2. No painel **Sistema de Arquivos / FileSystem**, expanda **scenes** e dê dois
   cliques em **arena_interativa.tscn**.
3. Pressione **F5**. Se o projeto já estiver rodando, pressione **F8** antes.
4. O cubo laranja é a peça. Ela começa em **X1 / Z1**.
5. Clique com o botão esquerdo ou toque numa casa do tabuleiro. O destino fica
   verde durante o percurso, e o cubo anda de casa em casa até chegar.
6. O painel superior mostra a posição atual. O painel inferior mostra o destino,
   o número de casas do caminho ou o motivo de uma tentativa recusada.

As coordenadas começam em zero: um tabuleiro 12×12 vai de **0 a 11** em cada eixo.
Clique/toque fora do piso não movimenta a peça. Enquanto ela anda, um novo pedido
é recusado, preservando o percurso em andamento. Nesta fase, a distância não é
limitada por pontos de movimento; essa regra pertence à Fase 3.

## Hierarquia exata, já entregue

```text
ArenaInterativa                         Node3D / arena_interativa.gd
├── Arena                               Instância de scenes/arena.tscn
│   ├── Board                           Node3D / board.gd
│   │   └── Tiles                       Gerado ao executar
│   │       └── Cell_coluna_linha        StaticBody3D
│   │           ├── Visual              MeshInstance3D
│   │           └── Collision           CollisionShape3D
│   ├── Camera3D                        Camera3D
│   ├── DirectionalLight3D              DirectionalLight3D
│   └── WorldEnvironment                WorldEnvironment
├── Player                              Node3D / grid_mover.gd
│   └── Visual                          MeshInstance3D, BoxMesh laranja
├── Destination                         MeshInstance3D, BoxMesh verde
├── GridPicker                          Node / grid_picker.gd
├── MoveAuthority                       Node / move_authority.gd
└── UI                                  CanvasLayer
    ├── InfoPanel                       PanelContainer
    │   └── Margin                      MarginContainer
    │       └── Lines                   VBoxContainer
    │           ├── Title               Label
    │           └── Coordinates         Label
    └── StatusPanel                     PanelContainer
        └── Margin                      MarginContainer
            └── Status                  Label
```

`Arena` reutiliza a cena da Fase 1. `Player` usa o centro da casa como origem;
sua malha tem tamanho **(0,62; 0,85; 0,62)** e posição local **(0; 0,425; 0)**,
deixando a base sobre o piso. A peça não tem corpo físico: a seleção consulta o piso,
e a ocupação das casas pertence aos dados lógicos. Todos os elementos são blocos básicos.

## Scripts completos e função de cada arquivo

Os links abaixo abrem os arquivos inteiros. No Godot, você também pode expandir
**scripts** no painel FileSystem e dar dois cliques no arquivo desejado.

| Arquivo | Responsabilidade |
|---|---|
| [grid_picker.gd](scripts/grid_picker.gd) | Lê clique/toque, projeta o raio e emite a célula solicitada. |
| [grid_navigation.gd](scripts/grid_navigation.gd) | Calcula o caminho mínimo com AStarGrid2D, sem diagonais. |
| [move_authority.gd](scripts/move_authority.gd) | Valida destinos, ocupação e a sequência das posições lógicas. |
| [grid_mover.gd](scripts/grid_mover.gd) | Executa o caminho aprovado com um Tween por casa. |
| [arena_interativa.gd](scripts/arena_interativa.gd) | Conecta entrada, validação, peça, marcador e interface. |
| [arena_interativa.tscn](scenes/arena_interativa.tscn) | Cena completa, com os nós e propriedades configurados. |

Para copiar um script: abra o arquivo no Godot, clique no texto, pressione **Ctrl+A**
e **Ctrl+C**. Para anexá-lo a um nó novo: selecione o nó na árvore, clique no ícone
**Anexar Script / Attach Script**, escolha o caminho do arquivo e confirme.
Mantenha exatamente os nomes e a hierarquia acima, pois o script de composição
localiza os nós por esses caminhos. `grid_navigation.gd` é um objeto de dados
instanciado pelo código; ele não precisa de nó nem de Autoload.

Para definir manualmente a cena principal: **Projeto → Configurações do Projeto →
Application → Run → Main Scene**, escolha **scenes/arena_interativa.tscn**.
Essa configuração já foi aplicada.

## Ajustes pelo Inspector

- **Velocidade:** selecione **Player** e altere **Step Duration**. O padrão é
  **0,28 segundo por casa**; valores maiores deixam a caminhada mais lenta.
- **Posição inicial:** selecione **ArenaInterativa** e altere **Spawn Cell**.
  O campo Y desse Vector2i representa a linha **Z** do tabuleiro.
- **Tabuleiro:** abra **scenes/arena.tscn**, selecione **Board** e ajuste **Columns**
  e **Rows**, atualmente **12** e **12**. Salve e execute o projeto novamente.
- **Câmera:** na cena interativa, selecione a instância **Arena**. **Framing Margin**
  deixa espaço para a interface, e **Headroom** inclui a altura da peça no enquadramento.

## Separação preparada para as fases futuras

```text
Clique/toque
    ↓
GridPicker.cell_requested(destino)             INTENÇÃO
    ↓
MoveAuthority.request_move(id_da_peça, destino) VALIDAÇÃO + ASTAR
    ↓
MoveAuthority.path_approved(id, caminho)
    ↓
GridMover.execute_path(caminho)                 ANIMAÇÃO
    ↓ a cada casa
step_completed → confirm_step                  POSIÇÃO LÓGICA
```

O picker consulta a física apenas em `_physics_process()`. O raio usa a câmera
ortográfica e a máscara **1**, destinada ao piso, lendo o metadado `board_cell` do
corpo atingido. O metadado evita arredondar a posição da batida junto às bordas.
Interface e segundo dedo são ignorados; mouse emulado por um toque não duplica pedidos.

O AStar trabalha em `(X, Z)` usando `Vector2i`. A apresentação converte cada casa para
3D por `Board.cell_to_world()`. O caminho inclui a origem: **passos = path.size() - 1**.
A distância de cada passo é uma casa, com custo uniforme e sem diagonais. Terreno
marcado com `walkable = false` e casas ocupadas são excluídos da rota. Nesta cena de
demonstração, o piso começa livre e existe uma peça; os testes também exercitam obstáculos.

**Multiplayer futuro:** `_on_move_intent()` é o ponto de conexão com o transporte.
O cliente deverá enviar a identidade da peça e a célula desejada. O Host deverá
validar posse, turno e destino e recalcular o caminho. O caminho aprovado retornará
para `execute_path()`. Os IDs das peças são independentes de IDs de conexão.
O adaptador local atualmente confirma passos ao terminar cada Tween; numa partida
em rede, a posição lógica e o relógio das ações deverão ser controlados pelo Host,
e os clientes reproduzirão os passos autorizados. RPCs e sincronização ainda não
estão implementados.

**IA e Linha de Visão futuras:** `MoveAuthority.step_rule` é uma função opcional
com assinatura `(unit_id, origem, proxima_casa) -> bool`, consultada antes de cada
passo. Ao devolver `false`, o restante do movimento é interrompido. Para uma detecção
após entrar numa casa, conecte `step_committed` e chame `Player.request_stop()`.
Se a parada for pedida durante um Tween, a peça termina esse passo e para centrada
na casa. O cálculo de Linha de Visão e os monstros serão implementados depois.

Exemplo de integração de uma regra de teste (bloqueia X6/Z6 antes de entrar):

```gdscript
$MoveAuthority.step_rule = func(_unit_id: StringName, _origin: Vector2i, next_cell: Vector2i) -> bool:
    return next_cell != Vector2i(6, 6)
```

Nenhuma lógica de seleção depende do jogador ser humano. A IA poderá chamar
`request_move()` com um destino escolhido por ela. O executor continuará recebendo
apenas um caminho aprovado. As ações desta versão são sequenciais, uma por vez.

## Testes realizados

- **47 verificações da Fase 2, sem falhas:** caminho mínimo, desvio de obstáculos,
  casas ocupadas, destinos inválidos, clique, toque, deduplicação, sequência dos
  Tweens, interrupção por regra externa, cancelamento ao regenerar o tabuleiro,
  recuperação após confirmação rejeitada e posição inicial bloqueada.
- **815 verificações da base 12×12, sem falhas.**
- Na janela gráfica do Godot, um clique injetado no fluxo de entrada moveu a peça
  de **X1/Z1 para X7/Z5**, em dez passos, e as prévias foram renderizadas.
- O toque foi verificado com eventos `InputEventScreenTouch` sintéticos no Godot.
  Ainda não houve teste em aparelho físico nem exportação Web/Mobile.
- Revisão estática pelo DeepSeek via OpenRouter em cópia isolada dos scripts.
  Os dois caminhos de falha apontados foram corrigidos e exercitados no Godot;
  as APIs da grelha usadas pela navegação também foram confirmadas pelos testes.

Comandos para reproduzir, no PowerShell aberto na pasta do projeto:

```powershell
& '.\.tools\godot\Godot_v4.6.1-stable_win64_console.exe' --headless --path . --script res://tests/test_movement.gd
& '.\.tools\godot\Godot_v4.6.1-stable_win64_console.exe' --headless --path . --script res://tests/run_tests.gd
& '.\.tools\godot\Godot_v4.6.1-stable_win64_console.exe' --path . --script res://tests/capture_phase2.gd
```

![Peça após se mover para X7/Z5](.artifacts/phase2-movimento.png)

Referências oficiais: [AStarGrid2D](https://docs.godotengine.org/en/stable/classes/class_astargrid2d.html),
[Raycasting](https://docs.godotengine.org/en/stable/tutorials/physics/ray-casting.html),
[eventos emulados](https://docs.godotengine.org/en/stable/classes/class_inputevent.html).
