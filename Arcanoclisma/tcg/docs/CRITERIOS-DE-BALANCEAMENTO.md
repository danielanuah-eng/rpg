# Critérios de balanceamento (rodada 4)

Parâmetros do autor para dizer que o jogo está perto de um bom resultado com decks e estratégias boas ou óbvias, desconsiderando a habilidade do jogador. Em código: [`sim/criterios.json`](../sim/criterios.json). Medidos por `node sim/avaliar.ts --variante <id>`.

**Vida do Conjurador: 20** (decisão do autor; padrão das simulações a partir desta rodada).

## Regras gerais

- Nenhuma montagem passa de **59%** de vitória média. Confrontos podem ser desiguais.
- Toda montagem fica jogável: pelo menos **38%** de média. Esse piso é proposta minha; a Mistura fica de fora.
- Partidas por volta de 20 turnos totais: média entre **16 e 26**.
- No máximo **5%** de partidas travadas.
- Quem começa vence entre **45% e 55%**.

## O papel de cada estratégia

As definições seguem o uso consagrado no Magic e o que você descreveu.

| Estratégia | O que ela faz | Como é medida |
|---|---|---|
| **Aggro** | Tenta acabar rápido: começa a bater cedo, gasta a mão e o baralho mais rápido. Os bons recursos cansam e custam a voltar quando é controlado, mas não é impossível. Pode explodir às vezes, sem ser óbvio. | Primeiro dano até o turno global 4 (mediana); até 2,5 cartas na mão ao fim do 4º turno; no máximo 40% contra o Controle em partidas longas (18+ turnos); pelo menos 30% contra o Controle no geral. |
| **Burn** | Causa dano em Aliados ou no jogador com constância a cada turno, mesmo que pouco. | Dano em pelo menos 75% dos próprios turnos (a partir do 2º). |
| **Controle** | Tem recursos para controlar o Aggro, evitar dano, acelerar o late game e gerenciar recursos; troca perdendo menos que o oponente; remoções mais fortes com o passar do jogo. Jogar de Controle é mais difícil. | Pelo menos 35% contra o Aggro; vence 55% ou mais das partidas longas (18+ turnos); perde no máximo tantos Aliados quanto o oponente em partidas de 14+ turnos. |
| **Midrange** | Early game para pressionar o Controle e controle para segurar o Aggro. | Pelo menos 50% contra o Controle e 50% contra o Aggro. |
| **Tempo** | Híbrido aggro-controle: ameaças baratas e interação barata (remoção, anulação) para manter o oponente atrás. | Pelo menos 50% contra o Controle; pelo menos 1 carta repentina por partida no turno do oponente. |
| **Stall** | Defende, mas precisa de uma forma de ganhar dando dano. Costuma ser pior contra o Aggro; se o controla, ganha. | 75% das vitórias por dano (não por impasse); pelo menos 30% contra o Aggro; 60% ou mais contra o Aggro nas partidas longas (16+ turnos). |
| **Combo** | Mais lento, interage pouco, fica muito forte se monta. O counter é o Controle. | Ainda não existe montagem de Combo. Critérios opcionais: no máximo 45% contra o Controle; 60% ou mais nas partidas longas. |

Não é medido: saber sequenciar ou distribuir o dano. O jogador automático não tem essa habilidade.

## Fontes das definições

- [Magic: The Gathering deck types (Wikipedia)](https://en.wikipedia.org/wiki/Magic:_The_Gathering_deck_types)
- [Aggro, Control, Combo, and Midrange: A Guide To Magic's Biggest Archetypes (CBR)](https://www.cbr.com/mtg-guide-to-magics-biggest-archetypes/)
- [The Ultimate Guide to Tempo Decks in Magic: The Gathering (Draftsim)](https://draftsim.com/mtg-tempo-deck/)
- [What Is A Tempo Deck In MTG? (TheGamer)](https://www.thegamer.com/magic-the-gathering-mtg-tempo-deck-guide-explanation/)

## Fluxo dos agentes

1. **agente-de-testes** (`.claude/agents/agente-de-testes.md`) busca objetivos usando só cartas existentes e o estilo de jogo. Se esgotar sem alcançar, relata e pede para acionar o balanceador.
2. **agente-balanceador** (`.claude/agents/agente-balanceador.md`) pode criar e mudar cartas, efeitos, mecânicas e variáveis, sem mudar a estrutura do jogo nem complicar o físico. Trabalha na variante `BAL`. Quando `avaliar` responde **PRÓXIMO**, para, escreve o diagnóstico e pede permissão a você.
