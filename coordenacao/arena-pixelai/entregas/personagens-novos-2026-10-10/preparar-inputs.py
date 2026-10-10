from pathlib import Path
from PIL import Image
import base64,json
here=Path(__file__).resolve().parent
root=here.parents[3]
result={"padding":{},"icons":{},"blank":{},"rotation":{}}
import io
for key,size in [("128",(128,128)),("192",(192,192)),("ship",(256,192))]:
 buffer=io.BytesIO();Image.new("RGBA",size).save(buffer,format="PNG")
 result["blank"][key]=base64.b64encode(buffer.getvalue()).decode()
for src in (here/"gryvro/celulas/rotations").glob("*.png"):
 im=Image.open(src).convert("RGBA")
 assert im.width>=128 and im.height>=128,im.size
 x=(im.width-128)//2;y=(im.height-128)//2
 im=im.crop((x,y,x+128,y+128))
 out=Image.new("RGBA",(160,160));out.paste(im,(16,16))
 target=here/"gryvro/transicoes"/src.name;target.parent.mkdir(parents=True,exist_ok=True);out.save(target)
 result["padding"][src.stem]=base64.b64encode(target.read_bytes()).decode()
for src in (root/"coordenacao/arena-pixelai/entregas/loja-v2/itens").glob("*.png"):
 result["icons"][src.stem]=base64.b64encode(src.read_bytes()).decode()
for file in here.glob("*/export-registro.json"):
 record=json.loads(file.read_text(encoding="utf8"));actor=record["actor"]
 layout=json.loads((file.parent/"layout-fonte.json").read_text(encoding="utf8"))
 size=layout["character"]["size"]
 width,height=size["width"],size["height"]
 for src in (file.parent/"celulas/rotations").glob("*.png"):
  im=Image.open(src).convert("RGBA");x=(im.width-width)//2;y=(im.height-height)//2
  im=im.crop((x,y,x+width,y+height));buffer=io.BytesIO();im.save(buffer,format="PNG")
  result["rotation"][actor["character_id"]+"_"+src.stem]=base64.b64encode(buffer.getvalue()).decode()
(here/"inputs-locais.json").write_text(json.dumps(result),encoding="utf8")
print(json.dumps({"padding":list(result["padding"]),"icons":list(result["icons"])}))

