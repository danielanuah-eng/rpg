from pathlib import Path
from PIL import Image, ImageDraw
import concurrent.futures, urllib.request, re, json, hashlib, argparse, zipfile

ROOT=Path(__file__).resolve().parent
def fetch(task):
    url,path=task
    path.parent.mkdir(parents=True,exist_ok=True)
    if path.exists():
        try:
            with Image.open(path) as im: im.verify()
            return str(path)
        except Exception: pass
    with urllib.request.urlopen(url,timeout=45) as res: data=res.read()
    with Image.open(__import__('io').BytesIO(data)) as im: im.verify()
    path.write_bytes(data)
    return str(path)

def main():
    snapshots=list(ROOT.glob('*/snapshot-*.txt'))
    tasks=[]
    for textfile in snapshots:
        content=textfile.read_text(encoding='utf-8')
        for name,url in re.findall(r'^  ([\w-]+): (https://\S+)',content,re.M):
            if name in ['south','south-east','east','north-east','north','north-west','west','south-west']:
                tasks.append((url,textfile.parent/'sprites'/f'{name}.png'))
        for name,direction,url in re.findall(r'^  ([\w-]+) \[([\w-]+)\]: (https://\S+)',content,re.M):
            tasks.append((url,textfile.parent/'sprites'/f'{name}_{direction}.png'))
    tasks=list({str(p):(u,p) for u,p in tasks}.values())
    errors=[]
    # ZIP oficial e a fonte de download: URLs Backblaze retornam 403 fora do servico.
    # Este script confere os arquivos ja extraidos por baixar-zip.py.
    metrics=[]
    for file in ROOT.glob('*/sprites/*.png'):
        im=Image.open(file).convert('RGBA')
        alpha=im.getchannel('A'); hist=alpha.histogram()
        metrics.append({'file':str(file.relative_to(ROOT)),'width':im.width,'height':im.height,'alphaMin':alpha.getextrema()[0],'alphaMax':alpha.getextrema()[1],'transparentPixels':hist[0],'partialAlphaPixels':sum(hist[1:255]),'visiblePixels':sum(hist[1:]),'sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
    (ROOT/'verificacao.json').write_text(json.dumps({'files':metrics,'errors':errors},ensure_ascii=False,indent=2),encoding='utf-8')
    sprites=list(ROOT.glob('*/sprites/south.png'))
    sheet=Image.new('RGB',(800,440),'#27343d'); draw=ImageDraw.Draw(sheet)
    for i,file in enumerate(sprites[:8]):
        sprite=Image.open(file).convert('RGBA').resize((192,192),Image.Resampling.NEAREST)
        x,y=(i%4)*200,(i//4)*220
        sheet.paste(sprite,(x,y+24),sprite); draw.text((x+5,y+5),file.parent.parent.name,fill='white')
    sheet.save(ROOT/'galeria-bases.png')
    print(json.dumps({'sourceTasks':len(tasks),'files':len(metrics),'errors':errors,'opaqueFiles':[x['file'] for x in metrics if x['alphaMin']!=0]},ensure_ascii=False))

if __name__=='__main__': main()
