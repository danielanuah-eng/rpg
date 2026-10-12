"""Nova entrega visual: geometria/IDs existentes, materiais limpos e novos renders.
Blender --background --python-exit-code 1 --python polir_mapas.py -- --theme bosque
"""
from pathlib import Path
import bpy, json, hashlib, math, sys, struct
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
PROJECT=ROOT.parents[3]
SOURCE=ROOT.parent/'mapa-blender/v03/fontes'
LIVE=PROJECT/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Models/Mapa'
THEME=sys.argv[sys.argv.index('--theme')+1] if '--theme' in sys.argv else 'bosque'
PALETTES={
 'bosque':{'grama_a':(.29,.43,.19),'grama_b':(.31,.45,.21),'zona':(.38,.50,.28),'terra':(.20,.12,.065),'folhagem':(.09,.28,.10),'casca':(.25,.12,.055),'pedra':(.30,.37,.39)},
 'pedreira':{'grama_a':(.31,.36,.40),'grama_b':(.33,.38,.42),'zona':(.40,.48,.51),'terra':(.12,.15,.19),'folhagem':(.055,.19,.18),'casca':(.22,.16,.12),'pedra':(.15,.21,.28)},
 'deserto':{'grama_a':(.72,.52,.28),'grama_b':(.75,.55,.31),'zona':(.83,.65,.40),'terra':(.39,.22,.11),'folhagem':(.18,.32,.12),'casca':(.36,.21,.09),'pedra':(.56,.39,.24)},
 'neve':{'grama_a':(.74,.84,.88),'grama_b':(.78,.87,.91),'zona':(.34,.58,.72),'terra':(.16,.25,.34),'folhagem':(.29,.48,.52),'casca':(.19,.25,.30),'pedra':(.14,.24,.32)},
 'vulcao':{'grama_a':(.095,.115,.16),'grama_b':(.115,.135,.18),'zona':(.23,.26,.33),'terra':(.035,.045,.065),'folhagem':(.055,.065,.085),'casca':(.04,.032,.04),'pedra':(.34,.37,.43),'agua_emissivo':(1,.16,.015),'lava_emissivo':(1,.16,.015)},
 'comum':{}
}
ACCENTS={'madeira':(.42,.23,.09),'telha':(.53,.15,.095),'ferro':(.18,.24,.29),'ouro':(.95,.56,.11),'creme':(.88,.77,.56),'sombra':(.035,.05,.055)}

def aim(obj,target=(0,0,.25)):
    obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()

