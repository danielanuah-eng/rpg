"""Verifica os arquivos efetivamente exportados, não só a cena de origem."""
import bpy
import json
import struct
import math
import hashlib
import sys
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT.parent/'v02'
THEME = sys.argv[sys.argv.index('--theme')+1] if '--theme' in sys.argv else 'bosque'
manifest = json.loads((ROOT/'manifestos'/(THEME+'.json')).read_text(encoding='utf-8'))
records = []
failures = []

def require(condition, message):
    if not condition:
        raise AssertionError(message)

def values(document, binary, accessor_index):
    a = document['accessors'][accessor_index]
    view = document['bufferViews'][a['bufferView']]
    components = {'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4}[a['type']]
    fmt, size, div = {5121:('B',1,255),5123:('H',2,65535),5125:('I',4,4294967295),5126:('f',4,1)}[a['componentType']]
    offset = view.get('byteOffset',0)+a.get('byteOffset',0)
    stride = view.get('byteStride',components*size)
    for i in range(a['count']):
        row = struct.unpack_from('<'+fmt*components,binary,offset+i*stride)
        yield tuple(v/div if a.get('normalized') else v for v in row)

def validate_glb(entry):
    data=(ROOT/entry['glb']).read_bytes()
    require(data[:4]==b'glTF' and struct.unpack_from('<I',data,8)[0]==len(data),'GLB header')
    length=struct.unpack_from('<I',data,12)[0]
    document=json.loads(data[20:20+length])
    offset=20+length
    binary=data[offset+8:]
    reds=[]
    coords=[]
    tri_count=0
    for mesh in document['meshes']:
        for primitive in mesh['primitives']:
            attrs=primitive['attributes']
            require('TEXCOORD_0' in attrs,'UV ausente GLB')
            coords.extend(values(document,binary,attrs['POSITION']))
            for c in values(document,binary,attrs['COLOR_0']):
                require(all(abs(v-1)<.005 for v in c),'COLOR_0 deve permanecer branco no GLB')
            for c in values(document,binary,attrs['COLOR_1']):
                require(abs(c[1])<.005 and abs(c[2])<.005,'Pesos de vento fora do canal R')
                reds.append(c[0])
            tri_count+=document['accessors'][primitive['indices']]['count']//3
    require(tri_count==entry['tris'],'Tris GLB diferentes do manifesto')
    dimensions=[max(v[a] for v in coords)-min(v[a] for v in coords) for a in range(3)]
    expected=[entry['dimensoes'][0],entry['dimensoes'][2],entry['dimensoes'][1]]
    require(all(abs(a-b)<.0002 for a,b in zip(dimensions,expected)),'GLB escala/eixos')
    if entry['vento']=='sim':
        require(min(reds)<.005 and max(reds)>.995,'GLB perdeu pesos de vento 0..1')
    if entry['peca'].startswith('entorno'):
        require(max(reds)<.005,'Entorno limpo ainda tem vértices com vento no GLB')
    for sampler in document.get('samplers',[]):
        require(sampler['magFilter']==9728 and sampler['minFilter']==9728,'GLB sem filtro nearest')
    nodes={n['name'] for n in document['nodes'] if 'name' in n}
    require(all(e['nome'] in nodes for e in entry['empties']),'Socket ausente no GLB')
    for e in entry['empties']:
        node=next(n for n in document['nodes'] if n.get('name')==e['nome'])
        x,y,z=e['posicaoBlender']
        require(all(abs(a-b)<.0002 for a,b in zip(node.get('translation',[0,0,0]),[x,z,-y])),'Socket GLB deslocado')
    return {'tris':tri_count,'dimensoesYup':[round(v,4) for v in dimensions],
            'ventoR':[round(min(reds),4),round(max(reds),4)],'sockets':len(entry['empties'])}

def validate_fbx(entry):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(ROOT/entry['arquivo']),use_image_search=False,colors_type='LINEAR')
    objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
    require(len(objs)==1,'FBX deve conter uma malha combinada')
    obj=objs[0]
    coords=[obj.matrix_world@v.co for v in obj.data.vertices]
    dimensions=[max(v[a] for v in coords)-min(v[a] for v in coords) for a in range(3)]
    require(all(abs(a-b)<.0003 for a,b in zip(dimensions,entry['dimensoes'])),'FBX escala/eixos na reimportação')
    require(obj.location.length<.0001,'FBX pivô deslocado')
    obj.data.calc_loop_triangles()
    require(len(obj.data.loop_triangles)==entry['tris'],'FBX tris diferentes')
    require(len(obj.data.uv_layers)>0,'FBX UV ausente')
    color=obj.data.color_attributes.get('vento')
    require(color is not None,'FBX perdeu atributo vento')
    reds=[d.color[0] for d in color.data]
    if entry['vento']=='sim':
        require(min(reds)<.005 and max(reds)>.995,'FBX vento perdido')
    if entry['peca'].startswith('entorno'):
        require(max(reds)<.005,'Entorno limpo ainda tem vértices com vento no FBX')
    names={o.name for o in bpy.context.scene.objects if o.type=='EMPTY'}
    require(all(e['nome'] in names for e in entry['empties']),'FBX socket ausente')
    for e in entry['empties']:
        obj_socket=bpy.data.objects.get(e['nome'])
        require((obj_socket.matrix_world.translation-Vector(e['posicaoBlender'])).length<.0002,'Socket FBX deslocado')
    if entry['peca'].startswith('entorno'):
        n=12 if entry['peca']=='entorno' else int(entry['peca'].split('_')[1])
        tree=BVHTree.FromPolygons(coords,[tuple(t.vertices) for t in obj.data.loop_triangles],all_triangles=True)
        for x in range(n):
            for y in range(n):
                hit=tree.ray_cast(Vector((x-(n-1)/2,y-(n-1)/2,100)),Vector((0,0,-1)),200)
                require(hit[0] is None,f'Entorno sobrepõe casa {x},{y}')
    elif not entry['peca'].startswith('beira'):
        require(abs(min(v.z for v in coords))<.0001,'Base do prop deve estar em Z=0')
    return {'tris':len(obj.data.loop_triangles),'pivo':[round(v,6) for v in obj.location],
            'dimensoesZup':[round(v,4) for v in dimensions],
            'ventoR':[round(min(reds),4),round(max(reds),4)],'sockets':len(names)}

