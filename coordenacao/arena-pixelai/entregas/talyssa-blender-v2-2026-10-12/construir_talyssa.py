"""Amostra de modelagem real: malhas, superfícies de roupa e mechas editáveis.
Executar no Blender 5.2: blender -b --python construir_talyssa.py
Nenhuma imagem conceitual é aplicada ao modelo ou ao render.
"""
import bpy, math, random, json, sys
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi, exp, sqrt

OUT=Path(__file__).resolve().parent
random.seed(41)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.unit_settings.system='METRIC'
model=bpy.data.collections.new('TALYSSA — modelo editavel');scene.collection.children.link(model)
studio=bpy.data.collections.new('ESTUDIO — somente apresentacao');scene.collection.children.link(studio)

def move(o,c=model):
    for col in list(o.users_collection):col.objects.unlink(o)
    c.objects.link(o);return o

def material(name,c,rough=.6,metal=0,grain=0,scale=150):
    m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True
    nt=m.node_tree;p=nt.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=rough
    p.inputs['Metallic'].default_value=metal
    if grain:
        noise=nt.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=scale
        noise.inputs['Detail'].default_value=2
        bump=nt.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.24
        bump.inputs['Distance'].default_value=grain
        nt.links.new(noise.outputs['Fac'],bump.inputs['Height']);nt.links.new(bump.outputs['Normal'],p.inputs['Normal'])
    return m

skin=material('Pele — castanho quente',(.34,.165,.090),.5,grain=.00022,scale=320)
skin.node_tree.nodes.get('Principled BSDF').inputs['Subsurface Weight'].default_value=.055
lip=material('Labios — terracota discreta',(.27,.102,.060),.47)
crease=material('Sombra de boca e narinas',(.105,.040,.025),.8)
white=material('Esclera marfim',(.62,.58,.49),.31)
iris=material('Iris castanha',(.14,.074,.025),.3)
pupil=material('Pupilas e cilios',(.009,.006,.004),.45)
hair=material('Cabelo — prata marfim',(.67,.65,.60),.36)
hairshade=material('Cabelo — sombra de mechas',(.35,.34,.32),.45)
hairlight=material('Cabelo — fios claros',(.83,.80,.72),.39)
green=material('Capa — linho verde floresta',(.025,.077,.043),.9,grain=.00065,scale=240)
greenedge=material('Capa — costura oliva',(.105,.15,.074),.87)
ivory=material('Tunica — linho cru',(.48,.415,.302),.88,grain=.00045,scale=260)
dark=material('Calca — sarja carvao',(.033,.028,.025),.85,grain=.0005,scale=200)
leather=material('Couro castanho',(.107,.047,.023),.48,grain=.0006,scale=160)
leather2=material('Couro bordas e relevos',(.193,.094,.044),.55,grain=.0003,scale=160)
thread=material('Linha de costura encerada',(.40,.28,.15),.82)
brass=material('Fivelas de latao envelhecido',(.35,.22,.072),.32,.72)
steel=material('Guarda — ferro escurecido',(.066,.063,.056),.34,.7)
rose=material('Lamina — cristal magenta',(.56,.012,.15),.24,.2)
rp=rose.node_tree.nodes.get('Principled BSDF');rp.inputs['Emission Color'].default_value=(.8,.008,.18,1);rp.inputs['Emission Strength'].default_value=.65
roseedge=material('Aresta magenta luminosa',(.9,.035,.3),.2,.15)
rp=roseedge.node_tree.nodes.get('Principled BSDF');rp.inputs['Emission Color'].default_value=(1,.015,.24,1);rp.inputs['Emission Strength'].default_value=2.5

def mesh(name,vs,fs,mat,sub=0,solid=0,smooth=True):
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update()
    o=bpy.data.objects.new(name,me);model.objects.link(o);me.materials.append(mat)
    for p in me.polygons:p.use_smooth=smooth
    if sub:
        mod=o.modifiers.new('Superficie suave editavel','SUBSURF');mod.levels=sub;mod.render_levels=sub
    if solid:
        mod=o.modifiers.new('Espessura real','SOLIDIFY');mod.thickness=solid
    return o

