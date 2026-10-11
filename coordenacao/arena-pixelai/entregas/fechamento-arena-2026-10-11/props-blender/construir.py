"""Cinco props novos da Arena Tatica, com loops reais em GLB e fonte Blender."""
from pathlib import Path
from mathutils import Vector
import bpy, hashlib, json, math
ROOT=Path(__file__).resolve().parent
for folder in ('modelos','previews','fontes'):(ROOT/folder).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.fps=24;scene.frame_start=1;scene.frame_end=49
PALETTE={'ink':(.12,.13,.19),'stone':(.36,.40,.44),'stoneLight':(.57,.61,.61),'wood':(.40,.26,.17),'woodLight':(.61,.42,.25),'gold':(.76,.55,.25),'silver':(.56,.63,.68),'leaf':(.31,.57,.29),'leafLight':(.55,.71,.36),'water':(.21,.64,.73),'ice':(.55,.80,.87),'lava':(.88,.30,.14),'flame':(.98,.62,.24),'violet':(.57,.40,.72),'sand':(.77,.67,.43)}
MATS={}
for name,color in PALETTE.items():
    m=bpy.data.materials.new('arena_prop_'+name);m.diffuse_color=(*color,1);m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=.88
    if name in ('water','lava','flame','violet'):
        bs.inputs['Emission Color'].default_value=(*color,1);bs.inputs['Emission Strength'].default_value=.22
    MATS[name]=m

def root(name,theme):
    col=bpy.data.collections.new(name);scene.collection.children.link(col)
    o=bpy.data.objects.new(name,None);col.objects.link(o);o['arena_id']=name;o['theme']=theme;o['collision']='decoracao';o['unit_meters']=1.0
    return o,col

def finish_object(o,name,parent,col,material):
    o.name=name
    for c in list(o.users_collection):c.objects.unlink(o)
    col.objects.link(o);o.parent=parent
    if o.type=='MESH':
        o.data.materials.append(MATS[material])
        for p in o.data.polygons:p.use_smooth=False
    return o

def box(name,parent,col,pos,size,mat,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('chanfro_plano','BEVEL');mod.width=bevel;mod.segments=1
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish_object(o,name,parent,col,mat)

def cylinder(name,parent,col,pos,radius,depth,mat,verts=8):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=radius,depth=depth,location=pos)
    return finish_object(bpy.context.object,name,parent,col,mat)

def cone(name,parent,col,pos,r1,r2,depth,mat,verts=8):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r1,radius2=r2,depth=depth,location=pos)
    return finish_object(bpy.context.object,name,parent,col,mat)

def gem(name,parent,col,pos,size,mat):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=pos);o=bpy.context.object;o.scale=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish_object(o,name,parent,col,mat)

def curves(action):
    if hasattr(action,'fcurves'):return list(action.fcurves)
    return [fc for layer in action.layers for strip in layer.strips for bag in getattr(strip,'channelbags',[]) for fc in bag.fcurves]

def animate(o,prop,values,linear=False):
    for frame,value in values:
        setattr(o,prop,value);o.keyframe_insert(data_path=prop,frame=frame)
    action=o.animation_data.action;action.name=o.name+'_ambient'
    for fc in curves(action):
        for key in fc.keyframe_points:key.interpolation='LINEAR' if linear else 'BEZIER'
    track=o.animation_data.nla_tracks.new();track.name='AmbientLoop'
    strip=track.strips.new('AmbientLoop',1,action);strip.action_frame_start=1;strip.action_frame_end=49
    o.animation_data.action=None

def wave(o,prop,base,amplitude,phase=0):
    values=[]
    for f in range(1,50,6):
        s=math.sin((f-1)/48*2*math.pi+phase)
        values.append((f,tuple(b+a*s for b,a in zip(base,amplitude))))
    animate(o,prop,values)

