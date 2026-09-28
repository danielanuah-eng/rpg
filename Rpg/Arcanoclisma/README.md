# Arcanoclisma

*Um passo diante do desconforto e do compromisso.*

Sistema de RPG próprio, inspirado em Daggerheart, Rune RPG, Ethmos e Triangle Agency.

**O que é**: 2d12 em que cada dado par te dá Arcano e cada dado ímpar dá Cataclisma ao mestre; ações descritas com atributo + verbo (a tabela de Interações); magia feita de palavras (9 Valores, 36 palavras cada), em Graus que trocam o conjunto de dados sem passar de 24; sem turnos: Eventos, o mestre emoldura, marcadores; adversários também feitos de palavras.

## Por onde ler

1. [[00 Roteiro]] — o plano das 5 fases e onde estamos
2. [[09 Perguntas pra decidir]] — **comece a responder por aqui** (as 🔴 primeiro)
3. [[01 Regras centrais]] — rolagem, Arcano, Cataclisma, Eventos e o emoldurar, dano, Cansaço, morte
4. [[02 Magia]] — Graus, custos, palavras que somam, Efeitos Colaterais, Magia em Dupla, Palavras Básicas
5. [[03 Valores]] — os 9 Valores com as 36 palavras
6. [[04 Comportamentos]] — os 12 Comportamentos e seus talentos
7. [[05 Personagem e níveis]] — criação, Vínculo, níveis 1–10
8. [[06 Itens e dinheiro]] — armas, armaduras, catalisadores, relíquias, familiares, Ordynus
9. [[07 Mestre - Cataclisma, abalos e terrenos]] — gastos do mestre, Abalos, Marés, Correntes, terrenos
10. [[08 Mundo e entidades]] — origem, par e ímpar, Primordiais, semideuses, Reinos, dragões, Eras, perguntas de escritor
11. [[10 Adversários - como criar]] — adversários por palavras, régua, maestria
12. [[11 Bestiário]] — 280 personagens do mundo numa lista só (gente comum, Comprometidos, povos infundidos, feras, Disruptos, Protetores, seres de energia e dos planos, semideuses, dragões, Era Tecnomaga), cada um com Básicas e, se conjura, Valor
13. [[12 Tesouros e itens mágicos]] — itens narrativos, Maravilhas, itens comuns, achados
14. [[13 Teste de mesa 1]] — kit do primeiro teste (4 personagens, 3 Eventos) e relatório do combate simulado
15. [[14 Teste das mecânicas]] — etapas, facilidade e vantagem repetida de cada mecânica; economia de Arcano
16. [[15 Teste de escala e criatividade]] — combate em todas as Molduras e tamanhos de mesa, testes difíceis, Eventos sociais, uso criativo das palavras
17. [[16 Sessão de teste - Castian]] — sessão jogada: ficha do Castian, a Iolanda e o registro dos Eventos, com as regras ajustadas na mesa
18. [[17 Interações - proposta]] — a tabela de Interações (atributos + 30 verbos): o porquê e as contas (P-122, aprovada)
19. [[18 Sessão de teste 2 - Alvenyr]] — segunda sessão: solo, Moldura 3, Sarn-das-Escadas, luta de Chefe
20. [[19 Fim de sessão - as perguntas de envolvimento]] — o fim de sessão e de aventura: Laços, as três perguntas, *Perceber e Mudar*, Marcos e o que eles compram
21. [[20 Prompts de arte - personagens]] — os personagens das sessões como rostos do livro: estilo base (traço Wilderfeast) e um prompt por personagem, interagindo com a página

## Conta dos dados

`calc/dados.py` calcula todas as chances usadas nas regras; `calc/saida.md` é o resultado. `calc/simular_combate.py` joga o combate do teste milhares de vezes; `calc/simular_economia.py` testa ganhos e custos de Arcano; `calc/simular_mesa.py` testa todas as Molduras, tamanhos de mesa e Eventos sociais. Se mudar um dado ou uma Dificuldade, rode `python3 calc/dados.py > calc/saida.md` de novo.
