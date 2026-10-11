"""Doze avisos originais desenhados em pixels, sem API ou geracao paga."""
from pathlib import Path
from PIL import Image, ImageDraw
import colorsys, hashlib, json, math
ROOT=Path(__file__).resolve().parent
SIZE=96;COUNT=12;DURATION=160
EVENTS=[('chuva-de-moedas','Chuva de moedas'),('queda-de-pedras','Queda de pedras'),('terremoto','Terremoto'),('mare-de-lava','Mare de lava'),('portal-caotico','Portal caotico'),('eco-ancestral','Eco ancestral'),('mare-de-energia','Mare de energia'),('festival-de-fogos','Festival de fogos'),('nevoeiro','Nevoeiro'),('vendaval','Vendaval'),('papa-moedas','Papa-moedas'),('errante','Errante')]
INK='#262438'; ROCK='#545769'; ROCKL='#92999f'; WOOD='#9b704a'; GOLD='#e4b452'; LIGHT='#ffe6ad'; BLUE='#55b5d2'; TEAL='#6bd2b0'; VIOLET='#ae88de'; PINK='#e494b2'

def art(event,frame):
    im=Image.new('RGBA',(SIZE,SIZE));d=ImageDraw.Draw(im)
    phase=2*math.pi*frame/COUNT
    hue=(frame/COUNT+.11*EVENTS.index(next(e for e in EVENTS if e[0]==event)))%1
    rgb=colorsys.hsv_to_rgb(hue,.40,.87)
    accent=tuple(round(v*255) for v in rgb)+(255,)
    def poly(points,c,outline=INK,w=2):
        d.polygon(points,fill=c);d.line(points+[points[0]],fill=outline,width=w,joint='curve')
    def rect(box,c,outline=INK,w=2):d.rectangle(box,fill=c,outline=outline,width=w)
    def ellipse(box,c,outline=INK,w=2):d.ellipse(box,fill=c,outline=outline,width=w)
    def line(points,c,w=2):d.line(points,fill=c,width=w,joint='curve')
    def coin(x,y,r=10):
        ellipse((x-r,y-r,x+r,y+r),GOLD);d.arc((x-r+3,y-r+3,x+r-3,y+r-3),100,260,fill='#9c683d',width=3)
        poly([(x,y-4),(x+3,y),(x,y+4),(x-3,y)],LIGHT,w=1)
    def spark(x,y,c=LIGHT):
        line([(x-3,y),(x+3,y)],c);line([(x,y-3),(x,y+3)],c)
    def slab(x=48,y=57):
        poly([(x-28,y),(x,y-14),(x+28,y),(x,y+15)],ROCKL)
        poly([(x-28,y),(x,y+15),(x,y+25),(x-28,y+10)],ROCK)
        poly([(x,y+15),(x+28,y),(x+28,y+10),(x,y+25)],'#3b4054')
    # A moldura e os brilhos mudam de cor lentamente; o simbolo permanece legivel.
    for x,y in ((12,29),(80,22),(84,72)):
        line([(x,y-2),(x,y+2)],accent,1);line([(x-2,y),(x+2,y)],accent,1)
    if event=='chuva-de-moedas':
        for x,y in ((28,26),(61,38),(41,65)):
            line([(x-4,y-17),(x-4,y-12)],BLUE);coin(x,y)
        poly([(57,68),(74,63),(82,68),(66,78)],'#795b43');coin(67,64,8)
        spark(17,51,accent);spark(77,43)
    elif event=='queda-de-pedras':
        slab(y=63)
        for pts,c in [([(28,18),(42,15),(49,25),(43,40),(29,36),(23,27)],ROCKL),([(59,30),(73,29),(81,38),(76,52),(63,52),(56,42)],ROCK),([(42,48),(51,45),(57,54),(50,60),(40,57)],'#afb1a5')]:
            poly(pts,c);line([pts[0],pts[1],pts[2]],'#c3c7bd',2)
        line([(19,15),(26,25)],BLUE);line([(64,16),(72,24)],accent)
        for x,y in ((21,66),(31,79),(78,69)):rect((x,y,x+3,y+2),'#c9bc9a',w=1)
    elif event=='terremoto':
        slab(y=55);poly([(25,48),(40,43),(47,50),(44,57),(31,61)],'#bbc1b4')
        poly([(44,50),(48,55),(44,62),(55,69),(50,75),(54,77),(61,68),(51,60),(54,52)],'#c88055',w=1)
        line([(9,47),(14,43),(10,39)],accent);line([(81,60),(87,56),(83,52)],accent)
        poly([(69,38),(75,33),(80,37),(76,44)],ROCKL,w=1)
    elif event=='mare-de-lava':
        slab(y=57)
        poly([(24,56),(36,52),(43,57),(51,53),(69,55),(59,63),(66,67),(51,73),(46,64),(34,64)],'#cb6543')
        line([(28,57),(36,56),(44,61),(51,59),(61,60)],'#ffc768',3)
        poly([(43,34),(41,46),(50,47),(58,39),(56,29),(52,38),(48,30),(48,19)],'#e78147')
        poly([(47,35),(46,42),(50,43),(53,39),(51,35),(50,39)],LIGHT,w=1)
        ellipse((25,42,34,48),'#ee9250',w=1);ellipse((63,43,69,48),'#ee9250',w=1)
        line([(34,29),(31,26),(33,22)],ROCKL,3)
    elif event=='portal-caotico':
        ellipse((22,16,76,79),'#533b78');ellipse((27,22,70,74),VIOLET);ellipse((32,29,64,68),'#243044')
        d.arc((33,32,65,65),45,320,fill=BLUE,width=4);d.arc((40,38,58,59),190,470,fill=accent,width=3)
        spark(48,49,LIGHT)
        for x,y in ((19,22),(73,31),(19,68),(74,75)):
            poly([(x,y-3),(x+3,y),(x,y+3),(x-3,y)],GOLD,w=1)
    elif event=='eco-ancestral':
        poly([(32,71),(48,77),(65,70),(62,80),(48,85),(34,80)],ROCK)
        rect((44,33,51,72),GOLD);rect((46,29,49,67),LIGHT,outline=LIGHT,w=1)
        ellipse((31,19,65,53),'#bc8847');ellipse((37,25,59,47),GOLD)
        poly([(48,28),(53,36),(48,44),(43,36)],LIGHT,w=1)
        for p in [[(48,9),(48,15)],[(23,31),(29,31)],[(67,31),(74,31)],[(30,14),(35,19)],[(62,16),(67,11)]]:line(p,accent)
        line([(35,57),(35,68)],TEAL,2);line([(60,56),(60,68)],TEAL,2)
    elif event=='mare-de-energia':
        ellipse((23,22,73,73),'#326276');ellipse((29,29,67,67),'#35877e')
        poly([(53,13),(33,47),(45,47),(39,79),(66,40),(52,40)],TEAL)
        poly([(50,26),(41,43),(50,43),(47,58),(58,44),(48,44)],LIGHT,outline=LIGHT,w=1)
        d.arc((16,32,80,68),15,150,fill=accent,width=3);d.arc((16,31,80,67),190,340,fill=BLUE,width=3)
    elif event=='festival-de-fogos':
        for x,y,r,c in ((29,31,16,PINK),(64,24,14,GOLD),(63,58,17,TEAL)):
            for a in range(0,360,45):
                t=math.radians(a);line([(round(x+6*math.cos(t)),round(y+6*math.sin(t))),(round(x+r*math.cos(t)),round(y+r*math.sin(t)))],c,3)
            spark(x,y,LIGHT)
        line([(37,80),(40,64),(38,49)],VIOLET,2);line([(70,82),(72,69)],accent,2)
        poly([(19,72),(24,63),(29,72),(25,79)],PINK,w=1)
    elif event=='nevoeiro':
        for y,c in ((29,'#879ba9'),(46,'#abbac0'),(64,'#d3dbd0')):
            pts=[(15,y+8),(18,y),(28,y),(33,y-5),(42,y-5),(48,y),(61,y),(67,y-4),(76,y),(80,y+8),(75,y+13),(21,y+13)]
            poly(pts,c,outline='#536776')
            line([(24,y+8),(43,y+8),(49,y+4),(64,y+4)],'#e1e5d8',2)
        line([(23,78),(39,78)],accent);line([(52,20),(70,20)],accent)
    elif event=='vendaval':
        d.arc((14,16,69,42),180,365,fill=BLUE,width=4);line([(13,40),(52,40)],BLUE,4)
        d.arc((37,39,84,65),185,490,fill='#d2e2d5',width=4);line([(17,53),(55,53)],'#d2e2d5',4)
        d.arc((18,53,59,78),10,185,fill=VIOLET,width=3)
        poly([(24,22),(34,18),(33,27),(26,31)],GOLD,w=1);line([(25,29),(33,21)],'#856844',1)
        poly([(62,69),(74,67),(69,78),(60,80)],TEAL,w=1);line([(61,78),(72,69)],'#407359',1)
    elif event=='papa-moedas':
        ellipse((21,68,76,81),ROCK);ellipse((28,61,70,78),'#312f42')
        poly([(26,66),(23,45),(31,32),(43,27),(59,31),(71,47),(67,66),(54,74),(37,74)],'#a6794e')
        poly([(29,47),(34,36),(45,32),(57,36),(64,47),(63,59),(54,68),(37,67)],'#c79560',w=1)
        ellipse((33,37,43,47),LIGHT,w=1);ellipse((51,36,61,46),LIGHT,w=1)
        rect((38,39,40,44),INK,w=1);rect((53,38,55,43),INK,w=1)
        ellipse((36,51,60,69),INK);coin(50,58,8)
        poly([(22,52),(17,61),(27,65),(31,57)],'#a6794e');poly([(66,52),(77,57),(74,65),(65,62)],'#a6794e')
        coin(72,24,7);spark(17,36,accent)
    elif event=='errante':
        poly([(17,78),(20,31),(31,17),(45,13),(59,17),(77,34),(80,78),(69,78),(69,39),(58,27),(40,24),(28,39),(28,78)],ROCK)
        poly([(32,76),(31,50),(37,38),(46,33),(60,37),(69,51),(68,75)],'#65595c')
        poly([(38,39),(43,28),(48,37)],'#c6b09a');poly([(56,38),(62,29),(65,44)],'#c6b09a')
        poly([(40,46),(58,43),(61,54),(50,60),(39,55)],'#a79d87')
        line([(43,46),(47,47)],'#edb972',2);line([(53,46),(57,45)],'#edb972',2)
        poly([(28,61),(35,55),(40,65),(38,77),(27,77)],ROCKL);poly([(61,59),(70,56),(77,70),(71,78),(61,73)],ROCKL)
        poly([(32,17),(41,17),(38,24),(32,25)],VIOLET,w=1);spark(75,17,accent)
    # Variacao cromatica suave no desenho, mantendo contorno e alfa exatos.
    data=[];ink=tuple(int(INK[i:i+2],16) for i in (1,3,5))
    pulse=.97+.03*math.cos(phase)
    for r,g,b,a in im.get_flattened_data():
        if a and (r,g,b)!=ink:
            rgb=tuple(round((v*.88+accent[i]*.12)*pulse) for i,v in enumerate((r,g,b)))
            data.append((*rgb,a))
        else:data.append((r,g,b,a))
    im.putdata(data)
    return im

