"""Kit modular Bosque, tarefa 011. Executar com Blender 5.2 --background --python.

Arte e exportação somente; não modifica o jogo. Seeds fixas. Z-up no .blend,
FBX -Z forward/Y up e GLB Y-up. Texturas autorais PixelLab ficam ao lado do FBX.
"""
import bpy
import json
import math
import random
import hashlib
import sys
import struct
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'mapa' / 'bosque'
PREV = ROOT / 'previews'
OUT.mkdir(parents=True, exist_ok=True)
PREV.mkdir(parents=True, exist_ok=True)
RNG = random.Random(10102026)
MATS = {}
PIECES = {}
MANIFEST = []
PARTS = []

def clean():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for data in list(bpy.data.collections):
        if data.name != 'Collection':
            bpy.data.collections.remove(data)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1.0

def mat(key, color, tex=None, emission=0):
    m = bpy.data.materials.new('bosque_' + key)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Roughness'].default_value = .9
    if emission:
        p.inputs['Emission Color'].default_value = (*color, 1)
        p.inputs['Emission Strength'].default_value = emission
    if tex:
        path = OUT / (m.name + '.png')
        if not path.exists():
            raise FileNotFoundError('Textura PixelLab obrigatória: ' + str(path))
        img = bpy.data.images.load(str(path), check_existing=True)
        node = m.node_tree.nodes.new('ShaderNodeTexImage')
        node.image = img
        node.interpolation = 'Closest'
        node.extension = 'REPEAT'
        m.node_tree.links.new(node.outputs['Color'], p.inputs['Base Color'])
    MATS[key] = m
    return m

def uv_wind(obj, material, wind=0):
    mesh = obj.data
    obj.data.materials.append(MATS[material])
    uv = mesh.uv_layers.new(name='UVMap') if not mesh.uv_layers else mesh.uv_layers[0]
    for poly in mesh.polygons:
        axis = max(range(3), key=lambda i: abs(poly.normal[i]))
        for li in poly.loop_indices:
            co = mesh.vertices[mesh.loops[li].vertex_index].co
            if axis == 2:
                uv.data[li].uv = (co.x + .5, co.y + .5)
            elif axis == 1:
                uv.data[li].uv = (co.x + .5, co.z)
            else:
                uv.data[li].uv = (co.y + .5, co.z)
    attr = mesh.color_attributes.new(name='vento', type='FLOAT_COLOR', domain='POINT')
    mesh.color_attributes.active_color = attr
    tip = max(.09, max(v.co.z for v in mesh.vertices))
    for v in mesh.vertices:
        value = 0 if not wind else min(1, max(0, (v.co.z - .08) / (tip - .08)))
        attr.data[v.index].color = (value, 0, 0, 1)
    for p in mesh.polygons:
        p.use_smooth = False
    PARTS.append(obj)
    return obj

def box(name, loc, scale, material, bevel=.025, rotation=0, wind=0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = bpy.context.object
    o.name = name
    o.scale = scale
    o.rotation_euler.z = rotation
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        mod = o.modifiers.new('chanfro', 'BEVEL')
        mod.width = min(bevel, min(scale) / 3)
        mod.segments = 1
        bpy.context.view_layer.objects.active = o
        bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=False)
    return uv_wind(o, material, wind)

def ico(name, loc, scale, material, seed=0, irregular=.1, wind=0):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1, location=loc)
    o = bpy.context.object
    o.name = name
    r = random.Random(seed)
    for v in o.data.vertices:
        v.co *= 1 + r.uniform(-irregular, irregular)
    o.scale = scale
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return uv_wind(o, material, wind)

def beam(name, a, b, r1, r2, material, vertices=7, wind=0):
    a, b = Vector(a), Vector(b)
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r1, radius2=r2,
                                  depth=(b-a).length, location=(a+b)/2)
    o = bpy.context.object
    o.name = name
    o.rotation_euler = (b-a).to_track_quat('Z', 'Y').to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return uv_wind(o, material, wind)

def mesh_part(name, verts, faces, material):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    o = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(o)
    return uv_wind(o, material)

def empty(name, pos):
    o = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(o)
    o.location = pos
    o.empty_display_type = 'PLAIN_AXES'
    o.empty_display_size = .15
    PARTS.append(o)
    return o

