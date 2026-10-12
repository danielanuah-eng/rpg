import bpy,json,math,sys,importlib.util,hashlib,struct
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
PROJECT=ROOT.parents[3]
def module(name):
    s=importlib.util.spec_from_file_location(name,ROOT/'fontes'/(name+'.py'));m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
g=module('personagens_geometria');motion=module('personagens_movimento')
inventory=json.loads((ROOT/'personagens/inventario.json').read_text(encoding='utf-8'))
selected=sys.argv[sys.argv.index('--personagem')+1] if '--personagem' in sys.argv else None

def aim(o,target):o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()

def main(entry):
    folder=entry['pasta'];out=ROOT/'personagens'/folder;out.mkdir(exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True);bpy.context.scene.unit_settings.system='METRIC'
    rig,mesh,factor=g.build(folder)
    actions=list(entry['acoes'])
    additions={'Colosso':['stomp','steal-reach','steal-slam','steal-cast'],'Diantus':['leap'],'Talyssa':['attack'],'Ossyro':['breath']}
    actions=sorted(set(actions+additions.get(folder,[])))
    clips=motion.generate(rig,folder,actions,factor)
    scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=False
    scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100
    scene.render.filter_size=.8;scene.render.image_settings.file_format='PNG';scene.render.film_transparent=True
    scene.world=bpy.data.worlds.new('Iluminacao_neutra');scene.world.use_nodes=True;scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.7
    scene.view_settings.view_transform='Standard';scene.view_settings.look='None'
    zmax=max(v.co.z for v in mesh.data.vertices);target=(0,0,zmax*.5)
    for typ,pos,energy,size in [('AREA',(-3,-4,6),400,4),('AREA',(4,2,3),300,3)]:
        bpy.ops.object.light_add(type=typ,location=pos);o=bpy.context.object;o.data.energy=energy;o.data.size=size;aim(o,target)
    bpy.ops.object.camera_add(location=(4,-7,4));cam=bpy.context.object;cam.data.type='ORTHO'
    xs=[v.co.x for v in mesh.data.vertices];ys=[v.co.y for v in mesh.data.vertices]
    cam.data.ortho_scale=max(zmax,max(xs)-min(xs),max(ys)-min(ys))*1.50;aim(cam,target);scene.camera=cam
    scene['referencia']=entry['referencia'];scene['estado']='recriacao_3d_para_revisao_visual'
    bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
    blend=out/(folder+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(blend))
    glb=out/(folder+'.glb')
    bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_animations=True,
      export_animation_mode='ACTIONS',export_force_sampling=True,export_frame_step=1,
      export_materials='EXPORT',export_yup=True,export_cameras=False,export_lights=False,export_rest_position_armature=False)
    data=glb.read_bytes();doc=json.loads(data[20:20+struct.unpack_from('<I',data,12)[0]])
    if folder=='EdgarFantasma':
        for m in doc['materials']:
            m['alphaMode']='BLEND';m['doubleSided']=True;m['pbrMetallicRoughness']['baseColorFactor'][3]=.60
        n=struct.unpack_from('<I',data,12)[0];tail=data[20+n:];encoded=json.dumps(doc,separators=(',',':')).encode();encoded+=b' '*((-len(encoded))%4)
        data=struct.pack('<III',0x46546c67,2,20+len(encoded)+len(tail))+struct.pack('<II',len(encoded),0x4e4f534a)+encoded+tail
        glb.write_bytes(data)
    names=[a.get('name') for a in doc.get('animations',[])]
    assert set(actions)==set(names),(folder,actions,names)
    assert doc.get('skins'),folder+' sem rig exportado'
    scene.render.filepath=str(out/'previa.png');bpy.ops.render.render(write_still=True)
    manifest={'id':folder,'estado':'exportado_para_revisao','integracao':'pendente_claude','referencia':entry['referencia'],'sha256Referencia':entry['sha256'],
      'modelo':folder+'.glb','fonte':folder+'.blend','preview':'previa.png','sha256':hashlib.sha256(data).hexdigest(),
      'unidade':'metro','frenteBlender':'-Y','frenteGLB':'+Z','pivo':'origem na base, sem root motion de deslocamento',
      'dimensoesRestBlender':list(mesh.dimensions),'ossos':len(rig.data.bones),
      'triangulos':sum(doc['accessors'][p['indices']]['count']//3 for m in doc['meshes'] for p in m['primitives']),
      'clipes':clips,'observacoes':['Modelo estilizado em volumes de malha, materiais sem textura ruidosa.',
      'Rig articulado com pesos rígidos por segmento; conferência no Unity ainda pendente.',
      'Projéteis e efeitos de campo continuam assets separados; não mudam regras.',
      'Transformações entre personagens usam clipe de preparação + modelo de destino, a ligar pelo Claude.']}
    manifest['ligacoes']=[{'idAcao':clip['id'],'modelo':folder+'.glb','clipeGLB':clip['id'],
      'referenciasEstadoAtual':entry['acoes'].get(clip['id'],[]),'integracao':'pendente_claude'} for clip in clips]
    (out/'personagem.visual.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    print('PERSONAGEM_OK',folder,len(names),flush=True)

for entry in inventory:
    if selected and entry['pasta'] not in selected.split(','):continue
    main(entry)
