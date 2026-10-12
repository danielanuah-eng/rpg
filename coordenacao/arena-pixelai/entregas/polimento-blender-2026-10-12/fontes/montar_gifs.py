from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
count=0
for path in (ROOT/'personagens/animacoes-preview').glob('*/*'):
    if not path.is_dir():continue
    frames=[Image.open(p).convert('RGB') for p in sorted(path.glob('[0-9][0-9].png'))]
    assert len(frames)==8,path
    frames[0].save(path.parent/(path.name+'.gif'),save_all=True,append_images=frames[1:],duration=125,loop=0)
    strip=Image.new('RGB',(1536,192))
    for i,frame in enumerate(frames):strip.paste(frame.resize((192,192)),(i*192,0))
    strip.save(path.parent/(path.name+'-quadros.png'))
    count+=1
print('GIFS',count)
