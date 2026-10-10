# Arena Tática — Fase 8

**A cena principal inclui HUD de HP/PA, turno, IA, mortes sincronizadas e Vitória/Derrota.**
Abra **ABRIR-GODOT.cmd**, pressione **F5** e use **Host** ou **Join**.
P1 controla o cubo laranja; P2 controla o azul. As esferas roxas são os monstros.
Ative **Atacar** e clique no adversário ou monstro. Consulte o **[guia da Fase 8](FASE-8.md)** para integração e testes; o **[guia da Fase 5](FASE-5.md)** preserva a referência de rede.

O **[guia da Fase 2](FASE-2.md)** registra a etapa original de movimento. A cena atual é
`scenes/arena_interativa.tscn`; `scenes/arena.tscn` continua sendo a base reutilizada.

## Referência da base — Fase 1

Projeto completo em GDScript para Godot 4.6.1, com renderizador Compatibility.
Tabuleiro procedural de 12 × 12 blocos básicos, câmera isométrica ortográfica,
iluminação e colisão por célula. A grelha é criada ao executar o jogo.

## Abrir e jogar

1. Nesta pasta, dê dois cliques em **ABRIR-GODOT.cmd**.
2. Aguarde a primeira importação terminar.
3. Se a cena não abrir automaticamente, no painel **Sistema de Arquivos / FileSystem**,
   expanda **scenes** e dê dois cliques em **arena_interativa.tscn**.
4. Pressione **F5**, ou clique no triângulo **Executar Projeto** no canto superior direito.
5. Você verá 144 blocos e a peça laranja. Clique numa casa para mover. **F8** encerra a execução.

Alternativa: abra seu Godot 4, clique em **Importar / Import**, depois em **Procurar / Browse**,
selecione o arquivo **project.godot** desta pasta e clique em **Importar e Editar**.
Não é necessário criar nós, colar código ou instalar extensões.

## Alterar o tamanho

1. Pare a execução com **F8**.
2. Abra **scenes/arena.tscn** e, na árvore **Cena / Scene**, selecione **Board**.
3. No **Inspetor / Inspector**, mude **Columns** e **Rows** (1 a 32).
4. **Cell Size** muda a distância entre centros; **Tile Height** muda a espessura.
5. Salve com **Ctrl+S** e pressione **F5** novamente.

Os blocos aparecem durante a execução, não na prévia do editor. Durante o jogo,
clique em **Remoto / Remote** na árvore para ver **Board → Tiles → Cell_0_0**, etc.
A câmera mantém o tabuleiro completo visível em janelas horizontais e verticais.

## Estrutura entregue

```text
Arena                          scripts/arena.gd
├── Board                      scripts/board.gd
│   └── Tiles                  gerado durante a execução
│       └── Cell_coluna_linha   StaticBody3D, um por célula
│           ├── Visual         MeshInstance3D / BoxMesh
│           └── Collision      CollisionShape3D / BoxShape3D
├── Camera3D
├── DirectionalLight3D
└── WorldEnvironment
```

- `scripts/grid_model.gd`: dados lógicos independentes de nós, câmera e transporte.
- `scripts/board.gd`: cubos, colisões, limites e conversão célula ↔ posição.
- `scripts/arena.gd`: posição e enquadramento da câmera.
- `scenes/arena.tscn`: cena editável com iluminação e ambiente.
- `tests/run_tests.gd`: verificações automáticas de coordenadas, colisões e enquadramento.

## Convenções para as próximas fases

Uma célula usa `Vector2i(coluna, linha)`. A linha corresponde ao eixo **Z** do mundo;
**Y** é altura. O topo do tabuleiro fica em Y = 0 no espaço local de Board.
`cell_to_world()` devolve o centro desse topo. `world_to_cell()` converte uma posição
para coordenadas lógicas; sempre confira `contains_cell()` depois. Essa conversão
inclui os pequenos vãos visuais entre blocos e não substitui um teste de colisão.

