# Relatório para o agente-balanceador

O agente de testes testou 3010 combinações de cartas existentes e estilos de jogo na variante BAL e **não alcançou** os objetivos.

## Controle: segura o Aggro e o Burn com as varreduras novas, troca bem e vence as partidas longas

- Melhor medida (amostra de confirmação): Controle: Aggro 38.3% (sobrevive 12.8 turnos) · Burn 35.4% (sobrevive 12.9 turnos) · Midrange 48.8% (sobrevive 21.4 turnos) · Tempo 29.6% (sobrevive 18.9 turnos) · outras 47.8% · travadas 0.4%
- Falhas: vitória contra Midrange: 48.8% > 48%; vitória contra Tempo: 29.6% < 30%
- Melhor baralho: 8× base-recurso, 4× base-curar-c3, 3× bal-remocao-grau-c3, 3× base-comprar-c3, 2× base-curar-c6, 2× base-curar-c8, 2× base-destruir-c3, 2× eternyr-aliado-c6-g8, 2× eternyr-comprar-c4, 2× eternyr-dano-aliado-c5, 2× eternyr-dano-aliado-c6, 2× r6-anjo-c6, 2× r6-furia-c3, 1× bal-estrutura-cerco-c3, 1× bal-varredura-c5, 1× base-aliado-c2-defensivo, 1× base-aliado-c4-equilibrado-grau5, 1× base-aliado-c6-defensivo, 1× base-aliado-c6-equilibrado-descarta-grau, 1× base-aliado-recurso-c5, 1× base-comprar-c2-repentina, 1× base-curar-c2, 1× base-curar-c4, 1× base-dano-c2-repentina, 1× base-dano-conjurador-c3, 1× base-destruir-c4-repentina, 1× base-estrutura-c6-nucleo, 1× base-estrutura-c8-reator, 1× eternyr-aliado-c8-g10, 1× eternyr-comprar-c1, 1× eternyr-dano-aliado-c4, 1× eternyr-dano-jogador-c5, 1× r6-colosso-c7, 1× r6-espiao-c4, 1× r6-ira-c4, 1× r6-presa-trovao-c5, 1× r6-varredura-c2
- Estilo: {"nome":"Controle","agressividade":0.6,"reservaInstantes":true,"grauAlvo":8,"queimaFace":false}

## Tempo jogável: ameaça barata e interação barata, pressiona o Controle

- Melhor medida (amostra de confirmação): Tempo: Controle 68.8% (sobrevive 27.7 turnos) · Midrange 44.2% (sobrevive 20.7 turnos) · Aggro 47.5% (sobrevive 15.0 turnos) · outras 47.5% · travadas 0.0%
- Falhas: nenhuma
- Melhor baralho: 11× base-recurso, 4× base-dano-c2-repentina, 4× base-destruir-c4-repentina, 3× bal-etereo-c3, 3× base-aliado-c5-equilibrado, 3× base-comprar-c1, 3× base-estrutura-c3, 2× bal-devolver-c2-repentina, 2× base-aliado-c2-agressivo-proficiente, 2× base-aliado-c2-equilibrado, 2× base-comprar-c2-repentina, 2× base-dano-conjurador-c3, 2× r6-piromante-c4, 1× bal-estrutura-cerco-c3, 1× bal-etereo-c2, 1× bal-remocao-grau-c3, 1× base-aliado-c4-agressivo-proficiente, 1× base-aliado-c6-equilibrado-descarta-grau, 1× base-aliado-c7-equilibrado, 1× base-aliado-recurso-c1, 1× base-curar-c3, 1× base-dano-c1-repentina, 1× base-dano-c2, 1× base-destruir-c3, 1× base-mistica-c4, 1× eternyr-comprar-c4, 1× eternyr-dano-aliado-c5, 1× r6-colosso-c7, 1× r6-espiao-c4, 1× r6-varredura-c2
- Estilo: {"nome":"Tempo","agressividade":0.8,"reservaInstantes":true,"grauAlvo":6,"queimaFace":false}

