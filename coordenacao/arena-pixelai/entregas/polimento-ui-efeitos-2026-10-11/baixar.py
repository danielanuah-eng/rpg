"""Baixa somente URLs oficiais registradas; não recebe segredos nem modifica Assets."""
from pathlib import Path
import concurrent.futures, hashlib, json, sys, urllib.request

ROOT = Path(__file__).resolve().parent
spec = json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'))
def fetch(item):
    dest = (ROOT / item['file']).resolve()
    if not dest.is_relative_to(ROOT):
        raise ValueError('Destino fora da entrega')
    if not item['url'].startswith(('https://api.pixellab.ai/', 'https://backblaze.pixellab.ai/')):
        raise ValueError('Fonte não oficial')
    dest.parent.mkdir(parents=True, exist_ok=True)
    if dest.exists() and dest.stat().st_size > 0:
        data = dest.read_bytes()
        return {'file': item['file'], 'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()}
    req = urllib.request.Request(item['url'], headers={'User-Agent': 'ArenaTatica-art-delivery/1.0'})
    with urllib.request.urlopen(req, timeout=120) as response:
        data = response.read()
    if not data:
        raise ValueError('Arquivo vazio')
    temp = dest.with_suffix(dest.suffix + '.part')
    temp.write_bytes(data)
    temp.replace(dest)
    return {'file': item['file'], 'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()}
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
    results = list(pool.map(fetch, spec))
(ROOT / (Path(sys.argv[1]).stem + '-baixados.json')).write_text(json.dumps(results, indent=2), encoding='utf-8')
print(json.dumps(results))
