# 13 — Teste de mesa 1: O Ovo Roubado

> Kit para o primeiro teste: resumo de regras, 4 personagens prontos (nível 1), 3 Eventos (conversa, combate, Ameaça com Magia em Dupla) e o que observar. No fim, o **relatório do teste simulado**: o combate jogado 5.000 vezes por script (`calc/simular_combate.py`), com o que já dá para ajustar antes da mesa.

---

## 1. Resumo de regras (uma página para a mesa)

**Testes**: diga o **verbo** (o que faz) e o **atributo** (como faz; por padrão, o da linha na tabela de Interações). Role **2d12 + atributo + verbo** contra a Dificuldade (9 fácil · 12 médio · 15 difícil · 19 muito difícil · 23 lendário).

**Dados**: cada **par** = +1 Arcano para você. Cada **ímpar** = +1 Cataclisma para o mestre. **Dois iguais = Crítico** (sucesso automático, +Arcano igual ao dado, máx. 6). **Maior dado ímpar** (jogada por Cataclisma) ou **falha** → o mestre emoldura.

**Magia**: 1 palavra do seu Valor (núcleo) + até 1 palavra de Valor a mais (1 Arcano) + **uma** Palavra Básica de graça (um verbo da tabela ou uma forma). Na Moldura 1: **Grau 1** é grátis e acontece sem rolar (ou vira o meio de uma ação: dano 1d8+1); **Grau 2** custa **1 Arcano** e rola **2d12 + 2 (Sintonia) + 1 por palavra de Valor** (a combinação, o sentido e o Grau combinados com o mestre) pelo tipo de efeito: **Projétil** contra a Esquiva/Dificuldade do alvo; **Teste mágico** (sono, medo, encanto): você não rola, o alvo rola contra **12 + 2 + palavras** (15 ou 16); **Pequeno evento** (mudar o mundo) contra 12. Dano de Grau 2: **1d10 + 2**. Dura até a próxima Falha.

**Bônus**: só a tabela (atributo + verbo) e **Vantagem (+1d6)**, até 2 por rolagem; Desvantagem tira uma. **Arcano** (começa 3, máx. 12): 1 = Vivência (Vantagem) · 1 = Ajudar (Vantagem) · 1 = palavra extra · 3 = Hábito · 3 = Ação em Dupla · 3 = Detalhe · 3 = Forçar o Grau (Grau 3, 3d8, contra 14).

**Atalho ou Passo**: Atalho = o mestre ganha 2 (+3 numa rolagem, ou ignorar condição) ou 4 (falha vira sucesso). Passo de Valor = aceitar o desconforto (Desvantagem, ou 1–2 Cansaço, ou a consequência) → +1 Arcano, dê certo ou não. Dois Atalhos sem nenhum Passo apertam o Laço (ações fora do padrão dele ficam +2, uma relação estremece, a consequência cresce); o Passo solta.

**Dano**: abaixo do Maior = 1 PV · Maior = 2 · Grave = 3. Marque 1 Proteção para marcar 1 PV a menos.

**Holofote**: jogadores escolhem quem age; cada um age uma vez antes de repetir. Mover até Perto vem junto.

---

## 2. Personagens prontos

### Tomé Arrasta-Ferro
*Ex-soldado da Cidade-Muralha que ainda luta como se o capitão estivesse olhando.*

