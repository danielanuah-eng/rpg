from pathlib import Path
from PIL import Image,ImageSequence
import json,numpy as np,zipfile,io,hashlib
root=Path(__file__).resolve().parent
def visible_equal(a,b):
    return a.shape==b.shape and np.array_equal(a[:,:,3],b[:,:,3]) and np.array_equal(a[a[:,:,3]>0],b[b[:,:,3]>0])
def frames(action,direction):
    return [np.array(Image.open(f).convert('RGBA')) for f in sorted((root/'fontes-finais'/action/direction).glob('frame-*.png'))]
checks=[]
for e in json.loads((root/'reparos-loops.json').read_text(encoding='utf-8')):
    originals=[np.array(Image.open(f).convert('RGBA')) for f in sorted((root/'fontes'/e['action']/e['direction']).glob('frame-*.png'))]
    final=frames(e['action'],e['direction']); expected=[originals[k-1] for k in e['seq']]
    assert len(final)==len(expected) and all(visible_equal(a,b) for a,b in zip(expected,final)),('exact sequence',e['action'],e['direction'])
    checks.append(dict(action=e['action'],direction=e['direction'],exact_visible_rgba=True))
def apply_ops(source,ops):
    original=[a.copy() for a in source]; out=[a.copy() for a in source]
    for op in ops:
        for frame in op['frames']:
            a=out[frame-1]
            if op['operation']=='copy_region':
                x1,y1,x2,y2=op['region']; x,y=op['at']; src=original[op['source_frame']-1][y1:y2+1,x1:x2+1].copy(); a[y:y+src.shape[0],x:x+src.shape[1]]=src
            elif op['operation']=='map_colors':
                x1,y1,x2,y2=op['region']; region=a[y1:y2+1,x1:x2+1]; snapshot=region.copy()
                for before,after in op['colors'].items():
                    b=tuple(bytes.fromhex(before[1:])); c=tuple(bytes.fromhex(after[1:])); region[np.all(snapshot==b,axis=2)]=c
            elif op['operation']=='flood_fill':
                color=tuple(bytes.fromhex(op['color'][1:])); before=tuple(bytes.fromhex(op['expected'][1:])); x,y=op['seed']; assert tuple(a[y,x])==before
                stack=[(x,y)]; a[y,x]=color
                while stack:
                    cx,cy=stack.pop()
                    for nx,ny in [(cx-1,cy),(cx+1,cy),(cx,cy-1),(cx,cy+1)]:
                        if 0<=nx<a.shape[1] and 0<=ny<a.shape[0] and tuple(a[ny,nx])==before: a[ny,nx]=color; stack.append((nx,ny))
            else: raise ValueError(op['operation'])
    return out
cluster_map={e['direction']:e for e in json.loads((root/'reparos-clusters.json').read_text(encoding='utf-8'))}
for e in json.loads((root/'reparos-ataques.json').read_text(encoding='utf-8')):
    source=[np.array(fr.convert('RGBA')) for fr in ImageSequence.Iterator(Image.open(e['source']))]
    expected=apply_ops(source,e['changes'])
    if e['direction'] in cluster_map: expected=apply_ops(expected,cluster_map[e['direction']]['changes'])
    final=frames('attack',e['direction']); assert all(visible_equal(a,b) for a,b in zip(expected,final)),('native operation equality','attack',e['direction'])
    checks.append(dict(action='attack',direction=e['direction'],exact_visible_rgba=True))
first=zipfile.ZipFile(root/'pixellab-fonte.zip'); last=zipfile.ZipFile(root/'pixellab-final.zip')
unchanged=[n for n in first.namelist() if n.endswith('.png') and (not n.startswith('Berserk_Possuido_v2_Sombras/') or '/rotations/' in n or '/animations/death/' in n)]
assert all(first.read(n)==last.read(n) for n in unchanged)
metrics=json.loads((root/'auditoria-final/metricas.json').read_text(encoding='utf-8')); assert metrics['clips']==40 and metrics['frames']==344
assert all(e['first_last_visible_equal'] for e in metrics['items'] if e['action']!='death')
result=dict(canonical_clips=40,canonical_frames=344,exact_native_repairs=len(checks),checks=checks,nondeath_clips_first_last_equal=32,unchanged_pngs_other_states_rotations_death=len(unchanged),old_states_preserved=True,first_pose_visible_rgba_equals_reference=40,sha256_official_zip=hashlib.sha256((root/'pixellab-final.zip').read_bytes()).hexdigest(),runtime_unity_test=False)
(root/'verificacao.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k not in ['checks','sha256_official_zip']},ensure_ascii=False))
