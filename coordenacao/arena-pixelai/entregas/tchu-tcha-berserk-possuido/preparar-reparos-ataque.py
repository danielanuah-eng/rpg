from pathlib import Path
from PIL import Image,ImageSequence
import numpy as np,json
root=Path(__file__).resolve().parent
sequences={
 'north':[1,2,3,4,5,4,3,2,1],
 'north-east':[1,2,3,5,6,8,8,1,1],
 'east':[1,2,3,4,5,6,7,8,1],
 'south-east':[1,2,3,4,4,7,8,1,1],
 'south':[1,2,3,4,6,7,8,1,1],
 'south-west':[1,2,3,4,5,6,7,8,1],
 'west':[1,2,3,4,6,7,8,1,1],
 'north-west':[1,2,3,4,6,7,8,1,1]}
records=[]
for direction,seq in sequences.items():
    folder=root/'refinados'/direction; raw=[Image.open(folder/f'frame-{i+1:02d}.png').convert('RGBA') for i in range(9)]
    large=max(max(fr.size) for fr in raw); expanded=[]
    for im in raw:
        frame=Image.new('RGBA',(large,large)); frame.paste(im,((large-im.width)//2,(large-im.height)//2)); expanded.append(frame)
    canonical_first=Image.open(root/'fontes'/'attack'/direction/'frame-01.png').convert('RGBA')
    s=canonical_first.width; source_box=expanded[0].getbbox(); target_box=canonical_first.getbbox()
    crop_x,crop_y=source_box[0]-target_box[0],source_box[1]-target_box[1]
    frames=[]
    for i,fr in enumerate(expanded,1):
        canvas=Image.new('RGBA',(s,s)); canvas.paste(fr,(-crop_x,-crop_y))
        if i in seq:
            a=np.array(fr); check=Image.new('RGBA',(large,large)); check.paste(canvas,(crop_x,crop_y)); b=np.array(check)
            assert np.array_equal(a[:,:,3],b[:,:,3]) and np.array_equal(a[a[:,:,3]>0],b[b[:,:,3]>0]),('selected pixels would be cropped',direction,i)
        frames.append(canvas)
    a=np.array(frames[0]); b=np.array(canonical_first)
    assert np.array_equal(a[:,:,3],b[:,:,3]) and np.array_equal(a[a[:,:,3]>0],b[b[:,:,3]>0])
    colors=sorted(set(tuple(p[:3]) for im in frames for p in np.array(im).reshape(-1,4) if p[3]>0)); assert len(colors)<256
    palette=[(0,0,0)]+colors; palette += [(0,0,0)]*(256-len(palette)); indexed=[]; lookup={c:i+1 for i,c in enumerate(colors)}
    for frame in frames:
        a=np.array(frame); pixels=np.zeros((s,s),dtype=np.uint8)
        for rgb,index in lookup.items(): pixels[np.all(a[:,:,:3]==rgb,axis=2)&(a[:,:,3]>0)]=index
        im=Image.fromarray(pixels,'P'); im.putpalette([v for c in palette for v in c]); im.info['transparency']=0; indexed.append(im)
    gif=folder/'fonte-quadrada.gif'; indexed[0].save(gif,save_all=True,append_images=indexed[1:],duration=100,loop=0,transparency=0,disposal=2,optimize=False)
    decoded=[f.convert('RGBA').copy() for f in ImageSequence.Iterator(Image.open(gif))]; assert len(decoded)==9
    for original,copy in zip(frames,decoded):
        a=np.array(original); b=np.array(copy); assert np.array_equal(a[:,:,3],b[:,:,3]) and np.array_equal(a[a[:,:,3]>0],b[b[:,:,3]>0])
    first=np.array(frames[0]); initial_colors=set(tuple(p) for p in first.reshape(-1,4) if p[3]>0)
    # Only generated luminous VFX colours absent from the standing reference.
    maps={}
    for color in set(tuple(p) for fr in frames for p in np.array(fr).reshape(-1,4) if p[3]>0):
        r,g,b,a=map(int,color)
        if color not in initial_colors and ((min(r,g,b)>170) or (r>200 and g<70 and b<70)):
            maps['#'+''.join(f'{v:02x}' for v in [r,g,b,a])]='#362736ff' if min(r,g,b)>170 else '#201020ff'
    changes=[]
    for dest,src in enumerate(seq,1):
        if dest!=src: changes.append(dict(operation='copy_region',layer='image',frames=[dest],source_layer='image',source_frame=src,region=[0,0,s-1,s-1],at=[0,0],reason='Preserve wind-up/slash/recovery while omitting generated full-body red flash; final returns to standing reference.'))
    if maps: changes.append(dict(operation='map_colors',layer='image',frames=list(range(2,9)),region=[0,0,s-1,s-1],colors=maps,reason='Generated white/red VFX colours absent from the reference become black-purple shadow trail; original visor and shield palette preserved.'))
    ref=Image.open(root/'fontes'/(direction+'.png')).convert('RGBA'); fb=frames[0].getbbox(); rb=ref.getbbox(); dx,dy=fb[0]-rb[0],fb[1]-rb[1]
    # Keep helmet/visor identity and facing. Reviewed helmet zone in the 128px reference.
    region=[dx+44,dy,dx+88,dy+56]
    changes.append(dict(operation='copy_region',layer='image',frames=list(range(2,9)),source_layer='image',source_frame=1,region=region,at=region[:2],reason='Preserve the original helmet and facing; generated strokes had added a frontal visor to back-facing helmets. Keep arms and sword motion outside this zone.'))
    records.append(dict(direction=direction,cell=s,source=str(gif),sequence=seq,changes=changes,exact_rgba_gif_pack=True,canvas_registration_crop=[crop_x,crop_y,s,s],selected_frames_no_pixel_loss=True,source_group='1a774d55-7c52-48fb-abbd-d272b055a6da',target_group='b32eec9f-07de-4057-9171-be3ff653c476'))
(root/'reparos-ataque.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print('Exact source GIF packing verified for',len(records),'directions; native edit decisions prepared')
