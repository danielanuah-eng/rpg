"""Verifica/exporta props e confere enquadramento das quatro câmeras no Blender."""
from pathlib import Path
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
import bpy, json, hashlib

ROOT=Path(__file__).resolve().parent
report={'status':'passed','props':[],'maps':[],'gameplayValidated':False}
# O último tema exportado por construir.py foi neve; copiar/embutir suas texturas no FBX.
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'fontes/neve-bastioes-diagonais.blend'))
for name in ('relicario_solar','barricada_partida'):
    obj=bpy.data.objects.get('novo_'+name)
    assert obj is not None, name
    bpy.ops.object.select_all(action='DESELECT'); obj.select_set(True)
    bpy.context.view_layer.objects.active=obj
    bpy.ops.export_scene.fbx(filepath=str(ROOT/'props'/f'{name}.fbx'),use_selection=True,axis_forward='-Z',axis_up='Y',apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,path_mode='COPY',embed_textures=True)
for name in ('relicario_solar','barricada_partida'):
    for ext in ('glb','fbx'):
        bpy.ops.wm.read_factory_settings(use_empty=True)
        path=ROOT/'props'/f'{name}.{ext}'
        if ext=='glb': bpy.ops.import_scene.gltf(filepath=str(path))
        else: bpy.ops.import_scene.fbx(filepath=str(path))
        meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
        assert len(meshes)==1,(name,ext,len(meshes))
        o=meshes[0]; o.data.calc_loop_triangles()
        verts=[o.matrix_world@v.co for v in o.data.vertices]
        # Exportações Y-up são convertidas novamente pelo importador Blender.
        span=[max(v[a] for v in verts)-min(v[a] for v in verts) for a in range(3)]
        assert max(span)<1.01 and len(o.data.loop_triangles)<3000,(name,ext,span)
        assert abs(min(v.z for v in verts))<.001,(name,ext,'base pivot')
        textures={n.image for m in o.data.materials if m and m.use_nodes for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image}
        assert all(len(im.pixels)>0 for im in textures),(name,ext,'missing texture')
        report['props'].append({'id':name,'format':ext,'dimensions':span,'tris':len(o.data.loop_triangles),'texturesLoaded':len(textures),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
for path in sorted((ROOT/'fontes').glob('*.blend')):
    bpy.ops.wm.open_mainfile(filepath=str(path))
    scene=bpy.context.scene; cameras=[o for o in scene.objects if o.type=='CAMERA']
    assert len(cameras)==4,(path.name,'camera count',len(cameras))
    fits=[]
    for cam in cameras:
        assert cam.data.type=='ORTHO'
        points=[world_to_camera_view(scene,cam,Vector((x,y,z))) for x in (-6,6) for y in (-6,6) for z in (0,1.35)]
        assert all(0<=p.x<=1 and 0<=p.y<=1 and p.z>0 for p in points),(path.name,cam.name,'board clipped')
        fits.append(cam.name)
    report['maps'].append({'file':path.relative_to(ROOT).as_posix(),'orthographicCameras':fits,'wholeBoardFits':True})
(ROOT/'verificacao-blender.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('VERIFICACAO_MAPAS_OK')
