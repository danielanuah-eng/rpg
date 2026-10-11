from pathlib import Path
import argparse,io,json,urllib.request,zipfile,concurrent.futures,hashlib
from PIL import Image,ImageDraw
HERE=Path(__file__).resolve().parent
STATE=json.loads((HERE/"estado-atual.json").read_text(encoding="utf8"))
PLANS=STATE["animationPlans"]
def actors():
 result=[dict(x,folder=x["id"],owner=x["id"]) for x in STATE["characters"] if x.get("ready")]
 for key,value in STATE["damian"].items():
  if value.get("ready"):result.append(dict(value,folder="damian-"+key,owner="damian",stateKey=key))
 for key,value in STATE["special"].items():
  if value.get("ready"):result.append(dict(value,folder=value["owner"]+"-"+key,owner=value["owner"],stateKey=key))
 return result
def belongs(p,a):
 if p.get("character_id")!=a["character_id"]:return False
 return bool(p.get("group_id"))
def fetch_actor(a):
 folder=HERE/a["folder"];folder.mkdir(parents=True,exist_ok=True)
 url="https://api.pixellab.ai/mcp/characters/"+a["character_id"]+"/spritesheet"
 with urllib.request.urlopen(url,timeout=90) as r:data=r.read()
 archive=folder/"folha-fonte.zip";archive.write_bytes(data)
 z=zipfile.ZipFile(io.BytesIO(data))
 layouts=[n for n in z.namelist() if n.endswith(".json")]
 assert len(layouts)==1,layouts
 layout=json.loads(z.read(layouts[0]))
 sheet=layout["spritesheet"]
 png=sheet["path"]
 if png not in z.namelist():png=next(n for n in z.namelist() if n.endswith(".png"))
 atlas=Image.open(io.BytesIO(z.read(png))).convert("RGBA")
 cw,ch=sheet["cell_size"]["width"],sheet["cell_size"]["height"]
 records=[]
 selected={p["group_id"]:p for p in PLANS if belongs(p,a)}
 for row in sheet["rows"]:
  y=row["row"]*ch
  if row["type"]=="rotations":
   for i,d in enumerate(row["directions"]):
    out=folder/"celulas/rotations"/(d+".png");out.parent.mkdir(parents=True,exist_ok=True)
    atlas.crop((i*cw,y,(i+1)*cw,y+ch)).save(out)
  elif row["type"]=="animation" and row.get("animation_group_id") in selected:
   p=selected[row["animation_group_id"]];d=row["direction"];n=row["frame_count"]
   dest=folder/"celulas/animations"/p["name"]/d;dest.mkdir(parents=True,exist_ok=True)
   for i in range(n):atlas.crop((i*cw,y,(i+1)*cw,y+ch)).save(dest/f"frame-{i:02d}.png")
   records.append(dict(plan=p["key"],name=p["name"],direction=d,count=n,cell=[cw,ch],group_id=row["animation_group_id"],fps=p["fps"],loop=p["loop"],reverse=p.get("deriveReverse")))
 (folder/"layout-fonte.json").write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding="utf8")
 (folder/"export-registro.json").write_text(json.dumps(dict(actor=a,clips=records,source_cell=[cw,ch]),ensure_ascii=False,indent=2),encoding="utf8")
 return dict(asset=a["folder"],clips=len(records),cell=[cw,ch])
def checker(im):
 bg=Image.new("RGBA",im.size,(38,43,52,255));d=ImageDraw.Draw(bg)
 for y in range(0,im.height,12):
  for x in range(0,im.width,12):
   if (x//12+y//12)%2:d.rectangle((x,y,x+11,y+11),fill=(51,58,69,255))
 bg.alpha_composite(im);return bg.convert("RGB")
def pad(im,size):
 out=Image.new("RGBA",size);out.paste(im,((size[0]-im.width)//2,(size[1]-im.height)//2));return out
def normalize():
 groups={}
 for file in HERE.glob("*/export-registro.json"):
  record=json.loads(file.read_text(encoding="utf8"));groups.setdefault(record["actor"]["owner"],[]).append((file.parent,record))
 manifests=[]
 for owner,records in groups.items():
  size=(max(r["source_cell"][0] for _,r in records),max(r["source_cell"][1] for _,r in records))
  for folder,r in records:
   native=r["actor"];rot=[]
   for file in (folder/"celulas/rotations").glob("*.png"):
    im=pad(Image.open(file).convert("RGBA"),size);dest=folder/"sprites"/file.name;dest.parent.mkdir(exist_ok=True);im.save(dest);rot.append(file.stem)
   clips=[]
   for item in r["clips"]:
    frames=[pad(Image.open(file).convert("RGBA"),size) for file in sorted((folder/"celulas/animations"/item["name"]/item["direction"]).glob("*.png"))]
    assert len(frames)==item["count"]
    for name,sequence,derived in [(item["name"],frames,False)]+([(item["reverse"],list(reversed(frames)),True)] if item.get("reverse") else []):
     dest=folder/"animacoes"/name;dest.mkdir(parents=True,exist_ok=True)
     strip=Image.new("RGBA",(size[0]*len(sequence),size[1]))
     for i,im in enumerate(sequence):strip.paste(im,(i*size[0],0))
     png=dest/(item["direction"]+".png");strip.save(png)
     previews=[checker(im) for im in sequence]
     previews[0].save(dest/(item["direction"]+".gif"),save_all=True,append_images=previews[1:],duration=round(1000/item["fps"]),loop=0 if item["loop"] else 1,disposal=2)
     edge=sum(1 for im in sequence if any(im.getchannel("A").crop(box).getbbox() for box in [(0,0,size[0],1),(0,0,1,size[1]),(0,size[1]-1,size[0],size[1]),(size[0]-1,0,size[0],size[1])]))
     clips.append(dict(name=name,direction=item["direction"],frame_count=len(sequence),frame_size=list(size),fps=item["fps"],loop=item["loop"],derived_reverse=derived,pivot="cell-center",png=str(png.relative_to(HERE)).replace(chr(92),"/"),sha256=hashlib.sha256(png.read_bytes()).hexdigest(),alpha_extrema=strip.getchannel("A").getextrema(),edge_frames=edge,source_group=item["group_id"]))
   manifest=dict(actor_id=native["character_id"],owner=owner,state_key=native.get("stateKey"),rotations=rot,frame_size=list(size),pivot="cell-center",no_rescaling=True,clips=clips,integration="pendente_claude")
   (folder/"sprites.visual.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding="utf8");manifests.append(manifest)
 (HERE/"exportacao-atual.json").write_text(json.dumps(manifests,ensure_ascii=False,indent=2),encoding="utf8")
 return dict(actors=len(manifests),clips=sum(len(m["clips"]) for m in manifests))
if __name__=="__main__":
 parser=argparse.ArgumentParser();parser.add_argument("--download",action="store_true");parser.add_argument("--normalize",action="store_true");parser.add_argument("--only",nargs="*");args=parser.parse_args()
 if args.download:
  with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
   jobs={pool.submit(fetch_actor,a):a for a in actors() if not args.only or a["folder"] in args.only}
   for future in concurrent.futures.as_completed(jobs):
    try:print(json.dumps(future.result(),ensure_ascii=False),flush=True)
    except Exception as exc:print(json.dumps(dict(asset=jobs[future]["folder"],error=str(exc)),ensure_ascii=False),flush=True)
 if args.normalize:print(json.dumps(normalize()),flush=True)

