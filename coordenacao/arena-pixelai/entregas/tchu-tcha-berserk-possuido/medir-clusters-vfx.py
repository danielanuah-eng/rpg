from pathlib import Path
from PIL import Image
import numpy as np,json
root=Path(__file__).resolve().parent
records=[]
for entry in json.loads((root/'reparos-ataques.json').read_text(encoding='utf-8')):
    direction=entry['direction']; ops=[]; components=[]
    for frame in range(2,9):
        a=np.array(Image.open(root/'fontes-finais'/'attack'/direction/f'frame-{frame:02d}.png').convert('RGBA'))
        colors=np.unique(a[a[:,:,3]>0],axis=0)
        for rgba in colors:
            r,g,b,alpha=map(int,rgba); white=min(r,g,b)>170; red=r>200 and g<70 and b<70
            if not (white or red): continue
            mask=np.all(a==rgba,axis=2); seen=np.zeros(mask.shape,dtype=bool)
            for y,x in zip(*np.nonzero(mask)):
                if seen[y,x]: continue
                seen[y,x]=True; stack=[(int(x),int(y))]; points=[]
                while stack:
                    cx,cy=stack.pop(); points.append((cx,cy))
                    for nx,ny in [(cx-1,cy),(cx+1,cy),(cx,cy-1),(cx,cy+1)]:
                        if 0<=nx<a.shape[1] and 0<=ny<a.shape[0] and mask[ny,nx] and not seen[ny,nx]: seen[ny,nx]=True; stack.append((nx,ny))
                threshold=25 if white else 100
                if len(points)<threshold: continue
                expected='#'+''.join(f'{v:02x}' for v in [r,g,b,alpha]); target='#362736ff' if white else '#201020ff'
                ops.append(dict(operation='flood_fill',layer='image',frames=[frame],seed=list(points[0]),expected=expected,color=target,reason='Measured large connected VFX cluster becomes shadow. Tiny armor glints and red eye/visor clusters stay below this size threshold.'))
                components.append(dict(frame=frame,color=expected,pixels=len(points),seed=list(points[0])))
    records.append(dict(direction=direction,image_id=entry['image_id'],cell=entry['cell'],changes=ops,components=components,target_group=entry['target_group']))
(root/'clusters-vfx.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps([(e['direction'],len(e['changes']),sum(c['pixels'] for c in e['components'])) for e in records]))
