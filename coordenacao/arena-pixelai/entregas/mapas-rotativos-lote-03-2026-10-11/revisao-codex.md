# Conferência final — estudo visual aprovado

Fonte 12×12 e quatro vistas locais conferidas pelo Codex. Após o relatório inicial, corrigidos os patamares fragmentados, duas árvores sobre a faixa elevada e a leitura da iluminação. A passarela agora usa duas casas por fileira, dois patamares contíguos e relevo simétrico em 180 graus. A loja continua sobre as quatro casas centrais de nível 1. Materiais do kit preservados; só a iluminação de apresentação foi clareada.

A primeira versão, incluindo fonte Blender, prompts, layouts, hashes e prévias, está em `fontes/vulcao-v1-antes-da-revisao.zip`. IDs da entrega permanecem iguais. Relatórios independentes inicial/final estão nesta pasta. A revisão final do DeepSeek confirma percurso legível, árvores fora dos patamares e mesmo tabuleiro inteiro nas quatro vistas.

Codex executou `verificar.py` depois da última mudança: 144 tampas, oito texturas empacotadas, hashes conferidos, quatro câmeras ortográficas e bounds inteiros. Os três layouts têm oito props no chão baixo, centro 2×2 livre e conectividade abstrata com passos de um nível. Patamares contíguos: 4/4 casas em 12 e 16, 6/6 em 18; relevo simétrico nos três. PNGs 1000×750 e alfa integral 255 medido.

Pendências (?) do DeepSeek: fontes e hashes foram conferidos pelo Codex; origem dos motivos quentes e da linha em V corresponde ao kit original, cujo SHA foi conferido. Os props foram verificados em coordenadas de chão baixo, sem inferir posição pela cor da imagem. As semelhanças das vistas opostas são esperadas pela simetria; a câmera muda e as coordenadas permanecem fixas.

Aprovação refere-se à entrega visual. Regra de subida, colisão, spawns, alcance, oclusão, importação e jogabilidade continuam com Claude. Lava é decoração existente, sem nova regra de dano. Fonte/prévias são 12×12; 16/18 são layouts discretos a montar e testar no Unity. A Arena continua sem encerramento confirmado.
