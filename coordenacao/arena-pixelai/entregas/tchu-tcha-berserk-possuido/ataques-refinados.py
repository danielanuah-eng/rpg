from pathlib import Path
from PIL import Image,ImageDraw
import zipfile,re,numpy as np,json
root=Path(__file__).resolve().parent; z=zipfile.ZipFile(root/'pixellab-refinado.zip')
directions=['north','north-east','east','south-east','south','south-west','west','north-west']
folder=root/'auditoria-ataques'; folder.mkdir(exist_ok=True); allframes={}; checks=[]
for direction in directions:
    entries=sorted([n for n in z.namelist() if n.startswith('Berserk_Possuido_v2_Sombras/') and '/animations/attack-refinado-sombras/'+direction+'/' in n and n.endswith('.png')],key=lambda n:int(re.search(r'(\d+)\.png$',n).group(1)))
    assert len(entries)==9,(direction,len(entries))
    dest=root/'refinados'/direction; dest.mkdir(parents=True,exist_ok=True); frames=[]
    for i,entry in enumerate(entries):
        file=dest/f'frame-{i+1:02d}.png'; file.write_bytes(z.read(entry)); frames.append(Image.open(file).convert('RGBA'))
    source_sizes=[list(fr.size) for fr in frames]
    s=max(max(fr.size) for fr in frames)
    normalized=[]
    for fr in frames:
        canvas=Image.new('RGBA',(s,s)); canvas.paste(fr,((s-fr.width)//2,(s-fr.height)//2)); normalized.append(canvas)
    frames=normalized
    a=np.array(frames[0]); b=np.array(frames[-1]); visible_equal=bool(np.array_equal(a[:,:,3],b[:,:,3]) and np.array_equal(a[a[:,:,3]>0],b[b[:,:,3]>0]))
    checks.append(dict(direction=direction,frames=9,cell=s,source_sizes=source_sizes,first_last_visible_equal=visible_equal,unique_visible_colors=len(set(tuple(p) for fr in frames for p in np.array(fr).reshape(-1,4) if p[3]>0)),alpha_partial=sum(int(((np.array(fr)[:,:,3]>0)&(np.array(fr)[:,:,3]<255)).sum()) for fr in frames)))
    allframes[direction]=frames
for page in range(2):
    sheet=Image.new('RGBA',(9*256,4*256),(35,37,44,255)); d=ImageDraw.Draw(sheet)
    for row,direction in enumerate(directions[page*4:page*4+4]):
        for i,fr in enumerate(allframes[direction]):
            assert max(fr.size)<=248
            sheet.alpha_composite(fr,(i*256+(256-fr.width)//2,row*256+(256-fr.height)//2)); d.text((i*256+5,row*256+5),direction+' attack '+str(i+1),fill='white')
    sheet.save(folder/f'ataques-{page+1}.png')
(folder/'metricas.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
print(json.dumps(checks))
