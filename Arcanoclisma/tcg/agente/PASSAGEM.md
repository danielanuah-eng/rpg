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
- Pedido do autor: git push de tudo feito até agora (feito, ver commit). Depois: nova etapa em outra conversa.
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
