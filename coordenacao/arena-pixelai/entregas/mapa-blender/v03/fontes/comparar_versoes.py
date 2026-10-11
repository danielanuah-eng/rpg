"""Prancha comparativa renderizada no Blender a partir das três prévias reais."""
import bpy
import sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
versions=[('bosque-tabuleiro-12x12.png','1 · Clareira entre bosquetes'),
          ('bosque-versao-02.png','2 · Trilha entre clareiras'),
          ('bosque-versao-03.png','3 · Encosta e clareira')]
themes_mode='--temas' in sys.argv
if themes_mode:
    versions=[('deserto-tabuleiro-12x12.png','Deserto'),('vulcao-tabuleiro-12x12.png','Vulcão'),
              ('pedreira-tabuleiro-12x12.png','Montanha rochosa'),('neve-tabuleiro-12x12.png','Neve')]
for i,(filename,title) in enumerate(versions):
    x=(-2.1 if i%2==0 else 2.1) if themes_mode else (i-1)*4.2
    y=(1.85 if i<2 else -1.85) if themes_mode else 0
    mesh=bpy.data.meshes.new(title)
    mesh.from_pydata([(x-2,y-1.5,0),(x+2,y-1.5,0),(x+2,y+1.5,0),(x-2,y+1.5,0)],[],[(0,1,2,3)])
    mesh.uv_layers.new(name='UVMap')
    for j,uv in enumerate([(0,0),(1,0),(1,1),(0,1)]): mesh.uv_layers.active.data[j].uv=uv
    obj=bpy.data.objects.new(title,mesh)
    bpy.context.collection.objects.link(obj)
    mat=bpy.data.materials.new(title)
    mat.use_nodes=True
    nodes=mat.node_tree.nodes
    nodes.clear()
    texture=nodes.new('ShaderNodeTexImage')
    texture.image=bpy.data.images.load(str(ROOT/'previews'/filename))
    texture.interpolation='Closest'
    emission=nodes.new('ShaderNodeEmission')
    output=nodes.new('ShaderNodeOutputMaterial')
    mat.node_tree.links.new(texture.outputs['Color'],emission.inputs['Color'])
    mat.node_tree.links.new(emission.outputs[0],output.inputs['Surface'])
    obj.data.materials.append(mat)
    bpy.ops.object.text_add(location=(x,y-1.80,.02))
    text=bpy.context.object
    text.data.body=title
    text.data.align_x='CENTER'
    text.data.size=.25
    ink=bpy.data.materials.get('Titulo') or bpy.data.materials.new('Titulo')
    ink.use_nodes=True
    ink.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(1,1,1,1)
    ink.node_tree.nodes.get('Principled BSDF').inputs['Emission Color'].default_value=(.9,.93,.92,1)
    ink.node_tree.nodes.get('Principled BSDF').inputs['Emission Strength'].default_value=1
    text.data.materials.append(ink)
bpy.ops.object.camera_add(location=(0,0,10))
camera=bpy.context.object
camera.data.type='ORTHO'
camera.data.ortho_scale=9.2 if themes_mode else 13
scene=bpy.context.scene
scene.camera=camera
scene.world.use_nodes=True
scene.world.node_tree.nodes.get('Background').inputs['Color'].default_value=(.08,.11,.13,1)
scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=1
scene.render.engine='CYCLES'
scene.cycles.samples=1
scene.render.resolution_x=1600 if themes_mode else 1920
scene.render.resolution_y=1400 if themes_mode else 600
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='Standard'
scene.view_settings.look='None'
name='quatro-cenarios' if themes_mode else 'bosque-tres-versoes'
scene.render.filepath=str(ROOT/'previews'/(name+'.png'))
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/(name+'.blend')))
bpy.ops.render.render(write_still=True)
print('COMPARACAO_VERSOES_OK',flush=True)
