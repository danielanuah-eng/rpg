# Arena Tática — Unity

Port da Arena Tática Godot para **Unity 6000.6.4f1 / URP**, iniciado em 09/10/2026. O projeto original e o pacote de migração permanecem na pasta ArenaTatica. Este diretório contém o projeto Unity editável.

## Abrir e jogar

- **Editor:** execute `ABRIR-UNITY.cmd`. A cena é `Assets/ArenaTatica/Scenes/Arena.unity`; pressione Play.
- **Windows:** execute `JOGAR.cmd`. O executável fica em `Builds/Windows/ArenaTatica.exe`.
- **Web:** execute `SERVIR-WEB.cmd` e abra `http://127.0.0.1:8080`. Requer Node.js e a build em `Builds/Web`. Abrir o HTML diretamente pelo Explorador não serve os arquivos WASM corretamente.
- **Android:** `powershell -ExecutionPolicy Bypass -File Tools/build-android.ps1` gera `Builds/Android/ArenaTatica.apk`. Usa uma cópia temporária sem acentos, exigida pelas ferramentas Android. O módulo, SDK, NDK e JDK precisam estar instalados. Conferir o estado real da entrega em `VALIDACAO.md`.

Em partida local, a mesma pessoa controla P1 e P2 por turnos. Escolha **Mover** e clique em uma casa alcançável, ou **Atacar** e clique numa miniatura adversária. **Encerrar turno** ou Espaço passa a vez; os dragões agem automaticamente após P2.

O tabuleiro inteiro permanece visível. P1 é marcado em dourado, P2 em azul, dragões em violeta. O dourado pequeno está importado para uso futuro; nenhuma skill nova foi inventada para ele.

## Dois jogadores

1. No Windows, clique **Host**. Isso começa uma nova partida na porta TCP 9080.
2. Na outra instância Windows ou no navegador, informe o endereço do Host e clique **Entrar**.
3. No mesmo computador: `ws://127.0.0.1:9080`. Em rede local: `ws://IP-PRIVADO-DO-HOST:9080`. O Windows pode pedir liberação do aplicativo para a rede escolhida pelo usuário.
4. Host controla P1; cliente controla P2. Apenas uma conexão de jogador é aceita por partida. Comandos do cliente são validados no Host.
5. Se a conexão cair, use **Entrar** novamente para recuperar o estado existente. Unidades mortas não são recriadas. No Host, **Retomar local** continua a partida localmente. No cliente, esse botão começa uma partida local nova.

O navegador é cliente; o Host usa o executável nativo. Para internet, usar `wss://` atrás de proxy TLS e certificado válido. Uma página HTTPS exige WSS. Esta entrega não publica nem configura um servidor na internet. A conexão sem TLS é limitada a localhost e faixas IPv4 privadas, inclusive em builds release.

## Regras preservadas

Mapa 24 × 24, movimento ortogonal, 3 PM por turno e ocupação exclusiva. Jogadores começam com 100 HP, têm 2 PA por turno, ataque de 15 de dano por 1 PA e linha de visão lógica sem limite artificial de distância. Ordem P1 → P2 → dragões, ignorando derrotados. Monstros têm 30 HP; aproximam-se do humano mais próximo e atacam conforme seus atributos. A vitória exige um humano vivo e nenhum monstro; a derrota de todos os humanos encerra sem vencedor.

A regra é independente da apresentação. Modelos e animações representam o estado confirmado pelo Host. Entrada tardia aguarda o término da ação; efeitos antigos não são reproduzidos no snapshot. A HUD mostra HP, PA, PM, turno, resultado, RTT, sondas expiradas e cliques filtrados.

## Estrutura

- `Assets/ArenaTatica/Scripts/ArenaRules.cs`: células, caminhos, combate, turnos, IA e resultado.
- `ArenaGame.cs`: sessão, comandos, snapshots e coordenação das animações.
- `ArenaTransport.cs` + `Assets/Plugins/WebGL/ArenaWebSocket.jslib`: WebSocket nativo e navegador.
- `ArenaPresentation.cs`: tabuleiro, câmera, modelos, seleção, movimentos, VFX e pool de áudio.
- `ArenaHud.cs`: interface uGUI com mouse/toque e layouts horizontal/vertical.
- `Resources`: modelos GLB, PNGs, WAVs e material URP explicitamente incluído nas builds.
- `Assets/Editor/ArenaBuild.cs`: criação da cena e exportação Windows/Web/Android pelo Editor.
- `Assets/ArenaTatica/Tests/Editor`: testes de regras e validação.

Os GLBs vêm das cenas procedurais existentes no Godot, exportadas sem regenerá-las. Os PNGs e WAVs vêm do pacote de migração. A avaliação artística dos modelos continua pendente conforme os registros originais. O port não implica aprovação das artes.

## Reproduzir a validação

Com o Editor fechado:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/validate.ps1 -BuildWindows -BuildWeb
node Tools/smoke.mjs
node Tools/network-contract.mjs
```

O primeiro comando prepara a cena, executa os testes EditMode e gera as builds selecionadas. O segundo executa uma partida local e dois jogadores na build Windows, salvando logs e captura em `Artifacts`. Caches, builds, logs e configurações locais estão excluídos do Git; `.meta`, Assets, Packages e ProjectSettings fazem parte do projeto.

Os testes automatizados não substituem avaliação de jogabilidade, aparência e performance em aparelhos reais. O estado confirmado de cada alvo fica em `VALIDACAO.md`.

## Pacotes

`Tools/package.ps1` cria o ZIP do projeto editável e os ZIPs Windows/Web em Builds, com tamanho e SHA-256 em `entregas.json`. O APK fica em Builds/Android. O ZIP de fontes exclui caches, builds e logs; após extraí-lo, o Unity reimporta os assets. Para jogar pelo ZIP Windows, extraia a pasta inteira e execute ArenaTatica.exe mantendo os arquivos vizinhos.
