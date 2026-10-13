# 018 — Codex/GPT: acesso total aos pedidos da Arena de 13/10

**De:** autor (13/10/2026), registrado pelo Claude
**Para:** Codex/GPT (e Claude)
**Status:** aberta
**Aprovação:** o autor liberou o GPT para mexer em **tudo** desta lista, inclusive código (`ArenaTaticaUnity/Assets/ArenaTatica/Scripts`), além de arte, UI e efeitos.

## Pedidos do autor

1. **Turnos (prioridade alta):** escolher entre *Iniciativa alternada* e *Jogador completo*.
2. **Tabuleiro reto:** quadrado de frente, inclinação só na vertical (antes ficava de lado).
3. **Giro linear:** girar aos poucos, sem ficar preso a 4 vistas.
4. **Ficha ao clicar no lutador:** ver o que cada skill faz.
5. **Edgar fantasma:** não é atingido por nada.
6. **Nerf da invisibilidade:** golpe de frente acerta, movimento limitado.
7. **Ezequiela:** clone sempre colado, lado sorteado, sem dar para saber qual é a original.
8. **Mais skills genéricas** (D&D, Krosmaster, anime).
9. **Sala online em 3 passos:** abrir a sala → escolher modo e configurar → liberar → iniciar (não começa sozinha).
10. **Reiniciar partida** na LAN/internet sem recriar a sala.
11. **Loja configurável** por sala.
12. **Modo de equipes.**
13. **Torneio:** até 6 jogadores, pool de lutadores, até 2 bans por jogador.
14. **Botões com LERP** (transição suave) em todo o jogo.
15. **Tela de seleção mais organizada.**

## O que o Claude já fez (13/10), para não repetir

| # | Onde |
|---|------|
| 1 | `TurnMode` em `Core/CoreTypes.cs`, `Match.BuildTimeline`; menu Jogar/Sala |
| 2, 3 | `ArenaPresentation`: `CameraSide=0`, `Yaw`/`RotateBy`/`StraightenView`; HUD ◄ Reto ► |
| 4 | `ArenaHud.Inspect` (painel "FichaLutador"); toque com Mover num lutador |
| 5 | `Match.Hurt`: forma intangível zera todo dano |
| 6 | `Match.HiddenPm=2`; inimigo colado acerta e quem encosta revela |
| 7 | `Match.SpawnIllusions`: casa, id, ordem na lista e linha do tempo sorteados |
| 8 | `Content.Generic` (40 skills) + "Biblioteca de skills genéricas" no Editor |
| 9, 10, 11, 12, 13 | `ArenaGame` (OpenLobby/Release/StartRoom/RestartRoom/BackToLobby, torneio `Cup`), aba **Sala** do `ArenaMenu`, formato "equipes" |
| 14 | `ArenaTema.Lerp` + `ArenaBotaoLerp` em todos os botões (menu, HUD, editor de mapas) |
| 15 | Seletor por Nível e nome, filtros, ficha do lutador ao lado |

## Onde o Codex pode melhorar

- **Visual** da aba Sala, do seletor, da ficha do lutador, da chave do torneio (hoje é texto) e dos botões (ícones, sombras, cor).
- **Efeito** de "revelar" (quem encosta no escondido) e de "imune" no Edgar fantasma.
- **Arte** para as skills genéricas, se quiser um ícone por skill.
- **Código:** liberado. Para evitar conflito, antes de editar um arquivo `.cs`, registre numa nota (`notas/`) qual arquivo vai mexer. Puxe o estado atual e rode os testes depois (`unity test --mode EditMode` com o Editor fechado). Não mude regras sem o autor pedir.

## Pendências reais

- Torneio: quem não está jogando a partida só vê o resumo da sala (ainda não assiste à partida).
- Chave do torneio é mata-mata sorteado (ímpar passa direto).

Resposta em `respostas/018-codex.md`.
