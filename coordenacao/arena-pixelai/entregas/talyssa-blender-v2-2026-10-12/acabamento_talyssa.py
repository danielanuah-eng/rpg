"""Acabamento da amostra estatica: sobrancelha projetada, cabelos e camadas."""
import bpy,math,random,json
from mathutils import Vector
from mathutils.bvhtree import BVHTree
from pathlib import Path
from math import sin,cos,pi,exp
P=Path(__file__).resolve().parent;random.seed(14)
bpy.ops.wm.open_mainfile(filepath=str(P/'Talyssa-v2-refinada.blend'))
col=bpy.data.collections['TALYSSA — modelo editavel'];sc=bpy.context.scene
head=bpy.data.objects['Cabeca anatomica adaptada — fonte CC0 MakeHuman']
for o in list(col.objects):
 if o.name.startswith('Fio sobrancelha'):bpy.data.objects.remove(o,do_unlink=True)
bpy.context.view_layer.update();tree=BVHTree.FromObject(head,bpy.context.evaluated_depsgraph_get())
def surf(x,z,extra=.0007):
 p,n,i,d=tree.ray_cast(Vector((x,-2,z)),Vector((0,1,0)),3)
 return (x,p.y-extra,z) if p is not None else (x,-.105,z)
def path(name,pts,r,mat):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.bevel_depth=r;c.bevel_resolution=3;c.resolution_u=10
 s=c.splines.new('BEZIER');s.bezier_points.add(len(pts)-1)
 for b,p in zip(s.bezier_points,pts):b.co=p;b.handle_left_type='AUTO';b.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);col.objects.link(o);c.materials.append(mat);return o
def mesh(name,vs,fs,mat,sub=1):
 me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new(name,me);col.objects.link(o);me.materials.append(mat)
 for p in me.polygons:p.use_smooth=True
 if sub:m=o.modifiers.new('Mecha suave','SUBSURF');m.levels=sub;m.render_levels=sub
 return o
brown=bpy.data.materials['Couro castanho'];black=bpy.data.materials['Pupilas e cilios'];silver=bpy.data.materials['Cabelo — prata marfim'];light=bpy.data.materials['Cabelo — fios claros']
for s in [-1,1]:
 # Duas camadas de fios cobrem o arco, acompanhando a pele real.
 for k in range(48):
  t=k/47;x=s*(.015+.045*t);z=1.636+.005*sin(t*pi)-.005*t
  length=.0045*(1-.65*t)
  path('Sobrancelha castanha',[surf(x,z),surf(x+s*.002,z+length)],.00075 if t<.7 else .0005,brown)
 # Delineado rente à abertura superior, baseado nos vértices de borda do olho.
 # Trajeto sobre a pele, acima da abertura: sombra delicada, sem aro grosso.
 for k in range(20):
  t=k/19;x=s*(.018+.037*t);z=1.618+.007*sin(pi*t)
  path('Cilio fino',[surf(x,z,.0005),surf(x+s*.0007,z+.0015,.0008)],.00036,black)

# Variação de cor na pele e lábios pintada em atributo da malha, não textura de conceito.
attr=head.data.color_attributes.new(name='Tons anatomicos',type='FLOAT_COLOR',domain='POINT')
for v,c in zip(head.data.vertices,attr.data):
 x,y,z=v.co;base=Vector((.34,.165,.090))
 cheeks=.08*exp(-((abs(x)-.052)/.026)**2-((z-1.59)/.025)**2) if y<-.05 else 0
 base=base.lerp(Vector((.35,.12,.075)),cheeks)
 # boca da malha anatomica em z ~1.547; elipse de cor suave nos dois labios.
 lw=exp(-((x/.027)**6)-(((z-1.544)/.010)**6)) if y<-.102 else 0
 base=base.lerp(Vector((.24,.075,.042)),lw*.6)
 c.color=(*base,1)
skin=bpy.data.materials['Pele — castanho quente'];nt=skin.node_tree;p=nt.nodes.get('Principled BSDF');node=nt.nodes.new('ShaderNodeVertexColor');node.layer_name='Tons anatomicos';nt.links.new(node.outputs['Color'],p.inputs['Base Color'])
# Somente cabeça usa a variação; o resto recebe material independente sem atributo.
for o in col.objects:
 if o.type!='MESH' or o==head:continue
 for slot in o.material_slots:
  if slot.material==skin:
   m=skin.copy();m.name='Pele corpo sem atributo';n=m.node_tree.nodes.get('Principled BSDF')
   for l in list(m.node_tree.links):
    if l.to_socket==n.inputs['Base Color']:m.node_tree.links.remove(l)
   n.inputs['Base Color'].default_value=(.34,.165,.090,1);slot.material=m
