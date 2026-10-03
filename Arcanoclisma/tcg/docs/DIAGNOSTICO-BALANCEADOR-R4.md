# Diagnóstico do agente-balanceador (rodada 4, Vida 20)

> **Decisão do autor (rodada 5):** aprovado como padrão, **exceto** a Cataclisma crescente, o Grau suspenso de quem começa e a cota de Aliados no Controle. Entrou a regra de entrada do Aliado (não ataca nem usa efeito de suspender ao entrar, salvo Proficiente). Situação atual e próximos passos: seção "Rodada 5" de [MUDANCAS-DO-BALANCEADOR.md](MUDANCAS-DO-BALANCEADOR.md) e [AVALIACAO-PADRAO.md](AVALIACAO-PADRAO.md).

**Situação: os 20 critérios obrigatórios foram atingidos**, confirmados com 100 partidas por confronto (8.100 partidas) na variante **`BAL`** (= BAL12c). Relatório: [AVALIACAO-BAL12c-confirmacao.md](AVALIACAO-BAL12c-confirmacao.md). Histórico passo a passo: [MUDANCAS-DO-BALANCEADOR.md](MUDANCAS-DO-BALANCEADOR.md).

**Nada disso virou padrão do jogo.** Está tudo na variante `BAL`, esperando a sua permissão.

## 1. Resultado

| Critério | Antes (P, Vida 20) | Agora (BAL) | Meta |
|---|---|---|---|
| Critérios atingidos | 11 de 20 | **20 de 20** | — |
| Vitória média por montagem | 34% a 68% | **39% a 58%** | 38% a 59% |
| Duração média | 31,9 turnos | **20,8** | 16 a 26 |
| Partidas travadas | 2,2% | **0%** | até 5% |
| Quem começa vence | 45,1% | **50,2%** | 45% a 55% |
| Controle contra Aggro | 22,5% | **53,0%** | ≥ 35% |
| Controle em partidas longas | 62,6% | **63,2%** | ≥ 55% |
| Aliados perdidos pelo Controle / oponente | 0,43 | **0,95** | ≤ 1 |
| Burn dá dano nos próprios turnos | 83% | **83,8%** | ≥ 75% |
| Midrange contra Controle / contra Aggro | 65% / 75% | **52,5% / 84%** | ≥ 50% / ≥ 50% |
| Tempo contra Controle | 36,3% | **70,0%** | ≥ 50% |
| Stall vence por dano (não por impasse) | 41,5% | **98,9%** | ≥ 75% |
| Stall contra Aggro / em partidas longas contra Aggro | 20% / 23% | **62,5% / 80,6%** | ≥ 30% / ≥ 60% |

Vitória média: Aggro 48,6%, Midrange 58,1%, Controle 48,9%, Burn 39,3%, Stall 54,8%, Ramp 52,1%, Tempo 57,0%, Mistura 45,3%, Eternyr 45,9%.

## 2. O que mudou

### Regras (mudam o jogo físico)
1. **Cataclisma crescente**: a partir do 10º turno de cada jogador, o Cataclisma causa ao Conjurador do turno 1 de dano, depois 2, 3... É o que leva as partidas para ~20 turnos e acaba com o travamento. **Exige marcar o turno de cada jogador** (um dado ou contador).
2. **Quem começa pode pôr Grau no turno 1, mas a carta entra suspensa** (substitui "não pode pôr Grau no turno 1").
3. **O segundo jogador compra 1 carta a mais** na preparação (sem Arcano extra).
4. **Sai a regra "o Conjurador de quem começa não ataca no turno 1"** (não fazia diferença e complicava).
5. Continuam: relógio de impasse (5), Conjurador ataca sem custo e pôr Grau não o suspende, Vida 20.

### Palavras-chave e efeitos novos (uma linha cada)
- **Etéreo**: só é bloqueado por Aliados Etéreos (Tempo).
- **Devolver** um Aliado ou Mística à mão (Tempo, repentina, custo 2).
- **Anular** uma carta na pilha (Eternyr e Controle, repentina, custo 3).
- **Varredura**: dano em cada Aliado do oponente (custo 3: 2 de dano; custo 5: 3).
- **Remoção que cresce**: "1 + o número de cartas no seu Grau" de dano em um Aliado (custo 3).
- **Estrutura de cerco** (Stall): suspenda para causar 1 de dano ao Conjurador inimigo.

### Ajustes de carta
- Estrutura de cura passa a curar **só Aliados** (não o Conjurador). Com a cura do Conjurador, o Stall dominava.
- Aliados que descartam Grau descartam **1 carta a mais** (Ramp estava em 65%).

### Baralhos e estilos (só simulação)
- Aggro com curva mais baixa; Tempo com Etéreos e Devolver; Stall com varreduras, remoção que cresce e cerco.
- Eternyr, Ramp e Controle ajustados pelo agente de testes (`sim/baralhos-bal11.json`). O Controle ficou com no máximo 14 Aliados (remoção, cura, varredura).

## 3. Riscos e ressalvas

- **Tudo é medido com o jogador automático**, que não blefa, não sequencia bem e não lê o oponente. Precisa de teste com pessoas.
- **Alguns confrontos são muito desiguais**: Aggro vence o Burn 96,5%, Midrange vence o Aggro 84%. Os critérios permitem, mas vale olhar.
- **Burn ficou em 39,3%**, perto do piso de 38%.
- Os baralhos ajustados pelo agente misturam cartas de Valores diferentes (as simulações ainda não aplicam a regra de Valores do Conjurador).
- **Combo ainda não existe** (os três critérios de Combo são opcionais).
- A Cataclisma crescente acrescenta um contador. Alternativa sem contador: começar quando o Grau do jogador chegar a 8 (não testada).

## 4. O que preciso de você

**PEDIR PERMISSÃO AO AUTOR: aplicar as mudanças da variante BAL como padrão?**

Se sim, eu passo as regras para o livro e o motor, e as cartas e baralhos para os arquivos-base. Se quiser só uma parte (por exemplo, as cartas sem a Cataclisma crescente), diga quais.
