# Entrega visual do Sol — Arena Tática, 11/10/2026

Este lote contém sete clipes reenquadrados e salvos no PixelLab, os 12 avisos centrais do Cataclisma e cinco props originais animados no Blender. A produção e a validação técnica deste lote terminaram. Avaliação do Astra e integração no Unity continuam pendentes; a fila inteira da Arena não está concluída.

## Conteúdo e estado

| Entrega | Arquivo de entrada | Conferência |
|---|---|---|
| Baltazar: idle, walk e attack W/SW; Guardião de Musgo: idle NW | `correcoes-urgentes/enquadramentos.visual.json` | 7 tiras, 42 quadros, RGBA idêntico à fonte após deslocamento constante. Os 343 outros PNGs dos dois downloads permanecem idênticos. |
| Avisos do Cataclisma | `eventos-centrais/eventos.visual.json` | 12 eventos, 144 quadros, alfa 0–255, atlas e reprodução dos GIFs conferidos pixel a pixel. |
| Props por tema | `props-blender/props.visual.json` | 5 GLB, todos com curvas móveis reais em `AmbientLoop`, ciclos fechados de dois segundos e sem geometria da apresentação. |
| Inventário e pedidos antigos | `AUDITORIA-SOL.md` e `lacunas-confirmadas-sol.json` | 47 estados remotos consultados, 90 pedidos reconciliados; não houve geração nova. |

Os PNGs originais dos sete clipes não tocavam a borda do canvas. O ajuste apenas redistribui as margens e mantém o tamanho original; **não comprova a correção do corte observado no visor ou no cliente**. Os ZIPs anteriores, os posteriores e os comandos de desfazer estão preservados. O problema visual deve ser avaliado no cliente antes de ser encerrado.

## Conferência visual

- `correcoes-urgentes/contato-enquadramento.png`: todos os quadros reenquadrados, para comparar anatomia e apoio.
- `eventos-centrais/contato.png`: os 12 símbolos. Cada pasta de evento contém um `preview.gif`.
- `eventos-centrais/preview.html`: prévia local interativa, com escolha do evento, pausa e movimento reduzido. Conferida no navegador: troca para Errante, pausa, modo estático e carregamento sem erros no console.
- `props-blender/preview-animado.gif`: apresentação conjunta do lote; `previews/` contém os renders individuais do ciclo.
- `props-blender/fontes/props-cinco-temas.blend`: fonte editável com os cinco modelos, câmera e iluminação da apresentação.

## Orientações de importação para o Claude

**Enquadramentos:** as tiras são horizontais e os quadros conservam dimensões, contagem e pixels do download posterior do PixelLab. Preservar o timing existente: os downloads não informam duração. O manifesto registra o deslocamento por clipe e o delta de pivô normalizado para comparar o apoio entre ações. Não centralizar cada quadro separadamente. Conferir a área de corte do visor/importador antes de afirmar que o defeito original terminou.

**Eventos:** usar o ID emitido em `cataclisma.info`, conforme `Core/Match.cs` e `ArenaPresentation.cs`. O manifesto cobre os 12 IDs reais, incluindo Papa-moedas e Errante. Cada atlas tem 384×288, com 4 colunas e 3 linhas, em ordem de leitura. Cada quadro tem 96×96 e dura 160 ms; um ciclo dura 1,92 s. Proposta: um ciclo no centro da tela, amostragem Point, sem mipmaps, pivô central. `estatico.png` atende movimento reduzido. Esses avisos não substituem os efeitos de campo, céu, habilidades ou regras do evento.

**Props:** estandarte da clareira (Bosque), braseiro de obsidiana (Vulcão), fonte de geada (Gelo), cata-vento solar (Deserto) e ampulheta das ruínas (Ruínas). Cada GLB exporta seu `AmbientLoop`. Usar escala em metros; o exportador converte Z-up do Blender para Y-up do GLB. Base no pivô zero; largura e profundidade menores que um metro durante o ciclo. São decoração: colisão e obstáculos dependem da integração do Claude. O braseiro inclui o ponto opcional `fx_fumaca`. Não foi testado o playback destes GLB no Unity.

## Custo

Consumo de gerações PixelLab neste lote: **0**. Saldo confirmado ao fechar a produção: **165**, crédito monetário PixelLab **US$ 0**, renovação em **10/11/2026**. As artes dos eventos e os props foram produzidos localmente. As auditorias de leitura utilizaram DeepSeek via OpenRouter conforme AGENTS; seu custo monetário não foi informado e não é estimado aqui.

## Reproduzir as verificações

Executar a partir da raiz do repositório com Python contendo Pillow e NumPy:

```powershell
python coordenacao/arena-pixelai/entregas/fechamento-arena-2026-10-11/correcoes-urgentes/verificar-enquadramento.py
python coordenacao/arena-pixelai/entregas/fechamento-arena-2026-10-11/correcoes-urgentes/exportar-tiras.py
python coordenacao/arena-pixelai/entregas/fechamento-arena-2026-10-11/eventos-centrais/verificar.py
python coordenacao/arena-pixelai/entregas/fechamento-arena-2026-10-11/props-blender/verificar.py
python coordenacao/arena-pixelai/entregas/fechamento-arena-2026-10-11/reconciliar-inventario.py
```

`eventos-centrais/produzir.py` recria os desenhos e os atlas. `props-blender/construir.py` recria modelos, animações e renders com Blender 5.2.2 LTS. `correcoes-urgentes/exportar-tiras.py` apenas empacota os PNGs posteriores; não aplica novamente o deslocamento.

O manifesto `entrega-sol.visual.json` relaciona os arquivos desta entrega e seus hashes. Não inclui os arquivos concorrentes anteriores na mesma pasta. Nenhum código ou configuração do Unity foi alterado pelo Sol neste lote.
