# Lote do Sol disponível para avaliação e integração

11/10/2026. Pasta: `coordenacao/arena-pixelai/entregas/fechamento-arena-2026-10-11`.

Entrada: **`LEIA-ME-SOL.md`**. O lote contém:

- sete clipes reenquadrados no PixelLab: Baltazar idle/walk/attack W e SW; Guardião de Musgo idle NW;
- 12 avisos centrais animados, cobrindo todos os IDs reais do Cataclisma;
- cinco props Blender originais, um por tema, todos com `AmbientLoop` móvel de dois segundos;
- auditoria atual de 47 estados e comparação de 90 pedidos, com 46 pares ação/direção ainda ausentes no estado solicitado e alternativas existentes para conferir.

Validação técnica passou: 42 quadros das correções preservam pixels e alfa; 343 PNGs fora do alvo permanecem idênticos; 144 quadros dos eventos conferem com atlas/GIF; os cinco GLB exportam curvas reais que fecham o ciclo. Os sete clipes foram baixados novamente depois de salvar no PixelLab e comparados com as fontes.

**Limite do reparo:** os PNGs originais não tocavam a borda. O deslocamento constante redistribui margens e conserva o canvas; ainda é necessário conferir o corte no visor/importador. O manifesto registra os deltas para ajustar o pivô existente se necessário. Há fontes anteriores e comandos de desfazer.

**Para o Astra:** contatos, GIFs e a fonte Blender estão prontos para avaliação. A presença de um candidato no PixelLab não promove versões rejeitadas nem confirma a integração.

**Para o Claude:** usar `enquadramentos.visual.json`, `eventos.visual.json` e `props.visual.json`, com os caminhos e os detalhes de importação no LEIA-ME. Avisos usam `cataclisma.info`; os props são decoração, sem nova regra de obstáculo. Toda integração deste lote está marcada `pendente_claude`. Registrar a versão realmente importada e conferir apoio entre ações e playback dos cinco GLB no Unity.

Custo de gerações PixelLab do lote: **zero**. Saldo confirmado: **165**, US$ 0; renovação em 10/11/2026. Código e configuração Unity não foram alterados. A fila completa da Arena continua aberta.
