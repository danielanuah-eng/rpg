import bpy,importlib.util,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('polimento',ROOT/'fontes/polir_mapas.py')
p=importlib.util.module_from_spec(spec);spec.loader.exec_module(p)
(ROOT/'models/comum').mkdir(exist_ok=True)
records=[]
for theme in ['bosque','pedreira','deserto','neve','vulcao','comum']:
    extras=ROOT/'fontes/extras';extras.mkdir(exist_ok=True)
    for source in sorted((p.LIVE/theme).glob('*.glb')):
        target=ROOT/'models'/theme/source.name
        if theme!='comum' and (ROOT/'fontes'/f'{theme}.blend').exists():
            # As peças base já pertencem à fonte completa do tema.
            manifest=json.loads((ROOT/'manifestos'/f'{theme}.json').read_text(encoding='utf-8'))
            entry=next((e for e in manifest['pecas'] if e['id']==source.stem),None)
            if entry and 'ArenaTaticaUnity/' not in entry.get('origem',''): continue
        bpy.ops.wm.read_factory_settings(use_empty=True)
        bpy.ops.import_scene.gltf(filepath=str(source));p.clean_materials(theme)
        bpy.ops.object.select_all(action='SELECT')
        rec=p.export_selected(target,True)
        bpy.ops.wm.save_as_mainfile(filepath=str(extras/(theme+'-'+source.stem+'.blend')))
        rec.update(id=source.stem,tema=theme,fonte=str((extras/(theme+'-'+source.stem+'.blend')).relative_to(ROOT)))
        if theme!='comum':
            manifest['pecas']=[e for e in manifest['pecas'] if e['id']!=source.stem]+[rec]
            (ROOT/'manifestos'/f'{theme}.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
        records.append(rec)
(ROOT/'manifestos/extras.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
print('EXTRAS_OK',len(records))
