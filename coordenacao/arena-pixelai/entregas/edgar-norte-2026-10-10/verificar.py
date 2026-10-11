"""Confere pixels entregues pelo PixelLab e empacota os quadros sem retoques."""
from pathlib import Path
from PIL import Image, ImageDraw
import io, json, hashlib, zipfile

ROOT = Path(__file__).resolve().parent
def png(z, n):
    return Image.open(io.BytesIO(z.read(n))).convert('RGBA')
def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()
def alpha(im):
    h = im.getchannel('A').histogram()
    return {'transparent': h[0], 'opaque': h[255], 'partial': sum(h[1:255])}
def main():
    before = zipfile.ZipFile(ROOT/'fontes/edgar-antes.zip')
    after = zipfile.ZipFile(ROOT/'fontes/edgar-depois.zip')
    plan = json.loads((ROOT/'decisoes.json').read_text(encoding='utf-8'))
    out = ROOT/'sprites'; out.mkdir(exist_ok=True)
    checked, patched = [], set()
    for item in plan:
        action, direction = item['key'].split('_', 1)
        prefix = f'Idle/rotations/{direction}.png' if action == 'rotation' else f'Idle/animations/{action}/{direction}/'
        names = [prefix] if action == 'rotation' else sorted(n for n in before.namelist() if n.startswith(prefix) and n.endswith('.png'))
        frames = []
        for change, name in zip(item['changes'], names, strict=True):
            old, delivered = png(before, name), png(after, name)
            expected = old.copy()
            for x, y, previous, new in change['pixels']:
                rgba = tuple(bytes.fromhex(previous[1:]))
                assert old.getpixel((x, y)) == rgba, (name, x, y, 'source guard')
                expected.putpixel((x, y), tuple(bytes.fromhex(new[1:])))
            assert expected.size == delivered.size and expected.tobytes() == delivered.tobytes(), (name, 'delivery differs from guarded changes')
            assert old.getchannel('A').tobytes() == delivered.getchannel('A').tobytes(), (name, 'alpha changed')
            frames.append(delivered); patched.add(name)
        w, h = frames[0].size
        sheet = Image.new('RGBA', (w*len(frames), h))
        for i, frame in enumerate(frames): sheet.paste(frame, (i*w, 0))
        dest = out/f"edgar-{item['key']}.png"; sheet.save(dest)
        checked.append({'key': item['key'], 'file': dest.relative_to(ROOT).as_posix(), 'frames': len(frames), 'frameSize': [w,h], 'layout': 'horizontal', 'sha256': sha(dest), 'alpha': alpha(sheet), 'pixelsChanged': sum(len(c['pixels']) for c in item['changes'])})
    unchanged = []
    for n in before.namelist():
        if not n.endswith('.png') or n in patched: continue
        assert n in after.namelist(), ('missing original', n)
        assert png(before,n).tobytes() == png(after,n).tobytes(), ('unrequested modification', n)
        unchanged.append(n)
    report = {'status': 'passed', 'framesCorrected': len(patched), 'assets': checked, 'otherFramesUnchanged': len(unchanged), 'addedFrames': [n for n in after.namelist() if n.endswith('.png') and n not in before.namelist()], 'integration': 'pendente_claude', 'checks': ['exact guarded pixel changes', 'identical alpha', 'original frame dimensions and order', 'other original frames unchanged']}
    (ROOT/'verificacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    gallery = Image.new('RGB', (960, 680), '#202632'); draw = ImageDraw.Draw(gallery)
    for i, direction in enumerate(('north-west','north','north-east')):
        for j, z in enumerate((before,after)):
            im=png(z,f'Idle/rotations/{direction}.png').resize((256,256), Image.Resampling.NEAREST)
            gallery.paste(im,(i*320+32,j*330+50),im)
            draw.text((i*320+32,j*330+20), f"{direction} / {'antes' if j==0 else 'corrigido'}",fill='white')
    gallery.save(ROOT/'edgar-antes-depois.png')
    print(json.dumps({'status':report['status'],'frames':len(patched),'otherFramesUnchanged':len(unchanged),'addedFrames':len(report['addedFrames'])}))
if __name__ == '__main__': main()
