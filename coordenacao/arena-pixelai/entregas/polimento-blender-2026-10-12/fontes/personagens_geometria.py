"""Modelagem autoral em Blender a partir das referências individuais da Arena.
Cada malha é editável e usa pesos rígidos no rig; não há imagens planas no modelo.
"""
import bpy,math
from mathutils import Vector

PARTS=[]; BONES={}; MATS={}
COLORS={'skin':(.65,.38,.22),'hair':(.045,.035,.03),'cloth':(.08,.10,.13),'trim':(.52,.31,.09),
 'white':(.85,.91,.90),'black':(.01,.018,.025),'eye':(.09,.23,.28),'metal':(.45,.56,.64),
 'wood':(.24,.12,.055),'gold':(.95,.54,.09),'green':(.075,.42,.10),'pink':(.75,.045,.45),
 'cyan':(.08,.65,.80),'purple':(.28,.12,.48),'red':(.60,.07,.045),'bone':(.73,.63,.40)}

def mat(key,color=None,emit=0):
    if key in MATS:return MATS[key]
    m=bpy.data.materials.new(key);m.diffuse_color=(* (color or COLORS.get(key,(.4,.4,.4))),1);m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=m.diffuse_color
    p.inputs['Roughness'].default_value=.65;p.inputs['Specular IOR Level'].default_value=.23
    if emit:p.inputs['Emission Color'].default_value=m.diffuse_color;p.inputs['Emission Strength'].default_value=emit
    MATS[key]=m;return m

def bone(name,anchor,parent='root'):
    BONES[name]=(tuple(anchor),parent)

def finish(obj,name,key,b='spine'):
    obj.name=name;obj.data.materials.append(mat(key));bpy.context.view_layer.objects.active=obj
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    obj.vertex_groups.new(name=b).add(list(range(len(obj.data.vertices))),1,'REPLACE');PARTS.append(obj);return obj

def ell(name,pos,scale,key,b='spine',sub=2):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=1,location=pos)
    o=bpy.context.object;o.scale=scale;return finish(o,name,key,b)

def box(name,pos,scale,key,b='spine',bevel=.025):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('arestas_legiveis','BEVEL');mod.width=bevel;mod.segments=2
        bpy.ops.object.modifier_apply(modifier=mod.name)
    return finish(o,name,key,b)

def rod(name,a,z,r,key,b='spine',tip=None,vertices=8):
    a=Vector(a);z=Vector(z);v=z-a
    bpy.ops.mesh.primitive_cone_add(vertices=vertices,radius1=r,radius2=r if tip is None else tip,depth=v.length,location=(a+z)/2)
    o=bpy.context.object;o.rotation_euler=v.to_track_quat('Z','Y').to_euler();return finish(o,name,key,b)

def curve(name,points,r,key,b='spine'):
    for i,(a,z) in enumerate(zip(points,points[1:])):rod(name+'_'+str(i),a,z,r,key,b,tip=max(r*.6,.008))

def sheet(name,points,key,b='spine'):
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(points,[],[tuple(range(len(points)))]);mesh.update()
    o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o)
    mod=o.modifiers.new('espessura','SOLIDIFY');mod.thickness=.018
    bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.modifier_apply(modifier=mod.name)
    o.select_set(False);return finish(o,name,key,b)

