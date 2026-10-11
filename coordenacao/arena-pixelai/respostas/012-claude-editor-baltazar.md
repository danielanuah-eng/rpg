# 012 — Resposta do Claude

Claude, 10/10/2026. **Recebida e implementada em parte.** Os itens fechados abaixo foram testados.

## Baltazar
- A ficha está em `Core/Content.cs` (`baltazar`). Os sprites, o anjo e o demônio foram importados.
- **Dualidade guardiã:** é o tipo novo `SkillKind.Aura`. Custa **1 PA por turno** mantido e fica ligada até o jogador desligar. Alcance de 2 casas: cura 1 nos aliados e fere 1 nos inimigos. O efeito sai da resolução lógica (`Match.AuraPulse`), nunca da colisão dos acompanhantes.
- O anjo e o demônio orbitam só enquanto a aura está ligada. Somem quando ela desliga ou quando o Baltazar cai.
- Os números são provisórios.

## Crítico e armadura
Ficam como demanda futura do PvPvE. A Arena não mudou.

## Aba Edição de jogo
- **Fichas e skills:** a aba Editor permite editar vida, PA, PM, alcance (em casas) e dano. Também salva e restaura o padrão de uma ficha ou de todas. As edições salvas carregam sozinhas ao abrir o jogo. Na sala, valem as edições do Host, que vão no estado da partida.
- **Mapas:** a aba Mapas tem o editor de mapas dentro do jogo (`ArenaMapEditor.cs`).
  - Criar um mapa vazio (12×12, 16×16 ou 18×18) ou partir de um mapa sorteado.
  - Ferramentas: subir e descer relevo, árvore, arbusto, caixa, limpar casa, zona central e mover peça.
  - Espelho ligado por padrão (mapa justo).
  - Escolher o cenário.
  - Validação: as casas de início e a zona central precisam ter caminho (`MapGen.Problem`).
  - Salvar em `persistentDataPath/arena-mapas.json`, usar nas próximas partidas ou "Salvar e jogar".
  - Na sala, o mapa do Host vai no estado da partida.
- **Props:** o kit Blender (tarefa 011) já está integrado. O editor troca o modelo de cada peça e coloca 14 enfeites, entre eles o relicário e a barricada (atualizado em 11/10).

## Testes
- EditMode: **54/54**, incluindo `SavedMapIsUsedAndKeepsSpawnsFree`.
- `Tools/smoke.mjs`: local, host e cliente PASS.
- `Tools/network-contract.mjs`: 10 cenários PASS.
- No Play: editar, salvar e jogar no mapa salvo. Conferido em captura.
- Build Windows: Succeeded, 0 erros.

## Pendências reais para fechar a Arena
1. **Catálogo de assets na aba:** ainda não lê `entregas/editor-jogo-2026-10-10/catalogo.json`. Falta mostrar preview, ID, versão e os estados entregue, importado, disponível e pendente.
2. ~~Câmera em 4 vistas~~: feita em 11/10 (ver 013-claude-edgar-mapas-rotativos.md).
3. ~~Props no editor~~: feitos em 11/10.
4. **Idle do Damian por forma:** pedido ao Codex na tarefa 015.
5. **PvPvE:** por último, como combinado.
