# 017 — Polimento dos mapas e primeira recriação do elenco em Blender

Direção direta do autor, 12/10/2026: priorizar estética/nitidez dos mapas e peças limpas; depois recriar todo o elenco e suas ações atuais no Blender. Todo o elenco foi confirmado pelo autor nesta conversa.

Entrega: `../entregas/polimento-blender-2026-10-12/`. Contrato completo em `LEIA-ME-CLAUDE.md`; estado e hashes em `entrega.visual.json`.

- Cinco temas e 183 GLB dos mapas conferidos contra o kit atual do Unity. Mesmos triângulos, limites globais, pivôs/eixos e nomes dos clipes; materiais mais limpos, contraste revisto e novos renders. Beneficia os 22 layouts que utilizam o kit. Fontes anteriores preservadas.
- Primeira interpretação 3D de 43 conjuntos: elenco, formas, invocações/companheiros, anjo/demônio do Baltazar e vendedor. Fontes `.blend`, rigs, GLB e manifestos individuais. 249 clipes nomeados, com validação de duração, movimento real dos canais, ciclos e reimportação dos rigs.
- Prévias animadas de oito casos, feitas a partir do GLB reimportado. Os arquivos de referência, as associações de clipes e os estados de integração estão nos manifestos.

**Estado real:** entrega para avaliação. Mapas validados como assets; elenco ainda é primeira versão visual, não aprovação estética do autor. Nenhuma reprodução no Unity é declarada. O cliente de sprites não foi alterado. Refinamentos posteriores dependem da avaliação estética e da integração, sem retirar os sprites aprovados.

Claude: integrar o kit dos mapas preservando `.meta` e conferir quatro câmeras/12,16,18/oclusão/loops. Para os personagens, preparar a apresentação 3D em ramo próprio e associar os clipes aos eventos existentes. As transformações TchuTcha/Edgar usam os modelos de destino separados; Damian possui ossos de arma com canais de escala. Nenhuma regra foi alterada nesta entrega.

Publicação em ramo `codex/polimento-blender-2026-10-12`, com arquivos próprios e estado de revisão explícito; alterações concorrentes não fazem parte da entrega.
