import bpy
import json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
themes=['pedreira','bosque','deserto','neve','vulcao']
sheet=bpy.data.images.new('Texturas',width=512,height=320,alpha=True)
pixels=[.05,.05,.05,1]*(512*320)
checks=[]
for row,theme in enumerate(themes):
 choice=json.loads((root/'fontes'/(theme+'-texturas.json')).read_text(encoding='utf-8'))
 for col,material in enumerate(choice['selecao']):
  img=bpy.data.images.load(str(root/'mapa'/theme/(material+'.png')))
  w,h=img.size; assert (w,h)==(64,64)
  src=list(img.pixels)
  for y in range(64):
   offset=((256-row*64+y)*512+col*64)*4
   pixels[offset:offset+256]=src[y*256:(y+1)*256]
  alpha_min=min(src[3::4]); alpha_max=max(src[3::4])
  assert alpha_min==1 and alpha_max==1,material+': textura deve ser opaca'
  assert (root/'mapa'/theme/(material+'.png')).read_bytes()==(root/choice['selecao'][material]).read_bytes(),material+': origem alterada'
  checks.append({'material':material,'size':[w,h],'alphaMin':alpha_min,'alphaMax':alpha_max,'originalConfirmado':True})
sheet.pixels=pixels;sheet.filepath_raw=str(root/'previews'/'texturas-contacto.png');sheet.file_format='PNG';sheet.save()
(root/'fontes'/'texturas-auditoria.json').write_text(json.dumps(checks,indent=2),encoding='utf-8')
