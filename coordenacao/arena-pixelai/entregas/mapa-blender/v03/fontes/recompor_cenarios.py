"""Revisão de composição: usa os modelos aprovados da v02 sem alterar o jogo.

Blender --background --python-exit-code 1 --python recompor_cenarios.py -- --theme bosque
"""
import bpy
import bmesh
import hashlib
import importlib.util
import json
import math
import sys
from pathlib import Path
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
ASSETS=ROOT.parent/'v02'
THEME=sys.argv[sys.argv.index('--theme')+1] if '--theme' in sys.argv else 'bosque'
spec=importlib.util.spec_from_file_location('mapa_arte_base',ASSETS/'fontes'/'construir_cenarios.py')
base=importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)
base.ROOT=ROOT
base.PREV=ROOT/'previews'
LABEL=base.LABEL
manifest=json.loads((ASSETS/'manifestos'/(THEME+'.json')).read_text(encoding='utf-8'))

CONFIGS={
 'bosque':{
  'nome':'Clareira entre bosquetes','perfil':'bosque_oeste','lago':[8,6],'nivelLago':0,'yaw':0,'placa':[5,7],
  'terrenos':[(2,3,'arvore'),(3,9,'arvore'),(9,3,'arvore'),(8,9,'arvore'),(2,7,'arvore_jovem'),
              (4,2,'arbusto'),(6,9,'arbusto'),(7,2,'arbusto'),(5,2,'caixa'),(4,8,'caixa')],
  'props':[(3,5,'enfeite_pedra_baixa'),(7,9,'enfeite_pedra_baixa'),(8,2,'enfeite_pedra_baixa'),(3,7,'enfeite_fogueira')],
  'direcao':'Bosquetes em setores opostos, clareira central ampla e acampamento lateral. Sem cristal repetido junto à loja.'},
 'bosque_02':{
  'nome':'Trilha entre clareiras','perfil':'bosque_norte','lago':[3,7],'nivelLago':0,'yaw':0,'placa':[5,7],
  'terrenos':[(2,3,'arvore'),(3,9,'arvore'),(8,2,'arvore'),(9,8,'arvore'),(7,9,'arvore_jovem'),
              (4,2,'arbusto'),(6,9,'arbusto'),(9,4,'arbusto'),(7,3,'caixa'),(5,9,'caixa')],
  'props':[(2,5,'enfeite_pedra_baixa'),(8,5,'enfeite_pedra_baixa'),(4,9,'enfeite_pedra_baixa'),(8,8,'enfeite_fogueira')],
  'direcao':'Lago a oeste e relevo no lado oposto; grupos separados por duas clareiras.'},
 'bosque_03':{
  'nome':'Encosta e clareira','perfil':'bosque_leste','lago':[8,3],'nivelLago':1,'yaw':0,'placa':[5,7],
  'terrenos':[(2,2,'arvore'),(3,8,'arvore'),(8,9,'arvore'),(9,6,'arvore'),(2,6,'arvore_jovem'),
              (5,2,'arbusto'),(6,9,'arbusto'),(9,8,'arbusto'),(4,9,'caixa'),(8,5,'caixa')],
  'props':[(3,4,'enfeite_pedra_baixa'),(8,7,'enfeite_pedra_baixa'),(6,2,'enfeite_pedra_baixa'),(2,9,'enfeite_fogueira')],
  'direcao':'Encosta oriental e lago elevado; lado ocidental aberto, sem espelhar a primeira versão.'},
 'deserto':{
  'nome':'Dunas e oásis lateral','perfil':'deserto','lago':[3,7],'nivelLago':0,'yaw':-math.pi/2,'placa':[7,5],
  'terrenos':[(2,2,'arvore'),(9,3,'arvore'),(8,9,'arvore'),(9,7,'arbusto'),(5,2,'caixa'),(9,5,'caixa_rachada')],
  'props':[(2,9,'arvore_palmeira'),(4,9,'arvore_palmeira'),(3,4,'enfeite_pedra_baixa'),
           (7,2,'enfeite_pedra_baixa'),(6,9,'enfeite_pedra_baixa')],
  'direcao':'Dunas baixas assimétricas; oásis e duas palmeiras no setor sudoeste, cactos em setores separados. Grandes vazios de areia.'},
 'pedreira':{
  'nome':'Passo entre maciços','perfil':'montanha','lago':[8,8],'nivelLago':1,'yaw':0,'placa':[5,7],
  'terrenos':[(2,2,'arvore'),(3,9,'arvore'),(9,4,'arvore_jovem'),(5,2,'caixa'),(6,9,'caixa')],
  'props':[(2,5,'pedra'),(4,8,'pedra'),(8,3,'pedra'),(9,6,'pedra'),
           (4,3,'enfeite_pedra_baixa'),(7,3,'enfeite_pedra_baixa'),(9,2,'enfeite_cristal')],
  'direcao':'Dois maciços separados por um passo largo; afloramentos espaçados nas encostas e lago elevado ao sul. Centro sem muralha de props.'},
 'neve':{
  'nome':'Vale diagonal de gelo','perfil':'neve','lago':[3,8],'nivelLago':1,'yaw':math.pi/2,'placa':[4,6],
  'terrenos':[(2,2,'arvore'),(8,2,'arvore'),(9,7,'arvore'),
              (6,2,'arbusto'),(5,9,'arbusto'),(9,4,'caixa')],
  'props':[(2,5,'enfeite_pedra_baixa'),(8,5,'enfeite_pedra_baixa'),(4,3,'enfeite_cristal'),
           (7,8,'enfeite_cristal'),(7,3,'enfeite_fogueira')],
  'direcao':'Vale diagonal entre bancos de neve, lago congelado elevado e abetos afastados. Refúgio separado dos cristais.'},
 'vulcao':{
  'nome':'Fenda de basalto e lava','perfil':'vulcao','lago':[3,3],'nivelLago':0,'yaw':math.pi,'placa':[6,4],
  'terrenos':[(2,8,'arvore'),(9,2,'arvore'),(9,9,'arvore'),(5,2,'caixa'),(8,8,'caixa_rachada')],
  'props':[(3,6,'pedra'),(8,3,'pedra'),(6,9,'pedra'),(7,9,'enfeite_lago',.45),
           (9,5,'enfeite_lago',.45),(3,9,'enfeite_tocha')],
  'direcao':'Fenda diagonal baixa, três poças de lava em setores distintos, poucas árvores carbonizadas. Sem arbustos verdes nem acampamento repetido ao lado da loja.'}
}

