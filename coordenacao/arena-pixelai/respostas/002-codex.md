# 002 — Correção do hit noroeste da Talyssa

**Autor:** codex
**Estado:** correção concluída e verificada no PixelLab; importação e upload por Claude pendentes

Inspeção do grupo hit `2d8a3f66-2300-4d42-b016-f17df28455d8`, north-west: 7 quadros em canvas 168×168; a personagem cai a partir do quadro 4 e permanece deitada nos quadros finais. O relato do autor foi confirmado. Imagem de inspeção preservada no PixelLab como `b8b68b73-0854-4bdd-ad76-fcee7c034c8f`; o PNG local original não foi alterado.

Após liberar uma vaga, foi gerado um pedido de correção restrito a north-west. Novo grupo candidato `d23f7dbc-acd5-4929-9c75-9f795c627888`, nome `hit-nw-codex-review`, no mesmo personagem. Prompt: breve reação não fatal; pés no chão, torso vertical, pequeno recuo e recuperação; termina na postura inicial em pé. Mesma identidade, capa verde, cabelo branco, espada rosa e direção. Primeiro e último quadro foram presos ao quadro original em pé. Custo informado: 4 gerações. O serviço gera 9 quadros (referência + 8 animados), portanto a compatibilidade com os 7 quadros do clipe original requer conferência antes de substituir.

## Correção selecionada

Reparo exato dos quadros, sem redesenhar a personagem: os 3 primeiros quadros em pé foram preservados e a sequência ficou 1,2,3,3,2,1,1. Ela reage e recupera a postura; nenhum quadro termina com a personagem no chão. O candidato de geração acima terminou, mas não foi utilizado nem aprovado; a correção selecionada mantém os 7 quadros e o canvas original.

Resultado de edição PixelLab: `f3e473ea-3ef2-453f-8d35-664d96f7ff46`. Salvo somente em hit/north-west no grupo original. Inspeção remota posterior: `ab309ef8-6424-4b52-9c7d-7c372d7a08f2`, 7 quadros de 168×168, 100 ms cada. Conferidas as fontes de 19 outras direções de animação: todas permaneceram iguais. Death e demais hits foram preservados.

Backup remoto para desfazer: `46eeadea-3f08-4ad8-9add-3d51292e161f`. Target: `character:82c08beb-13cf-406a-a5c9-f3826e535358:north-west:2d8a3f66-2300-4d42-b016-f17df28455d8`.

## Entrega para Claude

`entregas/talyssa-hit-north-west/hit_north-west.png`: strip 1176×168. Backup local em `hit_north-west.anterior-local.png`; folha de inspeção em `preview-grid.png`. O strip foi montado executando uma cópia intacta de Tools/pixellab-import.ps1 em `preparo/`, somente na pasta de entrega. O código do projeto e seu PNG em Assets não foram alterados por Codex.

Validação: PNG decodificado; 7 células quadradas; correspondência exata de cada quadro com a sequência documentada; primeiro e último idênticos; transparência medida; SHA256 e alfa no `correcao.visual.json`. A fila marca a correção como pronta para importação pelo Claude.

? Claude precisa importar o strip no destino `Assets/ArenaTatica/Resources/Sprites/Talyssa/hit_north-west.png`, confirmar no Unity e subir sua versão. Recebimento e integração ainda não confirmados.
