# Passagem — estado atual (04/10/2026, 16h53)

Versão enxuta. Histórico completo (o que foi feito em 03–04/10, decisões 15–26, detalhes de mesa, tutorial, editor, itens v2/v3, construtor de efeitos): `agente/passagens/PASSAGEM-2026-10-04_16-53.md`. Ler só se a tarefa exigir.

## Objetivos do autor
- Site online do TCG Arcanoclisma: https://arcanoclisma.arcanoclisma-online.workers.dev (código em `Arcanoclisma/online/`, motor em `Arcanoclisma/tcg/`). Jogar contra bot e pessoas, tutorial, construtor, Editor do autor, Lore, comentários/ideias.
- O Claude: programação, regras, dados, backend, rede e integração. O GPT/Codex: arte, templates, ícones, animações e a gestão dos agentes visuais.
- Regras de atualização: seguir as orientações novas do GPT; polir textos do autor sem mudar regra (mostrar antes/depois); todo efeito automatizado no motor com teste.

## Equipe de subagentes (definida em 04/10)
Ver `.claude/EQUIPE.md`: `triagem-feedback` (haiku), `motor-regras` (opus médio), `motor-regras-alto` (opus alto), `integrador-visual` (sonnet), `verificador` (haiku); DeepSeek para leitura grande e tela; equipe de balanceamento desligada até o autor pedir.

## Estado
- Publicado e no GitHub em 04/10 à noite (commit ff1d1da, deploy b2a1eccb): Concessão + Reanimum (dec. 27), destruir não mira Conjurador (28), Editor dos Conjuradores visual (29), Protessência (30), pilha sem passe automático quando dá para responder, Ovo de Wyvern com Grau na mesa.
- Testes: tcg 162, online 11, partida online 0 erros. Nada conferido na tela pelo Claude.
- Mecânicas: agentes motor-regras (Opus médio) e motor-regras-alto (Opus alto), pedido do autor.
- Limite mensal de gasto atingido em 04/10 (reinicia 19:10 para a sessão).

## Também publicado (04/10, noite, deploy 6f9bfab5)
Protegido unificado (ex-proteção mágica; vários Protegidos cobrados juntos), efeitos secundários (dano no Conjurador, descartar, sacrificar Aliado, Grau ao cemitério), reanimar com pagamento (grátis/custo/custo+X, cartas da mão ao Arcano), gatilho cataclismaOponente, tirar da mão (descartar/banir; acaso/dono escolhe/olhando, com sigilo). Decisões 30–35. Testes tcg 181, online 13.

## Também publicado (04/10, madrugada, deploy 9934d90b)
Conjurador atacante sem revide (dec. 36), regra das cores no deckbuilding (dec. 37; itens com a mesma regra; baralhos das simulações e prontos migrados), equipamento do bot aleatório com peso pelo Valor ou escolhido (dec. 38), bot usa Extras. Testes tcg 193, online 14.
Simulação depois da migração: Ricardo 68,7, Diástaros 68,6, Lyandra 62,8, Agnelo 59,2, Roh 46,3, Osmar 46,3, Carmal 44,8, Mitonho 41,1, **Ana 12,2** — o autor vai editar as cartas da Ana ele mesmo no Editor; equipe de balanceamento ainda não chamada.

## Feito (05/10, madrugada) — publicado em 05/10 com as 46 artes aprovadas do GPT
- 39 cartas provisórias (tcg/src/cartas-provisorias.ts, provisoria: true, arte aprovada fora da expansão, identidade.json) repõem as funções que saíram dos baralhos pela regra das cores; baralhos da simulação reescritos (sim/conjuradores-decks.ts); decisão 39. Simulação: Ricardo 69,7 · Diástaros 62,0 · Lyandra 60,6 · Agnelo 51,4 · Osmar 48,9 · Carmal 45,8 · Roh 38,0 · Ana 36,9 · Mitonho 36,7.
- Editor: aba Provisórias, filtro por Valor (algum/exatamente), agrupar por Valor, atalho por Conjurador.
- FEITO: +7 provisórias (5 Místicas, 2 Primitivum: cura da Lyandra e defensor do Carmal) = 46 cartas. Pedido ao GPT v2 em cartas/pedidos/cartas-provisorias-v1.claude.json com 46 artes e bloco `alinhamento`; o GPT deve confirmar em cartas/entregas/cartas-provisorias-v1.visual.json. O Codex já começou (arte/cartas-rapidas/prov-*). Testes NÃO rodados desde as 7 novas (ordem do autor): rodar uma vez antes de publicar (o limite do teste de provisórias subiu para 50).
- Próximo: quando a entrega do GPT chegar aprovada, integrar (integrador-visual), rodar testes uma vez, publicar e comitar.
- Ordens do autor: curas ele ajusta; prontos do site com mais provisórias ok; **parar testes/simulações nesta etapa — só design e efeitos**; **publicar só depois que o GPT entregar as artes aprovadas**; alinhar com o GPT quantas e quais artes.

## Próximos passos
1. (feito 04/10) Iconografia v19 ligada; Editor auditado: texto congelado, clique indo para "Gerar texto" e aviso de edição não salva corrigidos.
2. Conferir na tela: Editor (Protegido, efeito secundário, reanimar, tirar da mão, Conjuradores) e mesa (sacrifício, reanimar com pagamento, olhar mão, ◈ Protessência, ⏳).
3. Efeitos de magia v17: conferir pendências da integração.

## Esperando o autor

- Novos comentários marcados Yes / novos ENVIAR.
- Liberação da expansão "A Mestra dos Dragões" (até lá, não tocar).
- Push de mudanças que não sejam entregas do Codex: perguntar.

## Regras que valem
CLAUDE.md do projeto, memória e `.claude/EQUIPE.md`. Sem penalidade em mecânica, sem cota de tipo por baralho, sem raridade automática, sem carta neutra, alvo nunca automático, imagem só com OK do GPT.

## Como verificar
Agente `verificador`, ou: `cd Arcanoclisma/tcg && npm test`; `cd Arcanoclisma/online && node construir.ts && npm test && node test/online-local.mjs`. Publicar: `node construir.ts && node node_modules/wrangler/bin/wrangler.js deploy` (em `Arcanoclisma/online`; o npx é bloqueado no PowerShell).
