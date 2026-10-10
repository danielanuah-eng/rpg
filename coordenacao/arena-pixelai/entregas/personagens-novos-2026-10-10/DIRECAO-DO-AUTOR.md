# Arena Tatica — personagens e modos, 10/10/2026

Destino confirmado pelo autor: ArenaTaticaUnity. A entrega contem sprites e animacoes de ataque baseadas na ideia por tras das referencias de arte. Substitui o rascunho de poses sem ataques em coordenacao/pixellab-personagens/2026-10-10-novos/briefing.json.

Personagens novos: Edgar, Gryvro (Grifo-de-Vidro), Raizor (Urso-Raiz), Alvyr (Cervo Branco), Ossyro (Dragao-Palido), Nixira (Goblins + Verdejantes) e Brumak (forca/escala de Golias com design original de minotauro de basalto).

Gryvro ataca com a garra. Pode voar entre pontos do tabuleiro gastando PM. Custo exato, alcance, regras de obstaculos, linha de visao e pouso aguardam design/implementacao do Claude; nao foram inventados aqui.

Edgar: melee com punhos cobertos de energia fantasmagorica; forma fantasma por 1 turno durante a qual golpes nao o acertam; ult invoca um navio fantasma. Sem novos numeros de combate definidos pelo Codex.

Damian: preservar identidade e armas. Ordem autorizada: camaleao vem para a mao -> transformacao em martelo -> estado modo martelo. Para mudar de modo: arma atual volta a camaleao na mao -> gesto/acomodacao na mao -> transformacao na nova arma. Martelada nao pode fazer as vezes da transformacao. O anim=hammer atual em Content.cs:38 e um golpe; Content.cs:39-41 usa morph para todos os modos. O Claude deve encadear os novos clipes de transformacao/destransformacao nesses eventos; Codex entrega os assets e os vinculos sem editar o codigo.

Formato: oito vistas base 128x128; animacoes nas quatro diagonais usadas pelo jogo atual. Nomes de eventos novos sao especificacoes de entrega, pendente_claude, nao evidencia de integracao.
