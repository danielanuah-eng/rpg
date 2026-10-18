# Artes das cartas da loja (18/10/2026)

Origem: Claude, PixelLab `create_image_pixflux`, 64×64, fundo transparente, contorno preto, sombreamento médio, muito detalhe, seed 4417.
Prefixo de todos os prompts: "fantasy RPG item card art: ".

| Carta (id) | Nome no jogo | Prompt | Situação |
|---|---|---|---|
| yew-bow | Arco longo de teixo | modelado e renderizado no Blender 5.2 (`exemplo-blender/blender_bow.py`): arco de teixo afinando nas pontas, cabo de couro, corda e pontas de chifre; 64×64 sem antialias, rampas da paleta das cartas e contorno `#141217` | em uso (a versão PixelLab antiga, fina, está em `yew-bow-pixflux-antiga.png`) |
| leather-vest | Elmo acolchoado | padded quilted leather helmet with stitched seams and brass rivets | em uso |
| keen-dagger | Adaga afiada | keen steel dagger with a glinting razor edge and red wrapped hilt, diagonal | em uso |
| echo-staff | Cajado do eco | wooden magic staff topped by a glowing blue crystal inside a ring of echoing light, diagonal | em uso |
| heavy-crossbow | Besta pesada | desenhada à mão no editor gratuito do PixelLab (pixelart_workbench, desenho f7d8288d → 89f902d9): coronha de madeira, arco de aço, corda e virote | em uso |
| rune-greatsword | Machado rúnico | large double-bladed battle axe engraved with glowing cyan runes, diagonal | em uso |
| glass-arrows | Flechas de vidro-dragão | bundle of three arrows with translucent red dragon-glass arrowheads and dark feathers | em uso |
| scale-coat | Escudo de escamas | round shield covered in overlapping green dragon scales with a bronze rim, front view | em uso |
| obsidian-spear | Maça de obsidiana | black obsidian mace with a spiked flanged head and purple glassy sheen, diagonal | em uso |
| iron-blade | Lâmina de ferro batido | desenhada à mão no editor gratuito do PixelLab (pixelart_workbench, desenho 25788bf6): espada de ferro com marcas de martelo, guarda escura e cabo de couro | em uso |

No jogo: `ArenaTaticaUnity/Assets/DungeonBDX/Art/Resources/DungeonBDX/Iso/UI/Cards/<id>.png`. Sem arte, a carta usa o ícone genérico de `Iso/UI/Icons/`.

Atualização 18/10/2026: sem gerações de IA no plano, as duas artes que faltavam foram desenhadas por Claude no editor de pixels do PixelLab (sem custo), na mesma paleta e com o mesmo contorno preto. A loja agora segue a Fase de Melhorias do Arcadia: nível 1 = iron-blade, yew-bow, leather-vest, keen-dagger, echo-staff; nível 2 = heavy-crossbow, rune-greatsword, glass-arrows, scale-coat, obsidian-spear (5 cópias de cada).

Atualização 18/10/2026 (Blender): o autor aprovou o caminho **Blender para as artes das cartas** (retoque no editor gratuito do PixelLab quando precisar; Canva para a interface). O `yew-bow` foi o primeiro: `blender -b -P exemplo-blender/blender_bow.py -- <saída.png>` monta o modelo, renderiza com Workbench em 4× e reduz para 64×64 por cobertura, escolhe a faixa de cor de cada material pela luz e põe o contorno de 1 px. `exemplo-blender/comparacao-atual-x-blender.png` mostra a versão antiga e a nova (`compare.py` gera a comparação).
