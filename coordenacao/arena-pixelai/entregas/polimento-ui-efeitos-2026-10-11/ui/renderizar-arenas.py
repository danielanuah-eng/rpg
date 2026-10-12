"""Re-renderiza cenas existentes com alfa; preserva todos os arquivos-fonte."""
from pathlib import Path
import bpy, hashlib, json
ROOT=Path(__file__).resolve().parent
REPO=ROOT.parents[4]
records=[]
for tema,scene_id in [('bosque','bosque-quatro-patios'),('pedreira','pedreira-anel-partido'),('neve','neve-bastioes-diagonais')]:
    source=REPO/f'coordenacao/arena-pixelai/entregas/mapas-rotativos-2026-10-10/fontes/{scene_id}.blend'
    before=hashlib.sha256(source.read_bytes()).hexdigest()
    bpy.ops.wm.open_mainfile(filepath=str(source))
    scene=bpy.context.scene
    scene.camera=bpy.data.objects['VISTA_JOGADOR_1']
    scene.render.film_transparent=True
    scene.render.resolution_x=1200;scene.render.resolution_y=900;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
    if scene.render.engine=='CYCLES':scene.cycles.samples=16
    scene.render.filepath=str(ROOT/'art'/f'mapa-{tema}.png')
    bpy.ops.render.render(write_still=True)
    assert hashlib.sha256(source.read_bytes()).hexdigest()==before
    records.append({'theme':tema,'source':str(source.relative_to(REPO)).replace('\\','/'),'sourceSha256':before,'output':f'art/mapa-{tema}.png','alpha':True,'camera':'VISTA_JOGADOR_1'})
(ROOT/'arenas-renderizadas.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8',newline='\n')
