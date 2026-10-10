# 012 — Diagnóstico de nitidez

Data: 10/10/2026. Estado: diagnóstico local entregue; correção de apresentação e validação no jogo aguardam Claude. Nenhum PNG de produção foi alterado.

## Resultado

As artes conservam detalhes suficientes no arquivo original. O problema predominante observado no comparativo é a redução para poucos pixels na tela. Recortar transparência ou exportar a mesma arte numa resolução maior não recupera detalhes que desaparecem nessa redução. A conclusão sobre a tela real ainda depende de capturas do jogo: o comparativo abaixo é um laboratório de redimensionamento, não uma captura de Unity.

Conferi **410 PNGs** por leitura, medindo alfa, contorno visível e hashes em `auditorias/nitidez-012/metricas.json`. O procedimento reproduzível está em `auditorias/nitidez-012/medir.py`; o comparativo de seis personagens em `comparativo.png`.

## Margens, corpo visível e importação

O limiar usado foi alfa >= 16. Exemplos de `south.png`:

| Personagem | Arquivo | Altura visível | Margem inferior |
| --- | --- | --- | --- |
| Ciantia | 128×128 | 120 px (93,75%) | 4 px |
| Talyssa | 128×128 | 119 px (92,97%) | 4 px |
| Perry | 128×128 | 120 px (93,75%) | 4 px |
| Teantha | 128×128 | 123 px (96,09%) | 2 px |
| Dragão Espectral | 192×192 | 148 px (77,08%) | 18 px |

As tiras animadas humanas têm quadros maiores (168–180 px), mas corpos de aproximadamente 113–127 px; o dragão animado tem quadros de 244 px e corpo de aproximadamente 140–149 px. Há espaço para movimento. Isso não significa que os personagens devam ser ampliados pela razão entre corpo e canvas: `ArenaPresentation.cs:305` normaliza a altura pelo `Renderer.bounds.size.y`, com mesh Tight; o pivô do pé também é compensado (`:307`). O tamanho final não é definido apenas pela dimensão do PNG.

Os metas conferidos dos sprites `south.png` usam PPU 128, sem compressão, filtro Trilinear e mipmaps. O importador em `Assets/Editor/ArenaBuild.cs:64` usa o limite alfa > 40 para localizar o pé, comum ao arquivo; o limiar diferente desta auditoria é intencional e consta das métricas. Não recortar quadros separadamente nem deslocar pivôs por quadro: isso pode criar tremor.

**Importante para interpretar as métricas:** `Projeteis/` é importado como Single mesmo quando a imagem é larga. A auditoria foi ajustada para não dividir esses arquivos em falsos quadros. Nas outras pastas, a quantidade de quadros quadrados é inferida pelas dimensões; o importador é a referência de uso em Unity.

## Legibilidade entre 30 e 60 pixels

O laboratório reduz os mesmos arquivos para quadros de 30, 60 e 96 px, comparando nearest e Lanczos. Lanczos aproxima uma redução suave; não reproduz exatamente o filtro, mipmaps, shader e iluminação de Unity. A ampliação da prévia é apenas para inspeção, com limite de tamanho.

A 30 px, rosto, roupa e contornos internos se misturam; a silhueta e as cores maiores ainda identificam a peça. A 60 px há melhoria, mas detalhes pequenos continuam frágeis. A 96 px a identidade visual fica mais legível. Point conserva bordas duras e também evidencia irregularidade ao diminuir; não o recomendo automaticamente para todos os personagens do tabuleiro 3D. Mipmaps e Trilinear podem reduzir cintilação na visão geral; a comparação real deve decidir o ajuste.

Contorno adicional e contraste localizado podem ajudar a separar silhuetas do chão. Antes de redesenhar toda a coleção, medir o personagem na tela real e experimentar a apresentação. Não há evidência local que justifique gerar agora uma segunda coleção de miniaturas. **Custo de novas gerações nesta auditoria: zero.** Se a apresentação ainda falhar, avaliar primeiro uma amostra simplificada de um personagem a 48/64 px, com orçamento confirmado antes de ampliar a produção.

## Encaminhamento para Claude: apresentação, câmera e HUD

