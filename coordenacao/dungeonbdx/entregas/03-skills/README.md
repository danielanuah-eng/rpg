# DungeonBDX — habilidades v1

Domínio C# sem UnityEngine, dependente apenas de DungeonBDX.Combat. A cena de prévia continua mostrando fichas/skills de origem. A integração de combate, mapas e HUD ocorre em 05–06.

18 skills originais: combate básico, pistoleiro/atirador, arcano versátil e vínculo dracônico. `DungeonHeroProfiles.AdaptArena` cria um perfil independente para cada um dos 28 IDs jogáveis; entrada explícita de atributos e snapshot dos IDs de skills de origem. A adaptação usa loadouts por arquétipo, não uma transcrição 1:1 dos efeitos/passivas/formas antigos. Três novos perfis: Lyra Faísca, Orion Prisma e Vaelis do Vínculo. Ainda requerem assets na etapa 04.

`DungeonSkillSession` recebe `HeroSpawn`s e `ISkillBoard`. O tabuleiro inteiro de protótipo é pequeno; o mapa procedural real implementará esse contrato em 05. A ordem fornecida de heróis define os turnos; summons não entram nessa fila. `EndTurn(actorId)` só aceita o ator ativo. A futura camada de servidor autentica a identidade e serializa comandos; esta API não é um endpoint de rede.

`Cast(actorId, skillId, targetIds, destination)` valida tudo antes de gastar PA ou RNG. Em área, `targetIds[0]` é o centro escolhido; apenas a distribuição do orçamento ordena os alvos por ID ordinal. Um único roll alimenta o cast inteiro. `SkillUseResult` devolve impactos, mitigação em BP, dano antes de absorção de escudo, deltas reais de Vida/escudo, origem/destino e IDs de animação/SFX. A visualização deve reagir ao resultado; eventos de animação não aplicam dano novamente.

Todos os efeitos passam pelo mesmo ActionBudget/TurnBudget do dono. Uma invocação de 12PV, armadura 5, por dono; comandos pagos usam PA, cooldown e RNG do herói. Morte do pet libera o slot; recolhimento/morte nunca devolve PA nem reseta cooldown. Morte do dono remove o pet. Escudo total até 12; vida até o máximo do perfil (limite de segurança 200). Cura/escudo em excesso gastam o orçamento e não geram créditos.

Cooldown C pula os próximos C turnos do dono. Status de duração 1 expiram no início do tick de dono atual+2, incluindo pets pelo relógio do dono; refresh não soma potência. Mira vale para um ataque, inclusive se errar: uma rerrolagem de falha por dado e +1500BP aditivos. Slow reduz o passo do avanço, sem impedir toda movimentação; teleporte ignora esse slow. Não há stun, geração de PA, cópia de skill nem cadeia de críticos.

Companions, Control, Teleport e FriendlyFire são módulos independentes da sessão. Os caps matemáticos são obrigatórios. Todos os loadouts têm Golpe firme como alternativa quando outros módulos forem desligados. Não confundir Companions com o flag de encontros de dragão da prévia; a campanha/mapa fará essa ligação explícita em 05.

Depois de compilar/testar no Unity, `pwsh -NoProfile -File Tools/export-dungeonbdx-contracts.ps1` exporta o contrato a partir das DLLs puras compiladas, com expectativas exatas e piores casos calculados pelo core. `RandomState` é apenas o estado do RNG; não constitui um save completo da sessão. Persistência, contas, convites e lobbies pertencem a 05.
