"""Galeria no Blender dos modelos reais exportados, com a mesma escala."""
import bpy
import math
import json
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
themes=[('pedreira','Pinheiro da montanha'),('bosque','Bosque frondoso'),('deserto','Cacto e palmeira'),('neve','Pinheiro nevado'),('vulcao','Árvore carbonizada')]
camera_pos=Vector((0,-17,9))
for i,(theme,label) in enumerate(themes):
    x=(i-2)*3.15
    manifest=json.loads((ROOT/'manifestos'/(theme+'.json')).read_text(encoding='utf-8'))
    models=['arvore','arvore_palmeira'] if theme=='deserto' else ['arvore']
    for j,name in enumerate(models):
        bpy.ops.object.select_all(action='DESELECT')
        before=set(bpy.data.objects)
        entry=next(e for e in manifest['pecas'] if e['peca']==name)
        bpy.ops.import_scene.fbx(filepath=str(ROOT/entry['arquivo']),colors_type='LINEAR')
        for obj in set(bpy.data.objects)-before:
            if obj.type=='MESH': obj.location.x=x+(-.52 if j==0 else .52) if len(models)>1 else x
    for mat in bpy.data.materials:
        if mat.use_nodes:
            for node in mat.node_tree.nodes:
                if node.type=='TEX_IMAGE': node.interpolation='Closest'
    bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=1.32,depth=.10,location=(x,0,-.055))
    platform=bpy.context.object
    platform.name='BASE_EXPOSICAO_'+theme
    mat=bpy.data.materials.new('Exposicao_'+theme)
    mat.use_nodes=True
    mat.diffuse_color=(.17,.22,.24,1)
    mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.17,.22,.24,1)
    mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.9
    platform.data.materials.append(mat)
    bpy.ops.object.text_add(location=(x,-1.85,-.07))
    caption=bpy.context.object
    caption.name='LEGENDA_'+theme
    caption.data.body=label
    caption.data.align_x='CENTER'
    caption.data.size=.25
    caption.rotation_euler=Vector((0,-17,8.3)).to_track_quat('Z','Y').to_euler()
    ink=bpy.data.materials.get('Legenda') or bpy.data.materials.new('Legenda')
    ink.diffuse_color=(.95,.93,.81,1)
    ink.use_nodes=True
    ink.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.95,.93,.81,1)
    caption.data.materials.append(ink)
bpy.ops.object.camera_add(location=camera_pos)
camera=bpy.context.object
camera.rotation_euler=(Vector((0,0,.7))-camera.location).to_track_quat('-Z','Y').to_euler()
camera.data.type='ORTHO'
camera.data.ortho_scale=16.5
scene=bpy.context.scene
scene.camera=camera
scene.world.color=(.16,.20,.22)
scene.world.use_nodes=True
scene.world.node_tree.nodes.get('Background').inputs['Color'].default_value=(.22,.28,.31,1)
scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.65
bpy.ops.object.light_add(type='AREA',location=(-5,-6,8))
key=bpy.context.object
key.data.energy=1400
key.data.shape='DISK'
key.data.size=8
key.rotation_euler=(Vector((0,0,.8))-key.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.light_add(type='SUN',location=(4,3,9))
sun=bpy.context.object
sun.data.energy=1.5
sun.rotation_euler=(Vector((0,0,0))-sun.location).to_track_quat('-Z','Y').to_euler()
scene.render.engine='CYCLES'
scene.cycles.samples=32
scene.cycles.use_denoising=True
scene.render.resolution_x=1800
scene.render.resolution_y=650
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='Standard'
scene.view_settings.look='None'
scene.render.filepath=str(ROOT/'previews'/'arvores-cinco-temas.png')
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/'arvores-cinco-temas.blend'))
bpy.ops.render.render(write_still=True)
print('GALERIA_ARVORES_OK',flush=True)
