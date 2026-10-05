# Responsabilidades do Claude

## Ajustes atuais de Valores e raridade

Ler `coordenacao/site-online/VALORES-E-RARIDADE-03-10.md` e a entrega `Arcanoclisma/cartas/entregas/templates-valor-v12.visual.json`. Codex cuida de arte, templates e movimentos; mantenha catálogo, editor, triggers, custos e keywords com você. Raridade fica sem aplicação automática no app, **sem apagar** acabamentos já criados; o autor escolhe no editor. Esfera e moldura mostram o(s) Valor(es), e apenas equipamentos podem ser genéricos/cinza. Solicitar artes variadas pelos IDs reais das cartas. O autor reafirmou aprovação de todos os pushes das imagens; publicar a entrega não aprova automaticamente cada arte nova nem libera a expansão.

## Prioridade e autorização atuais (03/10/2026)

Codex está refazendo as 151 artes do jogo atual, carta por carta, com cenas novas e integradas; consuma `Arcanoclisma/cartas/entregas/lote-site-v1.visual.json` por ID. Ler `coordenacao/site-online/INTERACOES-E-NITIDEZ.md` para o pedido de arraste, pagamento confirmado, retorno à mão, tela cheia, efeitos particulares de resolução e cartas dentro da área azul do campo. Codex cria arte e movimentos; você integra regras, alvos, custos e eventos. **O autor autorizou push automático das entregas concluídas e integração com você nesta conversa**, substituindo a exigência anterior de perguntar a cada push desse trabalho. A expansão de 90 cartas A Mestra dos Dragões ainda exige liberação específica para entrar em jogo ou ter suas cartas alteradas.

## Expansão bloqueada: A Mestra dos Dragões (03/10/2026)

Pedido atual do autor: Codex produz somente as artes desta expansão, que introduzirá Ciantia. **Você só poderá colocar a expansão em jogo ou criar/alterar suas cartas quando o autor liberar explicitamente.** A aprovação das imagens e uma eventual autorização de push não constituem essa liberação. Consulte `Arcanoclisma/expansoes/a-mestra-dos-dragoes/expansao.json` e `AGENTS.md`; inicialmente `liberacaoDoAutor.concedida` é false. Esta restrição aplica-se também às propostas de Ciantia/Philgard destinadas a essa expansão. Nomes, descrições e categorias visuais não autorizam regras, custos, raridades jogáveis, balanceamento ou inclusão em baralhos.

## Etapa atual: site online — gestão com Codex (03/10/2026)

Pedido mais recente do autor: reduzir seus créditos, especialmente os gastos gerenciando agentes. **Codex assume a triagem, divisão de tarefas, escolha de agentes, fila e consolidação das entregas.** Você concentra o trabalho em programação, regras, dados, backend, rede e integração.

- Leia uma vez `coordenacao/site-online/DIVISAO.md`. Não é necessário reler todo o catálogo ou todas as skills para pedir algo visual.
- Envie a necessidade atual em `Arcanoclisma/online/pedidos-visuais/site.claude.json`: objetivo, telas/assets, textos/dados, requisitos de integração e prioridade. Não distribua essa mesma tarefa entre vários agentes. Codex decide as funções visuais necessárias e conserva os agentes desligados.
- Consuma o resumo e os arquivos indicados em `Arcanoclisma/online/entregas-visuais/site.codex.json`; registre somente decisões de integração ou bloqueios em `coordenacao/site-online/resposta.claude.json`.
- Artes, templates e acabamento v9 já estão publicados no commit `0b38961`. Reuse-os a partir de `Arcanoclisma/arena/entrega-claude.md`. As propostas de Ciantia estão em `Arcanoclisma/cartas/propostas/PARA-CLAUDE.md`.
- Continue trabalhando nos arquivos do site. Codex escreve os registros de coordenação e assets em seu escopo; não há alteração automática de sua conversa nem confirmação de recebimento até sua resposta local.


## Pedidos visuais do TCG

**Direção atual do autor (03/10/2026): toda carta final tem cenário e ambientação próprios**, incluindo Conjurador, Aliado, Temporária/feitiço, Item/equipamento, Mística, Estrutura e cartas de Grau. Imagens recortadas do RPG são referências de identidade; não são a arte final da carta. Em todo briefing/prompt ao GPT, incluir `Arcanoclisma/cartas/DIRETRIZ-CENARIOS.md`. A revisão da mesa pede contadores discretos sobre o campo, Arcano em esfera lateral, mão fora da mesa e controles/pilha à direita, deixando o centro livre.

**Varie os ambientes e horários entre cartas.** Nem toda cena precisa de um evento grandioso: retratos ambientados e situações calmas valem. Codex pode adaptar repetições estéticas do briefing. Para um mesmo evento em várias cartas, explicite a relação de cada carta com o acontecimento e preserve a continuidade de clima/horário. Inclua ambiente, momento do dia, escala e, quando aplicável, eventoId/continuidadeCom no briefing. A revisão visual da mesa está em `/css/mesa-codex-v10.css` e `/js/mesa-movimentos-codex.js`; preservar os links/hooks de integração.


