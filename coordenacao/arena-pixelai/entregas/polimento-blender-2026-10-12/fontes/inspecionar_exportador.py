import bpy,json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(root.parent/'mapa-blender/v03/fontes/bosque.blend'))
p=bpy.ops.export_scene.gltf.get_rna_type().properties
for key in p.keys():
    if 'color' in key:
        prop=p[key]
        print('COLOR_OPTION',key,[(e.identifier,e.name) for e in prop.enum_items] if prop.type=='ENUM' else prop.default)
o=bpy.data.objects['bosque_arvore']
print('ATTRS',[(a.name,a.domain,a.data_type) for a in o.data.color_attributes], 'ACTIVE',o.data.color_attributes.active_color_name)
