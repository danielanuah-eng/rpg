"""Inspeciona fontes intactas e prepara receita PixelLab; nao altera PNGs canonicos."""
from pathlib import Path
from PIL import Image
import base64, io, json, re, sys, zipfile

ROOT = Path(__file__).resolve().parent
CLIPS = [
    ('baltazar', 'attack', 'west', 'bb5036c6-0a52-45b8-b97b-3f80e9d55fbb'),
    ('baltazar', 'attack', 'south-west', 'bb5036c6-0a52-45b8-b97b-3f80e9d55fbb'),
    ('baltazar', 'idle', 'west', '9e2189d7-02e2-442b-b32a-9389955e834d'),
    ('baltazar', 'idle', 'south-west', '9e2189d7-02e2-442b-b32a-9389955e834d'),
    ('baltazar', 'walk', 'west', '2e791df3-cf33-47e2-b61f-c83d9d071dc5'),
    ('baltazar', 'walk', 'south-west', '2e791df3-cf33-47e2-b61f-c83d9d071dc5'),
    ('guardiao-de-musgo', 'idle', 'north-west', 'aa7ea27d-1641-4cd4-994b-f17c3abb08d2'),
]

def prepare(c):
    actor, action, direction, group = c
    with zipfile.ZipFile(ROOT/'fontes'/f'{actor}-antes.zip') as z:
        paths = sorted([p for p in z.namelist() if '/animations/' in '/'+p and f'/{action}/' in p and f'/{direction}/' in p and p.endswith('.png')], key=lambda p:int(re.search(r'\d+$',Path(p).stem).group()))
        if not paths:
            raise RuntimeError(f'Fonte ausente: {actor}/{action}/{direction}; nomes: {z.namelist()[:20]}')
        frames = [Image.open(io.BytesIO(z.read(p))).convert('RGBA') for p in paths]
    w,h=frames[0].size
    assert all(f.size==(w,h) for f in frames)
    boxes=[f.getchannel('A').getbbox() for f in frames]
    union=[min(b[0] for b in boxes),min(b[1] for b in boxes),max(b[2] for b in boxes),max(b[3] for b in boxes)]
    dx=(w-(union[2]-union[0]))//2-union[0]
    dy=(h-(union[3]-union[1]))//2-union[1]
    return {'actor':actor,'action':action,'direction':direction,'group':group,'size':[w,h],'frames':len(frames),'bbox':boxes,'union':union,'centralizarDelta':[dx,dy],'touchesBorder':any(b[0]==0 or b[1]==0 or b[2]==w or b[3]==h for b in boxes)},frames

if __name__=='__main__':
    report=[prepare(c)[0] for c in CLIPS]
    (ROOT/'inspecao-fontes.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
    print(json.dumps(report,ensure_ascii=False))
