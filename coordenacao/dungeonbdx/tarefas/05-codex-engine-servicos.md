# 05. Cenários e serviços

> Direção posterior do autor: não utiliza Antigravity; Codex redistribui e continua a base funcional. Dependências abaixo referem-se ao briefing original. Estado atual e pendências: PIPELINE.md e entregas/04-06-local/.

- **De:** autor / UltraBridge
- **Para:** codex / agentes World, Services e Online
- **Status:** local-validado; multiplayer-cloud-pendente
- **Projeto / pasta de trabalho:** C:\Users\Anuah\OneDrive\Área de Trabalho\Rpg
- **Entrega:** módulo DungeonBDX validado
- **Dependência:** 04 revisada e integrada; 02/03 versionadas

Geração procedural determinística com tiles 04, validação de conectividade/objetivos/spawns e mapas detalhados respeitando orçamento PC/mobile. Iluminação dinâmica e texturas com níveis de qualidade.

Contas com senha por provedor apropriado, sessões autenticadas, salas privadas, autorização do host/servidor, convites in-app, recuperação e save contínuo versionado/idempotente. Definir serviço e contratos antes de implementar; nunca senha em PlayerPrefs. Bypass do criador permitido apenas Editor/Development offline, sem identidade falsa em servidor nem bypass em release. Validar colisões de save, reconexão, acesso não autorizado e separação de usuários. Não usar token de sala como credencial de conta.

Propriedade a reservar antes de editar: Assets/DungeonBDX/World/, Services/, Tests/World/, Tests/Services/. Não iniciar enquanto 04 pendente. Configuração real de Cloud depende do projeto/serviços disponíveis; registrar bloqueios reais, sem simular integração concluída.

Antes de builds mobile: adotar identificador de aplicativo próprio do DungeonBDX para não substituir Arena Tática; separar carregamento dos recursos herdados por jogo/plataforma. A prévia Windows 01 inclui os Resources da Arena e tem cerca de 1,60 GB; não tratar esse pacote como orçamento de distribuição mobile.
