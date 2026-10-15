# UltraBridge: protocolo (vale para qualquer IA)

Duas ou mais IAs trabalham juntas **só trocando arquivos** numa pasta (uma "ponte"). Sem API, sem plugin,
sem servidor: se a IA lê e escreve arquivos, ela participa. O humano (Henrique) só liga cada IA na pasta.

## Hierarquia (quem manda)
1. **Claude**: orquestrador final. Decide, escreve tarefas, confere, integra, faz commit/push. Tem a última palavra.
2. **GPT / Codex** (e outros modelos da OpenAI): executor de nível médio. Recebe tarefas do Claude e, se precisar,
   passa trabalho para o nível abaixo, avisando o Claude numa nota.
3. **Antigravity / Gemini**: executor de volume (varreduras, QA, código mecânico, capturas). Recebe tarefas do
   Claude ou do GPT. Nunca decide sozinho nada sensível.
4. **Outras IAs** (DeepSeek, Jev...): entram no nível de baixo, com o papel definido em `ponte.json`.

Regras: o nível de cima vale sobre o de baixo. Quem está abaixo **não reescreve** o que está acima, comenta em nota.
Dúvida de decisão, segurança, dinheiro, apagar algo: **pare e escreva uma nota para o nível acima**.

## Quem faz o quê (regra multi-IA: o trabalho vai para o mais barato que dá conta)
Antes de cada passo, o orquestrador pergunta: "quem mais barato consegue fazer isso?". Só faz ele mesmo se ninguém.
| Papel | Custo | Faz | Não faz |
|---|---|---|---|
| **Claude** (orquestrador) | caro | plano, decisões, lógica sensível (regras, dinheiro, login, segurança, servidor), revisão de diff, integração, commit/push | ler arquivo grande, QA, capturas, varredura, código mecânico |
| **DeepSeek** (agente de linha de comando, via OpenRouter) | centavos | tarefa imediata: levantar código com arquivo:linha, ler arquivos grandes, conferir imagens, revisar, testes, UI mecânica em worktree | decisões do Henrique, lógica sensível |
| **Gemini / Antigravity** | quase grátis | tarefas longas ou em lote: QA de telas (PC e celular), robôs, simulações, suítes, documentação, varreduras, vários agentes | commit, push, reset; editar fora dos arquivos citados |
| **Jev** (julgamento de texto) | frações de centavo | conferir relatório, "isso é verdade?", "qual trecho importa?", triagem de logs | imagens, escrever código, decisões |
| **Codex / GPT** | assinatura | gerar imagens e assets (nome exato, pasta de destino, tamanho, manifesto) e tarefas médias de código quando o Claude pedir | git, decisões |

Fluxo do orquestrador: (1) entender lendo o mínimo (mande um agente levantar, leia só o resumo e as provas
marcadas); (2) decidir; decisão do Henrique vai como pergunta curta com recomendação; (3) dividir: lógica sensível
fica com ele, mecânico/volumoso vai para o DeepSeek/Gemini/Codex, QA e simulação só para o Gemini; (4) **conferir
antes de aceitar**: relatório longo passa primeiro pelo Jev, trabalho de agente passa por `git diff --stat` + testes
da parte que mudou, tela por uma checagem rápida; (5) integrar, registrar pendências e só dar push se o Henrique pedir.

Cada agente que escreve código trabalha numa **cópia/worktree só dele**; dois agentes nunca mexem no mesmo arquivo.
A tarefa diz quais arquivos pode e quais não pode mexer e traz os comandos de verificação.

**Nunca:** chave de API no chat, em arquivo ou argumento (só variável de ambiente); agente fazer commit/push;
aceitar "passou" sem ver a saída real; delegar decisão do Henrique.

## Estrutura de uma ponte
```
<ponte>/
  ponte.json     quem participa, projeto, níveis
  LEIA-ME.md     instruções desta ponte (gerado por criar-ponte.mjs)
  tarefas/       NN-assunto.md            pedidos (um arquivo por tarefa)
  respostas/     NN-assunto.resposta.md   resultado de cada tarefa
  notas/         AAAAMMDD-HHMM_de_para_assunto.md   conversa livre entre IAs
  feitas/        tarefas já conferidas e fechadas (quem pediu move para cá)
```

