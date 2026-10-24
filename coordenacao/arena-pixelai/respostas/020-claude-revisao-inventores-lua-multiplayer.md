# 020 — Resposta de Claude: revisão Inventores e Lua (multiplayer)

**De:** claude · **Para:** Carlos/Codex e autor · **Data:** 23/10/2026
**Status:** primeira rodada concluída no projeto principal; sem commit/push.

## Arquivos reservados e alterados (principal)
- `Assets/ArenaTatica/Scripts/Core/Match.cs`: só o fim de `ViewFor` (trecho comentado "Revisão 020").
- `Assets/ArenaTatica/Scripts/Core/Match.InventoresLuna.cs`: `RebuildTimelineKeepingSummons`, `LunaDeath` (Viva Luna) e `SharkSwim`.
- Novo: `Assets/ArenaTatica/Tests/Editor/Revisao020Tests.cs` (4 regressões).
A cópia `work/` do Somnax não foi tocada. Ao integrar o Somnax, preserve esses trechos.

## Bugs corrigidos
1. **ALTA: o chefe some da ordem de turnos quando há Bifia na partida.** `LunaEndRound` → `RebuildTimelineKeepingSummons` chamava `BuildTimeline`, que só põe lutadores de jogador, e devolvia apenas as invocações. O Rei Thaagarys (time Neutro) e as entradas dele "a cada N turnos" eram apagados na 1ª virada de rodada. Correção: cada id fora dos lutadores de jogador (invocações, chefe e repetições) volta depois do mesmo lutador que vinha antes dele. Teste: `BifiaRebuildKeepsTheBossTurns`.
2. **ALTA: o Viva Luna nunca acabava quando a Bifia morria.** O campo só conta "turnos da Bifia", e caída ela não tem turno. Resultado: bônus de iniciativa/PM permanente e a ordem reconstruída para sempre. Correção: `LunaDeath` encerra os campos dela com `EndVivaLuna`. Teste: `VivaLunaEndsWhenBifiaFalls`. *É decisão de regra: se o autor quiser o efeito até o fim mesmo com ela caída, reverter e fazer a contagem por rodada.*
3. **MÉDIA: vazamento na névoa: engrenagens.** `ViewFor` enviava todas as engrenagens, inclusive em casas que o time não vê (moedas já eram filtradas por `Fog.Known`). Agora o time só recebe engrenagens de outra família nas casas que vê; a família do Nikola continua vendo todas (a saída da matriz precisa delas). Teste: `EnemyGearsOutsideVisionAreNotSentInFog`.
4. **BAIXA: vazamento: marca do Celinos.** `LunaMark.x/z` mostrava a casa de um alvo que ficou invisível depois de marcado. Agora vai -1/-1 para quem não o vê; a apresentação usa só o id. Teste: `MarkOnHiddenTargetDoesNotRevealItsCell`.
5. **BAIXA: tubarão parado.** `SharkSwim` usava `default(Cell)`, que é (0,0), como "nenhuma casa". Trocado por `First()` (a lista já tem casas).

## Testes
- `ArenaTaticaUnity/Artifacts/revisao020-editmode-2.xml`: **687/688**. A única falha, `ClosedMapTests.ClosedMapsAreFairWalledAndPlayable` (Docas, que é aberta, entra na lista dos mapas fechados), já falhava antes destas mudanças (`revisao020-editmode.xml`). A cópia do Somnax já corrige esse teste (`MapGen.Closed`); entra com a integração dela.
- Build Windows refeito com as correções (`Artifacts/revisao020-build.log`): **Success, 0 erros**.
- **Contrato de rede estava quebrado:** `Tools/network-contract.mjs` mandava protocolo 13, e o jogo está em 14. Corrigido para 14.
- **O host não abre no caminho com acento** (`Área de Trabalho`): o log para em `MonoManager ReloadAssembly` e a porta 9080 nunca abre. Copiado para uma pasta sem acento, o host abre em 15 s. Rode builds e testes de rede a partir de uma cópia sem acento, como no Android.
- Contrato de rede com o build novo (cópia sem acento): **PASS, 10 cenários** (dono, revisão velha, RNG oculto, armadilhas ocultas, movimento autoritativo, reconexão, sem VFX repetido, snapshot forjado recusado, troca de turno). Ver `ArenaTaticaUnity/Artifacts/revisao020-network-contract.json`. Esses cenários não exercitam as skills novas de Inventores e Lua.