- **Comportamento**: Briguento · **Valor**: Conflitüo · **Canal**: Arma (machado)
- **Interações**: Força +1 · Agilidade +1 · Inteligência −1 · Bater **+2** · Proteger +1 · Intuir +1 · Ameaçar +1 · Mentir −1 → Bater +3 · Proteger +1 · Ameaçar +1 · Intuir +1 · Mentir −1
- **Esquiva** 10 · **PV** 6 · **Cansaço** 6 · **Arcano** 3 · **Limiares** 8 / 16 · **Proteção** 4 (cota, Média: −1 em testes de mobilidade)
- **Machado**: Força, Toque, **d10+3** (com Gosto de Sangue), Cruel (dano Grave marca +1 PV). Pela arma, a magia também é Cruel.
- **Talento — Gosto de Sangue**: usa todas as armas, qualquer coisa vira arma (d8+1); +1 de dano com armas; dano Grave = +1 Arcano. **Hábito — Surra** (3): dano dobrado ou um segundo alvo.
- **Skill — Fio da Guerra**: em combate, magia com +1 dado de dano ou +2 no total. **Retaliação**: fora de combate, magia custa +2 Arcano.
- **Compromisso**: "Não fujo de briga que é minha."
- **Laços**: *O Capitão Brando, que me mandou fugir e morreu na muralha* · *Resolver na porrada antes de pensar* (Atalho de costume) · *O medo de ser só uma arma*
- **Vivências**: Soldado da Cidade-Muralha · Leio uma briga antes de ela começar
- **Magias prontas**: *Lâmina* + projétil (G2, lâmina vermelha atirada) · *Aparar* + defender (G2) · *Grito* + intimidar (G2, Amedrontado até a próxima Falha)

### Iara do Rio
*Aprendiz de vidreira do Porto de Vidro, que carrega no pescoço um frasco do rio onde quase se afogou.*

- **Comportamento**: Curiosa · **Valor**: Sublimeyn (o Eixo) · **Canal**: Foco (o frasco)
- **Interações**: Inteligência +1 · Percepção +1 · Força −1 · Consertar **+2** · Estudar +1 · Movimentar +1 · Observar +1 · Ameaçar −1 → Consertar +3 · Estudar +2 · Observar +2 · Movimentar +1
- **Esquiva** 10 · **PV** 5 · **Cansaço** 6 · **Arcano** 3 · **Limiares** 6 / 12 · **Proteção** 3 (couro, Leve)
- **Adaga**: Agilidade, Toque, d8+1. **Magia de dano** (G2): 1d10 + 3 (com o Foco).
- **Talento — Já Li Sobre Isso**: duas vezes por sessão, um fato objetivo e descritivo, verdadeiro, sobre uma criatura, objeto ou lugar. **Hábito — Análise** (3): no começo de um Evento, descobre **um** dado (a Dificuldade, os Limiares, uma palavra ou o Valor de um adversário; um costume; um perigo).
- **Skill — Mistura** (1 Arcano): dois elementos numa palavra. **Retaliação**: +1 Arcano por passo de estado além do primeiro.
- **Compromisso**: "Nada precisa ficar como está."
- **Laços**: *Meu pai, que acha que magia é doença* · *Estudar em vez de sentir* (Atalho de costume) · *A culpa pelo afogamento do Caio*
- **Vivências**: Aprendiz de vidreira · Nado como peixe
- **Magias prontas**: *Gelo* + projétil (G2) · *Solidificar* + chão (G2, Preso até a próxima Falha) · *Vapor* + esconder (G1 no meio da ação) ou *Vapor* + área (G2, para o grupo)

### Nhanda das Máscaras
*Atriz que largou a trupe na estrada e vive de ser quem os outros precisam que ela seja.*

- **Comportamento**: Enganadora · **Valor**: Persona · **Canal**: Voz
- **Interações**: Carisma +1 · Agilidade +1 · Força −1 · Mentir **+2** · Atrair +1 · Esconder +1 · Criar +1 · Firmar −1 → Mentir +3 · Atrair +2 · Esconder +2 · Criar +1
- **Esquiva** 12 · **PV** 5 · **Cansaço** 6 · **Arcano** 3 · **Limiares** 6 / 12 · **Proteção** 3 (Leve)
- **Adaga**: Agilidade, Toque, d8+1. **Magia de dano** (G2): 1d10 + 2.
- **Talento — Mil Caras**: Vantagem para disfarçar, mentir e se esconder; mentira que funciona num PNJ = +1 Arcano. **Hábito — Sumir na Paisagem** (3, uma vez por sessão): se esconde no ambiente sem ninguém perceber, descrevendo como.
- **Skill — Toque Humano**: em pessoas, o primeiro passo de Ampliar sai de graça. **Retaliação — Tropeço** (e fora de gente, só pelo corpo ou sentimento): se falhar, a magia sai inútil ou atrapalha.
- **Compromisso**: "Gente vem antes de tudo."
- **Laços**: *A trupe que eu abandonei na estrada* · *Mentir antes que me perguntem* (Atalho de costume) · *O medo de que ninguém goste de quem eu sou sem máscara*
- **Vivências**: Atriz de trupe · Conheço todo tipo de gente de feira
- **Magias prontas**: *Medo* + inimigo (G2) · *Encantar* + convencer (G2) · *Voz* + distrair (G1 no meio da ação)

