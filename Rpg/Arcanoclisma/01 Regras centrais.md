# 01 — Regras centrais

> Rascunho v0 da Fase 1. Números conferidos em `calc/saida.md`. O que precisa da sua decisão está marcado **❓ P-xx** (ver [[09 Perguntas pra decidir]]).

## O jogo em um minuto

- Quando seu personagem tenta algo arriscado, você rola **2d12 + atributo** contra uma **Dificuldade**.
- **Cada dado par te dá 1 Arcano. Cada dado ímpar dá 1 Cataclisma ao mestre.** Par e ímpar são o equilíbrio do mundo.
- **Dois dados iguais = Crítico.**
- Magia não usa atributo: você junta **palavras** do seu Valor, escolhe o **Grau** (o tamanho do feito) e rola um conjunto de dados que **sempre soma no máximo 24**: 2d12 → 3d8 → 4d6 → 6d4. Mais dados = mais confiável, mas mais Arcano *e* mais Cataclisma saindo da mesa.
- Adversários rolam **d20**. O mestre gasta **Cataclisma** para fazer o mundo reagir.
- Não há turnos: quem tem o **holofote** age; ele passa ao mestre quando alguém falha ou tira um resultado Ímpar.

---

## 1. Atributos

Seis, usados só em testes e ataques com arma. **Não entram em magia** (exceto quando uma regra disser).

| Atributo | Para quê |
|---|---|
| **Agilidade** | esquivar ativamente, correr, acrobacia, armas leves e de disparo |
| **Força** | levantar, empurrar, agarrar, armas pesadas |
| **Carisma** | convencer, liderar, mentir, intimidar |
| **Inteligência** | lembrar, deduzir, consertar, estudar magia alheia |
| **Sabedoria** | intuição, sobreviver, resistir à mente, acalmar |
| **Percepção** | notar, mirar à distância, rastrear, ouvir |

Distribuição inicial: **+2, +1, +1, 0, 0, −1**. Máximo **+5**.

## 2. O teste: 2d12 + atributo

Role 2d12, some o atributo (e bônus), compare com a Dificuldade. Igual ou maior = sucesso.

| Dificuldade | Nome | chance com +2 |
|---|---|---|
| 10 | Fácil | 85% |
| 12 | Normal | 75% |
| 15 | Difícil | 54% |
| 20 | Muito difícil | 19% |
| 25 | Lendário | 2% (precisa de ajuda) |

**Vantagem**: some +1d6. **Desvantagem**: subtraia 1d6. Não acumulam; uma anula a outra. O d6 extra **não conta** para par/ímpar.

### Leitura dos dados (paridade)

Olhe os dois d12:

| Os dois dados | Nome | Efeito |
|---|---|---|
| iguais (qualquer número) | **Crítico** | sucesso automático + efeito crítico; ganha Arcano igual ao número do dado (**máx. 6**) — ❓ P-03 |
| os dois pares | **Par** | +2 Arcano para você |
| um par, um ímpar | **Equilíbrio** | +1 Arcano para você, +1 Cataclisma para o mestre |
| os dois ímpares | **Ímpar** | +2 Cataclisma para o mestre; **o holofote passa ao mestre** |

Isso vale igual para sucesso e falha. Um sucesso Ímpar é "conseguiu, mas o mundo cobrou"; uma falha Par é "não deu, mas você aprendeu algo" (o Arcano).

Chance de cada leitura no 2d12: Crítico 8%, Par 21%, Equilíbrio 50%, Ímpar 21%.

**Efeitos de crítico** (escolha um): ignorar um efeito negativo de magia; o melhor resultado possível do teste; dobrar o dano.

**Rolagens de reação** (resistir a algo que o mestre fez com você) usam 2d12 + atributo mas **não geram** Arcano nem Cataclisma. Isso segura a economia.

❓ **P-01**: você escreveu "d20 pra testes básicos" e "2d12 pra tudo". Eu fiz: **jogador sempre 2d12, mestre sempre d20**. Confirma?

## 3. Arcano (dos jogadores)

