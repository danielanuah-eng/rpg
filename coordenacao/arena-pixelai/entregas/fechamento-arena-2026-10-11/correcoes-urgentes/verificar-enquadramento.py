"""Valida os quadros realmente baixados depois de salvar no PixelLab."""
from pathlib import Path
from PIL import Image, ImageDraw
import io, json, re, zipfile
import numpy as np
ROOT=Path(__file__).resolve().parent
rows=json.loads((ROOT/'inspecao-fontes.json').read_text(encoding='utf-8'))
checked=[]; targeted=set(); unchanged=0
preview=Image.new('RGB',(840,7*155),(27,31,44)); label=ImageDraw.Draw(preview)
for n,row in enumerate(rows):
    actor=row['actor'];dx,dy=row['centralizarDelta'];boxes=[]
    with zipfile.ZipFile(ROOT/'fontes'/f'{actor}-antes.zip') as before, zipfile.ZipFile(ROOT/'fontes'/f'{actor}-depois.zip') as after:
        files=sorted([p for p in before.namelist() if '/animations/' in '/'+p and f"/{row['action']}/" in p and f"/{row['direction']}/" in p and p.endswith('.png')],key=lambda p:int(re.search(r'\d+$',Path(p).stem).group()))
        assert len(files)==row['frames']
        label.text((8,n*155+4),f"{actor} | {row['action']} | {row['direction']}",fill=(226,233,243))
        for i,p in enumerate(files):
            source=Image.open(io.BytesIO(before.read(p))).convert('RGBA')
            actual=Image.open(io.BytesIO(after.read(p))).convert('RGBA')
            expected=Image.new('RGBA',source.size);expected.paste(source,(dx,dy))
            assert actual.size==source.size
            assert np.array_equal(np.array(expected),np.array(actual)),f'Pixels alterados alem do deslocamento: {actor}/{p}'
            a=np.array(actual)[:,:,3];b=actual.getchannel('A').getbbox()
            assert a.min()==0 and a.max()==255
            assert b[0]>0 and b[1]>0 and b[2]<actual.width and b[3]<actual.height
            assert np.count_nonzero(a)==np.count_nonzero(np.array(source)[:,:,3])
            boxes.append(list(b));targeted.add((actor,p))
            display=actual.copy();display.thumbnail((90,120),Image.Resampling.NEAREST)
            preview.paste(display,(10+i*91,n*155+25),display)
        checked.append({**row,'bboxAfter':boxes,'pixelAlphaIdenticalAfterConstantTranslation':True,'remoteSaveVerified':True,'status':'enquadramento_salvo; avaliar_exibicao_no_cliente'})
for actor in ('baltazar','guardiao-de-musgo'):
    with zipfile.ZipFile(ROOT/'fontes'/f'{actor}-antes.zip') as before,zipfile.ZipFile(ROOT/'fontes'/f'{actor}-depois.zip') as after:
        for p in before.namelist():
            if p.endswith('.png') and (actor,p) not in targeted:
                assert before.read(p)==after.read(p),f'Mudanca fora do escopo: {actor}/{p}'
                unchanged+=1
report={'status':'passed','clips':len(checked),'frames':sum(r['frames'] for r in checked),'otherPngsUnchanged':unchanged,'generationCost':0,'sourceClippingObserved':False,'limitation':'As fontes nao tocavam a borda. Centralizacao maximiza margens mantendo canvas. Nao comprova correcao de corte produzido pelo visor/importador. Avaliar no cliente.','checks':checked,'integration':'pendente_claude'}
(ROOT/'verificacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
preview.save(ROOT/'contato-enquadramento.png')
print(json.dumps({k:v for k,v in report.items() if k!='checks'},ensure_ascii=False))
