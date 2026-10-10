"""Auditoria somente por leitura dos PNGs; comparativos não alteram os assets."""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
SPRITES = ROOT / "ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites"
OUT = HERE
OUT.mkdir(parents=True, exist_ok=True)
rows = []
for path in sorted(SPRITES.rglob("*.png")):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    # Projeteis são Single no importador, mesmo quando a imagem é larga.
    # Nas demais pastas, quadros quadrados são uma inferência pela dimensão.
    is_projectile = "Projeteis" in path.parts
    count = w // h if w > h and w % h == 0 and not is_projectile else 1
    fw = h if count > 1 else w
    boxes = []
    for i in range(count):
        frame = im.crop((i * fw, 0, (i + 1) * fw, h))
        mask = frame.getchannel("A").point(lambda a: 255 if a >= 16 else 0)
        box = mask.getbbox()
        if box:
            boxes.append({"frame": i, "bbox": list(box), "width": box[2]-box[0],
                          "height": box[3]-box[1], "heightRatio": round((box[3]-box[1])/h, 4),
                          "bottomMargin": h-box[3], "centerX": (box[0]+box[2])/2,
                          "opaqueAreaRatio": round(sum(frame.getchannel("A").histogram()[16:])/(fw*h), 4)})
    meta = path.with_suffix(".png.meta")
    text = meta.read_text(encoding="utf-8") if meta.exists() else ""
    import re
    settings = {k: (re.search(r"(?m)^\s*"+k+r":\s*([^\r\n]+)", text).group(1)
                    if re.search(r"(?m)^\s*"+k+r":\s*([^\r\n]+)", text) else None)
                for k in ["filterMode", "enableMipMap", "spritePixelsToUnits", "textureCompression"]}
    rows.append({"file": path.relative_to(ROOT).as_posix(), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
                 "canvas": [w,h], "inferredSquareFrames": count, "projectileImportedSingle": is_projectile, "alphaMinMax": list(im.getchannel("A").getextrema()),
                 "frames": boxes, "import": settings})

(OUT/"metricas.json").write_text(json.dumps({"alphaThreshold":16,"files":rows},ensure_ascii=False,indent=2),encoding="utf-8")
samples = []
for name in ["Ciantia","Talyssa","Perry","Diantus","DragaoEspectral","TchuTchaBerserk"]:
    paths = sorted((SPRITES/name).glob("*.png"))
    path = next((p for p in paths if p.name == "south.png"), None)
    if path is None:
        path = next((p for p in paths if p.name == "idle_south.png"), paths[0] if paths else None)
    if path:
        samples.append((name,Image.open(path).convert("RGBA")))
font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 15)
canvas = Image.new("RGB", (1050, 70+len(samples)*230), "#22313b")
d = ImageDraw.Draw(canvas)
d.text((20,12),"LABORATORIO: mesmas artes, quadro 30 / 60 / 96 px; ampliacao limitada para inspecao",font=font,fill="white")
d.text((20,35),"Point (superior) e Lanczos (inferior, aproxima reducao suave). Nao e captura do jogo.",font=font,fill="#bbd2db")
for j,(name,im) in enumerate(samples):
    if im.width > im.height and im.width % im.height == 0:
        im=im.crop((0,0,im.height,im.height))
    y=70+j*230
    d.text((15,y),name,font=font,fill="white")
    for k,height in enumerate([30,60,96]):
        x=165+k*285
        # Primeiro reduzir para a altura indicada; a ampliação seguinte só
        # permite inspecionar quais detalhes sobreviveram na tela simulada.
        target=(max(1,round(im.width*height/im.height)),height)
        for line,filt in enumerate([Image.Resampling.NEAREST,Image.Resampling.LANCZOS]):
            small=im.resize(target,filt)
            preview=small.resize((small.width*2,small.height*2),Image.Resampling.NEAREST)
            preview.thumbnail((230,96),Image.Resampling.NEAREST)
            canvas.paste(preview,(x,y+22+line*100),preview)
        d.text((x,y),str(height)+"px quadro",font=font,fill="#d5e8e7")
canvas.save(OUT/"comparativo.png")
summary=[]
for r in rows:
    if Path(r["file"]).name in ["south.png","idle_south.png"] and r["frames"]:
        f=r["frames"][0]
        summary.append({"file":r["file"],"canvas":r["canvas"],"bbox":f["bbox"],"heightRatio":f["heightRatio"],"bottomMargin":f["bottomMargin"],"import":r["import"]})
print(json.dumps({"filesScanned":len(rows),"idleSouth":summary},ensure_ascii=False))
