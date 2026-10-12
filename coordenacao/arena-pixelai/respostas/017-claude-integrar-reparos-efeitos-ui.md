# 017 - Resposta do Claude: reparos, efeitos e modelo de interface

**Status:** integrado parcialmente no codigo; verificacao no Unity pendente (rascunho).

## Importados
- Reparos (sobrescritos preservando GUID): `Sprites/Baltazar/idle_west` (4x168), `Sprites/Baltazar/walk_west` (6x176), `Sprites/GuardiaoMusgo/idle_north-west` (4x176). Slicing, PPU 128, Point e pivo do manifesto pelo importador `Assets/Editor/ArenaEfeitosImport.cs`.
- 171 tiras de `unity/Sprites/**` copiadas para `Assets/ArenaTatica/Resources/Sprites/**` (Efeitos, Objetos, Projeteis, ProjeteisAnimados). `unity/Biblioteca` nao foi copiada.
- `Resources/Efeitos/manifesto.json`: copia enxuta do manifesto (FPS, flags ownerOnly etc.).
- Avisos do lote `fechamento-arena-2026-10-11`: os 12 ja estao em `Resources/Sprites/Avisos` e sao usados por `ArenaHud.ShowWarning`.

## Gatilhos conectados (codigo)
- Fase explicita: `ArenaEfeitos.Frames(caminho)` carrega um arquivo e filtra por nome; a pasta sozinha nao concatena fases.
- Conjuracao no lancador (compras/itens, disparos), impacto no destino, projetil animado (`ProjeteisAnimados`) orientado a trajetoria.
- Eventos: `inicio` no cataclisma, `loop` enquanto `s.ev` durar, `fim` em `evento-fim`, `festival-de-fogos/explosao`.
- Terreno lava/gelo/fogos em laco (`SpriteLoop`); mina de gelo armada em laco, so para aliados do dono.
- Visibilidade: `ArenaEfeitos.Visible` (ownerOnly + estado/inheritTargetVisibility); aplicada ao efeito de uso sobre o alvo.
- Devorar: efeito no lancador, moeda do alvo ao lancador (ja existente).

## UI
- `ArenaTema.cs` com os tokens do modelo; menu e HUD usam a paleta nova (dourado so como acento, TeamColor/PlayerColors intactos).
- Seletor de equipe: borda dourada + selo OK + fundo, foco de teclado, avanco desabilitado com equipe vazia ("Escolha um lutador"). Nove abas, callbacks e elenco completo mantidos.

- Tela Inicio: titulo, subtitulo e CTA Jogar dourado a esquerda (~45%) com atalhos Personagens / Editor / Voltar; tabuleiro transparente inteiro (mapa-bosque, copiado sem alteracao para Resources/Sprites/Menu) a direita; Novidades em painel com borda dos tokens.
- Seletor de equipe (HUD): cartoes com retrato, nome, custo, PV (token hp) e PA/PM (token pm), selo OK + borda dourada, foco; painel lateral com a equipe, custo total e avanco desabilitado com equipe vazia.
- Conferencia visual desses dois layouts: feita em Play Mode (ver abaixo).

## Testes
- `Tests/Editor/PresentationTests.cs` (novo): quadros do manifesto, isolamento de fases, reparos 4/6/4, 9 abas, equipe vazia/custo, ownerOnly, visibilidade de estados, terreno em laco.
- Compilacao sem erros; EditMode 72/72 (60 existentes + 12 novos), rodado pelo Opus no editor.

## Pendencias reais
- Estados persistentes (armadura, buff, preso, escondido, sem-pm-oleo) e ids da Asamyh nao estao ligados a loops.
- Objetos de campo alem do idle da mina (surgir/quebrar, totem, prisao, casas, bau, buraco) nao ligados.
- Dano nao esta alinhado a `impactFrame0` (martelo perfurante no quadro 6).
- Seletor em modo retrato (celular) mantem o painel lateral; nao ajustado.
- Escala dos efeitos de evento (largura = 90% do tabuleiro) e dos objetos precisa de conferencia visual.

## Play Mode (Opus, 12/10/2026)
- Callbacks das abas: Inicio e Jogar navegaram; as 9 abas e Voltar ao jogo estao presentes.
- Tela inicial: duas colunas com o tabuleiro do bosque. Seletor: elenco completo (32), retratos, custo, PV/PA/PM, selo OK, borda dourada e painel lateral com custo 12/12. O texto das Novidades corta a ultima linha em 16:9.
- Equipe vazia que desabilita o avanco: coberta por EmptyTeamDisablesAdvanceAndCostFollowsTheTeam; nao foi clicada em tela.
- Console: sem erros do jogo; o unico erro foi o timeout da propria ferramenta de automacao.
- Reparos: 168/176/176 px, Point, sem mip (.meta + RepairedStripsSliceIntoTheRightCells). O recorte visual em partida nao foi observado.
- Fases e ancoras dos efeitos, terreno em laco, visibilidade por dono e tabuleiro inteiro em partida: cobertos so por testes EditMode; conferencia visual fica para o autor jogar.
