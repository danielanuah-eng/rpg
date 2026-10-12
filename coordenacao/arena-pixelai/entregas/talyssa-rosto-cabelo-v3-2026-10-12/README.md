# Talyssa — revisão de rosto e cabelo V3

Pedido do autor: “rosto esquisito e cabelo esquisito, mas tá no rumo”. A direção segue em revisão; isso não constitui aprovação final do modelo.

Escopo: suavizar a expressão, ajustar pálpebras e íris, trocar as sobrancelhas rígidas por fios delicados e reconstruir o cabelo curto em mechas com direção e comprimento variáveis. Corpo, roupas, equipamento, câmeras e luzes são preservados da amostra V2. A preservação da geometria e das matrizes de câmera/luz é conferida no script e registrada em `preservacao.json`.

Fonte anterior: `../talyssa-blender-v2-2026-10-12/Talyssa-v2-amostra.blend`. Origem anatômica e licença CC0: `../talyssa-blender-v2-2026-10-12/base-cc0/origem.json`. A imagem conceitual não é usada como textura ou plano no render.

Execução no Blender 5.2.2 LTS:

`blender -b --python-exit-code 1 --python refinar_rosto_cabelo.py -- --final`

Sem `--final`, produz apenas uma prévia mais rápida de revisão. Cada execução reabre a V2; os ajustes não se acumulam sobre a saída.

Arquivo final da rodada: `Talyssa-rosto-cabelo-v3.blend`. Prévias: `rosto-v3.png` e `corpo-v3.png`. Continuam pendentes a aprovação visual, otimização, rig, animações e integração. Não reproduzir em todo o elenco nem integrar esta amostra como personagem pronto.