def finish(name, replaces='', wind=False, note=''):
    global PARTS
    meshes = [p for p in PARTS if p.type == 'MESH']
    empties = [p for p in PARTS if p.type == 'EMPTY']
    bpy.ops.object.select_all(action='DESELECT')
    for p in meshes:
        p.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = 'bosque_' + name
    if not name.startswith(('beira', 'entorno')):
        floor = min(v.co.z for v in obj.data.vertices)
        if abs(floor) > .00001:
            for v in obj.data.vertices:
                v.co.z -= floor
            for e in empties:
                e.location.z -= floor
    if name in ['beira_canto', 'enfeite_cerca_canto']:
        for axis in [0,1]:
            center = (min(v.co[axis] for v in obj.data.vertices) + max(v.co[axis] for v in obj.data.vertices)) / 2
            for v in obj.data.vertices:
                v.co[axis] -= center
    if name == 'caixa_quebrada':
        span = max(max(v.co[a] for v in obj.data.vertices) - min(v.co[a] for v in obj.data.vertices) for a in [0,1])
        for v in obj.data.vertices:
            v.co.x *= .85/span
            v.co.y *= .85/span
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    bpy.context.view_layer.update()
    obj['peca'] = name
    obj['cenario'] = 'Bosque'
    obj['pivo'] = 'centro da base; Z=0'
    obj['substitui'] = replaces
    obj['vento'] = wind
    for e in empties:
        e.parent = obj
    PIECES[name] = obj
    obj.data.calc_loop_triangles()
    bounds = [Vector(v) for v in obj.bound_box]
    materials = [m for m in obj.data.materials if m]
    entry = {
        'cenario': 'Bosque', 'peca': name,
        'arquivo': 'mapa/bosque/' + name + '.fbx',
        'glb': 'mapa/bosque/' + name + '.glb',
        'textura': ['mapa/bosque/' + m.name + '.png' for m in materials
                    if (OUT / (m.name + '.png')).exists()],
        'materiais': [m.name for m in materials],
        'dimensoes': [round(float(v), 4) for v in obj.dimensions],
        'bounds': {'min': [round(min(v[i] for v in bounds), 4) for i in range(3)],
                   'max': [round(max(v[i] for v in bounds), 4) for i in range(3)]},
        'tris': len(obj.data.loop_triangles), 'vento': 'sim' if wind else 'não',
        'emissivo': 'sim' if any('emissivo' in m.name for m in materials) else 'não',
        'empties': [{'nome': e.name.split('.')[0], 'nomeNoBlend': e.name, 'posicaoBlender': list(e.location)} for e in empties],
        'substitui': replaces, 'pivoBlender': [0, 0, 0], 'observacao': note,
    }
    limit = 40000 if name.startswith('entorno') else 3000
    assert entry['tris'] <= limit, (name, entry['tris'])
    assert all(abs(v - 1) < 1e-5 for v in obj.scale)
    assert obj.location.length < 1e-5 and obj.rotation_euler.to_matrix().is_identity
    MANIFEST.append(entry)
    PARTS = []
    return obj

def grass_top(name, material):
    box(name, (0,0,.025), (1,1,.05), material, .012)
    finish(name, 'piso:zona' if name == 'topo_zona' else 'piso:' + name[-1])

def column(name, seed):
    box('terra', (0,0,.225), (1,1,.45), 'terra', .026)
    # Face fragments stay INSIDE the exact 1x1 footprint.
    for x, y, z in [(.484,.22,.2),(-.24,.484,.31),(.18,-.484,.14),(-.484,-.2,.35)]:
        ico('pedra_embutida', (x*.97,y*.97,z), (.025,.025,.055), 'pedra', seed, .05)
    finish(name, 'coluna', note='Segmento empilhável de 0,45 m; faces de terra, pedra e raízes.')

def shrub(seed=1):
    for i, (x,y,z,s) in enumerate([(-.16,-.05,.28,.21),(.15,.06,.27,.22),
                                  (-.03,.17,.29,.21),(.02,-.13,.34,.22)]):
        ico('folhas', (x,y,z), (s,s,s*.85), 'folhagem', seed+i, .09, wind=.6)
    for x,y in [(-.2,.05),(.15,-.06),(0,.18)]:
        beam('galho', (x*.5,y*.5,0),(x,y,.34),.022,.012,'casca')
    for i in range(4):
        ico('frutinha', (math.cos(i*2.4)*.26,math.sin(i*2.4)*.26,.37),
            (.029,.029,.03),'fruta',i,0)
    finish('arbusto', 'terreno:Arbusto', True)

