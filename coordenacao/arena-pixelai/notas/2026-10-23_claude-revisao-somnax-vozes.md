# Claude → Carlos: revisão do Somnax, guarda do Tchu Tcha e falas (23/10/2026)

Revisão somente leitura da cópia `Artifacts/carlos-somnax-2026-10-23/work/ArenaTaticaUnity` e de `carlos-vozes-2026-10-23`. Nada foi editado na cópia (os arquivos são seus). Corrija na cópia antes de integrar.

## Corrigir antes de integrar
1. **ALTA: protocolo.** O C# está em 15 (`ArenaGame.cs:20` e `:906`), mas `Tools/network-contract.mjs:17,24,29` manda 13: o Host recusa o hello e o contrato de rede (com a verificação de reconexão) nunca roda. No principal o mesmo erro existia (14 contra 13) e já corrigi para 14. Na integração, troque para 15. Também convém tratar a ausência do campo `protocol` como versão incompatível (hoje o padrão do campo faz um JSON sem ele passar como atual).
2. **MÉDIA: guarda passiva do Tchu.** `Match.cs:450` (Hurt) só confere `Alive`, `Def.guard` e distância ≤2. Preso ou banido, ele continua absorvendo dano dos aliados, enquanto a guarda direcional (`GuardActive`) exige `prisoned==0&&!banished`. Use um critério único. Se o desenho for outro, avise o autor.
3. **MÉDIA: PA na 1ª casa de skill de duas casas.** `Match.cs:730`: com `c2` nulo, a checagem de PA é pulada. O Doce e Salgado (5 PA) aparece como usável e destaca alvos com 2 PA, e o Host só recusa no segundo toque.
4. **BAIXA: vazamento em `ViewFor`.** `directionalGuard`, `guardDirection`, `guardSkill`, `pactTargets`, `renounceTargets` e `twinBondUsed` vão crus para os inimigos. Zere esses campos para quem não é aliado, como já é feito com `channeling`.
5. **BAIXA: bot.** `Bot.cs` só desliga a guarda direcional e nunca a liga (`Targets`/`Value` não tratam `GuardaDirecional`).

## Atualização: o autor autorizou corrigir tudo ("pode fazer a festa, mexem em tudo")
Os 5 itens acima foram corrigidos **na cópia** `work/ArenaTaticaUnity`, marcados "Revisão Claude (23/10)":
- `Tools/network-contract.mjs`: protocolo 15; `ArenaGame.cs`: mensagem sem `protocol` é incompatível.
- `Core/Match.cs`: guarda passiva ignora preso ou banido; PA base checado na 1ª casa; `ViewFor` limpa Pacto/Renúncia/Elo do inimigo e a guarda do inimigo escondido.
- `Core/Bot.cs`: `GuardWorth`/`RaiseGuard`: liga a guarda virada para o inimigo quando protege aliado, e a mantém enquanto vale.
- Novo `Tests/Editor/Revisao020SomnaxTests.cs` (4 testes). Resultado: `integracao/editmode-claude-revisao-5.xml` **723/723**. Depois, o teste do bot passou a zerar PA/PM do Tchu (a guarda custa 0 PA), para o Perry não morrer antes e o teste não depender do ataque: `Revisao020SomnaxTests` 4/4.
Revalide os hashes destes arquivos antes de integrar. Na integração, mesclar com os trechos "Revisão 020" do principal (radar do Celinos em `Concealed`/`ViewFor`, `chessFoe`, Roque) e manter o protocolo em 15.

## Falas (14)
- As 14 têm WAV em `export/` e em `Resources/Audio/Vozes` e gancho no código (`ArenaVoiceCatalog`/`ArenaPresentation`).
- Os 4 arquivos com o mesmo tamanho (damian_acerto/pele_d_ark, damian_pistola/martelo) **não** são duplicados: os SHA-256 conferem e são diferentes.
- 8 seguem provisórias, aguardando gravações do autor: jack_ultimate, jack_flecha_abissal, damian_canhao, damian_pistola, damian_martelo, damian_pele_d_ark, damian_acerto, tchu_possessao. Conferidas: damian_morte, primal_cacador, tchu_escudo, tchu_sair_ultimate, tchu_proteger_aliado, tchu_morte.
- O texto difere do roteiro em jack_ultimate e damian_martelo (é o áudio antigo; esperado até a regravação). Outras 5 diferem só em caixa ou pontuação.
- O manifesto diz `integration: nao_iniciada`, mas os áudios já estão no Resources e no catálogo da cópia: atualize o status.

## Conflito com a integração do principal
No principal, alterei `Core/Match.cs` (fim de `ViewFor`), `Core/Match.InventoresLuna.cs` (`RebuildTimelineKeepingSummons`, `LunaDeath`, `SharkSwim`) e `Tools/network-contract.mjs`, além de criar `Tests/Editor/Revisao020Tests.cs`. Revalide esses hashes antes de integrar o Somnax. Detalhes em `respostas/020-claude-revisao-inventores-lua-multiplayer.md`.

## Integrado no principal (23/10, 1.2.13)
- Mescla por hash contra `integracao/baseline-main.json`: 10 arquivos só da cópia + 203 novos copiados; `Match.cs`, `CoreTypes.cs`, `ArenaGame.cs`, `ClosedMapTests.cs` e `network-contract.mjs` mesclados à mão (Somnax + radar/engrenagens/marcas da revisão 020). `.meta` em conflito: ficou o do principal. Protocolo **15**.
- Pedidos do autor na mesma versão: contador de engrenagens (ícone + ×N ao lado da vida) para Nikola/Blip/Blipware/Celinos; esfera do Nepturius flutua (só no desenho); radar do Celinos menor e translúcido; funilaria 3 engrenagens rodada sim, rodada não (`gearEvery`, editável); matriz fechada bloqueia qualquer skill/ataque e a saída só vale a partir do próximo turno; Roque em quaisquer dois aliados, sem limite de distância do Desland; rede de segurança que remove peças do xadrez se o domínio acabou.
- EditMode no principal: **733/733**. Build Windows: Success, 0 erros.

## Publicado no principal (23/10, 1.2.14)
- Música ambiente de RPG desligada: menu e modo sem forma ficam em silêncio (a trilha `Ambient` segue no código, sem tocar).
- Xadrez do Desland: `dominio-xadrez` com recarga 2 (não é mais 1 vez por partida); dano das peças `pieceDamage=2` e rainha `queenDamage=4` (editáveis no `LunaDef`); torre, bispo e rainha deslizam; peão que chega à borda mais longe do Desland vira rainha. Mover inimigo continua 1 vez por ultimate.
- EditMode **736/736**. Build Windows: Success, 0 erros. Contrato de rede PASS 3×. Publicado no itch (windows, 1.2.14).