def path(name,pts,r,mat):
    c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=12
    c.bevel_depth=r;c.bevel_resolution=3
    s=c.splines.new('BEZIER');s.bezier_points.add(len(pts)-1)
    for b,co in zip(s.bezier_points,pts):b.co=co;b.handle_left_type='AUTO';b.handle_right_type='AUTO'
    o=bpy.data.objects.new(name,c);model.objects.link(o);c.materials.append(mat);return o

def ell(name,p,s,mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=20,radius=1,location=p)
    o=move(bpy.context.object);o.name=name;o.scale=s;o.data.materials.append(mat)
    for f in o.data.polygons:f.use_smooth=True
    return o

def cube(name,p,s,mat,bev=.004):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=move(bpy.context.object);o.name=name;o.scale=s
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
    if bev:
        m=o.modifiers.new('Bordas de couro','BEVEL');m.width=bev;m.segments=3
        m=o.modifiers.new('Normais','WEIGHTED_NORMAL')
    return o

def loft(name,rings,mat,n=48,sub=1,ripple=0):
    vs=[]
    for z,x,y,rx,ry in rings:
        for j in range(n):
            t=2*pi*j/n;d=1+ripple*cos(t*9+z*20)
            vs.append((x+rx*cos(t)*d,y+ry*sin(t)*d,z))
    fs=[tuple(reversed(range(n)))]
    for i in range(len(rings)-1):
        for j in range(n):fs.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
    fs.append(tuple((len(rings)-1)*n+j for j in range(n)))
    return mesh(name,vs,fs,mat,sub)

def tube(name,pts,radii,mat,n=12):
    vs=[]
    for i,(p,r) in enumerate(zip(pts,radii)):
        p=Vector(p);axis=(Vector(pts[min(i+1,len(pts)-1)])-Vector(pts[max(0,i-1)])).normalized()
        side=axis.cross(Vector((0,1,0))).normalized()
        if side.length<.1:side=axis.cross(Vector((1,0,0))).normalized()
        front=axis.cross(side).normalized()
        for j in range(n):vs.append(p+r*(side*cos(j*2*pi/n)+front*sin(j*2*pi/n)))
    fs=[tuple(reversed(range(n)))]
    for i in range(len(pts)-1):
        for j in range(n):fs.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
    fs.append(tuple((len(pts)-1)*n+j for j in range(n)))
    return mesh(name,vs,fs,mat,1)

def buckle(name,x,y,z,w=.036,h=.028):
    pts=[(x-w/2,y,z-h/2),(x-w/2,y,z+h/2),(x+w/2,y,z+h/2),(x+w/2,y,z-h/2),(x-w/2,y,z-h/2)]
    path(name,pts,.0026,brass);path(name+' pino',[(x,y-.003,z-h/2),(x,y-.003,z+h*.25)],.0017,brass)

def lerp_profile(z,prof):
    for a,b in zip(prof,prof[1:]):
        if a[0]<=z<=b[0]:
            t=(z-a[0])/(b[0]-a[0]);return a[1]*(1-t)+b[1]*t
    return prof[0][1] if z<prof[0][0] else prof[-1][1]

def gauss(x,z,cx,cz,sx,sz):return exp(-((x-cx)/sx)**2-((z-cz)/sz)**2)

