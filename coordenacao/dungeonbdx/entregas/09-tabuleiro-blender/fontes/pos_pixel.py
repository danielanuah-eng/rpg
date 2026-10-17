# Reduz a paleta dos renders crus (pixel art 16 bits) e amplia 8x sem filtro.
# Uso: python pos_pixel.py <cru> <final>  -> PNGs 512x512 / 512x192, prancha.png e manifesto.json
import sys, json, hashlib
from pathlib import Path
from PIL import Image

cru, final = Path(sys.argv[1]), Path(sys.argv[2])
final.mkdir(parents=True, exist_ok=True)
SCALE = 8
COLORS = {'mosaic': 14, 'garden': 14, 'marble': 10}
itens, tiles = [], []
for src in sorted(cru.glob('*.png')):
    img = Image.open(src).convert('RGB')
    q = img.quantize(colors=COLORS.get(src.stem, 12), method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE).convert('RGB')
    big = q.resize((img.width * SCALE, img.height * SCALE), Image.NEAREST)
    dst = final / src.name
    big.save(dst, optimize=True)
    if src.stem != 'wall':
        tiles.append((src.stem, big))
    itens.append({
        'id': src.stem, 'tipo': 'parede' if src.stem == 'wall' else ('moldura' if src.stem == 'frame' else 'piso'),
        'origem': 'Blender 5.2 (fontes/tabuleiro_texturas.py) + fontes/pos_pixel.py',
        'caminho': dst.name, 'sha256': hashlib.sha256(dst.read_bytes()).hexdigest(),
        'projecao': 'frontal' if src.stem == 'wall' else 'topo',
        'largura': big.width, 'altura': big.height, 'pixelArte': [img.width, img.height],
    })
(final / 'manifesto.json').write_text(json.dumps({'lote': '09-tabuleiro-blender', 'itens': itens}, indent=2, ensure_ascii=False), encoding='utf-8')

# Prancha: cada piso repetido 2x2 (confere a emenda) reduzido à metade.
cell = 256
sheet = Image.new('RGB', (cell * 2 * 4 + 50, (cell * 2 + 10) * 2 + 10 + 110), (24, 22, 20))
for k, (name, big) in enumerate(tiles):
    small = big.resize((cell, cell), Image.NEAREST)
    ox, oy = 10 + (k % 4) * (cell * 2 + 10), 10 + (k // 4) * (cell * 2 + 10)
    for i in range(2):
        for j in range(2):
            sheet.paste(small, (ox + i * cell, oy + j * cell))
wall = Image.open(final / 'wall.png')
for i in range(4):
    sheet.paste(wall.resize((256, 96), Image.NEAREST), (10 + i * 256, sheet.height - 106))
sheet.save(final / 'prancha.png')
print('ok', len(itens))
