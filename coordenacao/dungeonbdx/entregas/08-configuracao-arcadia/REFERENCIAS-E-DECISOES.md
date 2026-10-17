# Configuração Arcadia — fontes e decisões

Direção do autor: aproximar regras, composição do tabuleiro e fluxo de Arcadia Quest, preservando os limites de balanceamento DungeonBDX. Esta entrega substitui a expedição de AP/recargas como entrada padrão por um modo independente. Os saves antigos continuam no modo legado.

## Material efetivamente conferido

- Manual Inferno fornecido em KrosArcano: páginas 13–24 e 26, com movimento, ocupação, ataque, descanso, Guarda, Vingança, Extermínio, entrada de monstros e vitória por missões.
- Um Portal Turbulento: história/preparação Base, páginas 1–2, e variante Inferno, páginas 3–4. Diagramas conferidos visualmente: nove tiles em 3×3, cada tile com nove espaços, salas intercaladas por ruas, portas, portais, marcadores e áreas iniciais com duas casas.
- O Dia de Ação de Tacos, página 2: distribuição alternativa de salas/ruas, início das guildas, portais, exploração e monstros. O desafio dos emblemas desta entrega é autoral, não uma transcrição desse cenário.
- Fonte de Cura: regra ilustrada de sala secreta, exploração e recuperação. Não implementada como regra universal.
- Folhas de campanha e inventário anexadas pelo autor: caminhos ramificados, resultados por cenário, títulos/recompensas e três heróis por guilda com quatro posições de equipamento. Servem de direção para a campanha persistente, ainda incompleta.
- Catálogo anterior: `../../referencias/arcadia-quest/README.md`, incluindo planilha e inventário de duplicatas. Os originais permanecem em KrosArcano.

**Limites dos arquivos:** o PDF local Pets salta da página impressa 13 para a 32; não contém as páginas intermediárias de aventuras. O arquivo local chamado Riders Campaign tem apenas duas folhas de campanha/inventário. Esses arquivos não foram tratados como livros completos de aventuras. Os três cenários DungeonBDX são adaptações; as topologias e aventuras comerciais não foram integralmente reproduzidas.

Complemento oficial: [manual Arcadia Quest](https://www.cmon.com/wp-content/uploads/2023/06/Rulebook_ArcadiaQuest.pdf) e [FAQ oficial](https://www.cmon.com/wp-content/uploads/2023/06/AQ-FAQ-v1.pdf). A FAQ esclarece que uma linha de visão que apenas tangencia uma quina bloqueada pode passar, em qualquer ângulo; duas barreiras que fecham a passagem continuam bloqueando.

## Regras presentes no novo modo

| Área | Comportamento |
|---|---|
| Guildas | 2–4 guildas locais, três heróis diferentes por guilda, cinco cartas iniciais distribuídas |
| Espaços | Até duas figuras comuns; Colosso/figuras enormes ocupam dois lugares; não se encerra movimento em casa cheia |
| Movimento | Três pontos ortogonais; portas e portais usam movimento; dois inimigos bloqueiam passagem e visão; aliados podem permitir travessia, não parada |
| Ativação | Um herói por turno ou descanso da guilda; um ataque; movimento não pode ser dividido por um ataque, inclusive depois de abrir uma porta gratuitamente |
| Ataque | Carta fica exausta; corpo a corpo no mesmo espaço ou adjacente aberto; distância sem limite numérico, condicionada à linha de visão |
| Monstros | Guarda antes de sair de Perto ou atacar outro alvo; Vingança após ataque, inclusive ataque que erra; rival à direita controla a reação, com opção automática explícita |
| Morte | Vingança mortal precede a remoção, salvo Extermínio; recompensa e fila contabilizadas uma vez; fichas carregadas caem no espaço |
| Descanso | Restaura cartas; redistribui equipamento; permite escolher ressurreição na origem/Perto de aliado que já estava vivo; não cura automaticamente heróis vivos |
| Entrada | Fila de cinco ou ausência de monstros; dois símbolos de ataque comparados aos marcadores; espaço ocupado ou par ausente remove o monstro da partida |
| Vitória | Duas missões em dois jogadores, três em três/quatro; ao menos uma missão PvE; não exige eliminar toda a guilda adversária |
| Heróis | 31 habilidades naturais distintas, com condições próprias; equipamentos transferíveis continuam reutilizáveis entre heróis |

## Exceções e conteúdo autoral

- Crítico linear, até três sucessos adicionais, sem cadeia de novas rolagens. Até seis dados; mitigação máxima de 60%; limites de dano/cura preservados.
- Heróis, atributos, armas, habilidades, monstros, mapas, narrativa e distribuição dos pares de entrada são adaptações DungeonBDX. Não são o catálogo oficial AQ nem um balanceamento competitivo já concluído.
- Portais em Ruínas: investigação de pistas e transporte de uma relíquia. Praça dos Quatro Emblemas: objetivos em alas e confronto com guardião central. Resgate nas Catacumbas: escolta e ritual, com casas de perigo sinalizadas.
- Pisos e elementos de cenário são gerados pelo código com materiais/texturas próprios; sprites de personagens e golpes vêm da Arena Tática existente. Os três personagens FBX nativos ainda carecem de rigs e animações particulares.
- Ainda faltam campanha encadeada, mercado entre aventuras, maldições da morte, catálogo completo de itens/monstros/eventos e os módulos de expansões. A sessão atual é local; salas online existentes não sincronizam esse combate.

## Persistência e compatibilidade

Novo journal determinístico em `DungeonBDX/Campaigns/Arcadia-v1`, separado do legado. Cada ação aceita é gravada antes do feedback visual. Escritor exclusivo, troca atômica, backup, digest, validação por replay e revisão monotônica. Recuperação invalida escritores que ainda detenham a revisão perdida; slots recusam dados de outro cenário. O digest detecta corrupção local; não constitui autorização de servidor multiplayer.

## Vista isométrica (16/10, Claude)

- Projeção 2D isométrica sobre o mesmo grid lógico `AqCell`; nada de segundo grid nem combate na UI.
- Figuras em pé: pés no pivô (0,153) da Arena, sem a margem transparente do quadro; escala pelo primeiro quadro `idle`.
- Paredes rebaixadas (0,26) e portas (0,46) para não esconder casas; só a vida sobre as figuras.
- HUD inferior compacto e coluna de câmera à direita: o tabuleiro ganhou cerca de 20% no PC.
- Menos informação no mapa: fichas só por cor (sem letras), distâncias discretas, passagens abertas sem moldura.
- Figuras a 74% da altura anterior para caber na casa; HUD do PC em três colunas (heróis | cartas | ações).
- Estilo das artes novas: RPG em pixel art 16 bits (combina com os sprites da Arena).
- Artes novas: NanoBanana (Gemini), pedidos em `entregas/nanobanana/` e `tarefas/08-gemini-nanobanana-artes.md`.

## Validação

Resultados e capturas em `README.md` e `validacao.json` desta pasta. Pendentes: QA de toque em aparelho físico, desempenho Android
e conferência visual quadro a quadro da Vingança mortal (a lógica de adiar a morte existe, sem captura dedicada).
