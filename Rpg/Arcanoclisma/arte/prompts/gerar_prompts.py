#!/usr/bin/env python3
"""Gera a lista de prompts de arte do catálogo (Fase 4.5).

Lê os .md do livro e escreve:
  - ../../25 Prompts de arte - catálogo.md  (para ler e conferir)
  - catalogo.json                            (id, tipo, formato, prompt completo: para a geração em lote, 4.6)

Rode de novo sempre que o bestiário, os itens, os terrenos ou os lugares mudarem:
  python3 Rpg/Arcanoclisma/arte/prompts/gerar_prompts.py
"""
import json, re, unicodedata
from pathlib import Path

AQUI = Path(__file__).resolve().parent
LIVRO = AQUI.parent.parent

def ler(nome):
    return (LIVRO / nome).read_text(encoding='utf-8')

def slug(t):
    t = unicodedata.normalize('NFKD', t).encode('ascii', 'ignore').decode()
    t = re.sub(r'[^a-zA-Z0-9]+', '-', t).strip('-').lower()
    return t

def limpo(t):
    t = re.sub(r'\[\[(.+?)\]\]', r'\1', t)
    t = re.sub(r'\*+', '', t)
    t = re.sub(r'\s*❓\s*P-\d+', '', t)
    return re.sub(r'\s+', ' ', t).strip()

def tabela(texto, cabecalho):
    """Linhas (listas de células) da tabela markdown que começa com `cabecalho`."""
    i = texto.index(cabecalho)
    linhas = texto[i:].split('\n')
    out = []
    for ln in linhas[2:]:
        if not ln.startswith('|'):
            break
        out.append([c.strip() for c in ln.strip().strip('|').split('|')])
    return out

# ---------------------------------------------------------------- estilos
# Estilos em inglês: no teste do Canva (4.6) o estilo em inglês seguiu o traço grosso e chapado;
# em português saiu realista. A parte específica de cada prompt continua em português.
INK = ('Bold graphic fantasy RPG illustration in a thick expressive ink style: heavy black outlines, flat vibrant colors with hard block shadows, dry-brush paper texture, stylized exaggerated proportions, comic-book cel shading, not realistic, not 3D, not a painting.')
ESTILOS = {
    'P': ('Personagem e criatura (1:1)',
          INK + ' Full body character, dynamic pose, strong facial expression. '
          'Plain solid white background, no scenery, no text, no logo. Subject (in Portuguese):'),
    # Dragões: traço do Wilderfeast (INK) + as referências do dono (imponência, corpo nobre e alongado, escamas como
    # cristal, paleta curta com acento que brilha, chamas bruxuleantes em volta do corpo) + a lore do 08: nascem de um
    # desejo antigo que ganhou carne; são Arcano e Cataclisma num corpo só (partes fechadas e inteiras, partes rachadas
    # e mudando); nobres até na violência, orgulhosos, cheios de alma, com peso emocional; os mortais são estações.
    'D': ('Dragão (1:1)',
          INK + ' An imposing, noble full body dragon seen slightly from below, head raised, chest proud, huge wingspan, '
          'majestic even when threatening. It is an ancient desire that became flesh and scales, complete, alive and full of soul: '
          'made of order and change at once, part of its body whole, closed and symmetrical, part cracked open with light '
          'leaking through and shapes shifting. Slender elegant body with a long neck and long tail. Soft, smooth, organic scales: '
          'small overlapping scales that flow with the curves of the body like the skin of a snake or a lizard, drawn with few '
          'simple strokes and soft gradients of flat color; NOT stone, NOT rock, NOT armor plates, NOT rigid or artificial; only a '
          'few spines. A limited palette of two or three hues plus one glowing accent color. The flames are NOT '
          'realistic fire: they are ghostly, translucent, stylized flame shapes drawn as clean flat graphic ribbons and smoke-like '
          'curls with simple edges, flowing in long elegant curves that trace and frame the silhouette of the body like a spectral '
          'mantle. Their color, shape and behavior follow the dragon\'s Value, as described in the subject. They appear only in a few places: the wing tips, the tip of the tail, the crest along the spine and the eyes, '
          'where the body seems to dissolve into flame; plus a few thin glowing veins along the scales. '
          'Deep expressive eyes that carry emotional weight, love, fear or longing, looking at the viewer as if mortals were '
          'passing seasons. Plain light gray background, no scenery, no text, no logo, no signature. Subject (in Portuguese):'),
    'I': ('Item (1:1)',
          INK + ' A single isolated object, whole and centered, three-quarter view, signs of use. '
          'Plain solid white background, no hand, no character, no text, no logo. Subject (in Portuguese):'),
    'C': ('Cenário (paisagem 3:2)',
          'Fantasy RPG environment illustration in a bold graphic ink style: heavy black outlines, flat vibrant colors with '
          'hard block shadows, dry-brush paper texture, wide establishing view, dramatic light, chapter-opening mood, '
          'no characters in the foreground, no text, no logo. Subject (in Portuguese):'),
}
FORMATO = {'P': '1:1', 'D': '1:1', 'I': '1:1', 'C': '3:2'}

