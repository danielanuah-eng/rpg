"""Idle de respiração sem gerar no PixelLab: desce a parte de cima do quadro parado 0-1-2-1 px.

Uso: python -I idle-respiracao.py <Pasta> [<Pasta> ...]
Lê Sprites/<Pasta>/<diagonal>.png e grava idle_<diagonal>.png (tira de 4 quadros).
Só as diagonais leste: o animador espelha sudoeste/noroeste a partir delas.
"""
import sys, json, hashlib
from pathlib import Path
from PIL import Image

HERE = Path(__file__).resolve().parent
SPRITES = HERE.parents[3] / 'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites'
SHIFTS = (0, 1, 2, 1)


def breathe(still, shift):
    if not shift:
        return still.copy()
    box = still.getchannel('A').getbbox()
    # Head and torso (top 55% of the figure) sink; the legs stay planted.
    cut = box[1] + int((box[3] - box[1]) * .55)
    frame = still.copy()
    top = still.crop((0, 0, still.width, cut))
    frame.paste((0, 0, 0, 0), (0, 0, still.width, cut))
    frame.alpha_composite(top, (0, shift))
    return frame


for folder in sys.argv[1:]:
    out = SPRITES / folder
    log_path = HERE / f'{folder}.json'
    log = json.loads(log_path.read_text(encoding='utf-8'))
    for d in ('south-east', 'north-east'):
        still = Image.open(out / f'{d}.png').convert('RGBA')
        strip = Image.new('RGBA', (still.width * len(SHIFTS), still.height), (0, 0, 0, 0))
        for k, s in enumerate(SHIFTS):
            strip.paste(breathe(still, s), (k * still.width, 0))
        name = f'idle_{d}.png'
        strip.save(out / name)
        log['files'][name] = hashlib.sha256((out / name).read_bytes()).hexdigest()
    log['idle'] = 'idle-respiracao.py: quadro parado com a parte de cima descendo 0-1-2-1 px (sem geração)'
    log_path.write_text(json.dumps(log, indent=1), encoding='utf-8')
    print(folder, 'idle ok')
