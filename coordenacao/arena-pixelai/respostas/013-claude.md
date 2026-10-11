# 013 — Dados integrados (Claude, 10/10/2026)

- Os 7 PNGs de `entregas/dados-013/Dados/` foram copiados para `Resources/Sprites/Dados/`, com os hashes conferidos.
- O importador (`Assets/Editor/ArenaBuild.cs`, exceção `/Dados/`) usa filtro Point, sem mipmaps, com pivô central, PPU 128 e sem compressão. `rolagem` virou Multiple com 10 quadros (`rolagem_0`…`rolagem_9`, ordenados pelo número).
- No jogo, a rolagem tem 14 FPS e DiceLand de 0,7 s, como pedido. A face final aparece por 0,9 s.
- Testado no build Windows: a Ciantia ataca a Ciantia com o Dragão Dourado. Capturas em `ArenaTaticaUnity/Artifacts/shots-1280x720-*.png` e `shots-1920x1080-*.png`: 0 é antes do ataque, 1 a 3 durante a rolagem, 4 a 7 na face final e no impacto. Para repetir, rode `ArenaTatica.exe --arena-smoke --arena-shots` (sem batchmode, senão a captura sai preta).
- Ajustes feitos junto:
  - a lista de PV e o placar esperam a animação, para não mostrar o dano antes de os dados caírem;
  - a barra de cima cabe em 1280 px.
- Evento `trava`: usa o mesmo `RollDice`, com as legendas Trava/Esquiva. Ainda não há captura.
- Regras: o manual foi lido e registrado no DESIGN, sem mudanças. As diferenças (Cataclisma, coringa Esquiva/Trava, Escudo −1, Trava tira todo PA e PM) são escolhas nossas. A comparação crítico/armadura fica para o PvPvE.
- 012 (nitidez): ainda não há capturas em retrato nem em 2400 px, porque o monitor limita a janela a 1080 px de altura. Medida em 1280×720: a Ciantia tem cerca de 45 px visíveis, e em 1920×1080 cerca de 90 px. O retrato do lutador ativo está pronto: fica no canto inferior esquerdo da mesa (paisagem), com cerca de 120 px de arte e nome e PV embaixo. Captura: `Artifacts/shots-1280x720-5.png`. Ainda falta decidir o multiplicador de 1,15 no sprite da mesa.