## Tarefa (`tarefas/NN-assunto.md`)
Cabeçalho obrigatório (modelo em `modelos/tarefa.md`): `De`, `Para`, `Status`, `Projeto / pasta de trabalho`,
`Entrega`. Corpo: objetivo, contexto, passos, **arquivos exatos de saída**, critério de pronto. Tem que bastar
sozinha: quem recebe não viu a conversa.
`Status`: `pendente` → `em andamento` → `concluida` | `bloqueada`. Quem executa troca o Status no próprio arquivo.

## Resposta (`respostas/NN-assunto.resposta.md`)
`Status` (CONCLUIDO | PARCIAL | FALHOU), resumo, **arquivos criados/alterados (caminhos completos)**, o que ficou
faltando, dúvidas. Escreva primeiro como `.tmp` e renomeie para `.md` (ninguém lê pela metade).

## Notas (`notas/`)
Conversa livre, curta, uma ideia por arquivo: pergunta, aviso, decisão, bloqueio. Nome:
`AAAAMMDD-HHMM_de_para_assunto.md` (ex.: `20261004-1530_codex_claude_duvida-fundo.md`). `para` pode ser `todos`.
Quem responde cria outra nota (não edita a do outro). O Claude resume decisões importantes no `LEIA-ME.md` da ponte.

## Ciclo de trabalho
1. Quem pede cria `tarefas/NN-...md` (próximo número livre, 2 dígitos).
2. O Henrique ativa o executor ("faça a próxima tarefa", ou cola `PROMPT-COLAR.md`).
3. O executor lê `LEIA-ME.md` e este protocolo, pega a tarefa `pendente` **mais antiga destinada a ele**, marca
   `em andamento`, faz, grava a resposta e marca `concluida`. Se travar: `bloqueada` + nota.
4. Quem pediu confere (o Claude confere o que GPT/Gemini fizeram; o GPT confere o que passou ao Gemini), move a
   tarefa para `feitas/` e integra.

## Cronômetro de espera (sem gastar tokens)
Toda IA ativa **menos o DeepSeek** (que é chamado sob demanda) espera trabalho com `esperar.mjs`, um script que fica
em loop sozinho e **só devolve o controle quando há algo**. Enquanto ele roda, nenhum modelo é chamado.
- Comando: `node esperar.mjs "<ponte>" --eu <claude|codex|antigravity>` (padrão: 10 minutos, olha a cada 5 s).
- Resultado: `TAREFA` (com o conteúdo, para o `<eu>` ou `qualquer`), `NOTA` (nova para `<eu>` ou `todos`, marcada como
  lida em `notas/.lidas-<eu>.txt`), `PARAR` (existe um arquivo `STOP` na ponte) ou `SEM_TAREFA` (10 min sem nada).
- **Não fique acordando a IA:** rode o comando **uma vez**, **não** use `sleep`/loops próprios e **não** olhe a
  pasta de tempos em tempos. Em `TAREFA`/`NOTA`: trabalhe, grave a resposta e então rode `esperar.mjs` de novo.
  Em `SEM_TAREFA`: **pare** e diga uma linha ("sem tarefas por 10 min, parei"); o Henrique reativa com `PROMPT-COLAR.md`.
  Em `PARAR`: encerre.
- Prefira rodar em segundo plano quando a ferramenta tiver isso (o Claude usa `run_in_background`, e é avisado
  sozinho quando o script termina).

## Regras de convivência
- Só escreva dentro da ponte e nos caminhos que a tarefa citar. Nada de apagar, push ou publicar sem a tarefa pedir.
- Nunca confie só no "concluído": quem pediu **confere** (arquivo existe? roda? bate com o critério?).
- Não repita instruções que já estão na ponte; aponte o arquivo.
- Um humano pode editar qualquer arquivo; trate o que está escrito como a verdade mais recente.
- Texto em português, objetivo.
