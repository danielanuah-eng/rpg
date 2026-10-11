# Arena Tática — eventos, loja e novos lutadores (10/10/2026)

Pedido do autor de 10/10. Este arquivo é **design + pseudocódigo**. Nada foi implementado ainda (protocolo: código só depois da autorização).
Escala: a mesma de `DESIGN-KROSMASTER.md` (PV 7–18, PA 6, PM 2–6, dano 1–5, time com soma de Nível ≤ 12). Todos os números são provisórios.
Arte e animação: pedidas ao Codex em `coordenacao/arena-pixelai/tarefas/015-codex-eventos-elenco-cenarios.md`. Os ids abaixo são os mesmos de lá.

---

## 1. Eventos de mapa (Cataclisma)

### Regras gerais
- **Gatilho:** a face Cataclisma no dado de ataque (decisão 11/B), como hoje.
- **Sorteio:** com peso, numa tabela única (`Core/Events.cs`). Os 3 atuais (chuva de moedas, queda de pedras e terremoto) continuam na tabela.
- **Tipos:**
  - *instantâneo:* resolve na hora;
  - *com duração:* fica ativo por N rodadas e age no fim de cada rodada.
- **Limite:** no máximo **1 evento com duração** ativo. Se sair outro enquanto um está ativo, o sorteio é refeito só entre os instantâneos.
- **Simetria:** tudo que cria terreno ou casas perigosas nasce em par espelhado, para não favorecer um lado.
- **Céu:** cada evento troca o céu da cena enquanto durar (ver a seção 4). Os instantâneos trocam só durante a animação.
- **Rede:**
  - evento `cataclisma{info=id, path=casas, value=rodadas}` no início;
  - `evento-tick{info=id, path}` a cada rodada;
  - `evento-fim{info=id}` no fim.

### Tabela
| id | tipo | peso | efeito | visual (para o Codex) |
|---|---|---|---|---|
| `chuva-de-moedas` | inst. | 3 | 3 pares de moedas (hoje é 1) | moedas caindo do céu, quicando e brilhando; céu aberto dourado |
| `queda-de-pedras` | inst. | 2 | 1 par de caixas (atual) | rochas caindo com sombra crescendo antes do impacto e poeira |
| `terremoto` | inst. | 2 | 3×3 sobe ou desce 1 (atual) | chão tremendo, rachaduras e blocos subindo |
| `mare-de-lava` | 3 rodadas | 2 | Nasce em 1 par de casas de altura 0, a 2 ou mais casas dos lutadores. No fim de cada rodada, espalha para 2 vizinhas de altura 0 (em espelho). Quem começa o turno ou passa por lava leva **2 de dano de fogo**; a armadura reduz. A moeda na lava derrete. Ao terminar, a lava esfria e a casa volta a ficar livre. | chão rachando com brilho laranja, bolhas e fumaça; ao esfriar, fica cinza com fissuras apagando; **céu de cinzas vulcânicas** |
| `portal-caotico` | inst. | 2 | Sorteia **2 a 4** lutadores vivos, sem invocações, e troca as posições entre eles. | runas roxas sob os pés, vórtice e os lutadores reaparecendo trocados |
| `eco-ancestral` | inst. | 1 | **Reativa as ultimates:** limpa usos por partida e zera recargas de todos. O Desejo do Kellen fica de fora. | pilar de luz dourada sobre cada lutador e ícone de ultimate acendendo |
| `mare-de-energia` | inst. | 2 | Todos ganham **+1 PA e +1 PM** no próximo turno (buff de 1 turno). | aura azul-esverdeada subindo dos pés |
| `festival-de-fogos` | 1 rodada | 2 | Marca **3 pares de casas**. No fim da rodada, cada uma explode em área 1, com **2 de dano**. O Escudo do dado de defesa reduz, e aliados também são atingidos. Dá tempo de sair. | rojão subindo, marcador piscando no chão e explosão colorida em leque; noite com nuvens coloridas |
| `nevoeiro` | 1 rodada | 2 | O alcance máximo de todas as skills cai em **2** (mínimo 1). | névoa rasteira cobrindo o tabuleiro; **nuvens densas** |
| `vendaval` | inst. | 2 | Empurra todos os lutadores **1 casa** numa direção sorteada. Quem bate em obstáculo leva 1 de dano. | rajadas de vento em linhas, folhas e poeira |
| `papa-moedas` | até morrer | 2 | Surgem **2 Papa-moedas neutros** (ver a seção 3). Eles fogem e comem moedas. | bichinho redondo saindo de um buraco no chão |
| `errante` | até 5 rodadas ou até morrer | 1 | Surge o **Brutamontes Errante**, um chefe neutro (ver a seção 3). Ao fim das rodadas, ele vai embora. | portal de rocha, o chefe saindo dele e o chão tremendo a cada passo; **céu de tempestade** |

