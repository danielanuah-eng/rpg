# Contrato visual — DungeonBDX 2D isométrico (Claude, 16/10/2026)

Vale para todo pedido ao Gemini/NanoBanana (via GPT/Codex) e para Blender como referência.
A Unity é a dona da projeção: **as artes não vêm "achatadas" em losango**, exceto personagens/props,
que são desenhados já vistos do ângulo da câmera.

## Projeção (implementada em `Runtime/AqBoardView.cs`)

| Item | Valor |
|---|---|
| Câmera | Ortográfica, inclinação **30°** (losango 2:1), giro 360° que repousa em **45° + 90°·k** |
| Grid lógico | `AqCell(X,Y)` quadrado, 1 casa = 1 unidade de mundo; movimento continua ortogonal |
| Personagens | Sprites em pé, giram só no eixo vertical; esticados por 1/cos30 para manter a proporção desenhada |
| Ordem de desenho | Pela profundidade dos pés (SortingGroup na âncora); paredes/portas opacas ocultam pela profundidade real |
| Duas figuras na casa | Atrás-esquerda e frente-direita; Colosso/enormes ficam centralizados |
| Direção do sprite | Recalculada em relação à câmera a cada giro (8 direções) |

Na vista de repouso, os eixos do tabuleiro aparecem nas diagonais da tela. Por isso **as 4 diagonais
são as direções obrigatórias** dos clipes; as 4 ortogonais são opcionais (o motor usa a diagonal mais próxima).

## Formatos por tipo de asset

| Tipo | Vista | Tamanho | Fundo | Pivô | Destino na Unity (Claude copia após conferir) |
|---|---|---|---|---|---|
| Piso por superfície | **De cima, sem perspectiva**, emendável nas 4 bordas, sem sombra direcional embutida | 512×512 | Opaco | — | `Resources/DungeonBDX/Iso/Tiles/<superficie>.png` |
| Parede | **Elevação frontal**, emendável na horizontal | 512×192 | Opaco | — | `Resources/DungeonBDX/Iso/Walls/wall.png` |
| Porta (painel fechado) | Elevação frontal | 256×256 | Transparente | base central | `Resources/DungeonBDX/Iso/Doors/door.png` (integração pendente) |
| Ficha no chão (token) | De cima, redonda | 256×256 | Transparente | centro | `Resources/DungeonBDX/Iso/Tokens/<tipo>.png` (integração pendente) |
| Prop (estante, mesa, barris, fonte, estátua, sarcófago, runa, árvore, baú) | Câmera a 30°, 4 diagonais | 256×256 | Transparente | pés (0,5; 0,1) | `Resources/DungeonBDX/Iso/Props/<prop>_<direcao>.png` (integração pendente) |
| Herói/monstro | Câmera a 30°, corpo inteiro | quadro 176×176 (enormes 256×256), tira horizontal | Transparente (alfa real) | (0,5; 0,153) como a Arena; PPU 128 | `ArenaTatica/Resources/Sprites/<Pasta>/` |
| Moldura de carta | Frontal | 750×1050 (63×88) | Transparente | — | `Resources/DungeonBDX/Iso/UI/` |
| Botão / painel | Frontal, 9-slice com bordas de 32 px | 256×96 / 512×256 | Transparente | — | `Resources/DungeonBDX/Iso/UI/` |

Superfícies existentes: `street`, `crypt`, `wood`, `terracotta`, `mosaic`, `marble`, `garden`.
Sem arte entregue, o motor usa a textura procedural atual. Piso e parede já têm o gancho no código.

## Personagens: nomes, clipes e tempo

Manter o padrão da Arena: estáticos `<direcao>.png` (8 direções; mínimo as 4 diagonais) e clipes
`<clipe>_<direcao>.png`, quadros da esquerda para a direita, fatiados pela Unity como `<clipe>_<direcao>_<n>`.
Direções: `south`, `south-east`, `east`, `north-east`, `north`, `north-west`, `west`, `south-west`.

| Clipe | FPS | Quadros | Loop | Observação |
|---|---|---|---|---|
| idle | 5 | 4–8 | sim | pés parados no pivô |
| walk | 10 | 6–8 | sim | sem deslocar a figura no quadro |
| attack / cast / summon / clipes especiais | 7 | 6–9 | não | indicar o quadro de impacto no manifesto |
| hit | 8 | 3–4 | não | |
| death | 6 | 6–9 | não | o último quadro fica caído |

A escala no jogo vem da tabela de alturas (`AqHeroImport.Height`: comuns 1,5; Colosso 2,4; Brutamontes 2,5).
A figura deve encostar os pés no pivô e ocupar a altura útil do quadro. Não reduzir o Colosso para caber no quadro de 176.

## Textos e números

Vida, defesa, custo, alcance, dados, nomes e textos são UI da Unity sobre as molduras. **Nunca embutir valores no PNG.**

## Manifesto obrigatório por lote

`entregas/nanobanana/<lote>/manifesto.json`, um item por arquivo:
`id`, `tipo`, `origem` (ferramenta + prompt), `caminho`, `sha256`, `projecao` ("topo" | "frontal" | "camera30"),
`largura`, `altura`, `pivo`, `ppu`, `clipe`, `direcao`, `quadros`, `fps`, `loop`, `quadroImpacto`.
Arte que ainda não existe não entra no manifesto.

## O que realmente falta (levantado em 16/10)

Os 28 heróis com pasta da Arena já têm idle/walk/attack/hit/death nas 4 diagonais e estáticos em 8 direções: **servem como estão**.

| Prioridade | Asset | Situação no jogo hoje |
|---|---|---|
| 1 | Pisos: 7 superfícies | procedural |
| 1 | Parede (1 estilo) | bloco liso |
| 2 | `lyra-faisca`, `orion-prisma`, `vaelis-vinculo`: conjunto completo (8 estáticos + idle/walk/ataque/hit/death nas 4 diagonais) | sem sprite; usam modelo FBX sem rig ou cápsula |
| 2 | `baltazar`: `hit` e `death` nas 4 diagonais | usa reação/morte procedural |
| 3 | Fichas no chão: clue, explore, emblem, captive, ritual, relic, hazard, portal | cilindros com letra |
| 3 | Moldura de carta, botões (normal/selecionado/desativado), painéis do HUD inferior | UI lisa |
| 4 | Props em 4 diagonais e painel de porta | primitivas 3D |

**Amostra antes do lote:** 1 piso (`wood`), a parede e 1 herói faltante (`lyra-faisca`, só `idle` + estáticos).
Claude integra a amostra e confirma antes de pedir o restante.