# Corpo com cintura, quadril e pernas independentes; roupa ajustada, nao capsulas.
loft('Tunica interna',[(.91,0,0,.127,.078),(.95,0,0,.143,.085),(1.06,0,0,.109,.078),(1.17,0,0,.117,.082),(1.29,0,0,.151,.088),(1.38,0,0,.155,.064),(1.405,0,.006,.137,.052)],ivory,ripple=.015)
loft('Pescoco',[(1.36,0,.012,.046,.044),(1.40,0,.014,.045,.043),(1.47,0,.007,.038,.037),(1.51,0,0,.052,.044)],skin)
for s in [-1,1]:
    loft('Calca com dobras '+str(s),[(.44,s*.109,.004,.049,.047),(.50,s*.109,.006,.052,.051),(.55,s*.111,0,.048,.054),(.63,s*.110,.004,.062,.061),(.76,s*.095,.012,.071,.070),(.90,s*.078,.005,.078,.084),(1.00,s*.069,0,.077,.085)],dark,ripple=.027)
    # Botas com sola, biqueira, tornozelo e cano moldados, sem esferas nos pes.
    x=s*.112
    loft('Bota cano '+str(s),[(.07,x,.007,.047,.046),(.11,x,.01,.041,.043),(.20,x,.012,.039,.037),(.29,x,.014,.046,.042),(.39,x,.011,.055,.050),(.465,x,.008,.054,.049),(.48,x,.008,.055,.048)],leather,ripple=.025)
    loft('Bota pe '+str(s),[(.027,x,-.051,.057,.112),(.033,x,-.051,.060,.115),(.045,x,-.051,.060,.115),(.068,x,-.046,.059,.110),(.088,x,-.032,.052,.088),(.123,x,.005,.040,.046),(.145,x,.011,.036,.034)],leather)
    loft('Sola '+str(s),[(.007,x,-.051,.058,.112),(.013,x,-.051,.060,.114),(.026,x,-.051,.060,.114),(.031,x,-.051,.058,.112)],dark,sub=1)
    path('Costura frontal da bota '+str(s),[(x-.036,-.053,.11),(x-.031,-.034,.24),(x-.036,-.037,.38),(x-.04,-.03,.46)],.0016,leather2)
    for z in [.19,.34,.435]:
        loft('Cinta da bota',[(z-.012,x,.012,.054,.054),(z+.012,x,.012,.054,.054)],leather2,sub=0)
        buckle('Fivela bota',x+s*.015,-.047,z,.022,.020)
    # Joelho em couro, aba angular.
    verts=[(x-.047,-.052,.43),(x-.057,-.060,.51),(x-.033,-.066,.557),(x+.035,-.066,.557),(x+.056,-.060,.51),(x+.047,-.052,.43),(x,-.076,.50)]
    mesh('Joelheira articulada',verts,[(i,(i+1)%6,6) for i in range(6)],leather,1,.006)
    # Braços com transicao anatomica, mangas e bracadeiras separadas.
    tube('Braco anatomico',[(s*.137,.008,1.376),(s*.184,.008,1.366),(s*.204,.002,1.302),(s*.220,-.009,1.218),(s*.226,-.009,1.17)], [.037,.046,.040,.032,.030],skin,20)
    tube('Manga curta',[(s*.142,.008,1.375),(s*.175,.008,1.36),(s*.19,.005,1.33)], [.046,.051,.047],dark,24)
    tube('Antebraco',[(s*.226,-.009,1.188),(s*.236,-.018,1.125),(s*.256,-.032,1.035),(s*.27,-.048,.965)], [.029,.035,.030,.020],skin,20)
    tube('Bracadeira de couro',[(s*.232,-.017,1.145),(s*.24,-.021,1.115),(s*.254,-.032,1.045),(s*.266,-.045,.992)], [.037,.038,.031,.025],leather,24)
    path('Relevo da bracadeira',[(s*.238,-.052,1.135),(s*.249,-.063,1.08),(s*.263,-.066,1.003)],.0025,leather2)
    for z,xx,yy in [(1.125,.237,-.02),(1.025,.258,-.038)]:
        tube('Cinta bracadeira',[(s*xx,yy,z-.009),(s*(xx-.004),yy+.001,z+.009)], [.039 if z>1.1 else .03]*2,leather2,20)
        buckle('Fivela braco',s*xx,yy-.04,z,.019,.016)
    # Luva com dedos individualizados e polegar em oposicao.
    ell('Palma luva',(s*.274,-.052,.936),(.027,.019,.040),leather)
    for k in range(4):
        x=s*(.256+k*.012)
        tube('Dedo luva',[(x,-.057,.928),(x+s*.003,-.069,.906),(x+s*.004,-.055,.889+abs(1.5-k)*.003)], [.007,.0067,.005],leather,10)
    tube('Polegar',[(s*.248,-.050,.950),(s*.243,-.077,.933),(s*.26,-.084,.915)], [.010,.009,.006],leather,12)