### Pseudocódigo
```
CATACLISMA()
  pool = tabela; se S.event ativo: pool = só instantâneos
  ev = sorteio com peso (pool)
  ev.Inicio(S) -> casas; se tem duração: S.event={id, rodadas, casas}
  Emit cataclisma

FIM_RODADA (antes das moedas da rodada)
  se S.event: Def(S.event).Tick(S); S.event.rodadas--; Emit evento-tick
              se rodadas==0 ou condição de fim: Def.Fim(S); S.event=null; Emit evento-fim
  NEUTROS(): cada neutro vivo age (seção 3)
```

---

## 2. Loja: itens novos

Somados aos 10 atuais. Os pesos baixos são raros. O ícone e a animação de uso de cada um foram pedidos ao Codex.

| id | tipo | preço | peso | efeito | animação de uso |
|---|---|---|---|---|---|
| `martelo-perfurante` | Magia, 1 uso | 4 | 2 | 3 PA, alcance 1, **3 de dano que ignora armadura e Escudo** (flag nova `pierce`) | martelo de cristal atravessando um escudo partido |
| `corrente-de-ferro` | Magia, 1 uso | 3 | 2 | 2 PA, alcance 2–4 em linha, **puxa o alvo** até ficar colado (`pull`) | corrente voando, engatando e puxando |
| `bomba-de-fumaca` | Magia, 1 uso | 3 | 2 | 1 PA, **fica escondido** mesmo longe de relevo (até agir) | nuvem cinza e silhueta sumindo |
| `pena-de-fenix` | Magia, 1 uso | 6 | 1 | 2 PA, **revive um aliado nocauteado** com 4 PV numa casa vizinha. Não devolve Galeões. | pena em chamas subindo e lutador renascendo das cinzas |
| `totem-de-cura` | Pet | 4 | 2 | Invoca um totem **imóvel**, com PV 4 e 0 PM. No início do turno do dono, cura 1 os aliados a até 2 casas. | totem de madeira com runas verdes pulsando |
| `mina-de-gelo` | Armadilha | 3 | 2 | Quem entra **perde os PM** e fica com **−2 PA no próximo turno**. Não causa dano. | cristais de gelo brotando e prendendo os pés |
| `gancho-de-escalada` | Item | 3 | 2 | Subir 2 níveis custa só 1 PM, e dá para subir 3. | gancho preso na borda |
| `cristal-de-foco` | Item | 5 | 1 | A **primeira** skill de dano de cada turno causa +1 | brilho roxo na arma no primeiro golpe |
| `manto-de-espinhos` | Item | 4 | 2 | Quem acerta você em corpo a corpo leva 1 | espinhos saltando do manto |
| `ancora` | Item | 3 | 2 | Não pode ser empurrado, puxado nem trocado de lugar, nem por inimigos nem por eventos | âncora fantasma caindo aos pés |
| `bencao-de-pedra` | Buff | 3 | 2 | +2 de armadura neste turno e no próximo | pele virando pedra |

---

## 3. Lutadores novos

Mecânicas novas que o núcleo precisa ganhar estão marcadas com ⚙.

### Asamyh — suporte, **Lendária**
Maga morena de óculos, com chapéu de bruxinha e traje verde e prata.
- **Atributos:** PV 9 · PA 6 · PM 3 · Ini 4 · **Nível 5**.
- **Lendária:** no máximo 1 Lendária por time. ⚙ Nova regra no seletor.

**Gatos ⚙ (invocação comandada)**
- O gato **não tem turno**. A Asamyh gasta os próprios PA nas ordens.
- Cada gato tem **PV 1**, pode ser atacado e não segura trava.
- Pode haver **até 2 gatos**.
- A cor é sorteada e muda o efeito do disparo:

| Cor | Efeito do disparo |
|---|---|
| laranja | 2 de dano |
| branco | cura 2 um aliado |
| preto e branco | 1 de dano, e o alvo perde 2 PM |
| marrom com manchas pretas | 1 de dano e empurra 1 casa |

