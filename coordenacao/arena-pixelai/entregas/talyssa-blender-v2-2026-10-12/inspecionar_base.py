import json
from pathlib import Path
P=Path(__file__).resolve().parent/'base-cc0'
vs=[];fs=[];groups={};g=''
for l in (P/'base.obj').read_text().splitlines():
 a=l.split()
 if not a:continue
 if a[0]=='v':vs.append(list(map(float,a[1:4])))
 elif a[0]=='g':g=a[1];groups.setdefault(g,[])
 elif a[0]=='f':
  f=[int(x.split('/')[0])-1 for x in a[1:]];fs.append((g,f));groups[g].extend(f)
for name,w in [('caucasian-female-young.target',.5),('african-female-young.target',.3),('asian-female-young.target',.2)]:
 for l in (P/name).read_text().splitlines():
  a=l.split()
  if len(a)==4 and a[0].isdigit():
   idx=int(a[0]);vs[idx]=[v+w*float(d) for v,d in zip(vs[idx],a[1:])]
for name in ['body','joint-head','joint-head-2','joint-neck','joint-jaw','joint-mouth','joint-l-eye','joint-r-eye','helper-l-eye','helper-r-eye']:
 ids=set(groups[name]);v=[vs[i] for i in ids]
 print(name,'n',len(ids),'center',[round(sum(p[k] for p in v)/len(v),4) for k in range(3)],'bounds',[[round(min(p[k] for p in v),4),round(max(p[k] for p in v),4)] for k in range(3)])
(P/'parsed.json').write_text(json.dumps({'vertices':vs,'faces':fs,'groups':groups}))
