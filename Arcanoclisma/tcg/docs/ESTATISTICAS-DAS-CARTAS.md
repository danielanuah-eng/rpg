# Estatísticas das cartas (régua de balanceamento, rascunho 0.1)

Sem compromisso com arte nem com nomes: é só a régua para comparar cartas e criar novas. Está em código em [balanco.ts](../src/balanco.ts), e as cartas básicas em [CARTAS-BASE.md](CARTAS-BASE.md).

Tudo aqui é **proposta minha** a partir das suas regras; os preços foram escolhidos para dar uma base coerente, não foram testados em jogo real. Ajuste à vontade.

## 1. A ideia: Pontos de Poder (PP)

Cada carta tem um **orçamento** de PP, que cresce com o custo:

**Orçamento = 2 × custo + 2**, menos 2 por cada carta de Arcano que ela gera **acima de 1** ao ser enviada como pitch (nunca abaixo de 0).

Cada coisa que a carta faz tem um preço. A soma dos preços não pode passar do orçamento. É a mesma ideia do "vanilla test" do Magic.

| Item | Preço (PP) |
|---|---|
| 1 ponto de Ataque | 1 |
| 1 ponto de Vida | 1 |
| 1 de dano a um alvo (efeito) | 2 |
| 1 de dano só no Conjurador inimigo (efeito) | 1,5 |
| 1 de cura | 1 |
| Destruir um alvo | 7 |
| Comprar 1 carta | 3 |
| Ganhar 1 Arcano | 2 |
| Dar 1 Arcano ao oponente | −2 (abate) |
| Velocidade repentina | +2 |
| Palavra-chave **Proficiente** | +2 |
| Descartar 1 carta do Grau (custo da carta) | libera +3 |
| Grau acima do padrão | libera +1 por ponto |
| Grau abaixo do padrão | cobra 1 por ponto |
| Exigir uma carta de um Valor no Grau | libera +1 |

## 2. Custo, Grau e o ritmo do jogo

- **Grau padrão = custo − 1** (mínimo 0). Como o jogador põe 1 carta no Grau por turno e costuma enviar 1 carta como pitch, uma carta de custo C fica jogável por volta do **turno C − 1 do dono**.
- Com o jogo durando cerca de 20 turnos (10 de cada jogador), o Grau útil vai até uns **8 a 10**. Grau 11 e 12 só aparecem em jogo muito longo.
- Fases do jogo, em turnos do dono: **early** = 1 a 3, **mid** = 4 a 6, **late** = 7 ou mais. Early: custos 0 a 3. Mid: 3 a 6. Late: 6 a 10.
- Arcano disponível no seu turno ≈ número de cartas no Grau + cartas enviadas como pitch. Por isso o custo médio das cartas boas costuma ficar um pouco acima do Grau.

## 3. Aliados: divisão de Ataque e Vida

Corpo = orçamento menos palavras-chave. Vida = 55% do corpo no perfil **equilibrado**, 40% no **agressivo**, 70% no **defensivo** (arredondado para cima). O resto é Ataque. A Vida pesa mais que em outros TCGs porque o **dano fica guardado** entre os turnos.

| Custo | Corpo | Equilibrado | Agressivo | Defensivo |
|---|---|---|---|---|
| 1 | 4 | 1/3 | 2/2 | 1/3 |
| 2 | 6 | 2/4 | 3/3 | 1/5 |
| 3 | 8 | 3/5 | 4/4 | 2/6 |
| 4 | 10 | 4/6 | 6/4 | 3/7 |
| 6 | 14 | 6/8 | 8/6 | 4/10 |
| 8 | 18 | 8/10 | 10/8 | 5/13 |

(Ataque/Vida.) Proficiente tira 2 do corpo.

## 4. Temporárias

| Tipo | Custo 1 | Custo 2 | Custo 3 | Custo 4 |
|---|---|---|---|---|
| Dano a alvo, findante | 2 | 3 | 4 | 5 |
| Dano a alvo, repentina | 1 | 2 | 3 | 4 |
| Dano ao Conjurador, findante | | | 5 | (custo 5: 8) |
| Comprar, findante | 1 | | 2 | |
| Cura, findante | | 6 | | 10 |

## 5. Místicas e Estruturas

- Mística básica: só Vida, igual ao orçamento (custo 2: Vida 6; custo 4: Vida 10). Ainda não existe efeito passivo no motor.
- Estrutura básica: ao entrar gera Arcano (e compra 1 carta a partir do custo 3), e o resto vira Vida. Estruturas só são atingidas por magias.
- **Recurso**: carta de custo 0 com 3 de pitch e nenhum efeito, para quem só quer gerar Arcano.

## 6. Como usar

1. Para criar uma carta: escolha tipo e custo, some os preços e confira com `avaliar()`.
2. Se ela passa do orçamento, tire efeito ou suba o custo, o Grau ou o descarte de Grau.
3. Os Valores **ajustam** a régua: por exemplo, Conflitüo (alto dano, early forte) pode gastar mais em Ataque; Cognitium (compra 4) paga caro pelo draw. Isso sai da [tabela de Valores](TABELA-DE-VALORES.md) na próxima etapa.

## 6b. Novidades da rodada 2 (opcionais, em teste)

- **Habilidade ativada** (Estruturas e Aliados de recurso): suspende e paga custo em Arcano. Preço: 1,5 × o efeito se só gera Arcano, 2 × o efeito nos demais, menos 1,5 por Arcano de custo. Exemplo: Estrutura de custo 1 que gera 1 Arcano por turno e tem 1 de Vida.
- **Burn anulável** (`pagarParaAnular`): depois que a carta é jogada, o oponente pode pagar X de Arcano (Grau levantado, reserva ou cartas enviadas) para ela não resolver.
- **Valor Eternyr**: comprar carta custa 2,5 PP (em vez de 3), logo mais cartas pelo mesmo custo.
- **Cura**: custa 0,6 PP por ponto e leva um efeito extra (Arcano ou comprar) que cresce com o custo.
- Resultados em [COMPARACAO-VARIANTES.md](COMPARACAO-VARIANTES.md) e [DIAGNOSTICO-FINAL.md](DIAGNOSTICO-FINAL.md).

## 7. O que ainda não tem preço

Efeitos passivos de Místicas e Estruturas, ativação por suspensão, tokens, equipamentos, anulação de dano, Extras (dois Valores), efeitos de topo do baralho, olhar a mão, descarte do oponente e custo extra para o oponente. Cada um entra quando o motor suportar o efeito.

## 8. Testes

`test/balanco.test.ts` confere que toda carta básica está dentro do orçamento (no máximo 2 PP de folga), que os números ficam entre 0 e 20 e que 40 partidas com jogadores aleatórios não quebram o motor. Na última execução, 39 das 40 terminaram, com mediana de 34 turnos. Jogadores aleatórios não são jogadores reais, então isso não calibra a duração. Só mostra que o motor aguenta.
