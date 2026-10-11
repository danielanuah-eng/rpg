# Arena Tática — plano de fechamento e direção visual

Planejamento do Astra em 11/10/2026. Executor: **Sol 6.1 xhigh**, conversa `01a12c36-88f8-7db3-9760-df681b263fa3`. Claude mantém a programação e integração. Enzo permanece pausado. Este documento organiza o trabalho autorizado; não declara assets novos produzidos ou integração concluída.

O pedido mais recente determina a ordem: corrigir Baltazar e Guardião no PixelLab; conferir lacunas; priorizar os avisos dos eventos fora do PixelLab, economizando créditos; depois produzir props animados ou modelos no Blender. Os pedidos anteriores de personagens, armas e efeitos permanecem na fila. A conversão completa do elenco para 3D é uma etapa posterior, não requisito para entregar os avisos de evento.

## 1. Organização e sequência

| Etapa | Entrega concreta | Execução | Condição de fechamento |
|---|---|---|---|
| 0 — urgente | Baltazar west/south-west e Guardião de Musgo idle north-west corrigidos **no PixelLab** | Sol | Identificar clipes exatos; comparar todos os quadros, margem, identidade e pivô; salvar correção no asset e registrar versão/undo. |
| 1 — auditoria | Matriz personagem × forma × ação × direção e matriz efeito × gatilho | Sol; Astra avalia | Cruzar PixelLab atual, arquivos selecionados e importação. Unir grupos divididos e preservar espelhos documentados. Só lacunas comprovadas entram na geração. |
| 2 — prioridade visual | **12 avisos centrais animados de Cataclisma**, feitos localmente | Sol; Astra avalia | Um por ID real, arte e cores distintas, previews, atlas/quadros, metadados e alfa conferidos. Lote completo pronto para Claude. |
| 3 — Blender | Um lote coerente de props animados para os cinco temas | Sol; Astra avalia | Fontes editáveis, animações exportadas e comprovadas, previews e manifesto; entregar o lote inteiro ao Unity/Claude. |
| 4 — fechamento do elenco | Lacunas reais de Brumak, Lobo de Cinzas, Ossyro, Alvyr, Nixira, demais personagens e armas de Damian | Sol | Completar só o que não estiver válido; separar produção, revisão e confirmação de uso no jogo. |
| 5 — demais efeitos | Habilidades, itens, estados, invocações e efeitos persistentes do cenário | Sol | Cada efeito com ID/gatilho, começo/loop/fim quando aplicável, origem/pivô e duração. |
| 6 — integração final | Catálogo no editor, oclusão, animações do cenário, importações e validação no jogo | Claude; Astra avalia evidências | Matriz de cobertura sem fallback não declarado; cenas reais com quatro câmeras e modelos animados funcionando. |
| Depois da Arena | Conversão comparativa completa PixelLab → Blender; PvPvE/KrosArcano e balanceamento futuro | A organizar após fechamento | Arena encerrada com pendências explicitadas e aceite do autor. |

A auditoria e a revisão de materiais existentes podem acompanhar as etapas 2 e 3, sem iniciar uma segunda fila concorrente de gerações. Se um personagem estiver comprovadamente impedindo jogar, sua correção sobe de prioridade; registrar o motivo. Não aguardar o reset do PixelLab para produzir efeitos locais, revisar exports ou trabalhar no Blender.

**Divisão de arquivos:** Sol possui `entregas/fechamento-arena-2026-10-11/` e a reserva PixelLab. Astra possui esta pasta de planejamento. Claude possui integração/código. Nenhum agente edita o mesmo manifesto simultaneamente. A automação existente respeita `RESERVA-PIXELLAB.json`; não criar outra automação ou reativar Enzo.

## 2. Estado conhecido e o que realmente falta

Esta tabela é um retrato de evidências locais em 11/10, não uma segunda auditoria remota. Um relato de importação não prova qual hash está no Unity nem a reprodução em Play. A auditoria atual do Sol prevalece sobre snapshots antigos.

