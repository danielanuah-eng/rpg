# Preservação visual — Enzo

Meta concluída em 10/10/2026: preservar os materiais autorizados de Arcanoclisma na biblioteca local.

## Entrega

- 279 arquivos visuais acrescentados ao índice, incluindo cenas de cartas e versões anteriores.
- 841 documentos acrescentados nesta execução (2 na primeira passagem e 839 após incluir as subpastas).
- As 210 artes aprovadas do site e as 46 cartas rápidas aprovadas têm cópia indexada disponível.
- A biblioteca contém 780 versões de assets e 1595 versões de documentos.
- Definições atuais de Carlos e Enzo sincronizadas; Carlos recebeu atualização.

Biblioteca: C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo.
Índice: indices/arcanoclisma.json. Os objetos originais ficam em objetos/, identificados por SHA-256.

## Procedimento reutilizável

O script percorre a pasta de arte autorizada, incluindo cartas-site e cartas-rapidas.
Guarda imagens, descrições, prompts, registros, procedimentos e decisões em formatos abertos.
O registro de geração acompanha a cópia quando o caminho e o hash correspondem.
Procedência ausente fica como “não informado”; aprovação é preservada conforme os registros.

Na pasta Rpg, execute:

~~~powershell
node "coordenacao/enzo/arquivar.mjs" "C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/Arcanoclisma/arte" "C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo"
~~~

A versão instalada na biblioteca continua atendendo ao comando da automação:

~~~powershell
node "C:/Users/Anuah/OneDrive/Área de Trabalho/Biblioteca-Enzo/scripts/arquivar.mjs" "C:/Users/Anuah/OneDrive/Área de Trabalho/Rpg/Arcanoclisma/arte"
~~~

Há um bloqueio exclusivo por projeto durante a execução. O índice é substituído após o salvamento dos objetos.
Conteúdos idênticos reutilizam o mesmo objeto; revisões e caminhos anteriores permanecem no índice.
A preservação da biblioteca é independente da geração de artes e da integração no jogo.
