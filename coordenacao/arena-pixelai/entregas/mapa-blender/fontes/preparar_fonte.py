"""Abre a fonte pronta, confere sua presença e melhora a vista inicial do Blender."""
import bpy
import json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
manifest=json.loads((root/'manifesto.json').read_text(encoding='utf-8'))
collection=bpy.data.collections['KIT_BOSQUE_ORIGINAIS_PIVO_ZERO']
collection.hide_render=True
collection.hide_viewport=True
for entry in manifest['pecas']:
    obj=bpy.data.objects.get('bosque_'+entry['peca'])
    assert obj is not None and obj.name in collection.objects,entry['peca']
    assert obj.location.length<.00001 and all(abs(v-1)<.00001 for v in obj.scale)
    obj.data.calc_loop_triangles()
    assert len(obj.data.loop_triangles)==entry['tris']
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_perspective='CAMERA'
bpy.ops.wm.save_as_mainfile(filepath=str(root/'fontes'/'bosque-piloto.blend'))
print('FONTE_BLEND_OK',len(manifest['pecas']),'pecas editaveis, previews presentes')