Cada jogador tem sua reserva. **Começa com 3, máximo 12**, e passa de uma sessão para outra. Se você ganharia Arcano acima de 12, pode dar o excesso a um aliado que te veja.

Por rolagem de teste o jogador ganha em média **1 Arcano** (2 em magia de Grau 3, 3 em Grau 5). É bastante, de propósito: Arcano é para ser gasto o tempo todo.

| Custo | Gasto |
|---|---|
| 1 | **Vivência**: +2 numa rolagem em que uma Vivência sua se aplica (uma por rolagem) |
| 1 | **Ajudar**: dê Vantagem a um aliado (descreva como) |
| 1 | **Palavra extra** numa magia (cada uma além da primeira) |
| 1–3 | **Ampliar** uma magia um passo (1 no Grau 1–2, 2 no Grau 3–4, 3 no Grau 5) |
| 2 | **Suavizar**: role de novo o d20 de uma Dificuldade de magia e fique com o melhor |
| 3 | **Hábito**: o talento especial do seu Comportamento |
| 3 | **Ação em Dupla**: você e um aliado rolam, os dois usam o maior resultado, e os efeitos se somam |
| 3 | **Detalhe**: acrescente algo verdadeiro à cena (uma corda, um conhecido, uma saída) — o mestre ajusta |
| 3 | **Forçar o Grau**: conjure 1 Grau acima do seu máximo (e marque Cansaço igual ao Grau) |
| igual ao custo | **Conter**: anule um Evento de Cataclisma de até 4 pontos que afete só a sua ficha. Aliados podem ajudar a pagar |

## 4. Cataclisma (do mestre)

O mestre **começa a sessão com 2 por jogador**, máximo **20**.

**Transbordo**: se o mestre ganharia Cataclisma acima de 20, o excesso não some — ele dispara *na hora* um Evento de Cataclisma do tamanho do excesso (ver [[07 Mestre - Cataclisma, adversários e terrenos]]). O mestre não consegue "guardar tudo pro chefe"; o mundo vaza.

O mestre gasta Cataclisma para: pegar o holofote, ativar mais adversários, ativar habilidades de adversários e de lugares, trazer reforços, endurecer uma magia (+2 na Dificuldade por ponto, até +4), cancelar uma magia Vacilante, e disparar Eventos (que podem mexer até na ficha dos jogadores). Tabela completa no arquivo 07.

## 5. Holofote (combate sem turno)

Não há iniciativa nem rodada.

- Os jogadores decidem entre si quem age. Quem age faz **uma ação** (atacar, conjurar, ajudar, uma manobra) e pode se mover até **Perto** junto com ela.
- **O holofote passa ao mestre** quando um jogador **falha** ou tira **Ímpar** (dois ímpares). Na vez dele, o mestre ativa **1 adversário de graça** e **mais 1 por Cataclisma** gasto; depois devolve aos jogadores.
- O mestre pode **tomar o holofote** a qualquer momento gastando 1 Cataclisma (ex.: logo depois de uma magia gigante).
- Regra de mesa: ninguém age duas vezes seguidas se outro jogador ainda não agiu desde a sua última ação, a não ser que todos concordem.

**Duração em ações.** Efeitos que "duram 3 ações" contam **qualquer** ação de **qualquer** personagem (jogador ou adversário). Use um dado de contagem na mesa.

## 6. Alcance

| Faixa | Na mesa | Na ficção |
|---|---|---|
| **Toque** | encostado | ao alcance da mão ou da arma |
| **Perto** | até 1 passo curto | do outro lado de uma sala pequena |
| **Médio** | até 3 passos | um salão, um pátio |
| **Longe** | até 6 passos | do outro lado da praça, tiro de arco |
| **Horizonte** | o que se vê | outro morro, o céu |

Mover até Perto vem junto com a ação. Ir mais longe **é** a ação (ou um teste de Agilidade para ir mais rápido).

## 7. Esquiva, dano e Vida

**Esquiva** vem do Comportamento. Adversário acerta se o **d20 + ataque ≥ Esquiva**.

