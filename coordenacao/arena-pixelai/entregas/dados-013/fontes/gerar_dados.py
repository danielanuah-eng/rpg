"""Dados originais: primitivas em grade de 64px, exportadas em 128px.

Não utiliza modelo remoto nem copia símbolos do manual. A rolagem usa
projeção ortográfica de um cubo com as mesmas seis faces, em 10 quadros.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import math
import json
import hashlib

HERE = Path(__file__).resolve().parent
OUT = HERE.parent
ASSETS = OUT / "Dados"
ASSETS.mkdir(parents=True, exist_ok=True)
INK = "#182c41"
CREAM = "#fff0cd"
LABELS = ["Cataclisma", "Escudo", "Trava", "Esquiva", "Esquiva / Trava", "Coringa"]
NAMES = ["cataclisma", "escudo", "trava", "esquiva", "coringa-esquiva-trava", "coringa"]

def die():
    im = Image.new("RGBA", (64,64))
    d = ImageDraw.Draw(im)
    d.polygon([(11,7),(52,7),(57,12),(57,52),(52,57),(12,57),(7,52),(7,12)], fill=INK)
    d.polygon([(12,9),(51,9),(55,13),(55,51),(51,55),(13,55),(9,51),(9,13)], fill="#b58142")
    d.polygon([(13,10),(50,10),(53,13),(53,50),(50,53),(13,53),(10,50),(10,13)], fill=CREAM)
    d.polygon([(13,10),(50,10),(52,12),(13,12),(12,13),(12,49),(10,49),(10,13)], fill="#fffaf0")
    d.polygon([(13,50),(50,50),(50,13),(53,13),(53,50),(50,53),(13,53)], fill="#dec08d")
    d.line([(14,15),(49,15)],fill="#e5cb98",width=1)
    d.line([(14,15),(14,48)],fill="#e5cb98",width=1)
    return im

def shield():
    im=Image.new("RGBA",(32,32)); d=ImageDraw.Draw(im)
    d.polygon([(5,3),(26,3),(28,6),(26,21),(16,29),(6,21),(3,6)],fill=INK)
    d.polygon([(7,5),(24,5),(25,7),(23,19),(16,25),(9,19),(6,7)],fill="#267ca0")
    d.polygon([(7,5),(16,5),(16,23),(10,18)],fill="#66d7df")
    d.polygon([(11,8),(21,8),(22,10),(20,18),(16,21),(12,18),(10,10)],fill="#164a70")
    d.line([(12,8),(19,8)],fill="#a3fff4",width=2)
    return im

def chain():
    im=Image.new("RGBA",(32,32)); d=ImageDraw.Draw(im)
    d.rounded_rectangle((4,1,17,20),radius=4,fill=INK)
    d.rounded_rectangle((6,3,15,18),radius=3,fill="#edb640")
    d.rounded_rectangle((9,6,12,14),radius=1,fill=CREAM)
    d.rounded_rectangle((14,12,27,30),radius=4,fill=INK)
    d.rounded_rectangle((16,14,25,28),radius=3,fill="#d5882b")
    d.rounded_rectangle((19,17,22,25),radius=1,fill=CREAM)
    d.line([(7,5),(7,14)],fill="#fff3a0",width=2)
    d.line([(17,16),(17,23)],fill="#ffd65b",width=2)
    d.rectangle((12,13,19,18),fill=INK)
    d.rectangle((12,13,17,15),fill="#edb640")
    return im

def wing():
    im=Image.new("RGBA",(32,32)); d=ImageDraw.Draw(im)
    d.polygon([(2,20),(14,6),(28,3),(24,12),(20,13),(22,16),(16,20),(16,23),(8,28),(7,23)],fill=INK)
    d.polygon([(6,19),(15,8),(24,6),(21,10),(16,13),(19,14),(14,17),(13,21),(9,24),(9,20)],fill="#37a9a3")
    d.polygon([(8,18),(16,10),(22,8),(14,17)],fill="#9aecd1")
    d.line([(1,25),(5,25)],fill="#317f89",width=2)
    d.line([(1,29),(8,29)],fill="#317f89",width=2)
    return im

def rift():
    im=Image.new("RGBA",(32,32)); d=ImageDraw.Draw(im)
    pts=[(16,0),(20,9),(29,5),(24,14),(32,17),(23,21),(27,29),(18,25),(13,32),(11,23),(2,27),(7,18),(0,13),(10,11),(7,3)]
    d.polygon(pts,fill=INK)
    pts=[(16,5),(18,12),(25,10),(20,16),(26,18),(19,20),(21,25),(16,22),(13,27),(13,20),(7,22),(11,16),(6,14),(14,13),(12,8)]
    d.polygon(pts,fill="#b14ba4")
    d.polygon([(16,8),(18,14),(22,16),(18,19),(15,24),(13,18),(10,16),(14,13)],fill="#ee87c3")
    d.polygon([(16,12),(20,16),(16,21),(12,16)],fill=INK)
    d.rectangle((16,14,17,17),fill="#ffdf84")
    return im

def wildcard():
    im=Image.new("RGBA",(32,32)); d=ImageDraw.Draw(im)
    for x,y,color,shine in [(10,3,"#347da0","#8eeae9"),(1,17,"#d8962d","#ffe781"),(19,17,"#37a9a3","#a6f2d4")]:
        d.polygon([(x+6,y),(x+12,y+6),(x+6,y+12),(x,y+6)],fill=INK)
        d.polygon([(x+6,y+2),(x+10,y+6),(x+6,y+10),(x+2,y+6)],fill=color)
        d.line([(x+4,y+5),(x+6,y+3)],fill=shine,width=2)
    d.line([(16,15),(16,19)],fill=INK,width=2)
    d.line([(12,20),(8,22)],fill=INK,width=2)
    d.line([(20,20),(24,22)],fill=INK,width=2)
    return im

icons=[rift(),shield(),chain(),wing(),None,wildcard()]
pair=Image.new("RGBA",(32,32))
small_chain=chain().resize((19,19),Image.Resampling.NEAREST)
small_wing=wing().resize((19,19),Image.Resampling.NEAREST)
pair.paste(small_chain,(0,11),small_chain)
pair.paste(small_wing,(13,1),small_wing)
icons[4]=pair
faces=[]
for name,icon in zip(NAMES,icons):
    im=die(); im.paste(icon,(16,16),icon)
    faces.append(im)
    im.resize((128,128),Image.Resampling.NEAREST).save(ASSETS/f"{name}.png")

# Cubo com índices das faces frente, direita, atrás, esquerda, cima, baixo.
vertices=[(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]
face_indices=[(4,5,6,7),(5,1,2,6),(1,0,3,2),(0,4,7,3),(7,6,2,3),(0,1,5,4)]
textures=[]
for face in faces:
    tex=face.crop((8,8,56,56))
    flat=Image.new("RGBA",tex.size,CREAM); flat.alpha_composite(tex)
    textures.append(flat)

def transform(v,yaw,pitch,roll):
    x,y,z=v
    x,z=x*math.cos(yaw)+z*math.sin(yaw),-x*math.sin(yaw)+z*math.cos(yaw)
    y,z=y*math.cos(pitch)-z*math.sin(pitch),y*math.sin(pitch)+z*math.cos(pitch)
    return (x*math.cos(roll)-y*math.sin(roll), x*math.sin(roll)+y*math.cos(roll), z)

def triangle(im,pts,uv,tex,shade):
    a,b,c=pts
    den=(b[1]-c[1])*(a[0]-c[0])+(c[0]-b[0])*(a[1]-c[1])
    if abs(den)<0.05:return
    p=im.load(); q=tex.load()
    for y in range(max(0,math.floor(min(t[1] for t in pts))),min(64,math.ceil(max(t[1] for t in pts))+1)):
        for x in range(max(0,math.floor(min(t[0] for t in pts))),min(64,math.ceil(max(t[0] for t in pts))+1)):
            aa=((b[1]-c[1])*(x+0.5-c[0])+(c[0]-b[0])*(y+0.5-c[1]))/den
            bb=((c[1]-a[1])*(x+0.5-c[0])+(a[0]-c[0])*(y+0.5-c[1]))/den
            cc=1-aa-bb
            if min(aa,bb,cc)<-0.015:continue
            u=max(0,min(47,round(aa*uv[0][0]+bb*uv[1][0]+cc*uv[2][0])))
            v=max(0,min(47,round(aa*uv[0][1]+bb*uv[1][1]+cc*uv[2][1])))
            r,g,bv,_=q[u,v]
            p[x,y]=(round(r*shade),round(g*shade),round(bv*shade),255)

frames=[]
for i in range(10):
    t=i/10
    verts=[transform(v,2*math.pi*t,2*math.pi*t,0.20*math.sin(2*math.pi*t)) for v in vertices]
    extent=max(max(v[0] for v in verts)-min(v[0] for v in verts),max(v[1] for v in verts)-min(v[1] for v in verts))
    scale=46/extent
    pts=[(32+v[0]*scale,32-v[1]*scale) for v in verts]
    im=Image.new("RGBA",(64,64))
    order=sorted(range(6),key=lambda k:sum(verts[j][2] for j in face_indices[k])/4)
    for k in order:
        ids=face_indices[k]; quad=[pts[j] for j in ids]
        cross=(quad[1][0]-quad[0][0])*(quad[2][1]-quad[0][1])-(quad[1][1]-quad[0][1])*(quad[2][0]-quad[0][0])
        if cross>=-0.01:continue
        shade=0.80+0.20*min(1,abs(cross)/(scale*scale*4))
        triangle(im,quad[:3],[(0,47),(47,47),(47,0)],textures[k],shade)
        triangle(im,[quad[0],quad[2],quad[3]],[(0,47),(47,0),(0,0)],textures[k],shade)
        ImageDraw.Draw(im).line(quad+[quad[0]],fill=INK,width=1)
    frames.append(im.resize((128,128),Image.Resampling.NEAREST))
strip=Image.new("RGBA",(1280,128))
for i,im in enumerate(frames):strip.paste(im,(i*128,0))
strip.save(ASSETS/"rolagem.png")
frames[0].save(OUT/"rolagem-primeiro-quadro.png")
frames[0].save(OUT/"rolagem.gif",save_all=True,append_images=frames[1:],duration=70,loop=0,disposal=2,transparency=0)

preview=Image.new("RGB",(960,300),"#233440")
d=ImageDraw.Draw(preview)
font=ImageFont.truetype("C:/Windows/Fonts/arial.ttf",15)
for i,(name,label,face) in enumerate(zip(NAMES,LABELS,faces)):
    im=face.resize((128,128),Image.Resampling.NEAREST)
    preview.paste(im,(16+i*158,12),im)
    d.text((16+i*158,144),label,font=font,fill="white")
    im=face # Tamanho real de 64px para conferência do símbolo.
    preview.paste(im,(48+i*158,180),im)
d.text((16,267),"Faces em 128px acima e 64px abaixo. Arte original em grade, paleta e contorno consistentes.",font=font,fill="#c6dfde")
preview.save(OUT/"previa.png")

assets=[]
for path in sorted(ASSETS.glob("*.png")):
    im=Image.open(path)
    alpha=im.getchannel("A")
    assert im.size==((1280,128) if path.stem=="rolagem" else (128,128))
    assert alpha.getextrema()==(0,255)
    if path.stem=="rolagem":
        for i in range(10):
            assert im.crop((i*128,0,(i+1)*128,128)).getchannel("A").getbbox()
    assets.append({"arquivo":str(path.relative_to(OUT)).replace("\\","/"),"sha256":hashlib.sha256(path.read_bytes()).hexdigest(),"dimensoes":list(im.size),"alphaMinMax":list(alpha.getextrema())})
manifest={"id":"013-dados","versao":"v01","estado":"entregue-aguarda-importacao-e-teste-claude",
    "origem":"desenho original deterministico com primitivas; sem copia do Krosmaster ou modelo remoto",
    "destino":"ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Dados/",
    "arquivos":assets,"rolagem":{"quadros":10,"tamanhoQuadro":[128,128],"ordem":"esquerda para direita","fps":14,"duracaoQuadros":10/14,"duracaoLogicaAtual":0.7,"resultado":"face final definida pelo motor, nunca pela animacao"},
    "importacaoSugerida":{"spritePixelsPerUnit":128,"pivot":[0.5,0.5],"compression":"None","filterMode":"Point","mipmaps":False,"observacao":"dados da HUD; testar separadamente dos personagens minimizados em 3D"},
    "procedimento":"fontes/gerar_dados.py","revisao":"alfa e dimensoes verificados; conferir previa e quadros; jogo ainda nao testado"}
(OUT/"dados.visual.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding="utf-8")
print(json.dumps({"entrega":str(OUT),"assets":len(assets),"rolagemFrames":len(frames),"alpha":"0-255 em todos"},ensure_ascii=False))
