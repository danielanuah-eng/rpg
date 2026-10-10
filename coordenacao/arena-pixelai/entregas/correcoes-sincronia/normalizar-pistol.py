from pathlib import Path
from PIL import Image,ImageSequence
import numpy as np,json
p=Path(__file__).resolve().parent/'damianPistolSE'
frames=[Image.open(p/f'frame-{i+1:02d}.png').convert('RGBA') for i in range(9)]
for fr in frames:
    a=np.array(fr)
    assert fr.size==(172,168)
    assert np.all(a[:,:2,3]==0) and np.all(a[:,170:,3]==0), 'Crop would discard visible artwork'
frames=[fr.crop((2,0,170,168)) for fr in frames]
old=Image.open(p/'pistol_south-east.anterior.png').convert('RGBA').crop((0,0,168,168))
assert np.array_equal(np.array(frames[0]),np.array(old)), 'First frame anchor differs from original'
frames[-1]=frames[0].copy()
assert all(np.isin(np.array(fr)[:,:,3],[0,255]).all() for fr in frames)
gif=p/'pistol_south-east.canonico.gif'
colors=sorted(set(tuple(rgb) for fr in frames for rgb in np.array(fr)[:,:,:3][np.array(fr)[:,:,3]>0]))
assert len(colors)<=255
palette=[0,0,0]+[int(c) for rgb in colors for c in rgb]
palette+= [0]*(768-len(palette))
indexed=[]
for fr in frames:
    a=np.array(fr); indices=np.zeros(a.shape[:2],dtype=np.uint8)
    for i,rgb in enumerate(colors,1): indices[np.all(a[:,:,:3]==rgb,axis=2)&(a[:,:,3]>0)]=i
    indexed_frame=Image.fromarray(indices,'P'); indexed_frame.putpalette(palette); indexed_frame.info['transparency']=0; indexed.append(indexed_frame)
indexed[0].save(gif,save_all=True,append_images=indexed[1:],duration=100,loop=0,disposal=2,optimize=False,transparency=0)
decoded=[fr.convert('RGBA') for fr in ImageSequence.Iterator(Image.open(gif))]
assert len(decoded)==9
for a,b in zip(frames,decoded):
    x,y=np.array(a),np.array(b); visible=x[:,:,3]>0
    assert np.array_equal(x[:,:,3],y[:,:,3]) and np.array_equal(x[:,:,:3][visible],y[:,:,:3][visible]), 'GIF conversion lost visible pixels'
strip=Image.new('RGBA',(168*9,168))
for i,fr in enumerate(frames): strip.paste(fr,(168*i,0))
strip.save(p/'pistol_south-east.png')
print(json.dumps(dict(frames=9,cell=168,first_equals_original=True,first_last_equal=True,gif_visible_rgba_exact=True,palette_colors=len(colors),removed_only_transparent_columns=True)))
