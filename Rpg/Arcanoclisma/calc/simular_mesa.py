"""Simulador geral: combate em qualquer Moldura, com 2 a 5 jogadores e
encontros de dificuldade diferente, e Eventos sociais com Ameaça.

Personagens genéricos por Moldura (metade de arma, metade de magia);
adversários pela régua de 10 Adversários; orçamento 3 × jogadores + 2.
O mestre ativa um adversário quando emoldura e interrompe (1 Cataclisma)
depois de 3 ações seguidas de jogador, gastando para ativar mais.
Roda: python3 simular_mesa.py
"""
import random
import statistics as st

R = random.Random(3)
d = lambda n: R.randint(1, n)
NIVEL = {1: 1, 2: 3, 3: 6, 4: 9}
POOL = {1: None, 2: (12, 2), 3: (8, 3), 4: (6, 4), 5: (4, 6)}
CUSTO = {1: 0, 2: 1, 3: 2, 4: 3, 5: 5}
REGUA = {  # Dif, Maior, Grave, PV, ataque, (dados, lados, bônus) — régua nova (P-111)
    1: (12, 8, 14, 5, 1, (1, 10, 2)), 2: (13, 11, 21, 6, 2, (2, 10, 2)),
    3: (14, 16, 30, 7, 3, (3, 10, 3)), 4: (15, 22, 42, 8, 4, (4, 10, 4)),
}


def limiar(x, maior, grave):
    return 3 if x >= grave else 2 if x >= maior else 1


def rolar(n, lados):
    return [d(lados) for _ in range(n)]


class PJ:
    def __init__(s, M, magico):
        L = NIVEL[M]
        s.M, s.magico = M, magico
        s.attr = {1: 2, 2: 3, 3: 4, 4: 5}[M]
        s.esq = 9 + M
        s.pv = 6 + (M - 1)
        arm = {1: (2, 4), 2: (4, 8), 3: (6, 13), 4: (8, 17)}[M]
        s.maior, s.grave = 5 + L + arm[0], 11 + L + arm[1]
        s.prot = 4 + (M >= 3)
        s.arcano = 3
        s.caido = False

    def acao(s, alvo_dif):
        """devolve (dados, bonus, dif, dano_fn)"""
        M = s.M
        if s.magico:
            g = M + 1
            while g >= 2 and s.arcano < CUSTO[g]:
                g -= 1
            if g >= 2:
                s.arcano -= CUSTO[g]
                q, l = POOL[g]
                S = M + 1
                dano = lambda: sum(rolar(q - 1, 10)) + S + 1
                return rolar(q, l), S + 1, alvo_dif, dano
            return rolar(2, 12), s.attr, alvo_dif, lambda: d(8) + M  # Grau 1 no meio da ação
        prof = M
        return rolar(2, 12), s.attr, alvo_dif, lambda: sum(rolar(prof, 10)) + 3 + 2 * (M - 1)


class Adv:
    def __init__(s, M, papel, n_pj):
        dif, maior, grave, pv, atk, (q, l, b) = REGUA[M]
        s.nome = papel
        s.ataques = 1
        s.lacaio = 0
        mult = 1.0
        if papel == "Lacaio":
            dif -= 2; maior, grave = maior // 2, grave // 2; mult = 0.5; s.lacaio = n_pj; pv = n_pj
        elif papel == "Bruto":
            dif -= 1; maior, grave = int(maior * 1.3), int(grave * 1.3); pv += 3; mult = 1.4
        elif papel == "Líder":
            dif += 1; pv += 1
        elif papel == "Chefe":
            dif += 2; maior, grave = int(maior * 1.5), int(grave * 1.5); pv += 5; s.ataques = 2
        s.dif, s.maior, s.grave, s.pv, s.atk = dif, maior, grave, pv, atk
        s.bonus_dano = 0
        s.dano = lambda: int((sum(rolar(q, l)) + b) * mult) + s.bonus_dano
        s.pontos = PONTOS_P[papel]

    def melhorar(s, tipo, M):
        if tipo == "dano": s.bonus_dano += 2 + (M >= 3)
        elif tipo == "acerto": s.atk += 1
        elif tipo == "defesa": s.dif += 1
        elif tipo == "duro": s.maior += 2 + (M >= 3); s.grave += 4 + 2 * (M >= 3)
        elif tipo == "vida": s.pv = min(12, s.pv + 1)


PONTOS = {"Lacaio": 1, "Padrão": 2, "Líder": 3, "Bruto": 4, "Chefe": 5}
PONTOS_P = PONTOS


def montar_encontro(M, n, extra=0, chefe=False, base=None, melhorias=0):
    orc = (3 * n + 2 if base is None else base) + extra + {2: 2, 3: 1}.get(n, 0)  # P-112
    lista = []
    if chefe:
        lista.append("Chefe"); orc -= 5
    elif orc >= 7:
        lista.append("Líder"); orc -= 3
    while orc >= 4 and lista.count("Bruto") < 2:
        lista.append("Bruto"); orc -= 4
    while orc >= 2:
        lista.append("Padrão"); orc -= 2
    if orc >= 1:
        lista.append("Lacaio")
    advs = [Adv(M, p, n) for p in lista]
    tipos = ["acerto", "dano", "defesa", "duro", "vida"]
    for i in range(melhorias):  # 1 ponto = 1 melhoria para todos os adversários
        for a in advs:
            a.melhorar(tipos[i % len(tipos)], M)
    return advs


