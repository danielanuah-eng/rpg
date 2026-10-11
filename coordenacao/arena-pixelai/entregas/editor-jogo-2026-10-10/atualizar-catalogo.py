"""Catálogo visual incremental para o editor que será implementado pelo Claude."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib

ROOT=Path(__file__).resolve().parent
DELIVERY=ROOT.parent
def read(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    indexed={}
    state=read(ROOT/'estado-producao.json')
    partial=state.get('integracao')=='parcial_confirmada_claude_012'
    previous=ROOT/'catalogo.json'
    cache={a['file']:a for a in read(previous)['assets']} if previous.exists() else {}
    def add(p,meta):
        if not p.is_file(): return
        rel=p.relative_to(DELIVERY).as_posix()
        stat=p.stat(); old=cache.get(rel,{})
        sha=old.get('sha256') if old.get('sizeBytes')==stat.st_size and old.get('mtimeNs')==stat.st_mtime_ns else digest(p)
        indexed[rel]={**indexed.get(rel,{}),**meta,'assetId':meta.get('assetId',meta.get('id')),'id':'asset-'+hashlib.sha1(rel.encode()).hexdigest()[:12],'file':rel,'sha256':sha,'sizeBytes':stat.st_size,'mtimeNs':stat.st_mtime_ns,'availableOnDisk':True}
    for f in read(DELIVERY/'plano-de-importacao.json')['files']:
        p=DELIVERY/f['source']
        add(p,{'id':'legado-'+hashlib.sha1(f['source'].encode()).hexdigest()[:12],'type':'sprite','integration':f.get('state','pendente_claude'),'unityDestination':f.get('destination'),'frames':f.get('frames'),'origin':'plano-de-importacao.json'})
    correction=read(DELIVERY/'edgar-norte-2026-10-10/verificacao.json')
    for a in correction['assets']:
        add(DELIVERY/'edgar-norte-2026-10-10'/a['file'],{'id':'edgar-'+a['key'],'type':'sprite','hero':'edgar','integration':'pendente_claude','frames':a['frames'],'frameSize':a['frameSize'],'replacesEarlierEdgar':True,'technicalVerified':True,'visualReview':'approved_codex'})
    supplement=DELIVERY/'edgar-norte-2026-10-10/suplemento-transformacao-norte/manifesto.json'
    if supplement.exists():
        for a in read(supplement)['assets']:
            add(supplement.parent/a['file'],{**a,'type':'sprite','hero':'edgar','origin':supplement.relative_to(DELIVERY).as_posix()})
    baltazar=read(DELIVERY/'baltazar-2026-10-10/manifesto.json')
    for a in baltazar['assets']:
        add(DELIVERY/'baltazar-2026-10-10'/a['file'],{**a,'type':'sprite','hero':'baltazar','technicalVerified':True,'visualReview':a.get('visualReview','pending'),'integration':'importado_reportado_claude_012' if partial else a.get('integration','pendente_claude'),'importedVersionHashVerified':False})
    companions=DELIVERY/'baltazar-2026-10-10/companheiros/manifesto.json'
    if companions.exists():
        for a in read(companions)['assets']:
            add(companions.parent/a['file'],{**a,'type':'efeito','hero':'baltazar','integration':'importado_reportado_claude_012' if partial else a.get('integration','pendente_claude'),'importedVersionHashVerified':False})
    maps=[]
    reportedIds=set(state.get('mapas',{}).get('confirmadosClaude',[])) | set(state.get('mapas',{}).get('propsConfirmadosClaude',[]))
    for manifest in sorted(DELIVERY.glob('mapas-rotativos*/manifesto.json')):
        for a in read(manifest)['assets']:
            reported=a['id'] in reportedIds
            metadata={**a,'integration':'importado_reportado_claude_013' if reported else 'pendente_claude',
                      'editorAvailabilityReported':reported,'importedVersionHashVerified':False,
                      'integrationEvidence':state.get('editor',{}).get('respostaCameraProps') if reported else None,
                      'gameplayValidationReported':reported and a['tipo']=='mapa'}
            maps.append({**metadata,'deliveryRoot':manifest.parent.relative_to(DELIVERY).as_posix()})
            paths=[a[k] for k in ('arquivo','glb','fonte') if a.get(k)]
            for f in paths: add(manifest.parent/f,{**metadata,'type':a['tipo'],'origin':manifest.relative_to(DELIVERY).as_posix()})
    # Descobrir entregas novas sem listas fechadas: mantém referências separadas de imports.
    for p in sorted(DELIVERY.rglob('*')):
        if not p.is_file() or p.suffix.lower() not in ('.png','.fbx','.glb','.blend'): continue
        if ROOT in p.parents or p.as_posix() in indexed: continue
        rel=p.relative_to(DELIVERY).as_posix()
        if rel in indexed: continue
        reference=any(t in rel.lower() for t in ('/fontes/','/previews/','referencia','antes-depois','oito-vistas','folha-fonte','contato','comparativo'))
        kind='referencia' if reference else 'modelo' if p.suffix.lower() in ('.fbx','.glb','.blend') else 'asset_visual'
        add(p,{'id':'asset-'+hashlib.sha1(rel.encode()).hexdigest()[:12],'type':kind,'integration':'referencia' if reference else 'pendente_confirmacao_claude','origin':'descoberta_incremental','selectableInMatch':False})
    heroes={c['id']:{'id':c['id'],'name':c['name'],'characterId':c['character_id'],'integration':'pendente_confirmacao_claude'} for c in read(DELIVERY/'personagens-novos-2026-10-10/estado-atual.json')['characters']}
    heroes['baltazar']={'id':'baltazar','name':'Baltazar','characterId':baltazar['characterId'],'integration':'implementado_reportado_claude_012' if partial else 'pendente_claude','proposal':'baltazar-2026-10-10/ficha-proposta.json'}
    # Também oferecer os heróis dos destinos históricos, sem inventar os valores atuais.
    for a in indexed.values():
        dest=a.get('unityDestination','') or ''
        if '/Sprites/' in dest:
            name=dest.split('/Sprites/',1)[1].split('/',1)[0]
            heroes.setdefault(name.lower(),{'id':name.lower(),'name':name,'integration':'consultar_roster_claude'})
    fields=[{'key':'movement','label':'Movimento','type':'integer','min':0},{'key':'health','label':'Vida','type':'integer','min':1},{'key':'range','label':'Alcance','type':'integer','min':0},{'key':'damage','label':'Dano','type':'integer','min':0}]
    report={'schemaVersion':1,'project':'ArenaTaticaUnity','assetRoot':'coordenacao/arena-pixelai/entregas','implementation':'editor_parcial_confirmado_claude_012_catalogo_pendente' if partial else 'pendente_claude_tarefa_012','implementationEvidence':state.get('editor',{}).get('resposta'),'heroes':list(heroes.values()),'heroEditableFields':fields,'assets':list(indexed.values()),'newDeliveryPolicy':'Rodar este gerador ao receber entrega. Atualizar catálogo do editor preservando IDs e ajustes do autor. Disponibilidade em disco não confirma integração. Referências não são seleção automática em partida.','maps':maps,'futureStages':{'requiresArenaCompletion':True,'compareAllPixelLabInBlender':True,'PvEvPReferences':'KrosArcano','critAndArmorBalance':'somente futuro PvEvP'}}
    (ROOT/'catalogo.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({'assets':len(indexed),'heroes':len(heroes),'implementation':report['implementation']}))
    # Folha técnica de conferência das quatro vistas, não asset jogável.
    gallery=Image.new('RGB',(1200,760),'#202632'); d=ImageDraw.Draw(gallery)
    for row,name in enumerate(('bosque-quatro-patios','pedreira-anel-partido','neve-bastioes-diagonais')):
        for col in range(4):
            im=Image.open(DELIVERY/'mapas-rotativos-2026-10-10/previews'/f'{name}-jogador-{col+1}.png').convert('RGB').resize((300,225),Image.Resampling.LANCZOS)
            gallery.paste(im,(col*300,row*250+20));d.text((col*300+4,row*250+4),f'{name} / jogador {col+1}',fill='white')
    gallery.save(ROOT/'mapas-quatro-perspectivas.png')
if __name__=='__main__':main()