COR = {
    'Conflitüo': 'magia vermelho-ferrugem', 'Sublimeyn': 'magia prata-mercúrio', 'Dominum': 'magia roxo-sangue',
    'Persona': 'magia coral', 'Alvorum': 'magia dourada e preta', 'Morbidyum': 'magia verde-pálida',
    'Cognitium': 'magia índigo', 'Eternyr': 'magia azul-noite com riscos brancos', 'Primitivum': 'magia verde-musgo e âmbar',
}
def cor_de(texto):
    for v, c in COR.items():
        if v in texto:
            return c
    return ''

itens = []  # dicts: id, tipo, grupo, nome, meta, estilo, especifico
ids = set()
def add(tipo, grupo, nome, meta, estilo, especifico, id_=None, prioridade=False):
    i = id_ or slug(nome)
    if i in ids:
        i = f'{i}-{slug(grupo)}'
    ids.add(i)
    itens.append(dict(id=i, tipo=tipo, grupo=grupo, nome=nome, meta=meta, estilo=estilo,
                      formato=FORMATO[estilo], especifico=especifico.strip(), prioridade=prioridade))

# ---------------------------------------------------------------- armas e armaduras (06)
ARMAS = {
    'Adaga': 'adaga curta de lâmina larga, cabo de couro gasto',
    'Espada curta': 'espada curta reta e equilibrada, guarda simples',
    'Espada longa': 'espada longa de duas mãos, lâmina comprida e guarda em cruz',
    'Machado': 'machado de uma mão com lâmina curva e cabo de madeira escura',
    'Martelo de guerra': 'martelo de guerra enorme de duas mãos, cabeça maciça de metal amassado',
    'Lança': 'lança longa com ponta de folha e fitas amarradas perto da ponta',
    'Chicote': 'chicote de couro trançado enrolado, ponta com nó de metal',
    'Cajado': 'cajado de madeira nodosa, alto como uma pessoa, com uma pedra presa no topo',
    'Soqueira': 'soqueira de metal com quatro anéis grossos e rebites',
    'Funda': 'funda de couro com tiras compridas e uma pedra lisa no bolsão',
    'Arco curto': 'arco curto de madeira clara com corda tensa',
    'Arco longo': 'arco longo, alto, de madeira escura laminada',
    'Besta': 'besta de madeira e ferro com manivela de recarga',
    'Pistola': 'pistola tecnomaga de latão e madeira, cano curto com runas, engrenagens à mostra',
    'Rifle': 'rifle tecnomago comprido de latão e madeira, cano com runas e mira de lente',
}
t06 = ler('06 Itens e dinheiro.md')
for r in tabela(t06, '| Arma | Combina com |'):
    nome = limpo(re.sub(r'\*\(.*?\)\*', '', r[0]))
    desc = ARMAS.get(nome, nome.lower())
    add('arma', 'Armas', nome, f'{r[2]} · {r[3]} · {r[5]}', 'I', f'{desc}.', prioridade=True)
add('arma', 'Armas', 'Espada de Bronze Arco', 'metal arcano comum', 'I',
    'espada longa de Bronze Arco, metal dourado-escuro, com runas gravadas ao longo da lâmina acendendo em luz vermelho-ferrugem.',
    prioridade=True)
