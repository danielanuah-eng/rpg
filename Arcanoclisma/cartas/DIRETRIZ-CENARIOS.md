# Cenário obrigatório em toda arte de carta

Pedido direto do autor em 03/10/2026. Aplicar a toda carta final: Conjurador, Aliado, Temporária/feitiço, Mística, Estrutura, Canal, Armadura, Extra e demais equipamentos/itens. Raridade e arte alternativa não restringem esta exigência.

## Instrução para todos os prompts ao GPT/imagegen

> Criar uma ilustração completa de carta com cenário, ambientação e profundidade próprios. Integrar personagem, criatura, objeto ou magia ao lugar e, quando houver, à ação da carta. Usar a imagem do RPG somente como referência de identidade e direção de arte. Não entregar apenas um personagem/objeto isolado ou o recorte original sobre fundo vazio. Retratos ambientados, cenas tranquilas e objetos em um lugar também são válidos: não é obrigatório inventar um evento grandioso. Preservar o traço, paleta e identidade aprovados; adaptar pose, expressão, luz, roupa e contexto quando cabível. Fundo legível com iluminação difusa e hierarquia visual; brilho localizado nas magias, ouro/bronze contido, sem ofuscar rosto, textos ou atributos. A arte ocupa a carta; textos, símbolos e números continuam em camadas editáveis.

Personagens existentes recebem cenas próprias por carta, especialmente quando já possuem carta de Conjurador. Equipamentos podem aparecer em um local, em uso ou ligados a um acontecimento, conforme o briefing; não precisam ganhar um personagem novo sem necessidade. Cenários devem responder à identidade mecânica/narrativa indicada pelo Claude.

## Variedade, escala e horário

- Claude deve variar os briefings entre ambientes naturais, construídos, interiores e exteriores; arquitetura, materiais, estação, clima e escala. Não repetir pontes, portais, ruínas flutuantes, catedrais ou brilho dourado por padrão.
- Codex/Designer pode substituir detalhes estéticos repetitivos do briefing e abstrair a composição, preservando identidade, mecânica e continuidade confirmada. Registrar a escolha; não inventar uma alteração de regras ou uma cronologia canônica.
- Definir na descrição e no prompt o momento/iluminação: manhã, dia, tarde, crepúsculo, noite ou interior com luz caracterizada. Distribuir essas escolhas pelo lote; evitar transformar toda cena em dia claro ou noite azul. Num interior, mostrar o horário por janela/luz externa quando pertinente, sem forçar um céu visível.
- Garantir escala espacial coerente: chão/superfície de apoio, plano principal, profundidade, perspectiva e referências de tamanho. Usar escala monumental apenas quando combinar com o assunto; objetos/equipamentos mantêm proporção legível e personagens não ficam minúsculos para mostrar o cenário.
- Quando o mesmo evento aparecer em outras cartas, registrar `eventoId`, `continuidadeCom` e `relacaoComEvento`: personagem diferente, aproximação, consequência, preparação ou detalhe daquele acontecimento. Manter clima/horário coerentes com a continuidade e descrever essa relação de modo próprio em cada carta.
- Uma série pode compartilhar lugar e identidade visual; variar enquadramento, luz local, atividade e ponto de vista. Não exigir um novo bioma em cada carta nem copiar a descrição de outra.

Campos da entrega: `ambiente`, `momentoDoDia`, `clima`, `escala`, `eventoId` (ou null), `continuidadeCom` (ou []), `relacaoComEvento` (ou null) e `liberdadesVisuais`. Descrições de imagens prontas registram somente aspectos visíveis; horário exato não demonstrado fica como interpretação, não como fato.

## Arquivos e aprovação

- Guardar recortes do RPG como fontes e referências; não sobrescrevê-los para criar a versão de carta.
- Cenas completas podem ter fundo opaco. Transparência continua obrigatória para overlays, molduras e recortes que tenham essa função, com alfa medido.
- Registrar arte de referência, carta de destino, cenário, alterações, prompt e arquivo final na entrega visual.
- Aplicar também o acabamento v9 quando a carta for lendária ou alternativa. Raridade permanece somente estética.
- Uma carta que ainda usa recorte de referência na prévia está visualmente provisória. Não marcar sua arte como final nem inventar uma cena pronta por CSS.
- Esta diretriz não autoriza gerar cartas ausentes do pedido atual ou refazer cenas já aprovadas. Os dois modelos aprovados de Ciantia já têm cenários.

## Acabamento de cenários — preferência confirmada em 03/10/2026

O autor preferiu a V2 das duas cenas de Philgard: cenários mais lisos, menos rugosos e menos extremamente expressivos. Priorizar formas amplas, transições suaves e textura discreta no terreno, rochas, troncos e vegetação; reduzir granulação, rachaduras exageradas, pinceladas agressivas e contraste excessivo nos pequenos detalhes. Manter profundidade, materiais legíveis e detalhes dos personagens, com o cenário apoiando o assunto principal. As V2 de Philgard são referência local desse tratamento. Não refazer automaticamente artes já aprovadas.
