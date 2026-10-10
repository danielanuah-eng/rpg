from pathlib import Path
import json,urllib.request,concurrent.futures,hashlib,shutil,zipfile,io
from PIL import Image
import numpy as np
p=Path(__file__).resolve().parent
bridge=p.parents[1]
items=json.loads((p/'fontes.json').read_text(encoding='utf-8'))
if any(e.get('locked_final') for e in items):
    raise SystemExit('Entrega final já validada. Não regenerar a partir dos ZIPs antigos. Use a tira final e canonico-verificado.json do Damian.')
archives={}
for character_id in sorted(set(e['id'] for e in items)):
    url='https://api.pixellab.ai/mcp/characters/'+character_id+'/download'
    archive=p/(character_id+'.zip')
    if archive.exists(): data=archive.read_bytes()
    else:
        with urllib.request.urlopen(url,timeout=60) as r: data=r.read()
        archive.write_bytes(data)
    archives[character_id]=zipfile.ZipFile(io.BytesIO(data))
jobs=[]
for e in items:
    folder=p/e['key']; folder.mkdir(parents=True,exist_ok=True)
    zip_names=archives[e['id']].namelist()
    export_clip='pistol-se-codex-review-v2' if e.get('candidate') else e['clip']
    selected=[n for n in zip_names if '/'+e['dir']+'/' in n and '/'+export_clip+'/' in n and n.endswith('.png')]
    if e['key']=='berserkIdleSW': selected=[n for n in selected if n.startswith('Berserk/')]
    if not selected:
        # Export archives organize animations as animations/<name>/<direction>/frame_N.png.
        selected=[n for n in zip_names if e['dir'] in n and e['clip'] in n and n.endswith('.png')]
    if len(selected)!=len(e['urls']):
        raise RuntimeError('Export layout mismatch for '+e['key']+': '+str(selected)+'; archive sample: '+str(zip_names[:40]))
    selected.sort(key=lambda s:int(__import__('re').search(r'(\d+)\.png$',s).group(1)))
    for i,entry in enumerate(selected):
        target=folder/f'frame-{i+1:02d}.png'; target.write_bytes(archives[e['id']].read(entry)); Image.open(target).verify()
def download(pair):
    url,target=pair
    with urllib.request.urlopen(url,timeout=45) as r: data=r.read()
    target.write_bytes(data)
    Image.open(target).verify()
    return target.name
checks=[]
for e in items:
    folder=p/e['key']; frames=[Image.open(folder/f'frame-{i+1:02d}.png').convert('RGBA') for i in range(len(e['urls']))]
    original=Image.open(bridge/e['source_file']).convert('RGBA')
    name=e['clip']+'_'+e['dir']
    shutil.copy2(bridge/e['source_file'],folder/(name+'.anterior.png'))
    if not e.get('candidate'):
        size=e['size']; assert all(fr.size==(size,size) for fr in frames)
        if e.get('sequence'):
            assert all(np.array_equal(np.array(fr),np.array(original.crop(((k-1)*size,0,k*size,size)))) for fr,k in zip(frames,e['sequence']))
        else:
            srcs=[np.array(original.crop((i*size,0,(i+1)*size,size))) for i in range(len(frames))]
            for ch in e['patch']['changes']:
                for fi in ch['frames']:
                    for x,y,expected,replacement in ch['pixels']: srcs[fi-1][y,x]=[0,0,0,0]
            assert all(np.array_equal(np.array(fr),src) for fr,src in zip(frames,srcs))
        verified='exact pixel equality to approved edits'
    else:
        size=max(max(fr.size) for fr in frames)
        normalized=[]
        for fr in frames:
            square=Image.new('RGBA',(size,size)); square.paste(fr,((size-fr.width)//2,(size-fr.height)//2)); normalized.append(square)
        frames=normalized
        frames[-1]=frames[0].copy()
        verified='lossless transparent padding to square; final frame copied from first'
    strip=Image.new('RGBA',(size*len(frames),size))
    for i,fr in enumerate(frames): strip.paste(fr,(i*size,0))
    final=folder/(name+'.png'); strip.save(final)
    thumb=Image.new('RGBA',(size*3,size*((len(frames)+2)//3)))
    for i,fr in enumerate(frames): thumb.paste(fr,((i%3)*size,(i//3)*size))
    thumb.save(folder/'preview-grid.png')
    frames[0].save(folder/'preview.gif',save_all=True,append_images=frames[1:],duration=160 if e['clip']!='idle' else 200,loop=0,disposal=2,optimize=False)
    checks.append(dict(key=e['key'],path=str(final),frames=len(frames),cell=size,sha256=hashlib.sha256(final.read_bytes()).hexdigest(),verification=verified,first_last_equal=np.array_equal(np.array(frames[0]),np.array(frames[-1])),alpha_zero=int((np.array(strip)[:,:,3]==0).sum()),source_sizes=[list(Image.open(folder/f'frame-{i+1:02d}.png').size) for i in range(len(e['urls']))]))
(p/'verificacao.json').write_text(json.dumps(checks,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(checks,ensure_ascii=False))