ARMAD = {'Leve': 'armadura leve de couro e gibão acolchoado, casaco reforçado, exposta num manequim de madeira simples',
         'Média': 'armadura média de cota de malha e escamas de metal, exposta num manequim de madeira simples',
         'Pesada': 'armadura pesada de placas de metal com rebites e amassados, exposta num manequim de madeira simples'}
for k, v in ARMAD.items():
    add('armadura', 'Armaduras', f'Armadura {k}', 'Proteção', 'I', v + '.', id_=f'armadura-{slug(k)}', prioridade=True)

# ---------------------------------------------------------------- itens mágicos (06), consumíveis, montarias, materiais
FAM = {'Palavra': 'uma palavra de Valor gravada, brilhando de leve', 'Ofício': 'útil e bem-feito, de artesão',
       'Estabilidade': 'calmo e equilibrado, brilho sereno', 'Grau': 'carregado de poder, brilho forte', 'Valor': ''}
for r in tabela(t06, '| Item | Família | Moldura | Efeito | Preço |'):
    nome, fam, mold, efeito = limpo(r[0]), limpo(r[1]), r[2], limpo(r[3])
    base = fam.split(' ')[0]
    cor = cor_de(fam) or cor_de(efeito)
    extra = FAM.get(base, '')
    txt = f'{nome}: objeto mágico ({extra}{", " if extra and cor else ""}{"brilho de " + cor if cor else ""}).'
    add('item-magico', 'Itens mágicos (Relíquias)', nome, f'{fam} · M{mold}', 'I', txt)
for r in tabela(t06, '| Item | Efeito | Preço |'):
    add('consumivel', 'Consumíveis', limpo(r[0]), limpo(r[1]), 'I',
        f'{limpo(r[0])}: consumível de fantasia (frasco, pacote, vidrinho ou pote conforme o nome), rótulo sem letras.')
for r in tabela(t06, '| Montaria comum | Traço | Preço |'):
    est = 'I' if 'Barco' in r[0] else 'P'
    add('montaria', 'Montarias', limpo(r[0]), limpo(r[1]), est,
        f'{limpo(r[0])} de viagem, arreado e pronto, com alforjes; {limpo(r[1])}.')
for r in tabela(t06, '| Valor | Material |'):
    nm = limpo(r[1]).split(':')[0]
    cor = ('com brilho de ' + COR[limpo(r[0])]) if limpo(r[0]) in COR else 'âmbar escuro com uma rachadura que brilha em ferrugem'
    add('material', 'Materiais', nm, limpo(r[0]), 'I', f'{nm}: material raro de fantasia, um pedaço bruto, {cor}.')

# ---------------------------------------------------------------- itens do dia a dia e maravilhas (12)
t12 = ler('12 Tesouros e itens mágicos.md')
for cab, grupo in [('## Itens mágicos do dia a dia (d66)', 'Itens mágicos do dia a dia'), ('## Maravilhas (d66)', 'Maravilhas')]:
    sec = t12[t12.index(cab):]
    for r in tabela(sec, '| d66 | Item | O que faz | Porém |'):
        add('item-narrativo', grupo, limpo(r[1]), f'd66 {r[0]}', 'I',
            f'{limpo(r[1])}: objeto mágico do dia a dia, com cara de coisa comum e um detalhe estranho. O que faz: {limpo(r[2])}.')

# ---------------------------------------------------------------- artefatos (06 + 21)
add('artefato', 'Artefatos', 'A Coroa de Vyal', 'Persona', 'I',
    'A Coroa de Vyal: coroa fina de ouro rosado com uma escama de dragão coral no centro, brilho coral pulsando como um coração.',
    prioridade=True)
t21 = ler('21 Kit de campanha.md')
for r in tabela(t21, '| d8 | Artefato | Começa com | Rumor |'):
    add('artefato', 'Artefatos', limpo(r[1]), f'palavra {limpo(r[2])}', 'I',
        f'{limpo(r[1])}: artefato lendário, antigo e único. Rumor: {limpo(r[3])}.', prioridade=True)