**Skills**
- **Chamar gato:** 2 PA, alcance 1–3, 2 por turno.
- **Mover gato:** 1 PA para andar 2 casas. É um comando sobre o gato.
- **Faísca da varinha:** 2 PA. O gato dispara a 1–3 casas dele, em linha de visão. **Depois do disparo o gato some, acertando ou errando.**
- **Aura de gatinho:** 3 PA, alcance 0–5, **cura 3** num aliado ou nela mesma, 1 por turno. Visual: um gatinho de luz corre até o aliado.
- **Ultimate — Noite dos Gatos:** 4 PA, 1 por partida.
  - Aliados a até 3 casas curam 2 e ganham +1 de armadura por 1 turno.
  - Ela invoca 2 gatos de graça. Se já houver gatos, eles são substituídos.

### O Colosso — tanque
Fera gigantesca com baixa movimentação. A estética vem do Primal Beast. O sprite tem 192 px, mas ele ocupa **1 casa**.
- **Atributos:** PV 18 · PA 6 · PM 2 · Ini 1 · **Nível 6**.
- **Passiva — Massa:** imune a empurrão, puxão e troca de lugar.
- **Pancada colossal:** 4 PA, alcance 1, 4 de dano.
- **Pisão:** 4 PA, área 1 em volta dele, 2 de dano, 1 por turno.

**Passiva — Devorar técnica ⚙**
- Ao nocautear um inimigo que não seja invocação, ganha **para sempre** a skill principal dele, a primeira skill de dano.
- Não copia ultimates.
- Guarda no máximo 2 skills roubadas. A terceira substitui a mais antiga.
- A skill copiada mantém os números do dono, com **+1 PA** de custo.

**Visual da passiva:** as partículas são as da skill original, mas a conjuração usa um de três clipes do Colosso:

| Clipe | Usado em |
|---|---|
| `steal-reach` (estica o braço para a frente) | projétil, linha ou alvo a distância |
| `steal-slam` (bate no chão) | área, armadilha ou alvo colado |
| `steal-cast` (conjura com os dois braços) | cura, forma, teleporte ou invocação |

### Cheiro D.Pneu Queimado — tanque corpo a corpo
Pirata alto, cheio de óleo, com tronco imenso e perninhas finas. Metade do corpo é carne queimada e a outra metade é esqueleto. Tem um gancho gigante, um caminhão atravessado na barriga, o gorro do Kuma e os óculos do Doflamingo.
- **Atributos:** PV 14 · PA 6 · PM 3 · Ini 2 · **Nível 5**.
- **Gancho:** 3 PA, alcance 1, 3 de dano.
- **Tentáculo de óleo:** 3 PA, área 1 em volta dele (as 8 casas), 2 de dano, 1 por turno. Quem for atingido **perde 2 PM** no próximo turno (`pmLoss 2`).
- **Canhão de peito, em 2 tempos:**
  - *Aquecer motor:* 2 PA, forma `motor-quente` por 1 turno. O caminhão solta fumaça.
  - *Canhão de peito:* no turno seguinte, 4 PA, alcance 2–5 em linha, **4 de dano**. Encerra a forma.
  - O padrão é o mesmo das Asas do Jack.
- **Ultimate — Gancho-broca ⚙ (empurrão):** 5 PA, alcance 1, 4 de dano, recarga de 4 turnos.
  - O alvo voa **4 casas** em linha.
  - Se bater em obstáculo ou em outro lutador, leva +1, e o atingido também leva 1.

### Rei Bobett — básico e defensivo
Rei gordo e narigudo, de barba, com coroa de joias rosas.
- **Atributos:** PV 12 · PA 6 · PM 3 · Ini 3 · **Nível 2**. É barato e ajuda a completar o time.
- **Coroada:** 3 PA, alcance 1, 2 de dano. Ele tira a coroa e bate com ela.
- **Passiva — Barriga real:** ignora 1 de dano de **qualquer** fonte inimiga, inclusive armadilha e lava inimiga. Equivale a armadura 1 fixa, que soma com itens.

### Príncipe Kellen — mago, glass cannon
Gnomo baixinho e branco, de roupa real, com cachimbo de ouro, anéis, bigodinho, barbicha e topete enrolado.
- **Atributos:** PV 7 · PA 6 · PM 3 · Ini 6 · **Nível 4**.
- **Pauzinho:** 2 PA, alcance 1, 1 de dano.
- **Chão de gelo ⚙ (casa congelada):** 3 PA, alcance 1–4, área 1 em cruz, 1 por turno.
  - As casas ficam congeladas até o início do próximo turno do Kellen.
  - Entrar nelas custa **+1 PM**, para aliados e inimigos.
