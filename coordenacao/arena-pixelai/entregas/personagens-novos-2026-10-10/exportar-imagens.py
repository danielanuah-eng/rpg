from pathlib import Path
import io,zipfile,urllib.request,json,concurrent.futures
from PIL import Image
HERE=Path(__file__).resolve().parent
STATE=json.loads((HERE/"estado-atual.json").read_text(encoding="utf8"))
def download(job,dest,count):
 dest.mkdir(parents=True,exist_ok=True)
 if count>1:
  for i in range(count):
   with urllib.request.urlopen("https://api.pixellab.ai/mcp/images/"+job+"/download?index="+str(i),timeout=75) as r:frame=r.read()
   Image.open(io.BytesIO(frame)).convert("RGBA").save(dest/f"frame-{i:02d}.png")
  return dict(job=job,folder=dest.name,frames=count)
 with urllib.request.urlopen("https://api.pixellab.ai/mcp/images/"+job+"/download",timeout=75) as r:data=r.read()
 if data[:2]==b"PK":
  (dest/"fonte.zip").write_bytes(data);z=zipfile.ZipFile(io.BytesIO(data))
  files=[n for n in z.namelist() if n.lower().endswith(".png")]
  for i,name in enumerate(files):Image.open(io.BytesIO(z.read(name))).convert("RGBA").save(dest/f"frame-{i:02d}.png")
 else:Image.open(io.BytesIO(data)).convert("RGBA").save(dest/"frame-00.png");files=["image"]
 return dict(job=job,folder=dest.name,frames=len(files))
tasks=[("5234edf2-2d26-4f73-8045-c3449d5830d7",HERE/"edgar-navio/base",1)]
tasks.extend((p["job_id"],HERE/"loja-animada"/p["id"],p["frames"]+1) for p in STATE.get("shopPlans",[]) if p.get("job_id"))
tasks.extend((p["job_id"],HERE/"edgar-navio"/p["name"],p["frames"]+1) for p in STATE.get("shipPlans",[]) if p.get("job_id"))
with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
 futures={pool.submit(download,j,d,n):(j,d) for j,d,n in tasks}
 for f in concurrent.futures.as_completed(futures):
  try:print(json.dumps(f.result()),flush=True)
  except Exception as exc:print(json.dumps(dict(folder=futures[f][1].name,error=str(exc))),flush=True)

