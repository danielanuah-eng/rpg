"""Verifica os arquivos efetivamente exportados, não só a cena de origem."""
import bpy
import json
import struct
import math
import hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree

ROOT = Path(__file__).resolve().parents[1]
manifest = json.loads((ROOT/'manifesto.json').read_text(encoding='utf-8'))
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
        records.append({'peca':entry['peca'],'status':'ok','fbx':fbx,'glb':glb})
        print('VALIDADO',entry['peca'],flush=True)
    except Exception as error:
        failures.append({'peca':entry['peca'],'erro':str(error)})
        print('FALHA',entry['peca'],str(error),flush=True)

selection=json.loads((ROOT/'fontes'/'texturas-selecao.json').read_text(encoding='utf-8'))
for material,source in selection['selecao'].items():
    require((ROOT/'mapa'/'bosque'/(material+'.png')).read_bytes()==(ROOT/'fontes'/'pixellab-originais'/source).read_bytes(),'Procedência da textura')

result={'status':'aprovado_tecnicamente' if not failures else 'falhas','pecas':len(records),
        'fbxReimportados':len(records),'glbConferidos':len(records),'texturasOriginais':8,
        'checks':'escala, eixos, pivôs, tris, UV, hashes, pesos R, COLOR_0 neutro no GLB, nearest, sockets, vão central dos entornos',
        'jogoUnityTestado':False,'avaliacaoVisualAutor':'pendente','resultados':records,'falhas':failures}
(ROOT/'validacao.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
require(not failures,str(failures))
print('VALIDACAO_OK',len(records),'FBX e GLB',flush=True)
