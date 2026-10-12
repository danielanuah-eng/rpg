# 017 — Claude: integrar reparos, efeitos e modelo de interface

**De:** autor, por execução do Sol 6.1
**Para:** Claude
**Status:** aberta — produção concluída, integração pendente
**Aprovação:** automática, explicitamente autorizada pelo autor. Prosseguir com integração e testes; não pedir nova aprovação das artes deste lote.

O autor priorizou: corrigir apenas Baltazar e Guardião de Musgo, depois os efeitos faltantes, e melhorar tela inicial, layout e botões de seleção. O Sol concluiu os assets e o modelo. Claude aplica ao jogo e confirma o funcionamento.

## Entrega

`coordenacao/arena-pixelai/entregas/polimento-ui-efeitos-2026-10-11/`

Ler `LEIA-ME-SOL.md`, `AUDITORIA-SOL.md`, `assets.visual.json`, `inventario.json` e `ui/modelo-tela.visual.json`. Verificação local passou: 231 assets, 2.864 quadros e 56 loops, zero erros. Inclui biblioteca, reuso e aliases; não são 231 efeitos exclusivos. Unity ainda não foi importado pelo Sol.

## 1. Reparos dos dois personagens

Sobrescrever somente as três tiras correspondentes em Resources, preservando GUIDs e referências:

- `Sprites/Baltazar/idle_west`: 4 células de 168 px, 10 FPS.
- `Sprites/Baltazar/walk_west`: 6 células de 176 px, 10 FPS.
- `Sprites/GuardiaoMusgo/idle_north-west`: 4 células de 176 px, 10 FPS.

Refazer o slicing: as metas antigas de 128 px cortariam cabeça e misturariam frames. PPU 128, Point, compressão None, mipmap false, pivô customizado por `unityPivot` do manifesto. Não reamostrar. Conferir chifres/pescoço/cabeça e direção ao reproduzir. Referência extra inicial do PixelLab já foi retirada; as tiras mantêm 4/6/4 quadros. Originais e hashes estão preservados.

Os clipes de ataque e as outras direções continuam existentes. Não refazer nem substituir outros atores por causa deste lote. O enquadramento anterior de sete clipes está em entrega própria; estes três reparos têm precedência sobre suas versões antigas correspondentes.

## 2. Efeitos, objetos e vínculo aos gatilhos

Copiar as entradas de `unity/Sprites` mantendo os caminhos Resource do manifesto e slicing/FPS/pivô individual. Não copiar `unity/Biblioteca` para Resources. Usar PNG RGBA; GIF é só prévia.

- Dez eventos: maré de lava, chuva de moedas, queda de pedras, terremoto, portal caótico, eco ancestral, maré de energia, festival de fogos, nevoeiro e vendaval. Cada um tem `inicio`, `loop`, `fim`; fogos tem explosão separada. Terremoto usa pulso repetido deliberado. Não mudar suas regras ou sorteios.
- Conjuração no lançador, impacto no destino e projétil na trajetória. Escolher arquivo de fase explicitamente: `EffectFrames`/`LoadAll` não deve concatenar conjuração, impacto, uso e permanência do mesmo diretório.
- `peakFrame0` é maior área de alfa, `impactFrame0` é colisão visual. Martelo perfurante colide no quadro 6. Vincular dano/evento ao gatilho atual, sem mudar valores.
- Estados, seis desejos, Devorar, dez usos de itens e dez objetos de campo estão no inventário. Gancho de escalada permanece passivo com ícone existente. Ícones/idle da loja já entregues são reutilizados, sem regeneração neste lote.
- IDs atuais da Asamyh incluídos: `chamar-gato`, `mover-gato`, `faisca-da-varinha`, `aura-de-gatinho`, `noite-dos-gatos`; existem aliases de aura. Reusar os loops quando o estado estiver ativo e retirar ao acabar.
- Mina armada traz `ownerOnly`; respeitar o dono e a visão atual. Efeitos de estado acompanham visibilidade do alvo; `sem-pm-oleo` traz `inheritTargetVisibility`. Não revelar unidade oculta por sua sobreposição.
- Devorar traz `moveFromTargetToCaster`: ajustar o deslocamento entre posições reais. Projéteis estão voltados para a direita; orientar à trajetória. Versões estáticas compatíveis e tiras animadas estão separadas.
- Terreno `lava`, `gelo`, `fogos` tem aliases de loop: substituir a exibição atual só do primeiro sprite por animação durante o estado. Objetos têm surgir/idle/quebrar ou sumir; ligar ao ciclo real e escala da casa.

As sobreposições de efeitos dos demais lutadores não representam clipes corporais completos. Não encerrar a fila 016 com base nelas.

## 3. Aplicar o modelo de tela ao Unity

`ui/index.html` demonstra tela inicial, equipe, catálogo, mapas, tabuleiro e galeria. As capturas registram o resultado. Reaproveitar desenho, cores, espaçamento, retratos, check de seleção, foco e hierarquia de CTA nos Canvas/uGUI existentes.

Manter todas as nove abas atuais, callbacks, catálogo completo e dados existentes. Os seis lutadores do modelo são exemplos com atributos atuais. Acento dourado de interface não muda TeamColor/PlayerColors. Preservar menu ScaleWithScreenSize 1280×720 e HUD ConstantPixelSize + UiScale; adaptar dentro dos sistemas atuais. Manter tabuleiro inteiro visível, sem retornar à câmera com rolagem. Não alterar regras/login/economia por causa do visual.

O Sol não modificou `ArenaMenu.cs`/`ArenaHud.cs`: há alterações concorrentes do Claude nesses arquivos. Aplicar o modelo sobre seu estado atual. Os mapas transparentes do modelo foram renderizados com Blender a partir das fontes existentes; a geometria/fontes originais foi preservada.

## Confirmar a integração

Rodar os testes adequados e verificar Play Mode: animações corrigidas sem recorte, fases/âncoras dos efeitos, terreno em loop, visibilidade por dono, seleção com custo e equipe vazia, callbacks das abas e tabuleiro inteiro. Conferir um caso de cada grupo de gatilhos e ausência de erros no console. Não declarar integração apenas por copiar arquivos.

Responder em `coordenacao/arena-pixelai/respostas/017-claude-integrar-reparos-efeitos-ui.md`, listando importados, gatilhos conectados, testes e pendências reais. Aprovação automática do autor não equivale a teste já executado.

O lote anterior `fechamento-arena-2026-10-11` contém 12 avisos centrais; confirmar sua integração se ainda pendente. Os sete cenários estáticos/skyboxes da tarefa 015 e as animações corporais da 016 mantêm seus estados próprios. Esta entrega não encerra toda a fila nem libera a expansão bloqueada.
