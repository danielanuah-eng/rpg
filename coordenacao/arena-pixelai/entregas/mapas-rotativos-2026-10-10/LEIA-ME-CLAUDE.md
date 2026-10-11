# Entrega visual da Arena Tática — 10/10/2026

Três estudos novos: Bosque dos Quatro Pátios, Pedreira do Anel Partido e Neve dos Bastiões Diagonais. Cada um tem fonte Blender editável, quatro prévias e layouts de 12, 16 e 18 casas. Uma casa mede 1 m e um nível mede 0,45 m. Os modelos reutilizam o kit de cenários v02.

As quatro câmeras são perspectivas locais do mesmo mundo. Implementar rotação de 90 graus por jogador mantendo as coordenadas, a lógica e o estado compartilhado da partida. Atualizar a seleção das oito direções dos personagens conforme a câmera local. As câmeras Blender foram verificadas para mostrar todo o tabuleiro do estudo 12x12; ajustar o enquadramento no motor para 16x16 e 18x18 e testar telas estreitas.

Os layouts são propostas visuais; ainda exigem validação no MapGen de spawns, acessibilidade, linha de visão, cobertura, alturas e colisões. Loja central e regras existentes permanecem como referência. Não registrar estes arquivos como mapas jogáveis aprovados antes desse teste.

Props novos: relicário solar (240 triângulos) e barricada partida (264 triângulos), ambos menores que uma casa e com pivô na base. São decoração até o autor definir outra função no editor. Entregues em GLB com texturas incorporadas e FBX com texturas copiadas/incorporadas; verificar o shader emissivo do relicário na importação Unity. As variantes exportadas usam os materiais de neve; os estudos de bosque e pedreira usam seus materiais próprios no Blender.

Importação de ida e volta, base, dimensões, texturas referenciadas e quatro câmeras passaram em `verificacao-blender.json`. `manifesto.json` conserva hashes e estado pendente de integração. Adicionar mapas e props ao catálogo de Edição de jogo referenciado na tarefa 012.
