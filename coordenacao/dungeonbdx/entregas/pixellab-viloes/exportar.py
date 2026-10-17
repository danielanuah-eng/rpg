"""Exporta personagens do PixelLab (modo padrão, quadro 180x180) para o formato das pastas da Arena.

Uso: python -I exportar.py <Pasta> <character_id> [<Pasta> <character_id> ...]
Todas as direções e quadros recebem o MESMO recorte quadrado justo (pés na base), para a figura
ocupar o quadro como os monstros da Arena (quadro 128 com a figura de ~122 px). Saída:
ArenaTatica/Resources/Sprites/<Pasta>/<direcao>.png e <clipe>_<direcao>.png (tira horizontal),
mais <Pasta>.json com sha256 nesta pasta.
"""
import sys, io, re, json, zipfile, hashlib, urllib.request
from pathlib import Path
from PIL import Image

HERE = Path(__file__).resolve().parent
SPRITES = HERE.parents[3] / 'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites'


def export(folder, cid):
    data = urllib.request.urlopen(f'https://api.pixellab.ai/mcp/characters/{cid}/download', timeout=180).read()
    (HERE / f'{folder}.zip').write_bytes(data)
    z = zipfile.ZipFile(io.BytesIO(data))
    stills, clips = {}, {}
    for name in z.namelist():
        if not name.endswith('.png'):
            continue
        parts = Path(name).parts
        if 'rotations' in parts:
            stills[Path(name).stem] = Image.open(io.BytesIO(z.read(name))).convert('RGBA')
        elif 'animations' in parts:
            i = parts.index('animations')
            clip = re.sub(r'-[0-9a-f]{8}$', '', parts[i + 1])
            clip = 'walk' if clip.startswith('walking') else 'idle' if 'idle' in clip else clip
            clips.setdefault((clip, parts[i + 2]), []).append(name)
    frames = {k: [Image.open(io.BytesIO(z.read(n))).convert('RGBA') for n in sorted(v, key=lambda s: int(re.sub(r"[^0-9]", "", Path(s).stem)))]
              for k, v in clips.items()}
    every = list(stills.values()) + [f for fs in frames.values() for f in fs]
    boxes = [im.getchannel('A').point(lambda a: 255 if a > 40 else 0).getbbox() for im in every]
    boxes = [b for b in boxes if b]
    x0, y0 = min(b[0] for b in boxes), min(b[1] for b in boxes)
    x1, y1 = max(b[2] for b in boxes), max(b[3] for b in boxes)
    side = max(x1 - x0, y1 - y0) + 4
    cx = (x0 + x1) // 2
    crop = (cx - side // 2, y1 + 2 - side, cx - side // 2 + side, y1 + 2)
    out = SPRITES / folder
    out.mkdir(parents=True, exist_ok=True)
    log = {'character_id': cid, 'frame': side, 'crop180': crop, 'files': {}}
    def save(img, name):
        p = out / name
        img.save(p)
        log['files'][name] = hashlib.sha256(p.read_bytes()).hexdigest()
    for d, im in stills.items():
        save(im.crop(crop), f'{d}.png')
    for (clip, d), fs in frames.items():
        strip = Image.new('RGBA', (side * len(fs), side), (0, 0, 0, 0))
        for k, f in enumerate(fs):
            strip.paste(f.crop(crop), (k * side, 0))
        save(strip, f'{clip}_{d}.png')
    (HERE / f'{folder}.json').write_text(json.dumps(log, indent=1), encoding='utf-8')
    print(folder, 'quadro', side, 'estáticos', len(stills), 'clipes', sorted({c for c, _ in frames}))


args = sys.argv[1:]
for i in range(0, len(args), 2):
    export(args[i], args[i + 1])
