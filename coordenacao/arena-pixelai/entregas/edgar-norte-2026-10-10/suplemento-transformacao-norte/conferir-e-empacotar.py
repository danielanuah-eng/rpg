"""Decisões exatas para o PixelLab e verificação da exportação, sem redesenhar imagens."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, importlib.util, io, json, sys, zipfile

ROOT = Path(__file__).resolve().parent
BASE = ROOT.parent
TARGET = 'character:590cc7bc-3673-477c-aa23-f5655dd2d97a:north-east:ff56fd4f-3054-42af-a54a-7c68df8f6286'
PREFIX = 'Idle/animations/transform-ghost/north-east/'
BEFORE = BASE/'fontes/edgar-monitor-2026-10-10.zip'
AFTER = ROOT/'edgar-depois-suplemento.zip'

def sha(data): return hashlib.sha256(data).hexdigest()
def read_image(z, n): return Image.open(io.BytesIO(z.read(n))).convert('RGBA')
def write(name, obj): (ROOT/name).write_text(json.dumps(obj, ensure_ascii=False, indent=2), encoding='utf-8')

def prepare():
    spec = importlib.util.spec_from_file_location('decisions', BASE/'preparar-decisoes.py')
    module = importlib.util.module_from_spec(spec); spec.loader.exec_module(module)
    with zipfile.ZipFile(BEFORE) as z:
        names = sorted(n for n in z.namelist() if n.startswith(PREFIX) and n.endswith('.png'))
        assert len(names) == 9
        changes, metrics = zip(*(module.decision(read_image(z,n), i+1) for i,n in enumerate(names[:5])))
        for c,m in zip(changes,metrics):
            glow = {tuple(p) for p in m['cranialCyan']}
            c['pixels'] = [p for p in c['pixels'] if tuple(p[:2]) not in glow]
            c['reason'] = 'Somente pele exposta da coroa recebe cabelo grisalho curto e rente. Preservar brilho, corpo, pose e alfa da transformação.'
            m['transformationGlowPreserved'] = len(glow)
        write('decisoes.json', {'target':TARGET,'sourceZipSha256':sha(BEFORE.read_bytes()),'entries':names,'changes':changes,'metrics':metrics})
        print(json.dumps({'frames':len(changes),'hairPixels':sum(len(c['pixels']) for c in changes),'target':TARGET}))

def verify():
    d = json.loads((ROOT/'decisoes.json').read_text(encoding='utf-8'))
    assert d['sourceZipSha256'] == sha(BEFORE.read_bytes())
    a,b = zipfile.ZipFile(BEFORE),zipfile.ZipFile(AFTER)
    names = d['entries']; operations = {c['frames'][0]:c for c in d['changes']}
    changed = []; unchanged = 0; added = sorted(set(b.namelist())-set(a.namelist()))
    for n in a.namelist():
        if not n.endswith('.png'): continue
        assert n in b.namelist(), n
        old,new = read_image(a,n),read_image(b,n)
        assert old.size == new.size, n
        assert old.getchannel('A').tobytes() == new.getchannel('A').tobytes(), n
        expected = old.copy()
        if n in names[:5]:
            frame = names.index(n)+1
            for x,y,was,now in operations[frame]['pixels']:
                assert '#'+bytes(expected.getpixel((x,y))).hex() == was
                expected.putpixel((x,y), tuple(bytes.fromhex(now[1:])))
            changed.append(n)
        else: unchanged += 1
        assert expected.tobytes() == new.tobytes(), ('Mudança fora das decisões',n)
    frames = [read_image(b,n) for n in names]
    side = max(frames[0].size); w,h = frames[0].size
    sheet = Image.new('RGBA',(side*9,side)); offset=((side-w)//2,(side-h)//2)
    for i,frame in enumerate(frames): sheet.paste(frame,(i*side+offset[0],offset[1]))
    sprite = ROOT/'edgar-transform-ghost-north-east.png'; sheet.save(sprite)
    for i,frame in enumerate(frames):
        assert sheet.crop((i*side+offset[0],offset[1],i*side+offset[0]+w,offset[1]+h)).tobytes() == frame.tobytes()
    gallery = Image.new('RGB',(5*300,310),'#20252e'); draw=ImageDraw.Draw(gallery)
    for i,n in enumerate(names[:5]):
        for row,z in enumerate((a,b)):
            im=read_image(z,n).crop((30,15,110,85)).resize((160,140),Image.Resampling.NEAREST)
            gallery.paste(im,(i*300+65,row*150+20),im)
        draw.text((i*300+8,2),f'Quadro {i+1}: antes / depois',fill='white')
    gallery.save(ROOT/'coroa-antes-depois.png')
    review={'passed':True,'modifiedFrames':changed,'modifiedPixels':sum(len(c['pixels']) for c in operations.values()),'otherPngUnchanged':unchanged,'additionalEntries':added,'alphaUnchanged':True,'frameCount':9,'nativeFrameSize':[w,h],'frameSize':[side,side],'nativePaddingOffset':offset,'sha256':sha(sprite.read_bytes()),'visualReview':'Coroas dos cinco quadros humanos aprovadas; quadros espectrais e efeito original preservados, revisão da transformação completa cabe ao Claude.'}
    write('verificacao.json',review)
    write('manifesto.json',{'schemaVersion':1,'project':'ArenaTaticaUnity','source':'PixelLab','characterId':'590cc7bc-3673-477c-aa23-f5655dd2d97a','groupId':'ff56fd4f-3054-42af-a54a-7c68df8f6286','target':TARGET,'prompt':'Manter Edgar idoso com cabelo grisalho curto e rente nas vistas norte. Corrigir somente o topo exposto dos cinco quadros humanos recém-chegados. Preservar pose, corpo, alfa, efeito e os quatro quadros espectrais.','sourceZipSha256':sha(BEFORE.read_bytes()),'finalZipSha256':sha(AFTER.read_bytes()),'assets':[{'id':'edgar-transform-ghost-north-east-correcao-norte','file':sprite.name,'frames':9,'frameSize':[side,side],'nativeFrameSize':[w,h],'nativePaddingOffset':offset,'direction':'north-east','action':'transform-ghost','sourceGroupId':'ff56fd4f-3054-42af-a54a-7c68df8f6286','sha256':review['sha256'],'technicalVerified':True,'visualReview':'hair_fix_approved_codex_rest_preserved','integration':'pendente_confirmacao_claude','replacesEarlierEdgar':True}]})
    print(json.dumps(review,ensure_ascii=False))

if __name__=='__main__':
    prepare() if sys.argv[1:] == ['preparar'] else verify()