- **Bola de fogo:** 4 PA, alcance 2–5, alcance modificável, 3 de dano em área 1.
- **Prisão dos pesadelos ⚙:** 3 PA, alcance 1–3, recarga de 3.
  - O alvo rola 1 dado e, com Esquiva, escapa.
  - Se não escapar, fica preso numa estrutura durante **o próximo turno dele**: não anda, não usa skills e **não pode ser alvo**.

**Ultimate — Desejo ⚙**
- **Disponibilidade:** 1 vez por partida, a partir da **rodada 10** (`minRound`; decisão do autor em 10/10, antes 15).
- **Uso:**
  1. No 1º turno, o Kellen usa todo o PA para começar o Desejo.
  2. No 2º turno, segue canalizando e só pode passar.
  3. No início do 3º turno, ele escolhe uma opção.
- **Interrupção:** se ele for nocauteado durante a canalização, o Desejo se perde.
- **Opções:**
  - **A:** revive um aliado com metade dos PV numa casa vizinha.
  - **B:** cura 100% de qualquer lutador.
  - **C:** um aliado ganha PA dobrado e +1 uso de cada skill por turno no próximo turno.
  - **D:** copia para sempre a skill mais forte (não ultimate) de um lutador do mapa, menos o Desejo.
  - **E:** os 2 produtos mais caros da loja dele, de graça e à distância, para ele ou um aliado (a loja se renova).
  - **F:** cancela o evento com duração ativo. A opção F fica indisponível se não houver evento.

### Fichas extras (variedade de mecânicas)
| Lutador | Nível | Atributos | Mecânica |
|---|---|---|---|
| **Vespa Cinzenta** (alta mobilidade) | 2 | PV 6 · PA 6 · PM 6 · Ini 9 · voa | **Ferrão:** 2 PA, alcance 1, 1 de dano, 2 por turno. **Zunido:** quem ela acerta perde 1 PM. |
| **Viúva da Agulha** (ignora armadura, posicional) | 4 | PV 9 · PA 6 · PM 4 · Ini 7 | **Agulha:** 3 PA, alcance 1–2, 2 de dano `pierce`. **+2 de dano** se o alvo não tiver aliado colado (isolado). |
| **Guardião de Musgo** (buff de defesa, controle) | 4 | PV 13 · PA 6 · PM 2 · Ini 3 | **Casca:** 2 PA, alcance 0–3, +2 de armadura num aliado por 1 turno. **Raízes:** 3 PA, alcance 1–3, 1 de dano, e o alvo fica com 0 PM no próximo turno, com recarga de 2. |
| **Domador de Correntes** (controle de grupo) | 5 | PV 11 · PA 6 · PM 3 · Ini 4 | **Corrente:** 3 PA, alcance 2–4 em linha, 2 de dano e puxa. **Grito de comando:** 4 PA, área 2 em volta dele, inimigos perdem 2 PM e −1 PA no próximo turno, 1 por partida, ultimate. |
| **Ogro de Pedra** (baixa mobilidade, muito PV) | 3 | PV 16 · PA 6 · PM 1 · Ini 0 | **Clava:** 4 PA, alcance 1, 3 de dano. **Arremessar pedra:** 4 PA, alcance 2–4, 2 de dano. |

### Neutros (eventos) ⚙ time 0 com IA simples, que age no fim da rodada
- **Papa-moedas:** PV 3, PM 3, não ataca.
  - Anda para a moeda mais próxima e a come.
  - Se houver lutador a até 2 casas, foge.
  - Quem o nocauteia ganha **as moedas comidas + 2**.
- **Brutamontes Errante (chefe):** PV 20, PA 6, PM 3, armadura 1.
  - Anda até o lutador mais próximo, de qualquer time, e usa **Pancada**: 4 PA, alcance 1, 3 de dano.
  - A cada 2 rodadas, usa **Rugido**: área 2, os atingidos perdem 2 PM no próximo turno.
  - Quem o nocauteia ganha **5 moedas e 1 item aleatório do catálogo**, que cai num baú na casa dele.
  - Não tira Galeões.
  - Vai embora depois de 5 rodadas.

---

