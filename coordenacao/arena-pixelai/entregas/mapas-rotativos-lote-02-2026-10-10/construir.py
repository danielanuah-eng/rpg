"""Mapa visual editável: galerias de deserto deslocadas, sem mudar regras do jogo."""
from pathlib import Path
from mathutils import Vector
import bpy, hashlib, json, math

ROOT=Path(__file__).resolve().parent
ID='deserto-galerias-deslocadas'
CAMERAS=[(19,19,19),(-19,19,19),(-19,-19,19),(19,-19,19)]
for name in ('fontes','layouts','previews'): (ROOT/name).mkdir(parents=True,exist_ok=True)

def aim(obj): obj.rotation_euler=(Vector((0,0,.4))-obj.location).to_track_quat('-Z','Y').to_euler()
def inst(src,col,pos,rotation=0):
    obj=src.copy(); obj.data=src.data; col.objects.link(obj); obj.location=pos; obj.rotation_euler.z=rotation
    for child in src.children:
        copy=child.copy(); col.objects.link(copy); copy.parent=obj
    return obj

def layout(n):
    heights=[]
    for y in range(n):
        row=[]
        for x in range(n):
            u,v=x*11/(n-1),y*11/(n-1)
            level=0
            for cx,cy in ((3,4),(8,7)):
                if abs(u-cx)<=1.5 and abs(v-cy)<=2: level=max(level,1)
                if abs(u-cx)<=.5 and abs(v-cy)<=1: level=2
            if x in (n//2-1,n//2) and y in (n//2-1,n//2): level=1
            row.append(level)
        heights.append(row)
    props=[]
    for x,y,kind in ((3,4,'arvore'),(8,7,'arvore'),(2,9,'arvore'),(9,2,'arvore'),
                     (1,2,'arbusto'),(10,9,'arbusto'),(4,9,'caixa'),(7,2,'caixa')):
        props.append({'peca':kind,'casa':[round(x*(n-1)/11),round(y*(n-1)/11)],'rotacaoGraus':0 if x<6 else 180,'regra':'existente'})
    return {'id':ID,'tema':'deserto','dimensao':n,'alturaPorNivel':.45,'alturas':heights,'props':props,
            'loja':{'centroBlender':[0,0,.5],'zonaCentral':'preservar_pedido_anterior_do_autor'},
            'mapaJogavelValidado':False,'estado':'estudo_visual_para_importacao_claude'}

def main():
    source=ROOT.parent/'mapa-blender/v02/fontes/deserto.blend'
    bpy.ops.wm.open_mainfile(filepath=str(source))
    originals={o['peca']:o for o in bpy.data.objects if 'peca' in o and o.name=='deserto_'+o['peca']}
    assert all(k in originals for k in ('topo_a','topo_b','topo_zona','coluna_a','coluna_b','entorno','loja_mascate','arvore','arbusto','caixa'))
    keep=set(originals.values())|{child for obj in originals.values() for child in obj.children}
    for obj in list(bpy.data.objects):
        if obj not in keep: bpy.data.objects.remove(obj,do_unlink=True)
    sources=bpy.data.collections.new('FONTES_EDITAVEIS'); bpy.context.scene.collection.children.link(sources)
    for obj in keep:
        for col in list(obj.users_collection): col.objects.unlink(obj)
        sources.objects.link(obj)
    sources.hide_render=True
    scene=bpy.context.scene
    demo=bpy.data.collections.new('MAPA_'+ID); scene.collection.children.link(demo)
    bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children.get(demo.name)
    data=layout(12); inst(originals['entorno'],demo,(0,0,0))
    for y,row in enumerate(data['alturas']):
        for x,level in enumerate(row):
            for z in range(level): inst(originals['coluna_a' if (x+y+z)%2==0 else 'coluna_b'],demo,(x-5.5,y-5.5,z*.45))
            kind='topo_zona' if x in (5,6) and y in (5,6) else 'topo_a' if (x+y)%2==0 else 'topo_b'
            inst(originals[kind],demo,(x-5.5,y-5.5,level*.45))
    for p in data['props']:
        x,y=p['casa']; inst(originals[p['peca']],demo,(x-5.5,y-5.5,data['alturas'][y][x]*.45+.05),math.radians(p['rotacaoGraus']))
    inst(originals['loja_mascate'],demo,(0,0,.5))
    for n in (12,16,18): (ROOT/'layouts'/f'{ID}-{n}.json').write_text(json.dumps(layout(n),ensure_ascii=False,indent=2),encoding='utf-8')
    scene.render.engine='CYCLES'; scene.cycles.samples=8; scene.cycles.use_denoising=True
    scene.render.resolution_x=1000; scene.render.resolution_y=750; scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'; scene.render.film_transparent=False
    scene.view_settings.view_transform='Standard'; scene.world.color=(.17,.15,.12)
    bpy.ops.object.light_add(type='SUN',location=(-6,-7,12)); bpy.context.object.data.energy=2; aim(bpy.context.object)
    bpy.ops.object.light_add(type='AREA',location=(0,0,12)); bpy.context.object.data.energy=400; bpy.context.object.data.size=10; aim(bpy.context.object)
    cameras=[]
    for seat,position in enumerate(CAMERAS,1):
        bpy.ops.object.camera_add(location=position); camera=bpy.context.object; camera.name=f'VISTA_JOGADOR_{seat}'
        camera.data.type='ORTHO'; camera.data.ortho_scale=27; aim(camera); cameras.append(camera)
    for im in bpy.data.images:
        if im.source=='FILE' and not im.packed_file: im.pack()
    scene.camera=cameras[0]; scene['estado']='estudo_visual; integracao e jogo pendentes'
    scene['rotacao']='camera local; coordenadas e props identicos em todas as vistas'
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/f'{ID}.blend'))
    previews=[]
    for seat,camera in enumerate(cameras,1):
        scene.camera=camera; file=ROOT/'previews'/f'{ID}-jogador-{seat}.png'; scene.render.filepath=str(file)
        bpy.ops.render.render(write_still=True); previews.append(file.relative_to(ROOT).as_posix()); print('VISTA_OK',seat,flush=True)
    hashes={p.relative_to(ROOT).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in ROOT.rglob('*') if p.suffix in ('.png','.blend','.json') and p.name!='manifesto.json'}
    asset={'id':ID,'tipo':'mapa','tema':'deserto','fonte':f'fontes/{ID}.blend','layout':f'layouts/{ID}-12.json','dimensoesSuportadas':[12,16,18],
           'dimensaoFonte':12,'dimensoesLayouts':[12,16,18],'dimensaoPreviews':12,
           'previews':previews,'camerasBlender':CAMERAS,'cameraLocalPorJogador':True,'estado':'entregue','integracao':'pendente_claude','mapaJogavelValidado':False}
    manifest={'versao':1,'data':'2026-10-10','assets':[asset],'hashes':hashes,'sourceKit':'mapa-blender/v02/fontes/deserto.blend',
              'sourceKitSha256':hashlib.sha256(source.read_bytes()).hexdigest(),
              'prompt':'Duas galerias elevadas e deslocadas no deserto, ligadas por rotas baixas alternativas. Loja central preservada; quatro vistas locais mostram o mesmo tabuleiro inteiro. Fontes, terreno e props editáveis; nenhuma regra nova.',
              'nota':'Estudo visual; Claude valida mobilidade, colisao, spawns e linhas de visao.'}
    (ROOT/'manifesto.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
    print('MAPA_DESERTO_OK',flush=True)

if __name__=='__main__':main()