### Oruã Casca-Grossa
*Filho da Vila da Colheita, grande e calado, que fala melhor com bicho do que com gente.*

- **Comportamento**: Brutamontes · **Valor**: Primitivum · **Canal**: Corpo
- **Interações**: Força +1 · Sabedoria +1 · Inteligência −1 · Levantar **+2** · Sobreviver +1 · Rastrear +1 · Aguentar +1 · Negociar −1 → Levantar +3 · Aguentar +2 · Sobreviver +2 · Rastrear +1
- **Esquiva** 8 · **PV** 7 · **Cansaço** 6 · **Arcano** 3 · **Limiares** 11 / 20 · **Proteção** 5 (placas, Pesada: −1 Esquiva; a mobilidade não sofre, por ser Brutamontes)
- **Lança**: Força, Perto, d10+2, Alcance.
- **Talento — Ombro de Aríete**: Vantagem para empurrar, levantar, arrombar, segurar; armadura não atrapalha a mobilidade. **Hábito — Corpo de Pedra** (3): até o fim do Evento, Vantagem em testes de Força e de resistir.
- **Skill — Forma Animal**: 1 vez nesta sessão (Moldura 1), um animal até o tamanho de gente, com 2 Traços Animais. **Retaliação**: como bicho, não conjura.
- **Compromisso**: "O corpo sabe antes da cabeça."
- **Laços**: *Minha avó Jaci, que me ensinou a falar com bicho* · *Ficar calado e aguentar* (Atalho de costume) · *A raiva que eu tenho da cidade*
- **Vivências**: Criado na mata · Carrego qualquer coisa
- **Magias prontas**: *Raiz* + segurar (G2) · *Garra* + atacar, bruto (G1 no meio da ação: dano 1d8+1) · *Farejar* + procurar, instintivo (G1)

**Relações da party** (para a sessão zero): Oruã e Iara se conheceram quando ele a tirou de um rio · Tomé desconfia de Nhanda desde que ela mentiu para um guarda por ele · Nhanda sabe que Iara esconde um diário · Tomé deve a vida a Oruã e nunca agradeceu direito.

---

## 3. A aventura: O Ovo Roubado

**Premissa.** Da Vila da Colheita, onde Oruã cresceu, roubaram um **ovo de wyvern** que a vila chocava havia meses. O rastro leva à **Cidade-Muralha**, ao **Mercado de Feras**. O ovo vai chocar em dois dias, e a wyvern vai nascer leal a quem estiver perto.

### Evento 1 — O Mercado de Feras (conversa e investigação)

**Terreno: Mercado de Feras** (Moldura 1 · Social · Dominum): barulho e cheiro, Desvantagem para ouvir e rastrear. Ações de Cataclisma: *Jaula Aberta* (3): uma fera escapa · *Leilão* (1): alguém dá lance num jogador.

**Termina quando** o grupo descobrir onde está o ovo (o celeiro da Domadora) ou for expulso do mercado.

**Gente no mercado** (ficha de palavras):

