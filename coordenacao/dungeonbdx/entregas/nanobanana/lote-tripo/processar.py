# Lote Tripo (NanoBanana/Gemini, gerado pelo autor): recorta o verde, reduz ao formato do contrato
# ISOMETRICO-2D, mede o alfa e grava manifesto.json. Uso: python -I processar.py <pasta-Tripo> <pasta-saida>
import sys, json, hashlib
from pathlib import Path
from PIL import Image

src, out = Path(sys.argv[1]), Path(sys.argv[2])
out.mkdir(parents=True, exist_ok=True)


def key_green(img):
    """Fundo verde -> transparente; borda esverdeada recebe alfa parcial e perde o verde (despill)."""
    img = img.convert('RGBA')
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            r, g, b, a = px[x, y]
            spill = g - max(r, b)
            if spill > 70 and g > 120:
                px[x, y] = (0, 0, 0, 0)
            elif spill > 25:
                alpha = int(255 * (70 - spill) / 45)
                px[x, y] = (r, max(r, b), b, alpha)
    return img


def crop_alpha(img):
    box = img.getchannel('A').point(lambda v: 255 if v > 16 else 0).getbbox()
    return img.crop(box) if box else img


def fit(img, w, h):
    return img.resize((w, h), Image.LANCZOS)


def scale_to(img, max_w, max_h):
    k = min(max_w / img.width, max_h / img.height)
    return img.resize((max(1, round(img.width * k)), max(1, round(img.height * k))), Image.LANCZOS)


JOBS = []  # (arquivo-fonte por prefixo, destino relativo, tipo, projeção, função)
def find(prefix):
    hits = [p for p in src.iterdir() if p.name.lower().startswith(prefix.lower())]
    if not hits:
        raise SystemExit('faltou ' + prefix)
    return hits[0]

def job(prefix, dest, tipo, projecao, fn, pivo=None):
    JOBS.append((prefix, dest, tipo, projecao, fn, pivo))

for s in ['crypt', 'garden', 'marble', 'mosaic', 'street', 'terracotta']:
    job('tile-' + s, f'Tiles/{s}.png', 'piso', 'topo', lambda im: fit(im.convert('RGB'), 512, 512))
job('Piso de madeira', 'Tiles/wood.png', 'piso', 'topo', lambda im: fit(im.convert('RGB'), 512, 512))
job('Parede da masmorra', 'Walls/wall.png', 'parede', 'frontal', lambda im: fit(im.convert('RGB'), 512, 192))
for t in ['captive', 'clue', 'emblem', 'explore', 'hazard', 'portal', 'relic', 'ritual']:
    job('token-' + t, f'Tokens/{t}.png', 'ficha', 'topo', lambda im: fit(crop_alpha(key_green(im)), 256, 256), [0.5, 0.5])
for d in ['ne', 'nw', 'se', 'sw']:
    job('prop-bookshelf_' + d, f'Props/bookshelf_{d}.png', 'prop', 'camera30',
        lambda im: scale_to(crop_alpha(key_green(im)), 256, 256), [0.5, 0.0])

def door(im):
    # Painel da porta é baixo (paredes de 0,26): fica a faixa central com tábuas, ferragens e argola.
    full = crop_alpha(key_green(im))
    band = full.crop((0, int(full.height * .30), full.width, int(full.height * .86)))
    return scale_to(band, 256, 256)
job('porta da masmorra', 'Doors/door.png', 'porta', 'frontal', door)
job('ui-button-disabled', 'UI/button-disabled.png', 'ui', 'frontal', lambda im: scale_to(crop_alpha(key_green(im)), 384, 384))
job('ui-button-selected', 'UI/button-selected.png', 'ui', 'frontal', lambda im: scale_to(crop_alpha(key_green(im)), 384, 384))
job('ui-button.png', 'UI/button.png', 'ui', 'frontal', lambda im: scale_to(crop_alpha(key_green(im)), 384, 384))
job('ui-panel', 'UI/panel.png', 'ui', 'frontal', lambda im: scale_to(crop_alpha(key_green(im)), 768, 768))
job('ui-card-frame', 'UI/card-frame.png', 'ui', 'frontal', lambda im: fit(crop_alpha(key_green(im)), 375, 525))

itens = []
for prefix, dest, tipo, projecao, fn, pivo in JOBS:
    f = find(prefix)
    res = fn(Image.open(f))
    p = out / dest
    p.parent.mkdir(parents=True, exist_ok=True)
    res.save(p, optimize=True)
    alpha = None
    if res.mode == 'RGBA':
        a = list(res.getchannel('A').getdata())
        alpha = {'transparente': round(sum(v < 16 for v in a) / len(a), 3), 'parcial': round(sum(16 <= v < 240 for v in a) / len(a), 3)}
    itens.append({'id': Path(dest).stem, 'tipo': tipo, 'origem': 'NanoBanana (Gemini), gerado pelo autor; pasta Tripo: ' + f.name,
                  'caminho': dest, 'sha256': hashlib.sha256(p.read_bytes()).hexdigest(), 'projecao': projecao,
                  'largura': res.width, 'altura': res.height, 'pivo': pivo, 'alfa': alpha})
    print(dest, res.size, alpha)
(out / 'manifesto.json').write_text(json.dumps({'lote': 'tripo-2026-10-16', 'itens': itens,
    'fora': ['Lyra faiska modelo.jpeg (referência de 1 quadro, fundo bege: sprites vão pelo PixelLab)']},
    indent=2, ensure_ascii=False), encoding='utf-8')