## Aggro abaixo do teto: rápido, mas controlável pelo Controle, Midrange e Stall

- Melhor medida (amostra de confirmação): Aggro: Controle 59.6% (sobrevive 22.9 turnos) · Midrange 37.9% (sobrevive 19.6 turnos) · Stall 28.3% (sobrevive 32.7 turnos) · outras 53.5% · travadas 0.2%
- Falhas: nenhuma
- Melhor baralho: 6× base-recurso, 4× base-aliado-c2-equilibrado, 4× base-aliado-c4-agressivo, 3× base-aliado-c4-agressivo-proficiente, 3× base-dano-c2, 3× base-dano-conjurador-c3, 2× bal-varredura-c3, 2× base-aliado-c1-equilibrado, 2× base-aliado-c2-defensivo, 2× base-aliado-c3-equilibrado, 2× base-mistica-c2, 2× eternyr-comprar-c3, 2× eternyr-dano-aliado-c5, 1× bal-etereo-c1, 1× bal-etereo-c3, 1× base-aliado-c2-agressivo, 1× base-aliado-c5-equilibrado, 1× base-aliado-c6-equilibrado-descarta-grau, 1× base-aliado-c8-equilibrado, 1× base-aliado-recurso-c1, 1× base-aliado-recurso-c5, 1× base-aliado-recurso-c7, 1× base-comprar-c3, 1× base-curar-c6, 1× base-dano-c1-repentina, 1× base-dano-c3, 1× base-dano-c4, 1× base-dano-conjurador-c1, 1× base-dano-conjurador-c4, 1× base-dano-conjurador-c5, 1× base-destruir-c4-repentina, 1× base-estrutura-c1, 1× base-estrutura-c4-fonte, 1× eternyr-aliado-c8-g10, 1× eternyr-destruir-c6-g8, 1× r6-furia-c3
- Estilo: {"nome":"Aggro","agressividade":0.95,"reservaInstantes":false,"grauAlvo":4,"queimaFace":false}

## Ramp forte no late game com as bombas, mas abaixo do teto

- Melhor medida (amostra de confirmação): Ramp: Stall 47.1% (sobrevive 40.3 turnos) · Midrange 65.8% (sobrevive 21.9 turnos) · outras 48.3% · travadas 0.2%
- Falhas: nenhuma
- Melhor baralho: 10× base-recurso, 4× base-aliado-c8-equilibrado-descarta-grau, 3× base-aliado-c2-defensivo, 3× base-aliado-c5-equilibrado, 3× base-comprar-c3, 3× base-estrutura-c3, 2× bal-devolver-c2-repentina, 2× bal-etereo-c2, 2× base-aliado-c4-equilibrado-grau5, 2× base-aliado-recurso-c7, 2× base-estrutura-c1, 2× base-estrutura-c4-torre, 2× eternyr-comprar-c2, 1× bal-anular-c3-repentina, 1× bal-etereo-c3, 1× base-aliado-c4-defensivo, 1× base-aliado-c6-defensivo, 1× base-aliado-c6-equilibrado, 1× base-aliado-recurso-c1, 1× base-aliado-recurso-c5, 1× base-comprar-c1, 1× base-dano-c2, 1× base-destruir-c3, 1× base-destruir-c4-repentina, 1× base-estrutura-c4-fonte, 1× eternyr-comprar-c1, 1× eternyr-comprar-c3, 1× eternyr-comprar-c4, 1× eternyr-dano-aliado-c4, 1× r6-anjo-c6, 1× r6-colosso-c7, 1× r6-furia-c3, 1× r6-mercador-c7
- Estilo: {"nome":"Ramp","agressividade":0.2,"reservaInstantes":false,"grauAlvo":9,"queimaFace":false}

Próximo passo: acionar o agente-balanceador (`.claude/agents/agente-balanceador.md`).