- **Tânia Pena-Curta** — Moldura 1 · Social · — · Ser: Vendedora de aves, Fofoqueira · Agir: Pechinchar · Contar (livre: conta um boato verdadeiro e um falso) · Parecer: Barulhenta, Astuta · Quer: vender, e saber de tudo antes de todos · Laço: *Medo*.
- **Guarda Bento** — Moldura 1 · Social · — · Ser: Guarda, Cansado · Agir: Mandar embora · Aceitar (livre: um suborno de 5 o ou um bom motivo) · Parecer: Covarde · Quer: que o turno acabe sem problema.
- **O Menino das Gaiolas** — Moldura 1 · Social · — · Ser: Criança, Testemunha · Agir: Fugir · Apontar (livre: se alguém for gentil com ele, aponta o celeiro) · Quer: não apanhar da Domadora.

**Testes esperados**: *convencer, gentil* (Carisma, 12) com Tânia ou com o menino · *procurar, metódico* (Inteligência, 12) nas gaiolas vazias · *farejar, instintivo* (Oruã, Sabedoria, 15 com Desvantagem do terreno, ou Grau 1 de Primitivum) · *enganar, charmoso* (Nhanda, Carisma, 12) com o guarda.

**Momento de Laço**: o guarda empurra o menino. Quem se mete (Passo de Valor) e quem finge não ver (Atalho)?

### Evento 2 — O Celeiro da Domadora (combate)

**Termina quando** os adversários caírem, fugirem ou se renderem, ou o grupo pegar o ovo e sair.

**Combate** (não escaramuça): orçamento 3 × 4 + 2 = **14 pontos**; usados **13**. **Moral**: quando metade dos pontos cair (7), os bandidos fogem e a Domadora tenta negociar o ovo pela vida dela.

- **Domadora de Garmhal** — Moldura 1 · **Líder** (3) · Dominum · Dif 13 · Limiares 8/14 · PV 6 · Ataque +1 · Dano 1d10+2
  Ser: Domadora · Agir: Chicotear (Perto) · Mandar (livre: ativa uma fera aliada) · Soltar a Fera (3C: o Javali age agora, mesmo que já tenha agido) · Parecer: Leal · Quer: vender o ovo para Garmhal; foge com 2 PV · Laço: *Dívida*.
- **Cão de Coleira Dupla** ×2 — Moldura 1 · **Padrão** (2 cada) · Dominum · Dif 12 · Limiares 8/14 · PV 5 · Ataque +1 · Dano 1d10+2
  Ser: Cão, Servo · Agir: Morder · Agarrar (livre · Toque · 1 · Força · Preso · até o mestre emoldurar) · Parecer: Leal · Quer: obedecer.
- **Javali de Ferro** — Moldura 1 · **Bruto** (4) · Primitivum · Dif 11 · Limiares 10/18 · PV 8 · Ataque +1 · Dano 1d12+4
  Ser: Javali · Agir: Chifrar · Atropelar (1C · Médio em linha · Grupo · Agilidade · dano ×1 + Caído) · Parecer: Blindado, Furioso · Quer: sair da jaula. É **Primitivum**: resiste à magia de Oruã (Dificuldade +2, metade do dano), mas Oruã pode tentar **acalmar, calmo** (Sabedoria 15) falando com ele como bicho.
- **Bandidos da Domadora** (4) — Moldura 1 · **Lacaios** (1) · — · Dif 10 · Limiares 4/7 (1, 2 ou 3 caem por golpe) · Dano 4
  Ser: Ladrão · Agir: Esfaquear · Fugir · Parecer: Covarde · Quer: a parte deles.

**Valores**: a Domadora e os cães são **Dominum**: resistem a Dominum e são **fracos contra Primitivum** (Oruã: Dificuldade −2 e +1 dado de dano com magia).

**Mestre**: gaste Cataclisma quando quiser, sempre que for bom para a cena (interromper custa 1). No teste simulado, quem gasta pouco termina a luta com quase 20 e a luta fica fácil; gastar ao longo da luta (uma interrupção a cada 3 ações mais ou menos) deixa ela tensa na medida.

### Evento 3 — O celeiro pega fogo (Ameaça e Magia em Dupla)

Na briga, uma lamparina cai no feno. O ovo começa a rachar com o calor.

