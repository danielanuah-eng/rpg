from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json
ROOT=Path(__file__).resolve().parents[1]
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',22)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
themes=['bosque','pedreira','deserto','neve','vulcao']
sheet=Image.new('RGB',(1800,1100),'#243341');draw=ImageDraw.Draw(sheet)
for i,theme in enumerate(themes):
    x=(i%3)*600;y=(i//3)*550
    im=Image.open(ROOT/'previews'/(theme+'-clean.png'));im.thumbnail((600,470))
    sheet.paste(im,(x,y));draw.text((x+24,y+472),'Vulcão' if theme=='vulcao' else theme.capitalize(),font=font,fill='white')
draw.text((1225,650),'Materiais limpos',font=font,fill='white');draw.text((1225,695),'183 modelos verificados',font=small,fill='white')
draw.text((1225,730),'Geometria e pivôs preservados',font=small,fill='white')
sheet.save(ROOT/'previews/cinco-temas-clean.png')
inventory=json.loads((ROOT/'personagens/inventario.json').read_text(encoding='utf-8'))
for start in range(0,len(inventory),20):
    entries=inventory[start:start+20];sheet=Image.new('RGB',(1600,1400),'#263442');draw=ImageDraw.Draw(sheet)
    for i,entry in enumerate(entries):
        path=ROOT/'personagens'/entry['pasta']/'previa.png'
        if not path.exists():continue
        im=Image.open(path).convert('RGBA');im.thumbnail((310,310),Image.Resampling.LANCZOS)
        x=(i%5)*320;y=(i//5)*350
        sheet.paste(im,(x+(320-im.width)//2,y),im);draw.text((x+12,y+316),entry['pasta'],font=small,fill='white')
    sheet.save(ROOT/'personagens'/f'elenco-3d-{start//20+1}.png')
print('PRANCHAS_OK')
