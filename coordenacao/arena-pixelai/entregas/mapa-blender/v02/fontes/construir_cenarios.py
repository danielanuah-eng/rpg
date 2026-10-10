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
THEME = sys.argv[sys.argv.index('--theme')+1] if '--theme' in sys.argv else 'bosque'
LABEL = {'bosque':'Bosque','pedreira':'Montanha rochosa','deserto':'Deserto','neve':'Neve','vulcao':'Vulcão'}[THEME]
OUT = ROOT / 'mapa' / THEME
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
    material_key = 'lava_emissivo' if THEME=='vulcao' and key=='agua_emissivo' else key
    m = bpy.data.materials.new(THEME + '_' + material_key)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Roughness'].default_value = .9
    p.inputs['Specular IOR Level'].default_value = .07
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

def ico(name, loc, scale, material, seed=0, irregular=.1, wind=0, subdivisions=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions, radius=1, location=loc)
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
    obj.name = THEME + '_' + name
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
    obj['cenario'] = LABEL
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
        'cenario': LABEL, 'peca': name,
        'arquivo': 'mapa/'+THEME+'/' + name + '.fbx',
        'glb': 'mapa/'+THEME+'/' + name + '.glb',
        'textura': ['mapa/'+THEME+'/' + m.name + '.png' for m in materials
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

def lush_oak(small=False):
    h=.72 if small else 1
    beam('tronco',(0,0,0),(.04,.015,1.14*h),.14,.085,'casca',9)
    for i in range(5):
        a=i*math.tau/5+.2
        beam('raiz',(0,0,.11),(math.cos(a)*.28,math.sin(a)*.28,.015),.055,.012,'casca',5)
    for i in range(6):
        a=i*math.tau/6
        beam('galho',(0,0,.68*h),(math.cos(a)*.4*h,math.sin(a)*.4*h,1.25*h),.065,.025,'casca',6)
        ico('copa_frondosa',(math.cos(a)*.31*h,math.sin(a)*.31*h,1.27*h),
            (.48*h,.45*h,.48*h),'folhagem',100+i,.07,wind=2.2,subdivisions=2)
    for i,(x,y,z) in enumerate([(-.22,.05,1.62),(.22,-.08,1.61),(0,.02,1.77)]):
        ico('copa_cheia',(x*h,y*h,z*h),(.43*h,.43*h,.4*h),'folhagem',150+i,.06,wind=2.2,subdivisions=2)
    finish('arvore_jovem' if small else 'arvore','' if small else 'terreno:Arvore',True,
           'Carvalho frondoso, copa cheia e baixa, nove volumes com mais folhas; modelo próprio do Bosque.')

def pine(small=False, snow=False):
    h=.74 if small else 1
    lean=.11 if not snow else .025
    beam('tronco',(0,0,0),(lean,0,1.97*h),.095,.022,'casca',8)
    levels=[(.53,.60,.72),(1.00,.46,.66),(1.42,.33,.55),(1.77,.19,.39)]
    if snow: levels=[(.45,.49,.59),(.80,.42,.58),(1.14,.34,.52),(1.47,.26,.48),(1.78,.17,.4)]
    for i,(z,r,depth) in enumerate(levels):
        x=lean*(z/2)
        beam('copa_pinheiro',(x,0,z*h),(x+lean*.1,0,(z+depth)*h),r*h,.005,'folhagem',11 if snow else 9,wind=2.2)
        if snow:
            for j in range(4):
                a=j*math.tau/4+i*.6
                ico('neve_nos_ramos',(x+math.cos(a)*r*.49*h,math.sin(a)*r*.49*h,(z+depth*.25)*h),
                    (r*.3*h,r*.23*h,.065*h),'neve',i*7+j,.04,wind=2.2)
    finish('arvore_jovem' if small else 'arvore','' if small else 'terreno:Arvore',True,
           'Abeto nevado com cinco níveis e neve volumétrica.' if snow else 'Pinheiro da pedreira com quatro níveis e tronco ligeiramente inclinado.')

def snowy_fir(small=False):
    h=.74 if small else 1
    beam('tronco_abeto',(0,0,0),(.02,0,2.09*h),.09,.015,'casca',8)
    for i in range(4):
        a=i*math.tau/4+.3
        beam('raiz_abeto',(0,0,.07),(math.cos(a)*.18,math.sin(a)*.18,.013),.038,.011,'casca',5)
    # Separate drooping fans instead of the quarry's stack of solid cones.
    for tier,(z,r) in enumerate([(.61,.72),(1.04,.58),(1.45,.43),(1.80,.26)]):
        for j in range(7):
            a=j*math.tau/7+tier*.47
            direction=Vector((math.cos(a),math.sin(a),0))
            side=Vector((-math.sin(a),math.cos(a),0))
            def p(distance,width,height):
                return (direction*distance+side*width+Vector((.02,0,height)))*h
            leaf=mesh_part('ramo_aberto',
                [p(.06,0,z+.13),p(r*.43,r*.22,z+.035),p(r,0,z-.10),
                 p(r*.43,-r*.22,z+.035),p(r*.43,0,z+.11),p(r*.40,0,z-.035)],
                [(0,1,4),(1,2,4),(2,3,4),(3,0,4),(0,5,1),(1,5,2),(2,5,3),(3,5,0)],'folhagem')
            for k,w in enumerate([0,.55,1,.55,.6,.4]):
                leaf.data.color_attributes['vento'].data[k].color=(w,0,0,1)
            snow=mesh_part('manto_neve',
                [p(r*.18,0,z+.15),p(r*.44,r*.17,z+.07),p(r*.88,0,z-.065),
                 p(r*.44,-r*.17,z+.07),p(r*.44,0,z+.15)],
                [(0,1,4),(1,2,4),(2,3,4),(3,0,4)],'neve')
            for k,w in enumerate([0,.55,1,.55,.6]):
                snow.data.color_attributes['vento'].data[k].color=(w,0,0,1)
            beam('galho_abeto',p(.02,0,z+.08),p(r*.77,0,z-.065),.026*h,.009*h,'casca',5)
    beam('ponteiro_abeto',(.02,0,1.88*h),(.02,0,2.18*h),.18*h,.002,'folhagem',7,wind=2.2)
    finish('arvore_jovem' if small else 'arvore','' if small else 'terreno:Arvore',True,
           'Abeto nevado exclusivo: 28 ramos abertos e pendentes, mantos de neve geométricos e ponteiro; silhueta distinta do pinheiro cônico da Pedreira.')

def cactus(small=False):
    h=.73 if small else 1
    beam('cacto',(0,0,0),(0,0,1.77*h),.13,.115,'folhagem',10)
    ico('topo',(0,0,1.77*h),(.116,.116,.13),'folhagem',2,.01)
    for i,(x,z,height) in enumerate([(-.35,.65,.52),(.36,.97,.52)]):
        beam('braco',(0,0,z*h),(x,0,z*h),.085,.078,'folhagem',8)
        beam('braco',(x,0,z*h),(x,0,(z+height)*h),.078,.06,'folhagem',8)
        ico('ponta',(x,0,(z+height)*h),(.061,.061,.065),'folhagem',i,0)
    finish('arvore_jovem' if small else 'arvore','' if small else 'terreno:Arvore',False,
           'Cacto saguaro do Deserto; silhueta exclusiva, braços eretos e base de uma casa.')

def palm():
    for i in range(5):
        beam('tronco_curvo',(i*.027,0,i*.31),((i+1)*.027,0,(i+1)*.31),.09-i*.008,.082-i*.008,'casca',8)
    center=Vector((.135,0,1.56))
    for i in range(9):
        a=i*math.tau/9
        direction=Vector((math.cos(a),math.sin(a),0))
        side=Vector((-math.sin(a),math.cos(a),0))*.13
        middle=center+direction*.40+Vector((0,0,.24))
        tip=center+direction*.76+Vector((0,0,-.12))
        leaf=mesh_part('folha_palmeira',[center,middle+side,tip,middle-side,middle+Vector((0,0,.025))],
                       [(0,1,4),(1,2,4),(2,3,4),(3,0,4)],'folhagem')
        colors=leaf.data.color_attributes['vento']
        for j,w in enumerate([0,.6,1,.6,.5]): colors.data[j].color=(w,0,0,1)
    finish('arvore_palmeira','',True,'Palmeira de oásis para a ambientação do Deserto; não substitui automaticamente o cacto do tabuleiro.')

def burned(small=False):
    h=.75 if small else 1
    joints=[(0,0,0),(.045,0,.66*h),(-.055,.045,1.29*h),(.13,.02,1.92*h)]
    for i in range(3): beam('tronco_carbonizado',joints[i],joints[i+1],.15-i*.032,.12-i*.032,'casca',7)
    for i in range(5):
        a=i*2.3
        z=(.73+i*.18)*h
        mid=(math.cos(a)*.36,math.sin(a)*.36,z+.19*h)
        tip=(math.cos(a)*.49,math.sin(a)*.49,z+.46*h)
        beam('galho_seco',(0,0,z),mid,.057,.029,'casca',6,wind=2.2)
        beam('galho_seco',mid,tip,.029,.009,'casca',5,wind=2.2)
        ico('brasa',tip,(.018,.018,.025),'brasa_emissivo',i,0)
    finish('arvore_jovem' if small else 'arvore','' if small else 'terreno:Arvore',True,
           'Árvore carbonizada do Vulcão, tronco torto, ramos desnudos e pontas com brasas discretas.')

def themed_trees():
    for small in [False,True]:
        if THEME=='bosque': lush_oak(small)
        elif THEME=='pedreira': pine(small)
        elif THEME=='neve': snowy_fir(small)
        elif THEME=='deserto': cactus(small)
        else: burned(small)
    if THEME=='deserto': palm()

def ground_details():
    for i in range(5):
        a=i*2.4
        ico('seixo',(math.cos(a)*.18,math.sin(a)*.13,.025),(.05+.01*(i%2),.04,.025),'pedra',i,.1)
    finish('enfeite_seixos','',note='Detalhe baixo, sem colisão, para distribuição também no interior.')
    for i in range(7):
        a=i*2.4
        beam('capim',(math.cos(a)*.08,math.sin(a)*.08,0),
             (math.cos(a)*.14,math.sin(a)*.14,.12+(i%3)*.025),.018,.001,'folhagem',4,wind=.2)
    finish('enfeite_capim','',True,'Tufo baixo decorativo, sem colisão; espalhar entre áreas livres.')
    for i in range(3):
        beam('raiz_no_solo',(-.17+i*.09,-.14,.016),(.16-i*.04,.17,.022),.019,.01,'casca',5)
    finish('enfeite_raiz_chao','',note='Raízes baixas decorativas, não representam obstáculo.')

def middle_landmarks():
    ico('pedra_baixa',(0,0,.10),(.24,.21,.115),'pedra',17,.07)
    finish('enfeite_pedra_baixa','',note='Pedra baixa de ambientação, sem colisão; usar grupos também no interior, longe das bordas.')
    count=12
    verts=[(math.cos(i*math.tau/count)*(.70+.04*math.sin(i*2)),math.sin(i*math.tau/count)*.47,.035) for i in range(count)]
    mesh_part('lago',verts,[tuple(range(count))],'agua_emissivo')
    for i in range(9):
        a=i*math.tau/9
        ico('margem',(math.cos(a)*.72,math.sin(a)*.49,.045),(.08,.07,.045),
            'neve' if THEME=='neve' else 'pedra',20+i,.06)
    for j in range(2):
        for i in range(3):
            beam('ondinha',(-.25+i*.15,-.1+j*.21+.015*math.sin(i),.042),
                 (-.10+i*.15,-.1+j*.21+.015*math.sin(i+1),.042),.008,.008,'creme',4)
    finish('enfeite_lago','',note='Lagoa rasa decorativa para o INTERIOR do mapa; material de água/gelo/lava separado. Sem nova regra ou colisão definida pela arte.')

def ground_coin():
    beam('moeda_deitada',(0,0,0),(0,0,.028),.12,.12,'ouro',20)
    n=20
    verts=[(math.cos(i*math.tau/n)*r,math.sin(i*math.tau/n)*r,.029) for r in [.073,.101] for i in range(n)]
    faces=[(i,(i+1)%n,n+(i+1)%n,n+i) for i in range(n)]
    mesh_part('aro_gravado',verts,faces,'madeira')
    # Relief stamp, no printed letters: a four-point compass inside the coin.
    verts=[(0,.055,.031),(.015,.015,.031),(.055,0,.031),(.015,-.015,.031),
           (0,-.055,.031),(-.015,-.015,.031),(-.055,0,.031),(-.015,.015,.031)]
    mesh_part('selo_gravado',verts,[tuple(range(8))],'creme')
    finish('moeda_chao','moeda:chao',note='Moeda deitada, pivô na face inferior; apresentar no piso onde a lógica do jogo indicar moedas. Preview só demonstra a distribuição.')

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
    if THEME=='pedreira':
        count=7
        verts=[]
        for z,rx,ry,dx,dy in [(0,.38,.33,0,0),(.60,.30,.25,.045,-.03),(1.02,.18,.15,-.04,.02)]:
            for i in range(count):
                a=i*math.tau/count+.17
                verts.append((dx+math.cos(a)*rx,dy+math.sin(a)*ry,z))
        verts.append((.09,.045,1.3))
        faces=[tuple(reversed(range(count)))]
        for ring in range(2):
            for i in range(count):
                a=ring*count+i; b=ring*count+(i+1)%count
                faces.append((a,b,b+count,a+count))
        for i in range(count): faces.append((2*count+i,2*count+(i+1)%count,3*count))
        mesh_part('afloramento_granito',verts,faces,'pedra')
        ico('fragmento_rochoso',(.20,.06,.15),(.24,.21,.16),'pedra',seed,.08)
        finish(name,'',note='Afloramento angular da Montanha Rochosa, base dentro de uma casa. Decoração modular; integração e colisão ficam com Claude.')
        return
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
    mesh_part('telhado',verts,[(0,1,3,2),(2,3,5,4),(0,2,4),(1,5,3)],'creme' if THEME=='deserto' else 'pedra' if THEME=='vulcao' else 'telha')
    if THEME=='neve':
        mesh_part('neve_telhado',[(x,y,z+.04) for x,y,z in verts],[(0,1,3,2),(2,3,5,4)],'neve')
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
    for i in range(8):
        a=i*math.tau/12+.17
        sq=max(abs(math.cos(a)),abs(math.sin(a)))
        rr=(half+.48)/sq
        x,y=math.cos(a)*rr,math.sin(a)*rr
        merge_copy('enfeite_seixos',(x,y,0),a,.7)
        if i%2==0:
            merge_copy('enfeite_capim',(x,y,0),a,.65)
        else:
            merge_copy('enfeite_cogumelos',(x,y,0),a,.65)
    for i in range(3):
        merge_copy('enfeite_cerca',(-half-.48,-1+i*.92,0),math.pi/2)
    # The shop is deliberately separate: the author now requires it at map center.
    # No large props or lake baked into the rim: all landmarks belong towards the center.
    finish('entorno' if size==12 else 'entorno_'+str(size),'entorno',True,
           note=f'Entorno de {size}×{size} m com vão central livre, sem loja incorporada. Props internos são módulos separados, posicionados pelo jogo.')

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

def setup_preview(layout=0):
    # Hide prototypes in their source collection, not the demonstration instances.
    kit=bpy.data.collections.get('KIT_'+THEME.upper()+'_ORIGINAIS_PIVO_ZERO')
    if kit is None:
        kit=bpy.data.collections.new('KIT_'+THEME.upper()+'_ORIGINAIS_PIVO_ZERO')
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
    old_demo=bpy.data.collections.get('PREVIA_TABULEIRO_12x12_APENAS_EXEMPLO')
    if old_demo:
        for old_obj in list(old_demo.objects): bpy.data.objects.remove(old_obj,do_unlink=True)
        bpy.data.collections.remove(old_demo)
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
            if THEME=='pedreira' and layout==0:
                h=3 if (2<=x<=4 and 2<=y<=4) or (8<=x<=9 and 2<=y<=3) else 2 if (1<=x<=4 and 1<=y<=6) or (7<=x<=9 and 1<=y<=4) else 1 if x<5 or y<4 else 0
            if layout==1:
                h=2 if x<4 and 3<y<8 else 1 if (x>7 and y<5) or (3<=x<=6 and y>8) else 0
                if x==2 and y==5: h=3
            if layout==2:
                h=2 if x>7 and 3<=y<=8 else 1 if x>=5 else 0
            zone=x in [5,6] and y in [5,6]
            if zone: h=1
            heights[x,y]=h
            pos=(x-5.5,y-5.5,h*.45)
            for level in range(h):
                copy_piece('coluna_a' if (x+y+level)%2==0 else 'coluna_b',(pos[0],pos[1],level*.45))
            copy_piece('topo_zona' if zone else 'topo_a' if (x+y)%2==0 else 'topo_b',pos,
                       0 if zone else ((x*3+y)%4)*math.pi/2)
    terrain=[(3,3,'arvore'),(8,3,'arvore'),(2,7,'arvore'),(8,8,'arvore'),(4,9,'arvore_jovem'),
             (3,6,'arbusto'),(7,7,'arbusto'),(6,2,'arbusto'),(3,5,'arbusto'),(2,2,'arbusto'),
             (4,3,'caixa'),(7,4,'caixa'),(5,8,'caixa'),(9,9,'caixa_rachada')]
    if layout==1:
        terrain=[(2,3,'arvore'),(4,8,'arvore'),(8,4,'arvore'),(7,9,'arvore'),(3,7,'arvore_jovem'),
                 (4,4,'arbusto'),(6,3,'arbusto'),(3,8,'arbusto'),(9,3,'arbusto'),
                 (3,5,'caixa'),(7,4,'caixa'),(5,9,'caixa'),(9,8,'caixa_rachada')]
    if layout==2:
        terrain=[(3,4,'arvore'),(7,3,'arvore'),(8,8,'arvore'),(2,8,'arvore'),(4,2,'arvore_jovem'),
                 (4,7,'arbusto'),(7,7,'arbusto'),(8,4,'arbusto'),(3,9,'arbusto'),
                 (4,3,'caixa'),(7,4,'caixa'),(5,8,'caixa'),(9,6,'caixa_rachada')]
    pool_cell=[(8,6),(8,7),(3,7)][layout]
    reserved_pool={(x,y) for x in range(pool_cell[0]-1,pool_cell[0]+2) for y in range(pool_cell[1]-1,pool_cell[1]+2)}
    terrain=[t for t in terrain if t[:2] not in reserved_pool]
    for x,y,name in terrain:
        copy_piece(name,(x-5.5,y-5.5,heights[x,y]*.45+.05), (x+y)*.7 if name.startswith('arvore') else 0)
    copy_piece('loja_mascate',(0,0,.5))
    sign_cell=(5,7)
    copy_piece('enfeite_placa',(sign_cell[0]-5.5,sign_cell[1]-5.5,heights[sign_cell]*.45+.05),.2)
    lake_pos=(pool_cell[0]-5.5,pool_cell[1]-5.5,heights[pool_cell]*.45+.05)
    assert len({heights[x,y] for x,y in reserved_pool})==1,'Lago deve ter terreno nivelado por baixo'
    copy_piece('enfeite_lago',lake_pos)
    used={(x,y) for x,y,_ in terrain}|reserved_pool|{(5,5),(5,6),(6,5),(6,6),sign_cell}
    landmark_cells=[]
    def place_free(name,candidates,rotation=0,scale=1):
        cell=next((c for c in candidates if c not in used),None)
        assert cell is not None,('Sem casa livre para',name)
        used.add(cell)
        x,y=cell
        pos=(x-5.5,y-5.5,heights[cell]*.45+.05)
        copy_piece(name,pos,rotation,scale)
        landmark_cells.append({'peca':name,'casaExemplo':list(cell),'posicaoBlender':list(pos)})
        return cell
    for i,c in enumerate([(2,4),(7,2),(8,7),(4,8)]):
        place_free('enfeite_pedra_baixa',[c,(4,6),(6,8),(3,4),(8,2),(7,9)],i+.4)
    place_free('enfeite_cristal',[(7,8),(6,8),(7,9),(4,6),(3,4)],.3,.8)
    place_free('enfeite_fogueira',[(3,7),(4,7),(4,6),(6,8),(7,9)])
    if THEME=='deserto':
        place_free('arvore_palmeira',[(8,4),(4,8),(3,6),(7,9)],.6)
    if THEME=='pedreira':
        for i,cell in enumerate([(2,5),(3,4),(4,4),(6,8),(8,2),(9,8)]):
            place_free('pedra',[cell,(4,5),(7,9),(9,3),(2,8)],i*.9,.85+(i%3)*.07)
    # A reproducible EXAMPLE only: decorative low props spread across playable areas.
    # The game must choose free cells from its own MapGen output, not copy this map.
    scatter=[]
    coin_cells=[(4,5),(7,5),(5,4),(6,7),(3,4),(8,9),(4,8),(7,3),(2,6),(6,9),(9,4),(5,7)]
    candidates=[(1,3),(2,5),(3,8),(4,6),(7,6),(8,5),(9,3),(6,9),(3,4),(7,8),
                (8,2),(2,9),(9,7),(4,2),(6,3),(3,7),(8,9),(9,5),(5,3),(7,9),(5,7)]
    for i,(x,y) in enumerate(candidates):
        if (x,y) in used or (x,y) in coin_cells: continue
        names=['enfeite_seixos','enfeite_capim','enfeite_raiz_chao','enfeite_flores','enfeite_cogumelos']
        name=names[i%len(names)]
        if THEME in ['pedreira','vulcao'] and name=='enfeite_flores': name='enfeite_seixos'
        px,py=x-5.5+.10*math.sin(i),y-5.5+.10*math.cos(i)
        pz=heights[x,y]*.45+.05
        copy_piece(name,(px,py,pz),i*.9)
        scatter.append({'peca':name,'casaExemplo':[x,y],'posicaoBlender':[px,py,pz],'colisao':False})
    coins=[]
    for i,(x,y) in enumerate(coin_cells):
        if (x,y) in used or (x in [5,6] and y in [5,6]): continue
        pos=(x-5.5,y-5.5,heights[x,y]*.45+.051)
        copy_piece('moeda_chao',pos,i*.73)
        if i%3==0: copy_piece('moeda_chao',(pos[0]+.19,pos[1]-.16,pos[2]),i*.73+.2)
        coins.append({'casaExemplo':[x,y],'posicaoBlender':list(pos)})
    bpy.context.view_layer.update()
    for obj in list(demo.objects):
        if obj.type=='MESH' and obj.get('peca') not in ['entorno','entorno_16','entorno_18','topo_a','topo_b','topo_zona','coluna_a','coluna_b','beira_reta','beira_canto']:
            for corner in obj.bound_box:
                point=obj.matrix_world@Vector(corner)
                assert abs(point.x)<=5.5 and abs(point.y)<=5.5,(obj.name,'sem margem na ilha')
    suffix='' if layout==0 else '-versao-'+str(layout+1).zfill(2)
    (ROOT/'manifestos'/(THEME+suffix+'-distribuicao.json')).write_text(json.dumps({
        'cenario':LABEL,'estado':'especificacao_visual_para_Claude','loja':{
            'posicaoBlender':[0,0,.5],'posicaoUnity':[0,.5,0],
            'independenteDoTemaETamanho':True,'footprintMetros':[2,2],
            'regrasColisaoZonaCentral':'Claude deve conciliar centro da loja, zona central e casas acessíveis; esta entrega visual não altera MapGen.'},
        'pontosPrincipais':{'lago':{'peca':'enfeite_lago','posicaoBlender':list(lake_pos)},
                           'pedras':[p['posicaoBlender'] for p in landmark_cells if p['peca']=='enfeite_pedra_baixa'],
                           'outros':landmark_cells,
                           'arvores':[[x-5.5,y-5.5,heights[x,y]*.45+.05] for x,y,n in terrain if n.startswith('arvore')]},
        'moedasChao':{'modelo':'moeda_chao','deitadas':True,'estado':'exemplo visual; instanciar onde o jogo indicar moedas','exemplo12x12':coins},
        'decoracao':{'distribuicao':'Loja, lago, pedras e árvores distribuídos pelo INTERIOR; entorno sem grandes props na extremidade.',
                    'somenteCasasLivres':True,'evitarLojaEPontosDeInteracao':True,
                    'margemSegura':'Grande prop: no mínimo 0,5 casa entre bounds da peça e contorno da ilha; conferir suporte por baixo da base.',
                    'naoAplicarColisaoAEnfeitesBaixos':True,'exemplo12x12':scatter}},ensure_ascii=False,indent=2),encoding='utf-8')
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
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/(THEME+suffix+'.blend')))
    scene.render.filepath=str(PREV/(THEME+'-tabuleiro-12x12.png' if layout==0 else THEME+suffix+'.png'))
    bpy.ops.render.render(write_still=True)
    print('RENDER_OK',THEME+'-tabuleiro-12x12',flush=True)
    if layout!=0: return
    cam.data.ortho_scale=10.2
    cam.location=(13,13,13)
    aim(cam,(1,0,.7))
    scene.render.resolution_x=1400
    scene.render.resolution_y=1100
    scene.render.filepath=str(PREV/(THEME+'-detalhe.png'))
    bpy.ops.render.render(write_still=True)
    print('RENDER_OK',THEME+'-detalhe',flush=True)

