"""Verificação geométrica do estudo; não substitui os testes de jogabilidade do Claude."""
from pathlib import Path
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
from collections import deque
import bpy, hashlib, json

ROOT=Path(__file__).resolve().parent
def main():
    manifest=json.loads((ROOT/'manifesto.json').read_text(encoding='utf-8'))
    for file,sha in manifest['hashes'].items(): assert hashlib.sha256((ROOT/file).read_bytes()).hexdigest()==sha,file
    asset=manifest['assets'][0]; reports=[]
    for n in (12,16,18):
        data=json.loads((ROOT/'layouts'/f"{asset['id']}-{n}.json").read_text(encoding='utf-8'))
        heights=data['alturas']; assert len(heights)==n and all(len(row)==n for row in heights)
        assert all(isinstance(z,int) and 0<=z<=2 for row in heights for z in row)
        assert all(heights[y][x]==heights[n-1-y][n-1-x] for y in range(n) for x in range(n)), 'Relevo simétrico em 180 graus'
        positions=[tuple(p['casa']) for p in data['props']]
        assert len(positions)==len(set(positions))
        assert all(0<=x<n and 0<=y<n for x,y in positions)
        center={n//2-1,n//2}
        assert all(not (x in center and y in center) for x,y in positions)
        assert all(heights[y][x]==1 for x in center for y in center)
        assert all(heights[y][x]==0 for x,y in positions), 'Props devem ficar nas laterais baixas'
        high={(x,y) for y,row in enumerate(heights) for x,z in enumerate(row) if z==2}
        components=[]
        while high:
            queue=deque([high.pop()]); group=set(queue)
            while queue:
                x,y=queue.popleft()
                for p in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                    if p in high: high.remove(p); group.add(p); queue.append(p)
            components.append(group)
        assert len(components)==2 and all(len(group)>=2 for group in components), 'Dois patamares contíguos'
        blocked=set(positions)
        queue=deque([(0,0)]); visited={(0,0)}
        while queue:
            x,y=queue.popleft()
            for xx,yy in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
                if 0<=xx<n and 0<=yy<n and (xx,yy) not in blocked and (xx,yy) not in visited and abs(heights[yy][xx]-heights[y][x])<=1:
                    visited.add((xx,yy)); queue.append((xx,yy))
        assert {(0,0),(n-1,0),(n-1,n-1),(0,n-1)}<=visited
        assert all((x,y) in visited for x in center for y in center)
        reports.append({'dimension':n,'uniqueProps':len(positions),'propsOnLowGround':True,'connectedHighPlatforms':[len(group) for group in components],'cornerConnectivityWithOneLevelSteps':True,'centralZoneClear':True,'terrainSymmetric180':True,'mapaJogavelValidado':False})
    bpy.ops.wm.open_mainfile(filepath=str(ROOT/asset['fonte']))
    scene=bpy.context.scene; collection=bpy.data.collections['MAPA_'+asset['id']]
    tiles=[o for o in collection.objects if o.get('peca','').startswith('topo_')]
    assert len(tiles)==144
    assert len({(round(o.location.x,3),round(o.location.y,3)) for o in tiles})==144
    assert sum(o.get('peca')=='topo_zona' for o in tiles)==4
    original=json.loads((ROOT/'layouts'/f"{asset['id']}-12.json").read_text(encoding='utf-8'))
    for tile in tiles:
        x,y=round(tile.location.x+5.5),round(tile.location.y+5.5)
        assert abs(tile.location.z-original['alturas'][y][x]*.45)<1e-5
    cameras=[o for o in scene.objects if o.type=='CAMERA']; assert len(cameras)==4
    framing=[]
    for camera in sorted(cameras,key=lambda o:o.name):
        assert camera.data.type=='ORTHO' and abs(camera.data.ortho_scale-27)<1e-5
        # Tabuleiro e seus props; o entorno é uma moldura exterior decorativa.
        points=[world_to_camera_view(scene,camera,o.matrix_world@Vector(corner)) for o in collection.objects if o.type=='MESH' and o.get('peca')!='entorno' for corner in o.bound_box]
        assert all(0.02<=p.x<=.98 and .02<=p.y<=.98 and p.z>0 for p in points),camera.name
        framing.append({'camera':camera.name,'location':list(camera.location),'projectedBounds':[min(p.x for p in points),min(p.y for p in points),max(p.x for p in points),max(p.y for p in points)]})
    textures=[im for im in bpy.data.images if im.source=='FILE']; assert textures and all(im.packed_file for im in textures)
    report={'passed':True,'layouts':reports,'tileCount':144,'packedTextures':len(textures),'framing':framing,'sameBoardFourViews':True,'integration':'pendente_claude','gameplayValidation':'Somente conectividade abstrata: Claude verifica regras reais, colisão, spawns, alcance, câmera local e mobilidade no Unity.'}
    (ROOT/'verificacao.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False))

if __name__=='__main__':main()
