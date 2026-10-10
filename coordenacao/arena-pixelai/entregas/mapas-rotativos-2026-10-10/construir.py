"""Estudos visuais da Arena: reutiliza o kit aprovado, sem alterar MapGen."""
from pathlib import Path
from mathutils import Vector
import bpy, math, json, hashlib

ROOT = Path(__file__).resolve().parent
KIT = ROOT.parent/'mapa-blender/v02'
for d in ('fontes','previews','layouts','props'):
    (ROOT/d).mkdir(parents=True, exist_ok=True)
CAMERAS = [(19,19,19),(-19,19,19),(-19,-19,19),(19,-19,19)]

def aim(o, target=(0,0,.4)):
    o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()

def inst(src, collection, pos, rot=0):
    o=src.copy(); o.data=src.data
    collection.objects.link(o); o.location=pos; o.rotation_euler.z=rot
    for e in src.children:
        child=e.copy(); collection.objects.link(child); child.parent=o
    return o

def material(name, color, emission=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Roughness'].default_value=.8
    if emission:
        p.inputs['Emission Color'].default_value=(*color,1); p.inputs['Emission Strength'].default_value=emission
    return m

def cube(parts, pos, scale, mat, angle=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos)
    o=bpy.context.object; o.scale=scale; o.rotation_euler.y=angle
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    o.data.materials.append(mat)
    mod=o.modifiers.new('chanfro','BEVEL'); mod.width=.025; mod.segments=1
    bpy.ops.object.modifier_apply(modifier=mod.name); parts.append(o)

def props():
    silver=material('arena_nova_prata_fosca',(.55,.59,.65))
    wood=next((m for m in bpy.data.materials if m.name.endswith('_madeira')),silver)
    rock=next((m for m in bpy.data.materials if m.name.endswith('_pedra')),silver)
    light=material('arena_nova_bencao_emissivo',(.25,.65,.62),.35)
    result={}
    for name in ('relicario_solar','barricada_partida'):
        pieces=[]
        if name=='relicario_solar':
            cube(pieces,(0,0,.06),(.76,.76,.12),rock)
            cube(pieces,(0,0,.22),(.34,.34,.25),silver)
            for x in (-.26,.26): cube(pieces,(x,0,.37),(.09,.25,.55),silver)
            cube(pieces,(0,0,.68),(.64,.31,.12),silver)
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.16,location=(0,0,.46))
            o=bpy.context.object; o.data.materials.append(light); pieces.append(o)
        else:
            cube(pieces,(0,0,.04),(.90,.45,.08),rock)
            for x,h in ((-.32,.55),(0,.40),(.32,.62)):
                cube(pieces,(x,0,h/2+.08),(.10,.16,h),wood,.07*x)
            cube(pieces,(0,0,.28),(.85,.12,.13),wood,.10)
            cube(pieces,(.22,.15,.10),(.42,.12,.08),wood,-.15)
        bpy.ops.object.select_all(action='DESELECT')
        for o in pieces: o.select_set(True)
        bpy.context.view_layer.objects.active=pieces[0]; bpy.ops.object.join()
        o=bpy.context.object; o.name='novo_'+name
        bpy.context.scene.cursor.location=(0,0,0); bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
        o['peca']=name; o['integracao']='pendente_claude'; o['colisao']='decoracao_sem_regra_nova'
        o.data.calc_loop_triangles()
        assert len(o.data.loop_triangles)<3000
        assert abs(min(v.co.z for v in o.data.vertices))<1e-5
        assert o.dimensions.x<=1 and o.dimensions.y<=1
        out=ROOT/'props'/name
        bpy.ops.export_scene.gltf(filepath=str(out.with_suffix('.glb')),export_format='GLB',use_selection=True,export_yup=True)
        bpy.ops.export_scene.fbx(filepath=str(out.with_suffix('.fbx')),use_selection=True,axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False)
        result[name]=o
    return result