| Frente | Evidência atual | Próxima verificação/pendência |
|---|---|---|
| Baltazar | Personagem, aura, anjo/demônio e câmera já relatados integrados. Autor encontrou cortes W/SW. | Corrigir somente os clipes afetados; revisar martelo inteiro, pés, cabeça, arco do golpe e as transições entre quadros. Nova versão precisa depois chegar ao Unity. |
| Guardião de Musgo | Walk v3 já solicitado nas quatro diagonais; relatório antigo dizendo “sem walk” está superado. | Reparar idle NW e conferir que a seleção/importação usa o walk v3, sem o template reprovado pelo musgo piscando. |
| Brumak | Há importação relatada de review-v2; walk SW e death NE corretivos já produzidos. | Resolver seleção do hit SW sem clarão branco, descartar visualmente candidatos rejeitados e confirmar hashes usados. Não reiniciar os 20 clipes. |
| Lobo de Cinzas e Ossyro | Gerações e importações relatadas; fontes e jobs já existem. | Conferir spawn do Lobo, baforada de Ossyro, término do death e cobertura real. Exportar/selecionar o que falta, antes de gerar. |
| Alvyr e Nixira | Importações relatadas; Alvyr hit NW usa espelho de NE. | Revisão das ações e direções selecionadas; espelho só conta como solução após conferir assimetria. Snapshot “nenhuma animação” está superado. |
| Raizor, Asamyh, Cheiro Pneu | Raizor 20, Asamyh 32 e Cheiro 36 tiras constam como importados em relato. | Validar seleção e bases corrigidas. Cheiro death NE é espelho de NW; preservar essa decisão ao reimportar. |
| Edgar e forma fantasma | Nota antiga dizia sem importação; relato posterior registra importação. | Confirmar se inclui especificamente as correções de norte, transformação e navio; não tratar importação geral como prova de todas as revisões. |
| Gryvro/voo | Cobertura avançou; há espelhos de walk SE e death SW. Uma direção de takeoff usa fallback. | Conferir takeoff, ligação solo/voo, escala e grupos mais recentes. Não repetir antiga lista de walks faltantes sem consultar. |
| Colosso | Idle/walk/attack/hit NW espelhados de NE; especiais usam attack genérico. | `stomp`, `steal-reach`, `steal-slam`, `steal-cast`: lacunas de animação específica confirmadas no relato, a verificar no estado atual. |
| Gatos da Asamyh | Existem quatro variantes e produção/importação em andamento/relatada. | Por variante: idle, walk, attack, hit, death, zap e spawn. Zap deve terminar vazio; spawn deve aparecer de forma legível. Recolorir só quando anatomia/padrão e frames corresponderem de fato. |
| Domador, Totem, Kellen e demais neutros | Muitos clipes completos ou importados. Domador recebeu walk e idle NW. Totem tem PM=0. | Não criar walk para Totem imóvel. Rever especiais Kellen, morte NW do Totem se ainda faltar, ataque SE refeito do Brutamontes e seleção de Vespa/Viúva/Ogro/Papa/Rei Bobett. |
| Teantha | Revisão anterior cobre 24 clipes; hit/death e ataques corrigidos aprovados nessa revisão. | Conferir importação dessa seleção e runtime; evitar repetir geração aprovada. |
| Damian | Idle martelo de oito direções selecionado/aprovado; outras formas e transições têm produção paralela anterior. | Fechar a matriz de estados descrita abaixo; canhão/pistola e golpes exigem consulta atual, não geração automática. |
| Itens da loja | Ícones/idles produzidos têm registros próprios. | Conferir todos os itens reais do jogo: presença e idle, e efeitos de uso aplicáveis. Gancho de escalada não exige animação de uso pelo briefing. |
| Mapas/editor | Kit de 176 GLB e 16 composições/layouts relatados integrados; editor de fichas/mapas/props e quatro câmeras feitos. | Catálogo visual ainda não consumido; árvores precisam de oclusão. Lotes posteriores Deserto/Vulcão precisam confirmação individual de importação/jogabilidade. |
| Animação do cenário | Geometria entregue; Unity ainda sem vento, água/lava e sockets de fogo/fumaça segundo resposta 011. | Separar “modelo importado” de “animação funcionando”; incluir modelos de modo e caixa quebrada nas verificações. |

Não refazer mapas, personagens ou biblioteca inteira com base no saldo antigo de 947/1100. A lista efetiva para geração é a diferença entre o contrato e a seleção válida atual.

## 3. Créditos: limite, previsão e controle