# Corselete de couro: malha ajustada com paineis e costuras.
loft('Corselete estruturado',[(1.045,0,-.004,.112,.080),(1.06,0,-.004,.114,.082),(1.12,0,-.003,.112,.083),(1.20,0,-.003,.128,.088),(1.24,0,-.004,.137,.088)],leather,sub=1)
for x in [-.080,-.041,.041,.080]:
    pts=[(x*.94,-.073,1.052),(x,-.080,1.105),(x*1.04,-.084,1.17),(x*1.16,-.078,1.231)]
    path('Nervura costurada corselete',pts,.0018,leather2)
    for k in range(12):
        z=1.064+k*.013;xx=x*(.94+(z-1.052)*1.2)
        path('Ponto costura',[(xx-.001,-.084,z),(xx+.001,-.084,z+.004)],.0006,thread)
for z in [1.09,1.145,1.20]:
    for x in [-.012,.012]:ell('Rebite corselete',(x,-.090,z),(.0024,.0015,.0024),brass)
    path('Cordao cruzado',[(-.012,-.092,z),(.012,-.092,z+.036)],.001,thread)

# Painéis de tunica e saia assimetrica; tecido com bordas reais.
for s in [-1,1]:
    vs=[];N=14;M=10
    for i in range(N):
        t=i/(N-1)
        for j in range(M):
            u=j/(M-1)
            x=s*(.014+u*.112+t*(.011+u*.027))
            z=1.07-t*(.28+.07*(1-u))
            y=-.091+.037*u-.015*sin(u*pi*3+t)*t
            vs.append((x,y,z))
    fs=[(i*M+j,i*M+j+1,(i+1)*M+j+1,(i+1)*M+j) for i in range(N-1) for j in range(M-1)]
    mesh('Aba de tunica creme',vs,fs,ivory,1,.002)
    path('Bainha tunica',[vs[i*M] for i in range(N)],.0013,thread)
    path('Bainha inferior tunica',vs[-M:],.0013,thread)

# Cintos circundam o corpo; diagonal do arnês com fivelas.
loft('Cinto cintura',[(1.017,0,-.001,.139,.091),(1.049,0,-.001,.139,.091)],leather2,sub=0)
buckle('Fivela cintura',.011,-.096,1.034,.045,.035)
for x in [-.10,-.075,-.05,.063,.091]:ell('Furo do cinto',(x,-.086,1.034),(.0025,.0013,.0025),dark)
pts=[(-.117,-.039,1.397),(-.103,-.073,1.329),(-.070,-.094,1.252),(-.029,-.100,1.182),(.025,-.093,1.106)]
vs=[]
for x,y,z in pts:vs.extend([(x-.014,y,z+.006),(x+.014,y,z-.006)])
mesh('Arnes diagonal',vs,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(pts)-1)],leather,2,.006)
path('Costura arnes esquerda',[(x-.010,y-.004,z+.004) for x,y,z in pts],.0009,thread)
path('Costura arnes direita',[(x+.010,y-.004,z-.004) for x,y,z in pts],.0009,thread)
buckle('Fivela peitoral',-.067,-.103,1.257,.038,.037)
for s in [-1,1]:
    o=cube('Bolsa de couro',(s*.128,-.047,1.005),(.067,.051,.10),leather,.009)
    o.rotation_euler[1]=s*.12
    cube('Aba bolsa',(s*.128,-.078,1.026),(.070,.012,.054),leather2,.006)
    cube('Fecho bolsa',(s*.128,-.087,1.012),(.012,.006,.045),leather,.002)
    ell('Botao bolsa',(s*.128,-.092,1.013),(.003,.002,.003),brass)

