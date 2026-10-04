# Equipe do Claude — Arcanoclisma online (04/10/2026)

Objetivo: menos crédito, tarefas eficientes, fácil de funcionar. A conversa principal decide e fala com o autor; os subagentes fazem trabalho autocontido e devolvem relatório curto.

| Agente | Modelo | Quando usar | Escreve? |
|---|---|---|---|
| `triagem-feedback` | haiku | começo da rodada; "comentei/enviei algo" | não |
| `motor-regras` | opus médio | mecânica simples ou correção pontual (motor + teste + Editor + livro) | sim |
| `motor-regras-alto` | opus alto | mecânica complexa: pilha, combate, construtor de efeitos, editor de Conjuradores | sim |
| `integrador-visual` | sonnet | entrega nova do GPT/Codex (artes, templates, ícones, efeitos) | sim |
| `verificador` | haiku | depois de mudar código, antes de publicar | não |
| DeepSeek (`ds.mjs`, fora do Claude) | — | ler arquivo grande, achar código, conferir tela, revisar | só em cópia |
| equipe de balanceamento (`.claude/agentes-desligados/`) | — | só com "preciso da minha equipe de balanceamento" | — |

Visual (artes, templates, animações, agentes Carlos/Enzo/Vuco etc.) é do Codex: não criamos agentes para isso.

## Regras de economia
- Tarefa pequena (uma edição, < ~5 mil tokens de leitura): a conversa principal faz direto. Subagente começa do zero e relê contexto.
- Leitura grande só para localizar algo: DeepSeek ou Grep, nunca Read de arquivo inteiro.
- `motor-regras` e `integrador-visual` podem rodar juntos só se mexerem em arquivos diferentes (cuidado com `mesa.js`, `carta.js`, `editor.js`: um de cada vez).
- Fluxo típico: triagem → (motor-regras | integrador-visual | principal) → verificador → principal publica.
- Publicar (`node construir.ts` + deploy), commit e push: só a conversa principal. O autor autorizou push automático das entregas concluídas do Codex integradas; o resto pergunta antes. Expansão "A Mestra dos Dragões": bloqueada.

## Continuidade
- Skill `pao-duro`: conversa ≥ 40% de contexto → atualizar `Arcanoclisma/tcg/agente/PASSAGEM.md` (curta: só estado atual) e abrir conversa nova com "continue pela passagem".
- Histórico detalhado fica em `Arcanoclisma/tcg/agente/passagens/`; não reler salvo necessidade.
- Ligar/desligar agente: mover o `.md` entre `.claude/agents/` e `.claude/agentes-desligados/` e abrir conversa nova.
