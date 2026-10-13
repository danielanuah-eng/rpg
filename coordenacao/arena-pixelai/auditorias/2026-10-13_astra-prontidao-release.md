# Arena Tática — auditoria de prontidão, 13/10/2026

**Conclusão:** Windows tem build e verificações básicas aprovadas. A entrega completa ainda não está pronta para ser declarada publicada: faltam validar os fluxos novos, tratar a exposição de invisíveis na rede, atualizar os pacotes pretendidos e registrar a publicação real. Produção de personagens Blender pausada por ordem do autor.

Base do código: `f7317a350d0399f432af2b49f6224d4bdb91332b`. Auditoria Astra com apoio somente leitura do DeepSeek/OpenRouter. Nenhum C# foi alterado por Astra. Os relatos anteriores de implementação do Claude foram confrontados com o código; presença de implementação não equivale a teste funcional completo.

## Verificação executada por Astra

| Verificação | Resultado | Alcance |
|---|---|---|
| Unity EditMode do checkout atual | **84/84, zero falhas e ignorados** | Rodada 13/10, 19:46:44–19:46:46 UTC; saída do CLI exit 0 |
| `node ArenaTaticaUnity/Tools/smoke.mjs` | **Local, Host e cliente PASS** | Build Windows existente, não recompilada nesta auditoria |
| `node ArenaTaticaUnity/Tools/network-contract.mjs` | **PASS, 10 checks** | Contrato WebSocket local; não comprova Relay/internet ou torneio |
| Diff de scripts/Tools | Sem erro em `git diff --check` | Nenhuma modificação de implementação nesta rodada |

XML novo: `Artifacts/astra-release-editmode-2026-10-13.xml` na raiz do repositório (o CLI resolve `--output` contra o diretório de execução). Logs locais/Host/cliente e `network-contract.json`: `ArenaTaticaUnity/Artifacts/`. Esses arquivos locais não são uma publicação.

Há testes para TurnMode alternado, Edgar fantasma, limite/revelação do invisível, posição/ordem das ilusões e loja restrita. Faltam testes funcionais do ciclo completo de sala e torneio. Os 10 checks de rede não devem ser descritos como dez fluxos de produto distintos: o manifesto atual lista nove rótulos e conta dez verificações.

## Pacotes e rastreabilidade

- Windows ZIP: `Builds/ArenaTatica-Windows.zip`, **215.601.451 bytes**, 13/10 às 14:45:15 -03. Proveniência em `Logs/build-StandaloneWindows64-1791913287617.provenance.json`: sucesso, início 17:41:29 UTC, mas revisão declarada `1a26b6d0` com `dirty:true`. É evidência de build bem-sucedida; não garante por si só identidade exata com o commit `f7317a35`.
- Android disponível: `Builds/Android/ArenaTatica.apk`, **212.583.618 bytes**, **12/10 às 18:41:03 -03**, anterior aos pedidos de 13/10. O log da tentativa nova termina após 1h40 sem conclusão. Na checagem de processos não apareceu build Android em execução; investigar o encerramento antes de reiniciar. Não chamar o APK antigo de versão nova.
- Web: `Builds/Web/index.html` de **09/10**. Atualizar se Web participar desta publicação; do contrário manter identificação explícita de versão antiga.
- `Tools/package.ps1:13` ainda usa `Compress-Archive` para Windows/Web. `Tools/build-windows.ps1` passou a usar `tar.exe` por recusa de ZIP com entradas conflitantes no itch.io. O empacotador geral pode desfazer a correção: unificar e conferir integridade antes de usar.
- `bundleVersion=1.0.0` e `AndroidBundleVersionCode=1`. Definir identificação da nova entrega com base no canal existente; verificar compatibilidade de atualização e assinatura, sem inventar credenciais.
- `LEIA-ME.md`, `VALIDACAO.md` e `Documentacao/testes.json` descrevem a migração antiga, 26 testes e fluxo de rede ultrapassado. Atualizar com fatos atuais.
- Não foi confirmado o destino/estado do último upload no itch.io. Referência ao itch existe no histórico/script; não é prova de publicação. Sol deve recuperar o projeto/canal existente antes do upload, sem criar página duplicada.

## Achados e prioridade

**P0 para anunciar multiplayer com invisibilidade: dados de invisíveis enviados ao adversário.** `Core/Match.cs:289–304` copia todo MatchState e filtra RNG/armadilhas/disfarce; mantém a célula de inimigos escondidos. `ArenaGame.cs:409,453,543,579` envia essa visão em snapshots/estado. A apresentação oculta a miniatura, mas isso não protege o protocolo. Corrigir com visão de rede apropriada e testes de serialização/eventos/reconexão; preservar a autoridade do Host e as regras atuais de contato e revelação. Não simplesmente remover lutadores e quebrar timeline, ocupação ou Preview. Trata-se de integridade do jogo, não de vazamento de dados pessoais.

**P1: sala e torneio sem verificação ponta a ponta.** Cobrir Configurar → Liberar → Iniciar; tentativas inválidas antes da liberação; reiniciar e voltar ao lobby conservando conexão; desconectar/reentrar; loja por sala; formatos/equipes; torneio com 2, 3 e 6 pessoas, bans, escolha de time, bye, avanço e campeão. Validar que o assento da sala vira corretamente o time da partida. Rede local aprovada não substitui teste de Relay entre clientes.

**P1: concluir e identificar pacotes atuais.** Resolver Android, empacotador e rastreabilidade acima. Build limpa para a revisão final, testes no binário final, hashes e notas por plataforma. Não gerar Windows e Android simultaneamente na mesma cópia `%LOCALAPPDATA%/ArenaTaticaAndroid`.

**P1: conferir UI real.** Sala, seleção/ficha, descrições, tabuleiro completo, câmera reta/giro, toque e botões; pelo menos desktop e tela estreita. Registrar limitações se não houver aparelho Android. Acabamento que não prejudique leitura/uso pode seguir depois.

**Não classificar como bug sem decisão de design:** DeepSeek apontou pool mínima de cinco e dois bans deixando três lutadores. O time admite **até** quatro e tem teto de Nível; três não prova time inválido. Não aumentar pool, mudar bans ou regras por esse achado. Testar se o draft permite time válido; corrigir apenas falha reproduzida.

**Limitação já declarada:** espectadores do torneio só veem resumo; a chave é mata-mata com bye, sem visualização da partida. Tornar isso claro. Não implementar novo modo de torneio nem bloquear toda publicação por acabamento da chave. Se a divulgação prometer assistir, retirar a promessa até a função existir.

## Concorrência e efeitos da validação

Três `.meta` de mapas já estavam modificados antes da auditoria: preservar. Após Unity EditMode, apareceu a remoção de `SENTIS_ANALYTICS_ENABLED` em `ProjectSettings.asset`; não foi edição manual de Astra. Verificar se é atualização automática do pacote e não incluir silenciosamente. Há `Assets/Resources/UnityPlayerAccountSettings.asset` e `.meta` não rastreados; revisar necessidade e conteúdo antes de incluir, sem divulgar identificadores/tokens de conta. O restante do monorepo contém trabalho concorrente e exclusões alheias; não incluir na entrega.

As lacunas do relatório DeepSeek sobre builds, logs e scripts de teste foram supridas pelas verificações desta auditoria. As referências de teste de clones foram conferidas diretamente. Permanecem sem comprovação: Relay em execução, fluxo novo completo, telas/aparelho e upload remoto.

Execução atribuída ao Sol pela tarefa `019-sol-fechar-versao-arena-13-10.md`. Astra encerra a auditoria e o repasse; isso não encerra a preparação/publicação do produto.