**Limiares** (como no Daggerheart): cada personagem tem **Maior** e **Grave**.
- Dano **abaixo do Maior** → marque **1 PV**
- **Maior ou mais** → marque **2 PV**
- **Grave ou mais** → marque **3 PV**

Limiares = base do Comportamento + nível + armadura.

**Proteção**: casas da armadura. Marque 1 casa para reduzir em 1 os PV marcados de um golpe. Restaura em Pausa.

**Magia causa dano físico** normal (passa pelos Limiares). Efeitos de magia que não são dano (medo, paralisia, cegueira) são resistidos com **2d12 + atributo** contra a Dificuldade do efeito.

**Vida (PV)**: 5–7 casas conforme o Comportamento, máximo 12.

## 8. Cansaço

**6 casas** no início, máximo 12. Cansaço é o preço do corpo e da mente; Arcano é o preço do mundo.

**Como ganha**: gastando para Forçar magia, algumas Dificuldades de magia, reações de Dominum, talentos de Comportamento, alguns ataques de adversários.

**Com o que gasta (marcando casas)**:
- **Forçar** uma magia: marque até (Grau) casas, **+2 no total por casa**.
- **Ampliar** uma magia: pague a ampliação em Cansaço em vez de Arcano.
- Talentos que pedem Cansaço.

**Exausto** (todas as casas marcadas): você fica **Vulnerável** (ataques contra você têm Vantagem), não pode marcar Cansaço de propósito, e cada Cansaço que você teria de marcar vira **1 PV**.

## 9. Descansos

**Pausa** (uma hora): escolha **dois**: recuperar 1d4 + Patamar PV; limpar 1d4 + Patamar Cansaço; restaurar toda a Proteção; **Meditar** (+1 Arcano). O mestre ganha **1d4 Cataclisma**.

**Pouso** (uma noite): escolha **dois**, cada um completo: todos os PV; todo o Cansaço; toda a Proteção; trocar uma Palavra Estrangeira; trabalhar num projeto. O mestre ganha **1d4 + nº de jogadores** de Cataclisma.

## 10. Quando os PV acabam

Ao marcar o último PV, escolha:

- **Apagar**: você cai inconsciente e volta depois da cena com 1 PV. Role 1d12; se tirar **até o seu nível**, ganha uma **Marca do Cataclisma** (risque 1 casa de Arcano máximo, e diga que marca ficou no seu corpo).
- **Última Palavra**: conjure uma magia 1 Grau acima do seu máximo, sem pagar Arcano e sem rolar Dificuldades. Depois, seu personagem morre.
- **Jogar os dados**: role 2d12. **Crítico ou Par**: levanta com PV e Cansaço limpos. **Equilíbrio**: levanta com 1 PV e uma Marca do Cataclisma. **Ímpar**: morre.

(Morbidyum tem regra própria, ver [[03 Valores]].)

## 11. Vivências

Frases curtas do passado do personagem ("Cresci num porto", "Fui soldado do Rei-Semideus", "Sei mentir para guardas"). Começa com **2**, cada uma vale **+2** quando você gasta **1 Arcano** numa rolagem em que ela faz sentido. Ganha mais nos níveis 2, 5 e 8.

## 12. Patamar

Quase tudo que cresce com o nível passa pelo **Patamar**:

| Nível | Patamar | Grau máximo | Palavras por magia | Sintonia (com o Canal) | Proficiência |
|---|---|---|---|---|---|
| 1 | 1 | 2 | 2 | +2 | 1 |
| 2–4 | 2 | 3 | 3 | +3 | 2 |
| 5–7 | 3 | 4 | 4 | +4 | 3 |
| 8–10 | 4 | 5 | 5 | +5 | 4 |

Regra fácil de lembrar: **Grau máximo, Palavras e Sintonia = Patamar + 1**. (Sintonia é Patamar + 1 do Canal; sem a condição do Canal, é só o Patamar.)

**Sintonia** é o bônus que se soma aos dados de magia (e ao dano de magia). Atributo não entra em magia.