def oak(seed=2, small=False):
    h = 1 if not small else .72
    beam('tronco',(0,0,0),(.035,.005,1.28*h),.14,.065,'casca',8)
    for i in range(5):
        a = i*math.tau/5 + .4
        beam('raiz',(0,0,.12),(math.cos(a)*.29,math.sin(a)*.29,.025),.055,.018,'casca',5)
    for i in range(4):
        a=i*1.65
        beam('galho',(0,0,.75*h),(math.cos(a)*.36,math.sin(a)*.36,1.5*h),.055,.022,'casca',6)
    for i,(x,y,z,s) in enumerate([(-.35,-.03,1.5,.4),(.34,.05,1.58,.41),
                                 (-.02,-.33,1.57,.39),(.06,.32,1.6,.4),
                                 (.01,0,1.84,.36),(-.23,.2,1.79,.33)]):
        ico('copa',(x*h,y*h,z*h),(s*h,s*h,s*.77*h),'folhagem',seed+i,.08,wind=2.2*h)
    finish('arvore' if not small else 'arvore_jovem', 'terreno:Arvore' if not small else '', True)

def crate(state):
    if state != 'quebrada':
        box('miolo',(0,0,.4),(.78,.78,.8),'madeira',.02)
        for x in [-.397,.397]:
            for y in [-.32,.32]:
                box('cinta',(x,y,.4),(.048,.07,.77),'ferro',.01)
        for z in [.095,.71]:
            box('faixa',(0,0,z),(.82,.82,.055),'ferro',.009)
        box('travessa_tampa',(0,0,.817),(.69,.095,.03),'madeira',.008,rotation=.72)
        ico('selo',(0,.418,.57),(.07,.018,.07),'fruta',2,0)
        if state == 'rachada':
            # Thin dark cracks are geometry, leaving the shared outer footprint unchanged.
            beam('fenda',(-.24,.407,.14),(.03,.407,.47),.008,.008,'sombra',4)
            beam('fenda',(.03,.407,.47),(-.07,.407,.72),.008,.004,'sombra',4)
            beam('fenda',(.03,.407,.47),(.22,.407,.54),.007,.003,'sombra',4)
    else:
        for i in range(6):
            a=i*.81
            box('tabua_quebrada',(math.cos(a)*.17,math.sin(a)*.15,.035+i*.018),
                (.6,.105,.055),'madeira',.014,rotation=a)
        box('fundo',(0,0,.035),(.72,.72,.07),'madeira',.018)
        box('aro_caido',(.08,.07,.12),(.7,.05,.075),'ferro',.008,rotation=-.35)
    finish('caixa' if state == 'inteira' else 'caixa_'+state, 'terreno:Caixa',
           note='Estado só visual; destrutibilidade depende das regras do autor.')

def rock(name='pedra', seed=3):
    ico('rocha',(0,0,.29),(.39,.34,.31),'pedra',seed,.09)
    ico('musgo',(-.08,.06,.52),(.22,.17,.06),'grama_a',seed+1,.1)
    finish(name, '')

def fence(corner=False, broken=False):
    positions=[(-.42,0),(.42,0)] if not corner else [(-.42,0),(.42,0),(.42,.84)]
    for x,y in positions:
        beam('mourao',(x,y,0),(x,y,.65),.052,.04,'madeira',5)
    for z in [.22,.46]:
        box('ripa',(0,0,z),(.88,.055,.09),'madeira',.016)
        if corner:
            box('ripa',(.42,.42,z),(.055,.88,.09),'madeira',.016)
    if broken:
        box('tabua_solteira',(.12,.1,.05),(.42,.065,.07),'madeira',.016,rotation=-.6)
    finish('enfeite_cerca_canto' if corner else 'enfeite_cerca', '')

