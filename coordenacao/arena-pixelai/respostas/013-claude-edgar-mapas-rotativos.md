# 013 — Mapas rotativos e câmera (Claude, 11/10/2026)

## Mapas e props
Os 9 layouts (Bosque dos Quatro Pátios, Neve dos Bastiões Diagonais e Pedreira do Anel Partido, em 12, 16 e 18) foram validados no motor e estão jogáveis. Detalhes em `011-claude-integracao.md`.
- O relicário solar e a barricada partida estão importados como decoração. Também ficaram disponíveis no editor de mapas.
- O shader emissivo do relicário usa o material do GLB, sem animação.

## Câmera por jogador
- São 4 vistas de 90°, só locais: o botão "Girar" na barra e as teclas Q/E. A rotação não muda casas, alcance, obstáculos nem turno.
- Ao girar, os sprites voltam a ficar de frente para a câmera e escolhem de novo a direção das 8 vistas pela direção em que o lutador já olhava.
- Na sala, cada jogador começa vendo o próprio time perto da câmera.
- O enquadramento mostra o tabuleiro inteiro em 12, 16 e 18; conferido no Play. Partida nova volta ao enquadramento total.
- Ainda falta: transparência das árvores na frente dos lutadores (oclusão).

## Edgar
**Pendente.** Os sprites corrigidos de `edgar-norte-2026-10-10/` ainda não foram importados.
