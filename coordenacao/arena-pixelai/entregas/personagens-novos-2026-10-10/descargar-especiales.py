from pathlib import Path
import urllib.request, zipfile, json
ROOT=Path(__file__).resolve().parent
tasks=[('https://api.pixellab.ai/mcp/images/26b0b333-9150-4718-a798-057debec1dd2/download',ROOT/'edgar-navio/v1-opaco.png'),('https://api.pixellab.ai/mcp/characters/590cc7bc-3673-477c-aa23-f5655dd2d97a/spritesheet',ROOT/'edgar/folha-fonte.zip')]
for url,file in tasks:
    try:
        with urllib.request.urlopen(url,timeout=45) as response:data=response.read()
        file.parent.mkdir(parents=True,exist_ok=True);file.write_bytes(data)
        if file.suffix=='.zip':
            with zipfile.ZipFile(file) as z:
                print(json.dumps({'file':str(file),'entries':z.namelist()[:12]}))
                for name in z.namelist():
                    if name.endswith('.json'): print(z.read(name).decode('utf-8')[:2600])
        else: print(json.dumps({'file':str(file),'bytes':len(data)}))
    except Exception as exc:print(json.dumps({'file':str(file),'error':str(exc)}))
