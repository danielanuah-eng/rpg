"""Compara pedidos registrados com o snapshot remoto, sem iniciar gerações."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parent


def main():
    snapshot = json.loads((ROOT / 'auditoria-remota-sol.json').read_text(encoding='utf-8'))
    requests = json.loads((ROOT / 'pedidos-conferidos-sol.json').read_text(encoding='utf-8'))
    assert snapshot['status'] == 'consulta_completa' and not snapshot['errors']
    characters = {e['id']: e for e in snapshot['entries']}
    rows = []
    for request in requests['requests']:
        character = characters.get(request['characterId'])
        if character is None:
            rows.append({**request, 'status': 'estado_nao_localizado; conferir_id', 'missing': request['directions']})
            continue
        coverage = set(character['coverage'].get(request['action'], []))
        groups = [g for g in character['groups'] if g['action'] == request['action']]
        group_match = [g for g in character['groups'] if g['group'] == request.get('previousGroupId')]
        if group_match:
            groups += [g for g in group_match if g not in groups]
            coverage.update(d for g in group_match for d in g['directions'])
        alternatives = []
        if not coverage and request['key'] == 'damian-canhao-idle':
            alternatives = [g for g in character['groups'] if g['action'] == 'idle-cannon']
        missing = sorted(set(request['directions']) - coverage)
        status = 'cobertura_remota_completa; revisar_selecao_e_integracao' if not missing else 'lacuna_remota_do_pedido; conferir_reuso_antes_de_gerar'
        if alternatives:
            alternative_dirs = {d for g in alternatives for d in g['directions']}
            if set(request['directions']).issubset(alternative_dirs):
                status = 'idle_equivalente_completo; confirmar_selecao_sem_regenerar'
                missing = []
        rows.append({
            **request, 'status': status, 'covered': sorted(coverage), 'missing': missing,
            'matchedGroups': groups, 'equivalentGroupsToReview': alternatives,
            'review': 'presenca remota nao significa aprovacao nem integracao'
        })
    unique_gaps = {(r['characterId'], r['action'], d) for r in rows for d in r['missing']}
    report = {
        'version': 1, 'date': snapshot['date'], 'remoteStates': len(characters),
        'requests': len(rows), 'uniqueRemoteGaps': len(unique_gaps),
        'policy': 'Consulta tecnica dos pedidos existentes. Variantes e clipes de outro estado exigem selecao; nao iniciar geracao automaticamente.',
        'generationCostPixelLab': 0, 'rows': rows
    }
    (ROOT / 'lacunas-confirmadas-sol.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8', newline='\n')
    print(json.dumps({k: v for k, v in report.items() if k != 'rows'}, ensure_ascii=False))
    for row in rows:
        if row['missing']:
            print(row['key'], ','.join(row['missing']))


if __name__ == '__main__':
    main()
