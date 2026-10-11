"""Consolida arquivos reais e compara topologia/ocupação das sete composições."""
import json
import hashlib
from itertools import combinations
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
ASSETS=ROOT.parent/'v02'
themes=['pedreira','bosque','deserto','neve','vulcao']
kits=[]
checks=[]
layouts=[]
total=0
max_prop=0
max_surround=0
for theme in themes:
    manifest=json.loads((ROOT/'manifestos'/(theme+'.json')).read_text(encoding='utf-8'))
    report=json.loads((ROOT/'manifestos'/(theme+'-validacao.json')).read_text(encoding='utf-8'))
    assert report['status']=='aprovado_tecnicamente' and not report['falhas'],theme
    assert report['pecas']==len(manifest['pecas'])==report['fbxReimportados']==report['glbConferidos'],theme
    assert len(report['previewsConferidas'])==(3 if theme=='bosque' else 1),theme
    for check in report['previewsConferidas']:
        assert check['lagoSeparadoDaLojaMetros']>.35 and all(check['propsPrincipaisPorSetor']),check
        assert check['raioLivreCentroMetros']==2,check
    for entry in manifest['pecas']:
        for kind in ['arquivo','glb']:
            path=ROOT/entry[kind]
            assert hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'][kind],str(path)
        if entry['peca'].startswith('entorno'): max_surround=max(max_surround,entry['tris'])
        else: max_prop=max(max_prop,entry['tris'])
    versions=manifest['versoesBosque'] or [{'fonte':manifest['fonte'],'preview':manifest['previews'][0]}]
    for version in versions:
        assert (ROOT/version['fonte']).is_file() and (ROOT/version['preview']).is_file(),version
        stem=Path(version['fonte']).stem
        path=ROOT/'manifestos'/(stem+'-distribuicao.json')
        layout=json.loads(path.read_text(encoding='utf-8'))
        assert all(layout['propsPrincipaisPorSetor']) and layout['loja']['sempreCentral'],path
        layouts.append((stem,layout))
        version['distribuicao']='manifestos/'+path.name
    total+=len(manifest['pecas'])
    kits.append({'tema':theme,'nome':manifest['cenario'],'pecasReutilizadas':len(manifest['pecas'])-3,'entornosLimpos':3,
                 'manifesto':'manifestos/'+theme+'.json','validacao':'manifestos/'+theme+'-validacao.json',
                 'composicoes':versions,'origemTexturas':manifest['texturas']})
    checks.extend(dict(tema=theme,**c) for c in report['previewsConferidas'])
differences=[]
for (a,one),(b,two) in combinations(layouts,2):
    height_diff=sum(one['alturaExemplo12x12'][x][y]!=two['alturaExemplo12x12'][x][y] for x in range(12) for y in range(12))
    occupied_one={tuple(p['casaExemplo']) for p in one['colocacoes'] if p['principal']}
    occupied_two={tuple(p['casaExemplo']) for p in two['colocacoes'] if p['principal']}
    prop_diff=len(occupied_one^occupied_two)
    assert height_diff>0 and prop_diff>0,(a,b,'Layout repetido')
    differences.append({'composicoes':[a,b],'casasComAlturaDiferente':height_diff,'casasComOcupacaoPrincipalDiferente':prop_diff})
alpha=json.loads((ASSETS/'fontes'/'texturas-auditoria.json').read_text(encoding='utf-8'))
assert len(alpha)==40 and all(c['alphaMin']==c['alphaMax']==1 and c['originalConfirmado'] for c in alpha)
for path in ['previews/bosque-tres-versoes.png','fontes/bosque-tres-versoes.blend',
             'previews/quatro-cenarios.png','fontes/quatro-cenarios.blend']:
    assert (ROOT/path).is_file(),path
manifest={'versao':3,'data':'2026-10-10','tarefa':'011','estado':'entregue_para_avaliacao',
          'direcaoAutor':['Dividir os props em grupos menores e setores distintos','Design e relevo diferentes entre os mapas',
                          'Loja central preservada','Moedas no chão','Manter modelos próprios de cada bioma'],
          'integracaoUnity':'pendente_do_Claude','baseModelos':'../v02/','kits':kits,'modelosPorFormato':total,
          'modelosReutilizadosPorFormato':total-15,'entornosLimposPorFormato':15,
          'formatos':['FBX','GLB'],'fontesComposicao':len(checks),'comparacaoArvores':'../v02/previews/arvores-cinco-temas.png',
          'comparacaoVersoes':'previews/bosque-tres-versoes.png','quatroCenariosPedidos':'previews/quatro-cenarios.png',
          'revisaoVisual':{'registro':'fontes/revisao-visual-deepseek.log','reconferencia':'fontes/revisao-visual-final-deepseek.log',
                          'ajustesAposRevisao':['Removidas cercas e decorações dos entornos de todos os temas',
                                               'Neve reduzida a três abetos em pontos mais separados; abeto inferior afastado da parede em clareira plana',
                                               'Todas as composições passam a ter nove moedas no piso'],
                          'limiteReconferencia':'Reconferência visual antecede o último deslocamento do abeto inferior; folga final conferida geometricamente e pelo Codex',
                          'avaliacaoAutor':'pendente'},
          'instrucoes':'LEIA-ME-CLAUDE.md','validacao':'validacao.json'}
result={'status':'aprovado_tecnicamente','fbxReimportados':total,'glbConferidos':total,
        'texturas64x64OriginaisOpacasReutilizadas':len(alpha),'fontesComposicaoReabertas':len(checks),
        'maxTrisProp':max_prop,'maxTrisEntorno':max_surround,'composicoes':checks,'diferencasEntreLayouts':differences,
        'checks':'FBX/GLB, hashes, escala/eixos, pivôs, UV, vento, sockets, entornos; apoio e margem de todos os props; loja central, moedas deitadas, quatro setores, centro livre de props grandes e layouts distintos',
        'jogoUnityTestado':False,'avaliacaoVisualAutor':'pendente','falhas':[]}
for filename,data in [('manifesto.json',manifest),('validacao.json',result)]:
    (ROOT/filename).write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8',newline='\n')
print('CONSOLIDACAO_OK',total,'FBX +',total,'GLB;',len(checks),'composições;',len(differences),'pares diferentes',flush=True)
