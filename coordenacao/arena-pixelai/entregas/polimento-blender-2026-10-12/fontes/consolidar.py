from pathlib import Path
import json,hashlib
ROOT=Path(__file__).resolve().parents[1]
inventory=json.loads((ROOT/'personagens/inventario.json').read_text(encoding='utf-8'))
maps=json.loads((ROOT/'manifestos/mapas-validacao.json').read_text(encoding='utf-8'))
characters=json.loads((ROOT/'personagens/validacao.json').read_text(encoding='utf-8'))
assert maps['estado']==characters['estado']=='aprovado_tecnicamente'
assert len(inventory)==characters['personagensConferidos']
for entry in inventory:
    path=ROOT/'personagens'/entry['pasta']/'personagem.visual.json'
    doc=json.loads(path.read_text(encoding='utf-8'))
    doc['ligacoes']=[{'idAcao':clip['id'],'modelo':doc['modelo'],'clipeGLB':clip['id'],
      'referenciasEstadoAtual':entry['acoes'].get(clip['id'],[]),'integracao':'pendente_claude'} for clip in doc['clipes']]
    if entry['pasta']=='TchuTcha':doc['transicao']={'transform':'../TchuTchaBerserk/TchuTchaBerserk.glb','gatilho':'pendente_claude'}
    if entry['pasta']=='Edgar':doc['transicao']={'transformghost':'../EdgarFantasma/EdgarFantasma.glb','gatilho':'pendente_claude'}
    path.write_text(json.dumps(doc,ensure_ascii=False,indent=2),encoding='utf-8')
    entry['estado']='exportado_validado_para_avaliacao';entry['manifesto']=str(path.relative_to(ROOT));entry['integracao']='pendente_claude'
(ROOT/'personagens/inventario.json').write_text(json.dumps(inventory,ensure_ascii=False,indent=2),encoding='utf-8')
files=[p for p in ROOT.rglob('*') if p.is_file() and p.suffix in ['.blend','.glb','.png','.gif','.py','.json','.md'] and '__pycache__' not in p.parts and p.name!='entrega.visual.json']
manifest={'versao':1,'data':'2026-10-12','projeto':'ArenaTaticaUnity','estado':'entrega_visual_para_avaliacao',
 'integracaoUnity':'pendente_claude','avaliacaoAutor':'pendente','mapas':{'temas':5,'modelosValidados':maps['modelosConferidos'],'falhas':0},
 'personagens':{'modelosValidados':characters['personagensConferidos'],'clipesValidados':characters['clipesConferidos'],'falhas':0,
 'estilo':'interpretação 3D estilizada inicial, com rig articulado de segmentos; não é aprovação visual do autor'},
 'previewsAnimadosGLB':[str(p.relative_to(ROOT)) for p in (ROOT/'personagens/animacoes-preview').rglob('*.gif')],
 'hashes':{str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in files}}
(ROOT/'entrega.visual.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print('ENTREGA',manifest['mapas']['modelosValidados'],manifest['personagens']['modelosValidados'],manifest['personagens']['clipesValidados'])
