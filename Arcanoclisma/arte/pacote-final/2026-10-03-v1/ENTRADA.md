# Arcanoclisma — pacote final aprovado

O autor aprovou as 435 imagens atuais do catálogo 087–521 em sua mensagem “Pronto, pode confirmar todas elas”. A revisão de Aurelhaz (510) acrescentou o segundo braço preservando o desenho. Versões antigas foram preservadas, mas esta aprovação corresponde aos arquivos atuais.

## Entradas e assets

O índice de Enzo está em ../../../indices/arcanoclisma.json, relativo a este pacote. Cada entrada aponta para um objeto SHA-256 em ../../../objetos/. As entradas cujo campo aprovacao tem estado aprovada identificam os arquivos finais; versões anteriores preservam sua procedência. Descrições e prompts estão nos registros arquivados e nas entradas do índice. Não inferir materiais de Claude inacessíveis.

## Procedimentos utilizados

1. Ler catálogo, direção de arte e preferências; produzir ou editar somente o escopo autorizado.
2. Usar image_gen integrada. Para editar, inspecionar o alvo com view_image e fornecer o arquivo como referência. Solicitar alfa transparente, limitar a alteração e preservar identidade, pose e estilo.
3. Copiar a saída para imagens/<id>.png, preservando o original da ferramenta e versões anteriores. Registrar prompt, fonte, adaptação e decisão em registros/<numero>.json.
4. Executar conferir.mjs no projeto original com Node.js. Usa Sharp, mede alfa, cantos e pixels, grava progresso e gera galeria. Medição não equivale a aprovação estética.
5. No encerramento aprovado, executar encerrar-projeto.mjs. Preserva decisão, registros, biblioteca e pacote. A pausa de Carlos foi feita pela ferramenta automation_update, não pelo script.

Comandos efetivamente usados no encerramento:

- Node.js: C:\Program Files\nodejs\node.exe
- Arquivamento: node "C:\Users\Anuah\OneDrive\Área de Trabalho\Biblioteca-Enzo\scripts\arquivar.mjs" "C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\Arcanoclisma\arte"
- Encerramento: node "C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg\Arcanoclisma\arte\encerrar-projeto.mjs"

Os scripts copiados são snapshots. Conferir/galeria/salvar dependem da estrutura original com catálogo, registros e imagens; Sharp tem caminho absoluto local, a adaptar em outra máquina. Arquivar depende de sua localização em Biblioteca-Enzo/scripts. Encerrar depende da estrutura Rpg/Arcanoclisma/arte ao lado de Biblioteca-Enzo. Não executar os snapshots isoladamente sem adaptar os caminhos.

## Ferramentas e reprodução local

Node.js v24.19.0, sistema de arquivos e Sharp fazem salvamento, hash, medição e indexação localmente. image_gen e automation_update são ferramentas remotas desta sessão, não bibliotecas offline. O pacote não contém pesos, capacidades internas do Codex ou credenciais. Uma IA offline pode ler procedimentos, prompts e código, mas precisa de uma alternativa de geração de imagens para reproduzir o fluxo sem rede. Alternativa local não foi implementada neste trabalho.

Canva, Unity e modelos de movimento não foram usados na produção destes assets. Claude cuida de código e integração; imagens estáticas não incluem lógica de movimento. Nenhum push foi realizado. As automações auxiliares permanecem pausadas.
