from pathlib import Path
import urllib.request, concurrent.futures
from PIL import Image

root=Path(__file__).resolve().parent
jobs={
 'baforada':'0efb8559-e8ed-43bc-a86c-ead0e39a5b14',
 'raio-arcano':'4b253912-9df2-40cc-a500-3a1b11a47891',
 'cura':'312ddcee-5b87-494b-b29c-67e154054a77',
 'impacto-area':'0fbc45ae-956f-4c68-92d4-f22be1fa83e3',
 'invocacao':'39b2d4ea-c4ca-441d-a0af-87372f2f4d6c'}
tasks=[]
for name,job in jobs.items():
    folder=root/'eventos-cenarios'/name; folder.mkdir(parents=True,exist_ok=True)
    for i in range(5):
        tasks.append((f'https://api.pixellab.ai/mcp/images/{job}/download?index={i}',folder/f'frame-{i+1:02d}.png'))
folder=root/'correcoes-sincronia'
tasks.append(('https://api.pixellab.ai/mcp/characters/0e585583-ec07-4831-9269-44b1b7fbd0b0/download',folder/'damian-canonico-final.zip'))
def download(task):
    url,file=task
    with urllib.request.urlopen(url,timeout=60) as response: data=response.read()
    file.write_bytes(data)
    if file.suffix=='.png': Image.open(file).verify()
    return file.name
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
    results=list(pool.map(download,tasks))
print('Downloaded and decoded',len(results),'official assets')
