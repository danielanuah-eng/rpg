# 013 — Dados em sprite e leitura do manual do Krosmaster (pedido do autor, 10/10)
**De:** usuario (via claude)
**Para:** codex
**Status:** aberta, PRIORIDADE 2 (depois da 012)
**Responsável:** codex (arte dos dados e leitura do manual); claude (integração, regras e decisões)

## Parte A — Dados rolando no meio do tabuleiro
Quando alguém ataca ou tenta sair de uma casa travada, os dados rolam no meio da área do tabuleiro e param na face sorteada.
**O código já está pronto** (`ArenaHud.RollDice`, chamado pelos eventos `ataque` e `trava`). Por enquanto o dado é um quadrado colorido com o nome da face; ao chegar a sua arte, ela entra sozinha.

Dado de 6 faces, como no Krosmaster, mas **sem crítico**: a face do crítico virou Cataclisma.

| Arquivo | Face | Efeito no jogo |
|---|---|---|
| `cataclisma.png` | Cataclisma | Só no dado de ataque: dispara um evento aleatório no mapa |
| `escudo.png` | Escudo | Defesa: reduz 1 de dano |
| `trava.png` | Trava | Prende quem tenta sair de perto |
| `esquiva.png` | Esquiva | Escapa da trava |
| `coringa-esquiva-trava.png` | Esquiva ou Trava | Vale como Esquiva ou como Trava |
| `coringa.png` | Coringa | Vale qualquer face, menos Cataclisma (Escudo, Trava ou Esquiva) |

Entrega em `ArenaTaticaUnity/Assets/ArenaTatica/Resources/Sprites/Dados/`:
- **As 6 faces** acima: PNG quadrado de 128×128, fundo transparente, com o dado visto de frente e o símbolo legível a 64 px.
- **`rolagem.png`, opcional mas desejado:** tira horizontal de quadros quadrados de 128 px (8 a 12 quadros) com o dado girando. O importador fatia sozinho qualquer PNG mais largo que alto em `rolagem_0`, `rolagem_1`… O jogo toca a 14 quadros por segundo durante cerca de 0,7 s e depois mostra a face final.
- Estilo pixel combinando com os lutadores do PixelLab. Os símbolos devem ser **criados por nós**, inspirados na função; não copiar a arte do Krosmaster.
- Na resposta, informe os caminhos e os hashes. Se quiser outro tempo, tamanho ou quantidade de quadros, diga o valor exato que eu ajusto (`ArenaHud.DiceLand`).

## Parte B — Ler o manual do Krosmaster
Manual: `krosmaster_arena_2_0_krosmaster_arena_2_0_manual_ga_76609.pdf`, na raiz do Rpg. Tem 32 páginas e é só imagem, sem texto selecionável.
O autor pediu para verificar **itens, dados, armadilhas e designs**.
Escreva um resumo **com suas palavras** em `respostas/013-codex-manual.md`, citando a página de cada item. Não copie trechos longos.
1. **Dados:** faces, quem rola, quando, quantos dados por ataque, defesa e trava, e o que cada face faz. Aponte as diferenças em relação à nossa tabela acima.
2. **Itens e equipamentos** (recompensas, artefatos etc.): tipos, custo, como se compram ou ganham, duração e limites.
3. **Armadilhas e elementos do mapa:** arbustos, árvores, caixas, armadilhas, desafios e moedas. O que bloqueia visão ou movimento e como se ativam.
4. **Designs:** como o manual mostra peças, fichas, marcadores, tabuleiro e dados. Diga o que vale como referência visual para a nossa HUD e para os marcadores.

Eu decido as regras a partir do resumo. As mudanças de regra serão registradas em `ArenaTaticaUnity/Documentacao/DESIGN-KROSMASTER.md`. A arte não define regra.
