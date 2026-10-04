# Passagem — estado atual (04/10/2026, 16h53)

Versão enxuta. Histórico completo (o que foi feito em 03–04/10, decisões 15–26, detalhes de mesa, tutorial, editor, itens v2/v3, construtor de efeitos): `agente/passagens/PASSAGEM-2026-10-04_16-53.md`. Ler só se a tarefa exigir.

## Objetivos do autor
- Site online do TCG Arcanoclisma: https://arcanoclisma.arcanoclisma-online.workers.dev (código em `Arcanoclisma/online/`, motor em `Arcanoclisma/tcg/`). Jogar contra bot e pessoas, tutorial, construtor, Editor do autor, Lore, comentários/ideias.
- O Claude: programação, regras, dados, backend, rede e integração. O GPT/Codex: arte, templates, ícones, animações e a gestão dos agentes visuais.
- Regras de atualização: seguir as orientações novas do GPT; polir textos do autor sem mudar regra (mostrar antes/depois); todo efeito automatizado no motor com teste.

## Equipe de subagentes (definida em 04/10)
Ver `.claude/EQUIPE.md`: `triagem-feedback` (haiku), `motor-regras` (sonnet), `integrador-visual` (sonnet), `verificador` (haiku); DeepSeek para leitura grande e tela; equipe de balanceamento desligada até o autor pedir.

## Estado
- Tudo comitado até `c86b85c` (git limpo em 04/10 16h50), incluindo Etéreo por papel, alvo nunca automático e templates v18 do GPT integrados.
- Testes: tcg 110, online 7, partida online local 0 erros (última medição).

## Próximos passos
1. Rodar `triagem-feedback` (diagnóstico Yes + envios + entregas do Codex desde 04/10 tarde).
2. Integrar o que estiver pendente do Codex: efeitos de magia v17 (`efeitos/entregas/magias-v17.visual.json`, conferir a `.integracao.claude.json` existente), ícones (`kit/icones/manifesto.json` com `requisito-grau`/`requisito-arcano`), template que ressalta o Arcano.
3. Usar os ícones nos balões do tutorial quando chegarem.
4. Simulação não foi refeita com as regras novas de combate/itens v3 (só relatar; balancear é com a equipe).

## Esperando o autor
- Novos comentários marcados Yes / novos ENVIAR.
- Liberação da expansão "A Mestra dos Dragões" (até lá, não tocar).
- Push de mudanças que não sejam entregas do Codex: perguntar.

## Regras que valem
CLAUDE.md do projeto, memória e `.claude/EQUIPE.md`. Sem penalidade em mecânica, sem cota de tipo por baralho, sem raridade automática, sem carta neutra, alvo nunca automático, imagem só com OK do GPT.

## Como verificar
Agente `verificador`, ou: `cd Arcanoclisma/tcg && npm test`; `cd Arcanoclisma/online && node construir.ts && npm test && node test/online-local.mjs`. Publicar: `node construir.ts && node node_modules/wrangler/bin/wrangler.js deploy` (em `Arcanoclisma/online`; o npx é bloqueado no PowerShell).
