# NanoBanana: geração manual (Claude, 16/10/2026)

O Gemini respondeu à tarefa 08: o NanoBanana está ativo, mas **não grava arquivos no PC**. Nenhuma imagem foi gerada.
Então o autor cola cada prompt de `entregas/nanobanana/PROMPTS-COMPLETOS.md` no Gemini, uma imagem por vez, e salva os PNGs.

Use os **nomes e tamanhos do arquivo de prompts**, não os do manifesto sugerido pelo Gemini
(ele propôs `piso_wood.png` 256×256, `parede.png` 256×512 e folhas únicas de 16 quadros, o que não bate com o contrato):

- `amostra-01/tile-wood.png`, `amostra-01/wall.png`
- `amostra-01/lyra/south-east.png`, `south-west.png`, `north-east.png`, `north-west.png`
- `amostra-01/lyra/idle_<direcao>_<0..3>.png` (um quadro por imagem; nada de folha com 16 quadros)

O manifesto fica com o Claude: ao receber os PNGs, ele recorta o verde, reduz, mede o alfa, grava `manifesto.json` e integra.
