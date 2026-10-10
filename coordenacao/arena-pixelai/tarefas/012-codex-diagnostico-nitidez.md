# 012 — Diagnóstico de nitidez dos sprites na mesa (pedido do autor, 10/10)
**De:** usuario (via claude)
**Para:** codex
**Status:** aberta, PRIORIDADE 1
**Responsável:** codex (leitura das imagens, diagnóstico e decisão); claude (mudanças no Unity que o diagnóstico pedir)

O autor disse que, no jogo, os sprites estão "muito distorcidos e desfocados" e que não dá para ver os detalhes.
Ele decidiu que **a leitura das imagens fica com você**: leia, faça o diagnóstico e **decida quem corrige**.
Se a causa estiver na imagem ou no modelo, você mesmo corrige. Se estiver no programa, escreva o pedido para mim.

## Fluxo automático
1. Ao terminar qualquer mudança sua neste assunto, rode de novo a leitura e o diagnóstico, sem esperar novo pedido.
2. Escreva o resultado em `respostas/012-codex.md`, separando:
   - **O que você mudou** (arquivos e hashes);
   - **O que pede para o Claude mudar no Unity**, com o valor exato (filtro, escala, câmera, contorno, shader, PPU).
3. Repita até a figura ficar legível nas resoluções da lista abaixo, ou até precisar da decisão do autor.

## O que já foi feito no código (10/10, pelo Claude)
- Os quadros têm 168 px (o dragão espectral, 192 px) e 128 PPU. Na mesa inteira, a figura ocupa só de 25 a 60 px de altura na tela.
- O filtro era Point sem mipmaps, o que descartava pixels ao reduzir. **Agora é trilinear com mipmaps (Kaiser)**, em `Assets/Editor/ArenaBuild.cs` (`EnsureSprites`).
- A câmera é ortográfica isométrica (35,26° / 225°). O sprite fica de pé e girado só no eixo Y. Ele era achatado em cerca de 18%, e agora a altura é esticada por 1/cos(35,26°).
- Há zoom (roda do mouse ou pinça, até cerca de 3,3×) e arraste da câmera. Na tela inteira, porém, a figura continua pequena.
- A altura de cada lutador no mundo está em `ArenaPresentation.Heights` (por exemplo, Ciantia 1,6). O valor conta o quadro inteiro, inclusive a margem transparente. Por isso, a figura visível é menor que isso.

## O que ler
- Sprites: `ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/<Lutador>/` (`<direção>.png` parado e tiras `<clipe>_<direção>.png`).
- O jogo rodando: `ArenaTaticaUnity/Builds/Windows/ArenaTatica.exe` (janela). Capture a mesa inteira e também com zoom.
- Resoluções de referência: 1280×720, 1920×1080 e celular em retrato e em paisagem (cerca de 1080×2400).

## Perguntas que o diagnóstico precisa responder
1. Quanto do quadro é margem transparente? Recortar ou centralizar ajudaria a figura a ficar maior, sem perder a referência do pé?
2. A arte de 128 px reduzida para 30 a 60 px ainda é legível? Ajudaria contorno escuro de 1 px, mais contraste ou uma paleta mais limitada?
3. Valeria uma versão "miniatura" mais simples para a mesa inteira e a atual com zoom? Diga o custo de PixelLab, se houver.
4. Os efeitos e projéteis têm o mesmo problema?
5. Do meu lado, o que você recomenda?
   - Figura maior em relação à casa (dizer quanto).
   - Zoom inicial mais próximo.
   - Contorno feito por shader.
   - Voltar ao filtro Point com escala inteira (pixel perfect).
   - Outro ajuste.

Não alterar regras do jogo. Preservar os nomes de arquivo e o formato de tira, que o importador usa.
