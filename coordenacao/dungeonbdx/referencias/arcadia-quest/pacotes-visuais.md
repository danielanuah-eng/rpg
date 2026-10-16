# ZIPs, RARs e cenários

Os 12 pacotes únicos foram enumerados e extraídos em diretórios temporários confinados, com validação de caminhos. Nenhum executável, macro ou instrução de documento foi executado. Arquivos AppleDouble (`__MACOSX/._...`) são metadados, não documentos adicionais. A extração por si só não significa revisão integral do conteúdo.

## Conteúdo e leitura

| Pacote (sufixo) | Conteúdo identificado | Aproveitamento |
|---|---|---|
| modo_172937.zip | Solo 1.2 de 16p e folhas de impressão A4/Letter | Ciclo solo; separar regras de layouts de impressão |
| herois_presentes_na_17920.zip | Seis PNGs de Faceless/Heartless/Nameless | Variação de magos: rerrolagem, controle, mobilidade e custo de exaustão |
| side_quest_159138.zip | Livro Dungeons of Arcadia de 6p e 17 PNGs | Aranhas, teias, ovos, resgate e recompensas com identidade própria |
| nerd_otaku_rev_03_98218.rar | 12 PNGs D&D/LOTR/Naruto e LEIA-ME RTF | Material comunitário; referência de arquétipos, sem importar personagens/licenças para o jogo |
| promocionais_presente_17919.zip | 24 PNGs de heróis | Utilidade, mobilidade, controle, reações e habilidades situacionais |
| base_inferno_152251.rar | Um Portal Turbulento: missão 4p/cartas 2p | Cenário com objetivo de rede de portais, separado da eliminação |
| base_inferno_152660.rar | Dia de Ação de Tacos: cenário 4p, cartas Base/Inferno 3p cada | Variações de preparação e missão por edição |
| cenario_2_o_ulti_161276.rar | Missão 2 de 4p e JPG da campanha | Escolta de cidadãos com progresso e revelação de recompensa no refúgio |
| tacosgiveday_144259.zip | Cenário 2p, cartas de missão 2p e verso 1p (+metadados Mac) | Documento de evento com condições especiais; não regra-base universal |
| mini_expansao_69402.rar | Fonte de Cura 2p e tokens 1p | Elemento interativo adicional; distinguir fan-made da regra principal |
| natalinos_165746.zip | Seis STL: orc, goblin, presente e bases | Miniaturas estáticas de impressão; não modelos Unity animados/prontos para mobile |
| herois_da_17918.zip | 12 PNGs de heróis clássicos | Identidade visual e passiva individual |

Foram inspecionados os 71 PNGs de cartas/tokens em folhas de contato, além dos JPGs de campanha enviados. As folhas permitem comparar silhueta, template e texto; não substituem auditoria editorial de traduções. Nos PDFs de pacotes com imagem, a cobertura de páginas visualmente examinadas é explícita no inventário.

## Exemplos de gameplay extraídos

- Solo 1.2 p.1–6: pressão por mortes, recuperação de cartas e fase de monstros; p.7–16 traz exceções por cenário e FAQ. Não é só uma partida competitiva com um dos times removido.
- `O Último é a Mulher do Padre - Missão 2 - PT BR.pdf`, p.1 e3: levar dois cidadãos a refúgios; os marcadores transportados permanecem ocultos até a chegada, há progresso por guilda e recompensa limitada. P.2 e4 são mapas de preparação por edição. Implementação futura precisa objetivos persistentes, transporte, queda ao morrer e contadores, além de combate.
- Cartas de Dungeons of Arcadia: destruir ovos, caçar quatro aranhas, derrotar a matriarca, escoltar aldeão e salvar herói em casulo. Teia/casulo/ovos têm função legível; variação do cenário deve alterar decisões e percurso, não só aparência.
- Livro Dungeons of Arcadia, p.2–3: resgates podem aumentar o elenco entre cenários e substituir um herói segundo regras próprias; teias acrescentam custo de movimento, reduzem ataque contra aranhas e podem disparar guarda. Usar como referência para objetivos com consequências, sem importar as exceções automaticamente.
- Fonte de Cura, p.1: portais verdes conectam uma sala adicional, com recuperação de vida/carta ao entrar. Uma adaptação precisa impedir entrar/sair repetidamente para contornar caps de cura e recuperação.
- Heróis promocionais: Koba reposiciona perto de aliado; Pluck empurra após ataque não mágico; Elysia reduz mobilidade; Viktor encadeia ataque sob condição. Inspiram papéis distintos, mas encadeamentos e deslocamentos precisam limites autorais.
- O RTF do lote Nerd/Otaku identifica explicitamente 12 heróis customizados, autor Vítor LeGoLaS e correspondências com miniaturas-base. A recomendação de usá-los juntos por equilíbrio é parte do documento de referência, não instrução do autor do DungeonBDX nem validação matemática dessas combinações.

## Arte e integração

Preservar nos novos assets: identificação imediata da função, proporções estilizadas, armas e magia legíveis, famílias de cores e variações de pose correspondentes ao efeito. Em mapas: ruas/salas conectadas, objetos de missão destacados e fronteiras claras. As imagens de referência não foram convertidas em sprites jogáveis nem usadas para substituir os assets originais.

O kit original atual de 18 FBX continua protótipo estático sem rig. As spritesheets herdadas da Arena usam quadros reais para idle/walk/attack/cast/hit/death quando disponíveis; os três modelos nativos recebem feedback procedural de apresentação, sem declarar animação esquelética inexistente.
