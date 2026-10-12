from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import re,json,hashlib

ROOT=Path(__file__).resolve().parents[1]
PROJECT=ROOT.parents[3]
SPRITES=PROJECT/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites'
folders='Ciantia Talyssa Teantha Devon Diantus Damian Ezequiela TchuTcha TchuTchaBerserk ArqueiroJack Perry DragaoEspectral LoboCinzas Asamyh GatoLaranja GatoBranco GatoMalhado GatoManchado Colosso CheiroPneu ReiBobett PrincipeKellen Baltazar VespaCinzenta ViuvaAgulha GuardiaoMusgo DomadorCorrentes OgroPedra Edgar EdgarFantasma EdgarNavio Brumak Ossyro Gryvro Alvyr Raizor Nixira TotemCura PapaMoedas BrutamontesErrante'.split()
inventory=[]
for folder in folders:
    src=SPRITES/folder
    reference=src/'south-east.png'
    if not reference.exists(): reference=src/'idle_south-east.png'
    if not reference.exists(): reference=next(src.rglob('*.png'))
    actions={}
    for path in sorted(src.rglob('*.png')):
        match=re.match(r'(.+)_(south-east|south-west|north-east|north-west|east|west|north|south)$',path.stem)
        if match: actions.setdefault(match.group(1),[]).append(str(path.relative_to(PROJECT)))
    inventory.append({'pasta':folder,'referencia':str(reference.relative_to(PROJECT)),'sha256':hashlib.sha256(reference.read_bytes()).hexdigest(),'acoes':actions,'estado':'a_modelar_no_blender'})
for name,ref in [('BaltazarAnjo',SPRITES/'Efeitos/baltazar-anjo.png'),('BaltazarDemonio',SPRITES/'Efeitos/baltazar-demonio.png'),('Vendedor',SPRITES/'Vendedor/frame-01.png')]:
    inventory.append({'pasta':name,'referencia':str(ref.relative_to(PROJECT)),'sha256':hashlib.sha256(ref.read_bytes()).hexdigest(),
      'acoes':{'idle':[str(ref.relative_to(PROJECT))]},'tipo':'companheiro_visual' if name.startswith('Baltazar') else 'npc_loja',
      'estado':'a_modelar_no_blender'})
(ROOT/'personagens').mkdir(exist_ok=True)
(ROOT/'personagens/inventario.json').write_text(json.dumps(inventory,ensure_ascii=False,indent=2),encoding='utf-8')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
for start in range(0,len(inventory),20):
    entries=inventory[start:start+20]
    sheet=Image.new('RGB',(1200,1000),'#263442');draw=ImageDraw.Draw(sheet)
    for i,entry in enumerate(entries):
        im=Image.open(PROJECT/entry['referencia']).convert('RGBA')
        if im.width>im.height: im=im.crop((0,0,im.height,im.height))
        im.thumbnail((200,205),Image.Resampling.NEAREST)
        x=(i%5)*240+(240-im.width)//2;y=(i//5)*250+16
        sheet.paste(im,(x,y),im)
        draw.text(((i%5)*240+12,(i//5)*250+224),entry['pasta'],font=font,fill='white')
    sheet.save(ROOT/'personagens'/f'referencias-{start//20+1}.png')
print('INVENTARIO',len(inventory),'ACOES',sum(len(e['acoes']) for e in inventory))