# ---------------------------------------------------------------- terrenos (07)
TERRENOS = {
    'Forja dos Mil Golpes': 'forja gigantesca de pedra negra, bigornas do tamanho de casas, martelos que batem sozinhos, rios de metal em brasa, fumaça e faíscas vermelhas',
    'Charco Fervente': 'pântano que ferve, poças de lama borbulhando entre lajes de pedra que sobem e afundam, vapor prateado',
    'Floresta que Anda': 'floresta densa em que as árvores têm raízes como pernas e mudam de lugar, trilhas que se fecham, luz âmbar entre as folhas',
    'Feira das Máscaras': 'feira noturna lotada, barracas coloridas, todo mundo de máscara, lanternas coral, música no ar',
    'Arena de Kairvahnn': 'arena circular de pedra com arquibancadas cheias, correntes pendendo, portões de jaula, poeira e luz roxa',
    'Farol Sem Noite': 'farol altíssimo num penhasco jogando luz dourada em todas as direções, sem nenhuma sombra no chão, céu escuro em volta',
    'Cemitério que Respira': 'cemitério antigo sob névoa verde-pálida, a terra das covas subindo e descendo devagar como um peito respirando',
    'Biblioteca que Esquece': 'biblioteca labiríntica de estantes infinitas, livros abertos com páginas em branco, letras índigo escapando e se apagando no ar',
    'Estação Parada': 'estação de trem tecnomaga com um relógio gigante parado, vapor congelado no ar, trem imóvel de latão, poeira suspensa',
    'Cidade-Muralha': 'cidade cercada por muralhas altas e grossas, ruas estreitas cheias de gente armada, estandartes vermelho-ferrugem',
    'Porto de Vidro': 'cidade portuária toda de vidro, casas e cais transparentes brilhando ao sol, barcos atracados, reflexos prateados',
    'Mercado de Feras': 'mercado barulhento de jaulas e cercados, feras exóticas, domadores com chicotes, lama e palha, luz roxa nas coleiras',
    'Vila do Mesmo Dia': 'vila pequena ao pôr do sol, sombras repetidas como ecos, relógio de torre, céu azul-noite com riscos brancos',
    'Cidade Insone': 'cidade noturna toda acesa, janelas iluminadas, gente acordada nas ruas, sonhos dourados e pretos vazando das janelas como fumaça',
    'Aldeia da Curandeira': 'aldeia pequena de ervas penduradas nas portas, horta de plantas medicinais, cabana da curandeira com brilho verde-pálido',
    'Rua dos Espelhos': 'rua estreita cheia de espelhos nas paredes, cada reflexo mostrando uma cena diferente, luz índigo',
    'Vila da Colheita': 'vila rural em época de colheita, campos dourados, carroças de feno, bandeirolas, luz âmbar de fim de tarde',
    'Estrada do Crepúsculo': 'estrada longa que corta a paisagem ao meio: um lado em pleno dia, o outro em noite estrelada, a linha do crepúsculo cruzando a estrada',
    'Mina Funda': 'galeria de mina profunda, vigas de madeira, trilhos, minério prateado brilhando nas paredes, pouca luz de lanterna',
    'Pátio de Duelos': 'pátio de pedra de uma escola de magia, círculos de duelo desenhados no chão, arquibancadas de alunos, marcas de feitiço',
    'Torre dos Protetores': 'torre alta e austera de pedra cinza coberta de selos e lacres, uma cúpula apagada em volta onde nenhuma magia brilha',
}

t07 = ler('07 Mestre - Cataclisma, abalos e terrenos.md')
for cab in ['| Terreno | Moldura · Tipo · Valor | Traço | Ações de Cataclisma |']:
    for pos in [m.start() for m in re.finditer(re.escape(cab), t07)]:
        for r in tabela(t07[pos:], cab):
            nome = limpo(re.sub(r'\*\(.*?\)\*', '', r[0]))
            info = limpo(r[1])
            cor = cor_de(info)
            vis = TERRENOS.get(nome, limpo(r[2]).rstrip('.'))
            add('terreno', 'Terrenos', nome, info, 'C',
                f'{nome}: {vis}.{" Sinais de " + cor + " no ar." if cor else ""}')

