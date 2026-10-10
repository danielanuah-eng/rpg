from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np,json,hashlib

root=Path(__file__).resolve().parent
def hash_file(file): return hashlib.sha256(file.read_bytes()).hexdigest()
def stats(file):
    im=Image.open(file).convert('RGBA'); a=np.array(im)[:,:,3]
    return dict(file=str(file.relative_to(root)),size=list(im.size),sha256=hash_file(file),alpha_zero=int((a==0).sum()),alpha_partial=int(((a>0)&(a<255)).sum()),alpha_opaque=int((a==255).sum()))
def strip(frames,file):
    w,h=frames[0].size; assert all(im.size==(w,h) for im in frames)
    out=Image.new('RGBA',(w*len(frames),h))
    for i,im in enumerate(frames): out.paste(im,(i*w,0))
    out.save(file); return out
def preview(frames,file,ms):
    opaque=[]
    for im in frames:
        back=Image.new('RGBA',im.size,(35,37,44,255)); back.alpha_composite(im); opaque.append(back.convert('RGB'))
    opaque[0].save(file,save_all=True,append_images=opaque[1:],duration=ms,loop=0)
def grid(items,file,cols=5,cell=(148,124)):
    sheet=Image.new('RGBA',(cols*cell[0],((len(items)+cols-1)//cols)*cell[1]),(35,37,44,255)); d=ImageDraw.Draw(sheet)
    for i,(label,im) in enumerate(items):
        x=(i%cols)*cell[0]; y=(i//cols)*cell[1]
        sheet.alpha_composite(im,(x+(cell[0]-im.width)//2,y+8)); d.text((x+5,y+cell[1]-18),label[:23],fill=(240,240,240,255))
    sheet.save(file)

# Shop scene: a native PixelLab edit restricted to mouth and left arm/hand.
# Preserve the original scene pixel-for-pixel outside these two moving regions.
shop=root/'loja-v2'; original=Image.open(root.parents[2]/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Cenarios/loja.png').convert('RGBA')
atlas=Image.open(shop/'vendedor-estavel.png').convert('RGBA'); assert atlas.size==(480,384) and original.size==(384,216)
source_crop=Image.open(shop/'vendedor-referencia.png').convert('RGBA'); assert np.array_equal(np.array(source_crop),np.array(original.crop((112,56,272,184))))
rects=[(64,41,94,71),(8,46,60,95)]
scene_folder=shop/'cenario'; stable_folder=shop/'vendedor-estabilizado'
scene_folder.mkdir(exist_ok=True); stable_folder.mkdir(exist_ok=True)
scenes=[]; vendors=[]; moving=[]
mask=np.zeros((216,384),dtype=bool)
for x1,y1,x2,y2 in rects: mask[56+y1:56+y2,112+x1:112+x2]=True
for i in range(9):
    frame=atlas.crop(((i%3)*160,(i//3)*128,(i%3+1)*160,(i//3+1)*128)); scene=original.copy(); vendor=source_crop.copy()
    if i not in (0,8):
        for rect in rects:
            patch=frame.crop(rect); scene.paste(patch,(112+rect[0],56+rect[1])); vendor.paste(patch,(rect[0],rect[1]))
    scene.save(scene_folder/f'frame-{i+1:02d}.png'); vendor.save(stable_folder/f'frame-{i+1:02d}.png')
    assert np.array_equal(np.array(scene)[~mask],np.array(original)[~mask])
    delta=np.any(np.array(scene)!=np.array(original),axis=2); moving.append(int(delta.sum())); scenes.append(scene); vendors.append(vendor)
assert np.array_equal(np.array(scenes[0]),np.array(scenes[-1])) and max(moving)>0
strip(scenes,shop/'loja.png'); strip(vendors,shop/'vendedor.png'); preview(scenes,shop/'loja-preview.gif',200)
items=sorted((shop/'itens').glob('*.png')); assert len(items)==10
grid([(f.stem,Image.open(f).convert('RGBA')) for f in items],shop/'itens-preview.png',5,(148,100))
shop_manifest=dict(status='pronto_para_integrar',integration='Claude confirmou ícones e fonte inicial de vendedor; reimportar cenário/vendedor estabilizados deste manifesto.',scene=stats(shop/'loja.png'),frames=9,frame_size=[384,216],fps=5,loop=True,first_last_identical=True,shop_open_only=True,vendor_overlay=dict(**stats(shop/'vendedor.png'),frame_size=[160,128],origin_in_scene=[112,56]),native_source_job='fe3c9585-0fa0-4ca0-b1c4-1c5048bc705f',native_stabilized_image='a8155ff9-20aa-49f8-b521-b06ad7db4046',moving_regions_in_vendor=rects,background_exact_outside_regions=True,changed_pixels_by_frame=moving,items=[stats(f) for f in items],preview_is_game_asset=False)
(shop/'loja.visual.json').write_text(json.dumps(shop_manifest,ensure_ascii=False,indent=2),encoding='utf-8')

fx=root/'eventos-cenarios'; effect_folder=fx/'efeitos'; effect_folder.mkdir(exist_ok=True)
jobs={'baforada':'0efb8559-e8ed-43bc-a86c-ead0e39a5b14','raio-arcano':'4b253912-9df2-40cc-a500-3a1b11a47891','cura':'312ddcee-5b87-494b-b29c-67e154054a77','impacto-area':'0fbc45ae-956f-4c68-92d4-f22be1fa83e3','invocacao':'39b2d4ea-c4ca-441d-a0af-87372f2f4d6c','moeda':'235a0b7c-18ed-4e0b-9151-f8fb47bab479'}
animations=[]; contact=[]
for name,job in jobs.items():
    files=sorted((fx/name).glob('frame-*.png')); frames=[Image.open(f).convert('RGBA') for f in files]
    expected=9 if name=='moeda' else 5; assert len(frames)==expected
    w,h=frames[0].size; s=max(w,h); packed=[]
    for im in frames:
        assert im.size==(w,h)
        canvas=Image.new('RGBA',(s,s)); canvas.paste(im,((s-w)//2,(s-h)//2)); packed.append(canvas)
    final=effect_folder/(name+'.png'); strip(packed,final); preview(packed,fx/name/'preview.gif',160 if name=='moeda' else 100)
    animations.append(dict(id=name,job_id=job,**stats(final),frames=len(frames),frame_size=[s,s],original_frame_size=[w,h],transparent_padding_only=True,pivot=[.5,.5],suggested_fps=6 if name=='moeda' else 10,loop=name in ['moeda','baforada','raio-arcano'],native_frame_files=[str(f.relative_to(root)) for f in files]))
    for i,im in enumerate(frames): contact.append((f'{name} {i+1}/{len(frames)}',im))
grid(contact,fx/'animacoes-preview.png',9,(148,124))
static_files=sorted((fx/'projeteis').glob('*.png'))
grid([(f.stem,Image.open(f).convert('RGBA')) for f in static_files],fx/'projeteis-preview.png',5,(148,124))
terrain=[]
for file in sorted(f for f in (fx/'terrenos').glob('*.png') if '.repeticao' not in f.name):
    im=Image.open(file).convert('RGBA'); arr=np.array(im)[:,:,:3].astype(float)
    internal=np.concatenate([np.abs(arr[:,1:]-arr[:,:-1]).reshape(-1,3),np.abs(arr[1:]-arr[:-1]).reshape(-1,3)])
    seams=np.concatenate([np.abs(arr[:,0]-arr[:,-1]),np.abs(arr[0]-arr[-1])])
    tile=Image.new('RGBA',(128,128))
    for x,y in [(0,0),(64,0),(0,64),(64,64)]: tile.paste(im,(x,y))
    tile.resize((512,512),Image.Resampling.NEAREST).save(fx/'terrenos'/(file.stem+'.repeticao.png'))
    terrain.append(dict(**stats(file),wrap_edge_mean_rgb_delta=round(float(seams.mean()),3),internal_mean_rgb_delta=round(float(internal.mean()),3),seamless_certified=False))
(fx/'assets.visual.json').write_text(json.dumps(dict(status='pronto_para_integrar',animations=animations,projectiles=[stats(f) for f in static_files],terrain=terrain,preview_is_game_asset=False),ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(shop_frames=len(scenes),shop_changed_pixels=moving,item_icons=len(items),fx_animations=len(animations),projectiles=len(static_files),terrain=len(terrain)),ensure_ascii=False))
