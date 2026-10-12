"""Empacota pixels oficiais sem reamostragem; mantém fontes e pivôs auditáveis."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, io, json, math, zipfile

ROOT = Path(__file__).resolve().parent
OUT = ROOT / 'unity'
ENTRIES = []
ANCHORS = json.loads((ROOT/'efeitos/ancoras-bibliotecas.json').read_text(encoding='utf-8'))

def save_json(path, obj):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(obj, ensure_ascii=False, indent=2)+'\n', encoding='utf-8', newline='\n')

def unpack(pack_id, name='whole'):
    with zipfile.ZipFile(ROOT/f'efeitos/fontes/{pack_id}.zip') as z:
        manifest = json.loads(z.read('manifest.json'))
        clip = next(c for c in manifest['clips'] if c['name']==name)
        frames=[]
        fw,fh=clip['frame']['w'],clip['frame']['h']
        for sheet in clip['sheets']:
            im=Image.open(io.BytesIO(z.read(sheet['file']))).convert('RGBA')
            for i in range(sheet['frames']):
                x=(i%sheet['columns'])*fw; y=(i//sheet['columns'])*fh
                frames.append(im.crop((x,y,x+fw,y+fh)))
        anchor=manifest['anchor']; offset=clip['offset']
        point=[anchor['x'],anchor['y']]
        if pack_id.startswith('lib-'):
            point=ANCHORS[pack_id[4:]][name.removeprefix('part-')]
        relative=(offset['x']+anchor['x']-point[0],offset['y']+anchor['y']-point[1])
        return frames, relative, manifest['fps'], clip, manifest

def glyph(pack_id):
    return unpack('glyph-'+pack_id)

def normalize(frames, offset, bounds=None):
    fw,fh=frames[0].size
    x,y=offset
    if bounds is None: bounds=(min(x,0),min(y,0),max(x+fw,0),max(y+fh,0))
    x0,y0,x1,y1=bounds
    size=math.ceil((max(x1-x0,y1-y0)+16)/8)*8
    point=(8-x0,8-y0)
    out=[]
    for frame in frames:
        canvas=Image.new('RGBA',(size,size))
        canvas.paste(frame,(point[0]+x,point[1]+y))
        out.append(canvas)
    return out,point

def trim(frames):
    last=len(frames)
    while last>1 and not frames[last-1].getchannel('A').getbbox(): last-=1
    return frames[:last]

def emit(resource, frames, offset, fps, loop=False, source='', bounds=None, point=None, **extra):
    if point is None: frames,point=normalize(frames,offset,bounds)
    if not loop: frames=trim(frames)
    assert len(frames)>0 and all(f.size==frames[0].size for f in frames)
    n=frames[0].width
    assert frames[0].height==n
    path=OUT/(resource+'.png'); path.parent.mkdir(parents=True,exist_ok=True)
    strip=Image.new('RGBA',(n*len(frames),n))
    for i,f in enumerate(frames): strip.paste(f,(i*n,0))
    strip.save(path,optimize=True)
    opaque=[sum(v>0 for v in f.getchannel('A').get_flattened_data()) for f in frames]
    impact=max(range(len(frames)),key=lambda i:opaque[i])
    piv=[point[0]/n,1-point[1]/n]
    entry={'resource':resource,'file':str(path.relative_to(ROOT)).replace('\\','/'),'frameSize':n,'frames':len(frames),'fps':fps,'loop':loop,'peakFrame0':impact,'impactFrame0':6 if resource.endswith('martelo-perfurante/uso') else impact,'anchorPixelsTopLeft':list(point),'unityPivot':piv,'pixelsPerUnit':128,'source':source,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),**extra}
    ENTRIES.append(entry)
    # Preview GIF somente: a fonte de integração é a tira RGBA, sem redução de cores.
    preview=ROOT/'previews'/resource
    preview.parent.mkdir(parents=True,exist_ok=True)
    gif=[]
    for f in frames:
        bg=Image.new('RGBA',f.size,(21,27,36,255)); bg.alpha_composite(f)
        gif.append(bg.convert('RGB'))
    gif[0].save(preview.with_suffix('.gif'),save_all=True,append_images=gif[1:],duration=round(1000/fps),loop=0 if loop else 1,optimize=False)
    return entry

def native(pack, part, resource, loop=False, **extra):
    fr,off,fps,clip,m=unpack(pack,'part-'+part)
    return emit(resource,fr,off,fps,loop,source=pack+':'+part,**extra)

def combined(first,second,resource,loop=False):
    a,ao,af,_,_=first; b,bo,bf,_,_=second
    x0=min(ao[0],bo[0],0); y0=min(ao[1],bo[1],0)
    x1=max(ao[0]+a[0].width,bo[0]+b[0].width,0); y1=max(ao[1]+a[0].height,bo[1]+b[0].height,0)
    size=math.ceil((max(x1-x0,y1-y0)+16)/8)*8; point=(8-x0,8-y0)
    out=[]; count=max(len(a),len(b))
    for i in range(count):
        im=Image.new('RGBA',(size,size))
        if i<len(a): im.alpha_composite(a[i],(point[0]+ao[0],point[1]+ao[1]))
        if i<len(b): im.alpha_composite(b[i],(point[0]+bo[0],point[1]+bo[1]))
        out.append(im)
    return emit(resource,out,(0,0),max(af,bf),loop,source='composição declarada de duas peças PixelLab',point=point)

def timeline(pack):
    frames,off,fps,clip,m=unpack(pack)
    full=[Image.new('RGBA',(m['canvas']['w'],m['canvas']['h'])) for _ in range(24)]
    for i,f in enumerate(frames):
        index=i+clip['start_frame']
        if index<24: full[index].paste(f,(m['anchor']['x']+off[0],m['anchor']['y']+off[1]))
    point=(m['anchor']['x'],m['anchor']['y'])
    for name,ix,loop in [('inicio',0,False),('loop',8,True),('fim',16,False)]:
        emit(f'Sprites/Efeitos/{pack}/{name}',full[ix:ix+8],(0,0),fps,loop,source=pack+':timeline',point=point)

def repairs():
    specs=[('baltazar-idle-west-v2','Baltazar/idle_west',4),('baltazar-walk-west-v2','Baltazar/walk_west',6),('guardiao-idle-north-west-v2','GuardiaoMusgo/idle_north-west',4)]
    for folder,resource,count in specs:
        fr=[Image.open(ROOT/f'reparos/{folder}/originais/{i}.png').convert('RGBA') for i in range(1,count+1)]
        w,h=fr[0].size; side=max(w,h); dx=(side-w)//2
        out=[]
        for im in fr:
            canvas=Image.new('RGBA',(side,side));canvas.paste(im,(dx,0));out.append(canvas)
        # Só um deslocamento comum: não estabiliza cada frame nem redesenha a caminhada.
        foot=max(f.getchannel('A').getbbox()[3] for f in out)
        emit('Sprites/'+resource,out,(0,0),10,True,source=folder,point=(side//2,foot),removedReferenceFrame0=True,sourceIndices=list(range(1,count+1)),nativeFrameSize=[w,h])

def events():
    for pack in ['mare-de-lava','chuva-de-moedas','queda-de-pedras']: timeline(pack)
    for pack in ['eco-ancestral','portal-caotico','mare-de-energia','vendaval','terremoto','festival-de-fogos','nevoeiro']:
        records=[unpack(pack,'part-'+name) for name in ['inicio','loop','fim']]
        bounds=(min(0,*[r[1][0] for r in records]),min(0,*[r[1][1] for r in records]),max(0,*[r[1][0]+r[0][0].width for r in records]),max(0,*[r[1][1]+r[0][0].height for r in records]))
        for name,r in zip(['inicio','loop','fim'],records):
            emit(f'Sprites/Efeitos/{pack}/{name}',r[0],r[1],r[2],name=='loop',source=pack+':'+name,bounds=bounds,repeatPulse=pack=='terremoto' and name=='loop')
    # Só a explosão, sem tocar no foguete de lançamento.
    fr,off,fps,clip,_=unpack('festival-de-fogos','part-explosao')
    emit('Sprites/Efeitos/festival-de-fogos/explosao',fr[5:],off,fps,source='festival-de-fogos:explosao, sem os primeiros cinco quadros')

def libraries():
    for lid in ANCHORS:
        with zipfile.ZipFile(ROOT/f'efeitos/fontes/lib-{lid}.zip') as z: m=json.loads(z.read('manifest.json'))
        for c in m['clips']:
            if not c['name'].startswith('part-'):continue
            part=c['name'][5:]
            fr,off,fps,_,_=unpack('lib-'+lid,c['name'])
            emit(f'Biblioteca/{lid}/{part}',fr,off,fps,loop=lid in ['estados','projeteis'],source='lib-'+lid+':'+part)

def bindings():
    # As artes de conjuração são overlays. Não mudam animações corporais de outros atores.
    impacts={'pisao':('golpes','pisao'),'pancada-colossal':('golpes','pancada'),'gancho':('golpes','gancho'),'coroada':('golpes','coroada'),'pauzinho':('golpes','pauzinho'),'ferrao':('natureza','ferrao'),'agulha':('natureza','agulha'),'casca':('natureza','casca'),'raizes':('natureza','raizes'),'grito-de-comando':('natureza','grito'),'clava':('golpes','clava'),'arremessar-pedra':('golpes','pedra'),'pancada-do-errante':('golpes','errante'),'rugido':('golpes','rugido'),'bola-de-fogo':('magias','fogo'),'chao-de-gelo':('magias','gelo'),'prisao-dos-pesadelos':('magias','prisao'),'noite-dos-gatos':('magias','noite'),'tentaculo-de-oleo':('magias','oleo'),'canhao-de-peito':('magias','canhao'),'gancho-broca':('golpes','gancho'),'aquecer-motor':('conjuracao','fogo'),'faisca-da-varinha':('conjuracao','magia')}
    for col in ['laranja','branca','malhada','manchada']: impacts['faisca-'+col]=('magias',col)
    for skill,(lib,part) in impacts.items():
        native('lib-'+lib,part,f'Sprites/Efeitos/{skill}/impacto')
        style='fisico' if lib=='golpes' else 'natureza' if lib=='natureza' else 'fogo' if skill in ['bola-de-fogo','canhao-de-peito','aquecer-motor'] else 'cura' if skill in ['faisca-branca','noite-dos-gatos'] else 'magia'
        native('lib-conjuracao',style,f'Sprites/Efeitos/{skill}/conjuracao',casterOverlay=True)
    fr,off,fps,_,_=glyph('corrente-fisica')
    emit('Sprites/Efeitos/corrente/impacto',fr,off,fps,source='glyph-corrente-fisica')
    native('lib-conjuracao','metal','Sprites/Efeitos/corrente/conjuracao',casterOverlay=True)
    for part,id in [('escondido','escondido'),('preso','preso'),('sem_pm','sem-pm'),('armadura','armadura'),('desejo','desejo-canalizando'),('buff','buff-pa-pm')]:
        fr,off,fps,_,_=unpack('lib-estados','part-'+part)
        if part=='sem_pm':
            mid=fr[6:18]; fr=mid+mid[-2:0:-1]
        emit(f'Sprites/Efeitos/Estados/{id}/loop',fr,off,fps,True,source='lib-estados:'+part)
    for part,id in [('reviver','a'),('cura','b'),('pa','c'),('copiar','d')]:native('lib-desejos',part,f'Sprites/Efeitos/desejo-{id}/impacto')
    for id,pack in [('e','desejo-itens'),('f','desejo-apagar')]:
        fr,off,fps,_,_=glyph(pack);emit(f'Sprites/Efeitos/desejo-{id}/impacto',fr,off,fps,source='glyph-'+pack)
    native('lib-estados','desejo','Sprites/Efeitos/desejo/inicio',True)
    fr,off,fps,_,_=glyph('devorar-essencia');emit('Sprites/Efeitos/devorar/impacto',fr,off,fps,source='glyph-devorar-essencia',moveFromTargetToCaster=True)
    for id,part in [('bola-de-fogo','fogo'),('faisca-laranja','laranja'),('faisca-branca','branca'),('faisca-malhada','malhada'),('faisca-manchada','manchada'),('canhao-de-peito','canhao')]:
        native('lib-projeteis',part,f'Sprites/ProjeteisAnimados/{id}',True,facing='right')
        fr,off,fps,_,_=unpack('lib-projeteis','part-'+part)
        emit(f'Sprites/Projeteis/{id}',[fr[0]],off,fps,source='lib-projeteis:'+part,staticProjectile=True,facing='right')
    uses={'bomba-de-fumaca':'fumaca','totem-de-cura':'totem','mina-de-gelo':'mina','cristal-de-foco':'foco','manto-de-espinhos':'espinhos','bencao-de-pedra':'bencao'}
    for id,part in uses.items():native('lib-loja',part,f'Sprites/Efeitos/{id}/uso',id=='totem-de-cura')
    combined(unpack('lib-loja','part-martelo'),glyph('martelo-cristal'),'Sprites/Efeitos/martelo-perfurante/uso')
    combined(unpack('lib-loja','part-fenix'),glyph('pena-fenix'),'Sprites/Efeitos/pena-de-fenix/uso')
    for id,pack in [('ancora','ancora-fantasma'),('corrente-de-ferro','corrente-fisica')]:
        fr,off,fps,_,_=glyph(pack);emit(f'Sprites/Efeitos/{id}/uso',fr,off,fps,source='glyph-'+pack)

def objects():
    for pack in ['bau-recompensa','prisao-dos-pesadelos','portal-de-rocha','buraco-papa-moedas','casa-resfriada','casa-congelada','mina-de-gelo-armada']:
        fr,off,fps,clip,m=unpack('obj-'+pack)
        full=[Image.new('RGBA',(160,160)) for _ in range(24)]
        for i,f in enumerate(fr):
            idx=i+clip['start_frame']
            if idx<24: full[idx].paste(f,(m['anchor']['x']+off[0],m['anchor']['y']+off[1]))
        point=(m['anchor']['x'],m['anchor']['y'])
        for name,start,loop in [('surgir',0,False),('idle',8,True),('quebrar',16,False)]:
            emit(f'Sprites/Objetos/{pack}/{name}',full[start:start+8],(0,0),fps,loop,source='obj-'+pack,point=point,ownerOnly=pack=='mina-de-gelo-armada')
    # Arte existente do Totem: uso visual, sem refazer outro personagem.
    repo=ROOT.parents[3]
    spr=repo/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/TotemCura'
    for dest,clip in [('idle','idle_south-east'),('surgir','spawn_south-east'),('quebrar','death_south-east')]:
        im=Image.open(spr/(clip+'.png')).convert('RGBA');n=im.height
        assert im.width%n==0
        frames=[im.crop((i*n,0,(i+1)*n,n)) for i in range(im.width//n)]
        bottom=max(f.getchannel('A').getbbox()[3] for f in frames if f.getchannel('A').getbbox())
        emit(f'Sprites/Objetos/totem-de-cura/{dest}',frames,(0,0),10,dest=='idle',source='arte existente TotemCura/'+clip,point=(n//2,bottom),reusedActorArt=True)
    for src,dest in [('mare-de-lava','casa-de-lava'),('festival-de-fogos','marcador-de-fogos')]:
        for a,b in [('inicio','surgir'),('loop','idle'),('fim','sumir')]:
            if src=='mare-de-lava':
                r=next(e for e in ENTRIES if e['resource']==f'Sprites/Efeitos/{src}/{a}')
                im=Image.open(ROOT/r['file']).convert('RGBA');n=r['frameSize']
                fr=[im.crop((i*n,0,(i+1)*n,n)) for i in range(r['frames'])]
                emit(f'Sprites/Objetos/{dest}/{b}',fr,(0,0),r['fps'],b=='idle',source='reuso '+r['resource'],point=tuple(r['anchorPixelsTopLeft']))
            else:native(src,a,f'Sprites/Objetos/{dest}/{b}',b=='idle')
    fr,off,fps,_,_=unpack('obj-aura-gatinho')
    emit('Sprites/Efeitos/aura-gatinho/loop',fr,off,fps,True,source='obj-aura-gatinho')
    emit('Sprites/Efeitos/aura-de-gatinho/loop',fr,off,fps,True,source='obj-aura-gatinho')
    emit('Sprites/Efeitos/aura-de-gatinho/impacto',fr,off,fps,False,source='obj-aura-gatinho')
    native('lib-conjuracao','cura','Sprites/Efeitos/aura-de-gatinho/conjuracao',casterOverlay=True)
    native('portal-caotico','inicio','Sprites/Efeitos/chamar-gato/impacto')
    native('lib-conjuracao','magia','Sprites/Efeitos/chamar-gato/conjuracao',casterOverlay=True)
    emit('Sprites/Efeitos/mover-gato/impacto',fr,off,fps,False,source='obj-aura-gatinho')
    native('lib-conjuracao','magia','Sprites/Efeitos/mover-gato/conjuracao',casterOverlay=True)
    native('lib-golpes','pancada','Sprites/Efeitos/baltazar-martelo/impacto')
    native('lib-conjuracao','metal','Sprites/Efeitos/baltazar-martelo/conjuracao',casterOverlay=True)
    fr_oil,off_oil,fps_oil,_,_=unpack('lib-magias','part-oleo')
    middle=fr_oil[4:11];fr_oil=middle+middle[-2:0:-1]
    emit('Sprites/Efeitos/Estados/sem-pm-oleo/loop',fr_oil,off_oil,fps_oil,True,source='lib-magias:oleo, fase central reversível',inheritTargetVisibility=True)
    emit('Sprites/Efeitos/noite-dos-gatos/aura',fr,off,fps,True,source='obj-aura-gatinho')
    # Aliases exatos esperados pelos efeitos de terreno já presentes no motor.
    for src,dst in [('casa-de-lava','lava'),('casa-congelada','gelo'),('marcador-de-fogos','fogos')]:
        r=next(e for e in ENTRIES if e['resource']==f'Sprites/Objetos/{src}/idle')
        im=Image.open(ROOT/r['file']).convert('RGBA');n=r['frameSize']
        fr=[im.crop((i*n,0,(i+1)*n,n)) for i in range(r['frames'])]
        emit(f'Sprites/Efeitos/{dst}/loop',fr,(0,0),r['fps'],True,source='alias '+r['resource'],point=tuple(r['anchorPixelsTopLeft']))

def contacts():
    entries=[e for e in ENTRIES if e['resource'].startswith('Sprites/Efeitos/') and e['resource'].endswith(('/impacto','/uso','/loop'))]
    for page in range(0,len(entries),24):
        subset=entries[page:page+24]; board=Image.new('RGB',(960,math.ceil(len(subset)/6)*160),(19,25,33));d=ImageDraw.Draw(board)
        for i,e in enumerate(subset):
            im=Image.open(ROOT/e['file']).convert('RGBA');n=e['frameSize'];ix=e['impactFrame0'];f=im.crop((ix*n,0,(ix+1)*n,n));f.thumbnail((144,120),Image.Resampling.NEAREST)
            x=(i%6)*160;y=(i//6)*160; board.paste(f,(x+(160-f.width)//2,y),f)
            title=e['resource'].split('/')[-2];d.text((x+5,y+122),title[:23],fill='#e8dab8');d.text((x+5,y+139),f"{e['frames']}f / {e['fps']}fps",fill='#9eaab6')
        board.save(ROOT/f'previews/contact-{page//24+1:02}.png')

if __name__=='__main__':
    repairs();events();libraries();bindings();objects();contacts()
    save_json(ROOT/'assets.visual.json',{'schema':'arena-visual-v2','date':'2026-10-11','review':'aprovação automática solicitada pelo autor; integração pendente de confirmação Claude','entries':ENTRIES,'sampling':{'filter':'Point','mipmaps':False,'compression':'None','alphaIsTransparency':True},'note':'Não importar Biblioteca no Resources; são peças-fonte. GIFs são prévias; PNGs RGBA são a entrega.'})
    print(json.dumps({'assets':len(ENTRIES),'frames':sum(e['frames'] for e in ENTRIES)},ensure_ascii=False))