# ---------------------------------------------------------------- lugares (21)
sec = t21[t21.index('## Lugares'):t21.index('## Entidades')]
for bloco in sec.split('\n### ')[1:]:
    linhas = bloco.strip().split('\n')
    nome = limpo(linhas[0])
    esboco = limpo(linhas[1]) if len(linhas) > 1 else ''
    esboco = [f for f in re.split(r'(?<=\.)\s', esboco) if not re.match(r'(Senhora?|Rei|Rainha|Onde vive)\b', f)]
    esboco = ' '.join(esboco[:2])
    sent = next((limpo(l.split(':', 1)[1]) for l in linhas if l.startswith('- **Sentimento**')), '')
    add('lugar', 'Lugares', nome, sent.split('.')[0], 'C', f'{nome}: {esboco} Clima: {sent.split(".")[0]}.', prioridade=True)

# ---------------------------------------------------------------- reinos e povos (08)
t08 = ler('08 Mundo e entidades.md')
for r in tabela(t08, '| Plano | Valor | Quem (ou o quê) está lá |'):
    nome, valor = limpo(r[0]), limpo(r[1])
    add('reino', 'Reinos', nome, valor, 'C',
        f'{nome}, o plano de {valor}: {limpo(r[3])}. Quem vive lá: {limpo(r[2])}. Cor dominante: {COR[valor].replace("magia ", "")}.',
        prioridade=True)
for r in tabela(t08, '| Povo | Esboço | Pesa em |'):
    nome = limpo(r[0])
    esb = re.sub(r'\s*\(.*?\)', '', limpo(r[1])).replace('; pouca facilidade com o Arcano', '')
    desc_povo = f'Uma pessoa do povo {nome}, roupas de viagem simples: {esb}.'
    add('povo', 'Povos', nome, limpo(r[2]), 'P',
        desc_povo, id_=f'povo-{slug(nome)}', prioridade=True)
add('povo', 'Povos', 'Innhomus', 'fora de Contacto', 'P',
    'Um Innhomus: divindade esquecida e cataclísmica, ser disforme e abstrato feito de formas geométricas, letras soltas, sinais e símbolos flutuando juntos, quase indescritível.',
    id_='povo-innhomus', prioridade=True)

# ---------------------------------------------------------------- dragões (08)
# Chamas por Valor: a cor e o jeito da chama seguem o Valor do dragão (08, coluna Chamas), sempre espectrais.
CHAMAS = {
    'Conflitüo': 'ghostly rust-red flames shaped like blade edges and flying shards, pulsing like a heartbeat in battle',
    'Sublimeyn': 'ghostly silver-blue flames that keep changing state as they rise: vapor, then droplets, then little crystal shards',
    'Dominum': 'ghostly violet flames shaped like chain links and collar rings, held tight to the body, burning brightest in the eyes',
    'Persona': 'ghostly coral-pink flames soft as breath that shift hue with its mood, sometimes curling into faint face-like shapes',
    'Alvorum': 'ghostly flames split in two: radiant white flames on the spines and black shadow-flames in the eyes, like day and night',
    'Morbidyum': 'ghostly pale-green flames, cold and trembling, drifting upward like will-o-the-wisps and small departing souls',
    'Cognitium': 'ghostly almost colorless veil-like flames, seen only at the edge of sight, blurring into indigo mist and thin labyrinth lines',
    'Eternyr': 'ghostly night-blue flames with tiny white star points that move far too slowly, leaving frozen trails like long exposures',
    'Primitivum': 'ghostly amber flames like fireflies and floating seeds, drifting around moss and branch-like horns',
}
for r in tabela(t08, '| Valor | Dragão | Cor e forma |'):
    valor, nome = limpo(r[0]), limpo(r[1])
    add('dragao', 'Dragões', nome, valor, 'D',
        f'{nome}, dragão de {valor}: {limpo(r[2])}. Its flames: {CHAMAS[valor]}. '
        f'O sopro: {re.sub(r" *[(].*?[)]", "", limpo(r[4]))}.', prioridade=True)
