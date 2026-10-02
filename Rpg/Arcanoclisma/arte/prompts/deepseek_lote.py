#!/usr/bin/env python3
"""Trabalho em lote nos prompts de arte com o DeepSeek, pela OpenRouter.

O que faz hoje:
  traduzir   traduz o assunto de cada prompt (português) para inglês e escreve prompts 100% em inglês:
             catalogo_en.json e todos_os_prompts_en.txt (para Midjourney, Flux, etc.)
  testar     manda um pedido minúsculo para ver se a chave e o modelo funcionam
  registrar  guarda a chave da OpenRouter (pede digitando, sem aparecer na tela)

A chave vem, nesta ordem, de:
  1. a variável de ambiente OPENROUTER_API_KEY
  2. o arquivo ~/.config/arcanoclisma/openrouter_key  (criado por `registrar`; fica fora do repositório)

Uso:
  python3 Rpg/Arcanoclisma/arte/prompts/deepseek_lote.py registrar
  python3 Rpg/Arcanoclisma/arte/prompts/deepseek_lote.py testar
  python3 Rpg/Arcanoclisma/arte/prompts/deepseek_lote.py traduzir            # tudo
  python3 Rpg/Arcanoclisma/arte/prompts/deepseek_lote.py traduzir --limite 30 # só um teste

Retoma de onde parou (guarda o que já traduziu em cache_traducao.json). Rode gerar_prompts.py antes,
para o catalogo.json estar em dia.

Ambiente: o endereço openrouter.ai precisa estar liberado na rede do ambiente.
Modelo padrão: deepseek/deepseek-chat (mude com DEEPSEEK_MODELO, ex.: deepseek/deepseek-r1).
"""
import argparse
import getpass
import json
import os
import sys
import time
import urllib.error
import urllib.request
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

AQUI = Path(__file__).resolve().parent
ARQ_CHAVE = Path.home() / '.config' / 'arcanoclisma' / 'openrouter_key'
BASE = os.environ.get('OPENROUTER_BASE_URL', 'https://openrouter.ai/api/v1').rstrip('/')
MODELO = os.environ.get('DEEPSEEK_MODELO', 'deepseek/deepseek-chat')
MARCA = 'Subject (in Portuguese):'
CACHE = AQUI / 'cache_traducao.json'

SISTEMA = (
    'You translate short visual descriptions for a fantasy tabletop RPG art catalog from Portuguese to English, '
    'to be used as the subject of an image-generation prompt. Rules: keep each translation as short and concrete as the '
    'original, describing only what is visible; keep proper nouns and game terms unchanged (Conflitüo, Sublimeyn, Dominum, '
    'Persona, Alvorum, Morbidyum, Cognitium, Eternyr, Primitivum, Bronze Arco, Ordynus, GrandGaleonis, character names); '
    'do not add details, do not mention artists, do not add style words. You receive a JSON object {id: portuguese_text} '
    'and must answer with ONLY a JSON object {id: english_text} with exactly the same ids.'
)


def chave():
    k = os.environ.get('OPENROUTER_API_KEY', '').strip()
    if k:
        return k
    if ARQ_CHAVE.exists():
        return ARQ_CHAVE.read_text(encoding='utf-8').strip()
    return ''


def registrar():
    k = getpass.getpass('Cole a chave da OpenRouter (não aparece na tela): ').strip()
    if not k:
        sys.exit('Nada digitado.')
    ARQ_CHAVE.parent.mkdir(parents=True, exist_ok=True)
    ARQ_CHAVE.write_text(k, encoding='utf-8')
    ARQ_CHAVE.chmod(0o600)
    print(f'Chave guardada em {ARQ_CHAVE} (só você lê). Teste com: deepseek_lote.py testar')


def chamar(mensagens, tentativas=5):
    k = chave()
    if not k:
        sys.exit('Sem chave. Defina OPENROUTER_API_KEY ou rode: deepseek_lote.py registrar')
    corpo = json.dumps({'model': MODELO, 'messages': mensagens, 'temperature': 0.2}).encode('utf-8')
    espera = 2
    for t in range(1, tentativas + 1):
        req = urllib.request.Request(
            BASE + '/chat/completions', data=corpo, method='POST',
            headers={'Authorization': 'Bearer ' + k, 'Content-Type': 'application/json',
                     'X-Title': 'Arcanoclisma'})
        try:
            with urllib.request.urlopen(req, timeout=120) as r:
                return json.load(r)['choices'][0]['message']['content']
        except urllib.error.HTTPError as e:
            msg = e.read().decode('utf-8', 'ignore')[:300]
            if e.code in (401, 403) and t == 1:
                sys.exit(f'Recusado ({e.code}): {msg}\nChave errada, sem crédito, ou o endereço está bloqueado na rede do ambiente.')
            if e.code in (429, 500, 502, 503, 504) and t < tentativas:
                time.sleep(espera)
                espera *= 2
                continue
            sys.exit(f'Erro {e.code}: {msg}')
        except (urllib.error.URLError, TimeoutError) as e:
            if t < tentativas:
                time.sleep(espera)
                espera *= 2
                continue
            sys.exit(f'Sem rede para {BASE}: {e}')