**Ameaça**: "o teto cai quando este pote tiver **6**" (o mestre põe um copo na mesa e coloca Cataclisma nele quando quiser). **Termina quando** o grupo sair com o ovo, ou o teto cair.

- Apagar ou conter o fogo é Grau 3 (uma sala): só com **Forçar o Grau** (3 Arcano) ou **Magia em Dupla**.
- **Dupla sugerida**: **Iara** (Sublimeyn, o Eixo) com qualquer um — *Vapor* ou *Solidificar* + a palavra do outro — efeito de Grau 3; quem conduz rola **3d8 + 2 + 1**, o parceiro soma **+2**, contra **14** (uns 89%). Custa 3 Arcano.
- **Dupla de opostos**: **Tomé** (Conflitüo) e **Nhanda** (Persona) — o efeito sobe 2 Graus (Grau 4: apagar o celeiro inteiro de uma vez); quem conduz rola **4d6 + 2 + 1 (a palavra dele)**, o parceiro soma **+2**, contra **16 + 2 = 18** (uns 66%); os dois marcam 2 Cansaço, e se sair Ímpar rolam uma Dificuldade a mais. Arriscado e bonito.
- **O ovo choca**: a wyvern nasce leal a quem estiver mais perto. Quem é? Vira uma linha na ficha (sem ficha própria).

**Fim da sessão**: as três perguntas de fim de sessão, e cada jogador diz se algum Laço apertou.

---

## 4. O que observar na mesa

Anotem durante (uma folha, um risco por vez):

- Quanto tempo levou cada Evento? (meta do combate: 45–60 min)
- Quantas vezes alguém ficou sem saber qual verbo e atributo usar?
- Quantas vezes alguém usou magia? Grau 1 como meio de ação, Grau 2, Dupla?
- Quanto Arcano cada um tinha no fim? Chegou no máximo (12)?
- Quanto Cataclisma o mestre tinha no fim de cada Evento? Transbordou?
- Quantos Atalhos e quantos Passos de Valor? Alguém nomeou o próprio Atalho?
- Alguma regra fez a mesa parar para procurar no texto? Qual?
- Algum personagem ficou sem o que fazer em algum Evento?

**Depois**, cada jogador responde em uma frase: o que foi mais divertido; o que foi mais confuso; o que você cortaria.

---

## 5. Relatório do teste simulado

O combate do Evento 2 foi jogado **5.000 vezes** por `calc/simular_combate.py` com os personagens acima. O modelo é simples: os jogadores atacam o adversário mais ferido e não gastam Arcano; o mestre ativa um adversário de graça quando emoldura e gasta Cataclisma para ativar mais. Não modela Atalhos, Hábitos, condições nem Abalos. Serve para achar números fora do lugar.

### Resultado base

| medida | média |
|---|---|
| vitória dos jogadores | 96% |
| ações de jogador até acabar | 22 (entre 19 e 26 em 80% das lutas) |
| vezes que o mestre emoldurou | 7,5 |
| PV perdidos pelo grupo (de 23) | 12 |
| jogadores que caíram | 0,8 |
| Arcano ganho pelo grupo | 29 (uns 7 por jogador) |
| Cataclisma ganho pelo mestre | 20 |
| Cataclisma gasto ativando adversários | 11 |
| Cataclisma no fim do combate | 17 (de 20) |

### Variações

| variação | vitória | PV perdidos | caídos | Cataclisma no fim |
|---|---|---|---|---|
| base (13 pontos) | 96% | 12 | 0,8 | 17 |
| **mestre interrompe** com 1C depois de 3 ações seguidas de jogador | 93% | 15,6 | 1,3 | 11 |
| um Bruto a mais (17 pontos, "difícil") | 74% | 17,8 | 2,0 | 14 |

### Trecho de uma luta simulada

