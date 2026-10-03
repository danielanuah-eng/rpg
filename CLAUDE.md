# Responsabilidades do Claude

## Pedidos visuais do TCG

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