**Consulta direta PixelLab em 11/10/2026:** 165 gerações restantes de 5.000; 4.835 usadas; crédito monetário **US$ 0,00**; renovação informada para **10/11/2026**. São gerações da assinatura, não dólares e não uma previsão de 165 clipes. Saldo pode mudar enquanto o Sol trabalha.

Distribuição proposta para proteger o fechamento; **tetos de planejamento, não pedidos de geração**:

| Uso | Teto em gerações | Como reduzir |
|---|---:|---|
| Reparos urgentes e defeitos que impedem uso | 12 | Editor exato gratuito; reaproveitar fonte íntegra e corrigir somente quadros/partes afetados. |
| Damian: lacunas por arma/transição | 36 | Reusar martelo aprovado e transições existentes; um piloto por problema antes de expandir direções. |
| Lacunas confirmadas do elenco | 64 | Revisar/importar o já gerado; preservar espelhos válidos; priorizar ações ausentes, depois polimento. |
| Eventos locais, efeitos desenhados localmente e Blender | **0 PixelLab** | Trabalho local com fontes editáveis; aproveitar biblioteca e materiais já existentes. |
| Reserva para reprovação e última conferência | **53** | Não consumir por geração especulativa ou para refazer assets já aprovados. |
| Total alocado | **165** | Meta de consumo: menor possível; nenhum teto precisa ser atingido. |

Regras de execução:

1. Consultar saldo e jobs antes do lote. Registrar asset, ação, direção, modelo, custo estimado e custo observado; descontar gerações mesmo se a arte for reprovada.
2. Calcular `direções realmente ausentes × custo por direção do modo/resolução/quadros`. Medir pelo retorno da ferramenta e saldo, evitando atribuir consumo de outra frente ao lote.
3. Referências anteriores divergem entre 1 geração em alguns pedidos pixminimax e 2–4 em v3. Usar isso apenas para cenário: **12 direções poderiam custar 12–48 gerações**, antes de novas tentativas. Não prometer um número de clipes por crédito.
4. Preferir 4 quadros para idle/hit discretos e 8 para walk/attack/death quando o movimento exigir, respeitando a identidade e os clipes existentes. O modelo pode acrescentar o quadro de referência: registrar o número real no manifesto, sem descartá-lo cegamente.
5. Um piloto reprovado deve provocar correção do prompt/referência ou edição local; não repetir o mesmo erro nas outras direções. Não insistir cegamente em tentativas pagas.
6. Se a conta exceder o teto de uma frente, Astra redistribui apenas saldo livre, usando a matriz atual. Isso não autoriza compra, upgrade ou uso automático de crédito monetário.
7. Para catálogo, alfa, hashes e cobertura, preferir scripts locais. Auditorias grandes usam DeepSeek/OpenRouter conforme AGENTS, com leitura focalizada e sem repetir a mesma análise. Tokens do Sol/Astra, créditos OpenRouter e gerações PixelLab são custos diferentes; não inventar conversão para reais.

## 4. Decisões visuais — personagens e Damian

**Estilo:** preservar a identidade e a paleta das bases aprovadas. Personagens em pixel art legível sobre cenário 3D simples, texturas nítidas e brilho focal. Nada de normalizar à força tamanhos de monstros ou redesenhar personagens por preferência estética.

**Movimento:** idle com respiração pequena e pés apoiados; walk com passada legível; ataque com antecipação, contato e recuperação; hit curto sem trocar direção; death termina imóvel. Death, zap que desaparece e transformação são exceções à volta à pose inicial. Efeito de impacto fica separado do corpo, salvo detalhe já aprovado como parte da ação. Não disfarçar falha anatômica com clarão.

**Enquadramento:** canvas suficiente para toda a arma, asas/chifres/cauda e extensão do golpe. Comparar bbox de todos os frames; contato na borda exige inspeção. Padding não recupera pixels que já foram cortados. Preservar proporção nativa, alinhar apoio dos pés e pivôs; não escalar cada frame individualmente. Medir alfa, não confiar só no fundo exibido.

**Direções:** quatro diagonais conforme contrato geral; oito onde já contratado (Baltazar, formas/idle de Damian). Os cortes west/south-west pedidos agora são exceção explícita. Não multiplicar todos os atores para oito ações animadas apenas porque a câmera gira. Claude deve confirmar seleção/fallback por câmera. Espelhar personagem assimétrico pode trocar mão, arma ou adereços: precisa revisão, nunca equivalência presumida.