assets=[]
def make_banner():
    name='estandarte_da_clareira';r,c=root(name,'bosque')
    box('base_musgo',r,c,(0,0,.06),(.46,.40,.12),'stone',.025)
    box('haste',r,c,(-.18,0,.73),(.065,.075,1.30),'wood')
    gem('ponteira_folha',r,c,(-.18,0,1.42),(.085,.075,.10),'leafLight')
    box('travessa',r,c,(.04,0,1.27),(.53,.06,.07),'woodLight')
    for i in range(3):
        x=-.13+i*.15
        panel=box('pano_'+str(i),r,c,(x,0,1.24),(.15,.025,.46),'leaf')
        # Origem no topo: oscila a partir da travessa sem movimentar a base.
        for v in panel.data.vertices:v.co.z-=.23
        wave(panel,'rotation_euler',(0,0,0),(.055,.025,0),phase=i*.30)
        if i==1:
            glyph=gem('emblema_folha',panel,c,(0,-.030,-.24),(.06,.012,.12),'leafLight')
    assets.append((r,c,name,'bosque','pano segmentado; movimento suave junto a haste fixa'))

def make_brazier():
    name='braseiro_de_obsidiana';r,c=root(name,'vulcao')
    box('pedestal',r,c,(0,0,.055),(.70,.62,.11),'ink',.025)
    cylinder('coluna',r,c,(0,0,.25),.16,.35,'stone')
    cone('bacia',r,c,(0,0,.48),.19,.34,.22,'ink')
    cylinder('brasas',r,c,(0,0,.60),.26,.025,'lava')
    for i,(x,y,z,h) in enumerate(((-.10,.02,.78,.20),(.08,-.06,.87,.28),(.10,.10,.75,.18))):
        flame=gem('chama_'+str(i),r,c,(x,y,z),(.09,.10,h),'flame' if i%2 else 'lava')
        wave(flame,'scale',(1,1,1),(.08,-.04,.18),phase=i*.90)
    socket=bpy.data.objects.new('fx_fumaca',None);c.objects.link(socket);socket.parent=r;socket.location=(0,0,1.0)
    assets.append((r,c,name,'vulcao','tres chamas facetadas com variacao de volume; socket opcional fx_fumaca'))

def make_fountain():
    name='fonte_de_geada';r,c=root(name,'gelo')
    cylinder('base_octal',r,c,(0,0,.075),.43,.15,'stoneLight')
    cone('cuba',r,c,(0,0,.23),.33,.41,.20,'ice')
    cylinder('agua',r,c,(0,0,.34),.32,.020,'water')
    cylinder('miolo',r,c,(0,0,.49),.095,.31,'silver')
    cone('jato_cristalino',r,c,(0,0,.72),.10,.055,.27,'water')
    for i in range(4):
        a=i*math.pi/2;x=.18*math.cos(a);y=.18*math.sin(a)
        drop=gem('gota_'+str(i),r,c,(x,y,.56),(.045,.045,.065),'ice')
        wave(drop,'location',(x,y,.56),(0,0,.065),phase=a)
    ring=gem('coroa_geada',r,c,(0,0,.88),(.14,.14,.09),'ice')
    wave(ring,'rotation_euler',(0,0,0),(0,0,.18))
    assets.append((r,c,name,'gelo','quatro gotas em ciclos defasados; coroa gira discretamente'))

def make_windmill():
    name='cata_vento_solar';r,c=root(name,'deserto')
    box('base_arenito',r,c,(0,0,.07),(.54,.54,.14),'sand',.02)
    cylinder('mastro',r,c,(0,0,.62),.055,1.04,'wood',verts=6)
    wheel=bpy.data.objects.new('rotor',None);c.objects.link(wheel);wheel.parent=r;wheel.location=(0,0,1.12)
    cylinder('centro_dourado',wheel,c,(0,0,0),.09,.10,'gold')
    for i in range(4):
        a=i*math.pi/2
        arm=box('braco_'+str(i),wheel,c,(.14*math.cos(a),.14*math.sin(a),0),(.31,.035,.04),'woodLight');arm.rotation_euler.z=a
        vane=box('pa_'+str(i),wheel,c,(.25*math.cos(a),.25*math.sin(a),0),(.16,.16,.035),'gold' if i%2 else 'sand');vane.rotation_euler.z=a+.24
    animate(wheel,'rotation_euler',[(1,(0,0,0)),(49,(0,0,2*math.pi))],linear=True)
    assets.append((r,c,name,'deserto','rotor faz uma volta completa; base fixa'))

