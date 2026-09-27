"""Simula o Evento de combate do Teste de mesa 1 (Moldura 1, 4 jogadores).

Modelo simples, feito para achar números fora do lugar, não para jogar por
ninguém: os jogadores atacam o adversário mais ferido; o mestre ativa um
adversário de graça quando emoldura e gasta Cataclisma para ativar
mais enquanto tiver reserva. Roda: python3 simular_combate.py [nº de lutas]
"""
import random
import statistics as st
import sys

R = random.Random()


def d(n):
    return R.randint(1, n)


def limiar(dano, maior, grave):
    return 3 if dano >= grave else 2 if dano >= maior else 1


class PJ:
    def __init__(s, nome, esq, pv, maior, grave, prot, ataque):
        s.nome, s.esq, s.pv, s.maior, s.grave, s.prot = nome, esq, pv, maior, grave, prot
        s.ataque = ataque  # função(alvo) -> (bônus, dano_fn, dif_mod, magia)
        s.caido = False
        s.arcano = 3


class Adv:
    def __init__(s, nome, dif, pv, maior, grave, atk, dano, humano, lacaio=0):
        s.nome, s.dif, s.pv, s.maior, s.grave, s.atk, s.dano = nome, dif, pv, maior, grave, atk, dano
        s.humano, s.lacaio = humano, lacaio  # lacaio = quantos no bando


def montar():
    pjs = [
        # Tomé: Briguento, Conflitüo, machado Cruel (arma) d10+2 +1 Gosto de Sangue, Força +2
        PJ("Tomé", 10, 6, 8, 16, 4, lambda a: (2, lambda: d(10) + 3, 0, False)),
        # Iara: Curiosa, Sublimeyn, Foco; magia G2: 2d12 + Sintonia 2 + Relação 1; dano 1d10+2+1
        PJ("Iara", 10, 5, 6, 12, 3, lambda a: (3, lambda: d(10) + 3, 0, True)),
        # Nhanda: Enganadora, Persona, Voz; magia G2 com −2 de Dificuldade em pessoas
        PJ("Nhanda", 12, 5, 6, 12, 3, lambda a: (3, lambda: d(10) + 2, -2 if a.humano else 0, True)),
        # Oruã: Brutamontes, Primitivum, Corpo; lança d10+2, Força +2, armadura Pesada
        PJ("Oruã", 9, 7, 11, 20, 5, lambda a: (2, lambda: d(10) + 2, 0, False)),
    ]
    advs = [
        Adv("Domadora (Líder)", 13, 6, 8, 14, 1, lambda: d(10) + 2, True),
        Adv("Cão 1 (Padrão)", 12, 5, 8, 14, 1, lambda: d(10) + 2, False),
        Adv("Cão 2 (Padrão)", 12, 5, 8, 14, 1, lambda: d(10) + 2, False),
        Adv("Javali (Bruto)", 11, 8, 10, 18, 1, lambda: d(12) + 4, False),
        Adv("Bandidos (Lacaios)", 10, 1, 99, 99, 0, lambda: 4, True, lacaio=4),
    ]
    return pjs, advs


