from pathlib import Path
import urllib.request, zipfile, io, concurrent.futures, re, json, unicodedata
from PIL import Image
ROOT=Path(__file__).resolve().parent
def run(file):
    text=file.read_text(encoding='utf-8')
    match=re.search(r'^download: (https://\S+)',text,re.M)
    if not match: return {'file':str(file),'error':'no download'}
    folder=file.parent
    with urllib.request.urlopen(match[1],timeout=60) as r: data=r.read()
    z=zipfile.ZipFile(io.BytesIO(data))
    names=[x for x in z.namelist() if x.endswith('.png')]
    state=re.search(r'^state_name: (.+)',text,re.M).group(1)
    folders=sorted({Path(x).parts[0] for x in names})
    def norm(value):
        value=unicodedata.normalize('NFKD',value)
        return ''.join(x.lower() for x in value if x.isalnum() and not unicodedata.combining(x))
    matches=[s for s in folders if norm(s)==norm(state)]
    if len(matches)!=1:
        return {'asset':folder.name,'error':'state folder ambiguous','state':state,'folders':folders}
    names=[x for x in names if Path(x).parts[0]==matches[0]]
    (folder/'pixellab-fonte.zip').write_bytes(data)
    for entry in names:
        blob=z.read(entry)
        im=Image.open(io.BytesIO(blob)); im.verify()
        parts=Path(entry).parts
        if 'rotations' in parts:
            relative=Path('sprites')/Path(entry).name
        elif 'animations' in parts:
            index=parts.index('animations')
            tail=parts[index+1:]
            if len(tail)>=2:
                relative=Path('sprites')/(tail[0]+'_'+Path(tail[-1]).name)
            else: relative=Path('fonte')/Path(entry).name
        else: relative=Path('fonte')/Path(entry).name
        path=folder/relative;path.parent.mkdir(parents=True,exist_ok=True);path.write_bytes(blob)
    return {'asset':folder.name,'state':state,'pngs':len(names),'sample':names[:12]}
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
    futures={pool.submit(run,p):p for p in ROOT.glob('*/snapshot-base.txt')}
    for future in concurrent.futures.as_completed(futures):
        try: print(json.dumps(future.result(),ensure_ascii=False))
        except Exception as e: print(json.dumps({'file':str(futures[future]),'error':str(e)}))
