"""Revisao localizada de Talyssa: rosto e cabelo, preservando corpo e roupa V2.
Blender 5.2: -b --python-exit-code 1 --python refinar_rosto_cabelo.py [-- --final]
"""
import bpy,math,random,json,hashlib,struct,sys
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from pathlib import Path
from math import sin,cos,pi,exp,sqrt
P=Path(__file__).resolve().parent
SOURCE=P.parent/'talyssa-blender-v2-2026-10-12/Talyssa-v2-amostra.blend'
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
sc=bpy.context.scene;c=bpy.data.collections['TALYSSA — modelo editavel'];random.seed(571)
FINAL='--final' in sys.argv

def signature(o):
 h=hashlib.sha256();h.update(o.type.encode())
 for row in o.matrix_world:
  h.update(struct.pack('<4f',*row))
 if o.type=='MESH':
  for v in o.data.vertices:h.update(struct.pack('<3f',*v.co))
 elif o.type=='CURVE':
  for s in o.data.splines:
   for p in s.bezier_points:h.update(struct.pack('<3f',*p.co))
   for p in s.points:h.update(struct.pack('<4f',*p.co))
 return h.hexdigest()

def is_head(o):
 return o.name.startswith(('Cabeca','Cabelo','Volume raiz','Mecha','Nuca','Franja','Fio sobre','Fio de brilho','Sobrancelha','Cilio','Olho anatomico','Iris natural','Pupila natural','Reflexo natural'))
body_before={o.name:signature(o) for o in c.objects if not is_head(o)}
lights_before={o.name:[list(row) for row in o.matrix_world] for o in sc.objects if o.type in ('LIGHT','CAMERA')}

def material(name,color,rough=.45):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough
 return m

def mesh(name,vs,fs,mat):
 me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new(name,me);c.objects.link(o);me.materials.append(mat)
 for p in me.polygons:p.use_smooth=True
 return o

def curve_object(name,r,materials,bevel=2):
 data=bpy.data.curves.new(name,'CURVE');data.dimensions='3D';data.bevel_depth=r;data.bevel_resolution=bevel;data.resolution_u=1
 for m in materials:data.materials.append(m)
 o=bpy.data.objects.new(name,data);c.objects.link(o);return o

def strand(obj,points,radii,mat=0):
 s=obj.data.splines.new('POLY');s.points.add(len(points)-1);s.material_index=mat
 for p,co,r in zip(s.points,points,radii):p.co=(*co,1);p.radius=r

# A V2 é carregada de novo a cada execução; nenhuma deformação se acumula.
head=bpy.data.objects['Cabeca anatomica adaptada — fonte CC0 MakeHuman']
cx=.006
def gaussian(x,z,x0,z0,rx,rz):return exp(-((x-x0)/rx)**2-((z-z0)/rz)**2)
for v in head.data.vertices:
 x,y,z=v.co;x-=cx
 if y<-.035:
  cheek=gaussian(abs(x),z,.051,1.586,.026,.022)
  v.co.y-=.0024*cheek
  chin=gaussian(x,z,0,1.501,.047,.023)
  v.co.x-=x*.14*chin
  # Fechar ligeiramente os labios, com cantos em repouso menos caidos.
  mouth=gaussian(x,z,0,1.535,.028,.009)
  v.co.z+=(.00075 if z<1.535 else -.0003)*mouth
  v.co.z+=.0030*gaussian(abs(x),z,.025,1.537,.010,.012)
  close_lips=gaussian(x,z,0,1.539,.029,.007)
  v.co.z+=(1.539-z)*.65*close_lips
  v.co.y+=.0013*gaussian(x,z,0,1.554,.008,.008)
  # Nariz um pouco menos projetado; preservar a malha e os detalhes anatomicos.
  nose=gaussian(x,z,0,1.575,.021,.014)
  v.co.y+=.0017*nose;v.co.x-=x*.055*nose
  # Pálpebra superior menos aberta e leve volume da pálpebra inferior.
  upper=gaussian(abs(x),z,.0365,1.625,.024,.0038)
  lower=gaussian(abs(x),z,.0365,1.611,.022,.0035)
  v.co.z-=.0013*upper;v.co.z+=.0005*lower
  v.co.y-=.0006*lower
