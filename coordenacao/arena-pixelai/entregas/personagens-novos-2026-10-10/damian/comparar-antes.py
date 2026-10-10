from pathlib import Path
from PIL import Image, ImageDraw
here=Path(__file__).resolve().parent
root=here.parents[4]
source=root/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Damian'
sheet=Image.new('RGB',(640,300),'#263139')
draw=ImageDraw.Draw(sheet)
for row,clip in enumerate(['morph','hammer']):
    im=Image.open(source/f'{clip}_south-east.png').convert('RGBA')
    draw.text((5,row*150+4),f'{clip} - 0 2 4 6 8',fill='white')
    for col,frame in enumerate([0,2,4,6,8]):
        tile=im.crop((frame*im.height,0,(frame+1)*im.height,im.height))
        tile.thumbnail((120,120))
        sheet.paste(tile,(col*128,row*150+24),tile)
sheet.save(here/'antes.png')
print('comparativo salvo')