def flowers():
    for i in range(5):
        x,y=math.cos(i*2.4)*.14,math.sin(i*2.4)*.14
        h=.14+(i%3)*.025
        beam('caule',(x,y,0),(x,y,h),.009,.006,'folhagem',4)
        for j in range(5):
            a=j*math.tau/5
            ico('petala',(x+math.cos(a)*.025,y+math.sin(a)*.025,h),
                (.024,.024,.012),'flor' if i%2 else 'flor_clara',j,0)
        ico('miolo',(x,y,h+.009),(.012,.012,.012),'ouro',i,0)
    finish('enfeite_flores','')

def mushrooms():
    for i,(x,y,h) in enumerate([(-.1,0,.2),(.1,.06,.12),(.06,-.1,.15)]):
        beam('haste',(x,y,0),(x,y,h),.027,.022,'creme',6)
        ico('chapeu',(x,y,h),(.08,.08,.042),'fruta',i,.04)
        for j in range(3):
            ico('pinta',(x+math.cos(j*2.1)*.044,y+math.sin(j*2.1)*.04,h+.033),
                (.014,.014,.008),'creme',j,0)
    finish('enfeite_cogumelos','')

def torch():
    beam('poste',(0,0,0),(0,0,.72),.045,.035,'casca',6)
    box('braseiro',(0,0,.75),(.2,.2,.12),'ferro',.02)
    empty('fx_fogo',(0,0,.84))
    finish('enfeite_tocha','',note='fx_fogo recebe chama em sprite; sem fogo sólido no FBX.')

def firepit():
    for i in range(8):
        a=i*math.tau/8
        ico('pedra',(math.cos(a)*.24,math.sin(a)*.24,.045),(.08,.06,.045),'pedra',i,.1)
    for i in range(3):
        box('lenha',(0,0,.075+i*.035),(.36,.07,.07),'casca',.02,rotation=i*1.1)
    empty('fx_fogo',(0,0,.15))
    finish('enfeite_fogueira','')

def sign():
    beam('poste',(0,0,0),(0,0,1),.044,.033,'casca',7)
    box('placa',(.02,0,.84),(.55,.065,.16),'madeira',.015)
    box('seta',(.07,0,.61),(.42,.065,.12),'madeira',.015,rotation=.08)
    # Coin pictogram, no lettering rasterized into the asset.
    beam('moeda',(.01,-.043,.84),(.01,-.051,.84),.05,.05,'ouro',8)
    finish('enfeite_placa','')

def chest():
    box('base',(0,0,.235),(.66,.44,.47),'madeira',.035)
    for x in [-.25,.25]:
        box('cinta',(x,0,.25),(.045,.48,.48),'ouro',.018)
    box('tampa',(0,0,.51),(.68,.46,.09),'madeira',.02)
    box('fechadura',(0,.245,.35),(.075,.035,.11),'ouro',.012)
    finish('bau_fechado','modo:Tesouro')

def pedestal(name, coin=False):
    box('base',(0,0,.07),(.68,.68,.14),'pedra',.055)
    box('corpo',(0,0,.29),(.43,.43,.44),'pedra',.035)
    box('capitel',(0,0,.56),(.57,.57,.1),'pedra',.03)
    if coin:
        beam('galeao',(0,-.035,.86),(0,.035,.86),.19,.19,'ouro',12)
        finish(name,'modo:Galeoes')
    else:
        beam('runa',(0,.231,.37),(0,.234,.37),.09,.09,'runa_emissivo',6)
        finish(name,'modo:Dominio',note='Runa em material separado para cor e pulso do jogador.')

def crystal():
    for i,(x,y,h) in enumerate([(-.08,0,.43),(.12,.07,.24),(.04,-.12,.29)]):
        beam('cristal',(x,y,0),(x,y,h*.8),.09,.055,'cristal_emissivo',5)
        beam('ponta',(x,y,h*.8),(x,y,h),.055,0,'cristal_emissivo',5)
    finish('enfeite_cristal','')

def border(corner=False):
    box('labio',(0,0,0), (1,.17,.09),'casca',.02)
    for i in range(4):
        x=-.38+i*.24
        beam('raiz',(x,0,.01),(x+.07,.065,-.36-(i%2)*.06),.045,.012,'casca',5)
    if corner:
        box('labio_canto',(.415,.415,0),(.17,1,.09),'casca',.02)
        beam('raiz_canto',(.43,.4,0),(.47,.45,-.4),.065,.01,'casca',6)
    finish('beira_canto' if corner else 'beira_reta','beira',
           note='Pivô de encaixe no topo da borda; raízes pendem abaixo de Z=0.')

