# Arena Tática — design estilo Krosmaster (10/10/2026)

Decisões do autor nesta data + pseudocódigo do núcleo (Passo 3). Nada aqui é código final.

## Decisões do autor
1. Ciantia tem as skills **Dragão Dourado** (ataque básico) e **Invocação de dragão**.
2. Dragão Dourado: dragãozinho flutua na mão e arremessa bola de fogo pequena **em linha reta**.
3. Invocação: unidade com PV próprio; **máximo 1** ativa.
4. Times e turnos seguem as regras do Krosmaster.
5. Vários modos de vitória.
6. Sem progressão fora da partida, por enquanto. Moedas compram itens, magias, pets, armadilhas e buffs em uma **loja randomizada**.
7. Altura "igual Krosmaster". Ver *Pendente A*.
8. Rede: a melhor opção para jogar com amigos.
9. Sprites com a maior resolução possível. **Fica por último**; avisar se acabarem os créditos do PixelLab.
10. Mapa **12×12** (mudado pelo autor em 10/10; antes 24×24). Tamanho em `Board.Size`.
11. **Sem crítico.** No dado, a face de crítico vira **Cataclisma**: um evento aleatório no mapa.

## Escala (Krosmaster, substitui HP 100 / PA 2 do port Godot)
- Lutador: `PV ~8–14`, `PA 6`, `PM 3`, `Iniciativa`, `Nível` (custo em Galeões).
- Dado de 6 faces: `Cataclisma`, `Escudo`, `Trava`, `Esquiva`, `Coringa(Esquiva|Trava)`, `Coringa(qualquer exceto Cataclisma)`.

## Pseudocódigo do núcleo (C# puro, determinístico, roda no Host)

```
Estado: tabuleiro, lutadores[], linhaDoTempo, moedas[time], galeoes[time],
        lojaVisivel[time], rng(seed), modoVitoria, pilhaEventos

Celula { altura 0..3, tipo: Livre|Arbusto(bloqueia movimento, não visão)|Árvore(bloqueia ambos)
         |Caixa(bloqueia ambos, pode ser empurrada), moeda?, armadilha?(dono) }

INÍCIO
  linhaDoTempo = lutadores ordenados por Iniciativa, alternando times (regra Krosmaster)
  invocação entra logo após o invocador

TURNO(lutador)
  restaura PA/PM; aplica efeitos de início (buff, veneno, armadilha)
  enquanto ação:
    MOVER(caminho):   cada passo custa 1 PM (+1 PM se subir 2+ níveis)
                      ao sair de casa adjacente a inimigo -> TRAVA
                      ao entrar em casa com moeda -> coleta; com armadilha inimiga -> dispara
    SKILL(skill, alvo): valida PA, alcance, linha reta, linha de visão, usos por turno
                      -> RESOLVE_EFEITOS
    COMPRAR(slot):    custo em moedas; 1 compra por turno do lutador
    PASSAR
  fim: renova a loja só para slots comprados; checa VITÓRIA

TRAVA(saindo, inimigosAdj)
  para cada inimigo: dado do inimigo == Trava/Coringa? e dado do saindo != Esquiva/Coringa?
     -> saindo perde todo PA e PM

ROLAGEM(dados) -> faces
  para cada face Cataclisma: CATACLISMA()   // no máximo 1 por ação

ATAQUE(dano base, alvo)
  rola N dados do atacante (Cataclisma não dá dano extra)
  alvo rola dados de defesa: cada Escudo reduz 1
  dano = max(0, base - escudos) -> PV; PV<=0 -> KO: time perde Galeões = Nível

CATACLISMA()
  sorteia em tabelaCataclismas (dados JSON, com peso), por exemplo:
  chuva de moedas, terremoto (muda alturas), chamas em linha, nevoeiro (corta visão 1 turno),
  queda de pedras (cria caixas), portal (troca 2 lutadores)

LOJA (randomizada por time, 3 a 4 slots)
  categorias: Item | Magia (skill extra, 1 uso) | Pet (unidade pequena) | Armadilha | Buff
  sorteio com pesos e raridade; preços em moedas
  Moedas: spawn inicial + spawn a cada rodada em casas aleatórias livres

VITÓRIA (escolhida no lobby, regras plugáveis)
  Galeões: time chega a 0 Galeões (padrão Krosmaster)
  Eliminação: nenhum lutador vivo
  Tesouro: acumular X moedas
  Domínio: controlar zonas por N rodadas
```