# Capa de superficie tecida: do ombro ate a coxa, drapeado por coordenadas.
vs=[];rows=32;cols=40
for i in range(rows):
    t=i/(rows-1)
    for j in range(cols):
        u=j/(cols-1);theta=.12*pi+u*.76*pi
        # envolvimento posterior, abertura livre na frente
        x=(.173+.11*t)*cos(theta)*1.4
        y=.04+(.07+.04*t)*sin(theta)+.021*sin(u*pi*9+t*2)*t
        z=1.414-t*(.63+.045*sin(u*9))-.027*cos(theta)**2
        vs.append((x,y,z))
fs=[(i*cols+j,i*cols+j+1,(i+1)*cols+j+1,(i+1)*cols+j) for i in range(rows-1) for j in range(cols-1)]
mesh('Capa verde drapeada',vs,fs,green,1,.003)
for ids in [[i*cols for i in range(rows)],[i*cols+cols-1 for i in range(rows)],list(range((rows-1)*cols,rows*cols))]:
    path('Bainha costurada capa',[vs[i] for i in ids],.0015,greenedge)
# Ombreira em pano dobrado e gola enrolada em camadas.
for s in [-1,1]:
    mesh('Pelerine de ombro',[(s*.028,-.035,1.454),(s*.142,-.047,1.421),(s*.217,-.018,1.355),(s*.22,.071,1.33),(s*.146,.128,1.402),(s*.038,.067,1.457)],[(0,1,2,3,4,5)],green,2,.004)
for k in range(3):
    pts=[]
    for j in range(33):
        a=j*2*pi/32
        pts.append((.070*cos(a),.004+.059*sin(a),1.43+k*.020+.012*cos(a+.6+k*.7)))
    path('Dobra cachecol '+str(k),pts,.020 if k==0 else .016,green)
ell('Broche capa',(-.126,-.057,1.398),(.014,.005,.011),brass)
path('Incisao broche',[(-.132,-.062,1.397),(-.126,-.063,1.404),(-.12,-.062,1.397)],.0015,steel)

# Face: superficie continua esculpida por perfis e volumes, com olhos discretos.
width=[(1.475,.013),(1.485,.035),(1.505,.056),(1.53,.074),(1.565,.088),(1.602,.098),(1.635,.097),(1.676,.094),(1.71,.079),(1.742,.049),(1.757,.002)]
depth=[(1.475,.036),(1.49,.055),(1.52,.068),(1.56,.083),(1.60,.087),(1.63,.080),(1.67,.084),(1.71,.076),(1.74,.041),(1.757,.001)]
def fy(x,z):
    w=max(.005,lerp_profile(z,width));front=lerp_profile(z,depth)*sqrt(max(.02,1-(x/w)**2))
    d=.028*gauss(x,z,0,1.583,.014,.013)+.014*gauss(x,z,0,1.615,.012,.037)
    d+=.010*gauss(x,z,0,1.543,.032,.015)
    d+=.007*(gauss(x,z,.057,1.596,.030,.020)+gauss(x,z,-.057,1.596,.030,.020))
    d+=.007*(gauss(x,z,.040,1.65,.030,.010)+gauss(x,z,-.040,1.65,.030,.010))
    d-=.006*(gauss(x,z,.042,1.629,.027,.012)+gauss(x,z,-.042,1.629,.027,.012))
    return -front-d
vs=[];nz=135;na=160
for iz in range(nz):
    z=1.475+(1.757-1.475)*iz/(nz-1);w=lerp_profile(z,width);d=lerp_profile(z,depth)
    for j in range(na):
        a=2*pi*j/na;x=w*cos(a);y=d*sin(a)
        if sin(a)<0:y=fy(x,z)
        else:y*=1.12
        vs.append((x,y,z))
