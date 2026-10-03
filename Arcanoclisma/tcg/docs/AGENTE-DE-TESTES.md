# Agente de testes

Roda simulações e ajusta montagens **até todos os objetivos da lista serem atingidos e confirmados**. Só para quando alcança (ou se você interromper, ou se usar `--limite`).

## Como começar

Na pasta `Arcanoclisma/tcg`:

```bash
npm run agente
```

ou, com limite de tentativas (código de saída 2 se não alcançar):

```bash
node sim/agente.ts --limite 1500
```

Também existe o agente do Claude Code `agente-de-testes` (`.claude/agents/agente-de-testes.md`): peça "agente-de-testes, começar". Ele usa só cartas existentes e o estilo de jogo. Se esgotar essas variáveis sem alcançar, escreve `agente/relatorio-para-balanceador.md` e pede para acionar o **agente-balanceador**, que pode mudar cartas, efeitos e variáveis (ver [CRITERIOS-DE-BALANCEAMENTO.md](CRITERIOS-DE-BALANCEAMENTO.md)).

## Os objetivos

Estão em [`sim/objetivos.json`](../sim/objetivos.json) e podem ser editados por você. Cada objetivo escolhe uma montagem e define metas. O atual:

- **Controle sobrevive a Aggro e Burn**: Controle vence pelo menos **40%** contra Aggro e **40%** contra Burn, e quando perde sobrevive em média pelo menos **12 turnos**.
- Restrições para não "resolver" quebrando o jogo: vitória média contra as outras montagens entre 30% e 62%; Aggro e Burn não podem ficar abaixo de 30% contra o Controle; no máximo 35% de partidas travadas.

Para mudar a meta, edite o arquivo (por exemplo `vitoriaMin`). Para adicionar outro objetivo, acrescente um item em `objetivos`.

## Como funciona

1. Mede o estado atual (partidas alternando quem começa) e calcula uma pontuação: 0 = tudo atingido.
2. Propõe uma mudança na montagem-alvo: troca uma ou mais cartas (até 4 cópias de cada; 10 do Recurso) ou muda o estilo do jogador automático (agressividade, segurar Arcano para repentinas, Grau desejado, modo lento).
3. Aceita se melhorar (ou, às vezes, se piorar pouco, para escapar de um mínimo local). Volta ao melhor ponto de tempos em tempos.
4. Quando a pontuação chega a 0, **confirma** com uma amostra grande (`partidasConfirmacao`, 400 por confronto). Se a confirmação falhar, continua.
5. Ao confirmar, escreve `agente/resultado.md` e `agente/resultado.json` e termina.

O progresso fica em `agente/estado.json` (retoma sozinho se os objetivos não mudaram) e o histórico em `agente/diario.md`. Use `--novo` para recomeçar do zero.

## Limites

- Só combina cartas **que já existem** no catálogo da variante (`--variante`, padrão `P`, o pacote da rodada 3). Não inventa números de carta.
- A taxa de vitória depende do jogador automático. O resultado vale para ele, não para humanos.
- Uma meta pode ser impossível com as cartas atuais. Com `--limite`, ao chegar no limite ele escreve o relatório para o agente-balanceador.
- A partir da rodada 4 o Conjurador tem **Vida 20**; o resultado anterior (Vida 12) precisa ser refeito.
