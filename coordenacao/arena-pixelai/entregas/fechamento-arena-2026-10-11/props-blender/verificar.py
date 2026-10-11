"""Le os GLB publicados: geometria, curvas efetivas e fechamento dos loops."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json, math, struct
import numpy as np
ROOT=Path(__file__).resolve().parent
TYPES={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4,'MAT4':16}
DTYPES={5126:'<f4',5125:'<u4',5123:'<u2',5121:'u1'}

def load(path):
    raw=path.read_bytes();magic,version,length=struct.unpack_from('<III',raw)
    assert magic==0x46546c67 and version==2 and length==len(raw)
    offset=12;chunks={}
    while offset<len(raw):
        n,kind=struct.unpack_from('<II',raw,offset);offset+=8;chunks[kind]=raw[offset:offset+n];offset+=n
    return json.loads(chunks[0x4e4f534a]),chunks[0x004e4942]

def access(g,data,index):
    a=g['accessors'][index];view=g['bufferViews'][a['bufferView']];n=TYPES[a['type']];dtype=np.dtype(DTYPES[a['componentType']]);start=view.get('byteOffset',0)+a.get('byteOffset',0);stride=view.get('byteStride',n*dtype.itemsize)
    return np.ndarray((a['count'],n),dtype=dtype,buffer=data,offset=start,strides=(stride,dtype.itemsize)).copy()

def main():
    manifest=json.loads((ROOT/'props.visual.json').read_text(encoding='utf-8'));rows=[]
    for asset in manifest['assets']:
        path=ROOT/asset['file'];assert hashlib.sha256(path.read_bytes()).hexdigest()==asset['sha256'];g,blob=load(path)
        assert g.get('animations'),asset['id']+' sem animacao exportada'
        assert not g.get('cameras') and not any(n.get('name','').startswith('chao_preview') for n in g['nodes'])
        triangles=sum(g['accessors'][p['indices']]['count']//3 for mesh in g['meshes'] for p in mesh['primitives']);assert 0<triangles<3000
        moving=[];duration=[]
        for anim in g['animations']:
            assert anim['name']=='AmbientLoop'
            for channel in anim['channels']:
                sampler=anim['samplers'][channel['sampler']];t=access(g,blob,sampler['input']).ravel();values=access(g,blob,sampler['output']);target=channel['target'];name=g['nodes'][target['node']].get('name','');kind=target['path']
                assert np.all(np.diff(t)>0) and abs(float(t[-1]-t[0])-2)<.01
                if sampler.get('interpolation')=='CUBICSPLINE':values=values[1::3]
                assert len(values)==len(t)
                if kind=='rotation':
                    assert abs(abs(float(np.dot(values[0],values[-1])))-1)<1e-4,'Rotacao nao fecha'
                else:assert np.allclose(values[0],values[-1],atol=1e-5),'Loop nao fecha '+name
                if float(np.abs(values-values[0]).max())>1e-5:moving.append({'node':name,'property':kind,'keys':len(t)})
                duration.append(round(float(t[-1]-t[0]),4))
        assert moving,asset['id']+' so contem curvas estaticas'
        rows.append({'id':asset['id'],'trianglesExported':triangles,'animations':len(g['animations']),'movingChannels':moving,'durationSeconds':max(duration),'loopClosed':True,'previewGeometryNotExported':True})
        asset['visualReview']='revisado_sol; aguardando_avaliacao_astra';asset['technicalValidation']='passed';asset['integration']='pendente_claude'
    previews=[Image.open(ROOT/'previews'/f'{f:02d}.png').convert('RGB') for f in range(1,49,6)]
    assert len(previews)==8 and all(i.size==previews[0].size for i in previews)
    assert any(not np.array_equal(np.array(previews[0]),np.array(i)) for i in previews[1:]),'Preview nao se move'
    previews[0].save(ROOT/'preview-animado.gif',save_all=True,append_images=previews[1:],duration=250,loop=0,optimize=False)
    contact=Image.new('RGB',(960,1080));contact.paste(previews[0],(0,0));contact.paste(previews[2],(0,540));d=ImageDraw.Draw(contact);d.text((12,12),'Loop — frame 1',fill='white');d.text((12,552),'Loop — frame 13',fill='white');contact.save(ROOT/'contato.png')
    report={'status':'passed','assets':len(rows),'allGlbsHaveMovingAnimation':True,'loopsClosed':True,'generationCostPixelLab':0,'checks':rows,'integration':'pendente_claude','limitation':'Nao testado no runtime Unity; arte/GLB e curvas validados.'}
    (ROOT/'verificacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
    manifest['status']='lote_tecnico_pronto; aguardando_avaliacao_astra';(ROOT/'props.visual.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
    print(json.dumps({k:v for k,v in report.items() if k!='checks'},ensure_ascii=False))

if __name__=='__main__':main()
