# Vulcão — Passarela em S

Lote visual adicional da tarefa `013-claude-edgar-mapas-rotativos.md`. Uma passarela sinuosa de basalto atravessa o terreno, com dois patamares contíguos elevados e rotas baixas alternativas. A loja ocupa as quatro casas centrais de nível 1. O kit original de vulcão fornece árvores, caixas, arbustos, chão e entorno.

- ID estável: `vulcao-passarela-em-s`.
- Fonte editável: `fontes/vulcao-passarela-em-s.blend`, com módulos em `FONTES_EDITAVEIS` e texturas empacotadas.
- Fonte e quatro prévias: **12×12**. Layouts JSON alternativos: 12×12, 16×16 e 18×18. A discretização da passarela varia conforme o tamanho.
- Uma casa = 1 m; níveis 0/1/2, com 0,45 m por nível. Oito props existentes em cada layout. Não derivar colisão dos PNGs.
- Quatro câmeras locais ortográficas, a 90 graus entre si, mostram o mesmo tabuleiro inteiro; coordenadas, regras, loja e props ficam iguais.
- `construir.py` reproduz a entrega; `verificar.py` mede hashes, 144 tampas, quatro casas centrais, bounds de câmera, texturas e conectividade abstrata dos layouts.

A lava e os detalhes quentes são visuais do kit existente. Não há regra nova de dano, perigo ou terreno intransponível nesta entrega. Antes de liberar em partida, Claude valida o layout em MapGen, as casas iniciais, colisão, mobilidade, linha de visão e pivôs. Para 16/18, montar o JSON e ajustar o enquadramento local no Unity. Ler o estado e os hashes do manifesto; integração e jogabilidade deste mapa continuam pendentes.

A câmera em quatro vistas e os props no editor já foram confirmados pelo Claude nas respostas 012/013. Este lote acrescenta um mapa à estrutura existente. O catálogo visual ainda precisa ser consumido pela aba: preview, ID, versão e estados separados. Preservar os ajustes do autor. Importação de Edgar corrigido continua pendente; não sobrescrever com sprites antigos.

Arena ainda não finalizada. Comparação de todos os sprites em Blender e preparação do futuro PvEvP permanecem posteriores ao encerramento confirmado.
