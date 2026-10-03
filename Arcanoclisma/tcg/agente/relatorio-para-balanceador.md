# Relatório para o agente-balanceador

O agente de testes testou 3032 combinações de cartas existentes e estilos de jogo na variante P e **não alcançou** os objetivos.

## Melhorar o Controle a sobreviver contra Aggro e Burn

- Melhor medida (amostra de confirmação): Controle: Aggro 70.5% (sobrevive 30.4 turnos) · Burn 88.0% (sobrevive 16.0 turnos) · outras 49.4% · travadas 4.0%
- Falhas: vitória contra Aggro: 70.5% > 70%; vitória contra Burn: 88.0% > 70%
- Melhor baralho: 4× base-curar-c1, 4× base-estrutura-c1, 3× base-aliado-c4-agressivo, 3× base-curar-c3, 3× base-curar-c4, 3× base-destruir-c4-repentina, 3× base-estrutura-c3, 3× eternyr-comprar-c1, 2× base-aliado-c3-equilibrado, 2× base-aliado-c4-equilibrado, 2× base-aliado-c6-equilibrado-descarta-grau, 2× base-aliado-recurso-c1, 2× base-comprar-c1, 2× base-comprar-c3, 2× base-dano-conjurador-c1, 2× base-dano-conjurador-c2, 2× base-estrutura-c3-enfermaria, 2× base-recurso, 1× base-aliado-c1-equilibrado, 1× base-aliado-c2-agressivo, 1× base-aliado-c2-agressivo-proficiente, 1× base-aliado-c2-equilibrado, 1× base-aliado-c4-agressivo-proficiente, 1× base-aliado-c4-defensivo, 1× base-aliado-c6-defensivo, 1× base-aliado-recurso-c3, 1× base-comprar-c2-repentina, 1× base-curar-c2, 1× base-curar-c6, 1× base-dano-c3, 1× base-estrutura-c6-nucleo, 1× base-mistica-c2
- Estilo: {"nome":"Controle","agressividade":0.6,"reservaInstantes":false,"grauAlvo":11,"queimaFace":false}

Próximo passo: acionar o agente-balanceador (`.claude/agents/agente-balanceador.md`).
