"""Confere replay da montagem e empacotamento sem alterar pixels nativos."""
from pathlib import Path
from PIL import Image
import zipfile,io,json,hashlib,collections
ROOT=Path(__file__).resolve().parent
def png(z,n):return Image.open(io.BytesIO(z.read(n))).convert('RGBA')
def same(a,b):return a.size==b.size and a.tobytes()==b.tobytes()
def main():
    before=zipfile.ZipFile(ROOT/'baltazar-completo-v2-antes-da-montagem.zip')
    after=zipfile.ZipFile(ROOT/'baltazar-completo.zip')
    swapped={f'Idle/animations/attack-v2/south-east/frame_{i:03}.png':f'Idle/animations/attack-v2/south-east/frame_{j:03}.png' for i,j in ((4,3),(6,5))}
    unchanged=0
    for n in before.namelist():
        if not n.endswith('.png'):continue
        expected=png(before,swapped.get(n,n));assert same(expected,png(after,n)),n
        if n not in swapped:unchanged+=1
    manifest=json.loads((ROOT/'manifesto.json').read_text(encoding='utf-8'))
    assert collections.Counter(a['action'] for a in manifest['assets'])=={'attack':8,'idle':8,'rotation':8,'walk':8}
    anchors=[]
    for a in manifest['assets']:
        im=Image.open(ROOT/a['file']).convert('RGBA');w,h=a['frameSize'];nw,nh=a['nativeFrameSize'];dx,dy=a['paddingOrigin']
        assert w==h and im.size==(w*a['frames'],h)
        assert hashlib.sha256((ROOT/a['file']).read_bytes()).hexdigest()==a['sha256']
        count={'attack':9,'idle':4,'rotation':1,'walk':6}[a['action']];assert a['frames']==count
        prefix=f"Idle/rotations/{a['direction']}.png" if a['action']=='rotation' else f"Idle/animations/{a['sourceAction']}/{a['direction']}/"
        names=[prefix] if a['action']=='rotation' else sorted(n for n in after.namelist() if n.startswith(prefix) and n.endswith('.png'))
        bases=[]
        for i,n in enumerate(names):
            cell=im.crop((i*w,0,(i+1)*w,h));expected=Image.new('RGBA',(w,h));expected.paste(png(after,n),(dx,dy))
            assert same(cell,expected),(a['id'],i,'pixel or alpha changed in packing')
            bbox=cell.getchannel('A').point(lambda x:255 if x>40 else 0).getbbox();assert bbox
            bases.append(bbox[3]-1)
        anchors.append({'id':a['id'],'baseAlpha40':bases,'span':max(bases)-min(bases),'frameSize':[w,h]})
    report={'status':'passed','assets':32,'frames':sum(a['frames'] for a in manifest['assets']),'counts':dict(collections.Counter(a['action'] for a in manifest['assets'])),'assembly':{'source':'baltazar-completo-v2-antes-da-montagem.zip','framesReplacedZeroBased':{'4':'copy of 3','6':'copy of 5'},'otherNativeFramesUnchanged':unchanged},'checks':['exact replay of native frame assembly','pixel/alpha-identical transparent padding','square cells for importer','eight directions per action','expected native frame counts','hashes'],'unityIntegration':'pendente_claude'}
    (ROOT/'verificacao-tecnica.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    (ROOT/'ancoras-medidas.json').write_text(json.dumps({'clips':anchors,'note':'Pivo constante por clipe, levando em conta padding. Nao recortar quadros individualmente.'},ensure_ascii=False,indent=2),encoding='utf-8')
    print('BALTAZAR_VERIFICADO',report['frames'],'quadros;',unchanged,'outros quadros nativos iguais')
if __name__=='__main__':main()