fs=[]
for iz in range(nz-1):
    for j in range(na):
        ids=(iz*na+j,iz*na+(j+1)%na,(iz+1)*na+(j+1)%na,(iz+1)*na+j)
        x=sum(vs[k][0] for k in ids)/4;z=sum(vs[k][2] for k in ids)/4;y=sum(vs[k][1] for k in ids)/4
        # abertura em amendoa no tecido facial, contornada pelas palpebras.
        q=(abs(x)-.043)/.026
        eye=abs(q)<1 and abs(z-(1.630+.002*q))<.010*max(0,1-q*q)**.65
        if not (y<0 and eye):fs.append(ids)
mesh('Rosto continuo com anatomia',vs,fs,skin,0)
for s in [-1,1]:
    ell('Orelha',(s*.095,.002,1.59),(.017,.013,.032),skin)
    path('Helice da orelha',[(s*.102,-.005,1.616),(s*.11,-.009,1.60),(s*.103,-.012,1.574)],.0035,skin)
    ell('Cavidade auricular',(s*.102,-.011,1.59),(.006,.003,.014),lip)
    # Esclera em superficie lenticular limitada por duas palpebras.
    ev=[];nx=40;ny=12
    for i in range(nx):
        q=-1+2*i/(nx-1);x=s*(.043+q*.027)
        for j in range(ny):
            v=-1+2*j/(ny-1);z=1.630+.002*q+v*.0108*max(0,1-q*q)**.65
            ev.append((x,fy(x,z)-.0023-.0014*(1-v*v)*(1-q*q),z))
    mesh('Olho encaixado',ev,[(i*ny+j,(i+1)*ny+j,(i+1)*ny+j+1,i*ny+j+1) for i in range(nx-1) for j in range(ny-1)],white)
    x=s*.041;z=1.630;y=fy(x,z)-.0049
    ell('Iris',(x,y,z),(.008,.0011,.0085),iris)
    ell('Pupila',(x,y-.0011,z),(.0035,.0007,.0045),pupil)
    ell('Reflexo ocular',(x-.002,y-.0018,z+.003),(.0012,.0005,.0012),white)
    for sign in [-1,1]:
        pts=[]
        for i in range(24):
            q=-1+2*i/23;x=s*(.043+q*.027);z=1.63+.002*q+sign*.0112*max(0,1-q*q)**.65
            pts.append((x,fy(x,z)-.003,z))
        path('Palpebra superior' if sign==1 else 'Palpebra inferior',pts,.0020,skin)
        if sign==1:path('Linha delicada de cilios',[(x,y-.0015,z-.001) for x,y,z in pts],.0009,pupil)
    pts=[]
    for i in range(16):
        q=i/15;x=s*(.018+.053*q);z=1.650+.004*sin(q*pi)-.007*q
        pts.append((x,fy(x,z)-.002,z))
    path('Sobrancelha definida',pts,.0030,hairshade)
    ell('Narina',(s*.0105,-.105,1.572),(.004,.0017,.0020),crease)
# Labios em dois volumes baixos, sem sorriso pintado.
for upper in [True,False]:
    vv=[];n=42;m=7
    for i in range(n):
        q=-1+2*i/(n-1);x=q*.027
        line=1.545+.0013*cos(q*pi)-.0015*exp(-(q/.21)**2)
        thickness=(.0042 if upper else .0050)*max(0,1-q*q)
        if upper:thickness*=.8+.32*exp(-((abs(q)-.36)/.22)**2)
        for j in range(m):
            t=j/(m-1);z=line+(1 if upper else -1)*thickness*t
            vv.append((x,fy(x,z)-.0025-.0025*sin(pi*t)*(1-q*q),z))
    mesh('Labio superior' if upper else 'Labio inferior',vv,[(i*m+j,(i+1)*m+j,(i+1)*m+j+1,i*m+j+1) for i in range(n-1) for j in range(m-1)],lip)
