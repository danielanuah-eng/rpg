"""Simula a economia de Arcano de um jogador ao longo de uma sessão.

Uma sessão = 3 Eventos × 8 rolagens. O jogador gasta de um jeito razoável:
Vivência (Vantagem, +1d6) quando ajuda e dá, palavra extra em metade das magias,
Hábito uma vez por Evento, ajudar um aliado de vez em quando, Forçar o Grau
uma vez por sessão. Compara regras de ganho e de custo.
Roda: python3 simular_economia.py
"""
import random
import statistics as st

R = random.Random(1)
MAX = 12


def rolar(ganho):
    a, b = R.randint(1, 12), R.randint(1, 12)
    pares = (a % 2 == 0) + (b % 2 == 0)
    return a, b, ganho(a, b, pares)


GANHOS = {
    "atual: +1 por dado par": lambda a, b, p: min(a, 6) if a == b else p,
    "Par +2, Equilíbrio +1, Ímpar 0 (= atual)": lambda a, b, p: min(a, 6) if a == b else p,
    "só com Par: +1 (crítico +2)": lambda a, b, p: 2 if a == b else (1 if p == 2 else 0),
    "+1 se tiver algum par (crítico +2)": lambda a, b, p: 2 if a == b else (1 if p >= 1 else 0),
    "+1 por dado par, só na falha ou no Par": None,  # tratado à parte
}


def sessao(ganho_fn, custo_viv=1, custo_palavra=1, custo_habito=3, so_falha_ou_par=False, reserva=2, custo_grau2=0):
    arcano = 3
    st_ = dict(ganho=0, gasto=0, perdido=0, viv=0, rolagens=0, sucessos=0, habitos=0, palavras=0, magias=0)
    for evento in range(3):
        usou_habito = False
        for _ in range(8):
            st_["rolagens"] += 1
            magia = R.random() < 0.4
            bonus = 2 + (1 if magia else 0)  # atributo +2, ou Sintonia 2 + Relação 1
            dif = R.choice([12, 12, 13, 14, 15])
            if magia and arcano - custo_grau2 < 0:
                magia = False  # sem Arcano: Grau 1 no meio de uma ação
                bonus = 2
            if magia:
                arcano -= custo_grau2; st_["gasto"] += custo_grau2
                st_["magias"] += 1
                if arcano - custo_palavra >= reserva and R.random() < 0.5:
                    arcano -= custo_palavra; st_["gasto"] += custo_palavra; st_["palavras"] += 1
            if not usou_habito and arcano >= custo_habito + reserva and R.random() < 0.5:
                arcano -= custo_habito; st_["gasto"] += custo_habito; st_["habitos"] += 1; usou_habito = True
            viv = 0
            if R.random() < 0.6 and arcano - custo_viv >= reserva:
                arcano -= custo_viv; st_["gasto"] += custo_viv; st_["viv"] += 1; viv = R.randint(1, 6)  # Vivência = Vantagem
            if R.random() < 0.15 and arcano - 1 >= reserva + 1:  # ajudar alguém
                arcano -= 1; st_["gasto"] += 1
            a, b = R.randint(1, 12), R.randint(1, 12)
            pares = (a % 2 == 0) + (b % 2 == 0)
            sucesso = a == b or a + b + bonus + viv >= dif
            st_["sucessos"] += sucesso
            g = ganho_fn(a, b, pares)
            if so_falha_ou_par and sucesso and pares < 2 and a != b:
                g = 0
            st_["ganho"] += g
            arcano += g
            if arcano > MAX:
                st_["perdido"] += arcano - MAX
                arcano = MAX
    st_["final"] = arcano
    return st_


def rodar(nome, n=4000, **kw):
    fn = kw.pop("fn")
    res = [sessao(fn, **kw) for _ in range(n)]
    m = lambda k: st.mean(r[k] for r in res)
    rol = m("rolagens")
    return (f"| {nome} | {m('ganho')/rol:.2f} | {100*m('viv')/rol:.0f}% | {m('habitos'):.1f} | "
            f"{100*m('palavras')/m('magias'):.0f}% | {m('perdido'):.1f} | {m('final'):.1f} | {100*m('sucessos')/rol:.0f}% |")


def main():
    print("## Economia de Arcano por jogador, uma sessão (24 rolagens)\n")
    print("| regra | Arcano ganho por rolagem | rolagens com Vivência | Hábitos por sessão | magias com palavra extra | Arcano perdido no máximo | Arcano no fim | sucesso |")
    print("|---|---|---|---|---|---|---|---|")
    atual = lambda a, b, p: min(a, 6) if a == b else p
    print(rodar("A · atual (+1 por par; custos 1/1/3)", fn=atual))
    print(rodar("B · atual, custos 2/1/4 (Vivência 2, Hábito 4)", fn=atual, custo_viv=2, custo_habito=4))
    print(rodar("C · só Par dá Arcano (+1; crítico +2)", fn=lambda a, b, p: 2 if a == b else (1 if p == 2 else 0)))
    print(rodar("D · +1 se tiver algum par (crítico +2)", fn=lambda a, b, p: 2 if a == b else (1 if p >= 1 else 0)))
    print(rodar("E · +1 por par, mas sucesso sem Par não dá", fn=atual, so_falha_ou_par=True))
    print(rodar("F · E + custos 2/1/4", fn=atual, so_falha_ou_par=True, custo_viv=2, custo_habito=4))
    print(rodar("H · atual, Grau 2 custa 1 (custos 1/1/3)", fn=atual, custo_grau2=1))
    print(rodar("I · atual, Grau 2 custa 1, Hábito 4", fn=atual, custo_grau2=1, custo_habito=4))
    print(rodar("J · atual, Grau 2 custa 1, Vivência 2, Hábito 4", fn=atual, custo_grau2=1, custo_viv=2, custo_habito=4))


if __name__ == "__main__":
    main()
