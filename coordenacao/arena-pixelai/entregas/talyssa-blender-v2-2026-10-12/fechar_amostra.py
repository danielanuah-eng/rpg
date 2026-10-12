"""Última passagem: cabelo em fios e continuidade das superficies do estudo."""
import bpy,bmesh,random,math
from math import sin,pi
from mathutils import Vector
from pathlib import Path
P=Path(__file__).resolve().parent;random.seed(104)
bpy.ops.wm.open_mainfile(filepath=str(P/'Talyssa-v2-refinada.blend'))
c=bpy.data.collections['TALYSSA — modelo editavel'];sc=bpy.context.scene

# A roupa são cascas com espessura; tampas ngon causavam bordas onduladas.
for o in list(c.objects):
 if o.type!='MESH':continue
 if o.name.startswith(('Tunica interna','Corselete','Bracadeira de couro','Manga curta','Bota cano')):
  bm=bmesh.new();bm.from_mesh(o.data)
  bmesh.ops.delete(bm,geom=[f for f in bm.faces if len(f.verts)>=20],context='FACES_ONLY')
  bm.to_mesh(o.data);bm.free();o.data.update()
  mod=o.modifiers.new('Borda real da peca de roupa','SOLIDIFY');mod.thickness=.0025

# Unir braços e antebraços para eliminar junta visual de boneco no cotovelo.
for sign in [-1,1]:
 parts=[]
 for o in c.objects:
  if o.type=='MESH' and o.name.startswith(('Braco anatomico','Antebraco')):
   center=sum((o.matrix_world@v.co).x for v in o.data.vertices)/len(o.data.vertices)
   if center*sign>0:parts.append(o)
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:
  o.select_set(True);bpy.context.view_layer.objects.active=o
  for m in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=m.name)
 if parts:
  bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='Braco continuo '+str(sign)
  m=o.modifiers.new('Uniao anatomica','REMESH');m.mode='VOXEL';m.voxel_size=.0018;m.use_smooth_shade=True
  bpy.ops.object.modifier_apply(modifier=m.name)
  m=o.modifiers.new('Suavizar cotovelo','SMOOTH');m.factor=.6;m.iterations=4

# Substitui as fitas aparentes do cabelo por conjuntos de fios afilados.
hm=bpy.data.materials.new('Fios de cabelo prata');hm.diffuse_color=(.52,.50,.47,1);hm.use_nodes=True
p=hm.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(.52,.50,.47,1);p.inputs['Roughness'].default_value=.37
anisotropic=p.inputs.get('Anisotropic')
if anisotropic is not None:anisotropic.default_value=.72
curve=bpy.data.curves.new('Cabelo em fios editaveis','CURVE');curve.dimensions='3D';curve.resolution_u=1;curve.bevel_depth=.00034;curve.bevel_resolution=1;curve.resolution_u=1
o=bpy.data.objects.new('Cabelo — fios modelados',curve);c.objects.link(o);curve.materials.append(hm)
sources=[]
for ob in list(c.objects):
 if ob.type=='MESH' and ob.name.startswith(('Mecha lateral','Nuca','Franja esculpida','Franja curta','Franja detalhada')):sources.append(ob)
for ob in sources:
 M=9 if ob.name.startswith('Franja detalhada') else 7;N=24
 vs=[ob.matrix_world@v.co for v in ob.data.vertices]
 if len(vs)!=M*N:continue
 for strand in range(85):
  q=random.uniform(.03,.97);qidx=q*(M-1);j=int(qidx);f=qidx-j
  end=random.uniform(.86,1.08);lift=random.uniform(-.0012,.0018);jitter=random.uniform(-.0012,.0012)
  s=curve.splines.new('POLY');s.points.add(N-1)
  for i,b in enumerate(s.points):
   t=i/(N-1);ii=t*(N-1)*min(end,1);row=min(N-2,int(ii));rr=ii-row
   a=vs[row*M+j].lerp(vs[row*M+j+1],f);z=vs[(row+1)*M+j].lerp(vs[(row+1)*M+j+1],f);v=a.lerp(z,rr)
   if end>1:v+=(vs[(N-1)*M+3]-vs[(N-2)*M+3])*(end-1)*(N-1)*t*t*t
   v.y-=lift*sin(pi*t);v.x+=jitter*sin(pi*t*1.5)
   b.co=(*v,1);b.radius=max(.045,(.78+.22*sin(pi*t))*(1-t**2.6))
 ob.hide_render=True;ob.hide_viewport=True
for ob in list(c.objects):
 if ob.type=='CURVE' and ob.name.startswith(('Fio sobre mecha','Fio de brilho franja')):ob.hide_render=True;ob.hide_viewport=True

# Menos distancia entre pele e fundo: contraste suave para conferência de materiais.
sc.camera=bpy.data.objects['Camera rosto'];sc.render.resolution_x=1100;sc.render.resolution_y=1100;sc.cycles.samples=64
sc.render.filepath=str(P/'talyssa-rosto-final.png')
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-v2-amostra.blend'))
bpy.ops.render.render(write_still=True)
sc.camera=bpy.data.objects['Camera corpo'];sc.render.resolution_x=1050;sc.render.resolution_y=1400
sc.render.filepath=str(P/'talyssa-corpo-final.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-v2-amostra.blend'))
print('AMOSTRA_FINAL_RENDERIZADA',flush=True)
