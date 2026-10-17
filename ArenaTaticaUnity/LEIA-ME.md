# Arena Tática — Unity 1.2.2

Jogo de combate tático por turnos para 2 a 6 participantes, desenvolvido em Unity 6000.6.4f1 com URP. Distribuição: https://eiediboy.itch.io/bdx-master. Consulte VALIDACAO.md para a evidência e as limitações da versão publicada.

## Abrir e jogar

- Windows: extraia o ZIP inteiro e execute ArenaTatica.exe, mantendo os arquivos vizinhos. No projeto, JOGAR.cmd abre a build em Builds/Windows.
- Android: instale ArenaTatica.apk em aparelho ARM64. A interface tem suporte a toque; a instalação e o desempenho em aparelho físico dependem de validação própria.
- Editor: ABRIR-UNITY.cmd; cena Assets/ArenaTatica/Scenes/Arena.unity.

No menu, monte seu time com até quatro lutadores e limite de 12 níveis. Escolha partida local, bots ou sala online. O mapa acompanha o número de participantes. Os atributos e as skills de cada lutador aparecem na ficha; os valores variam entre personagens e equipamentos.

Clique ou toque brevemente num lutador visível para abrir sua ficha. Clique numa casa alcançável para mover; as skills exigem dois toques no mesmo botão: o primeiro destaca e o segundo confirma. Depois, toque em qualquer casa dentro do alcance, mesmo vazia ou ocupada por alguém escondido. **Passar** e Espaço também exigem duas confirmações. Ao encontrar um inimigo escondido no caminho, o movimento para antes da colisão e preserva os PM não gastos. Arrastar desloca a câmera; arrastar com o botão direito gira a vista. No toque, segure por cerca de 0,45 segundo e arraste para girar; dois dedos controlam exclusivamente o zoom, com limiar contra tremidas e sem giro ou deslocamento pelo gesto. A iniciativa no topo mostra retratos e PV/PA/PM, com o turno atual e o próximo destacados. Em Opções, **Fechar a Partida** volta ao menu; o anfitrião devolve todos à sala e o cliente se desconecta. A roda do mouse também controla zoom. O enquadramento inicial mostra o tabuleiro inteiro.

## Aleatorizador

Em Jogar ou Configurar sala, ligue Skills e/ou Atributos limitados e escolha Bots, Jogadores ou ambos. Os números são sorteados por lutador pelo anfitrião, ficam na partida e não alteram o Editor: Vida 4–30, PA 5–8, PM 1–6, Iniciativa 0–12, dano positivo 1–8 e alcance até 8. Habilidades só em si preservam alcance zero. Bots existem nas partidas locais. Skills sorteadas mantêm a quantidade da ficha, sem duplicatas, com pelo menos um ataque disponível.

Clone das sombras copia a identidade visual e a cor de facção de quem conjurou. O Colosso recebe sua passiva quando qualquer entidade morre definitivamente no mapa; as habilidades absorvidas conservam os números efetivos, com +1 PA no custo e limite de duas.

## Sala online

1. O anfitrião abre **Configurar**, escolhe participantes, formato, mapa, opções de turno e loja e libera a sala.
2. Em rede local, os clientes entram pelo endereço ws://IP-PRIVADO-DO-HOST:9080; no mesmo PC, ws://127.0.0.1:9080. Para conexão pela internet, use a opção de sala Relay e seu código.
3. Os participantes escolhem seus times. O anfitrião inicia quando as vagas necessárias estiverem preenchidas.
4. Reiniciar começa outra partida na mesma sala; voltar à sala permite nova preparação. Reconectar recupera o assento e o estado confirmado pelo anfitrião, enquanto essa sessão continuar aberta.

O anfitrião valida comandos, custos, vez e ocupação. O protocolo desta versão é **5**: todos os participantes devem atualizar. A visão enviada ao adversário omite posição e deslocamento de unidades invisíveis, armadilhas inimigas e o estado do gerador aleatório. Aliados e anfitrião mantêm sua visão permitida; o contato revela a unidade conforme as regras. Comandos rejeitados continuam sujeitos às regras de ocupação.