for entry in manifest['pecas']:
    try:
        for kind in ['arquivo','glb']:
            path=ROOT/entry[kind]
            require(hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'][kind],'Hash alterado')
        glb=validate_glb(entry)
        fbx=validate_fbx(entry)
        limit=40000 if entry['peca'].startswith('entorno') else 3000
        require(entry['tris']<=limit,'Limite de tris')
        if entry['peca']=='arbusto':
            require(entry['dimensoes'][2]<=.6 and max(entry['dimensoes'][:2])<=1,'Arbusto fora de escala')
        if entry['peca']=='arvore':
            require(entry['dimensoes'][2]<=2.2,'Árvore muito alta')
        if entry['peca'].startswith('caixa'):
            require(max(entry['dimensoes'][:2])<=.8501,'Caixa fora da casa')
        if entry['peca'].startswith('entorno'):
            require(entry['bounds']['max'][2]<=.0301,'Entorno da revisão 03 tem decoração acima do chão')
        records.append({'peca':entry['peca'],'status':'ok','fbx':fbx,'glb':glb})
        print('VALIDADO',entry['peca'],flush=True)
    except Exception as error:
        failures.append({'peca':entry['peca'],'erro':str(error)})
        print('FALHA',entry['peca'],str(error),flush=True)

selection=json.loads((ASSETS/'fontes'/(THEME+'-texturas.json')).read_text(encoding='utf-8'))
for material,source in selection['selecao'].items():
    require((ASSETS/'mapa'/THEME/(material+'.png')).read_bytes()==(ASSETS/source).read_bytes(),'Procedência da textura')

bpy.ops.wm.open_mainfile(filepath=str(ROOT/manifest['fonte']))
for entry in manifest['pecas']:
    original=bpy.data.objects.get(THEME+'_'+entry['peca'])
    require(original is not None and original.location.length<.0001,'Fonte Blender: peça ou pivô inválido')
shop=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith(THEME+'_loja_mascate') and o.location.length>.01]
require(len(shop)==1 and abs(shop[0].location.x)<.0001 and abs(shop[0].location.y)<.0001,'A loja da prévia deve estar no meio')

