"""Consolida proveniência, inventário e hashes da entrega já conferida."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[3]

def save(path, data):
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8', newline='\n')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

reparos = json.loads((ROOT / 'reparos-pixellab-v2.json').read_text(encoding='utf-8'))
reparos['status'] = 'produção concluída, conferência técnica passou; importação Unity pendente Claude'
save(ROOT / 'reparos-pixellab-v2.json', reparos)

ui = ROOT / 'ui'
fontes = json.loads((ui / 'fontes.json').read_text(encoding='utf-8'))
rendered = json.loads((ui / 'arenas-renderizadas.json').read_text(encoding='utf-8'))
renders = {r['output']: r for r in rendered}
for item in fontes:
    relative = item['file'].replace('\\', '/')
    item['file'] = relative
    if relative in renders:
        old_source = item['source']
        old_hash = item.pop('sha256', None)
        item.update({'source': renders[relative]['source'], 'sourceSha256': renders[relative]['sourceSha256'],
                     'outputSha256': sha(ui / relative), 'method': 'Blender, fundo transparente, câmera existente',
                     'previousPreview': old_source, 'previousPreviewSha256': old_hash})
    else:
        item['sourceSha256'] = item.pop('sha256', item.get('sourceSha256'))
        item['outputSha256'] = sha(ui / relative)
        assert item['sourceSha256'] == item['outputSha256']
save(ui / 'fontes.json', fontes)
for item in rendered:
    assert sha(REPO / item['source']) == item['sourceSha256'], item['source']
    item['outputSha256'] = sha(ui / item['output'])
save(ui / 'arenas-renderizadas.json', rendered)

manifest = json.loads((ROOT / 'assets.visual.json').read_text(encoding='utf-8'))
entries = manifest['entries']
groups = {}
for e in entries:
    parts = e['resource'].split('/')
    group = parts[0] if parts[0] != 'Sprites' else parts[1]
    groups.setdefault(group, []).append(e['resource'])
inventory = {'assets': len(entries), 'frames': sum(e['frames'] for e in entries),
             'loops': sum(bool(e['loop']) for e in entries),
             'countsIncludeLibraryReuseAliases': True,
             'byGroup': {g: {'count': len(v), 'resources': sorted(v)} for g, v in sorted(groups.items())},
             'integration': 'pendente Claude', 'actorRepairsOnly': ['Baltazar', 'GuardiaoMusgo']}
save(ROOT / 'inventario.json', inventory)

# Uniformiza apenas os textos desta entrega; nenhum arquivo-fonte externo é alterado.
for path in ROOT.rglob('*'):
    if path.is_file() and path.suffix in {'.py', '.js', '.css', '.html', '.json', '.md'}:
        text = path.read_text(encoding='utf-8-sig')
        path.write_text(text, encoding='utf-8', newline='\n')

records = []
for path in sorted(ROOT.rglob('*')):
    if path.is_file() and path.name != 'entrega-sol.visual.json':
        records.append({'file': path.relative_to(ROOT).as_posix(), 'bytes': path.stat().st_size, 'sha256': sha(path)})
save(ROOT / 'entrega-sol.visual.json', {'schema': 'arena-delivery-v1', 'status': 'produção conferida; integração pendente',
     'approval': 'automática autorizada pelo autor', 'files': records})
print(json.dumps({'files': len(records), 'megabytes': round(sum(r['bytes'] for r in records) / 1048576, 2),
                  'assets': len(entries), 'groups': {g: len(v) for g, v in groups.items()}}, ensure_ascii=False))
