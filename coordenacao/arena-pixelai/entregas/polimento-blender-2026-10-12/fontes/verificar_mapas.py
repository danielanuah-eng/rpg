"""Verifica o GLB entregue contra a versão efetivamente usada no jogo."""
from pathlib import Path
import json,struct,hashlib
import numpy as np
ROOT=Path(__file__).resolve().parents[1]
LIVE=ROOT.parents[3]/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Models/Mapa'
FORMATS={5120:np.int8,5121:np.uint8,5122:np.int16,5123:np.uint16,5125:np.uint32,5126:np.float32}
COUNT={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4,'MAT4':16}
def read(path):
    data=path.read_bytes();assert data[:4]==b'glTF' and struct.unpack_from('<I',data,8)[0]==len(data),path
    n=struct.unpack_from('<I',data,12)[0];doc=json.loads(data[20:20+n]);return doc,data[28+n:]
def accessor(doc,bin,index):
    a=doc['accessors'][index];v=doc['bufferViews'][a['bufferView']];dtype=FORMATS[a['componentType']];n=COUNT[a['type']]
    row=np.dtype(dtype).itemsize*n;offset=v.get('byteOffset',0)+a.get('byteOffset',0)
    values=np.ndarray((a['count'],n),dtype=dtype,buffer=bin,offset=offset,strides=(v.get('byteStride',row),np.dtype(dtype).itemsize)).astype(float)
    if a.get('normalized'):values/=np.iinfo(dtype).max
    return values
def transform(node):
    if 'matrix' in node:return np.array(node['matrix']).reshape(4,4).T
    x,y,z,w=node.get('rotation',[0,0,0,1]);t=np.eye(4)
    t[:3,:3]=[[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)],[2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],[2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]]
    t[:3,:3]=t[:3,:3]@np.diag(node.get('scale',[1,1,1]));t[:3,3]=node.get('translation',[0,0,0]);return t
def bbox(doc,bin):
    points=[]
    def visit(i,parent):
        node=doc['nodes'][i];world=parent@transform(node)
        if 'mesh' in node:
            for primitive in doc['meshes'][node['mesh']]['primitives']:
                p=accessor(doc,bin,primitive['attributes']['POSITION']);points.append((np.c_[p,np.ones(len(p))]@world.T)[:,:3])
        for c in node.get('children',[]):visit(c,world)
    for i in doc['scenes'][doc.get('scene',0)]['nodes']:visit(i,np.eye(4))
    p=np.vstack(points);return p.min(axis=0),p.max(axis=0)
def triangles(doc):return sum(doc['accessors'][p['indices']]['count']//3 for m in doc.get('meshes',[]) for p in m['primitives'])
reports=[];failures=[]
for original in sorted(LIVE.rglob('*.glb')):
    rel=original.relative_to(LIVE);target=ROOT/'models'/rel
    try:
        a,abin=read(original);b,bbin=read(target)
        before=bbox(a,abin);after=bbox(b,bbin)
        assert all(np.max(np.abs(x-y))<.0003 for x,y in zip(before,after)),str(rel)+' bounds/pivo/eixos alterados'
        assert triangles(a)==triangles(b),(str(rel),'topologia alterada',triangles(a),triangles(b))
        clips_before={x.get('name') for x in a.get('animations',[])};clips_after={x.get('name') for x in b.get('animations',[])}
        assert clips_before==clips_after,(rel,'clipes alterados',clips_before,clips_after)
        assert not b.get('textures'),str(rel)+' textura ruidosa restante'
        for mesh in b['meshes']:
            for p in mesh['primitives']:
                attrs=p['attributes']
                if 'COLOR_0' in attrs:assert np.max(np.abs(accessor(b,bbin,attrs['COLOR_0'])-1))<.005,(rel,'cor neutra nao branca')
        wind_ranges=[]
        for mesh in b['meshes']:
            for p in mesh['primitives']:
                if 'COLOR_1' in p['attributes']:
                    color=accessor(b,bbin,p['attributes']['COLOR_1']);wind_ranges.append([float(color[:,0].min()),float(color[:,0].max())])
        reports.append({'arquivo':str(rel),'triangulos':triangles(b),'boundsGLB':[x.tolist() for x in after],
          'clipes':sorted(clips_after),'pesosVento':wind_ranges,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
    except Exception as e:failures.append(str(e))
report={'estado':'aprovado_tecnicamente' if not failures else 'falha','modelosConferidos':len(reports),'falhas':failures,'pecas':reports,'integracaoUnity':'pendente_claude'}
(ROOT/'manifestos/mapas-validacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('MAPAS',len(reports),'FALHAS',len(failures));print('\n'.join(failures[:15]));assert not failures