preview_checks=[]
sources=manifest['versoesBosque'] or [{'fonte':manifest['fonte']}]
for version in sources:
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/version['fonte']))
    bpy.context.view_layer.update()
    demo=bpy.data.collections['PREVIA_TABULEIRO_12x12_APENAS_EXEMPLO']
    verts=[]
    faces=[]
    for obj in demo.objects:
        if obj.type!='MESH' or not obj.get('peca','').startswith('topo'): continue
        offset=len(verts)
        verts.extend(obj.matrix_world@v.co for v in obj.data.vertices)
        faces.extend(tuple(offset+i for i in p.vertices) for p in obj.data.polygons)
    floor=BVHTree.FromPolygons(verts,faces)
    prop_count=0
    coins=0
    centered=0
    for obj in demo.objects:
        name=obj.get('peca','')
        if obj.type!='MESH' or name.startswith(('topo','coluna','beira','entorno')): continue
        world=[obj.matrix_world@v.co for v in obj.data.vertices]
        require(all(abs(v.x)<=5.5001 and abs(v.y)<=5.5001 for v in world),obj.name+': sem margem dentro da ilha')
        min_z=min(v.z for v in world)
        for point in world:
            if point.z-min_z>.002: continue
            hit=floor.ray_cast(point+Vector((0,0,5)),Vector((0,0,-1)),10)
            require(hit[0] is not None,obj.name+': base sem chão')
            require(abs(hit[0].z-min_z)<.004,obj.name+': base flutua ou atravessa o chão')
        prop_count+=1
        if name=='moeda_chao':
            coins+=1
            require(abs(obj.rotation_euler.x)<.0001 and abs(obj.rotation_euler.y)<.0001,'Moeda deve ficar deitada')
        if name=='loja_mascate':
            centered+=1
            require(abs(obj.location.x)<.0001 and abs(obj.location.y)<.0001,'Loja fora do meio')
    require(centered==1 and coins>0,'Prévia precisa de loja central e moedas no chão')
    sectors=[0,0,0,0]
    mains=[o for o in demo.objects if o.type=='MESH' and o.get('prop_principal')]
    for obj in mains:
        x,y=obj['casa_exemplo']
        require(((x-5.5)**2+(y-5.5)**2)**.5>=2,'Prop grande junto à loja')
        sectors[(x>=6)+2*(y>=6)]+=1
    require(all(n>=1 for n in sectors),'Props concentrados em poucos setores')
    def xy_bounds(obj):
        points=[obj.matrix_world@Vector(p) for p in obj.bound_box]
        return [min(p.x for p in points),min(p.y for p in points),max(p.x for p in points),max(p.y for p in points)]
    lake=next(o for o in demo.objects if o.get('peca')=='enfeite_lago')
    store=next(o for o in demo.objects if o.get('peca')=='loja_mascate')
    a,b=xy_bounds(lake),xy_bounds(store)
    gap=max(a[0]-b[2],b[0]-a[2],a[1]-b[3],b[1]-a[3])
    require(gap>.35,'Lago invade ou fica colado à loja')
    snow_gaps=[]
    if THEME=='neve':
        for tree in [o for o in mains if o.get('peca','').startswith('arvore')]:
            tree_bounds=xy_bounds(tree)
            for tile in [o for o in demo.objects if o.get('peca','').startswith('topo') and o.location.z+.05>tree.location.z+.004]:
                tile_bounds=xy_bounds(tile)
                clearance=max(tree_bounds[0]-tile_bounds[2],tile_bounds[0]-tree_bounds[2],tree_bounds[1]-tile_bounds[3],tile_bounds[1]-tree_bounds[3])
                require(clearance>.25,'Abeto encosta em terraço mais alto')
                snow_gaps.append(clearance)
    preview_checks.append({'fonte':version['fonte'],'propsComApoioConferido':prop_count,'moedasDeitadas':coins,'margemMetros':.5,'lojaCentral':True,'lagoSeparadoDaLojaMetros':round(gap,4),'propsPrincipaisPorSetor':sectors,'raioLivreCentroMetros':2,'folgaAbetosDeTerracosAltos':round(min(snow_gaps),4) if snow_gaps else None})

result={'status':'aprovado_tecnicamente' if not failures else 'falhas','pecas':len(records),
        'fbxReimportados':len(records),'glbConferidos':len(records),'texturasOriginais':8,
        'checks':'escala, eixos, pivôs, tris, UV, hashes, pesos R, COLOR_0 neutro no GLB, nearest, sockets, vão central dos entornos',
        'fonteBlenderReaberta':True,'lojaCentralConferida':True,'previewsConferidas':preview_checks,'jogoUnityTestado':False,'avaliacaoVisualAutor':'pendente','resultados':records,'falhas':failures}
(ROOT/'manifestos'/(THEME+'-validacao.json')).write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
require(not failures,str(failures))
print('VALIDACAO_OK',len(records),'FBX e GLB',flush=True)
