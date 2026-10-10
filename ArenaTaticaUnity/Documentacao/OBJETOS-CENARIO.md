# Objetos de cenário — aba "objects"

Arena Tática. Vista isométrica, 128 px por casa, pixel art com contorno escuro de 1 px e luz vinda do noroeste.
Os 5 cenários são Pedreira, Bosque, Deserto, Neve e Vulcão. Cada objeto tem uma variante por cenário, que muda só o material e a cor; a silhueta continua a mesma.

**Regras:** dentro do tabuleiro, um objeto só pode representar os terrenos que já existem: Arbusto, Árvore ou Caixa. O que fica fora do tabuleiro é só enfeite.
Os itens marcados **(proposta)** criariam uma regra nova e dependem da decisão do autor. A arte pode ser feita antes; a regra, não.

**Camada:**
- **Y-sort:** a ordem depende da posição. O objeto fica na frente do lutador quando está mais perto da câmera.
- **Chão:** sempre atrás de tudo.
- **Topo:** sempre na frente, como fumaça e placas altas.

---

## 1. A Casinha da Loja

**Loja do Mascate:** uma loja de madeira de 2×2 casas que fica fora do tabuleiro, encostada na borda norte. Usa 256×256 px e a base tem 256×128.
- **Visual:**
  - Telhado de telhas vermelhas, um pouco torto, e chaminé de pedra soltando fumaça em espiral.
  - Toldo listrado de creme e vinho sobre um balcão aberto, com o vendedor atrás. O vendedor já foi entregue em `loja-v2/vendedor`.
  - Placa pendurada em correntes, com um saco de moedas pintado, balançando.
  - Lanterna amarela no canto do balcão, caixotes empilhados ao lado e um barril de poções coloridas.
  - Variantes por cenário:
    - Neve: neve no telhado e pingentes de gelo.
    - Deserto: lona de couro no lugar das telhas.
    - Vulcão: pedra escura e brasas na chaminé.
- **Colisão:** a base inteira de 2×2. O telhado não tem colisão.
- **Entrada:** o balcão fica na face sul e dá para o tabuleiro. O ponto de interação é a casa logo em frente ao balcão.
- **Camada:** Y-sort pela base. A fumaça fica no Topo.
- **Animação:**
  - Fumaça: 6 quadros em loop, a 6 fps.
  - Placa balançando: 4 quadros, a 4 fps.
  - Lanterna piscando: 3 quadros.
  - Toldo: estático.
- **Prompt (EN):** `small isometric wooden merchant shop, red tile roof, stone chimney with smoke, striped cream and wine awning over open counter, hanging coin-bag sign on chains, yellow lantern, crates and potion barrel, cozy fantasy, pixel art, dark outline`

---

## 2. Interativos / destrutíveis

Os estados são **inteiro → danificado → quebrado/aberto**, e cada estado ocupa a mesma célula de 128 px para manter a âncora no pé.

| Objeto | Descrição visual | Estados | Colisão / camada / animação |
|---|---|---|---|
| **Caixa de Suprimentos** (= terreno Caixa) | Caixote de tábuas com cintas de ferro e um selo de cera vermelho. Variantes: madeira clara na Pedreira, palha amarrada no Deserto, coberta de gelo na Neve. | inteiro, rachado, quebrado (tábuas soltas e palha no chão) | Bloqueia a casa e a visão, como a Caixa atual. Y-sort. A quebra tem 5 quadros. **Proposta:** a caixa ser destrutível. |
| **Barril de Pólvora** (proposta) | Barril escuro com aros de cobre, um pavio curto e uma caveira pintada. | inteiro, pavio aceso (faísca de 3 quadros), explodido (marca chamuscada) | Bloqueia a casa. Y-sort. A explosão tem 7 quadros. Dano em área depende do autor. |
| **Baú do Tesouro** (modo Tesouro) | Baú de carvalho com cantos de latão, fechadura em forma de olho e brilho dourado na fresta. | fechado, entreaberto (luz dourada), aberto vazio | Não bloqueia; fica sobre a casa. Y-sort. Brilho em loop de 4 quadros e abertura de 5 quadros. |
| **Poste-Lanterna** | Poste de ferro curvo com uma lanterna de vidro âmbar e mariposas em volta. | aceso, apagado, tombado (proposta) | Só a base de 1 casa bloqueia. Y-sort. Chama de 4 quadros e mariposas de 6. |
| **Pilar de Galeão** (modo Galeões) | Pedestal de pedra com uma moeda grande girando em cima. | ativo (gira), capturado (bandeira da cor do jogador), vazio | Não bloqueia. Y-sort. A moeda gira em 9 quadros; dá para reaproveitar a moeda entregue. |
| **Totem de Domínio** (modo Domínio) | Obelisco baixo com uma runa que brilha na cor de quem domina. | neutro (cinza), dominado (cor do jogador), disputado (pisca) | Bloqueia a casa. Y-sort. A runa pulsa em 6 quadros. |
| **Armadilha de Espinhos** (item da loja) | Placa de ferro com furos e espinhos que sobem. | escondida, armada, disparada | Chão. Disparo em 4 quadros. Já existe como ícone em `Loja/armadilha-de-espinhos.png`. |

