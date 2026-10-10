from pathlib import Path
import json,re
root=Path(__file__).resolve().parent
provenance=json.loads((root/'proveniencia-pixellab-v2.json').read_text(encoding='utf-8'))
def job(text):
    m=re.search(r'(?:job_id|id): ([a-z0-9-]+)',text); return m.group(1) if m else None
shopfile=root/'loja-v2/loja.visual.json'; shop=json.loads(shopfile.read_text(encoding='utf-8'))
for entry in shop['items']:
    identity=Path(entry['file']).stem; entry.update(id=identity,job_id=job(provenance['items'][identity]),destination='Assets/ArenaTatica/Resources/Sprites/Loja/'+identity+'.png',status='integrado_confirmado_pelo_claude')
shop['scene']['destination']='Assets/ArenaTatica/Resources/Sprites/Cenarios/loja.png'
shop['scene']['status']='reimportar_pacote_estabilizado'
shopfile.write_text(json.dumps(shop,ensure_ascii=False,indent=2),encoding='utf-8')
fxfile=root/'eventos-cenarios/assets.visual.json'; fx=json.loads(fxfile.read_text(encoding='utf-8'))
skill_sources={'dragao-dourado':'dragao-dourado','foice-verde':'foice-verde','tiro-de-pistola':'bala-pistola','canhao-de-mao':'bala-canhao','ilusao-confusa':'ilusao-confusa','tiro-carregado':'flecha','flecha-abissal':'flecha-abissal','baforada':'baforada','raio-arcano':'raio-arcano'}
for entry in fx['projectiles']:
    identity=Path(entry['file']).stem; entry.update(id=identity,job_id=job(provenance['effects'][identity]),pivot=[.5,.5])
    skills=[skill for skill,source in skill_sources.items() if identity==source]; entry['consumer_skill_ids']=skills
    if skills: entry['destination']='Assets/ArenaTatica/Resources/Sprites/Projeteis/'+skills[0]+'.png'
    else: entry['status']='reference_only_animated_coin_replaces_static'
for entry in fx['animations']:
    entry['duration_seconds']=round(entry['frames']/entry['suggested_fps'],3)
    entry['destination']='Assets/ArenaTatica/Resources/Sprites/Efeitos/'+entry['id']+'.png'
for entry in fx['terrain']:
    identity=Path(entry['file']).stem; entry['id']=identity; entry['job_id']=job(provenance['terrain'][identity])
    theme={'pedra':'pedreira','grama':'bosque','ruinas':'deserto','neve':'neve','vulcao':'vulcao'}.get(identity)
    entry['theme']=theme
    entry['destination']='Assets/ArenaTatica/Resources/Textures/tile_'+theme+'.png' if theme else None
    entry['status']='theme_ready' if theme else 'optional_crystal_cave_not_snow'