add('dragao', 'Dragões', 'Wyvern', 'cria de dragão', 'D',
    'Wyvern, a cria de um dragão: menor, duas patas e asas, sem chamas, olhar de bicho, arreio de couro de montaria.', prioridade=True)

# ---------------------------------------------------------------- bestiário (11), com os prompts feitos à mão do 20
t20 = ler('20 Prompts de arte - personagens.md')
feitos = {}
for r in tabela(t20, '| id | Personagem | Onde no livro |'):
    nome20 = limpo(r[1]).split('(')[0].strip()
    feitos[nome20] = (r[0].strip('`'), limpo(r[3]))

MOLDURA = {'Gente comum': 'pessoa comum do mundo, roupas de trabalho e de rua',
           'Comprometidos': 'um Comprometido (usuário de magia), a magia visível em volta das mãos',
           'Povos infundidos': 'de um povo infundido, o Valor visível no próprio corpo',
           'Feras e monstros': 'criatura selvagem',
           'Disruptos (nascidos do Cataclisma)': 'criatura nascida do Cataclisma, formas quebradas, frestas e pedaços que escapam do corpo, cores sujas',
           'Protetores do Arcano': 'guardião que anula a magia, armadura com selos e lacres',
           'Seres de energia': 'ser feito de energia luminosa e sólida, bruxuleante',
           'Seres dos planos dos Valores': 'ser de outro plano, disforme e estranho',
           'Semideuses': 'semideus, presença enorme e solene, aura divina',
           'Dragões e wyverns': 'dragão ou wyvern',
           'Era Tecnomaga': 'estética tecnomaga: magia e máquina, latão, vapor e runas'}
PAPEL = {'Lacaio': 'um de um bando', 'Chefe': 'pose imponente de chefe', 'Líder': 'postura de quem comanda',
         'Bruto': 'grande e pesado', 'Ágil': 'leve e veloz', 'Social': 'pose de conversa', 'Suporte': 'pose de quem ajuda os outros'}
t11 = ler('11 Bestiário.md')
secoes = re.split(r'\n## ', t11)
for s in secoes[1:]:
    titulo = s.split('\n')[0].strip()
    for m in re.finditer(r'^\*\*([^*]+)\*\*([^—\n]*) — (M\d)([^\n]*)\n((?:(?!\n\n).)*)', s, re.M | re.S):
        nome = limpo(m.group(1))
        extra = limpo(m.group(2)).strip(' ()')
        cab = limpo(m.group(3) + m.group(4))
        corpo = m.group(5)
        campos = dict(re.findall(r'(Ser|Agir|Parecer|Quer): (.*?)(?= · (?:Ser|Agir|Parecer|Quer):|\n|$)', corpo))
        partes = [p.strip() for p in cab.split('·')]
        papel = partes[1] if len(partes) > 1 else ''
        valor = partes[2] if len(partes) > 2 else ''
        chave = next((k for k in feitos if k and k.split(',')[0] in nome), None)
        if chave:
            i, pr = feitos[chave]
            add('adversario', f'Bestiário · {titulo}', nome, f'{cab} · prompt do [[20 Prompts de arte - personagens]]', 'P', pr, id_=i)
            continue
        IDADE = {'Dragonete': 'um filhote pequeno, curioso e selvagem; o desejo nele é puro impulso: fome, brilho, a mãe',
                 'Dragão Jovem': 'um dragão jovem, fera territorial e faminta, ainda sem sabedoria, desejo em impulso',
                 'Dragão Adulto': 'um dragão adulto, senhor do seu Domínio, orgulhoso e consciente do próprio desejo',
                 'Dragão Ancião': 'um dragão ancião, memória do mundo, sábio e paciente, cicatrizes antigas, Eras inteiras no olhar'}
        if nome in IDADE:
            add('adversario', f'Bestiário · {titulo}', nome, cab, 'D',
                f'{nome}: {IDADE[nome]}. Cor, forma e chamas do Valor do dragão (desenhe no Valor escolhido; padrão: Conflitüo, vermelho-ferrugem).')
            continue
        ser = limpo(campos.get('Ser', '')).lower()
        agir = limpo(campos.get('Agir', ''))
        acao = re.split(r'[ (·]', agir)[0].lower() if agir else ''
        parecer = limpo(campos.get('Parecer', '')).lower()
        cor = cor_de(valor)
        txt = f'{nome}{" (" + extra + ")" if extra else ""}: {ser}; {MOLDURA.get(titulo, "")}'
        if parecer:
            txt += f'; jeito {parecer}'
        if PAPEL.get(papel):
            txt += f'; {PAPEL[papel]}'
        if acao:
            txt += f'; no gesto de {acao}'
        if cor:
            txt += f'; {cor}'
        est = 'D' if re.search(r'\bdrag(ão|oa|ões)\b', ser + ' ' + nome.lower()) else 'P'
        add('adversario', f'Bestiário · {titulo}', nome, cab, est, txt + '.')

