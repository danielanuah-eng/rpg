from pathlib import Path
import json,hashlib
from PIL import Image,ImageDraw
HERE=Path(__file__).resolve().parent
directions=['south','south-east','east','north-east','north','north-west','west','south-west']
actors={'Asamyh':'d0744019-2e8c-4ba0-82f2-5d4a64be69de','CheiroPneu':'fc89abf9-cecd-4ad1-9e2e-af3f43656cd1'}
rows=[]
gallery=Image.new('RGB',(8*192,2*226),(39,44,53));draw=ImageDraw.Draw(gallery)
portrait=Image.new('RGB',(2*288,312),(39,44,53));pdraw=ImageDraw.Draw(portrait)
for row,(actor,uuid) in enumerate(actors.items()):
 folder=HERE/actor
 layout=json.loads((folder/'layout-fonte.json').read_text(encoding='utf8'))
 assert layout['character']['id']==uuid,(actor,layout['character'])
 for col,direction in enumerate(directions):
  file=folder/'celulas/rotations'/(direction+'.png');im=Image.open(file).convert('RGBA')
  alpha=im.getchannel('A');bbox=alpha.getbbox()
  assert im.size==(128,128),(actor,im.size)
  assert alpha.getextrema()==(0,255),(actor,direction,alpha.getextrema())
  assert bbox and bbox[0]>0 and bbox[1]>0 and bbox[2]<128 and bbox[3]<128,(actor,direction,bbox)
  normalized=Image.new('RGBA',(132,132));normalized.paste(im,(2,128-bbox[3]))
  target=folder/'sprites'/(direction+'.png');target.parent.mkdir(exist_ok=True);normalized.save(target)
  norm_alpha=normalized.getchannel('A');norm_bbox=norm_alpha.getbbox()
  assert norm_bbox[3]==128
  assert norm_alpha.histogram()[1:]==alpha.histogram()[1:],(actor,direction,'pixels clipped')
  assert norm_bbox[0]>0 and norm_bbox[1]>0 and norm_bbox[2]<132 and norm_bbox[3]<132
  rows.append({'actor':actor,'character_id':uuid,'direction':direction,'source':str(file.relative_to(HERE)).replace(chr(92),'/'),'png':str(target.relative_to(HERE)).replace(chr(92),'/'),'alpha_extrema':alpha.getextrema(),'bbox':bbox,'native_size':im.size,'frame_size':[132,132],'foot_pixel_y':127,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
  gallery.paste(im.resize((192,192),Image.Resampling.NEAREST),(col*192,row*226),im.resize((192,192),Image.Resampling.NEAREST))
  draw.text((col*192+8,row*226+194),actor+' / '+direction,fill='white')
  if direction=='south':
   large=im.resize((256,256),Image.Resampling.NEAREST);portrait.paste(large,(row*288+16,8),large);pdraw.text((row*288+16,278),actor,fill='white')
 record={'actor_id':uuid,'owner':actor,'rotations':directions,'native_size':[128,128],'frame_size':[132,132],'pivot':'lowest-opaque-foot-pixel','foot_pixel_y':127,'reference_pixels_per_unit':128,'no_rescaling':True,'clips':[],'status':'base_corrigida_conferida','animations':'em_producao','integration':'pendente_claude','frames':[r for r in rows if r['actor']==actor]}
 (folder/'sprites.visual.json').write_text(json.dumps(record,ensure_ascii=False,indent=2)+'\n',encoding='utf8',newline='\n')
 receipt=json.loads((folder/'correcao-autor.json').read_text(encoding='utf8'));receipt.update(status='base_corrigida_conferida',directions=8,alpha_measured=True,manifest='sprites.visual.json',animations='em_producao',integration='pendente_claude')
 (folder/'correcao-autor.json').write_text(json.dumps(receipt,ensure_ascii=False,indent=2)+'\n',encoding='utf8',newline='\n')
 previous=folder/'geracao.json'
 if previous.exists():
  receipt=json.loads(previous.read_text(encoding='utf8'));receipt.update(status='referencia_anterior_rejeitada_pelo_autor',superseded_by='correcao-autor.json');previous.write_text(json.dumps(receipt,ensure_ascii=False,indent=2)+'\n',encoding='utf8',newline='\n')
gallery_path=HERE/'correcoes-autor-oito-angulos.png';gallery.save(gallery_path)
portrait.save(HERE/'correcoes-autor-preview.png')
(HERE/'correcoes-autor-verificacao.json').write_text(json.dumps({'status':'bases_corrigidas_conferidas','alpha_measured':True,'frames':rows,'animations':'em_producao','integration':'pendente_claude'},ensure_ascii=False,indent=2)+'\n',encoding='utf8',newline='\n')
for folder in [HERE/'Asamyh',HERE/'CheiroPneu']:
 for file in folder.iterdir():
  if file.suffix in ['.json','.txt']:
   file.write_bytes(b'\n'.join(line.rstrip(b' \t') for line in file.read_bytes().replace(b'\r\n',b'\n').split(b'\n')))
print(json.dumps({'bases':len(actors),'directions':len(rows),'alpha':'0..255 em todos os PNGs','preview':str(HERE/'correcoes-autor-preview.png')},ensure_ascii=False))
