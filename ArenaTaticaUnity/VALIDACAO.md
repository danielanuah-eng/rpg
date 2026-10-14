# Validação da Arena Tática 1.2.0

Estado: publicado e verificado. Itens 1–3 concluídos na revisão bea26431, protocolo 5. Windows (build 2127612) e Android (build 2127611), versão 2026.10.14-1.2.0, com processamento concluído no itch.io. Os 248 arquivos Windows e o hash do APK baixados são idênticos aos pacotes locais. Recibos e hashes em Documentacao/release-1.2.0.json. O canal Web não faz parte desta atualização.

140/140 testes EditMode; smoke local/anfitrião/cliente; 10 cenários de contrato; salas e torneios com 2, 3 e 6 participantes; Relay DTLS real entre dois Windows no mesmo PC. A interface foi exercitada no player em retrato e paisagem e com 24 lutadores, incluindo duplo toque, bloqueio durante pinça, ataque no chão e encerramento. Tabuleiro e retratos ficam dentro da área visível. Evidências permanentes: Documentacao/020-testes.json e Documentacao/020-interface.json, com quatro capturas.

Atributos base sorteados: Vida 4–30, PA 5–8, PM 1–6, Iniciativa 0–12; dano positivo das skills próprias 1–8 e alcance até 8. Formas, equipamentos, buffs e habilidades adquiridas continuam com seus efeitos. Skills e atributos têm controles independentes e escopos Bots/Jogadores; ambos ativados aplicam a todos os participantes iniciais. Não altera as fichas do Editor.

Sem Android físico conectado: o gesto foi simulado no Windows; instalação, toque real e desempenho em celular ainda não testados. Distribuição atual utiliza os canais Windows e Android existentes no itch.io. Metadados concorrentes dos mapas e configuração de analytics foram preservados fora dos commits desta entrega.

## Histórico da versão 1.1.0

# Validação — Arena Tática 1.1.0

Unity 6000.6.4f1 / URP. Protocolo de rede 4. Revisão de código: 7b13322e21f5d4068099b503af0f80385d72af83.

## Confirmado

- EditMode: **103/103**, nenhuma falha ou teste ignorado. Lista de casos e horário em Documentacao/testes.json.
- Regressões de invisibilidade: posição e caminho ausentes da visão inimiga; aliados, autoridade, timeline, armadilhas, clones, contato/revelação e preview preservados.
- Controlador de sala: configurar/liberar/iniciar, reinício e retorno à sala, vagas inválidas, reconexão, formatos/equipes e opções de partida para 2/3/6.
- Controlador de torneio: bans, seleção com pool mínima de cinco, bye, progressão até campeão, congelamento da pool na reconexão, reinício de resultado já registrado e pool automática persistente.
- Revisão mecânica somente leitura pelo DeepSeek, consolidada pelo Codex: nenhum defeito comprovado no patch nem nos consumidores de coordenadas desconhecidas. A compilação, a execução Windows e o empacotamento têm evidências próprias abaixo.

- Relay: times completos medidos em cinco mapas para 2/3/6 participantes. Maior snapshot: 57.347 bytes; capacidade anterior 49.152, corrigida para 81.920, dentro do limite confiável do SDK.
- Arte: retirado aviso genérico antigo de produção; retorno do Berserk reaproveita transformação normal ao contrário quando faltar clipe na forma atual.

## Evidência da distribuição

- Windows: build concluída em 14/10/2026, código limpo e idêntico ao da cópia ASCII; recibo em Builds/Windows/proveniencia.json. Smoke local/anfitrião/cliente aprovado.
- Contrato de rede: 10 cenários aprovados; repetido nesta retomada de 14/10.
- Sala no executável: 2, 3 e 6 participantes aprovados, incluindo invisibilidade, revelação, reconexão, reinício, retorno à sala e nova partida. Torneio: 2, 3 e 6 participantes até campeão. Evidência em Artifacts/019-release-network.json.
- Unity Relay DTLS real: anfitrião e cliente Windows no mesmo PC, comando autoritativo e troca de turnos até a rodada 2 aprovados. Evidência em Artifacts/019-relay.json. Isso não comprova uma partida entre celular e computador.
- Interface: seis capturas não vazias em Documentacao/019-capturas, configuração, sala, tabuleiro, ficha e seleção em 1280×720 e 960×540. Tabuleiro completo no enquadramento de jogo; configuração/sala possuem rolagem. Avaliação em Documentacao/019-interface.json.
- ZIP Windows: 248 arquivos extraídos idênticos à origem, nenhuma entrada duplicada ou arquivo de conta; SHA-256 do ZIP local final `a01e2e86faf8704fb1f367e5b28954730644b8b2851830f7a0c0293d4726be82`. Evidência em Artifacts/019-windows-package.json.
- Android: build final concluída em 14/10/2026, mesma revisão e mesmo hash de árvore de entrada do Windows. APK `org.arcanoclisma.arenatatica` 1.1.0/code 2, ARM64, internet habilitada, assinatura verificada e certificado preservado da versão anterior. SHA-256 `7eee7738d28534d3ad5c43b62b9b44815a4e77b0ff3faded7651fe0e7bf205cb`; 212.645.978 bytes. Evidência em Artifacts/019-android-package.json e Builds/Android/proveniencia.json. O verificador também rejeitou corretamente o APK antigo 1.0.0/code 1.
- A retomada superou falha inicial de Java e atraso do cliente de licença. O Android Lint terminou com a configuração original; nenhuma checagem foi desativada. Nenhum C# foi alterado nesta retomada.

Publicação confirmada em 14/10/2026: `2026.10.14-1.1.0`, canais existentes Windows (build 2125800) e Android (build 2125802), ambos com processamento concluído. Os canais foram baixados novamente: todos os 248 arquivos Windows e o SHA-256 do APK são idênticos aos pacotes locais verificados. Resultados, hashes e IDs em Documentacao/release-1.1.0.json. Não foi necessário criar outra página ou canal.

Destino confirmado: https://eiediboy.itch.io/bdx-master, canais existentes windows e android. O canal Web não faz parte desta atualização.

## Limitações deliberadas

- Não há aparelho Android conectado: instalação, abertura, toque e desempenho em dispositivo físico ainda não testados.
- Espectadores de torneio recebem resumo; não há transmissão do tabuleiro.
- Desconexão aguarda reconexão; não há derrota automática ou alteração de bye.
- O teste de chave no player utiliza vencedores definidos pela ferramenta; não equivale a uma simulação completa de partidas.
- Assets e personagens Blender em produção têm avaliação separada e continuam pausados nesta meta.
- Polimento visual pendente: quebra do botão Personagens, textos pequenos em 960 px, contraste de títulos secundários, mensagens sobre sprites e recuo nas quebras das skills. Os controles abaixo da borda das páginas permanecem acessíveis por rolagem; toque físico não foi validado.
- A fonte mantém alterações prévias de metadados de três mapas e analytics. O recibo de build identifica a árvore de entrada efetiva; essas alterações concorrentes não são atribuídas a esta entrega.

Logs e caches permanecem locais, fora das distribuições. Arquivos locais de conta Unity são excluídos dos ZIPs de fonte. A aprovação da arte não é inferida da compilação.
