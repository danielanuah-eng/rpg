"""Render the actual editable asset library, without modifying the .blend source."""
import bpy
from pathlib import Path
from mathutils import Vector

project = Path(__file__).resolve().parents[2]
art = project / 'Assets' / 'DungeonBDX' / 'Art'
bpy.ops.wm.open_mainfile(filepath=str(project / 'Tools' / 'GenerateDungeonAssets' / 'Source' / 'DungeonBDX-original-kit.blend'))
scene = bpy.context.scene
scene.render.engine = 'BLENDER_EEVEE'
scene.render.resolution_x = 1600
scene.render.resolution_y = 1000
scene.render.resolution_percentage = 100
scene.world = bpy.data.worlds.new('Preview ambience')
scene.world.use_nodes = True
scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.12, .15, .22, 1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value = .8
bpy.ops.object.camera_add(location=(13, -12, 17))
camera = bpy.context.object
camera.rotation_euler = (Vector((7.3, 2.9, .45)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 20
scene.camera = camera
for position, energy, size in (((3, -5, 10), 1800, 9), ((14, 7, 9), 2400, 7)):
    bpy.ops.object.light_add(type='AREA', location=position)
    light = bpy.context.object
    light.data.energy = energy
    light.data.shape = 'DISK'
    light.data.size = size
    light.rotation_euler = (Vector((7.5, 3, 0)) - light.location).to_track_quat('-Z', 'Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -.19))
mat = bpy.data.materials.new('Preview ground')
mat.diffuse_color = (.045, .055, .075, 1)
bpy.context.object.data.materials.append(mat)
scene.view_settings.view_transform = 'AgX'
scene.render.image_settings.file_format = 'PNG'
(art / 'Preview').mkdir(exist_ok=True)
scene.render.filepath = str(art / 'Preview' / 'original-kit.png')
bpy.ops.render.render(write_still=True)