def combate(M, n, extra=0, chefe=False, base=None, melhorias=0, moral=False):
    pjs = [PJ(M, i % 2 == 1) for i in range(n)]
    advs = montar_encontro(M, n, extra, chefe, base, melhorias)
    pts_total = sum(a.pontos for a in advs)
    cat = 2 * n
    out = dict(acoes=0, pv=0, caidos=0, mestre=0)

    def ativar(a):
        for _ in range(a.ataques):
            vivos = [p for p in pjs if not p.caido]
            if not vivos:
                return
            p = R.choice(vivos)
            if d(20) + a.atk >= p.esq:
                m = limiar(a.dano(), p.maior, p.grave)
                if m >= 2 and p.prot:
                    p.prot -= 1; m -= 1
                p.pv -= m; out["pv"] += m
                if p.pv <= 0 and not p.caido:
                    p.caido = True; out["caidos"] += 1

    def mestre():
        nonlocal cat
        out["mestre"] += 1
        vivos = [a for a in advs if a.pv > 0]
        R.shuffle(vivos)
        for i, a in enumerate(vivos):
            if i == 0:
                ativar(a)
            elif cat >= 1:
                cat -= 1; ativar(a)

    fila = seguidas = 0
    def acabou():
        if not any(a.pv > 0 for a in advs):
            return True
        if moral:  # o resto foge ou se rende: metade dos pontos caiu
            caidos = sum(a.pontos for a in advs if a.pv <= 0)
            return caidos * 2 >= pts_total
        return False

    while not acabou() and any(not p.caido for p in pjs) and out["acoes"] < 400:
        vivos = [p for p in pjs if not p.caido]
        pj = vivos[fila % len(vivos)]; fila += 1
        alvo = min((a for a in advs if a.pv > 0), key=lambda a: a.pv)
        dados, bonus, dif, dano = pj.acao(alvo.dif)
        viv = 0
        if pj.arcano >= 3 and R.random() < 0.5:
            pj.arcano -= 1; viv = 2
        out["acoes"] += 1
        pares = sum(1 for x in dados if x % 2 == 0)
        impares = len(dados) - pares
        crit = (len(dados) == 2 and dados[0] == dados[1]) or (len(dados) > 2 and impares == 0)
        pj.arcano = min(12, pj.arcano + (min(max(dados), 6) if crit else pares))
        cat = min(20, cat + (0 if crit else impares))
        ok = crit or sum(dados) + bonus + viv >= dif
        if ok:
            x = dano() * (2 if crit else 1)
            m = limiar(x, alvo.maior, alvo.grave)
            alvo.pv -= m
        seguidas += 1
        por_cat = (max(dados) % 2 == 1) if len(dados) == 2 else impares > pares
        if (not crit and (not ok or por_cat)) or (seguidas >= 3 and cat >= 1):
            if ok and not por_cat and seguidas >= 3:
                cat -= 1
            seguidas = 0
            mestre()
    out["vitoria"] = any(not p.caido for p in pjs)
    out["tpk"] = all(p.caido for p in pjs)
    out["pv_total"] = sum(6 + (M - 1) for _ in range(n))
    return out


def resumo(M, n, extra=0, chefe=False, N=2000, **kw):
    res = [combate(M, n, extra, chefe, **kw) for _ in range(N)]
    m = lambda k: st.mean(r[k] for r in res)
    return (f"{100*m('vitoria'):.0f}%", f"{m('acoes')/n:.1f}", f"{100*m('pv')/res[0]['pv_total']:.0f}%",
            f"{m('caidos'):.1f}", f"{100*m('tpk'):.0f}%",
            f"{m('acoes') * 2.5 + m('mestre') * 1.5:.0f} min")


def social(n_jog, attr_bom, dif, precisa=3, ameaca=6, ajudas=0, N=20000):
    """Evento social: o grupo precisa de `precisa` sucessos antes de o mestre
    juntar `ameaca` de Cataclisma na Ameaça (ele põe lá os ímpares que saem)."""
    vit = 0
    for _ in range(N):
        suc = pote = 0
        i = 0
        while suc < precisa and pote < ameaca:
            a, b = d(12), d(12)
            bonus = attr_bom if i % n_jog == 0 else attr_bom - 2
            bonus += sum(R.choice([2, d(6)]) for _ in range(ajudas))
            i += 1
            if a == b or a + b + bonus >= dif:
                suc += 1
            pote += (a % 2) + (b % 2) if a != b else 0
        vit += suc >= precisa
    return f"{100*vit/N:.0f}%"


def main():
    print("## Combate: vitória · ações por jogador · PV perdidos · caídos · todos caídos · tempo\n")
    print("| Moldura | jogadores | encontro | vitória | ações/jogador | PV perdidos | caídos | todos caem | tempo |")
    print("|---|---|---|---|---|---|---|---|---|")
    for M in (1, 2, 3, 4):
        for n in (4, 3, 2):
            for nome, kw in (("escaramuça", {"base": 2 * n, "moral": True}), ("combate", {"moral": True}),
                             ("combate difícil (+2 melhorias)", {"moral": True, "melhorias": 2}),
                             ("batalha", {}), ("chefe", {"chefe": True})):
                r = resumo(M, n, **kw)
                print(f"| {M} | {n} | {nome} | " + " | ".join(r) + " |")
    print("\n## Evento social com Ameaça (3 sucessos antes de a Ameaça de 6 encher com os ímpares)\n")
    print("| jogadores | atributo do melhor | Dificuldade | sem ajuda | com 1 ajuda | com 2 ajudas |")
    print("|---|---|---|---|---|---|")
    for n in (4, 2):
        for attr in (2, 4):
            for dif in (12, 15, 18):
                print(f"| {n} | +{attr} | {dif} | {social(n,attr,dif)} | {social(n,attr,dif,ajudas=1)} | {social(n,attr,dif,ajudas=2)} |")


if __name__ == "__main__":
    main()