## Ciantia (números iniciais; balancear depois)
- PV 11 · PA 6 · PM 3 · Ini 5 · Nível 4
- **Dragão Dourado**: 3 PA, alcance 1–4 em linha reta, exige visão, 2 de dano de fogo, alcance modificável pela altura.
- **Invocação de dragão**: 4 PA, alcance 1–2, máximo 1 ativa. Dragão espectral: PV 6, PA 4, PM 4, voa (ignora altura e arbustos), Garra 2 PA, 2 de dano corpo a corpo.

## Rede
- O protocolo atual permanece: o Host decide tudo; comandos sobem, eventos e o estado descem.
- **Amigos pela internet:** Unity Relay + Lobby com código de sala. Funciona sem abrir portas e também no Web e no Android.
- **LAN:** WebSocket direto, como hoje.
- O RNG roda só no Host. O cliente recebe as faces já sorteadas.

## Decididas pelo autor (10/10)
- **A. Altura:** quem está mais alto que o alvo ganha +1 de alcance (skills de alcance modificável). Relevo 2+ níveis acima do mais baixo dos dois bloqueia a visão. Subir 2 níveis custa 2 PM; 3+ níveis não dá para subir.
- **B. Cataclisma:** só no dado de ataque.
- **C. Moedas:** pegas ao passar pela casa.

## Estado da implementação
- Núcleo em `Assets/ArenaTatica/Scripts/Core/`, testes em `Tests/Editor/CoreMatchTests.cs` (18 testes do núcleo).
- **Loja (`Core/Shop.cs`, padrões provisórios do Claude):** 3 slots por time, sorteio com peso e sem repetir; comando `comprar` (campo `slot`), sem custo de PA, 1 compra por turno do lutador, invocações não compram; o slot comprado renova no fim do turno.
  - Item (permanente, vale na hora): Botas aladas +1 PM (4), Escudo de bronze +1 armadura (5), Amuleto vital +3 PV (3), Luneta +1 alcance em skills modificáveis (4).
  - Magia (1 uso): Raio arcano 2 PA, 1–5, 3 de dano (3); Poção de cura 1 PA, 0–1, cura 4 até o máximo (2).
  - Pet: Lobo de cinzas (4): skill de 1 uso, 2 PA, invoca lobo PV 3/PA 3/PM 3, Mordida 1. Limite de invocação é por tipo (lobo convive com o dragão).
  - Armadilha de espinhos (2): 1 PA, alcance 1–3, casa livre. Inimigo que entra leva 2 (menos armadura), perde os PM e a armadilha some. Dono e voadores passam.
  - Buff (dura este turno e o próximo): Fúria +2 PA (3), Passo leve +2 PM (2).
  - Armadura reduz todo dano (ataque e armadilha). Nocauteado no próprio turno encerra o turno.
- **Passo 4 feito (10/10):** cena, HUD e rede usam `Core.Match` (protocolo 2, estado filtrado por time: sem RNG e sem armadilhas do outro time). `ArenaRules` e seus testes saíram. Validação: 18/18 EditMode, `Tools/smoke.mjs` (local, host, cliente) e `Tools/network-contract.mjs` (9 cenários) passando.
- **Mapa 12×12 (10/10):** 2 platôs 2–3 casas, 3 árvores, 3 arbustos e 2 caixas (cada um espelhado), zona central 2×2, lutadores nas casas x 2/4/7/9 da linha 1 (espelhadas para o time 2). Moedas: 2 pares no início, 1 par por rodada, Chuva de moedas 1 par, Queda de pedras 1 par de caixas. Números provisórios.
- **Passo 5 (10/10):** sala pela internet com Unity Relay (sem Lobby; código = join code de 6 letras), código no `ArenaTransport.cs` (bloco `partial` no fim do arquivo: o Editor aberto não incluía arquivo .cs novo na compilação). HUD: Host LAN, Criar sala, campo com código ou ws://. Compila; 18/18, smoke e contrato LAN passando. **Testado em 10/10:** dois exe (`--arena-room` / `--arena-room-join CÓDIGO`) conectaram pelo Relay (RTT ~150–200 ms), snapshot sincronizado e movimento do host refletido no cliente. Corrigido o cursor desalinhado: `Pick` ainda usava o meio do mapa 24×24 (`+12`); agora `Board.Size*.5f`. **Reconexão testada (10/10):** cliente morto e reaberto com o mesmo código recebeu o snapshot com o estado atual e seguiu recebendo os comandos. Para isso a alocação passou a ter 2 vagas (o Relay ainda conta o P2 antigo até expirar) e o host troca a conexão antiga pela nova. 18/18 EditMode.
- **Passo 5b (10/10): até 6 jogadores.** Sala com 2 a 6 assentos e formato escolhido pelo host: todos contra todos, 2v2 (4), 3v3 ou 2v2v2 (6). Jogador = assento (lutadores, moedas e loja próprias); time = Galeões, coleta, domínio e vitória divididos (`MatchState.alliance`). Mapa 12×12 (2), 16×16 (3–4), 18×18 (5–6), ainda simétrico em 180°; spawns em pares espelhados nas bordas, aliados do mesmo lado (2v2v2: esquerda, direita e centro, provisório). Turnos em rodízio entre times pela Iniciativa. Time sem lutadores ou sem Galeões sai (os lutadores restantes saem do tabuleiro); vence o último de pé ou quem bate a meta. Rede (protocolo 3): cliente manda `hello{token}` (token salvo por sala), host dá o assento, começa sozinho quando a sala enche ou em Iniciar (assentos vazios saem); quem volta com o mesmo token recupera o assento. Relay com (assentos−1)×2 vagas. Testado: 24/24 EditMode, smoke, contrato LAN (10) e sala Relay com 3 exe (turnos P1→P2→P3 e reconexão do P3, rodada 2 voltando ao P1). HUD a 960×540 conferido com 6 jogadores (lista, placar de 6 e placar por time no 3v3). `--arena-seats N` vale para a próxima partida (clicar Nova local ou Criar sala).
- Próximo: sprites do PixelLab. Sem IA de oponente por enquanto (jogo local é hotseat).