As dimensões são configurações de geração: após alterá-las por código, chame
`generate_board()` antes de usar as conversões. Gerar novamente cria um mapa novo
e reinicia os dados de terreno. Gere fora do processamento de consultas físicas.

Camadas físicas já nomeadas em **Projeto → Configurações do Projeto → Layer Names → 3D Physics**:

| Camada | Máscara em bits | Uso |
|---|---|---|
| 1 | 1 | Piso selecionável |
| 2 | 2 | Obstáculos de Linha de Visão, futuramente |
| 3 | 4 | Unidades, futuramente |

A seleção futura usará raios da câmera e a máscara 1, lendo `board_cell` do corpo
atingido. Consultas físicas devem ocorrer em `_physics_process()`. O futuro serviço
de Linha de Visão fará consultas entre unidades, com suas próprias regras e máscara;
a câmera não participa da decisão tática. `blocks_los` é um dado reservado, ainda
sem um cálculo de visibilidade implementado.

Direção para a arquitetura seguinte: `TurnManager` administra IDs de unidades e
equipes. Um controlador humano ou de IA propõe a mesma estrutura de ação. O servidor
valida ações e mantém o estado autoritativo; a apresentação apenas o representa.
Não use IDs de peers como IDs de unidades, pois monstros não precisam de um cliente.
O adaptador de transporte poderá usar WebSocketMultiplayerPeer ou WebRTCMultiplayerPeer.
WebRTC exigirá planejar sinalização e conectividade; a escolha de transporte não faz
parte desta fase. Não se pressupõe determinismo da física entre plataformas.

## Escopo

A base entrega configuração e geração da grelha. A Fase 2 acrescenta seleção por
clique/toque, AStar e animação da peça. Personagens 2.5D, turnos, IA, Linha de Visão
e multiplayer pertencem às fases seguintes. Ainda não há executáveis exportados
para Mobile ou Web nem validação de cross-play. Somente blocos básicos são usados.

## Verificação pelo terminal

Validação realizada em **07/10/2026** no Godot **4.6.1 stable**, Windows:

- Importação pelo editor concluída sem erros.
- **833 verificações automáticas, 0 falhas:** 815 da grelha/câmera/física e 18 da janela real.
- Conversões de coordenadas verificadas também com translação, rotação e escala.
- Colisões verificadas com raio real da física e máscaras distintas, incluindo
  a remoção de colisões antigas e a criação das novas após regenerar o mapa.
- Enquadramento verificado em 1280 × 720, 720 × 1280 e 600 × 600, com mapas
  12 × 12, 10 × 6, 1 × 1 e 32 × 32.
- Prévias 1280 × 720 e 720 × 1280 renderizadas com Compatibility e conferidas.
- Janela real redimensionada para 1000 × 600 e 600 × 900: o viewport acompanhou
  a proporção e o tabuleiro permaneceu completo, usando stretch `expand`.
- Revisão estática pelo DeepSeek via OpenRouter; ajuste da fonte de luz ambiente
  confirmado no próprio Godot. A validação de execução acima foi realizada pelo Codex.

![Prévia do tabuleiro](.artifacts/preview-1280x720.png)

No PowerShell, aberto nesta pasta:

```powershell
& '.\.tools\godot\Godot_v4.6.1-stable_win64_console.exe' --headless --path . --editor --import
& '.\.tools\godot\Godot_v4.6.1-stable_win64_console.exe' --headless --path . --script res://tests/run_tests.gd
& '.\.tools\godot\Godot_v4.6.1-stable_win64_console.exe' --path . --script res://tests/check_window.gd
```

`.godot`, `.tools` e `.artifacts` são pastas locais ignoradas pelo Git. A cópia
portátil do editor fica em `.tools`; a prévia renderizada e os logs ficam em `.artifacts`.

Referências oficiais: [câmera ortográfica](https://docs.godotengine.org/en/stable/classes/class_camera3d.html),
[exportação Web](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_web.html),
[linha de comando](https://docs.godotengine.org/en/stable/tutorials/editor/command_line_tutorial.html).