A leitura do código aponta câmera ortográfica isométrica (`ArenaPresentation.cs:105`), enquadramento automático por cantos e região útil da HUD (`:223`, `:238`), compensação vertical de aproximadamente 1/cos(35,26°) no sprite (`:312`) e zoom existente até cerca de 3,33 vezes o enquadramento geral (`:797`). Portanto, não aplicar novamente a compensação vertical e não reativar a câmera antiga com rolagem. O tabuleiro inteiro permanece visível na visão geral, conforme a direção do autor.

Peço testar, em ordem:

1. Medir altura **visível do corpo em pixels de tela**, sem contar rótulo, sombra ou canvas transparente, nas resoluções abaixo. Registrar também tamanho da região útil da câmera e zoom. Separar problema de escala de problema de contraste.
2. Mostrar a arte da unidade selecionada em retrato da HUD com ao menos 96 px de altura visível, preferencialmente 128 px quando houver espaço. Isso fornece leitura de rosto e equipamento sem retirar a visão geral do tabuleiro. Os valores são alvos iniciais para teste, não tamanhos já validados em jogo.
3. Se a silhueta continuar pequena, comparar a apresentação atual com um multiplicador visual uniforme de **1,15** apenas no sprite. Preservar posição lógica, colisão e movimento; verificar sobreposição em células adjacentes antes de adotar. Não aumentar automaticamente todos os personagens. O tamanho relativo do dragão e dos humanos deve permanecer coerente.
4. Comparar o filtro atual com uma alternativa em uma amostra durante idle, caminhada e zoom. Usar Point sem mipmaps para ícones da HUD como os novos dados; decidir os personagens do tabuleiro pela captura e movimento reais. Não aplicar Pixel Perfect Camera como correção automática a essa câmera 3D ortográfica.
5. Se necessário após esses testes, pedir ao Codex uma alteração visual localizada de contorno/contraste com a imagem e a escala de tela problemáticas. Não reenviar todos os sprites ao gerador.

## Efeitos e projéteis

`Sprites/Efeitos/` contém apenas `moeda.png` na inspeção atual. Outros impactos usam fallback de apresentação quando não existem quadros (`ArenaPresentation.cs`, leitura de `EffectFrames`). Não declarar os efeitos exclusivos entregues com base na existência desse diretório.

As imagens largas em `Projeteis/` são imagens únicas, não tiras: `baforada` 128×32, `dragao-dourado` 64×32. A largura visual pode conservar a leitura e a espessura ainda desaparecer ao diminuir. Em `canhao-de-mao`, os quatro cantos têm alfa alternado (255/0/255/0); há pixels opacos no canto e é necessário conferir o resultado sobre o chão antes de interpretar qualquer bloco como erro de transparência.

Claude: medir a espessura real do núcleo do feixe e a extensão do impacto; testar núcleo de pelo menos 2 px de tela e impacto de pelo menos 12 px como ponto de partida. São ajustes visuais sugeridos para comparação, não mudanças de dano, alcance ou área lógica. O pivô central de Efeitos/Projeteis já existe no importador; não trocar por pivô de pé.

## Pendências que exigem conferência em Unity

- Conferir os nomes reais dos sprites Single carregados por `Resources.LoadAll<Sprite>`: `ReadRig` (`ArenaPresentation.cs:476`) procura nomes exatos das oito direções. Alguns metas antigos guardam um nome com sufixo `_0`; isso **não prova** o nome retornado em runtime. Não trocar a busca por prefixo, pois `south` também coincide com `southwest`.
- Conferir pipeline e shader ativos na cena. A presença de URP em Packages não confirma que ele seja o pipeline usado pela câmera. Não houve captura nem inspeção ao vivo do jogo nesta rodada.
- Devolver `respostas/012-claude.md` com capturas de **1280×720**, **1920×1080**, **1080×2400 retrato** e **2400×1080 paisagem**, antes/depois, uma unidade selecionada, grupo em células vizinhas, dragão, projétil e impacto. Registrar alturas visíveis medidas e filtros efetivamente usados.

Depois dessa resposta, Codex confere as capturas e reavalia apenas o que continua ilegível. O problema não está marcado como resolvido e esta etapa não autoriza alteração de regras.
