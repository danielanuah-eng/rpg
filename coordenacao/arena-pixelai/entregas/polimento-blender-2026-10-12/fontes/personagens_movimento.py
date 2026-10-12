"""Clipes de rig exportáveis, com gestos ajustados à anatomia e ao equipamento.
Efeitos de campo/projéteis continuam vinculados às artes existentes do jogo.
"""
import bpy,math

DESCRIPTIONS={
 'idle':'Respiração e ajuste curto de postura; retorna à pose inicial.',
 'walk':'Ciclo de marcha no lugar, apoios alternados; deslocamento pertence ao motor.',
 'attack':'Preparação, golpe do equipamento/corpo e recuperação.',
 'hit':'Recuo curto, mesma direção, recuperação.',
 'death':'Perda de sustentação e queda, mantendo a orientação até a queda.',
 'summon':'Abrir mãos e erguer o gesto de invocação.',
 'spawn':'Entrada de baixo para cima e crescimento até a escala normal.',
 'zap':'Preparar disparo e desaparecer ao fim; projétil separado.',
 'leap':'Antecipação, salto com joelhos flexionados e aterrissagem.',
 'teleport':'Erguer a foice, recolher corpo e reaparecer na mesma orientação.',
 'hide':'Abaixar-se e recolher os braços.',
 'mirror':'Abrir os dois braços diante do corpo para o espelho.',
 'heal':'Elevar as mãos/cabeça em gesto de cura.',
 'ult':'Preparação e gesto amplo da habilidade máxima.',
 'ultcharge':'Abrir as asas e tensionar o arco.',
 'ultshot':'Soltar a flecha e recolher os braços.',
 'breath':'Recolher pescoço, abrir postura e projetar a cabeça para o sopro.',
 'takeoff':'Impulso, abertura de asas e subida.',
 'transform':'Preparar a troca para a forma berserk; a forma de destino é modelo separado.',
 'transformghost':'Preparar a troca para EdgarFantasma, modelo separado.',
 'ultship':'Chamar o navio com os dois braços erguidos.',
 'pistol':'Alinhar pistola, disparar e absorver recuo.',
 'cannon':'Alinhar canhão, disparar e absorver recuo pesado.',
 'hammer':'Erguer martelo, golpe descendente e recuperação.',
 'morph':'Recolher a mão e alternar o camaleão entre as três armas.',
 'idlecannon':'Respirar sustentando o canhão.',
 'idlehammer':'Respirar sustentando o martelo.',
 'tentacle':'Avançar os dois tentáculos de óleo e recolher.',
 'charge':'Abrir o canhão de peito e tensionar o tronco.',
 'drill':'Avançar o braço do gancho-broca e retornar.',
 'fireball':'Recolher a mão, preparar a bola de fogo e lançá-la.',
 'ice':'Apontar o cajado para o chão.',
 'prison':'Fechar os braços para a prisão.',
 'wish':'Abrir o cajado e concluir a conjuração de desejo.',
 'wishchannel':'Canalização sustentada, em ciclo.',
 'bark':'Fechar os braços junto ao tronco para a casca.',
 'roots':'Abaixar o corpo e estender os braços para o chão.',
 'chain':'Preparar e estender o braço da corrente.',
 'shout':'Abrir os braços, elevar o peito e gritar.',
 'throw':'Recolher o braço, arremessar e recuperar.',
 'roar':'Abaixar o tronco e erguer cabeça/peito para o rugido.',
 'stomp':'Levantar o pé dianteiro e bater no chão.',
 'steal-reach':'Alcançar com a garra para devorar a habilidade.',
 'steal-slam':'Erguer as patas e desferir golpe pesado.',
 'steal-cast':'Abrir as patas e projetar a habilidade devorada.'}

LOOPS={'idle','walk','idlecannon','idlehammer','wishchannel'}

