# Retomada 019 — fechamento da Arena Tática

Ordem atual do autor: continuar a Arena Tática de onde parou. Esta conversa assume a continuação do fechamento 019; a frente anterior está sem execução ativa do Editor/Java. Personagens Blender continuam pausados.

Revisão do jogo: `7b13322e21f5d4068099b503af0f80385d72af83`, versão 1.1.0, protocolo 4. Evidências recuperadas: 103/103 EditMode; sala e torneio 2/3/6; Relay DTLS real em duas instâncias Windows no mesmo PC; seis capturas não vazias; ZIP Windows com 248 arquivos, sem duplicatas, conteúdo extraído igual à origem e sem arquivos de conta Unity.

O Android interrompido terminou em falha: Java do Unity 6000.6.4f1 não inicializou ao invocar Gradle 9.3.1 (`-1073741502`, `0xC0000142`). O APK presente em Builds/Android é anterior a esta revisão e não deve ser publicado como 1.1.0.

Escopo assumido nesta retomada: diagnóstico do build Android, validação dos pacotes, atualização de documentação/resposta 019, publicação nos canais existentes `eiediboy/bdx-master:windows` e `:android` e confirmação remota. Preservar mapas/metadados, analytics, arquivos locais de conta e entregas visuais concorrentes. Nenhum `.cs` adicional será alterado sem registrar a reserva antes.

DeepSeek: investigação somente leitura via OpenRouter numa cópia restrita dos scripts de build e ArenaBuild.cs, em tmp/019-android-review. Não enviar logs brutos com ambiente nem arquivos de conta. Teste físico Android permanece pendente se nenhum dispositivo estiver conectado.

Estado: em execução; nenhuma nova publicação confirmada nesta retomada.

Diagnóstico realizado: OpenJDK 17.0.18 iniciou e respondeu `-version`; nova build Android iniciada fora do sandbox. DeepSeek revisou os quatro arquivos somente leitura, sem achar causa comprovada no código. Suas pendências foram conferidas: o log contém a invocação interna Java/Gradle; o diff de ProjectSettings só remove o define SENTIS_ANALYTICS_ENABLED de Standalone; a pasta real de build permanece isolada. A associação `0xC0000142`/`STATUS_DLL_INIT_FAILED` foi confirmada na tabela oficial Microsoft: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55. A causa específica da falha de inicialização permanece indeterminada; não atribuir automaticamente ao sandbox.

Retomada: contrato de rede repetido, 10/10; Butler validou a pasta Windows e reconheceu ArenaTatica.exe Windows amd64/Unity 6000.6.4f1. SHA-256 do ZIP segue igual ao relatório anterior. ADB não encontrou aparelhos. Os canais remotos ainda apontavam para 2026.10.12 antes da atualização.

A primeira repetição falhou antes do build: o cliente de licença demorou mais que o timeout do Editor. O canal de licença apareceu depois; foi encerrado somente o Editor identificado desta tentativa, preservando o cliente. `unity license status` retornou sucesso. A nova tentativa passou por licença e recarga de assemblies, sem mudanças de código.

Ferramenta própria desta retomada: Tools/verify-android.ps1 confere identidade/versão/code do APK, ARM64, internet, assinatura preservada, recibo do código final e SHA-256. Deve rejeitar o APK antigo antes de validar o novo. Nenhum C# alterado.

Resultado: APK final gerado com sucesso, incluindo a conclusão do Android Lint original. Não foi necessário desativar lint ou modificar Gradle. Recibo Android e Windows apresentam a mesma revisão 7b13322e e árvore de entrada SHA-256 938270e4b80fbedfcca3b791b6e9f25583b8843e2664ba3f21e53ef7b6c23a84. Verificador rejeitou o antigo e aprovou o final: 1.1.0/code 2, ARM64, internet, certificado anterior preservado, SHA-256 7eee7738d28534d3ad5c43b62b9b44815a4e77b0ff3faded7651fe0e7bf205cb. Publicação iniciada nos canais existentes; aguardar conclusão remota antes de afirmar entregue.

Publicação confirmada: versão 2026.10.14-1.1.0, Windows 2125800 e Android 2125802, ambos completed. Downloads de ambos realizados; 248 arquivos Windows idênticos à origem e SHA-256 do APK remoto igual ao validado. Relatório persistente em Documentacao/release-1.1.0.json. Nenhuma nova página/canal criado; APK físico e polimento permanecem limitações declaradas.

Estado atual: fechamento 019 concluído para Windows/Android, com as limitações explícitas acima. Fonte final: 3.784 arquivos extraídos idênticos à origem, sem duplicatas/arquivos de conta, SHA-256 6d457d3a4f08e5aa37109fef2bbf89b21e7a59fd70d25b2f48121690b810fe9f. ZIP Windows recomposto a01e2e86faf8704fb1f367e5b28954730644b8b2851830f7a0c0293d4726be82 contém os mesmos arquivos verificados no download; o contêiner original de upload era 39ed0961e5714473f429e806b33684ee04011b1b94c804ee336e26811434a58c. Manifesto final inclui fonte, Windows e Android. A reserva geral PixelLab e as alterações concorrentes continuam preservadas.
