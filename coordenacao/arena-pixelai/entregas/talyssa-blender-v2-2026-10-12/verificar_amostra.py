"""Reabre o Blender entregue e mede a geometria real. Sem validar estética por contagem."""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parent
src=P/'Talyssa-v2-amostra.blend'
bpy.ops.wm.open_mainfile(filepath=str(src))
c=bpy.data.collections['TALYSSA — modelo editavel'];dep=bpy.context.evaluated_depsgraph_get()
vertices=triangles=0;low=[float('inf')]*3;high=[-float('inf')]*3;fail=[]
for o in c.objects:
 if o.type not in ('MESH','CURVE') or o.hide_render:continue
 evaluated=o.evaluated_get(dep);m=evaluated.to_mesh()
 vertices+=len(m.vertices);m.calc_loop_triangles();triangles+=len(m.loop_triangles)
 for v in m.vertices:
  p=o.matrix_world@v.co
  if not all(math.isfinite(a) for a in p):fail.append(o.name+' tem coordenada invalida')
  for k in range(3):low[k]=min(low[k],p[k]);high[k]=max(high[k],p[k])
 evaluated.to_mesh_clear()
if not vertices:fail.append('Colecao de modelo vazia')
if any(o.type=='ARMATURE' for o in c.objects):fail.append('Amostra declarada estatica possui armature inesperado')
if any(i.source=='FILE' and not i.packed_file for i in bpy.data.images):fail.append('Imagem externa nao empacotada')
artifacts={}
for name in ['Talyssa-v2-amostra.blend','talyssa-corpo-final.png','talyssa-rosto-final.png']:
 p=P/name
 if not p.exists():fail.append(name+' ausente');continue
 b=p.read_bytes();artifacts[name]={'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}
 if p.suffix=='.png' and b[:8]!=b'\x89PNG\r\n\x1a\n':fail.append(name+' invalido')
r={'estado':'verificacao_estrutural_ok' if not fail else 'falha','falhas':fail,'objetos':len(c.objects),'verticesAvaliados':vertices,'triangulosAvaliados':triangles,'limites':[low,high],'arquivos':artifacts,'observacoes':['Amostra estatica de modelagem; sem rig, clipes ou integracao.','Renderiza geometria real com materiais Blender; conceito nao usado como textura ou plano.','Contagem de geometria nao comprova qualidade artistica nem aptidao para jogo.','Retopologia e otimizacao de curvas ainda pendentes para producao.']}
(P/'verificacao.json').write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(r,ensure_ascii=False),flush=True)
assert not fail,fail