def main():
    contact=Image.new('RGB',(4*248,3*248),(27,31,44));cd=ImageDraw.Draw(contact);manifest=[]
    for index,(event,title) in enumerate(EVENTS):
        dest=ROOT/event;dest.mkdir(parents=True,exist_ok=True);frames=[art(event,f) for f in range(COUNT)]
        atlas=Image.new('RGBA',(SIZE*4,SIZE*3))
        for f,img in enumerate(frames):
            img.save(dest/f'{f:02d}.png');atlas.paste(img,((f%4)*SIZE,(f//4)*SIZE))
        atlas.save(dest/'atlas.png');frames[0].save(dest/'estatico.png')
        # Cada frame usa paleta local exata; o encoder adaptativo pode deslocar RGB.
        indexed=[]
        for image in frames:
            pixels=list(image.get_flattened_data());colors=sorted({p[:3] for p in pixels if p[3]})
            assert len(colors)<256
            table={c:i+1 for i,c in enumerate(colors)}
            p=Image.new('P',image.size);p.putdata([table[c[:3]] if c[3] else 0 for c in pixels])
            palette=[0,0,0]+[v for c in colors for v in c]
            p.putpalette(palette+[0]*(768-len(palette)));p.info['transparency']=0
            indexed.append(p)
        indexed[0].save(dest/'preview.gif',save_all=True,append_images=indexed[1:],duration=DURATION,loop=0,disposal=2,transparency=0,optimize=False,include_color_table=True)
        paths=sorted(dest.glob('*.png'))
        manifest.append({'id':event,'nome':title,'frames':COUNT,'frameSize':[SIZE,SIZE],'layout':{'columns':4,'rows':3,'order':'left-to-right, top-to-bottom'},'durationsMs':[DURATION]*COUNT,'cycleMs':DURATION*COUNT,'loop':True,'pivot':[.5,.5],'presentation':{'anchor':'screen-center','sampling':'Point','mipmaps':False,'oneCycleOnCataclisma':True,'suggestedLogicalSizePx':192,'staticForReducedMotion':f'{event}/estatico.png'},'atlas':f'{event}/atlas.png','preview':f'{event}/preview.gif','files':[{'path':p.relative_to(ROOT).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths],'alpha':'binary; verified separately','integration':'pendente_claude'})
        x=(index%4)*248;y=(index//4)*248
        contact.paste(frames[0].resize((192,192),Image.Resampling.NEAREST),(x+28,y+14),frames[0].resize((192,192),Image.Resampling.NEAREST))
        cd.text((x+12,y+214),title,fill='#dbe5e6')
    contact.save(ROOT/'contato.png')
    contract={'version':1,'project':'Arena Tatica','request':'2026-10-11; aviso central conforme evento do dado Cataclisma','method':'desenho original procedural local; sem PixelLab ou API de imagem','generationCost':0,'source':'ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Match.cs:797','trigger':{'type':'cataclisma','idField':'info'},'integrationStatus':'pendente_claude','events':manifest}
    (ROOT/'eventos.visual.json').write_text(json.dumps(contract,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
    cards=''.join(f'<button data-id="{event}"><img src="{event}/estatico.png" alt="{title}"><span>{title}</span></button>' for event,title in EVENTS)
    html='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Cataclisma — avisos centrais</title><style>body{background:#1b1f2c;color:#dbe5e6;font:16px system-ui;margin:24px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:10px;max-width:920px}button{background:#292e40;color:inherit;border:1px solid #49536b;border-radius:8px;padding:10px;cursor:pointer}button img{width:120px;height:120px;image-rendering:pixelated;display:block;margin:auto}span{display:block}#stage{height:290px;margin:20px 0;background:#202838;display:flex;flex-direction:column;align-items:center;justify-content:center;border:1px solid #465069;border-radius:12px}canvas{width:192px;height:192px;image-rendering:pixelated}label{margin-left:12px}h1{font-size:22px}</style><h1>Avisos do Cataclisma</h1><p>Escolha um evento para ver um ciclo de cor no centro.</p><button id="toggle">Pausar</button><label><input type="checkbox" id="static"> Movimento reduzido</label><div id="stage"><canvas width="96" height="96"></canvas><p id="name"></p></div><main>''' + cards + '''</main><script>const data='''+json.dumps(dict(EVENTS),ensure_ascii=False)+''';const c=document.querySelector('canvas'),ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;let paused=false,reduced=matchMedia('(prefers-reduced-motion: reduce)').matches,selected=Object.keys(data)[0],atlas=new Image(),start=performance.now();document.querySelector('#static').checked=reduced;function pick(id){selected=id;atlas.src=id+'/atlas.png';start=performance.now();document.querySelector('#name').textContent=data[id]}document.querySelectorAll('[data-id]').forEach(b=>b.onclick=()=>pick(b.dataset.id));document.querySelector('#static').onchange=e=>reduced=e.target.checked;document.querySelector('#toggle').onclick=e=>{paused=!paused;e.target.textContent=paused?'Continuar':'Pausar'};let held=0;function paint(t){if(!paused)held=Math.floor((t-start)/160)%12;let f=reduced?0:held;ctx.clearRect(0,0,96,96);if(atlas.complete&&atlas.naturalWidth)ctx.drawImage(atlas,(f%4)*96,Math.floor(f/4)*96,96,96,0,0,96,96);requestAnimationFrame(paint)}pick(selected);requestAnimationFrame(paint);</script></html>'''
    (ROOT/'preview.html').write_text(html,encoding='utf-8',newline='\n');print('12 eventos; 144 quadros; atlas, estaticos e previews produzidos.')

if __name__=='__main__':main()
