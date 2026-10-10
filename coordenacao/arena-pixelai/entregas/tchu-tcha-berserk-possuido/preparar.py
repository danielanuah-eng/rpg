from pathlib import Path
from PIL import Image,ImageDraw
import json,zipfile,io,re,hashlib,numpy as np,sys
root=Path(__file__).resolve().parent
directions=['north','north-east','east','south-east','south','south-west','west','north-west']
actions=['idle','walk','attack','hit','death']
final='--final' in sys.argv
z=zipfile.ZipFile(root/('pixellab-final.zip' if final else 'pixellab-fonte.zip')); prefix='Berserk_Possuido_v2_Sombras/'
names=[n for n in z.namelist() if n.startswith(prefix) and n.endswith('.png')]
source=root/('fontes-finais' if final else 'fontes'); sheets=root/('auditoria-final' if final else 'auditoria'); assets=root/'sprites'
for d in [source,sheets,assets]: d.mkdir(exist_ok=True)
references={}; clips={}; checks=[]
for direction in directions:
    match=[n for n in names if n.endswith('/rotations/'+direction+'.png')]
    assert len(match)==1,(direction,match)
    target=source/(direction+'.png'); target.write_bytes(z.read(match[0])); im=Image.open(target).convert('RGBA'); assert im.size==(128,128)
    im.save(assets/(direction+'.png')); references[direction]=im
    for action in actions:
        entries=sorted([n for n in names if '/animations/'+action+'/'+direction+'/' in n],key=lambda n:int(re.search(r'(\d+)\.png$',n).group(1)))
        assert len(entries)==(7 if action=='hit' else 9),(action,direction,len(entries))
        folder=source/action/direction; folder.mkdir(parents=True,exist_ok=True); frames=[]
        for i,n in enumerate(entries):
            file=folder/f'frame-{i+1:02d}.png'; file.write_bytes(z.read(n)); frames.append(Image.open(file).convert('RGBA'))
        s=max(max(fr.size) for fr in frames); assert all(fr.size==(s,s) for fr in frames),(action,direction,[fr.size for fr in frames])
        ba=np.array(im.crop(im.getbbox())); fb=frames[0].getbbox(); fa=np.array(frames[0].crop(fb))
        same=bool(np.array_equal(ba[:,:,3],fa[:,:,3]) and np.array_equal(ba[ba[:,:,3]>0],fa[fa[:,:,3]>0]))
        assert same,('first frame mismatch',action,direction)
        # Initial pivot proposal preserves the static reference ground; labelled boot anchor can override this later.
        base_pivot=[.5,(128-im.getbbox()[3])/128]
        dx=fb[0]-im.getbbox()[0]; dy=fb[1]-im.getbbox()[1]
        pivot=[(128*base_pivot[0]+dx)/s,(s-(128*(1-base_pivot[1])+dy))/s]
        strip=Image.new('RGBA',(s*len(frames),s))
        for i,fr in enumerate(frames): strip.paste(fr,(i*s,0))
        file=assets/(action+'_'+direction+'.png'); strip.save(file)
        first_array=np.array(frames[0]); last_array=np.array(frames[-1]); equal=bool(np.array_equal(first_array[:,:,3],last_array[:,:,3]) and np.array_equal(first_array[first_array[:,:,3]>0],last_array[last_array[:,:,3]>0]))
        if final and action!='death': assert equal,('unclosed return',action,direction)
        checks.append(dict(action=action,direction=direction,frames=len(frames),cell=s,first_visible_rgba_equals_reference=True,pivot=pivot,reference_pivot=base_pivot,first_last_visible_equal=equal,alpha_zero=int((np.array(strip)[:,:,3]==0).sum()),alpha_partial=int(((np.array(strip)[:,:,3]>0)&(np.array(strip)[:,:,3]<255)).sum()),sha256=hashlib.sha256(file.read_bytes()).hexdigest(),file=str(file.relative_to(root))))
        clips[(action,direction)]=frames
    panel=Image.new('RGBA',(9*192,6*192),(35,37,44,255)); draw=ImageDraw.Draw(panel)
    panel.alpha_composite(im,(32,32)); draw.text((4,4),'BASE '+direction,fill='white')
    for row,action in enumerate(actions,1):
        for i,fr in enumerate(clips[(action,direction)]):
            assert max(fr.size)<=190,('preview cell too small',fr.size)
            panel.alpha_composite(fr,(i*192+(192-fr.width)//2,row*192+(192-fr.height)//2)); draw.text((i*192+4,row*192+3),action+' '+str(i+1),fill='white')
    panel.save(sheets/(direction+'.png'))
rotations=Image.new('RGBA',(4*256,2*256),(35,37,44,255)); draw=ImageDraw.Draw(rotations)
for i,direction in enumerate(directions):
    im=references[direction].resize((256,256),Image.Resampling.NEAREST); x=(i%4)*256; y=(i//4)*256
    rotations.alpha_composite(im,(x,y)); draw.text((x+4,y+3),direction,fill='white')
rotations.save(root/'rotacoes-preview.png')
(sheets/'metricas.json').write_text(json.dumps(dict(character_id='215bb320-de53-44a1-bec7-f2e23f4e7265',reference_views=8,clips=40,frames=sum(x['frames'] for x in checks),first_visible_reference_equal=True,items=checks),ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(views=8,clips=len(checks),frames=sum(x['frames'] for x in checks),cells=sorted(set(x['cell'] for x in checks))),ensure_ascii=False))
