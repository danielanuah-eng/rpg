# Revisão visual do lote inicial

O DeepSeek via OpenRouter conferiu uma amostra de seis imagens: 087, 088, 103, 104, 106 e 109. Todas passaram em dimensões quadradas, assunto, equilíbrio quente/frio, pontilhado, recorte e transparência medida.

Foi encontrado um desvio claro no manto 087: transições suaves e aspecto de aquarela. Nos outros itens da amostra, foi apontado excesso de sombreado metálico/volumétrico e de contorno pesado, com grau de interpretação visual. A direção atual pede cores chapadas, sombras em blocos e linhas de espessuras variadas.

Decisão do Codex: testar uma versão com cel shading gráfico mais simples e conferir visualmente antes de continuar a produção. A aprovação de alfa em `progresso.json` não substitui a revisão de estilo. O restante do lote inicial ainda não foi aprovado nessa revisão visual.

As descrições originais foram preservadas como fonte. Os prompts de geração seguem o guia atual, que prevalece sobre os blocos antigos de estilo.

Nenhum push foi realizado. O autor exige autorização explícita antes do envio.

## Versão 2 em produção

O manto 087 foi redesenhado e conferido pelo Codex. O lote 088–115 foi substituído pelo prompt com cores chapadas, sombras duras e retícula; os registros anteriores foram preservados em `registros-estilo-inicial`. O campo `estiloAtual` em `progresso.json` conta somente imagens com registro da versão 2 e alfa conferido. Esta contagem não equivale à aprovação visual final.

Uma segunda amostra pelo DeepSeek via OpenRouter incluiu 088, 090, 094, 098, 100 e 101. Todos tinham alfa real. O Codex confirmou e corrigiu 098 (chapéu interpretado como guarda-chuva) e simplificou o mostrador 094. Também corrigiu a margem de 103. A suspeita geral de metal muito volumétrico permanece como apreciação subjetiva a considerar na revisão final. Os registros novos e as cópias de PNG substituídas confirmam a origem v2; a dúvida de proveniência do agente foi resolvida pelo histórico da geração.

Correções e inspeções locais estão registradas em `registros/revisoes-087-108.json`.

## Cenários transparentes

### Amostra 155, 164 e 168 e limite da ferramenta

Atualização da retomada: a geração voltou a funcionar na imagem 186. Os cenários 155, 164 e 168 foram editados pela ferramenta integrada e incorporados após inspeção visual do Codex e alfa/margens medidos. Vapores e magias de 155 agora usam formas chapadas; 164 tem efeitos gráficos mais simples; 168 ficou com três viajantes, abrigo e guincho discretos. Originais preservados em `versoes-anteriores`, prompts e medições em `registros/revisao-*-nova.json` e `registros/alfa-revisoes-cenarios.json`. O relato abaixo descreve o estado anterior a essas correções.

O DeepSeek via OpenRouter olhou uma vez cada PNG numa pasta contendo somente a tarefa e os três assets do projeto, sem ler o catálogo inteiro. Confirmou os assuntos centrais e alfa medido. Apontou transições suaves e brilho excessivo no vapor e magia de 155, na magia/sol de 164 e nos veios de ouro de 168. Levantou dúvidas sobre bordas cortadas, sobre a linha dia/noite acompanhar a estrada em 164 e sobre muitos aventureiros no vale inóspito de 168. A paleta fria de 168 é compatível com neve; o contraste quente deve ser conferido sem descaracterizar o ambiente.

O Codex conferiu 155 e confirmou que há vapor com tons macios e magia luminosa a simplificar. 164 e 168 foram vistos na saída da geração; são assuntos reconhecíveis, mas devem receber revisão de margem e de efeitos suaves antes da aprovação visual final. Estas pendências não foram corrigidas: a ferramenta atingiu `usage_limit_reached` em 186. Registro da revisão: `revisao-cenarios/ds-logs/2026-10-01T22-40-09-285Z-30472.json`.

A Coroa Riscada 143 foi corrigida por edição integrada: borda direita completada, sujeito inteiro com margens e tons metálicos mais simples. Registro: `registros/revisao-143.json`. A conferência final contou 99 PNGs (087–185), todos com alfa válido, 336 pendentes. Nenhum push foi feito.

A geração e a edição em formato 3:2 produziram fundos opacos em 147–150. O alfa medido reprovou os arquivos. Os cenários foram refeitos como vinhetas em tela quadrada; 147–154 passaram no alfa. 150 preserva a feira cheia, as máscaras, os músicos e as lanternas coral. 151 foi refeito como arena circular vista do alto, com arquibancadas cheias, correntes e portões. Os registros originais 148–150 foram preservados em `registros-rascunhos-cenarios`. A composição passou a priorizar o recorte transparente solicitado sobre a recomendação de proporção 3:2.
