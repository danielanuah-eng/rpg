# Revisão visual de Baltazar — 10/10/2026

DeepSeek/OpenRouter fez a revisão das folhas e de quatro tiras nativas, somente por leitura. Relatório em `revisao-deepseek.relatorio.json`. Codex conferiu diretamente os quadros apontados e tomou as decisões abaixo.

- Bases, espera e caminhada: aprovadas visualmente. A silhueta de walk/north-west 4–5 corresponde à perna levantada, sem novo membro nem troca de direção.
- Attack/south-west 6: aprovado. Corpo abaixado, martelo junto ao chão e pequenos fragmentos claros correspondem ao impacto; não há troca de identidade nem braço extra.
- Attack/south-east original: reprovado. Os quadros 5–6 têm uma forma branca sólida cobrindo braço/arma; não é uma pose de martelo aceitável. Fonte preservada em `baltazar-completo-v1.zip` e `fontes/attack-south-east-v1-reprovado.png`.
- As outras sete direções do golpe: aprovadas visualmente após a revisão das folhas. Arma atrás da cabeça durante levantamento normal não foi classificada como braço fantasma.
- Anjo e demônio: dez quadros aprovados; transparência medida localmente. Ordem ping-pong proposta para flutuação.

Substituição somente do golpe SE gerada com PixMiniMax, primeiro e último quadro vinculados à mesma base. Grupo `99610ab1-96e2-4daa-a468-30b90f10cf7c`, nome nativo `attack-v2`. O gerador ainda incluiu dois flashes de silhueta branca, nos índices 4 e 6. Na montagem nativa gratuita do PixelLab, esses dois quadros foram substituídos integralmente pelas poses texturizadas adjacentes: 4 repete 3 para uma pausa de levantamento; 6 repete 5 para uma pausa de impacto. Nenhum pixel dessas poses foi redesenhado. Os nove quadros finais foram conferidos e aprovados; preservam direção, equipamento e anatomia, sem flash branco.

Montagem salva também no personagem PixelLab: imagem `9dcb620d-bf89-46ed-9795-abe52408a484`; desfazer com imagem `d4d1cd61-d8b1-4809-806c-6fc741a53186` no mesmo alvo. Fonte anterior em `baltazar-completo-v2-antes-da-montagem.zip`. Replay e os outros 167 quadros nativos foram conferidos em `verificacao-tecnica.json`. A célula nativa SE de 176×188 recebeu apenas seis colunas transparentes de cada lado para virar 188×188 no importador; todos os pixels e o alfa originais foram preservados.

Referência visual de impacto SE: índice 5, mantido em 6 como pausa. Ligação e teste do efeito no motor permanecem com Claude. O manifesto final permite importar os 32 clipes aprovados e exclui a versão original de attack/south-east com mancha branca.

Verificação técnica e medidas de ancoragem pertencem aos relatórios locais. Pivô final e reprodução/sincronismo no Unity ainda são de Claude; aprovação visual não confirma integração.
