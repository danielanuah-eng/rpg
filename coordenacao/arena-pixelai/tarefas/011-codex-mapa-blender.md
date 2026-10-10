# 011 — Mapa em Blender com texturas do PixelLab (decisão do autor, 10/10)
**De:** usuario (via claude)
**Para:** codex
**Status:** aberta, PRIORIDADE 1
**Responsável:** codex (modelos e texturas); claude (importação, montagem, luz e câmera)

O autor decidiu: **o mapa é feito em Blender** (há um Blender 5.2 instalado neste PC), com **texturas em pixel art feitas na aba Map do PixelLab**.
Os personagens e os efeitos continuam sendo sprites do PixelLab. O estilo buscado é o HD-2D: cenário 3D de poucos polígonos e personagens em pixel.
Esta tarefa substitui a seção "Aba Map" da 010 para o tabuleiro. As abas Objects e UI da 010 continuam valendo; os objetos do tabuleiro passam a ser modelos 3D.

## Princípio: kit modular, não mapa fixo
O jogo sorteia o mapa a cada partida pelo MapGen: relevo de 0 a 3, Arbusto, Árvore, Caixa e a zona central.
Por isso o Blender entrega **peças** que o jogo encaixa casa por casa. A única cena fixa é o **entorno** de cada cenário.

## Medidas do jogo (obrigatórias)
- 1 casa = 1 unidade do Blender = 1 m, e a peça ocupa 1×1.
- Cada nível de altura vale 0,45 m. O relevo vai de 0 a 3.
- O tabuleiro tem no máximo 12×12 casas e encolhe conforme o número de jogadores.
- Câmera isométrica a 45° vinda de (+X, +Y, +Z).
- Luz principal direcional vinda do noroeste.

## Peças por cenário (Pedreira, Bosque, Deserto, Neve e Vulcão)

| Peça | Descrição |
|---|---|
| `topo_a`, `topo_b` | Tampa da casa, de 1×1 e cerca de 0,05 de altura, em duas variantes para o xadrez sutil. |
| `topo_zona` | Tampa da zona central, mais destacada (runas, mosaico). |
| `coluna` | Segmento lateral de 1×1×0,45, empilhável, com pedra e terra nas faces. O jogo empilha conforme a altura da casa. Fazer 2 variantes. |
| `beira` | Acabamento da borda do tabuleiro, pendurado nas laterais externas, como raízes ou rocha quebrada. Peças reta e de canto. |
| `arbusto` | Cabe em 1 casa, com até 0,6 de altura. |
| `arvore` | Cabe em 1 casa na base; a copa pode passar um pouco. Até 2,2 de altura. |
| `caixa` | Cerca de 0,85 por lado. Se possível, também os estados rachada e quebrada (ver `OBJETOS-CENARIO.md`). |
| `pedra`, `enfeite_*` | Decoração da borda e do entorno, como cerca, placa, tocha e cristal. |
| `entorno` | Cena fixa: ilha ou plataforma onde o tabuleiro de 12×12 se apoia, com a borda se desfazendo no cenário e o fundo. O centro (0,0,0) fica livre para o tabuleiro. |

Correspondência com o tema: Pedreira usa rocha e cascalho; Bosque, grama, raízes e carvalho; Deserto, arenito, ruínas e cacto; Neve, neve, gelo, cristal e pinheiro; Vulcão, basalto, lava e árvore carbonizada.

## Estilo
- Poucos polígonos, com formas chanfradas e um pouco irregulares, para o mapa não parecer feito de cubos perfeitos.
- **Texturas em pixel art** de 32 a 64 px por casa, feitas na **aba Map do PixelLab**: tiles de piso, faces laterais e casca/folhagem.
  - Usar sem suavização e com paleta limitada, combinando com os sprites dos lutadores.
  - Não usar texturas fotográficas nem PBR realista.
- As sombras vêm da luz do Unity. Não pintar sombra fixa na textura do topo.
- **Movimento** para o mapa não ficar estático:
  - Copas e arbustos com uma vertex color marcando o quanto cada ponto balança (vermelho = 1 na ponta, 0 na base). Eu faço o shader de vento.
  - Lava, água e cristais em material separado (`*_emissivo`), para eu animar.
  - Tochas e fogueiras com um "empty" chamado `fx_fogo`, onde eu coloco o fogo em sprite do PixelLab.

## Exportação
- Formato **FBX**: Apply Transforms, Forward −Z, Up Y e escala 1.0. Aceito também `.glb`.
- Um arquivo por peça: `mapa/<cenario>/<peca>.fbx`. O `entorno` vai em um arquivo próprio.
- Pivô no **centro da base** de cada peça, na altura 0.
- Texturas PNG ao lado, com os mesmos nomes dos materiais.
- Guardar o `.blend` de origem em `entregas/mapa-blender/fontes/`.

## Entrega
- Pasta `entregas/mapa-blender/`.
- `manifesto.json` com os campos `cenario`, `peca`, `arquivo`, `textura`, `dimensoes`, `tris`, `vento` (sim ou não), `emissivo` (sim ou não), `empties` e `substitui`. Exemplos de `substitui`: `piso:a`, `coluna`, `terreno:Arvore`.
- Um render de preview por cenário, com o kit montado num tabuleiro de exemplo de 12×12 com relevo.
- **Ordem:** primeiro o Bosque completo, como piloto, para eu integrar e o autor aprovar o visual. Depois os outros 4 cenários.
- Limite: até ~3 mil triângulos por peça. O `entorno` pode ter até ~40 mil.

## Regras
- A arte não muda regra: a posição dos terrenos e o relevo continuam vindo do MapGen.
- Arbusto, Árvore e Caixa precisam continuar legíveis como obstáculos para o jogador.
