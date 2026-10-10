from pathlib import Path
from PIL import Image
import numpy as np,json
root=Path(__file__).resolve().parent
report=json.loads((root/'pivos-contagens-transicoes.json').read_text(encoding='utf-8'))
items=[]
for name,character in report['characters'].items():
    for clip,value in character['clips'].items():
        direction=clip.split('/')[1]; reference=character['rotations'][direction]
        base=Image.open(reference['file']).convert('RGBA'); sheet=Image.open(value['file']).convert('RGBA'); size=sheet.height; first=sheet.crop((0,0,size,size))
        bb=base.getbbox(); fb=first.getbbox()
        alpha_equal=np.array_equal(np.array(base.crop(bb))[:,:,3],np.array(first.crop(fb))[:,:,3])
        ba=np.array(base.crop(bb)); fa=np.array(first.crop(fb))
        visible_equal=bool(alpha_equal and np.array_equal(ba[ba[:,:,3]>0],fa[fa[:,:,3]>0]))
        anchor=None; offset=None
        if alpha_equal and reference['metadata']['pivot']:
            dx,dy=fb[0]-bb[0],fb[1]-bb[1]; px,py=reference['metadata']['pivot']
            anchor=[round((base.width*px+dx)/size,9),round((size-(base.height*(1-py)+dy))/size,9)]
            current=value['metadata']['pivot']
            if current: offset=[round((anchor[0]-current[0])*size,3),round((anchor[1]-current[1])*size,3)]
        items.append(dict(character=name,clip=clip,file=value['file'],cell=size,frames=value['frames'],first_alpha_equals_static_reference=alpha_equal,first_visible_rgba_equals_static_reference=visible_equal,pivot_for_all_frames=anchor,difference_from_imported_pivot_px=offset,method='Register unchanged first-frame alpha silhouette to static reference; preserve static configured origin. One pivot for every frame, no per-step normalization.',status='reference_registered' if anchor else 'manual_reference_needed'))
result=dict(scope='Asset reference registration, not a Unity execution test; source snapshot plus corrected delivery files.',clips=len(items),registered=sum(i['pivot_for_all_frames'] is not None for i in items),visible_first_frames_equal=sum(i['first_visible_rgba_equals_static_reference'] for i in items),manual=sum(i['pivot_for_all_frames'] is None for i in items),items=items,limitations=['Registration preserves the configured static pivot; it does not independently certify that this pivot is between the physical feet.','RGB in fully transparent pixels is ignored; visible RGBA equality is measured separately.','Do not recompute pivot from last death frame, lowest smoke particle, shield or whole-animation bounding box.'])
(root/'ancoras-para-claude.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k not in ['items','limitations']},ensure_ascii=False))