```
Tomé (6,3 +2 = 11 vs 10) acerta os Bandidos: derruba 3
Iara (11,1 +3 = 15 vs 12) acerta os Bandidos: derruba os outros   ← dois ímpares: o mestre emoldura
  Javali acerta Tomé: −1 PV · Domadora acerta Tomé: −1 PV · Cão 2 e Cão 1 erram
Nhanda (1,10 +3 = 14 vs 12) acerta o Cão 1: −1 PV
Oruã (4,11 +2 = 17 vs 12) acerta o Cão 1: −2 PV
Tomé (1,10 +2 = 13 vs 12) acerta o Cão 1: −2 PV, ele cai
Iara (7,1 +3 = 11 vs 12) erra o Cão 2                            ← falha: o mestre emoldura
  Cão 2 acerta Iara: −1 · Javali acerta Iara: −1 · Domadora acerta Nhanda: −1
...
18 ações de jogador; o mestre emoldurou só 2 vezes e terminou com 18 de Cataclisma.
```

### O que o teste mostrou

1. **O orçamento de encontro funciona.** 13 pontos deram uma luta padrão (metade dos PV do grupo, quase 1 personagem caído, 96% de vitória); 17 pontos deram uma luta difícil de verdade (2 caídos, 74%). A régua dos adversários está no lugar.
2. **O mestre fica com Cataclisma sobrando.** Ganha uns 20 por combate e só consegue gastar 11 ativando adversários, porque a partir da metade da luta sobram poucos adversários para ativar. Termina perto de 20, e o próximo Evento começa transbordando. **Decidido (P-99)**: sem regra fixa; o mestre gasta Cataclisma **quando quiser** e age sempre que for bom para a narrativa. A simulação serve de referência: interromper mais ou menos a cada 3 ações deixa a luta mais tensa (1,3 caídos) e termina com 11 na reserva.
3. **O mestre emoldura pouco** (34% das ações). Como os jogadores acertam uns 75% das vezes, o mestre só age quando alguém falha ou tira dois ímpares. O mestre gastando para interromper quando quiser resolve isso.
4. **Arcano sobra na Moldura 1.** Cada jogador ganha uns 7 por combate e começa com 3: depois de uma luta está perto do máximo (12). Na mesa real eles vão gastar em Vivências, Hábitos e Forçar o Grau, mas vale observar se alguém bate no teto. Se bater, há duas saídas: máximo menor na Moldura 1, ou Equilíbrio dar só Cataclisma.
5. **A Skill de Persona some na Moldura 1.** Nhanda tem −2 na Dificuldade contra pessoas, mas a magia usa **a maior** entre a Dificuldade do Grau (12) e a do alvo. Contra a Domadora (13 − 2 = 11), a Dificuldade volta a ser 12: o bônus quase não aparece. **Sugestão**: o −2 de Persona vale **depois** de escolher a maior (12 − 2 = 10).
6. **A Retaliação de Conflitüo não pesa na Moldura 1.** "Fora de combate, 2 palavras" é o limite de todo mundo na Moldura 1. **Sugestão**: "fora de combate, a primeira palavra extra custa 2 Arcano", ou deixar e aceitar que a Retaliação só aparece da Moldura 2 em diante.
7. **A regra de Lacaios precisa de uma frase mais clara.** No script: cada acerto derruba 1, mais 1 para cada 4 de dano além do primeiro ponto. **Sugestão**: "um acerto derruba um Lacaio; cada 4 de dano a mais derruba outro".
8. **A Magia em Dupla funciona bem na Moldura 1**: comum uns 89%, de opostos uns 66% (agora numa rolagem só, ver [[14 Teste das mecânicas]]). Na mesa, ver se o preço de 3 Arcano e o Cansaço dos opostos parecem justos pelo tamanho do efeito.
9. **Duração**: 22 ações de jogador é uma luta de uns 40–50 minutos numa mesa que já conhece as regras. Dentro da meta.

**Ainda não testado pelo script** (só a mesa responde): se atributo + verbo é fácil de escolher; se a relação (as palavras somam) flui ou trava a conversa; se Atalho e Passo aparecem naturalmente; a Fusão; o Evento social.