def height_at(profile,x,y):
    if profile=='bosque_oeste':
        return 2 if x<=2 and 7<=y<=9 else 1 if x<=3 and y>=5 else 0
    if profile=='bosque_norte':
        return 2 if x>=8 and y<=2 else 1 if y>=8 or (x>=8 and y<=3) else 0
    if profile=='bosque_leste':
        return 2 if x>=9 and 4<=y<=8 else 1 if x>=8 or y<=2 else 0
    if profile=='deserto':
        a=(x-8.5)**2+(y-2.0)**2
        b=(x-2.0)**2+(y-9.5)**2
        return 2 if a<1.1 else 1 if a<5.5 or b<4.4 else 0
    if profile=='montanha':
        return 3 if (x<=3 and 3<=y<=8) or (x>=8 and 2<=y<=5) else 2 if x<=4 or (x>=7 and y<=8) else 1 if x>=6 and y>=9 else 0
    if profile=='neve':
        d=y-.65*x-1.7
        return 0 if abs(d)<1.35 else 2 if d>1.8 and y>7 else 1 if d>1.8 or (d< -1.8 and x>7) else 0
    if profile=='vulcao':
        return 0 if abs(x-(3+.4*y))<1.05 else 2 if (x>7 and y<5) or (x<4 and y>7) else 1 if x>8 or y>8 else 0
    raise ValueError(profile)

def setup_demo():
    kit=bpy.data.collections['KIT_'+THEME.upper()+'_ORIGINAIS_PIVO_ZERO']
    kit.hide_render=True
    kit.hide_viewport=True
    old=bpy.data.collections.get('PREVIA_TABULEIRO_12x12_APENAS_EXEMPLO')
    if old:
        for obj in list(old.objects): bpy.data.objects.remove(obj,do_unlink=True)
        bpy.data.collections.remove(old)
    demo=bpy.data.collections.new('PREVIA_TABULEIRO_12x12_APENAS_EXEMPLO')
    bpy.context.scene.collection.children.link(demo)
    bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[demo.name]
    base.copy_piece('entorno',(0,0,0))
    return demo

