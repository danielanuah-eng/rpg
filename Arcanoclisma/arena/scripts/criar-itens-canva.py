"""Três modelos visuais de Item, em 63 x 88 mm; números e textos editáveis."""
from pathlib import Path
from reportlab.pdfgen import canvas
from reportlab.lib.units import mm
from reportlab.lib.colors import HexColor
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import pypdfium2 as pdfium
ROOT=Path(__file__).resolve().parent.parent
pdfmetrics.registerFont(TTFont('CardTitle','C:/Windows/Fonts/georgiaz.ttf'))
pdfmetrics.registerFont(TTFont('CardBody','C:/Windows/Fonts/segoeui.ttf'))
W,H=63*mm,88*mm
out=ROOT/'templates'/'modelos-itens-canva.pdf'
c=canvas.Canvas(str(out),pagesize=(W,H))
c.setTitle('Arcanoclisma - Canal, Armadura e Extra')
def text(s,x,y,size=7,color='#21313c',font='CardBody'):
    c.setFont(font,size);c.setFillColor(HexColor(color));c.drawCentredString(x*mm,H-y*mm,s)
for key,label in [('canal','Canal'),('armadura','Armadura'),('extra','Extra')]:
    c.setFillColor(HexColor('#81979e'));c.rect(0,0,W,H,stroke=0,fill=1)
    c.drawImage(str(ROOT/'templates'/f'overlay-{key}-v3.png'),0,0,W,H,mask='auto')
    text('[Nome do Item]',31.5,6.3,8,'#f5ead7','CardTitle')
    for x,y in [(5.2,6.2),(5.2,14.1),(5.2,23.3)]:text('X',x,y,8,'#fff5df')
    text('Item',31.5,67.2,5.5)
    text(label+' · [Subtipos]',31.5,71,5.2)
    text('[Texto de regras]',31.5,77.5,6)
    text('[Ambientação opcional]',31.5,82.5,4.8)
    if key=='armadura':
        text('X',46,61,8,'#fff5df');text('X',55.5,61,8,'#fff5df')
    c.showPage()
c.save()
doc=pdfium.PdfDocument(str(out))
for i,key in enumerate(['canal','armadura','extra']):
    doc[i].render(scale=4).to_pil().save(str(ROOT/'registros'/f'modelo-{key}-preview.png'))
print(str(out))