def main():
    if '--preview-only' in sys.argv:
        bpy.ops.wm.open_mainfile(filepath=str(ROOT/'fontes'/(THEME+'.blend')))
        existing=json.loads((ROOT/'manifestos'/(THEME+'.json')).read_text(encoding='utf-8'))
        for entry in existing['pecas']:
            PIECES[entry['peca']]=bpy.data.objects[THEME+'_'+entry['peca']]
        if '--layout' in sys.argv:
            setup_preview(int(sys.argv[sys.argv.index('--layout')+1]))
        else:
            setup_preview()
            if THEME=='bosque':
                setup_preview(1)
                setup_preview(2)
        return
    clean()
    for key,col in [('grama_a',(.30,.43,.20)),('grama_b',(.32,.45,.21)),('zona',(.40,.53,.45)),
                    ('terra',(.27,.20,.13)),('casca',(.35,.20,.12)),('folhagem',(.23,.41,.18)),
                    ('madeira',(.52,.34,.18)),('pedra',(.37,.43,.44))]:
        mat(key,col,tex=True)
    for key,col in [('ferro',(.19,.24,.24)),('fruta',(.63,.19,.16)),('sombra',(.08,.10,.07)),
                    ('creme',(.80,.75,.59)),('flor',(.57,.34,.65)),('flor_clara',(.92,.83,.54)),
                    ('ouro',(.78,.58,.21)),('telha',(.51,.19,.14)),('vinho',(.44,.12,.18))]:
        mat(key,col)
    mat('neve',(.77,.86,.91))
    mat('brasa_emissivo',(.65,.12,.025),emission=.25)
    for key,col,e in [('runa_emissivo',(.23,.64,.56),.4),('cristal_emissivo',(.25,.64,.59),.35),
                      ('agua_emissivo',(.16,.43,.46),.15),('lanterna_emissivo',(1,.63,.16),.6)]:
        if THEME=='vulcao' and key=='agua_emissivo': col=(.9,.19,.025); e=1.25
        if THEME=='neve' and key=='agua_emissivo': col=(.36,.58,.7)
        mat(key,col,emission=e)
    grass_top('topo_a','grama_a')
    grass_top('topo_b','grama_b')
    grass_top('topo_zona','zona')
    column('coluna_a',1)
    column('coluna_b',5)
    border()
    border(True)
    shrub()
    themed_trees()
    for state in ['inteira','rachada','quebrada']:
        crate(state)
    rock()
    fence()
    fence(True)
    flowers()
    mushrooms()
    ground_details()
    middle_landmarks()
    ground_coin()
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
    manifest={'versao':2,'tarefa':'011','data':'2026-10-10','estado':'revisao_entregue_para_avaliacao',
        'avaliacaoAutor':'v1_aprovada_com_ajustes; v2_pendente','integracaoClaude':'pendente','cenario':LABEL,
        'unidadeMetros':1,'alturaPorNivel':.45,'eixosOrigem':'Blender Z-up, planta XY',
        'eixosExportacao':'Y-up, FBX forward -Z; conversão de (x,y,z) Blender para (x,z,-y)',
        'texturas':{'origem':'PixelLab create_tiles_pro / Map','id':json.loads((ROOT/'fontes'/(THEME+'-texturas.json')).read_text(encoding='utf-8-sig'))['id'],
                    'filtro':'Point/Closest','mipmaps':False,'wrap':'Repeat','pixelsPorCasa':64},
        'vento':{'atributo':'vento','fbx':'COLOR_0','glb':'COLOR_1','glbColor0':'branco neutro','canal':'R','base':0,'pontas':1,
                 'shader':'Claude integra; entrega contém pesos, não shader executável'},
        'animacao':'Materiais emissivos e fx_fogo são pontos de integração. Não contém animação Unity pronta.',
        'fonte':'fontes/'+THEME+'.blend','script':'fontes/construir_cenarios.py',
        'previews':['previews/'+THEME+'-tabuleiro-12x12.png','previews/'+THEME+'-detalhe.png'],
        'revisaoAutor':['Loja no meio em todos os mapas.','Props também distribuídos pelo interior.','Árvores com silhueta própria de cada tema; Bosque mais frondoso.'],
        'moedasChao':'Modelo moeda_chao deitado sobre o piso; prévia com distribuição de exemplo.',
        'versoesBosque':[{'id':'v01','nome':'Clareira e colina','fonte':'fontes/bosque.blend','preview':'previews/bosque-tabuleiro-12x12.png'},
                         {'id':'v02','nome':'Trilhas cruzadas','fonte':'fontes/bosque-versao-02.blend','preview':'previews/bosque-versao-02.png'},
                         {'id':'v03','nome':'Terraços e lagoa','fonte':'fontes/bosque-versao-03.blend','preview':'previews/bosque-versao-03.png'}] if THEME=='bosque' else [],
        'divergenciasBriefing':['Código atual usa 12/16/18 casas; entorno e variantes entorno_16/entorno_18 entregues sem mudar regras.'],
        'pecas':MANIFEST}
    for entry in MANIFEST:
        entry['sha256']={kind:hashlib.sha256((ROOT/entry[kind]).read_bytes()).hexdigest() for kind in ['arquivo','glb']}
    (ROOT/'manifestos'/(THEME+'.json')).write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    print('KIT_OK',len(MANIFEST),'pecas',flush=True)
    if '--no-render' not in sys.argv:
        setup_preview()
        if THEME=='bosque':
            setup_preview(1)
            setup_preview(2)

if __name__=='__main__':
    main()