head.data.update()
attr=head.data.color_attributes.get('Tons anatomicos')
if attr:
 for v,p in zip(head.data.vertices,attr.data):
  x,y,z=v.co;x-=cx
  if y<-.097:
   lip=gaussian(x,z,0,1.535,.027,.008)
   base=Vector(p.color[:3]);base=base.lerp(Vector((.235,.104,.084)),lip*.36);p.color=(*base,1)
for n in head.active_material.node_tree.nodes:
 if n.type=='BUMP':n.inputs['Strength'].default_value=.30

# O olhar ganha iris escura com variacao radial; brilho vem do material real.
im=bpy.data.materials['Iris castanha'];nt=im.node_tree;p=nt.nodes.get('Principled BSDF')
p.inputs['Base Color'].default_value=(.075,.031,.010,1);p.inputs['Roughness'].default_value=.24
p.inputs['Coat Weight'].default_value=.55;p.inputs['Coat Roughness'].default_value=.045
noise=nt.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=28;noise.inputs['Detail'].default_value=2
ramp=nt.nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].color=(.012,.006,.004,1);ramp.color_ramp.elements[1].color=(.080,.030,.012,1)
nt.links.new(noise.outputs['Fac'],ramp.inputs[0]);nt.links.new(ramp.outputs['Color'],p.inputs['Base Color'])
wm=bpy.data.materials['Esclera marfim'];wm.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.45,.425,.385,1)
for o in list(c.objects):
 if o.name.startswith(('Iris natural','Pupila natural')):
  o.location.x+=.00075;o.location.z+=.0010
 if o.name.startswith('Reflexo natural'):o.hide_render=True;o.hide_viewport=True
 if o.name.startswith(('Sobrancelha','Cilio fino')):bpy.data.objects.remove(o,do_unlink=True)

bpy.context.view_layer.update();tree=BVHTree.FromObject(head,bpy.context.evaluated_depsgraph_get())
def on_skin(x,z,offset=.00045):
 hit,*_=tree.ray_cast(Vector((x,-1,z)),Vector((0,1,0)),2)
 return Vector((x,hit.y-offset,z)) if hit is not None else Vector((x,-.1,z))
browmat=material('Sobrancelha — fios castanhos',(.045,.025,.017),.64)
brows=curve_object('Sobrancelhas naturais V3',.00034,[browmat],2)
eyelashmat=material('Margem ciliar castanha',(.025,.013,.010),.55)
lashes=curve_object('Cilios delicados V3',.00019,[eyelashmat],2)
for side in [-1,1]:
 for k in range(120):
  t=(k+random.random()*.8)/121
  x=cx+side*(.012+.049*t);z=1.635+.0055*sin(pi*t)-.003*t+random.uniform(-.0008,.0007)
  ln=random.uniform(.002,.0046)*(1-.50*t)
  pts=[on_skin(x,z),on_skin(x+side*.0011,z+ln*.7),on_skin(x+side*.0025,z+ln)]
  strand(brows,pts,[.65,1,.04])
 # Acento contíguo junto à pálpebra, sem a antiga série de traços verticais.
 pts=[]
 for k in range(35):
  t=k/34;x=cx+side*(.016+.039*t);z=1.6175+.0065*sin(pi*t)-.0004*t
  pts.append(on_skin(x,z,.00020))
 strand(lashes,pts,[.15+.7*sin(pi*k/34) for k in range(35)])

# Retira da apresentação a construção anterior de cabelo e suas guias.
for o in list(c.objects):
 if o.name.startswith(('Cabelo','Volume raiz','Mecha','Nuca','Franja','Fio sobre','Fio de brilho')):
  o.hide_render=True;o.hide_viewport=True