# ---------------------------------------------------------------- saída
for it in itens:
    it['prompt'] = f"{ESTILOS[it['estilo']][1]} {it['especifico']}"
(AQUI / 'catalogo.json').write_text(json.dumps(
    [{k: it[k] for k in ('id', 'tipo', 'nome', 'formato', 'prioridade', 'prompt')} for it in itens],
    ensure_ascii=False, indent=1), encoding='utf-8')

grupos = []
for it in itens:
    if it['grupo'] not in grupos:
        grupos.append(it['grupo'])
n_pri = sum(1 for it in itens if it['prioridade'])
md = [f'# 25 — Prompts de arte: o catálogo (Fase 4.5)',
      '',
      f'> **{len(itens)} imagens**, uma por arma, armadura, item, artefato, terreno, lugar, Reino, povo, dragão e personagem do bestiário. '
      f'**{n_pri} são do livro** (marcadas **livro · gerar primeiro**); as outras são do app do gerador ([[22 O que é gerado]], P-125). '
      'Gerado por `arte/prompts/gerar_prompts.py` a partir dos arquivos do livro; o prompt completo (estilo + específico) está em `arte/prompts/catalogo.json`. '
      'Não edite à mão: mude o arquivo de origem e rode o script de novo. Personagens das sessões continuam em [[20 Prompts de arte - personagens]].',
      '',
      '## Como fazer',
      '',
      '1. Para cada linha, gere a imagem com o **prompt completo** do `catalogo.json` (ou: o **estilo** da linha, abaixo, seguido do texto da linha). Canva, *generate-image*, no **formato** da linha (1:1 ou paisagem 3:2).',
      '2. Salve como **`<id>.png`** (o id da linha) em `arte/img/<tipo>/` (ex.: `arte/img/dragao/dragao-lamina.png`).',
      '3. Nas imagens 1:1 o fundo é branco liso: tire o fundo antes de usar (4.6). Cenários (3:2) ficam inteiros.',
      '4. Confira uma a uma: é o objeto ou o ser certo, inteiro, sem texto nem letras inventadas, no traço do estilo e na cor do Valor. Se estiver ruim (cortado, com cenário, com mão, com texto), gere de novo em vez de ajustar.',
      '5. Se o filtro barrar, troque só a palavra que parece violenta (arma, sangue, morte) por uma mais neutra (ferramenta, marca, sono) e gere de novo.',
      '',
      '## Estilos',
      '',
      '| Código | Para | Texto (vai antes de cada prompt) |',
      '|---|---|---|']
for k, (para, txt) in ESTILOS.items():
    md.append(f'| **{k}** | {para} | {txt} |')
md.append('')
md.append('## Lista')
for g in grupos:
    do = [it for it in itens if it['grupo'] == g]
    md += ['', f'### {g} ({len(do)})', '']
    for it in do:
        pri = ' · **livro · gerar primeiro**' if it['prioridade'] else ''
        md.append(f"- **{it['nome']}** · {it['meta']} · estilo **{it['estilo']}** · {it['formato']}{pri}  ")
        md.append(f"  id `{it['id']}`  ")
        md.append(f"  > {it['especifico']}")
(LIVRO / '25 Prompts de arte - catálogo.md').write_text('\n'.join(md) + '\n', encoding='utf-8')
print(len(itens), 'prompts;', n_pri, 'do livro;', len(grupos), 'grupos')