path('Separacao dos labios',[(q*.026,fy(q*.026,1.545)-.0033,1.545+.0013*cos(q*pi)-.0015*exp(-(q/.21)**2)) for q in [-1+i/20 for i in range(41)]],.00065,crease)

# Cabelo: mechas folheadas com eixo curvo e ponta afilada, sem cones.
ell('Volume raiz do cabelo',(0,.021,1.679),(.102,.088,.091),hairshade)
def bez(p0,p1,p2,p3,t):return (1-t)**3*Vector(p0)+3*(1-t)**2*t*Vector(p1)+3*(1-t)*t*t*Vector(p2)+t**3*Vector(p3)
def lock(name,ps,w,mat,normal=(0,-1,.15)):
    vv=[];N=24;M=7;cent=[];norm=Vector(normal).normalized()
    for i in range(N):
        t=i/(N-1);p=bez(*ps,t);cent.append(p)
        tangent=(bez(*ps,min(1,t+.005))-bez(*ps,max(0,t-.005))).normalized();side=tangent.cross(norm).normalized()
        width=w*(.48+.62*sin(pi*t))*(1-t**2)+.0003
        for j in range(M):
            q=-1+2*j/(M-1);v=p+side*q*width+norm*(1-q*q)*width*.24
            vv.append(v)
    mesh(name,vv,[(i*M+j,(i+1)*M+j,(i+1)*M+j+1,i*M+j+1) for i in range(N-1) for j in range(M-1)],mat,1,.0015)
    for q in [-.46,.35]:
        pts=[]
        for i in range(2,N-2):
            t=i/(N-1);p=cent[i];tangent=(cent[min(i+1,N-1)]-cent[i-1]).normalized();side=tangent.cross(norm).normalized()
            ww=w*(.48+.62*sin(pi*t))*(1-t*t)+.0003
            pts.append(p+side*q*ww+norm*((1-q*q)*ww*.24+.0006))
        path('Fio sobre mecha',pts,.00035,hairlight)
# Laterais e nuca primeiro.
for s in [-1,1]:
    for i in range(10):
        y=-.02+i*.011;z=1.746-i*.007
        lock('Mecha lateral',[(s*.04,y,z),(s*.093,y-.01,z-.005),(s*.112,y-.02,1.632),(s*(.105+.015*sin(i*2)),y-.025,1.565+(i%3)*.017)],.014,hair,normal=(s,-.4,.05))
    for i in range(7):
        x=s*(.014+i*.012)
        lock('Nuca',[(x,.055,1.732),(x*1.1,.092,1.704),(x*1.2,.1,1.621),(x*1.12,.092,1.595)],.013,hair,normal=(0,1,0))
# Franja lateral longa, cobre uma parte da testa mas preserva os olhos.
for i in range(15):
    t=i/14
    ps=[(.045-t*.035,.016-t*.016,1.772-t*.014),(.077-t*.123,-.040,1.792-t*.012),(-.034-t*.040,-.089-t*.008,1.743-t*.045),(-.078-t*.02,-.085+t*.006,1.657-t*.043)]
    lock('Franja esculpida '+str(i),ps,.013+(i%3)*.0017,hair if i%4 else hairlight)
for i in range(6):
    ps=[(.044,.015,1.766-i*.004),(.093,-.006,1.753-i*.006),(.102,-.057,1.704-i*.007),(.088,-.075,1.662-i*.007)]
    lock('Franja curta direita',ps,.012,hair,normal=(.5,-1,.1))

