"""Prepara decisões de pixels; o PixelLab aplica as alterações e preserva as fontes."""
from pathlib import Path
from PIL import Image
import io, json, zipfile

ROOT = Path(__file__).resolve().parent
UUID = '590cc7bc-3673-477c-aa23-f5655dd2d97a'
GROUPS = {'idle': 'e0606797-2c10-485a-ba3f-9f1eeb12efe4',
          'walk': 'eee6dcd4-c73f-432a-acdd-9a8e73173fb8',
          'attack': '5a6a2513-0fcb-4a25-88dc-2e775c03fef2'}

def cyan(p):
    r, g, b, a = p
    return a > 0 and ((r < 170 and g > 160 and b > 150 and g-r > 35) or (b-r > 20 and b-g > 8))

def warm(p):
    r, g, b, a = p
    return a > 0 and r > 140 and g > 75 and r-g > 17 and b < r

def hx(p):
    return '#' + ''.join(f'{v:02x}' for v in p)

def decision(im, frame):
    im = im.convert('RGBA')
    w, h = im.size
    pix = im.load()
    # A coroa é a primeira superfície sólida no terço central do personagem.
    occupied = [(x, y) for y in range(h) for x in range(int(w*.28), int(w*.72))
                if pix[x,y][3] > 0 and not cyan(pix[x,y])]
    top = min(y for x,y in occupied)
    cap = [(x,y) for y in range(top, min(h, top+10)) for x in range(w) if warm(pix[x,y])]
    if not cap:
        raise ValueError(('coroa sem pele observada', frame))
    left, right = min(x for x,y in cap), max(x for x,y in cap)
    center = (left+right)//2
    patches = []
    for x,y in cap:
        old = pix[x,y]
        # Pequena risca mantém a idade/recuo; não deixa a cúpula inteira exposta.
        if x == center and top+2 <= y <= top+3:
            continue
        band = ((x-center) + (y-top)//2) % 5
        grey = (200,201,206) if band <= 1 else (164,164,177)
        if old[0] < 190:
            grey = (129,129,141)
        new = (*grey, old[3])
        if new != old:
            patches.append([x,y,hx(old),hx(new)])
    cranial_cyan = []
    # Apenas a coroa e a testa superior; olhos e energia das mãos ficam protegidos.
    for y in range(top, min(h, top+24)):
        for x in range(max(0,left-2),min(w,right+3)):
            old = pix[x,y]
            if not cyan(old):
                continue
            nearby = sorted([(abs(xx-x)+abs(yy-y),pix[xx,yy])
                             for yy in range(max(0,y-6),min(h,y+7))
                             for xx in range(max(0,x-6),min(w,x+7))
                             if warm(pix[xx,yy])], key=lambda v:v[0])
            new = (200,201,206,old[3]) if y < top+10 else nearby[0][1] if nearby else (0,0,0,0)
            patches.append([x,y,hx(old),hx(new)])
            cranial_cyan.append([x,y])
    return {'operation':'patch_pixels','layer':'image','frames':[frame],
            'pixels':patches,
            'reason':'Coroa observada totalmente calva: cabelo grisalho curto e rente, sem mudar silhueta, pose ou roupa; remover apenas artefato ciano craniano.'}, {
                'frame':frame,'top':top,'capBounds':[left,top,right,top+9],
                'hairPixels':len(patches)-len(cranial_cyan),'cranialCyan':cranial_cyan}

def main():
    z = zipfile.ZipFile(ROOT/'fontes/edgar-antes.zip')
    batches = {}
    for n in z.namelist():
        p = Path(n).parts
        if p[0] != 'Idle' or not n.endswith('.png'):
            continue
        if p[1] == 'rotations' and Path(n).stem in ('north','north-east','north-west'):
            key = ('rotation',Path(n).stem)
        elif p[1] == 'animations' and p[3].startswith('north') and p[2] in GROUPS:
            key = (p[2],p[3])
        else:
            continue
        batches.setdefault(key,[]).append(n)
    result = []
    for (action,direction), names in sorted(batches.items()):
        changes, metrics = [], []
        for i,n in enumerate(sorted(names)):
            change, info = decision(Image.open(io.BytesIO(z.read(n))), i+1)
            changes.append(change); metrics.append(info)
        target = f'character:{UUID}:{direction}'
        if action != 'rotation':
            target += ':' + GROUPS[action]
        result.append({'key':action+'_'+direction,'source':target,'target':target,
                       'changes':changes,'metrics':metrics,
                       'originalZipEntries':sorted(names)})
    (ROOT/'decisoes.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
    palette = sorted({p[2] for r in result for c in r['changes'] for p in c['pixels']} |
                     {p[3] for r in result for c in r['changes'] for p in c['pixels']})
    compressed = []
    for r in result:
        frames = []
        for c,m in zip(r['changes'],r['metrics']):
            cyan_positions = {tuple(p) for p in m['cranialCyan']}
            hairs = sorted((p for p in c['pixels'] if tuple(p[:2]) not in cyan_positions), key=lambda p:(p[1],p[0]))
            runs = []
            for x,y,old,new in hairs:
                index = palette.index(old)
                if runs and runs[-1][1] == y and runs[-1][2] == index and runs[-1][0]+runs[-1][3] == x:
                    runs[-1][3] += 1
                else:
                    runs.append([x,y,index,1])
            ghosts = [[p[0],p[1],palette.index(p[2]),palette.index(p[3])]
                      for p in c['pixels'] if tuple(p[:2]) in cyan_positions]
            frames.append({'f':c['frames'][0],'top':m['top'],
                           'center':(m['capBounds'][0]+m['capBounds'][2])//2,
                           'runs':runs,'ghosts':ghosts})
        compressed.append({'key':r['key'],'source':r['source'],'target':r['target'],'frames':frames})
    (ROOT/'decisoes-compactas.json').write_text(json.dumps({'palette':palette,'batches':compressed},separators=(',',':')),encoding='utf-8')
    print(json.dumps([{'key':r['key'],'frames':len(r['changes']),
                      'pixels':sum(len(c['pixels']) for c in r['changes']),
                      'cyanRemoved':sum(len(m['cranialCyan']) for m in r['metrics'])}
                     for r in result],ensure_ascii=False))

if __name__ == '__main__':
    main()
