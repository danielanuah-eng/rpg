"""Integridade do atlas, replay GIF, cobertura e margens reais de todos os frames."""
from pathlib import Path
from PIL import Image, ImageSequence
import hashlib, json
import numpy as np
ROOT=Path(__file__).resolve().parent
manifest=json.loads((ROOT/'eventos.visual.json').read_text(encoding='utf-8'))
ids={e['id'] for e in manifest['events']}
repo=ROOT.parents[4]
core=(repo/'ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Match.cs').read_text(encoding='utf-8-sig')
assert len(ids)==12 and all('"'+i+'"' in core for i in ids)
rows=[]
for event in manifest['events']:
    dest=ROOT/event['id'];w,h=event['frameSize'];atlas=Image.open(ROOT/event['atlas']).convert('RGBA');margins=[];frames=[]
    assert atlas.size==(w*4,h*3)
    for f in range(event['frames']):
        img=Image.open(dest/f'{f:02d}.png').convert('RGBA');pixels=np.array(img);a=pixels[:,:,3];b=img.getchannel('A').getbbox()
        assert img.size==(w,h) and set(np.unique(a))=={0,255}
        assert b[0]>=6 and b[1]>=6 and w-b[2]>=6 and h-b[3]>=6
        cell=atlas.crop(((f%4)*w,(f//4)*h,(f%4+1)*w,(f//4+1)*h))
        assert np.array_equal(pixels,np.array(cell))
        frames.append(pixels);margins.append(min(b[0],b[1],w-b[2],h-b[3]))
    with Image.open(dest/'preview.gif') as gif:
        assert gif.n_frames==event['frames'] and gif.info['loop']==0
        for f,decoded in enumerate(ImageSequence.Iterator(gif)):
            actual=np.array(decoded.convert('RGBA'));original=frames[f];mask=original[:,:,3]>0
            assert np.array_equal(actual[:,:,3],original[:,:,3]),f'Alpha GIF {event["id"]} F{f}'
            assert np.array_equal(actual[:,:,:3][mask],original[:,:,:3][mask]),f'RGB GIF {event["id"]} F{f}'
            assert decoded.info['duration']==event['durationsMs'][f]
    for item in event['files']:assert hashlib.sha256((ROOT/item['path']).read_bytes()).hexdigest()==item['sha256']
    mean_steps=[float(np.abs(frames[f][:,:,:3].astype(int)-frames[(f+1)%len(frames)][:,:,:3].astype(int)).mean()) for f in range(len(frames))]
    assert max(mean_steps)<6,'Mudanca muito brusca no ciclo'
    rows.append({'id':event['id'],'frames':len(frames),'alphaRange':[0,255],'minMarginPx':min(margins),'maxMeanRgbStep':round(max(mean_steps),3),'gifReplayExact':True,'atlasExact':True})
report={'status':'passed','events':len(rows),'frames':sum(r['frames'] for r in rows),'generationCost':0,'coverage':'12 IDs presentes no Match.cs','checks':rows,'visualReview':'contato visto pelo Sol; formas distintas e contornos preservados','integration':'pendente_claude'}
(ROOT/'verificacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
print(json.dumps({k:v for k,v in report.items() if k!='checks'},ensure_ascii=False))
