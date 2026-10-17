"""Ataque e dano sem gerar no PixelLab: inclina e desloca o quadro parado (pixel exato, sem filtro).

Uso: python -I golpe-dano.py <Pasta> [<Pasta> ...]
Lê Sprites/<Pasta>/<diagonal>.png e grava attack_<diagonal>.png (6 quadros: recua, carrega, avança, volta)
e hit_<diagonal>.png (4 quadros: tranco para trás com clarão vermelho que some).
Só as diagonais leste (a figura olha para a direita): o animador espelha sudoeste/noroeste.
"""
import sys, json, hashlib
from pathlib import Path
from PIL import Image

HERE = Path(__file__).resolve().parent
SPRITES = HERE.parents[3] / 'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites'
# (inclinação do topo em px por px de altura, deslocamento horizontal em px)
ATTACK = ((0, 0), (-.05, -1), (-.09, -2), (.12, 4), (.07, 3), (.02, 1))
# (inclinação, deslocamento, força do clarão)
HIT = ((-.08, -3, .45), (-.05, -2, .25), (-.02, -1, .1), (0, 0, 0))


def lean(still, k, dx):
    """Shear around the feet: the top moves k px per px above the base; the whole figure moves dx."""
    box = still.getchannel('A').getbbox()
    base = box[3] if box else still.height
    # x_src = x + k*(y - base) - dx  →  rows above the base slide right when k > 0.
    out = still.transform(still.size, Image.AFFINE, (1, k, -k * base - dx, 0, 1, 0), resample=Image.NEAREST)
    return out


def flash(frame, amount):
    if not amount:
        return frame
    tint = Image.new('RGBA', frame.size, (255, 70, 60, 0))
    mixed = Image.blend(frame, tint, amount)
    mixed.putalpha(frame.getchannel('A'))
    return mixed


def strip(frames):
    w, h = frames[0].size
    out = Image.new('RGBA', (w * len(frames), h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        out.paste(f, (i * w, 0))
    return out


for folder in sys.argv[1:]:
    out = SPRITES / folder
    log_path = HERE / f'{folder}.json'
    log = json.loads(log_path.read_text(encoding='utf-8'))
    for d in ('south-east', 'north-east'):
        still = Image.open(out / f'{d}.png').convert('RGBA')
        made = {
            f'attack_{d}.png': strip([lean(still, k, dx) for k, dx in ATTACK]),
            f'hit_{d}.png': strip([flash(lean(still, k, dx), a) for k, dx, a in HIT]),
        }
        for name, image in made.items():
            image.save(out / name)
            log['files'][name] = hashlib.sha256((out / name).read_bytes()).hexdigest()
    log['golpe'] = 'golpe-dano.py: ataque (6 quadros) e dano (4 quadros) a partir do quadro parado, sem geração'
    log_path.write_text(json.dumps(log, indent=1), encoding='utf-8')
    print(folder, 'ataque e dano ok')
