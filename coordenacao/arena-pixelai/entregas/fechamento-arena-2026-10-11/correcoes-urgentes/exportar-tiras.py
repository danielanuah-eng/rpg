"""Empacota os PNGs baixados do PixelLab; não desenha nem altera os quadros."""
from pathlib import Path
from PIL import Image
import hashlib
import io
import json
import re
import zipfile

ROOT = Path(__file__).resolve().parent


def main():
    inspection = json.loads((ROOT / 'inspecao-fontes.json').read_text(encoding='utf-8'))
    verification = json.loads((ROOT / 'verificacao.json').read_text(encoding='utf-8'))
    assert verification['status'] == 'passed' and verification['frames'] == 42
    assets = []
    for row in inspection:
        actor, action, direction = (row[k] for k in ('actor', 'action', 'direction'))
        with zipfile.ZipFile(ROOT / 'fontes' / f'{actor}-depois.zip') as archive:
            state = json.loads(archive.read('metadata.json'))['states'][0]
            names = state['frames']['animations'][action][direction]
            frames = [Image.open(io.BytesIO(archive.read(n))).convert('RGBA') for n in names]
            assert len(frames) == row['frames'] and all(list(f.size) == row['size'] for f in frames)
            width, height = frames[0].size
            strip = Image.new('RGBA', (width * len(frames), height))
            for index, frame in enumerate(frames):
                strip.paste(frame, (index * width, 0))
            relative = Path('tiras') / actor / f'{action}-{direction}.png'
            destination = ROOT / relative
            destination.parent.mkdir(parents=True, exist_ok=True)
            strip.save(destination)
            reopened = Image.open(destination).convert('RGBA')
            for index, frame in enumerate(frames):
                assert reopened.crop((index * width, 0, (index + 1) * width, height)).tobytes() == frame.tobytes()
            dx, dy = row['centralizarDelta']
            save = json.loads((ROOT / f'salvamentos-{actor}-{action}-{direction}.json').read_text(encoding='utf-8'))
            save_text = '\n'.join(c.get('text', '') for c in save['save']['content'])
            undo = re.search(r'undo: save_to_asset\(image="([^"]+)"', save_text)
            assert undo and not save['save'].get('isError', False)
            assets.append({
                'actor': actor, 'characterId': state['character']['id'], 'action': action,
                'direction': direction, 'groupId': row['group'], 'file': relative.as_posix(),
                'sha256': hashlib.sha256(destination.read_bytes()).hexdigest(),
                'frameSize': [width, height], 'frames': len(frames), 'layout': 'horizontal-left-to-right',
                'fps': None, 'timingPolicy': 'preservar o timing já usado no cliente; download sem duração',
                'pixelTranslationTopLeft': [dx, dy],
                'pivotDeltaForExistingNormalizedUnityPivot': [dx / width, -dy / height],
                'pivotPolicy': 'Claude deve conferir o apoio entre ações; delta é constante por clipe',
                'pixelLabSaveRecord': f'salvamentos-{actor}-{action}-{direction}.json',
                'undoImageId': undo.group(1), 'technicalValidation': 'passed',
                'visualReview': 'enquadramento visto pelo Sol; corte no cliente ainda não confirmado',
                'integration': 'pendente_claude'
            })
    result = {
        'version': 1, 'project': 'Arena Tatica', 'generationCostPixelLab': 0,
        'method': 'edicao exata e salvamento no PixelLab; exportacao local sem alterar arte',
        'sourceClippingObserved': False,
        'limitation': 'As fontes tinham margens. Deslocamento nao recupera pixels ausentes nem comprova reparo do visor/importador.',
        'status': 'enquadramento_salvo; aguardando_avaliacao_no_cliente', 'assets': assets
    }
    (ROOT / 'enquadramentos.visual.json').write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8', newline='\n')
    print(json.dumps({'strips': len(assets), 'frames': sum(a['frames'] for a in assets), 'exportPixelExact': True}))


if __name__ == '__main__':
    main()
