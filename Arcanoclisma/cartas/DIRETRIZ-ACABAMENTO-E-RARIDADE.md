# Raridade e arte alternativa — direção do autor

Revisão 9, 03/10/2026. Destinatário: Claude e agentes de produção de cartas.

## Regras de uso

- Raridade é **somente estética**. Não modifica custo, Grau, recurso gerado, ataque, vida, habilidade, velocidade, efeitos ou balanceamento. Variantes visuais compartilham a definição mecânica da carta.
- Claude informa `raridade` em **todo pedido e prompt de imagem**: `comum`, `incomum`, `rara` ou `lendaria`.
- Informar também `arteAlternativa: true | false`. Arte alternativa é uma variante visual independente da raridade; full art por si só não muda regras nem implica raridade lendária.
- No renderer, os campos ficam em `cosmetico.raridade` e `cosmetico.arteAlternativa`. Não enviar esses dados privados junto de uma carta oculta; todos os versos continuam iguais.
- Toda ilustração lendária **ou** alternativa segue o padrão obrigatório de iluminação, textura e brilho descrito abaixo, além do acabamento de interface.

## Acabamentos atuais

| Perfil | Aparência |
| --- | --- |
| Comum | Arte normal, sem foil adicional. |
| Incomum | Acabamento da rara anterior (v5), prata/roxo e brilho moderado. |
| Rara | Película e varredura da lendária anterior (v7), recoloridas em prata/roxo. |
| Lendária | Arco-íris mais intenso, luz dourada e textura holográfica sobre a face. |
| Arte alternativa | Padrão luminoso da lendária, com composição full art e ilusão de profundidade contida na carta. |

## Bloco obrigatório para prompts lendários/alternativos

Acrescentar às instruções de identidade e cena: **“Iluminação focal coerente, pontos de luz expressivos e reflexos dourados/iridescentes; realçar o brilho das magias presentes na cena, com núcleo luminoso e transições suaves para a cor do Valor. Textura gráfica fina e materialidade visível, preservando o traço, as cores e a leitura da figura. Brilho concentrado, sem apagar rosto, detalhes ou silhueta. Composição preparada para o acabamento holográfico dourado e arco-íris da carta.”**

Para alternativa, acrescentar: **“Full art: personagem integrado ao ambiente. Criar ilusão de volume e profundidade com escorço, sobreposição de planos, contraste entre primeiro plano e fundo, iluminação de contorno e sombras coerentes. A figura parece avançar em direção ao observador, mas tudo permanece dentro da imagem e dos limites da carta.”**

Sem magia na descrição, concentrar o brilho na iluminação/reflexos de materiais e do ambiente; não inventar habilidade mecânica ou alterar a identidade da carta. Preservar a direção de arte aprovada. Personagem existente requer cena e pose novas, especialmente se já tiver carta de Conjurador.

Não gerar nomes, números ou regras como parte da ilustração: esses campos continuam editáveis no template. Transparência depende da função do asset; cenários de carta podem ser opacos, recortes e overlays devem ter alfa medido.

## Campos que Claude envia

```json
{
  "raridade": "lendaria",
  "arteAlternativa": false,
  "acabamentoVisual": "arcanoclisma-v9",
  "cena": "Descrição atual da carta e de sua ação",
  "arteObrigatorio": ["Iluminação, textura e brilho conforme a diretriz v9"]
}
```

O plano deve registrar os campos cosméticos e incluir o bloco de brilho antes de gerar. Arquivar prompt completo, fonte da identidade, arquivos da arte, parâmetros de acabamento e revisão na entrega. Aplicar tanto às criações quanto às revisões de lendárias/alternativas; não refazer automaticamente o catálogo já aprovado.

## Integração local

Ler `../arena/acabamento-v9.json`, carregar `acabamento-v9.css` após `cartas.css` e montar o filtro SVG `arcano-highlight-v9` disponível em `../arena/index.html`. O filtro realça regiões luminosas por intensidade de cor; não identifica semanticamente magia. Iluminação de magia precisa existir na arte/prompt.

Camadas: arte 0; realce e profundidade 1; moldura 2; textura/reflexo 3; textos/estatísticas 4; recursos 5. A varredura fica recortada à carta e passa sob os dados legíveis. Movimento reduzido conserva acabamento estático. A profundidade alternativa não usa deslocamento para fora do retângulo.

Texturas vetoriais e PNGs semitransparentes separados: `foil-arcoiris-dourado-v9` e `foil-prata-roxo-v9`. Arquivos originais da arte permanecem preservados. Claude confirma integração em um recibo de entrega; esta documentação local não confirma leitura ou contato com a conversa privada dele.

## Iluminação de cenários e derivações — 03/10/2026

Direção mais recente do autor: fundos de carta têm iluminação difusa e discreta, com hierarquia semelhante à referência Altered e a paleta/traço do personagem regular. Evitar HDR, céu estourado, anime genérico, bloom por toda a paisagem e dourado extremamente resplandecente. Cenários dourados usam ocre/âmbar/bronze envelhecido; brilho forte fica focal nas magias relevantes. Não ampliar a aura da personagem para iluminar toda a cena. Variar pose, olhar, expressão, roupa e interação, preservando identidade. Após cada carta, identificar criaturas, itens, lugares e eventos realmente visíveis em elementos-derivaveis.json, como ideias para outras cartas; o Claude define a necessidade e mecânica, sem produzir novas cartas por iniciativa própria. A arte do dragão da travessia foi aprovada e deve ser preservada.