hairmats=[material('Prata cabelo V3 '+str(i),co,.43) for i,co in enumerate([(.40,.415,.405),(.52,.535,.515),(.65,.66,.63),(.32,.34,.33)])]
capmat=material('Subcabelo prata escura',(.40,.42,.40),.80)
center=Vector((cx,.019,1.674));rx=.104;ry=.099;rz=.093
def scalp(theta,phi,lift=0):
 v=Vector((rx*sin(theta)*cos(phi),ry*sin(theta)*sin(phi),rz*cos(theta)))
 direction=v.normalized()
 hit,*_=tree.ray_cast(center,direction,.35)
 # Base aderente ao cranio real; evita a abertura escura sob uma touca elipsoidal.
 if hit is not None:return hit+direction*(.002+lift)
 return center+v+direction*lift
def cutoff(phi):
 front=max(0,-sin(phi));back=max(0,sin(phi))
 return 1.89-.70*front+.25*back
# Superficie base delgada, con linea frontal irregular bajo la franja.
vs=[];rows=28;cols=100
for i in range(rows):
 t=i/(rows-1)
 for j in range(cols):
  phi=2*pi*j/cols;th=(.012+(cutoff(phi)+.018*sin(phi*9)) * t)
  vs.append(scalp(th,phi,-.0015))
fs=[(i*cols+j,(i+1)*cols+j,(i+1)*cols+(j+1)%cols,i*cols+(j+1)%cols) for i in range(rows-1) for j in range(cols)]
guide_cap=mesh('Base do corte curto V3',vs,fs,capmat)
guide_cap.hide_render=True;guide_cap.hide_viewport=True
guide_cap['funcao']='Guia auxiliar do corte; o cabelo visivel e formado pelos fios, nao por uma touca.'
hair=curve_object('Mechas e fios V3',.00034,hairmats,1)

def bez(ps,t):return (1-t)**3*ps[0]+3*t*(1-t)**2*ps[1]+3*t*t*(1-t)*ps[2]+t**3*ps[3]
def bundle(ps,width,thickness,count,seed,clump=.83):
 rr=random.Random(seed);ps=[Vector(p) for p in ps]
 for k in range(count):
  a=rr.uniform(0,2*pi);r=sqrt(rr.random());offx=cos(a)*r;offy=sin(a)*r
  end=rr.uniform(.83,1.10);lift=rr.uniform(-.0006,.0016);wave=rr.uniform(0,2*pi)
  points=[];radii=[]
  for j in range(25):
   t=j/24;u=t*end
   p=bez(ps,u);tangent=(bez(ps,u+.002)-bez(ps,u-.002)).normalized()
   normal=(p-center).normalized();across=tangent.cross(normal).normalized();normal=across.cross(tangent).normalized()
   spread=(1-.65*t)*(1-clump*t**2)+.04
   p+=across*(offx*width*spread)+normal*(offy*thickness*spread+lift*sin(pi*t))
   p+=across*.00045*sin(wave+t*7)*sin(pi*t)
   points.append(p);radii.append(max(.025,(.80+.2*sin(t*pi))*(1-t**2.3)))
  strand(hair,points,radii,rr.choices([0,1,2,3],[.2,.45,.25,.1])[0])

# Laterais e nuca seguem o cranio com direcao variavel, sem abas retas.
for k in range(105):
 phi=2*pi*(k/105)+random.uniform(-.08,.08)
 maxth=cutoff(phi);start=random.uniform(.40,max(.55,maxth-.35));end=min(maxth+.13,start+random.uniform(.35,.70))
 # Topo varre para esquerda; laterais e nuca acompanham o cranio.
 turn=random.uniform(-.09,.14)
 ps=[scalp(start,phi,.001),scalp(start+(end-start)*.30,phi+turn*.3,.008),scalp(start+(end-start)*.75,phi+turn,.009),scalp(end,phi+turn,.010)]
 bundle(ps,.0045+random.random()*.003,.0026,36,710+k)

