from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np, json, hashlib, shutil, re

root=Path(__file__).resolve().parents[4]
src=root/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites'
out=Path(__file__).resolve().parent
dirs=['north-east','north-west','south-east','south-west']
names=['Damian','Ciantia','Talyssa','Teantha','Devon','Diantus','DragaoEspectral','Ezequiela','TchuTcha','TchuTchaBerserk','ArqueiroJack','Perry']
index={'source':str(src),'characters':{},'sheets':[],'totals':{'rotations':0,'clips':0,'frames':0}}
for name in names:
    local=src/name
    snapshot=out/'fontes'/name
    snapshot.mkdir(parents=True,exist_ok=True)
    clips={}; rotations={}
    for path in sorted(local.glob('*.png')):
        shutil.copy2(path,snapshot/path.name)
        if path.with_suffix('.png.meta').exists(): shutil.copy2(path.with_suffix('.png.meta'),snapshot/(path.name+'.meta'))
        im=Image.open(snapshot/path.name).convert('RGBA')
        w,h=im.size
        if path.stem in ['north','north-east','east','south-east','south','south-west','west','north-west']:
            rotations[path.stem]=path.name; index['totals']['rotations']+=1; continue
        n=w//h if w>h else 1
        frames=[]
        arrays=[]
        for i in range(n):
            fr=im.crop((i*h,0,(i+1)*h,h)); a=np.array(fr); arrays.append(a)
            mask=a[:,:,3]>40
            yy,xx=np.nonzero(mask)
            bbox=fr.getbbox()
            foot=None
            if len(yy):
                bottom=yy.max(); last=xx[yy>=bottom-2]
                foot=[round(float(np.median(last)),2),int(bottom)]
            frames.append({'frame':i+1,'bbox':bbox,'pixels':int(mask.sum()),'foot':foot,'alpha_zero':int((a[:,:,3]==0).sum()),'edge_pixels':int(mask[0].sum()+mask[-1].sum()+mask[:,0].sum()+mask[:,-1].sum())})
        changes=[]
        for i in range(n):
            a,b=arrays[i],arrays[(i+1)%n]; ma=a[:,:,3]>40; mb=b[:,:,3]>40; u=ma|mb
            changes.append({'from':i+1,'to':(i+1)%n+1,'silhouette_iou':round(float((ma&mb).sum()/max(1,u.sum())),4),'changed_rgba':int(np.any(a!=b,axis=2).sum()),'identical':bool(np.array_equal(a,b))})
        m=re.match(r'(.+)_(north-east|north-west|south-east|south-west)$',path.stem)
        clip,di=m.groups() if m else (path.stem,'unknown')
        key=clip+'/'+di
        clips[key]={'file':str(snapshot/path.name),'relative':str(path.relative_to(root)),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'canvas':[w,h],'cell':h,'count':n,'frames':frames,'changes':changes}
        index['totals']['clips']+=1; index['totals']['frames']+=n
    index['characters'][name]={'rotations':rotations,'clips':clips}
    for di in dirs:
        rows=[(k,v) for k,v in clips.items() if k.endswith('/'+di)]
        cell=145 if name=='Damian' else 158
        cols=max([v['count'] for _,v in rows]+[1])
        width=cols*cell+100; height=(len(rows)+1)*(cell+20)+35
        sheet=Image.new('RGB',(width,height),(38,38,43)); draw=ImageDraw.Draw(sheet)
        draw.text((8,8),name+' / '+di+' - ALL frames; nearest-neighbor analysis thumbnails',(255,255,255))
        def paste_frame(fr,x,y,label):
            size=min(cell-8,fr.height)
            fr=fr.resize((size,size),Image.Resampling.NEAREST)
            tile=Image.new('RGBA',(cell,cell),(66,64,72,255))
            tile.alpha_composite(fr,((cell-size)//2,(cell-size)//2))
            sheet.paste(tile.convert('RGB'),(x,y))
            draw.text((x+4,y+cell+2),label,(235,235,235))
        base=snapshot/rotations.get(di,'missing.png')
        if base.exists(): paste_frame(Image.open(base).convert('RGBA'),100,35,'base '+di)
        draw.text((3,35),'BASE',(235,235,235))
        for r,(k,v) in enumerate(rows,1):
            im=Image.open(v['file']).convert('RGBA'); h=v['cell']; y=35+r*(cell+20)
            draw.text((3,y),k.split('/')[0],(255,220,140))
            for i in range(v['count']): paste_frame(im.crop((i*h,0,(i+1)*h,h)),100+i*cell,y,f'{i+1} ({h}px)')
        sheetpath=out/'folhas'/name/(di+'.png'); sheetpath.parent.mkdir(parents=True,exist_ok=True); sheet.save(sheetpath)
        index['sheets'].append({'character':name,'direction':di,'path':str(sheetpath),'size':list(sheet.size),'clips':[k for k,_ in rows]})
out.joinpath('indice.json').write_text(json.dumps(index,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'totals':index['totals'],'sheets':len(index['sheets']),'characters':{n:len(x['clips']) for n,x in index['characters'].items()}},ensure_ascii=False))
