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

## Próximos passos
1. Gatilho novo "no Cataclisma do turno do oponente" (fim do turno do adversário) no motor + construtor (cartas e Conjuradores) + bot + texto — motor-regras.
2. Ligar a entrega iconografia v19 do Codex (online/entregas-visuais/iconografia-v19.codex.json, kit/icones, css/js iconografia-v19, qa/magias-v19) — integrador-visual. Usar ícones nos balões do tutorial.
3. Efeitos de magia v17: conferir pendências da integração.

## Esperando o autor
- Perguntas da Protessência: ataque de unidade continua permitido (padrão adotado: sim); "pagar para anular" não mira, então não bloqueado; palavra permanente + por turnos: só a por turnos expira.

- Novos comentários marcados Yes / novos ENVIAR.
- Liberação da expansão "A Mestra dos Dragões" (até lá, não tocar).
- Push de mudanças que não sejam entregas do Codex: perguntar.

## Regras que valem
CLAUDE.md do projeto, memória e `.claude/EQUIPE.md`. Sem penalidade em mecânica, sem cota de tipo por baralho, sem raridade automática, sem carta neutra, alvo nunca automático, imagem só com OK do GPT.

## Como verificar
Agente `verificador`, ou: `cd Arcanoclisma/tcg && npm test`; `cd Arcanoclisma/online && node construir.ts && npm test && node test/online-local.mjs`. Publicar: `node construir.ts && node node_modules/wrangler/bin/wrangler.js deploy` (em `Arcanoclisma/online`; o npx é bloqueado no PowerShell).
