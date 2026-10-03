# Passagem (Pão Duro) — 03/10/2026, ~15h30

A conversa anterior chegou a 47% do contexto e foi encerrada para economizar crédito. Transcript: `C:\Users\Anuah\.claude\projects\C--Users-Anuah-OneDrive--rea-de-Trabalho-Rpg\a7908bef-a28d-4ebd-9919-cde080c9aa36.jsonl`.

## Objetivos do autor
- TCG Arcanoclisma (`Arcanoclisma/tcg`). O autor testa no app (`app/INICIAR.bat`, http://localhost:8765), comenta e envia pelo botão ENVIAR (arquivos em `envios/`). O Claude ajusta o jogo com base nesses comentários.
- Editor de cartas e mecânicas em `app/editor.html`. As edições ficam em `data/edicoes.json` e valem nas variantes PADRAO e CONJ.
- Balancear os Conjuradores é tarefa da equipe ("preciso da minha equipe de balanceamento"), e ela só trabalha quando o autor pedir.

## Feito nesta conversa (envio de 03/10)
- **Regras**, gravadas como edições no editor:
  - repor a mão até a mão inicial no Início (o Final não compra mais);
  - o Conjurador levanta no Início;
  - o Conjurador só ataca com arma, e o dano básico dos 9 Conjuradores ficou 0.
- **Itens** (fora do baralho, um por espaço): Canal, Armadura e Extra, em `src/itens.ts`. Cada Conjurador está equipado com Cajado de aprendiz e Gibão de couro. A Armadura evita dano até a sua Defesa quando o Conjurador bloqueia e se desgasta até quebrar.
- **Carta**: `base-comprar-c1` virou "Olhar o topo (custo 1, repentina)": olha 3 cartas, põe 1 na mão e manda as outras para o fundo do baralho. Usa o efeito `olharTopo` e a pendência `escolherDoTopo`.
- **App**:
  - acontecimentos agrupados por turno e etapa, escritos com "você" e "oponente", e o registro mais completo;
  - a janela de pagamento mostra quanto falta e envia ao Arcano só as cartas necessárias;
  - mostra os itens e o ataque do Conjurador, e a escolha de "olhar o topo";
  - `?teste` na URL usa outra chave de armazenamento, para testar sem mexer nas partidas do autor.
- **Editor**: itens (com espaço por Conjurador na aba Baralhos), o efeito olharTopo e as 3 regras novas.
- **Livro de regras** 0.10: seções 5, 8, 8.3, 9 e 9.3 (itens) e a decisão 15.
- **Investigado sem bug**: as jogadas "antes da fase principal" eram só a ordem de exibição do registro, que já foi corrigida. O Grau já podia ser usado no mesmo turno em que entra.
- **Simulação** (CONJ, 40 partidas por confronto, regras novas):

  | Conjurador | Vitória |
  |---|---|
  | Ricardo | 78,4% |
  | Osmar | 60,9% |
  | Lyandra | 60,8% |
  | Carmal | 59,5% |
  | Agnelo | 49,4% |
  | Mitonho | 47,2% |
  | Diástaros | 38,7% |
  | Roh | 30,8% |
  | Ana | 24,2% |

  Quem começa vence 40,6%; as partidas duram 23,3 turnos.
- **Codex**:
  - pedido de templates em `Arcanoclisma/cartas/pedidos/templates-mecanica.claude.json` (o que cada template precisa ter);
  - agente `.agents/skills/produtor-de-cartas/SKILL.md`, registrado em `AGENTES-VISUAIS.json`;
  - modelo de lote em `cartas/modelos/lote.claude.exemplo.json`.
- **Pão Duro**: `.claude/skills/pao-duro/SKILL.md`, com nota na memória.

## Atualização (respostas do autor, mesma conversa)
- Armadura só protege ao bloquear: confirmado.
- Conjurador que bloqueia **não** devolve o dano da arma (só se a arma tiver efeito assim).
- Itens vêm de um **side deck**; cada jogador escolhe antes da partida um Canal, uma Armadura e um Extra e não troca durante o jogo. O **Extra é um consumível de suporte** (sem Ataque; ativação de uso único): Poção de cura e Elixir de foco. O bot ainda NÃO usa o Extra. A escolha do side deck já está no app (tela inicial).
- "Levanta também" vale só para o Conjurador: confirmado.
- Push feito: commit e0b7614 em origin/main (tudo do TCG, agentes e pedidos; ficaram de fora as artes e a pasta Arcanoclisma/arena, que são do Codex). Depois: nova etapa em outra conversa.
- Pendências do Codex: pedido `templates-mecanica` precisa trocar "Extra com Ataque" por "Extra consumível sem Ataque".

## Esperando o autor (itens 1 a 3 acima já respondidos; ficam o 5 e novos envios)
1. Itens:
   - a armadura só protege quando o Conjurador bloqueia;
   - o Conjurador que bloqueia devolve o Ataque das armas;
   - itens não são trocados durante a partida nem mirados por efeitos;
   - os itens de teste são iguais para todos (Cajado e Gibão).
2. "Levanta também" foi aplicado só ao Conjurador. Aliados que bloqueiam continuam sem atacar no turno seguinte (decisão 1 do livro).
3. Novas partidas no app e um novo ENVIAR.
4. O Codex responder ao pedido de templates (`cartas/entregas/templates-mecanica.visual.json`).
5. Definir um primeiro lote de cartas para o Produtor de Cartas. O Claude descreve os personagens e o que a arte precisa ter.

## Regras que valem
- Sem push e sem commit sem permissão (nada foi comitado).
- O Codex cuida das artes.
- Não pôr penalidades em mecânicas nem cotas de tipo de carta por baralho.

## Como verificar
- `cd Arcanoclisma/tcg && npm test`: 76 testes passam.
- `node sim/conjuradores.ts 40`: a simulação.
- O servidor do app recarrega o motor só ao reiniciar (`node app/servidor.ts` ou `INICIAR.bat`).

---

# Passagem — 03/10/2026, noite: site online (Cloudflare)

Pedido do autor: primeira versão jogável na internet (Cloudflare), contra o bot e contra pessoas, construtor com a regra das cores, baralhos prontos, tutorial completo contra o bot, login por e-mail, comentários/notas/dúvidas com aprovação do autor, aba de ideias, arte do site e das cartas pedida ao GPT.

## Feito (tudo em `Arcanoclisma/online/`, ver `online/README.md`)
- Worker + D1 + Durable Objects (sala online autoritativa com sigilo; saguão com partida rápida, salas abertas e código privado).
- Site: início (com ideias), Jogar (bot / pessoas), tutorial (4 lições + partida com dicas, todas testadas no navegador), Baralhos (prontos, meus, comunidade, coleção, construtor), Comentários, Ideias, conta.
- Mesa no layout pedido pelo autor (tabuleiro limpo; vida/ataque sobre as imagens; esfera de Arcano; Grau virado; mão fora da mesa; fases, decisões e pilha com carta + efeito na direita).
- Coleção nova `tcg/src/cartas-colecao.ts`: 32 cartas e 12 itens cobrindo as mecânicas que não tinham carta (custoValores, Repentina em Aliado, pagar para anular, arcanoOponente, banir, reanimar, anular do cemitério, desconto, olhar o topo maior, Místicas com ativação/gatilho...). Fora dos baralhos prontos: simulações não mudam.
- Identidade provisória (`online/dados/identidade.json`): cada carta mecânica usa uma arte aprovada e o título dela como nome; cenário do Valor atrás da arte.
- Pedidos ao GPT/Codex: `cartas/pedidos/lote-site-v1.claude.json` (9 Conjuradores + 142 cartas, todas com cenário de fundo obrigatório) e `online/pedidos-visuais/site.claude.json` (logo, telas, cenários dos Valores, peças de interface, animações).
- Subagentes: equipe de balanceamento desligada (`.claude/agentes-desligados/`, tarefa agendada pausada); Enzo e Copião desligados em `AGENTES-VISUAIS.json`; Produtor de Cartas ativo por causa do lote.
- Testes: `online/npm test` (4) e `tcg/npm test` (77) passam; `node test/online-local.mjs` jogou uma partida online inteira entre dois bots (0 erros, sigilo ok).

## Esperando o autor
1. Login no Cloudflare (`npx wrangler login`) para publicar; passos em `online/README.md`.
2. Escolher a `AUTOR_CHAVE` de produção e, se quiser, o ID do Google.
3. Push (nada foi comitado).
4. Revisar nomes/artes provisórios, limite de 4 cópias e as cartas novas.

## Pedidos do autor de 03/10 (depois do site) — FEITOS na conversa seguinte (03/10, fim da tarde)
1. **Armadura por escolha** (`tcg/src/motor.ts`, danoDeCombate e bloquear): comando `bloquear` com `usarArmadura?` (sem o campo = usa, para o bot); `habUsos[j].armadura` limita a **um ataque protegido por turno** (zera no Início). Decisão 16 e seção 9.3 do livro. Teste novo em `test/envio-0310.test.ts`. No site, bloquear com o Conjurador pergunta "Usar a Armadura?" (só se há armadura e ela ainda não protegeu no turno). **Não conferido no navegador** (precisa o bot atacar o Conjurador).
2. **Itens fora da mesa** (`online/public/js/mesa.js`): clicar no Conjurador abre Atacar, Usar habilidade, Usar <Extra> (consome) e Ver equipamento (janela com Canal, Armadura com desgaste e Extra com botão Usar); o do oponente também mostra o equipamento.
3. **Mesa na tela**: `ladoHTML` gera o markup do v10 do Codex (`.fileira-fundo` com esfera, Conjurador no eixo central, Grau e montes; Aliados atrás). `css/mesa-ajuste.css` (depois do v10): a mesa tem a altura da tela, a mão fica sempre visível e o tabuleiro **rola** dentro do palco (direção do autor no `coordenacao/site-online/INTERFACE-MESA.md`: não reduzir as cartas); também corrige o `.campo` do v10 sem escopo que estragava os formulários.
4. **Até 3 cópias**: `online/construir.ts` MAX_COPIAS = 3; o excesso dos baralhos prontos passa para outras cartas do mesmo baralho (mesmo tipo, custo próximo): 15 trocas, listadas no build. Os baralhos das simulações (`tcg/sim`) **não** mudaram.
5. **Editor do autor no site** (`#/editor`, `public/js/editor.js`; `src/edicoes.ts` também vira `public/jogo/edicoes.js`; `src/edicoes-db.ts`; tabela `edicoes` no `schema.sql`; rotas `GET/POST /api/edicoes`, `GET /api/edicoes?id=N`, `GET /api/edicoes/historico`). Edita nome, texto (ou gera da mecânica), arte (qualquer uma das 435; o build agora exporta todas e `jogo/artes.json`), números, Valores, velocidade, palavras-chave, subtipos e mecânica em JSON; Conjuradores: nome, habilidade, arte, números, Valores, comprometimento, poder (JSON). Rascunho no navegador (vale na página, dá para testar contra o bot), salvar com nota, baixar/carregar JSON, abrir versão antiga. A sala online guarda `edicoes`/`edVersao` no início e o cliente carrega a mesma versão. Testado no navegador (salvar, arte, JSON, texto automático) e na partida online local (usou a versão 1); depois foi salva a versão 2 vazia para voltar ao original. `test/edicoes.test.ts`.
6. **Prompts v2** (`online/pedidos-visuais/gerar-lote-cartas.ts`): campo `variacao` por carta (hora do dia com peso para o dia, ~7% à noite; pose, perspectiva, enquadramento, olhar, acabamento, relação com o evento); variantes de texto para dano, cura, comprar, destruir e anular; Conjuradores sem hora fixa. `cartas/pedidos/lote-site-v1.claude.json` versão 2 e `site.claude.json` versão 2 (horaDoDia nos cenários).
7. **Cartas mais claras** (`public/js/carta.js` cenaDe + `site.css`): cenário provisório com luz da hora do dia (mistura "screen"), enquadramento e Valor variados por carta.
8. Resposta ao Codex: `coordenacao/site-online/resposta.claude.json` (integração do v10, pedido de escopo para `.campo`).

## Esperando o autor
1. Respondido: a Mina Funda também tem limite de 3 (39 trocas nos baralhos prontos, quase todas por outras Estruturas); os baralhos das simulações não mudam.
2. PUBLICADO (03/10): https://arcanoclisma.arcanoclisma-online.workers.dev (conta Cloudflare do login: lucasmisterioso@gmail.com). D1 de produção c56e5110-e965-4331-9c45-6ee9a26449a1 em wrangler.jsonc; tabelas criadas. Login do wrangler: node node_modules/wrangler/bin/wrangler.js (o npx é bloqueado pelo PowerShell). Falta o autor definir AUTOR_CHAVE (secret put). O autor colou um token da Cloudflare na conversa: foi recomendado revogar; não foi usado.
3. Login no Cloudflare, `AUTOR_CHAVE` de produção, push (nada foi comitado). Em produção, rodar `npm run db:remoto` de novo para criar a tabela `edicoes`.
4. Simulação não foi refeita com a regra nova da Armadura.

Testes: `tcg npm test` 78 passam; `online npm test` 6 passam; `node test/online-local.mjs` 0 erros.
Servidor local: a configuração `arcanoclisma-online` (porta 8787) pode estar rodando por outra conversa; os arquivos de `public/` e `src/` recarregam sozinhos.