WebSocket sem TLS é limitado a localhost e endereços IPv4 privados. Uma publicação Web em HTTPS requer WSS para esse transporte. O canal Web antigo não faz parte desta distribuição; não o misture com os clientes 1.2.0.

## Torneio

Até seis participantes disputam partidas de eliminação, um contra um. Cada jogador escolhe uma pool de até oito lutadores; uma seleção pequena é completada até cinco. Os bans configurados retiram opções da pool adversária, e cada jogador monta um time válido com as restantes. Quatro lutadores é o máximo, não o mínimo. Participantes sem adversário na rodada avançam por bye.

O anfitrião avança a chave após o resultado. Reiniciar a partida atual desfaz seu resultado registrado antes de disputá-la novamente. A reconexão conserva o draft; desconexão não gera derrota automática. Quem está fora da partida recebe o resumo do torneio: transmissão do tabuleiro para espectadores ainda não está implementada.

## Projeto e validação

- Assets/ArenaTatica/Scripts/Core: regras, conteúdo, mapas e estado determinístico.
- ArenaGame.cs: sala, torneio, comandos e projeções de rede.
- ArenaTransport.cs: transporte local; componentes Relay cuidam das salas pela internet.
- ArenaPresentation.cs, ArenaHud.cs e menus: tabuleiro, câmera e interface.
- Assets/Editor/ArenaBuild.cs: configuração e exportação.
- Assets/ArenaTatica/Tests/Editor: testes de regras e controlador.

~~~powershell
./Tools/build-windows.ps1
unity test "$env:LOCALAPPDATA/ArenaTaticaAndroid" --editor-version 6000.6.4f1 --mode EditMode --output Artifacts/editmode.xml --timeout 1200 -- -nographics
node Tools/interaction-qa.mjs
node Tools/smoke.mjs
node Tools/network-contract.mjs
node Tools/release-network.mjs
node Tools/relay-contract.mjs
./Tools/build-android.ps1
./Tools/verify-android.ps1
./Tools/package.ps1 -Platforms Windows
./Tools/verify-package.ps1
./Tools/verify-package.ps1 -Source
~~~

Builds Windows e Android usam sequencialmente a cópia ASCII %LOCALAPPDATA%/ArenaTaticaAndroid; não rode os dois ao mesmo tempo. Os scripts conferem caminhos, travam a cópia, registram revisão e hashes e recusam código alterado durante o build. São necessários Unity CLI, editor e módulos correspondentes. O build Android usa IL2CPP/ARM64; o primeiro build pode ser demorado. Logs ficam em Artifacts.

package.ps1 usa tar.exe, confere entradas duplicadas e caminhos internos e grava SHA-256 em Builds/entregas.json. Escolha explicitamente as plataformas atuais. O ZIP de fontes exclui caches, builds, logs e arquivos locais de conta Unity. As builds contêm proveniencia.json com a revisão de origem. Testes de torneio com vencedores definidos pela ferramenta verificam o fluxo da chave, não uma simulação completa de combate.

verify-android.ps1 rejeita APK antigo ou de outro aplicativo e confere versão/code, ARM64, internet, certificado da atualização anterior e recibo do código final. A validação de arquivo e assinatura não substitui instalação e uso em aparelho físico.

O projeto editável usa Assets com seus .meta, Packages e ProjectSettings. Arte em produção e pedidos Blender têm avaliação separada; esta versão não representa conclusão dessa fila.

Atualização 1.2.1: sprites de uso da bomba de fumaça, mina de gelo e totem; conjuração/impacto do motor quente; conjuração do chão de gelo; uso de quatro equipamentos/buffs da loja. Magias e pets só animam ao usar. Skills copiadas/sorteadas mantêm a arte e o tempo da original.

Atualização 1.2.2 (protocolo 6): posicionamento inicial, desempate de iniciativa, gatos, formas, reviver, alturas, mina terrestre; quatro céus; lava cercada volta a crescer; 44 efeitos de impacto novos. Aparelhos na 1.2.1 não jogam online com a 1.2.2.
