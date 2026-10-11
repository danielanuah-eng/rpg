"""Consolida os resultados reais; falha se faltar qualquer composição ou validação."""
import json
import hashlib
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
themes=['pedreira','bosque','deserto','neve','vulcao']
kits=[]
checks=[]
total=0
max_prop=0
max_surround=0
for theme in themes:
    manifest=json.loads((ROOT/'manifestos'/(theme+'.json')).read_text(encoding='utf-8'))
    report=json.loads((ROOT/'manifestos'/(theme+'-validacao.json')).read_text(encoding='utf-8'))
    assert report['status']=='aprovado_tecnicamente' and not report['falhas'],theme
    assert report['pecas']==len(manifest['pecas'])==report['fbxReimportados']==report['glbConferidos'],theme
    assert len(report['previewsConferidas'])==(3 if theme=='bosque' else 1),theme
    assert all(c['lagoSeparadoDaLojaMetros']>.35 for c in report['previewsConferidas']),theme
    for entry in manifest['pecas']:
        for kind in ['arquivo','glb']:
            path=ROOT/entry[kind]
            assert hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'][kind],str(path)
        if entry['peca'].startswith('entorno'): max_surround=max(max_surround,entry['tris'])
        else: max_prop=max(max_prop,entry['tris'])
    versions=manifest['versoesBosque'] or [{'fonte':manifest['fonte'],'preview':manifest['previews'][0]}]
    for version in versions:
        assert (ROOT/version['fonte']).is_file() and (ROOT/version['preview']).is_file(),version
    total+=len(manifest['pecas'])
    kits.append({'tema':theme,'nome':manifest['cenario'],'pecas':len(manifest['pecas']),
                 'manifesto':'manifestos/'+theme+'.json','validacao':'manifestos/'+theme+'-validacao.json',
                 'composicoes':versions,'origemTexturas':manifest['texturas']})
    checks.extend(dict(tema=theme,**c) for c in report['previewsConferidas'])
alpha=json.loads((ROOT/'fontes'/'texturas-auditoria.json').read_text(encoding='utf-8'))
assert len(alpha)==40 and all(c['alphaMin']==c['alphaMax']==1 and c['originalConfirmado'] for c in alpha)
for path in ['previews/arvores-cinco-temas.png','fontes/arvores-cinco-temas.blend',
             'previews/bosque-tres-versoes.png','fontes/bosque-tres-versoes.blend',
             'previews/quatro-cenarios.png','fontes/quatro-cenarios.blend']:
    assert (ROOT/path).is_file(),path
manifest={'versao':2,'data':'2026-10-10','tarefa':'011','estado':'entregue_para_avaliacao',
          'direcaoAutor':['Loja no meio em qualquer mapa','Props distribuídos também pelo interior',
                          'Árvores frondosas ou pinheiros e modelos próprios para cada mapa',
                          'Diferentes versões do mapa','Moedas deitadas no chão'],
          'integracaoUnity':'pendente_do_Claude','kits':kits,'totalPecasPorFormato':total,
          'formatos':['FBX','GLB'],'fontesComposicao':len(checks),'comparacaoArvores':'previews/arvores-cinco-temas.png',
          'comparacaoVersoes':'previews/bosque-tres-versoes.png',
          'quatroCenariosPedidos':'previews/quatro-cenarios.png',
          'revisaoVisual':{'primeiraRevisao':'DeepSeek via OpenRouter conferiu as sete imagens',
                          'registro':'fontes/revisao-visual-deepseek.log',
                          'ajustesPosteriores':['Abeto da Neve remodelado com ramos abertos para distinguir da Pedreira',
                                                'Lago da terceira composição deslocado para melhor leitura; separação da loja medida'],
                          'avaliacaoAutor':'pendente'},
          'instrucoes':'LEIA-ME-CLAUDE.md','validacao':'validacao.json'}
result={'status':'aprovado_tecnicamente','fbxReimportados':total,'glbConferidos':total,
        'texturas64x64OriginaisOpacas':len(alpha),'fontesComposicaoReabertas':len(checks),
        'maxTrisProp':max_prop,'maxTrisEntorno':max_surround,'composicoes':checks,
        'checks':'Escala, eixos, pivôs, UV, tris, hashes, vento, sockets, nearest, vão dos entornos, loja central, margem e apoio das bases, moedas deitadas',
        'jogoUnityTestado':False,'avaliacaoVisualAutor':'pendente','falhas':[]}
for filename,data in [('manifesto.json',manifest),('validacao.json',result)]:
    (ROOT/filename).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
print('CONSOLIDACAO_OK',total,'FBX +',total,'GLB;',len(checks),'composições;',len(alpha),'texturas',flush=True)