**Damian — fluxo obrigatório:** ombro → camaleão na mão → arma escolhida. Troca de arma: arma atual → camaleão na mão → nova arma. Golpe não inclui transformação. A versão antiga puxando a arma das costas não deve substituir este fluxo.

| Forma/ação | Decisão |
|---|---|
| Neutro/camaleão no ombro e na mão | Preservar os estados e referências já existentes; não gerar outro Damian. |
| Martelo | Idle de oito direções aprovado é a referência. Golpe transmite peso, tem arco legível e mantém a empunhadura correta. |
| Pistola | Silhueta pequena distinta do canhão; recuo curto; mão e arma presentes em todos os frames. |
| Canhão | Arma maior, guarda firme, recuo corporal controlado; sem mudar escala ou virar outra arma durante idle. |
| Transformações | Conferir ida/volta, continuidade de posição e cor do camaleão. Reverter clip só se contato e continuidade funcionarem; não presumir que toda animação reversa é correta. |
| Walk/hit/death equipados | Conferir o que o motor realmente usa por forma. Não produzir cegamente combinações extras, mas tampouco declarar “todas as armas fechadas” com uma delas desaparecendo nessas ações. |

## 5. Eventos: 12 artes centrais, sem gastar PixelLab

Fonte executável: `ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Match.cs:797–800`, tabela `Cataclysms`. O documento antigo menciona `Events.cs`, mas a tabela atual está em `Match.cs`. O aviso nasce do evento já sorteado; a arte não sorteia novamente nem muda peso/regra.

**Formato proposto:** base 128×128 px RGBA, pivô central (0,5; 0,5), 20 quadros a 12 fps (aprox. 1,67 s), atlas horizontal 2560×128 e frames PNG. Se houver convenção já adotada pelo executor/importador, manter compatibilidade e registrar o ajuste. Preview GIF é para revisão, PNG é fonte de integração. Renderização por nearest/Point, sem blur. Área visual inicial de aproximadamente 18% da altura da tela, ajustável pelo Claude sem cobrir permanentemente o jogo.

**Animação proposta:** entrada curta, dois pulsos suaves de cor, saída curta. O símbolo permanece reconhecível; alternar acentos entre três cores relacionadas, sem tela branca inteira ou contraste extremo. A forma distingue o evento também sem cor. Nome do evento é texto separado da interface, não texto pintado no sprite.

| ID do evento | Silhueta central | Base e cores de pulsação |
|---|---|---|
| `chuva-de-moedas` | Três moedas em queda, estrela curta | Ouro, âmbar, creme |
| `queda-de-pedras` | Pedra angular descendo, duas lascas | Ardósia, cobre, ocre |
| `terremoto` | Dois blocos separados por fissura em zigue-zague | Terra, amarelo, laranja |
| `mare-de-lava` | Onda baixa de lava sobre rocha rachada | Carmim, laranja, amarelo |
| `portal-caotico` | Aro oval interrompido com espiral/runa | Violeta, magenta, ciano |
| `eco-ancestral` | Pilar/runa vertical com halo antigo | Ouro envelhecido, creme, turquesa |
| `mare-de-energia` | Raio entre duas ondas ascendentes | Azul, ciano, verde |
| `festival-de-fogos` | Três pequenas explosões em leque | Magenta, amarelo, ciano |
| `nevoeiro` | Três faixas de névoa envolvendo uma silhueta | Azul acinzentado, lilás claro, gelo |
| `vendaval` | Espiral aberta com duas folhas | Verde, turquesa, amarelo claro |
| `papa-moedas` | Silhueta reconhecível do Papa diante de moeda | Marrom, ouro, verde suave; identidade pela referência |
| `errante` | Cabeça/silhueta do chefe em portal de pedra | Basalto, âmbar, vermelho; identidade pela referência |

**Aceite:** 12 IDs exatos sem duplicatas; leitura em fundo escuro/claro e no tamanho final; nenhuma borda cortada; alpha correto; duração e pivôs iguais entre arquivos/manifesto. Não exigir alpha opaco em todos os frames se a entrada/saída intencionalmente fica vazia. Claude liga a animação ao evento confirmado da partida, uma vez por ocorrência, e trata sobreposição/fila sem travar o jogo.

