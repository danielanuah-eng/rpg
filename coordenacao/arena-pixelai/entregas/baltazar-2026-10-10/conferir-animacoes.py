"""Folhas de conferência do bundle nativo completo, sem editar os sprites."""
from pathlib import Path
from PIL import Image, ImageDraw
import zipfile,io,json
ROOT=Path(__file__).resolve().parent
selection=json.loads((ROOT/'selecao.json').read_text(encoding='utf-8')) if (ROOT/'selecao.json').exists() else {}
with zipfile.ZipFile(ROOT/'baltazar-completo.zip') as z:
    for action in ('walk','idle','attack'):
        groups={}
        for n in z.namelist():
            p=Path(n).parts
            if len(p)>=5 and p[1]=='animations' and n.endswith('.png'):
                if any(e['action']==p[2] and e['direction']==p[3] for e in selection.get('excludedSources',[])):continue
                if selection.get('actionAliases',{}).get(p[2],p[2])==action:groups.setdefault(p[3],[]).append(n)
        if not groups:continue
        rows=sorted(groups);cols=max(len(groups[r]) for r in rows)
        gallery=Image.new('RGB',(cols*180,len(rows)*200),'#202632');d=ImageDraw.Draw(gallery)
        for row,direction in enumerate(rows):
            for i,n in enumerate(sorted(groups[direction])):
                im=Image.open(io.BytesIO(z.read(n))).convert('RGBA')
                im.thumbnail((176,176),Image.Resampling.NEAREST)
                gallery.paste(im,(i*180,row*200+20),im)
                d.text((i*180+4,row*200+3),f'{direction} / {i}',fill='white')
        gallery.save(ROOT/f'{action}-conferencia.png')
print('CONFERENCIA_LOCAL_OK')
