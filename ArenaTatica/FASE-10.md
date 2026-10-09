# Fase 10 — entrega aplicada

## Módulos

- `scripts/DebugInfoOverlay.gd`: FPS, papel Host/Client/Local, RTT de aplicação, percentual de sondas expiradas e cliques filtrados. Atualização do texto a cada 250 ms; sondas a cada segundo, timeout de 3 segundos, janela máxima de 60 resultados. WebSocket/TCP não permite medir perda real de datagramas; o percentual é de sondas sem resposta no prazo.
- `scripts/NetworkManager.gd`: debounce de 120 ms por ação e peer; proteção também no Host contra RPCs diretos. Fechamento desvincula o peer antes de fechá-lo, interrompe IA, limpa métricas/comandos e reinicia turnos. Falha de conexão usa fechamento diferido com geração de sessão para descartar callbacks antigos. Menu permite retomar localmente ou voltar aos controles de conexão.
- `scripts/PlayerMovement.gd` e `scripts/CombatManager.gd`: filtro antes de enviar pedido e antes de validar RPC recebido. Cliques descartados não são enfileirados. O formato compacto das ações foi preservado.
- `scripts/VisualFXManager.gd`: partículas são liberadas por `finished`, labels ao fim do tween; o contêiner sai quando perde o último filho. Nenhum nó VFX é criado no servidor headless.
- `scripts/AudioManager.gd`: até 12 vozes ativas; `finished` limpa a referência do stream e executa `queue_free()`. Encerramento libera sons ainda ativos. Os quatro recursos de áudio permanecem em cache enquanto o gerente existe, para evitar recarregamentos.
- `scripts/TurnManager.gd`: bloqueio de passar turno enquanto o menu de recuperação está ativo.

## Validação

Executar, na raiz de ArenaTatica:

```powershell
node tools/test_phase9.mjs --phase10
```

Resultado final: **1.037 verificações, zero falhas**, em regressões locais e pares Host/Client. Testes antigos de regras isolam o debounce; testes da Fase 10 exercitam o limite real de cliques e RPCs.

Teste gráfico `tests/test_phase10_visual.gd`: 120 efeitos/sons, limite de vozes, limpeza ao terminar e ao fechar, debounce de movimento e expiração de sondas passaram. Logs finais sem `leaked`, `still in use at exit`, erros de script ou conexão não tratada. Isso valida os cenários testados, não uma prova de ausência de todo vazamento possível.

Logs: `.artifacts/phase10/`. Captura do HUD: `.artifacts/phase10/debug.png`.

## Exportação

Rotina pronta, a executar na raiz de ArenaTatica:

```powershell
node tools/export.mjs pack
node tools/export.mjs windows
node tools/export.mjs web
node tools/export.mjs android
```

Somente `pack` foi executado nesta entrega: `build/phase10.pck`, um pacote de recursos, não um executável independente. A rotina interrompe com falha se o Godot registrar erros; logs em `.artifacts/phase10/export-ALVO.log`.

1. Instalar templates **Godot 4.6.1**. No painel **Projeto → Exportar**, usar os presets já salvos, versão `0.10.0` (Android code `10`).
2. **Web:** Compatibility, sem threads; exportar para `build/web/index.html`. Servir todo o diretório por HTTPS; `.wasm` com MIME `application/wasm`. Se habilitar threads: COOP `same-origin` e COEP `require-corp`.
3. **Android:** configurar SDK/JDK e assinatura release; `Internet` já habilitado, ARM64. Exportar APK para teste em aparelho; para Play, configurar Gradle/AAB e a assinatura apropriada.
4. **Protocolo final:** `wss://` com certificado válido e proxy WebSocket Upgrade até o Host nativo (TCP 9080). Release recusa WS; páginas HTTPS também recusam WS em debug. Validar Join/RTT, queda e reconexão no navegador e aparelho.

Templates, SDK/assinatura e domínio/certificado de produção continuam sendo dependências para as builds de plataforma. Não foram geradas ou publicadas builds finais Web/Android nesta etapa de validação.