def make_hourglass():
    name='ampulheta_das_ruinas';r,c=root(name,'ruinas')
    cylinder('base',r,c,(0,0,.065),.34,.13,'stone')
    cylinder('aro_inferior',r,c,(0,0,.18),.26,.09,'gold')
    cylinder('aro_superior',r,c,(0,0,.87),.26,.08,'gold')
    for i in range(4):
        a=math.pi/4+i*math.pi/2
        box('coluna_'+str(i),r,c,(.23*math.cos(a),.23*math.sin(a),.52),(.045,.045,.67),'gold')
    cone('areia_inferior',r,c,(0,0,.35),.19,.02,.25,'sand')
    cone('areia_superior',r,c,(0,0,.70),.02,.19,.25,'sand')
    gem('gargalo',r,c,(0,0,.525),(.075,.075,.065),'violet')
    for i in range(3):
        grain=gem('grao_luz_'+str(i),r,c,(0,-.08,.55+i*.035),(.013,.013,.020),'violet')
        wave(grain,'location',(0,-.08,.55+i*.035),(0,0,.04),phase=i*2*math.pi/3)
    orb=gem('runa_flutuante',r,c,(0,0,1.045),(.085,.085,.10),'violet')
    wave(orb,'location',(0,0,1.045),(0,0,.035))
    assets.append((r,c,name,'ruinas','runa levita e graos luminosos oscilam dentro da estrutura'))

make_banner();make_brazier();make_fountain();make_windmill();make_hourglass()
manifest=[]
for r,c,name,theme,note in assets:
    scene.frame_set(1);bpy.ops.object.select_all(action='DESELECT')
    for o in c.objects:o.select_set(True)
    bpy.context.view_layer.objects.active=r
    triangles=0
    for o in c.objects:
        if o.type=='MESH':o.data.calc_loop_triangles();triangles+=len(o.data.loop_triangles)
    assert triangles<3000
    corners=[]
    for f in range(1,50):
        scene.frame_set(f)
        for o in c.objects:
            if o.type=='MESH':corners.extend(o.matrix_world@Vector(v) for v in o.bound_box)
    bbox=[[min(p[i] for p in corners) for i in range(3)],[max(p[i] for p in corners) for i in range(3)]]
    assert bbox[0][2]>-1e-5 and bbox[1][0]-bbox[0][0]<=1 and bbox[1][1]-bbox[0][1]<=1
    scene.frame_set(1)
    path=ROOT/'modelos'/f'{name}.glb'
    bpy.ops.export_scene.gltf(filepath=str(path),export_format='GLB',use_selection=True,export_yup=True,export_animations=True,export_animation_mode='NLA_TRACKS',export_frame_range=True,export_force_sampling=True,export_nla_strips=True,export_lights=False,export_cameras=False,export_extras=True)
    manifest.append({'id':name,'theme':theme,'file':path.relative_to(ROOT).as_posix(),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'trianglesSource':triangles,'bboxAllFramesBlender':bbox,'pivotBlender':[0,0,0],'axes':'Blender Z-up; GLB Y-up','unitMeters':1,'collision':'decoracao; nao aplicar regra de obstaculo sem Claude','loop':{'name':'AmbientLoop','fps':24,'frames':[1,49],'durationSeconds':2,'gltf':True},'note':note,'integration':'pendente_claude'})
    r.location.x=(len(manifest)-3)*1.18

# Apresentacao separada dos modelos exportados; nenhum piso/luz/camera entra no GLB.
floor=box('chao_preview',None,scene.collection,(0,0,-.065),(7,3,.1),'ink')
bpy.ops.object.camera_add(location=(4,-7,5.5));camera=bpy.context.object;camera.name='camera_preview';camera.rotation_euler=(Vector((0,0,.55))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=7;scene.camera=camera
for name,pos,energy,size in [('luz_principal',(1,-3,6),1100,5),('luz_preenchimento',(-4,1,3),550,4)]:
    bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.name=name;o.data.energy=energy;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(Vector((0,0,.5))-o.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new('mundo_preview');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.13,.15,.22,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.4
scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=960;scene.render.resolution_y=540;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/'props-cinco-temas.blend'))
(ROOT/'props.visual.json').write_text(json.dumps({'version':1,'project':'Arena Tatica','generationCostPixelLab':0,'status':'lote_gerado; aguardando_validacao_glb_e_avaliacao_astra','source':'fontes/props-cinco-temas.blend','producer':'construir.py','assets':manifest},ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
for f in range(1,49,6):
    scene.frame_set(f);scene.render.filepath=str(ROOT/'previews'/f'{f:02d}.png');bpy.ops.render.render(write_still=True)
print('LOTE_PRONTO: cinco GLB com AmbientLoop e oito quadros de preview.')