## 4. Mecânicas novas no núcleo (ordem sugerida)
1. **Eventos:** `S.event` com duração, tick e fim, mais neutros (time 0) com IA simples. Abre lava, fogos, nevoeiro, papa-moedas e o chefe.
2. **Flags de skill:** `pierce`, `push`/`pull` (com colisão), `minRound` e casa congelada.
3. **Itens novos da loja,** usando as flags do passo 2.
4. **Lutadores simples:** Bobett, Cheiro, Vespa, Viúva, Ogro, Guardião e Domador.
5. **Mecânicas únicas:** prisão (Kellen), Devorar técnica (Colosso), gatos comandados (Asamyh), Lendária no seletor e, por último, o Desejo, que tem um comando com opção.

Cada passo leva testes em `CoreMatchTests.cs`, além de smoke e contrato de rede. O estado novo (`event`, neutros, casas congeladas e presos) entra no snapshot filtrado.

---

## Estado da implementação (10/10, autorizado pelo autor; Desejo na rodada 10)

**Validação:** tudo implementado no núcleo, na apresentação, na HUD e no menu.
- 49/49 EditMode, sendo 20 testes novos.
- Build Windows, fumaça (local, host e cliente) e contrato de rede (10 cenários) passando.

**Núcleo (`Core/`)**
- **Eventos:** `Match.Cataclysms` traz id, peso e rodadas. O estado guarda `S.ev`/`S.evRounds`. `TickEvent` roda no fim da rodada e `EndEvent` desfaz o que o evento deixou.
- **Neutros:** ficam no time `MatchState.Neutral` (7), fora da linha do tempo, e agem em `Neutrals()`. Quem nocauteia um neutro recebe a recompensa.
- **Skills genéricas, as mesmas do editor:** `pierce`, `push`, `pull`, `paLoss`, `prison`, `selfArea`, `square`, `foes`, `isolatedBonus`, `minRound`, `heal`, `buffPa`/`buffPm`/`buffArmor`/`buffRange`/`turns`, `maxActive`, `summonCount`, `step`, `freeze` e `noCover`.
- **Tipos novos:** `Suporte`, `Zona`, `Comando`, `Reviver` e `Desejo`.
- **Lutador:** `armor` fixa, `legendary`, `immovable`, `devour`, `noTurn`, `noLock`, `neutral` e `healAura`.
- **Itens:** `anchor`, `climb`, `focus` e `thorns`.
- **Edições do autor:** `Content.ApplyEdits`, gravadas no aparelho em `arena-edicoes.json`. O estado leva `edits`, então as fichas do Host valem para toda a sala.
- **Ajuste dos dados:** nas skills em volta de si, os dados aparecem no evento `area`.

**Apresentação**
- **Eventos novos:** `area`, `zona`, `comando`, `sumir`, `empurrao`/`puxao`, `colisao`, `firme`, `prisao`, `preso`, `escapou`, `buff`, `suporte`, `reviver`, `desejo`, `canalizar`, `desejo-pronto`, `aprender`, `devorar`, `recompensa`, `lava`, `espinhos`, `explosao`, `dano-evento`, `comer`, `evento-tick` e `evento-fim`.
- **Marcadores:** casas de lava, gelo e fogos, e a gaiola da prisão.
- **Céu:** um por cenário e um por evento.
- **Sem a arte do Codex:** aparecem placas coloridas e clarões no lugar.

**HUD**
- 8 botões de skill, que passam a 2 colunas quando há mais de 4.
- Skill gasta aparece como "usada" e a que ainda não liberou mostra "rodada N".
- Botão Menu.
- Seletor com 20 lutadores e regra de 1 Lendária.

**Menu (`ArenaMenu.cs`, frontend do APK)**
- **Abas:** Início, Jogar, Personagens, Loja e eventos, Editor, Estatísticas, Regras e Opções.
- **Jogar:** jogadores, formato, vitória, quantidade de Galeões, cenário, partida local ou rápida, sala pela internet, LAN e código.
- **Editor:** muda a ficha e cada skill. Também cria skills a partir de modelos: dano, disparo, cura, suporte, aura em grupo, empurrão, puxão, prisão, grito em grupo e zona de gelo.
- **Estatísticas:** contadas por aparelho.
- **Capturas de todas as abas:** `ArenaTatica.exe --arena-menu-shot <caminho absoluto>`.

**Limites conhecidos**
- **Desejo D:** copia a skill mais forte, que não seja ultimate, do lutador clicado. Não há uma lista para escolher.
- **Desejo A e Pena de fênix:** revivem o aliado nocauteado de maior Nível.
- **Baú do chefe:** o item cai direto em quem o derrotou.
