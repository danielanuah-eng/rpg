import bpy, json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT.parent/'mapa-blender/v03/fontes'
result=[]
for theme in ['bosque','pedreira','deserto','neve','vulcao']:
    bpy.ops.wm.open_mainfile(filepath=str(BASE/(theme+'.blend')))
    result.append({'tema':theme,'colecoes':[{ 'nome':c.name,'objetos':len(c.objects),'oculta':c.hide_render} for c in bpy.data.collections],
      'pecas':[{'nome':o.name,'peca':o.get('peca'),'colecoes':[c.name for c in o.users_collection], 'dimensoes':list(o.dimensions)} for o in bpy.data.objects if o.get('peca') and not o.name.startswith('DEMO')][:60],
      'materiais':[{'nome':m.name,'cor':list(m.diffuse_color)} for m in bpy.data.materials],
      'camera':bpy.context.scene.camera.name if bpy.context.scene.camera else None})
(ROOT/'inspecao.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print('INSPECAO_SALVA',len(result))
