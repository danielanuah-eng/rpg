# 015 — Correções de animação, céus dinâmicos, eventos, loja e elenco novo (pedido do autor, 10/10)

**De:** usuario (via claude)
**Para:** codex
**Status:** aberta
**Responsável:**
- **codex:** sprites, animações, efeitos, ícones e céus no PixelLab e nas ferramentas dele.
- **claude:** regras, gatilhos e integração no Unity.

**Design completo:** `ArenaTaticaUnity/Documentacao/EVENTOS-LOJA-ELENCO.md`. Ids, números e efeitos estão lá. A arte não define regra.

## 1. Correções de animação (PRIORIDADE 1)

**Arqueiro Jack, Tchu tcha (e Berserk), Devon e Teantha**
- Revisar sprites e movimentos em todos os clipes e nas 4 diagonais.
- Procurar estes defeitos:
  - membro sumindo ou trocando de lado;
  - arma trocando de mão;
  - quadro pulando;
  - pé fora da linha;
  - pose incoerente.
- Se já estiver dentro da 014, registre lá e marque aqui.

**Damian — `hammer_south-east.png` (crítico)**
- O martelo é puxado das costas com o movimento errado ou com defeito.
- Refaça só esse clipe, mantendo o mesmo número de quadros das outras 3 direções. O código assume um tempo comum entre as direções.
- Pasta: `Assets/ArenaTatica/Resources/Sprites/Damian/`.

## 2. Céus dinâmicos (planos de fundo pela cena e pelo evento)

Hoje o céu é uma cor sólida por cenário (`ArenaPresentation`, paleta de `MapGen.Themes`: Pedreira, Bosque, Deserto, Neve, Vulcão). Crie fundos em camadas.

**Formato:** `Resources/Sprites/Ceus/<id>/fundo.png` (2048×1024) + `nuvens.png` (2048×512, com alfa, para rolar devagar).

| id | quando aparece |
|---|---|
| `ceu-aberto` | padrão Bosque/Pedreira; chuva de moedas (versão dourada `ceu-dourado`) |
| `nuvens-densas` | Neve; nevoeiro |
| `cinzas-vulcanicas` | Vulcão; maré de lava |
| `deserto-poente` | Deserto |
| `tempestade` | chegada do Brutamontes Errante |
| `noite-festiva` | festival de fogos |

O Claude faz a troca com fade (evento `cataclisma` → céu do evento; `evento-fim` → céu do cenário).

## 3. Eventos de mapa (efeitos)

**Formato:** `Resources/Sprites/Efeitos/<id>/` com tiras horizontais de quadros quadrados:
- `inicio.png` — invocação do evento;
- `loop.png` — repetível enquanto dura;
- `fim.png`.

O tamanho do quadro fica a critério de vocês. **Informe o tamanho do quadro e o quadro do impacto** na resposta.

| id | o que animar |
|---|---|
| `mare-de-lava` | por casa: rachadura → lava brilhando (loop com bolhas e fumaça) → resfriando |
| `chuva-de-moedas` | moedas caindo e quicando |
| `queda-de-pedras` | sombra crescendo, rocha caindo e poeira |
| `terremoto` | rachaduras e blocos subindo |
| `portal-caotico` | runas roxas no chão, vórtice e reaparição |
| `eco-ancestral` | pilar de luz dourada sobre o lutador |
| `mare-de-energia` | aura azul-esverdeada subindo |
| `festival-de-fogos` | rojão subindo, marcador no chão piscando (loop) e explosão colorida em área 3×3 |
| `nevoeiro` | névoa rasteira em loop, ladrilhável |
| `vendaval` | rajadas em linha, folhas e poeira |

## 4. Loja: ícones e animação de uso

- **Ícone:** `Resources/Sprites/Loja/<id>.png`, no mesmo tamanho dos ícones atuais.
- **Uso:** `Resources/Sprites/Efeitos/<id>/uso.png`.

| id | ícone | animação de uso |
|---|---|---|
| `martelo-perfurante` | martelo de cristal | martelo atravessando um escudo partido |
| `corrente-de-ferro` | corrente | corrente voando, engatando e puxando |
| `bomba-de-fumaca` | bomba | nuvem cinza e silhueta sumindo |
| `pena-de-fenix` | pena em chamas | pena subindo e renascimento nas cinzas |
| `totem-de-cura` | totem | totem de madeira com runas verdes pulsando, em loop. Também vira **objeto no mapa**, com idle e quebra. |
| `mina-de-gelo` | mina | cristais brotando e prendendo os pés |
| `gancho-de-escalada` | gancho | só o ícone |
| `cristal-de-foco` | cristal | brilho roxo na arma |
| `manto-de-espinhos` | manto | espinhos saltando |
| `ancora` | âncora | âncora fantasma caindo aos pés |
| `bencao-de-pedra` | bênção de pedra | pele virando pedra |