def shop():
    # 2x2 shop outside the board, open counter on Blender +Y (towards camera).
    box('base',(0,0,.09),(1.86,1.86,.18),'pedra',.065)
    box('parede_fundo',(0,-.63,.76),(1.65,.12,1.25),'madeira',.025)
    for x in [-.8,.8]:
        box('parede_lateral',(x,-.12,.75),(.12,1.14,1.2),'madeira',.025)
        beam('poste_frente',(x,.68,.18),(x,.68,1.58),.065,.055,'casca',6)
    box('balcao',(0,.7,.7),(1.72,.46,.2),'madeira',.04)
    verts=[(-.96,-.9,1.43),(.96,-.9,1.43),(-.96,0,1.98),(.96,0,1.98),
           (-.96,.95,1.43),(.96,.95,1.43)]
    mesh_part('telhado',verts,[(0,1,3,2),(2,3,5,4),(0,2,4),(1,5,3)],'telha')
    for i in range(7):
        x=-.72+i*.24
        mesh_part('toldo',[(x-.115,.72,1.4),(x+.115,.72,1.4),(x+.115,1,1.29),(x-.115,1,1.29)],
                  [(0,1,2,3)],'creme' if i%2 else 'vinho')
    box('chamine',(-.55,-.35,1.88),(.25,.25,.75),'pedra',.03)
    empty('fx_fumaca',(-.55,-.35,2.27))
    box('placa_loja',(.51,.85,1.05),(.31,.045,.21),'madeira',.02)
    beam('saco_moedas',(.51,.883,1.05),(.51,.893,1.05),.068,.065,'ouro',8)
    box('lanterna',(-.6,.83,1.13),(.11,.13,.2),'ferro',.015)
    box('vidro_lanterna',(-.6,.9,1.13),(.074,.02,.13),'lanterna_emissivo',.005)
    finish('loja_mascate','loja:estrutura',note='Fonte 3D; vendedor continua sprite. Frente +Y Blender = -Z Unity após exportação.')

def copy_piece(name, loc, rot=0, scale=1):
    src=PIECES[name]
    o=src.copy()
    o.data=src.data
    bpy.context.collection.objects.link(o)
    o.location=loc
    o.rotation_euler.z=rot
    o.scale=(scale,)*3
    for e in src.children:
        child=e.copy()
        bpy.context.collection.objects.link(child)
        child.parent=o
    return o

def merge_copy(name, loc, rot=0, scale=1):
    src=PIECES[name]
    o=bpy.data.objects.new(name,src.data.copy())
    bpy.context.collection.objects.link(o)
    o.location=loc
    o.rotation_euler.z=rot
    o.scale=(scale,)*3
    bpy.ops.object.select_all(action='DESELECT')
    o.select_set(True)
    bpy.context.view_layer.objects.active=o
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    PARTS.append(o)
    for e in src.children:
        empty(e.name.split('.')[0],Vector(loc)+e.location*scale)
    return o

