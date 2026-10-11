"""Empacota os quadros nativos; a órbita é implementada pelo motor."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib
ROOT=Path(__file__).resolve().parent
IDS={'anjo':('c9d02e54-2403-4c7f-b541-bac5002ccfde','baltazar-aura-curar'),'demonio':('7b7eb063-4b50-4c00-8ca2-832752db06fd','baltazar-aura-ferir')}
assets=[]
gallery=Image.new('RGB',(800,360),'#202632');d=ImageDraw.Draw(gallery)
for row,(name,(job,event)) in enumerate(IDS.items()):
    frames=[Image.open(p).convert('RGBA') for p in sorted((ROOT/'fontes'/name).glob('frame_*.png'))]
    assert len(frames)==5 and all(im.size==(64,64) for im in frames)
    sheet=Image.new('RGBA',(320,64)); measured=[]
    for i,im in enumerate(frames):
        a=im.getchannel('A').histogram();assert a[0]>0 and a[255]>0
        measured.append({'transparent':a[0],'opaque':a[255],'partial':sum(a[1:255])})
        sheet.paste(im,(i*64,0));preview=im.resize((160,160),Image.Resampling.NEAREST)
        gallery.paste(preview,(i*160,row*180+20),preview)
        d.text((i*160+6,row*180+4),f'{name} / {i}',fill='white')
    path=ROOT/f'{name}-hover.png';sheet.save(path)
    assets.append({'id':f'baltazar-{name}-hover','file':path.name,'frameSize':[64,64],'frames':5,'layout':'horizontal','playbackFrames':[0,1,2,3,4,3,2,1],'fpsProposed':6,'motion':'hover suave; ping-pong evita salto na emenda','sourceJob':job,'alphaPerFrame':measured,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'eventProposed':event,'integration':'pendente_claude','orbit':'motor; dois companheiros separados 180 graus; centro em Baltazar; fase frente/tras conforme camera; colisao visual nao repete efeito'})
(ROOT/'manifesto.json').write_text(json.dumps({'assets':assets,'orbitRangeCells':2,'maintenancePerTurn':1,'resource':'PA proposto; confirmar com Claude','implementation':'pendente_claude','logic':'Uma aplicacao por alvo por turno de Baltazar; conforme ficha proposta, independente da animacao.'},ensure_ascii=False,indent=2),encoding='utf-8')
gallery.save(ROOT/'companheiros-quadros.png')
print('COMPANHEIROS_OK_10_QUADROS')
