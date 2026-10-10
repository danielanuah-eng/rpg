from PIL import Image
import json,numpy as np
from pathlib import Path
p=Path(__file__).resolve().parent
a=np.array(Image.open(p/'fontes/Devon/death_south-east.png').convert('RGBA'))
h=a.shape[0]; changes=[]; report=[]
for i in [7,8]:
    f=a[:,i*h:(i+1)*h]
    mask=(f[:,:,0]>220)&(f[:,:,1]<90)&(f[:,:,2]<90)&(f[:,:,3]>0)
    yy,xx=np.nonzero(mask)
    pix=[[int(x),int(y),'#'+''.join(format(int(v),'02x') for v in f[y,x]),'#00000000'] for y,x in zip(yy,xx)]
    if not len(pix): continue
    changes.append(dict(operation='patch_pixels',layer='image',frames=[i+1],pixels=pix,reason='Apagar marcador vermelho isolado acima da cabeça, preservando todos os quadros de morte.'))
    report.append(dict(frame=i+1,count=len(pix),bbox=[int(xx.min()),int(yy.min()),int(xx.max()),int(yy.max())]))
print(json.dumps(dict(changes=changes,report=report)))
