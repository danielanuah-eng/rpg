# Baltazar — pacote visual e proposta de personagem

Draconato paladino, kevlar prateado reforçado, martelo de guerra, boa vida e dano moderado. A ficha proposta usa 14 de vida, movimento 3, ataque corpo a corpo de dano 2 e custo 3 PA; são valores iniciais para teste, disponíveis para edição, sem declaração de balanceamento final.

`baltazar-completo.zip` contém o pacote nativo PixelLab. `sprites/` contém 32 PNGs: oito bases de 128 px e três ações nas oito direções. Espera: quatro quadros por direção; caminhada: seis; golpe: nove. As dimensões nativas do quadro constam em `manifesto.json`; não presumir 128 px nas ações. Total: 160 quadros. Alfa, hashes, dimensões e contagem foram conferidos em `verificacao-tecnica.json`.

Revisão visual é separada da verificação técnica. Consultar `visualReview` e eventuais pendências antes de importar cada clipe. A aprovação visual não confirma reprodução no Unity. As folhas `*-conferencia.png` são somente para revisão.

`ancoras-medidas.json` mede a base alfa > 40 de cada quadro. Usar um pivô constante por clipe conforme o importador existente; preservar o espaço para o martelo e o deslocamento da caminhada. Não recortar ou centralizar os quadros independentemente.

Golpe SE: importar `sprites/attack/south-east.png`, com nove células de 188×188. Usa a fonte `attack-v2` já corrigida também no PixelLab. `paddingOrigin:[6,0]` preserva a arte nativa 176×188 sem distorção. Índices 4 e 6 são pausas de levantamento/impacto com poses nativas adjacentes; impacto visual em 5. A versão original `attack/south-east` no ZIP foi reprovada e fica apenas como fonte histórica: seguir `selecao.json` ao reempacotar.

`companheiros/` entrega anjo e demônio em duas tiras 64×64, cinco quadros cada. A ordem proposta `[0,1,2,3,4,3,2,1]`, a 6 FPS, conserva um movimento suave. Pivô central para estes efeitos flutuantes. Claude implementa a órbita ao redor de Baltazar, com raio lógico de duas casas, defasagem de 180 graus e ordenação de profundidade pela câmera local. A órbita visual não repete o efeito.

A habilidade propõe cobrar 1 PA por turno mantido, curar 1 aliado ou causar 1 de dano no inimigo, com uma aplicação por alvo por turno de Baltazar. Desligar/religar não reinicia a contagem. Desligar por recusa de manutenção, insuficiência de recurso, nocaute ou fim de partida. Valores, recurso final e métrica de alcance seguem a ficha e a implementação de Claude.

Eventos propostos: martelo, ativar/manter aura, cura, dano e desligamento. IDs estão em `ficha-proposta.json` e precisam ser vinculados ao motor. Incluir personagem, habilidade, aparência e companheiros na aba Edição de jogo da tarefa 012; atualizar o catálogo depois de integrar.