def clean_surrounds():
    """Remove apenas ilhas de geometria decorativa acima do chão; preserva o anel."""
    for name,obj in base.PIECES.items():
        if not name.startswith('entorno'): continue
        obj.data=obj.data.copy()
        mesh=bmesh.new()
        mesh.from_mesh(obj.data)
        seen=set()
        remove=[]
        for vertex in mesh.verts:
            if vertex in seen: continue
            group=[]
            pending=[vertex]
            seen.add(vertex)
            while pending:
                current=pending.pop()
                group.append(current)
                for edge in current.link_edges:
                    neighbor=edge.other_vert(current)
                    if neighbor not in seen:
                        seen.add(neighbor)
                        pending.append(neighbor)
            if all(v.co.z>=-.0001 for v in group): remove.extend(group)
        assert remove,('Nenhuma decoração externa encontrada',name)
        bmesh.ops.delete(mesh,geom=remove,context='VERTS')
        mesh.to_mesh(obj.data)
        mesh.free()
        obj.data.update()
        obj.data.calc_loop_triangles()
        obj['vento']=False
        entry=next(e for e in manifest['pecas'] if e['peca']==name)
        bounds=[Vector(v) for v in obj.bound_box]
        entry.update(vento='não',tris=len(obj.data.loop_triangles),dimensoes=[round(float(v),4) for v in obj.dimensions],
                     bounds={'min':[round(min(v[i] for v in bounds),4) for i in range(3)],
                             'max':[round(max(v[i] for v in bounds),4) for i in range(3)]},
                     observacao='Revisão 03: anel externo sem cerca nem props decorativos incorporados; detalhes posicionados no interior.')
    # Exporta somente os três entornos alterados; os demais modelos seguem compartilhados.
    all_pieces=base.PIECES
    base.PIECES={name:obj for name,obj in all_pieces.items() if name.startswith('entorno')}
    base.OUT=ROOT/'mapa'/THEME
    base.OUT.mkdir(parents=True,exist_ok=True)
    kit=bpy.data.collections['KIT_'+THEME.upper()+'_ORIGINAIS_PIVO_ZERO']
    kit.hide_viewport=False
    bpy.context.view_layer.update()
    base.exports()
    base.PIECES=all_pieces
    kit.hide_viewport=True

def write_json(path,data):
    path.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')