# Pelos curtos nas têmporas e nuca cobrem a base sem formar uma faixa escura.
for k in range(150):
 phi=2*pi*k/150;limit=cutoff(phi);th=limit-random.uniform(.08,.32)
 ps=[scalp(th,phi,.001),scalp(th+.06,phi+.025,.003),scalp(th+.12,phi+.045,.003),scalp(min(limit+.035,th+.21),phi+.06,.003)]
 bundle(ps,.0028,.0013,17,4300+k)

# Coroa texturizada com tufos sobrepostos de comprimentos diferentes.
for k in range(36):
 phi=random.uniform(-pi,pi);th=random.uniform(.05,.65)
 turn=-.18 if cos(phi)<0 else .18
 ps=[scalp(th,phi,.001),scalp(th+.25,phi+turn,.011),scalp(th+.48,phi+turn*2,.014),scalp(th+.62,phi+turn*2.5,.015)]
 bundle(ps,.008,.004,45,1900+k)

# Franja assimetrica: mechas com trajectorias e pontas independentes.
for k in range(22):
 t=k/21
 root=Vector((cx+.050-random.uniform(0,.025),.01+random.uniform(-.007,.028),1.753+random.uniform(-.009,.008)))
 end=Vector((cx-.102+.074*t,-.072-.040*sin(t*pi*.7),1.657+.050*t+random.uniform(-.010,.010)))
 mid1=Vector((cx+.025-.030*t,-.040,1.785-random.uniform(0,.012)))
 mid2=end.lerp(mid1,.35)+Vector((-.010,-.017,.013))
 bundle([root,mid1,mid2,end],.007+random.random()*.004,.0055,62,2800+k,.72)
for k in range(7):
 root=Vector((cx+.055,.013,1.751-k*.002))
 end=Vector((cx+.082+random.uniform(-.005,.015),-.060-random.uniform(0,.016),1.650+random.uniform(-.010,.040)))
 bundle([root,root+Vector((.04,-.035,.016)),end+Vector((.011,-.019,.036)),end],.006,.003,45,3600+k)

after={o.name:signature(o) for o in c.objects if o.name in body_before}
assert body_before==after,'Geometria do corpo mudou fora do escopo'
for name,m in lights_before.items():assert m==[list(row) for row in bpy.data.objects[name].matrix_world]
sc['estado']='amostra_rosto_cabelo_v3_para_revisao';sc['feedback_autor']='Rosto e cabelo esquisitos, mas no rumo. Refinar rosto/cabelo; preservar corpo/roupa.'
sc.cycles.samples=64 if FINAL else 28
sc.render.resolution_x=1100 if FINAL else 760;sc.render.resolution_y=sc.render.resolution_x
sc.camera=bpy.data.objects['Camera rosto'];sc.render.filepath=str(P/('rosto-v3.png' if FINAL else 'rosto-estudo.png'))
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-rosto-cabelo-v3.blend'))
bpy.ops.render.render(write_still=True)
if FINAL:
 sc.camera=bpy.data.objects['Camera corpo'];sc.render.resolution_x=1050;sc.render.resolution_y=1400;sc.render.filepath=str(P/'corpo-v3.png');bpy.ops.render.render(write_still=True)
 sc.camera=bpy.data.objects['Camera rosto'];sc.render.resolution_x=1100;sc.render.resolution_y=1100
 bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-rosto-cabelo-v3.blend'))
(P/'preservacao.json').write_text(json.dumps({'corpoRoupaPreservados':body_before==after,'objetosComparados':len(body_before),'camerasLuzesPreservadas':True,'fonte':str(SOURCE),'sha256Fonte':hashlib.sha256(SOURCE.read_bytes()).hexdigest()},ensure_ascii=False,indent=2),encoding='utf8')
print('ROSTO_CABELO_V3_OK',FINAL,len(hair.data.splines),flush=True)
