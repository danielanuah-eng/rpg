# Sincronia dos sprites — entrega técnica do Codex

Auditoria do snapshot local de **12 pastas, 96 vistas estáticas, 284 clipes e 2.460 frames**. DeepSeek fez a inspeção visual das 48 folhas; Codex confirmou os defeitos escolhidos, mediu alfa, contagem e pivôs e verificou os reparos. Escopo e fontes estão em `auditorias/sincronia-2026-10-10/`.

| Correção | Resultado |
|---|---|
| Talyssa hit NW, entrega 002 anterior | Sete frames em pé e retorno; Claude confirmou importação. |
| Damian hit NW, Ciantia hit NW, Ezequiela hit NW | Removido o trecho de queda/morte, preservados sete frames por hit. |
| Damian pistol SE | Disparo no meio, recuperação final e nove frames de 168 px; exportação canônica idêntica à tira final em RGBA. |
| Devon idle NW | Respiração em pé com retorno, sem morte dentro do idle. |
| Devon death SE | Removidos somente 172 pixels de marcador vermelho, preservada a queda. |
| Jack ultshot SW | Removido o frame com polígono opaco sobre o corpo. |
| Berserk antigo idle SW | Removida a mancha vermelha isolada. |
| Teantha attack NW | Retirada a mancha azul dos dois frames finais, com recuperação. |

Os nove reparos desta etapa estão em `entregas/correcoes-sincronia/`, com fontes anteriores, desfazer remoto e `verificacao.json`. Talyssa mantém sua entrega separada. Os oito primeiros reparos são sequências/remoção exata de pixels, sem redesenho. Damian pistol foi gerado no PixelLab, com corte apenas de margens transparentes e cópia da primeira pose para o final. As outras 30 direções canônicas do Damian mantiveram suas URLs de origem; só pistol SE e hit NW mudaram.

**Pivôs:** 284/284 primeiras poses têm alfa e RGBA visível idênticos à referência estática, após descontar padding. Calculados pivôs por registro dessa referência, um por clipe, em `ancoras-para-claude.json`. **127** diferem da origem importada no snapshot. Essa é uma diferença de origem confirmada; os offsets por silhueta de `pivos-contagens-transicoes.json` são estimativas auxiliares e não devem dirigir correção por frame. Aplicação no Unity está na tarefa 009.

**Contagem e tempo:** todos os hits têm sete frames e as demais ações nove nas quatro diagonais disponíveis. Sem diferença de contagem entre direções da mesma ação. O snapshot antigo usava 10 FPS gerais; Claude confirmou a implementação atual de idle 5, walk 10, hit 8, death 6 e ataques/skills 7, além de movimento por distância. Não foi cronometrada execução real pelo Codex.

**Transições:** 48 prévias idle ↔ walk com pivô importado versus pivô registrado. Primeira pose alinhável sem mudar tamanho do personagem. Passos alternados, saltos, cauda do dragão e partículas movem o bounding box intencionalmente. A leitura visual não demonstrou mudança inequívoca do volume do corpo; não certifica proporções milimétricas de cada acessório.

**Direções:** os personagens atuais têm oito vistas paradas, mas apenas quatro diagonais animadas por ação; as cardinais usam fallback no código. A nova forma possuída é uma entrega independente com oito direções próprias. Clipes alternativos de revisão, revive remoto isolado e arquivos não usados não foram misturados ao inventário canônico.

**Pendências do relatório DeepSeek resolvidas:** Talyssa local já continha o reparo, confirmado depois pela resposta do Claude; nomes corretos são `ultshot`/`ultcharge`; metadados e offsets passaram a ter registros estruturados por clipe, sem depender da linha truncada de métricas. Efeitos intencionais de Perry, morph, leap e teleport foram preservados. Reprodução na partida, fallback cardinal, contato físico dos pés de unidades voadoras e exposição real dos frames permanecem verificações do Claude/autor.

Claude confirmou cópia dos nove reparos, compilação e testes EditMode **24/24** em `004-007-sincronia-claude.md`. Esse registro antecede a versão final de 168 px do pistol e o pacote final estabilizado da loja; ambos requerem nova importação. Codex verificou imagens e empacotamento, sem editar código do jogo.
