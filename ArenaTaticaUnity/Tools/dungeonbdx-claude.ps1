param([switch]$Check)
$ErrorActionPreference = 'Stop'
$dungeonRepo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$dungeonWorktree = Join-Path $dungeonRepo 'tmp/dungeonbdx-step1-git'
$dungeonCurrentState = Get-Content -LiteralPath (Join-Path $dungeonRepo 'coordenacao/dungeonbdx/estado.codex.json') -Raw | ConvertFrom-Json
if (($dungeonCurrentState.etapas | Where-Object { $_.id -eq 2 }).responsavel -ne 'Claude') {
    throw 'O autor atribuiu as etapas 2-3 ao Codex. Nao iniciar outro executor Claude neste worktree.'
}
$dungeonClaude = Get-Command claude -ErrorAction SilentlyContinue
if (-not $dungeonClaude) { throw 'Claude CLI ausente. Instale o CLI oficial antes de iniciar a etapa 2.' }
$dungeonClaudePath = $dungeonClaude.Source
$dungeonNativeCli = Join-Path (Split-Path $dungeonClaudePath -Parent) 'node_modules/@anthropic-ai/claude-code/bin/claude.exe'
if (Test-Path -LiteralPath $dungeonNativeCli) { $dungeonClaudePath = $dungeonNativeCli }
if (-not (Test-Path -LiteralPath (Join-Path $dungeonWorktree '.git'))) {
    throw 'Worktree da etapa 2 ausente. Solicite ao Codex preparar codex/dungeonbdx-step2.'
}
$dungeonBranch = & git -C $dungeonWorktree branch --show-current
if ($LASTEXITCODE -ne 0 -or $dungeonBranch -ne 'codex/dungeonbdx-step2') {
    throw 'O worktree deve estar na branch codex/dungeonbdx-step2. Nenhuma branch foi alterada.'
}
$dungeonTask = Join-Path $dungeonWorktree 'coordenacao/dungeonbdx/tarefas/02-claude-balanceamento.md'
if (-not (Test-Path -LiteralPath $dungeonTask)) { throw 'Tarefa 02 ausente no worktree.' }
$dungeonResponse = Join-Path $dungeonWorktree 'coordenacao/dungeonbdx/respostas/02-claude-balanceamento.resposta.md'
if (Test-Path -LiteralPath $dungeonResponse) {
    throw 'Ja existe uma resposta 02. Solicite revisao e testes ao Codex antes de iniciar outra etapa.'
}
function Get-DungeonClaudeAuth {
    $dungeonAuthText = (& $dungeonClaudePath auth status | Out-String)
    $dungeonAuth = $dungeonAuthText | ConvertFrom-Json
    return [bool]$dungeonAuth.loggedIn
}
$dungeonLoggedIn = Get-DungeonClaudeAuth
if ($Check) {
    [pscustomobject]@{ etapa = 2; branch = $dungeonBranch; claudeAutenticado = $dungeonLoggedIn; worktreePronto = $true } | ConvertTo-Json -Compress
    if (-not $dungeonLoggedIn) { exit 2 }
    exit 0
}
if (-not $dungeonLoggedIn) {
    Write-Host 'Autentique o Claude na janela/navegador oficial. Nao envie credenciais pela ponte.'
    & $dungeonClaudePath auth login
    if ($LASTEXITCODE -ne 0 -or -not (Get-DungeonClaudeAuth)) {
        throw 'Login do Claude nao concluido. A etapa 2 ainda nao foi iniciada.'
    }
}
$dungeonReference = Join-Path $dungeonRepo 'KrosArcano'
$dungeonPrompt = @"
Execute somente a etapa 02 do DungeonBDX, atribuida ao Claude pelo autor.
Voce esta no worktree isolado $dungeonWorktree, branch codex/dungeonbdx-step2.
Leia AGENTS.md, coordenacao/dungeonbdx/LEIA-ME.md, PIPELINE.md e tarefas/02-claude-balanceamento.md.
O diretorio de execucao desta mensagem substitui o caminho do checkout principal citado na tarefa.
O pipeline especifico do autor prevalece sobre a hierarquia generica do PROTOCOLO.md.
Voce nao esta sozinho no repositorio. Preserve alteracoes de outros executores.
Edite somente Assets/DungeonBDX/Combat e Tests/Combat sob ArenaTaticaUnity;
o contrato e resposta da etapa 02 sob coordenacao/dungeonbdx;
e uma nota propria de reserva .cs sob coordenacao/arena-pixelai/notas, antes de editar C#.
Nao altere combate Arena, Runtime, UI, ProjectSettings ou pacotes. Nao faca commit/push.
Referencias autorizadas, somente leitura local: $dungeonReference.
Nao envie PDFs/planilhas comerciais completos para ferramentas externas nem ao DeepSeek.
Use extracao local e descreva apenas conclusoes mecanicas relevantes, sem copiar textos/cartas.
Nucleo matematico sensivel fica com voce; DeepSeek pode investigar codigo via OpenRouter, sem documentos.
Nao inicie Unity neste worktree nem crie Library/cache: Codex roda os testes com o cache principal.
Implemente testes deterministas e reporte com precisao que aguardam execucao do Codex.
Entregue resposta atomica .tmp para .md. Nao modifique tarefas/estado compartilhados.
Pare apos a entrega 02, sem iniciar a etapa 03. Codex revisara diff, limites e testes antes de liberar 03.
"@
Push-Location -LiteralPath $dungeonWorktree
try {
    & $dungeonClaudePath --add-dir $dungeonReference -- $dungeonPrompt
    $dungeonExit = $LASTEXITCODE
} finally { Pop-Location }
exit $dungeonExit
