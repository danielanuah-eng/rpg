from pathlib import Path
from PIL import Image
import json,hashlib
root=Path(__file__).resolve().parent; bridge=root.parent
plan=[]
def add(source,destination,state,**extra):
    file=root/source; assert file.exists(); im=Image.open(file)
    plan.append(dict(source=file.relative_to(root).as_posix(),destination=destination,state=state,size=list(im.size),sha256=hashlib.sha256(file.read_bytes()).hexdigest(),**extra))
for e in json.loads((root/'correcoes-sincronia/fontes.json').read_text(encoding='utf-8')):
    folder=Path(e['source_file']).parent.name; name=e['clip']+'_'+e['dir']+'.png'
    if folder=='TchuTchaBerserk': continue # Retain old repair for history; new form replaces this destination.
    add('correcoes-sincronia/'+e['key']+'/'+name,'Assets/ArenaTatica/Resources/Sprites/'+folder+'/'+name,'reimportar_final_168' if e['key']=='damianPistolSE' else 'primeira_importacao_confirmada_claude',frames=7 if e['clip']=='hit' else 9)
add('talyssa-hit-north-west/hit_north-west.png','Assets/ArenaTatica/Resources/Sprites/Talyssa/hit_north-west.png','integrado_confirmado_claude',frames=7)
shop=json.loads((root/'loja-v2/loja.visual.json').read_text(encoding='utf-8'))
shop['recommended_import']=dict(source='loja-v2/cenario/frame-01..09.png',destination='Assets/ArenaTatica/Resources/Sprites/Vendedor/frame-01..09.png',frame_size=[384,216],fps=5,reason='ArenaHud replaces whole scene sprite. Full scene frames keep all environment visible without adding overlay composition.')
shop['scene']['destination']=None; shop['scene']['status']='optional_strip_requires_custom_384x216_slicing'
for i in range(1,10): add(f'loja-v2/cenario/frame-{i:02d}.png',f'Assets/ArenaTatica/Resources/Sprites/Vendedor/frame-{i:02d}.png','substituir_fonte_inicial_recortada_por_cena_completa',fps=5)
for e in shop['items']: add(e['file'].replace('\\','/'),e['destination'],'integrado_confirmado_claude')
(root/'loja-v2/loja.visual.json').write_text(json.dumps(shop,ensure_ascii=False,indent=2),encoding='utf-8')
fxfile=root/'eventos-cenarios/assets.visual.json'; fx=json.loads(fxfile.read_text(encoding='utf-8'))
tile_dest={'pedra':'pedreira','grama':'grama','ruinas':'ruinas','neve':'neve','vulcao':'vulcao'}
for e in fx['terrain']:
    if e['id'] in tile_dest:
        e['destination']='Assets/ArenaTatica/Resources/Textures/tile_'+tile_dest[e['id']]+'.png'
        add(e['file'].replace('\\','/'),e['destination'],'substituir_lookup_tema' if e['id'] in ['neve','vulcao'] else 'primeiro_lote_integrado_confirmado_claude')
for e in fx['projectiles']:
    if e.get('destination'): add(e['file'].replace('\\','/'),e['destination'],'primeiro_lote_integrado_confirmado_claude')
for e in fx['animations']: add(e['file'].replace('\\','/'),e['destination'],'nova_animacao_pronta' if e['id']!='moeda' else 'primeira_tira_integrada_confirmada_claude',frames=e['frames'],cell=e['frame_size'][0],suggested_fps=e['suggested_fps'])
fxfile.write_text(json.dumps(fx,ensure_ascii=False,indent=2),encoding='utf-8')
berserk_file=root/'tchu-tcha-berserk-possuido/berserk.visual.json'; berserk=json.loads(berserk_file.read_text(encoding='utf-8'))
berserk.update(form_id='berserk',activation_skill_id='olho-abissal',in_form_attack_skill_id='furia-negra',transition_clip='transform',transition_source_folder='TchuTcha')
for e in berserk['views']: add('tchu-tcha-berserk-possuido/'+e['file'],e['destination'],'novo_estado_pronto',pivot=e['pivot'],ppu=128)
for e in berserk['animations']: add('tchu-tcha-berserk-possuido/'+e['file'].replace('\\','/'),e['destination'],'novo_estado_pronto',frames=e['frames'],cell=e['cell'],pivot=e['pivot'],fps=e['fps'],ppu=128)
berserk_file.write_text(json.dumps(berserk,ensure_ascii=False,indent=2),encoding='utf-8')
assert len(set(e['destination'] for e in plan))==len(plan),'Duplicate import destination'
result=dict(status='entregue',ready_for_claude=True,author_visual_approval='pendente',integration_final='pendente_claude',scope='Images and technical asset preparation only; no game code, Assets, Tools or upload changed by Codex.',files=plan,optional_not_in_import_plan=['correcoes-sincronia/berserkIdleSW/idle_south-west.png (old form repair superseded by new state)','loja-v2/loja.png','loja-v2/vendedor.png','loja-v2/vendedor-estabilizado/*.png','eventos-cenarios/terrenos/cristal.png','all previews, source frames, backup strips, ZIPs and review groups'],tasks=dict(codex_delivered=['003','004','007','008'],claude_initial_confirmed=['005','006'],claude_final_pending=['009','010','import final 168 pistol','import full animated shop scene','import new possessed Berserk','bind new effect clips and snow/volcano textures']),validation=dict(sprite_snapshot_characters=12,sprite_snapshot_clips=284,reference_pivots_registered=284,imported_origins_different=127,new_berserk_clips=40,new_berserk_frames=344,new_berserk_views=8,new_berserk_return_clips_exact=32,shop_scene_frames=9,shop_product_icons=10,ranged_skills_linked=9,game_events_linked=26,terrain_themes=5,asset_checks_passed=True,unity_tests_by_codex=False,claude_previous_tests='24/24 EditMode; ARENA_PREPARE_OK per prior response before final assets'))
(root/'plano-de-importacao.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
(bridge/'estado-visual-final.json').write_text(json.dumps({k:v for k,v in result.items() if k!='files'},ensure_ascii=False,indent=2),encoding='utf-8')
for pattern in ['003-codex-sincronia-completa.md','004-codex-loja-animada-itens.md','007-codex-eventos-cenarios.md','008-codex-berserk-possuido.md']:
    file=bridge/'tarefas'/pattern; text=file.read_text(encoding='utf-8'); text=text.replace('**Status:** em andamento','**Status:** entregue; integração final pendente com Claude'); file.write_text(text,encoding='utf-8')
print('Import plan:',len(plan),'verified PNG files; producer tasks delivered, final integration pending')
