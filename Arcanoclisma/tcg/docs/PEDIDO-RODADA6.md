# Pedido do autor: rodada 6 (para a equipe de balanceamento)

Ponto de partida: variante **PADRAO** (`sim/lista.ts`), 16 de 22 critérios (`docs/AVALIACAO-PADRAO.md`). Critérios: `sim/criterios.json`.

## Regra confirmada pelo autor (já aplicada no motor e no PADRAO)

- **Quem começa não põe carta no Grau no turno 1.** Só por efeito de carta, e efeitos de pôr Grau não devem acontecer cedo (não crie cartas que ponham Grau nos primeiros turnos).

## Direção de design pedida

1. **Mais cartas de varredura para o Controle** (dano em todos os Aliados do oponente, com variações de custo e Grau). Referências do Magic: Pyroclasm, Anger of the Gods, Wrath of God, Supreme Verdict, Damnation, Sunfall.
2. **O Grau da carta tem que diferenciar bastante o poder.** Quanto maior o Grau e o custo, maior o "clock" (pressão de dano por turno) que a carta põe no oponente. Hoje o orçamento cresce em linha reta com o custo (`src/balanco.ts`: orçamento = 2 × custo + 2, Grau padrão = custo − 1); o autor quer uma curva que dê **bem mais poder** às cartas de Grau alto.
3. **Unidades de Grau médio** que, proporcionalmente ao Grau, **viram problema na mesa** (ameaças que pedem resposta).
4. **Unidades de Grau alto bem maiores** (bombas de late game).
5. **Quantidade de unidades (ir largo) deve perder no late game**, para varreduras e efeitos, ou na corrida contra unidades fortes e condições de vitória mais caras.
6. Pensar nas bombas e cartas de meio de jogo mais fortes do Magic. O autor gosta muito de **Tivit, Seller of Secrets**: Aliado caro, voador, difícil de remover (ward), que gera vantagem quando entra e quando causa dano de combate ao jogador. Outras referências: Baneslayer Angel, Grave Titan, Sun Titan, Wurmcoil Engine, Thragtusk, Siege Rhino.

## Limites (continuam valendo)

- Não mudar a estrutura do jogo nem deixar o físico mais complexo: palavras-chave curtas e efeitos de uma linha. Gatilhos simples ("ao entrar", "ao causar dano de combate ao Conjurador") são aceitáveis se forem de uma linha.
- **Não pôr custo nem penalidade em mecânicas que já existem** (exemplo recusado: Cataclisma crescente).
- **Não limitar quantidade de cartas de um tipo por baralho.** Deckbuilding livre.
- Não afrouxar critérios existentes. Pode **acrescentar** medidas que verifiquem este pedido (ex.: "no late game, unidades de Grau alto decidem a partida"), explicando.
- Não reintroduzir o que já foi recusado (seção 12 de `docs/LIVRO-DE-REGRAS.md`).
- Sem commit, sem push, sem artes.
