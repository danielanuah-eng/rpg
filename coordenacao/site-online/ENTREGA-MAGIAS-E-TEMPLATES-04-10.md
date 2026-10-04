# Magias, Economia, Valores e zonas — entrega de 04/10/2026

## Comandos atuais recebidos

Foram relidos `CLAUDE.md`, `resposta.claude.json`, `online/pedidos-visuais/site.claude.json` v3 e `efeitos/pedidos/mesa-efeitos-v1.claude.json`. As 210 artes já estão aprovadas e integradas. D20/abertura/ritmo e templates por Valor têm confirmação do Claude. A expansão continua bloqueada para inclusão em jogo.

O autor autorizou push automático de todos os pedidos concluídos. A escolha registrada pelo Claude é manter o tabuleiro inteiro na tela; a câmera fixa v15 permanece arquivada, sem carregamento.

## Entrega

- **Magias v17**: 132 IDs, 396 SVG e 396 PNG transparentes, três camadas por carta. Arte e descrição foram conferidas nas seis pranchas em `Arcanoclisma/efeitos/previas/revisao-artes`. Direção, paleta e movimento são individuais; não são determinados apenas pelo Valor. Feixes se estendem da origem até o alvo; ondas respeitam o campo próprio/rival; cura, portais, selos e ressurgimento têm movimentos separados.
- **Economia**: `modeloConjurador()` leva `compraFinal` ao renderer. A estatística usa um ícone de cartas ao lado da Vida e descrição acessível da compra no fim do turno. O mesmo número acompanha a Vida na faixa de status do Conjurador em campo. Não existe número fixo na arte nem mudança na quantidade comprada pelo motor.
- **Cabeçalho v16**: nome e esfera de Valor ficam na faixa de metadados, acima da arte. O enquadramento começa em 16% da altura; os arquivos de imagem originais não mudaram.
- **Zonas v16**: bandejas e contadores de Grau, baralho, cemitério, banimento e Arcano mais distintos; rótulos compensam parte da escala; celular em pé reserva mais altura ao tabuleiro. Conserva `ajustarMesa()` e o enquadramento inteiro do Claude.

## Integração de magias

`Arcanoclisma/efeitos/entregas/magias-v17.integracao.claude.json` confirma os ganchos conectados pelo Claude e uma conferência no tutorial. O adaptador aceita `resolver({ item, defId, origemEl, alvoEl, alvoRect, meu, resultado, evento, alvosRect?, skinId? })`.

`resultado` deve ser `resolvido`. Eventos: `resolucao`, `habilidade`, `gatilho` ou `entrada`, conforme `gatilhosAceitos`. Um projétil não resolve sem geometria do alvo; anulação/ausência de alvo não ganha impacto de dano. A pilha cancelada usa o retângulo capturado antes de desaparecer. Habilidades e cartas ocultas continuam sob controle do motor.

Toda imagem/skin futura vem acompanhada da revisão do efeito. Se houver `skinId`, criar a entrada explícita `manifesto[id].skins[skinId]` com arte e camadas correspondentes; o renderer não inventa uma variante. Ao atualizar texto/arte, atualizar esse vínculo. Efeitos estéticos não mudam raridade nem regras.

## Verificação e limites

- Cinco testes de contrato/ciclo de vida passaram, cobrindo 132 animações, existência/hashes dos 792 assets, canal alfa, extensão do feixe, cancelamento, redução de movimento, alvo obrigatório e resolução confirmada.
- Sintaxe dos módulos e do renderer conferida com Node.
- A política do navegador bloqueou abrir a prévia `file://`; não foi contornada. Prévia salva para abertura pelo autor: `Arcanoclisma/efeitos/previas/magias-v17/index.html`.
- O Claude conferiu `base-dano-c1-repentina` no tutorial. Revisão visual completa de todas as magias em partidas reais continua pendente, especialmente alvos na pilha, ondas em ambos os campos e ressurgimento.
- Apenas os arquivos visuais e registros desta entrega devem entrar no push; alterações concorrentes do motor pertencem ao Claude.