def clean_materials(theme):
    records=[]
    for m in bpy.data.materials:
        if not m.use_nodes: continue
        p=next((n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None)
        if p is None: continue
        key=m.name
        for prefix in ['bosque_','pedreira_','deserto_','neve_','vulcao_']: key=key.removeprefix(prefix)
        key=key.split('.')[0]
        old=list(m.diffuse_color)
        color=PALETTES[theme].get(key,ACCENTS.get(key,old[:3]))
        for socket in ['Base Color','Roughness','Normal']:
            for link in list(p.inputs[socket].links): m.node_tree.links.remove(link)
        for node in list(m.node_tree.nodes):
            if node.type=='TEX_IMAGE': m.node_tree.nodes.remove(node)
        p.inputs['Base Color'].default_value=(*color,1)
        p.inputs['Roughness'].default_value=.78 if key in ('ouro','ferro') else .88
        p.inputs['Specular IOR Level'].default_value=.18
        m.diffuse_color=(*color,1)
        if 'emissivo' in key:
            p.inputs['Emission Color'].default_value=(*color,1)
            p.inputs['Emission Strength'].default_value=.6 if 'lava' in key else .2
        records.append({'material':m.name,'antes':old,'depois':list(m.diffuse_color),'texturaRepetitivaRemovida':True})
    return records

def setup_render():
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=64
    scene.cycles.use_denoising=False
    scene.render.resolution_x=1800
    scene.render.resolution_y=1350
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.film_transparent=False
    scene.render.filter_size=.65
    scene.render.use_compositing=False
    scene.view_settings.view_transform='Standard'
    scene.view_settings.look='None'
    scene.world.use_nodes=True
    bg=scene.world.node_tree.nodes.get('Background')
    bg.inputs['Color'].default_value=(.16,.20,.25,1)
    bg.inputs['Strength'].default_value=.7
    for obj in bpy.data.objects:
        if obj.type=='LIGHT':
            if obj.data.type=='SUN': obj.data.energy=2.3; obj.data.angle=math.radians(4)
            else: obj.data.energy=650; obj.data.size=9
        if obj.type=='CAMERA':
            obj.data.dof.use_dof=False
            obj.data.ortho_scale=24
    scene['direcao_visual']='v04: paleta limpa, volumes legiveis, sem blur/denoise; IDs e malhas preservados'

def fix_glb(path):
    data=path.read_bytes(); n=struct.unpack_from('<I',data,12)[0]
    doc=json.loads(data[20:20+n]); tail=data[20+n:]
    for s in doc.get('samplers',[]): s.update(magFilter=9728,minFilter=9728)
    encoded=json.dumps(doc,separators=(',',':')).encode(); encoded+=b' '*((-len(encoded))%4)
    path.write_bytes(struct.pack('<III',0x46546c67,2,20+len(encoded)+len(tail))+struct.pack('<II',len(encoded),0x4e4f534a)+encoded+tail)
    return doc

def export_selected(path,animated=False):
    for obj in bpy.context.selected_objects:
        if obj.type!='MESH': continue
        attrs=obj.data.color_attributes
        neutral=attrs.get('clean_neutro') or next((a for a in attrs if len(a.data) and all(all(abs(v-1)<.0001 for v in c.color) for c in list(a.data)[:64])),None)
        if neutral is None: neutral=attrs.new(name='clean_neutro',type='FLOAT_COLOR',domain='POINT')
        neutral.data.foreach_set('color',[1.0]*(4*len(neutral.data)))
        attrs.active_color_index=list(attrs).index(neutral)
    bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,
      export_yup=True,export_materials='EXPORT',export_cameras=False,export_lights=False,
      export_animations=animated,export_vertex_color='ACTIVE',export_all_vertex_colors=True,export_animation_mode='ACTIONS')
    doc=fix_glb(path)
    return {'arquivo':str(path.relative_to(ROOT)),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
      'triangulos':sum(doc['accessors'][p['indices']]['count']//3 for mesh in doc.get('meshes',[]) for p in mesh['primitives']),
      'animacoes':[a.get('name') for a in doc.get('animations',[])],'texturas':len(doc.get('textures',[]))}

def main():
    for d in ['fontes','previews','manifestos','models/'+THEME]: (ROOT/d).mkdir(parents=True,exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE/(THEME+'.blend')))
    materials=clean_materials(THEME)
    setup_render()
    kit=next(c for c in bpy.data.collections if c.name.startswith('KIT_'))
    kit.hide_viewport=False
    kit.hide_render=False
    records=[]
    for obj in kit.objects:
        piece=obj.get('peca')
        if not piece: continue
        bpy.ops.object.select_all(action='DESELECT'); obj.select_set(True)
        for child in obj.children_recursive: child.select_set(True)
        bpy.context.view_layer.objects.active=obj
        record=export_selected(ROOT/'models'/THEME/(piece+'.glb'))
        record.update(id=piece,dimensoesBlender=list(obj.dimensions),pivo=list(obj.location),origem=str((SOURCE/(THEME+'.blend')).relative_to(ROOT.parent)))
        records.append(record)
    kit.hide_render=True
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/(THEME+'.blend')))
    scene=bpy.context.scene
    scene.render.filepath=str(ROOT/'previews'/(THEME+'-clean.png'))
    bpy.ops.render.render(write_still=True)
    print('RENDER_CLEAN_OK',THEME,flush=True)
    # Extras atuais, inclusive props animados: reexportar os canais efetivos.
    for original in sorted((LIVE/THEME).glob('*.glb')):
        if original.stem in {e['id'] for e in records}: continue
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.gltf(filepath=str(original))
        clean_materials(THEME)
        bpy.ops.object.select_all(action='SELECT')
        record=export_selected(ROOT/'models'/THEME/original.name,True)
        extras=ROOT/'fontes/extras'; extras.mkdir(exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=str(extras/(THEME+'-'+original.stem+'.blend')))
        record['fonte']=str((extras/(THEME+'-'+original.stem+'.blend')).relative_to(ROOT))
        record.update(id=original.stem,origem=str(original.relative_to(PROJECT)))
        records.append(record)
    report={'tema':THEME,'estado':'exportado_para_revisao','integracao':'pendente_claude',
      'fonte':'fontes/'+THEME+'.blend','preview':'previews/'+THEME+'-clean.png',
      'materiais':materials,'pecas':records,'geometria':'kit original preservado','regraDoMapa':'inalterada'}
    (ROOT/'manifestos'/(THEME+'.json')).write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print('TEMA_CLEAN_OK',THEME,len(records),flush=True)

if __name__=='__main__': main()
