"""Extrai sprites nativos e cria manifestos; não altera os desenhos."""
from pathlib import Path
from PIL import Image, ImageDraw
import io, json, zipfile, hashlib
ROOT=Path(__file__).resolve().parent
def alpha(im):
    h=im.getchannel('A').histogram()
    return {'transparent':h[0],'opaque':h[255],'partial':sum(h[1:255])}
def main():
    src=ROOT/'baltazar-completo.zip'
    if not src.exists(): src=ROOT/'baltazar-fonte.zip'
    assets=[]
    selection=json.loads((ROOT/'selecao.json').read_text(encoding='utf-8')) if (ROOT/'selecao.json').exists() else {}
    previous=json.loads((ROOT/'manifesto.json').read_text(encoding='utf-8')) if (ROOT/'manifesto.json').exists() else {}
    reviews={a['id']:a for a in previous.get('assets',[])}
    with zipfile.ZipFile(src) as z:
        groups={}
        for n in z.namelist():
            if not n.endswith('.png'): continue
            parts=Path(n).parts
            if parts[1]=='rotations': key=('rotation',Path(n).stem)
            elif parts[1]=='animations':
                if any(e['action']==parts[2] and e['direction']==parts[3] for e in selection.get('excludedSources',[])):continue
                key=(selection.get('actionAliases',{}).get(parts[2],parts[2]),parts[3])
            else: continue
            groups.setdefault(key,[]).append(n)
        for (action,direction),names in sorted(groups.items()):
            frames=[Image.open(io.BytesIO(z.read(n))).convert('RGBA') for n in sorted(names)]
            nw,nh=frames[0].size; assert all(im.size==(nw,nh) for im in frames)
            # Empacotamento para o importador de células quadradas: nenhum pixel é retocado.
            w=h=max(nw,nh);offset=((w-nw)//2,(h-nh)//2)
            sheet=Image.new('RGBA',(w*len(frames),h))
            for i,im in enumerate(frames): sheet.paste(im,(i*w+offset[0],offset[1]))
            folder=ROOT/'sprites'/action; folder.mkdir(parents=True,exist_ok=True)
            dest=folder/f'{direction}.png'; sheet.save(dest)
            a=alpha(sheet); assert a['transparent']>0 and a['opaque']>0
            ident=f'baltazar-{action}-{direction}';sha=hashlib.sha256(dest.read_bytes()).hexdigest()
            old=reviews.get(ident,{})
            review=old.get('visualReview','pending') if old.get('sha256')==sha else 'pending'
            assets.append({'id':ident,'action':action,'sourceAction':Path(names[0]).parts[2] if action!='rotation' else 'rotation','direction':direction,'file':dest.relative_to(ROOT).as_posix(),'frameSize':[w,h],'nativeFrameSize':[nw,nh],'paddingOrigin':list(offset),'frames':len(frames),'layout':'horizontal','alpha':a,'sha256':sha,'pivot':'pendente_claude; conservar ancora do sprite nativo com padding informado','integration':'pendente_claude','visualReview':review,'importAllowed':review=='approved_codex'})
    report={'id':'baltazar','characterId':'e6148923-ecab-4e29-8da1-6d52cd46c535','view':'high top-down','assets':assets,'integration':'pendente_claude','source':src.name,'selection':'selecao.json','technicalVerification':'verificacao-tecnica.json','anchors':'ancoras-medidas.json','review':'revisao-codex.md','excludedSources':selection.get('excludedSources',[])}
    (ROOT/'manifesto.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    directions=('south','south-east','east','north-east','north','north-west','west','south-west')
    gallery=Image.new('RGB',(1024,560),'#202632'); d=ImageDraw.Draw(gallery)
    for i,direction in enumerate(directions):
        im=Image.open(ROOT/'sprites/rotation'/f'{direction}.png').convert('RGBA').resize((256,256),Image.Resampling.NEAREST)
        x,y=(i%4)*256,(i//4)*280; gallery.paste(im,(x,y+20),im);d.text((x+8,y+5),direction,fill='white')
    gallery.save(ROOT/'baltazar-oito-vistas.png')
    print(json.dumps({'assets':len(assets),'frames':sum(a['frames'] for a in assets),'alpha':'passed'}))
if __name__=='__main__':main()
