# Polimento Blender — 12/10/2026

O autor definiu o foco: mapas nítidos, peças mais limpas e depois todo o elenco e suas ações atuais recriados no Blender. Confirmou todo o elenco nesta conversa.

## Mapas

Os cinco temas recebem materiais de cor sólida e volumes legíveis, eliminando o ruído das texturas repetidas. As malhas, IDs, pivôs e as animações existentes são preservados. Os 183 GLB da pasta `models/` foram conferidos contra os arquivos efetivamente presentes no Unity: mesmas caixas delimitadoras em coordenadas globais, contagem de triângulos e nomes dos clipes; sem texturas de superfície ruidosas. `COLOR_0` é neutro branco; os pesos de vento do kit permanecem em `COLOR_1`.

Fontes editáveis em `fontes/<tema>.blend` e `fontes/extras/`. Prévias novas em `previews/`; os layouts e regras não foram alterados. O tratamento de materiais beneficia os 22 mapas que usam essas peças.

Para integrar, substituir os GLB correspondentes em `Assets/ArenaTatica/Resources/Models/Mapa/`, preservando os `.meta` existentes. Não mudar as regras e a câmera. Confirmar no Play as quatro vistas, árvores/oclusão, as três dimensões de tabuleiro e os cinco loops ambientais. Os renders Blender não são evidência de reprodução no Unity.

## Personagens

Inventário atual em `personagens/inventario.json`: 43 conjuntos visuais — 40 conjuntos do elenco, formas alternativas e invocações, mais o anjo/demônio do Baltazar e o castor vendedor da loja. Cada pasta recebe fonte `.blend`, modelo `.glb`, preview e manifesto individual. Os modelos são uma interpretação estilizada em volumes de malha, editáveis e articulados; a avaliação visual do autor continua separada da validação técnica.

Os IDs de clipes existentes são mantidos; clipes que o catálogo de habilidades requer também são adicionados. Os gestos têm preparação, ação e recuperação. Walk é um ciclo no lugar; o deslocamento do personagem continua pertencendo ao motor. Os clipes de morte, entrada e desaparecimento têm finais específicos. Efeitos de campo e projéteis permanecem assets separados.

Frente Blender `-Y`, exportada para `+Z` no GLB; Z-up → Y-up; unidade em metros e pivô no chão. Os rigs usam segmentos articulados com pesos rígidos, adequados à interpretação estilizada. Integrar em ramo próprio: o cliente atual apresenta sprites e não foi modificado nesta entrega.

Damian contém ossos separados `pistol`, `cannon`, `hammer`, `chameleon`; o canal de escala seleciona a arma no clipe. Sua troca de estado precisa se ligar aos eventos existentes. TchuTcha/TchuTchaBerserk e Edgar/EdgarFantasma são modelos separados; os clipes `transform` e `transformghost` preparam a mudança e o motor troca o modelo no gatilho já definido. Não presumir que o GLB sozinho faz a troca entre arquivos.

Estado da integração: **pendente do Claude**. A validação de assets e a avaliação no jogo devem continuar separadas.