def layout(kind, n):
    h=[]
    for y in range(n):
        row=[]
        for x in range(n):
            u,v=x*11/(n-1),y*11/(n-1)
            if kind==0:
                level=1 if 3<=u<=8 and 3<=v<=8 else 0
                if abs(u-5.5)<1.6 or abs(v-5.5)<1.6: level=0
            elif kind==1:
                radius=math.hypot(u-5.5,v-5.5)
                level=1 if 3<=radius<=4.5 else 0
            else:
                distance=min(math.hypot(u-a,v-b) for a,b in ((1,1),(1,10),(10,1),(10,10)))
                level=max(0,min(3,int(3-distance/1.5)))
            if abs(u-5.5)<1.2 and abs(v-5.5)<1.2: level=1
            row.append(level)
        h.append(row)
    placements=[]
    for x,y,name in ((2,4,'arvore'),(9,7,'arvore'),(4,2,'arvore'),(7,9,'arvore'),
                     (3,7,'arbusto'),(8,4,'arbusto'),(4,4,'caixa'),(7,7,'caixa'),
                     (2,8,'relicario_solar'),(9,3,'relicario_solar'),
                     (3,3,'barricada_partida'),(8,8,'barricada_partida')):
        xx,yy=round(x*(n-1)/11),round(y*(n-1)/11)
        placements.append({'peca':name,'casa':[xx,yy],'rotacaoGraus':0 if x<6 else 180,
                           'regra':'existente' if name in ('arvore','arbusto','caixa') else 'somente_decoracao'})
    return {'dimensao':n,'alturaPorNivel':.45,'alturas':h,'props':placements,
            'loja':{'centroBlender':[0,0,.5],'zonaCentral':'preservar_pedido_anterior_do_autor'},
            'mapaJogavelValidado':False,'estado':'estudo_visual_para_importacao_claude'}

