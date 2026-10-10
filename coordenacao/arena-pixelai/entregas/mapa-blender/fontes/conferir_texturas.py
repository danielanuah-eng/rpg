import bpy
import json
from pathlib import Path
root = Path(__file__).resolve().parents[1]
originals = root / 'fontes' / 'pixellab-originais'
sheet = bpy.data.images.new('Contatos', width=64*4, height=64*4, alpha=True)
pixels = [0.06,0.08,0.1,1] * (256*256)
checks = []
for i in range(16):
    image = bpy.data.images.load(str(originals / f'tile_{i}.png'))
    w,h = image.size
    src = list(image.pixels)
    ox,oy=(i%4)*64,(3-i//4)*64
    assert (w,h) == (64,64), (i,w,h)
    for y in range(h):
        offset=((oy+y)*256+ox)*4
        pixels[offset:offset+w*4]=src[y*w*4:(y+1)*w*4]
    alphas=src[3::4]
    checks.append({'tile':i,'dimensoes':[w,h],'alfaMin':min(alphas),'alfaMax':max(alphas)})
sheet.pixels=pixels
sheet.filepath_raw=str(root/'previews'/'texturas-contato.png')
sheet.file_format='PNG'
sheet.save()
(root/'fontes'/'texturas-auditoria.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
print('TEXTURAS_CONFERIDAS',len(checks))
