from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
root=Path(__file__).resolve().parent
metrics=json.loads((root/'auditoria-final/metricas.json').read_text(encoding='utf-8'))
fps={'idle':5,'walk':10,'attack':7,'hit':8,'death':6}
groups={'idle':'6b45c58c-64d6-4eb4-bcc7-9020e27c5a9a','walk':'f066e10c-b51c-4bf2-aad4-42c7aecdafb6','attack':'b32eec9f-07de-4057-9171-be3ff653c476','hit':'0ea5036c-fcab-4df3-8dc7-2d4858c04c40','death':'f4d075da-1be8-47d8-a819-84a059f3e20b'}
for entry in metrics['items']:
    entry.update(fps=fps[entry['action']],duration_seconds=round(entry['frames']/fps[entry['action']],3),loop=entry['action'] in ['idle','walk'],group_id=groups[entry['action']],pivot_rule='One pivot registered from the unchanged static first pose for all frames; never from whole-animation bbox or smoke.',destination='Assets/ArenaTatica/Resources/Sprites/TchuTchaBerserk/'+Path(entry['file']).name)
references=[]
for file in sorted((root/'sprites').glob('*.png')):
    if '_' in file.stem: continue
    im=Image.open(file).convert('RGBA'); references.append(dict(direction=file.stem,file=file.relative_to(root).as_posix(),size=list(im.size),pivot=[.5,(128-im.getbbox()[3])/128],ppu=128,sha256=hashlib.sha256(file.read_bytes()).hexdigest(),destination='Assets/ArenaTatica/Resources/Sprites/TchuTchaBerserk/'+file.name))
manifest=dict(status='pronto_para_integrar',character_id='215bb320-de53-44a1-bec7-f2e23f4e7265',state_name='Berserk Possuído v2 Sombras',pixellab_state_folder='Berserk_Possuido_v2_Sombras',replaces_resource_folder='TchuTchaBerserk',old_character_id_preserved='a82010d8-719a-4c04-bfb0-2caedc6a6cf5',reference_resolution=[128,128],ppu=128,renderer_height_in_tiles=1.85,views=references,animations=metrics['items'],canonical_clips=40,canonical_frames=344,unused_review_group='1a774d55-7c52-48fb-abbd-d272b055a6da',source_zip='pixellab-final.zip',import_only='sprites/*.png plus this manifest pivots; do not import preview grids, sources, alternate state folders or attack-refinado-sombras.',filter='Point',mipmap=False,compression='None',rgba_check='verificacao.json',runtime_unity_test=False,limitations=['Pivots preserve the static origin; reference bottom alpha can include shadow drips. Physical boot contacts can still be adjusted consistently at reference level.','Frame holds/recovery sequences are intentional and documented in reparos-loops.json/reparos-ataques.json.','Body geometry review passed before final exact VFX recoloring; final pixel equality and unchanged alpha validated after recoloring.'])
(root/'berserk.visual.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
previews=root/'previas'; previews.mkdir(exist_ok=True)
for direction in ['south','south-east','north-west']:
    sequence=[]; delays=[]
    for action in ['idle','walk','attack','hit','idle']:
        entry=next(e for e in metrics['items'] if e['action']==action and e['direction']==direction)
        s=entry['cell']; px,py=entry['pivot']
        for i in range(entry['frames']):
            sprite=Image.open(root/'fontes-finais'/action/direction/f'frame-{i+1:02d}.png').convert('RGBA'); canvas=Image.new('RGBA',(256,256),(35,37,44,255)); draw=ImageDraw.Draw(canvas)
            draw.line((0,235,255,235),fill=(65,95,78,255)); canvas.alpha_composite(sprite,(round(128-px*s),round(235-(1-py)*s))); draw.text((8,8),direction+' '+action+' '+str(i+1),fill='white')
            sequence.append(canvas.resize((512,512),Image.Resampling.NEAREST).convert('RGB')); delays.append(round(1000/fps[action]))
    sequence[0].save(previews/(direction+'.gif'),save_all=True,append_images=sequence[1:],duration=delays,loop=0)
print('Final Berserk manifest:',len(references),'views,',len(metrics['items']),'clips; three transition previews')