def surround(size=12):
    r = random.Random(4400+size)
    half=size/2
    segments=48
    rings=[]
    for ri in range(3):
        ring=[]
        for i in range(segments):
            a=i*math.tau/segments
            sq=max(abs(math.cos(a)),abs(math.sin(a)))
            radius=(half+.10 if ri==0 else (half+2.12)*(1-.09*abs(math.sin(2*a)))+r.uniform(-.3,.3))/sq
            z=-.08 if ri==0 else r.uniform(-.2,.03) if ri==1 else r.uniform(-1.05,-.75)
            ring.append((math.cos(a)*radius,math.sin(a)*radius,z))
        rings.append(ring)
    verts=sum(rings,[])
    faces=[(i,(i+1)%segments,segments+(i+1)%segments,segments+i) for i in range(segments)]
    mesh_part('clareira_anel',verts[:segments*2],faces,'grama_a')
    sidefaces=[(segments+i,segments+(i+1)%segments,segments*2+(i+1)%segments,segments*2+i) for i in range(segments)]
    mesh_part('laterais_ilha',verts,sidefaces,'terra')
    # The entire central footprint is a real open hole, including below Z=0.
    for i in range(12):
        a=i*math.tau/12+.17
        sq=max(abs(math.cos(a)),abs(math.sin(a)))
        rr=(half+1.45)/sq
        x,y=math.cos(a)*rr,math.sin(a)*rr
        foreground=x+y>half*.9
        if foreground:
            merge_copy('pedra',(x,y,-.02),a, .65+r.random()*.3)
            if i%2==0:
                merge_copy('enfeite_flores',(x-.3,y+.3,0),a)
        else:
            merge_copy('arvore',(x,y,-.02),a,1.05+r.random()*.2)
            merge_copy('arbusto',(x+.35,y+.15,-.01),a,1.15)
        merge_copy('enfeite_cogumelos',(x-.33,y-.25,0),a,.85)
    for i in range(4):
        merge_copy('enfeite_cerca',(-half-1.4,-half+.2+i*.92,0),math.pi/2)
    merge_copy('loja_mascate',(-half+.85,-half-1.3,0),-.2)
    merge_copy('enfeite_placa',(-half+2.25,-half-.6,0),.12)
    merge_copy('enfeite_fogueira',(-half-1.25,half-1,0))
    merge_copy('enfeite_cristal',(-half-1.6,-half+.5,0),.3,1.4)
    # Recessed water patch outside the playable square; distinct animated material.
    cx,cy=half+1.15,-half*.45
    verts=[(cx+math.cos(i*math.tau/12)*.68,cy+math.sin(i*math.tau/12)*.43,-.012) for i in range(12)]
    mesh_part('agua',verts,[tuple(range(12))],'agua_emissivo')
    finish('entorno' if size==12 else 'entorno_'+str(size),'entorno',True,
           note=f'Clareira com vão central livre de {size}×{size} m; árvores altas só atrás/laterais; frente baixa para leitura. Não contém tabuleiro.')

def exports():
    for name,obj in PIECES.items():
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        for e in obj.children:
            e.select_set(True)
        renamed=[]
        for e in obj.children:
            desired=e.name.split('.')[0]
            if e.name != desired:
                occupant=bpy.data.objects.get(desired)
                if occupant and occupant != e:
                    renamed.append((occupant,occupant.name))
                    occupant.name='reserva_'+occupant.name
                renamed.append((e,e.name))
                e.name=desired
        bpy.context.view_layer.objects.active=obj
        bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,
            object_types={'MESH','EMPTY'},global_scale=1.0,apply_unit_scale=True,
            use_space_transform=True,bake_space_transform=True,axis_forward='-Z',axis_up='Y',
            mesh_smooth_type='FACE',path_mode='RELATIVE',add_leaf_bones=False,bake_anim=False,
            colors_type='LINEAR',prioritize_active_color=True)
        bpy.ops.export_scene.gltf(filepath=str(OUT/(name+'.glb')),use_selection=True,
            export_format='GLB',export_yup=True,export_materials='EXPORT',export_cameras=False,
            export_lights=False,export_animations=False,export_all_vertex_colors=True)
        # Keep the separate wind channel in COLOR_1; COLOR_0 is the exporter's neutral white.
        # Enforce nearest minification without generated mip levels in generic GLB viewers.
        glb=OUT/(name+'.glb')
        data=glb.read_bytes()
        size=struct.unpack_from('<I',data,12)[0]
        document=json.loads(data[20:20+size])
        for sampler in document.get('samplers',[]):
            sampler.update(magFilter=9728,minFilter=9728,wrapS=10497,wrapT=10497)
        encoded=json.dumps(document,separators=(',',':')).encode('utf-8')
        encoded+=b' '*((-len(encoded))%4)
        tail=data[20+size:]
        glb.write_bytes(struct.pack('<III',0x46546C67,2,20+len(encoded)+len(tail))+
                        struct.pack('<II',len(encoded),0x4E4F534A)+encoded+tail)
        for e,old in reversed(renamed):
            e.name=old
        print('EXPORT_OK',name,flush=True)

def aim(o, point):
    o.rotation_euler=(Vector(point)-o.location).to_track_quat('-Z','Y').to_euler()

