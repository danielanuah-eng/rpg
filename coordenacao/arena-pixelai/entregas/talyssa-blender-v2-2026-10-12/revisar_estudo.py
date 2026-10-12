"""Correcoes da revisao visual: maos anatomicas, cabelo curto e pele menos uniforme."""
import bpy,json,math,random
from pathlib import Path
from mathutils import Vector,Matrix
P=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(P/'Talyssa-v2-amostra.blend'))
c=bpy.data.collections['TALYSSA — modelo editavel'];sc=bpy.context.scene
d=json.loads((P/'base-cc0/parsed.json').read_text());src=[Vector(p) for p in d['vertices']]
def joint(name):
 ids=set(d['groups']['joint-l-'+name]);return sum((src[i] for i in ids),Vector())/len(ids)
wrist=joint('hand');elbow=joint('elbow')
across=(joint('finger-2-1')-joint('finger-5-1')).normalized()
forward=(joint('finger-3-1')-wrist).normalized();forward=(forward-across*forward.dot(across)).normalized()
normal=across.cross(forward).normalized()
source=Matrix((across,normal,forward)).transposed()
target=Matrix((Vector((-1,0,0)),Vector((0,-1,0)),Vector((0,0,-1)))).transposed()
rot=target@source.transposed();scale=.070
base=Vector((.283,-.047,.972))
def rest(p):return base+rot@(p-wrist)*scale
def segment_dist(p,a,b):
 t=max(0,min(1,(p-a).dot(b-a)/(b-a).length_squared));return (p-(a+t*(b-a))).length
fingers=[]
for f in range(1,6):
 chain=[rest(joint('finger-'+str(f)+'-'+str(k))) for k in range(1,5)]
 if f==1:
  posed=[chain[0],Vector((.254,-.077,.949)),Vector((.263,-.087,.932)),Vector((.277,-.079,.925))]
 else:
  xx=chain[0].x
  posed=[chain[0],Vector((xx,-.077,.934-(f-2)*.002)),Vector((xx,-.076,.915-(f-2)*.002)),Vector((xx,-.050,.916-(f-2)*.002))]
 for k in range(3):fingers.append((chain[k],chain[k+1],posed[k],posed[k+1]))
bodyfaces=[f for g,f in d['faces'] if g=='body']
direction=(wrist-elbow).normalized()
faces=[f for f in bodyfaces if all(src[i].x>3.4 and (src[i]-wrist).dot(direction)>-.15 for i in f)]
ids=sorted(set(i for f in faces for i in f));remap={n:i for i,n in enumerate(ids)}
skin=next(m for m in bpy.data.materials if m.name.startswith('Pele corpo sem atributo'))
leather=bpy.data.materials['Couro castanho']
for o in list(c.objects):
 if o.name.startswith(('Palma luva','Dedo luva','Polegar')):bpy.data.objects.remove(o,do_unlink=True)
for sign in [-1,1]:
 vs=[]
 for idx in ids:
  p=rest(src[idx]);q=p.copy()
  if sign==1:
   # Blend ponderado entre os ossos da mao em repouso e os dedos curvados.
   candidates=[(segment_dist(p,base,rest(joint('hand-2'))),p)]
   for a,b,ta,tb in fingers:
    dst=ta+(b-a).rotation_difference(tb-ta)@(p-a)
    candidates.append((segment_dist(p,a,b),dst))
   candidates=sorted(candidates,key=lambda x:x[0])[:3]
   weights=[1/(.003+dist)**5 for dist,_ in candidates]
   q=sum((v*w for (_,v),w in zip(candidates,weights)),Vector())/sum(weights)
  q.x*=sign;vs.append(q)
 me=bpy.data.meshes.new('Mao anatomica');me.from_pydata(vs,[],[[remap[i] for i in f] for f in faces]);me.update()
 o=bpy.data.objects.new('Mao anatomica '+str(sign),me);c.objects.link(o);me.materials.append(skin);me.materials.append(leather)
 for poly in me.polygons:
  poly.use_smooth=True
  if sum((src[ids[i]]-wrist).length for i in poly.vertices)/len(poly.vertices)<.74:poly.material_index=1
 mod=o.modifiers.new('Dedos e juntas naturais','SUBSURF');mod.levels=2;mod.render_levels=2
 o['fonte']='MakeHuman CC0, mesma origem da cabeca; mao adaptada e posada, sem rig'

