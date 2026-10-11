# 012 — Baltazar integrado (Claude, 10/10/2026)

## Recebido e importado
- 32 PNGs de `entregas/baltazar-2026-10-10/sprites/` em `ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Baltazar/`:
  - rotações em `<direção>.png`;
  - ações em `idle_`, `walk_` e `attack_<direção>.png`.
  - O importador (`ArenaBuild.EnsureSprites`) fatia cada tira em quadros quadrados pela altura, inclusive o golpe SE de 188 px, e põe o pivô no pé.
- Companheiros:
  - `anjo-hover.png` → `Sprites/Efeitos/baltazar-anjo.png`;
  - `demonio-hover.png` → `Sprites/Efeitos/baltazar-demonio.png`.
  - Os dois tocam em vaivém `[0,1,2,3,4,3,2,1]` a 6 FPS, com pivô central.
- Todos os itens estavam com `visualReview: approved_codex` e `importAllowed: true`.

## Regras (motor)
- **Ficha:** 14 PV, 6 PA, 3 PM, Iniciativa 2, Nível 4.
- **Martelo de guerra** (`baltazar-martelo`): 3 PA, alcance 1, 2 de dano.
- **Dualidade guardiã** (`baltazar-dualidade`, novo `SkillKind.Aura`):
  - **Recurso:** PA.
  - **Ligar:** custa 1 PA e aplica na hora.
  - **Manter:** com a aura ligada, cada turno do Baltazar começa cobrando 1 PA e aplicando de novo.
  - **Efeito:** até 2 casas (distância do motor), cura 1 nos aliados (ele não) e faz 1 de dano fixo nos inimigos, inclusive neutros.
  - **Limite:** 1 aplicação por alvo por turno dele; desligar e religar no mesmo turno não cobra nem aplica.
  - **Desliga quando:** o jogador escolhe desligar (o botão, sem custo), falta PA no começo do turno ou ele é nocauteado.
- **Eventos:**
  - `aura`, com info `ativar`, `manter` ou `desligar`;
  - `cura` nos aliados;
  - `dano-evento` nos inimigos (info `baltazar-dualidade`).
- **Visual:** o anjo e o demônio orbitam defasados em 180° só enquanto a aura está ligada e o Baltazar de pé. A órbita é só visual e não repete efeito. Ligar e manter mostram um anel do alcance.
- **Testes:**
  - `BaltazarAuraCostsOnePaPerTurnHealsAlliesAndHurtsFoes`;
  - 54/54 EditMode;
  - build Windows;
  - fumaça;
  - contrato de rede.
  - Conferido no Play: sprite no tabuleiro e companheiros orbitando.

## Pedido visual (quando houver créditos)
- **Damian:** idle parado por forma (`idle_cannon_<dir>` e `idle_hammer_<dir>`, nas 4 diagonais), segurando o canhão ou o martelo. Hoje o jogo usa o último quadro dos clipes `cannon`/`hammer` como pose parada (`FormDef.pose`).

## Ainda não feito da 012
- **Aba Edição de jogo:** não há catálogo de assets com estados nem rotação de câmera em quatro vistas.
- **Feito no lugar:** pedido do autor, um **editor de mapas** dentro do jogo (aba Mapas: relevo, árvore, arbusto, caixa, zona e mover peças, com espelho), mais o editor de fichas que já existia.
- **Props decorativos** entram com o kit Blender (011), ainda não integrado.
- **Crítico/armadura:** registrado só para o PvPvE futuro, sem mudança na Arena.
