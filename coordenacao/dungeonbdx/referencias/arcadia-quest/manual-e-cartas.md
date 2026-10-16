# Regras e diferenças para DungeonBDX

Referência principal: `KrosArcano/arcadia_quest_infer_livro_de_regras_arcadia_ques_109200.pdf` (40 páginas). Páginas citadas correspondem à numeração impressa/PDF, coincidente nos exemplos abaixo. Síntese original, sem substituir o manual.

## Estrutura do jogo de referência

- A guilda reúne três heróis. Cada um tem defesa em dados, vida, habilidade natural e inventário próprio de quatro cartas; cinco itens iniciais são distribuídos entre os três (p.8). Valores de defesa são dados, não percentuais de armadura.
- A ativação normal escolhe um herói, com movimento e ataque, ou a guilda descansa (seção de turno, p.14–15). Descanso e exaustão formam o ciclo de recursos. O modo Solo 1.2 altera a ordem e ativa os três heróis antes dos monstros.
- O tabuleiro modular tem nove espaços por tile, com até dois personagens por espaço. Decoração não bloqueia por si só. Paredes, portas fechadas e espaços bloqueados alteram a circulação e visão (p.13).
- “Perto” inclui o mesmo espaço e vizinhos ortogonais com borda aberta, excluindo diagonal e borda fechada (p.13). Não equivale simplesmente a um raio desenhado sobre qualquer parede.
- Ataque próximo exige proximidade. Ataque a distância depende de linha de visão e, na regra-base, não tem limite numérico universal de alcance. Visão segue centro a centro, bloqueios e tratamento próprio de quinas; não cruza área externa aos tiles (p.16).
- Usar ataque exausta sua carta; defesa adicional continua contando em cartas exaustas. Cartas de ampliação complementam um ataque e permanentes mantêm habilidades disponíveis (p.16,30).
- Críticos originais permitem dados adicionais em cadeia. Alvos múltiplos compartilham uma rolagem, com o alvo principal definindo modificadores pertinentes (p.18). A cadeia de dados não será transplantada: prevalece o crítico linear exigido pelo autor.
- Monstros podem reagir por guarda e vingança; extermínio evita a reação de um monstro que seria morto. Tipos, movimento, recompensa, vida, defesa e habilidades constam na carta e variam por nível (p.21–23). Isso difere de apenas alternar dois times simétricos.
- Campanha inclui missões, melhorias, recompensas, mortes e consequências. Folhas de campanha/inventário fornecidas mostram também títulos, registro de moedas e rotas entre cenários.

## Diferenças explícitas da base atual

| Aspecto | Referência AQ | DungeonBDX atual |
|---|---|---|
| Economia de ação | Ativação/movimento/ataque e exaustão | Três AP por turno; skills com cooldown/usos |
| Defesa | Dados e símbolos | Armadura/mitigação limitada a 60% |
| Crítico | Dados adicionais em cadeia | Incremento linear e rerrolagem limitada |
| Alcance à distância | Visão, sem limite universal | Alcance mínimo/máximo por skill + visão |
| Ocupação | Até dois personagens por espaço | Uma unidade viva por célula |
| Vitória | Missões próprias de cenário | Combate local por eliminação |
| Identidade | Habilidade natural e equipamento próprios | Perfis normalizados e loadouts por arquétipo |
| Persistência | Campanha, progresso e inventário | Journal/save contínuo da expedição local |

São adaptações/lacunas identificadas, não equivalências. O patch de câmera/alcance/animação não altera essas regras. Qualquer futura mudança de ciclo, ocupação, reação ou equipamento exige projeto e testes próprios, mantendo os caps obrigatórios.

## Expansões e modos

- **Solo 1.2:** arquivo `arcadia_quest_alguem_poderia_traduzir_o_modo_172937.zip`, membro `Solo Mode 1.2/AQ-Solo Mode-1.2.pdf`, 16 páginas, autores impressos Fred Perret/Guilherme Goulart. P.1–2: três heróis, missões PvE, limite coletivo de mortes por dificuldade; turno da guilda ativa todos ou descansa, seguido de duas ativações de monstros. P.4–6: ajustes por expansão, pets, componentes retirados e melhorias. P.5: títulos não são utilizados neste modo. P.7–16: mudanças por cenário e FAQ. O PDF avulso `...solo_mode_ofic_153046.pdf` é v1.1, não a mesma versão.
- **Cooperativo/solo de Raphael Gurian:** `...modo_cooperativo_e_solo_136121.pdf`, quatro páginas, variante declarada. Usa seis heróis para 1–3 jogadores, descanso individual e fase adicional dos monstros. Não confundir com Solo 1.2. A tabela de limite de rodadas deve ser examinada antes de adotar sua dificuldade.
- **Overlord:** `...regras_modo_co_op_5_jogad_108785.pdf`, três páginas, variante declarada de Vitor LeGoLaS. Um quinto jogador controla monstros; condições de vitória/derrota próprias, incluindo contagem de mortes. Não é autorização para tornar essa IA/jogador obrigatório.
- **Riders:** `...riders_rulebo_107662.pdf`, p.6–10 (especialmente p.8): montaria e cavaleiro se movem juntos; tamanho enorme, empurrão, efeitos e dano têm exceções próprias. A tradução `...traducao_da_expansao_riders_117679.pdf` é referência auxiliar, não nova versão automaticamente equivalente.
- **Pets:** livro de campanha `...livro_de_campanha_pets_ingl_62697.pdf` e abas Pets/Upgrades. Pets acrescentam utilidade e progressão própria; não equiparar todos a um atacante extra. O dragão companheiro atual é uma adaptação autoral simples, sem implementar toda expansão.
- **Dragões:** livros Havok, Icewyng, Mog'Dor e `...manuais_de_dragoes_a4_resum_244142.pdf`; abas Dragons/Dragon Turns. Encontros usam fases, movimentação de grande criatura, eventos e perigos específicos. Fogo, gelo, veneno e caos exigem contrajogo e sinalização diferentes.
- **Whole Lotta Lava:** manual `...whole_lotta_lava_manual_a4_244224.pdf`, três páginas, e cartas associadas: ambiente e chefes próprios, como Ignos, Hellcat e Anjo Caído. Fonte para cenários com perigo, não apenas mudança de cor do chão.

## Pendências de design

Definir futuramente qual modo é a campanha padrão e como seus módulos interagem. Títulos não podem ser aplicados cegamente ao Solo 1.2; descanso individual e ativação de todos não podem ser somados sem reequilibrar. Chefes, reações e ataques fora do turno devem compartilhar o orçamento do controlador para impedir contornar limites de dano, cura ou escudo.
