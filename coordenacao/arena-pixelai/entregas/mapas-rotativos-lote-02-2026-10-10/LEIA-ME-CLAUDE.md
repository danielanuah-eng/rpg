# Deserto — Galerias Deslocadas

Entrega visual adicional para a tarefa `013-claude-edgar-mapas-rotativos.md`. Duas galerias elevadas, deslocadas entre si, deixam rotas baixas alternativas e mantêm a loja central. O mapa reutiliza os módulos editáveis do kit deserto v02.

- ID estável: `deserto-galerias-deslocadas`.
- Fonte: `fontes/deserto-galerias-deslocadas.blend`, com texturas empacotadas e coleção `FONTES_EDITAVEIS`.
- Layouts separados 12×12, 16×16 e 18×18, com alturas 0/1/2 e oito props existentes por tamanho.
- Os três layouts reservam exatamente as quatro casas centrais para a loja. Galerias maiores são variantes discretas, não cópias geométricas idênticas. A fonte e as prévias são 12×12, explicitado no manifesto.
- Uma casa = 1 m; um nível = 0,45 m. Não derivar colisão dos PNGs.
- Quatro câmeras `VISTA_JOGADOR_1..4` e quatro prévias 1000×750 do mesmo tabuleiro 12×12. Somente a câmera muda, mantendo coordenadas, loja, props e regras.
- `construir.py` reproduz a fonte e as prévias. `verificar.py` confere hashes, 144 casas, texturas, quatro enquadramentos, layouts e conectividade abstrata entre os cantos e a loja.

O enquadramento inteiro foi medido na fonte 12×12. Para 16/18, Claude ajusta o zoom local em Unity conforme o tamanho e a tela, preservando o tabuleiro inteiro. Validar spawns, colisão, alcance, saltos/subida, bloqueios, linhas de visão e rotação dos sprites antes de marcar jogável. A conectividade abstrata com passos de um nível não confirma as regras do motor.

Adicionar ao catálogo e aos controles do editor sem sobrescrever configurações do autor. Até a resposta de Claude, integração e teste em partida permanecem pendentes. Nenhum código de jogo foi alterado nesta entrega.