O aviso central resolve o novo pedido prioritário. Ele não substitui automaticamente os efeitos de chão, duração de lava/nevoeiro, marcação dos fogos, spawn dos neutros ou céus pedidos antes. Esses itens continuam na etapa 5 até conferência explícita.

## 6. Outros efeitos: organizar por uso para reaproveitar

| Família | Conteúdo a conferir/completar | Decisão econômica |
|---|---|---|
| Combate dos cinco prioritários | Impacto Brumak, baforada Ossyro, cinzas Lobo, chifres Alvyr, garras Nixira já têm exports nesta entrega. | Revisar/empacotar primeiro; não gerar versões novas sem defeito demonstrado. |
| Projéteis | Bola de fogo, disparo dos gatos, tentáculos de óleo, canhão do peito, corrente; conferir demais existentes. | Fontes separadas de personagem; variantes de cor apenas onde a mesma forma serve. |
| Itens de uso | Martelo perfurante, corrente, fumaça, pena de fênix, totem, mina, foco, espinhos, âncora, bênção. | Ícone, idle e uso são estados diferentes. Gancho: só ícone/idle conforme pedido geral, sem inventar ação. |
| Estados | Oculto, preso, sem PM, armadura+, canalização de desejo, buffs PA/PM. | Ícone e silhueta discretos; loop pequeno, início/fim claros. Não esconder o lutador. |
| Habilidades específicas | Faísca, pisão, pancadas, gancho, coroada, pauzinho, ferrão, agulha, casca/raízes, corrente/grito, clava/pedra, rugido. | Reaproveitar impacto realmente compatível; manter gestos próprios e efeitos que comunicam regras diferentes distinguíveis. |
| Kellen/Colosso | Desejos (reviver, cura total, PA, copiar, chuva de itens, apagar evento), devorar e golpes roubados. | Uma família visual por personagem, com símbolos distintos por resultado; fallback attack não equivale a animação específica pronta. |
| Chão/objetos de evento | Lava/resfriamento/gelo, marca de fogos, mina, totem, prisão, baú, portal do chefe, buraco do Papa. | Reusar texturas/modelos já entregues; separar começo/loop/fim quando o objeto persiste. |
| Céus e ambiente | Aberto e versão dourada, nuvens, cinzas, poente, tempestade e noite festiva; vento/água/lava/fogo/fumaça. | Auditar assets existentes e animação no motor antes de produzir; textura/emissão estática não conta como animação exportada. |

## 7. Blender: lote proposto e limites visuais

O kit base já existe e foi integrado; a próxima entrega deve acrescentar variedade. **Proposta de lote: cinco props pequenos animados, um por tema**, reaproveitando materiais e convenções do kit. Antes de modelar, conferir inventário; se uma proposta já existir, animar/complementar a peça válida ou selecionar uma lacuna equivalente, registrando a decisão.

| Tema | Proposta | Animação exportável |
|---|---|---|
| Bosque | Marco de trilha com fitas e sinos de sementes | Fitas e sinos em oscilação curta; tronco/base imóveis. |
| Pedreira/Montanha | Poste de guincho com pequeno contrapeso | Roldana e contrapeso em ciclo lento dentro da base. |
| Deserto | Marco de caravana com tecido e contas | Tecido e contas balançam; paleta areia/ocre, acento turquesa discreto. |
| Neve | Marco com lanterninha e pingente de gelo | Lanterna e pingente oscilam; gelo com brilho localizado, sem depender só de shader. |
| Vulcão | Respiro de basalto com pequenas placas de rocha | Placas articuladas/malhas emissivas sobem e recolhem levemente, sem bloquear leitura da casa. |

As propostas são decoração visual; não criam cobertura, dano, recursos ou novas regras. Cada peça mantém ID e função distintos dos objetos interativos existentes. Bases de até uma casa; movimento não invade a loja, acessos ou casas reservadas. Variações de tamanho/posição ficam no editor sob regras existentes.

**Contrato:** 1 unidade = 1 m = uma casa, nível de relevo 0,45 m; pivô no centro da base; até aproximadamente 3 mil triângulos por peça; texturas de 32–64 px por casa, paleta limitada, sem PBR fotográfico. Unidade/eixos devem ser verificados na importação; não compensar eixo duas vezes.