def json_da_resposta(texto):
    texto = texto.strip()
    if texto.startswith('```'):
        texto = texto.split('\n', 1)[1].rsplit('```', 1)[0]
    i, j = texto.find('{'), texto.rfind('}')
    return json.loads(texto[i:j + 1])


def testar():
    r = chamar([{'role': 'user', 'content': 'Reply with exactly: ok'}], tentativas=1)
    print(f'Modelo {MODELO} respondeu: {r.strip()[:80]}')


def traduzir(limite, lote, paralelo):
    base = json.loads((AQUI / 'catalogo.json').read_text(encoding='utf-8'))
    if limite:
        base = base[:limite]
    cache = json.loads(CACHE.read_text(encoding='utf-8')) if CACHE.exists() else {}
    falta = [x for x in base if x['id'] not in cache]
    print(f'{len(base)} prompts; {len(base) - len(falta)} já no cache; {len(falta)} para traduzir ({MODELO}).')
    blocos = [falta[i:i + lote] for i in range(0, len(falta), lote)]

    def um(bloco):
        pedido = {x['id']: x['prompt'].split(MARCA, 1)[1].strip() for x in bloco}
        for _ in range(2):  # se a resposta vier torta, tenta de novo uma vez
            resp = json_da_resposta(chamar([{'role': 'system', 'content': SISTEMA},
                                            {'role': 'user', 'content': json.dumps(pedido, ensure_ascii=False)}]))
            if set(resp) == set(pedido):
                return {k: str(v).strip() for k, v in resp.items()}
        sys.exit('A resposta não trouxe os mesmos ids. Tente --lote menor.')

    feitos = 0
    with ThreadPoolExecutor(max_workers=paralelo) as ex:
        futs = [ex.submit(um, b) for b in blocos]
        for f in as_completed(futs):
            cache.update(f.result())
            CACHE.write_text(json.dumps(cache, ensure_ascii=False, indent=1), encoding='utf-8')
            feitos += 1
            print(f'  bloco {feitos}/{len(blocos)} ok ({len(cache)} no cache)')

    saida, linhas = [], ['ARCANOCLISMA · PROMPTS DE ARTE · TUDO EM INGLÊS', f'{len(base)} imagens. Cada bloco é um prompt inteiro em inglês.', '']
    for n, x in enumerate(base, 1):
        estilo = x['prompt'].split(MARCA, 1)[0].strip()
        p = f'{estilo} Subject: {cache[x["id"]]}'
        saida.append({'id': x['id'], 'nome': x['nome'], 'formato': x['formato'], 'prioridade': x['prioridade'], 'prompt': p})
        ar = 'square 1:1' if x['formato'] == '1:1' else 'landscape 3:2'
        linhas += [f'===== {n:03d} · {x["id"]} · {ar} · {x["nome"]} =====', p, '']
    (AQUI / 'catalogo_en.json').write_text(json.dumps(saida, ensure_ascii=False, indent=1), encoding='utf-8')
    (AQUI / 'todos_os_prompts_en.txt').write_text('\n'.join(linhas), encoding='utf-8')
    print(f'Escrevi catalogo_en.json e todos_os_prompts_en.txt ({len(saida)} prompts).')


if __name__ == '__main__':
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('comando', choices=['registrar', 'testar', 'traduzir'])
    ap.add_argument('--limite', type=int, default=0, help='só os N primeiros (teste)')
    ap.add_argument('--lote', type=int, default=25, help='prompts por pedido (padrão 25)')
    ap.add_argument('--paralelo', type=int, default=4, help='pedidos ao mesmo tempo (padrão 4)')
    a = ap.parse_args()
    if a.comando == 'registrar':
        registrar()
    elif a.comando == 'testar':
        testar()
    else:
        traduzir(a.limite, a.lote, a.paralelo)
