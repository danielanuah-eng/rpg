"""Matemática dos dados do Arcanoclisma.

Roda com `python3 dados.py` e imprime as tabelas usadas nas regras:
chance de passar em cada Dificuldade, Arcano/Cataclisma gerados por
rolagem, Crítico, Ruptura e Magia Ímpar para cada conjunto de dados.
Quando um número das regras mudar, rode de novo e confira aqui.
"""
from itertools import product
from fractions import Fraction

POOLS = {  # nome: (lados, quantidade)
    "2d12": (12, 2),
    "3d8": (8, 3),
    "4d6": (6, 4),
    "6d4": (4, 6),
    "8d4": (4, 8),  # proposta original, só para comparar
}


def analisar(lados, qtd):
    total = lados ** qtd
    somas = {}
    pares_esp = Fraction(0)
    crit = ruptura = impar = empate = 0
    for r in product(range(1, lados + 1), repeat=qtd):
        s = sum(r)
        somas[s] = somas.get(s, 0) + 1
        p = sum(1 for d in r if d % 2 == 0)
        i = qtd - p
        pares_esp += p
        if lados == 12:
            crit += r[0] == r[1]
        else:
            crit += i == 0
            ruptura += p == 0
        if i > p and not (lados == 12 and r[0] == r[1]):
            impar += 1
        empate += i == p
    return total, somas, pares_esp / total, crit / total, ruptura / total, impar / total, empate / total


def pct(x):
    return f"{100 * float(x):5.1f}%"


def main():
    print("## Conjuntos de dados\n")
    print("| dados | mín | máx | média | Arcano/rolagem | Cataclisma/rolagem | Crítico | Ruptura | Magia Ímpar | Empate |")
    print("|---|---|---|---|---|---|---|---|---|---|")
    dados = {}
    for nome, (l, q) in POOLS.items():
        total, somas, pe, c, ru, im, em = analisar(l, q)
        dados[nome] = (total, somas)
        media = sum(s * n for s, n in somas.items()) / total
        print(f"| {nome} | {q} | {l*q} | {media:.1f} | {float(pe):.1f} | {q-float(pe):.1f} | {pct(c)} | {pct(ru)} | {pct(im)} | {pct(em)} |")

    print("\n## Chance de tirar X ou mais (sem bônus)\n")
    alvos = list(range(8, 23))
    print("| dados | " + " | ".join(str(a) for a in alvos) + " |")
    print("|---|" + "---|" * len(alvos))
    for nome, (total, somas) in dados.items():
        linha = []
        for a in alvos:
            ok = sum(n for s, n in somas.items() if s >= a)
            linha.append(f"{100*ok/total:.0f}")
        print(f"| {nome} | " + " | ".join(linha) + " |")

    print("\n## Conjuração no Grau máximo da Moldura (dados + Sintonia contra a Dificuldade do Grau)\n")
    # Sintonia = Moldura + 1 (o +1 vem do Canal)
    graus = [("2d12", 2, 11, "G2 na Moldura 1"), ("3d8", 3, 13, "G3 na Moldura 2"),
             ("4d6", 4, 15, "G4 na Moldura 3"), ("6d4", 5, 18, "G5 na Moldura 4")]
    print("| caso | dados | Sintonia | Dificuldade | chance |")
    print("|---|---|---|---|---|")
    for nome, sint, dif, caso in graus:
        total, somas = dados[nome]
        ok = sum(n for s, n in somas.items() if s + sint >= dif)
        print(f"| {caso} | {nome} | +{sint} | {dif} | {100*ok/total:.0f}% |")

    print("\n## Ataque mágico (dados + Sintonia = Moldura + 1) contra a Dificuldade típica de adversário\n")
    adv = [(1, 12), (2, 14), (3, 16), (4, 18)]
    print("| Moldura | Dif. adversário | " + " | ".join(POOLS) + " |")
    print("|---|---|" + "---|" * len(POOLS))
    for pat, dif in adv:
        cel = []
        for nome, (total, somas) in dados.items():
            ok = sum(n for s, n in somas.items() if s + pat + 1 >= dif)
            cel.append(f"{100*ok/total:.0f}%")
        print(f"| {pat} | {dif} | " + " | ".join(cel) + " |")

    print("\n## Teste comum: 2d12 + atributo\n")
    total, somas = dados["2d12"]
    print("| atributo | Dif 10 | Dif 12 | Dif 15 | Dif 20 | Dif 25 |")
    print("|---|---|---|---|---|---|")
    for m in (-1, 0, 1, 2, 3, 4, 5):
        cel = []
        for d in (10, 12, 15, 20, 25):
            ok = sum(n for s, n in somas.items() if s + m >= d)
            cel.append(f"{100*ok/total:.0f}%")
        print(f"| {m:+d} | " + " | ".join(cel) + " |")

    print("\n## Adversário: d20 + ataque contra Esquiva\n")
    print("| ataque | Esq 9 | Esq 10 | Esq 11 | Esq 12 | Esq 13 | Esq 14 |")
    print("|---|---|---|---|---|---|---|")
    for a in (1, 2, 3, 4, 5, 6, 7):
        cel = [f"{max(0, min(20, 21 - (e - a))) * 5}%" for e in (9, 10, 11, 12, 13, 14)]
        print(f"| +{a} | " + " | ".join(cel) + " |")


if __name__ == "__main__":
    main()
