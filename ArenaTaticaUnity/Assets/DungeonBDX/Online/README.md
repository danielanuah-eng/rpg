# Contas e salas opcionais

`DungeonOnlineClient` usa os SDKs instalados Auth 3.8.0 e MPS 2.4.0. Nenhuma conexão inicia automaticamente. Login/cadastro requerem ação explícita e o provedor Username & Password habilitado no projeto Unity. O perfil local `dungeonbdx` separa o cache do perfil da Arena; uma conta conectada em outro perfil é preservada e o cliente recusa substituí-la.

O componente não guarda senhas em campos, arquivos, PlayerPrefs ou logs. Os parâmetros são encaminhados ao SDK apenas durante a operação solicitada. O cache de tokens de sessão pertence ao Unity Authentication SDK; `LogoutAsync` solicita sua remoção com `SignOut(true)` após tentar sair da sala. Mensagens públicas são fixas e sanitizadas, sem o texto bruto de exceções. A fila de operações é exclusiva. Toda operação de sala exige autorização real do SDK e o perfil correto, inclusive em Editor/Development.

Salas privadas comportam quatro participantes e permitem senha opcional distinta da senha da conta. O código mostrado é um código de entrada compartilhável. `IsPrivate` oculta a sala nas pesquisas; não é uma lista de convites. Não há transporte de combate configurado, replicação de ações, gravação cloud ou convites direcionados in-app. Esses recursos ainda exigem implementação e validação próprias. O estado local de combate continua local.

APIs públicas: `Task LoginAsync(string username, string password, bool register)`, `Task CreateRoomAsync(string roomPassword = null)`, `Task JoinRoomAsync(string code, string roomPassword = null)`, `Task LeaveRoomAsync()`, `Task LogoutAsync()`. A UI observa `Status`, `Busy`, `Authorized`, `HasRoom`, `RoomCode` e o evento `Changed`.

Os testes cobrem validação pura e o bloqueio de autorização; não criam contas ou salas, nem verificam a configuração ou disponibilidade cloud. Não existe login falso no modo criador: a sessão offline pode existir independentemente deste componente, mas não autoriza chamadas online.
