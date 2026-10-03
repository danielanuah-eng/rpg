"""Base vetorial: uma carta por página, corte de 63 x 88 mm. Sem regras inventadas."""
from pathlib import Path
from reportlab.pdfgen import canvas
from reportlab.lib.units import mm
from reportlab.lib.colors import HexColor
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import pypdfium2 as pdfium

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'templates' / 'base-generica-canva.pdf'
pdfmetrics.registerFont(TTFont('CardSans', 'C:/Windows/Fonts/segoeui.ttf'))
pdfmetrics.registerFont(TTFont('CardSerif', 'C:/Windows/Fonts/georgia.ttf'))
c = canvas.Canvas(str(OUT), pagesize=(63*mm,88*mm))
c.setTitle('Arcanoclisma - templates genéricos em 63 x 88 mm')
W,H = 63*mm,88*mm
def rect(x,y,w,h,color,alpha=1):
    c.saveState(); c.setFillAlpha(alpha); c.setFillColor(HexColor(color))
    c.rect(x*mm,H-(y+h)*mm,w*mm,h*mm,stroke=0,fill=1);c.restoreState()
def text(s,x,y,size=7,color='#172433',font='CardSans',center=False):
    c.setFont(font,size); c.setFillColor(HexColor(color))
    (c.drawCentredString if center else c.drawString)(x*mm,H-y*mm,s)
types=[('Genérico','#638697'),('Aliado','#638697'),('Temporária','#876d97'),('Mística','#71866e'),('Estrutura','#968568')]
for name,tint in types:
    # Área de arte ocupa a página toda: substituí-la por cena full art, abaixo dos campos.
    rect(0,0,63,88,'#667e87')
    rect(1,1,61,86,'#a6b1ac')
    rect(1,1,61,8,'#142837',.72)
    text('[Nome da carta]',34,6.5,8,'#fff4df','CardSerif',True)
    text('ILUSTRAÇÃO',34,43,7,'#506169','CardSans',True)
    c.setStrokeColor(HexColor('#1c2a39'));c.setLineWidth(.5)
    # Esfera simples: X é o total gerado, cor substituível por Arcano.
    c.setFillColor(HexColor(tint));c.circle(5.2*mm,H-5.1*mm,3.4*mm,stroke=1,fill=1)
    text('X',5.2,6.3,8,'#ffffff','CardSans',True)
    c.setFillColor(HexColor('#1c2a39'));c.setStrokeColor(HexColor('#a5bac5'))
    c.circle(5.2*mm,H-13.2*mm,3.1*mm,stroke=1,fill=1)
    text('X',5.2,14.4,7,'#f6eddb','CardSans',True)
    p=c.beginPath();p.moveTo(5.2*mm,H-19*mm);p.lineTo(8.3*mm,H-22.1*mm)
    p.lineTo(5.2*mm,H-25.2*mm);p.lineTo(2.1*mm,H-22.1*mm);p.close()
    c.setFillColor(HexColor('#1c2a39'))
    c.drawPath(p,stroke=1,fill=1);text('X',5.2,23.3,7,'#f6eddb','CardSans',True)
    # Texto em faixa leve, baixa, com a ilustração visível sob ela.
    rect(2,69,59,16.5,'#f0ebde',.83)
    text('[Tipo / Subtipo]',4,72,5.4)
    text('[Texto de regras]',4,76,6)
    text('[Ambientação opcional]',4,80,5,'#49535b','CardSerif')
    text('•',4.5,84,6,'#606d78')
    for cx,fill,is_life in [(46,'#385a52',False),(55.5,'#713f4c',True)]:
        c.setFillColor(HexColor(fill));c.setStrokeColor(HexColor('#bccbbf'));c.setLineWidth(.45)
        p=c.beginPath()
        if is_life:
            p.moveTo((cx-2.9)*mm,H-61.9*mm);p.lineTo((cx+2.9)*mm,H-61.9*mm)
            p.lineTo((cx+2.9)*mm,H-66.55*mm);p.lineTo(cx*mm,H-68.1*mm)
            p.lineTo((cx-2.9)*mm,H-66.55*mm)
        else:
            p.moveTo(cx*mm,H-61.9*mm)
            for dx,y in [(2.9,63.45),(2.9,66.55),(0,68.1),(-2.9,66.55),(-2.9,63.45)]:
                p.lineTo((cx+dx)*mm,H-y*mm)
        p.close();c.drawPath(p,stroke=1,fill=1)
        text('X',cx,66.1,8,'#fff6df','CardSans',True)
    c.showPage()
c.save()
doc=pdfium.PdfDocument(str(OUT))
doc[0].render(scale=4).to_pil().save(str(ROOT/'registros'/'base-canva-preview.png'))
print(str(OUT))
