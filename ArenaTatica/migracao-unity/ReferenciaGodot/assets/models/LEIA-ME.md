# Modelos Chibi 3D

As cenas abaixo contêm malhas sólidas, materiais e partes editáveis no Godot:

- `CiantiaChibi3D.tscn`: cabelo loiro, olhos verdes, casaco preto com forro roxo e bordas douradas.
- `DragaoDourado3D.tscn`: corpo, quatro patas, asas com espessura, chifres e cauda curva.
- `DragaoTransplanar3D.tscn`: versão azul-violeta com materiais claros nas nervuras e crista.

Abra uma cena no Godot, selecione uma parte e edite sua transformação ou material no Inspetor. O jogo carrega essas cenas diretamente ao iniciar. Reinicie com F8 e F5 para conferir alterações. A altura no tabuleiro é definida pelo metadado `visual_height` de cada entidade da arena.

`scripts/ChibiModels.gd` monta os modelos originais por geometria e também carrega as cenas editadas. `scripts/VisualSetup.gd` faz a integração, protege os modelos contra duplicação, acompanha a movimentação e reproduz a escala da animação de ataque. Os modelos não têm colisores e não usam billboard nem PNG como personagem. O piso continua usando sua textura sobre a geometria do tabuleiro.

O script `tests/capture_models_3d.gd` regenera as três cenas a partir do gerador e produz uma folha de vistas. **Ele sobrescreve as cenas acima**; preserve uma cópia antes de executá-lo se tiver feito ajustes manuais.

Validação específica: `tests/test_models_3d.gd` verifica volume, altura, apoio no chão, ausência de sprites/colisores, acompanhamento do movimento e ataque, independência da câmera, respawn, unicidade e liberação dos nós.

## Revisão 2 — 09/10/2026
Olhos menores e expressão sóbria para Ciantia; dragões quadrúpedes com focinho alongado. Dourado pequeno reservado às futuras skills, sem animação de skill implementada nesta entrega. Inimigos usam o transplanar. Arena 24 × 24, mantendo a câmera com todo o tabuleiro visível. Versão anterior preservada em .artifacts/modelos-v1.
