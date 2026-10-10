# 014 — Revisão das animações no PixelLab (pedido do autor, 10/10)
**De:** usuario (via claude)
**Para:** codex
**Status:** aberta, PRIORIDADE 1 (junto com a 012)
**Responsável:** codex (animações no PixelLab e reimportação); claude (tempo, gatilhos e integração no Unity)

As animações dos lutadores foram geradas pelo Claude no PixelLab, e o autor pediu que você as revise e corrija.

## 1. Todos os personagens: coerência e quadros quebrados
Revise **todos os clipes de todos os lutadores**:
- **Clipes base:** idle, walk, attack, hit e death.
- **Clipes de assinatura:** summon, teleport, leap, pistol, cannon, hammer, morph, mirror, transform, ultcharge, ultshot e hide.
- **Direções:** as 4 diagonais.

Corrija o que estiver incoerente ou bugado:
- membro que troca de lado ou some;
- arma que muda de mão;
- quadro que pula;
- direção errada;
- pose que não combina com a do quadro anterior.

**Exemplo dado pelo autor:** no **Tchu tcha**, num momento a mão dele fica virada para trás, como se defendesse de costas. Confira também o **Berserk** (`TchuTchaBerserk`).

## 2. Ceifeira (Teantha): ataque mais vivo
O autor achou o ataque **muito parado**. Ele quer a Teantha **se jogando para a frente e golpeando com a foice**:
- antecipação curta;
- avanço do corpo;
- golpe em arco;
- volta à guarda.

Pasta `Teantha`, skill de ataque `foice-verde`.

## 3. Dragão azul da Ciantia (Dragão espectral): animação melhor
Melhore idle, walk, attack (a baforada estelar), hit e death do **Dragão espectral**, na pasta `DragaoEspectral`. Ele é a invocação da Ciantia, voa e tem 192 px.
Ele deve parecer vivo:
- asas batendo;
- corpo flutuando;
- baforada com o corpo indo para trás e depois para a frente.

## Formato (obrigatório, o importador depende dele)
- **Arquivos:** `Assets/ArenaTatica/Resources/Sprites/<Pasta>/<clipe>_<direção>.png`. Cada arquivo é uma tira horizontal de quadros quadrados do mesmo tamanho.
- **Direções:** `north-east`, `north-west`, `south-east` e `south-west`.
- **Pé:** manter na mesma linha em todos os quadros, porque o pivô é o pixel opaco mais baixo. A figura não pode "pular" entre quadros.
- **IDs dos personagens no PixelLab:** `ArenaTaticaUnity/Tools/pixellab-ids.json`. A reimportação usa `Tools/pixellab-import.ps1 -Id <id> -Pasta <pasta> [-Estado Idle|Berserk] [-SemRotacoes]`.
- **Créditos:** antes de gerar, confira com `get_character` o que já existe, para não duplicar. Se os créditos do PixelLab acabarem, avise.

## O que mais depende de mim
- **Tempo do ataque:** está em `ArenaPresentation`. O golpe aparece na metade do clipe, e o projétil sai no quadro do disparo. Se precisar, diga o quadro exato do impacto de cada clipe de ataque.
- **Efeitos:** o impacto do dano agora usa partículas no código (faíscas, anel e recuo do alvo). Se fizer sprites de efeito, entregue em `Sprites/Efeitos/<id>/`.

Resposta: `respostas/014-codex.md`, com a lista do que mudou por personagem/clipe, os hashes e as pendências.
Depois de cada lote, rode de novo a leitura e o diagnóstico (como na 012) antes de dar o lote como pronto.