def setup_preview():
    # Hide prototypes in their source collection, not the demonstration instances.
    kit=bpy.data.collections.new('KIT_BOSQUE_ORIGINAIS_PIVO_ZERO')
    bpy.context.scene.collection.children.link(kit)
    kit.hide_render=True
    kit.hide_viewport=True
    for o in PIECES.values():
        for c in list(o.users_collection):
            c.objects.unlink(o)
        kit.objects.link(o)
        for e in o.children:
            for c in list(e.users_collection):
                c.objects.unlink(e)
            kit.objects.link(e)
    demo=bpy.data.collections.new('PREVIA_TABULEIRO_12x12_APENAS_EXEMPLO')
    bpy.context.scene.collection.children.link(demo)
    layer=bpy.context.view_layer.layer_collection.children.get(demo.name)
    bpy.context.view_layer.active_layer_collection=layer
    copy_piece('entorno',(0,0,0))
    heights={}
    for x in range(12):
        for y in range(12):
            dist=math.sqrt((x-2.5)**2+(y-2.3)**2)
            h=3 if dist<1.55 else 2 if dist<3.15 else 1 if dist<4.25 or (x<3 and y>8) else 0
            zone=x in [5,6] and y in [5,6]
            if zone: h=1
            heights[x,y]=h
            pos=(x-5.5,y-5.5,h*.45)
            for level in range(h):
                copy_piece('coluna_a' if (x+y+level)%2==0 else 'coluna_b',(pos[0],pos[1],level*.45))
            copy_piece('topo_zona' if zone else 'topo_a' if (x+y)%2==0 else 'topo_b',pos,
                       0 if zone else ((x*3+y)%4)*math.pi/2)
    terrain=[(1,6,'arvore'),(3,9,'arvore'),(7,2,'arvore'),(9,4,'arvore'),
             (2,4,'arbusto'),(4,7,'arbusto'),(8,7,'arbusto'),(8,1,'arbusto'),
             (3,5,'caixa'),(7,8,'caixa'),(10,6,'caixa'),(1,9,'caixa_rachada')]
    for x,y,name in terrain:
        copy_piece(name,(x-5.5,y-5.5,heights[x,y]*.45+.05), (x+y)*.7 if name.startswith('arvore') else 0)
    for i in range(12):
        copy_piece('beira_reta',(i-5.5,6,0))
        copy_piece('beira_reta',(6,i-5.5,0),math.pi/2)
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=32
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1600
    scene.render.resolution_y=1200
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.film_transparent=False
    scene.world.color=(.13,.16,.19)
    scene.world.use_nodes=True
    scene.world.node_tree.nodes.get('Background').inputs['Color'].default_value=(.22,.29,.35,1)
    scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.55
    scene.view_settings.view_transform='Standard'
    scene.view_settings.look='None'
    bpy.ops.object.light_add(type='SUN',location=(-6,-7,12))
    sun=bpy.context.object
    sun.name='LUZ_NOROESTE_PREVIA'
    sun.data.energy=2.0
    sun.data.angle=math.radians(7)
    aim(sun,(0,0,0))
    bpy.ops.object.light_add(type='AREA',location=(5,3,10))
    fill=bpy.context.object
    fill.data.energy=400
    fill.data.shape='DISK'
    fill.data.size=10
    aim(fill,(0,0,0))
    bpy.ops.object.camera_add(location=(19,19,19))
    cam=bpy.context.object
    cam.name='CAMERA_ISOMETRICA_PREVIA'
    aim(cam,(0,0,.25))
    cam.data.type='ORTHO'
    cam.data.ortho_scale=27
    cam.data.lens=50
    scene.camera=cam
    # Save an editable source with materials embedded and assets at native origin.
    for img in bpy.data.images:
        if img.source=='FILE':
            img.pack()
    scene['pedido']='coordenacao/arena-pixelai/tarefas/011-codex-mapa-blender.md'
    scene['status']='piloto para avaliacao; integracao Unity pendente'
    scene['vento']='FBX COLOR_0; GLB COLOR_1 (COLOR_0 branco); atributo vento; R=amplitude'
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type=='VIEW_3D':
                area.spaces.active.region_3d.view_perspective='CAMERA'
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/'bosque-piloto.blend'))
    scene.render.filepath=str(PREV/'bosque-tabuleiro-12x12.png')
    bpy.ops.render.render(write_still=True)
    print('RENDER_OK bosque-tabuleiro-12x12',flush=True)
    cam.data.ortho_scale=10.2
    cam.location=(13,13,13)
    aim(cam,(1,0,.7))
    scene.render.resolution_x=1400
    scene.render.resolution_y=1100
    scene.render.filepath=str(PREV/'bosque-detalhe.png')
    bpy.ops.render.render(write_still=True)
    print('RENDER_OK bosque-detalhe',flush=True)

