# Planilha, skills e arte

Fonte: `KrosArcano/Arcadia_Quest_-_Card_List_1.2.xlsx`; a versão “- Copia” tem o mesmo SHA-256. As 12 abas foram percorridas localmente em modo somente leitura. Fórmulas e valores em cache foram distinguidos; nenhum recálculo/modificação da fonte foi realizado. As contagens abaixo vêm dos registros de dados, não da aparência de tabelas agregadas.

| Aba | Registros após cabeçalho | Interpretação |
|---|---:|---|
| Upgrades | 308 | 542 exemplares pela coluna QTY; não 542 itens únicos |
| Quests | 104 | Missões por coleção/campanha |
| Death Curses | 16 | Tipos/variantes, 48 exemplares por Qty |
| Stone Cards | 40 | Tombstone/Brimstone/Dragonstone, 84 exemplares |
| Heroes | 144 | Entradas de heróis, não quantidade já implementada |
| Monsters | 198 | Inclui versões por nível, não 198 espécies |
| Pets | 78 | 26 pets em três níveis |
| Mounts | 10 | Montarias e habilidades |
| Dragons | 16 | Quatro dragões em quatro formas/fases |
| Dragon Turns | 46 | Tipos de evento, 96 exemplares por Qty |

Set Counts e Item Type by Set são abas agregadas. Upgrades contém 68 ataques próximos, 85 à distância, 104 permanentes, 33 ampliações, oito itens de pet e dez de montaria. O catálogo mistura conteúdo inicial, níveis 1–6, recompensas e poderes de dragão; não converter todas as linhas em uma progressão linear.

## Famílias úteis para skills autorais

| Família observada | Localizador de exemplo | Tradução de design para DungeonBDX |
|---|---|---|
| Flanqueamento/posicionamento | Heroes!B10:F10, Scarlet | Recompensa pequena por posição, limitada por ação |
| Arma alterando alcance | Heroes!B7:F7, Johan | Identidade de arma sem ignorar automaticamente visão |
| Recurso alternativo | Heroes!B8:F9, Kanga/Maya | Troca explícita com limite, sem gerar ações infinitas |
| Apoio com custo de movimento | Heroes!B6:F6, Hobsbawm | Suporte condicionado; ações extras entram no mesmo orçamento |
| Dreno e cura | Upgrades!E15:M15, Life Drain II | Cura pelo dano efetivamente causado, sob cap de cura |
| Controle de rota | Upgrades!E103:M103, Badda-Bomb | Armadilha persistente com limite e sinalização prévia |
| Perfuração | Heroes!B11:F11, Seth; Upgrades!E43:M43 | Penetração limitada e visível, sem imunidade/negação absoluta padrão |
| Retaliação | Heroes!B12:F12, Spike | Reação limitada por cadeia, sem loops entre dois refletores |
| Pet utilitário | Pets!B11:H14, Bones/Gadget | Redirecionamento limitado ou controle de portas/portais |
| Montaria dracônica | Mounts!B5:E5, Tianlong | Ação de sopro com custo e origem definidos |
| Chefe e ambiente | Dragons!B2:N17; Dragon Turns!B2:E47 | Etapas, locais perigosos e janelas de resposta |

Os exemplos acima são referências, não skills já adicionadas. As três linhas de desenvolvimento do autor continuam: pistoleiro com mira/ricochete/ruptura; mago com ataque/controle/defesa/reposicionamento; domador com comando, sopro, proteção e recolher. Cada variante precisa identidade visual, alvo/origem, condição, custo, cooldown e cap compartilhado.

## Combinações a conter

Upgrades!E101:M101 (Dragon Blade) escala por outras cartas de dragão; E129:M129 e E146:M146 acumulam bônus de aliados próximos; Heroes!B86:F86 escala por missões; outras cartas convertem críticos em efeitos extras. São bons sinais para testes de composição, não números para copiar. Evitar que aura × equipamento × pet × rerrolagem multiplique dano ou defesa sem limite. Manter o limite global mesmo em AoE, dano refletido, armadilhas e comandos de companheiro.

Há divergências entre materiais: a planilha descreve Grom como bônus enquanto ferido (Heroes!F5), enquanto o PNG traduzido `card-H-Grom.png` menciona bônus por marcador de dano. Registrar a divergência e conferir a fonte de regra antes de implementar; não presumir que todo paste-up seja normativo.

## Direção visual observada

Cards dos ZIPs de heróis usam silhuetas fortes, cabeças/mãos/armas exageradas, expressão clara, contorno escuro e pintura colorida; habilidades aparecem em bloco separado da ilustração. Moldura marfim ornamentada, campo azul/roxo e ícones grandes organizam nome, vida, defesa, classe e texto. Cards de monstros enfatizam nível/categoria, vida, extermínio, moedas, movimento e ataque. Missões usam arte simbólica e progresso/recompensa legíveis.

As três imagens de campanha/inventário fornecidas mostram grafo de rotas, dificuldades, vencedor, mortes, moedas, títulos e recompensas; o inventário é separado por guilda e herói. Servem para uma futura tela de campanha e organização das abas, mantendo o HUD inferior escuro solicitado. Não basta copiar uma textura de pergaminho: hierarquia e leitura no celular são essenciais.

Mapas dos manuais distinguem ruas e interiores, portas/portais, objetos de missão e perímetros de salas. A geração precisa preservar essa leitura tática antes de adicionar detalhe. Decoração deve parecer rica sem ser confundida com bloqueio funcional. Novas artes/modelos serão originais; os materiais de referência não foram incorporados como assets jogáveis.
