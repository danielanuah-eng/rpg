"""Verificacao do arquivo reaberto e dos renders; nao aprova qualidade artistica."""
import bpy,json,hashlib,math,struct
from pathlib import Path
P=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(P/'Talyssa-rosto-cabelo-v3.blend'))
collection=bpy.data.collections['TALYSSA — modelo editavel'];fail=[];vertices=0;curvepoints=0
for o in collection.objects:
 if o.hide_render:continue
 if o.type=='MESH':
  vertices+=len(o.data.vertices)
  if not all(math.isfinite(a) for v in o.data.vertices for a in v.co):fail.append(o.name+' coordenadas invalidas')
 if o.type=='CURVE':
  for s in o.data.splines:
   curvepoints+=len(s.points)+len(s.bezier_points)
   if not all(math.isfinite(a) for p in s.points for a in p.co):fail.append(o.name+' curva invalida')
   if not all(math.isfinite(a) for p in s.bezier_points for a in p.co):fail.append(o.name+' bezier invalida')
pres=json.loads((P/'preservacao.json').read_text(encoding='utf8'))
if not pres['corpoRoupaPreservados'] or not pres['camerasLuzesPreservadas']:fail.append('Mudanca fora do escopo')
source=Path(pres['fonte'])
if hashlib.sha256(source.read_bytes()).hexdigest()!=pres['sha256Fonte']:fail.append('Fonte V2 mudou depois da revisao')
for i in bpy.data.images:
 if i.source=='FILE' and not i.packed_file:fail.append('Imagem externa: '+i.name)
artifacts={}
for name in ['Talyssa-rosto-cabelo-v3.blend','rosto-v3.png','corpo-v3.png']:
 p=P/name
 if not p.exists():fail.append('Ausente: '+name);continue
 b=p.read_bytes();record={'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()}
 if p.suffix=='.png':
  if b[:8]!=b'\x89PNG\r\n\x1a\n':fail.append('PNG invalido: '+name)
  record['dimensoes']=list(struct.unpack('>II',b[16:24]))
 artifacts[name]=record
r={'estado':'verificacao_estrutural_ok' if not fail else 'falha','falhas':fail,'arquivoReaberto':str(P/'Talyssa-rosto-cabelo-v3.blend'),'verticesFonteVisiveis':vertices,'pontosCurvasVisiveis':curvepoints,'preservacao':pres,'arquivos':artifacts,'revisaoArtistica':'pendente_autor','rig':'pendente','animacoes':[],'observacoes':['Render de geometria real; sem imagem conceitual aplicada ao modelo.','Corpo, roupas, equipamento e matrizes de camera/luz iguais aos da V2.','Amostra de alta resolucao para direcao visual; nao esta otimizada para jogo.']}
(P/'verificacao.json').write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(r,ensure_ascii=False),flush=True)
assert not fail,fail
