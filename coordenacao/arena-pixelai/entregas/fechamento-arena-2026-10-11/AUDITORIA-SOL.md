# Auditoria atual — 11/10/2026

O snapshot `auditoria-remota-sol.json` contém 47 estados consultados diretamente no PixelLab, sem erros. A cobertura agrupa todas as direções de uma ação mesmo quando estão separadas em grupos 3+1. Presença remota significa produção existente; não implica aprovação visual, exportação selecionada ou integração.

`pedidos-conferidos-sol.json` preserva os campos dos 90 pedidos existentes em `planos.json`, `planos-restantes.json` e `planos-damian.json`, com hashes desses arquivos no momento da leitura. `reconciliar-inventario.py` gera a comparação reproduzível em `lacunas-confirmadas-sol.json`.

## Lacunas dos pedidos registrados

Há **46 pares ação/direção ausentes no estado solicitado**. Essa contagem não é orçamento nem ordem para gerar: ações equivalentes, espelhamentos aprovados e material de outro estado precisam ser avaliados antes do gasto.

| Estado | Pedido não encontrado na consulta atual | Direções |
|---|---|---|
| Alvyr | hit | NW; há registro de espelhamento NE no Unity, sem novo clipe remoto. |
| O Colosso | idle, walk, attack, hit | NW em cada ação: 4 pares. |
| O Colosso | steal-reach, steal-slam, steal-cast, stomp | NE, NW, SE, SW em cada ação: 16 pares. |
| Gryvro, em terra | takeoff | NW. O estado Voo já contém flight nas quatro diagonais. |
| Dragão espectral grande | spawn | As quatro diagonais; as cinco ações básicas já existem. |
| Damian, modo canhão | ataque específico do pedido | Quatro diagonais; comparar o ataque cannon do estado base antes de gerar. |
| Damian, modo pistola | idle e ataque específico do pedido | Idle: oito direções; ataque: quatro diagonais. O estado pistola está sem animações, mas o estado base já contém pistol. |
| Damian, modo martelo | ataque específico do pedido | Quatro diagonais. Existem ataques no estado base e candidatos SE no modo martelo; seleção continua necessária. |

Não foi encontrada uma ausência adicional para os demais pedidos registrados. Isso não encerra pendências de identidade, qualidade, habilidade não descrita nesses pedidos ou integração.

## Produção existente que não deve ser repetida

- Os quatro gatos da Asamyh têm idle, walk, attack, hit, death, zap e spawn nas quatro diagonais: 28 clipes por estado. A conclusão antiga de que três deles estavam sem animação ficou desatualizada.
- Brumak tem cobertura das cinco ações básicas nas quatro diagonais. Os candidatos de correção não equivalem à aprovação: preservar a rejeição registrada do hit SW com rosto branco. Não promover esse candidato por ele existir no serviço.
- Lobo de Cinzas tem as cinco ações básicas e spawn nas quatro diagonais; Ossyro tem as cinco básicas e breath; Nixira tem as cinco básicas. Alvyr tem as cinco básicas com a lacuna hit NW indicada acima.
- Guardião de Musgo já tem walk nas quatro diagonais. O novo salvamento deste lote é o enquadramento do idle NW, não uma nova geração do walk.
- O modo martelo do Damian tem idle-hammer nas oito direções, além de candidatos SE. O manifesto `martelo-v3-aprovado/manifesto.json` e o monitor de 17h43 registram a seleção existente: não repetir os idles.
- O modo canhão tem idle-cannon nas oito direções. O nome antigo idle-canhao-v3-pose não coincide com esse rótulo; a comparação marca cobertura equivalente para revisão, em vez de tratar as oito direções como falta.
- As três transformações no estado `Camaleão na mão v2, apoio comum` têm quatro diagonais cada. O estado antigo `Camaleão na mão` vazio não deve ser confundido com o estado atual. O fluxo ombro → mão → arma ainda requer avaliação da sequência e da integração.
- O kit anterior de 176 GLB e 16 layouts/composições é registrado como integrado nas respostas do Claude. Os cinco props deste lote complementam esse kit; não substituem seus arquivos.

## Eventos, efeitos e integração

A tabela real de 12 eventos está em `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Match.cs:797–800`; `Core/Events.cs` não existe. O gatilho do Cataclisma está em `Match.cs:631`. A auditoria de código encontrou aviso textual em `ArenaPresentation.cs:598`; os símbolos centrais produzidos aqui ainda precisam ser ligados a esse evento pelo Claude.

Os avisos deste lote cobrem os 12 IDs, inclusive Papa-moedas e Errante. A conferência dos PNGs, dos GIFs e dos atlas passou. A prévia HTML foi conferida no navegador, incluindo troca de evento, pausa e movimento reduzido. Essa validação não é um teste do Unity.

A auditoria DeepSeek encontrou ausência de `Resources/Sprites/Ceus` e das novas explosões do Festival de Fogos na pasta integrada examinada. Também apontou 11 ícones novos de loja sem correspondência na pasta integrada consultada. Esses achados são **lacunas locais de integração ou localização**, não prova de ausência no PixelLab ou nas entregas concorrentes. Conciliar as entregas antes de produzir novamente. O mesmo cuidado vale para efeitos de habilidade, itens e status do legado.

## Tratamento das dúvidas dos relatórios DeepSeek

Os dois relatórios originais foram preservados em `auditoria-deepseek-*.json`. O Sol tratou suas pendências assim:

| Limite ou dúvida | Resultado |
|---|---|
| Snapshot remoto antigo, gatos sem clipes e canhão incerto | Substituído pela consulta atual dos 47 estados e pela comparação dos 90 pedidos. |
| Grupos divididos ou variantes | União por ação/direção; comparação também por ID de grupo. Variantes não recebem aprovação automática. |
| ZIPs, PNGs e GLB não vistos pelo DeepSeek | As fontes dos sete clipes foram medidas; os 42 quadros e as 343 imagens fora do alvo foram comparados. Os 12 eventos e os cinco props passaram em verificadores próprios e tiveram contato visual visto pelo Sol. |
| Fontes antigas de código, testes e integração | Registros do Claude são identificados como registros. Não houve teste Play nem mudança de Unity neste lote. |
| Proposta antiga de quatro props | O lote atual contém cinco props originais, um por tema, compatível com a proposta posterior do Astra. |
| Ícones, céus e efeitos do legado fora da pasta integrada | Mantidos como pendência de reconciliação; não declarados ausentes globalmente nem regenerados. |
| Aprovação visual e corte no cliente | Avaliação do Astra/autor e confirmação no cliente continuam abertas. Centralização não foi apresentada como recuperação de pixels. |

As próximas ações são avaliar este lote, integrar os avisos e os GLB, confirmar o visor dos dois personagens e selecionar material já existente para as 46 lacunas de pedido. A Arena inteira permanece em andamento.