def render_layout(layout=0):
    key=THEME if layout==0 else THEME+'_'+str(layout+1).zfill(2)
    config=CONFIGS[key]
    demo=setup_demo()
    pool=tuple(config['lago'])
    pool_cells={(x,y) for x in range(pool[0]-1,pool[0]+2) for y in range(pool[1]-1,pool[1]+2)}
    zone={(x,y) for x in [5,6] for y in [5,6]}
    heights={}
    for x in range(12):
        for y in range(12):
            h=config['nivelLago'] if (x,y) in pool_cells else 1 if (x,y) in zone else height_at(config['perfil'],x,y)
            if THEME=='neve' and 8<=x<=10 and 6<=y<=8: h=0
            heights[x,y]=h
            pos=(x-5.5,y-5.5,h*.45)
            for z in range(h): base.copy_piece('coluna_a' if (x+y+z)%2 else 'coluna_b',(pos[0],pos[1],z*.45))
            base.copy_piece('topo_zona' if (x,y) in zone else 'topo_a' if (x+y)%2 else 'topo_b',pos)
    placed=[]
    occupied=set(zone)|pool_cells|{tuple(config['placa'])}
    corridor=zone|{(4,5),(4,6),(5,4),(6,4),(7,5),(7,6),(5,7),(6,7)}
    def place(name,cell,scale=1,rotation=0,protected=True):
        cell=tuple(cell)
        if protected:
            assert cell not in occupied and cell not in corridor,('Prop em área ocupada ou passagem central',name,cell)
            assert math.dist(cell,(5.5,5.5))>=2,('Prop grande muito perto da loja',name,cell)
            occupied.add(cell)
        x,y=cell
        pos=(x-5.5,y-5.5,heights[cell]*.45+.05)
        obj=base.copy_piece(name,pos,rotation,scale)
        obj['casa_exemplo']=list(cell)
        obj['prop_principal']=protected
        placed.append({'peca':name,'casaExemplo':list(cell),'posicaoBlender':list(pos),'posicaoUnity':[pos[0],pos[2],-pos[1]],
                       'rotacaoZBlender':rotation,'escala':scale,'principal':protected})
        return obj
    for x,y,name in config['terrenos']:
        place(name,(x,y),rotation=(x*3+y)*.41 if name.startswith('arvore') else 0)
    for i,data in enumerate(config['props']):
        x,y,name=data[:3]
        scale=data[3] if len(data)>3 else 1
        place(name,(x,y),scale,rotation=i*.62)
    store=base.copy_piece('loja_mascate',(0,0,.5),config['yaw'])
    store['prop_principal']=False
    place('enfeite_placa',config['placa'],rotation=config['yaw'],protected=False)
    lake=place('enfeite_lago',pool,protected=False)
    lake['prop_principal']=True
    placed[-1]['principal']=True
    lake['casa_exemplo']=list(pool)
    assert len({heights[c] for c in pool_cells})==1
    # Currency is a visual example only, and never invents rewards or purchases.
    coin_candidates=[(2,4),(4,9),(8,4),(9,7),(6,2),(2,9),(7,8),(3,2),(5,8),(8,9),(4,4),(7,2),(2,6),(9,8),(6,9),
                     (3,9),(8,8),(9,2),(2,5),(4,3),(7,3),(9,6),(2,8)]
    coin_cells=[]
    for i,cell in enumerate(coin_candidates):
        if cell in occupied: continue
        obj=place('moeda_chao',cell,rotation=i*.7,protected=False)
        obj.location.z+=.001
        placed[-1]['posicaoBlender'][2]+=.001
        placed[-1]['posicaoUnity'][1]+=.001
        coin_cells.append(cell)
        if len(coin_cells)==9: break
    assert len(coin_cells)==9
    # Small details occupy isolated free cells, rather than a repeated ring around the shop.
    low_candidates=[(2,6),(4,2),(7,9),(9,4),(5,9),(2,9),(9,7),(8,2),(3,5),(6,3),(4,8),(7,4),(8,6),(2,8),(9,2)]
    low_names=['enfeite_seixos','enfeite_capim','enfeite_raiz_chao','enfeite_flores','enfeite_cogumelos']
    details=0
    for i,cell in enumerate(low_candidates):
        if cell in occupied or cell in corridor or cell in coin_cells: continue
        name=low_names[(i+layout)%len(low_names)]
        if THEME in ['pedreira','vulcao'] and name in ['enfeite_capim','enfeite_flores','enfeite_cogumelos']: name='enfeite_seixos'
        if THEME=='deserto' and name in ['enfeite_flores','enfeite_cogumelos']: name='enfeite_seixos'
        place(name,cell,rotation=i*.5,protected=False)
        details+=1
        if details==7: break
    for i in range(12):
        base.copy_piece('beira_reta',(i-5.5,6,0))
        base.copy_piece('beira_reta',(6,i-5.5,0),math.pi/2)
    bpy.context.view_layer.update()
    mains=[o for o in demo.objects if o.type=='MESH' and o.get('prop_principal')]
    sectors=[0,0,0,0]
    for obj in mains:
        x,y=obj['casa_exemplo']
        sectors[(x>=6)+2*(y>=6)]+=1
        for corner in obj.bound_box:
            point=obj.matrix_world@Vector(corner)
            assert abs(point.x)<=5.5 and abs(point.y)<=5.5,('Sem margem',obj.name)
    assert all(n>=1 for n in sectors),('Props concentrados em poucos setores',sectors)
    suffix='' if layout==0 else '-versao-'+str(layout+1).zfill(2)
    write_json(ROOT/'manifestos'/(THEME+suffix+'-distribuicao.json'),{
        'versao':3,'cenario':LABEL,'nomeComposicao':config['nome'],'direcao':config['direcao'],
        'loja':{'posicaoBlender':[0,0,.5],'rotacaoZBlender':config['yaw'],'sempreCentral':True},
        'apoioLago':{'casasReservadas':[list(c) for c in sorted(pool_cells)],'nivel':config['nivelLago']},
        'alturaExemplo12x12':[[heights[x,y] for y in range(12)] for x in range(12)],
        'propsPrincipaisPorSetor':sectors,'raioSemPropsGrandesAoRedorDoCentro':2,
        'colocacoes':placed,'estado':'composicao_visual; MapGen e integracao ficam com Claude'})
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=24
    scene.cycles.use_denoising=True
    scene.render.resolution_x=1600
    scene.render.resolution_y=1200
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.film_transparent=False
    scene.world.use_nodes=True
    background=scene.world.node_tree.nodes.get('Background')
    background.inputs['Color'].default_value=(.22,.29,.35,1)
    background.inputs['Strength'].default_value=.55
    scene.view_settings.view_transform='Standard'
    scene.view_settings.look='None'
    bpy.ops.object.light_add(type='SUN',location=(-6,-7,12))
    sun=bpy.context.object
    sun.data.energy=2
    sun.data.angle=math.radians(7)
    base.aim(sun,(0,0,0))
    bpy.ops.object.light_add(type='AREA',location=(5,3,10))
    fill=bpy.context.object
    fill.data.energy=400
    fill.data.shape='DISK'
    fill.data.size=10
    base.aim(fill,(0,0,0))
    bpy.ops.object.camera_add(location=(19,19,19))
    camera=bpy.context.object
    base.aim(camera,(0,0,.25))
    camera.data.type='ORTHO'
    camera.data.ortho_scale=27
    scene.camera=camera
    scene['status']='revisao 03: distribuicao e topologias distintas; integracao Unity pendente'
    for image in bpy.data.images:
        if image.source=='FILE' and not image.packed_file: image.pack()
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type=='VIEW_3D': area.spaces.active.region_3d.view_perspective='CAMERA'
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/(THEME+suffix+'.blend')))
    scene.render.filepath=str(ROOT/'previews'/(THEME+'-tabuleiro-12x12.png' if layout==0 else THEME+suffix+'.png'))
    bpy.ops.render.render(write_still=True)
    print('COMPOSICAO_OK',key,sectors,flush=True)

