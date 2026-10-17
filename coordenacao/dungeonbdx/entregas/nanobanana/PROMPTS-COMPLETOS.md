# NanoBanana (Gemini) — amostra 01 do DungeonBDX isométrico

Claude, 16/10/2026. Sem ponte ativa com o Gemini: o autor cola cada prompt no Gemini (NanoBanana)
e salva o resultado em `entregas/nanobanana/amostra-01/` com o nome indicado. Claude recorta,
reduz, confere o alfa e integra. Contrato: `contratos/ISOMETRICO-2D.md`.

Regras para todos os prompts:
- Uma imagem por pedido (o Gemini erra tiras e grades; Claude monta as tiras).
- Fundo de recorte **verde chapado #00FF00** quando o asset é transparente (o Gemini não entrega alfa real).
- Nada de texto, número, logotipo ou moldura dentro da imagem.
- Estilo: miniatura de jogo de tabuleiro pintada à mão, cores saturadas, contorno escuro suave, luz de cima à esquerda, sem sombra projetada no chão.

## 1. Piso `wood` → `amostra-01/tile-wood.png`

```
Top-down orthographic texture of an old wooden plank floor for a fantasy board game dungeon tile.
Seen exactly from above, no perspective, no vanishing point. Warm brown oak planks running
horizontally, worn edges, a few nail heads and subtle scratches. Seamless and tileable on all
four edges. Even lighting, no directional shadow, no vignette, no objects, no text.
Square image, 1024x1024.
```

## 2. Parede → `amostra-01/wall.png`

```
Front elevation texture of a low dungeon wall for a fantasy board game, seen straight on,
no perspective. Rough grey-brown stone blocks with mortar lines and a thin gold trim at the top
edge. Seamless and tileable horizontally (left and right edges match). Even lighting,
no shadow, no objects, no text. Wide image, 1536x576 (aspect 8:3).
```

## 3. Herói `lyra-faisca` (Lyra Faísca) → `amostra-01/lyra/<direcao>.png`

Personagem: pistoleira jovem, cabelo ruivo curto com mechas que soltam faíscas, casaco curto de couro
marrom com detalhes de latão, lenço vermelho, revólver de cinza (cano escuro com brasas laranja) na mão direita.
Gerar primeiro `south-east` e repetir o mesmo texto trocando só a direção, anexando a primeira imagem
como referência de personagem para manter a identidade.

```
Full-body game miniature sprite of Lyra Faisca, a young fantasy gunslinger: short red hair with
small sparks, short brown leather coat with brass details, red scarf, ash-grey revolver with
glowing orange embers held in her right hand. Standing idle pose, feet together on the ground.
Camera looks down at 30 degrees (isometric board game view). She faces SOUTH-EAST (toward the
viewer's lower right). Whole body visible, feet near the bottom of the image, figure fills about
80% of the image height, centered. Flat pure green background #00FF00, no ground, no shadow,
no text. Square image, 1024x1024.
```

