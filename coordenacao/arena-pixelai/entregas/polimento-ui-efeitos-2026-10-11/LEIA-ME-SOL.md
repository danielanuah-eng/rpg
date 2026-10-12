# Entrega Sol — reparos, efeitos e modelo de interface

Produção concluída e conferida. Aprovação automática autorizada pelo autor; importação, ligação aos gatilhos e testes no Unity aguardam confirmação do Claude. Esta entrega não altera diretamente `ArenaMenu.cs`, `ArenaHud.cs`, regras ou dados de partida.

## Conteúdo

- 3 clipes corrigidos: Baltazar idle oeste e walk oeste; Guardião de Musgo idle noroeste. Apenas estes personagens foram corrigidos.
- 231 tiras RGBA, somando 2.864 quadros e 56 loops, incluindo peças de biblioteca, reutilizações e aliases. Essa contagem não representa 231 efeitos distintos.
- 10 eventos de campo com entrada, permanência e encerramento; habilidades com conjuração e impacto; projéteis; estados; seis desejos e Devorar; dez usos de itens; dez objetos de campo com fases.
- Modelo navegável da tela inicial, seleção de equipe, catálogo, mapas, prévia do tabuleiro e galeria de eventos. Seis lutadores e três mapas exemplificam o layout com atributos lidos do conteúdo atual.

`assets.visual.json` define cada tira: caminho Resource, número de quadros, tamanho nativo, FPS, loop, pivô, ponto de impacto e SHA-256. `inventario.json` lista a cobertura exata. `unity/Biblioteca` contém peças-fonte e não deve entrar em Resources. Os GIFs são prévias; os PNGs preservam RGBA.

## Importação dos reparos

| Recurso | Célula quadrada | Quadros | FPS |
|---|---:|---:|---:|
| `Sprites/Baltazar/idle_west` | 168 px | 4 | 10 |
| `Sprites/Baltazar/walk_west` | 176 px | 6 | 10 |
| `Sprites/GuardiaoMusgo/idle_north-west` | 176 px | 4 | 10 |

Refazer o slicing dos três arquivos com o tamanho acima, pivô do manifesto e PPU 128. Preservar GUIDs e referências existentes. O slicing anterior de 128 px cortaria cabeça e misturaria quadros. Nenhum quadro foi reduzido ou redesenhado no empacotamento: apenas margens transparentes foram acrescentadas.

O PixelLab retornou uma referência inicial adicional porque `keep_first_frame=true`. Foram preservados os quadros animados 1..4, 1..6 e 1..4, respectivamente. Os arquivos originais continuam em `reparos/*/originais`.

## Efeitos e objetos

Importar PNGs em Sprite/Multiple, Point, sem mipmaps e sem compressão, com alfa habilitado. Usar o `frameSize` de cada entrada; os efeitos têm dimensões diferentes. Separar conjuração no lançador, impacto no destino, permanência e projétil. Carregar cada fase explicitamente; um `LoadAll` que mistura arquivos do mesmo diretório produziria uma animação incorreta.

`impactFrame0` é o momento da colisão visual; `peakFrame0` mede a maior área de alfa e pode ser diferente. O martelo perfurante quebra o escudo no quadro 6, embora o escudo inteiro ocupe mais área antes.

Minas respeitam `ownerOnly`; efeitos sobre personagens respeitam a visibilidade da unidade. `sem-pm-oleo` traz `inheritTargetVisibility`. Devorar tem trajetória do alvo ao lançador, a ser adaptada às posições reais. Os aliases lava/gelo/fogos precisam de reprodução em loop: a leitura atual apenas do primeiro sprite não anima o terreno. Os projéteis estáticos oferecem compatibilidade, e `ProjeteisAnimados` contém a alternativa animada.

As habilidades de outros personagens receberam sobreposições de efeitos. Isso não representa correção nem conclusão das animações corporais deles. Gancho de escalada não recebeu efeito de uso: é o item passivo de ícone já existente.

## Modelo de interface

Abrir `ui/index.html` por um servidor local. A prévia desta entrega está em `http://127.0.0.1:8770/ui/index.html#inicio` enquanto o servidor permanecer ativo. Alternativa: servir esta pasta com Python e abrir `/ui/index.html`.

O botão Preparar partida abre a seleção; Ver arena mostra a composição e o tabuleiro. O modelo não inicia uma partida real. O Claude deve aplicar o desenho aos controles e callbacks atuais do Unity, manter todas as abas existentes e usar o elenco completo. `ui/modelo-tela.visual.json` contém os tokens e critérios de integração. As capturas desktop/mobile estão em `ui/capturas`.

Os três mapas foram renderizados no Blender com fundo transparente para este modelo, usando os arquivos existentes sem modificar suas fontes. O tabuleiro inteiro permanece visível. Os retratos são cópias inalteradas das artes atuais.

## Conferência e estado

`verificar.py` passou: 231 assets, 2.864 quadros, 56 loops, zero erros e zero observações de emenda. Verifica hashes, alfa, margens, dimensões, pivôs, pedidos de quadros e igualdade de pixels dos reparos. O JavaScript passou na conferência de sintaxe. A navegação foi testada no navegador, incluindo teclado, busca sem acentos, equipe vazia, remoção, voltar, pausa/retomada e largura de 390 px.

Foram gastas 7 gerações na correção dos três clipes. Os exports de efeitos foram gratuitos na assinatura; o último saldo consultado foi 158 gerações. O Blender foi utilizado nas imagens do modelo, sem necessidade de esgotar os créditos.

A tarefa de integração é `coordenacao/arena-pixelai/tarefas/017-claude-integrar-reparos-efeitos-ui.md`. A fila 016 de animações corporais e os cenários estáticos ainda pendentes da tarefa 015 não são encerrados por esta entrega. A expansão bloqueada não foi alterada.
