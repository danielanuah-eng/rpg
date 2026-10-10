from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np,json,re,statistics
p=Path(__file__).resolve().parent
bridge=p.parents[1]
index=json.loads((p/'indice.json').read_text(encoding='utf-8'))
fixroot=bridge/'entregas/correcoes-sincronia'
fixmap={}
registered_file=p/'ancoras-para-claude.json'
registered={(e['character'],e['clip']):e['pivot_for_all_frames'] for e in json.loads(registered_file.read_text(encoding='utf-8'))['items']} if registered_file.exists() else {}
for e in json.loads((fixroot/'fontes.json').read_text(encoding='utf-8')):
    name=e['source_file'].split('/')[-2]
    fixmap[(name,e['clip']+'/'+e['dir'])]=fixroot/e['key']/(e['clip']+'_'+e['dir']+'.png')
out={'scope':'snapshot local + corrected delivery assets; static metadata audit and asset playback, not a Unity runtime test','characters':{},'summary':{}}
def geometry(im):
    a=np.array(im); y,x=np.nonzero(a[:,:,3]>40)
    if not len(x): return None
    bottom=int(y.max()); low=x[y>=bottom-12]
    foot_x=float((np.percentile(low,5)+np.percentile(low,95))/2)
    return dict(bbox=list(im.getbbox()),ground=[round(foot_x,2),bottom],opaque=int((a[:,:,3]>40).sum()),alpha_centroid=[round(float(x.mean()),2),round(float(y.mean()),2)])
def metadata(path):
    text=path.read_text(encoding='utf-8') if path.exists() else ''
    mode=re.search(r'spriteMode: (\d+)',text)
    ppu=re.search(r'spritePixelsToUnits: ([\d.]+)',text)
    pivot=re.search(r'spritePivot: \{x: ([\d.e-]+), y: ([\d.e-]+)\}',text)
    return dict(available=bool(text),mode=int(mode.group(1)) if mode else None,ppu=float(ppu.group(1)) if ppu else None,pivot=[float(pivot.group(1)),float(pivot.group(2))] if pivot else None)
total_counts_ok=True; drift=[]
for name,c in index['characters'].items():
    snapshots=p/'fontes'/name
    refs={}
    for di,filename in c['rotations'].items():
        file=snapshots/filename; im=Image.open(file).convert('RGBA'); g=geometry(im); m=metadata(file.with_suffix('.png.meta'))
        refs[di]=dict(file=str(file),cell=im.height,geometry=g,metadata=m,proposed_pivot=[g['ground'][0]/im.width,(im.height-1-g['ground'][1])/im.height],ground_is_silhouette_estimate=True)
    clips={}; actions={}
    for key,v in c['clips'].items():
        file=fixmap.get((name,key),Path(v['file'])); strip=Image.open(file).convert('RGBA'); size=strip.height; n=strip.width//size
        clip,di=key.split('/'); frames=[strip.crop((i*size,0,(i+1)*size,size)) for i in range(n)]
        geos=[geometry(fr) for fr in frames]
        m=metadata(Path(v['file']+'.meta')); pivot=m['pivot']
        base=Image.open(refs[di]['file']).convert('RGBA'); same=np.array_equal(np.array(frames[0].crop(frames[0].getbbox())),np.array(base.crop(base.getbbox())))
        ground=geos[0]['ground']
        proposed=registered.get((name,key)) or [ground[0]/size,(size-1-ground[1])/size]
        current_world=[round(ground[0]-pivot[0]*size,3),round((size-1-ground[1])-pivot[1]*size,3)] if pivot else None
        if current_world: drift.append((name,key,current_world))
        clips[key]=dict(file=str(file),frames=n,cell=size,reference_equals_base_except_padding=same,metadata=m,geometry=geos,proposed_pivot=proposed,current_first_ground_offset_px=current_world,anchor_rule='Use one ground anchor for whole clip; do not pin centroid or normalize each footfall. Manual foot confirmation required for flying units, smoke, leap and effects.')
        actions.setdefault(clip,{})[di]=n
    consistency={k:dict(counts=v,equal=len(set(v.values()))==1,available_directions=len(v),cardinal_directions_missing=True) for k,v in actions.items()}
    total_counts_ok=total_counts_ok and all(v['equal'] for v in consistency.values())
    out['characters'][name]=dict(rotations=refs,clips=clips,count_consistency=consistency,all_eight_views_animated=False)
    for di in ['north-east','north-west','south-east','south-west']:
        idle=clips.get('idle/'+di); walk=clips.get('walk/'+di)
        if not idle or not walk: continue
        sequence=[(idle,0),(idle,min(2,idle['frames']-1))]+[(walk,i) for i in range(walk['frames'])]+[(idle,0),(idle,min(2,idle['frames']-1))]
        renders=[]
        for e,fi in sequence:
            src=Image.open(e['file']).convert('RGBA'); s=e['cell']; fr=src.crop((fi*s,0,(fi+1)*s,s))
            if not e['metadata']['pivot']: continue
            panel=Image.new('RGBA',(600,300),(36,38,44,255)); d=ImageDraw.Draw(panel)
            for col,kind in enumerate(['imported pivot','registered reference pivot']):
                anchor=e['metadata']['pivot'] if col==0 else e['proposed_pivot']
                x=150+col*300; y=260
                d.line((col*300,y,col*300+299,y),fill=(100,160,110,255))
                panel.alpha_composite(fr,(round(x-anchor[0]*s),round(y-(1-anchor[1])*s)))
                d.line((x-4,y,x+4,y),fill=(255,80,80,255)); d.line((x,y-4,x,y+4),fill=(255,80,80,255))
                d.text((col*300+8,6),kind,fill=(245,245,245,255))
            d.text((8,24),name+' '+di+' '+Path(e['file']).stem+' frame '+str(fi+1),fill=(245,245,245,255)); renders.append(panel.convert('RGB'))
        dest=p/'transicoes'/name; dest.mkdir(parents=True,exist_ok=True)
        renders[0].save(dest/(di+'.gif'),save_all=True,append_images=renders[1:],duration=180,loop=0)
        renders[0].save(dest/(di+'.png'))
out['summary']=dict(characters=len(out['characters']),canonical_clips=index['totals']['clips'],canonical_frames=index['totals']['frames'],same_count_all_available_directions=total_counts_ok,static_views=96,transition_previews=48,runtime_fps_current=10,cardinal_animation_missing='Every canonical clip has only 4 diagonal directions. Cardinal views rely on runtime fallback.',ground_offsets_over_3px=[dict(character=n,clip=k,offset=v) for n,k,v in drift if abs(v[1])>3],limitations=['Silhouette ground is an estimate, not a manually labelled skeletal foot point.','Centroid and alpha bounds include weapon, clothing and effects; they do not prove invariant body volume.','No Unity runtime or actual frame exposure timing executed.'])
(p/'pivos-contagens-transicoes.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(out['summary'],ensure_ascii=False))
