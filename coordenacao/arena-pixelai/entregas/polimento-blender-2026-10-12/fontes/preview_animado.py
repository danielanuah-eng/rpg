"""Renders de quadros a partir do GLB reimportado, não da fonte pré-exportação."""
import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
PAIRS=[('Nixira','walk'),('Nixira','attack'),('GatoLaranja','walk'),('Ossyro','breath'),('Gryvro','walk'),('Damian','morph'),('Teantha','attack'),('EdgarNavio','idle')]
def aim(o,t):o.rotation_euler=(Vector(t)-o.location).to_track_quat('-Z','Y').to_euler()
def bounds():
    bpy.context.view_layer.update();points=[]
    for o in bpy.data.objects:
        if o.type=='MESH':points.extend(o.matrix_world@Vector(p) for p in o.bound_box)
    return [min(p[i] for p in points) for i in range(3)],[max(p[i] for p in points) for i in range(3)]
for folder,clip in PAIRS:
    if '--personagem' in sys.argv and folder not in sys.argv[sys.argv.index('--personagem')+1].split(','):continue
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(ROOT/'personagens'/folder/(folder+'.glb')))
    rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
    action=next(a for a in bpy.data.actions if a.name==clip or a.name.startswith(clip+'_'))
    rig.animation_data.action=action
    if action.slots:rig.animation_data.action_slot=next((s for s in action.slots if s.target_id_type=='OBJECT'),action.slots[0])
    manifest=json.loads((ROOT/'personagens'/folder/'personagem.visual.json').read_text(encoding='utf-8'))
    duration=next(c['duracaoSegundos'] for c in manifest['clipes'] if c['id']==clip)
    scene=bpy.context.scene;scene.render.fps=24;scene.frame_set(1)
    mins=[];maxs=[]
    frames=[round(1+i*duration*24/8) for i in range(8)]
    for f in frames:scene.frame_set(f);a,z=bounds();mins.append(a);maxs.append(z)
    low=[min(v[i] for v in mins) for i in range(3)];high=[max(v[i] for v in maxs) for i in range(3)]
    target=[(a+z)/2 for a,z in zip(low,high)]
    scene.world=bpy.data.worlds.new('Mundo_preview');scene.world.use_nodes=True
    bg=scene.world.node_tree.nodes.get('Background');bg.inputs['Color'].default_value=(.08,.11,.15,1);bg.inputs['Strength'].default_value=.8
    scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=False
    scene.render.resolution_x=384;scene.render.resolution_y=384;scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='Standard';scene.view_settings.look='None'
    for pos,energy in [((-3,-4,6),350),((4,2,3),240)]:
        bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=energy;o.data.size=4;aim(o,target)
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.015));floor=bpy.context.object
    material=bpy.data.materials.new('piso_preview');material.use_nodes=True
    p=material.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(.035,.06,.08,1);p.inputs['Roughness'].default_value=.95
    floor.data.materials.append(material)
    bpy.ops.object.camera_add(location=(4,-7,4));camera=bpy.context.object;camera.data.type='ORTHO';aim(camera,target)
    camera.data.ortho_scale=max(z-a for a,z in zip(low,high))*1.22;scene.camera=camera
    out=ROOT/'personagens/animacoes-preview'/folder/clip;out.mkdir(parents=True,exist_ok=True)
    for i,f in enumerate(frames):
        scene.frame_set(f);scene.render.filepath=str(out/(str(i).zfill(2)+'.png'));bpy.ops.render.render(write_still=True)
    print('PREVIEW_EXPORTADO_OK',folder,clip,flush=True)
