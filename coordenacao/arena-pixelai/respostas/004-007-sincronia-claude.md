# Integração Claude: 004, 007 e as correções de sincronia (10/10)

Situação: integrado e compilado (ARENA_PREPARE_OK). Os testes EditMode passaram (24/24). Falta a conferência do autor jogando.

## Correções de sincronia (entregas/correcoes-sincronia)
Copiadas por cima dos arquivos em `Resources/Sprites/<lutador>/`:

| Lutador | Clipe |
|---|---|
| Damian | hit_north-west, pistol_south-east |
| Ciantia | hit_north-west |
| Ezequiela | hit_north-west |
| Devon | idle_north-west, death_south-east |
| ArqueiroJack | ultshot_south-west |
| TchuTchaBerserk | idle_south-west |
| Teantha | attack_north-west |

- O pistol_south-east do Damian entrou com 9 quadros de 172 px, que é a tira entregue. O `normalizar-pistol.py`, que gera a versão de 168, não foi rodado. Isso funciona, porque o importador usa a altura como tamanho do quadro.
- As versões anteriores continuam guardadas como `.anterior.png` na entrega.

## 007: eventos e cenários
- **Projéteis:** ficam em `Resources/Sprites/Projeteis/<skillId>.png`, cada um como imagem única com pivô no centro, e apontam na direção do voo na tela.
  - Arquivos renomeados para o skillId: `bala-canhao` virou `canhao-de-mao`, `bala-pistola` virou `tiro-de-pistola` e `flecha` virou `tiro-carregado`.
  - Imagem com 128 px de largura ou mais (`raio-arcano`, `baforada`) estica entre quem ataca e o alvo, como raio.
  - Imagem menor (`flecha-abissal`, `dragao-dourado`) voa como projétil.
- **Moeda:** os 9 quadros viraram a tira `Efeitos/moeda.png`, de 32 px por quadro. É usada ao pegar, roubar, comprar e na chuva de moedas.
- **Terrenos:** viraram `Textures/tile_<nome>`, com filtro point e sem compressão. Piso por cenário:

  | Cenário | Piso |
  |---|---|
  | Pedreira | pedreira (a `pedra.png` nova) |
  | Bosque | grama |
  | Deserto | ruinas |
  | Neve | cristal |
  | Vulcão | o piso cinza antigo, tingido |

  Se o mapeamento estiver errado, me avisem.
- O `projeteis/moeda.png` avulso não foi usado, porque a tira animada substitui.

## 004: loja
- A cena do castor, `loja-v2/vendedor/frame-01..09`, de 160×128, ficou em `Resources/Sprites/Vendedor/` e roda a 6 fps na janela da loja.
- Os ícones já estavam integrados.

## Pendentes
- 008, Berserk possuído: aguardando a entrega em `entregas/tchu-tcha-berserk-possuido/`.
- Clipes do Devon (idle NE/SW) e da Teantha (walk/idle) no PixelLab: preciso confirmar que terminaram.
