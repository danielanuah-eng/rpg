"""Inspeciona clipes reais do GLB e reimporta cada rig no Blender."""
import bpy,json,struct,math,hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
def read(path):
    data=path.read_bytes();assert data[:4]==b'glTF';n=struct.unpack_from('<I',data,12)[0]
    return json.loads(data[20:20+n]),data[28+n:],data
def values(doc,data,index):
    a=doc['accessors'][index];v=doc['bufferViews'][a['bufferView']];n={'SCALAR':1,'VEC3':3,'VEC4':4,'MAT4':16}[a['type']]
    assert a['componentType']==5126
    offset=v.get('byteOffset',0)+a.get('byteOffset',0);stride=v.get('byteStride',n*4)
    return [struct.unpack_from('<'+'f'*n,data,offset+i*stride) for i in range(a['count'])]
records=[];fails=[]
for path in sorted((ROOT/'personagens').glob('*/personagem.visual.json')):
    try:
        m=json.loads(path.read_text(encoding='utf-8'));glb=path.parent/m['modelo'];doc,bin,data=read(glb)
        assert hashlib.sha256(data).hexdigest()==m['sha256'];assert doc.get('skins')
        names={a['name'] for a in doc.get('animations',[])};assert names=={c['id'] for c in m['clipes']}
        clip_records=[]
        for clip in m['clipes']:
            a=next(a for a in doc['animations'] if a['name']==clip['id']);assert a['channels']
            varying=0;duration=0
            for sampler in a['samplers']:
                times=values(doc,bin,sampler['input']);rows=values(doc,bin,sampler['output']);duration=max(duration,times[-1][0]-times[0][0])
                assert all(math.isfinite(x) for row in rows for x in row)
                if any(max(abs(v-w) for v,w in zip(row,rows[0]))>.0001 for row in rows):varying+=1
                if clip['loop']:assert max(abs(x-y) for x,y in zip(rows[0],rows[-1]))<.001,(m['id'],clip['id'],'loop com salto')
            assert varying>0,(m['id'],clip['id'],'clipe sem movimento')
            assert abs(duration-clip['duracaoSegundos'])<.025,(m['id'],clip['id'],'duracao')
            clip_records.append({'id':clip['id'],'canaisComMovimento':varying,'duracaoExportada':round(duration,4)})
        bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(glb))
        rigs=[o for o in bpy.data.objects if o.type=='ARMATURE'];assert len(rigs)==1,(m['id'],'rig reimportado')
        assert len(rigs[0].data.bones)>=m['ossos'],(m['id'],'ossos perdidos')
        records.append({'id':m['id'],'clipes':clip_records,'ossosReimportados':len(rigs[0].data.bones),'sha256':m['sha256']})
        print('REIMPORT_OK',m['id'],len(clip_records),flush=True)
    except Exception as e:fails.append(str(e));print('FALHA',str(e),flush=True)
inventory=json.loads((ROOT/'personagens/inventario.json').read_text(encoding='utf-8'))
assert len(records)+len(fails)==len(inventory),(len(records),len(inventory),'modelos ausentes')
report={'estado':'aprovado_tecnicamente' if not fails else 'falha','personagensConferidos':len(records),
 'clipesConferidos':sum(len(r['clipes']) for r in records),'falhas':fails,'personagens':records,'avaliacaoAutor':'pendente','integracaoUnity':'pendente_claude'}
(ROOT/'personagens/validacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('ELENCO_VALIDADO',len(records),'CLIPES',report['clipesConferidos'],'FALHAS',len(fails));assert not fails