# Remove a antiga região de lábios com limite inadequado.
for poly in head.data.polygons:poly.material_index=0

# Franja em mechas tridimensionais estreitas e pontas assimetricas.
def bez(ps,t):return (1-t)**3*Vector(ps[0])+3*t*(1-t)**2*Vector(ps[1])+3*t*t*(1-t)*Vector(ps[2])+t**3*Vector(ps[3])
for k in range(25):
 t=k/24
 ps=[(.045-.026*t,-.005,1.768-.009*t),(.01-.040*t,-.091,1.757-.005*t),(-.028-.05*t,-.115,1.699-.025*t),(-.02-.075*t,-.119,1.663-.028*t)]
 if k>17:
  ps=[(.049,-.014,1.75),(.051+(k-17)*.003,-.088,1.735),(.061+(k-17)*.002,-.108,1.692),(.07+(k-17)*.002,-.098,1.665-(k%3)*.009)]
 vs=[];N=24;M=9;w=.006+(k%4)*.001
 for i in range(N):
  u=i/(N-1);v=bez(ps,u);tangent=(bez(ps,min(1,u+.01))-bez(ps,max(0,u-.01))).normalized();side=tangent.cross(Vector((0,-1,0))).normalized();ww=w*(.55+.75*sin(pi*u))*(1-u*u)+.00015
  for j in range(M):
   q=-1+2*j/(M-1);vs.append(v+side*q*ww+Vector((0,-1,0))*ww*.50*(1-q*q))
 o=mesh('Franja detalhada',vs,[(i*M+j,(i+1)*M+j,(i+1)*M+j+1,i*M+j+1) for i in range(N-1) for j in range(M-1)],silver)
 m=o.modifiers.new('Volume da mecha','SOLIDIFY');m.thickness=.0018
 path('Fio de brilho franja',[bez(ps,u)+Vector((0,-.0018,0)) for u in [j/20 for j in range(2,20)]],.00023,light)

# Separação efetiva das camadas de tecido e correias.
for o in col.objects:
 if o.type=='MESH' and o.name.startswith('Pelerine tecido'):
  for v in o.data.vertices:
   v.co.z+=.018;v.co.y=(v.co.y-.008)*1.36+.008
 if o.type=='MESH' and o.name.startswith('Arnes diagonal'):
  for v in o.data.vertices:
   if v.co.z>1.245:v.co.y=min(v.co.y,-.096)
 if o.type=='CURVE' and o.name.startswith('Costura arnes'):
  for spline in o.data.splines:
   for b in spline.bezier_points:
    if b.co.z>1.245:b.co.y=min(b.co.y,-.101)

# Espessura de fivelas mais nítida: cantos deixam de ser círculos suaves.
for o in col.objects:
 if o.type=='CURVE' and 'Fivela' in o.name:
  for s in o.data.splines:
   for b in s.bezier_points:b.handle_left_type='VECTOR';b.handle_right_type='VECTOR'

# Pose leve de contrapposto aplicada às geometrias de estudo (nenhum rig alegado).
def pose(v):
 p=Vector(v);p.x+=.018*sin(pi*max(0,min(1,p.z/1.8)));p.z+=.021*p.x
 return p
for o in col.objects:
 if o.type=='MESH':
  mw=o.matrix_world.copy();inv=mw.inverted()
  for v in o.data.vertices:v.co=inv@pose(mw@v.co)
 elif o.type=='CURVE':
  mw=o.matrix_world.copy();inv=mw.inverted()
  for s in o.data.splines:
   for b in s.bezier_points:b.co=inv@pose(mw@b.co)
sc.camera=bpy.data.objects['Camera rosto'];sc.render.resolution_x=1100;sc.render.resolution_y=1100
sc.render.filepath=str(P/'talyssa-rosto-refinado.png');sc.cycles.samples=64
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-v2-refinada.blend'))
bpy.ops.render.render(write_still=True)
sc.camera=bpy.data.objects['Camera corpo'];sc.render.resolution_x=1050;sc.render.resolution_y=1400
sc.render.filepath=str(P/'talyssa-corpo-refinado.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-v2-refinada.blend'))
print('ACABAMENTO_OK',flush=True)