def main():
    manifests=[]
    definitions=[('bosque','bosque-quatro-patios'),('pedreira','pedreira-anel-partido'),('neve','neve-bastioes-diagonais')]
    for kind,(theme,map_id) in enumerate(definitions):
        source=KIT/'fontes'/(theme+'.blend')
        bpy.ops.wm.open_mainfile(filepath=str(source))
        originals={o['peca']:o for o in bpy.data.objects if 'peca' in o and o.name==theme+'_'+o['peca']}
        assert all(k in originals for k in ('topo_a','topo_b','topo_zona','coluna_a','coluna_b','entorno','loja_mascate','arvore','arbusto','caixa'))
        keep=set(originals.values())|{c for o in originals.values() for c in o.children}
        for o in list(bpy.data.objects):
            if o not in keep: bpy.data.objects.remove(o,do_unlink=True)
        kit=bpy.data.collections.new('FONTES_VISUAIS'); bpy.context.scene.collection.children.link(kit)
        for o in keep:
            for c in list(o.users_collection): c.objects.unlink(o)
            kit.objects.link(o)
        kit.hide_render=True
        scene=bpy.context.scene
        staging=bpy.data.collections.new('PROPS_NOVOS'); scene.collection.children.link(staging)
        layer=bpy.context.view_layer.layer_collection.children.get(staging.name)
        bpy.context.view_layer.active_layer_collection=layer
        extras=props()
        if kind==0:
            for name,o in extras.items():
                o.data.calc_loop_triangles()
                manifests.append({'id':name,'tipo':'prop','arquivo':'props/'+name+'.fbx','glb':'props/'+name+'.glb',
                                  'dimensoes':list(o.dimensions),'tris':len(o.data.loop_triangles),
                                  'pivo':[0,0,0],'estado':'entregue','integracao':'pendente_claude',
                                  'regra':'decoracao','emissivo':name=='relicario_solar'})
        staging.hide_render=True
        demo=bpy.data.collections.new('ESTUDO_'+map_id); scene.collection.children.link(demo)
        bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children.get(demo.name)
        data=layout(kind,12)
        inst(originals['entorno'],demo,(0,0,0))
        for y,row in enumerate(data['alturas']):
            for x,height in enumerate(row):
                px,py=x-5.5,y-5.5
                for level in range(height): inst(originals['coluna_a' if (x+y+level)%2==0 else 'coluna_b'],demo,(px,py,level*.45))
                zone=x in (5,6) and y in (5,6)
                name='topo_zona' if zone else 'topo_a' if (x+y)%2==0 else 'topo_b'
                inst(originals[name],demo,(px,py,height*.45))
        for p in data['props']:
            x,y=p['casa']; src=extras.get(p['peca'],originals.get(p['peca']))
            inst(src,demo,(x-5.5,y-5.5,data['alturas'][y][x]*.45+.05),math.radians(p['rotacaoGraus']))
        inst(originals['loja_mascate'],demo,(0,0,.5))
        for n in (12,16,18):
            (ROOT/'layouts'/f'{map_id}-{n}.json').write_text(json.dumps(layout(kind,n),ensure_ascii=False,indent=2),encoding='utf-8')
        scene.render.engine='CYCLES'; scene.cycles.samples=8; scene.cycles.use_denoising=True
        scene.render.resolution_x=1000; scene.render.resolution_y=750; scene.render.resolution_percentage=100
        scene.render.image_settings.file_format='PNG'; scene.render.film_transparent=False
        scene.view_settings.view_transform='Standard'; scene.world.color=(.13,.16,.19)
        bpy.ops.object.light_add(type='SUN',location=(-6,-7,12)); sun=bpy.context.object; sun.data.energy=2; aim(sun)
        bpy.ops.object.light_add(type='AREA',location=(0,0,12)); fill=bpy.context.object; fill.data.energy=400; fill.data.size=10; aim(fill)
        cameras=[]
        for seat,position in enumerate(CAMERAS):
            bpy.ops.object.camera_add(location=position); camera=bpy.context.object
            camera.name=f'VISTA_JOGADOR_{seat+1}'; camera.data.type='ORTHO'; camera.data.ortho_scale=27; aim(camera)
            cameras.append(camera)
        for im in bpy.data.images:
            if im.source=='FILE' and not im.packed_file: im.pack()
        scene.camera=cameras[0]; scene['estado']='estudo_visual; integracao e jogo pendentes'
        scene['rotacao']='somente camera; coordenadas e props identicos em todas as vistas'
        bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'fontes'/(map_id+'.blend')))
        previews=[]
        for seat,camera in enumerate(cameras):
            scene.camera=camera; file=ROOT/'previews'/f'{map_id}-jogador-{seat+1}.png'
            scene.render.filepath=str(file); bpy.ops.render.render(write_still=True); previews.append(str(file.relative_to(ROOT)))
            print('VISTA_OK',map_id,seat+1,flush=True)
        manifests.append({'id':map_id,'tipo':'mapa','tema':theme,'fonte':'fontes/'+map_id+'.blend',
                          'layout':'layouts/'+map_id+'-12.json','dimensoesSuportadas':[12,16,18],
                          'previews':previews,'camerasBlender':CAMERAS,'cameraLocalPorJogador':True,
                          'estado':'entregue','integracao':'pendente_claude','mapaJogavelValidado':False})
    files={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in ROOT.rglob('*') if p.is_file() and p.suffix in ('.png','.blend','.json','.glb','.fbx')}
    (ROOT/'manifesto.json').write_text(json.dumps({'versao':1,'data':'2026-10-10','assets':manifests,'hashes':files,
        'nota':'Estudos visuais editaveis. Claude valida regras, colisao, spawns e alcance antes de incluir no jogo. Nao muda MapGen.'},ensure_ascii=False,indent=2),encoding='utf-8')
    print('MAPAS_ROTATIVOS_OK',len(manifests),flush=True)

main()
