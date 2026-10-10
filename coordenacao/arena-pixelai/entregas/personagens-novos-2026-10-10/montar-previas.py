from pathlib import Path
from PIL import Image,ImageDraw
import json
HERE=Path(__file__).resolve().parent
def bg(size):
 out=Image.new("RGBA",size,(35,41,50,255));d=ImageDraw.Draw(out)
 for y in range(0,size[1],12):
  for x in range(0,size[0],12):
   if (x//12+y//12)%2:d.rectangle((x,y,x+11,y+11),fill=(47,54,65,255))
 return out
names=["edgar","brumak","nixira","gryvro","raizor","alvyr","ossyro","lobo-de-cinzas"]
out=bg((4*220,2*250));draw=ImageDraw.Draw(out)
for i,name in enumerate(names):
 source=HERE/name/"sprites/south-east.png"
 if not source.exists():continue
 im=Image.open(source).convert("RGBA")
 if max(im.size)>184:
  scale=184/max(im.size);im=im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.NEAREST)
 x=(i%4)*220+(220-im.width)//2;y=(i//4)*250+22
 out.alpha_composite(im,(x,y));draw.text(((i%4)*220+12,(i//4)*250+225),name,fill="white")
out.convert("RGB").save(HERE/"personagens-preview.png")
items=[("camaleao-mao","damian-neutral"),("transform-martelo","damian-chameleonHandV2"),("transform-pistola","damian-chameleonHandV2"),("transform-canhao","damian-chameleonHandV2")]
out=bg((9*130+170,len(items)*155));draw=ImageDraw.Draw(out)
for row,(clip,folder) in enumerate(items):
 draw.text((8,row*155+68),clip,fill="white")
 files=sorted((HERE/folder/"celulas/animations"/clip/"south-east").glob("*.png"))
 for col,file in enumerate(files):
  im=Image.open(file).convert("RGBA")
  common=Image.new("RGBA",(192,176));common.paste(im,((192-im.width)//2,(176-im.height)//2));im=common
  scale=min(120/im.width,128/im.height);im=im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.NEAREST)
  out.alpha_composite(im,(170+col*130+(130-im.width)//2,row*155+8))
  draw.text((175+col*130,row*155+140),str(col),fill="white")
out.convert("RGB").save(HERE/"damian-sequencias-preview.png")