def generate(rig,folder,actions,factor):
    rig.animation_data_create();result=[];scene=bpy.context.scene;scene.render.fps=24
    bones=rig.pose.bones
    for action_name in actions:
        action=bpy.data.actions.new(action_name);rig.animation_data.action=action;action.use_fake_user=True
        duration=2.0 if action_name in {'idle','idlecannon','idlehammer','wishchannel'} else 1.0 if action_name=='walk' else 1.5 if action_name in {'death','transform','transformghost','morph','ultship'} else .8
        end=1+round(duration*24)
        for f in sorted(set([1,end]+list(range(1,end,3)))):
            t=(f-1)/(end-1);wave=math.sin(t*math.tau);pulse=math.sin(t*math.pi)**2
            for b in bones:b.rotation_mode='XYZ';b.rotation_euler=(0,0,0);b.location=(0,0,0);b.scale=(1,1,1)
            def rot(name,x=0,y=0,z=0):
                if name in bones:bones[name].rotation_euler=(x,y,z)
            def move(name,x=0,y=0,z=0):
                if name in bones:bones[name].location=(x*factor,y*factor,z*factor)
            def scale(name,value):
                if name in bones:bones[name].scale=(value,)*3
            for w in ['pistol','cannon','hammer','chameleon']:
                if folder=='Damian':scale(w,.001)
            if folder=='Damian':
                weapon='cannon' if action_name in ['cannon','idlecannon'] else 'hammer' if action_name in ['hammer','idlehammer'] else 'pistol'
                if action_name=='morph':weapon=['chameleon','pistol','cannon','hammer','pistol'][min(4,int(t*5))]
                scale(weapon,1)
            if action_name in {'idle','idlecannon','idlehammer','wishchannel'}:
                move('spine',z=.012*wave);rot('head',x=.012*wave)
                rot('armL',x=-.05-.018*wave);rot('armR',x=-.06+.018*wave)
                if action_name=='wishchannel':rot('armR',x=-1.3+.04*wave);rot('armL',x=-.6+.04*wave)
                rot('wingL',y=-.055*wave);rot('wingR',y=.055*wave)
                rot('tail',z=.05*wave)
                if folder=='Ciantia':rot('armR',x=-1.2);rot('foreR',x=-.4);rot('handR',x=1.6)
                if folder=='Ezequiela':
                    for s in ['L','R']:rot('arm'+s,x=-.85);rot('fore'+s,x=-.35);rot('hand'+s,x=1.2)
                if folder=='EdgarFantasma':move('root',z=.10+.025*wave)
                if folder=='EdgarNavio':rot('sail',x=.045*wave)
                if folder=='Vendedor':rot('foreR',x=-.25+.035*wave);rot('handR',z=.06*wave);rot('head',x=.08)
            elif action_name=='walk':
                move('spine',z=.018*(1-math.cos(t*math.tau*2)))
                for name in bones.keys():
                    if name.startswith('leg'):
                        side=name[3:];phase=0 if side in ['L','LF','RB','L0','R1','L2','R3'] else math.pi
                        swing=math.sin(t*math.tau+phase)*.42;rot(name,x=swing)
                        rot('shin'+side,x=-max(0,-swing)*.65)
                rot('armL',x=-wave*.23);rot('armR',x=wave*.23);rot('head',z=wave*.035);rot('tail',z=wave*.12)
                rot('wingL',y=-.13*wave);rot('wingR',y=.13*wave)
            elif action_name=='hit':
                move('root',y=.07*pulse);rot('spine',x=-.16*pulse);rot('head',x=-.14*pulse)
            elif action_name=='death':
                fall=max(0,min(1,(t-.20)/.70));rot('root',x=1.35*fall);move('root',z=.10*fall)
                rot('head',x=.25*fall);rot('armL',z=.35*fall);rot('armR',z=-.35*fall)
                for name in bones.keys():
                    if name.startswith('leg'):rot(name,x=.35*fall)
            elif action_name in {'spawn','zap'}:
                v=math.sin(t*math.pi/2) if action_name=='spawn' else 1 if t<.55 else max(.001,(1-t)/.45)
                scale('root',max(.001,v));move('root',z=.09*pulse);rot('head',x=-.15*pulse)
            elif action_name in {'leap','takeoff','teleport','hide','transform','transformghost'}:
                rot('legL',x=-.25*pulse);rot('legR',x=-.25*pulse);rot('shinL',x=.5*pulse);rot('shinR',x=.5*pulse)
                if action_name=='hide':move('root',z=-.14*min(1,t*3));rot('spine',x=.30*min(1,t*3))
                else:move('root',z=(.30 if action_name in {'leap','takeoff'} else .08)*pulse)
                if action_name=='teleport':scale('root',max(.08,1-.92*pulse));rot('armR',x=-1.1*pulse)
                if action_name in {'transform','transformghost'}:rot('armL',z=.45*pulse);rot('armR',z=-.45*pulse);rot('head',x=-.10*pulse)
                if action_name=='takeoff':rot('wingL',y=-.7*pulse);rot('wingR',y=.7*pulse)
            elif action_name in {'pistol','cannon','hammer','morph','charge','drill'}:
                if action_name=='hammer':rot('armR',x=-1.45*math.sin(t*math.pi));rot('foreR',x=-.35*pulse);rot('spine',x=.18*pulse)
                elif action_name=='morph':rot('armR',x=-.8*pulse);rot('handR',y=.5*pulse)
                else:rot('armR',x=-.7*pulse);rot('foreR',x=-.5*pulse);rot('spine',x=-.12*pulse);move('root',y=.06*pulse)
                if action_name=='charge':rot('cannon',x=-.25*pulse)
                if action_name=='drill':rot('handR',y=math.sin(t*math.tau*3)*.4*pulse)
            elif action_name in {'ultcharge','ultshot'}:
                rot('armL',x=-1.1*pulse);rot('armR',x=-.6*pulse,y=.45*pulse)
                rot('foreR',x=-.5*pulse);rot('wingL',y=-.8*pulse);rot('wingR',y=.8*pulse)
            elif action_name in {'breath','roar','shout'}:
                rot('head',x=-.45*pulse);rot('spine',x=-.18*pulse);rot('armL',z=.25*pulse);rot('armR',z=-.25*pulse)
            elif action_name=='tentacle':
                rot('tentacleL',x=-.7*pulse,z=.18*pulse);rot('tentacleR',x=-.7*pulse,z=-.18*pulse);rot('spine',x=.12*pulse)
            elif action_name in {'summon','mirror','heal','ult','ultship','ice','fireball','prison','wish','bark','roots','steal-cast'}:
                high=action_name in {'summon','heal','ult','ultship','wish'}
                rot('armL',x=(-1.55 if high else -.7)*pulse,z=.2*pulse);rot('armR',x=(-1.55 if high else -.9)*pulse,z=-.2*pulse)
                rot('foreL',x=-.25*pulse);rot('foreR',x=-.25*pulse);rot('head',x=(-.12 if high else .12)*pulse)
                if action_name in {'ice','roots'}:move('root',z=-.08*pulse);rot('spine',x=.25*pulse)
                if action_name=='bark':rot('armL',y=.3*pulse);rot('armR',y=-.3*pulse)
            else:
                # Attaques spécifiques à chaque famille anatomique.
                if 'armR' in bones:
                    rot('spine',z=-.18*pulse);rot('armR',x=-1.25*pulse);rot('foreR',x=-.3*pulse)
                    if folder in ['Edgar','EdgarFantasma','BrutamontesErrante']:rot('armL',x=-.50*pulse)
                    if folder=='ReiBobett':rot('head',x=.38*pulse);move('root',y=-.10*pulse)
                    if action_name in {'throw','chain'}:rot('armR' if action_name=='throw' else 'armL',x=-1.6*pulse)
                else:
                    rot('head',x=.30*pulse);move('root',y=-.13*pulse);rot('spine',x=.08*pulse)
                    if action_name in {'stomp','steal-reach','steal-slam'}:
                        rot('legLF',x=-.60*pulse);rot('legRF',x=-.60*pulse)
                    if folder=='EdgarNavio':rot('spine',z=.09*pulse);move('root',x=.035*pulse)
                rot('tail',z=.16*pulse)
            if folder=='ArqueiroJack' and action_name not in {'ultcharge','ultshot'}:
                scale('wingL',.001);scale('wingR',.001)
            for b in bones:
                b.keyframe_insert(data_path='rotation_euler',frame=f,group=b.name)
                b.keyframe_insert(data_path='location',frame=f,group=b.name)
                b.keyframe_insert(data_path='scale',frame=f,group=b.name)
        rig['clip_'+action_name]=DESCRIPTIONS.get(action_name,action_name)
        result.append({'id':action_name,'duracaoSegundos':duration,'quadrosFonte':end,'loop':action_name in LOOPS,'descricao':DESCRIPTIONS.get(action_name,action_name)})
    rig.animation_data.action=next(a for a in bpy.data.actions if a.name=='idle') if 'idle' in actions else bpy.data.actions[0]
    scene.frame_set(1)
    return result