# Corte acima da orelha nas laterais, preservando a franja diagonal.
hair=bpy.data.objects['Cabelo — fios modelados']
for s in hair.data.splines:
 for p in s.points:
  if p.co.z<1.643:p.co.z=1.643+(p.co.z-1.643)*.26

# Maior diferenca entre pele, tecido e couro; ruido em baixa intensidade.
for m in bpy.data.materials:
 if not m.use_nodes or not m.name.startswith('Pele'):continue
 nt=m.node_tree;p=nt.nodes.get('Principled BSDF')
 if not p:continue
 p.inputs['Roughness'].default_value=.49;p.inputs['Specular IOR Level'].default_value=.30
 p.inputs['Base Color'].default_value=(.255,.139,.094,1)
 # Variacao tonal procedural discreta modulando a cor ou atributo existente.
 tex=nt.nodes.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=82;tex.inputs['Detail'].default_value=3
 mix=nt.nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.22;mix.inputs[1].default_value=(.255,.139,.094,1)
 old=list(p.inputs['Base Color'].links)
 if old:
  nt.links.new(old[0].from_socket,mix.inputs[1]);nt.links.remove(old[0])
 nt.links.new(tex.outputs['Fac'],mix.inputs[2]);nt.links.new(mix.outputs[0],p.inputs['Base Color'])
 for n in nt.nodes:
  if n.type=='BUMP':n.inputs['Distance'].default_value=.0006;n.inputs['Strength'].default_value=.42
head=bpy.data.objects['Cabeca anatomica adaptada — fonte CC0 MakeHuman'];attr=head.data.color_attributes['Tons anatomicos']
for v,co in zip(head.data.vertices,attr.data):
 x,y,z=v.co;basecol=Vector((.255,.139,.094))
 redness=math.exp(-((abs(x)-.055)/.03)**2-((z-1.59)/.022)**2) if y<-.065 else 0
 basecol=basecol.lerp(Vector((.29,.115,.077)),redness*.22)
 mouth=math.exp(-((x-.016)/.03)**6-((z-1.535)/.010)**6) if y<-.097 else 0
 basecol=basecol.lerp(Vector((.25,.075,.046)),mouth*.6)
 co.color=(*basecol,1)

# Joelheiras mais planas, recortadas, e tecido com caimento em ondulacoes amplas.
for o in c.objects:
 if o.type=='MESH' and o.name.startswith('Joelheira'):
  for v in o.data.vertices:
   v.co.y=-.059+(v.co.y+.059)*.6
  for m in o.modifiers:
   if m.type=='SUBSURF':m.levels=0;m.render_levels=0
 if o.type=='MESH' and o.name.startswith('Capa verde drapeada'):
  for v in o.data.vertices:
   t=max(0,min(1,(1.414-v.co.z)/.65));v.co.y+=.020*math.sin(v.co.x*36+t)*t
 # As abas saem de baixo do cinto, sem oculta-lo.
 if o.type=='MESH' and o.name.startswith('Aba de tunica'):
  for v in o.data.vertices:
   t=max(0,min(1,(1.07-v.co.z)/.32));v.co.y+=.034*(1-t);v.co.z=.72+(v.co.z-.72)*.84
 if o.type=='CURVE' and o.name.startswith(('Bainha tunica','Bainha inferior tunica')):
  for s in o.data.splines:
   for b in s.bezier_points:
    t=max(0,min(1,(1.07-b.co.z)/.32));b.co.y-=.035;b.co.y+=.034*(1-t);b.co.z=.72+(b.co.z-.72)*.84
sc.camera=bpy.data.objects['Camera rosto'];sc.render.resolution_x=1100;sc.render.resolution_y=1100
sc.render.filepath=str(P/'talyssa-rosto-final.png')
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-v2-amostra.blend'))
bpy.ops.render.render(write_still=True)
sc.camera=bpy.data.objects['Camera corpo'];sc.render.resolution_x=1050;sc.render.resolution_y=1400
sc.render.filepath=str(P/'talyssa-corpo-final.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Talyssa-v2-amostra.blend'))
print('REVISAO_VISUAL_APLICADA',flush=True)