Arcanoclisma é um TCG. As 435 artes 087–521 estão aprovadas; veja `Arcanoclisma/cartas/catalogo-artes.json`. Designer de Cartas atende somente sua demanda atual de cartas, identidade, conteúdo e uso. Solicite por `Arcanoclisma/cartas/pedidos/<id>.claude.json`, conforme o exemplo e `COORDENACAO-CLAUDE.md`. Pode enriquecer descrições estéticas pobres com eventos, magia, ambientes, personagens e interações, mantendo função e identidade. Não alterar regras por escolhas visuais.

Embelezador atende sua demanda atual de assets para animações, efeitos e movimento em `Arcanoclisma/efeitos/`. Todos os aliados usam movimentação genérica fluida e compartilhada ao atacar, sem efeito específico por aliado. Prefira transformar a carta existente para esse movimento; solicite novos assets só quando necessários. Defina os gatilhos reais, formatos e timing para integração.

Consuma manifestos `.visual.json` e devolva `.integracao.claude.json` com confirmação ou ajustes. Textos de cartas permanecem editáveis no Canva e seguem seu conteúdo exato. Corte planejado de 63 × 88 mm, conforme a especificação de impressão. Canva teve erro de transporte nesta configuração; não há template real gerado ainda. A pasta de pedidos começa sem demanda confirmada. A coordenação ocorre por arquivos/Git; não presumir conexão direta com a conversa do Codex.

O autor atribuiu ao Codex a criação e revisão de imagens, designs e assets visuais, a leitura e análise de referências e modelos de imagem e a criação de modelos 3D quando necessários.

O Claude fica responsável pelas demais partes do projeto, incluindo programação, lógica, sistemas, configuração e integração dos assets entregues pelo Codex.

Ler `COLABORACAO-IAS.md` e as instruções específicas da área em que estiver trabalhando. Preservar assets aprovados; registrar pedidos de criação ou ajuste visual com formato, dimensões e destino esperados.

Usar pastas/worktrees e branches separadas quando os agentes trabalharem simultaneamente. Coordenar alterações em arquivos compartilhados.

Sempre perguntar ao autor antes de qualquer push para o GitHub.
# Memória visual com Enzo

## Coordenação com Vuco vuco

Seguir `COORDENACAO-VUCO-CLAUDE.md`. Receber as solicitações visuais e devolver os comandos/eventos/estados reais de cada movimento no arquivo `<tarefa>.claude.json`, com IDs compartilhados, referências de código e requisitos de integração. Claude escreve sua resposta; Vuco vuco escreve o manifesto visual. Preservar versões e informar mudanças para manter os vínculos imagem–comando atualizados. Não marcar integração como confirmada se estiver apenas planejada. Entregar os registros a Enzo.

Após entregar imagens, cenários, personagens, estruturas, poses ou movimentos e suas descrições, salvar os materiais em arquivos locais do projeto e encaminhar seus caminhos a Enzo conforme `.agents/skills/enzo/SKILL.md`. Registrar a origem Claude somente para materiais efetivamente produzidos por Claude. Guardar prompt, descrição, referências, versões e decisão do autor quando disponíveis. Carlos cuida da produção e avaliação das artes; Enzo preserva para uso futuro. Nunca incluir credenciais.

## Pedido visual atual — artes rápidas (04/10/2026)

O autor pediu artes simples para algumas cartas faltantes e mandou o Codex falar com você. Responda com os IDs prioritários e descrições em `Arcanoclisma/cartas/pedidos/artes-rapidas-jogo.claude.json`; as perguntas e o formato estão em `artes-rapidas-jogo.duvidas.json` na mesma pasta. Os 201 IDs do catálogo local já têm WebP final, portanto indique a necessidade real atual do site/editor ou cartas novas. Priorize de 3 a 5 para começar; Codex produz as cenas e devolve a entrega pelo Git.

Atualização Codex: identificadas as cartas novas de `tcg/src/cartas-provisorias.ts` e os IDs já disponíveis em `online/public/jogo/dados.json`. **Entregues 3 cenas completas novas** para `prov-sublimeyn-etereo-c2` (Garça-de-Neblina), `prov-sublimeyn-etereo-c3` (Grifo-de-Vidro) e `prov-alvorum-anjo-c6` (Cervo Branco). Consumir `cartas/entregas/artes-rapidas-jogo.visual.json`, com os caminhos exatos dos PNGs e hashes; fontes em `arte/cartas-rapidas/`. ArteModo=cena, sem montagem de recorte. Não houve alteração de regras nem das 210 artes anteriores. Confirmar integração no arquivo indicado pela entrega; a lista formal ainda é útil para as próximas prioridades.

