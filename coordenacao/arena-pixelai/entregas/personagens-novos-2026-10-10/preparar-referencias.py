from pathlib import Path
from PIL import Image, ImageDraw
import json, shutil

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
refs = ['grifo-de-vidro','urso-raiz','cervo-branco','velho-ossario-dragao-palido','povo-golias','povo-goblins','povo-verdejantes']
folder = HERE / 'referencias'
folder.mkdir(parents=True, exist_ok=True)
sheet = Image.new('RGB',(1000,600),'#243039')
draw = ImageDraw.Draw(sheet)
records = []
for i, name in enumerate(refs):
    source = ROOT / 'Arcanoclisma' / 'arte' / 'imagens' / f'{name}.png'
    if not source.exists():
        records.append({'id':name,'missing':True})
        continue
    shutil.copy2(source, folder / source.name)
    sprite = Image.open(source).convert('RGBA')
    sprite.thumbnail((228,250))
    x,y = (i%4)*250,(i//4)*300
    sheet.paste(sprite,(x+(250-sprite.width)//2,y+28+(250-sprite.height)//2),sprite)
    draw.text((x+6,y+8),name,fill='white')
    records.append({'id':name,'source':str(source),'width':Image.open(source).width,'height':Image.open(source).height})
sheet.save(folder/'referencias.png')
(folder/'fontes.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(records,ensure_ascii=False))