def main():
    clean()
    for key,col in [('grama_a',(.30,.43,.20)),('grama_b',(.32,.45,.21)),('zona',(.40,.53,.45)),
                    ('terra',(.27,.20,.13)),('casca',(.35,.20,.12)),('folhagem',(.23,.41,.18)),
                    ('madeira',(.52,.34,.18)),('pedra',(.37,.43,.44))]:
        mat(key,col,tex=True)
    for key,col in [('ferro',(.19,.24,.24)),('fruta',(.63,.19,.16)),('sombra',(.08,.10,.07)),
                    ('creme',(.80,.75,.59)),('flor',(.57,.34,.65)),('flor_clara',(.92,.83,.54)),
                    ('ouro',(.78,.58,.21)),('telha',(.51,.19,.14)),('vinho',(.44,.12,.18))]:
        mat(key,col)
    for key,col,e in [('runa_emissivo',(.23,.64,.56),.4),('cristal_emissivo',(.25,.64,.59),.35),
                      ('agua_emissivo',(.16,.43,.46),.15),('lanterna_emissivo',(1,.63,.16),.6)]:
        mat(key,col,emission=e)
    grass_top('topo_a','grama_a')
    grass_top('topo_b','grama_b')
    grass_top('topo_zona','zona')
    column('coluna_a',1)
    column('coluna_b',5)
    border()
    border(True)
    shrub()
    oak()
    oak(small=True)
    for state in ['inteira','rachada','quebrada']:
        crate(state)
    rock()
    fence()
    fence(True)
    flowers()
    mushrooms()
    torch()
    firepit()
    sign()
    chest()
    pedestal('pilar_galeao',True)
    pedestal('totem_dominio')
    crystal()
    shop()
    for size in [12,16,18]:
        surround(size)
    exports()
    manifest={'versao':1,'tarefa':'011','data':'2026-10-10','estado':'piloto_entregue_para_avaliacao',
        'avaliacaoAutor':'pendente','integracaoClaude':'pendente','cenario':'Bosque',
        'unidadeMetros':1,'alturaPorNivel':.45,'eixosOrigem':'Blender Z-up, planta XY',
        'eixosExportacao':'Y-up, FBX forward -Z; conversão de (x,y,z) Blender para (x,z,-y)',
        'texturas':{'origem':'PixelLab create_tiles_pro / Map','id':'969ac24a-abe9-4723-8531-baabde2c70e3',
                    'filtro':'Point/Closest','mipmaps':False,'wrap':'Repeat','pixelsPorCasa':64},
        'vento':{'atributo':'vento','fbx':'COLOR_0','glb':'COLOR_1','glbColor0':'branco neutro','canal':'R','base':0,'pontas':1,
                 'shader':'Claude integra; entrega contém pesos, não shader executável'},
        'animacao':'Materiais emissivos e fx_fogo são pontos de integração. Não contém animação Unity pronta.',
        'fonte':'fontes/bosque-piloto.blend','script':'fontes/construir_bosque.py',
        'previews':['previews/bosque-tabuleiro-12x12.png','previews/bosque-detalhe.png'],
        'divergenciasBriefing':['Código atual usa 12/16/18 casas; entorno e variantes entorno_16/entorno_18 entregues sem mudar regras.'],
        'pecas':MANIFEST}
    for entry in MANIFEST:
        entry['sha256']={kind:hashlib.sha256((ROOT/entry[kind]).read_bytes()).hexdigest() for kind in ['arquivo','glb']}
    (ROOT/'manifesto.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    print('KIT_OK',len(MANIFEST),'pecas',flush=True)
    if '--no-render' not in sys.argv:
        setup_preview()

if __name__=='__main__':
    main()