def lutar(log=None):
    pjs, advs = montar()
    cat = 8  # 2 por jogador
    stats = dict(acoes=0, arcano=0, cat_ganho=0, cat_gasto=0, transbordo=0,
                 pv_perdido=0, caidos=0, criticos=0, vezes_mestre=0)

    def ganhar_cat(n):
        nonlocal cat
        stats["cat_ganho"] += n
        cat += n
        if cat > 20:
            stats["transbordo"] += cat - 20
            cat = 20

    def ativar(adv):
        vivos = [p for p in pjs if not p.caido]
        if not vivos:
            return
        alvo = R.choice(vivos)
        if d(20) + adv.atk >= alvo.esq:
            marca = limiar(adv.dano(), alvo.maior, alvo.grave)
            if marca >= 2 and alvo.prot > 0:
                alvo.prot -= 1
                marca -= 1
            alvo.pv -= marca
            stats["pv_perdido"] += marca
            if log is not None:
                log.append(f"  {adv.nome} acerta {alvo.nome}: −{marca} PV (sobra {max(alvo.pv,0)})")
            if alvo.pv <= 0 and not alvo.caido:
                alvo.caido = True
                stats["caidos"] += 1
                if log is not None:
                    log.append(f"  {alvo.nome} cai (Apagar)")
        elif log is not None:
            log.append(f"  {adv.nome} erra {alvo.nome}")

    def vez_do_mestre():
        nonlocal cat
        stats["vezes_mestre"] += 1
        vivos = [a for a in advs if a.pv > 0]
        if not vivos:
            return
        R.shuffle(vivos)
        ativar(vivos[0])
        extra = 1
        while cat >= 3 and extra < len(vivos):  # guarda um pouco de reserva
            cat -= 1
            stats["cat_gasto"] += 1
            ativar(vivos[extra])
            extra += 1

    fila = 0
    while any(a.pv > 0 for a in advs) and any(not p.caido for p in pjs):
        vivos_pj = [p for p in pjs if not p.caido]
        pj = vivos_pj[fila % len(vivos_pj)]
        fila += 1
        alvo = min((a for a in advs if a.pv > 0), key=lambda a: (a.pv * (a.lacaio or 1)))
        bonus, dano_fn, dif_mod, magia = pj.ataque(alvo)
        dif = alvo.dif + dif_mod
        if magia:
            dif = max(dif, 12)  # Dificuldade do Grau 2
        a, b = d(12), d(12)
        stats["acoes"] += 1
        crit = a == b
        total = a + b + bonus
        pares = (a % 2 == 0) + (b % 2 == 0)
        if crit:
            stats["criticos"] += 1
            ganho = min(a, 6)
        else:
            ganho = pares
        pj.arcano += ganho
        stats["arcano"] += ganho
        if not crit:
            ganhar_cat(2 - pares)
        sucesso = crit or total >= dif
        if sucesso:
            dano = dano_fn() * (2 if crit else 1)
            if alvo.lacaio:
                mortos = 1 + max(0, dano - 1) // 4
                alvo.lacaio -= mortos
                if alvo.lacaio <= 0:
                    alvo.pv = 0
                txt = f"derruba {mortos} lacaio(s)"
            else:
                marca = limiar(dano, alvo.maior, alvo.grave)
                alvo.pv -= marca
                txt = f"{dano} de dano, −{marca} PV"
            if log is not None:
                log.append(f"{pj.nome} ({a},{b}{'+' if bonus>=0 else ''}{bonus}={total} vs {dif}) acerta {alvo.nome}: {txt}")
        elif log is not None:
            log.append(f"{pj.nome} ({a},{b}{'+' if bonus>=0 else ''}{bonus}={total} vs {dif}) erra {alvo.nome}")
        if not crit and (not sucesso or pares == 0):
            if log is not None:
                log.append("→ o mestre emoldura")
            vez_do_mestre()
    stats["vitoria"] = any(not p.caido for p in pjs)
    stats["cat_final"] = cat
    return stats


def main():
    n = int(sys.argv[1]) if len(sys.argv) > 1 else 5000
    res = [lutar() for _ in range(n)]

    def med(k):
        return st.mean(r[k] for r in res)

    print(f"## Simulação do combate de teste ({n} lutas)\n")
    print("| medida | média |")
    print("|---|---|")
    print(f"| vitória dos jogadores | {100*med('vitoria'):.0f}% |")
    print(f"| ações de jogador até acabar | {med('acoes'):.1f} |")
    print(f"| vezes que o mestre emoldurou | {med('vezes_mestre'):.1f} |")
    print(f"| PV perdidos pelo grupo (de 23) | {med('pv_perdido'):.1f} |")
    print(f"| jogadores que caíram | {med('caidos'):.2f} |")
    print(f"| Arcano ganho pelo grupo | {med('arcano'):.1f} |")
    print(f"| Cataclisma ganho pelo mestre | {med('cat_ganho'):.1f} |")
    print(f"| Cataclisma gasto ativando adversários | {med('cat_gasto'):.1f} |")
    print(f"| Cataclisma que transbordou | {med('transbordo'):.2f} |")
    print(f"| Cataclisma no fim do combate | {med('cat_final'):.1f} |")
    print(f"| críticos | {med('criticos'):.2f} |")
    acoes = sorted(r["acoes"] for r in res)
    print(f"\nAções de jogador: 10% das lutas acabam em {acoes[n//10]} ou menos, 90% em {acoes[9*n//10]} ou menos.")


if __name__ == "__main__":
    main()
