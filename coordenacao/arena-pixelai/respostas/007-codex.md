# Projéteis, eventos e terrenos — pacote final

Assets em `entregas/eventos-cenarios/`, com alfa/hashes/dimensões em `assets.visual.json`. Claude confirmou integração das imagens iniciais e implementação de projéteis, moedas e seleção aleatória de cenário em `005-006-claude.md` e `004-007-sincronia-claude.md`. As animações adicionais e os pisos Neve/Vulcão abaixo são uma entrega posterior.

| Skill consumidora | Imagem de cabeça/raio em projeteis/ | Uso |
|---|---|---|
| dragao-dourado | dragao-dourado.png | Dragão dourado voando, origem → alvo. |
| foice-verde | foice-verde.png | Foice giratória. |
| tiro-de-pistola | bala-pistola.png | Bala; renomear ao skillId no destino. |
| canhao-de-mao | bala-canhao.png | Bomba em arco + área resolvida; renomear ao skillId. |
| ilusao-confusa | ilusao-confusa.png | Orbe violeta. |
| tiro-carregado | flecha.png | Flecha; renomear ao skillId. |
| flecha-abissal | flecha-abissal.png | Flecha espectral na linha resolvida. |
| baforada | baforada.png | Raio/sopro azul, 128×32, origem → extremidade atingida. |
| raio-arcano | raio-arcano.png | Raio violeta, 128×32, origem → alvo. |

As imagens alongadas apontam para a **direita na tela** na referência. Orientar usando a projeção do vetor emissor→alvo no plano da câmera, com pivô central conforme o gancho atual do Claude. Não selecionar estilo pelo número de pixels da imagem; usar o ID da skill. Emitir também quando alcance efetivo for alterado por altura/itens. Em ataques de área/linha, destacar as casas realmente resolvidas e seus impactos; não inventar novos alvos pela aparência do raio.

**Seis animações novas/empacotadas:** `efeitos/moeda.png` (9 células de 32×32); `cura.png`, `impacto-area.png`, `invocacao.png` (5 células de 64×64 cada); `baforada.png`, `raio-arcano.png` (5 células de 128×128 cada, somente padding transparente do frame original 128×32). Os três anéis são overlays para a câmera; marcação de alcance/casas continua pela geometria da apresentação. Os dois raios animados servem para modular a arte entre emissor e alvo; não tratá-los como um anel circular de impacto. Fonte PNG por frame guardada por animação; nenhuma conversão GIF foi usada nos PNGs de jogo. Recomendações: moedas 6 FPS e efeitos 10 FPS; bursts podem terminar/fazer fade, loops apenas durante a duração do evento.

**Moedas:** usar a tira ao coletar, roubar, comprar e na chuva de moedas. Quantidade exibida deve vir do evento; um ícone não significa regra de uma moeda. **Cura, invocação e área:** usar anéis nas posições reais dos eventos; o círculo não substitui as células calculadas pelo núcleo. Os demais eventos têm vínculo proposto em `vinculos-eventos.json` e tarefa 010, usando também animações de personagem, ícones já entregues e efeitos procedurais existentes.

**Cenários compatíveis com os cinco temas atuais:**

| Tema | Fonte em terrenos/ | Observação |
|---|---|---|
| Pedreira | pedra.png | Pavimento frio de pedra. |
| Bosque | grama.png | Pavimento com musgo; não é grama solta. |
| Deserto | ruinas.png | Pavimento arenoso em ruínas. |
| Neve | neve.png | Novo piso branco/azulado, substituir cristal. |
| Vulcão | vulcao.png | Novo basalto escuro com rachaduras de brasa. |

`cristal.png` fica como **variante opcional de caverna**, sem mapear para Neve. Todas as seis texturas são 64×64 opacas. `*.repeticao.png` mostra repetição 2×2 para avaliação, não é textura de jogo. As bordas foram medidas; não há certificação automática de textura perfeitamente seamless. Repetir em escala coerente e manter zonas, altura, obstáculos e seleção legíveis sobre o piso. Point, sem compressão e mipmap.

Claude já relatou botão de cenário aleatório/nome, seed determinística sem consumo extra do RNG, paletas, iluminação e enfeites. Preservar a simetria, conectividade e spawns do MapGen. Não introduzir mudanças de regra para obter variedade visual. Aplicação dos pisos finais e efeitos adicionais fica com Claude; Codex não rodou a partida nem validou rede.