Direções (trocar `SOUTH-EAST (toward the viewer's lower right)`):
| arquivo | texto |
|---|---|
| `south-east.png` | SOUTH-EAST (toward the viewer's lower right) |
| `south-west.png` | SOUTH-WEST (toward the viewer's lower left) |
| `north-east.png` | NORTH-EAST (away from the viewer, toward the upper right; we see her back three-quarters) |
| `north-west.png` | NORTH-WEST (away from the viewer, toward the upper left; we see her back three-quarters) |

### 3b. `idle` (4 quadros por diagonal) → `amostra-01/lyra/idle_<direcao>_<n>.png`

Mesmo prompt da direção, anexando o estático como referência, trocando `Standing idle pose, feet together on the ground.` por:
| n | pose |
|---|---|
| 0 | `Idle breathing pose, frame 1 of 4: neutral stance, revolver lowered.` |
| 1 | `Idle breathing pose, frame 2 of 4: chest slightly raised, sparks in the hair brighter.` |
| 2 | `Idle breathing pose, frame 3 of 4: shoulders slightly lowered, revolver tilts a little up.` |
| 3 | `Idle breathing pose, frame 4 of 4: back to neutral, coat tail slightly swaying.` |

Pés no mesmo ponto em todos os quadros. Total da amostra: 2 texturas + 4 estáticos + 16 quadros.

## Lote 02 (depois da amostra conferida) → `entregas/nanobanana/lote-02/`

### Pisos restantes → `tile-<superficie>.png`
Mesmo prompt do piso `wood`, trocando a descrição do material:
| superfície | material |
|---|---|
| `street` | worn cobblestone street, grey and ochre rounded stones, moss in the gaps |
| `crypt` | dark crypt flagstones, cracked slate slabs, faint dust |
| `terracotta` | terracotta square tiles, sun-faded orange, chipped corners |
| `mosaic` | small mosaic tiles in teal, cream and gold geometric pattern |
| `marble` | white and grey veined marble slabs, polished, thin dark seams |
| `garden` | short green grass with small clover and a few tiny flowers |

### Fichas no chão → `token-<tipo>.png`
```
Top-down round board game token, seen exactly from above, thick painted rim, centered,
filling 90% of the image, symbol: <SYMBOL>. Fantasy dungeon style, saturated colors,
soft dark outline. Flat pure green background #00FF00, no shadow, no text, no numbers.
Square image, 1024x1024.
```
| tipo | símbolo (`<SYMBOL>`) |
|---|---|
| `clue` | a rolled parchment scroll with a magnifying glass, blue rim |
| `explore` | a lantern over a map, gold rim |
| `emblem` | a heraldic shield with four quarters, gold rim |
| `captive` | broken shackles, pink rim |
| `ritual` | a purple glowing rune circle, violet rim |
| `relic` | an ancient golden chalice, amber rim |
| `hazard` | a skull over crossed spikes, red rim |
| `portal` | a swirling blue portal, teal rim |

### Painel de porta → `door.png`
```
Front view of a closed dungeon door panel for a fantasy board game: heavy bronze-banded oak,
round iron ring handle, small teal gem on top. Seen straight on, no perspective, centered,
bottom edge touching the bottom of the image. Flat pure green background #00FF00, no text.
Square image, 1024x1024.
```

### Props (4 diagonais) → `prop-<prop>_<direcao>.png`
```
Game miniature prop: <PROP>. Camera looks down at 30 degrees (isometric board game view),
object seen from the <DIRECTION> side. Painted fantasy board game style, soft dark outline.
Base touching the bottom of the image, centered, filling about 80% of the image.
Flat pure green background #00FF00, no ground, no shadow, no text. Square image, 1024x1024.
```
Props: `bookshelf` (tall wooden bookshelf full of old books), `table` (round tavern table with a candle),
`barrels` (two stacked wooden barrels), `fountain` (small stone fountain with water), `statue`
(stone knight statue on a plinth), `sarcophagus` (stone sarcophagus with carved lid), `rune`
(floor rune stone with violet glow), `tree` (small leafy oak), `chest` (iron-bound treasure chest).
Direções: `south-east`, `south-west`, `north-east`, `north-west` (mesmo texto da seção do herói).

### UI → `ui-<peca>.png`
```
Fantasy board game UI element: <PIECE>. Front view, flat, painted bronze and dark slate,
soft gold trim, empty center (no text, no icons), borders even so it can be 9-sliced.
Flat pure green background #00FF00. <SIZE>.
```
| peça | `<PIECE>` | `<SIZE>` |
|---|---|---|
| `button` | rectangular button | wide image 1024x384 |
| `button-selected` | rectangular button glowing teal edge | wide image 1024x384 |
| `button-disabled` | rectangular button, desaturated grey | wide image 1024x384 |
| `panel` | large dashboard panel | wide image 1536x768 |
| `card-frame` | vertical playing card frame with a window for art and a lower text box | portrait image 750x1050 |

### Heróis faltantes (conjunto completo, 4 diagonais)
Usar o método do herói `lyra-faisca`: estáticos das 4 diagonais e quadros por clipe, anexando o estático como referência.
| herói | descrição |
|---|---|
| `orion-prisma` | young arcane prism mage, long midnight-blue robe with prism crystals on the shoulders, short silver hair, a floating triangular prism in the left hand |
| `vaelis-vinculo` | elven binder, green-and-bronze leather armor, long dark braid, glowing chains of light coiled around both forearms |
Clipes por diagonal: `idle` 4 quadros, `walk` 6, `attack` 6 (impacto no 4º), `hit` 3, `death` 6 (último caído).
Pose por quadro: descrever a fase do movimento ("walk cycle frame 2 of 6: left foot forward", etc.).

### `baltazar`: `hit` (3 quadros) e `death` (6 quadros) nas 4 diagonais
Anexar `ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Baltazar/south-east.png` como referência e pedir só as poses novas.

## Depois da amostra

Claude reduz para os tamanhos do contrato (piso 512×512, parede 512×192, quadro 176×176, PPU 128, pivô 0,5/0,153),
recorta o verde, mede o alfa, gera o `manifesto.json` e integra. O restante do lote só é pedido após conferir a amostra no jogo.