**Animação:** ciclo de cerca de 3–5 segundos, deslocamento pequeno, curva suave e começo/fim compatíveis. Para o lote, usar transforms de objetos, bones ou outro canal que comprovadamente sobreviva à exportação. Animação de material/shader não é presumida portável. Se Blender mostrar movimento mas o GLB não o contiver, o prop continua pendente.

**Entrega do lote inteiro:** `.blend` editável, GLB principal (o projeto usa GLB), PNGs, clipes nomeados `idle`, previews em vídeo/GIF e manifesto por peça com unidade, escala, dimensões estáticas/dinâmicas, tris, materiais, pivô, duração e nomes dos canais. FBX somente se houver necessidade comprovada do importador. Reabrir fontes e reimportar exportações; gerar preview do arquivo exportado. Claude confirma no Unity a associação do clip e a reprodução; importar geometria não encerra esse critério.

Se o autor preferir iniciar personagens 3D antes do restante, começar por **um piloto comparativo** de personagem existente, com identidade validada, rig e conjunto pequeno de ações. Não converter todo o elenco antes de comparar silhueta e leitura com os sprites. A escolha atual é props primeiro pelo menor risco e reaproveitamento do kit.

## 8. Conferência e encerramento

Estados separados: **existente/gerado → selecionado e revisado → exportado e validado → importado confirmado → reprodução no jogo confirmada**. Preservar rejeitados identificados, sem deixá-los no catálogo jogável. Cada correção mantém origem, ID/grupo/direção e hash da seleção; não sobrescrever silenciosamente espelhos ao reimportar.

Claude precisa conferir: catálogo com preview/ID/versão/estado; importação de Edgar corrigido e demais seleções; oclusão de árvores; vento/água/lava e sockets; modelos de modo/caixa quebrada; layouts Deserto/Vulcão posteriores; avisos dos 12 eventos; idle e golpes de cada arma de Damian; reprodução dos novos props. Editor de fichas e câmera já entregues não precisam ser refeitos.

Testes existentes pertencem às versões em que rodaram: não reutilizar 49/49 ou 54/54 como prova das novas entregas. Para novos assets, verificar alfa, bounds, frames, dimensões e hash; no jogo, conferir loop/pivô, leitura nas quatro câmeras, tabuleiros 12/16/18 e sincronização dos gatilhos sem exigir nova geração visual. O Sol entrega evidências e o Astra avalia; Claude confirma integração.

Commit/push automático apenas de uma meta completa e dos arquivos próprios. O plano completo pode ser publicado como **planejamento**, sem declarar a produção concluída. Arte reprovada, job em andamento ou importação sem teste continuam pendentes. Não atualizar arquivos concorrentes para fazer parecer que a Arena está encerrada.

### Fontes consultadas

- `../RESERVA-PIXELLAB.json`: executor e exclusividade atuais.
- `../tarefas/012-claude-editor-baltazar-balanceamento.md`: limites de Arena/PvPvE, editor e fluxo posterior.
- `../tarefas/014-codex-animacoes.md`, `015-codex-eventos-elenco-cenarios.md`, `016-codex-animacoes-restantes.md`: contratos de produção.
- `../respostas/011-claude-integracao.md`, `012-claude-editor-baltazar.md`, `013-claude-edgar-mapas-rotativos.md`, `015-codex.md`, `016-codex.md`: entregas e integração relatadas.
- `../notas/2026-10-11_monitor-personagens-importados-1813.codex.md`, `2026-10-11_monitor-martelo-importacoes-1743.codex.md`: mudanças posteriores ao snapshot inicial.
- `../notas/2026-10-10_damian-transformacoes-personagens-novos.codex.md`: fluxo de armas aprovado.
- `../notas/2026-10-11_vulcao-passarela-e-confirmacoes.codex.md`: lotes de mapas posteriores e pendências.
- `../entregas/fechamento-arena-2026-10-11/`: fontes, planos antigos, jobs e exports; não presumir que todo snapshot é atual.
- `../../../ArenaTaticaUnity/Assets/ArenaTatica/Scripts/Core/Match.cs`: IDs executáveis de Cataclisma.
- Consulta direta `PixelLab.get_balance`, 11/10/2026: custo disponível; nova consulta necessária antes de gastar.