fxfile.write_text(json.dumps(fx,ensure_ascii=False,indent=2),encoding='utf-8')
events={
 'turno':dict(visual='Foco suave no lutador ativo e transição no indicador da vez.',asset='procedural/UI',status='Claude: som confirmado; falta feedback visual'),
 'mover':dict(visual='Walk com deslocamento por distância e aceleração/freio contínuos.',asset='clipes walk',status='Claude implementou; validar pivôs registrados'),
 'trava':dict(visual='Pulso curto na borda da casa + rótulo do resultado.',asset='procedural/material',status='rótulo existente; ligar pulso'),
 'moeda':dict(visual='Moeda gira e voa da casa até o lutador, contador recebe quantidade real.',asset='efeitos/moeda.png',status='Claude integrou fonte inicial'),
 'ataque':dict(visual='Disparo da skill e impacto nos alvos/casas resolvidos, seguido de hit/dano.',asset='projeteis/<source>.png; efeitos/impacto-area.png',status='disparo integrado; adicionar efeitos/linhas finais'),
 'cura':dict(visual='Anel verde em quem foi curado e número real.',asset='efeitos/cura.png',status='novo anel pronto'),
 'armadilha':dict(visual='Burst sobre a casa, hit completo e valor do evento.',asset='efeitos/impacto-area.png',status='hit/texto existentes; ligar burst'),
 'armadilha-posta':dict(visual='Ícone surge na casa com breve pulso.',asset='../loja-v2/itens/armadilha-de-espinhos.png',status='ícone pronto'),
 'teleporte':dict(visual='Desaparecer/origem e surgir/destino com clipe teleport.',asset='clipe teleport + efeitos/invocacao.png',status='clipe existente, overlay opcional'),
 'salto':dict(visual='Trajetória em arco e clipe leap, sombra no chão.',asset='clipe leap + procedural',status='Claude implementou trajetória'),
 'forma':dict(visual='Transição de forma com glow e troca de estado no mesmo ponto do chão.',asset='clipes morph/transform + novo Berserk',status='nova forma aguarda entrega 008'),
 'esconder':dict(visual='Fade conforme visibilidade e clipe hide.',asset='clipe hide + material',status='apresentação existente'),
 'revelar':dict(visual='Retorno gradual da opacidade e pulso curto.',asset='procedural/material',status='flash existente'),
 'espelho':dict(visual='Clipe mirror e entrada de unidade ilusória.',asset='clipe mirror + efeitos/invocacao.png',status='apresentação existente; overlay opcional'),
 'guarda':dict(visual='Clipe guard + brilho curto no escudo e valor bloqueado.',asset='clipe guard + procedural',status='apresentação existente'),
 'roubo':dict(visual='Moeda gira e voa do alvo ao ladrão, quantidade do evento.',asset='efeitos/moeda.png',status='Claude implementou'),
 'invocar':dict(visual='Clipe summon e anel na casa de aparecimento.',asset='efeitos/invocacao.png',status='novo anel pronto'),
 'nocaute':dict(visual='Death completo antes de retirar unidade, atualizar equipe.',asset='clipe death',status='apresentação existente'),
 'renascer':dict(visual='Revive ou death reverso conforme clipe disponível, brilho e retorno em pé.',asset='clipe revive/death + efeitos/cura.png',status='fallback existente; não transformar hit em death'),
 'cataclisma':dict(visual='Efeito segundo evento real e rebuild; chuva usa moedas.',asset='efeitos/moeda.png + geometria/partículas',status='Claude implementou moedas/cenário'),
 'compra':dict(visual='Moedas saem e figura do produto aparece sobre o comprador.',asset='efeitos/moeda.png + ../loja-v2/itens/<id>.png',status='Claude implementou'),
 'rodada':dict(visual='Transição curta no contador de rodada.',asset='procedural/UI',status='apenas log na leitura atual; tarefa 010'),
 'dominio':dict(visual='Pulso nas casas da zona dominada na cor da equipe.',asset='procedural/material',status='apenas log na leitura atual; tarefa 010'),
 'eliminado':dict(visual='Dissolver retrato da equipe e indicador visível.',asset='procedural/UI',status='apenas log na leitura atual; tarefa 010'),
 'vitoria':dict(visual='Transição de encerramento e realce da equipe ou empate.',asset='procedural/UI',status='apenas log na leitura atual; tarefa 010'),
 'loja':dict(visual='Pulso curto nos slots que receberam produtos novos.',asset='../loja-v2/itens/<id>.png + UI',status='apenas log na leitura atual; tarefa 010')}
(root/'eventos-cenarios/vinculos-eventos.json').write_text(json.dumps(dict(scope='Apresentação sem mudança de regras; resolved targets/positions/values come from Core events.',ranged_skills=[dict(skill_id=skill,source='projeteis/'+source+'.png',reference_direction='screen-right',pivot=[.5,.5],target_rule='Usar alvos, células, origem e endpoint reais da resolução, com alcance efetivo.') for skill,source in skill_sources.items()],events=events),ensure_ascii=False,indent=2),encoding='utf-8')
print('Linked',len(shop['items']),'products,',len(skill_sources),'ranged skills,',len(events),'events and',sum(e['theme'] is not None for e in fx['terrain']),'existing themes')
