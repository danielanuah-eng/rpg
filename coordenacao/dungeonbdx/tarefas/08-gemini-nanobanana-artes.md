# 08. Gemini: gerar no NanoBanana as artes do DungeonBDX isométrico

- **De:** claude
- **Para:** gemini (NanoBanana)
- **Status:** bloqueada no Gemini (não grava arquivos); geração manual pelo autor — ver notas/20261016_claude_para_autor_nanobanana-manual.md
- **Projeto / pasta de trabalho:** C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\entregas\nanobanana
- **Entrega:** local (sem commit, sem push)

## Objetivo
Gerar no NanoBanana as imagens pedidas em `PROMPTS-COMPLETOS.md`, primeiro a amostra 01 e depois o lote 02.

## Contexto
- Prompts prontos, um por imagem: `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\entregas\nanobanana\PROMPTS-COMPLETOS.md`.
- Lista e destinos: `...\entregas\nanobanana\PEDIDOS.json`.
- Contrato visual (projeção, nomes, tamanhos): `C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\coordenacao\dungeonbdx\contratos\ISOMETRICO-2D.md`.
- Claude recorta o verde, reduz, mede o alfa e integra na Unity. Não copie nada para `ArenaTaticaUnity/`.

## Passos
1. Gerar a **amostra 01** (piso `wood`, parede, Lyra: 4 estáticos + 16 quadros de `idle`) e salvar em `entregas/nanobanana/amostra-01/` com os nomes do arquivo de prompts.
2. Escrever `entregas/nanobanana/amostra-01/manifesto.json` (um item por arquivo: `id`, `caminho`, `prompt`, `largura`, `altura`).
3. Deixar uma nota em `notas/` para o Claude. **Continuar** com o lote 02 em `entregas/nanobanana/lote-02/` (mesma regra de nomes e manifesto), sem esperar.
4. Se o NanoBanana não estiver disponível, escrever isso na nota e parar; não usar outra ferramenta no lugar.

## Saída esperada
`amostra-01/` e `lote-02/` com os PNGs e `manifesto.json` de cada pasta; nota `notas/<data>_antigravity_para_claude_nanobanana.md` com o que foi gerado e o que falhou.

## Critério de pronto
Todos os arquivos listados existem, com o nome certo, fundo verde chapado nos transparentes, sem texto na imagem.
Claude confere por amostragem e integra; imagem ausente não entra no manifesto.
