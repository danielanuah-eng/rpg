# Assets finais prontos para integração — Arena Tática

Claude escreve o código, integra e upa; Codex preparou e verificou imagens no PixelLab. Assets/Tools/código não foram alterados pelo Codex. Este pacote sucede as importações confirmadas nas respostas 003, 005-006 e 004-007 do Claude.

## Importações finais

| Pacote | Fonte na entrega | Destino/ação |
|---|---|---|
| Damian pistol SE | correcoes-sincronia/damianPistolSE/pistol_south-east.png | Sprites/Damian; **9×168**, substituir o provisório 9×172. |
| Oito reparos adicionais | correcoes-sincronia/<key>/<clip>_<dir>.png | Pastas atuais; primeira importação já confirmada. |
| Talyssa hit NW | talyssa-hit-north-west/hit_north-west.png | Já confirmado; 7×168. |
| Loja completa animada | loja-v2/cenario/frame-01..09.png | **Sprites/Vendedor/frame-01..09.png; nove PNGs 384×216; 5 FPS.** |
| Ícones da loja | loja-v2/itens/<id>.png | Sprites/Loja; dez PNGs 64×64 já integrados. |
| Animações de eventos | eventos-cenarios/efeitos/*.png | Sprites/Efeitos; slicing quadrado pelo manifesto. |
| Cabeças/raios | eventos-cenarios/projeteis/*.png | Sprites/Projeteis; nomes de skillId conforme vinculos-eventos.json. |
| Neve e Vulcão | eventos-cenarios/terrenos/neve.png e vulcao.png | Textures; ligar aos dois temas respectivos. |
| Berserk possuído | tchu-tcha-berserk-possuido/sprites/*.png | Sprites/TchuTchaBerserk; oito vistas + 40 tiras; UUID novo no pixellab-ids.json. |

Destinos relativos a `Assets/ArenaTatica/Resources/`. `plano-de-importacao.json` lista fontes, hashes, destinos e estado real, sem executar mudanças. Preservar .meta/GUIDs. Não copiar fontes/grids/ZIPs/candidatos como arquivos de jogo. O baixar.py antigo dos reparos foi bloqueado para proteger o pistol final dos ZIPs em cache.

## Pivôs e tempo

`../auditorias/sincronia-2026-10-10/ancoras-para-claude.json`: **284 clipes** da versão em uso; todas as primeiras poses idênticas à vista de referência em alfa e RGBA visível. Padding muda, corpo não foi redimensionado. **127** origens importadas diferem da registrada. Aplicar `pivot_for_all_frames`, um por clipe, sem recalcular por passo, death ou partícula; PPU 128. Tarefa 009 e 48 prévias comparativas registram a conferência.

Os personagens antigos têm oito vistas paradas e quatro diagonais animadas. Conferir fallback cardinal/espelho e mãos das armas na partida. A forma nova tem **oito direções próprias** para cinco ações; hit sete frames, outras nove, **344 frames**. FPS compatíveis com o código atual: idle 5, walk 10, attack 7, hit 8, death 6. Um pivô registrado por clipe e corpo de referência 128×128. Os 32 clipes não-death retornam exatamente à primeira pose; death não retorna. Holds e recuperação estão documentados em reparos-*.json.

## Loja e terrenos

**O ArenaHud atual troca shopScene.sprite pelo sprite inteiro de Vendedor (linhas 84–86/195); não compõe um overlay.** Portanto, para reduzir trabalho, copiar os nove quadros **completos** de cenario/ para Vendedor e mudar 6→5 FPS. Isso mostra a loja inteira com boca/mão em movimento e fundo estável, sem mudar a composição da UI. Os recortes 160×128 de vendedor/ ou vendedor-estabilizado/ sozinhos cortam o cenário. Overlay estabilizado é alternativa opcional em x112/y56/w160/h128 com adaptação de UI. A cena original de Cenarios/loja.png permanece como fallback.

loja-v2/loja.png é uma tira opcional 3456×216, nove células 384×216; vendedor.png tem nove células 160×128. Ambas exigem slicing específico, não quadrado. Os dez ícones já foram integrados pelo Claude.

Pisos 64×64: pedra→Pedreira; grama com musgo→Bosque; ruinas arenosas→Deserto; neve→Neve; vulcao→Vulcão. Cristal roxo é caverna opcional. Preservar os nomes atuais de lookup para os três primeiros; adicionar tile_neve/tile_vulcao aos temas correspondentes. Avaliar emendas com as prévias 2×2; não foi certificada perfeição seamless. Manter zonas, altura, obstáculos e seleção legíveis, sem mudar a topologia determinística.

## Eventos e validação

vinculos-eventos.json liga **nove ranged skills e 26 eventos** às imagens, clipes e feedbacks. Orientar cabeças/raios pela projeção emissor→alvo; selecionar estilo por skillId, nunca pela largura da imagem. Usar alvos, área/linha e alcance realmente resolvidos, inclusive altura/itens.

Moeda: nove frames 32×32. Cura, impacto e invocação: cinco frames 64×64. Raios: cinco frames 128×128, padding dos originais 128×32. Anéis são overlays; raios modulam o segmento e não substituem a área calculada. Referência: moeda 6 FPS, bursts/raios 10; duração acompanha o evento. Tarefa 010 completa turno/rodada/domínio/eliminado/vitória/renovação da loja e evita cortar hit de 0,875 s com espera fixa de 0,45 s.

ARENA_PREPARE_OK e 24 testes EditMode foram relatados pelo Claude **antes** destes assets finais. Codex validou PNGs, alfa, dimensões, frames, hashes, referências, recuperação e reparos contra a exportação canônica. O novo Berserk preservou 476 PNGs de estados anteriores/vistas/death. Reimportar, rodar testes apropriados e conferir a partida antes do upload. A entrega está pronta para revisão do autor; não foi marcada como aprovada visualmente por ele.


Fila remota consultada ao concluir: Devon, Teantha, Talyssa e Dragão espectral grande sem jobs pendentes. Grupos/direções/URLs em ../estado-pixellab-fila-claude.json. A seleção canônica continua excluindo grupos de revisão e revive isolado não integrado.
