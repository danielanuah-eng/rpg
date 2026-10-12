"""Revisa a amostra com base anatomica CC0 e corrige penetracoes da roupa.
Le o .blend desta mesma entrega. Nao modifica fontes externas.
"""
import bpy,json,math,sys
from pathlib import Path
from mathutils import Vector
from math import sin,cos,pi
OUT=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(OUT/'Talyssa-v2.blend'))
scene=bpy.context.scene;col=bpy.data.collections['TALYSSA — modelo editavel']

def mat(s):return bpy.data.materials[s]
skin=mat('Pele — castanho quente');white=mat('Esclera marfim');iris=mat('Iris castanha');pupil=mat('Pupilas e cilios')
lip=mat('Labios — terracota discreta');green=mat('Capa — linho verde floresta')

def mesh(name,vs,fs,m,sub=0):
 me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new(name,me);col.objects.link(o);me.materials.append(m)
 for f in me.polygons:f.use_smooth=True
 if sub:
  mod=o.modifiers.new('Subdivisao anatomica','SUBSURF');mod.levels=sub;mod.render_levels=sub
 return o

def ell(name,p,s,m):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=40,ring_count=24,radius=1,location=p);o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(m)
 for c in list(o.users_collection):c.objects.unlink(o)
 col.objects.link(o)
 for f in o.data.polygons:f.use_smooth=True
 return o

def path(name,pts,r,m):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.bevel_depth=r;c.bevel_resolution=3;c.resolution_u=12
 s=c.splines.new('BEZIER');s.bezier_points.add(len(pts)-1)
 for b,p in zip(s.bezier_points,pts):b.co=p;b.handle_left_type='AUTO';b.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);col.objects.link(o);c.materials.append(m);return o

remove=['Rosto continuo','Orelha','Helice da orelha','Cavidade auricular','Olho encaixado','Iris','Pupila','Reflexo ocular','Palpebra','Linha delicada','Sobrancelha','Narina','Labio','Separacao dos labios','Dobra cachecol','Pelerine de ombro']
for o in list(col.objects):
 if any(o.name.startswith(n) for n in remove):bpy.data.objects.remove(o,do_unlink=True)

data=json.loads((OUT/'base-cc0/parsed.json').read_text())
src=data['vertices'];groups=data['groups']
def tr(p):return (p[0]*.128,-(p[2]-.40)*.115,1.487+(p[1]-5.8)*.132)
faces=[f for g,f in data['faces'] if g=='body' and min(src[i][1] for i in f)>5.32]
ids=sorted(set(i for f in faces for i in f));remap={n:i for i,n in enumerate(ids)}
head=mesh('Cabeca anatomica adaptada — fonte CC0 MakeHuman',[tr(src[i]) for i in ids],[[remap[i] for i in f] for f in faces],skin,2)
head['origem']='MakeHuman Community base.obj + morphs female-young, CC0-1.0; ver base-cc0/origem.json'
head['transformacao']='Cabeca recortada e adaptada ao estudo Talyssa; nao e um escaneamento'
head.data.materials.append(lip)
# Cor discreta dos labios, limitada a regiao anterior.
for poly in head.data.polygons:
 p=poly.center
 # centros calculados depois do update; coordenadas mundiais pois vertices ja transformados
 if abs(p.x)<.028 and 1.555<p.z<1.574 and p.y<-.112:poly.material_index=1
for side in ['l','r']:
 ids=sorted(set(groups['helper-'+side+'-eye']));remap={n:i for i,n in enumerate(ids)}
 ef=[f for g,f in data['faces'] if g=='helper-'+side+'-eye']
 eye=mesh('Olho anatomico '+side,[tr(src[i]) for i in ids],[[remap[i] for i in f] for f in ef],white,2)
 points=[tr(src[i]) for i in ids];cx=sum(v[0] for v in points)/len(points);cz=sum(v[2] for v in points)/len(points);yy=min(v[1] for v in points)
 ell('Iris natural '+side,(cx,yy-.0002,cz),(.0069,.0011,.0070),iris)
 ell('Pupila natural '+side,(cx,yy-.0012,cz),(.0029,.0005,.0031),pupil)
 ell('Reflexo natural '+side,(cx-.002,yy-.0018,cz+.0022),(.0008,.0003,.0008),white)
 # Sobrancelhas castanhas curtas em dezenas de fios, nao tubo branco.
 s=1 if side=='l' else -1
 for k in range(25):
  t=k/24;x=s*(.015+t*.048);z=1.646+.008*sin(t*pi)-.008*t
  # superfície proxima à testa do modelo anatomico
  y=-.099+.030*t*t
  path('Fio sobrancelha',[(x,y,z),(x+s*.003,y-.001,z+.0035)],.0006,mat('Cabelo — sombra de mechas'))