---

## 3. Props de ambiente

### Vegetação (= terrenos Arbusto e Árvore, ou enfeite fora do tabuleiro)
- **Arbusto Redondo:** moita densa com folhas em 3 tons e algumas frutinhas vermelhas.
  - Variantes: arbusto seco espinhoso no Deserto, moita coberta de neve na Neve, arbusto de cinzas com brasas no Vulcão.
  - Bloqueia o movimento, como o Arbusto atual. Y-sort. Balanço de 4 quadros a 3 fps.
- **Árvore Alta:** tronco grosso com raízes aparentes e copa larga, ocupando 1 casa com 128×256 px.
  - Variantes: carvalho na Pedreira, árvore frutífera no Bosque, cacto saguaro no Deserto, pinheiro nevado na Neve, árvore carbonizada no Vulcão.
  - Bloqueia a casa e a visão. Y-sort pela base. As folhas caem em 6 quadros, só no Bosque.
- **Tufos de Flores:** 3 variações (amarelas, lilás e brancas), baixas, cobrindo ¼ da casa. Chão, sem colisão, estáticas.
- **Cogumelos:** um grupo de 3 cogumelos de chapéu vermelho com pintas, e uma variante azul que brilha à noite. Chão, sem colisão. A variante azul pulsa em 4 quadros.
- **Capim alto:** tufo que ondula com o vento. Chão, sem colisão, 4 quadros.

### Pedras e terreno
- **Pedra Grande:** rocha de granito com musgo no topo e rachaduras.
  - Variantes: arenito no Deserto, gelo azulado na Neve, obsidiana com veios de lava no Vulcão.
  - Bloqueia a casa, como Caixa ou Árvore conforme o autor decidir. Y-sort, estática.
- **Seixos:** 3 a 5 pedrinhas espalhadas. Chão, sem colisão.
- **Poça / fenda de lava** (Vulcão): fenda brilhante no chão que borbulha. Chão, sem colisão, 6 quadros. Só enfeite, fora do tabuleiro.

### Construções e caminho
- **Cerca de Madeira:** peças reta, de canto e quebrada, com mourões e duas ripas.
  - Variantes: cerca de osso no Deserto, cerca coberta de neve na Neve.
  - Só fora do tabuleiro, como borda do mapa. Y-sort, estática.
- **Placa de Direção:** poste com 2 ou 3 setas de madeira, uma delas apontando para a "Loja", e um corvo pousado.
  - Fica fora do tabuleiro. Y-sort. O corvo pisca e mexe a cabeça em 4 quadros.
- **Ruínas:** coluna partida, arco caído e um bloco com runas apagadas, para combinar com o terreno `ruinas.png` entregue. Fora do tabuleiro ou no lugar da Caixa. Y-sort.
- **Fogueira de Acampamento:** pedras em círculo, lenha e chamas. Fora do tabuleiro. Y-sort. Chama em 6 quadros.

### Detritos de chão (só Chão, sem colisão, estáticos)
- Ossinhos e um crânio pequeno (Deserto e Vulcão).
- Flecha quebrada fincada no chão.
- Tábua solta e pregos.
- Folhas secas (Bosque) e pegadas na neve (Neve).
- Marca de queimado. É também o rastro que fica depois da explosão do barril.

---

## Ordem sugerida de produção
1. Loja do Mascate.
2. Caixa (3 estados).
3. Arbusto e Árvore nos 5 cenários.
4. Pedra Grande.
5. Baú, Pilar de Galeão e Totem, para os modos de vitória.
6. Props de chão e enfeites de borda.
