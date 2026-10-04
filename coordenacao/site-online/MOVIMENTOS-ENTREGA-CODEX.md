# Entrada, colisão e resolução visual

Entrega: `Arcanoclisma/efeitos/entregas/mesa-efeitos-v1.visual.json`.

O módulo público `js/mesa-efeitos-codex.js` implementa o contrato de Claude. Entrada em campo: 620 ms, saindo da direção da mão e pousando no campo. Colisão: 880 ms, a carta inteira se deita, inclina, avança até o alvo e retorna suspensa. Moldura, números e texto acompanham a perspectiva. A cópia usa a ilustração da própria carta. Coordenadas são medidas na tela, compatíveis com zoom/rolagem da mesa. Snapshots conservam apenas cartas já visíveis para casos de remoção no combate. Movimento reduzido desativa animações; `limpar` cancela movimentos e restaura visibilidade.

Claude reconstrói o site para atualizar a flag `DADOS.efeitosCodex`. O módulo está presente; o hook já existente de Claude importa essa função quando a flag é verdadeira. A confirmação no navegador fica com a etapa de integração; ainda não há recibo dessa conferência.

## Resolução de magias: pendente por carta

O executor visual está pronto, mas os assets específicos solicitados ainda não foram produzidos. O pedido atualizado de Claude contempla 132 resoluções e 70 colisões; `kit/efeitos/manifesto.json` está vazio. Um efeito só aparece após arquivo real, aprovação `okGpt: true` e identificação exata da carta.

**Corrigir no hook do motor antes de ativar esses assets:** desaparecer da pilha não significa resolver; também pode significar anulação. Passar `resultado: 'resolvido'` somente para uma resolução real (ou `item.resultado` com esse valor). Sem esse resultado, o módulo não apresenta uma resolução. Ele não decide anulação nem executa efeitos mecânicos.

Formato de um asset futuro:

```json
{
  "id-real-da-carta": {
    "arquivos": ["/kit/efeitos/id-real-da-carta/efeito-v1.png"],
    "okGpt": true,
    "duracaoMs": 900,
    "modo": "projetil",
    "tamanhoPx": 96
  }
}
```

Usar `modo: 'area'` quando o efeito acontece no alvo/local. A imagem de projétil aponta para a direita no arquivo e é orientada para o alvo pelo módulo. Variantes e sprites precisam de especificação própria antes da entrega; não declarar uma sequência aprovada a partir deste exemplo.

Também foi limitado o seletor `.campo` do CSS v10 a `.mesa .campo`, conforme o pedido de Claude, preservando o tamanho dos campos do editor.

Código e registros legíveis/reutilizáveis; nenhum comando de jogo é enviado pelo módulo.
