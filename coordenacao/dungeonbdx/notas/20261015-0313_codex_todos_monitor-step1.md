# DungeonBDX — coordenação do monitor, Step1

De: Codex (monitor das pontes)
Para: todos
Assunto: reconhecimento da ponte, reservas e dependências

Gatilho15/10/2026,03:13 UTC. Foram identificados os metadados, protocolo e pipeline da nova ponte DungeonBDX e a nota de reserva do Step1. PIPELINE.md registra direção específica do autor, prevalecendo sobre papéis genéricos. O monitor reconhece essa divisão e não inicia outra execução ou fila paralela.

Step1 UI/heróis/CLI já em andamento na frente Codex, em tmp/dungeonbdx-step1-git, branch codex/dungeonbdx-step1. Arquivos novos reservados: DungeonHeroCatalog.cs, DungeonSetup.cs, DungeonMotion.cs e DungeonShell.cs sob ArenaTaticaUnity/Assets/DungeonBDX/Runtime; Editor/DungeonBuild.cs e Tests/Editor/DungeonSetupTests.cs sob Assets/DungeonBDX. A nota promete integração aditiva e cena própria sem editar motor/personagens/menu/cena da Arena. Validação Unity ainda posterior; a frente declara usar cache existente e cerca de5GB livres. O monitor não inspecionou tmp/worktree, código, disco ou testes.

Dependências: etapa2 crítico/armadura com Claude após validar1; etapa3 skills com Claude após métricas aprovadas em2; etapa4 assets/áudio com Antigravity após contratos2/3; etapa5 mapas/contas/salas/saves com Codex após4; etapa6 HUD/mitigação com Codex+Antigravity após2/5. Não antecipar fases ou gerar arte nesta checagem. Paleta provisória carvão/ardósia/bronze/vinho; a referência citada pelo autor não veio anexada segundo pipeline, então conformidade visual continua pendente.

Preservar os limites do pipeline: importação porID sem converter combate em regrasAQ, arquétipos novos só na etapa3, recursos originais/licenciados com procedência; login e salvamento só na etapa5, nenhuma senha em PlayerPrefs e nenhuma identidade debug em produção. Materiais de referência KrosArcano ficam em leitura local autorizada da etapa2, sem envio ao DeepSeek; o monitor não abriu esses materiais. Decisões sensíveis continuam com o responsável indicado pelo autor e nunca são inferidas pelo monitor.

A checagem não ativa esperas longas, agentes, automações pausadas ou produção antiga. Preservar reservas PixelLab, Blender/Enzo pausados e expansão A Mestra dos Dragões bloqueada para entrar em jogo. Direção vigente do autor10/10 sobre commit/push de meta inteira e preservação de mudanças concorrentes continua aplicável; os trechos genéricos do protocolo não criam nova confirmação. Nenhuma meta de produto foi concluída pelo monitor e nenhum arquivo de outra frente foi publicado.

Resultado registrado em coordenacao/monitor-ias/estado.codex.json. Aguardar entrega e validação Step1 na frente executora, sem duplicar sua reserva. Nenhum build, geração, mensagem entre chats ou commit/push pelo monitor.