# Tecido torcido do cachecol, com superfície angular e dobras em vez de três toros.
vv=[];na=112;nv=22
for i in range(na):
 a=2*pi*i/na
 for j in range(nv):
  t=j/(nv-1)
  r=.078+.014*sin(t*pi)+.005*sin(t*5*pi+a*2)
  x=r*cos(a);y=.01+r*.87*sin(a)
  z=1.409+t*.065+.007*sin(a*2+t*2)+.012*cos(a)
  vv.append((x,y,z))
fs=[(i*nv+j,((i+1)%na)*nv+j,((i+1)%na)*nv+j+1,i*nv+j+1) for i in range(na) for j in range(nv-1)]
o=mesh('Cachecol com dobras de tecido',vv,fs,green,1);mod=o.modifiers.new('Tecido espesso','SOLIDIFY');mod.thickness=.003
for s in [-1,1]:
 vv=[];N=14;M=18
 for i in range(N):
  t=i/(N-1)
  for j in range(M):
   q=j/(M-1);a=-.8*pi+q*1.45*pi
   x=s*(.050+.17*t);y=.008+(.033+.040*t)*sin(a)
   z=1.445-.068*t-.020*(1-cos(a))+.007*sin(q*4*pi)*t
   vv.append((x,y,z))
 o=mesh('Pelerine tecido moldado',vv,[(i*M+j,(i+1)*M+j,(i+1)*M+j+1,i*M+j+1) for i in range(N-1) for j in range(M-1)],green,1)
 m=o.modifiers.new('Espessura','SOLIDIFY');m.thickness=.003

# Corrigir penetrações concretas vistas no render e reduzir brilho excessivo.
for o in list(col.objects):
 if o.type!='MESH':continue
 if o.name.startswith('Tunica interna'):
  for v in o.data.vertices:
   if v.co.z<1.26:v.co.x*=.91;v.co.y*=.85
 if o.name.startswith('Aba de tunica'):
  for v in o.data.vertices:v.co.y-=.035
 if o.name.startswith('Antebraco'):
  for v in o.data.vertices:
   if v.co.z>1.16:v.co.z+=.02
 if o.name.startswith('Bracadeira de couro'):
  for v in o.data.vertices:
   v.co.x+=.001 if v.co.x>0 else -.001
   v.co.y-=.003
 if o.name.startswith('Manga curta'):
  for v in o.data.vertices:
   s=1 if v.co.x>0 else -1
   v.co.x+=s*.005;v.co.y*=1.2;v.co.z-=.002
 if o.name.startswith('Bota pe'):
  for v in o.data.vertices:
   if v.co.y<-.075:v.co.z*=.84
 if o.name.startswith('Joelheira'):
  for v in o.data.vertices:v.co.y-=.006
for m in bpy.data.materials:
 if not m.use_nodes:continue
 p=m.node_tree.nodes.get('Principled BSDF')
 if not p:continue
 if m.name.startswith('Pele'):
  p.inputs['Roughness'].default_value=.66;p.inputs['Specular IOR Level'].default_value=.24
 if m.name.startswith('Couro'):
  p.inputs['Roughness'].default_value=.72;p.inputs['Specular IOR Level'].default_value=.26
  for n in m.node_tree.nodes:
   if n.type=='BUMP':n.inputs['Strength'].default_value=.42;n.inputs['Distance'].default_value=.001
 if m.name.startswith('Capa') or m.name.startswith('Tunica'):
  p.inputs['Sheen Weight'].default_value=.18
  for n in m.node_tree.nodes:
   if n.type=='BUMP':n.inputs['Strength'].default_value=.5;n.inputs['Distance'].default_value=.0012
scene.cycles.samples=48
scene.camera=bpy.data.objects['Camera rosto'];scene.render.resolution_x=1100;scene.render.resolution_y=1100
scene.render.filepath=str(OUT/'talyssa-rosto-refinado.png')
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Talyssa-v2-refinada.blend'))
bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera corpo'];scene.render.resolution_x=1050;scene.render.resolution_y=1400
scene.render.filepath=str(OUT/'talyssa-corpo-refinado.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Talyssa-v2-refinada.blend'))
print('AMOSTRA_REFINADA_OK',flush=True)
