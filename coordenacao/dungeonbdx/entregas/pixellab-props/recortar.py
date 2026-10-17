"""Recorta os props do PixelLab (fonte/NN-<id>.png, quadro 80) para Iso/Props/<id>.png.
Uso: python -I recortar.py <pasta-de-saida>. Remove partículas soltas (<4% dos pixels) e corta justo."""
import sys,json,hashlib
from pathlib import Path
from PIL import Image
rec=Path(__file__).resolve().parent;out=Path(sys.argv[1])
log={'object_id':'d33c8639-c6b7-4523-aa1c-97c4ff25cc58','canvas':80,'props':{}}
for f in sorted((rec/'fonte').glob('*.png')):
    n=f.stem.split('-',1)[1];im=Image.open(f).convert('RGBA');W,H=im.size
    a=im.getchannel('A').point(lambda v:255 if v>40 else 0);px=a.load();seen=set();comps=[]
    for y in range(H):
        for x in range(W):
            if px[x,y] and (x,y) not in seen:
                st=[(x,y)];seen.add((x,y));pts=[]
                while st:
                    c=st.pop();pts.append(c)
                    for q in((c[0]+1,c[1]),(c[0]-1,c[1]),(c[0],c[1]+1),(c[0],c[1]-1)):
                        if 0<=q[0]<W and 0<=q[1]<H and px[q] and q not in seen:seen.add(q);st.append(q)
                comps.append(pts)
    total=sum(map(len,comps));keep=set(p for c in comps if len(c)>=.04*total for p in c)
    clean=Image.new('RGBA',im.size);src=im.load();dst=clean.load()
    for y in range(H):
        for x in range(W):
            if (x,y) in keep or (src[x,y][3] and any((x+dx,y+dy) in keep for dx in(-1,0,1) for dy in(-1,0,1))):dst[x,y]=src[x,y]
    clean=clean.crop(clean.getbbox());p=out/f'{n}.png';clean.save(p)
    log['props'][n]={'fonte':f.name,'size':clean.size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
    print(n,clean.size,len(comps))
(rec/'props.json').write_text(json.dumps(log,indent=1),encoding='utf-8')
