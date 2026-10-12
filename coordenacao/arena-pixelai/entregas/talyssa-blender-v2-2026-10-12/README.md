# Talyssa — amostra Blender V2

Direção visual aprovada pelo autor em 12/10/2026: resposta “isso” à prancha `../talyssa-direcao-v2-2026-10-12/talyssa-estudo-visual-v2.png`.

Esta etapa constrói uma amostra real em Blender para avaliação. Aprovar o conceito não aprova automaticamente a geometria resultante. Modelo, materiais e renders novos ficam nesta pasta; sprites do jogo e o elenco anterior são preservados.

Arquivo de avaliação: `Talyssa-v2-amostra.blend`. Prévias reais desse arquivo: `talyssa-corpo-final.png` e `talyssa-rosto-final.png`. Estado: amostra estática para revisão visual; não é um personagem pronto para integrar.

O arquivo Blender contém a geometria, materiais e duas câmeras, separados nas coleções de personagem e estúdio. A imagem conceitual não foi colocada em um plano nem aplicada como textura. As peças de cabelo anteriores ficam ocultas como guias editáveis; a apresentação usa fios modelados.

Ainda pendentes: avaliação artística do autor, aproximação do acabamento ao conceito aprovado, retopologia/otimização, rig com pesos contínuos, todas as ações atuais de Talyssa, exportação para jogo e integração Unity. Não propagar esta amostra ao elenco antes da avaliação do modelo real. Os modelos V1 foram rejeitados visualmente pelo autor; seus testes técnicos não constituem aprovação estética.

## Fontes e reprodução

Rosto adaptado da malha e modificadores anatômicos CC0 oficiais do MakeHuman Community. Licença e URLs com hashes em `base-cc0/origem.json`, `base-cc0/LICENSE.md` e `base-cc0/LICENSE.ASSETS.md`. Cabelo, roupa, equipamento, materiais e montagem são definidos pelos scripts locais. Não foi usado código do MakeHuman; apenas os assets.

Procedimento, na ordem (Blender 5.2.2 LTS):

1. `node obter_base_cc0.mjs` — obter os assets oficiais, se não estiverem presentes.
2. `python inspecionar_base.py` — preparar a combinação dos modificadores e dados de malha.
3. `blender -b --python-exit-code 1 --python construir_talyssa.py` — construir geometria inicial.
4. `blender -b --python-exit-code 1 --python refinar_talyssa.py` — aplicar anatomia e revisar roupa.
5. `blender -b --python-exit-code 1 --python acabamento_talyssa.py` — sobrancelhas, franja e materiais.
6. `blender -b --python-exit-code 1 --python fechar_amostra.py` — continuidade dos braços, cascas da roupa e cabelo em fios.
7. `blender -b --python-exit-code 1 --python revisar_estudo.py` — mãos anatômicas, cabelo curto, pele e revisão de caimento.
8. `blender -b --python-exit-code 1 --python verificar_amostra.py` — reabrir o arquivo final e medir sua estrutura.

Executar a sequência completa para reconstruir; não repetir apenas o acabamento sobre sua própria saída. Os arquivos sem `amostra` e as prévias sem `final` são etapas intermediárias preservadas para diagnóstico, não a versão a avaliar.

A verificação estrutural em `verificacao.json` mede geometria, presença dos arquivos, dependências e hashes. Ela não aprova aparência, desempenho em jogo nem ausência de toda interpenetração. A cena usa materiais procedurais do Blender, ainda sem bake para Unity.
