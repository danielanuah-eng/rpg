"""Checks independentes da arte final: arquivo, alfa, clipping, conteúdo e fontes."""
from pathlib import Path
from PIL import Image, ImageChops
import hashlib,json,statistics
ROOT=Path(__file__).resolve().parent
m=json.loads((ROOT/'assets.visual.json').read_text(encoding='utf-8'))
errors=[];notes=[];seen=set();frames_total=0;loops=0
for e in m['entries']:
    path=ROOT/e['file']; im=Image.open(path).convert('RGBA');n=e['frameSize'];frames_total+=e['frames']
    if e['resource'] in seen:errors.append('resource duplicado: '+e['resource'])
    seen.add(e['resource'])
    if im.size!=(n*e['frames'],n):errors.append('dimensão incorreta: '+e['resource'])
    if hashlib.sha256(path.read_bytes()).hexdigest()!=e['sha256']:errors.append('hash: '+e['resource'])
    if not all(0<=v<=1 for v in e['unityPivot']):errors.append('pivot fora do quadro: '+e['resource'])
    if not 0<=e['impactFrame0']<e['frames']:errors.append('impacto fora da tira: '+e['resource'])
    fr=[im.crop((i*n,0,(i+1)*n,n)) for i in range(e['frames'])]
    if not any(f.getchannel('A').getbbox() for f in fr):errors.append('clip vazio: '+e['resource'])
    alpha_values=im.getchannel('A').getextrema()
    if alpha_values[0]!=0:errors.append('sem transparência: '+e['resource'])
    for i,f in enumerate(fr):
        a=f.getchannel('A');bounds=a.getbbox()
        if bounds and (bounds[0]==0 or bounds[1]==0 or bounds[2]==n or bounds[3]==n):errors.append(f'corte na borda: {e["resource"]} f{i}')
    if e['loop'] and len(fr)>1:
        loops+=1
        diffs=[]
        for i in range(len(fr)):
            delta=ImageChops.difference(fr[i].getchannel('A'),fr[(i+1)%len(fr)].getchannel('A'))
            diffs.append(sum(delta.get_flattened_data())/(n*n*255))
        typical=statistics.median(diffs[:-1])
        if diffs[-1]>max(.018,typical*3):notes.append({'resource':e['resource'],'seamAlphaDelta':round(diffs[-1],4),'medianNeighborDelta':round(typical,4)})
requested=json.loads((ROOT/'reparos-pixellab-v2.json').read_text(encoding='utf-8'))['attempts']
for request in requested:
    folder=request['spec']['key'];count=request['spec']['frame_count']
    e=next(e for e in m['entries'] if e['source']==folder)
    if e['frames']!=count:errors.append('contagem do reparo: '+folder)
    strip=Image.open(ROOT/e['file']).convert('RGBA');n=e['frameSize']
    for i in range(count):
        src=Image.open(ROOT/f'reparos/{folder}/originais/{i+1}.png').convert('RGBA')
        dx=(n-src.width)//2
        actual=strip.crop((i*n+dx,0,i*n+dx+src.width,src.height))
        if actual.tobytes()!=src.tobytes():errors.append('pixels do reparo alterados: '+folder)
for rec in json.loads((ROOT/'downloads-reparos-baixados.json').read_text(encoding='utf-8')):
    if hashlib.sha256((ROOT/rec['file']).read_bytes()).hexdigest()!=rec['sha256']:errors.append('original alterado: '+rec['file'])
for p in (ROOT/'ui/art').glob('mapa-*.png'):
    im=Image.open(p).convert('RGBA')
    if im.size!=(1200,900) or im.getchannel('A').getextrema()[0]!=0:errors.append('render UI sem alfa ou tamanho incorreto: '+p.name)
report={'assets':len(m['entries']),'frames':frames_total,'loops':loops,'errors':errors,'loopNotes':notes,'scope':'PNG/manifestos. Não confirma importação nem gatilhos Unity.'}
(ROOT/'verificacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
print(json.dumps(report,ensure_ascii=False))
raise SystemExit(1 if errors else 0)