## Passo 4: ligar o núcleo (pseudocódigo, 10/10)
```
ArenaGame (Host/Local tem Core.Match; Cliente só tem Core.MatchState)
  timeLocal = Cliente?2 : Host?1 : Ativo.time (hotseat)
  podeAgir = sincronizado && !recuperação && !fim && Ativo.time==timeLocal && !View.Ocupada
  Enviar(cmd{mover|skill|comprar|passar, fighter=Ativo.id, revision}) -> Cliente: rede; senão Executar(cmd, timeLocal)
  Executar: match.Apply -> eventos -> View.Tocar(eventos, estado) -> Host envia "state" filtrado ao cliente
  Filtro por time: rng=0, armadilhas e "armadilha-posta" do outro time removidas
  Conexão: Host manda "snapshot" filtrado; cliente valida (12×12, 144 casas, times 1/2, casas no mapa)
  Protocolo 2: WireMessage{kind, state, command, events[]}
View
  tabuleiro: malha combinada por altura/terreno, refeita quando o relevo muda (Cataclisma)
  moedas, zona central e armadilhas próprias: marcadores refeitos a cada estado
  fila de eventos: mover(caminho com altura), ataque(dados+dano), cura, trava, moeda, armadilha,
                   invocar, nocaute, cataclisma(refaz), compra/loja, turno/rodada, vitória
  destaque: modo Mover = casas alcançáveis pelos PM; modo skill = alcance (claro) e alvos válidos (forte)
HUD
  topo: rodada, lutador ativo, PA/PM, moedas e Galeões dos times
  lateral: lutadores, Mover, botões das skills (próprias + cargas), 3 slots da loja, Passar
Sem IA de dragões por enquanto (era do protótipo antigo); ArenaRules e seus testes saem.
```

## Passo 5: sala pela internet (pseudocódigo, 10/10)
```
Pacote: com.unity.services.multiplayer 2.4.0 (Relay + Autenticação) + Unity Transport (módulo do Unity 6.6).
Sem Lobby: o código da sala é o join code do próprio Relay (6 letras). Menos serviços e nada de heartbeat de Lobby.
ArenaTransport (mesma interface: Connected/Disconnected/Message/Failed, Send, Close) ganha o modo Relay:
  Entrar nos serviços: UnityServices.Initialize + login anônimo (uma vez)
  CriarSala: Relay.CreateAllocation(2 conexões: 1 vaga extra para o P2 voltar) -> código; driver UTP com RelayServerData
             ("dtls" no Windows/Android, "wss" no navegador); Bind + Listen; mostra o código na HUD
  EntrarNaSala(código): Relay.JoinAllocation(código) -> driver -> Connect
  Canal: Fragmentação (até 48 KB) + Confiável em ordem (janela 64); o JSON do protocolo 2 segue igual
  Update: driver.ScheduleUpdate; Host aceita 1 conexão (a 2ª é recusada); eventos Connect/Data/Disconnect
  Queda do P2: Host segue com a sala aberta; P2 entra de novo com o mesmo código (mesma recuperação da LAN)
  Close: desconecta e libera o driver
HUD: "Host LAN" (WebSocket, porta 9080) + "Criar sala"; o campo aceita endereço ws:// ou código de 6 letras;
     "Entrar" decide pelo formato. No navegador também dá para criar sala (wss).
Linha de comando: --arena-room (cria) e --arena-room-join CÓDIGO.
Requisito: projeto ligado ao Unity Cloud (Project Settings > Services) com Relay ativo no painel.
```