## 5. Lutadores novos (PixelLab, 128 px; Colosso e chefe a 192 px)

**Clipes base:** idle, walk, attack, hit e death. São 4 diagonais, no mesmo formato da 014.

| Pasta | Assinaturas extras |
|---|---|
| `Asamyh` | `summon` (gato surgindo), `heal` (aura de gatinho), `ult` (Noite dos Gatos) |
| `Gato` com 4 estados de cor: laranja, branco, preto e branco, marrom com manchas pretas | idle, walk, `zap` (dispara pela varinha e some), death |
| `Colosso` | `steal-reach` (estica o braço enorme para a frente), `steal-slam` (bate no chão), `steal-cast` (conjura com os dois braços), `stomp` |
| `CheiroPneu` | `tentacle` (óleo em volta), `charge` (caminhão fumegando), `cannon` (disparo do peito), `drill` (gancho virando broca) |
| `ReiBobett` | o attack tira a coroa e bate com ela |
| `PrincipeKellen` | `ice`, `fireball`, `prison`, `wish-channel` (loop), `wish` |
| `VespaCinzenta` | voa |
| `ViuvaAgulha` | — |
| `GuardiaoMusgo` | `bark` (casca), `roots` |
| `DomadorCorrentes` | `chain`, `shout` |
| `OgroPedra` | `throw` |
| `PapaMoedas` | — |
| `BrutamontesErrante` | `roar`, `spawn` (saindo do portal) |

**Projéteis e efeitos de skill:** em `Resources/Sprites/Projeteis/<skill-id>.png` e `Efeitos/<skill-id>/`:
- bola de fogo;
- disparo do gato;
- tentáculo de óleo;
- canhão de peito;
- chão de gelo, por casa, em loop;
- prisão dos pesadelos (estrutura com idle e quebra);
- aura de gatinho;
- corrente.

As descrições visuais completas dos 5 personagens do autor estão em `EVENTOS-LOJA-ELENCO.md`, seção 3. Siga a aparência ao pé da letra.

## 6. Regra do autor: o Codex faz TODOS os objetos, animações e efeitos

Nada do que está em `EVENTOS-LOJA-ELENCO.md` fica sem arte. O Claude não substitui arte por partícula de código. A lista abaixo completa as seções 3 a 5.

**Objetos no mapa**
`Resources/Sprites/Objetos/<id>/` com idle (loop), surgir e quebrar ou sumir:
- casa de lava e casa resfriada;
- casa congelada (chão de gelo);
- marcador do festival de fogos;
- mina de gelo armada, vista só pelo dono;
- totem de cura;
- prisão dos pesadelos;
- baú de recompensa do chefe;
- portal de rocha do chefe;
- buraco do papa-moedas.

**Cada skill nova**
Cada skill dos lutadores novos, extras e neutros tem:
- clipe de conjuração;
- projétil, se houver;
- impacto.

As skills são:
- Asamyh: Faísca da varinha, uma por cor de gato;
- Pisão;
- Pancada colossal;
- Gancho;
- Coroada;
- Pauzinho;
- Ferrão;
- Agulha;
- Casca;
- Raízes;
- Corrente;
- Grito de comando;
- Clava;
- Arremessar pedra;
- Pancada e Rugido do chefe.

**Estados no lutador**
Em loop sobre o sprite:
- escondido;
- preso;
- sem PM (óleo ou raízes);
- +armadura (casca ou bênção de pedra);
- Desejo canalizando;
- buff de PA/PM.

**Desejo do Kellen**
Um efeito para cada uma das 6 opções:
- A, reviver;
- B, cura total;
- C, PA dobrado;
- D, copiar skill;
- E, itens caindo do céu;
- F, evento sendo apagado.

**Devorar técnica (Colosso)**
O efeito de absorver a skill do nocauteado: uma essência saindo do corpo e entrando no Colosso.

## Formato e regras
- **Pé:** na mesma linha em todos os quadros (o pivô é o pixel opaco mais baixo). As tiras são `<clipe>_<direção>.png`, com quadros quadrados.
- **PixelLab:** antes de gerar, confira com `get_character` o que já existe. Os IDs novos vão em `ArenaTaticaUnity/Tools/pixellab-ids.json`. **Se os créditos acabarem, avise.**
- **Prioridade:**
  1. §1;
  2. Damian;
  3. céus;
  4. eventos;
  5. personagens do autor (Asamyh, Colosso, Cheiro, Bobett, Kellen), com todas as skills e efeitos;
  6. objetos do mapa e ícones e usos da loja;
  7. fichas extras e neutros.
- **Tudo da lista é obrigatório.** A ordem é só a de entrega.

**Resposta:** `respostas/015-codex.md`, com o que mudou, os caminhos, os hashes, o tamanho do quadro e o quadro do impacto de cada clipe de ataque e evento, além das pendências.
