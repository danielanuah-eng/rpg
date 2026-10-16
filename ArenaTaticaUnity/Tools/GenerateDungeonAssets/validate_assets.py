"""Validate generated deliverables without Blender or third-party Python modules."""
import hashlib
import json
import math
import struct
import wave
from pathlib import Path

project = Path(__file__).resolve().parents[2]
art = project / 'Assets' / 'DungeonBDX' / 'Art'
manifest = json.loads((art / 'manifest.json').read_text(encoding='utf-8-sig'))
checks = []
for asset in manifest['models'] + manifest['audio']:
    path = art / asset['path']
    assert path.is_file(), path
    assert path.stat().st_size == asset['bytes'], str(path) + ': size mismatch'
    assert hashlib.sha256(path.read_bytes()).hexdigest() == asset['sha256'], str(path) + ': hash mismatch'
    if path.suffix != '.wav':
        continue
    with wave.open(str(path), 'rb') as f:
        assert f.getnchannels() == asset['channels']
        assert f.getframerate() == asset['sampleRate']
        assert f.getsampwidth() == 2
        assert abs(f.getnframes() / f.getframerate() - asset['durationSeconds']) < 1e-8
        raw = f.readframes(f.getnframes())
    values = struct.unpack('<' + 'h' * (len(raw) // 2), raw)
    assert max(abs(v) for v in values) < 32760, str(path) + ': clipping'
    result = {'id': asset['id'], 'status': 'passed', 'bytes': len(raw)}
    if asset['loop']:
        channels = asset['channels']
        seam = max(abs(values[c] - values[-channels + c]) for c in range(channels))
        adjacent = max(abs(values[i] - values[i - channels]) for i in range(channels, len(values)))
        assert seam <= adjacent * 1.2 + 2, str(path) + ': discontinuous loop seam'
        result.update({'seamJumpPCM': seam, 'maxAdjacentJumpPCM': adjacent,
                       'meanPCM': round(sum(values) / len(values), 4)})
    checks.append(result)
source = project / manifest['source']['path']
assert source.is_file()
assert hashlib.sha256(source.read_bytes()).hexdigest() == manifest['source']['sha256']
assert len({x['resource'] for x in manifest['models'] + manifest['audio']}) == 39
assert len({x['sha256'] for x in manifest['audio']}) == 21
result = {'status': 'passed', 'verifiedModelHashes': len(manifest['models']),
          'verifiedAudioHashes': len(manifest['audio']), 'verifiedBlendHash': True,
          'totalModelTrianglesFromBlender': sum(x['triangles'] for x in manifest['models']),
          'checks': checks,
          'notVerified': ['Unity FBX import and runtime playback: integrated project validation required',
                          'Physical mobile/PC performance', 'Author visual and soundtrack acceptance']}
(art / 'validation.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
print(json.dumps({k: v for k, v in result.items() if k != 'checks'}, ensure_ascii=False))