# Espada com lâmina facetada, guarda, cabo e fio real.
a=Vector((.274,-.055,.918));tip=Vector((.48,-.13,.505));axis=(tip-a).normalized();side=Vector((0,1,0)).cross(axis).normalized();front=axis.cross(side).normalized()
def wp(t,u=0,v=0):return a+axis*t+side*u+front*v
tube('Cabo espada',[wp(-.052),wp(.036)],[.012,.012],leather,16)
for k in range(9):path('Enrolamento cabo',[wp(-.046+k*.008,.010*cos(j*pi/8),.010*sin(j*pi/8)) for j in range(17)],.0012,leather2)
ell('Pomo',wp(-.06),(.017,.016,.016),steel)
path('Guarda espada',[wp(.039,-.053,-.004),wp(.025,0,0),wp(.039,.053,-.004)],.008,steel)
base=wp(.048);L=(tip-base).length
v=[wp(.048,-.030),wp(.048,0,-.009),wp(.048,.030),wp(.048,0,.009),wp(.048+L*.68,-.024),wp(.048+L*.68,0,-.009),wp(.048+L*.68,.024),wp(.048+L*.68,0,.009),wp(.048+L)]
mesh('Lamina de cristal facetada',v,[(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7),(4,5,8),(5,6,8),(6,7,8),(7,4,8),(0,3,2,1)],rose,smooth=False)
for ids in [[0,4,8],[2,6,8]]:path('Fio de luz da espada',[v[i] for i in ids],.0012,roseedge)
path('Veio cristalino',[wp(.066,-.015,-.007),wp(.13,.012,-.009),wp(.19,-.01,-.009),wp(.25,.013,-.009),wp(.35,0,-.005)],.00085,roseedge)

# Estudio, câmeras e metadados. Render é sempre obtido da geometria.
def aim(o,t):o.rotation_euler=(Vector(t)-o.location).to_track_quat('-Z','Y').to_euler()
floor=material('Fundo cinza quente',(.175,.16,.14),.9)
bpy.ops.mesh.primitive_plane_add(size=200);o=move(bpy.context.object,studio);o.name='Chao estudio';o.data.materials.append(floor)
scene.world=bpy.data.worlds.new('Ambiente neutro');scene.world.use_nodes=True
scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.36,.40,.46,1)
scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.3
for name,p,power,size in [('Principal',(-3,-4,5),420,3),('Preenchimento',(3,-2,3),220,2.5),('Recorte',(1,3,4),550,2)]:
    bpy.ops.object.light_add(type='AREA',location=p);o=move(bpy.context.object,studio);o.name=name;o.data.energy=power;o.data.shape='DISK';o.data.size=size;aim(o,(0,0,1))
bpy.ops.object.camera_add(location=(2.0,-6,2.9));cam=move(bpy.context.object,studio);cam.name='Camera corpo';cam.data.type='ORTHO';cam.data.ortho_scale=2.05;aim(cam,(.025,0,.91));scene.camera=cam
bpy.ops.object.camera_add(location=(.52,-3.8,1.79));close=move(bpy.context.object,studio);close.name='Camera rosto';close.data.type='ORTHO';close.data.ortho_scale=.48;aim(close,(0,-.01,1.57))
scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=True
scene.render.resolution_x=1050;scene.render.resolution_y=1400;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast'
scene.render.filter_size=1.0
scene['estado']='amostra_3d_para_avaliacao';scene['referencia']='Talyssa estudo visual V2 aprovado pelo autor'
scene['limites']='Modelo de estudo estatico. Nao e personagem animado ou integrado. Materiais procedurais Blender.'
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            area.spaces.active.region_3d.view_distance=3.3
            area.spaces.active.region_3d.view_location=(0,0,.9)
            area.spaces.active.shading.type='MATERIAL'
scene.camera=cam
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Talyssa-v2.blend'))
scene.render.filepath=str(OUT/'talyssa-corpo.png');bpy.ops.render.render(write_still=True)
scene.camera=close;scene.render.resolution_x=1200;scene.render.resolution_y=1200
scene.render.filepath=str(OUT/'talyssa-rosto.png');bpy.ops.render.render(write_still=True)
scene.camera=cam;scene.render.resolution_x=1050;scene.render.resolution_y=1400
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Talyssa-v2.blend'))
print('TALYSSA_V2_RENDERIZADA',len(model.objects),flush=True)