## Conferido sem bug
- O estado novo (`LunaState`, `Gear`, `LunaField`, `LunaMachine`, `Fighter.remoteShop/initBonus`) é `[Serializable]` com campos públicos e listas, e sobrevive ao snapshot/reconexão.
- Os sorteios da expansão passam por `Dice.Range(S,..)`. Sem `UnityEngine.Random`/`Time` no Core; `ViewFor` zera o RNG.
- `Match.Apply` exige revisão atual, lutador ativo e time do remetente; a loja remota zera em `BeginTurn`. Não achei comando de cliente que crie recursos ou controle invocação alheia.
- `comando-celinos` só marca inimigo visível; eletaser, flecha e matriz validam alvo e alcance.

## 2ª rodada: decisões do autor aplicadas (23/10, "pode fazer a festa, corrige")
- **Roque:** sem o tabuleiro (domínio) ligado, os dois aliados ficam no alcance da skill (8) a partir do Desland; com o domínio ligado, qualquer distância. `RoqueCheck(f,sk,…)`. Teste: `RoqueNeedsRangeUnlessChessBoardIsOn`.
- **Celinos:** revela só para o time do dono. `hidden` não muda mais; novo `LunaState.radar` (`LunaSeen{team,id}`), recalculado a cada ação (saiu do raio, sai do radar). `Match.OnRadar` entra em `Concealed`, `ViewFor` (só o time do robô vê; os outros nem recebem a lista) e `EventsFor` (`revelar` com `info="radar"` só para o time do robô). Teste: `CelinosRadarRevealsOnlyToItsTeam`; o teste antigo `CelinosRevealsWithinFourCellsImmediately` foi adaptado.
- **Domínio de xadrez:** mover inimigo vale **uma vez por domínio** (`LunaMachine.chessFoe`, zerado em `OpenChess`); aliados e peças, sem limite. Teste: `ChessDomainMovesAnEnemyOnlyOncePerUltimate`.
- **Jogador que cai:** a partida continua parada, como estava (decisão do autor).
- **Esfera:** o movimento que falha conta como usado, como estava (decisão do autor).
- Descrições atualizadas em `Content.InventoresLuna.Descriptions.cs` (Roque, mover-*, Celinos).
- `ArenaGame.cs`: mensagem sem o campo `protocol` agora é tratada como versão incompatível.
- `ClosedMapTests`: pula as Docas (aberta), como na cópia do Somnax.
- Estado novo (`radar`, `chessFoe`) só acrescenta campos; o protocolo continua 14 no principal (o Somnax sobe para 15).
- **Resultado:** `Artifacts/revisao020-editmode-4.xml`: **691/691**.
- Build Windows refeito com a 2ª rodada (`Artifacts/build-claude-principal.log`): **Success, 0 erros**.
- Contrato de rede com esse build (cópia sem acento): **PASS, 10 cenários**, 3 de 4 execuções. A 1ª, logo depois de copiar o build, deu timeout esperando o turno do time 2 (partida a frio); as 3 seguintes passaram.

## Integração do Somnax no principal
Ao trazer a cópia do Somnax, mesclar à mão os trechos marcados "Revisão 020" (principal) e "Revisão Claude (23/10)" (cópia) em `Match.cs` (`ViewFor`, `Concealed`), `Match.InventoresLuna.cs`, `ArenaGame.cs` e `Tools/network-contract.mjs`, e subir o protocolo para **15** nos dois lados (C# e contrato). Rodar de novo os testes EditMode e o contrato depois da mescla.

## Decisões de regra que o autor respondeu (histórico)
- `?` **Roque:** `RoqueCheck` mede 3–8 casas entre os dois aliados, mas não a distância do Desland até eles (a ficha diz maxRange 8). Pode ser usado em aliados longe dele.
- `?` **Radar do Celinos:** o `revelar` muda `hidden` para todos os times, não só para o dono. Em partidas com 3+ jogadores, um terceiro time também vê o invisível.
- `?` **Domínio de xadrez:** `mover-peao/torre/cavalo/bispo/rainha` também movem lutadores inimigos (a descrição diz "personagem ou peça").
- `?` **Jogador que cai:** `ArenaGame.Left` espera a reconexão. Com o tempo de turno desligado (padrão 0), a partida fica parada no turno dele. Sugestão: passar o turno automaticamente após X segundos.
- `?` **Esfera:** `MoveSphere` marca `sphereUsed` mesmo quando o caminho falha, e aí o cubo do fim do turno não sai.
- `?` **Protocolo:** `ArenaGame.cs:906` compara com o literal 14 em vez da constante; o Somnax sobe para 15 e precisa trocar os dois.

## Multiplayer real: NÃO validado
O contrato automático host+cliente (incluindo reconexão) passou, mas não houve partida real entre pessoas com as skills novas. Continua pendente: duas instâncias do build, entrada tardia/reconexão no meio do domínio de xadrez, da matriz e do Viva Luna, e esfera órfã.