def main():
    bpy.ops.wm.open_mainfile(filepath=str(ASSETS/manifest['fonte']))
    base.PIECES={entry['peca']:bpy.data.objects[THEME+'_'+entry['peca']] for entry in manifest['pecas']}
    clean_surrounds()
    render_layout()
    if THEME=='bosque':
        render_layout(1)
        render_layout(2)
    manifest.update(versao=3,estado='revisao_de_composicao_entregue_para_avaliacao',
        fonte='fontes/'+THEME+'.blend',script='fontes/recompor_cenarios.py',
        previews=['previews/'+THEME+'-tabuleiro-12x12.png'],
        revisaoAutor=['Relevo e distribuicao distintos por tema','Props em quatro setores','Centro com mais espaço livre'],
        modelosReutilizadosDaVersao='v02; somente os tres entornos por tema foram limpos e reexportados na v03')
    for entry in manifest['pecas']:
        for field in ['arquivo','glb']:
            if entry['peca'].startswith('entorno'):
                entry['sha256'][field]=hashlib.sha256((ROOT/entry[field]).read_bytes()).hexdigest()
            else: entry[field]='../v02/'+entry[field]
        entry['textura']=['../v02/'+path for path in entry['textura']]
    if THEME=='bosque':
        manifest['versoesBosque']=[{'id':'v01','nome':CONFIGS['bosque']['nome'],'fonte':'fontes/bosque.blend','preview':'previews/bosque-tabuleiro-12x12.png'},
                                  {'id':'v02','nome':CONFIGS['bosque_02']['nome'],'fonte':'fontes/bosque-versao-02.blend','preview':'previews/bosque-versao-02.png'},
                                  {'id':'v03','nome':CONFIGS['bosque_03']['nome'],'fonte':'fontes/bosque-versao-03.blend','preview':'previews/bosque-versao-03.png'}]
    write_json(ROOT/'manifestos'/(THEME+'.json'),manifest)

if __name__=='__main__': main()