def torus(name,pos,r,minor,key,b='spine',rotation=(math.pi/2,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=minor,major_segments=12,minor_segments=6,location=pos,rotation=rotation)
    return finish(bpy.context.object,name,key,b)

def face(z=1.35,width=.28,key='skin',ears=False,dragon=False):
    head=ell('cabeca',(0,0,z),(width,.24,.30),key,'head',3)
    for p in head.data.polygons:p.use_smooth=True
    if dragon:ell('focinho',(0,-.25,z-.08),(.21,.17,.13),key,'head')
    else:ell('nariz',(0,-.245,z-.025),(.037,.055,.04),key,'head',1)
    for sign in [-1,1]:
        x=sign*width*.43
        ell('contorno_olho',(x,-.222,z+.015),(.078,.018,.050),'black','head')
        ell('olho_branco',(x,-.239,z+.015),(.066,.012,.039),'white','head')
        ell('iris',(x,-.251,z+.012),(.028,.008,.037),'eye','head')
        ell('pupila',(x,-.259,z+.009),(.014,.005,.026),'black','head')
        ell('ponto_de_luz',(x-.008,-.264,z+.030),(.009,.004,.011),'white','head',1)
        rod('sobrancelha',(x-.06,-.241,z+.108),(x+.055,-.243,z+.115),.018,'hair','head')
        if ears:
            rod('orelha',(sign*width*.88,.015,z+.01),(sign*(width+.20),-.012,z+.135),.09,key,'head',tip=0)
        else:ell('orelha',(sign*width,.005,z-.02),(.05,.065,.07),key,'head',1)
    curve('sorriso',[(-.046,-.237,z-.105),(0,-.247,z-.123),(.046,-.237,z-.105)],.008,'black','head')

def hair(style='short',key='hair',z=1.35):
    ell('cabelo_coroa',(0,.035,z+.175),(.285,.245,.18),key,'head')
    if style!='slick':
        for i in range(7):
            x=-.24+i*.08
            rod('franja',(x,-.13,z+.25),(x+.035,-.24,z+.09+(i%3)*.035),.08,key,'head',tip=.012)
    else:
        for i in range(5):
            x=-.20+i*.10
            curve('cabelo_penteado',[(x,-.18,z+.21),(x,.02,z+.31),(x,.20,z+.21)],.035,key,'head')
    if style in ['long','curly','spiky']:
        for s in [-1,1]:
            if style=='curly':
                for i in range(5):ell('cacho',(s*(.25+math.sin(i)*.03),.10,z+.07-i*.13),(.11,.12,.12),key,'head')
            else:
                rod('mecha_longa',(s*.23,.08,z+.18),(s*.26,.15,z-.44),.13,key,'head',tip=.04)
        if style=='long':ell('cabelo_costas',(0,.17,z-.15),(.25,.10,.39),key,'head')
    if style=='spiky':
        for i in range(7):
            a=i*math.tau/7
            rod('espinho_cabelo',(math.cos(a)*.2,math.sin(a)*.17,z+.23),(math.cos(a)*.37,math.sin(a)*.31,z+.34),.09,key,'head',tip=0)

def human(folder):
    profiles={
      'Ciantia':{'hair':(.95,.55,.095),'cloth':(.035,.045,.09),'trim':(.87,.53,.09),'style':'curly'},
      'Talyssa':{'hair':(.75,.83,.86),'cloth':(.11,.28,.14),'trim':(.30,.15,.065),'style':'short'},
      'Teantha':{'hair':(.73,.84,.90),'cloth':(.035,.045,.065),'trim':(.14,.20,.23),'style':'spiky'},
      'Devon':{'hair':(.015,.018,.024),'cloth':(.028,.026,.035),'trim':(.16,.095,.065),'style':'long'},
      'Diantus':{'hair':(.65,.055,.035),'cloth':(.21,.095,.065),'trim':(.78,.41,.12),'style':'spiky'},
      'Damian':{'hair':(.55,.09,.04),'cloth':(.26,.12,.055),'trim':(.13,.23,.10),'style':'short'},
      'Ezequiela':{'hair':(.09,.045,.04),'cloth':(.08,.21,.105),'trim':(.17,.30,.13),'style':'short'},
      'TchuTcha':{'hair':(.72,.18,.025),'cloth':(.16,.12,.07),'trim':(.35,.40,.45),'style':'short'},
      'TchuTchaBerserk':{'skin':(.09,.07,.10),'hair':(.025,.025,.03),'cloth':(.018,.019,.028),'trim':(.055,.06,.085),'eye':(.9,.02,.015),'style':None},
      'ArqueiroJack':{'hair':(.04,.04,.05),'cloth':(.035,.035,.055),'trim':(.115,.12,.15),'style':None},
      'Perry':{'hair':(.025,.035,.04),'cloth':(.18,.12,.075),'trim':(.26,.20,.12),'style':'spiky'},
      'Asamyh':{'hair':(.15,.07,.19),'cloth':(.04,.035,.075),'trim':(.21,.07,.25),'skin':(.73,.55,.47),'style':'short'},
      'ReiBobett':{'hair':(.33,.07,.028),'cloth':(.48,.045,.10),'trim':(.92,.72,.43),'style':'short'},
      'PrincipeKellen':{'hair':(.17,.055,.025),'cloth':(.21,.055,.37),'trim':(.92,.52,.08),'style':'slick'},
      'Baltazar':{'skin':(.17,.12,.32),'hair':(.12,.095,.19),'cloth':(.41,.51,.61),'trim':(.19,.24,.30),'style':None},
      'DomadorCorrentes':{'skin':(.26,.31,.37),'hair':(.04,.045,.065),'cloth':(.045,.055,.07),'trim':(.41,.21,.10),'style':'short'},
      'OgroPedra':{'skin':(.37,.41,.45),'hair':(.30,.33,.38),'cloth':(.23,.145,.085),'trim':(.27,.29,.30),'style':None},
      'Edgar':{'skin':(.63,.38,.24),'hair':(.68,.73,.72),'cloth':(.035,.04,.055),'trim':(.53,.035,.03),'style':'slick'},
      'EdgarFantasma':{'skin':(.15,.68,.57),'hair':(.45,.88,.79),'cloth':(.04,.27,.29),'trim':(.12,.51,.46),'eye':(.01,.17,.13),'style':'slick'},
      'Brumak':{'skin':(.055,.047,.055),'hair':(.06,.055,.06),'cloth':(.07,.17,.16),'trim':(.27,.16,.085),'style':None},
      'BrutamontesErrante':{'skin':(.22,.25,.30),'hair':(.19,.22,.25),'cloth':(.13,.13,.15),'trim':(.29,.20,.11),'style':None},
      'Nixira':{'skin':(.22,.39,.065),'hair':(.09,.15,.055),'cloth':(.24,.13,.055),'trim':(.37,.25,.10),'eye':(.48,.34,.025),'style':None},
      'GuardiaoMusgo':{'skin':(.30,.14,.055),'hair':(.19,.10,.045),'cloth':(.23,.10,.03),'trim':(.04,.32,.065),'eye':(.09,.65,.19),'style':None},
      'BaltazarAnjo':{'skin':(.73,.53,.34),'hair':(.26,.13,.055),'cloth':(.81,.87,.90),'trim':(.92,.60,.16),'style':'short'},
      'BaltazarDemonio':{'skin':(.42,.025,.06),'hair':(.21,.03,.085),'cloth':(.30,.015,.06),'trim':(.61,.055,.11),'eye':(.88,.40,.06),'style':None},
      'Vendedor':{'skin':(.52,.23,.055),'hair':(.37,.13,.035),'cloth':(.045,.12,.35),'trim':(.90,.48,.09),'style':None}}
    profile=profiles[folder]
    for k in ['skin','hair','cloth','trim','eye']:mat(k,profile.get(k,COLORS[k]))
    bulky=folder in ['TchuTcha','TchuTchaBerserk','OgroPedra','Brumak','BrutamontesErrante','GuardiaoMusgo','ReiBobett','Baltazar','DomadorCorrentes','Vendedor']
    w=.34 if bulky else .24
    bone('spine',(0,0,.65));bone('head',(0,0,1.15),'spine')
    for side,s in [('L',-1),('R',1)]:
        shoulder=(s*(w+.035),0,1.055);elbow=(s*(w+.13),0,.82);hand=(s*(w+.16),-.06,.62)
        bone('arm'+side,shoulder,'spine');bone('fore'+side,elbow,'arm'+side);bone('hand'+side,hand,'fore'+side)
        thigh=(s*.145,0,.63);knee=(s*.18,0,.34);foot=(s*.19,-.025,.08)
        bone('leg'+side,thigh);bone('shin'+side,knee,'leg'+side);bone('foot'+side,foot,'shin'+side)
        radius=.095 if bulky else .072
        rod('braco',shoulder,elbow,radius,'skin' if folder=='TchuTcha' else 'cloth','arm'+side)
        ell('cotovelo',elbow,(radius,)*3,'trim','fore'+side,1)
        rod('antebraco',elbow,hand,radius*.9,'cloth','fore'+side)
        ell('mao',hand,(.075,.065,.085),'skin','hand'+side)
        rod('coxa',thigh,knee,radius*1.16,'cloth','leg'+side)
        rod('perna',knee,foot,radius*.95,'skin' if folder=='GuardiaoMusgo' else 'trim','shin'+side)
        ell('pe',(s*.19,-.07,.075),(.10,.17,.075),'skin' if folder in ['Perry','GuardiaoMusgo'] else 'trim','foot'+side)
    ell('tronco',(0,0,.89),(w,.165,.285),'skin' if folder in ['TchuTcha','ReiBobett'] else 'cloth')
    box('cinto',(0,-.01,.665),(w*2+.04,.35,.09),'trim');box('fivela',(0,-.193,.67),(.11,.035,.09),'gold')
    face(1.36,.29 if bulky else .27,'skin',folder in ['Nixira','PrincipeKellen'],folder in ['Brumak','Baltazar'])
    if profile['style']:hair(profile['style'])
    if folder in ['Ciantia','Teantha','Devon','Ezequiela','ArqueiroJack','Asamyh','ReiBobett','PrincipeKellen','Edgar','EdgarFantasma']:
        for s in [-1,1]:sheet('casaco',[(s*w,.09,1.06),(s*(w+.04),.14,.52),(s*.08,.24,.42),(s*.07,.15,1.04)],'cloth')
        if folder in ['Edgar','EdgarFantasma','PrincipeKellen']:
            for s in [-1,1]:rod('lapela',(s*.08,-.165,1.13),(s*.15,-.19,.78),.022,'trim')
    if folder in ['Talyssa','Diantus','Perry']:
        sheet('capa',[(0,.16,1.12),(-.34,.23,.45),(.32,.23,.45)],'cloth')
        rod('bandoleira',(-.18,-.177,1.12),(.17,-.185,.67),.045,'trim')
    if folder=='Ciantia':
        for s in [-1,1]:ell('ombreira_dourada',(s*.26,0,1.08),(.12,.19,.10),'gold')
        mat('chama',(.95,.25,.015),.6);rod('chama_mao',(.4,-.07,.67),(.40,-.08,.93),.07,'chama','handR',tip=0)
    if folder=='Talyssa':sword('pink',length=.58)
    if folder=='Devon':sword('green',length=.7,width=.09)
    if folder=='Diantus':sword('red',length=.6,width=.045);mat('chama',(.95,.2,.015),.5);rod('fogo_lamina',(.44,-.1,.8),(.53,-.1,1.14),.06,'chama','handR',tip=0)
    if folder=='Teantha':
        rod('foice_cabo',(.41,-.05,.49),(.44,-.05,1.83),.025,'metal','handR')
        curve('foice_lamina',[(.44,-.05,1.83),(.15,-.05,1.91),(-.15,-.05,1.78),(-.38,-.05,1.53)],.06,'green','handR')
    if folder=='Perry':
        sword('cyan',length=.3,width=.045)
        rod('adaga_esquerda',(-.40,-.06,.65),(-.48,-.06,.96),.045,'red','handL',tip=0)
    if folder=='Damian':
        cowboy_hat()
        for name,pos in [('pistol',(.42,-.08,.66)),('cannon',(.42,-.08,.66)),('hammer',(.42,-.08,.66)),('chameleon',(.42,-.08,.66))]:bone(name,pos,'handR')
        box('pistola_cano',(.44,-.23,.71),(.09,.32,.10),'metal','pistol');box('pistola_cabo',(.44,-.10,.60),(.075,.07,.16),'wood','pistol')
        rod('canhao',(.44,-.14,.71),(.44,-.57,.71),.15,'metal','cannon');torus('boca_canhao',(.44,-.58,.71),.12,.028,'black','cannon')
        rod('martelo_cabo',(.44,-.08,.59),(.44,-.08,1.03),.03,'wood','hammer');box('martelo_cabeca',(.44,-.08,1.10),(.40,.19,.23),'metal','hammer')
        ell('camaleao',(.45,-.16,.73),(.08,.13,.07),'green','chameleon');ell('camaleao_cabeca',(.45,-.27,.76),(.06,.07,.055),'green','chameleon')
        curve('cauda_camaleao',[(.45,-.08,.73),(.52,-.01,.76),(.58,-.04,.79),(.55,-.11,.77)],.015,'green','chameleon')
    if folder=='Ezequiela':
        mat('magia',(.51,.10,.71),.3)
        for s in [-1,1]:curve('runa_mao',[(s*.43,-.03,.70),(s*.54,-.04,.82),(s*.39,-.04,.90)],.024,'magia','handL' if s<0 else 'handR')
    if folder in ['TchuTcha','TchuTchaBerserk']:
        ell('elmo',(0,.035,1.60),(.30,.24,.16),'metal' if folder=='TchuTcha' else 'cloth','head')
        for s in [-1,1]:curve('chifre_elmo',[(s*.23,0,1.60),(s*.35,.01,1.70),(s*.35,.0,1.83)],.065,'bone','head')
        if folder=='TchuTcha':rod('barba',(0,-.2,1.27),(0,-.24,.99),.15,'hair','head',tip=.04)
        else:
            box('visor',(0,-.255,1.37),(.39,.04,.14),'black','head');rod('olhos_vermelhos',(-.17,-.281,1.38),(.17,-.281,1.38),.015,'red','head')
            for s in [-1,1]:
                ell('ombreira',(s*.35,0,1.10),(.15,.19,.12),'trim')
                rod('espinho_ombreira',(s*.36,0,1.15),(s*.48,0,1.36),.05,'trim',tip=0)
        box('escudo',(-.49,-.09,.85),(.30,.13,.67),'wood' if folder=='TchuTcha' else 'cloth','handL')
        box('escudo_faixa',(-.49,-.165,.85),(.08,.035,.63),'metal','handL')
        ell('escudo_boss',(-.49,-.20,.86),(.09,.065,.09),'metal' if folder=='TchuTcha' else 'red','handL')
    if folder=='ArqueiroJack':
        ell('capuz',(0,.06,1.39),(.32,.24,.35),'cloth','head')
        curve('borda_capuz',[(-.28,-.24,1.13),(-.30,-.23,1.47),(-.19,-.20,1.65),(0,-.18,1.70),(.19,-.20,1.65),(.30,-.23,1.47),(.28,-.24,1.13)],.044,'cloth','head')
        for s in [-1,1]:
            bone('wing'+('L' if s<0 else 'R'),(s*.15,.1,1.05),'spine')
            for i in range(6):rod('pena',(s*.20,.18,1.08-i*.05),(s*(.45+i*.06),.25,.52+i*.055),.055,'black','wingL' if s<0 else 'wingR',tip=.012)
        curve('arco',[(-.44,-.07,1.17),(-.54,-.16,.98),(-.56,-.16,.69),(-.43,-.06,.48)],.025,'wood','handL')
        rod('corda_arco',(-.44,-.07,1.17),(-.43,-.06,.48),.005,'white','handL')
        rod('flecha',(.39,-.05,.70),(.39,-.58,.70),.012,'metal','handR')
    if folder=='Asamyh':
        witch_hat();rod('varinha',(.4,-.06,.62),(.4,-.36,.73),.015,'wood','handR')
        for s in [-1,1]:torus('oculos',(s*.116,-.278,1.37),.075,.012,'metal','head')
        rod('ponte_oculos',(-.041,-.279,1.37),(.041,-.279,1.37),.012,'metal','head')
    if folder=='ReiBobett':
        ell('barriga',(0,-.04,.82),(.31,.23,.32),'skin');crown()
        for s in [-1,1]:rod('gola_pelo',(s*.27,-.04,1.14),(s*.24,-.22,.62),.08,'trim')
        rod('barba',(0,-.22,1.25),(0,-.24,1.07),.16,'hair','head',tip=.08)
    if folder=='PrincipeKellen':
        rod('cajado',(.41,-.04,.52),(.41,-.04,1.09),.025,'wood','handR');ell('gema_cajado',(.41,-.04,1.10),(.065,.065,.06),'gold','handR')
        box('colete',(0,-.14,.93),(.23,.07,.28),'trim')
        for s in [-1,1]:curve('bigode',[(s*.025,-.252,1.285),(s*.095,-.25,1.295),(s*.13,-.242,1.275)],.018,'hair','head')
    if folder in ['Baltazar','Brumak']:
        for s in [-1,1]:curve('chifre',[(s*.20,.02,1.58),(s*.35,.025,1.69),(s*.33,.01,1.85)],.07,'bone','head')
        bone('tail',(0,.15,.66));curve('cauda',[(0,.15,.66),(0,.38,.51),(0,.57,.31),(.18,.62,.23)],.07,'skin','tail')
        for s in [-1,1]:ell('ombreira',(s*.34,0,1.09),(.16,.20,.12),'cloth')
        if folder=='Baltazar':
            box('peitoral',(0,-.17,.91),(.48,.10,.35),'metal');rod('martelo_cabo',(.48,-.06,.56),(.48,-.06,1.03),.038,'wood','handR');box('martelo_paladino',(.48,-.06,1.07),(.43,.25,.27),'metal','handR')
        else:
            torus('argola_nariz',(0,-.405,1.27),.055,.015,'gold','head')
            for i in range(3):box('cinto_laranja',(0,-.19,.70+i*.14),(.50,.04,.025),'trim')
    if folder=='DomadorCorrentes':
        for i in range(7):torus('elo',(-.45,-.05,.59-i*.06),.045,.013,'metal','handL',rotation=(math.pi/2,i%2*math.pi/2,0))
        for s in [-1,1]:ell('ombreira_couro',(s*.28,0,1.08),(.13,.17,.085),'trim')
    if folder=='OgroPedra':
        ell('barriga_pedra',(0,-.09,.87),(.33,.21,.30),'skin');rod('clava',(.49,-.05,.54),(.64,-.04,1.07),.10,'wood','handR',tip=.15)
        for s in [-1,1]:rod('presa',(s*.08,-.25,1.18),(s*.1,-.26,1.29),.025,'bone','head',tip=0)
    if folder=='BrutamontesErrante':
        for s in [-1,1]:
            for i in range(4):ell('placa_pedra',(s*(.27+i*.03),0,1.13-i*.15),(.14,.17,.12),'skin')
        for i in range(5):rod('crista_pedra',(-.17+i*.08,.06,1.52),(-.22+i*.11,.05,1.85-abs(2-i)*.06),.065,'skin','head',tip=0)
    if folder=='GuardiaoMusgo':
        for s in [-1,1]:
            for i in range(4):rod('veio_madeira',(s*(.12+i*.035),-.18,.75),(s*(.13+i*.035),-.17,1.13),.025,'hair')
            for i in range(3):ell('musgo_ombreira',(s*(.25+i*.035),-.02,1.16),(.13,.12,.11),'green')
            curve('raiz_pe',[(s*.18,0,.16),(s*.30,-.13,.07),(s*.39,-.17,.025)],.04,'skin','footL' if s<0 else 'footR')
        for i in range(3):rod('galho_cabeca',(-.2+i*.2,0,1.58),(-.24+i*.24,0,1.81-abs(1-i)*.05),.04,'skin','head',tip=.012)
        for s in [-1,1]:
            for i in range(4):ell('casca_braco',(s*(.34+i*.025),-.02,1.04-i*.085),(.075,.10,.085),'skin','armL' if s<0 else 'armR')
            curve('cipó_peito',[(s*.18,-.12,1.14),(s*.22,-.17,.98),(s*.12,-.17,.83)],.018,'green')
            for i in range(3):ell('musgo_cabeca',(s*(.18-i*.045),.04,1.60+i*.055),(.10,.08,.07),'green','head')
    if folder=='Nixira':
        mushroom((0,.02,1.64),.39,'head')
        rod('bastao',(-.43,0,.04),(-.43,0,1.11),.025,'wood','handL');mushroom((-.43,0,1.14),.10,'handL')
        rod('tirante',(-.18,-.16,1.12),(.17,-.18,.67),.020,'trim');rod('tirante',(.18,-.16,1.12),(-.17,-.18,.67),.020,'trim')
        box('bolsa',(0,.18,.86),(.34,.14,.28),'wood');ell('frasco',(.42,-.12,.73),(.055,.06,.095),'cyan','handR')
        for i in range(3):curve('dedo_frasco',[(.46,-.07,.68+i*.028),(.48,-.13,.68+i*.028),(.45,-.16,.69+i*.028)],.012,'skin','handR')
    if folder in ['BaltazarAnjo','BaltazarDemonio']:
        for s in [-1,1]:wing('wingL' if s<0 else 'wingR',s,(s*.15,.14,1.06),'white' if folder=='BaltazarAnjo' else 'purple',folder=='BaltazarAnjo')
        if folder=='BaltazarAnjo':torus('aureola',(0,0,1.92),.19,.015,'gold','head',rotation=(0,0,0))
        else:
            for s in [-1,1]:curve('chifre_demonio',[(s*.18,.02,1.57),(s*.22,.01,1.73),(s*.17,0,1.85)],.045,'bone','head')
            bone('tail',(0,.15,.67));curve('cauda_demonio',[(0,.15,.67),(.20,.37,.51),(.34,.39,.62)],.025,'red','tail')
            ell('orbe_fogo',(.43,-.08,.73),(.075,)*3,'gold','handR')
    if folder=='Vendedor':
        ell('barriga_castor',(0,-.05,.88),(.35,.20,.28),'cloth')
        ell('focinho_castor',(0,-.23,1.25),(.20,.13,.13),'skin','head')
        ell('nariz_castor',(0,-.35,1.29),(.07,.035,.055),'black','head')
        for s in [-1,1]:
            ell('orelha_castor',(s*.25,.03,1.57),(.085,.04,.075),'skin','head')
            box('incisivo_castor',(s*.033,-.345,1.16),(.047,.045,.085),'white','head',.005)
        ell('boina_castor',(0,.035,1.62),(.30,.25,.10),'cloth','head');torus('fita_boina',(0,.02,1.60),.26,.018,'gold','head',rotation=(0,0,0))
        bone('tail',(0,.15,.65));ell('cauda_castor',(0,.36,.37),(.23,.095,.29),'wood','tail')
        box('livro_aberto',(0,-.43,.59),(.49,.32,.055),'bone','handL')
        rod('pena_escrever',(.46,-.11,.67),(.39,-.33,.84),.010,'metal','handR');ell('pena',( .40,-.30,.80),(.035,.025,.10),'white','handR')
        for i in range(3):ell('botao_casaco',(0,-.25,.78+i*.10),(.022,.01,.022),'gold')

def sword(key,length=.58,width=.065):
    rod('punho_espada',(.43,-.06,.60),(.43,-.06,.72),.025,'wood','handR')
    rod('guarda_espada',(.33,-.06,.72),(.53,-.06,.72),.018,'metal','handR')
    rod('lamina',(.43,-.06,.74),(.55,-.07,.74+length),width,key,'handR',tip=0,vertices=4)

def cowboy_hat():
    ell('aba_chapeu',(0,.01,1.58),(.41,.33,.055),'wood','head')
    box('copa_chapeu',(0,.02,1.71),(.44,.35,.24),'wood','head',.045)
    box('fita_chapeu',(0,-.16,1.63),(.39,.035,.05),'trim','head')

def witch_hat():
    ell('aba_bruxa',(0,.015,1.59),(.38,.30,.045),'purple','head')
    rod('cone_bruxa',(0,.015,1.59),(-.10,.035,1.99),.23,'purple','head',tip=.025)
    rod('ponta_bruxa',(-.10,.035,1.96),(-.30,.015,1.97),.045,'purple','head',tip=.008)

def crown():
    torus('coroa_aro',(0,0,1.57),.25,.035,'gold','head',rotation=(0,0,0))
    for i in range(7):
        a=i*math.tau/7;rod('coroa_ponta',(math.cos(a)*.23,math.sin(a)*.23,1.58),(math.cos(a)*.27,math.sin(a)*.27,1.80),.04,'gold','head',tip=0)
        ell('coroa_gema',(math.cos(a)*.25,math.sin(a)*.25,1.63),(.035,)*3,'pink','head',1)

def mushroom(pos,r,b):
    x,y,z=pos;ell('cogumelo_borda',(x,y,z),(r,r,.055),'bone',b)
    ell('cogumelo_chapeu',(x,y,z+r*.12),(r*.97,r*.97,r*.42),'red',b)
    for i in range(5):
        a=i*math.tau/5;ell('pinta_cogumelo',(x+math.cos(a)*r*.56,y+math.sin(a)*r*.56,z+r*.42),(r*.13,r*.13,.014),'bone',b,1)

def quadruped(folder):
    cat=folder.startswith('Gato');bear=folder=='Raizor';deer=folder=='Alvyr';hamster=folder=='PapaMoedas';monster=folder=='Colosso'
    colors={'GatoLaranja':(.80,.29,.045),'GatoBranco':(.83,.88,.90),'GatoMalhado':(.025,.034,.05),'GatoManchado':(.63,.22,.05),'LoboCinzas':(.19,.14,.12),'Raizor':(.25,.09,.028),'Alvyr':(.81,.75,.55),'PapaMoedas':(.63,.17,.025),'Colosso':(.33,.12,.055)}
    mat('fur',colors[folder]);mat('belly',(.74,.64,.44) if folder!='GatoBranco' else (.84,.89,.91))
    bone('spine',(0,0,.53));bone('head',(0,-.45,.66),'spine');bone('tail',(0,.40,.58),'spine')
    ell('corpo',(0,.08,.52),(.25 if cat else .33,.49 if not hamster else .35,.26 if not deer else .24),'fur')
    hz=.70 if cat or hamster else .82 if bear or monster else 1.14 if deer else .85
    head=ell('cabeca',(0,-.48,hz),(.23 if cat else .25,.23,.225 if cat else .23),'fur','head',3 if cat or hamster else 2)
    if cat:
        for s in [-1,1]:ell('bochecha_felina',(s*.055,-.677,hz-.07),(.07,.045,.05),'belly','head')
        rod('nariz_felino',(0,-.723,hz-.05),(0,-.733,hz-.085),.029,'pink','head',tip=0,vertices=3)
        for s in [-1,1]:
            for i in range(2):rod('bigode_felino',(s*.07,-.71,hz-.09+i*.02),(s*.16,-.705,hz-.07+i*.035),.004,'white','head')
    else:
        ell('focinho',(0,-.68,hz-.07),(.15,.10 if hamster else .13,.11),'belly','head')
        ell('nariz',(0,-.745 if hamster else -.785,hz-.035),(.045 if hamster else .065,.025,.03),'black','head',1)
    for s in [-1,1]:
        ell('olho',(s*.145,-.665,hz+.045),(.042,.027,.05),'black','head')
        ell('luz_olho',(s*.14,-.689,hz+.063),(.012,.009,.013),'white','head',1)
        if hamster or bear:ell('orelha',(s*.20,-.46,hz+.20),(.10,.04,.09),'fur','head')
        else:rod('orelha',(s*.14,-.46,hz+.13),(s*.21,-.44,hz+.38),.10,'fur','head',tip=.01)
        if cat:
            mat('rosa_orelha',(.76,.30,.33));sheet('orelha_interna',[(s*.12,-.51,hz+.19),(s*.24,-.49,hz+.19),(s*.21,-.48,hz+.32)],'rosa_orelha','head')
        for front in [True,False]:
            side=('L' if s<0 else 'R')+('F' if front else 'B');y=-.32 if front else .37
            upper=(s*.20,y,.52);knee=(s*.22,y+.03,.25);foot=(s*.23,y-.05,.06)
            bone('leg'+side,upper);bone('shin'+side,knee,'leg'+side);bone('foot'+side,foot,'shin'+side)
            r=.07 if cat or deer else .12
            rod('coxa',upper,knee,r,'fur','leg'+side);rod('canela',knee,foot,r*.67,'fur','shin'+side)
            ell('pata',(s*.23,y-.09,.06),(r*1.1,.13,.06),'bone' if deer else 'fur','foot'+side)
            if bear or monster:
                for i in range(3):rod('garra',(s*.23-.06+i*.06,y-.17,.06),(s*.23-.06+i*.06,y-.23,.035),.018,'bone','foot'+side,tip=0)
    if cat:
        curve('cauda',[(0,.41,.59),(0,.66,.76),(.02,.71,1.02),(.10,.67,1.13)],.055,'fur','tail')
        if folder in ['GatoLaranja','GatoManchado']:
            key='wood'
            for i in range(6):ell('marca_pelo',((-.18 if i%2 else .18),.27-(i//2)*.17,.71),(.045,.035,.028),key)
        if folder=='GatoMalhado':ell('peito_branco',(0,-.28,.46),(.20,.15,.19),'white')
        if folder=='GatoManchado':
            for s in [-1,1]:
                for i in range(12):ell('pinta_lateral',(s*.23,-.20+(i%4)*.15,.44+(i//4)*.10),(.015,.034,.030),'black')
                for i in range(3):ell('pinta_face',(s*.18,-.57,hz-.04+i*.055),(.025,.012,.018),'black','head',1)
    elif deer:
        rod('pescoco',(0,-.31,.56),(0,-.40,1.08),.14,'fur','head')
        for s in [-1,1]:
            curve('galhada',[(s*.12,-.43,1.32),(s*.21,-.40,1.55),(s*.32,-.37,1.81),(s*.35,-.34,1.96)],.032,'gold','head')
            for i in range(3):rod('ramo_galhada',(s*(.20+i*.055),-.40+i*.02,1.50+i*.14),(s*(.38+i*.04),-.41,1.63+i*.14),.022,'gold','head',tip=.005)
    elif bear:
        for i in range(9):ell('folha_dorso',((i%3-1)*.15,(i//3)*.16,.76),(.13,.15,.10),'green')
        for s in [-1,1]:curve('raiz_lateral',[(s*.23,.28,.65),(s*.34,.14,.53),(s*.32,-.02,.36)],.045,'wood')
    elif monster:
        for s in [-1,1]:curve('chifre_gigante',[(s*.17,-.50,hz+.10),(s*.32,-.63,hz+.25),(s*.31,-.76,hz+.43),(s*.25,-.78,hz+.48)],.08,'bone','head')
        for i in range(5):ell('placa_dorso',(0,.45-i*.18,.77),(.23,.13,.11),'wood')
        ell('carapaca_colosso',(0,.16,.81),(.38,.45,.17),'wood')
        for s in [-1,1]:
            curve('presa_colosso',[(s*.17,-.68,hz-.12),(s*.32,-.74,hz+.03),(s*.35,-.80,hz+.26),(s*.29,-.83,hz+.38)],.078,'bone','head')
    elif hamster:
        ell('bochecha',(0,-.62,.65),(.23,.12,.17),'belly','head');torus('moeda_boca',(0,-.755,.67),.07,.018,'gold','head')
        ell('barriga_redonda',(0,.06,.46),(.33,.36,.35),'fur')
    else:curve('cauda_lobo',[(0,.43,.55),(0,.67,.48),(.08,.82,.39)],.07,'fur','tail')

def wing(name,s,anchor,key,feather=False):
    bone(name,anchor,'spine');x,y,z=anchor
    pts=[(x,y,z),(x+s*.78,y+.17,z+.60),(x+s*.87,y+.39,z+.05),(x+s*.28,y+.36,z-.12)]
    if not feather:sheet('membrana_asa',pts,key,name)
    for end in pts[1:]:rod('nervura_asa',pts[0],end,.027,'bone' if key=='bone' else key,name)
    if feather:
        for i in range(8):rod('pena_cristal',(x+s*.25,y+.12,z+.22),(x+s*(.43+i*.065),y+.42,z-.20+i*.095),.07,key,name,tip=.006,vertices=4)

def dragon(folder):
    spectral=folder=='DragaoEspectral';griffin=folder=='Gryvro';key='dragon'
    mat(key,(.20,.13,.64) if spectral else (.09,.63,.80) if griffin else (.68,.56,.37))
    bone('spine',(0,0,.54));bone('head',(0,-.35,.93),'spine');bone('tail',(0,.35,.50),'spine')
    if folder=='Ossyro':
        rod('coluna_ossea',(0,.43,.64),(0,-.31,.65),.045,key)
        rod('pescoco_osseo',(0,-.27,.65),(0,-.42,1.06),.047,key,'head')
        for i in range(4):ell('vertebra_pescoco',(0,-.29-i*.036,.72+i*.10),(.082,.077,.05),key,'head',1)
    else:
        ell('corpo',(0,.02,.56),(.28,.45,.31),key)
        rod('pescoco',(0,-.27,.65),(0,-.42,1.06),.16,key,'head')
    ell('cabeca',(0,-.49,1.10),(.19,.22,.19),key,'head')
    ell('focinho',(0,-.69,1.05),(.14,.16,.10),'bone' if not spectral and not griffin else key,'head')
    for s in [-1,1]:
        ell('olho',(s*.145,-.65,1.15),(.055 if folder=='Ossyro' else .035,.025,.055 if folder=='Ossyro' else .035),'cyan' if spectral else 'black','head')
        if not griffin:rod('chifre',(s*.13,-.43,1.22),(s*.21,-.34,1.48),.05,'bone' if not spectral else key,'head',tip=0)
        wing('wingL' if s<0 else 'wingR',s,(s*.19,.04,.83),key,griffin)
        for front in [True,False]:
            name=('L' if s<0 else 'R')+('F' if front else 'B');y=-.25 if front else .30
            a=(s*.23,y,.56);k=(s*.27,y,.28);f=(s*.28,y-.08,.07)
            bone('leg'+name,a);bone('shin'+name,k,'leg'+name);bone('foot'+name,f,'shin'+name)
            rod('jambe',a,k,.065,key,'leg'+name);rod('tibia',k,f,.045,key,'shin'+name)
            for i in range(3):rod('garra',(s*.28-.055+i*.055,y-.08,.07),(s*.28-.055+i*.055,y-.23,.035),.018,'bone','foot'+name,tip=0)
    curve('cauda',[(0,.37,.5),(.12,.62,.34),(.37,.72,.23),(.58,.66,.35),(.63,.53,.43)],.07,key,'tail')
    if folder=='Ossyro':
        for i in range(5):
            y=.31-i*.15
            torus('costela',(0,y,.57),.28,.020,'bone',rotation=(math.pi/2,0,0))
        for i in range(6):rod('espinho_vertebra',(0,.4-i*.16,.78),(0,.4-i*.16,.99),.035,'bone',tip=0)
    if griffin:
        rod('bico',(0,-.68,1.11),(0,-.88,1.02),.12,'cyan','head',tip=.06,vertices=4)
        rod('gancho_bico',(0,-.87,1.03),(0,-.87,.97),.065,'cyan','head',tip=0,vertices=4)
        for i in range(3):rod('pluma_cabeca',(-.10+i*.10,-.38,1.22),(-.13+i*.13,-.24,1.41),.05,'cyan','head',tip=0,vertices=4)
        ell('tufo_cauda',(.64,.53,.43),(.10,.08,.11),key,'tail')

def insect(folder):
    spider=folder=='ViuvaAgulha';mat('carapace',(.025,.035,.075) if spider else (.12,.15,.17))
    bone('spine',(0,0,.5));bone('head',(0,-.32,.52),'spine')
    ell('abdomen',(0,.2,.55),(.31,.36,.27),'carapace');ell('thorax',(0,-.18,.48),(.20,.23,.19),'carapace')
    ell('tete',(0,-.38,.52),(.17,.16,.15),'carapace','head')
    for s in [-1,1]:
        ell('oeil',(s*.095,-.50,.56),(.045,.025,.05),'red' if spider else 'gold','head')
        for i in range(4 if spider else 3):
            name=('L' if s<0 else 'R')+str(i);a=(s*.16,.24-i*.15,.49);k=(s*(.45+abs(1.5-i)*.035),.31-i*.22,.50);f=(s*.65,.40-i*.27,.025)
            bone('leg'+name,a);bone('shin'+name,k,'leg'+name)
            rod('patte',a,k,.035,'carapace','leg'+name);rod('patte_bout',k,f,.025,'carapace','shin'+name,tip=.01)
        if not spider:wing('wingL' if s<0 else 'wingR',s,(s*.1,0,.57),'white')
    if spider:sheet('sablier',[(0,-.02,.79),(-.085,.12,.80),(0,.24,.81),(.085,.34,.78),(0,.40,.75),(-.07,.29,.81),(.075,.10,.80)],'red')
    else:
        for i in range(3):torus('rayure_abdomen',(0,.17+i*.1,.55),.20-i*.04,.018,'black',rotation=(math.pi/2,0,0))
        rod('dard',(0,.48,.48),(0,.64,.31),.045,'metal',tip=0)

def ship():
    bone('spine',(0,0,.28));bone('head',(0,0,.50),'spine');bone('sail',(0,0,.50),'spine')
    mat('ghost',(.13,.61,.53),.15)
    ell('coque',(0,0,.23),(.35,.81,.20),'ghost')
    for s in [-1,1]:rod('lisse',(s*.28,-.67,.34),(s*.28,.63,.34),.035,'ghost')
    for y in [-.25,.32]:
        rod('mat',(0,y,.35),(0,y,1.39),.025,'ghost','sail')
        points=[];faces=[];cols=8;rows=5
        for j in range(rows):
            for i in range(cols):
                x=-.30+i*.60/(cols-1);z=1.25-j*.55/(rows-1)
                if j==rows-1:z+=.04 if i%3==0 else -.03 if i%3==1 else 0
                points.append((x,y-.13*(1-(x/.31)**2)*math.sin(j*math.pi/(rows-1)),z))
        for j in range(rows-1):
            for i in range(cols-1):a=j*cols+i;faces.append((a,a+1,a+cols+1,a+cols))
        data=bpy.data.meshes.new('vela_enfunada');data.from_pydata(points,[],faces);data.update()
        o=bpy.data.objects.new('vela_enfunada',data);bpy.context.collection.objects.link(o);finish(o,'vela_enfunada','cyan','sail')
        rod('vergue',(-.36,y,1.25),(.36,y,1.25),.018,'ghost','sail')
        for s in [-1,1]:rod('hauban',(s*.28,y,.40),(0,y,1.38),.005,'ghost','sail')
    rod('beaupre',(0,-.63,.34),(0,-1.02,.60),.022,'ghost')
    for s in [-1,1]:
        for i in range(3):rod('canons',(s*.22,-.35+i*.27,.30),(s*.43,-.35+i*.27,.30),.035,'ghost')

def totem():
    bone('spine',(0,0,.15));bone('head',(0,0,.72),'spine')
    box('socle',(0,0,.07),(.42,.35,.14),'wood');box('tronc',(0,0,.67),(.21,.21,1.06),'wood')
    box('rosto_talhado',(0,0,1.03),(.27,.23,.29),'wood','head',.035);mat('rune',(.03,.75,.23),.3)
    for s in [-1,1]:
        ell('olho_talhado',(s*.067,-.124,1.06),(.027,.010,.024),'green','head',1)
        rod('sobrancelha_talhada',(s*.03,-.13,1.10),(s*.103,-.13,1.105),.010,'black','head')
    rod('nariz_talhado',(0,-.12,1.065),(0,-.17,1.03),.025,'wood','head',vertices=4)
    curve('boca_talhada',[(-.05,-.13,.97),(0,-.13,.95),(.05,-.13,.97)],.006,'black','head')
    for i in range(3):torus('rune_soin',(0,-.12,.36+i*.18),.044,.010,'rune')
    for s in [-1,1]:
        rod('branche',(s*.07,0,.74),(s*.27,0,.96),.025,'wood')
        for j in range(2):ell('feuille',(s*.22,0,1.0+j*.13),(.065,.028,.035),'green')

def pirate():
    # Costume et mécanismes propres à Cheiro D.Pneu Queimado.
    human('Edgar');mat('pirate_skin',(.34,.16,.09));mat('pirate_coat',(.13,.07,.11))
    for obj in PARTS:
        material=obj.data.materials[0]
        if material.name=='skin':obj.data.materials[0]=MATS['pirate_skin']
        elif material.name=='cloth':obj.data.materials[0]=MATS['pirate_coat']
    box('chapeau_pirate',(0,.015,1.65),(.64,.38,.18),'white','head',.05)
    for i in [-1,0,1]:ell('pois_chapeau',(i*.17,-.18,1.65),(.055,.02,.04),'black','head',1)
    rod('crochet_bras',(.40,-.03,.66),(.43,-.08,.53),.028,'metal','handR')
    curve('crochet',[(.43,-.08,.53),(.55,-.1,.50),(.59,-.10,.60),(.55,-.10,.65)],.025,'metal','handR')
    bone('cannon',(0,-.13,.89),'spine')
    rod('canon_poitrine',(0,-.18,.89),(0,-.37,.89),.10,'metal','cannon');torus('bouche_canon',(0,-.38,.89),.08,.022,'black','cannon')
    for s in [-1,1]:
        bone('tentacle'+('L' if s<0 else 'R'),(s*.24,.08,.73),'spine')
        curve('tentacule_huile',[(s*.24,.08,.73),(s*.43,.19,.48),(s*.56,.20,.54),(s*.61,.13,.82)],.045,'black','tentacleL' if s<0 else 'tentacleR')

def build(folder):
    global PARTS,BONES,MATS
    PARTS=[];BONES={'root':((0,0,0),None)};MATS={}
    if folder in ['LoboCinzas','Raizor','Alvyr','PapaMoedas','Colosso'] or folder.startswith('Gato'):quadruped(folder)
    elif folder in ['DragaoEspectral','Ossyro','Gryvro']:dragon(folder)
    elif folder in ['VespaCinzenta','ViuvaAgulha']:insect(folder)
    elif folder=='EdgarNavio':ship()
    elif folder=='TotemCura':totem()
    elif folder=='CheiroPneu':pirate()
    else:human(folder)
    if folder=='EdgarFantasma':
        for m in MATS.values():
            m.node_tree.nodes.get('Principled BSDF').inputs['Alpha'].default_value=.60
            m.diffuse_color=(*m.diffuse_color[:3],.60)
            m.surface_render_method='DITHERED'
    data=bpy.data.armatures.new(folder+'_rig');rig=bpy.data.objects.new(folder+'_rig',data);bpy.context.collection.objects.link(rig)
    bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
    for name,(pos,parent) in BONES.items():
        b=data.edit_bones.new(name);b.head=pos;b.tail=Vector(pos)+Vector((0,0,.12))
        if parent:b.parent=data.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')
    for p in PARTS:p.select_set(True)
    bpy.context.view_layer.objects.active=PARTS[0];bpy.ops.object.join();mesh=bpy.context.object;mesh.name=folder+'_mesh'
    mod=mesh.modifiers.new('Rig','ARMATURE');mod.object=rig;mesh.parent=rig
    # Unités en mètres, pivot aux pieds. Tailles volontairement individuelles.
    factor={'Ciantia':.98,'Nixira':.62,'Perry':.69,'Asamyh':.86,'ReiBobett':.91,'PrincipeKellen':.88,'Baltazar':1.12,'Brumak':1.18,'OgroPedra':1.17,'BrutamontesErrante':1.20,'GuardiaoMusgo':1.13,'GatoLaranja':.62,'GatoBranco':.62,'GatoMalhado':.62,'GatoManchado':.62,'LoboCinzas':.93,'Colosso':1.5,'Raizor':1.15,'Alvyr':.88,'Ossyro':1.15,'Gryvro':1.0,'BaltazarAnjo':.32,'BaltazarDemonio':.32,'Vendedor':.82}.get(folder,1.0)
    for v in mesh.data.vertices:v.co*=factor
    bpy.context.view_layer.objects.active=rig;rig.select_set(True);mesh.select_set(False);bpy.ops.object.mode_set(mode='EDIT')
    for b in data.edit_bones:b.head*=factor;b.tail*=factor
    bpy.ops.object.mode_set(mode='OBJECT')
    rig['identidade']=folder;rig['eixos']='Blender Z-up, frente -Y; GLB Y-up, frente +Z'
    return rig,mesh,factor
