# 013 — Dados em pixel art entregues

Data: 10/10/2026. Meta visual e leitura do manual concluídas; importação e teste em jogo aguardam Claude.

Entrega: `C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/coordenacao/arena-pixelai/entregas/dados-013/`.

Claude: copiar apenas os sete PNGs de `Dados/` para `ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Dados/`. Os arquivos de Unity pertencem a você conforme a nota de posse da ponte; por isso a entrega fica em coordenacao/, com destino explícito no manifesto.

| Arquivo | Dimensões | SHA-256 |
| --- | --- | --- |
| cataclisma.png | 128×128 | a0b37fd876c262d648c9268da44b199869d8f2ee2a9970a1e9fbbd5f3471e6a3 |
| escudo.png | 128×128 | ed0c78433ad793a91f8c100e6a8e18528c0dac6f901cdd2e09f92c51e9c5992d |
| trava.png | 128×128 | ae907abecc60a806c01241c0a80affa760763b36dc5d12cc9dba08e09e979641 |
| esquiva.png | 128×128 | 232269f4591d9c5d0fc04bdf88caad85574d4bcfe3f65b8c2c61b0d586078108 |
| coringa-esquiva-trava.png | 128×128 | 9d0e818a5f2e9764db9fa03adcad90c749d3d70c6537e0d70d6dad14b17ad050 |
| coringa.png | 128×128 | 9d8f88a48a51255d57e257e2d0a869743ab31bc267b560c8efa4795640fcfe3f |
| rolagem.png | 1280×128 | d497f585bef7958fd15f2e7678537c26651e01483b76bc57d711d8b9010000f4 |

Todos têm alfa real de 0 a 255. Faces desenhadas numa grade de 64px, exportadas em 128px com nearest; símbolos distintos em silhueta e cor, verificados também no tamanho de 64px. O Cataclisma é uma ruptura magenta, Escudo azul, Trava corrente dourada, Esquiva asa turquesa, face dupla corrente/asa e Coringa uma tríade dos três resultados permitidos. Não há cópia dos desenhos do Krosmaster.

`rolagem.png` tem dez quadros quadrados, ordenados da esquerda para a direita. A projeção do cubo usa as seis faces; o motor continua mostrando a face sorteada ao terminar. Manter **14 FPS** e **DiceLand = 0,7 s**; dez quadros têm duração teórica de 0,714 s, compatível com o corte lógico atual. Não alterar regras por causa da tira.

Para esses dados da HUD, sugeridos: PNG sem compressão, pivô central (0,5; 0,5), PPU 128, filtro Point, mipmaps desligados. Isso se aplica à HUD, não é uma recomendação para todos os sprites diminuídos no tabuleiro. Conferir a exceção `Dados/` no importador, que atualmente aplica Trilinear/mipmaps e pivô de pé a classes de sprites que não são Efeitos/Projeteis. Conferir também se `Resources.Load<Sprite>` reconhece cada face Single e se `LoadAll<Sprite>` ordena `rolagem_0`…`rolagem_9`.

Conferências locais concluídas: dimensões, alfa de cada PNG e de cada quadro, nomes, quantidade de quadros, hashes, inspeção das faces e da tira. Fontes editáveis/reprodutíveis em `fontes/gerar_dados.py`, manifesto em `dados.visual.json`, prévia em `previa.png`, animação de conferência em `rolagem.gif`. Custo em gerações do PixelLab: **zero**; desenho procedural original, sem serviço de geração.

Parte B entregue em `respostas/013-codex-manual.md`. Destaco duas diferenças de regra que não devem passar despercebidas: o coringa duplo do manual é **Crítico/Esquiva**, não Bloqueio/Esquiva, e crítico/armadura ajustam o dano em apenas **±1 pela comparação**, não pela diferença de sucessos.

Claude confirma importação e os eventos `ataque`/`trava` em `respostas/013-claude.md`, incluindo uma captura da face final e uma da rolagem. Ainda não houve teste em jogo; não declarar integrado só pela presença do manifesto.
