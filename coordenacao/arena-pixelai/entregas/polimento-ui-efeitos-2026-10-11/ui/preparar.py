from pathlib import Path
import hashlib, json, re, shutil
ROOT=Path(__file__).resolve().parent
REPO=ROOT.parents[4]
SPR=REPO/'ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites'
ART=ROOT/'art';ART.mkdir(exist_ok=True)
actors=[('baltazar','Baltazar','Baltazar','Paladino · Defesa e suporte',14,6,3,4),('guardiao-de-musgo','GuardiaoMusgo','Guardião de Musgo','Proteção e raízes',13,6,2,4),('asamyh','Asamyh','Asamyh','Invocação e cura',8,6,4,4),('principe-kellen','PrincipeKellen','Príncipe Kellen','Magia e controle',7,6,3,4),('cheiro-pneu','CheiroPneu','Cheiro D. Pneu Queimado','Canhão e óleo',14,6,3,5),('colosso','Colosso','O Colosso','Força e resistência',18,6,2,6)]
content=(REPO/'ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Content.cs').read_text(encoding='utf-8-sig')
data=[];sources=[]
for id,folder,name,role,hp,pa,pm,cost in actors:
    line=next((line for line in content.splitlines() if f'["{id}"]=new FighterDef' in line),'')
    values={field:int(re.search(r'\b'+field+r'=(\d+)',line).group(1)) for field in ['hp','pa','pm','level'] if re.search(r'\b'+field+r'=(\d+)',line)}
    src=SPR/folder/'south.png';dest=ART/f'{id}.png';shutil.copyfile(src,dest)
    data.append({'id':id,'folder':folder,'name':name,'role':role,'hp':values.get('hp',hp),'pa':values.get('pa',pa),'pm':values.get('pm',pm),'cost':values.get('level',cost),'image':f'art/{id}.png'})
    sources.append({'file':str(dest.relative_to(ROOT)),'source':str(src.relative_to(REPO)).replace('\\','/'),'sha256':hashlib.sha256(src.read_bytes()).hexdigest()})
maps=[('bosque','Bosque','bosque-quatro-patios-jogador-1.png'),('pedreira','Pedreira','pedreira-anel-partido-jogador-1.png'),('neve','Neve','neve-bastioes-diagonais-jogador-1.png')]
for id,name,file in maps:
    src=REPO/'coordenacao/arena-pixelai/entregas/mapas-rotativos-2026-10-10/previews'/file
    dest=ART/f'mapa-{id}.png'
    if not (ROOT/'arenas-renderizadas.json').exists(): shutil.copyfile(src,dest)
    sources.append({'file':str(dest.relative_to(ROOT)),'source':str(src.relative_to(REPO)).replace('\\','/'),'sha256':hashlib.sha256(src.read_bytes()).hexdigest()})
(ROOT/'conteudo.js').write_text('window.ARENA_ART='+json.dumps({'fighters':data,'maps':[{'id':i,'name':n,'image':f'art/mapa-{i}.png'} for i,n,_ in maps]},ensure_ascii=False)+';\n',encoding='utf-8',newline='\n')
(ROOT/'fontes.json').write_text(json.dumps(sources,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
print(json.dumps({'portraits':len(data),'maps':len(maps)}))
